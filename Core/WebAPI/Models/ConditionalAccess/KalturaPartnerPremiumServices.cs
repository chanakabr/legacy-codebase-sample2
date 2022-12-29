using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Linq;
using WebAPI.Exceptions;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    public partial class KalturaPartnerPremiumServices : KalturaOTTObject
    {
        /// <summary>
        /// A list of services
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects")]
        [XmlArrayItem("item")]
        public List<KalturaPartnerPremiumService> PremiumServices { get; set; }
    }
}