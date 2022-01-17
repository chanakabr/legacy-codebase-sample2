using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using ApiObjects.SearchObjects;
using DAL.SearchObjects.Converters;
using Newtonsoft.Json;
using Phx.Lib.Log;

namespace GroupsCacheManager.Mappers
{
    public static class ChannelDataRowMapper
    {
        private static readonly KLogger Log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType?.ToString());
        
        public static List<AssetOrder> BuildOrderingParameters(DataRow dr)
        {
            List<AssetOrder> result;
            var orderingParametersJson = ODBCWrapper.Utils.GetSafeStr(dr["ORDERING_PARAMETERS"]);
            if (string.IsNullOrEmpty(orderingParametersJson))
            {
                var channelOrder = BuildBaseChannelOrder(dr);
                result = new List<AssetOrder> {channelOrder};
            }
            else
            {
                result = JsonConvert.DeserializeObject<List<AssetOrder>>(orderingParametersJson,
                    new AssetOrderConverter());
            }

            return result;
        }

        private static AssetOrder BuildBaseChannelOrder(DataRow dr)
        {
            var orderByInt = ODBCWrapper.Utils.GetIntSafeVal(dr["order_by_type"]);
            var orderDirInt = ODBCWrapper.Utils.GetIntSafeVal(dr["order_by_dir"]) - 1;
            var orderBy = (OrderBy) Enum.ToObject(typeof(OrderBy), orderByInt);
            var orderDir = (OrderDir) Enum.ToObject(typeof(OrderDir), orderDirInt);

            AssetOrder channelOrder;
            switch (orderBy)
            {
                case OrderBy.NAME:
                case OrderBy.START_DATE:
                case OrderBy.CREATE_DATE:
                case OrderBy.RELATED:
                case OrderBy.ID:
                    channelOrder = new AssetOrder {Field = orderBy, Direction = orderDir};
                    break;
                case OrderBy.META:
                    var orderByValue = ODBCWrapper.Utils.GetSafeStr(dr, "ORDER_BY_VALUE");
                    channelOrder = new AssetOrderByMeta
                        {Field = orderBy, Direction = orderDir, MetaName = orderByValue};
                    break;
                case OrderBy.LIKE_COUNTER:
                case OrderBy.RATING:
                case OrderBy.VOTES_COUNT:
                case OrderBy.VIEWS:
                    var slidingWindowPeriod = ODBCWrapper.Utils.GetIntSafeVal(dr["SlidingWindowPeriod"]);
                    channelOrder = new AssetSlidingWindowOrder
                        {Field = orderBy, Direction = orderDir, SlidingWindowPeriod = slidingWindowPeriod};
                    break;
                default:
                    channelOrder = new AssetOrder {Field = OrderBy.CREATE_DATE, Direction = OrderDir.DESC};
                    Log.Warn(
                        $"{nameof(AssetOrder)} can not be determined: {nameof(orderBy)}={orderBy}. The default channel order has been created.");
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