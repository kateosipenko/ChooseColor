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
        private AppBarButton selectedBrush = null;
        private AppBarButton setAnswerButton;
        private AppBarButton cancelAnswerButton;
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
            setAnswerButton = GetTemplateChild("ok") as AppBarButton;
            cancelAnswerButton = GetTemplateChild("cancel") as AppBarButton;

            setAnswerButton.Tapped += OnSetAnswerButtonTapped;
            cancelAnswerButton.Tapped += OnCancelAnswerButtonTapped;
            setAnswerButton.IsEnabled = false;
            cancelAnswerButton.IsEnabled = false;

            //TODO: remove fake
            GenerateFakeBrushes();

            SetupPalette();
            SetupPicture();
        }

        #region EVENTS

        private void OnCancelAnswerButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            ClearSelection();
        }

        private void OnSetAnswerButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            if (selectedBrush != null && selectedPart != null)
            {
                SetAnswer();
                ClearSelection();
            }
        }

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
                            UpdateButtonsState();
                            AnimationHelper.ScaleInAnimation(selectedPart.UnknownPart).Begin();
                        }

                        break;
                    }
                }
            }

            UpdateButtonsState();
        }

        private void OnButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            if (selectedBrush != null)
                AnimationHelper.ScaleOutAnimation(selectedBrush).Begin();

            selectedBrush = (AppBarButton)sender;
            AnimationHelper.ScaleInAnimation(selectedBrush).Begin();
            UpdateButtonsState();
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

        #region Answer

        private void SetAnswer()
        {
            if (selectedPart != null && selectedBrush != null)
            {
                selectedPart.KnownPart.Visibility = Windows.UI.Xaml.Visibility.Visible;
                answers.Add(selectedPart, selectedBrush.Tag as SolidColorBrush);
                selectedBrush.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                ClearSelection();
            }
        }

        private void CancelAnswer()
        {
            selectedBrush = null;
            selectedPart = null;
        }

        private void UpdateButtonsState()
        {
            if (selectedBrush != null && selectedPart != null)
            {
                setAnswerButton.IsEnabled = true;
                cancelAnswerButton.IsEnabled = true;
            }
            else if (selectedPart != null || selectedBrush != null)
            {
                setAnswerButton.IsEnabled = false;
                cancelAnswerButton.IsEnabled = true;
            }
            else
            {
                setAnswerButton.IsEnabled = false;
                cancelAnswerButton.IsEnabled = false;
            }
        }

        #endregion Answer

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

        private void ClearSelection()
        {
            if (selectedPart != null)
            {
                AnimationHelper.ScaleOutAnimation(selectedPart.UnknownPart).Begin();
                selectedPart = null;
            }

            if (selectedBrush != null)
            {
                AnimationHelper.ScaleOutAnimation(selectedBrush).Begin();
                selectedBrush = null;
            }

            UpdateButtonsState();
        }

        private void CreateColorButton(SolidColorBrush brush)
        {
            AppBarButton button = new AppBarButton();
            button.Height = 80;
            button.Width = 80;
            button.Background = brush;
            button.Padding = new Thickness(0);
            button.Margin = new Thickness(10, 0, 10, 0);
            button.BorderThickness = new Thickness(1);
            button.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
            button.Tag = brush;
            button.Style = Application.Current.Resources["RoundButtonStyle"] as Style;
            button.Tapped += OnButtonTapped;
            palette.Children.Add(button);
        }

        // TODO: remove fake
        private void GenerateFakeBrushes()
        {
            brushes.Add(new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 0, 255)));
            brushes.Add(new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 0, 0)));
            brushes.Add(new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 255, 0)));
            brushes.Add(new SolidColorBrush(Windows.UI.Color.FromArgb(255, 43, 63, 135)));
            brushes.Add(new SolidColorBrush(Windows.UI.Color.FromArgb(255, 135, 63, 43)));
            brushes.Add(new SolidColorBrush(Windows.UI.Color.FromArgb(255, 135, 43, 63)));
            brushes.Add(new SolidColorBrush(Windows.UI.Color.FromArgb(255, 10, 255, 40)));
            brushes.Add(new SolidColorBrush(Windows.UI.Color.FromArgb(255, 16, 125, 80)));
            brushes.Add(new SolidColorBrush(Windows.UI.Color.FromArgb(255, 75, 0, 24)));
            brushes.Add(new SolidColorBrush(Windows.UI.Color.FromArgb(255, 54, 130, 47)));
        }
    }
}
