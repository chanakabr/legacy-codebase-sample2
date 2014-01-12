using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using TVPPro.SiteManager.Objects;
using TVPPro.SiteManager.TvinciPlatform.api;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPPro.SiteManager.TvinciPlatform.Users;

namespace RestfulTVPApi.ServiceModel
{
    public static class TranslateExtensions
    {
        public static UserResponseObjectDTO ToDto(this UserResponseObject response)
        {
            if (response == null)
                return null;

            UserResponseObjectDTO responseDTO = new UserResponseObjectDTO();

            responseDTO.m_RespStatus = (ResponseStatusDTO)response.m_RespStatus;

            if (response.m_user != null)
            {
                responseDTO.m_user = new UserDTO();

                responseDTO.m_user.m_domianID = response.m_user.m_domianID;
                responseDTO.m_user.m_eUserState = (UserStateDTO)response.m_user.m_eUserState;
                responseDTO.m_user.m_isDomainMaster = response.m_user.m_isDomainMaster;
                responseDTO.m_user.m_nSSOOperatorID = response.m_user.m_nSSOOperatorID;

                if (response.m_user.m_oBasicData != null)
                {
                    responseDTO.m_user.m_oBasicData = new UserBasicDataDTO();

                    responseDTO.m_user.m_oBasicData.m_bIsFacebookImagePermitted = response.m_user.m_oBasicData.m_bIsFacebookImagePermitted;
                    responseDTO.m_user.m_oBasicData.m_CoGuid = response.m_user.m_oBasicData.m_CoGuid;

                    responseDTO.m_user.m_oBasicData.m_Country = response.m_user.m_oBasicData.m_Country.ToDto();

                    responseDTO.m_user.m_oBasicData.m_ExternalToken = response.m_user.m_oBasicData.m_ExternalToken;
                    responseDTO.m_user.m_oBasicData.m_sAddress = response.m_user.m_oBasicData.m_sAddress;
                    responseDTO.m_user.m_oBasicData.m_sAffiliateCode = response.m_user.m_oBasicData.m_sAffiliateCode;
                    responseDTO.m_user.m_oBasicData.m_sCity = response.m_user.m_oBasicData.m_sCity;
                    responseDTO.m_user.m_oBasicData.m_sEmail = response.m_user.m_oBasicData.m_sEmail;
                    responseDTO.m_user.m_oBasicData.m_sFacebookID = response.m_user.m_oBasicData.m_sFacebookID;
                    responseDTO.m_user.m_oBasicData.m_sFacebookImage = response.m_user.m_oBasicData.m_sFacebookImage;
                    responseDTO.m_user.m_oBasicData.m_sFacebookToken = response.m_user.m_oBasicData.m_sFacebookToken;
                    responseDTO.m_user.m_oBasicData.m_sFirstName = response.m_user.m_oBasicData.m_sFirstName;
                    responseDTO.m_user.m_oBasicData.m_sLastName = response.m_user.m_oBasicData.m_sLastName;
                    responseDTO.m_user.m_oBasicData.m_sPhone = response.m_user.m_oBasicData.m_sPhone;

                    responseDTO.m_user.m_oBasicData.m_State = response.m_user.m_oBasicData.m_State.ToDto();

                    responseDTO.m_user.m_oBasicData.m_sUserName = response.m_user.m_oBasicData.m_sUserName;
                    responseDTO.m_user.m_oBasicData.m_sZip = response.m_user.m_oBasicData.m_sZip;

                    responseDTO.m_user.m_oBasicData.m_UserType = response.m_user.m_oBasicData.m_UserType.ToDto();
                }

                if (response.m_user.m_oDynamicData != null)
                {
                    responseDTO.m_user.m_oDynamicData = new UserDynamicDataDTO();

                    if (response.m_user.m_oDynamicData.m_sUserData != null)
                    {
                        responseDTO.m_user.m_oDynamicData.m_sUserData = response.m_user.m_oDynamicData.m_sUserData.Select(x => new UserDynamicDataContainerDTO()
                        {
                            m_sDataType = x.m_sDataType,
                            m_sValue = x.m_sValue
                        }).ToArray();
                    }
                }

                responseDTO.m_user.m_sSiteGUID = response.m_user.m_sSiteGUID;
            }

            responseDTO.m_userInstanceID = response.m_userInstanceID;

            return responseDTO;
        }

