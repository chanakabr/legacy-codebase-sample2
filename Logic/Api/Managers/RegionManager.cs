using ApiObjects;
using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;
using DAL;
using CachingProvider.LayeredCache;
using Core.Catalog.CatalogManagement;
using Core.Api;
using KLogMonitor;
using System.Reflection;

namespace ApiLogic.Api.Managers
{
    public class RegionManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static RegionsResponse GetRegions(int groupID, List<string> externalRegionList, RegionOrderBy orderBy)
        {
            RegionsResponse response = null;
            DataSet ds = null;
            try
            {
                if (externalRegionList == null)
                {
                    externalRegionList = new List<string>();

                }

                ds = DAL.ApiDAL.Get_RegionsByExternalRegions(groupID, externalRegionList, orderBy);

                Region region;
                if (ds != null && ds.Tables != null && ds.Tables.Count >= 2)
                {
                    response = new RegionsResponse();
                    response.Regions = new List<Region>();

                    if (ds.Tables[0] != null && ds.Tables[0].Rows != null)
                    {
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            region = new Region()
                            {
                                id = APILogic.Utils.GetIntSafeVal(row, "id"),
                                name = APILogic.Utils.GetSafeStr(row, "name"),
                                externalId = APILogic.Utils.GetSafeStr(row, "external_id"),
                                isDefault = APILogic.Utils.GetIntSafeVal(row, "is_default_region") == 1 ? true : false,
                                parentId = APILogic.Utils.GetIntSafeVal(row, "parent_id")
                            };
                            response.Regions.Add(region);
                        }
                    }
                    if (ds.Tables[1] != null && ds.Tables[1].Rows != null)
                    {
                        int regionId;
                        foreach (DataRow row in ds.Tables[1].Rows)
                        {
                            regionId = APILogic.Utils.GetIntSafeVal(row, "region_id");
                            region = response.Regions.Where(r => r.id == regionId).FirstOrDefault();
                            if (region != null)
                            {
                                region.linearChannels.Add(new ApiObjects.KeyValuePair(APILogic.Utils.GetIntSafeVal(row, "media_id").ToString(), APILogic.Utils.GetIntSafeVal(row, "channel_number").ToString()));
                            }
                        }
                    }

                }
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "OK");
            }
            catch (Exception)
            {
                response = new RegionsResponse();
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
            }
            return response;
        }

        internal static Status DeleteRegion(int groupId, int id, long userId)
        {
            try
            {
                Region region = GetRegion(groupId, id);
                if (region == null)
                {
                    log.ErrorFormat("Region wasn't found. groupId:{0}, id:{1}", groupId, id);
                    return new Status((int)eResponseStatus.RegionNotFound, "Region was not found");
                }

                // TODO: what if the region is a parent??

                if (!ApiDAL.DeleteRegion(groupId, id, userId))
                {
                    log.ErrorFormat("Error while trying to delete region. groupId:{0}, id:{1}", groupId, id);
                    return new Status((int)eResponseStatus.Error); ;
                }

                string invalidationKey = LayeredCacheKeys.GetRegionsKeyInvalidationKey(groupId);
                if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to set invalidation key for region. key = {0}", invalidationKey);
                }
            }
            catch (Exception exc)
            {
                log.ErrorFormat("DeleteRegion Failed. regionId {0}, groupId: {1}. ex: {2}", id, groupId, exc);
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

                if (regionToUpdate.externalId != null && regionToUpdate.externalId != region.externalId)
                {
                    RegionsResponse regions = GetRegions(groupId, new List<string>() { region.externalId }, RegionOrderBy.CreateDateAsc);
                    if (regions != null && regions.Regions != null && regions.Regions.Count > 0)
                    {
                        log.ErrorFormat("Region external ID already exists. groupId:{0}, externalId:{1}", groupId, region.externalId);
                        response.SetStatus((int)eResponseStatus.ExternalIdAlreadyExists, "Region external ID already exists");
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
                        response.SetStatus((int)eResponseStatus.RegionCannotBeParent, "Parent region cannot be parent");
                        return response;
                    }
                }

                if (regionToUpdate.linearChannels?.Count > 0 && !ValidateLinearChannelsExist(groupId, regionToUpdate.linearChannels))
                {
                    log.ErrorFormat("One or more of the assets in linear channel list does not exist. groupId:{0}, id:{1}", groupId, region.id);
                    response.SetStatus(eResponseStatus.Error, "One or more of the assets in linear channel list does not exist");
                    return response;
                }

                if (!ApiDAL.UpdateRegion(groupId, region, userId))
                {
                    log.ErrorFormat("Error while trying to update region. groupId:{0}, id:{1}", groupId, region.id);
                    response.SetStatus(eResponseStatus.Error);
                    return response;
                }

                string invalidationKey = LayeredCacheKeys.GetRegionsKeyInvalidationKey(groupId);
                if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to set invalidation key for region. key = {0}", invalidationKey);
                }

                if (parentRegion != null)
                {
                    regionToUpdate.linearChannels = parentRegion.linearChannels;
                }

                if (regionToUpdate.parentId == 0)
                {
                    List<string> assetsToIndex = null;
                    if (region.parentId > 0)
                    {
                        assetsToIndex = regionToUpdate.linearChannels.Select(lc => lc.key).ToList();
                    }
                    else
                    {
                        assetsToIndex = GetLinearChannelsDiff(region.linearChannels, regionToUpdate.linearChannels);
                    }
                    if (assetsToIndex?.Count > 0)
                    {
                        foreach (var asset in assetsToIndex)
                        {
                            IndexManager.UpsertMedia(groupId, long.Parse(asset));
                            // TODO: index programs
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

        internal static GenericResponse<Region> AddRegion(int groupId, Region region, long userId)
        {
            GenericResponse<Region> response = new GenericResponse<Region>();

            try
            {
                RegionsResponse regions = GetRegions(groupId, new List<string>() { region.externalId }, RegionOrderBy.CreateDateAsc);
                if (regions != null && regions.Regions != null && regions.Regions.Count > 0)
                {
                    log.ErrorFormat("Region external ID already exists. groupId:{0}, externalId:{1}", groupId, region.externalId);
                    response.SetStatus((int)eResponseStatus.ExternalIdAlreadyExists, "Region external ID already exists");
                    return response;
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

                string invalidationKey = LayeredCacheKeys.GetRegionsKeyInvalidationKey(groupId);
                if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to set invalidation key for region. key = {0}", invalidationKey);
                }

                if (parentRegion != null)
                {
                    region.linearChannels = parentRegion.linearChannels;
                }

                if (region.parentId == 0 && region.linearChannels?.Count > 0)
                {
                    var assets = region.linearChannels.Select(lc => lc.key);
                    foreach (var asset in assets)
                    {
                        IndexManager.UpsertMedia(groupId, long.Parse(asset));
                        // TODO: index programs
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
                if (!LayeredCache.Instance.Get(key, ref res, GetLinearMediaRegionsFromDB, new Dictionary<string, object>() { { "groupId", groupId } }, groupId,
                    LayeredCacheConfigNames.GET_LINEAR_MEDIA_REGIONS_NAME_CACHE_CONFIG_NAME, new List<string>() { LayeredCacheKeys.GetLinearMediaRegionsInvalidationKey(groupId) }))
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

        private static Tuple<Dictionary<long, List<int>>, bool> GetLinearMediaRegionsFromDB(Dictionary<string, object> funcParams)
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
                        var dt = ApiDAL.GetMediaRegions(groupId.Value);
                        if (dt != null && dt.Rows != null)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                long linearChannelId = ODBCWrapper.Utils.GetLongSafeVal(row, "MEDIA_ID");
                                int regionId = ODBCWrapper.Utils.GetIntSafeVal(row, "REGION_ID");

                                if (!result.ContainsKey(linearChannelId))
                                {
                                    result.Add(linearChannelId, new List<int>());
                                }

                                result[linearChannelId].Add(regionId);
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
                if (!LayeredCache.Instance.Get<RegionsCache>(key,
                                                          ref regionsCache,
                                                          GetAllRegionsDB,
                                                          new Dictionary<string, object>() { { "groupId", groupId } },
                                                          groupId,
                                                          LayeredCacheKeys.GetRegionsKeyInvalidationKey(groupId)))
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

            if (regionsCache != null && regionsCache.Regions != null)
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
                RegionsCache regionsCache = GetRegionsFromCache(groupId);

                if (regionsCache != null)
                {
                    result.Objects = regionsCache.Regions.Values.ToList();
                    result.TotalItems = regionsCache.Regions.Count;
                    result.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());

                    if (filter.RegionIds?.Count > 0)
                    {
                        result.Objects = regionsCache.Regions.Values.ToList();
                        result.TotalItems = regionsCache.Regions.Count;
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
                            result.SetStatus((int)eResponseStatus.RegionNotFound, "Parent region was not found");
                        }
                    }

                    if (result.Status.Code == (int)eResponseStatus.OK)
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
                                    isDefault = ODBCWrapper.Utils.GetIntSafeVal(row, "is_default_region") == 1 ? true : false,
                                    parentId = ODBCWrapper.Utils.GetIntSafeVal(row, "parent_id"),
                                    createDate = ODBCWrapper.Utils.GetDateSafeVal(row, "create_date")
                                };

                                regionsCache.Regions.Add(region.id, region);
                                regionsCache.ExternalIdsMapping.Add(region.externalId, region.id);

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
                            foreach (DataRow row in ds.Tables[1].Rows)
                            {
                                regionId = APILogic.Utils.GetIntSafeVal(row, "region_id");
                                region = regionsCache.Regions.ContainsKey(regionId) ? regionsCache.Regions[regionId] : null;
                                if (region != null)
                                {
                                    region.linearChannels.Add(new ApiObjects.KeyValuePair(APILogic.Utils.GetIntSafeVal(row, "media_id").ToString(), APILogic.Utils.GetIntSafeVal(row, "channel_number").ToString()));
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

        private static List<string> GetLinearChannelsDiff(List<KeyValuePair> originalLinearChannels, List<KeyValuePair> linearChannels)
        {
            return originalLinearChannels.Select(lc => lc.key).Except(linearChannels.Select(lc => lc.key)).ToList();
        }

        private static bool ValidateLinearChannelsExist(int groupId, List<KeyValuePair> linearChannels)
        {
            bool result = false;

            StringBuilder ksql = new StringBuilder("(or ");
            foreach (var channel in linearChannels)
            {
                ksql.AppendFormat("media_id='{0}' ", channel.key);
            }

            ksql.AppendFormat(")");

            var res = api.SearchAssets(groupId, ksql.ToString(), 0, 0, false, 0, false, string.Empty, string.Empty, string.Empty, 0, 0, true, true);
            if (res?.Length == linearChannels.Count)
            {
                result = true;
            }

            return result;
        }
    }
}
