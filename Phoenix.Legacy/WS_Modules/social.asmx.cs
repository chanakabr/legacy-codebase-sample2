using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Configuration;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using ApiObjects;
using KLogMonitor;
using System.Reflection;
using ApiObjects.Response;
using ApiObjects.Social;
using Core.Social.Requests;
using Core.Social.Responses;
using Core.Social;
using WebAPI.WebServices;

namespace WS_Social
{
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://social.tvinci.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class module : SocialService
    {

    }
}

    
