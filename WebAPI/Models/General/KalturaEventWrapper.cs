using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace WebAPI.Models.General
{
    public class KalturaEventWrapper : KalturaOTTObject
    {
        [DataMember(Name = "eventObject")]
        [JsonProperty(PropertyName = "eventObject")]
        [XmlElement(ElementName = "eventObject")]
        public KalturaOTTObject eventObject
        {
            get;
            set;
        }

        [DataMember(Name = "eventAction")]
        [JsonProperty(PropertyName = "eventAction")]
        [XmlElement(ElementName = "eventAction")]
        public KalturaEventAction eventAction
        {
            get;
            set;
        }

        [DataMember(Name = "objectType")]
        [JsonProperty(PropertyName = "objectType")]
        [XmlElement(ElementName = "objectType")]
        public string objectType
        {
            get;
            set;
        }
    }

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