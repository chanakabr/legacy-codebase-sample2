using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models.API
{
    /// <summary>
    /// PIN and its origin of definition
    /// </summary>
    [Serializable]
    public class KalturaPinResponse
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
        public eRuleLevel origin
        {
            get;
            set;
        }
    }
}