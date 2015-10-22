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