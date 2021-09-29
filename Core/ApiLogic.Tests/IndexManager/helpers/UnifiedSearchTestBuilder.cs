using System.Collections.Generic;
using ApiObjects;
using ApiObjects.SearchObjects;

namespace ApiLogic.Tests.IndexManager.helpers
{
    public static class UnifiedSearchTestsBuilder
    {
        public static UnifiedSearchDefinitions WithPageIndex(this UnifiedSearchDefinitions us, int pageIndex)
        {
            us.pageIndex = pageIndex;
            return us;
        }

        public static UnifiedSearchDefinitions WithGroupId(this UnifiedSearchDefinitions us, int id)
        {
            us.groupId = id;
            return us;
        }

        public static UnifiedSearchDefinitions WithSpecificAssets(this UnifiedSearchDefinitions us, Dictionary<ApiObjects.eAssetTypes, List<string>> data)
        {
            us.specificAssets = data;
            return us;
        }

        public static UnifiedSearchDefinitions ShouldSearchMedia(this UnifiedSearchDefinitions us)
        {
            us.shouldSearchMedia = true;
            return us;
        }
        public static UnifiedSearchDefinitions ShouldSearchEpg(this UnifiedSearchDefinitions us)
        {
            us.shouldSearchEpg = true;
            return us;
        }

        public static UnifiedSearchDefinitions WithPageSize(this UnifiedSearchDefinitions us, int data)
        {
            us.pageSize = data;
            return us;
        }

        public static UnifiedSearchDefinitions WithLanguage(this UnifiedSearchDefinitions us, LanguageObj data)
        {
            us.langauge = data;
            return us;
        }

        public static UnifiedSearchDefinitions WithGroupByOrder(this UnifiedSearchDefinitions us, AggregationOrder? data)
        {
            us.groupByOrder = data;
            return us;
        }
        public static UnifiedSearchDefinitions WithOrder(this UnifiedSearchDefinitions us, OrderObj data)
        {
            us.order = data;
            return us;
        }
        public static UnifiedSearchDefinitions WithDistinctGroup(this UnifiedSearchDefinitions us, GroupByDefinition groupBy)
        {
            us.distinctGroup = groupBy;
            return us;
        }
    }
}