        public static SubscriptionContainerDTO ToDto(this PermittedSubscriptionContainer response)
        {
            if (response == null)
                return null;

            SubscriptionContainerDTO responseDTO = new SubscriptionContainerDTO();

            responseDTO.m_bIsSubRenewable = response.m_bIsSubRenewable;
            responseDTO.m_bRecurringStatus = response.m_bRecurringStatus;
            responseDTO.m_dCurrentDate = response.m_dCurrentDate;
            responseDTO.m_dEndDate = response.m_dEndDate;
            responseDTO.m_dLastViewDate = response.m_dLastViewDate;
            responseDTO.m_dNextRenewalDate = response.m_dNextRenewalDate;
            responseDTO.m_dPurchaseDate = response.m_dPurchaseDate;
            responseDTO.m_nCurrentUses = response.m_nCurrentUses;
            responseDTO.m_nMaxUses = response.m_nMaxUses;
            responseDTO.m_nSubscriptionPurchaseID = response.m_nSubscriptionPurchaseID;
            responseDTO.m_paymentMethod = (PaymentMethodDTO)response.m_paymentMethod;
            responseDTO.m_sDeviceName = response.m_sDeviceName;
            responseDTO.m_sDeviceUDID = response.m_sDeviceUDID;
            responseDTO.m_sSubscriptionCode = response.m_sSubscriptionCode;

            return responseDTO;
        }

        public static MediaContainerDTO ToDto(this PermittedMediaContainer response)
        {
            if (response == null)
                return null;

            MediaContainerDTO responseDTO = new MediaContainerDTO();

            responseDTO.m_dCurrentDate = response.m_dCurrentDate;
            responseDTO.m_dEndDate = response.m_dEndDate;
            responseDTO.m_dPurchaseDate = response.m_dPurchaseDate;
            responseDTO.m_nCurrentUses = response.m_nCurrentUses;
            responseDTO.m_nMaxUses = response.m_nMaxUses;
            responseDTO.m_nMediaFileID = response.m_nMediaFileID;
            responseDTO.m_nMediaID = response.m_nMediaID;
            responseDTO.m_purchaseMethod = (PaymentMethodDTO)response.m_purchaseMethod;
            responseDTO.m_sDeviceName = response.m_sDeviceName;
            responseDTO.m_sDeviceUDID = response.m_sDeviceUDID;

            return responseDTO;
        }

        public static FavoriteDTO ToDto(this FavoritObject response)
        {
            if (response == null)
                return null;

            FavoriteDTO responseDTO = new FavoriteDTO();

            responseDTO.m_dUpdateDate = response.m_dUpdateDate;
            responseDTO.m_is_channel = response.m_is_channel;
            responseDTO.m_nDomainID = response.m_nDomainID;
            responseDTO.m_nID = response.m_nID;
            responseDTO.m_sDeviceName = response.m_sDeviceName;
            responseDTO.m_sDeviceUDID = response.m_sDeviceUDID;
            responseDTO.m_sExtraData = response.m_sExtraData;
            responseDTO.m_sItemCode = response.m_sItemCode;
            responseDTO.m_sSiteUserGUID = response.m_sSiteUserGUID;
            responseDTO.m_sType = response.m_sType;

            return responseDTO;
        }

