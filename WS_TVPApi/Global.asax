<%@ Application Language="C#" %>
<%@ Import Namespace="TVPApiModule.Helper" %>
<%@ Import Namespace="log4net" %>

<script runat="server">
    public static ILog logger = log4net.LogManager.GetLogger("GlobalASCX");
    
    void Application_Start(object sender, EventArgs e) 
    {
        //System.Net.ServicePointManager.DefaultConnectionLimit = 6;
        
        // setting log file name for cloud
        //string EnvironmentClient = "Test";//"System.Configuration.ConfigurationManager.AppSettings["ClientIdentifier"].ToLower();

        //if (!string.IsNullOrEmpty(TVPPro.Configuration.Technical.TechnicalConfiguration.Instance.Data.Site.LogBasePath))
        {
            log4net.GlobalContext.Properties["DebuggingLogFilePath"] = string.Format(@"{0}\{1}\TVPApi\{2}\Debugging.log", ConfigurationManager.AppSettings["BASE_LOGS_PATH"], ConfigurationManager.AppSettings["DomainEnv"], System.Environment.MachineName);
            log4net.GlobalContext.Properties["InformationLogFilePath"] = string.Format(@"{0}\{1}\TVPApi\{2}\Information.log", ConfigurationManager.AppSettings["BASE_LOGS_PATH"], ConfigurationManager.AppSettings["DomainEnv"], System.Environment.MachineName);
            log4net.GlobalContext.Properties["ExceptionsLogFilePath"] = string.Format(@"{0}\{1}\TVPApi\{2}\Exceptions.log", ConfigurationManager.AppSettings["BASE_LOGS_PATH"], ConfigurationManager.AppSettings["DomainEnv"], System.Environment.MachineName);
            log4net.GlobalContext.Properties["PerformancesLogFilePath"] = string.Format(@"{0}\{1}\TVPApi\{2}\Performances.log", ConfigurationManager.AppSettings["BASE_LOGS_PATH"], ConfigurationManager.AppSettings["DomainEnv"], System.Environment.MachineName);

            string logConfigPath = ConfigurationManager.AppSettings["Log4NetConfiguration"];
            if (!string.IsNullOrEmpty(logConfigPath))
            {
                logConfigPath = Server.MapPath(logConfigPath);
                log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(logConfigPath));
            }
        }
        Tvinci.Data.Loaders.CatalogRequestManager.EndPointAddress = ConfigurationManager.AppSettings["CatalogServiceURL"];
        Tvinci.Data.Loaders.CatalogRequestManager.SignatureKey = ConfigurationManager.AppSettings["CatalogServiceSignatureKey"];        
    }

    protected void Application_BeginRequest(Object sender, EventArgs e)
    {
        // Save site data (groupid, platform, wsuser, wspass) on session for further proccesses
        TVPApi.ConnectionHelper.InitServiceConfigs();
    }
    
    void Application_End(object sender, EventArgs e) 
    {
        //  Code that runs on application shutdown

    }
        
    void Application_Error(object sender, EventArgs e) 
    {
        // Code that runs when an unhandled error occurs
        Exception ex = new Exception("Unknown Exception");
        if (Server.GetLastError() != null && Server.GetLastError().GetBaseException() != null) ex = Server.GetLastError().GetBaseException();
        else if (Server.GetLastError() != null) ex = Server.GetLastError();

        logger.Error(string.Concat("Request: ", Request.RawUrl), ex);
        
        Server.ClearError();
    }

    void Session_Start(object sender, EventArgs e) 
    {
        // Code that runs when a new session is started
    }

    void Session_End(object sender, EventArgs e) 
    {
        // Code that runs when a session ends. 
        // Note: The Session_End event is raised only when the sessionstate mode
        // is set to InProc in the Web.config file. If session mode is set to StateServer 
        // or SQLServer, the event is not raised.

    }
       
</script>
