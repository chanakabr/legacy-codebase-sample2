using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using APILogic;
using ApiLogic.Api.Managers;
using APILogic.Api.Managers;
using ApiLogic.Pricing.Handlers;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.ConditionalAccess;
using ApiObjects.Pricing;
using ApiObjects.Response;
using ApiObjects.Rules;
using CachingProvider.LayeredCache;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.ConditionalAccess;
using DAL;
using Phx.Lib.Log;
using ApiObjects.Roles;
using KalturaRequestContext;

namespace Core.Pricing
{
    public class PriceManager : IPriceManager
    {
        private static readonly KLogger log = new KLogger(nameof(PriceManager));
        private const string ASSET_FILE_PPV_NOT_EXIST = "Asset file ppv doesn't exist";

        private static Lazy<IPriceManager> _lazy = new Lazy<IPriceManager>(
            () => new PriceManager(IndexManagerFactory.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        public static IPriceManager Instance => _lazy.Value;

        private readonly IIndexManagerFactory _indexManagerFactory;

        public PriceManager(IIndexManagerFactory indexManagerFactory)
        {
            _indexManagerFactory = indexManagerFactory;
        }

        public GenericResponse<AssetFilePpv> AddAssetFilePPV(ContextData contextData, AssetFilePpv assetFilePpv)
        {
            GenericResponse<AssetFilePpv> response = new GenericResponse<AssetFilePpv>();
            try
            {
                var status = ValidatePpvModuleExist(contextData, assetFilePpv.PpvModuleId);
                if (!status.IsOkStatusCode())
                {
                    return new GenericResponse<AssetFilePpv>(status);
                }

                var assetFileResponse = GetAssetFile(contextData.GroupId, assetFilePpv.AssetFileId);
                if (!assetFileResponse.IsOkStatusCode())
                {
                    return new GenericResponse<AssetFilePpv>(assetFileResponse.Status);
                }

                // validate ppvModuleId && mediaFileId not already exist
                bool isExist = IsAssetFilePpvExist(contextData.GroupId, assetFilePpv.AssetFileId, assetFilePpv.PpvModuleId);
                if (isExist)
                {
                    log.ErrorFormat("Error. mediaFileId {0} && ppvModuleId {1} already exist for groupId: {2}",
                        assetFilePpv.AssetFileId, assetFilePpv.PpvModuleId, contextData.GroupId);
                    response.SetStatus(eResponseStatus.Error, "AssetFilePpv already exist");
                    return response;
                }

                DataTable dt = PricingDAL.AddAssetFilePPV(
                    contextData.GroupId, assetFilePpv.AssetFileId, assetFilePpv.PpvModuleId, assetFilePpv.StartDate,
                    assetFilePpv.EndDate);

                if (dt == null || dt.Rows.Count == 0)
                {
                    log.ErrorFormat("Error while AddAssetFilePPV. groupId: {0}, mediaFileId: {1}, ppvModuleId: {2}",
                        contextData.GroupId, assetFilePpv.AssetFileId, assetFilePpv.PpvModuleId);
                    return response;
                }

                response.Object = new AssetFilePpv
                {
                    AssetFileId = assetFilePpv.AssetFileId,
                    EndDate = assetFilePpv.EndDate,
                    StartDate = assetFilePpv.StartDate,
                    PpvModuleId = assetFilePpv.PpvModuleId
                };

                response.Status.Code = (int)eResponseStatus.OK;
                response.Status.Message = eResponseStatus.OK.ToString();

                InvalidateAsset(contextData.GroupId, assetFileResponse.Object.AssetId);
                InvalidatePpvToFile(contextData.GroupId, assetFilePpv.AssetFileId);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed AddAssetFilePPV. groupId: {0}, mediaFileId: {1}, ppvModuleId: {2}. ex :{3}",
                    contextData.GroupId, assetFilePpv.AssetFileId, assetFilePpv.PpvModuleId, ex);
            }

            return response;
        }

        private void InvalidateAsset(int groupId, long assetId)
        {
            var indexingResult = _indexManagerFactory.GetIndexManager(groupId).UpsertMedia(assetId);
            if (!indexingResult)
            {
                log.ErrorFormat("Failed UpsertMedia index for assetId: {0}, groupId: {1} after AddMediaAsset", assetId, groupId);
            }
        }

        private void InvalidatePpvToFile(int groupId, long fileId)
        {
            //BEO-14429
            string invalidationKey = LayeredCacheKeys.GetPPVsforFileInvalidationKey(groupId, fileId);
            LayeredCache.Instance.SetInvalidationKey(invalidationKey);
        }

        public GenericResponse<AssetFilePpv> UpdateAssetFilePPV(ContextData contextData, AssetFilePpv request)
        {
            GenericResponse<AssetFilePpv> response = new GenericResponse<AssetFilePpv>();
            try
            {
                var status = ValidatePpvModuleExist(contextData, request.PpvModuleId);
                if (!status.IsOkStatusCode())
                {
                    return new GenericResponse<AssetFilePpv>(status);
                }

                var assetFileResponse = GetAssetFile(contextData.GroupId, request.AssetFileId);
                if (!assetFileResponse.IsOkStatusCode())
                {
                    return new GenericResponse<AssetFilePpv>(assetFileResponse.Status);
                }

                if (status != null && status.Code != (int)eResponseStatus.OK)
                {
                    response.SetStatus(status.Code, status.Message);
                    return response;
                }

                // validate ppvModuleId && mediaFileId not already exist
                bool isExist = IsAssetFilePpvExist(contextData.GroupId, request.AssetFileId, request.PpvModuleId);
                if (!isExist)
                {
                    log.ErrorFormat("Error. mediaFileId {0} && ppvModuleId {1} already exist for groupId: {2}",
                        request.AssetFileId, request.PpvModuleId, contextData.GroupId);
                    response.SetStatus(eResponseStatus.AssetFilePPVNotExist, ASSET_FILE_PPV_NOT_EXIST);
                    return response;
                }

                var nullableStartDate =
                    new DAL.NullableObj<DateTime?>(request.StartDate, request.IsNullablePropertyExists("StartDate"));
                var nullableEndDate =
                    new DAL.NullableObj<DateTime?>(request.EndDate, request.IsNullablePropertyExists("EndDate"));

                DataTable dt = PricingDAL.UpdateAssetFilePPV(contextData.GroupId, request.AssetFileId, request.PpvModuleId,
                    nullableStartDate, nullableEndDate);

                if (dt == null || dt.Rows.Count == 0)
                {
                    log.ErrorFormat("Error while UpdateAssetFilePPV. groupId: {0}, mediaFileId: {1}, ppvModuleId: {2}",
                        contextData.GroupId, request.AssetFileId, request.PpvModuleId);
                    return response;
                }

                response.Object = CreateAssetFilePPV(dt.Rows[0]);
                response.Status.Code = (int)eResponseStatus.OK;
                response.Status.Message = eResponseStatus.OK.ToString();

                InvalidateAsset(contextData.GroupId, assetFileResponse.Object.AssetId);
                InvalidatePpvToFile(contextData.GroupId, request.AssetFileId);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed UpdateAssetFilePPV. groupId: {0}, mediaFileId: {1}, ppvModuleId: {2}. ex :{3}",
                    contextData.GroupId, request.AssetFileId, request.PpvModuleId, ex);
            }

            return response;
        }

