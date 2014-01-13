using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class MediaMarkObject
    {
        public int nGroupID { get; set; }

        public string sSiteGUID { get; set; }

        public int nMediaID { get; set; }

        public int nLocationSec { get; set; }

        public string sDeviceName { get; set; }

        public string sDeviceID { get; set; }

        public MediaMarkObjectStatus eStatus { get; set; }
    }

    public enum MediaMarkObjectStatus
    {
        /// <remarks/>
        OK,

        /// <remarks/>
        FAILED,

        /// <remarks/>
        NA,

        /// <remarks/>
        MISSING_USER_SITE_GUID,

        /// <remarks/>
        MISIING_MEDIA_ID,
    }
}

