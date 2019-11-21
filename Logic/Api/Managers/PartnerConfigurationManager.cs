using ApiObjects;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using CouchbaseManager;
using DAL;
using KLogMonitor;
using System;
using System.Reflection;

namespace ApiLogic.Api.Managers
{
    public class PartnerConfigurationManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        internal static GenericListResponse<ObjectVirtualAssetPartnerConfig> GetObjectVirtualAssetPartnerConfiguration(int groupId)
        {
            GenericListResponse<ObjectVirtualAssetPartnerConfig> response = new GenericListResponse<ObjectVirtualAssetPartnerConfig>();
            try
            {
                eResultStatus resultStatus;
                var partnerConfig = GetObjectVirtualAssetPartnerConfig(groupId, out resultStatus);
                if(resultStatus == eResultStatus.KEY_NOT_EXIST)
                {
                    response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                    return response;
                }
                if (partnerConfig != null)
                {
                    response.Objects.Add(partnerConfig);
                    response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception exc)
            {
                log.Error($"Error while getting GetObjectVirtualAssetPartnerConfiguration for group {groupId}. exc {exc}");
            }

            return response;
        }

        internal static Status UpdateObjectVirtualAssetPartnerConfiguration(int groupId, ObjectVirtualAssetPartnerConfig partnerConfigToUpdate)
        {
            try
            {
                CatalogGroupCache catalogGroupCache = null;

                if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.Error($"failed to get catalogGroupCache for groupId: {groupId} when calling UpdateObjectVirtualAssetPartnerConfiguration");
                    return new Status((int)eResponseStatus.Error); ;
                }

                // 1. foreach ObjectVirtualAssetInfo check that AssetStructId and MetaUd exists
                foreach (var objectVirtualAsset in partnerConfigToUpdate.ObjectVirtualAssets)
                {
                    if (!catalogGroupCache.AssetStructsMapById.ContainsKey(objectVirtualAsset.AssetStructId))
                    {
                        log.Error($"AssetStructDoesNotExist {objectVirtualAsset.AssetStructId}. groupId: {groupId}");
                        return new Status((int)eResponseStatus.AssetStructDoesNotExist, eResponseStatus.AssetStructDoesNotExist.ToString());
                    }

                    if (!catalogGroupCache.TopicsMapById.ContainsKey(objectVirtualAsset.MetaId))
                    {
                        log.Error($"MetaDoesNotExist {objectVirtualAsset.MetaId}. groupId: {groupId}");
                        return new Status((int)eResponseStatus.MetaDoesNotExist, eResponseStatus.MetaDoesNotExist.ToString());
                    }
                }

                // upsert GeneralPartnerConfig            
                if (!ApiDAL.UpdateObjectVirtualAssetPartnerConfiguration(groupId, partnerConfigToUpdate))
                {
                    log.Error($"Error while update objectVirtualAsset. groupId: {groupId}");
                    return new Status((int)eResponseStatus.Error); ;
                }

                string invalidationKey = LayeredCacheKeys.GetObjectVirtualAssetPartnerConfigInvalidationKey(groupId);
                if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to set invalidation key for ObjectVirtualAssetPartnerConfig with invalidationKey: {0}", invalidationKey);
                }

                return new Status(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"UpdateObjectVirtualAssetPartnerConfiguration failed ex={ex}, groupId={groupId}");
                return new Status(eResponseStatus.Error);

            }
        }

        private static ObjectVirtualAssetPartnerConfig GetObjectVirtualAssetPartnerConfig(int groupId, out eResultStatus resultStatus)
        {
            try
            {
                return ApiDAL.GetObjectVirtualAssetPartnerConfiguration(groupId, out resultStatus);
            }
            catch (Exception)
            {
                log.Error($"Error while getting ObjectVirtualAssetPartnerConfig for group {groupId}");
            }

            resultStatus = eResultStatus.ERROR;
            return null;
        }
    }
}
