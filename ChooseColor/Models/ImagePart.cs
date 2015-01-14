using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace ChooseColor.Models
{

    public class ImagePart
    {
        public Image UnknownPart { get; set; }
        public Image KnownPart { get; set; }
        public string Key { get; set; }
    }
}
