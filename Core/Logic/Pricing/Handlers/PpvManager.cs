using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Pricing;
using ApiObjects.Pricing.Dto;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Api;
using Core.Api.Managers;
using Core.Catalog.CatalogManagement;
using Core.Pricing;
using DAL;
using Phx.Lib.Log;

namespace ApiLogic.Pricing.Handlers
{
    public class PpvManager : IPpvManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        
        private static readonly Lazy<PpvManager> lazy = new Lazy<PpvManager>(
            () => new PpvManager(
                PricingDAL.Instance,
                LayeredCache.Instance,
                PriceDetailsManager.Instance,
                DiscountDetailsManager.Instance,
                UsageModuleManager.Instance,
                Core.Pricing.Module.Instance,
                api.Instance,
                Core.Catalog.CatalogManagement.FileManager.Instance,
                AssetUserRuleManager.Instance),
            LazyThreadSafetyMode.PublicationOnly);
        public static PpvManager Instance => lazy.Value;
        private readonly IPriceDetailsManager _priceDetailsManager;
        private readonly IDiscountDetailsManager _discountDetailsManager;
        private readonly IPpvManagerRepository _repository;
        private readonly ILayeredCache _layeredCache;
        private readonly IPricingModule _pricingModule;
        private readonly IUsageModuleManager _usageModuleManager;
        private readonly IVirtualAssetManager _virtualAssetManager;
        private readonly IMediaFileTypeManager _fileManager;
        private readonly IAssetUserRuleManager _assetUserRuleManager;


        public PpvManager(
            IPpvManagerRepository ppvManagerRepository,
            ILayeredCache layeredCache,
            IPriceDetailsManager priceDetailsManager,
            IDiscountDetailsManager discountDetailsManager,
            IUsageModuleManager usageModuleManager,
            IPricingModule pricingModule,
            IVirtualAssetManager virtualAssetManager,
            IMediaFileTypeManager fileManager,
            IAssetUserRuleManager assetUserRuleManager)
        {
            _repository = ppvManagerRepository;
            _layeredCache = layeredCache;
            _priceDetailsManager = priceDetailsManager;
            _discountDetailsManager = discountDetailsManager;
            _usageModuleManager = usageModuleManager;
            _pricingModule = pricingModule;
            _virtualAssetManager = virtualAssetManager;
            _fileManager = fileManager;
            _assetUserRuleManager = assetUserRuleManager;
        }

