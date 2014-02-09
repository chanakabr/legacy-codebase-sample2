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
        private static readonly ILog logger = LogManager.GetLogger("Site.Guy");

		static Logger()
		{
            string s = "";
            if (ConfigurationManager.AppSettings["LOGS_PATH"] != null &&
                ConfigurationManager.AppSettings["LOGS_PATH"].ToString() != "")
                m_sLogsPath = ConfigurationManager.AppSettings["LOGS_PATH"].ToString() + "\\Logs\\";
            else if (HttpContext.Current != null)
                m_sLogsPath = HttpContext.Current.Server.MapPath(s) + "\\Logs\\";
            else
                m_sLogsPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Logs/";
            //m_sLogsPath += "//Logs/";
		}
        static public void SetPath(string sPath)
        {
            m_sLogsPath = sPath;
        }
        //private static void WriteToFile(string logMessage, string sLogFile)
        //{
        //    lock (m_sLogsPath)
        //    {
        //        string[] to_plist_with = {"."};
        //        string[] splited = sLogFile.Split(to_plist_with , StringSplitOptions.None);
        //        if (splited.Length > 1)
        //            sLogFile = splited[0] + DateTime.Now.Day.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Year.ToString() + "." + splited[1];
        //        else
        //            sLogFile = splited[0] + DateTime.Now.Day.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Year.ToString() + ".log";
        //        string sLogFileName = m_sLogsPath + sLogFile;
        //        StreamWriter w = File.AppendText(sLogFileName);
        //        w.Write("\r\n");
        //        //w.Write("{0} ", DateTime.Now.ToUniversalTime());
        //        //w.Write(", THREAD-");
        //        //w.Write(AppDomain.GetCurrentThreadId());
        //        //w.Write(", ");
        //        //w.Write(logHeader);
        //        w.Write("{0}", logMessage);
        //        w.Flush();
        //        w.Close();
        //    }
        //}

		public static void Log (string logHeader , string logMessage , string sLogFile)
		{
            logger.WarnFormat("{0} - {1}", logHeader, logMessage);

			lock(m_sLogsPath)
			{
				string sLogFileName = m_sLogsPath + sLogFile + DateTime.Now.Day.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Year.ToString() + ".log";

                if (Directory.Exists(Path.GetDirectoryName(sLogFileName)))
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
			}



            
		}

		public static string m_sLogsPath = "";
        public static string m_sBaseLogsPath = "";
	}
}
