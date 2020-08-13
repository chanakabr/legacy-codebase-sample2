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

namespace CampaignHandler
{
    public class CampaignUserHandler : IServiceEventHandler<CampaignUserEvent>
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        //private readonly CouchbaseManager.CouchbaseManager _CouchbaseManager = null;

        public CampaignUserHandler()
        {
            //_CouchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);
        }

        public Task Handle(CampaignUserEvent serviceEvent)
        {
            try
            {
                _Logger.Debug($"Starting CampaignUserHandler requestId:[{serviceEvent.RequestId}], CampaignId: [{serviceEvent.CampaignId}]");
                var contextData = new ContextData(serviceEvent.GroupId) { UserId = serviceEvent.UserId, DomainId = serviceEvent.DomainId };
                var campaign = CampaignManager.Instance.Get(contextData, serviceEvent.CampaignId);
                if (campaign == null)
                {
                    _Logger.Info($"No campaign with Id: {serviceEvent.CampaignId} was found for group: {serviceEvent.GroupId} and domain: {serviceEvent.DomainId}");
                    return Task.CompletedTask;
                }

                //Check all users from domain for existence of campaign keys in Inbox
                var domainUsers = domain.GetDomainUserList(serviceEvent.DomainId, serviceEvent.GroupId);
                var triggerCampaign = campaign.Object as ApiObjects.TriggerCampaign;
                var messages = triggerCampaign.Messages;

                if (!CampaignManager.Instance.ValidateTriggerCampaign(triggerCampaign, serviceEvent.EventObject))
                {
                    _Logger.Info($"Domain: {serviceEvent.DomainId} doesn't match campaign: {serviceEvent.CampaignId}, group: {serviceEvent.GroupId}");
                    return Task.CompletedTask;
                }
                var filter = new List<ApiObjects.eMessageCategory> { ApiObjects.eMessageCategory.Campaign };

                Parallel.ForEach(domainUsers, user =>
                {
                    if (!int.TryParse(user, out int userId))
                    {
                        _Logger.Info($"Incorrect user: {user} was sent to CampaignUserHandler");
                        return;
                    }
                    // get user inbox messages for a specific campaign
                    var inbox = MessageInboxManger.GetInboxMessages(serviceEvent.GroupId, userId, 100, 0, filter, triggerCampaign.CreateDate, 0);//Filter options

                    if (CampaignManager.Instance.ValidateCampaignConditionsToUser(contextData, triggerCampaign))
                    {
                        var inboxIds = inbox?.InboxMessages?.Select(x => x.Id).ToList();
                        var campaignMessageIds = messages.Select(x => x.Key).ToList();
                        var contained = !inboxIds.Except(campaignMessageIds).Any();//ToDo - Matan, Change id

                        if (contained)//Already exists
                        {
                            _Logger.Info($"Campaign: {serviceEvent.CampaignId} already assigned to user: {userId}, group: {serviceEvent.GroupId}");
                            return;
                        }
                        else
                        {
                            var missingMessages = campaignMessageIds.Where(msg => inboxIds.All(p2 => p2 != msg)).ToList();
                            foreach (var message in missingMessages)
                            {
                                //TODO - MATAN, ask Shir regarding override same document because key is not unique for each message, 
                                //need to update inbox document with more messages
                                SendMessage(serviceEvent, messages, userId, message);
                            }
                        }
                    }
                });

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _Logger.Error($"An Exception occurred in CampaignUserHandler requestId:[{serviceEvent.RequestId}], CampaignId:[{serviceEvent.CampaignId}].", ex);
                return Task.FromException(ex);
            }
        }

        private void SendMessage(CampaignUserEvent serviceEvent, List<KeyValuePair<string, string>> messages, int userId, string message)
        {
            var current = TVinciShared.DateUtils.GetUtcUnixTimestampNow();
            
            var pushMessage = new PushMessage()
            {
                Message = messages.Where(msg => msg.Key == message).First().Value
            };
            //Actual send is needed?
            Task.Run(() => EngagementManager.SendPushToUser(serviceEvent.GroupId, userId, pushMessage));

            //Add to user Inbox
            var inboxMessage = new InboxMessage
            {
                Id = DAL.NotificationDal.GetCampaignMessageKey(serviceEvent.GroupId, userId, serviceEvent.CampaignId.ToString()),
                Message = message,
                UserId = userId,
                CreatedAtSec = current,
                UpdatedAtSec = current,
                State = ApiObjects.eMessageState.Unread,
                Category = ApiObjects.eMessageCategory.Campaign
            };

            if (!DAL.NotificationDal.SetUserInboxMessage(serviceEvent.GroupId, inboxMessage, NotificationSettings.GetInboxMessageTTLDays(serviceEvent.GroupId)))
                _Logger.Error($"Failed to add campaign message (campaign: {serviceEvent.CampaignId}) to User: {userId} Inbox");
            else
                _Logger.Debug($"Campaign message (campaign: {serviceEvent.CampaignId}) sent successfully to User: {userId} Inbox");
        }
    }
}
