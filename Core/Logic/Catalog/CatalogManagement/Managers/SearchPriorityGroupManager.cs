using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using ApiLogic.Catalog.CatalogManagement.Validators;
using ApiObjects;
using ApiObjects.Response;
using ApiObjects.SearchPriorityGroups;
using CachingProvider.LayeredCache;
using Core.Catalog.CatalogManagement;
using DAL.SearchPriorityGroups;
using Phx.Lib.Log;
using Microsoft.Extensions.Logging;

namespace ApiLogic.Catalog.CatalogManagement.Managers
{
    public class SearchPriorityGroupManager : ISearchPriorityGroupManager
    {
        private readonly ISearchPriorityGroupRepository _searchPriorityGroupRepository;
        private readonly ISearchPriorityGroupOrderedListRepository _searchPriorityGroupOrderedListRepository;
        private readonly ISearchPriorityGroupValidator _validator;
        private readonly IKLogger _logger;
        private readonly ILayeredCache _layeredCache;

        private static readonly Lazy<SearchPriorityGroupManager> Lazy = new Lazy<SearchPriorityGroupManager>(
            () => new SearchPriorityGroupManager(SearchPriorityGroupRepository.Instance, SearchPriorityGroupOrderedListRepository.Instance, SearchPriorityGroupValidator.Instance, new KLogger(nameof(SearchPriorityGroupManager)), LayeredCache.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        public static SearchPriorityGroupManager Instance => Lazy.Value;

        public SearchPriorityGroupManager(
            ISearchPriorityGroupRepository searchPriorityGroupRepository,
            ISearchPriorityGroupOrderedListRepository searchPriorityGroupOrderedListRepository,
            ISearchPriorityGroupValidator validator,
            IKLogger logger,
            ILayeredCache layeredCache)
        {
            _searchPriorityGroupRepository = searchPriorityGroupRepository;
            _searchPriorityGroupOrderedListRepository = searchPriorityGroupOrderedListRepository;
            _validator = validator;
            _logger = logger;
            _layeredCache = layeredCache;
        }

        public GenericResponse<SearchPriorityGroup> AddSearchPriorityGroup(long groupId, SearchPriorityGroup searchPriorityGroup, long userId)
        {
            var result = _validator.ValidateSearchPriorityGroup(groupId, searchPriorityGroup, out var message) 
                ? _searchPriorityGroupRepository.Add(groupId, searchPriorityGroup, userId) 
                : new GenericResponse<SearchPriorityGroup>(eResponseStatus.Error, message);

            return result;
        }

        public GenericResponse<SearchPriorityGroup> UpdateSearchPriorityGroup(long groupId, SearchPriorityGroup searchPriorityGroup)
        {
            GenericResponse<SearchPriorityGroup> result;

            if (_validator.ValidateSearchPriorityGroup(groupId, searchPriorityGroup, out var message))
            {
                result = _searchPriorityGroupRepository.Update(groupId, searchPriorityGroup);
                InvalidatePriorityGroupMappings(groupId, result);
            }
            else
            {
                result = new GenericResponse<SearchPriorityGroup>(eResponseStatus.Error, message);
            }

            return result;
        }

        public Status DeleteSearchPriorityGroup(long groupId, long searchPriorityGroupId, long userId)
        {
            var orderedListResponse = _searchPriorityGroupOrderedListRepository.Get(groupId);
            if (orderedListResponse.IsOkStatusCode() && orderedListResponse.Object.PriorityGroupIds.Any(x => x == searchPriorityGroupId))
            {
                var newOrderedList = new SearchPriorityGroupOrderedIdsSet(orderedListResponse.Object.PriorityGroupIds.Where(x => x != searchPriorityGroupId));
                var orderedListUpdateResponse = _searchPriorityGroupOrderedListRepository.Update(groupId, newOrderedList);
                if (!orderedListUpdateResponse.IsOkStatusCode())
                {
                    orderedListResponse = orderedListUpdateResponse;
                }
            }

            var result = orderedListResponse.IsOkStatusCode()
                ? _searchPriorityGroupRepository.Delete(groupId, searchPriorityGroupId, userId)
                : orderedListResponse.Status;

            InvalidatePriorityGroupMappings(groupId, result);

            return result;
        }

        public GenericListResponse<SearchPriorityGroup> ListSearchPriorityGroups(long groupId, SearchPriorityGroupQuery query)
        {
            var priorityGroupIds = new long[0];
            if (query.OrderBy == SearchPriorityGroupOrderBy.PriorityDesc || query.ActiveOnly)
            {
                var orderedListResponse = _searchPriorityGroupOrderedListRepository.Get(groupId);
                if (orderedListResponse.IsOkStatusCode())
                {
                    priorityGroupIds = orderedListResponse.Object.PriorityGroupIds.ToArray();
                }
                else
                {
                    return new GenericListResponse<SearchPriorityGroup>(orderedListResponse.Status, null);
                }
            }
            
            var ids = new List<long>();
            if (query.IdEqual.HasValue)
            {
                ids.Add(query.IdEqual.Value);
            }
            else if (query.ActiveOnly)
            {
                if (priorityGroupIds.Any())
                {
                    ids.AddRange(priorityGroupIds);
                }
                else
                {
                    return new GenericListResponse<SearchPriorityGroup>(Status.Ok, new List<SearchPriorityGroup>());
                }
            }

            var result = _searchPriorityGroupRepository.List(groupId, ids);
            if (result.IsOkStatusCode())
            {
                result.Objects = OrderSearchPriorityGroups(result.Objects, query.OrderBy, query.Language, query.DefaultLanguage, priorityGroupIds)
                    .Skip(query.PageIndex * query.PageSize)
                    .Take(query.PageSize)
                    .ToList();
            }

            return result;
        }

        public GenericResponse<SearchPriorityGroupOrderedIdsSet> SetKalturaSearchPriorityGroupOrderedList(long groupId, SearchPriorityGroupOrderedIdsSet orderedList)
        {
            GenericResponse<SearchPriorityGroupOrderedIdsSet> result;

            var searchPriorityGroupsResponse = _searchPriorityGroupRepository.List(groupId, orderedList.PriorityGroupIds);
            if (searchPriorityGroupsResponse.IsOkStatusCode())
            {
                var missingIds = orderedList.PriorityGroupIds.Except(searchPriorityGroupsResponse.Objects.Select(x => x.Id)).ToArray();
                if (missingIds.Any())
                {
                    result = new GenericResponse<SearchPriorityGroupOrderedIdsSet>(
                        eResponseStatus.SearchPriorityGroupDoesNotExist,
                        $"One or more search priority groups does not exist: [{string.Join(",", missingIds)}].");
                }
                else
                {
                    result = _searchPriorityGroupOrderedListRepository.Update(groupId, orderedList);

                    InvalidatePriorityGroupMappings(groupId, result);
                }
            }
            else
            {
                result = new GenericResponse<SearchPriorityGroupOrderedIdsSet>(searchPriorityGroupsResponse.Status);
            }

            return result;
        }

        public GenericResponse<SearchPriorityGroupOrderedIdsSet> GetKalturaSearchPriorityGroupOrderedList(long groupId)
        {
            var result = _searchPriorityGroupOrderedListRepository.Get(groupId);

            return result;
        }

        public IReadOnlyDictionary<double, SearchPriorityGroup> ListSearchPriorityGroupMappings(int groupId)
        {
            IReadOnlyDictionary<double, SearchPriorityGroup> result = null;

            var listSearchPriorityGroupMappingsKey = LayeredCacheKeys.GetListSearchPriorityGroupMappingsKey(groupId);
            var listSearchPriorityGroupMappingsInvalidationKeys = new List<string>
            {
                LayeredCacheKeys.GetListSearchPriorityGroupMappingsInvalidationKey(groupId)
            };
            var parameters = new Dictionary<string, object>
            {
                { "groupId", groupId }
            };
            if (!_layeredCache.Get(
                listSearchPriorityGroupMappingsKey,
                ref result,
                ListSearchPriorityGroupsMappings,
                parameters,
                groupId,
                LayeredCacheConfigNames.LIST_SEARCH_PRIORITY_GROUPS_MAPPINGS,
                listSearchPriorityGroupMappingsInvalidationKeys))
            {
                _logger.LogWarning($"Can not fetch priority groups mappings! Returning empty...");
            }

            return result;
        }

        private Tuple<IReadOnlyDictionary<double, SearchPriorityGroup>, bool> ListSearchPriorityGroupsMappings(Dictionary<string, object> funcParams)
        {
            IReadOnlyDictionary<double, SearchPriorityGroup> result = new ReadOnlyDictionary<double, SearchPriorityGroup>(new Dictionary<double, SearchPriorityGroup>());

            if (funcParams != null && funcParams.Count == 1 && funcParams.ContainsKey("groupId"))
            {
                int? groupId = funcParams["groupId"] as int?;
                if (groupId.HasValue)
                {
                    var priorityGroupsResponse = ListSearchPriorityGroups(groupId.Value,
                        new SearchPriorityGroupQuery
                        {
                            ActiveOnly = true,
                            PageSize = SearchPriorityGroupValidator.MAX_CONDITION_COUNT,
                            OrderBy = SearchPriorityGroupOrderBy.PriorityDesc
                        });
                    if (!priorityGroupsResponse.IsOkStatusCode())
                    {
                        return new Tuple<IReadOnlyDictionary<double, SearchPriorityGroup>, bool>(result, false);
                    }

                    var priorityGroupsCount = priorityGroupsResponse.Objects.Count + 1;
                    result = priorityGroupsResponse.Objects.ToDictionary(k => (double)priorityGroupsCount--, pg => pg);
                    return new Tuple<IReadOnlyDictionary<double, SearchPriorityGroup>, bool>(result, true);
                }
            }

            return new Tuple<IReadOnlyDictionary<double, SearchPriorityGroup>, bool>(result, false);
        }

        private IEnumerable<SearchPriorityGroup> OrderSearchPriorityGroups(IEnumerable<SearchPriorityGroup> searchPriorityGroups, SearchPriorityGroupOrderBy orderBy, string language, string defaultLanguage, IEnumerable<long> priorityGroupIds)
        {
            switch (orderBy)
            {
                case SearchPriorityGroupOrderBy.NameAsc:
                case SearchPriorityGroupOrderBy.NameDesc:
                    var orderedList = searchPriorityGroups.OrderBy(x => GetLanguageValue(x.Name, language, defaultLanguage));

                    return orderBy == SearchPriorityGroupOrderBy.NameAsc
                        ? orderedList
                        : orderedList.Reverse();
                default:
                    var nonActiveIds = searchPriorityGroups.Select(x => x.Id).Except(priorityGroupIds);
                    var orderedIds = priorityGroupIds.Concat(nonActiveIds).ToList();
                    
                    return searchPriorityGroups.OrderBy(x => orderedIds.IndexOf(x.Id));
            }
        }

        private string GetLanguageValue(LanguageContainer[] languageContainers, string language, string defaultLanguage)
        {
            var languageContainer = languageContainers.FirstOrDefault(x => x.m_sLanguageCode3.Equals(language, StringComparison.InvariantCultureIgnoreCase))
                                    ?? languageContainers.FirstOrDefault(x => x.m_sLanguageCode3.Equals(defaultLanguage, StringComparison.InvariantCultureIgnoreCase));

            return languageContainer?.m_sValue;
        }

        private void InvalidatePriorityGroupMappings<T>(long groupId, GenericResponse<T> response)
        {
            InvalidatePriorityGroupMappings(groupId, response.Status);
        }

        private void InvalidatePriorityGroupMappings(long groupId, Status status)
        {
            if (!status.IsOkStatusCode())
            {
                return;
            }

            var invalidationKey = LayeredCacheKeys.GetListSearchPriorityGroupMappingsInvalidationKey(groupId);
            if (!_layeredCache.SetInvalidationKey(invalidationKey))
            {
                _logger.LogWarning($"Failed to set invalidation key {invalidationKey}");
            }
        }
    }
}