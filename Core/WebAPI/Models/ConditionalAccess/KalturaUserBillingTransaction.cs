using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{ 
    /// <summary>
    /// Billing transactions of single user
    /// </summary>
    [Serializable]
    [Obsolete]
    public partial class KalturaUserBillingTransaction : KalturaBillingTransaction
    {
        [DataMember(Name = "userId")]
        [JsonProperty("userId")]
        [XmlElement(ElementName = "userId")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("user_id")]
        public string UserID { get; set; }

        [DataMember(Name = "userFullName")]
        [JsonProperty("userFullName")]
        [XmlElement(ElementName = "userFullName")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("user_full_name")]
        public string UserFullName { get; set; }
    }

}