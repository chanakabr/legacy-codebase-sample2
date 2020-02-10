using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.ClientManagers.Client;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Kaltura event notification scope
    /// </summary>
    public abstract partial class KalturaEventNotificationScope : KalturaOTTObject
    {
        internal abstract bool NotifyEvent(int groupId);
    }

    /// <summary>
    /// Kaltura event notification object scope
    /// </summary>
    public partial class KalturaEventNotificationObjectScope : KalturaEventNotificationScope
    {
        /// <summary>
        /// Event object to fire
        /// </summary>
        [DataMember(Name = "eventObject")]
        [JsonProperty("eventObject")]
        [XmlElement(ElementName = "eventObject")]
        public KalturaEventObject EventObject { get; set; }

        internal override bool NotifyEvent(int groupId)
        {
            return ClientsManager.NotificationClient().DispatchEventNotification(groupId, this);
        }
    }
}
