using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaLabelFilter : KalturaFilter<KalturaLabelOrderBy>
    {
        /// <summary>
        /// Comma-separated identifiers of labels
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn")]
        public string IdIn { get; set; }

        /// <summary>
        /// Filter the label with this value
        /// </summary>
        [DataMember(Name = "labelEqual")]
        [JsonProperty("labelEqual")]
        [XmlElement(ElementName = "labelEqual")]
        public string LabelEqual { get; set; }

        /// <summary>
        /// Filter labels which start with this value
        /// </summary> 
        [DataMember(Name = "labelStartsWith")]
        [JsonProperty("labelStartsWith")]
        [XmlElement(ElementName = "labelStartsWith")]
        public string LabelStartsWith { get; set; }

        /// <summary>
        /// Type of entity that labels are associated with
        /// </summary>
        [DataMember(Name = "entityAttributeEqual")]
        [JsonProperty("entityAttributeEqual")]
        [XmlElement(ElementName = "entityAttributeEqual")]
        public KalturaEntityAttribute EntityAttributeEqual { get; set; }

        public override KalturaLabelOrderBy GetDefaultOrderByValue()
        {
            return KalturaLabelOrderBy.NONE;
        }
    }
}
