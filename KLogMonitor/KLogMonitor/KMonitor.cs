using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Web;
using log4net;
using Microsoft.Win32.SafeHandles;


namespace KLogMonitor
{
    [Serializable]
    public class KMonitor : IDisposable
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static string UniqueStaticId { get; set; }
        public static KLogEnums.AppType AppType { get; set; }
        private bool disposed = false;
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

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
                    logger.Monitor(this.ToString());
            }
            catch (Exception logException)
            {
                logger.ErrorFormat("Kmonitor Error in constructor on action: {0}", eventName.ToString(), logException);
            }
        }

        private void UpdateMonitorData()
        {
            switch (AppType)
            {
                case KLogEnums.AppType.WCF:

                    if (OperationContext.Current != null && OperationContext.Current.IncomingMessageProperties != null)
                    {
                        object temp;
                        if (OperationContext.Current.IncomingMessageProperties.TryGetValue(Constants.GROUP_ID, out temp))
                            this.PartnerID = temp.ToString();

                        if (OperationContext.Current.IncomingMessageProperties.TryGetValue(Constants.ACTION, out temp))
                            this.Action = temp.ToString();

                        if (OperationContext.Current.IncomingMessageProperties.TryGetValue(Constants.REQUEST_ID_KEY, out temp))
                            this.UniqueID = temp.ToString();

                        if (OperationContext.Current.IncomingMessageProperties.TryGetValue(Constants.CLIENT_TAG, out temp))
                            this.ClientTag = temp.ToString();

                        if (OperationContext.Current.IncomingMessageProperties.TryGetValue(Constants.HOST_IP, out temp))
                            this.IPAddress = temp.ToString();
                    }
                    break;

                case KLogEnums.AppType.WindowsService:

                    this.UniqueID = UniqueStaticId;
                    break;

                case KLogEnums.AppType.WS:
                default:

                    if (HttpContext.Current != null && HttpContext.Current.Items != null)
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
                    break;
            }
        }

        ~KMonitor()
        {
            Dispose(false);
        }

        public static void Configure(string logConfigFile, KLogEnums.AppType appType)
        {
            AppType = appType;
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, logConfigFile)));
        }

        public static void Configure(string logConfigFile, KLogEnums.AppType appType, string UniqueId)
        {
            AppType = appType;
            UniqueStaticId = UniqueId;
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, logConfigFile)));
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

        //protected virtual void Dispose(bool disposing)
        //{
        //    if (!disposed)
        //    {
        //        if (disposing)
        //        {
        //            //dispose managed resources
        //            this.Watch.Stop();

        //            if (this.Event == Events.GetEventString(Events.eEvent.EVENT_API_START))
        //            {
        //                /* We are firing the END event, so we just overriding the START */
        //                this.Event = Events.GetEventString(Events.eEvent.EVENT_API_END);
        //            }

        //            if (this.Event == Events.GetEventString(Events.eEvent.EVENT_CLIENT_API_START))
        //            {
        //                /* We are firing the END event, so we just overriding the START */
        //                this.Event = Events.GetEventString(Events.eEvent.EVENT_CLIENT_API_END);
        //            }

        //            // check if data from context was updated
        //            UpdateMonitorData();

        //            logger.Monitor(this.ToString());
        //        }
        //    }
        //    //dispose unmanaged resources
        //    disposed = true;
        //}

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                handle.Dispose();
                //dispose managed resources
                this.Watch.Stop();

                if (this.Event == Events.GetEventString(Events.eEvent.EVENT_API_START))
                {
                    /* We are firing the END event, so we just overriding the START */
                    this.Event = Events.GetEventString(Events.eEvent.EVENT_API_END);
                }

                if (this.Event == Events.GetEventString(Events.eEvent.EVENT_CLIENT_API_START))
                {
                    /* We are firing the END event, so we just overriding the START */
                    this.Event = Events.GetEventString(Events.eEvent.EVENT_CLIENT_API_END);
                }

                // check if data from context was updated
                UpdateMonitorData();

                logger.Monitor(this.ToString());
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }

        public void Dispose()
        {

            try
            {
                Dispose(true);
            }
            catch (Exception logException)
            {
                logger.ErrorFormat("Kmonitor Error in destructor on action: {0}", this.Event != null ? this.Event : string.Empty, logException);
            }
            GC.SuppressFinalize(this);
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
