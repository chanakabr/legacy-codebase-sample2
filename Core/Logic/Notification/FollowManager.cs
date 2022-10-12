using APILogic.AmazonSnsAdapter;
using APILogic.DmsService;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Notification;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.Catalog.Request;
using Core.Catalog.Response;
using Core.Notification.Adapters;
using DAL;
using Newtonsoft.Json;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ApiObjects.Catalog;
using TVinciShared;

namespace Core.Notification
{
    public class FollowManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public static DateTime oldEpgSendDate { get; set; }

        private static readonly Lazy<FollowManager> lazy = new Lazy<FollowManager>(() => new FollowManager());

        public static FollowManager Instance { get { return lazy.Value; } }

        private FollowManager() { }

        #region Consts

        private const string MESSAGE_PLACEHOLDERS_ARE_INVALID = "Message placeholders are not valid";
        private const string URL_PLACEHOLDERS_ARE_INVALID = "URL placeholders are not valid";
        private const string DATETIME_FORMAT_IS_INVALID = "Date time format is invalid";
        private const string FOLLOW_TEMPLATE_NOT_FOUND = "Message template not found";
        private static string CatalogSignString = Guid.NewGuid().ToString();
        private static string CatalogSignatureKey = ApplicationConfiguration.Current.CatalogSignatureKey.Value;
        private const string FOLLOW_PHRASE_FORMAT = "{0}='{1}'";
        #endregion

        #region Public Methods

        public static MessageTemplateResponse SetMessageTemplate(int groupId, MessageTemplate messageTemplate)
        {
            MessageTemplateResponse response = new MessageTemplateResponse
            {
                Status = ValidateMessageTemplate(groupId, messageTemplate)
            };

            if (response.Status.Code != (int)eResponseStatus.OK)
            {
                return response;
            }

            response.MessageTemplate = SetMessageTemplateAtDB(groupId, messageTemplate);

            if (response.MessageTemplate == null || response.MessageTemplate.Id <= 0)
            {
                log.ErrorFormat("Error while inserting messageTemplate {0} to DB", messageTemplate.ToString());
                response.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }

            // clear message templates cache
            NotificationCache.Instance().RemoveMessageTemplateFromCache(groupId);

            return response;
        }

        public static MessageTemplateResponse GetMessageTemplate(int groupId, MessageTemplateType assetType)
        {
            MessageTemplateResponse response = new MessageTemplateResponse();
            response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            response.MessageTemplate = NotificationCache.Instance().GetMessageTemplates(groupId).FirstOrDefault(x => x.TemplateType == assetType);

            if (response.MessageTemplate == null || response.MessageTemplate.Id <= 0)
            {
                response.Status = new Status((int)eResponseStatus.MessageTemplateNotFound, FOLLOW_TEMPLATE_NOT_FOUND);
                log.ErrorFormat("GetMessageTemplate: message template not found: GID: {0}, assetType: {1}", groupId, assetType.ToString());
            }

            return response;
        }

        public static Status ValidatePlaceholders<TEnum>(ref string message) where TEnum : struct
        {
            Status validationStatus = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            if (!string.IsNullOrEmpty(message))
            {
                //check for letters only inside {}
                //--------------------------------
                Regex expression = new Regex(@"\{([^}]*)\}");
                // get message place-holders
                var messagePlaceHolders = expression.Matches(message).Cast<Match>().Select(m => m.Value.Replace("{", "").Replace("}", "")).ToArray();
                //get enum values
                var followSeriesPlaceHoldersEnumValues = Enum.GetNames(typeof(TEnum));
                //compare string [message place-holders] not in [enum values]
                var notInEnum = messagePlaceHolders.Select(m => m.ToLower()).Except(followSeriesPlaceHoldersEnumValues.Select(x => x.ToLower())).ToList();
                //
                if (notInEnum.Count > 0)
                {
                    log.ErrorFormat("ValidateSeriesPlaceholders - Wrong place-holders {0}, message: {1}", string.Join(", ", notInEnum), message);
                    validationStatus.Code = (int)eResponseStatus.Error;
                }

                // replace placeHoldaer to the enums exact values
                TEnum tmpeFollowSeriesPlaceHolders;
                foreach (var s in messagePlaceHolders)
                {
                    if (Enum.TryParse<TEnum>(s, true, out tmpeFollowSeriesPlaceHolders))
                    {
                        message = message.Replace(s, tmpeFollowSeriesPlaceHolders.ToString());
                    }
                }
            }

            return validationStatus;
        }

        public static GenericResponse<DbAnnouncement> CreateFollowAnnouncement(FollowDataBase followItem)
        {
            GenericResponse<DbAnnouncement> response = new GenericResponse<DbAnnouncement>();

            try
            {
                int announcementId = 0;
                string announcementName = followItem.Title;

                // create Amazon topic
                string externalAnnouncementId = string.Empty;
                if (NotificationSettings.IsPartnerPushEnabled(followItem.GroupId))
                {
                    externalAnnouncementId = NotificationAdapter.CreateAnnouncement(followItem.GroupId, announcementName, true);
                    if (string.IsNullOrEmpty(externalAnnouncementId))
                    {
                        log.DebugFormat("failed to create announcement groupID = {0}, announcementName = {1}", followItem.GroupId, announcementName);
                        response.SetStatus(eResponseStatus.FailCreateAnnouncement, "fail create Guest announcement");
                        return response;
                    }
                }

                string mailExternalAnnouncementId = string.Empty;
                if (NotificationSettings.IsPartnerMailNotificationEnabled(followItem.GroupId))
                {
                    mailExternalAnnouncementId = MailNotificationAdapterClient.CreateAnnouncement(followItem.GroupId, announcementName);
                    if (string.IsNullOrEmpty(externalAnnouncementId))
                    {
                        log.DebugFormat("failed to create announcement groupID = {0}, announcementName = {1}", followItem.GroupId, announcementName);
                        response.SetStatus(eResponseStatus.FailCreateAnnouncement, "fail create Guest announcement");
                        return response;
                    }
                }

                // create DB announcement
                announcementId = NotificationDal.Insert_Announcement(followItem.GroupId, announcementName, externalAnnouncementId, (int)eMessageType.Push,
                    (int)eAnnouncementRecipientsType.Other, mailExternalAnnouncementId, followItem.FollowPhrase, followItem.FollowReference);
                if (announcementId == 0)
                {
                    log.DebugFormat("failed to insert announcement to DB groupID = {0}, announcementName = {1}", followItem.GroupId, announcementName);
                    response.SetStatus(eResponseStatus.Error, "fail insert guest announcement to DB");
                }
                else
                {
                    NotificationCache.Instance().RemoveAnnouncementsFromCache(followItem.GroupId);

                    response.Object = new DbAnnouncement()
                    {
                        ExternalId = externalAnnouncementId,
                        FollowPhrase = followItem.FollowPhrase,
                        FollowReference = followItem.FollowReference,
                        ID = announcementId,
                        Name = announcementName,
                        RecipientsType = eAnnouncementRecipientsType.Other,
                        MailExternalId = mailExternalAnnouncementId
                    };

                    response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("CreateFollowAnnouncement failed groupId = {0}, ex = {1}", followItem.GroupId, ex.Message), ex);
                return response;
            }

            return response;
        }

