using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using KLogMonitor;
using WebAPI.App_Start;
using WebAPI.Exceptions;


namespace WebAPI
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            TCMClient.Settings.Instance.Init();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            AutoMapperConfig.RegisterMappings();

            // set monitor and log configuration files
            KMonitor.Configure("log4net.config");
            KLogger.Configure("log4net.config");
        }

        protected void Application_BeginRequest()
        {
            // get group ID
            NameValueCollection queryParams = Request.Url.ParseQueryString();
            if (queryParams["group_id"] != null)
                HttpContext.Current.Items.Add(Constants.GROUP_ID, queryParams["group_id"]);

            if (queryParams["user_id"] != null)
                HttpContext.Current.Items.Add(Constants.USER_ID, queryParams["user_id"]);

            if (HttpContext.Current.Request != null)
            {
                // get user agent
                if (HttpContext.Current.Request.UserAgent != null)
                    HttpContext.Current.Items.Add(Constants.CLIENT_TAG, HttpContext.Current.Request.UserAgent);

                // get host IP
                if (HttpContext.Current.Request.UserHostAddress != null)
                    HttpContext.Current.Items.Add(Constants.HOST_IP, HttpContext.Current.Request.UserHostAddress);

                // get action name
                if (HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath != null)
                    HttpContext.Current.Items.Add(Constants.ACTION, HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath);
            }
        }
    }
}
