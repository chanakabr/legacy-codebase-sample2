using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Social
{
    public partial class KalturaSocialComment : KalturaOTTObject
    {
        /// <summary>
        /// Comment header
        /// </summary>
        [DataMember(Name = "header")]
        [JsonProperty(PropertyName = "header")]
        [XmlElement(ElementName = "header")]
        public string Header { get; set; }

        /// <summary>
        /// Comment body
        /// </summary>
        [DataMember(Name = "text")]
        [JsonProperty(PropertyName = "text")]
        [XmlElement(ElementName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// Comment creation date
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty(PropertyName = "createDate")]
        [XmlElement(ElementName = "createDate")]
        public long CreateDate { get; set; }

        /// <summary>
        /// The writer of the comment
        /// </summary>
        [DataMember(Name = "writer")]
        [JsonProperty(PropertyName = "writer")]
        [XmlElement(ElementName = "writer")]
        public string Writer { get; set; }
    }
}