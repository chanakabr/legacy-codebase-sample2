using System.Collections.Generic;
using ApiLogic.IndexManager.Sorting;
using ApiObjects.SearchObjects;
using ElasticSearch.Utils;

namespace ApiLogic.IndexManager.QueryBuilders
{
    public abstract class BaseEsUnifiedQueryBuilder : IUnifiedQueryBuilder
    {
        protected readonly IEsSortingService EsSortingService;
        protected readonly ISortingAdapter SortingAdapter;
        protected readonly IUnifiedQueryBuilderInitializer QueryInitializer;
        protected IReadOnlyCollection<IEsOrderByField> OrderByFields => SortingAdapter.ResolveOrdering(SearchDefinitions);

        protected BaseEsUnifiedQueryBuilder(
            IEsSortingService esSortingService,
            ISortingAdapter sortingAdapter,
            IUnifiedQueryBuilderInitializer queryInitializer)
        {
            EsSortingService = esSortingService;
            SortingAdapter = sortingAdapter;
            QueryInitializer = queryInitializer;
        }

        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public bool GetAllDocuments { get; set; }
        public bool ShouldPageGroups { get; set; }
        public int From { get; set; }
        public UnifiedSearchDefinitions SearchDefinitions { get; set; }

        public abstract void SetPagingForUnifiedSearch();
        public abstract void SetGroupByValuesForUnifiedSearch();
    }
}