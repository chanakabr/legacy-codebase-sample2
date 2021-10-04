using System.Collections.Generic;
using System.Linq;
using ApiObjects;

namespace ApiLogic.Catalog.Tree
{
    public class FilterTreeResultProcessor : IFilterTreeResultProcessor
    {
        public IndexesModel ProcessResults(eCutType operand, IEnumerable<IndexesModel> results)
        {
            if (!results.Any())
            {
                return null;
            }
            
            var result = new IndexesModel();
            var filteredResults = results.Where(x => x != null).ToArray();
            // in case there is only 1 node with result, we don't care what operator is (or/and) 
            if (filteredResults.Length == 1)
            {
                return filteredResults.First();
            }
            
            var index = filteredResults.Aggregate(result.Indexes, (current, r) => current | r.Indexes);
            switch (operand)
            {
                case eCutType.Or:
                {
                    result.Indexes = index;
                    break;
                }
                case eCutType.And:
                {
                    if (index.HasFlag(ElasticSearchIndexes.Epg))
                    {
                        result.Indexes |= ElasticSearchIndexes.Epg;
                    }

                    if (index.HasFlag(ElasticSearchIndexes.Media))
                    {
                        result.Indexes |= ElasticSearchIndexes.Media;
                    }

                    if (result.Indexes == 0)
                    {
                        result.Indexes |= ElasticSearchIndexes.Common;
                    }

                    break;
                }
            }

            return result;
        }
    }
}
