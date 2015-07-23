using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// PIN and its origin of definition
    /// </summary>
    [Serializable]
    public class KalturaPinResponse : KalturaOTTObject
    {
        /// <summary>
        /// PIN code
        /// </summary>
        [DataMember(Name = "pin")]
        [JsonProperty(PropertyName = "pin")]
        public string PIN
        {
            get;
            set;
        }

        /// <summary>
        /// Where the PIN was defined at – account, household or user
        /// </summary>
        [DataMember(Name = "origin")]
        [JsonProperty(PropertyName = "origin")]
        public KalturaRuleLevel origin
        {
            get;
            set;
        }
    }
}