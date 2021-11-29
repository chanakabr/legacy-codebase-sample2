using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog.SearchPriorityGroup
{
    public partial class KalturaSearchPriorityGroupListResponse : KalturaListResponse
    {
        /// <summary>
        /// List of search priority groups
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaSearchPriorityGroup> Objects { get; set; }
    }
}