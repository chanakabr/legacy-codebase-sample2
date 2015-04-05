using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Extentions
{
    public static class TranslateExtensions
    {
        public static RestfulTVPApi.Users.Country ToTvmObject(this RestfulTVPApi.Objects.Responses.Country response)
        {
            RestfulTVPApi.Users.Country country = new RestfulTVPApi.Users.Country();

            country.m_nObjecrtID = response.object_id;
            country.m_sCountryCode = response.country_code;
            country.m_sCountryName = response.country_name;

            return country;
        }

        public static RestfulTVPApi.Users.State ToTvmObject(this RestfulTVPApi.Objects.Responses.State response)
        {
            RestfulTVPApi.Users.State state = new RestfulTVPApi.Users.State();

            if (state.m_Country != null)
                state.m_Country = response.country.ToTvmObject();

            state.m_nObjecrtID = response.object_id;
            state.m_sStateCode = response.state_code;
            state.m_sStateName = response.state_name;

            return state;
        }

        public static RestfulTVPApi.Users.UserType ToTvmObject(this RestfulTVPApi.Objects.Responses.UserType response)
        {
            RestfulTVPApi.Users.UserType userType = new RestfulTVPApi.Users.UserType();

            userType.Description = response.description;
            userType.ID = response.id;
            userType.IsDefault = response.is_default;

            return userType;
        }

        public static RestfulTVPApi.Users.UserBasicData ToTvmObject(this RestfulTVPApi.Objects.Responses.UserBasicData response)
        {
            RestfulTVPApi.Users.UserBasicData basic_data = new RestfulTVPApi.Users.UserBasicData();

            if (response != null)
            {
                basic_data.m_bIsFacebookImagePermitted = response.is_facebook_image_permitted;
                basic_data.m_CoGuid = response.co_guid;

                if (response.country != null)
                {
                    basic_data.m_Country = response.country.ToTvmObject();
                }

                basic_data.m_ExternalToken = response.external_token;
                basic_data.m_sAddress = response.address;
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

        public static RestfulTVPApi.Users.UserDynamicData ToTvmObject(this RestfulTVPApi.Objects.Responses.UserDynamicData response)
        {
            RestfulTVPApi.Users.UserDynamicData dynamic_data = new RestfulTVPApi.Users.UserDynamicData();

            if (response != null)
            {
                if (response.user_data != null)
                {
                    dynamic_data.m_sUserData = response.user_data.Select(x => new RestfulTVPApi.Users.UserDynamicDataContainer()
                    {
                        m_sDataType = x.data_type,
                        m_sValue = x.value
                    }).ToArray();
                }
            }

            return dynamic_data;
        }

        public static RestfulTVPApi.Objects.Responses.UserDynamicData ToApiObject(this RestfulTVPApi.Users.UserDynamicData response)
        {
            RestfulTVPApi.Objects.Responses.UserDynamicData retVal = new RestfulTVPApi.Objects.Responses.UserDynamicData();

            if (response != null)
            {
                if (response.m_sUserData != null)
                {
                    retVal.user_data = response.m_sUserData.Select(x => new RestfulTVPApi.Objects.Responses.UserDynamicDataContainer()
                    {
                        data_type = x.m_sDataType,
                        value = x.m_sValue
                    }).ToArray();
                }
            }

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.UserResponseObject ToApiObject(this RestfulTVPApi.Users.UserResponseObject response)
        {
            RestfulTVPApi.Objects.Responses.UserResponseObject retVal = new RestfulTVPApi.Objects.Responses.UserResponseObject();

            retVal.resp_status = (RestfulTVPApi.Objects.Responses.Enums.eResponseStatus)response.m_RespStatus;

            if (response.m_user != null)
            {
                retVal.user = new RestfulTVPApi.Objects.Responses.User();

                retVal.user.domain_id = response.m_user.m_domianID;
                retVal.user.user_state = (RestfulTVPApi.Objects.Responses.Enums.UserState)response.m_user.m_eUserState;
                retVal.user.is_domain_master = response.m_user.m_isDomainMaster;
                retVal.user.sso_operator_id = response.m_user.m_nSSOOperatorID;

                if (response.m_user.m_oBasicData != null)
                {
                    retVal.user.basic_data = new RestfulTVPApi.Objects.Responses.UserBasicData();

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

        public static RestfulTVPApi.Objects.Responses.PermittedSubscriptionContainer ToApiObject(this RestfulTVPApi.ConditionalAccess.PermittedSubscriptionContainer response)
        {
            RestfulTVPApi.Objects.Responses.PermittedSubscriptionContainer retVal = new RestfulTVPApi.Objects.Responses.PermittedSubscriptionContainer();

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
            retVal.payment_method = (RestfulTVPApi.Objects.Responses.Enums.PaymentMethod)response.m_paymentMethod;
            retVal.device_name = response.m_sDeviceName;
            retVal.device_udid = response.m_sDeviceUDID;
            retVal.subscription_code = response.m_sSubscriptionCode;
            retVal.is_cancel_window = response.m_bCancelWindow;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.PermittedMediaContainer ToApiObject(this RestfulTVPApi.ConditionalAccess.PermittedMediaContainer response)
        {
            RestfulTVPApi.Objects.Responses.PermittedMediaContainer retVal = new RestfulTVPApi.Objects.Responses.PermittedMediaContainer();

            retVal.current_date = response.m_dCurrentDate;
            retVal.end_date = response.m_dEndDate;
            retVal.purchase_date = response.m_dPurchaseDate;
            retVal.current_uses = response.m_nCurrentUses;
            retVal.max_uses = response.m_nMaxUses;
            retVal.media_file_id = response.m_nMediaFileID;
            retVal.media_id = response.m_nMediaID;
            retVal.purchase_method = (RestfulTVPApi.Objects.Responses.Enums.PaymentMethod)response.m_purchaseMethod;
            retVal.device_name = response.m_sDeviceName;
            retVal.device_udid = response.m_sDeviceUDID;
            retVal.is_cancel_window = response.m_bCancelWindow;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.FavoriteObject ToApiObject(this RestfulTVPApi.Users.FavoritObject response)
        {
            RestfulTVPApi.Objects.Responses.FavoriteObject retVal = new RestfulTVPApi.Objects.Responses.FavoriteObject();

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

        public static RestfulTVPApi.Objects.Responses.GroupRule ToApiObject(this RestfulTVPApi.Api.GroupRule response)
        {
            RestfulTVPApi.Objects.Responses.GroupRule retVal = new RestfulTVPApi.Objects.Responses.GroupRule();

            retVal.all_tag_values = response.AllTagValues;
            retVal.block_type = (RestfulTVPApi.Objects.Responses.Enums.eBlockType)response.BlockType;
            retVal.dynamic_data_key = response.DynamicDataKey;
            retVal.group_rule_type = (RestfulTVPApi.Objects.Responses.Enums.eGroupRuleType)response.GroupRuleType;
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

        public static RestfulTVPApi.Objects.Responses.MediaMarkObject ToApiObject(this RestfulTVPApi.Api.MediaMarkObject response)
        {
            RestfulTVPApi.Objects.Responses.MediaMarkObject retVal = new RestfulTVPApi.Objects.Responses.MediaMarkObject();

            retVal.status = (RestfulTVPApi.Objects.Responses.Enums.MediaMarkObjectStatus)response.eStatus;
            retVal.group_id = response.nGroupID;
            retVal.location_sec = response.nLocationSec;
            retVal.media_id = response.nMediaID;
            retVal.device_id = response.sDeviceID;
            retVal.device_name = response.sDeviceName;
            retVal.site_guid = response.sSiteGUID;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.UserItemList ToApiObject(this RestfulTVPApi.Users.UserItemList response)
        {
            RestfulTVPApi.Objects.Responses.UserItemList retVal = new RestfulTVPApi.Objects.Responses.UserItemList();

            if (response.itemObj != null)
            {
                retVal.item_obj = response.itemObj.Select(x => new RestfulTVPApi.Objects.Responses.ItemObj()
                {
                    item = x.item,
                    order_num = x.orderNum
                }).ToArray();
            }

            retVal.item_type = (RestfulTVPApi.Objects.Responses.Enums.ItemType)response.itemType;
            retVal.list_type = (RestfulTVPApi.Objects.Responses.Enums.ListType)response.listType;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.State ToApiObject(this RestfulTVPApi.Users.State response)
        {
            RestfulTVPApi.Objects.Responses.State retVal = new RestfulTVPApi.Objects.Responses.State();
            
            if (response.m_Country != null)
                retVal.country = response.m_Country.ToApiObject();

            retVal.object_id = response.m_nObjecrtID;
            retVal.state_code = response.m_sStateCode;
            retVal.state_name = response.m_sStateName;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.Country ToApiObject(this RestfulTVPApi.Users.Country response)
        {
            RestfulTVPApi.Objects.Responses.Country retVal = new RestfulTVPApi.Objects.Responses.Country();

            retVal.object_id = response.m_nObjecrtID;
            retVal.country_code = response.m_sCountryCode;
            retVal.country_name = response.m_sCountryName;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.UserType ToApiObject(this RestfulTVPApi.Users.UserType response)
        {
            RestfulTVPApi.Objects.Responses.UserType retVal = new RestfulTVPApi.Objects.Responses.UserType();

            retVal.description = response.Description;
            retVal.id = response.ID;
            retVal.is_default = response.IsDefault;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.EPGChannel ToApiObject(this RestfulTVPApi.Api.EPGChannelObject response)
        {
            RestfulTVPApi.Objects.Responses.EPGChannel retVal = new RestfulTVPApi.Objects.Responses.EPGChannel();

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

        public static RestfulTVPApi.Objects.Responses.EPGMultiChannelProgrammeObject ToApiObject(this TVPPro.SiteManager.Objects.EPGMultiChannelProgrammeObject response)
        {
            RestfulTVPApi.Objects.Responses.EPGMultiChannelProgrammeObject retVal = new RestfulTVPApi.Objects.Responses.EPGMultiChannelProgrammeObject();

            retVal.epg_channel_id = response.EPG_CHANNEL_ID;

            if (response.EPGChannelProgrammeObject != null)
                retVal.epg_channel_program_object = response.EPGChannelProgrammeObject.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.EPGChannelProgrammeObject ToApiObject(this RestfulTVPApi.Api.EPGChannelProgrammeObject response)
        {
            RestfulTVPApi.Objects.Responses.EPGChannelProgrammeObject retVal = new RestfulTVPApi.Objects.Responses.EPGChannelProgrammeObject();

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

        public static RestfulTVPApi.Objects.Responses.EPGChannelProgrammeObject ToApiObject(this Tvinci.Data.Loaders.TvinciPlatform.Catalog.EPGChannelProgrammeObject response)
        {
            RestfulTVPApi.Objects.Responses.EPGChannelProgrammeObject retVal = new RestfulTVPApi.Objects.Responses.EPGChannelProgrammeObject();

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

        public static RestfulTVPApi.Objects.Responses.EPGDictionary ToApiObject(this RestfulTVPApi.Api.EPGDictionary response)
        {
            RestfulTVPApi.Objects.Responses.EPGDictionary retVal = new RestfulTVPApi.Objects.Responses.EPGDictionary();

            retVal.key = response.Key;
            retVal.value = response.Value;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.EPGDictionary ToApiObject(this Tvinci.Data.Loaders.TvinciPlatform.Catalog.EPGDictionary response)
        {
            RestfulTVPApi.Objects.Responses.EPGDictionary retVal = new RestfulTVPApi.Objects.Responses.EPGDictionary();

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

        public static RestfulTVPApi.Objects.Responses.BillingResponse ToApiObject(this RestfulTVPApi.ConditionalAccess.BillingResponse response)
        {
            RestfulTVPApi.Objects.Responses.BillingResponse retVal = new RestfulTVPApi.Objects.Responses.BillingResponse();

            retVal.status = (RestfulTVPApi.Objects.Responses.Enums.BillingResponseStatus)response.m_oStatus;
            retVal.external_receipt_code = response.m_sExternalReceiptCode;
            retVal.reciept_code = response.m_sRecieptCode;
            retVal.status_description = response.m_sStatusDescription;
            
            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.Currency ToApiObject(this RestfulTVPApi.ConditionalAccess.Currency response)
        {
            RestfulTVPApi.Objects.Responses.Currency retVal = new RestfulTVPApi.Objects.Responses.Currency();

            retVal.currency_id = response.m_nCurrencyID;
            retVal.currency_cd2 = response.m_sCurrencyCD2;
            retVal.currency_cd3 = response.m_sCurrencyCD3;
            retVal.currency_sign = response.m_sCurrencySign;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.Price ToApiObject(this RestfulTVPApi.ConditionalAccess.Price response)
        {
            RestfulTVPApi.Objects.Responses.Price retVal = new RestfulTVPApi.Objects.Responses.Price();

            retVal.price = response.m_dPrice;
            if (response.m_oCurrency != null)
                retVal.currency = response.m_oCurrency.ToApiObject();

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.BillingTransactionContainer ToApiObject(this RestfulTVPApi.ConditionalAccess.BillingTransactionContainer response)
        {
            RestfulTVPApi.Objects.Responses.BillingTransactionContainer retVal = new RestfulTVPApi.Objects.Responses.BillingTransactionContainer();

            retVal.is_recurring = response.m_bIsRecurring;
            retVal.action_date = response.m_dtActionDate;
            retVal.end_date = response.m_dtEndDate;
            retVal.start_date = response.m_dtStartDate;
            retVal.billing_action = (RestfulTVPApi.Objects.Responses.Enums.BillingAction)response.m_eBillingAction;
            retVal.item_type = (RestfulTVPApi.Objects.Responses.Enums.BillingItemsType)response.m_eItemType;
            retVal.payment_method = (RestfulTVPApi.Objects.Responses.Enums.PaymentMethod)response.m_ePaymentMethod;
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

        public static RestfulTVPApi.Objects.Responses.BillingTransactionsResponse ToApiObject(this RestfulTVPApi.ConditionalAccess.BillingTransactionsResponse response)
        {
            RestfulTVPApi.Objects.Responses.BillingTransactionsResponse retVal = new RestfulTVPApi.Objects.Responses.BillingTransactionsResponse();

            retVal.transactions_count = response.m_nTransactionsCount;
            if (response.m_Transactions != null)
                retVal.transactions = response.m_Transactions.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.AdyenBillingDetail ToApiObject(this RestfulTVPApi.Billing.AdyenBillingDetail response)
        {
            RestfulTVPApi.Objects.Responses.AdyenBillingDetail retVal = new RestfulTVPApi.Objects.Responses.AdyenBillingDetail();

            if (response.billingInfo != null)
                retVal.billing_info = response.billingInfo.ToApiObject();

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.BillingInfo ToApiObject(this RestfulTVPApi.Billing.BillingInfo response)
        {
            RestfulTVPApi.Objects.Responses.BillingInfo retVal = new RestfulTVPApi.Objects.Responses.BillingInfo();

            retVal.cvc = response.cvc;
            retVal.expiry_month = response.expiryMonth;
            retVal.expiry_year = response.expiryYear;
            retVal.holder_name = response.holderName;
            retVal.last_four_digits = response.lastFourDigits;
            retVal.variant = response.variant;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.CampaignActionInfo ToApiObject(this RestfulTVPApi.ConditionalAccess.CampaignActionInfo response)
        {
            RestfulTVPApi.Objects.Responses.CampaignActionInfo retVal = new RestfulTVPApi.Objects.Responses.CampaignActionInfo();

            retVal.media_id = response.m_mediaID;
            retVal.site_guid = response.m_siteGuid;
            retVal.media_link = response.m_mediaLink;
            retVal.sender_name = response.m_senderName;
            retVal.sender_email = response.m_senderEmail;
            retVal.status = (RestfulTVPApi.Objects.Responses.Enums.CampaignActionResult)response.m_status;
            
            if (response.m_socialInviteInfo != null)
                retVal.social_invite_info = response.m_socialInviteInfo.ToApiObject();

            if (response.m_voucherReceipents != null)
            retVal.voucher_receipents = response.m_voucherReceipents.Where(x => x != null).Select(x => x.ToApiObject()).ToArray(); 

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.VoucherReceipentInfo ToApiObject(this RestfulTVPApi.ConditionalAccess.VoucherReceipentInfo response)
        {
            RestfulTVPApi.Objects.Responses.VoucherReceipentInfo retVal = new RestfulTVPApi.Objects.Responses.VoucherReceipentInfo();

            retVal.email_add = response.m_emailAdd;
            retVal.receipent_name = response.m_receipentName;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.SocialInviteInfo ToApiObject(this RestfulTVPApi.ConditionalAccess.SocialInviteInfo response)
        {
            RestfulTVPApi.Objects.Responses.SocialInviteInfo retVal = new RestfulTVPApi.Objects.Responses.SocialInviteInfo();

            retVal.hash_code = response.m_hashCode;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.MediaFileItemPricesContainer ToApiObject(this RestfulTVPApi.ConditionalAccess.MediaFileItemPricesContainer response)
        {
            RestfulTVPApi.Objects.Responses.MediaFileItemPricesContainer retVal = new RestfulTVPApi.Objects.Responses.MediaFileItemPricesContainer();

            retVal.media_file_id = response.m_nMediaFileID;

            if (response.m_oItemPrices != null)
                retVal.item_prices = response.m_oItemPrices.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            retVal.product_code = response.m_sProductCode;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.ItemPriceContainer ToApiObject(this RestfulTVPApi.ConditionalAccess.ItemPriceContainer response)
        {
            RestfulTVPApi.Objects.Responses.ItemPriceContainer retVal = new RestfulTVPApi.Objects.Responses.ItemPriceContainer();

            retVal.ppv_module_code = response.m_sPPVModuleCode;
            retVal.subscription_only = response.m_bSubscriptionOnly;

            if (response.m_oFullPrice != null)
                retVal.price = response.m_oPrice.ToApiObject();

            if (response.m_oFullPrice != null)
                retVal.full_price = response.m_oFullPrice.ToApiObject();

            retVal.price_reason = (RestfulTVPApi.Objects.Responses.Enums.PriceReason)response.m_PriceReason;

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

            retVal.coupon_status = (RestfulTVPApi.Objects.Responses.Enums.CouponsStatus)response.m_couponStatus;

            retVal.first_device_name_found = response.m_sFirstDeviceNameFound;
            retVal.is_cancel_window = response.m_bCancelWindow;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.Collection ToApiObject(this RestfulTVPApi.ConditionalAccess.Collection response)
        {
            RestfulTVPApi.Objects.Responses.Collection collection = new RestfulTVPApi.Objects.Responses.Collection();

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
                collection.codes = response.m_sCodes.Where(x => x != null).Select(x => x.ToApiObject() as RestfulTVPApi.Objects.Responses.BundleCodeContainer).ToArray();
            }

            if (response.m_sName != null)
            {
                collection.name = response.m_sName.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();
            }

            collection.file_types = response.m_sFileTypes != null ? response.m_sFileTypes.ToArray() : null;

            return collection;
        }

        public static RestfulTVPApi.Objects.Responses.Subscription ToApiObject(this RestfulTVPApi.ConditionalAccess.Subscription response)
        {
            RestfulTVPApi.Objects.Responses.Subscription retVal = new RestfulTVPApi.Objects.Responses.Subscription();

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

        public static RestfulTVPApi.Objects.Responses.PrePaidModule ToApiObject(this RestfulTVPApi.ConditionalAccess.PrePaidModule response)
        {
            RestfulTVPApi.Objects.Responses.PrePaidModule retVal = new RestfulTVPApi.Objects.Responses.PrePaidModule();

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

        public static RestfulTVPApi.Objects.Responses.LanguageContainer ToApiObject(this RestfulTVPApi.ConditionalAccess.LanguageContainer response)
        {
            RestfulTVPApi.Objects.Responses.LanguageContainer retVal = new RestfulTVPApi.Objects.Responses.LanguageContainer();

            retVal.language_code_3 = response.m_sLanguageCode3;
            retVal.value = response.m_sValue;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.SubscriptionCodeContainer ToApiObject(this RestfulTVPApi.ConditionalAccess.BundleCodeContainer response)
        {
            RestfulTVPApi.Objects.Responses.SubscriptionCodeContainer retVal = new RestfulTVPApi.Objects.Responses.SubscriptionCodeContainer();

            retVal.code = response.m_sCode;
            retVal.name = response.m_sName;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.PriceCode ToApiObject(this RestfulTVPApi.ConditionalAccess.PriceCode response)
        {
            RestfulTVPApi.Objects.Responses.PriceCode retVal = new RestfulTVPApi.Objects.Responses.PriceCode();

            retVal.code = response.m_sCode;

            if (response.m_oPrise != null)
                retVal.price = response.m_oPrise.ToApiObject();

            retVal.object_id = response.m_nObjectID;

            if (response.m_sDescription != null)
                retVal.description = response.m_sDescription.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();


            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.DiscountModule ToApiObject(this RestfulTVPApi.ConditionalAccess.DiscountModule response)
        {
            RestfulTVPApi.Objects.Responses.DiscountModule retVal = new RestfulTVPApi.Objects.Responses.DiscountModule();

            retVal.percent = response.m_dPercent;
            retVal.relation_type = (RestfulTVPApi.Objects.Responses.Enums.RelationTypes)response.m_eTheRelationType;
            retVal.start_date = response.m_dStartDate;
            retVal.end_date = response.m_dEndDate;

            if (response.m_oWhenAlgo != null)
                retVal.when_algo = response.m_oWhenAlgo.ToApiObject();

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.UsageModule ToApiObject(this RestfulTVPApi.ConditionalAccess.UsageModule response)
        {
            RestfulTVPApi.Objects.Responses.UsageModule retVal = new RestfulTVPApi.Objects.Responses.UsageModule();

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

        public static RestfulTVPApi.Objects.Responses.PreviewModule ToApiObject(this RestfulTVPApi.ConditionalAccess.PreviewModule response)
        {
            RestfulTVPApi.Objects.Responses.PreviewModule retVal = new RestfulTVPApi.Objects.Responses.PreviewModule();

            retVal.id = response.m_nID;
            retVal.name = response.m_sName;
            retVal.full_life_cycle = response.m_tsFullLifeCycle;
            retVal.non_renew_period = response.m_tsNonRenewPeriod;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.CouponsGroup ToApiObject(this RestfulTVPApi.ConditionalAccess.CouponsGroup response)
        {
            RestfulTVPApi.Objects.Responses.CouponsGroup retVal = new RestfulTVPApi.Objects.Responses.CouponsGroup();

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

        public static RestfulTVPApi.Objects.Responses.WhenAlgo ToApiObject(this RestfulTVPApi.ConditionalAccess.WhenAlgo response)
        {
            RestfulTVPApi.Objects.Responses.WhenAlgo retVal = new RestfulTVPApi.Objects.Responses.WhenAlgo();

            retVal.algo_type = (RestfulTVPApi.Objects.Responses.Enums.WhenAlgoType)response.m_eAlgoType;
            retVal.n_times = response.m_nNTimes;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.SubscriptionsPricesContainer ToApiObject(this RestfulTVPApi.ConditionalAccess.SubscriptionsPricesContainer response)
        {
            RestfulTVPApi.Objects.Responses.SubscriptionsPricesContainer retVal = new RestfulTVPApi.Objects.Responses.SubscriptionsPricesContainer();

            retVal.subscription_code = response.m_sSubscriptionCode;

            if (response.m_oPrice != null)
                retVal.price = response.m_oPrice.ToApiObject();

            retVal.price_reason = (RestfulTVPApi.Objects.Responses.Enums.PriceReason)response.m_PriceReason;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.UserBillingTransactionsResponse ToApiObject(this RestfulTVPApi.ConditionalAccess.UserBillingTransactionsResponse response)
        {
            RestfulTVPApi.Objects.Responses.UserBillingTransactionsResponse retVal = new RestfulTVPApi.Objects.Responses.UserBillingTransactionsResponse();

            retVal.site_guid = response.m_sSiteGUID;

            if (response.m_BillingTransactionResponse != null)
                retVal.billing_transaction_response = response.m_BillingTransactionResponse.ToApiObject();

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.DomainBillingTransactionsResponse ToApiObject(this RestfulTVPApi.ConditionalAccess.DomainBillingTransactionsResponse response)
        {
            RestfulTVPApi.Objects.Responses.DomainBillingTransactionsResponse retVal = new RestfulTVPApi.Objects.Responses.DomainBillingTransactionsResponse();

            retVal.domain_id = response.m_nDomainID;

            if (response.m_BillingTransactionResponses != null)
                retVal.billing_transaction_responses = response.m_BillingTransactionResponses.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.DomainResponseObject ToApiObject(this RestfulTVPApi.Domains.DomainResponseObject response)
        {
            RestfulTVPApi.Objects.Responses.DomainResponseObject retVal = new RestfulTVPApi.Objects.Responses.DomainResponseObject();

            if (response.m_oDomain != null)
                retVal.domain = response.m_oDomain.ToApiObject();

            retVal.domain_response_status = (RestfulTVPApi.Objects.Responses.Enums.DomainResponseStatus)response.m_oDomainResponseStatus;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.Enums.DomainResponseStatus toApiObject(this RestfulTVPApi.Domains.DomainStatus domainStatus)
        {
            RestfulTVPApi.Objects.Responses.Enums.DomainResponseStatus status = RestfulTVPApi.Objects.Responses.Enums.DomainResponseStatus.Error;

            switch (domainStatus)
            {
                case RestfulTVPApi.Domains.DomainStatus.OK:
                    status = RestfulTVPApi.Objects.Responses.Enums.DomainResponseStatus.OK;
                    break;
                case RestfulTVPApi.Domains.DomainStatus.DomainAlreadyExists:
                    status = RestfulTVPApi.Objects.Responses.Enums.DomainResponseStatus.DomainAlreadyExists;
                    break;
                case RestfulTVPApi.Domains.DomainStatus.ExceededLimit:
                    status = RestfulTVPApi.Objects.Responses.Enums.DomainResponseStatus.ExceededLimit;
                    break;
                case RestfulTVPApi.Domains.DomainStatus.DeviceTypeNotAllowed:
                    status = RestfulTVPApi.Objects.Responses.Enums.DomainResponseStatus.DeviceTypeNotAllowed;
                    break;
                case RestfulTVPApi.Domains.DomainStatus.UnKnown:
                    status = RestfulTVPApi.Objects.Responses.Enums.DomainResponseStatus.UnKnown;
                    break;
                case RestfulTVPApi.Domains.DomainStatus.Error:
                    status = RestfulTVPApi.Objects.Responses.Enums.DomainResponseStatus.Error;
                    break;
                case RestfulTVPApi.Domains.DomainStatus.DeviceNotInDomin:
                    status = RestfulTVPApi.Objects.Responses.Enums.DomainResponseStatus.DeviceNotInDomain;
                    break;
                case RestfulTVPApi.Domains.DomainStatus.MasterEmailAlreadyExists:
                    break;
                case RestfulTVPApi.Domains.DomainStatus.UserNotInDomain:
                    status = RestfulTVPApi.Objects.Responses.Enums.DomainResponseStatus.UserNotExistsInDomain;
                    break;
                case RestfulTVPApi.Domains.DomainStatus.DomainNotExists:
                    status = RestfulTVPApi.Objects.Responses.Enums.DomainResponseStatus.DomainNotExists;
                    break;
                case RestfulTVPApi.Domains.DomainStatus.HouseholdUserFailed:
                    status = RestfulTVPApi.Objects.Responses.Enums.DomainResponseStatus.HouseholdUserFailed;
                    break;
                case RestfulTVPApi.Domains.DomainStatus.DomainCreatedWithoutNPVRAccount:
                    status = RestfulTVPApi.Objects.Responses.Enums.DomainResponseStatus.DomainCreatedWithoutNPVRAccount;
                    break;
                default:
                    break;
            }           

            return status;

        }

        //public static RestfulTVPApi.Objects.Responses.Enums.DomainResponseStatus toApiObject(this RestfulTVPApi.Domains.DomainStatus domainStatus)
        //{
        //    RestfulTVPApi.Objects.Responses.Enums.DomainResponseStatus status = RestfulTVPApi.Objects.Responses.Enums.DomainResponseStatus.Error;
        //    switch (domainStatus)
        //    {
        //        case RestfulTVPApi.Domains.DomainStatus.OK:
        //            status = RestfulTVPApi.Objects.Responses.Enums.DomainResponseStatus.OK;
        //            break;
        //        case RestfulTVPApi.Domains.DomainStatus.DomainAlreadyExists:
        //            status = RestfulTVPApi.Objects.Responses.Enums.DomainResponseStatus.DomainAlreadyExists;
        //            break;
        //        case RestfulTVPApi.Domains.DomainStatus.ExceededLimit:
        //            status = RestfulTVPApi.Objects.Responses.Enums.DomainResponseStatus.ExceededLimit;
        //            break;
        //        case RestfulTVPApi.Domains.DomainStatus.DeviceTypeNotAllowed:
        //            status = RestfulTVPApi.Objects.Responses.Enums.DomainResponseStatus.DeviceTypeNotAllowed;
        //            break;
        //        case RestfulTVPApi.Domains.DomainStatus.UnKnown:
        //            status = RestfulTVPApi.Objects.Responses.Enums.DomainResponseStatus.UnKnown;
        //            break;
        //        case RestfulTVPApi.Domains.DomainStatus.Error:
        //            status = RestfulTVPApi.Objects.Responses.Enums.DomainResponseStatus.Error;
        //            break;
        //        case RestfulTVPApi.Domains.DomainStatus.DeviceNotInDomin:
        //            status = RestfulTVPApi.Objects.Responses.Enums.DomainResponseStatus.DeviceNotInDomain;
        //            break;
        //        case RestfulTVPApi.Domains.DomainStatus.UserNotInDomain:
        //            status = RestfulTVPApi.Objects.Responses.Enums.DomainResponseStatus.UserNotExistsInDomain;
        //            break;
        //        case RestfulTVPApi.Domains.DomainStatus.DomainNotExists:
        //            status = RestfulTVPApi.Objects.Responses.Enums.DomainResponseStatus.DomainNotExists;
        //            break;
        //        case RestfulTVPApi.Domains.DomainStatus.HouseholdUserFailed:
        //            status = RestfulTVPApi.Objects.Responses.Enums.DomainResponseStatus.HouseholdUserFailed;
        //            break;
        //        default:
        //            status = RestfulTVPApi.Objects.Responses.Enums.DomainResponseStatus.UnKnown;
        //            break;
        //    }

        //    return status;

        //}
    

        public static RestfulTVPApi.Objects.Responses.Domain ToApiObject(this RestfulTVPApi.Domains.Domain response)
        {
            RestfulTVPApi.Objects.Responses.Domain retVal = new RestfulTVPApi.Objects.Responses.Domain();

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
            retVal.domain_status = (RestfulTVPApi.Objects.Responses.Enums.DomainStatus)response.m_DomainStatus;
            retVal.frequency_flag = response.m_frequencyFlag;
            retVal.next_action_freq = response.m_NextActionFreq;
            retVal.domain_restriction = (RestfulTVPApi.Objects.Responses.Enums.DomainRestriction)response.m_DomainRestriction;
            retVal.next_user_action_freq = response.m_NextUserActionFreq;
            retVal.sso_operator_id = response.m_nSSOOperatorID;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.DeviceContainer ToApiObject(this RestfulTVPApi.Domains.DeviceContainer response)
        {
            RestfulTVPApi.Objects.Responses.DeviceContainer retVal = new RestfulTVPApi.Objects.Responses.DeviceContainer();

            retVal.name = response.m_deviceFamilyName;
            retVal.device_family_id = response.m_deviceFamilyID;
            retVal.device_limit = response.m_deviceLimit;
            retVal.device_concurrent_limit = response.m_deviceConcurrentLimit;

            if (response.DeviceInstances != null)
                retVal.instances = response.DeviceInstances.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.Device ToApiObject(this RestfulTVPApi.Domains.Device response)
        {
            RestfulTVPApi.Objects.Responses.Device retVal = new RestfulTVPApi.Objects.Responses.Device();

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
            retVal.state = (RestfulTVPApi.Objects.Responses.Enums.DeviceState)response.m_state;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.DeviceResponseObject ToApiObject(this RestfulTVPApi.Domains.DeviceResponseObject response)
        {
            RestfulTVPApi.Objects.Responses.DeviceResponseObject retVal = new RestfulTVPApi.Objects.Responses.DeviceResponseObject();

            if (response.m_oDevice != null)
                retVal.device = response.m_oDevice.ToApiObject();

            retVal.device_response_status = (RestfulTVPApi.Objects.Responses.Enums.DeviceResponseStatus)response.m_oDeviceResponseStatus;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.PPVModule ToApiObject(this RestfulTVPApi.Pricing.PPVModule response)
        {
            RestfulTVPApi.Objects.Responses.PPVModule retVal = new RestfulTVPApi.Objects.Responses.PPVModule();

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

        public static RestfulTVPApi.Objects.Responses.PriceCode ToApiObject(this RestfulTVPApi.Pricing.PriceCode response)
        {
            RestfulTVPApi.Objects.Responses.PriceCode retVal = new RestfulTVPApi.Objects.Responses.PriceCode();

            retVal.code = response.m_sCode;

            if (response.m_oPrise != null)
                retVal.price = response.m_oPrise.ToApiObject();

            retVal.object_id = response.m_nObjectID;

            if (response.m_sDescription != null)
                retVal.description = response.m_sDescription.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.UsageModule ToApiObject(this RestfulTVPApi.Pricing.UsageModule response)
        {
            RestfulTVPApi.Objects.Responses.UsageModule retVal = new RestfulTVPApi.Objects.Responses.UsageModule();

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

        public static RestfulTVPApi.Objects.Responses.DiscountModule ToApiObject(this RestfulTVPApi.Pricing.DiscountModule response)
        {
            RestfulTVPApi.Objects.Responses.DiscountModule retVal = new RestfulTVPApi.Objects.Responses.DiscountModule();

            retVal.percent = response.m_dPercent;
            retVal.relation_type = (RestfulTVPApi.Objects.Responses.Enums.RelationTypes)response.m_eTheRelationType;
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

        public static RestfulTVPApi.Objects.Responses.CouponsGroup ToApiObject(this RestfulTVPApi.Pricing.CouponsGroup response)
        {
            RestfulTVPApi.Objects.Responses.CouponsGroup retVal = new RestfulTVPApi.Objects.Responses.CouponsGroup();

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

        public static RestfulTVPApi.Objects.Responses.LanguageContainer ToApiObject(this RestfulTVPApi.Pricing.LanguageContainer response)
        {
            RestfulTVPApi.Objects.Responses.LanguageContainer retVal = new RestfulTVPApi.Objects.Responses.LanguageContainer();

            retVal.language_code_3 = response.m_sLanguageCode3;
            retVal.value = response.m_sValue;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.Price ToApiObject(this RestfulTVPApi.Pricing.Price response)
        {
            RestfulTVPApi.Objects.Responses.Price retVal = new RestfulTVPApi.Objects.Responses.Price();

            retVal.price = response.m_dPrice;

            if (response.m_oCurrency != null)
                retVal.currency = response.m_oCurrency.ToApiObject();

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.WhenAlgo ToApiObject(this RestfulTVPApi.Pricing.WhenAlgo response)
        {
            RestfulTVPApi.Objects.Responses.WhenAlgo retVal = new RestfulTVPApi.Objects.Responses.WhenAlgo();

            retVal.algo_type = (RestfulTVPApi.Objects.Responses.Enums.WhenAlgoType)response.m_eAlgoType;
            retVal.n_times = response.m_nNTimes;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.Currency ToApiObject(this RestfulTVPApi.Pricing.Currency response)
        {
            RestfulTVPApi.Objects.Responses.Currency retVal = new RestfulTVPApi.Objects.Responses.Currency();

            retVal.currency_id = response.m_nCurrencyID;
            retVal.currency_cd2 = response.m_sCurrencyCD2;
            retVal.currency_cd3 = response.m_sCurrencyCD3;
            retVal.currency_sign = response.m_sCurrencySign;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.CouponData ToApiObject(this RestfulTVPApi.Pricing.CouponData response)
        {
            RestfulTVPApi.Objects.Responses.CouponData retVal = new RestfulTVPApi.Objects.Responses.CouponData();

            retVal.coupon_status = (RestfulTVPApi.Objects.Responses.Enums.CouponsStatus)response.m_CouponStatus;

            if (response.m_oCouponGroup != null)
                retVal.coupon_group = response.m_oCouponGroup.ToApiObject();

            retVal.coupon_type = (RestfulTVPApi.Objects.Responses.Enums.CouponType)response.m_CouponType;
            retVal.camp_id = response.m_campID;
            retVal.owner_guid = response.m_ownerGUID;
            retVal.owner_media = response.m_ownerMedia;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.Subscription ToApiObject(this RestfulTVPApi.Pricing.Subscription response)
        {
            RestfulTVPApi.Objects.Responses.Subscription retVal = new RestfulTVPApi.Objects.Responses.Subscription();

            if (response.m_sCodes != null)
                retVal.codes = response.m_sCodes.Where(x => x != null).Select(x => x.ToApiObject(Tvinci.Data.Loaders.TvinciPlatform.Catalog.eBundleType.SUBSCRIPTION) as RestfulTVPApi.Objects.Responses.SubscriptionCodeContainer).ToArray();

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

        public static RestfulTVPApi.Objects.Responses.UserType ToApiObject(this RestfulTVPApi.Pricing.UserType response)
        {
            RestfulTVPApi.Objects.Responses.UserType user = new RestfulTVPApi.Objects.Responses.UserType();

            user.description = response.Description;
            user.id = response.ID;
            user.is_default = response.IsDefault;

            return user;
        }


        //public static RestfulTVPApi.Objects.Responses.SubscriptionCodeContainer ToApiObject(this RestfulTVPApi.Pricing.BundleCodeContainer response)
        //{
        //    RestfulTVPApi.Objects.Responses.SubscriptionCodeContainer retVal = new RestfulTVPApi.Objects.Responses.SubscriptionCodeContainer();

        //    retVal.code = response.m_sCode;
        //    retVal.name = response.m_sName;

        //    return retVal;
        //}

        public static object ToApiObject(this RestfulTVPApi.Pricing.BundleCodeContainer response, Tvinci.Data.Loaders.TvinciPlatform.Catalog.eBundleType type)
        {
            object container = null;

            switch (type)
            {
                case Tvinci.Data.Loaders.TvinciPlatform.Catalog.eBundleType.SUBSCRIPTION:
                    container = new RestfulTVPApi.Objects.Responses.SubscriptionCodeContainer() { code = response.m_sCode, name = response.m_sName };
                    break;
                case Tvinci.Data.Loaders.TvinciPlatform.Catalog.eBundleType.COLLECTION:
                    container = new RestfulTVPApi.Objects.Responses.BundleCodeContainer() { code = response.m_sCode, name = response.m_sName };
                    break;
                default:
                    break;
            }

            return container;
        }

        public static RestfulTVPApi.Objects.Responses.PreviewModule ToApiObject(this RestfulTVPApi.Pricing.PreviewModule response)
        {
            RestfulTVPApi.Objects.Responses.PreviewModule retVal = new RestfulTVPApi.Objects.Responses.PreviewModule();

            retVal.id = response.m_nID;
            retVal.name = response.m_sName;
            retVal.full_life_cycle = response.m_tsFullLifeCycle;
            retVal.non_renew_period = response.m_tsNonRenewPeriod;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.GroupOperator ToApiObject(this RestfulTVPApi.Api.GroupOperator response)
        {
            RestfulTVPApi.Objects.Responses.GroupOperator retVal = new RestfulTVPApi.Objects.Responses.GroupOperator();

            if (response.UIData != null)
                retVal.ui_data = response.UIData.ToApiObject();

            retVal.id = response.ID;
            retVal.name = response.Name;
            retVal.type = (RestfulTVPApi.Objects.Responses.Enums.eOperatorType)response.Type;
            retVal.login_url = response.LoginUrl;
            retVal.sub_group_id = response.SubGroupID;

            if (response.Scopes != null)
                retVal.scopes = response.Scopes.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            retVal.group_user_name = response.GroupUserName;
            retVal.group_password = response.GroupPassword;
            retVal.logout_url = response.LogoutURL;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.Scope ToApiObject(this RestfulTVPApi.Api.Scope response)
        {
            RestfulTVPApi.Objects.Responses.Scope retVal = new RestfulTVPApi.Objects.Responses.Scope();

            retVal.login_url = response.LoginUrl;
            retVal.logout_url = response.LogoutUrl;
            retVal.name = response.Name;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.UIData ToApiObject(this RestfulTVPApi.Api.UIData response)
        {
            RestfulTVPApi.Objects.Responses.UIData retVal = new RestfulTVPApi.Objects.Responses.UIData();

            retVal.color_code = response.ColorCode;
            //retVal.pic_id = response.picID; // TODO: Check what are the changes regarding this parameter

            return retVal;
        }

        /*public static RestfulTVPApi.Objects.Responses.UserSocialActionObject ToApiObject(this RestfulTVPApi.Social.UserSocialActionObject response)
        {
            RestfulTVPApi.Objects.Responses.UserSocialActionObject retVal = new RestfulTVPApi.Objects.Responses.UserSocialActionObject();

            retVal.site_guid = response.m_sSiteGuid;
            retVal.social_action = (TVPApiModule.Objects.eUserAction)response.m_eSocialAction;
            retVal.social_platform = (RestfulTVPApi.Objects.Responses.SocialPlatform)response.m_eSocialPlatform;
            retVal.media_id = response.nMediaID;
            retVal.program_id = response.nProgramID;
            retVal.asset_type = (RestfulTVPApi.Objects.Responses.eAssetType)response.assetType;
            retVal.action_date = response.m_dActionDate;


            return retVal;
        }*/

        public static RestfulTVPApi.Objects.Responses.FBSignIn ToApiObject(this RestfulTVPApi.Social.FBSignin response)
        {
            RestfulTVPApi.Objects.Responses.FBSignIn retVal = new RestfulTVPApi.Objects.Responses.FBSignIn();

            retVal.status = response.status.ToString();
            if (response.user != null)
            {
                retVal.user = response.user.ToApiObject();
            }

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.UserResponseObject ToApiObject(this RestfulTVPApi.Social.UserResponseObject response)
        {
            RestfulTVPApi.Objects.Responses.UserResponseObject retVal = new RestfulTVPApi.Objects.Responses.UserResponseObject();

            retVal.resp_status = (RestfulTVPApi.Objects.Responses.Enums.eResponseStatus)response.m_RespStatus;
            retVal.user_instance_id = response.m_userInstanceID;
            if (response.m_user != null)
            {
                retVal.user = response.m_user.ToApiObject();
            }

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.User ToApiObject(this RestfulTVPApi.Social.User response)
        {
            RestfulTVPApi.Objects.Responses.User retVal = new RestfulTVPApi.Objects.Responses.User();

            retVal.domain_id = response.m_domianID;
            retVal.user_state = (RestfulTVPApi.Objects.Responses.Enums.UserState)response.m_eUserState;
            retVal.is_domain_master = response.m_isDomainMaster;
            retVal.sso_operator_id = response.m_nSSOOperatorID;

            if (response.m_oBasicData != null)
            {
                retVal.basic_data = new RestfulTVPApi.Objects.Responses.UserBasicData();

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
                retVal.dynamic_data = new RestfulTVPApi.Objects.Responses.UserDynamicData();

                if (response.m_oDynamicData.m_sUserData != null)
                {
                    retVal.dynamic_data.user_data = response.m_oDynamicData.m_sUserData.Select(x => new RestfulTVPApi.Objects.Responses.UserDynamicDataContainer()
                    {
                        data_type = x.m_sDataType,
                        value = x.m_sValue
                    }).ToArray();
                }
            }

            retVal.site_guid = response.m_sSiteGUID;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.UserType ToApiObject(this RestfulTVPApi.Social.UserType response)
        {
            RestfulTVPApi.Objects.Responses.UserType userType = new RestfulTVPApi.Objects.Responses.UserType();

            userType.description = response.Description;
            userType.id = response.ID;
            userType.is_default = response.IsDefault;

            return userType;
        }

        public static RestfulTVPApi.Objects.Responses.State ToApiObject(this RestfulTVPApi.Social.State response)
        {
            RestfulTVPApi.Objects.Responses.State state = new RestfulTVPApi.Objects.Responses.State();

            state.state_code = response.m_sStateCode;
            state.state_name = response.m_sStateName;
            state.object_id = response.m_nObjecrtID;

            if (response.m_Country != null)
            {
                state.country = response.m_Country.ToApiObject();
            }

            return state;
        }

        public static RestfulTVPApi.Objects.Responses.Country ToApiObject(this RestfulTVPApi.Social.Country response)
        {
            RestfulTVPApi.Objects.Responses.Country country = new RestfulTVPApi.Objects.Responses.Country();

            country.country_code = response.m_sCountryCode;
            country.country_name = response.m_sCountryName;
            country.object_id = response.m_nObjecrtID;            

            return country;
        }

        public static RestfulTVPApi.Objects.Responses.UserBasicData ToApiObject(this RestfulTVPApi.Users.UserBasicData response)
        {
            RestfulTVPApi.Objects.Responses.UserBasicData retVal = new RestfulTVPApi.Objects.Responses.UserBasicData();

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

        public static RestfulTVPApi.Objects.Responses.FriendWatchedObject ToApiObject(this RestfulTVPApi.Social.FriendWatchedObject response)
        {
            RestfulTVPApi.Objects.Responses.FriendWatchedObject retVal = new RestfulTVPApi.Objects.Responses.FriendWatchedObject();

            retVal.site_guid = response.SiteGuid;
            retVal.media_id = response.MediaID;
            retVal.update_date = response.UpdateDate;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.FacebookConfig ToApiObject(this RestfulTVPApi.Social.FacebookConfig response)
        {
            RestfulTVPApi.Objects.Responses.FacebookConfig retVal = new RestfulTVPApi.Objects.Responses.FacebookConfig();

            retVal.fb_key = response.sFBKey;
            retVal.fb_secret = response.sFBSecret;
            retVal.fb_callback = response.sFBCallback;
            retVal.fb_min_friends = response.nFBMinFriends;
            retVal.fb_permissions = response.sFBPermissions;
            retVal.fb_redirect = response.sFBRedirect;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.FBInterestData ToApiObject(this RestfulTVPApi.Social.FBInterestData response)
        {
            RestfulTVPApi.Objects.Responses.FBInterestData retVal = new RestfulTVPApi.Objects.Responses.FBInterestData();

            retVal.name = response.name;
            retVal.category = response.category;
            retVal.id = response.id;
            retVal.created_time = response.created_time;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.FBInterest ToApiObject(this RestfulTVPApi.Social.FBInterest response)
        {
            RestfulTVPApi.Objects.Responses.FBInterest retVal = new RestfulTVPApi.Objects.Responses.FBInterest();

            if (response.data != null)
                retVal.data = response.data.Where(x => x != null).Select(x => x.ToApiObject()).ToArray();

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.FBLoaction ToApiObject(this RestfulTVPApi.Social.FBLoaction response)
        {
            RestfulTVPApi.Objects.Responses.FBLoaction retVal = new RestfulTVPApi.Objects.Responses.FBLoaction();

            retVal.name = response.name;
            retVal.id = response.id;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.FBUser ToApiObject(this RestfulTVPApi.Social.FBUser response)
        {
            RestfulTVPApi.Objects.Responses.FBUser retVal = new RestfulTVPApi.Objects.Responses.FBUser();

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

        public static RestfulTVPApi.Objects.Responses.FacebookResponseObject ToApiObject(this RestfulTVPApi.Social.FacebookResponseObject response)
        {
            RestfulTVPApi.Objects.Responses.FacebookResponseObject retVal = new RestfulTVPApi.Objects.Responses.FacebookResponseObject();

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

        public static RestfulTVPApi.Objects.Responses.KeyValuePair ToApiObject(this RestfulTVPApi.Social.KeyValuePair response)
        {
            RestfulTVPApi.Objects.Responses.KeyValuePair retVal = new RestfulTVPApi.Objects.Responses.KeyValuePair();

            retVal.key = response.key;
            retVal.value = response.value;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.KeyValuePair ToApiObject(this RestfulTVPApi.Users.KeyValuePair response)
        {
            RestfulTVPApi.Objects.Responses.KeyValuePair retVal = new RestfulTVPApi.Objects.Responses.KeyValuePair();

            retVal.key = response.key;
            retVal.value = response.value;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.NetworkResponseObject ToApiObject(this RestfulTVPApi.Domains.NetworkResponseObject response)
        {
            RestfulTVPApi.Objects.Responses.NetworkResponseObject networkResponse = null;

            if (response != null)
            {
                networkResponse = new RestfulTVPApi.Objects.Responses.NetworkResponseObject();
                networkResponse.IsSuccess = response.bSuccess;
                networkResponse.Reason = (RestfulTVPApi.Objects.Responses.Enums.NetworkResponseStatus)response.eReason;
            }

            return networkResponse;
        }

        public static RestfulTVPApi.Objects.Responses.HomeNetwork ToApiObject(this RestfulTVPApi.Domains.HomeNetwork response)
        {
            RestfulTVPApi.Objects.Responses.HomeNetwork homeNetworkResponse = null;

            if (response != null)
            {
                homeNetworkResponse = new RestfulTVPApi.Objects.Responses.HomeNetwork();
                homeNetworkResponse.create_date = response.CreateDate;
                homeNetworkResponse.description = response.Description;
                homeNetworkResponse.is_active = response.IsActive;
                homeNetworkResponse.name = response.Name;
                homeNetworkResponse.uid = response.UID;
            }

            return homeNetworkResponse;
        }

        public static RestfulTVPApi.Objects.Responses.Collection ToApiObject(this RestfulTVPApi.Pricing.Collection response)
        {
            RestfulTVPApi.Objects.Responses.Collection retVal = new RestfulTVPApi.Objects.Responses.Collection();

            if (response.m_sCodes != null)
                retVal.codes = response.m_sCodes.Where(x => x != null).Select(x => x.ToApiObject(Tvinci.Data.Loaders.TvinciPlatform.Catalog.eBundleType.COLLECTION) as RestfulTVPApi.Objects.Responses.BundleCodeContainer).ToArray();

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

        public static RestfulTVPApi.Objects.Responses.CollectionPricesContainer ToApiObject(this RestfulTVPApi.ConditionalAccess.CollectionsPricesContainer response)
        {
            RestfulTVPApi.Objects.Responses.CollectionPricesContainer retVal = new RestfulTVPApi.Objects.Responses.CollectionPricesContainer();

            retVal.collection_code = response.m_sCollectionCode;

            if (response.m_oPrice != null)
                retVal.price = response.m_oPrice.ToApiObject();

            retVal.price_reason = (RestfulTVPApi.Objects.Responses.Enums.PriceReason)response.m_PriceReason;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.PermittedCollectionContainer ToApiObject(this RestfulTVPApi.ConditionalAccess.PermittedCollectionContainer response)
        {
            RestfulTVPApi.Objects.Responses.PermittedCollectionContainer retVal = new RestfulTVPApi.Objects.Responses.PermittedCollectionContainer();

            retVal.current_date = response.m_dCurrentDate;
            retVal.end_date = response.m_dEndDate;
            retVal.last_view_date = response.m_dLastViewDate;
            retVal.purchase_date = response.m_dPurchaseDate;
            retVal.collection_purchase_id = response.m_nCollectionPurchaseID;
            retVal.payment_method = (RestfulTVPApi.Objects.Responses.Enums.PaymentMethod)response.m_paymentMethod;
            retVal.device_name = response.m_sDeviceName;
            retVal.device_udid = response.m_sDeviceUDID;
            retVal.collection_code = response.m_sCollectionCode;
            retVal.is_cancel_window = response.m_bCancelWindow;

            return retVal;
        }

        public static RestfulTVPApi.Objects.Responses.DoSocialActionResponse ToApiObject(this RestfulTVPApi.Social.DoSocialActionResponse response)
        {
            RestfulTVPApi.Objects.Responses.DoSocialActionResponse returnedResponse = new RestfulTVPApi.Objects.Responses.DoSocialActionResponse();

            returnedResponse.action_response_status_extern = (RestfulTVPApi.Objects.Responses.Enums.SocialActionResponseStatus)response.m_eActionResponseStatusExtern;
            returnedResponse.action_response_status_intern = (RestfulTVPApi.Objects.Responses.Enums.SocialActionResponseStatus)response.m_eActionResponseStatusIntern;
            
            return returnedResponse;
        }

        /*public static RestfulTVPApi.Objects.Responses.Status ToApiObject(this RestfulTVPApi.ConditionalAccess.StatusObject response)
        {
            RestfulTVPApi.Objects.Responses.Status retVal = new RestfulTVPApi.Objects.Responses.Status();

            retVal.message = response.Message;
            retVal.code = response.Code;

            return retVal;
        }*/
    }
}
