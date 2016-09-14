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

        /// <summary>
        /// External Id
        /// </summary>
        [DataMember(Name = "idIn")]
        [XmlElement("idIn", IsNullable = true)]
        [JsonProperty("idIn")]
        [SchemeProperty(RequiresPermission = (int)RequestType.READ)]
        public string IdIn { get; set; }

        internal void Validate()
        {
            if ((!string.IsNullOrEmpty(UserNameEqual) && !string.IsNullOrEmpty(ExternalIdEqual) && !string.IsNullOrEmpty(IdIn)) ||
                (!string.IsNullOrEmpty(UserNameEqual) && !string.IsNullOrEmpty(ExternalIdEqual)) ||
                (!string.IsNullOrEmpty(UserNameEqual) && !string.IsNullOrEmpty(IdIn)) ||
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