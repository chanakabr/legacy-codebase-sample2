using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public class KalturaRuleFilter : KalturaOTTObject
    {
        /// <summary>
        ///Reference type to filter by
        /// </summary>
        [DataMember(Name = "by")]
        [JsonProperty("by")]
        [XmlElement(ElementName = "by")]
        public KalturaEntityReferenceBy By { get; set; }

        /// <summary>
        /// The identifier of the household user for whom to filter the rule (if filtering by user)
        /// </summary>
        [DataMember(Name = "household_id")]
        [JsonProperty("household_id")]
        [XmlElement(ElementName = "household_id")]
        public string HouseholdUserId { get; set; }
    }
}