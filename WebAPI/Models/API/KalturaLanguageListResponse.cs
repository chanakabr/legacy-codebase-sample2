using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Models.Domains;
using WebAPI.Models.General;
using WebAPI.Models.Users;

namespace WebAPI.Models.API
{
    public class KalturaLanguageListResponse : KalturaListResponse
    {
        /// <summary>
        /// Languages
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaLanguage> Objects { get; set; }
    }
}
