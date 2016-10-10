using ApiObjects.Response;
using AutoMapper;
using Newtonsoft.Json;
using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.DMS;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class DMSMapping
    {
        public static void RegisterMappings()
        {
            // from dms to local
            Mapper.CreateMap<DMSGroupConfiguration, KalturaConfigurationGroup>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.NumberOfDevices, opt => opt.MapFrom(src => src.NumberOfDevices))
                .ForMember(dest => dest.PartnerId, opt => opt.MapFrom(src => src.PartnerId))
                .ForMember(dest => dest.ConfigFiles, opt => opt.MapFrom(src => src.ConfigFileIds))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags));

            // from local to dms  
            Mapper.CreateMap<KalturaConfigurationGroup, DMSGroupConfiguration>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.NumberOfDevices, opt => opt.MapFrom(src => src.NumberOfDevices))
                .ForMember(dest => dest.PartnerId, opt => opt.MapFrom(src => src.PartnerId))
                .ForMember(dest => dest.ConfigFileIds, opt => opt.MapFrom(src => src.ConfigFiles))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags));

            // from DMSConfigurationMin to KalturaConfigurationMin
            Mapper.CreateMap<DMSConfigurationMin, KalturaConfigurationMin>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            // from local to dms  
            Mapper.CreateMap<KalturaConfigurationMin, DMSConfigurationMin>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            // from dms to local
            Mapper.CreateMap<DMSTagMapping, KalturaConfigurationGroupTag>()
                .ForMember(dest => dest.ConfigurationGroupId, opt => opt.MapFrom(src => src.GroupId))
                .ForMember(dest => dest.PartnerId, opt => opt.MapFrom(src => src.PartnerId))
                .ForMember(dest => dest.Tag, opt => opt.MapFrom(src => src.Tag));

            // from local to dms  
            Mapper.CreateMap<KalturaConfigurationGroupTag, DMSTagMapping>()
                .ForMember(dest => dest.GroupId, opt => opt.MapFrom(src => src.ConfigurationGroupId))
                .ForMember(dest => dest.PartnerId, opt => opt.MapFrom(src => src.PartnerId))
                .ForMember(dest => dest.Tag, opt => opt.MapFrom(src => src.Tag));

            // from dms to local
            Mapper.CreateMap<DMSDeviceMapping, KalturaConfigurationGroupDevice>()
                .ForMember(dest => dest.ConfigurationGroupId, opt => opt.MapFrom(src => src.GroupId))
                .ForMember(dest => dest.PartnerId, opt => opt.MapFrom(src => src.PartnerId))
                .ForMember(dest => dest.Udid, opt => opt.MapFrom(src => src.Udid));

            // from dms to local
            Mapper.CreateMap<DMSDevice, KalturaReport>()
                .ForMember(dest => dest.ConfigurationGroupId, opt => opt.MapFrom(src => src.GroupConfigurationId))
                .ForMember(dest => dest.PartnerId, opt => opt.MapFrom(src => src.GroupId))
                .ForMember(dest => dest.LastAccessDate, opt => opt.MapFrom(src => src.LastAccessDate))
                .ForMember(dest => dest.LastAccessIP, opt => opt.MapFrom(src => src.LastAccessIP))
                .ForMember(dest => dest.OperationSystem, opt => opt.MapFrom(src => src.OperationSystem))
                .ForMember(dest => dest.PushParameters, opt => opt.MapFrom(src => src.PushParameters))
                .ForMember(dest => dest.Udid, opt => opt.MapFrom(src => src.Udid))
                .ForMember(dest => dest.UserAgent, opt => opt.MapFrom(src => src.UserAgent))
                .ForMember(dest => dest.VersionAppName, opt => opt.MapFrom(src => src.VersionAppName))
                .ForMember(dest => dest.VersionNumber, opt => opt.MapFrom(src => src.VersionNumber))
                .ForMember(dest => dest.VersionPlatform, opt => opt.MapFrom(src => ConvertPlatform(src.VersionPlatform)));

            // from dms to local
            Mapper.CreateMap<DMSPushParams, KalturaPushParams>()
                .ForMember(dest => dest.ExternalToken, opt => opt.MapFrom(src => src.ExternalToken))
                .ForMember(dest => dest.Token, opt => opt.MapFrom(src => src.Token));

            // from dms to local
            Mapper.CreateMap<DMSAppVersion, KalturaConfiguration>()
                .ForMember(dest => dest.AppName, opt => opt.MapFrom(src => src.AppName))
                .ForMember(dest => dest.ClientVersion, opt => opt.MapFrom(src => src.ClientVersion))
                .ForMember(dest => dest.IsForceUpdate, opt => opt.MapFrom(src => src.IsForceUpdate))
                .ForMember(dest => dest.Platform, opt => opt.MapFrom(src => ConvertPlatform(src.Platform)))
                .ForMember(dest => dest.PartnerId, opt => opt.MapFrom(src => src.GroupId))
                .ForMember(dest => dest.ExternalPushId, opt => opt.MapFrom(src => src.ExternalPushId))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => ConvertToContent(src.Params)))
                .ForMember(dest => dest.ConfigurationGroupId, opt => opt.MapFrom(src => src.GroupConfigurationId))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault));

            // from local to dms 
            Mapper.CreateMap<KalturaConfiguration, DMSAppVersion>()
                .ForMember(dest => dest.AppName, opt => opt.MapFrom(src => src.AppName))
                .ForMember(dest => dest.ClientVersion, opt => opt.MapFrom(src => src.ClientVersion))
                .ForMember(dest => dest.IsForceUpdate, opt => opt.MapFrom(src => src.IsForceUpdate))
                .ForMember(dest => dest.Platform, opt => opt.MapFrom(src => ConvertPlatform(src.Platform)))
                .ForMember(dest => dest.GroupId, opt => opt.MapFrom(src => src.PartnerId))
                .ForMember(dest => dest.ExternalPushId, opt => opt.MapFrom(src => src.ExternalPushId))
                .ForMember(dest => dest.Params, opt => opt.MapFrom(src => ConvertToParms(src.Content)))
                .ForMember(dest => dest.GroupConfigurationId, opt => opt.MapFrom(src => src.ConfigurationGroupId))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault));
        }

        private static Dictionary<string, object> ConvertToParms(string data)
        {
            return JsonConvert.DeserializeObject < Dictionary<string, object>>(data);
        }

        private static string ConvertToContent(Dictionary<string, object> dictionary)
        {
            return JsonConvert.SerializeObject(dictionary);
        }

        private static KalturaPlatform ConvertPlatform(KalturaPlatform kalturaePlatform)
        {
            switch (kalturaePlatform)
            {
                case KalturaPlatform.Android:
                    return KalturaPlatform.Android;
                case KalturaPlatform.iOS:
                    return KalturaPlatform.iOS;
                case KalturaPlatform.WindowsPhone:
                    return KalturaPlatform.WindowsPhone;
                case KalturaPlatform.Blackberry:
                    return KalturaPlatform.Blackberry;
                case KalturaPlatform.STB:
                    return KalturaPlatform.STB;
                case KalturaPlatform.CTV:
                    return KalturaPlatform.CTV;
                case KalturaPlatform.Other:
                    return KalturaPlatform.Other;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown platform owner");
            }
        }


        private static KalturaPlatform ConvertPlatform(DMSePlatform dMSePlatform)
        {
            switch (dMSePlatform)
            {
                case DMSePlatform.Android:
                    return KalturaPlatform.Android;
                case DMSePlatform.iOS:
                    return KalturaPlatform.iOS;
                case DMSePlatform.WindowsPhone:
                    return KalturaPlatform.WindowsPhone;
                case DMSePlatform.Blackberry:
                    return KalturaPlatform.Blackberry;
                case DMSePlatform.STB:
                    return KalturaPlatform.STB;
                case DMSePlatform.CTV:
                    return KalturaPlatform.CTV;
                case DMSePlatform.Other:
                    return KalturaPlatform.Other;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown platform owner");
            }
        }

        internal static eResponseStatus ConvertDMSStatus(DMSeResponseStatus dMSeResponseStatus)
        {
            switch (dMSeResponseStatus)
            {
                case DMSeResponseStatus.Unknown:
                case DMSeResponseStatus.Error:
                    return eResponseStatus.Error;
                case DMSeResponseStatus.OK:
                    return eResponseStatus.OK;
                case DMSeResponseStatus.Forbidden:
                    return eResponseStatus.Forbidden;
                case DMSeResponseStatus.IllegalQueryParams:
                    return eResponseStatus.IllegalQueryParams;
                case DMSeResponseStatus.IllegalPostData:
                    return eResponseStatus.IllegalPostData;
                case DMSeResponseStatus.NotExist:
                    return eResponseStatus.NotExist;
                case DMSeResponseStatus.PartnerMismatch:
                    return eResponseStatus.PartnerMismatch;
                case DMSeResponseStatus.AlreadyExist:
                    return eResponseStatus.ItemAlreadyExist;
                default:
                    return eResponseStatus.Error;
            }
        }

        internal static eResponseStatus ConvertDMSStatus(DMSeStatus dMSeStatus)
        {
            switch (dMSeStatus)
            {
                case DMSeStatus.Forbidden:
                    return eResponseStatus.Forbidden;
                case DMSeStatus.IllegalParams:
                    return eResponseStatus.IllegalQueryParams;
                case DMSeStatus.IllegalPostData:
                    return eResponseStatus.IllegalPostData;
                case DMSeStatus.Registered:
                case DMSeStatus.Success:
                    return eResponseStatus.OK;
                case DMSeStatus.VersionNotFound:
                    return eResponseStatus.VersionNotFound;
                case DMSeStatus.Unregistered:
                case DMSeStatus.Unknown:
                case DMSeStatus.Error:
                default:
                    return eResponseStatus.Error;

            }
        }
    }
}