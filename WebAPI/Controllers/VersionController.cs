using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/version/action")]
    public class VersionController : ApiController
    {
        /// <summary>
        /// Returns information about the current version
        /// </summary>
        /// </remarks>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaProjectVersion Get()
        {
            KalturaProjectVersion version = new KalturaProjectVersion();

            version.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            return version;
        }
    }
}