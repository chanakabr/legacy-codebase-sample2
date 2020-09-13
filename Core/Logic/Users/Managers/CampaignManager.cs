using ApiLogic.Base;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Reflection;
using Campaign = ApiObjects.Campaign;
using DAL;
using System.Linq;
using ApiObjects.EventBus;
using TVinciShared;
using System.Collections.Generic;
using CachingProvider.LayeredCache;

namespace ApiLogic.Users.Managers
{
    public class CampaignManager : ICrudHandler<Campaign, long>
    {
        private const int MAX_TRIGGER_CAMPAIGNS = 100;
        private const int MAX_BATCH_CAMPAIGNS = 100;
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly Lazy<CampaignManager> lazy = new Lazy<CampaignManager>(() => new CampaignManager());
        public static CampaignManager Instance { get { return lazy.Value; } }

        private CampaignManager() { }

        public Status Delete(ContextData contextData, long id)
        {
            var response = Status.Error;

            try
            {
                var campaignToDeleteResponse = Get(contextData, id);
                if (!campaignToDeleteResponse.HasObject())
                {
                    response.Set(campaignToDeleteResponse.Status);
                    return response;
                }

                if (campaignToDeleteResponse.Object.State != ObjectState.INACTIVE)
                {
                    response.Set(eResponseStatus.CanDeleteOnlyInactiveCampaign, "Can delete only inactive campaign");
                }

                if (!PricingDAL.DeleteCampaign(contextData.GroupId, id))
                {
                    response.Set(eResponseStatus.Error, "Error while deleting Campaign");
                    return response;
                }

                SetInvalidationKeys(contextData, campaignToDeleteResponse.Object);
                response.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"Error while delete Campaign. contextData: {contextData}, ex: {ex}", ex);
            }

            return response;
        }

        public GenericResponse<Campaign> Get(ContextData contextData, long id)
        {
            var response = new GenericResponse<Campaign>();
            
            var filter = new CampaignIdInFilter() { IdIn = new List<long>() { id } };
            var listCampaingsByIdsResponse = ListCampaingsByIds(contextData, filter);

            if (!listCampaingsByIdsResponse.IsOkStatusCode())
            {
                response.SetStatus(listCampaingsByIdsResponse.Status);
                return response;
            }

            if (!listCampaingsByIdsResponse.HasObjects())
            {
                response.SetStatus(eResponseStatus.CampaignDoesNotExist, $"Campaign not found, id: {id}");
                return response;
            }
            
            response.Object = listCampaingsByIdsResponse.Objects.FirstOrDefault();
            response.SetStatus(eResponseStatus.OK);
            
            return response;
        }

        public GenericListResponse<TriggerCampaign> ListTriggerCampaigns(ContextData contextData, TriggerCampaignFilter filter, CorePager pager = null)
        {
            var response = new GenericListResponse<TriggerCampaign>();

            response.Objects = ListCampaignsByType<TriggerCampaign>(contextData, filter);

            if (response.Objects == null)
            {
                response.SetStatus(eResponseStatus.Error, "error while searching trigger campaigns");
                return response;
            }

            response.SetStatus(eResponseStatus.OK);

            if (filter.Service.HasValue)
            {
                response.Objects = response.Objects.Where(x => x.Service == filter.Service.Value).ToList();
            }

            if (filter.Action.HasValue)
            {
                response.Objects = response.Objects.Where(x => x.Action == filter.Action.Value).ToList();
            }

            if (pager != null)
            {
                response.TotalItems = response.Objects.Count;
                response.Objects = pager.PageSize > 0 ? response.Objects.Skip(pager.PageIndex * pager.PageSize).Take(pager.PageSize).ToList() : response.Objects;
            }

            return response;
        }

