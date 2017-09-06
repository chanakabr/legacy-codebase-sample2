using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public class KalturaAssetStruct : KalturaOTTObject
    {
        /// <summary>
        /// Asset Struct id 
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// Asset struct name for the partner
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public KalturaMultilingualString Name { get; set; }

        /// <summary>
        /// Asset Struct name for the partner
        /// </summary>
        [DataMember(Name = "systemName")]
        [JsonProperty("systemName")]
        [XmlElement(ElementName = "systemName")]
        public string SystemName { get; set; }

        /// <summary>
        ///  Is the Asset Struct predefined on the system
        /// </summary>
        [DataMember(Name = "isPredefined")]
        [JsonProperty("isPredefined")]
        [XmlElement(ElementName = "isPredefined")]
        public bool IsPredefined { get; set; }

        /// <summary>
        /// A list of comma separated meta ids associated with this asset struct, returned according to the order.
        /// </summary>
        [DataMember(Name = "metaIds")]
        [JsonProperty("metaIds")]
        [XmlElement(ElementName = "metaIds", IsNullable = true)]
        public string MetaIds { get; set; }

        /// <summary>
        /// Specifies when was the Asset Struct was created. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty("createDate")]
        [XmlElement(ElementName = "createDate")]
        [SchemeProperty(ReadOnly = true)]
        public long CreateDate { get; set; }

        /// <summary>
        /// Specifies when was the Asset Struct last updated. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty("updateDate")]
        [XmlElement(ElementName = "updateDate")]
        [SchemeProperty(ReadOnly = true)]
        public long UpdateDate { get; set; }
    }
}