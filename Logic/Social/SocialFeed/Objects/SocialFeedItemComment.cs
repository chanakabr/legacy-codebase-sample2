using ApiObjects.Social;
using System;

namespace Core.Social.SocialFeed
{
    public class SocialFeedItemComment
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string CreatorName { get; set; }
        public long CreateDate { get; set; }
        public string CreatorImageUrl { get; set; }
        public int PopularityCounter { get; set; }
        public eSocialPlatform SocialPlatform { get; set; }
    }
}
