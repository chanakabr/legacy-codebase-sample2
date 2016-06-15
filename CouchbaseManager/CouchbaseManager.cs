using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Text;
using System.Threading;
using System.Configuration;
using KLogMonitor;
using System.Reflection;
using Couchbase;
using Couchbase.Configuration;
using Couchbase.Configuration.Client.Providers;
using Couchbase.Configuration.Client;
using Couchbase.Core.Serialization;
using Couchbase.Core.Transcoders;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using CouchBaseExtensions;

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
        DRM = 10
    }

    public class CouchbaseManager
    {
        #region Consts

        //public const string COUCHBASE_CONFIG = "couchbaseClients/couchbase";
        public const string COUCHBASE_CONFIG = "couchbaseClients/";
        private const string TCM_KEY_FORMAT = "cb_{0}.{1}";

        /// <summary>
        /// Defines duration of a month in seconds, see http://docs.couchbase.com/developer/dev-guide-3.0/doc-expiration.html
        /// </summary>
        private const uint monthInSeconds = 30 * 24 * 60 * 60;

        #endregion

        #region Static Members

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static object syncObj = new object();
        private static ReaderWriterLockSlim m_oSyncLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        protected static Couchbase.Core.Serialization.DefaultSerializer serializer;

        #endregion

        #region Data Members

        private string configurationSection;
        private string bucketName;
        private ClientConfiguration clientConfiguration;

        #endregion

        #region Ctor

        static CouchbaseManager()
        {
            //binder = new TypeNameSerializationBinder("ApiObjects.SearchObjects.{0}, ApiObjects");

            serializer = serializer = new DefaultSerializer();

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

        /// <summary>
        /// Initializes a CouchbaseManager instance with configuration in web.config or TCM, according to dynamic bucket section
        /// </summary>
        /// <param name="bucket"></param>
        public CouchbaseManager(string subSection, bool fromTcm = false)
        {
            subSection = subSection.ToLower();

            if (!fromTcm)
            {
                this.configurationSection = string.Format("{0}{1}", COUCHBASE_CONFIG, subSection);
                bucketName = GetBucketName(configurationSection);
            }
            else
            {
                var urls = TCMClient.Settings.Instance.GetValue<List<string>>(String.Format(TCM_KEY_FORMAT, subSection, "urls"));

                if (urls != null)
                {
                    string userName = TCMClient.Settings.Instance.GetValue<string>(String.Format(TCM_KEY_FORMAT, subSection, "username"));
                    string password = TCMClient.Settings.Instance.GetValue<string>(String.Format(TCM_KEY_FORMAT, subSection, "password"));
                    this.bucketName = TCMClient.Settings.Instance.GetValue<string>(String.Format(TCM_KEY_FORMAT, subSection, "bucket"));

                    // Convert list of URLs to list of Uris
                    List<Uri> uris = new List<Uri>();
                    urls.ForEach(current => uris.Add(new Uri(current)));

                    this.clientConfiguration = new ClientConfiguration()
                    {
                        BucketConfigs = new Dictionary<string, BucketConfiguration>()
                        {
                            { 
                                this.bucketName, 
                                new BucketConfiguration()
                                    {
                                        Username = userName,
                                        Password = password,
                                        Servers = uris,
                                        BucketName = this.bucketName
                                    }
                            }
                        }
                    };
                }
            }

            this.clientConfiguration.Transcoder = GetTranscoder;
        }

        private ITypeTranscoder GetTranscoder()
        {
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings() { ContractResolver = new DefaultContractResolver() };
            JsonSerializerSettings deserializationSettings = new JsonSerializerSettings() { ContractResolver = new DefaultContractResolver() };
            CustomSerializer serializer = new CustomSerializer(deserializationSettings, serializerSettings);
            CustomTranscoder transcoder = new CustomTranscoder(serializer);

            return transcoder;
        }

        //private ITypeSerializer GetSerializer()
        //{
        //    Couchbase.Core.Serialization.DefaultSerializer serializer = new DefaultSerializer();
        //    serializer.DeserializationSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;
        //    serializer.SerializerSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;

        //    return serializer;   
        //}

        private string GetBucketName(string configurationSection)
        {
            string bucketName = string.Empty;

            var section = (CouchbaseClientSection)ConfigurationManager.GetSection(configurationSection);
            this.clientConfiguration = new ClientConfiguration(section);

            // Should be only one!
            foreach (var currentBucket in this.clientConfiguration.BucketConfigs)
            {
                bucketName = currentBucket.Value.BucketName;
                break;
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
                    {
                        log.DebugFormat("Could not find key on couchbase: {0}", key);
                    }
                    else
                    {
                        log.ErrorFormat("Error while executing action on CB. Status code = {0}", statusCode.Value);
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
                {
                    log.DebugFormat("Could not find key on couchbase: {0}", key);
                }
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
            //switch (statusCode)
            //{
            //    // Busy
            //    case 133:
            //    // SocketPoolTimeout
            //    case 145:
            //    // UnableToLocateNode
            //    case 146:
            //    // NodeShutdown
            //    case 147:
            //    // OperationTimeout
            //    case 148:
            //    {
            //        //m_Client = CouchbaseManager.CouchbaseManager.RefreshInstance(bucket);

            //        break;
            //    }
            //    default:
            //    break;
            //}
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
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            else
                return default(T);
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
                using (var cluster = new Cluster(clientConfiguration))
                {
                    using (var bucket = cluster.OpenBucket(bucketName))
                    {
                        expiration = FixExpirationTime(expiration);

                        IOperationResult insertResult = null;

                        string action = string.Format("Action: Insert bucket: {0} key: {1} expiration: {2} seconds", bucketName, key, expiration);
                        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                        {
                            if (!asJson)
                            {
                                insertResult = bucket.Insert(key, value, expiration);
                            }
                            else
                            {
                                string serializedValue = ObjectToJson(value);
                                insertResult = bucket.Insert(key, serializedValue, expiration);
                            }
                        }

                        if (insertResult != null)
                        {
                            if (insertResult.Exception != null)
                            {
                                throw insertResult.Exception;
                            }

                            if (insertResult.Status == Couchbase.IO.ResponseStatus.Success)
                            {
                                result = insertResult.Success;
                            }
                            else
                            {
                                HandleStatusCode(insertResult, key);

                                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                                {
                                    if (!asJson)
                                    {
                                        insertResult = bucket.Insert(key, value, expiration);
                                    }
                                    else
                                    {
                                        string serializedValue = ObjectToJson(value);
                                        insertResult = bucket.Insert(key, serializedValue, expiration);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchBaseCache - " + string.Format("Failed Add with key = {0}, error = {1}, ST = {2}", key, ex.Message, ex.StackTrace), ex);

                if (ex.InnerException != null)
                {
                    log.ErrorFormat("CouchBaseCache - " + string.Format("Failed Add with key = {0}, inner exception = {1}, ST = {2}", key,
                        ex.InnerException.Message, ex.InnerException.StackTrace), ex.InnerException);
                }
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
                using (var cluster = new Cluster(clientConfiguration))
                {
                    using (var bucket = cluster.OpenBucket(bucketName))
                    {
                        IOperationResult insertResult = null;
                        expiration = FixExpirationTime(expiration);

                        string action = string.Format("Action: Insert bucket: {0} key: {1} expiration: {2} seconds", bucketName, key, expiration);
                        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                        {
                        if (!asJson)
                        {
                            insertResult = bucket.Insert<T>(key, value, expiration);
                        }
                        else
                        {
                            string serializedValue = ObjectToJson(value);
                            insertResult = bucket.Insert<string>(key, serializedValue, expiration);
                        }
                        }

                        if (insertResult != null)
                        {
                            if (insertResult.Exception != null)
                            {
                                throw insertResult.Exception;
                            }

                            if (insertResult.Status == Couchbase.IO.ResponseStatus.Success)
                            {
                                result = insertResult.Success;
                            }
                            else
                            {
                                HandleStatusCode(insertResult, key);

                                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                                {
                                    if (!asJson)
                                    {
                                        insertResult = bucket.Insert<T>(key, value, expiration);
                                    }
                                    else
                                    {
                                        string serializedValue = ObjectToJson(value);
                                        insertResult = bucket.Insert<string>(key, serializedValue, expiration);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchBaseCache - " + string.Format("Failed Add with key = {0}, error = {1}, ST = {2}", key, ex.Message, ex.StackTrace), ex);

                if (ex.InnerException != null)
                {
                    log.ErrorFormat("CouchBaseCache - " + string.Format("Failed Add with key = {0}, inner exception = {1}, ST = {2}", key,
                        ex.InnerException.Message, ex.InnerException.StackTrace), ex.InnerException);
                }
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
                using (var cluster = new Cluster(clientConfiguration))
                {
                    using (var bucket = cluster.OpenBucket(bucketName))
                    {
                        IOperationResult insertResult = null;
                        expiration = FixExpirationTime(expiration);

                        string action = string.Format("Action: Upsert bucket: {0} key: {1} expiration: {2} seconds", bucketName, key, expiration);
                        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                        {
                            if (!asJson)
                            {
                                insertResult = bucket.Upsert(key, value, expiration);
                            }
                            else
                            {
                                string serializedValue = ObjectToJson(value);
                                insertResult = bucket.Upsert(key, serializedValue, expiration);
                            }
                        }

                        if (insertResult != null)
                        {
                            if (insertResult.Exception != null)
                            {
                                throw insertResult.Exception;
                            }

                            if (insertResult.Status == Couchbase.IO.ResponseStatus.Success)
                            {
                                result = insertResult.Success;
                            }
                            else
                            {
                                HandleStatusCode(insertResult, key);

                                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                                {
                                    if (!asJson)
                                    {
                                        insertResult = bucket.Upsert(key, value, expiration);
                                    }
                                    else
                                    {
                                        string serializedValue = ObjectToJson(value);
                                        insertResult = bucket.Upsert(key, serializedValue, expiration);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchBaseCache - " + string.Format("Failed Set with key = {0}, error = {1}, ST = {2}", key, ex.Message, ex.StackTrace), ex);

                if (ex.InnerException != null)
                {
                    log.ErrorFormat("CouchBaseCache - " + string.Format("Failed Set with key = {0}, inner exception = {1}, ST = {2}", key,
                        ex.InnerException.Message, ex.InnerException.StackTrace), ex.InnerException);
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
        /// <param name="expiration">TTL in seconds</param>
        /// <returns></returns>
        public bool Set<T>(string key, T value, uint expiration = 0, bool asJson = false)
        {
            bool result = false;

            try
            {
                using (var cluster = new Cluster(clientConfiguration))
                {
                    using (var bucket = cluster.OpenBucket(bucketName))
                    {
                        IOperationResult insertResult = null;
                        expiration = FixExpirationTime(expiration);

                        string action = string.Format("Action: Upsert bucket: {0} key: {1} expiration: {2} seconds", bucketName, key, expiration);
                        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                        {
                        if (!asJson)
                        {
                            insertResult = bucket.Upsert<T>(key, value, expiration);
                        }
                        else
                        {
                            string serializedValue = ObjectToJson(value);
                            insertResult = bucket.Upsert<string>(key, serializedValue, expiration);
                        }
                        }

                        if (insertResult != null)
                        {
                            if (insertResult.Exception != null)
                            {
                                throw insertResult.Exception;
                            }

                            if (insertResult.Status == Couchbase.IO.ResponseStatus.Success)
                            {
                                result = insertResult.Success;
                            }
                            else
                            {
                                HandleStatusCode(insertResult, key);

                                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                                {
                                    if (!asJson)
                                    {
                                        insertResult = bucket.Upsert<T>(key, value, expiration);
                                    }
                                    else
                                    {
                                        string serializedValue = ObjectToJson(value);
                                        insertResult = bucket.Upsert<string>(key, serializedValue, expiration);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchBaseCache - " + string.Format("Failed Set<T> with key = {0}, error = {1}, ST = {2}", key, ex.Message, ex.StackTrace), ex);

                if (ex.InnerException != null)
                {
                    log.ErrorFormat("CouchBaseCache - " + string.Format("Failed Set<T> with key = {0}, inner exception = {1}, ST = {2}", key,
                        ex.InnerException.Message, ex.InnerException.StackTrace), ex.InnerException);
                }
            }
            return result;
        }

        public T Get<T>(string key, bool asJson = false)
        {
            T result = default(T);

            if (asJson)
            {
                return this.GetJsonAsT<T>(key);
            }
            
            try
            {
                using (var cluster = new Cluster(clientConfiguration))
                {
                    using (var bucket = cluster.OpenBucket(bucketName))
                    {
                        IOperationResult<T> getResult = null;

                        string action = string.Format("Action: Get bucket: {0} key: {1}", bucketName, key);
                        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                        {
                            getResult = bucket.Get<T>(key);
                        }

                        if (getResult != null)
                        {
                            if (getResult.Exception != null)
                            {
                                throw getResult.Exception;
                            }

                            if (getResult.Status == Couchbase.IO.ResponseStatus.Success)
                            {
                                result = getResult.Value;
                            }
                            else
                            {
                                HandleStatusCode(getResult, key);

                                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                                {
                                    if (!asJson)
                                    {
                                        getResult = bucket.Get<T>(key);
                                    }
                                    else
                                    {
                                        return this.GetJsonAsT<T>(key);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchBaseCache - " + string.Format("Failed Get with key = {0}, error = {1}, ST = {2}", key, ex.Message, ex.StackTrace), ex);

                if (ex.InnerException != null)
                {
                    log.ErrorFormat("CouchBaseCache - " + string.Format("Failed Get with key = {0}, inner exception = {1}, ST = {2}", key,
                        ex.InnerException.Message, ex.InnerException.StackTrace), ex.InnerException);
                }
            }

            return result;
        }

        public bool Remove(string key)
        {
            bool result = false;

            try
            {
                using (var cluster = new Cluster(clientConfiguration))
                {
                    using (var bucket = cluster.OpenBucket(bucketName))
                    {
                        bool exists;

                        string action = string.Format("Action: Exists bucket: {0} key: {1}", bucketName, key);
                        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                        {
                            exists = bucket.Exists(key);
                        }

                        // if key doesn't exist, we're cool
                        if (!exists)
                        {
                            result = true;
                        }
                        else
                        {
                            // Otherwise, try to really remove the key
                            IOperationResult removeResult;

                            action = string.Format("Action: Remove bucket: {0} key: {1}", bucketName, key);
                            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                            {
                                removeResult = bucket.Remove(key);
                            }

                            if (removeResult.Exception != null)
                            {
                                throw removeResult.Exception;
                            }

                            if (removeResult.Status == Couchbase.IO.ResponseStatus.Success)
                            {
                                result = removeResult.Success;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchBaseCache - " + string.Format("Failed remove with key = {0}, error = {1}, ST = {2}", key, ex.Message, ex.StackTrace), ex);

                if (ex.InnerException != null)
                {
                    log.ErrorFormat("CouchBaseCache - " + string.Format("Failed remove with key = {0}, inner exception = {1}, ST = {2}", key,
                        ex.InnerException.Message, ex.InnerException.StackTrace), ex.InnerException);
                }
            }

            return result;
        }

        public T GetWithVersion<T>(string key, out ulong version, bool asJson = false)
        {
            version = 0;
            T result = default(T);

            if (asJson)
            {
                return GetJsonAsTWithVersion<T>(key, out version);
            }

            try
            {
                using (var cluster = new Cluster(clientConfiguration))
                {
                    using (var bucket = cluster.OpenBucket(bucketName))
                    {
                        IOperationResult<T> getResult;


                        string action = string.Format("Action: Get bucket: {0} key: {1}", bucketName, key);
                        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                        {
                            getResult = bucket.Get<T>(key);
                        }

                        if (getResult != null)
                        {
                            if (getResult.Exception != null)
                            {
                                throw getResult.Exception;
                            }

                            if (getResult.Status == Couchbase.IO.ResponseStatus.Success)
                            {
                                result = getResult.Value;
                                version = getResult.Cas;
                            }
                            else
                            {
                                HandleStatusCode(getResult, key);

                                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                                {
                                    result = bucket.Get<T>(key).Value;
                                }

                                version = getResult.Cas;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchBaseCache - " + string.Format("Failed Get with key = {0}, error = {1}, ST = {2}", key, ex.Message, ex.StackTrace), ex);

                if (ex.InnerException != null)
                {
                    log.ErrorFormat("CouchBaseCache - " + string.Format("Failed Get with key = {0}, inner exception = {1}, ST = {2}", key,
                        ex.InnerException.Message, ex.InnerException.StackTrace), ex.InnerException);
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
        public bool SetWithVersion(string key, object value, ulong version, uint expiration = 0, bool asJson = false)
        {
            bool result = false;

            using (var cluster = new Cluster(clientConfiguration))
            {
                using (var bucket = cluster.OpenBucket(bucketName))
                {
                    IOperationResult setResult;
                    expiration = FixExpirationTime(expiration);

                    string action = string.Format("Action: Upsert bucket: {0} key: {1} expiration: {2} seconds", bucketName, key, expiration);
                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                    {
                    if (!asJson)
                    {
                        setResult = bucket.Upsert(key, value, version, expiration);
                    }
                    else
                    {
                        string serializedValue = ObjectToJson(value);
                        setResult = bucket.Upsert(key, serializedValue, version, expiration);
                    }
                    }

                    if (setResult != null)
                    {
                        if (setResult.Exception != null)
                        {
                            throw setResult.Exception;
                        }

                        if (setResult.Status == Couchbase.IO.ResponseStatus.Success)
                        {
                            result = setResult.Success;
                        }
                        else
                        {
                            HandleStatusCode(setResult, key);

                            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                            {
                                if (!asJson)
                                {
                                    setResult = bucket.Upsert(key, value, version, expiration);
                                }
                                else
                                {
                                    string serializedValue = ObjectToJson(value);
                                    setResult = bucket.Upsert(key, serializedValue, version, expiration);
                                }
                            }
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

            using (var cluster = new Cluster(clientConfiguration))
            {
                using (var bucket = cluster.OpenBucket(bucketName))
                {
                    IOperationResult setResult;
                    expiration = FixExpirationTime(expiration);

                    string action = string.Format("Action: Upsert bucket: {0} key: {1} expiration: {2} seconds", bucketName, key, expiration);
                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                    {
                    if (!asJson)
                    {
                        setResult = bucket.Upsert(key, value, version, expiration);
                    }
                    else
                    {
                        string serializedValue = ObjectToJson(value);
                        setResult = bucket.Upsert(key, serializedValue, version, expiration);
                    }
                    }

                    if (setResult != null)
                    {
                        if (setResult.Exception != null)
                        {
                            throw setResult.Exception;
                        }

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
                                {
                                    setResult = bucket.Upsert(key, value, version, expiration);
                                }
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

            using (var cluster = new Cluster(clientConfiguration))
            {
                using (var bucket = cluster.OpenBucket(bucketName))
                {
                    IOperationResult setResult;
                    expiration = FixExpirationTime(expiration);

                    string action = string.Format("Action: Upsert bucket: {0} key: {1} expiration: {2} seconds", bucketName, key, expiration);
                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                    {
                    if (!asJson)
                    {
                        setResult = bucket.Upsert<T>(key, value, version, expiration);
                    }
                    else
                    {
                        string serializedValue = ObjectToJson(value);
                        setResult = bucket.Upsert(key, serializedValue, version, expiration);
                    }
                    }

                    if (setResult != null)
                    {
                        if (setResult.Exception != null)
                        {
                            throw setResult.Exception;
                        }

                        if (setResult.Status == Couchbase.IO.ResponseStatus.Success)
                        {
                            result = setResult.Success;
                        }
                        else
                        {
                            HandleStatusCode(setResult, key);

                            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                            {
                                if (!asJson)
                                {
                                    setResult = bucket.Upsert<T>(key, value, version, expiration);
                                }
                                else
                                {
                                    string serializedValue = ObjectToJson(value);
                                    setResult = bucket.Upsert(key, serializedValue, version, expiration);
                                }
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

            using (var cluster = new Cluster(clientConfiguration))
            {
                using (var bucket = cluster.OpenBucket(bucketName))
                {
                    IOperationResult setResult;
                    expiration = FixExpirationTime(expiration);

                    string action = string.Format("Action: Upsert bucket: {0} key: {1} expiration: {2} seconds", bucketName, key, expiration);
                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                    {
                    if (!asJson)
                    {
                        setResult = bucket.Upsert<T>(key, value, version, expiration);
                    }
                    else
                    {
                        string serializedValue = ObjectToJson(value);
                        setResult = bucket.Upsert(key, serializedValue, version, expiration);
                    }
                    }

                    if (setResult != null)
                    {
                        if (setResult.Exception != null)
                        {
                            throw setResult.Exception;
                        }

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
                                {
                                    setResult = bucket.Upsert<T>(key, value, version, expiration);
                                }
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
                {
                    result = true;
                }
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
                }

                // Convert all strings to objects
                foreach (var jsonValue in jsonValues)
                {
                    result.Add(
                        jsonValue.Key,
                        JsonToObject<T>(jsonValue.Value));
                }

                return result;
            }
            
            try
            {
                using (var cluster = new Cluster(clientConfiguration))
                {
                    using (var bucket = cluster.OpenBucket(bucketName))
                    {
                        IDictionary<string, IOperationResult<T>> getResult;

                        string action = string.Format("Action: Get bucket: {0} keys: {1}", bucketName, string.Join(",", keys.ToArray()));
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
                            {
                                throw item.Value.Exception;
                            }

                            // If any of the rows wasn't successful, maybe we need to break - depending if we allow partials or not
                            if (item.Value.Status != Couchbase.IO.ResponseStatus.Success)
                            {
                                log.ErrorFormat("Couchbase manager: failed to get key {0}, status {1}", item.Key, item.Value.Status);

                                status = item.Value.Status;

                                if (!shouldAllowPartialQuery)
                                {
                                    break;
                                }
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
                                if (item.Value.Status == Couchbase.IO.ResponseStatus.Success)
                                {
                                    result.Add(item.Key, item.Value.Value);
                                }
                            }
                        }
                        else
                        {
                            log.ErrorFormat("Error while executing action on CB. Status code = {0}; Status = {1}",
                                (int)status, status.ToString());
                        }
                    }
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
            {
                result = JsonToObject<T>(json);
            }

            return result;
        }

        public T GetJsonAsTWithVersion<T>(string key, out ulong version)
        {
            T result = default(T);

            var json = GetWithVersion<string>(key, out version);

            if (!string.IsNullOrEmpty(json))
            {
                result = JsonToObject<T>(json);
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
            List<T> result = new List<T>();

            try
            {
                using (var cluster = new Cluster(clientConfiguration))
                {
                    using (var bucket = cluster.OpenBucket(bucketName))
                    {
                        Dictionary<string, int> keysToIndexes = new Dictionary<string, int>();
                        List<string> missingKeys = new List<string>();
                        T defaultValue = default(T);

                        if (definitions.asJson)
                        {
                            List<ViewRow<object>> rowsJson = definitions.QueryRows<object>(bucket);

                            foreach (var viewRow in rowsJson)
                            {
                                if (viewRow != null)
                                {
                                    // If we have a result - convert it to the typed object and add it to list
                                    if (null != viewRow.Value)
                                    {
                                        result.Add(JsonToObject<T>(viewRow.Value.ToString()));
                                    }
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
                            List<ViewRow<T>> rowsAsT = definitions.QueryRows<T>(bucket);

                            foreach (var viewRow in rowsAsT)
                            {
                                if (viewRow != null)
                                {
                                    // If we have a result - simply add it to list
                                    if (null != viewRow.Value)
                                    {
                                        result.Add(viewRow.Value);
                                    }
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
                using (var cluster = new Cluster(clientConfiguration))
                {
                    using (var bucket = cluster.OpenBucket(bucketName))
                    {
                        if (definitions.asJson)
                        {
                            var jsonResults = definitions.QueryKeyValuePairs<object>(bucket);

                            foreach (var jsonResult in jsonResults)
                            {
                                result.Add(new KeyValuePair<object, T1>(
                                    jsonResult.Key,
                                    JsonToObject<T1>(jsonResult.Value.ToString())
                                    ));
                            }
                        }
                        else
                        {
                            result = definitions.QueryKeyValuePairs<T1>(bucket);
                        }
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
        /// Get only list of document IDs from view
        /// </summary>
        /// <param name="definitions"></param>
        /// <returns></returns>
        public List<string> ViewIds(ViewManager definitions)
        {
            List<string> result = new List<string>();

            try
            {
                using (var cluster = new Cluster(clientConfiguration))
                {
                    using (var bucket = cluster.OpenBucket(bucketName))
                    {
                        result = definitions.QueryIds(bucket);
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
        /// Get the entire view row from view. We emulate a similar class to avoid breaking changes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="definitions"></param>
        /// <returns></returns>
        public List<ViewRow<T>> ViewRows<T>(ViewManager definitions)
        {
            List<ViewRow<T>> result = new List<ViewRow<T>>();

            try
            {
                using (var cluster = new Cluster(clientConfiguration))
                {
                    using (var bucket = cluster.OpenBucket(bucketName))
                    {
                        if (definitions.asJson)
                        {
                            var jsonResults = definitions.QueryRows<object>(bucket);

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
                        {
                            result = definitions.QueryRows<T>(bucket);
                        }
                    }
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

            using (var cluster = new Cluster(clientConfiguration))
            {
                using (var bucket = cluster.OpenBucket(bucketName))
                {
                    IOperationResult<ulong> incrementResult = null;

                    string action = string.Format("Action: Increment bucket: {0} key: {1}", bucketName, key);
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
                }
            }

            return result;
        }

        #endregion

    }
}
