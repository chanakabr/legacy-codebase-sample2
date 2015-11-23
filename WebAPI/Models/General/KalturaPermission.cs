using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace WebAPI.Models.General
{
    public class KalturaPermission : KalturaOTTObject
    {
        /// <summary>
        /// Permission identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public long Id { get; set; }

        /// <summary>
        /// Permission name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// List of permission items associated with the permission
        /// </summary>
        [DataMember(Name = "permissionItems")]
        [JsonProperty("permissionItems")]
        [XmlArray(ElementName = "permissionItems", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaPermissionItem> PermissionItems { get; set; }

        /// <summary>
        /// Permission partner identifier
        /// </summary>
        [DataMember(Name = "partnerId")]
        [JsonProperty("partnerId")]
        [XmlElement(ElementName = "partnerId")]
        public int PartnerId { get; set; }
    }
}