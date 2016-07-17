using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    /// <summary>
    /// List of message follow data.
    /// </summary>
    [DataContract(Name = "KalturaPersonalFollowFeedResponse", Namespace = "")]
    [XmlRoot("KalturaPersonalFollowFeedResponse")]
    [Obsolete]
    public class KalturaPersonalFollowFeedResponse : KalturaListResponse
    {
        /// <summary>
        /// Follow data list
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaPersonalFollowFeed> PersonalFollowFeed { get; set; }
    }

    /// <summary>
    /// List of message follow data.
    /// </summary>
    [DataContract(Name = "KalturaPersonalFeedListResponse", Namespace = "")]
    [XmlRoot("KalturaPersonalFeedListResponse")]
    public class KalturaPersonalFeedListResponse : KalturaListResponse
    {
        /// <summary>
        /// Follow data list
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaPersonalFeed> PersonalFollowFeed { get; set; }
    }

    public enum KalturaPersonalFeedOrderBy
    {
        RELEVANCY_DESC,

        NAME_ASC,

        NAME_DESC,

        VIEWS_DESC,

        RATINGS_DESC,

        VOTES_DESC,

        START_DATE_DESC,

        START_DATE_ASC
    }

    public class KalturaPersonalFeedFilter : KalturaFilter<KalturaPersonalFeedOrderBy>
    {
        public override KalturaPersonalFeedOrderBy GetDefaultOrderByValue()
        {
            return KalturaPersonalFeedOrderBy.START_DATE_DESC;
        }
    }
}