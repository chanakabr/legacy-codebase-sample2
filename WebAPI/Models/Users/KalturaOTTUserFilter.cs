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

        // TODO SHIR - SET permissions IN C:\SourceCode\Master\tvpapi_rest\permissions\permission_items\objects\kalturaottuser.json
        /// <summary>
        /// Comma separated list of role Ids.
        /// </summary>
        [DataMember(Name = "roleIdsIn")]
        [JsonProperty("roleIdsIn")]
        [XmlElement(ElementName = "roleIdsIn")]
        public string RoleIdsIn { get; set; }

        internal void Validate()
        {
            if ((!string.IsNullOrEmpty(UsernameEqual) && !string.IsNullOrEmpty(ExternalIdEqual) && !string.IsNullOrEmpty(IdIn)) ||
                (!string.IsNullOrEmpty(UsernameEqual) && !string.IsNullOrEmpty(ExternalIdEqual)) ||
                (!string.IsNullOrEmpty(UsernameEqual) && !string.IsNullOrEmpty(IdIn)) ||
                (!string.IsNullOrEmpty(IdIn) && !string.IsNullOrEmpty(ExternalIdEqual)))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaOTTUserFilter.userNameEqual", "KalturaOTTUserFilter.externalIdEqual");
            }
        }

        internal List<string> GetIdIn()
        {
            List<string> list = null;
            if (!string.IsNullOrEmpty(IdIn))
            {
                list = new List<string>();
                string[] stringValues = IdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    list.Add(stringValue);
                }
            }

            return list;
        }

        internal HashSet<long> GetRoleIdsIn()
        {
            HashSet<long> values = new HashSet<long>();

            if (!string.IsNullOrEmpty(RoleIdsIn))
            {
                string[] stringValues = RoleIdsIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (long.TryParse(stringValue, out value) && value != 0)
                    {
                        if (values.Contains(value))
                        {
                            throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, "roleIdsIn");
                        }

                        values.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "roleIdsIn");
                    }
                }
            }
            
            return values;
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