using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using ApiObjects;
using ApiObjects.Response;
using ApiObjects.SearchPriorityGroups;
using Phx.Lib.Log;
using Microsoft.Extensions.Logging;

namespace DAL.SearchPriorityGroups
{
    public class SearchPriorityGroupRepository : ISearchPriorityGroupRepository
    {
        private readonly ISearchPriorityGroupDal _searchPriorityGroupDal;
        private readonly ISearchPriorityGroupCbRepository _searchPriorityGroupCbRepository;
        private readonly ILogger _logger;

        private static readonly Lazy<SearchPriorityGroupRepository> Lazy = new Lazy<SearchPriorityGroupRepository>(
            () => new SearchPriorityGroupRepository(SearchPriorityGroupDal.Instance, SearchPriorityGroupCbRepository.Instance, new KLogger(nameof(SearchPriorityGroupRepository))),
            LazyThreadSafetyMode.PublicationOnly);

        public static SearchPriorityGroupRepository Instance => Lazy.Value;

        public SearchPriorityGroupRepository(ISearchPriorityGroupDal searchPriorityGroupDal, ISearchPriorityGroupCbRepository searchPriorityGroupCbRepository, ILogger logger)
        {
            _searchPriorityGroupDal = searchPriorityGroupDal;
            _searchPriorityGroupCbRepository = searchPriorityGroupCbRepository;
            _logger = logger;
        }

