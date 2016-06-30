using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Domains
{
    public enum KalturaHouseholdWith
    {
        users_base_info,
        users_full_info
    }

    /// <summary>
    /// Holder object for Household With enum
    /// </summary>
    public class KalturaHouseholdWithHolder : KalturaOTTObject
    {
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        [XmlElement("type")]
        public KalturaHouseholdWith type { get; set; }
    }
}