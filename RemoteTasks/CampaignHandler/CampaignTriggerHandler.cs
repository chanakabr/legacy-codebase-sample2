using ApiLogic.Users.Managers;
using ApiObjects;
using ApiObjects.Base;
using Core.Users;
using EventBus.Abstraction;
using KLogMonitor;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TVinciShared;
using domain = Core.Domains.Module;

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
                var filter = new TriggerCampaignFilter()
                {
                    Service = (ApiService)serviceEvent.ApiService,
                    Action = (ApiAction)serviceEvent.ApiAction,
                    StateEqual = ObjectState.ACTIVE
                };

                var domain = new Domain((int)serviceEvent.DomainId);

                if (!domain.Initialize(serviceEvent.GroupId, (int)serviceEvent.DomainId) || domain.m_UsersIDs == null)
                {
                    _Logger.Error($"No users for domain: {domain.Id}, group: {domain.GroupId}");
                    return Task.CompletedTask;
                }

                var master = domain.m_masterGUIDs.FirstOrDefault();

                var contextData = new ContextData(serviceEvent.GroupId) { DomainId = serviceEvent.DomainId, UserId = master };

                var triggerCampaigns = CampaignManager.Instance.ListTriggerCampaigns(contextData, filter);

                if (!triggerCampaigns.HasObjects())
                {
                    _Logger.Debug($"Couldn't find any CampaignTriggerEvent for ContextData: {contextData}, " +
                        $"requestId: [{serviceEvent.RequestId}], Service: [{filter.Service.ToString()}]");
                    return Task.CompletedTask;
                }

                _Logger.Debug($"Starting CampaignTriggerHandler requestId:[{serviceEvent.RequestId}], Service: [{filter.Service.ToString()}]");

                foreach (var _triggerCampaign in triggerCampaigns.Objects)
                {
                    //Send to all users or only Master-user - TBD - Ask Oded? TODO - MATAN
                    Parallel.ForEach(domain.m_UsersIDs, user =>
                    {
                        var _contextData = new ContextData(serviceEvent.GroupId) { DomainId = serviceEvent.DomainId, UserId = user };
                        if (!_triggerCampaign.EvaluateTriggerConditions(serviceEvent.EventObject, _contextData))
                        {
                            _Logger.Info($"user: {_contextData.UserId} doesn't match campaign: {_triggerCampaign.Id}, group: {serviceEvent.GroupId}");
                        }
                        else
                        {
                            Core.Notification.MessageInboxManger.AddCampaignMessage(_triggerCampaign, serviceEvent.GroupId, serviceEvent.UserId);
                        }
                    });
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _Logger.Error($"An Exception occurred in CampaignTriggerHandler requestId:[{serviceEvent.RequestId}]", ex);
                return Task.FromException(ex);
            }
        }
    }
}