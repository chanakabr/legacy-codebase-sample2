using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApiModule.Objects;
using TVPApiModule.Objects.Responses;
using TVPPro.SiteManager.Objects;

namespace TVPApiModule.Extentions
{
    public static class TranslateExtensions
    {
        public static TVPPro.SiteManager.TvinciPlatform.Users.Country ToTvmObject(this Country response)
        {
            TVPPro.SiteManager.TvinciPlatform.Users.Country country = new TVPPro.SiteManager.TvinciPlatform.Users.Country();

            country.m_nObjecrtID = response.object_id;
            country.m_sCountryCode = response.country_code;
            country.m_sCountryName = response.country_name;

            return country;
        }

        public static TVPPro.SiteManager.TvinciPlatform.Users.State ToTvmObject(this TVPApiModule.Objects.Responses.State response)
        {
            TVPPro.SiteManager.TvinciPlatform.Users.State state = new TVPPro.SiteManager.TvinciPlatform.Users.State();

            if (state.m_Country != null)
                state.m_Country = response.country.ToTvmObject();

            state.m_nObjecrtID = response.object_id;
            state.m_sStateCode = response.state_code;
            state.m_sStateName = response.state_name;

            return state;
        }

        public static TVPPro.SiteManager.TvinciPlatform.Users.UserType ToTvmObject(this TVPApiModule.Objects.Responses.UserType response)
        {
            TVPPro.SiteManager.TvinciPlatform.Users.UserType userType = new TVPPro.SiteManager.TvinciPlatform.Users.UserType();

            userType.Description = response.description;
            userType.ID = response.id;
            userType.IsDefault = response.is_default;

            return userType;
        }

        public static TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData ToTvmObject(this TVPApiModule.Objects.Responses.UserBasicData response)
        {
            TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData basic_data = new TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData();

            if (response != null)
                {
                    basic_data.m_bIsFacebookImagePermitted = response.is_facebook_image_permitted;
                    basic_data.m_CoGuid = response.co_guid;

                    if (response.country != null)
                    {
                        basic_data.m_Country = response.country.ToTvmObject();
                    }

                    basic_data.m_ExternalToken = response.external_token;
                    basic_data.m_sAddress= response.address;
                    basic_data.m_sAffiliateCode = response.affiliate_code;
                    basic_data.m_sCity = response.city;
                    basic_data.m_sEmail = response.email;
                    basic_data.m_sFacebookID = response.facebook_id;
                    basic_data.m_sFacebookImage = response.facebook_image;
                    basic_data.m_sFacebookToken = response.facebook_token;
                    basic_data.m_sFirstName = response.first_name;
                    basic_data.m_sLastName = response.last_name;
                    basic_data.m_sPhone = response.phone;

                    if (response.state != null)
                        basic_data.m_State = response.state.ToTvmObject();

                    basic_data.m_sUserName = response.user_name;
                    basic_data.m_sZip = response.zip;

                    basic_data.m_UserType = response.user_type.ToTvmObject();
                }

            return basic_data;
        }

        public static TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData ToTvmObject(this TVPApiModule.Objects.Responses.UserDynamicData response)
        {
            TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData dynamic_data = new TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData();

            if (response != null)
            {
                if (response.user_data != null)
                {
                    dynamic_data.m_sUserData = response.user_data.Select(x => new TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicDataContainer()
                    {
                        m_sDataType = x.data_type,
                        m_sValue = x.value                        
                    }).ToArray();
                }
            }

            return dynamic_data;
        }

