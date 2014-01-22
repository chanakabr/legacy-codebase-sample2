using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPApiModule.Objects.Responses;

namespace TVPApiModule.Extentions
{
    public static class TranslateExtensions
    {
        public static UserResponseObject ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject response)
        {
            if (response == null)
                return null;

            UserResponseObject retVal = new UserResponseObject();

            retVal.m_RespStatus = (ResponseStatus)response.m_RespStatus;

            if (response.m_user != null)
            {
                retVal.m_user = new User();

                retVal.m_user.m_domianID = response.m_user.m_domianID;
                retVal.m_user.m_eUserState = (UserState)response.m_user.m_eUserState;
                retVal.m_user.m_isDomainMaster = response.m_user.m_isDomainMaster;
                retVal.m_user.m_nSSOOperatorID = response.m_user.m_nSSOOperatorID;

                if (response.m_user.m_oBasicData != null)
                {
                    retVal.m_user.m_oBasicData = new UserBasicData();

                    retVal.m_user.m_oBasicData.m_bIsFacebookImagePermitted = response.m_user.m_oBasicData.m_bIsFacebookImagePermitted;
                    retVal.m_user.m_oBasicData.m_CoGuid = response.m_user.m_oBasicData.m_CoGuid;

                    retVal.m_user.m_oBasicData.m_Country = response.m_user.m_oBasicData.m_Country.ToApiObject();

                    retVal.m_user.m_oBasicData.m_ExternalToken = response.m_user.m_oBasicData.m_ExternalToken;
                    retVal.m_user.m_oBasicData.m_sAddress = response.m_user.m_oBasicData.m_sAddress;
                    retVal.m_user.m_oBasicData.m_sAffiliateCode = response.m_user.m_oBasicData.m_sAffiliateCode;
                    retVal.m_user.m_oBasicData.m_sCity = response.m_user.m_oBasicData.m_sCity;
                    retVal.m_user.m_oBasicData.m_sEmail = response.m_user.m_oBasicData.m_sEmail;
                    retVal.m_user.m_oBasicData.m_sFacebookID = response.m_user.m_oBasicData.m_sFacebookID;
                    retVal.m_user.m_oBasicData.m_sFacebookImage = response.m_user.m_oBasicData.m_sFacebookImage;
                    retVal.m_user.m_oBasicData.m_sFacebookToken = response.m_user.m_oBasicData.m_sFacebookToken;
                    retVal.m_user.m_oBasicData.m_sFirstName = response.m_user.m_oBasicData.m_sFirstName;
                    retVal.m_user.m_oBasicData.m_sLastName = response.m_user.m_oBasicData.m_sLastName;
                    retVal.m_user.m_oBasicData.m_sPhone = response.m_user.m_oBasicData.m_sPhone;

                    retVal.m_user.m_oBasicData.m_State = response.m_user.m_oBasicData.m_State.ToApiObject();

                    retVal.m_user.m_oBasicData.m_sUserName = response.m_user.m_oBasicData.m_sUserName;
                    retVal.m_user.m_oBasicData.m_sZip = response.m_user.m_oBasicData.m_sZip;

                    retVal.m_user.m_oBasicData.m_UserType = response.m_user.m_oBasicData.m_UserType.ToApiObject();
                }

                if (response.m_user.m_oDynamicData != null)
                {
                    retVal.m_user.m_oDynamicData = new UserDynamicData();

                    if (response.m_user.m_oDynamicData.m_sUserData != null)
                    {
                        retVal.m_user.m_oDynamicData.m_sUserData = response.m_user.m_oDynamicData.m_sUserData.Select(x => new UserDynamicDataContainer()
                        {
                            m_sDataType = x.m_sDataType,
                            m_sValue = x.m_sValue
                        }).ToArray();
                    }
                }

                retVal.m_user.m_sSiteGUID = response.m_user.m_sSiteGUID;
            }

            retVal.m_userInstanceID = response.m_userInstanceID;

            return retVal;
        }

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

