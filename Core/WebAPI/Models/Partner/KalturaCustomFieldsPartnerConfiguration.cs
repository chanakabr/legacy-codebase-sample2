using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.Partner
{
    /// <summary>
    /// Custom Fields Partner Configuration
    /// </summary>
    public partial class KalturaCustomFieldsPartnerConfiguration : KalturaPartnerConfiguration
    {
        /// <summary>
        /// Array of clientTag values
        /// </summary>
        [DataMember(Name = "metaSystemNameInsteadOfAliasList")]
        [JsonProperty("metaSystemNameInsteadOfAliasList")]
        [XmlElement(ElementName = "metaSystemNameInsteadOfAliasList")]
        [Managers.Scheme.SchemeProperty(IsNullable = false)]
        public string MetaSystemNameInsteadOfAliasList { get; set; }
    }
}