        public GenericListResponse<BatchCampaign> ListBatchCampaigns(ContextData contextData, BatchCampaignFilter filter, CorePager pager = null)
        {
            var response = new GenericListResponse<BatchCampaign>();

            response.Objects = ListCampaignsByType<BatchCampaign>(contextData, filter);

            if (response.Objects == null)
            {
                response.SetStatus(eResponseStatus.Error, "error while searching batch campaigns");
                return response;
            }

            response.SetStatus(eResponseStatus.OK);

            if (pager != null)
            {
                response.TotalItems = response.Objects.Count;
                response.Objects = pager.PageSize > 0 ? response.Objects.Skip(pager.PageIndex * pager.PageSize).Take(pager.PageSize).ToList() : response.Objects;
            }

            return response;
        }

        public GenericListResponse<Campaign> ListCampaingsByIds(ContextData contextData, CampaignIdInFilter filter)
        {
            var response = new GenericListResponse<Campaign>
            {
                Objects = ListCampaignByIds(contextData, filter.IdIn)
            };
            if (response.Objects != null)
            {
                response.SetStatus(eResponseStatus.OK);
            }

            return response;
        }

        public GenericListResponse<Campaign> SearchCampaigns(ContextData contextData, CampaignSearchFilter filter, CorePager pager = null)
        {
            var response = new GenericListResponse<Campaign>();

            var triggerResponse = ListTriggerCampaigns(contextData, filter as TriggerCampaignFilter);
            if (!triggerResponse.IsOkStatusCode())
            {
                response.SetStatus(triggerResponse.Status);
                return response;
            }

            var batchResponse = ListBatchCampaigns(contextData, filter as BatchCampaignFilter);
            if (!batchResponse.IsOkStatusCode())
            {
                response.SetStatus(batchResponse.Status);
                return response;
            }

            response.SetStatus(eResponseStatus.OK);
            response.Objects.AddRange(triggerResponse.Objects);
            response.Objects.AddRange(batchResponse.Objects);

            if (pager != null)
            {
                response.TotalItems = response.Objects.Count;
                response.Objects = pager.PageSize > 0 ? response.Objects.Skip(pager.PageIndex * pager.PageSize).Take(pager.PageSize).ToList() : response.Objects;
            }

            return response;
        }

        public GenericResponse<Campaign> AddTriggerCampaign(ContextData contextData, TriggerCampaign campaignToAdd)
        {
            var response = new GenericResponse<Campaign>();
            
            try
            {
                var triggerCampaignFilter = new TriggerCampaignFilter() { StateEqual = ObjectState.ACTIVE };
                var triggerCampaigns = ListTriggerCampaigns(contextData, triggerCampaignFilter);
                if (triggerCampaigns.HasObjects() && triggerCampaigns.Objects.Count >= MAX_TRIGGER_CAMPAIGNS)
                {
                    response.SetStatus(eResponseStatus.ActiveCampaignsExceededMaxSize, "Active trigger campaigns Exceeded Max Size");
                    return response;
                }

                var validateStatus = ValidateCampaign(contextData, campaignToAdd);
                if (!validateStatus.IsOkStatusCode())
                {
                    response.SetStatus(validateStatus);
                    return response;
                }

                campaignToAdd.State = ObjectState.INACTIVE;
                campaignToAdd.CreateDate = DateUtils.GetUtcUnixTimestampNow();
                campaignToAdd.UpdateDate = campaignToAdd.CreateDate;

                var insertedCampaign = PricingDAL.AddCampaign(campaignToAdd, contextData);
                if (insertedCampaign?.Id > 0)
                {
                    response.Object = insertedCampaign;
                    SetInvalidationKeys(contextData, insertedCampaign);
                    response.SetStatus(eResponseStatus.OK);
                }
                else
                {
                    response.SetStatus(eResponseStatus.Error, $"Error while saving TriggerCampaign");
                }
            }
            catch (Exception ex)
            {
                log.Error($"Error while adding new TriggerCampaign. contextData: {contextData}, ex: {ex}", ex);
            }

            return response;
        }