        public static TVPApiModule.Objects.Responses.UserDynamicData ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData response)
        {
            TVPApiModule.Objects.Responses.UserDynamicData retVal = new TVPApiModule.Objects.Responses.UserDynamicData();

            if (response != null)
            {
                if (response.m_sUserData != null)
                {
                    retVal.user_data = response.m_sUserData.Select(x => new TVPApiModule.Objects.Responses.UserDynamicDataContainer()
                    {
                        data_type = x.m_sDataType,
                        value = x.m_sValue
                    }).ToArray();
                }
            }

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.UserResponseObject ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject response)
        {
            TVPApiModule.Objects.Responses.UserResponseObject retVal = new TVPApiModule.Objects.Responses.UserResponseObject();

            retVal.resp_status = (TVPApiModule.Objects.Responses.eResponseStatus)response.m_RespStatus;

            if (response.m_user != null)
            {
                retVal.user = new TVPApiModule.Objects.Responses.User();

                retVal.user.domain_id = response.m_user.m_domianID;
                retVal.user.user_state = (TVPApiModule.Objects.Responses.UserState)response.m_user.m_eUserState;
                retVal.user.is_domain_master = response.m_user.m_isDomainMaster;
                retVal.user.sso_operator_id = response.m_user.m_nSSOOperatorID;

                if (response.m_user.m_oBasicData != null)
                {
                    retVal.user.basic_data = new TVPApiModule.Objects.Responses.UserBasicData();

                    retVal.user.basic_data.is_facebook_image_permitted = response.m_user.m_oBasicData.m_bIsFacebookImagePermitted;
                    retVal.user.basic_data.co_guid = response.m_user.m_oBasicData.m_CoGuid;

                    if (retVal.user.basic_data.country != null)
                    {
                        retVal.user.basic_data.country = response.m_user.m_oBasicData.m_Country.ToApiObject();
                    }

                    retVal.user.basic_data.external_token = response.m_user.m_oBasicData.m_ExternalToken;
                    retVal.user.basic_data.address = response.m_user.m_oBasicData.m_sAddress;
                    retVal.user.basic_data.affiliate_code = response.m_user.m_oBasicData.m_sAffiliateCode;
                    retVal.user.basic_data.city = response.m_user.m_oBasicData.m_sCity;
                    retVal.user.basic_data.email = response.m_user.m_oBasicData.m_sEmail;
                    retVal.user.basic_data.facebook_id = response.m_user.m_oBasicData.m_sFacebookID;
                    retVal.user.basic_data.facebook_image = response.m_user.m_oBasicData.m_sFacebookImage;
                    retVal.user.basic_data.facebook_token = response.m_user.m_oBasicData.m_sFacebookToken;
                    retVal.user.basic_data.first_name = response.m_user.m_oBasicData.m_sFirstName;
                    retVal.user.basic_data.last_name = response.m_user.m_oBasicData.m_sLastName;
                    retVal.user.basic_data.phone = response.m_user.m_oBasicData.m_sPhone;
                    
                    if (retVal.user.basic_data.state != null)
                        retVal.user.basic_data.state = response.m_user.m_oBasicData.m_State.ToApiObject();

                    retVal.user.basic_data.user_name = response.m_user.m_oBasicData.m_sUserName;
                    retVal.user.basic_data.zip = response.m_user.m_oBasicData.m_sZip;

                    retVal.user.basic_data.user_type = response.m_user.m_oBasicData.m_UserType.ToApiObject();
                }

                if (response.m_user.m_oDynamicData != null)
                {
                    retVal.user.dynamic_data = response.m_user.m_oDynamicData.ToApiObject();
                }

                retVal.user.site_guid = response.m_user.m_sSiteGUID;
            }

            retVal.user_instance_id = response.m_userInstanceID;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.PermittedSubscriptionContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PermittedSubscriptionContainer response)
        {
            TVPApiModule.Objects.Responses.PermittedSubscriptionContainer retVal = new TVPApiModule.Objects.Responses.PermittedSubscriptionContainer();

            retVal.is_sub_renewable = response.m_bIsSubRenewable;
            retVal.recurring_status = response.m_bRecurringStatus;
            retVal.current_date = response.m_dCurrentDate;
            retVal.end_date = response.m_dEndDate;
            retVal.last_view_date = response.m_dLastViewDate;
            retVal.next_renewal_date = response.m_dNextRenewalDate;
            retVal.purchase_date = response.m_dPurchaseDate;
            retVal.current_uses = response.m_nCurrentUses;
            retVal.max_uses = response.m_nMaxUses;
            retVal.subscription_purchase_id = response.m_nSubscriptionPurchaseID;
            retVal.payment_method = (TVPApiModule.Objects.Responses.PaymentMethod)response.m_paymentMethod;
            retVal.device_name = response.m_sDeviceName;
            retVal.device_udid = response.m_sDeviceUDID;
            retVal.subscription_code = response.m_sSubscriptionCode;
            retVal.is_cancel_window = response.m_bCancelWindow;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.PermittedMediaContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PermittedMediaContainer response)
        {
            TVPApiModule.Objects.Responses.PermittedMediaContainer retVal = new TVPApiModule.Objects.Responses.PermittedMediaContainer();

            retVal.current_date = response.m_dCurrentDate;
            retVal.end_date = response.m_dEndDate;
            retVal.purchase_date = response.m_dPurchaseDate;
            retVal.current_uses = response.m_nCurrentUses;
            retVal.max_uses = response.m_nMaxUses;
            retVal.media_file_id = response.m_nMediaFileID;
            retVal.media_id = response.m_nMediaID;
            retVal.purchase_method = (TVPApiModule.Objects.Responses.PaymentMethod)response.m_purchaseMethod;
            retVal.device_name = response.m_sDeviceName;
            retVal.device_udid = response.m_sDeviceUDID;
            retVal.is_cancel_window = response.m_bCancelWindow;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.FavoriteObject ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Users.FavoritObject response)
        {
            TVPApiModule.Objects.Responses.FavoriteObject retVal = new TVPApiModule.Objects.Responses.FavoriteObject();

            retVal.update_date = response.m_dUpdateDate;
            retVal.is_channel = response.m_is_channel;
            retVal.domain_id = response.m_nDomainID;
            retVal.id = response.m_nID;
            retVal.device_name = response.m_sDeviceName;
            retVal.device_udid = response.m_sDeviceUDID;
            retVal.extra_data = response.m_sExtraData;
            retVal.item_code = response.m_sItemCode;
            retVal.site_user_guid = response.m_sSiteUserGUID;
            retVal.type = response.m_sType;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.GroupRule ToApiObject(this TVPPro.SiteManager.TvinciPlatform.api.GroupRule response)
        {
            TVPApiModule.Objects.Responses.GroupRule retVal = new TVPApiModule.Objects.Responses.GroupRule();

            retVal.all_tag_values = response.AllTagValues;
            retVal.block_type = (TVPApiModule.Objects.Responses.eBlockType)response.BlockType;
            retVal.dynamic_data_key = response.DynamicDataKey;
            retVal.group_rule_type = (TVPApiModule.Objects.Responses.eGroupRuleType)response.GroupRuleType;
            retVal.is_active = response.IsActive;
            retVal.name = response.Name;
            retVal.rule_id = response.RuleID;
            retVal.tag_type_id = response.TagTypeID;
            retVal.tag_value = response.TagValue;

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
            retVal.group_id = response.nGroupID;
            retVal.location_sec = response.nLocationSec;
            retVal.media_id = response.nMediaID;
            retVal.device_id = response.sDeviceID;
            retVal.device_name = response.sDeviceName;
            retVal.site_guid = response.sSiteGUID;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.UserItemList ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Users.UserItemList response)
        {
            TVPApiModule.Objects.Responses.UserItemList retVal = new TVPApiModule.Objects.Responses.UserItemList();

            if (response.itemObj != null)
            {
                retVal.item_obj = response.itemObj.Select(x => new TVPApiModule.Objects.Responses.ItemObj()
                {
                    item = x.item,
                    order_num = x.orderNum
                }).ToArray();
            }

            retVal.item_type = (TVPApiModule.Objects.Responses.ItemType)response.itemType;
            retVal.list_type = (TVPApiModule.Objects.Responses.ListType)response.listType;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.State ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Users.State response)
        {
            TVPApiModule.Objects.Responses.State retVal = new TVPApiModule.Objects.Responses.State();
            
            if (response.m_Country != null)
                retVal.country = response.m_Country.ToApiObject();

            retVal.object_id = response.m_nObjecrtID;
            retVal.state_code = response.m_sStateCode;
            retVal.state_name = response.m_sStateName;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.Country ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Users.Country response)
        {
            TVPApiModule.Objects.Responses.Country retVal = new TVPApiModule.Objects.Responses.Country();

            retVal.object_id = response.m_nObjecrtID;
            retVal.country_code = response.m_sCountryCode;
            retVal.country_name = response.m_sCountryName;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.UserType ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Users.UserType response)
        {
            TVPApiModule.Objects.Responses.UserType retVal = new TVPApiModule.Objects.Responses.UserType();

            retVal.description = response.Description;
            retVal.id = response.ID;
            retVal.is_default = response.IsDefault;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.EPGChannel ToApiObject(this TVPPro.SiteManager.TvinciPlatform.api.EPGChannelObject response)
        {
            TVPApiModule.Objects.Responses.EPGChannel retVal = new TVPApiModule.Objects.Responses.EPGChannel();

            retVal.channel_id = response.CHANNEL_ID;
            retVal.create_date = response.CREATE_DATE;
            retVal.description = response.DESCRIPTION;
            retVal.editor_remarks = response.EDITOR_REMARKS;
            retVal.epg_channel_id = response.EPG_CHANNEL_ID;
            retVal.group_id = response.GROUP_ID;
            retVal.is_active = response.IS_ACTIVE;
            retVal.media_id = response.MEDIA_ID;
            retVal.name = response.NAME;
            retVal.order_num = response.ORDER_NUM;
            retVal.pic_url = response.PIC_URL;
            retVal.publish_date = response.PUBLISH_DATE;
            retVal.status = response.STATUS;
            retVal.updater_id = response.UPDATER_ID;

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

        public static TVPApiModule.Objects.Responses.EPGMultiChannelProgrammeObject ToApiObject(this TVPPro.SiteManager.Objects.EPGMultiChannelProgrammeObject response)
        {
            TVPApiModule.Objects.Responses.EPGMultiChannelProgrammeObject retVal = new TVPApiModule.Objects.Responses.EPGMultiChannelProgrammeObject();

            retVal.epg_channel_id = response.EPG_CHANNEL_ID;

            if (response.EPGChannelProgrammeObject != null)
                retVal.epg_channel_program_object = response.EPGChannelProgrammeObject.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.EPGChannelProgrammeObject ToApiObject(this TVPPro.SiteManager.TvinciPlatform.api.EPGChannelProgrammeObject response)
        {
            TVPApiModule.Objects.Responses.EPGChannelProgrammeObject retVal = new TVPApiModule.Objects.Responses.EPGChannelProgrammeObject();

            retVal.create_date = response.CREATE_DATE;
            retVal.description = response.DESCRIPTION;
            retVal.end_date = response.END_DATE;
            retVal.epg_channel_id = response.EPG_CHANNEL_ID;
            retVal.epg_id = response.EPG_ID;
            retVal.epg_identifier = response.EPG_IDENTIFIER;

            if (response.EPG_Meta != null)
                response.EPG_Meta.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            if (response.EPG_TAGS != null)
                response.EPG_TAGS.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            retVal.group_id = response.GROUP_ID;
            retVal.is_active = response.IS_ACTIVE;
            retVal.like_counter = response.LIKE_COUNTER;
            retVal.media_id = response.media_id;
            retVal.name = response.NAME;
            retVal.pic_url = response.PIC_URL;
            retVal.publish_date = response.PUBLISH_DATE;
            retVal.start_date = response.START_DATE;
            retVal.status = response.STATUS;
            retVal.update_date = response.UPDATE_DATE;
            retVal.updater_id = response.UPDATER_ID;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.EPGChannelProgrammeObject ToApiObject(this Tvinci.Data.Loaders.TvinciPlatform.Catalog.EPGChannelProgrammeObject response)
        {
            TVPApiModule.Objects.Responses.EPGChannelProgrammeObject retVal = new TVPApiModule.Objects.Responses.EPGChannelProgrammeObject();

            retVal.create_date = response.CREATE_DATE;
            retVal.description = response.DESCRIPTION;
            retVal.end_date = response.END_DATE;
            retVal.epg_channel_id = response.EPG_CHANNEL_ID;
            retVal.epg_id = response.EPG_ID;
            retVal.epg_identifier = response.EPG_IDENTIFIER;

            if (response.EPG_Meta != null)
                retVal.epg_meta = response.EPG_Meta.Select(x => x.ToApiObject()).ToArray();

            if (response.EPG_TAGS != null)
                retVal.epg_tags = response.EPG_TAGS.Select(x => x.ToApiObject()).ToArray();

            retVal.group_id = response.GROUP_ID;
            retVal.is_active = response.IS_ACTIVE;
            retVal.like_counter = response.LIKE_COUNTER;
            retVal.media_id = response.media_id;
            retVal.name = response.NAME;
            retVal.pic_url = response.PIC_URL;
            retVal.publish_date = response.PUBLISH_DATE;
            retVal.start_date = response.START_DATE;
            retVal.status = response.STATUS;
            retVal.update_date = response.UPDATE_DATE;
            retVal.updater_id = response.UPDATER_ID;

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
            retVal.external_receipt_code = response.m_sExternalReceiptCode;
            retVal.reciept_code = response.m_sRecieptCode;
            retVal.status_description = response.m_sStatusDescription;
            
            return retVal;
        }

        public static TVPApiModule.Objects.Responses.Currency ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.Currency response)
        {
            TVPApiModule.Objects.Responses.Currency retVal = new TVPApiModule.Objects.Responses.Currency();

            retVal.currency_id = response.m_nCurrencyID;
            retVal.currency_cd2 = response.m_sCurrencyCD2;
            retVal.currency_cd3 = response.m_sCurrencyCD3;
            retVal.currency_sign = response.m_sCurrencySign;

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

            retVal.is_recurring = response.m_bIsRecurring;
            retVal.action_date = response.m_dtActionDate;
            retVal.end_date = response.m_dtEndDate;
            retVal.start_date = response.m_dtStartDate;
            retVal.billing_action = (TVPApiModule.Objects.Responses.BillingAction)response.m_eBillingAction;
            retVal.item_type = (TVPApiModule.Objects.Responses.BillingItemsType)response.m_eItemType;
            retVal.payment_method = (TVPApiModule.Objects.Responses.PaymentMethod)response.m_ePaymentMethod;
            retVal.billing_provider_ref = response.m_nBillingProviderRef;
            retVal.purchase_id = response.m_nPurchaseID;

            if (response.m_Price != null)
                retVal.price = response.m_Price.ToApiObject();

            retVal.payment_method_extra_details = response.m_sPaymentMethodExtraDetails;
            retVal.purchased_item_code = response.m_sPurchasedItemCode;
            retVal.purchased_item_name = response.m_sPurchasedItemName;
            retVal.reciept_code = response.m_sRecieptCode;
            retVal.remarks = response.m_sRemarks; 

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.BillingTransactionsResponse ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.BillingTransactionsResponse response)
        {
            TVPApiModule.Objects.Responses.BillingTransactionsResponse retVal = new TVPApiModule.Objects.Responses.BillingTransactionsResponse();

            retVal.transactions_count = response.m_nTransactionsCount;
            if (response.m_Transactions != null)
                retVal.transactions = response.m_Transactions.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.AdyenBillingDetail ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Billing.AdyenBillingDetail response)
        {
            TVPApiModule.Objects.Responses.AdyenBillingDetail retVal = new TVPApiModule.Objects.Responses.AdyenBillingDetail();

            if (response.billingInfo != null)
                retVal.billing_info = response.billingInfo.ToApiObject();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.BillingInfo ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Billing.BillingInfo response)
        {
            TVPApiModule.Objects.Responses.BillingInfo retVal = new TVPApiModule.Objects.Responses.BillingInfo();

            retVal.cvc = response.cvc;
            retVal.expiry_month = response.expiryMonth;
            retVal.expiry_year = response.expiryYear;
            retVal.holder_name = response.holderName;
            retVal.last_four_digits = response.lastFourDigits;
            retVal.variant = response.variant;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.CampaignActionInfo ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.CampaignActionInfo response)
        {
            TVPApiModule.Objects.Responses.CampaignActionInfo retVal = new TVPApiModule.Objects.Responses.CampaignActionInfo();

            retVal.media_id = response.m_mediaID;
            retVal.site_guid = response.m_siteGuid;
            retVal.media_link = response.m_mediaLink;
            retVal.sender_name = response.m_senderName;
            retVal.sender_email = response.m_senderEmail;
            retVal.status = (Objects.Responses.CampaignActionResult)response.m_status;
            
            if (response.m_socialInviteInfo != null)
                retVal.social_invite_info = response.m_socialInviteInfo.ToApiObject();

            if (response.m_voucherReceipents != null)
            retVal.voucher_receipents = response.m_voucherReceipents.Where(x => x != null).Select(x => x.ToApiObject()).ToArray(); 

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.VoucherReceipentInfo ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.VoucherReceipentInfo response)
        {
            TVPApiModule.Objects.Responses.VoucherReceipentInfo retVal = new TVPApiModule.Objects.Responses.VoucherReceipentInfo();

            retVal.email_add = response.m_emailAdd;
            retVal.receipent_name = response.m_receipentName;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.SocialInviteInfo ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.SocialInviteInfo response)
        {
            TVPApiModule.Objects.Responses.SocialInviteInfo retVal = new TVPApiModule.Objects.Responses.SocialInviteInfo();

            retVal.hash_code = response.m_hashCode;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.MediaFileItemPricesContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.MediaFileItemPricesContainer response)
        {
            TVPApiModule.Objects.Responses.MediaFileItemPricesContainer retVal = new TVPApiModule.Objects.Responses.MediaFileItemPricesContainer();

            retVal.media_file_id = response.m_nMediaFileID;

            if (response.m_oItemPrices != null)
                retVal.item_prices = response.m_oItemPrices.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            retVal.product_code = response.m_sProductCode;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.ItemPriceContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.ItemPriceContainer response)
        {
            TVPApiModule.Objects.Responses.ItemPriceContainer retVal = new TVPApiModule.Objects.Responses.ItemPriceContainer();

            retVal.ppv_module_code = response.m_sPPVModuleCode;
            retVal.subscription_only = response.m_bSubscriptionOnly;

            if (response.m_oFullPrice != null)
                retVal.price = response.m_oPrice.ToApiObject();

            if (response.m_oFullPrice != null)
                retVal.full_price = response.m_oFullPrice.ToApiObject();

            retVal.price_reason = (TVPApiModule.Objects.Responses.PriceReason)response.m_PriceReason;

            if (response.m_relevantSub != null)
                retVal.relevant_sub = response.m_relevantSub.ToApiObject();

            if (response.m_relevantCol != null)
            {
                retVal.relevant_collection = response.m_relevantCol.ToApiObject();
            }

            if (response.m_relevantPP != null)
                retVal.relevant_pp = response.m_relevantPP.ToApiObject();

            if (response.m_oPPVDescription != null)            
                retVal.ppv_description = response.m_oPPVDescription.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            retVal.coupon_status = (TVPApiModule.Objects.Responses.CouponsStatus)response.m_couponStatus;

            retVal.first_device_name_found = response.m_sFirstDeviceNameFound;
            retVal.is_cancel_window = response.m_bCancelWindow;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.Collection ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.Collection response)
        {
            TVPApiModule.Objects.Responses.Collection collection = new Collection();

            collection.start_date = response.m_dStartDate;
            collection.end_date = response.m_dEndDate;
            collection.collection_price_code = response.m_oCollectionPriceCode != null ? response.m_oCollectionPriceCode.ToApiObject() : null;
            collection.price_code = response.m_oPriceCode != null ? response.m_oPriceCode.ToApiObject() : null;
            collection.discount_module = response.m_oDiscountModule != null ? response.m_oDiscountModule.ToApiObject() : null;
            collection.usage_module = response.m_oUsageModule != null ? response.m_oUsageModule.ToApiObject() : null;
            collection.fictivic_media_iD = response.m_fictivicMediaID;
            collection.product_code = response.m_ProductCode;
            collection.collection_code = response.m_CollectionCode;

            if (response.m_sCodes != null)
            {
                collection.codes = response.m_sCodes.Where(x => x != null).Select(x => x.ToApiObject() as TVPApiModule.Objects.Responses.BundleCodeContainer).ToArray();
            }

            if (response.m_sName != null)
            {
                collection.name = response.m_sName.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();
            }

            collection.file_types = response.m_sFileTypes != null ? response.m_sFileTypes.ToArray() : null;

            return collection;
        }

        public static TVPApiModule.Objects.Responses.Subscription ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.Subscription response)
        {
            TVPApiModule.Objects.Responses.Subscription retVal = new TVPApiModule.Objects.Responses.Subscription();

            if (response.m_sCodes != null)                        
                retVal.codes = response.m_sCodes.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            retVal.start_date = response.m_dStartDate;
            retVal.end_date = response.m_dEndDate;
            retVal.file_types = response.m_sFileTypes;
            retVal.is_recurring = response.m_bIsRecurring;
            retVal.number_of_rec_periods = response.m_nNumberOfRecPeriods;

            if (response.m_oSubscriptionPriceCode != null)                        
                retVal.subscription_price_code = response.m_oSubscriptionPriceCode.ToApiObject();

            if (response.m_oExtDisountModule != null)                        
                retVal.ext_disount_module = response.m_oExtDisountModule.ToApiObject();

            if (response.m_sName != null)                        
                retVal.name = response.m_sName.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            if (response.m_oSubscriptionUsageModule != null)                        
                retVal.subscription_usage_module = response.m_oSubscriptionUsageModule.ToApiObject();

            retVal.fictivic_media_id = response.m_fictivicMediaID;
            retVal.priority = response.m_Priority;
            retVal.subscription_product_code = response.m_ProductCode;
            retVal.subscription_code = response.m_SubscriptionCode;

            if (response.m_MultiSubscriptionUsageModule != null)                        
                retVal.multi_subscription_usage_module = response.m_MultiSubscriptionUsageModule.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            retVal.geo_commerce_id = response.n_GeoCommerceID;
            retVal.is_infinite_recurring = response.m_bIsInfiniteRecurring;

            if (response.m_oPreviewModule != null)                        
                retVal.preview_module = response.m_oPreviewModule.ToApiObject();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.PrePaidModule ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PrePaidModule response)
        {
            TVPApiModule.Objects.Responses.PrePaidModule retVal = new TVPApiModule.Objects.Responses.PrePaidModule();

            if (response.m_PriceCode != null)                        
                retVal.price_code = response.m_PriceCode.ToApiObject();

            if (response.m_CreditValue != null)                        
                retVal.credit_value = response.m_CreditValue.ToApiObject();
                    
            if (response.m_UsageModule != null)                        
                retVal.usage_module = response.m_UsageModule.ToApiObject();

            if (response.m_DiscountModule != null)                        
                retVal.discount_module = response.m_DiscountModule.ToApiObject();

            if (response.m_CouponsGroup != null)                        
                retVal.coupons_group = response.m_CouponsGroup.ToApiObject();

            if (response.m_Description != null)                        
                retVal.description = response.m_Description.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            retVal.object_code = response.m_ObjectCode;
            retVal.title = response.m_Title;
            retVal.is_fixed_credit = response.m_isFixedCredit;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.LanguageContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.LanguageContainer response)
        {
            TVPApiModule.Objects.Responses.LanguageContainer retVal = new TVPApiModule.Objects.Responses.LanguageContainer();

            retVal.language_code_3 = response.m_sLanguageCode3;
            retVal.value = response.m_sValue;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.SubscriptionCodeContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.BundleCodeContainer response)
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
                retVal.price = response.m_oPrise.ToApiObject();

            retVal.object_id = response.m_nObjectID;

            if (response.m_sDescription != null)
                retVal.description = response.m_sDescription.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();


            return retVal;
        }

        public static TVPApiModule.Objects.Responses.DiscountModule ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.DiscountModule response)
        {
            TVPApiModule.Objects.Responses.DiscountModule retVal = new TVPApiModule.Objects.Responses.DiscountModule();

            retVal.percent = response.m_dPercent;
            retVal.relation_type = (TVPApiModule.Objects.Responses.RelationTypes)response.m_eTheRelationType;
            retVal.start_date = response.m_dStartDate;
            retVal.end_date = response.m_dEndDate;

            if (response.m_oWhenAlgo != null)
                retVal.when_algo = response.m_oWhenAlgo.ToApiObject();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.UsageModule ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.UsageModule response)
        {
            TVPApiModule.Objects.Responses.UsageModule retVal = new TVPApiModule.Objects.Responses.UsageModule();

            retVal.object_id = response.m_nObjectID;
            retVal.virtual_name = response.m_sVirtualName;
            retVal.max_number_of_views = response.m_nMaxNumberOfViews;
            retVal.view_life_cycle = response.m_tsViewLifeCycle;
            retVal.max_usage_module_life_cycle = response.m_tsMaxUsageModuleLifeCycle;
            retVal.ext_discount_id = response.m_ext_discount_id;
            retVal.internal_discount_id = response.m_internal_discount_id;
            retVal.pricing_id = response.m_pricing_id;
            retVal.coupon_id = response.m_coupon_id;
            retVal.subscription_only = response.m_subscription_only;
            retVal.is_renew = response.m_is_renew;
            retVal.num_of_rec_periods = response.m_num_of_rec_periods;
            retVal.device_limit_id = response.m_device_limit_id;
            retVal.type = response.m_type;
            retVal.is_waiver = response.m_bWaiver;
            retVal.waived_period = response.m_nWaiverPeriod;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.PreviewModule ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PreviewModule response)
        {
            TVPApiModule.Objects.Responses.PreviewModule retVal = new TVPApiModule.Objects.Responses.PreviewModule();

            retVal.id = response.m_nID;
            retVal.name = response.m_sName;
            retVal.full_life_cycle = response.m_tsFullLifeCycle;
            retVal.non_renew_period = response.m_tsNonRenewPeriod;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.CouponsGroup ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.CouponsGroup response)
        {
            TVPApiModule.Objects.Responses.CouponsGroup retVal = new TVPApiModule.Objects.Responses.CouponsGroup();

            if (response.m_oDiscountCode != null)
                retVal.discount_module = response.m_oDiscountCode.ToApiObject();

            retVal.discount_code = response.m_sDiscountCode;

            if (response.m_sDescription != null)
                retVal.description = response.m_sDescription.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            retVal.start_date = response.m_dStartDate;
            retVal.end_date = response.m_dEndDate;
            retVal.max_use_count_for_coupon = response.m_nMaxUseCountForCoupon;
            retVal.group_code = response.m_sGroupCode;
            retVal.group_name = response.m_sGroupName;
            retVal.financial_entity_id = response.m_nFinancialEntityID;
            retVal.max_recurring_uses_count_for_coupon = response.m_nMaxRecurringUsesCountForCoupon;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.WhenAlgo ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.WhenAlgo response)
        {
            TVPApiModule.Objects.Responses.WhenAlgo retVal = new TVPApiModule.Objects.Responses.WhenAlgo();

            retVal.algo_type = (TVPApiModule.Objects.Responses.WhenAlgoType)response.m_eAlgoType;
            retVal.n_times = response.m_nNTimes;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.SubscriptionsPricesContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.SubscriptionsPricesContainer response)
        {
            TVPApiModule.Objects.Responses.SubscriptionsPricesContainer retVal = new TVPApiModule.Objects.Responses.SubscriptionsPricesContainer();

            retVal.subscription_code = response.m_sSubscriptionCode;

            if (response.m_oPrice != null)
                retVal.price = response.m_oPrice.ToApiObject();

            retVal.price_reason = (TVPApiModule.Objects.Responses.PriceReason)response.m_PriceReason;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.UserBillingTransactionsResponse ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.UserBillingTransactionsResponse response)
        {
            TVPApiModule.Objects.Responses.UserBillingTransactionsResponse retVal = new TVPApiModule.Objects.Responses.UserBillingTransactionsResponse();

            retVal.site_guid = response.m_sSiteGUID;

            if (response.m_BillingTransactionResponse != null)
                retVal.billing_transaction_response = response.m_BillingTransactionResponse.ToApiObject();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.DomainBillingTransactionsResponse ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.DomainBillingTransactionsResponse response)
        {
            TVPApiModule.Objects.Responses.DomainBillingTransactionsResponse retVal = new TVPApiModule.Objects.Responses.DomainBillingTransactionsResponse();

            retVal.domain_id = response.m_nDomainID;

            if (response.m_BillingTransactionResponses != null)
                retVal.billing_transaction_responses = response.m_BillingTransactionResponses.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.DomainResponseObject ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Domains.DomainResponseObject response)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject retVal = new TVPApiModule.Objects.Responses.DomainResponseObject();

            if (response.m_oDomain != null)
                retVal.domain = response.m_oDomain.ToApiObject();

            retVal.domain_response_status = (TVPApiModule.Objects.Responses.DomainResponseStatus)response.m_oDomainResponseStatus;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.DomainResponseStatus ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Domains.DomainStatus domainStatus)
        {
            DomainResponseStatus status = DomainResponseStatus.Error;

            switch (domainStatus)
            {
                case TVPPro.SiteManager.TvinciPlatform.Domains.DomainStatus.OK:
                    status = DomainResponseStatus.OK;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.Domains.DomainStatus.DomainAlreadyExists:
                    status = DomainResponseStatus.DomainAlreadyExists;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.Domains.DomainStatus.ExceededLimit:
                    status = DomainResponseStatus.ExceededLimit;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.Domains.DomainStatus.DeviceTypeNotAllowed:
                    status = DomainResponseStatus.DeviceTypeNotAllowed;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.Domains.DomainStatus.UnKnown:
                    status = DomainResponseStatus.UnKnown;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.Domains.DomainStatus.Error:
                    status = DomainResponseStatus.Error;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.Domains.DomainStatus.DeviceNotInDomin:
                    status = DomainResponseStatus.DeviceNotInDomain;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.Domains.DomainStatus.MasterEmailAlreadyExists:
                    break;
                case TVPPro.SiteManager.TvinciPlatform.Domains.DomainStatus.UserNotInDomain:
                    status = DomainResponseStatus.UserNotExistsInDomain;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.Domains.DomainStatus.DomainNotExists:
                    status = DomainResponseStatus.DomainNotExists;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.Domains.DomainStatus.HouseholdUserFailed:
                    status = DomainResponseStatus.HouseholdUserFailed;
                    break;
                case TVPPro.SiteManager.TvinciPlatform.Domains.DomainStatus.DomainCreatedWithoutNPVRAccount:
                    status = DomainResponseStatus.DomainCreatedWithoutNPVRAccount;
                    break;
                default:
                    break;
            }           

            return status;

        }

