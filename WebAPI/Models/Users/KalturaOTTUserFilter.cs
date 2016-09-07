using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers.Scheme;
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
        [SchemeProperty(RequiresPermission = (int)RequestType.READ)]
        public string UserNameEqual { get; set; }

        /// <summary>
        /// External Id
        /// </summary>
        [DataMember(Name = "externalIdEqual")]
        [XmlElement("externalIdEqual", IsNullable = true)]
        [JsonProperty("externalIdEqual")]
        [SchemeProperty(RequiresPermission = (int)RequestType.READ)]
        public string ExternalIdEqual { get; set; }

        internal void Validate()
        {
            if (!string.IsNullOrEmpty(UserNameEqual) && !string.IsNullOrEmpty(ExternalIdEqual))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaOTTUserFilter.userNameEqual", "KalturaOTTUserFilter.externalIdEqual");
            }
        }

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