        public GenericResponse<PpvModuleInternal> Update(int id, ContextData contextData,
            PpvModuleInternal ppvModuleToUpdate)
        {
            var response = new GenericResponse<PpvModuleInternal>();
            VirtualAssetInfo virtualAssetInfo = null;
            var oldPpvResponse = GetPpvById(contextData, id, true);
            if (!oldPpvResponse.HasObject())
            {
                response.SetStatus(oldPpvResponse.Status);
                return response;
            }
            
            Status validate = Validate(contextData.GroupId, ppvModuleToUpdate.PriceId, ppvModuleToUpdate.UsageModuleId, 
                ppvModuleToUpdate.DiscountId, ppvModuleToUpdate.CouponsGroupId, ppvModuleToUpdate.RelatedFileTypes);
            if (!validate.IsOkStatusCode())
            {
                response.SetStatus(validate);
                return response;
            }
            
            var oldPpvModule = oldPpvResponse.Object;
            if (!string.IsNullOrEmpty(ppvModuleToUpdate.Name))
            { 
                virtualAssetInfo = new VirtualAssetInfo()
                {
                    Type = ObjectVirtualAssetInfoType.Tvod,
                    Id = id,
                    Name = ppvModuleToUpdate.Name,
                    UserId = contextData.UserId.Value
                };
            }

            bool shouldUpdateFileTypes = false;
            if (ppvModuleToUpdate.ShouldUpdateFileTypes(oldPpvModule)) 
            {
                shouldUpdateFileTypes = true;
            }
            else
            {
                ppvModuleToUpdate.RelatedFileTypes = oldPpvModule.m_relatedFileTypes;
            }
            
            bool shouldUpdateDescription = false;
            if (ppvModuleToUpdate.ShouldUpdateDescription(oldPpvModule))
            {
                shouldUpdateDescription = true;
            }
            else
            {
                ppvModuleToUpdate.Description = oldPpvModule.m_sDescription;
            }
            bool shouldUpdate = ppvModuleToUpdate.ShouldUpdate(oldPpvModule);

            if (ppvModuleToUpdate.SubscriptionOnly.HasValue && !ppvModuleToUpdate.SubscriptionOnly.Value)
            {
                if (!ppvModuleToUpdate.PriceId.HasValue || ppvModuleToUpdate.PriceId.Value == 0 || !ppvModuleToUpdate.UsageModuleId.HasValue || ppvModuleToUpdate.UsageModuleId.Value == 0)
                {
                    response.SetStatus(eResponseStatus.Error, "cannot have subscription only false without PriceId and UsageModuleId");
                    return response;
                }
            }
            ppvModuleToUpdate.Id = id;
            
            if (shouldUpdate || shouldUpdateFileTypes || shouldUpdateDescription)
            {
                // Due to atomic action update virtual asset before ppv update
                if (virtualAssetInfo != null)
                {
                    var virtualAssetInfoResponse = _virtualAssetManager.UpdateVirtualAsset(contextData.GroupId, virtualAssetInfo);
                    if (virtualAssetInfoResponse.Status == VirtualAssetInfoStatus.Error)
                    {
                        log.Error($"Error while update ppv's virtualAsset. groupId: {contextData.GroupId}, ppvId: {id}, Name: {ppvModuleToUpdate.Name} ");
                        if (virtualAssetInfoResponse.ResponseStatus != null)
                        {
                            response.SetStatus(virtualAssetInfoResponse.ResponseStatus);
                        }
                        else
                        {
                            response.SetStatus(eResponseStatus.Error, "Error while updating ppv.");
                        }
                        return response;
                    }
                    ppvModuleToUpdate.VirtualAssetId = virtualAssetInfoResponse.AssetId;
                }

                PpvDTO ppvDto = convertToDto(ppvModuleToUpdate);
                int updatedRow = 0;
                if (shouldUpdate)
                {
                    updatedRow += _repository.UpdatePPV(contextData.GroupId, contextData.UserId.Value, id, 
                        ppvDto);
                }
                if (shouldUpdateFileTypes)
                {
                    updatedRow += _repository.UpdatePPVFileTypes(contextData.GroupId, id, 
                        ppvDto.FileTypesIds);
                }
                if (shouldUpdateDescription)
                {
                    updatedRow += _repository.UpdatePPVDescriptions(contextData.GroupId, contextData.UserId.Value, id, 
                        ppvDto.Descriptions);
                }

                if (updatedRow > 0)
                {
                    SetPpvInvalidation(contextData.GroupId, id);
                    response.Object = ppvModuleToUpdate;
                    response.SetStatus(eResponseStatus.OK);
                }
            }
            else
            {
                response.Object = ppvModuleToUpdate;
                response.SetStatus(eResponseStatus.OK);
            }
            return response;
        }

