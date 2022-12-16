using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.Notification
{
    public partial class KalturaDateTrigger : KalturaTrigger
    {
        /// <summary>
        /// Trigger date
        /// </summary>
        [DataMember(Name = "date")]
        [JsonProperty(PropertyName = "date")]
        [XmlElement(ElementName = "date")]
        public long Date { get; set; }
    }
}