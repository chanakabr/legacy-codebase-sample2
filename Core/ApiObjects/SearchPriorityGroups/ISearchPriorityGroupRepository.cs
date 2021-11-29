using System.Collections.Generic;
using ApiObjects.Response;

namespace ApiObjects.SearchPriorityGroups
{
    public interface ISearchPriorityGroupRepository
    {
        GenericResponse<SearchPriorityGroup> Add(long groupId, SearchPriorityGroup searchPriorityGroup, long updaterId);
        GenericResponse<SearchPriorityGroup> Update(long groupId, SearchPriorityGroup searchPriorityGroup);
        Status Delete(long groupId, long searchPriorityGroupId, long updaterId);
        GenericListResponse<SearchPriorityGroup> List(long groupId, IEnumerable<long> ids);
    }
}