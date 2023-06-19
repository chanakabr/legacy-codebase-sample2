using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using ApiLogic.Api.Validators;
using ApiObjects;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Phx.Lib.Appconfig;
using Core.Api;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.GroupManagers;
using DAL;
using GroupsCacheManager;
using Phx.Lib.Log;
using TVinciShared;
using Utils = ODBCWrapper.Utils;

namespace ApiLogic.Api.Managers
{
    public class RegionManager : IRegionManager
    {
        private static readonly KLogger Log = new KLogger(nameof(RegionManager));
        private static readonly Lazy<RegionManager> RegionManagerLazy = new Lazy<RegionManager>(
            () => new RegionManager(RegionValidator.Instance, CatalogManager.Instance, GroupSettingsManager.Instance, new GroupManager()),
            LazyThreadSafetyMode.PublicationOnly);
        private readonly IRegionValidator _regionValidator;
        private readonly ICatalogManager _catalogManager;
        private readonly IGroupSettingsManager _groupSettingsManager;
        private readonly IGroupManager _groupManager;

        public static RegionManager Instance => RegionManagerLazy.Value;
        private static readonly bool ShouldUseUpdateRegionPerformanceImprovement = ApplicationConfiguration.Current.CatalogLogicConfiguration.ShouldUseUpdateRegionPerformanceImprovement.Value;

        public RegionManager(
            IRegionValidator regionValidator,
            ICatalogManager catalogManager,
            IGroupSettingsManager groupSettingsManager,
            IGroupManager groupManager)
        {
            _regionValidator = regionValidator ?? throw new ArgumentNullException(nameof(regionValidator));
            _catalogManager = catalogManager ?? throw new ArgumentNullException(nameof(catalogManager));
            _groupSettingsManager = groupSettingsManager ?? throw new ArgumentNullException(nameof(groupSettingsManager));
            _groupManager = groupManager ?? throw new ArgumentNullException(nameof(groupManager));
        }

        internal Status DeleteRegion(int groupId, int id, long userId)
        {
            try
            {
                Region region = GetRegion(groupId, id);
                if (region == null)
                {
                    Log.Error($"Region wasn't found. groupId:{groupId}, id:{id}");
                    return new Status((int)eResponseStatus.RegionNotFound, "Region was not found");
                }

                if (region.isDefault)
                {
                    Log.Error($"Default region cannot be deleted. groupId:{groupId}, id:{id}");
                    return new Status((int)eResponseStatus.DefaultRegionCannotBeDeleted, "Default region cannot be deleted");
                }

                // check if region in use
                var status = CheckIsRegionInUse(groupId, region);
                if (!status.IsOkStatusCode())
                {
                    return status;
                }

                // TODO: what if the region is a parent??
                if (region.parentId == 0)
                {
                    var subRegions = GetRegions(groupId, new RegionFilter() { ParentId = region.id });
                    if (subRegions.HasObjects())
                    {
                        foreach (var subRegion in subRegions.Objects)
                        {
                            DeleteRegion(groupId, subRegion.id, userId);
                        }
                    }
                }


                if (!ApiDAL.DeleteRegion(groupId, id, userId))
                {
                    Log.Error($"Error while trying to delete region. groupId:{groupId}, id:{id}");
                    return new Status((int)eResponseStatus.Error);
                }

                InvalidateRegions(groupId);
            }
            catch (Exception exc)
            {
                Log.Error($"DeleteRegion Failed. regionId {id}, groupId: {groupId}. ex: {exc}");
                return new Status((int)eResponseStatus.Error);
            }

            return new Status((int)eResponseStatus.OK);
        }

