using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Text;
using TVPApi;

/// <summary>
/// Summary description for LogManager
/// </summary>
public class LogManager
{
    private object m_lockObject = new object();
    private static LogManager m_instance = null;

    public static LogManager Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = new LogManager();
                
            }
            return m_instance;
        }
    }

    public void Log(int groupID, string logHeader, string logMessage)
    {
        //Get log path according to client
        bool isLog = false;
        if (Boolean.TryParse(ConfigurationManager.AppSettings["IsDebug"], out isLog) && isLog)
        {
            lock (m_lockObject)
            {
                try
                {
                    string logPathRel = ConfigurationManager.AppSettings[groupID.ToString() + "_Log"];
                    if (!string.IsNullOrEmpty(logPathRel))
                    {
                        string logPath = HttpContext.Current.Server.MapPath(logPathRel);
                        Logger.Logger.SetPath(logPath);
                        Logger.Logger.Log(logHeader, logMessage, "WSTVPApi");
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
    }

    public void Log(int groupID, string logHeader, string logMessage, InitializationObject initObj, string userName, string password)
    {
        lock (m_lockObject)
        {
            //Get log path according to client
            string logPathRel = ConfigurationManager.AppSettings[groupID.ToString() + "_Log"];
            if (!string.IsNullOrEmpty(logPathRel))
            {
                try
                {
                    string logPath = HttpContext.Current.Server.MapPath(logPathRel);
                    Logger.Logger.SetPath(logPath);
                    StringBuilder sb = new StringBuilder(logMessage);
                    string language = string.Empty;
                    string country = string.Empty;
                    string siteGuid = string.Empty;
                    string device = string.Empty;
                    if (initObj.Locale != null)
                    {
                        language = initObj.Locale.LocaleLanguage;
                        country = initObj.Locale.LocaleCountry;
                        siteGuid = initObj.Locale.SiteGuid;
                        device = initObj.Locale.LocaleDevice;
                    }

                    sb = sb.AppendFormat(" Username = {0},", userName);
                    sb = sb.AppendFormat(" Password = {0},", password);
                    sb = sb.Append(" with locale info:");
                    sb = sb.AppendFormat(" Language = {0},", language);
                    sb = sb.AppendFormat(" Country = {0},", country);
                    sb = sb.AppendFormat(" SiteGuid = {0},", siteGuid);
                    sb = sb.AppendFormat(" Device = {0}", device);
                    Logger.Logger.Log(logHeader, sb.ToString(), "WSTVPApi");
                }
                catch (Exception ex)
                {
                    // Logger.Logger.Log(logHeader, ex.Message, "WSTVPApi");
                }
            }
        }
    }

    private LogManager()
    {

    }
}
