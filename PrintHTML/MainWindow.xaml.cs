using PrintHTML.Helpers;
using PrintHTML.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PrintHTML
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _selectedPrinter;
        private readonly HtmlPrinterService _printerService;

        public MainWindow()
        {
            InitializeComponent();
            LoadPrinters();
            _printerService = new HtmlPrinterService();
        }

        private void LoadPrinters()
        {
            var printers = PrinterInfo.GetPrinterNames();
            cbPrinters.ItemsSource = printers;
            if (printers.Any())
            {
                cbPrinters.SelectedIndex = 0;
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_selectedPrinter))
                {
                    MessageBox.Show("Lütfen bir yazıcı seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtHtmlContent.Text))
                {
                    MessageBox.Show("Yazdırılacak HTML içeriği boş olamaz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                btnPrint.IsEnabled = false;
                Mouse.OverrideCursor = Cursors.Wait;

                var content = txtHtmlContent.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                AsyncPrintTask.Exec(true, () => _printerService.PrintAsync(content, _selectedPrinter));

                MessageBox.Show("Yazdırma işlemi başarıyla tamamlandı.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Yazdırma işlemi sırasında bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnPrint.IsEnabled = true;
                Mouse.OverrideCursor = null;
            }
        }

        private async void btnPreview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var htmlContent = txtHtmlContent.Text;

                if (string.IsNullOrWhiteSpace(htmlContent))
                {
                    MessageBox.Show("Önizleme için HTML içeriği boş olamaz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                flowDocumentScrollViewer.Document = await _printerService.GeneratePreview(htmlContent);
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Önizleme sırasında bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cbPrinters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbPrinters.SelectedItem != null)
            {
                _selectedPrinter = cbPrinters.SelectedItem.ToString();
            }
        }
    }
}
