using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChooseColor.ViewModels.Helpers
{
    public class RelayCommand<T> : ICommand
    {
         #region Fields

        private Action<T> execute;
        private Func<T, bool> canExecute;

        #endregion Fields

        #region Constructors

        public RelayCommand(Action<T> execute)
        {
            this.execute = execute;
        }

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        #endregion Constructors

        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || parameter is T && canExecute.Invoke((T)parameter);
        }       

        public void Execute(object parameter)
        {
            if (this.execute != null && CanExecute(parameter))
            {
                this.execute.Invoke((T)parameter);
            }
        }

        public event EventHandler CanExecuteChanged;
        public void RaiseCanExecuteChanged()
        {
            if (this.CanExecuteChanged != null)
                this.CanExecuteChanged(this, new EventArgs());
        }
    }
}
