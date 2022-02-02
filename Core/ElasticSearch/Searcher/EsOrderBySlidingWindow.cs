using ApiObjects.SearchObjects;

namespace ElasticSearch.Searcher
{
    public class EsOrderBySlidingWindow : EsBaseOrderByField
    {
        public EsOrderBySlidingWindow(OrderBy field, OrderDir direction, int slidingWindowPeriod)
            : base(direction)
        {
            OrderByField = field;
            SlidingWindowPeriod = slidingWindowPeriod;
        }

        public OrderBy OrderByField { get; }

        public int SlidingWindowPeriod { get; }
    }
}