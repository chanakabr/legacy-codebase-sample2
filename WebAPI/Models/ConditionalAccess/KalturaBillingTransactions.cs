using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Billing Transactions
    /// </summary>
    [Serializable]
    public class KalturaBillingTransactions
    {
         /// <summary>
        ///Transactions
        /// </summary>
        [DataMember(Name = "transactions")]
        [JsonProperty("transactions")]
        public List<KalturaBillingTransaction> transactions { get; set; }

        /// <summary>
        ///Transactions Count
        /// </summary>
        [DataMember(Name = "transactionscount")]
        [JsonProperty("transactionscount")]
        public int transactionsCount { get; set; }

    }

     /// <summary>
    /// Billing Transaction
    /// </summary>
    [Serializable]
    public class KalturaBillingTransaction
    {
        /// <summary>
        ///Reciept Code
        /// </summary>
        [DataMember(Name = "recieptcode")]
        [JsonProperty("recieptcode")]
        public string recieptCode;

        /// <summary>
        ///Purchased Item Name
        /// </summary>
        [DataMember(Name = "purchaseditemname")]
        [JsonProperty("purchaseditemname")]
        public string purchasedItemName;

        /// <summary>
        ///Purchased Item Code
        /// </summary>
        [DataMember(Name = "purchaseditemcode")]
        [JsonProperty("purchaseditemcode")]
        public string purchasedItemCode;

        /// <summary>
        ///Item Type
        /// </summary>
        [DataMember(Name = "itemtype")]
        [JsonProperty("itemtype")]
        public KalturaBillingItemsType itemType;

        /// <summary>
        ///Billing Action
        /// </summary>
        [DataMember(Name = "billingaction")]
        [JsonProperty("billingaction")]
        public KalturaBillingAction billingAction;

        /// <summary>
        ///price
        /// </summary>
        [DataMember(Name = "price")]
        [JsonProperty("price")]
        public Pricing.KalturaPrice price;

        /// <summary>
        ///Action Date
        /// </summary>
        [DataMember(Name = "actiondate")]
        [JsonProperty("actiondate")]
        public long actionDate;

        /// <summary>
        ///Start Date
        /// </summary>
        [DataMember(Name = "startDate")]
        [JsonProperty("startDate")]
        public long startDate;

        /// <summary>
        ///End Date
        /// </summary>
        [DataMember(Name = "enddate")]
        [JsonProperty("enddate")]
        public long endDate;

        /// <summary>
        ///Payment Method
        /// </summary>
        [DataMember(Name = "paymentmethod")]
        [JsonProperty("paymentmethod")]
        public KalturaPaymentMethod paymentMethod;

        /// <summary>
        ///Payment Method Extra Details
        /// </summary>
        [DataMember(Name = "paymentmethodextradetails")]
        [JsonProperty("paymentmethodextradetails")]
        public string paymentMethodExtraDetails;

        /// <summary>
        ///Is Recurring
        /// </summary>
        [DataMember(Name = "isrecurring")]
        [JsonProperty("isrecurring")]
        public bool isRecurring;
        
        /// <summary>
        ///Billing Provider Ref
        /// </summary>
        [DataMember(Name = "billingproviderref")]
        [JsonProperty("billingproviderref")]
        public Int32 billingProviderRef;

        /// <summary>
        ///Purchase ID
        /// </summary>
        [DataMember(Name = "purchaseid")]
        [JsonProperty("purchaseid")]
        public Int32 purchaseID;

        /// <summary>
        ///Remarks
        /// </summary>
        [DataMember(Name = "remarks")]
        [JsonProperty("remarks")]
        public string remarks;
    }

}