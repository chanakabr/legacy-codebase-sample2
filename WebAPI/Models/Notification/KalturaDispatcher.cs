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
    public partial class KalturaDispatcher : KalturaOTTObject
    {
     
    }

    public partial class KalturaSmsDispatcher : KalturaDispatcher
    {
    }

    public partial class KalturaMailDispatcher : KalturaDispatcher
    {
        /// <summary>
        /// Mail body template
        /// </summary>
        [DataMember(Name = "bodyTemplate")]
        [JsonProperty(PropertyName = "bodyTemplate")]
        [XmlElement(ElementName = "bodyTemplate")]
        public string BodyTemplate { get; set; }

        /// <summary>
        /// Mail subjsct template
        /// </summary>
        [DataMember(Name = "subjectTemplate")]
        [JsonProperty(PropertyName = "subjectTemplate")]
        [XmlElement(ElementName = "subjectTemplate")]
        public string SubjectTemplate { get; set; }
    }
}