using ApiObjects.EventBus;
using EventBus.Abstraction;
using KLogMonitor;
using System;
using System.Reflection;
using System.Threading.Tasks;
using ApiObjects.Base;
using ApiLogic.Users.Managers;
using System.Linq;
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
                _Logger.Debug($"Starting CampaignTriggerHandler requestId:[{serviceEvent.RequestId}], CampaignId: [{serviceEvent.CampaignId}]");
                var contextData = new ContextData(serviceEvent.GroupId) { UserId = serviceEvent.UserId, DomainId = serviceEvent.DomainId };
                var campaign = CampaignManager.Instance.Get(contextData, serviceEvent.CampaignId);
                if (campaign == null)
                {
                    _Logger.Info($"No campaign with Id: {serviceEvent.CampaignId} was found for group: {serviceEvent.GroupId} and domain: {serviceEvent.DomainId}");
                    return Task.CompletedTask;
                }

                var domainUsers = domain.GetDomainUserList(serviceEvent.DomainId, serviceEvent.GroupId);
                var triggerCampaign = campaign.Object as TriggerCampaign;

                if (!CampaignManager.Instance.ValidateTriggerCampaign(triggerCampaign, serviceEvent.EventObject))
                {
                    _Logger.Info($"Domain: {serviceEvent.DomainId} doesn't match campaign: {serviceEvent.CampaignId}, group: {serviceEvent.GroupId}");
                    return Task.CompletedTask;
                }

                var filter = new List<eMessageCategory> { eMessageCategory.Campaign };

                Parallel.ForEach(domainUsers, user =>
                {
                    if (!int.TryParse(user, out int userId))
                    {
                        _Logger.Info($"Incorrect user: {user} was sent to CampaignTriggerHandler");
                        return;
                    }

                    if (CampaignManager.Instance.ValidateCampaignConditionsToUser(contextData, triggerCampaign))
                    {
                        SendMessage(serviceEvent, userId, triggerCampaign, triggerCampaign.Message);
                    }
                });

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _Logger.Error($"An Exception occurred in CampaignTriggerHandler requestId:[{serviceEvent.RequestId}], CampaignId:[{serviceEvent.CampaignId}].", ex);
                return Task.FromException(ex);
            }
        }

        private void SendMessage(CampaignTriggerEvent serviceEvent, int userId, TriggerCampaign campaign, string message)
        {
            var current = TVinciShared.DateUtils.GetUtcUnixTimestampNow();

            //Add to user Inbox
            var inboxMessage = new InboxMessage
            {
                Id = Guid.NewGuid().ToString(),
                Message = message,
                UserId = userId,
                CreatedAtSec = current,
                UpdatedAtSec = current,
                State = eMessageState.Unread,
                Category = eMessageCategory.Campaign
            };

            var isSuccess = false;

            if (!DAL.NotificationDal.SetCampaignInboxMessage(serviceEvent.GroupId, inboxMessage, serviceEvent.CampaignId,
                NotificationSettings.GetInboxMessageTTLDays(serviceEvent.GroupId)))
            {
                _Logger.Error($"Failed to add campaign message (campaign: {serviceEvent.CampaignId}) to User: {userId} Inbox");
                isSuccess = true;
            }
            else
                _Logger.Debug($"Campaign message (campaign: {serviceEvent.CampaignId}) sent successfully to User: {userId} Inbox");

            if (isSuccess)
                DAL.NotificationDal.SetInboxMessageCampaignMapping(serviceEvent.GroupId, serviceEvent.UserId, campaign, inboxMessage.Id);
        }
    }
}
