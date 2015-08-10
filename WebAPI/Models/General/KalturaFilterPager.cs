using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace WebAPI.Models.General
{
    /// <summary>
    /// The KalturaFilterPager object enables paging management to be applied upon service list actions
    /// </summary>
    public class KalturaFilterPager : KalturaOTTObject
    {
        /// <summary>
        /// The number of objects to retrieve
        /// </summary>
        [DataMember(Name = "pageSize")]
        [JsonProperty(PropertyName = "pageSize")]
        [XmlElement(ElementName = "pageSize")]
        public int PageSize { get; set; }

        /// <summary>
        /// The page number for which {pageSize} of objects should be retrieved
        /// </summary>
        [DataMember(Name = "pageIndex")]
        [JsonProperty(PropertyName = "pageIndex")]
        [XmlElement(ElementName = "pageIndex")]
        public int PageIndex { get; set; }
    }
}