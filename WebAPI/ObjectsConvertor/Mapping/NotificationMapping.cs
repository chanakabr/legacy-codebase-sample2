using ApiObjects;
using ApiObjects.Notification;
using ApiObjects.SearchObjects;
using AutoMapper;
using System;
using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Models.Notification;
using WebAPI.Models.Notifications;

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
                 .ForMember(dest => dest.ReminderEnabled, opt => opt.MapFrom(src => src.IsRemindersEnabled))
                 .ForMember(dest => dest.ReminderOffset, opt => opt.MapFrom(src => src.RemindersPrePaddingSec))
                 .ForMember(dest => dest.PushAdapterUrl, opt => opt.MapFrom(src => src.PushAdapterUrl))
                 .ForMember(dest => dest.ChurnMailSubject, opt => opt.MapFrom(src => src.ChurnMailSubject))
                 .ForMember(dest => dest.ChurnMailTemplateName, opt => opt.MapFrom(src => src.ChurnMailTemplateName))
                 .ForMember(dest => dest.SenderEmail, opt => opt.MapFrom(src => src.SenderEmail))
                 .ForMember(dest => dest.MailSenderName, opt => opt.MapFrom(src => src.MailSenderName))
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
                 .ForMember(dest => dest.IsRemindersEnabled, opt => opt.MapFrom(src => src.ReminderEnabled))
                 .ForMember(dest => dest.RemindersPrePaddingSec, opt => opt.MapFrom(src => src.ReminderOffset))
                 .ForMember(dest => dest.PushAdapterUrl, opt => opt.MapFrom(src => src.PushAdapterUrl))
                 .ForMember(dest => dest.ChurnMailSubject, opt => opt.MapFrom(src => src.ChurnMailSubject))
                 .ForMember(dest => dest.ChurnMailTemplateName, opt => opt.MapFrom(src => src.ChurnMailTemplateName))
                 .ForMember(dest => dest.SenderEmail, opt => opt.MapFrom(src => src.SenderEmail))
                 .ForMember(dest => dest.MailSenderName, opt => opt.MapFrom(src => src.MailSenderName))
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
                 .ForMember(dest => dest.ReminderEnabled, opt => opt.MapFrom(src => src.IsRemindersEnabled))
                 .ForMember(dest => dest.ReminderOffset, opt => opt.MapFrom(src => src.RemindersPrePaddingSec))
                 .ForMember(dest => dest.PushAdapterUrl, opt => opt.MapFrom(src => src.PushAdapterUrl))
                 .ForMember(dest => dest.ChurnMailSubject, opt => opt.MapFrom(src => src.ChurnMailSubject))
                 .ForMember(dest => dest.ChurnMailTemplateName, opt => opt.MapFrom(src => src.ChurnMailTemplateName))
                 .ForMember(dest => dest.SenderEmail, opt => opt.MapFrom(src => src.SenderEmail))
                 .ForMember(dest => dest.MailSenderName, opt => opt.MapFrom(src => src.MailSenderName))
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
                 .ForMember(dest => dest.IsRemindersEnabled, opt => opt.MapFrom(src => src.ReminderEnabled))
                 .ForMember(dest => dest.RemindersPrePaddingSec, opt => opt.MapFrom(src => src.ReminderOffset))
                 .ForMember(dest => dest.PushAdapterUrl, opt => opt.MapFrom(src => src.PushAdapterUrl))
                 .ForMember(dest => dest.ChurnMailSubject, opt => opt.MapFrom(src => src.ChurnMailSubject))
                 .ForMember(dest => dest.ChurnMailTemplateName, opt => opt.MapFrom(src => src.ChurnMailTemplateName))
                 .ForMember(dest => dest.SenderEmail, opt => opt.MapFrom(src => src.SenderEmail))
                 .ForMember(dest => dest.MailSenderName, opt => opt.MapFrom(src => src.MailSenderName))
                 ;

            Mapper.CreateMap<UserNotificationSettings, KalturaNotificationSettings>()
                 .ForMember(dest => dest.PushNotificationEnabled, opt => opt.MapFrom(src => src.EnablePush))
                 .ForMember(dest => dest.PushFollowEnabled, opt => opt.MapFrom(src => src.FollowSettings.EnablePush));

            Mapper.CreateMap<bool?, UserFollowSettings>()
               .ForMember(dest => dest.EnablePush, opt => opt.MapFrom(src => src));

            Mapper.CreateMap<KalturaNotificationsSettings, UserNotificationSettings>()
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
                 .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => ConvertTemplateAssetType(src.TemplateType)));

            //KalturaMessageTemplate TO MessageTemplate
            Mapper.CreateMap<KalturaMessageTemplate, MessageTemplate>()
                 .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                 .ForMember(dest => dest.DateFormat, opt => opt.MapFrom(src => src.DateFormat))
                 .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.Action))
                 .ForMember(dest => dest.Sound, opt => opt.MapFrom(src => src.Sound))
                 .ForMember(dest => dest.URL, opt => opt.MapFrom(src => src.URL))
                 .ForMember(dest => dest.TemplateType, opt => opt.MapFrom(src => ConvertTemplateAssetType(src.AssetType)));

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

            Mapper.CreateMap<FollowDataBase, KalturaFollowTvSeries>()
                 .ForMember(dest => dest.AnnouncementId, opt => opt.MapFrom(src => src.AnnouncementId))
                 .ForMember(dest => dest.FollowPhrase, opt => opt.MapFrom(src => src.FollowPhrase))
                 .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => ParseInt(src.FollowReference)))
                 .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                 .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp))
                 .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title));

            Mapper.CreateMap<KalturaFollowTvSeries, FollowDataBase>()
                 .ForMember(dest => dest.AnnouncementId, opt => opt.MapFrom(src => src.AnnouncementId))
                 .ForMember(dest => dest.FollowPhrase, opt => opt.MapFrom(src => src.FollowPhrase))
                 .ForMember(dest => dest.FollowReference, opt => opt.MapFrom(src => src.AssetId))
                 .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                 .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp))
                 .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title));

            Mapper.CreateMap<KalturaFollowTvSeries, FollowDataTvSeries>()
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
                 .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ConvertInboxMessageType(src.Category)));

            //KalturaInboxMessage TO InboxMessage
            Mapper.CreateMap<KalturaInboxMessage, InboxMessage>()
                 .ForMember(dest => dest.Category, opt => opt.MapFrom(src => ConvertInboxMessageType(src.Type)))
                 .ForMember(dest => dest.CreatedAtSec, opt => opt.MapFrom(src => src.CreatedAt))
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                 .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                 .ForMember(dest => dest.State, opt => opt.MapFrom(src => ConvertInboxMessageStatus(src.Status)))
                 .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url));

            //DbAnnouncement to KalturaTopic
            Mapper.CreateMap<DbAnnouncement, KalturaTopic>()
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
                 .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                 .ForMember(dest => dest.SubscribersAmount, opt => opt.MapFrom(src => src.SubscribersAmount))
                 .ForMember(dest => dest.LastMessageSentDateSec, opt => opt.MapFrom(src => src.LastMessageSentDateSec))
                 .ForMember(dest => dest.AutomaticIssueNotification, opt => opt.MapFrom(src => ConvertAutomaticIssueNotification(src.AutomaticIssueFollowNotification)));

            //KalturaTopic TO DbAnnouncement
            Mapper.CreateMap<KalturaTopic, DbAnnouncement>()
                 .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
                 .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                 .ForMember(dest => dest.SubscribersAmount, opt => opt.MapFrom(src => src.SubscribersAmount))
                 .ForMember(dest => dest.LastMessageSentDateSec, opt => opt.MapFrom(src => src.LastMessageSentDateSec))
                 .ForMember(dest => dest.AutomaticIssueFollowNotification, opt => opt.MapFrom(src => ConvertAutomaticIssueNotification(src.AutomaticIssueNotification)));

            //RegistryParameter to KalturaPushWebParameters
            Mapper.CreateMap<RegistryResponse, KalturaRegistryResponse>()
                 .ForMember(dest => dest.AnnouncementId, opt => opt.MapFrom(src => src.NotificationId))
                 .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Key))
                 .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url));

            //DbReminder to KalturaReminder
            Mapper.CreateMap<DbReminder, KalturaReminder>()
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
                 .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            // KalturaReminder to DbReminder
            Mapper.CreateMap<KalturaReminder, DbReminder>()
                 .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
                 .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            //DbReminder to KalturaAssetReminder
            Mapper.CreateMap<DbReminder, KalturaAssetReminder>()
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
                 .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                 .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.Reference));

            #region Engagement Adapter

            Mapper.CreateMap<KalturaEngagementAdapter, EngagementAdapter>()
               .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
               .ForMember(dest => dest.ProviderUrl, opt => opt.MapFrom(src => src.ProviderUrl))
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
               .ForMember(dest => dest.SkipSettings, opt => opt.MapFrom(src => src.Settings == null))
               .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => ConvertEngagementAdapterSettings(src.Settings)))
               ;

            Mapper.CreateMap<EngagementAdapter, KalturaEngagementAdapter>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
              .ForMember(dest => dest.ProviderUrl, opt => opt.MapFrom(src => src.ProviderUrl))
              .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
              .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => ConvertEngagementAdapterSettings(src.Settings)))
              ;

            Mapper.CreateMap<EngagementAdapterBase, KalturaEngagementAdapterBase>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            Mapper.CreateMap<EngagementAdapterResponse, KalturaEngagementAdapter>()
             .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.EngagementAdapter.AdapterUrl))
             .ForMember(dest => dest.ProviderUrl, opt => opt.MapFrom(src => src.EngagementAdapter.ProviderUrl))
             .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.EngagementAdapter.ID))
             .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.EngagementAdapter.IsActive))
             .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.EngagementAdapter.Name))
             .ForMember(dest => dest.SharedSecret, opt => opt.MapFrom(src => src.EngagementAdapter.SharedSecret))
             .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => ConvertEngagementAdapterSettings(src.EngagementAdapter.Settings)));

            #endregion

            #region Engagement

            Mapper.CreateMap<KalturaEngagement, Engagement>()
               .ForMember(dest => dest.AdapterDynamicData, opt => opt.MapFrom(src => src.AdapterDynamicData))
               .ForMember(dest => dest.AdapterId, opt => opt.MapFrom(src => src.AdapterId))
               .ForMember(dest => dest.EngagementType, opt => opt.MapFrom(src => ConvertEngagementType( src.Type)))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.IntervalSeconds, opt => opt.MapFrom(src => src.IntervalSeconds))
               .ForMember(dest => dest.SendTime, opt => opt.MapFrom(src => ConvertSendTime(src.SendTimeInSeconds)))
               .ForMember(dest => dest.TotalNumberOfRecipients, opt => opt.MapFrom(src => src.TotalNumberOfRecipients))
               .ForMember(dest => dest.UserList, opt => opt.MapFrom(src => src.UserList))
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
               .ForMember(dest => dest.CouponGroupId, opt => opt.MapFrom(src => src.CouponGroupId))
               ;

            Mapper.CreateMap<Engagement, KalturaEngagement>()
               .ForMember(dest => dest.AdapterDynamicData, opt => opt.MapFrom(src => src.AdapterDynamicData))
               .ForMember(dest => dest.AdapterId, opt => opt.MapFrom(src => src.AdapterId))
               .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ConvertEngagementType(src.EngagementType)))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.IntervalSeconds, opt => opt.MapFrom(src => src.IntervalSeconds))
               .ForMember(dest => dest.SendTimeInSeconds, opt => opt.MapFrom(src => ConvertSendTime(src.SendTime)))
               .ForMember(dest => dest.TotalNumberOfRecipients, opt => opt.MapFrom(src => src.TotalNumberOfRecipients))
               .ForMember(dest => dest.UserList, opt => opt.MapFrom(src => src.UserList))
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
               .ForMember(dest => dest.CouponGroupId, opt => opt.MapFrom(src => src.CouponGroupId))
               ;
            
            #endregion
        }

        private static long ConvertSendTime(DateTime dateTime)
        {
            return WebAPI.Utils.SerializationUtils.ConvertToUnixTimestamp(dateTime);
        }

        private static DateTime ConvertSendTime(long dateTime)
        {
            return WebAPI.Utils.SerializationUtils.ConvertFromUnixTimestamp(dateTime);
        }

        public static KalturaEngagementType ConvertEngagementType(eEngagementType eEngagementType)
        {
            switch (eEngagementType)
            {
                case eEngagementType.Churn:
                    return KalturaEngagementType.Churn;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown asset Type");
            }
        }

        public static eEngagementType ConvertEngagementType(KalturaEngagementType kalturaEngagementType)
        {
            switch (kalturaEngagementType)
            {
                case KalturaEngagementType.Churn:
                    return eEngagementType.Churn;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown asset Type");
            }
        }

        internal static Dictionary<string, KalturaStringValue> ConvertEngagementAdapterSettings(List<EngagementAdapterSettings> settings)
        {
            Dictionary<string, KalturaStringValue> result = null;

            if (settings != null && settings.Count > 0)
            {
                result = new Dictionary<string, KalturaStringValue>();
                foreach (var data in settings)
                {
                    if (!string.IsNullOrEmpty(data.Key))
                    {
                        result.Add(data.Key, new KalturaStringValue() { value = data.Value });
                    }
                }
            }
            return result;
        }

        internal static List<EngagementAdapterSettings> ConvertEngagementAdapterSettings(SerializableDictionary<string, KalturaStringValue> settings)
        {
            List<EngagementAdapterSettings> result = null;

            if (settings != null && settings.Count > 0)
            {
                result = new List<EngagementAdapterSettings>();
                EngagementAdapterSettings pc;
                foreach (KeyValuePair<string, KalturaStringValue> data in settings)
                {
                    if (!string.IsNullOrEmpty(data.Key))
                    {
                        pc = new EngagementAdapterSettings();
                        pc.Key = data.Key;
                        pc.Value = data.Value.value;
                        result.Add(pc);
                    }
                }
            }
            if (result != null && result.Count > 0)
            {
                return result;
            }
            else
            {
                return null;
            }
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
                case KalturaInboxMessageType.Engagement:
                    result = eMessageCategory.Engagement;
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
                case eMessageCategory.Engagement:
                    result = KalturaInboxMessageType.Engagement;
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

        public static KalturaMessageTemplateType ConvertTemplateAssetType(MessageTemplateType assetType)
        {
            KalturaMessageTemplateType result;

            switch (assetType)
            {
                case MessageTemplateType.Series:
                    result = KalturaMessageTemplateType.Series;
                    break;
                case MessageTemplateType.Reminder:
                    result = KalturaMessageTemplateType.Reminder;
                    break;
                case MessageTemplateType.Churn:
                    result = KalturaMessageTemplateType.Churn;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown message Type");
            }

            return result;
        }

        public static MessageTemplateType ConvertTemplateAssetType(KalturaMessageTemplateType assetType)
        {
            MessageTemplateType result;

            switch (assetType)
            {
                case KalturaMessageTemplateType.Series:
                    result = MessageTemplateType.Series;
                    break;
                case KalturaMessageTemplateType.Reminder:
                    result = MessageTemplateType.Reminder;
                    break;
                case KalturaMessageTemplateType.Churn:
                    result = MessageTemplateType.Churn;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown message Type");
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
                case KalturaInboxMessageType.Engagement:
                    messageCategory = eMessageCategory.Engagement;
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

        internal static OrderObj ConvertOrderToOrderObj(KalturaAssetOrderBy orderBy)
        {

            OrderObj result = new OrderObj();

            switch (orderBy)
            {
                case KalturaAssetOrderBy.NAME_ASC:
                    result.m_eOrderBy = OrderBy.NAME;
                    result.m_eOrderDir = OrderDir.ASC;
                    break;
                case KalturaAssetOrderBy.NAME_DESC:
                    result.m_eOrderBy = OrderBy.NAME;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.VIEWS_DESC:
                    result.m_eOrderBy = OrderBy.VIEWS;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.RATINGS_DESC:
                    result.m_eOrderBy = OrderBy.RATING;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.VOTES_DESC:
                    result.m_eOrderBy = OrderBy.VOTES_COUNT;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.START_DATE_DESC:
                    result.m_eOrderBy = OrderBy.START_DATE;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.RELEVANCY_DESC:
                    result.m_eOrderBy = OrderBy.RELATED;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.START_DATE_ASC:
                    result.m_eOrderBy = OrderBy.START_DATE;
                    result.m_eOrderDir = OrderDir.ASC;
                    break;
                case KalturaAssetOrderBy.LIKES_DESC:
                    result.m_eOrderBy = OrderBy.LIKE_COUNTER;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
            }
            return result;
        }       
    }
}