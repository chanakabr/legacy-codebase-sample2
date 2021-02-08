using ApiObjects;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using CouchbaseManager;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace ApiLogic.Api.Managers
{
    public interface IVirtualAssetPartnerConfigManager
    {
        long GetAssetStructByObjectVirtualAssetInfo(int groupId, ObjectVirtualAssetInfoType objectVirtualAssetInfoType, string type);
        GenericListResponse<ObjectVirtualAssetPartnerConfig> GetObjectVirtualAssetPartnerConfiguration(int groupId);
    }

    public class VirtualAssetPartnerConfigManager : IVirtualAssetPartnerConfigManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<VirtualAssetPartnerConfigManager> lazy = new Lazy<VirtualAssetPartnerConfigManager>(() =>
            new VirtualAssetPartnerConfigManager(ApiDAL.Instance,
                                                 LayeredCache.Instance,
                                                 CatalogManager.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        public static VirtualAssetPartnerConfigManager Instance { get { return lazy.Value; } }

        private readonly IVirtualAssetPartnerConfigRepository _repository;
        private readonly ILayeredCache _layeredCache;
        private readonly ICatalogManager _catalogManager;

        public VirtualAssetPartnerConfigManager(IVirtualAssetPartnerConfigRepository repository,
                                                ILayeredCache layeredCache,
                                                ICatalogManager catalogManager)
        {
            _repository = repository;
            _layeredCache = layeredCache;
            _catalogManager = catalogManager;
        }

        public long GetAssetStructByObjectVirtualAssetInfo(int groupId, ObjectVirtualAssetInfoType objectVirtualAssetInfoType, string type)
        {
            ObjectVirtualAssetInfo objectVirtualAssetInfo = GetObjectVirtualAssetInfo(groupId, objectVirtualAssetInfoType);

            if (objectVirtualAssetInfo == null)
            {
                log.Debug($"No objectVirtualAssetInfo for groupId {groupId}. virtualAssetInfo.Type {objectVirtualAssetInfoType}. type {type}");
                return 0;
            }

            if (objectVirtualAssetInfo.ExtendedTypes?.Count > 0 && objectVirtualAssetInfo.ExtendedTypes.ContainsKey(type))
            {
                return objectVirtualAssetInfo.ExtendedTypes[type];
            }

            log.Debug($"No type at objectVirtualAssetInfo for groupId {groupId}. virtualAssetInfo.Type {objectVirtualAssetInfoType}. type {type}");
            return 0;
        }

        public ObjectVirtualAssetInfo GetObjectVirtualAssetInfo(int groupId, ObjectVirtualAssetInfoType objectVirtualAssetInfoType)
        {
            ObjectVirtualAssetInfo objectVirtualAssetInfo = null;

            var objectVirtualAssetPartnerConfig = GetObjectVirtualAssetPartnerConfiguration(groupId);
            if (objectVirtualAssetPartnerConfig.HasObjects())
            {
                objectVirtualAssetInfo = objectVirtualAssetPartnerConfig.Objects[0].ObjectVirtualAssets.FirstOrDefault(x => x.Type == objectVirtualAssetInfoType);
            }

            return objectVirtualAssetInfo;
        }

        private ObjectVirtualAssetPartnerConfig GetObjectVirtualAssetPartnerConfig(int groupId, out eResultStatus resultStatus)
        {
            resultStatus = eResultStatus.ERROR;
            ObjectVirtualAssetPartnerConfig partnerConfig = null;

            try
            {
                string key = LayeredCacheKeys.GetObjectVirtualAssetPartnerConfig(groupId);
                List<string> configInvalidationKey = new List<string>() { LayeredCacheKeys.GetObjectVirtualAssetPartnerConfigInvalidationKey(groupId) };
                if (!_layeredCache.Get<ObjectVirtualAssetPartnerConfig>(key,
                                                          ref partnerConfig,
                                                          GetObjectVirtualAssetPartnerConfigDB,
                                                          new Dictionary<string, object>() { { "groupId", groupId } },
                                                          groupId,
                                                          LayeredCacheConfigNames.GET_OBJECT_VIRTUAL_ASSET_PARTNER_CONFIG,
                                                          configInvalidationKey))
                {
                    log.ErrorFormat("Failed getting ObjectVirtualAssetPartnerConfig from LayeredCache, groupId: {0}, key: {1}", groupId, key);
                }
                else
                {
                    if (partnerConfig == null)
                    {
                        resultStatus = eResultStatus.KEY_NOT_EXIST;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed GetObjectVirtualAssetPartnerConfig for groupId: {groupId}", ex);
            }

            return partnerConfig;
        }

        private Tuple<ObjectVirtualAssetPartnerConfig, bool> GetObjectVirtualAssetPartnerConfigDB(Dictionary<string, object> funcParams)
        {
            ObjectVirtualAssetPartnerConfig generalPartnerConfig = null;
            eResultStatus resultStatus = eResultStatus.ERROR;

            try
            {
                int? groupId = funcParams["groupId"] as int?;
                if (groupId.HasValue)
                {
                    generalPartnerConfig = _repository.GetObjectVirtualAssetPartnerConfiguration(groupId.Value, out resultStatus);

                    if (resultStatus == eResultStatus.KEY_NOT_EXIST)
                    {
                        // save null document
                        if (!_repository.UpdateObjectVirtualAssetPartnerConfiguration(groupId.Value, new ObjectVirtualAssetPartnerConfig()))
                        {
                            log.Error($"failed to save ObjectVirtualAssetPartnerConfig null document for groupId {groupId.Value}");
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error($"GetObjectVirtualAssetPartnerConfigDB failed, parameters : {string.Join(";", funcParams.Keys)}, ex : {ex}");
            }
            return new Tuple<ObjectVirtualAssetPartnerConfig, bool>(generalPartnerConfig, resultStatus != eResultStatus.ERROR);
        }

        public GenericListResponse<ObjectVirtualAssetPartnerConfig> GetObjectVirtualAssetPartnerConfiguration(int groupId)
        {
            GenericListResponse<ObjectVirtualAssetPartnerConfig> response = new GenericListResponse<ObjectVirtualAssetPartnerConfig>();
            try
            {
                eResultStatus resultStatus;
                var partnerConfig = GetObjectVirtualAssetPartnerConfig(groupId, out resultStatus);
                if (resultStatus == eResultStatus.KEY_NOT_EXIST)
                {
                    response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                    return response;
                }

                if (partnerConfig != null)
                {
                    response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                    if (partnerConfig.ObjectVirtualAssets?.Count > 0)
                    {
                        response.Objects.Add(partnerConfig);
                    }
                }
            }
            catch (Exception exc)
            {
                log.Error($"Error while getting GetObjectVirtualAssetPartnerConfiguration for group {groupId}. exc {exc}");
            }

            return response;
        }

        public Status UpdateObjectVirtualAssetPartnerConfiguration(int groupId, ObjectVirtualAssetPartnerConfig partnerConfigToUpdate)
        {
            try
            {
                CatalogGroupCache catalogGroupCache = null;
                if (!_catalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.Error($"failed to get catalogGroupCache for groupId: {groupId} when calling UpdateObjectVirtualAssetPartnerConfiguration");
                    return new Status((int)eResponseStatus.Error); ;
                }

                // 1. foreach ObjectVirtualAssetInfo check that AssetStructId and MetaId exists
                foreach (var objectVirtualAsset in partnerConfigToUpdate.ObjectVirtualAssets)
                {
                    if (!catalogGroupCache.AssetStructsMapById.ContainsKey(objectVirtualAsset.AssetStructId))
                    {
                        log.Error($"AssetStructDoesNotExist {objectVirtualAsset.AssetStructId}. groupId: {groupId}");
                        return new Status((int)eResponseStatus.AssetStructDoesNotExist, $"AssetStruct {objectVirtualAsset.AssetStructId} does not exist");
                    }

                    if (!catalogGroupCache.TopicsMapById.ContainsKey(objectVirtualAsset.MetaId))
                    {
                        log.Error($"MetaDoesNotExist {objectVirtualAsset.MetaId}. groupId: {groupId}");
                        return new Status((int)eResponseStatus.MetaDoesNotExist, $"Meta id {objectVirtualAsset.MetaId} does not exist");
                    }

                    //check asset struct at mapping current
                    var currentOVA = GetObjectVirtualAssetInfo(groupId, objectVirtualAsset.Type);

                    if (currentOVA != null && currentOVA.ExtendedTypes?.Count > 0 && objectVirtualAsset.ExtendedTypes != null && objectVirtualAsset.ExtendedTypes.Count > 0)
                    {
                        // value of type should not change
                        foreach (var type in currentOVA.ExtendedTypes.Keys)
                        {
                            if (objectVirtualAsset.ExtendedTypes.ContainsKey(type) && objectVirtualAsset.ExtendedTypes[type] != currentOVA.ExtendedTypes[type])
                            {
                                log.Error($"Extended type {type} value {objectVirtualAsset.ExtendedTypes[type]} cannot be changed. groupId: {groupId}");
                                return new Status((int)eResponseStatus.ExtendedTypeValueCannotBeChanged, $"Extended type '{type}' value: {objectVirtualAsset.ExtendedTypes[type]} cannot be changed.");
                            }
                        }
                    }

                    if (objectVirtualAsset.ExtendedTypes != null && objectVirtualAsset.ExtendedTypes.Count > 0)
                    {
                        foreach (var assetStructId in objectVirtualAsset.ExtendedTypes.Values)
                        {
                            if (!catalogGroupCache.AssetStructsMapById.ContainsKey(assetStructId))
                            {
                                log.Error($"AssetStructDoesNotExist {assetStructId}. groupId: {groupId}");
                                return new Status((int)eResponseStatus.AssetStructDoesNotExist, $"AssetStruct id :{assetStructId} does not exist.");
                            }
                        }
                    }
                }

                // upsert GeneralPartnerConfig            
                if (!_repository.UpdateObjectVirtualAssetPartnerConfiguration(groupId, partnerConfigToUpdate))
                {
                    log.Error($"Error while update objectVirtualAsset. groupId: {groupId}");
                    return new Status((int)eResponseStatus.Error); ;
                }

                string invalidationKey = LayeredCacheKeys.GetObjectVirtualAssetPartnerConfigInvalidationKey(groupId);
                if (!_layeredCache.SetInvalidationKey(invalidationKey))
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
    }
}