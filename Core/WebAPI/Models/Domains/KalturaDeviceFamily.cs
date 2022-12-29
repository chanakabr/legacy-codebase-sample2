using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Domains
{
    /// <summary>
    /// Device family details
    /// </summary>
    public partial class KalturaDeviceFamily : KalturaDeviceFamilyBase
    {
        /// <summary>
        /// List of all the devices in this family
        /// </summary>
        [DataMember(Name = "devices")]
        [JsonProperty("devices")]
        [XmlArray(ElementName = "devices", IsNullable = true)]
        [XmlArrayItem("item")]
        [Obsolete]
        public List<KalturaDevice> Devices { get; set; }
    }
}