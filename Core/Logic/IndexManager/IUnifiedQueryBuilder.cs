using ApiObjects.SearchObjects;

namespace ApiLogic.IndexManager
{
    public interface IUnifiedQueryBuilder
    {
        int PageSize { get; set; }
        int PageIndex { get; set; }
        bool GetAllDocuments { get; set; }
        bool ShouldPageGroups { get; set; }
        int From { get; set; }
        UnifiedSearchDefinitions SearchDefinitions { get; set; }
        void SetPagingForUnifiedSearch();
        void SetGroupByValuesForUnifiedSearch();
    }
}