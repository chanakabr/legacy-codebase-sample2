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
    /// Household limitation module
    /// </summary>
    public partial class KalturaHouseholdLimitationModuleListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of household limitation module
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaHouseholdLimitationModule> PlaybackProfiles { get; set; }
    }
}