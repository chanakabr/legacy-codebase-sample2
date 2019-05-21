using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Notifications
{
    [Serializable]
    public partial class KalturaTopicNotificationFilter : KalturaFilter<KalturaTopicNotificationOrderBy>
    {
        /// <summary>
        /// Subscribe rreference
        /// </summary>
        [DataMember(Name = "subscribeReference")]
        [JsonProperty(PropertyName = "subscribeReference")]
        [XmlElement(ElementName = "subscribeReference")]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public KalturaSubscribeReference SubscribeReference { get; set; }

        public override KalturaTopicNotificationOrderBy GetDefaultOrderByValue()
        {
            return KalturaTopicNotificationOrderBy.NONE;
        }
    }

    public enum KalturaTopicNotificationOrderBy
    {
        NONE
    }

}