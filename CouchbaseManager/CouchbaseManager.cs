using Couchbase;
using Couchbase.Configuration.Client;
using Couchbase.Configuration.Client.Providers;
using Couchbase.Core.Serialization;
using Couchbase.Core.Transcoders;
using CouchBaseExtensions;
using KLogMonitor;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Reflection;
using System.Text;
using System.Threading;

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
        MEMCACHED = 15
    }

    public class CouchbaseManager
    {
        #region Consts

        public const string COUCHBASE_CONFIG = "couchbaseClients/Couchbase";
        public const string COUCHBASE_APP_CONFIG = "CouchbaseSectionMapping";
        private const string TCM_KEY_FORMAT = "cb_{0}.{1}";
        private const double GET_LOCK_TS_SECONDS = 5;

        /// <summary>
        /// Defines duration of a month in seconds, see http://docs.couchbase.com/developer/dev-guide-3.0/doc-expiration.html
        /// </summary>
        private const uint monthInSeconds = 30 * 24 * 60 * 60;

        #endregion

        #region Static Members

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static object locker = new object();
        private static ReaderWriterLockSlim m_oSyncLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        protected static Couchbase.Core.Serialization.DefaultSerializer serializer;

        private static bool IsClusterInitialized
        {
            get;
            set;
        }

        #endregion

        #region Data Members

        private string bucketName;
        private ClientConfiguration clientConfiguration;

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

            this.clientConfiguration = new ClientConfiguration((CouchbaseClientSection)ConfigurationManager.GetSection(COUCHBASE_CONFIG));

            if (fromTcm)
            {
                bucketName = GetBucketName(subSection);
            }
            else if (useApplicationSettingMapping)
            {
                bucketName = GetBucketNameFromSettings(subSection);
            }

            this.clientConfiguration.Transcoder = GetTranscoder;

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

        private ITypeTranscoder GetTranscoder()
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

        /// <summary>
        /// Retrieve  bucketName from web.config or app.conifg. in case no TCM application. 
        ///  configSections should be added to  web.config or app.conifg 
        ///  Sample:
        ///  <configSections>
        ///  <section name="CouchbaseSectionMapping" type="System.Configuration.NameValueFileSectionHandler,System, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" />
        ///  </configSections>
        ///  <CouchbaseSectionMapping><add key="authorization" value="crowdsource" /> </CouchbaseSectionMapping>
        /// </summary>
        /// <param name="subSection">key</param>
        /// <returns>value</returns>
        private string GetBucketNameFromSettings(string subSection)
        {
            string bucketName = string.Empty;
            try
            {
                NameValueCollection section = (NameValueCollection)ConfigurationManager.GetSection(COUCHBASE_APP_CONFIG);
                if (section != null)
                {
                    bucketName = section[subSection];
                    if (string.IsNullOrEmpty(bucketName))
                        log.ErrorFormat("Error getting bucketName for couchbaseBucket:{0}. Not exist", subSection);
                }
                else
                    log.ErrorFormat("Error getting bucketName for couchbaseBucket:{0}. CouchbaseSectionMapping is empty.", subSection);
            }
            catch (Exception exc)
            {
                log.ErrorFormat("Error getting bucketName for couchbaseBucket:{0}. Exception:{1}", subSection, exc);
            }

            return bucketName;
        }

        private string GetBucketName(string couchbaseBucket)
        {
            string bucketName = string.Empty;
            try
            {
                Dictionary<string, string> couchbaseBucketWithBucketNameDic = TCMClient.Settings.Instance.GetValue<Dictionary<string, string>>("CouchbaseSectionMapping");
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

        private void HandleStatusCode(IOperationResult result, string key = "")
        {
            if (result.Status != Couchbase.IO.ResponseStatus.Success)
            {
                // 1 - not found
                if (result.Status == Couchbase.IO.ResponseStatus.KeyNotFound)
                    log.DebugFormat("Could not find key on couchbase: {0}", key);
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
                    break;
                case Couchbase.IO.ResponseStatus.KeyNotFound:
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
                    break;
                case Couchbase.IO.ResponseStatus.OutOfMemory:
                    break;
                case Couchbase.IO.ResponseStatus.Success:
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

        private static string ObjectToJson<T>(T obj)
        {
            if (obj != null)
                return Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.None);
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
                result = DateTimeToUnixTimestamp(expirationDate);
            }

            return result;
        }

        private static uint DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (uint)(dateTime - new DateTime(1970, 1, 1).ToUniversalTime()).TotalSeconds;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiration">TTL in seconds</param>
        /// <returns></returns>
        public bool Add(string key, object value, uint expiration = 0, bool asJson = false)
        {
            bool result = false;

            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);
                expiration = FixExpirationTime(expiration);

                IOperationResult insertResult = null;

                string action = string.Format("Action: Insert; bucket: {0}; key: {1}; expiration: {2} seconds", bucketName, key, expiration);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
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
                    if (insertResult.Exception != null)
                        throw insertResult.Exception;

                    if (insertResult.Status == Couchbase.IO.ResponseStatus.Success)
                        result = insertResult.Success;
                    else
                    {
                        HandleStatusCode(insertResult, key);

                        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
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
                log.ErrorFormat("CouchBaseCache - Failed Add with key = {0}, ex = {1}", key, ex);
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
        public bool Add<T>(string key, T value, uint expiration = 0, bool asJson = false)
        {
            bool result = false;

            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);
                IOperationResult insertResult = null;
                expiration = FixExpirationTime(expiration);

                string action = string.Format("Action: Insert; bucket: {0}; key: {1}; expiration: {2} seconds", bucketName, key, expiration);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
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
                    if (insertResult.Exception != null)
                        throw insertResult.Exception;

                    if (insertResult.Status == Couchbase.IO.ResponseStatus.Success)
                        result = insertResult.Success;
                    else
                    {
                        HandleStatusCode(insertResult, key);

                        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
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
                log.ErrorFormat("CouchBaseCache - Failed Add with key = {0}, ex = {1}", key, ex);
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

                string action = string.Format("Action: Upsert; bucket: {0}; key: {1}; expiration: {2} seconds", bucketName, key, expiration);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
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
                        throw insertResult.Exception;

                    if (insertResult.Status == Couchbase.IO.ResponseStatus.Success)
                        result = insertResult.Success;
                    else
                    {
                        HandleStatusCode(insertResult, key);

                        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
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
                log.ErrorFormat("CouchBaseCache - Failed Set with key = {0}, ex = {1}", key, ex);
            }
            return result;
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

                string action = string.Format("Action: Upsert; bucket: {0}; key: {1}; expiration: {2} seconds", bucketName, key, expiration);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
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
                        throw insertResult.Exception;

                    if (insertResult.Status == Couchbase.IO.ResponseStatus.Success)
                        result = insertResult.Success;
                    else
                    {
                        HandleStatusCode(insertResult, key);

                        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
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
                log.ErrorFormat("CouchBaseCache - Failed Set<T> with key = {0}, error = {1}", key, ex);
            }
            return result;
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

                string action = string.Format("Action: Upsert; bucket: {0}; key: {1}; expiration: {2} seconds", bucketName, key, expiration);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
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
                        throw insertResult.Exception;

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
                        HandleStatusCode(insertResult, key);

                        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
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
                log.ErrorFormat("CouchBaseCache - Failed Set<T> with key = {0}, error = {1}", key, ex);
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

                string action = string.Format("Action: Upsert; bucket: {0}; key: {1}; expiration: {2} seconds", bucketName, key, expiration);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
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
                        throw insertResult.Exception;

                    if (insertResult.Status == Couchbase.IO.ResponseStatus.Success)
                    {
                        result = insertResult.Success;

                        //log.DebugFormat("SET before unlocking {0}, cas: {1}", key, cas);
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
                        HandleStatusCode(insertResult, key);

                        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                        {
                            if (cas > 0)
                                insertResult = bucket.Upsert<T>(key, value, cas, expiration);
                            else
                                insertResult = bucket.Upsert<T>(key, value, expiration);

                            if (insertResult.Status == Couchbase.IO.ResponseStatus.Success)
                            {
                                result = insertResult.Success;

                                //log.Debug("before lock2");
                                //if (unlock && cas > 0)
                                //{
                                //    var unlockResult = bucket.Unlock(key, cas);
                                //    if (unlockResult.Success)
                                //        log.DebugFormat("SET after unlocking {0}, cas: {1}", key, cas);
                                //    else
                                //        log.DebugFormat("failed to unlock - key: {0}, cas: {1}, error = {2}", key, cas, unlockResult.Status.ToString());


                                //    log.Debug("after lock2");
                                //}
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchBaseCache - Failed Set<T> with key = {0}, error = {1}", key, ex);
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
                        throw unlockResult.Exception;

                    if (unlockResult.Status == Couchbase.IO.ResponseStatus.Success)
                        result = true;
                    else
                        HandleStatusCode(unlockResult, key);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchBaseCache - Failed Unlock with key = {0}, error = {1}", key, ex);
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

                string action = string.Format("Action: Get; bucket: {0}; key: {1}", bucketName, key);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                {
                    getResult = bucket.Get<T>(key);
                }

                if (getResult != null)
                {
                    if (getResult.Exception != null)
                        throw getResult.Exception;

                    if (getResult.Status == Couchbase.IO.ResponseStatus.Success)
                        result = getResult.Value;
                    else
                        HandleStatusCode(getResult, key);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchBaseCache - Failed Get with key = {0}, ex = {1}", key, ex);
            }

            return result;
        }

        public T Get<T>(string key, out Couchbase.IO.ResponseStatus status)
        {
            T result = default(T);
            status = Couchbase.IO.ResponseStatus.None;

            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);
                IOperationResult<T> getResult = null;

                string action = string.Format("Action: Get; bucket: {0}; key: {1}", bucketName, key);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                {
                    getResult = bucket.Get<T>(key);
                }

                if (getResult != null)
                {
                    status = getResult.Status;
                    if (getResult.Exception != null)
                        throw getResult.Exception;

                    if (getResult.Status == Couchbase.IO.ResponseStatus.Success)
                        result = getResult.Value;
                    else
                        HandleStatusCode(getResult, key);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchBaseCache - Failed Get with key = {0}, ex = {1}", key, ex);
            }

            return result;
        }

        public bool Get<T>(string key, ref T result)
        {
            bool res = false;
            Couchbase.IO.ResponseStatus status = Couchbase.IO.ResponseStatus.None;
            try
            {
                result = Get<T>(key, out status);
                res = status == Couchbase.IO.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchBaseCache - Failed Get with key = {0}, ex = {1}", key, ex);
            }

            return res;
        }

        public T Get<T>(string key, out Couchbase.IO.ResponseStatus status, int inMemoryCacheTTL)
        {
            T result = default(T);
            status = Couchbase.IO.ResponseStatus.None;

            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);
                IOperationResult<T> getResult = null;

                string action = string.Format("Action: Get; bucket: {0}; key: {1}", bucketName, key);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                {
                    getResult = bucket.Get<T>(key);
                }

                if (getResult != null)
                {
                    status = getResult.Status;
                    if (getResult.Exception != null)
                        throw getResult.Exception;

                    if (getResult.Status == Couchbase.IO.ResponseStatus.Success)
                        result = getResult.Value;
                    else
                        HandleStatusCode(getResult, key);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchBaseCache - Failed Get with key = {0}, ex = {1}", key, ex);
            }

            return result;
        }

        public T Get<T>(string key, bool withLock, out ulong cas, out Couchbase.IO.ResponseStatus status)
        {
            T result = default(T);
            cas = 0;
            status = Couchbase.IO.ResponseStatus.None;

            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);
                IOperationResult<T> getResult = null;

                string action = string.Format("Action: GetWithLock; bucket: {0}; key: {1}", bucketName, key);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                {
                    if (withLock)
                    {
                        getResult = bucket.GetWithLock<T>(key, TimeSpan.FromSeconds(GET_LOCK_TS_SECONDS));
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
                    if (getResult.Exception != null)
                        throw getResult.Exception;

                    status = getResult.Status;

                    if (getResult.Status == Couchbase.IO.ResponseStatus.Success)
                    {
                        result = getResult.Value;
                        cas = getResult.Cas;
                    }
                    else
                        HandleStatusCode(getResult, key);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchBaseCache - Failed GetWithLock with key = {0}, ex = {1}", key, ex);
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
                string action = string.Format("Action: Remove; bucket: {0}; key: {1}", bucketName, key);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                {
                    if (cas == 0)
                        removeResult = bucket.Remove(key);
                    else
                        removeResult = bucket.Remove(key, cas);
                }

                if (removeResult.Exception != null)
                    throw removeResult.Exception;

                if (removeResult.Status == Couchbase.IO.ResponseStatus.Success || removeResult.Status == Couchbase.IO.ResponseStatus.KeyNotFound)
                    result = removeResult.Success;
                else
                    log.ErrorFormat("Error while trying to delete document. key: {0}, CAS: {1}. CB response: {2}", key, cas, JsonConvert.SerializeObject(removeResult));
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchBaseCache - Failed remove with key = {0}, ex = {1}", key, ex);
            }

            return result;
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

                string action = string.Format("Action: Get; bucket: {0}; key: {1}", bucketName, key);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                {
                    getResult = bucket.Get<T>(key);
                }

                if (getResult != null)
                {
                    if (getResult.Exception != null)
                        throw getResult.Exception;

                    if (getResult.Status == Couchbase.IO.ResponseStatus.Success)
                    {
                        result = getResult.Value;
                        version = getResult.Cas;
                    }
                    else
                    {
                        HandleStatusCode(getResult, key);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchBaseCache - Failed GetWithVersion with key = {0}, ex = {1}", key, ex);
            }

            return result;
        }

        public bool GetWithVersion<T>(string key, out ulong version, ref T result)
        {
            bool res = false;
            version = 0;
            Couchbase.IO.ResponseStatus status = Couchbase.IO.ResponseStatus.None;
            try
            {
                result = GetWithVersion<T>(key, out version, out status);
                res = status == Couchbase.IO.ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchBaseCache - Failed Get with key = {0}, ex = {1}", key, ex);
            }

            return res;
        }

        public T GetWithVersion<T>(string key, out ulong version, out Couchbase.IO.ResponseStatus status, bool asJson = false)
        {
            version = 0;
            T result = default(T);
            status = Couchbase.IO.ResponseStatus.None;


            if (asJson)
                return GetJsonAsTWithVersion<T>(key, out version, out status);

            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);

                IOperationResult<T> getResult;

                string action = string.Format("Action: Get; bucket: {0}; key: {1}", bucketName, key);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                {
                    getResult = bucket.Get<T>(key);
                }

                if (getResult != null)
                {
                    if (getResult.Exception != null)
                        throw getResult.Exception;

                    status = getResult.Status;

                    if (getResult.Status == Couchbase.IO.ResponseStatus.Success)
                    {
                        result = getResult.Value;
                        version = getResult.Cas;
                    }
                    else
                    {
                        HandleStatusCode(getResult, key);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchBaseCache - Failed GetWithVersion with key = {0}, ex = {1}", key, ex);
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

            string action = string.Format("Action: Upsert; bucket: {0}; key: {1}; expiration: {2} seconds", bucketName, key, expiration);
            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
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
                if (setResult.Exception != null)
                    throw setResult.Exception;

                if (setResult.Status == Couchbase.IO.ResponseStatus.Success)
                    result = setResult.Success;
                else
                {
                    HandleStatusCode(setResult, key);

                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
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

            return result;
        }

        public bool SetWithVersion(string key, object value, ulong version, out ulong newVersion, uint expiration = 0, bool asJson = false)
        {
            bool result = false;
            newVersion = 0;

            var bucket = ClusterHelper.GetBucket(bucketName);
            IOperationResult setResult;
            expiration = FixExpirationTime(expiration);

            string action = string.Format("Action: Upsert; bucket: {0}; key: {1}; expiration: {2} seconds", bucketName, key, expiration);
            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
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
                if (setResult.Exception != null)
                    throw setResult.Exception;

                if (setResult.Status == Couchbase.IO.ResponseStatus.Success)
                {
                    result = setResult.Success;
                    newVersion = setResult.Cas;
                }
                else
                {
                    HandleStatusCode(setResult, key);

                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                    {
                        if (!asJson)
                            setResult = bucket.Upsert(key, value, version, expiration);
                        else
                        {
                            string serializedValue = ObjectToJson(value);
                            setResult = bucket.Upsert(key, serializedValue, version, expiration);
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

            string action = string.Format("Action: Upsert; bucket: {0}; key: {1}; expiration: {2} seconds", bucketName, key, expiration);
            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
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
                if (setResult.Exception != null)
                    throw setResult.Exception;

                if (setResult.Status == Couchbase.IO.ResponseStatus.Success)
                    result = setResult.Success;
                else
                {
                    HandleStatusCode(setResult, key);

                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
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

            return result;
        }

        public bool SetWithVersion<T>(string key, T value, ulong version, out ulong newVersion, uint expiration = 0, bool asJson = false)
        {
            bool result = false;
            newVersion = 0;

            var bucket = ClusterHelper.GetBucket(bucketName);
            IOperationResult setResult;
            expiration = FixExpirationTime(expiration);

            string action = string.Format("Action: Upsert; bucket: {0}; key: {1}; expiration: {2} seconds", bucketName, key, expiration);
            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
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
                if (setResult.Exception != null)
                    throw setResult.Exception;

                if (setResult.Status == Couchbase.IO.ResponseStatus.Success)
                {
                    result = setResult.Success;
                    newVersion = setResult.Cas;
                }
                else
                {
                    HandleStatusCode(setResult, key);

                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
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

        public IDictionary<string, T> GetValues<T>(List<string> keys, bool shouldAllowPartialQuery = false, bool asJson = false)
        {
            IDictionary<string, T> result = null;

            if (asJson)
            {
                IDictionary<string, string> jsonValues = this.GetValues<string>(keys, shouldAllowPartialQuery);

                if (jsonValues != null)
                    result = new Dictionary<string, T>();

                // Convert all strings to objects
                foreach (var jsonValue in jsonValues)
                    result.Add(jsonValue.Key, JsonToObject<T>(jsonValue.Value));

                return result;
            }

            try
            {
                var bucket = ClusterHelper.GetBucket(bucketName);
                IDictionary<string, IOperationResult<T>> getResult;

                string action = string.Format("Action: Get, bucket: {0}, keys: {1}", bucket.Name, string.Join(",", keys.ToArray()));
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                {
                    getResult = bucket.Get<T>(keys);
                }

                // Success until proven otherwise
                Couchbase.IO.ResponseStatus status = Couchbase.IO.ResponseStatus.Success;

                foreach (var item in getResult)
                {
                    // Throw exception if there is one
                    if (item.Value.Exception != null)
                        throw item.Value.Exception;

                    // If any of the rows wasn't successful, maybe we need to break - depending if we allow partials or not
                    if (item.Value.Status != Couchbase.IO.ResponseStatus.Success)
                    {
                        if (item.Value.Status == Couchbase.IO.ResponseStatus.KeyNotFound)
                            log.WarnFormat("Couchbase manager: failed to get key {0}, status {1}", item.Key, item.Value.Status);
                        else
                            log.ErrorFormat("Couchbase manager: failed to get key {0}, status {1}", item.Key, item.Value.Status);

                        status = item.Value.Status;

                        if (!shouldAllowPartialQuery)
                            break;
                    }
                    else
                        log.DebugFormat("Couchbase manager: GetValues success - get key {0}, status {1}", item.Key, item.Value.Status);
                }

                if (shouldAllowPartialQuery || status == Couchbase.IO.ResponseStatus.Success)
                {
                    // if successful - build dictionary based on execution result
                    result = new Dictionary<string, T>();

                    foreach (var item in getResult)
                    {
                        if (item.Value.Status == Couchbase.IO.ResponseStatus.Success)
                            result.Add(item.Key, item.Value.Value);
                    }
                }
                else
                    log.ErrorFormat("Error while executing action on CB. Status code = {0}; Status = {1}", (int)status, status.ToString());
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

        public T GetJsonAsTWithVersion<T>(string key, out ulong version, out Couchbase.IO.ResponseStatus status)
        {
            status = Couchbase.IO.ResponseStatus.None;

            T result = default(T);
            var json = GetWithVersion<string>(key, out version, out status);

            if (!string.IsNullOrEmpty(json))
                result = JsonToObject<T>(json);

            return result;
        }

        public bool GetValues<T>(List<string> keys, ref IDictionary<string, T> results, bool shouldAllowPartialQuery = false)
        {
            bool res = false;
            try
            {
                results = GetValues<T>(keys, shouldAllowPartialQuery);
                if (results != null)
                {
                    if (shouldAllowPartialQuery)
                    {
                        res = true;
                    }
                    else
                    {
                        res = keys.Count == results.Count;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error in GetValues<T> from CB while getting the following keys: {0}", string.Join(",", keys)), ex);
            }

            return res;
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
            return View<T>(definitions, ref  totalNumOfResults);
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
                log.ErrorFormat("CouchBaseCache - " + string.Format("Failed Getting view. error = {0}, ST = {1}", ex.Message, ex.StackTrace), ex);
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
                log.ErrorFormat("CouchBaseCache - " + string.Format("Failed Getting view. error = {0}, ST = {1}", ex.Message, ex.StackTrace), ex);
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
                log.ErrorFormat("CouchBaseCache - " + string.Format("Failed Getting view. error = {0}, ST = {1}", ex.Message, ex.StackTrace), ex);
            }

            return result;
        }

        /// <summary>
        /// Get the entire view row from view. We emulate a similar class to avoid breaking changes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="definitions"></param>
        /// <returns></returns>
        public List<ViewRow<T>> ViewRows<T>(ViewManager definitions)
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
                log.ErrorFormat("CouchBaseCache - " + string.Format("Failed Getting view. error = {0}, ST = {1}", ex.Message, ex.StackTrace), ex);
            }

            return result;
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
                log.ErrorFormat("CouchBaseCache - " + string.Format("Failed Getting view. error = {0}, ST = {1}", ex.Message, ex.StackTrace), ex);
            }

            return result;
        }
        #endregion

        public ulong Increment(string key, ulong delta)
        {
            ulong result = 0;

            var bucket = ClusterHelper.GetBucket(bucketName);
            IOperationResult<ulong> incrementResult = null;

            string action = string.Format("Action: Increment; bucket: {0}; key: {1}", bucketName, key);
            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
            {
                incrementResult = bucket.Increment(key, delta);
            }

            if (incrementResult != null)
            {
                if (incrementResult.Exception != null)
                {
                    throw incrementResult.Exception;
                }

                if (incrementResult.Status == Couchbase.IO.ResponseStatus.Success)
                {
                    result = incrementResult.Value;
                }
                else
                {
                    HandleStatusCode(incrementResult, key);
                }
            }

            return result;
        }

        #endregion


    }
}