        public GenericResponse<Campaign> AddBatchCampaign(ContextData contextData, BatchCampaign campaignToAdd)
        {
            var response = new GenericResponse<Campaign>();

            try
            {
                var campaignFilter = new BatchCampaignFilter() { StateEqual = ObjectState.ACTIVE };
                var campaigns = ListBatchCampaigns(contextData, campaignFilter);
                if (campaigns.HasObjects() && campaigns.Objects.Count >= MAX_BATCH_CAMPAIGNS)
                {
                    response.SetStatus(eResponseStatus.ActiveCampaignsExceededMaxSize, "Active batch campaigns Exceeded Max Size");
                    return response;
                }

                var validateStatus = ValidateCampaign(contextData, campaignToAdd);
                if (!validateStatus.IsOkStatusCode())
                {
                    response.SetStatus(validateStatus);
                    return response;
                }

                campaignToAdd.State = ObjectState.INACTIVE;
                campaignToAdd.CreateDate = DateUtils.ToUtcUnixTimestampSeconds(DateTime.UtcNow);
                campaignToAdd.UpdateDate = campaignToAdd.CreateDate;

                var insertedCampaign = PricingDAL.AddCampaign(campaignToAdd, contextData);
                if (insertedCampaign?.Id > 0)
                {
                    response.Object = insertedCampaign;
                    SetInvalidationKeys(contextData, insertedCampaign);
                    response.SetStatus(eResponseStatus.OK);
                }
                else
                {
                    response.SetStatus(eResponseStatus.Error, $"Error while saving BatchCampaign");
                }
            }
            catch (Exception ex)
            {
                log.Error($"Error while adding new BatchCampaign. contextData: {contextData}, ex: {ex}", ex);
            }

            return response;
        }

        public GenericResponse<Campaign> UpdateTriggerCampaign(ContextData contextData, TriggerCampaign campaignToUpdate)
        {
            var response = new GenericResponse<Campaign>();
            try
            {
                var _response = Get(contextData, campaignToUpdate.Id);
                if (!_response.IsOkStatusCode() || !(_response.Object is TriggerCampaign))
                {
                    response.SetStatus(eResponseStatus.Error, $"Couldn't update TriggerCampaign: {campaignToUpdate.Id}");
                    return response;
                }

                var campaign = response.Object as TriggerCampaign;

                if (campaign == null)
                {
                    response.SetStatus(eResponseStatus.Error, $"Couldn't update TriggerCampaign: {campaign.Id}, campaign not found");
                    return response;
                }

                var validateStatus = ValidateCampaign(contextData, campaignToUpdate);
                if (!validateStatus.IsOkStatusCode())
                {
                    response.SetStatus(validateStatus);
                    return response;
                }

                campaignToUpdate.FillEmpty(campaign);
                campaignToUpdate.UpdateDate = DateUtils.GetUtcUnixTimestampNow();

                if (PricingDAL.Update_Campaign(campaignToUpdate, contextData))
                {
                    response.Object = campaignToUpdate;
                    SetInvalidationKeys(contextData, campaignToUpdate);
                    response.SetStatus(eResponseStatus.OK);
                }
                else
                    response.SetStatus(eResponseStatus.Error, $"Error Updating TriggerCampaign: [{campaignToUpdate.Id}] for group: {contextData.GroupId}");
            }
            catch (Exception ex)
            {
                log.Error($"Error while updating new TriggerCampaign({campaignToUpdate.Id}). contextData: {contextData}, ex: {ex}", ex);
            }

            return response;
        }

