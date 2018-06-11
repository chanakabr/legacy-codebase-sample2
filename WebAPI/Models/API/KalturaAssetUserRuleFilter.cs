using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public enum KalturaAssetUserRuleOrderBy
    {
        NONE
    }

    /// <summary>
    /// Asset user rule filter
    /// </summary>
    public class KalturaAssetUserRuleFilter : KalturaFilter<KalturaAssetUserRuleOrderBy>
    {
        /// <summary>
        /// Indicates if to get the asset user rule list for the attached user or for the entire group
        /// </summary>
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        [DataMember(Name = "attachedUserIdEqualCurrent")]
        [JsonProperty("attachedUserIdEqualCurrent")]
        [XmlElement(ElementName = "attachedUserIdEqualCurrent", IsNullable = true)]
        public bool? AttachedUserIdEqualCurrent { get; set; }

        public override KalturaAssetUserRuleOrderBy GetDefaultOrderByValue()
        {
            return KalturaAssetUserRuleOrderBy.NONE;
        }
    }
}