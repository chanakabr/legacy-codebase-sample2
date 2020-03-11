using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPPro.SiteManager.Objects
{
    public class MediaMarkObject
    {
        public int nGroupID { get; set; }
        public string sSiteGUID { get; set; }
        public int nMediaID{ get; set; }
        public int nLocationSec { get; set; }
        public string sDeviceName { get; set; }
        public string sDeviceID { get; set; }
        public MediaMarkObjectStatus eStatus { get; set; }
    }


    public enum MediaMarkObjectStatus
    {
        OK,
        FAILED,
        NA,
        MISSING_USER_SITE_GUID,
        MISIING_MEDIA_ID
    }
}
