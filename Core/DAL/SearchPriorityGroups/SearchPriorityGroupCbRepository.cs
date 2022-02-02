using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CouchbaseManager;
using Phx.Lib.Log;
using Microsoft.Extensions.Logging;

namespace DAL.SearchPriorityGroups
{
    public class SearchPriorityGroupCbRepository : ISearchPriorityGroupCbRepository
    {
        private readonly ICouchbaseManager _couchbaseManager;
        private readonly IKeyGenerator _keyGenerator;
        private readonly ILogger _logger;

        private static readonly Lazy<SearchPriorityGroupCbRepository> Lazy = new Lazy<SearchPriorityGroupCbRepository>(
            () => new SearchPriorityGroupCbRepository(new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS), KeyGenerator.Instance, new KLogger(nameof(SearchPriorityGroupCbRepository))),
            LazyThreadSafetyMode.PublicationOnly);

        public static SearchPriorityGroupCbRepository Instance => Lazy.Value;

        public SearchPriorityGroupCbRepository(ICouchbaseManager couchbaseManager, IKeyGenerator keyGenerator, ILogger logger)
        {
            _couchbaseManager = couchbaseManager;
            _keyGenerator = keyGenerator;
            _logger = logger;
        }

        public string Save(long groupId, SearchPriorityGroupCb searchPriorityGroupCb)
        {
            string result = null;
            try
            {
                var documentKey = $"{groupId}_KalturaSearchPriorityGroup_{_keyGenerator.GetGuidKey()}";
                result = Save(groupId, documentKey, searchPriorityGroupCb);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(Save)} failed. Error: {e.Message}.");
            }

            return result;
        }

        public string Save(long groupId, string documentKey, SearchPriorityGroupCb searchPriorityGroupCb)
        {
            string result = null;
            try
            {
                if (IsKeyValid(groupId, documentKey))
                {
                    var saveResult = _couchbaseManager.Set(documentKey, searchPriorityGroupCb, 0);
                    if (saveResult)
                    {
                        result = documentKey;
                    }
                    else
                    {
                        _logger.LogError($"{nameof(ICouchbaseManager.Set)} failed: {nameof(documentKey)}={documentKey}.");
                    }
                }
                else
                {
                    _logger.LogError($"{nameof(Save)} failed: {nameof(documentKey)}={documentKey} is invalid.");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(Save)} failed. Error: {e.Message}.");
            }

            return result;
        }

        public SearchPriorityGroupCb Get(long groupId, string documentKey)
        {
            SearchPriorityGroupCb result = null; 
            try
            {
                if (IsKeyValid(groupId, documentKey))
                {
                    result = _couchbaseManager.Get<SearchPriorityGroupCb>(documentKey);
                }
                else
                {
                    _logger.LogError($"{nameof(Get)} failed: {nameof(documentKey)}={documentKey} is invalid.");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(Get)} failed with {nameof(documentKey)}={documentKey}. Error: {e.Message}.");
            }

            return result;
        }

        public IDictionary<string, SearchPriorityGroupCb> List(long groupId, IEnumerable<string> documentKeys)
        {
            try
            {
                documentKeys = documentKeys.Where(x => IsKeyValid(groupId, x));
                var documentValues = _couchbaseManager.GetValues<SearchPriorityGroupCb>(documentKeys.ToList());

                return documentValues;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(List)} failed with {nameof(documentKeys)}=[{string.Join(",", documentKeys)}]. Error: {e.Message}.");

                return null;
            }
        }

        public bool Delete(long groupId, string documentKey)
        {
            var result = false;
            try
            {
                if (IsKeyValid(groupId, documentKey))
                {
                    result = _couchbaseManager.Remove(documentKey);
                    if (!result)
                    {
                        _logger.LogError($"{nameof(ICouchbaseManager.Remove)} failed: {nameof(documentKey)}={documentKey}.");
                    }
                }
                else
                {
                    _logger.LogError($"{nameof(Delete)} failed: {nameof(documentKey)}={documentKey} is invalid.");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(Delete)} failed. Error: {e.Message}.");
            }

            return result;
        }

        private bool IsKeyValid(long groupId, string documentKey)
        {
            return documentKey.StartsWith($"{groupId}_KalturaSearchPriorityGroup_");
        }
    }
}