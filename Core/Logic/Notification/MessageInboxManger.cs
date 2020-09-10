using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Notification;
using ApiObjects.Response;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TVinciShared;
using APILogic.ConditionalAccess;
using ApiObjects.Segmentation;

namespace Core.Notification
{
    public class MessageInboxManger
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const string MESSAGE_IDENTIFIER_REQUIRED = "Message identifier is required";
        private const string USER_INBOX_MESSAGE_NOT_EXIST = "User inbox message not exist";


        public static InboxMessageResponse GetInboxMessage(int groupId, int userId, string messageId)
        {
            InboxMessageResponse response = new InboxMessageResponse()
            {
                Status = new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() },
                InboxMessages = new List<InboxMessage>()
            };

            // validate partner inbox configuration is enabled
            if (!NotificationSettings.IsPartnerInboxEnabled(groupId))
            {
                log.ErrorFormat("Partner inbox feature is off. GID: {0}, UID: {1}", groupId, userId);
                response.Status = new Status() { Code = (int)eResponseStatus.FeatureDisabled, Message = eResponseStatus.FeatureDisabled.ToString() };
                return response;
            }

            string logData = string.Format("GID: {0}, UserId: {1}, messageId: {2}", groupId, userId, messageId);

            //check for empty message
            if (string.IsNullOrEmpty(messageId))
            {
                log.ErrorFormat("No user inbox message identifier. {0}.", logData);
                response.Status = new Status() { Code = (int)eResponseStatus.MessageIdentifierRequired, Message = MESSAGE_IDENTIFIER_REQUIRED };
                return response;
            }

            try
            {
                // get user inbox message
                var userInboxMessage = NotificationDal.GetUserInboxMessage(groupId, userId, messageId);

                if (userInboxMessage != null)
                {
                    response.InboxMessages = new List<InboxMessage>();
                    response.InboxMessages.Add(userInboxMessage);
                }
                else
                {
                    log.DebugFormat("No user inbox message. {0}.", logData);
                    response.Status = new Status() { Code = (int)eResponseStatus.UserInboxMessagesNotExist, Message = USER_INBOX_MESSAGE_NOT_EXIST };
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in GetInboxMessage {0}", logData, ex);
                response.Status = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };
            }

