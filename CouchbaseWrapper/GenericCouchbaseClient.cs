﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
﻿using System.Threading;
﻿using Couchbase;
using Couchbase.Configuration;
using Couchbase.Extensions;
using CouchbaseWrapper.DalEntities;
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

        public bool Exists(string Id)
        {
            return _client.KeyExists(Id);
        }

        public T Get<T>(string Id) where T : CbDocumentBase
        {
            return _client.GetJson<T>(Id);
        }

        public IEnumerable<T> Get<T>(List<string> idList) where T : CbDocumentBase
        {
            IDictionary<string, object> dict = _client.Get(idList);
            JsonSerializer serializer = new JsonSerializer();
            return dict.Values.Select(item => serializer.Deserialize<T>(new JsonTextReader(new StringReader(item.ToString())))).ToList();
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
            JsonSerializer serializer = new JsonSerializer();
            CasGetResult<T> retVal = new CasGetResult<T>()
            {
                DocVersion = casResult.Cas,
                OperationResult = (eOperationResult)casResult.StatusCode,
            };
            if (retVal.OperationResult == eOperationResult.NoError)
            {
                retVal.Value = serializer.Deserialize<T>(new JsonTextReader(new StringReader(casResult.Result)));
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
    }
}