using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    public partial class KalturaAssetFileContext : KalturaOTTObject
    {
        /// <summary>
        /// viewLifeCycle
        /// </summary>
        [DataMember(Name = "viewLifeCycle")]
        [JsonProperty("viewLifeCycle")]
        [XmlElement(ElementName = "viewLifeCycle")]
        [SchemeProperty(ReadOnly = true)]
        public string ViewLifeCycle { get; set; }

        /// <summary>
        /// fullLifeCycle
        /// </summary>
        [DataMember(Name = "fullLifeCycle")]
        [JsonProperty("fullLifeCycle")]
        [XmlElement(ElementName = "fullLifeCycle")]
        [SchemeProperty(ReadOnly = true)]
        public string FullLifeCycle { get; set; }

        /// <summary>
        /// isOfflinePlayBack
        /// </summary>
        [DataMember(Name = "isOfflinePlayBack")]
        [JsonProperty("isOfflinePlayBack")]
        [XmlElement(ElementName = "isOfflinePlayBack")]
        [SchemeProperty(ReadOnly = true)]
        public bool IsOfflinePlayBack { get; set; }

        /// <summary>
        /// Is Live PlayBack
        /// </summary>
        [DataMember(Name = "isLivePlayBack")]
        [JsonProperty("isLivePlayBack")]
        [XmlElement(ElementName = "isLivePlayBack")]
        [SchemeProperty(ReadOnly = true)]
        public bool IsLivePlayBack { get; set; }
    }
}