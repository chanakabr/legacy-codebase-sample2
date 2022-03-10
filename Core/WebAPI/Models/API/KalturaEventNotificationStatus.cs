using System;
using System.Collections.Generic;
using System.Text;

namespace WebAPI.Models.API
{
    public enum KalturaEventNotificationStatus
    {
        SENT = 0,
        FAILED = 1,
        SUCCESS = 2,
        FAILED_TO_SEND = 3
    }
}
