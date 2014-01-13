using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPApiModule.Objects.Responses;
//using TVPApiModule.Objects.Responses;

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

        public static TVPApiModule.Objects.Responses.PermittedSubscriptionContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PermittedSubscriptionContainer response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.PermittedSubscriptionContainer retVal = new TVPApiModule.Objects.Responses.PermittedSubscriptionContainer();

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

        public static TVPApiModule.Objects.Responses.PermittedMediaContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PermittedMediaContainer response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.PermittedMediaContainer retVal = new TVPApiModule.Objects.Responses.PermittedMediaContainer();

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

        public static TVPApiModule.Objects.Responses.FavoriteObject ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Users.FavoritObject response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.FavoriteObject retVal = new TVPApiModule.Objects.Responses.FavoriteObject();

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

        public static TVPApiModule.Objects.Responses.MediaMarkObject ToApiObject(this TVPPro.SiteManager.TvinciPlatform.api.MediaMarkObject response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.MediaMarkObject retVal = new TVPApiModule.Objects.Responses.MediaMarkObject();

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

        public static TVPApiModule.Objects.Responses.EPGMultiChannelProgrammeObject ToApiObject(this TVPPro.SiteManager.TvinciPlatform.api.EPGMultiChannelProgrammeObject response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.EPGMultiChannelProgrammeObject retVal = new TVPApiModule.Objects.Responses.EPGMultiChannelProgrammeObject();

            retVal.EPG_CHANNEL_ID = response.EPG_CHANNEL_ID;

            if (response.EPGChannelProgrammeObject != null)
            {
                retVal.EPGChannelProgrammeObject = response.EPGChannelProgrammeObject.Select(x => x.ToApiObject()).ToArray();
            }

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.EPGChannelProgrammeObject ToApiObject(this TVPPro.SiteManager.TvinciPlatform.api.EPGChannelProgrammeObject response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.EPGChannelProgrammeObject retVal = new TVPApiModule.Objects.Responses.EPGChannelProgrammeObject();

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

        public static TVPApiModule.Objects.Responses.EPGChannelProgrammeObject ToApiObject(this Tvinci.Data.Loaders.TvinciPlatform.Catalog.EPGChannelProgrammeObject response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.EPGChannelProgrammeObject retVal = new TVPApiModule.Objects.Responses.EPGChannelProgrammeObject();

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

        public static TVPApiModule.Objects.Responses.AdyenBillingDetail ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Billing.AdyenBillingDetail response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.AdyenBillingDetail retVal = new TVPApiModule.Objects.Responses.AdyenBillingDetail();

            retVal.billingInfo = response.billingInfo.ToApiObject();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.BillingInfo ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Billing.BillingInfo response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.BillingInfo retVal = new TVPApiModule.Objects.Responses.BillingInfo();

            retVal.cvc = response.cvc;
            retVal.expiryMonth = response.expiryMonth;
            retVal.expiryYear = response.expiryYear;
            retVal.holderName = response.holderName;
            retVal.lastFourDigits = response.lastFourDigits;
            retVal.variant = response.variant;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.CampaignActionInfo ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.CampaignActionInfo response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.CampaignActionInfo retVal = new TVPApiModule.Objects.Responses.CampaignActionInfo();

            retVal.m_mediaID = response.m_mediaID;
            retVal.m_siteGuid = response.m_siteGuid;
            retVal.m_mediaLink = response.m_mediaLink;
            retVal.m_senderName = response.m_senderName;
            retVal.m_senderEmail = response.m_senderEmail;
            retVal.m_status = (Objects.Responses.CampaignActionResult)response.m_status;
            retVal.m_socialInviteInfo = response.m_socialInviteInfo.ToApiObject();
            retVal.m_voucherReceipents = response.m_voucherReceipents.Select(vr => vr.ToApiObject()).ToArray(); 

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.VoucherReceipentInfo ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.VoucherReceipentInfo response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.VoucherReceipentInfo retVal = new TVPApiModule.Objects.Responses.VoucherReceipentInfo();

            retVal.m_emailAdd = response.m_emailAdd;
            retVal.m_receipentName = response.m_receipentName;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.SocialInviteInfo ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.SocialInviteInfo response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.SocialInviteInfo retVal = new TVPApiModule.Objects.Responses.SocialInviteInfo();

            retVal.m_hashCode = response.m_hashCode;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.MediaFileItemPricesContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.MediaFileItemPricesContainer response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.MediaFileItemPricesContainer retVal = new TVPApiModule.Objects.Responses.MediaFileItemPricesContainer();

            retVal.m_nMediaFileID = response.m_nMediaFileID;
            retVal.m_oItemPrices = response.m_oItemPrices.Select(mf => mf.ToApiObject()).ToArray();
            retVal.m_sProductCode = response.m_sProductCode;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.ItemPriceContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.ItemPriceContainer response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.ItemPriceContainer retVal = new TVPApiModule.Objects.Responses.ItemPriceContainer();

            retVal.m_sPPVModuleCode = response.m_sPPVModuleCode;
            retVal.m_bSubscriptionOnly = response.m_bSubscriptionOnly;
            retVal.m_oPrice = response.m_oPrice.ToApiObject();
            retVal.m_oFullPrice = response.m_oFullPrice.ToApiObject();
            retVal.m_PriceReason = (TVPApiModule.Objects.Responses.PriceReason)response.m_PriceReason;
            retVal.m_relevantSub = response.m_relevantSub.ToApiObject();
            retVal.m_relevantPP = response.m_relevantPP.ToApiObject();
            retVal.m_oPPVDescription = response.m_oPPVDescription.Select(l => l.ToApiObject()).ToArray();
            retVal.m_couponStatus = (TVPApiModule.Objects.Responses.CouponsStatus)response.m_couponStatus;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.Subscription ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.Subscription response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.Subscription retVal = new TVPApiModule.Objects.Responses.Subscription();

            retVal.m_sCodes = response.m_sCodes.Select(c => c.ToApiObject()).ToArray();
            retVal.m_dStartDate = response.m_dStartDate;
            retVal.m_dEndDate = response.m_dEndDate;
            retVal.m_sFileTypes = response.m_sFileTypes;
            retVal.m_bIsRecurring = response.m_bIsRecurring;
            retVal.m_nNumberOfRecPeriods = response.m_nNumberOfRecPeriods;
            retVal.m_oSubscriptionPriceCode = response.m_oSubscriptionPriceCode.ToApiObject();
            retVal.m_oExtDisountModule = response.m_oExtDisountModule.ToApiObject();
            retVal.m_sName = response.m_sName.Select(l => l.ToApiObject()).ToArray();
            retVal.m_oSubscriptionUsageModule = response.m_oSubscriptionUsageModule.ToApiObject();
            retVal.m_fictivicMediaID = response.m_fictivicMediaID;
            retVal.m_Priority = response.m_Priority;
            retVal.m_ProductCode = response.m_ProductCode;
            retVal.m_SubscriptionCode = response.m_SubscriptionCode;
            retVal.m_MultiSubscriptionUsageModule = response.m_MultiSubscriptionUsageModule.Select(m => m.ToApiObject()).ToArray();
            retVal.n_GeoCommerceID = response.n_GeoCommerceID;
            retVal.m_bIsInfiniteRecurring = response.m_bIsInfiniteRecurring;
            retVal.m_oPreviewModule = response.m_oPreviewModule.ToApiObject();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.PrePaidModule ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PrePaidModule response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.PrePaidModule retVal = new TVPApiModule.Objects.Responses.PrePaidModule();

            retVal.m_PriceCode = response.m_PriceCode.ToApiObject();
            retVal.m_CreditValue = response.m_CreditValue.ToApiObject();
            retVal.m_UsageModule = response.m_UsageModule.ToApiObject();
            retVal.m_DiscountModule = response.m_DiscountModule.ToApiObject();
            retVal.m_CouponsGroup = response.m_CouponsGroup.ToApiObject();
            retVal.m_Description = response.m_Description.Select(l => l.ToApiObject()).ToArray();
            retVal.m_ObjectCode = response.m_ObjectCode;
            retVal.m_Title = response.m_Title;
            retVal.m_isFixedCredit = response.m_isFixedCredit;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.LanguageContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.LanguageContainer response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.LanguageContainer retVal = new TVPApiModule.Objects.Responses.LanguageContainer();

            retVal.m_sLanguageCode3 = response.m_sLanguageCode3;
            retVal.m_sValue = response.m_sValue;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.SubscriptionCodeContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.SubscriptionCodeContainer response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.SubscriptionCodeContainer retVal = new TVPApiModule.Objects.Responses.SubscriptionCodeContainer();

            retVal.m_sCode = response.m_sCode;
            retVal.m_sName = response.m_sName;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.PriceCode ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PriceCode response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.PriceCode retVal = new TVPApiModule.Objects.Responses.PriceCode();

            retVal.m_sCode = response.m_sCode;
            retVal.m_oPrise = response.m_oPrise.ToApiObject();
            retVal.m_nObjectID = response.m_nObjectID;
            retVal.m_sDescription = response.m_sDescription.Select(l => l.ToApiObject()).ToArray();


            return retVal;
        }

        public static TVPApiModule.Objects.Responses.DiscountModule ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.DiscountModule response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.DiscountModule retVal = new TVPApiModule.Objects.Responses.DiscountModule();

            retVal.m_dPercent = response.m_dPercent;
            retVal.m_eTheRelationType = (TVPApiModule.Objects.Responses.RelationTypes)response.m_eTheRelationType;
            retVal.m_dStartDate = response.m_dStartDate;
            retVal.m_dEndDate = response.m_dEndDate;
            retVal.m_oWhenAlgo = response.m_oWhenAlgo.ToApiObject();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.UsageModule ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.UsageModule response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.UsageModule retVal = new TVPApiModule.Objects.Responses.UsageModule();

            retVal.m_nObjectID = response.m_nObjectID;
            retVal.m_sVirtualName = response.m_sVirtualName;
            retVal.m_nMaxNumberOfViews = response.m_nMaxNumberOfViews;
            retVal.m_tsViewLifeCycle = response.m_tsViewLifeCycle;
            retVal.m_tsMaxUsageModuleLifeCycle = response.m_tsMaxUsageModuleLifeCycle;
            retVal.m_ext_discount_id = response.m_ext_discount_id;
            retVal.m_internal_discount_id = response.m_internal_discount_id;
            retVal.m_pricing_id = response.m_pricing_id;
            retVal.m_coupon_id = response.m_coupon_id;
            retVal.m_subscription_only = response.m_subscription_only;
            retVal.m_is_renew = response.m_is_renew;
            retVal.m_num_of_rec_periods = response.m_num_of_rec_periods;
            retVal.m_device_limit_id = response.m_device_limit_id;
            retVal.m_type = response.m_type;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.PreviewModule ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PreviewModule response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.PreviewModule retVal = new TVPApiModule.Objects.Responses.PreviewModule();

            retVal.m_nID = response.m_nID;
            retVal.m_sName = response.m_sName;
            retVal.m_tsFullLifeCycle = response.m_tsFullLifeCycle;
            retVal.m_tsNonRenewPeriod = response.m_tsNonRenewPeriod;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.CouponsGroup ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.CouponsGroup response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.CouponsGroup retVal = new TVPApiModule.Objects.Responses.CouponsGroup();

            retVal.m_oDiscountCode = response.m_oDiscountCode.ToApiObject();
            retVal.m_sDiscountCode = response.m_sDiscountCode;
            retVal.m_sDescription = response.m_sDescription.Select(l => l.ToApiObject()).ToArray();
            retVal.m_dStartDate = response.m_dStartDate;
            retVal.m_dEndDate = response.m_dEndDate;
            retVal.m_nMaxUseCountForCoupon = response.m_nMaxUseCountForCoupon;
            retVal.m_sGroupCode = response.m_sGroupCode;
            retVal.m_sGroupName = response.m_sGroupName;
            retVal.m_nFinancialEntityID = response.m_nFinancialEntityID;
            retVal.m_nMaxRecurringUsesCountForCoupon = response.m_nMaxRecurringUsesCountForCoupon;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.WhenAlgo ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.WhenAlgo response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.WhenAlgo retVal = new TVPApiModule.Objects.Responses.WhenAlgo();

            retVal.m_eAlgoType = (TVPApiModule.Objects.Responses.WhenAlgoType)response.m_eAlgoType;
            retVal.m_nNTimes = response.m_nNTimes;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.SubscriptionsPricesContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.SubscriptionsPricesContainer response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.SubscriptionsPricesContainer retVal = new TVPApiModule.Objects.Responses.SubscriptionsPricesContainer();

            retVal.m_sSubscriptionCode = response.m_sSubscriptionCode;
            retVal.m_oPrice = response.m_oPrice.ToApiObject();
            retVal.m_PriceReason = (TVPApiModule.Objects.Responses.PriceReason)response.m_PriceReason;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.UserBillingTransactionsResponse ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.UserBillingTransactionsResponse response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.UserBillingTransactionsResponse retVal = new TVPApiModule.Objects.Responses.UserBillingTransactionsResponse();

            retVal.m_sSiteGUID = response.m_sSiteGUID;
            retVal.m_BillingTransactionResponse = response.m_BillingTransactionResponse.ToApiObject();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.DomainBillingTransactionsResponse ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.DomainBillingTransactionsResponse response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.DomainBillingTransactionsResponse retVal = new TVPApiModule.Objects.Responses.DomainBillingTransactionsResponse();

            retVal.m_nDomainID = response.m_nDomainID;
            retVal.m_BillingTransactionResponses = response.m_BillingTransactionResponses.Select(bt => bt.ToApiObject()).ToArray();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.DomainResponseObject ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Domains.DomainResponseObject response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.DomainResponseObject retVal = new TVPApiModule.Objects.Responses.DomainResponseObject();

            retVal.m_oDomain = response.m_oDomain.ToApiObject();
            retVal.m_oDomainResponseStatus = (TVPApiModule.Objects.Responses.DomainResponseStatus)response.m_oDomainResponseStatus;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.Domain ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Domains.Domain response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.Domain retVal = new TVPApiModule.Objects.Responses.Domain();

            retVal.m_sName = response.m_sName;
            retVal.m_sDescription = response.m_sDescription;
            retVal.m_sCoGuid = response.m_sCoGuid;
            retVal.m_nDomainID = response.m_nDomainID;
            retVal.m_nGroupID = response.m_nGroupID;
            retVal.m_nLimit = response.m_nLimit;
            retVal.m_nDeviceLimit = response.m_nDeviceLimit;
            retVal.m_nUserLimit = response.m_nUserLimit;
            retVal.m_nConcurrentLimit = response.m_nConcurrentLimit;
            retVal.m_nStatus = response.m_nStatus;
            retVal.m_nIsActive = response.m_nIsActive;
            retVal.m_UsersIDs = response.m_UsersIDs;
            retVal.m_deviceFamilies = response.m_deviceFamilies.Select(d => d.ToApiObject()).ToArray();
            retVal.m_masterGUIDs = response.m_masterGUIDs;
            retVal.m_DomainStatus = (TVPApiModule.Objects.Responses.DomainStatus)response.m_DomainStatus;
            retVal.m_frequencyFlag = response.m_frequencyFlag;
            retVal.m_NextActionFreq = response.m_NextActionFreq;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.DeviceContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Domains.DeviceContainer response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.DeviceContainer retVal = new TVPApiModule.Objects.Responses.DeviceContainer();

            retVal.m_deviceFamilyName = response.m_deviceFamilyName;
            retVal.m_deviceFamilyID = response.m_deviceFamilyID;
            retVal.m_deviceLimit = response.m_deviceLimit;
            retVal.m_deviceConcurrentLimit = response.m_deviceConcurrentLimit;
            retVal.DeviceInstances = response.DeviceInstances.Select(d => d.ToApiObject()).ToArray();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.Device ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Domains.Device response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.Device retVal = new TVPApiModule.Objects.Responses.Device();

            retVal.m_id = response.m_id;
            retVal.m_deviceUDID = response.m_deviceUDID;
            retVal.m_deviceBrand = response.m_deviceBrand;
            retVal.m_deviceFamily = response.m_deviceFamily;
            retVal.m_deviceFamilyID = response.m_deviceFamilyID;
            retVal.m_domainID = response.m_domainID;
            retVal.m_deviceName = response.m_deviceName;
            retVal.m_deviceBrandID = response.m_deviceBrandID;
            retVal.m_pin = response.m_pin;
            retVal.m_activationDate = response.m_activationDate;
            retVal.m_state = (TVPApiModule.Objects.Responses.DeviceState)response.m_state;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.DeviceResponseObject ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Domains.DeviceResponseObject response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.DeviceResponseObject retVal = new TVPApiModule.Objects.Responses.DeviceResponseObject();

            retVal.m_oDevice = response.m_oDevice.ToApiObject();
            retVal.m_oDeviceResponseStatus = (DeviceResponseStatus)response.m_oDeviceResponseStatus;

            return retVal;
        }
    }
}
