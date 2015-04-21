using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.Objects
{
    public class Enums
    {
        public enum PlatformType
        {
            Web,
            STB,
            iPad,
            ConnectedTV,
            Cellular,
            Unknown
        }
        
        public enum Client
        {
            Api,
            Billing,
            ConditionalAccess,
            Domains,
            Notification,
            Pricing,
            Social,
            Users,
            Catalog
        }

        public enum UserItemType
        {
            Rental = 0,
            Subscription = 1,
            Package = 2,
            Favorite = 3,
            All = 4
        }

        public enum OrderBy
        {
            None = 0,
            Added = 1,
            Views = 2,
            Rating = 3,
            ABC = 4,
            Meta = 5
        }

        public enum eOrderDirection
        {
            Asc,
            Desc
        }

        public enum ePeriod
        {
            All = 0,
            Day = 1,
            Week = 7,
            Month = 30
        }

        public enum ActionType
        {
            Rate,
            Vote,
            Recommend,
            Share,
            AddFavorite,
            RemoveFavorite,
            Comment,
            Record,
            Reminder,
            Watch
        }
    }
}