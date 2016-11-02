using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Social
{
    public class KalturaSocialAction : KalturaOTTObject
    {
        /// <summary>
        /// Action type
        /// </summary>
        [DataMember(Name = "actionType")]
        [JsonProperty("actionType")]
        [XmlElement(ElementName = "actionType")]
        public KalturaSocialActionType ActionType { get; set; }

        /// <summary>
        /// EPOC based timestamp for when the action occurred
        /// </summary>
        [DataMember(Name = "actionTime")]
        [JsonProperty("actionTime")]
        [XmlElement(ElementName = "actionTime")]
        public long? ActionTime { get; set; }
    }

    public class KalturaSocialActionRate : KalturaSocialAction
    {
        /// <summary>
        /// The value of the rating
        /// </summary>
        [DataMember(Name = "rate")]
        [JsonProperty("rate")]
        [XmlElement(ElementName = "rate")]
        public int Rate { get; set; }

        public KalturaSocialActionRate(int value)
        {
            ActionType = KalturaSocialActionType.RATE;
            Rate = value;
        }
    }

    public enum KalturaSocialActionType
    {
        LIKE,
        WATCH,
        RATE,
    }
}