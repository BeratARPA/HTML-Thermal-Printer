using PrintHTML.Core.Helpers;
using PrintHTML.Core.HtmlConverter;
using System;
using System.Printing;
using System.Text;
using System.Windows;
using System.Windows.Documents;
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

                var formattedHtml = FormatHtmlContentAsync(lines);
                var xamlContent = ConvertHtmlToXaml(formattedHtml);
                var flowDocument = CreateFlowDocument(xamlContent);

                PrintDocument(flowDocument, printerName);
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
            </style>"
            );

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

        public FlowDocument GeneratePreview(string previewContent, int charactersPerLine = 42)
        {
            try
            {
                SetCharactersPerLine(charactersPerLine);
                var content = previewContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                var formattedHtml = FormatHtmlContentAsync(content);
                var xamlContent = ConvertHtmlToXaml(formattedHtml);
                return CreateFlowDocument(xamlContent);
            }
            catch (Exception exception)
            {
                throw new Exception("Failed to create preview.", exception);
            }
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