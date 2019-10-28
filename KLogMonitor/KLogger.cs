using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using log4net;
using System.Collections.Concurrent;
using log4net.Core;
using log4net.Repository;
using log4net.Util;
using log4net.Appender;

namespace KLogMonitor
{
    [Serializable]
    public class KLogger : IDisposable
    {
        private static readonly ILog _Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private bool _Disposed = false;
        private static ConcurrentDictionary<string, ILog> _SeparateLogsMap = null;
        private static ILoggerRepository _LogRepository;
        private readonly List<LogEvent> _Logs;

        public static LogicalThreadContextProperties LogContextData => LogicalThreadContext.Properties;

        public static KLogEnums.AppType AppType { get; set; }
        public static string UniqueStaticId { get; set; }
        //public string UniqueID { get; set; }
        //public string PartnerID { get; set; }
        public string ClassName { get; set; }
        //public string Action { get; set; }
        //public string ClientTag { get; set; }
        //public string UserID { get; set; }
        private string Server { get; set; }
        //public string IPAddress { get; set; }
        //public string MethodName { get; set; }
        //public string Topic { get; set; }
        public string LoggerName { get; set; }


        public KLogger(string className, string separateLoggerName = null)
        {
            this._Logs = new List<LogEvent>();
            this.Server = Environment.MachineName;
            this.ClassName = className;
            if (_SeparateLogsMap == null)
            {
                _SeparateLogsMap = new ConcurrentDictionary<string, ILog>();
            }

            if (!string.IsNullOrEmpty(separateLoggerName))
            {
                var repo = GetLoggerRepository();
                if (!_SeparateLogsMap.TryAdd(separateLoggerName, LogManager.GetLogger(repo.Name, separateLoggerName)))
                {
                    throw new Exception(string.Format("Failed adding ILog with LoggerName: {0} to separateLogsMap", separateLoggerName));
                }

                this.LoggerName = separateLoggerName;
            }
            else
            {
                this.LoggerName = string.Empty;
            }

        }

        public KLogger(string className)
        {
            this._Logs = new List<LogEvent>();
            this.Server = Environment.MachineName;
            this.ClassName = className;
            this.LoggerName = string.Empty;
            if (_SeparateLogsMap == null)
            {
                _SeparateLogsMap = new ConcurrentDictionary<string, ILog>();
            }
        }

        ~KLogger()
        {
            Dispose(false);
        }

        public static void SetAppType(KLogEnums.AppType appType)
        {
            AppType = appType;
        }

        public static void InitLogger(string logConfigFile, KLogEnums.AppType appType, string defaultLogsPath)
        {
            var assembly = Assembly.GetEntryAssembly();
            var fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            var assemblyVersion = $"{fvi.FileMajorPart}_{fvi.FileMinorPart}_{fvi.FileBuildPart}";
            var logDir = Environment.GetEnvironmentVariable("API_LOG_DIR");
            var stdOutLogLevel = Environment.GetEnvironmentVariable("API_STD_OUT_LOG_LEVEL");

            logDir = logDir != null ? Environment.ExpandEnvironmentVariables(logDir) : defaultLogsPath;
            log4net.GlobalContext.Properties["LogDir"] = logDir;
            log4net.GlobalContext.Properties["ApiVersion"] = assemblyVersion;
            log4net.GlobalContext.Properties["LogName"] = assembly.GetName().Name;

            KMonitor.Configure(logConfigFile, appType);
            KLogger.Configure(logConfigFile, appType);

            if (!string.IsNullOrEmpty(stdOutLogLevel))
            {
                var loggerRepo = KLogger.GetLoggerRepository();
                var stdOutLogLevelThreshold = loggerRepo.LevelMap[stdOutLogLevel];
                var stdOutAppenders = loggerRepo.GetAppenders()
                    .Where(a => a is ManagedColoredConsoleAppender || a is ConsoleAppender)
                    .Cast<AppenderSkeleton>().ToList();

                stdOutAppenders.ForEach(a =>
                {
                    _Logger.Info($"Setting log-level threshold for std out logs, appender:[{a.Name}], threshold:[{stdOutLogLevelThreshold}]");
                    a.Threshold = stdOutLogLevelThreshold;
                });
            }
        }

        public static void Configure(string logConfigFile, KLogEnums.AppType appType)
        {
            AppType = appType;
            var repository = GetLoggerRepository();
            var file = new System.IO.FileInfo(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, logConfigFile));
            if (!repository.Configured)
            {
                log4net.Config.XmlConfigurator.Configure(repository, file);
            }
        }

