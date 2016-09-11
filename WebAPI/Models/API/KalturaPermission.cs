using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    [XmlInclude(typeof(KalturaGroupPermission))]
    public class KalturaPermission : KalturaOTTObject
    {
        /// <summary>
        /// Permission identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long? Id { get; set; }

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
    }
}