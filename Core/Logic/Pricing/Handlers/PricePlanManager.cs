using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Pricing;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Pricing;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace ApiLogic.Pricing.Handlers
{
    public class PricePlanManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<PricePlanManager> lazy = new Lazy<PricePlanManager>(() =>
                                    new PricePlanManager(PricingDAL.Instance,
                                        PricingDAL.Instance,
                                        PricingCache.Instance,
                                        LayeredCache.Instance,
                                        PriceDetailsManager.Instance,
                                        Core.Pricing.Utils.Instance,
                                        PricingDAL.Instance
                                    ),
            LazyThreadSafetyMode.PublicationOnly);

        public static PricePlanManager Instance => lazy.Value;

        private readonly IPricePlanRepository _repository;
        private readonly IPriceDetailsRepository _priceDetailsRepository;
        private readonly IPricingCache _pricingCache;
        private readonly ILayeredCache _layeredCache;
        private readonly IPriceDetailsManager _priceDetailsManager;
        private readonly IPricingUtils _pricingUtils;
        private readonly IModuleManagerRepository _moduleManagerRepository;

        public PricePlanManager(IPricePlanRepository pricePlanRepository,
                                IPriceDetailsRepository priceDetailsRepository,
                                IPricingCache pricingCache,
                                ILayeredCache layeredCache,
                                IPriceDetailsManager priceDetailsManager,
                                IPricingUtils pricingUtils,
                                IModuleManagerRepository moduleManagerRepository)
        {
            _repository = pricePlanRepository;
            _priceDetailsRepository = priceDetailsRepository;
            _pricingCache = pricingCache;
            _layeredCache = layeredCache;
            _priceDetailsManager = priceDetailsManager;
            _pricingUtils = pricingUtils;
            _moduleManagerRepository = moduleManagerRepository;
        }

        public GenericListResponse<UsageModule> GetPricePlans(int groupId, List<long> pricePlanIds)
        {
            var response = new GenericListResponse<UsageModule>();

            string cacheKey = GetGroupPricePlansKey(groupId);
            List<UsageModule> usageModuleList = null;
            if (!_pricingCache.TryGetGroupPricePlans(cacheKey, out usageModuleList) || usageModuleList == null)
            {
                List<UsageModuleDTO> usageModuleDTOList = _repository.GetPricePlansDTO(groupId);
                response.Objects = ConvertUsageModule(usageModuleDTOList);

                if (response.Objects != null)
                {
                    PricingCache.TryAddGroupPricePlans(cacheKey, response.Objects);
                }
            }
            else
            {
                response.Objects = usageModuleList;
            }

            if (pricePlanIds?.Count > 0 && response.Objects?.Count > 0)
            {
                response.Objects = response.Objects.Where(pp => pricePlanIds.Contains(pp.m_nObjectID)).ToList();
            }

            response.Status.Set(eResponseStatus.OK);

            response.TotalItems = response.Objects != null ? response.Objects.Count : 0;

            return response;
        }

        public GenericResponse<UsageModule> Update(int groupId, int id, UsageModule pricePlanToUpdate)
        {
            GenericResponse<UsageModule> response = new GenericResponse<UsageModule>();

            var usageModulesDTOs = _repository.GetPricePlansDTO(groupId, new List<long>() { id });

            if (usageModulesDTOs == null || usageModulesDTOs.Count == 0)
            {
                response.SetStatus(eResponseStatus.PricePlanDoesNotExist, $"Price plan {id} does not exist");
                return response;
            }

            if (!_priceDetailsRepository.IsPriceCodeExistsById(groupId, pricePlanToUpdate.m_pricing_id))
            {
                response.SetStatus(eResponseStatus.PriceDetailsDoesNotExist, "Price details does not exist");
                return response;
            }

            var usageModules = ConvertUsageModule(usageModulesDTOs);

            if (usageModules?.Count > 0)
            {
                // update only price code ID
                if (usageModules[0].m_pricing_id == pricePlanToUpdate.m_pricing_id || _repository.UpdatePricePlanAndSubscriptionsPriceCode(groupId, id, pricePlanToUpdate.m_pricing_id))
                {
                    usageModules[0].m_pricing_id = pricePlanToUpdate.m_pricing_id;
                    response.Object = usageModules[0];
                    response.SetStatus(eResponseStatus.OK);
                }
            }

            return response;
        }

        public Status Delete(ContextData contextData, long id)
        {
            Status result = new Status();

            if (!_moduleManagerRepository.IsUsageModuleExistsById(contextData.GroupId, id))
            {
                result.Set(eResponseStatus.PricePlanDoesNotExist, $"Price plan {id} does not exist");
                return result;
            }

            if (!_repository.DeletePricePlan(contextData.GroupId, id, contextData.UserId.Value))
            {
                log.Error($"Error while DeletePricePlan. contextData: {contextData.ToString()}.");
                result.Set(eResponseStatus.Error);
                return result;
            }

            string invalidationKey = LayeredCacheKeys.GetPricingSettingsInvalidationKey(contextData.GroupId);
            if (!_layeredCache.SetInvalidationKey(invalidationKey))
            {
                log.ErrorFormat("Failed to set pricing settings invalidation key after usage module add/update, key = {0}", invalidationKey);
            }

            result.Set(eResponseStatus.OK);
            return result;
        }

        public GenericResponse<UsageModule> Add(ContextData contextData, UsageModule pricePlanToInsert)
        {
            var response = new GenericResponse<UsageModule>();

            try
            {
                if (string.IsNullOrEmpty(pricePlanToInsert.m_sVirtualName))
                {
                    response.SetStatus(eResponseStatus.NameRequired, "Name required");
                    return response;
                }

                if (pricePlanToInsert.m_num_of_rec_periods < 0)
                {
                    response.SetStatus(eResponseStatus.InvalidArgumentValue, "renewalsNumber invalid value");

                }

                if (pricePlanToInsert.m_pricing_id == 0)
                {
                    response.SetStatus(eResponseStatus.InvalidPriceCode, $"Invalid priceDetails {pricePlanToInsert.m_pricing_id}");
                    return response;
                }
                else if (!_priceDetailsManager.IsPriceCodeExist(contextData.GroupId, pricePlanToInsert.m_pricing_id))
                {
                    response.SetStatus(eResponseStatus.PriceCodeDoesNotExist, $"Price details {pricePlanToInsert.m_pricing_id} does not exist");
                    return response;
                }

                if (pricePlanToInsert.m_tsMaxUsageModuleLifeCycle == 0 || string.IsNullOrEmpty(_pricingUtils.GetMinPeriodDescription(pricePlanToInsert.m_tsMaxUsageModuleLifeCycle)))
                {
                    response.SetStatus(eResponseStatus.InvalidArgumentValue, "fullLifeCycle invalid value");
                    return response;
                }

                if (pricePlanToInsert.m_tsViewLifeCycle == 0 || string.IsNullOrEmpty(_pricingUtils.GetMinPeriodDescription(pricePlanToInsert.m_tsViewLifeCycle)))
                {
                    response.SetStatus(eResponseStatus.InvalidArgumentValue, "viewLifeCycle invalid value");
                    return response;
                }


                IngestPricePlan ingestPricePlan = new IngestPricePlan()
                {
                    Code = pricePlanToInsert.m_sVirtualName,
                    IsActive = true,
                    MaxViews = pricePlanToInsert.m_nMaxNumberOfViews,
                    IsRenewable = pricePlanToInsert.m_is_renew == 1 ? true : false,
                    RecurringPeriods = pricePlanToInsert.m_num_of_rec_periods
                };

                int id = _repository.InsertPricePlan(contextData.GroupId, ingestPricePlan, pricePlanToInsert.m_pricing_id, pricePlanToInsert.m_tsMaxUsageModuleLifeCycle, pricePlanToInsert.m_tsViewLifeCycle, pricePlanToInsert.m_internal_discount_id);
                if (id == 0)
                {
                    log.Error($"Error while InsertPricePlan. contextData: {contextData.ToString()}.");
                    return response;
                }

                //SetPriceCodeValidation(contextData.GroupId, id);

                pricePlanToInsert.m_nObjectID = id;
                response.Object = pricePlanToInsert;
                response.Status.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in PricePlan. contextData:{contextData.ToString()}, name:{pricePlanToInsert.m_sVirtualName}.", ex);
            }

            return response;
        }

        private string GetGroupPricePlansKey(int groupId)
        {
            return $"GroupPricePlan_{groupId}";
        }

        private List<UsageModule> ConvertUsageModule(List<UsageModuleDTO> usageModuleDTOList)
        {
            List<UsageModule> usageModules = null;

            if (usageModuleDTOList?.Count > 0)
            {
                usageModules = new List<UsageModule>();
                UsageModule usageModule;
                foreach (var usageModuleDTO in usageModuleDTOList)
                {
                    usageModule = new UsageModule()
                    {
                        m_bIsOfflinePlayBack = usageModuleDTO.IsOfflinePlayBack,
                        m_bWaiver = usageModuleDTO.Waiver,
                        m_coupon_id = usageModuleDTO.CouponId,
                        m_ext_discount_id = usageModuleDTO.ExtDiscountId,
                        m_is_renew = usageModuleDTO.IsRenew,
                        m_nMaxNumberOfViews = usageModuleDTO.MaxNumberOfViews,
                        m_nObjectID = usageModuleDTO.Id,
                        m_num_of_rec_periods = usageModuleDTO.NumOfRecPeriods,
                        m_nWaiverPeriod = usageModuleDTO.WaiverPeriod,
                        m_pricing_id = usageModuleDTO.PricingId,
                        m_sVirtualName = usageModuleDTO.VirtualName,
                        m_tsMaxUsageModuleLifeCycle = usageModuleDTO.TsMaxUsageModuleLifeCycle,
                        m_tsViewLifeCycle = usageModuleDTO.TsViewLifeCycle
                    };
                    usageModules.Add(usageModule);
                }
            }

            return usageModules;
        }
    }
}