using PrintHTML.Core.Helpers;
using QRCoder;
using System.Drawing;

namespace PrintHTML.Core.Formatters
{
    internal class QRCodeFormatter : AbstractLineFormatter
    {
        public QRCodeFormatter(string documentLine, int maxWidth)
            : base(documentLine, maxWidth)
        {
        }

        public override string GetFormattedLine()
        {
            string qrContent = Line.Trim();

            if (string.IsNullOrWhiteSpace(qrContent))
                return "<br/>";

            int size = GetDimension("w", 150);
            int h = GetDimension("h", 0);
            if (h > size) size = h;

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
                using (QRCode qrCode = new QRCode(qrCodeData))
                {
                    using (Bitmap rawBitmap = qrCode.GetGraphic(100))
                    {
                        using (Bitmap resizedBitmap = new Bitmap(rawBitmap, new Size(size, size)))
                        {
                            string filePath = ImageHelper.SaveBitmapToTempFile(resizedBitmap);
                            if (filePath == null) return "<b>QR Error</b><br/>";
                            return $"<img src='{filePath}'  width='{size}'/><br/>";
                        }
                    }
                }
            }
        }
    }
}
