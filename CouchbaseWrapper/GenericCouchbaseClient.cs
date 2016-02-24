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
using KLogMonitor;
using System.Reflection;

namespace CouchbaseWrapper
{
    public class GenericCouchbaseClient
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const int MAX_RETRY = 3;

        private CouchbaseClient _client;
        ICouchbaseClientConfiguration configuration;

        private static object syncObj = new object();
        private static ReaderWriterLockSlim m_oSyncLock = new ReaderWriterLockSlim();

        internal GenericCouchbaseClient(CouchbaseClientSection configSection)
        {
            this.configuration = configSection;
            createInstance();
        }

        public GenericCouchbaseClient(CouchbaseClientConfiguration clientConfig)
        {
            this.configuration = clientConfig;
            createInstance();
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
                    // VBucketBelongsToAnotherServer
                    case 7:
                    // OutOfMemory
                    case 130:
                    // InternalError
                    case 132:
                    // Busy
                    case 133:
                    // TemporaryFailure
                    case 134:
                    // SocketPoolTimeout 
                    case 91:
                    // SocketPoolTimeout
                    case 145:
                    // UnableToLocateNode
                    case 146:
                    // NodeShutdown
                    case 147:
                    // OperationTimeout
                    case 148:
                    {
                        createInstance();

                        break;
                    }
                    default:
                    break;
                }
            }
        }


        private void createInstance()
        {
            if (_client != null)
            {
                _client.Dispose();
                _client = null;
            }

            if (m_oSyncLock.TryEnterWriteLock(1000))
            {
                try
                {
                    bool isDone = false;
                    int currentRetry = 0;


                    while (!isDone)
                    {
                        CouchbaseClient client = new CouchbaseClient(this.configuration);

                        if (client != null)
                        {
                            // test connection
                            bool isOK = true;
                            try
                            {
                                Enyim.Caching.Memcached.ServerStats stats = client.Stats();
                            }
                            catch (Exception ex)
                            {
                                isOK = false;

                                log.ErrorFormat("Connection test failed. error message = {0}, stack trace = {1}",
                                            ex.Message, ex.StackTrace);
                            }

                            if (!isOK)
                            {
                                currentRetry++;

                                if (currentRetry > MAX_RETRY)
                                {
                                    isDone = true;
                                    throw new Exception("Exceeded maximum number of Couchbase instance refresh");
                                }

                                Thread.Sleep(500);
                            }
                            else
                            {
                                _client = client;
                                isDone = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Failed creating instance of couchbase: ", ex);
                }
                finally
                {
                    m_oSyncLock.ExitWriteLock();
                }
            }
        }

        public bool Exists(string id)
        {
            return _client.KeyExists(id);
        }

        public T Get<T>(string id) where T : CbDocumentBase
        {
            T result = default(T);

            try
            {
                var executeGet = _client.ExecuteGetJson<T>(id);

                result = executeGet.Value;

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
            }
            catch (Exception ex)
            {
                createInstance();
                result = _client.GetJson<T>(id);
            }

            return result;
        }

        public IDictionary<string, T> Get<T>(List<string> idList) where T : CbDocumentBase
        {
            IDictionary<string, T> retVal = null;
            try
            {
                retVal = new Dictionary<string, T>();
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
                        retVal = new Dictionary<string, T>();

                        foreach (var item in executeGet)
                        {
                            retVal.Add(item.Key, JsonConvert.DeserializeObject<T>(item.Value.ToString()));
                        }
                    }
                    else
                    {
                        // Otherwise, recreate connection and try again
                        HandleStatusCode(statusCode);

                        //_client.Get<T>(
                        //retVal = _client.Get<T>(idList);
                    }
                }
            }
            catch (Exception ex)
            {
                createInstance();
                //retVal = _client.Get(idList);
            }

            return retVal;
        }

        public bool Store<T>(T document) where T : CbDocumentBase
        {
            bool result = false;

            try
            {
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
            }
            catch (Exception ex)
            {
                createInstance();
                result = _client.StoreJson(StoreMode.Set, document.Id, document);
            }

            return result;
        }

        public bool Store<T>(T document, DateTime expiresAt) where T : CbDocumentBase
        {
            bool result = false;

            try
            {
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
            }
            catch (Exception ex)
            {
                createInstance();
                result = _client.StoreJson(StoreMode.Set, document.Id, document, expiresAt);
            }

            return result;
        }

        public bool Store<T>(T document, TimeSpan ttl) where T : CbDocumentBase
        {
            bool result = false;

            try
            {
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
            }
            catch (Exception ex)
            {
                createInstance();
                result = _client.StoreJson(StoreMode.Set, document.Id, document, ttl);
            }

            return result;
        }

        public bool Add<T>(T document, DateTime expiresAt) where T : CbDocumentBase
        {
            bool result = false;

            try
            {
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

                        result = _client.StoreJson(StoreMode.Add, document.Id, document, expiresAt);
                    }
                }
            }
            catch (Exception ex)
            {
                createInstance();
                result = _client.StoreJson(StoreMode.Add, document.Id, document, expiresAt);
            }

            return result;
        }

        public bool Remove(string id)
        {
            bool result = false;

            try
            {
                var executeRemove = _client.ExecuteRemove(id);
                result = executeRemove.Success;

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
            }
            catch (Exception ex)
            {
                createInstance();
                result = _client.Remove(id);
            }

            return result;
        }

        public bool Cas<T>(T document, ulong docVersion) where T : CbDocumentBase
        {
            bool result = false;

            try
            {
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
            }
            catch (Exception ex)
            {
                createInstance();
                result = _client.CasJson(StoreMode.Set, document.Id, document, docVersion);
            }

            return result;
        }

        public bool Cas<T>(T document, DateTime expiresAt, ulong docVersion) where T : CbDocumentBase
        {
            bool result = false;
            try
            {


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
            }
            catch (Exception)
            {

                createInstance();
                result = _client.CasJson(StoreMode.Set, document.Id, document, docVersion, expiresAt);

            }

            return result;
        }

        public bool Cas<T>(T document, TimeSpan validFor, ulong docVersion) where T : CbDocumentBase
        {
            bool result = false;
            try
            {

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

            }
            catch (Exception)
            {
                createInstance();
                result = _client.CasJson(StoreMode.Set, document.Id, document, docVersion, validFor);
            }
            return result;
        }

        public CasGetResult<T> GetWithCas<T>(string id) where T : CbDocumentBase
        {
            CasGetResult<T> result = new CasGetResult<T>();
            try
            {

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
            }
            catch (Exception)
            {
                createInstance();

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
            try
            {

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

            }
            catch (Exception)
            {
                createInstance();

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
            return result;
        }

        public bool Unlock(string id, ulong cas) 
        {
            bool result = false;
            try
            {

                var executeUnlock = _client.ExecuteUnlock(id, cas);

                result = executeUnlock.Success;

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

            }
            catch (Exception)
            {
                createInstance();
                result = _client.Unlock(id, cas);
            }
            return result;
        }

        public ServerStats Stats()
        {
            return _client.Stats();
        }
    }
}