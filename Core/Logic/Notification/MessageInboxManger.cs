using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Notification;
using ApiObjects.Response;
using DAL;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TVinciShared;
using APILogic.ConditionalAccess;
using ApiObjects.Segmentation;
using ApiLogic.ConditionalAccess;
using ApiLogic.Users.Managers;
using ApiLogic.Modules;
using CachingProvider.LayeredCache;
using System.Threading;
using ApiLogic.Segmentation;
using Phx.Lib.Appconfig;

namespace Core.Notification
{
    public class MessageInboxManger
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const string MESSAGE_IDENTIFIER_REQUIRED = "Message identifier is required";
        private const string USER_INBOX_MESSAGE_NOT_EXIST = "User inbox message not exist";

        private static readonly Lazy<MessageInboxManger> lazy = new Lazy<MessageInboxManger>(() =>
           new MessageInboxManger(
               new UserInboxMessageStatusRepository(SetMongoConnectionString()),
               LayeredCache.Instance, NotificationSettings.Instance, AnnouncementManager.Instance, NotificationDal.Instance),
           LazyThreadSafetyMode.PublicationOnly);

        private readonly IUserInboxMessageStatusRepository _inboxMessageStatusRepository;
        private readonly ILayeredCache _layeredCache;
        private readonly INotificationSettings _notificationSettings;
        private readonly IAnnouncementManager _announcementManager;
        private readonly INotificationDal _notificationRepository;

        public static MessageInboxManger Instance { get { return lazy.Value; } }

        public MessageInboxManger(IUserInboxMessageStatusRepository inboxMessageStatusRepository, ILayeredCache layeredCache, 
            INotificationSettings notificationSettings, IAnnouncementManager announcementManager, INotificationDal notificationRepository)
        {
            _inboxMessageStatusRepository = inboxMessageStatusRepository;
            _layeredCache = layeredCache;
            _notificationSettings = notificationSettings;
            _announcementManager = announcementManager;
            _notificationRepository = notificationRepository;
        }

        private static string SetMongoConnectionString()
        {
            // sample of mongoDB connection string -->   mongodb://username:password@hostName:port/?replicaSet=myRepl
            var connectionString = $"mongodb://{ApplicationConfiguration.Current.MongoDBConfiguration.Username.Value}:" +
               $"{ApplicationConfiguration.Current.MongoDBConfiguration.Password.Value}@" +
               $"{ApplicationConfiguration.Current.MongoDBConfiguration.HostName.Value}:" +
               $"{ApplicationConfiguration.Current.MongoDBConfiguration.Port.Value}";

            if (!string.IsNullOrEmpty(ApplicationConfiguration.Current.MongoDBConfiguration.replicaSetName.Value))
            {
                return $"{connectionString}?replicaSet={ApplicationConfiguration.Current.MongoDBConfiguration.replicaSetName.Value}";
            }

            return connectionString;
        }

        public InboxMessageResponse GetInboxMessageCache(int groupId, int userId, string messageId)
        {
            var response = new InboxMessageResponse();
            //check for empty message
            if (string.IsNullOrEmpty(messageId))
            {
                log.Error("No user inbox message identifier");
                response.Status = new Status() { Code = (int)eResponseStatus.MessageIdentifierRequired, Message = MESSAGE_IDENTIFIER_REQUIRED };
                return response;
            }

            response = GetUserInboxCachedMessages(groupId, userId);
            if (response != null && response.InboxMessages.Any())
            {
                response.InboxMessages = response.InboxMessages.Where(m => m.Id.Equals(messageId)).ToList();
                response.TotalCount = response.InboxMessages.Count;
                response.Status = new Status(eResponseStatus.OK);
            }

            return response;
        }

