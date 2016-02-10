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
        CROWDSOURCE = 9
    }

    public class CouchbaseManager
    {
        #region Consts

        public const string COUCHBASE_CONFIG = "couchbaseClients/couchbase";

        #endregion

        #region Static Members

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static object syncObj = new object();
        private static ReaderWriterLockSlim m_oSyncLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        #endregion

        #region Data Members

        private string bucketName;

        #endregion

        #region Ctor

        public CouchbaseManager(eCouchbaseBucket bucket)
        {
            this.bucketName = GetBucketName(bucket);
        }

        private static string GetBucketName(eCouchbaseBucket bucket)
        {
            string bucketName = string.Empty;
            //switch (bucket)
            //{
            //    case eCouchbaseBucket.SOCIAL:
            //    case eCouchbaseBucket.SOCIALFRIENDS:
            //    case eCouchbaseBucket.EPG:
            //    case eCouchbaseBucket.STATISTICS:
            //    case eCouchbaseBucket.DEFAULT:
            //    case eCouchbaseBucket.MEDIAMARK:
            //    case eCouchbaseBucket.SCHEDULED_TASKS:
            //        var socialBucketSection = (CouchbaseClientSection)ConfigurationManager.GetSection(string.Format("couchbase/{0}", bucket.ToString().ToLower()));
            //        //oRes = new CouchbaseClient(socialBucketSection);
            //        break;
            //    case eCouchbaseBucket.NOTIFICATION:
            //        break;
            //    case eCouchbaseBucket.CACHE:
            //        var groupChacheBucketSection = (CouchbaseClientSection)ConfigurationManager.GetSection(string.Format("couchbase/{0}", bucket.ToString().ToLower()));
            //        //oRes = new CouchbaseClient(groupChacheBucketSection);
            //        break;
            //}

            // See http://developer.couchbase.com/documentation/server/4.0/sdks/dotnet-2.2/configuring-the-client.html
            // We have a list of buckets in the server, but we don't know the exact bucket name.
            // We do know the name of the SECTION
            // after "parsing" the configuration to the client object, we will see which of the items in the app.config has the same key as the enum.
            // If they are identical, then we can get the server's bucket name from it
            var section = (CouchbaseClientSection)ConfigurationManager.GetSection(COUCHBASE_CONFIG);
            var clientConfiguration = new ClientConfiguration(section);

            foreach (var currentBucket in clientConfiguration.BucketConfigs)
            {
                if (currentBucket.Key == bucket.ToString().ToLower())
                {
                    bucketName = currentBucket.Value.BucketName;
                }
            }

            return bucketName;
        }

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

        private void HandleStatusCode(Couchbase.IO.ResponseStatus status, string key = "")
        {
            if (status != Couchbase.IO.ResponseStatus.Success)
            {
                // 1 - not found
                if (status == Couchbase.IO.ResponseStatus.KeyNotFound)
                {
                    log.DebugFormat("Could not find key on couchbase: {0}", key);
                }
                else
                {
                    log.ErrorFormat("Error while executing action on CB. Status code = {0}; Status = {1}", (int)status, status.ToString());
                }
            }

            // Cases of retry
            switch (status)
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
                return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
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

        #endregion

        #region Public Methods
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiration">TTL in seconds</param>
        /// <returns></returns>
        public bool Add(string key, object value, uint expiration = 0)
        {
            bool result = false;

            try
            {
                using (var cluster = new Cluster(COUCHBASE_CONFIG))
                {
                    using (var bucket = cluster.OpenBucket(bucketName))
                    {
                        var insertResult = bucket.Insert(key, value, expiration);

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
                                HandleStatusCode(insertResult.Status);

                                insertResult = bucket.Insert(key, value, expiration);

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchBaseCache - " + string.Format("Failed Add with key = {0}, error = {1}, ST = {2}", key, ex.Message, ex.StackTrace), ex);
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
        public bool Set(string key, object value, uint expiration = 0)
        {
            bool result = false;

            using (var cluster = new Cluster(COUCHBASE_CONFIG))
            {
                using (var bucket = cluster.OpenBucket(bucketName))
                {
                    var insertResult = bucket.Replace(key, value, expiration);

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
                            HandleStatusCode(insertResult.Status);

                            insertResult = bucket.Replace(key, value, expiration);
                        }
                    }
                }
            }

            return result;
        }

        public T Get<T>(string key)
        {
            T result = default(T);

            try
            {
                using (var cluster = new Cluster(COUCHBASE_CONFIG))
                {
                    using (var bucket = cluster.OpenBucket(bucketName))
                    {
                        var getResult = bucket.Get<T>(key);

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
                                HandleStatusCode(getResult.Status);

                                result = bucket.Get<T>(key).Value;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchBaseCache - " + string.Format("Failed Get with key = {0}, error = {1}, ST = {2}", key, ex.Message, ex.StackTrace), ex);
            }

            return result;
        }

        public bool Remove(string key)
        {
            bool result = false;

            try
            {
                using (var cluster = new Cluster(COUCHBASE_CONFIG))
                {
                    using (var bucket = cluster.OpenBucket(bucketName))
                    {
                        var removeResult = bucket.Remove(key);

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
            catch (Exception ex)
            {
                log.ErrorFormat("CouchBaseCache - " + string.Format("Failed Get with key = {0}, error = {1}, ST = {2}", key, ex.Message, ex.StackTrace), ex);
            }

            return result;
        }

        public T GetWithVersion<T>(string key, out ulong version)
        {
            version = 0;
            T result = default(T);

            try
            {
                using (var cluster = new Cluster(COUCHBASE_CONFIG))
                {
                    using (var bucket = cluster.OpenBucket(bucketName))
                    {
                        var getResult = bucket.Get<T>(key);

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
                                HandleStatusCode(getResult.Status);

                                result = bucket.Get<T>(key).Value;
                                version = getResult.Cas;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchBaseCache - " + string.Format("Failed Get with key = {0}, error = {1}, ST = {2}", key, ex.Message, ex.StackTrace), ex);
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
        public bool SetWithVersion(string key, object value, ulong version, uint expiration = 0)
        {
            bool result = false;

            using (var cluster = new Cluster(COUCHBASE_CONFIG))
            {
                using (var bucket = cluster.OpenBucket(bucketName))
                {
                    var setResult = bucket.Replace(key, value, version, expiration);

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
                            HandleStatusCode(setResult.Status);

                            setResult = bucket.Replace(key, value, version, expiration);
                        }
                    }
                }
            }

            return result;

        }

        public bool SetWithVersionWithRetry<T>(string key, object value, ulong version, int numOfRetries, int retryInterval)
        {
            bool result = false;

            if (numOfRetries >= 0)
            {
                bool operationResult = SetWithVersion(key, value, version);
                if (!operationResult)
                {
                    numOfRetries--;
                    Thread.Sleep(retryInterval);

                    ulong newVersion;
                    var getResult = GetWithVersion<T>(key, out newVersion);

                    result = SetWithVersionWithRetry<T>(key, value, newVersion, numOfRetries, retryInterval);
                }
                else
                {
                    result = true;
                }
            }

            return result;
        }

        public IDictionary<string, T> GetValues<T>(List<string> keys, bool shouldAllowPartialQuery = false)
        {
            IDictionary<string, T> result = null;
            try
            {
                using (var cluster = new Cluster(COUCHBASE_CONFIG))
                {
                    using (var bucket = cluster.OpenBucket(bucketName))
                    {
                        var getResult = bucket.Get<T>(keys);

                        Couchbase.IO.ResponseStatus status = Couchbase.IO.ResponseStatus.None;

                        foreach (var item in getResult)
                        {
                            if (item.Value.Exception != null)
                            {
                                throw item.Value.Exception;
                            }

                            if (item.Value.Status != Couchbase.IO.ResponseStatus.Success)
                            {
                                status = item.Value.Status;

                                if (!shouldAllowPartialQuery)
                                {
                                    break;
                                }
                            }
                        }

                        if (shouldAllowPartialQuery || status == Couchbase.IO.ResponseStatus.Success)
                        {
                            // if successfull - build dictionary based on execution result
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
                            // Otherwise, recreate connection and try again
                            HandleStatusCode(status);
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

        public List<T> View<T>(ViewManager definitions)
        {
            List<T> result = new List<T>();

            try
            {
                using (var cluster = new Cluster(COUCHBASE_CONFIG))
                {
                    using (var bucket = cluster.OpenBucket(bucketName))
                    {
                        result = definitions.Query<T>(bucket);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchBaseCache - " + string.Format("Failed Getting view. error = {0}, ST = {1}", ex.Message, ex.StackTrace), ex);
            }

            return result;
        }

        public List<KeyValuePair<object, object>> ViewGeneric(ViewManager definitions)
        {
            List<KeyValuePair<object, object>> result = new List<KeyValuePair<object, object>>();

            try
            {
                using (var cluster = new Cluster(COUCHBASE_CONFIG))
                {
                    using (var bucket = cluster.OpenBucket(bucketName))
                    {
                        result = definitions.QueryGeneric(bucket);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CouchBaseCache - " + string.Format("Failed Getting view. error = {0}, ST = {1}", ex.Message, ex.StackTrace), ex);
            }

            return result;
        }

        public ulong Increment(string key, ulong delta)
        {
            ulong result = 0;

            using (var cluster = new Cluster(COUCHBASE_CONFIG))
            {
                using (var bucket = cluster.OpenBucket(bucketName))
                {
                    var incrementResult = bucket.Increment(key, delta);

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
                            HandleStatusCode(incrementResult.Status);
                        }
                    }
                }
            }

            return result;
        }

        #endregion

    }
}