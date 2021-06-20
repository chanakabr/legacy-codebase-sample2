using ApiLogic.Api.Managers;
using ApiObjects.Base;
using ApiObjects.Pricing;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Pricing;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace ApiLogic.Pricing.Handlers
{
    public interface IPriceDetailsManager
    {
        bool IsPriceCodeExist(int groupId, long priceCodeId);
    }

    public class PriceDetailsManager : IPriceDetailsManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<PriceDetailsManager> lazy = new Lazy<PriceDetailsManager>(() =>
            new PriceDetailsManager(PricingDAL.Instance,
                                    GeneralPartnerConfigManager.Instance,
                                    Price.Instance,
                                    LayeredCache.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        public static PriceDetailsManager Instance => lazy.Value;

        private readonly IPriceDetailsRepository _repository;
        private readonly IGeneralPartnerConfigManager _generalPartnerConfigManager;
        private readonly IPrice _price;
        private readonly ILayeredCache _layeredCache;

        public PriceDetailsManager(IPriceDetailsRepository priceDetailsRepository,
                                    IGeneralPartnerConfigManager generalPartnerConfigManager,
                                    IPrice price,
                                    ILayeredCache layeredCache)
        {
            _repository = priceDetailsRepository;
            _generalPartnerConfigManager = generalPartnerConfigManager;
            _price = price;
            _layeredCache = layeredCache;
        }

        public GenericListResponse<PriceDetails> GetPriceCodesDataByCurrency(int groupId, List<long> priceCodeIds, string currencyCode)
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
                return response;
            }

            string key = LayeredCacheKeys.GetGroupPriceCodesKey(groupId);

            var funcParams = new Dictionary<string, object>() { { "groupId", groupId } };
            List<PriceDetails> priceCodes = null;
            _layeredCache.Get(key, ref priceCodes, GetGroupPriceCodes, funcParams, groupId,
                LayeredCacheConfigNames.GET_GROUP_PRICE_CODES_LAYERED_CACHE_CONFIG_NAME,
                new List<string>() {LayeredCacheKeys.GetGroupPriceCodesInvalidationKey(groupId)});

            if (priceCodes != null)
            {
                response.Objects = new List<PriceDetails>();

                foreach (var pc in priceCodes)
                {
                    // filter by IDs
                    if (priceCodeIds != null && priceCodeIds.Count > 0 && !priceCodeIds.Contains(pc.Id))
                        continue;

                    // filter by currency 
                    if (!currencyCode.Trim().Equals("*"))
                    {
                        var n = new PriceDetails(pc);
                        n.Prices = pc.Prices != null ? pc.Prices.Where(p => p.m_oCurrency.m_sCurrencyCD3 == currencyCode).ToList() : null;
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

            response.TotalItems = response.Objects?.Count ?? 0;
            return response;
        }


        public GenericResponse<PriceDetails> Add(ContextData contextData, PriceDetails priceDetailsToInsert)
        {
            var response = new GenericResponse<PriceDetails>();

            try
            {
                if (string.IsNullOrEmpty(priceDetailsToInsert.Name))
                {
                    response.SetStatus(eResponseStatus.NameRequired, "Name required");
                    return response;
                }

                if (priceDetailsToInsert.Prices == null || priceDetailsToInsert.Prices.Count == 0)
                {
                    response.SetStatus(eResponseStatus.PriceIsMissing, "Price required");
                    return response;
                }

                // validate Price
                foreach (var item in priceDetailsToInsert.Prices)
                {
                    if (item.m_dPrice < 1)
                    {
                        response.SetStatus(eResponseStatus.AmountIsMissing, "Amount required");
                        return response;
                    }

                    if (item.m_oCurrency == null || string.IsNullOrEmpty(item.m_oCurrency.m_sCurrencyCD3))
                    {
                        response.SetStatus(eResponseStatus.CurrencyIsMissing, "Currency Required");
                        return response;
                    }

                    if (!_generalPartnerConfigManager.IsValidCurrencyCode(contextData.GroupId, item.m_oCurrency.m_sCurrencyCD3))
                    {
                        response.SetStatus(eResponseStatus.InvalidCurrency, "Invalid currency");
                        return response;
                    }

                    item.m_oCurrency.m_nCurrencyID = _price.InitializeByCD3(item.m_oCurrency.m_sCurrencyCD3, item.m_dPrice).m_oCurrency.m_nCurrencyID;
                }

                List<PriceDTO> pricesDTO = null;
                if (priceDetailsToInsert.Prices.Count > 1)
                {
                    pricesDTO = ConvertPriceDetails(priceDetailsToInsert.Prices.Skip(1).ToList());
                }

                long id = _repository.InsertPriceDetails(contextData.GroupId, priceDetailsToInsert.Name, priceDetailsToInsert.Prices[0].m_dPrice, priceDetailsToInsert.Prices[0].m_oCurrency.m_nCurrencyID, pricesDTO, contextData.UserId.Value);
                if (id == 0)
                {
                    log.Error($"Error while InsertPriceDetails. contextData: {contextData.ToString()}.");
                    return response;
                }

                SetPriceCodeValidation(contextData.GroupId, id);

                priceDetailsToInsert.Id = id;
                response.Object = priceDetailsToInsert;
                response.Status.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in PriceDetails. contextData:{contextData.ToString()}, name:{priceDetailsToInsert.Name}.", ex);
            }

            return response;
        }

        private static List<PriceDTO> ConvertPriceDetails(IEnumerable<Price> prices)
        {
            return prices?.Select(p => new PriceDTO
            {
                Price = p.m_dPrice,
                CountryId = p.countryId,
                Currency = new CurrencyDTO
                {
                    CurrencyId = p.m_oCurrency.m_nCurrencyID
                }
            }).ToList();
        }

        public Status Delete(ContextData contextData, long id)
        {
            Status result = new Status();

            if (!_repository.IsPriceCodeExistsById(contextData.GroupId, id))
            {
                result.Set(eResponseStatus.PriceDetailsDoesNotExist, $"Price details {id} does not exist");
                return result;
            }

            if (!_repository.DeletePriceDetails(contextData.GroupId, id, contextData.UserId.Value))
            {
                log.Error($"Error while DeletePriceCode. contextData: {contextData.ToString()}.");
                result.Set(eResponseStatus.Error);
                return result;
            }

            SetPriceCodeValidation(contextData.GroupId, id);

            result.Set(eResponseStatus.OK);
            return result;
        }

        private Tuple<List<PriceDetails>, bool> GetGroupPriceCodes(Dictionary<string, object> funcParams)
        {
            List<PriceDetails> priceDetailsList = null;

            try
            {
                if (funcParams != null && funcParams.Count == 1 && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    List<PriceDetailsDTO> priceDetailsDTOList = _repository.GetPriceCodesDTO(groupId.Value);

                    if (priceDetailsDTOList != null)
                    {
                        priceDetailsList = new List<PriceDetails>();
                        PriceDetails priceDetails = null;

                        foreach (var priceDetailsDTO in priceDetailsDTOList)
                        {
                            priceDetails = new PriceDetails()
                            {
                                Name = priceDetailsDTO.Name,
                                Id = priceDetailsDTO.Id
                            };

                            if (priceDetailsDTO.Prices?.Count > 0)
                            {
                                priceDetails.Prices = new List<Price>();
                                Price price = null;
                                foreach (var priceDTO in priceDetailsDTO.Prices)
                                {
                                    price = new Price()
                                    {
                                        m_dPrice = priceDTO.Price,
                                        countryId = priceDTO.CountryId
                                    };

                                    price.m_oCurrency.InitializeById(priceDTO.Currency.CurrencyId);
                                    priceDetails.Prices.Add(price);
                                }
                            }

                            priceDetailsList.Add(priceDetails);
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error(string.Format("GetGroupPriceCodes failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            bool res = priceDetailsList != null;

            return new Tuple<List<PriceDetails>, bool>(priceDetailsList, res);
        }

        private void SetPriceCodeValidation(int groupId, long priceDetailsId)
        {
            // invalidation keys
            string invalidationKey = LayeredCacheKeys.GetGroupPriceCodesInvalidationKey(groupId);
            if (_layeredCache.SetInvalidationKey(invalidationKey))
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

        public bool IsPriceCodeExist(int groupId, long priceCodeId)
        {
            string key = LayeredCacheKeys.GetGroupPriceCodesKey(groupId);

            var funcParams = new Dictionary<string, object>() { { "groupId", groupId } };
            List<PriceDetails> priceCodes = null;
            _layeredCache.Get(key, ref priceCodes, GetGroupPriceCodes, funcParams, groupId,
                LayeredCacheConfigNames.GET_GROUP_PRICE_CODES_LAYERED_CACHE_CONFIG_NAME,
                new List<string>() {LayeredCacheKeys.GetGroupPriceCodesInvalidationKey(groupId)});

            PriceDetails priceDetails = null;

            if (priceCodes?.Count > 0)
            {
                priceDetails = priceCodes.FirstOrDefault(pc => pc.Id == priceCodeId);
            }

            return priceDetails != null && priceDetails.Id != 0;
        }
    }
}