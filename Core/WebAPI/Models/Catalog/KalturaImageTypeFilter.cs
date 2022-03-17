using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaImageTypeFilter : KalturaFilter<KalturaImageTypeOrderBy>
    {
        /// <summary>
        /// IDs to filter by
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn")]
        [SchemeProperty(DynamicMinInt = 1)]
        public string IdIn { get; set; }

        /// <summary>
        /// Ratio IDs to filter by
        /// </summary>
        [DataMember(Name = "ratioIdIn")]
        [JsonProperty("ratioIdIn")]
        [XmlElement(ElementName = "ratioIdIn")]
        [SchemeProperty(DynamicMinInt = 1)]
        public string RatioIdIn { get; set; }

        public override KalturaImageTypeOrderBy GetDefaultOrderByValue()
        {
            return KalturaImageTypeOrderBy.NONE;
        }
    }
}