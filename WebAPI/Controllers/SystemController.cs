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
    [Service("system")]
    public class SystemController : IKalturaController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Returns country details by the provided IP, if not provided - by the client IP
        /// </summary>
        /// <param name="ip">IP</param>
        /// <remarks>
        /// Possible status codes:  Country was not found = 4025
        /// </remarks>
        [Action("getCountry")]
        [Obsolete]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.CountryNotFound)]
        static public KalturaCountry GetCountry(string ip = null)
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
        [Action("ping")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        static public bool Ping()
        {
            log.Error("in rest method");

            return true;
        }

        /// <summary>
        /// Returns current server timestamp
        /// </summary>
        [Action("getTime")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        static public long GetTime()
        {
            DateTime serverTime = (DateTime)HttpContext.Current.Items[RequestParser.REQUEST_TIME];
            return Utils.Utils.DateTimeToUnixTimestamp(serverTime, false);
        }

        /// <summary>
        /// Returns current server version
        /// </summary>
        [Action("getVersion")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        static public string GetVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersionInfo.FileVersion;
        }
    }
}