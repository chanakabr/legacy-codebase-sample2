using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog.SearchPriorityGroup
{
    public partial class KalturaSearchPriorityGroupOrderedIdsSet : KalturaOTTObject
    {
        /// <summary>
        /// The order and effectively the priority of each group.
        /// </summary>
        [DataMember(Name = "priorityGroupIds")]
        [JsonProperty("priorityGroupIds")]
        [XmlElement(ElementName = "priorityGroupIds")]
        public string PriorityGroupIds { get; set; }
    }
}