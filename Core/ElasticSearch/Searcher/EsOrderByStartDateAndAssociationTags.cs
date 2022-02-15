using ApiObjects.SearchObjects;

namespace ElasticSearch.Searcher
{
    public class EsOrderByStartDateAndAssociationTags : EsBaseOrderByField
    {
        public EsOrderByStartDateAndAssociationTags(OrderDir direction) : base(direction)
        {
        }
    }
}