using ApiLogic.Base;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Reflection;
using APILogic.ConditionalAccess;
using Campaign = ApiObjects.Campaign;
using DAL;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
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
            // TODO SHIR \ MATAN
            throw new NotImplementedException();
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

            var campaignsListResponse = SearchCampaignsByType<TriggerCampaign>(contextData, filter);

            if (!campaignsListResponse.IsOkStatusCode())
            {
                response.SetStatus(campaignsListResponse.Status);
                return response;
            }

            response.Objects = campaignsListResponse.Objects;
            response.SetStatus(eResponseStatus.OK);

            if (filter.Service.HasValue)
            {
                response.Objects = response.Objects.Where(x => x.Service == filter.Service.Value).ToList();
            }

            if (filter.Action.HasValue)
            {
                response.Objects = response.Objects.Where(x => x.Action == filter.Action.Value).ToList();
            }

            // TODO SHIR - ASK MATAN WHY WE NEED THIS HERE?
            response.Objects = response.Objects.Select(cmp => JsonConvert.DeserializeObject<TriggerCampaign>(cmp.CampaignJson)).ToList();

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

            var campaignsListResponse = SearchCampaignsByType<BatchCampaign>(contextData, filter);

            if (!campaignsListResponse.IsOkStatusCode())
            {
                response.SetStatus(campaignsListResponse.Status);
                return response;
            }

            response.Objects = campaignsListResponse.Objects;
            response.SetStatus(eResponseStatus.OK);

            // TODO SHIR - ASK MATAN WHY WE NEED THIS HERE?
            response.Objects = response.Objects.Select(cmp => JsonConvert.DeserializeObject<BatchCampaign>(cmp.CampaignJson)).ToList();

            if (pager != null)
            {
                response.TotalItems = response.Objects.Count;
                response.Objects = pager.PageSize > 0 ? response.Objects.Skip(pager.PageIndex * pager.PageSize).Take(pager.PageSize).ToList() : response.Objects;
            }

            return response;
        }

        public GenericListResponse<Campaign> ListCampaingsByIds(ContextData contextData, CampaignIdInFilter filter)
        {
            // TODO SHIR
            var response = new GenericListResponse<Campaign>();
            // get all campaigns then filter
            response.Objects = response.Objects.Where(camp => filter.IdIn.Contains(camp.Id)).ToList();
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

                if (campaignToAdd.DiscountModuleId.HasValue)
                {
                    var discounts = Core.Pricing.Module.GetValidDiscounts(contextData.GroupId);
                    if (!discounts.HasObjects() || !discounts.Objects.Any(x => x.Id == campaignToAdd.DiscountModuleId.Value))
                    {
                        response.SetStatus(eResponseStatus.DiscountCodeNotExist);
                        return response;
                    }
                }

                campaignToAdd.GroupId = contextData.GroupId;
                campaignToAdd.State = ObjectState.INACTIVE;
                campaignToAdd.CreateDate = DateUtils.ToUtcUnixTimestampSeconds(DateTime.UtcNow);
                campaignToAdd.UpdateDate = campaignToAdd.CreateDate;
                campaignToAdd.UpdaterId = contextData.UserId.Value;

                var insertedCampaign = PricingDAL.AddCampaign(campaignToAdd);
                if (insertedCampaign?.Id > 0)
                {
                    response.Object = insertedCampaign;
                    SetInvalidationKeys(contextData);
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
            // TODO SHIR
            throw new NotImplementedException();
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

                ValidateParametersForUpdate(campaignToUpdate);
                campaignToUpdate.FillEmpty(campaign);
                campaignToUpdate.UpdateDate = DateUtils.ToUtcUnixTimestampSeconds(DateTime.UtcNow);
                campaignToUpdate.GroupId = contextData.GroupId;

                if (PricingDAL.Update_Campaign(campaignToUpdate, contextData.UserId.Value))
                {
                    response.Object = campaignToUpdate;
                    SetInvalidationKeys(contextData);
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
            // TODO SHIR
            throw new NotImplementedException();
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

                ValidateStateChange(campaign.Object, newState);

                var _campaign = campaign.Object as TriggerCampaign;
                var validated = new Status(eResponseStatus.OK);

                if (newState == ObjectState.ACTIVE)
                {
                    validated = ValidateActivation(_campaign);
                }
                else if (newState == ObjectState.INACTIVE)
                {
                    validated = ValidateDeactivation(_campaign);
                }

                if (!validated.IsOkStatusCode())
                {
                    response.SetStatus(campaign.Status);
                    return response;
                }

                if (PricingDAL.Update_Campaign(_campaign, contextData.UserId.Value))
                {
                    SetInvalidationKeys(contextData);
                    response.Object = Get(contextData, _campaign.Id)?.Object;
                    response.SetStatus(eResponseStatus.OK);
                }
                else
                    response.SetStatus(eResponseStatus.Error, $"Error Updating TriggerCampaign: [{id}] for group: {contextData.GroupId}");

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

            if (newState == ObjectState.ACTIVE
                && campaign.EndDate <= DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow))//Check set active
            {
                response.Set(eResponseStatus.Error, $"Campaign: {campaign.Id} already ended");
                return response;
            }

            campaign.State = newState;

            return response;
        }

        /// <summary>
        /// check if can be activate
        /// </summary>
        /// <param name="object"></param>
        private Status ValidateActivation(TriggerCampaign campaign)
        {
            var status = Status.Ok;
            //Todo - Shir or Matan
            //if (campaign.IsActive)
            if (campaign.Status == 1 || campaign.State == ObjectState.ACTIVE)
            {
                log.Error($"Campaign: {campaign.Id} is already active, campaign event: {campaign.CampaignJson}");
                status.Set(eResponseStatus.Error, $"Campaign: {campaign.Id} is already active");
            }
            if (campaign.EndDate <= TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow))
            {
                log.Error($"Campaign: {campaign.Id} was ended at ({campaign.EndDate}), campaign event: {campaign.CampaignJson}");
                status.Set(eResponseStatus.Error, $"Campaign: {campaign.Id} was ended");
            }
            if (campaign.TriggerConditions == null || campaign.TriggerConditions.Count == 0)
            {
                log.Error($"Campaign: {campaign.Id} must have a t least a single trigger condition, campaign event: {campaign.CampaignJson}");
                status.Set(eResponseStatus.Error, $"Campaign: {campaign.Id} must have a t least a single trigger condition");
            }
            if (campaign.DiscountConditions == null || campaign.DiscountConditions.Count == 0)
            {
                log.Error($"Campaign: {campaign.Id} must have a t least a single discount condition, campaign event: {campaign.CampaignJson}");
                status.Set(eResponseStatus.Error, $"Campaign: {campaign.Id} must have a t least a single discount condition");
            }

            campaign.Status = status.IsOkStatusCode() ? 1 : 0;
            return status;
        }

        /// <summary>
        /// check if can be deactivate
        /// </summary>
        /// <param name="object"></param>
        private Status ValidateDeactivation(TriggerCampaign campaign)
        {
            var status = Status.Ok;
            //if (!campaign.IsActive)
            if (campaign.Status != 1)
            {
                log.Error($"Campaign: {campaign.Id} is not active, campaign event: {campaign.CampaignJson}");
                status.Set(eResponseStatus.Error, $"Campaign: {campaign.Id} is not active");
            }

            campaign.Status = status.IsOkStatusCode() ? 1 : 0;
            return status;
        }

        private void ValidateParametersForUpdate(TriggerCampaign campaignToUpdate)
        {
            //TODO - Shir
        }

        private void SetInvalidationKeys(ContextData contextData)
        {
            // TODO SHIR - SetInvalidationKeys
        }

        public void PublishTriggerCampaign(int groupId, int domainId, ICampaignObject eventObject, ApiService apiService, ApiAction apiAction)
        {
            //TODO - Shir: Check if campaign of this type is allowed for group
            if (1 != 1)
            {
                return;
            }

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

        private GenericListResponse<T> SearchCampaignsByType<T>(ContextData contextData, CampaignSearchFilter filter) where T : Campaign, new()
        {
            var response = new GenericListResponse<T>();
            try
            {
                var type = typeof(T).Name;
                IEnumerable<T> campaigns = null;
                var key = LayeredCacheKeys.GetGroupCampaignKey(contextData.GroupId, type);
                var cacheResult = LayeredCache.Instance.Get(key,
                                                            ref campaigns,
                                                            GetCampaignsByGroupId<T>,
                                                            new Dictionary<string, object>() { { "groupId", contextData.GroupId } },
                                                            contextData.GroupId,
                                                            LayeredCacheConfigNames.GET_GROUP_CAMPAIGNS,
                                                            new List<string>() { LayeredCacheKeys.GetGroupCampaignInvalidationKey(contextData.GroupId, type) });

                if (campaigns != null && campaigns.Count() > 0)
                {
                    if (filter.StartDateGreaterThanOrEqual.HasValue)
                    {
                        campaigns = campaigns.Where(x => x.StartDate >= filter.StartDateGreaterThanOrEqual.Value);
                    }

                    if (filter.EndDateLessThanOrEqual.HasValue)
                    {
                        campaigns = campaigns.Where(x => x.EndDate <= filter.EndDateLessThanOrEqual.Value);
                    }

                    if (filter.ContainDiscountModel.HasValue)
                    {
                        if (filter.ContainDiscountModel.Value)
                        {
                            campaigns = campaigns.Where(x => x.DiscountModuleId.HasValue);
                        }
                        else
                        {
                            campaigns = campaigns.Where(x => !x.DiscountModuleId.HasValue);
                        }
                    }

                    response.Objects.AddRange(campaigns);
                }

                response.SetStatus(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"ex: {ex}", ex);
                response.SetStatus(eResponseStatus.Error, $"Couldn't get list of campaigns for group: {contextData.GroupId}");
            }

            return response;
        }

        private static Tuple<IEnumerable<T>, bool> GetCampaignsByGroupId<T>(Dictionary<string, object> arg) where T : Campaign, new()
        {
            try
            {
                var groupId = (int)arg["groupId"];
                var contextData = new ContextData(groupId);
                var list = PricingDAL.List_Campaign<T>(contextData);
                return new Tuple<IEnumerable<T>, bool>(list, true);
            }
            catch (Exception ex)
            {
                log.Error($"Failed to get Campaign list from DB group:[{arg["groupId"]}], ex: {ex}", ex);
                return new Tuple<IEnumerable<T>, bool>(Enumerable.Empty<T>(), false);
            }
        }

        // TODO SHIR
        // 1. list by group id -> return ALL CAMPAIGNS
        // SP:
        //1. get all camp by group -> get all object as is from db

        //cache:
        //2.filter by state(lazy archive?) 
        //private GenericListResponse<T> ListCampaignsByState<T>(ContextData contextData, CampaignSearchFilter filter) where T : Campaign, new()
        //{

        //}

        //none cache:
        //3. FILTER BY id - ask ira
    }
}