        public GenericResponse<Campaign> UpdateBatchCampaign(ContextData contextData, BatchCampaign campaignToUpdate)
        {
            var response = new GenericResponse<Campaign>();
            try
            {
                var _response = Get(contextData, campaignToUpdate.Id);
                if (!_response.IsOkStatusCode() || !(_response.Object is TriggerCampaign))
                {
                    response.SetStatus(eResponseStatus.Error, $"Couldn't update BatchCampaign: {campaignToUpdate.Id}");
                    return response;
                }

                var campaign = response.Object as TriggerCampaign;

                if (campaign == null)
                {
                    response.SetStatus(eResponseStatus.Error, $"Couldn't update BatchCampaign: {campaign.Id}, campaign not found");
                    return response;
                }

                var validateStatus = ValidateCampaign(contextData, campaignToUpdate);
                if (!validateStatus.IsOkStatusCode())
                {
                    response.SetStatus(validateStatus);
                    return response;
                }

                campaignToUpdate.FillEmpty(campaign);
                campaignToUpdate.UpdateDate = DateUtils.GetUtcUnixTimestampNow();

                if (PricingDAL.Update_Campaign(campaignToUpdate, contextData))
                {
                    response.Object = campaignToUpdate;
                    SetInvalidationKeys(contextData, campaignToUpdate);
                    response.SetStatus(eResponseStatus.OK);
                }
                else
                    response.SetStatus(eResponseStatus.Error, $"Error Updating BatchCampaign: [{campaignToUpdate.Id}] for group: {contextData.GroupId}");
            }
            catch (Exception ex)
            {
                log.Error($"Error while updating new BatchCampaign({campaignToUpdate.Id}). contextData: {contextData}, ex: {ex}", ex);
            }

            return response;
        }

        public GenericResponse<Campaign> SetState(ContextData contextData, long id, ObjectState newState)
        {
            var response = new GenericResponse<Campaign>();

            try
            {
                var campaign = Get(contextData, id);

                if (!campaign.IsOkStatusCode())
                {
                    response.SetStatus(campaign.Status);
                    return response;
                }

                var validationStatus = ValidateStateChange(campaign.Object, newState);
                if (!validationStatus.IsOkStatusCode())
                {
                    response.SetStatus(validationStatus);
                    return response;
                }

                if (PricingDAL.Update_Campaign(campaign.Object, contextData))
                {
                    SetInvalidationKeys(contextData, campaign.Object);
                    response.Object = Get(contextData, campaign.Object.Id)?.Object;
                    response.SetStatus(eResponseStatus.OK);
                }
                else
                    response.SetStatus(eResponseStatus.Error, $"Error Updating Campaign: [{id}] for group: {contextData.GroupId}");

            }
            catch (Exception ex)
            {
                log.Error($"Failed setting campaign: {id} state to: {newState}, ex: {ex}", ex);
                response.SetStatus(eResponseStatus.Error, $"Campaign: {id} wasn't updated");
            }

            return response;
        }

        private Status ValidateStateChange(Campaign campaign, ObjectState newState)
        {
            var response = new Status(eResponseStatus.OK);

            if (campaign.State == ObjectState.ARCHIVE)
            {
                response.Set(eResponseStatus.Error, "Can't update archived campaign");
                return response;
            }
            if (campaign.State == newState)
            {
                response.Set(eResponseStatus.Error, $"Campaign: {campaign.Id} already in state: {newState}");
                return response;
            }
            if (newState == ObjectState.ACTIVE && campaign.EndDate <= DateUtils.GetUtcUnixTimestampNow())
            {
                response.Set(eResponseStatus.Error, $"Campaign: {campaign.Id} was ended");
                return response;
            }

            campaign.State = newState;

            return response;
        }

        private Status ValidateCampaign(ContextData contextData, Campaign campaignToUpdate)
        {
            //TODO SHIR / MATAN - ValidateParametersForUpdate
            var status = Status.Error;
            if (campaignToUpdate.Promotion != null)
            {
                var discounts = Core.Pricing.Module.GetValidDiscounts(contextData.GroupId);
                if (!discounts.HasObjects() || !discounts.Objects.Any(x => x.Id == campaignToUpdate.Promotion.DiscountModuleId))
                {
                    status.Set(eResponseStatus.DiscountCodeNotExist);
                    return status;
                }
            }

            status.Set(eResponseStatus.OK);
            return status;
        }

        private void SetInvalidationKeys(ContextData contextData, Campaign campaign)
        {
            var type = campaign.GetType().Name;
            var invalidationKey = LayeredCacheKeys.GetGroupCampaignInvalidationKey(contextData.GroupId, type);
            LayeredCache.Instance.SetInvalidationKey(invalidationKey);
        }

