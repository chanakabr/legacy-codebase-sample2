using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models
{
    public class Image
    {
        /// <summary>
        /// Ratio
        /// </summary>
        [DataMember(Name = "ratio")]
        [JsonProperty(PropertyName = "ratio")]
        public string Ratio { get; set; }

        /// <summary>
        /// Width
        /// </summary>
        [DataMember(Name = "width")]
        [JsonProperty(PropertyName = "width")]
        public int Width { get; set; }

        /// <summary>
        /// Height
        /// </summary>
        [DataMember(Name = "height")]
        [JsonProperty(PropertyName = "height")]
        public int Height { get; set; }

        /// <summary>
        /// URL
        /// </summary>
        [DataMember(Name = "url")]
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }
}