using System;
using System.Threading;
using ApiObjects.Response;
using ApiObjects.SearchPriorityGroups;
using CouchbaseManager;
using KLogMonitor;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DAL.SearchPriorityGroups
{
    public class SearchPriorityGroupOrderedListRepository : ISearchPriorityGroupOrderedListRepository
    {
        private readonly ICouchbaseManager _couchbaseManager;
        private readonly ILogger _logger;

        private static readonly Lazy<SearchPriorityGroupOrderedListRepository> Lazy = new Lazy<SearchPriorityGroupOrderedListRepository>(
            () => new SearchPriorityGroupOrderedListRepository(new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS), new KLogger(nameof(SearchPriorityGroupOrderedListRepository))),
            LazyThreadSafetyMode.PublicationOnly);

        public static SearchPriorityGroupOrderedListRepository Instance => Lazy.Value;

        public SearchPriorityGroupOrderedListRepository(ICouchbaseManager couchbaseManager, ILogger logger)
        {
            _couchbaseManager = couchbaseManager;
            _logger = logger;
        }

        public GenericResponse<SearchPriorityGroupOrderedIdsSet> Get(long groupId)
        {
            var response = new GenericResponse<SearchPriorityGroupOrderedIdsSet>();
            try
            {
                var documentKey = GetDocumentKey(groupId);

                var cbOrderedList = _couchbaseManager.Get<SearchPriorityGroupOrderedListCb>(documentKey, out var status, new JsonSerializerSettings());
                if (status == eResultStatus.SUCCESS || status == eResultStatus.KEY_NOT_EXIST)
                {
                    var orderedList = new SearchPriorityGroupOrderedIdsSet(cbOrderedList?.PriorityGroupIds);
                    response = new GenericResponse<SearchPriorityGroupOrderedIdsSet>(Status.Ok, orderedList);
                }
                else
                {
                    _logger.LogError($"{nameof(ICouchbaseManager.Get)} failed: {nameof(documentKey)}={documentKey}.");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(Get)} failed with {nameof(groupId)}={groupId}. Error: {e.Message}.");
            }

            return response;
        }

        public GenericResponse<SearchPriorityGroupOrderedIdsSet> Update(long groupId, SearchPriorityGroupOrderedIdsSet orderedList)
        {
            var response = new GenericResponse<SearchPriorityGroupOrderedIdsSet>();
            try
            {
                var documentKey = GetDocumentKey(groupId);

                var cbOrderedList = new SearchPriorityGroupOrderedListCb(orderedList.PriorityGroupIds);
                var result = _couchbaseManager.Set(documentKey, cbOrderedList, 0);
                if (result)
                {
                    response = new GenericResponse<SearchPriorityGroupOrderedIdsSet>(Status.Ok, orderedList);
                }
                else
                {
                    _logger.LogError($"{nameof(ICouchbaseManager.Set)} failed: {nameof(documentKey)}={documentKey}, {nameof(orderedList)}:[{string.Join(",", orderedList.PriorityGroupIds)}].");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(Update)} failed. Error: {e.Message}.");
            }

            return response;
        }

        private string GetDocumentKey(long groupId)
        {
            var documentKey = $"{groupId}_KalturaSearchPriorityGroupOrderedIdsSet";

            return documentKey;
        }
    }
}