        public Status UpdateInboxMessageStatus(int groupId, int userId, string messageId, eMessageState status)
        {
            var _status = new Status();
            
            // validate partner inbox configuration is enabled
            if (!_notificationSettings.IsPartnerInboxEnabled(groupId))
            {
                log.Error($"Partner inbox feature is off. groupId: {groupId}, userId: {userId}");
                return new Status()
                {
                    Code = (int)eResponseStatus.FeatureDisabled,
                    Message = eResponseStatus.FeatureDisabled.ToString()
                };
            }

            //check for empty message
            if (string.IsNullOrEmpty(messageId))
            {
                log.Error("No user inbox message identifier.");
                return new Status() { Code = (int)eResponseStatus.MessageIdentifierRequired, Message = MESSAGE_IDENTIFIER_REQUIRED };
            }

            // get user inbox messages
            var messages = GetUserInboxCachedMessages(groupId, userId);
            if (messages == null || messages.InboxMessages == null || messages.InboxMessages.IsEmpty())
                _status.Set(eResponseStatus.UserInboxMessagesNotExist, $"No messages for user: {userId}");
            else
            {
                // get user inbox message
                var message = messages.InboxMessages.FirstOrDefault(x => x.Id.Equals(messageId));
                if (message != null)
                {
                    _inboxMessageStatusRepository.UpsertStatus(groupId, userId, messageId, status, message.ExpirationDate);
                    _layeredCache.SetInvalidationKey(LayeredCacheKeys.GetUserMessagesStatusInvalidationKey(groupId, userId));
                    _status.Set(eResponseStatus.OK);
                }
                else
                {
                    _status.Set(eResponseStatus.UserInboxMessagesNotExist, $"Couldn't find message Id: {messageId}");
                }
            }

            return _status;
        }

        public UserInboxMessageStatus GetInboxMessageStatus(int groupId, int userId, string messageId)
        {
            var _status = new UserInboxMessageStatus();

            var message = GetUserInboxCachedMessages(groupId, userId);
            if (message != null && message.InboxMessages.Any(m => m.Id.Equals(messageId)))
            {
                _status = _inboxMessageStatusRepository.GetMessageStatus(groupId, messageId);
            }

            return _status;
        }

        public Dictionary<string, UserInboxMessageStatus> GetInboxMessageStatuses(ContextData contextData)
        {
            Dictionary<string, UserInboxMessageStatus> result = null;

            var _userId = contextData.UserId.HasValue ? (int)contextData.UserId.Value : 0;
            var key = LayeredCacheKeys.GetUserMessagesStatusKey(contextData.GroupId, _userId);
            var cacheResult = _layeredCache.Get(key,
                                                ref result,
                                                GetMessageStatusesDb,
                                                new Dictionary<string, object>() {
                                                    { "groupId", contextData.GroupId },
                                                    { "userId", _userId }
                                                },
                                                contextData.GroupId,
                                                LayeredCacheConfigNames.GET_USER_MESSAGES_STATUS,
                                                new List<string>() {
                                                    LayeredCacheKeys.GetUserMessagesStatusInvalidationKey(contextData.GroupId ,_userId)
                                                });

            return result ?? new Dictionary<string, UserInboxMessageStatus>();
        }

        private Tuple<Dictionary<string, UserInboxMessageStatus>, bool> GetMessageStatusesDb(Dictionary<string, object> arg)
        {
            var groupId = (int)arg["groupId"];
            var userId = (int)arg["userId"];
            var messages = _inboxMessageStatusRepository.GetMessageStatuses(groupId, userId);

            return new Tuple<Dictionary<string, UserInboxMessageStatus>, bool>(messages, messages != null);
        }