        public Status DeleteAssetFilePPV(ContextData contextData, long mediaFileId, long ppvModuleId)
        {
            try
            {
                // Validate mediaFileId && ppvModuleId
                var validationStatus = ValidatePpvModuleExist(contextData, ppvModuleId);
                if (!validationStatus.IsOkStatusCode())
                {
                    return validationStatus;
                }

                var assetFileResponse = GetAssetFile(contextData.GroupId, mediaFileId);
                if (!assetFileResponse.IsOkStatusCode())
                {
                    return assetFileResponse.Status;
                }

                int res = PricingDAL.DeleteAssetFilePPV(contextData.GroupId, mediaFileId, ppvModuleId);
                if (res == 0)
                {
                    return new Status((int)eResponseStatus.Error, "failed to DeleteAssetFilePPV");
                }

                if (res == -1)
                {
                    return new Status((int)eResponseStatus.AssetFilePPVNotExist, ASSET_FILE_PPV_NOT_EXIST);
                }

                InvalidateAsset(contextData.GroupId, assetFileResponse.Object.AssetId);
                InvalidatePpvToFile(contextData.GroupId, mediaFileId);

                return new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed DeleteAssetFilePPV. groupId: {0}, mediaFileId: {1}, ppvModuleId: {2}. ex :{3}",
                    contextData.GroupId, mediaFileId, ppvModuleId, ex);
            }

