using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Entitlement
    /// </summary>
    [Serializable]
    public class KalturaEntitlement : KalturaOTTObject
    {
        /// <summary>
        ///Entitlement type
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        public KalturaTransactionType type;

        /// <summary>
        ///Entitlement identifier
        /// </summary>
        [DataMember(Name = "entitlement_id")]
        [JsonProperty("entitlement_id")]
        public string entitlementId;

        /// <summary>
        ///Current uses
        /// </summary>
        [DataMember(Name = "current_uses")]
        [JsonProperty("current_uses")]
        public int currentUses;

        /// <summary>
        ///End date
        /// </summary>
        [DataMember(Name = "end_date")]
        [JsonProperty("end_date")]
        public long endDate { get; set; }

        /// <summary>
        ///Current date
        /// </summary>
        [DataMember(Name = "current_date")]
        [JsonProperty("current_date")]
        public long currentDate;

        /// <summary>
        ///Last view date
        /// </summary>
        [DataMember(Name = "last_view_date")]
        [JsonProperty("last_view_date")]
        public long lastViewDate;

        /// <summary>
        ///Purchase date
        /// </summary>
        [DataMember(Name = "purchase_date")]
        [JsonProperty("purchase_date")]
        public long purchaseDate;

        /// <summary>
        ///Purchase identifier (subscription + collection)
        /// </summary>
        [DataMember(Name = "purchase_id")]
        [JsonProperty("purchase_id")]
        public int purchaseID; 

        /// <summary>
        ///Payment Method
        /// </summary>
        [DataMember(Name = "payment_method")]
        [JsonProperty("payment_method")]
        public KalturaPaymentMethod paymentMethod;

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
        ///Max uses (subscription + PPV)
        /// </summary>
        [DataMember(Name = "max_uses")]
        [JsonProperty("max_uses")]
        public int maxUses;

        /// <summary>
        ///Next renewal date (subscription)
        /// </summary>
        [DataMember(Name = "next_renewal_date")]
        [JsonProperty("next_renewal_date")]
        public long nextRenewalDate;

        /// <summary>
        ///Recurring status (subscription)
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
        ///Media file identifier (ppv)
        /// </summary>
        [DataMember(Name = "media_file_id")]
        [JsonProperty("media_file_id")]
        public int mediaFileID;


    }
}