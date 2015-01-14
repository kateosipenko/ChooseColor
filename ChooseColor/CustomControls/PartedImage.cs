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
        private StackPanel palette;
        private List<SolidColorBrush> brushes = new List<SolidColorBrush>();
        private ImagePart selectedPart = null;
        private Button selectedBrush = null;
        private Dictionary<ImagePart, SolidColorBrush> answers = new Dictionary<ImagePart, SolidColorBrush>();

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

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            parent = GetTemplateChild("mainGrid") as Grid;
            palette = GetTemplateChild("palette") as StackPanel;

            //TODO: remove fake
            GenerateFakeBrushes();

            SetupPalette();
            SetupPicture();            
        }

        #region EVENTS

        private async void OnImageTapped(object sender, TappedRoutedEventArgs e)
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
                        if (selectedPart != part)
                        {
                            if (selectedPart != null)
                            {
                                AnimationHelper.ScaleOutAnimation(selectedPart.UnknownPart).Begin();
                            }

                            selectedPart = part;
                            AnimationHelper.ScaleInAnimation(selectedPart.UnknownPart).Begin();
                        }

                        break;
                    }
                }
            }
        }

        private void OnButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            selectedBrush = (Button)sender;
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

            unknown.Tapped += OnImageTapped;
            known.Tapped += OnImageTapped;

            ImagePart part = new ImagePart
            {
                Key = name,
                KnownPart = known,
                UnknownPart = unknown
            };

            parts.Add(part);
        }

        private void SetAnswer()
        {
            if (selectedPart != null && selectedBrush != null)
            {
                selectedPart.KnownPart.Visibility = Windows.UI.Xaml.Visibility.Visible;
                answers.Add(selectedPart, selectedBrush.Tag as SolidColorBrush);
                selectedBrush.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                selectedPart = null;
                selectedBrush = null;
            }
        }

        private void CancelAnswer()
        {
            selectedBrush = null;
            selectedPart = null;
        }

        private async void SetupPicture()
        {
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

        private void SetupPalette()
        {
            foreach (var item in brushes)
            {
                CreateColorButton(item);
            }
        }

        private void CreateColorButton(SolidColorBrush brush)
        {
            Button button = new Button();
            button.Height = 80;
            button.Width = 80;
            button.Background = brush;
            button.Padding = new Thickness(0);
            button.Margin = new Thickness(10, 0, 10, 0);
            button.BorderThickness = new Thickness(1);
            button.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
            button.Tag = brush;
            button.Tapped += OnButtonTapped;
            palette.Children.Add(button);
        }

        // TODO: remove fake
        private void GenerateFakeBrushes()
        {
            brushes.Add(new SolidColorBrush(Windows.UI.Color.FromArgb(100,0,0,255)));
            brushes.Add(new SolidColorBrush(Windows.UI.Color.FromArgb(100,255,0,0)));
            brushes.Add(new SolidColorBrush(Windows.UI.Color.FromArgb(100, 0, 255, 0)));
        }
    }
}