        public static TVPApiModule.Objects.Responses.DomainResponseStatus toApiObject(this TVPApiModule.Objects.Responses.DomainStatus domainStatus)
        {
            DomainResponseStatus status = DomainResponseStatus.Error;
            switch (domainStatus)
            {
                case DomainStatus.OK:
                    status = DomainResponseStatus.OK;
                    break;
                case DomainStatus.DomainAlreadyExists:
                    status = DomainResponseStatus.DomainAlreadyExists;
                    break;
                case DomainStatus.ExceededLimit:
                    status = DomainResponseStatus.ExceededLimit;
                    break;
                case DomainStatus.DeviceTypeNotAllowed:
                    status = DomainResponseStatus.DeviceTypeNotAllowed;
                    break;
                case DomainStatus.UnKnown:
                    status = DomainResponseStatus.UnKnown;
                    break;
                case DomainStatus.Error:
                    status = DomainResponseStatus.Error;
                    break;
                case DomainStatus.DeviceNotInDomin:
                    status = DomainResponseStatus.DeviceNotInDomain;
                    break;                                    
                case DomainStatus.UserNotInDomain:
                    status = DomainResponseStatus.UserNotExistsInDomain;
                    break;
                case DomainStatus.DomainNotExists:
                    status = DomainResponseStatus.DomainNotExists;
                    break;
                case DomainStatus.HouseholdUserFailed:
                    status = DomainResponseStatus.HouseholdUserFailed;
                    break;
                default:
                    status = DomainResponseStatus.UnKnown;
                    break;
            }

            return status;

        }
    