        public GenericResponse<Region> UpdateRegion(int groupId, Region regionToUpdate, long userId)
        {
            var response = new GenericResponse<Region>();

            try
            {
                var region = GetRegion(groupId, regionToUpdate.id);

                var validationStatus = _regionValidator.IsValidToUpdate(groupId, regionToUpdate);
                if (!validationStatus.IsOkStatusCode())
                {
                    response.SetStatus(validationStatus);
                    return response;
                }

                //BEO-13685
                regionToUpdate.name = !regionToUpdate.name.IsNullOrEmptyOrWhiteSpace() ? regionToUpdate.name : region.name;
                regionToUpdate.externalId = !regionToUpdate.externalId.IsNullOrEmptyOrWhiteSpace() ? regionToUpdate.externalId : region.externalId;
                regionToUpdate.linearChannels = regionToUpdate.linearChannels != null ? regionToUpdate.linearChannels : region.linearChannels;

                if (!ApiDAL.UpdateRegion(groupId, regionToUpdate, userId))
                {
                    Log.ErrorFormat("Error while trying to update region. groupId:{0}, id:{1}", groupId, regionToUpdate.id);
                    response.SetStatus(eResponseStatus.Error);
                    return response;
                }

                InvalidateRegions(groupId);

                if (regionToUpdate.linearChannels?.Count > 0 || region.linearChannels?.Count > 0)
                {
                    List<long> assetsToIndex;
                    if (regionToUpdate.linearChannels?.Count > 0 && region.linearChannels?.Count > 0)
                    {
                        assetsToIndex = GetLinearChannelsDiff(region.linearChannels, regionToUpdate.linearChannels);
                    }
                    else if (regionToUpdate.linearChannels?.Count > 0)
                    {
                        assetsToIndex = regionToUpdate.linearChannels.Select(lc => lc.Key).ToList();
                    }
                    else
                    {
                        assetsToIndex = region.linearChannels?.Select(lc => lc.Key).ToList();
                    }

                    if (assetsToIndex?.Count > 0)
                    {
                        UpdateIndex(groupId, assetsToIndex);
                    }
                }

                if (regionToUpdate.parentId > 0)
                {
                    if (regionToUpdate.linearChannels == null)
                    {
                        regionToUpdate.linearChannels = new List<KeyValuePair<long, int>>();
                    }

                    var parentRegion = GetRegion(groupId, regionToUpdate.parentId);
                    regionToUpdate.linearChannels.AddRange(parentRegion.linearChannels);
                }
            }
            catch (Exception exc)
            {
                Log.ErrorFormat("UpdateRegion Failed. regionId {0}, groupId: {1}. ex: {2}", regionToUpdate.id, groupId, exc);
                response.SetStatus(eResponseStatus.Error);
                return response;
            }

            response.Object = regionToUpdate;
            response.SetStatus(eResponseStatus.OK);
            return response;
        }

        internal static bool InvalidateRegions(int groupId)
        {
            bool result = false;
            string invalidationKey = LayeredCacheKeys.GetRegionsInvalidationKey(groupId);
            if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
            {
                Log.ErrorFormat("Failed to set invalidation key for region. key = {0}", invalidationKey);
            }
            else
            {
                result = true;
            }

            return result;
        }

        public GenericResponse<Region> AddRegion(int groupId, Region region, long userId)
        {
            var response = new GenericResponse<Region>();

            try
            {
                var validationStatus = _regionValidator.IsValidToAdd(groupId, region);
                if (!validationStatus.IsOkStatusCode())
                {
                    response.SetStatus(validationStatus);
                    return response;
                }

                region.id = ApiDAL.AddRegion(groupId, region, userId);
                if (region.id == 0)
                {
                    Log.ErrorFormat("Error while trying to update region. groupId:{0}, id:{1}", groupId, region.id);
                    response.SetStatus(eResponseStatus.Error);
                    return response;
                }

                InvalidateRegions(groupId);

                if (region.linearChannels?.Count > 0)
                {
                    var assetIds = region.linearChannels.Select(lc => lc.Key).ToList();
                    UpdateIndex(groupId, assetIds);
                }

                if (region.parentId > 0)
                {
                    var parentRegion = GetRegion(groupId, region.parentId);
                    if (region.linearChannels == null)
                    {
                        region.linearChannels = parentRegion.linearChannels;
                    }
                    else
                    {
                        region.linearChannels.AddRange(parentRegion.linearChannels);
                    }
                }
            }
            catch (Exception exc)
            {
                Log.ErrorFormat("AddRegion Failed. regionId {0}, groupId: {1}. ex: {2}", region.id, groupId, exc);
                response.SetStatus(eResponseStatus.Error);
                return response;
            }

            response.Object = region;
            response.SetStatus(eResponseStatus.OK);
            return response;
        }

        public IReadOnlyDictionary<long, List<int>> GetLinearMediaToRegionsMapWhenEnabled(int groupId) => GetLinearMediaRegions(groupId);

