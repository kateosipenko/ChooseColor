using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChooseColor.ViewModels
{
    public class Locator
    {
        #region Main

        private static MainViewModel mainVM;

        public static MainViewModel MainStatic
        {
            get
            {
                if (mainVM == null)
                    mainVM = new MainViewModel();

                return mainVM;
            }
        }

        public MainViewModel Main
        {
            get { return MainStatic; }
        }

        #endregion Main

        #region Result

        private static ResultViewModel result;

        public static ResultViewModel ResultStatic
        {
            get
            {
                if (result == null)
                {
                    result = new ResultViewModel();
                }

                return result;
            }
        }

        public ResultViewModel Result
        {
            get { return ResultStatic; }
        }

        #endregion Result
    }
}
