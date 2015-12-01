using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Permissions filter
    /// </summary>
    public class KalturaPermissionsFilter : KalturaOTTObject
    {

        /// <summary>
        /// The permissions identifiers
        /// </summary>
        [DataMember(Name = "ids")]
        [JsonProperty("ids")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaLongValue> Ids { get; set; }
    }
}