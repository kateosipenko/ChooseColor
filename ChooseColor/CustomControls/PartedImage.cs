using ChooseColor.Models;
using ChooseColor.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        #region Fields

        private Canvas parent;
        private bool isPictureSetuped;
        private bool isTemplateApplyed;
        private StackPanel palette;
        private List<SolidColorBrush> brushes = new List<SolidColorBrush>();
        private ImagePart selectedPart = null;
        private AppBarButton selectedBrush = null;
        private AppBarButton setAnswerButton;
        private AppBarButton cancelAnswerButton;
        private Dictionary<ImagePart, SolidColorBrush> answers = new Dictionary<ImagePart, SolidColorBrush>();
        private Image original;

        #endregion Fields

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

        #region ImagePartsProperty

        public static readonly DependencyProperty ImagePartsProperty = DependencyProperty.Register(
            "ImageParts",
            typeof(ObservableCollection<ImagePart>),
            typeof(PartedImage),
            new PropertyMetadata(null, OnImagePartsPropertyChanged));

        public ObservableCollection<ImagePart> ImageParts
        {
            get { return (ObservableCollection<ImagePart>)GetValue(ImagePartsProperty); }
            set { SetValue(ImagePartsProperty, value); }
        }

        private static void OnImagePartsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((PartedImage)sender).SetupPicture();
        }

        #endregion ImagePartsProperty

        #region OriginalSourceProperty

        public static readonly DependencyProperty OriginalSourceProperty = DependencyProperty.Register(
            "OriginalSource",
            typeof(string),
            typeof(PartedImage),
            new PropertyMetadata(null, OnOriginalSourcePropertyChanged));

        public string OriginalSource
        {
            get { return (string)GetValue(OriginalSourceProperty); }
            set { SetValue(OriginalSourceProperty, value); }
        }

        private static void OnOriginalSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((PartedImage)sender).SetupPicture();
        }

        #endregion OriginalSourceProperty

        #region PatternSourceProperty

        public static readonly DependencyProperty PatternSourceProperty = DependencyProperty.Register(
            "PatternSource",
            typeof(string),
            typeof(PartedImage),
            new PropertyMetadata(null, OnOriginalSourcePropertyChanged));

        public string PatternSource
        {
            get { return (string)GetValue(PatternSourceProperty); }
            set { SetValue(PatternSourceProperty, value); }
        }

        #endregion PatternSourceProperty

        public PartedImage()
        {
            this.DefaultStyleKey = typeof(PartedImage);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            parent = GetTemplateChild("parent") as Canvas;
            palette = GetTemplateChild("palette") as StackPanel;
            original = GetTemplateChild("original") as Image;
            isTemplateApplyed = true;
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
            }
        }

        private async void OnImageTapped(object sender, TappedRoutedEventArgs e)
        {
            foreach (var part in ImageParts)
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

        private void SetAnswer()
        {
            if (selectedPart != null && selectedBrush != null)
            {
                var brush = selectedBrush.Tag as SolidColorBrush;
                selectedPart.KnownPart.Visibility = Windows.UI.Xaml.Visibility.Visible;
                answers.Add(selectedPart, brush);
                selectedBrush.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                var part = this.ImageParts.SingleOrDefault(item => item.Key == selectedPart.Key);
                SaveAnswer(part, brush);
                selectedPart.UnknownPart.SetValue(Canvas.ZIndexProperty, 0);
                selectedPart.KnownPart.SetValue(Canvas.ZIndexProperty, 1);
                selectedPart.UserAnswer = brush;
                AnimationHelper.ScaleOutAnimation(selectedPart.KnownPart).Begin();
                ClearSelection();


                if (ImageParts.Where(item => item.UserAnswer != null).Count() == ImageParts.Count())
                {
                    CompleteGame();
                }
            }
        }

        private async void SaveAnswer(ImagePart part, SolidColorBrush brush)
        {
            part.UserAnswerPath = await ImageUtils.ChangeImageColor(selectedPart.UnknownPart, brush, selectedPart.Key);
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

        private async void SetupPicture()
        {
            if (ImageParts != null && ImageParts.Count() > 0 && !string.IsNullOrEmpty(OriginalSource)
                && !string.IsNullOrEmpty(PatternSource) && !isPictureSetuped && isTemplateApplyed)
            {
                original.Source = new BitmapImage(new Uri(OriginalSource, UriKind.Absolute));
                // for appearing animation
                original.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
                original.RenderTransform = new CompositeTransform { TranslateX = 800 };

                while (parent.ActualHeight == 0)
                    await Task.Delay(100);

                int imageHeight = (int)(parent.ActualHeight);
                int top = (int)((parent.ActualHeight - imageHeight) / 2);
                var imageSize = await ImageUtils.GetImagePixelSize(string.Format(PatternSource));
                var width = (imageHeight * imageSize.Width) / imageSize.Height;
                int left = (int)((this.ActualWidth - width) / 2);

                original.Width = left - original.Margin.Right - original.Margin.Left;

                foreach (var item in ImageParts)
                {
                    item.UnknownPart.Height = imageHeight;
                    item.KnownPart.Height = imageHeight;
                    item.UnknownPart.SetValue(Canvas.TopProperty, top);
                    item.KnownPart.SetValue(Canvas.TopProperty, top);
                    item.UnknownPart.SetValue(Canvas.LeftProperty, left);
                    item.KnownPart.SetValue(Canvas.LeftProperty, left);

                    item.UnknownPart.Tapped += OnImageTapped;
                    item.KnownPart.Tapped += OnImageTapped;

                    parent.Children.Add(item.UnknownPart);
                    parent.Children.Add(item.KnownPart);
                }

                isPictureSetuped = true;
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

            setAnswerButton = CreateColorButton(null);
            setAnswerButton.Icon = new SymbolIcon(Symbol.Accept);

            cancelAnswerButton = CreateColorButton(null);
            cancelAnswerButton.Icon = new SymbolIcon(Symbol.Cancel);

            setAnswerButton.Tapped -= OnButtonTapped;
            cancelAnswerButton.Tapped -= OnButtonTapped;
            setAnswerButton.Tapped += OnSetAnswerButtonTapped;
            cancelAnswerButton.Tapped += OnCancelAnswerButtonTapped;
            setAnswerButton.IsEnabled = false;
            cancelAnswerButton.IsEnabled = false;
            
            AnimateAppearance();
        }

        private void SetupColors()
        {
            foreach (var item in ImageParts)
            {
                brushes.Add(item.Color);
            }

            Random rand = new Random();
            brushes = brushes.OrderBy(c => rand.Next()).ToList();
            SetupPalette();
        }

        #endregion Palette

        private void AnimateAppearance()
        {            
            AnimationHelper.TranslateXAnimation(original, 800, 0).Begin();
            AnimationHelper.PaletteAnimation(palette.Children, 95, 0).Begin();
            AnimationHelper.OpacityAnimation(ImageParts.Select(item => item.UnknownPart)).Begin();
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

        private AppBarButton CreateColorButton(SolidColorBrush brush)
        {
            AppBarButton button = new AppBarButton();
            button.Width = this.ActualWidth / 16;
            button.Height = button.Width;
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
            return button;
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
                if (CompletedCommand != null && CompletedCommand.CanExecute(ImageParts))
                {
                    CompletedCommand.Execute(ImageParts);
                }
            };

            animation.Begin();
        }
    }
}
