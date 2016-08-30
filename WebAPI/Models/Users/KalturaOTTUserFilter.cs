using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// OTT User filter
    /// </summary>
    public class KalturaOTTUserFilter : KalturaFilter<KalturaOTTUserOrderBy>
    {
        /// <summary>
        ///User Name
        /// </summary>
        [DataMember(Name = "userNameEqual")]
        [JsonProperty("userNameEqual")]
        [XmlElement(ElementName = "userNameEqual")]
        public string UserNameEqual { get; set; }

        /// <summary>
        /// External Id
        /// </summary>
        [DataMember(Name = "externalIdEqual")]
        [XmlElement("externalIdEqual", IsNullable = true)]
        [JsonProperty("externalIdEqual")]
        public string ExternalIdEqual { get; set; }

        public override KalturaOTTUserOrderBy GetDefaultOrderByValue()
        {
            return KalturaOTTUserOrderBy.ID_ASC;
        }
    }


    public enum KalturaOTTUserOrderBy
    {
        ID_ASC   
    }
}