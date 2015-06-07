using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
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
        public string UserID { get; set; }
        private string Server { get; set; }
        public string IPAddress { get; set; }
        public string MethodName { get; set; }


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

        private void handleEvent(string msg, KLogger.LogEvent.LogLevel level, bool isFlush, object[] args, Exception ex = null)
        {

            StackTrace stackTrace = new StackTrace();           // get call stack
            StackFrame[] stackFrames = stackTrace.GetFrames();  // get method calls (frames)

            StackFrame callingFrame = stackFrames[2];
            this.MethodName = callingFrame.GetMethod().Name;


            if (args != null && ex != null)
                throw new Exception("Args and Exception cannot co exist");

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

                if (HttpContext.Current.Items[Constants.USER_ID] != null)
                    this.UserID = HttpContext.Current.Items[Constants.USER_ID].ToString();
            }

            LogEvent le = new LogEvent()
            {
                Message = formatMessage(msg, DateTime.UtcNow),
                Exception = ex,
                Level = level,
                args = args
            };

            if (isFlush)
                sendLog(le);
            else
                logs.Add(le);
        }

        private string formatMessage(string msg, DateTime creationDate)
        {
            return string.Format("{0} - class: {1}, method: {2}, server:{3} ip:{4} reqid:{5} partner:{6} action:{7} client:{8} uid:{9} msg:{10}",
                creationDate,                                  // 0
                ClassName != null ? ClassName : string.Empty,  // 1
                MethodName != null ? MethodName : string.Empty,// 2
                Server != null ? Server : string.Empty,        // 3
                IPAddress != null ? IPAddress : string.Empty,  // 4
                UniqueID != null ? UniqueID : string.Empty,    // 5
                PartnerID != null ? PartnerID : string.Empty,  // 6
                Action != null ? Action : string.Empty,        // 7
                ClientTag != null ? ClientTag : string.Empty,  // 8
                UserID != null ? UserID : "0",                 // 9
                msg != null ? msg : string.Empty);             // 10
        }

        private void sendLog(LogEvent logEvent)
        {
            switch (logEvent.Level)
            {
                case LogEvent.LogLevel.DEBUG:

                    if (logEvent.args != null && logEvent.args.Count() > 0)
                        logger.DebugFormat(logEvent.Message, logEvent.args);
                    else
                        logger.Debug(logEvent.Message, logEvent.Exception);
                    break;

                case LogEvent.LogLevel.WARNING:

                    if (logEvent.args != null && logEvent.args.Count() > 0)
                        logger.WarnFormat(logEvent.Message, logEvent.args);
                    else
                        logger.Warn(logEvent.Message, logEvent.Exception);
                    break;

                case LogEvent.LogLevel.ERROR:

                    if (logEvent.args != null && logEvent.args.Count() > 0)
                        logger.ErrorFormat(logEvent.Message, logEvent.args);
                    else
                        logger.Error(logEvent.Message, logEvent.Exception);
                    break;

                case LogEvent.LogLevel.INFO:

                    if (logEvent.args != null && logEvent.args.Count() > 0)
                        logger.InfoFormat(logEvent.Message, logEvent.args);
                    else
                        logger.Info(logEvent.Message, logEvent.Exception);
                    break;

                default:

                    throw new Exception("Log level is unknown");
            }
        }

        public void Debug(string sMessage, Exception ex = null)
        {
            handleEvent(sMessage, KLogger.LogEvent.LogLevel.DEBUG, true, null, ex);
        }

        public void DebugFormat(string format, params object[] args)
        {
            handleEvent(format, KLogger.LogEvent.LogLevel.DEBUG, true, args, null);
        }

        public void Info(string sMessage, Exception ex = null)
        {
            handleEvent(sMessage, KLogger.LogEvent.LogLevel.INFO, true, null, ex);
        }

        public void InfoFormat(string format, params object[] args)
        {
            handleEvent(format, KLogger.LogEvent.LogLevel.INFO, true, args, null);
        }

        public void Warn(string sMessage, Exception ex = null)
        {
            handleEvent(sMessage, KLogger.LogEvent.LogLevel.WARNING, true, null, ex);
        }

        public void WarnFormat(string format, params object[] args)
        {
            handleEvent(format, KLogger.LogEvent.LogLevel.WARNING, true, args, null);
        }


        public void Error(string sMessage, Exception ex = null)
        {
            handleEvent(sMessage, KLogger.LogEvent.LogLevel.ERROR, true, null, ex);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            handleEvent(format, KLogger.LogEvent.LogLevel.ERROR, true, args, null);
        }

        public void DebugNoFlush(string sMessage, Exception ex = null)
        {
            handleEvent(sMessage, KLogger.LogEvent.LogLevel.DEBUG, false, null, ex);
        }

        public void DebugFormatNoFlush(string format, params object[] args)
        {
            handleEvent(format, KLogger.LogEvent.LogLevel.DEBUG, false, args, null);
        }

        public void InfoNoFlush(string sMessage, Exception ex = null)
        {
            handleEvent(sMessage, KLogger.LogEvent.LogLevel.INFO, false, null, ex);
        }

        public void InfoFormatNoFlush(string format, params object[] args)
        {
            handleEvent(format, KLogger.LogEvent.LogLevel.INFO, false, args, null);
        }

        public void WarnNoFlush(string sMessage, Exception ex = null)
        {
            handleEvent(sMessage, KLogger.LogEvent.LogLevel.WARNING, false, null, ex);
        }

        public void WarnFormatNoFlush(string format, params object[] args)
        {
            handleEvent(format, KLogger.LogEvent.LogLevel.WARNING, false, args, null);
        }

        public void ErrorNoFlush(string sMessage, Exception ex = null)
        {
            handleEvent(sMessage, KLogger.LogEvent.LogLevel.ERROR, false, null, ex);
        }

        public void ErrorFormatNoFlush(string format, params object[] args)
        {
            handleEvent(format, KLogger.LogEvent.LogLevel.ERROR, false, args, null);
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
            public object[] args { get; set; }
        }
    }
}

