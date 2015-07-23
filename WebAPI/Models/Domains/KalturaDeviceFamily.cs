using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using WebAPI.Models.General;

namespace WebAPI.Models.Domains
{
    /// <summary>
    /// Device family details
    /// </summary>
    public class KalturaDeviceFamily : KalturaOTTObject
    {
        /// <summary>
        /// Device family identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        public long Id { get; set; }

        /// <summary>
        /// Device family name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Max number of devices allowed for this family
        /// </summary>
        [DataMember(Name = "device_limit")]
        [JsonProperty("device_limit")]
        public int DeviceLimit { get; set; }

        /// <summary>
        /// Max number of streams allowed for this family
        /// </summary>
        [DataMember(Name = "concurrent_limit")]
        [JsonProperty("concurrent_limit")]
        public int ConcurrentLimit { get; set; }

        /// <summary>
        /// List of all the devices in this family
        /// </summary>
        [DataMember(Name = "devices")]
        [JsonProperty("devices")]
        public List<KalturaDevice> Devices { get; set; }
    }
}