using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace WebAPI.Models.General
{
    [Serializable]
    public partial class KalturaEventContextAction: KalturaEventContext
    {
        /// <summary>
        /// The name of the called service
        /// </summary>
        [DataMember(Name = "service")]
        [JsonProperty(PropertyName = "service")]
        [XmlElement(ElementName = "service")]
        public string Service { get; set; }

        /// <summary>
        /// The name of the called action
        /// </summary>
        [DataMember(Name = "action")]
        [JsonProperty(PropertyName = "action")]
        [XmlElement(ElementName = "action")]
        public string Action { get; set; }
    }
}