        public static void Configure(string logConfigFile, KLogEnums.AppType appType, string uniqueID)
        {
            UniqueStaticId = uniqueID;
            Configure(logConfigFile, appType);
        }

        internal static ILoggerRepository GetLoggerRepository()
        {
            if (_LogRepository != null) return _LogRepository;

            var repo = LogManager.GetAllRepositories().FirstOrDefault();
            _LogRepository = repo;
            return repo;
        }

        private void HandleEvent(string msg, KLogger.LogEvent.LogLevel level, bool isFlush, object[] args, Exception ex = null)
        {
            try
            {
                var stackTrace = new StackTrace();         // get call stack
                var stackFrames = stackTrace.GetFrames();  // get method calls (frames)

                if (stackFrames != null && stackFrames.Length > 2)
                {
                    var callingFrame = stackFrames[2];
                    LogContextData[Constants.METHOD_NAME] = callingFrame.GetMethod().Name;
                }

                if (args != null && ex != null)
                    throw new Exception("Args and Exception cannot co exist");


                

                var le = new LogEvent
                {
                    Message = FormatMessage(msg, DateTime.UtcNow),
                    Exception = ex,
                    Level = level,
                    args = args
                };

                if (isFlush)
                    SendLog(le);
                else
                    _Logs.Add(le);
            }
            catch (Exception logException)
            {
                _Logger.ErrorFormat("Klogger Error in handle event. original log message: {0}, ex: {1}", msg, logException);
            }
        }

        private string FormatMessage(string msg, DateTime creationDate)
        {

            var ClientTag = LogContextData[Constants.CLIENT_TAG]?.ToString();
            var IPAddress = LogContextData[Constants.HOST_IP]?.ToString();
            var UniqueID = LogContextData[Constants.REQUEST_ID_KEY]?.ToString();
            var PartnerID = LogContextData[Constants.GROUP_ID]?.ToString();
            var Action = LogContextData[Constants.ACTION]?.ToString();
            var UserID = LogContextData[Constants.USER_ID]?.ToString();
            var Topic = LogContextData[Constants.TOPIC]?.ToString();
            var MethodName = LogContextData[Constants.METHOD_NAME]?.ToString();

            return string.Format("class:{0} topic:{1} method:{2} server:{3} ip:{4} reqid:{5} partner:{6} action:{7} uid:{8} msg:{9}",
                !string.IsNullOrWhiteSpace(ClassName) ? ClassName : "null",  // 0
                !string.IsNullOrWhiteSpace(Topic) ? Topic : "null",          // 1
                !string.IsNullOrWhiteSpace(MethodName) ? MethodName : "null",// 2
                !string.IsNullOrWhiteSpace(Server) ? Server : "null",        // 3 
                !string.IsNullOrWhiteSpace(IPAddress) ? IPAddress : "null",  // 4
                !string.IsNullOrWhiteSpace(UniqueID) ? UniqueID : "null",    // 5
                !string.IsNullOrWhiteSpace(PartnerID) ? PartnerID : "null",  // 6
                !string.IsNullOrWhiteSpace(Action) ? Action : "null",        // 7
                !string.IsNullOrWhiteSpace(UserID) ? UserID : "0",           // 8
                !string.IsNullOrWhiteSpace(msg) ? msg : "null");             // 9
        }

        private ILog TryGetSeparateLogger()
        {
            if (string.IsNullOrEmpty(this.LoggerName)) return _Logger;

            ILog separateLogger;
            if (_SeparateLogsMap.TryGetValue(this.LoggerName, out separateLogger))
            {
                return separateLogger;
            }

            return _Logger;
        }

