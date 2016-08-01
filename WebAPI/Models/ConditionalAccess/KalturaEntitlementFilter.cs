using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Entitlements filter 
    /// </summary>
    [OldStandard("entitlementType", "entitlement_type")]
    public class KalturaEntitlementFilter : KalturaFilter<KalturaEntitlementOrderBy> 
    {
        /// <summary>
        ///The type of the entitlements to return
        /// </summary>
        [DataMember(Name = "entitlementTypeEqual")]
        [JsonProperty("entitlementTypeEqual")]
        [XmlElement(ElementName = "entitlementTypeEqual")]
        public KalturaTransactionType EntitlementTypeEqual { get; set; }

        /// <summary>
        ///Reference type to filter by
        /// </summary>
        [DataMember(Name = "entityReferenceEqual")]
        [JsonProperty("entityReferenceEqual")]
        [XmlElement(ElementName = "entityReferenceEqual")]
        public KalturaEntityReferenceBy EntityReferenceEqual { get; set; }

        /// <summary>
        ///Is expired 
        /// </summary>
        [DataMember(Name = "isExpiredEqual")]
        [JsonProperty("isExpiredEqual")]
        [XmlElement(ElementName = "isExpiredEqual")]
        public bool? IsExpiredEqual { get; set; }

        public override KalturaEntitlementOrderBy GetDefaultOrderByValue()
        {
            return KalturaEntitlementOrderBy.PURCHASE_DATE_ASC;
        }

        internal bool getIsExpiredEqual()
        {
            return IsExpiredEqual.HasValue ? (bool)IsExpiredEqual : false;
        }
    }
}