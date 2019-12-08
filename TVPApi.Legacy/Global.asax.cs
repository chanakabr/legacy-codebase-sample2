using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace WAP_TVPApi
{
    public class Global : System.Web.HttpApplication
    {
        private static readonly KLogMonitor.KLogger logger = new KLogMonitor.KLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public static log4net.ILog performanceLogger = log4net.LogManager.GetLogger("GlobalASCX");

        protected void Application_Start(object sender, EventArgs e)
        {
            bool verifyHttps = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings.Get("verifyHttps"));
            if (!verifyHttps)
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            }

            Tvinci.Data.Loaders.CatalogRequestManager.SignatureKey = System.Configuration.ConfigurationManager.AppSettings["CatalogServiceSignatureKey"];

            string logDir = System.Environment.GetEnvironmentVariable("API_LOG_DIR");
            if (logDir != null)
            {
                logDir = System.Environment.ExpandEnvironmentVariables(logDir);
            }
            else
            {
                logDir = "C:\\log\\tvpapi";
            }
            log4net.GlobalContext.Properties["LogDir"] = logDir;

            // set monitor and log configuration files
            KLogMonitor.KMonitor.Configure("log4net.config", KLogMonitor.KLogEnums.AppType.WS);
            KLogMonitor.KLogger.Configure("log4net.config", KLogMonitor.KLogEnums.AppType.WS);

            ConfigurationManager.ApplicationConfiguration.Initialize(true, true);
            // This line is here to avoid error while deserilizing json that was serlizied using net core with TypeNameHandling
            TVinciShared.AssemblyUtils.RedirectAssembly("System.Private.CoreLib", "mscorlib");
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            //Patch for .net v4.0+
            if (HttpContext.Current.Items.Count > 0)
                HttpContext.Current.Items.Clear();

            // get start time for the logs
            HttpContext.Current.Items.Add("RequestStartTime", DateTime.UtcNow);

            // Save site data (groupid, platform, wsuser, wspass) on session for further processes
            TVPApi.ConnectionHelper.InitServiceConfigs();

            if (!Request.ContentType.Contains("xml"))
            {
                HttpApplication app = HttpContext.Current.ApplicationInstance;
                System.IO.Stream prevUncompressedStream = app.Response.Filter;
                string acceptEncoding = app.Request.Headers["Accept-Encoding"];
                if (acceptEncoding == null || acceptEncoding.Length == 0)
                    return;

                acceptEncoding = acceptEncoding.ToLower();

                if (acceptEncoding.Contains("gzip"))
                {
                    // gzip
                    app.Response.Filter = new System.IO.Compression.GZipStream(prevUncompressedStream,
                        System.IO.Compression.CompressionMode.Compress);
                    app.Response.AppendHeader("Content-Encoding", "gzip");
                }
                else if (acceptEncoding.Contains("deflate") || acceptEncoding == "*")
                {
                    // deflate
                    app.Response.Filter = new System.IO.Compression.DeflateStream(prevUncompressedStream,
                        System.IO.Compression.CompressionMode.Compress);
                    app.Response.AppendHeader("Content-Encoding", "deflate");
                }
            }
            Guid sessionId;

            var headerRequest = HttpContext.Current.Request.Headers.Get(KLogMonitor.Constants.REQUEST_ID_KEY);
            if (!string.IsNullOrEmpty(headerRequest))
            {
                sessionId = new Guid(headerRequest);
            }
            else
            {
                sessionId = Guid.NewGuid();
            }

            //HttpContext.Current.Items[KLogMonitor.Constants.REQUEST_ID_KEY] = Session.ToString();

            HttpContext.Current.Items[KLogMonitor.Constants.REQUEST_ID_KEY] = sessionId.ToString();
            KLogger.SetRequestId(sessionId.ToString());

            HttpContext.Current.Response.Headers["X-Kaltura-Session"] = sessionId.ToString();

            // get action name
            if (HttpContext.Current.Request.QueryString["m"] != null)
            {
                var m = HttpContext.Current.Request.QueryString["m"];
                HttpContext.Current.Items[KLogMonitor.Constants.ACTION] = m;
                KLogger.SetAction(m);
            }

            // get user agent
            if (HttpContext.Current.Request.UserAgent != null)
            {
                HttpContext.Current.Items[KLogMonitor.Constants.CLIENT_TAG] = HttpContext.Current.Request.UserAgent;
                KLogger.LogContextData[KLogMonitor.Constants.CLIENT_TAG] = HttpContext.Current.Request.UserAgent;
            }

            // get host IP
            if (HttpContext.Current.Request.UserHostAddress != null)
            {
                HttpContext.Current.Items[KLogMonitor.Constants.HOST_IP] = HttpContext.Current.Request.UserHostAddress;
                KLogger.LogContextData[KLogMonitor.Constants.HOST_IP] = HttpContext.Current.Request.UserHostAddress;
            }
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Response.Clear();

            // Code that runs when an unhandled error occurs
            Exception ex = new Exception("Unknown Exception");
            if (Server.GetLastError() != null
                && Server.GetLastError().GetBaseException() != null)
            {
                ex = Server.GetLastError().GetBaseException();
            }
            else if (Server.GetLastError() != null)
                ex = Server.GetLastError();

            logger.Error(string.Concat("Request: ", Request.RawUrl), ex);
            Server.ClearError();
            Response.Write("Error");
        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }


        void Application_EndRequest(Object Sender, EventArgs e)
        {
            // Get response time in milliseconds
            int timeTaken = (DateTime.UtcNow - (DateTime)HttpContext.Current.Items["RequestStartTime"]).Milliseconds;

            // Get the request URL
            string requestUrl = Request.Url.AbsoluteUri;

            // Get the request message body 
            System.IO.StreamReader reader = new System.IO.StreamReader(Request.InputStream);
            reader.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
            string requestBody = reader.ReadToEnd();

            // Get the client IP
            string clienIP = TVPPro.SiteManager.Helper.SiteHelper.GetClientIP();

            // Check if exception or error occurred
            object error = HttpContext.Current.Items["Error"];
            string sError = null;

            if (error != null)
            {
                // Exception was thrown - write log
                if (error is Exception)
                {
                    sError = "Unknown error ";

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
                    string json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(new { Error = sError });
                    Response.ClearContent();
                    Response.Write(json);
                    HttpContext.Current.Response.HeaderEncoding = HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;
                    HttpContext.Current.Response.Charset = "utf-8";

                }
            }

            // Write log
            if (!string.IsNullOrEmpty(sError))
            {
                performanceLogger.ErrorFormat("Application_EndRequest: URL = {0}, ClientIP = {1}, RequestBody = {2}, TimeTaken = {3} (Milliseconds), Error = {4} ", requestUrl, clienIP, requestBody, timeTaken, error is Exception ? (error as Exception).Message : sError);
            }
            else
            {
                performanceLogger.DebugFormat("Application_EndRequest: URL = {0}, ClientIP = {1}, RequestBody = {2}, TimeTaken = {3} (Milliseconds)", requestUrl, clienIP, requestBody, timeTaken);
            }

            // Check for Status code
            if (HttpContext.Current.Items.Contains("StatusCode"))
            {
                Response.ClearContent();
                Response.StatusCode = (int)HttpContext.Current.Items["StatusCode"];
                if (HttpContext.Current.Items.Contains("StatusDescription"))
                {
                    Response.StatusDescription = HttpContext.Current.Items["StatusDescription"].ToString();
                }
                Response.TrySkipIisCustomErrors = true;
            }

            // Append to IIS log the full Url
            Response.AppendToLog(string.Format("|{0}", requestUrl));
        }

    }
}