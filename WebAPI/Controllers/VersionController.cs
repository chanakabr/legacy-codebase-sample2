using System;
using System.Reflection;
using WebAPI.Managers.Scheme;

#if NETCOREAPP3_0
using Microsoft.AspNetCore.Mvc;
#endif
#if NET48
using System.Web.Http.Description;
#endif

namespace WebAPI.Controllers
{
    [Service("version")]
    [Obsolete]
    public class VersionController : IKalturaController
    {
        /// <summary>
        /// Returns information about the current version
        /// </summary>
        [Action("")]
        [ApiExplorerSettings(IgnoreApi = true)]
        static public string Get()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}