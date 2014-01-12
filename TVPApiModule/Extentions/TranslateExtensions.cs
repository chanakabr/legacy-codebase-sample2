using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPApiModule.Objects.Responses;
using TVPPro.SiteManager.TvinciPlatform.api;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPPro.SiteManager.TvinciPlatform.Users;

namespace TVPApiModule.Extentions
{
    public static class TranslateExtensions
    {    
        //public static UserResponseObject To(this UserResponseObject response)
        //{
        //    if (response == null)
        //        return null;

        //    UserResponseObject response = new UserResponseObject();

        //    response.m_RespStatus = (ResponseStatus)response.m_RespStatus;

        //    if (response.m_user != null)
        //    {
        //        response.m_user = new User();

        //        response.m_user.m_domianID = response.m_user.m_domianID;
        //        response.m_user.m_eUserState = (UserState)response.m_user.m_eUserState;
        //        response.m_user.m_isDomainMaster = response.m_user.m_isDomainMaster;
        //        response.m_user.m_nSSOOperatorID = response.m_user.m_nSSOOperatorID;

        //        if (response.m_user.m_oBasicData != null)
        //        {
        //            response.m_user.m_oBasicData = new UserBasicData();

        //            response.m_user.m_oBasicData.m_bIsFacebookImagePermitted = response.m_user.m_oBasicData.m_bIsFacebookImagePermitted;
        //            response.m_user.m_oBasicData.m_CoGuid = response.m_user.m_oBasicData.m_CoGuid;

        //            response.m_user.m_oBasicData.m_Country = response.m_user.m_oBasicData.m_Country.To();

        //            response.m_user.m_oBasicData.m_ExternalToken = response.m_user.m_oBasicData.m_ExternalToken;
        //            response.m_user.m_oBasicData.m_sAddress = response.m_user.m_oBasicData.m_sAddress;
        //            response.m_user.m_oBasicData.m_sAffiliateCode = response.m_user.m_oBasicData.m_sAffiliateCode;
        //            response.m_user.m_oBasicData.m_sCity = response.m_user.m_oBasicData.m_sCity;
        //            response.m_user.m_oBasicData.m_sEmail = response.m_user.m_oBasicData.m_sEmail;
        //            response.m_user.m_oBasicData.m_sFacebookID = response.m_user.m_oBasicData.m_sFacebookID;
        //            response.m_user.m_oBasicData.m_sFacebookImage = response.m_user.m_oBasicData.m_sFacebookImage;
        //            response.m_user.m_oBasicData.m_sFacebookToken = response.m_user.m_oBasicData.m_sFacebookToken;
        //            response.m_user.m_oBasicData.m_sFirstName = response.m_user.m_oBasicData.m_sFirstName;
        //            response.m_user.m_oBasicData.m_sLastName = response.m_user.m_oBasicData.m_sLastName;
        //            response.m_user.m_oBasicData.m_sPhone = response.m_user.m_oBasicData.m_sPhone;

        //            response.m_user.m_oBasicData.m_State = response.m_user.m_oBasicData.m_State.To();

        //            response.m_user.m_oBasicData.m_sUserName = response.m_user.m_oBasicData.m_sUserName;
        //            response.m_user.m_oBasicData.m_sZip = response.m_user.m_oBasicData.m_sZip;

        //            response.m_user.m_oBasicData.m_UserType = response.m_user.m_oBasicData.m_UserType.To();
        //        }

        //        if (response.m_user.m_oDynamicData != null)
        //        {
        //            response.m_user.m_oDynamicData = new UserDynamicData();

        //            if (response.m_user.m_oDynamicData.m_sUserData != null)
        //            {
        //                response.m_user.m_oDynamicData.m_sUserData = response.m_user.m_oDynamicData.m_sUserData.Select(x => new UserDynamicDataContainer()
        //                {
        //                    m_sDataType = x.m_sDataType,
        //                    m_sValue = x.m_sValue
        //                }).ToArray();
        //            }
        //        }

        //        response.m_user.m_sSiteGUID = response.m_user.m_sSiteGUID;
        //    }

        //    response.m_userInstanceID = response.m_userInstanceID;

        //    return response;
        //}

