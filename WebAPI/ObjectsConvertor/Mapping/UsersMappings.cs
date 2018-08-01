using ApiObjects;
using AutoMapper;
using Core.Users;
using DAL;
using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Models.Users;
using WebAPI.Utils;
using System;
using ApiObjects.SSOAdapter;
using System.Linq;
using AutoMapper.Configuration;

namespace ObjectsConvertor.Mapping
{
    public class UsersMappings
    {
        public static void RegisterMappings(MapperConfigurationExpression cfg)
        {
            // PinCode
            cfg.CreateMap<PinCodeResponse, KalturaUserLoginPin>()
                .ForMember(dest => dest.UserId, opt => opt.ResolveUsing(src => src.siteGuid))
                .ForMember(dest => dest.PinCode, opt => opt.ResolveUsing(src => src.pinCode))
                .ForMember(dest => dest.ExpirationTime, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.expiredDate)));

            // UserType
            cfg.CreateMap<UserType, KalturaOTTUserType>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.ID))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => src.Description));

            // Country
            cfg.CreateMap<Core.Users.Country, KalturaCountry>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.m_nObjecrtID))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.m_sCountryName))
                .ForMember(dest => dest.Code, opt => opt.ResolveUsing(src => src.m_sCountryCode));

            //// UserBasicData
            //cfg.CreateMap<UserBasicData, KalturaUserBasicData>()
            //    .ForMember(dest => dest.Address, opt => opt.ResolveUsing(src => src.m_sAddress))
            //    .ForMember(dest => dest.AffiliateCode, opt => opt.ResolveUsing(src => src.m_sAffiliateCode))
            //    .ForMember(dest => dest.City, opt => opt.ResolveUsing(src => src.m_sCity))
            //    .ForMember(dest => dest.Country, opt => opt.ResolveUsing(src => src.m_Country))
            //    .ForMember(dest => dest.Email, opt => opt.ResolveUsing(src => src.m_sEmail))
            //    .ForMember(dest => dest.ExternalId, opt => opt.ResolveUsing(src => src.m_CoGuid))
            //    .ForMember(dest => dest.FacebookId, opt => opt.ResolveUsing(src => src.m_sFacebookID))
            //    .ForMember(dest => dest.FacebookImage, opt => opt.ResolveUsing(src => src.m_bIsFacebookImagePermitted ? src.m_sFacebookImage : null))
            //    .ForMember(dest => dest.FacebookToken, opt => opt.ResolveUsing(src => src.m_sFacebookToken))
            //    .ForMember(dest => dest.FirstName, opt => opt.ResolveUsing(src => src.m_sFirstName))
            //    .ForMember(dest => dest.LastName, opt => opt.ResolveUsing(src => src.m_sLastName))
            //    .ForMember(dest => dest.Phone, opt => opt.ResolveUsing(src => src.m_sPhone))
            //    .ForMember(dest => dest.Username, opt => opt.ResolveUsing(src => src.m_sUserName))
            //    .ForMember(dest => dest.UserType, opt => opt.ResolveUsing(src => src.m_UserType))
            //    .ForMember(dest => dest.Zip, opt => opt.ResolveUsing(src => src.m_sZip));

            // User
            cfg.CreateMap<UserResponseObject, KalturaOTTUser>()
                .ForMember(dest => dest.FirstName, opt => opt.ResolveUsing(src => src.m_user.m_oBasicData.m_sFirstName))
                .ForMember(dest => dest.LastName, opt => opt.ResolveUsing(src => src.m_user.m_oBasicData.m_sLastName))
                .ForMember(dest => dest.Username, opt => opt.ResolveUsing(src => src.m_user.m_oBasicData.m_sUserName))
                .ForMember(dest => dest.Email, opt => opt.ResolveUsing(src => src.m_user.m_oBasicData.m_sEmail))
                .ForMember(dest => dest.Address, opt => opt.ResolveUsing(src => src.m_user.m_oBasicData.m_sAddress))
                .ForMember(dest => dest.City, opt => opt.ResolveUsing(src => src.m_user.m_oBasicData.m_sCity))
                .ForMember(dest => dest.Country, opt => opt.ResolveUsing(src => src.m_user.m_oBasicData.m_Country))
                .ForMember(dest => dest.CountryId, opt => opt.ResolveUsing(src => src.m_user.m_oBasicData.m_Country != null ? src.m_user.m_oBasicData.m_Country.m_nObjecrtID : 0))
                .ForMember(dest => dest.Zip, opt => opt.ResolveUsing(src => src.m_user.m_oBasicData.m_sZip))
                .ForMember(dest => dest.Phone, opt => opt.ResolveUsing(src => src.m_user.m_oBasicData.m_sPhone))
                .ForMember(dest => dest.FacebookId, opt => opt.ResolveUsing(src => src.m_user.m_oBasicData.m_sFacebookID))
                .ForMember(dest => dest.FacebookImage, opt => opt.ResolveUsing(src => src.m_user.m_oBasicData.m_sFacebookImage))
                .ForMember(dest => dest.FacebookToken, opt => opt.ResolveUsing(src => src.m_user.m_oBasicData.m_sFacebookToken))
                .ForMember(dest => dest.AffiliateCode, opt => opt.ResolveUsing(src => src.m_user.m_oBasicData.m_sAffiliateCode))
                .ForMember(dest => dest.ExternalId, opt => opt.ResolveUsing(src => src.m_user.m_oBasicData.m_CoGuid))
                .ForMember(dest => dest.UserType, opt => opt.ResolveUsing(src => src.m_user.m_oBasicData.m_UserType))
                .ForMember(dest => dest.HouseholdID, opt => opt.ResolveUsing(src => src.m_user.m_domianID))
                .ForMember(dest => dest.DynamicData, opt => opt.ResolveUsing(src => ConvertDynamicData(src.m_user.m_oDynamicData)))
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.m_user.m_sSiteGUID))
                .ForMember(dest => dest.SuspentionState, opt => opt.ResolveUsing(src => ConvertDomainSuspentionStatus(src.m_user.m_eSuspendState)))
                .ForMember(dest => dest.SuspensionState, opt => opt.ResolveUsing(src => ConvertDomainSuspentionStatus(src.m_user.m_eSuspendState)))
                .ForMember(dest => dest.IsHouseholdMaster, opt => opt.ResolveUsing(src => src.m_user.m_isDomainMaster))
                .ForMember(dest => dest.UserState, opt => opt.ResolveUsing(src => ConvertResponseStatusToUserState(src.m_RespStatus, src.m_user.IsActivationGracePeriod)));

            // User
            cfg.CreateMap<User, KalturaOTTUser>()
                .ForMember(dest => dest.FirstName, opt => opt.ResolveUsing(src => src.m_oBasicData.m_sFirstName))
                .ForMember(dest => dest.LastName, opt => opt.ResolveUsing(src => src.m_oBasicData.m_sLastName))
                .ForMember(dest => dest.Username, opt => opt.ResolveUsing(src => src.m_oBasicData.m_sUserName))
                .ForMember(dest => dest.Email, opt => opt.ResolveUsing(src => src.m_oBasicData.m_sEmail))
                .ForMember(dest => dest.Address, opt => opt.ResolveUsing(src => src.m_oBasicData.m_sAddress))
                .ForMember(dest => dest.City, opt => opt.ResolveUsing(src => src.m_oBasicData.m_sCity))
                .ForMember(dest => dest.Country, opt => opt.ResolveUsing(src => src.m_oBasicData.m_Country))
                .ForMember(dest => dest.CountryId, opt => opt.ResolveUsing(src => src.m_oBasicData.m_Country != null ? src.m_oBasicData.m_Country.m_nObjecrtID : 0))
                .ForMember(dest => dest.Zip, opt => opt.ResolveUsing(src => src.m_oBasicData.m_sZip))
                .ForMember(dest => dest.Phone, opt => opt.ResolveUsing(src => src.m_oBasicData.m_sPhone))
                .ForMember(dest => dest.FacebookId, opt => opt.ResolveUsing(src => src.m_oBasicData.m_sFacebookID))
                .ForMember(dest => dest.FacebookImage, opt => opt.ResolveUsing(src => src.m_oBasicData.m_sFacebookImage))
                .ForMember(dest => dest.FacebookToken, opt => opt.ResolveUsing(src => src.m_oBasicData.m_sFacebookToken))
                .ForMember(dest => dest.AffiliateCode, opt => opt.ResolveUsing(src => src.m_oBasicData.m_sAffiliateCode))
                .ForMember(dest => dest.ExternalId, opt => opt.ResolveUsing(src => src.m_oBasicData.m_CoGuid))
                .ForMember(dest => dest.UserType, opt => opt.ResolveUsing(src => src.m_oBasicData.m_UserType))
                .ForMember(dest => dest.HouseholdID, opt => opt.ResolveUsing(src => src.m_domianID))
                .ForMember(dest => dest.DynamicData, opt => opt.ResolveUsing(src => ConvertDynamicData(src.m_oDynamicData)))
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.m_sSiteGUID))
                .ForMember(dest => dest.SuspentionState, opt => opt.ResolveUsing(src => ConvertDomainSuspentionStatus(src.m_eSuspendState)))
                .ForMember(dest => dest.SuspensionState, opt => opt.ResolveUsing(src => ConvertDomainSuspentionStatus(src.m_eSuspendState)))
                .ForMember(dest => dest.IsHouseholdMaster, opt => opt.ResolveUsing(src => src.m_isDomainMaster))
                .ForMember(dest => dest.UserState, opt => opt.ResolveUsing(src => ConvertResponseStatusToUserState(ResponseStatus.OK, src.IsActivationGracePeriod))); // for activation status

            // SlimUser
            cfg.CreateMap<KalturaOTTUser, KalturaBaseOTTUser>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id))
                .ForMember(dest => dest.Username, opt => opt.ResolveUsing(src => src.Username))
                .ForMember(dest => dest.FirstName, opt => opt.ResolveUsing(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.ResolveUsing(src => src.LastName));

            // UserId to SlimUser
            cfg.CreateMap<int, KalturaBaseOTTUser>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src));

            // Rest UserBasicData ==> WS_Users UserBasicData
            cfg.CreateMap<KalturaOTTUser, UserBasicData>()
                .ForMember(dest => dest.m_sAddress, opt => opt.ResolveUsing(src => src.Address))
                .ForMember(dest => dest.m_sAffiliateCode, opt => opt.ResolveUsing(src => src.AffiliateCode))
                .ForMember(dest => dest.m_sCity, opt => opt.ResolveUsing(src => src.City))
                .ForMember(dest => dest.m_Country, opt => opt.ResolveUsing(src => ConvertContry(src.Country, src.CountryId)))
                .ForMember(dest => dest.m_sEmail, opt => opt.ResolveUsing(src => src.Email))
                .ForMember(dest => dest.m_CoGuid, opt => opt.ResolveUsing(src => src.ExternalId != null ? src.ExternalId : null))
                .ForMember(dest => dest.m_sFacebookID, opt => opt.ResolveUsing(src => src.FacebookId))
                .ForMember(dest => dest.m_sFacebookImage, opt => opt.ResolveUsing(src => src.FacebookImage))
                .ForMember(dest => dest.m_sFacebookToken, opt => opt.ResolveUsing(src => src.FacebookToken))
                .ForMember(dest => dest.m_sFirstName, opt => opt.ResolveUsing(src => src.FirstName))
                .ForMember(dest => dest.m_sLastName, opt => opt.ResolveUsing(src => src.LastName))
                .ForMember(dest => dest.m_sPhone, opt => opt.ResolveUsing(src => src.Phone))
                .ForMember(dest => dest.m_sUserName, opt => opt.ResolveUsing(src => src.Username))
                .ForMember(dest => dest.m_UserType, opt => opt.ResolveUsing(src => src.UserType == null ? null : src.UserType))
                .ForMember(dest => dest.m_sZip, opt => opt.ResolveUsing(src => src.Zip));

            // Country
            cfg.CreateMap<KalturaCountry, Core.Users.Country>()
                .ForMember(dest => dest.m_nObjecrtID, opt => opt.ResolveUsing(src => src.Id))
                .ForMember(dest => dest.m_sCountryName, opt => opt.ResolveUsing(src => src.Name))
                .ForMember(dest => dest.m_sCountryCode, opt => opt.ResolveUsing(src => src.Code));

            // UserType
            cfg.CreateMap<KalturaOTTUserType, UserType>()
                .ForMember(dest => dest.ID, opt => opt.ResolveUsing(src => src.Id))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => src.Description));

            cfg.CreateMap<Dictionary<string, KalturaStringValue>, UserDynamicData>()
                .ForMember(dest => dest.m_sUserData, opt => opt.ResolveUsing(src => ConvertDynamicData(src)));

            // MediaId to AssetInfo
            cfg.CreateMap<string, KalturaAssetInfo>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => ConvertToLong(src)))
                .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => 0));

            // Rest WS_Users FavoritObject ==>  Favorite  
            cfg.CreateMap<FavoritObject, KalturaFavorite>()
                .ForMember(dest => dest.ExtraData, opt => opt.ResolveUsing(src => src.m_sExtraData))
                .ForMember(dest => dest.AssetId, opt => opt.ResolveUsing(src => src.m_sItemCode))
                .ForMember(dest => dest.Asset, opt => opt.ResolveUsing(src => src.m_sItemCode))
                .ForMember(dest => dest.CreateDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dCreateDate)));

            // UserItemsList to KalturaUserAssetsList
            cfg.CreateMap<UserItemsList, KalturaUserAssetsList>()
                .ForMember(dest => dest.List, opt => opt.ResolveUsing(src => src.ItemsList))
                .ForMember(dest => dest.ListType, opt => opt.ResolveUsing(src => ConvertUserAssetsListType(src.ListType)));

            // Item to KalturaUserAssetsListItem
            cfg.CreateMap<Item, KalturaUserAssetsListItem>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.ItemId))
                .ForMember(dest => dest.OrderIndex, opt => opt.ResolveUsing(src => src.OrderIndex))
                .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => ConvertUserAssetsListItemType(src.ItemType)))
                .ForMember(dest => dest.ListType, opt => opt.ResolveUsing(src => ConvertUserAssetsListType(src.ListType)))
                .ForMember(dest => dest.UserId, opt => opt.ResolveUsing(src => src.UserId));

            // Item to KalturaUserAssetsListItem
            cfg.CreateMap<KalturaUserAssetsListItem, Item>()
                .ForMember(dest => dest.ItemId, opt => opt.ResolveUsing(src => src.Id))
                .ForMember(dest => dest.OrderIndex, opt => opt.ResolveUsing(src => src.OrderIndex))
                .ForMember(dest => dest.ItemType, opt => opt.ResolveUsing(src => ConvertUserAssetsListItemType(src.Type)))
                .ForMember(dest => dest.ListType, opt => opt.ResolveUsing(src => ConvertUserAssetsListType(src.ListType)))
                .ForMember(dest => dest.UserId, opt => opt.ResolveUsing(src => src.UserId));

            // Country
            cfg.CreateMap<int, Core.Users.Country>()
                .ForMember(dest => dest.m_nObjecrtID, opt => opt.ResolveUsing(src => src));

            #region UserInterest

            cfg.CreateMap<KalturaUserInterest, UserInterest>()
               .ForMember(dest => dest.UserInterestId, opt => opt.ResolveUsing(src => src.Id))
               .ForMember(dest => dest.Topic, opt => opt.ResolveUsing(src => src.Topic))
               ;

            cfg.CreateMap<UserInterest, KalturaUserInterest>()
               .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.UserInterestId))
               .ForMember(dest => dest.Topic, opt => opt.ResolveUsing(src => src.Topic))
               ;

            cfg.CreateMap<KalturaUserInterestTopic, UserInterestTopic>()
               .ForMember(dest => dest.MetaId, opt => opt.ResolveUsing(src => src.MetaId))
               .ForMember(dest => dest.Value, opt => opt.ResolveUsing(src => src.Value))
               .ForMember(dest => dest.ParentTopic, opt => opt.ResolveUsing(src => src.ParentTopic))
               ;

            cfg.CreateMap<UserInterestTopic, KalturaUserInterestTopic>()
             .ForMember(dest => dest.MetaId, opt => opt.ResolveUsing(src => src.MetaId))
             .ForMember(dest => dest.Value, opt => opt.ResolveUsing(src => src.Value))
             .ForMember(dest => dest.ParentTopic, opt => opt.ResolveUsing(src => src.ParentTopic))
             ;

            #endregion

            // UserDynamicData to KalturaOTTUserDynamicData
            cfg.CreateMap<UserDynamicData, KalturaOTTUserDynamicData>()
                .ForMember(dest => dest.UserId, opt => opt.ResolveUsing(src => src.UserId))
                .ForMember(dest => dest.Key, opt => opt.ResolveUsing(src => ConvertDynamicDataKey(src)))
                .ForMember(dest => dest.Value, opt => opt.ResolveUsing(src => ConvertDynamicDataValue(src)));

            cfg.CreateMap<UserDynamicData, KalturaOTTUserDynamicDataList>()
                .ForMember(dest => dest.DynamicData, opt => opt.ResolveUsing(src => ConvertDynamicData(src)))
                .ForMember(dest => dest.UserId, opt => opt.ResolveUsing(src => src.UserId));

            cfg.CreateMap<SSOAdapter, KalturaSSOAdapterProfile>()
               .ForMember(dest => dest.Settings, opt => opt.ResolveUsing(src => src.Settings != null ? src.Settings.ToDictionary(k => k.Key, v => v.Value) : null));

            cfg.CreateMap<KalturaSSOAdapterProfile, SSOAdapter>()
                .ForMember(dest => dest.Settings, opt => opt.ResolveUsing(src => ConvertSsoAdapterSettings(src)));
        }

        private static List<SSOAdapterParam> ConvertSsoAdapterSettings(KalturaSSOAdapterProfile src)
        {
            if (src.Settings == null) { return new List<SSOAdapterParam>(); }
            var settingsList = src.Settings.Select(s => new SSOAdapterParam
            {
                AdapterId = src.Id ?? 0,
                Key = s.Key,
                Value = s.Value != null ? s.Value.value : null,
            });

            return settingsList.ToList();
        }

        private static Core.Users.Country ConvertContry(KalturaCountry country, int? countryId)
        {
            Core.Users.Country response = new Core.Users.Country();
            if (countryId.HasValue && countryId.Value > 0)
            {
                response.m_nObjecrtID = countryId.Value;
            }
            else if (country != null)
            {
                response.m_nObjecrtID = country.Id;
            }
            return response;
        }

        // ListType to KalturaUserAssetsListType
        private static KalturaUserAssetsListType ConvertUserAssetsListType(ListType listType)
        {
            KalturaUserAssetsListType result;
            switch (listType)
            {
                case ListType.Watch:
                    result = KalturaUserAssetsListType.watch;
                    break;
                case ListType.Purchase:
                    result = KalturaUserAssetsListType.purchase;
                    break;
                case ListType.Library:
                    result = KalturaUserAssetsListType.library;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown user assets list type");
            }
            return result;
        }

        // KalturaUserAssetsListType to ListType
        public static ListType ConvertUserAssetsListType(KalturaUserAssetsListType listType)
        {
            ListType result;
            switch (listType)
            {
                case KalturaUserAssetsListType.all:
                    result = ListType.All;
                    break;
                case KalturaUserAssetsListType.watch:
                    result = ListType.Watch;
                    break;
                case KalturaUserAssetsListType.purchase:
                    result = ListType.Purchase;
                    break;
                case KalturaUserAssetsListType.library:
                    result = ListType.Library;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown user assets list type");
            }
            return result;
        }

        public static ListItemType ConvertUserAssetsListItemType(KalturaUserAssetsListItemType itemType)
        {
            ListItemType result;

            switch (itemType)
            {
                case KalturaUserAssetsListItemType.all:
                    result = ListItemType.All;
                    break;
                case KalturaUserAssetsListItemType.media:
                    result = ListItemType.Media;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown user assets list item type");
            }
            return result;
        }

        private static KalturaUserAssetsListItemType ConvertUserAssetsListItemType(ListItemType itemType)
        {
            KalturaUserAssetsListItemType result;

            switch (itemType)
            {
                case ListItemType.Media:
                    result = KalturaUserAssetsListItemType.media;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown user assets list item type");
            }
            return result;
        }

        private static long ConvertToLong(string src)
        {
            long output = 0;
            long.TryParse(src, out output);
            return output;


        }

        private static KalturaUserState ConvertResponseStatusToUserState(ResponseStatus type, bool isActivationGracePeriod)
        {
            KalturaUserState result;
            if (isActivationGracePeriod)
            {
                result = KalturaUserState.user_not_activated;
            }
            else
            {
                switch (type)
                {
                    case ResponseStatus.OK:
                    case ResponseStatus.UserSuspended:
                        result = KalturaUserState.ok;
                        break;
                    case ResponseStatus.UserWithNoDomain:
                        result = KalturaUserState.user_with_no_household;
                        break;
                    case ResponseStatus.UserCreatedWithNoRole:
                        result = KalturaUserState.user_created_with_no_role;
                        break;
                    default:
                        throw new ClientException((int)StatusCode.Error, "Unknown user state");
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

        private static List<UserDynamicDataContainer> ConvertDynamicData(Dictionary<string, KalturaStringValue> userDynamicData)
        {
            List<UserDynamicDataContainer> result = null;

            if (userDynamicData != null && userDynamicData.Count > 0)
            {
                result = new List<UserDynamicDataContainer>();
                foreach (KeyValuePair<string, KalturaStringValue> data in userDynamicData)
                {
                    if (!string.IsNullOrEmpty(data.Key))
                    {
                        result.Add(new UserDynamicDataContainer() { m_sDataType = data.Key, m_sValue = data.Value.value });
                    }
                }
            }

            return result;
        }

        public static Dictionary<string, KalturaStringValue> ConvertDynamicData(UserDynamicData userDynamicData)
        {
            Dictionary<string, KalturaStringValue> result = null;

            if (userDynamicData != null && userDynamicData.m_sUserData != null)
            {
                result = new Dictionary<string, KalturaStringValue>();
                foreach (var data in userDynamicData.m_sUserData)
                {
                    if (!string.IsNullOrEmpty(data.m_sDataType))
                    {
                        result.Add(data.m_sDataType, new KalturaStringValue() { value = data.m_sValue });
                    }
                }
            }

            return result;
        }

        public static string ConvertDynamicDataKey(UserDynamicData userDynamicData)
        {
            string result = string.Empty;

            if (userDynamicData != null && userDynamicData.m_sUserData != null && userDynamicData.m_sUserData.Length > 0)
            {
                result = userDynamicData.m_sUserData[0].m_sDataType;
            }

            return result;
        }

        public static KalturaStringValue ConvertDynamicDataValue(UserDynamicData userDynamicData)
        {
            KalturaStringValue result = null;

            if (userDynamicData != null && userDynamicData.m_sUserData != null && userDynamicData.m_sUserData.Length > 0)
            {
                result = new KalturaStringValue() { value = userDynamicData.m_sUserData[0].m_sValue };
            }

            return result;
        }

        public static FavoriteOrderBy ConvertFavoriteOrderBy(KalturaFavoriteOrderBy orderBy)
        {
            FavoriteOrderBy result;

            switch (orderBy)
            {
                case KalturaFavoriteOrderBy.CREATE_DATE_DESC:
                    result = FavoriteOrderBy.CreateDateDesc;
                    break;
                case KalturaFavoriteOrderBy.CREATE_DATE_ASC:
                    result = FavoriteOrderBy.CreateDateAsc;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown export task order by");
            }

            return result;
        }

        internal static KalturaOTTUserDynamicData ConvertOTTUserDynamicData(string userId, string key, KalturaStringValue value)
        {
            KalturaOTTUserDynamicData result = new KalturaOTTUserDynamicData() { UserId = userId, Key = key, Value = value };
            return result;
        }
    }
}
