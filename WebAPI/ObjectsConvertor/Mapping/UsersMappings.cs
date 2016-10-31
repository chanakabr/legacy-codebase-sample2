using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Users;
using DAL;
using WebAPI.Models.Users;
using WebAPI.Utils;
using WebAPI.Models.General;
using WebAPI.Models.Catalog;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using ApiObjects;

namespace ObjectsConvertor.Mapping
{
    public class UsersMappings
    {
        public static void RegisterMappings()
        {
            // PinCode
            Mapper.CreateMap<PinCodeResponse, KalturaUserLoginPin>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.siteGuid))
                .ForMember(dest => dest.PinCode, opt => opt.MapFrom(src => src.pinCode))
                .ForMember(dest => dest.ExpirationTime, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.expiredDate)));

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

            // User
            Mapper.CreateMap<UserResponseObject, KalturaOTTUser>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_sFirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_sLastName))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_sUserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_sEmail))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_sAddress))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_sCity))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_Country))
                .ForMember(dest => dest.CountryId, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_Country != null ? src.m_user.m_oBasicData.m_Country.m_nObjecrtID : 0))
                .ForMember(dest => dest.Zip, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_sZip))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_sPhone))
                .ForMember(dest => dest.FacebookId, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_sFacebookID))
                .ForMember(dest => dest.FacebookImage, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_sFacebookImage))
                .ForMember(dest => dest.FacebookToken, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_sFacebookToken))
                .ForMember(dest => dest.AffiliateCode, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_sAffiliateCode))
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_CoGuid))
                .ForMember(dest => dest.UserType, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_UserType))
                .ForMember(dest => dest.HouseholdID, opt => opt.MapFrom(src => src.m_user.m_domianID))
                .ForMember(dest => dest.DynamicData, opt => opt.MapFrom(src => ConvertDynamicData(src.m_user.m_oDynamicData)))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_user.m_sSiteGUID))
                .ForMember(dest => dest.SuspentionState, opt => opt.MapFrom(src => ConvertDomainSuspentionStatus(src.m_user.m_eSuspendState)))
                .ForMember(dest => dest.SuspensionState, opt => opt.MapFrom(src => ConvertDomainSuspentionStatus(src.m_user.m_eSuspendState)))
                .ForMember(dest => dest.IsHouseholdMaster, opt => opt.MapFrom(src => src.m_user.m_isDomainMaster))
                .ForMember(dest => dest.UserState, opt => opt.MapFrom(src => ConvertResponseStatusToUserState(src.m_RespStatus, src.m_user.IsActivationGracePeriod)));

            // User
            Mapper.CreateMap<User, KalturaOTTUser>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.m_oBasicData.m_sFirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.m_oBasicData.m_sLastName))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.m_oBasicData.m_sUserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.m_oBasicData.m_sEmail))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.m_oBasicData.m_sAddress))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.m_oBasicData.m_sCity))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.m_oBasicData.m_Country))
                .ForMember(dest => dest.CountryId, opt => opt.MapFrom(src => src.m_oBasicData.m_Country != null ? src.m_oBasicData.m_Country.m_nObjecrtID : 0))
                .ForMember(dest => dest.Zip, opt => opt.MapFrom(src => src.m_oBasicData.m_sZip))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.m_oBasicData.m_sPhone))
                .ForMember(dest => dest.FacebookId, opt => opt.MapFrom(src => src.m_oBasicData.m_sFacebookID))
                .ForMember(dest => dest.FacebookImage, opt => opt.MapFrom(src => src.m_oBasicData.m_sFacebookImage))
                .ForMember(dest => dest.FacebookToken, opt => opt.MapFrom(src => src.m_oBasicData.m_sFacebookToken))
                .ForMember(dest => dest.AffiliateCode, opt => opt.MapFrom(src => src.m_oBasicData.m_sAffiliateCode))
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.m_oBasicData.m_ExternalToken))
                .ForMember(dest => dest.UserType, opt => opt.MapFrom(src => src.m_oBasicData.m_UserType))
                .ForMember(dest => dest.HouseholdID, opt => opt.MapFrom(src => src.m_domianID))
                .ForMember(dest => dest.DynamicData, opt => opt.MapFrom(src => ConvertDynamicData(src.m_oDynamicData)))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_sSiteGUID))
                .ForMember(dest => dest.SuspentionState, opt => opt.MapFrom(src => ConvertDomainSuspentionStatus(src.m_eSuspendState)))
                .ForMember(dest => dest.SuspensionState, opt => opt.MapFrom(src => ConvertDomainSuspentionStatus(src.m_eSuspendState)))
                .ForMember(dest => dest.IsHouseholdMaster, opt => opt.MapFrom(src => src.m_isDomainMaster));

            // SlimUser
            Mapper.CreateMap<KalturaOTTUser, KalturaBaseOTTUser>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName));

            // UserId to SlimUser
            Mapper.CreateMap<int, KalturaBaseOTTUser>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src));

            // Rest UserBasicData ==> WS_Users UserBasicData
            Mapper.CreateMap<KalturaOTTUser, UserBasicData>()
                .ForMember(dest => dest.m_sAddress, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.m_sAffiliateCode, opt => opt.MapFrom(src => src.AffiliateCode))
                .ForMember(dest => dest.m_sCity, opt => opt.MapFrom(src => src.City))
                .ForMember(dest => dest.m_Country, opt => opt.MapFrom(src => ConvertContry(src.Country, src.CountryId)))
                .ForMember(dest => dest.m_sEmail, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.m_CoGuid, opt => opt.MapFrom(src => src.ExternalId))
                .ForMember(dest => dest.m_sFacebookID, opt => opt.MapFrom(src => src.FacebookId))
                .ForMember(dest => dest.m_sFacebookImage, opt => opt.MapFrom(src => src.FacebookImage))
                .ForMember(dest => dest.m_sFacebookToken, opt => opt.MapFrom(src => src.FacebookToken))
                .ForMember(dest => dest.m_sFirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.m_sLastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.m_sPhone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.m_sUserName, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.m_UserType, opt => opt.ResolveUsing(src => src.UserType == null ? null : src.UserType))
                .ForMember(dest => dest.m_sZip, opt => opt.MapFrom(src => src.Zip));

            // Country
            Mapper.CreateMap<KalturaCountry, Users.Country>()
                .ForMember(dest => dest.m_nObjecrtID, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.m_sCountryName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.m_sCountryCode, opt => opt.MapFrom(src => src.Code));

            // UserType
            Mapper.CreateMap<KalturaOTTUserType, Users.UserType>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            Mapper.CreateMap<Dictionary<string, KalturaStringValue>, UserDynamicData>()
                .ForMember(dest => dest.m_sUserData, opt => opt.MapFrom(src => ConvertDynamicData(src)));

            // MediaId to AssetInfo
            Mapper.CreateMap<string, KalturaAssetInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => ConvertToLong(src)))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => 0));

            // Rest WS_Users FavoritObject ==>  Favorite  
            Mapper.CreateMap<FavoritObject, KalturaFavorite>()
                .ForMember(dest => dest.ExtraData, opt => opt.MapFrom(src => src.m_sExtraData))
                .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.m_sItemCode))
                .ForMember(dest => dest.Asset, opt => opt.MapFrom(src => src.m_sItemCode));

            // UserItemsList to KalturaUserAssetsList
            Mapper.CreateMap<UserItemsList, KalturaUserAssetsList>()
                .ForMember(dest => dest.List, opt => opt.MapFrom(src => src.ItemsList))
                .ForMember(dest => dest.ListType, opt => opt.MapFrom(src => ConvertUserAssetsListType(src.ListType)));

            // Item to KalturaUserAssetsListItem
            Mapper.CreateMap<Item, KalturaUserAssetsListItem>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ItemId))
                .ForMember(dest => dest.OrderIndex, opt => opt.MapFrom(src => src.OrderIndex))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ConvertUserAssetsListItemType(src.ItemType)))
                .ForMember(dest => dest.ListType, opt => opt.MapFrom(src => ConvertUserAssetsListType(src.ListType)))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId));

            // Item to KalturaUserAssetsListItem
            Mapper.CreateMap<KalturaUserAssetsListItem, Item>()
                .ForMember(dest => dest.ItemId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.OrderIndex, opt => opt.MapFrom(src => src.OrderIndex))
                .ForMember(dest => dest.ItemType, opt => opt.MapFrom(src => ConvertUserAssetsListItemType(src.Type)))
                .ForMember(dest => dest.ListType, opt => opt.MapFrom(src => ConvertUserAssetsListType(src.ListType)))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId));

            // Country
            Mapper.CreateMap<int, Users.Country>()
                .ForMember(dest => dest.m_nObjecrtID, opt => opt.MapFrom(src => src));
        }

        private static Users.Country ConvertContry(KalturaCountry country, int? countryId)
        {
            Users.Country response = new Users.Country();
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

        public static ItemType ConvertUserAssetsListItemType(KalturaUserAssetsListItemType itemType)
        {
            ItemType result;

            switch (itemType)
            {
                case KalturaUserAssetsListItemType.all:
                    result = ItemType.All;
                    break;
                case KalturaUserAssetsListItemType.media:
                    result = ItemType.Media;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown user assets list item type");
            }
            return result;
        }

        private static KalturaUserAssetsListItemType ConvertUserAssetsListItemType(ItemType itemType)
        {
            KalturaUserAssetsListItemType result;

            switch (itemType)
            {
                case ItemType.Media:
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
    }
}