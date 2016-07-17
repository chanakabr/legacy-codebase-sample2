using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.ConditionalAccess
{
    [Serializable]
    public enum KalturaRecordingType
    {
        SINGLE = 0,
        SERIES = 1,
        SEASON = 2
    }
}