        private void SendLog(LogEvent logEvent)
        {
            var logger = TryGetSeparateLogger();

            if (logEvent.args != null && logEvent.args.Any())
            {
                switch (logEvent.Level)
                {
                    case LogEvent.LogLevel.INFO:
                        logger.InfoFormat(logEvent.Message, logEvent.args);
                        break;
                    case LogEvent.LogLevel.DEBUG:
                        logger.DebugFormat(logEvent.Message, logEvent.args);
                        break;
                    case LogEvent.LogLevel.WARNING:
                        logger.WarnFormat(logEvent.Message, logEvent.args);
                        break;
                    case LogEvent.LogLevel.ERROR:
                        logger.ErrorFormat(logEvent.Message, logEvent.args);
                        break;
                    default:
                        logger.DebugFormat(logEvent.Message, logEvent.args);
                        break;
                }
            }
            else
            {
                switch (logEvent.Level)
                {
                    case LogEvent.LogLevel.INFO:
                        logger.Info(logEvent.Message, logEvent.Exception);
                        break;
                    case LogEvent.LogLevel.DEBUG:
                        logger.Debug(logEvent.Message, logEvent.Exception);
                        break;
                    case LogEvent.LogLevel.WARNING:
                        logger.Warn(logEvent.Message, logEvent.Exception);
                        break;
                    case LogEvent.LogLevel.ERROR:
                        logger.Error(logEvent.Message, logEvent.Exception);
                        break;
                    default:
                        logger.Debug(logEvent.Message, logEvent.Exception);
                        break;
                }

            }


        }

        #region Logging Methods
        public void Debug(string sMessage, Exception ex = null)
        {
            HandleEvent(sMessage != null ? sMessage : string.Empty, KLogger.LogEvent.LogLevel.DEBUG, true, null, ex);
        }

        public void DebugFormat(string format, params object[] args)
        {
            HandleEvent(format, KLogger.LogEvent.LogLevel.DEBUG, true, args, null);
        }

        public void Info(string sMessage, Exception ex = null)
        {
            HandleEvent(sMessage != null ? sMessage : string.Empty, KLogger.LogEvent.LogLevel.INFO, true, null, ex);
        }

        public void InfoFormat(string format, params object[] args)
        {
            HandleEvent(format, KLogger.LogEvent.LogLevel.INFO, true, args, null);
        }

        public void Warn(string sMessage, Exception ex = null)
        {
            HandleEvent(sMessage != null ? sMessage : string.Empty, KLogger.LogEvent.LogLevel.WARNING, true, null, ex);
        }

        public void WarnFormat(string format, params object[] args)
        {
            HandleEvent(format, KLogger.LogEvent.LogLevel.WARNING, true, args, null);
        }

        public void Error(string sMessage, Exception ex = null)
        {
            HandleEvent(sMessage != null ? sMessage : string.Empty, KLogger.LogEvent.LogLevel.ERROR, true, null, ex);
        }

        public static string GetRequestId()
        {
            return LogContextData[Constants.REQUEST_ID_KEY]?.ToString();
        }

        public static void SetRequestId(string sessionId)
        {
            LogContextData[Constants.REQUEST_ID_KEY] = sessionId;
        }

        public void ErrorFormat(string format, params object[] args)
        {
            HandleEvent(format, KLogger.LogEvent.LogLevel.ERROR, true, args, null);
        }

        public void DebugNoFlush(string sMessage, Exception ex = null)
        {
            HandleEvent(sMessage != null ? sMessage : string.Empty, KLogger.LogEvent.LogLevel.DEBUG, false, null, ex);
        }

        public void DebugFormatNoFlush(string format, params object[] args)
        {
            HandleEvent(format, KLogger.LogEvent.LogLevel.DEBUG, false, args, null);
        }

        public void InfoNoFlush(string sMessage, Exception ex = null)
        {
            HandleEvent(sMessage != null ? sMessage : string.Empty, KLogger.LogEvent.LogLevel.INFO, false, null, ex);
        }

        public void InfoFormatNoFlush(string format, params object[] args)
        {
            HandleEvent(format, KLogger.LogEvent.LogLevel.INFO, false, args, null);
        }

        public void WarnNoFlush(string sMessage, Exception ex = null)
        {
            HandleEvent(sMessage != null ? sMessage : string.Empty, KLogger.LogEvent.LogLevel.WARNING, false, null, ex);
        }

        public void WarnFormatNoFlush(string format, params object[] args)
        {
            HandleEvent(format, KLogger.LogEvent.LogLevel.WARNING, false, args, null);
        }

        public void ErrorNoFlush(string sMessage, Exception ex = null)
        {
            HandleEvent(sMessage != null ? sMessage : string.Empty, KLogger.LogEvent.LogLevel.ERROR, false, null, ex);
        }

        public void ErrorFormatNoFlush(string format, params object[] args)
        {
            HandleEvent(format, KLogger.LogEvent.LogLevel.ERROR, false, args, null);
        }
        #endregion

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_Disposed)
                return;

            if (disposing)
            {
                //dispose managed resources
                foreach (LogEvent e in _Logs)
                    SendLog(e);

                _Logs.Clear();
            }

            _Disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

