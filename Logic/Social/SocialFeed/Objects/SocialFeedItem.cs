using ApiObjects.Response;
using System.Collections.Generic;

namespace Core.Social.SocialFeed
{
    public class SocialFeedItem : SocialFeedItemComment
    {
        public List<SocialFeedItemComment> Comments { get; set; }
        public string FeedItemLink { get; set; }
    }

    public class SocialFeedResponse
    {
        public List<SocialFeedItem> SocialFeeds { get; set; }
        public int TotalCount { get; set; }
        public Status Status { get; set; }
    }

    public enum SocialFeedOrderBy
    {
        CreateDateAsc,
        CreateDateDesc
    }
}