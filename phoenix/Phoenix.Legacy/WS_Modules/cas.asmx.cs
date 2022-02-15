using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Net;
using System.Text;
using System.IO;
using System.Web.Script.Serialization;
using System.Diagnostics;
using ApiObjects.Response;
using Phx.Lib.Log;
using ApiObjects.Billing;
using System.Reflection;
using ApiObjects;
using Core.ConditionalAccess;
using Core.ConditionalAccess.Response;
using ApiObjects.ConditionalAccess;
using ApiObjects.TimeShiftedTv;
using ApiObjects.Pricing;
using WebAPI.WebServices;

namespace WS_ConditionalAccess
{
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://ca.tvinci.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class module : ConditionalAccessService
    {
    }
}
