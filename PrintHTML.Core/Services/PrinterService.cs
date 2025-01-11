using Figgle;
using PrintHTML.Core.Helpers;
using PrintHTML.Core.HtmlConverter;
using System;
using System.Linq;
using System.Printing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Xps;

namespace PrintHTML.Core.Services
{
    public class PrinterService
    {
        private int _charactersPerLine;
        public void SetCharactersPerLine(int charactersPerLine)
        {
            _charactersPerLine = charactersPerLine;
        }

        public void DoPrint(string content, string printerName, int charactersPerLine = 42)
        {
            try
            {
                SetCharactersPerLine(charactersPerLine);
                string[] lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                var asciiArtFlowDocument = ConvertASCIIArt(lines);

                var formattedHtml = FormatHtmlContentAsync(lines);
                var xamlContent = ConvertHtmlToXaml(formattedHtml);
                var flowDocument = CreateFlowDocument(xamlContent);

                var mergeFlowDocument = MergeFlowDocuments(asciiArtFlowDocument, flowDocument);

                PrintDocument(mergeFlowDocument, printerName);
            }
            catch (Exception exception)
            {
                throw new Exception("Printing failed.", exception);
            }
        }

        private string FormatHtmlContentAsync(string[] content)
        {
            var htmlBuilder = new StringBuilder();

            // Varsayılan stiller
            htmlBuilder.AppendLine(@"<style type='text/css'>
                html { font-family: 'Consolas'; font-size: 12px; }
                div { margin: 0; }
            </style>");

            // İçeriği formatla
            foreach (var line in content)
            {
                if (string.IsNullOrEmpty(line)) continue;

                var formattedLine = HtmlFormattedDocument.FormatLine(line, _charactersPerLine);
                htmlBuilder.AppendLine(formattedLine);
            }

            return htmlBuilder.ToString();
        }

        private string ConvertHtmlToXaml(string html)
        {
            return HtmlToXamlConverter.ConvertHtmlToXaml(html, false);
        }

        private FlowDocument CreateFlowDocument(string xamlContent)
        {
            return PrinterTools.XamlToFlowDocument(xamlContent);
        }

        private FlowDocument ConvertASCIIArt(string[] lines)
        {
            // FlowDocument oluştur
            var flowDocument = new FlowDocument();

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrEmpty(line)) continue;

                // Etiketleri tanımlamak için bir regex oluştur
                var regex = new Regex(@"<([aA][sS][cC][iI][iI])>\[(.*?)\]", RegexOptions.None);

                // Eşleşmeyi kontrol et
                var match = regex.Match(line);
                if (!match.Success) continue;

                // Etiket içeriği (örneğin "C", "T" gibi)
                var tag = match.Groups[1].Value;
                if (string.IsNullOrEmpty(tag)) continue;

                // Etiketlerden arındırılmış içerik (gerekliyse kullanılır)
                var content = match.Groups[2].Value;

                // ASCII sanatı oluştur
                var fontStyle = FiggleFonts.Standard;
                var asciiArt = fontStyle.Render(content);

                // FlowDocument'e paragraf ekle
                Paragraph paragraph = new Paragraph();
                paragraph.Inlines.Add(new Run(asciiArt));
                paragraph.FontFamily = new FontFamily("Consolas");
                paragraph.FontSize = 10;

                // Paragrafı FlowDocument'e ekle
                flowDocument.Blocks.Add(paragraph);

                // İşlenen satırı boşalt
                lines[i] = string.Empty;
            }

            return flowDocument;
        }

        public FlowDocument GeneratePreview(string previewContent, int charactersPerLine = 42)
        {
            try
            {
                SetCharactersPerLine(charactersPerLine);
                var lines = previewContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                var asciiArtFlowDocument = ConvertASCIIArt(lines);

                var formattedHtml = FormatHtmlContentAsync(lines);
                var xamlContent = ConvertHtmlToXaml(formattedHtml);
                var flowDocument = CreateFlowDocument(xamlContent);

                return MergeFlowDocuments(asciiArtFlowDocument, flowDocument);
            }
            catch (Exception exception)
            {
                throw new Exception("Failed to create preview.", exception);
            }
        }

        public FlowDocument MergeFlowDocuments(FlowDocument doc1, FlowDocument doc2)
        {
            FlowDocument mergedDoc = new FlowDocument();

            // Birinci FlowDocument'teki blokları listeye al
            var blocksDoc1 = doc1.Blocks.Cast<Block>().ToList();

            // İkinci FlowDocument'teki blokları listeye al
            var blocksDoc2 = doc2.Blocks.Cast<Block>().ToList();

            // Birinci FlowDocument'in bloklarını birleştirilen belgeye ekle
            foreach (var block in blocksDoc1)
            {
                mergedDoc.Blocks.Add(block);
            }

            // İkinci FlowDocument'in bloklarını birleştirilen belgeye ekle
            foreach (var block in blocksDoc2)
            {
                mergedDoc.Blocks.Add(block);
            }

            return mergedDoc;
        }

        private void PrintDocument(FlowDocument document, string printerName)
        {
            var printer = PrinterInfo.GetPrinter(printerName);
            if (printer == null)
                throw new Exception($"No printer found: {printerName}");

            // Mevcut yazdırma metodunuzu burada kullanın
            PrintFlowDocument(printer, document);
        }

        private void PrintFlowDocument(PrintQueue printer, FlowDocument document)
        {
            try
            {
                // get information about the dimensions of the seleted printer+media.
                XpsDocumentWriter docWriter = PrintQueue.CreateXpsDocumentWriter(printer);
                PageImageableArea ia = printer.GetPrintCapabilities().PageImageableArea;
                PrintTicket pt = printer.UserPrintTicket;

                if (docWriter != null && ia != null)
                {
                    DocumentPaginator paginator = ((IDocumentPaginatorSource)document).DocumentPaginator;

                    // Change the PageSize and PagePadding for the document to match the CanvasSize for the printer device.
                    paginator.PageSize = new Size((double)pt.PageMediaSize.Width, (double)pt.PageMediaSize.Height);
                    Thickness t = new Thickness(3);  // copy.PagePadding;
                    document.PagePadding = new Thickness(
                                     Math.Max(ia.OriginWidth, t.Left),
                                       Math.Max(ia.OriginHeight, t.Top),
                                       Math.Max((double)pt.PageMediaSize.Width - (ia.OriginWidth + ia.ExtentWidth), t.Right),
                                       Math.Max((double)pt.PageMediaSize.Height - (ia.OriginHeight + ia.ExtentHeight), t.Bottom));

                    document.ColumnWidth = double.PositiveInfinity;
                    document.FontFamily = new System.Windows.Media.FontFamily("Consolas");
                    //copy.PageWidth = 528; // allow the page to be the natural with of the output device

                    // Send content to the printer.
                    docWriter.Write(paginator);
                }
            }
            catch (Exception exception)
            {
                throw new Exception($"Error during printing process: {exception.Message}", exception);
            }
        }
    }
}