        public static GetUserFollowsResponse Get_UserFollows(int groupId, int userId, int pageSize, int pageIndex, ApiObjects.SearchObjects.OrderDir order, bool isFollowTvSeriesRequest = false)
        {
            GetUserFollowsResponse userFollowResponse = new GetUserFollowsResponse();
            userFollowResponse.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            userFollowResponse.Follows = new List<FollowDataBase>();

            // get user notifications
            bool docExists = false;
            UserNotification userNotificationData = DAL.NotificationDal.GetUserNotificationData(groupId, userId, ref docExists);

            if (userNotificationData == null && docExists)
            {
                userFollowResponse.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.ErrorFormat("Error retrieving User notification data. PID: {0}, UID: {1}", groupId, userId);
                return userFollowResponse;
            }

            if (userNotificationData != null &&
                userNotificationData.Announcements != null &&
                userNotificationData.Announcements.Count > 0)
            {
                // get announcement from DB
                List<DbAnnouncement> dbAnnouncements = null;
                NotificationCache.Instance().TryGetAnnouncements(groupId, ref dbAnnouncements);

                if (dbAnnouncements != null)
                {
                    // filter out mail if its for follow tv series
                    if (isFollowTvSeriesRequest)
                    {
                        dbAnnouncements = dbAnnouncements.Where(ann => ann.RecipientsType != eAnnouncementRecipientsType.Mail &&
                                                                       userNotificationData.Announcements.Select(userAnn => userAnn.AnnouncementId).Contains(ann.ID)).ToList();
                    }
                    else
                    {
                        dbAnnouncements = dbAnnouncements.Where(ann => userNotificationData.Announcements.Select(userAnn => userAnn.AnnouncementId).Contains(ann.ID)).ToList();
                    }
                }

                List<Announcement> userAnnouncements = userNotificationData.Announcements;

                if (userAnnouncements != null && userAnnouncements.Count > 0)
                {
                    // create response object
                    foreach (var currDBAnnouncement in dbAnnouncements)
                    {
                        Announcement currAnnouncement = userAnnouncements.FirstOrDefault(userAnn => userAnn.AnnouncementId == currDBAnnouncement.ID);

                        if (currAnnouncement != null)
                        {
                            FollowDataBase currFollowDataBase = new FollowDataBase(groupId, currDBAnnouncement.FollowPhrase)
                            {
                                AnnouncementId = currDBAnnouncement.ID,
                                Status = 1,                         // only enabled status in this phase
                                Title = currDBAnnouncement.Name,
                                FollowReference = currDBAnnouncement.FollowReference,
                                Timestamp = currAnnouncement.AddedDateSec,
                            };

                            userFollowResponse.Follows.Add(currFollowDataBase);
                        }
                    }

                    userFollowResponse.Follows = userFollowResponse.Follows.OrderBy(x => x.Timestamp).ToList();
                }
            }
            else
                log.DebugFormat("User doesn't have any follow notifications. PID: {0}, UID: {1}", groupId, userId);

            if (userFollowResponse.Follows != null && userFollowResponse.Follows.Count > 0)
            {
                // set result order
                if (order == ApiObjects.SearchObjects.OrderDir.DESC)
                    userFollowResponse.Follows.Reverse();

                // update total results
                userFollowResponse.TotalCount = userFollowResponse.Follows.Count;

                // paging - pageSize = 0 return everything
                if (pageSize > 0)
                {
                    userFollowResponse.Follows = userFollowResponse.Follows.Skip(pageSize * pageIndex).Take(pageSize).ToList();
                }
            }

            return userFollowResponse;
        }

        public static Status Unfollow(int groupId, long userId, FollowDataBase followData)
        {
            Status statusResult = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            var followPhrase = GetFollowPhrase(groupId, followData);

            if (followPhrase == null)
            {
                return statusResult;
            }

            followData.FollowPhrase = followPhrase;

            // get announcement from DB
            List<DbAnnouncement> announcements = null;
            NotificationCache.Instance().TryGetAnnouncements(groupId, ref announcements);

            // get user announcement
            DbAnnouncement userDbAnnouncement = null;
            if (announcements != null)
                userDbAnnouncement = announcements.FirstOrDefault(ann => ann.FollowPhrase.ToLower() == followData.FollowPhrase.ToLower());

            if (userDbAnnouncement == null)
            {
                log.ErrorFormat("user is not following any asset. group: {0}, user: {1}, phrase: {2}", groupId, userId, followData.FollowPhrase);
                return new Status((int)eResponseStatus.UserNotFollowing, "user is not following asset");
            }

            statusResult = RemoveFollowItemFromUser(groupId, userId, userDbAnnouncement);

            return statusResult;
        }

        public static Status Delete(int groupId, long userId, int assetId)
        {
            var statusResult = new Status((int) eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                var seriesNameResult = TryGetSeriesNameAndAssetStructId(groupId, assetId, out var seriesName, out _);
                if (!seriesNameResult.IsOkStatusCode())
                {
                    return seriesNameResult;
                }

                var followData = new FollowDataTvSeries
                {
                    AssetId = assetId,
                    GroupId = groupId,
                    Title = seriesName
                };

                statusResult = Unfollow(groupId, userId, followData);
            }
            catch (Exception e)
            {
                log.ErrorFormat($"An Exception occurred while Deleting FollowDataTvSeries. {nameof(groupId)}:{groupId}, {nameof(userId)}:{userId}, {nameof(assetId)}:{assetId}. ex: {e}");
            }

            return statusResult;
        }

        internal static Status DeletePersonalListItemFromUser(int groupId, int userId, long personalListId)
        {
            // get announcement from DB
            List<DbAnnouncement> announcements = null;
            NotificationCache.Instance().TryGetAnnouncements(groupId, ref announcements);

            // get user announcement
            DbAnnouncement userDbAnnouncement = null;
            if (announcements != null)
                userDbAnnouncement = announcements.FirstOrDefault(ann => ann.ID == personalListId);

            if (userDbAnnouncement == null)
            {
                log.ErrorFormat("user is not following any asset. group: {0}, user: {1}, personalListId: {2}", groupId, userId, personalListId);
                return new Status((int)eResponseStatus.UserNotFollowing, "user is not following asset");
            }

            return RemoveFollowItemFromUser(groupId, userId, userDbAnnouncement);
        }

        internal static GenericResponse<FollowDataBase> AddPersonalListItemToUser(int userId, FollowDataBase personalListItemToFollow)
        {
            return AddFollowItemToUser(userId, personalListItemToFollow);
        }

        public static GenericResponse<FollowDataBase> Follow(int groupId, int userId, FollowDataBase followData)
        {
            GenericResponse<FollowDataBase> response = new GenericResponse<FollowDataBase>();
            followData.GroupId = groupId;

            var followPhrase = GetFollowPhrase(groupId, followData);
            if (followPhrase == null)
            {
                response.SetStatus(eResponseStatus.InvalidAssetId, "invalid asset");
                return response;
            }

            followData.FollowPhrase = followPhrase;

            // validate asset type
            var isFollowDataValidate = false;
            if (followData is FollowDataTvSeries)
            {
                isFollowDataValidate = NotificationCache.Instance().GetOTTAssetTypeByMediaTypeId(groupId, followData.Type) == eOTTAssetTypes.Series;
                if (!isFollowDataValidate)
                    log.DebugFormat("Invalid asset: group: {0}, user: {1}, asset: {2}", groupId, userId, (followData as FollowDataTvSeries).AssetId);
            }

            if (!isFollowDataValidate)
            {
                response.SetStatus(eResponseStatus.InvalidAssetId, "invalid asset");
                return response;
            }

            response = AddFollowItemToUser(userId, followData);

            return response;
        }

        public static void AddTvSeriesFollowRequestForNonOpc(int groupId, string userId, int mediaID)
        {
            // Get catalog start date & series name && validate the media type ID is a series
            MediaObj episodeMedia = GetMediaObj(groupId, mediaID, userId);
            if (episodeMedia == null || episodeMedia.m_oMediaType.m_nTypeID != NotificationCache.Instance().GetEpisodeMediaTypeId(groupId))
            {
                return;
            }

            var episodeAssociationTag = GetEpisodeAssociationTagForNonOpc(groupId);
            var seriesNames = GetSeriesNames(ApiObjects.MetaType.Tag, episodeAssociationTag, episodeMedia.m_lTags);

            // check series name was found
            if (seriesNames.Count == 0)
            {
                log.DebugFormat("ingested media is not a series episode (series name is empty). group {0}, media: {1}", groupId, mediaID);
                return;
            }
            else
            {
                log.DebugFormat("ingested media is an episode of a series. group {0}, media: {1}, series name: {2}", groupId, mediaID, string.Join(",", seriesNames));
            }

            // build of series phrases
            var phrases = seriesNames.Select(x => GetFollowPhrase(groupId, x)).ToList();

            // get announcement of message
            List<DbAnnouncement> dbAnnouncements = null;
            NotificationCache.Instance().TryGetAnnouncements(groupId, ref dbAnnouncements);
            if (dbAnnouncements != null)
                dbAnnouncements = dbAnnouncements.Where(dbAnn => phrases.Contains(dbAnn.FollowPhrase)).ToList();

            if (dbAnnouncements == null || dbAnnouncements.Count == 0)
            {
                log.Debug($"no announcements found for ingested media: group {groupId}, media: {mediaID}, search phrase phrases: {string.Join(",", phrases)}");
                return;
            }
            else
            {
                log.DebugFormat("announcement found for ingested media: GID: {0}, media ID: {1}, Announcement ID: {2}, announcement name: {3}",
                                groupId, mediaID, dbAnnouncements[0].ID, dbAnnouncements[0].Name);
            }

            // validate if announcement should be automatically sent
            DbAnnouncement announcement = dbAnnouncements[0];
            if ((announcement.AutomaticIssueFollowNotification.HasValue && !announcement.AutomaticIssueFollowNotification.Value)
              || (!announcement.AutomaticIssueFollowNotification.HasValue && !NotificationSettings.ShouldIssueAutomaticFollowNotification(groupId)))
            {
                log.DebugFormat("Notification wasn't sent due to 'ShouldIssueAutomaticFollowNotification' parameter is false. group:{0}, media:{1}, announcement:{2}.",
                                groupId, mediaID, announcement.ToString());
                return;
            }

            // get message template and build message with it
            var messageTemplateResponse = GetMessageTemplate(groupId, MessageTemplateType.Series);
            if (!messageTemplateResponse.Status.IsOkStatusCode())
            {
                log.ErrorFormat("Series message template not found. group:{0}, error:{1}.", groupId, messageTemplateResponse.Status.ToString());
                return;
            }

            var message = new MessageAnnouncement()
            {
                AnnouncementId = announcement.ID,
                Enabled = true,
                Message = messageTemplateResponse.MessageTemplate.Message,
                MessageReference = mediaID.ToString(),
                Name = string.Format("announcement_{0}_{1}", eOTTAssetTypes.Series.ToString(), mediaID),
                Recipients = eAnnouncementRecipientsType.Other,
                StartTime = DateUtils.DateTimeToUtcUnixTimestampSeconds(episodeMedia.m_dCatalogStartDate),
                Timezone = "UTC",
                Status = eAnnouncementStatus.NotSent,
                IncludeUserInbox = true
            };

            SendMessageAnnouncement(announcement, message, mediaID, groupId);
        }

