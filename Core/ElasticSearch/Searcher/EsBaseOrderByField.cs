using ApiObjects.SearchObjects;
using Nest;

namespace ElasticSearch.Searcher
{
    public abstract class EsBaseOrderByField : IEsOrderByField
    {
        protected EsBaseOrderByField(OrderDir direction)
        {
            OrderByDirection = direction;
        }
        public OrderDir OrderByDirection { get; }
    }
}