        public static SubscriptionContainer ToApiObject(this PermittedSubscriptionContainer response)
        {
            if (response == null)
                return null;

            SubscriptionContainer retVal = new SubscriptionContainer();

            retVal.m_bIsSubRenewable = response.m_bIsSubRenewable;
            retVal.m_bRecurringStatus = response.m_bRecurringStatus;
            retVal.m_dCurrentDate = response.m_dCurrentDate;
            retVal.m_dEndDate = response.m_dEndDate;
            retVal.m_dLastViewDate = response.m_dLastViewDate;
            retVal.m_dNextRenewalDate = response.m_dNextRenewalDate;
            retVal.m_dPurchaseDate = response.m_dPurchaseDate;
            retVal.m_nCurrentUses = response.m_nCurrentUses;
            retVal.m_nMaxUses = response.m_nMaxUses;
            retVal.m_nSubscriptionPurchaseID = response.m_nSubscriptionPurchaseID;
            retVal.m_paymentMethod = (TVPApiModule.Objects.Responses.PaymentMethod)response.m_paymentMethod;
            retVal.m_sDeviceName = response.m_sDeviceName;
            retVal.m_sDeviceUDID = response.m_sDeviceUDID;
            retVal.m_sSubscriptionCode = response.m_sSubscriptionCode;

            return retVal;
        }

        public static MediaContainer ToApiObject(this PermittedMediaContainer response)
        {
            if (response == null)
                return null;

            MediaContainer retVal = new MediaContainer();

            retVal.m_dCurrentDate = response.m_dCurrentDate;
            retVal.m_dEndDate = response.m_dEndDate;
            retVal.m_dPurchaseDate = response.m_dPurchaseDate;
            retVal.m_nCurrentUses = response.m_nCurrentUses;
            retVal.m_nMaxUses = response.m_nMaxUses;
            retVal.m_nMediaFileID = response.m_nMediaFileID;
            retVal.m_nMediaID = response.m_nMediaID;
            retVal.m_purchaseMethod = (TVPApiModule.Objects.Responses.PaymentMethod)response.m_purchaseMethod;
            retVal.m_sDeviceName = response.m_sDeviceName;
            retVal.m_sDeviceUDID = response.m_sDeviceUDID;

            return retVal;
        }

