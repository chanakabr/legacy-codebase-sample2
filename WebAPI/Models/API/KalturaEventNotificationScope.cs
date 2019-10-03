using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Kaltura event notification scope
    /// </summary>
    public abstract partial class KalturaEventNotificationScope : KalturaOTTObject
    {
        /// <summary>
        /// The Scope
        /// </summary>
        public KalturaScopeType Scope { get; set; }
    }

    /// <summary>
    /// Kaltura Scope type
    /// </summary>
    public enum KalturaScopeType
    {
        ConcurrencyEventNotification
    }

    /// <summary>
    /// Kaltura event notification event object type
    /// </summary>
    public partial class KalturaEventNotificationEventObjectType : KalturaEventNotificationScope
    {
        /// <summary>
        /// Event object to fire
        /// </summary>
        [DataMember(Name = "eventObject")]
        [JsonProperty("eventObject")]
        [XmlElement(ElementName = "eventObject")]
        public KalturaOTTObject EventObject { get; set; }
    }
}
