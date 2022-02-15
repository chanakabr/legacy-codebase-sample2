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

            AssetOrder channelOrder;
            switch (orderBy)
            {
                case OrderBy.NAME:
                case OrderBy.START_DATE:
                case OrderBy.CREATE_DATE:
                case OrderBy.RELATED:
                case OrderBy.ID:
                    channelOrder = new AssetOrder { Field = orderBy, Direction = orderDir };
                    break;
                case OrderBy.META:
                    channelOrder = new AssetOrderByMeta { Field = orderBy, Direction = orderDir, MetaName = orderByValue };
                    break;
                case OrderBy.LIKE_COUNTER:
                case OrderBy.RATING:
                case OrderBy.VOTES_COUNT:
                case OrderBy.VIEWS:
                    var slidingWindowPeriod = ODBCWrapper.Utils.GetIntSafeVal(dr["SlidingWindowPeriod"]);
                    channelOrder = new AssetSlidingWindowOrder { Field = orderBy, Direction = orderDir, SlidingWindowPeriod = slidingWindowPeriod };
                    break;
                default:
                    if (orderByInt >= (int)MetasEnum.META1_STR && orderByInt <= (int)MetasEnum.META10_DOUBLE)
                    {
                        // TVM account. MetaName will be updated by ChannelRepository.
                        channelOrder = new AssetOrderByMeta { Field = OrderBy.META, Direction = orderDir };
                    }
                    else
                    {
                        Log.Warn($"{nameof(AssetOrder)} can not be determined: {nameof(orderBy)}={orderBy}. The default channel order has been created.");
                        channelOrder = new AssetOrder { Field = OrderBy.CREATE_DATE, Direction = OrderDir.DESC };
                    }

                    break;
            }

            return channelOrder;
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