using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Entitlements filter 
    /// </summary>
    [OldStandard("entitlementType", "entitlement_type")]
    public class KalturaEntitlementsFilter : KalturaOTTObject 
    {
        /// <summary>
        ///The type of the entitlements to return
        /// </summary>
        [DataMember(Name = "entitlementType")]
        [JsonProperty("entitlementType")]
        [XmlElement(ElementName = "entitlementType")]
        public KalturaTransactionType EntitlementType { get; set; }

        /// <summary>
        ///Reference type to filter by
        /// </summary>
        [DataMember(Name = "by")]
        [JsonProperty("by")]
        [XmlElement(ElementName = "by")]
        public KalturaEntityReferenceBy By { get; set; }

        ///// <summary>
        ///// order by
        ///// </summary>
        //[DataMember(Name = "orderBy")]
        //[JsonProperty("orderBy")]
        //[XmlElement(ElementName = "orderBy", IsNullable = true)]
        //[ValidationException(SchemaValidationType.FILTER_SUFFIX)]
        //public KalturaEntitlementOrderBy? OrderBy { get; set; }

        //public override object GetDefaultOrderByValue()
        //{
        //    return KalturaEntitlementOrderBy.PURCHASE_DATE_DESC;
        //}
    }
}