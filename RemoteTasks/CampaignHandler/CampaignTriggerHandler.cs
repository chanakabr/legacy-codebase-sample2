using ApiObjects.EventBus;
using EventBus.Abstraction;
using KLogMonitor;
using System;
using System.Reflection;
using System.Threading.Tasks;
using ApiObjects.Base;
using ApiLogic.Users.Managers;
using domain = Core.Domains.Module;
using ApiObjects.Notification;
using ApiObjects;
using System.Linq;
using Core.Users;

namespace CampaignHandler
{
    public class CampaignTriggerHandler : IServiceEventHandler<CampaignTriggerEvent>
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public CampaignTriggerHandler()
        {
            _Logger.Debug("Starting 'CampaignTriggerHandler'");
        }

        public Task Handle(CampaignTriggerEvent serviceEvent)
        {
            try
            {
                var filter = new TriggerCampaignFilter()
                {
                    Service = (ApiService)serviceEvent.ApiService,
                    Action = (ApiAction)serviceEvent.ApiAction
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
                            AddMessageToInbox(serviceEvent, user, _triggerCampaign);
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

        private void AddMessageToInbox(CampaignTriggerEvent serviceEvent, int userId, TriggerCampaign campaign)
        {
            //Check if user has this campaign in his inbox
            var inboxMessage = DAL.NotificationDal.GetCampaignInboxMessage(serviceEvent.GroupId, userId, campaign.Id.ToString());

            if (inboxMessage != null && !string.IsNullOrEmpty(inboxMessage.Id))
            {
                _Logger.Info($"Campaign: {campaign.Id} already assigned to user: {userId}, group: {serviceEvent.GroupId}, domain: {serviceEvent.DomainId}");
                return;
            }

            var ttl = TVinciShared.DateUtils.UtcUnixTimestampSecondsToDateTime(campaign.EndDate) - DateTime.UtcNow;
            var current = TVinciShared.DateUtils.GetUtcUnixTimestampNow();

            //Add to user Inbox
            inboxMessage = new InboxMessage
            {
                Id = Guid.NewGuid().ToString(),
                Message = campaign.Message,
                UserId = userId,
                CreatedAtSec = current,
                UpdatedAtSec = current,
                State = eMessageState.Unread,
                Category = eMessageCategory.Campaign
            };

            if (!DAL.NotificationDal.SetCampaignInboxMessage(serviceEvent.GroupId, inboxMessage, campaign.Id, ttl.Days))
            {
                _Logger.Error($"Failed to add campaign message (campaign: {campaign.Id}) to User: {userId} Inbox");
                DAL.NotificationDal.SetInboxMessageCampaignMapping(serviceEvent.GroupId, serviceEvent.UserId, campaign, inboxMessage.Id);//update mapping
            }
            else
                _Logger.Debug($"Campaign message (campaign: {campaign.Id}) sent successfully to User: {userId} Inbox");
        }
    }
}
