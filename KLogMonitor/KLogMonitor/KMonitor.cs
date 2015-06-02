using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using log4net;


namespace KLogMonitor
{
    [Serializable]
    public class KMonitor : IDisposable
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [Newtonsoft.Json.JsonProperty(PropertyName = "e")]
        [DataMember(Name = "e")]
        public string Event { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "s")]
        [DataMember(Name = "s")]
        public string Server { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "i")]
        [DataMember(Name = "i")]
        public string IPAddress { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "u")]
        [DataMember(Name = "u")]
        public string UniqueID { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "p")]
        [DataMember(Name = "p")]
        public string PartnerID { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "a")]
        [DataMember(Name = "a")]
        public string Action { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "l")]
        [DataMember(Name = "l")]
        public string ClientTag { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "r")]
        [DataMember(Name = "r")]
        public string ErrorCode { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "t")]
        [DataMember(Name = "t")]
        public string Table { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "q")]
        [DataMember(Name = "q")]
        public string QueryTypeString { get; private set; }


        public Events.eDBQueryType QueryType
        {
            set { this.QueryTypeString = value.ToString(); }
        }

        [Newtonsoft.Json.JsonProperty(PropertyName = "d")]
        [DataMember(Name = "d")]
        public string Database { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "x")]
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
            try
            {
                Newtonsoft.Json.JsonSerializer _jsonWriter = new Newtonsoft.Json.JsonSerializer
            {
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
            };


#if RUNNING_ON_3_5
                return Newtonsoft.Json.JsonConvert.SerializeObject(this, new Newtonsoft.Json.JsonSerializerSettings() { NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore });
#endif
#if NOT_RUNNING_ON_3_5
                return Jil.JSON.Serialize<KMonitor>(this, new Jil.Options(excludeNulls: true));
#endif
            }
            catch (Exception)
            {
                // first load - there may be an exception - ignore it!
            }

            return string.Empty;
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
