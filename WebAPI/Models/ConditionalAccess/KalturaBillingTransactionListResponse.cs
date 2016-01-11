using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
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
    public class KalturaBillingTransaction : KalturaOTTObject
    {
        /// <summary>
        ///Reciept Code
        /// </summary>
        [DataMember(Name = "reciept_code")]
        [JsonProperty("reciept_code")]
        [XmlElement(ElementName = "reciept_code")]
        public string recieptCode { get; set; }

        /// <summary>
        ///Purchased Item Name
        /// </summary>
        [DataMember(Name = "purchased_item_name")]
        [JsonProperty("purchased_item_name")]
        [XmlElement(ElementName = "purchased_item_name")]
        public string purchasedItemName { get; set; }

        /// <summary>
        ///Purchased Item Code
        /// </summary>
        [DataMember(Name = "purchased_item_code")]
        [JsonProperty("purchased_item_code")]
        [XmlElement(ElementName = "purchased_item_code")]
        public string purchasedItemCode { get; set; }

        /// <summary>
        ///Item Type
        /// </summary>
        [DataMember(Name = "item_type")]
        [JsonProperty("item_type")]
        [XmlElement(ElementName = "item_type")]
        public KalturaBillingItemsType itemType { get; set; }

        /// <summary>
        ///Billing Action
        /// </summary>
        [DataMember(Name = "billing_action")]
        [JsonProperty("billing_action")]
        [XmlElement(ElementName = "billing_action", IsNullable = true)]
        public KalturaBillingAction billingAction { get; set; }

        /// <summary>
        ///price
        /// </summary>
        [DataMember(Name = "price")]
        [JsonProperty("price")]
        [XmlElement(ElementName = "price", IsNullable = true)]
        public Pricing.KalturaPrice price { get; set; }

        /// <summary>
        ///Action Date
        /// </summary>
        [DataMember(Name = "action_date")]
        [JsonProperty("action_date")]
        [XmlElement(ElementName = "action_date")]
        public long actionDate { get; set; }

        /// <summary>
        ///Start Date
        /// </summary>
        [DataMember(Name = "start_date")]
        [JsonProperty("start_date")]
        [XmlElement(ElementName = "start_date")]
        public long startDate { get; set; }

        /// <summary>
        /// End Date
        /// </summary>
        [DataMember(Name = "end_date")]
        [JsonProperty("end_date")]
        [XmlElement(ElementName = "end_date")]
        public long endDate { get; set; }

        /// <summary>
        ///Payment Method
        /// </summary>
        [DataMember(Name = "payment_method")]
        [JsonProperty("payment_method")]
        [XmlElement(ElementName = "payment_method", IsNullable = true)]
        public KalturaPaymentMethod paymentMethod { get; set; }

        /// <summary>
        ///Payment Method Extra Details
        /// </summary>
        [DataMember(Name = "payment_method_extra_details")]
        [JsonProperty("payment_method_extra_details")]
        [XmlElement(ElementName = "payment_method_extra_details")]
        public string paymentMethodExtraDetails { get; set; }

        /// <summary>
        ///Is Recurring
        /// </summary>
        [DataMember(Name = "is_recurring")]
        [JsonProperty("is_recurring")]
        [XmlElement(ElementName = "is_recurring")]
        public bool isRecurring { get; set; }

        /// <summary>
        ///Billing Provider Ref
        /// </summary>
        [DataMember(Name = "billing_provider_ref")]
        [JsonProperty("billing_provider_ref")]
        [XmlElement(ElementName = "billing_provider_ref")]
        public Int32 billingProviderRef { get; set; }

        /// <summary>
        ///Purchase ID
        /// </summary>
        [DataMember(Name = "purchase_id")]
        [JsonProperty("purchase_id")]
        [XmlElement(ElementName = "purchase_id")]
        public Int32 purchaseID { get; set; }

        /// <summary>
        ///Remarks
        /// </summary>
        [DataMember(Name = "remarks")]
        [JsonProperty("remarks")]
        [XmlElement(ElementName = "remarks")]
        public string remarks { get; set; }
    }

    /// <summary>
    /// Billing transactions of single user
    /// </summary>
    [Serializable]
    public class KalturaUserBillingTransaction : KalturaBillingTransaction
    {
        [DataMember(Name = "user_id")]
        [JsonProperty("user_id")]
        [XmlElement(ElementName = "user_id")]
        public string UserID
        {
            get;
            set;
        }

        [DataMember(Name = "user_full_name")]
        [JsonProperty("user_full_name")]
        [XmlElement(ElementName = "user_full_name")]
        public string UserFullName { get; set; }
    }

}