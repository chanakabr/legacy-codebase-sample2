using ApiObjects;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Api;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using ApiLogic.Api.Validators;
using ConfigurationManager;
using KeyValuePair = ApiObjects.KeyValuePair;

namespace ApiLogic.Api.Managers
{
    public class RegionManager : IRegionManager
    {
        private static readonly Lazy<RegionManager> Lazy = new Lazy<RegionManager>(() => new RegionManager(), LazyThreadSafetyMode.PublicationOnly);
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly IRegionValidator _regionValidator = new RegionValidator();
        private static readonly bool ShouldUseUpdateRegionPerformanceImprovement = ApplicationConfiguration.Current.CatalogLogicConfiguration.ShouldUseUpdateRegionPerformanceImprovement.Value;

        public static RegionManager Instance => Lazy.Value;

        internal static Status DeleteRegion(int groupId, int id, long userId)
        {
            try
            {
                Region region = GetRegion(groupId, id);
                if (region == null)
                {
                    log.Error($"Region wasn't found. groupId:{groupId}, id:{id}");
                    return new Status((int)eResponseStatus.RegionNotFound, "Region was not found");
                }

                if (region.isDefault)
                {
                    log.Error($"Default region cannot be deleted. groupId:{groupId}, id:{id}");
                    return new Status((int)eResponseStatus.DefaultRegionCannotBeDeleted, "Default region cannot be deleted");
                }

                // check if region in use
                if (DomainDal.IsRegionInUse(groupId, id))
                {
                    log.Error($"Region in use by household and cannot be deleted. groupId:{groupId}, id:{id}");
                    return new Status((int)eResponseStatus.CannotDeleteRegionInUse, "Region in use by household and cannot be deleted");
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
                    log.Error($"Error while trying to delete region. groupId:{groupId}, id:{id}");
                    return new Status((int)eResponseStatus.Error);
                }

                InvalidateRegions(groupId);
            }
            catch (Exception exc)
            {
                log.Error($"DeleteRegion Failed. regionId {id}, groupId: {groupId}. ex: {exc}");
                return new Status((int)eResponseStatus.Error);
            }

            return new Status((int)eResponseStatus.OK);
        }

        internal static GenericResponse<Region> UpdateRegion(int groupId, Region regionToUpdate, long userId)
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

                if (!ApiDAL.UpdateRegion(groupId, regionToUpdate, userId))
                {
                    log.ErrorFormat("Error while trying to update region. groupId:{0}, id:{1}", groupId, regionToUpdate.id);
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
                log.ErrorFormat("UpdateRegion Failed. regionId {0}, groupId: {1}. ex: {2}", regionToUpdate.id, groupId, exc);
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
                log.ErrorFormat("Failed to set invalidation key for region. key = {0}", invalidationKey);
            }
            else
            {
                result = true;
            }

            return result;
        }

