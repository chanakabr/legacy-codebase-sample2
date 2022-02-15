using ApiObjects;
using ApiObjects.Billing;
using Core.Billing;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Web.Services;
using WebAPI.WebServices;

namespace WS_Billing
{
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://billing.tvinci.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class module : BillingService
    {

    }
}
