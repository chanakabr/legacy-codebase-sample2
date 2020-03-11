using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.Configuration;
using System.Web;

namespace Logger
{
    public class Logger4Net : ILogger4Net
    {
        private readonly ILog _log;

        #region - creat log file per request - 3 different file per level (Debug,Info,Error)
        public Logger4Net(Type type)
        {
            // create 3 different log File to each level (with the same LoggerName)
            this._log = LogManager.GetLogger(type);
            string sLogPath = GetConfigurationValue("LOG4NET_PATH");           
            log4net.GlobalContext.Properties["DebugLogFilePath"] = string.Format(@"{0}", sLogPath);
            log4net.GlobalContext.Properties["InfoLogFilePath"] = string.Format(@"{0}", sLogPath);
            log4net.GlobalContext.Properties["ErrorLogFilePath"] = string.Format(@"{0}", sLogPath);
           
            // log4net Configuration     
            string logConfigPath = GetConfigurationValue("LOG4NET_CONFIG_PATH");
            if (!string.IsNullOrEmpty(logConfigPath))
            {
                // load configuration from a given file path
                log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(logConfigPath));
            }
            else
            {
                // load configuration from a web.config section
                log4net.Config.XmlConfigurator.Configure();
            }
        }

        private string GetConfigurationValue(string key)
        {
            return Utils.GetTcmConfigValue(key);
        }

        #endregion

        #region ILogger4Net Members

        public bool IsDebugEnabled()
        {
            return this._log.IsDebugEnabled;
        }
        public bool IsInfoEnabled()
        {
            return this._log.IsInfoEnabled;
        }

        public bool IsWarnEnabled()
        {
            return this._log.IsWarnEnabled;
        }

        public bool IsErrorEnabled()
        {
            return this._log.IsErrorEnabled;
        }

        public bool IsFatalEnabled()
        {
            return this._log.IsFatalEnabled;
        }

        public void Debug(object message)
        {
            this._log.Debug(message);
        }

        public void Info(object message)
        {
            this._log.Info(message);
        }

        public void Warn(object message)
        {
            this._log.Warn(message);
        }

        public void Error(object message)
        {
            this._log.Error(message);
        }

        public void Fatal(object message)
        {
            this._log.Fatal(message);
        }

        public void Debug(object message, Exception t)
        {
            this._log.Debug(message, t);
        }

        public void Info(object message, Exception t)
        {
            this._log.Info(message, t);
        }

        public void Warn(object message, Exception t)
        {
            this._log.Warn(message, t);
        }

        public void Error(object message, Exception t)
        {

            this._log.Error(message, t);
        }

        public void Fatal(object message, Exception t)
        {
            this._log.Fatal(message, t);
        }

        public void DebugFormat(string format, params object[] args)
        {
            this._log.DebugFormat(format, args);
        }

        public void InfoFormat(string format, params object[] args)
        {
            this._log.InfoFormat(format, args);
        }
        public void WarnFormat(string format, params object[] args)
        {
            this._log.WarnFormat(format, args);

        }

        public void ErrorFormat(string format, params object[] args)
        {
            this._log.ErrorFormat(format, args);
        }

        public void FatalFormat(string format, params object[] args)
        {
            this._log.FatalFormat(format, args);
        }

        public void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            this._log.DebugFormat(provider, format, args);
        }
        public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            this._log.InfoFormat(provider, format, args);
        }
        public void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
            this._log.WarnFormat(provider, format, args);
        }

        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            this._log.ErrorFormat(provider, format, args);
        }
        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
            this._log.FatalFormat(provider, format, args);
        }

        #endregion
    }
}

