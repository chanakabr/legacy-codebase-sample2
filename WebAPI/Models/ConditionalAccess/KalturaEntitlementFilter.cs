using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Entitlements filter 
    /// </summary>
    public partial class KalturaEntitlementFilter : KalturaFilter<KalturaEntitlementOrderBy> 
    {
        /// <summary>
        ///The type of the entitlements to return
        /// </summary>
        [DataMember(Name = "entitlementTypeEqual")]
        [JsonProperty("entitlementTypeEqual")]
        [XmlElement(ElementName = "entitlementTypeEqual")]
        [OldStandardProperty("entitlement_type")]
        [Deprecated("4.8.0.0")]
        public KalturaTransactionType? EntitlementTypeEqual { get; set; }

        /// <summary>
        ///The type of the entitlements to return
        /// </summary>
        [DataMember(Name = "productTypeEqual")]
        [JsonProperty("productTypeEqual")]
        [XmlElement(ElementName = "productTypeEqual")]
        public KalturaTransactionType? ProductTypeEqual { get; set; }

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

        internal void Validate()
        {
            if (!ProductTypeEqual.HasValue && !EntitlementTypeEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaEntitlementFilter.productTypeEqual");
            }
        }
    }
}