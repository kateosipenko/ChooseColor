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
        private Type currentPageType;
        private List<KeyValuePair<Type, object>> backStack = new List<KeyValuePair<Type,object>>();

        public Type CurrentPageType
        {
            get 
            {
                CheckRootFrame();
                return rootFrame.Content.GetType(); 
            }
        }

        private void CheckRootFrame()
        {
            if (rootFrame == null)
            {
                rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigated += rootFrame_Navigated;
            }
        }

        void rootFrame_Navigated(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            if (e.NavigationMode == Windows.UI.Xaml.Navigation.NavigationMode.Forward || e.NavigationMode == Windows.UI.Xaml.Navigation.NavigationMode.New)
            {
                this.backStack.Add(new KeyValuePair<Type, object>(e.SourcePageType, e.Parameter));
                currentPageType = e.SourcePageType;
            }
            else if (e.NavigationMode == Windows.UI.Xaml.Navigation.NavigationMode.Back)
            {
                var lastEntry = this.backStack.LastOrDefault(item => item.Key == currentPageType);
                if (lastEntry.Key != null && lastEntry.Value != null)
                {
                    this.backStack.Remove(lastEntry);
                }

                if (this.backStack.Count > 0)
                {
                    currentPageType = this.backStack.Last().Key;
                }
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

        public T GetNavigationParameter<T>(Type pageType)
        {
            T result = default(T);
            var lastEntry = this.backStack.LastOrDefault(item => item.Key == pageType);
            if (lastEntry.Key != null && lastEntry.Value != null)
            {
                result = (T)lastEntry.Value;
            }

            return result;
        }
    }
}
