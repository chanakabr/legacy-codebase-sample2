using ApiObjects.Base;
using ApiObjects.Response;
using Core.Pricing;
using KLogMonitor;
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

namespace ApiLogic.Pricing.Handlers
{
    public class DiscountDetailsManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<DiscountDetailsManager> lazy = new Lazy<DiscountDetailsManager>(() => new DiscountDetailsManager(
            PricingDAL.Instance, Price.Instance, LayeredCache.Instance), LazyThreadSafetyMode.PublicationOnly);
        public static DiscountDetailsManager Instance => lazy.Value;

        private readonly IDiscountDetailsRepository _repository;
        private readonly IPrice _price;
        private readonly ILayeredCache _layeredCache;

        public DiscountDetailsManager(IDiscountDetailsRepository repository, IPrice price, ILayeredCache layeredCache)
        {
            _price = price;
            _repository = repository;
            _layeredCache = layeredCache;
        }

        public GenericResponse<DiscountDetails> Add(ContextData contextData, DiscountDetails discountDetailsToInsert)
        {
            var response = new GenericResponse<DiscountDetails>();

            try
            {
                discountDetailsToInsert.MultiCurrencyDiscounts.ForEach(discount =>
                {
                    discount.m_oCurrency.m_nCurrencyID = _price.InitializeByCD3(discount.m_oCurrency.m_sCurrencyCD3, discount.m_dPrice).m_oCurrency.m_nCurrencyID;
                });

                List<DiscountDTO> discounts = DiscountDetails.ConvertToDtos(discountDetailsToInsert.MultiCurrencyDiscounts.Skip(1).ToList());

                long id = _repository.InsertDiscountDetails(contextData.GroupId, discountDetailsToInsert.Name, discountDetailsToInsert.MultiCurrencyDiscounts[0].m_dPrice,
                    discountDetailsToInsert.MultiCurrencyDiscounts[0].Percentage, discountDetailsToInsert.MultiCurrencyDiscounts[0].m_oCurrency.m_nCurrencyID,
                    discountDetailsToInsert.StartDate, discountDetailsToInsert.EndDate, contextData.UserId.Value, discounts, discountDetailsToInsert.WhenAlgoType, discountDetailsToInsert.WhenAlgoTimes);

                if (id == 0)
                {
                    log.Error($"Error while InsertDiscountDetails. contextData: {contextData.ToString()}.");
                    return response;
                }

                SetDiscountCodeInvalidation(contextData.GroupId, id);

                discountDetailsToInsert.Id = id;
                response.Object = discountDetailsToInsert;
                response.Status.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in discountDetails. contextData:{contextData.ToString()}, name:{discountDetailsToInsert.Name}.", ex);
            }

            return response;
        }

        public Status Delete(ContextData contextData, long id)
        {
            Status result = new Status();

            try
            {
                if (!IsDiscountCodeExist(contextData.GroupId, id))
                {
                    result.Set(eResponseStatus.DiscountCodeNotExist, $"Discount details {id} does not exist");
                    return result;
                }

                if (!_repository.DeleteDiscountDetails(contextData.GroupId, id, contextData.UserId.Value))
                {
                    log.Error($"Error while DeleteDiscountCode. contextData: {contextData.ToString()}.");
                    result.Set(eResponseStatus.Error);
                    return result;
                }

                SetDiscountCodeInvalidation(contextData.GroupId, id);

                result.Set(eResponseStatus.OK);
            }

            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in  delete discountDetails. contextData:{contextData.ToString()}, id:{id}.", ex);
            }

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

            string key = LayeredCacheKeys.GetDiscountsKey(groupId);

            var funcParams = new Dictionary<string, object>() { { "groupId", groupId } };
            List<DiscountDetails> discountDetails = null;
            _layeredCache.Get(key, ref discountDetails, GetGroupDiscounts, funcParams, groupId,
                LayeredCacheConfigNames.GET_GROUP_DISCOUNTS_LAYERED_CACHE_CONFIG_NAME, new List<string>()
                { LayeredCacheKeys.GetGroupDiscountsInvalidationKey(groupId) });

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

        private Tuple<List<DiscountDetails>, bool> GetGroupDiscounts(Dictionary<string, object> funcParams)
        {
            List<DiscountDetails> discountDetails = null;

            try
            {
                if (funcParams != null && funcParams.Count == 1 && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    DataTable discountsDt = PricingDAL.GetGroupDiscounts(groupId.Value);
                    if (discountsDt != null)
                    {
                        discountDetails = Utils.BuildDiscountsFromDataTable(discountsDt);
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
            if (_layeredCache.SetInvalidationKey(invalidationKey))
            {
                log.ErrorFormat("Failed to set invalidation key for group discount codes. key = {0}", invalidationKey);
            }

            invalidationKey = LayeredCacheKeys.GetDiscountCodeInvalidationKey(groupId, (int)discountDetailsId);
            if (!_layeredCache.SetInvalidationKey(invalidationKey))
            {
                log.ErrorFormat("Failed to set invalidation key for discount code. key = {0}", invalidationKey);
            }
        }

        private bool IsDiscountCodeExist(int groupId, long discountCodeId)
        {
            string key = LayeredCacheKeys.GetGroupDiscountCodesKey(groupId);

            var funcParams = new Dictionary<string, object>() { { "groupId", groupId } };
            List<DiscountDetails> discountCodes = null;
            _layeredCache.Get(key, ref discountCodes, GetGroupDiscounts, funcParams, groupId,
                LayeredCacheConfigNames.GET_GROUP_DISCOUNTS_LAYERED_CACHE_CONFIG_NAME, new List<string>() { LayeredCacheKeys.GetGroupDiscountsInvalidationKey(groupId) });

            DiscountDetails discountDetails = null;

            if (discountCodes != null && discountCodes.Count > 0)
            {
                discountDetails = discountCodes.FirstOrDefault(dc => dc.Id == discountCodeId);
            }

            return discountDetails != null && discountDetails.Id != 0;
        }
    }
}
