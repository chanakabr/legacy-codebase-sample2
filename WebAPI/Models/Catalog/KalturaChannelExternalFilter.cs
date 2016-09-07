using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Catalog
{
    public class KalturaChannelExternalFilter : KalturaAssetFilter
    {
        /// <summary>
        ///External Channel Id. 
        /// </summary>
        [DataMember(Name = "idEqual")]
        [JsonProperty("idEqual")]
        [XmlElement(ElementName = "idEqual", IsNullable = true)]
        [SchemeProperty(MinInteger = 1)]
        public int IdEqual { get; set; }
        
        /// <summary>
        /// UtcOffsetEqual 
        /// </summary>
        [DataMember(Name = "utcOffsetEqual")]
        [JsonProperty("utcOffsetEqual")]
        [XmlElement(ElementName = "utcOffsetEqual", IsNullable = true)]
        [SchemeProperty(MinFloat = -12, MaxFloat = 12)]
        public float UtcOffsetEqual { get; set; }

        /// <summary>
        ///FreeTextEqual
        /// </summary>
        [DataMember(Name = "freeText")]
        [JsonProperty("freeText")]
        [XmlElement(ElementName = "freeText", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string FreeText { get; set; }
    }
}