        public static GroupRuleDTO ToDto(this GroupRule response)
        {
            if (response == null)
                return null;

            GroupRuleDTO responseDTO = new GroupRuleDTO();

            responseDTO.AllTagValues = response.AllTagValues;
            responseDTO.BlockType = (eBlockTypeDTO)response.BlockType;
            responseDTO.DynamicDataKey = response.DynamicDataKey;
            responseDTO.GroupRuleType = (eGroupRuleTypeDTO)response.GroupRuleType;
            responseDTO.IsActive = response.IsActive;
            responseDTO.Name = response.Name;
            responseDTO.RuleID = response.RuleID;
            responseDTO.TagTypeID = response.TagTypeID;
            responseDTO.TagValue = response.TagValue;

            return responseDTO;
        }

        public static MediaDTO ToDto(this Media response)
        {
            if (response == null)
                return null;

            MediaDTO responseDTO = new MediaDTO();

            if (response.AdvertisingParameters != null)
            {
                responseDTO.AdvertisingParameters = response.AdvertisingParameters.Select(x => new TagMetaPairDTO()
                {
                    Key = x.Key,
                    Value = x.Value
                }).ToList();
            }

            responseDTO.CreationDate = response.CreationDate;
            responseDTO.Description = response.Description;
            responseDTO.Duration = response.Duration;

            if (response.ExternalIDs != null)
            {
                responseDTO.ExternalIDs = response.ExternalIDs.Select(x => new MediaDTO.ExtIDPairDTO()
                {
                    Key = x.Key,
                    Value = x.Value
                }).ToList();
            }

            responseDTO.FileID = response.FileID;

            if (response.Files != null)
            {
                responseDTO.Files = response.Files.Select(x => new MediaDTO.FileDTO()
                {
                    BreakPoints = x.BreakPoints,
                    BreakProvider = x.BreakProvider != null ? new AdvertisingProviderDTO()
                    {
                        ID = x.BreakProvider.ID,
                        Name = x.BreakProvider.Name
                    } :null,
                    Duration = x.Duration,
                    FileID = x.FileID,
                    Format = x.Format,
                    OverlayPoints = x.OverlayPoints,
                    OverlayProvider = x.OverlayProvider != null ? new AdvertisingProviderDTO()
                    {
                        ID = x.OverlayProvider.ID,
                        Name = x.OverlayProvider.Name
                    } : null,
                    PostProvider = x.PostProvider != null ? new AdvertisingProviderDTO()
                    {
                        ID = x.PostProvider.ID,
                        Name = x.PostProvider.Name
                    } : null,
                    PreProvider = x.PreProvider != null ? new AdvertisingProviderDTO()
                    {
                        ID = x.PreProvider.ID,
                        Name = x.PreProvider.Name
                    } : null
                }).ToList();
            }

            responseDTO.GeoBlock = response.GeoBlock;
            responseDTO.LastWatchDate = response.LastWatchDate;
            responseDTO.like_counter = response.like_counter;

            if (response.MediaDynamicData != null)
            {
                responseDTO.MediaDynamicData = new DynamicDataDTO();

                responseDTO.MediaDynamicData.ExpirationDate = response.MediaDynamicData.ExpirationDate;
                responseDTO.MediaDynamicData.IsFavorite = response.MediaDynamicData.IsFavorite;
                responseDTO.MediaDynamicData.MediaMark = response.MediaDynamicData.MediaMark;
                responseDTO.MediaDynamicData.Notification = response.MediaDynamicData.Notification;
                responseDTO.MediaDynamicData.Price = response.MediaDynamicData.Price;
                responseDTO.MediaDynamicData.PriceType = (PriceReasonDTO)response.MediaDynamicData.PriceType;
            }

            responseDTO.MediaID = response.MediaID;
            responseDTO.MediaName = response.MediaName;
            responseDTO.MediaTypeID = response.MediaTypeID;
            responseDTO.MediaTypeName = response.MediaTypeName;
            responseDTO.MediaWebLink = response.MediaWebLink;

            if (response.Metas != null)
            {
                responseDTO.Metas = response.Metas.Select(x => new TagMetaPairDTO()
                {
                    Key = x.Key,
                    Value = x.Value
                }).ToList();
            }

            if (response.Pictures != null)
            {
                responseDTO.Pictures = response.Pictures.Select(x => new MediaDTO.PictureDTO()
                {
                    PicSize = x.PicSize,
                    URL = x.URL
                }).ToList();
            }

            responseDTO.PicURL = response.PicURL;
            responseDTO.Rating = response.Rating;
            responseDTO.StartDate = response.StartDate;
            responseDTO.SubDuration = response.SubDuration;
            responseDTO.SubFileFormat = response.SubFileFormat;
            responseDTO.SubFileID = response.SubFileID;
            responseDTO.SubURL = response.SubURL;

            if (response.Tags != null)
            {
                responseDTO.Tags = response.Tags.Select(x => new TagMetaPairDTO()
                {
                    Key = x.Key,
                    Value = x.Value
                }).ToList();
            }

            responseDTO.TotalItems = response.TotalItems;
            responseDTO.URL = response.URL;
            responseDTO.ViewCounter = response.ViewCounter;

            return responseDTO;
        }

