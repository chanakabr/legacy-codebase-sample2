using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data;
using ApiObjects.Response;
using KLogMonitor;
using ApiObjects;
using Core.Pricing;
using ApiObjects.Pricing;
using WebAPI.WebServices;

namespace WS_Pricing
{
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://pricing.tvinci.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class module : PricingService
    {
    }
}
