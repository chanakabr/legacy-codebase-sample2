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
using Microsoft.Extensions.Logging;
using System.Web;

namespace KLogMonitor
{
    [Serializable]
    public class KLogger : Microsoft.Extensions.Logging.ILogger
    {
        // this logger is used to log Klogger configuration etc...
        private static readonly ILog _InternalLogger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ILog _Logger;
        private static ILoggerRepository _LogRepository;
        private static string configurationFileName;

        public static LogicalThreadContextProperties LogContextData => LogicalThreadContext.Properties;

        public static KLogEnums.AppType AppType { get; set; }
        public static string UniqueStaticId { get; set; }
        public string ClassName { get; set; }
        private string Server { get; set; }


        #region Getters and setters

        public static string GetRequestId() => LogContextData[Constants.REQUEST_ID_KEY]?.ToString();

        public static void SetRequestId(string sessionId) => LogContextData[Constants.REQUEST_ID_KEY] = sessionId;

        public static void SetAction(string action) => LogContextData[Constants.ACTION] = action;

        public static void SetTopic(string topic) => LogContextData[Constants.TOPIC] = topic;

        public static void SetGroupId(string groupId) => LogContextData[Constants.GROUP_ID] = groupId;
        public string Topic
        {
            get => LogContextData[Constants.TOPIC]?.ToString();
            set => LogContextData[Constants.TOPIC] = value;
        }
        public string LoggerName { get; set; }

        #endregion



        [Obsolete("This CTOR is no longer relveant, all loggers are now seperate")]
        public KLogger(string className, string separateLoggerName = null) : this(separateLoggerName ?? className) { }

        public KLogger(string className)
        {
            var repo = GetLoggerRepository();
            _Logger = LogManager.GetLogger(repo.Name, className);
            Server = Environment.MachineName;
            ClassName = className;
            LoggerName = string.Empty;

            LogContextData[Constants.CLASS_NAME] = className;

            string server = !string.IsNullOrWhiteSpace(Server) ? Server : "null";
            LogContextData[Constants.SERVER] = server;
        }



        #region Initialization and configuration

        public static void SetAppType(KLogEnums.AppType appType) => AppType = appType;

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
                    _InternalLogger.Info($"Setting log-level threshold for std out logs, appender:[{a.Name}], threshold:[{stdOutLogLevelThreshold}]");
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



        internal static ILoggerRepository GetLoggerRepository()
        {
            if (_LogRepository != null) return _LogRepository;

            var repo = LogManager.GetAllRepositories().FirstOrDefault();
            _LogRepository = repo;
            return repo;
        }

        #region Message Handling

        private void HandleEvent(string msg, KLogger.LogEvent.LogLevel level, object[] args, Exception ex = null, string callerMemberName = null)
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

