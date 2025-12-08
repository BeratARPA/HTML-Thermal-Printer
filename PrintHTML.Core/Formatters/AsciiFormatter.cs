using Figgle;
using System;
using System.Text;

namespace PrintHTML.Core.Formatters
{
    internal class AsciiFormatter : AbstractLineFormatter
    {
        public AsciiFormatter(string documentLine, int maxWidth)
            : base(documentLine, maxWidth)
        {
        }

        public override string GetFormattedLine()
        {
            // Figgle ile ASCII sanatına dönüştür
            var asciiArt = FiggleFonts.Standard.Render(Line);

            // ASCII sanatını HTML uyumlu hale getir
            var formattedAscii = ConvertAsciiToHtml(asciiArt);

            return $"<pre>{formattedAscii}</pre>";
        }

        private string ConvertAsciiToHtml(string asciiArt)
        {
            var htmlBuilder = new StringBuilder();

            // Her satırı işle
            foreach (var line in asciiArt.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                // Boşlukları &nbsp; ile değiştir
                var formattedLine = line.Replace(" ", "&nbsp;");

                // Satırı HTML'e ekle ve <br/> ile satır sonu ekle
                htmlBuilder.Append(formattedLine);
                htmlBuilder.Append("<br/>");
            }

            return $"{htmlBuilder.ToString()}";
        }
    }
}
