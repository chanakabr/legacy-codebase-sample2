using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiLogic.IndexManager.QueryBuilders
{
    public class UnifiedSearchSortAndPagingDefinitions
    {
        public bool IsOrderedByStat { get; set; }
        public bool IsOrderedByString { get; set; }
        public bool ShouldSortByStartDateOfAssociationTagsAndParentMedia { get; internal set; }
        public int PageIndex { get; internal set; }
        public int PageSize { get; internal set; }
        public GroupByDefinition DistinctGroup { get; internal set; }
        public OrderBy OriginalOrderBy { get; internal set; }
    }
}
