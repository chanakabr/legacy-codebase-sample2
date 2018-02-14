using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Channel order details
    /// </summary>
    public class KalturaChannelOrder : KalturaOTTObject
    {

        private readonly HashSet<int> SLIDING_WINDOW_ORDER_BY_OPTIONS = new HashSet<int>()
        { (int)KalturaChannelOrderBy.LIKES_DESC, (int)KalturaChannelOrderBy.RATINGS_DESC, (int)KalturaChannelOrderBy.VIEWS_DESC, (int)KalturaChannelOrderBy.VOTES_DESC };

        /// <summary>
        /// Channel dynamic order by (meta)
        /// </summary>
        [DataMember(Name = "dynamicOrderBy")]
        [JsonProperty("dynamicOrderBy")]
        [XmlElement(ElementName = "dynamicOrderBy", IsNullable = true)]        
        public KalturaDynamicOrderBy DynamicOrderBy { get; set; }

        /// <summary>
        /// Channel order by
        /// </summary>
        [DataMember(Name = "orderBy")]
        [JsonProperty("orderBy")]
        [XmlElement(ElementName = "orderBy", IsNullable = true)]
        public KalturaChannelOrderBy? orderBy { get; set; }

        /// <summary>
        /// Sliding window period in minutes
        /// </summary>
        [DataMember(Name = "slidingWindowPeriod")]
        [JsonProperty(PropertyName = "slidingWindowPeriod")]
        [XmlElement(ElementName = "slidingWindowPeriod", IsNullable = true)]
        [SchemeProperty(MinLong = 1)]
        public int? SlidingWindowPeriod { get; set; }

        internal void Validate(string objectType)
        {
            if (DynamicOrderBy != null && orderBy.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaChannelOrder.dynamicOrderBy", "KalturaChannelOrder.orderBy");
            }

            if (objectType == KalturaChannel.DYNAMIC_CHANNEL && orderBy.HasValue && orderBy.Value == KalturaChannelOrderBy.ORDER_NUM)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "KalturaChannelOrder.orderBy", "objectType");
            }

            if (objectType == KalturaChannel.MANUAL_CHANNEL && orderBy.HasValue && orderBy.Value == KalturaChannelOrderBy.RELEVANCY_DESC)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "KalturaChannelOrder.orderBy", "objectType");
            }

            if (SlidingWindowPeriod.HasValue && orderBy.HasValue && !SLIDING_WINDOW_ORDER_BY_OPTIONS.Contains((int)orderBy.Value))            
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "KalturaChannelOrder.slidingWindowPeriod", "KalturaChannelOrder.orderBy");
            }
        }

    }

    public enum KalturaChannelOrderBy
    {
        ORDER_NUM,
        RELEVANCY_DESC,
        NAME_ASC,
        NAME_DESC,
        VIEWS_DESC,
        RATINGS_DESC,
        VOTES_DESC,
        START_DATE_DESC,
        START_DATE_ASC,
        LIKES_DESC,
        CREATE_DATE_ASC,
        CREATE_DATE_DESC
    }
}