using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Models.Partner;

namespace WebAPI.Models.Partner
{
    public enum KalturaPartnerConfigurationOrderBy
    {
        NONE
    }

    /// <summary>
    /// Partner configuration filter 
    /// </summary>
    public class KalturaPartnerConfigurationFilter : KalturaFilter<KalturaPartnerConfigurationOrderBy>
    {
        /// <summary>
        /// Indicates which partner configuration list to return
        /// </summary>
        [DataMember(Name = "partnerConfigurationType")]
        [JsonProperty("partnerConfigurationType")]
        [XmlElement(ElementName = "partnerConfigurationType")]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public KalturaPartnerConfigurationType partnerConfigurationType { get; set; }
        
        public override KalturaPartnerConfigurationOrderBy GetDefaultOrderByValue()
        {
            return KalturaPartnerConfigurationOrderBy.NONE;
        }
    }
}