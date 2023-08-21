using Couchbase;
using Couchbase.Configuration.Client;
using Couchbase.Core.Serialization;
using Couchbase.Core.Transcoders;
using Phx.Lib.Log;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using Couchbase.N1QL;
using System.Linq;
using System.Threading.Tasks;
using CouchbaseManager.Exceptions;
using Phx.Lib.Appconfig;
using CouchbaseManager.Models;
using Polly;
using Microsoft.Extensions.Logging;

namespace CouchbaseManager
{
    public enum eCouchbaseBucket
    {
        DEFAULT = 0,
        NOTIFICATION = 1,
        SOCIAL = 2,
        SOCIALFRIENDS = 3,
        EPG = 4,
        MEDIAMARK = 5,
        STATISTICS = 6,
        CACHE = 7,
        SCHEDULED_TASKS = 8,
        CROWDSOURCE = 9,
        DRM = 10,
        RECORDINGS = 11,
        DOMAIN_CONCURRENCY = 12,
        EPG_MARKS = 13,
        MEDIA_HITS = 14,
        MEMCACHED = 15,
        OTT_APPS = 16,
        NGINX = 17,
        GROUPS = 18
    }

    public enum eResultStatus
    {
        ERROR = 0,
        SUCCESS = 1,
        KEY_NOT_EXIST = 2
    }

    public class CouchbaseManager : ICouchbaseManager
    {
        #region Consts

        public const string COUCHBASE_CONFIG = "couchbaseClients/Couchbase";
        public const string COUCHBASE_TCM_CONFIG_KEY = "couchbase_client_config";
        public const string MAX_DEGREE_OF_PARALLELISM_KEY = "max_degree_of_parallelism";
        public const string COUCHBASE_APP_CONFIG = "CouchbaseSectionMapping";
        private const string TCM_KEY_FORMAT = "cb_{0}.{1}";
        private const double GET_LOCK_TS_SECONDS = 5;
        private const int CACHE_KEY_MAX_SIZE = 250;

        /// <summary>
        /// Defines duration of a month in seconds, see http://docs.couchbase.com/developer/dev-guide-3.0/doc-expiration.html
        /// </summary>
        private const uint monthInSeconds = 30 * 24 * 60 * 60;

        /// <summary>
        /// Defines the default SendTimeout value (just like in documentation)
        /// </summary>
        internal const uint SEND_TIMEOUT_DEFAULT_MILLISECONDS = 15000;

        #endregion

        #region Static Members

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static object locker = new object();
        private static ReaderWriterLockSlim m_oSyncLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private static ParallelOptions PARALLEL_OPTIONS = new ParallelOptions { MaxDegreeOfParallelism = 1 };
        private static DateTime lastInitializationTime = DateTime.MinValue;

        protected static Couchbase.Core.Serialization.DefaultSerializer serializer;


        private static bool IsClusterInitialized
        {
            get;
            set;
        }

        #endregion

        #region Data Members

        private string bucketName;
        private static readonly ClientConfiguration clientConfiguration;

        #endregion

        #region Ctor

        static CouchbaseManager()
        {
            serializer = new DefaultSerializer();

            // DeserializationSettings
            serializer.DeserializationSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;
            serializer.DeserializationSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            serializer.DeserializationSettings.Formatting = Newtonsoft.Json.Formatting.Indented;

            // SerializerSettings
            serializer.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            serializer.SerializerSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;
            serializer.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
            clientConfiguration = GetCouchbaseClientConfiguration();

            var maxDegreeOfParallelism = Phx.Lib.Appconfig.TCMClient.Settings.Instance.GetValue<int>($"{COUCHBASE_TCM_CONFIG_KEY}.{MAX_DEGREE_OF_PARALLELISM_KEY}");
            PARALLEL_OPTIONS.MaxDegreeOfParallelism = maxDegreeOfParallelism > 1 ? maxDegreeOfParallelism : 1;


        }
        /// <summary>
        /// Initializes a CouchbaseManager instance with configuration in web.config, according to predefined bucket sections
        /// </summary>
        /// <param name="bucket"></param>
        public CouchbaseManager(eCouchbaseBucket bucket)
            : this(bucket.ToString().ToLower())
        {
        }

        public CouchbaseManager(eCouchbaseBucket bucket, bool fromTcm = true, bool useApplicationSettingMapping = false) :
            this(bucket.ToString(), fromTcm, useApplicationSettingMapping)
        {
        }

        /// <summary>
        /// Initializes a CouchbaseManager instance with configuration in web.config or TCM, according to dynamic bucket section
        /// </summary>
        /// <param name="bucket"></param>
        public CouchbaseManager(string subSection, bool fromTcm = true, bool useApplicationSettingMapping = false)
        {
            subSection = subSection.ToLower();
            bucketName = GetBucketName(subSection);

            if (!IsClusterInitialized)
            {
                lock (locker)
                {
                    if (!IsClusterInitialized)
                    {
                        ClusterHelper.Initialize(clientConfiguration);
                        IsClusterInitialized = true;
                    }
                }
            }
        }

        // Should we cache this in memory instead of retriving the setting again from tcm \ web.config ?
        private static ClientConfiguration GetCouchbaseClientConfiguration()
        {
            //First try to load from TCM 
            var configToReturn = GetCouchbaseClientConfigurationFromTCM();

            // if Get from TCM fail it will return null then load from Web.Config
            //if (configToReturn == null)
            //{
            //    configToReturn = GetCouchbaseClientConfigurationFromWebConfig();
            //}

            return configToReturn;
        }

        private static ClientConfiguration GetCouchbaseClientConfigurationFromTCM()
        {
            try
            {
                ClientConfiguration couchbaseConfigFromTCM = ApplicationConfiguration.Current.CouchbaseClientConfiguration.GetClientConfiguration();

                if (couchbaseConfigFromTCM != null)
                {
                    // This is here because the default constructor of ClientConfiguration adds a http://localhost:8091/pools url to the 0 index :\
                    // Sunny: somehow I got to a situation where we don't have this localhost server, so I added more conditions so we won't remove a "good" server
                    if (couchbaseConfigFromTCM.Servers != null && couchbaseConfigFromTCM.Servers.Count > 0)
                    {
                        var firstServer = couchbaseConfigFromTCM.Servers[0];

                        if (firstServer.AbsoluteUri.ToLower().Contains("localhost"))
                        {
                            couchbaseConfigFromTCM.Servers.RemoveAt(0);
                        }
                    }
                    couchbaseConfigFromTCM.Transcoder = GetTranscoder;
                    return couchbaseConfigFromTCM;
                }
            }
            catch (Exception e)
            {
                log.WarnFormat("Could not load couchbase configuration from TCM using key:[{0}], trying to load it from web.config file. exception details:{1}", COUCHBASE_TCM_CONFIG_KEY, e);
            }

            return null;
        }

        private static ITypeTranscoder GetTranscoder()
        {
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = new DefaultContractResolver()
            };
            JsonSerializerSettings deserializationSettings = new JsonSerializerSettings()
            {
                ContractResolver = new DefaultContractResolver()
            };
            CustomSerializer serializer = new CustomSerializer(deserializationSettings, serializerSettings);
            CustomTranscoder transcoder = new CustomTranscoder(serializer);

            return transcoder;
        }

        // Removed because .config is no longer releavnt for .net core
        //private static ClientConfiguration GetCouchbaseClientConfigurationFromWebConfig()
        //{
        //    ClientConfiguration configToReturn;
        //    try
        //    {
        //        configToReturn = new ClientConfiguration((CouchbaseClientSection) ConfigurationManager.GetSection(COUCHBASE_CONFIG));
        //        configToReturn.Transcoder = GetTranscoder;
        //    }
        //    catch (Exception e)
        //    {
        //        log.WarnFormat("Could not load couchbase configuration from Web.Config using key:[{0}], trying to load it from web.config file. exception details:{1}", COUCHBASE_CONFIG, e);
        //        throw;
        //    }

        //    return configToReturn;
        //}

        // Removed because .config is no longer releavnt for .net core
        ///// <summary>
        ///// Retrieve  bucketName from web.config or app.conifg. in case no TCM application. 
        /////  configSections should be added to  web.config or app.conifg 
        /////  Sample:
        /////  <configSections>
        /////  <section name="CouchbaseSectionMapping" type="System.Configuration.NameValueFileSectionHandler,System, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" />
        /////  </configSections>
        /////  <CouchbaseSectionMapping><add key="authorization" value="crowdsource" /> </CouchbaseSectionMapping>
        ///// </summary>
        ///// <param name="subSection">key</param>
        ///// <returns>value</returns>
        //private string GetBucketNameFromSettings(string subSection)
        //{
        //    string bucketName = string.Empty;
        //    try
        //    {
        //        NameValueCollection section = null;//(NameValueCollection)ConfigurationManager.GetSection(COUCHBASE_APP_CONFIG);
        //        if (section != null)
        //        {
        //            bucketName = section[subSection];
        //            if (string.IsNullOrEmpty(bucketName))
        //                log.ErrorFormat("Error getting bucketName for couchbaseBucket:{0}. Not exist", subSection);
        //        }
        //        else
        //            log.ErrorFormat("Error getting bucketName for couchbaseBucket:{0}. CouchbaseSectionMapping is empty.", subSection);
        //    }
        //    catch (Exception exc)
        //    {
        //        log.ErrorFormat("Error getting bucketName for couchbaseBucket:{0}. Exception:{1}", subSection, exc);
        //    }

        //    return bucketName;
        //}

