using PrintHTML.Core.Helpers;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using ZXing;
using ZXing.Common;

namespace PrintHTML.Core.Formatters
{
    internal class BarcodeFormatter : AbstractLineFormatter
    {
        public BarcodeFormatter(string documentLine, int maxWidth)
            : base(documentLine, maxWidth)
        {
        }

        public override string GetFormattedLine()
        {
            string barcodeContent = Line.Trim();

            if (string.IsNullOrWhiteSpace(barcodeContent))
                return "<br/>";

            var barcodeType = BarcodeFormat.CODE_128;
            if (Tag.Tag.Contains(":"))
            {
                string[] parts = Tag.Tag.Trim('<', '>').Split(new[] { ' ', ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1 && !parts[1].ToLower().StartsWith("w") && !parts[1].ToLower().StartsWith("h"))
                {
                    barcodeType = ParseBarcodeType(parts[1]);
                }
            }

            int width = GetDimension("w", 300);
            int height = GetDimension("h", 100);

            var encodingOptions = new EncodingOptions
            {
                Height = height,
                Width = width,
                Margin = 0,
                PureBarcode = false
            };
         
            var writer = new BarcodeWriterPixelData
            {
                Format = barcodeType,
                Options = encodingOptions
            };

            var pixelData = writer.Write(barcodeContent);
            using (var bitmap = new Bitmap(pixelData.Width, pixelData.Height, PixelFormat.Format32bppArgb))
            {
                using (var ms = new MemoryStream())
                {
                    var bitmapData = bitmap.LockBits(new Rectangle(0, 0, pixelData.Width, pixelData.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                    try
                    {
                        Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
                    }
                    finally
                    {
                        bitmap.UnlockBits(bitmapData);
                    }

                    string filePath = ImageHelper.SaveBitmapToTempFile(bitmap);
                    if (filePath == null) return "<b>Barcode Error</b><br/>";
                    return $"<img src='{filePath}' width='{width}' height='{height}'/><br/>";
                }
            }
        }

        private BarcodeFormat ParseBarcodeType(string typeStr)
        {
            switch (typeStr.ToUpper())
            {
                case "CODE128":
                case "CODE_128":
                    return BarcodeFormat.CODE_128;

                case "CODE93":
                case "CODE_93":
                    return BarcodeFormat.CODE_93;

                case "CODE39":
                case "CODE_39":
                    return BarcodeFormat.CODE_39;

                case "EAN8":
                case "EAN_8":
                    return BarcodeFormat.EAN_8;

                case "EAN13":
                case "EAN_13":
                    return BarcodeFormat.EAN_13;

                case "UPCA":
                case "UPC_A":
                    return BarcodeFormat.UPC_A;

                case "UPCE":
                case "UPC_E":
                    return BarcodeFormat.UPC_E;

                case "QR":
                case "QRCODE":
                case "QR_CODE":
                    return BarcodeFormat.QR_CODE;

                case "PDF417":
                    return BarcodeFormat.PDF_417;

                case "DATAMATRIX":
                case "DATA_MATRIX":
                    return BarcodeFormat.DATA_MATRIX;

                case "ITF":
                    return BarcodeFormat.ITF;

                case "CODABAR":
                    return BarcodeFormat.CODABAR;

                case "AZTEC":
                    return BarcodeFormat.AZTEC;

                default:
                    return BarcodeFormat.CODE_128;
            }
        }
    }
}