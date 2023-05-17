using ApiLogic.Api.Managers;
using ApiObjects.Base;
using ApiObjects.Pricing;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Api;
using Core.Pricing;
using DAL;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Core.GroupManagers;

namespace ApiLogic.Pricing.Handlers
{
    public interface IPriceDetailsManager
    {
        GenericResponse<PriceDetails> GetPriceDetailsById(int groupId, long priceDetailsId);
    }

    public class PriceDetailsManager : IPriceDetailsManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<PriceDetailsManager> lazy = new Lazy<PriceDetailsManager>(() =>
            new PriceDetailsManager(PricingDAL.Instance,
                                    GeneralPartnerConfigManager.Instance,
                                    LayeredCache.Instance,
                                    api.Instance,
                                    GroupSettingsManager.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        public static PriceDetailsManager Instance => lazy.Value;

        private readonly IPriceDetailsRepository _repository;
        private readonly IGeneralPartnerConfigManager _generalPartnerConfigManager;
        private readonly ILayeredCache _layeredCache;
        private readonly ICountryManager _countryManager;
        private readonly IGroupSettingsManager _groupSettingsManager;
        public PriceDetailsManager(IPriceDetailsRepository priceDetailsRepository,
                                    IGeneralPartnerConfigManager generalPartnerConfigManager,
                                    ILayeredCache layeredCache,
                                    ICountryManager countryManager,
                                    IGroupSettingsManager groupSettingsManager)
        {
            _repository = priceDetailsRepository;
            _generalPartnerConfigManager = generalPartnerConfigManager;
            _layeredCache = layeredCache;
            _countryManager = countryManager;
            _groupSettingsManager = groupSettingsManager;
        }

        public GenericListResponse<PriceDetails> GetPriceDetailsList(int groupId, List<long> ids, string currencyCode = null)
        {
            var response = new GenericListResponse<PriceDetails>();

            // get prices with specific currency 
            if (!string.IsNullOrEmpty(currencyCode) && !currencyCode.Trim().Equals("*"))
            {
                if (!_generalPartnerConfigManager.IsValidCurrencyCode(groupId, currencyCode))
                {
                    response.Status.Set(eResponseStatus.InvalidCurrency, "Invalid currency");
                    return response;
                }
            }

            if (string.IsNullOrEmpty(currencyCode) && !_generalPartnerConfigManager.GetGroupDefaultCurrency(groupId, ref currencyCode))
            {
                response.SetStatus(eResponseStatus.Error, "could not get group default currencyCode");
                return response;
            }

            List<PriceDetails> allPriceDetails = null;
            if (!_layeredCache.Get(LayeredCacheKeys.GetGroupPriceCodesKey(groupId), 
                                   ref allPriceDetails, 
                                   GetGroupPriceCodes,
                                   new Dictionary<string, object>() { { "groupId", groupId } }, 
                                   groupId,
                                   LayeredCacheConfigNames.GET_GROUP_PRICE_CODES_LAYERED_CACHE_CONFIG_NAME,
                                   new List<string>() { LayeredCacheKeys.GetGroupPriceCodesInvalidationKey(groupId) }))
            {
                log.Error($"faild to GetPriceDetailsList.GetGroupPriceCodes from layeredCache for groupId:{groupId}.");
                return response;
            }

            var searchByIds = ids != null && ids.Count > 0;
            response.Objects = searchByIds ? new List<PriceDetails>(ids.Count) : new List<PriceDetails>(allPriceDetails.Count);
            if (allPriceDetails != null)
            {
                foreach (var pc in allPriceDetails)
                {
                    if (searchByIds && !ids.Contains(pc.Id))
                        continue;

                    // filter by currency 
                    if (!currencyCode.Trim().Equals("*"))
                    {
                        var n = new PriceDetails(pc)
                        {
                            Prices = pc.Prices?.Where(p => p.m_oCurrency.m_sCurrencyCD3.ToLower() == currencyCode.ToLower()).ToList()
                        };
                        response.Objects.Add(n);
                    }
                    else
                    {
                        response.Objects.Add(pc);
                    }
                }

                response.Objects = response.Objects.OrderBy(pc => pc.Name).ToList();
            }

            response.Status.Set(eResponseStatus.OK);
            return response;
        }

        public GenericResponse<PriceDetails> Add(ContextData contextData, PriceDetails priceDetailsToInsert)
        {
            var response = new GenericResponse<PriceDetails>();
            if (!_groupSettingsManager.IsOpc(contextData.GroupId))
            {
                response.SetStatus(eResponseStatus.AccountIsNotOpcSupported, "Account Is Not OPC Supported");
                return response;
            }
            
            try
            {
                var currencyMap = _generalPartnerConfigManager.GetCurrencyMapByCode3(contextData.GroupId);
                var countryMap = _countryManager.GetCountryMapById(contextData.GroupId);

                var validateStatus = ValidatePrices(priceDetailsToInsert.Prices, currencyMap, countryMap);
                if (!validateStatus.IsOkStatusCode())
                {
                    response.SetStatus(validateStatus);
                    return response;
                }

                var priceDetailsDTO = ConvertToPriceDetailsDTO(priceDetailsToInsert, currencyMap, countryMap);
                long id = _repository.InsertPriceDetails(contextData.GroupId, priceDetailsDTO, contextData.UserId.Value);
                if (id == 0)
                {
                    log.Error($"Error while InsertPriceDetails. contextData: {contextData}.");
                    return response;
                }

                SetPriceCodeValidation(contextData.GroupId, id);

                priceDetailsToInsert.Id = id;
                response.Object = priceDetailsToInsert;
                response.Status.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in PriceDetails.add. contextData:{contextData}, name:{priceDetailsToInsert.Name}.", ex);
            }

            return response;
        }

        public Status Delete(ContextData contextData, long id)
        {
            if (!_groupSettingsManager.IsOpc(contextData.GroupId))
            {
                return new Status(eResponseStatus.AccountIsNotOpcSupported, "Account Is Not OPC Supported");
            }
            
            var currPriceDetails = GetPriceDetailsById(contextData.GroupId, id);
            if (!currPriceDetails.HasObject())
            {
                return currPriceDetails.Status;
            }
            
            if (!_repository.DeletePriceDetails(contextData.GroupId, id, contextData.UserId.Value))
            {
                log.Error($"Error while DeletePriceDetails. contextData: {contextData}.");
                return Status.Error;
            }

            SetPriceCodeValidation(contextData.GroupId, id);
            return Status.Ok;
        }

        private Tuple<List<PriceDetails>, bool> GetGroupPriceCodes(Dictionary<string, object> funcParams)
        {
            List<PriceDetails> priceDetailsList = null;

            try
            {
                if (funcParams != null && funcParams.Count == 1 && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    var allCurrencies = _generalPartnerConfigManager.GetCurrencyList(groupId.Value);
                    if (allCurrencies == null)
                    {
                        log.Error($"could not GetGroupPriceCodes because group {groupId} does not have any Currencies");
                        return new Tuple<List<PriceDetails>, bool>(priceDetailsList, false);
                    }
                    var currencyMap = allCurrencies.ToDictionary(x => x.m_nCurrencyID);

                    List<PriceDetailsDTO> priceDetailsDTOList = _repository.GetPriceDetails(groupId.Value);
                    if (priceDetailsDTOList != null)
                    {
                        priceDetailsList = new List<PriceDetails>(priceDetailsDTOList.Count);
                        foreach (var priceDetailsDTO in priceDetailsDTOList)
                        {
                            var priceDetails = new PriceDetails()
                            {
                                Id = priceDetailsDTO.Id,
                                Name = priceDetailsDTO.Name,
                                Prices = priceDetailsDTO.Prices?.Select(p => new Price
                                {
                                    m_dPrice = p.Price,
                                    countryId = (int)p.CountryId,
                                    m_oCurrency = new Currency(currencyMap[(int)p.CurrencyId]),
                                }).ToList()
                            };

                            priceDetailsList.Add(priceDetails);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in GetGroupPriceCodes. parameters:{string.Join("; ", funcParams.Keys)}", ex);
            }

            bool res = priceDetailsList != null;

            return new Tuple<List<PriceDetails>, bool>(priceDetailsList, res);
        }

        private void SetPriceCodeValidation(int groupId, long priceDetailsId)
        {
            // invalidation keys
            string invalidationKey = LayeredCacheKeys.GetGroupPriceCodesInvalidationKey(groupId);
            if (!_layeredCache.SetInvalidationKey(invalidationKey))
            {
                log.ErrorFormat("Failed to set invalidation key for group price codes. key = {0}", invalidationKey);
            }

            invalidationKey = LayeredCacheKeys.GetPriceCodeInvalidationKey(groupId, (int)priceDetailsId);
            if (!_layeredCache.SetInvalidationKey(invalidationKey))
            {
                log.ErrorFormat("Failed to set invalidation key for Price code. key = {0}", invalidationKey);
            }

            invalidationKey = LayeredCacheKeys.GetPricingSettingsInvalidationKey(groupId);
            if (!_layeredCache.SetInvalidationKey(invalidationKey))
            {
                log.ErrorFormat("Failed to set pricing settings invalidation key after price code add/update, key = {0}", invalidationKey);
            }
        }

        public GenericResponse<PriceDetails> GetPriceDetailsById(int groupId, long priceDetailsId)
        {
            GenericResponse<PriceDetails> response = new GenericResponse<PriceDetails>();
            var priceDetailsList = GetPriceDetailsList(groupId, new List<long>() { priceDetailsId }, "*");
            if (!priceDetailsList.HasObjects())
            {
                response.SetStatus(eResponseStatus.PriceDetailsDoesNotExist, $"PriceDetails {priceDetailsId} does not exist");
            }
            else
            {
                response.Object = priceDetailsList.Objects[0];
                response.SetStatus(eResponseStatus.OK);
            }

            return response;
        }

        public GenericResponse<PriceDetails> Update(ContextData contextData, PriceDetails priceDetailsToUpdate)
        {
            var response = new GenericResponse<PriceDetails>();
            if (!_groupSettingsManager.IsOpc(contextData.GroupId))
            {
                response.SetStatus(eResponseStatus.AccountIsNotOpcSupported, "Account Is Not OPC Supported");
                return response;
            }
            
            var currPriceDetails = GetPriceDetailsById(contextData.GroupId, priceDetailsToUpdate.Id);
            if (!currPriceDetails.HasObject())
            {
                response.SetStatus(currPriceDetails.Status);
                return response;
            }

            try
            {
                var currencyMap = _generalPartnerConfigManager.GetCurrencyMapByCode3(contextData.GroupId);
                var countryMap = _countryManager.GetCountryMapById(contextData.GroupId);

                bool updatePriceCodes = false;
                if (!string.IsNullOrWhiteSpace(priceDetailsToUpdate.Name))
                {
                    updatePriceCodes = true;
                }
                else
                {
                    priceDetailsToUpdate.Name = currPriceDetails.Object.Name;
                }

                bool updatePriceCodesLocales = false;
                if (priceDetailsToUpdate.Prices?.Count > 0)
                {
                    updatePriceCodes = true;
                    updatePriceCodesLocales = true;
                    var validateStatus = ValidatePrices(priceDetailsToUpdate.Prices, currencyMap, countryMap);
                    if (!validateStatus.IsOkStatusCode())
                    {
                        response.SetStatus(validateStatus);
                        return response;
                    }
                }
                else
                {
                    priceDetailsToUpdate.Prices = currPriceDetails.Object.Prices;
                }

                if (updatePriceCodes || updatePriceCodesLocales)
                {
                    var priceDetailsDTO = ConvertToPriceDetailsDTO(priceDetailsToUpdate, currencyMap, countryMap);
                    var success = _repository.UpdatePriceDetails(contextData.GroupId, priceDetailsToUpdate.Id, updatePriceCodes, priceDetailsDTO, 
                        updatePriceCodesLocales, contextData.UserId.Value);
                    if (!success)
                    {
                        log.Error($"Error while UpdatePriceDetails id {priceDetailsToUpdate.Id}. contextData: {contextData}.");
                        return response;
                    }

                    SetPriceCodeValidation(contextData.GroupId, priceDetailsToUpdate.Id);
                }
                
                response.Object = priceDetailsToUpdate;
                response.Status.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in PriceDetails.Update. id:{priceDetailsToUpdate.Id}, contextData:{contextData}.", ex);
            }

            return response;
        }

        private Status ValidatePrices(List<Price> prices, Dictionary<string, Currency> currencyMap, Dictionary<int, ApiObjects.Country> countryMap)
        {
            foreach (var item in prices)
            {
                if (item.countryId != 0 && !countryMap.ContainsKey(item.countryId))
                {
                    return new Status(eResponseStatus.CountryNotFound, $"Country {item.countryId} not found");
                }

                if (!currencyMap.ContainsKey(item.m_oCurrency.m_sCurrencyCD3.ToLower()))
                {
                    return new Status(eResponseStatus.InvalidCurrency, $"Invalid currency {item.m_oCurrency.m_sCurrencyCD3}");
                }
            }
            return Status.Ok;
        }

        private PriceDetailsDTO ConvertToPriceDetailsDTO(PriceDetails priceDetails, Dictionary<string, Currency> currencyMap,
            Dictionary<int, ApiObjects.Country> countryMap)
        {
            return new PriceDetailsDTO()
            {
                Name = priceDetails.Name,
                Prices = priceDetails.Prices?.Select(p => new PriceCodeLocaleDTO
                {
                    Price = p.m_dPrice,
                    CountryCode = countryMap.ContainsKey(p.countryId) ? countryMap[p.countryId].Code : "--",
                    CurrencyId = currencyMap[p.m_oCurrency.m_sCurrencyCD3.ToLower()].m_nCurrencyID
                }).ToList()
            };
        }
    }
}