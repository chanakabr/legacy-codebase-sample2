using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.General
{
    /// <summary>
    /// DynamicListIdInFilter
    /// </summary>
    public partial class KalturaDynamicListIdInFilter : KalturaDynamicListFilter
    {
        /// <summary>
        /// DynamicList identifiers
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public string IdIn { get; set; }
    }
}
