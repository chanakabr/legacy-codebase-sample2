using System;
using System.Diagnostics;
using System.Reflection;
using System.Web;
using ApiObjects.Response;
using KalturaRequestContext;
using Phx.Lib.Log;
using TVinciShared;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Users;
using WebAPI.ObjectsConvertor.Mapping;
using WebAPI.Utils;

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
        public static KalturaCountry GetCountry(string ip = null)
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
        public static bool Ping()
        {
            return true;
        }

        /// <summary>
        /// Returns current server timestamp
        /// </summary>
        [Action("getTime")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public static long GetTime()
        {
            DateTime serverTime = (DateTime)HttpContext.Current.Items[RequestContextConstants.REQUEST_TIME];
            return DateUtils.DateTimeToUtcUnixTimestampSeconds(serverTime);
        }

        /// <summary>
        /// Returns current server version
        /// </summary>
        [Action("getVersion")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public static string GetVersion()
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
        public static string GetLogLevel()
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
        public static bool SetLogLevel(KalturaLogLevel level)
        {
            try
            {
                var loggerLevel = GeneralMappings.ConvertLogLevel(level);
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
        [Action(name: "clearLocalServerCache")]
        public static bool ClearLocalServerCache(string clearCacheAction = null, string key = null)
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
        [Action(name: "incrementLayeredCacheGroupConfigVersion")]
        public static bool IncrementLayeredCacheGroupConfigVersion(int groupId = 0)
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

        /// <summary>
        /// Returns the current layered cache group config of the sent groupId. You need to send groupId only if you wish to get it for a specific groupId and not the one the KS belongs to.
        /// </summary>
        /// <param name="groupId">groupId</param>
        /// <returns></returns>
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Action(name: "getLayeredCacheGroupConfig")]
        public static KalturaStringValue GetLayeredCacheGroupConfig(int groupId = 0)
        {
            KalturaStringValue version = new KalturaStringValue();
            try
            {
                int groupIdToGet = KS.GetFromRequest().GroupId;
                if (groupId > 0)
                {
                    groupIdToGet = groupId;
                }

                version = ClientsManager.ApiClient().GetLayeredCacheGroupConfig(groupIdToGet);
            }
            catch (ClientException ex)
            {
                log.Error($"Error GetLayeredCacheGroupConfig. ex = {ex}");
                ErrorUtils.HandleClientException(ex);
            }

            return version;
        }

        /// <summary>
        /// Returns true if the invalidation key was invalidated successfully or false otherwise.
        /// </summary>
        /// <param name="key">the invalidation key to invalidate</param>
        /// <returns></returns>
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Action(name: "invalidateLayeredCacheInvalidationKey")]
        public static bool InvalidateLayeredCacheInvalidationKey(string key)
        {
            try
            {
                return ClientsManager.ApiClient().InvalidateLayeredCacheInvalidationKey(key);
            }
            catch (Exception ex)
            {
                log.Error($"Error InvalidateLayeredCacheInvalidationKey. ex = {ex}");
                return false;
            }
        }

        /// <summary>
        /// Returns the epoch value of an invalidation key if it was found
        /// </summary>
        /// <param name="layeredCacheConfigName">the layered cache config name of the invalidation key</param>
        /// <param name="invalidationKey">the invalidation key to fetch it's value</param>
        /// <param name="groupId">groupId</param>
        /// <returns></returns>
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Action(name: "getInvalidationKeyValue")]
        public static KalturaLongValue GetInvalidationKeyValue(string invalidationKey, string layeredCacheConfigName = null, int groupId = 0)
        {
            KalturaLongValue result = new KalturaLongValue();
            try
            {
                int groupIdToUse = KS.GetFromRequest().GroupId;
                if (groupId > 0)
                {
                    groupIdToUse = groupId;
                }

                result = ClientsManager.ApiClient().GetInvalidationKeyValue(groupIdToUse, layeredCacheConfigName, invalidationKey);
            }
            catch (Exception ex)
            {
                log.Error($"Error GetInvalidationKeyValue. ex = {ex}");
            }

            return result;
        }

    }
}