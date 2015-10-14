using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    [Serializable]
    public class KalturaExternalChannelFilter : KalturaAssetInfoFilter
    {
        /// <summary>
        /// Device Type
        /// </summary>
        [DataMember(Name = "device_type")]
        [JsonProperty(PropertyName = "device_type")]
        [XmlElement("device_type", IsNullable = true)]
        public string DeviceType
        {
            get;
            set;
        }

        /// <summary>
        /// UTC Offset
        /// </summary>
        [DataMember(Name = "utc_offset")]
        [JsonProperty(PropertyName = "utc_offset")]
        [XmlElement("utc_offset", IsNullable = true)]
        public string UtcOffset
        {
            get;
            set;
        }
    }
}