        public GenericResponse<SearchPriorityGroup> Add(long groupId, SearchPriorityGroup searchPriorityGroup, long updaterId)
        {
            var response = new GenericResponse<SearchPriorityGroup>();
            try
            {
                var name = ConvertToDictionary(searchPriorityGroup.Name);
                var cbDocument = new SearchPriorityGroupCb(name, searchPriorityGroup.Criteria.Type, searchPriorityGroup.Criteria.Value);

                var documentKey = _searchPriorityGroupCbRepository.Save(groupId, cbDocument);
                if (string.IsNullOrEmpty(documentKey))
                {
                    _logger.LogError($"Could not save {nameof(SearchPriorityGroupCb)}.");
                }
                else
                {
                    var dataSet = _searchPriorityGroupDal.Add(groupId, documentKey, updaterId);
                    response = CreateSearchPriorityGroup(dataSet, cbDocument);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error while executing {nameof(Add)}: {e.Message}.");
            }

            return response;
        }

        public GenericResponse<SearchPriorityGroup> Update(long groupId, SearchPriorityGroup searchPriorityGroup)
        {
            var response = new GenericResponse<SearchPriorityGroup>();
            try
            {
                var dataSet = _searchPriorityGroupDal.Get(groupId, searchPriorityGroup.Id);
                var entityResponse = CreateSearchPriorityGroupEntity(dataSet, true);
                if (entityResponse.IsOkStatusCode())
                {
                    var existingSearchPriorityGroupCb = _searchPriorityGroupCbRepository.Get(groupId, entityResponse.Object.DocumentKey);
                    var updatedSearchPriorityGroup = GetUpdatedSearchPriorityGroupCb(searchPriorityGroup, existingSearchPriorityGroupCb);
                    var updatedName = ConvertToDictionary(updatedSearchPriorityGroup.Name);
                    var updatedSearchPriorityGroupCb = new SearchPriorityGroupCb(updatedName, updatedSearchPriorityGroup.Criteria.Type, updatedSearchPriorityGroup.Criteria.Value);

                    var documentKey = _searchPriorityGroupCbRepository.Save(groupId, entityResponse.Object.DocumentKey, updatedSearchPriorityGroupCb);
                    if (string.IsNullOrEmpty(documentKey))
                    {
                        _logger.LogError($"Could not save {nameof(SearchPriorityGroupCb)}.");
                    }
                    else
                    {
                        response = new GenericResponse<SearchPriorityGroup>(Status.Ok, updatedSearchPriorityGroup);
                    }
                }
                else
                {
                    response = new GenericResponse<SearchPriorityGroup>(entityResponse.Status);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error while executing {nameof(Update)}: {e.Message}.");
            }

            return response;
        }

        public Status Delete(long groupId, long searchPriorityGroupId, long updaterId)
        {
            var response = Status.Error;
            try
            {
                var dataSet = _searchPriorityGroupDal.Get(groupId, searchPriorityGroupId);
                var entityResponse = CreateSearchPriorityGroupEntity(dataSet, true);
                if (entityResponse.IsOkStatusCode())
                {
                    var entityDeleteResult = _searchPriorityGroupDal.Delete(groupId, searchPriorityGroupId, updaterId);
                    if (entityDeleteResult)
                    {
                        var result = _searchPriorityGroupCbRepository.Delete(groupId, entityResponse.Object.DocumentKey);
                        if (result)
                        {
                            response = Status.Ok;
                        }
                        else
                        {
                            _logger.LogError($"Could not delete {nameof(SearchPriorityGroupCb)}.");
                        }
                    }
                    else
                    {
                        _logger.LogError($"Could not delete search priority group with id={searchPriorityGroupId}.");
                    }
                }
                else
                {
                    response = entityResponse.Status;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error while executing {nameof(Delete)}: {e.Message}.");
            }

            return response;
        }

        public GenericListResponse<SearchPriorityGroup> List(long groupId, IEnumerable<long> ids)
        {
            var response = new GenericListResponse<SearchPriorityGroup>();
            try
            {
                var dataSet = ids.Any()
                    ? _searchPriorityGroupDal.List(groupId, ids)
                    : _searchPriorityGroupDal.List(groupId);
                if (dataSet != null && dataSet.Tables.Count == 1 && dataSet.Tables[0] != null)
                {
                    var entities = new List<SearchPriorityGroupEntity>();
                    foreach (DataRow row in dataSet.Tables[0].Rows)
                    {
                        var entity = CreateSearchPriorityGroupEntity(row);
                        entities.Add(entity);
                    }

                    var documentKeys = entities.Select(x => x.DocumentKey).ToArray();
                    var documents = _searchPriorityGroupCbRepository.List(groupId, documentKeys);
                    if (documents == null)
                    {
                        _logger.LogError($"Could not get {nameof(SearchPriorityGroupCb)}'s with documentKeys [{string.Join(",", documentKeys)}].");
                    }
                    else
                    {
                        var searchPriorityGroups = new List<SearchPriorityGroup>();
                        foreach (var entity in entities)
                        {
                            var document = documents[entity.DocumentKey];
                            var searchPriorityGroup = CreateSearchPriorityGroup(entity.Id, document);
                            searchPriorityGroups.Add(searchPriorityGroup);
                        }

                        response = new GenericListResponse<SearchPriorityGroup>(Status.Ok, searchPriorityGroups)
                        {
                            TotalItems = searchPriorityGroups.Count
                        };
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error while executing {nameof(List)}: {e.Message}.");
            }

            return response;
        }

        private SearchPriorityGroup GetUpdatedSearchPriorityGroupCb(SearchPriorityGroup searchPriorityGroup, SearchPriorityGroupCb existingSearchPriorityGroupCb)
        {
            var updatedName = new Dictionary<string, string>(existingSearchPriorityGroupCb.Name);
            foreach (var languageContainer in searchPriorityGroup.Name)
            {
                if (updatedName.ContainsKey(languageContainer.m_sLanguageCode3))
                {
                    updatedName[languageContainer.m_sLanguageCode3] = languageContainer.m_sValue;
                }
                else
                {
                    updatedName.Add(languageContainer.m_sLanguageCode3, languageContainer.m_sValue);
                }
            }

            var updatedCriteriaValue = searchPriorityGroup.Criteria?.Value ?? existingSearchPriorityGroupCb.Criteria.Value;

            var updatedSearchPriorityGroup = new SearchPriorityGroup(searchPriorityGroup.Id, updatedName.Select(x => new LanguageContainer(x.Key, x.Value)).ToArray(), SearchPriorityCriteriaType.KSql, updatedCriteriaValue);

            return updatedSearchPriorityGroup;
        }

        private GenericResponse<SearchPriorityGroup> CreateSearchPriorityGroup(DataSet dataSet, SearchPriorityGroupCb cbDocument)
        {
            var entityResponse = CreateSearchPriorityGroupEntity(dataSet, false);
            if (entityResponse.IsOkStatusCode())
            {
                var searchPriorityGroup = CreateSearchPriorityGroup(entityResponse.Object.Id, cbDocument);

                return new GenericResponse<SearchPriorityGroup>(Status.Ok, searchPriorityGroup);
            }

            return new GenericResponse<SearchPriorityGroup>(entityResponse.Status);
        }

        private GenericResponse<SearchPriorityGroupEntity> CreateSearchPriorityGroupEntity(DataSet dataSet, bool allowDoesNotExistResponse)
        {
            if (dataSet == null || dataSet.Tables.Count != 1 || dataSet.Tables[0] == null || dataSet.Tables[0].Rows.Count > 1)
            {
                return new GenericResponse<SearchPriorityGroupEntity>();
            }

            if (dataSet.Tables[0].Rows.Count == 0)
            {
                return allowDoesNotExistResponse
                    ? new GenericResponse<SearchPriorityGroupEntity>(eResponseStatus.SearchPriorityGroupDoesNotExist)
                    : new GenericResponse<SearchPriorityGroupEntity>();
            }

            var entity = CreateSearchPriorityGroupEntity(dataSet.Tables[0].Rows[0]);

            return new GenericResponse<SearchPriorityGroupEntity>(Status.Ok, entity);
        }

        private static SearchPriorityGroup CreateSearchPriorityGroup(long id, SearchPriorityGroupCb document)
        {
            var name = document.Name
                .Select(x => new LanguageContainer(x.Key, x.Value))
                .ToArray();
            var searchPriorityGroup = new SearchPriorityGroup(id, name, document.Criteria.Type, document.Criteria.Value);

            return searchPriorityGroup;
        }

        private SearchPriorityGroupEntity CreateSearchPriorityGroupEntity(DataRow row)
        {
            var id = ODBCWrapper.Utils.GetLongSafeVal(row, "ID");
            var documentKey = ODBCWrapper.Utils.GetSafeStr(row, "DOCUMENT_KEY");

            return new SearchPriorityGroupEntity(id, documentKey);
        }

        private Dictionary<string, string> ConvertToDictionary(IEnumerable<LanguageContainer> languageContainers)
        {
            return languageContainers.ToDictionary(x => x.m_sLanguageCode3, x => x.m_sValue);
        }

        private class SearchPriorityGroupEntity
        {
            public long Id { get; }
            public string DocumentKey { get; }

            public SearchPriorityGroupEntity(long id, string documentKey)
            {
                Id = id;
                DocumentKey = documentKey;
            }
        }
    }
}