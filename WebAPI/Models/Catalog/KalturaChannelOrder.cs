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
    public partial class KalturaChannelOrder : KalturaOTTObject
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
        /// Sliding window period in minutes, used only when ordering by LIKES_DESC / VOTES_DESC / RATINGS_DESC / VIEWS_DESC
        /// </summary>
        [DataMember(Name = "period")]
        [JsonProperty(PropertyName = "period")]
        [XmlElement(ElementName = "period", IsNullable = true)]
        [SchemeProperty(MinLong = 1)]
        public int? SlidingWindowPeriod { get; set; }

        internal void Validate(Type type)
        {
            if (DynamicOrderBy != null && orderBy.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaChannelOrder.dynamicOrderBy", "KalturaChannelOrder.orderBy");
            }            

            Type dynamicChannelType = typeof(KalturaDynamicChannel);
            if (dynamicChannelType.IsAssignableFrom(type) && orderBy.HasValue && orderBy.Value == KalturaChannelOrderBy.ORDER_NUM)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "KalturaChannelOrder.orderBy", "objectType");
            }

            Type manualChannelType = typeof(KalturaManualChannel);
            if (manualChannelType.IsAssignableFrom(type) && orderBy.HasValue && orderBy.Value == KalturaChannelOrderBy.RELEVANCY_DESC)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "KalturaChannelOrder.orderBy", "objectType");
            }

            if (SlidingWindowPeriod.HasValue && orderBy.HasValue && !SLIDING_WINDOW_ORDER_BY_OPTIONS.Contains((int)orderBy.Value))            
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "KalturaChannelOrder.period", "KalturaChannelOrder.orderBy");
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