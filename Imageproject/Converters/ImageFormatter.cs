using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;

namespace Imageproject.Converters
{
    public class ImageFormatter
    {
        /// <summary>
        /// Bitmap轉BitmapImage
        /// </summary>
        public static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using MemoryStream stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Bmp);
            stream.Position = 0;

            BitmapImage result = new BitmapImage();
            result.BeginInit();
            result.CacheOption = BitmapCacheOption.OnLoad;
            result.StreamSource = stream;
            result.EndInit();
            result.Freeze();

            return result;
        }

        /// <summary>
        /// BitmapImage轉Bitmap
        /// </summary>
        public static Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
        {
            using MemoryStream outStream = new MemoryStream();
            BitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            encoder.Save(outStream);
            Bitmap bitmap = new Bitmap(outStream);

            return new Bitmap(bitmap);
        }

        /// <summary>
        /// Bitmap轉byte[]
        /// </summary>
        public static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            byte[] bytes = ms.GetBuffer();
            ms.Close();

            return bytes;
        }

        /// <summary>
        /// byte[]轉Bitmap
        /// </summary>
        public static Bitmap ByteArrayToBitbmp(byte[] bytes)
        {
            MemoryStream stream = new MemoryStream(bytes);
            return new Bitmap(stream);
        }

        /// <summary>
        /// byte[]轉BitmapImage
        /// </summary>
        public static BitmapImage ByteArrayToBitmapImage(byte[] array)
        {
            using var ms = new System.IO.MemoryStream(array);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = ms;
            image.EndInit();
            image.Freeze();

            return image;
        }

        /// <summary>
        /// BitmapImage存檔
        /// </summary>
        /// <param name="filePath">檔案名稱</param>
        public static void Save(BitmapImage image, string filePath)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            using var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create);
            encoder.Save(fileStream);
        }
    }
}
