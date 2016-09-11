using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Filters;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Domains
{
    public class KalturaHouseholdUserFilter : KalturaFilter<KalturaHouseholdUserOrderBy>
    {
        /// <summary>
        /// The identifier of the household
        /// </summary>
        [DataMember(Name = "householdIdEqual")]
        [JsonProperty("householdIdEqual")]
        [XmlElement(ElementName = "householdIdEqual")]
        [SchemeProperty(RequiresPermission = (int)RequestType.READ)]
        public int? HouseholdIdEqual { get; set; }

        public override KalturaHouseholdUserOrderBy GetDefaultOrderByValue()
        {
            return KalturaHouseholdUserOrderBy.NONE;
        }
    }

    public enum KalturaHouseholdUserOrderBy
    {
        NONE
    }
}