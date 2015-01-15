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
    public class MainViewModel : ViewModel
    {
        public MainViewModel()
        {
            AnswersCompletedCommand = new RelayCommand<List<ImagePart>>(AnswersCompletedExecute);
        }

        public RelayCommand<List<ImagePart>> AnswersCompletedCommand { get; private set; }


        private void AnswersCompletedExecute(List<ImagePart> answers)
        {
            Locator.ResultStatic.SetAnswers(answers);
            NavigationProvider.Navigate(typeof(ResultPage));
        }
    }
}
