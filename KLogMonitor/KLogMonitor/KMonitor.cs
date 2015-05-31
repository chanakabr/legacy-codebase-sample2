using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using log4net;


namespace KLogMonitor
{
    [Serializable]
    public class KMonitor : IDisposable
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
        public string QueryTypeString { get; private set; }

        public Events.eDBQueryType QueryType
        {
            set { this.QueryTypeString = value.ToString(); }
        }

        [DataMember(Name = "d")]
        public string Database { get; set; }

        [DataMember(Name = "x")]
        public string ExecutionTime
        {
            get
            {
                return this.Watch.Elapsed.TotalSeconds.ToString("0.0000000000");
            }
        }

        private Stopwatch Watch { get; set; }

        public KMonitor(Events.eEvent eventName, string groupID = null, string action = null, string uniqueID = null, string clientTag = null)
        {
            // start counter
            this.Watch = new Stopwatch();
            this.Watch.Start();

            this.Event = Events.GetEventString(eventName);
            this.Server = Environment.MachineName;

            // If used under WEB
            if (HttpContext.Current != null &&
                HttpContext.Current.Items != null)
            {
                if (HttpContext.Current.Items[Constants.GROUP_ID] != null)
                    this.PartnerID = HttpContext.Current.Items[Constants.GROUP_ID].ToString();

                if (HttpContext.Current.Items[Constants.ACTION] != null)
                    this.Action = HttpContext.Current.Items[Constants.ACTION].ToString();

                if (HttpContext.Current.Items[Constants.REQUEST_ID_KEY] != null)
                    this.UniqueID = HttpContext.Current.Items[Constants.REQUEST_ID_KEY].ToString();

                if (HttpContext.Current.Items[Constants.CLIENT_TAG] != null)
                    this.ClientTag = HttpContext.Current.Items[Constants.CLIENT_TAG].ToString();

                if (HttpContext.Current.Items[Constants.HOST_IP] != null)
                    this.IPAddress = HttpContext.Current.Items[Constants.HOST_IP].ToString();
            }

            if (groupID != null)
                this.PartnerID = groupID;

            if (action != null)
                this.Action = action;

            if (uniqueID != null)
                this.UniqueID = uniqueID;

            if (clientTag != null)
                this.ClientTag = clientTag;

            /* In case this is a start event, we fire it first, and on dispose, we will fire the END */
            if (eventName == Events.eEvent.EVENT_API_START)
                logger.Monitor(this.ToString());
        }

        public static void Configure(string logConfigFile)
        {
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, logConfigFile)));
        }

        public virtual void Dispose()
        {
            this.Watch.Stop();

            if (this.Event == Events.GetEventString(Events.eEvent.EVENT_API_START))
            {
                /* We are firing the END event, so we just overriding the START */
                this.Event = Events.GetEventString(Events.eEvent.EVENT_API_END);
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
