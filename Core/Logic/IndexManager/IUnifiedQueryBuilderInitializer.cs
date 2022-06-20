namespace ApiLogic.IndexManager
{
    public interface IUnifiedQueryBuilderInitializer
    {
        void SetPagingForUnifiedSearch(IUnifiedQueryBuilder queryBuilder);
    }
}