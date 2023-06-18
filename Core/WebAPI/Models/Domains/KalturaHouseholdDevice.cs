using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TVinciShared;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using static WebAPI.Exceptions.BadRequestException;

namespace WebAPI.Models.Domains
{
    /// <summary>
    /// Device details
    /// </summary>
    [XmlInclude(typeof(KalturaDevice))]
    public partial class KalturaHouseholdDevice : KalturaOTTObjectSupportNullable
    {
        /// <summary>
        /// Household identifier
        /// </summary>
        [DataMember(Name = "householdId")]
        [JsonProperty("householdId")]
        [XmlElement(ElementName = "householdId")]
        [SchemeProperty(RequiresPermission = (int)RequestType.WRITE)]
        public int HouseholdId { get; set; }

        /// <summary>
        /// Device UDID
        /// </summary>
        [DataMember(Name = "udid")]
        [JsonProperty("udid")]
        [XmlElement(ElementName = "udid")]
        [SchemeProperty(InsertOnly = true)]
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
        [Obsolete]
        public string Brand { get; set; }

        /// <summary>
        /// Device brand identifier
        /// </summary>
        [DataMember(Name = "brandId")]
        [JsonProperty("brandId")]
        [XmlElement(ElementName = "brandId")]
        [OldStandardProperty("brand_id")]
        public int? BrandId { get; set; }

        /// <summary>
        /// Device activation date (epoch)
        /// </summary>
        [DataMember(Name = "activatedOn")]
        [JsonProperty("activatedOn")]
        [XmlElement(ElementName = "activatedOn")]
        [OldStandardProperty("activated_on")]
        public long? ActivatedOn { get; set; }

        /// <summary>
        /// Device state
        /// </summary>
        [DataMember(Name = "state")]
        [JsonProperty("state")]
        [XmlElement(ElementName = "state", IsNullable = true)]
        [Obsolete]
        [SchemeProperty(ReadOnly = true)]
        public KalturaDeviceState? State { get; set; }

        /// <summary>
        /// Device state
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public KalturaDeviceStatus? Status { get; set; }

        /// <summary>
        /// Device family id
        /// </summary>
        [DataMember(Name = "deviceFamilyId")]
        [JsonProperty("deviceFamilyId")]
        [XmlElement(ElementName = "deviceFamilyId", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long? DeviceFamilyId { get; set; }

        /// <summary>
        /// Device DRM data
        /// </summary>
        [DataMember(Name = "drm")]
        [JsonProperty("drm")]
        [XmlElement(ElementName = "drm", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public KalturaCustomDrmPlaybackPluginData Drm { get; set; }

        /// <summary>
        /// external Id
        /// </summary>
        [DataMember(Name = "externalId")]
        [JsonProperty("externalId")]
        [XmlElement(ElementName = "externalId", IsNullable = true)]
        [SchemeProperty(RequiresPermission = (int)RequestType.WRITE, IsNullable = true, MaxLength = 255)]
        public string ExternalId { get; set; }

        /// <summary>
        /// mac address
        /// </summary>
        [DataMember(Name = "macAddress")]
        [JsonProperty("macAddress")]
        [XmlElement(ElementName = "macAddress", IsNullable = true)]
        [SchemeProperty(IsNullable = true, MaxLength = 255)]
        public string MacAddress { get; set; }

        /// <summary>
        /// Dynamic data
        /// </summary>
        [DataMember(Name = "dynamicData")]
        [JsonProperty("dynamicData")]
        [XmlElement(ElementName = "dynamicData", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public SerializableDictionary<string, KalturaStringValue> DynamicData { get; set; }
        
        /// <summary>
        /// model
        /// </summary>
        [DataMember(Name = "model")]
        [JsonProperty("model")]
        [XmlElement(ElementName = "model", IsNullable = true)]
        [SchemeProperty(IsNullable = true, MaxLength = 255)]
        public string Model { get; set; }

        /// <summary>
        /// manufacturer
        /// </summary>
        [DataMember(Name = "manufacturer")]
        [JsonProperty("manufacturer")]
        [XmlElement(ElementName = "manufacturer", IsNullable = true)]
        [SchemeProperty(IsNullable = true, MaxLength = 128)]
        public string Manufacturer { get; set; }

        /// <summary>
        /// manufacturer Id, read only
        /// </summary>
        [DataMember(Name = "manufacturerId")]
        [JsonProperty("manufacturerId")]
        [XmlElement(ElementName = "manufacturerId", IsNullable = true)]
        [SchemeProperty(IsNullable = true, ReadOnly = true)]
        public long? ManufacturerId { get; set; }

        /// <summary>
        /// Last Activity Time, read only
        /// </summary>
        [DataMember(Name = "lastActivityTime")]
        [JsonProperty("lastActivityTime")]
        [XmlElement(ElementName = "lastActivityTime", IsNullable = true)]
        [SchemeProperty(IsNullable = true, ReadOnly = true)]
        public long? LastActivityTime { get; set; }
    }
}