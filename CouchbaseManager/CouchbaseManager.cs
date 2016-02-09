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
        public const string COUCHBASE_CONFIG = "couchbaseClients/couchbase";

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static object syncObj = new object();
        private static ReaderWriterLockSlim m_oSyncLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private string bucketName;

        public CouchbaseManager(eCouchbaseBucket bucket)
        {
            this.bucketName = GetBucketName(bucket);
        }

        private static string GetBucketName(eCouchbaseBucket bucket)
        {
            string bucketName = string.Empty;
            switch (bucket)
            {
                case eCouchbaseBucket.SOCIAL:
                case eCouchbaseBucket.SOCIALFRIENDS:
                case eCouchbaseBucket.EPG:
                case eCouchbaseBucket.STATISTICS:
                case eCouchbaseBucket.DEFAULT:
                case eCouchbaseBucket.MEDIAMARK:
                case eCouchbaseBucket.SCHEDULED_TASKS:
                    var socialBucketSection = (CouchbaseClientSection)ConfigurationManager.GetSection(string.Format("couchbase/{0}", bucket.ToString().ToLower()));
                    //oRes = new CouchbaseClient(socialBucketSection);
                    break;
                case eCouchbaseBucket.NOTIFICATION:
                    break;
                case eCouchbaseBucket.CACHE:
                    var groupChacheBucketSection = (CouchbaseClientSection)ConfigurationManager.GetSection(string.Format("couchbase/{0}", bucket.ToString().ToLower()));
                    //oRes = new CouchbaseClient(groupChacheBucketSection);
                    break;
            }

            return bucketName;
        }

        //public static CouchbaseClient GetInstance(eCouchbaseBucket eBucket)
        //{
        //    CouchbaseClient tempClient = null;

        //    if (!m_CouchbaseInstances.ContainsKey(eBucket.ToString()))
        //    {
        //        if (m_oSyncLock.TryEnterWriteLock(1000))
        //        {
        //            try
        //            {
        //                if (!m_CouchbaseInstances.ContainsKey(eBucket.ToString()))
        //                {
        //                    CouchbaseClient client = createNewInstance(eBucket);

        //                    if (client != null)
        //                    {
        //                        m_CouchbaseInstances.Add(eBucket.ToString(), client);
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                log.Error("", ex);
        //            }
        //            finally
        //            {
        //                m_oSyncLock.ExitWriteLock();
        //            }
        //        }
        //    }

        //    // If item already exist
        //    if (m_oSyncLock.TryEnterReadLock(1000))
        //    {
        //        try
        //        {
        //            m_CouchbaseInstances.TryGetValue(eBucket.ToString(), out tempClient);
        //        }
        //        catch (Exception ex)
        //        {
        //            log.Error("", ex);
        //        }
        //        finally
        //        {
        //            m_oSyncLock.ExitReadLock();
        //        }
        //    }

        //    return tempClient;
        //}

        //private static CouchbaseClient createNewInstance(eCouchbaseBucket eBucket)
        //{
        //    CouchbaseClient oRes = null;
        //    switch (eBucket)
        //    {
        //        case eCouchbaseBucket.SOCIAL:
        //        case eCouchbaseBucket.SOCIALFRIENDS:
        //        case eCouchbaseBucket.EPG:
        //        case eCouchbaseBucket.STATISTICS:
        //        case eCouchbaseBucket.DEFAULT:
        //        case eCouchbaseBucket.MEDIAMARK:
        //        case eCouchbaseBucket.SCHEDULED_TASKS:
        //            var socialBucketSection = (CouchbaseClientSection)ConfigurationManager.GetSection(string.Format("couchbase/{0}", eBucket.ToString().ToLower()));
        //            oRes = new CouchbaseClient(socialBucketSection);
        //            break;
        //        case eCouchbaseBucket.NOTIFICATION:
        //            break;
        //        case eCouchbaseBucket.CACHE:
        //            var groupChacheBucketSection = (CouchbaseClientSection)ConfigurationManager.GetSection(string.Format("couchbase/{0}", eBucket.ToString().ToLower()));
        //            oRes = new CouchbaseClient(groupChacheBucketSection);
        //            break;
        //    }

        //    return oRes;
        //}

        ///// <summary>
        ///// Recreates an instance in case of failure
        ///// </summary>
        ///// <param name="eBucket"></param>
        ///// <returns></returns>
        //public static CouchbaseClient RefreshInstance(eCouchbaseBucket eBucket)
        //{
        //    if (m_CouchbaseInstances.ContainsKey(eBucket.ToString()))
        //    {
        //        if (m_oSyncLock.TryEnterWriteLock(1000))
        //        {
        //            try
        //            {
        //                if (m_CouchbaseInstances.ContainsKey(eBucket.ToString()))
        //                {
        //                    var client = m_CouchbaseInstances[eBucket.ToString()];
        //                    client.Dispose();

        //                    m_CouchbaseInstances.Remove(eBucket.ToString());
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                log.Error("", ex);
        //            }
        //            finally
        //            {
        //                m_oSyncLock.ExitWriteLock();
        //            }
        //        }
        //    }

        //    return GetInstance(eBucket);
        //}


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

        public bool Remove<T>(string key)
        {
            bool result = false;

            try
            {
                using (var cluster = new Cluster(COUCHBASE_CONFIG))
                {
                    using (var bucket = cluster.OpenBucket(bucketName))
                    {
                        var executeGet = bucket.GetDocument<T>(key);

                        if (executeGet != null)
                        {
                            if (executeGet.Exception != null)
                            {
                                throw executeGet.Exception;
                            }

                            if (executeGet.Document != null)
                            {
                                var removeResult = bucket.Remove<T>(executeGet.Document);

                                if (removeResult.Exception != null)
                                {
                                    throw removeResult.Exception;
                                }

                                if (removeResult.Status == Couchbase.IO.ResponseStatus.Success)
                                {
                                    result = removeResult.Success;
                                }
                                //else
                                //{
                                //    HandleStatusCode(removeResult.Status);

                                //    result = bucket.Replace(key, value, expiration);
                                //}
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
    }
}