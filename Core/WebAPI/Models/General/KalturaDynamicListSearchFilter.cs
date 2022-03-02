using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.General
{
    /// <summary>
    /// DynamicListSearchFilter
    /// </summary>
    public abstract partial class KalturaDynamicListSearchFilter : KalturaDynamicListFilter
    {
        /// <summary>
        /// DynamicList id to search by
        /// </summary>
        [DataMember(Name = "idEqual")]
        [JsonProperty("idEqual")]
        [XmlElement(ElementName = "idEqual", IsNullable = true)]
        [SchemeProperty(IsNullable = true, MinLong = 1)]
        public long? IdEqual { get; set; }

        /// <summary>
        /// udid value that should be in the DynamicList
        /// </summary>
        [DataMember(Name = "valueEqual")]
        [JsonProperty("valueEqual")]
        [XmlElement(ElementName = "valueEqual", IsNullable = true)]
        [SchemeProperty(IsNullable = true, MinLength = 1)]
        public string ValueEqual { get; set; }
    }
}
