using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonWithSL
{
    public class UserData
    {
        public string UserName { get; set; }
        public string SiteGuid { get; set; }
        private string _parentalControl = "18";
        public string ParentalControl
        {
            get
            {
                return _parentalControl;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _parentalControl = value;
                }
            }
        }

    }
}
