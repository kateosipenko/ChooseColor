using MeGoGo.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChooseColor.ViewModels
{
    public abstract class ViewModel : INotifyPropertyChanged
    {
        private bool isNetworkAvaible;

        #region Fields

        private int busyCount = 0;
        private static NavigationProvider navigationProvider;

        #endregion Fields

        public ViewModel()
        {
            if (navigationProvider == null)
            {
                navigationProvider = new NavigationProvider();
            }
        }

        #region Properties

        protected NavigationProvider NavigationProvider
        {
            get
            {
                return navigationProvider;
            }
        }

        #endregion Properties

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