        public static CommentDTO ToDto(this Comment response)
        {
            CommentDTO responseDTO = new CommentDTO();

            responseDTO.AddedDate = response.AddedDate;
            responseDTO.Author = response.Author;
            responseDTO.Content = response.Content;
            responseDTO.Header = response.Header;

            return responseDTO;
        }

        public static RateMediaDTO ToDto(this RateMediaObject response)
        {
            if (response == null)
                return null;

            RateMediaDTO responseDTO = new RateMediaDTO();

            responseDTO.nAvg = response.nAvg;
            responseDTO.nCount = response.nCount;
            responseDTO.nSum = response.nSum;

            if (response.oStatus != null)
            {
                responseDTO.oStatus.m_nStatusCode = response.oStatus.m_nStatusCode;
                responseDTO.oStatus.m_sStatusDescription = response.oStatus.m_sStatusDescription;
            }
            
            return responseDTO;
        }

        public static MediaMarkDTO ToDto(this MediaMarkObject response)
        {
            if (response == null)
                return null;

            MediaMarkDTO responseDTO = new MediaMarkDTO();

            responseDTO.eStatus = (MediaMarkObjectStatusDTO)response.eStatus;
            responseDTO.nGroupID = response.nGroupID;
            responseDTO.nLocationSec = response.nLocationSec;
            responseDTO.nMediaID = response.nMediaID;
            responseDTO.sDeviceID = response.sDeviceID;
            responseDTO.sDeviceName = response.sDeviceName;
            responseDTO.sSiteGUID = response.sSiteGUID;

            return responseDTO;
        }

        public static UserItemListDTO ToDto(this UserItemList response)
        {
            if (response == null)
                return null;

            UserItemListDTO responseDTO = new UserItemListDTO();

            if (response.itemObj != null)
            {
                responseDTO.itemObj = response.itemObj.Select(x => new ItemObjDTO()
                {
                    item = x.item,
                    orderNum = x.orderNum
                }).ToArray();
            }

            responseDTO.itemType = (ItemTypeDTO)response.itemType;
            responseDTO.listType = (ListTypeDTO)response.listType;

            return responseDTO;
        }

        public static KeyValuePair<string, string> ToDto(this KeyValuePair response)
        {
            KeyValuePair<string, string> responseDTO = new KeyValuePair<string, string>(response.key, response.value);

            return responseDTO;
        }

        public static StateDTO ToDto(this State response)
        {
            if (response == null)
                return null;

            StateDTO responseDTO = new StateDTO();

            responseDTO.m_Country = response.m_Country.ToDto();

            responseDTO.m_nObjecrtID = response.m_nObjecrtID;
            responseDTO.m_sStateCode = response.m_sStateCode;
            responseDTO.m_sStateName = response.m_sStateName;

            return responseDTO;
        }

        public static CountryDTO ToDto(this Country response)
        {
            if (response == null)
                return null;

            CountryDTO responseDTO = new CountryDTO();

            responseDTO.m_nObjecrtID = response.m_nObjecrtID;
            responseDTO.m_sCountryCode = response.m_sCountryCode;
            responseDTO.m_sCountryName = response.m_sCountryName;

            return responseDTO;
        }

