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
    public class KLogger : IDisposable
    {
        private class LogEvent
        {
            public enum LogLevel { INFO, DEBUG, ERROR }
            public string Message { get; set; }
            public Exception Exception { get; set; }
            public LogLevel Level { get; set; }
        }

        private static readonly ILog logger = log4net.LogManager.GetLogger("KLogger");

        private string Server { get; set; }
        public string IPAddress { get; set; }
        public string UniqueID { get; set; }
        public string PartnerID { get; set; }
        public string Action { get; set; }
        public string ClientTag { get; set; }
        public string ErrorCode { get; set; }
        private List<LogEvent> logs;

        public KLogger(string groupID, string action)
        {
            this.logs = new List<LogEvent>();
            this.Server = Environment.MachineName;
            this.IPAddress = HttpContext.Current.Request.UserHostAddress;
            HttpRequestMessage httpRequestMessage = HttpContext.Current.Items["MS_HttpRequestMessage"] as HttpRequestMessage;
            this.UniqueID = httpRequestMessage.GetCorrelationId().ToString();
            this.PartnerID = groupID;
            this.Action = action;
            this.ClientTag = HttpContext.Current.Request.UserAgent;
        }

        private void handleEvent(string msg, Logger.KLogger.LogEvent.LogLevel level,
            bool isFlush, Exception ex = null)
        {
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
            return string.Format("{0} - s:{1} ip:{2} req:{3} partner:{4} action:{5} client:{6} error:{7} msg:{8}", creationDate, Server,
                IPAddress, UniqueID, PartnerID, Action, ClientTag, ErrorCode, msg);
        }

        private void sendLog(LogEvent logEvent)
        {
            switch (logEvent.Level)
            {
                case LogEvent.LogLevel.DEBUG:
                    logger.Debug(logEvent.Message, logEvent.Exception);
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

        public void Debug(string sMessage, bool isFlush = false)
        {
            handleEvent(sMessage, Logger.KLogger.LogEvent.LogLevel.DEBUG, isFlush);
        }

        public void Info(string sMessage, bool isFlush = false)
        {
            handleEvent(sMessage, Logger.KLogger.LogEvent.LogLevel.INFO, isFlush);
        }

        public void Error(string sMessage, bool isFlush = false)
        {
            handleEvent(sMessage, Logger.KLogger.LogEvent.LogLevel.ERROR, isFlush);
        }

        public virtual void Dispose()
        {
            foreach (LogEvent e in logs)
            {
                sendLog(e);
            }

            logs.Clear();
        }
    }
}
