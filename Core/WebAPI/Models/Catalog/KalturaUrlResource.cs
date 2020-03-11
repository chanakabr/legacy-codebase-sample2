using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaUrlResource : KalturaContentResource
    {
        /// <summary>
        /// URL of the content
        /// </summary>
        [DataMember(Name = "url")]
        [JsonProperty(PropertyName = "url")]
        [XmlElement(ElementName = "url")]
        public string Url { get; set; }

        public override string GetUrl(int groupId)
        {
            return Url;
        }
    }
}