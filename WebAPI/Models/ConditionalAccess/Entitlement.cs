using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Entitlement
    /// </summary>
    [Serializable]
    public class Entitlement
    {
        /// <summary>
        ///Entitlement Type
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        public TransactionType type;

        /// <summary>
        ///Entitlement ID
        /// </summary>
        [DataMember(Name = "entitlements_id")]
        [JsonProperty("entitlements_id")]
        public string entitlementsId;

        /// <summary>
        ///Current Uses
        /// </summary>
        [DataMember(Name = "current_uses")]
        [JsonProperty("current_uses")]
        public Int32 currentUses;

        /// <summary>
        ///End Date
        /// </summary>
        [DataMember(Name = "end_date")]
        [JsonProperty("end_date")]
        public long endDate { get; set; }

        /// <summary>
        ///Current Date
        /// </summary>
        [DataMember(Name = "current_date")]
        [JsonProperty("current_date")]
        public long currentDate;

        /// <summary>
        ///Last View Date
        /// </summary>
        [DataMember(Name = "last_view_date")]
        [JsonProperty("last_view_date")]
        public long lastViewDate;

        /// <summary>
        ///Purchase Date
        /// </summary>
        [DataMember(Name = "purchase_date")]
        [JsonProperty("purchase_date")]
        public long purchaseDate;

        /// <summary>
        ///Purchase ID (sunscription + collection)
        /// </summary>
        [DataMember(Name = "purchase_id")]
        [JsonProperty("purchase_id")]
        public Int32 purchaseID; 

        /// <summary>
        ///Payment Method
        /// </summary>
        [DataMember(Name = "payment_method")]
        [JsonProperty("payment_method")]
        public PaymentMethod paymentMethod;

        /// <summary>
        ///Device UDID
        /// </summary>
        [DataMember(Name = "device_udid")]
        [JsonProperty("device_udid")]
        public string deviceUDID;

        /// <summary>
        ///Device Name
        /// </summary>
        [DataMember(Name = "device_name")]
        [JsonProperty("device_name")]
        public string deviceName;

        /// <summary>
        ///Cancel Window
        /// </summary>
        [DataMember(Name = "cancel_window")]
        [JsonProperty("cancel_window")]
        public bool cancelWindow;

        /// <summary>
        ///Max Uses (subscription + ppv)
        /// </summary>
        [DataMember(Name = "max_uses")]
        [JsonProperty("max_uses")]
        public Int32 maxUses;

        /// <summary>
        ///Next Renewal Date (subscription)
        /// </summary>
        [DataMember(Name = "next_renewal_date")]
        [JsonProperty("next_renewal_date")]
        public long nextRenewalDate;

        /// <summary>
        ///Recurring Status (subscription)
        /// </summary>
        [DataMember(Name = "recurring_status")]
        [JsonProperty("recurring_status")]
        public bool recurringStatus;

        /// <summary>
        ///Is Renewable (subscription)
        /// </summary>
        [DataMember(Name = "is_renewable")]
        [JsonProperty("is_renewable")]
        public bool isRenewable;

        /// <summary>
        ///Media FileID (ppv)
        /// </summary>
        [DataMember(Name = "media_file_id")]
        [JsonProperty("media_file_id")]
        public Int32 mediaFileID;


    }
}