using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiObjects.SearchObjects;

namespace ApiLogic.IndexManager.Sorting
{
    public class RecommendationSortStrategy : IRecommendationSortStrategy
    {
        private static readonly Lazy<IRecommendationSortStrategy> LazyValue = new Lazy<IRecommendationSortStrategy>(
            () => new RecommendationSortStrategy(),
            LazyThreadSafetyMode.PublicationOnly);

        public static IRecommendationSortStrategy Instance => LazyValue.Value;

        public IEnumerable<(long id, string sortValue)> Sort(IEnumerable<long> assetIds, UnifiedSearchDefinitions unifiedSearchDefinitions)
        {
            var idsHashset = new HashSet<long>(assetIds);

            // Add all ordered ids from definitions first
            var increment = 0;
            var orderedIds =
                (from assetId in unifiedSearchDefinitions.specificOrder
                where idsHashset.Remove(assetId)
                select (assetId, (++increment).ToString()))
                .ToList();

            // Add all ids that are left;
            orderedIds.AddRange(idsHashset.Select(x => (x, (++increment).ToString())));
            return orderedIds;
        }
    }
}
