using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace WebAPI.Models.Social
{
    public class KalturaSocialUserConfig : KalturaSocialConfig
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