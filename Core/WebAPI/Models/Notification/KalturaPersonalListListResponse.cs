using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.Api;
using WebAPI.Models.General;

namespace WebAPI.Models.Api
{
    /// <summary>
    /// List of KalturaPersonalList.
    /// </summary>
    [DataContract(Name = "KalturaPersonalListListResponse", Namespace = "")]
    [XmlRoot("KalturaPersonalListListResponse")]
    public partial class KalturaPersonalListListResponse : KalturaListResponse
    {
        /// <summary>
        /// Follow data list
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaPersonalList> PersonalListList { get; set; }
    }
}