        private static CampaignInboxMessageMap HandleCampaignsToUser(int groupId, int userId)
        {
            //get all valid batch campaigns(by dates and status = active).
            var utcNow = DateUtils.GetUtcUnixTimestampNow();
            var contextData = new ContextData(groupId) { UserId = userId };

            var batchFilter = new BatchCampaignFilter()
            {
                StateEqual = CampaignState.ACTIVE,
                IsActiveNow = true
            };

            var batchCampaignsResponse = ApiLogic.Users.Managers.CampaignManager.Instance.ListBatchCampaigns(contextData, batchFilter);
            List<BatchCampaign> batchCampaigns = batchCampaignsResponse.HasObjects() ? batchCampaignsResponse.Objects : null;
            NotificationDal.RemoveOldCampaignsFromInboxMessageMapCB(groupId, userId, utcNow);

            //get all existing user’s batch campaigns(all existing user’s campaign messages).
            var userCampaignsMap = NotificationDal.GetCampaignInboxMessageMapCB(groupId, userId);
            var userBatchCampaignsMap = userCampaignsMap != null ?
                userCampaignsMap.Campaigns?.Where(x => x.Value?.Type == eCampaignType.Batch).ToDictionary(x => x.Key, x => x.Value)
                : new Dictionary<long, CampaignMessageDetails>();

            // add new batch campaigns to user
            if (batchCampaigns != null)
            {
                //remove shared campaigns from all campaigns list(In order to "reduce the cost" of finding the campaigns that are suitable for the user).
                var missingBatchCampaigns = batchCampaigns.Where(x => !userBatchCampaignsMap.ContainsKey(x.Id)).ToList();

                if (missingBatchCampaigns.Count > 0)
                {
                    var userSegments = UserSegmentLogic.List(groupId, userId.ToString(), out int totalCount);
                    var scope = new BatchCampaignConditionScope()
                    {
                        FilterBySegments = true,
                        SegmentIds = userSegments.Any() ? userSegments : null
                    };

                    //filter relevant campaigns by populationConditions.
                    foreach (var campaign in missingBatchCampaigns)
                    {
                        var isValid = campaign.EvaluateConditions(scope);
                        if (isValid)
                        {
                            //add all relevant campaigns to user’s inbox message with TTL by the end date of the campaign.
                            AddCampaignMessage(campaign, groupId, userId);
                        }
                    }
                }
            }

            var filter = new BatchCampaignFilter()
            {
                StateEqual = CampaignState.ARCHIVE
            };

            GenericListResponse<Campaign> archiveCampaignsResponse = CampaignManager.Instance.SearchCampaigns(new ContextData(groupId) { UserId = userId }, filter);
            if (archiveCampaignsResponse.HasObjects())
            {
                NotificationDal.RemoveArchiveCampaignFromInboxMessage(groupId, userId, archiveCampaignsResponse.Objects);
            }

            //get all existing user’s batch campaigns(all existing user’s campaign messages).
            return NotificationDal.GetCampaignInboxMessageMapCB(groupId, userId);
        }

        public static void AddCampaignMessage(Campaign campaign, int groupId, long userId, string udid = null, long? productId = null, eTransactionType? productType = null)
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

