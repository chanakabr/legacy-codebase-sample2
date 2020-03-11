using System;
using System.IO;
using System.Threading;
using Microsoft.Win32;
using System.Web;
using System.Web.SessionState;
using System.Configuration;
using log4net;

namespace Logger
{
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    public class Logger
    {
        //five diffrent logs - for each level
        //        private static ILog m_Logger = log4net.LogManager.GetLogger(typeof(Logger));

        //  private static ILog m_Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static Logger()
        {
            string s = "";
            if (Utils.GetTcmConfigValue("LOGS_PATH") != string.Empty)
                m_sLogsPath = Utils.GetTcmConfigValue("LOGS_PATH") + "/Logs/";
            else if (HttpContext.Current != null)
                m_sLogsPath = HttpContext.Current.Server.MapPath(s) + "//Logs/";
            else
                m_sLogsPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Logs/";

            try
            {
                m_sServerName = Environment.MachineName;
            }
            catch
            {
                if (Utils.GetTcmConfigValue("SERVER_NAME") != string.Empty)
                    m_sServerName = Utils.GetTcmConfigValue("SERVER_NAME");
                else
                    m_sServerName = "Unknown";
            }

            if (Utils.GetTcmConfigValue("APPLICATION_NAME") != string.Empty)
                m_sApplicationName = Utils.GetTcmConfigValue("APPLICATION_NAME");
            else
                m_sApplicationName = "Unknown";

            m_sLogsPath += m_sServerName + "/" + m_sApplicationName + "/";
            if (m_sLogsPath.StartsWith("\\\\") == true)
            {
                m_sLogsPath = "\\\\" + m_sLogsPath.Substring(2).Replace("/", "\\");
            }
            if (System.IO.Directory.Exists(m_sLogsPath) == false)
                System.IO.Directory.CreateDirectory(m_sLogsPath);

            // #region Log4Net Configuration - 05.11.2012 

            // string dFormat = "dd-MM-yyyy";
            // log4net.GlobalContext.Properties["DebugLogFilePath"]     = string.Format(@"{0}{1}_{2}.txt", m_sLogsPath, "_Debug", DateTime.Now.ToString(dFormat));
            // log4net.GlobalContext.Properties["InfoLogFilePath"]      = string.Format(@"{0}{1}_{2}.txt", m_sLogsPath, "_Info",  DateTime.Now.ToString(dFormat));
            // log4net.GlobalContext.Properties["WarnLogFilePath"]      = string.Format(@"{0}{1}_{2}.txt", m_sLogsPath, "_Warn",  DateTime.Now.ToString(dFormat));
            // log4net.GlobalContext.Properties["ErrorLogFilePath"]     = string.Format(@"{0}{1}_{2}.txt", m_sLogsPath, "_Error", DateTime.Now.ToString(dFormat));
            // log4net.GlobalContext.Properties["FatalLogFilePath"]     = string.Format(@"{0}{1}_{2}.txt", m_sLogsPath, "_Fatal", DateTime.Now.ToString(dFormat));

            // string logConfigPath = string.Format(@"J:\ODE\TVM\Libs\Core\Logger\Log4Net.config");
            // //log4net Config
            //// string logConfigPath = string.Format(@"..\Log4Net.config");
            // if (!string.IsNullOrEmpty(logConfigPath))
            // {
            //     log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(logConfigPath));
            // }

            // #endregion

        }
        static public void SetPath(string sPath)
        {
            m_sLogsPath = sPath;
        }
        public static void WriteToFile(string logMessage, string sLogFile)
        {
            lock (m_sLogsPath)
            {
                string[] to_plist_with = { "." };
                string[] splited = sLogFile.Split(to_plist_with, StringSplitOptions.None);
                if (splited.Length > 1)
                    sLogFile = splited[0] + DateTime.Now.Day.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Year.ToString() + "." + splited[1];
                else
                    sLogFile = splited[0] + DateTime.Now.Day.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Year.ToString() + ".log";
                string sLogFileName = m_sLogsPath + sLogFile;
                StreamWriter w = File.AppendText(sLogFileName);
                w.Write("\r\n");
                //w.Write("{0} ", DateTime.Now.ToUniversalTime());
                //w.Write(", THREAD-");
                //w.Write(AppDomain.GetCurrentThreadId());
                //w.Write(", ");
                //w.Write(logHeader);
                w.Write("{0}", logMessage);
                w.Flush();
                w.Close();
            }
        }

        public static void Log(string logHeader, string logMessage, string sLogFile)
        {
            Log(logHeader, logMessage, sLogFile, "");
        }

        //public static void MyLog(string progName)
        //{
        //    //string temp = log4net.GlobalContext.Properties["DebuggingLogFilePath"].ToString();
        ////    log4net.GlobalContext.Properties["DebuggingLogFilePath"] = log4net.GlobalContext.Properties["DebuggingLogFilePath"].ToString().Replace(Path.GetDirectoryName(log4net.GlobalContext.Properties["DebuggingLogFilePath"].ToString()), progName);


        //    //log4net.GlobalContext.Properties["DebuggingLogFilePath"] =  log4net.GlobalContext.Properties["DebuggingLogFilePath"].ToString().Replace("LogFile", progName);


        //    string temp = log4net.GlobalContext.Properties["DebuggingLogFilePath"].ToString();
        //    temp = temp.Substring(0, temp.LastIndexOf('/') + 1) + progName + temp.Substring(temp.IndexOf('_'));
        //    log4net.GlobalContext.Properties["DebuggingLogFilePath"] = temp;

        //  //  HandleDebug("test 123", new Exception());

        //}

        /*SendSMS : get 1 parameter => send the SMS to the numbers that storage under SMS_WS_PHONES key*/
        public static void SendSMS(string logMessage)
        {
            SMSCenterSender.SendSMS("Tvinci Logger", m_sServerName + ":" + m_sApplicationName + ":" + logMessage);
        }
        /*SendSMS : get 2 parameter => send the SMS to the numbers that storage under the "AppSmsKey" key */
        public static void SendSMS(string logMessage, string AppSmsKey)
        {
            SMSCenterSender.SendSMS("Tvinci Logger", m_sServerName + ":" + m_sApplicationName + ":" + logMessage, AppSmsKey);
        }

        public static void Log(string logHeader, string logMessage, string sLogFile, string sSMSStr)
        {
            Log(logHeader, logMessage, sLogFile, sSMSStr, "");
        }

        public static void Log(string logHeader, string logMessage, string sLogFile, string sSMSStr, string AppSmsKey)
        {
            try
            {
                string sLogFileName = m_sLogsPath + sLogFile + DateTime.Now.Day.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Year.ToString() + ".log";
                if (!Directory.Exists(Path.GetDirectoryName(sLogFileName)))
                {
                    return;
                }
                lock (m_sLogsPath)
                {
                    StreamWriter w = File.AppendText(sLogFileName);
                    w.Write("\r\n");
                    w.Write("{0} ", DateTime.Now.ToUniversalTime());
                    w.Write(", THREAD-");
                    w.Write(AppDomain.GetCurrentThreadId());
                    w.Write(", ");
                    w.Write(logHeader);
                    w.Write(" : {0}", logMessage);
                    w.Flush();
                    w.Close();
                }


                if (sSMSStr != "")
                {
                    if (String.IsNullOrEmpty(AppSmsKey))
                        SendSMS(logHeader + ": " + sSMSStr);
                    else
                        SendSMS(logHeader + ": " + sSMSStr, AppSmsKey);
                }



            }
            catch { }
        }

        #region log4Net
        /*Get or creat lof file by name*/
        //public static void GetOrCreateLogFile(string sLogFileName)
        //{

        //    string temp = log4net.GlobalContext.Properties["DebugLogFilePath"].ToString();
        //    temp = temp.Substring(0, temp.LastIndexOf('/') + 1) + sLogFileName + temp.Substring(temp.IndexOf('_'));
        //    log4net.GlobalContext.Properties["DebugLogFilePath"] = temp;

        //    //temp = log4net.GlobalContext.Properties["InfoLogFilePath"].ToString();
        //    //temp = temp.Substring(0, temp.LastIndexOf('/') + 1) + sLogFileName + temp.Substring(temp.IndexOf('_'));
        //    //log4net.GlobalContext.Properties["InfoLogFilePath"] = temp;

        //    //temp = log4net.GlobalContext.Properties["WarnLogFilePath"].ToString();
        //    //temp = temp.Substring(0, temp.LastIndexOf('/') + 1) + sLogFileName + temp.Substring(temp.IndexOf('_'));
        //    //log4net.GlobalContext.Properties["WarnLogFilePath"] = temp;

        //    //temp = log4net.GlobalContext.Properties["ErrorLogFilePath"].ToString();
        //    //temp = temp.Substring(0, temp.LastIndexOf('/') + 1) + sLogFileName + temp.Substring(temp.IndexOf('_'));
        //    //log4net.GlobalContext.Properties["ErrorLogFilePath"] = temp;

        //    //temp = log4net.GlobalContext.Properties["FatalLogFilePath"].ToString();
        //    //temp = temp.Substring(0, temp.LastIndexOf('/') + 1) + sLogFileName + temp.Substring(temp.IndexOf('_'));
        //    //log4net.GlobalContext.Properties["FatalLogFilePath"] = temp;

        //    //string logConfigPath = string.Format(@"J:\ODE\TVM\Libs\Core\Logger\Log4Net.config");
        //    //log4net Config
        //     string logConfigPath = string.Format(@"..\Log4Net.config");
        //    if (!string.IsNullOrEmpty(logConfigPath))
        //    {
        //        log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(logConfigPath));
        //    }
        //}

        //public void SetLogFile(string logFileName, string message, Exception ex)
        //{

        //   // log4net.ThreadContext.Properties["Version"] = "1";

        //    OpenLogFile("myNewLogFile");
        //    ILog logger = LogManager.GetLogger(typeof(Logger));



        //     string temp = log4net.GlobalContext.Properties["DebugLogFilePath"].ToString();
        //    temp = temp.Substring(0, temp.LastIndexOf('/') + 1) + logFileName + temp.Substring(temp.IndexOf('_'));
        //    log4net.GlobalContext.Properties["DebugLogFilePath"] = temp;


        //    //log4net.GlobalContext.Properties["LogName"] = logFileName;


        //    //log4net.Config.XmlConfigurator.Configure();
        //    string logConfigPath = string.Format(@"..\Log4Net.config");
        //    if (!string.IsNullOrEmpty(logConfigPath))
        //    {
        //        log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(logConfigPath));
        //    }

        //    logger.Debug("my own msg");

        //}
        //public void OpenLogFile(string fileName)
        //{
        //    log4net.Layout.ILayout layout = new log4net.Layout.PatternLayout("%d [%t]%-5p : - %m%n"); ;
        //    log4net.Appender.FileAppender appender = new log4net.Appender.FileAppender();
        //    appender.File = fileName;
        //    appender.Layout = layout;
        //    appender.Threshold = log4net.Core.Level.Info;
        //    log4net.Config.BasicConfigurator.Configure(appender);
        //}

        //public static void HandleDebug(string msg, Exception ex)
        //{
        //    if (ex != null)
        //        m_Logger.Debug(msg, ex);
        //    else
        //        m_Logger.Debug(msg);
        // }
        //public static void HandleError(string msg, Exception ex)
        //{
        //    if (m_Logger.IsErrorEnabled)
        //    {
        //        m_Logger.Error(msg, ex);
        //    }
        //}

        //public static void HandleInfo(string msg)
        //{
        //    if (m_Logger.IsInfoEnabled)
        //    {
        //        m_Logger.Info(msg);
        //    }

        //}
        //public static void HandleWarn(string msg)
        //{
        //    if (m_Logger.IsWarnEnabled)
        //    {
        //        m_Logger.Warn(msg);
        //    }

        //}
        //public static void HandleFatal(string msg, Exception ex)
        //{
        //    if (m_Logger.IsFatalEnabled)
        //    {
        //        if (ex != null)
        //            m_Logger.Fatal(msg, ex);
        //        else
        //            m_Logger.Fatal(msg);
        //    }
        //}
        #endregion
        public static string m_sLogsPath = "";
        public static string m_sServerName = "";
        public static string m_sApplicationName = "";
        public static string m_sBaseLogsPath = "";
    }
}
