using Couchbase;
using Couchbase.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Web;
using WebAPI.Exceptions;
using WebAPI.Models;
using System.Reflection;
using KLogMonitor;

namespace WebAPI.Managers
{
    public enum CouchbaseBucket { Groups = 0, Tokens = 1 }

    public class CouchbaseManager
    {
        //private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const string TCM_KEY_FORMAT = "cb_{0}.{1}";
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
                        log.ErrorFormat("Error while getting CB instance {0}, exception: {1}", eBucket.ToString(), ex);
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
                    log.ErrorFormat("Error while getting CB instance_ {0}, exception: {1}", eBucket.ToString(), ex);
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
            CouchbaseClient client = null;

            var urls = TCMClient.Settings.Instance.GetValue<List<string>>(String.Format(TCM_KEY_FORMAT, eBucket.ToString().ToLower(), "urls"));
            if (urls != null)
            {
                CouchbaseClientConfiguration clientConfig = new CouchbaseClientConfiguration()
                {
                    Bucket = TCMClient.Settings.Instance.GetValue<string>(String.Format(TCM_KEY_FORMAT, eBucket.ToString().ToLower(), "bucket")),
                    Username = TCMClient.Settings.Instance.GetValue<string>(String.Format(TCM_KEY_FORMAT, eBucket.ToString().ToLower(), "username")),
                    Password = TCMClient.Settings.Instance.GetValue<string>(String.Format(TCM_KEY_FORMAT, eBucket.ToString().ToLower(), "password")),
                };
                urls.ForEach(u => clientConfig.Urls.Add(new Uri(u)));
                client = new CouchbaseClient(clientConfig);
            }
            else
            {
                var section = (CouchbaseClientSection)ConfigurationManager.GetSection(string.Format("couchbase/{0}", eBucket.ToString().ToLower()));
                client = new CouchbaseClient(section);
            }

            return client;
        }
    }
}