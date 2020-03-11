using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    [Serializable]
    public class MediaMarkObject
    {

        public enum MediaMarkObjectStatus
        {
            OK = 0,
            FAILED = 1,
            NA = 2,
            MISSING_USER_SITE_GUID = 3,
            MISIING_MEDIA_ID = 4
        }

        public Int32 nGroupID;
        public string sSiteGUID;
        public Int32 nMediaID;
        public Int32 nLocationSec;
        public string sDeviceName;
        public string sDeviceID;

        public MediaMarkObjectStatus eStatus;

        public MediaMarkObject()
        {
            nGroupID = 0;
            nMediaID = 0;
            nLocationSec = 0;

            sSiteGUID = string.Empty;
            sDeviceName = string.Empty;
            sDeviceID = string.Empty;

            eStatus = MediaMarkObjectStatus.OK;
        }

        public void Initialize(Int32 GroupID, Int32 MID, string SiteGuid)
        {
            nGroupID = GroupID;
            nMediaID = MID;
            sSiteGUID = SiteGuid;
        }

    }
}
