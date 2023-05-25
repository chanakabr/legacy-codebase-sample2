using System;
using System.Collections.Generic;
using System.Linq;
using WebAPI.Exceptions;
using WebAPI.Models.Catalog;
using WebAPI.ObjectsConvertor.Ordering;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class AssetFilterMapper
    {
        public static List<string> getGroupByValue(this KalturaBaseSearchAssetFilter model)
        {
            if (model.GroupBy == null || model.GroupBy.Count == 0)
                return null;

            List<string> values = model.GroupBy.Select(x => x.GetValue()).ToList();

            return values;
        }

        public static IReadOnlyCollection<KalturaBaseAssetOrder> GetOrderings(this KalturaAssetFilter filter)
        {
            if (filter.OrderParameters != null)
            {
                return filter.OrderParameters.Any()
                    ? filter.OrderParameters
                    : KalturaOrderAdapter.Instance.MapToOrderingList(filter.GetDefaultOrderByValue());
            }

            if (filter.DynamicOrderBy?.OrderBy != null)
            {
                return KalturaOrderAdapter.Instance.MapToOrderingList(filter.DynamicOrderBy, filter.GetDefaultOrderByValue());
            }

            var orderByValue = filter.OrderBy ?? filter.GetDefaultOrderByValue();

            return KalturaOrderAdapter.Instance.MapToOrderingList(orderByValue, filter.TrendingDaysEqual);
        }

        public static IReadOnlyCollection<KalturaBaseAssetOrder> GetOrderings(this KalturaChannelFilter filter)
        {
            return filter.DynamicOrderBy == null && !filter.OrderBy.HasValue && (filter.OrderParameters == null || filter.OrderParameters.Count == 0)
                ? null
                : (filter as KalturaAssetFilter).GetOrderings();
        }

        public static List<int> getTypeIn(this KalturaSearchExternalFilter filter)
        {
            if (string.IsNullOrEmpty(filter.TypeIn))
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaSearchExternalFilter.typeIn");

            List<int> values = new List<int>();
            string[] stringValues = filter.TypeIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string stringValue in stringValues)
            {
                int value;
                if (int.TryParse(stringValue, out value))
                {
                    values.Add(value);
                }
                else
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaSearchExternalFilter.typeIn");
                }
            }

            return values;
        }

        public static List<long> ConvertChannelsIn(this KalturaScheduledRecordingProgramFilter filter)
            => WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(filter.ChannelsIn, "KalturaScheduledRecordingProgramFilter.ChannelsIn");

        public static List<string> ConvertSeriesIdsIn(this KalturaScheduledRecordingProgramFilter filter)
            => WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<string>, string>(filter.SeriesIdsIn, "KalturaScheduledRecordingProgramFilter.SeriesIdsIn");

        public static List<int> getTypeIn(this KalturaRelatedExternalFilter filter)
        {
            return Utils.Utils.ParseCommaSeparatedValues<List<int>, int>(filter.TypeIn, "KalturaRelatedExternalFilter.typeIn");
        }

        public static List<int> getTypeIn(this KalturaBundleFilter filter) => 
            WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<int>, int>(filter.TypeIn, "KalturaBundleFilter.typeIn");

        public static HashSet<int> GetPartnerListTypeIn(this KalturaPersonalListSearchFilter filter)
            => WebAPI.Utils.Utils.ParseCommaSeparatedValues<HashSet<int>, int>(filter.PartnerListTypeIn, "KalturaPersonalListSearchFilter.PartnerListTypeIn", false, true);

        public static List<int> getTypeIn(this KalturaSearchAssetFilter filter)
        {
            if (string.IsNullOrEmpty(filter.TypeIn))
                return null;

            List<int> values = new List<int>();
            string[] stringValues = filter.TypeIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string stringValue in stringValues)
            {
                int value;
                if (int.TryParse(stringValue, out value))
                {
                    values.Add(value);
                }
                else
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaSearchAssetFilter.typeIn");
                }
            }

            return values;
        }

        public static List<int> getEpgChannelIdIn(this KalturaSearchAssetFilter filter)
        {
            if (string.IsNullOrEmpty(filter.IdIn))
                return null;

            List<int> values = new List<int>();
            string[] stringValues = filter.IdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string stringValue in stringValues)
            {
                int value;
                if (int.TryParse(stringValue, out value))
                {
                    values.Add(value);
                }
                else
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaSearchAssetFilter.idIn");
                }
            }

            return values;
        }

        public static List<int> GetTypeIn(this KalturaRelatedFilter filter) =>
            WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<int>, int>(filter.TypeIn, "KalturaRelatedFilter.typeIn");
    }
}
