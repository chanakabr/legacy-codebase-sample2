using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Text;
using System.Threading;
using System.Configuration;
using KLogMonitor;
using System.Reflection;
using Couchbase.Management;
using Couchbase;
using Couchbase.Configuration;

namespace CouchbaseManager
{
    public enum eCouchbaseBucket { DEFAULT = 0, NOTIFICATION = 1, SOCIAL = 2, SOCIALFRIENDS = 3, EPG = 4, MEDIAMARK = 5, STATISTICS = 6, CACHE = 7, SCHEDULED_TASKS = 8 }

    public class CouchbaseManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static volatile Dictionary<string, CouchbaseClient> m_CouchbaseInstances = new Dictionary<string, CouchbaseClient>();
        private static object syncObj = new object();
        private static ReaderWriterLockSlim m_oSyncLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public static CouchbaseClient GetInstance(eCouchbaseBucket eBucket)
        {
            CouchbaseClient tempClient = null;

            if (!m_CouchbaseInstances.ContainsKey(eBucket.ToString()))
            {
                if (m_oSyncLock.TryEnterWriteLock(1000))
                {
                    try
                    {
                        if (!m_CouchbaseInstances.ContainsKey(eBucket.ToString()))
                        {
                            CouchbaseClient client = createNewInstance(eBucket);

                            if (client != null)
                            {
                                m_CouchbaseInstances.Add(eBucket.ToString(), client);
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
            if (m_CouchbaseInstances.ContainsKey(eBucket.ToString()))
            {
                if (m_oSyncLock.TryEnterWriteLock(1000))
                {
                    try
                    {
                        if (m_CouchbaseInstances.ContainsKey(eBucket.ToString()))
                        {
                            var client = m_CouchbaseInstances[eBucket.ToString()];
                            client.Dispose();

                            m_CouchbaseInstances.Remove(eBucket.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("", ex);
                    }
                    finally
                    {
                        m_oSyncLock.ExitWriteLock();
                    }
                }
            }

            return GetInstance(eBucket);
        }
    }
}