        public Dictionary<long, List<int>> GetLinearMediaRegions(int groupId)
        {
            Dictionary<long, List<int>> res = null;

            try
            {
                string key = LayeredCacheKeys.GetLinearMediaRegionsKey(groupId);
                if (!LayeredCache.Instance.Get(key, ref res, GetLinearMediaRegionsMap, new Dictionary<string, object>() { { "groupId", groupId } }, groupId,
                    LayeredCacheConfigNames.GET_LINEAR_MEDIA_REGIONS_NAME_CACHE_CONFIG_NAME, new List<string>() { LayeredCacheKeys.GetRegionsInvalidationKey(groupId) }))
                {
                    Log.ErrorFormat("Failed getting GetLinearMediaRegions from LayeredCache, groupId: {0}", groupId);
                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Failed GetLinearMediaRegions with groupId: {0}", groupId), ex);
            }

            return res;
        }

        private Tuple<Dictionary<long, List<int>>, bool> GetLinearMediaRegionsMap(Dictionary<string, object> funcParams)
        {
            bool res = false;
            Dictionary<long, List<int>> result = new Dictionary<long, List<int>>();

            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    if (groupId.HasValue && groupId.Value > 0)
                    {
                        var regions = GetRegions(groupId.Value, new RegionFilter());
                        if (regions != null && regions.HasObjects())
                        {
                            foreach (var region in regions.Objects)
                            {
                                if (region.linearChannels?.Count > 0)
                                {
                                    foreach (var kvp in region.linearChannels)
                                    {
                                        long mediaId = kvp.Key;
                                        if (mediaId > 0)
                                        {
                                            if (!result.ContainsKey(mediaId))
                                            {
                                                result.Add(mediaId, new List<int>());
                                            }

                                            result[mediaId].Add(region.id);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                res = result != null;
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("GetLinearMediaRegionsFromDB failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<long, List<int>>, bool>(result, res);
        }

        private static RegionsCache GetRegionsFromCache(int groupId)
        {
            RegionsCache regionsCache = null;

            try
            {
                string key = LayeredCacheKeys.GetRegionsKey(groupId);
                List<string> regionsInvalidationKey = new List<string>() { LayeredCacheKeys.GetRegionsInvalidationKey(groupId) };
                if (!LayeredCache.Instance.Get<RegionsCache>(key,
                                                          ref regionsCache,
                                                          GetAllRegionsDB,
                                                          new Dictionary<string, object>() { { "groupId", groupId } },
                                                          groupId,
                                                          LayeredCacheConfigNames.GET_GROUP_REGIONS,
                                                          regionsInvalidationKey))
                {
                    Log.ErrorFormat("Failed getting GetRegions from LayeredCache, groupId: {0}, key: {1}", groupId, key);
                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Failed GetRegions for groupId: {0}", groupId), ex);
            }

            return regionsCache;
        }

        internal static Region GetRegion(int groupId, int id)
        {
            Region result = null;

            RegionsCache regionsCache = GetRegionsFromCache(groupId);

            if (regionsCache != null && regionsCache.Regions != null && regionsCache.Regions.ContainsKey(id))
            {
                result = regionsCache.Regions[id];
            }

            return result;
        }

        public List<int> GetRegionIds(int groupId) => GetRegionsFromCache(groupId)?.Regions?.Keys.ToList();

        public List<long> GetChildRegionIds(int groupId, long parentRegionId)
        {
            var result = new List<long>();
            var regionCache = GetRegionsFromCache(groupId);
            if (regionCache != null && regionCache.ParentIdsToRegionIdsMapping.TryGetValue((int)parentRegionId, out var childRegions))
            {
                result.AddRange(childRegions.Select(x => (long)x));
            }

            return result;
        }

        public GenericListResponse<Region> GetRegions(int groupId, RegionFilter filter, int pageIndex = 0, int pageSize = 0)
        {
            GenericListResponse<Region> result = new GenericListResponse<Region>();

            try
            {
                if (filter.LiveAssetId > 0)
                {
                    var map = GetLinearMediaRegions(groupId);
                    if (map != null && map.ContainsKey(filter.LiveAssetId))
                    {
                        filter.RegionIds = map[filter.LiveAssetId];
                    }

                    if (filter.RegionIds == null || filter.RegionIds.Count == 0)
                    {
                        result.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                        return result;
                    }
                }

                RegionsCache regionsCache = GetRegionsFromCache(groupId);

                if (regionsCache != null)
                {
                    result.Objects = regionsCache.Regions.Values.ToList();
                    result.TotalItems = regionsCache.Regions.Count;
                    result.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());

                    if (filter.RegionIds?.Count > 0)
                    {
                        result.Objects = regionsCache.Regions.Where(r => filter.RegionIds.Contains(r.Key)).Select(r => r.Value).ToList();
                        result.TotalItems = result.Objects.Count;

                        if (filter.ParentOnly)
                        {
                            result.Objects = result.Objects.Where(x => x.parentId == 0).ToList();
                            result.TotalItems = result.Objects.Count;
                        }
                    }
                    else if (filter.ExternalIds?.Count > 0)
                    {
                        var idsToFilter = regionsCache.ExternalIdsMapping.Where(eid => filter.ExternalIds.Contains(eid.Key)).Select(e => e.Value);
                        result.Objects = regionsCache.Regions.Where(r => idsToFilter.Contains(r.Key)).Select(r => r.Value).ToList();
                        result.TotalItems = result.Objects.Count;
                    }
                    else if (filter.ParentId > 0)
                    {
                        if (regionsCache.ParentIdsToRegionIdsMapping.ContainsKey(filter.ParentId))
                        {
                            var idsToFilter = regionsCache.ParentIdsToRegionIdsMapping[filter.ParentId];
                            result.Objects = regionsCache.Regions.Where(r => idsToFilter.Contains(r.Key)).Select(r => r.Value).ToList();
                            result.TotalItems = result.Objects.Count;
                        }
                        else
                        {
                            result.Objects = null;
                            result.TotalItems = 0;
                        }
                    }
                    else if (filter.ParentOnly)
                    {
                        result.Objects = regionsCache.Regions.Where(x => x.Value.parentId == 0).Select(x => x.Value).ToList();
                        result.TotalItems = result.Objects.Count;
                    }

                    if (result.Status.Code == (int)eResponseStatus.OK && result.Objects?.Count > 0)
                    {
                        if (filter.orderBy == RegionOrderBy.CreateDateAsc)
                        {
                            result.Objects = result.Objects.OrderBy(r => r.createDate).ToList();
                        }
                        else if (filter.orderBy == RegionOrderBy.CreateDateDesc)
                        {
                            result.Objects = result.Objects.OrderByDescending(r => r.createDate).ToList();
                        }

                        result.Objects = pageSize > 0 ? result.Objects.Skip(pageIndex * pageSize).Take(pageSize).ToList() : result.Objects;

                        foreach (var item in result.Objects)
                        {
                            if (item.parentId > 0 && regionsCache.Regions.ContainsKey(item.parentId))
                            {
                                if (item.linearChannels == null)
                                {
                                    item.linearChannels = new List<KeyValuePair<long, int>>();
                                }

                                var parentRegion = regionsCache.Regions[item.parentId];
                                if (filter.ExclusiveLcn)
                                {
                                    item.linearChannels = item.linearChannels
                                        .Where(x => !parentRegion.linearChannels.Contains(x))
                                        .ToList();
                                }
                                else
                                {
                                    var missingLinearChannels = parentRegion.linearChannels
                                        .Where(x => !item.linearChannels.Contains(x))
                                        .ToList();
                                    item.linearChannels.AddRange(missingLinearChannels);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Failed GetRegions for groupId: {0}", groupId), ex);
            }

            return result;
        }

        private static Tuple<RegionsCache, bool> GetAllRegionsDB(Dictionary<string, object> funcParams)
        {
            RegionsCache regionsCache = null;

            try
            {
                int? groupId = funcParams["groupId"] as int?;
                if (groupId.HasValue)
                {
                    DataSet ds = ApiDAL.Get_Regions(groupId.Value, null); // TODO: move the ordering to code from SP

                    if (ds != null && ds.Tables != null && ds.Tables.Count >= 2)
                    {
                        regionsCache = new RegionsCache();
                        Region region;

                        if (ds.Tables[0] != null && ds.Tables[0].Rows != null)
                        {
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                region = new Region
                                {
                                    id = Utils.GetIntSafeVal(row, "id"),
                                    name = Utils.GetSafeStr(row, "name"),
                                    externalId = Utils.GetSafeStr(row, "external_id"),
                                    isDefault = Utils.GetIntSafeVal(row, "IS_DEFAULT_REGION") == 1,
                                    parentId = Utils.GetIntSafeVal(row, "parent_id"),
                                    createDate = Utils.GetDateSafeVal(row, "create_date")
                                };

                                regionsCache.Regions.Add(region.id, region);
                                if (!string.IsNullOrEmpty(region.externalId))
                                {
                                    regionsCache.ExternalIdsMapping.Add(region.externalId, region.id);
                                }

                                if (region.parentId > 0)
                                {
                                    if (!regionsCache.ParentIdsToRegionIdsMapping.ContainsKey(region.parentId))
                                    {
                                        regionsCache.ParentIdsToRegionIdsMapping.Add(region.parentId, new List<int>());
                                    }
                                    regionsCache.ParentIdsToRegionIdsMapping[region.parentId].Add(region.id);
                                }
                            }
                        }

                        if (ds.Tables[1] != null && ds.Tables[1].Rows != null)
                        {
                            int regionId;
                            int assetId;
                            foreach (DataRow row in ds.Tables[1].Rows)
                            {
                                regionId = APILogic.Utils.GetIntSafeVal(row, "region_id");
                                region = regionsCache.Regions.ContainsKey(regionId) ? regionsCache.Regions[regionId] : null;
                                if (region != null)
                                {
                                    assetId = APILogic.Utils.GetIntSafeVal(row, "media_id");
                                    region.linearChannels.Add(new KeyValuePair<long, int>(assetId, APILogic.Utils.GetIntSafeVal(row, "channel_number")));
                                }
                            }

                            foreach (var key in regionsCache.ParentIdsToRegionIdsMapping.Keys)
                            {
                                if (regionsCache.Regions.ContainsKey(key))
                                {
                                    Region parent = regionsCache.Regions[key];
                                    parent.childrenCount = regionsCache.ParentIdsToRegionIdsMapping[key].Count;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(GetAllRegionsDB)} failed, parameters : {string.Join(";", funcParams.Keys)}", ex);
                regionsCache = null;
            }

            return new Tuple<RegionsCache, bool>(regionsCache, regionsCache != null);
        }

        private static List<long> GetLinearChannelsDiff(IEnumerable<KeyValuePair<long, int>> newLinearChannels, IEnumerable<KeyValuePair<long, int>> existingLinearChannels)
        {
            var existingIds = existingLinearChannels.Select(x => x.Key).ToList();
            var newIds = newLinearChannels.Select(x => x.Key).ToList();

            var diffIds = existingIds.Except(newIds)
                .Concat(newIds.Except(existingIds))
                .ToList();

            return diffIds;
        }

        public long? GetDefaultRegionId(int groupId)
        {
            if (!_catalogManager.IsRegionalizationEnabled(groupId))
            {
                return null;
            }

            if (!_groupSettingsManager.IsOpc(groupId))
            {
                var group = _groupManager.GetGroup(groupId);

                return group?.defaultRegion > 0 ? group.defaultRegion : (long?)null;
            }

            if (!_catalogManager.TryGetCatalogGroupCacheFromCache(groupId, out var catalogGroupCache))
            {
                return null;
            }

            return catalogGroupCache.DefaultRegion > 0 ? catalogGroupCache.DefaultRegion : (long?)null;
        }

        internal GenericListResponse<Region> GetDefaultRegion(int groupId)
        {
            GenericListResponse<Region> result = new GenericListResponse<Region>();

            try
            {
                var generalPartnerConfig = GeneralPartnerConfigManager.Instance.GetGeneralPartnerConfiguration(groupId);
                if (generalPartnerConfig.IsOkStatusCode() && generalPartnerConfig?.Objects?.Count > 0 && generalPartnerConfig.Objects[0].DefaultRegion.HasValue)
                {
                    var defaultRegionId = generalPartnerConfig.Objects[0].DefaultRegion.Value;

                    RegionFilter filter = new RegionFilter() { RegionIds = new List<int>(defaultRegionId) };
                    return GetRegions(groupId, filter);
                }
                else
                {
                    result.Objects = null;
                    result.TotalItems = 0;
                    result.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Failed GetDefaultRegion for groupId: {0}", groupId), ex);
            }

            return result;
        }

        public Status BulkUpdateRegions(int groupId, long userId, long linearChannelId, IReadOnlyCollection<RegionChannelNumber> regionChannelNumbers)
        {
            try
            {
                var validationResult = _regionValidator.IsValidToBulkUpdate(groupId, linearChannelId, regionChannelNumbers);
                if (!validationResult.IsOkStatusCode())
                {
                    return validationResult;
                }

                var dbUpdateResult = ApiDAL.UpdateLinearChannelRegions(groupId, linearChannelId, regionChannelNumbers, userId);
                if (!dbUpdateResult)
                {
                    Log.Error("Error while trying to update linear channel in multiple regions.");
                    return new Status(eResponseStatus.Error);
                }

                InvalidateRegions(groupId);

                UpdateIndex(groupId, linearChannelId);

                return Status.Ok;
            }
            catch (Exception e)
            {
                Log.Error($"{nameof(BulkUpdateRegions)} failed.", e);

                return Status.Error;
            }
        }

        private static void UpdateIndex(int groupId, IEnumerable<long> linearChannelIds)
        {
            foreach (var linearChannelId in linearChannelIds.Distinct())
            {
                UpdateIndex(groupId, linearChannelId);
            }
        }

        private static void UpdateIndex(int groupId, long linearChannelId)
        {
            var epgKsql = $"(and linear_media_id='{linearChannelId}')";
            var epgResult = api.SearchAssets(groupId, epgKsql, 0, 0, true, 0, false, string.Empty, string.Empty, string.Empty, 0, 0, true, true);

            var result = CatalogLogic.UpdateIndex(new List<long> { linearChannelId }, groupId, eAction.Update);
            if (!result)
            {
                Log.Error($"Index update failed. {nameof(groupId)}:{groupId}, {nameof(linearChannelId)}:{linearChannelId}");
            }

            if (epgResult?.Length > 0)
            {
                var epgIds = epgResult.Select(x => long.Parse(x.AssetId)).ToList();
                result = ShouldUseUpdateRegionPerformanceImprovement
                    ? CatalogLogic.UpdateEpgRegionsIndex(epgIds, new[] { linearChannelId }, groupId, eAction.Update, null, false)
                    : CatalogLogic.UpdateEpgIndex(epgIds, groupId, eAction.Update, null, false);

                if (!result)
                {
                    Log.Error($"Index update failed. {nameof(groupId)}:{groupId}, {nameof(epgIds)}:{string.Join(",", epgIds)}");
                }
            }
        }

        public GenericResponse<Region> GetRegion(long groupId, long regionId)
        {
            var regionFilter = new RegionFilter
            {
                RegionIds = new List<int> { (int)regionId }
            };
            var response = GetRegions((int)groupId, regionFilter);

            return response.IsOkStatusCode()
                ? response.HasObjects()
                    ? new GenericResponse<Region>(Status.Ok, response.Objects.First())
                    : new GenericResponse<Region>(eResponseStatus.RegionNotFound)
                : new GenericResponse<Region>(response.Status);
        }

        private Status CheckIsRegionInUse(int groupId, Region region)
        {
            if (DomainDal.IsRegionInUse(groupId, region.id))
            {
                Log.Error($"Region in use by household and cannot be deleted. groupId:{groupId}, id:{region.id}");
                return new Status((int)eResponseStatus.CannotDeleteRegionInUse, "Region in use by household and cannot be deleted");
    }

            if (region.parentId == 0)
            {
                var subRegions = GetRegions(groupId, new RegionFilter() { ParentId = region.id });
                if (subRegions.HasObjects())
                {
                    foreach (var subRegion in subRegions.Objects)
                    {
                        if (DomainDal.IsRegionInUse(groupId, subRegion.id))
                        {
                            Log.Error($"Region has sub-region in use by household and cannot be deleted. groupId:{groupId}, regionId:{region.id}, subRegionId: {subRegion.id}");
                            return new Status((int)eResponseStatus.CannotDeleteSubRegionInUse, "Region has sub-region in use by household and cannot be deleted");
}
                    }
                }
            }

            return Status.Ok;
        }
    }
}
