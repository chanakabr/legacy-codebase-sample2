using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Exceptions;
using WebAPI.Models.General;

namespace WebAPI.Models.Api
{
    public partial class KalturaPersonalListFilter : KalturaFilter<KalturaPersonalListOrderBy>
    {
        /// <summary>
        /// Comma separated list of partner list types to search within. 
        /// If omitted – all types should be included.
        /// </summary>
        [DataMember(Name = "partnerListTypeIn")]
        [JsonProperty("partnerListTypeIn")]
        [XmlElement(ElementName = "partnerListTypeIn", IsNullable = true)]
        public string PartnerListTypeIn { get; set; }

        public override KalturaPersonalListOrderBy GetDefaultOrderByValue()
        {
            return KalturaPersonalListOrderBy.CREATE_DATE_DESC;
        }
    }
}