using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    public enum KalturaTransactionHistoryOrderBy
    {
        CREATE_DATE_ASC,
        CREATE_DATE_DESC
    }

    /// <summary>
    /// Transactions filter
    /// </summary>
    public partial class KalturaTransactionHistoryFilter : KalturaFilter<KalturaTransactionHistoryOrderBy>
    {
        public override KalturaTransactionHistoryOrderBy GetDefaultOrderByValue()
        {
            return KalturaTransactionHistoryOrderBy.CREATE_DATE_DESC;
        }

        /// <summary>
        ///Reference type to filter by
        /// </summary>
        [DataMember(Name = "entityReferenceEqual")]
        [JsonProperty("entityReferenceEqual")]
        [XmlElement(ElementName = "entityReferenceEqual")]
        public KalturaEntityReferenceBy EntityReferenceEqual { get; set; }

        /// <summary>
        ///Filter transactions later than specific date
        /// </summary>
        [DataMember(Name = "startDateGreaterThanOrEqual")]
        [JsonProperty("startDateGreaterThanOrEqual")]
        [XmlElement(ElementName = "startDateGreaterThanOrEqual", IsNullable = true)]
        public DateTime? StartDateGreaterThanOrEqual { get; set; }


        /// <summary>
        ///Filter transactions earlier than specific date
        /// </summary>
        [DataMember(Name = "endDateLessThanOrEqual")]
        [JsonProperty("endDateLessThanOrEqual")]
        [XmlElement(ElementName = "endDateLessThanOrEqual", IsNullable = true)]
        public DateTime? EndDateLessThanOrEqual { get; set; }

        /// <summary>
        ///Filter transaction by entitlement id
        /// </summary>
        [DataMember(Name = "entitlementIdEqual")]
        [JsonProperty("entitlementIdEqual")]
        [XmlElement(ElementName = "entitlementIdEqual", IsNullable = true)]
        public long? EntitlementIdEqual { get; set; }

        /// <summary>
        ///Filter transaction by external Id
        /// </summary>
        [DataMember(Name = "externalIdEqual")]
        [JsonProperty("externalIdEqual")]
        [XmlElement(ElementName = "externalIdEqual", IsNullable = true)]
        public string ExternalIdEqual { get; set; }

        /// <summary>
        ///Filter transaction by type
        /// </summary>
        [DataMember(Name = "billingItemsTypeEqual")]
        [JsonProperty("billingItemsTypeEqual")]
        [XmlElement(ElementName = "billingItemsTypeEqual", IsNullable = true)]
        public KalturaBillingItemsType? BillingItemsTypeEqual { get; set; }

        /// <summary>
        ///Filter transaction by business module type
        /// </summary>
        [DataMember(Name = "billingActionEqual")]
        [JsonProperty("billingActionEqual")]
        [XmlElement(ElementName = "billingActionEqual", IsNullable = true)]
        public KalturaBillingAction? BillingActionEqual { get; set; }

        

            
    }
}