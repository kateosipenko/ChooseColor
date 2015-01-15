using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChooseColor.ViewModels.Helpers
{
    public class RelayCommand : ICommand
    { 
        #region Fields

        private Action execute;
        private Func<bool> canExecute;

        #endregion Fields

        #region Constructors

        public RelayCommand(Action execute)
        {
            this.execute = execute;
        }

        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        #endregion Constructors

        public bool CanExecute(object parameter)
        {
            return this.canExecute != null ? this.canExecute.Invoke() : true;
        }       

        public void Execute(object parameter)
        {
            if (this.execute != null && this.CanExecute(parameter))
            {
                this.execute.Invoke();
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
