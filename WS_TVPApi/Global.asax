<%@ Application Language="C#" %>

<script runat="server">

    void Application_Start(object sender, EventArgs e) 
    {
        // setting log file name for cloud
        //string EnvironmentClient = "Test";//"System.Configuration.ConfigurationManager.AppSettings["ClientIdentifier"].ToLower();

        //if (!string.IsNullOrEmpty(TVPPro.Configuration.Technical.TechnicalConfiguration.Instance.Data.Site.LogBasePath))
        {
            log4net.GlobalContext.Properties["DebuggingLogFilePath"] = string.Format(@"c:\log\TVPApi\Debugging_{0}.log", System.Environment.MachineName);
            log4net.GlobalContext.Properties["InformationLogFilePath"] = string.Format(@"c:\log\TVPApi\Information_{0}.log", System.Environment.MachineName);
            log4net.GlobalContext.Properties["ExceptionsLogFilePath"] = string.Format(@"c:\log\TVPApi\Exceptions_{0}.log", System.Environment.MachineName);
            log4net.GlobalContext.Properties["PerformancesLogFilePath"] = string.Format(@"c:\log\TVPApi\Performances_{0}.log", System.Environment.MachineName);

            string logConfigPath = ConfigurationManager.AppSettings["Log4NetConfiguration"];
            if (!string.IsNullOrEmpty(logConfigPath))
            {
                logConfigPath = Server.MapPath(logConfigPath);
                log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(logConfigPath));
            }
        }
    }
    
    void Application_End(object sender, EventArgs e) 
    {
        //  Code that runs on application shutdown

    }
        
    void Application_Error(object sender, EventArgs e) 
    {
        // Code that runs when an unhandled error occurs
        Exception objErr = Server.GetLastError().GetBaseException();
        
        Logger.Logger.Log("Exception in TVPApi", "Request is : " + Request.RawUrl + " Exception is :" + objErr.Message.ToString() + "StackTrace : " + objErr.StackTrace.ToString(), "TVPApiExceptions");

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
