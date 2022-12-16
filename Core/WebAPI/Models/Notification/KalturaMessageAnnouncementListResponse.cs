using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    /// <summary>
    /// List of message announcements from DB.
    /// </summary>
    [DataContract(Name = "KalturaAnnouncementListResponse", Namespace = "")]
    [XmlRoot("KalturaAnnouncementListResponse")]
    public partial class KalturaAnnouncementListResponse : KalturaListResponse
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

    /// <summary>
    /// order announcements
    /// </summary>
    public enum KalturaAnnouncementOrderBy
    {
        NONE
    }

    /// <summary>
    /// order announcements
    /// </summary>
    public partial class KalturaAnnouncementFilter : KalturaFilter<KalturaAnnouncementOrderBy>
    {
        /// <summary>
        /// A list of comma separated announcement ids.
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn", IsNullable = true)]
        public string IdIn { get; set; }

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
    public partial class KalturaMessageAnnouncementListResponse : KalturaListResponse
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