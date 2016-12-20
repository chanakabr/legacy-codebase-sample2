using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;
using WebAPI.Models.Notifications;

namespace WebAPI.Models.Notification
{
    /// <summary>
    /// List of reminders from DB.
    /// </summary>
    [DataContract(Name = "KalturaReminderListResponse", Namespace = "")]
    [XmlRoot("KalturaReminderListResponse")]
    public class KalturaReminderListResponse : KalturaListResponse
    {
        /// <summary>
        /// Reminders
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaReminder> Reminders { get; set; }
    }
}