        public static UserTypeDTO ToDto(this TVPPro.SiteManager.TvinciPlatform.Users.UserType response)
        {
            if (response == null)
                return null;

            UserTypeDTO responseDTO = new UserTypeDTO();

            responseDTO.Description = response.Description;
            responseDTO.ID = response.ID;
            responseDTO.IsDefault = response.IsDefault;

            return responseDTO;
        }

        public static EPGChannelObjectDTO ToDto(this EPGChannelObject response)
        {
            if (response == null)
                return null;

            EPGChannelObjectDTO responseDTO = new EPGChannelObjectDTO();

            responseDTO.CHANNEL_ID = response.CHANNEL_ID;
            responseDTO.CREATE_DATE = response.CREATE_DATE;
            responseDTO.DESCRIPTION = response.DESCRIPTION;
            responseDTO.EDITOR_REMARKS = response.EDITOR_REMARKS;
            responseDTO.EPG_CHANNEL_ID = response.EPG_CHANNEL_ID;
            responseDTO.GROUP_ID = response.GROUP_ID;
            responseDTO.IS_ACTIVE = response.IS_ACTIVE;
            responseDTO.MEDIA_ID = response.MEDIA_ID;
            responseDTO.NAME = response.NAME;
            responseDTO.ORDER_NUM = response.ORDER_NUM;
            responseDTO.PIC_URL = response.PIC_URL;
            responseDTO.PUBLISH_DATE = response.PUBLISH_DATE;
            responseDTO.STATUS = response.STATUS;
            responseDTO.UPDATER_ID = response.UPDATER_ID;

            return responseDTO;
        }

        public static EPGCommentDTO ToDto(this EPGComment response)
        {
            if (response == null)
                return null;

            EPGCommentDTO responseDTO = new EPGCommentDTO();

            responseDTO.ContentText = response.ContentText;
            responseDTO.CreateDate = response.CreateDate;
            responseDTO.EPGProgramID = response.EPGProgramID;
            responseDTO.Header = response.Header;
            responseDTO.ID = response.ID;
            responseDTO.Language = response.Language;
            responseDTO.LanguageName = response.LanguageName;
            responseDTO.Writer = response.Writer;

            return responseDTO;
        }

        public static EPGMultiChannelProgrammeObjectDTO ToDto(this EPGMultiChannelProgrammeObject response)
        {
            if (response == null)
                return null;

            EPGMultiChannelProgrammeObjectDTO responseDTO = new EPGMultiChannelProgrammeObjectDTO();

            responseDTO.EPG_CHANNEL_ID = response.EPG_CHANNEL_ID;

            if (response.EPGChannelProgrammeObject != null)
            {
                responseDTO.EPGChannelProgrammeObject = response.EPGChannelProgrammeObject.Select(x => x.ToDto()).ToArray();
            }

            return responseDTO;
        }

        public static EPGChannelProgrammeObjectDTO ToDto(this EPGChannelProgrammeObject response)
        {
            if (response == null)
                return null;

            EPGChannelProgrammeObjectDTO responseDTO = new EPGChannelProgrammeObjectDTO();

            responseDTO.CREATE_DATE = response.CREATE_DATE;
            responseDTO.DESCRIPTION = response.DESCRIPTION;
            responseDTO.END_DATE = response.END_DATE;
            responseDTO.EPG_CHANNEL_ID = response.EPG_CHANNEL_ID;
            responseDTO.EPG_ID = response.EPG_ID;
            responseDTO.EPG_IDENTIFIER = response.EPG_IDENTIFIER;

            if (response.EPG_Meta != null)
            {
                response.EPG_Meta.Select(x => x.ToDto()).ToArray();
            }

            if (response.EPG_TAGS != null)
            {
                response.EPG_TAGS.Select(x => x.ToDto()).ToArray();
            }

            responseDTO.GROUP_ID = response.GROUP_ID;
            responseDTO.IS_ACTIVE = response.IS_ACTIVE;
            responseDTO.LIKE_COUNTER = response.LIKE_COUNTER;
            responseDTO.media_id = response.media_id;
            responseDTO.NAME = response.NAME;
            responseDTO.PIC_URL = response.PIC_URL;
            responseDTO.PUBLISH_DATE = response.PUBLISH_DATE;
            responseDTO.START_DATE = response.START_DATE;
            responseDTO.STATUS = response.STATUS;
            responseDTO.UPDATE_DATE = response.UPDATE_DATE;
            responseDTO.UPDATER_ID = response.UPDATER_ID;

            return responseDTO;
        }