        public static TVPApiModule.Objects.Responses.Domain ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Domains.Domain response)
        {
            TVPApiModule.Objects.Responses.Domain retVal = new TVPApiModule.Objects.Responses.Domain();

            retVal.name = response.m_sName;
            retVal.description = response.m_sDescription;
            retVal.co_guid = response.m_sCoGuid;
            retVal.domain_id = response.m_nDomainID;
            retVal.group_id = response.m_nGroupID;
            retVal.limit = response.m_nLimit;
            retVal.device_limit = response.m_nDeviceLimit;
            retVal.user_limit = response.m_nUserLimit;
            retVal.concurrent_limit = response.m_nConcurrentLimit;
            retVal.status = response.m_nStatus;
            retVal.is_active = response.m_nIsActive;
            retVal.users_ids = response.m_UsersIDs;

            if (response.m_deviceFamilies != null)
            {
                retVal.device_families = response.m_deviceFamilies.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();
            }

            if (response.m_homeNetworks != null)
            {
                retVal.home_networks = response.m_homeNetworks.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();
            }

            if (response.m_masterGUIDs != null)
            {
                retVal.master_guids = response.m_masterGUIDs.ToArray();
            }

            if (response.m_PendingUsersIDs != null)
            {
                retVal.pending_users_ids = response.m_PendingUsersIDs.ToArray();
            }

            if (response.m_DefaultUsersIDs != null)
            {
                retVal.default_users_ids = response.m_DefaultUsersIDs.ToArray();
            }

            retVal.master_guids = response.m_masterGUIDs;
            retVal.domain_status = (TVPApiModule.Objects.Responses.DomainStatus)response.m_DomainStatus;
            retVal.frequency_flag = response.m_frequencyFlag;
            retVal.next_action_freq = response.m_NextActionFreq;
            retVal.domain_restriction = (TVPApiModule.Objects.Responses.DomainRestriction)response.m_DomainRestriction;
            retVal.next_user_action_freq = response.m_NextUserActionFreq;
            retVal.sso_operator_id = response.m_nSSOOperatorID;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.DeviceContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Domains.DeviceContainer response)
        {
            TVPApiModule.Objects.Responses.DeviceContainer retVal = new TVPApiModule.Objects.Responses.DeviceContainer();

            retVal.name = response.m_deviceFamilyName;
            retVal.device_family_id = response.m_deviceFamilyID;
            retVal.device_limit = response.m_deviceLimit;
            retVal.device_concurrent_limit = response.m_deviceConcurrentLimit;

            if (response.DeviceInstances != null)
                retVal.instances = response.DeviceInstances.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.Device ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Domains.Device response)
        {
            TVPApiModule.Objects.Responses.Device retVal = new TVPApiModule.Objects.Responses.Device();

            retVal.id = response.m_id;
            retVal.udid = response.m_deviceUDID;
            retVal.brand = response.m_deviceBrand;
            retVal.family = response.m_deviceFamily;
            retVal.device_family_id = response.m_deviceFamilyID;
            retVal.domain_id = response.m_domainID;
            retVal.device_name = response.m_deviceName;
            retVal.device_brand_id = response.m_deviceBrandID;
            retVal.pin = response.m_pin;
            retVal.activation_date = response.m_activationDate;
            retVal.state = (TVPApiModule.Objects.Responses.DeviceState)response.m_state;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.DeviceResponseObject ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Domains.DeviceResponseObject response)
        {
            TVPApiModule.Objects.Responses.DeviceResponseObject retVal = new TVPApiModule.Objects.Responses.DeviceResponseObject();

            if (response.m_oDevice != null)
                retVal.device = response.m_oDevice.ToApiObject();

            retVal.device_response_status = (TVPApiModule.Objects.Responses.DeviceResponseStatus)response.m_oDeviceResponseStatus;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.PPVModule ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.PPVModule response)
        {
            TVPApiModule.Objects.Responses.PPVModule retVal = new TVPApiModule.Objects.Responses.PPVModule();

            if (response.m_oPriceCode != null)
                retVal.price_code = response.m_oPriceCode.ToApiObject();

            if (response.m_oUsageModule != null)
                retVal.usage_module = response.m_oUsageModule.ToApiObject();

            if (response.m_oDiscountModule != null)
                retVal.discount_module = response.m_oDiscountModule.ToApiObject();

            if (response.m_oCouponsGroup != null)
                retVal.coupons_group = response.m_oCouponsGroup.ToApiObject();

            if (response.m_sDescription != null)
                retVal.description = response.m_sDescription.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            retVal.object_code = response.m_sObjectCode;
            retVal.object_virtual_name = response.m_sObjectVirtualName;
            retVal.subscription_only = response.m_bSubscriptionOnly;
            retVal.related_file_types = response.m_relatedFileTypes;
            retVal.product_code = response.m_Product_Code;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.PriceCode ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.PriceCode response)
        {
            TVPApiModule.Objects.Responses.PriceCode retVal = new TVPApiModule.Objects.Responses.PriceCode();

            retVal.code = response.m_sCode;

            if (response.m_oPrise != null)
                retVal.price = response.m_oPrise.ToApiObject();

            retVal.object_id = response.m_nObjectID;

            if (response.m_sDescription != null)
                retVal.description = response.m_sDescription.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.UsageModule ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.UsageModule response)
        {
            TVPApiModule.Objects.Responses.UsageModule retVal = new TVPApiModule.Objects.Responses.UsageModule();

            retVal.object_id = response.m_nObjectID;
            retVal.virtual_name = response.m_sVirtualName;
            retVal.max_number_of_views = response.m_nMaxNumberOfViews;
            retVal.view_life_cycle = response.m_tsViewLifeCycle;
            retVal.max_usage_module_life_cycle = response.m_tsMaxUsageModuleLifeCycle;
            retVal.ext_discount_id = response.m_ext_discount_id;
            retVal.internal_discount_id = response.m_internal_discount_id;
            retVal.pricing_id = response.m_pricing_id;
            retVal.coupon_id = response.m_coupon_id;
            retVal.subscription_only = response.m_subscription_only;
            retVal.is_renew = response.m_is_renew;
            retVal.num_of_rec_periods = response.m_num_of_rec_periods;
            retVal.device_limit_id = response.m_device_limit_id;
            retVal.type = response.m_type;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.DiscountModule ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.DiscountModule response)
        {
            TVPApiModule.Objects.Responses.DiscountModule retVal = new TVPApiModule.Objects.Responses.DiscountModule();

            retVal.percent = response.m_dPercent;
            retVal.relation_type = (TVPApiModule.Objects.Responses.RelationTypes)response.m_eTheRelationType;
            retVal.start_date = response.m_dStartDate;
            retVal.end_date = response.m_dEndDate;

            if (response.m_oWhenAlgo != null)
                retVal.when_algo = response.m_oWhenAlgo.ToApiObject();

            retVal.code = response.m_sCode;

            if (response.m_oPrise != null)
                retVal.price = response.m_oPrise.ToApiObject();

            retVal.object_id = response.m_nObjectID;
            if (response.m_sDescription != null)
                retVal.description = response.m_sDescription.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();
            

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.CouponsGroup ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.CouponsGroup response)
        {
            TVPApiModule.Objects.Responses.CouponsGroup retVal = new TVPApiModule.Objects.Responses.CouponsGroup();

            if (response.m_oDiscountCode != null)
                retVal.discount_module = response.m_oDiscountCode.ToApiObject();

            retVal.discount_code = response.m_sDiscountCode;

            if (response.m_sDescription != null)
                retVal.description = response.m_sDescription.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            retVal.start_date = response.m_dStartDate;
            retVal.end_date = response.m_dEndDate;
            retVal.max_use_count_for_coupon = response.m_nMaxUseCountForCoupon;
            retVal.group_code = response.m_sGroupCode;
            retVal.group_name = response.m_sGroupName;
            retVal.financial_entity_id = response.m_nFinancialEntityID;
            retVal.max_recurring_uses_count_for_coupon = response.m_nMaxRecurringUsesCountForCoupon;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.LanguageContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.LanguageContainer response)
        {
            TVPApiModule.Objects.Responses.LanguageContainer retVal = new TVPApiModule.Objects.Responses.LanguageContainer();

            retVal.language_code_3 = response.m_sLanguageCode3;
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

            retVal.algo_type = (TVPApiModule.Objects.Responses.WhenAlgoType)response.m_eAlgoType;
            retVal.n_times = response.m_nNTimes;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.Currency ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.Currency response)
        {
            TVPApiModule.Objects.Responses.Currency retVal = new TVPApiModule.Objects.Responses.Currency();

            retVal.currency_id = response.m_nCurrencyID;
            retVal.currency_cd2 = response.m_sCurrencyCD2;
            retVal.currency_cd3 = response.m_sCurrencyCD3;
            retVal.currency_sign = response.m_sCurrencySign;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.CouponData ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.CouponData response)
        {
            TVPApiModule.Objects.Responses.CouponData retVal = new TVPApiModule.Objects.Responses.CouponData();

            retVal.coupon_status = (TVPApiModule.Objects.Responses.CouponsStatus)response.m_CouponStatus;

            if (response.m_oCouponGroup != null)
                retVal.coupon_group = response.m_oCouponGroup.ToApiObject();

            retVal.coupon_type = (TVPApiModule.Objects.Responses.CouponType)response.m_CouponType;
            retVal.camp_id = response.m_campID;
            retVal.owner_guid = response.m_ownerGUID;
            retVal.owner_media = response.m_ownerMedia;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.Subscription ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.Subscription response)
        {
            TVPApiModule.Objects.Responses.Subscription retVal = new TVPApiModule.Objects.Responses.Subscription();

            if (response.m_sCodes != null)
                retVal.codes = response.m_sCodes.Where(x => x != null).Select(x => x.ToApiObject(eBundleType.SUBSCRIPTION) as TVPApiModule.Objects.Responses.SubscriptionCodeContainer).ToArray();

            retVal.start_date = response.m_dStartDate;
            retVal.end_date = response.m_dEndDate;
            retVal.file_types = response.m_sFileTypes;
            retVal.is_recurring = response.m_bIsRecurring;
            retVal.number_of_rec_periods = response.m_nNumberOfRecPeriods;

            if (response.m_oSubscriptionPriceCode != null)
                retVal.subscription_price_code = response.m_oSubscriptionPriceCode.ToApiObject();

            if (response.m_oExtDisountModule != null)            
                retVal.ext_disount_module = response.m_oExtDisountModule.ToApiObject();

            if (response.m_sName != null)
                retVal.name = response.m_sName.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            if (response.m_oSubscriptionUsageModule != null)
                retVal.subscription_usage_module = response.m_oSubscriptionUsageModule.ToApiObject();

            retVal.fictivic_media_id = response.m_fictivicMediaID;
            retVal.priority = response.m_Priority;
            retVal.subscription_product_code = response.m_ProductCode;
            retVal.subscription_code = response.m_SubscriptionCode;

            if (response.m_MultiSubscriptionUsageModule != null)
                retVal.multi_subscription_usage_module = response.m_MultiSubscriptionUsageModule.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();
            retVal.geo_commerce_id = response.n_GeoCommerceID;
            retVal.is_infinite_recurring = response.m_bIsInfiniteRecurring;

            if (response.m_oPreviewModule != null)
                retVal.preview_module = response.m_oPreviewModule.ToApiObject();

            if (response.m_UserTypes != null)
                retVal.user_types = response.m_UserTypes.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            if (response.m_sDescription != null)
                retVal.description = response.m_sDescription.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.UserType ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.UserType response)
        {
            TVPApiModule.Objects.Responses.UserType user = new TVPApiModule.Objects.Responses.UserType();

            user.description = response.Description;
            user.id = response.ID;
            user.is_default = response.IsDefault;

            return user;
        }


        //public static TVPApiModule.Objects.Responses.SubscriptionCodeContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.BundleCodeContainer response)
        //{
        //    TVPApiModule.Objects.Responses.SubscriptionCodeContainer retVal = new TVPApiModule.Objects.Responses.SubscriptionCodeContainer();

        //    retVal.code = response.m_sCode;
        //    retVal.name = response.m_sName;

        //    return retVal;
        //}

        public static object ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.BundleCodeContainer response, eBundleType type)
        {
            object container = null;

            switch (type)
            {
                case eBundleType.SUBSCRIPTION:
                    container = new TVPApiModule.Objects.Responses.SubscriptionCodeContainer() { code = response.m_sCode, name = response.m_sName };
                    break;
                case eBundleType.COLLECTION:
                    container = new TVPApiModule.Objects.Responses.BundleCodeContainer() { code = response.m_sCode, name = response.m_sName };
                    break;
                default:
                    break;
            }

            return container;
        }

        public static TVPApiModule.Objects.Responses.PreviewModule ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.PreviewModule response)
        {
            TVPApiModule.Objects.Responses.PreviewModule retVal = new TVPApiModule.Objects.Responses.PreviewModule();

            retVal.id = response.m_nID;
            retVal.name = response.m_sName;
            retVal.full_life_cycle = response.m_tsFullLifeCycle;
            retVal.non_renew_period = response.m_tsNonRenewPeriod;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.GroupOperator ToApiObject(this TVPPro.SiteManager.TvinciPlatform.api.GroupOperator response)
        {
            TVPApiModule.Objects.Responses.GroupOperator retVal = new TVPApiModule.Objects.Responses.GroupOperator();

            if (response.UIData != null)
                retVal.ui_data = response.UIData.ToApiObject();

            retVal.id = response.ID;
            retVal.name = response.Name;
            retVal.type = (TVPApiModule.Objects.Responses.eOperatorType)response.Type;
            retVal.login_url = response.LoginUrl;
            retVal.sub_group_id = response.SubGroupID;

            if (response.Scopes != null)
                retVal.scopes = response.Scopes.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            retVal.group_user_name = response.GroupUserName;
            retVal.group_password = response.GroupPassword;
            retVal.logout_url = response.LogoutURL;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.Scope ToApiObject(this TVPPro.SiteManager.TvinciPlatform.api.Scope response)
        {
            TVPApiModule.Objects.Responses.Scope retVal = new TVPApiModule.Objects.Responses.Scope();

            retVal.login_url = response.LoginUrl;
            retVal.logout_url = response.LogoutUrl;
            retVal.name = response.Name;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.UIData ToApiObject(this TVPPro.SiteManager.TvinciPlatform.api.UIData response)
        {
            TVPApiModule.Objects.Responses.UIData retVal = new TVPApiModule.Objects.Responses.UIData();

            retVal.color_code = response.ColorCode;
            //retVal.pic_id = response.picID; // TODO: Check what are the changes regarding this parameter

            return retVal;
        }

        /*public static TVPApiModule.Objects.Responses.UserSocialActionObject ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Social.UserSocialActionObject response)
        {
            TVPApiModule.Objects.Responses.UserSocialActionObject retVal = new TVPApiModule.Objects.Responses.UserSocialActionObject();

            retVal.site_guid = response.m_sSiteGuid;
            retVal.social_action = (TVPApiModule.Objects.eUserAction)response.m_eSocialAction;
            retVal.social_platform = (TVPApiModule.Objects.Responses.SocialPlatform)response.m_eSocialPlatform;
            retVal.media_id = response.nMediaID;
            retVal.program_id = response.nProgramID;
            retVal.asset_type = (TVPApiModule.Objects.Responses.eAssetType)response.assetType;
            retVal.action_date = response.m_dActionDate;


            return retVal;
        }*/

        public static FBSignIn ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Social.FBSignin response)
        {
            FBSignIn retVal = new FBSignIn();

            retVal.status = response.status;
            if (response.user != null)
            {
                retVal.user = response.user.ToApiObject();
            }

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.UserResponseObject ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Social.UserResponseObject response)
        {
            UserResponseObject retVal = new UserResponseObject();

            retVal.resp_status = (eResponseStatus)response.m_RespStatus;
            retVal.user_instance_id = response.m_userInstanceID;
            if (response.m_user != null)
            {
                retVal.user = response.m_user.ToApiObject();
            }

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.User ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Social.User response)
        {
            User retVal = new User();

            retVal.domain_id = response.m_domianID;
            retVal.user_state = (TVPApiModule.Objects.Responses.UserState)response.m_eUserState;
            retVal.is_domain_master = response.m_isDomainMaster;
            retVal.sso_operator_id = response.m_nSSOOperatorID;

            if (response.m_oBasicData != null)
            {
                retVal.basic_data = new TVPApiModule.Objects.Responses.UserBasicData();

                retVal.basic_data.is_facebook_image_permitted = response.m_oBasicData.m_bIsFacebookImagePermitted;
                retVal.basic_data.co_guid = response.m_oBasicData.m_CoGuid;

                if (retVal.basic_data.country != null)
                {
                    retVal.basic_data.country = response.m_oBasicData.m_Country.ToApiObject();
                }

                retVal.basic_data.external_token = response.m_oBasicData.m_ExternalToken;
                retVal.basic_data.address = response.m_oBasicData.m_sAddress;
                retVal.basic_data.affiliate_code = response.m_oBasicData.m_sAffiliateCode;
                retVal.basic_data.city = response.m_oBasicData.m_sCity;
                retVal.basic_data.email = response.m_oBasicData.m_sEmail;
                retVal.basic_data.facebook_id = response.m_oBasicData.m_sFacebookID;
                retVal.basic_data.facebook_image = response.m_oBasicData.m_sFacebookImage;
                retVal.basic_data.facebook_token = response.m_oBasicData.m_sFacebookToken;
                retVal.basic_data.first_name = response.m_oBasicData.m_sFirstName;
                retVal.basic_data.last_name = response.m_oBasicData.m_sLastName;
                retVal.basic_data.phone = response.m_oBasicData.m_sPhone;

                if (retVal.basic_data.state != null)
                    retVal.basic_data.state = response.m_oBasicData.m_State.ToApiObject();

                retVal.basic_data.user_name = response.m_oBasicData.m_sUserName;
                retVal.basic_data.zip = response.m_oBasicData.m_sZip;

                retVal.basic_data.user_type = response.m_oBasicData.m_UserType.ToApiObject();
            }

            if (response.m_oDynamicData != null)
            {
                retVal.dynamic_data = new TVPApiModule.Objects.Responses.UserDynamicData();

                if (response.m_oDynamicData.m_sUserData != null)
                {
                    retVal.dynamic_data.user_data = response.m_oDynamicData.m_sUserData.Select(x => new TVPApiModule.Objects.Responses.UserDynamicDataContainer()
                    {
                        data_type = x.m_sDataType,
                        value = x.m_sValue
                    }).ToArray();
                }
            }

            retVal.site_guid = response.m_sSiteGUID;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.UserType ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Social.UserType response)
        {
            TVPApiModule.Objects.Responses.UserType userType = new UserType();

            userType.description = response.Description;
            userType.id = response.ID;
            userType.is_default = response.IsDefault;

            return userType;
        }

        public static TVPApiModule.Objects.Responses.State ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Social.State response)
        {
            TVPApiModule.Objects.Responses.State state = new State();

            state.state_code = response.m_sStateCode;
            state.state_name = response.m_sStateName;
            state.object_id = response.m_nObjecrtID;

            if (response.m_Country != null)
            {
                state.country = response.m_Country.ToApiObject();
            }

            return state;
        }

        public static TVPApiModule.Objects.Responses.Country ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Social.Country response)
        {
            TVPApiModule.Objects.Responses.Country country = new Country();

            country.country_code = response.m_sCountryCode;
            country.country_name = response.m_sCountryName;
            country.object_id = response.m_nObjecrtID;            

            return country;
        }

        public static TVPApiModule.Objects.Responses.UserBasicData ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData response)
        {
            TVPApiModule.Objects.Responses.UserBasicData retVal = new TVPApiModule.Objects.Responses.UserBasicData();

            retVal.is_facebook_image_permitted = response.m_bIsFacebookImagePermitted;
            retVal.co_guid = response.m_CoGuid;

            if (response.m_Country != null)
                retVal.country = response.m_Country.ToApiObject();

            retVal.external_token = response.m_ExternalToken;
            retVal.address = response.m_sAddress;
            retVal.affiliate_code = response.m_sAffiliateCode;
            retVal.city = response.m_sCity;
            retVal.email = response.m_sEmail;
            retVal.facebook_id = response.m_sFacebookID;
            retVal.facebook_image = response.m_sFacebookImage;
            retVal.facebook_token = response.m_sFacebookToken;
            retVal.first_name = response.m_sFirstName;
            retVal.last_name = response.m_sLastName;
            retVal.phone = response.m_sPhone;

            if (response.m_State != null)            
                retVal.state = response.m_State.ToApiObject();

            retVal.user_name = response.m_sUserName;
            retVal.zip = response.m_sZip;

            if (response.m_UserType != null)                        
                retVal.user_type = response.m_UserType.ToApiObject();


            return retVal;
        }

        public static TVPApiModule.Objects.Responses.FriendWatchedObject ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Social.FriendWatchedObject response)
        {
            TVPApiModule.Objects.Responses.FriendWatchedObject retVal = new TVPApiModule.Objects.Responses.FriendWatchedObject();

            retVal.site_guid = response.SiteGuid;
            retVal.media_id = response.MediaID;
            retVal.update_date = response.UpdateDate;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.FacebookConfig ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Social.FacebookConfig response)
        {
            TVPApiModule.Objects.Responses.FacebookConfig retVal = new TVPApiModule.Objects.Responses.FacebookConfig();

            retVal.fb_key = response.sFBKey;
            retVal.fb_secret = response.sFBSecret;
            retVal.fb_callback = response.sFBCallback;
            retVal.fb_min_friends = response.nFBMinFriends;
            retVal.fb_permissions = response.sFBPermissions;
            retVal.fb_redirect = response.sFBRedirect;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.FBInterestData ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Social.FBInterestData response)
        {
            TVPApiModule.Objects.Responses.FBInterestData retVal = new TVPApiModule.Objects.Responses.FBInterestData();

            retVal.name = response.name;
            retVal.category = response.category;
            retVal.id = response.id;
            retVal.created_time = response.created_time;

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

            retVal.site_guid = response.m_sSiteGuid;
            retVal.birthday = response.Birthday;

            if (response.Location != null)
                retVal.location = response.Location.ToApiObject();

            if (response.interests != null)
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
            TVPApiModule.Objects.Responses.FacebookResponseObject retVal = new TVPApiModule.Objects.Responses.FacebookResponseObject();

            retVal.status = response.status;
            retVal.site_guid = response.siteGuid;
            retVal.tvinci_name = response.tvinciName;
            retVal.facebook_name = response.facebookName;
            retVal.pic = response.pic;
            retVal.data = response.data;
            retVal.min_friends = response.minFriends;
            if (response.fbUser != null)
                retVal.fb_user = response.fbUser.ToApiObject();

            retVal.token = response.token;

            return retVal;
        }

        public static KeyValuePair ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair response)
        {
            KeyValuePair retVal = new KeyValuePair();

            retVal.key = response.key;
            retVal.value = response.value;

            return retVal;
        }

        public static KeyValuePair ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Users.KeyValuePair response)
        {
            KeyValuePair retVal = new KeyValuePair();

            retVal.key = response.key;
            retVal.value = response.value;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.NetworkResponseObject ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Domains.NetworkResponseObject response)
        {
            TVPApiModule.Objects.Responses.NetworkResponseObject networkResponse = null;

            if (response != null)
            {
                networkResponse = new Objects.Responses.NetworkResponseObject();
                networkResponse.IsSuccess = response.bSuccess;
                networkResponse.Reason = (TVPApiModule.Objects.Responses.NetworkResponseStatus)response.eReason;
            }

            return networkResponse;
        }

        public static TVPApiModule.Objects.Responses.HomeNetwork ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Domains.HomeNetwork response)
        {
            TVPApiModule.Objects.Responses.HomeNetwork homeNetworkResponse = null;

            if (response != null)
            {
                homeNetworkResponse = new Objects.Responses.HomeNetwork();
                homeNetworkResponse.create_date = response.CreateDate;
                homeNetworkResponse.description = response.Description;
                homeNetworkResponse.is_active = response.IsActive;
                homeNetworkResponse.name = response.Name;
                homeNetworkResponse.uid = response.UID;
            }

            return homeNetworkResponse;
        }

        public static TVPApiModule.Objects.Responses.Collection ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Pricing.Collection response)
        {
            TVPApiModule.Objects.Responses.Collection retVal = new TVPApiModule.Objects.Responses.Collection();

            if (response.m_sCodes != null)
                retVal.codes = response.m_sCodes.Where(x => x != null).Select(x => x.ToApiObject(eBundleType.COLLECTION) as TVPApiModule.Objects.Responses.BundleCodeContainer).ToArray();

            retVal.start_date = response.m_dStartDate;
            retVal.end_date = response.m_dEndDate;
            retVal.file_types = response.m_sFileTypes;            

            if (response.m_oCollectionPriceCode != null)
                retVal.price_code = response.m_oCollectionPriceCode.ToApiObject();

            if (response.m_oExtDisountModule != null)
                retVal.ext_discount_module = response.m_oExtDisountModule.ToApiObject();

            if (response.m_sName != null)
                retVal.name = response.m_sName.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            if (response.m_oCollectionUsageModule != null)
                retVal.collection_usage_module = response.m_oCollectionUsageModule.ToApiObject();

            if (response.m_oDiscountModule != null)
                retVal.discount_module = response.m_oDiscountModule.ToApiObject();

            if (response.m_oUsageModule != null)
                retVal.usage_module = response.m_oUsageModule.ToApiObject();

            retVal.fictivic_media_iD = response.m_fictivicMediaID;
            retVal.collection_product_code = response.m_ProductCode;
            retVal.collection_code = response.m_CollectionCode;
            retVal.object_virtual_name = response.m_sObjectVirtualName;
            retVal.subscription_only = response.m_bSubscriptionOnly;
            retVal.related_file_types = response.m_relatedFileTypes;
            retVal.first_device_limitation = response.m_bFirstDeviceLimitation;
            retVal.object_code = response.m_sObjectCode;
            retVal.product_code = response.m_Product_Code;
            
            //retVal.
            //if (response.m_oSubscriptionUsageModule != null)
            //    retVal.multi_subscription_usage_module = response.m_MultiSubscriptionUsageModule.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();
            //retVal. = response.g;
            //retVal.is_infinite_recurring = response.rec;

            //if (response.p != null)
            //    retVal.preview_module = response.m_oPreviewModule.ToApiObject();

            return retVal;
        }

        public static CollectionPricesContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.CollectionsPricesContainer response)
        {
            TVPApiModule.Objects.Responses.CollectionPricesContainer retVal = new TVPApiModule.Objects.Responses.CollectionPricesContainer();

            retVal.collection_code = response.m_sCollectionCode;

            if (response.m_oPrice != null)
                retVal.price = response.m_oPrice.ToApiObject();

            retVal.price_reason = (TVPApiModule.Objects.Responses.PriceReason)response.m_PriceReason;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.PermittedCollectionContainer ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PermittedCollectionContainer response)
        {
            TVPApiModule.Objects.Responses.PermittedCollectionContainer retVal = new TVPApiModule.Objects.Responses.PermittedCollectionContainer();

            retVal.current_date = response.m_dCurrentDate;
            retVal.end_date = response.m_dEndDate;
            retVal.last_view_date = response.m_dLastViewDate;
            retVal.purchase_date = response.m_dPurchaseDate;
            retVal.collection_purchase_id = response.m_nCollectionPurchaseID;
            retVal.payment_method = (TVPApiModule.Objects.Responses.PaymentMethod)response.m_paymentMethod;
            retVal.device_name = response.m_sDeviceName;
            retVal.device_udid = response.m_sDeviceUDID;
            retVal.collection_code = response.m_sCollectionCode;
            retVal.is_cancel_window = response.m_bCancelWindow;

            return retVal;
        }

        public static TVPApiModule.Objects.Responses.DoSocialActionResponse ToApiObject(this TVPPro.SiteManager.TvinciPlatform.Social.DoSocialActionResponse response)
        {
            TVPApiModule.Objects.Responses.DoSocialActionResponse returnedResponse = new DoSocialActionResponse();

            returnedResponse.action_response_status_extern = (TVPApiModule.Objects.Responses.SocialActionResponseStatus)response.m_eActionResponseStatusExtern;
            returnedResponse.action_response_status_intern = (TVPApiModule.Objects.Responses.SocialActionResponseStatus)response.m_eActionResponseStatusIntern;
            
            return returnedResponse;
        }

        /*public static TVPApiModule.Objects.Responses.Status ToApiObject(this TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.StatusObject response)
        {
            TVPApiModule.Objects.Responses.Status retVal = new TVPApiModule.Objects.Responses.Status();

            retVal.message = response.Message;
            retVal.code = response.Code;

            return retVal;
        }*/
    }
}
