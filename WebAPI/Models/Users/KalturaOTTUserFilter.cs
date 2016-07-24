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
        ///User Filter By
        /// </summary>
        [DataMember(Name = "userByEqual")]
        [JsonProperty("userByEqual")]
        [XmlElement(ElementName = "userByEqual")]
        public KalturaOTTUserBy UserByEqual { get; set; }

        /// <summary>
        /// The User identifiers
        /// </summary>
        [DataMember(Name = "valueEqual")]
        [XmlElement("valueEqual", IsNullable = true)]
        [JsonProperty("valueEqual")]
        public string ValueEqual { get; set; }

        public override KalturaOTTUserOrderBy GetDefaultOrderByValue()
        {
            return KalturaOTTUserOrderBy.ID_ASC;
        }


    }


    public enum KalturaOTTUserOrderBy
    {
        ID_ASC   
    }

    public enum KalturaOTTUserBy
    {
        USER_NAME,
        EXTERNAL_ID
    }
}