using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using WebAPI.Models.Users;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Social;
using WebAPI.Models.General;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class SocialMappings
    {
        public static void RegisterMappings()
        {
            // FacebookResponse to ClientFacebookResponse
            Mapper.CreateMap<WebAPI.Social.FacebookResponseObject, WebAPI.Models.Social.KalturaFacebookSocial>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.fbUser.id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.fbUser.name))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.fbUser.first_name))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.fbUser.last_name))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.fbUser.email))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.fbUser.gender))
                .ForMember(dest => dest.Birthday, opt => opt.MapFrom(src => src.fbUser.Birthday))
                .ForMember(dest => dest.PictureUrl, opt => opt.MapFrom(src => src.pic))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.status))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.siteGuid));

            Mapper.CreateMap<WebAPI.Social.FacebookResponseObject, WebAPI.Models.Social.KalturaSocialResponse>()
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.data))
                .ForMember(dest => dest.SocialNetworkUsername, opt => opt.MapFrom(src => src.facebookName))
                .ForMember(dest => dest.SocialUser, opt => opt.MapFrom(src => src.fbUser))
                .ForMember(dest => dest.KalturaName, opt => opt.MapFrom(src => src.tvinciName))
                .ForMember(dest => dest.MinFriends, opt => opt.MapFrom(src => src.minFriends))
                .ForMember(dest => dest.Pic, opt => opt.MapFrom(src => src.pic))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.status))
                .ForMember(dest => dest.Token, opt => opt.MapFrom(src => src.token))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.siteGuid));

            // FBUser to FacebookUser
            Mapper.CreateMap<WebAPI.Social.FBUser, WebAPI.Models.Social.KalturaSocialUser>()
                .ForMember(dest => dest.Birthday, opt => opt.MapFrom(src => src.Birthday))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.email))
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.first_name))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.gender))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.last_name))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.name))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.m_sSiteGuid));

            // UserType
            Mapper.CreateMap<Social.UserType, KalturaOTTUserType>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            // Country
            Mapper.CreateMap<Social.Country, KalturaCountry>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nObjecrtID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sCountryName))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.m_sCountryCode));

            //// UserBasicData
            //Mapper.CreateMap<Social.UserBasicData, KalturaUserBasicData>()
            //    .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.m_sAddress))
            //    .ForMember(dest => dest.AffiliateCode, opt => opt.MapFrom(src => src.m_sAffiliateCode))
            //    .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.m_sCity))
            //    .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.m_Country))
            //    .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.m_sEmail))
            //    .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.m_CoGuid))
            //    .ForMember(dest => dest.FacebookId, opt => opt.MapFrom(src => src.m_sFacebookID))
            //    .ForMember(dest => dest.FacebookImage, opt => opt.MapFrom(src => src.m_bIsFacebookImagePermitted ? src.m_sFacebookImage : null))
            //    .ForMember(dest => dest.FacebookToken, opt => opt.MapFrom(src => src.m_sFacebookToken))
            //    .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.m_sFirstName))
            //    .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.m_sLastName))
            //    .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.m_sPhone))
            //    .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.m_sUserName))
            //    .ForMember(dest => dest.UserType, opt => opt.MapFrom(src => src.m_UserType))
            //    .ForMember(dest => dest.Zip, opt => opt.MapFrom(src => src.m_sZip));

            // User - TODO: Unifiy with UsersMapping
            Mapper.CreateMap<Social.UserResponseObject, KalturaOTTUser>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_sFirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_sLastName))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_sUserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_sEmail))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_sAddress))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_sCity))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_Country))
                .ForMember(dest => dest.Zip, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_sZip))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_sPhone))
                .ForMember(dest => dest.FacebookId, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_sFacebookID))
                .ForMember(dest => dest.FacebookImage, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_sFacebookImage))
                .ForMember(dest => dest.FacebookToken, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_sFacebookToken))
                .ForMember(dest => dest.AffiliateCode, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_sAffiliateCode))
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_ExternalToken))
                .ForMember(dest => dest.UserType, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_UserType))
                .ForMember(dest => dest.HouseholdID, opt => opt.MapFrom(src => src.m_user.m_domianID))
                .ForMember(dest => dest.DynamicData, opt => opt.MapFrom(src => ConvertDynamicData(src.m_user.m_oDynamicData)))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_user.m_sSiteGUID))
                .ForMember(dest => dest.SuspentionState, opt => opt.MapFrom(src => ConvertDomainSuspentionStatus(src.m_user.m_eSuspendState)))
                .ForMember(dest => dest.SuspensionState, opt => opt.MapFrom(src => ConvertDomainSuspentionStatus(src.m_user.m_eSuspendState)))
                .ForMember(dest => dest.IsHouseholdMaster, opt => opt.MapFrom(src => src.m_user.m_isDomainMaster))
                .ForMember(dest => dest.UserState, opt => opt.MapFrom(src => ConvertResponseStatusToUserState(src.m_RespStatus)));

            // FacebookConfig to KalturaFacebookConfig
            Mapper.CreateMap<Social.FacebookConfig, KalturaSocialConfig>()
                .ForMember(dest => dest.AppId, opt => opt.MapFrom(src => src.sFBKey))
                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.sFBPermissions));

            // SocialActivityDoc to KalturaSocialFriendActivity
            Mapper.CreateMap<Social.SocialActivityDoc, KalturaSocialFriendActivity>()
                .ForMember(dest => dest.ActionTime, opt => opt.MapFrom(src => src.LastUpdate))
                .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.ActivityObject.AssetID))
                .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => ConvertToKalturaAssetType(src.ActivityObject.AssetType)))
                .ForMember(dest => dest.SocialAction, opt => opt.MapFrom(src => src.ActivityVerb))
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.ActivitySubject.ActorTvinciUsername))
                .ForMember(dest => dest.UserPictureUrl, opt => opt.MapFrom(src => src.ActivitySubject.ActorPicUrl));

            // ActivityVerb to KalturaSocialAction
            Mapper.CreateMap<Social.SocialActivityVerb, KalturaSocialAction>().ConstructUsing(ConvertToKalturaSocialAction);
        }

        private static KalturaSocialAction ConvertToKalturaSocialAction(ResolutionContext context)
        {
            KalturaSocialAction result;
            var action = (Social.SocialActivityVerb)context.SourceValue;
            var actionType = ConvertToKalturaSocialActionType(action.ActionType);

            if (actionType == KalturaSocialActionType.RATE)
            {
                result = new KalturaSocialActionRate(action.RateValue);
            }
            else
            {
                result = new KalturaSocialAction()
                {
                    ActionType = actionType
                };
            }

            return result;
        }

        public static WebAPI.Social.KeyValuePair[] ConvertDictionaryToKeyValue(Dictionary<string, string> dictionary)
        {
            if (dictionary != null && dictionary.Count > 0)
            {
                WebAPI.Social.KeyValuePair[] keyValuePair = new Social.KeyValuePair[dictionary.Count];

                for (int i = 0; i < dictionary.Count; i++)
                {
                    keyValuePair[i] = new Social.KeyValuePair()
                    {
                        key = dictionary.ElementAt(i).Key,
                        value = dictionary.ElementAt(i).Value
                    };
                }

                return keyValuePair;
            }

            return null;
        }

        public static SerializableDictionary<string, KalturaStringValue> ConvertDynamicData(Social.UserDynamicData userDynamicData)
        {
            SerializableDictionary<string, KalturaStringValue> result = null;

            if (userDynamicData != null && userDynamicData.m_sUserData != null)
            {
                result = new SerializableDictionary<string, KalturaStringValue>();
                foreach (var data in userDynamicData.m_sUserData)
                {
                    if (!string.IsNullOrEmpty(data.m_sDataType))
                    {
                        result.Add(data.m_sDataType, new KalturaStringValue(){ value = data.m_sValue });
                    }
                }
            }

            return result;
        }

        private static KalturaHouseholdSuspentionState ConvertDomainSuspentionStatus(WebAPI.Social.DomainSuspentionStatus type)
        {
            KalturaHouseholdSuspentionState result;
            switch (type)
            {
                case WebAPI.Social.DomainSuspentionStatus.OK:
                    result = KalturaHouseholdSuspentionState.not_suspended;
                    break;
                case WebAPI.Social.DomainSuspentionStatus.Suspended:
                    result = KalturaHouseholdSuspentionState.suspended;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown domain suspention state");
            }
            return result;
        }

        private static KalturaUserState ConvertResponseStatusToUserState(WebAPI.Social.ResponseStatus type)
        {
            KalturaUserState result;
            switch (type)
            {
                case WebAPI.Social.ResponseStatus.OK:
                    result = KalturaUserState.ok;
                    break;
                case WebAPI.Social.ResponseStatus.UserWithNoDomain:
                    result = KalturaUserState.user_with_no_household;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown user state");
            }
            return result;
        }

        internal static Social.eUserAction ConvertSocialAction(KalturaSocialActionType action)
        {
            Social.eUserAction result;

            switch (action)
            {
                case KalturaSocialActionType.LIKE:
                    result = Social.eUserAction.LIKE;
                    break;
                case KalturaSocialActionType.WATCH:
                    result = Social.eUserAction.WATCHES;
                    break;
                case KalturaSocialActionType.RATE:
                    result = Social.eUserAction.RATES;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown social action");
                    break;
            }

            return result;
        }

        //eAssetType to KalturaAssetType
        public static WebAPI.Models.Catalog.KalturaAssetType ConvertToKalturaAssetType(WebAPI.Social.eAssetType assetType)
        {
            WebAPI.Models.Catalog.KalturaAssetType result;
            switch (assetType)
            {
                case WebAPI.Social.eAssetType.PROGRAM:
                    result = WebAPI.Models.Catalog.KalturaAssetType.epg;
                    break;
                case WebAPI.Social.eAssetType.MEDIA:
                    result = WebAPI.Models.Catalog.KalturaAssetType.media;
                    break;
                case WebAPI.Social.eAssetType.UNKNOWN:
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown Asset Type");
            }

            return result;
        }

        //int (eUserAction) to KalturaSocialActionType
        public static KalturaSocialActionType ConvertToKalturaSocialActionType(int action)
        {
            KalturaSocialActionType result;
            switch (action)
            {
                case 2:
                    result = KalturaSocialActionType.LIKE;
                    break;
                case 32:
                    result = KalturaSocialActionType.WATCH;
                    break;
                case 128:
                    result = KalturaSocialActionType.RATE;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown social action");
            }

            return result;
        }
    }
}