        public static EPGDictionaryDTO ToDto(this EPGDictionary response)
        {
            if (response == null)
                return null;

            EPGDictionaryDTO responseDTO = new EPGDictionaryDTO();

            responseDTO.Key = response.Key;
            responseDTO.Value = response.Value;

            return responseDTO;
        }

        public static ChannelDTO ToDto(this Channel response)
        {
            if (response == null)
                return null;

            ChannelDTO responseDTO = new ChannelDTO();

            responseDTO.ChannelID = response.ChannelID; ;
            responseDTO.MediaCount = response.MediaCount;
            responseDTO.PicURL = response.PicURL;
            responseDTO.Title = response.Title;

            return responseDTO;
        }

        public static CategoryDTO ToDto(this Category response)
        {
            if (response == null)
                return null;

            CategoryDTO responseDTO = new CategoryDTO();

            if (response.Channels != null)
            {
                responseDTO.Channels = response.Channels.Select(x => x.ToDto()).ToList();
            }

            responseDTO.ID = response.ID;

            if (response.InnerCategories != null)
            {
                responseDTO.InnerCategories = response.InnerCategories.Select(x => x.ToDto()).ToList();
            }

            responseDTO.PicURL = response.PicURL;
            responseDTO.Title = response.Title;

            return responseDTO;
        }

        public static SubscriptionPriceDTO ToDto(this SubscriptionPrice response)
        {
            if (response == null)
                return null;

            SubscriptionPriceDTO responseDTO = new SubscriptionPriceDTO();

            responseDTO.Currency = response.Currency;
            responseDTO.Price = response.Price;
            responseDTO.SubscriptionCode = response.SubscriptionCode;

            return responseDTO;
        }

        public static BillingTransactionsResponseDTO ToDto(this BillingTransactionsResponse response)
        {
            if (response == null)
                return null;

            BillingTransactionsResponseDTO responseDTO = new BillingTransactionsResponseDTO();

            if (response.m_Transactions != null)
            {
                responseDTO.m_Transactions = response.m_Transactions.Select(x => x.ToDto()).ToArray();
            }

            responseDTO.m_nTransactionsCount = response.m_nTransactionsCount;

            return responseDTO;
        }

        public static BillingTransactionContainerDTO ToDto(this BillingTransactionContainer response)
        {
            if (response == null)
                return null;

            BillingTransactionContainerDTO responseDTO = new BillingTransactionContainerDTO();

            //responseDTO.m_bIsRecurring = response.m_bIsRecurring;
            //responseDTO.m_dtActionDate = response.m_dtActionDate;
            //responseDTO.m_dtEndDate = response.m_dtEndDate;
            //responseDTO.m_dtStartDate = response.m_dtStartDate;
            //responseDTO.m_eBillingAction = (BillingActionDTO)response.m_eBillingAction;
            //responseDTO.m_eItemType = (BillingItemsTypeDTO)response.m_eItemType;
            //responseDTO.m_ePaymentMethod = response.m_ePaymentMethod.ToDto();
            //responseDTO.m_nBillingProviderRef = response.m_nBillingProviderRef;
            //responseDTO.m_nPurchaseID = response.m_nPurchaseID;
            //responseDTO. = response.m_bIsRecurring;
            //responseDTO.m_bIsRecurring = response.m_bIsRecurring;

            return responseDTO;
        }
    }
}