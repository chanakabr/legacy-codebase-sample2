using ApiLogic.Users.Managers;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using System;
using System.Collections.Generic;
using WebAPI.Clients;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Models.General;
using WebAPI.ModelsValidators;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("campaign")]
    public class CampaignController : IKalturaController
    {
        /// <summary>
        /// Set campaign's state
        /// </summary>
        /// <param name="campaignId">campaign Id</param>
        /// <param name="newState">new campaign state</param>
        /// <returns>Kaltura campaign object</returns>
        [Action("setState")]
        [ApiAuthorize]
        [Throws(eResponseStatus.CampaignDoesNotExist)]
        [Throws(eResponseStatus.ExceededMaxCapacity)]
        [Throws(eResponseStatus.InvalidCampaignState)]
        [Throws(eResponseStatus.CampaignStateUpdateNotAllowed)]
        [Throws(eResponseStatus.InvalidCampaignEndDate)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public static void SetState(long campaignId, KalturaObjectState newState)
        {
            var response = new GenericResponse<KalturaCampaign>();
            response.SetStatus(eResponseStatus.OK);
            var contextData = Managers.Models.KS.GetContextData();

            try
            {
                var _newState = AutoMapper.Mapper.Map<CampaignState>(newState);
                Func<Status> coreFunc = () => CampaignManager.Instance.SetState(contextData, campaignId, _newState);
                Clients.ClientUtils.GetResponseStatusFromWS(coreFunc);
            }
            catch (Exceptions.ClientException ex)
            {
                Utils.ErrorUtils.HandleClientException(ex);
            }
        }

        /// <summary>
        /// Add new Campaign
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="objectToAdd">Campaign Object to add</param>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.DiscountCodeNotExist)]
        [Throws(eResponseStatus.NotExist)]
        [Throws(eResponseStatus.ChannelDoesNotExist)]
        [Throws(eResponseStatus.MediaFileTypeDoesNotExist)]
        [Throws(eResponseStatus.CouponGroupNotExist)]
        [Throws(eResponseStatus.EntityIsNotAssociatedWithShop)]
        [Throws(eResponseStatus.SegmentsIdsDoesNotExist)]
        [Throws(eResponseStatus.PpvModuleNotExist)]
        [Throws(eResponseStatus.CollectionNotExist)]
        [Throws(eResponseStatus.AssetUserRuleDoesNotExists)]
        static public KalturaCampaign Add(KalturaCampaign objectToAdd)
        {
            var contextData = KS.GetContextData();

            objectToAdd.ValidateForAdd();

            // call to manager and get response
            KalturaCampaign response;
            switch (objectToAdd)
            {
                case KalturaBatchCampaign c: response = AddBatchCampaign(contextData, c); break;
                case KalturaTriggerCampaign c: response = AddTriggerCampaign(contextData, c); break;
                default: throw new NotImplementedException($"Add for {objectToAdd.objectType} is not implemented");
            }

            return response;
        }

        private static KalturaBatchCampaign AddBatchCampaign(ContextData contextData, KalturaBatchCampaign batchCampaign)
        {
            Func<BatchCampaign, GenericResponse<BatchCampaign>> addFunc = (BatchCampaign objectToAdd) =>
                CampaignManager.Instance.AddCampaign(contextData, objectToAdd);

            var result = ClientUtils.GetResponseFromWS(batchCampaign, addFunc);

            return result;
        }

        private static KalturaTriggerCampaign AddTriggerCampaign(ContextData contextData, KalturaTriggerCampaign triggerCampaign)
        {
            Func<TriggerCampaign, GenericResponse<TriggerCampaign>> addFunc = (TriggerCampaign objectToAdd) =>
                CampaignManager.Instance.AddCampaign(contextData, objectToAdd);

            var result = ClientUtils.GetResponseFromWS(triggerCampaign, addFunc);

            return result;
        }

        /// <summary>
        /// Update existing Campaign
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">id of Campaign to update</param>
        /// <param name="objectToUpdate">Campaign Object to update</param>
        [Action("update")]
        [ApiAuthorize]
        [SchemeArgument("id", MinLong = 1)]
        [Throws(eResponseStatus.DiscountCodeNotExist)]
        [Throws(eResponseStatus.CampaignDoesNotExist)]
        [Throws(eResponseStatus.NotExist)]
        [Throws(eResponseStatus.ChannelDoesNotExist)]
        [Throws(eResponseStatus.MediaFileTypeDoesNotExist)]
        [Throws(eResponseStatus.CouponGroupNotExist)]
        [Throws(eResponseStatus.CampaignUpdateNotAllowed)]
        [Throws(eResponseStatus.EntityIsNotAssociatedWithShop)]
        [Throws(eResponseStatus.SegmentsIdsDoesNotExist)]
        [Throws(eResponseStatus.PpvModuleNotExist)]
        [Throws(eResponseStatus.CollectionNotExist)]
        [Throws(eResponseStatus.AssetUserRuleDoesNotExists)]
        static public KalturaCampaign Update(long id, KalturaCampaign objectToUpdate)
        {
            objectToUpdate.ValidateForUpdate();
            var contextData = KS.GetContextData();
            objectToUpdate.Id = id;

            // call to manager and get response
            KalturaCampaign response;
            switch (objectToUpdate)
            {
                case KalturaBatchCampaign c: response = UpdateBatchCampaign(contextData, c); break;
                case KalturaTriggerCampaign c: response = UpdateTriggerCampaign(contextData, c); break;
                default: throw new NotImplementedException($"Update for {objectToUpdate.objectType} is not implemented");
            }

            return response;
        }

        private static KalturaBatchCampaign UpdateBatchCampaign(ContextData contextData, KalturaBatchCampaign batchCampaign)
        {
            Func<BatchCampaign, GenericResponse<BatchCampaign>> addFunc = (BatchCampaign objectToUpdate) =>
                CampaignManager.Instance.UpdateBatchCampaign(contextData, objectToUpdate);

            var result = ClientUtils.GetResponseFromWS(batchCampaign, addFunc);

            return result;
        }

        private static KalturaTriggerCampaign UpdateTriggerCampaign(ContextData contextData, KalturaTriggerCampaign triggerCampaign)
        {
            Func<TriggerCampaign, GenericResponse<TriggerCampaign>> addFunc = (TriggerCampaign objectToUpdate) =>
                CampaignManager.Instance.UpdateTriggerCampaign(contextData, objectToUpdate);

            var result = ClientUtils.GetResponseFromWS(triggerCampaign, addFunc);

            return result;
        }

        /// <summary>
        /// Delete existing Campaign
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">Campaign identifier</param>
        [Action("delete")]
        [ApiAuthorize]
        [SchemeArgument("id", MinLong = 1)]
        [Throws(eResponseStatus.CampaignDoesNotExist)]
        [Throws(eResponseStatus.CanDeleteOnlyInactiveCampaign)]
        static public void Delete(long id)
        {
            var contextData = KS.GetContextData();
            Func<Status> deleteFunc = () => CampaignManager.Instance.Delete(contextData, id);
            ClientUtils.GetResponseStatusFromWS(deleteFunc);
        }

        /// <summary>
        /// Returns the list of available Campaigns
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="pager">Pager</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaCampaignListResponse List(KalturaCampaignFilter filter, KalturaFilterPager pager = null)
        {
            var contextData = KS.GetContextData();
            filter.Validate(contextData);

            if (pager == null)
            {
                pager = new KalturaFilterPager();
            }
            var corePager = AutoMapper.Mapper.Map<CorePager>(pager);

            KalturaGenericListResponse<KalturaCampaign> result;
            switch (filter)
            {
                case KalturaTriggerCampaignSearchFilter f: result = ListByTriggerCampaignSearchFilter(contextData, corePager, f); break;
                case KalturaBatchCampaignSearchFilter f: result = ListByBatchCampaignSearchFilter(contextData, corePager, f); break;
                case KalturaCampaignSegmentFilter f: result = ListByCampaignSegmentFilter(contextData, f); break;
                case KalturaCampaignSearchFilter f: result = ListByCampaignSearchFilter(contextData, corePager, f); break;
                case KalturaCampaignIdInFilter f: result = ListByCampaignIdInFilter(contextData, f); break;
                case KalturaCampaignFilter f: result = ListByCampaignSearchFilter(contextData, corePager, new KalturaCampaignSearchFilter()); break;
                default: throw new NotImplementedException($"List for {filter.objectType} is not implemented");
            }

            KalturaCampaignListResponse response = new KalturaCampaignListResponse
            {
                Objects = result.Objects,
                TotalCount = result.TotalCount
            };

            return response;
        }

        private static KalturaGenericListResponse<KalturaCampaign> ListByTriggerCampaignSearchFilter(ContextData contextData, CorePager pager, KalturaTriggerCampaignSearchFilter filter)
        {
            var coreFilter = AutoMapper.Mapper.Map<TriggerCampaignFilter>(filter);

            Func<GenericListResponse<TriggerCampaign>> listFunc = () =>
                CampaignManager.Instance.ListTriggerCampaigns(contextData, coreFilter, pager);

            KalturaGenericListResponse<KalturaTriggerCampaign> triggerCampaignResponse =
               ClientUtils.GetResponseListFromWS<KalturaTriggerCampaign, TriggerCampaign>(listFunc);

            var response = new KalturaGenericListResponse<KalturaCampaign>()
            {
                Objects = new List<KalturaCampaign>(triggerCampaignResponse.Objects),
                TotalCount = triggerCampaignResponse.TotalCount
            };
            
            return response;
        }

        private static KalturaGenericListResponse<KalturaCampaign> ListByBatchCampaignSearchFilter(ContextData contextData, CorePager pager, KalturaBatchCampaignSearchFilter filter)
        {
            var coreFilter = AutoMapper.Mapper.Map<BatchCampaignFilter>(filter);

            Func<GenericListResponse<BatchCampaign>> listFunc = () =>
                CampaignManager.Instance.ListBatchCampaigns(contextData, coreFilter, pager);

            KalturaGenericListResponse<KalturaBatchCampaign> batchCampaignResponse =
               ClientUtils.GetResponseListFromWS<KalturaBatchCampaign, BatchCampaign>(listFunc);

            var response = new KalturaGenericListResponse<KalturaCampaign>()
            {
                Objects = new List<KalturaCampaign>(batchCampaignResponse.Objects),
                TotalCount = batchCampaignResponse.TotalCount
            };
            
            return response;
        }

        private static KalturaGenericListResponse<KalturaCampaign> ListByCampaignSearchFilter(ContextData contextData, CorePager pager, KalturaCampaignSearchFilter filter)
        {
            var coreFilter = AutoMapper.Mapper.Map<CampaignSearchFilter>(filter);

            Func<GenericListResponse<Campaign>> listFunc = () =>
                CampaignManager.Instance.SearchCampaigns(contextData, coreFilter, pager);

            KalturaGenericListResponse<KalturaCampaign> response =
               ClientUtils.GetResponseListFromWS<KalturaCampaign, Campaign>(listFunc);

            return response;
        }

        private static KalturaGenericListResponse<KalturaCampaign> ListByCampaignIdInFilter(ContextData contextData, KalturaCampaignIdInFilter filter)
        {
            var coreFilter = AutoMapper.Mapper.Map<CampaignIdInFilter>(filter);
            coreFilter.IsAllowedToViewInactiveCampaigns =
                Utils.Utils.IsAllowedToViewInactiveAssets(contextData.GroupId, contextData.UserId.ToString(), true);

            Func<GenericListResponse<Campaign>> listFunc = () =>
                CampaignManager.Instance.ListCampaingsByIds(contextData, coreFilter);

            KalturaGenericListResponse<KalturaCampaign> response =
               ClientUtils.GetResponseListFromWS<KalturaCampaign, Campaign>(listFunc);

            return response;
        }

        private static KalturaGenericListResponse<KalturaCampaign> ListByCampaignSegmentFilter(ContextData contextData, KalturaCampaignSegmentFilter filter)
        {
            var coreFilter = AutoMapper.Mapper.Map<CampaignSegmentFilter>(filter);

            Func<GenericListResponse<Campaign>> listFunc = () =>
                CampaignManager.Instance.ListCampaignsBySegment(contextData, coreFilter);

            KalturaGenericListResponse<KalturaCampaign> response =
               ClientUtils.GetResponseListFromWS<KalturaCampaign, Campaign>(listFunc);

            return response;
        }
    }
}