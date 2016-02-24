using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Text;
using System.Threading;
using System.Configuration;
using Couchbase;
using Couchbase.Configuration;
using KLogMonitor;
using System.Reflection;


namespace CouchbaseManager
{
    public enum eCouchbaseBucket { DEFAULT = 0, NOTIFICATION = 1, SOCIAL = 2, SOCIALFRIENDS = 3, EPG = 4, MEDIAMARK = 5, STATISTICS = 6, CACHE = 7, SCHEDULED_TASKS = 8 }

    public class CouchbaseManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const int MAX_RETRY = 3;

        private static volatile Dictionary<string, CouchbaseClient> m_CouchbaseInstances = new Dictionary<string, CouchbaseClient>();
        private static object syncObj = new object();
        private static ReaderWriterLockSlim m_oSyncLock = new ReaderWriterLockSlim();

        public static CouchbaseClient GetInstance(eCouchbaseBucket eBucket)
        {
            CouchbaseClient tempClient = null;

            if (!m_CouchbaseInstances.ContainsKey(eBucket.ToString()))
            {
                if (m_oSyncLock.TryEnterWriteLock(1000))
                {
                    try
                    {
                        bool isDone = false;
                        int currentRetry = 0;

                        if (!m_CouchbaseInstances.ContainsKey(eBucket.ToString()))
                        {
                            while (!isDone)
                            {
                                CouchbaseClient client = createNewInstance(eBucket);

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

                                        Thread.Sleep(100);
                                    }
                                    else
                                    {
                                        m_CouchbaseInstances.Add(eBucket.ToString(), client);
                                        isDone = true;
                                    }
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

            // If item already exist
            if (m_oSyncLock.TryEnterReadLock(1000))
            {
                try
                {
                    m_CouchbaseInstances.TryGetValue(eBucket.ToString(), out tempClient);

                }
                catch (Exception ex)
                {
                    log.Error("", ex);
                }
                finally
                {
                    m_oSyncLock.ExitReadLock();
                }
            }

            return tempClient;
        }

        private static CouchbaseClient createNewInstance(eCouchbaseBucket eBucket)
        {
            CouchbaseClient oRes = null;
            switch (eBucket)
            {
                case eCouchbaseBucket.SOCIAL:
                case eCouchbaseBucket.SOCIALFRIENDS:
                case eCouchbaseBucket.EPG:
                case eCouchbaseBucket.STATISTICS:
                case eCouchbaseBucket.DEFAULT:
                case eCouchbaseBucket.MEDIAMARK:
                case eCouchbaseBucket.SCHEDULED_TASKS:
                    var socialBucketSection = (CouchbaseClientSection)ConfigurationManager.GetSection(string.Format("couchbase/{0}", eBucket.ToString().ToLower()));
                    oRes = new CouchbaseClient(socialBucketSection);
                    break;
                case eCouchbaseBucket.NOTIFICATION:
                    break;
                case eCouchbaseBucket.CACHE:
                    var groupChacheBucketSection = (CouchbaseClientSection)ConfigurationManager.GetSection(string.Format("couchbase/{0}", eBucket.ToString().ToLower()));
                    oRes = new CouchbaseClient(groupChacheBucketSection);
                    break;
            }

            return oRes;
        }

        /// <summary>
        /// Recreates an instance in case of failure
        /// </summary>
        /// <param name="eBucket"></param>
        /// <returns></returns>
        public static CouchbaseClient RefreshInstance(eCouchbaseBucket eBucket)
        {

            if (m_oSyncLock.TryEnterWriteLock(1000))
            {
                try
                {
                    foreach (var key in new List<string>(m_CouchbaseInstances.Keys))
                    {
                        m_CouchbaseInstances[key].Dispose();
                        m_CouchbaseInstances[key] = null;
                    }

                    m_CouchbaseInstances.Clear();
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    m_oSyncLock.ExitWriteLock();
                }
            }

            return GetInstance(eBucket);
        }
    }
}
