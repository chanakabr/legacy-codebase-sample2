﻿using System;
using System.Collections.Generic;
using System.Linq;
﻿using System.Threading;
﻿using Couchbase;
using Couchbase.Configuration;
using Couchbase.Extensions;
﻿using CouchbaseWrapper.DalEntities;
using Enyim.Caching.Memcached;
using Newtonsoft.Json;
using ApiObjects.CouchbaseWrapperObjects;

namespace CouchbaseWrapper
{
    public class GenericCouchbaseClient
    {
        private CouchbaseClient _client;

        internal GenericCouchbaseClient(CouchbaseClientSection configSection)
        {
            _client = new CouchbaseClient(configSection);
        }

        public GenericCouchbaseClient(CouchbaseClientConfiguration clientConfig)
        {
            _client = new CouchbaseClient(clientConfig);
        }

        public bool Exists(string id)
        {
            return _client.KeyExists(id);
        }

        public T Get<T>(string id) where T : CbDocumentBase
        {
            return _client.GetJson<T>(id);
        }

        public IDictionary<string, T> Get<T>(List<string> idList) where T : CbDocumentBase
        {
            IDictionary<string, T> retVal = null;
            IDictionary<string, object> dict = _client.Get(idList);
            if (dict!= null && dict.Count > 0)
            {
                retVal = dict.ToDictionary(item => item.Key, item => JsonConvert.DeserializeObject<T>(item.Value.ToString()));
            }
            return retVal;
        }

        public bool Store<T>(T document) where T : CbDocumentBase
        {
            return _client.StoreJson(StoreMode.Set, document.Id, document);
        }

        public bool Store<T>(T document, DateTime expiresAt) where T : CbDocumentBase
        {
            return _client.StoreJson(StoreMode.Set, document.Id, document, expiresAt);
        }

        public bool Store<T>(T document, TimeSpan ttl) where T : CbDocumentBase
        {
            return _client.StoreJson(StoreMode.Set, document.Id, document, ttl);
        }

        public bool Add<T>(T document, DateTime expiresAt) where T : CbDocumentBase
        {
            return _client.StoreJson(StoreMode.Add, document.Id, document, expiresAt);
        }

        public bool Remove(string id)
        {
            return _client.Remove(id);
        }

        public bool Cas<T>(T document, ulong docVersion) where T : CbDocumentBase
        {
            return _client.CasJson(StoreMode.Set, document.Id, document, docVersion);
        }

        public bool Cas<T>(T document, DateTime expiresAt, ulong docVersion) where T : CbDocumentBase
        {
            return _client.CasJson(StoreMode.Set, document.Id, document, docVersion, expiresAt);
        }

        public bool Cas<T>(T document, TimeSpan validFor, ulong docVersion) where T : CbDocumentBase
        {
            return _client.CasJson(StoreMode.Set, document.Id, document, docVersion, validFor);
        }

        public CasGetResult<T> GetWithCas<T>(string id) where T : CbDocumentBase
        {
            CasResult<string> casResult = _client.GetWithCas<string>(id);
            CasGetResult<T> retVal = new CasGetResult<T>()
            {
                DocVersion = casResult.Cas,
                OperationResult = (eOperationResult) casResult.StatusCode,
            };
            if ((eOperationResult) casResult.StatusCode == eOperationResult.NoError && !string.IsNullOrEmpty(casResult.Result))
            {
                retVal.Value = JsonConvert.DeserializeObject<T>(casResult.Result);
            }

            return retVal;
        }

        public bool CasWithRetry<T>(T document, ulong docVersion, int numOfRetries, int retryInterval) where T : CbDocumentBase
        {
            if (numOfRetries >= 0)
            {
                bool bCasOpearationRes = Cas<T>(document, docVersion);
                if (!bCasOpearationRes)
                {
                    numOfRetries--;
                    Thread.Sleep(retryInterval);
                    CasGetResult<T> casGetResult = GetWithCas<T>(document.Id);
                    return CasWithRetry<T>(document, casGetResult.DocVersion, numOfRetries, retryInterval);
                }
                else return true;
            }
            else return false;
        }

        public CasGetResult<T> GetWithLock<T>(string id, TimeSpan lockExpiration) where T : CbDocumentBase
        {
            CasResult<string> casResult = _client.GetWithLock<string>(id, lockExpiration);
            CasGetResult<T> retVal = new CasGetResult<T>()
            {
                DocVersion = casResult.Cas,
                OperationResult = (eOperationResult)casResult.StatusCode,
            };
            if ((eOperationResult)casResult.StatusCode == eOperationResult.NoError && !string.IsNullOrEmpty(casResult.Result))
            {
                retVal.Value = JsonConvert.DeserializeObject<T>(casResult.Result);
            }
            return retVal;
        }

        public bool Unlock(string id, ulong cas) 
        {
            return _client.Unlock(id, cas);
        }

        public ServerStats Stats()
        {
            return _client.Stats();
        }
    }
}