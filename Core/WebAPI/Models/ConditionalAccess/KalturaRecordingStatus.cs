using System;

namespace WebAPI.Models.ConditionalAccess
{
    [Serializable]
    public enum KalturaRecordingStatus
    {
        SCHEDULED = 0,
        RECORDING = 1,
        RECORDED = 2,
        CANCELED = 3,
        FAILED = 4,
        DELETED = 5
    } 
}