using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Language filter
    /// </summary>
    public partial class KalturaLanguageFilter : KalturaFilter<KalturaLanguageOrderBy>
    {

        /// <summary>
        /// Language codes
        /// </summary>
        [DataMember(Name = "codeIn")]
        [JsonProperty("codeIn")]
        [XmlElement(ElementName = "codeIn", IsNullable = true)]
        public string CodeIn { get; set; }

        /// <summary>
        /// Exclude partner
        /// </summary>
        [DataMember(Name = "excludePartner")]
        [JsonProperty("excludePartner")]
        [XmlElement(ElementName = "excludePartner", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public bool? ExcludePartner { get; set; }

        public override KalturaLanguageOrderBy GetDefaultOrderByValue()
        {
            return KalturaLanguageOrderBy.SYSTEM_NAME_ASC;
        }
    }
}