using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Purchase settings and PIN
    /// </summary>
    [Serializable]
    class PurchaseSettingsResponse
    {
        /// <summary>
        /// Purchase settings type - block, ask or allow
        /// </summary>
        public ePurchaeSettingsType type
        {
            get;
            set;
        }

        /// <summary>
        /// PIN applied on user or household
        /// </summary>
        [DataMember(Name = "pin")]
        [JsonProperty(PropertyName = "pin", NullValueHandling = NullValueHandling.Ignore)]    
        public string pin
        {
            get;
            set;
        }

        /// <summary>
        /// Where were these settings defined - account, household or user
        /// </summary>
        [DataMember(Name = "origin")]
        [JsonProperty(PropertyName = "origin")]
        public eRuleLevel origin
        {
            get;
            set;
        }
        
    }

    /// <summary>
    /// One of the following options:
    /// -	Block – purchases not allowed
    /// -	Ask – allow purchase subject to purchase PIN
    /// -	Allow – allow purchases with no purchase PIN
    /// </summary>
    public enum ePurchaeSettingsType
    {
        Block = 0,
        Ask = 1,
        Allow = 2
    }
}