                SendLog(le);
            }
            catch (Exception logException)
            {
                _Logger.ErrorFormat("Klogger Error in handle event. original log message: {0}, ex: {1}", msg, logException);
            }
        }

        private void SendLog(LogEvent logEvent)
        {
            SetLogContextData();
            if (logEvent.args != null && logEvent.args.Any())
            {
                switch (logEvent.Level)
                {
                    case LogEvent.LogLevel.INFO:
                        _Logger.InfoFormat(logEvent.Message, logEvent.args);
                        break;
                    case LogEvent.LogLevel.DEBUG:
                        _Logger.DebugFormat(logEvent.Message, logEvent.args);
                        break;
                    case LogEvent.LogLevel.WARNING:
                        _Logger.WarnFormat(logEvent.Message, logEvent.args);
                        break;
                    case LogEvent.LogLevel.ERROR:
                        _Logger.ErrorFormat(logEvent.Message, logEvent.args);
                        break;
                    default:
                        _Logger.DebugFormat(logEvent.Message, logEvent.args);
                        break;
                }
            }
            else
            {
                switch (logEvent.Level)
                {
                    case LogEvent.LogLevel.INFO:
                        _Logger.Info(logEvent.Message, logEvent.Exception);
                        break;
                    case LogEvent.LogLevel.DEBUG:
                        _Logger.Debug(logEvent.Message, logEvent.Exception);
                        break;
                    case LogEvent.LogLevel.WARNING:
                        _Logger.Warn(logEvent.Message, logEvent.Exception);
                        break;
                    case LogEvent.LogLevel.ERROR:
                        _Logger.Error(logEvent.Message, logEvent.Exception);
                        break;
                    default:
                        _Logger.Debug(logEvent.Message, logEvent.Exception);
                        break;
                }
            }
        }

        #endregion

        #region Logging Methods

        public void Debug(string sMessage, Exception ex = null, [CallerMemberName]string callerMemberName = null)
        {
            HandleEvent(sMessage != null ? sMessage : string.Empty, KLogger.LogEvent.LogLevel.DEBUG, null, ex, callerMemberName);
        }

        public void DebugFormat(string format, params object[] args)
        {
            HandleEvent(format, KLogger.LogEvent.LogLevel.DEBUG, args, null);
        }

        public void Info(string sMessage, Exception ex = null, [CallerMemberName]string callerMemberName = null)
        {
            HandleEvent(sMessage != null ? sMessage : string.Empty, KLogger.LogEvent.LogLevel.INFO, null, ex, callerMemberName);
        }

        public void InfoFormat(string format, params object[] args)
        {
            HandleEvent(format, KLogger.LogEvent.LogLevel.INFO, args, null);
        }

        public void Warn(string sMessage, Exception ex = null, [CallerMemberName]string callerMemberName = null)
        {
            HandleEvent(sMessage != null ? sMessage : string.Empty, KLogger.LogEvent.LogLevel.WARNING, null, ex, callerMemberName);
        }

        public void WarnFormat(string format, params object[] args)
        {
            HandleEvent(format, KLogger.LogEvent.LogLevel.WARNING, args, null);
        }

        public void Error(string sMessage, Exception ex = null, [CallerMemberName]string callerMemberName = null)
        {
            HandleEvent(sMessage != null ? sMessage : string.Empty, KLogger.LogEvent.LogLevel.ERROR, null, ex, callerMemberName);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            HandleEvent(format, KLogger.LogEvent.LogLevel.ERROR, args, null);
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

        #endregion

        #region Microsoft ILogger Implementation

        void Microsoft.Extensions.Logging.ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            SetLogContextData();

            var msg = formatter(state, exception);
            switch (logLevel)
            {
                case LogLevel.Debug:
                    Debug(msg, exception);
                    break;
                case LogLevel.Information:
                    Info(msg, exception);
                    break;
                case LogLevel.Warning:
                    Warn(msg, exception);
                    break;
                case LogLevel.Critical:
                case LogLevel.Error:
                    Error(msg, exception);
                    break;
                default:
                    Debug(msg, exception);
                    break;

            }
        }

        private void SetLogContextData()
        {
            LogContextData[Constants.CLASS_NAME] = this.ClassName;
            LogContextData[Constants.SERVER] = this.Server;


            if (HttpContext.Current?.Items[Constants.CLIENT_TAG] != null)
            {
                LogContextData[Constants.CLIENT_TAG] = HttpContext.Current?.Items[Constants.CLIENT_TAG];
            }

            if (HttpContext.Current?.Items[Constants.HOST_IP] != null)
            {
                LogContextData[Constants.HOST_IP] = HttpContext.Current?.Items[Constants.HOST_IP];
            }

            if ( HttpContext.Current?.Items[Constants.REQUEST_ID_KEY] != null)
            {
                LogContextData[Constants.REQUEST_ID_KEY] = HttpContext.Current?.Items[Constants.REQUEST_ID_KEY];
            }

            if (HttpContext.Current?.Items[Constants.GROUP_ID] != null)
            {
                LogContextData[Constants.GROUP_ID] = HttpContext.Current?.Items[Constants.GROUP_ID];
            }

            if (HttpContext.Current?.Items[Constants.ACTION] != null)
            {
                LogContextData[Constants.ACTION] = HttpContext.Current?.Items[Constants.ACTION];
            }

            if (HttpContext.Current?.Items[Constants.USER_ID] != null)
            {
                LogContextData[Constants.USER_ID] = HttpContext.Current?.Items[Constants.USER_ID];
            }

            if (HttpContext.Current?.Items[Constants.TOPIC] != null)
            {
                LogContextData[Constants.TOPIC] = HttpContext.Current?.Items[Constants.TOPIC];
            }
        }

        bool Microsoft.Extensions.Logging.ILogger.IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        IDisposable Microsoft.Extensions.Logging.ILogger.BeginScope<TState>(TState state)
        {
            return null;
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

