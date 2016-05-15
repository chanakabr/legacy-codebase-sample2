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
    public class KalturaInboxMessageFilter : KalturaOTTObject
    {
        /// <summary>
        /// List of inbox message types to search within.
        /// </summary>
        [DataMember(Name = "typeIn")]
        [JsonProperty(PropertyName = "typeIn")]
        [XmlArray(ElementName = "typeIn", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaInboxMessageType> TypeIn { get; set; }

        /// <summary>
        /// TODO:
        /// </summary>
        [DataMember(Name = "createdAtGreaterThanOrEqual")]
        [JsonProperty(PropertyName = "createdAtGreaterThanOrEqual")]
        [XmlElement(ElementName = "createdAtGreaterThanOrEqual", IsNullable = true)]
        public long? CreatedAtGreaterThanOrEqual { get; set; }

        /// <summary>
        /// TODO:
        /// </summary>
        [DataMember(Name = "createdAtLessThanOrEqual")]
        [JsonProperty(PropertyName = "createdAtLessThanOrEqual")]
        [XmlElement(ElementName = "createdAtLessThanOrEqual", IsNullable = true)]
        public long? CreatedAtLessThanOrEqual { get; set; }
    }
}