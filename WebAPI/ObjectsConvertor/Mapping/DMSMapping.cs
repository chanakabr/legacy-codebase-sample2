using ApiObjects.Response;
using AutoMapper;
using Newtonsoft.Json;
using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.DMS;
using WebAPI.Models.General;
using AutoMapper.Configuration;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class DMSMapping
    {
        public static void RegisterMappings(MapperConfigurationExpression cfg)
        {
            // from dms to local
            cfg.CreateMap<DMSGroupConfiguration, KalturaConfigurationGroup>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id))
                .ForMember(dest => dest.IsDefault, opt => opt.ResolveUsing(src => src.IsDefault))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.Name))
                .ForMember(dest => dest.NumberOfDevices, opt => opt.ResolveUsing(src => src.NumberOfDevices))
                .ForMember(dest => dest.PartnerId, opt => opt.ResolveUsing(src => src.PartnerId))
                .ForMember(dest => dest.ConfigurationIdentifiers, opt => opt.ResolveUsing(src => src.ConfigFileIds))
                .ForMember(dest => dest.Tags, opt => opt.ResolveUsing(src => src.Tags));

            // from local to dms  
            cfg.CreateMap<KalturaConfigurationGroup, DMSGroupConfiguration>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id))
                .ForMember(dest => dest.IsDefault, opt => opt.ResolveUsing(src => src.IsDefault))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.Name))
                .ForMember(dest => dest.NumberOfDevices, opt => opt.ResolveUsing(src => src.NumberOfDevices))
                .ForMember(dest => dest.PartnerId, opt => opt.ResolveUsing(src => src.PartnerId))
                .ForMember(dest => dest.ConfigFileIds, opt => opt.ResolveUsing(src => src.ConfigurationIdentifiers))
                .ForMember(dest => dest.Tags, opt => opt.ResolveUsing(src => src.Tags));

            // from DMSConfigurationMin to KalturaConfigurationMin
            cfg.CreateMap<DMSConfigurationMin, KalturaConfigurationIdentifier>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.Name));

            // from local to dms  
            cfg.CreateMap<KalturaConfigurationIdentifier, DMSConfigurationMin>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.Name));

            // from dms to local
            cfg.CreateMap<DMSTagMapping, KalturaConfigurationGroupTag>()
                .ForMember(dest => dest.ConfigurationGroupId, opt => opt.ResolveUsing(src => src.GroupId))
                .ForMember(dest => dest.PartnerId, opt => opt.ResolveUsing(src => src.PartnerId))
                .ForMember(dest => dest.Tag, opt => opt.ResolveUsing(src => src.Tag));

            // from local to dms  
            cfg.CreateMap<KalturaConfigurationGroupTag, DMSTagMapping>()
                .ForMember(dest => dest.GroupId, opt => opt.ResolveUsing(src => src.ConfigurationGroupId))
                .ForMember(dest => dest.PartnerId, opt => opt.ResolveUsing(src => src.PartnerId))
                .ForMember(dest => dest.Tag, opt => opt.ResolveUsing(src => src.Tag));

            // from dms to local
            cfg.CreateMap<DMSDeviceMapping, KalturaConfigurationGroupDevice>()
                .ForMember(dest => dest.ConfigurationGroupId, opt => opt.ResolveUsing(src => src.GroupId))
                .ForMember(dest => dest.PartnerId, opt => opt.ResolveUsing(src => src.PartnerId))
                .ForMember(dest => dest.Udid, opt => opt.ResolveUsing(src => src.Udid));

            cfg.CreateMap<BaseReport, KalturaReport>()
                .Include<DMSDevice, KalturaDeviceReport>()
                ;

            // from dms to local
            cfg.CreateMap<DMSDevice, KalturaDeviceReport>()
                .ForMember(dest => dest.ConfigurationGroupId, opt => opt.ResolveUsing(src => src.GroupConfigurationId))
                .ForMember(dest => dest.PartnerId, opt => opt.ResolveUsing(src => src.GroupId))
                .ForMember(dest => dest.LastAccessDate, opt => opt.ResolveUsing(src => src.LastAccessDate))
                .ForMember(dest => dest.LastAccessIP, opt => opt.ResolveUsing(src => src.LastAccessIP))
                .ForMember(dest => dest.OperationSystem, opt => opt.ResolveUsing(src => src.OperationSystem))
                .ForMember(dest => dest.PushParameters, opt => opt.ResolveUsing(src => src.PushParameters))
                .ForMember(dest => dest.Udid, opt => opt.ResolveUsing(src => src.Udid))
                .ForMember(dest => dest.UserAgent, opt => opt.ResolveUsing(src => src.UserAgent))
                .ForMember(dest => dest.VersionAppName, opt => opt.ResolveUsing(src => src.VersionAppName))
                .ForMember(dest => dest.VersionNumber, opt => opt.ResolveUsing(src => src.VersionNumber))
                .ForMember(dest => dest.VersionPlatform, opt => opt.ResolveUsing(src => ConvertPlatform(src.VersionPlatform)));

            // from dms to local
            cfg.CreateMap<DMSPushParams, KalturaPushParams>()
                .ForMember(dest => dest.ExternalToken, opt => opt.ResolveUsing(src => src.ExternalToken))
                .ForMember(dest => dest.Token, opt => opt.ResolveUsing(src => src.Token));

            // from dms to local
            cfg.CreateMap<DMSAppVersion, KalturaConfigurations>()
                .ForMember(dest => dest.AppName, opt => opt.ResolveUsing(src => src.AppName))
                .ForMember(dest => dest.ClientVersion, opt => opt.ResolveUsing(src => src.ClientVersion))
                .ForMember(dest => dest.IsForceUpdate, opt => opt.ResolveUsing(src => src.IsForceUpdate))
                .ForMember(dest => dest.Platform, opt => opt.ResolveUsing(src => ConvertPlatform(src.Platform)))
                .ForMember(dest => dest.PartnerId, opt => opt.ResolveUsing(src => src.GroupId))
                .ForMember(dest => dest.ExternalPushId, opt => opt.ResolveUsing(src => src.ExternalPushId))
                .ForMember(dest => dest.Content, opt => opt.ResolveUsing(src => ConvertToContent(src.Params)))
                .ForMember(dest => dest.ConfigurationGroupId, opt => opt.ResolveUsing(src => src.GroupConfigurationId))
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id));

            // from local to dms 
            cfg.CreateMap<KalturaConfigurations, DMSAppVersion>()
                .ForMember(dest => dest.AppName, opt => opt.ResolveUsing(src => src.AppName))
                .ForMember(dest => dest.ClientVersion, opt => opt.ResolveUsing(src => src.ClientVersion))
                .ForMember(dest => dest.IsForceUpdate, opt => opt.ResolveUsing(src => src.IsForceUpdate))
                .ForMember(dest => dest.Platform, opt => opt.ResolveUsing(src => ConvertPlatform(src.Platform)))
                .ForMember(dest => dest.GroupId, opt => opt.ResolveUsing(src => src.PartnerId))
                .ForMember(dest => dest.ExternalPushId, opt => opt.ResolveUsing(src => src.ExternalPushId))
                .ForMember(dest => dest.Params, opt => opt.ResolveUsing(src => ConvertToParms(src.Content)))
                .ForMember(dest => dest.GroupConfigurationId, opt => opt.ResolveUsing(src => src.ConfigurationGroupId))
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id));
        }

        private static Dictionary<string, object> ConvertToParms(string data)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                StringEscapeHandling = StringEscapeHandling.EscapeHtml
            };

            return JsonConvert.DeserializeObject<Dictionary<string, object>>(data, settings);
        }

        private static string ConvertToContent(Dictionary<string, object> dictionary)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                StringEscapeHandling = StringEscapeHandling.EscapeHtml
            };

            return JsonConvert.SerializeObject(dictionary, settings);
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