        public static UserItemList ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Users.UserItemList response)
        {
            if (response == null)
                return null;

            UserItemList retVal = new UserItemList();

            if (response.itemObj != null)
            {
                retVal.itemObj = response.itemObj.Select(x => new ItemObj()
                {
                    item = x.item,
                    orderNum = x.orderNum
                }).ToArray();
            }

            retVal.itemType = (ItemType)response.itemType;
            retVal.listType = (ListType)response.listType;

            return retVal;
        }

        //public static KeyValuePair<string, string> ToApiObject(this KeyValuePair response)
        //{
        //    KeyValuePair<string, string> response = new KeyValuePair<string, string>(response.key, response.value);

        //    return response;
        //}

        public static State ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Users.State response)
        {
            if (response == null)
                return null;

            State retVal = new State();

            retVal.m_Country = response.m_Country.ToApiObject();

            retVal.m_nObjecrtID = response.m_nObjecrtID;
            retVal.m_sStateCode = response.m_sStateCode;
            retVal.m_sStateName = response.m_sStateName;

            return retVal;
        }

        public static Country ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Users.Country response)
        {
            if (response == null)
                return null;

            Country retVal = new Country();

            retVal.m_nObjecrtID = response.m_nObjecrtID;
            retVal.m_sCountryCode = response.m_sCountryCode;
            retVal.m_sCountryName = response.m_sCountryName;

            return retVal;
        }

        public static UserType ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Users.UserType response)
        {
            if (response == null)
                return null;

            UserType retVal = new UserType();

            retVal.Description = response.Description;
            retVal.ID = response.ID;
            retVal.IsDefault = response.IsDefault;

            return retVal;
        }

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

        public static TVPApiModule.Objects.Responses.PPVModule ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.PPVModule response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.PPVModule retVal = new TVPApiModule.Objects.Responses.PPVModule();

            retVal.m_oPriceCode = response.m_oPriceCode.ToApiObject();
            retVal.m_oUsageModule = response.m_oUsageModule.ToApiObject();
            retVal.m_oDiscountModule = response.m_oDiscountModule.ToApiObject();
            retVal.m_oCouponsGroup = response.m_oCouponsGroup.ToApiObject();
            retVal.m_sDescription = response.m_sDescription.Select(l => l.ToApiObject()).ToArray();
            retVal.m_sObjectCode = response.m_sObjectCode;
            retVal.m_sObjectVirtualName = response.m_sObjectVirtualName;
            retVal.m_bSubscriptionOnly = response.m_bSubscriptionOnly;
            retVal.m_relatedFileTypes = response.m_relatedFileTypes;
            retVal.m_Product_Code = response.m_Product_Code;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.PriceCode ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.PriceCode response)
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

        public static TVPApiModule.Objects.Responses.UsageModule ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.UsageModule response)
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

        public static TVPApiModule.Objects.Responses.DiscountModule ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.DiscountModule response)
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

        public static TVPApiModule.Objects.Responses.CouponsGroup ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.CouponsGroup response)
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

        public static TVPApiModule.Objects.Responses.LanguageContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.LanguageContainer response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.LanguageContainer retVal = new TVPApiModule.Objects.Responses.LanguageContainer();

            retVal.m_sLanguageCode3 = response.m_sLanguageCode3;
            retVal.m_sValue = response.m_sValue;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.Price ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.Price response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.Price retVal = new TVPApiModule.Objects.Responses.Price();

            retVal.m_dPrice = response.m_dPrice;
            retVal.m_oCurrency = response.m_oCurrency.ToApiObject();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.WhenAlgo ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.WhenAlgo response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.WhenAlgo retVal = new TVPApiModule.Objects.Responses.WhenAlgo();

            retVal.m_eAlgoType = (TVPApiModule.Objects.Responses.WhenAlgoType)response.m_eAlgoType;
            retVal.m_nNTimes = response.m_nNTimes;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.Currency ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.Currency response)
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

        public static TVPApiModule.Objects.Responses.CouponData ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.CouponData response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.CouponData retVal = new TVPApiModule.Objects.Responses.CouponData();

            retVal.m_CouponStatus = (CouponsStatus)response.m_CouponStatus;
            retVal.m_oCouponGroup = response.m_oCouponGroup.ToApiObject();
            retVal.m_CouponType = (CouponType)response.m_CouponType;
            retVal.m_campID = response.m_campID;
            retVal.m_ownerGUID = response.m_ownerGUID;
            retVal.m_ownerMedia = response.m_ownerMedia;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.Subscription ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.Subscription response)
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

        public static TVPApiModule.Objects.Responses.SubscriptionCodeContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.SubscriptionCodeContainer response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.SubscriptionCodeContainer retVal = new TVPApiModule.Objects.Responses.SubscriptionCodeContainer();

            retVal.m_sCode = response.m_sCode;
            retVal.m_sName = response.m_sName;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.PreviewModule ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.PreviewModule response)
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

        public static TVPApiModule.Objects.Responses.GroupOperator ToApiObject(this TVPPro.SiteManager.TvinciPlatform.api.GroupOperator response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.GroupOperator retVal = new TVPApiModule.Objects.Responses.GroupOperator();

            retVal.UIData = response.UIData.ToApiObject();
            retVal.ID = response.ID;
            retVal.Name = response.Name;
            retVal.Type = (eOperatorType)response.Type;
            retVal.LoginUrl = response.LoginUrl;
            retVal.SubGroupID = response.SubGroupID;
            retVal.Scopes = response.Scopes.Select(s => s.ToApiObject()).ToArray();
            retVal.GroupUserName = response.GroupUserName;
            retVal.GroupPassword = response.GroupPassword;
            retVal.LogoutURL = response.LogoutURL;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.Scope ToApiObject(this TVPPro.SiteManager.TvinciPlatform.api.Scope response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.Scope retVal = new TVPApiModule.Objects.Responses.Scope();

            retVal.LoginUrl = response.LoginUrl;
            retVal.LogoutUrl = response.LogoutUrl;
            retVal.Name = response.Name;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.UIData ToApiObject(this TVPPro.SiteManager.TvinciPlatform.api.UIData response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.UIData retVal = new TVPApiModule.Objects.Responses.UIData();

            retVal.ColorCode = response.ColorCode;
            retVal.picID = response.picID;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.UserSocialActionObject ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Social.UserSocialActionObject response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.UserSocialActionObject retVal = new TVPApiModule.Objects.Responses.UserSocialActionObject();

            retVal.m_sSiteGuid = response.m_sSiteGuid;
            retVal.m_eSocialAction = (eUserAction)response.m_eSocialAction;
            retVal.m_eSocialPlatform = (SocialPlatform)response.m_eSocialPlatform;
            retVal.nMediaID = response.nMediaID;
            retVal.nProgramID = response.nProgramID;
            retVal.assetType = (eAssetType)response.assetType;
            retVal.m_dActionDate = response.m_dActionDate;


            return retVal;
        }

        public static TVPApiModule.Objects.Responses.UserBasicData ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.UserBasicData retVal = new TVPApiModule.Objects.Responses.UserBasicData();

            retVal.m_bIsFacebookImagePermitted = response.m_bIsFacebookImagePermitted;
            retVal.m_CoGuid = response.m_CoGuid;
            retVal.m_Country = response.m_Country.ToApiObject();
            retVal.m_ExternalToken = response.m_ExternalToken;
            retVal.m_sAddress = response.m_sAddress;
            retVal.m_sAffiliateCode = response.m_sAffiliateCode;
            retVal.m_sCity = response.m_sCity;
            retVal.m_sEmail = response.m_sEmail;
            retVal.m_sFacebookID = response.m_sFacebookID;
            retVal.m_sFacebookImage = response.m_sFacebookImage;
            retVal.m_sFacebookToken = response.m_sFacebookToken;
            retVal.m_sFirstName = response.m_sFirstName;
            retVal.m_sLastName = response.m_sLastName;
            retVal.m_sPhone = response.m_sPhone;
            retVal.m_State = response.m_State.ToApiObject();
            retVal.m_sUserName = response.m_sUserName;
            retVal.m_sZip = response.m_sZip;
            retVal.m_UserType = response.m_UserType.ToApiObject();


            return retVal;
        }


        public static TVPApiModule.Objects.Responses.FriendWatchedObject ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Social.FriendWatchedObject response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.FriendWatchedObject retVal = new TVPApiModule.Objects.Responses.FriendWatchedObject();

            retVal.SiteGuid = response.SiteGuid;
            retVal.MediaID = response.MediaID;
            retVal.UpdateDate = response.UpdateDate;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.FacebookConfig ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Social.FacebookConfig response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.FacebookConfig retVal = new TVPApiModule.Objects.Responses.FacebookConfig();

            retVal.sFBKey = response.sFBKey;
            retVal.sFBSecret = response.sFBSecret;
            retVal.sFBCallback = response.sFBCallback;
            retVal.nFBMinFriends = response.nFBMinFriends;
            retVal.sFBPermissions = response.sFBPermissions;
            retVal.sFBRedirect = response.sFBRedirect;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.FBInterestData ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Social.FBInterestData response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.FBInterestData retVal = new TVPApiModule.Objects.Responses.FBInterestData();

            retVal.name = response.name;
            retVal.category = response.category;
            retVal.id = response.id;
            retVal.created_time = response.created_time;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.FBInterest ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Social.FBInterest response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.FBInterest retVal = new TVPApiModule.Objects.Responses.FBInterest();

            retVal.data = response.data.Select(fbid => fbid.ToApiObject()).ToArray();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.FBLoaction ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Social.FBLoaction response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.FBLoaction retVal = new TVPApiModule.Objects.Responses.FBLoaction();

            retVal.name = response.name;
            retVal.id = response.id;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.FBUser ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Social.FBUser response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.FBUser retVal = new TVPApiModule.Objects.Responses.FBUser();

            retVal.m_sSiteGuid = response.m_sSiteGuid;
            retVal.Birthday = response.Birthday;
            retVal.Location = response.Location.ToApiObject();
            retVal.interests = response.interests.ToApiObject();
            retVal.name = response.name;
            retVal.id = response.id;
            retVal.uid = response.uid;
            retVal.first_name = response.first_name;
            retVal.last_name = response.last_name;
            retVal.email = response.email;
            retVal.gender = response.gender;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.FacebookResponseObject ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Social.FacebookResponseObject response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.FacebookResponseObject retVal = new TVPApiModule.Objects.Responses.FacebookResponseObject();

            retVal.status = response.status;
            retVal.siteGuid = response.siteGuid;
            retVal.tvinciName = response.tvinciName;
            retVal.facebookName = response.facebookName;
            retVal.pic = response.pic;
            retVal.data = response.data;
            retVal.minFriends = response.minFriends;
            retVal.fbUser = response.fbUser.ToApiObject();
            retVal.token = response.token;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.KeyValuePair ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.KeyValuePair retVal = new TVPApiModule.Objects.Responses.KeyValuePair();

            retVal.key = response.key;
            retVal.value = response.value;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.KeyValuePair ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Users.KeyValuePair response)
        {
            if (response == null)
                return null;

            TVPApiModule.Objects.Responses.KeyValuePair retVal = new TVPApiModule.Objects.Responses.KeyValuePair();

            retVal.key = response.key;
            retVal.value = response.value;

            return retVal;
        }
    }
}
