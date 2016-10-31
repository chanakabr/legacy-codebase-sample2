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
    /// Billing Transactions
    /// </summary>
    [Serializable]
    public class KalturaBillingTransactionListResponse : KalturaListResponse
    {
        /// <summary>
        ///Transactions
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaBillingTransaction> transactions { get; set; }
    }

    /// <summary>
    /// Billing Transaction
    /// </summary>
    [Serializable]
    [OldStandard("recieptCode", "reciept_code")]
    [OldStandard("purchasedItemName", "purchased_item_name")]
    [OldStandard("purchasedItemCode", "purchased_item_code")]
    [OldStandard("itemType", "item_type")]
    [OldStandard("billingAction", "billing_action")]
    [OldStandard("actionDate", "action_date")]
    [OldStandard("startDate", "start_date")]
    [OldStandard("endDate", "end_date")]
    [OldStandard("paymentMethod", "payment_method")]
    [OldStandard("paymentMethodExtraDetails", "payment_method_extra_details")]
    [OldStandard("isRecurring", "is_recurring")]
    [OldStandard("billingProviderRef", "billing_provider_ref")]
    [OldStandard("purchaseId", "purchase_id")]
    [XmlInclude(typeof(KalturaUserBillingTransaction))]
    public class KalturaBillingTransaction : KalturaOTTObject
    {
        /// <summary>
        ///Reciept Code
        /// </summary>
        [DataMember(Name = "recieptCode")]
        [JsonProperty("recieptCode")]
        [XmlElement(ElementName = "recieptCode")]
        [SchemeProperty(ReadOnly = true)]
        public string recieptCode { get; set; }

        /// <summary>
        ///Purchased Item Name
        /// </summary>
        [DataMember(Name = "purchasedItemName")]
        [JsonProperty("purchasedItemName")]
        [XmlElement(ElementName = "purchasedItemName")]
        [SchemeProperty(ReadOnly = true)]
        public string purchasedItemName { get; set; }

        /// <summary>
        ///Purchased Item Code
        /// </summary>
        [DataMember(Name = "purchasedItemCode")]
        [JsonProperty("purchasedItemCode")]
        [XmlElement(ElementName = "purchasedItemCode")]
        [SchemeProperty(ReadOnly = true)]
        public string purchasedItemCode { get; set; }

        /// <summary>
        ///Item Type
        /// </summary>
        [DataMember(Name = "itemType")]
        [JsonProperty("itemType")]
        [XmlElement(ElementName = "itemType")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaBillingItemsType itemType { get; set; }

        /// <summary>
        ///Billing Action
        /// </summary>
        [DataMember(Name = "billingAction")]
        [JsonProperty("billingAction")]
        [XmlElement(ElementName = "billingAction")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaBillingAction billingAction { get; set; }

        /// <summary>
        ///price
        /// </summary>
        [DataMember(Name = "price")]
        [JsonProperty("price")]
        [XmlElement(ElementName = "price", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public Pricing.KalturaPrice price { get; set; }

        /// <summary>
        ///Action Date
        /// </summary>
        [DataMember(Name = "actionDate")]
        [JsonProperty("actionDate")]
        [XmlElement(ElementName = "actionDate")]
        [SchemeProperty(ReadOnly = true)]
        public long? actionDate { get; set; }

        /// <summary>
        ///Start Date
        /// </summary>
        [DataMember(Name = "startDate")]
        [JsonProperty("startDate")]
        [XmlElement(ElementName = "startDate")]
        [SchemeProperty(ReadOnly = true)]
        public long? startDate { get; set; }

        /// <summary>
        /// End Date
        /// </summary>
        [DataMember(Name = "endDate")]
        [JsonProperty("endDate")]
        [XmlElement(ElementName = "endDate")]
        [SchemeProperty(ReadOnly = true)]
        public long? endDate { get; set; }

        /// <summary>
        ///Payment Method
        /// </summary>
        [DataMember(Name = "paymentMethod")]
        [JsonProperty("paymentMethod")]
        [XmlElement(ElementName = "paymentMethod")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaPaymentMethodType paymentMethod { get; set; }

        /// <summary>
        ///Payment Method Extra Details
        /// </summary>
        [DataMember(Name = "paymentMethodExtraDetails")]
        [JsonProperty("paymentMethodExtraDetails")]
        [XmlElement(ElementName = "paymentMethodExtraDetails")]
        [SchemeProperty(ReadOnly = true)]
        public string paymentMethodExtraDetails { get; set; }

        /// <summary>
        ///Is Recurring
        /// </summary>
        [DataMember(Name = "isRecurring")]
        [JsonProperty("isRecurring")]
        [XmlElement(ElementName = "isRecurring")]
        [SchemeProperty(ReadOnly = true)]
        public bool? isRecurring { get; set; }

        /// <summary>
        ///Billing Provider Ref
        /// </summary>
        [DataMember(Name = "billingProviderRef")]
        [JsonProperty("billingProviderRef")]
        [XmlElement(ElementName = "billingProviderRef")]
        [SchemeProperty(ReadOnly = true)]
        public Int32? billingProviderRef { get; set; }

        /// <summary>
        ///Purchase ID
        /// </summary>
        [DataMember(Name = "purchaseId")]
        [JsonProperty("purchaseId")]
        [XmlElement(ElementName = "purchaseId")]
        [SchemeProperty(ReadOnly = true)]
        public Int32? purchaseID { get; set; }

        /// <summary>
        ///Remarks
        /// </summary>
        [DataMember(Name = "remarks")]
        [JsonProperty("remarks")]
        [XmlElement(ElementName = "remarks")]
        [SchemeProperty(ReadOnly = true)]
        public string remarks { get; set; }
    }

    /// <summary>
    /// Billing transactions of single user
    /// </summary>
    [Serializable]
    [OldStandard("userId", "user_id")]
    [OldStandard("userFullName", "user_full_name")]
    [Obsolete]
    public class KalturaUserBillingTransaction : KalturaBillingTransaction
    {
        [DataMember(Name = "userId")]
        [JsonProperty("userId")]
        [XmlElement(ElementName = "userId")]
        [SchemeProperty(ReadOnly = true)]
        public string UserID
        {
            get;
            set;
        }

        [DataMember(Name = "userFullNName")]
        [JsonProperty("userFullName")]
        [XmlElement(ElementName = "userFullName")]
        [SchemeProperty(ReadOnly = true)]
        public string UserFullName { get; set; }
    }

}