        private string GetBucketName(string couchbaseBucket)
        {
            string bucketName = string.Empty;
            try
            {
                Dictionary<string, string> couchbaseBucketWithBucketNameDic = Phx.Lib.Appconfig.TCMClient.Settings.Instance.GetValue<Dictionary<string, string>>("CouchbaseSectionMapping");
                if (couchbaseBucketWithBucketNameDic != null)
                {
                    if (couchbaseBucketWithBucketNameDic.ContainsKey(couchbaseBucket.ToLower()))
                        bucketName = couchbaseBucketWithBucketNameDic[couchbaseBucket.ToLower()];
                    else
                        log.ErrorFormat("Error getting bucketName for couchbaseBucket:{0}. Not exist", couchbaseBucket);
                }
                else
                    log.ErrorFormat("Error getting bucketName for couchbaseBucket:{0}. CouchbaseSectionMapping is empty.", couchbaseBucket);
            }
            catch (Exception exc)
            {
                log.ErrorFormat("Error getting bucketName for couchbaseBucket:{0}. Exception:{1}", couchbaseBucket, exc);
            }

            return bucketName;
        }

        /*
            // See http://developer.couchbase.com/documentation/server/4.0/sdks/dotnet-2.2/configuring-the-client.html
            // We have a list of buckets in the server, but we don't know the exact bucket name.
            // We do know the name of the SECTION
            // after "parsing" the configuration to the client object, we will see which of the items in the app.config has the same key as the enum.
            // If they are identical, then we can get the server's bucket name from it
         */
        #endregion

        #region Private Methods

