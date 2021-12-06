using ApiObjects.SearchPriorityGroups;

namespace ApiLogic.Catalog.CatalogManagement.Validators
{
    public interface ISearchPriorityGroupValidator
    {
        bool ValidateSearchPriorityGroup(long groupId, SearchPriorityGroup searchPriorityGroup, out string message);
    }
}