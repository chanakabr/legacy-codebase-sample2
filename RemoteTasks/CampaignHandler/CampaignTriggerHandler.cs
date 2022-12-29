using ApiLogic.Users.Managers;
using ApiObjects;
using ApiObjects.Base;
using EventBus.Abstraction;
using Newtonsoft.Json;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using domain = Core.Domains.Module;
using messageInboxManger = Core.Notification.MessageInboxManger;

namespace CampaignHandler
{
    public class CampaignTriggerHandler : IServiceEventHandler<Core.Api.Modules.CampaignTriggerEvent>
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public CampaignTriggerHandler()
        {
            _Logger.Debug("Starting 'CampaignTriggerHandler'");
        }

        public Task Handle(Core.Api.Modules.CampaignTriggerEvent serviceEvent)
        {
            try
            {
                _Logger.Debug($"Starting CampaignTriggerHandler event:[{JsonConvert.SerializeObject(serviceEvent)}].");

                var domain =
                    Core.ConditionalAccess.Utils.GetDomainInfo((int)serviceEvent.DomainId, serviceEvent.GroupId);
                if (domain == null)
                {
                    _Logger.Debug($"Couldn't find domain: {(int)serviceEvent.DomainId}, " +
                                  $"requestId: [{serviceEvent.RequestId}], Service: [{(ApiService)serviceEvent.ApiService}]");
                    return Task.CompletedTask;
                }

                var master = domain.m_masterGUIDs.FirstOrDefault();
                var contextData = new ContextData(serviceEvent.GroupId)
                    { DomainId = serviceEvent.DomainId, UserId = master };

                var filter = new TriggerCampaignFilter()
                {
                    Service = (ApiService)serviceEvent.ApiService,
                    Action = (ApiAction)serviceEvent.ApiAction,
                    StateEqual = CampaignState.ACTIVE,
                    IsActiveNow = true
                };
                var triggerCampaigns = CampaignManager.Instance.ListTriggerCampaigns(contextData, filter);
                if (!triggerCampaigns.HasObjects())
                {
                    _Logger.Debug($"Couldn't find any CampaignTriggerEvent for ContextData: {contextData}, " +
                                  $"requestId: [{serviceEvent.RequestId}], Service: [{filter.Service}]");
                    return Task.CompletedTask;
                }

                var existingCampaigns = DAL.CampaignUsageRepository.Instance.GetCampaignInboxMessageMapCB(serviceEvent.GroupId, master);

                foreach (var triggerCampaign in triggerCampaigns.Objects)
                {
                    //BEO-8610 handle anti fraud
                    var deviceTriggerCampaignsUses =
                        DAL.CampaignUsageRepository.Instance.GetDeviceTriggerCampainsUses(serviceEvent.GroupId,
                            serviceEvent.EventObject.Udid);
                    var isExists = deviceTriggerCampaignsUses?.Uses?.ContainsKey(triggerCampaign.Id);
                    if (isExists.HasValue && isExists.Value)
                    {
                        continue;
                    }

                    isExists = existingCampaigns?.Campaigns?.ContainsKey(triggerCampaign.Id);
                    if (isExists.HasValue && isExists.Value)
                    {
                        if (!existingCampaigns.Campaigns[triggerCampaign.Id].Devices
                                .Contains(serviceEvent.EventObject.Udid))
                        {
                            var campaignDetails = existingCampaigns.Campaigns[triggerCampaign.Id];
                            if (campaignDetails.Devices == null)
                            {
                                campaignDetails.Devices = new List<string>();
                            }

                            campaignDetails.Devices.Add(serviceEvent.EventObject.Udid);

                            if (!DAL.CampaignUsageRepository.Instance.SaveToCampaignInboxMessageMapCb(triggerCampaign.Id,
                                    contextData.GroupId, master, campaignDetails))
                            {
                                _Logger.Error(
                                    $"Failed SaveToCampaignInboxMessageMapCB with campaign: {triggerCampaign.Id}, " +
                                    $"hh: {serviceEvent.DomainId}, group: {contextData.GroupId}");
                            }
                        }

                        continue;
                    }

                    Parallel.ForEach(domain.m_masterGUIDs, user =>
                    {
                        var _contextData = new ContextData(serviceEvent.GroupId)
                        {
                            DomainId = serviceEvent.DomainId,
                            UserId = user
                        };

                        var scope = serviceEvent.EventObject.CreateTriggerCampaignConditionScope(contextData);
                        if (triggerCampaign.EvaluateConditions(scope))
                        {
                            _contextData.Udid = serviceEvent.EventObject?.Udid;
                            messageInboxManger.Instance.AddCampaignMessageToUser(triggerCampaign, serviceEvent.GroupId,
                                user, _contextData.Udid);
                            CampaignManager.Instance.NotifyTriggerCampaignEvent(triggerCampaign, _contextData);
                        }
                    });
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _Logger.Error($"An Exception occurred in CampaignTriggerHandler requestId:[{serviceEvent.RequestId}]",
                    ex);
                return Task.FromException(ex);
            }
        }
    }
}