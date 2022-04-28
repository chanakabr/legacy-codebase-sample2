using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.General
{
    /// <summary>
    /// The KalturaFilterPager object enables paging management to be applied upon service list actions
    /// </summary>
    public partial class KalturaFilterPager : KalturaOTTObject
    {
        private const int MAX_PAGE_SIZE = 500;
        private int pageSize;
        private int pageIndex;

        /// <summary>
        /// <![CDATA[The number of objects to retrieve. Possible range 1 ≤ value ≤ 50. If omitted or value < 1 - will be set to 25. If a value > 50 provided – will be set to 50]]>
        /// </summary>
        [DataMember(Name = "pageSize")]
        [JsonProperty(PropertyName = "pageSize")]
        [XmlElement(ElementName = "pageSize")]
        [SchemeProperty(IsNullable = true, MinInteger = 1, Default = 30)]
        public int? PageSize
        {
            get
            {
                return pageSize > 0 ? pageSize : MAX_PAGE_SIZE;
            }

            set
            {
                if (!value.HasValue)
                {
                    pageSize = 30;
                }
                else if (value > MAX_PAGE_SIZE)
                {
                    pageSize = MAX_PAGE_SIZE;
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
        [SchemeProperty(IsNullable = true, MinInteger = 1, Default = 1)]
        public int? PageIndex
        {
            get
            {
                return GetPageIndex();
            }

            set
            {
                if (!value.HasValue)
                {
                    pageIndex = 1;
                    return;
                }

                if (value <= 0)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED, "KalturaFilterPager.pageIndex", "1");
                }
                pageIndex = (int)value;
            }
        }

        internal int GetPageIndex()
        {
            return this.pageIndex - 1;
        }

        protected override void Init()
        {
            base.Init();
            pageSize = 30;
            pageIndex = 1;
        }
    }
}
