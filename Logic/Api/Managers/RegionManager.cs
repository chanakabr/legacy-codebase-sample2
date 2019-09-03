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
                //TODO: get region by ID
                Region region = null; // GetRegion(groupId, id);
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

                //TODO: add invalidation key
                string invalidationKey = null; //LayeredCacheKeys.GetRegionsInvalidationKey(groupId);
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
                //TODO: get region by ID
                Region region = null; // GetRegion(groupId, regionToUpdate.id);
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
                if (regionToUpdate.parentRegionId != 0 && regionToUpdate.parentRegionId != region.parentRegionId)
                {
                    //TODO: get region by ID
                    //parentRegion = GetRegion(groupId, regionToUpdate.parentRegionId);
                    if (parentRegion == null)
                    {
                        log.ErrorFormat("Parent region wasn't found. groupId:{0}, id:{1}", groupId, regionToUpdate.parentRegionId);
                        response.SetStatus((int)eResponseStatus.RegionNotFound, "Parent region was not found");
                        return response;
                    }

                    if (parentRegion.parentRegionId != 0)
                    {
                        log.ErrorFormat("Parent region cannot be parent. groupId:{0}, id:{1}", groupId, regionToUpdate.parentRegionId);
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

                //TODO: add invalidation key
                string invalidationKey = null; //LayeredCacheKeys.GetRegionsInvalidationKey(groupId);
                if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to set invalidation key for region. key = {0}", invalidationKey);
                }

                if (parentRegion != null)
                {
                    regionToUpdate.linearChannels = parentRegion.linearChannels;
                }

                if (regionToUpdate.parentRegionId == 0)
                {
                    List<string> assetsToIndex = null;
                    if (region.parentRegionId > 0)
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
                if (region.parentRegionId != 0)
                {
                    //TODO: get region by ID
                    //parentRegion = GetRegion(groupId, regionToUpdate.parentRegionId);
                    if (parentRegion == null)
                    {
                        log.ErrorFormat("Parent region wasn't found. groupId:{0}, id:{1}", groupId, region.parentRegionId);
                        response.SetStatus((int)eResponseStatus.RegionNotFound, "Parent region was not found");
                        return response;
                    }

                    if (parentRegion.parentRegionId != 0)
                    {
                        log.ErrorFormat("Parent region cannot be parent. groupId:{0}, id:{1}", groupId, region.parentRegionId);
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

                //TODO: add invalidation key
                string invalidationKey = null; //LayeredCacheKeys.GetRegionsInvalidationKey(groupId);
                if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to set invalidation key for region. key = {0}", invalidationKey);
                }

                if (parentRegion != null)
                {
                    region.linearChannels = parentRegion.linearChannels;
                }

                if (region.parentRegionId == 0 && region.linearChannels?.Count > 0)
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
