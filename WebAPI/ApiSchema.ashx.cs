using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Xml;
using WebAPI.ClientManagers.Client;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Models.General;

namespace WebAPI
{
    /// <summary>
    /// Prints out the API XML schema
    /// </summary>
    public class ApiSchema : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/xml";
            SchemaManager.Generate(context.Response.OutputStream);
        }
    }
}