            return Status.Error;
        }

        public GenericListResponse<AssetFilePpv> GetAssetFilePPVList(ContextData contextData, long assetId, long assetFileId)
        {
            var response = new GenericListResponse<AssetFilePpv>();
            response.SetStatus(Status.Ok);
            List<AssetFile> assetFiles = null;
            
            if (assetId > 0)
            {
                // check if assetId exist
                GenericResponse<Asset> assetResponse =
                    AssetManager.Instance.GetAsset(contextData.GroupId, assetId, eAssetTypes.MEDIA, true);
                if (assetResponse != null && assetResponse.Status != null &&
                    assetResponse.Status.Code != (int)eResponseStatus.OK)
                {
                    response.SetStatus(assetResponse.Status);
                    return response;
                }

                assetFiles = FileManager.GetAssetFilesByAssetId(contextData.GroupId, assetId);
                // Get Asset Files
                if (assetFiles == null)
                {
                    log.ErrorFormat("Error while getting assetFiles. groupId: {0}, assetId {1}", contextData.GroupId, assetId);
                    response.SetStatus(Status.Error);
                    return response;
                }
            }
            else if (assetFileId > 0)
            {
                // check assetFileId  exist
                var assetFile = FileManager.Instance.GetAssetFileById(contextData.GroupId, assetFileId);

                // Get Asset Files
                assetFiles = assetFile == null ? new List<AssetFile>() : new List<AssetFile> { assetFile };
            }

            if (assetFiles.Count > 0)
            {
                var assetFileIds = assetFiles.Select(x => (int)x.Id).ToArray();
                var assetFilePPVList = GetAssetFilePPVByFileIds(contextData, assetFileIds);
                if (assetFilePPVList != null && assetFilePPVList.Count > 0)
                {
                    response.Objects.AddRange(assetFilePPVList);
                }
            }

            return response;
        }

        private static List<AssetFilePpv> GetAssetFilePPVByFileIds(ContextData contextData, int[] assetFileIds)
        {
            var mediaFilePPVContainerList = Module.GetPPVModuleListForMediaFilesWithExpiry(contextData.GroupId, assetFileIds, false);
            if (mediaFilePPVContainerList == null || mediaFilePPVContainerList.Length == 0) return null;

            var assetFilePPVList = new List<AssetFilePpv>();

            // BEO-14526 filter files only if request is impersonate or by master
            var shouldFilterFileByDates = RequestContextUtilsInstance.Get().IsImpersonateRequest() || contextData.UserRoleIds.All(x => x <= PredefinedRoleId.MASTER);
            foreach (var mediaFilePPVContainer in mediaFilePPVContainerList)
            {
                // validate assetFileId Exists
                if (mediaFilePPVContainer.m_oPPVModules == null || 
                    mediaFilePPVContainer.m_oPPVModules.Length == 0 || 
                    !GetAssetFile(contextData.GroupId, mediaFilePPVContainer.m_nMediaFileID).HasObject()) { continue; }

                foreach (var ppvModule in mediaFilePPVContainer.m_oPPVModules)
                {
                    var ppvModuleId = long.Parse(ppvModule.PPVModule.m_sObjectCode);
                    if (!ValidatePpvModuleExist(contextData, ppvModuleId).IsOkStatusCode()) { continue; }

                    var startDate = ppvModule.GetStartDate();
                    var endDate = ppvModule.GetEndDate();

                    if (shouldFilterFileByDates)
                    {
                        var now = DateTime.UtcNow;
                        if ((startDate.HasValue && startDate > now) || (endDate.HasValue && endDate < now)) { continue; }
                    }

                    assetFilePPVList.Add(new AssetFilePpv()
                    {
                        AssetFileId = mediaFilePPVContainer.m_nMediaFileID,
                        PpvModuleId = ppvModuleId,
                        StartDate = startDate,
                        EndDate = endDate
                    });
                }
            }

            return assetFilePPVList;
        }

