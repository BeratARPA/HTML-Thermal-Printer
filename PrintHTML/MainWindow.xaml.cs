using PrintHTML.Core.Helpers;
using PrintHTML.Core.Services;
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
        private int _charactersPerLine;
        private readonly PrinterService _printerService = new PrinterService();

        public MainWindow()
        {
            InitializeComponent();

            LoadPrinters();
        }

        private void LoadPrinters()
        {
            var printers = PrinterInfo.GetPrinterNames();
            ComboBoxPrinters.ItemsSource = printers;
            if (printers.Any())
                ComboBoxPrinters.SelectedIndex = 0;
        }

        private void ButtonPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_selectedPrinter))
                {
                    MessageBox.Show("Please select a printer.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(TextBoxContent.Text))
                {
                    MessageBox.Show("The content to be printed cannot be empty.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                ButtonPrint.IsEnabled = false;
                Mouse.OverrideCursor = Cursors.Wait;

                var content = TextBoxContent.Text;

                AsyncPrintTask.Exec(true, () => _printerService.DoPrint(content, _selectedPrinter, _charactersPerLine));

                MessageBox.Show("The printing process was completed successfully.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception exception)
            {
                MessageBox.Show($"An error occurred during printing: {exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ButtonPrint.IsEnabled = true;
                Mouse.OverrideCursor = null;
            }
        }

        private void ButtonPreview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var htmlContent = TextBoxContent.Text;

                if (string.IsNullOrWhiteSpace(htmlContent))
                {
                    MessageBox.Show("Content for preview cannot be empty.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                FlowDocumentScrollViewer.Document = _printerService.GeneratePreview(htmlContent, _charactersPerLine);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during preview: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ComboBoxPrinters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxPrinters.SelectedItem != null)
                _selectedPrinter = ComboBoxPrinters.SelectedItem.ToString();
        }

        private void TextBoxMaxWidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = TextBoxMaxWidth.Text;
            if (!string.IsNullOrEmpty(text))
                _charactersPerLine = Convert.ToInt16(TextBoxMaxWidth.Text);
            else
                _charactersPerLine = 42;

        }

        private void TextBoxMaxWidth_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Yalnızca rakamlara izin ver
            e.Handled = !e.Text.All(char.IsDigit);
        }
    }
}
