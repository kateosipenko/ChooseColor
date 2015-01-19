using ChooseColor.Models;
using ChooseColor.Utils;
using ChooseColor.ViewModels.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace ChooseColor.ViewModels
{
    public class MainViewModel : ViewModel
    {
        #region Constants

        // TODO: update before build
        private const string PartsFolder = "time";
        private const string PicturesFolder = "ImageParts";
        private const string ColorsFile = "colors.xml";
        private const string OriginalFileName = "original.jpg";
        private const string KnownUriFormat = "ms-appx:///{0}/{1}/known{2}";
        private const string UnknownUriFormat = "ms-appx:///{0}/{1}/{2}";

        #endregion Constants

        #region ImageParts

        private ObservableCollection<ImagePart> imageParts = new ObservableCollection<ImagePart>();

        public ObservableCollection<ImagePart> ImageParts
        {
            get { return imageParts; }
            set
            {
                imageParts = value;
                RaisePropertyChanged();
            }
        }


        #endregion ImageParts

        #region OriginalSource

        private string originalSource = string.Empty;

        public string OriginalSource
        {
            get { return originalSource; }
            private set
            {
                originalSource = value;
                RaisePropertyChanged();
            }
        }

        #endregion OriginalSource

        #region PatterSource

        private string patternSource = string.Empty;

        public string PatternSource
        {
            get { return patternSource; }
            private set
            {
                patternSource = value;
                RaisePropertyChanged();
            }
        }

        #endregion PatterntSource

        public MainViewModel()
        {
            AnswersCompletedCommand = new RelayCommand<ObservableCollection<ImagePart>>(AnswersCompletedExecute);
        }

        public RelayCommand<ObservableCollection<ImagePart>> AnswersCompletedCommand { get; private set; }


        private void AnswersCompletedExecute(ObservableCollection<ImagePart> answers)
        {
            Locator.ResultStatic.SetAnswers(answers);
            NavigationProvider.Navigate(typeof(ResultPage));
        }

        public async void CreateImageParts()
        {
            if (imageParts == null || imageParts.Count == 0)
            {
                imageParts = new ObservableCollection<ImagePart>();
                OriginalSource = string.Format(UnknownUriFormat, PicturesFolder, PartsFolder, OriginalFileName);
                PatternSource = string.Format(UnknownUriFormat, PicturesFolder, PartsFolder, "1.png");
                StorageFolder folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                folder = await folder.GetFolderAsync(PicturesFolder);
                folder = await folder.GetFolderAsync(PartsFolder);
                if (folder != null)
                {
                    var files = await folder.GetFilesAsync();
                    foreach (var item in files)
                    {
                        if (item.Name.Contains("known") || item.Name.Contains("original") || !ImageUtils.IsPartImageFile(item.Name))
                            continue;

                        CreateImagePart(item);
                    }

                    ImageParts = imageParts;
                }

                SetupColors();
            }
            else
            {
                foreach (var item in imageParts)
                {
                    var name = item.KnownPart.Tag as string;
                    // existing controls are already childs of another control
                    Image unknown = new Image { Tag = name, Opacity = 0 };
                    unknown.Source = item.UnknownPart.Source;
                    unknown.SetValue(Canvas.ZIndexProperty, 0);

                    Image known = new Image { Tag = name, Visibility = Windows.UI.Xaml.Visibility.Collapsed };
                    known.Source = item.KnownPart.Source;
                    known.SetValue(Canvas.ZIndexProperty, 0);
                    known.Tag = name;

                    item.UnknownPart = unknown;
                    item.KnownPart = known;
                    item.UserAnswerPath = string.Empty;
                    item.UserAnswer = null;
                }
            }
        }

        private void CreateImagePart(StorageFile file)
        {
            string name = file.Name;

            Image unknown = new Image { Tag = name, Opacity = 0 };
            string imagePath = string.Format(UnknownUriFormat, PicturesFolder, PartsFolder, name);
            unknown.Source = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
            unknown.SetValue(Canvas.ZIndexProperty, 0);

            Image known = new Image { Tag = name, Visibility = Windows.UI.Xaml.Visibility.Collapsed };
            known.Source = new BitmapImage(new Uri(string.Format(KnownUriFormat, PicturesFolder, PartsFolder, name), UriKind.Absolute));
            known.SetValue(Canvas.ZIndexProperty, 0);
            known.Tag = name;

            ImagePart part = new ImagePart
            {
                Key = name,
                KnownPart = known,
                UnknownPart = unknown,
                UnknownImagePath = imagePath,
            };

            imageParts.Add(part);
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
                    for (int i = 0; i < imageParts.Count; i++)
                    {
                        if (values.Count > i)
                        {
                            var color = GetColorFromHex(values[i]);
                            imageParts[i].Color = color;
                        }
                    }
                }
            }
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
    }
}
