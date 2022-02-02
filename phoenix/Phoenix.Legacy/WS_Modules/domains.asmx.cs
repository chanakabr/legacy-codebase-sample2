using ApiObjects;
using ApiObjects.MediaMarks;
using ApiObjects.Response;
using Core.Users;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Services;
using WebAPI.WebServices;

namespace WS_Domains
{
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://domains.tvinci.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class module : DomainsService
    {
        
    }
}