        public GenericResponse<PpvModuleInternal> Add(ContextData contextData, PpvModuleInternal ppvToInsert)
        {
            var response = new GenericResponse<PpvModuleInternal>();
            Status validate = Validate(contextData.GroupId, ppvToInsert.PriceId, ppvToInsert.UsageModuleId, 
                ppvToInsert.DiscountId, ppvToInsert.CouponsGroupId, ppvToInsert.RelatedFileTypes);
            if (!validate.IsOkStatusCode())
            {
                response.SetStatus(validate);
                return response;
            }

            bool isShopUser = false;
            if (ppvToInsert.AssetUserRuleId > 0)
            {
                var assetRuleResponse = _assetUserRuleManager.GetAssetUserRuleByRuleId(contextData.GroupId, ppvToInsert.AssetUserRuleId.Value);
                if (!assetRuleResponse.IsOkStatusCode())
                {
                    response.SetStatus(assetRuleResponse.Status);
                    return response;
                }
            }

            var shopId = _assetUserRuleManager.GetShopAssetUserRuleId(contextData.GroupId, contextData.UserId);
            if (shopId > 0)
            {
                isShopUser = true;
                ppvToInsert.AssetUserRuleId = shopId;
            }

            ppvToInsert.CreateDate = ppvToInsert.UpdateDate = DateTime.UtcNow;
            int id = _repository.InsertPPV(contextData.GroupId, contextData.UserId.Value, convertToDto(ppvToInsert));
            if (id == 0)
            {
                log.Error($"Error while InsertPPV. contextData: {contextData}.");
                return response;
            }

            // Add VirtualAssetInfo for new ppv 
            var virtualAssetInfo = new VirtualAssetInfo()
            {
                Type = ObjectVirtualAssetInfoType.Tvod,
                Id = id,
                Name = ppvToInsert.Name,
                UserId = contextData.UserId.Value
            };

            if (!isShopUser && ppvToInsert.AssetUserRuleId > 0)
            {
                virtualAssetInfo.AssetUserRuleId = ppvToInsert.AssetUserRuleId;
            }

            var virtualAssetInfoResponse = _virtualAssetManager.AddVirtualAsset(contextData.GroupId, virtualAssetInfo);
            if (virtualAssetInfoResponse.Status == VirtualAssetInfoStatus.Error)
            {
                log.Error($"Error while AddVirtualAsset - ppv: {id} will delete ");
                Delete(contextData, id);
                return response;
            }

            if (virtualAssetInfoResponse.Status == VirtualAssetInfoStatus.OK && virtualAssetInfoResponse.AssetId > 0)
            {
                ppvToInsert.VirtualAssetId = virtualAssetInfoResponse.AssetId;
                _repository.UpdatePpvVirtualAssetId(contextData.GroupId, id, virtualAssetInfoResponse.AssetId, contextData.UserId.Value);
            }

            ppvToInsert.Id = id;
            response.Object = ppvToInsert;
            SetPpvInvalidation(contextData.GroupId);
            response.SetStatus(Status.Ok);
            return response;
        }
        
        public Status Delete(ContextData contextData, long id)
        {
            Status result = new Status();
            var oldPpvResponse = GetPpvById(contextData, id, true);
            if (!oldPpvResponse.HasObject())
            {
                result.Set(oldPpvResponse.Status);
                return result;
            }

            if (oldPpvResponse.Object.VirtualAssetId.HasValue)
            {
                // Due to atomic action delete virtual asset before ppv delete
                var virtualAssetInfo = new VirtualAssetInfo()
                {
                    Type = ObjectVirtualAssetInfoType.Tvod,
                    Id = id,
                    UserId = contextData.UserId.Value
                };

                var response = _virtualAssetManager.DeleteVirtualAsset(contextData.GroupId, virtualAssetInfo);
            
                if (response.Status == VirtualAssetInfoStatus.Error)
                {
                    log.Error($"Error while delete ppv virtual asset id {virtualAssetInfo}");
                    result.Set(eResponseStatus.Error);
                    return result;
                }
            }

            if (!_repository.DeletePPV(contextData.GroupId, contextData.UserId.Value, id))
            {
                log.Error($"Error while DeleteUsageModule. contextData: {contextData.ToString()}.");
                result.Set(eResponseStatus.Error);
                return result;
            }
            SetPpvInvalidation(contextData.GroupId, (int)id);
            result.Set(eResponseStatus.OK);
            return result;
        }
        
        public GenericResponse<PPVModule> GetPpvById(ContextData contextData, long ppvModuleId, bool alsoInactive = false)
        {
            var response = new GenericResponse<PPVModule>();
            var ppvResponse = GetPPVModules(contextData, new List<long> { ppvModuleId }, alsoInactive: alsoInactive);

            if (!ppvResponse.HasObjects())
            {
                response.SetStatus(eResponseStatus.PpvModuleNotExist, $"PpvModule Code {ppvModuleId} does not exist");
                return response;
            }
            response.Object = ppvResponse.Objects[0];
            response.SetStatus(eResponseStatus.OK);

            return response;
        }
        
