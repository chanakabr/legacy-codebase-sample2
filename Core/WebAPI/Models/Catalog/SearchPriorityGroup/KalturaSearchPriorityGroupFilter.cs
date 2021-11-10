using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog.SearchPriorityGroup
{
    public partial class KalturaSearchPriorityGroupFilter : KalturaFilter<KalturaSearchPriorityGroupOrderBy>
    {
        /// <summary>
        /// Return only search priority groups that are in use
        /// </summary>
        [DataMember(Name = "activeOnlyEqual")]
        [JsonProperty("activeOnlyEqual")]
        [XmlElement(ElementName = "activeOnlyEqual")]
        public bool ActiveOnly { get; set; }

        /// <summary>
        /// Identifier of search priority group to return
        /// </summary>
        [DataMember(Name = "idEqual")]
        [JsonProperty("idEqual")]
        [XmlElement(ElementName = "idEqual")]
        public long? IdEqual { get; set; }

        public override KalturaSearchPriorityGroupOrderBy GetDefaultOrderByValue()
        {
            return KalturaSearchPriorityGroupOrderBy.PRIORITY_DESC;
        }
    }
}