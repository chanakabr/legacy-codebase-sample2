using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Notification;
using ApiObjects.SearchObjects;
using AutoMapper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using TVinciShared;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Models.Notification;
using WebAPI.ObjectsConvertor.Extensions;
using KeyValuePair = ApiObjects.KeyValuePair;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class NotificationMapping
    {
        public static void RegisterMappings(MapperConfigurationExpression cfg)
        {
            //NotificationPartnerSettings to KalturaPartnerNotificationSettings
            cfg.CreateMap<NotificationPartnerSettings, KalturaPartnerNotificationSettings>()
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
                 .ForMember(dest => dest.MailNotificationAdapterId, opt => opt.MapFrom(src => src.MailNotificationAdapterId))
                 .ForMember(dest => dest.SmsEnabled, opt => opt.MapFrom(src => src.IsSMSEnabled))
                 .ForMember(dest => dest.IotEnabled, opt => opt.MapFrom(src => src.IsIotEnabled));

            //KalturaPartnerNotificationSettings TO NotificationPartnerSettings
            cfg.CreateMap<KalturaPartnerNotificationSettings, NotificationPartnerSettings>()
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
                 .ForMember(dest => dest.MailNotificationAdapterId, opt => opt.MapFrom(src => src.MailNotificationAdapterId))
                 .ForMember(dest => dest.IsSMSEnabled, opt => opt.MapFrom(src => src.SmsEnabled))
                 .ForMember(dest => dest.IsIotEnabled, opt => opt.MapFrom(src => src.IotEnabled))
                 ;

            //NotificationPartnerSettings to KalturaNotificationPartnerSettings
            cfg.CreateMap<NotificationPartnerSettings, KalturaNotificationsPartnerSettings>()
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
                 .ForMember(dest => dest.MailNotificationAdapterId, opt => opt.MapFrom(src => src.MailNotificationAdapterId))
                 .ForMember(dest => dest.SmsEnabled, opt => opt.MapFrom(src => src.IsSMSEnabled))
                 .ForMember(dest => dest.IotEnabled, opt => opt.MapFrom(src => src.IsIotEnabled))
                 .ForMember(dest => dest.EpgNotification, opt => opt.MapFrom(src => src.EpgNotification))
                 .ForMember(dest => dest.LineupNotification, opt => opt.MapFrom(src => src.LineupNotification))
                 ;

            //KalturaNotificationPartnerSettings TO NotificationPartnerSettings
            cfg.CreateMap<KalturaNotificationsPartnerSettings, NotificationPartnerSettings>()
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
                 .ForMember(dest => dest.MailNotificationAdapterId, opt => opt.MapFrom(src => src.MailNotificationAdapterId))
                 .ForMember(dest => dest.MailSenderName, opt => opt.MapFrom(src => src.MailSenderName))
                 .ForMember(dest => dest.IsSMSEnabled, opt => opt.MapFrom(src => src.SmsEnabled))
                 .ForMember(dest => dest.IsIotEnabled, opt => opt.MapFrom(src => src.IotEnabled))
                 .ForMember(dest => dest.EpgNotification, opt => opt.MapFrom(src => src.EpgNotification))
                 .ForMember(dest => dest.LineupNotification, opt => opt.MapFrom(src => src.LineupNotification))
                 ;

            cfg.CreateMap<KalturaEpgNotificationSettings, EpgNotificationSettings>()
                .ForMember(dest => dest.Enabled, opt => opt.MapFrom(src => src.Enabled))
                .ForMember(dest => dest.BackwardTimeRange, opt => opt.ResolveUsing(src => src.BackwardTimeRange ?? 24))
                .ForMember(dest => dest.ForwardTimeRange, opt => opt.ResolveUsing(src => src.ForwardTimeRange ?? 24))
                .ForMember(dest => dest.DeviceFamilyIds, opt => opt.ResolveUsing(src => src.DeviceFamilyIds
                    .GetItemsIn<int>(out var failed, true)
                    .ThrowIfFailed(failed, () => new ClientException((int)StatusCode.InvalidArgumentValue, "invalid value in deviceFamilyIds"))))
                .ForMember(dest => dest.LiveAssetIds, opt => opt.ResolveUsing(src => src.LiveAssetIds
                    .GetItemsIn<long>(out var failed, true)
                    .ThrowIfFailed(failed, () => new ClientException((int)StatusCode.InvalidArgumentValue, "invalid value in liveAssetIds"))))
                ;

            cfg.CreateMap<KalturaLineupNotificationSettings, LineupNotificationSettings>()
                .ForMember(dest => dest.Enabled, opt => opt.MapFrom(src => src.Enabled));

            cfg.CreateMap<EpgNotificationSettings, KalturaEpgNotificationSettings>()
                .ForMember(dest => dest.Enabled, opt => opt.MapFrom(src => src.Enabled))
                .ForMember(dest => dest.BackwardTimeRange, opt => opt.MapFrom(src => src.BackwardTimeRange))
                .ForMember(dest => dest.ForwardTimeRange, opt => opt.MapFrom(src => src.ForwardTimeRange))
                .ForMember(dest => dest.DeviceFamilyIds, opt => opt.ResolveUsing(src => string.Join(",", src.DeviceFamilyIds)))
                .ForMember(dest => dest.LiveAssetIds, opt => opt.ResolveUsing(src => string.Join(",", src.LiveAssetIds)))
                ;

            cfg.CreateMap<LineupNotificationSettings, KalturaLineupNotificationSettings>()
                .ForMember(dest => dest.Enabled, opt => opt.MapFrom(src => src.Enabled));

            cfg.CreateMap<UserNotificationSettings, KalturaNotificationSettings>()
                 .ForMember(dest => dest.PushNotificationEnabled, opt => opt.MapFrom(src => src.EnablePush))
                 .ForMember(dest => dest.PushFollowEnabled, opt => opt.MapFrom(src => src.FollowSettings.EnablePush))
                 .ForMember(dest => dest.MailEnabled, opt => opt.MapFrom(src => src.EnableMail))
                 .ForMember(dest => dest.SmsEnabled, opt => opt.MapFrom(src => src.EnableSms))
                ;
            cfg.CreateMap<bool?, UserFollowSettings>()
               .ForMember(dest => dest.EnablePush, opt => opt.MapFrom(src => src));

            cfg.CreateMap<KalturaNotificationsSettings, UserNotificationSettings>()
                 .ForMember(dest => dest.EnablePush, opt => opt.MapFrom(src => src.PushNotificationEnabled))
                 .ForMember(dest => dest.FollowSettings, opt => opt.MapFrom(src => src.PushFollowEnabled))
                 .ForMember(dest => dest.EnableMail, opt => opt.MapFrom(src => src.MailEnabled))
                 .ForMember(dest => dest.EnableSms, opt => opt.MapFrom(src => src.SmsEnabled));

            cfg.CreateMap<KalturaSmsAdapterProfile, SmsAdapterProfile>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.SharedSecret, opt => opt.MapFrom(src => src.SharedSecret))
                .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.ExternalIdentifier))
                ;

            cfg.CreateMap<SmsAdapterProfile, KalturaSmsAdapterProfile>()
               .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => src.Settings != null ? 
               src.Settings.ToDictionary(k => k.Key, v => v.Value) : null))
               ;

            cfg.CreateMap<KalturaSmsAdapterProfile, SmsAdapterProfile>()
                .ForMember(dest => dest.Settings, opt => opt.ResolveUsing(src => ConvertSmsAdapterSettings(src)))
                ;

            cfg.CreateMap<SmsAdapterProfile, KalturaSmsAdapterProfile>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => src.Settings))
                .ForMember(dest => dest.SharedSecret, opt => opt.MapFrom(src => src.SharedSecret))
                .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
                ;

            cfg.CreateMap<KalturaAnnouncement, MessageAnnouncement>()
                 .ForMember(dest => dest.Enabled, opt => opt.MapFrom(src => src.getEnabled()))
                 .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                 .ForMember(dest => dest.MessageAnnouncementId, opt => opt.MapFrom(src => src.getId()))
                 .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                 .ForMember(dest => dest.Recipients, opt => opt.MapFrom(src => src.Recipients))
                 .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.getStartTime()))
                 .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                 .ForMember(dest => dest.Timezone, opt => opt.MapFrom(src => src.Timezone))
                 .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
                 .ForMember(dest => dest.IncludeMail, opt => opt.MapFrom(src => src.IncludeMail))
                 .ForMember(dest => dest.MailSubject, opt => opt.MapFrom(src => src.MailSubject))
                 .ForMember(dest => dest.MailTemplate, opt => opt.MapFrom(src => src.MailTemplate))
                 .ForMember(dest => dest.IncludeSms, opt => opt.MapFrom(src => src.IncludeSms))
                 .ForMember(dest => dest.IncludeIot, opt => opt.MapFrom(src => src.IncludeIot))
                 .ForMember(dest => dest.IncludeUserInbox, opt => opt.MapFrom(src => src.IncludeUserInbox ?? false))
                 ;

            cfg.CreateMap<KalturaAnnouncementRecipientsType, eAnnouncementRecipientsType>()
                .ConvertUsing(KalturaRecipientsType =>
                {
                    switch (KalturaRecipientsType)
                    {
                        case KalturaAnnouncementRecipientsType.All:
                            return eAnnouncementRecipientsType.All;
                        case KalturaAnnouncementRecipientsType.LoggedIn:
                            return eAnnouncementRecipientsType.LoggedIn;
                        case KalturaAnnouncementRecipientsType.Guests:
                            return eAnnouncementRecipientsType.Guests;
                        case KalturaAnnouncementRecipientsType.Other:
                            return eAnnouncementRecipientsType.Other;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown KalturaAnnouncementRecipientsType value: {KalturaRecipientsType}");
                    }
                });

            cfg.CreateMap<KalturaAnnouncementStatus, eAnnouncementStatus>()
                .ConvertUsing(KalturaAnnouncementStatus =>
                {
                    switch (KalturaAnnouncementStatus)
                    {
                        case KalturaAnnouncementStatus.NotSent:
                            return eAnnouncementStatus.NotSent;
                        case KalturaAnnouncementStatus.Sending:
                            return eAnnouncementStatus.Sending;
                        case KalturaAnnouncementStatus.Sent:
                            return eAnnouncementStatus.Sent;
                        case KalturaAnnouncementStatus.Aborted:
                            return eAnnouncementStatus.Aborted;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown KalturaAnnouncementStatus value: {KalturaAnnouncementStatus}");
                    }
                });

            cfg.CreateMap<MessageAnnouncement, KalturaAnnouncement>()
                 .ForMember(dest => dest.Enabled, opt => opt.MapFrom(src => src.Enabled))
                 .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.MessageAnnouncementId))
                 .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                 .ForMember(dest => dest.Recipients, opt => opt.MapFrom(src => src.Recipients))
                 .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
                 .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                 .ForMember(dest => dest.Timezone, opt => opt.MapFrom(src => src.Timezone))
                 .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
                 .ForMember(dest => dest.IncludeMail, opt => opt.MapFrom(src => src.IncludeMail))
                 .ForMember(dest => dest.MailSubject, opt => opt.MapFrom(src => src.MailSubject))
                 .ForMember(dest => dest.MailTemplate, opt => opt.MapFrom(src => src.MailTemplate))
                 .ForMember(dest => dest.IncludeSms, opt => opt.MapFrom(src => src.IncludeSms))
                 .ForMember(dest => dest.IncludeIot, opt => opt.MapFrom(src => src.IncludeIot))
                 .ForMember(dest => dest.IncludeUserInbox, opt => opt.MapFrom(src => src.IncludeUserInbox))
                 ;

            cfg.CreateMap<eAnnouncementRecipientsType, KalturaAnnouncementRecipientsType>()
                .ConvertUsing(recipientsType =>
                {
                    switch (recipientsType)
                    {
                        case eAnnouncementRecipientsType.All:
                            return KalturaAnnouncementRecipientsType.All;
                        case eAnnouncementRecipientsType.LoggedIn:
                            return KalturaAnnouncementRecipientsType.LoggedIn;
                        case eAnnouncementRecipientsType.Guests:
                            return KalturaAnnouncementRecipientsType.Guests;
                        case eAnnouncementRecipientsType.Other:
                            return KalturaAnnouncementRecipientsType.Other;
                        default:
                            throw new ClientException((int)StatusCode.Error, "Unknown recipients Type");
                    }
                });

            cfg.CreateMap<eAnnouncementStatus, KalturaAnnouncementStatus>()
                .ConvertUsing(announcementStatus =>
                {
                    switch (announcementStatus)
                    {
                        case eAnnouncementStatus.NotSent:
                            return KalturaAnnouncementStatus.NotSent;
                        case eAnnouncementStatus.Sending:
                            return KalturaAnnouncementStatus.Sending;
                        case eAnnouncementStatus.Sent:
                            return KalturaAnnouncementStatus.Sent;
                        case eAnnouncementStatus.Aborted:
                            return KalturaAnnouncementStatus.Aborted;
                        default:
                            throw new ClientException((int)StatusCode.Error, "Unknown status Type");
                    }
                });

            cfg.CreateMap<KalturaAnnouncementFilter, MessageAnnouncementFilter>()
                 .ForMember(dest => dest.MessageAnnouncementIds, opt => opt.MapFrom(src => WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(src.IdIn, "KalturaAnnouncementFilter.idIn", true, true)));

            //MessageTemplate to KalturaMessageTemplate
            cfg.CreateMap<MessageTemplate, KalturaMessageTemplate>()
                 .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                 .ForMember(dest => dest.DateFormat, opt => opt.MapFrom(src => src.DateFormat))
                 .ForMember(dest => dest.Sound, opt => opt.MapFrom(src => src.Sound))
                 .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.Action))
                 .ForMember(dest => dest.URL, opt => opt.MapFrom(src => src.URL))
                 .ForMember(dest => dest.MailTemplate, opt => opt.MapFrom(src => src.MailTemplate))
                 .ForMember(dest => dest.MailSubject, opt => opt.MapFrom(src => src.MailSubject))
                 .ForMember(dest => dest.RatioId, opt => opt.MapFrom(src => src.RatioId))
                 .ForMember(dest => dest.MessageType, opt => opt.ResolveUsing(src => ConvertTemplateAssetType(src.TemplateType)));

            //KalturaMessageTemplate TO MessageTemplate
            cfg.CreateMap<KalturaMessageTemplate, MessageTemplate>()
                 .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                 .ForMember(dest => dest.DateFormat, opt => opt.MapFrom(src => src.DateFormat))
                 .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.Action))
                 .ForMember(dest => dest.Sound, opt => opt.MapFrom(src => src.Sound))
                 .ForMember(dest => dest.URL, opt => opt.MapFrom(src => src.URL))
                 .ForMember(dest => dest.MailTemplate, opt => opt.MapFrom(src => src.MailTemplate))
                 .ForMember(dest => dest.MailSubject, opt => opt.MapFrom(src => src.MailSubject))
                 .ForMember(dest => dest.RatioId, opt => opt.MapFrom(src => src.RatioId))
                 .ForMember(dest => dest.TemplateType, opt => opt.ResolveUsing(src => ConvertTemplateAssetType(src.MessageType)));

            cfg.CreateMap<FollowDataBase, KalturaFollowDataTvSeries>()
                 .ForMember(dest => dest.AnnouncementId, opt => opt.MapFrom(src => src.AnnouncementId))
                 .ForMember(dest => dest.FollowPhrase, opt => opt.MapFrom(src => src.FollowPhrase))
                 .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => ParseInt(src.FollowReference)))
                 .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                 .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp))
                 .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title));

            cfg.CreateMap<FollowDataBase, KalturaFollowTvSeries>()
                .ForMember(dest => dest.AnnouncementId, opt => opt.MapFrom(src => src.AnnouncementId))
                .ForMember(dest => dest.FollowPhrase, opt => opt.MapFrom(src => src.FollowPhrase))
                .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => ParseInt(src.FollowReference)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title));

            cfg.CreateMap<KalturaFollowDataTvSeries, FollowDataBase>()
                 .ForMember(dest => dest.AnnouncementId, opt => opt.MapFrom(src => src.AnnouncementId))
                 .ForMember(dest => dest.FollowPhrase, opt => opt.MapFrom(src => src.FollowPhrase))
                 .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                 .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp))
                 .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title));

            cfg.CreateMap<KalturaFollowDataTvSeries, FollowDataTvSeries>()
                 .ForMember(dest => dest.AnnouncementId, opt => opt.MapFrom(src => src.AnnouncementId))
                 .ForMember(dest => dest.FollowPhrase, opt => opt.MapFrom(src => src.FollowPhrase))
                 .ForMember(dest => dest.FollowReference, opt => opt.MapFrom(src => src.AssetId.ToString()))
                 .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                 .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp))
                 .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                 .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.AssetId))
                 .ForMember(dest => dest.Type, opt => opt.MapFrom(src => 0));

            cfg.CreateMap<KalturaFollowTvSeries, FollowDataBase>()
                 .ForMember(dest => dest.AnnouncementId, opt => opt.MapFrom(src => src.AnnouncementId))
                 .ForMember(dest => dest.FollowPhrase, opt => opt.MapFrom(src => src.FollowPhrase))
                 .ForMember(dest => dest.FollowReference, opt => opt.MapFrom(src => src.AssetId))
                 .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                 .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp))
                 .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title));

            cfg.CreateMap<KalturaFollowTvSeries, FollowDataTvSeries>()
                 .ForMember(dest => dest.AnnouncementId, opt => opt.MapFrom(src => src.AnnouncementId))
                 .ForMember(dest => dest.FollowPhrase, opt => opt.MapFrom(src => src.FollowPhrase))
                 .ForMember(dest => dest.FollowReference, opt => opt.MapFrom(src => src.AssetId.ToString()))
                 .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                 .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp))
                 .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                 .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.AssetId))
                 .ForMember(dest => dest.Type, opt => opt.MapFrom(src => 0));

            cfg.CreateMap<int, KalturaPersonalFollowFeed>()
               .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src));

            cfg.CreateMap<int, KalturaPersonalFeed>()
               .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src));

            //InboxMessage to KalturaInboxMessage
            cfg.CreateMap<InboxMessage, KalturaInboxMessage>()
                 .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAtSec))
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                 .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                 .ForMember(dest => dest.Status, opt => opt.ResolveUsing(src => ConvertInboxMessageStatus(src.State)))
                 .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url))
                 .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => ConvertInboxMessageType(src.Category)))
                 .ForMember(dest => dest.CampaignId, opt => opt.MapFrom(src => src.CampaignId))
                 ;

            //KalturaInboxMessage TO InboxMessage
            cfg.CreateMap<KalturaInboxMessage, InboxMessage>()
                 .ForMember(dest => dest.Category, opt => opt.ResolveUsing(src => ConvertInboxMessageType(src.Type)))
                 .ForMember(dest => dest.CreatedAtSec, opt => opt.MapFrom(src => src.CreatedAt))
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                 .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                 .ForMember(dest => dest.State, opt => opt.ResolveUsing(src => ConvertInboxMessageStatus(src.Status)))
                 .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url))
                 .ForMember(dest => dest.CampaignId, opt => opt.MapFrom(src => src.CampaignId))
                 ;

            //DbAnnouncement to KalturaTopic
            cfg.CreateMap<DbAnnouncement, KalturaTopic>()
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
                 .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                 .ForMember(dest => dest.SubscribersAmount, opt => opt.MapFrom(src => src.SubscribersAmount))
                 .ForMember(dest => dest.LastMessageSentDateSec, opt => opt.MapFrom(src => src.LastMessageSentDateSec))
                 .ForMember(dest => dest.AutomaticIssueNotification, opt => opt.ResolveUsing(src => ConvertAutomaticIssueNotification(src.AutomaticIssueFollowNotification)));

            //KalturaTopic TO DbAnnouncement
            cfg.CreateMap<KalturaTopic, DbAnnouncement>()
                 .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
                 .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                 .ForMember(dest => dest.SubscribersAmount, opt => opt.MapFrom(src => src.SubscribersAmount))
                 .ForMember(dest => dest.LastMessageSentDateSec, opt => opt.MapFrom(src => src.LastMessageSentDateSec))
                 .ForMember(dest => dest.AutomaticIssueFollowNotification, opt => opt.ResolveUsing(src => ConvertAutomaticIssueNotification(src.AutomaticIssueNotification)));

            //RegistryParameter to KalturaPushWebParameters
            cfg.CreateMap<RegistryResponse, KalturaRegistryResponse>()
                 .ForMember(dest => dest.AnnouncementId, opt => opt.MapFrom(src => src.NotificationId))
                 .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Key))
                 .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url));

            //DbReminder to KalturaReminder
            cfg.CreateMap<DbReminder, KalturaReminder>()
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
                 .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                 .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src is KalturaSeriesReminder ? KalturaReminderType.SERIES : KalturaReminderType.ASSET));

            // KalturaReminder to DbReminder
            cfg.CreateMap<KalturaReminder, DbReminder>()
                 .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
                 .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            //DbReminder to KalturaAssetReminder
            cfg.CreateMap<DbReminder, KalturaAssetReminder>()
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
                 .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                 .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.Reference))
                 .ForMember(dest => dest.Type, opt => opt.MapFrom(src => KalturaReminderType.ASSET))
                 ;

            //DbReminder to KalturaAssetReminder
            cfg.CreateMap<DbSeriesReminder, KalturaSeriesReminder>()
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
                 .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                 .ForMember(dest => dest.SeriesId, opt => opt.MapFrom(src => src.SeriesId))
                 .ForMember(dest => dest.SeasonNumber, opt => opt.MapFrom(src => src.SeasonNumber.ToNullable()))
                 .ForMember(dest => dest.EpgChannelId, opt => opt.MapFrom(src => src.EpgChannelId))
                 .ForMember(dest => dest.Type, opt => opt.MapFrom(src => KalturaReminderType.SERIES));

            cfg.CreateMap<DbReminder, KalturaReminder>()
                .Include<DbSeriesReminder, KalturaSeriesReminder>();

            cfg.CreateMap<KalturaReminder, DbReminder>()
                .Include<KalturaSeriesReminder, DbSeriesReminder>();

            cfg.CreateMap<KalturaSeriesReminder, DbSeriesReminder>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.SeriesId, opt => opt.MapFrom(src => src.SeriesId))
                .ForMember(dest => dest.SeasonNumber, opt => opt.MapFrom(src => src.SeasonNumber))
                .ForMember(dest => dest.EpgChannelId, opt => opt.MapFrom(src => src.EpgChannelId));

            cfg.CreateMap<KalturaOTTObject, CoreObject>();
            cfg.CreateMap<CoreObject, KalturaOTTObject>();

            cfg.CreateMap<KalturaConcurrencyViolation, ConcurrencyViolation>()
                .IncludeBase<KalturaOTTObject, CoreObject>()
                .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.HouseholdId, opt => opt.MapFrom(src => src.HouseholdId))
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp))
                .ForMember(dest => dest.UDID, opt => opt.MapFrom(src => src.UDID))
                .ForMember(dest => dest.ViolationRule, opt => opt.MapFrom(src => src.ViolationRule))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId));

            cfg.CreateMap<ConcurrencyViolation, KalturaConcurrencyViolation>()
                .IncludeBase<CoreObject, KalturaOTTObject>()
                .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.HouseholdId, opt => opt.MapFrom(src => src.HouseholdId))
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp))
                .ForMember(dest => dest.UDID, opt => opt.MapFrom(src => src.UDID))
                .ForMember(dest => dest.ViolationRule, opt => opt.MapFrom(src => src.ViolationRule))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId));

            cfg.CreateMap<KalturaBookmarkEvent, BookmarkEvent>()
                .IncludeBase<KalturaOTTObject, CoreObject>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.HouseholdId, opt => opt.MapFrom(src => src.HouseholdId))
                .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.FileId, opt => opt.MapFrom(src => src.FileId))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Position))
                .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.Action))
                .ForMember(dest => dest.ProductType, opt => opt.MapFrom(src => src.ProductType))
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId));

            cfg.CreateMap<BookmarkEvent, KalturaBookmarkEvent>()
               .IncludeBase<CoreObject, KalturaOTTObject>()
               .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
               .ForMember(dest => dest.HouseholdId, opt => opt.MapFrom(src => src.HouseholdId))
               .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.AssetId))
               .ForMember(dest => dest.FileId, opt => opt.MapFrom(src => src.FileId))
               .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Position))
               .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.Action))
               .ForMember(dest => dest.ProductType, opt => opt.MapFrom(src => src.ProductType))
               .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId));

            cfg.CreateMap<KalturaTriggerCampaignEvent, TriggerCampaignEvent>()
                .IncludeBase<KalturaOTTObject, CoreObject>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.CampaignId, opt => opt.MapFrom(src => src.CampaignId))
                .ForMember(dest => dest.Udid, opt => opt.MapFrom(src => src.Udid))
                .ForMember(dest => dest.DomainId, opt => opt.MapFrom(src => src.HouseholdId))
                ;

            cfg.CreateMap<TriggerCampaignEvent, KalturaTriggerCampaignEvent>()
               .IncludeBase<CoreObject, KalturaOTTObject>()
               .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.CampaignId, opt => opt.MapFrom(src => src.CampaignId))
                .ForMember(dest => dest.Udid, opt => opt.MapFrom(src => src.Udid))
                .ForMember(dest => dest.HouseholdId, opt => opt.MapFrom(src => src.DomainId))
                ;

            cfg.CreateMap<KalturaBookmarkActionType, MediaPlayActions>()
                .ConvertUsing(bookmarkActionType =>
                {
                    switch (bookmarkActionType)
                    {
                        case KalturaBookmarkActionType.HIT:
                            return MediaPlayActions.HIT;
                        case KalturaBookmarkActionType.PLAY:
                            return MediaPlayActions.PLAY;
                        case KalturaBookmarkActionType.STOP:
                            return MediaPlayActions.STOP;
                        case KalturaBookmarkActionType.PAUSE:
                            return MediaPlayActions.PAUSE;
                        case KalturaBookmarkActionType.FIRST_PLAY:
                            return MediaPlayActions.FIRST_PLAY;
                        case KalturaBookmarkActionType.SWOOSH:
                            return MediaPlayActions.SWOOSH;
                        case KalturaBookmarkActionType.FULL_SCREEN:
                            return MediaPlayActions.FULL_SCREEN;
                        case KalturaBookmarkActionType.SEND_TO_FRIEND:
                            return MediaPlayActions.SEND_TO_FRIEND;
                        case KalturaBookmarkActionType.LOAD:
                            return MediaPlayActions.LOAD;
                        case KalturaBookmarkActionType.FULL_SCREEN_EXIT:
                            return MediaPlayActions.FULL_SCREEN_EXIT;
                        case KalturaBookmarkActionType.FINISH:
                            return MediaPlayActions.FINISH;
                        case KalturaBookmarkActionType.BITRATE_CHANGE:
                            return MediaPlayActions.BITRATE_CHANGE;
                        case KalturaBookmarkActionType.ERROR:
                            return MediaPlayActions.ERROR;
                        case KalturaBookmarkActionType.NONE:
                            return MediaPlayActions.NONE;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown KalturaBookmarkActionType value: {bookmarkActionType}");
                    }
                });

            cfg.CreateMap<MediaPlayActions, KalturaBookmarkActionType>()
                .ConvertUsing(mediaPlayAction =>
                {
                    switch (mediaPlayAction)
                    {
                        case MediaPlayActions.HIT:
                            return KalturaBookmarkActionType.HIT;
                        case MediaPlayActions.PLAY:
                            return KalturaBookmarkActionType.PLAY;
                        case MediaPlayActions.STOP:
                            return KalturaBookmarkActionType.STOP;
                        case MediaPlayActions.PAUSE:
                            return KalturaBookmarkActionType.PAUSE;
                        case MediaPlayActions.FIRST_PLAY:
                            return KalturaBookmarkActionType.FIRST_PLAY;
                        case MediaPlayActions.SWOOSH:
                            return KalturaBookmarkActionType.SWOOSH;
                        case MediaPlayActions.FULL_SCREEN:
                            return KalturaBookmarkActionType.FULL_SCREEN;
                        case MediaPlayActions.SEND_TO_FRIEND:
                            return KalturaBookmarkActionType.SEND_TO_FRIEND;
                        case MediaPlayActions.LOAD:
                            return KalturaBookmarkActionType.LOAD;
                        case MediaPlayActions.FULL_SCREEN_EXIT:
                            return KalturaBookmarkActionType.FULL_SCREEN_EXIT;
                        case MediaPlayActions.FINISH:
                            return KalturaBookmarkActionType.FINISH;
                        case MediaPlayActions.BITRATE_CHANGE:
                            return KalturaBookmarkActionType.BITRATE_CHANGE;
                        case MediaPlayActions.ERROR:
                            return KalturaBookmarkActionType.ERROR;
                        case MediaPlayActions.NONE:
                            return KalturaBookmarkActionType.NONE;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown MediaPlayActions value: {mediaPlayAction}");
                    }
                });

            cfg.CreateMap<KalturaAssetEvent, AssetEvent>()
                .IncludeBase<KalturaOTTObject, CoreObject>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.ExternalId));

            cfg.CreateMap<KalturaProgramAssetEvent, EpgAssetEvent>()
                .IncludeBase<KalturaAssetEvent, AssetEvent>()
                .ForMember(dest => dest.LiveAssetId, opt => opt.MapFrom(src => src.LiveAssetId));

            cfg.CreateMap<AssetEvent, KalturaAssetEvent>()
                .IncludeBase<CoreObject, KalturaOTTObject>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.ExternalId));

            cfg.CreateMap<EpgAssetEvent, KalturaProgramAssetEvent>()
                .IncludeBase<AssetEvent, KalturaAssetEvent>()
                .ForMember(dest => dest.LiveAssetId, opt => opt.MapFrom(src => src.LiveAssetId));

            #region Engagement Adapter

            cfg.CreateMap<KalturaEngagementAdapter, EngagementAdapter>()
               .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
               .ForMember(dest => dest.ProviderUrl, opt => opt.MapFrom(src => src.ProviderUrl))
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
               .ForMember(dest => dest.SkipSettings, opt => opt.MapFrom(src => src.Settings == null))
               .ForMember(dest => dest.Settings, opt => opt.ResolveUsing(src => ConvertEngagementAdapterSettings(src.Settings)))
               ;

            cfg.CreateMap<EngagementAdapter, KalturaEngagementAdapter>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
              .ForMember(dest => dest.ProviderUrl, opt => opt.MapFrom(src => src.ProviderUrl))
              .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
              .ForMember(dest => dest.Settings, opt => opt.ResolveUsing(src => ConvertEngagementAdapterSettings(src.Settings)))
              ;

            cfg.CreateMap<EngagementAdapterBase, KalturaEngagementAdapterBase>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            cfg.CreateMap<EngagementAdapterResponse, KalturaEngagementAdapter>()
             .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.EngagementAdapter.AdapterUrl))
             .ForMember(dest => dest.ProviderUrl, opt => opt.MapFrom(src => src.EngagementAdapter.ProviderUrl))
             .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.EngagementAdapter.ID))
             .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.EngagementAdapter.IsActive))
             .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.EngagementAdapter.Name))
             .ForMember(dest => dest.SharedSecret, opt => opt.MapFrom(src => src.EngagementAdapter.SharedSecret))
             .ForMember(dest => dest.Settings, opt => opt.ResolveUsing(src => ConvertEngagementAdapterSettings(src.EngagementAdapter.Settings)));

            #endregion

            #region Engagement

            cfg.CreateMap<KalturaEngagement, Engagement>()
               .ForMember(dest => dest.AdapterDynamicData, opt => opt.MapFrom(src => src.AdapterDynamicData))
               .ForMember(dest => dest.AdapterId, opt => opt.MapFrom(src => src.AdapterId))
               .ForMember(dest => dest.EngagementType, opt => opt.ResolveUsing(src => ConvertEngagementType(src.Type)))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.IntervalSeconds, opt => opt.MapFrom(src => src.IntervalSeconds))
               .ForMember(dest => dest.SendTime, opt => opt.ResolveUsing(src => DateUtils.UtcUnixTimestampSecondsToDateTime(src.SendTimeInSeconds)))
               .ForMember(dest => dest.TotalNumberOfRecipients, opt => opt.MapFrom(src => src.TotalNumberOfRecipients))
               .ForMember(dest => dest.UserList, opt => opt.MapFrom(src => src.UserList))
               .ForMember(dest => dest.CouponGroupId, opt => opt.MapFrom(src => src.CouponGroupId))
               ;

            cfg.CreateMap<Engagement, KalturaEngagement>()
               .ForMember(dest => dest.AdapterDynamicData, opt => opt.MapFrom(src => src.AdapterDynamicData))
               .ForMember(dest => dest.AdapterId, opt => opt.MapFrom(src => src.AdapterId))
               .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => ConvertEngagementType(src.EngagementType)))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.IntervalSeconds, opt => opt.MapFrom(src => src.IntervalSeconds))
               .ForMember(dest => dest.SendTimeInSeconds, opt => opt.ResolveUsing(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.SendTime)))
               .ForMember(dest => dest.TotalNumberOfRecipients, opt => opt.MapFrom(src => src.TotalNumberOfRecipients))
               .ForMember(dest => dest.UserList, opt => opt.MapFrom(src => src.UserList))
               .ForMember(dest => dest.CouponGroupId, opt => opt.MapFrom(src => src.CouponGroupId))
               ;

            #endregion

            #region Topic Notification
            cfg.CreateMap<KalturaTopicNotification, TopicNotification>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.SubscribeReference, opt => opt.MapFrom(src => src.SubscribeReference));

            cfg.CreateMap<TopicNotification, KalturaTopicNotification>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.SubscribeReference, opt => opt.MapFrom(src => src.SubscribeReference));

            cfg.CreateMap<SubscribeReference, KalturaSubscribeReference>();

            cfg.CreateMap<SubscriptionSubscribeReference, KalturaSubscriptionSubscribeReference>()
               .IncludeBase<SubscribeReference, KalturaSubscribeReference>()
               .ForMember(dest => dest.SubscriptionId, opt => opt.MapFrom(src => src.SubscriptionId));

            cfg.CreateMap<KalturaSubscribeReference, SubscribeReference>();

            cfg.CreateMap<KalturaSubscriptionSubscribeReference, SubscriptionSubscribeReference>()
               .IncludeBase<KalturaSubscribeReference, SubscribeReference>()
               .ForMember(dest => dest.SubscriptionId, opt => opt.MapFrom(src => src.SubscriptionId));

            cfg.CreateMap<KalturaTrigger, TopicNotificationTrigger>();

            cfg.CreateMap<KalturaDateTrigger, TopicNotificationDateTrigger>()
               .IncludeBase<KalturaTrigger, TopicNotificationTrigger>()
               .ForMember(dest => dest.Date, opt => opt.MapFrom(src => DateUtils.UtcUnixTimestampSecondsToDateTime(src.Date)))
               .ForMember(dest => dest.Type, opt => opt.MapFrom(src => TopicNotificationTriggerType.Date))
                ;

            cfg.CreateMap<TopicNotificationTrigger, KalturaTrigger>();

            cfg.CreateMap<TopicNotificationDateTrigger, KalturaDateTrigger>()
               .IncludeBase<TopicNotificationTrigger, KalturaTrigger>()
                 .ForMember(dest => dest.Date, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.Date)));

            cfg.CreateMap<KalturaSubscriptionTrigger, TopicNotificationSubscriptionTrigger>()
               .IncludeBase<KalturaTrigger, TopicNotificationTrigger>()
               .ForMember(dest => dest.Offset, opt => opt.MapFrom(src => src.Offset))
               .ForMember(dest => dest.TriggerType, opt => opt.MapFrom(src => ConvertSubscriptionTriggerType(src.Type)))
               .ForMember(dest => dest.Type, opt => opt.MapFrom(src => TopicNotificationTriggerType.Subscription))
               ;

            cfg.CreateMap<TopicNotificationSubscriptionTrigger, KalturaSubscriptionTrigger>()
              .IncludeBase<TopicNotificationTrigger, KalturaTrigger>()
                .ForMember(dest => dest.Offset, opt => opt.MapFrom(src => src.Offset))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ConvertSubscriptionTriggerType(src.TriggerType)))
                ;

            cfg.CreateMap<KalturaDispatcher, TopicNotificationDispatcher>();

            cfg.CreateMap<TopicNotificationDispatcher, KalturaDispatcher>();

            cfg.CreateMap<KalturaSmsDispatcher, TopicNotificationSmsDispatcher>()
              .IncludeBase<KalturaDispatcher, TopicNotificationDispatcher>();

            cfg.CreateMap<TopicNotificationSmsDispatcher, KalturaSmsDispatcher>()
              .IncludeBase<TopicNotificationDispatcher, KalturaDispatcher>();

            cfg.CreateMap<KalturaMailDispatcher, TopicNotificationMailDispatcher>()
              .IncludeBase<KalturaDispatcher, TopicNotificationDispatcher>()
                .ForMember(dest => dest.SubjectTemplate, opt => opt.MapFrom(src => src.SubjectTemplate))
                .ForMember(dest => dest.BodyTemplate, opt => opt.MapFrom(src => src.BodyTemplate));

            cfg.CreateMap<TopicNotificationMailDispatcher, KalturaMailDispatcher>()
             .IncludeBase<TopicNotificationDispatcher, KalturaDispatcher>()
               .ForMember(dest => dest.SubjectTemplate, opt => opt.MapFrom(src => src.SubjectTemplate))
               .ForMember(dest => dest.BodyTemplate, opt => opt.MapFrom(src => src.BodyTemplate));

            cfg.CreateMap<TopicNotificationMessage, KalturaTopicNotificationMessage>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Dispatchers, opt => opt.MapFrom(src => src.Dispatchers))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                .ForMember(dest => dest.TopicNotificationId, opt => opt.MapFrom(src => src.TopicNotificationId))
                .ForMember(dest => dest.Trigger, opt => opt.MapFrom(src => src.Trigger))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
                .ForMember(dest => dest.Status, opt => opt.ResolveUsing(src => ConvertTopicNotificationStatus(src.Status)));

            cfg.CreateMap<KalturaTopicNotificationMessage, TopicNotificationMessage>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Dispatchers, opt => opt.MapFrom(src => src.Dispatchers))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                .ForMember(dest => dest.TopicNotificationId, opt => opt.MapFrom(src => src.TopicNotificationId))
                .ForMember(dest => dest.Trigger, opt => opt.MapFrom(src => src.Trigger))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl));

            #endregion

            #region PushMessage

            cfg.CreateMap<KalturaPushMessage, PushMessage>()
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.Action))
                .ForMember(dest => dest.Sound, opt => opt.MapFrom(src => src.Sound))
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url))
                .ForMember(dest => dest.Udid, opt => opt.MapFrom(src => src.Udid))
                .ForMember(dest => dest.PushChannels, opt => opt.MapFrom(src => ConvertPushChannels(src.PushChannels)));

            #endregion
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

        private static List<SmsAdapterParam> ConvertSmsAdapterSettings(KalturaSmsAdapterProfile src)
        {
            if (src.Settings == null) { return new List<SmsAdapterParam>(); }

            var settingsList = src.Settings.Select(s => new SmsAdapterParam
            {
                AdapterId = (int)src.Id,
                Key = s.Key,
                Value = s.Value != null ? s.Value.value : null,
            });

            return settingsList.ToList();
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
                case KalturaInboxMessageType.Interest:
                    result = eMessageCategory.Interest;
                    break;
                case KalturaInboxMessageType.Campaign:
                    result = eMessageCategory.Campaign;
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
                case eMessageCategory.Interest:
                    result = KalturaInboxMessageType.Interest;
                    break;
                case eMessageCategory.Campaign:
                    result = KalturaInboxMessageType.Campaign;
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
                case MessageTemplateType.SeriesReminder:
                    result = KalturaMessageTemplateType.SeriesReminder;
                    break;
                case MessageTemplateType.InterestEPG:
                    result = KalturaMessageTemplateType.InterestEPG;
                    break;
                case MessageTemplateType.InterestVod:
                    result = KalturaMessageTemplateType.InterestVod;
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
                case KalturaMessageTemplateType.SeriesReminder:
                    result = MessageTemplateType.SeriesReminder;
                    break;
                case KalturaMessageTemplateType.InterestVod:
                    result = MessageTemplateType.InterestVod;
                    break;
                case KalturaMessageTemplateType.InterestEPG:
                    result = MessageTemplateType.InterestEPG;
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
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC;
                    break;
                case Models.Catalog.KalturaOrder.z_to_a:
                    result.m_eOrderBy = OrderBy.NAME;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case Models.Catalog.KalturaOrder.views:
                    result.m_eOrderBy = OrderBy.VIEWS;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case Models.Catalog.KalturaOrder.ratings:
                    result.m_eOrderBy = OrderBy.RATING;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case Models.Catalog.KalturaOrder.votes:
                    result.m_eOrderBy = OrderBy.VOTES_COUNT;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case Models.Catalog.KalturaOrder.newest:
                    result.m_eOrderBy = OrderBy.START_DATE;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case Models.Catalog.KalturaOrder.relevancy:
                    result.m_eOrderBy = OrderBy.RELATED;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case Models.Catalog.KalturaOrder.oldest_first:
                    result.m_eOrderBy = OrderBy.START_DATE;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC;
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
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC;
                    break;
                case KalturaPersonalFeedOrderBy.NAME_DESC:
                    result.m_eOrderBy = OrderBy.NAME;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaPersonalFeedOrderBy.VIEWS_DESC:
                    result.m_eOrderBy = OrderBy.VIEWS;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaPersonalFeedOrderBy.RATINGS_DESC:
                    result.m_eOrderBy = OrderBy.RATING;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaPersonalFeedOrderBy.VOTES_DESC:
                    result.m_eOrderBy = OrderBy.VOTES_COUNT;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaPersonalFeedOrderBy.START_DATE_DESC:
                    result.m_eOrderBy = OrderBy.START_DATE;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaPersonalFeedOrderBy.RELEVANCY_DESC:
                    result.m_eOrderBy = OrderBy.RELATED;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaPersonalFeedOrderBy.START_DATE_ASC:
                    result.m_eOrderBy = OrderBy.START_DATE;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC;
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
                case KalturaInboxMessageType.Interest:
                    messageCategory = eMessageCategory.Interest;
                    break;
                case KalturaInboxMessageType.Campaign:
                    messageCategory = eMessageCategory.Campaign;
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
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC;
                    break;
                case KalturaAssetOrderBy.NAME_DESC:
                    result.m_eOrderBy = OrderBy.NAME;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.VIEWS_DESC:
                    result.m_eOrderBy = OrderBy.VIEWS;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.RATINGS_DESC:
                    result.m_eOrderBy = OrderBy.RATING;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.VOTES_DESC:
                    result.m_eOrderBy = OrderBy.VOTES_COUNT;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.START_DATE_DESC:
                    result.m_eOrderBy = OrderBy.START_DATE;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.RELEVANCY_DESC:
                    result.m_eOrderBy = OrderBy.RELATED;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.START_DATE_ASC:
                    result.m_eOrderBy = OrderBy.START_DATE;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC;
                    break;
                case KalturaAssetOrderBy.CREATE_DATE_ASC:
                    result.m_eOrderBy = OrderBy.CREATE_DATE;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC;
                    break;
                case KalturaAssetOrderBy.CREATE_DATE_DESC:
                    result.m_eOrderBy = OrderBy.CREATE_DATE;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.LIKES_DESC:
                    result.m_eOrderBy = OrderBy.LIKE_COUNTER;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
            }
            return result;
        }

        internal static OrderObj ConvertOrderToOrderObj(KalturaAssetReminderOrderBy orderBy)
        {
            OrderObj result = new OrderObj();

            switch (orderBy)
            {
                case KalturaAssetReminderOrderBy.NAME_ASC:
                    result.m_eOrderBy = OrderBy.NAME;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC;
                    break;
                case KalturaAssetReminderOrderBy.NAME_DESC:
                    result.m_eOrderBy = OrderBy.NAME;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaAssetReminderOrderBy.VIEWS_DESC:
                    result.m_eOrderBy = OrderBy.VIEWS;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaAssetReminderOrderBy.RATINGS_DESC:
                    result.m_eOrderBy = OrderBy.RATING;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaAssetReminderOrderBy.VOTES_DESC:
                    result.m_eOrderBy = OrderBy.VOTES_COUNT;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaAssetReminderOrderBy.START_DATE_DESC:
                    result.m_eOrderBy = OrderBy.START_DATE;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaAssetReminderOrderBy.RELEVANCY_DESC:
                    result.m_eOrderBy = OrderBy.RELATED;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaAssetReminderOrderBy.START_DATE_ASC:
                    result.m_eOrderBy = OrderBy.START_DATE;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC;
                    break;
                case KalturaAssetReminderOrderBy.LIKES_DESC:
                    result.m_eOrderBy = OrderBy.LIKE_COUNTER;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
            }
            return result;
        }

        internal static ReminderType ConvertReminderType(KalturaReminderType reminderType)
        {
            ReminderType result = ReminderType.Single;

            switch (reminderType)
            {
                case KalturaReminderType.ASSET:
                    result = ReminderType.Single;
                    break;
                case KalturaReminderType.SERIES:
                    result = ReminderType.Series;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown reminder type");
                    break;
            }

            return result;
        }

        internal static DynamicMailRequest ConvertEmailMessage(KalturaEmailMessage emailMessage)
        {
            DynamicMailRequest result = new DynamicMailRequest();

            result.m_sTemplateName = emailMessage.TemplateName;
            result.m_sSubject = emailMessage.Subject;
            result.m_sFirstName = emailMessage.FirstName;
            result.m_sLastName = emailMessage.LastName;
            result.m_sSenderName = emailMessage.SenderName;
            result.m_sSenderFrom = emailMessage.SenderFrom;
            result.m_sSenderTo = emailMessage.SenderTo;
            result.m_sBCCAddress = emailMessage.BccAddress;
            if (emailMessage.ExtraParameters != null && emailMessage.ExtraParameters.Count > 0)
            {
                result.values = new List<KeyValuePair>();
                foreach (KalturaKeyValue kkv in emailMessage.ExtraParameters)
                {
                    result.values.Add(new KeyValuePair(kkv.key, kkv.value));
                }
            }
            return result;
        }

        private static KalturaSubscriptionTriggerType ConvertSubscriptionTriggerType(TopicNotificationSubscriptionTriggerType triggerType)
        {
            KalturaSubscriptionTriggerType result = KalturaSubscriptionTriggerType.START_DATE;

            switch (triggerType)
            {
                case TopicNotificationSubscriptionTriggerType.StartDate:
                    result = KalturaSubscriptionTriggerType.START_DATE;
                    break;
                case TopicNotificationSubscriptionTriggerType.EndDate:
                    result = KalturaSubscriptionTriggerType.END_DATE;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown subscription trigger type");
                    break;
            }

            return result;
        }

        private static TopicNotificationSubscriptionTriggerType ConvertSubscriptionTriggerType(KalturaSubscriptionTriggerType triggerType)
        {
            TopicNotificationSubscriptionTriggerType result = TopicNotificationSubscriptionTriggerType.StartDate;

            switch (triggerType)
            {
                case KalturaSubscriptionTriggerType.START_DATE:
                    result = TopicNotificationSubscriptionTriggerType.StartDate;
                    break;
                case KalturaSubscriptionTriggerType.END_DATE:
                    result = TopicNotificationSubscriptionTriggerType.EndDate;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown subscription trigger type");
                    break;
            }

            return result;
        }

        public static KalturaTopicNotificationMessageStatus ConvertTopicNotificationStatus(TopicNotificationMessageStatus status)
        {
            KalturaTopicNotificationMessageStatus result;
            switch (status)
            {
                case TopicNotificationMessageStatus.Pending:
                    result = KalturaTopicNotificationMessageStatus.PENDING;
                    break;
                case TopicNotificationMessageStatus.Sent:
                    result = KalturaTopicNotificationMessageStatus.SENT;
                    break;

                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown status Type");
            }

            return result;
        }

        private static List<PushChannel> ConvertPushChannels(string pushChannels)
        {
            List<PushChannel> pushChannelList = null;

            if (!string.IsNullOrEmpty(pushChannels))
            {
                pushChannelList = new List<PushChannel>();
                foreach (string pushChannel in pushChannels.Split(','))
                {
                    KalturaPushChannel pushChannelType;
                    if (Enum.TryParse<KalturaPushChannel>(pushChannel.ToUpper(), out pushChannelType))
                    {
                        switch (pushChannelType)
                        {
                            case KalturaPushChannel.PUSH:
                                pushChannelList.Add(PushChannel.Push);
                                break;
                            case KalturaPushChannel.IOT:
                                pushChannelList.Add(PushChannel.Iot);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            return pushChannelList;
        }
    }
}