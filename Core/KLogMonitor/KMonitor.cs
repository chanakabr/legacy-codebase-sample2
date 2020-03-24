using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using log4net;
using log4net.Core;
using log4net.Filter;
using log4net.Layout;
using log4net.Layout.Pattern;
using log4net.Util;
using Microsoft.Win32.SafeHandles;


namespace KLogMonitor
{
    [Serializable]
    public class KMonitor : IDisposable
    {
        public const string TOTAL_MILLISECONDS = "totalMilliseconds";

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
                this.Watch = new Stopwatch();
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
                    // -1 milliseconds to force it down the filter
                    _Logger.Monitor(this, -1, null);

                // start counter
                this.Watch.Start();
                this.TimeInTicks = (DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1).ToUniversalTime().Ticks).ToString();
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

        public static void Reconfigure(string logConfigFile)
        {
            var repository = KLogger.GetLoggerRepository();
            var file = new System.IO.FileInfo(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, logConfigFile));
            repository.ResetConfiguration();

            log4net.Config.XmlConfigurator.Configure(repository, file);
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

                var totalMillseconds = this.Watch.Elapsed.TotalMilliseconds;
                
                //var repo = _Logger.Logger.Repository;
                _Logger.Monitor(this, totalMillseconds, null);
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
        public static void Monitor(this ILog log, string message, double totalMilliseconds, Exception exception)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            var le = new LoggingEvent(MethodBase.GetCurrentMethod().DeclaringType,
                log.Logger.Repository,
                log.Logger.Name,
                log4net.Core.Level.Trace,
                message,
                exception
                );
            le.Properties[KMonitor.TOTAL_MILLISECONDS] = totalMilliseconds;

            log.Logger.Log(le);
        }
        public static void Monitor(this ILog log, KMonitor monitor, double totalMilliseconds, Exception exception)
        {
            var le = new LoggingEvent(MethodBase.GetCurrentMethod().DeclaringType,
                log.Logger.Repository,
                log.Logger.Name,
                log4net.Core.Level.Trace,
                monitor,
                exception
                );

            le.Properties[KMonitor.TOTAL_MILLISECONDS] = totalMilliseconds;
            le.Properties["m"] = monitor.TimeInTicks;
            le.Properties["e"] = monitor.Event;
            le.Properties["s"] = monitor.Server;
            le.Properties["i"] = monitor.IPAddress;
            le.Properties["u"] = monitor.UniqueID;
            le.Properties["p"] = monitor.PartnerID;
            le.Properties["a"] = monitor.Action;
            le.Properties["l"] = monitor.ClientTag;
            le.Properties["r"] = monitor.ErrorCode;
            le.Properties["t"] = monitor.Table;
            le.Properties["q"] = monitor.QueryTypeString;
            le.Properties["x"] = monitor.ExecutionTime;
            le.Properties["d"] = monitor.Database;
            le.Properties["w"] = monitor.IsWritable;
            le.Fix = FixFlags.Properties;

            log.Logger.Log(le);
        }

        public static void Monitor(this ILog log, string message, double totalMilliseconds)
        {
            log.Monitor(message, totalMilliseconds, null);
        }
    }

    public class KMonitorThresholdFilter : FilterSkeleton
    {
        /// <summary>
        /// minimum threshold time for logging monitors - in milliseconds
        /// </summary>
        public double minThreshold { get; set; }

        public KMonitorThresholdFilter() : base()
        {
            minThreshold = -1;
        }
        public override void ActivateOptions()
        {
            base.ActivateOptions();

            if (this.minThreshold < 0)
            {
                // set default to 500 miliseconds
                this.minThreshold = 500;
            }
        }

        public override FilterDecision Decide(LoggingEvent loggingEvent)
        {
            FilterDecision decision = FilterDecision.Neutral;

            object totalMilliSecondsObject = loggingEvent.LookupProperty(KMonitor.TOTAL_MILLISECONDS);

            if (totalMilliSecondsObject != null)
            {
                var totalMilliSeconds = (double)totalMilliSecondsObject;

                if (totalMilliSeconds > 0 && totalMilliSeconds < minThreshold)
                {
                    decision = FilterDecision.Deny;
                }
            }

            return decision;
        }
    }

    public class ReflectionReader : PatternLayoutConverter
    {
        public ReflectionReader()
        {
            _getValue = GetValueFirstTime;
        }

        protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
        {
            writer.Write(_getValue(loggingEvent.MessageObject));
        }

        private Func<object, String> _getValue;
        private string GetValueFirstTime(object source)
        {
            _targetProperty = source.GetType().GetProperty(Option);
            if (_targetProperty == null)
            {
                _getValue = x => "null";
            }
            else
            {
                _getValue = x => String.Format("{0}", _targetProperty.GetValue(x, null));
            }
            return _getValue(source);
        }

        private PropertyInfo _targetProperty;
    }

    public class ReflectionLayoutPattern : PatternLayout
    {
        public ReflectionLayoutPattern()
        {
            this.AddConverter("item", typeof(ReflectionReader));
        }
    }
}
