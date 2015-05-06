using Couchbase;
using Couchbase.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Web;
using WebAPI.Filters;
using WebAPI.Models;

namespace WebAPI.Managers
{
    public enum CouchbaseBucket { Groups = 0}

    public class CouchbaseManager
    {
        private static volatile Dictionary<string, CouchbaseClient> m_CouchbaseInstances = new Dictionary<string, CouchbaseClient>();
        private static object syncObj = new object();
        private static ReaderWriterLockSlim m_oSyncLock = new ReaderWriterLockSlim();

        public static CouchbaseClient GetInstance(CouchbaseBucket eBucket)
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
                        throw new InternalServerErrorException();
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
                    throw new InternalServerErrorException();
                }
                finally
                {
                    m_oSyncLock.ExitReadLock();
                }
            }

            return tempClient;
        }

        private static CouchbaseClient createNewInstance(CouchbaseBucket eBucket)
        {
            CouchbaseClient oRes = null;

            switch (eBucket)
            {
                case CouchbaseBucket.Groups:
                    var grpupsBucketSection = (CouchbaseClientSection)ConfigurationManager.GetSection(string.Format("couchbase/{0}", eBucket.ToString().ToLower()));
                    oRes = new CouchbaseClient(grpupsBucketSection);
                    break;
            }

            return oRes;
        }
    }
}