        public GenericListResponse<PPVModule> GetPPVModules(ContextData contextData, List<long> ppvModuleIds = null, 
            bool shouldShrink = false, int? couponGroupIdEqual = null, bool alsoInactive = false, PPVOrderBy orderBy = PPVOrderBy.NameAsc,
            int pageIndex = 0, int pageSize = 30, bool shouldIgnorePaging = true, List<long> assetUserRuleIds = null, string nameContains = null)
        {
            var response = new GenericListResponse<PPVModule>();

            int groupId = contextData.GroupId;

            List<PpvDTO> allPpvs = new List<PpvDTO>();
            string allPpvsKey = LayeredCacheKeys.GetAllPpvsKey(groupId);
            if (!_layeredCache.Get(allPpvsKey,
                ref allPpvs,
                GetAllPpvs,
                new Dictionary<string, object>()
                {
                    {"groupId", groupId},
                    {"shouldShrink", shouldShrink}
                },
                groupId,
                LayeredCacheConfigNames.PPV_MODULES_CACHE_CONFIG_NAME,
                new List<string>() {LayeredCacheKeys.GetPpvGroupInvalidationKey(groupId)}))
            {
                log.Error($"Failed to GetPPVModules list. GetPPVModules from layeredCache for groupId:{groupId}.");
                return response;
            }

            var tempPpvs = allPpvs.AsEnumerable();
            if (ppvModuleIds != null && ppvModuleIds.Count > 0)
            {
                tempPpvs = tempPpvs.Where(p => ppvModuleIds.Contains(p.Id));
            }

            if (!alsoInactive)
            {
                tempPpvs = tempPpvs.Where(p => p.IsActive);
            }

            if (!string.IsNullOrEmpty(nameContains))
            {
                tempPpvs = tempPpvs.Where(p => !string.IsNullOrEmpty(p.Name) && p.Name.IndexOf(nameContains, StringComparison.OrdinalIgnoreCase) > -1);
            }
            if (couponGroupIdEqual.HasValue)
            {
                tempPpvs = tempPpvs.Where(ppv => ppv.CouponsGroupCode == couponGroupIdEqual.Value);
            }

            // get assetUserRuleId to filter ppvs by the same shop
            long userId = contextData.GetCallerUserId();
            if (CatalogManager.Instance.DoesGroupUsesTemplates(groupId) && userId > 0)
            {
                var shopId = Core.Api.Managers.AssetUserRuleManager.Instance.GetShopAssetUserRuleId(groupId, userId);
                if (shopId > 0)
                {
                    if (assetUserRuleIds == null)
                    {
                        assetUserRuleIds = new List<long>() { shopId };
                    }
                    else if (!assetUserRuleIds.Contains(shopId))
                    {
                        assetUserRuleIds.Add(shopId);
                    }
                }
            }

            if (assetUserRuleIds?.Count > 0)
            {
                tempPpvs = tempPpvs.Where(x => x.AssetUserRuleId.HasValue && assetUserRuleIds.Contains(x.AssetUserRuleId.Value));

            }

            switch (orderBy)
            {
                case PPVOrderBy.NameAsc:
                    tempPpvs = tempPpvs.OrderBy(ppv => ppv.Name);
                    break;
                case PPVOrderBy.NameDesc:
                    tempPpvs = tempPpvs.OrderByDescending(ppv => ppv.Name);
                    break;
                case PPVOrderBy.UpdateDataAsc:
                    tempPpvs = tempPpvs.OrderBy(ppv => ppv.UpdateDate);
                    break;
                case PPVOrderBy.UpdateDataDesc:
                    tempPpvs = tempPpvs.OrderByDescending(ppv => ppv.UpdateDate);
                    break;
            }
                
            response.TotalItems = tempPpvs.Count();

            if (!shouldIgnorePaging)
            {
                int startIndexOnList = pageIndex * pageSize;
                int rangeToGetFromList = (startIndexOnList + pageSize) > allPpvs.Count ? (allPpvs.Count - startIndexOnList) > 0 ? (allPpvs.Count - startIndexOnList) : 0 : pageSize;
                if (rangeToGetFromList > 0)
                {
                    tempPpvs = tempPpvs.Skip(startIndexOnList).Take(rangeToGetFromList);
                }
            }

            var ppvWithData = GetPpvsData(tempPpvs.ToList(), shouldShrink, groupId);
            response.Objects = ppvWithData;
            response.SetStatus(eResponseStatus.OK);
            return response;
        }

