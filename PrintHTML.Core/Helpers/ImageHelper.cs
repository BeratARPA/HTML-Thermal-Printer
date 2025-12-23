using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace PrintHTML.Core.Helpers
{
    public static class ImageHelper
    {
        // Base64 metodunu tutabilirsiniz ama asıl işi bu yeni metot yapacak
        public static string SaveBitmapToTempFile(Bitmap bitmap)
        {
            try
            {
                string tempFolder = Path.GetTempPath();
                string fileName = $"{Guid.NewGuid()}.png";
                string fullPath = Path.Combine(tempFolder, fileName);

                bitmap.Save(fullPath, ImageFormat.Png);

                return fullPath;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
