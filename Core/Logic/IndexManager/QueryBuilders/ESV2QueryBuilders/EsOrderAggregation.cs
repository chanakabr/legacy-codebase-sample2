using ElasticSearch.Searcher;

namespace ApiLogic.IndexManager.QueryBuilders.ESV2QueryBuilders
{
    public class EsOrderAggregation
    {
        public string DistinctOrder { get; set; }

        public string DistinctDirection { get; set; }

        public ESBaseAggsItem OrderAggregation { get; set; }
    }
}