using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public class KalturaParentalRuleFilter : KalturaFilter<KalturaParentalRuleOrderBy>
    {
        /// <summary>
        ///Reference type to filter by
        /// </summary>
        [DataMember(Name = "entityReferenceEqual")]
        [JsonProperty("entityReferenceEqual")]
        [XmlElement(ElementName = "entityReferenceEqual")]
        public KalturaEntityReferenceBy EntityReferenceEqual { get; set; }

        public override KalturaParentalRuleOrderBy GetDefaultOrderByValue()
        {
            return KalturaParentalRuleOrderBy.PARTNER_SORT_VALUE;
        }
    }
}