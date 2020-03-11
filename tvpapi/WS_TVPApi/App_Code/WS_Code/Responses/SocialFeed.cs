using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using TVPPro.SiteManager.DataLoaders;

/// <summary>
/// Summary description for SocialFeed
/// </summary>

[DataContract,  Newtonsoft.Json.JsonObject]
public class SocialFeed
{
    [DataMember]
    public SerializableDictionary<string, List<SocialFeedItem>> Feed { get; set; }
    [DataMember, Newtonsoft.Json.JsonProperty]
    public string Error { get; set; }

    public SocialFeed()
    {
        Feed = new SerializableDictionary<string, List<SocialFeedItem>>();
    }
}

[DataContract]
public class SocialFeedItem : SocialFeedItemComment
{
    [DataMember]
    public List<SocialFeedItemComment> Comments { get; set; }
    [DataMember]    
    public string FeedItemLink { get; set; }
}

[DataContract]
public class SocialFeedItemComment
{
    [DataMember]
    public string Title { get; set; }
    [DataMember]
    public string Body { get; set; }
    [DataMember]
    public string CreatorName { get; set; }
    [DataMember]
    public long CreateDate { get; set; }
    [DataMember]
    public string CreatorImageUrl { get; set; }
    [DataMember]
    public int PopularityCounter { get; set; }
}