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
    public partial class KalturaOTTUserFilter : KalturaFilter<KalturaOTTUserOrderBy>
    {
        /// <summary>
        /// Username
        /// </summary>
        [DataMember(Name = "usernameEqual")]
        [JsonProperty("usernameEqual")]
        [XmlElement(ElementName = "usernameEqual")]
        [SchemeProperty(RequiresPermission = (int)RequestType.READ)]
        public string UsernameEqual { get; set; }

        /// <summary>
        /// User external identifier
        /// </summary>
        [DataMember(Name = "externalIdEqual")]
        [XmlElement("externalIdEqual", IsNullable = true)]
        [JsonProperty("externalIdEqual")]
        [SchemeProperty(RequiresPermission = (int)RequestType.READ)]
        public string ExternalIdEqual { get; set; }

        /// <summary>
        /// List of user identifiers separated by ','
        /// </summary>
        [DataMember(Name = "idIn")]
        [XmlElement("idIn", IsNullable = true)]
        [JsonProperty("idIn")]
        [SchemeProperty(RequiresPermission = (int)RequestType.READ)]
        public string IdIn { get; set; }

        /// <summary>
        /// Comma separated list of role Ids.
        /// </summary>
        [DataMember(Name = "roleIdsIn")]
        [JsonProperty("roleIdsIn")]
        [XmlElement(ElementName = "roleIdsIn")]
        [SchemeProperty(RequiresPermission = (int)RequestType.WRITE)]
        public string RoleIdsIn { get; set; }

        internal void Validate(bool isOperatorOrAbove)
        {
            // validate that filter is only by username
            if (!string.IsNullOrEmpty(UsernameEqual))
            {
                if (!string.IsNullOrEmpty(ExternalIdEqual))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, 
                                                  "KalturaOTTUserFilter.userNameEqual", 
                                                  "KalturaOTTUserFilter.externalIdEqual");
                }

                if (!string.IsNullOrEmpty(IdIn))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER,
                                                  "KalturaOTTUserFilter.userNameEqual",
                                                  "KalturaOTTUserFilter.idIn");
                }

                if (!string.IsNullOrEmpty(RoleIdsIn))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER,
                                                  "KalturaOTTUserFilter.userNameEqual",
                                                  "KalturaOTTUserFilter.roleIdsIn");
                }
            }

            // validate that filter is only by externalId
            if (!string.IsNullOrEmpty(ExternalIdEqual))
            {
                if (!string.IsNullOrEmpty(IdIn))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER,
                                                  "KalturaOTTUserFilter.externalIdEqual",
                                                  "KalturaOTTUserFilter.idIn");
                }

                if (!string.IsNullOrEmpty(RoleIdsIn))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER,
                                                  "KalturaOTTUserFilter.externalIdEqual",
                                                  "KalturaOTTUserFilter.roleIdsIn");
                }
            }

            // validate that filter is only by idIn
            if (!string.IsNullOrEmpty(IdIn) && !string.IsNullOrEmpty(RoleIdsIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER,
                                              "KalturaOTTUserFilter.idIn",
                                              "KalturaOTTUserFilter.roleIdsIn");
            }

            // RoleIdsIn cannot be empty if the user isOperatorOrAbove
            if (string.IsNullOrEmpty(RoleIdsIn) && isOperatorOrAbove)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaOTTUserFilter.roleIdsIn");
            }
        }

        internal List<string> GetIdIn()
        {
            return this.GetItemsIn<List<string>, string>(IdIn, "KalturaOTTUserFilter.idIn");
        }

        internal HashSet<long> GetRoleIdsIn()
        {
            return this.GetItemsIn<HashSet<long>, long>(RoleIdsIn, "KalturaOTTUserFilter.roleIdsIn");
        }

        public override KalturaOTTUserOrderBy GetDefaultOrderByValue()
        {
            return KalturaOTTUserOrderBy.NONE;
        }
    }
    
    public enum KalturaOTTUserOrderBy
    {
        NONE   
    }
}