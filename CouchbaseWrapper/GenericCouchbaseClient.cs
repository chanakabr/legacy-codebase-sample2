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
            List<T> res = new List<T>();
            ICollection<object> coll = dict.Values;
            if (coll != null && coll.Count > 0)
            {
                foreach (object obj in coll)
                {
                    if (obj != null)
                    {
                        JsonTextReader jtr = null;
                        StringReader sr = null;
                        try
                        {
                            sr = new StringReader(obj.ToString());
                            jtr = new JsonTextReader(sr);
                            res.Add(serializer.Deserialize<T>(jtr));
                        }
                        finally
                        {
                            if (sr != null)
                            {
                                sr.Close();
                            }
                            if (jtr != null)
                            {
                                jtr.Close();
                            }
                        }
                    }
                } // foreach
            }

            return res;
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
                JsonTextReader jtr = null;
                StringReader sr = null;
                try
                {
                    sr = new StringReader(casResult.Result);
                    jtr = new JsonTextReader(sr);
                    retVal.Value = serializer.Deserialize<T>(jtr);
                }
                finally
                {
                    if (sr != null)
                    {
                        sr.Close();
                    }
                    if (jtr != null)
                    {
                        jtr.Close();
                    }
                }
            }

            return retVal;
        }

        public bool CasWithRetry<T>(T document, ulong docVersion, int numOfRetries, int retryInterval) where T : CbDocumentBase
        {
            bool res = false;
            for (int i = 0; i < numOfRetries; i++)
            {
                if (!Cas<T>(document, docVersion))
                {
                    if (retryInterval > 0)
                    {
                        Thread.Sleep(retryInterval);
                        CasGetResult<T> casGetResult = GetWithCas<T>(document.Id);
                    }
                    else
                    {
                        res = false;
                    }
                }
                else
                {
                    res = true;
                }
            } // for

            return res;
        }
    }
}