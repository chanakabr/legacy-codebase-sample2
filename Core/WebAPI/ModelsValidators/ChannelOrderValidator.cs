using System;
using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Models.Catalog;

namespace WebAPI.ModelsValidators
{
    public static class ChannelOrderValidator
    {
        private static readonly HashSet<int> SLIDING_WINDOW_ORDER_BY_OPTIONS = new HashSet<int>()
            { (int)KalturaChannelOrderBy.LIKES_DESC, (int)KalturaChannelOrderBy.RATINGS_DESC, (int)KalturaChannelOrderBy.VIEWS_DESC, (int)KalturaChannelOrderBy.VOTES_DESC };
        
        public static void Validate(this KalturaChannelOrder model, Type type)
        {
            if (model.DynamicOrderBy != null && model.orderBy.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaChannelOrder.dynamicOrderBy", "KalturaChannelOrder.orderBy");
            }            

            Type dynamicChannelType = typeof(KalturaDynamicChannel);
            if (dynamicChannelType.IsAssignableFrom(type) && model.orderBy.HasValue && model.orderBy.Value == KalturaChannelOrderBy.ORDER_NUM)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "KalturaChannelOrder.orderBy", "objectType");
            }

            Type manualChannelType = typeof(KalturaManualChannel);
            if (manualChannelType.IsAssignableFrom(type) && model.orderBy.HasValue && model.orderBy.Value == KalturaChannelOrderBy.RELEVANCY_DESC)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "KalturaChannelOrder.orderBy", "objectType");
            }

            if (model.SlidingWindowPeriod.HasValue && model.orderBy.HasValue && !SLIDING_WINDOW_ORDER_BY_OPTIONS.Contains((int)model.orderBy.Value))            
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "KalturaChannelOrder.period", "KalturaChannelOrder.orderBy");
            }
        }
    }
}