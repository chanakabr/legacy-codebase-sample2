using ApiObjects.Response;
using ApiObjects.SearchObjects;
using CachingProvider.LayeredCache;
using ConfigurationManager;
using Core.Catalog;
using Core.Catalog.Request;
using Core.Catalog.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tvinci.Core.DAL;

namespace APILogic.Api.Managers
{
    public class EpgManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        internal static string GetEpgChannelId(int mediaId, int groupId)
        {
            string allLinearMediaIdsKey = LayeredCacheKeys.GetAllLinearMediaKey(groupId);
            //TODO SHIR - ASK IRA FOR RELEVENT INVALIDATIONS KEYS (GetAllLinearMedia)
            Dictionary<long, string> allLinearMedia = null;

            if (!LayeredCache.Instance.Get<Dictionary<long, string>>(allLinearMediaIdsKey,
                                                                    ref allLinearMedia,
                                                                    GetAllLinearMedia,
                                                                    new Dictionary<string, object>() { { "groupId", groupId } },
                                                                    groupId,
                                                                    LayeredCacheConfigNames.GET_ALL_LINEAR_MEDIA))
            {
                log.ErrorFormat("GetEpgChannelId - GetAllLinearMedia - Failed get data from cache. groupId: {0}", groupId);
            }

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

        internal static long GetCurrentProgram(int groupId, string epgChannelId, bool isRetry = false)
        {
            string adjacentProgramsKey = LayeredCacheKeys.GetAdjacentProgramsKey(groupId, epgChannelId);
            List<ExtendedSearchResult> adjacentPrograms = null;

            if (!LayeredCache.Instance.Get<List<ExtendedSearchResult>>(adjacentProgramsKey,
                                                                    ref adjacentPrograms,
                                                                    GetAdjacentPrograms,
                                                                    new Dictionary<string, object>() { { "groupId", groupId }, { "epgChannelId", epgChannelId } },
                                                                    groupId,
                                                                    LayeredCacheConfigNames.GET_ADJACENT_PROGRAMS,
                                                                    new List<string>() { LayeredCacheKeys.GetAdjacentProgramsInvalidationKey(groupId, epgChannelId) }))
            {
                log.ErrorFormat("GetCurrentProgram - GetAdjacentPrograms - Failed get data from cache. groupId: {0}", groupId);
            }

            if (adjacentPrograms != null && adjacentPrograms.Count > 0)
            {
                var currentProgram = adjacentPrograms.FirstOrDefault(x => x.StartDate <= DateTime.UtcNow && x.EndDate >= DateTime.UtcNow);
                if (currentProgram != null)
                {
                    log.DebugFormat("GetCurrentProgram - found program: {0}", currentProgram.AssetId);
                    return long.Parse(currentProgram.AssetId);
                }
            }

            if (!isRetry)
            {
                log.DebugFormat("GetCurrentProgram - no program found.");
                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetAdjacentProgramsInvalidationKey(groupId, epgChannelId));
                return GetCurrentProgram(groupId, epgChannelId, true);
            }

            return 0;
        }

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
                            var programs = SearchPrograms(groupId.Value, epgChannelId, 1, OrderBy.START_DATE, OrderDir.ASC);

                            if (programs != null)
                            {
                                adjacentPrograms = new List<ExtendedSearchResult>(programs);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetAdjacentPrograms faild params : {0}", string.Join(";", funcParams.Keys)), ex);
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
                StringBuilder ksql = new StringBuilder();
                ksql.AppendFormat("(and epg_channel_id = '{0}' end_date > '0')", epgChannelId);

                ExtendedSearchRequest request = new ExtendedSearchRequest()
                {
                    m_nGroupID = groupId,
                    m_dServerTime = DateTime.UtcNow,
                    m_nPageIndex = 0,
                    m_nPageSize = pageSize,
                    assetTypes = new List<int> { 0 },
                    filterQuery = ksql.ToString(),
                    order = new ApiObjects.SearchObjects.OrderObj()
                    {
                        m_eOrderBy = orderBy,
                        m_eOrderDir = orderDir
                    },
                    m_oFilter = new Filter()
                    {
                        m_bOnlyActiveMedia = true
                    },
                    ExtraReturnFields = new List<string> { },
                };

                FillCatalogSignature(request);

                UnifiedSearchResponse response = request.GetResponse(request) as UnifiedSearchResponse;

                if (response == null || response.status == null)
                {
                    log.ErrorFormat("Got empty response from Catalog 'GetResponse' for 'ExtendedSearchRequest'");
                    return programs;
                }

                if (response.status.Code != (int)eResponseStatus.OK)
                {
                    log.ErrorFormat("Got error response from catalog 'GetResponse' for 'ExtendedSearchRequest'. response: code = {0}, message = {1}",
                                    response.status.Code, response.status.Message);
                    return programs;
                }

                programs = response.searchResults.ConvertAll(x => x as ExtendedSearchResult);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed SearchPrograms, channelId: {0}, Exception: {1}.", epgChannelId, ex.ToString());
            }

            return programs;
        }

        internal static void FillCatalogSignature(BaseRequest request)
        {
            request.m_sSignString = Guid.NewGuid().ToString();
            request.m_sSignature = TVinciShared.WS_Utils.GetCatalogSignature(request.m_sSignString, ApplicationConfiguration.CatalogSignatureKey.Value);
        }
    }
}
