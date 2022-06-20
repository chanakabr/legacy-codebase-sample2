using System.Collections.Generic;
using ApiObjects.SearchObjects;
using ApiObjects.SearchPriorityGroups;
using Core.Catalog.Request;
using GroupsCacheManager;

namespace ApiLogic.IndexManager.QueryBuilders.SearchPriority
{
    public interface IPriorityGroupsPreprocessor
    {
        IReadOnlyDictionary<double, IEsPriorityGroup> Preprocess(
            IReadOnlyDictionary<double, SearchPriorityGroup> priorityGroupsMappings,
            BaseRequest request,
            UnifiedSearchDefinitions definitions,
            Group group,
            int groupId);
    }
}
