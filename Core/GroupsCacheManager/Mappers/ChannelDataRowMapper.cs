using System;
using System.Collections.Generic;
using System.Data;
using ApiObjects.Catalog;
using ApiObjects.SearchObjects;
using Newtonsoft.Json;
using Phx.Lib.Log;

namespace GroupsCacheManager.Mappers
{
    public static class ChannelDataRowMapper
    {
        private static readonly KLogger Log = new KLogger(nameof(ChannelDataRowMapper));

        public static List<AssetOrder> BuildOrderingParameters(DataRow dr)
        {
            List<AssetOrder> result;
            var orderingParametersJson = ODBCWrapper.Utils.GetSafeStr(dr["ORDERING_PARAMETERS"]);
            if (string.IsNullOrEmpty(orderingParametersJson))
            {
                var channelOrder = BuildBaseChannelOrder(dr);
                result = new List<AssetOrder> { channelOrder };
            }
            else
            {
                result = JsonConvert.DeserializeObject<List<AssetOrder>>(orderingParametersJson);
            }

            return result;
        }

        private static AssetOrder BuildBaseChannelOrder(DataRow dr)
        {
            var orderDirInt = ODBCWrapper.Utils.GetIntSafeVal(dr["order_by_dir"]) - 1;
            var orderDir = (OrderDir)Enum.ToObject(typeof(OrderDir), orderDirInt);

            var orderByValue = ODBCWrapper.Utils.GetSafeStr(dr, "ORDER_BY_VALUE");

            var orderByInt = ODBCWrapper.Utils.GetIntSafeVal(dr["order_by_type"]);
            var orderBy = string.IsNullOrEmpty(orderByValue)
                ? (OrderBy)Enum.ToObject(typeof(OrderBy), orderByInt)
                : OrderBy.META;

            switch (orderBy)
            {
                case OrderBy.NAME:
                case OrderBy.START_DATE:
                case OrderBy.CREATE_DATE:
                case OrderBy.RELATED:
                case OrderBy.ID:
                    return new AssetOrder { Field = orderBy, Direction = orderDir };
                case OrderBy.META:
                    return new AssetOrderByMeta { Field = orderBy, Direction = orderDir, MetaName = orderByValue };
                case OrderBy.LIKE_COUNTER:
                case OrderBy.RATING:
                case OrderBy.VOTES_COUNT:
                case OrderBy.VIEWS:
                    var isSlidingWindow = ODBCWrapper.Utils.GetIntSafeVal(dr["IsSlidingWindow"]) == 1;
                    var slidingWindowPeriod = 0;
                    if (isSlidingWindow)
                    {
                        slidingWindowPeriod = ODBCWrapper.Utils.GetIntSafeVal(dr["SlidingWindowPeriod"]);
                    }

                    return new AssetSlidingWindowOrder { Field = orderBy, Direction = orderDir, SlidingWindowPeriod = slidingWindowPeriod };
                default:
                    if (orderByInt >= (int)MetasEnum.META1_STR && orderByInt <= (int)MetasEnum.META10_DOUBLE)
                    {
                        // TVM account. MetaName will be updated by ChannelRepository.
                        return new AssetOrderByMeta { Field = OrderBy.META, Direction = orderDir };
                    }

                    Log.Warn($"{nameof(AssetOrder)} can not be determined: {nameof(orderBy)}={orderBy}. The default channel order has been created.");
                    return new AssetOrder { Field = OrderBy.CREATE_DATE, Direction = OrderDir.DESC };
            }
        }

        public static OrderObj BuildOrderObj(AssetOrder channelOrder)
        {
            var orderObj = new OrderObj
            {
                m_eOrderBy = channelOrder.Field,
                m_eOrderDir = channelOrder.Direction
            };

            if (channelOrder is AssetOrderByMeta orderByMeta)
            {
                orderObj.m_sOrderValue = orderByMeta.MetaName;
            }
            else if (channelOrder is AssetSlidingWindowOrder slidingWindowOrder)
            {
                orderObj.m_bIsSlidingWindowField = orderObj.isSlidingWindowFromRestApi = true;
                orderObj.lu_min_period_id = slidingWindowOrder.SlidingWindowPeriod;
            }

            return orderObj;
        }
    }
}