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

namespace ChooseColor.Utils
{
    public static class ImageUtils
    {
        public static async Task<bool> IsTappedOnTransparent(Image image, Point point)
        {
            bool isTappedOnTransparent = false;
            //var imageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(imageSource, UriKind.Relative)));
            //var stream = await imageFile.OpenReadAsync();
            //var bitmap = new WriteableBitmap(0, 0);
            //await bitmap.SetSourceAsync(stream);
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

        public static async Task<Size> GetImagePixelSize(string imagePath)
        {
            BitmapImage image = new BitmapImage();
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(imagePath, UriKind.Absolute));
            var stream = await file.OpenStreamForReadAsync();
            await image.SetSourceAsync(stream.AsRandomAccessStream());
            return new Size(image.PixelWidth, image.PixelHeight);
        }
    }
}
