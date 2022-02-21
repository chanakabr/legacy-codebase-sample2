using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    [Serializable]
    public partial class KalturaEventNotificationFilter : KalturaFilter<KalturaEventNotificationOrderBy>
    {
        /// <summary>
        /// Indicates which event notification to return by their event notifications Id.
        /// </summary>
        [DataMember(Name = "idEqual")]
        [JsonProperty("idEqual")]
        [XmlElement(ElementName = "objectIdidEqualEqual", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string IdEqual { get; set; }

        /// <summary>
        /// Indicates which objectId to return by their event notifications.
        /// </summary>
        [DataMember(Name = "objectIdEqual")]
        [JsonProperty("objectIdEqual")]
        [XmlElement(ElementName = "objectIdEqual", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        [SchemeProperty(MinInteger = 1)]
        public long? ObjectIdEqual { get; set; }

        /// <summary>
        /// Indicates which objectType to return by their event notifications.
        /// </summary>
        [DataMember(Name = "eventObjectTypeEqual")]
        [JsonProperty("eventObjectTypeEqual")]
        [XmlElement(ElementName = "eventObjectTypeEqual", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string EventObjectTypeEqual { get; set; }

        public override KalturaEventNotificationOrderBy GetDefaultOrderByValue()
        {
            return KalturaEventNotificationOrderBy.NONE;
        }
    }
}
