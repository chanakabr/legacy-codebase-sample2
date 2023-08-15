using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ApiLogic.Catalog.NextEpisode;
using ApiObjects.Response;
using Core.Catalog.Response;
using Core.GroupManagers;
using Phx.Lib.Log;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.InternalModels;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Mappers;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.ObjectsConvertor.Extensions;
using WebAPI.Utils;
using WebAPI.Validation;

namespace WebAPI.Controllers
{
    [Service("assetHistory")]
    public class AssetHistoryController : IKalturaController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Get recently watched media for user, ordered by recently watched first.    
        /// </summary>
        /// <param name="filter">Filter parameters for filtering out the result</param>
        /// <param name="pager"><![CDATA[Page size and index. Number of assets to return per page. Possible range 5 ≤ size ≥ 50. If omitted - will be set to 25. If a value > 50 provided – will set to 50]]></param>
        /// <remarks>Possible status codes: 
        /// </remarks>
        [Action("list")]
        [ApiAuthorize]
        [Throws(eResponseStatus.BadSearchRequest)]
        static public KalturaAssetHistoryListResponse List(KalturaAssetHistoryFilter filter = null, KalturaFilterPager pager = null)
        {
            KalturaAssetHistoryListResponse response = null;
            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;
            string udid = KSUtils.ExtractKSPayload().UDID;
            KalturaBaseResponseProfile responseProfile = Utils.Utils.GetResponseProfileFromRequest();

            if (pager == null)
                pager = new KalturaFilterPager();

            // page size - 5 <= size <= 50
            if (pager.PageSize == 0)
            {
                pager.PageSize = 25;
            }
            else if (pager.PageSize > 50)
            {
                pager.PageSize = 50;
            }
            else if (pager.PageSize < 5)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED, "KalturaAssetHistoryFilter.pageSize", "5");
            }

            if (filter == null)
            {
                filter = new KalturaAssetHistoryFilter();
            }

            // validate and set filter status value if not provided
            if (!filter.StatusEqual.HasValue)
            {
                filter.StatusEqual = KalturaWatchStatus.all;
            }

            // days - default value 7
            if (filter.DaysLessThanOrEqual == null || (filter.DaysLessThanOrEqual.HasValue && filter.DaysLessThanOrEqual.Value == 0))
                filter.DaysLessThanOrEqual = 7;

            string language = Utils.Utils.GetLanguageFromRequest();

            try
            {
                bool suppress = false;
                if (responseProfile != null && responseProfile is KalturaDetachedResponseProfile detachedResponseProfile)
                {
                    var profile = detachedResponseProfile.RelatedProfiles?.FirstOrDefault(x => x.Filter is KalturaAssetHistorySuppressFilter);

                    if (profile != null)
                    {
                        suppress = true;
                    }
                }

                // call client
                response = ClientsManager.CatalogClient().getAssetHistory(groupId, userId.ToString(), udid,
                    language, pager.GetRealPageIndex(), pager.PageSize, filter.StatusEqual.Value, filter.getDaysLessThanOrEqual(), filter.getTypeIn(), filter.getAssetIdIn(),
                    suppress, filter.Ksql);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response; 
        }

        /// <summary>
        /// Get recently watched media for user, ordered by recently watched first.    
        /// </summary>
        /// <param name="filter">Filter parameters for filtering out the result</param>
        /// <param name="pager"><![CDATA[Page size and index. Number of assets to return per page. Possible range 5 ≤ size ≥ 50. If omitted - will be set to 25. If a value > 50 provided – will set to 50]]></param>
        /// <remarks>Possible status codes: 
        /// </remarks>
        [Action("listOldStandard")]
        [OldStandardAction("list")]
        [ApiAuthorize]
        [Obsolete]
        static public KalturaWatchHistoryAssetWrapper ListOldStandard(KalturaAssetHistoryFilter filter = null, KalturaFilterPager pager = null)
        {
            KalturaWatchHistoryAssetWrapper response = null;
            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;
            string udid = KSUtils.ExtractKSPayload().UDID;

            if (pager == null)
                pager = new KalturaFilterPager();

            // page size - 5 <= size <= 50
            if (pager.PageSize == 0)
            {
                pager.PageSize = 25;
            }
            else if (pager.PageSize > 50)
            {
                pager.PageSize = 50;
            }
            else if (pager.PageSize < 5)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED, "KalturaFilterPager.pageSize", "5");
            }

            if (filter == null)
            {
                filter = new KalturaAssetHistoryFilter();
            }

            // validate and set filter status value if not provided
            if (!filter.StatusEqual.HasValue)
            {
                filter.StatusEqual = KalturaWatchStatus.all;
            }

            // days - default value 7
            if (filter.DaysLessThanOrEqual == null || (filter.DaysLessThanOrEqual.HasValue && filter.DaysLessThanOrEqual.Value == 0))
                filter.DaysLessThanOrEqual = 7;

            string language = Utils.Utils.GetLanguageFromRequest();

            try
            {
                var withList = filter.with != null ?
                    filter.with.Select(x => x.type).ToList() :
                    new List<KalturaCatalogWith>();

                // call client
                response = ClientsManager.CatalogClient().WatchHistory(groupId, userId.ToString(), udid,
                    language, pager.GetRealPageIndex(), pager.PageSize, filter.StatusEqual.Value, filter.getDaysLessThanOrEqual(), filter.getTypeIn(), filter.getAssetIdIn(), withList);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Clean the user’s viewing history
        /// </summary>
        /// <param name="filter">List of assets identifier</param>
        /// <returns></returns>
        [Action("cleanOldStandard")]
        [OldStandardAction("clean")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        static public bool CleanOldStandard(KalturaAssetsFilter filter = null)
        {
            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;

            try
            {
                // call client
                return ClientsManager.ApiClient().CleanUserHistory(groupId, userId, filter.Assets);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return true;
        }

        /// <summary>
        /// Clean the user’s viewing history
        /// </summary>
        /// <param name="filter">Filter for cleaning asset history</param>
        /// <returns></returns>
        [Action("clean")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.BadSearchRequest)]
        static public void Clean(KalturaAssetHistoryFilter filter = null)
        {
            var contextData = KS.GetContextData();

            if (filter == null)
            {
                filter = new KalturaAssetHistoryFilter();
            }

            // days - default value 7
            if (filter.DaysLessThanOrEqual == null || (filter.DaysLessThanOrEqual.HasValue && filter.DaysLessThanOrEqual.Value == 0))
                filter.DaysLessThanOrEqual = 7;

            if (!filter.StatusEqual.HasValue)
            {
                filter.StatusEqual = KalturaWatchStatus.all;
            }

            // validate typeIn - can be multiple only if does not contain recordings!
            List<int> filterTypes = filter.getTypeIn();
            if (filterTypes != null && filterTypes.Count > 1)
            {
                if (filterTypes.Contains(1))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER,
                        "KalturaAssetHistoryFilter.typeIn containing recording (1)", "KalturaAssetHistoryFilter.typeIn with single / multiple media types");
                }

                if (filterTypes.Contains(0))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER,
                        "KalturaAssetHistoryFilter.typeIn containing program (0)", "KalturaAssetHistoryFilter.typeIn with single / multiple media types");
                }
            }

            try
            {
                var userId = contextData.UserId ?? default;
                // call client
                ClientsManager.ApiClient().CleanUserAssetHistory(contextData.GroupId, userId, contextData.Udid, filter.getAssetIdIn(), filterTypes, filter.StatusEqual.Value, filter.getDaysLessThanOrEqual(), filter.Ksql);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
        }

        /// <summary>
        /// Get next episode by last watch asset in given assetId
        /// </summary>
        /// <param name="assetId">asset Id of series to search for next episode</param>
        /// <param name="seriesIdArguments">series Id arguments</param>
        /// <param name="watchedAllReturnStrategy">watched all series episodes strategy</param>
        /// <param name="notWatchedReturnStrategy">not watched any episode strategy</param>
        /// <returns></returns>
        [Action("getNextEpisode")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [SchemeArgument("assetId", MinLong = 1)]
        [SchemaMethod(OneOf = new[] { "assetId", "seriesIdArguments" })]
        [Throws(eResponseStatus.InvalidAssetType)]
        [Throws(eResponseStatus.InvalidAssetStruct)]
        [Throws(eResponseStatus.TopicNotFound)]
        [Throws(eResponseStatus.MetaDoesNotExist)]
        [Throws(eResponseStatus.NoNextEpisode)]
        public static KalturaAssetHistory GetNextEpisode(
            long? assetId = null,
            KalturaSeriesIdArguments seriesIdArguments = null,
            KalturaNotWatchedReturnStrategy? notWatchedReturnStrategy = null,
            KalturaWatchedAllReturnStrategy? watchedAllReturnStrategy = null)
        {
            var contextData = KS.GetContextData();
            var context = NextEpisodeMapper.Instance.MapToContext(contextData, notWatchedReturnStrategy, watchedAllReturnStrategy);
            NextEpisodeValidator.Instance.Validate(contextData.GroupId, seriesIdArguments);
            var isOpcAccount = GroupSettingsManager.Instance.IsOpc(contextData.GroupId);
            GenericResponse<UserWatchHistory> response;
            if (!isOpcAccount)
            {
                response = assetId.HasValue
                    ? NextEpisodeService.GetNextEpisodeByAssetIdForTvm(context, assetId.Value)
                    : NextEpisodeService.GetNextEpisodeBySeriesIdForTvm(context, seriesIdArguments.SeriesId);
            }
            else if (assetId.HasValue)
            {
                response = NextEpisodeService.GetNextEpisodeByAssetIdForOpc(context, assetId.Value);
            }
            else
            {
                var seriesType = NextEpisodeMapper.Instance.MapToSeriesType(seriesIdArguments);
                response = NextEpisodeService.GetNextEpisodeBySeriesIdForOpc(context, seriesIdArguments.SeriesId, seriesType);
            }

            return !response.IsOkStatusCode()
                ? throw new ClientException(response.Status)
                : AutoMapper.Mapper.Map<KalturaAssetHistory>(response.Object);
        }
    }
}