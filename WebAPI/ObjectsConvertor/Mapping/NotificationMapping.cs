using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models.Notification;
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
        }
    }
}