using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ApiObjects;
using CouchbaseManager;
using KLogMonitor;
using Newtonsoft.Json;

namespace DalCB
{
    public class General_Couchbase
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static bool ModifyCB(string bucket, string key, eDbActionType action, string data, long ttlMinutes = 0)
        {
            try
            {
                // get 
                eCouchbaseBucket bucketType = eCouchbaseBucket.DEFAULT;
                if (!Enum.TryParse<eCouchbaseBucket>(bucket, true, out bucketType))
                {
                    log.ErrorFormat("Error while trying to modify CB. bucket type wasn't found: {0}", bucket);
                    return false;
                }

                var cbManager = new CouchbaseManager.CouchbaseManager(bucketType);

                switch (action)
                {
                    case eDbActionType.Delete:
                        return cbManager.Remove<string>(key);

                    case eDbActionType.Add:

                        bool result;
                        if (ttlMinutes > 0)
                            result = cbManager.Set(key, data, (uint) ttlMinutes * 60);
                        else
                            result = cbManager.Set(key, data);

                        if (!result)
                        {
                            log.ErrorFormat("error while trying to add data to CB. bucket: {0), key: {1}, data: {2}", bucket, key, data);
                            return false;
                        }
                        else
                        {
                            log.DebugFormat("successfully added data to CB. bucket: {0), key: {1}, data: {2}", bucket, key, data);
                            return true;
                        }

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
            }

            return false;
        }
    }
}
