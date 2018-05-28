using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    [Serializable]
    public class KalturaPersonalList : KalturaOTTObject
    {
        /// <summary>
        /// Id
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name")]
        [SchemeProperty(MinLength = 1)]
        public string Name { get; set; }

        /// <summary>
        /// Create Date
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty(PropertyName = "createDate")]
        [XmlElement(ElementName = "createDate")]
        [SchemeProperty(ReadOnly = true)]
        public long CreateDate { get; set; }

        /// <summary>
        /// Ksql
        /// </summary>
        [DataMember(Name = "ksql")]
        [JsonProperty(PropertyName = "ksql")]
        [XmlElement(ElementName = "ksql")]
        [SchemeProperty(MinLength = 1)]
        public string Ksql { get; set; }
    }

    /// <summary>
    /// List of KalturaPersonalList.
    /// </summary>
    [DataContract(Name = "KalturaPersonalListListResponse", Namespace = "")]
    [XmlRoot("KalturaPersonalListListResponse")]
    public class KalturaPersonalListListResponse : KalturaListResponse
    {
        /// <summary>
        /// Follow data list
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaPersonalList> PersonalListList { get; set; }
    }

    public enum KalturaPersonalListOrderBy
    {
        START_DATE_DESC,
        START_DATE_ASC
    }

    public class KalturaPersonalListFilter : KalturaFilter<KalturaPersonalListOrderBy>
    {
        public override KalturaPersonalListOrderBy GetDefaultOrderByValue()
        {
            return KalturaPersonalListOrderBy.START_DATE_DESC;
        }
    }
}