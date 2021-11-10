using System.Collections.Generic;
using ApiObjects.Response;
using ApiObjects.SearchPriorityGroups;

namespace Core.Catalog.CatalogManagement
{
    public interface ISearchPriorityGroupManager
    {
        GenericResponse<SearchPriorityGroup> AddSearchPriorityGroup(long groupId, SearchPriorityGroup searchPriorityGroup, long userId);
        GenericResponse<SearchPriorityGroup> UpdateSearchPriorityGroup(long groupId, SearchPriorityGroup searchPriorityGroup);
        Status DeleteSearchPriorityGroup(long groupId, long searchPriorityGroupId, long userId);
        GenericListResponse<SearchPriorityGroup> ListSearchPriorityGroups(long groupId, SearchPriorityGroupQuery query);
        GenericResponse<SearchPriorityGroupOrderedIdsSet> SetKalturaSearchPriorityGroupOrderedList(long groupId, SearchPriorityGroupOrderedIdsSet orderedList);
        GenericResponse<SearchPriorityGroupOrderedIdsSet> GetKalturaSearchPriorityGroupOrderedList(long groupId);
        IReadOnlyDictionary<double, SearchPriorityGroup> ListSearchPriorityGroupMappings(int groupId);
    }
}
