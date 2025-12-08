using PrintHTML.Core.Formatters;
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
        private const double CONSOLAS_WIDTH_RATIO = 0.6; // Consolas karakter genişlik/yükseklik oranı

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

                var printer = PrinterInfo.GetPrinter(printerName);
                if (printer == null) throw new Exception($"No printer found: {printerName}");

                var ia = printer.GetPrintCapabilities().PageImageableArea;
                double printableWidth = ia?.ExtentWidth ?? 280;

                // Dinamik font boyutu hesaplama
                int fontSize = CalculateFontSize(printableWidth, charactersPerLine);

                var formattedHtml = FormatHtmlContentAsync(lines, fontSize);
                var xamlContent = ConvertHtmlToXaml(formattedHtml);
                var flowDocument = CreateFlowDocument(xamlContent);

                PrintDocument(flowDocument, printerName);
            }
            catch (Exception exception)
            {
                throw new Exception("Printing failed.", exception);
            }
        }

        public FlowDocument GeneratePreview(string previewContent, string printerName = null, int charactersPerLine = 42)
        {
            try
            {
                SetCharactersPerLine(charactersPerLine);
                var lines = previewContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                int fontSize;

                // Eğer yazıcı seçiliyse, aynı hesaplamayı kullan (tutarlılık için)
                if (!string.IsNullOrEmpty(printerName))
                {
                    var printer = PrinterInfo.GetPrinter(printerName);
                    if (printer != null)
                    {
                        var ia = printer.GetPrintCapabilities().PageImageableArea;
                        double printableWidth = ia?.ExtentWidth ?? 280;
                        fontSize = CalculateFontSize(printableWidth, charactersPerLine);
                    }
                    else
                    {
                        fontSize = GetDefaultPreviewFontSize(charactersPerLine);
                    }
                }
                else
                {
                    fontSize = GetDefaultPreviewFontSize(charactersPerLine);
                }

                var formattedHtml = FormatHtmlContentAsync(lines, fontSize);
                var xamlContent = ConvertHtmlToXaml(formattedHtml);
                var flowDocument = CreateFlowDocument(xamlContent);

                return flowDocument;
            }
            catch (Exception exception)
            {
                throw new Exception("Failed to create preview.", exception);
            }
        }

        private string FormatHtmlContentAsync(string[] content, int fontSize)
        {
            var htmlBuilder = new StringBuilder();

            // Dinamik font-size kullanımı
            htmlBuilder.AppendLine($@"<style type='text/css'>
        html {{ font-family: 'Consolas'; font-size: {fontSize}px; }}
        div {{ margin: 0; }}
    </style>");

            var text = new FormattedDocument(content, _charactersPerLine).GetFormattedText();
            htmlBuilder.Append(text);

            return htmlBuilder.ToString();
        }

        private int CalculateFontSize(double printableWidth, int charactersPerLine)
        {
            // Formül: printableWidth / (charactersPerLine * CONSOLAS_WIDTH_RATIO)
            // WPF'de 1 birim = 1/96 inch, Consolas monospace font
            double calculatedSize = printableWidth / (charactersPerLine * CONSOLAS_WIDTH_RATIO);

            int fontSize = (int)Math.Round(calculatedSize);

            // Sınırları uygula - daha geniş aralık
            return Clamp(fontSize, 6, 20);
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private int GetDefaultPreviewFontSize(int charactersPerLine)
        {
            // Yazıcı bilgisi yokken tahmini değerler
            // 80mm ≈ 227 WPF units, 58mm ≈ 165 WPF units
            double estimatedWidth = charactersPerLine >= 40 ? 227 : 165;
            return CalculateFontSize(estimatedWidth, charactersPerLine);
        }

        private string ConvertHtmlToXaml(string html)
        {
            return HtmlToXamlConverter.ConvertHtmlToXaml(html, false);
        }

        private FlowDocument CreateFlowDocument(string xamlContent)
        {
            return PrinterTools.XamlToFlowDocument(xamlContent);
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