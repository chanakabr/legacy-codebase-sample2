using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Image details
    /// </summary>
    public class KalturaImage : KalturaOTTObject
    {
        /// <summary>
        /// Image aspect ratio
        /// </summary>
        [DataMember(Name = "ratio")]
        [JsonProperty(PropertyName = "ratio")]
        public string Ratio { get; set; }

        /// <summary>
        /// Image width
        /// </summary>
        [DataMember(Name = "width")]
        [JsonProperty(PropertyName = "width")]
        public int Width { get; set; }

        /// <summary>
        /// Image height
        /// </summary>
        [DataMember(Name = "height")]
        [JsonProperty(PropertyName = "height")]
        public int Height { get; set; }

        /// <summary>
        /// Image URL
        /// </summary>
        [DataMember(Name = "url")]
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }
}