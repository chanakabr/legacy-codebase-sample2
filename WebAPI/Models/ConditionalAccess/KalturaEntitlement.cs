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
        [XmlElement(ElementName = "type")]
        public KalturaTransactionType type;

        /// <summary>
        ///Entitlement identifier
        /// </summary>
        [DataMember(Name = "entitlement_id")]
        [JsonProperty("entitlement_id")]
        [XmlElement(ElementName = "entitlement_id")]
        public string entitlementId;

        /// <summary>
        ///Current uses
        /// </summary>
        [DataMember(Name = "current_uses")]
        [JsonProperty("current_uses")]
        [XmlElement(ElementName = "current_uses")]
        public int currentUses;

        /// <summary>
        ///End date
        /// </summary>
        [DataMember(Name = "end_date")]
        [JsonProperty("end_date")]
        [XmlElement(ElementName = "end_date")]
        public long endDate { get; set; }

        /// <summary>
        ///Current date
        /// </summary>
        [DataMember(Name = "current_date")]
        [JsonProperty("current_date")]
        [XmlElement(ElementName = "current_date")]
        public long currentDate;

        /// <summary>
        ///Last view date
        /// </summary>
        [DataMember(Name = "last_view_date")]
        [JsonProperty("last_view_date")]
        [XmlElement(ElementName = "last_view_date")]
        public long lastViewDate;

        /// <summary>
        ///Purchase date
        /// </summary>
        [DataMember(Name = "purchase_date")]
        [JsonProperty("purchase_date")]
        [XmlElement(ElementName = "purchase_date")]
        public long purchaseDate;

        /// <summary>
        ///Purchase identifier (subscription + collection)
        /// </summary>
        [DataMember(Name = "purchase_id")]
        [JsonProperty("purchase_id")]
        [XmlElement(ElementName = "purchase_id")]
        public int purchaseID;

        /// <summary>
        ///Payment Method
        /// </summary>
        [DataMember(Name = "payment_method")]
        [JsonProperty("payment_method")]
        [XmlElement(ElementName = "payment_method")]
        public KalturaPaymentMethod paymentMethod;

        /// <summary>
        ///Device UDID
        /// </summary>
        [DataMember(Name = "device_udid")]
        [JsonProperty("device_udid")]
        [XmlElement(ElementName = "device_udid")]
        public string deviceUDID;

        /// <summary>
        ///Device Name
        /// </summary>
        [DataMember(Name = "device_name")]
        [JsonProperty("device_name")]
        [XmlElement(ElementName = "device_name")]
        public string deviceName;

        /// <summary>
        ///Cancel Window
        /// </summary>
        [DataMember(Name = "cancel_window")]
        [JsonProperty("cancel_window")]
        [XmlElement(ElementName = "cancel_window")]
        public bool cancelWindow;

        /// <summary>
        ///Max uses (subscription + PPV)
        /// </summary>
        [DataMember(Name = "max_uses")]
        [JsonProperty("max_uses")]
        [XmlElement(ElementName = "max_uses")]
        public int maxUses;

        /// <summary>
        ///Next renewal date (subscription)
        /// </summary>
        [DataMember(Name = "next_renewal_date")]
        [JsonProperty("next_renewal_date")]
        [XmlElement(ElementName = "next_renewal_date")]
        public long nextRenewalDate;

        /// <summary>
        ///Recurring status (subscription)
        /// </summary>
        [DataMember(Name = "recurring_status")]
        [JsonProperty("recurring_status")]
        [XmlElement(ElementName = "recurring_status")]
        public bool recurringStatus;

        /// <summary>
        ///Is Renewable (subscription)
        /// </summary>
        [DataMember(Name = "is_renewable")]
        [JsonProperty("is_renewable")]
        [XmlElement(ElementName = "is_renewable")]
        public bool isRenewable;

        /// <summary>
        ///Media file identifier (ppv)
        /// </summary>
        [DataMember(Name = "media_file_id")]
        [JsonProperty("media_file_id")]
        [XmlElement(ElementName = "media_file_id")]
        public int mediaFileID;


    }
}