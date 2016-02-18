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
        ICouchbaseClientConfiguration configuration;

        internal GenericCouchbaseClient(CouchbaseClientSection configSection)
        {
            this.configuration = configSection;
            _client = new CouchbaseClient(configSection);
        }

        public GenericCouchbaseClient(CouchbaseClientConfiguration clientConfig)
        {
            this.configuration = clientConfig;
            _client = new CouchbaseClient(clientConfig);
        }

        /// <summary>
        /// See status codes at: http://docs.couchbase.com/couchbase-sdk-net-1.3/#checking-error-codes
        /// </summary>
        /// <param name="statusCode"></param>
        private void HandleStatusCode(int? statusCode, string key = "")
        {
            if (statusCode != null)
            {
                if (statusCode.Value != 0)
                {
                    // 1 - not found
                    if (statusCode.Value == 1)
                    {
                        //log.DebugFormat("Could not find key on couchbase: {0}", key);
                    }
                    else
                    {
                        //log.ErrorFormat("Error while executing action on CB. Status code = {0}", statusCode.Value);
                    }
                }

                // Cases of retry
                switch (statusCode)
                {
                    // Busy
                    case 133:
                    // SocketPoolTimeout
                    case 145:
                    // UnableToLocateNode
                    case 146:
                    // NodeShutdown
                    case 147:
                    // OperationTimeout
                    case 148:
                    {
                        _client = new CouchbaseClient(this.configuration);

                        break;
                    }
                    default:
                    break;
                }
            }
        }
        public bool Exists(string id)
        {
            return _client.KeyExists(id);
        }

        public T Get<T>(string id) where T : CbDocumentBase
        {
            var executeGet = _client.ExecuteGetJson<T>(id);

            T result = executeGet.Value;

            if (executeGet != null)
            {
                if (executeGet.Exception != null)
                {
                    throw executeGet.Exception;
                }

                if (executeGet.StatusCode != 0)
                {
                    int? statusCode = executeGet.StatusCode;
                    HandleStatusCode(statusCode, id);

                    result = _client.GetJson<T>(id);
                }
            }

            return result;
        }

        public IDictionary<string, T> Get<T>(List<string> idList) where T : CbDocumentBase
        {
            IDictionary<string, T> retVal = null;
            IDictionary<string, object> dict = new Dictionary<string, object>();
                //_client.Get(idList);

            var executeGet = _client.ExecuteGet(idList);

            if (executeGet != null)
            {
                int? statusCode = 0;
                foreach (var item in executeGet)
                {
                    if (item.Value.Exception != null)
                    {
                        throw item.Value.Exception;
                    }

                    if (item.Value.StatusCode != 0)
                    {
                        statusCode = item.Value.StatusCode;
                        break;
                    }
                }

                if (statusCode == 0)
                {
                    // if successfull - build dictionary based on execution result
                    dict = new Dictionary<string, object>();

                    foreach (var item in executeGet)
                    {
                        dict.Add(item.Key, JsonConvert.DeserializeObject<T>(item.Value.ToString()));
                    }
                }
                else
                {
                    // Otherwise, recreate connection and try again
                    HandleStatusCode(statusCode);

                    dict = _client.Get(idList);
                }
            }

            return retVal;
        }

        public bool Store<T>(T document) where T : CbDocumentBase
        {
            bool result = false; 
            
            var executeStore = _client.ExecuteStoreJson(StoreMode.Set, document.Id, document);

            if (executeStore != null)
            {
                if (executeStore.Exception != null)
                {
                    throw executeStore.Exception;
                }

                if (executeStore.StatusCode == 0)
                {
                    result = executeStore.Success;
                }
                else
                {
                    HandleStatusCode(executeStore.StatusCode);

                    result = _client.StoreJson(StoreMode.Set, document.Id, document);
                }
            }

            return result;
        }

        public bool Store<T>(T document, DateTime expiresAt) where T : CbDocumentBase
        {
            bool result = false;

            var executeStore = _client.ExecuteStoreJson(StoreMode.Set, document.Id, document, expiresAt);

            if (executeStore != null)
            {
                if (executeStore.Exception != null)
                {
                    throw executeStore.Exception;
                }

                if (executeStore.StatusCode == 0)
                {
                    result = executeStore.Success;
                }
                else
                {
                    HandleStatusCode(executeStore.StatusCode);

                    result = _client.StoreJson(StoreMode.Set, document.Id, document, expiresAt);
                }
            }

            return result;
        }

        public bool Store<T>(T document, TimeSpan ttl) where T : CbDocumentBase
        {
            bool result = false;

            var executeStore = _client.ExecuteStoreJson(StoreMode.Set, document.Id, document, ttl);

            if (executeStore != null)
            {
                if (executeStore.Exception != null)
                {
                    throw executeStore.Exception;
                }

                if (executeStore.StatusCode == 0)
                {
                    result = executeStore.Success;
                }
                else
                {
                    HandleStatusCode(executeStore.StatusCode);

                    result = _client.StoreJson(StoreMode.Set, document.Id, document, ttl);
                }
            }

            return result;
        }

        public bool Add<T>(T document, DateTime expiresAt) where T : CbDocumentBase
        {
            bool result = false;

            var executeStore = _client.ExecuteStoreJson(StoreMode.Add, document.Id, document, expiresAt);

            if (executeStore != null)
            {
                if (executeStore.Exception != null)
                {
                    throw executeStore.Exception;
                }

                if (executeStore.StatusCode == 0)
                {
                    result = executeStore.Success;
                }
                else
                {
                    HandleStatusCode(executeStore.StatusCode);

                    result = _client.StoreJson(StoreMode.Set, document.Id, document, expiresAt);
                }
            }

            return result;
        }

        public bool Remove(string id)
        {
            var executeRemove = _client.ExecuteRemove(id);
            bool result = executeRemove.Success;

            if (executeRemove != null)
            {
                if (executeRemove.Exception != null)
                {
                    throw executeRemove.Exception;
                }

                if (executeRemove.StatusCode == 0)
                {
                    result = executeRemove.Success;
                }
                else
                {
                    int? statusCode = executeRemove.StatusCode;
                    HandleStatusCode(statusCode);

                    result = _client.Remove(id);
                }
            }

            return result;
        }

        public bool Cas<T>(T document, ulong docVersion) where T : CbDocumentBase
        {
            bool result = false;

            var executeCas = _client.ExecuteCasJson(StoreMode.Set, document.Id, document, docVersion);

            if (executeCas != null)
            {
                if (executeCas.Exception != null)
                {
                    throw executeCas.Exception;
                }

                if (executeCas.StatusCode == 0)
                {
                    result = executeCas.Success;
                }
                else
                {
                    HandleStatusCode(executeCas.StatusCode);

                    result = _client.CasJson(StoreMode.Set, document.Id, document, docVersion);
                }
            }

            return result;
        }

        public bool Cas<T>(T document, DateTime expiresAt, ulong docVersion) where T : CbDocumentBase
        {
            bool result = false;

            var executeCas = _client.ExecuteCasJson(StoreMode.Set, document.Id, document, expiresAt, docVersion);

            if (executeCas != null)
            {
                if (executeCas.Exception != null)
                {
                    throw executeCas.Exception;
                }

                if (executeCas.StatusCode == 0)
                {
                    result = executeCas.Success;
                }
                else
                {
                    HandleStatusCode(executeCas.StatusCode);

                    result = _client.CasJson(StoreMode.Set, document.Id, document, docVersion, expiresAt);
                }
            }

            return result;
        }

        public bool Cas<T>(T document, TimeSpan validFor, ulong docVersion) where T : CbDocumentBase
        {
            bool result = false;

            var executeCas = _client.ExecuteCasJson(StoreMode.Set, document.Id, document, validFor, docVersion);

            if (executeCas != null)
            {
                if (executeCas.Exception != null)
                {
                    throw executeCas.Exception;
                }

                if (executeCas.StatusCode == 0)
                {
                    result = executeCas.Success;
                }
                else
                {
                    HandleStatusCode(executeCas.StatusCode);

                    result = _client.CasJson(StoreMode.Set, document.Id, document, docVersion, validFor);
                }
            }

            return result;
        }

        public CasGetResult<T> GetWithCas<T>(string id) where T : CbDocumentBase
        {
            CasGetResult<T> result = new CasGetResult<T>();

            var executeGet = _client.ExecuteGet<string>(id);

            if (executeGet != null)
            {
                if (executeGet.Exception != null)
                {
                    throw executeGet.Exception;
                }

                if (executeGet.StatusCode == 0)
                {
                    if (executeGet.StatusCode.HasValue)
                    {
                        result.OperationResult = (eOperationResult)executeGet.StatusCode.Value;
                    }

                    if (result.OperationResult == eOperationResult.NoError && !string.IsNullOrEmpty(executeGet.Value))
                    {
                        result.Value = JsonConvert.DeserializeObject<T>(executeGet.Value);
                    }

                    result.DocVersion = executeGet.Cas;
                }
                else
                {
                    int? statusCode = executeGet.StatusCode;
                    HandleStatusCode(statusCode);

                    CasResult<string> casResult = _client.GetWithCas<string>(id);
                    result = new CasGetResult<T>()
                    {
                        DocVersion = casResult.Cas,
                        OperationResult = (eOperationResult)casResult.StatusCode,
                    };

                    if ((eOperationResult)casResult.StatusCode == eOperationResult.NoError && !string.IsNullOrEmpty(casResult.Result))
                    {
                        result.Value = JsonConvert.DeserializeObject<T>(casResult.Result);
                    }
                }
            }

            return result;
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
            CasGetResult<T> result = new CasGetResult<T>();

            var executeGet = _client.ExecuteGetWithLock<string>(id, lockExpiration);

            if (executeGet != null)
            {
                if (executeGet.Exception != null)
                {
                    throw executeGet.Exception;
                }

                if (executeGet.StatusCode == 0)
                {
                    if (executeGet.StatusCode.HasValue)
                    {
                        result.OperationResult = (eOperationResult)executeGet.StatusCode.Value;
                    }

                    if (result.OperationResult == eOperationResult.NoError && !string.IsNullOrEmpty(executeGet.Value))
                    {
                        result.Value = JsonConvert.DeserializeObject<T>(executeGet.Value);
                    }

                    result.DocVersion = executeGet.Cas;
                }
                else
                {
                    int? statusCode = executeGet.StatusCode;
                    HandleStatusCode(statusCode);

                    CasResult<string> casResult = _client.GetWithLock<string>(id, lockExpiration);
                    result = new CasGetResult<T>()
                    {
                        DocVersion = casResult.Cas,
                        OperationResult = (eOperationResult)casResult.StatusCode,
                    };

                    if ((eOperationResult)casResult.StatusCode == eOperationResult.NoError && !string.IsNullOrEmpty(casResult.Result))
                    {
                        result.Value = JsonConvert.DeserializeObject<T>(casResult.Result);
                    }
                }
            }

            return result;
        }

        public bool Unlock(string id, ulong cas) 
        {
            var executeUnlock = _client.ExecuteUnlock(id, cas);
            bool result = executeUnlock.Success;

            if (executeUnlock != null)
            {
                if (executeUnlock.Exception != null)
                {
                    throw executeUnlock.Exception;
                }

                if (executeUnlock.StatusCode == 0)
                {
                    result = executeUnlock.Success;
                }
                else
                {
                    int? statusCode = executeUnlock.StatusCode;
                    HandleStatusCode(statusCode);

                    result = _client.Unlock(id, cas);
                }
            }

            return result;
        }
    }
}