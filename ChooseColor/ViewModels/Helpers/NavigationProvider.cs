using System;
using System.Linq;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MeGoGo.ViewModel.Helpers
{
    public class NavigationProvider
    {
        private static Frame rootFrame;

        private void CheckRootFrame()
        {
            if (rootFrame == null)
            {
                rootFrame = Window.Current.Content as Frame;
            }
        }

        public bool CanGoBack()
        {
            CheckRootFrame();
            return rootFrame.CanGoBack;
        }

        public void GoBack()
        {
            CheckRootFrame();
            rootFrame.GoBack();
        }

        public void Navigate(Type pageType)
        {
            CheckRootFrame();
            rootFrame.Navigate(pageType);
        }

        public void Navigate(Type pageType, object parameter)
        {
            CheckRootFrame();
            rootFrame.Navigate(pageType, parameter);
        }
    }
}
