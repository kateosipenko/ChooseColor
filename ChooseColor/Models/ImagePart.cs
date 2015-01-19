using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace ChooseColor.Models
{

    public class ImagePart
    {
        public Image UnknownPart { get; set; }

        public string UnknownImagePath { get; set; }

        public Image KnownPart { get; set; }
        public string Key { get; set; }

        public SolidColorBrush Color { get; set; }

        public SolidColorBrush UserAnswer { get; set; }

        public string UserAnswerPath { get; set; }
    }
}
