using ApiObjects.Base;
using ApiObjects.Response;
using Core.Pricing;
using Phx.Lib.Log;
using System;
using DAL;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using ApiObjects.Pricing;
using System.Threading;
using ApiLogic.Api.Managers;
using CachingProvider.LayeredCache;
using System.Data;
using Core.Api;
using Core.GroupManagers;

namespace ApiLogic.Pricing.Handlers
{
    public interface IDiscountDetailsManager
    {
        GenericResponse<DiscountDetails> GetDiscountDetailsById(int groupId, long discountCodeId);
    }

    public class DiscountDetailsManager: IDiscountDetailsManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<DiscountDetailsManager> lazy = new Lazy<DiscountDetailsManager>(() => new DiscountDetailsManager(
            PricingDAL.Instance, LayeredCache.Instance, GeneralPartnerConfigManager.Instance, api.Instance, GroupSettingsManager.Instance), LazyThreadSafetyMode.PublicationOnly);
        public static DiscountDetailsManager Instance => lazy.Value;

        private readonly IDiscountDetailsRepository _repository;
        private readonly ILayeredCache _layeredCache;
        private readonly IGeneralPartnerConfigManager _generalPartnerConfigManager;
        private readonly ICountryManager _countryManager;
        private readonly IGroupSettingsManager _groupSettingsManager;
        
        public DiscountDetailsManager(IDiscountDetailsRepository repository,
                                      ILayeredCache layeredCache, 
                                      IGeneralPartnerConfigManager generalPartnerConfigManager,
                                      ICountryManager countryManager,
                                      IGroupSettingsManager groupSettingsManager)
        {
            _repository = repository;
            _layeredCache = layeredCache;
            _generalPartnerConfigManager = generalPartnerConfigManager;
            _countryManager = countryManager;
            _groupSettingsManager = groupSettingsManager;
        }

        public GenericResponse<DiscountDetails> Add(ContextData contextData, DiscountDetails discountDetailsToInsert)
        {
            var response = new GenericResponse<DiscountDetails>();
            if (!_groupSettingsManager.IsOpc(contextData.GroupId))
            {
                response.SetStatus(eResponseStatus.AccountIsNotOpcSupported, "Account Is Not OPC Supported");
                return response;
            }
            var currencyMap = _generalPartnerConfigManager.GetCurrencyMapByCode3(contextData.GroupId);
            var countryMap = _countryManager.GetCountryMapById(contextData.GroupId);
            var validateStatus = ValidateDiscounts(discountDetailsToInsert.MultiCurrencyDiscounts, currencyMap, countryMap);
            if (!validateStatus.IsOkStatusCode())
            {
                response.SetStatus(validateStatus);
                return response;
            }

            DiscountDetailsDTO discountDetailsDTO = ConvertToDiscountDetailsDtos(discountDetailsToInsert, currencyMap, countryMap);
            long id = _repository.InsertDiscountDetails(contextData.GroupId, contextData.UserId.Value, discountDetailsDTO);

            if (id == 0)
            {
                log.Error($"Error while InsertDiscountDetails. contextData: {contextData}.");
                return response;
            }

            SetDiscountCodeInvalidation(contextData.GroupId, id);
            discountDetailsToInsert.Id = id;
            response.Object = discountDetailsToInsert;
            response.Status.Set(eResponseStatus.OK);

            return response;
        }

        public Status Delete(ContextData contextData, long id)
        {
            Status result = new Status();
            if (!_groupSettingsManager.IsOpc(contextData.GroupId))
            {
                result.Set(eResponseStatus.AccountIsNotOpcSupported, "Account Is Not OPC Supported");
                return result;
            }
            var disocuntDetailes = GetDiscountDetailsById(contextData.GroupId, id);
            if (!disocuntDetailes.HasObject())
            {
                return disocuntDetailes.Status;
            }
            if (!_repository.DeleteDiscountDetails(contextData.GroupId, id, contextData.UserId.Value))
            {
                log.Error($"Error while DeleteDiscountCode. id: {id}.");
                result.Set(eResponseStatus.Error);
                return result;
            }

            SetDiscountCodeInvalidation(contextData.GroupId, id);
            result.Set(eResponseStatus.OK);

            return result;
        }

