using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
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
                 .ForMember(dest => dest.PushNotificationEnabled, opt => opt.MapFrom(src => src.push_notification_enabled))
                 .ForMember(dest => dest.PushSystemAnnouncementsEnabled, opt => opt.MapFrom(src => src.push_system_announcements_enabled));

            //KalturaPartnerNotificationSettings TO NotificationPartnerSettings
            Mapper.CreateMap<KalturaPartnerNotificationSettings, NotificationPartnerSettings>()
                 .ForMember(dest => dest.push_notification_enabled, opt => opt.MapFrom(src => src.PushNotificationEnabled))
                 .ForMember(dest => dest.push_system_announcements_enabled, opt => opt.MapFrom(src => src.PushSystemAnnouncementsEnabled));

            Mapper.CreateMap<NotificationSettings, KalturaNotificationSettings>()
                 .ForMember(dest => dest.PushNotificationEnabled, opt => opt.MapFrom(src => src.push_notification_enabled));

            Mapper.CreateMap<KalturaNotificationSettings, NotificationSettings>()
                 .ForMember(dest => dest.push_notification_enabled, opt => opt.MapFrom(src => src.PushNotificationEnabled));

            Mapper.CreateMap<MessageAnnouncement, KalturaAnnouncement>()
                 .ForMember(dest => dest.Enabled, opt => opt.MapFrom(src => src.Enabled))
                 .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.MessageAnnouncementId))
                 .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                 .ForMember(dest => dest.Recipients, opt => opt.MapFrom(src => ConvertRecipientsType(src.Recipients)))
                 .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
                 .ForMember(dest => dest.Timezone, opt => opt.MapFrom(src => src.Timezone))
                 .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ConvertAnnouncementStatusType(src.Status)));
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
    }
}