            return response;
        }

        public static InboxMessageResponse GetInboxMessages(int groupId, int userId, int pageSize, int pageIndex, List<eMessageCategory> messageCategorys, long CreatedAtGreaterThanOrEqual, long CreatedAtLessThanOrEqual)
        {
            var response = new InboxMessageResponse() { Status = new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() } };

            // validate partner inbox configuration is enabled
            if (!NotificationSettings.IsPartnerInboxEnabled(groupId))
            {
                log.ErrorFormat("Partner inbox feature is off. GID: {0}, UID: {1}", groupId, userId);
                response.Status = new Status() { Code = (int)eResponseStatus.FeatureDisabled, Message = eResponseStatus.FeatureDisabled.ToString() };
                return response;
            }

            string logData = string.Format("GID: {0}, UserId: {1}", groupId, userId);

            try
            {
                // get user notification 
                bool docExist = false;
                var userNotification = NotificationDal.GetUserNotificationData(groupId, userId, ref docExist);
                if (userNotification == null)
                    log.DebugFormat("user notification object wasn't found. GID: {0}, UID: {1}", groupId, userId);

                // check if user was created after the requested date
                long fromSystemAnnouncementDate = CreatedAtGreaterThanOrEqual;
                if (userNotification != null)
                {
                    if (userNotification.CreateDateSec > CreatedAtGreaterThanOrEqual)
                    {
                        log.DebugFormat("from date was updated to user creation date. GID: {0}, UID: {1}, requested date: {2}, new date: {3}",
                            groupId,
                            userId,
                            TVinciShared.DateUtils.UtcUnixTimestampSecondsToDateTime(CreatedAtGreaterThanOrEqual),
                            TVinciShared.DateUtils.UtcUnixTimestampSecondsToDateTime(userNotification.CreateDateSec));

                        fromSystemAnnouncementDate = userNotification.CreateDateSec;
                    }
                }

                //get user unread messages join with system announcement  
                var systemMessages = NotificationDal.GetSystemInboxMessagesView(groupId, fromSystemAnnouncementDate);
                if (systemMessages == null)
                {
                    log.DebugFormat("No system inbox message. {0}", logData);
                    systemMessages = new List<string>();
                }

                SetBatchCampaignsToUser(groupId, userId);
                
                var userMessages = NotificationDal.GetUserMessagesView(groupId, userId, false, CreatedAtGreaterThanOrEqual);
                if (userMessages == null)
                {
                    log.DebugFormat("No user inbox message. {0}", logData);

                    // user has empty inbox.
                    userMessages = new List<InboxMessage>();
                }

                // merge System InboxMessages To UserInbox
                MergeSystemInboxMessagesToUserInbox(groupId, userId, logData, systemMessages, ref userMessages);

                // in case messageCategorys  is null, no filter. get all.
                if (messageCategorys == null)
                {
                    messageCategorys = new List<eMessageCategory>();
                    messageCategorys = Enum.GetValues(typeof(eMessageCategory)).Cast<eMessageCategory>().ToList();
                }

                // filter userMessage according to category and CreatedAtLessThanOrEqual
                List<InboxMessage> filteredUserMessages = null;
                if (CreatedAtLessThanOrEqual > 0)
                    filteredUserMessages = userMessages.Where(x => x.CreatedAtSec <= CreatedAtLessThanOrEqual && messageCategorys.Contains(x.Category)).ToList();
                else
                    filteredUserMessages = userMessages.Where(x => messageCategorys.Contains(x.Category)).ToList();

                response.InboxMessages = filteredUserMessages;
                response.TotalCount = filteredUserMessages.Count;

                // paging
                response.InboxMessages = filteredUserMessages.Skip(pageSize * pageIndex).Take(pageSize).ToList();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in GetInboxMessages {0}", logData, ex);
            }

            return response;
        }

        private static void MergeSystemInboxMessagesToUserInbox(int groupId, int userId, string logData, List<string> systemMessages,
        ref List<InboxMessage> userMessages)
        {
            try
            {
                //compare messageId [userMessages] not in [systemMessages]            
                var newSystemMessage = systemMessages.Select(m => m).Except(userMessages.Select(x => x.Id)).ToList();

                //get newInboxSystemMessage for update and saving to user inbox
                List<InboxMessage> newInboxSystemMessage = NotificationDal.GetSystemInboxMessages(groupId, newSystemMessage);

                if (newInboxSystemMessage != null)
                {
                    foreach (InboxMessage systemInboxMessage in newInboxSystemMessage)
                    {
                        systemInboxMessage.UserId = userId;

                        var res = NotificationDal.SetUserInboxMessage(groupId, systemInboxMessage, NotificationSettings.GetInboxMessageTTLDays(groupId));
                        if (res)
                        {
                            //add system message to user inbox
                            userMessages.Add(systemInboxMessage);
                        }
                        else
                        {
                            log.ErrorFormat("Error while saving system Message to user. {0}, messageId: {1}", logData, systemInboxMessage.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at MergeSystemInboxMessagesToUserInbox. {0}", logData, ex);
            }
        }

        public static Status UpdateInboxMessage(int groupId, int userId, string messageId, eMessageState status)
        {
            string logData = string.Format("GID: {0}, UserId: {1}, messageId: {2}, status: {3}", groupId, userId, messageId, status.ToString());

            // validate partner inbox configuration is enabled
            if (!NotificationSettings.IsPartnerInboxEnabled(groupId))
            {
                log.ErrorFormat("Partner inbox feature is off. GID: {0}, UID: {1}", groupId, userId);
                return new Status() { Code = (int)eResponseStatus.FeatureDisabled, Message = eResponseStatus.FeatureDisabled.ToString() };
            }

            try
            {
                //check for empty message
                if (string.IsNullOrEmpty(messageId))
                {
                    log.ErrorFormat("No user inbox message identifier. {0}.", logData);
                    return new Status() { Code = (int)eResponseStatus.MessageIdentifierRequired, Message = MESSAGE_IDENTIFIER_REQUIRED };
                }

                // get user inbox message
                var userInboxMessage = NotificationDal.GetUserInboxMessage(groupId, userId, messageId);

                if (userInboxMessage == null)
                {
                    log.ErrorFormat("No user inbox message. {0}.", logData);
                    return new Status() { Code = (int)eResponseStatus.UserInboxMessagesNotExist, Message = USER_INBOX_MESSAGE_NOT_EXIST };
                }

                //get newInboxSystemMessage for update and saving to user inbox
                var isSet = NotificationDal.UpdateInboxMessageState(groupId, userId, messageId, status);

                if (!isSet)
                {
                    log.ErrorFormat("Failed updating InboxMessage. {0}", logData);
                    return new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at UpdateInboxMessage. {0}", logData, ex);
                return new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }

            return new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString()); ;
        }

        private static void SetBatchCampaignsToUser(int groupId, int userId)
        {
            //get all valid batch campaigns(by dates and status = active).
            var utcNow = DateUtils.GetUtcUnixTimestampNow();
            var contextData = new ContextData(groupId) { UserId = userId };

            var batchFilter = new BatchCampaignFilter()
            {
                IsActiveNow = true,
                StateEqual = ObjectState.ACTIVE
            };
            var batchCampaignsResponse = ApiLogic.Users.Managers.CampaignManager.Instance.ListBatchCampaigns(contextData, batchFilter);
            List<BatchCampaign> batchCampaigns = batchCampaignsResponse.HasObjects() ? batchCampaignsResponse.Objects : null;

            NotificationDal.RemoveOldCampaignsFromInboxMessageMapCB(groupId, userId, utcNow);

            //get all existing user’s batch campaigns(all existing user’s campaign messages).
            var userCampaignsMap = NotificationDal.GetCampaignInboxMessageMapCB(groupId, userId);
            var userBatchCampaignsMap = userCampaignsMap != null ? userCampaignsMap.BatchCampaigns : new Dictionary<long, InboxMessageWithExpiration>();

            // add new batch campaigns to user
            if (batchCampaigns != null)
            {
                //remove shared campaigns from all campaigns list(In order to "reduce the cost" of finding the campaigns that are suitable for the user).
                var missingBatchCampaigns = batchCampaigns.Where(x => !userBatchCampaignsMap.ContainsKey(x.Id)).ToList();
                
                if (missingBatchCampaigns.Count > 0)
                {
                    var userSegments = UserSegment.List(groupId, userId.ToString(), out int totalCount);
                    var scope = new ConditionScope()
                    {
                        GroupId = groupId,
                        UserId = contextData.UserId.ToString(),
                        FilterBySegments = true,
                        SegmentIds = userSegments != null ? userSegments.Select(x => x.SegmentId).ToList() : null
                    };

                    //filter relevant campaigns by populationConditions.
                    foreach (var campaign in missingBatchCampaigns)
                    {
                        var isValid = campaign.EvaluatePopulationConditions(scope);
                        if (isValid)
                        {
                            //add all relevant campaigns to user’s inbox message with TTL by the end date of the campaign.
                            AddCampaignMessage(campaign, groupId, userId);
                        }
                    }
                }
            }
        }

        public static void AddCampaignMessage(Campaign campaign, int groupId, long userId)
        {
            var ttl = DateUtils.UtcUnixTimestampSecondsToDateTime(campaign.EndDate) - DateTime.UtcNow;
            var current = DateUtils.GetUtcUnixTimestampNow();

            //Add to user Inbox
            var inboxMessage = new InboxMessage
            {
                Id = Guid.NewGuid().ToString(),
                Message = campaign.Message,
                UserId = userId,
                CreatedAtSec = current,
                UpdatedAtSec = current,
                State = eMessageState.Unread,
                Category = eMessageCategory.Campaign,
                CampaignId = campaign.Id
            };

            if (!DAL.NotificationDal.SetUserInboxMessage(groupId, inboxMessage, ttl.TotalDays))
            {
                log.Error($"Failed to add campaign message (campaign: {campaign.Id}) to User: {userId} Inbox");
            }
            else
            {
                log.Debug($"Campaign message (campaign: {campaign.Id}) sent successfully to User: {userId} Inbox");
                var inboxMessageExpiration = new InboxMessageWithExpiration() { MessageId = inboxMessage.Id, ExpiredAt = campaign.EndDate };
                DAL.NotificationDal.AddToCampaignInboxMessageMapCB(campaign, groupId, userId, inboxMessageExpiration);//update mapping
            }
        }
    }
}
