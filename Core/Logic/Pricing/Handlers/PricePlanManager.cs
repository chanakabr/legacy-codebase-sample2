using ApiObjects.Base;
using ApiObjects.Pricing;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.GroupManagers;
using Core.GroupManagers.Adapters;
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
    public interface IPricePlanManager
    {
        GenericListResponse<PricePlan> GetPricePlans(int groupId, List<long> pricePlanIds);
    }

    public class PricePlanManager : IPricePlanManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<PricePlanManager> lazy = new Lazy<PricePlanManager>(() =>
                                    new PricePlanManager(PricingDAL.Instance,
                                        LayeredCache.Instance,
                                        PriceDetailsManager.Instance,
                                        DiscountDetailsManager.Instance,
                                        GroupSettingsManagerAdapter.Instance                                 
                                    ),
            LazyThreadSafetyMode.PublicationOnly);

        public static PricePlanManager Instance => lazy.Value;

        private readonly IPricePlanRepository _repository;
        private readonly ILayeredCache _layeredCache;
        private readonly IPriceDetailsManager _priceDetailsManager;
        private readonly IDiscountDetailsManager _discountDetailsManager;
        private readonly IGroupSettingsManager _groupSettingsManager;

        public PricePlanManager(IPricePlanRepository pricePlanRepository,
                                ILayeredCache layeredCache,
                                IPriceDetailsManager priceDetailsManager,
                                IDiscountDetailsManager discountDetailsManager,
                                IGroupSettingsManager groupSettingsManager)
        {
            _repository = pricePlanRepository;
            _layeredCache = layeredCache;
            _priceDetailsManager = priceDetailsManager;
            _discountDetailsManager = discountDetailsManager;
            _groupSettingsManager = groupSettingsManager;
        }

        public GenericListResponse<PricePlan> GetPricePlans(int groupId, List<long> pricePlanIds = null)
        {
            var response = new GenericListResponse<PricePlan>();
            string key = LayeredCacheKeys.GetGroupPricePlansKey(groupId);
            var funcParams = new Dictionary<string, object>() { { "groupId", groupId } };
            List<PricePlan> PricePlans = null;

            if (!_layeredCache.Get(key, ref PricePlans, GetPricePlans, funcParams, groupId,
                LayeredCacheConfigNames.GET_GROUP_PRICE_PLAN_LAYERED_CACHE_CONFIG_NAME, new List<string>() { LayeredCacheKeys.GetGroupPricePlanInvalidationKey(groupId) }))
            {
                log.Error($"faild to GetPricePlans from layeredCache for groupId:{groupId}.");
                return response;
            }
            if (PricePlans != null)
            {
                response.Objects = PricePlans;
            }
            if (pricePlanIds?.Count > 0 && response.Objects?.Count > 0)
            {
                response.Objects = response.Objects.Where(pp => pricePlanIds.Contains(pp.Id.Value)).ToList();
            }

            response.TotalItems = response.Objects != null ? response.Objects.Count : 0;
            response.Status.Set(eResponseStatus.OK);

            return response;
        }

        private GenericResponse<PricePlan> GetPricePlane(int groupId, long id)
        {
            GenericResponse<PricePlan> response = new GenericResponse<PricePlan>();
            var pricePlanList = GetPricePlans(groupId, new List<long>() { id });

            if (!pricePlanList.HasObjects())
            {
                response.SetStatus(eResponseStatus.PricePlanDoesNotExist, $"Price plan {id} does not exist");
                return response;
            }

            response.Object = pricePlanList.Objects[0];
            response.SetStatus(eResponseStatus.OK);
            return response;
        }

        public GenericResponse<PricePlan> Update(ContextData contextData, int id, PricePlan pricePlanToUpdate)
        {
            GenericResponse<PricePlan> response = new GenericResponse<PricePlan>();
            if (!_groupSettingsManager.IsOpc(contextData.GroupId))
            {
                response.SetStatus(eResponseStatus.AccountIsNotOpcSupported, eResponseStatus.AccountIsNotOpcSupported.ToString());
                return response;
            }
            var PricePlanResponse = GetPricePlane(contextData.GroupId, id);

            if (!PricePlanResponse.HasObject())
            {
                return PricePlanResponse;
            }

            var oldPricePlan = PricePlanResponse.Object;
            Status validate = Validate(contextData.GroupId, pricePlanToUpdate);

            if (!validate.IsOkStatusCode())
            {
                PricePlanResponse.SetStatus(validate);
                return PricePlanResponse;
            }

            Boolean shouldUpdate = pricePlanToUpdate.IsNeedToUpdate(oldPricePlan);
            pricePlanToUpdate.Id = id;

            if (shouldUpdate)
            {
                int updatedRow = _repository.UpdatePricePlan(contextData.GroupId, pricePlanToUpdate, id, contextData.UserId.Value);

                if (updatedRow > 0)
                {
                    SetPricePlanInvalidation(contextData.GroupId);
                    response.Object = pricePlanToUpdate;
                    response.SetStatus(eResponseStatus.OK);
                }
            }
            else
            {
                response.Object = pricePlanToUpdate;
                response.SetStatus(eResponseStatus.OK);
            }

            return response;
        }

        public Status Delete(ContextData contextData, long id)
        {
            Status result = new Status();
            if (!_groupSettingsManager.IsOpc(contextData.GroupId))
            {
                result.Set(eResponseStatus.AccountIsNotOpcSupported, eResponseStatus.AccountIsNotOpcSupported.ToString());
                return result;
            }

            var PricePlanResponse = GetPricePlane(contextData.GroupId, id);
            if (!PricePlanResponse.HasObject())
            {
                result.Set(PricePlanResponse.Status);
                return result;
            }

            if (!_repository.DeletePricePlan(contextData.GroupId, id, contextData.UserId.Value))
            {
                log.Error($"Error while DeletePricePlan. pricePlan id:{id}, contextData: {contextData}.");
                result.Set(eResponseStatus.Error);
                return result;
            }

            SetPricePlanInvalidation(contextData.GroupId);
            result.Set(eResponseStatus.OK);

            return result;
        }

        public GenericResponse<PricePlan> Add(ContextData contextData, PricePlan pricePlanToInsert)
        {
            var response = new GenericResponse<PricePlan>();
            if (!_groupSettingsManager.IsOpc(contextData.GroupId))
            {
                response.SetStatus(eResponseStatus.AccountIsNotOpcSupported, eResponseStatus.AccountIsNotOpcSupported.ToString());
                return response;
            }
            Status validate = Validate(contextData.GroupId, pricePlanToInsert);
            if (!validate.IsOkStatusCode())
            {
                response.SetStatus(validate);
                return response;
            }
            long id = _repository.InsertPricePlan(contextData.GroupId, pricePlanToInsert, contextData.UserId.Value);
            if (id == 0)
            {
                log.Error($"Error while InsertPricePlan. contextData: {contextData}, pricePlan name:{pricePlanToInsert.Name}..");
                return response;
            }

            SetPricePlanInvalidation(contextData.GroupId);
            pricePlanToInsert.Id = id;
            response.Object = pricePlanToInsert;
            response.Status.Set(eResponseStatus.OK);

            return response;
        }

        public void SetPricePlanInvalidation(int groupId)
        {
            // invalidation keys
            string invalidationKey = LayeredCacheKeys.GetGroupPricePlanInvalidationKey(groupId);

            if (!_layeredCache.SetInvalidationKey(invalidationKey))
            {
                log.Error($"Failed to set invalidation key for PricePlan codes. key = {invalidationKey}");
            }
        }

        private Status Validate(int groupId, PricePlan pricePlan)
        {
            if (pricePlan.PriceDetailsId.HasValue)
            {
                var currPriceDetails = _priceDetailsManager.GetPriceDetailsById(groupId, pricePlan.PriceDetailsId.Value);
                if (!currPriceDetails.HasObject())
                {
                    return currPriceDetails.Status;
                }
            }

            if (pricePlan.DiscountId.HasValue)
            {
                var disocuntDetailes = _discountDetailsManager.GetDiscountDetailsById(groupId, pricePlan.DiscountId.Value);
                if (!disocuntDetailes.HasObject())
                {
                    return disocuntDetailes.Status;
                }
            }

            return Status.Ok;
        }

        private Tuple<List<PricePlan>, bool> GetPricePlans(Dictionary<string, object> funcParams)
        {
            List<PricePlan> pricePlan = null;
            if (funcParams != null && funcParams.Count == 1 && funcParams.ContainsKey("groupId"))
            {
                int? groupId = funcParams["groupId"] as int?;

                DataTable pricePlans = _repository.GetPricePlansDT(groupId.Value);

                if (pricePlans?.Rows.Count > 0)
                {
                    pricePlan = Utils.BuildPricePlanFromDataTable(pricePlans);
                }
            }
            bool res = pricePlan != null;

            return new Tuple<List<PricePlan>, bool>(pricePlan, res);
        }

    }
}