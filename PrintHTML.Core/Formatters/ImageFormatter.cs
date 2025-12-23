using System.IO;

namespace PrintHTML.Core.Formatters
{
    internal class ImageFormatter : AbstractLineFormatter
    {
        public ImageFormatter(string documentLine, int maxWidth) : base(documentLine, maxWidth) { }

        public override string GetFormattedLine()
        {
            string filePath = Line.Trim();

            if (!File.Exists(filePath))
                return "<b>[Image Not Found]</b><br/>";

            // İstenen boyutları oku
            int targetWidth = GetDimension("w", 0);  // 0 ise orjinal
            int targetHeight = GetDimension("h", 0); // 0 ise orjinal

            // Html string oluştur
            string imgTag = $"<img src='{filePath}'";

            // Eğer genişlik belirtilmişse ekle
            if (targetWidth > 0)
                imgTag += $" width='{targetWidth}'";

            // Eğer yükseklik belirtilmişse ekle
            if (targetHeight > 0)
                imgTag += $" height='{targetHeight}'";

            // Eğer hiçbiri belirtilmemişse varsayılan bir genişlik verelim (sayfayı taşmasın)
            if (targetWidth == 0 && targetHeight == 0)
                imgTag += " width='200'"; // Varsayılan

            return imgTag += " /><br/>";
        }
    }
}