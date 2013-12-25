using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.ServiceModel
{
    public class MediaMarkDTO
    {
        public int nGroupID { get; set; }

        public string sSiteGUID { get; set; }

        public int nMediaID { get; set; }

        public int nLocationSec { get; set; }

        public string sDeviceName { get; set; }

        public string sDeviceID { get; set; }

        public MediaMarkObjectStatusDTO eStatus { get; set; }
    }

    public enum MediaMarkObjectStatusDTO
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