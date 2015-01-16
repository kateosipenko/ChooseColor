using ChooseColor.Models;
using ChooseColor.ViewModels.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChooseColor.ViewModels
{
    public class ResultViewModel : ViewModel
    {
        #region Answers

        // TODO: replace
        private const string TextResultPattern = "Вы на {0}% Сальвадор Дали";

        private ObservableCollection<ImagePart> answers = new ObservableCollection<ImagePart>();

        public ObservableCollection<ImagePart> Answers
        {
            get { return answers; }
            private set
            {
                answers = value;
                RaisePropertyChanged();
            }
        }

        #endregion Answers

        #region TextResult

        private string textResult = string.Empty;

        public string TextResult
        {
            get { return textResult;  }
            private set
            {
                textResult = value;
                RaisePropertyChanged();
            }
        }

        #endregion TextResult

        #region GoToStartCommand

        public RelayCommand GoToStartCommand { get; private set; }

        #endregion GoToStartCommand

        public ResultViewModel()
        {
            GoToStartCommand = new RelayCommand(GoToStartExecute);
        }

        public void InitializeViewModel()
        {
            double correctAnsswersCount = answers.Where(item => item.UserAnswer.Color == item.Color.Color).Count();
            int percent =(int) ((correctAnsswersCount / answers.Count) * 100);
            TextResult = string.Format(TextResultPattern, percent);
        }

        private void GoToStartExecute()
        {
            NavigationProvider.Navigate(typeof(StartPage));
        }

        public void SetAnswers(List<ImagePart> answers)
        {
            this.answers = new ObservableCollection<ImagePart>(answers);
        }

        public void Clear()
        {
            this.TextResult = string.Empty;
            this.Answers.Clear();
        }
    }
}
