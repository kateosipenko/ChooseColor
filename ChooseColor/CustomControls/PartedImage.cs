using ChooseColor.Models;
using ChooseColor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace ChooseColor.CustomControls
{
    public class PartedImage : Control
    {
        #region Constants

        private const string PicturesFolder = "ImageParts";
        private const string KnownUriFormat = "ms-appx:///{0}/{1}/known{2}";
        private const string UnknownUriFormat = "ms-appx:///{0}/{1}/{2}";

        #endregion Constants

        #region Fields

        private Grid parent;
        private List<ImagePart> parts = new List<ImagePart>();

        #endregion Fields

        #region PartsFolderProperty

        public static readonly DependencyProperty PartsFolderProperty = DependencyProperty.Register(
            "PartsFolder",
            typeof(string),
            typeof(PartedImage),
            new PropertyMetadata(null));

        public string PartsFolder
        {
            get { return (string)GetValue(PartsFolderProperty); }
            set { SetValue(PartsFolderProperty, value); }
        }

        #endregion PartsFolderProperty

        public PartedImage()
        {
            this.DefaultStyleKey = typeof(PartedImage);
        }

        #region EVENTS

        protected async override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            parent = GetTemplateChild("mainGrid") as Grid;
            StorageFolder folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            folder = await folder.GetFolderAsync(PicturesFolder);
            folder = await folder.GetFolderAsync(PartsFolder);
            if (folder != null)
            {
                var files = await folder.GetFilesAsync();
                foreach (var item in files)
                {
                    if (item.Name.Contains("known"))
                        continue;

                    CreateImagePart(item);
                }
            }
        }

        private async void OnTapped(object sender, TappedRoutedEventArgs e)
        {
            foreach (var part in parts)
            {
                if (part.KnownPart.Visibility != Windows.UI.Xaml.Visibility.Visible)
                {
                    bool isTransparentPoint = await ImageUtils.IsTappedOnTransparent(part.UnknownPart, e.GetPosition(part.UnknownPart));
                    if (isTransparentPoint)
                        continue;
                    else
                    {
                        e.Handled = true;
                        part.KnownPart.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    }
                }
            }
        }

        #endregion EVENTS

        private void CreateImagePart(StorageFile file)
        {
            string name = file.Name;

            Image unknown = new Image();
            unknown.Source = new BitmapImage(new Uri(string.Format(UnknownUriFormat, PicturesFolder, PartsFolder, name), UriKind.Absolute));
            unknown.Tag = name;
            parent.Children.Add(unknown);

            Image known = new Image();
            known.Source = new BitmapImage(new Uri(string.Format(KnownUriFormat, PicturesFolder, PartsFolder, name), UriKind.Absolute));
            known.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            known.Tag = name;
            parent.Children.Add(known);

            unknown.Tapped += OnTapped;
            known.Tapped += OnTapped;

            ImagePart part = new ImagePart
            {
                Key = name,
                KnownPart = known,
                UnknownPart = unknown
            };

            parts.Add(part);
        }




    }
}
