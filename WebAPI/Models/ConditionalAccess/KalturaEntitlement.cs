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
        public KalturaTransactionType Type;

        /// <summary>
        ///Entitlement identifier
        /// </summary>
        [DataMember(Name = "entitlement_id")]
        [JsonProperty("entitlement_id")]
        [XmlElement(ElementName = "entitlement_id")]
        public string EntitlementId;

        /// <summary>
        ///The current number of uses 
        /// </summary>
        [DataMember(Name = "current_uses")]
        [JsonProperty("current_uses")]
        [XmlElement(ElementName = "current_uses")]
        public int CurrentUses;

        /// <summary>
        ///The end date of the entitlement
        /// </summary>
        [DataMember(Name = "end_date")]
        [JsonProperty("end_date")]
        [XmlElement(ElementName = "end_date")]
        public DateTime EndDate { get; set; }

        /// <summary>
        ///Current date
        /// </summary>
        [DataMember(Name = "current_date")]
        [JsonProperty("current_date")]
        [XmlElement(ElementName = "current_date")]
        public DateTime CurrentDate;

        /// <summary>
        ///The last date the item was viewed
        /// </summary>
        [DataMember(Name = "last_view_date")]
        [JsonProperty("last_view_date")]
        [XmlElement(ElementName = "last_view_date")]
        public DateTime LastViewDate;

        /// <summary>
        ///Purchase date
        /// </summary>
        [DataMember(Name = "purchase_date")]
        [JsonProperty("purchase_date")]
        [XmlElement(ElementName = "purchase_date")]
        public DateTime PurchaseDate;

        /// <summary>
        ///Purchase identifier (for subscriptions and collections only)
        /// </summary>
        [DataMember(Name = "purchase_id")]
        [JsonProperty("purchase_id")]
        [XmlElement(ElementName = "purchase_id")]
        public int PurchaseId;

        /// <summary>
        ///Payment Method
        /// </summary>
        [DataMember(Name = "payment_method")]
        [JsonProperty("payment_method")]
        [XmlElement(ElementName = "payment_method")]
        public KalturaPaymentMethod PaymentMethod;

        /// <summary>
        ///The UDID of the device from which the purchase was made
        /// </summary>
        [DataMember(Name = "device_udid")]
        [JsonProperty("device_udid")]
        [XmlElement(ElementName = "device_udid")]
        public string DeviceUDID;

        /// <summary>
        ///The name of the device from which the purchase was made
        /// </summary>
        [DataMember(Name = "device_name")]
        [JsonProperty("device_name")]
        [XmlElement(ElementName = "device_name")]
        public string DeviceName;

        /// <summary>
        ///Indicates whether a cancelation window period is enabled
        /// </summary>
        [DataMember(Name = "is_cancelation_window_enabled")]
        [JsonProperty("is_cancelation_window_enabled")]
        [XmlElement(ElementName = "is_cancelation_window_enabled")]
        public bool IsCancelationWindowEnabled;

        /// <summary>
        ///The maximum number of uses available for this item (only for subscription and PPV)
        /// </summary>
        [DataMember(Name = "max_uses")]
        [JsonProperty("max_uses")]
        [XmlElement(ElementName = "max_uses")]
        public int MaxUses;

        /// <summary>
        ///The date of the next renewal (only for subscription)
        /// </summary>
        [DataMember(Name = "next_renewal_date")]
        [JsonProperty("next_renewal_date")]
        [XmlElement(ElementName = "next_renewal_date")]
        public DateTime NextRenewalDate;

        /// <summary>
        ///Indicates whether the subscription is renewable in this purchase (only for subscription)
        /// </summary>
        [DataMember(Name = "is_renewable_for_purchase")]
        [JsonProperty("is_renewable_for_purchase")]
        [XmlElement(ElementName = "is_renewable_for_purchase")]
        public bool IsRenewableForPurchase;

        /// <summary>
        ///Indicates whether a subscription is renewable (only for subscription)
        /// </summary>
        [DataMember(Name = "is_renewable")]
        [JsonProperty("is_renewable")]
        [XmlElement(ElementName = "is_renewable")]
        public bool IsRenewable;

        /// <summary>
        ///Media file identifier (only for PPV)
        /// </summary>
        [DataMember(Name = "media_file_id")]
        [JsonProperty("media_file_id")]
        [XmlElement(ElementName = "media_file_id")]
        public int MediaFileId;

        /// <summary>
        ///Media identifier (only for PPV)
        /// </summary>
        [DataMember(Name = "media_id")]
        [JsonProperty("media_id")]
        [XmlElement(ElementName = "media_id")]
        public int MediaId;

    }
}