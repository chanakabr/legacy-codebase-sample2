using ApiObjects.Catalog;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using CachingProvider.LayeredCache;
using Phx.Lib.Appconfig;
using Core.Catalog;
using Core.Catalog.Cache;
using Core.Catalog.Request;
using Core.Catalog.Response;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tvinci.Core.DAL;
using TVinciShared;

namespace APILogic.Api.Managers
{
    public class EpgManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static Dictionary<long, string> GetChannelIds(int groupId)
        {
            string allLinearMediaIdsKey = LayeredCacheKeys.GetAllLinearMediaKey(groupId);
            Dictionary<long, string> allLinearMedia = null;

            if (!LayeredCache.Instance.Get<Dictionary<long, string>>(allLinearMediaIdsKey,
                    ref allLinearMedia,
                    GetAllLinearMedia,
                    new Dictionary<string, object>() { { "groupId", groupId } },
                    groupId,
                    LayeredCacheConfigNames.GET_ALL_LINEAR_MEDIA))
            {
                log.ErrorFormat("GetEpgChannelId - GetAllLinearMedia - Failed get data from cache. groupId: {0}", groupId);
                return allLinearMedia;
            }

            return allLinearMedia;
        }
        
        public static string GetEpgChannelId(int mediaId, int groupId)
        {
            Dictionary<long, string> allLinearMedia = GetChannelIds(groupId);
            long lMediaId = long.Parse(mediaId.ToString());
            if (allLinearMedia != null && allLinearMedia.ContainsKey(lMediaId))
            {
                return allLinearMedia[lMediaId];
            }
            
            return null;
        }

