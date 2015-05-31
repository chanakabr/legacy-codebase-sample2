using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using log4net;


namespace KLogMonitor
{
    [Serializable]
    public class KLogger : IDisposable
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string UniqueID { get; set; }
        public string PartnerID { get; set; }
        public string ClassName { get; set; }
        public string Action { get; set; }
        public string ClientTag { get; set; }
        public string ErrorCode { get; set; }
        private string Server { get; set; }
        public string IPAddress { get; set; }

        private List<LogEvent> logs;

        public KLogger(string className)
        {
            this.logs = new List<LogEvent>();
            this.Server = Environment.MachineName;
            this.ClassName = className;
        }

        public static void Configure(string logConfigFile)
        {
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, logConfigFile)));
        }

        private void handleEvent(string msg, KLogger.LogEvent.LogLevel level, bool isFlush, Exception ex = null)
        {
            if (HttpContext.Current != null && HttpContext.Current.Items != null)
            {
                if (HttpContext.Current.Items[Constants.CLIENT_TAG] != null)
                    this.ClientTag = HttpContext.Current.Items[Constants.CLIENT_TAG].ToString();

                if (HttpContext.Current.Items[Constants.HOST_IP] != null)
                    this.IPAddress = HttpContext.Current.Items[Constants.HOST_IP].ToString();

                if (HttpContext.Current.Items[Constants.REQUEST_ID_KEY] != null)
                    this.UniqueID = HttpContext.Current.Items[Constants.REQUEST_ID_KEY].ToString();

                if (HttpContext.Current.Items[Constants.GROUP_ID] != null)
                    this.PartnerID = HttpContext.Current.Items[Constants.GROUP_ID].ToString();

                if (HttpContext.Current.Items[Constants.ACTION] != null)
                    this.Action = HttpContext.Current.Items[Constants.ACTION].ToString();
            }

            LogEvent le = new LogEvent()
            {
                Message = formatMessage(msg, DateTime.UtcNow),
                Exception = ex,
                Level = level
            };

            if (isFlush)
                sendLog(le);
            else
                logs.Add(le);
        }

        private string formatMessage(string msg, DateTime creationDate)
        {
            return string.Format("{0} - class: {1} server:{2} ip:{3} reqid:{4} partner:{5} action:{6} client:{7} error:{8} msg:{9}",
                creationDate,                                  // 0
                ClassName != null ? ClassName : string.Empty,  // 1
                Server != null ? Server : string.Empty,        // 2
                IPAddress != null ? IPAddress : string.Empty,  // 3
                UniqueID != null ? UniqueID : string.Empty,    // 4
                PartnerID != null ? PartnerID : string.Empty,  // 5
                Action != null ? Action : string.Empty,        // 6
                ClientTag != null ? ClientTag : string.Empty,  // 7
                ErrorCode != null ? ErrorCode : "0",           // 8
                msg != null ? msg : string.Empty);             // 9
        }

        private void sendLog(LogEvent logEvent)
        {
            switch (logEvent.Level)
            {
                case LogEvent.LogLevel.DEBUG:
                    logger.Debug(logEvent.Message, logEvent.Exception);
                    break;
                case LogEvent.LogLevel.WARNING:
                    logger.Warn(logEvent.Message, logEvent.Exception);
                    break;
                case LogEvent.LogLevel.ERROR:
                    logger.Error(logEvent.Message, logEvent.Exception);
                    break;
                case LogEvent.LogLevel.INFO:
                    logger.Info(logEvent.Message, logEvent.Exception);
                    break;
                default:
                    throw new Exception("Log level is unknown");
            }
        }

        public void Debug(string sMessage, bool isFlush = true)
        {
            handleEvent(sMessage, KLogger.LogEvent.LogLevel.DEBUG, isFlush);
        }

        public void DebugFormat(string format, bool isFlush = true, params object[] args)
        {
            string msg = string.Format(format, args);
            handleEvent(msg, KLogger.LogEvent.LogLevel.DEBUG, isFlush);
        }

        public void Info(string sMessage, bool isFlush = true)
        {
            handleEvent(sMessage, KLogger.LogEvent.LogLevel.INFO, isFlush);
        }

        public void InfoFormat(string format, bool isFlush = true, params object[] args)
        {
            string msg = string.Format(format, args);
            handleEvent(msg, KLogger.LogEvent.LogLevel.INFO, isFlush);
        }

        public void Warning(string sMessage, bool isFlush = true, Exception ex = null)
        {
            handleEvent(sMessage, KLogger.LogEvent.LogLevel.WARNING, isFlush, ex);
        }

        public void WarningFormat(string format, bool isFlush = true, Exception ex = null, params object[] args)
        {
            string msg = string.Format(format, args);
            handleEvent(msg, KLogger.LogEvent.LogLevel.WARNING, isFlush, ex);
        }


        public void Error(string sMessage, bool isFlush = true, Exception ex = null)
        {
            handleEvent(sMessage, KLogger.LogEvent.LogLevel.ERROR, isFlush, ex);
        }

        public void ErrorFormat(string format, bool isFlush = true, Exception ex = null, params object[] args)
        {
            string msg = string.Format(format, args);
            handleEvent(msg, KLogger.LogEvent.LogLevel.ERROR, isFlush, ex);
        }

        public virtual void Dispose()
        {
            foreach (LogEvent e in logs)
                sendLog(e);

            logs.Clear();
        }

        private class LogEvent
        {
            public enum LogLevel { INFO, DEBUG, WARNING, ERROR }
            public string Message { get; set; }
            public Exception Exception { get; set; }
            public LogLevel Level { get; set; }
        }
    }
}
