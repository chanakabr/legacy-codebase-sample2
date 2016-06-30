using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.Notifications
{
    public enum KalturaAnnouncementRecipientsType
    {
        All = 0,
        LoggedIn = 1,
        Guests = 2,
        Other = 3
    }
}