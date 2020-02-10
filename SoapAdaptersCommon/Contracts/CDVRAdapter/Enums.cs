using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CDVRAdapter.Models
{
    public enum RecordingStatus
    {
        OK = 0,
        Failed = 1,
    }

    public enum RecordingProviderFailReason
    {
        Success = 0,
        internalProviderError = 1,
        BadRequest = 2,
        InsufficientStorage = 3,
        Unauthorized = 4,
        NoResponseFromProvider = 5
    }
}