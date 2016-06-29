using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.Notifications
{
    public enum KalturaAnnouncementRecipientsType
    {
        ALL = 0,
        LOGGEDIN = 1,
        GUESTS = 2,
        OTHER = 3
    }
}