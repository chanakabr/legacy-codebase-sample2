using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Social.SocialFeed.SocialFeedJsonTemplates
{
    public class FacebookResp : ISocialFeed
    {
        public List<Post> data { get; set; }
        public Paging paging { get; set; }

        public List<SocialFeedItem> ToBaseSocialFeedObj()
        {
            List<SocialFeedItem> retVal = new List<SocialFeedItem>();

            foreach (Post post in data.Where(p => p.message != null))
            {

                retVal.Add(new SocialFeedItem()
                {
                    Body = post.message,
                    CreatorName = post.from.name,
                    CreateDate = TVinciShared.DateUtils.DateTimeToUnixTimestamp(post.created_time),
                    CreatorImageUrl = string.Format(@"http://graph.facebook.com/{0}/picture?type=square", post.from.id),
                    FeedItemLink = post.link ?? post.picture,
                    PopularityCounter = post.likes != null ? post.likes.summary.total_count : 0,
                    Comments = post.comments != null ? post.comments.data.Select(comment => new SocialFeedItemComment()
                    {
                        Body = comment.message,
                        CreateDate = TVinciShared.DateUtils.DateTimeToUnixTimestamp(comment.created_time),
                        CreatorImageUrl = string.Format(@"http://graph.facebook.com/{0}/picture?type=square", comment.from.id),
                        CreatorName = comment.from.name,
                        PopularityCounter = comment.like_count

                    }).ToList() : null,
                });


            }

            return retVal;
        }

        public class Summary
        {
            public int total_count { get; set; }
        }

        public class Likes
        {
            public Summary summary { get; set; }
        }

        public class From
        {
            public string name { get; set; }
            public string id { get; set; }
            public string category { get; set; }
        }

        public class Comment
        {
            public int like_count { get; set; }
            public string message { get; set; }
            public From from { get; set; }
            public DateTime created_time { get; set; }
            public string id { get; set; }
        }

        public class Comments
        {
            public List<Comment> data { get; set; }
        }

        public class Post
        {
            public string message { get; set; }
            public string picture { get; set; }
            public string link { get; set; }
            public DateTime created_time { get; set; }
            public string id { get; set; }
            public Likes likes { get; set; }
            public Comments comments { get; set; }
            public From from { get; set; }
        }

        public class Paging
        {
            public string previous { get; set; }
            public string next { get; set; }
        }
    }
}