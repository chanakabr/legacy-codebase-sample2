using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.ConditionalAccess
{
    [Serializable]
    public enum KalturaRecordingStatus
    {
        scheduled = 0,
        recording = 1,
        recorded = 2,
        canceled = 3,
        failed = 4,
        does_not_exists = 5,
        deleted = 6
    }
}