        internal static GenericResponse<Region> AddRegion(int groupId, Region region, long userId)
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
                    log.ErrorFormat("Error while trying to update region. groupId:{0}, id:{1}", groupId, region.id);
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
                log.ErrorFormat("AddRegion Failed. regionId {0}, groupId: {1}. ex: {2}", region.id, groupId, exc);
                response.SetStatus(eResponseStatus.Error);
                return response;
            }

            response.Object = region;
            response.SetStatus(eResponseStatus.OK);
            return response;
        }

        public static IReadOnlyDictionary<long, List<int>> GetLinearMediaToRegionsMapWhenEnabled(int groupId)
        {
            var linearChannelsRegionsMapping = CatalogManager.Instance.IsRegionalizationEnabled(groupId)
                ? GetLinearMediaRegions(groupId)
                : new Dictionary<long, List<int>>();

            return linearChannelsRegionsMapping;
        }

        public static Dictionary<long, List<int>> GetLinearMediaRegions(int groupId)
        {
            Dictionary<long, List<int>> res = null;

            try
            {
                string key = LayeredCacheKeys.GetLinearMediaRegionsKey(groupId);
                if (!LayeredCache.Instance.Get(key, ref res, GetLinearMediaRegionsMap, new Dictionary<string, object>() { { "groupId", groupId } }, groupId,
                    LayeredCacheConfigNames.GET_LINEAR_MEDIA_REGIONS_NAME_CACHE_CONFIG_NAME, new List<string>() { LayeredCacheKeys.GetRegionsInvalidationKey(groupId) }))
                {
                    log.ErrorFormat("Failed getting GetLinearMediaRegions from LayeredCache, groupId: {0}", groupId);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetLinearMediaRegions with groupId: {0}", groupId), ex);
            }

            return res;
        }

        private static Tuple<Dictionary<long, List<int>>, bool> GetLinearMediaRegionsMap(Dictionary<string, object> funcParams)
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
                log.Error(string.Format("GetLinearMediaRegionsFromDB failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
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
                    log.ErrorFormat("Failed getting GetRegions from LayeredCache, groupId: {0}, key: {1}", groupId, key);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetRegions for groupId: {0}", groupId), ex);
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

        internal static List<int> GetRegionIds(int groupId)
        {
            List<int> result = null;

            RegionsCache regionsCache = GetRegionsFromCache(groupId);

            if (regionsCache != null && regionsCache.Regions != null)
            {
                result = regionsCache.Regions.Keys.ToList();
            }

            return result;
        }

        public static GenericListResponse<Region> GetRegions(int groupId, RegionFilter filter, int pageIndex = 0, int pageSize = 0)
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

                                if (filter.ExclusiveLcn)
                                {
                                    var parentChannelIds = regionsCache.Regions[item.parentId].linearChannels.Select(x => x.Key);
                                    item.linearChannels = item.linearChannels.Where(x => !parentChannelIds.Contains(x.Key)).ToList();
                                }
                                else
                                {
                                    var currentChannelIds = item.linearChannels.Select(x => x.Key);
                                    var missingChannels = regionsCache.Regions[item.parentId].linearChannels.Where(x => !currentChannelIds.Contains(x.Key));
                                    item.linearChannels.AddRange(missingChannels);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetRegions for groupId: {0}", groupId), ex);
            }

            return result;
        }

        private static Tuple<List<int>, bool> GetRegionsFromDB(Dictionary<string, object> funcParams)
        {
            bool res = false;
            List<int> result = null;

            try
            {
                if (funcParams != null && funcParams.Count == 1 && funcParams.ContainsKey("groupId"))
                {
                    int? groupId;
                    groupId = funcParams["groupId"] as int?;
                    if (groupId.HasValue)
                    {
                        var ds = ApiDAL.Get_Regions(groupId.Value, null);

                        if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                        {
                            if (ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                            {
                                res = true;

                                if (result == null)
                                {
                                    result = new List<int>();
                                }

                                foreach (DataRow dr in ds.Tables[0].Rows)
                                {
                                    int id = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
                                    result.Add(id);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetRegionsFromDB failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<List<int>, bool>(result, res);
        }

        private static Tuple<RegionsCache, bool> GetAllRegionsDB(Dictionary<string, object> funcParams)
        {
            RegionsCache regionsCache = null;

            try
            {
                int? groupId = funcParams["groupId"] as int?;
                if (groupId.HasValue)
                {
                    DataSet ds = DAL.ApiDAL.Get_Regions(groupId.Value, null); // TODO: move the ordering to code from SP

                    if (ds != null && ds.Tables != null && ds.Tables.Count >= 2)
                    {
                        regionsCache = new RegionsCache();
                        Region region;

                        if (ds.Tables[0] != null && ds.Tables[0].Rows != null)
                        {
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                region = new Region()
                                {
                                    id = ODBCWrapper.Utils.GetIntSafeVal(row, "id"),
                                    name = ODBCWrapper.Utils.GetSafeStr(row, "name"),
                                    externalId = ODBCWrapper.Utils.GetSafeStr(row, "external_id"),
                                    isDefault = ODBCWrapper.Utils.GetIntSafeVal(row, "IS_DEFAULT_REGION") == 1,
                                    parentId = ODBCWrapper.Utils.GetIntSafeVal(row, "parent_id"),
                                    createDate = ODBCWrapper.Utils.GetDateSafeVal(row, "create_date")
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
                log.Error(string.Format("GetAllAssetRulesDB failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
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

        internal static GenericListResponse<Region> GetDefaultRegion(int groupId)
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
                log.Error(string.Format("Failed GetDefaultRegion for groupId: {0}", groupId), ex);
            }

            return result;
        }

        internal static Status BulkUpdateRegions(int groupId, long userId, long linearChannelId, IReadOnlyCollection<RegionChannelNumber> regionChannelNumbers)
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
                    log.Error("Error while trying to update linear channel in multiple regions.");
                    return new Status(eResponseStatus.Error);
                }

                InvalidateRegions(groupId);

                UpdateIndex(groupId, linearChannelId);

                return Status.Ok;
            }
            catch (Exception e)
            {
                log.Error($"{nameof(BulkUpdateRegions)} failed.", e);

                return Status.Error;
            }
        }

        private static void UpdateIndex(int groupId, IEnumerable<long> linearChannelIds)
        {
            foreach (var linearChannelId in linearChannelIds)
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
                log.Error($"Index update failed. {nameof(groupId)}:{groupId}, {nameof(linearChannelId)}:{linearChannelId}");
            }
            
            if (epgResult?.Length > 0)
            {
                var epgIds = epgResult.Select(x => long.Parse(x.AssetId)).ToList();
                result = ShouldUseUpdateRegionPerformanceImprovement
                    ? CatalogLogic.UpdateEpgRegionsIndex(epgIds, new[] { linearChannelId }, groupId, eAction.Update, null, false)
                    : CatalogLogic.UpdateEpgIndex(epgIds, groupId, eAction.Update, null, false);
                
                if (!result)
                {
                    log.Error($"Index update failed. {nameof(groupId)}:{groupId}, {nameof(epgIds)}:{string.Join(",", epgIds)}");
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
    }
}
