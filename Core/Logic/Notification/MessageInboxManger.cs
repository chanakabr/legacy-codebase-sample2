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
using Nest;
using Phx.Lib.Appconfig;
using Status = ApiObjects.Response.Status;

namespace Core.Notification
{
    public interface IMessageInboxManger
    {
        void AddCampaignInboxMessage(Campaign campaign, int groupId);
        void RemoveCampaignInboxMessage(long campaignId, int groupId);
    }

    public class MessageInboxManger : IMessageInboxManger
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const string MESSAGE_IDENTIFIER_REQUIRED = "Message identifier is required";
        private const string USER_INBOX_MESSAGE_NOT_EXIST = "User inbox message not exist";

        private static readonly Lazy<MessageInboxManger> lazy = new Lazy<MessageInboxManger>(() =>
                new MessageInboxManger(new UserInboxMessageStatusRepository(SetMongoConnectionString()),
                LayeredCache.Instance, 
                NotificationSettings.Instance, 
                AnnouncementManager.Instance,
                NotificationDal.Instance,
                CampaignUsageRepository.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        private readonly IUserInboxMessageStatusRepository _inboxMessageStatusRepository;
        private readonly ILayeredCache _layeredCache;
        private readonly INotificationSettings _notificationSettings;
        private readonly IAnnouncementManager _announcementManager;
        private readonly INotificationDal _notificationRepository;
        private readonly ICampaignUsageRepository _campaignUsageRepository;

        public static MessageInboxManger Instance
        {
            get { return lazy.Value; }
        }

        public MessageInboxManger(IUserInboxMessageStatusRepository inboxMessageStatusRepository,
                                  ILayeredCache layeredCache,
                                  INotificationSettings notificationSettings, 
                                  IAnnouncementManager announcementManager,
                                  INotificationDal notificationRepository,
                                  ICampaignUsageRepository campaignUsageRepository)
        {
            _inboxMessageStatusRepository = inboxMessageStatusRepository;
            _layeredCache = layeredCache;
            _notificationSettings = notificationSettings;
            _announcementManager = announcementManager;
            _notificationRepository = notificationRepository;
            _campaignUsageRepository = campaignUsageRepository;
        }

        private static string SetMongoConnectionString()
        {
            // sample of mongoDB connection string -->   mongodb://username:password@hostName:port/?replicaSet=myRepl
            var connectionString =
                $"mongodb://{ApplicationConfiguration.Current.MongoDBConfiguration.Username.Value}:" +
                $"{ApplicationConfiguration.Current.MongoDBConfiguration.Password.Value}@" +
                $"{ApplicationConfiguration.Current.MongoDBConfiguration.HostName.Value}:" +
                $"{ApplicationConfiguration.Current.MongoDBConfiguration.Port.Value}";

            if (!string.IsNullOrEmpty(ApplicationConfiguration.Current.MongoDBConfiguration.replicaSetName.Value))
            {
                return
                    $"{connectionString}?replicaSet={ApplicationConfiguration.Current.MongoDBConfiguration.replicaSetName.Value}";
            }

            return connectionString;
        }

        public InboxMessageResponse GetInboxMessageCache(int groupId, long domainId, int userId, string messageId)
        {
            var response = new InboxMessageResponse();
            //check for empty message
            if (string.IsNullOrEmpty(messageId))
            {
                log.Error("No user inbox message identifier");
                response.Status = new Status()
                    { Code = (int)eResponseStatus.MessageIdentifierRequired, Message = MESSAGE_IDENTIFIER_REQUIRED };
                return response;
            }

            response = GetUserInboxCachedMessages(groupId, domainId, userId);
            if (response != null && response.InboxMessages.Any())
            {
                response.InboxMessages = response.InboxMessages.Where(m => m.Id.Equals(messageId)).ToList();
                response.TotalCount = response.InboxMessages.Count;
                response.Status = new Status(eResponseStatus.OK);
            }

            return response;
        }

        public Status UpdateInboxMessageStatus(int groupId, long domainId, int userId, string messageId, eMessageState status)
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
                return new Status()
                    { Code = (int)eResponseStatus.MessageIdentifierRequired, Message = MESSAGE_IDENTIFIER_REQUIRED };
            }

            // get user inbox messages
            var messages = GetUserInboxCachedMessages(groupId, domainId, userId);
            if (messages == null || messages.InboxMessages == null || messages.InboxMessages.IsEmpty())
                _status.Set(eResponseStatus.UserInboxMessagesNotExist, $"No messages for user: {userId}");
            else
            {
                // get user inbox message
                var message = messages.InboxMessages.FirstOrDefault(x => x.Id.Equals(messageId));
                if (message != null)
                {
                    _inboxMessageStatusRepository.UpsertStatus(groupId, userId, messageId, status,
                        message.ExpirationDate);
                    _layeredCache.SetInvalidationKey(
                        LayeredCacheKeys.GetUserMessagesStatusInvalidationKey(groupId, userId));
                    _status.Set(eResponseStatus.OK);
                }
                else
                {
                    _status.Set(eResponseStatus.UserInboxMessagesNotExist, $"Couldn't find message Id: {messageId}");
                }
            }

            return _status;
        }