        private Tuple<List<PpvDTO>, bool> GetAllPpvs(Dictionary<string, object> funcParams)
        {
            List<PpvDTO> ppvDtos = null;
            bool shouldShrink = false;
            int? groupId;
            if (funcParams != null && funcParams.Count > 0 && funcParams.ContainsKey("groupId"))
            {
                groupId = funcParams["groupId"] as int?;
                ppvDtos = _repository.Get_AllPpvModuleData(groupId.Value);
                
            }
            bool res = ppvDtos != null;

            return new Tuple<List<PpvDTO>, bool>(ppvDtos, res);
        }

        private List<PPVModule> GetPpvsData(List<PpvDTO> ppvDtos, bool shouldShrink, int groupId)
        {
            List<PPVModule> ppvModules = new List<PPVModule>();

            if (ppvDtos != null && ppvDtos.Count > 0)
            {
                foreach (var ppvDto in ppvDtos)
                {
                    PPVModule ppvModule = new PPVModule();

                    string PpvKey = LayeredCacheKeys.GetPpvKey(ppvDto.Id);
                    if (!_layeredCache.Get<PPVModule>(PpvKey,
                        ref ppvModule,
                        GetPpv,
                        new Dictionary<string, object>()
                        {
                            {"groupId", groupId},
                            {"ppvDto", ppvDto},
                            {"shouldShrink", shouldShrink}
                        },
                        groupId,
                        LayeredCacheConfigNames.PPV_MODULE_CACHE_CONFIG_NAME,
                        new List<string>() { LayeredCacheKeys.GetPpvInvalidationKey(ppvDto.Id) }))
                    {
                        log.Error($"faild to GetPPVModules list .GetPPVModules from layeredCache for groupId:{groupId}.");
                    }

                    ppvModules.Add(ppvModule);
                }
            }
            return ppvModules;
        }
          
        private Tuple<PPVModule, bool> GetPpv(Dictionary<string, object> funcParams)
        {
            PPVModule ppvModule = new PPVModule();
            bool shouldShrink = false;
            PpvDTO ppvDto;
            int? groupId;
            if (funcParams != null && funcParams.Count > 0 && funcParams.ContainsKey("groupId")  && funcParams.ContainsKey("ppvDto"))
            {
                groupId = funcParams["groupId"] as int?;
                ppvDto = funcParams["ppvDto"] as PpvDTO;
                if (ppvDto != null)
                {
                    if (funcParams.ContainsKey("shouldShrink"))
                    {
                        shouldShrink = Convert.ToBoolean(funcParams["shouldShrink"]);
                    }
                    ppvModule = BuildPPVModuleFromDTO(groupId.Value, ppvDto, shouldShrink);
                }
            }

            return new Tuple<PPVModule, bool>(ppvModule, true);
        }

        public PPVModule BuildPPVModuleFromDTO(int groupId, PpvDTO ppvDto, bool shouldShrink)
        {
            var ppvModule = new PPVModule();
            if (!shouldShrink)
            {
                string couponsGroupCode = "";
                if (ppvDto.CouponsGroupCode != 0)
                {
                    couponsGroupCode = ppvDto.CouponsGroupCode.ToString();
                }

                ppvModule.Initialize(ppvDto.PriceCode.ToString(), ppvDto.UsageModuleCode.ToString(), ppvDto.DiscountCode.ToString(),
                    couponsGroupCode, GetPPVDescription(ppvDto.Id), groupId,
                    ppvDto.Id.ToString(), ppvDto.SubscriptionOnly,
                    ppvDto.Name, string.Empty, string.Empty, string.Empty,
                    GetPPVFileTypes(groupId, ppvDto.Id), ppvDto.FirstDeviceLimitation,
                    ppvDto.ProductCode, 0, ppvDto.AdsPolicy, ppvDto.AdsParam, ppvDto.CreateDate,
                    ppvDto.UpdateDate, ppvDto.IsActive, ppvDto.VirtualAssetId, ppvDto.AssetUserRuleId);
            }
            else
            {
                ppvModule.Initialize(ppvDto.PriceCode.ToString(), string.Empty, string.Empty, string.Empty,
                    GetPPVDescription(ppvDto.Id), groupId, ppvDto.Id.ToString(),
                    ppvDto.SubscriptionOnly,
                    ppvDto.Name, string.Empty, string.Empty, string.Empty,
                    GetPPVFileTypes(groupId, ppvDto.Id), ppvDto.FirstDeviceLimitation,
                    ppvDto.ProductCode,
                    0, ppvDto.AdsPolicy, ppvDto.AdsParam);
            }
            return ppvModule;
        }

