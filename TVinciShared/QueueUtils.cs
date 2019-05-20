using ApiObjects;
using KLogMonitor;
using QueueWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TVinciShared
{
    public class QueueUtils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static bool UpdateCache(int groupId, string bucket, string[] keys)
        {
            bool result = false;

            var queue = new UpdateCacheQueue();

            CouchbaseManager.eCouchbaseBucket couchbaseBucket = CouchbaseManager.eCouchbaseBucket.DEFAULT;

            if (Enum.TryParse<CouchbaseManager.eCouchbaseBucket>(bucket, out couchbaseBucket))
            {
                var data = new UpdateCacheData(groupId, bucket, keys);

                try
                {
                    result = queue.Enqueue(data, "PROCESS_UPDATE_CACHE");
                }
                catch (Exception ex)
                {
                    log.Error("UpdateCache - " +
                            string.Format("Error in UpdateCache: group = {0} ex = {1}, ST = {2}", groupId, ex.Message, ex.StackTrace),
                            ex);
                }
            }
            else
            {
                log.ErrorFormat("UpdateCache - invalid couchbase bucket received: {0}", bucket);
            }

            return result;
        }
    }
}