        public Status AddTvSeriesFollowRequestForOpc(int groupId, MediaAsset episodeMediaAsset, CatalogGroupCache cache)
        {
            var response = new Status(eResponseStatus.Error);

            if (!TryGetEpisodeAssetStructByEpisode(episodeMediaAsset, cache, out AssetStruct episodeAssetStruct) || !episodeMediaAsset.CatalogStartDate.HasValue)
            {
                response.Set(eResponseStatus.InvalidAssetId, "invalid asset");
                log.DebugFormat(response.ToString());
                return response;
            }

            var tags = episodeMediaAsset.Tags;
            var metas = episodeMediaAsset.Metas;


            if ((tags == null || tags.Count == 0) && (metas == null || metas.Count == 0))
            {
                AssetManager.GetMediaAssetMetasAndTags(groupId, episodeMediaAsset.Id, out metas, out tags);
            }

            var topic = cache.TopicsMapById[episodeAssetStruct.ConnectedParentMetaId.Value];
            var seriesName = GetSeriesNames(topic.Type, topic.SystemName, episodeMediaAsset.Tags, episodeMediaAsset.Metas).FirstOrDefault();
            if (string.IsNullOrEmpty(seriesName))
            {
                log.DebugFormat("ingested media is not a series episode (series name is empty). group {0}, media: {1}", groupId, episodeMediaAsset.Id);
                response.Set(eResponseStatus.Error, "ingested media is not a series episode (series name is empty)");
                return response;
            }

            // Already validated that ParentId is not null
            var followPhrase = GetFollowPhrase(groupId, seriesName, episodeAssetStruct.ParentId.Value);
            DbAnnouncement announcement = null;
            List<DbAnnouncement> dbAnnouncements = null;
            if (NotificationCache.Instance().TryGetAnnouncements(groupId, ref dbAnnouncements))
            {
                announcement = dbAnnouncements.FirstOrDefault(x => x.FollowPhrase == followPhrase);
            }

            if (announcement == null)
            {
                log.DebugFormat("no announcements found for ingested media: group {0}, media: {1}, followPhrase: {2}", groupId, episodeMediaAsset.Id, followPhrase);
                response.Set(eResponseStatus.Error, "no announcements found for ingested media");
                return response;
            }

            // validate if announcement should be automatically sent
            if ((announcement.AutomaticIssueFollowNotification.HasValue && !announcement.AutomaticIssueFollowNotification.Value)
               || (!announcement.AutomaticIssueFollowNotification.HasValue && !NotificationSettings.ShouldIssueAutomaticFollowNotification(groupId)))
            {
                log.DebugFormat("Notification wasn't sent due to 'ShouldIssueAutomaticFollowNotification' parameter is false. group:{0}, media:{1}, announcement:{2}.",
                                groupId, episodeMediaAsset.Id, announcement.ToString());
                response.Set(eResponseStatus.Error, "Notification wasn't sent due to 'ShouldIssueAutomaticFollowNotification' parameter is false");
                return response;
            }

            // get message template and build message with it
            var messageTemplateResponse = GetMessageTemplate(groupId, MessageTemplateType.Series);
            if (!messageTemplateResponse.Status.IsOkStatusCode())
            {
                log.ErrorFormat("Series message template not found. group:{0}, error:{1}.", groupId, messageTemplateResponse.Status.ToString());
                response.Set(messageTemplateResponse.Status);
                return response;
            }

            var message = new MessageAnnouncement()
            {
                AnnouncementId = announcement.ID,
                Enabled = true,
                Message = messageTemplateResponse.MessageTemplate.Message,
                MessageReference = episodeMediaAsset.Id.ToString(),
                Name = string.Format("announcement_{0}_{1}", eOTTAssetTypes.Series.ToString(), episodeMediaAsset.Id),
                Recipients = eAnnouncementRecipientsType.Other,
                StartTime = episodeMediaAsset.CatalogStartDate.Value.ToUtcUnixTimestampSeconds(),
                Timezone = "UTC",
                Status = eAnnouncementStatus.NotSent,
                IncludeUserInbox = true
            };

            // check if previous unsent messages were queued for this announcement and asset (series).
            if (SendMessageAnnouncement(announcement, message, episodeMediaAsset.Id, groupId))
            {
                response.Set(eResponseStatus.OK);
            }

            return response;
        }

        public static IdsResponse Get_FollowedAssetIdsFromAssets(int groupId, int userId, List<int> assets)
        {
            IdsResponse response = new IdsResponse(new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString()), new List<int>());

            var userFollows = Get_UserFollows(groupId, userId, 1000, 0, ApiObjects.SearchObjects.OrderDir.ASC);
            if (userFollows == null ||
                userFollows.Status == null ||
                userFollows.Status.Code != (int)eResponseStatus.OK ||
                userFollows.Follows == null ||
                userFollows.Follows.Count == 0)
            {
                // error is printed in Get_UserFollows
                response.Status = userFollows.Status;
                return response;
            }

            List<int> followedAssets = Get_FollowedAssets(groupId, userFollows.Follows);

            response.Ids = followedAssets.Where(x => assets.Contains(x)).ToList();

