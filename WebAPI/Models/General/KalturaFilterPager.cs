using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;

namespace WebAPI.Models.General
{
    /// <summary>
    /// The KalturaFilterPager object enables paging management to be applied upon service list actions
    /// </summary>
    public class KalturaFilterPager : KalturaOTTObject
    {
        private static int maxPageSize;
        private const int DEFAULT_PAGE_SIZE = 30;
        private const int DEFAULT_PAGE_INDEX = 1;
        private int pageSize;
        private int pageIndex;

        /// <summary>
        /// <![CDATA[The number of objects to retrieve. Possible range 1 ≤ value ≤ 50. If omitted or value < 1 - will be set to 25. If a value > 50 provided – will be set to 50]]>
        /// 
        /// </summary>
        [DataMember(Name = "pageSize")]
        [JsonProperty(PropertyName = "pageSize")]
        [XmlElement(ElementName = "pageSize")]
        public int? PageSize
        {
            get 
            {
                return pageSize > 0 ? pageSize : maxPageSize;
            }

            set
            {
                if (!value.HasValue)
                {
                    pageIndex = DEFAULT_PAGE_SIZE;
                    return;
                }

                if (value > KalturaFilterPager.maxPageSize)
                {
                    pageSize = KalturaFilterPager.maxPageSize;
                }
                else if (value < 1)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED, "KalturaFilterPager.pageSize", "1");
                }
                else
                {
                    pageSize = (int)value;
                }
            }
        }
        
        /// <summary>
        /// The page number for which {pageSize} of objects should be retrieved
        /// </summary>
        [DataMember(Name = "pageIndex")]
        [JsonProperty(PropertyName = "pageIndex")]
        [XmlElement(ElementName = "pageIndex")]
        public int? PageIndex
        {
            get
            {
                return getPageIndex();
            }

            set
            {
                if (!value.HasValue)
                {
                    pageIndex = DEFAULT_PAGE_INDEX;
                    return;
                }
                
                if (value <= 0)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED, "KalturaFilterPager.pageIndex", "1");
                }
                pageIndex = (int)value;
            }
        }

        public KalturaFilterPager()
        {            
            if (KalturaFilterPager.maxPageSize == 0)
            {
                KalturaFilterPager.maxPageSize = TCMClient.Settings.Instance.GetValue<int>("max_page_size");

                if (KalturaFilterPager.maxPageSize == 0)
                    KalturaFilterPager.maxPageSize = DEFAULT_PAGE_SIZE;
            }

            pageSize = DEFAULT_PAGE_SIZE;
            pageIndex = DEFAULT_PAGE_INDEX;
        }

        internal int getPageIndex()
        {
            return pageIndex - 1;
        }

        internal int getPageSize()
        {
            return pageSize;
        }
    }
}