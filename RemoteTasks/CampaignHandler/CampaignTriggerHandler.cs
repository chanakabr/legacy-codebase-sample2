using ApiLogic.Users.Managers;
using ApiObjects;
using ApiObjects.Base;
using Core.Users;
using EventBus.Abstraction;
using KLogMonitor;
using Newtonsoft.Json;
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
                _Logger.Debug($"Debug event: {JsonConvert.SerializeObject(serviceEvent)}");

                var filter = new TriggerCampaignFilter()
                {
                    Service = (ApiService)serviceEvent.ApiService,
                    Action = (ApiAction)serviceEvent.ApiAction,
                    StateEqual = CampaignState.ACTIVE,
                    IsActiveNow = true
                };

                var domain = Core.ConditionalAccess.Utils.GetDomainInfo((int)serviceEvent.DomainId, serviceEvent.GroupId);

                if (domain == null)
                {
                    _Logger.Debug($"Couldn't find domain: {(int)serviceEvent.DomainId}, " +
                        $"requestId: [{serviceEvent.RequestId}], Service: [{filter.Service}]");
                    return Task.CompletedTask;
                }

                var master = domain.m_masterGUIDs.FirstOrDefault();
                var contextData = new ContextData(serviceEvent.GroupId) { DomainId = serviceEvent.DomainId, UserId = master };

                var triggerCampaigns = CampaignManager.Instance.ListTriggerCampaigns(contextData, filter);

                if (!triggerCampaigns.HasObjects())
                {
                    _Logger.Debug($"Couldn't find any CampaignTriggerEvent for ContextData: {contextData}, " +
                        $"requestId: [{serviceEvent.RequestId}], Service: [{filter.Service}]");
                    return Task.CompletedTask;
                }

                _Logger.Debug($"Starting CampaignTriggerHandler requestId:[{serviceEvent.RequestId}], Service: [{filter.Service}]");

                var existingCampaigns = DAL.NotificationDal.GetCampaignInboxMessageMapCB(serviceEvent.GroupId, master);

                foreach (var _triggerCampaign in triggerCampaigns.Objects)
                {
                    var isExists = existingCampaigns?.Campaigns?.ContainsKey(_triggerCampaign.Id);
                    if (isExists.HasValue && isExists.Value)
                        continue;

                    Parallel.ForEach(domain.m_masterGUIDs, user =>
                    {
                        var _contextData = new ContextData(serviceEvent.GroupId) { DomainId = serviceEvent.DomainId, UserId = user };
                        if (_triggerCampaign.EvaluateTriggerConditions(serviceEvent.EventObject, _contextData))
                        {
                            Core.Notification.MessageInboxManger.AddCampaignMessage(_triggerCampaign, serviceEvent.GroupId, user);
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