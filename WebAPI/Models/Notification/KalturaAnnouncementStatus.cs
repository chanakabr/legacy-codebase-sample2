using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.Notifications
{
    public enum KalturaAnnouncementStatus
    {
        NotSent = 0,
        Sending = 1,
        Sent = 2,
        Aborted = 3
    }
}