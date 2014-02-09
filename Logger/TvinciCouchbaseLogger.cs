using System;
using System.Collections.Generic;
using System.Text;
using log4net;
using log4net.Appender;
using Newtonsoft.Json;
using Couchbase;
using Enyim.Caching.Memcached;
using Couchbase.Extensions;
using Couchbase.Configuration;
using System.Web;
using System.Linq;

namespace Logger
{
    public abstract class RepositoryBase<T> where T : DebugAppender
    {
        protected static CouchbaseClient _Client { get; set; }
        static RepositoryBase()
        {
            _Client = new CouchbaseClient();
        }
    }

    class TvinciCouchbaseLogger : AppenderSkeleton
    {
        protected override void Append(log4net.Core.LoggingEvent loggingEvent)
        {
                string IP;
                try
                {
                    if (HttpContext.Current.Request == null)
                    {
                        IP = "NoIP";
                    }
                    else
                    {
                        IP = (HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] == null) ? HttpContext.Current.Request.UserHostAddress : HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    }
                }
                catch
                {
                    IP = "NoIP";
                }

                string key = loggingEvent.TimeStamp.ToString() + " " + IP;
                DebugInfo newDebugEntry = new DebugInfo
                {
                    IP = IP,
                    Datetime = loggingEvent.TimeStamp.ToString(),
                    Server = Environment.MachineName,
                    Severity = loggingEvent.Level.Name,
                    Source = loggingEvent.LocationInformation.FileName.Substring(loggingEvent.LocationInformation.FileName.LastIndexOf('\\')+1),
                    Content = loggingEvent.MessageObject.ToString()
                };

                CouchbaseClient client = CouchbaseManager.Instance;
                bool result = client.StoreJson(StoreMode.Set, key, newDebugEntry);
        }

        public static class CouchbaseManager
        {
            private static CouchbaseClient _instance;
            private static object locker = new object();

            public static CouchbaseClient Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        lock (locker)
                        {
                            if (_instance == null)
                            {
                                _instance = new CouchbaseClient();
                            }
                        }
                    }
                    return _instance;
                }
            }
        }


        public class DebugInfo
        {
            [JsonProperty("IP")]
            public string IP { get; set; }

            [JsonProperty("Datetime")]
            public string Datetime { get; set; }

            [JsonProperty("Server")]
            public string Server { get; set; }

            [JsonProperty("Severity")]
            public string Severity { get; set; }

            [JsonProperty("Source")]
            public string Source { get; set; }

            [JsonProperty("Content")]
            public string Content { get; set; }
        }
    }
}
