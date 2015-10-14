using CachingProvider;
using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace UpdateCacheHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region ITaskHandler Members

        public string HandleTask(string data)
        {
            string result = "failure";

            try
            {
                log.InfoFormat("starting update cache task. data={0}", data);

                UpdateCacheRequest request = JsonConvert.DeserializeObject<UpdateCacheRequest>(data);

                List<string> nonRemovedKeys = RemoveKeysFromCache(request);

                result = "success";

                // log non removed keys
                if (nonRemovedKeys != null && nonRemovedKeys.Count > 0)
                {
                    string keys = string.Format("Non removed keys are: {0}", String.Join(",", nonRemovedKeys));

                    log.Info(keys);

                    result = string.Concat(result, ". ", keys);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

        private static List<string> RemoveKeysFromCache(UpdateCacheRequest request)
        {
            List<string> nonRemovedKeys = new List<string>();

            CouchBaseCache<object> client = CouchBaseCache<object>.GetInstance(request.Bucket.ToString());

            foreach (string key in request.Keys)
            {
                var removeResult = client.Remove(key);

                // If no response or result is negative
                if (removeResult == null || !Convert.ToBoolean(removeResult.result))
                {
                    nonRemovedKeys.Add(key);
                }
            }

            return nonRemovedKeys;
        }

        #endregion
    }
}
