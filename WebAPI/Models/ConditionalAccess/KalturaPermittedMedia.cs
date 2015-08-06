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
    /// Permitted item
    /// </summary>
    public class KalturaPermittedMedia : KalturaOTTObject
    {
        /// <summary>
        /// Media identifier
        /// </summary>
        [DataMember(Name = "media_id")]
        [JsonProperty("media_id")]
        [XmlElement(ElementName = "media_id")]
        public int MediaId { get; set; }

        /// <summary>
        /// Media file identifier
        /// </summary>
        [DataMember(Name = "file_id")]
        [JsonProperty("file_id")]
        [XmlElement(ElementName = "file_id")]
        public int FileId{ get; set; }

        /// <summary>
        /// The maximum number the item can be viewed
        /// </summary>
        [DataMember(Name = "max_views")]
        [JsonProperty("max_views")]
        [XmlElement(ElementName = "max_views")]
        public int MaxViews { get; set; }

        /// <summary>
        /// The number of times the item was already viewed 
        /// </summary>
        [DataMember(Name = "current_views")]
        [JsonProperty("current_views")]
        [XmlElement(ElementName = "current_views")]
        public int CurrentViews { get; set; }

        /// <summary>
        /// The last date the item can be viewed
        /// </summary>
        [DataMember(Name = "end_date")]
        [JsonProperty("end_date")]
        [XmlElement(ElementName = "end_date")]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// The purchase date of the item 
        /// </summary>
        [DataMember(Name = "purchase_date")]
        [JsonProperty("purchase_date")]
        [XmlElement(ElementName = "purchase_date")]
        public DateTime PurchaseDate { get; set; }

        /// <summary>
        /// The last date the item was viewed 
        /// </summary>
        [DataMember(Name = "last_view_date")]
        [JsonProperty("last_view_date")]
        [XmlElement(ElementName = "last_view_date")]
        public DateTime LastViewDate { get; set; }

        /// <summary>
        /// The payment method that was used to purchase the item
        /// </summary>
        [DataMember(Name = "payment_method")]
        [JsonProperty("payment_method")]
        [XmlElement(ElementName = "payment_method")]
        public KalturaPaymentMethod PaymentMethod{ get; set; }

        /// <summary>
        /// The device UDID from which the item was purchased
        /// </summary>
        [DataMember(Name = "device_udid")]
        [JsonProperty("device_udid")]
        [XmlElement(ElementName = "device_udid")]
        public string DeviceUDID { get; set; }

        /// <summary>
        /// The device name from which the item was purchased
        /// </summary>
        [DataMember(Name = "device_name")]
        [JsonProperty("device_name")]
        [XmlElement(ElementName = "device_name")]
        public string DeviceName { get; set; }

        /// <summary>
        /// Indicates whether a cancelation window enabled
        /// </summary>
        [DataMember(Name = "is_cancelation_window_enabled")]
        [JsonProperty("is_cancelation_window_enabled")]
        [XmlElement(ElementName = "is_cancelation_window_enabled")]
        public bool IsCancelationWindowEnabled { get; set; }
    }
}