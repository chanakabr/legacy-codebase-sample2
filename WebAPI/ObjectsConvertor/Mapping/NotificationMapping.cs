using AutoMapper;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.General;
using WebAPI.Models.Notification;
using WebAPI.Models.Notifications;
using WebAPI.Notifications;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class NotificationMapping
    {
        public static void RegisterMappings()
        {
            //NotificationPartnerSettings to KalturaPartnerNotificationSettings
            Mapper.CreateMap<NotificationPartnerSettings, KalturaPartnerNotificationSettings>()
                 .ForMember(dest => dest.PushNotificationEnabled, opt => opt.MapFrom(src => src.IsPushNotificationEnabled))
                 .ForMember(dest => dest.PushSystemAnnouncementsEnabled, opt => opt.MapFrom(src => src.IsPushSystemAnnouncementsEnabled))
                 .ForMember(dest => dest.PushStartHour, opt => opt.MapFrom(src => src.PushStartHour))
                 .ForMember(dest => dest.PushEndHour, opt => opt.MapFrom(src => src.PushEndHour))
                 .ForMember(dest => dest.InboxEnabled, opt => opt.MapFrom(src => src.IsInboxEnabled))
                 .ForMember(dest => dest.MessageTTLDays, opt => opt.MapFrom(src => src.MessageTTLDays))
                 .ForMember(dest => dest.AutomaticIssueFollowNotification, opt => opt.MapFrom(src => src.AutomaticIssueFollowNotifications))
                 .ForMember(dest => dest.TopicExpirationDurationDays, opt => opt.MapFrom(src => src.TopicExpirationDurationDays))
                 ;

            //KalturaPartnerNotificationSettings TO NotificationPartnerSettings
            Mapper.CreateMap<KalturaPartnerNotificationSettings, NotificationPartnerSettings>()
                 .ForMember(dest => dest.IsPushNotificationEnabled, opt => opt.MapFrom(src => src.PushNotificationEnabled))
                 .ForMember(dest => dest.IsPushSystemAnnouncementsEnabled, opt => opt.MapFrom(src => src.PushSystemAnnouncementsEnabled))
                 .ForMember(dest => dest.PushStartHour, opt => opt.MapFrom(src => src.PushStartHour))
                 .ForMember(dest => dest.PushEndHour, opt => opt.MapFrom(src => src.PushEndHour))
                 .ForMember(dest => dest.IsInboxEnabled, opt => opt.MapFrom(src => src.InboxEnabled))
                 .ForMember(dest => dest.MessageTTLDays, opt => opt.MapFrom(src => src.MessageTTLDays))
                 .ForMember(dest => dest.AutomaticIssueFollowNotifications, opt => opt.MapFrom(src => src.AutomaticIssueFollowNotification))
                 .ForMember(dest => dest.TopicExpirationDurationDays, opt => opt.MapFrom(src => src.TopicExpirationDurationDays))
                 ;

            //NotificationPartnerSettings to KalturaNotificationPartnerSettings
            Mapper.CreateMap<NotificationPartnerSettings, KalturaNotificationsPartnerSettings>()
                 .ForMember(dest => dest.PushNotificationEnabled, opt => opt.MapFrom(src => src.IsPushNotificationEnabled))
                 .ForMember(dest => dest.PushSystemAnnouncementsEnabled, opt => opt.MapFrom(src => src.IsPushSystemAnnouncementsEnabled))
                 .ForMember(dest => dest.PushStartHour, opt => opt.MapFrom(src => src.PushStartHour))
                 .ForMember(dest => dest.PushEndHour, opt => opt.MapFrom(src => src.PushEndHour))
                 .ForMember(dest => dest.InboxEnabled, opt => opt.MapFrom(src => src.IsInboxEnabled))
                 .ForMember(dest => dest.MessageTTLDays, opt => opt.MapFrom(src => src.MessageTTLDays))
                 .ForMember(dest => dest.AutomaticIssueFollowNotification, opt => opt.MapFrom(src => src.AutomaticIssueFollowNotifications))
                 .ForMember(dest => dest.TopicExpirationDurationDays, opt => opt.MapFrom(src => src.TopicExpirationDurationDays))
                 ;

            //KalturaNotificationPartnerSettings TO NotificationPartnerSettings
            Mapper.CreateMap<KalturaNotificationsPartnerSettings, NotificationPartnerSettings>()
                 .ForMember(dest => dest.IsPushNotificationEnabled, opt => opt.MapFrom(src => src.PushNotificationEnabled))
                 .ForMember(dest => dest.IsPushSystemAnnouncementsEnabled, opt => opt.MapFrom(src => src.PushSystemAnnouncementsEnabled))
                 .ForMember(dest => dest.PushStartHour, opt => opt.MapFrom(src => src.PushStartHour))
                 .ForMember(dest => dest.PushEndHour, opt => opt.MapFrom(src => src.PushEndHour))
                 .ForMember(dest => dest.IsInboxEnabled, opt => opt.MapFrom(src => src.InboxEnabled))
                 .ForMember(dest => dest.MessageTTLDays, opt => opt.MapFrom(src => src.MessageTTLDays))
                 .ForMember(dest => dest.AutomaticIssueFollowNotifications, opt => opt.MapFrom(src => src.AutomaticIssueFollowNotification))
                 .ForMember(dest => dest.TopicExpirationDurationDays, opt => opt.MapFrom(src => src.TopicExpirationDurationDays))
                 ;

            Mapper.CreateMap<UserNotificationSettings, KalturaNotificationSettings>()
                 .ForMember(dest => dest.PushNotificationEnabled, opt => opt.MapFrom(src => src.EnablePush))
                 .ForMember(dest => dest.PushFollowEnabled, opt => opt.MapFrom(src => src.FollowSettings.EnablePush));

            Mapper.CreateMap<bool?, UserFollowSettings>()
               .ForMember(dest => dest.EnablePush, opt => opt.MapFrom(src => src));

            Mapper.CreateMap<KalturaNotificationSettings, UserNotificationSettings>()
                 .ForMember(dest => dest.EnablePush, opt => opt.MapFrom(src => src.PushNotificationEnabled))
                 .ForMember(dest => dest.FollowSettings, opt => opt.MapFrom(src => src.PushFollowEnabled));

            Mapper.CreateMap<MessageAnnouncement, KalturaAnnouncement>()
                 .ForMember(dest => dest.Enabled, opt => opt.MapFrom(src => src.Enabled))
                 .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.MessageAnnouncementId))
                 .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                 .ForMember(dest => dest.Recipients, opt => opt.MapFrom(src => ConvertRecipientsType(src.Recipients)))
                 .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
                 .ForMember(dest => dest.Timezone, opt => opt.MapFrom(src => src.Timezone))
                 .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ConvertAnnouncementStatusType(src.Status)));

            //MessageTemplate to KalturaMessageTemplate
            Mapper.CreateMap<MessageTemplate, KalturaMessageTemplate>()
                 .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                 .ForMember(dest => dest.DateFormat, opt => opt.MapFrom(src => src.DateFormat))
                 .ForMember(dest => dest.Sound, opt => opt.MapFrom(src => src.Sound))
                 .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.Action))
                 .ForMember(dest => dest.URL, opt => opt.MapFrom(src => src.URL))
                 .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => ConvertOTTAssetType(src.AssetType)));

            //KalturaMessageTemplate TO MessageTemplate
            Mapper.CreateMap<KalturaMessageTemplate, MessageTemplate>()
                 .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                 .ForMember(dest => dest.DateFormat, opt => opt.MapFrom(src => src.DateFormat))
                 .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.Action))
                 .ForMember(dest => dest.Sound, opt => opt.MapFrom(src => src.Sound))
                 .ForMember(dest => dest.URL, opt => opt.MapFrom(src => src.URL))
                 .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => ConvertOTTAssetType(src.AssetType)));

            Mapper.CreateMap<FollowDataBase, KalturaFollowDataTvSeries>()
                 .ForMember(dest => dest.AnnouncementId, opt => opt.MapFrom(src => src.AnnouncementId))
                 .ForMember(dest => dest.FollowPhrase, opt => opt.MapFrom(src => src.FollowPhrase))
                 .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => ParseInt(src.FollowReference)))
                 .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                 .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp))
                 .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title));

            Mapper.CreateMap<KalturaFollowDataTvSeries, FollowDataBase>()
                 .ForMember(dest => dest.AnnouncementId, opt => opt.MapFrom(src => src.AnnouncementId))
                 .ForMember(dest => dest.FollowPhrase, opt => opt.MapFrom(src => src.FollowPhrase))
                 .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                 .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp))
                 .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title));

            Mapper.CreateMap<KalturaFollowDataTvSeries, FollowDataTvSeries>()
                 .ForMember(dest => dest.AnnouncementId, opt => opt.MapFrom(src => src.AnnouncementId))
                 .ForMember(dest => dest.FollowPhrase, opt => opt.MapFrom(src => src.FollowPhrase))
                 .ForMember(dest => dest.FollowReference, opt => opt.MapFrom(src => src.AssetId.ToString()))
                 .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                 .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp))
                 .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                 .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.AssetId))
                 .ForMember(dest => dest.Type, opt => opt.MapFrom(src => 0));

            Mapper.CreateMap<int, KalturaPersonalFollowFeed>()
               .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src));

            Mapper.CreateMap<int, KalturaPersonalFeed>()
               .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src));

            //InboxMessage to KalturaInboxMessage
            Mapper.CreateMap<InboxMessage, KalturaInboxMessage>()
                 .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAtSec))
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                 .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                 .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ConvertInboxMessageStatus(src.State)))
                 .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url))
                 .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ConvertInboxMessageType(src.Category)))
                 ;

            //KalturaInboxMessage TO InboxMessage
            Mapper.CreateMap<KalturaInboxMessage, InboxMessage>()
                 .ForMember(dest => dest.Category, opt => opt.MapFrom(src => ConvertInboxMessageType(src.Type)))
                 .ForMember(dest => dest.CreatedAtSec, opt => opt.MapFrom(src => src.CreatedAt))
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                 .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                 .ForMember(dest => dest.State, opt => opt.MapFrom(src => ConvertInboxMessageStatus(src.Status)))
                 .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url))
                 ;

            //DbAnnouncement to KalturaTopic
            Mapper.CreateMap<DbAnnouncement, KalturaTopic>()
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
                 .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                 .ForMember(dest => dest.SubscribersAmount, opt => opt.MapFrom(src => src.SubscribersAmount))
                 .ForMember(dest => dest.LastMessageSentDateSec, opt => opt.MapFrom(src => src.LastMessageSentDateSec))
                 .ForMember(dest => dest.AutomaticIssueNotification, opt => opt.MapFrom(src => ConvertAutomaticIssueNotification(src.AutomaticIssueFollowNotification)))
                 ;

            //KalturaTopic TO DbAnnouncement
            Mapper.CreateMap<KalturaTopic, DbAnnouncement>()
                 .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
                 .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                 .ForMember(dest => dest.SubscribersAmount, opt => opt.MapFrom(src => src.SubscribersAmount))
                 .ForMember(dest => dest.LastMessageSentDateSec, opt => opt.MapFrom(src => src.LastMessageSentDateSec))
                 .ForMember(dest => dest.AutomaticIssueFollowNotification, opt => opt.MapFrom(src => ConvertAutomaticIssueNotification(src.AutomaticIssueNotification)))
                 ;

            //RegistryParameter to KalturaPushWebParameters
            Mapper.CreateMap<RegistryParameter, KalturaRegistryParameter>()
                 .ForMember(dest => dest.AnnouncementId, opt => opt.MapFrom(src => src.AnnouncementId))
                 .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Key))
                 .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url))
                 ;
        }



        public static eMessageState ConvertInboxMessageStatus(KalturaInboxMessageStatus kalturaInboxMessageStatus)
        {
            eMessageState result;

            switch (kalturaInboxMessageStatus)
            {
                case KalturaInboxMessageStatus.Unread:
                    result = eMessageState.Unread;
                    break;
                case KalturaInboxMessageStatus.Read:
                    result = eMessageState.Read;
                    break;
                case KalturaInboxMessageStatus.Deleted:
                    result = eMessageState.Trashed;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown inbox message status");
            }

            return result;
        }

        private static KalturaInboxMessageStatus ConvertInboxMessageStatus(eMessageState eMessageState)
        {
            KalturaInboxMessageStatus result;

            switch (eMessageState)
            {
                case eMessageState.Unread:
                    result = KalturaInboxMessageStatus.Unread;
                    break;
                case eMessageState.Read:
                    result = KalturaInboxMessageStatus.Read;
                    break;
                case eMessageState.Trashed:
                    result = KalturaInboxMessageStatus.Deleted;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown inbox message status");
            }

            return result;
        }

        public static eMessageCategory ConvertInboxMessageType(KalturaInboxMessageType kalturaInboxMessageType)
        {
            eMessageCategory result;

            switch (kalturaInboxMessageType)
            {
                case KalturaInboxMessageType.SystemAnnouncement:
                    result = eMessageCategory.SystemAnnouncement;
                    break;
                case KalturaInboxMessageType.Followed:
                    result = eMessageCategory.Followed;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown inbox message type");
            }

            return result;
        }

        private static KalturaInboxMessageType ConvertInboxMessageType(eMessageCategory eMessageCategory)
        {
            KalturaInboxMessageType result;
            switch (eMessageCategory)
            {
                case eMessageCategory.SystemAnnouncement:
                    result = KalturaInboxMessageType.SystemAnnouncement;
                    break;
                case eMessageCategory.Followed:
                    result = KalturaInboxMessageType.Followed;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown inbox message type");
            }

            return result;
        }

        private static bool? GetFollowSettingsEnablePush(UserNotificationSettings userFollowSettings)
        {
            return userFollowSettings.FollowSettings.EnablePush;
        }

        public static KalturaAnnouncementRecipientsType ConvertRecipientsType(eAnnouncementRecipientsType recipients)
        {
            KalturaAnnouncementRecipientsType result;
            switch (recipients)
            {
                case eAnnouncementRecipientsType.All:
                    result = KalturaAnnouncementRecipientsType.All;
                    break;
                case eAnnouncementRecipientsType.Guests:
                    result = KalturaAnnouncementRecipientsType.Guests;
                    break;
                case eAnnouncementRecipientsType.LoggedIn:
                    result = KalturaAnnouncementRecipientsType.LoggedIn;
                    break;
                case eAnnouncementRecipientsType.Other:
                    result = KalturaAnnouncementRecipientsType.Other;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown recipients Type");
            }

            return result;
        }

        public static KalturaAnnouncementStatus ConvertAnnouncementStatusType(eAnnouncementStatus status)
        {
            KalturaAnnouncementStatus result;
            switch (status)
            {
                case eAnnouncementStatus.Aborted:
                    result = KalturaAnnouncementStatus.Aborted;
                    break;
                case eAnnouncementStatus.NotSent:
                    result = KalturaAnnouncementStatus.NotSent;
                    break;
                case eAnnouncementStatus.Sending:
                    result = KalturaAnnouncementStatus.Sending;
                    break;
                case eAnnouncementStatus.Sent:
                    result = KalturaAnnouncementStatus.Sent;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown status Type");
            }

            return result;
        }

        public static KalturaOTTAssetType ConvertOTTAssetType(eOTTAssetTypes assetType)
        {
            KalturaOTTAssetType result;

            switch (assetType)
            {
                case eOTTAssetTypes.Series:
                    result = KalturaOTTAssetType.Series;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown asset Type");
            }

            return result;
        }

        public static eOTTAssetTypes ConvertOTTAssetType(KalturaOTTAssetType assetType)
        {
            eOTTAssetTypes result;

            switch (assetType)
            {
                case KalturaOTTAssetType.Series:
                    result = eOTTAssetTypes.Series;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown asset Type");
            }

            return result;
        }

        private static int ParseInt(string input)
        {
            int ret = 0;
            int.TryParse(input, out ret);

            return ret;
        }

        internal static OrderObj ConvertOrderToOrderObj(Models.Catalog.KalturaOrder order)
        {
            OrderObj result = new OrderObj();

            switch (order)
            {
                case Models.Catalog.KalturaOrder.a_to_z:
                    result.m_eOrderBy = OrderBy.NAME;
                    result.m_eOrderDir = OrderDir.ASC;
                    break;
                case Models.Catalog.KalturaOrder.z_to_a:
                    result.m_eOrderBy = OrderBy.NAME;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case Models.Catalog.KalturaOrder.views:
                    result.m_eOrderBy = OrderBy.VIEWS;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case Models.Catalog.KalturaOrder.ratings:
                    result.m_eOrderBy = OrderBy.RATING;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case Models.Catalog.KalturaOrder.votes:
                    result.m_eOrderBy = OrderBy.VOTES_COUNT;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case Models.Catalog.KalturaOrder.newest:
                    result.m_eOrderBy = OrderBy.START_DATE;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case Models.Catalog.KalturaOrder.relevancy:
                    result.m_eOrderBy = OrderBy.RELATED;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case Models.Catalog.KalturaOrder.oldest_first:
                    result.m_eOrderBy = OrderBy.START_DATE;
                    result.m_eOrderDir = OrderDir.ASC;
                    break;
            }
            return result;
        }

        internal static OrderObj ConvertOrderToOrderObj(KalturaPersonalFeedOrderBy order)
        {
            OrderObj result = new OrderObj();

            switch (order)
            {
                case KalturaPersonalFeedOrderBy.NAME_ASC:
                    result.m_eOrderBy = OrderBy.NAME;
                    result.m_eOrderDir = OrderDir.ASC;
                    break;
                case KalturaPersonalFeedOrderBy.NAME_DESC:
                    result.m_eOrderBy = OrderBy.NAME;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaPersonalFeedOrderBy.VIEWS_DESC:
                    result.m_eOrderBy = OrderBy.VIEWS;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaPersonalFeedOrderBy.RATINGS_DESC:
                    result.m_eOrderBy = OrderBy.RATING;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaPersonalFeedOrderBy.VOTES_DESC:
                    result.m_eOrderBy = OrderBy.VOTES_COUNT;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaPersonalFeedOrderBy.START_DATE_DESC:
                    result.m_eOrderBy = OrderBy.START_DATE;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaPersonalFeedOrderBy.RELEVANCY_DESC:
                    result.m_eOrderBy = OrderBy.RELATED;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaPersonalFeedOrderBy.START_DATE_ASC:
                    result.m_eOrderBy = OrderBy.START_DATE;
                    result.m_eOrderDir = OrderDir.ASC;
                    break;
            }
            return result;
        }

        internal static eMessageCategory ConvertInboxMessageType(KalturaInboxMessageTypeHolder inboxMessageTypeHolder)
        {
            eMessageCategory messageCategory = eMessageCategory.SystemAnnouncement;

            switch (inboxMessageTypeHolder.type)
            {
                case KalturaInboxMessageType.SystemAnnouncement:
                    messageCategory = eMessageCategory.SystemAnnouncement;
                    break;
                case KalturaInboxMessageType.Followed:
                    messageCategory = eMessageCategory.Followed;
                    break;
                default:
                    break;
            }

            return messageCategory;
        }

        internal static eTopicAutomaticIssueNotification ConvertAutomaticIssueNotification(KalturaTopicAutomaticIssueNotification automaticIssueNotification)
        {
            switch (automaticIssueNotification)
            {
                case KalturaTopicAutomaticIssueNotification.Yes:
                    return eTopicAutomaticIssueNotification.Yes;
                case KalturaTopicAutomaticIssueNotification.No:
                    return eTopicAutomaticIssueNotification.No;
                case KalturaTopicAutomaticIssueNotification.Inherit:
                default:
                    return eTopicAutomaticIssueNotification.Default;
            }
        }

        private static eTopicAutomaticIssueNotification ConvertAutomaticIssueNotification(bool? automaticIssueFollowNotification)
        {
            if (automaticIssueFollowNotification.HasValue)
            {
                if (automaticIssueFollowNotification.Value)
                    return eTopicAutomaticIssueNotification.Yes;
                else
                    return eTopicAutomaticIssueNotification.No;
            }
            else
                return eTopicAutomaticIssueNotification.Default;
        }
    }
}