            return response;
        }

        public static IdListResponse GetUserFeeder(int groupId, int userId, int pageSize, int pageIndex, OrderObj orderObj)
        {
            IdListResponse response = new IdListResponse() { Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString()), Ids = new List<int>() };
            List<int> mediaIds = null;

            bool isDocExist = false;
            UserNotification userNotificationData = NotificationDal.GetUserNotificationData(groupId, userId, ref isDocExist);

            if (userNotificationData == null ||
                userNotificationData.Announcements == null ||
                userNotificationData.Announcements.Count == 0)
            {
                log.DebugFormat("User doesn't have any follow announcements. GID: {0}, user ID: {1}", groupId, userId);
                return response;
            }

            // get announcement from DB
            List<DbAnnouncement> dbAnnouncements = null;
            NotificationCache.Instance().TryGetAnnouncements(groupId, ref dbAnnouncements);

            if (dbAnnouncements != null)
            {
                var announcements = dbAnnouncements.Where(ann => userNotificationData.Announcements.Select(userAnn => userAnn.AnnouncementId).Contains(ann.ID) && ann.RecipientsType == eAnnouncementRecipientsType.Other);
                if (announcements != null && announcements.ToList<DbAnnouncement>().Count > 0)
                {
                    dbAnnouncements = announcements.ToList();
                }
            }

            if (dbAnnouncements == null || dbAnnouncements.Count == 0)
            {
                log.ErrorFormat("user announcements were not found on the DB. GID: {0}, user ID: {1}", groupId, userId);
                response.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                return response;
            }

            // build ElasticSearch filter
            StringBuilder fullFilter = new StringBuilder();
            fullFilter.Append("(or ");
            foreach (DbAnnouncement announcement in dbAnnouncements)
            {
                //Elastic search should receive phrase + start date.
                fullFilter.Append(GetAnnouncementFilter(announcement, userNotificationData.Announcements.FirstOrDefault(userAnn => userAnn.AnnouncementId == announcement.ID).AddedDateSec));
            }
            fullFilter.Append(" )");

            // build unified search request
            UnifiedSearchRequest request = new UnifiedSearchRequest()
            {
                m_sSignature = NotificationUtils.GetSignature(CatalogSignString, CatalogSignatureKey),
                m_sSignString = CatalogSignString,
                filterQuery = fullFilter.ToString(),
                m_nGroupID = groupId,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize,
                m_oFilter = new Filter()
                {
                    m_bOnlyActiveMedia = true
                },
                shouldDateSearchesApplyToAllTypes = true,
                order = orderObj
            };

            // perform the search
            UnifiedSearchResponse unifiedSearchResponse = NotificationUtils.GetUnifiedSearchResponse(request);

            if (unifiedSearchResponse == null || unifiedSearchResponse.searchResults == null)
            {
                log.ErrorFormat("elastic search did not find any results. GID: {0}, user ID: {1}, search object: {2}", groupId, userId, JsonConvert.SerializeObject(request));
                response.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                return response;
            }

            if (unifiedSearchResponse.searchResults.Count == 0)
            {
                log.DebugFormat("elastic search did not find any results. GID: {0}, user ID: {1}, search Query: {2}", groupId, userId, fullFilter.ToString());
                response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                return response;
            }

            mediaIds = new List<int>();
            mediaIds.AddRange(unifiedSearchResponse.searchResults.Select((item => int.Parse(item.AssetId))));
            response.Ids = mediaIds;
            response.TotalCount = unifiedSearchResponse.m_nTotalItems;

            return response;
        }

        public static string GetEpisodeAssociationTag(int groupId, ref CatalogGroupCache cache, ref AssetStruct episodeAssetStruct, long? seriesMediaTypeId = null)
        {
            string associationTag = string.Empty;
            if (CatalogManager.Instance.DoesGroupUsesTemplates(groupId))
            {
                if (cache == null)
                {
                    if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out cache))
                    {
                        log.Error($"failed to get catalogGroupCache for groupId: {groupId} when calling GetEpisodeAssociationTag");
                        return associationTag;
                    }
                }

                if (episodeAssetStruct == null)
                {
                    if (!TryGetEpisodeAssetStructBySeries(cache, seriesMediaTypeId, out episodeAssetStruct))
                    {
                        log.Error($"failed to get GetEpisodeAssetStruct for groupId: {groupId}");
                        return associationTag;
                    }
                }

                associationTag = cache.TopicsMapById[episodeAssetStruct.ConnectingMetaId.Value].SystemName;
            }
            else
            {
                // validate association tag
                associationTag = GetEpisodeAssociationTagForNonOpc(groupId);
            }

            if (string.IsNullOrEmpty(associationTag))
            {
                log.ErrorFormat("Error getting episode association tag - Association tag wasn't found. groupId: {0}", groupId);
            }

            return associationTag;
        }

        private static string GetEpisodeAssociationTagForNonOpc(int groupId)
        {
            string associationTag = NotificationCache.Instance().GetEpisodeAssociationTagName(groupId);
            return associationTag;
        }

        public static List<string> GetSeriesNames(ApiObjects.MetaType topicType, string episodeConnectedTag, List<ApiObjects.Catalog.Tags> tags, List<ApiObjects.Catalog.Metas> metas = null)
        {
            List<string> seriesNames = new List<string>();
            if (topicType == ApiObjects.MetaType.Tag)
            {
                if (tags != null && tags.Count > 0)
                {
                    var tag = tags.FirstOrDefault(x => x.m_oTagMeta != null && x.m_oTagMeta.m_sName.ToLower().Trim() == episodeConnectedTag.ToLower().Trim());
                    if (tag != null && tag.m_lValues != null)
                    {
                        seriesNames.AddRange(tag.m_lValues);
                    }
                }
            }
            else
            {
                if (metas != null && metas.Count > 0)
                {
                    var meta = metas.FirstOrDefault(x => x.m_oTagMeta != null && x.m_oTagMeta.m_sName.ToLower().Trim() == episodeConnectedTag.ToLower().Trim());
                    if (meta != null && !string.IsNullOrEmpty(meta.m_sValue))
                    {
                        seriesNames.Add(meta.m_sValue);
                    }
                }
            }

            // 'valueForM'
            return seriesNames;
        }

        private static string GetFollowPhrase(int groupId, string seriesName, long? seriesMediaTypeId = null)
        {
            CatalogGroupCache cache = null;
            AssetStruct episodeAssetStruct = null;

            // validate association tag
            string episodeAssociationTag = GetEpisodeAssociationTag(groupId, ref cache, ref episodeAssetStruct, seriesMediaTypeId);
            if (string.IsNullOrEmpty(episodeAssociationTag))
            {
                log.ErrorFormat("Error getting follow series phrase - Association tag wasn't found. groupId: {0}, title: {1}", groupId, seriesName);
                return null;
            }

            //SeriesId='valueForMedia'
            return string.Format(FOLLOW_PHRASE_FORMAT, episodeAssociationTag, seriesName);
        }

        private static string GetFollowPhrase(int groupId, FollowDataBase followData)
        {
            var followDataTvSeries = followData as FollowDataTvSeries;

            if (followDataTvSeries == null || !CatalogManager.Instance.DoesGroupUsesTemplates(groupId))
            {
                return GetFollowPhrase(groupId, followData.Title);
            }

            CatalogGroupCache cache = null;
            if (!TryGetSeriesAssetStructBySeriesMediaId(groupId, ref cache, followDataTvSeries.AssetId,
                    out var seriesAssetStruct))
            {
                return null;
            }

            return GetFollowPhrase(groupId, followData.Title, seriesAssetStruct.Id);
        }

        private static Status TryGetSeriesNameAndAssetStructId(int groupId, int assetId, out string seriesName, out long seriesAssetStructId)
        {
            var statusResult = new Status((int) eResponseStatus.Error, eResponseStatus.Error.ToString());
            seriesName = string.Empty;
            seriesAssetStructId = 0;

            if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out var cache))
            {
                log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling FollowManager.TryGetSeriesNameAndAssetStructId", groupId);
                return statusResult;
            }

            var assetResponse = AssetManager.Instance.GetAsset(groupId, assetId, eAssetTypes.MEDIA, false);
            if (!assetResponse.HasObject())
            {
                return assetResponse.Status;
            }

            var seriesMediaAsset = assetResponse.Object as MediaAsset;
            if (seriesMediaAsset == null || !TryGetEpisodeAssetStructBySeries(cache, seriesMediaAsset.MediaType.m_nTypeID, out var episodeAssetStruct))
            {
                statusResult.Set(eResponseStatus.InvalidAssetId, "invalid asset");
            }
            else
            {
                var topic = cache.TopicsMapById[episodeAssetStruct.ConnectedParentMetaId.Value];
                seriesName = GetSeriesNames(topic.Type, topic.SystemName, seriesMediaAsset.Tags, seriesMediaAsset.Metas).FirstOrDefault();
                seriesAssetStructId = seriesMediaAsset.MediaType.m_nTypeID;
                if (string.IsNullOrEmpty(seriesName))
                {
                    statusResult.Set(eResponseStatus.InvalidAssetId, "media is not a series episode (series name is empty)");
                }
                else
                {
                    statusResult.Set(eResponseStatus.OK);
                }
            }

            return statusResult;
        }

        #endregion

        #region CRUD methods

        public GenericResponse<FollowDataTvSeries> Add(ContextData contextData, FollowDataTvSeries followDataTvToAdd)
        {
            var response = new GenericResponse<FollowDataTvSeries>();

            try
            {
                if (!contextData.UserId.HasValue || contextData.UserId.Value == 0)
                {
                    response.SetStatus(eResponseStatus.InvalidUser);
                    return response;
                }

                var seriesNameResult = TryGetSeriesNameAndAssetStructId(contextData.GroupId, followDataTvToAdd.AssetId,
                    out var seriesName, out var seriesAssetStructId);
                if (!seriesNameResult.IsOkStatusCode())
                {
                    response.SetStatus(seriesNameResult.Code);
                }

                followDataTvToAdd.Title = seriesName;
                followDataTvToAdd.GroupId = contextData.GroupId;
                followDataTvToAdd.FollowPhrase = GetFollowPhrase(contextData.GroupId, followDataTvToAdd.Title, seriesAssetStructId);

                response = AddFollowItemToUser((int) contextData.UserId.Value, followDataTvToAdd);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("An Exception was occurred while Adding FollowDataTvSeries. contextData:{0}, FollowDataTvSeries.AssetId:{1}. ex: {2}",
                    contextData.ToString(), followDataTvToAdd.AssetId, ex);
            }

            return response;
        }

        public Status Delete(ContextData contextData, int id)
        {
            throw new NotImplementedException();
        }

        public GenericResponse<FollowDataTvSeries> Get(ContextData contextData, int id)
        {
            throw new NotImplementedException();
        }

        public GenericListResponse<FollowDataTvSeries> List(ContextData contextData, FollowTvSeriesFilter filter)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private Methods

        public static MediaObj GetMediaObj(int groupId, int mediaId, string userId = null)
        {
            // send get media to catalog
            var request = new MediasProtocolRequest()
            {
                m_sSignature = NotificationUtils.GetSignature(CatalogSignString, CatalogSignatureKey),
                m_sSignString = CatalogSignString,
                m_lMediasIds = new List<int> { mediaId },
                m_nGroupID = groupId,
                m_oFilter = new Filter(),
                m_sSiteGuid = userId
            };

            MediaResponse mediaResponse = null;
            try
            {
                mediaResponse = request.GetMediasByIDs(request);
            }
            catch (Exception ex)
            {
                log.Error("GetSeriesMediaObj: error when calling catalog: ", ex);
                return null;
            }

            if (mediaResponse != null && mediaResponse.m_lObj != null)
            {
                var mediaObj = mediaResponse.m_lObj.FirstOrDefault() as MediaObj;
                return mediaObj;
            }

            return null;
        }

        private static bool SendMessageAnnouncement(DbAnnouncement announcement, MessageAnnouncement message, long mediaId, int groupId)
        {
            var shouldSendMessageAnnouncement = true;
            var previousAnnouncementMessages = NotificationDal.Get_MessageAnnouncementByAnnouncementAndReference(announcement.ID, mediaId.ToString());
            if (previousAnnouncementMessages != null)
            {
                // check for each message (should be a single message) if not sent and needs to update
                foreach (var messageAnnouncement in previousAnnouncementMessages)
                {
                    if (messageAnnouncement.Status == eAnnouncementStatus.NotSent)
                    {
                        if (messageAnnouncement.StartTime == message.StartTime)
                        {
                            // message not changed.
                            log.DebugFormat("found previous message announcement that wasn't sent with the same start time. canceling follow notification. group {0}, previous message: {1}",
                                            groupId, JsonConvert.SerializeObject(messageAnnouncement));
                            shouldSendMessageAnnouncement = false;
                            break;
                        }
                        else
                        {
                            // update message & start time
                            log.DebugFormat("found previous message announcement that wasn't sent with the different start time. updating old message and canceling new one. group {0}, previous message: {1}, new message: {2}, new start time: {3}",
                                            groupId, JsonConvert.SerializeObject(messageAnnouncement), message.Message, message.StartTime);

                            // updating message
                            messageAnnouncement.StartTime = message.StartTime;
                            messageAnnouncement.Message = message.Message;
                            if (AnnouncementManager.Instance.UpdateMessageAnnouncement(groupId,
                                messageAnnouncement.MessageAnnouncementId, messageAnnouncement, true, false).Status.IsOkStatusCode())
                            {
                                log.DebugFormat("successfully updated previous message announcement. group ID: {0}, previous message: {1}, new message: {2}, new start time: {3}",
                                                groupId, JsonConvert.SerializeObject(messageAnnouncement), message.Message, message.StartTime);

                                // update announcement message sent date
                                if (!NotificationDal.UpdateAnnouncement(groupId, messageAnnouncement.AnnouncementId, announcement.AutomaticIssueFollowNotification, DateTime.UtcNow))
                                    log.ErrorFormat("Error while trying to update last announcement message sent date. GID: {0}, message: {1}", groupId, JsonConvert.SerializeObject(messageAnnouncement));

                                shouldSendMessageAnnouncement = false;
                                break;
                            }
                            else
                            {
                                log.ErrorFormat("error while trying to update previous message announcement that wasn't sent with the different start time. group {0}, previous message: {1}, new message: {2}, new start time: {3}",
                                                groupId, JsonConvert.SerializeObject(messageAnnouncement), message.Message, message.StartTime);
                            }
                        }
                    }
                }
            }

            var messageSent = false;
            if (shouldSendMessageAnnouncement)
            {
                // sending message to queue
                log.DebugFormat("about to add message announcement for: group {0}, media: {1}, start date: {2}", groupId, mediaId, message.StartTime);
                var addMsgAnnResponse = AnnouncementManager.AddMessageAnnouncement(groupId, message, true, false);
                if (!addMsgAnnResponse.Status.IsOkStatusCode())
                {
                    log.ErrorFormat("add message announcement failed. group: {0}, asset type: {1}, error: {2}-{3}",
                                    groupId, eOTTAssetTypes.Series.ToString(), addMsgAnnResponse.Status.ToString());
                }
                else
                {
                    log.DebugFormat("successfully created new message announcement in queue. group ID: {0}, message: {1}", groupId, JsonConvert.SerializeObject(message));
                    // update announcement message sent date
                    if (!NotificationDal.UpdateAnnouncement(groupId, message.AnnouncementId, announcement.AutomaticIssueFollowNotification, DateTime.UtcNow))
                    {
                        log.ErrorFormat("Error while trying to update last announcement message sent date. GID: {0}, message: {1}", groupId, JsonConvert.SerializeObject(message));
                    }

                    if (message.IncludeUserInbox)
                        AnnouncementManager.Instance.InvalidateFollowMessageAnnouncement(groupId, message.AnnouncementId);

                    messageSent = true;
                }
            }

            return messageSent;
        }

        private static bool TryGetEpisodeAssetStructBySeries(CatalogGroupCache cache, long? seriesTypeId, out AssetStruct episodeAssetStruct)
        {
            episodeAssetStruct = null;

            if (!seriesTypeId.HasValue || seriesTypeId.Value == 0)
            {
                var seriesAssetStruct = cache.AssetStructsMapById.Values.FirstOrDefault(x => x.IsSeriesAssetStruct);
                if (seriesAssetStruct != null)
                {
                    seriesTypeId = seriesAssetStruct.Id;
                }
            }

            if (!seriesTypeId.HasValue || seriesTypeId.Value == 0 || !cache.AssetStructsMapById.ContainsKey(seriesTypeId.Value))
            {
                return false;
            }

            episodeAssetStruct = cache.AssetStructsMapById.Values.FirstOrDefault(x => x.ParentId.HasValue && x.ParentId.Value == seriesTypeId);

            if (episodeAssetStruct == null ||
                !episodeAssetStruct.ConnectedParentMetaId.HasValue ||
                !cache.TopicsMapById.ContainsKey(episodeAssetStruct.ConnectedParentMetaId.Value) ||
                !episodeAssetStruct.ConnectingMetaId.HasValue ||
                !cache.TopicsMapById.ContainsKey(episodeAssetStruct.ConnectingMetaId.Value))
            {
                return false;
            }

            return true;
        }

        private bool TryGetEpisodeAssetStructByEpisode(MediaAsset episodeMediaAsset, CatalogGroupCache cache, out AssetStruct episodeAssetStruct)
        {
            episodeAssetStruct = null;

            if (episodeMediaAsset == null ||
                !cache.AssetStructsMapById.TryGetValue(episodeMediaAsset.MediaType.m_nTypeID, out episodeAssetStruct) ||
                !episodeAssetStruct.ParentId.HasValue ||
                !episodeAssetStruct.ConnectedParentMetaId.HasValue ||
                !cache.TopicsMapById.ContainsKey(episodeAssetStruct.ConnectedParentMetaId.Value) ||
                !episodeAssetStruct.ConnectingMetaId.HasValue ||
                !cache.TopicsMapById.ContainsKey(episodeAssetStruct.ConnectingMetaId.Value) ||
                !cache.AssetStructsMapById.ContainsKey(episodeAssetStruct.ParentId.Value))
            {
                return false;
            }

            return true;
        }

        private static bool TryGetSeriesAssetStructBySeriesMediaId(int groupId, ref CatalogGroupCache cache, long seriesMediaId, out AssetStruct seriesAssetStruct)
        {
            seriesAssetStruct = null;

            if (!TryGetAssetStructByMediaId(groupId, ref cache, seriesMediaId, out seriesAssetStruct))
            {
                log.Error($"failed to get assetStruct for seriesMediaId: {seriesMediaId} when calling {nameof(TryGetSeriesAssetStructBySeriesMediaId)}");
                return false;
            }

            var seriesAssetStructId = seriesAssetStruct.Id;
            if (!cache.AssetStructsMapById.Values.Any(x => x.ParentId == seriesAssetStructId))
            {
                log.Error($"assetStruct {seriesAssetStructId} has no children. It is not series.");
                return false;
            }

            return true;
        }

        private static bool TryGetAssetStructByMediaId(int groupId, ref CatalogGroupCache cache, long mediaId, out AssetStruct assetStruct)
        {
            assetStruct = null;

            var media = GetMediaObj(groupId, (int) mediaId);
            if (media == null)
            {
                log.Error($"failed to get media by ID: {mediaId} when calling {nameof(TryGetAssetStructByMediaId)}");
                return false;
            }

            if (cache == null && !CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out cache))
            {
                log.Error($"failed to get catalogGroupCache for groupId: {groupId} when calling {nameof(TryGetAssetStructByMediaId)}");
                return false;
            }

            if (!cache.AssetStructsMapById.TryGetValue(media.m_oMediaType.m_nTypeID, out assetStruct))
            {
                log.Error($"failed to get assetStruct by ID: {media.m_oMediaType.m_nTypeID} when calling {nameof(TryGetAssetStructByMediaId)}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Add/Update group's Message template
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="messageTemplate"></param>
        /// <returns></returns>
        private static MessageTemplate SetMessageTemplateAtDB(int groupId, MessageTemplate messageTemplate)
        {
            return NotificationDal.SetMessageTemplate(groupId, messageTemplate);
        }

        private static Status ValidateMessageTemplate(int groupId, MessageTemplate messageTemplate)
        {
            Status validationStatus = new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };

            // ValidateMessage -
            // 1. check if eFollowSeriesPlaceHolders
            // 2. replace placeHolder case Sensitive   (formattedMessage)
            string formattedMessage = messageTemplate.Message;
            validationStatus = ValidateMessage(ref formattedMessage, messageTemplate.TemplateType);

            if (validationStatus.Code != (int)eResponseStatus.OK)
            {
                log.ErrorFormat("ValidateMessageTemplate: invalid message: {0}, GID: {1}", messageTemplate.Message, groupId);
                return validationStatus;
            }

            messageTemplate.Message = formattedMessage;

            // ValidateDatFormat
            validationStatus = ValidateDateFormat(messageTemplate.DateFormat);
            if (validationStatus.Code != (int)eResponseStatus.OK)
            {
                log.ErrorFormat("ValidateMessageTemplate: invalid date format: {0}, GID: {1}", messageTemplate.DateFormat, groupId);
                return validationStatus;
            }

            // ValidateMessage -
            // 1. check if eFollowSeriesPlaceHolders
            // 2. replace placeHolder case Sensitive   (formattedMessage)
            string formattedUrl = messageTemplate.URL;
            validationStatus = ValidateUrl(ref formattedUrl, messageTemplate.TemplateType);

            if (validationStatus.Code != (int)eResponseStatus.OK)
            {
                log.ErrorFormat("ValidateMessageTemplate: invalid url: {0}, GID: {1}", messageTemplate.URL, groupId);
                return validationStatus;
            }

            messageTemplate.URL = formattedUrl;

            return validationStatus;
        }

        private static Status ValidateDateFormat(string dateTimeFormat)
        {
            Status validationStatus = new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };

            DateTime tmpDateTime; // only for date format check
            bool isValid = DateTime.TryParseExact(DateTime.UtcNow.ToString(dateTimeFormat), dateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out tmpDateTime);
            if (!isValid)
            {
                log.ErrorFormat("ValidateDateFormat - Wrong DateTime format {0}", dateTimeFormat);
                validationStatus.Code = (int)eResponseStatus.DatetimeFormatIsInvalid;
                validationStatus.Message = DATETIME_FORMAT_IS_INVALID;
            }

            return validationStatus;
        }

        private static Status ValidateMessage(ref string message, MessageTemplateType assetTypes)
        {
            Status validationStatus = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };

            switch (assetTypes)
            {
                case ApiObjects.MessageTemplateType.Series:
                    validationStatus = ValidatePlaceholders<eFollowSeriesPlaceHolders>(ref message);
                    break;
                case ApiObjects.MessageTemplateType.Reminder:
                    validationStatus = ValidatePlaceholders<eReminderPlaceHolders>(ref message);
                    break;
                case ApiObjects.MessageTemplateType.Churn:
                    validationStatus = ValidatePlaceholders<eChurnPlaceHolders>(ref message);
                    break;
                case ApiObjects.MessageTemplateType.SeriesReminder:
                    validationStatus = ValidatePlaceholders<eSeriesReminderPlaceHolders>(ref message);
                    break;
                case ApiObjects.MessageTemplateType.InterestVod:
                    validationStatus = ValidatePlaceholders<eFollowSeriesPlaceHolders>(ref message);
                    break;
                case ApiObjects.MessageTemplateType.InterestEPG:
                    validationStatus = ValidatePlaceholders<eReminderPlaceHolders>(ref message);
                    break;
                default:
                    break;
            }

            if (validationStatus.Code != (int)eResponseStatus.OK)
            {
                validationStatus.Code = (int)eResponseStatus.MessagePlaceholdersInvalid; ;
                validationStatus.Message = MESSAGE_PLACEHOLDERS_ARE_INVALID;
            }
            return validationStatus;
        }

        private static Status ValidateUrl(ref string url, MessageTemplateType assetTypes)
        {
            Status validationStatus = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };

            switch (assetTypes)
            {
                case ApiObjects.MessageTemplateType.Series:
                case ApiObjects.MessageTemplateType.InterestVod:
                    validationStatus = ValidatePlaceholders<eFollowSeriesPlaceHolders>(ref url);
                    break;
                case ApiObjects.MessageTemplateType.Reminder:
                case ApiObjects.MessageTemplateType.InterestEPG:
                    validationStatus = ValidatePlaceholders<eReminderPlaceHolders>(ref url);
                    break;
                case ApiObjects.MessageTemplateType.Churn:
                    validationStatus = ValidatePlaceholders<eChurnPlaceHolders>(ref url);
                    break;
                case ApiObjects.MessageTemplateType.SeriesReminder:
                    validationStatus = ValidatePlaceholders<eSeriesReminderPlaceHolders>(ref url);
                    break;
                default:
                    break;
            }

            if (validationStatus.Code != (int)eResponseStatus.OK)
            {
                validationStatus.Code = (int)eResponseStatus.URLPlaceholdersInvalid; ;
                validationStatus.Message = URL_PLACEHOLDERS_ARE_INVALID;
            }
            return validationStatus;
        }

        private static void HandleUnfollowPush(int groupId, long userId, UserNotification userNotificationData, long announcementId)
        {
            if (userNotificationData.devices == null || userNotificationData.devices.Count == 0)
                log.DebugFormat("User doesn't have any devices. PID: {0}, UID: {1}", groupId, userId);
            else
            {
                bool docExists = false;
                foreach (UserDevice device in userNotificationData.devices)
                {
                    string udid = device.Udid;
                    if (string.IsNullOrEmpty(udid))
                    {
                        log.ErrorFormat("device UDID invalid: UDID: {0} PID: {1}, UID: {2}", device.Udid, groupId, userId);
                        continue;
                    }

                    // get device data
                    DeviceNotificationData deviceNotificationData = DAL.NotificationDal.GetDeviceNotificationData(groupId, udid, ref docExists);
                    if (deviceNotificationData == null)
                    {
                        log.ErrorFormat("device notification data not found group: {0}, UID: {1}, UDID: {2}", groupId, userId, device.Udid);
                        continue;
                    }

                    // get device subscription
                    var subscribedAnnouncements = deviceNotificationData.SubscribedAnnouncements.Where(x => x.Id == announcementId);
                    if (subscribedAnnouncements == null || subscribedAnnouncements.Count() == 0)
                    {
                        log.ErrorFormat("device notification data had no subscription to announcement. group: {0}, UID: {1}, UDID: {2}", groupId, userId, device.Udid);
                        continue;
                    }

                    // unsubscribe device
                    List<UnSubscribe> unsubscibeList = new List<UnSubscribe>()
                    {
                        new UnSubscribe()
                        {
                            SubscriptionArn = subscribedAnnouncements.First().ExternalId
                        }
                    };

                    unsubscibeList = NotificationAdapter.UnSubscribeToAnnouncement(groupId, unsubscibeList);
                    if (unsubscibeList == null ||
                        unsubscibeList.Count == 0 ||
                        !unsubscibeList.First().Success
                        || !deviceNotificationData.SubscribedAnnouncements.Remove(subscribedAnnouncements.First())
                        || !DAL.NotificationDal.SetDeviceNotificationData(groupId, udid, deviceNotificationData))
                    {
                        log.ErrorFormat("error removing announcement from device subscribed. group: {0}, UID: {1}, UDID: {2}", groupId, userId, device.Udid);
                        continue;
                    }
                    else
                        log.DebugFormat("Successfully unsubscribed device from announcement group: {0}, UID: {1}, UDID: {2}", groupId, userId, device.Udid);
                }
            }
        }

        private static void HandleUnfollowSms(int groupId, long userId, long announcementId)
        {
            SmsNotificationData smsNotificationData = NotificationDal.GetUserSmsNotificationData(groupId, userId);
            if (smsNotificationData != null)
            {
                var subscribedAnnouncements = smsNotificationData.SubscribedAnnouncements.Where(x => x.Id == announcementId);
                if (subscribedAnnouncements == null || subscribedAnnouncements.Count() == 0)
                {
                    log.DebugFormat("sms notification data had no subscription to announcement. group: {0}, userId: {1}", groupId, userId);
                    return;
                }

                // unsubscribe sms
                List<UnSubscribe> unsubscibeList = new List<UnSubscribe>()
                    {
                        new UnSubscribe()
                        {
                            SubscriptionArn = subscribedAnnouncements.First().ExternalId
                        }
                    };

                unsubscibeList = NotificationAdapter.UnSubscribeToAnnouncement(groupId, unsubscibeList);
                if (unsubscibeList == null ||
                    unsubscibeList.Count == 0 ||
                    !unsubscibeList.First().Success
                    || !smsNotificationData.SubscribedAnnouncements.Remove(subscribedAnnouncements.First())
                    || !DAL.NotificationDal.SetUserSmsNotificationData(groupId, userId, smsNotificationData))
                {
                    log.ErrorFormat("error removing announcement from sms subscribed. group: {0}, userId: {1}", groupId, userId);
                }
                else
                    log.DebugFormat("Successfully unsubscribed device from announcement group: {0}, userId: {1}", groupId, userId);
            }
        }

        private static GenericResponse<T> AddFollowItemToUser<T>(int userId, T followItem) where T : FollowDataBase, new()
        {
            var response = new GenericResponse<T>();

            // get user notifications
            UserNotification userNotificationData = null;
            var getUserNotificationDataStatus = Utils.GetUserNotificationData(followItem.GroupId, userId, out userNotificationData);
            if (getUserNotificationDataStatus.Code != (int)eResponseStatus.OK || userNotificationData == null)
            {
                response.SetStatus(getUserNotificationDataStatus);
                return response;
            }

            try
            {
                // get user announcements from DB
                DbAnnouncement announcementToFollow = null;
                List<DbAnnouncement> dbAnnouncements = null;

                if (NotificationCache.Instance().TryGetAnnouncements(followItem.GroupId, ref dbAnnouncements))
                    announcementToFollow = dbAnnouncements.FirstOrDefault(ann => ann.FollowPhrase == followItem.FollowPhrase);

                if (announcementToFollow == null)
                {
                    // follow announcement doesn't exists - first time the series is being followed - create a new one
                    var announcementToFollowResponse = CreateFollowAnnouncement(followItem);

                    if (announcementToFollowResponse.Status.Code != (int)eResponseStatus.OK)
                    {
                        log.ErrorFormat("user notification data not found group: {0}, user: {1}", followItem.GroupId, userId);
                        response.SetStatus((eResponseStatus)announcementToFollowResponse.Status.Code, announcementToFollowResponse.Status.Message);
                        return response;
                    }

                    announcementToFollow = announcementToFollowResponse.Object;
                }

                // validate existence of db follow announcement
                if (announcementToFollow == null)
                {
                    log.ErrorFormat("announcement not found group: {0}, user: {1}, phrase: {2}",
                                    followItem.GroupId, userId, followItem.FollowPhrase);
                    return response;
                }

                followItem.AnnouncementId = announcementToFollow.ID;
                if (userNotificationData.Announcements.Count(x => x.AnnouncementId == followItem.AnnouncementId) > 0)
                {
                    // user already follows the series
                    log.DebugFormat("User is already following announcement. PID: {0}, UID: {1}, Announcement ID: {2}",
                                    followItem.GroupId, userId, followItem.AnnouncementId);
                    response.SetStatus(eResponseStatus.UserAlreadyFollowing, "User already following");
                    return response;
                }

                // create added time
                long addedSecs = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);

                if (userNotificationData.Settings.EnableMail.HasValue &&
                    userNotificationData.Settings.EnableMail.Value &&
                    !string.IsNullOrEmpty(userNotificationData.UserData.Email))
                {
                    if (!MailNotificationAdapterClient.SubscribeToAnnouncement(followItem.GroupId,
                                                                               new List<string>() { announcementToFollow.MailExternalId },
                                                                               userNotificationData.UserData,
                                                                               userId))
                    {
                        log.ErrorFormat("Failed subscribing user to email announcement. group: {0}, userId: {1}, email: {2}",
                                        followItem.GroupId, userId, userNotificationData.UserData.Email);
                    }
                }

                HandleFollowSms(userId, followItem, userNotificationData, announcementToFollow, addedSecs);
                HandleFollowPush(userId, followItem, userNotificationData, announcementToFollow, addedSecs);

                // update user notification object
                userNotificationData.Announcements.Add(new Announcement()
                {
                    AnnouncementId = followItem.AnnouncementId,
                    AnnouncementName = announcementToFollow.Name,
                    AddedDateSec = addedSecs,
                });

                response.Object = new T()
                {
                    GroupId = followItem.GroupId,
                    FollowPhrase = announcementToFollow.FollowPhrase,
                    AnnouncementId = announcementToFollow.ID,
                    Status = 1,                         // only enabled status in this phase
                    Title = announcementToFollow.Name,
                    //Type = FollowType.TV_Series_VOD,  // only TV series in this phase
                    FollowReference = announcementToFollow.FollowReference,
                    Timestamp = addedSecs,
                };

                if (!DAL.NotificationDal.SetUserNotificationData(followItem.GroupId, userId, userNotificationData))
                    log.ErrorFormat("error setting user notification data. group: {0}, user id: {1}", followItem.GroupId, userId);
                else
                {
                    // update user following items
                    if (!NotificationDal.SetUserFollowNotificationData(followItem.GroupId, userId, (int)followItem.AnnouncementId))
                        log.ErrorFormat("Error updating the user following notification data. GID :{0}, user ID: {1}, Announcement ID: {2}", followItem.GroupId, userId, followItem.AnnouncementId);
                    else
                        log.DebugFormat("successfully set notification announcements inbox mapping. group: {0}, user id: {1}, Announcements ID: {2}", followItem.GroupId, userId, (int)followItem.AnnouncementId);

                    log.DebugFormat("successfully updated user notification data. group: {0}, user id: {1}", followItem.GroupId, userId);
                }

                response.SetStatus(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error("Error in AddFollowItemToUser", ex);
                response.SetStatus(eResponseStatus.Error);
            }

            return response;
        }

        private static Status RemoveFollowItemFromUser(int groupId, long userId, DbAnnouncement userDbAnnouncement)
        {
            Status statusResult = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            // get user notification data
            bool docExists = false;
            UserNotification userNotificationData = DAL.NotificationDal.GetUserNotificationData(groupId, userId, ref docExists);

            if (userNotificationData == null ||
                userNotificationData.Announcements == null ||
                userNotificationData.Announcements.Count(x => x.AnnouncementId == userDbAnnouncement.ID) == 0)
            {
                log.DebugFormat("user notification data wasn't found. GID: {0}, UID: {1}", groupId, userId);
                statusResult = new Status((int)eResponseStatus.UserNotFollowing, "user is not following asset");
                return statusResult;
            }

            if (!string.IsNullOrEmpty(userNotificationData.UserData.Email) &&
                userNotificationData.Settings.EnableMail.HasValue &&
                userNotificationData.Settings.EnableMail.Value)
            {
                MailNotificationAdapterClient.UnSubscribeToAnnouncement(groupId, new List<string>() { userDbAnnouncement.MailExternalId }, userNotificationData.UserData, userId);
            }

            HandleUnfollowSms(groupId, userId, userDbAnnouncement.ID);
            HandleUnfollowPush(groupId, userId, userNotificationData, userDbAnnouncement.ID);

            // remove announcement from user announcement list
            Announcement announcement = userNotificationData.Announcements.FirstOrDefault(x => x.AnnouncementId == userDbAnnouncement.ID);
            if (!userNotificationData.Announcements.Remove(announcement) ||
                !NotificationDal.RemoveUserFollowNotification(groupId, userId, announcement.AnnouncementId) ||
                !DAL.NotificationDal.SetUserNotificationData(groupId, userId, userNotificationData))
            {
                log.DebugFormat("an error while trying to remove announcement. GID: {0}, UID: {1}, announcementId: {2}", groupId, userId, userDbAnnouncement.ID);
                statusResult = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            else
            {
                log.DebugFormat("Successfully removed announcement from user announcements object group: {0}, UID: {1}", groupId, userId);
                statusResult = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return statusResult;
        }

        private static void HandleFollowPush(int userId, FollowDataBase followItem, UserNotification userNotificationData, DbAnnouncement announcementToFollow, long addedSecs)
        {
            if (userNotificationData.devices != null &&
                userNotificationData.devices.Count > 0 &&
                NotificationSettings.IsPartnerPushEnabled(followItem.GroupId) &&
                NotificationSettings.IsUserFollowPushEnabled(userNotificationData.Settings))
            {
                bool docExists = false;

                foreach (UserDevice device in userNotificationData.devices)
                {
                    string udid = device.Udid;
                    if (string.IsNullOrEmpty(udid))
                    {
                        log.Error("device UDID is empty: " + device.Udid);
                        continue;
                    }

                    log.DebugFormat("adding announcement to device group: {0}, user: {1}, UDID: {2}, announcementId: {3}", followItem.GroupId, userId, udid, followItem.AnnouncementId);

                    // get device notification data
                    docExists = false;
                    DeviceNotificationData deviceNotificationData = DAL.NotificationDal.GetDeviceNotificationData(followItem.GroupId, udid, ref docExists);
                    if (deviceNotificationData == null)
                    {
                        log.ErrorFormat("device notification data not found group: {0}, UDID: {1}", followItem.GroupId, device.Udid);
                        continue;
                    }

                    try
                    {
                        // validate device doesn't already have the announcement
                        var isSubscribedAnnouncements = deviceNotificationData.SubscribedAnnouncements.Count(x => x.Id == followItem.AnnouncementId) > 0;
                        if (isSubscribedAnnouncements)
                        {
                            log.ErrorFormat("user already following announcement on device. group: {0}, UDID: {1}", followItem.GroupId, device.Udid);
                            continue;
                        }

                        // get push data
                        PushData pushData = PushAnnouncementsHelper.GetPushData(followItem.GroupId, udid, string.Empty);
                        if (pushData == null)
                        {
                            log.ErrorFormat("push data not found. group: {0}, UDID: {1}", followItem.GroupId, device.Udid);
                            continue;
                        }

                        // subscribe device to announcement
                        AnnouncementSubscriptionData subData = new AnnouncementSubscriptionData()
                        {
                            EndPointArn = pushData.ExternalToken, // take from pushdata (with UDID)
                            Protocol = EnumseDeliveryProtocol.application,
                            TopicArn = announcementToFollow.ExternalId,
                            ExternalId = announcementToFollow.ID
                        };

                        List<AnnouncementSubscriptionData> subs = new List<AnnouncementSubscriptionData>() { subData };
                        subs = NotificationAdapter.SubscribeToAnnouncement(followItem.GroupId, subs);
                        if (subs == null || subs.Count == 0 || string.IsNullOrEmpty(subs.First().SubscriptionArnResult))
                        {
                            log.ErrorFormat("Error registering device to announcement. group: {0}, UDID: {1}", followItem.GroupId, device.Udid);
                            continue;
                        }

                        // update device notification object
                        NotificationSubscription sub = new NotificationSubscription()
                        {
                            ExternalId = subs.First().SubscriptionArnResult,
                            Id = followItem.AnnouncementId,
                            SubscribedAtSec = addedSecs
                        };
                        deviceNotificationData.SubscribedAnnouncements.Add(sub);

                        if (!DAL.NotificationDal.SetDeviceNotificationData(followItem.GroupId, udid, deviceNotificationData))
                            log.ErrorFormat("error setting device notification data. group: {0}, UDID: {1}, topic: {2}", followItem.GroupId, device.Udid, subData.EndPointArn);
                        else
                        {
                            log.DebugFormat("Successfully registered device to announcement. group: {0}, UDID: {1}, topic: {2}", followItem.GroupId, device.Udid, subData.EndPointArn);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Error in follow for push", ex);
                    }
                }
            }
        }

        private static void HandleFollowSms(int userId, FollowDataBase followItem, UserNotification userNotificationData, DbAnnouncement announcementToFollow, long addedSecs)
        {
            if (NotificationSettings.IsPartnerSmsNotificationEnabled(followItem.GroupId) &&
                userNotificationData.Settings.EnableSms.HasValue &&
                userNotificationData.Settings.EnableSms.Value &&
                !string.IsNullOrEmpty(userNotificationData.UserData.PhoneNumber))
            {
                try
                {
                    SmsNotificationData userSmsNotificationData = DAL.NotificationDal.GetUserSmsNotificationData(followItem.GroupId, userNotificationData.UserId);
                    if (userSmsNotificationData == null)
                    {
                        log.DebugFormat("user sms notification data is empty {0}", userId);
                        return;
                    }

                    var subscribedAnnouncements = userSmsNotificationData.SubscribedAnnouncements.Where(x => x.Id == followItem.AnnouncementId);
                    if (subscribedAnnouncements != null && subscribedAnnouncements.Count() > 0)
                    {
                        log.ErrorFormat("user already following announcement on sms. userId: {0}", userId);
                        return;
                    }

                    AnnouncementSubscriptionData subData = new AnnouncementSubscriptionData()
                    {
                        EndPointArn = userNotificationData.UserData.PhoneNumber,
                        Protocol = EnumseDeliveryProtocol.sms,
                        TopicArn = announcementToFollow.ExternalId,
                        ExternalId = announcementToFollow.ID
                    };

                    List<AnnouncementSubscriptionData> subs = new List<AnnouncementSubscriptionData>() { subData };
                    subs = NotificationAdapter.SubscribeToAnnouncement(followItem.GroupId, subs);
                    if (subs == null || subs.Count == 0 || string.IsNullOrEmpty(subs.First().SubscriptionArnResult))
                    {
                        log.ErrorFormat("Error registering sms to announcement. userId: {0}, PhoneNumber: {1}", userId, userNotificationData.UserData.PhoneNumber);
                        return;
                    }

                    // update notification object
                    NotificationSubscription sub = new NotificationSubscription()
                    {
                        ExternalId = subs.First().SubscriptionArnResult,
                        Id = followItem.AnnouncementId,
                        SubscribedAtSec = addedSecs
                    };
                    userSmsNotificationData.SubscribedAnnouncements.Add(sub);

                    if (!DAL.NotificationDal.SetUserSmsNotificationData(followItem.GroupId, userId, userSmsNotificationData))
                    {
                        log.ErrorFormat("error setting sms notification data. group: {0}, userId: {1}, topic: {2}", followItem.GroupId, userId, subData.EndPointArn);
                    }
                    else
                    {
                        log.DebugFormat("Successfully registered device to announcement. group: {0}, userId: {1}, topic: {2}", followItem.GroupId, userId, subData.EndPointArn);
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error in follow for sms", ex);
                }
            }
        }

        private static List<int> Get_FollowedAssets(int groupId, List<FollowDataBase> follows)
        {
            List<int> ret = new List<int>();

            foreach (var follow in follows)
            {
                // if tv series take series asset id from follow reference field
                CatalogGroupCache cache = null;
                AssetStruct assetStruct = null;

                long? seriesMediaTypeId = null;
                var followDataTvSeries = follow as FollowDataTvSeries;
                if (followDataTvSeries != null && TryGetSeriesAssetStructBySeriesMediaId(groupId, ref cache, followDataTvSeries.AssetId,
                        out var seriesAssetStruct))
                {
                    seriesMediaTypeId = seriesAssetStruct.Id;
                }

                if (follow.FollowPhrase.ToLower().Trim().StartsWith(GetEpisodeAssociationTag(groupId, ref cache, ref assetStruct, seriesMediaTypeId).ToLower().Trim()))
                {
                    if (int.TryParse(follow.FollowReference, out int id))
                    {
                        ret.Add(id);
                    }
                }
            }

            return ret;
        }

        private static string GetAnnouncementFilter(DbAnnouncement announcement, long addedDateSec)
        {
            long startDate = GetStartDate(addedDateSec);
            string filter = string.Format(" (and {0} start_date >= '{1}') ", announcement.FollowPhrase.ToLower(), startDate);
            return filter;
        }

        private static long GetStartDate(long addedDateSec)
        {
            long startDate = 0;
            long nowMinusTtl = GetPersonalizedFeedTtlDaysInSec();
            //Start date is determined as followed:
            //SF - this is the date the user started to follow the series.
            //SF < Now- TTL -> phrase + SF
            //SF > Now - TTL ->phrase + (Now - TTL)
            startDate = (addedDateSec) > nowMinusTtl ? addedDateSec : nowMinusTtl;
            return startDate;

        }

        private static long GetPersonalizedFeedTtlDaysInSec()
        {
            int personalizedFeedTtlDay = ApplicationConfiguration.Current.PersonalizedFeedTTLDays.Value;

            if (personalizedFeedTtlDay <= 0)
            {
                throw new Exception("TCM value [PersonalizedFeedTTLDays] isn't valid");
            }

            return TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow.AddDays(-personalizedFeedTtlDay));
        }

        #endregion
    }
}