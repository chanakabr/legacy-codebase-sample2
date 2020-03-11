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
using System.Runtime.CompilerServices;
using System.Xml;
using System.IO;

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
        private static string configurationFileName;
        public static LogicalThreadContextProperties LogContextData => LogicalThreadContext.Properties;

        #region Props

        public static KLogEnums.AppType AppType { get; set; }
        public static string UniqueStaticId { get; set; }
        public string ClassName { get; set; }
        private string Server { get; set; }
        public string Topic
        {
            get => LogContextData[Constants.TOPIC]?.ToString();
            set => LogContextData[Constants.TOPIC] = value;
        }
        public string LoggerName { get; set; }

        #endregion

        #region Ctors and dtor

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

            LogContextData[Constants.CLASS_NAME] = className;

            string server = !string.IsNullOrWhiteSpace(Server) ? Server : "null";
            LogContextData[Constants.SERVER] = server;
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

            LogContextData[Constants.CLASS_NAME] = className;

            string server = !string.IsNullOrWhiteSpace(Server) ? Server : "null";
            LogContextData[Constants.SERVER] = server;
        }

        ~KLogger()
        {
            Dispose(false);
        }

        #endregion

        #region Initialization and configuration

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

            configurationFileName = logConfigFile;
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
            
            if (SystemInfo.NullText == "(null)")
            {
                SystemInfo.NullText = "null";
            }

            configurationFileName = logConfigFile;
        }

        public static void Configure(string logConfigFile, KLogEnums.AppType appType, string uniqueID)
        {
            UniqueStaticId = uniqueID;
            Configure(logConfigFile, appType);
        }

        public static void ReconfigureFromFile(string logConfigFile)
        {
            try
            {
                var repository = GetLoggerRepository();
                var file = new System.IO.FileInfo(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, logConfigFile));
                repository.ResetConfiguration();
                
                log4net.Config.XmlConfigurator.Configure(repository, file);
                KMonitor.Reconfigure(logConfigFile);
            }
            catch
            {
            }
        }

        public static string GetConfigurationXML()
        {
            return File.ReadAllText(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, configurationFileName));
        }

        public static void Reconfigure(string configurationXML)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(configurationXML);
                var element = (XmlElement)xmlDocument.SelectSingleNode("//configuration/log4net");

                if (element != null)
                {
                    KLogger.Reconfigure(element);
                    KMonitor.Reconfigure(element);
                }
            }
            catch
            {
            }
        }

        public static void Reconfigure(XmlElement xml)
        {
            var repository = GetLoggerRepository();
            repository.ResetConfiguration();
            log4net.Config.XmlConfigurator.Configure(repository, xml);
        }

        public static void SetLogLevel(log4net.Core.Level level)
        {
            var repostitory = GetLoggerRepository();
            repostitory.Threshold = level;
            ((log4net.Repository.Hierarchy.Hierarchy)repostitory).RaiseConfigurationChanged(EventArgs.Empty);
        }

        public static log4net.Core.Level GetLogLevel()
        {
            var repostitory = GetLoggerRepository();
            return repostitory.Threshold;
        }

        #endregion

        #region Getters and setters

        public static string GetRequestId() => LogContextData[Constants.REQUEST_ID_KEY]?.ToString();

        public static void SetRequestId(string sessionId) => LogContextData[Constants.REQUEST_ID_KEY] = sessionId;

        public static void SetAction(string action) => LogContextData[Constants.ACTION] = action;

        public static void SetTopic(string topic) => LogContextData[Constants.TOPIC] = topic;

        public static void SetGroupId(string groupId) => LogContextData[Constants.GROUP_ID] = groupId;

        #endregion

        internal static ILoggerRepository GetLoggerRepository()
        {
            if (_LogRepository != null) return _LogRepository;

            var repo = LogManager.GetAllRepositories().FirstOrDefault();
            _LogRepository = repo;
            return repo;
        }

        #region Message Handling

        private void HandleEvent(string msg, KLogger.LogEvent.LogLevel level, bool isFlush, object[] args, Exception ex = null, string callerMemberName = null)
        {
            try
            {
                if (args != null && ex != null)
                    throw new Exception("Args and Exception cannot co exist");

                SetTopic();

                var le = new LogEvent
                {
                    Message = msg,
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

        #endregion

        #region Logging Methods

        public void Debug(string sMessage, Exception ex = null, [CallerMemberName]string callerMemberName = null)
        {
            HandleEvent(sMessage != null ? sMessage : string.Empty, KLogger.LogEvent.LogLevel.DEBUG, true, null, ex, callerMemberName);
        }

        public void DebugFormat(string format, params object[] args)
        {
            HandleEvent(format, KLogger.LogEvent.LogLevel.DEBUG, true, args, null);
        }

        public void Info(string sMessage, Exception ex = null, [CallerMemberName]string callerMemberName = null)
        {
            HandleEvent(sMessage != null ? sMessage : string.Empty, KLogger.LogEvent.LogLevel.INFO, true, null, ex, callerMemberName);
        }

        public void InfoFormat(string format, params object[] args)
        {
            HandleEvent(format, KLogger.LogEvent.LogLevel.INFO, true, args, null);
        }

        public void Warn(string sMessage, Exception ex = null, [CallerMemberName]string callerMemberName = null)
        {
            HandleEvent(sMessage != null ? sMessage : string.Empty, KLogger.LogEvent.LogLevel.WARNING, true, null, ex, callerMemberName);
        }

        public void WarnFormat(string format, params object[] args)
        {
            HandleEvent(format, KLogger.LogEvent.LogLevel.WARNING, true, args, null);
        }

        public void Error(string sMessage, Exception ex = null, [CallerMemberName]string callerMemberName = null)
        {
            HandleEvent(sMessage != null ? sMessage : string.Empty, KLogger.LogEvent.LogLevel.ERROR, true, null, ex, callerMemberName);
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

        #region Private auxillary methods

        private void SetTopic()
        {
            string topic = this.Topic;
            string topicContextData = LogContextData[Constants.TOPIC]?.ToString();

            if (!string.IsNullOrWhiteSpace(topicContextData))
            {
                topic = topicContextData;
            }

            topic = !string.IsNullOrWhiteSpace(topic) ? topic : "null";
            LogContextData[Constants.TOPIC_LOG] = topic;
        }

        private void SetMethodName()
        {
            var stackTrace = new StackTrace();         // get call stack
            var stackFrames = stackTrace.GetFrames();  // get method calls (frames)

            if (stackFrames != null && stackFrames.Length > 2)
            {
                var callingFrame = stackFrames[2];
                //this.MethodName = callingFrame.GetMethod().Name;
            }
        }

        private string FormatMessage(string msg, DateTime creationDate)
        {
            string className = !string.IsNullOrWhiteSpace(ClassName) ? ClassName : "null";
            LogContextData[Constants.CLASS_NAME] = ClassName;

            string server = !string.IsNullOrWhiteSpace(Server) ? Server : "null";
            LogContextData[Constants.SERVER] = server;

            string topic = this.Topic;
            string topicContextData = LogContextData[Constants.TOPIC]?.ToString();

            if (!string.IsNullOrWhiteSpace(topicContextData))
            {
                topic = topicContextData;
            }

            topic = !string.IsNullOrWhiteSpace(topic) ? topic : "null";
            LogContextData[Constants.TOPIC_LOG] = topic;

            string ip = LogContextData[Constants.HOST_IP]?.ToString();
            ip = !string.IsNullOrWhiteSpace(ip) ? ip : "null";

            string reqid = LogContextData[Constants.REQUEST_ID_KEY]?.ToString();
            reqid = !string.IsNullOrWhiteSpace(reqid) ? reqid : "null";

            string partner = LogContextData[Constants.GROUP_ID]?.ToString();
            partner = !string.IsNullOrWhiteSpace(partner) ? partner : "null";

            string action = LogContextData[Constants.ACTION]?.ToString();
            action = !string.IsNullOrWhiteSpace(action) ? action : "null";

            string uid = LogContextData[Constants.USER_ID]?.ToString();
            uid = !string.IsNullOrWhiteSpace(uid) ? uid : "0";

            string message = !string.IsNullOrWhiteSpace(msg) ? msg : "null";

            //$"class:{className} topic:{topic} server:{server} ip:{ip} reqid:{reqid} partner:{partner} action:{action} uid:{uid} msg:{message}";
            return !string.IsNullOrWhiteSpace(msg) ? msg : "null";
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

        #endregion

        #region Dispose

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

        #endregion

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

