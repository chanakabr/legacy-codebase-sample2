using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace WebAPI.Models.General
{
    [Serializable]
    public class KalturaNotification : KalturaOTTObject
    {
        [DataMember(Name = "object")]
        [JsonProperty(PropertyName = "object")]
        [XmlElement(ElementName = "object")]
        public KalturaOTTObject eventObject
        {
            get;
            set;
        }

        [DataMember(Name = "eventType")]
        [JsonProperty(PropertyName = "eventType")]
        [XmlElement(ElementName = "eventType")]
        public KalturaEventAction eventType
        {
            get;
            set;
        }

        [DataMember(Name = "eventObjectType")]
        [JsonProperty(PropertyName = "eventObjectType")]
        [XmlElement(ElementName = "eventObjectType")]
        public string eventObjectType
        {
            get;
            set;
        }
    }

    [Serializable]
    public enum KalturaEventAction
    {
        None,
        Added,
        Changed,
        Copied,
        Created,
        Deleted,
        Erased,
        Saved,
        Updated,
        Replaced
    }

}