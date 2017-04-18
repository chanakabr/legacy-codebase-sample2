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
        [OldStandardProperty("reciept_code")]
        public string recieptCode { get; set; }

        /// <summary>
        ///Purchased Item Name
        /// </summary>
        [DataMember(Name = "purchasedItemName")]
        [JsonProperty("purchasedItemName")]
        [XmlElement(ElementName = "purchasedItemName")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("purchased_item_name")]
        public string purchasedItemName { get; set; }

        /// <summary>
        ///Purchased Item Code
        /// </summary>
        [DataMember(Name = "purchasedItemCode")]
        [JsonProperty("purchasedItemCode")]
        [XmlElement(ElementName = "purchasedItemCode")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("purchased_item_code")]
        public string purchasedItemCode { get; set; }

        /// <summary>
        ///Item Type
        /// </summary>
        [DataMember(Name = "itemType")]
        [JsonProperty("itemType")]
        [XmlElement(ElementName = "itemType")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("item_type")]
        public KalturaBillingItemsType itemType { get; set; }

        /// <summary>
        ///Billing Action
        /// </summary>
        [DataMember(Name = "billingAction")]
        [JsonProperty("billingAction")]
        [XmlElement(ElementName = "billingAction")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("billing_action")]
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
        [OldStandardProperty("action_date")]
        public long? actionDate { get; set; }

        /// <summary>
        ///Start Date
        /// </summary>
        [DataMember(Name = "startDate")]
        [JsonProperty("startDate")]
        [XmlElement(ElementName = "startDate")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("start_date")]
        public long? startDate { get; set; }

        /// <summary>
        /// End Date
        /// </summary>
        [DataMember(Name = "endDate")]
        [JsonProperty("endDate")]
        [XmlElement(ElementName = "endDate")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("end_date")]
        public long? endDate { get; set; }

        /// <summary>
        ///Payment Method
        /// </summary>
        [DataMember(Name = "paymentMethod")]
        [JsonProperty("paymentMethod")]
        [XmlElement(ElementName = "paymentMethod")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("payment_method")]
        public KalturaPaymentMethodType paymentMethod { get; set; }

        /// <summary>
        ///Payment Method Extra Details
        /// </summary>
        [DataMember(Name = "paymentMethodExtraDetails")]
        [JsonProperty("paymentMethodExtraDetails")]
        [XmlElement(ElementName = "paymentMethodExtraDetails")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("payment_method_extra_details")]
        public string paymentMethodExtraDetails { get; set; }

        /// <summary>
        ///Is Recurring
        /// </summary>
        [DataMember(Name = "isRecurring")]
        [JsonProperty("isRecurring")]
        [XmlElement(ElementName = "isRecurring")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("is_recurring")]
        public bool? isRecurring { get; set; }

        /// <summary>
        ///Billing Provider Ref
        /// </summary>
        [DataMember(Name = "billingProviderRef")]
        [JsonProperty("billingProviderRef")]
        [XmlElement(ElementName = "billingProviderRef")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("billing_provider_ref")]
        public Int32? billingProviderRef { get; set; }

        /// <summary>
        ///Purchase ID
        /// </summary>
        [DataMember(Name = "purchaseId")]
        [JsonProperty("purchaseId")]
        [XmlElement(ElementName = "purchaseId")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("purchase_id")]
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
    [Obsolete]
    public class KalturaUserBillingTransaction : KalturaBillingTransaction
    {
        [DataMember(Name = "userId")]
        [JsonProperty("userId")]
        [XmlElement(ElementName = "userId")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("user_id")]
        public string UserID { get; set; }

        [DataMember(Name = "userFullName")]
        [JsonProperty("userFullName")]
        [XmlElement(ElementName = "userFullName")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("user_full_name")]
        public string UserFullName { get; set; }
    }

}