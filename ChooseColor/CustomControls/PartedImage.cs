using ChooseColor.Models;
using ChooseColor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using Windows.Storage;
using Windows.UI;
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
        private const string ColorsFile = "colors.xml";
        private const string OriginalFileName = "original.jpg";
        private const string KnownUriFormat = "ms-appx:///{0}/{1}/known{2}";
        private const string UnknownUriFormat = "ms-appx:///{0}/{1}/{2}";

        #endregion Constants

        #region Fields

        private Canvas parent;
        private List<ImagePart> parts = new List<ImagePart>();
        private StackPanel palette;
        private List<SolidColorBrush> brushes = new List<SolidColorBrush>();
        private ImagePart selectedPart = null;
        private AppBarButton selectedBrush = null;
        private AppBarButton setAnswerButton;
        private AppBarButton cancelAnswerButton;
        private Dictionary<ImagePart, SolidColorBrush> answers = new Dictionary<ImagePart, SolidColorBrush>();
        private Image original;

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

        #region CompletedCommandProperty

        public static readonly DependencyProperty CompletedCommandProperty = DependencyProperty.Register(
            "CompletedCommand",
            typeof(ICommand),
            typeof(PartedImage),
            new PropertyMetadata(null));

        public ICommand CompletedCommand
        {
            get { return (ICommand)GetValue(CompletedCommandProperty); }
            set { SetValue(CompletedCommandProperty, value); }
        }

        #endregion CompletedCommandProperty

        public PartedImage()
        {
            this.DefaultStyleKey = typeof(PartedImage);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            parent = GetTemplateChild("parent") as Canvas;
            palette = GetTemplateChild("palette") as StackPanel;
            setAnswerButton = GetTemplateChild("ok") as AppBarButton;
            cancelAnswerButton = GetTemplateChild("cancel") as AppBarButton;
            original = GetTemplateChild("original") as Image;

            original.Source = new BitmapImage(new Uri(string.Format(UnknownUriFormat, PicturesFolder, PartsFolder, OriginalFileName), UriKind.Absolute));
            // for appearing animation
            original.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
            original.RenderTransform = new CompositeTransform { TranslateX = 800 };

            SetupAnswerButtons();
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
                                selectedPart.UnknownPart.SetValue(Canvas.ZIndexProperty, 0);
                            }

                            selectedPart = part;
                            UpdateButtonsState();
                            selectedPart.UnknownPart.SetValue(Canvas.ZIndexProperty, 1);
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

        #region Answer

        private void SetupAnswerButtons()
        {
            setAnswerButton.Tapped += OnSetAnswerButtonTapped;
            cancelAnswerButton.Tapped += OnCancelAnswerButtonTapped;
            setAnswerButton.IsEnabled = false;
            cancelAnswerButton.IsEnabled = false;

            // for palette animation
            setAnswerButton.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
            setAnswerButton.RenderTransform = new CompositeTransform() { TranslateY = 95 };

            cancelAnswerButton.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
            cancelAnswerButton.RenderTransform = new CompositeTransform() { TranslateY = 95 };
        }

        private void SetAnswer()
        {
            if (selectedPart != null && selectedBrush != null)
            {
                selectedPart.KnownPart.Visibility = Windows.UI.Xaml.Visibility.Visible;
                answers.Add(selectedPart, selectedBrush.Tag as SolidColorBrush);
                selectedBrush.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                selectedPart.UnknownPart.SetValue(Canvas.ZIndexProperty, 0);
                selectedPart.KnownPart.SetValue(Canvas.ZIndexProperty, 1);
                selectedPart.UserAnswer = selectedBrush.Tag as SolidColorBrush;
                AnimationHelper.ScaleOutAnimation(selectedPart.KnownPart).Begin();                
                ClearSelection();

                if (parts.Where(item => item.UserAnswer != null).Count() == parts.Count)
                {
                    CompleteGame();
                }
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

        #region SetupControl

        #region Picture

        private void CreateImagePart(StorageFile file, int imageHeight, int top, int left)
        {
            string name = file.Name;

            Image unknown = new Image { Tag = name, Opacity = 0, Height = imageHeight };
            string imagePath = string.Format(UnknownUriFormat, PicturesFolder, PartsFolder, name);
            unknown.Source = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
            unknown.SetValue(Canvas.TopProperty, top);
            unknown.SetValue(Canvas.LeftProperty, left);
            unknown.SetValue(Canvas.ZIndexProperty, 0);
            parent.Children.Add(unknown);

            Image known = new Image { Tag = name, Visibility = Windows.UI.Xaml.Visibility.Collapsed, Height = imageHeight };
            known.Source = new BitmapImage(new Uri(string.Format(KnownUriFormat, PicturesFolder, PartsFolder, name), UriKind.Absolute));
            known.SetValue(Canvas.TopProperty, top);
            known.SetValue(Canvas.LeftProperty, left);
            known.SetValue(Canvas.ZIndexProperty, 0);
            known.Tag = name;

            unknown.Tapped += OnImageTapped;
            known.Tapped += OnImageTapped;

            ImagePart part = new ImagePart
            {
                Key = name,
                KnownPart = known,
                UnknownPart = unknown,
                UnknownImagePath = imagePath,
            };

            parts.Add(part);
        }

        private void AddKnownParts()
        {
            foreach (var part in parts)
            {
                parent.Children.Add(part.KnownPart);
            }
        }

        private async void SetupPicture()
        {
            StorageFolder folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            folder = await folder.GetFolderAsync(PicturesFolder);
            folder = await folder.GetFolderAsync(PartsFolder);
            if (folder != null)
            {
                var files = await folder.GetFilesAsync();
                int imageHeight = (int)(parent.ActualHeight);
                int top = (int)((parent.ActualHeight - imageHeight) / 2);
                var imageSize = await ImageUtils.GetImagePixelSize(string.Format(UnknownUriFormat, PicturesFolder, PartsFolder, "1.png"));
                var width = (imageHeight * imageSize.Width) / imageSize.Height;
                int left = (int) ((this.ActualWidth - width) / 2);
                foreach (var item in files)
                {
                    if (item.Name.Contains("known") || item.Name.Contains("original") || !ImageUtils.IsPartImageFile(item.Name))
                        continue;

                    CreateImagePart(item, imageHeight, top, left);
                }


                AddKnownParts();
                SetupColors();
            }

        }

        #endregion Picture

        #region Palette

        private void SetupPalette()
        {
            foreach (var item in brushes)
            {
                CreateColorButton(item);
            }

            AnimateAppearance();
        }

        private async void SetupColors()
        {
            StorageFolder folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            folder = await folder.GetFolderAsync(PicturesFolder);
            folder = await folder.GetFolderAsync(PartsFolder);
            string fileContent = string.Empty;
            try
            {
                StorageFile colorsFile = await folder.GetFileAsync(ColorsFile);
                fileContent = await FileIO.ReadTextAsync(colorsFile);
            }
            catch (Exception)
            {
            }

            if (!string.IsNullOrEmpty(fileContent))
            {
                var xmlContent = XDocument.Parse(fileContent);
                if (xmlContent != null)
                {
                    var values = xmlContent.Root.Elements().Select(item => item.Value).ToList();
                    for (int i = 0; i < parts.Count; i++)
                    {
                        if (values.Count > i)
                        {
                            var color = GetColorFromHex(values[i]);
                            brushes.Add(color);
                            parts[i].Color = color;
                        }
                    }
                }

                Random rand = new Random();
                brushes = brushes.OrderBy(c => rand.Next()).ToList();
            }

            SetupPalette();
        }

        private SolidColorBrush GetColorFromHex(string hexColor)
        {
            hexColor = hexColor.Replace("#", "");
            if (hexColor.Length != 8)
                throw new InvalidOperationException();

            byte a = byte.Parse(hexColor.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte r = byte.Parse(hexColor.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hexColor.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hexColor.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            return new SolidColorBrush(Color.FromArgb(a, r, g, b));
        }

        #endregion Palette

        private void AnimateAppearance()
        {
            AnimationHelper.OpacityQueueAnimation(parts.Select(item => item.UnknownPart)).Begin();
            AnimationHelper.TranslateXAnimation(original, 800, 0).Begin();
            var paletteItems = new List<UIElement>();
            paletteItems.AddRange(palette.Children);
            paletteItems.Add(setAnswerButton);
            paletteItems.Add(cancelAnswerButton);
            AnimationHelper.PaletteAnimation(paletteItems, 95, 0).Begin();
        }

        #endregion SetupControl

        private void ClearSelection()
        {
            if (selectedPart != null)
            {
                selectedPart.UnknownPart.SetValue(Canvas.ZIndexProperty, 0);
                selectedPart.KnownPart.SetValue(Canvas.ZIndexProperty, 0);
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

            // for palette animation
            button.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
            button.RenderTransform = new CompositeTransform() { TranslateY = 95 };

            palette.Children.Add(button);
        }

        private void CompleteGame()
        {
            AnimationHelper.TranslateXAnimation(original, 0, 800).Begin();
            var paletteItems = new List<UIElement>();
            paletteItems.Add(setAnswerButton);
            paletteItems.Add(cancelAnswerButton);
            AnimationHelper.PaletteAnimation(paletteItems, 0, 95).Begin();
            var animation = AnimationHelper.ScaleInAnimation(parent, 1.5, 1);
            animation.Completed += (sender, args) =>
            {
                if (CompletedCommand != null && CompletedCommand.CanExecute(this.parts))
                {
                    CompletedCommand.Execute(this.parts);
                }
            };

            animation.Begin();
        }
    }
}
