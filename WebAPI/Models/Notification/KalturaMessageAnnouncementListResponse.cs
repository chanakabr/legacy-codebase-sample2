using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;
using WebAPI.Models.Notifications;

namespace WebAPI.Models.Notification
{
    /// <summary>
    /// List of message announcements from DB.
    /// </summary>
    [DataContract(Name = "KalturaAnnouncementListResponse", Namespace = "")]
    [XmlRoot("KalturaAnnouncementListResponse")]
    public class KalturaAnnouncementListResponse : KalturaListResponse
    {
        /// <summary>
        /// Announcements
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaAnnouncement> Announcements { get; set; }
    }

    public enum KalturaAnnouncementOrderBy
    {
        NONE
    }

    public class KalturaAnnouncementFilter : KalturaFilter<KalturaAnnouncementOrderBy>
    {
        public override KalturaAnnouncementOrderBy GetDefaultOrderByValue()
        {
            return KalturaAnnouncementOrderBy.NONE;
        }
    }

    /// <summary>
    /// List of message announcements from DB.
    /// </summary>
    [DataContract(Name = "KalturaAssetsPositionsResponse", Namespace = "")]
    [XmlRoot("KalturaMessageAnnouncementListResponse")]
    [Obsolete]
    public class KalturaMessageAnnouncementListResponse : KalturaListResponse
    {
        /// <summary>
        /// Announcements
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaAnnouncement> Announcements { get; set; }
    }
}