using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    public partial class KalturaInboxMessageFilter : KalturaFilter<KalturaInboxMessageOrderBy>
    {
        /// <summary>
        /// List of inbox message types to search within.
        /// </summary>
        [DataMember(Name = "typeIn")]
        [JsonProperty(PropertyName = "typeIn")]
        [XmlElement(ElementName = "typeIn", IsNullable = true)]
        public string TypeIn { get; set; }

        /// <summary>
        /// createdAtGreaterThanOrEqual
        /// </summary>
        [DataMember(Name = "createdAtGreaterThanOrEqual")]
        [JsonProperty(PropertyName = "createdAtGreaterThanOrEqual")]
        [XmlElement(ElementName = "createdAtGreaterThanOrEqual", IsNullable = true)]
        public long? CreatedAtGreaterThanOrEqual { get; set; }

        /// <summary>
        /// createdAtLessThanOrEqual
        /// </summary>
        [DataMember(Name = "createdAtLessThanOrEqual")]
        [JsonProperty(PropertyName = "createdAtLessThanOrEqual")]
        [XmlElement(ElementName = "createdAtLessThanOrEqual", IsNullable = true)]
        public long? CreatedAtLessThanOrEqual { get; set; }
        public override KalturaInboxMessageOrderBy GetDefaultOrderByValue()
        {
            return KalturaInboxMessageOrderBy.NONE;
        }
    }
}