        public GenericListResponse<DiscountDetails> GetDiscounts(int groupId, List<long> discountIds, string currencyCode)
        {
            GenericListResponse<DiscountDetails> response = new GenericListResponse<DiscountDetails>();

            if (!string.IsNullOrEmpty(currencyCode) && !currencyCode.Trim().Equals("*"))
            {
                if (!GeneralPartnerConfigManager.Instance.IsValidCurrencyCode(groupId, currencyCode))
                {
                    response.SetStatus(eResponseStatus.InvalidCurrency, "Invalid currency");
                    return response;
                }
            }
            if (string.IsNullOrEmpty(currencyCode) && !GeneralPartnerConfigManager.Instance.GetGroupDefaultCurrency(groupId, ref currencyCode))
            {
                return response;
            }

            var discountDetails = GetDiscountDetails(groupId);
            if (discountDetails != null)
            {
                response.Objects = new List<DiscountDetails>();
                foreach (DiscountDetails dt in discountDetails)
                {
                    try
                    {
                        DiscountDetails dd = new DiscountDetails(dt);
                        // filter by IDs
                        if (discountIds != null && discountIds.Count > 0 && !discountIds.Contains(dt.Id))
                            continue;

                        // filter by currency 
                        if (!currencyCode.Trim().Equals("*"))
                        {
                            dd.MultiCurrencyDiscounts = dt.MultiCurrencyDiscounts != null ?
                                new List<Discount>(dt.MultiCurrencyDiscounts.Where(p => p.m_oCurrency.m_sCurrencyCD3 == currencyCode).ToList()) : null;
                        }

                        response.Objects.Add(dd);
                    }
                    catch (Exception ex)
                    {
                        log.Error($"Error creating DiscountDetails from id: {dt.Id}", ex);
                    }
                }
            }
            response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            return response;
        }

        public GenericResponse<DiscountDetails> Update(ContextData contextData, long id, DiscountDetails discountDetails)
        {
            var response = new GenericResponse<DiscountDetails>();
            if (!_groupSettingsManager.IsOpc(contextData.GroupId))
            {
                response.SetStatus(eResponseStatus.AccountIsNotOpcSupported, "Account Is Not OPC Supported");
                return response;
            }
            var OldDisocuntDetaileslResponse = GetDiscountDetailsById(contextData.GroupId, id);
            if (!OldDisocuntDetaileslResponse.HasObject())
            {
                response.SetStatus(OldDisocuntDetaileslResponse.Status);
                return response;
            }

            var oldPDiscountDetail = OldDisocuntDetaileslResponse.Object;
            var currencyMap = _generalPartnerConfigManager.GetCurrencyMapByCode3(contextData.GroupId);
            var countryMap = _countryManager.GetCountryMapById(contextData.GroupId);
            var validateStatus = ValidateDiscounts(discountDetails.MultiCurrencyDiscounts, currencyMap, countryMap);
            if (!validateStatus.IsOkStatusCode())
            {
                response.SetStatus(validateStatus);
                return response;
            }

            bool isMultiCurrencyUpdateNeeded = discountDetails.MultiCurrencyDiscounts?.Count > 0 ? true : false;
                
            bool isUpdateNeeded = discountDetails.IsUpdateNedded(oldPDiscountDetail);
            if (isUpdateNeeded || isMultiCurrencyUpdateNeeded)
            {
                DiscountDetailsDTO discountDetailsDTO = ConvertToDiscountDetailsDtos(discountDetails, currencyMap, countryMap);
                long dd = _repository.UpdateDiscountDetails(id, contextData.GroupId, contextData.UserId.Value,  isMultiCurrencyUpdateNeeded,isUpdateNeeded, discountDetailsDTO);

                if (dd > 0)
                {
                    SetDiscountCodeInvalidation(contextData.GroupId, id);
                    discountDetails.Id = id;
                    response.Object = discountDetails;
                    response.SetStatus(eResponseStatus.OK);
                }
            }
            else
            {
                discountDetails.Id = id;
                response.Object = discountDetails;
                response.SetStatus(eResponseStatus.OK);
            }

            return response;
        }

        private DiscountDetailsDTO ConvertToDiscountDetailsDtos(DiscountDetails discountDetails, Dictionary<string, Currency> currencyMap,
                                                                Dictionary<int, ApiObjects.Country> countryMap)
        {
            return new DiscountDetailsDTO()
            {
                Name = discountDetails.Name,
                StartDate = discountDetails.StartDate,
                EndDate = discountDetails.EndDate,
                WhenAlgoType = discountDetails.WhenAlgoType,
                WhenAlgoTimes = discountDetails.WhenAlgoTimes,
                Discounts = discountDetails.MultiCurrencyDiscounts?.Select(d => new DiscountDTO(d.m_dPrice,
                                                                          d.Percentage,
                                                                          currencyMap[d.m_oCurrency.m_sCurrencyCD3.ToLower()].m_nCurrencyID,
                                                                          countryMap.ContainsKey(d.countryId) ? countryMap[d.countryId].Code : "--")).ToList()
            };
        }

