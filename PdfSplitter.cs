using System;
using System.IO;
using System.Linq;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;


// Console.WriteLine($"Estratto: {estratto}");
namespace PdfSplitterAndRenaimer
{
    public class PdfSplitter
    {
        static void Main()
        {
            try
            {
                string inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "input.pdf");
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string outputDir = Path.Combine(desktopPath, "PDF_Splittati");

                if (!Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                using (PdfDocument pdfDoc = new PdfDocument(new PdfReader(inputPath)))
                {
                    int totalPages = pdfDoc.GetNumberOfPages();

                    for (int i = 1; i <= totalPages; i++)
                    {
                        string tempFileName = $"pagina_{i}.pdf";
                        string outputPath = Path.Combine(outputDir, tempFileName);

                        // Splitta la pagina
                        using (PdfDocument newDoc = new PdfDocument(new PdfWriter(outputPath)))
                        {
                            pdfDoc.CopyPagesTo(i, i, newDoc);
                        }

                        // Ora apri il PDF appena creato ed estrai il testo
                        string estratto = EstraiTestoDaPagina(outputPath);

                        // Estrai il nome operatore e il mese/anno
                        var (nomeOperatore, meseAnno) = TrovaOperatoreEMeseAnno(estratto);
                        
                        if (!string.IsNullOrWhiteSpace(nomeOperatore) && !string.IsNullOrWhiteSpace(meseAnno))
                        {

                            
                            // Pulisci il nome
                            string[] nomeParti = nomeOperatore.Split(' ');
                            string cognome = nomeParti.Length > 0 ? nomeParti[0] : "";
                            
                            string nome = string.Join(" ", nomeParti.Skip(1));

                            
                            nome = nome.Replace(" ", "_");
                            Console.WriteLine($"Nome: {nome}");
                            string nuovoNome = Path.Combine(outputDir, $"{meseAnno}_Timesheet_{cognome}_{nome}.pdf");

                            // Evita errori se il file esiste già
                            if (!File.Exists(nuovoNome))
                            {
                                File.Move(outputPath, nuovoNome);
                                Console.WriteLine($"Pagina {i} -> {nomeOperatore} ({meseAnno})");
                            }
                            else
                            {
                                Console.WriteLine($"Pagina {i} -> file già esistente. Saltata.");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Pagina {i} -> nome operatore o mese/anno non trovato.");
                        }
                    }

                    Console.WriteLine($"Splittate {totalPages} pagine in {outputDir}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore: {ex.Message}");
            }

            Console.ReadKey();
        }

        static string EstraiTestoDaPagina(string filePath)
        {
            using (var pdfDoc = new PdfDocument(new PdfReader(filePath)))
            {
                var strategy = new SimpleTextExtractionStrategy();
                return PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(1), strategy);
            }
        }

        

        static (string? nomeOperatore, string? meseAnno) TrovaOperatoreEMeseAnno(string testo)
        {
            string? nomeOperatore = null;
            string? meseAnno = null;

            foreach (var line in testo.Split('\n'))
            {

                if (line.ToLower().Contains("operatore"))
                {
                    Console.WriteLine($"{line}\n");
                    string[] parole = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    if (parole.Length >= 8)
                    {
                        string mese = parole.ElementAtOrDefault(4);
                        string anno = parole.ElementAtOrDefault(5);

                        if (!string.IsNullOrWhiteSpace(mese) && !string.IsNullOrWhiteSpace(anno) && int.TryParse(anno, out _))
                        {
                            string meseNumerico = GetMeseNumerico(mese);
                            meseAnno = $"{anno}{meseNumerico}";
                        }

                        string cognome = parole[6];
                        Console.WriteLine($"Cognome: {cognome}");
                        var nomeParti = parole.Skip(7).ToList();
                        
                        // Rimuovi il codice fiscale se presente (16 caratteri)
                        if (nomeParti.Count > 1 && nomeParti.Last().Length == 16)
                            nomeParti.RemoveAt(nomeParti.Count - 1);
                        Console.WriteLine($"Nome parti: {string.Join(" ", nomeParti)}");

                        // Non rimuovere nessun elemento da nomeParti; usa il contenuto completo
                        string nome = string.Join(" ", nomeParti);
                        Console.WriteLine($"Nome: {nome}");
                        // Unisci cognome e nome
                        nomeOperatore = $"{cognome} {nome}";
                        Console.WriteLine($"Nome operatore: {nomeOperatore}");
                    }
                }
            }

            return (nomeOperatore?.Trim(), meseAnno?.Trim());
        }
        
    static string GetMeseNumerico(string mese)
        {
            switch (mese.ToUpper())
            {
                case "GENNAIO": return "01";
                case "FEBBRAIO": return "02";
                case "MARZO": return "03";
                case "APRILE": return "04";
                case "MAGGIO": return "05";
                case "GIUGNO": return "06";
                case "LUGLIO": return "07";
                case "AGOSTO": return "08";
                case "SETTEMBRE": return "09";
                case "OTTOBRE": return "10";
                case "NOVEMBRE": return "11";
                case "DICEMBRE": return "12";
                default: return "";
            }
        }
    }
}