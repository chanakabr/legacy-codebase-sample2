using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Partner
{
    public partial class KalturaPersonalActivityCleanupConfiguration : KalturaOTTObject
    {
        /// <summary>
        /// Retention Period Days
        /// </summary>
        [DataMember(Name = "retentionPeriodDays")]
        [JsonProperty(PropertyName = "retentionPeriodDays")]
        [XmlElement(ElementName = "retentionPeriodDays")]
        [SchemeProperty(MinLong = 0)]
        public long RetentionPeriodDays { get; set; }
    }
}