        public static GenericListResponse<PPVModule> GetPPVList(int groupId, int pageIndex, int pageSize)
        {
            GenericListResponse<PPVModule> response = new GenericListResponse<PPVModule>();

            try
            {
                List<PPVModule> allPpvs = new List<PPVModule>();
                string allPpvsKey = LayeredCacheKeys.GetAllPpvsKey(groupId);

                if (!LayeredCache.Instance.Get<List<PPVModule>>(allPpvsKey,
                        ref allPpvs,
                        GetAllPpvs,
                        new Dictionary<string, object>()
                        {
                            { "groupId", groupId }
                        },
                        groupId,
                        LayeredCacheConfigNames.PPV_MODULES_CACHE_CONFIG_NAME,
                        new List<string>() { LayeredCacheKeys.GetPricingSettingsInvalidationKey(groupId) }))
                {
                    return response;
                }

                if (pageSize > 0)
                {
                    int skip = pageIndex * pageSize;

                    if (allPpvs.Count > skip)
                    {
                        response.Objects = (allPpvs.Count) > (skip + pageSize)
                            ? allPpvs.Skip(skip).Take(pageSize).ToList()
                            : allPpvs.Skip(skip).ToList();
                    }
                }
                else
                {
                    response.Objects = allPpvs;
                }
            }
            catch (Exception ex)
            {
                response.SetStatus(eResponseStatus.Error, eResponseStatus.Error.ToString());
            }

            return response;
        }

