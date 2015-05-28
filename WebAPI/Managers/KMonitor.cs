using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using log4net;
using System.Web;
using System.Net.Http;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Logger
{
    [Serializable]
    public class KMonitor : IDisposable
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger("KMonitor");

        [DataMember(Name = "e")]
        public string Event { get; set; }
        [DataMember(Name = "s")]
        public string Server { get; set; }
        [DataMember(Name = "i")]
        public string IPAddress { get; set; }
        [DataMember(Name = "u")]
        public string UniqueID { get; set; }
        [DataMember(Name = "p")]
        public string PartnerID { get; set; }
        [DataMember(Name = "a")]
        public string Action { get; set; }
        [DataMember(Name = "l")]
        public string ClientTag { get; set; }
        [DataMember(Name = "r")]
        public string ErrorCode { get; set; }
        [DataMember(Name = "t")]
        public string Table { get; set; }
        [DataMember(Name = "q")]
        public string QueryType { get; set; }
        [DataMember(Name = "d")]
        public string Database { get; set; }
        [DataMember(Name = "x")]
        public double ExecutionTime
        {
            get
            {
                return this.Watch.Elapsed.TotalSeconds;
            }
        }

        private Stopwatch Watch { get; set; }

        public const string EVENT_API_START = "start";
        private const string EVENT_API_END = "end";
        public const string EVENT_DATABASE = "db";
        public const string EVENT_SPHINX = "sphinx";
        public const string EVENT_CONNTOOK = "conn";
        public const string EVENT_DUMPFILE = "file";

        public KMonitor(string eventName, string groupID, string action, string uniqueID, string clientTag)
        {
            this.Watch = new Stopwatch();
            this.Watch.Start();

            this.Event = eventName;
            this.Server = Environment.MachineName;
            this.IPAddress = HttpContext.Current.Request.UserHostAddress;
            this.UniqueID = uniqueID;
            this.PartnerID = groupID;
            this.Action = action;
            this.ClientTag = clientTag;

            /* In case this is a start event, we fire it first, and on dispose, we will fire the END */
            if (eventName == EVENT_API_START)
            {
                logger.Monitor(this.ToString());
            }
        }

        public KMonitor(string eventName, string groupID)
            : this(eventName, groupID, HttpContext.Current.Items["kmon_action"].ToString(),
            HttpContext.Current.Items["kmon_req_id"].ToString(), HttpContext.Current.Request.UserAgent)
        {

        }

        public virtual void Dispose()
        {
            this.Watch.Stop();

            if (this.Event == EVENT_API_START)
            {
                /* We are firing the END event, so we just overriding the START */
                this.Event = EVENT_API_END;
            }

            logger.Monitor(this.ToString());
        }

        public override string ToString()
        {
            return Jil.JSON.Serialize<KMonitor>(this, new Jil.Options(excludeNulls: true));
        }
    }

    public static class ILogExtentions
    {
        public static void Monitor(this ILog log, string message, Exception exception)
        {
            log.Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType,
                log4net.Core.Level.Trace, message, exception);
        }

        public static void Monitor(this ILog log, string message)
        {
            log.Monitor(message, null);
        }
    }
}
