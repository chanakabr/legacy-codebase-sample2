using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Text;
using System.Threading;
using System.Configuration;
using Couchbase;
using Couchbase.Configuration;

namespace CouchbaseManager
{
    public enum eCouchbaseBucket { UNKNOWN = 0, NOTIFICATION = 1, SOCIALHUB = 2, SOCIALFRIENDS = 3, EPG = 4, MEDIAMARK = 5 }

    public class CouchbaseManager
    {
        private static volatile Dictionary<string, CouchbaseClient> m_CouchbaseInstances = new Dictionary<string, CouchbaseClient>();
        private static object syncObj = new object();
        private static ReaderWriterLockSlim m_oSyncLock = new ReaderWriterLockSlim();

        public static CouchbaseClient GetInstance(eCouchbaseBucket eBucket)
        {
            CouchbaseClient tempClient = null;

            if (m_CouchbaseInstances == null)
            {
                m_CouchbaseInstances = new Dictionary<string, CouchbaseClient>();
            }

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
                }
                finally
                {
                    m_oSyncLock.ExitWriteLock();
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
                case eCouchbaseBucket.SOCIALHUB:
                case eCouchbaseBucket.SOCIALFRIENDS:
                case eCouchbaseBucket.EPG:
                case eCouchbaseBucket.MEDIAMARK:
                    var socialBucketSection = (CouchbaseClientSection)ConfigurationManager.GetSection(string.Format("couchbase/{0}", eBucket.ToString().ToLower()));
                    oRes = new CouchbaseClient(socialBucketSection);
                    break;
                case eCouchbaseBucket.NOTIFICATION:
                    break;
            }

            return oRes;
        }
    }
}
