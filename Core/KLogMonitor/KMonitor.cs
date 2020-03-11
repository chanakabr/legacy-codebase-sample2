using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using log4net;
using log4net.Util;
using Microsoft.Win32.SafeHandles;


namespace KLogMonitor
{
    [Serializable]
    public class KMonitor : IDisposable
    {
        private static readonly ILog _Logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static LogicalThreadContextProperties LogContextData => LogicalThreadContext.Properties;
        private bool _Disposed = false;

        public static string UniqueStaticId { get; set; }
        public static KLogEnums.AppType AppType { get; set; }

        private const string MUTLIREQUEST_ACTION = "multirequest";

        [Newtonsoft.Json.JsonProperty(PropertyName = "m")]
        [DataMember(Name = "z")]
        public string IsMultiRequest { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "m")]
        [DataMember(Name = "m")]
        public string TimeInTicks { get; set; }

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

        public KLogMonitor.KLogEnums.eDBQueryType QueryType
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

        [Newtonsoft.Json.JsonProperty(PropertyName = "w")]
        [DataMember(Name = "w")]
        public string IsWritable { get; set; }

        private Stopwatch Watch { get; set; }

        public KMonitor(Events.eEvent eventName, string groupID = null, string action = null, string uniqueID = null, string clientTag = null)
        {
            try
            {
                // start counter
                this.Watch = new Stopwatch();
                this.Watch.Start();
                this.TimeInTicks = (DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1).ToUniversalTime().Ticks).ToString();
                this.IsMultiRequest = "0";

                this.Event = Events.GetEventString(eventName);
                this.Server = Environment.MachineName;

                // get monitor data from context
                // WCF -> data is stored in IncomingMessageProperties
                // WS  -> data is stored in OperationContext
                UpdateMonitorData();

                // check if current constructor overwrites any of the context data
                if (groupID != null)
                    this.PartnerID = groupID;

                if (action != null)
                    this.Action = action;

                if (uniqueID != null)
                    this.UniqueID = uniqueID;

                if (clientTag != null)
                    this.ClientTag = clientTag;

                // In case this is a start event, we fire it first, and on dispose, we will fire the END 
                if (eventName == Events.eEvent.EVENT_API_START || eventName == Events.eEvent.EVENT_CLIENT_API_START)
                    _Logger.Monitor(this.ToString());
            }
            catch (Exception logException)
            {
                _Logger.ErrorFormat("Kmonitor Error in constructor on action: {0}. EX: {1}", eventName.ToString(), logException);
            }
        }

        private void UpdateMonitorData()
        {
            this.TimeInTicks = (DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1).ToUniversalTime().Ticks).ToString();

            this.PartnerID = LogContextData[Constants.GROUP_ID]?.ToString();
            this.Action = LogContextData[Constants.ACTION]?.ToString();
            this.UniqueID = LogContextData[Constants.REQUEST_ID_KEY]?.ToString();
            this.ClientTag = LogContextData[Constants.CLIENT_TAG]?.ToString();
            this.IPAddress = LogContextData[Constants.HOST_IP]?.ToString();
            this.IsMultiRequest = LogContextData[Constants.MULTIREQUEST]?.ToString();
        }

        ~KMonitor()
        {
            Dispose(false);
        }

        public static void SetAppType(KLogEnums.AppType appType)
        {
            AppType = appType;
        }

        public static void Configure(string logConfigFile, KLogEnums.AppType appType)
        {
            AppType = appType;
            var loggerRepository = KLogger.GetLoggerRepository();
            if (!loggerRepository.Configured)
            {
                var file = new System.IO.FileInfo(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, logConfigFile));
                log4net.Config.XmlConfigurator.Configure(loggerRepository, file);
            }
        }

        public static void Configure(string logConfigFile, KLogEnums.AppType appType, string UniqueId)
        {
            UniqueStaticId = UniqueId;
            Configure(logConfigFile, appType);
        }

        public override string ToString()
        {
            try
            {
                return Jil.JSON.Serialize(this, new Jil.Options(excludeNulls: true));
            }
            catch (Exception)
            {
                // first load - there may be an exception - ignore it!
            }

            return string.Empty;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_Disposed)
                return;

            if (disposing)
            {
                //dispose managed resources
                this.Watch.Stop();

                if (this.Event == Events.GetEventString(Events.eEvent.EVENT_API_START))
                {
                    /* We are firing the END event, so we just overriding the START */
                    this.Event = Events.GetEventString(Events.eEvent.EVENT_API_END);
                    // check if data from context was updated (needed to add action to end_api log)
                    UpdateMonitorData();
                }

                if (this.Event == Events.GetEventString(Events.eEvent.EVENT_CLIENT_API_START))
                {
                    /* We are firing the END event, so we just overriding the START */
                    this.Event = Events.GetEventString(Events.eEvent.EVENT_CLIENT_API_END);
                    // check if data from context was updated (needed to add action to end_api log)
                    UpdateMonitorData();
                }

                _Logger.Monitor(this.ToString());
            }

            // Free any unmanaged objects here.
            //
            _Disposed = true;
        }

        public void Dispose()
        {

            try
            {
                Dispose(true);
            }
            catch (Exception logException)
            {
                _Logger.ErrorFormat("Kmonitor Error in destructor on action: {0}. EX: {1}", this.Event != null ? this.Event : string.Empty, logException);
            }
            GC.SuppressFinalize(this);
        }
    }

    public static class ILogExtentions
    {
        public static void Monitor(this ILog log, string message, Exception exception)
        {
            log.Logger.Log(MethodBase.GetCurrentMethod().DeclaringType,
                log4net.Core.Level.Trace, message, exception);
        }

        public static void Monitor(this ILog log, string message)
        {
            log.Monitor(message, null);
        }
    }

}
