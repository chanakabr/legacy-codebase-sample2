using System;

namespace WebAPI.Models.ConditionalAccess
{
    [Serializable]
    public enum KalturaRecordingType
    {
        SINGLE = 0,
        SEASON = 1,
        SERIES = 2,
        OriginalBroadcast = 3
    }
}