        private LanguageContainer[] GetPPVDescription(Int32 nPPVModuleID)
        {
            LanguageContainer[] theContainer = null;
            DataTable dtPPVDescription = PricingDAL.Get_PPVDescription(nPPVModuleID);
            if (dtPPVDescription != null)
            {
                int nCount = dtPPVDescription.Rows.Count;
                if (nCount > 0)
                {
                    theContainer = new LanguageContainer[nCount];
                }
                Int32 nIndex = 0;
                for (int i = 0; i < nCount; i++)
                {
                    DataRow ppvDescriptionRow = dtPPVDescription.Rows[i];
                    string sLang = ODBCWrapper.Utils.GetSafeStr(ppvDescriptionRow["language_code3"]);
                    string sVal = ODBCWrapper.Utils.GetSafeStr(ppvDescriptionRow["description"]);
                    LanguageContainer t = new LanguageContainer();
                    t.Initialize(sLang, sVal);
                    theContainer[nIndex] = t;
                    nIndex++;
                }

            }
            return theContainer;
        }
        
        private List<int> GetPPVFileTypes(int nGroupID, int nPPVModuleID)
        {
            List<int> retVal = null;

            DataTable dtFileTypes = PricingDAL.Get_PPVFileTypes(nGroupID, nPPVModuleID);
            if (dtFileTypes != null)
            {
                int nCount = dtFileTypes.Rows.Count;
                if (nCount > 0)
                    retVal = new List<int>();
                for (int i = 0; i < nCount; i++)
                {
                    DataRow fileTypesRow = dtFileTypes.Rows[i];
                    int nFileTypeID = ODBCWrapper.Utils.GetIntSafeVal(fileTypesRow["file_type_id"]);
                    retVal.Add(nFileTypeID);
                }
            }
            return retVal;
        }
        
        private PpvDTO convertToDto(PpvModuleInternal ppv)
        {
            var ppvDto =  new PpvDTO()
            {
                Descriptions = ppv.Description,
                Name = ppv.Name,
                AdsParam = ppv.AdsParam,
                ProductCode = ppv.ProductCode,
                FirstDeviceLimitation = ppv.FirstDeviceLimitation.HasValue ? ppv.FirstDeviceLimitation.Value: false,
                SubscriptionOnly = ppv.SubscriptionOnly.HasValue ? ppv.SubscriptionOnly.Value: false,
                IsActive = ppv.IsActive.HasValue ? ppv.IsActive.Value: true,
                alias = ppv.alias,
                FileTypesIds = ppv.RelatedFileTypes,
                AdsPolicy = ppv.AdsPolicy,
                CouponsGroupCode = ppv.CouponsGroupId.HasValue ? ppv.CouponsGroupId.Value : 0,
                UsageModuleCode = ppv.UsageModuleId.HasValue ? ppv.UsageModuleId.Value : 0,
                DiscountCode = ppv.DiscountId.HasValue ? ppv.DiscountId.Value : 0,
                PriceCode = ppv.PriceId.HasValue ? ppv.PriceId.Value : 0,
            };
            if (ppv.VirtualAssetId.HasValue)
                ppvDto.VirtualAssetId = ppv.VirtualAssetId;
            if (ppv.CreateDate.HasValue)
                ppvDto.CreateDate = ppv.CreateDate.Value;
            if (ppv.UpdateDate.HasValue)
                ppvDto.UpdateDate = ppv.UpdateDate;
            
            if (ppv.AssetUserRuleId.HasValue)
            {
                ppvDto.AssetUserRuleId = ppv.AssetUserRuleId;
            }
            
            return ppvDto;
        }
        
