using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Social.SocialFeed.SocialFeedJsonTemplates
{
    public class TweeterResp : ISocialFeed
    {
        public List<Status> statuses { get; set; }

        public class Status
        {
            public long id { get; set; }
            public DateTime created_at { get; set; }
            public string text { get; set; }
            public int favorite_count { get; set; }
            public int retweet_count { get; set; }
            public User user { get; set; }
        }

        public class User
        {
            public long id { get; set; }
            public string name { get; set; }
            public string profile_image_url { get; set; }

        }

        public List<SocialFeedItem> ToBaseSocialFeedObj()
        {
            List<SocialFeedItem> retVal = new List<SocialFeedItem>();
            foreach (var status in statuses)
            {
                retVal.Add(new SocialFeedItem()
                    {
                        Body = status.text,
                        CreatorName = status.user.name,
                        CreatorImageUrl = status.user.profile_image_url,
                        CreateDate = TVinciShared.DateUtils.DateTimeToUnixTimestamp(status.created_at),
                        PopularityCounter = status.favorite_count
                    });
            }

            return retVal;
        }
    }
}
