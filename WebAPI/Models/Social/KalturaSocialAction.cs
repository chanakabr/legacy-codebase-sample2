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
    }

    public class KalturaSocialActionRate : KalturaSocialAction
    {
        /// <summary>
        /// The value of the rating
        /// </summary>
        [DataMember(Name = "value")]
        [JsonProperty("value")]
        [XmlElement(ElementName = "value")]
        public int Value { get; set; }

        public KalturaSocialActionRate(int value)
        {
            ActionType = KalturaSocialActionType.RATE;
            Value = value;
        }
    }

    public enum KalturaSocialActionType
    {
        LIKE,
        WATCH,
        RATE,
    }
}