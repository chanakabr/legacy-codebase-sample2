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
    /// Permissions filter
    /// </summary>
    public partial class KalturaPermissionFilter : KalturaFilter<KalturaPermissionOrderBy>
    {

        /// <summary>
        /// Indicates whether the results should be filtered by userId using the current
        /// </summary>
        [DataMember(Name = "currentUserPermissionsContains")]
        [JsonProperty(PropertyName = "currentUserPermissionsContains")]
        [XmlElement(ElementName = "currentUserPermissionsContains", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public bool? CurrentUserPermissionsContains { get; set; }

        public override KalturaPermissionOrderBy GetDefaultOrderByValue()
        {
            return KalturaPermissionOrderBy.NONE;
        }

    }

    public enum KalturaPermissionOrderBy
    {
        NONE
    }

}