using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Domains
{
    /// <summary>
    /// Device details
    /// </summary>
    [OldStandard("brandId", "brand_id")]
    [OldStandard("activatedOn", "activated_on")]
    public class KalturaHouseholdDevice : KalturaOTTObject
    {
        /// <summary>
        /// Device UDID
        /// </summary>
        [DataMember(Name = "udid")]
        [JsonProperty("udid")]
        [XmlElement(ElementName = "udid")]
        public string Udid { get; set; }

        /// <summary>
        /// Device name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Device brand name
        /// </summary>
        [DataMember(Name = "brand")]
        [JsonProperty("brand")]
        [XmlElement(ElementName = "brand")]
        public string Brand { get; set; }

        /// <summary>
        /// Device brand identifier
        /// </summary>
        [DataMember(Name = "brandId")]
        [JsonProperty("brandId")]
        [XmlElement(ElementName = "brandId")]
        public int? BrandId { get; set; }

        /// <summary>
        /// Device activation date (epoch)
        /// </summary>
        [DataMember(Name = "activatedOn")]
        [JsonProperty("activatedOn")]
        [XmlElement(ElementName = "activatedOn")]
        public long? ActivatedOn { get; set; }

        /// <summary>
        /// Device state
        /// </summary>
        [DataMember(Name = "state")]
        [JsonProperty("state")]
        [XmlElement(ElementName = "state", IsNullable = true)]
        [Obsolete]
        public KalturaDeviceState State { get; set; }

        /// <summary>
        /// Device state
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status", IsNullable = true)]
        [SchemeProperty(InsertOnly = true)]
        public KalturaDeviceStatus Status { get; set; }

        internal int getBrandId()
        {
            return BrandId.HasValue ? (int)BrandId : 0;
        }
    }

    [Obsolete]
    public class KalturaDevice : KalturaHouseholdDevice
    {
    }
}