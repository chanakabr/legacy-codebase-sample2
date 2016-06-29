using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.Notifications
{
    public enum KalturaAnnouncementStatus
    {
        NOTSENT = 0,
        SENDING = 1,
        SENT = 2,
        ABORTED = 3
    }
}