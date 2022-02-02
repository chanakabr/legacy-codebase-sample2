using System;
using ApiObjects.SearchObjects;

namespace ElasticSearch.Searcher
{
    public class EsOrderByStatisticsField : EsBaseOrderByField
    {
        public EsOrderByStatisticsField(OrderBy orderBy, OrderDir direction, DateTime? trendingAssetWindow)
            : base(direction)
        {
            OrderByField = orderBy;
            TrendingAssetWindow = trendingAssetWindow;
        }

        public OrderBy OrderByField { get; }

        public DateTime? TrendingAssetWindow { get; }
    }
}
