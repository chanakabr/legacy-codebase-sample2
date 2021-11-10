using ApiObjects.Response;

namespace ApiObjects.SearchPriorityGroups
{
    public interface ISearchPriorityGroupOrderedListRepository
    {
        GenericResponse<SearchPriorityGroupOrderedIdsSet> Get(long groupId);
        GenericResponse<SearchPriorityGroupOrderedIdsSet> Update(long groupId, SearchPriorityGroupOrderedIdsSet orderedList);
    }
}