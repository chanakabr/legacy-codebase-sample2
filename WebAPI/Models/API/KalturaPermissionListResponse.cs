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

    /// <summary>
    /// Permissions list
    /// </summary>
    [DataContract(Name = "Permissions", Namespace = "")]
    [XmlRoot("Permissions")]
    public partial class KalturaPermissionListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of permissions
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaPermission> Permissions { get; set; }
    }
}