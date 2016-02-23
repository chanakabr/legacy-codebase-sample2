using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using Couchbase.Configuration;
using System.Linq;
using System.Text;

namespace CouchbaseWrapper
{

    public class CouchbaseManager
    {
        private const int MAX_RETRY = 3;

        private static volatile Dictionary<string, GenericCouchbaseClient> m_CouchbaseInstances = new Dictionary<string, GenericCouchbaseClient>();
        private static object locker = new object();

        public static GenericCouchbaseClient GetInstance(string bucketName)
        {
            string loweredBucketName = bucketName.ToLower();
            if (!m_CouchbaseInstances.ContainsKey(loweredBucketName))
            {
                lock (locker)
                {
                    bool isDone = false;
                    int currentRetry = 0;

                    if (!m_CouchbaseInstances.ContainsKey(loweredBucketName))
                    {
                        try
                        {
                            while (!isDone)
                            {
                                GenericCouchbaseClient client = createNewInstance(loweredBucketName);

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

                                        Logger.Logger.Log("GetInstance",
                                                    string.Format("Connection test failed. error message = {0}, stack trace = {1}",
                                                    ex.Message, ex.StackTrace),
                                                    "CouchbaseManager");
                                    }

                                    if (!isOK)
                                    {
                                        currentRetry++;

                                        if (currentRetry > MAX_RETRY)
                                        {
                                            isDone = true;
                                            throw new Exception("Exceeded maximum number of Couchbase instance refresh");
                                        }

                                        Thread.Sleep(500);
                                    }
                                    else
                                    {
                                        m_CouchbaseInstances.Add(loweredBucketName, client);
                                        isDone = true;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            #region Logging
                            StringBuilder sb = new StringBuilder("Exception at CouchbaseWrapper.CouchbaseManager.GetInstance. ");
                            sb.Append(String.Concat("Ex msg: ", ex.Message));
                            sb.Append(String.Concat(" Bucket name: ", bucketName));
                            sb.Append(String.Concat(" Ex type: ", ex.GetType().Name));
                            sb.Append(String.Concat(" Stack trace: ", ex.StackTrace));

                            Logger.Logger.Log("Exception", sb.ToString(), "CBWrapper");
                            #endregion
                        }
                    }
                }
            }

            if (m_CouchbaseInstances.ContainsKey(loweredBucketName))
                return m_CouchbaseInstances[loweredBucketName];
            return null;

        }

        private static GenericCouchbaseClient createNewInstance(string bucketName)
        {
            ClientConfig tcmConfig = TCMClient.Settings.Instance.GetValue<ClientConfig>(String.Concat("cb_", bucketName));
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