            //Todo - Matan, When the campaign is shared, remove personal message
            if (!DAL.NotificationDal.SetUserInboxMessage(groupId, inboxMessage, ttl.TotalDays))
            {
                log.Error($"Failed to add campaign message (campaign: {campaign.Id}) to User: {userId} Inbox");
            }
            else
            {
                log.Debug($"Campaign message (campaign: {campaign.Id}) sent successfully to User: {userId} Inbox");
                var campaignMessageDetails = new CampaignMessageDetails() 
                { 
                    MessageId = inboxMessage.Id, 
                    ExpiredAt = campaign.EndDate,
                    Type = campaign.CampaignType
                };

                if (!string.IsNullOrEmpty(udid))
                {
                    campaignMessageDetails.Devices.Add(udid);
                }

                if (productId.HasValue && productType.HasValue && productType.Value == eTransactionType.Subscription)
                {
                    campaignMessageDetails.SubscriptionUses.Add(productId.Value, current);
                }

                DAL.NotificationDal.SaveToCampaignInboxMessageMapCB(campaign.Id, groupId, userId, campaignMessageDetails);//update mapping
            }
        }

        internal InboxMessageResponse GetUserInboxCachedMessages(int nGroupID, int userId)
        {
            var inboxMessages = ListUserInboxMessages(nGroupID, userId);
            return new InboxMessageResponse()
            {
                InboxMessages = inboxMessages.messages,
                Status = new Status(inboxMessages.success ? eResponseStatus.OK : eResponseStatus.Error),
                TotalCount = inboxMessages.messages.Count
            };
        }

        private (bool success, List<InboxMessage> messages) ListUserInboxMessages(int groupId, int userId)
        {
            var messages = new List<InboxMessage>();

            var potentialMessages = GetAllUserPotentialMessages(groupId, userId)?.OrderBy(x=> x.CreatedAtSec).ToList();
            var contextData = new ContextData(groupId) { UserId = userId };
            Dictionary<string, UserInboxMessageStatus> statuses = null;

            if (potentialMessages != null && potentialMessages.Any())
            {
                statuses = GetInboxMessageStatuses(contextData);
            }

            //Calc message status
            foreach (var potentialMessage in potentialMessages)
            {
                UserInboxMessageStatus _status = null;
                var getStatus = statuses != null && statuses.TryGetValue(potentialMessage.Id, out _status);
                if (!getStatus || _status.Status != eMessageState.Trashed.ToString())//Not exists or notnot deletd
                {
                    if (!getStatus)
                        potentialMessage.State = eMessageState.Unread;//init
                    else
                    {
                        if (Enum.TryParse<eMessageState>(_status.Status, out var _parsedState))
                            potentialMessage.State = _parsedState;
                    }
                    messages.Add(potentialMessage);
                }
            }

            return (potentialMessages != null, messages);
        }

        private List<InboxMessage> GetAllUserPotentialMessages(int groupId, int userId)
        {
            //Get system announcements
            var _systemAnnouncements = _announcementManager.Get_AllMessageAnnouncements(groupId, 0, 0,null, false);

            //Get active campaigns
            var _campaigns = HandleCampaignsToUser(groupId, userId);

            //Get user followed series
            var _usersFollowedSeries = _announcementManager.GetUserFollowedSeries(groupId, userId);

            var potentialMessages = new List<InboxMessage>();
            int ttlDays = _notificationSettings.GetInboxMessageTTLDays(groupId);

            //System Announcement
            foreach (var sa in _systemAnnouncements?.messageAnnouncements)
            {
                var _startTime = DateUtils.UtcUnixTimestampSecondsToDateTime(sa.StartTime);
                var _date = new List<DateTime> { _startTime, DateTime.UtcNow }.Max(d => d);
                potentialMessages.Add(new InboxMessage
                {
                    Category = eMessageCategory.SystemAnnouncement,
                    Id = sa.MessageAnnouncementId.ToString(),
                    ImageUrl = sa.ImageUrl,
                    Message = sa.Message,
                    UserId = userId,
                    CreatedAtSec = sa.StartTime,
                    ExpirationDate = _date.AddDays(ttlDays)//Acceptable?
                });
            }

            //Campaigns
            foreach (var cm in _campaigns?.Campaigns)
            {
                //TODO - After making the campaign message shared, add to cache
                var _inboxMessage = _notificationRepository.GetUserInboxMessage(groupId, userId, cm.Value.MessageId);
                if (_inboxMessage != null)
                {
                    _inboxMessage.ExpirationDate = DateUtils.UtcUnixTimestampSecondsToDateTime(cm.Value.ExpiredAt);
                    _inboxMessage.Id = cm.Value.MessageId; //TODO - Matan, other Id?
                    potentialMessages.Add(_inboxMessage);
                }
            }

            //users Followed Series
            potentialMessages.AddRange(_usersFollowedSeries);

            //Mark all as unread    
            potentialMessages = potentialMessages?
                .Select(message => { message.State = eMessageState.Unread; return message; }).ToList();
            return potentialMessages;
        }
    }
}
