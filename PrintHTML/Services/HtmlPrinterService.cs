using PrintHTML.Helpers;
using PrintHTML.HtmlConverter;
using System;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Xps;

namespace PrintHTML.Services
{
    public class HtmlPrinterService
    {
        public async Task PrintAsync(string[] content, string printerName)
        {
            try
            {
                var formattedHtml = await FormatHtmlContentAsync(content);
                var xamlContent = ConvertHtmlToXaml(formattedHtml);
                var flowDocument = CreateFlowDocument(xamlContent);

                PrintDocument(flowDocument, printerName);
            }
            catch (Exception ex)
            {
                throw new Exception("Yazdırma işlemi başarısız", ex);
            }
        }

        public async Task<FlowDocument> GeneratePreview(string htmlContent)
        {
            try
            {
                var content = htmlContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var formattedHtml = await FormatHtmlContentAsync(content);
                var xamlContent = ConvertHtmlToXaml(formattedHtml);
                return CreateFlowDocument(xamlContent);
            }
            catch (Exception ex)
            {
                throw new Exception("Önizleme oluşturulamadı", ex);
            }
        }

        private string ConvertHtmlToXaml(string html)
        {
            return HtmlToXamlConverter.ConvertHtmlToXaml(html, false);
        }
      
        private async Task<string> FormatHtmlContentAsync(string[] content)
        {
            return await Task.Run(() =>
            {
                var htmlBuilder = new StringBuilder();

                // Varsayılan stiller
                htmlBuilder.AppendLine(@"<style type='text/css'>
                html { font-family: 'Consolas', monospace; font-size: 12px; }
                div { margin: 0; }
            </style>");

                // İçeriği formatla
                foreach (var line in content)
                {
                    if (string.IsNullOrEmpty(line)) continue;

                    var formattedLine = DocumentFormatter.FormatLine(line);
                    htmlBuilder.AppendLine(formattedLine);
                }

                return htmlBuilder.ToString();
            });
        }

        private FlowDocument CreateFlowDocument(string xamlContent)
        {
            return PrinterTools.XamlToFlowDocument(xamlContent);
        }

        private void PrintDocument(FlowDocument document, string printerName)
        {
            var printer = PrinterInfo.GetPrinter(printerName);
            if (printer == null)
                throw new Exception($"Yazıcı bulunamadı: {printerName}");

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
                    document.FontFamily = new System.Windows.Media.FontFamily("Segoe UI Semibold");
                    //copy.PageWidth = 528; // allow the page to be the natural with of the output device

                    // Send content to the printer.
                    docWriter.Write(paginator);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Yazdırma işlemi sırasında hata: {ex.Message}", ex);
            }
        }
    }
}