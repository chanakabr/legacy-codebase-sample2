using System;
using System.Collections.Generic;
using System.Threading;
using ApiObjects.SearchObjects;

namespace ApiLogic.IndexManager.Sorting
{
    public class RecommendationSortStrategy : IRecommendationSortStrategy
    {
        private static readonly Lazy<IRecommendationSortStrategy> LazyValue = new Lazy<IRecommendationSortStrategy>(() => new RecommendationSortStrategy(), LazyThreadSafetyMode.PublicationOnly);

        public static IRecommendationSortStrategy Instance => LazyValue.Value;

        public IEnumerable<long> Sort(IEnumerable<long> assetIds, UnifiedSearchDefinitions unifiedSearchDefinitions)
        {
            var orderedIds = new List<long>();
            var idsHashset = new HashSet<long>(assetIds);

            // Add all ordered ids from definitions first
            foreach (var asset in unifiedSearchDefinitions.specificOrder)
            {
                // If the id exists in search results
                if (idsHashset.Remove(asset.Value))
                {
                    // add to ordered list
                    orderedIds.Add(asset.Value);
                }
            }

            // Add all ids that are left
            orderedIds.AddRange(idsHashset);
            return orderedIds;
        }
    }
}
