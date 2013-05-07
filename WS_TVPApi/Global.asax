<%@ Application Language="C#" %>
<%@ Import Namespace="TVPApiModule.Helper" %>
<%@ Import Namespace="log4net" %>
<%@ Import Namespace="System.Diagnostics" %>


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
        HttpContext.Current.Items.Add("RequestStartTime", DateTime.UtcNow);
    }
    
    void Application_End(object sender, EventArgs e) 
    {
        //  Code that runs on application shutdown

    }
        
    void Application_Error(object sender, EventArgs e) 
    {
        Response.Clear();
        
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

    void Application_EndRequest(Object Sender, EventArgs e)
    {
        // Get response time in milliseconds
        int timeTaken = (DateTime.UtcNow - (DateTime)HttpContext.Current.Items["RequestStartTime"]).Milliseconds;        
        
        // Get the request Url
        string sURL = Request.Url.AbsoluteUri;        
        
        // Get the request message body 
        System.IO.StreamReader reader = new System.IO.StreamReader(Request.InputStream);
        reader.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
        string requestBody = reader.ReadToEnd();
        
        // Get the client ip
        string clienIP = TVPPro.SiteManager.Helper.SiteHelper.GetClientIP();

        // Check if exception or error occurred
        object error = HttpContext.Current.Items["Error"];
        string sError = null;

        if (error != null)
        {
            //Response.ClearContent();
            // Exception was thrown - write log
            if (error is Exception)
            {
                sError = "Unknown error";
            }
            // Error occurred - write log
            else
            {
                sError = error as string;
            }
            // Return an error message to client
            if (Response.ContentType.Contains("xml"))
            {
                string xml = string.Format("<soap:Envelope xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'><soap:Body><Error error='{0}'/></soap:Body></soap:Envelope>", sError); ;
                Response.Clear();
                Response.Write(xml);
            }
            else
            {
                HttpContext.Current.Response.Clear();
                string json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(new { Error = sError });
                Response.Write(json);
            }
        }
        
        // Write log
        if (!string.IsNullOrEmpty(sError))
        {
            logger.ErrorFormat("Application_EndRequest: URL = {0}, ClientIP = {1}, RequestBody = {2}, TimeTaken = {3} (Milliseconds), Error = {4} ", sURL, clienIP, requestBody, timeTaken, sError);
        }
        else
        {
            logger.DebugFormat("Application_EndRequest: URL = {0}, ClientIP = {1}, RequestBody = {2}, TimeTaken = {3} (Milliseconds)", sURL, clienIP, requestBody, timeTaken);            
        }
        // Append to IIS log the full Url
        Response.AppendToLog(string.Format("|{0}", sURL)); 
    }
       
</script>
