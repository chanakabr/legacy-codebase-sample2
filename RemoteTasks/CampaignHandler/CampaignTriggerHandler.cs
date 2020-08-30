using ApiObjects.EventBus;
using EventBus.Abstraction;
using KLogMonitor;
using System;
using System.Reflection;
using System.Threading.Tasks;
using ApiObjects.Base;
using ApiLogic.Users.Managers;
using System.Collections.Generic;
using domain = Core.Domains.Module;
using Core.Notification;
using ApiObjects.Notification;
using ApiObjects;

namespace CampaignHandler
{
    public class CampaignTriggerHandler : IServiceEventHandler<CampaignTriggerEvent>
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public CampaignTriggerHandler()
        {
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

                var contextData = new ContextData(serviceEvent.GroupId) { DomainId = serviceEvent.DomainId };
                var domainUsers = domain.GetDomainUserList((int)serviceEvent.DomainId, serviceEvent.GroupId);

                if (domainUsers == null || domainUsers.Count == 0)
                {
                    _Logger.Error($"No domain users for domain: {contextData.DomainId}, group: {contextData.GroupId}");
                    return Task.CompletedTask;
                }

                var triggerCampaigns = CampaignManager.Instance.ListTriggerCampaigns(contextData, filter);

                if (!triggerCampaigns.HasObjects())
                {
                    _Logger.Error($"Error finding CampaignTriggerEvent for object: {serviceEvent.EventObject}");
                    return Task.CompletedTask;
                }

                _Logger.Debug($"Starting CampaignTriggerHandler requestId:[{serviceEvent.RequestId}], Service: [{filter.Service.ToString()}]");

                foreach (var _triggerCampaign in triggerCampaigns.Objects)
                {
                    if (!CampaignManager.Instance.ValidateTriggerCampaign(_triggerCampaign, serviceEvent.EventObject))
                    {
                        _Logger.Info($"Domain: {serviceEvent.DomainId} doesn't match campaign: {_triggerCampaign.Id}, group: {serviceEvent.GroupId}");
                        continue;
                    }

                    var _filter = new List<eMessageCategory> { eMessageCategory.Campaign };

                    Parallel.ForEach(domainUsers, user =>
                    {
                        if (!int.TryParse(user, out int userId))
                        {
                            _Logger.Info($"Incorrect user: {user} was sent to CampaignTriggerHandler, campaign: {_triggerCampaign.Id}");
                            return;
                        }

                        if (CampaignManager.Instance.ValidateCampaignConditionsToUser(contextData, _triggerCampaign))
                        {
                            AddMessageToInbox(serviceEvent, userId, _triggerCampaign);
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

            if (!DAL.NotificationDal.SetCampaignInboxMessage(serviceEvent.GroupId, inboxMessage, campaign.Id,
                NotificationSettings.GetInboxMessageTTLDays(serviceEvent.GroupId)))
            {
                _Logger.Error($"Failed to add campaign message (campaign: {campaign.Id}) to User: {userId} Inbox");
                DAL.NotificationDal.SetInboxMessageCampaignMapping(serviceEvent.GroupId, serviceEvent.UserId, campaign, inboxMessage.Id);//update mapping
            }
            else
                _Logger.Debug($"Campaign message (campaign: {campaign.Id}) sent successfully to User: {userId} Inbox");
        }
    }
}
