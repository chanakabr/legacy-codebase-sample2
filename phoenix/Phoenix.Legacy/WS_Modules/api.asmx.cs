using APILogic;
using ApiObjects;
using ApiObjects.AssetLifeCycleRules;
using ApiObjects.BulkExport;
using ApiObjects.Response;
using ApiObjects.Roles;
using ApiObjects.Rules;
using ApiObjects.TimeShiftedTv;
using Core.Catalog.Response;
using Phx.Lib.Log;
using ScheduledTasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Services;
using System.Xml;
using System.Xml.Serialization;
using WebAPI.WebServices;

namespace WS_API
{
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://api.tvinci.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    //[System.Web.Script.Services.ScriptService]
    public class API : WebAPI.WebServices.ApiService
    {
    }
}
