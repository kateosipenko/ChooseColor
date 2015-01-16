using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using System.IO;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.Storage.Streams;
using Windows.Storage.Pickers;

namespace ChooseColor.Utils
{
    public static class ImageUtils
    {
        public static async Task<bool> IsTappedOnTransparent(Image image, Point point)
        {
            bool isTappedOnTransparent = false;
            RenderTargetBitmap target = new RenderTargetBitmap();
            await target.RenderAsync(image);
            var pixels = await target.GetPixelsAsync();
            var stream = pixels.AsStream();
            stream.Seek((long)(point.Y) * target.PixelWidth * 4 + (long)(point.X) * 4, SeekOrigin.Begin);
            byte b = (byte)stream.ReadByte();
            byte g = (byte)stream.ReadByte();
            byte r = (byte)stream.ReadByte();
            byte a = (byte)stream.ReadByte();
            isTappedOnTransparent = a == 0 && r == 0 && b == 0 && g == 0;
            return isTappedOnTransparent;
        }

        public static async Task<string> ChangeImageColor(Image image, SolidColorBrush color, string fileName)
        {
            RenderTargetBitmap target = new RenderTargetBitmap();
            await target.RenderAsync(image);
            var pixels = await target.GetPixelsAsync();
            byte[] resultBytes = pixels.ToArray();
            for (int i = 0; i < resultBytes.Length; i += 4)
            {
                if (resultBytes[i] == 0 && resultBytes[i + 1] == 0 && resultBytes[i + 2] == 0 && resultBytes[i + 3] == 0)
                    continue;

                resultBytes[i] = color.Color.B;
                resultBytes[i + 1] = color.Color.G;
                resultBytes[i + 2] = color.Color.R;
                resultBytes[i + 3] = color.Color.A;
            }

            var folder = ApplicationData.Current.LocalFolder;
            string resultFilePath = string.Empty;
            var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
            resultFilePath = file.Path;
            var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite);
            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, fileStream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, (uint)target.PixelWidth, (uint)target.PixelHeight, 96, 96, resultBytes);
            await encoder.FlushAsync();

            return resultFilePath;
        }

        public static async Task<Size> GetImagePixelSize(string imagePath)
        {
            BitmapImage image = new BitmapImage();
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(imagePath, UriKind.Absolute));
            var stream = await file.OpenStreamForReadAsync();
            await image.SetSourceAsync(stream.AsRandomAccessStream());
            return new Size(image.PixelWidth, image.PixelHeight);
        }

        public static bool IsPartImageFile(string fileName)
        {
            var extension = fileName.Substring(fileName.LastIndexOf("."));
            return extension == ".png";
        }
    }
}
