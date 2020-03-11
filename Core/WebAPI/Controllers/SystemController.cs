using ApiObjects.Response;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Web;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Users;
using WebAPI.Utils;
using KLogMonitor;
using TVinciShared;
using WebAPI.Models.General;

namespace WebAPI.Controllers
{
    /// <summary>
    /// 
    /// </summary>
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
            return true;
        }

        /// <summary>
        /// Returns current server timestamp
        /// </summary>
        [Action("getTime")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        static public long GetTime()
        {
            DateTime serverTime = (DateTime)HttpContext.Current.Items[RequestContextUtils.REQUEST_TIME];
            return DateUtils.DateTimeToUtcUnixTimestampSeconds(serverTime);
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

        /// <summary>
        /// Gets the current level of the KLogger
        /// </summary>
        /// <returns></returns>
        [Action(name:"getLogLevel", isInternal:true)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ApiAuthorize]
        static public string GetLogLevel()
        {
            return KLogger.GetLogLevel().ToString();
        }

        /// <summary>
        /// Sets the current level of the KLogger
        /// </summary>
        /// <param name="level">Possible levels: trace, debug, info, warning, error, all</param>
        /// <returns></returns>
        [Action(name: "setLogLevel", isInternal: true)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ApiAuthorize]
        static public bool SetLogLevel(KalturaLogLevel level)
        {
            try
            {
                var loggerLevel = WebAPI.ObjectsConvertor.Mapping.GeneralMappings.ConvertLogLevel(level);
                KLogger.SetLogLevel(loggerLevel);
                return true;
            }
            catch (Exception ex)
            {
                log.Error($"Error setting log level. ex = {ex}");
                return false;
            }
        }

        /// <summary>
        /// Clear local server cache
        /// </summary>
        /// <param name="clearCacheAction">clear cache action to perform, possible values: clear_all / keys / getKey</param>
        /// <param name="key">key to get in case you send action getKey</param>
        /// <returns></returns>
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Action(name: "clearLocalServerCache", isInternal: true)]
        static public bool ClearLocalServerCache(string clearCacheAction = null, string key = null)
        {
            try
            {
                if (clearCacheAction == null)
                {
                    clearCacheAction = "clear_all";
                }

                return ClientsManager.ApiClient().ClearLocalServerCache(clearCacheAction, key);
            }
            catch (Exception ex)
            {
                log.Error($"Error ClearLocalServerCache. ex = {ex}");
                return false;
            }
        }

        /// <summary>
        /// Returns true if version has been incremented successfully or false otherwise. You need to send groupId only if you wish to increment for a specific groupId and not the one the KS belongs to.
        /// </summary>
        /// <param name="groupId">groupId</param>
        /// <returns></returns>
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Action(name: "incrementLayeredCacheGroupConfigVersion", isInternal: true)]
        static public bool IncrementLayeredCacheGroupConfigVersion(int groupId = 0)
        {
            try
            {                
                int groupIdToIncrement = KS.GetFromRequest().GroupId;
                if (groupId > 0)
                {
                    groupIdToIncrement = groupId;
                }

                return ClientsManager.ApiClient().IncrementLayeredCacheGroupConfigVersion(groupIdToIncrement);
            }
            catch (Exception ex)
            {
                log.Error($"Error IncrementLayeredCacheGroupConfigVersion. ex = {ex}");
                return false;
            }
        }
    }
}