        private void HandleException(string key, IOperationResult operationResult)
        {
            eResultStatus status = eResultStatus.ERROR;
            HandleStatusCode(operationResult, ref status, key);
            throw operationResult.Exception;
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
                        log.DebugFormat("Could not find key on couchbase: {0}", key);
                    else
                        log.ErrorFormat("Error while executing action on CB. Status code = {0}", statusCode.Value);
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
                            //m_Client = CouchbaseManager.CouchbaseManager.RefreshInstance(bucket);
                            break;
                        }
                    default:
                        break;
                }
            }
        }

        private void HandleStatusCode(IOperationResult result, ref eResultStatus status, string key = "")
        {
            status = eResultStatus.ERROR;
            if (result.Status != Couchbase.IO.ResponseStatus.Success)
            {
                // 1 - not found
                if (result.Status == Couchbase.IO.ResponseStatus.KeyNotFound)
                    log.LogTrace($"Could not find key on couchbase: {key}");
                else
                {
                    log.ErrorFormat("Error while executing action on CB. Key = {0}, Status code = {1}; Status = {2}, Message = {3}, EX = {4}",
                        key,
                        (int)result.Status, result.Status.ToString(),
                        result.Message,
                        (result.Exception == null ? string.Empty : result.Exception.ToString()));
                }
            }

            // Cases of retry
            switch (result.Status)
            {
                case Couchbase.IO.ResponseStatus.AuthenticationContinue:
                    break;
                case Couchbase.IO.ResponseStatus.AuthenticationError:
                    break;
                case Couchbase.IO.ResponseStatus.Busy:
                    break;
                case Couchbase.IO.ResponseStatus.ClientFailure:
                    break;
                case Couchbase.IO.ResponseStatus.DocumentMutationLost:
                    break;
                case Couchbase.IO.ResponseStatus.IncrDecrOnNonNumericValue:
                    break;
                case Couchbase.IO.ResponseStatus.InternalError:
                    break;
                case Couchbase.IO.ResponseStatus.InvalidArguments:
                    break;
                case Couchbase.IO.ResponseStatus.InvalidRange:
                    break;
                case Couchbase.IO.ResponseStatus.ItemNotStored:
                    break;
                case Couchbase.IO.ResponseStatus.KeyExists:
                    status = eResultStatus.SUCCESS;
                    break;
                case Couchbase.IO.ResponseStatus.KeyNotFound:
                    status = eResultStatus.KEY_NOT_EXIST;
                    break;
                case Couchbase.IO.ResponseStatus.NoReplicasFound:
                    break;
                case Couchbase.IO.ResponseStatus.NodeUnavailable:
                    break;
                case Couchbase.IO.ResponseStatus.None:
                    break;
                case Couchbase.IO.ResponseStatus.NotSupported:
                    break;
                case Couchbase.IO.ResponseStatus.OperationTimeout:
                    {
                        // remove bucket
                        lock (locker)
                        {
                            // don't do this if we had already done this in last 15 seconds, 
                            // to avoid problems with resource concurrency (dispose and use at same time)
                            if ((DateTime.Now - lastInitializationTime).TotalSeconds > 15)
                            {
                                log.DebugFormat("CouchBase : OperationTimeout detected. " +
                                    "Due to SDK bug, most likely the timeout will repeat infinitely until restart. Therefore, removing bucket {0} now - : " +
                                    "it will be reinitialized later.", bucketName);
                                lastInitializationTime = DateTime.Now;
                                ClusterHelper.RemoveBucket(bucketName);
                            }
                        }

                        break;
                    }
                case Couchbase.IO.ResponseStatus.OutOfMemory:
                    break;
                case Couchbase.IO.ResponseStatus.Success:
                    status = eResultStatus.SUCCESS;
                    break;
                case Couchbase.IO.ResponseStatus.TemporaryFailure:
                    break;
                case Couchbase.IO.ResponseStatus.TransportFailure:
                    break;
                case Couchbase.IO.ResponseStatus.UnknownCommand:
                    break;
                case Couchbase.IO.ResponseStatus.VBucketBelongsToAnotherServer:
                    break;
                case Couchbase.IO.ResponseStatus.ValueTooLarge:
                    break;
                default:
                    break;
            }
        }

        private IOperationResult<T> HandleNodeUnavailable<T>(string key, Couchbase.Core.IBucket bucket, IOperationResult<T> getResult, string cbDescription)
        {
            // for node unavailable or operation timeout on regular or ephemeral buckets - try to get from replica
            if ((getResult.Status == Couchbase.IO.ResponseStatus.NodeUnavailable || getResult.Status == Couchbase.IO.ResponseStatus.OperationTimeout)
                && (bucket.BucketType == Couchbase.Core.Buckets.BucketTypeEnum.Couchbase || bucket.BucketType == Couchbase.Core.Buckets.BucketTypeEnum.Ephemeral))
            {
                log.ErrorFormat("CouchbaseManager..Get failed because of {0}. Trying GetFromReplica. Bucket = {1} , Key = {2}", getResult.Status, bucketName, key);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.SELECT, Database = cbDescription })
                {
                    getResult = bucket.GetFromReplica<T>(key);
                    if (getResult.Status == Couchbase.IO.ResponseStatus.Success)
                    {
                        log.DebugFormat("successfully get document from replica, Bucket = {0} , Key = {1}", bucketName, key);
                    }
                }
            }

            return getResult;
        }

        private static List<IDocument<string>> ObjectsToJson<T>(List<IDocument<T>> objs)
        {
            if (objs == null) { return new List<IDocument<string>>(); }
            var result = objs.Select(o => new Document<string>
            {
                Id = o.Id,
                Content = ObjectToJson(o.Content),
                Cas = o.Cas,
                Expiry = o.Expiry,
            });
            return result.Cast<IDocument<string>>().ToList();
        }

        private static string ObjectToJson<T>(T obj)
        {
            if (obj != null)
                return Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.None);
            else
                return string.Empty;
        }

        private static string ObjectToJson<T>(T obj, JsonSerializerSettings jsonSerializerSettings)
        {
            if (obj != null && jsonSerializerSettings != null)
                return Newtonsoft.Json.JsonConvert.SerializeObject(obj, jsonSerializerSettings);
            else
                return string.Empty;
        }

        private static T JsonToObject<T>(string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                if (serializer != null && serializer.DeserializationSettings != null)
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json, serializer.DeserializationSettings);
                }
                else
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
                }
            }
            else
            {
                return default(T);
            }
        }

        private static T JsonToObject<T>(string json, JsonSerializerSettings jsonSerializerSettings)
        {
            if (!string.IsNullOrEmpty(json) && jsonSerializerSettings != null)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json, jsonSerializerSettings);
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        /// See http://docs.couchbase.com/developer/dev-guide-3.0/doc-expiration.html
        /// </summary>
        /// <param name="expiration"></param>
        /// <returns></returns>
        private static uint FixExpirationTime(uint expiration)
        {
            uint result = expiration;

            // If document should expire in more than a month, convert it to unix time
            if (expiration > monthInSeconds)
            {
                DateTime expirationDate = DateTime.UtcNow.AddSeconds(expiration);
                DateTime truncDateTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                result = (uint)(expirationDate - truncDateTimeUtc).TotalSeconds;
            }
            
            return result;
        }
        
        /// <summary>
        /// See http://docs.couchbase.com/developer/dev-guide-3.0/doc-expiration.html
        /// This method except seconds as input, but returns MILLISECONDS. Should be used everywhere where <see cref="Document{T}"/> property Expiry used.
        /// </summary>
        /// <param name="expiration"></param>
        /// <returns></returns>
        private static uint FixExpirationTimeMilliseconds(uint expiration)
        {
            return FixExpirationTime(expiration) * 1000;
        }

        #endregion

        #region Public Methods

        public bool HealthCheck()
        {
            bool result = false;

            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);
                var pingReport = bucket.Ping();

                // basic check for ping
                if (pingReport != null && !string.IsNullOrEmpty(pingReport.Id) && pingReport.Version > 0)
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                log.Error($"Health check failed for CouchBase. ex = {ex}");
            }

            return result;
        }

        public bool IsKeyExists(string key)
        {
            bool res = false;

            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);
                string cbDescription = string.Format("bucket: {0}; key: {1}", bucketName, key);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.SELECT, Database = cbDescription })
                {
                    res = bucket.Exists(key);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchbaseManager - Failed IsKeyExists on bucket = {0} for key = {1}, ex = {2}", bucketName, key, ex);
            }

            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiration">TTL in seconds</param>
        /// <returns></returns>
        public bool Add(string key, object value, uint expiration = 0, bool asJson = false, bool suppressErrors = false)
        {
            bool result = false;

            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);
                expiration = FixExpirationTime(expiration);

                IOperationResult insertResult = null;

                string cbDescription = string.Format("bucket: {0}; key: {1}; expiration: {2} seconds", bucketName, key, expiration);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.INSERT, Database = cbDescription })
                {
                    if (!asJson)
                        insertResult = bucket.Insert(key, value, expiration);
                    else
                    {
                        string serializedValue = ObjectToJson(value);
                        insertResult = bucket.Insert(key, serializedValue, expiration);
                    }
                }

                if (insertResult != null)
                {
                    if (!suppressErrors && insertResult.Exception != null)
                        HandleException(key, insertResult);

                    if (insertResult.Status == Couchbase.IO.ResponseStatus.Success)
                        result = insertResult.Success;
                    else if (!suppressErrors)
                    {
                        eResultStatus status = eResultStatus.ERROR;
                        HandleStatusCode(insertResult, ref status, key);

                        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, cbDescription))
                        {
                            if (!asJson)
                                insertResult = bucket.Insert(key, value, expiration);
                            else
                            {
                                string serializedValue = ObjectToJson(value);
                                insertResult = bucket.Insert(key, serializedValue, expiration);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchbaseManager - Failed Add on bucket = {0} on key = {1}, ex = {2}", bucketName, key, ex);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiration">TTL in seconds</param>
        /// <returns></returns>
        public bool Add<T>(string key, T value, uint expiration = 0, bool asJson = false, bool suppressErrors = false)
        {
            bool result = false;

            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);

                IOperationResult insertResult = null;
                expiration = FixExpirationTime(expiration);

                string cbDescription = string.Format("bucket: {0}; key: {1}; expiration: {2} seconds", bucketName, key, expiration);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.INSERT, Database = cbDescription })
                {
                    if (!asJson)
                        insertResult = bucket.Insert<T>(key, value, expiration);
                    else
                    {
                        string serializedValue = ObjectToJson(value);
                        insertResult = bucket.Insert<string>(key, serializedValue, expiration);
                    }
                }

                if (insertResult != null)
                {
                    if (!suppressErrors && insertResult.Exception != null)
                        HandleException(key, insertResult);

                    if (insertResult.Status == Couchbase.IO.ResponseStatus.Success)
                        result = insertResult.Success;
                    else if (!suppressErrors)
                    {
                        eResultStatus status = eResultStatus.ERROR;
                        HandleStatusCode(insertResult, ref status, key);

                        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, cbDescription))
                        {
                            if (!asJson)
                                insertResult = bucket.Insert<T>(key, value, expiration);
                            else
                            {
                                string serializedValue = ObjectToJson(value);
                                insertResult = bucket.Insert<string>(key, serializedValue, expiration);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchbaseManager - Failed Add on bucket = {0} on key = {1}, ex = {2}", bucketName, key, ex);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiration">TTL in seconds</param>
        /// <returns></returns>
        public bool Set(string key, object value, uint expiration = 0, bool asJson = false)
        {
            bool result = false;

            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);
                IOperationResult insertResult = null;
                expiration = FixExpirationTime(expiration);

                string cbDescription = string.Format("bucket: {0}; key: {1}; expiration: {2} seconds", bucketName, key, expiration);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.UPDATE, Database = cbDescription })
                {
                    if (!asJson)
                        insertResult = bucket.Upsert(key, value, expiration);
                    else
                    {
                        string serializedValue = ObjectToJson(value);
                        insertResult = bucket.Upsert(key, serializedValue, expiration);
                    }
                }

                if (insertResult != null)
                {
                    if (insertResult.Exception != null)
                        HandleException(key, insertResult);

                    if (insertResult.Status == Couchbase.IO.ResponseStatus.Success)
                        result = insertResult.Success;
                    else
                    {
                        eResultStatus status = eResultStatus.ERROR;
                        HandleStatusCode(insertResult, ref status, key);

                        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, cbDescription))
                        {
                            if (!asJson)
                                insertResult = bucket.Upsert(key, value, expiration);
                            else
                            {
                                string serializedValue = ObjectToJson(value);
                                insertResult = bucket.Upsert(key, serializedValue, expiration);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchbaseManager - Failed Set on bucket = {0} on key = {1}, ex = {2}", bucketName, key, ex);
            }
            return result;
        }

        public async Task<IDictionary<string, bool>> MultiSet<T>(IEnumerable<CouchbaseRecord<T>> values, bool allowPartial = false)
        {
            IDictionary<string, bool> results = null;
            IOperationResult<T>[] operationResults = null;

            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);
                using (var km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.UPDATE, Database = $"bucket: {bucketName}; keyCount: {values.Count()};" })
                {
                    var tasks = values.Select(v => bucket.UpsertAsync(v.Key, v.Content, FixExpirationTime(v.Expiration))).ToArray();
                    await Task.WhenAll(tasks);
                    operationResults = tasks.Select(t => t.Result).ToArray();
                }
                
                if (operationResults.Any(d => d.Status != Couchbase.IO.ResponseStatus.Success))
                {
                    var errors = operationResults.Where(r => r.Exception != null).Select(r => r.Exception).ToList();

                    if (!allowPartial)
                    {
                        var failedItems = operationResults.Where(r => !r.Success);
                        errors.Add(new Exception($"CouchbaseManager - Will not allow partial set, One or more of the set operation failed: [{ObjectToJson(failedItems)}]"));
                    }

                    if (errors.Any())
                    {
                        log.Error($"CouchbaseManager - errors during MultiSet<T>: {ObjectToJson(errors)}");
                        throw new AggregateException(errors);
                    }
                }

                results = operationResults.ToDictionary(k => k.Id, v => v.Success);
            }
            catch (Exception ex)
            {
                log.Error($"CouchbaseManager - Failed MultiSet<T> with keys:[{ObjectToJson(values.Select(v => v.Key))}],", ex);
            }

            return results;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiration">TTL in seconds</param>
        /// <returns></returns>
        public bool Set<T>(string key, T value, uint expiration = 0, bool asJson = false)
        {
            bool result = false;

            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);
                IOperationResult insertResult = null;
                expiration = FixExpirationTime(expiration);

                string cbDescription = string.Format("bucket: {0}; key: {1}; expiration: {2} seconds", bucketName, key, expiration);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.UPDATE, Database = cbDescription })
                {
                    if (!asJson)
                        insertResult = bucket.Upsert<T>(key, value, expiration);
                    else
                    {
                        string serializedValue = ObjectToJson(value);
                        insertResult = bucket.Upsert<string>(key, serializedValue, expiration);
                    }
                }

                if (insertResult != null)
                {
                    if (insertResult.Exception != null)
                        HandleException(key, insertResult);

                    if (insertResult.Status == Couchbase.IO.ResponseStatus.Success)
                        result = insertResult.Success;
                    else
                    {
                        eResultStatus status = eResultStatus.ERROR;
                        HandleStatusCode(insertResult, ref status, key);

                        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, cbDescription))
                        {
                            if (!asJson)
                                insertResult = bucket.Upsert<T>(key, value, expiration);
                            else
                            {
                                string serializedValue = ObjectToJson(value);
                                insertResult = bucket.Upsert<string>(key, serializedValue, expiration);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchbaseManager - Failed Set<T> with key = {0}, error = {1}", key, ex);
            }
            return result;
        }

        public bool Set<T>(string key, T value, uint expiration)
        {
            return Set(key, value, expiration, null);
        }

        public bool Set<T>(string key, T value, uint expiration, JsonSerializerSettings jsonSerializerSettings)
        {
            bool result = false;

            IOperationResult setResult;
            try
            {
                setResult = Upsert(key, value, expiration, jsonSerializerSettings);
            }
            catch (KeySizeExceededException exception)
            {
                log.Error(exception.Message);
                return false;
            }

            if (setResult != null)
            {
                if (setResult.Exception != null)
                {
                    if (!(setResult.Exception is CasMismatchException))
                        HandleException(key, setResult);
                }

                if (setResult.Status == Couchbase.IO.ResponseStatus.Success)
                    result = setResult.Success;
                else
                {
                    eResultStatus status = eResultStatus.ERROR;
                    HandleStatusCode(setResult, ref status, key);
                }
            }

            return result;
        }
        
        private IOperationResult Upsert<T>(string key, T value, uint expiration, JsonSerializerSettings jsonSerializerSettings = null)
        {
            if (key?.Length > CACHE_KEY_MAX_SIZE)
            {
                throw new KeySizeExceededException(key);
            }

            var bucket = ClusterHelper.GetBucket(bucketName);
            expiration = FixExpirationTime(expiration);
            
            string cbDescription = string.Format("bucket: {0}; key: {1}; expiration: {2} seconds", bucketName, key, expiration);
            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.UPDATE, Database = cbDescription })
            {
                if (jsonSerializerSettings == null)
                {
                    return bucket.Upsert(key, value, expiration);
                }
            
                return bucket.Upsert(key, ObjectToJson(value, jsonSerializerSettings), expiration);
            }
        }

        public bool Set<T>(string key, T value, bool unlock, out ulong outCas, uint expiration = 0, ulong cas = 0)
        {
            bool result = false;
            outCas = 0;
            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);

                IOperationResult insertResult = null;
                expiration = FixExpirationTime(expiration);

                string cbDescription = string.Format("bucket: {0}; key: {1}; expiration: {2} seconds", bucketName, key, expiration);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.UPDATE, Database = cbDescription })
                {
                    if (cas > 0)
                    {
                        log.DebugFormat("Upsert cas. key: {0}, cas: {1}", key, cas);
                        insertResult = bucket.Upsert<T>(key, value, cas, expiration);
                    }
                    else
                    {
                        log.DebugFormat("Upsert cas. key: {0}, cas: {1}", key, cas);
                        insertResult = bucket.Upsert<T>(key, value, expiration);
                    }
                }

                if (insertResult != null)
                {
                    if (insertResult.Exception != null)
                        HandleException(key, insertResult);

                    if (insertResult.Status == Couchbase.IO.ResponseStatus.Success)
                    {
                        result = insertResult.Success;
                        outCas = insertResult.Cas;

                        // log.DebugFormat("SET before unlocking {0}, cas: {1}", key, cas);
                        //if (unlock && cas > 0)
                        //{
                        //    var unlockResult = bucket.Unlock(key, cas);
                        //    if (unlockResult.Success)
                        //        log.DebugFormat("SET after unlocking {0}, cas: {1}", key, cas);
                        //    else
                        //        log.DebugFormat("failed to unlock - key: {0}, cas: {1}, error = {2}", key, cas, unlockResult.Status.ToString());
                        //}
                    }
                    else
                    {
                        eResultStatus status = eResultStatus.ERROR;
                        HandleStatusCode(insertResult, ref status, key);

                        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, cbDescription))
                        {
                            if (cas > 0)
                                insertResult = bucket.Upsert<T>(key, value, cas, expiration);
                            else
                                insertResult = bucket.Upsert<T>(key, value, expiration);

                            if (insertResult.Status == Couchbase.IO.ResponseStatus.Success)
                            {
                                result = insertResult.Success;
                                outCas = insertResult.Cas;

                                //if (unlock && cas > 0)
                                //{
                                //    var unlockResult = bucket.Unlock(key, cas);
                                //    if (unlockResult.Success)
                                //        log.DebugFormat("SET after unlocking {0}, cas: {1}", key, cas);
                                //    else
                                //        log.DebugFormat("failed to unlock - key: {0}, cas: {1}, error = {2}", key, cas, unlockResult.Status.ToString());
                                //}
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchbaseManager - Failed Set<T> with key = {0}, error = {1}", key, ex);
            }
            return result;
        }

        public bool Set<T>(string key, T value, bool unlock, uint expiration = 0, ulong cas = 0)
        {
            bool result = false;

            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);

                IOperationResult insertResult = null;
                expiration = FixExpirationTime(expiration);

                string cbDescription = string.Format("bucket: {0}; key: {1}; expiration: {2} seconds", bucketName, key, expiration);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.UPDATE, Database = cbDescription })
                {
                    if (cas > 0)
                    {
                        log.DebugFormat("Upsert cas. key: {0}, cas: {1}", key, cas);
                        insertResult = bucket.Upsert<T>(key, value, cas, expiration);
                    }
                    else
                    {
                        log.DebugFormat("Upsert cas. key: {0}, cas: {1}", key, cas);
                        insertResult = bucket.Upsert<T>(key, value, expiration);
                    }
                }

                if (insertResult != null)
                {
                    if (insertResult.Exception != null)
                        HandleException(key, insertResult);

                    if (insertResult.Status == Couchbase.IO.ResponseStatus.Success)
                        result = insertResult.Success;
                    else
                    {
                        eResultStatus status = eResultStatus.ERROR;
                        HandleStatusCode(insertResult, ref status, key);

                        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, cbDescription))
                        {
                            if (cas > 0)
                                insertResult = bucket.Upsert<T>(key, value, cas, expiration);
                            else
                                insertResult = bucket.Upsert<T>(key, value, expiration);

                            if (insertResult.Status == Couchbase.IO.ResponseStatus.Success)
                                result = insertResult.Success;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchbaseManager - Failed Set<T> with key = {0}, error = {1}", key, ex);
            }
            return result;
        }

        public bool Unlock(string key, ulong cas)
        {
            bool result = false;

            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);
                IOperationResult unlockResult = bucket.Unlock(key, cas);

                if (unlockResult != null)
                {
                    if (unlockResult.Exception != null)
                        HandleException(key, unlockResult);

                    if (unlockResult.Status == Couchbase.IO.ResponseStatus.Success)
                        result = true;
                    else
                    {
                        eResultStatus status = eResultStatus.ERROR;
                        HandleStatusCode(unlockResult, ref status, key);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchbaseManager - Failed Unlock with key = {0}, error = {1}", key, ex);
            }
            return result;
        }

        public T Get<T>(string key, bool asJson = false)
        {
            T result = default(T);

            if (asJson)
                return this.GetJsonAsT<T>(key);

            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);
                IOperationResult<T> getResult = null;

                string cbDescription = string.Format("bucket: {0}; key: {1}", bucketName, key);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.SELECT, Database = cbDescription })
                {
                    getResult = bucket.Get<T>(key);
                }

                if (getResult != null)
                {
                    getResult = HandleNodeUnavailable(key, bucket, getResult, cbDescription);

                    if (getResult.Exception != null && getResult.Status != Couchbase.IO.ResponseStatus.KeyNotFound)
                    {
                        HandleException(key, getResult);
                    }

                    if (getResult.Status == Couchbase.IO.ResponseStatus.Success)
                    {
                        result = getResult.Value;
                    }
                    else
                    {
                        eResultStatus status = eResultStatus.ERROR;
                        HandleStatusCode(getResult, ref status, key);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchbaseManager - Failed Get on bucket = {0} on key = {1}, ex = {2}", bucketName, key, ex);
            }

            return result;
        }

        public T Get<T>(string key, out eResultStatus status)
        {
            T result = default(T);
            status = eResultStatus.ERROR;

            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);
                IOperationResult<T> getResult = null;

                string cbDescription = string.Format("bucket: {0}; key: {1}", bucketName, key);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.SELECT, Database = cbDescription })
                {
                    getResult = bucket.Get<T>(key);
                }

                if (getResult != null)
                {
                    getResult = HandleNodeUnavailable(key, bucket, getResult, cbDescription);

                    if (getResult.Exception != null && getResult.Status != Couchbase.IO.ResponseStatus.KeyNotFound)
                        HandleException(key, getResult);

                    if (getResult.Status == Couchbase.IO.ResponseStatus.Success)
                    {
                        result = getResult.Value;
                        status = eResultStatus.SUCCESS;
                    }
                    else
                    {
                        HandleStatusCode(getResult, ref status, key);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchbaseManager - Failed Get on bucket = {0} on key = {1}, ex = {2}", bucketName, key, ex);
            }

            return result;
        }

        public T Get<T>(string key, out eResultStatus status, JsonSerializerSettings jsonSerializerSettings)
        {
            T result = default(T);
            status = eResultStatus.ERROR;

            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);
                IOperationResult<string> getResult = null;

                string cbDescription = string.Format("bucket: {0}; key: {1}", bucketName, key);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.SELECT, Database = cbDescription })
                {
                    getResult = bucket.Get<string>(key);
                }

                if (getResult != null)
                {
                    getResult = HandleNodeUnavailable(key, bucket, getResult, cbDescription);

                    if (getResult.Exception != null && getResult.Status != Couchbase.IO.ResponseStatus.KeyNotFound)
                        HandleException(key, getResult);

                    if (getResult.Status == Couchbase.IO.ResponseStatus.Success)
                    {
                        if (!string.IsNullOrEmpty(getResult.Value))
                        {
                            result = JsonToObject<T>(getResult.Value, jsonSerializerSettings);
                            status = eResultStatus.SUCCESS;
                        }
                    }
                    else
                    {
                        HandleStatusCode(getResult, ref status, key);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchbaseManager - Failed Get on bucket = {0} on key = {1}, ex = {2}", bucketName, key, ex);
            }

            return result;
        }

        public bool Get<T>(string key, ref T result)
        {
            bool res = false;
            eResultStatus status = eResultStatus.ERROR;

            try
            {
                result = Get<T>(key, out status);
                res = status == eResultStatus.SUCCESS;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchbaseManager - Failed Get on bucket = {0} on key = {1}, ex = {2}", bucketName, key, ex);
            }

            return res;
        }

        public bool Get<T>(string key, ref T result, JsonSerializerSettings jsonSerializerSettings = null)
        {
            bool res = false;
            eResultStatus status = eResultStatus.ERROR;

            try
            {
                if (jsonSerializerSettings != null)
                {
                    result = Get<T>(key, out status, jsonSerializerSettings);
                }
                else
                {
                    result = Get<T>(key, out status);
                }
                res = status == eResultStatus.SUCCESS;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchbaseManager - Failed Get on bucket = {0} on key = {1}, ex = {2}", bucketName, key, ex);
            }

            return res;
        }

        public T Get<T>(string key, out eResultStatus status, int inMemoryCacheTTL)
        {
            T result = default(T);
            status = eResultStatus.ERROR;

            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);
                IOperationResult<T> getResult = null;

                string cbDescription = string.Format("bucket: {0}; key: {1}", bucketName, key);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.SELECT, Database = cbDescription })
                {
                    getResult = bucket.Get<T>(key);
                }

                if (getResult != null)
                {
                    getResult = HandleNodeUnavailable(key, bucket, getResult, cbDescription);

                    if (getResult.Exception != null && getResult.Status != Couchbase.IO.ResponseStatus.KeyNotFound)
                        HandleException(key, getResult);

                    if (getResult.Status == Couchbase.IO.ResponseStatus.Success)
                    {
                        result = getResult.Value;
                        status = eResultStatus.SUCCESS;
                    }
                    else
                        HandleStatusCode(getResult, ref status, key);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchbaseManager - Failed Get on bucket = {0} on key = {1}, ex = {2}", bucketName, key, ex);
            }

            return result;
        }

        public T Get<T>(string key, bool withLock, out ulong cas, out eResultStatus status)
        {
            T result = default(T);
            cas = 0;
            status = eResultStatus.ERROR;

            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);
                IOperationResult<T> getResult = null;

                string cbDescription = string.Format("bucket: {0}; key: {1}", bucketName, key);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.SELECT, Database = cbDescription })
                {
                    if (withLock)
                    {
                        getResult = bucket.GetAndLock<T>(key, TimeSpan.FromSeconds(GET_LOCK_TS_SECONDS));
                        log.DebugFormat("GET locking {0}, cas: {1}", key, getResult.Cas);
                    }
                    else
                    {
                        getResult = bucket.Get<T>(key);
                        log.DebugFormat("GET not locking {0}", key);
                    }
                }

                if (getResult != null)
                {
                    getResult = HandleNodeUnavailable(key, bucket, getResult, cbDescription);

                    if (getResult.Exception != null && getResult.Status != Couchbase.IO.ResponseStatus.KeyNotFound)
                        HandleException(key, getResult);

                    if (getResult.Status == Couchbase.IO.ResponseStatus.Success)
                    {
                        result = getResult.Value;
                        cas = getResult.Cas;
                        status = eResultStatus.SUCCESS;
                    }
                    else
                        HandleStatusCode(getResult, ref status, key);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchbaseManager - Failed GetWithLock on bucket = {0} on key = {1}, ex = {2}", bucketName, key, ex);
            }

            return result;
        }

        public bool Remove(string key, ulong cas = 0)
        {
            bool result = false;

            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);

                IOperationResult removeResult;
                string cbDescription = string.Format("bucket: {0}; key: {1}", bucketName, key);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.DELETE, Database = cbDescription })
                {
                    if (cas == 0)
                        removeResult = bucket.Remove(key);
                    else
                        removeResult = bucket.Remove(key, cas);
                }

                if (removeResult.Success)
                {
                    result = true;
                }
                // if key is already deleted - we regard this remove as successful
                else if (removeResult.Status == Couchbase.IO.ResponseStatus.KeyNotFound)
                {
                    result = true;
                }
                else
                {
                    log.ErrorFormat("Error while trying to delete document. key: {0}, CAS: {1}. CB response: {2}", key, cas, JsonConvert.SerializeObject(removeResult));
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchbaseManager - Failed remove on bucket = {0} on key = {1}, ex = {2}", bucketName, key, ex);
            }

            return result;
        }

        public async Task<bool> RemoveAsync(string key)
        {
            try
            {
                var bucket = await ClusterHelper.GetBucketAsync(bucketName);
                IOperationResult operationResult;
                using (new KMonitor(Events.eEvent.EVENT_COUCHBASE)
                    {
                        QueryType = KLogEnums.eDBQueryType.DELETE,
                        Database = $"bucket: {bucketName}; key: {key}"
                    })
                {
                    operationResult = await bucket.RemoveAsync(key);
                }

                if (operationResult == null)
                {
                    return false;
                }

                if (operationResult.Success)
                {
                    return true;
                }
                // if key is already deleted - we regard this remove as successful
                if (operationResult.Status == Couchbase.IO.ResponseStatus.KeyNotFound)
                {
                    return true;
                }

                log.ErrorFormat("Error while trying to delete document. key: {0}. CB response: {2}", key, JsonConvert.SerializeObject(operationResult));
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchbaseManager - Failed remove on bucket = {0} on key = {1}, ex = {2}", bucketName, key, ex);
            }

            return false;
        }

        public T GetWithVersion<T>(string key, out ulong version, bool asJson = false)
        {
            version = 0;
            T result = default(T);

            if (asJson)
                return GetJsonAsTWithVersion<T>(key, out version);

            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);

                IOperationResult<T> getResult;

                string cbDescription = string.Format("bucket: {0}; key: {1}", bucketName, key);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.SELECT, Database = cbDescription })
                {
                    getResult = bucket.Get<T>(key);
                }

                if (getResult != null)
                {
                    getResult = HandleNodeUnavailable(key, bucket, getResult, cbDescription);

                    if (getResult.Exception != null && getResult.Status != Couchbase.IO.ResponseStatus.KeyNotFound)
                        HandleException(key, getResult);

                    if (getResult.Status == Couchbase.IO.ResponseStatus.Success)
                    {
                        result = getResult.Value;
                        version = getResult.Cas;
                    }
                    else
                    {
                        eResultStatus status = eResultStatus.ERROR;
                        HandleStatusCode(getResult, ref status, key);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchbaseManager - Failed GetWithVersion on bucket = {0} on key = {1}, ex = {2}", bucketName, key, ex);
            }

            return result;
        }

        public T GetWithVersion<T>(string key, out ulong version, JsonSerializerSettings jsonSerializerSettings)
        {
            version = 0;
            T result = default(T);

            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);

                IOperationResult<string> getResult;

                string cbDescription = string.Format("bucket: {0}; key: {1}", bucketName, key);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.SELECT, Database = cbDescription })
                {
                    getResult = bucket.Get<string>(key);
                }

                if (getResult != null)
                {
                    getResult = HandleNodeUnavailable(key, bucket, getResult, cbDescription);

                    if (getResult.Exception != null && getResult.Status != Couchbase.IO.ResponseStatus.KeyNotFound)
                        HandleException(key, getResult);

                    if (getResult.Status == Couchbase.IO.ResponseStatus.Success)
                    {
                        if (!string.IsNullOrEmpty(getResult.Value))
                        {
                            result = JsonToObject<T>(getResult.Value, jsonSerializerSettings);
                            version = getResult.Cas;
                        }
                    }
                    else
                    {
                        eResultStatus status = eResultStatus.ERROR;
                        HandleStatusCode(getResult, ref status, key);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchbaseManager - Failed GetWithVersion on bucket = {0} on key = {1}, ex = {2}", bucketName, key, ex);
            }

            return result;
        }

        public bool GetWithVersion<T>(string key, out ulong version, ref T result)
        {
            bool res = false;
            version = 0;
            eResultStatus status = eResultStatus.ERROR;
            try
            {
                result = GetWithVersion<T>(key, out version, out status);
                res = status == eResultStatus.SUCCESS;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchbaseManager - Failed Get on bucket = {0} on key = {1}, ex = {2}", bucketName, key, ex);
            }

            return res;
        }

        public bool GetWithVersion<T>(string key, out ulong version, ref T result, JsonSerializerSettings jsonSerializerSettings)
        {
            bool res = false;
            version = 0;
            eResultStatus status = eResultStatus.ERROR;
            try
            {
                result = GetWithVersion<T>(key, out version, out status, jsonSerializerSettings);
                res = status == eResultStatus.SUCCESS;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchbaseManager - Failed Get on bucket = {0} on key = {1}, ex = {2}", bucketName, key, ex);
            }

            return res;
        }

        public T GetWithVersion<T>(string key, out ulong version, out eResultStatus status, bool asJson = false)
        {
            version = 0;
            T result = default(T);
            status = eResultStatus.ERROR;


            if (asJson)
                return GetJsonAsTWithVersion<T>(key, out version, out status);

            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);

                IOperationResult<T> getResult;

                string cbDescription = string.Format("bucket: {0}; key: {1}", bucketName, key);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.SELECT, Database = cbDescription })
                {
                    getResult = bucket.Get<T>(key);
                }

                if (getResult != null)
                {
                    getResult = HandleNodeUnavailable(key, bucket, getResult, cbDescription);

                    if (getResult.Exception != null && getResult.Status != Couchbase.IO.ResponseStatus.KeyNotFound)
                        HandleException(key, getResult);

                    if (getResult.Status == Couchbase.IO.ResponseStatus.Success)
                    {
                        result = getResult.Value;
                        version = getResult.Cas;
                        status = eResultStatus.SUCCESS;
                    }
                    else
                    {
                        HandleStatusCode(getResult, ref status, key);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchbaseManager - Failed GetWithVersion on bucket = {0} on key = {1}, ex = {2}", bucketName, key, ex);
            }

            return result;
        }

        public T GetWithVersion<T>(string key, out ulong version, out eResultStatus status, JsonSerializerSettings jsonSerializerSettings)
        {
            version = 0;
            T result = default(T);
            status = eResultStatus.ERROR;

            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);

                IOperationResult<string> getResult;

                string cbDescription = string.Format("bucket: {0}; key: {1}", bucketName, key);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.SELECT, Database = cbDescription })
                {
                    getResult = bucket.Get<string>(key);
                }

                if (getResult != null)
                {
                    getResult = HandleNodeUnavailable(key, bucket, getResult, cbDescription);

                    if (getResult.Exception != null && getResult.Status != Couchbase.IO.ResponseStatus.KeyNotFound)
                        HandleException(key, getResult);

                    if (getResult.Status == Couchbase.IO.ResponseStatus.Success)
                    {
                        if (!string.IsNullOrEmpty(getResult.Value))
                        {
                            result = JsonToObject<T>(getResult.Value, jsonSerializerSettings);
                            version = getResult.Cas;
                            status = eResultStatus.SUCCESS;
                        }
                    }
                    else
                    {
                        HandleStatusCode(getResult, ref status, key);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchbaseManager - Failed GetWithVersion on bucket = {0} on key = {1}, ex = {2}", bucketName, key, ex);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="version"></param>
        /// <param name="expiration">TTL in seconds</param>
        /// <returns></returns>
        public bool SetWithVersion(string key, object value, ulong version, uint expiration = 0, bool asJson = false)
        {
            bool result = false;

            var bucket = ClusterHelper.GetBucket(bucketName);
            IOperationResult setResult;
            expiration = FixExpirationTime(expiration);

            string cbDescription = string.Format("bucket: {0}; key: {1}; expiration: {2} seconds", bucketName, key, expiration);
            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.UPDATE, Database = cbDescription })
            {
                if (!asJson)
                    setResult = bucket.Upsert(key, value, version, expiration);
                else
                {
                    string serializedValue = ObjectToJson(value);
                    setResult = bucket.Upsert(key, serializedValue, version, expiration);
                }
            }

            if (setResult != null)
            {
                bool shouldRetry = true;

                if (setResult.Exception != null)
                {
                    shouldRetry = false;

                    if (!(setResult.Exception is CasMismatchException))
                        HandleException(key, setResult);
                }

                if (setResult.Status == Couchbase.IO.ResponseStatus.Success)
                    result = setResult.Success;
                else
                {
                    eResultStatus status = eResultStatus.ERROR;
                    HandleStatusCode(setResult, ref status, key);

                    if (shouldRetry)
                    {
                        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, cbDescription))
                        {
                            if (!asJson)
                                setResult = bucket.Upsert(key, value, version, expiration);
                            else
                            {
                                string serializedValue = ObjectToJson(value);
                                setResult = bucket.Upsert(key, serializedValue, version, expiration);
                            }
                        }
                    }
                }
            }

            return result;
        }
        
        public bool SetWithVersion<T>(string key, T content, ulong version, uint expiration)
        {
            return SetWithVersion(key, content, version, expiration, asJson: false);
        }

        public bool SetWithVersion(string key, object value, ulong version, JsonSerializerSettings jsonSerializerSettings, uint expiration = 0)
        {
            bool result = false;

            var bucket = ClusterHelper.GetBucket(bucketName);
            IOperationResult setResult;
            expiration = FixExpirationTime(expiration);

            string cbDescription = string.Format("bucket: {0}; key: {1}; expiration: {2} seconds", bucketName, key, expiration);
            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.UPDATE, Database = cbDescription })
            {
                string serializedValue = ObjectToJson(value, jsonSerializerSettings);
                setResult = bucket.Upsert(key, serializedValue, version, expiration);
            }

            if (setResult != null)
            {
                bool shouldRetry = true;

                if (setResult.Exception != null)
                {
                    shouldRetry = false;

                    if (!(setResult.Exception is CasMismatchException))
                        HandleException(key, setResult);
                }

                if (setResult.Status == Couchbase.IO.ResponseStatus.Success)
                    result = setResult.Success;
                else
                {
                    eResultStatus status = eResultStatus.ERROR;
                    HandleStatusCode(setResult, ref status, key);

                    if (shouldRetry)
                    {
                        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, cbDescription))
                        {
                            string serializedValue = ObjectToJson(value, jsonSerializerSettings);
                            setResult = bucket.Upsert(key, serializedValue, version, expiration);
                        }
                    }
                }
            }

            return result;
        }

        public bool SetWithVersion(string key, object value, ulong version, out ulong newVersion, uint expiration = 0, bool asJson = false)
        {
            bool result = false;
            newVersion = 0;

            var bucket = ClusterHelper.GetBucket(bucketName);
            IOperationResult setResult;
            expiration = FixExpirationTime(expiration);

            string cbDescription = string.Format("bucket: {0}; key: {1}; expiration: {2} seconds", bucketName, key, expiration);
            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.UPDATE, Database = cbDescription })
            {
                if (!asJson)
                    setResult = bucket.Upsert(key, value, version, expiration);
                else
                {
                    string serializedValue = ObjectToJson(value);
                    setResult = bucket.Upsert(key, serializedValue, version, expiration);
                }
            }

            if (setResult != null)
            {
                bool shouldRetry = true;

                if (setResult.Exception != null)
                {
                    shouldRetry = false;

                    if (!(setResult.Exception is CasMismatchException))
                        HandleException(key, setResult);
                }

                if (setResult.Status == Couchbase.IO.ResponseStatus.Success)
                {
                    result = setResult.Success;
                    newVersion = setResult.Cas;
                }
                else
                {
                    eResultStatus status = eResultStatus.ERROR;
                    HandleStatusCode(setResult, ref status, key);

                    if (shouldRetry)
                    {
                        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, cbDescription))
                        {
                            if (!asJson)
                                setResult = bucket.Upsert(key, value, version, expiration);
                            else
                            {
                                string serializedValue = ObjectToJson(value);
                                setResult = bucket.Upsert(key, serializedValue, version, expiration);
                            }
                        }
                    }

                    newVersion = setResult.Cas;
                }
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="version"></param>
        /// <param name="expiration">TTL in seconds</param>
        /// <returns></returns>
        public bool SetWithVersion<T>(string key, T value, ulong version, uint expiration = 0, bool asJson = false)
        {
            bool result = false;

            var bucket = ClusterHelper.GetBucket(bucketName);

            IOperationResult setResult;
            expiration = FixExpirationTime(expiration);

            string cbDescription = string.Format("bucket: {0}; key: {1}; expiration: {2} seconds", bucketName, key, expiration);
            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.UPDATE, Database = cbDescription })
            {
                if (!asJson)
                    setResult = bucket.Upsert<T>(key, value, version, expiration);
                else
                {
                    string serializedValue = ObjectToJson(value);
                    setResult = bucket.Upsert(key, serializedValue, version, expiration);
                }
            }

            if (setResult != null)
            {
                bool shouldRetry = true;

                if (setResult.Exception != null)
                {
                    shouldRetry = false;

                    if (!(setResult.Exception is CasMismatchException))
                        HandleException(key, setResult);
                }

                if (setResult.Status == Couchbase.IO.ResponseStatus.Success)
                    result = setResult.Success;
                else
                {
                    eResultStatus status = eResultStatus.ERROR;
                    HandleStatusCode(setResult, ref status, key);

                    if (shouldRetry)
                    {
                        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, cbDescription))
                        {
                            if (!asJson)
                                setResult = bucket.Upsert<T>(key, value, version, expiration);
                            else
                            {
                                string serializedValue = ObjectToJson(value);
                                setResult = bucket.Upsert(key, serializedValue, version, expiration);
                            }
                        }
                    }
                }
            }

            return result;
        }

        public bool SetWithVersion<T>(string key, T value, ulong version, out ulong newVersion, uint expiration = 0, bool asJson = false)
        {
            bool result = false;
            newVersion = 0;

            var bucket = ClusterHelper.GetBucket(bucketName);
            IOperationResult setResult;
            expiration = FixExpirationTime(expiration);

            string cbDescription = string.Format("bucket: {0}; key: {1}; expiration: {2} seconds", bucketName, key, expiration);
            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.UPDATE, Database = cbDescription })
            {
                if (!asJson)
                    setResult = bucket.Upsert<T>(key, value, version, expiration);
                else
                {
                    string serializedValue = ObjectToJson(value);
                    setResult = bucket.Upsert(key, serializedValue, version, expiration);
                }
            }

            if (setResult != null)
            {
                bool shouldRetry = true;

                if (setResult.Exception != null)
                {
                    shouldRetry = false;

                    if (!(setResult.Exception is CasMismatchException))
                        HandleException(key, setResult);
                }

                if (setResult.Status == Couchbase.IO.ResponseStatus.Success)
                {
                    result = setResult.Success;
                    newVersion = setResult.Cas;
                }
                else
                {
                    eResultStatus status = eResultStatus.ERROR;
                    HandleStatusCode(setResult, ref status, key);

                    if (shouldRetry)
                    {
                        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, cbDescription))
                        {
                            if (!asJson)
                                setResult = bucket.Upsert<T>(key, value, version, expiration);
                            else
                            {
                                string serializedValue = ObjectToJson(value);
                                setResult = bucket.Upsert(key, serializedValue, version, expiration);
                            }
                        }

                        newVersion = setResult.Cas;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="version"></param>
        /// <param name="numOfRetries"></param>
        /// <param name="retryInterval"></param>
        /// <param name="expiration">TTL in seconds</param>
        /// <returns></returns>
        public bool SetWithVersionWithRetry<T>(string key, object value, ulong version, int numOfRetries, int retryInterval, uint expiration = 0, bool asJson = false)
        {
            bool result = false;

            if (numOfRetries >= 0)
            {
                bool operationResult = SetWithVersion(key, value, version, expiration, asJson);
                if (!operationResult)
                {
                    numOfRetries--;
                    Thread.Sleep(retryInterval);

                    ulong newVersion;
                    var getResult = GetWithVersion<T>(key, out newVersion, asJson);

                    result = SetWithVersionWithRetry<T>(key, value, newVersion, numOfRetries, retryInterval, expiration, asJson);
                }
                else
                    result = true;
            }

            return result;
        }

        public bool SetWithVersionWithRetry<T>(string key, object value, ulong version, int numOfRetries, int retryInterval, JsonSerializerSettings jsonSerializerSettings, uint expiration = 0)
        {
            bool result = false;

            if (numOfRetries >= 0)
            {
                bool operationResult = SetWithVersion(key, value, version, jsonSerializerSettings, expiration);
                if (!operationResult)
                {
                    numOfRetries--;
                    Thread.Sleep(retryInterval);

                    ulong newVersion;
                    var getResult = GetWithVersion<T>(key, out newVersion, jsonSerializerSettings);

                    result = SetWithVersionWithRetry<T>(key, value, newVersion, numOfRetries, retryInterval, jsonSerializerSettings, expiration);
                }
                else
                    result = true;
            }

            return result;
        }

        public IDictionary<string, T> GetValues<T>(List<string> keys, bool shouldAllowPartialQuery = false, bool asJson = false)
        {
            IDictionary<string, T> result = null;

            if (asJson)
            {
                IDictionary<string, string> jsonValues = this.GetValues<string>(keys, shouldAllowPartialQuery);

                if (jsonValues != null)
                {
                    result = new Dictionary<string, T>();

                    // Convert all strings to objects
                    foreach (var jsonValue in jsonValues)
                        result.Add(jsonValue.Key, JsonToObject<T>(jsonValue.Value));
                }

                return result;
            }

            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);
                IDictionary<string, IOperationResult<T>> getResult;

                string cbDescription = string.Format("bucket: {0}, count: {1} keys: {2}", bucket.Name, keys.Count, string.Join(",", keys.ToArray().Take(20)));
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.SELECT, Database = cbDescription })
                {
                    getResult = bucket.Get<T>(keys, TimeSpan.FromMilliseconds(SEND_TIMEOUT_DEFAULT_MILLISECONDS));
                }

                // Success until proven otherwise
                Couchbase.IO.ResponseStatus status = Couchbase.IO.ResponseStatus.Success;

                foreach (var item in getResult)
                {
                    // handle exception if there is one
                    if (item.Value.Exception != null && item.Value.Status != Couchbase.IO.ResponseStatus.KeyNotFound)
                        HandleException(item.Key, item.Value);

                    // If any of the rows wasn't successful, maybe we need to break - depending if we allow partials or not
                    if (item.Value.Status != Couchbase.IO.ResponseStatus.Success)
                    {
                        if (item.Value.Status == Couchbase.IO.ResponseStatus.KeyNotFound || item.Value.Status == Couchbase.IO.ResponseStatus.OperationTimeout)
                        {
                            log.LogTrace($"Couchbase manager: failed to get key {item.Key}, status {item.Value.Status}");
                        }
                        else
                        {
                            log.LogTrace($"Couchbase manager: failed to get key {item.Key}, status {item.Value.Status}, message {item.Value.Message}");

                            // Throw exception if there is one
                            if (item.Value.Exception != null)
                                throw item.Value.Exception;
                        }

                        status = item.Value.Status;

                        if (!shouldAllowPartialQuery)
                            break;
                    }
                    else
                    {
                        log.LogTrace($"Couchbase manager: GetValues success - get key {item.Key}, status {item.Value.Status}");
                    }
                }
                
                //shouldAllowPartialQuery will return result only if our query return something
                //example: shouldAllowPartialQuery with single key but key doesn't exist - shouldn't return anything
                if ((shouldAllowPartialQuery && getResult.Keys.Count > 0) || status == Couchbase.IO.ResponseStatus.Success)
                {
                    // if successful - build dictionary based on execution result
                    result = new Dictionary<string, T>();

                    foreach (var item in getResult)
                    {
                        if (item.Value.Status == Couchbase.IO.ResponseStatus.Success)
                        {
                            result.Add(item.Key, item.Value.Value);
                        }
                    }
                }
                else
                {
                    log.ErrorFormat("Error while executing action on CB. Status code = {0}; Status = {1}", (int)status, status.ToString());
                }
            }
            catch (Exception ex)
            {
                if (keys != null && keys.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var item in keys)
                        sb.Append(item + " ");

                    log.ErrorFormat("Error while getting the following keys from CB: {0}. Exception: {1}", sb.ToString(), ex);
                }
                else
                    log.Error("Error while getting keys from CB", ex);
            }

            return result;
        }

        public IDictionary<string, T> GetValues<T>(List<string> keys, JsonSerializerSettings jsonSerializerSettings, bool shouldAllowPartialQuery = false)
        {
            IDictionary<string, T> result = null;

            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);
                IDictionary<string, IOperationResult<string>> getResult;

                string cbDescription = string.Format("bucket: {0}, keys: {1}", bucket.Name, string.Join(",", keys.ToArray().Take(20)));
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.SELECT, Database = cbDescription })
                {
                    getResult = bucket.Get<string>(keys, TimeSpan.FromMilliseconds(SEND_TIMEOUT_DEFAULT_MILLISECONDS));
                }

                // Success until proven otherwise
                Couchbase.IO.ResponseStatus status = Couchbase.IO.ResponseStatus.Success;

                foreach (var item in getResult)
                {
                    // Handle exception if there is one
                    if (item.Value.Exception != null && item.Value.Status != Couchbase.IO.ResponseStatus.KeyNotFound)
                        HandleException(item.Key, item.Value);

                    // If any of the rows wasn't successful, maybe we need to break - depending if we allow partials or not
                    if (item.Value.Status != Couchbase.IO.ResponseStatus.Success)
                    {
                        if (item.Value.Status == Couchbase.IO.ResponseStatus.KeyNotFound)
                        {
                            log.DebugFormat("Couchbase manager: failed to get key {0}, status {1}", item.Key, item.Value.Status);
                        }
                        else
                        {
                            log.DebugFormat("Couchbase manager: failed to get key {0}, status {1}, message {2}", item.Key, item.Value.Status, item.Value.Message);
                        }

                        status = item.Value.Status;

                        if (!shouldAllowPartialQuery)
                            break;
                    }
                    else
                    {
                        log.DebugFormat("Couchbase manager: GetValues success - get key {0}, status {1}", item.Key, item.Value.Status);
                    }
                }

                if (shouldAllowPartialQuery || status == Couchbase.IO.ResponseStatus.Success)
                {
                    // if successful - build dictionary based on execution result
                    result = new Dictionary<string, T>();

                    foreach (var item in getResult)
                    {
                        if (item.Value.Status == Couchbase.IO.ResponseStatus.Success && !string.IsNullOrEmpty(item.Value.Value))
                        {
                            result.Add(item.Key, JsonToObject<T>(item.Value.Value, jsonSerializerSettings));
                        }
                    }
                }
                else
                {
                    log.ErrorFormat("Error while executing action on CB. Status code = {0}; Status = {1}", (int)status, status.ToString());
                }
            }
            catch (Exception ex)
            {
                if (keys != null && keys.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var item in keys)
                        sb.Append(item + " ");

                    log.ErrorFormat("Error while getting the following keys from CB: {0}. Exception: {1}", sb.ToString(), ex);
                }
                else
                    log.Error("Error while getting keys from CB", ex);
            }

            return result;
        }

        public bool SetJson<T>(string key, T value, uint expiration = 0)
        {
            var json = ObjectToJson<T>(value);
            return this.Set(key, json, expiration);
        }

        public T GetJsonAsT<T>(string key)
        {
            T result = default(T);

            var json = Get<string>(key);

            if (!string.IsNullOrEmpty(json))
                result = JsonToObject<T>(json);

            return result;
        }

        public T GetJsonAsTWithVersion<T>(string key, out ulong version)
        {
            T result = default(T);
            var json = GetWithVersion<string>(key, out version);

            if (!string.IsNullOrEmpty(json))
                result = JsonToObject<T>(json);

            return result;
        }

        public T GetJsonAsTWithVersion<T>(string key, out ulong version, out eResultStatus status)
        {
            status = eResultStatus.ERROR;

            T result = default(T);
            var json = GetWithVersion<string>(key, out version, out status);

            if (!string.IsNullOrEmpty(json))
                result = JsonToObject<T>(json);

            return result;
        }

        public bool GetValues<T>(List<string> keys, ref IDictionary<string, T> results, bool shouldAllowPartialQuery = false)
        {
            bool result = false;

            try
            {
                results = GetValues<T>(keys, shouldAllowPartialQuery);

                if (results != null)
                {
                    if (shouldAllowPartialQuery)
                    {
                        result = true;
                    }
                    else
                    {
                        result = keys.Count == results.Count;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error in GetValues<T> from CB while getting the following keys: {0}", string.Join(",", keys)), ex);
            }

            return result;
        }

        public bool GetValues<T>(List<string> keys, ref IDictionary<string, T> results, JsonSerializerSettings jsonSerializerSettings = null, bool shouldAllowPartialQuery = false)
        {
            bool result = false;

            try
            {
                if (jsonSerializerSettings != null)
                {
                    results = GetValues<T>(keys, jsonSerializerSettings, shouldAllowPartialQuery);
                }
                else
                {
                    results = GetValues<T>(keys, shouldAllowPartialQuery);
                }

                if (results != null)
                {
                    if (shouldAllowPartialQuery)
                    {
                        result = true;
                    }
                    else
                    {
                        result = keys.Count == results.Count;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error in GetValues<T> from CB while getting the following keys: {0}", string.Join(",", keys)), ex);
            }

            return result;
        }

        #region View Methods

        /// <summary>
        /// Get specific, typed, objects from view
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="definitions"></param>
        /// <returns></returns>
        public List<T> View<T>(ViewManager definitions)
        {
            long totalNumOfResults = 0;
            return View<T>(definitions, ref totalNumOfResults);
        }

        /// <summary>
        /// Get specific, typed, objects from view including total number of results
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="definitions"></param>
        /// <returns></returns>
        public List<T> View<T>(ViewManager definitions, ref long totalNumOfResults)
        {
            List<T> result = new List<T>();
            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);
                Dictionary<string, int> keysToIndexes = new Dictionary<string, int>();
                List<string> missingKeys = new List<string>();
                T defaultValue = default(T);

                if (definitions.asJson)
                {
                    List<ViewRow<object>> rowsJson = definitions.QueryRows<object>(bucket, ref totalNumOfResults);

                    foreach (var viewRow in rowsJson)
                    {
                        if (viewRow != null)
                        {
                            // If we have a result - convert it to the typed object and add it to list
                            if (null != viewRow.Value)
                                result.Add(JsonToObject<T>(viewRow.Value.ToString()));
                            else
                            {
                                // If we don't - list all missing keys so that we get them later on
                                result.Add(defaultValue);
                                missingKeys.Add(viewRow.Id);
                                keysToIndexes.Add(viewRow.Id, result.Count - 1);
                            }
                        }
                    }
                }
                else
                {
                    List<ViewRow<T>> rowsAsT = definitions.QueryRows<T>(bucket, ref totalNumOfResults);

                    foreach (var viewRow in rowsAsT)
                    {
                        if (viewRow != null)
                        {
                            // If we have a result - simply add it to list
                            if (null != viewRow.Value)
                                result.Add(viewRow.Value);
                            else
                            {
                                // If we don't - list all missing keys so that we get them later on
                                result.Add(defaultValue);
                                missingKeys.Add(viewRow.Id);
                                keysToIndexes.Add(viewRow.Id, result.Count - 1);
                            }
                        }
                    }
                }

                // Get all missing values from Couchbase and fill the list
                var missingValues = GetValues<T>(missingKeys, definitions.allowPartialQuery, definitions.asJson);

                if (missingValues != null)
                {
                    foreach (var currentValue in missingValues)
                    {
                        int index = keysToIndexes[currentValue.Key];
                        result[index] = currentValue.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchbaseManager - " + string.Format("Failed Getting view. error = {0}, ST = {1}", ex.Message, ex.StackTrace), ex);
            }

            return result;
        }

        /// <summary>
        /// Get generic keys and values from view
        /// </summary>
        /// <param name="definitions"></param>
        /// <returns></returns>
        public List<KeyValuePair<object, T1>> ViewKeyValuePairs<T1>(ViewManager definitions)
        {
            List<KeyValuePair<object, T1>> result = new List<KeyValuePair<object, T1>>();

            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);
                if (definitions.asJson)
                {
                    var jsonResults = definitions.QueryKeyValuePairs<object>(bucket);

                    foreach (var jsonResult in jsonResults)
                        result.Add(new KeyValuePair<object, T1>(jsonResult.Key, JsonToObject<T1>(jsonResult.Value.ToString())));
                }
                else
                    result = definitions.QueryKeyValuePairs<T1>(bucket);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchbaseManager - " + string.Format("Failed Getting view. error = {0}, ST = {1}", ex.Message, ex.StackTrace), ex);
            }

            return result;
        }

        /// <summary>
        /// Get only list of document IDs from view
        /// </summary>
        /// <param name="definitions"></param>
        /// <returns></returns>
        public List<string> ViewIds(ViewManager definitions)
        {
            List<string> result = new List<string>();

            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);
                result = definitions.QueryIds(bucket);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchbaseManager - " + string.Format("Failed Getting view. error = {0}, ST = {1}", ex.Message, ex.StackTrace), ex);
            }

            return result;
        }

        /// <summary>
        /// Get the entire view row from view. We emulate a similar class to avoid breaking changes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="definitions"></param>
        /// <param name="shouldRethrowEx"></param>
        /// <returns></returns>
        public List<ViewRow<T>> ViewRows<T>(ViewManager definitions, bool shouldRethrowEx = false)
        {
            List<ViewRow<T>> result = new List<ViewRow<T>>();
            long totalNumOfRes = 0;
            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);
                if (definitions.asJson)
                {

                    var jsonResults = definitions.QueryRows<object>(bucket, ref totalNumOfRes);

                    foreach (var jsonResult in jsonResults)
                    {
                        result.Add(new ViewRow<T>()
                        {
                            Id = jsonResult.Id,
                            Key = jsonResult.Key,
                            Value = JsonToObject<T>(jsonResult.Value.ToString())
                        });
                    }
                }
                else
                    result = definitions.QueryRows<T>(bucket, ref totalNumOfRes);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchbaseManager - " + $"Failed Getting view. error = {ex.Message}, ST = {ex.StackTrace}", ex);
                if (shouldRethrowEx)
                {
                    throw;
                }
            }

            return result;
        }

        /// <summary>
        /// Get the entire view row from view. We emulate a similar class to avoid breaking changes.
        /// If the result couldn't be retrieved, exception will be thrown.
        /// </summary>
        /// <param name="definitions"></param>
        /// <param name="attempts"></param>
        /// <param name="interval"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<ViewRow<T>> ViewRows<T>(ViewManager definitions, int attempts, TimeSpan interval)
        {
            var retryPolicy = Policy.Handle<Exception>().WaitAndRetry(attempts, i => interval);
            var policyResult = retryPolicy.ExecuteAndCapture(() => this.ViewRows<T>(definitions, true));
            if (policyResult.Outcome == OutcomeType.Failure)
            {
                log.ErrorFormat(
                    "CouchbaseManager - " + $"Failed Getting view. error = {policyResult.FinalException.Message}, ST = {policyResult.FinalException.StackTrace}",
                    policyResult.FinalException);
                throw policyResult.FinalException;
            }

            return policyResult.Result;
        }

        /// <summary>
        /// Returns the map-reduce result of a view
        /// </summary>
        /// <param name="definitions"></param>
        /// <returns></returns>
        public int ViewReduce(ViewManager definitions)
        {
            int result = 0;

            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);

                if (definitions != null)
                {
                    definitions.reduce = true;

                    result = definitions.QueryReduce(bucket);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchbaseManager - " + string.Format("Failed Getting view. error = {0}, ST = {1}", ex.Message, ex.StackTrace), ex);
            }

            return result;
        }
        #endregion

        public ulong Increment(string key, ulong delta, uint? ttl = null)
        {
            ulong result = 0;

            var bucket = ClusterHelper.GetBucket(bucketName);
            IOperationResult<ulong> incrementResult = null;

            string cbDescription = string.Format("bucket: {0}; key: {1}", bucketName, key);
            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, null, null, null) { QueryType = KLogEnums.eDBQueryType.COMMAND, Database = cbDescription })
            {
                if (ttl == null)
                    incrementResult = bucket.Increment(key, delta);
                else
                    incrementResult = bucket.Increment(key, delta, 1, (uint)ttl);
            }

            if (incrementResult != null)
            {
                if (incrementResult.Exception != null)
                {
                    HandleException(key, incrementResult);
                }

                if (incrementResult.Status == Couchbase.IO.ResponseStatus.Success)
                {
                    result = incrementResult.Value;
                }
                else
                {
                    eResultStatus status = eResultStatus.ERROR;
                    HandleStatusCode(incrementResult, ref status, key);
                }
            }

            return result;
        }

        #endregion


        #region N1QL

        public List<T> Query<T>(N1QLManager queryManager)
        {
            Exception exceptionToThrow = null;
            List<T> result = new List<T>();

            if (queryManager == null || string.IsNullOrEmpty(queryManager.statement))
            {
                return result;
            }

            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);
                var queryRequest = new QueryRequest();

                #region Build QueryRequest

                // .Statement
                // replace {0} with bucket name - outside people are not supposed to know the bucket name, so they will use {0} instead
                queryRequest.Statement(string.Format(queryManager.statement, bucketName));

                // .AddNamedParameter
                if (queryManager.namedParameters != null)
                {
                    queryRequest.AddNamedParameter(queryManager.namedParameters);
                }

                // .AddPositionalParameter
                if (queryManager.positionalParameters != null)
                {
                    foreach (var parameter in queryManager.positionalParameters)
                    {
                        queryRequest.AddPositionalParameter(parameter);
                    }
                }

                #endregion

                IQueryResult<T> queryResponse;

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, null, null, null)
                {
                    QueryType = KLogEnums.eDBQueryType.SELECT,
                    Database = bucketName,
                    Table = queryRequest.ToString()
                })
                {
                    queryResponse = bucket.Query<T>(queryRequest);
                }

                // Checking the response
                if (queryResponse != null)
                {
                    if (queryResponse.Success)
                    {
                        result = queryResponse.Rows;
                    }

                    #region Handle Errors / Exception

                    if (queryResponse.Exception != null)
                    {
                        log.ErrorFormat("Exception when running N1QL query. Status = {0}, Statement = {1}, ex = {2}",
                            queryResponse.Status, queryManager.statement, queryResponse.Exception);
                    }

                    if (queryResponse.Errors != null && queryResponse.Errors.Count > 0)
                    {
                        foreach (var error in queryResponse.Errors)
                        {
                            log.ErrorFormat("Error when running N1QL query. Status = {0}, error = {2}, Statement = {1},",
                                queryResponse.Status, queryManager.statement, error.Message);
                        }

                        if (queryResponse.Status == QueryStatus.Fatal)
                        {
                            var error = queryResponse.Errors.Last();
                            exceptionToThrow = new Exception(string.Format("Code = {0} Message = {1}",
                                error.Code, error.Message));
                        }
                    }

                    #endregion
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception when running N1QL query. Statement = {0}, ex = {1}",
                    queryManager.statement, ex);
            }

            if (exceptionToThrow != null)
            {
                throw exceptionToThrow;
            }

            return result;
        }

        public T QuerySingleValue<T>(N1QLManager queryManager, string fieldName)
        {
            T result = default(T);

            var queryResult = Query<dynamic>(queryManager);

            if (queryResult != null && queryResult.Count > 0)
            {
                result = Convert.ChangeType(queryResult.First()[fieldName], typeof(T));
            }

            return result;
        }

        public List<T> QueryList<T>(N1QLManager queryManager, string fieldName)
        {
            List<T> result = new List<T>();

            var queryResult = Query<dynamic>(queryManager);

            if (queryResult != null)
            {
                foreach (var item in queryResult)
                {
                    result.Add(Convert.ChangeType(item[fieldName], typeof(T)));
                }
            }

            return result;
        }
        #endregion

    }
}
