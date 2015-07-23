using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Container for translation
    /// </summary>
    public class KalturaTranslationContainer : KalturaOTTObject
    {
        /// <summary>
        /// Language code
        /// </summary>
        [DataMember(Name = "language")]
        [JsonProperty("language")]
        public string Language { get; set; }

        /// <summary>
        /// Translated value
        /// </summary>
        [DataMember(Name = "value")]
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}