        public UserInboxMessageStatus GetInboxMessageStatus(int groupId, long domainId, int userId, string messageId)
        {
            var _status = new UserInboxMessageStatus();

            var message = GetUserInboxCachedMessages(groupId, domainId, userId);
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
                new Dictionary<string, object>()
                {
                    { "groupId", contextData.GroupId },
                    { "userId", _userId }
                },
                contextData.GroupId,
                LayeredCacheConfigNames.GET_USER_MESSAGES_STATUS,
                new List<string>()
                {
                    LayeredCacheKeys.GetUserMessagesStatusInvalidationKey(contextData.GroupId, _userId)
                });

            return result ?? new Dictionary<string, UserInboxMessageStatus>();
        }

        private Tuple<Dictionary<string, UserInboxMessageStatus>, bool> GetMessageStatusesDb(
            Dictionary<string, object> arg)
        {
            var groupId = (int)arg["groupId"];
            var userId = (int)arg["userId"];
            var messages = _inboxMessageStatusRepository.GetMessageStatuses(groupId, userId);

            return new Tuple<Dictionary<string, UserInboxMessageStatus>, bool>(messages, messages != null);
        }

        private CampaignInboxMessageMap HandleCampaignsToUser(int groupId, long domainId, int userId)
        {
            var utcNow = DateUtils.GetUtcUnixTimestampNow();

            // clean existing campaign map to user (all archive + expired)
            var archiveFilter = new CampaignSearchFilter() { StateEqual = CampaignState.ARCHIVE, IgnoreSetFilterByShop = true };
            var archiveCampaignsResponse = CampaignManager.Instance.SearchCampaigns(new ContextData(groupId) { UserId = userId }, archiveFilter);
            var archiveCampaigns = archiveCampaignsResponse.HasObjects() ? archiveCampaignsResponse.Objects.Select(x => x.Id) : new List<long>();
            _campaignUsageRepository.CleanCampaignInboxMessageMap(groupId, userId, archiveCampaigns, utcNow);

            //get all valid batch campaigns (by dates and status = active).
            var batchFilter = new BatchCampaignFilter()
            {
                StateEqual = CampaignState.ACTIVE,
                IsActiveNow = true,
                IgnoreSetFilterByShop = true
            };
            var batchCampaignsResponse = CampaignManager.Instance.ListBatchCampaigns(new ContextData(groupId) { UserId = userId }, batchFilter);
            List<BatchCampaign> batchCampaigns = batchCampaignsResponse.HasObjects() ? batchCampaignsResponse.Objects : null;
            
            //get all existing user’s batch campaigns(all existing user’s campaign messages).
            var userCampaignsMap = _campaignUsageRepository.GetCampaignInboxMessageMapCB(groupId, userId);

            // add new batch campaigns to user
            if (batchCampaigns != null && batchCampaigns.Any())
            {
                BatchCampaignConditionScope scope = null;
                foreach (var batchCampaign in batchCampaigns)
                {
                    if (userCampaignsMap.Campaigns.ContainsKey(batchCampaign.Id)) { continue; }
                    if (scope == null)
                    {
                        scope = new BatchCampaignConditionScope()
                        {
                            FilterBySegments = true,
                            SegmentIds = ConditionalAccess.Utils.GetDomainSegments(groupId, domainId, new List<string> { userId.ToString()})
                        };
                    }

                    if (batchCampaign.EvaluateConditions(scope))
                    {
                        Instance.AddCampaignMessageToUser(batchCampaign, groupId, userId);
                    }
                }
            }

            return _campaignUsageRepository.GetCampaignInboxMessageMapCB(groupId, userId);
        }

        public void RemoveCampaignInboxMessage(long campaignId, int groupId)
        {
            _notificationRepository.DeleteCampaignInboxMessage(groupId, campaignId);
        }

        public void AddCampaignInboxMessage(Campaign campaign, int groupId)
        {
            var current = DateUtils.GetUtcUnixTimestampNow();
            var expirationDate = DateUtils.UtcUnixTimestampSecondsToDateTime(campaign.EndDate);
            
            // Add single campaign InboxMessage
            var inboxMessage = new InboxMessage
            {
                Id = Guid.NewGuid().ToString(),
                Message = campaign.Message,
                CreatedAtSec = current,
                UpdatedAtSec = current,
                ExpirationDate = expirationDate,
                State = eMessageState.Unread,
                Category = eMessageCategory.Campaign,
                CampaignId = campaign.Id,
                UserId = 0 //for all users
            };

            var ttl = (expirationDate - DateTime.UtcNow).TotalDays;
            if (!_notificationRepository.SetCampaignInboxMessage(groupId, inboxMessage, ttl))
            {
                log.Error($"Failed to add campaign {campaign.Id} inbox message");
            }
        }

        public void AddCampaignMessageToUser(Campaign campaign, int groupId, long userId, string udid = null, long? productId = null, eTransactionType? productType = null)
        {
            var inboxMessage = _notificationRepository.GetCampaignInboxMessage(groupId, campaign.Id);
            if (inboxMessage == default)
            {
                log.Debug($"Campaign {campaign.Id} inbox message does not exist");
                return;
            }

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
                //update user's mapping
                var current = DateUtils.GetUtcUnixTimestampNow();
                campaignMessageDetails.SubscriptionUses.Add(productId.Value, current);
            }

            // update mapping
            if (!_campaignUsageRepository.SaveToCampaignInboxMessageMapCb(campaign.Id, groupId, userId, campaignMessageDetails))
            {
                log.Error($"Failed to add Campaign [{campaign.Id}] MessageDetails to User [{userId}] Inbox");
            }
        }

        internal InboxMessageResponse GetUserInboxCachedMessages(int nGroupID, long domainId, int userId)
        {
            var inboxMessages = ListUserInboxMessages(nGroupID, domainId, userId);
            return new InboxMessageResponse()
            {
                InboxMessages = inboxMessages.messages,
                Status = new Status(inboxMessages.success ? eResponseStatus.OK : eResponseStatus.Error),
                TotalCount = inboxMessages.messages.Count
            };
        }

        private (bool success, List<InboxMessage> messages) ListUserInboxMessages(int groupId, long domainId, int userId)
        {
            var messages = new List<InboxMessage>();

            var potentialMessages = GetAllUserPotentialMessages(groupId, domainId, userId)?.OrderBy(x=> x.CreatedAtSec).ToList();
            var contextData = new ContextData(groupId) { UserId = userId };
            Dictionary<string, UserInboxMessageStatus> statuses = null;

            if (potentialMessages != null && potentialMessages.Any())
            {
                statuses = GetInboxMessageStatuses(contextData);
            }

            //Calc message status
            if (potentialMessages != null)
            {
                foreach (var potentialMessage in potentialMessages)
                {
                    UserInboxMessageStatus status = null;
                    var getStatus = statuses != null && statuses.TryGetValue(potentialMessage.Id, out status);
                    if (!getStatus || status.Status != eMessageState.Trashed.ToString())
                    {
                        if (!getStatus)
                            potentialMessage.State = eMessageState.Unread; //init
                        else
                        {
                            if (Enum.TryParse<eMessageState>(status.Status, out var parsedState))
                                potentialMessage.State = parsedState;
                        }

                        messages.Add(potentialMessage);
                    }
                }
            }

            return (potentialMessages != null, messages);
        }

        private List<InboxMessage> GetAllUserPotentialMessages(int groupId, long domainId, int userId)
        {
            //Get system announcements
            var systemAnnouncements = _announcementManager.Get_AllMessageAnnouncements(groupId, 0, 0, null, false);

            //Get user followed series
            var usersFollowedSeries = _announcementManager.GetUserFollowedSeries(groupId, userId);

            var potentialMessages = new List<InboxMessage>();
            int ttlDays = _notificationSettings.GetInboxMessageTTLDays(groupId);

            //System Announcement
            if (systemAnnouncements?.messageAnnouncements != null)
                foreach (var sa in systemAnnouncements.messageAnnouncements)
                {
                    var startTime = DateUtils.UtcUnixTimestampSecondsToDateTime(sa.StartTime);
                    var date = new List<DateTime> { startTime, DateTime.UtcNow }.Max(d => d);
                    potentialMessages.Add(new InboxMessage
                    {
                        Category = eMessageCategory.SystemAnnouncement,
                        Id = sa.MessageAnnouncementId.ToString(),
                        ImageUrl = sa.ImageUrl,
                        Message = sa.Message,
                        UserId = userId,
                        CreatedAtSec = sa.StartTime,
                        ExpirationDate = date.AddDays(ttlDays)
                    });
                }

            //Get active campaigns
            var campaigns = HandleCampaignsToUser(groupId, domainId, userId);

            if (campaigns?.Campaigns != null)
            {
                foreach (var campaign in campaigns.Campaigns)
                {
                    // message for each campaign - new behavior of getting Campaigns messages
                    var campaignInboxMessage = _notificationRepository.GetCampaignInboxMessage(groupId, campaign.Key);
                    if (campaignInboxMessage == null)
                    {
                        // message for each user - old behavior of getting Campaigns messages
                        campaignInboxMessage = _notificationRepository.GetUserInboxMessage(groupId, userId, campaign.Value.MessageId);
                    }

                    if (campaignInboxMessage != null)
                    {
                        campaignInboxMessage.UserId = userId;
                        potentialMessages.Add(campaignInboxMessage);
                    }
                }
            }

            //users Followed Series
            potentialMessages.AddRange(usersFollowedSeries);

            //Mark all as unread    
            potentialMessages = potentialMessages?
                .Select(message =>
                {
                    message.State = eMessageState.Unread;
                    return message;
                }).ToList();
            return potentialMessages;
        }
    }
}