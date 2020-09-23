using ApiObjects.EventBus;
using EventBus.Abstraction;
using KLogMonitor;
using System;
using System.Reflection;
using System.Threading.Tasks;
using ApiObjects.Base;
using ApiLogic.Users.Managers;
using domain = Core.Domains.Module;
using ApiObjects;
using System.Linq;
using Core.Users;
using TVinciShared;

namespace CampaignHandler
{
    public class CampaignTriggerHandler : IServiceEventHandler<ApiObjects.EventBus.CampaignTriggerEvent>
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public CampaignTriggerHandler()
        {
            _Logger.Debug("Starting 'CampaignTriggerHandler'");
        }

        public Task Handle(ApiObjects.EventBus.CampaignTriggerEvent serviceEvent)
        {
            try
            {
                var filter = new TriggerCampaignFilter()
                {
                    Service = (ApiService)serviceEvent.ApiService,
                    Action = (ApiAction)serviceEvent.ApiAction,
                    StateEqual = ObjectState.ACTIVE,
                    StartDateGreaterThanOrEqual = DateUtils.GetUtcUnixTimestampNow()
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
                    _Logger.Error($"Error finding CampaignTriggerEvent for object: {serviceEvent.EventObject}");
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