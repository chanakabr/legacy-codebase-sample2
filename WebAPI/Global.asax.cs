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
using WebAPI.Filters;

namespace WebAPI
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            TCMClient.Settings.Instance.Init();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            AutoMapperConfig.RegisterMappings();
            EventNotificationsConfig.SubscribeConsumers();

            // set monitor and log configuration files
            KMonitor.Configure("log4net.config", KLogEnums.AppType.WS);
            KLogger.Configure("log4net.config", KLogEnums.AppType.WS);
        }

        protected void Application_BeginRequest()
        {
            // get host IP
            if (HttpContext.Current.Request.UserHostAddress != null)
                HttpContext.Current.Items[Constants.HOST_IP] = HttpContext.Current.Request.UserHostAddress;
        }
    }
}
