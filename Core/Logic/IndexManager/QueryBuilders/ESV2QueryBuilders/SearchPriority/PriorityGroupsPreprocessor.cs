using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using ApiLogic.IndexManager.QueryBuilders.ESV2QueryBuilders.SearchPriority.Models;
using ApiObjects.SearchObjects;
using ApiObjects.SearchPriorityGroups;
using Core.Catalog;
using Core.Catalog.Request;
using GroupsCacheManager;

namespace ApiLogic.IndexManager.QueryBuilders.ESV2QueryBuilders.SearchPriority
{
    public class PriorityGroupsPreprocessor : IPriorityGroupsPreprocessor
    {
        private static readonly Lazy<IPriorityGroupsPreprocessor> Lazy = new Lazy<IPriorityGroupsPreprocessor>(() => new PriorityGroupsPreprocessor(), LazyThreadSafetyMode.PublicationOnly);

        public static IPriorityGroupsPreprocessor Instance => Lazy.Value;
        
        public IReadOnlyDictionary<double, IEsPriorityGroup> Preprocess(
            IReadOnlyDictionary<double, SearchPriorityGroup> priorityGroupsMappings,
            BaseRequest request,
            UnifiedSearchDefinitions definitions,
            Group group,
            int groupId)
        {
            var result = new Dictionary<double, IEsPriorityGroup>();
            foreach (var priorityGroupMapping in priorityGroupsMappings)
            {
                var searchPriorityGroup = priorityGroupMapping.Value;
                switch (searchPriorityGroup.Criteria.Type)
                {
                    case SearchPriorityCriteriaType.KSql:
                    {
                        BooleanPhraseNode tree = null;
                        BooleanPhraseNode.ParseSearchExpression(searchPriorityGroup.Criteria.Value, ref tree);
                        if (tree != null)
                        {
                            CatalogLogic.UpdateNodeTreeFields(request, ref tree, definitions, group, groupId);
                        }

                        result.Add(priorityGroupMapping.Key, new KSqlEsPriorityGroup(tree));
                        break;
                    }
                }
            }

            return new ReadOnlyDictionary<double, IEsPriorityGroup>(result);
        }
    }
}
