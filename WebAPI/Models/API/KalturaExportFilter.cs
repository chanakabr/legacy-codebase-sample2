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
    /// Bulk export tasks filter
    /// </summary>
    public class KalturaExportFilter : KalturaOTTObject
    {

        /// <summary>
        /// The tasks identifiers or external keys - depends on "by" parameter
        /// </summary>
        [DataMember(Name = "external_keys")]
        [JsonProperty("external_keys")]
        [XmlArray(ElementName = "objects")]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaStringValue> ExternalKeys{ get; set; }
    }
}