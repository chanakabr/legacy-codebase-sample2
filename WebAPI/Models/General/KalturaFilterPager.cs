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
        private int pageSize;

        /// <summary>
        /// <![CDATA[The number of objects to retrieve. Possible range 1 ≤ value ≤ 50. If omitted or value < 1 - will be set to 25. If a value > 50 provided – will be set to 50]]>
        /// 
        /// </summary>
        [DataMember(Name = "pageSize")]
        [JsonProperty(PropertyName = "pageSize")]
        [XmlElement(ElementName = "pageSize")]
        public int PageSize
        {
            get 
            {
                if (pageSize > 50)
                {
                    return 50;
                }
                else if (pageSize < 1)
                {
                    return 25;
                }
                return pageSize;
            }

            set { pageSize = value; }
        }
        
        /// <summary>
        /// The page number for which {pageSize} of objects should be retrieved
        /// </summary>
        [DataMember(Name = "pageIndex")]
        [JsonProperty(PropertyName = "pageIndex")]
        [XmlElement(ElementName = "pageIndex")]
        public int PageIndex { get; set; }
    }
}