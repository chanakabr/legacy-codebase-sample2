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
    public class KalturaBillingTransactions : KalturaOTTObject
    {
        /// <summary>
        ///Transactions
        /// </summary>
        [DataMember(Name = "transactions")]
        [JsonProperty("transactions")]
        [XmlElement(ElementName = "transactions")]
        public List<KalturaBillingTransaction> transactions { get; set; }

        /// <summary>
        ///Transactions Count
        /// </summary>
        [DataMember(Name = "transactions_count")]
        [JsonProperty("transactions_count")]
        [XmlElement(ElementName = "transactions_count")]
        public int transactionsCount { get; set; }
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
        public string recieptCode;

        /// <summary>
        ///Purchased Item Name
        /// </summary>
        [DataMember(Name = "purchased_item_name")]
        [JsonProperty("purchased_item_name")]
        [XmlElement(ElementName = "purchased_item_name")]
        public string purchasedItemName;

        /// <summary>
        ///Purchased Item Code
        /// </summary>
        [DataMember(Name = "purchased_item_code")]
        [JsonProperty("purchased_item_code")]
        [XmlElement(ElementName = "purchased_item_code")]
        public string purchasedItemCode;

        /// <summary>
        ///Item Type
        /// </summary>
        [DataMember(Name = "item_type")]
        [JsonProperty("item_type")]
        [XmlElement(ElementName = "item_type")]
        public KalturaBillingItemsType itemType;

        /// <summary>
        ///Billing Action
        /// </summary>
        [DataMember(Name = "billing_action")]
        [JsonProperty("billing_action")]
        [XmlElement(ElementName = "billing_action")]
        public KalturaBillingAction billingAction;

        /// <summary>
        ///price
        /// </summary>
        [DataMember(Name = "price")]
        [JsonProperty("price")]
        [XmlElement(ElementName = "price")]
        public Pricing.KalturaPrice price;

        /// <summary>
        ///Action Date
        /// </summary>
        [DataMember(Name = "action_date")]
        [JsonProperty("action_date")]
        [XmlElement(ElementName = "action_date")]
        public long actionDate;

        /// <summary>
        ///Start Date
        /// </summary>
        [DataMember(Name = "start_date")]
        [JsonProperty("start_date")]
        [XmlElement(ElementName = "start_date")]
        public long startDate;

        /// <summary>
        /// End Date
        /// </summary>
        [DataMember(Name = "end_date")]
        [JsonProperty("end_date")]
        [XmlElement(ElementName = "end_date")]
        public long endDate;

        /// <summary>
        ///Payment Method
        /// </summary>
        [DataMember(Name = "payment_method")]
        [JsonProperty("payment_method")]
        [XmlElement(ElementName = "payment_method")]
        public KalturaPaymentMethod paymentMethod;

        /// <summary>
        ///Payment Method Extra Details
        /// </summary>
        [DataMember(Name = "payment_method_extra_details")]
        [JsonProperty("payment_method_extra_details")]
        [XmlElement(ElementName = "payment_method_extra_details")]
        public string paymentMethodExtraDetails;

        /// <summary>
        ///Is Recurring
        /// </summary>
        [DataMember(Name = "is_recurring")]
        [JsonProperty("is_recurring")]
        [XmlElement(ElementName = "is_recurring")]
        public bool isRecurring;

        /// <summary>
        ///Billing Provider Ref
        /// </summary>
        [DataMember(Name = "billing_provider_ref")]
        [JsonProperty("billing_provider_ref")]
        [XmlElement(ElementName = "billing_provider_ref")]
        public Int32 billingProviderRef;

        /// <summary>
        ///Purchase ID
        /// </summary>
        [DataMember(Name = "purchase_id")]
        [JsonProperty("purchase_id")]
        [XmlElement(ElementName = "purchase_id")]
        public Int32 purchaseID;

        /// <summary>
        ///Remarks
        /// </summary>
        [DataMember(Name = "remarks")]
        [JsonProperty("remarks")]
        [XmlElement(ElementName = "remarks")]
        public string remarks;
    }

}