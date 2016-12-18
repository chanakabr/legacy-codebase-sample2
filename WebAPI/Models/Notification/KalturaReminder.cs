using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Notifications
{
    [Serializable]
    public class KalturaReminder : KalturaOTTObject
    {
        /// <summary>
        /// Reminder name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name")]
        [SchemeProperty(ReadOnly = true)]
        public string Name { get; set; }

        /// <summary>
        /// Identifies the reminder type (Series, etc). 
        /// Possible values: 0 – Series.
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty(PropertyName = "type")]
        [XmlElement(ElementName = "type")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaReminderType Type { get; set; }

        /// <summary>
        /// Reminder id
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public int? Id { get; set; }

        internal int getId()
        {
            return Id.HasValue ? (int)Id : 0;
        }
    }
}