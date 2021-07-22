using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaLabel : KalturaOTTObject
    {
        /// <summary>
        /// Label identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// Label value. It must be unique in the context of entityAttribute
        /// </summary>
        [DataMember(Name = "value")]
        [JsonProperty("value")]
        [XmlElement(ElementName = "value")]
        public string Value { get; set; }

        /// <summary>
        /// Identifier of entity to which label belongs
        /// </summary>
        [DataMember(Name = "entityAttribute")]
        [JsonProperty("entityAttribute")]
        [XmlElement(ElementName = "entityAttribute")]
        [SchemeProperty(InsertOnly = true)]
        public KalturaEntityAttribute EntityAttribute { get; set; }
    }
}