        private PpvDTO convertToDto(PPVModule ppv)
        {
            PpvDTO ppvDto =  new PpvDTO()
            {
                Name = ppv.m_sObjectVirtualName,
                PriceCode = ppv.m_oPriceCode.m_nObjectID,
                UsageModuleCode = ppv.m_oUsageModule.m_nObjectID,
                Descriptions = ppv.m_sDescription,
                AdsParam = ppv.AdsParam,
                SubscriptionOnly = ppv.m_bSubscriptionOnly,
                FileTypesIds = ppv.m_relatedFileTypes,
                ProductCode = ppv.m_Product_Code,
                FirstDeviceLimitation = ppv.m_bFirstDeviceLimitation,
                alias = ppv.alias,
                AdsPolicy = ppv.AdsPolicy,
                AssetUserRuleId = ppv.AssetUserRuleId
            };
            if (ppv.m_oDiscountModule != null)
            {
                ppvDto.DiscountCode = ppv.m_oDiscountModule.m_nObjectID;
            }
            if (ppv.m_oCouponsGroup != null)
            {
                ppvDto.CouponsGroupCode = int.Parse(ppv.m_oCouponsGroup.m_sGroupCode);
            }

            return ppvDto;
        }
        
        private Status Validate(int groupId, int? PriceId, long? usageModuleId, long? discountModuleId, long? couponsGroupId, List<int> fileTypesIds = null)
        {
            if (PriceId.HasValue && PriceId.Value != 0)
            {
                var currPriceDetails = _priceDetailsManager.GetPriceDetailsById(groupId, PriceId.Value);
                if (!currPriceDetails.HasObject())
                {
                    return currPriceDetails.Status;
                }
            }
            if (usageModuleId.HasValue && usageModuleId.Value != 0)
            {
                 var currUsageModule = _usageModuleManager.GetUsageModuleById(groupId, usageModuleId.Value);
                if (!currUsageModule.HasObject())
                {
                    return currUsageModule.Status;
                }
            }
            if (discountModuleId.HasValue && discountModuleId.Value != 0)
            {
                var disocuntDetailes = _discountDetailsManager.GetDiscountDetailsById(groupId, discountModuleId.Value);
                if (!disocuntDetailes.HasObject())
                {
                    return disocuntDetailes.Status;
                }
            }
            if (couponsGroupId.HasValue && couponsGroupId.Value != 0)
            {
                var result = _pricingModule.GetCouponsGroup(groupId, couponsGroupId.Value);

                if (!result.Status.IsOkStatusCode() || result.CouponsGroup == null)
                {
                    return result.Status;
                }
            }
            if (fileTypesIds != null && fileTypesIds.Count > 0)
            {
                var res = _fileManager.GetMediaFileTypes(groupId);
                if (res.Objects == null || res.Objects.Count < 0)
                {
                    return new Status(eResponseStatus.InvalidFileTypes, $"FileTypes are missing for group");
                }

                List<long> groupFileTypeIds = res.Objects.Select(x => x.Id).ToList();

                foreach (var fileTypesId in fileTypesIds)
                {
                    if (!groupFileTypeIds.Contains(fileTypesId))
                    {
                        return new Status(eResponseStatus.InvalidFileType, $"FileType not valid {fileTypesId}");
                    }
                }
            }

            return Status.Ok;
        }

        public void SetPpvInvalidation(int groupId, int id = 0)
        {
            // invalidation keys
            var invalidationGroupKey = LayeredCacheKeys.GetPpvGroupInvalidationKey(groupId);
            if (!_layeredCache.SetInvalidationKey(invalidationGroupKey))
                log.Error($"Failed to set invalidation key for group ppv. key = {invalidationGroupKey}");

            if (id != 0)
            {
                var invalidationIdKey = LayeredCacheKeys.GetPpvInvalidationKey(id);
                if (!_layeredCache.SetInvalidationKey(invalidationIdKey))
                    log.Error($"Failed to set invalidation key for ppv. key = {invalidationIdKey}");
            }
        }
    }
}