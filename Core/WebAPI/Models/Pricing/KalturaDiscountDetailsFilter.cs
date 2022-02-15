using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    public partial class KalturaDiscountDetailsFilter : KalturaFilter<KalturaDiscountFilterOrderBy>
    {
        /// <summary>
        /// Comma separated discount codes
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn")]
        [SchemeProperty(DynamicMinInt = 1)]
        public string IdIn { get; set; }
        
        public override KalturaDiscountFilterOrderBy GetDefaultOrderByValue()
        {
            return KalturaDiscountFilterOrderBy.CODE_ASC;
        }
    }
}