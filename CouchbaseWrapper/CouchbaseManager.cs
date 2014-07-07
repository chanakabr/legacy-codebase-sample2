using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using Couchbase.Configuration;
using System.Linq;

namespace CouchbaseWrapper
{

    public class CouchbaseManager
    {
        private static ReaderWriterLockSlim m_oSyncLock = new ReaderWriterLockSlim();
        private static volatile Dictionary<string, GenericCouchbaseClient> m_CouchbaseInstances = new Dictionary<string, GenericCouchbaseClient>();

        public static GenericCouchbaseClient GetInstance(string bucketName)
        {
            GenericCouchbaseClient tempClient = null;
            if (!m_CouchbaseInstances.ContainsKey(bucketName.ToLower()))
            {
                try
                {
                    if (m_oSyncLock.TryEnterWriteLock(1000))
                    {
                        GenericCouchbaseClient client = createNewInstance(bucketName.ToLower());

                        if (client != null)
                        {
                            m_CouchbaseInstances.Add(bucketName.ToLower(), client);
                            tempClient = client;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Logger.Log("Error creating client", ex.Message, "CbWrapper: " + DateTime.UtcNow);
                }
                finally
                {
                    m_oSyncLock.ExitWriteLock();
                }
            }
            else
            {
                try
                {
                    m_CouchbaseInstances.TryGetValue(bucketName.ToLower(), out tempClient);
                }
                catch (Exception ex)
                {
                    Logger.Logger.Log("Error getting client", ex.Message, "CbWrapper: " + DateTime.UtcNow);
                }
            }
            return tempClient;
        }

        private static GenericCouchbaseClient createNewInstance(string bucketName)
        {
            ClientConfig tcmConfig = TCMClient.Settings.Instance.GetValue<ClientConfig>("cb_"+bucketName);
            if (tcmConfig != null)
            {
                CouchbaseClientConfiguration clientConfig = new CouchbaseClientConfiguration()
                {
                    Bucket = tcmConfig.Bucket,
                    Username = tcmConfig.Username,
                    Password = tcmConfig.Password,
                };
                tcmConfig.URLs.ForEach(x => clientConfig.Urls.Add(new Uri(x)));
                return new GenericCouchbaseClient(clientConfig);
            }
            else
            {
                GenericCouchbaseClient oRes = null;
                var socialBucketSection = (CouchbaseClientSection)ConfigurationManager.GetSection(string.Format("couchbase/{0}", bucketName.ToLower()));
                oRes = new GenericCouchbaseClient(socialBucketSection);
                return oRes;
            }
        }


    }
}