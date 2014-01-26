using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Extentions
{
    public static class TranslateExtensions
    {
        public static TVPApiModule.Objects.Responses.UserResponseObject ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject response)
        {
            TVPApiModule.Objects.Responses.UserResponseObject retVal = new TVPApiModule.Objects.Responses.UserResponseObject();

            retVal.respStatus = (TVPApiModule.Objects.Responses.ResponseStatus)response.m_RespStatus;

            if (response.m_user != null)
            {
                retVal.user = new TVPApiModule.Objects.Responses.User();

                retVal.user.domianID = response.m_user.m_domianID;
                retVal.user.userState = (TVPApiModule.Objects.Responses.UserState)response.m_user.m_eUserState;
                retVal.user.domainMaster = response.m_user.m_isDomainMaster;
                retVal.user.ssoOperatorID = response.m_user.m_nSSOOperatorID;

                if (response.m_user.m_oBasicData != null)
                {
                    retVal.user.basicData = new TVPApiModule.Objects.Responses.UserBasicData();

                    retVal.user.basicData.isFacebookImagePermitted = response.m_user.m_oBasicData.m_bIsFacebookImagePermitted;
                    retVal.user.basicData.coGuid = response.m_user.m_oBasicData.m_CoGuid;

                    retVal.user.basicData.country = response.m_user.m_oBasicData.m_Country.ToApiObject();

                    retVal.user.basicData.externalToken = response.m_user.m_oBasicData.m_ExternalToken;
                    retVal.user.basicData.address = response.m_user.m_oBasicData.m_sAddress;
                    retVal.user.basicData.affiliateCode = response.m_user.m_oBasicData.m_sAffiliateCode;
                    retVal.user.basicData.city = response.m_user.m_oBasicData.m_sCity;
                    retVal.user.basicData.email = response.m_user.m_oBasicData.m_sEmail;
                    retVal.user.basicData.facebookID = response.m_user.m_oBasicData.m_sFacebookID;
                    retVal.user.basicData.facebookImage = response.m_user.m_oBasicData.m_sFacebookImage;
                    retVal.user.basicData.facebookToken = response.m_user.m_oBasicData.m_sFacebookToken;
                    retVal.user.basicData.firstName = response.m_user.m_oBasicData.m_sFirstName;
                    retVal.user.basicData.lastName = response.m_user.m_oBasicData.m_sLastName;
                    retVal.user.basicData.phone = response.m_user.m_oBasicData.m_sPhone;

                    retVal.user.basicData.state = response.m_user.m_oBasicData.m_State.ToApiObject();

                    retVal.user.basicData.userName = response.m_user.m_oBasicData.m_sUserName;
                    retVal.user.basicData.zip = response.m_user.m_oBasicData.m_sZip;

                    retVal.user.basicData.userType = response.m_user.m_oBasicData.m_UserType.ToApiObject();
                }

                if (response.m_user.m_oDynamicData != null)
                {
                    retVal.user.dynamicData = new TVPApiModule.Objects.Responses.UserDynamicData();

                    if (response.m_user.m_oDynamicData.m_sUserData != null)
                    {
                        retVal.user.dynamicData.userData = response.m_user.m_oDynamicData.m_sUserData.Select(x => new TVPApiModule.Objects.Responses.UserDynamicDataContainer()
                        {
                            dataType = x.m_sDataType,
                            value = x.m_sValue
                        }).ToArray();
                    }
                }

                retVal.user.siteGUID = response.m_user.m_sSiteGUID;
            }

            retVal.userInstanceID = response.m_userInstanceID;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.PermittedSubscriptionContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PermittedSubscriptionContainer response)
        {
            TVPApiModule.Objects.Responses.PermittedSubscriptionContainer retVal = new TVPApiModule.Objects.Responses.PermittedSubscriptionContainer();

            retVal.isSubRenewable = response.m_bIsSubRenewable;
            retVal.recurringStatus = response.m_bRecurringStatus;
            retVal.currentDate = response.m_dCurrentDate;
            retVal.endDate = response.m_dEndDate;
            retVal.lastViewDate = response.m_dLastViewDate;
            retVal.nextRenewalDate = response.m_dNextRenewalDate;
            retVal.purchaseDate = response.m_dPurchaseDate;
            retVal.currentUses = response.m_nCurrentUses;
            retVal.maxUses = response.m_nMaxUses;
            retVal.subscriptionPurchaseID = response.m_nSubscriptionPurchaseID;
            retVal.paymentMethod = (TVPApiModule.Objects.Responses.PaymentMethod)response.m_paymentMethod;
            retVal.deviceName = response.m_sDeviceName;
            retVal.deviceUDID = response.m_sDeviceUDID;
            retVal.subscriptionCode = response.m_sSubscriptionCode;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.PermittedMediaContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PermittedMediaContainer response)
        {
            TVPApiModule.Objects.Responses.PermittedMediaContainer retVal = new TVPApiModule.Objects.Responses.PermittedMediaContainer();

            retVal.currentDate = response.m_dCurrentDate;
            retVal.endDate = response.m_dEndDate;
            retVal.purchaseDate = response.m_dPurchaseDate;
            retVal.currentUses = response.m_nCurrentUses;
            retVal.maxUses = response.m_nMaxUses;
            retVal.mediaFileID = response.m_nMediaFileID;
            retVal.mediaID = response.m_nMediaID;
            retVal.purchaseMethod = (TVPApiModule.Objects.Responses.PaymentMethod)response.m_purchaseMethod;
            retVal.deviceName = response.m_sDeviceName;
            retVal.deviceUDID = response.m_sDeviceUDID;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.FavoriteObject ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Users.FavoritObject response)
        {
            TVPApiModule.Objects.Responses.FavoriteObject retVal = new TVPApiModule.Objects.Responses.FavoriteObject();

            retVal.updateDate = response.m_dUpdateDate;
            retVal.is_channel = response.m_is_channel;
            retVal.domainID = response.m_nDomainID;
            retVal.id = response.m_nID;
            retVal.deviceName = response.m_sDeviceName;
            retVal.deviceUDID = response.m_sDeviceUDID;
            retVal.extraData = response.m_sExtraData;
            retVal.itemCode = response.m_sItemCode;
            retVal.siteUserGUID = response.m_sSiteUserGUID;
            retVal.type = response.m_sType;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.GroupRule ToApiObject(this TVPPro.SiteManager.TvinciPlatform.api.GroupRule response)
        {
            TVPApiModule.Objects.Responses.GroupRule retVal = new TVPApiModule.Objects.Responses.GroupRule();

            retVal.allTagValues = response.AllTagValues;
            retVal.blockType = (TVPApiModule.Objects.Responses.eBlockType)response.BlockType;
            retVal.dynamicDataKey = response.DynamicDataKey;
            retVal.groupRuleType = (TVPApiModule.Objects.Responses.eGroupRuleType)response.GroupRuleType;
            retVal.isActive = response.IsActive;
            retVal.name = response.Name;
            retVal.ruleID = response.RuleID;
            retVal.tagTypeID = response.TagTypeID;
            retVal.tagValue = response.TagValue;

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
            TVPApiModule.Objects.Responses.MediaMarkObject retVal = new TVPApiModule.Objects.Responses.MediaMarkObject();

            retVal.status = (TVPApiModule.Objects.Responses.MediaMarkObjectStatus)response.eStatus;
            retVal.groupID = response.nGroupID;
            retVal.locationSec = response.nLocationSec;
            retVal.mediaID = response.nMediaID;
            retVal.deviceID = response.sDeviceID;
            retVal.deviceName = response.sDeviceName;
            retVal.siteGUID = response.sSiteGUID;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.UserItemList ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Users.UserItemList response)
        {
            TVPApiModule.Objects.Responses.UserItemList retVal = new TVPApiModule.Objects.Responses.UserItemList();

            if (response.itemObj != null)
            {
                retVal.itemObj = response.itemObj.Select(x => new TVPApiModule.Objects.Responses.ItemObj()
                {
                    item = x.item,
                    orderNum = x.orderNum
                }).ToArray();
            }

            retVal.itemType = (TVPApiModule.Objects.Responses.ItemType)response.itemType;
            retVal.listType = (TVPApiModule.Objects.Responses.ListType)response.listType;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.State ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Users.State response)
        {
            TVPApiModule.Objects.Responses.State retVal = new TVPApiModule.Objects.Responses.State();
            
            if (response.m_Country != null)
                retVal.country = response.m_Country.ToApiObject();

            retVal.objectID = response.m_nObjecrtID;
            retVal.stateCode = response.m_sStateCode;
            retVal.stateName = response.m_sStateName;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.Country ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Users.Country response)
        {
            TVPApiModule.Objects.Responses.Country retVal = new TVPApiModule.Objects.Responses.Country();

            retVal.objectID = response.m_nObjecrtID;
            retVal.countryCode = response.m_sCountryCode;
            retVal.countryName = response.m_sCountryName;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.UserType ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Users.UserType response)
        {
            TVPApiModule.Objects.Responses.UserType retVal = new TVPApiModule.Objects.Responses.UserType();

            retVal.description = response.Description;
            retVal.id = response.ID;
            retVal.isDefault = response.IsDefault;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.EPGChannel ToApiObject(this TVPPro.SiteManager.TvinciPlatform.api.EPGChannelObject response)
        {
            TVPApiModule.Objects.Responses.EPGChannel retVal = new TVPApiModule.Objects.Responses.EPGChannel();

            retVal.channelID = response.CHANNEL_ID;
            retVal.createDate = response.CREATE_DATE;
            retVal.description = response.DESCRIPTION;
            retVal.editorRemarks = response.EDITOR_REMARKS;
            retVal.epgChannelID = response.EPG_CHANNEL_ID;
            retVal.groupID = response.GROUP_ID;
            retVal.isActive = response.IS_ACTIVE;
            retVal.mediaID = response.MEDIA_ID;
            retVal.name = response.NAME;
            retVal.orderNum = response.ORDER_NUM;
            retVal.picUrl = response.PIC_URL;
            retVal.publishDate = response.PUBLISH_DATE;
            retVal.status = response.STATUS;
            retVal.updaterID = response.UPDATER_ID;

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
            TVPApiModule.Objects.Responses.EPGMultiChannelProgrammeObject retVal = new TVPApiModule.Objects.Responses.EPGMultiChannelProgrammeObject();

            retVal.epgChannelID = response.EPG_CHANNEL_ID;

            if (response.EPGChannelProgrammeObject != null)
                retVal.epgChannelProgramObject = response.EPGChannelProgrammeObject.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.EPGChannelProgrammeObject ToApiObject(this TVPPro.SiteManager.TvinciPlatform.api.EPGChannelProgrammeObject response)
        {
            TVPApiModule.Objects.Responses.EPGChannelProgrammeObject retVal = new TVPApiModule.Objects.Responses.EPGChannelProgrammeObject();

            retVal.createDate = response.CREATE_DATE;
            retVal.description = response.DESCRIPTION;
            retVal.endDate = response.END_DATE;
            retVal.epgChannelID = response.EPG_CHANNEL_ID;
            retVal.epgID = response.EPG_ID;
            retVal.epgIdentifier = response.EPG_IDENTIFIER;

            if (response.EPG_Meta != null)
                response.EPG_Meta.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            if (response.EPG_TAGS != null)
                response.EPG_TAGS.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            retVal.groupID = response.GROUP_ID;
            retVal.isActive = response.IS_ACTIVE;
            retVal.likeCounter = response.LIKE_COUNTER;
            retVal.mediaID = response.media_id;
            retVal.name = response.NAME;
            retVal.picUrl = response.PIC_URL;
            retVal.publishDate = response.PUBLISH_DATE;
            retVal.startDate = response.START_DATE;
            retVal.status = response.STATUS;
            retVal.updateDate = response.UPDATE_DATE;
            retVal.updaterID = response.UPDATER_ID;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.EPGChannelProgrammeObject ToApiObject(this Tvinci.Data.Loaders.TvinciPlatform.Catalog.EPGChannelProgrammeObject response)
        {
            TVPApiModule.Objects.Responses.EPGChannelProgrammeObject retVal = new TVPApiModule.Objects.Responses.EPGChannelProgrammeObject();

            retVal.createDate = response.CREATE_DATE;
            retVal.description = response.DESCRIPTION;
            retVal.endDate = response.END_DATE;
            retVal.epgChannelID = response.EPG_CHANNEL_ID;
            retVal.epgID = response.EPG_ID;
            retVal.epgIdentifier = response.EPG_IDENTIFIER;

            if (response.EPG_Meta != null)
                response.EPG_Meta.Select(x => x.ToApiObject()).ToArray();

            if (response.EPG_TAGS != null)
                response.EPG_TAGS.Select(x => x.ToApiObject()).ToArray();

            retVal.groupID = response.GROUP_ID;
            retVal.isActive = response.IS_ACTIVE;
            retVal.likeCounter = response.LIKE_COUNTER;
            retVal.mediaID = response.media_id;
            retVal.name = response.NAME;
            retVal.picUrl = response.PIC_URL;
            retVal.publishDate = response.PUBLISH_DATE;
            retVal.startDate = response.START_DATE;
            retVal.status = response.STATUS;
            retVal.updateDate = response.UPDATE_DATE;
            retVal.updaterID = response.UPDATER_ID;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.EPGDictionary ToApiObject(this TVPPro.SiteManager.TvinciPlatform.api.EPGDictionary response)
        {
            TVPApiModule.Objects.Responses.EPGDictionary retVal = new TVPApiModule.Objects.Responses.EPGDictionary();

            retVal.key = response.Key;
            retVal.value = response.Value;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.EPGDictionary ToApiObject(this Tvinci.Data.Loaders.TvinciPlatform.Catalog.EPGDictionary response)
        {
            TVPApiModule.Objects.Responses.EPGDictionary retVal = new TVPApiModule.Objects.Responses.EPGDictionary();

            retVal.key = response.Key;
            retVal.value = response.Value;

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
            TVPApiModule.Objects.Responses.BillingResponse retVal = new TVPApiModule.Objects.Responses.BillingResponse();

            retVal.status = (TVPApiModule.Objects.Responses.BillingResponseStatus)response.m_oStatus;
            retVal.externalReceiptCode = response.m_sExternalReceiptCode;
            retVal.recieptCode = response.m_sRecieptCode;
            retVal.statusDescription = response.m_sStatusDescription;
            
            return retVal;
        }

        public static TVPApiModule.Objects.Responses.Currency ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.Currency response)
        {
            TVPApiModule.Objects.Responses.Currency retVal = new TVPApiModule.Objects.Responses.Currency();

            retVal.currencyID = response.m_nCurrencyID;
            retVal.currencyCD2 = response.m_sCurrencyCD2;
            retVal.currencyCD3 = response.m_sCurrencyCD3;
            retVal.currencySign = response.m_sCurrencySign;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.Price ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.Price response)
        {
            TVPApiModule.Objects.Responses.Price retVal = new TVPApiModule.Objects.Responses.Price();

            retVal.price = response.m_dPrice;
            if (response.m_oCurrency != null)
                retVal.currency = response.m_oCurrency.ToApiObject();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.BillingTransactionContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.BillingTransactionContainer response)
        {
            TVPApiModule.Objects.Responses.BillingTransactionContainer retVal = new TVPApiModule.Objects.Responses.BillingTransactionContainer();

            retVal.isRecurring = response.m_bIsRecurring;
            retVal.actionDate = response.m_dtActionDate;
            retVal.endDate = response.m_dtEndDate;
            retVal.startDate = response.m_dtStartDate;
            retVal.billingAction = (TVPApiModule.Objects.Responses.BillingAction)response.m_eBillingAction;
            retVal.itemType = (TVPApiModule.Objects.Responses.BillingItemsType)response.m_eItemType;
            retVal.paymentMethod = (TVPApiModule.Objects.Responses.PaymentMethod)response.m_ePaymentMethod;
            retVal.billingProviderRef = response.m_nBillingProviderRef;
            retVal.purchaseID = response.m_nPurchaseID;

            if (response.m_Price != null)
                retVal.price = response.m_Price.ToApiObject();

            retVal.paymentMethodExtraDetails = response.m_sPaymentMethodExtraDetails;
            retVal.purchasedItemCode = response.m_sPurchasedItemCode;
            retVal.purchasedItemName = response.m_sPurchasedItemName;
            retVal.recieptCode = response.m_sRecieptCode;
            retVal.remarks = response.m_sRemarks; 

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.BillingTransactionsResponse ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.BillingTransactionsResponse response)
        {
            TVPApiModule.Objects.Responses.BillingTransactionsResponse retVal = new TVPApiModule.Objects.Responses.BillingTransactionsResponse();

            retVal.transactionsCount = response.m_nTransactionsCount;
            if (response.m_Transactions != null)
                retVal.transactions = response.m_Transactions.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.AdyenBillingDetail ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Billing.AdyenBillingDetail response)
        {
            TVPApiModule.Objects.Responses.AdyenBillingDetail retVal = new TVPApiModule.Objects.Responses.AdyenBillingDetail();

            if (response.billingInfo != null)
                retVal.billingInfo = response.billingInfo.ToApiObject();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.BillingInfo ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Billing.BillingInfo response)
        {
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
            TVPApiModule.Objects.Responses.CampaignActionInfo retVal = new TVPApiModule.Objects.Responses.CampaignActionInfo();

            retVal.mediaID = response.m_mediaID;
            retVal.siteGuid = response.m_siteGuid;
            retVal.mediaLink = response.m_mediaLink;
            retVal.senderName = response.m_senderName;
            retVal.senderEmail = response.m_senderEmail;
            retVal.status = (Objects.Responses.CampaignActionResult)response.m_status;
            
            if (response.m_socialInviteInfo != null)
                retVal.socialInviteInfo = response.m_socialInviteInfo.ToApiObject();

            if (response.m_voucherReceipents != null)
            retVal.voucherReceipents = response.m_voucherReceipents.Where(x => x != null).Select(x => x.ToApiObject()).ToArray(); 

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.VoucherReceipentInfo ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.VoucherReceipentInfo response)
        {
            TVPApiModule.Objects.Responses.VoucherReceipentInfo retVal = new TVPApiModule.Objects.Responses.VoucherReceipentInfo();

            retVal.emailAdd = response.m_emailAdd;
            retVal.receipentName = response.m_receipentName;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.SocialInviteInfo ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.SocialInviteInfo response)
        {
            TVPApiModule.Objects.Responses.SocialInviteInfo retVal = new TVPApiModule.Objects.Responses.SocialInviteInfo();

            retVal.hashCode = response.m_hashCode;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.MediaFileItemPricesContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.MediaFileItemPricesContainer response)
        {
            TVPApiModule.Objects.Responses.MediaFileItemPricesContainer retVal = new TVPApiModule.Objects.Responses.MediaFileItemPricesContainer();

            retVal.mediaFileID = response.m_nMediaFileID;

            if (response.m_oItemPrices != null)
                retVal.itemPrices = response.m_oItemPrices.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            retVal.productCode = response.m_sProductCode;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.ItemPriceContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.ItemPriceContainer response)
        {
            TVPApiModule.Objects.Responses.ItemPriceContainer retVal = new TVPApiModule.Objects.Responses.ItemPriceContainer();

            retVal.ppvModuleCode = response.m_sPPVModuleCode;
            retVal.subscriptionOnly = response.m_bSubscriptionOnly;

            if (response.m_oFullPrice != null)
                retVal.price = response.m_oPrice.ToApiObject();

            if (response.m_oFullPrice != null)
                retVal.fullPrice = response.m_oFullPrice.ToApiObject();

            retVal.priceReason = (TVPApiModule.Objects.Responses.PriceReason)response.m_PriceReason;

            if (response.m_relevantSub != null)
                retVal.relevantSub = response.m_relevantSub.ToApiObject();

            if (response.m_relevantPP != null)
                retVal.relevantPP = response.m_relevantPP.ToApiObject();

            if (response.m_oPPVDescription != null)            
                retVal.ppvVDescription = response.m_oPPVDescription.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            retVal.couponStatus = (TVPApiModule.Objects.Responses.CouponsStatus)response.m_couponStatus;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.Subscription ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.Subscription response)
        {
            TVPApiModule.Objects.Responses.Subscription retVal = new TVPApiModule.Objects.Responses.Subscription();

            if (response.m_sCodes != null)                        
                retVal.codes = response.m_sCodes.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            retVal.startDate = response.m_dStartDate;
            retVal.endDate = response.m_dEndDate;
            retVal.fileTypes = response.m_sFileTypes;
            retVal.isRecurring = response.m_bIsRecurring;
            retVal.numberOfRecPeriods = response.m_nNumberOfRecPeriods;

            if (response.m_oSubscriptionPriceCode != null)                        
                retVal.subscriptionPriceCode = response.m_oSubscriptionPriceCode.ToApiObject();

            if (response.m_oExtDisountModule != null)                        
                retVal.extDisountModule = response.m_oExtDisountModule.ToApiObject();

            if (response.m_sName != null)                        
                retVal.name = response.m_sName.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            if (response.m_oSubscriptionUsageModule != null)                        
                retVal.subscriptionUsageModule = response.m_oSubscriptionUsageModule.ToApiObject();

            retVal.fictivicMediaID = response.m_fictivicMediaID;
            retVal.priority = response.m_Priority;
            retVal.productCode = response.m_ProductCode;
            retVal.subscriptionCode = response.m_SubscriptionCode;

            if (response.m_MultiSubscriptionUsageModule != null)                        
                retVal.multiSubscriptionUsageModule = response.m_MultiSubscriptionUsageModule.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            retVal.geoCommerceID = response.n_GeoCommerceID;
            retVal.isInfiniteRecurring = response.m_bIsInfiniteRecurring;

            if (response.m_oPreviewModule != null)                        
                retVal.previewModule = response.m_oPreviewModule.ToApiObject();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.PrePaidModule ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PrePaidModule response)
        {
            TVPApiModule.Objects.Responses.PrePaidModule retVal = new TVPApiModule.Objects.Responses.PrePaidModule();

            if (response.m_PriceCode != null)                        
                retVal.priceCode = response.m_PriceCode.ToApiObject();

            if (response.m_CreditValue != null)                        
                retVal.creditValue = response.m_CreditValue.ToApiObject();
                    
            if (response.m_UsageModule != null)                        
                retVal.usageModule = response.m_UsageModule.ToApiObject();

            if (response.m_DiscountModule != null)                        
                retVal.discountModule = response.m_DiscountModule.ToApiObject();

            if (response.m_CouponsGroup != null)                        
                retVal.couponsGroup = response.m_CouponsGroup.ToApiObject();

            if (response.m_Description != null)                        
                retVal.description = response.m_Description.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            retVal.objectCode = response.m_ObjectCode;
            retVal.title = response.m_Title;
            retVal.isFixedCredit = response.m_isFixedCredit;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.LanguageContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.LanguageContainer response)
        {
            TVPApiModule.Objects.Responses.LanguageContainer retVal = new TVPApiModule.Objects.Responses.LanguageContainer();

            retVal.languageCode3 = response.m_sLanguageCode3;
            retVal.value = response.m_sValue;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.SubscriptionCodeContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.SubscriptionCodeContainer response)
        {
            TVPApiModule.Objects.Responses.SubscriptionCodeContainer retVal = new TVPApiModule.Objects.Responses.SubscriptionCodeContainer();

            retVal.code = response.m_sCode;
            retVal.name = response.m_sName;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.PriceCode ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PriceCode response)
        {
            TVPApiModule.Objects.Responses.PriceCode retVal = new TVPApiModule.Objects.Responses.PriceCode();

            retVal.code = response.m_sCode;

            if (response.m_oPrise != null)
                retVal.prise = response.m_oPrise.ToApiObject();

            retVal.objectID = response.m_nObjectID;

            if (response.m_sDescription != null)
                retVal.description = response.m_sDescription.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();


            return retVal;
        }

        public static TVPApiModule.Objects.Responses.DiscountModule ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.DiscountModule response)
        {
            TVPApiModule.Objects.Responses.DiscountModule retVal = new TVPApiModule.Objects.Responses.DiscountModule();

            retVal.percent = response.m_dPercent;
            retVal.theRelationType = (TVPApiModule.Objects.Responses.RelationTypes)response.m_eTheRelationType;
            retVal.startDate = response.m_dStartDate;
            retVal.endDate = response.m_dEndDate;

            if (response.m_oWhenAlgo != null)
                retVal.whenAlgo = response.m_oWhenAlgo.ToApiObject();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.UsageModule ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.UsageModule response)
        {
            TVPApiModule.Objects.Responses.UsageModule retVal = new TVPApiModule.Objects.Responses.UsageModule();

            retVal.objectID = response.m_nObjectID;
            retVal.virtualName = response.m_sVirtualName;
            retVal.maxNumberOfViews = response.m_nMaxNumberOfViews;
            retVal.viewLifeCycle = response.m_tsViewLifeCycle;
            retVal.maxUsageModuleLifeCycle = response.m_tsMaxUsageModuleLifeCycle;
            retVal.extDiscountId = response.m_ext_discount_id;
            retVal.internalDiscountID = response.m_internal_discount_id;
            retVal.pricingID = response.m_pricing_id;
            retVal.couponID = response.m_coupon_id;
            retVal.subscriptionOnly = response.m_subscription_only;
            retVal.isRenew = response.m_is_renew;
            retVal.numOfRecPeriods = response.m_num_of_rec_periods;
            retVal.deviceLimitID = response.m_device_limit_id;
            retVal.type = response.m_type;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.PreviewModule ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PreviewModule response)
        {
            TVPApiModule.Objects.Responses.PreviewModule retVal = new TVPApiModule.Objects.Responses.PreviewModule();

            retVal.id = response.m_nID;
            retVal.name = response.m_sName;
            retVal.fullLifeCycle = response.m_tsFullLifeCycle;
            retVal.nonRenewPeriod = response.m_tsNonRenewPeriod;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.CouponsGroup ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.CouponsGroup response)
        {
            TVPApiModule.Objects.Responses.CouponsGroup retVal = new TVPApiModule.Objects.Responses.CouponsGroup();

            if (response.m_oDiscountCode != null)
                retVal.discountModule = response.m_oDiscountCode.ToApiObject();

            retVal.discountCode = response.m_sDiscountCode;

            if (response.m_sDescription != null)
                retVal.description = response.m_sDescription.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            retVal.startDate = response.m_dStartDate;
            retVal.endDate = response.m_dEndDate;
            retVal.maxUseCountForCoupon = response.m_nMaxUseCountForCoupon;
            retVal.groupCode = response.m_sGroupCode;
            retVal.groupName = response.m_sGroupName;
            retVal.financialEntityID = response.m_nFinancialEntityID;
            retVal.maxRecurringUsesCountForCoupon = response.m_nMaxRecurringUsesCountForCoupon;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.WhenAlgo ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.WhenAlgo response)
        {
            TVPApiModule.Objects.Responses.WhenAlgo retVal = new TVPApiModule.Objects.Responses.WhenAlgo();

            retVal.algoType = (TVPApiModule.Objects.Responses.WhenAlgoType)response.m_eAlgoType;
            retVal.nTimes = response.m_nNTimes;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.SubscriptionsPricesContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.SubscriptionsPricesContainer response)
        {
            TVPApiModule.Objects.Responses.SubscriptionsPricesContainer retVal = new TVPApiModule.Objects.Responses.SubscriptionsPricesContainer();

            retVal.subscriptionCode = response.m_sSubscriptionCode;

            if (response.m_oPrice != null)
                retVal.price = response.m_oPrice.ToApiObject();

            retVal.priceReason = (TVPApiModule.Objects.Responses.PriceReason)response.m_PriceReason;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.UserBillingTransactionsResponse ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.UserBillingTransactionsResponse response)
        {
            TVPApiModule.Objects.Responses.UserBillingTransactionsResponse retVal = new TVPApiModule.Objects.Responses.UserBillingTransactionsResponse();

            retVal.siteGUID = response.m_sSiteGUID;

            if (response.m_BillingTransactionResponse != null)
                retVal.billingTransactionResponse = response.m_BillingTransactionResponse.ToApiObject();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.DomainBillingTransactionsResponse ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.DomainBillingTransactionsResponse response)
        {
            TVPApiModule.Objects.Responses.DomainBillingTransactionsResponse retVal = new TVPApiModule.Objects.Responses.DomainBillingTransactionsResponse();

            retVal.domainID = response.m_nDomainID;

            if (response.m_BillingTransactionResponses != null)
                retVal.billingTransactionResponses = response.m_BillingTransactionResponses.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.DomainResponseObject ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Domains.DomainResponseObject response)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject retVal = new TVPApiModule.Objects.Responses.DomainResponseObject();

            if (response.m_oDomain != null)
                retVal.domain = response.m_oDomain.ToApiObject();

            retVal.domainResponseStatus = (TVPApiModule.Objects.Responses.DomainResponseStatus)response.m_oDomainResponseStatus;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.Domain ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Domains.Domain response)
        {
            TVPApiModule.Objects.Responses.Domain retVal = new TVPApiModule.Objects.Responses.Domain();

            retVal.name = response.m_sName;
            retVal.description = response.m_sDescription;
            retVal.coGuid = response.m_sCoGuid;
            retVal.domainID = response.m_nDomainID;
            retVal.groupID = response.m_nGroupID;
            retVal.limit = response.m_nLimit;
            retVal.deviceLimit = response.m_nDeviceLimit;
            retVal.userLimit = response.m_nUserLimit;
            retVal.concurrentLimit = response.m_nConcurrentLimit;
            retVal.status = response.m_nStatus;
            retVal.isActive = response.m_nIsActive;
            retVal.usersIDs = response.m_UsersIDs;

            if (response.m_deviceFamilies != null)
                retVal.deviceFamilies = response.m_deviceFamilies.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            retVal.masterGUIDs = response.m_masterGUIDs;
            retVal.domainStatus = (TVPApiModule.Objects.Responses.DomainStatus)response.m_DomainStatus;
            retVal.frequencyFlag = response.m_frequencyFlag;
            retVal.nextActionFreq = response.m_NextActionFreq;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.DeviceContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Domains.DeviceContainer response)
        {
            TVPApiModule.Objects.Responses.DeviceContainer retVal = new TVPApiModule.Objects.Responses.DeviceContainer();

            retVal.deviceFamilyName = response.m_deviceFamilyName;
            retVal.deviceFamilyID = response.m_deviceFamilyID;
            retVal.deviceLimit = response.m_deviceLimit;
            retVal.deviceConcurrentLimit = response.m_deviceConcurrentLimit;

            if (response.DeviceInstances != null)
                retVal.deviceInstances = response.DeviceInstances.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.Device ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Domains.Device response)
        {
            TVPApiModule.Objects.Responses.Device retVal = new TVPApiModule.Objects.Responses.Device();

            retVal.id = response.m_id;
            retVal.deviceUDID = response.m_deviceUDID;
            retVal.deviceBrand = response.m_deviceBrand;
            retVal.deviceFamily = response.m_deviceFamily;
            retVal.deviceFamilyID = response.m_deviceFamilyID;
            retVal.domainID = response.m_domainID;
            retVal.deviceName = response.m_deviceName;
            retVal.deviceBrandID = response.m_deviceBrandID;
            retVal.pin = response.m_pin;
            retVal.activationDate = response.m_activationDate;
            retVal.state = (TVPApiModule.Objects.Responses.DeviceState)response.m_state;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.DeviceResponseObject ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Domains.DeviceResponseObject response)
        {
            TVPApiModule.Objects.Responses.DeviceResponseObject retVal = new TVPApiModule.Objects.Responses.DeviceResponseObject();

            if (response.m_oDevice != null)
                retVal.device = response.m_oDevice.ToApiObject();

            retVal.deviceResponseStatus = (TVPApiModule.Objects.Responses.DeviceResponseStatus)response.m_oDeviceResponseStatus;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.PPVModule ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.PPVModule response)
        {
            TVPApiModule.Objects.Responses.PPVModule retVal = new TVPApiModule.Objects.Responses.PPVModule();

            if (response.m_oPriceCode != null)
                retVal.priceCode = response.m_oPriceCode.ToApiObject();

            if (response.m_oUsageModule != null)
                retVal.usageModule = response.m_oUsageModule.ToApiObject();

            if (response.m_oDiscountModule != null)
                retVal.discountModule = response.m_oDiscountModule.ToApiObject();

            if (response.m_oCouponsGroup != null)
                retVal.couponsGroup = response.m_oCouponsGroup.ToApiObject();

            if (response.m_sDescription != null)
                retVal.description = response.m_sDescription.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            retVal.objectCode = response.m_sObjectCode;
            retVal.objectVirtualName = response.m_sObjectVirtualName;
            retVal.subscriptionOnly = response.m_bSubscriptionOnly;
            retVal.relatedFileTypes = response.m_relatedFileTypes;
            retVal.productCode = response.m_Product_Code;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.PriceCode ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.PriceCode response)
        {
            TVPApiModule.Objects.Responses.PriceCode retVal = new TVPApiModule.Objects.Responses.PriceCode();

            retVal.code = response.m_sCode;

            if (response.m_oPrise != null)
                retVal.prise = response.m_oPrise.ToApiObject();

            retVal.objectID = response.m_nObjectID;

            if (response.m_sDescription != null)
                retVal.description = response.m_sDescription.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.UsageModule ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.UsageModule response)
        {
            TVPApiModule.Objects.Responses.UsageModule retVal = new TVPApiModule.Objects.Responses.UsageModule();

            retVal.objectID = response.m_nObjectID;
            retVal.virtualName = response.m_sVirtualName;
            retVal.maxNumberOfViews = response.m_nMaxNumberOfViews;
            retVal.viewLifeCycle = response.m_tsViewLifeCycle;
            retVal.maxUsageModuleLifeCycle = response.m_tsMaxUsageModuleLifeCycle;
            retVal.extDiscountId = response.m_ext_discount_id;
            retVal.internalDiscountID = response.m_internal_discount_id;
            retVal.pricingID = response.m_pricing_id;
            retVal.couponID = response.m_coupon_id;
            retVal.subscriptionOnly = response.m_subscription_only;
            retVal.isRenew = response.m_is_renew;
            retVal.numOfRecPeriods = response.m_num_of_rec_periods;
            retVal.deviceLimitID = response.m_device_limit_id;
            retVal.type = response.m_type;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.DiscountModule ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.DiscountModule response)
        {
            TVPApiModule.Objects.Responses.DiscountModule retVal = new TVPApiModule.Objects.Responses.DiscountModule();

            retVal.percent = response.m_dPercent;
            retVal.theRelationType = (TVPApiModule.Objects.Responses.RelationTypes)response.m_eTheRelationType;
            retVal.startDate = response.m_dStartDate;
            retVal.endDate = response.m_dEndDate;

            if (response.m_oWhenAlgo != null)
                retVal.whenAlgo = response.m_oWhenAlgo.ToApiObject();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.CouponsGroup ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.CouponsGroup response)
        {
            TVPApiModule.Objects.Responses.CouponsGroup retVal = new TVPApiModule.Objects.Responses.CouponsGroup();

            if (response.m_oDiscountCode != null)
                retVal.discountModule = response.m_oDiscountCode.ToApiObject();

            retVal.discountCode = response.m_sDiscountCode;

            if (response.m_sDescription != null)
                retVal.description = response.m_sDescription.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            retVal.startDate = response.m_dStartDate;
            retVal.endDate = response.m_dEndDate;
            retVal.maxUseCountForCoupon = response.m_nMaxUseCountForCoupon;
            retVal.groupCode = response.m_sGroupCode;
            retVal.groupName = response.m_sGroupName;
            retVal.financialEntityID = response.m_nFinancialEntityID;
            retVal.maxRecurringUsesCountForCoupon = response.m_nMaxRecurringUsesCountForCoupon;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.LanguageContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.LanguageContainer response)
        {
            TVPApiModule.Objects.Responses.LanguageContainer retVal = new TVPApiModule.Objects.Responses.LanguageContainer();

            retVal.languageCode3 = response.m_sLanguageCode3;
            retVal.value = response.m_sValue;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.Price ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.Price response)
        {
            TVPApiModule.Objects.Responses.Price retVal = new TVPApiModule.Objects.Responses.Price();

            retVal.price = response.m_dPrice;

            if (response.m_oCurrency != null)
                retVal.currency = response.m_oCurrency.ToApiObject();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.WhenAlgo ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.WhenAlgo response)
        {
            TVPApiModule.Objects.Responses.WhenAlgo retVal = new TVPApiModule.Objects.Responses.WhenAlgo();

            retVal.algoType = (TVPApiModule.Objects.Responses.WhenAlgoType)response.m_eAlgoType;
            retVal.nTimes = response.m_nNTimes;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.Currency ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.Currency response)
        {
            TVPApiModule.Objects.Responses.Currency retVal = new TVPApiModule.Objects.Responses.Currency();

            retVal.currencyID = response.m_nCurrencyID;
            retVal.currencyCD2 = response.m_sCurrencyCD2;
            retVal.currencyCD3 = response.m_sCurrencyCD3;
            retVal.currencySign = response.m_sCurrencySign;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.CouponData ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.CouponData response)
        {
            TVPApiModule.Objects.Responses.CouponData retVal = new TVPApiModule.Objects.Responses.CouponData();

            retVal.couponStatus = (TVPApiModule.Objects.Responses.CouponsStatus)response.m_CouponStatus;

            if (response.m_oCouponGroup != null)
                retVal.couponGroup = response.m_oCouponGroup.ToApiObject();

            retVal.couponType = (TVPApiModule.Objects.Responses.CouponType)response.m_CouponType;
            retVal.campID = response.m_campID;
            retVal.ownerGUID = response.m_ownerGUID;
            retVal.ownerMedia = response.m_ownerMedia;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.Subscription ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.Subscription response)
        {
            TVPApiModule.Objects.Responses.Subscription retVal = new TVPApiModule.Objects.Responses.Subscription();

            if (response.m_sCodes != null)
                retVal.codes = response.m_sCodes.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            retVal.startDate = response.m_dStartDate;
            retVal.endDate = response.m_dEndDate;
            retVal.fileTypes = response.m_sFileTypes;
            retVal.isRecurring = response.m_bIsRecurring;
            retVal.numberOfRecPeriods = response.m_nNumberOfRecPeriods;

            if (response.m_oSubscriptionPriceCode != null)
                retVal.subscriptionPriceCode = response.m_oSubscriptionPriceCode.ToApiObject();

            if (response.m_oExtDisountModule != null)            
                retVal.extDisountModule = response.m_oExtDisountModule.ToApiObject();

            if (response.m_sName != null)
                retVal.name = response.m_sName.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            if (response.m_oSubscriptionUsageModule != null)
                retVal.subscriptionUsageModule = response.m_oSubscriptionUsageModule.ToApiObject();

            retVal.fictivicMediaID = response.m_fictivicMediaID;
            retVal.priority = response.m_Priority;
            retVal.productCode = response.m_ProductCode;
            retVal.subscriptionCode = response.m_SubscriptionCode;

            if (response.m_oSubscriptionUsageModule != null)
                retVal.multiSubscriptionUsageModule = response.m_MultiSubscriptionUsageModule.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();
            retVal.geoCommerceID = response.n_GeoCommerceID;
            retVal.isInfiniteRecurring = response.m_bIsInfiniteRecurring;

            if (response.m_oPreviewModule != null)
                retVal.previewModule = response.m_oPreviewModule.ToApiObject();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.SubscriptionCodeContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.SubscriptionCodeContainer response)
        {
            TVPApiModule.Objects.Responses.SubscriptionCodeContainer retVal = new TVPApiModule.Objects.Responses.SubscriptionCodeContainer();

            retVal.code = response.m_sCode;
            retVal.name = response.m_sName;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.PreviewModule ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.PreviewModule response)
        {
            TVPApiModule.Objects.Responses.PreviewModule retVal = new TVPApiModule.Objects.Responses.PreviewModule();

            retVal.id = response.m_nID;
            retVal.name = response.m_sName;
            retVal.fullLifeCycle = response.m_tsFullLifeCycle;
            retVal.nonRenewPeriod = response.m_tsNonRenewPeriod;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.GroupOperator ToApiObject(this TVPPro.SiteManager.TvinciPlatform.api.GroupOperator response)
        {
            TVPApiModule.Objects.Responses.GroupOperator retVal = new TVPApiModule.Objects.Responses.GroupOperator();

            if (response.UIData != null)
                retVal.uiData = response.UIData.ToApiObject();

            retVal.id = response.ID;
            retVal.name = response.Name;
            retVal.type = (TVPApiModule.Objects.Responses.eOperatorType)response.Type;
            retVal.loginUrl = response.LoginUrl;
            retVal.subGroupID = response.SubGroupID;

            if (response.Scopes != null)
                retVal.scopes = response.Scopes.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            retVal.groupUserName = response.GroupUserName;
            retVal.groupPassword = response.GroupPassword;
            retVal.logoutURL = response.LogoutURL;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.Scope ToApiObject(this TVPPro.SiteManager.TvinciPlatform.api.Scope response)
        {
            TVPApiModule.Objects.Responses.Scope retVal = new TVPApiModule.Objects.Responses.Scope();

            retVal.loginUrl = response.LoginUrl;
            retVal.logoutUrl = response.LogoutUrl;
            retVal.name = response.Name;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.UIData ToApiObject(this TVPPro.SiteManager.TvinciPlatform.api.UIData response)
        {
            TVPApiModule.Objects.Responses.UIData retVal = new TVPApiModule.Objects.Responses.UIData();

            retVal.colorCode = response.ColorCode;
            retVal.picID = response.picID;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.UserSocialActionObject ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Social.UserSocialActionObject response)
        {
            TVPApiModule.Objects.Responses.UserSocialActionObject retVal = new TVPApiModule.Objects.Responses.UserSocialActionObject();

            retVal.siteGuid = response.m_sSiteGuid;
            retVal.socialAction = (TVPApiModule.Objects.Responses.eUserAction)response.m_eSocialAction;
            retVal.socialPlatform = (TVPApiModule.Objects.Responses.SocialPlatform)response.m_eSocialPlatform;
            retVal.mediaID = response.nMediaID;
            retVal.programID = response.nProgramID;
            retVal.assetType = (TVPApiModule.Objects.Responses.eAssetType)response.assetType;
            retVal.actionDate = response.m_dActionDate;


            return retVal;
        }

        public static TVPApiModule.Objects.Responses.UserBasicData ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData response)
        {
            TVPApiModule.Objects.Responses.UserBasicData retVal = new TVPApiModule.Objects.Responses.UserBasicData();

            retVal.isFacebookImagePermitted = response.m_bIsFacebookImagePermitted;
            retVal.coGuid = response.m_CoGuid;

            if (response.m_Country != null)
                retVal.country = response.m_Country.ToApiObject();

            retVal.externalToken = response.m_ExternalToken;
            retVal.address = response.m_sAddress;
            retVal.affiliateCode = response.m_sAffiliateCode;
            retVal.city = response.m_sCity;
            retVal.email = response.m_sEmail;
            retVal.facebookID = response.m_sFacebookID;
            retVal.facebookImage = response.m_sFacebookImage;
            retVal.facebookToken = response.m_sFacebookToken;
            retVal.firstName = response.m_sFirstName;
            retVal.lastName = response.m_sLastName;
            retVal.phone = response.m_sPhone;

            if (response.m_State != null)            
                retVal.state = response.m_State.ToApiObject();

            retVal.userName = response.m_sUserName;
            retVal.zip = response.m_sZip;

            if (response.m_UserType != null)                        
                retVal.userType = response.m_UserType.ToApiObject();


            return retVal;
        }

        public static TVPApiModule.Objects.Responses.FriendWatchedObject ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Social.FriendWatchedObject response)
        {
            TVPApiModule.Objects.Responses.FriendWatchedObject retVal = new TVPApiModule.Objects.Responses.FriendWatchedObject();

            retVal.siteGuid = response.SiteGuid;
            retVal.mediaID = response.MediaID;
            retVal.updateDate = response.UpdateDate;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.FacebookConfig ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Social.FacebookConfig response)
        {
            TVPApiModule.Objects.Responses.FacebookConfig retVal = new TVPApiModule.Objects.Responses.FacebookConfig();

            retVal.fbKey = response.sFBKey;
            retVal.fbSecret = response.sFBSecret;
            retVal.fbCallback = response.sFBCallback;
            retVal.fbMinFriends = response.nFBMinFriends;
            retVal.fbPermissions = response.sFBPermissions;
            retVal.fbRedirect = response.sFBRedirect;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.FBInterestData ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Social.FBInterestData response)
        {
            TVPApiModule.Objects.Responses.FBInterestData retVal = new TVPApiModule.Objects.Responses.FBInterestData();

            retVal.name = response.name;
            retVal.category = response.category;
            retVal.id = response.id;
            retVal.createdTime = response.created_time;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.FBInterest ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Social.FBInterest response)
        {
            TVPApiModule.Objects.Responses.FBInterest retVal = new TVPApiModule.Objects.Responses.FBInterest();

            if (response.data != null)
                retVal.data = response.data.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.FBLoaction ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Social.FBLoaction response)
        {
            TVPApiModule.Objects.Responses.FBLoaction retVal = new TVPApiModule.Objects.Responses.FBLoaction();

            retVal.name = response.name;
            retVal.id = response.id;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.FBUser ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Social.FBUser response)
        {
            TVPApiModule.Objects.Responses.FBUser retVal = new TVPApiModule.Objects.Responses.FBUser();

            retVal.siteGuid = response.m_sSiteGuid;
            retVal.birthday = response.Birthday;

            if (response.Location != null)
                retVal.location = response.Location.ToApiObject();

            if (response.interests != null)
                retVal.interests = response.interests.ToApiObject();

            retVal.name = response.name;
            retVal.id = response.id;
            retVal.uid = response.uid;
            retVal.firstName = response.first_name;
            retVal.lastName = response.last_name;
            retVal.email = response.email;
            retVal.gender = response.gender;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.FacebookResponseObject ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Social.FacebookResponseObject response)
        {
            TVPApiModule.Objects.Responses.FacebookResponseObject retVal = new TVPApiModule.Objects.Responses.FacebookResponseObject();

            retVal.status = response.status;
            retVal.siteGuid = response.siteGuid;
            retVal.tvinciName = response.tvinciName;
            retVal.facebookName = response.facebookName;
            retVal.pic = response.pic;
            retVal.data = response.data;
            retVal.minFriends = response.minFriends;
            if (response.fbUser != null)
                retVal.fbUser = response.fbUser.ToApiObject();

            retVal.token = response.token;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.KeyValuePair ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair response)
        {
            TVPApiModule.Objects.Responses.KeyValuePair retVal = new TVPApiModule.Objects.Responses.KeyValuePair();

            retVal.key = response.key;
            retVal.value = response.value;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.KeyValuePair ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Users.KeyValuePair response)
        {
            TVPApiModule.Objects.Responses.KeyValuePair retVal = new TVPApiModule.Objects.Responses.KeyValuePair();

            retVal.key = response.key;
            retVal.value = response.value;

            return retVal;
        }
    }
}