        public void PublishTriggerCampaign(int groupId, int domainId, ICampaignObject eventObject, ApiService apiService, ApiAction apiAction)
        {
            var serviceEvent = new CampaignTriggerEvent()
            {
                RequestId = KLogger.GetRequestId(),
                GroupId = groupId,
                EventObject = eventObject,
                DomainId = domainId,
                ApiAction = (int)apiAction,
                ApiService = (int)apiService
            };

            var publisher = EventBus.RabbitMQ.EventBusPublisherRabbitMQ.GetInstanceUsingTCMConfiguration();
            publisher.Publish(serviceEvent);
        }

        private static List<T> ListCampaignsByType<T>(ContextData contextData, CampaignSearchFilter filter) where T : Campaign, new()
        {
            List<T> list = null;

            try
            {
                var type = typeof(T).Name;
                IEnumerable<CampaignDB> campaignsDB = null;
                var key = LayeredCacheKeys.GetGroupCampaignKey(contextData.GroupId, type);
                var cacheResult = LayeredCache.Instance.Get(key,
                                                            ref campaignsDB,
                                                            ListCampaignsByGroupIdDB,
                                                            new Dictionary<string, object>() { { "groupId", contextData.GroupId } },
                                                            contextData.GroupId,
                                                            LayeredCacheConfigNames.LIST_CAMPAIGNS_BY_GROUP_ID,
                                                            new List<string>() { LayeredCacheKeys.GetGroupCampaignInvalidationKey(contextData.GroupId, type) });

                if (campaignsDB != null && campaignsDB.Count() > 0)
                {
                    if (filter.StateEqual.HasValue)
                    {
                        campaignsDB = campaignsDB.Where(x => x.State <= filter.StateEqual.Value);
                    }

                    if (filter.StartDateGreaterThanOrEqual.HasValue)
                    {
                        campaignsDB = campaignsDB.Where(x => x.StartDate >= filter.StartDateGreaterThanOrEqual.Value);
                    }

                    if (filter.EndDateLessThanOrEqual.HasValue)
                    {
                        campaignsDB = campaignsDB.Where(x => x.EndDate <= filter.EndDateLessThanOrEqual.Value);
                    }

                    if (filter.IsActiveNow)
                    {
                        var utcNow = DateUtils.GetUtcUnixTimestampNow();
                        campaignsDB = campaignsDB.Where(x => x.StartDate <= utcNow && x.EndDate >= utcNow);
                    }

                    if (filter.HasPromotion.HasValue)
                    {
                        if (filter.HasPromotion.Value)
                        {
                            campaignsDB = campaignsDB.Where(x => x.HasPromotion);
                        }
                        else
                        {
                            campaignsDB = campaignsDB.Where(x => !x.HasPromotion);
                        }
                    }

                    var ids = campaignsDB.Select(x => x.Id).ToList();
                    list = PricingDAL.GetCampaignByType<T>(contextData.GroupId, ids);
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed to ListCampaignsByType contextData:{contextData}, ex: {ex}", ex);
            }

            return list;
        }

        private static List<Campaign> ListCampaignByIds(ContextData contextData, List<long> ids)
        {
            List<Campaign> list = null;

            try
            {
                list = PricingDAL.GetCampaignByIds(contextData.GroupId, ids);
            }
            catch (Exception ex)
            {
                log.Error($"Failed to ListCampaignByIds contextData:{contextData}, ex: {ex}", ex);
            }

            return list;
        }

        private static Tuple<IEnumerable<CampaignDB>, bool> ListCampaignsByGroupIdDB(Dictionary<string, object> arg)
        {
            IEnumerable<CampaignDB> list = null;

            try
            {
                var groupId = (int)arg["groupId"];
                list = PricingDAL.GetCampaignsByGroupId(groupId);
            }
            catch (Exception ex)
            {
                log.Error($"Failed ListCampaignsByGroupId group:[{arg["groupId"]}], ex: {ex}", ex);
            }

            return new Tuple<IEnumerable<CampaignDB>, bool>(list, list != null);
        }

        // TODO SHIR -.filter by state(lazy archive?) 
    }
}