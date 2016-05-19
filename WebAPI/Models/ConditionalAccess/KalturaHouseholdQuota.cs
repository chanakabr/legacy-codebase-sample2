using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    public class KalturaHouseholdQuota : KalturaOTTObject
    {
        /// <summary>
        /// Household identifier
        /// </summary>
        [DataMember(Name = "householdId")]
        [JsonProperty("householdId")]
        [XmlElement(ElementName = "householdId", IsNullable = true)]
        public long HouseholdId { get; set; }

        /// <summary>
        /// Total quota that is allocated to the household
        /// </summary>
        [DataMember(Name = "totalQuota")]
        [JsonProperty("totalQuota")]
        [XmlElement(ElementName = "totalQuota")]
        public int TotalQuota { get; set; }

        /// <summary>
        /// Available quota that household has remaining
        /// </summary>
        [DataMember(Name = "availableQuota")]
        [JsonProperty("availableQuota")]
        [XmlElement(ElementName = "availableQuota")]
        public int AvailableQuota { get; set; }

    }
}