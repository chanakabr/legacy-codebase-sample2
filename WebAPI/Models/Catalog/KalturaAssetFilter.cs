using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public class KalturaAssetFilter : KalturaPersistedFilter<KalturaAssetOrderBy>
    {
        public override KalturaAssetOrderBy GetDefaultOrderByValue()
        {
            return KalturaAssetOrderBy.RELEVANCY_DESC;
        }

        internal virtual void Validate()
        {   
        }

        /// <summary>
        /// dynamicOrderBy - order by Meta
        /// </summary>
        [DataMember(Name = "dynamicOrderBy")]
        [JsonProperty("dynamicOrderBy")]
        [XmlElement(ElementName = "dynamicOrderBy", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public KalturaDynamicOrderBy DynamicOrderBy { get; set; }
      
    }
}



