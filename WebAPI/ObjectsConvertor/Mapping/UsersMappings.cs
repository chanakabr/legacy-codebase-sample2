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
using WebAPI.Models.Catalog;
using WebAPI.Managers.Models;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class UsersMappings
    {
        public static void RegisterMappings()
        {
            //User 
            Mapper.CreateMap<Users.UserResponseObject, KalturaClientUser>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => SerializationUtils.MaskSensitiveObject(src.m_user.m_sSiteGUID)))
                .ForMember(dest => dest.HouseholdID, opt => opt.MapFrom(src => SerializationUtils.MaskSensitiveObject(src.m_user.m_domianID.ToString())))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_sFirstName));

            // PinCode
            Mapper.CreateMap<Users.PinCodeResponse, KalturaLoginPin>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.siteGuid))
                .ForMember(dest => dest.PinCode, opt => opt.MapFrom(src => src.pinCode))
                .ForMember(dest => dest.ExpirationTime, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.expiredDate)));

            // UserType
            Mapper.CreateMap<Users.UserType, KalturaUserType>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            // Country
            Mapper.CreateMap<Users.Country, KalturaCountry>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nObjecrtID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sCountryName))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.m_sCountryCode));

            // UserBasicData
            Mapper.CreateMap<Users.UserBasicData, KalturaUserBasicData>()
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
            Mapper.CreateMap<Users.UserResponseObject, KalturaUser>()
                .ForMember(dest => dest.BasicData, opt => opt.MapFrom(src => src.m_user.m_oBasicData))
                .ForMember(dest => dest.HouseholdID, opt => opt.MapFrom(src => src.m_user.m_domianID))
                .ForMember(dest => dest.DynamicData, opt => opt.MapFrom(src => ConvertDynamicData(src.m_user.m_oDynamicData)))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_user.m_sSiteGUID))
                .ForMember(dest => dest.SuspentionState, opt => opt.MapFrom(src => ConvertDomainSuspentionStatus(src.m_user.m_eSuspendState)))
                .ForMember(dest => dest.IsHouseholdMaster, opt => opt.MapFrom(src => src.m_user.m_isDomainMaster))
                .ForMember(dest => dest.UserState, opt => opt.MapFrom(src => ConvertResponseStatusToUserState(src.m_RespStatus)));

            // SlimUser
            Mapper.CreateMap<KalturaUser, KalturaSlimUser>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.BasicData.Username))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.BasicData.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.BasicData.LastName));

            // UserId to SlimUser
            Mapper.CreateMap<int, KalturaSlimUser>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src));

            // Rest UserBasicData ==> WS_Users UserBasicData
            Mapper.CreateMap<KalturaUserBasicData, Users.UserBasicData>()
                .ForMember(dest => dest.m_sAddress, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.m_sAffiliateCode, opt => opt.MapFrom(src => src.AffiliateCode))
                .ForMember(dest => dest.m_sCity, opt => opt.MapFrom(src => src.City))
                .ForMember(dest => dest.m_Country, opt => opt.MapFrom(src => src.Country))
                .ForMember(dest => dest.m_sEmail, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.m_CoGuid, opt => opt.MapFrom(src => src.ExternalId))
                .ForMember(dest => dest.m_sFacebookID, opt => opt.MapFrom(src => src.FacebookId))
                .ForMember(dest => dest.m_sFacebookImage, opt => opt.MapFrom(src => src.FacebookImage))
                .ForMember(dest => dest.m_sFacebookToken, opt => opt.MapFrom(src => src.FacebookToken))
                .ForMember(dest => dest.m_sFirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.m_sLastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.m_sPhone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.m_sUserName, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.m_UserType, opt => opt.MapFrom(src => src.UserType))
                .ForMember(dest => dest.m_sZip, opt => opt.MapFrom(src => src.Zip));

            // Country
            Mapper.CreateMap<KalturaCountry, Users.Country>()
                .ForMember(dest => dest.m_nObjecrtID, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.m_sCountryName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.m_sCountryCode, opt => opt.MapFrom(src => src.Code));

            // UserType
            Mapper.CreateMap<KalturaUserType, Users.UserType>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            Mapper.CreateMap<Dictionary<string, string>, Users.UserDynamicData>()
                .ForMember(dest => dest.m_sUserData, opt => opt.MapFrom(src => ConvertDynamicData(src)));


            // MediaId to AssetInfo
            Mapper.CreateMap<string, KalturaAssetInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => ConvertToLong(src)))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => 0));

            // Rest WS_Users FavoritObject ==>  Favorite  
            Mapper.CreateMap<Users.FavoritObject, KalturaFavorite>()
                .ForMember(dest => dest.ExtraData, opt => opt.MapFrom(src => src.m_sExtraData))
                .ForMember(dest => dest.Asset, opt => opt.MapFrom(src => src.m_sItemCode));

        }

        private static long ConvertToLong(string src)
        {
            long output = 0;
            long.TryParse(src, out output);
            return output;


        }

        private static KalturaUserState ConvertResponseStatusToUserState(WebAPI.Users.ResponseStatus type)
        {
            KalturaUserState result;
            switch (type)
            {
                case WebAPI.Users.ResponseStatus.OK:
                    result = KalturaUserState.ok;
                    break;
                case WebAPI.Users.ResponseStatus.UserWithNoDomain:
                    result = KalturaUserState.user_with_no_household;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown user state");
            }
            return result;
        }

        private static KalturaHouseholdSuspentionState ConvertDomainSuspentionStatus(WebAPI.Users.DomainSuspentionStatus type)
        {
            KalturaHouseholdSuspentionState result;
            switch (type)
            {
                case WebAPI.Users.DomainSuspentionStatus.OK:
                    result = KalturaHouseholdSuspentionState.not_suspended;
                    break;
                case WebAPI.Users.DomainSuspentionStatus.Suspended:
                    result = KalturaHouseholdSuspentionState.suspended;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown domain suspention state");
            }
            return result;
        }

        private static Users.UserDynamicData ConvertDynamicData(Dictionary<string, string> userDynamicData)
        {
            Users.UserDynamicData result = null;

            if (userDynamicData != null && userDynamicData.Count > 0)
            {
                result = new Users.UserDynamicData();
                List<Users.UserDynamicDataContainer> udc = new List<Users.UserDynamicDataContainer>();
                Users.UserDynamicDataContainer ud;
                foreach (KeyValuePair<string, string> data in userDynamicData)
                {
                    if (!string.IsNullOrEmpty(data.Key))
                    {
                        ud = new Users.UserDynamicDataContainer();
                        ud.m_sDataType = data.Key;
                        ud.m_sValue = data.Value;
                        udc.Add(ud);
                    }
                }
                if (udc != null && udc.Count > 0)
                {
                    result.m_sUserData = udc.ToArray();
                }
            }

            return result;
        }

        private static Dictionary<string, string> ConvertDynamicData(Users.UserDynamicData userDynamicData)
        {
            Dictionary<string, string> result = null;

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