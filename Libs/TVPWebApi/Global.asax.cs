using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Newtonsoft.Json.Serialization;
using TVPWebApi.Models;

namespace TVPWebApi
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            IOCBootstrapper.Initialise();

            GlobalConfiguration.Configuration.MessageHandlers.Add(new InitObjHandler());

            Tvinci.Data.Loaders.CatalogRequestManager.EndPointAddress = ConfigurationManager.AppSettings["CatalogServiceURL"];
            Tvinci.Data.Loaders.CatalogRequestManager.SignatureKey = ConfigurationManager.AppSettings["CatalogServiceSignatureKey"];

        }
    }
}