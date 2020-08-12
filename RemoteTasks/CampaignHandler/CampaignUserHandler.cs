using ApiObjects.EventBus;
using CouchbaseManager;
using EventBus.Abstraction;
using KLogMonitor;
using System;
using System.Reflection;
using System.Threading.Tasks;
using ApiObjects.Base;
using ApiLogic.Users.Managers;
using WebAPI.ClientManagers.Client;
using System.Linq;
using System.Collections.Generic;
using domain = Core.Domains.Module;
using notification = Core.Notification.Module;
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
                //TODO - Matan - Add actual logic
                _Logger.Debug($"Starting CampaignUserHandler requestId:[{serviceEvent.RequestId}], CampaignId: [{serviceEvent.CampaignId}]");
                var contextData = new ContextData(serviceEvent.GroupId) { UserId = serviceEvent.UserId, DomainId = serviceEvent.DomainId };
                var campaign = CampaignManager.Instance.Get(contextData, serviceEvent.CampaignId);
                if (campaign == null)
                {
                    _Logger.Info($"No campaign with Id: {serviceEvent.CampaignId} was found for group: {serviceEvent.GroupId} and domain: {serviceEvent.DomainId}");
                    return Task.CompletedTask;
                }

                //Check all users in domain for existing of campaign in Inbox
                var domainUsers = domain.GetDomainUserList(serviceEvent.DomainId, serviceEvent.GroupId);

                var filter = new List<ApiObjects.eMessageCategory> { ApiObjects.eMessageCategory.Interest };
                var triggerCampaign = campaign.Object as ApiObjects.TriggerCampaign;
                var messages = triggerCampaign.Messages;

                if (!CampaignManager.Instance.ValidateTriggerCampaign(triggerCampaign, serviceEvent.EventObject))
                {
                    _Logger.Info($"Domain: {serviceEvent.DomainId} doesn't match to campaign: {serviceEvent.CampaignId}, group: {serviceEvent.GroupId}");
                    return Task.CompletedTask;
                }

                Parallel.ForEach(domainUsers, user =>
                {
                    if (!int.TryParse(user, out int userId))
                    {
                        _Logger.Info($"Incorrect user: {user} was sent to CampaignUserHandler");
                        return;
                    }

                    // get user inbox messages for this specific campaign
                    var inbox = MessageInboxManger.GetInboxMessages(serviceEvent.GroupId, userId, 100, 0, filter, 0, 0);//change inbox messages filter

                    if (CampaignManager.Instance.ValidateCampaignConditionsToUser(contextData, triggerCampaign))
                    {
                        var inboxIds = inbox?.InboxMessages?.Select(x => x.Id).ToList();
                        var campaignMessageIds = messages.Select(x => x.Key).ToList();
                        var contained = !inboxIds.Except(campaignMessageIds).Any();

                        if (contained)
                        {
                            _Logger.Info($"Campaign: {serviceEvent.CampaignId} already assigned to user: {userId}, group: {serviceEvent.GroupId}");
                            return;
                        }
                        else
                        {
                            var missingMessages = campaignMessageIds.Where(msg => inboxIds.All(p2 => p2 != msg)).ToList();
                            foreach (var message in missingMessages)
                            {
                                var pushMessage = new PushMessage()
                                {
                                    //Message = messages
                                };
                                var result = EngagementManager.SendPushToUser(serviceEvent.GroupId, userId, pushMessage);
                            }
                        }

                        ////TODO - Matan: send message
                        //var pushMessage = new PushMessage()
                        //{

                        //};
                        //var result = EngagementManager.SendPushToUser(serviceEvent.GroupId, userId, pushMessage);
                    }





                    //var inbox = notification.GetInboxMessages(serviceEvent.GroupId, userId, 100, 0, filter, 0, 0);//change inbox messages filter
                    //var messageExists = inbox?.InboxMessages?.Select(im => im.Message).Contains(serviceEvent.CampaignId.ToString()); //change condition
                    //if (messages.Count > 0)
                    //{
                    //    var exists = true;
                    //    foreach (var message in messages)
                    //    {
                    //        exists += inbox?.InboxMessages?.Select(im=>im)
                    //    }
                    //}

                    ////if message exist not need to do anything
                    //if (messageExists.HasValue && messageExists.Value)
                    //{
                    //    _Logger.Info($"Campaign: {serviceEvent.CampaignId} already assigned to user: {userId}, group: {serviceEvent.GroupId}");
                    //    return;
                    //}
                    //else
                    //{
                       
                    //}
                });

                return null;
            }
            catch (Exception ex)
            {
                _Logger.Error($"An Exception occurred in CampaignUserHandler requestId:[{serviceEvent.RequestId}], CampaignId:[{serviceEvent.CampaignId}].", ex);
                return Task.FromException(ex);
            }
        }
    }
}
