using System.Collections.Generic;

namespace Core.Social.SocialFeed
{
    public interface ISocialFeed
    {
        List<SocialFeedItem> ToBaseSocialFeedObj();
    }
}
