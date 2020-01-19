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

namespace ApiLogic.Api.Managers
{
    public class RegionManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

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

                // TODO: what if the region is a parent??

                // check if region in use
                if (DomainDal.IsRegionInUse(groupId, id))
                {
                    log.Error($"Region in use cannot be deleted. groupId:{groupId}, id:{id}");
                    return new Status((int)eResponseStatus.CannotDeleteRegionInUse, "Region in use cannot be deleted");
                }

                if (!ApiDAL.DeleteRegion(groupId, id, userId))
                {
                    log.Error($"Error while trying to delete region. groupId:{groupId}, id:{id}");
                    return new Status((int)eResponseStatus.Error); ;
                }

                InvalidateRegions(groupId);
            }
            catch (Exception exc)
            {
                log.Error($"DeleteRegion Failed. regionId {id}, groupId: {groupId}. ex: {exc}");
                return new Status((int)eResponseStatus.Error); ;
            }

            return new Status((int)eResponseStatus.OK);
        }

        internal static GenericResponse<Region> UpdateRegion(int groupId, Region regionToUpdate, long userId)
        {
            GenericResponse<Region> response = new GenericResponse<Region>();

            try
            {
                Region region = GetRegion(groupId, regionToUpdate.id);
                if (region == null)
                {
                    log.ErrorFormat("Region wasn't found. groupId:{0}, id:{1}", groupId, regionToUpdate.id);
                    response.SetStatus((int)eResponseStatus.RegionNotFound, "Region was not found");
                    return response;
                }

                if (!string.IsNullOrEmpty(regionToUpdate.externalId) && regionToUpdate.externalId != region.externalId)
                {
                    RegionFilter filter = new RegionFilter() { ExternalIds = new List<string>() { regionToUpdate.externalId } };
                    var regions = GetRegions(groupId, filter);
                    if (regions != null && regions.HasObjects())
                    {
                        log.ErrorFormat("Region external ID already exists. groupId:{0}, externalId:{1}", groupId, region.externalId);
                        response.SetStatus((int)eResponseStatus.ExternalIdAlreadyExists, "Region external ID already exists");
                        return response;
                    }
                }

                if (regionToUpdate.parentId > 0 && region.parentId == 0)
                {
                    RegionFilter filterParent = new RegionFilter() { ParentId = region.id };
                    var subRegions = GetRegions(groupId, filterParent);
                    if (subRegions != null && subRegions.HasObjects())
                    {
                        log.ErrorFormat("Sub region cannot be parent region. groupId:{0}, regionId:{1}", groupId, region.id);
                        response.SetStatus((int)eResponseStatus.RegionCannotBeParent, "Sub region cannot be parent");
                        return response;
                    }
                }


                Region parentRegion = null;
                if (regionToUpdate.parentId != 0 && regionToUpdate.parentId != region.parentId)
                {
                    parentRegion = GetRegion(groupId, regionToUpdate.parentId);
                    if (parentRegion == null)
                    {
                        log.ErrorFormat("Parent region wasn't found. groupId:{0}, id:{1}", groupId, regionToUpdate.parentId);
                        response.SetStatus((int)eResponseStatus.RegionNotFound, "Parent region was not found");
                        return response;
                    }

                    if (parentRegion.parentId != 0)
                    {
                        log.ErrorFormat("Parent region cannot be parent. groupId:{0}, id:{1}", groupId, regionToUpdate.parentId);
                        response.SetStatus((int)eResponseStatus.RegionCannotBeParent, "Parent region cannot be sub region");
                        return response;
                    }
                }

                if (regionToUpdate.parentId == 0 && region.parentId > 0)
                {
                    RegionFilter filter = new RegionFilter() { ParentId = region.parentId };
                    var regions = GetRegions(groupId, filter);
                    if (regions != null && regions.HasObjects())
                    {
                        //log.ErrorFormat("");
                        response.SetStatus((int)eResponseStatus.Error, "Cannot set region to sub region");
                        return response;
                    }
                }

                if (regionToUpdate.linearChannels?.Count > 0 && !ValidateLinearChannelsExist(groupId, regionToUpdate.linearChannels))
                {
                    log.ErrorFormat("One or more of the assets in linear channel list does not exist. groupId:{0}, id:{1}", groupId, region.id);
                    response.SetStatus(eResponseStatus.Error, "One or more of the assets in linear channel list does not exist");
                    return response;
                }

                if (!ApiDAL.UpdateRegion(groupId, regionToUpdate, userId))
                {
                    log.ErrorFormat("Error while trying to update region. groupId:{0}, id:{1}", groupId, region.id);
                    response.SetStatus(eResponseStatus.Error);
                    return response;
                }

                InvalidateRegions(groupId);

                if (parentRegion != null)
                {
                    regionToUpdate.linearChannels = parentRegion.linearChannels;
                }

                if (regionToUpdate.parentId == 0)
                {
                    List<long> assetsToIndex = null;
                    if (region.parentId > 0)
                    {
                        assetsToIndex = regionToUpdate.linearChannels.Select(lc => long.Parse(lc.key)).ToList();
                    }
                    else
                    {
                        assetsToIndex = GetLinearChannelsDiff(region.linearChannels, regionToUpdate.linearChannels);
                    }
                    if (assetsToIndex?.Count > 0)
                    {
                        CatalogLogic.UpdateIndex(assetsToIndex, groupId, eAction.Update);
                        foreach (var asset in assetsToIndex)
                        {
                            UpdateProgramsRegions(groupId, asset);
                        }
                    }
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
            GenericResponse<Region> response = new GenericResponse<Region>();

            try
            {
                if (!string.IsNullOrEmpty(region.externalId))
                {
                    RegionFilter filter = new RegionFilter() { ExternalIds = new List<string>() { region.externalId } };
                    var regions = GetRegions(groupId, filter);
                    if (regions != null && regions.HasObjects())
                    {
                        log.ErrorFormat("Region external ID already exists. groupId:{0}, externalId:{1}", groupId, region.externalId);
                        response.SetStatus((int)eResponseStatus.ExternalIdAlreadyExists, "Region external ID already exists");
                        return response;
                    }
                }

                Region parentRegion = null;
                if (region.parentId != 0)
                {
                    parentRegion = GetRegion(groupId, region.parentId);
                    if (parentRegion == null)
                    {
                        log.ErrorFormat("Parent region wasn't found. groupId:{0}, id:{1}", groupId, region.parentId);
                        response.SetStatus((int)eResponseStatus.RegionNotFound, "Parent region was not found");
                        return response;
                    }

                    if (parentRegion.parentId != 0)
                    {
                        log.ErrorFormat("Parent region cannot be parent. groupId:{0}, id:{1}", groupId, region.parentId);
                        response.SetStatus((int)eResponseStatus.RegionCannotBeParent, "Parent region cannot be parent");
                        return response;

                    }
                }

                if (region.linearChannels?.Count > 0 && !ValidateLinearChannelsExist(groupId, region.linearChannels))
                {
                    log.ErrorFormat("One or more of the assets in linear channel list does not exist. groupId:{0}", groupId);
                    response.SetStatus(eResponseStatus.Error, "One or more of the assets in linear channel list does not exist");
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

                if (parentRegion != null)
                {
                    region.linearChannels = parentRegion.linearChannels;
                }

                if (region.parentId == 0 && region.linearChannels?.Count > 0)
                {
                    var assets = region.linearChannels.Select(lc => long.Parse(lc.key)).ToList();
                    CatalogLogic.UpdateIndex(assets, groupId, eAction.Update);

                    foreach (var asset in assets)
                    {
                        UpdateProgramsRegions(groupId, asset);
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
                            var parents = regions.Objects.Where(x => x.parentId == 0).ToList();
                            if (parents?.Count > 0)
                            {
                                foreach (var region in parents)
                                {
                                    if (region.linearChannels?.Count > 0)
                                    {
                                        foreach (var kvp in region.linearChannels)
                                        {
                                            int mediaId = 0;
                                            if (int.TryParse(kvp.key, out mediaId) && mediaId > 0)
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

        internal static GenericListResponse<Region> GetRegions(int groupId, RegionFilter filter)
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
                    
                    if (filter.RegionIds?.Count == 0)
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
                                    region.linearChannels.Add(new ApiObjects.KeyValuePair(assetId.ToString(), APILogic.Utils.GetIntSafeVal(row, "channel_number").ToString()));
                                }
                            }

                            foreach (var key in regionsCache.ParentIdsToRegionIdsMapping.Keys)
                            {
                                Region parent = regionsCache.Regions[key];
                                foreach (var item in regionsCache.ParentIdsToRegionIdsMapping[key])
                                {
                                    regionsCache.Regions[item].linearChannels = parent.linearChannels;
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

        private static List<long> GetLinearChannelsDiff(List<KeyValuePair> originalLinearChannels, List<KeyValuePair> linearChannels)
        {
            Dictionary<long, int> linearChannelsDic = new Dictionary<long, int>();
            foreach (var originalLinearChannel in originalLinearChannels)
            {
                linearChannelsDic.Add(long.Parse(originalLinearChannel.key), 1);
            }

            foreach (var linearChannel in linearChannels)
            {
                var mediaId = long.Parse(linearChannel.key);

                if (linearChannelsDic.ContainsKey(mediaId))
                {
                    linearChannelsDic[mediaId] = 2;
                }
                else
                {
                    linearChannelsDic.Add(mediaId, 1);
                }
            }

            return linearChannelsDic.Where(x => x.Value == 1).Select(y => y.Key).ToList();
        }

        private static bool ValidateLinearChannelsExist(int groupId, List<KeyValuePair> linearChannels)
        {
            bool result = false;
            try
            {
                List<KeyValuePair<eAssetTypes, long>> assets = linearChannels.Select(x => new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, long.Parse(x.key))).ToList();
                var allAssets = AssetManager.GetAssets(groupId, assets, true);
                result = allAssets?.Count == linearChannels.Count;
            }
            catch (Exception ex)
            {

            }

            return result;
        }

        private static bool UpdateProgramsRegions(int groupId, long linearChannel)
        {
            bool result = false;

            string ksql = string.Format("(and linear_media_id='{0}')", linearChannel);

            var res = api.SearchAssets(groupId, ksql, 0, 0, true, 0, false, string.Empty, string.Empty, string.Empty, 0, 0, true, true);
            if (res?.Length > 0)
            {
                result = CatalogLogic.UpdateEpgIndex(res.Select(x => long.Parse(x.AssetId)).ToList(), groupId, eAction.Update);
            }

            return result;
        }

        internal static GenericListResponse<Region> GetDefaultRegion(int groupId)
        {
            GenericListResponse<Region> result = new GenericListResponse<Region>();

            try
            {
                var generalPartnerConfig = PartnerConfigurationManager.GetGeneralPartnerConfiguration(groupId);
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
    }
}
