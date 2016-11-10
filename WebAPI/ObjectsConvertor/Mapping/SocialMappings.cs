using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using Users;
using Users;
using DAL;
using ApiObjects;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.Models.Users;
using WebAPI.Models.General;
using Social;
using WebAPI.Models.Social;
using ApiObjects.Social;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class SocialMappings
    {
        public static void RegisterMappings()
        {
            // FacebookResponse to ClientFacebookResponse
            Mapper.CreateMap<FacebookResponseObject, KalturaFacebookSocial>()
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

            Mapper.CreateMap<FacebookResponseObject, KalturaSocialResponse>()
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
            Mapper.CreateMap<FBUser, KalturaSocialUser>()
                .ForMember(dest => dest.Birthday, opt => opt.MapFrom(src => src.Birthday))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.email))
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.first_name))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.gender))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.last_name))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.name))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.m_sSiteGuid));

            // UserType
            Mapper.CreateMap<Users.UserType, KalturaOTTUserType>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            // Country
            Mapper.CreateMap<Users.Country, KalturaCountry>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nObjecrtID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sCountryName))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.m_sCountryCode));

            //// UserBasicData
            //Mapper.CreateMap<UserBasicData, KalturaUserBasicData>()
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
            Mapper.CreateMap<UserResponseObject, KalturaOTTUser>()
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
            Mapper.CreateMap<FacebookConfig, KalturaSocialFacebookConfig>()
                .ForMember(dest => dest.AppId, opt => opt.MapFrom(src => src.sFBKey))
                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.sFBPermissions));

            // SocialActivityDoc to KalturaSocialFriendActivity
            Mapper.CreateMap<SocialActivityDoc, KalturaSocialFriendActivity>()
                .ForMember(dest => dest.SocialAction, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.ActivitySubject.ActorTvinciUsername))
                .ForMember(dest => dest.UserPictureUrl, opt => opt.MapFrom(src => src.ActivitySubject.ActorPicUrl));

            // ActivityVerb to KalturaSocialAction

            Mapper.CreateMap<KalturaSocialUserConfig, ApiObjects.Social.SocialNetwork[]>().ConstructUsing(ConvertSocialNetwork);

            Mapper.CreateMap<ApiObjects.Social.SocialPrivacySettings, KalturaSocialConfig>().ConstructUsing(ConvertSocialNetwork);

            Mapper.CreateMap<SocialActivityDoc, KalturaSocialAction>().ConstructUsing(ConvertToKalturaSocialAction);

            Mapper.CreateMap<ApiObjects.Social.UserSocialActionRequest, KalturaSocialAction>()
                 .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.AssetID))
                 .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src =>ConvertAssetType(src.AssetType)))
                 .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ConvertSocialActionType(src.Action)));

            Mapper.CreateMap<KalturaSocialAction ,ApiObjects.Social.UserSocialActionRequest>()
                 .ForMember(dest => dest.AssetID, opt => opt.MapFrom(src => src.AssetId))
                 .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => ConvertAssetType(src.AssetType)))
                 .ForMember(dest => dest.Action, opt => opt.MapFrom(src => ConvertSocialActionType(src.Type)));

            Mapper.CreateMap<ApiObjects.Social.UserSocialActionRequest, KalturaSocialActionRate>()
                 .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.AssetID))
                 .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src =>ConvertAssetType(src.AssetType)))
                 .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ConvertSocialActionType(src.Action)))
                  .ForMember(dest => dest.Rate, opt => opt.MapFrom(src => ConvertRateParams(src.ExtraParams)));

            Mapper.CreateMap<KalturaSocialActionRate, ApiObjects.Social.UserSocialActionRequest>()
                 .ForMember(dest => dest.AssetID, opt => opt.MapFrom(src => src.AssetId))
                 .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => ConvertAssetType(src.AssetType)))
                 .ForMember(dest => dest.Action, opt => opt.MapFrom(src => ConvertSocialActionType(src.Type)))
                  .ForMember(dest => dest.ExtraParams, opt => opt.MapFrom(src => ConvertRateParams(src.Rate)));


            Mapper.CreateMap<UserSocialActionResponse, KalturaSocialAction>()
                .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.UserAction.AssetID))
                .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => ConvertAssetType(src.UserAction.AssetType)))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ConvertSocialActionType(src.UserAction.Action)))
                ;
        }

        //private static List<KalturaNetworkActionStatus> ConvertNetworkActionStatus(List<NetworkActionStatus> src)
        //{
        //    List<KalturaNetworkActionStatus> result = new List<KalturaNetworkActionStatus>();
        //    KalturaNetworkActionStatus kns;
        //    foreach (NetworkActionStatus networkStatus in src)
        //    {
        //        kns = new KalturaNetworkActionStatus();
        //        if (networkStatus.Network != null)
        //        {
        //            switch (networkStatus.Network)
        //            {
        //                case SocialPlatform.FACEBOOK:
        //                    kns.Network = KalturaSocialNetwork.facebook;
        //                    break;
        //                default:
        //                    kns.Network = null;
        //                    break;
        //            }
        //        }
        //        switch (networkStatus.Status.Code)
        //        {
        //            case (int)ApiObjects.Response.eResponseStatus.Error:
        //                kns.Status = KalturaSocialStatus.error;
        //                break;
        //            case (int)ApiObjects.Response.eResponseStatus.OK:
        //                kns.Status = KalturaSocialStatus.ok;
        //                break;
        //            case (int)ApiObjects.Response.eResponseStatus.UserDoesNotExist:
        //                kns.Status = KalturaSocialStatus.user_does_not_exist;
        //                break;
        //            case (int)ApiObjects.Response.eResponseStatus.NoUserSocialSettingsFound:
        //                kns.Status = KalturaSocialStatus.no_user_social_settings_found;
        //                break;
        //            case (int)ApiObjects.Response.eResponseStatus.AssetAlreadyLiked:
        //                kns.Status = KalturaSocialStatus.asset_already_liked;
        //                break;
        //            case (int)ApiObjects.Response.eResponseStatus.NotAllowed:
        //                kns.Status = KalturaSocialStatus.not_allowed;
        //                break;
        //            case (int)ApiObjects.Response.eResponseStatus.InvalidParameters:
        //                kns.Status = KalturaSocialStatus.invalid_parameters;
        //                break;
        //            case (int)ApiObjects.Response.eResponseStatus.NoFacebookAction:
        //                kns.Status = KalturaSocialStatus.no_facebook_action;
        //                break;
        //            case (int)ApiObjects.Response.eResponseStatus.AssetAlreadyRated:
        //                kns.Status = KalturaSocialStatus.asset_already_rated;
        //                break;
        //            case (int)ApiObjects.Response.eResponseStatus.AssetDoseNotExists:
        //                kns.Status = KalturaSocialStatus.asset_dose_not_exists;
        //                break;
        //            case (int)ApiObjects.Response.eResponseStatus.InvalidPlatformRequest:
        //                kns.Status = KalturaSocialStatus.invalid_platform_request;
        //                break;
        //            case (int)ApiObjects.Response.eResponseStatus.InvalidAccessToken:
        //                kns.Status = KalturaSocialStatus.invalid_access_token;
        //                break;
        //            default:
        //                kns.Status = KalturaSocialStatus.error;
        //                break;
        //        }
        //        result.Add(kns);
        //    }

        //    return result;
        //}

        private static List<ApiObjects.KeyValuePair> ConvertRateParams(int RateValue)
        {
            List<ApiObjects.KeyValuePair> ExtraParams = new List<KeyValuePair>();
            ExtraParams.Add(new KeyValuePair() { key = "rating:value", value = RateValue.ToString()});
            return ExtraParams;
        }

        private static eUserAction ConvertSocialActionType(KalturaSocialActionType kalturaSocialActionType)
        {
            eUserAction result;
            switch (kalturaSocialActionType)
            {
                case KalturaSocialActionType.LIKE:
                    result = eUserAction.LIKE;
                    break;
                case KalturaSocialActionType.WATCH:
                    result = eUserAction.WATCHES;
                    break;
                case KalturaSocialActionType.RATE:
                    result = eUserAction.RATES;
                    break;
                case KalturaSocialActionType.UNLIKE:
                    result = eUserAction.UNLIKE;
                    break;
                case KalturaSocialActionType.SHARE:
                    result = eUserAction.SHARE;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown Action");
            }
            return result;
        }

        private static eAssetType ConvertAssetType(KalturaAssetType kalturaAssetType)
        {
            switch (kalturaAssetType)
            {
                case KalturaAssetType.media:
                    return eAssetType.MEDIA;
                    break;
                case KalturaAssetType.recording:
                    return eAssetType.UNKNOWN;
                    break;
                case KalturaAssetType.epg:
                    return eAssetType.PROGRAM;
                    break;
                default:
                    return eAssetType.UNKNOWN;
                    break;
            }
        }

        private static int ConvertRateParams(List<KeyValuePair> list)
        {
            int RateValue = 0;
            Dictionary<string, string> extraParams = KvpListToDictionary(ref list);
            if (extraParams.ContainsKey("rating:value") && !int.TryParse(extraParams["rating:value"], out RateValue))
                return RateValue;
            return RateValue;
        }
        
        private static Dictionary<string, string> KvpListToDictionary(ref List<KeyValuePair> lExtraParams)
        {
            Dictionary<string, string> dResult = new Dictionary<string, string>();

            if (lExtraParams != null && lExtraParams.Count > 0)
            {
                for (int i = 0; i < lExtraParams.Count; i++)
                {
                    if (lExtraParams[i] != null && !string.IsNullOrEmpty(lExtraParams[i].key) && !string.IsNullOrEmpty(lExtraParams[i].value))
                    {
                        dResult[lExtraParams[i].key] = lExtraParams[i].value;
                    }
                }
            }

            return dResult;
        }

        private static KalturaSocialActionType ConvertSocialActionType(eUserAction eUserAction)
        {
            KalturaSocialActionType result = KalturaSocialActionType.LIKE;
            switch (eUserAction)
            {  
                case eUserAction.LIKE:
                    result = KalturaSocialActionType.LIKE;
                    break;
                case eUserAction.UNLIKE:
                    result = KalturaSocialActionType.UNLIKE;
                    break;
                case eUserAction.SHARE:
                    result = KalturaSocialActionType.SHARE;
                    break;
                case eUserAction.WATCHES:
                    break;
                    result = KalturaSocialActionType.WATCH;
                    break;
                case eUserAction.RATES:
                    result = KalturaSocialActionType.RATE;
                    break;   
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown Action");                   
            }
            return result;
        }

        private static KalturaAssetType ConvertAssetType(eAssetType eAssetType)
        {
            switch (eAssetType)
            {                
                case eAssetType.MEDIA:
                    return KalturaAssetType.media;
                    break;
                case eAssetType.PROGRAM:
                    return KalturaAssetType.epg;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown AssetType");
                    break;
            }
        }

        private static KalturaSocialAction ConvertToKalturaSocialAction(ResolutionContext context)
        {
            KalturaSocialAction result;
            var action = (SocialActivityDoc)context.SourceValue;
            var actionType = ConvertToKalturaSocialActionType(action.ActivityVerb.ActionType);

            if (actionType == KalturaSocialActionType.RATE)
            {
                result = new KalturaSocialActionRate(action.ActivityVerb.RateValue);
            }
            else
            {
                result = new KalturaSocialAction()
                {
                    Type = actionType
                };
            }

            result.Time = action.LastUpdate;
            result.AssetId = action.ActivityObject.AssetID;
            result.AssetType = ConvertToKalturaAssetType(action.ActivityObject.AssetType);

            return result;
        }

        public static KeyValuePair[] ConvertDictionaryToKeyValue(Dictionary<string, string> dictionary)
        {
            if (dictionary != null && dictionary.Count > 0)
            {
                KeyValuePair[] keyValuePair = new KeyValuePair[dictionary.Count];

                for (int i = 0; i < dictionary.Count; i++)
                {
                    keyValuePair[i] = new KeyValuePair()
                    {
                        key = dictionary.ElementAt(i).Key,
                        value = dictionary.ElementAt(i).Value
                    };
                }

                return keyValuePair;
            }

            return null;
        }

        public static SerializableDictionary<string, KalturaStringValue> ConvertDynamicData(UserDynamicData userDynamicData)
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

        private static KalturaHouseholdSuspentionState ConvertDomainSuspentionStatus(DomainSuspentionStatus type)
        {
            KalturaHouseholdSuspentionState result;
            switch (type)
            {
                case DomainSuspentionStatus.OK:
                    result = KalturaHouseholdSuspentionState.not_suspended;
                    break;
                case DomainSuspentionStatus.Suspended:
                    result = KalturaHouseholdSuspentionState.suspended;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown domain suspention state");
            }
            return result;
        }

        private static KalturaUserState ConvertResponseStatusToUserState(ResponseStatus type)
        {
            KalturaUserState result;
            switch (type)
            {
                case ResponseStatus.OK:
                    result = KalturaUserState.ok;
                    break;
                case ResponseStatus.UserWithNoDomain:
                    result = KalturaUserState.user_with_no_household;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown user state");
            }
            return result;
        }

        internal static eUserAction ConvertSocialAction(KalturaSocialActionType action)
        {
            eUserAction result;

            switch (action)
            {
                case KalturaSocialActionType.LIKE:
                    result = eUserAction.LIKE;
                    break;
                case KalturaSocialActionType.WATCH:
                    result = eUserAction.WATCHES;
                    break;
                case KalturaSocialActionType.RATE:
                    result = eUserAction.RATES;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown social action");
                    break;
            }

            return result;
        }

        //eAssetType to KalturaAssetType
        public static KalturaAssetType ConvertToKalturaAssetType(eAssetType assetType)
        {
            KalturaAssetType result;
            switch (assetType)
            {
                case eAssetType.PROGRAM:
                    result = KalturaAssetType.epg;
                    break;
                case eAssetType.MEDIA:
                    result = KalturaAssetType.media;
                    break;
                case eAssetType.UNKNOWN:
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

        private static ApiObjects.Social.SocialNetwork[] ConvertSocialNetwork(KalturaSocialUserConfig config)
        {
            List<ApiObjects.Social.SocialNetwork> socialNetworkList = new List<ApiObjects.Social.SocialNetwork>();
            ApiObjects.Social.SocialNetwork socialnetwork;

            ApiObjects.Social.SocialPrivacySettings settings = new ApiObjects.Social.SocialPrivacySettings();

            foreach (KalturaActionPermissionItem permissionItem in config.PermissionItems)
            {
                socialnetwork = new ApiObjects.Social.SocialNetwork();
                if (permissionItem.Network == null) // internal seetings
                {
                    switch (permissionItem.ActionPrivacy)
                    {
                        case KalturaSocialActionPrivacy.ALLOW:
                            settings.InternalPrivacy = eSocialActionPrivacy.ALLOW;
                            break;
                        case KalturaSocialActionPrivacy.DONT_ALLOW:
                            settings.InternalPrivacy = eSocialActionPrivacy.DONT_ALLOW;
                            break;
                        default:
                            settings.InternalPrivacy = eSocialActionPrivacy.ALLOW;
                            break;
                    }
                }
                else
                {
                    switch (permissionItem.Network)
                    {
                        case KalturaSocialNetwork.facebook:
                            socialnetwork.Network = SocialPlatform.FACEBOOK;
                            break;
                        default:
                            break;
                    }
                    switch (permissionItem.ActionPrivacy)
                    {
                        case KalturaSocialActionPrivacy.ALLOW:
                            socialnetwork.Privacy = eSocialActionPrivacy.ALLOW;
                            break;
                        case KalturaSocialActionPrivacy.DONT_ALLOW:
                            socialnetwork.Privacy = eSocialActionPrivacy.DONT_ALLOW;
                            break;
                        default:
                            socialnetwork.Privacy = eSocialActionPrivacy.DONT_ALLOW;
                            break;
                    }
                    switch (permissionItem.Privacy)
                    {
                        case KalturaSocialPrivacy.UNKNOWN:
                            socialnetwork.SocialPrivacy = eSocialPrivacy.UNKNOWN;
                            break;
                        case KalturaSocialPrivacy.EVERYONE:
                            socialnetwork.SocialPrivacy = eSocialPrivacy.EVERYONE;
                            break;
                        case KalturaSocialPrivacy.ALL_FRIENDS:
                            socialnetwork.SocialPrivacy = eSocialPrivacy.ALL_FRIENDS;
                            break;
                        case KalturaSocialPrivacy.FRIENDS_OF_FRIENDS:
                            socialnetwork.SocialPrivacy = eSocialPrivacy.FRIENDS_OF_FRIENDS;
                            break;
                        case KalturaSocialPrivacy.SELF:
                            socialnetwork.SocialPrivacy = eSocialPrivacy.SELF;
                            break;
                        case KalturaSocialPrivacy.CUSTOM:
                            socialnetwork.SocialPrivacy = eSocialPrivacy.CUSTOM;
                            break;
                        default:
                            break;
                    }

                    socialNetworkList.Add(socialnetwork);
                }
            }           
            return socialNetworkList.ToArray();
        }

        private static KalturaSocialConfig ConvertSocialNetwork(ApiObjects.Social.SocialPrivacySettings socialPrivacySettings)
        {
            KalturaSocialUserConfig ksc = new KalturaSocialUserConfig();
            KalturaActionPermissionItem kapi = new KalturaActionPermissionItem();
            if (socialPrivacySettings != null)
            {
                ksc.PermissionItems = new List<KalturaActionPermissionItem>();
                switch (socialPrivacySettings.InternalPrivacy)
                {
                    case ApiObjects.eSocialActionPrivacy.ALLOW:
                        kapi.ActionPrivacy = KalturaSocialActionPrivacy.ALLOW;
                        break;
                    case ApiObjects.eSocialActionPrivacy.DONT_ALLOW:
                        kapi.ActionPrivacy = KalturaSocialActionPrivacy.DONT_ALLOW;
                        break;
                    default:
                        kapi.ActionPrivacy = KalturaSocialActionPrivacy.ALLOW;
                        break;
                }
                kapi.Network = null;

                ksc.PermissionItems.Add(kapi);

                foreach (var item in socialPrivacySettings.SocialNetworks)
                {
                    kapi = new KalturaActionPermissionItem();
                    switch (item.Network)
                    {
                        case ApiObjects.SocialPlatform.FACEBOOK:
                            kapi.Network = KalturaSocialNetwork.facebook;
                            break;
                    }
                    switch (item.SocialPrivacy)
                    {
                        case ApiObjects.eSocialPrivacy.UNKNOWN:
                            kapi.Privacy = KalturaSocialPrivacy.UNKNOWN;
                            break;
                        case ApiObjects.eSocialPrivacy.EVERYONE:
                            kapi.Privacy = KalturaSocialPrivacy.EVERYONE;
                            break;
                        case ApiObjects.eSocialPrivacy.ALL_FRIENDS:
                            kapi.Privacy = KalturaSocialPrivacy.ALL_FRIENDS;
                            break;
                        case ApiObjects.eSocialPrivacy.FRIENDS_OF_FRIENDS:
                            kapi.Privacy = KalturaSocialPrivacy.FRIENDS_OF_FRIENDS;
                            break;
                        case ApiObjects.eSocialPrivacy.SELF:
                            kapi.Privacy = KalturaSocialPrivacy.SELF;
                            break;
                        case ApiObjects.eSocialPrivacy.CUSTOM:
                            kapi.Privacy = KalturaSocialPrivacy.CUSTOM;
                            break;
                        default:
                            break;
                    }
                    switch (item.Privacy)
                    {
                        case ApiObjects.eSocialActionPrivacy.ALLOW:
                            kapi.ActionPrivacy = KalturaSocialActionPrivacy.ALLOW;
                            break;
                        case ApiObjects.eSocialActionPrivacy.DONT_ALLOW:
                            kapi.ActionPrivacy = KalturaSocialActionPrivacy.DONT_ALLOW;
                            break;
                        default:
                            kapi.ActionPrivacy = KalturaSocialActionPrivacy.DONT_ALLOW;
                            break;
                    }

                    ksc.PermissionItems.Add(kapi);
                }
            }
            return ksc;
        }
        
    }
}