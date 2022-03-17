using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace WebAPI.Models.Notifications
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