        public GenericResponse<DiscountDetails> GetDiscountDetailsById(int groupId, long discountCodeId)
        {
            GenericResponse<DiscountDetails> response = new GenericResponse<DiscountDetails>();
            var discountDetailResponse = GetDiscounts(groupId, new List<long> { discountCodeId }, "*");

            if (!discountDetailResponse.HasObjects())
            {
                response.SetStatus(eResponseStatus.DiscountCodeNotExist, $"Discount Code {discountCodeId} does not exist");
                return response;
            }

            response.Object = discountDetailResponse.Objects[0];
            response.SetStatus(eResponseStatus.OK);

            return response;
        }

        private Status ValidateDiscounts(List<Discount> multiCurrencyDiscounts, Dictionary<string, Currency> currencyMap, Dictionary<int, ApiObjects.Country> countryMap)
        {
            for (int i = 0; i < multiCurrencyDiscounts.Count; i++)
            {
                var discount = multiCurrencyDiscounts[i];

                if (discount.countryId !=0 && !countryMap.ContainsKey(discount.countryId))
                {
                    return new Status(eResponseStatus.CountryNotFound, $"Country {discount.countryId} not found");
                }
                if (!currencyMap.ContainsKey(discount.m_oCurrency.m_sCurrencyCD3.ToLower()))
                {
                    return new Status(eResponseStatus.InvalidCurrency, $"Invalid currency {discount.m_oCurrency.m_sCurrencyCD3}");
                }

                if (discount.m_oCurrency != null)
                {
                    discount.m_oCurrency.m_nCurrencyID = currencyMap[discount.m_oCurrency.m_sCurrencyCD3.ToLower()].m_nCurrencyID;
                }
            };
            return Status.Ok;
        }

        public List<DiscountDetails> GetDiscountDetails(int groupId)
        {
            string key = LayeredCacheKeys.GetDiscountsKey(groupId);
            List<DiscountDetails> discountDetails = null;
            _layeredCache.Get(key, 
                              ref discountDetails, 
                              GetGroupDiscounts,
                              new Dictionary<string, object>() { { "groupId", groupId } }, 
                              groupId,
                              LayeredCacheConfigNames.GET_GROUP_DISCOUNTS_LAYERED_CACHE_CONFIG_NAME, 
                              new List<string>(){ LayeredCacheKeys.GetGroupDiscountsInvalidationKey(groupId) });

            return discountDetails;
        }

        private Tuple<List<DiscountDetails>, bool> GetGroupDiscounts(Dictionary<string, object> funcParams)
        {
            List<DiscountDetails> discountDetails = null;

            try
            {
                if (funcParams != null && funcParams.Count == 1 && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    DataTable discountsDt = _repository.GetGroupDiscounts(groupId.Value);
                    if (discountsDt != null)
                    {
                        // set order multi pricing
                        discountsDt.DefaultView.Sort = "dcl_id asc";
                        discountsDt = discountsDt.DefaultView.ToTable();
                        discountDetails = Utils.BuildDiscountsFromDataTable(groupId.Value, discountsDt);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetGroupDiscounts failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }
            bool res = discountDetails != null;

            return new Tuple<List<DiscountDetails>, bool>(discountDetails, res);
        }

        private void SetDiscountCodeInvalidation(int groupId, long discountDetailsId)
        {
            // invalidation keys
            string invalidationKey = LayeredCacheKeys.GetGroupDiscountsInvalidationKey(groupId);
            if (!_layeredCache.SetInvalidationKey(invalidationKey))
            {
                log.ErrorFormat("Failed to set invalidation key for group discount codes. key = {0}", invalidationKey);
            }

            invalidationKey = LayeredCacheKeys.GetDiscountCodeInvalidationKey(groupId, (int)discountDetailsId);
            if (!_layeredCache.SetInvalidationKey(invalidationKey))
            {
                log.ErrorFormat("Failed to set invalidation key for discount code. key = {0}", invalidationKey);
            }
        }
    }
}
