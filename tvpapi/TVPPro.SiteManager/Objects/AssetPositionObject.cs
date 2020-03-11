using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPPro.SiteManager.Objects
{
    public class AssetPositionObject
    {
        public int userID { get; set; }
        public eUserType userType { get; set; }
        public int Position { get; set; }
    }

    public enum eUserType
    {
        HOUSEHOLD = 0,
        PERSONAL = 1
    }
}
