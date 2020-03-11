using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.Social
{
    public partial class KalturaSocialUserConfig : KalturaSocialConfig
    {
        /// <summary>
        /// List of action permission items
        /// </summary>
        [DataMember(Name = "actionPermissionItems")]
        [JsonProperty("actionPermissionItems")]
        [XmlArray(ElementName = "actionPermissionItems", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaActionPermissionItem> PermissionItems { get; set; }
    }
}