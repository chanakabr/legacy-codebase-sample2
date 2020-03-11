using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;
using WebAPI.Models.Users;

namespace WebAPI.Models.API
{
    public partial class KalturaCountryListResponse : KalturaListResponse
    {
        /// <summary>
        /// Countries
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaCountry> Objects { get; set; }
    }
}
