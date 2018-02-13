using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Channel order details
    /// </summary>
    public class KalturaChannelOrder : KalturaOTTObject
    {

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
        public KalturaChannelOrderBy orderBy { get; set; }

        internal void Validate(string objectType)
        {
            if (DynamicOrderBy != null && orderBy != null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaChannelOrder.dynamicOrderBy", "KalturaChannelOrder.orderBy");
            }

            if (objectType == KalturaChannel.DYNAMIC_CHANNEL && orderBy == KalturaChannelOrderBy.ORDER_NUM)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "KalturaChannelOrder.orderBy", "objectType");
            }

            if (objectType == KalturaChannel.MANUAL_CHANNEL && orderBy == KalturaChannelOrderBy.RELEVANCY_DESC)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "KalturaChannelOrder.orderBy", "objectType");
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