        private static Tuple<Dictionary<long, string>, bool> GetAllLinearMedia(Dictionary<string, object> funcParams)
        {
            Dictionary<long, string> allLinearMedia = null;
            bool res = true;
            try
            {
                if (funcParams != null && funcParams.Count == 1)
                {
                    if (funcParams.ContainsKey("groupId"))
                    {
                        int? groupId = funcParams["groupId"] as int?;

                        if (groupId.HasValue)
                        {
                            DataTable dtAllLinearMedia = EpgDal.GetAllLinearMedia(groupId.Value);

                            if (dtAllLinearMedia != null && dtAllLinearMedia.Rows != null)
                            {
                                allLinearMedia = new Dictionary<long, string>();

                                foreach (DataRow drLinearMedia in dtAllLinearMedia.Rows)
                                {
                                    long linearMediaId = ODBCWrapper.Utils.GetLongSafeVal(drLinearMedia, "ID");
                                    string channelId = ODBCWrapper.Utils.GetSafeStr(drLinearMedia, "EPG_IDENTIFIER");

                                    allLinearMedia.Add(linearMediaId, channelId);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res = false;
                log.Error(string.Format("GetAllLinearMedia faild params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<long, string>, bool>(allLinearMedia, res);
        }

        private static List<ExtendedSearchResult> GetFuturePrograms(int groupId, string epgChannelId)
        {
            string adjacentProgramsKey = LayeredCacheKeys.GetAdjacentProgramsKey(groupId, epgChannelId);
            List<ExtendedSearchResult> adjacentPrograms = null;
            var invalidationKey = LayeredCacheKeys.GetAdjacentProgramsInvalidationKey(groupId, epgChannelId);

            if (!LayeredCache.Instance.Get(adjacentProgramsKey,
                ref adjacentPrograms,
                GetAdjacentPrograms,
                new Dictionary<string, object>() { { "groupId", groupId }, { "epgChannelId", epgChannelId } },
                groupId,
                LayeredCacheConfigNames.GET_ADJACENT_PROGRAMS,
                new List<string>() { invalidationKey }))
            {
                log.Error($"GetCurrentProgram.GetAdjacentPrograms - Failed get data from LayeredCache, key: {adjacentProgramsKey}.");
                return null;
            }

            return adjacentPrograms;
        }

        public static List<ExtendedSearchResult> GetPrograms(int groupId, string epgChannelId, int numberOfProgram = 1, bool isRetry = false)
        {
            var invalidationKey = LayeredCacheKeys.GetAdjacentProgramsInvalidationKey(groupId, epgChannelId);
            List<ExtendedSearchResult> adjacentPrograms = GetFuturePrograms(groupId, epgChannelId);
            
            if (adjacentPrograms != null && adjacentPrograms.Count > 0)
            {
                var availableProgram = adjacentPrograms.FindAll(x => !string.IsNullOrEmpty(x.AssetId));
                if (!isRetry && availableProgram.Count == 0)
                {
                    log.Debug($"GetPrograms - need to refresh LayeredCache. Invalidating by key: {invalidationKey}.");
                    LayeredCache.Instance.SetInvalidationKey(invalidationKey);
                    return GetPrograms(groupId, epgChannelId, numberOfProgram, true);
                }
                
                log.Debug($"GetPrograms - found programs");
                return numberOfProgram == 0 ? availableProgram : availableProgram.Take(numberOfProgram).ToList();
            }

            return null;
        }

        internal static ExtendedSearchResult GetCurrentProgram(int groupId, string epgChannelId, bool isRetry = false)
        {
            var invalidationKey = LayeredCacheKeys.GetAdjacentProgramsInvalidationKey(groupId, epgChannelId);
            List<ExtendedSearchResult> adjacentPrograms = GetFuturePrograms(groupId, epgChannelId);
        
            if (adjacentPrograms != null && adjacentPrograms.Count > 0)
            {
                var currentProgram = adjacentPrograms.FirstOrDefault(x => x.StartDate <= DateTime.UtcNow && x.EndDate >= DateTime.UtcNow && !string.IsNullOrEmpty(x.AssetId));
                if (currentProgram != null)
                {
                    log.Debug($"GetCurrentProgram - found program: {currentProgram.AssetId}");
                    return currentProgram;
                }
        
                if (!isRetry && !adjacentPrograms.Any(x => x.EndDate >= DateTime.UtcNow))
                {
                    log.Debug($"GetCurrentProgram - need to refresh LayeredCache. Invalidating by key: {invalidationKey}.");
                    LayeredCache.Instance.SetInvalidationKey(invalidationKey);
                    return GetCurrentProgram(groupId, epgChannelId, true);
                }
            }
        
            return null;
        }

        /// <summary>
        /// Search all Programs from now to the next 24 hours
        /// </summary>
        /// <param name="funcParams"></param>
        /// <returns></returns>
        private static Tuple<List<ExtendedSearchResult>, bool> GetAdjacentPrograms(Dictionary<string, object> funcParams)
        {
            List<ExtendedSearchResult> adjacentPrograms = null;

            try
            {
                if (funcParams != null && funcParams.Count == 2)
                {
                    if (funcParams.ContainsKey("groupId") && funcParams.ContainsKey("epgChannelId"))
                    {
                        int? groupId = funcParams["groupId"] as int?;
                        string epgChannelId = funcParams["epgChannelId"] as string;

                        if (groupId.HasValue && !string.IsNullOrEmpty(epgChannelId))
                        {
                            var programs = SearchPrograms(groupId.Value, epgChannelId, 100, OrderBy.START_DATE, ApiObjects.SearchObjects.OrderDir.ASC);

                            if (programs != null)
                            {
                                adjacentPrograms = new List<ExtendedSearchResult>(programs);

                                if (adjacentPrograms.Count == 0)
                                {
                                    var nextDateToRefreshCache = DateTime.UtcNow.AddHours(24);
                                    adjacentPrograms.Add(new ExtendedSearchResult() { EndDate = nextDateToRefreshCache });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"Exception at GetAdjacentPrograms. funcParams: {string.Join(";", funcParams.Keys)}, exception: {ex.ToString()}.");
            }

            return new Tuple<List<ExtendedSearchResult>, bool>(adjacentPrograms, adjacentPrograms != null);
        }

        /// <summary>
        /// Search all Programs from now to the next 24 hours
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="epgChannelId"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderBy"></param>
        /// <param name="orderDir"></param>
        /// <returns></returns>
        internal static List<ExtendedSearchResult> SearchPrograms(int groupId, string epgChannelId, int pageSize, OrderBy orderBy, ApiObjects.SearchObjects.OrderDir orderDir)
        {
            List<ExtendedSearchResult> programs = null;

            try
            {
                var ksql = $"(and epg_channel_id = '{epgChannelId}' end_date > '0')";

                var request = new ExtendedSearchRequest()
                {
                    m_nGroupID = groupId,
                    m_dServerTime = DateTime.UtcNow,
                    m_nPageIndex = 0,
                    m_nPageSize = pageSize,
                    assetTypes = new List<int> { 0 },
                    filterQuery = ksql,
                    order = new OrderObj()
                    {
                        m_eOrderBy = orderBy,
                        m_eOrderDir = orderDir
                    },
                    m_oFilter = new Filter()
                    {
                        m_bOnlyActiveMedia = true
                    },
                    ExtraReturnFields = new List<string> { },
                    isAllowedToViewInactiveAssets = true
                };

                FillCatalogSignature(request);
                UnifiedSearchResponse response = request.GetResponse(request) as UnifiedSearchResponse;

                if (response == null || response.status == null || response.searchResults == null)
                {
                    log.Error("Got empty response from Catalog 'GetResponse' for 'ExtendedSearchRequest'");
                    return programs;
                }

                if (response.status.Code != (int)eResponseStatus.OK)
                {
                    log.Error($"Got error response from catalog 'GetResponse' for 'ExtendedSearchRequest'. response status: {response.status.ToString()}.");
                    return programs;
                }

                programs = response.searchResults.ConvertAll(x => x as ExtendedSearchResult);
            }
            catch (Exception ex)
            {
                log.Error($"Failed SearchPrograms, channelId: {epgChannelId}, Exception: {ex.ToString()}.");
            }

            return programs;
        }

        internal static void FillCatalogSignature(BaseRequest request)
        {
            request.m_sSignString = Guid.NewGuid().ToString();
            request.m_sSignature = TVinciShared.WS_Utils.GetCatalogSignature(request.m_sSignString, ApplicationConfiguration.Current.CatalogSignatureKey.Value);
        }

        internal static LinearChannelSettings GetLinearChannelSettings(int groupId, long? epgChannelId)
        {
            LinearChannelSettings linearSettings = null;

            if (epgChannelId.HasValue && epgChannelId.Value > 0)
            {
                string strEpgChannelId = epgChannelId.Value.ToString();
                Dictionary<string, LinearChannelSettings> linearChannelSettings =
                    CatalogCache.Instance().GetLinearChannelSettings(groupId, new List<string>() { strEpgChannelId });

                if (linearChannelSettings != null && linearChannelSettings.Count > 0 && linearChannelSettings.ContainsKey(strEpgChannelId))
                {
                    linearSettings = linearChannelSettings[strEpgChannelId];
                }
            }

            if (linearSettings == null)
            {
                log.ErrorFormat("GetLinearChannelSettings - No LinearChannelSettings were found for groupId: {0} and epgChannelId: {1}.", groupId, epgChannelId);
            }

            return linearSettings;
        }
    }
}
