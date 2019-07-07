using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.General
{
    /// <summary>
    /// A string representation to return an array of strings
    /// </summary>
    public partial class KalturaStringValue : KalturaValue
    {
        /// <summary>
        /// Value
        /// </summary>
        [DataMember(Name = "value")]
        [XmlElement("value", IsNullable = true)]
        [JsonProperty("value")]
        public string value { get; set; }
    }
}