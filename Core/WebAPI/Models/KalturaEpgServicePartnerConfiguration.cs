using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models
{
    public partial class KalturaEpgServicePartnerConfiguration : KalturaOTTObject
    {
        /// <summary>
        /// The number of slots (NOS) that are supported (1, 2, 3, 4, 6, 8, 12, 24)
        /// </summary>
        [DataMember(Name = "numberOfSlots")]
        [JsonProperty("numberOfSlots")]
        [XmlElement(ElementName = "numberOfSlots")]
        [SchemeProperty(IsNullable = true)]
        public int? NumberOfSlots { get; set; }

        /// <summary>
        /// The offset of the first slot from 00:00 UTC
        /// </summary>
        [DataMember(Name = "firstSlotOffset")]
        [JsonProperty("firstSlotOffset")]
        [XmlElement(ElementName = "firstSlotOffset")]
        [SchemeProperty(IsNullable = true)]
        public int? FirstSlotOffset { get; set; }
    }
}
