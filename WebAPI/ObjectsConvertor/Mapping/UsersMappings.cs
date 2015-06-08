using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models;
using WebAPI.Utils;
using WebAPI.Models.Users;
using WebAPI.Models.General;
using WebAPI.Exceptions;

namespace WebAPI.Mapping.ObjectsConvertor
{
    public class UsersMappings
    {
        public static void RegisterMappings()
        {
            //User 
            Mapper.CreateMap<Users.UserResponseObject, ClientUser>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => SerializationUtils.MaskSensitiveObject(src.m_user.m_sSiteGUID)))
                .ForMember(dest => dest.DomainID, opt => opt.MapFrom(src => SerializationUtils.MaskSensitiveObject(src.m_user.m_domianID.ToString())))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_sFirstName));

            // PinCode
            Mapper.CreateMap<Users.PinCodeResponse, LoginPin>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.siteGuid))
                .ForMember(dest => dest.PinCode, opt => opt.MapFrom(src => src.pinCode))
                .ForMember(dest => dest.ExpirationTime, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.expiredDate)));

            // UserType
            Mapper.CreateMap<Users.UserType, UserType>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            // Country
            Mapper.CreateMap<Users.Country, Country>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nObjecrtID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sCountryName))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.m_sCountryCode));

            // UserBasicData
            Mapper.CreateMap<Users.UserBasicData, UserBasicData>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.m_sAddress))
                .ForMember(dest => dest.AffiliateCode, opt => opt.MapFrom(src => src.m_sAffiliateCode))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.m_sCity))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.m_Country))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.m_sEmail))
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.m_CoGuid))
                .ForMember(dest => dest.FacebookId, opt => opt.MapFrom(src => src.m_sFacebookID))
                .ForMember(dest => dest.FacebookImage, opt => opt.MapFrom(src => src.m_bIsFacebookImagePermitted ? src.m_sFacebookImage : null))
                .ForMember(dest => dest.FacebookToken, opt => opt.MapFrom(src => src.m_sFacebookToken))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.m_sFirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.m_sLastName))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.m_sPhone))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.m_sUserName))
                .ForMember(dest => dest.UserType, opt => opt.MapFrom(src => src.m_UserType))
                .ForMember(dest => dest.Zip, opt => opt.MapFrom(src => src.m_sZip));

            // User
            Mapper.CreateMap<Users.User, User>()
                .ForMember(dest => dest.BasicDate, opt => opt.MapFrom(src => src.m_oBasicData))
                .ForMember(dest => dest.DomainId, opt => opt.MapFrom(src => src.m_domianID))
                .ForMember(dest => dest.DynamicDate, opt => opt.MapFrom(src => ConvertDynamicData(src.m_oDynamicData)))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_sSiteGUID))
                .ForMember(dest => dest.SuspentionState, opt => opt.MapFrom(src => src.m_eSuspendState))
                .ForMember(dest => dest.IsDomainMaster, opt => opt.MapFrom(src => src.m_isDomainMaster));


            //DomainSuspentionStatus to DomainSuspentionState
            Mapper.CreateMap<WebAPI.Users.DomainSuspentionStatus, DomainSuspentionState>().ConstructUsing((WebAPI.Users.DomainSuspentionStatus type) =>
            {
                DomainSuspentionState result;
                switch (type)
                {
                    case WebAPI.Users.DomainSuspentionStatus.OK:
                        result = DomainSuspentionState.not_suspended;
                        break;
                    case WebAPI.Users.DomainSuspentionStatus.Suspended:
                        result = DomainSuspentionState.suspended;
                        break;
                    default:
                        throw new ClientException((int)StatusCode.Error, "Unknown domain suspention state");
                }
                return result;
            });
        }

        private static Dictionary<string,string> ConvertDynamicData(Users.UserDynamicData userDynamicData)
        {
            Dictionary<string,string> result = null;

            if (userDynamicData != null && userDynamicData.m_sUserData != null)
            {
                result = new Dictionary<string, string>();
                foreach (var data in userDynamicData.m_sUserData)
                {
                    if (!string.IsNullOrEmpty(data.m_sDataType))
                    {
                        result.Add(data.m_sDataType, data.m_sValue);
                    }
                }
            }

            return result;
        }

    }
}