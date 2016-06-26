using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public class KalturaAssetFilter : KalturaSearchFilter
    {
        /// <summary>
        /// For related media - the ID of the asset for which to return related assets
        /// </summary>
        [DataMember(Name = "relatedMediaIdEqual")]
        [JsonProperty("relatedMediaIdEqual")]
        [XmlElement(ElementName = "relatedMediaIdEqual", IsNullable = true)]
        public string RelatedMediaIdEqual { get; set; }
    }
}



