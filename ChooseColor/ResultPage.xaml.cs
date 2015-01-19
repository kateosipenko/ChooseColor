using ChooseColor.Utils;
using ChooseColor.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ChooseColor
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ResultPage : Page
    {
        public ResultPage()
        {
            this.InitializeComponent();
            Locator.ResultStatic.InitializeViewModel();
            Loaded += ResultPage_Loaded;
        }

        async void ResultPage_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var item in Locator.ResultStatic.Answers)
            {
                Image image = new Image();
                image.Source = new BitmapImage(new Uri(item.UserAnswerPath, UriKind.Absolute));
                image.Opacity = 0;
                parent.Children.Add(image);
            }

            AnimationHelper.OpacityAnimation(parent.Children).Begin();
            AnimationHelper.OpacityAnimation(correctAnswer).Begin();
        }
    }
}