        public static Favorite ToApiObject(this FavoritObject response)
        {
            if (response == null)
                return null;

            Favorite retVal = new Favorite();

            retVal.m_dUpdateDate = response.m_dUpdateDate;
            retVal.m_is_channel = response.m_is_channel;
            retVal.m_nDomainID = response.m_nDomainID;
            retVal.m_nID = response.m_nID;
            retVal.m_sDeviceName = response.m_sDeviceName;
            retVal.m_sDeviceUDID = response.m_sDeviceUDID;
            retVal.m_sExtraData = response.m_sExtraData;
            retVal.m_sItemCode = response.m_sItemCode;
            retVal.m_sSiteUserGUID = response.m_sSiteUserGUID;
            retVal.m_sType = response.m_sType;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.GroupRule ToApiObject(this TVPPro.SiteManager.TvinciPlatform.api.GroupRule response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.GroupRule retVal = new TVPApiModule.Objects.Responses.GroupRule();

            retVal.AllTagValues = response.AllTagValues;
            retVal.BlockType = (TVPApiModule.Objects.Responses.eBlockType)response.BlockType;
            retVal.DynamicDataKey = response.DynamicDataKey;
            retVal.GroupRuleType = (TVPApiModule.Objects.Responses.eGroupRuleType)response.GroupRuleType;
            retVal.IsActive = response.IsActive;
            retVal.Name = response.Name;
            retVal.RuleID = response.RuleID;
            retVal.TagTypeID = response.TagTypeID;
            retVal.TagValue = response.TagValue;

            return retVal;
        }

        //public static Media To(this Media response)
        //{
        //    if (response == null)
        //        return null;

        //    Media response = new Media();

        //    if (response.AdvertisingParameters != null)
        //    {
        //        response.AdvertisingParameters = response.AdvertisingParameters.Select(x => new TagMetaPair()
        //        {
        //            Key = x.Key,
        //            Value = x.Value
        //        }).ToList();
        //    }

        //    response.CreationDate = response.CreationDate;
        //    response.Description = response.Description;
        //    response.Duration = response.Duration;

        //    if (response.ExternalIDs != null)
        //    {
        //        response.ExternalIDs = response.ExternalIDs.Select(x => new Media.ExtIDPair()
        //        {
        //            Key = x.Key,
        //            Value = x.Value
        //        }).ToList();
        //    }

        //    response.FileID = response.FileID;

        //    if (response.Files != null)
        //    {
        //        response.Files = response.Files.Select(x => new Media.File()
        //        {
        //            BreakPoints = x.BreakPoints,
        //            BreakProvider = x.BreakProvider != null ? new AdvertisingProvider()
        //            {
        //                ID = x.BreakProvider.ID,
        //                Name = x.BreakProvider.Name
        //            } :null,
        //            Duration = x.Duration,
        //            FileID = x.FileID,
        //            Format = x.Format,
        //            OverlayPoints = x.OverlayPoints,
        //            OverlayProvider = x.OverlayProvider != null ? new AdvertisingProvider()
        //            {
        //                ID = x.OverlayProvider.ID,
        //                Name = x.OverlayProvider.Name
        //            } : null,
        //            PostProvider = x.PostProvider != null ? new AdvertisingProvider()
        //            {
        //                ID = x.PostProvider.ID,
        //                Name = x.PostProvider.Name
        //            } : null,
        //            PreProvider = x.PreProvider != null ? new AdvertisingProvider()
        //            {
        //                ID = x.PreProvider.ID,
        //                Name = x.PreProvider.Name
        //            } : null
        //        }).ToList();
        //    }

        //    response.GeoBlock = response.GeoBlock;
        //    response.LastWatchDate = response.LastWatchDate;
        //    response.like_counter = response.like_counter;

        //    if (response.MediaDynamicData != null)
        //    {
        //        response.MediaDynamicData = new DynamicData();

        //        response.MediaDynamicData.ExpirationDate = response.MediaDynamicData.ExpirationDate;
        //        response.MediaDynamicData.IsFavorite = response.MediaDynamicData.IsFavorite;
        //        response.MediaDynamicData.MediaMark = response.MediaDynamicData.MediaMark;
        //        response.MediaDynamicData.Notification = response.MediaDynamicData.Notification;
        //        response.MediaDynamicData.Price = response.MediaDynamicData.Price;
        //        response.MediaDynamicData.PriceType = (PriceReason)response.MediaDynamicData.PriceType;
        //    }

        //    response.MediaID = response.MediaID;
        //    response.MediaName = response.MediaName;
        //    response.MediaTypeID = response.MediaTypeID;
        //    response.MediaTypeName = response.MediaTypeName;
        //    response.MediaWebLink = response.MediaWebLink;

        //    if (response.Metas != null)
        //    {
        //        response.Metas = response.Metas.Select(x => new TagMetaPair()
        //        {
        //            Key = x.Key,
        //            Value = x.Value
        //        }).ToList();
        //    }

        //    if (response.Pictures != null)
        //    {
        //        response.Pictures = response.Pictures.Select(x => new Media.Picture()
        //        {
        //            PicSize = x.PicSize,
        //            URL = x.URL
        //        }).ToList();
        //    }

        //    response.PicURL = response.PicURL;
        //    response.Rating = response.Rating;
        //    response.StartDate = response.StartDate;
        //    response.SubDuration = response.SubDuration;
        //    response.SubFileFormat = response.SubFileFormat;
        //    response.SubFileID = response.SubFileID;
        //    response.SubURL = response.SubURL;

        //    if (response.Tags != null)
        //    {
        //        response.Tags = response.Tags.Select(x => new TagMetaPair()
        //        {
        //            Key = x.Key,
        //            Value = x.Value
        //        }).ToList();
        //    }

        //    response.TotalItems = response.TotalItems;
        //    response.URL = response.URL;
        //    response.ViewCounter = response.ViewCounter;

        //    return response;
        //}

        //public static Comment To(this Comment response)
        //{
        //    Comment response = new Comment();

        //    response.AddedDate = response.AddedDate;
        //    response.Author = response.Author;
        //    response.Content = response.Content;
        //    response.Header = response.Header;

        //    return response;
        //}

        //public static RateMedia To(this RateMediaObject response)
        //{
        //    if (response == null)
        //        return null;

        //    RateMedia response = new RateMedia();

        //    response.nAvg = response.nAvg;
        //    response.nCount = response.nCount;
        //    response.nSum = response.nSum;

        //    if (response.oStatus != null)
        //    {
        //        response.oStatus.m_nStatusCode = response.oStatus.m_nStatusCode;
        //        response.oStatus.m_sStatusDescription = response.oStatus.m_sStatusDescription;
        //    }
            
        //    return response;
        //}

        public static MediaMark ToApiObject(this MediaMarkObject response)
        {
            if (response == null)
                return null;

            MediaMark retVal = new MediaMark();

            retVal.eStatus = (TVPApiModule.Objects.Responses.MediaMarkObjectStatus)response.eStatus;
            retVal.nGroupID = response.nGroupID;
            retVal.nLocationSec = response.nLocationSec;
            retVal.nMediaID = response.nMediaID;
            retVal.sDeviceID = response.sDeviceID;
            retVal.sDeviceName = response.sDeviceName;
            retVal.sSiteGUID = response.sSiteGUID;

            return retVal;
        }

        //public static UserItemList To(this UserItemList response)
        //{
        //    if (response == null)
        //        return null;

        //    UserItemList response = new UserItemList();

        //    if (response.itemObj != null)
        //    {
        //        response.itemObj = response.itemObj.Select(x => new ItemObj()
        //        {
        //            item = x.item,
        //            orderNum = x.orderNum
        //        }).ToArray();
        //    }

        //    response.itemType = (ItemType)response.itemType;
        //    response.listType = (ListType)response.listType;

        //    return response;
        //}

        //public static KeyValuePair<string, string> To(this KeyValuePair response)
        //{
        //    KeyValuePair<string, string> response = new KeyValuePair<string, string>(response.key, response.value);

        //    return response;
        //}

        //public static State To(this State response)
        //{
        //    if (response == null)
        //        return null;

        //    State response = new State();

        //    response.m_Country = response.m_Country.To();

        //    response.m_nObjecrtID = response.m_nObjecrtID;
        //    response.m_sStateCode = response.m_sStateCode;
        //    response.m_sStateName = response.m_sStateName;

        //    return response;
        //}

        //public static Country To(this Country response)
        //{
        //    if (response == null)
        //        return null;

        //    Country response = new Country();

        //    response.m_nObjecrtID = response.m_nObjecrtID;
        //    response.m_sCountryCode = response.m_sCountryCode;
        //    response.m_sCountryName = response.m_sCountryName;

        //    return response;
        //}

        //public static UserType To(this TVPPro.SiteManager.TvinciPlatform.Users.UserType response)
        //{
        //    if (response == null)
        //        return null;

        //    UserType response = new UserType();

        //    response.Description = response.Description;
        //    response.ID = response.ID;
        //    response.IsDefault = response.IsDefault;

        //    return response;
        //}

        public static TVPApiModule.Objects.Responses.EPGChannel ToApiObject(this TVPPro.SiteManager.TvinciPlatform.api.EPGChannelObject response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.EPGChannel retVal = new TVPApiModule.Objects.Responses.EPGChannel();

            retVal.CHANNEL_ID = response.CHANNEL_ID;
            retVal.CREATE_DATE = response.CREATE_DATE;
            retVal.DESCRIPTION = response.DESCRIPTION;
            retVal.EDITOR_REMARKS = response.EDITOR_REMARKS;
            retVal.EPG_CHANNEL_ID = response.EPG_CHANNEL_ID;
            retVal.GROUP_ID = response.GROUP_ID;
            retVal.IS_ACTIVE = response.IS_ACTIVE;
            retVal.MEDIA_ID = response.MEDIA_ID;
            retVal.NAME = response.NAME;
            retVal.ORDER_NUM = response.ORDER_NUM;
            retVal.PIC_URL = response.PIC_URL;
            retVal.PUBLISH_DATE = response.PUBLISH_DATE;
            retVal.STATUS = response.STATUS;
            retVal.UPDATER_ID = response.UPDATER_ID;

            return retVal;
        }

        //public static EPGComment To(this EPGComment response)
        //{
        //    if (response == null)
        //        return null;

        //    EPGComment response = new EPGComment();

        //    response.ContentText = response.ContentText;
        //    response.CreateDate = response.CreateDate;
        //    response.EPGProgramID = response.EPGProgramID;
        //    response.Header = response.Header;
        //    response.ID = response.ID;
        //    response.Language = response.Language;
        //    response.LanguageName = response.LanguageName;
        //    response.Writer = response.Writer;

        //    return response;
        //}

        public static EPGMultiChannelProgramme ToApiObject(this EPGMultiChannelProgrammeObject response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.EPGMultiChannelProgramme retVal = new TVPApiModule.Objects.Responses.EPGMultiChannelProgramme();

            retVal.EPG_CHANNEL_ID = response.EPG_CHANNEL_ID;

            if (response.EPGChannelProgrammeObject != null)
            {
                retVal.EPGChannelProgrammeObject = response.EPGChannelProgrammeObject.Select(x => x.ToApiObject()).ToArray();
            }

            return retVal;
        }

        public static EPGChannelProgramme ToApiObject(this EPGChannelProgrammeObject response)
        {
            if (response == null)
                return null;

            EPGChannelProgramme retVal = new EPGChannelProgramme();

            retVal.CREATE_DATE = response.CREATE_DATE;
            retVal.DESCRIPTION = response.DESCRIPTION;
            retVal.END_DATE = response.END_DATE;
            retVal.EPG_CHANNEL_ID = response.EPG_CHANNEL_ID;
            retVal.EPG_ID = response.EPG_ID;
            retVal.EPG_IDENTIFIER = response.EPG_IDENTIFIER;

            if (response.EPG_Meta != null)
            {
                response.EPG_Meta.Select(x => x.ToApiObject()).ToArray();
            }

            if (response.EPG_TAGS != null)
            {
                response.EPG_TAGS.Select(x => x.ToApiObject()).ToArray();
            }

            retVal.GROUP_ID = response.GROUP_ID;
            retVal.IS_ACTIVE = response.IS_ACTIVE;
            retVal.LIKE_COUNTER = response.LIKE_COUNTER;
            retVal.media_id = response.media_id;
            retVal.NAME = response.NAME;
            retVal.PIC_URL = response.PIC_URL;
            retVal.PUBLISH_DATE = response.PUBLISH_DATE;
            retVal.START_DATE = response.START_DATE;
            retVal.STATUS = response.STATUS;
            retVal.UPDATE_DATE = response.UPDATE_DATE;
            retVal.UPDATER_ID = response.UPDATER_ID;

            return retVal;
        }

        public static EPGChannelProgramme ToApiObject(this Tvinci.Data.Loaders.TvinciPlatform.Catalog.EPGChannelProgrammeObject response)
        {
            if (response == null)
                return null;

            EPGChannelProgramme retVal = new EPGChannelProgramme();

            retVal.CREATE_DATE = response.CREATE_DATE;
            retVal.DESCRIPTION = response.DESCRIPTION;
            retVal.END_DATE = response.END_DATE;
            retVal.EPG_CHANNEL_ID = response.EPG_CHANNEL_ID;
            retVal.EPG_ID = response.EPG_ID;
            retVal.EPG_IDENTIFIER = response.EPG_IDENTIFIER;

            if (response.EPG_Meta != null)
            {
                response.EPG_Meta.Select(x => x.ToApiObject()).ToArray();
            }

            if (response.EPG_TAGS != null)
            {
                response.EPG_TAGS.Select(x => x.ToApiObject()).ToArray();
            }

            retVal.GROUP_ID = response.GROUP_ID;
            retVal.IS_ACTIVE = response.IS_ACTIVE;
            retVal.LIKE_COUNTER = response.LIKE_COUNTER;
            retVal.media_id = response.media_id;
            retVal.NAME = response.NAME;
            retVal.PIC_URL = response.PIC_URL;
            retVal.PUBLISH_DATE = response.PUBLISH_DATE;
            retVal.START_DATE = response.START_DATE;
            retVal.STATUS = response.STATUS;
            retVal.UPDATE_DATE = response.UPDATE_DATE;
            retVal.UPDATER_ID = response.UPDATER_ID;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.EPGDictionary ToApiObject(this TVPPro.SiteManager.TvinciPlatform.api.EPGDictionary response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.EPGDictionary retVal = new TVPApiModule.Objects.Responses.EPGDictionary();

            retVal.Key = response.Key;
            retVal.Value = response.Value;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.EPGDictionary ToApiObject(this Tvinci.Data.Loaders.TvinciPlatform.Catalog.EPGDictionary response)
        {
            if (response.Key == null && response.Value == null)
                return null;

            TVPApiModule.Objects.Responses.EPGDictionary retVal = new TVPApiModule.Objects.Responses.EPGDictionary();

            retVal.Key = response.Key;
            retVal.Value = response.Value;

            return retVal;
        }

        //public static Channel To(this Channel response)
        //{
        //    if (response == null)
        //        return null;

        //    Channel response = new Channel();

        //    response.ChannelID = response.ChannelID; ;
        //    response.MediaCount = response.MediaCount;
        //    response.PicURL = response.PicURL;
        //    response.Title = response.Title;

        //    return response;
        //}

        //public static Category To(this Category response)
        //{
        //    if (response == null)
        //        return null;

        //    Category response = new Category();

        //    if (response.Channels != null)
        //    {
        //        response.Channels = response.Channels.Select(x => x.To()).ToList();
        //    }

        //    response.ID = response.ID;

        //    if (response.InnerCategories != null)
        //    {
        //        response.InnerCategories = response.InnerCategories.Select(x => x.To()).ToList();
        //    }

        //    response.PicURL = response.PicURL;
        //    response.Title = response.Title;

        //    return response;
        //}

        

        public static TVPApiModule.Objects.Responses.BillingResponse ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.BillingResponse response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.BillingResponse retVal = new TVPApiModule.Objects.Responses.BillingResponse();

            retVal.m_oStatus = (TVPApiModule.Objects.Responses.BillingResponseStatus)response.m_oStatus;
            retVal.m_sExternalReceiptCode = response.m_sExternalReceiptCode;
            retVal.m_sRecieptCode = response.m_sRecieptCode;
            retVal.m_sStatusDescription = response.m_sStatusDescription;
            
            return retVal;
        }

        public static TVPApiModule.Objects.Responses.Currency ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.Currency response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.Currency retVal = new TVPApiModule.Objects.Responses.Currency();

            retVal.m_nCurrencyID = response.m_nCurrencyID;
            retVal.m_sCurrencyCD2 = response.m_sCurrencyCD2;
            retVal.m_sCurrencyCD3 = response.m_sCurrencyCD3;
            retVal.m_sCurrencySign = response.m_sCurrencySign;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.Price ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.Price response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.Price retVal = new TVPApiModule.Objects.Responses.Price();

            retVal.m_dPrice = response.m_dPrice;
            retVal.m_oCurrency = response.m_oCurrency.ToApiObject();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.BillingTransactionContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.BillingTransactionContainer response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.BillingTransactionContainer retVal = new TVPApiModule.Objects.Responses.BillingTransactionContainer();

            retVal.m_bIsRecurring = response.m_bIsRecurring;
            retVal.m_dtActionDate = response.m_dtActionDate;
            retVal.m_dtEndDate = response.m_dtEndDate;
            retVal.m_dtStartDate = response.m_dtStartDate;
            retVal.m_eBillingAction = (TVPApiModule.Objects.Responses.BillingAction)response.m_eBillingAction;
            retVal.m_eItemType = (TVPApiModule.Objects.Responses.BillingItemsType)response.m_eItemType;
            retVal.m_ePaymentMethod = (TVPApiModule.Objects.Responses.PaymentMethod)response.m_ePaymentMethod;
            retVal.m_nBillingProviderRef = response.m_nBillingProviderRef;
            retVal.m_nPurchaseID = response.m_nPurchaseID;
            retVal.m_Price = response.m_Price.ToApiObject();
            retVal.m_sPaymentMethodExtraDetails = response.m_sPaymentMethodExtraDetails;
            retVal.m_sPurchasedItemCode = response.m_sPurchasedItemCode;
            retVal.m_sPurchasedItemName = response.m_sPurchasedItemName;
            retVal.m_sRecieptCode = response.m_sRecieptCode;
            retVal.m_sRemarks = response.m_sRemarks; 

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.BillingTransactionsResponse ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.BillingTransactionsResponse response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.BillingTransactionsResponse retVal = new TVPApiModule.Objects.Responses.BillingTransactionsResponse();

            retVal.m_nTransactionsCount = response.m_nTransactionsCount;
            retVal.m_Transactions = response.m_Transactions.Select(t => t.ToApiObject()).ToArray();

            return retVal;
        }
    }
}
