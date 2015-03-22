using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Logger
{
    public class BaseLog : IDisposable
    {
        #region Members

        private readonly ILogger4Net logger = Log4NetManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const string datePattern = @"yyyy-MM-dd HH:mm:ss.fff";

        private List<LogWithSeverity> logsFullMessage;

        #endregion

        #region Properties
        private const string WS_URL_DEFAULT = "Web Service";
        private const string sKey = "APPLICATION_NAME";
        public DateTime ObjectCreationDate { get; set; }   // In UtcNow format
        public double TimeTaken { get; set; }
        public string Id { get; set; }
        public string Message { get; set; }
        public string Service
        {
            get
            {
                return GetWSURL(sKey);
            }
        }

        public string Method { get; set; }
        public string Severity { get; set; }
        public eLogType Type { get; set; }
        public string HostName
        {
            get
            {
                return Environment.MachineName;
            }
        }
        public string UserAgent { get; set; }
        public string IP { get; set; }

        #endregion

        #region CTOR

        public BaseLog()
        {

        }

        public BaseLog(DateTime utcTime)
        {
            ObjectCreationDate = utcTime;
        }

        /// <summary>
        /// CTOR
        /// </summary>
        /// <param name="eLogType"></param>
        /// <param name="utcTime"></param>
        /// <param name="bShouldCreateId"></param>
        public BaseLog(eLogType eLogType, DateTime utcTime, bool bShouldCreateId)
        {
            this.Type = eLogType;
            ObjectCreationDate = utcTime;
            if (bShouldCreateId)
            {
                this.Id = Guid.NewGuid().ToString();
            }
            logsFullMessage = new List<LogWithSeverity>();
        }

        #endregion

        #region Private Functions

        private void InitMessageAndTimeSpan(string sMessage)
        {
            this.Message = sMessage;
            this.CalcTimeSpan(DateTime.UtcNow, this.ObjectCreationDate);
        }


        /// <summary>
        /// This function get a log message, its severity and isFlush flag.
        /// </summary>
        /// <param name="sMessage">Log's message</param>
        /// <param name="sSeverity">"INFO", "DEBUG", "ERROR"</param>
        /// <param name="isFlush">True  - The message will be written immediately
        ///                       False - The message will be added to queue and will be written on object disposal</param>
        private void HandleLog(string sMessage, bool isFlush)
        {
            InitMessageAndTimeSpan(sMessage);

            string sFullMessage = this.ToString();

            LogWithSeverity logWithSeverity = new LogWithSeverity(sFullMessage, this.Severity);

            // Immediate writing message
            if (isFlush)
            {
                this.WriteMessage(logWithSeverity);
            }
            else
            {
                this.logsFullMessage.Add(logWithSeverity); // Adding the object to a queue which will be flushed on object disposal event
            }
        }

        private void WriteMessage(LogWithSeverity item)
        {
            switch (item.LogSeverity)
            {
                case "INFO":
                    logger.Info(item.FullLogMessage);
                    break;
                case "DEBUG":
                    logger.Debug(item.FullLogMessage);
                    break;
                case "ERROR":
                    logger.Error(item.FullLogMessage);
                    break;
                default:
                    break;
            }
        }

        // This function is used to clean any characters from the message given
        private void formatJson()
        {
            if (!this.Message.StartsWith("{"))
            {
                this.Message = this.Message.PadLeft(this.Message.Length + 1, '"');
                this.Message = this.Message.PadRight(this.Message.Length + 1, '"');
            }
        }

        #endregion

        #region Public Functions

        public void Debug(string sMessage, bool isFlush = false)
        {
            this.Severity = "DEBUG";
            HandleLog(sMessage, isFlush);
        }

        public void Info(string sMessage, bool isFlush = false)
        {
            this.Severity = "INFO";
            HandleLog(sMessage, isFlush);
        }

        public void Error(string sMessage, bool isFlush = false)
        {
            this.Severity = "ERROR";
            HandleLog(sMessage, isFlush);
        }

        public void CalcTimeSpan(DateTime endOfOperationTime, DateTime startOperationTime)
        {
            TimeTaken = (endOfOperationTime - startOperationTime).TotalMilliseconds;
        }

        #endregion

        #region IDisposable implementation

        public virtual void Dispose()
        {
            foreach (LogWithSeverity item in this.logsFullMessage)
            {
                WriteMessage(item);
            }

            this.logsFullMessage.Clear();
        }

        #endregion

        #region Override Functions

        public override string ToString()
        {
            formatJson();

            string[] lines = {"\"" + "EventTime" + "\"" + ":" + "\"" + "{0}" + "\"",
                              "\"" + "Id"   + "\"" + ":" + "\"" + "{1}" + "\"", 
                              "\"" + "Service" + "\"" + ":" + "\"" + "{2}" + "\"",
                              "\"" + "Method" + "\"" + ":" + "\"" + "{3}" + "\"", 
                              "\"" + "Severity" + "\"" + ":" + "\"" + "{4}" + "\"", 
                              "\"" + "message" + "\"" + ":" + "{5}",
                              "\"" + "Type" + "\"" + ":" + "\"" + "{6}" + "\"",
                              "\"" + "TimeTaken" + "\"" + ":" + "{7}",
                              "\"" + "HostName" + "\"" + ":" + "\"" + "{8}" + "\"",
                              "\"" + "User Agent" + "\"" + ":" + "\"" + "{9}" + "\"",
                              "\"" + "IP" + "\"" + ":" + "\"" + "{10}" + "\""};

            string log = string.Format(string.Join(",", lines), DateTime.UtcNow.ToString(datePattern), this.Id, this.Service, this.Method, this.Severity, this.Message/* this.Message.Replace("\"", "''")*/, this.Type.ToString(), this.TimeTaken, this.HostName, this.UserAgent, this.IP);

            log = log.PadLeft(log.Length + 1, '{');
            log = log.PadRight(log.Length + 1, '}');

            return log;
        }

        #endregion

        public static string GetWSURL(string sKey)
        {
            string sWsUrl = string.Empty;

            try
            {
                sWsUrl = GetValueFromConfig(sKey);

                if (string.IsNullOrEmpty(sWsUrl))
                {
                    sWsUrl = WS_URL_DEFAULT;
                }
            }
            catch
            {
                sWsUrl = WS_URL_DEFAULT;
            }

            return sWsUrl;
        }

        public static string GetValueFromConfig(string sKey)
        {
            return Utils.GetTcmConfigValue(sKey);
        }
    }
}