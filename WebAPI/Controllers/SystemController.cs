using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Users;
using WebAPI.Utils;
using KLogMonitor;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/system/action")]
    public class SystemController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Returns country details by the provided IP, if not provided - by the client IP
        /// </summary>
        /// <param name="ip">IP</param>
        /// <remarks>
        /// Possible status codes:  Country was not found = 4025
        /// </remarks>
        [Route("getCountry"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.CountryNotFound)]
        public KalturaCountry GetCountry(string ip = null)
        {
            KalturaCountry response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrEmpty(ip))
            {
                ip = Utils.Utils.GetClientIP();
            }

            try
            {
                response = ClientsManager.CatalogClient().GetCountryByIp(groupId, ip);
                
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Returns true
        /// </summary>
        [Route("ping"), HttpPost]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public bool Ping()
        {
            log.Error("in rest method");

            return true;
        }

        /// <summary>
        /// Returns current server timestamp
        /// </summary>
        [Route("getTime"), HttpPost]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public long GetTime()
        {
            DateTime serverTime = (DateTime)HttpContext.Current.Items[RequestParser.REQUEST_TIME];
            return Utils.Utils.DateTimeToUnixTimestamp(serverTime, false);
        }

        /// <summary>
        /// Returns current server version
        /// </summary>
        [Route("getVersion"), HttpPost]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public string GetVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersionInfo.FileVersion;
        }
    }
}