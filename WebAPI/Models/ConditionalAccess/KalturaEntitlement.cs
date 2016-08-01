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
    /// Entitlement
    /// </summary>
    [Serializable]
    [OldStandard("entitlementId", "entitlement_id")]
    [OldStandard("currentUses", "current_uses")]
    [OldStandard("endDate", "end_date")]
    [OldStandard("currentDate", "current_date")]
    [OldStandard("lastViewDate", "last_view_date")]
    [OldStandard("purchaseDate", "purchase_date")]
    [OldStandard("purchaseId", "purchase_id")]
    [OldStandard("paymentMethod", "payment_method")]
    [OldStandard("deviceUdid", "device_udid")]
    [OldStandard("deviceName", "device_name")]
    [OldStandard("isCancelationWindowEnabled", "is_cancelation_window_enabled")]
    [OldStandard("maxUses", "max_uses")]
    [OldStandard("nextRenewalDate", "next_renewal_date")]
    [OldStandard("isRenewableForPurchase", "is_renewable_for_purchase")]
    [OldStandard("isRenewable", "is_renewable")]
    [OldStandard("mediaFileId", "media_file_id")]
    [OldStandard("mediaId", "media_id")]
    [OldStandard("isInGracePeriod", "is_in_grace_period")]
    public class KalturaEntitlement : KalturaOTTObject
    {
        /// <summary>
        ///Entitlement type
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        [XmlElement(ElementName = "type", IsNullable = true)]
        public KalturaTransactionType Type { get; set; }

        /// <summary>
        ///Entitlement identifier
        /// </summary>
        [DataMember(Name = "entitlementId")]
        [JsonProperty("entitlementId")]
        [XmlElement(ElementName = "entitlementId")]
        public string EntitlementId { get; set; }

        /// <summary>
        ///The current number of uses 
        /// </summary>
        [DataMember(Name = "currentUses")]
        [JsonProperty("currentUses")]
        [XmlElement(ElementName = "currentUses")]
        public int? CurrentUses { get; set; }

        /// <summary>
        ///The end date of the entitlement
        /// </summary>
        [DataMember(Name = "endDate")]
        [JsonProperty("endDate")]
        [XmlElement(ElementName = "endDate")]
        public long? EndDate { get; set; }

        /// <summary>
        ///Current date
        /// </summary>
        [DataMember(Name = "currentDate")]
        [JsonProperty("currentDate")]
        [XmlElement(ElementName = "currentDate")]
        public long? CurrentDate { get; set; }

        /// <summary>
        ///The last date the item was viewed
        /// </summary>
        [DataMember(Name = "lastViewDate")]
        [JsonProperty("lastViewDate")]
        [XmlElement(ElementName = "lastViewDate")]
        public long? LastViewDate { get; set; }

        /// <summary>
        ///Purchase date
        /// </summary>
        [DataMember(Name = "purchaseDate")]
        [JsonProperty("purchaseDate")]
        [XmlElement(ElementName = "purchaseDate")]
        public long? PurchaseDate { get; set; }

        /// <summary>
        ///Purchase identifier (for subscriptions and collections only)
        /// </summary>
        [DataMember(Name = "purchaseId")]
        [JsonProperty("purchaseId")]
        [XmlElement(ElementName = "purchaseId")]
        public int? PurchaseId { get; set; }

        /// <summary>
        ///Payment Method
        /// </summary>
        [DataMember(Name = "paymentMethod")]
        [JsonProperty("paymentMethod")]
        [XmlElement(ElementName = "paymentMethod", IsNullable = true)]
        public KalturaPaymentMethodType PaymentMethod { get; set; }

        /// <summary>
        ///The UDID of the device from which the purchase was made
        /// </summary>
        [DataMember(Name = "deviceUdid")]
        [JsonProperty("deviceUdid")]
        [XmlElement(ElementName = "deviceUdid")]
        public string DeviceUDID { get; set; }

        /// <summary>
        ///The name of the device from which the purchase was made
        /// </summary>
        [DataMember(Name = "deviceName")]
        [JsonProperty("deviceName")]
        [XmlElement(ElementName = "deviceName")]
        public string DeviceName { get; set; }

        /// <summary>
        ///Indicates whether a cancelation window period is enabled
        /// </summary>
        [DataMember(Name = "isCancelationWindowEnabled")]
        [JsonProperty("isCancelationWindowEnabled")]
        [XmlElement(ElementName = "isCancelationWindowEnabled")]
        public bool? IsCancelationWindowEnabled { get; set; }

        /// <summary>
        ///The maximum number of uses available for this item (only for subscription and PPV)
        /// </summary>
        [DataMember(Name = "maxUses")]
        [JsonProperty("maxUses")]
        [XmlElement(ElementName = "maxUses")]
        public int? MaxUses { get; set; }

        /// <summary>
        ///The date of the next renewal (only for subscription)
        /// </summary>
        [DataMember(Name = "nextRenewalDate")]
        [JsonProperty("nextRenewalDate")]
        [XmlElement(ElementName = "nextRenewalDate")]
        public long? NextRenewalDate { get; set; }

        /// <summary>
        ///Indicates whether the subscription is renewable in this purchase (only for subscription)
        /// </summary>
        [DataMember(Name = "isRenewableForPurchase")]
        [JsonProperty("isRenewableForPurchase")]
        [XmlElement(ElementName = "isRenewableForPurchase")]
        public bool? IsRenewableForPurchase { get; set; }

        /// <summary>
        ///Indicates whether a subscription is renewable (only for subscription)
        /// </summary>
        [DataMember(Name = "isRenewable")]
        [JsonProperty("isRenewable")]
        [XmlElement(ElementName = "isRenewable")]
        public bool? IsRenewable { get; set; }

        /// <summary>
        ///Media file identifier (only for PPV)
        /// </summary>
        [DataMember(Name = "mediaFileId")]
        [JsonProperty("mediaFileId")]
        [XmlElement(ElementName = "mediaFileId")]
        public int? MediaFileId { get; set; }

        /// <summary>
        ///Media identifier (only for PPV)
        /// </summary>
        [DataMember(Name = "mediaId")]
        [JsonProperty("mediaId")]
        [XmlElement(ElementName = "mediaId")]
        public int? MediaId { get; set; }

        /// <summary>
        /// Indicates whether the user is currently in his grace period entitlement
        /// </summary>
        [DataMember(Name = "isInGracePeriod")]
        [JsonProperty("isInGracePeriod")]
        [XmlElement(ElementName = "isInGracePeriod")]
        public bool? IsInGracePeriod { get; set; }

    }
}