        private static Tuple<List<PPVModule>, bool> GetAllPpvs(Dictionary<string, object> funcParams)
        {
            List<PPVModule> allPpvs = new List<PPVModule>();

            try
            {
                if (funcParams != null && funcParams.Count == 1)
                {
                    if (funcParams.ContainsKey("groupId"))
                    {
                        int? groupId = funcParams["groupId"] as int?;
                        if (groupId.HasValue)
                        {
                            var ppvDtoList = PricingDAL.Instance.Get_PPVModuleData(groupId.Value, null);
                            if (ppvDtoList != null && ppvDtoList.Any())
                            {
                                for (int i = 0; i < ppvDtoList.Count; i++)
                                {
                                    var ppvDto = ppvDtoList[i];
                                    PPVModule t = new PPVModule();
                                    string couponsGroupCode = "";
                                    if (ppvDto.CouponsGroupCode != 0)
                                    {
                                        couponsGroupCode = ppvDto.CouponsGroupCode.ToString();
                                    }

                                    t.Initialize(ppvDto.ProductCode, ppvDto.UsageModuleCode.ToString(), ppvDto.DiscountCode.ToString(), couponsGroupCode,
                                        null, groupId.Value, ppvDto.Id.ToString(), ppvDto.SubscriptionOnly, ppvDto.Name, string.Empty, string.Empty, string.Empty, null,
                                        ppvDto.FirstDeviceLimitation, ppvDto.ProductCode, 0, ppvDto.AdsPolicy, ppvDto.AdsParam, ppvDto.CreateDate, ppvDto.UpdateDate, ppvDto.IsActive,
                                        ppvDto.VirtualAssetId, ppvDto.AssetUserRuleId);

                                    allPpvs.Add(t);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                allPpvs = null;
            }

            return new Tuple<List<PPVModule>, bool>(allPpvs, allPpvs != null);
        }

        private static AssetFilePpv CreateAssetFilePPV(DataRow dataRow)
        {
            AssetFilePpv assetFilePPV = new AssetFilePpv()
            {
                AssetFileId = ODBCWrapper.Utils.GetLongSafeVal(dataRow, "MEDIA_FILE_ID"),
                PpvModuleId = ODBCWrapper.Utils.GetLongSafeVal(dataRow, "PPV_MODULE_ID"),
                StartDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dataRow, "START_DATE"),
                EndDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dataRow, "END_DATE")
            };

            return assetFilePPV;
        }

        private static Status ValidatePpvModuleExist(ContextData contextData, long ppvModuleId)
        {
            // validate ppvModuleId Exists
            var ppvById = PpvManager.Instance.GetPpvById(contextData, ppvModuleId);
            bool isExist = ppvById != null && ppvById.HasObject();

            if (!isExist)
            {
                log.Error($"Error. Unknown PPVModule: {ppvModuleId} for groupId: {contextData.GroupId}");
                return new Status((int)eResponseStatus.UnKnownPPVModule, "The ppv module is unknown");
            }

            return Status.Ok;
        }

        private static GenericResponse<AssetFile> GetAssetFile(int partnerId, long mediaFileId)
        {
            var assetFile = FileManager.Instance.GetAssetFileById(partnerId, mediaFileId);
            if (assetFile == null)
            {
                log.ErrorFormat("Error. Unknown mediaFileId: {0} for groupId: {1}", mediaFileId, partnerId);

                return new GenericResponse<AssetFile>(eResponseStatus.MediaFileDoesNotExist, "Media file does not exist");
            }

            return new GenericResponse<AssetFile>(Status.Ok, assetFile);
        }

        private static bool IsPPVModuleExist(int groupId, long ppvModuleId)
        {
            // check ppvModuleId  exist
            var ppvDTOs = PricingDAL.Instance.Get_PPVModuleData(groupId, (int)ppvModuleId);
            if (ppvDTOs != null && ppvDTOs.Any())
            {
                return true;
            }

            return false;
        }

        private static bool IsAssetFilePpvExist(int groupId, long mediaFileId, long ppvModuleId)
        {
            // check ppvModuleId  exist
            var dr = PricingDAL.Get_PPVModuleForMediaFile((int)mediaFileId, ppvModuleId, groupId);

            return dr != null;
        }

        public static Price GetPagoFinalPrice(int groupId, long userId, ref PriceReason priceReason,
            ProgramAssetGroupOffer pago,
            string country, string ip, string currency = null)
        {
            // get user status and validity if needed
            int domainId = 0;
            if (!CatalogLogic.IsUserValid(userId.ToString(), groupId, ref domainId))
            {
            }

            Price price = null;

            price = HandlePagoPrice(ref priceReason, groupId, ref currency, ref country, pago.PriceDetailsId, ip,
                userId);
            if (priceReason != PriceReason.ForPurchase)
            {
                return price;
            }

            DomainEntitlements domainEntitlements = null;
            if (ConditionalAccess.Utils.TryGetDomainEntitlementsFromCache(groupId, domainId, null,
                    ref domainEntitlements))
            {
                if (domainEntitlements.PagoEntitlements != null &&
                    domainEntitlements.PagoEntitlements.ContainsKey(pago.Id))
                {
                    bool isPending = domainEntitlements.PagoEntitlements[pago.Id].IsPending;
                    priceReason = isPending ? PriceReason.PendingEntitlement : PriceReason.PagoPurchased;
                    price.m_dPrice = 0.0;
                    return price;
                }
            }

            return price;
        }

        private static Price HandlePagoPrice(ref PriceReason priceReason, int groupId, ref string currency,
            ref string country, long? priceDetailsId,
            string ip = null, long? userId = null)
        {
            Price price = new Price();
            PriceCode priceCode = null;

            if (priceDetailsId.HasValue)
            {
                GenericResponse<PriceDetails> priceDetailsResponse =
                    PriceDetailsManager.Instance.GetPriceDetailsById(groupId, priceDetailsId.Value);
                if (!priceDetailsResponse.HasObject())
                {
                    log.Warn($"Warning: at HandlePriceAndDiscount: PriceDetails {priceDetailsId} does not exist");
                    return price;
                }

                bool isValidCurrencyCode = false;
                // Validate currencyCode if it was passed in the request
                if (!string.IsNullOrEmpty(currency))
                {
                    if (!GeneralPartnerConfigManager.Instance.IsValidCurrencyCode(groupId, currency))
                    {
                        priceReason = PriceReason.InvalidCurrency;
                        return price;
                    }

                    isValidCurrencyCode = true;
                }

                price = Extensions.Clone(priceDetailsResponse.Object.Prices[0]);

                // Get price code according to country and currency (if exists on the request)
                if (!string.IsNullOrEmpty(ip) && (isValidCurrencyCode ||
                                                  GeneralPartnerConfigManager.Instance.GetGroupDefaultCurrency(groupId,
                                                      ref currency)))
                {
                    country = APILogic.Utils.GetIP2CountryCode(groupId, ip);
                    PriceCode priceCodeWithCurrency =
                        Module.GetPriceCodeDataByCountyAndCurrency(groupId, (int)priceDetailsResponse.Object.Id,
                            country, currency);
                    if (priceCodeWithCurrency == null)
                    {
                        priceReason = PriceReason.CurrencyNotDefinedOnPriceCode;
                        return price;
                    }

                    priceCode = Extensions.Clone(priceCodeWithCurrency);
                }

                if (priceCode != null)
                {
                    price = Extensions.Clone(priceCode.m_oPrise);
                }
            }

            priceReason = PriceReason.ForPurchase;
            return price;
        }

        /// <summary>
        /// Calculate lowest price according to external Discount Module and BusinessModuleRules
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="currentPrice"></param>
        /// <param name="externalDiscount"></param>
        /// <param name="domainId"></param>
        /// <param name="transactionType"></param>
        /// <param name="currencyCode"></param>
        /// <param name="businessModuleId"></param>
        /// <param name="countryCode"></param>
        /// <returns></returns>
        public static Price GetLowestPrice(int groupId, Price currentPrice, int domainId, Price discountPrice,
            eTransactionType transactionType,
            string currencyCode, long businessModuleId, string countryCode, ref string couponCode,
            CouponsGroup couponsGroup,
            List<SubscriptionCouponGroup> subscriptionCouponGroups, List<string> allUserIdsInDomain, long mediaId = 0)
        {
            Price lowestPrice = discountPrice ?? currentPrice;

            if (BusinessModuleRuleManager.IsActionTypeRuleExists(groupId, RuleActionType.ApplyDiscountModuleRule))
            {
                if (allUserIdsInDomain == null || allUserIdsInDomain.Count == 0)
                {
                    allUserIdsInDomain = Domains.Module.GetDomainUserList(groupId, domainId);
                }

                // get all segments in domain
                List<long> segmentIds =
                    ConditionalAccess.Utils.GetDomainSegments(groupId, domainId, allUserIdsInDomain);

                // calc lowest price
                var filter = new BusinessModuleRuleConditionScope()
                {
                    BusinessModuleId = businessModuleId,
                    BusinessModuleType = transactionType,
                    SegmentIds = segmentIds,
                    FilterByDate = true,
                    FilterBySegments = true,
                    GroupId = groupId,
                    MediaId = mediaId
                };

                var businessModuleRules =
                    BusinessModuleRuleManager.GetBusinessModuleRules(groupId, filter,
                        RuleActionType.ApplyDiscountModuleRule);
                if (businessModuleRules.HasObjects())
                {
                    log.DebugFormat("Utils.GetLowestPrice - businessModuleRules count: {0}",
                        businessModuleRules.Objects.Count);
                    foreach (var businessModuleRule in businessModuleRules.Objects)
                    {
                        if (businessModuleRule.Actions != null && businessModuleRule.Actions.Count == 1)
                        {
                            var discountModule = Module.Instance.GetDiscountCodeDataByCountryAndCurrency(
                                groupId,
                                (int)(businessModuleRule.Actions[0] as ApplyDiscountModuleRuleAction).DiscountModuleId,
                                countryCode, currencyCode);
                            if (discountModule != null)
                            {
                                var tempPrice =
                                    ConditionalAccess.Utils.Instance.GetPriceAfterDiscount(currentPrice, discountModule,
                                        1);
                                if (tempPrice != null && tempPrice.m_dPrice < lowestPrice.m_dPrice)
                                {
                                    lowestPrice = tempPrice;
                                }
                            }
                        }
                    }
                }
            }

            if (lowestPrice.IsFree() || transactionType == eTransactionType.PPV || string.IsNullOrEmpty(couponCode))
            {
                return lowestPrice;
            }

            lowestPrice = ConditionalAccess.Utils.Instance.GetLowestPriceByCouponCode(groupId, ref couponCode,
                subscriptionCouponGroups, lowestPrice, domainId,
                couponsGroup, countryCode);
            return lowestPrice;
        }
    }
}
