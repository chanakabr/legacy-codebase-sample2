// NOTICE: This is a generated file, to modify it, edit Program.cs in Reflector project
using System;
using System.Linq;
using System.Web;
using WebAPI.App_Start;
using WebAPI.EventNotifications;
using WebAPI.Filters;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Models.Billing;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.DMS;
using WebAPI.Models.Domains;
using WebAPI.Models.General;
using WebAPI.Models.Notification;
using WebAPI.Models.Notifications;
using WebAPI.Models.Partner;
using WebAPI.Models.Pricing;
using WebAPI.Models.Social;
using WebAPI.Models.Users;
using static WebAPI.App_Start.WrappingHandler; 

namespace WebAPI.Reflection
{
    public class KalturaJsonSerializer
    {
        public static string Serialize(IKalturaJsonable ottObject)
        {
            Version currentVersion = (Version)HttpContext.Current.Items[RequestParser.REQUEST_VERSION];
            string ret = "{";
            bool append;
            switch (ottObject.GetType().Name)
            {
                case "KalturaAccessControlBlockAction":
                    KalturaAccessControlBlockAction kalturaAccessControlBlockAction = ottObject as KalturaAccessControlBlockAction;
                    ret += "\"objectType\": " + "\"" + kalturaAccessControlBlockAction.objectType + "\"";
                    if(kalturaAccessControlBlockAction.relatedObjects != null && kalturaAccessControlBlockAction.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAccessControlBlockAction.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"type\": " + kalturaAccessControlBlockAction.Type;
                    break;
                    
                case "KalturaAccessControlMessage":
                    KalturaAccessControlMessage kalturaAccessControlMessage = ottObject as KalturaAccessControlMessage;
                    ret += "\"objectType\": " + "\"" + kalturaAccessControlMessage.objectType + "\"";
                    if(kalturaAccessControlMessage.relatedObjects != null && kalturaAccessControlMessage.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAccessControlMessage.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"code\": " + "\"" + kalturaAccessControlMessage.Code + "\"";
                    ret += ", \"message\": " + "\"" + kalturaAccessControlMessage.Message + "\"";
                    break;
                    
                case "KalturaActionPermissionItem":
                    KalturaActionPermissionItem kalturaActionPermissionItem = ottObject as KalturaActionPermissionItem;
                    ret += "\"objectType\": " + "\"" + kalturaActionPermissionItem.objectType + "\"";
                    if(kalturaActionPermissionItem.relatedObjects != null && kalturaActionPermissionItem.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaActionPermissionItem.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"action\": " + "\"" + kalturaActionPermissionItem.Action + "\"";
                    ret += ", \"actionPrivacy\": " + kalturaActionPermissionItem.ActionPrivacy;
                    if(kalturaActionPermissionItem.Network.HasValue)
                    {
                        ret += ", \"network\": " + kalturaActionPermissionItem.Network;
                    }
                    ret += ", \"privacy\": " + kalturaActionPermissionItem.Privacy;
                    break;
                    
                case "KalturaAdsContext":
                    KalturaAdsContext kalturaAdsContext = ottObject as KalturaAdsContext;
                    ret += "\"objectType\": " + "\"" + kalturaAdsContext.objectType + "\"";
                    if(kalturaAdsContext.relatedObjects != null && kalturaAdsContext.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAdsContext.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaAdsContext.Sources != null && kalturaAdsContext.Sources.Count > 0)
                    {
                        ret += ", \"sources\": " + "[" + String.Join(", ", kalturaAdsContext.Sources.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaAdsSource":
                    KalturaAdsSource kalturaAdsSource = ottObject as KalturaAdsSource;
                    ret += "\"objectType\": " + "\"" + kalturaAdsSource.objectType + "\"";
                    if(kalturaAdsSource.relatedObjects != null && kalturaAdsSource.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAdsSource.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"adsParam\": " + "\"" + kalturaAdsSource.AdsParams + "\"";
                    if(kalturaAdsSource.AdsPolicy.HasValue)
                    {
                        ret += ", \"adsPolicy\": " + kalturaAdsSource.AdsPolicy;
                    }
                    if(kalturaAdsSource.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaAdsSource.Id;
                    }
                    ret += ", \"type\": " + "\"" + kalturaAdsSource.Type + "\"";
                    break;
                    
                case "KalturaAggregationCountFilter":
                    KalturaAggregationCountFilter kalturaAggregationCountFilter = ottObject as KalturaAggregationCountFilter;
                    ret += "\"objectType\": " + "\"" + kalturaAggregationCountFilter.objectType + "\"";
                    if(kalturaAggregationCountFilter.relatedObjects != null && kalturaAggregationCountFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAggregationCountFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaAggregationCountFilter.OrderBy;
                    break;
                    
                case "KalturaAnnouncement":
                    KalturaAnnouncement kalturaAnnouncement = ottObject as KalturaAnnouncement;
                    ret += "\"objectType\": " + "\"" + kalturaAnnouncement.objectType + "\"";
                    if(kalturaAnnouncement.relatedObjects != null && kalturaAnnouncement.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAnnouncement.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaAnnouncement.Enabled.HasValue)
                    {
                        ret += ", \"enabled\": " + kalturaAnnouncement.Enabled;
                    }
                    if(kalturaAnnouncement.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaAnnouncement.Id;
                    }
                    ret += ", \"imageUrl\": " + "\"" + kalturaAnnouncement.ImageUrl + "\"";
                    ret += ", \"message\": " + "\"" + kalturaAnnouncement.Message + "\"";
                    ret += ", \"name\": " + "\"" + kalturaAnnouncement.Name + "\"";
                    ret += ", \"recipients\": " + kalturaAnnouncement.Recipients;
                    if(kalturaAnnouncement.StartTime.HasValue)
                    {
                        ret += ", \"startTime\": " + kalturaAnnouncement.StartTime;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"start_time\": " + kalturaAnnouncement.StartTime;
                        }
                    }
                    ret += ", \"status\": " + kalturaAnnouncement.Status;
                    ret += ", \"timezone\": " + "\"" + kalturaAnnouncement.Timezone + "\"";
                    break;
                    
                case "KalturaAnnouncementFilter":
                    KalturaAnnouncementFilter kalturaAnnouncementFilter = ottObject as KalturaAnnouncementFilter;
                    ret += "\"objectType\": " + "\"" + kalturaAnnouncementFilter.objectType + "\"";
                    if(kalturaAnnouncementFilter.relatedObjects != null && kalturaAnnouncementFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAnnouncementFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaAnnouncementFilter.OrderBy;
                    break;
                    
                case "KalturaAnnouncementListResponse":
                    KalturaAnnouncementListResponse kalturaAnnouncementListResponse = ottObject as KalturaAnnouncementListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaAnnouncementListResponse.objectType + "\"";
                    if(kalturaAnnouncementListResponse.relatedObjects != null && kalturaAnnouncementListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAnnouncementListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaAnnouncementListResponse.TotalCount;
                    if(kalturaAnnouncementListResponse.Announcements != null && kalturaAnnouncementListResponse.Announcements.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaAnnouncementListResponse.Announcements.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaApiActionPermissionItem":
                    KalturaApiActionPermissionItem kalturaApiActionPermissionItem = ottObject as KalturaApiActionPermissionItem;
                    ret += "\"objectType\": " + "\"" + kalturaApiActionPermissionItem.objectType + "\"";
                    if(kalturaApiActionPermissionItem.relatedObjects != null && kalturaApiActionPermissionItem.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaApiActionPermissionItem.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaApiActionPermissionItem.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaApiActionPermissionItem.Id;
                    }
                    ret += ", \"isExcluded\": " + kalturaApiActionPermissionItem.IsExcluded;
                    ret += ", \"name\": " + "\"" + kalturaApiActionPermissionItem.Name + "\"";
                    ret += ", \"action\": " + "\"" + kalturaApiActionPermissionItem.Action + "\"";
                    ret += ", \"service\": " + "\"" + kalturaApiActionPermissionItem.Service + "\"";
                    break;
                    
                case "KalturaApiArgumentPermissionItem":
                    KalturaApiArgumentPermissionItem kalturaApiArgumentPermissionItem = ottObject as KalturaApiArgumentPermissionItem;
                    ret += "\"objectType\": " + "\"" + kalturaApiArgumentPermissionItem.objectType + "\"";
                    if(kalturaApiArgumentPermissionItem.relatedObjects != null && kalturaApiArgumentPermissionItem.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaApiArgumentPermissionItem.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaApiArgumentPermissionItem.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaApiArgumentPermissionItem.Id;
                    }
                    ret += ", \"isExcluded\": " + kalturaApiArgumentPermissionItem.IsExcluded;
                    ret += ", \"name\": " + "\"" + kalturaApiArgumentPermissionItem.Name + "\"";
                    ret += ", \"action\": " + "\"" + kalturaApiArgumentPermissionItem.Action + "\"";
                    ret += ", \"parameter\": " + "\"" + kalturaApiArgumentPermissionItem.Parameter + "\"";
                    ret += ", \"service\": " + "\"" + kalturaApiArgumentPermissionItem.Service + "\"";
                    break;
                    
                case "KalturaAPIException":
                    KalturaAPIException kalturaAPIException = ottObject as KalturaAPIException;
                    append = false;
                    if(kalturaAPIException.args != null && kalturaAPIException.args.Count > 0)
                    {
                        append = true;
                        ret += "\"args\": " + "[" + String.Join(", ", kalturaAPIException.args.Select(item => Serialize(item))) + "]";
                    }
                    if(append)
                    {
                        ret += ", ";
                    }
                    ret += "\"code\": " + "\"" + kalturaAPIException.code + "\"";
                    ret += ", \"message\": " + "\"" + kalturaAPIException.message + "\"";
                    ret += ", \"objectType\": " + "\"" + kalturaAPIException.objectType + "\"";
                    break;
                    
                case "KalturaApiExceptionArg":
                    KalturaApiExceptionArg kalturaApiExceptionArg = ottObject as KalturaApiExceptionArg;
                    ret += "\"objectType\": " + "\"" + kalturaApiExceptionArg.objectType + "\"";
                    if(kalturaApiExceptionArg.relatedObjects != null && kalturaApiExceptionArg.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaApiExceptionArg.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"name\": " + "\"" + kalturaApiExceptionArg.name + "\"";
                    ret += ", \"value\": " + "\"" + kalturaApiExceptionArg.value + "\"";
                    break;
                    
                case "KalturaAPIExceptionWrapper":
                    KalturaAPIExceptionWrapper kalturaAPIExceptionWrapper = ottObject as KalturaAPIExceptionWrapper;
                    ret += "\"error\": " + Serialize(kalturaAPIExceptionWrapper.error);
                    break;
                    
                case "KalturaApiParameterPermissionItem":
                    KalturaApiParameterPermissionItem kalturaApiParameterPermissionItem = ottObject as KalturaApiParameterPermissionItem;
                    ret += "\"objectType\": " + "\"" + kalturaApiParameterPermissionItem.objectType + "\"";
                    if(kalturaApiParameterPermissionItem.relatedObjects != null && kalturaApiParameterPermissionItem.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaApiParameterPermissionItem.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaApiParameterPermissionItem.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaApiParameterPermissionItem.Id;
                    }
                    ret += ", \"isExcluded\": " + kalturaApiParameterPermissionItem.IsExcluded;
                    ret += ", \"name\": " + "\"" + kalturaApiParameterPermissionItem.Name + "\"";
                    ret += ", \"action\": " + kalturaApiParameterPermissionItem.Action;
                    ret += ", \"object\": " + "\"" + kalturaApiParameterPermissionItem.Object + "\"";
                    ret += ", \"parameter\": " + "\"" + kalturaApiParameterPermissionItem.Parameter + "\"";
                    break;
                    
                case "KalturaAppToken":
                    KalturaAppToken kalturaAppToken = ottObject as KalturaAppToken;
                    ret += "\"objectType\": " + "\"" + kalturaAppToken.objectType + "\"";
                    if(kalturaAppToken.relatedObjects != null && kalturaAppToken.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAppToken.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaAppToken.Expiry.HasValue)
                    {
                        ret += ", \"expiry\": " + kalturaAppToken.Expiry;
                    }
                    if(kalturaAppToken.HashType.HasValue)
                    {
                        ret += ", \"hashType\": " + kalturaAppToken.HashType;
                    }
                    ret += ", \"id\": " + "\"" + kalturaAppToken.Id + "\"";
                    if(kalturaAppToken.PartnerId.HasValue)
                    {
                        ret += ", \"partnerId\": " + kalturaAppToken.PartnerId;
                    }
                    if(kalturaAppToken.SessionDuration.HasValue)
                    {
                        ret += ", \"sessionDuration\": " + kalturaAppToken.SessionDuration;
                    }
                    ret += ", \"sessionPrivileges\": " + "\"" + kalturaAppToken.SessionPrivileges + "\"";
                    if(kalturaAppToken.SessionType.HasValue)
                    {
                        ret += ", \"sessionType\": " + kalturaAppToken.SessionType;
                    }
                    ret += ", \"sessionUserId\": " + "\"" + kalturaAppToken.SessionUserId + "\"";
                    ret += ", \"status\": " + kalturaAppToken.Status;
                    ret += ", \"token\": " + "\"" + kalturaAppToken.Token + "\"";
                    break;
                    
                case "KalturaAsset":
                    KalturaAsset kalturaAsset = ottObject as KalturaAsset;
                    ret += "\"objectType\": " + "\"" + kalturaAsset.objectType + "\"";
                    if(kalturaAsset.relatedObjects != null && kalturaAsset.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAsset.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"description\": " + Serialize(kalturaAsset.Description);
                    if(kalturaAsset.EnableCatchUp.HasValue)
                    {
                        ret += ", \"enableCatchUp\": " + kalturaAsset.EnableCatchUp;
                    }
                    if(kalturaAsset.EnableCdvr.HasValue)
                    {
                        ret += ", \"enableCdvr\": " + kalturaAsset.EnableCdvr;
                    }
                    if(kalturaAsset.EnableStartOver.HasValue)
                    {
                        ret += ", \"enableStartOver\": " + kalturaAsset.EnableStartOver;
                    }
                    if(kalturaAsset.EnableTrickPlay.HasValue)
                    {
                        ret += ", \"enableTrickPlay\": " + kalturaAsset.EnableTrickPlay;
                    }
                    if(kalturaAsset.EndDate.HasValue)
                    {
                        ret += ", \"endDate\": " + kalturaAsset.EndDate;
                    }
                    ret += ", \"externalId\": " + "\"" + kalturaAsset.ExternalId + "\"";
                    if(kalturaAsset.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaAsset.Id;
                    }
                    if(kalturaAsset.Images != null && kalturaAsset.Images.Count > 0)
                    {
                        ret += ", \"images\": " + "[" + String.Join(", ", kalturaAsset.Images.Select(item => Serialize(item))) + "]";
                    }
                    if(kalturaAsset.MediaFiles != null && kalturaAsset.MediaFiles.Count > 0)
                    {
                        ret += ", \"mediaFiles\": " + "[" + String.Join(", ", kalturaAsset.MediaFiles.Select(item => Serialize(item))) + "]";
                    }
                    if(kalturaAsset.Metas != null && kalturaAsset.Metas.Count > 0)
                    {
                        ret += ", \"metas\": " + "{" + String.Join(", ", kalturaAsset.Metas.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"name\": " + Serialize(kalturaAsset.Name);
                    if(kalturaAsset.StartDate.HasValue)
                    {
                        ret += ", \"startDate\": " + kalturaAsset.StartDate;
                    }
                    ret += ", \"stats\": " + Serialize(kalturaAsset.Statistics);
                    if(kalturaAsset.Tags != null && kalturaAsset.Tags.Count > 0)
                    {
                        ret += ", \"tags\": " + "{" + String.Join(", ", kalturaAsset.Tags.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaAsset.Type.HasValue)
                    {
                        ret += ", \"type\": " + kalturaAsset.Type;
                    }
                    break;
                    
                case "KalturaAssetBookmark":
                    KalturaAssetBookmark kalturaAssetBookmark = ottObject as KalturaAssetBookmark;
                    ret += "\"objectType\": " + "\"" + kalturaAssetBookmark.objectType + "\"";
                    if(kalturaAssetBookmark.relatedObjects != null && kalturaAssetBookmark.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAssetBookmark.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaAssetBookmark.IsFinishedWatching.HasValue)
                    {
                        ret += ", \"finishedWatching\": " + kalturaAssetBookmark.IsFinishedWatching;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"finished_watching\": " + kalturaAssetBookmark.IsFinishedWatching;
                        }
                    }
                    if(kalturaAssetBookmark.Position.HasValue)
                    {
                        ret += ", \"position\": " + kalturaAssetBookmark.Position;
                    }
                    ret += ", \"positionOwner\": " + kalturaAssetBookmark.PositionOwner;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"position_owner\": " + kalturaAssetBookmark.PositionOwner;
                    }
                    ret += ", \"user\": " + Serialize(kalturaAssetBookmark.User);
                    break;
                    
                case "KalturaAssetBookmarks":
                    KalturaAssetBookmarks kalturaAssetBookmarks = ottObject as KalturaAssetBookmarks;
                    ret += "\"objectType\": " + "\"" + kalturaAssetBookmarks.objectType + "\"";
                    if(kalturaAssetBookmarks.relatedObjects != null && kalturaAssetBookmarks.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAssetBookmarks.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"id\": " + "\"" + kalturaAssetBookmarks.Id + "\"";
                    ret += ", \"type\": " + kalturaAssetBookmarks.Type;
                    if(kalturaAssetBookmarks.Bookmarks != null && kalturaAssetBookmarks.Bookmarks.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaAssetBookmarks.Bookmarks.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaAssetComment":
                    KalturaAssetComment kalturaAssetComment = ottObject as KalturaAssetComment;
                    ret += "\"objectType\": " + "\"" + kalturaAssetComment.objectType + "\"";
                    if(kalturaAssetComment.relatedObjects != null && kalturaAssetComment.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAssetComment.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"createDate\": " + kalturaAssetComment.CreateDate;
                    ret += ", \"header\": " + "\"" + kalturaAssetComment.Header + "\"";
                    ret += ", \"text\": " + "\"" + kalturaAssetComment.Text + "\"";
                    ret += ", \"writer\": " + "\"" + kalturaAssetComment.Writer + "\"";
                    ret += ", \"assetId\": " + kalturaAssetComment.AssetId;
                    ret += ", \"assetType\": " + kalturaAssetComment.AssetType;
                    ret += ", \"id\": " + kalturaAssetComment.Id;
                    ret += ", \"subHeader\": " + "\"" + kalturaAssetComment.SubHeader + "\"";
                    break;
                    
                case "KalturaAssetCommentFilter":
                    KalturaAssetCommentFilter kalturaAssetCommentFilter = ottObject as KalturaAssetCommentFilter;
                    ret += "\"objectType\": " + "\"" + kalturaAssetCommentFilter.objectType + "\"";
                    if(kalturaAssetCommentFilter.relatedObjects != null && kalturaAssetCommentFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAssetCommentFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaAssetCommentFilter.OrderBy;
                    ret += ", \"assetIdEqual\": " + kalturaAssetCommentFilter.AssetIdEqual;
                    ret += ", \"assetTypeEqual\": " + kalturaAssetCommentFilter.AssetTypeEqual;
                    break;
                    
                case "KalturaAssetCommentListResponse":
                    KalturaAssetCommentListResponse kalturaAssetCommentListResponse = ottObject as KalturaAssetCommentListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaAssetCommentListResponse.objectType + "\"";
                    if(kalturaAssetCommentListResponse.relatedObjects != null && kalturaAssetCommentListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAssetCommentListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaAssetCommentListResponse.TotalCount;
                    if(kalturaAssetCommentListResponse.Objects != null && kalturaAssetCommentListResponse.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaAssetCommentListResponse.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaAssetCount":
                    KalturaAssetCount kalturaAssetCount = ottObject as KalturaAssetCount;
                    ret += "\"objectType\": " + "\"" + kalturaAssetCount.objectType + "\"";
                    if(kalturaAssetCount.relatedObjects != null && kalturaAssetCount.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAssetCount.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"count\": " + kalturaAssetCount.Count;
                    if(kalturaAssetCount.SubCounts != null && kalturaAssetCount.SubCounts.Count > 0)
                    {
                        ret += ", \"subs\": " + "[" + String.Join(", ", kalturaAssetCount.SubCounts.Select(item => Serialize(item))) + "]";
                    }
                    ret += ", \"value\": " + "\"" + kalturaAssetCount.Value + "\"";
                    break;
                    
                case "KalturaAssetCountListResponse":
                    KalturaAssetCountListResponse kalturaAssetCountListResponse = ottObject as KalturaAssetCountListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaAssetCountListResponse.objectType + "\"";
                    if(kalturaAssetCountListResponse.relatedObjects != null && kalturaAssetCountListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAssetCountListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaAssetCountListResponse.TotalCount;
                    ret += ", \"assetsCount\": " + kalturaAssetCountListResponse.AssetsCount;
                    if(kalturaAssetCountListResponse.Objects != null && kalturaAssetCountListResponse.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaAssetCountListResponse.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaAssetFieldGroupBy":
                    KalturaAssetFieldGroupBy kalturaAssetFieldGroupBy = ottObject as KalturaAssetFieldGroupBy;
                    ret += "\"objectType\": " + "\"" + kalturaAssetFieldGroupBy.objectType + "\"";
                    if(kalturaAssetFieldGroupBy.relatedObjects != null && kalturaAssetFieldGroupBy.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAssetFieldGroupBy.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"value\": " + kalturaAssetFieldGroupBy.Value;
                    break;
                    
                case "KalturaAssetFileContext":
                    KalturaAssetFileContext kalturaAssetFileContext = ottObject as KalturaAssetFileContext;
                    ret += "\"objectType\": " + "\"" + kalturaAssetFileContext.objectType + "\"";
                    if(kalturaAssetFileContext.relatedObjects != null && kalturaAssetFileContext.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAssetFileContext.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"fullLifeCycle\": " + "\"" + kalturaAssetFileContext.FullLifeCycle + "\"";
                    ret += ", \"isOfflinePlayBack\": " + kalturaAssetFileContext.IsOfflinePlayBack;
                    ret += ", \"viewLifeCycle\": " + "\"" + kalturaAssetFileContext.ViewLifeCycle + "\"";
                    break;
                    
                case "KalturaAssetFilter":
                    KalturaAssetFilter kalturaAssetFilter = ottObject as KalturaAssetFilter;
                    ret += "\"objectType\": " + "\"" + kalturaAssetFilter.objectType + "\"";
                    if(kalturaAssetFilter.relatedObjects != null && kalturaAssetFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAssetFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaAssetFilter.OrderBy;
                    ret += ", \"name\": " + "\"" + kalturaAssetFilter.Name + "\"";
                    ret += ", \"dynamicOrderBy\": " + Serialize(kalturaAssetFilter.DynamicOrderBy);
                    break;
                    
                case "KalturaAssetGroupBy":
                    KalturaAssetGroupBy kalturaAssetGroupBy = ottObject as KalturaAssetGroupBy;
                    ret += "\"objectType\": " + "\"" + kalturaAssetGroupBy.objectType + "\"";
                    if(kalturaAssetGroupBy.relatedObjects != null && kalturaAssetGroupBy.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAssetGroupBy.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    break;
                    
                case "KalturaAssetHistory":
                    KalturaAssetHistory kalturaAssetHistory = ottObject as KalturaAssetHistory;
                    ret += "\"objectType\": " + "\"" + kalturaAssetHistory.objectType + "\"";
                    if(kalturaAssetHistory.relatedObjects != null && kalturaAssetHistory.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAssetHistory.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"assetId\": " + kalturaAssetHistory.AssetId;
                    ret += ", \"assetType\": " + kalturaAssetHistory.AssetType;
                    if(kalturaAssetHistory.Duration.HasValue)
                    {
                        ret += ", \"duration\": " + kalturaAssetHistory.Duration;
                    }
                    if(kalturaAssetHistory.IsFinishedWatching.HasValue)
                    {
                        ret += ", \"finishedWatching\": " + kalturaAssetHistory.IsFinishedWatching;
                    }
                    if(kalturaAssetHistory.LastWatched.HasValue)
                    {
                        ret += ", \"watchedDate\": " + kalturaAssetHistory.LastWatched;
                    }
                    if(kalturaAssetHistory.Position.HasValue)
                    {
                        ret += ", \"position\": " + kalturaAssetHistory.Position;
                    }
                    break;
                    
                case "KalturaAssetHistoryFilter":
                    KalturaAssetHistoryFilter kalturaAssetHistoryFilter = ottObject as KalturaAssetHistoryFilter;
                    ret += "\"objectType\": " + "\"" + kalturaAssetHistoryFilter.objectType + "\"";
                    if(kalturaAssetHistoryFilter.relatedObjects != null && kalturaAssetHistoryFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAssetHistoryFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaAssetHistoryFilter.OrderBy;
                    ret += ", \"assetIdIn\": " + "\"" + kalturaAssetHistoryFilter.AssetIdIn + "\"";
                    if(kalturaAssetHistoryFilter.DaysLessThanOrEqual.HasValue)
                    {
                        ret += ", \"daysLessThanOrEqual\": " + kalturaAssetHistoryFilter.DaysLessThanOrEqual;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"days\": " + kalturaAssetHistoryFilter.DaysLessThanOrEqual;
                        }
                    }
                    if(kalturaAssetHistoryFilter.filterTypes != null && kalturaAssetHistoryFilter.filterTypes.Count > 0)
                    {
                        ret += ", \"filterTypes\": " + "[" + String.Join(", ", kalturaAssetHistoryFilter.filterTypes.Select(item => Serialize(item))) + "]";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"filter_types\": " + "[" + String.Join(", ", kalturaAssetHistoryFilter.filterTypes.Select(item => Serialize(item))) + "]";
                        }
                    }
                    if(kalturaAssetHistoryFilter.StatusEqual.HasValue)
                    {
                        ret += ", \"statusEqual\": " + kalturaAssetHistoryFilter.StatusEqual;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"filter_status\": " + kalturaAssetHistoryFilter.StatusEqual;
                        }
                    }
                    ret += ", \"typeIn\": " + "\"" + kalturaAssetHistoryFilter.TypeIn + "\"";
                    if(kalturaAssetHistoryFilter.with != null && kalturaAssetHistoryFilter.with.Count > 0)
                    {
                        ret += ", \"with\": " + "[" + String.Join(", ", kalturaAssetHistoryFilter.with.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaAssetHistoryListResponse":
                    KalturaAssetHistoryListResponse kalturaAssetHistoryListResponse = ottObject as KalturaAssetHistoryListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaAssetHistoryListResponse.objectType + "\"";
                    if(kalturaAssetHistoryListResponse.relatedObjects != null && kalturaAssetHistoryListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAssetHistoryListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaAssetHistoryListResponse.TotalCount;
                    if(kalturaAssetHistoryListResponse.Objects != null && kalturaAssetHistoryListResponse.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaAssetHistoryListResponse.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaAssetInfo":
                    KalturaAssetInfo kalturaAssetInfo = ottObject as KalturaAssetInfo;
                    ret += "\"objectType\": " + "\"" + kalturaAssetInfo.objectType + "\"";
                    if(kalturaAssetInfo.relatedObjects != null && kalturaAssetInfo.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAssetInfo.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"description\": " + "\"" + kalturaAssetInfo.Description + "\"";
                    if(kalturaAssetInfo.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaAssetInfo.Id;
                    }
                    if(kalturaAssetInfo.Images != null && kalturaAssetInfo.Images.Count > 0)
                    {
                        ret += ", \"images\": " + "[" + String.Join(", ", kalturaAssetInfo.Images.Select(item => Serialize(item))) + "]";
                    }
                    if(kalturaAssetInfo.MediaFiles != null && kalturaAssetInfo.MediaFiles.Count > 0)
                    {
                        ret += ", \"mediaFiles\": " + "[" + String.Join(", ", kalturaAssetInfo.MediaFiles.Select(item => Serialize(item))) + "]";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"media_files\": " + "[" + String.Join(", ", kalturaAssetInfo.MediaFiles.Select(item => Serialize(item))) + "]";
                        }
                    }
                    ret += ", \"name\": " + "\"" + kalturaAssetInfo.Name + "\"";
                    ret += ", \"stats\": " + Serialize(kalturaAssetInfo.Statistics);
                    if(kalturaAssetInfo.Type.HasValue)
                    {
                        ret += ", \"type\": " + kalturaAssetInfo.Type;
                    }
                    if(kalturaAssetInfo.EndDate.HasValue)
                    {
                        ret += ", \"endDate\": " + kalturaAssetInfo.EndDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"end_date\": " + kalturaAssetInfo.EndDate;
                        }
                    }
                    if(kalturaAssetInfo.ExtraParams != null && kalturaAssetInfo.ExtraParams.Count > 0)
                    {
                        ret += ", \"extraParams\": " + "{" + String.Join(", ", kalturaAssetInfo.ExtraParams.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"extra_params\": " + "{" + String.Join(", ", kalturaAssetInfo.ExtraParams.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                        }
                    }
                    if(kalturaAssetInfo.Metas != null && kalturaAssetInfo.Metas.Count > 0)
                    {
                        ret += ", \"metas\": " + "{" + String.Join(", ", kalturaAssetInfo.Metas.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaAssetInfo.StartDate.HasValue)
                    {
                        ret += ", \"startDate\": " + kalturaAssetInfo.StartDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"start_date\": " + kalturaAssetInfo.StartDate;
                        }
                    }
                    if(kalturaAssetInfo.Tags != null && kalturaAssetInfo.Tags.Count > 0)
                    {
                        ret += ", \"tags\": " + "{" + String.Join(", ", kalturaAssetInfo.Tags.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    break;
                    
                case "KalturaAssetInfoFilter":
                    KalturaAssetInfoFilter kalturaAssetInfoFilter = ottObject as KalturaAssetInfoFilter;
                    ret += "\"objectType\": " + "\"" + kalturaAssetInfoFilter.objectType + "\"";
                    if(kalturaAssetInfoFilter.relatedObjects != null && kalturaAssetInfoFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAssetInfoFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"cut_with\": " + kalturaAssetInfoFilter.cutWith;
                    if(kalturaAssetInfoFilter.FilterTags != null && kalturaAssetInfoFilter.FilterTags.Count > 0)
                    {
                        ret += ", \"filter_tags\": " + "{" + String.Join(", ", kalturaAssetInfoFilter.FilterTags.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaAssetInfoFilter.IDs != null && kalturaAssetInfoFilter.IDs.Count > 0)
                    {
                        ret += ", \"ids\": " + "[" + String.Join(", ", kalturaAssetInfoFilter.IDs.Select(item => Serialize(item))) + "]";
                    }
                    ret += ", \"referenceType\": " + kalturaAssetInfoFilter.ReferenceType;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"reference_type\": " + kalturaAssetInfoFilter.ReferenceType;
                    }
                    break;
                    
                case "KalturaAssetInfoListResponse":
                    KalturaAssetInfoListResponse kalturaAssetInfoListResponse = ottObject as KalturaAssetInfoListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaAssetInfoListResponse.objectType + "\"";
                    if(kalturaAssetInfoListResponse.relatedObjects != null && kalturaAssetInfoListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAssetInfoListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaAssetInfoListResponse.TotalCount;
                    if(kalturaAssetInfoListResponse.Objects != null && kalturaAssetInfoListResponse.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaAssetInfoListResponse.Objects.Select(item => Serialize(item))) + "]";
                    }
                    ret += ", \"requestId\": " + "\"" + kalturaAssetInfoListResponse.RequestId + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"request_id\": " + "\"" + kalturaAssetInfoListResponse.RequestId + "\"";
                    }
                    break;
                    
                case "KalturaAssetListResponse":
                    KalturaAssetListResponse kalturaAssetListResponse = ottObject as KalturaAssetListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaAssetListResponse.objectType + "\"";
                    if(kalturaAssetListResponse.relatedObjects != null && kalturaAssetListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAssetListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaAssetListResponse.TotalCount;
                    if(kalturaAssetListResponse.Objects != null && kalturaAssetListResponse.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaAssetListResponse.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaAssetMetaOrTagGroupBy":
                    KalturaAssetMetaOrTagGroupBy kalturaAssetMetaOrTagGroupBy = ottObject as KalturaAssetMetaOrTagGroupBy;
                    ret += "\"objectType\": " + "\"" + kalturaAssetMetaOrTagGroupBy.objectType + "\"";
                    if(kalturaAssetMetaOrTagGroupBy.relatedObjects != null && kalturaAssetMetaOrTagGroupBy.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAssetMetaOrTagGroupBy.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"value\": " + "\"" + kalturaAssetMetaOrTagGroupBy.Value + "\"";
                    break;
                    
                case "KalturaAssetPrice":
                    KalturaAssetPrice kalturaAssetPrice = ottObject as KalturaAssetPrice;
                    ret += "\"objectType\": " + "\"" + kalturaAssetPrice.objectType + "\"";
                    if(kalturaAssetPrice.relatedObjects != null && kalturaAssetPrice.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAssetPrice.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"asset_id\": " + "\"" + kalturaAssetPrice.AssetId + "\"";
                    ret += ", \"asset_type\": " + kalturaAssetPrice.AssetType;
                    if(kalturaAssetPrice.FilePrices != null && kalturaAssetPrice.FilePrices.Count > 0)
                    {
                        ret += ", \"file_prices\": " + "[" + String.Join(", ", kalturaAssetPrice.FilePrices.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaAssetReminder":
                    KalturaAssetReminder kalturaAssetReminder = ottObject as KalturaAssetReminder;
                    ret += "\"objectType\": " + "\"" + kalturaAssetReminder.objectType + "\"";
                    if(kalturaAssetReminder.relatedObjects != null && kalturaAssetReminder.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAssetReminder.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaAssetReminder.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaAssetReminder.Id;
                    }
                    ret += ", \"name\": " + "\"" + kalturaAssetReminder.Name + "\"";
                    ret += ", \"type\": " + kalturaAssetReminder.Type;
                    ret += ", \"assetId\": " + kalturaAssetReminder.AssetId;
                    break;
                    
                case "KalturaAssetReminderFilter":
                    KalturaAssetReminderFilter kalturaAssetReminderFilter = ottObject as KalturaAssetReminderFilter;
                    ret += "\"objectType\": " + "\"" + kalturaAssetReminderFilter.objectType + "\"";
                    if(kalturaAssetReminderFilter.relatedObjects != null && kalturaAssetReminderFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAssetReminderFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaAssetReminderFilter.OrderBy;
                    ret += ", \"kSql\": " + "\"" + kalturaAssetReminderFilter.KSql + "\"";
                    break;
                    
                case "KalturaAssetsBookmarksResponse":
                    KalturaAssetsBookmarksResponse kalturaAssetsBookmarksResponse = ottObject as KalturaAssetsBookmarksResponse;
                    ret += "\"objectType\": " + "\"" + kalturaAssetsBookmarksResponse.objectType + "\"";
                    if(kalturaAssetsBookmarksResponse.relatedObjects != null && kalturaAssetsBookmarksResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAssetsBookmarksResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaAssetsBookmarksResponse.TotalCount;
                    if(kalturaAssetsBookmarksResponse.AssetsBookmarks != null && kalturaAssetsBookmarksResponse.AssetsBookmarks.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaAssetsBookmarksResponse.AssetsBookmarks.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaAssetsCount":
                    KalturaAssetsCount kalturaAssetsCount = ottObject as KalturaAssetsCount;
                    ret += "\"objectType\": " + "\"" + kalturaAssetsCount.objectType + "\"";
                    if(kalturaAssetsCount.relatedObjects != null && kalturaAssetsCount.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAssetsCount.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"field\": " + "\"" + kalturaAssetsCount.Field + "\"";
                    if(kalturaAssetsCount.Objects != null && kalturaAssetsCount.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaAssetsCount.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaAssetsFilter":
                    KalturaAssetsFilter kalturaAssetsFilter = ottObject as KalturaAssetsFilter;
                    ret += "\"objectType\": " + "\"" + kalturaAssetsFilter.objectType + "\"";
                    if(kalturaAssetsFilter.relatedObjects != null && kalturaAssetsFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAssetsFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaAssetsFilter.Assets != null && kalturaAssetsFilter.Assets.Count > 0)
                    {
                        ret += ", \"assets\": " + "[" + String.Join(", ", kalturaAssetsFilter.Assets.Select(item => Serialize(item))) + "]";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"Assets\": " + "[" + String.Join(", ", kalturaAssetsFilter.Assets.Select(item => Serialize(item))) + "]";
                        }
                    }
                    break;
                    
                case "KalturaAssetStatistics":
                    KalturaAssetStatistics kalturaAssetStatistics = ottObject as KalturaAssetStatistics;
                    ret += "\"objectType\": " + "\"" + kalturaAssetStatistics.objectType + "\"";
                    if(kalturaAssetStatistics.relatedObjects != null && kalturaAssetStatistics.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAssetStatistics.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"assetId\": " + kalturaAssetStatistics.AssetId;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"asset_id\": " + kalturaAssetStatistics.AssetId;
                    }
                    ret += ", \"buzzScore\": " + Serialize(kalturaAssetStatistics.BuzzAvgScore);
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"buzz_score\": " + Serialize(kalturaAssetStatistics.BuzzAvgScore);
                    }
                    ret += ", \"likes\": " + kalturaAssetStatistics.Likes;
                    ret += ", \"rating\": " + kalturaAssetStatistics.Rating;
                    ret += ", \"ratingCount\": " + kalturaAssetStatistics.RatingCount;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"rating_count\": " + kalturaAssetStatistics.RatingCount;
                    }
                    ret += ", \"views\": " + kalturaAssetStatistics.Views;
                    break;
                    
                case "KalturaAssetStatisticsListResponse":
                    KalturaAssetStatisticsListResponse kalturaAssetStatisticsListResponse = ottObject as KalturaAssetStatisticsListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaAssetStatisticsListResponse.objectType + "\"";
                    if(kalturaAssetStatisticsListResponse.relatedObjects != null && kalturaAssetStatisticsListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAssetStatisticsListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaAssetStatisticsListResponse.TotalCount;
                    if(kalturaAssetStatisticsListResponse.AssetsStatistics != null && kalturaAssetStatisticsListResponse.AssetsStatistics.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaAssetStatisticsListResponse.AssetsStatistics.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaAssetStatisticsQuery":
                    KalturaAssetStatisticsQuery kalturaAssetStatisticsQuery = ottObject as KalturaAssetStatisticsQuery;
                    ret += "\"objectType\": " + "\"" + kalturaAssetStatisticsQuery.objectType + "\"";
                    if(kalturaAssetStatisticsQuery.relatedObjects != null && kalturaAssetStatisticsQuery.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaAssetStatisticsQuery.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"assetIdIn\": " + "\"" + kalturaAssetStatisticsQuery.AssetIdIn + "\"";
                    ret += ", \"assetTypeEqual\": " + kalturaAssetStatisticsQuery.AssetTypeEqual;
                    ret += ", \"endDateGreaterThanOrEqual\": " + kalturaAssetStatisticsQuery.EndDateGreaterThanOrEqual;
                    ret += ", \"startDateGreaterThanOrEqual\": " + kalturaAssetStatisticsQuery.StartDateGreaterThanOrEqual;
                    break;
                    
                case "KalturaBaseAssetInfo":
                    KalturaBaseAssetInfo kalturaBaseAssetInfo = ottObject as KalturaBaseAssetInfo;
                    ret += "\"objectType\": " + "\"" + kalturaBaseAssetInfo.objectType + "\"";
                    if(kalturaBaseAssetInfo.relatedObjects != null && kalturaBaseAssetInfo.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaBaseAssetInfo.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"description\": " + "\"" + kalturaBaseAssetInfo.Description + "\"";
                    if(kalturaBaseAssetInfo.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaBaseAssetInfo.Id;
                    }
                    if(kalturaBaseAssetInfo.Images != null && kalturaBaseAssetInfo.Images.Count > 0)
                    {
                        ret += ", \"images\": " + "[" + String.Join(", ", kalturaBaseAssetInfo.Images.Select(item => Serialize(item))) + "]";
                    }
                    if(kalturaBaseAssetInfo.MediaFiles != null && kalturaBaseAssetInfo.MediaFiles.Count > 0)
                    {
                        ret += ", \"mediaFiles\": " + "[" + String.Join(", ", kalturaBaseAssetInfo.MediaFiles.Select(item => Serialize(item))) + "]";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"media_files\": " + "[" + String.Join(", ", kalturaBaseAssetInfo.MediaFiles.Select(item => Serialize(item))) + "]";
                        }
                    }
                    ret += ", \"name\": " + "\"" + kalturaBaseAssetInfo.Name + "\"";
                    ret += ", \"stats\": " + Serialize(kalturaBaseAssetInfo.Statistics);
                    if(kalturaBaseAssetInfo.Type.HasValue)
                    {
                        ret += ", \"type\": " + kalturaBaseAssetInfo.Type;
                    }
                    break;
                    
                case "KalturaBaseChannel":
                    KalturaBaseChannel kalturaBaseChannel = ottObject as KalturaBaseChannel;
                    ret += "\"objectType\": " + "\"" + kalturaBaseChannel.objectType + "\"";
                    if(kalturaBaseChannel.relatedObjects != null && kalturaBaseChannel.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaBaseChannel.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaBaseChannel.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaBaseChannel.Id;
                    }
                    ret += ", \"name\": " + "\"" + kalturaBaseChannel.Name + "\"";
                    break;
                    
                case "KalturaBaseOTTUser":
                    KalturaBaseOTTUser kalturaBaseOTTUser = ottObject as KalturaBaseOTTUser;
                    ret += "\"objectType\": " + "\"" + kalturaBaseOTTUser.objectType + "\"";
                    if(kalturaBaseOTTUser.relatedObjects != null && kalturaBaseOTTUser.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaBaseOTTUser.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"firstName\": " + "\"" + kalturaBaseOTTUser.FirstName + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"first_name\": " + "\"" + kalturaBaseOTTUser.FirstName + "\"";
                    }
                    ret += ", \"id\": " + "\"" + kalturaBaseOTTUser.Id + "\"";
                    ret += ", \"lastName\": " + "\"" + kalturaBaseOTTUser.LastName + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"last_name\": " + "\"" + kalturaBaseOTTUser.LastName + "\"";
                    }
                    ret += ", \"username\": " + "\"" + kalturaBaseOTTUser.Username + "\"";
                    break;
                    
                case "KalturaBaseResponseProfile":
                    KalturaBaseResponseProfile kalturaBaseResponseProfile = ottObject as KalturaBaseResponseProfile;
                    ret += "\"objectType\": " + "\"" + kalturaBaseResponseProfile.objectType + "\"";
                    if(kalturaBaseResponseProfile.relatedObjects != null && kalturaBaseResponseProfile.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaBaseResponseProfile.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    break;
                    
                case "KalturaBaseSearchAssetFilter":
                    KalturaBaseSearchAssetFilter kalturaBaseSearchAssetFilter = ottObject as KalturaBaseSearchAssetFilter;
                    ret += "\"objectType\": " + "\"" + kalturaBaseSearchAssetFilter.objectType + "\"";
                    if(kalturaBaseSearchAssetFilter.relatedObjects != null && kalturaBaseSearchAssetFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaBaseSearchAssetFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaBaseSearchAssetFilter.OrderBy;
                    ret += ", \"name\": " + "\"" + kalturaBaseSearchAssetFilter.Name + "\"";
                    ret += ", \"dynamicOrderBy\": " + Serialize(kalturaBaseSearchAssetFilter.DynamicOrderBy);
                    if(kalturaBaseSearchAssetFilter.GroupBy != null && kalturaBaseSearchAssetFilter.GroupBy.Count > 0)
                    {
                        ret += ", \"groupBy\": " + "[" + String.Join(", ", kalturaBaseSearchAssetFilter.GroupBy.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaBillingPartnerConfig":
                    KalturaBillingPartnerConfig kalturaBillingPartnerConfig = ottObject as KalturaBillingPartnerConfig;
                    ret += "\"objectType\": " + "\"" + kalturaBillingPartnerConfig.objectType + "\"";
                    if(kalturaBillingPartnerConfig.relatedObjects != null && kalturaBillingPartnerConfig.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaBillingPartnerConfig.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"partnerConfigurationType\": " + Serialize(kalturaBillingPartnerConfig.PartnerConfigurationType);
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"partner_configuration_type\": " + Serialize(kalturaBillingPartnerConfig.PartnerConfigurationType);
                    }
                    ret += ", \"type\": " + kalturaBillingPartnerConfig.Type;
                    ret += ", \"value\": " + "\"" + kalturaBillingPartnerConfig.Value + "\"";
                    break;
                    
                case "KalturaBillingResponse":
                    KalturaBillingResponse kalturaBillingResponse = ottObject as KalturaBillingResponse;
                    ret += "\"objectType\": " + "\"" + kalturaBillingResponse.objectType + "\"";
                    if(kalturaBillingResponse.relatedObjects != null && kalturaBillingResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaBillingResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"externalReceiptCode\": " + "\"" + kalturaBillingResponse.ExternalReceiptCode + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"external_receipt_code\": " + "\"" + kalturaBillingResponse.ExternalReceiptCode + "\"";
                    }
                    ret += ", \"receiptCode\": " + "\"" + kalturaBillingResponse.ReceiptCode + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"receipt_code\": " + "\"" + kalturaBillingResponse.ReceiptCode + "\"";
                    }
                    break;
                    
                case "KalturaBillingTransaction":
                    KalturaBillingTransaction kalturaBillingTransaction = ottObject as KalturaBillingTransaction;
                    ret += "\"objectType\": " + "\"" + kalturaBillingTransaction.objectType + "\"";
                    if(kalturaBillingTransaction.relatedObjects != null && kalturaBillingTransaction.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaBillingTransaction.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaBillingTransaction.actionDate.HasValue)
                    {
                        ret += ", \"actionDate\": " + kalturaBillingTransaction.actionDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"action_date\": " + kalturaBillingTransaction.actionDate;
                        }
                    }
                    ret += ", \"billingAction\": " + kalturaBillingTransaction.billingAction;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"billing_action\": " + kalturaBillingTransaction.billingAction;
                    }
                    ret += ", \"billingPriceType\": " + kalturaBillingTransaction.billingPriceType;
                    if(kalturaBillingTransaction.billingProviderRef.HasValue)
                    {
                        ret += ", \"billingProviderRef\": " + kalturaBillingTransaction.billingProviderRef;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"billing_provider_ref\": " + kalturaBillingTransaction.billingProviderRef;
                        }
                    }
                    if(kalturaBillingTransaction.endDate.HasValue)
                    {
                        ret += ", \"endDate\": " + kalturaBillingTransaction.endDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"end_date\": " + kalturaBillingTransaction.endDate;
                        }
                    }
                    if(kalturaBillingTransaction.isRecurring.HasValue)
                    {
                        ret += ", \"isRecurring\": " + kalturaBillingTransaction.isRecurring;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_recurring\": " + kalturaBillingTransaction.isRecurring;
                        }
                    }
                    ret += ", \"itemType\": " + kalturaBillingTransaction.itemType;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"item_type\": " + kalturaBillingTransaction.itemType;
                    }
                    ret += ", \"paymentMethod\": " + kalturaBillingTransaction.paymentMethod;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"payment_method\": " + kalturaBillingTransaction.paymentMethod;
                    }
                    ret += ", \"paymentMethodExtraDetails\": " + "\"" + kalturaBillingTransaction.paymentMethodExtraDetails + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"payment_method_extra_details\": " + "\"" + kalturaBillingTransaction.paymentMethodExtraDetails + "\"";
                    }
                    ret += ", \"price\": " + Serialize(kalturaBillingTransaction.price);
                    ret += ", \"purchasedItemCode\": " + "\"" + kalturaBillingTransaction.purchasedItemCode + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"purchased_item_code\": " + "\"" + kalturaBillingTransaction.purchasedItemCode + "\"";
                    }
                    ret += ", \"purchasedItemName\": " + "\"" + kalturaBillingTransaction.purchasedItemName + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"purchased_item_name\": " + "\"" + kalturaBillingTransaction.purchasedItemName + "\"";
                    }
                    if(kalturaBillingTransaction.purchaseID.HasValue)
                    {
                        ret += ", \"purchaseId\": " + kalturaBillingTransaction.purchaseID;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"purchase_id\": " + kalturaBillingTransaction.purchaseID;
                        }
                    }
                    ret += ", \"recieptCode\": " + "\"" + kalturaBillingTransaction.recieptCode + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"reciept_code\": " + "\"" + kalturaBillingTransaction.recieptCode + "\"";
                    }
                    ret += ", \"remarks\": " + "\"" + kalturaBillingTransaction.remarks + "\"";
                    if(kalturaBillingTransaction.startDate.HasValue)
                    {
                        ret += ", \"startDate\": " + kalturaBillingTransaction.startDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"start_date\": " + kalturaBillingTransaction.startDate;
                        }
                    }
                    break;
                    
                case "KalturaBillingTransactionListResponse":
                    KalturaBillingTransactionListResponse kalturaBillingTransactionListResponse = ottObject as KalturaBillingTransactionListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaBillingTransactionListResponse.objectType + "\"";
                    if(kalturaBillingTransactionListResponse.relatedObjects != null && kalturaBillingTransactionListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaBillingTransactionListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaBillingTransactionListResponse.TotalCount;
                    if(kalturaBillingTransactionListResponse.transactions != null && kalturaBillingTransactionListResponse.transactions.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaBillingTransactionListResponse.transactions.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaBookmark":
                    KalturaBookmark kalturaBookmark = ottObject as KalturaBookmark;
                    ret += "\"objectType\": " + "\"" + kalturaBookmark.objectType + "\"";
                    if(kalturaBookmark.relatedObjects != null && kalturaBookmark.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaBookmark.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"id\": " + "\"" + kalturaBookmark.Id + "\"";
                    ret += ", \"type\": " + kalturaBookmark.Type;
                    if(kalturaBookmark.IsFinishedWatching.HasValue)
                    {
                        ret += ", \"finishedWatching\": " + kalturaBookmark.IsFinishedWatching;
                    }
                    ret += ", \"playerData\": " + Serialize(kalturaBookmark.PlayerData);
                    if(kalturaBookmark.Position.HasValue)
                    {
                        ret += ", \"position\": " + kalturaBookmark.Position;
                    }
                    ret += ", \"positionOwner\": " + kalturaBookmark.PositionOwner;
                    ret += ", \"user\": " + Serialize(kalturaBookmark.User);
                    ret += ", \"userId\": " + "\"" + kalturaBookmark.UserId + "\"";
                    break;
                    
                case "KalturaBookmarkFilter":
                    KalturaBookmarkFilter kalturaBookmarkFilter = ottObject as KalturaBookmarkFilter;
                    ret += "\"objectType\": " + "\"" + kalturaBookmarkFilter.objectType + "\"";
                    if(kalturaBookmarkFilter.relatedObjects != null && kalturaBookmarkFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaBookmarkFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaBookmarkFilter.OrderBy;
                    ret += ", \"assetIdIn\": " + "\"" + kalturaBookmarkFilter.AssetIdIn + "\"";
                    if(kalturaBookmarkFilter.AssetIn != null && kalturaBookmarkFilter.AssetIn.Count > 0)
                    {
                        ret += ", \"assetIn\": " + "[" + String.Join(", ", kalturaBookmarkFilter.AssetIn.Select(item => Serialize(item))) + "]";
                    }
                    if(kalturaBookmarkFilter.AssetTypeEqual.HasValue)
                    {
                        ret += ", \"assetTypeEqual\": " + kalturaBookmarkFilter.AssetTypeEqual;
                    }
                    break;
                    
                case "KalturaBookmarkListResponse":
                    KalturaBookmarkListResponse kalturaBookmarkListResponse = ottObject as KalturaBookmarkListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaBookmarkListResponse.objectType + "\"";
                    if(kalturaBookmarkListResponse.relatedObjects != null && kalturaBookmarkListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaBookmarkListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaBookmarkListResponse.TotalCount;
                    if(kalturaBookmarkListResponse.AssetsBookmarks != null && kalturaBookmarkListResponse.AssetsBookmarks.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaBookmarkListResponse.AssetsBookmarks.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaBookmarkPlayerData":
                    KalturaBookmarkPlayerData kalturaBookmarkPlayerData = ottObject as KalturaBookmarkPlayerData;
                    ret += "\"objectType\": " + "\"" + kalturaBookmarkPlayerData.objectType + "\"";
                    if(kalturaBookmarkPlayerData.relatedObjects != null && kalturaBookmarkPlayerData.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaBookmarkPlayerData.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"action\": " + kalturaBookmarkPlayerData.action;
                    if(kalturaBookmarkPlayerData.averageBitRate.HasValue)
                    {
                        ret += ", \"averageBitrate\": " + kalturaBookmarkPlayerData.averageBitRate;
                    }
                    if(kalturaBookmarkPlayerData.currentBitRate.HasValue)
                    {
                        ret += ", \"currentBitrate\": " + kalturaBookmarkPlayerData.currentBitRate;
                    }
                    if(kalturaBookmarkPlayerData.FileId.HasValue)
                    {
                        ret += ", \"fileId\": " + kalturaBookmarkPlayerData.FileId;
                    }
                    if(kalturaBookmarkPlayerData.totalBitRate.HasValue)
                    {
                        ret += ", \"totalBitrate\": " + kalturaBookmarkPlayerData.totalBitRate;
                    }
                    break;
                    
                case "KalturaBooleanValue":
                    KalturaBooleanValue kalturaBooleanValue = ottObject as KalturaBooleanValue;
                    ret += "\"objectType\": " + "\"" + kalturaBooleanValue.objectType + "\"";
                    if(kalturaBooleanValue.relatedObjects != null && kalturaBooleanValue.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaBooleanValue.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"description\": " + "\"" + kalturaBooleanValue.description + "\"";
                    ret += ", \"value\": " + kalturaBooleanValue.value;
                    break;
                    
                case "KalturaBundleFilter":
                    KalturaBundleFilter kalturaBundleFilter = ottObject as KalturaBundleFilter;
                    ret += "\"objectType\": " + "\"" + kalturaBundleFilter.objectType + "\"";
                    if(kalturaBundleFilter.relatedObjects != null && kalturaBundleFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaBundleFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaBundleFilter.OrderBy;
                    ret += ", \"name\": " + "\"" + kalturaBundleFilter.Name + "\"";
                    ret += ", \"dynamicOrderBy\": " + Serialize(kalturaBundleFilter.DynamicOrderBy);
                    ret += ", \"bundleTypeEqual\": " + kalturaBundleFilter.BundleTypeEqual;
                    ret += ", \"idEqual\": " + kalturaBundleFilter.IdEqual;
                    ret += ", \"typeIn\": " + "\"" + kalturaBundleFilter.TypeIn + "\"";
                    break;
                    
                case "KalturaBuzzScore":
                    KalturaBuzzScore kalturaBuzzScore = ottObject as KalturaBuzzScore;
                    ret += "\"objectType\": " + "\"" + kalturaBuzzScore.objectType + "\"";
                    if(kalturaBuzzScore.relatedObjects != null && kalturaBuzzScore.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaBuzzScore.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaBuzzScore.AvgScore.HasValue)
                    {
                        ret += ", \"avgScore\": " + kalturaBuzzScore.AvgScore;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"avg_score\": " + kalturaBuzzScore.AvgScore;
                        }
                    }
                    if(kalturaBuzzScore.NormalizedAvgScore.HasValue)
                    {
                        ret += ", \"normalizedAvgScore\": " + kalturaBuzzScore.NormalizedAvgScore;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"normalized_avg_score\": " + kalturaBuzzScore.NormalizedAvgScore;
                        }
                    }
                    if(kalturaBuzzScore.UpdateDate.HasValue)
                    {
                        ret += ", \"updateDate\": " + kalturaBuzzScore.UpdateDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"update_date\": " + kalturaBuzzScore.UpdateDate;
                        }
                    }
                    break;
                    
                case "KalturaCatalogWithHolder":
                    KalturaCatalogWithHolder kalturaCatalogWithHolder = ottObject as KalturaCatalogWithHolder;
                    ret += "\"objectType\": " + "\"" + kalturaCatalogWithHolder.objectType + "\"";
                    if(kalturaCatalogWithHolder.relatedObjects != null && kalturaCatalogWithHolder.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaCatalogWithHolder.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"type\": " + kalturaCatalogWithHolder.type;
                    break;
                    
                case "KalturaCDNAdapterProfile":
                    KalturaCDNAdapterProfile kalturaCDNAdapterProfile = ottObject as KalturaCDNAdapterProfile;
                    ret += "\"objectType\": " + "\"" + kalturaCDNAdapterProfile.objectType + "\"";
                    if(kalturaCDNAdapterProfile.relatedObjects != null && kalturaCDNAdapterProfile.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaCDNAdapterProfile.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"adapterUrl\": " + "\"" + kalturaCDNAdapterProfile.AdapterUrl + "\"";
                    ret += ", \"baseUrl\": " + "\"" + kalturaCDNAdapterProfile.BaseUrl + "\"";
                    if(kalturaCDNAdapterProfile.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaCDNAdapterProfile.Id;
                    }
                    if(kalturaCDNAdapterProfile.IsActive.HasValue)
                    {
                        ret += ", \"isActive\": " + kalturaCDNAdapterProfile.IsActive;
                    }
                    ret += ", \"name\": " + "\"" + kalturaCDNAdapterProfile.Name + "\"";
                    if(kalturaCDNAdapterProfile.Settings != null && kalturaCDNAdapterProfile.Settings.Count > 0)
                    {
                        ret += ", \"settings\": " + "{" + String.Join(", ", kalturaCDNAdapterProfile.Settings.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"sharedSecret\": " + "\"" + kalturaCDNAdapterProfile.SharedSecret + "\"";
                    ret += ", \"systemName\": " + "\"" + kalturaCDNAdapterProfile.SystemName + "\"";
                    break;
                    
                case "KalturaCDNAdapterProfileListResponse":
                    KalturaCDNAdapterProfileListResponse kalturaCDNAdapterProfileListResponse = ottObject as KalturaCDNAdapterProfileListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaCDNAdapterProfileListResponse.objectType + "\"";
                    if(kalturaCDNAdapterProfileListResponse.relatedObjects != null && kalturaCDNAdapterProfileListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaCDNAdapterProfileListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaCDNAdapterProfileListResponse.TotalCount;
                    if(kalturaCDNAdapterProfileListResponse.Adapters != null && kalturaCDNAdapterProfileListResponse.Adapters.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaCDNAdapterProfileListResponse.Adapters.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaCDNPartnerSettings":
                    KalturaCDNPartnerSettings kalturaCDNPartnerSettings = ottObject as KalturaCDNPartnerSettings;
                    ret += "\"objectType\": " + "\"" + kalturaCDNPartnerSettings.objectType + "\"";
                    if(kalturaCDNPartnerSettings.relatedObjects != null && kalturaCDNPartnerSettings.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaCDNPartnerSettings.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaCDNPartnerSettings.DefaultAdapterId.HasValue)
                    {
                        ret += ", \"defaultAdapterId\": " + kalturaCDNPartnerSettings.DefaultAdapterId;
                    }
                    if(kalturaCDNPartnerSettings.DefaultRecordingAdapterId.HasValue)
                    {
                        ret += ", \"defaultRecordingAdapterId\": " + kalturaCDNPartnerSettings.DefaultRecordingAdapterId;
                    }
                    break;
                    
                case "KalturaCDVRAdapterProfile":
                    KalturaCDVRAdapterProfile kalturaCDVRAdapterProfile = ottObject as KalturaCDVRAdapterProfile;
                    ret += "\"objectType\": " + "\"" + kalturaCDVRAdapterProfile.objectType + "\"";
                    if(kalturaCDVRAdapterProfile.relatedObjects != null && kalturaCDVRAdapterProfile.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaCDVRAdapterProfile.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"adapterUrl\": " + "\"" + kalturaCDVRAdapterProfile.AdapterUrl + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"adapter_url\": " + "\"" + kalturaCDVRAdapterProfile.AdapterUrl + "\"";
                    }
                    if(kalturaCDVRAdapterProfile.DynamicLinksSupport.HasValue)
                    {
                        ret += ", \"dynamicLinksSupport\": " + kalturaCDVRAdapterProfile.DynamicLinksSupport;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"dynamic_links_support\": " + kalturaCDVRAdapterProfile.DynamicLinksSupport;
                        }
                    }
                    ret += ", \"externalIdentifier\": " + "\"" + kalturaCDVRAdapterProfile.ExternalIdentifier + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"external_identifier\": " + "\"" + kalturaCDVRAdapterProfile.ExternalIdentifier + "\"";
                    }
                    if(kalturaCDVRAdapterProfile.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaCDVRAdapterProfile.Id;
                    }
                    if(kalturaCDVRAdapterProfile.IsActive.HasValue)
                    {
                        ret += ", \"isActive\": " + kalturaCDVRAdapterProfile.IsActive;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_active\": " + kalturaCDVRAdapterProfile.IsActive;
                        }
                    }
                    ret += ", \"name\": " + "\"" + kalturaCDVRAdapterProfile.Name + "\"";
                    if(kalturaCDVRAdapterProfile.Settings != null && kalturaCDVRAdapterProfile.Settings.Count > 0)
                    {
                        ret += ", \"settings\": " + "{" + String.Join(", ", kalturaCDVRAdapterProfile.Settings.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"sharedSecret\": " + "\"" + kalturaCDVRAdapterProfile.SharedSecret + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"shared_secret\": " + "\"" + kalturaCDVRAdapterProfile.SharedSecret + "\"";
                    }
                    break;
                    
                case "KalturaCDVRAdapterProfileListResponse":
                    KalturaCDVRAdapterProfileListResponse kalturaCDVRAdapterProfileListResponse = ottObject as KalturaCDVRAdapterProfileListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaCDVRAdapterProfileListResponse.objectType + "\"";
                    if(kalturaCDVRAdapterProfileListResponse.relatedObjects != null && kalturaCDVRAdapterProfileListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaCDVRAdapterProfileListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaCDVRAdapterProfileListResponse.TotalCount;
                    if(kalturaCDVRAdapterProfileListResponse.Objects != null && kalturaCDVRAdapterProfileListResponse.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaCDVRAdapterProfileListResponse.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaChannel":
                    KalturaChannel kalturaChannel = ottObject as KalturaChannel;
                    ret += "\"objectType\": " + "\"" + kalturaChannel.objectType + "\"";
                    if(kalturaChannel.relatedObjects != null && kalturaChannel.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaChannel.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaChannel.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaChannel.Id;
                    }
                    ret += ", \"name\": " + "\"" + kalturaChannel.Name + "\"";
                    if(kalturaChannel.AssetTypes != null && kalturaChannel.AssetTypes.Count > 0)
                    {
                        ret += ", \"assetTypes\": " + "[" + String.Join(", ", kalturaChannel.AssetTypes.Select(item => Serialize(item))) + "]";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"asset_types\": " + "[" + String.Join(", ", kalturaChannel.AssetTypes.Select(item => Serialize(item))) + "]";
                        }
                    }
                    ret += ", \"description\": " + "\"" + kalturaChannel.Description + "\"";
                    ret += ", \"filterExpression\": " + "\"" + kalturaChannel.FilterExpression + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"filter_expression\": " + "\"" + kalturaChannel.FilterExpression + "\"";
                    }
                    ret += ", \"groupBy\": " + Serialize(kalturaChannel.GroupBy);
                    if(kalturaChannel.Images != null && kalturaChannel.Images.Count > 0)
                    {
                        ret += ", \"images\": " + "[" + String.Join(", ", kalturaChannel.Images.Select(item => Serialize(item))) + "]";
                    }
                    if(kalturaChannel.IsActive.HasValue)
                    {
                        ret += ", \"isActive\": " + kalturaChannel.IsActive;
                    }
                    if(kalturaChannel.MediaTypes != null && kalturaChannel.MediaTypes.Count > 0)
                    {
                        ret += ", \"media_types\": " + "[" + String.Join(", ", kalturaChannel.MediaTypes.Select(item => Serialize(item))) + "]";
                    }
                    ret += ", \"order\": " + kalturaChannel.Order;
                    break;
                    
                case "KalturaChannelEnrichmentHolder":
                    KalturaChannelEnrichmentHolder kalturaChannelEnrichmentHolder = ottObject as KalturaChannelEnrichmentHolder;
                    ret += "\"objectType\": " + "\"" + kalturaChannelEnrichmentHolder.objectType + "\"";
                    if(kalturaChannelEnrichmentHolder.relatedObjects != null && kalturaChannelEnrichmentHolder.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaChannelEnrichmentHolder.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"type\": " + kalturaChannelEnrichmentHolder.type;
                    break;
                    
                case "KalturaChannelExternalFilter":
                    KalturaChannelExternalFilter kalturaChannelExternalFilter = ottObject as KalturaChannelExternalFilter;
                    ret += "\"objectType\": " + "\"" + kalturaChannelExternalFilter.objectType + "\"";
                    if(kalturaChannelExternalFilter.relatedObjects != null && kalturaChannelExternalFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaChannelExternalFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaChannelExternalFilter.OrderBy;
                    ret += ", \"name\": " + "\"" + kalturaChannelExternalFilter.Name + "\"";
                    ret += ", \"dynamicOrderBy\": " + Serialize(kalturaChannelExternalFilter.DynamicOrderBy);
                    ret += ", \"freeText\": " + "\"" + kalturaChannelExternalFilter.FreeText + "\"";
                    ret += ", \"idEqual\": " + kalturaChannelExternalFilter.IdEqual;
                    ret += ", \"utcOffsetEqual\": " + kalturaChannelExternalFilter.UtcOffsetEqual;
                    break;
                    
                case "KalturaChannelFilter":
                    KalturaChannelFilter kalturaChannelFilter = ottObject as KalturaChannelFilter;
                    ret += "\"objectType\": " + "\"" + kalturaChannelFilter.objectType + "\"";
                    if(kalturaChannelFilter.relatedObjects != null && kalturaChannelFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaChannelFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaChannelFilter.OrderBy;
                    ret += ", \"name\": " + "\"" + kalturaChannelFilter.Name + "\"";
                    ret += ", \"dynamicOrderBy\": " + Serialize(kalturaChannelFilter.DynamicOrderBy);
                    ret += ", \"idEqual\": " + kalturaChannelFilter.IdEqual;
                    ret += ", \"kSql\": " + "\"" + kalturaChannelFilter.KSql + "\"";
                    ret += ", \"orderBy\": " + kalturaChannelFilter.OrderBy;
                    break;
                    
                case "KalturaChannelProfile":
                    KalturaChannelProfile kalturaChannelProfile = ottObject as KalturaChannelProfile;
                    ret += "\"objectType\": " + "\"" + kalturaChannelProfile.objectType + "\"";
                    if(kalturaChannelProfile.relatedObjects != null && kalturaChannelProfile.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaChannelProfile.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaChannelProfile.AssetTypes != null && kalturaChannelProfile.AssetTypes.Count > 0)
                    {
                        ret += ", \"assetTypes\": " + "[" + String.Join(", ", kalturaChannelProfile.AssetTypes.Select(item => item.ToString())) + "]";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"asset_types\": " + "[" + String.Join(", ", kalturaChannelProfile.AssetTypes.Select(item => item.ToString())) + "]";
                        }
                    }
                    ret += ", \"description\": " + "\"" + kalturaChannelProfile.Description + "\"";
                    ret += ", \"filterExpression\": " + "\"" + kalturaChannelProfile.FilterExpression + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"filter_expression\": " + "\"" + kalturaChannelProfile.FilterExpression + "\"";
                    }
                    if(kalturaChannelProfile.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaChannelProfile.Id;
                    }
                    if(kalturaChannelProfile.IsActive.HasValue)
                    {
                        ret += ", \"isActive\": " + kalturaChannelProfile.IsActive;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_active\": " + kalturaChannelProfile.IsActive;
                        }
                    }
                    ret += ", \"name\": " + "\"" + kalturaChannelProfile.Name + "\"";
                    ret += ", \"order\": " + kalturaChannelProfile.Order;
                    break;
                    
                case "KalturaClientConfiguration":
                    KalturaClientConfiguration kalturaClientConfiguration = ottObject as KalturaClientConfiguration;
                    ret += "\"objectType\": " + "\"" + kalturaClientConfiguration.objectType + "\"";
                    if(kalturaClientConfiguration.relatedObjects != null && kalturaClientConfiguration.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaClientConfiguration.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"apiVersion\": " + "\"" + kalturaClientConfiguration.ApiVersion + "\"";
                    ret += ", \"clientTag\": " + "\"" + kalturaClientConfiguration.ClientTag + "\"";
                    break;
                    
                case "KalturaCollection":
                    KalturaCollection kalturaCollection = ottObject as KalturaCollection;
                    ret += "\"objectType\": " + "\"" + kalturaCollection.objectType + "\"";
                    if(kalturaCollection.relatedObjects != null && kalturaCollection.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaCollection.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaCollection.Channels != null && kalturaCollection.Channels.Count > 0)
                    {
                        ret += ", \"channels\": " + "[" + String.Join(", ", kalturaCollection.Channels.Select(item => Serialize(item))) + "]";
                    }
                    if(kalturaCollection.CouponGroups != null && kalturaCollection.CouponGroups.Count > 0)
                    {
                        ret += ", \"couponsGroups\": " + "[" + String.Join(", ", kalturaCollection.CouponGroups.Select(item => Serialize(item))) + "]";
                    }
                    ret += ", \"description\": " + Serialize(kalturaCollection.Description);
                    ret += ", \"discountModule\": " + Serialize(kalturaCollection.DiscountModule);
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"discount_module\": " + Serialize(kalturaCollection.DiscountModule);
                    }
                    if(kalturaCollection.EndDate.HasValue)
                    {
                        ret += ", \"endDate\": " + kalturaCollection.EndDate;
                    }
                    ret += ", \"externalId\": " + "\"" + kalturaCollection.ExternalId + "\"";
                    ret += ", \"id\": " + "\"" + kalturaCollection.Id + "\"";
                    ret += ", \"name\": " + Serialize(kalturaCollection.Name);
                    if(kalturaCollection.PriceDetailsId.HasValue)
                    {
                        ret += ", \"priceDetailsId\": " + kalturaCollection.PriceDetailsId;
                    }
                    if(kalturaCollection.ProductCodes != null && kalturaCollection.ProductCodes.Count > 0)
                    {
                        ret += ", \"productCodes\": " + "[" + String.Join(", ", kalturaCollection.ProductCodes.Select(item => Serialize(item))) + "]";
                    }
                    if(kalturaCollection.StartDate.HasValue)
                    {
                        ret += ", \"startDate\": " + kalturaCollection.StartDate;
                    }
                    ret += ", \"usageModule\": " + Serialize(kalturaCollection.UsageModule);
                    break;
                    
                case "KalturaCollectionEntitlement":
                    KalturaCollectionEntitlement kalturaCollectionEntitlement = ottObject as KalturaCollectionEntitlement;
                    ret += "\"objectType\": " + "\"" + kalturaCollectionEntitlement.objectType + "\"";
                    if(kalturaCollectionEntitlement.relatedObjects != null && kalturaCollectionEntitlement.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaCollectionEntitlement.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaCollectionEntitlement.CurrentDate.HasValue)
                    {
                        ret += ", \"currentDate\": " + kalturaCollectionEntitlement.CurrentDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"current_date\": " + kalturaCollectionEntitlement.CurrentDate;
                        }
                    }
                    if(kalturaCollectionEntitlement.CurrentUses.HasValue)
                    {
                        ret += ", \"currentUses\": " + kalturaCollectionEntitlement.CurrentUses;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"current_uses\": " + kalturaCollectionEntitlement.CurrentUses;
                        }
                    }
                    ret += ", \"deviceName\": " + "\"" + kalturaCollectionEntitlement.DeviceName + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"device_name\": " + "\"" + kalturaCollectionEntitlement.DeviceName + "\"";
                    }
                    ret += ", \"deviceUdid\": " + "\"" + kalturaCollectionEntitlement.DeviceUDID + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"device_udid\": " + "\"" + kalturaCollectionEntitlement.DeviceUDID + "\"";
                    }
                    if(kalturaCollectionEntitlement.EndDate.HasValue)
                    {
                        ret += ", \"endDate\": " + kalturaCollectionEntitlement.EndDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"end_date\": " + kalturaCollectionEntitlement.EndDate;
                        }
                    }
                    ret += ", \"entitlementId\": " + "\"" + kalturaCollectionEntitlement.EntitlementId + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"entitlement_id\": " + "\"" + kalturaCollectionEntitlement.EntitlementId + "\"";
                    }
                    ret += ", \"householdId\": " + kalturaCollectionEntitlement.HouseholdId;
                    if(kalturaCollectionEntitlement.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaCollectionEntitlement.Id;
                    }
                    if(kalturaCollectionEntitlement.IsCancelationWindowEnabled.HasValue)
                    {
                        ret += ", \"isCancelationWindowEnabled\": " + kalturaCollectionEntitlement.IsCancelationWindowEnabled;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_cancelation_window_enabled\": " + kalturaCollectionEntitlement.IsCancelationWindowEnabled;
                        }
                    }
                    if(kalturaCollectionEntitlement.IsInGracePeriod.HasValue)
                    {
                        ret += ", \"isInGracePeriod\": " + kalturaCollectionEntitlement.IsInGracePeriod;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_in_grace_period\": " + kalturaCollectionEntitlement.IsInGracePeriod;
                        }
                    }
                    if(kalturaCollectionEntitlement.IsRenewable.HasValue)
                    {
                        ret += ", \"isRenewable\": " + kalturaCollectionEntitlement.IsRenewable;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_renewable\": " + kalturaCollectionEntitlement.IsRenewable;
                        }
                    }
                    if(kalturaCollectionEntitlement.IsRenewableForPurchase.HasValue)
                    {
                        ret += ", \"isRenewableForPurchase\": " + kalturaCollectionEntitlement.IsRenewableForPurchase;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_renewable_for_purchase\": " + kalturaCollectionEntitlement.IsRenewableForPurchase;
                        }
                    }
                    if(kalturaCollectionEntitlement.LastViewDate.HasValue)
                    {
                        ret += ", \"lastViewDate\": " + kalturaCollectionEntitlement.LastViewDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"last_view_date\": " + kalturaCollectionEntitlement.LastViewDate;
                        }
                    }
                    if(kalturaCollectionEntitlement.MaxUses.HasValue)
                    {
                        ret += ", \"maxUses\": " + kalturaCollectionEntitlement.MaxUses;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"max_uses\": " + kalturaCollectionEntitlement.MaxUses;
                        }
                    }
                    if(kalturaCollectionEntitlement.MediaFileId.HasValue)
                    {
                        ret += ", \"mediaFileId\": " + kalturaCollectionEntitlement.MediaFileId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"media_file_id\": " + kalturaCollectionEntitlement.MediaFileId;
                        }
                    }
                    if(kalturaCollectionEntitlement.MediaId.HasValue)
                    {
                        ret += ", \"mediaId\": " + kalturaCollectionEntitlement.MediaId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"media_id\": " + kalturaCollectionEntitlement.MediaId;
                        }
                    }
                    if(kalturaCollectionEntitlement.NextRenewalDate.HasValue)
                    {
                        ret += ", \"nextRenewalDate\": " + kalturaCollectionEntitlement.NextRenewalDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"next_renewal_date\": " + kalturaCollectionEntitlement.NextRenewalDate;
                        }
                    }
                    ret += ", \"paymentMethod\": " + kalturaCollectionEntitlement.PaymentMethod;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"payment_method\": " + kalturaCollectionEntitlement.PaymentMethod;
                    }
                    ret += ", \"productId\": " + "\"" + kalturaCollectionEntitlement.ProductId + "\"";
                    if(kalturaCollectionEntitlement.PurchaseDate.HasValue)
                    {
                        ret += ", \"purchaseDate\": " + kalturaCollectionEntitlement.PurchaseDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"purchase_date\": " + kalturaCollectionEntitlement.PurchaseDate;
                        }
                    }
                    if(kalturaCollectionEntitlement.PurchaseId.HasValue)
                    {
                        ret += ", \"purchaseId\": " + kalturaCollectionEntitlement.PurchaseId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"purchase_id\": " + kalturaCollectionEntitlement.PurchaseId;
                        }
                    }
                    ret += ", \"type\": " + kalturaCollectionEntitlement.Type;
                    ret += ", \"userId\": " + "\"" + kalturaCollectionEntitlement.UserId + "\"";
                    break;
                    
                case "KalturaCollectionFilter":
                    KalturaCollectionFilter kalturaCollectionFilter = ottObject as KalturaCollectionFilter;
                    ret += "\"objectType\": " + "\"" + kalturaCollectionFilter.objectType + "\"";
                    if(kalturaCollectionFilter.relatedObjects != null && kalturaCollectionFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaCollectionFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaCollectionFilter.OrderBy;
                    ret += ", \"collectionIdIn\": " + "\"" + kalturaCollectionFilter.CollectionIdIn + "\"";
                    if(kalturaCollectionFilter.MediaFileIdEqual.HasValue)
                    {
                        ret += ", \"mediaFileIdEqual\": " + kalturaCollectionFilter.MediaFileIdEqual;
                    }
                    break;
                    
                case "KalturaCollectionListResponse":
                    KalturaCollectionListResponse kalturaCollectionListResponse = ottObject as KalturaCollectionListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaCollectionListResponse.objectType + "\"";
                    if(kalturaCollectionListResponse.relatedObjects != null && kalturaCollectionListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaCollectionListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaCollectionListResponse.TotalCount;
                    if(kalturaCollectionListResponse.Collections != null && kalturaCollectionListResponse.Collections.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaCollectionListResponse.Collections.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaCollectionPrice":
                    KalturaCollectionPrice kalturaCollectionPrice = ottObject as KalturaCollectionPrice;
                    ret += "\"objectType\": " + "\"" + kalturaCollectionPrice.objectType + "\"";
                    if(kalturaCollectionPrice.relatedObjects != null && kalturaCollectionPrice.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaCollectionPrice.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"price\": " + Serialize(kalturaCollectionPrice.Price);
                    ret += ", \"productId\": " + "\"" + kalturaCollectionPrice.ProductId + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"product_id\": " + "\"" + kalturaCollectionPrice.ProductId + "\"";
                    }
                    ret += ", \"productType\": " + kalturaCollectionPrice.ProductType;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"product_type\": " + kalturaCollectionPrice.ProductType;
                    }
                    ret += ", \"purchaseStatus\": " + kalturaCollectionPrice.PurchaseStatus;
                    break;
                    
                case "KalturaCompensation":
                    KalturaCompensation kalturaCompensation = ottObject as KalturaCompensation;
                    ret += "\"objectType\": " + "\"" + kalturaCompensation.objectType + "\"";
                    if(kalturaCompensation.relatedObjects != null && kalturaCompensation.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaCompensation.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"amount\": " + kalturaCompensation.Amount;
                    ret += ", \"appliedRenewalIterations\": " + kalturaCompensation.AppliedRenewalIterations;
                    ret += ", \"compensationType\": " + kalturaCompensation.CompensationType;
                    ret += ", \"id\": " + kalturaCompensation.Id;
                    ret += ", \"purchaseId\": " + kalturaCompensation.PurchaseId;
                    ret += ", \"subscriptionId\": " + kalturaCompensation.SubscriptionId;
                    ret += ", \"totalRenewalIterations\": " + kalturaCompensation.TotalRenewalIterations;
                    break;
                    
                case "KalturaConfigurationGroup":
                    KalturaConfigurationGroup kalturaConfigurationGroup = ottObject as KalturaConfigurationGroup;
                    ret += "\"objectType\": " + "\"" + kalturaConfigurationGroup.objectType + "\"";
                    if(kalturaConfigurationGroup.relatedObjects != null && kalturaConfigurationGroup.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaConfigurationGroup.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaConfigurationGroup.ConfigurationIdentifiers != null && kalturaConfigurationGroup.ConfigurationIdentifiers.Count > 0)
                    {
                        ret += ", \"configurationIdentifiers\": " + "[" + String.Join(", ", kalturaConfigurationGroup.ConfigurationIdentifiers.Select(item => Serialize(item))) + "]";
                    }
                    ret += ", \"id\": " + "\"" + kalturaConfigurationGroup.Id + "\"";
                    ret += ", \"isDefault\": " + kalturaConfigurationGroup.IsDefault;
                    ret += ", \"name\": " + "\"" + kalturaConfigurationGroup.Name + "\"";
                    ret += ", \"numberOfDevices\": " + kalturaConfigurationGroup.NumberOfDevices;
                    ret += ", \"partnerId\": " + kalturaConfigurationGroup.PartnerId;
                    if(kalturaConfigurationGroup.Tags != null && kalturaConfigurationGroup.Tags.Count > 0)
                    {
                        ret += ", \"tags\": " + "[" + String.Join(", ", kalturaConfigurationGroup.Tags.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaConfigurationGroupDevice":
                    KalturaConfigurationGroupDevice kalturaConfigurationGroupDevice = ottObject as KalturaConfigurationGroupDevice;
                    ret += "\"objectType\": " + "\"" + kalturaConfigurationGroupDevice.objectType + "\"";
                    if(kalturaConfigurationGroupDevice.relatedObjects != null && kalturaConfigurationGroupDevice.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaConfigurationGroupDevice.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"configurationGroupId\": " + "\"" + kalturaConfigurationGroupDevice.ConfigurationGroupId + "\"";
                    ret += ", \"partnerId\": " + kalturaConfigurationGroupDevice.PartnerId;
                    ret += ", \"udid\": " + "\"" + kalturaConfigurationGroupDevice.Udid + "\"";
                    break;
                    
                case "KalturaConfigurationGroupDeviceFilter":
                    KalturaConfigurationGroupDeviceFilter kalturaConfigurationGroupDeviceFilter = ottObject as KalturaConfigurationGroupDeviceFilter;
                    ret += "\"objectType\": " + "\"" + kalturaConfigurationGroupDeviceFilter.objectType + "\"";
                    if(kalturaConfigurationGroupDeviceFilter.relatedObjects != null && kalturaConfigurationGroupDeviceFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaConfigurationGroupDeviceFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaConfigurationGroupDeviceFilter.OrderBy;
                    ret += ", \"configurationGroupIdEqual\": " + "\"" + kalturaConfigurationGroupDeviceFilter.ConfigurationGroupIdEqual + "\"";
                    break;
                    
                case "KalturaConfigurationGroupDeviceListResponse":
                    KalturaConfigurationGroupDeviceListResponse kalturaConfigurationGroupDeviceListResponse = ottObject as KalturaConfigurationGroupDeviceListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaConfigurationGroupDeviceListResponse.objectType + "\"";
                    if(kalturaConfigurationGroupDeviceListResponse.relatedObjects != null && kalturaConfigurationGroupDeviceListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaConfigurationGroupDeviceListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaConfigurationGroupDeviceListResponse.TotalCount;
                    if(kalturaConfigurationGroupDeviceListResponse.Objects != null && kalturaConfigurationGroupDeviceListResponse.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaConfigurationGroupDeviceListResponse.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaConfigurationGroupListResponse":
                    KalturaConfigurationGroupListResponse kalturaConfigurationGroupListResponse = ottObject as KalturaConfigurationGroupListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaConfigurationGroupListResponse.objectType + "\"";
                    if(kalturaConfigurationGroupListResponse.relatedObjects != null && kalturaConfigurationGroupListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaConfigurationGroupListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaConfigurationGroupListResponse.TotalCount;
                    if(kalturaConfigurationGroupListResponse.Objects != null && kalturaConfigurationGroupListResponse.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaConfigurationGroupListResponse.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaConfigurationGroupTag":
                    KalturaConfigurationGroupTag kalturaConfigurationGroupTag = ottObject as KalturaConfigurationGroupTag;
                    ret += "\"objectType\": " + "\"" + kalturaConfigurationGroupTag.objectType + "\"";
                    if(kalturaConfigurationGroupTag.relatedObjects != null && kalturaConfigurationGroupTag.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaConfigurationGroupTag.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"configurationGroupId\": " + "\"" + kalturaConfigurationGroupTag.ConfigurationGroupId + "\"";
                    ret += ", \"partnerId\": " + kalturaConfigurationGroupTag.PartnerId;
                    ret += ", \"tag\": " + "\"" + kalturaConfigurationGroupTag.Tag + "\"";
                    break;
                    
                case "KalturaConfigurationGroupTagFilter":
                    KalturaConfigurationGroupTagFilter kalturaConfigurationGroupTagFilter = ottObject as KalturaConfigurationGroupTagFilter;
                    ret += "\"objectType\": " + "\"" + kalturaConfigurationGroupTagFilter.objectType + "\"";
                    if(kalturaConfigurationGroupTagFilter.relatedObjects != null && kalturaConfigurationGroupTagFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaConfigurationGroupTagFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaConfigurationGroupTagFilter.OrderBy;
                    ret += ", \"configurationGroupIdEqual\": " + "\"" + kalturaConfigurationGroupTagFilter.ConfigurationGroupIdEqual + "\"";
                    break;
                    
                case "KalturaConfigurationGroupTagListResponse":
                    KalturaConfigurationGroupTagListResponse kalturaConfigurationGroupTagListResponse = ottObject as KalturaConfigurationGroupTagListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaConfigurationGroupTagListResponse.objectType + "\"";
                    if(kalturaConfigurationGroupTagListResponse.relatedObjects != null && kalturaConfigurationGroupTagListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaConfigurationGroupTagListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaConfigurationGroupTagListResponse.TotalCount;
                    if(kalturaConfigurationGroupTagListResponse.Objects != null && kalturaConfigurationGroupTagListResponse.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaConfigurationGroupTagListResponse.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaConfigurationIdentifier":
                    KalturaConfigurationIdentifier kalturaConfigurationIdentifier = ottObject as KalturaConfigurationIdentifier;
                    ret += "\"objectType\": " + "\"" + kalturaConfigurationIdentifier.objectType + "\"";
                    if(kalturaConfigurationIdentifier.relatedObjects != null && kalturaConfigurationIdentifier.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaConfigurationIdentifier.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"id\": " + "\"" + kalturaConfigurationIdentifier.Id + "\"";
                    ret += ", \"name\": " + "\"" + kalturaConfigurationIdentifier.Name + "\"";
                    break;
                    
                case "KalturaConfigurations":
                    KalturaConfigurations kalturaConfigurations = ottObject as KalturaConfigurations;
                    ret += "\"objectType\": " + "\"" + kalturaConfigurations.objectType + "\"";
                    if(kalturaConfigurations.relatedObjects != null && kalturaConfigurations.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaConfigurations.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"appName\": " + "\"" + kalturaConfigurations.AppName + "\"";
                    ret += ", \"clientVersion\": " + "\"" + kalturaConfigurations.ClientVersion + "\"";
                    ret += ", \"configurationGroupId\": " + "\"" + kalturaConfigurations.ConfigurationGroupId + "\"";
                    ret += ", \"content\": " + "\"" + kalturaConfigurations.Content + "\"";
                    ret += ", \"externalPushId\": " + "\"" + kalturaConfigurations.ExternalPushId + "\"";
                    ret += ", \"id\": " + "\"" + kalturaConfigurations.Id + "\"";
                    ret += ", \"isForceUpdate\": " + kalturaConfigurations.IsForceUpdate;
                    ret += ", \"partnerId\": " + kalturaConfigurations.PartnerId;
                    ret += ", \"platform\": " + kalturaConfigurations.Platform;
                    break;
                    
                case "KalturaConfigurationsFilter":
                    KalturaConfigurationsFilter kalturaConfigurationsFilter = ottObject as KalturaConfigurationsFilter;
                    ret += "\"objectType\": " + "\"" + kalturaConfigurationsFilter.objectType + "\"";
                    if(kalturaConfigurationsFilter.relatedObjects != null && kalturaConfigurationsFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaConfigurationsFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaConfigurationsFilter.OrderBy;
                    ret += ", \"configurationGroupIdEqual\": " + "\"" + kalturaConfigurationsFilter.ConfigurationGroupIdEqual + "\"";
                    break;
                    
                case "KalturaConfigurationsListResponse":
                    KalturaConfigurationsListResponse kalturaConfigurationsListResponse = ottObject as KalturaConfigurationsListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaConfigurationsListResponse.objectType + "\"";
                    if(kalturaConfigurationsListResponse.relatedObjects != null && kalturaConfigurationsListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaConfigurationsListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaConfigurationsListResponse.TotalCount;
                    if(kalturaConfigurationsListResponse.Objects != null && kalturaConfigurationsListResponse.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaConfigurationsListResponse.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaCountry":
                    KalturaCountry kalturaCountry = ottObject as KalturaCountry;
                    ret += "\"objectType\": " + "\"" + kalturaCountry.objectType + "\"";
                    if(kalturaCountry.relatedObjects != null && kalturaCountry.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaCountry.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"code\": " + "\"" + kalturaCountry.Code + "\"";
                    ret += ", \"currency\": " + "\"" + kalturaCountry.CurrencyCode + "\"";
                    ret += ", \"currencySign\": " + "\"" + kalturaCountry.CurrencySign + "\"";
                    ret += ", \"id\": " + kalturaCountry.Id;
                    ret += ", \"languagesCode\": " + "\"" + kalturaCountry.LanguagesCode + "\"";
                    ret += ", \"mainLanguageCode\": " + "\"" + kalturaCountry.MainLanguageCode + "\"";
                    ret += ", \"name\": " + "\"" + kalturaCountry.Name + "\"";
                    if(kalturaCountry.VatPercent.HasValue)
                    {
                        ret += ", \"vatPercent\": " + kalturaCountry.VatPercent;
                    }
                    break;
                    
                case "KalturaCountryFilter":
                    KalturaCountryFilter kalturaCountryFilter = ottObject as KalturaCountryFilter;
                    ret += "\"objectType\": " + "\"" + kalturaCountryFilter.objectType + "\"";
                    if(kalturaCountryFilter.relatedObjects != null && kalturaCountryFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaCountryFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaCountryFilter.OrderBy;
                    ret += ", \"idIn\": " + "\"" + kalturaCountryFilter.IdIn + "\"";
                    ret += ", \"ipEqual\": " + "\"" + kalturaCountryFilter.IpEqual + "\"";
                    if(kalturaCountryFilter.IpEqualCurrent.HasValue)
                    {
                        ret += ", \"ipEqualCurrent\": " + kalturaCountryFilter.IpEqualCurrent;
                    }
                    break;
                    
                case "KalturaCountryListResponse":
                    KalturaCountryListResponse kalturaCountryListResponse = ottObject as KalturaCountryListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaCountryListResponse.objectType + "\"";
                    if(kalturaCountryListResponse.relatedObjects != null && kalturaCountryListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaCountryListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaCountryListResponse.TotalCount;
                    if(kalturaCountryListResponse.Objects != null && kalturaCountryListResponse.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaCountryListResponse.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaCoupon":
                    KalturaCoupon kalturaCoupon = ottObject as KalturaCoupon;
                    ret += "\"objectType\": " + "\"" + kalturaCoupon.objectType + "\"";
                    if(kalturaCoupon.relatedObjects != null && kalturaCoupon.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaCoupon.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"couponsGroup\": " + Serialize(kalturaCoupon.CouponsGroup);
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"coupons_group\": " + Serialize(kalturaCoupon.CouponsGroup);
                    }
                    ret += ", \"status\": " + kalturaCoupon.Status;
                    break;
                    
                case "KalturaCouponsGroup":
                    KalturaCouponsGroup kalturaCouponsGroup = ottObject as KalturaCouponsGroup;
                    ret += "\"objectType\": " + "\"" + kalturaCouponsGroup.objectType + "\"";
                    if(kalturaCouponsGroup.relatedObjects != null && kalturaCouponsGroup.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaCouponsGroup.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaCouponsGroup.CouponGroupType.HasValue)
                    {
                        ret += ", \"couponGroupType\": " + kalturaCouponsGroup.CouponGroupType;
                    }
                    if(kalturaCouponsGroup.Descriptions != null && kalturaCouponsGroup.Descriptions.Count > 0)
                    {
                        ret += ", \"descriptions\": " + "[" + String.Join(", ", kalturaCouponsGroup.Descriptions.Select(item => Serialize(item))) + "]";
                    }
                    if(kalturaCouponsGroup.EndDate.HasValue)
                    {
                        ret += ", \"endDate\": " + kalturaCouponsGroup.EndDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"end_date\": " + kalturaCouponsGroup.EndDate;
                        }
                    }
                    ret += ", \"id\": " + "\"" + kalturaCouponsGroup.Id + "\"";
                    if(kalturaCouponsGroup.MaxUsesNumber.HasValue)
                    {
                        ret += ", \"maxUsesNumber\": " + kalturaCouponsGroup.MaxUsesNumber;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"max_uses_number\": " + kalturaCouponsGroup.MaxUsesNumber;
                        }
                    }
                    if(kalturaCouponsGroup.MaxUsesNumberOnRenewableSub.HasValue)
                    {
                        ret += ", \"maxUsesNumberOnRenewableSub\": " + kalturaCouponsGroup.MaxUsesNumberOnRenewableSub;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"max_uses_number_on_renewable_sub\": " + kalturaCouponsGroup.MaxUsesNumberOnRenewableSub;
                        }
                    }
                    ret += ", \"name\": " + "\"" + kalturaCouponsGroup.Name + "\"";
                    if(kalturaCouponsGroup.StartDate.HasValue)
                    {
                        ret += ", \"startDate\": " + kalturaCouponsGroup.StartDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"start_date\": " + kalturaCouponsGroup.StartDate;
                        }
                    }
                    break;
                    
                case "KalturaCurrency":
                    KalturaCurrency kalturaCurrency = ottObject as KalturaCurrency;
                    ret += "\"objectType\": " + "\"" + kalturaCurrency.objectType + "\"";
                    if(kalturaCurrency.relatedObjects != null && kalturaCurrency.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaCurrency.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"code\": " + "\"" + kalturaCurrency.Code + "\"";
                    ret += ", \"isDefault\": " + kalturaCurrency.IsDefault;
                    ret += ", \"name\": " + "\"" + kalturaCurrency.Name + "\"";
                    ret += ", \"sign\": " + "\"" + kalturaCurrency.Sign + "\"";
                    break;
                    
                case "KalturaCurrencyFilter":
                    KalturaCurrencyFilter kalturaCurrencyFilter = ottObject as KalturaCurrencyFilter;
                    ret += "\"objectType\": " + "\"" + kalturaCurrencyFilter.objectType + "\"";
                    if(kalturaCurrencyFilter.relatedObjects != null && kalturaCurrencyFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaCurrencyFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaCurrencyFilter.OrderBy;
                    ret += ", \"codeIn\": " + "\"" + kalturaCurrencyFilter.CodeIn + "\"";
                    break;
                    
                case "KalturaCurrencyListResponse":
                    KalturaCurrencyListResponse kalturaCurrencyListResponse = ottObject as KalturaCurrencyListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaCurrencyListResponse.objectType + "\"";
                    if(kalturaCurrencyListResponse.relatedObjects != null && kalturaCurrencyListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaCurrencyListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaCurrencyListResponse.TotalCount;
                    if(kalturaCurrencyListResponse.Objects != null && kalturaCurrencyListResponse.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaCurrencyListResponse.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaCustomDrmPlaybackPluginData":
                    KalturaCustomDrmPlaybackPluginData kalturaCustomDrmPlaybackPluginData = ottObject as KalturaCustomDrmPlaybackPluginData;
                    ret += "\"objectType\": " + "\"" + kalturaCustomDrmPlaybackPluginData.objectType + "\"";
                    if(kalturaCustomDrmPlaybackPluginData.relatedObjects != null && kalturaCustomDrmPlaybackPluginData.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaCustomDrmPlaybackPluginData.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"licenseURL\": " + "\"" + kalturaCustomDrmPlaybackPluginData.LicenseURL + "\"";
                    ret += ", \"scheme\": " + kalturaCustomDrmPlaybackPluginData.Scheme;
                    ret += ", \"data\": " + "\"" + kalturaCustomDrmPlaybackPluginData.Data + "\"";
                    break;
                    
                case "KalturaDetachedResponseProfile":
                    KalturaDetachedResponseProfile kalturaDetachedResponseProfile = ottObject as KalturaDetachedResponseProfile;
                    ret += "\"objectType\": " + "\"" + kalturaDetachedResponseProfile.objectType + "\"";
                    if(kalturaDetachedResponseProfile.relatedObjects != null && kalturaDetachedResponseProfile.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaDetachedResponseProfile.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"filter\": " + Serialize(kalturaDetachedResponseProfile.Filter);
                    ret += ", \"name\": " + "\"" + kalturaDetachedResponseProfile.Name + "\"";
                    if(kalturaDetachedResponseProfile.RelatedProfiles != null && kalturaDetachedResponseProfile.RelatedProfiles.Count > 0)
                    {
                        ret += ", \"relatedProfiles\": " + "[" + String.Join(", ", kalturaDetachedResponseProfile.RelatedProfiles.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaDevice":
                    KalturaDevice kalturaDevice = ottObject as KalturaDevice;
                    ret += "\"objectType\": " + "\"" + kalturaDevice.objectType + "\"";
                    if(kalturaDevice.relatedObjects != null && kalturaDevice.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaDevice.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaDevice.ActivatedOn.HasValue)
                    {
                        ret += ", \"activatedOn\": " + kalturaDevice.ActivatedOn;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"activated_on\": " + kalturaDevice.ActivatedOn;
                        }
                    }
                    ret += ", \"brand\": " + "\"" + kalturaDevice.Brand + "\"";
                    if(kalturaDevice.BrandId.HasValue)
                    {
                        ret += ", \"brandId\": " + kalturaDevice.BrandId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"brand_id\": " + kalturaDevice.BrandId;
                        }
                    }
                    if(kalturaDevice.DeviceFamilyId.HasValue)
                    {
                        ret += ", \"deviceFamilyId\": " + kalturaDevice.DeviceFamilyId;
                    }
                    ret += ", \"drm\": " + Serialize(kalturaDevice.Drm);
                    ret += ", \"householdId\": " + kalturaDevice.HouseholdId;
                    ret += ", \"name\": " + "\"" + kalturaDevice.Name + "\"";
                    if(kalturaDevice.State.HasValue)
                    {
                        ret += ", \"state\": " + kalturaDevice.State;
                    }
                    if(kalturaDevice.Status.HasValue)
                    {
                        ret += ", \"status\": " + kalturaDevice.Status;
                    }
                    ret += ", \"udid\": " + "\"" + kalturaDevice.Udid + "\"";
                    break;
                    
                case "KalturaDeviceBrand":
                    KalturaDeviceBrand kalturaDeviceBrand = ottObject as KalturaDeviceBrand;
                    ret += "\"objectType\": " + "\"" + kalturaDeviceBrand.objectType + "\"";
                    if(kalturaDeviceBrand.relatedObjects != null && kalturaDeviceBrand.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaDeviceBrand.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaDeviceBrand.DeviceFamilyId.HasValue)
                    {
                        ret += ", \"deviceFamilyid\": " + kalturaDeviceBrand.DeviceFamilyId;
                    }
                    if(kalturaDeviceBrand.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaDeviceBrand.Id;
                    }
                    ret += ", \"name\": " + "\"" + kalturaDeviceBrand.Name + "\"";
                    break;
                    
                case "KalturaDeviceBrandListResponse":
                    KalturaDeviceBrandListResponse kalturaDeviceBrandListResponse = ottObject as KalturaDeviceBrandListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaDeviceBrandListResponse.objectType + "\"";
                    if(kalturaDeviceBrandListResponse.relatedObjects != null && kalturaDeviceBrandListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaDeviceBrandListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaDeviceBrandListResponse.TotalCount;
                    if(kalturaDeviceBrandListResponse.Objects != null && kalturaDeviceBrandListResponse.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaDeviceBrandListResponse.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaDeviceFamily":
                    KalturaDeviceFamily kalturaDeviceFamily = ottObject as KalturaDeviceFamily;
                    ret += "\"objectType\": " + "\"" + kalturaDeviceFamily.objectType + "\"";
                    if(kalturaDeviceFamily.relatedObjects != null && kalturaDeviceFamily.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaDeviceFamily.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaDeviceFamily.ConcurrentLimit.HasValue)
                    {
                        ret += ", \"concurrentLimit\": " + kalturaDeviceFamily.ConcurrentLimit;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"concurrent_limit\": " + kalturaDeviceFamily.ConcurrentLimit;
                        }
                    }
                    if(kalturaDeviceFamily.DeviceLimit.HasValue)
                    {
                        ret += ", \"deviceLimit\": " + kalturaDeviceFamily.DeviceLimit;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"device_limit\": " + kalturaDeviceFamily.DeviceLimit;
                        }
                    }
                    if(kalturaDeviceFamily.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaDeviceFamily.Id;
                    }
                    ret += ", \"name\": " + "\"" + kalturaDeviceFamily.Name + "\"";
                    if(kalturaDeviceFamily.Devices != null && kalturaDeviceFamily.Devices.Count > 0)
                    {
                        ret += ", \"devices\": " + "[" + String.Join(", ", kalturaDeviceFamily.Devices.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaDeviceFamilyBase":
                    KalturaDeviceFamilyBase kalturaDeviceFamilyBase = ottObject as KalturaDeviceFamilyBase;
                    ret += "\"objectType\": " + "\"" + kalturaDeviceFamilyBase.objectType + "\"";
                    if(kalturaDeviceFamilyBase.relatedObjects != null && kalturaDeviceFamilyBase.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaDeviceFamilyBase.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaDeviceFamilyBase.ConcurrentLimit.HasValue)
                    {
                        ret += ", \"concurrentLimit\": " + kalturaDeviceFamilyBase.ConcurrentLimit;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"concurrent_limit\": " + kalturaDeviceFamilyBase.ConcurrentLimit;
                        }
                    }
                    if(kalturaDeviceFamilyBase.DeviceLimit.HasValue)
                    {
                        ret += ", \"deviceLimit\": " + kalturaDeviceFamilyBase.DeviceLimit;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"device_limit\": " + kalturaDeviceFamilyBase.DeviceLimit;
                        }
                    }
                    if(kalturaDeviceFamilyBase.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaDeviceFamilyBase.Id;
                    }
                    ret += ", \"name\": " + "\"" + kalturaDeviceFamilyBase.Name + "\"";
                    break;
                    
                case "KalturaDeviceFamilyListResponse":
                    KalturaDeviceFamilyListResponse kalturaDeviceFamilyListResponse = ottObject as KalturaDeviceFamilyListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaDeviceFamilyListResponse.objectType + "\"";
                    if(kalturaDeviceFamilyListResponse.relatedObjects != null && kalturaDeviceFamilyListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaDeviceFamilyListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaDeviceFamilyListResponse.TotalCount;
                    if(kalturaDeviceFamilyListResponse.Objects != null && kalturaDeviceFamilyListResponse.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaDeviceFamilyListResponse.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaDevicePin":
                    KalturaDevicePin kalturaDevicePin = ottObject as KalturaDevicePin;
                    ret += "\"objectType\": " + "\"" + kalturaDevicePin.objectType + "\"";
                    if(kalturaDevicePin.relatedObjects != null && kalturaDevicePin.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaDevicePin.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"pin\": " + "\"" + kalturaDevicePin.Pin + "\"";
                    break;
                    
                case "KalturaDeviceRegistrationStatusHolder":
                    KalturaDeviceRegistrationStatusHolder kalturaDeviceRegistrationStatusHolder = ottObject as KalturaDeviceRegistrationStatusHolder;
                    ret += "\"objectType\": " + "\"" + kalturaDeviceRegistrationStatusHolder.objectType + "\"";
                    if(kalturaDeviceRegistrationStatusHolder.relatedObjects != null && kalturaDeviceRegistrationStatusHolder.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaDeviceRegistrationStatusHolder.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"status\": " + kalturaDeviceRegistrationStatusHolder.Status;
                    break;
                    
                case "KalturaDeviceReport":
                    KalturaDeviceReport kalturaDeviceReport = ottObject as KalturaDeviceReport;
                    ret += "\"objectType\": " + "\"" + kalturaDeviceReport.objectType + "\"";
                    if(kalturaDeviceReport.relatedObjects != null && kalturaDeviceReport.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaDeviceReport.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"configurationGroupId\": " + "\"" + kalturaDeviceReport.ConfigurationGroupId + "\"";
                    ret += ", \"lastAccessDate\": " + kalturaDeviceReport.LastAccessDate;
                    ret += ", \"lastAccessIP\": " + "\"" + kalturaDeviceReport.LastAccessIP + "\"";
                    ret += ", \"operationSystem\": " + "\"" + kalturaDeviceReport.OperationSystem + "\"";
                    ret += ", \"partnerId\": " + kalturaDeviceReport.PartnerId;
                    ret += ", \"pushParameters\": " + Serialize(kalturaDeviceReport.PushParameters);
                    ret += ", \"udid\": " + "\"" + kalturaDeviceReport.Udid + "\"";
                    ret += ", \"userAgent\": " + "\"" + kalturaDeviceReport.UserAgent + "\"";
                    ret += ", \"versionAppName\": " + "\"" + kalturaDeviceReport.VersionAppName + "\"";
                    ret += ", \"versionNumber\": " + "\"" + kalturaDeviceReport.VersionNumber + "\"";
                    ret += ", \"versionPlatform\": " + kalturaDeviceReport.VersionPlatform;
                    break;
                    
                case "KalturaDeviceReportFilter":
                    KalturaDeviceReportFilter kalturaDeviceReportFilter = ottObject as KalturaDeviceReportFilter;
                    ret += "\"objectType\": " + "\"" + kalturaDeviceReportFilter.objectType + "\"";
                    if(kalturaDeviceReportFilter.relatedObjects != null && kalturaDeviceReportFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaDeviceReportFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaDeviceReportFilter.OrderBy;
                    ret += ", \"lastAccessDateGreaterThanOrEqual\": " + kalturaDeviceReportFilter.LastAccessDateGreaterThanOrEqual;
                    break;
                    
                case "KalturaDiscountModule":
                    KalturaDiscountModule kalturaDiscountModule = ottObject as KalturaDiscountModule;
                    ret += "\"objectType\": " + "\"" + kalturaDiscountModule.objectType + "\"";
                    if(kalturaDiscountModule.relatedObjects != null && kalturaDiscountModule.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaDiscountModule.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaDiscountModule.EndDate.HasValue)
                    {
                        ret += ", \"endDate\": " + kalturaDiscountModule.EndDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"end_date\": " + kalturaDiscountModule.EndDate;
                        }
                    }
                    if(kalturaDiscountModule.Percent.HasValue)
                    {
                        ret += ", \"percent\": " + kalturaDiscountModule.Percent;
                    }
                    if(kalturaDiscountModule.StartDate.HasValue)
                    {
                        ret += ", \"startDate\": " + kalturaDiscountModule.StartDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"start_date\": " + kalturaDiscountModule.StartDate;
                        }
                    }
                    break;
                    
                case "KalturaDoubleValue":
                    KalturaDoubleValue kalturaDoubleValue = ottObject as KalturaDoubleValue;
                    ret += "\"objectType\": " + "\"" + kalturaDoubleValue.objectType + "\"";
                    if(kalturaDoubleValue.relatedObjects != null && kalturaDoubleValue.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaDoubleValue.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"description\": " + "\"" + kalturaDoubleValue.description + "\"";
                    ret += ", \"value\": " + kalturaDoubleValue.value;
                    break;
                    
                case "KalturaDrmPlaybackPluginData":
                    KalturaDrmPlaybackPluginData kalturaDrmPlaybackPluginData = ottObject as KalturaDrmPlaybackPluginData;
                    ret += "\"objectType\": " + "\"" + kalturaDrmPlaybackPluginData.objectType + "\"";
                    if(kalturaDrmPlaybackPluginData.relatedObjects != null && kalturaDrmPlaybackPluginData.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaDrmPlaybackPluginData.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"licenseURL\": " + "\"" + kalturaDrmPlaybackPluginData.LicenseURL + "\"";
                    ret += ", \"scheme\": " + kalturaDrmPlaybackPluginData.Scheme;
                    break;
                    
                case "KalturaDynamicOrderBy":
                    KalturaDynamicOrderBy kalturaDynamicOrderBy = ottObject as KalturaDynamicOrderBy;
                    ret += "\"objectType\": " + "\"" + kalturaDynamicOrderBy.objectType + "\"";
                    if(kalturaDynamicOrderBy.relatedObjects != null && kalturaDynamicOrderBy.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaDynamicOrderBy.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"name\": " + "\"" + kalturaDynamicOrderBy.Name + "\"";
                    if(kalturaDynamicOrderBy.OrderBy.HasValue)
                    {
                        ret += ", \"orderBy\": " + kalturaDynamicOrderBy.OrderBy;
                    }
                    break;
                    
                case "KalturaEmailMessage":
                    KalturaEmailMessage kalturaEmailMessage = ottObject as KalturaEmailMessage;
                    ret += "\"objectType\": " + "\"" + kalturaEmailMessage.objectType + "\"";
                    if(kalturaEmailMessage.relatedObjects != null && kalturaEmailMessage.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaEmailMessage.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"bccAddress\": " + "\"" + kalturaEmailMessage.BccAddress + "\"";
                    if(kalturaEmailMessage.ExtraParameters != null && kalturaEmailMessage.ExtraParameters.Count > 0)
                    {
                        ret += ", \"extraParameters\": " + "[" + String.Join(", ", kalturaEmailMessage.ExtraParameters.Select(item => Serialize(item))) + "]";
                    }
                    ret += ", \"firstName\": " + "\"" + kalturaEmailMessage.FirstName + "\"";
                    ret += ", \"lastName\": " + "\"" + kalturaEmailMessage.LastName + "\"";
                    ret += ", \"senderFrom\": " + "\"" + kalturaEmailMessage.SenderFrom + "\"";
                    ret += ", \"senderName\": " + "\"" + kalturaEmailMessage.SenderName + "\"";
                    ret += ", \"senderTo\": " + "\"" + kalturaEmailMessage.SenderTo + "\"";
                    ret += ", \"subject\": " + "\"" + kalturaEmailMessage.Subject + "\"";
                    ret += ", \"templateName\": " + "\"" + kalturaEmailMessage.TemplateName + "\"";
                    break;
                    
                case "KalturaEngagement":
                    KalturaEngagement kalturaEngagement = ottObject as KalturaEngagement;
                    ret += "\"objectType\": " + "\"" + kalturaEngagement.objectType + "\"";
                    if(kalturaEngagement.relatedObjects != null && kalturaEngagement.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaEngagement.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"adapterDynamicData\": " + "\"" + kalturaEngagement.AdapterDynamicData + "\"";
                    ret += ", \"adapterId\": " + kalturaEngagement.AdapterId;
                    ret += ", \"couponGroupId\": " + kalturaEngagement.CouponGroupId;
                    ret += ", \"id\": " + kalturaEngagement.Id;
                    ret += ", \"intervalSeconds\": " + kalturaEngagement.IntervalSeconds;
                    ret += ", \"sendTimeInSeconds\": " + kalturaEngagement.SendTimeInSeconds;
                    ret += ", \"totalNumberOfRecipients\": " + kalturaEngagement.TotalNumberOfRecipients;
                    ret += ", \"type\": " + kalturaEngagement.Type;
                    ret += ", \"userList\": " + "\"" + kalturaEngagement.UserList + "\"";
                    break;
                    
                case "KalturaEngagementAdapter":
                    KalturaEngagementAdapter kalturaEngagementAdapter = ottObject as KalturaEngagementAdapter;
                    ret += "\"objectType\": " + "\"" + kalturaEngagementAdapter.objectType + "\"";
                    if(kalturaEngagementAdapter.relatedObjects != null && kalturaEngagementAdapter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaEngagementAdapter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaEngagementAdapter.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaEngagementAdapter.Id;
                    }
                    ret += ", \"name\": " + "\"" + kalturaEngagementAdapter.Name + "\"";
                    ret += ", \"adapterUrl\": " + "\"" + kalturaEngagementAdapter.AdapterUrl + "\"";
                    if(kalturaEngagementAdapter.IsActive.HasValue)
                    {
                        ret += ", \"isActive\": " + kalturaEngagementAdapter.IsActive;
                    }
                    ret += ", \"providerUrl\": " + "\"" + kalturaEngagementAdapter.ProviderUrl + "\"";
                    if(kalturaEngagementAdapter.Settings != null && kalturaEngagementAdapter.Settings.Count > 0)
                    {
                        ret += ", \"engagementAdapterSettings\": " + "{" + String.Join(", ", kalturaEngagementAdapter.Settings.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"sharedSecret\": " + "\"" + kalturaEngagementAdapter.SharedSecret + "\"";
                    break;
                    
                case "KalturaEngagementAdapterBase":
                    KalturaEngagementAdapterBase kalturaEngagementAdapterBase = ottObject as KalturaEngagementAdapterBase;
                    ret += "\"objectType\": " + "\"" + kalturaEngagementAdapterBase.objectType + "\"";
                    if(kalturaEngagementAdapterBase.relatedObjects != null && kalturaEngagementAdapterBase.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaEngagementAdapterBase.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaEngagementAdapterBase.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaEngagementAdapterBase.Id;
                    }
                    ret += ", \"name\": " + "\"" + kalturaEngagementAdapterBase.Name + "\"";
                    break;
                    
                case "KalturaEngagementAdapterListResponse":
                    KalturaEngagementAdapterListResponse kalturaEngagementAdapterListResponse = ottObject as KalturaEngagementAdapterListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaEngagementAdapterListResponse.objectType + "\"";
                    if(kalturaEngagementAdapterListResponse.relatedObjects != null && kalturaEngagementAdapterListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaEngagementAdapterListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaEngagementAdapterListResponse.TotalCount;
                    if(kalturaEngagementAdapterListResponse.EngagementAdapters != null && kalturaEngagementAdapterListResponse.EngagementAdapters.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaEngagementAdapterListResponse.EngagementAdapters.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaEngagementFilter":
                    KalturaEngagementFilter kalturaEngagementFilter = ottObject as KalturaEngagementFilter;
                    ret += "\"objectType\": " + "\"" + kalturaEngagementFilter.objectType + "\"";
                    if(kalturaEngagementFilter.relatedObjects != null && kalturaEngagementFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaEngagementFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaEngagementFilter.OrderBy;
                    if(kalturaEngagementFilter.SendTimeGreaterThanOrEqual.HasValue)
                    {
                        ret += ", \"sendTimeGreaterThanOrEqual\": " + kalturaEngagementFilter.SendTimeGreaterThanOrEqual;
                    }
                    ret += ", \"typeIn\": " + "\"" + kalturaEngagementFilter.TypeIn + "\"";
                    break;
                    
                case "KalturaEngagementListResponse":
                    KalturaEngagementListResponse kalturaEngagementListResponse = ottObject as KalturaEngagementListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaEngagementListResponse.objectType + "\"";
                    if(kalturaEngagementListResponse.relatedObjects != null && kalturaEngagementListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaEngagementListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaEngagementListResponse.TotalCount;
                    if(kalturaEngagementListResponse.Engagements != null && kalturaEngagementListResponse.Engagements.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaEngagementListResponse.Engagements.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaEntitlement":
                    KalturaEntitlement kalturaEntitlement = ottObject as KalturaEntitlement;
                    ret += "\"objectType\": " + "\"" + kalturaEntitlement.objectType + "\"";
                    if(kalturaEntitlement.relatedObjects != null && kalturaEntitlement.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaEntitlement.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaEntitlement.CurrentDate.HasValue)
                    {
                        ret += ", \"currentDate\": " + kalturaEntitlement.CurrentDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"current_date\": " + kalturaEntitlement.CurrentDate;
                        }
                    }
                    if(kalturaEntitlement.CurrentUses.HasValue)
                    {
                        ret += ", \"currentUses\": " + kalturaEntitlement.CurrentUses;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"current_uses\": " + kalturaEntitlement.CurrentUses;
                        }
                    }
                    ret += ", \"deviceName\": " + "\"" + kalturaEntitlement.DeviceName + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"device_name\": " + "\"" + kalturaEntitlement.DeviceName + "\"";
                    }
                    ret += ", \"deviceUdid\": " + "\"" + kalturaEntitlement.DeviceUDID + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"device_udid\": " + "\"" + kalturaEntitlement.DeviceUDID + "\"";
                    }
                    if(kalturaEntitlement.EndDate.HasValue)
                    {
                        ret += ", \"endDate\": " + kalturaEntitlement.EndDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"end_date\": " + kalturaEntitlement.EndDate;
                        }
                    }
                    ret += ", \"entitlementId\": " + "\"" + kalturaEntitlement.EntitlementId + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"entitlement_id\": " + "\"" + kalturaEntitlement.EntitlementId + "\"";
                    }
                    ret += ", \"householdId\": " + kalturaEntitlement.HouseholdId;
                    if(kalturaEntitlement.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaEntitlement.Id;
                    }
                    if(kalturaEntitlement.IsCancelationWindowEnabled.HasValue)
                    {
                        ret += ", \"isCancelationWindowEnabled\": " + kalturaEntitlement.IsCancelationWindowEnabled;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_cancelation_window_enabled\": " + kalturaEntitlement.IsCancelationWindowEnabled;
                        }
                    }
                    if(kalturaEntitlement.IsInGracePeriod.HasValue)
                    {
                        ret += ", \"isInGracePeriod\": " + kalturaEntitlement.IsInGracePeriod;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_in_grace_period\": " + kalturaEntitlement.IsInGracePeriod;
                        }
                    }
                    if(kalturaEntitlement.IsRenewable.HasValue)
                    {
                        ret += ", \"isRenewable\": " + kalturaEntitlement.IsRenewable;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_renewable\": " + kalturaEntitlement.IsRenewable;
                        }
                    }
                    if(kalturaEntitlement.IsRenewableForPurchase.HasValue)
                    {
                        ret += ", \"isRenewableForPurchase\": " + kalturaEntitlement.IsRenewableForPurchase;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_renewable_for_purchase\": " + kalturaEntitlement.IsRenewableForPurchase;
                        }
                    }
                    if(kalturaEntitlement.LastViewDate.HasValue)
                    {
                        ret += ", \"lastViewDate\": " + kalturaEntitlement.LastViewDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"last_view_date\": " + kalturaEntitlement.LastViewDate;
                        }
                    }
                    if(kalturaEntitlement.MaxUses.HasValue)
                    {
                        ret += ", \"maxUses\": " + kalturaEntitlement.MaxUses;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"max_uses\": " + kalturaEntitlement.MaxUses;
                        }
                    }
                    if(kalturaEntitlement.MediaFileId.HasValue)
                    {
                        ret += ", \"mediaFileId\": " + kalturaEntitlement.MediaFileId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"media_file_id\": " + kalturaEntitlement.MediaFileId;
                        }
                    }
                    if(kalturaEntitlement.MediaId.HasValue)
                    {
                        ret += ", \"mediaId\": " + kalturaEntitlement.MediaId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"media_id\": " + kalturaEntitlement.MediaId;
                        }
                    }
                    if(kalturaEntitlement.NextRenewalDate.HasValue)
                    {
                        ret += ", \"nextRenewalDate\": " + kalturaEntitlement.NextRenewalDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"next_renewal_date\": " + kalturaEntitlement.NextRenewalDate;
                        }
                    }
                    ret += ", \"paymentMethod\": " + kalturaEntitlement.PaymentMethod;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"payment_method\": " + kalturaEntitlement.PaymentMethod;
                    }
                    ret += ", \"productId\": " + "\"" + kalturaEntitlement.ProductId + "\"";
                    if(kalturaEntitlement.PurchaseDate.HasValue)
                    {
                        ret += ", \"purchaseDate\": " + kalturaEntitlement.PurchaseDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"purchase_date\": " + kalturaEntitlement.PurchaseDate;
                        }
                    }
                    if(kalturaEntitlement.PurchaseId.HasValue)
                    {
                        ret += ", \"purchaseId\": " + kalturaEntitlement.PurchaseId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"purchase_id\": " + kalturaEntitlement.PurchaseId;
                        }
                    }
                    ret += ", \"type\": " + kalturaEntitlement.Type;
                    ret += ", \"userId\": " + "\"" + kalturaEntitlement.UserId + "\"";
                    break;
                    
                case "KalturaEntitlementCancellation":
                    KalturaEntitlementCancellation kalturaEntitlementCancellation = ottObject as KalturaEntitlementCancellation;
                    ret += "\"objectType\": " + "\"" + kalturaEntitlementCancellation.objectType + "\"";
                    if(kalturaEntitlementCancellation.relatedObjects != null && kalturaEntitlementCancellation.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaEntitlementCancellation.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"householdId\": " + kalturaEntitlementCancellation.HouseholdId;
                    ret += ", \"id\": " + kalturaEntitlementCancellation.Id;
                    ret += ", \"productId\": " + "\"" + kalturaEntitlementCancellation.ProductId + "\"";
                    ret += ", \"type\": " + kalturaEntitlementCancellation.Type;
                    ret += ", \"userId\": " + "\"" + kalturaEntitlementCancellation.UserId + "\"";
                    break;
                    
                case "KalturaEntitlementFilter":
                    KalturaEntitlementFilter kalturaEntitlementFilter = ottObject as KalturaEntitlementFilter;
                    ret += "\"objectType\": " + "\"" + kalturaEntitlementFilter.objectType + "\"";
                    if(kalturaEntitlementFilter.relatedObjects != null && kalturaEntitlementFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaEntitlementFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaEntitlementFilter.OrderBy;
                    if(kalturaEntitlementFilter.EntitlementTypeEqual.HasValue)
                    {
                        ret += ", \"entitlementTypeEqual\": " + kalturaEntitlementFilter.EntitlementTypeEqual;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"entitlement_type\": " + kalturaEntitlementFilter.EntitlementTypeEqual;
                        }
                    }
                    ret += ", \"entityReferenceEqual\": " + kalturaEntitlementFilter.EntityReferenceEqual;
                    if(kalturaEntitlementFilter.IsExpiredEqual.HasValue)
                    {
                        ret += ", \"isExpiredEqual\": " + kalturaEntitlementFilter.IsExpiredEqual;
                    }
                    if(kalturaEntitlementFilter.ProductTypeEqual.HasValue)
                    {
                        ret += ", \"productTypeEqual\": " + kalturaEntitlementFilter.ProductTypeEqual;
                    }
                    break;
                    
                case "KalturaEntitlementListResponse":
                    KalturaEntitlementListResponse kalturaEntitlementListResponse = ottObject as KalturaEntitlementListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaEntitlementListResponse.objectType + "\"";
                    if(kalturaEntitlementListResponse.relatedObjects != null && kalturaEntitlementListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaEntitlementListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaEntitlementListResponse.TotalCount;
                    if(kalturaEntitlementListResponse.Entitlements != null && kalturaEntitlementListResponse.Entitlements.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaEntitlementListResponse.Entitlements.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaEntitlementRenewal":
                    KalturaEntitlementRenewal kalturaEntitlementRenewal = ottObject as KalturaEntitlementRenewal;
                    ret += "\"objectType\": " + "\"" + kalturaEntitlementRenewal.objectType + "\"";
                    if(kalturaEntitlementRenewal.relatedObjects != null && kalturaEntitlementRenewal.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaEntitlementRenewal.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"date\": " + kalturaEntitlementRenewal.Date;
                    ret += ", \"price\": " + Serialize(kalturaEntitlementRenewal.Price);
                    ret += ", \"purchaseId\": " + kalturaEntitlementRenewal.PurchaseId;
                    ret += ", \"subscriptionId\": " + kalturaEntitlementRenewal.SubscriptionId;
                    break;
                    
                case "KalturaEntitlementRenewalBase":
                    KalturaEntitlementRenewalBase kalturaEntitlementRenewalBase = ottObject as KalturaEntitlementRenewalBase;
                    ret += "\"objectType\": " + "\"" + kalturaEntitlementRenewalBase.objectType + "\"";
                    if(kalturaEntitlementRenewalBase.relatedObjects != null && kalturaEntitlementRenewalBase.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaEntitlementRenewalBase.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"price\": " + kalturaEntitlementRenewalBase.Price;
                    ret += ", \"purchaseId\": " + kalturaEntitlementRenewalBase.PurchaseId;
                    ret += ", \"subscriptionId\": " + kalturaEntitlementRenewalBase.SubscriptionId;
                    break;
                    
                case "KalturaEntitlementsFilter":
                    KalturaEntitlementsFilter kalturaEntitlementsFilter = ottObject as KalturaEntitlementsFilter;
                    ret += "\"objectType\": " + "\"" + kalturaEntitlementsFilter.objectType + "\"";
                    if(kalturaEntitlementsFilter.relatedObjects != null && kalturaEntitlementsFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaEntitlementsFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"by\": " + kalturaEntitlementsFilter.By;
                    ret += ", \"entitlementType\": " + kalturaEntitlementsFilter.EntitlementType;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"entitlement_type\": " + kalturaEntitlementsFilter.EntitlementType;
                    }
                    break;
                    
                case "KalturaEPGChannelAssets":
                    KalturaEPGChannelAssets kalturaEPGChannelAssets = ottObject as KalturaEPGChannelAssets;
                    ret += "\"objectType\": " + "\"" + kalturaEPGChannelAssets.objectType + "\"";
                    if(kalturaEPGChannelAssets.relatedObjects != null && kalturaEPGChannelAssets.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaEPGChannelAssets.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaEPGChannelAssets.TotalCount;
                    if(kalturaEPGChannelAssets.Assets != null && kalturaEPGChannelAssets.Assets.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaEPGChannelAssets.Assets.Select(item => Serialize(item))) + "]";
                    }
                    if(kalturaEPGChannelAssets.ChannelID.HasValue)
                    {
                        ret += ", \"channelId\": " + kalturaEPGChannelAssets.ChannelID;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"channel_id\": " + kalturaEPGChannelAssets.ChannelID;
                        }
                    }
                    break;
                    
                case "KalturaEPGChannelAssetsListResponse":
                    KalturaEPGChannelAssetsListResponse kalturaEPGChannelAssetsListResponse = ottObject as KalturaEPGChannelAssetsListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaEPGChannelAssetsListResponse.objectType + "\"";
                    if(kalturaEPGChannelAssetsListResponse.relatedObjects != null && kalturaEPGChannelAssetsListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaEPGChannelAssetsListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaEPGChannelAssetsListResponse.TotalCount;
                    if(kalturaEPGChannelAssetsListResponse.Channels != null && kalturaEPGChannelAssetsListResponse.Channels.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaEPGChannelAssetsListResponse.Channels.Select(item => Serialize(item))) + "]";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"assets\": " + "[" + String.Join(", ", kalturaEPGChannelAssetsListResponse.Channels.Select(item => Serialize(item))) + "]";
                        }
                    }
                    break;
                    
                case "KalturaEpgChannelFilter":
                    KalturaEpgChannelFilter kalturaEpgChannelFilter = ottObject as KalturaEpgChannelFilter;
                    ret += "\"objectType\": " + "\"" + kalturaEpgChannelFilter.objectType + "\"";
                    if(kalturaEpgChannelFilter.relatedObjects != null && kalturaEpgChannelFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaEpgChannelFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaEpgChannelFilter.EndTime.HasValue)
                    {
                        ret += ", \"endTime\": " + kalturaEpgChannelFilter.EndTime;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"end_time\": " + kalturaEpgChannelFilter.EndTime;
                        }
                    }
                    if(kalturaEpgChannelFilter.IDs != null && kalturaEpgChannelFilter.IDs.Count > 0)
                    {
                        ret += ", \"ids\": " + "[" + String.Join(", ", kalturaEpgChannelFilter.IDs.Select(item => Serialize(item))) + "]";
                    }
                    if(kalturaEpgChannelFilter.StartTime.HasValue)
                    {
                        ret += ", \"startTime\": " + kalturaEpgChannelFilter.StartTime;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"start_time\": " + kalturaEpgChannelFilter.StartTime;
                        }
                    }
                    break;
                    
                case "KalturaExportFilter":
                    KalturaExportFilter kalturaExportFilter = ottObject as KalturaExportFilter;
                    ret += "\"objectType\": " + "\"" + kalturaExportFilter.objectType + "\"";
                    if(kalturaExportFilter.relatedObjects != null && kalturaExportFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaExportFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaExportFilter.OrderBy;
                    if(kalturaExportFilter.ids != null && kalturaExportFilter.ids.Count > 0)
                    {
                        ret += ", \"ids\": " + "[" + String.Join(", ", kalturaExportFilter.ids.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaExportTask":
                    KalturaExportTask kalturaExportTask = ottObject as KalturaExportTask;
                    ret += "\"objectType\": " + "\"" + kalturaExportTask.objectType + "\"";
                    if(kalturaExportTask.relatedObjects != null && kalturaExportTask.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaExportTask.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"alias\": " + "\"" + kalturaExportTask.Alias + "\"";
                    ret += ", \"dataType\": " + kalturaExportTask.DataType;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"data_type\": " + kalturaExportTask.DataType;
                    }
                    ret += ", \"exportType\": " + kalturaExportTask.ExportType;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"export_type\": " + kalturaExportTask.ExportType;
                    }
                    ret += ", \"filter\": " + "\"" + kalturaExportTask.Filter + "\"";
                    if(kalturaExportTask.Frequency.HasValue)
                    {
                        ret += ", \"frequency\": " + kalturaExportTask.Frequency;
                    }
                    if(kalturaExportTask.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaExportTask.Id;
                    }
                    if(kalturaExportTask.IsActive.HasValue)
                    {
                        ret += ", \"isActive\": " + kalturaExportTask.IsActive;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_active\": " + kalturaExportTask.IsActive;
                        }
                    }
                    ret += ", \"name\": " + "\"" + kalturaExportTask.Name + "\"";
                    ret += ", \"notificationUrl\": " + "\"" + kalturaExportTask.NotificationUrl + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"notification_url\": " + "\"" + kalturaExportTask.NotificationUrl + "\"";
                    }
                    if(kalturaExportTask.VodTypes != null && kalturaExportTask.VodTypes.Count > 0)
                    {
                        ret += ", \"vodTypes\": " + "[" + String.Join(", ", kalturaExportTask.VodTypes.Select(item => Serialize(item))) + "]";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"vod_types\": " + "[" + String.Join(", ", kalturaExportTask.VodTypes.Select(item => Serialize(item))) + "]";
                        }
                    }
                    break;
                    
                case "KalturaExportTaskFilter":
                    KalturaExportTaskFilter kalturaExportTaskFilter = ottObject as KalturaExportTaskFilter;
                    ret += "\"objectType\": " + "\"" + kalturaExportTaskFilter.objectType + "\"";
                    if(kalturaExportTaskFilter.relatedObjects != null && kalturaExportTaskFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaExportTaskFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaExportTaskFilter.OrderBy;
                    ret += ", \"idIn\": " + "\"" + kalturaExportTaskFilter.IdIn + "\"";
                    break;
                    
                case "KalturaExportTaskListResponse":
                    KalturaExportTaskListResponse kalturaExportTaskListResponse = ottObject as KalturaExportTaskListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaExportTaskListResponse.objectType + "\"";
                    if(kalturaExportTaskListResponse.relatedObjects != null && kalturaExportTaskListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaExportTaskListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaExportTaskListResponse.TotalCount;
                    if(kalturaExportTaskListResponse.Objects != null && kalturaExportTaskListResponse.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaExportTaskListResponse.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaExternalChannelProfile":
                    KalturaExternalChannelProfile kalturaExternalChannelProfile = ottObject as KalturaExternalChannelProfile;
                    ret += "\"objectType\": " + "\"" + kalturaExternalChannelProfile.objectType + "\"";
                    if(kalturaExternalChannelProfile.relatedObjects != null && kalturaExternalChannelProfile.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaExternalChannelProfile.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaExternalChannelProfile.Enrichments != null && kalturaExternalChannelProfile.Enrichments.Count > 0)
                    {
                        ret += ", \"enrichments\": " + "[" + String.Join(", ", kalturaExternalChannelProfile.Enrichments.Select(item => Serialize(item))) + "]";
                    }
                    ret += ", \"externalIdentifier\": " + "\"" + kalturaExternalChannelProfile.ExternalIdentifier + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"external_identifier\": " + "\"" + kalturaExternalChannelProfile.ExternalIdentifier + "\"";
                    }
                    ret += ", \"filterExpression\": " + "\"" + kalturaExternalChannelProfile.FilterExpression + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"filter_expression\": " + "\"" + kalturaExternalChannelProfile.FilterExpression + "\"";
                    }
                    if(kalturaExternalChannelProfile.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaExternalChannelProfile.Id;
                    }
                    if(kalturaExternalChannelProfile.IsActive.HasValue)
                    {
                        ret += ", \"isActive\": " + kalturaExternalChannelProfile.IsActive;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_active\": " + kalturaExternalChannelProfile.IsActive;
                        }
                    }
                    ret += ", \"name\": " + "\"" + kalturaExternalChannelProfile.Name + "\"";
                    if(kalturaExternalChannelProfile.RecommendationEngineId.HasValue)
                    {
                        ret += ", \"recommendationEngineId\": " + kalturaExternalChannelProfile.RecommendationEngineId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"recommendation_engine_id\": " + kalturaExternalChannelProfile.RecommendationEngineId;
                        }
                    }
                    break;
                    
                case "KalturaExternalChannelProfileListResponse":
                    KalturaExternalChannelProfileListResponse kalturaExternalChannelProfileListResponse = ottObject as KalturaExternalChannelProfileListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaExternalChannelProfileListResponse.objectType + "\"";
                    if(kalturaExternalChannelProfileListResponse.relatedObjects != null && kalturaExternalChannelProfileListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaExternalChannelProfileListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaExternalChannelProfileListResponse.TotalCount;
                    if(kalturaExternalChannelProfileListResponse.Objects != null && kalturaExternalChannelProfileListResponse.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaExternalChannelProfileListResponse.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaExternalReceipt":
                    KalturaExternalReceipt kalturaExternalReceipt = ottObject as KalturaExternalReceipt;
                    ret += "\"objectType\": " + "\"" + kalturaExternalReceipt.objectType + "\"";
                    if(kalturaExternalReceipt.relatedObjects != null && kalturaExternalReceipt.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaExternalReceipt.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaExternalReceipt.ContentId.HasValue)
                    {
                        ret += ", \"contentId\": " + kalturaExternalReceipt.ContentId;
                    }
                    ret += ", \"productId\": " + kalturaExternalReceipt.ProductId;
                    ret += ", \"productType\": " + kalturaExternalReceipt.ProductType;
                    ret += ", \"paymentGatewayName\": " + "\"" + kalturaExternalReceipt.PaymentGatewayName + "\"";
                    ret += ", \"receiptId\": " + "\"" + kalturaExternalReceipt.ReceiptId + "\"";
                    break;
                    
                case "KalturaFacebookPost":
                    KalturaFacebookPost kalturaFacebookPost = ottObject as KalturaFacebookPost;
                    ret += "\"objectType\": " + "\"" + kalturaFacebookPost.objectType + "\"";
                    if(kalturaFacebookPost.relatedObjects != null && kalturaFacebookPost.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaFacebookPost.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"createDate\": " + kalturaFacebookPost.CreateDate;
                    ret += ", \"header\": " + "\"" + kalturaFacebookPost.Header + "\"";
                    ret += ", \"text\": " + "\"" + kalturaFacebookPost.Text + "\"";
                    ret += ", \"writer\": " + "\"" + kalturaFacebookPost.Writer + "\"";
                    ret += ", \"authorImageUrl\": " + "\"" + kalturaFacebookPost.AuthorImageUrl + "\"";
                    ret += ", \"likeCounter\": " + "\"" + kalturaFacebookPost.LikeCounter + "\"";
                    if(kalturaFacebookPost.Comments != null && kalturaFacebookPost.Comments.Count > 0)
                    {
                        ret += ", \"comments\": " + "[" + String.Join(", ", kalturaFacebookPost.Comments.Select(item => Serialize(item))) + "]";
                    }
                    ret += ", \"link\": " + "\"" + kalturaFacebookPost.Link + "\"";
                    break;
                    
                case "KalturaFacebookSocial":
                    KalturaFacebookSocial kalturaFacebookSocial = ottObject as KalturaFacebookSocial;
                    ret += "\"objectType\": " + "\"" + kalturaFacebookSocial.objectType + "\"";
                    if(kalturaFacebookSocial.relatedObjects != null && kalturaFacebookSocial.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaFacebookSocial.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"birthday\": " + "\"" + kalturaFacebookSocial.Birthday + "\"";
                    ret += ", \"email\": " + "\"" + kalturaFacebookSocial.Email + "\"";
                    ret += ", \"firstName\": " + "\"" + kalturaFacebookSocial.FirstName + "\"";
                    ret += ", \"gender\": " + "\"" + kalturaFacebookSocial.Gender + "\"";
                    ret += ", \"id\": " + "\"" + kalturaFacebookSocial.ID + "\"";
                    ret += ", \"lastName\": " + "\"" + kalturaFacebookSocial.LastName + "\"";
                    ret += ", \"name\": " + "\"" + kalturaFacebookSocial.Name + "\"";
                    ret += ", \"pictureUrl\": " + "\"" + kalturaFacebookSocial.PictureUrl + "\"";
                    ret += ", \"status\": " + "\"" + kalturaFacebookSocial.Status + "\"";
                    ret += ", \"userId\": " + "\"" + kalturaFacebookSocial.UserId + "\"";
                    break;
                    
                case "KalturaFairPlayPlaybackPluginData":
                    KalturaFairPlayPlaybackPluginData kalturaFairPlayPlaybackPluginData = ottObject as KalturaFairPlayPlaybackPluginData;
                    ret += "\"objectType\": " + "\"" + kalturaFairPlayPlaybackPluginData.objectType + "\"";
                    if(kalturaFairPlayPlaybackPluginData.relatedObjects != null && kalturaFairPlayPlaybackPluginData.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaFairPlayPlaybackPluginData.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"licenseURL\": " + "\"" + kalturaFairPlayPlaybackPluginData.LicenseURL + "\"";
                    ret += ", \"scheme\": " + kalturaFairPlayPlaybackPluginData.Scheme;
                    ret += ", \"certificate\": " + "\"" + kalturaFairPlayPlaybackPluginData.Certificate + "\"";
                    break;
                    
                case "KalturaFavorite":
                    KalturaFavorite kalturaFavorite = ottObject as KalturaFavorite;
                    ret += "\"objectType\": " + "\"" + kalturaFavorite.objectType + "\"";
                    if(kalturaFavorite.relatedObjects != null && kalturaFavorite.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaFavorite.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"asset\": " + Serialize(kalturaFavorite.Asset);
                    ret += ", \"assetId\": " + kalturaFavorite.AssetId;
                    ret += ", \"createDate\": " + kalturaFavorite.CreateDate;
                    ret += ", \"extraData\": " + "\"" + kalturaFavorite.ExtraData + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"extra_data\": " + "\"" + kalturaFavorite.ExtraData + "\"";
                    }
                    break;
                    
                case "KalturaFavoriteFilter":
                    KalturaFavoriteFilter kalturaFavoriteFilter = ottObject as KalturaFavoriteFilter;
                    ret += "\"objectType\": " + "\"" + kalturaFavoriteFilter.objectType + "\"";
                    if(kalturaFavoriteFilter.relatedObjects != null && kalturaFavoriteFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaFavoriteFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaFavoriteFilter.OrderBy;
                    ret += ", \"mediaIdIn\": " + "\"" + kalturaFavoriteFilter.MediaIdIn + "\"";
                    if(kalturaFavoriteFilter.MediaIds != null && kalturaFavoriteFilter.MediaIds.Count > 0)
                    {
                        ret += ", \"media_ids\": " + "[" + String.Join(", ", kalturaFavoriteFilter.MediaIds.Select(item => Serialize(item))) + "]";
                    }
                    if(kalturaFavoriteFilter.MediaTypeEqual.HasValue)
                    {
                        ret += ", \"mediaTypeEqual\": " + kalturaFavoriteFilter.MediaTypeEqual;
                    }
                    if(kalturaFavoriteFilter.MediaTypeIn.HasValue)
                    {
                        ret += ", \"mediaTypeIn\": " + kalturaFavoriteFilter.MediaTypeIn;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"media_type\": " + kalturaFavoriteFilter.MediaTypeIn;
                        }
                    }
                    ret += ", \"udid\": " + "\"" + kalturaFavoriteFilter.UDID + "\"";
                    if(kalturaFavoriteFilter.UdidEqualCurrent.HasValue)
                    {
                        ret += ", \"udidEqualCurrent\": " + kalturaFavoriteFilter.UdidEqualCurrent;
                    }
                    break;
                    
                case "KalturaFavoriteListResponse":
                    KalturaFavoriteListResponse kalturaFavoriteListResponse = ottObject as KalturaFavoriteListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaFavoriteListResponse.objectType + "\"";
                    if(kalturaFavoriteListResponse.relatedObjects != null && kalturaFavoriteListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaFavoriteListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaFavoriteListResponse.TotalCount;
                    if(kalturaFavoriteListResponse.Favorites != null && kalturaFavoriteListResponse.Favorites.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaFavoriteListResponse.Favorites.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaFeed":
                    KalturaFeed kalturaFeed = ottObject as KalturaFeed;
                    ret += "\"objectType\": " + "\"" + kalturaFeed.objectType + "\"";
                    if(kalturaFeed.relatedObjects != null && kalturaFeed.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaFeed.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"assetId\": " + kalturaFeed.AssetId;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"asset_id\": " + kalturaFeed.AssetId;
                    }
                    break;
                    
                case "KalturaFilter":
                    KalturaFilter<int> kalturaFilter = ottObject as KalturaFilter<int>;
                    ret += "\"objectType\": " + "\"" + kalturaFilter.objectType + "\"";
                    if(kalturaFilter.relatedObjects != null && kalturaFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaFilter.OrderBy;
                    break;
                    
                case "KalturaFilterPager":
                    KalturaFilterPager kalturaFilterPager = ottObject as KalturaFilterPager;
                    ret += "\"objectType\": " + "\"" + kalturaFilterPager.objectType + "\"";
                    if(kalturaFilterPager.relatedObjects != null && kalturaFilterPager.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaFilterPager.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaFilterPager.PageIndex.HasValue)
                    {
                        ret += ", \"pageIndex\": " + kalturaFilterPager.PageIndex;
                    }
                    if(kalturaFilterPager.PageSize.HasValue)
                    {
                        ret += ", \"pageSize\": " + kalturaFilterPager.PageSize;
                    }
                    break;
                    
                case "KalturaFollowDataBase":
                    KalturaFollowDataBase kalturaFollowDataBase = ottObject as KalturaFollowDataBase;
                    ret += "\"objectType\": " + "\"" + kalturaFollowDataBase.objectType + "\"";
                    if(kalturaFollowDataBase.relatedObjects != null && kalturaFollowDataBase.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaFollowDataBase.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"announcementId\": " + kalturaFollowDataBase.AnnouncementId;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"announcement_id\": " + kalturaFollowDataBase.AnnouncementId;
                    }
                    ret += ", \"followPhrase\": " + "\"" + kalturaFollowDataBase.FollowPhrase + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"follow_phrase\": " + "\"" + kalturaFollowDataBase.FollowPhrase + "\"";
                    }
                    ret += ", \"status\": " + kalturaFollowDataBase.Status;
                    ret += ", \"timestamp\": " + kalturaFollowDataBase.Timestamp;
                    ret += ", \"title\": " + "\"" + kalturaFollowDataBase.Title + "\"";
                    break;
                    
                case "KalturaFollowDataTvSeries":
                    KalturaFollowDataTvSeries kalturaFollowDataTvSeries = ottObject as KalturaFollowDataTvSeries;
                    ret += "\"objectType\": " + "\"" + kalturaFollowDataTvSeries.objectType + "\"";
                    if(kalturaFollowDataTvSeries.relatedObjects != null && kalturaFollowDataTvSeries.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaFollowDataTvSeries.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"announcementId\": " + kalturaFollowDataTvSeries.AnnouncementId;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"announcement_id\": " + kalturaFollowDataTvSeries.AnnouncementId;
                    }
                    ret += ", \"followPhrase\": " + "\"" + kalturaFollowDataTvSeries.FollowPhrase + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"follow_phrase\": " + "\"" + kalturaFollowDataTvSeries.FollowPhrase + "\"";
                    }
                    ret += ", \"status\": " + kalturaFollowDataTvSeries.Status;
                    ret += ", \"timestamp\": " + kalturaFollowDataTvSeries.Timestamp;
                    ret += ", \"title\": " + "\"" + kalturaFollowDataTvSeries.Title + "\"";
                    ret += ", \"assetId\": " + kalturaFollowDataTvSeries.AssetId;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"asset_id\": " + kalturaFollowDataTvSeries.AssetId;
                    }
                    break;
                    
                case "KalturaFollowTvSeries":
                    KalturaFollowTvSeries kalturaFollowTvSeries = ottObject as KalturaFollowTvSeries;
                    ret += "\"objectType\": " + "\"" + kalturaFollowTvSeries.objectType + "\"";
                    if(kalturaFollowTvSeries.relatedObjects != null && kalturaFollowTvSeries.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaFollowTvSeries.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"announcementId\": " + kalturaFollowTvSeries.AnnouncementId;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"announcement_id\": " + kalturaFollowTvSeries.AnnouncementId;
                    }
                    ret += ", \"followPhrase\": " + "\"" + kalturaFollowTvSeries.FollowPhrase + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"follow_phrase\": " + "\"" + kalturaFollowTvSeries.FollowPhrase + "\"";
                    }
                    ret += ", \"status\": " + kalturaFollowTvSeries.Status;
                    ret += ", \"timestamp\": " + kalturaFollowTvSeries.Timestamp;
                    ret += ", \"title\": " + "\"" + kalturaFollowTvSeries.Title + "\"";
                    ret += ", \"assetId\": " + kalturaFollowTvSeries.AssetId;
                    break;
                    
                case "KalturaFollowTvSeriesFilter":
                    KalturaFollowTvSeriesFilter kalturaFollowTvSeriesFilter = ottObject as KalturaFollowTvSeriesFilter;
                    ret += "\"objectType\": " + "\"" + kalturaFollowTvSeriesFilter.objectType + "\"";
                    if(kalturaFollowTvSeriesFilter.relatedObjects != null && kalturaFollowTvSeriesFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaFollowTvSeriesFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaFollowTvSeriesFilter.OrderBy;
                    break;
                    
                case "KalturaFollowTvSeriesListResponse":
                    KalturaFollowTvSeriesListResponse kalturaFollowTvSeriesListResponse = ottObject as KalturaFollowTvSeriesListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaFollowTvSeriesListResponse.objectType + "\"";
                    if(kalturaFollowTvSeriesListResponse.relatedObjects != null && kalturaFollowTvSeriesListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaFollowTvSeriesListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaFollowTvSeriesListResponse.TotalCount;
                    if(kalturaFollowTvSeriesListResponse.FollowDataList != null && kalturaFollowTvSeriesListResponse.FollowDataList.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaFollowTvSeriesListResponse.FollowDataList.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaGenericRule":
                    KalturaGenericRule kalturaGenericRule = ottObject as KalturaGenericRule;
                    ret += "\"objectType\": " + "\"" + kalturaGenericRule.objectType + "\"";
                    if(kalturaGenericRule.relatedObjects != null && kalturaGenericRule.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaGenericRule.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"description\": " + "\"" + kalturaGenericRule.Description + "\"";
                    if(kalturaGenericRule.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaGenericRule.Id;
                    }
                    ret += ", \"name\": " + "\"" + kalturaGenericRule.Name + "\"";
                    ret += ", \"ruleType\": " + kalturaGenericRule.RuleType;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"rule_type\": " + kalturaGenericRule.RuleType;
                    }
                    break;
                    
                case "KalturaGenericRuleFilter":
                    KalturaGenericRuleFilter kalturaGenericRuleFilter = ottObject as KalturaGenericRuleFilter;
                    ret += "\"objectType\": " + "\"" + kalturaGenericRuleFilter.objectType + "\"";
                    if(kalturaGenericRuleFilter.relatedObjects != null && kalturaGenericRuleFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaGenericRuleFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaGenericRuleFilter.AssetId.HasValue)
                    {
                        ret += ", \"assetId\": " + kalturaGenericRuleFilter.AssetId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"asset_id\": " + kalturaGenericRuleFilter.AssetId;
                        }
                    }
                    if(kalturaGenericRuleFilter.AssetType.HasValue)
                    {
                        ret += ", \"assetType\": " + kalturaGenericRuleFilter.AssetType;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"asset_type\": " + kalturaGenericRuleFilter.AssetType;
                        }
                    }
                    break;
                    
                case "KalturaGenericRuleListResponse":
                    KalturaGenericRuleListResponse kalturaGenericRuleListResponse = ottObject as KalturaGenericRuleListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaGenericRuleListResponse.objectType + "\"";
                    if(kalturaGenericRuleListResponse.relatedObjects != null && kalturaGenericRuleListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaGenericRuleListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaGenericRuleListResponse.TotalCount;
                    if(kalturaGenericRuleListResponse.GenericRules != null && kalturaGenericRuleListResponse.GenericRules.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaGenericRuleListResponse.GenericRules.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaGroupPermission":
                    KalturaGroupPermission kalturaGroupPermission = ottObject as KalturaGroupPermission;
                    ret += "\"objectType\": " + "\"" + kalturaGroupPermission.objectType + "\"";
                    if(kalturaGroupPermission.relatedObjects != null && kalturaGroupPermission.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaGroupPermission.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaGroupPermission.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaGroupPermission.Id;
                    }
                    ret += ", \"name\": " + "\"" + kalturaGroupPermission.Name + "\"";
                    if(kalturaGroupPermission.PermissionItems != null && kalturaGroupPermission.PermissionItems.Count > 0)
                    {
                        ret += ", \"permissionItems\": " + "[" + String.Join(", ", kalturaGroupPermission.PermissionItems.Select(item => Serialize(item))) + "]";
                    }
                    ret += ", \"group\": " + "\"" + kalturaGroupPermission.Group + "\"";
                    break;
                    
                case "KalturaHomeNetwork":
                    KalturaHomeNetwork kalturaHomeNetwork = ottObject as KalturaHomeNetwork;
                    ret += "\"objectType\": " + "\"" + kalturaHomeNetwork.objectType + "\"";
                    if(kalturaHomeNetwork.relatedObjects != null && kalturaHomeNetwork.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaHomeNetwork.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"description\": " + "\"" + kalturaHomeNetwork.Description + "\"";
                    ret += ", \"externalId\": " + "\"" + kalturaHomeNetwork.ExternalId + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"external_id\": " + "\"" + kalturaHomeNetwork.ExternalId + "\"";
                    }
                    if(kalturaHomeNetwork.IsActive.HasValue)
                    {
                        ret += ", \"isActive\": " + kalturaHomeNetwork.IsActive;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_active\": " + kalturaHomeNetwork.IsActive;
                        }
                    }
                    ret += ", \"name\": " + "\"" + kalturaHomeNetwork.Name + "\"";
                    break;
                    
                case "KalturaHomeNetworkListResponse":
                    KalturaHomeNetworkListResponse kalturaHomeNetworkListResponse = ottObject as KalturaHomeNetworkListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaHomeNetworkListResponse.objectType + "\"";
                    if(kalturaHomeNetworkListResponse.relatedObjects != null && kalturaHomeNetworkListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaHomeNetworkListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaHomeNetworkListResponse.TotalCount;
                    if(kalturaHomeNetworkListResponse.Objects != null && kalturaHomeNetworkListResponse.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaHomeNetworkListResponse.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaHousehold":
                    KalturaHousehold kalturaHousehold = ottObject as KalturaHousehold;
                    ret += "\"objectType\": " + "\"" + kalturaHousehold.objectType + "\"";
                    if(kalturaHousehold.relatedObjects != null && kalturaHousehold.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaHousehold.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaHousehold.ConcurrentLimit.HasValue)
                    {
                        ret += ", \"concurrentLimit\": " + kalturaHousehold.ConcurrentLimit;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"concurrent_limit\": " + kalturaHousehold.ConcurrentLimit;
                        }
                    }
                    if(kalturaHousehold.DefaultUsers != null && kalturaHousehold.DefaultUsers.Count > 0)
                    {
                        ret += ", \"defaultUsers\": " + "[" + String.Join(", ", kalturaHousehold.DefaultUsers.Select(item => Serialize(item))) + "]";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"default_users\": " + "[" + String.Join(", ", kalturaHousehold.DefaultUsers.Select(item => Serialize(item))) + "]";
                        }
                    }
                    ret += ", \"description\": " + "\"" + kalturaHousehold.Description + "\"";
                    if(kalturaHousehold.DeviceFamilies != null && kalturaHousehold.DeviceFamilies.Count > 0)
                    {
                        ret += ", \"deviceFamilies\": " + "[" + String.Join(", ", kalturaHousehold.DeviceFamilies.Select(item => Serialize(item))) + "]";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"device_families\": " + "[" + String.Join(", ", kalturaHousehold.DeviceFamilies.Select(item => Serialize(item))) + "]";
                        }
                    }
                    if(kalturaHousehold.DevicesLimit.HasValue)
                    {
                        ret += ", \"devicesLimit\": " + kalturaHousehold.DevicesLimit;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"devices_limit\": " + kalturaHousehold.DevicesLimit;
                        }
                    }
                    ret += ", \"externalId\": " + "\"" + kalturaHousehold.ExternalId + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"external_id\": " + "\"" + kalturaHousehold.ExternalId + "\"";
                    }
                    if(kalturaHousehold.FrequencyNextDeviceAction.HasValue)
                    {
                        ret += ", \"frequencyNextDeviceAction\": " + kalturaHousehold.FrequencyNextDeviceAction;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"frequency_next_device_action\": " + kalturaHousehold.FrequencyNextDeviceAction;
                        }
                    }
                    if(kalturaHousehold.FrequencyNextUserAction.HasValue)
                    {
                        ret += ", \"frequencyNextUserAction\": " + kalturaHousehold.FrequencyNextUserAction;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"frequency_next_user_action\": " + kalturaHousehold.FrequencyNextUserAction;
                        }
                    }
                    if(kalturaHousehold.HouseholdLimitationsId.HasValue)
                    {
                        ret += ", \"householdLimitationsId\": " + kalturaHousehold.HouseholdLimitationsId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"household_limitations_id\": " + kalturaHousehold.HouseholdLimitationsId;
                        }
                    }
                    if(kalturaHousehold.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaHousehold.Id;
                    }
                    if(kalturaHousehold.IsFrequencyEnabled.HasValue)
                    {
                        ret += ", \"isFrequencyEnabled\": " + kalturaHousehold.IsFrequencyEnabled;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_frequency_enabled\": " + kalturaHousehold.IsFrequencyEnabled;
                        }
                    }
                    if(kalturaHousehold.MasterUsers != null && kalturaHousehold.MasterUsers.Count > 0)
                    {
                        ret += ", \"masterUsers\": " + "[" + String.Join(", ", kalturaHousehold.MasterUsers.Select(item => Serialize(item))) + "]";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"master_users\": " + "[" + String.Join(", ", kalturaHousehold.MasterUsers.Select(item => Serialize(item))) + "]";
                        }
                    }
                    ret += ", \"name\": " + "\"" + kalturaHousehold.Name + "\"";
                    if(kalturaHousehold.PendingUsers != null && kalturaHousehold.PendingUsers.Count > 0)
                    {
                        ret += ", \"pendingUsers\": " + "[" + String.Join(", ", kalturaHousehold.PendingUsers.Select(item => Serialize(item))) + "]";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"pending_users\": " + "[" + String.Join(", ", kalturaHousehold.PendingUsers.Select(item => Serialize(item))) + "]";
                        }
                    }
                    if(kalturaHousehold.RegionId.HasValue)
                    {
                        ret += ", \"regionId\": " + kalturaHousehold.RegionId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"region_id\": " + kalturaHousehold.RegionId;
                        }
                    }
                    if(kalturaHousehold.Restriction.HasValue)
                    {
                        ret += ", \"restriction\": " + kalturaHousehold.Restriction;
                    }
                    if(kalturaHousehold.RoleId.HasValue)
                    {
                        ret += ", \"roleId\": " + kalturaHousehold.RoleId;
                    }
                    if(kalturaHousehold.State.HasValue)
                    {
                        ret += ", \"state\": " + kalturaHousehold.State;
                    }
                    if(kalturaHousehold.Users != null && kalturaHousehold.Users.Count > 0)
                    {
                        ret += ", \"users\": " + "[" + String.Join(", ", kalturaHousehold.Users.Select(item => Serialize(item))) + "]";
                    }
                    if(kalturaHousehold.UsersLimit.HasValue)
                    {
                        ret += ", \"usersLimit\": " + kalturaHousehold.UsersLimit;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"users_limit\": " + kalturaHousehold.UsersLimit;
                        }
                    }
                    break;
                    
                case "KalturaHouseholdDevice":
                    KalturaHouseholdDevice kalturaHouseholdDevice = ottObject as KalturaHouseholdDevice;
                    ret += "\"objectType\": " + "\"" + kalturaHouseholdDevice.objectType + "\"";
                    if(kalturaHouseholdDevice.relatedObjects != null && kalturaHouseholdDevice.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaHouseholdDevice.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaHouseholdDevice.ActivatedOn.HasValue)
                    {
                        ret += ", \"activatedOn\": " + kalturaHouseholdDevice.ActivatedOn;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"activated_on\": " + kalturaHouseholdDevice.ActivatedOn;
                        }
                    }
                    ret += ", \"brand\": " + "\"" + kalturaHouseholdDevice.Brand + "\"";
                    if(kalturaHouseholdDevice.BrandId.HasValue)
                    {
                        ret += ", \"brandId\": " + kalturaHouseholdDevice.BrandId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"brand_id\": " + kalturaHouseholdDevice.BrandId;
                        }
                    }
                    if(kalturaHouseholdDevice.DeviceFamilyId.HasValue)
                    {
                        ret += ", \"deviceFamilyId\": " + kalturaHouseholdDevice.DeviceFamilyId;
                    }
                    ret += ", \"drm\": " + Serialize(kalturaHouseholdDevice.Drm);
                    ret += ", \"householdId\": " + kalturaHouseholdDevice.HouseholdId;
                    ret += ", \"name\": " + "\"" + kalturaHouseholdDevice.Name + "\"";
                    if(kalturaHouseholdDevice.State.HasValue)
                    {
                        ret += ", \"state\": " + kalturaHouseholdDevice.State;
                    }
                    if(kalturaHouseholdDevice.Status.HasValue)
                    {
                        ret += ", \"status\": " + kalturaHouseholdDevice.Status;
                    }
                    ret += ", \"udid\": " + "\"" + kalturaHouseholdDevice.Udid + "\"";
                    break;
                    
                case "KalturaHouseholdDeviceFamilyLimitations":
                    KalturaHouseholdDeviceFamilyLimitations kalturaHouseholdDeviceFamilyLimitations = ottObject as KalturaHouseholdDeviceFamilyLimitations;
                    ret += "\"objectType\": " + "\"" + kalturaHouseholdDeviceFamilyLimitations.objectType + "\"";
                    if(kalturaHouseholdDeviceFamilyLimitations.relatedObjects != null && kalturaHouseholdDeviceFamilyLimitations.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaHouseholdDeviceFamilyLimitations.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaHouseholdDeviceFamilyLimitations.ConcurrentLimit.HasValue)
                    {
                        ret += ", \"concurrentLimit\": " + kalturaHouseholdDeviceFamilyLimitations.ConcurrentLimit;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"concurrent_limit\": " + kalturaHouseholdDeviceFamilyLimitations.ConcurrentLimit;
                        }
                    }
                    if(kalturaHouseholdDeviceFamilyLimitations.DeviceLimit.HasValue)
                    {
                        ret += ", \"deviceLimit\": " + kalturaHouseholdDeviceFamilyLimitations.DeviceLimit;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"device_limit\": " + kalturaHouseholdDeviceFamilyLimitations.DeviceLimit;
                        }
                    }
                    if(kalturaHouseholdDeviceFamilyLimitations.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaHouseholdDeviceFamilyLimitations.Id;
                    }
                    ret += ", \"name\": " + "\"" + kalturaHouseholdDeviceFamilyLimitations.Name + "\"";
                    if(kalturaHouseholdDeviceFamilyLimitations.ConcurrentLimit.HasValue)
                    {
                        ret += ", \"concurrentLimit\": " + kalturaHouseholdDeviceFamilyLimitations.ConcurrentLimit;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"concurrent_limit\": " + kalturaHouseholdDeviceFamilyLimitations.ConcurrentLimit;
                        }
                    }
                    if(kalturaHouseholdDeviceFamilyLimitations.DeviceLimit.HasValue)
                    {
                        ret += ", \"deviceLimit\": " + kalturaHouseholdDeviceFamilyLimitations.DeviceLimit;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"device_limit\": " + kalturaHouseholdDeviceFamilyLimitations.DeviceLimit;
                        }
                    }
                    if(kalturaHouseholdDeviceFamilyLimitations.Frequency.HasValue)
                    {
                        ret += ", \"frequency\": " + kalturaHouseholdDeviceFamilyLimitations.Frequency;
                    }
                    break;
                    
                case "KalturaHouseholdDeviceFilter":
                    KalturaHouseholdDeviceFilter kalturaHouseholdDeviceFilter = ottObject as KalturaHouseholdDeviceFilter;
                    ret += "\"objectType\": " + "\"" + kalturaHouseholdDeviceFilter.objectType + "\"";
                    if(kalturaHouseholdDeviceFilter.relatedObjects != null && kalturaHouseholdDeviceFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaHouseholdDeviceFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaHouseholdDeviceFilter.OrderBy;
                    ret += ", \"deviceFamilyIdIn\": " + "\"" + kalturaHouseholdDeviceFilter.DeviceFamilyIdIn + "\"";
                    if(kalturaHouseholdDeviceFilter.HouseholdIdEqual.HasValue)
                    {
                        ret += ", \"householdIdEqual\": " + kalturaHouseholdDeviceFilter.HouseholdIdEqual;
                    }
                    break;
                    
                case "KalturaHouseholdDeviceListResponse":
                    KalturaHouseholdDeviceListResponse kalturaHouseholdDeviceListResponse = ottObject as KalturaHouseholdDeviceListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaHouseholdDeviceListResponse.objectType + "\"";
                    if(kalturaHouseholdDeviceListResponse.relatedObjects != null && kalturaHouseholdDeviceListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaHouseholdDeviceListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaHouseholdDeviceListResponse.TotalCount;
                    if(kalturaHouseholdDeviceListResponse.Objects != null && kalturaHouseholdDeviceListResponse.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaHouseholdDeviceListResponse.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaHouseholdLimitations":
                    KalturaHouseholdLimitations kalturaHouseholdLimitations = ottObject as KalturaHouseholdLimitations;
                    ret += "\"objectType\": " + "\"" + kalturaHouseholdLimitations.objectType + "\"";
                    if(kalturaHouseholdLimitations.relatedObjects != null && kalturaHouseholdLimitations.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaHouseholdLimitations.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaHouseholdLimitations.ConcurrentLimit.HasValue)
                    {
                        ret += ", \"concurrentLimit\": " + kalturaHouseholdLimitations.ConcurrentLimit;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"concurrent_limit\": " + kalturaHouseholdLimitations.ConcurrentLimit;
                        }
                    }
                    if(kalturaHouseholdLimitations.DeviceFamiliesLimitations != null && kalturaHouseholdLimitations.DeviceFamiliesLimitations.Count > 0)
                    {
                        ret += ", \"deviceFamiliesLimitations\": " + "[" + String.Join(", ", kalturaHouseholdLimitations.DeviceFamiliesLimitations.Select(item => Serialize(item))) + "]";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"device_families_limitations\": " + "[" + String.Join(", ", kalturaHouseholdLimitations.DeviceFamiliesLimitations.Select(item => Serialize(item))) + "]";
                        }
                    }
                    if(kalturaHouseholdLimitations.DeviceFrequency.HasValue)
                    {
                        ret += ", \"deviceFrequency\": " + kalturaHouseholdLimitations.DeviceFrequency;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"device_frequency\": " + kalturaHouseholdLimitations.DeviceFrequency;
                        }
                    }
                    ret += ", \"deviceFrequencyDescription\": " + "\"" + kalturaHouseholdLimitations.DeviceFrequencyDescription + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"device_frequency_description\": " + "\"" + kalturaHouseholdLimitations.DeviceFrequencyDescription + "\"";
                    }
                    if(kalturaHouseholdLimitations.DeviceLimit.HasValue)
                    {
                        ret += ", \"deviceLimit\": " + kalturaHouseholdLimitations.DeviceLimit;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"device_limit\": " + kalturaHouseholdLimitations.DeviceLimit;
                        }
                    }
                    if(kalturaHouseholdLimitations.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaHouseholdLimitations.Id;
                    }
                    ret += ", \"name\": " + "\"" + kalturaHouseholdLimitations.Name + "\"";
                    if(kalturaHouseholdLimitations.NpvrQuotaInSeconds.HasValue)
                    {
                        ret += ", \"npvrQuotaInSeconds\": " + kalturaHouseholdLimitations.NpvrQuotaInSeconds;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"npvr_quota_in_seconds\": " + kalturaHouseholdLimitations.NpvrQuotaInSeconds;
                        }
                    }
                    if(kalturaHouseholdLimitations.UserFrequency.HasValue)
                    {
                        ret += ", \"userFrequency\": " + kalturaHouseholdLimitations.UserFrequency;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"user_frequency\": " + kalturaHouseholdLimitations.UserFrequency;
                        }
                    }
                    ret += ", \"userFrequencyDescription\": " + "\"" + kalturaHouseholdLimitations.UserFrequencyDescription + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"user_frequency_description\": " + "\"" + kalturaHouseholdLimitations.UserFrequencyDescription + "\"";
                    }
                    if(kalturaHouseholdLimitations.UsersLimit.HasValue)
                    {
                        ret += ", \"usersLimit\": " + kalturaHouseholdLimitations.UsersLimit;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"users_limit\": " + kalturaHouseholdLimitations.UsersLimit;
                        }
                    }
                    break;
                    
                case "KalturaHouseholdPaymentGateway":
                    KalturaHouseholdPaymentGateway kalturaHouseholdPaymentGateway = ottObject as KalturaHouseholdPaymentGateway;
                    ret += "\"objectType\": " + "\"" + kalturaHouseholdPaymentGateway.objectType + "\"";
                    if(kalturaHouseholdPaymentGateway.relatedObjects != null && kalturaHouseholdPaymentGateway.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaHouseholdPaymentGateway.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaHouseholdPaymentGateway.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaHouseholdPaymentGateway.Id;
                    }
                    if(kalturaHouseholdPaymentGateway.IsDefault.HasValue)
                    {
                        ret += ", \"isDefault\": " + kalturaHouseholdPaymentGateway.IsDefault;
                    }
                    ret += ", \"name\": " + "\"" + kalturaHouseholdPaymentGateway.Name + "\"";
                    ret += ", \"selectedBy\": " + kalturaHouseholdPaymentGateway.selectedBy;
                    break;
                    
                case "KalturaHouseholdPaymentGatewayListResponse":
                    KalturaHouseholdPaymentGatewayListResponse kalturaHouseholdPaymentGatewayListResponse = ottObject as KalturaHouseholdPaymentGatewayListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaHouseholdPaymentGatewayListResponse.objectType + "\"";
                    if(kalturaHouseholdPaymentGatewayListResponse.relatedObjects != null && kalturaHouseholdPaymentGatewayListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaHouseholdPaymentGatewayListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaHouseholdPaymentGatewayListResponse.TotalCount;
                    if(kalturaHouseholdPaymentGatewayListResponse.Objects != null && kalturaHouseholdPaymentGatewayListResponse.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaHouseholdPaymentGatewayListResponse.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaHouseholdPaymentMethod":
                    KalturaHouseholdPaymentMethod kalturaHouseholdPaymentMethod = ottObject as KalturaHouseholdPaymentMethod;
                    ret += "\"objectType\": " + "\"" + kalturaHouseholdPaymentMethod.objectType + "\"";
                    if(kalturaHouseholdPaymentMethod.relatedObjects != null && kalturaHouseholdPaymentMethod.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaHouseholdPaymentMethod.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaHouseholdPaymentMethod.AllowMultiInstance.HasValue)
                    {
                        ret += ", \"allowMultiInstance\": " + kalturaHouseholdPaymentMethod.AllowMultiInstance;
                    }
                    ret += ", \"details\": " + "\"" + kalturaHouseholdPaymentMethod.Details + "\"";
                    ret += ", \"externalId\": " + "\"" + kalturaHouseholdPaymentMethod.ExternalId + "\"";
                    if(kalturaHouseholdPaymentMethod.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaHouseholdPaymentMethod.Id;
                    }
                    if(kalturaHouseholdPaymentMethod.IsDefault.HasValue)
                    {
                        ret += ", \"isDefault\": " + kalturaHouseholdPaymentMethod.IsDefault;
                    }
                    ret += ", \"name\": " + "\"" + kalturaHouseholdPaymentMethod.Name + "\"";
                    if(kalturaHouseholdPaymentMethod.PaymentGatewayId.HasValue)
                    {
                        ret += ", \"paymentGatewayId\": " + kalturaHouseholdPaymentMethod.PaymentGatewayId;
                    }
                    ret += ", \"paymentMethodProfileId\": " + kalturaHouseholdPaymentMethod.PaymentMethodProfileId;
                    if(kalturaHouseholdPaymentMethod.Selected.HasValue)
                    {
                        ret += ", \"selected\": " + kalturaHouseholdPaymentMethod.Selected;
                    }
                    break;
                    
                case "KalturaHouseholdPaymentMethodListResponse":
                    KalturaHouseholdPaymentMethodListResponse kalturaHouseholdPaymentMethodListResponse = ottObject as KalturaHouseholdPaymentMethodListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaHouseholdPaymentMethodListResponse.objectType + "\"";
                    if(kalturaHouseholdPaymentMethodListResponse.relatedObjects != null && kalturaHouseholdPaymentMethodListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaHouseholdPaymentMethodListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaHouseholdPaymentMethodListResponse.TotalCount;
                    if(kalturaHouseholdPaymentMethodListResponse.Objects != null && kalturaHouseholdPaymentMethodListResponse.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaHouseholdPaymentMethodListResponse.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaHouseholdPremiumService":
                    KalturaHouseholdPremiumService kalturaHouseholdPremiumService = ottObject as KalturaHouseholdPremiumService;
                    ret += "\"objectType\": " + "\"" + kalturaHouseholdPremiumService.objectType + "\"";
                    if(kalturaHouseholdPremiumService.relatedObjects != null && kalturaHouseholdPremiumService.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaHouseholdPremiumService.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaHouseholdPremiumService.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaHouseholdPremiumService.Id;
                    }
                    ret += ", \"name\": " + "\"" + kalturaHouseholdPremiumService.Name + "\"";
                    break;
                    
                case "KalturaHouseholdPremiumServiceListResponse":
                    KalturaHouseholdPremiumServiceListResponse kalturaHouseholdPremiumServiceListResponse = ottObject as KalturaHouseholdPremiumServiceListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaHouseholdPremiumServiceListResponse.objectType + "\"";
                    if(kalturaHouseholdPremiumServiceListResponse.relatedObjects != null && kalturaHouseholdPremiumServiceListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaHouseholdPremiumServiceListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaHouseholdPremiumServiceListResponse.TotalCount;
                    if(kalturaHouseholdPremiumServiceListResponse.PremiumServices != null && kalturaHouseholdPremiumServiceListResponse.PremiumServices.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaHouseholdPremiumServiceListResponse.PremiumServices.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaHouseholdQuota":
                    KalturaHouseholdQuota kalturaHouseholdQuota = ottObject as KalturaHouseholdQuota;
                    ret += "\"objectType\": " + "\"" + kalturaHouseholdQuota.objectType + "\"";
                    if(kalturaHouseholdQuota.relatedObjects != null && kalturaHouseholdQuota.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaHouseholdQuota.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"availableQuota\": " + kalturaHouseholdQuota.AvailableQuota;
                    ret += ", \"householdId\": " + kalturaHouseholdQuota.HouseholdId;
                    ret += ", \"totalQuota\": " + kalturaHouseholdQuota.TotalQuota;
                    break;
                    
                case "KalturaHouseholdUser":
                    KalturaHouseholdUser kalturaHouseholdUser = ottObject as KalturaHouseholdUser;
                    ret += "\"objectType\": " + "\"" + kalturaHouseholdUser.objectType + "\"";
                    if(kalturaHouseholdUser.relatedObjects != null && kalturaHouseholdUser.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaHouseholdUser.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaHouseholdUser.HouseholdId.HasValue)
                    {
                        ret += ", \"householdId\": " + kalturaHouseholdUser.HouseholdId;
                    }
                    ret += ", \"householdMasterUsername\": " + "\"" + kalturaHouseholdUser.HouseholdMasterUsername + "\"";
                    if(kalturaHouseholdUser.IsDefault.HasValue)
                    {
                        ret += ", \"isDefault\": " + kalturaHouseholdUser.IsDefault;
                    }
                    if(kalturaHouseholdUser.IsMaster.HasValue)
                    {
                        ret += ", \"isMaster\": " + kalturaHouseholdUser.IsMaster;
                    }
                    ret += ", \"status\": " + kalturaHouseholdUser.Status;
                    ret += ", \"userId\": " + "\"" + kalturaHouseholdUser.UserId + "\"";
                    break;
                    
                case "KalturaHouseholdUserFilter":
                    KalturaHouseholdUserFilter kalturaHouseholdUserFilter = ottObject as KalturaHouseholdUserFilter;
                    ret += "\"objectType\": " + "\"" + kalturaHouseholdUserFilter.objectType + "\"";
                    if(kalturaHouseholdUserFilter.relatedObjects != null && kalturaHouseholdUserFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaHouseholdUserFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaHouseholdUserFilter.OrderBy;
                    if(kalturaHouseholdUserFilter.HouseholdIdEqual.HasValue)
                    {
                        ret += ", \"householdIdEqual\": " + kalturaHouseholdUserFilter.HouseholdIdEqual;
                    }
                    break;
                    
                case "KalturaHouseholdUserListResponse":
                    KalturaHouseholdUserListResponse kalturaHouseholdUserListResponse = ottObject as KalturaHouseholdUserListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaHouseholdUserListResponse.objectType + "\"";
                    if(kalturaHouseholdUserListResponse.relatedObjects != null && kalturaHouseholdUserListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaHouseholdUserListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaHouseholdUserListResponse.TotalCount;
                    if(kalturaHouseholdUserListResponse.Objects != null && kalturaHouseholdUserListResponse.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaHouseholdUserListResponse.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaHouseholdWithHolder":
                    KalturaHouseholdWithHolder kalturaHouseholdWithHolder = ottObject as KalturaHouseholdWithHolder;
                    ret += "\"objectType\": " + "\"" + kalturaHouseholdWithHolder.objectType + "\"";
                    if(kalturaHouseholdWithHolder.relatedObjects != null && kalturaHouseholdWithHolder.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaHouseholdWithHolder.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"type\": " + kalturaHouseholdWithHolder.type;
                    break;
                    
                case "KalturaHttpNotification":
                    KalturaHttpNotification kalturaHttpNotification = ottObject as KalturaHttpNotification;
                    ret += "\"objectType\": " + "\"" + kalturaHttpNotification.objectType + "\"";
                    if(kalturaHttpNotification.relatedObjects != null && kalturaHttpNotification.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaHttpNotification.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"object\": " + Serialize(kalturaHttpNotification.eventObject);
                    ret += ", \"eventObjectType\": " + "\"" + kalturaHttpNotification.eventObjectType + "\"";
                    if(kalturaHttpNotification.eventType.HasValue)
                    {
                        ret += ", \"eventType\": " + kalturaHttpNotification.eventType;
                    }
                    ret += ", \"systemName\": " + "\"" + kalturaHttpNotification.systemName + "\"";
                    break;
                    
                case "KalturaIdentifierTypeFilter":
                    KalturaIdentifierTypeFilter kalturaIdentifierTypeFilter = ottObject as KalturaIdentifierTypeFilter;
                    ret += "\"objectType\": " + "\"" + kalturaIdentifierTypeFilter.objectType + "\"";
                    if(kalturaIdentifierTypeFilter.relatedObjects != null && kalturaIdentifierTypeFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaIdentifierTypeFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"by\": " + kalturaIdentifierTypeFilter.By;
                    ret += ", \"identifier\": " + "\"" + kalturaIdentifierTypeFilter.Identifier + "\"";
                    break;
                    
                case "KalturaInboxMessage":
                    KalturaInboxMessage kalturaInboxMessage = ottObject as KalturaInboxMessage;
                    ret += "\"objectType\": " + "\"" + kalturaInboxMessage.objectType + "\"";
                    if(kalturaInboxMessage.relatedObjects != null && kalturaInboxMessage.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaInboxMessage.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"createdAt\": " + kalturaInboxMessage.CreatedAt;
                    ret += ", \"id\": " + "\"" + kalturaInboxMessage.Id + "\"";
                    ret += ", \"message\": " + "\"" + kalturaInboxMessage.Message + "\"";
                    ret += ", \"status\": " + kalturaInboxMessage.Status;
                    ret += ", \"type\": " + kalturaInboxMessage.Type;
                    ret += ", \"url\": " + "\"" + kalturaInboxMessage.Url + "\"";
                    break;
                    
                case "KalturaInboxMessageFilter":
                    KalturaInboxMessageFilter kalturaInboxMessageFilter = ottObject as KalturaInboxMessageFilter;
                    ret += "\"objectType\": " + "\"" + kalturaInboxMessageFilter.objectType + "\"";
                    if(kalturaInboxMessageFilter.relatedObjects != null && kalturaInboxMessageFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaInboxMessageFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaInboxMessageFilter.OrderBy;
                    if(kalturaInboxMessageFilter.CreatedAtGreaterThanOrEqual.HasValue)
                    {
                        ret += ", \"createdAtGreaterThanOrEqual\": " + kalturaInboxMessageFilter.CreatedAtGreaterThanOrEqual;
                    }
                    if(kalturaInboxMessageFilter.CreatedAtLessThanOrEqual.HasValue)
                    {
                        ret += ", \"createdAtLessThanOrEqual\": " + kalturaInboxMessageFilter.CreatedAtLessThanOrEqual;
                    }
                    ret += ", \"typeIn\": " + "\"" + kalturaInboxMessageFilter.TypeIn + "\"";
                    break;
                    
                case "KalturaInboxMessageListResponse":
                    KalturaInboxMessageListResponse kalturaInboxMessageListResponse = ottObject as KalturaInboxMessageListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaInboxMessageListResponse.objectType + "\"";
                    if(kalturaInboxMessageListResponse.relatedObjects != null && kalturaInboxMessageListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaInboxMessageListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaInboxMessageListResponse.TotalCount;
                    if(kalturaInboxMessageListResponse.InboxMessages != null && kalturaInboxMessageListResponse.InboxMessages.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaInboxMessageListResponse.InboxMessages.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaInboxMessageResponse":
                    KalturaInboxMessageResponse kalturaInboxMessageResponse = ottObject as KalturaInboxMessageResponse;
                    ret += "\"objectType\": " + "\"" + kalturaInboxMessageResponse.objectType + "\"";
                    if(kalturaInboxMessageResponse.relatedObjects != null && kalturaInboxMessageResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaInboxMessageResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaInboxMessageResponse.TotalCount;
                    if(kalturaInboxMessageResponse.InboxMessages != null && kalturaInboxMessageResponse.InboxMessages.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaInboxMessageResponse.InboxMessages.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaInboxMessageTypeHolder":
                    KalturaInboxMessageTypeHolder kalturaInboxMessageTypeHolder = ottObject as KalturaInboxMessageTypeHolder;
                    ret += "\"objectType\": " + "\"" + kalturaInboxMessageTypeHolder.objectType + "\"";
                    if(kalturaInboxMessageTypeHolder.relatedObjects != null && kalturaInboxMessageTypeHolder.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaInboxMessageTypeHolder.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"type\": " + kalturaInboxMessageTypeHolder.type;
                    break;
                    
                case "KalturaIntegerValue":
                    KalturaIntegerValue kalturaIntegerValue = ottObject as KalturaIntegerValue;
                    ret += "\"objectType\": " + "\"" + kalturaIntegerValue.objectType + "\"";
                    if(kalturaIntegerValue.relatedObjects != null && kalturaIntegerValue.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaIntegerValue.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"description\": " + "\"" + kalturaIntegerValue.description + "\"";
                    ret += ", \"value\": " + kalturaIntegerValue.value;
                    break;
                    
                case "KalturaIntegerValueListResponse":
                    KalturaIntegerValueListResponse kalturaIntegerValueListResponse = ottObject as KalturaIntegerValueListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaIntegerValueListResponse.objectType + "\"";
                    if(kalturaIntegerValueListResponse.relatedObjects != null && kalturaIntegerValueListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaIntegerValueListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaIntegerValueListResponse.TotalCount;
                    if(kalturaIntegerValueListResponse.Values != null && kalturaIntegerValueListResponse.Values.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaIntegerValueListResponse.Values.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaItemPrice":
                    KalturaItemPrice kalturaItemPrice = ottObject as KalturaItemPrice;
                    ret += "\"objectType\": " + "\"" + kalturaItemPrice.objectType + "\"";
                    if(kalturaItemPrice.relatedObjects != null && kalturaItemPrice.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaItemPrice.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"price\": " + Serialize(kalturaItemPrice.Price);
                    ret += ", \"productId\": " + "\"" + kalturaItemPrice.ProductId + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"product_id\": " + "\"" + kalturaItemPrice.ProductId + "\"";
                    }
                    ret += ", \"productType\": " + kalturaItemPrice.ProductType;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"product_type\": " + kalturaItemPrice.ProductType;
                    }
                    ret += ", \"purchaseStatus\": " + kalturaItemPrice.PurchaseStatus;
                    if(kalturaItemPrice.FileId.HasValue)
                    {
                        ret += ", \"fileId\": " + kalturaItemPrice.FileId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"file_id\": " + kalturaItemPrice.FileId;
                        }
                    }
                    if(kalturaItemPrice.PPVPriceDetails != null && kalturaItemPrice.PPVPriceDetails.Count > 0)
                    {
                        ret += ", \"ppvPriceDetails\": " + "[" + String.Join(", ", kalturaItemPrice.PPVPriceDetails.Select(item => Serialize(item))) + "]";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"ppv_price_details\": " + "[" + String.Join(", ", kalturaItemPrice.PPVPriceDetails.Select(item => Serialize(item))) + "]";
                        }
                    }
                    break;
                    
                case "KalturaItemPriceListResponse":
                    KalturaItemPriceListResponse kalturaItemPriceListResponse = ottObject as KalturaItemPriceListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaItemPriceListResponse.objectType + "\"";
                    if(kalturaItemPriceListResponse.relatedObjects != null && kalturaItemPriceListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaItemPriceListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaItemPriceListResponse.TotalCount;
                    if(kalturaItemPriceListResponse.ItemPrice != null && kalturaItemPriceListResponse.ItemPrice.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaItemPriceListResponse.ItemPrice.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaKeyValue":
                    KalturaKeyValue kalturaKeyValue = ottObject as KalturaKeyValue;
                    ret += "\"objectType\": " + "\"" + kalturaKeyValue.objectType + "\"";
                    if(kalturaKeyValue.relatedObjects != null && kalturaKeyValue.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaKeyValue.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"key\": " + "\"" + kalturaKeyValue.key + "\"";
                    ret += ", \"value\": " + "\"" + kalturaKeyValue.value + "\"";
                    break;
                    
                case "KalturaLanguage":
                    KalturaLanguage kalturaLanguage = ottObject as KalturaLanguage;
                    ret += "\"objectType\": " + "\"" + kalturaLanguage.objectType + "\"";
                    if(kalturaLanguage.relatedObjects != null && kalturaLanguage.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaLanguage.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"code\": " + "\"" + kalturaLanguage.Code + "\"";
                    ret += ", \"direction\": " + "\"" + kalturaLanguage.Direction + "\"";
                    ret += ", \"isDefault\": " + kalturaLanguage.IsDefault;
                    ret += ", \"name\": " + "\"" + kalturaLanguage.Name + "\"";
                    ret += ", \"systemName\": " + "\"" + kalturaLanguage.SystemName + "\"";
                    break;
                    
                case "KalturaLanguageFilter":
                    KalturaLanguageFilter kalturaLanguageFilter = ottObject as KalturaLanguageFilter;
                    ret += "\"objectType\": " + "\"" + kalturaLanguageFilter.objectType + "\"";
                    if(kalturaLanguageFilter.relatedObjects != null && kalturaLanguageFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaLanguageFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaLanguageFilter.OrderBy;
                    ret += ", \"codeIn\": " + "\"" + kalturaLanguageFilter.CodeIn + "\"";
                    break;
                    
                case "KalturaLanguageListResponse":
                    KalturaLanguageListResponse kalturaLanguageListResponse = ottObject as KalturaLanguageListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaLanguageListResponse.objectType + "\"";
                    if(kalturaLanguageListResponse.relatedObjects != null && kalturaLanguageListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaLanguageListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaLanguageListResponse.TotalCount;
                    if(kalturaLanguageListResponse.Objects != null && kalturaLanguageListResponse.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaLanguageListResponse.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaLastPosition":
                    KalturaLastPosition kalturaLastPosition = ottObject as KalturaLastPosition;
                    ret += "\"objectType\": " + "\"" + kalturaLastPosition.objectType + "\"";
                    if(kalturaLastPosition.relatedObjects != null && kalturaLastPosition.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaLastPosition.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"position\": " + kalturaLastPosition.Position;
                    ret += ", \"position_owner\": " + kalturaLastPosition.PositionOwner;
                    ret += ", \"user_id\": " + "\"" + kalturaLastPosition.UserId + "\"";
                    break;
                    
                case "KalturaLastPositionFilter":
                    KalturaLastPositionFilter kalturaLastPositionFilter = ottObject as KalturaLastPositionFilter;
                    ret += "\"objectType\": " + "\"" + kalturaLastPositionFilter.objectType + "\"";
                    if(kalturaLastPositionFilter.relatedObjects != null && kalturaLastPositionFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaLastPositionFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"by\": " + kalturaLastPositionFilter.By;
                    if(kalturaLastPositionFilter.Ids != null && kalturaLastPositionFilter.Ids.Count > 0)
                    {
                        ret += ", \"ids\": " + "[" + String.Join(", ", kalturaLastPositionFilter.Ids.Select(item => Serialize(item))) + "]";
                    }
                    ret += ", \"type\": " + kalturaLastPositionFilter.Type;
                    break;
                    
                case "KalturaLastPositionListResponse":
                    KalturaLastPositionListResponse kalturaLastPositionListResponse = ottObject as KalturaLastPositionListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaLastPositionListResponse.objectType + "\"";
                    if(kalturaLastPositionListResponse.relatedObjects != null && kalturaLastPositionListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaLastPositionListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaLastPositionListResponse.TotalCount;
                    if(kalturaLastPositionListResponse.LastPositions != null && kalturaLastPositionListResponse.LastPositions.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaLastPositionListResponse.LastPositions.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaLicensedUrl":
                    KalturaLicensedUrl kalturaLicensedUrl = ottObject as KalturaLicensedUrl;
                    ret += "\"objectType\": " + "\"" + kalturaLicensedUrl.objectType + "\"";
                    if(kalturaLicensedUrl.relatedObjects != null && kalturaLicensedUrl.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaLicensedUrl.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"altUrl\": " + "\"" + kalturaLicensedUrl.AltUrl + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"alt_url\": " + "\"" + kalturaLicensedUrl.AltUrl + "\"";
                    }
                    ret += ", \"mainUrl\": " + "\"" + kalturaLicensedUrl.MainUrl + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"main_url\": " + "\"" + kalturaLicensedUrl.MainUrl + "\"";
                    }
                    break;
                    
                case "KalturaLicensedUrlBaseRequest":
                    KalturaLicensedUrlBaseRequest kalturaLicensedUrlBaseRequest = ottObject as KalturaLicensedUrlBaseRequest;
                    ret += "\"objectType\": " + "\"" + kalturaLicensedUrlBaseRequest.objectType + "\"";
                    if(kalturaLicensedUrlBaseRequest.relatedObjects != null && kalturaLicensedUrlBaseRequest.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaLicensedUrlBaseRequest.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"assetId\": " + "\"" + kalturaLicensedUrlBaseRequest.AssetId + "\"";
                    break;
                    
                case "KalturaLicensedUrlEpgRequest":
                    KalturaLicensedUrlEpgRequest kalturaLicensedUrlEpgRequest = ottObject as KalturaLicensedUrlEpgRequest;
                    ret += "\"objectType\": " + "\"" + kalturaLicensedUrlEpgRequest.objectType + "\"";
                    if(kalturaLicensedUrlEpgRequest.relatedObjects != null && kalturaLicensedUrlEpgRequest.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaLicensedUrlEpgRequest.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"assetId\": " + "\"" + kalturaLicensedUrlEpgRequest.AssetId + "\"";
                    ret += ", \"baseUrl\": " + "\"" + kalturaLicensedUrlEpgRequest.BaseUrl + "\"";
                    ret += ", \"contentId\": " + kalturaLicensedUrlEpgRequest.ContentId;
                    ret += ", \"startDate\": " + kalturaLicensedUrlEpgRequest.StartDate;
                    ret += ", \"streamType\": " + kalturaLicensedUrlEpgRequest.StreamType;
                    break;
                    
                case "KalturaLicensedUrlMediaRequest":
                    KalturaLicensedUrlMediaRequest kalturaLicensedUrlMediaRequest = ottObject as KalturaLicensedUrlMediaRequest;
                    ret += "\"objectType\": " + "\"" + kalturaLicensedUrlMediaRequest.objectType + "\"";
                    if(kalturaLicensedUrlMediaRequest.relatedObjects != null && kalturaLicensedUrlMediaRequest.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaLicensedUrlMediaRequest.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"assetId\": " + "\"" + kalturaLicensedUrlMediaRequest.AssetId + "\"";
                    ret += ", \"baseUrl\": " + "\"" + kalturaLicensedUrlMediaRequest.BaseUrl + "\"";
                    ret += ", \"contentId\": " + kalturaLicensedUrlMediaRequest.ContentId;
                    break;
                    
                case "KalturaLicensedUrlRecordingRequest":
                    KalturaLicensedUrlRecordingRequest kalturaLicensedUrlRecordingRequest = ottObject as KalturaLicensedUrlRecordingRequest;
                    ret += "\"objectType\": " + "\"" + kalturaLicensedUrlRecordingRequest.objectType + "\"";
                    if(kalturaLicensedUrlRecordingRequest.relatedObjects != null && kalturaLicensedUrlRecordingRequest.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaLicensedUrlRecordingRequest.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"assetId\": " + "\"" + kalturaLicensedUrlRecordingRequest.AssetId + "\"";
                    ret += ", \"fileType\": " + "\"" + kalturaLicensedUrlRecordingRequest.FileType + "\"";
                    break;
                    
                case "KalturaListFollowDataTvSeriesResponse":
                    KalturaListFollowDataTvSeriesResponse kalturaListFollowDataTvSeriesResponse = ottObject as KalturaListFollowDataTvSeriesResponse;
                    ret += "\"objectType\": " + "\"" + kalturaListFollowDataTvSeriesResponse.objectType + "\"";
                    if(kalturaListFollowDataTvSeriesResponse.relatedObjects != null && kalturaListFollowDataTvSeriesResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaListFollowDataTvSeriesResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaListFollowDataTvSeriesResponse.TotalCount;
                    if(kalturaListFollowDataTvSeriesResponse.FollowDataList != null && kalturaListFollowDataTvSeriesResponse.FollowDataList.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaListFollowDataTvSeriesResponse.FollowDataList.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaListResponse":
                    KalturaListResponse kalturaListResponse = ottObject as KalturaListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaListResponse.objectType + "\"";
                    if(kalturaListResponse.relatedObjects != null && kalturaListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaListResponse.TotalCount;
                    break;
                    
                case "KalturaLoginResponse":
                    KalturaLoginResponse kalturaLoginResponse = ottObject as KalturaLoginResponse;
                    ret += "\"objectType\": " + "\"" + kalturaLoginResponse.objectType + "\"";
                    if(kalturaLoginResponse.relatedObjects != null && kalturaLoginResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaLoginResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"loginSession\": " + Serialize(kalturaLoginResponse.LoginSession);
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"login_session\": " + Serialize(kalturaLoginResponse.LoginSession);
                    }
                    ret += ", \"user\": " + Serialize(kalturaLoginResponse.User);
                    break;
                    
                case "KalturaLoginSession":
                    KalturaLoginSession kalturaLoginSession = ottObject as KalturaLoginSession;
                    ret += "\"objectType\": " + "\"" + kalturaLoginSession.objectType + "\"";
                    if(kalturaLoginSession.relatedObjects != null && kalturaLoginSession.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaLoginSession.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"ks\": " + "\"" + kalturaLoginSession.KS + "\"";
                    ret += ", \"refreshToken\": " + "\"" + kalturaLoginSession.RefreshToken + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"refresh_token\": " + "\"" + kalturaLoginSession.RefreshToken + "\"";
                    }
                    break;
                    
                case "KalturaLongValue":
                    KalturaLongValue kalturaLongValue = ottObject as KalturaLongValue;
                    ret += "\"objectType\": " + "\"" + kalturaLongValue.objectType + "\"";
                    if(kalturaLongValue.relatedObjects != null && kalturaLongValue.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaLongValue.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"description\": " + "\"" + kalturaLongValue.description + "\"";
                    ret += ", \"value\": " + kalturaLongValue.value;
                    break;
                    
                case "KalturaMediaAsset":
                    KalturaMediaAsset kalturaMediaAsset = ottObject as KalturaMediaAsset;
                    ret += "\"objectType\": " + "\"" + kalturaMediaAsset.objectType + "\"";
                    if(kalturaMediaAsset.relatedObjects != null && kalturaMediaAsset.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaMediaAsset.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"description\": " + Serialize(kalturaMediaAsset.Description);
                    if(kalturaMediaAsset.EnableCatchUp.HasValue)
                    {
                        ret += ", \"enableCatchUp\": " + kalturaMediaAsset.EnableCatchUp;
                    }
                    if(kalturaMediaAsset.EnableCdvr.HasValue)
                    {
                        ret += ", \"enableCdvr\": " + kalturaMediaAsset.EnableCdvr;
                    }
                    if(kalturaMediaAsset.EnableStartOver.HasValue)
                    {
                        ret += ", \"enableStartOver\": " + kalturaMediaAsset.EnableStartOver;
                    }
                    if(kalturaMediaAsset.EnableTrickPlay.HasValue)
                    {
                        ret += ", \"enableTrickPlay\": " + kalturaMediaAsset.EnableTrickPlay;
                    }
                    if(kalturaMediaAsset.EndDate.HasValue)
                    {
                        ret += ", \"endDate\": " + kalturaMediaAsset.EndDate;
                    }
                    ret += ", \"externalId\": " + "\"" + kalturaMediaAsset.ExternalId + "\"";
                    if(kalturaMediaAsset.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaMediaAsset.Id;
                    }
                    if(kalturaMediaAsset.Images != null && kalturaMediaAsset.Images.Count > 0)
                    {
                        ret += ", \"images\": " + "[" + String.Join(", ", kalturaMediaAsset.Images.Select(item => Serialize(item))) + "]";
                    }
                    if(kalturaMediaAsset.MediaFiles != null && kalturaMediaAsset.MediaFiles.Count > 0)
                    {
                        ret += ", \"mediaFiles\": " + "[" + String.Join(", ", kalturaMediaAsset.MediaFiles.Select(item => Serialize(item))) + "]";
                    }
                    if(kalturaMediaAsset.Metas != null && kalturaMediaAsset.Metas.Count > 0)
                    {
                        ret += ", \"metas\": " + "{" + String.Join(", ", kalturaMediaAsset.Metas.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"name\": " + Serialize(kalturaMediaAsset.Name);
                    if(kalturaMediaAsset.StartDate.HasValue)
                    {
                        ret += ", \"startDate\": " + kalturaMediaAsset.StartDate;
                    }
                    ret += ", \"stats\": " + Serialize(kalturaMediaAsset.Statistics);
                    if(kalturaMediaAsset.Tags != null && kalturaMediaAsset.Tags.Count > 0)
                    {
                        ret += ", \"tags\": " + "{" + String.Join(", ", kalturaMediaAsset.Tags.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaMediaAsset.Type.HasValue)
                    {
                        ret += ", \"type\": " + kalturaMediaAsset.Type;
                    }
                    if(kalturaMediaAsset.CatchUpBuffer.HasValue)
                    {
                        ret += ", \"catchUpBuffer\": " + kalturaMediaAsset.CatchUpBuffer;
                    }
                    ret += ", \"deviceRule\": " + "\"" + kalturaMediaAsset.DeviceRule + "\"";
                    if(kalturaMediaAsset.EnableRecordingPlaybackNonEntitledChannel.HasValue)
                    {
                        ret += ", \"enableRecordingPlaybackNonEntitledChannel\": " + kalturaMediaAsset.EnableRecordingPlaybackNonEntitledChannel;
                    }
                    ret += ", \"entryId\": " + "\"" + kalturaMediaAsset.EntryId + "\"";
                    ret += ", \"externalIds\": " + "\"" + kalturaMediaAsset.ExternalIds + "\"";
                    ret += ", \"geoBlockRule\": " + "\"" + kalturaMediaAsset.GeoBlockRule + "\"";
                    if(kalturaMediaAsset.TrickPlayBuffer.HasValue)
                    {
                        ret += ", \"trickPlayBuffer\": " + kalturaMediaAsset.TrickPlayBuffer;
                    }
                    ret += ", \"typeDescription\": " + "\"" + kalturaMediaAsset.TypeDescription + "\"";
                    ret += ", \"watchPermissionRule\": " + "\"" + kalturaMediaAsset.WatchPermissionRule + "\"";
                    break;
                    
                case "KalturaMediaFile":
                    KalturaMediaFile kalturaMediaFile = ottObject as KalturaMediaFile;
                    ret += "\"objectType\": " + "\"" + kalturaMediaFile.objectType + "\"";
                    if(kalturaMediaFile.relatedObjects != null && kalturaMediaFile.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaMediaFile.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"altCdnCode\": " + "\"" + kalturaMediaFile.AltCdnCode + "\"";
                    if(kalturaMediaFile.AssetId.HasValue)
                    {
                        ret += ", \"assetId\": " + kalturaMediaFile.AssetId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"asset_id\": " + kalturaMediaFile.AssetId;
                        }
                    }
                    ret += ", \"billingType\": " + "\"" + kalturaMediaFile.BillingType + "\"";
                    ret += ", \"cdnCode\": " + "\"" + kalturaMediaFile.CdnCode + "\"";
                    ret += ", \"cdnName\": " + "\"" + kalturaMediaFile.CdnName + "\"";
                    if(kalturaMediaFile.Duration.HasValue)
                    {
                        ret += ", \"duration\": " + kalturaMediaFile.Duration;
                    }
                    ret += ", \"externalId\": " + "\"" + kalturaMediaFile.ExternalId + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"external_id\": " + "\"" + kalturaMediaFile.ExternalId + "\"";
                    }
                    ret += ", \"fileSize\": " + kalturaMediaFile.FileSize;
                    ret += ", \"handlingType\": " + "\"" + kalturaMediaFile.HandlingType + "\"";
                    if(kalturaMediaFile.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaMediaFile.Id;
                    }
                    ret += ", \"ppvModules\": " + Serialize(kalturaMediaFile.PPVModules);
                    ret += ", \"productCode\": " + "\"" + kalturaMediaFile.ProductCode + "\"";
                    ret += ", \"quality\": " + "\"" + kalturaMediaFile.Quality + "\"";
                    ret += ", \"type\": " + "\"" + kalturaMediaFile.Type + "\"";
                    ret += ", \"url\": " + "\"" + kalturaMediaFile.Url + "\"";
                    break;
                    
                case "KalturaMediaImage":
                    KalturaMediaImage kalturaMediaImage = ottObject as KalturaMediaImage;
                    ret += "\"objectType\": " + "\"" + kalturaMediaImage.objectType + "\"";
                    if(kalturaMediaImage.relatedObjects != null && kalturaMediaImage.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaMediaImage.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaMediaImage.Height.HasValue)
                    {
                        ret += ", \"height\": " + kalturaMediaImage.Height;
                    }
                    ret += ", \"id\": " + "\"" + kalturaMediaImage.Id + "\"";
                    if(kalturaMediaImage.IsDefault.HasValue)
                    {
                        ret += ", \"isDefault\": " + kalturaMediaImage.IsDefault;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_default\": " + kalturaMediaImage.IsDefault;
                        }
                    }
                    ret += ", \"ratio\": " + "\"" + kalturaMediaImage.Ratio + "\"";
                    ret += ", \"url\": " + "\"" + kalturaMediaImage.Url + "\"";
                    if(kalturaMediaImage.Version.HasValue)
                    {
                        ret += ", \"version\": " + kalturaMediaImage.Version;
                    }
                    if(kalturaMediaImage.Width.HasValue)
                    {
                        ret += ", \"width\": " + kalturaMediaImage.Width;
                    }
                    break;
                    
                case "KalturaMessageAnnouncementListResponse":
                    KalturaMessageAnnouncementListResponse kalturaMessageAnnouncementListResponse = ottObject as KalturaMessageAnnouncementListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaMessageAnnouncementListResponse.objectType + "\"";
                    if(kalturaMessageAnnouncementListResponse.relatedObjects != null && kalturaMessageAnnouncementListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaMessageAnnouncementListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaMessageAnnouncementListResponse.TotalCount;
                    if(kalturaMessageAnnouncementListResponse.Announcements != null && kalturaMessageAnnouncementListResponse.Announcements.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaMessageAnnouncementListResponse.Announcements.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaMessageTemplate":
                    KalturaMessageTemplate kalturaMessageTemplate = ottObject as KalturaMessageTemplate;
                    ret += "\"objectType\": " + "\"" + kalturaMessageTemplate.objectType + "\"";
                    if(kalturaMessageTemplate.relatedObjects != null && kalturaMessageTemplate.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaMessageTemplate.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"action\": " + "\"" + kalturaMessageTemplate.Action + "\"";
                    ret += ", \"dateFormat\": " + "\"" + kalturaMessageTemplate.DateFormat + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"date_format\": " + "\"" + kalturaMessageTemplate.DateFormat + "\"";
                    }
                    ret += ", \"message\": " + "\"" + kalturaMessageTemplate.Message + "\"";
                    ret += ", \"messageType\": " + kalturaMessageTemplate.MessageType;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"asset_type\": " + kalturaMessageTemplate.MessageType;
                    }
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0 || currentVersion.CompareTo(new Version("3.6.2094.15157")) > 0)
                    {
                        ret += ", \"assetType\": " + kalturaMessageTemplate.MessageType;
                    }
                    ret += ", \"sound\": " + "\"" + kalturaMessageTemplate.Sound + "\"";
                    ret += ", \"url\": " + "\"" + kalturaMessageTemplate.URL + "\"";
                    break;
                    
                case "KalturaMeta":
                    KalturaMeta kalturaMeta = ottObject as KalturaMeta;
                    ret += "\"objectType\": " + "\"" + kalturaMeta.objectType + "\"";
                    if(kalturaMeta.relatedObjects != null && kalturaMeta.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaMeta.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"assetType\": " + kalturaMeta.AssetType;
                    ret += ", \"features\": " + "\"" + kalturaMeta.Features + "\"";
                    ret += ", \"fieldName\": " + kalturaMeta.FieldName;
                    ret += ", \"id\": " + "\"" + kalturaMeta.Id + "\"";
                    ret += ", \"name\": " + "\"" + kalturaMeta.Name + "\"";
                    ret += ", \"parentId\": " + "\"" + kalturaMeta.ParentId + "\"";
                    ret += ", \"partnerId\": " + kalturaMeta.PartnerId;
                    ret += ", \"type\": " + kalturaMeta.Type;
                    break;
                    
                case "KalturaMetaFilter":
                    KalturaMetaFilter kalturaMetaFilter = ottObject as KalturaMetaFilter;
                    ret += "\"objectType\": " + "\"" + kalturaMetaFilter.objectType + "\"";
                    if(kalturaMetaFilter.relatedObjects != null && kalturaMetaFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaMetaFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaMetaFilter.OrderBy;
                    if(kalturaMetaFilter.AssetTypeEqual.HasValue)
                    {
                        ret += ", \"assetTypeEqual\": " + kalturaMetaFilter.AssetTypeEqual;
                    }
                    ret += ", \"featuresIn\": " + "\"" + kalturaMetaFilter.FeaturesIn + "\"";
                    if(kalturaMetaFilter.FieldNameEqual.HasValue)
                    {
                        ret += ", \"fieldNameEqual\": " + kalturaMetaFilter.FieldNameEqual;
                    }
                    if(kalturaMetaFilter.FieldNameNotEqual.HasValue)
                    {
                        ret += ", \"fieldNameNotEqual\": " + kalturaMetaFilter.FieldNameNotEqual;
                    }
                    if(kalturaMetaFilter.TypeEqual.HasValue)
                    {
                        ret += ", \"typeEqual\": " + kalturaMetaFilter.TypeEqual;
                    }
                    break;
                    
                case "KalturaMetaListResponse":
                    KalturaMetaListResponse kalturaMetaListResponse = ottObject as KalturaMetaListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaMetaListResponse.objectType + "\"";
                    if(kalturaMetaListResponse.relatedObjects != null && kalturaMetaListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaMetaListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaMetaListResponse.TotalCount;
                    if(kalturaMetaListResponse.Objects != null && kalturaMetaListResponse.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaMetaListResponse.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaMultilingualString":
                    KalturaMultilingualString kalturaMultilingualString = ottObject as KalturaMultilingualString;
                    ret += "\"objectType\": " + "\"" + kalturaMultilingualString.objectType + "\"";
                    if(kalturaMultilingualString.relatedObjects != null && kalturaMultilingualString.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaMultilingualString.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaMultilingualString.Values != null && kalturaMultilingualString.Values.Count > 0)
                    {
                        ret += ", \"values\": " + "[" + String.Join(", ", kalturaMultilingualString.Values.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaMultilingualStringValue":
                    KalturaMultilingualStringValue kalturaMultilingualStringValue = ottObject as KalturaMultilingualStringValue;
                    ret += "\"objectType\": " + "\"" + kalturaMultilingualStringValue.objectType + "\"";
                    if(kalturaMultilingualStringValue.relatedObjects != null && kalturaMultilingualStringValue.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaMultilingualStringValue.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"description\": " + "\"" + kalturaMultilingualStringValue.description + "\"";
                    ret += ", \"value\": " + Serialize(kalturaMultilingualStringValue.value);
                    break;
                    
                case "KalturaMultilingualStringValueArray":
                    KalturaMultilingualStringValueArray kalturaMultilingualStringValueArray = ottObject as KalturaMultilingualStringValueArray;
                    ret += "\"objectType\": " + "\"" + kalturaMultilingualStringValueArray.objectType + "\"";
                    if(kalturaMultilingualStringValueArray.relatedObjects != null && kalturaMultilingualStringValueArray.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaMultilingualStringValueArray.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaMultilingualStringValueArray.Objects != null && kalturaMultilingualStringValueArray.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaMultilingualStringValueArray.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaNetworkActionStatus":
                    KalturaNetworkActionStatus kalturaNetworkActionStatus = ottObject as KalturaNetworkActionStatus;
                    ret += "\"objectType\": " + "\"" + kalturaNetworkActionStatus.objectType + "\"";
                    if(kalturaNetworkActionStatus.relatedObjects != null && kalturaNetworkActionStatus.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaNetworkActionStatus.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaNetworkActionStatus.Network.HasValue)
                    {
                        ret += ", \"network\": " + kalturaNetworkActionStatus.Network;
                    }
                    ret += ", \"status\": " + kalturaNetworkActionStatus.Status;
                    break;
                    
                case "KalturaNotification":
                    KalturaNotification kalturaNotification = ottObject as KalturaNotification;
                    ret += "\"objectType\": " + "\"" + kalturaNotification.objectType + "\"";
                    if(kalturaNotification.relatedObjects != null && kalturaNotification.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaNotification.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"object\": " + Serialize(kalturaNotification.eventObject);
                    ret += ", \"eventObjectType\": " + "\"" + kalturaNotification.eventObjectType + "\"";
                    if(kalturaNotification.eventType.HasValue)
                    {
                        ret += ", \"eventType\": " + kalturaNotification.eventType;
                    }
                    ret += ", \"systemName\": " + "\"" + kalturaNotification.systemName + "\"";
                    break;
                    
                case "KalturaNotificationSettings":
                    KalturaNotificationSettings kalturaNotificationSettings = ottObject as KalturaNotificationSettings;
                    ret += "\"objectType\": " + "\"" + kalturaNotificationSettings.objectType + "\"";
                    if(kalturaNotificationSettings.relatedObjects != null && kalturaNotificationSettings.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaNotificationSettings.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaNotificationSettings.PushFollowEnabled.HasValue)
                    {
                        ret += ", \"pushFollowEnabled\": " + kalturaNotificationSettings.PushFollowEnabled;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"push_follow_enabled\": " + kalturaNotificationSettings.PushFollowEnabled;
                        }
                    }
                    if(kalturaNotificationSettings.PushNotificationEnabled.HasValue)
                    {
                        ret += ", \"pushNotificationEnabled\": " + kalturaNotificationSettings.PushNotificationEnabled;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"push_notification_enabled\": " + kalturaNotificationSettings.PushNotificationEnabled;
                        }
                    }
                    break;
                    
                case "KalturaNotificationsPartnerSettings":
                    KalturaNotificationsPartnerSettings kalturaNotificationsPartnerSettings = ottObject as KalturaNotificationsPartnerSettings;
                    ret += "\"objectType\": " + "\"" + kalturaNotificationsPartnerSettings.objectType + "\"";
                    if(kalturaNotificationsPartnerSettings.relatedObjects != null && kalturaNotificationsPartnerSettings.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaNotificationsPartnerSettings.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaNotificationsPartnerSettings.AutomaticIssueFollowNotification.HasValue)
                    {
                        ret += ", \"automaticIssueFollowNotification\": " + kalturaNotificationsPartnerSettings.AutomaticIssueFollowNotification;
                    }
                    ret += ", \"churnMailSubject\": " + "\"" + kalturaNotificationsPartnerSettings.ChurnMailSubject + "\"";
                    ret += ", \"churnMailTemplateName\": " + "\"" + kalturaNotificationsPartnerSettings.ChurnMailTemplateName + "\"";
                    if(kalturaNotificationsPartnerSettings.InboxEnabled.HasValue)
                    {
                        ret += ", \"inboxEnabled\": " + kalturaNotificationsPartnerSettings.InboxEnabled;
                    }
                    ret += ", \"mailSenderName\": " + "\"" + kalturaNotificationsPartnerSettings.MailSenderName + "\"";
                    if(kalturaNotificationsPartnerSettings.MessageTTLDays.HasValue)
                    {
                        ret += ", \"messageTTLDays\": " + kalturaNotificationsPartnerSettings.MessageTTLDays;
                    }
                    ret += ", \"pushAdapterUrl\": " + "\"" + kalturaNotificationsPartnerSettings.PushAdapterUrl + "\"";
                    if(kalturaNotificationsPartnerSettings.PushEndHour.HasValue)
                    {
                        ret += ", \"pushEndHour\": " + kalturaNotificationsPartnerSettings.PushEndHour;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"push_end_hour\": " + kalturaNotificationsPartnerSettings.PushEndHour;
                        }
                    }
                    if(kalturaNotificationsPartnerSettings.PushNotificationEnabled.HasValue)
                    {
                        ret += ", \"pushNotificationEnabled\": " + kalturaNotificationsPartnerSettings.PushNotificationEnabled;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"push_notification_enabled\": " + kalturaNotificationsPartnerSettings.PushNotificationEnabled;
                        }
                    }
                    if(kalturaNotificationsPartnerSettings.PushStartHour.HasValue)
                    {
                        ret += ", \"pushStartHour\": " + kalturaNotificationsPartnerSettings.PushStartHour;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"push_start_hour\": " + kalturaNotificationsPartnerSettings.PushStartHour;
                        }
                    }
                    if(kalturaNotificationsPartnerSettings.PushSystemAnnouncementsEnabled.HasValue)
                    {
                        ret += ", \"pushSystemAnnouncementsEnabled\": " + kalturaNotificationsPartnerSettings.PushSystemAnnouncementsEnabled;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"push_system_announcements_enabled\": " + kalturaNotificationsPartnerSettings.PushSystemAnnouncementsEnabled;
                        }
                    }
                    if(kalturaNotificationsPartnerSettings.ReminderEnabled.HasValue)
                    {
                        ret += ", \"reminderEnabled\": " + kalturaNotificationsPartnerSettings.ReminderEnabled;
                    }
                    if(kalturaNotificationsPartnerSettings.ReminderOffset.HasValue)
                    {
                        ret += ", \"reminderOffsetSec\": " + kalturaNotificationsPartnerSettings.ReminderOffset;
                    }
                    ret += ", \"senderEmail\": " + "\"" + kalturaNotificationsPartnerSettings.SenderEmail + "\"";
                    if(kalturaNotificationsPartnerSettings.TopicExpirationDurationDays.HasValue)
                    {
                        ret += ", \"topicExpirationDurationDays\": " + kalturaNotificationsPartnerSettings.TopicExpirationDurationDays;
                    }
                    break;
                    
                case "KalturaNotificationsSettings":
                    KalturaNotificationsSettings kalturaNotificationsSettings = ottObject as KalturaNotificationsSettings;
                    ret += "\"objectType\": " + "\"" + kalturaNotificationsSettings.objectType + "\"";
                    if(kalturaNotificationsSettings.relatedObjects != null && kalturaNotificationsSettings.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaNotificationsSettings.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaNotificationsSettings.PushFollowEnabled.HasValue)
                    {
                        ret += ", \"pushFollowEnabled\": " + kalturaNotificationsSettings.PushFollowEnabled;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"push_follow_enabled\": " + kalturaNotificationsSettings.PushFollowEnabled;
                        }
                    }
                    if(kalturaNotificationsSettings.PushNotificationEnabled.HasValue)
                    {
                        ret += ", \"pushNotificationEnabled\": " + kalturaNotificationsSettings.PushNotificationEnabled;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"push_notification_enabled\": " + kalturaNotificationsSettings.PushNotificationEnabled;
                        }
                    }
                    break;
                    
                case "KalturaNpvrPremiumService":
                    KalturaNpvrPremiumService kalturaNpvrPremiumService = ottObject as KalturaNpvrPremiumService;
                    ret += "\"objectType\": " + "\"" + kalturaNpvrPremiumService.objectType + "\"";
                    if(kalturaNpvrPremiumService.relatedObjects != null && kalturaNpvrPremiumService.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaNpvrPremiumService.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaNpvrPremiumService.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaNpvrPremiumService.Id;
                    }
                    ret += ", \"name\": " + "\"" + kalturaNpvrPremiumService.Name + "\"";
                    if(kalturaNpvrPremiumService.QuotaInMinutes.HasValue)
                    {
                        ret += ", \"quotaInMinutes\": " + kalturaNpvrPremiumService.QuotaInMinutes;
                    }
                    break;
                    
                case "KalturaOSSAdapterBaseProfile":
                    KalturaOSSAdapterBaseProfile kalturaOSSAdapterBaseProfile = ottObject as KalturaOSSAdapterBaseProfile;
                    ret += "\"objectType\": " + "\"" + kalturaOSSAdapterBaseProfile.objectType + "\"";
                    if(kalturaOSSAdapterBaseProfile.relatedObjects != null && kalturaOSSAdapterBaseProfile.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaOSSAdapterBaseProfile.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaOSSAdapterBaseProfile.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaOSSAdapterBaseProfile.Id;
                    }
                    ret += ", \"name\": " + "\"" + kalturaOSSAdapterBaseProfile.Name + "\"";
                    break;
                    
                case "KalturaOSSAdapterProfile":
                    KalturaOSSAdapterProfile kalturaOSSAdapterProfile = ottObject as KalturaOSSAdapterProfile;
                    ret += "\"objectType\": " + "\"" + kalturaOSSAdapterProfile.objectType + "\"";
                    if(kalturaOSSAdapterProfile.relatedObjects != null && kalturaOSSAdapterProfile.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaOSSAdapterProfile.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaOSSAdapterProfile.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaOSSAdapterProfile.Id;
                    }
                    ret += ", \"name\": " + "\"" + kalturaOSSAdapterProfile.Name + "\"";
                    ret += ", \"adapterUrl\": " + "\"" + kalturaOSSAdapterProfile.AdapterUrl + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"adapter_url\": " + "\"" + kalturaOSSAdapterProfile.AdapterUrl + "\"";
                    }
                    ret += ", \"externalIdentifier\": " + "\"" + kalturaOSSAdapterProfile.ExternalIdentifier + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"external_identifier\": " + "\"" + kalturaOSSAdapterProfile.ExternalIdentifier + "\"";
                    }
                    if(kalturaOSSAdapterProfile.IsActive.HasValue)
                    {
                        ret += ", \"isActive\": " + kalturaOSSAdapterProfile.IsActive;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_active\": " + kalturaOSSAdapterProfile.IsActive;
                        }
                    }
                    if(kalturaOSSAdapterProfile.Settings != null && kalturaOSSAdapterProfile.Settings.Count > 0)
                    {
                        ret += ", \"ossAdapterSettings\": " + "{" + String.Join(", ", kalturaOSSAdapterProfile.Settings.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"oss_adapter_settings\": " + "{" + String.Join(", ", kalturaOSSAdapterProfile.Settings.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                        }
                    }
                    ret += ", \"sharedSecret\": " + "\"" + kalturaOSSAdapterProfile.SharedSecret + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"shared_secret\": " + "\"" + kalturaOSSAdapterProfile.SharedSecret + "\"";
                    }
                    break;
                    
                case "KalturaOSSAdapterProfileListResponse":
                    KalturaOSSAdapterProfileListResponse kalturaOSSAdapterProfileListResponse = ottObject as KalturaOSSAdapterProfileListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaOSSAdapterProfileListResponse.objectType + "\"";
                    if(kalturaOSSAdapterProfileListResponse.relatedObjects != null && kalturaOSSAdapterProfileListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaOSSAdapterProfileListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaOSSAdapterProfileListResponse.TotalCount;
                    if(kalturaOSSAdapterProfileListResponse.OSSAdapterProfiles != null && kalturaOSSAdapterProfileListResponse.OSSAdapterProfiles.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaOSSAdapterProfileListResponse.OSSAdapterProfiles.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaOTTCategory":
                    KalturaOTTCategory kalturaOTTCategory = ottObject as KalturaOTTCategory;
                    ret += "\"objectType\": " + "\"" + kalturaOTTCategory.objectType + "\"";
                    if(kalturaOTTCategory.relatedObjects != null && kalturaOTTCategory.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaOTTCategory.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaOTTCategory.Channels != null && kalturaOTTCategory.Channels.Count > 0)
                    {
                        ret += ", \"channels\": " + "[" + String.Join(", ", kalturaOTTCategory.Channels.Select(item => Serialize(item))) + "]";
                    }
                    if(kalturaOTTCategory.ChildCategories != null && kalturaOTTCategory.ChildCategories.Count > 0)
                    {
                        ret += ", \"childCategories\": " + "[" + String.Join(", ", kalturaOTTCategory.ChildCategories.Select(item => Serialize(item))) + "]";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"child_categories\": " + "[" + String.Join(", ", kalturaOTTCategory.ChildCategories.Select(item => Serialize(item))) + "]";
                        }
                    }
                    if(kalturaOTTCategory.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaOTTCategory.Id;
                    }
                    if(kalturaOTTCategory.Images != null && kalturaOTTCategory.Images.Count > 0)
                    {
                        ret += ", \"images\": " + "[" + String.Join(", ", kalturaOTTCategory.Images.Select(item => Serialize(item))) + "]";
                    }
                    ret += ", \"name\": " + "\"" + kalturaOTTCategory.Name + "\"";
                    if(kalturaOTTCategory.ParentCategoryId.HasValue)
                    {
                        ret += ", \"parentCategoryId\": " + kalturaOTTCategory.ParentCategoryId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"parent_category_id\": " + kalturaOTTCategory.ParentCategoryId;
                        }
                    }
                    break;
                    
                case "KalturaOTTObject":
                    KalturaOTTObject kalturaOTTObject = ottObject as KalturaOTTObject;
                    ret += "\"objectType\": " + "\"" + kalturaOTTObject.objectType + "\"";
                    if(kalturaOTTObject.relatedObjects != null && kalturaOTTObject.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaOTTObject.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    break;
                    
                case "KalturaOTTUser":
                    KalturaOTTUser kalturaOTTUser = ottObject as KalturaOTTUser;
                    ret += "\"objectType\": " + "\"" + kalturaOTTUser.objectType + "\"";
                    if(kalturaOTTUser.relatedObjects != null && kalturaOTTUser.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaOTTUser.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"firstName\": " + "\"" + kalturaOTTUser.FirstName + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"first_name\": " + "\"" + kalturaOTTUser.FirstName + "\"";
                    }
                    ret += ", \"id\": " + "\"" + kalturaOTTUser.Id + "\"";
                    ret += ", \"lastName\": " + "\"" + kalturaOTTUser.LastName + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"last_name\": " + "\"" + kalturaOTTUser.LastName + "\"";
                    }
                    ret += ", \"username\": " + "\"" + kalturaOTTUser.Username + "\"";
                    ret += ", \"address\": " + "\"" + kalturaOTTUser.Address + "\"";
                    ret += ", \"affiliateCode\": " + "\"" + kalturaOTTUser.AffiliateCode + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"affiliate_code\": " + "\"" + kalturaOTTUser.AffiliateCode + "\"";
                    }
                    ret += ", \"city\": " + "\"" + kalturaOTTUser.City + "\"";
                    ret += ", \"country\": " + Serialize(kalturaOTTUser.Country);
                    if(kalturaOTTUser.CountryId.HasValue)
                    {
                        ret += ", \"countryId\": " + kalturaOTTUser.CountryId;
                    }
                    if(kalturaOTTUser.DynamicData != null && kalturaOTTUser.DynamicData.Count > 0)
                    {
                        ret += ", \"dynamicData\": " + "{" + String.Join(", ", kalturaOTTUser.DynamicData.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"dynamic_data\": " + "{" + String.Join(", ", kalturaOTTUser.DynamicData.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                        }
                    }
                    ret += ", \"email\": " + "\"" + kalturaOTTUser.Email + "\"";
                    ret += ", \"externalId\": " + "\"" + kalturaOTTUser.ExternalId + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"external_id\": " + "\"" + kalturaOTTUser.ExternalId + "\"";
                    }
                    ret += ", \"facebookId\": " + "\"" + kalturaOTTUser.FacebookId + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"facebook_id\": " + "\"" + kalturaOTTUser.FacebookId + "\"";
                    }
                    ret += ", \"facebookImage\": " + "\"" + kalturaOTTUser.FacebookImage + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"facebook_image\": " + "\"" + kalturaOTTUser.FacebookImage + "\"";
                    }
                    ret += ", \"facebookToken\": " + "\"" + kalturaOTTUser.FacebookToken + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"facebook_token\": " + "\"" + kalturaOTTUser.FacebookToken + "\"";
                    }
                    if(kalturaOTTUser.HouseholdID.HasValue)
                    {
                        ret += ", \"householdId\": " + kalturaOTTUser.HouseholdID;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"household_id\": " + kalturaOTTUser.HouseholdID;
                        }
                    }
                    if(kalturaOTTUser.IsHouseholdMaster.HasValue)
                    {
                        ret += ", \"isHouseholdMaster\": " + kalturaOTTUser.IsHouseholdMaster;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_household_master\": " + kalturaOTTUser.IsHouseholdMaster;
                        }
                    }
                    ret += ", \"phone\": " + "\"" + kalturaOTTUser.Phone + "\"";
                    ret += ", \"suspensionState\": " + kalturaOTTUser.SuspensionState;
                    ret += ", \"suspentionState\": " + kalturaOTTUser.SuspentionState;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"suspention_state\": " + kalturaOTTUser.SuspentionState;
                    }
                    ret += ", \"userState\": " + kalturaOTTUser.UserState;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"user_state\": " + kalturaOTTUser.UserState;
                    }
                    ret += ", \"userType\": " + Serialize(kalturaOTTUser.UserType);
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"user_type\": " + Serialize(kalturaOTTUser.UserType);
                    }
                    ret += ", \"zip\": " + "\"" + kalturaOTTUser.Zip + "\"";
                    break;
                    
                case "KalturaOTTUserDynamicData":
                    KalturaOTTUserDynamicData kalturaOTTUserDynamicData = ottObject as KalturaOTTUserDynamicData;
                    ret += "\"objectType\": " + "\"" + kalturaOTTUserDynamicData.objectType + "\"";
                    if(kalturaOTTUserDynamicData.relatedObjects != null && kalturaOTTUserDynamicData.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaOTTUserDynamicData.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"key\": " + "\"" + kalturaOTTUserDynamicData.Key + "\"";
                    ret += ", \"userId\": " + "\"" + kalturaOTTUserDynamicData.UserId + "\"";
                    ret += ", \"value\": " + Serialize(kalturaOTTUserDynamicData.Value);
                    break;
                    
                case "KalturaOTTUserFilter":
                    KalturaOTTUserFilter kalturaOTTUserFilter = ottObject as KalturaOTTUserFilter;
                    ret += "\"objectType\": " + "\"" + kalturaOTTUserFilter.objectType + "\"";
                    if(kalturaOTTUserFilter.relatedObjects != null && kalturaOTTUserFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaOTTUserFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaOTTUserFilter.OrderBy;
                    ret += ", \"externalIdEqual\": " + "\"" + kalturaOTTUserFilter.ExternalIdEqual + "\"";
                    ret += ", \"idIn\": " + "\"" + kalturaOTTUserFilter.IdIn + "\"";
                    ret += ", \"usernameEqual\": " + "\"" + kalturaOTTUserFilter.UsernameEqual + "\"";
                    break;
                    
                case "KalturaOTTUserListResponse":
                    KalturaOTTUserListResponse kalturaOTTUserListResponse = ottObject as KalturaOTTUserListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaOTTUserListResponse.objectType + "\"";
                    if(kalturaOTTUserListResponse.relatedObjects != null && kalturaOTTUserListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaOTTUserListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaOTTUserListResponse.TotalCount;
                    if(kalturaOTTUserListResponse.Users != null && kalturaOTTUserListResponse.Users.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaOTTUserListResponse.Users.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaOTTUserType":
                    KalturaOTTUserType kalturaOTTUserType = ottObject as KalturaOTTUserType;
                    ret += "\"objectType\": " + "\"" + kalturaOTTUserType.objectType + "\"";
                    if(kalturaOTTUserType.relatedObjects != null && kalturaOTTUserType.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaOTTUserType.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"description\": " + "\"" + kalturaOTTUserType.Description + "\"";
                    if(kalturaOTTUserType.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaOTTUserType.Id;
                    }
                    break;
                    
                case "KalturaParentalRule":
                    KalturaParentalRule kalturaParentalRule = ottObject as KalturaParentalRule;
                    ret += "\"objectType\": " + "\"" + kalturaParentalRule.objectType + "\"";
                    if(kalturaParentalRule.relatedObjects != null && kalturaParentalRule.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaParentalRule.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaParentalRule.blockAnonymousAccess.HasValue)
                    {
                        ret += ", \"blockAnonymousAccess\": " + kalturaParentalRule.blockAnonymousAccess;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"block_anonymous_access\": " + kalturaParentalRule.blockAnonymousAccess;
                        }
                    }
                    ret += ", \"description\": " + "\"" + kalturaParentalRule.description + "\"";
                    if(kalturaParentalRule.epgTagTypeId.HasValue)
                    {
                        ret += ", \"epgTag\": " + kalturaParentalRule.epgTagTypeId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"epg_tag\": " + kalturaParentalRule.epgTagTypeId;
                        }
                    }
                    if(kalturaParentalRule.epgTagValues != null && kalturaParentalRule.epgTagValues.Count > 0)
                    {
                        ret += ", \"epgTagValues\": " + "[" + String.Join(", ", kalturaParentalRule.epgTagValues.Select(item => Serialize(item))) + "]";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"epg_tag_values\": " + "[" + String.Join(", ", kalturaParentalRule.epgTagValues.Select(item => Serialize(item))) + "]";
                        }
                    }
                    if(kalturaParentalRule.id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaParentalRule.id;
                    }
                    if(kalturaParentalRule.isDefault.HasValue)
                    {
                        ret += ", \"isDefault\": " + kalturaParentalRule.isDefault;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_default\": " + kalturaParentalRule.isDefault;
                        }
                    }
                    if(kalturaParentalRule.mediaTagTypeId.HasValue)
                    {
                        ret += ", \"mediaTag\": " + kalturaParentalRule.mediaTagTypeId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"media_tag\": " + kalturaParentalRule.mediaTagTypeId;
                        }
                    }
                    if(kalturaParentalRule.mediaTagValues != null && kalturaParentalRule.mediaTagValues.Count > 0)
                    {
                        ret += ", \"mediaTagValues\": " + "[" + String.Join(", ", kalturaParentalRule.mediaTagValues.Select(item => Serialize(item))) + "]";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"media_tag_values\": " + "[" + String.Join(", ", kalturaParentalRule.mediaTagValues.Select(item => Serialize(item))) + "]";
                        }
                    }
                    ret += ", \"name\": " + "\"" + kalturaParentalRule.name + "\"";
                    if(kalturaParentalRule.order.HasValue)
                    {
                        ret += ", \"order\": " + kalturaParentalRule.order;
                    }
                    ret += ", \"origin\": " + kalturaParentalRule.Origin;
                    ret += ", \"ruleType\": " + kalturaParentalRule.ruleType;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"rule_type\": " + kalturaParentalRule.ruleType;
                    }
                    break;
                    
                case "KalturaParentalRuleFilter":
                    KalturaParentalRuleFilter kalturaParentalRuleFilter = ottObject as KalturaParentalRuleFilter;
                    ret += "\"objectType\": " + "\"" + kalturaParentalRuleFilter.objectType + "\"";
                    if(kalturaParentalRuleFilter.relatedObjects != null && kalturaParentalRuleFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaParentalRuleFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaParentalRuleFilter.OrderBy;
                    if(kalturaParentalRuleFilter.EntityReferenceEqual.HasValue)
                    {
                        ret += ", \"entityReferenceEqual\": " + kalturaParentalRuleFilter.EntityReferenceEqual;
                    }
                    break;
                    
                case "KalturaParentalRuleListResponse":
                    KalturaParentalRuleListResponse kalturaParentalRuleListResponse = ottObject as KalturaParentalRuleListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaParentalRuleListResponse.objectType + "\"";
                    if(kalturaParentalRuleListResponse.relatedObjects != null && kalturaParentalRuleListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaParentalRuleListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaParentalRuleListResponse.TotalCount;
                    if(kalturaParentalRuleListResponse.ParentalRule != null && kalturaParentalRuleListResponse.ParentalRule.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaParentalRuleListResponse.ParentalRule.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaPartnerConfiguration":
                    KalturaPartnerConfiguration kalturaPartnerConfiguration = ottObject as KalturaPartnerConfiguration;
                    ret += "\"objectType\": " + "\"" + kalturaPartnerConfiguration.objectType + "\"";
                    if(kalturaPartnerConfiguration.relatedObjects != null && kalturaPartnerConfiguration.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPartnerConfiguration.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    break;
                    
                case "KalturaPartnerConfigurationHolder":
                    KalturaPartnerConfigurationHolder kalturaPartnerConfigurationHolder = ottObject as KalturaPartnerConfigurationHolder;
                    ret += "\"objectType\": " + "\"" + kalturaPartnerConfigurationHolder.objectType + "\"";
                    if(kalturaPartnerConfigurationHolder.relatedObjects != null && kalturaPartnerConfigurationHolder.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPartnerConfigurationHolder.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"type\": " + kalturaPartnerConfigurationHolder.type;
                    break;
                    
                case "KalturaPartnerNotificationSettings":
                    KalturaPartnerNotificationSettings kalturaPartnerNotificationSettings = ottObject as KalturaPartnerNotificationSettings;
                    ret += "\"objectType\": " + "\"" + kalturaPartnerNotificationSettings.objectType + "\"";
                    if(kalturaPartnerNotificationSettings.relatedObjects != null && kalturaPartnerNotificationSettings.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPartnerNotificationSettings.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaPartnerNotificationSettings.AutomaticIssueFollowNotification.HasValue)
                    {
                        ret += ", \"automaticIssueFollowNotification\": " + kalturaPartnerNotificationSettings.AutomaticIssueFollowNotification;
                    }
                    ret += ", \"churnMailSubject\": " + "\"" + kalturaPartnerNotificationSettings.ChurnMailSubject + "\"";
                    ret += ", \"churnMailTemplateName\": " + "\"" + kalturaPartnerNotificationSettings.ChurnMailTemplateName + "\"";
                    if(kalturaPartnerNotificationSettings.InboxEnabled.HasValue)
                    {
                        ret += ", \"inboxEnabled\": " + kalturaPartnerNotificationSettings.InboxEnabled;
                    }
                    ret += ", \"mailSenderName\": " + "\"" + kalturaPartnerNotificationSettings.MailSenderName + "\"";
                    if(kalturaPartnerNotificationSettings.MessageTTLDays.HasValue)
                    {
                        ret += ", \"messageTTLDays\": " + kalturaPartnerNotificationSettings.MessageTTLDays;
                    }
                    ret += ", \"pushAdapterUrl\": " + "\"" + kalturaPartnerNotificationSettings.PushAdapterUrl + "\"";
                    if(kalturaPartnerNotificationSettings.PushEndHour.HasValue)
                    {
                        ret += ", \"pushEndHour\": " + kalturaPartnerNotificationSettings.PushEndHour;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"push_end_hour\": " + kalturaPartnerNotificationSettings.PushEndHour;
                        }
                    }
                    if(kalturaPartnerNotificationSettings.PushNotificationEnabled.HasValue)
                    {
                        ret += ", \"pushNotificationEnabled\": " + kalturaPartnerNotificationSettings.PushNotificationEnabled;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"push_notification_enabled\": " + kalturaPartnerNotificationSettings.PushNotificationEnabled;
                        }
                    }
                    if(kalturaPartnerNotificationSettings.PushStartHour.HasValue)
                    {
                        ret += ", \"pushStartHour\": " + kalturaPartnerNotificationSettings.PushStartHour;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"push_start_hour\": " + kalturaPartnerNotificationSettings.PushStartHour;
                        }
                    }
                    if(kalturaPartnerNotificationSettings.PushSystemAnnouncementsEnabled.HasValue)
                    {
                        ret += ", \"pushSystemAnnouncementsEnabled\": " + kalturaPartnerNotificationSettings.PushSystemAnnouncementsEnabled;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"push_system_announcements_enabled\": " + kalturaPartnerNotificationSettings.PushSystemAnnouncementsEnabled;
                        }
                    }
                    if(kalturaPartnerNotificationSettings.ReminderEnabled.HasValue)
                    {
                        ret += ", \"reminderEnabled\": " + kalturaPartnerNotificationSettings.ReminderEnabled;
                    }
                    if(kalturaPartnerNotificationSettings.ReminderOffset.HasValue)
                    {
                        ret += ", \"reminderOffsetSec\": " + kalturaPartnerNotificationSettings.ReminderOffset;
                    }
                    ret += ", \"senderEmail\": " + "\"" + kalturaPartnerNotificationSettings.SenderEmail + "\"";
                    if(kalturaPartnerNotificationSettings.TopicExpirationDurationDays.HasValue)
                    {
                        ret += ", \"topicExpirationDurationDays\": " + kalturaPartnerNotificationSettings.TopicExpirationDurationDays;
                    }
                    break;
                    
                case "KalturaPaymentGateway":
                    KalturaPaymentGateway kalturaPaymentGateway = ottObject as KalturaPaymentGateway;
                    ret += "\"objectType\": " + "\"" + kalturaPaymentGateway.objectType + "\"";
                    if(kalturaPaymentGateway.relatedObjects != null && kalturaPaymentGateway.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPaymentGateway.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"payment_gateway\": " + Serialize(kalturaPaymentGateway.paymentGateway);
                    ret += ", \"selected_by\": " + kalturaPaymentGateway.selectedBy;
                    break;
                    
                case "KalturaPaymentGatewayBaseProfile":
                    KalturaPaymentGatewayBaseProfile kalturaPaymentGatewayBaseProfile = ottObject as KalturaPaymentGatewayBaseProfile;
                    ret += "\"objectType\": " + "\"" + kalturaPaymentGatewayBaseProfile.objectType + "\"";
                    if(kalturaPaymentGatewayBaseProfile.relatedObjects != null && kalturaPaymentGatewayBaseProfile.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPaymentGatewayBaseProfile.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaPaymentGatewayBaseProfile.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaPaymentGatewayBaseProfile.Id;
                    }
                    if(kalturaPaymentGatewayBaseProfile.IsDefault.HasValue)
                    {
                        ret += ", \"isDefault\": " + kalturaPaymentGatewayBaseProfile.IsDefault;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_default\": " + kalturaPaymentGatewayBaseProfile.IsDefault;
                        }
                    }
                    ret += ", \"name\": " + "\"" + kalturaPaymentGatewayBaseProfile.Name + "\"";
                    if(kalturaPaymentGatewayBaseProfile.PaymentMethods != null && kalturaPaymentGatewayBaseProfile.PaymentMethods.Count > 0)
                    {
                        ret += ", \"paymentMethods\": " + "[" + String.Join(", ", kalturaPaymentGatewayBaseProfile.PaymentMethods.Select(item => Serialize(item))) + "]";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"payment_methods\": " + "[" + String.Join(", ", kalturaPaymentGatewayBaseProfile.PaymentMethods.Select(item => Serialize(item))) + "]";
                        }
                    }
                    if(kalturaPaymentGatewayBaseProfile.selectedBy.HasValue)
                    {
                        ret += ", \"selectedBy\": " + kalturaPaymentGatewayBaseProfile.selectedBy;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"selected_by\": " + kalturaPaymentGatewayBaseProfile.selectedBy;
                        }
                    }
                    break;
                    
                case "KalturaPaymentGatewayConfiguration":
                    KalturaPaymentGatewayConfiguration kalturaPaymentGatewayConfiguration = ottObject as KalturaPaymentGatewayConfiguration;
                    ret += "\"objectType\": " + "\"" + kalturaPaymentGatewayConfiguration.objectType + "\"";
                    if(kalturaPaymentGatewayConfiguration.relatedObjects != null && kalturaPaymentGatewayConfiguration.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPaymentGatewayConfiguration.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaPaymentGatewayConfiguration.Configuration != null && kalturaPaymentGatewayConfiguration.Configuration.Count > 0)
                    {
                        ret += ", \"paymentGatewayConfiguration\": " + "[" + String.Join(", ", kalturaPaymentGatewayConfiguration.Configuration.Select(item => Serialize(item))) + "]";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"payment_gatewaye_configuration\": " + "[" + String.Join(", ", kalturaPaymentGatewayConfiguration.Configuration.Select(item => Serialize(item))) + "]";
                        }
                    }
                    break;
                    
                case "KalturaPaymentGatewayProfile":
                    KalturaPaymentGatewayProfile kalturaPaymentGatewayProfile = ottObject as KalturaPaymentGatewayProfile;
                    ret += "\"objectType\": " + "\"" + kalturaPaymentGatewayProfile.objectType + "\"";
                    if(kalturaPaymentGatewayProfile.relatedObjects != null && kalturaPaymentGatewayProfile.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPaymentGatewayProfile.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaPaymentGatewayProfile.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaPaymentGatewayProfile.Id;
                    }
                    if(kalturaPaymentGatewayProfile.IsDefault.HasValue)
                    {
                        ret += ", \"isDefault\": " + kalturaPaymentGatewayProfile.IsDefault;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_default\": " + kalturaPaymentGatewayProfile.IsDefault;
                        }
                    }
                    ret += ", \"name\": " + "\"" + kalturaPaymentGatewayProfile.Name + "\"";
                    if(kalturaPaymentGatewayProfile.PaymentMethods != null && kalturaPaymentGatewayProfile.PaymentMethods.Count > 0)
                    {
                        ret += ", \"paymentMethods\": " + "[" + String.Join(", ", kalturaPaymentGatewayProfile.PaymentMethods.Select(item => Serialize(item))) + "]";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"payment_methods\": " + "[" + String.Join(", ", kalturaPaymentGatewayProfile.PaymentMethods.Select(item => Serialize(item))) + "]";
                        }
                    }
                    if(kalturaPaymentGatewayProfile.selectedBy.HasValue)
                    {
                        ret += ", \"selectedBy\": " + kalturaPaymentGatewayProfile.selectedBy;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"selected_by\": " + kalturaPaymentGatewayProfile.selectedBy;
                        }
                    }
                    ret += ", \"adapterUrl\": " + "\"" + kalturaPaymentGatewayProfile.AdapterUrl + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"adapter_url\": " + "\"" + kalturaPaymentGatewayProfile.AdapterUrl + "\"";
                    }
                    ret += ", \"externalIdentifier\": " + "\"" + kalturaPaymentGatewayProfile.ExternalIdentifier + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"external_identifier\": " + "\"" + kalturaPaymentGatewayProfile.ExternalIdentifier + "\"";
                    }
                    ret += ", \"externalVerification\": " + kalturaPaymentGatewayProfile.ExternalVerification;
                    if(kalturaPaymentGatewayProfile.IsActive.HasValue)
                    {
                        ret += ", \"isActive\": " + kalturaPaymentGatewayProfile.IsActive;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_active\": " + kalturaPaymentGatewayProfile.IsActive;
                        }
                    }
                    if(kalturaPaymentGatewayProfile.PendingInterval.HasValue)
                    {
                        ret += ", \"pendingInterval\": " + kalturaPaymentGatewayProfile.PendingInterval;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"pending_interval\": " + kalturaPaymentGatewayProfile.PendingInterval;
                        }
                    }
                    if(kalturaPaymentGatewayProfile.PendingRetries.HasValue)
                    {
                        ret += ", \"pendingRetries\": " + kalturaPaymentGatewayProfile.PendingRetries;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"pending_retries\": " + kalturaPaymentGatewayProfile.PendingRetries;
                        }
                    }
                    if(kalturaPaymentGatewayProfile.RenewIntervalMinutes.HasValue)
                    {
                        ret += ", \"renewIntervalMinutes\": " + kalturaPaymentGatewayProfile.RenewIntervalMinutes;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"renew_interval_minutes\": " + kalturaPaymentGatewayProfile.RenewIntervalMinutes;
                        }
                    }
                    if(kalturaPaymentGatewayProfile.RenewStartMinutes.HasValue)
                    {
                        ret += ", \"renewStartMinutes\": " + kalturaPaymentGatewayProfile.RenewStartMinutes;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"renew_start_minutes\": " + kalturaPaymentGatewayProfile.RenewStartMinutes;
                        }
                    }
                    ret += ", \"renewUrl\": " + "\"" + kalturaPaymentGatewayProfile.RenewUrl + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"renew_url\": " + "\"" + kalturaPaymentGatewayProfile.RenewUrl + "\"";
                    }
                    if(kalturaPaymentGatewayProfile.Settings != null && kalturaPaymentGatewayProfile.Settings.Count > 0)
                    {
                        ret += ", \"paymentGatewaySettings\": " + "{" + String.Join(", ", kalturaPaymentGatewayProfile.Settings.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"payment_gateway_settings\": " + "{" + String.Join(", ", kalturaPaymentGatewayProfile.Settings.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                        }
                    }
                    ret += ", \"sharedSecret\": " + "\"" + kalturaPaymentGatewayProfile.SharedSecret + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"shared_secret\": " + "\"" + kalturaPaymentGatewayProfile.SharedSecret + "\"";
                    }
                    ret += ", \"statusUrl\": " + "\"" + kalturaPaymentGatewayProfile.StatusUrl + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"status_url\": " + "\"" + kalturaPaymentGatewayProfile.StatusUrl + "\"";
                    }
                    ret += ", \"transactUrl\": " + "\"" + kalturaPaymentGatewayProfile.TransactUrl + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"transact_url\": " + "\"" + kalturaPaymentGatewayProfile.TransactUrl + "\"";
                    }
                    break;
                    
                case "KalturaPaymentGatewayProfileListResponse":
                    KalturaPaymentGatewayProfileListResponse kalturaPaymentGatewayProfileListResponse = ottObject as KalturaPaymentGatewayProfileListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaPaymentGatewayProfileListResponse.objectType + "\"";
                    if(kalturaPaymentGatewayProfileListResponse.relatedObjects != null && kalturaPaymentGatewayProfileListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPaymentGatewayProfileListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaPaymentGatewayProfileListResponse.TotalCount;
                    if(kalturaPaymentGatewayProfileListResponse.PaymentGatewayProfiles != null && kalturaPaymentGatewayProfileListResponse.PaymentGatewayProfiles.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaPaymentGatewayProfileListResponse.PaymentGatewayProfiles.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaPaymentMethod":
                    KalturaPaymentMethod kalturaPaymentMethod = ottObject as KalturaPaymentMethod;
                    ret += "\"objectType\": " + "\"" + kalturaPaymentMethod.objectType + "\"";
                    if(kalturaPaymentMethod.relatedObjects != null && kalturaPaymentMethod.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPaymentMethod.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaPaymentMethod.AllowMultiInstance.HasValue)
                    {
                        ret += ", \"allowMultiInstance\": " + kalturaPaymentMethod.AllowMultiInstance;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"allow_multi_instance\": " + kalturaPaymentMethod.AllowMultiInstance;
                        }
                    }
                    if(kalturaPaymentMethod.HouseholdPaymentMethods != null && kalturaPaymentMethod.HouseholdPaymentMethods.Count > 0)
                    {
                        ret += ", \"householdPaymentMethods\": " + "[" + String.Join(", ", kalturaPaymentMethod.HouseholdPaymentMethods.Select(item => Serialize(item))) + "]";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"household_payment_methods\": " + "[" + String.Join(", ", kalturaPaymentMethod.HouseholdPaymentMethods.Select(item => Serialize(item))) + "]";
                        }
                    }
                    if(kalturaPaymentMethod.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaPaymentMethod.Id;
                    }
                    ret += ", \"name\": " + "\"" + kalturaPaymentMethod.Name + "\"";
                    break;
                    
                case "KalturaPaymentMethodProfile":
                    KalturaPaymentMethodProfile kalturaPaymentMethodProfile = ottObject as KalturaPaymentMethodProfile;
                    ret += "\"objectType\": " + "\"" + kalturaPaymentMethodProfile.objectType + "\"";
                    if(kalturaPaymentMethodProfile.relatedObjects != null && kalturaPaymentMethodProfile.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPaymentMethodProfile.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaPaymentMethodProfile.AllowMultiInstance.HasValue)
                    {
                        ret += ", \"allowMultiInstance\": " + kalturaPaymentMethodProfile.AllowMultiInstance;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"allow_multi_instance\": " + kalturaPaymentMethodProfile.AllowMultiInstance;
                        }
                    }
                    if(kalturaPaymentMethodProfile.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaPaymentMethodProfile.Id;
                    }
                    ret += ", \"name\": " + "\"" + kalturaPaymentMethodProfile.Name + "\"";
                    if(kalturaPaymentMethodProfile.PaymentGatewayId.HasValue)
                    {
                        ret += ", \"paymentGatewayId\": " + kalturaPaymentMethodProfile.PaymentGatewayId;
                    }
                    break;
                    
                case "KalturaPaymentMethodProfileFilter":
                    KalturaPaymentMethodProfileFilter kalturaPaymentMethodProfileFilter = ottObject as KalturaPaymentMethodProfileFilter;
                    ret += "\"objectType\": " + "\"" + kalturaPaymentMethodProfileFilter.objectType + "\"";
                    if(kalturaPaymentMethodProfileFilter.relatedObjects != null && kalturaPaymentMethodProfileFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPaymentMethodProfileFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaPaymentMethodProfileFilter.OrderBy;
                    if(kalturaPaymentMethodProfileFilter.PaymentGatewayIdEqual.HasValue)
                    {
                        ret += ", \"paymentGatewayIdEqual\": " + kalturaPaymentMethodProfileFilter.PaymentGatewayIdEqual;
                    }
                    break;
                    
                case "KalturaPaymentMethodProfileListResponse":
                    KalturaPaymentMethodProfileListResponse kalturaPaymentMethodProfileListResponse = ottObject as KalturaPaymentMethodProfileListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaPaymentMethodProfileListResponse.objectType + "\"";
                    if(kalturaPaymentMethodProfileListResponse.relatedObjects != null && kalturaPaymentMethodProfileListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPaymentMethodProfileListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaPaymentMethodProfileListResponse.TotalCount;
                    if(kalturaPaymentMethodProfileListResponse.PaymentMethodProfiles != null && kalturaPaymentMethodProfileListResponse.PaymentMethodProfiles.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaPaymentMethodProfileListResponse.PaymentMethodProfiles.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaPermission":
                    KalturaPermission kalturaPermission = ottObject as KalturaPermission;
                    ret += "\"objectType\": " + "\"" + kalturaPermission.objectType + "\"";
                    if(kalturaPermission.relatedObjects != null && kalturaPermission.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPermission.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaPermission.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaPermission.Id;
                    }
                    ret += ", \"name\": " + "\"" + kalturaPermission.Name + "\"";
                    if(kalturaPermission.PermissionItems != null && kalturaPermission.PermissionItems.Count > 0)
                    {
                        ret += ", \"permissionItems\": " + "[" + String.Join(", ", kalturaPermission.PermissionItems.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaPermissionItem":
                    KalturaPermissionItem kalturaPermissionItem = ottObject as KalturaPermissionItem;
                    ret += "\"objectType\": " + "\"" + kalturaPermissionItem.objectType + "\"";
                    if(kalturaPermissionItem.relatedObjects != null && kalturaPermissionItem.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPermissionItem.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaPermissionItem.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaPermissionItem.Id;
                    }
                    ret += ", \"isExcluded\": " + kalturaPermissionItem.IsExcluded;
                    ret += ", \"name\": " + "\"" + kalturaPermissionItem.Name + "\"";
                    break;
                    
                case "KalturaPermissionsFilter":
                    KalturaPermissionsFilter kalturaPermissionsFilter = ottObject as KalturaPermissionsFilter;
                    ret += "\"objectType\": " + "\"" + kalturaPermissionsFilter.objectType + "\"";
                    if(kalturaPermissionsFilter.relatedObjects != null && kalturaPermissionsFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPermissionsFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaPermissionsFilter.Ids != null && kalturaPermissionsFilter.Ids.Count > 0)
                    {
                        ret += ", \"ids\": " + "[" + String.Join(", ", kalturaPermissionsFilter.Ids.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaPersistedFilter":
                    KalturaPersistedFilter<int> kalturaPersistedFilter = ottObject as KalturaPersistedFilter<int>;
                    ret += "\"objectType\": " + "\"" + kalturaPersistedFilter.objectType + "\"";
                    if(kalturaPersistedFilter.relatedObjects != null && kalturaPersistedFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPersistedFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaPersistedFilter.OrderBy;
                    ret += ", \"name\": " + "\"" + kalturaPersistedFilter.Name + "\"";
                    break;
                    
                case "KalturaPersonalAsset":
                    KalturaPersonalAsset kalturaPersonalAsset = ottObject as KalturaPersonalAsset;
                    ret += "\"objectType\": " + "\"" + kalturaPersonalAsset.objectType + "\"";
                    if(kalturaPersonalAsset.relatedObjects != null && kalturaPersonalAsset.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPersonalAsset.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaPersonalAsset.Bookmarks != null && kalturaPersonalAsset.Bookmarks.Count > 0)
                    {
                        ret += ", \"bookmarks\": " + "[" + String.Join(", ", kalturaPersonalAsset.Bookmarks.Select(item => Serialize(item))) + "]";
                    }
                    if(kalturaPersonalAsset.Files != null && kalturaPersonalAsset.Files.Count > 0)
                    {
                        ret += ", \"files\": " + "[" + String.Join(", ", kalturaPersonalAsset.Files.Select(item => Serialize(item))) + "]";
                    }
                    ret += ", \"following\": " + kalturaPersonalAsset.Following;
                    if(kalturaPersonalAsset.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaPersonalAsset.Id;
                    }
                    ret += ", \"type\": " + kalturaPersonalAsset.Type;
                    break;
                    
                case "KalturaPersonalAssetListResponse":
                    KalturaPersonalAssetListResponse kalturaPersonalAssetListResponse = ottObject as KalturaPersonalAssetListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaPersonalAssetListResponse.objectType + "\"";
                    if(kalturaPersonalAssetListResponse.relatedObjects != null && kalturaPersonalAssetListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPersonalAssetListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaPersonalAssetListResponse.TotalCount;
                    if(kalturaPersonalAssetListResponse.Objects != null && kalturaPersonalAssetListResponse.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaPersonalAssetListResponse.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaPersonalAssetRequest":
                    KalturaPersonalAssetRequest kalturaPersonalAssetRequest = ottObject as KalturaPersonalAssetRequest;
                    ret += "\"objectType\": " + "\"" + kalturaPersonalAssetRequest.objectType + "\"";
                    if(kalturaPersonalAssetRequest.relatedObjects != null && kalturaPersonalAssetRequest.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPersonalAssetRequest.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaPersonalAssetRequest.FileIds != null && kalturaPersonalAssetRequest.FileIds.Count > 0)
                    {
                        ret += ", \"fileIds\": " + "[" + String.Join(", ", kalturaPersonalAssetRequest.FileIds.Select(item => item.ToString())) + "]";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"file_ids\": " + "[" + String.Join(", ", kalturaPersonalAssetRequest.FileIds.Select(item => item.ToString())) + "]";
                        }
                    }
                    if(kalturaPersonalAssetRequest.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaPersonalAssetRequest.Id;
                    }
                    ret += ", \"type\": " + kalturaPersonalAssetRequest.Type;
                    break;
                    
                case "KalturaPersonalAssetWithHolder":
                    KalturaPersonalAssetWithHolder kalturaPersonalAssetWithHolder = ottObject as KalturaPersonalAssetWithHolder;
                    ret += "\"objectType\": " + "\"" + kalturaPersonalAssetWithHolder.objectType + "\"";
                    if(kalturaPersonalAssetWithHolder.relatedObjects != null && kalturaPersonalAssetWithHolder.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPersonalAssetWithHolder.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"type\": " + kalturaPersonalAssetWithHolder.type;
                    break;
                    
                case "KalturaPersonalFeed":
                    KalturaPersonalFeed kalturaPersonalFeed = ottObject as KalturaPersonalFeed;
                    ret += "\"objectType\": " + "\"" + kalturaPersonalFeed.objectType + "\"";
                    if(kalturaPersonalFeed.relatedObjects != null && kalturaPersonalFeed.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPersonalFeed.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"assetId\": " + kalturaPersonalFeed.AssetId;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"asset_id\": " + kalturaPersonalFeed.AssetId;
                    }
                    break;
                    
                case "KalturaPersonalFeedFilter":
                    KalturaPersonalFeedFilter kalturaPersonalFeedFilter = ottObject as KalturaPersonalFeedFilter;
                    ret += "\"objectType\": " + "\"" + kalturaPersonalFeedFilter.objectType + "\"";
                    if(kalturaPersonalFeedFilter.relatedObjects != null && kalturaPersonalFeedFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPersonalFeedFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaPersonalFeedFilter.OrderBy;
                    break;
                    
                case "KalturaPersonalFeedListResponse":
                    KalturaPersonalFeedListResponse kalturaPersonalFeedListResponse = ottObject as KalturaPersonalFeedListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaPersonalFeedListResponse.objectType + "\"";
                    if(kalturaPersonalFeedListResponse.relatedObjects != null && kalturaPersonalFeedListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPersonalFeedListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaPersonalFeedListResponse.TotalCount;
                    if(kalturaPersonalFeedListResponse.PersonalFollowFeed != null && kalturaPersonalFeedListResponse.PersonalFollowFeed.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaPersonalFeedListResponse.PersonalFollowFeed.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaPersonalFile":
                    KalturaPersonalFile kalturaPersonalFile = ottObject as KalturaPersonalFile;
                    ret += "\"objectType\": " + "\"" + kalturaPersonalFile.objectType + "\"";
                    if(kalturaPersonalFile.relatedObjects != null && kalturaPersonalFile.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPersonalFile.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaPersonalFile.Discounted.HasValue)
                    {
                        ret += ", \"discounted\": " + kalturaPersonalFile.Discounted;
                    }
                    if(kalturaPersonalFile.Entitled.HasValue)
                    {
                        ret += ", \"entitled\": " + kalturaPersonalFile.Entitled;
                    }
                    if(kalturaPersonalFile.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaPersonalFile.Id;
                    }
                    ret += ", \"offer\": " + "\"" + kalturaPersonalFile.Offer + "\"";
                    break;
                    
                case "KalturaPersonalFollowFeed":
                    KalturaPersonalFollowFeed kalturaPersonalFollowFeed = ottObject as KalturaPersonalFollowFeed;
                    ret += "\"objectType\": " + "\"" + kalturaPersonalFollowFeed.objectType + "\"";
                    if(kalturaPersonalFollowFeed.relatedObjects != null && kalturaPersonalFollowFeed.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPersonalFollowFeed.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"assetId\": " + kalturaPersonalFollowFeed.AssetId;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"asset_id\": " + kalturaPersonalFollowFeed.AssetId;
                    }
                    break;
                    
                case "KalturaPersonalFollowFeedResponse":
                    KalturaPersonalFollowFeedResponse kalturaPersonalFollowFeedResponse = ottObject as KalturaPersonalFollowFeedResponse;
                    ret += "\"objectType\": " + "\"" + kalturaPersonalFollowFeedResponse.objectType + "\"";
                    if(kalturaPersonalFollowFeedResponse.relatedObjects != null && kalturaPersonalFollowFeedResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPersonalFollowFeedResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaPersonalFollowFeedResponse.TotalCount;
                    if(kalturaPersonalFollowFeedResponse.PersonalFollowFeed != null && kalturaPersonalFollowFeedResponse.PersonalFollowFeed.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaPersonalFollowFeedResponse.PersonalFollowFeed.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaPin":
                    KalturaPin kalturaPin = ottObject as KalturaPin;
                    ret += "\"objectType\": " + "\"" + kalturaPin.objectType + "\"";
                    if(kalturaPin.relatedObjects != null && kalturaPin.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPin.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"origin\": " + kalturaPin.Origin;
                    ret += ", \"pin\": " + "\"" + kalturaPin.PIN + "\"";
                    ret += ", \"type\": " + kalturaPin.Type;
                    break;
                    
                case "KalturaPinResponse":
                    KalturaPinResponse kalturaPinResponse = ottObject as KalturaPinResponse;
                    ret += "\"objectType\": " + "\"" + kalturaPinResponse.objectType + "\"";
                    if(kalturaPinResponse.relatedObjects != null && kalturaPinResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPinResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"origin\": " + kalturaPinResponse.Origin;
                    ret += ", \"pin\": " + "\"" + kalturaPinResponse.PIN + "\"";
                    ret += ", \"type\": " + kalturaPinResponse.Type;
                    break;
                    
                case "KalturaPlaybackContext":
                    KalturaPlaybackContext kalturaPlaybackContext = ottObject as KalturaPlaybackContext;
                    ret += "\"objectType\": " + "\"" + kalturaPlaybackContext.objectType + "\"";
                    if(kalturaPlaybackContext.relatedObjects != null && kalturaPlaybackContext.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPlaybackContext.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaPlaybackContext.Actions != null && kalturaPlaybackContext.Actions.Count > 0)
                    {
                        ret += ", \"actions\": " + "[" + String.Join(", ", kalturaPlaybackContext.Actions.Select(item => Serialize(item))) + "]";
                    }
                    if(kalturaPlaybackContext.Messages != null && kalturaPlaybackContext.Messages.Count > 0)
                    {
                        ret += ", \"messages\": " + "[" + String.Join(", ", kalturaPlaybackContext.Messages.Select(item => Serialize(item))) + "]";
                    }
                    if(kalturaPlaybackContext.Sources != null && kalturaPlaybackContext.Sources.Count > 0)
                    {
                        ret += ", \"sources\": " + "[" + String.Join(", ", kalturaPlaybackContext.Sources.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaPlaybackContextOptions":
                    KalturaPlaybackContextOptions kalturaPlaybackContextOptions = ottObject as KalturaPlaybackContextOptions;
                    ret += "\"objectType\": " + "\"" + kalturaPlaybackContextOptions.objectType + "\"";
                    if(kalturaPlaybackContextOptions.relatedObjects != null && kalturaPlaybackContextOptions.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPlaybackContextOptions.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"assetFileIds\": " + "\"" + kalturaPlaybackContextOptions.AssetFileIds + "\"";
                    if(kalturaPlaybackContextOptions.Context.HasValue)
                    {
                        ret += ", \"context\": " + kalturaPlaybackContextOptions.Context;
                    }
                    ret += ", \"mediaProtocol\": " + "\"" + kalturaPlaybackContextOptions.MediaProtocol + "\"";
                    ret += ", \"streamerType\": " + "\"" + kalturaPlaybackContextOptions.StreamerType + "\"";
                    break;
                    
                case "KalturaPlaybackSource":
                    KalturaPlaybackSource kalturaPlaybackSource = ottObject as KalturaPlaybackSource;
                    ret += "\"objectType\": " + "\"" + kalturaPlaybackSource.objectType + "\"";
                    if(kalturaPlaybackSource.relatedObjects != null && kalturaPlaybackSource.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPlaybackSource.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"altCdnCode\": " + "\"" + kalturaPlaybackSource.AltCdnCode + "\"";
                    if(kalturaPlaybackSource.AssetId.HasValue)
                    {
                        ret += ", \"assetId\": " + kalturaPlaybackSource.AssetId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"asset_id\": " + kalturaPlaybackSource.AssetId;
                        }
                    }
                    ret += ", \"billingType\": " + "\"" + kalturaPlaybackSource.BillingType + "\"";
                    ret += ", \"cdnCode\": " + "\"" + kalturaPlaybackSource.CdnCode + "\"";
                    ret += ", \"cdnName\": " + "\"" + kalturaPlaybackSource.CdnName + "\"";
                    if(kalturaPlaybackSource.Duration.HasValue)
                    {
                        ret += ", \"duration\": " + kalturaPlaybackSource.Duration;
                    }
                    ret += ", \"externalId\": " + "\"" + kalturaPlaybackSource.ExternalId + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"external_id\": " + "\"" + kalturaPlaybackSource.ExternalId + "\"";
                    }
                    ret += ", \"fileSize\": " + kalturaPlaybackSource.FileSize;
                    ret += ", \"handlingType\": " + "\"" + kalturaPlaybackSource.HandlingType + "\"";
                    if(kalturaPlaybackSource.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaPlaybackSource.Id;
                    }
                    ret += ", \"ppvModules\": " + Serialize(kalturaPlaybackSource.PPVModules);
                    ret += ", \"productCode\": " + "\"" + kalturaPlaybackSource.ProductCode + "\"";
                    ret += ", \"quality\": " + "\"" + kalturaPlaybackSource.Quality + "\"";
                    ret += ", \"type\": " + "\"" + kalturaPlaybackSource.Type + "\"";
                    ret += ", \"url\": " + "\"" + kalturaPlaybackSource.Url + "\"";
                    ret += ", \"adsParam\": " + "\"" + kalturaPlaybackSource.AdsParams + "\"";
                    if(kalturaPlaybackSource.AdsPolicy.HasValue)
                    {
                        ret += ", \"adsPolicy\": " + kalturaPlaybackSource.AdsPolicy;
                    }
                    if(kalturaPlaybackSource.Drm != null && kalturaPlaybackSource.Drm.Count > 0)
                    {
                        ret += ", \"drm\": " + "[" + String.Join(", ", kalturaPlaybackSource.Drm.Select(item => Serialize(item))) + "]";
                    }
                    ret += ", \"format\": " + "\"" + kalturaPlaybackSource.Format + "\"";
                    ret += ", \"protocols\": " + "\"" + kalturaPlaybackSource.Protocols + "\"";
                    break;
                    
                case "KalturaPlayerAssetData":
                    KalturaPlayerAssetData kalturaPlayerAssetData = ottObject as KalturaPlayerAssetData;
                    ret += "\"objectType\": " + "\"" + kalturaPlayerAssetData.objectType + "\"";
                    if(kalturaPlayerAssetData.relatedObjects != null && kalturaPlayerAssetData.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPlayerAssetData.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"action\": " + "\"" + kalturaPlayerAssetData.action + "\"";
                    if(kalturaPlayerAssetData.averageBitRate.HasValue)
                    {
                        ret += ", \"averageBitrate\": " + kalturaPlayerAssetData.averageBitRate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"average_bitrate\": " + kalturaPlayerAssetData.averageBitRate;
                        }
                    }
                    if(kalturaPlayerAssetData.currentBitRate.HasValue)
                    {
                        ret += ", \"currentBitrate\": " + kalturaPlayerAssetData.currentBitRate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"current_bitrate\": " + kalturaPlayerAssetData.currentBitRate;
                        }
                    }
                    if(kalturaPlayerAssetData.location.HasValue)
                    {
                        ret += ", \"location\": " + kalturaPlayerAssetData.location;
                    }
                    if(kalturaPlayerAssetData.totalBitRate.HasValue)
                    {
                        ret += ", \"totalBitrate\": " + kalturaPlayerAssetData.totalBitRate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"total_bitrate\": " + kalturaPlayerAssetData.totalBitRate;
                        }
                    }
                    break;
                    
                case "KalturaPluginData":
                    KalturaPluginData kalturaPluginData = ottObject as KalturaPluginData;
                    ret += "\"objectType\": " + "\"" + kalturaPluginData.objectType + "\"";
                    if(kalturaPluginData.relatedObjects != null && kalturaPluginData.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPluginData.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    break;
                    
                case "KalturaPpv":
                    KalturaPpv kalturaPpv = ottObject as KalturaPpv;
                    ret += "\"objectType\": " + "\"" + kalturaPpv.objectType + "\"";
                    if(kalturaPpv.relatedObjects != null && kalturaPpv.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPpv.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"couponsGroup\": " + Serialize(kalturaPpv.CouponsGroup);
                    if(kalturaPpv.Descriptions != null && kalturaPpv.Descriptions.Count > 0)
                    {
                        ret += ", \"descriptions\": " + "[" + String.Join(", ", kalturaPpv.Descriptions.Select(item => Serialize(item))) + "]";
                    }
                    ret += ", \"discountModule\": " + Serialize(kalturaPpv.DiscountModule);
                    if(kalturaPpv.FileTypes != null && kalturaPpv.FileTypes.Count > 0)
                    {
                        ret += ", \"fileTypes\": " + "[" + String.Join(", ", kalturaPpv.FileTypes.Select(item => Serialize(item))) + "]";
                    }
                    if(kalturaPpv.FirstDeviceLimitation.HasValue)
                    {
                        ret += ", \"firstDeviceLimitation\": " + kalturaPpv.FirstDeviceLimitation;
                    }
                    ret += ", \"id\": " + "\"" + kalturaPpv.Id + "\"";
                    if(kalturaPpv.IsSubscriptionOnly.HasValue)
                    {
                        ret += ", \"isSubscriptionOnly\": " + kalturaPpv.IsSubscriptionOnly;
                    }
                    ret += ", \"name\": " + "\"" + kalturaPpv.Name + "\"";
                    ret += ", \"price\": " + Serialize(kalturaPpv.Price);
                    ret += ", \"productCode\": " + "\"" + kalturaPpv.ProductCode + "\"";
                    ret += ", \"usageModule\": " + Serialize(kalturaPpv.UsageModule);
                    break;
                    
                case "KalturaPpvEntitlement":
                    KalturaPpvEntitlement kalturaPpvEntitlement = ottObject as KalturaPpvEntitlement;
                    ret += "\"objectType\": " + "\"" + kalturaPpvEntitlement.objectType + "\"";
                    if(kalturaPpvEntitlement.relatedObjects != null && kalturaPpvEntitlement.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPpvEntitlement.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaPpvEntitlement.CurrentDate.HasValue)
                    {
                        ret += ", \"currentDate\": " + kalturaPpvEntitlement.CurrentDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"current_date\": " + kalturaPpvEntitlement.CurrentDate;
                        }
                    }
                    if(kalturaPpvEntitlement.CurrentUses.HasValue)
                    {
                        ret += ", \"currentUses\": " + kalturaPpvEntitlement.CurrentUses;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"current_uses\": " + kalturaPpvEntitlement.CurrentUses;
                        }
                    }
                    ret += ", \"deviceName\": " + "\"" + kalturaPpvEntitlement.DeviceName + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"device_name\": " + "\"" + kalturaPpvEntitlement.DeviceName + "\"";
                    }
                    ret += ", \"deviceUdid\": " + "\"" + kalturaPpvEntitlement.DeviceUDID + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"device_udid\": " + "\"" + kalturaPpvEntitlement.DeviceUDID + "\"";
                    }
                    if(kalturaPpvEntitlement.EndDate.HasValue)
                    {
                        ret += ", \"endDate\": " + kalturaPpvEntitlement.EndDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"end_date\": " + kalturaPpvEntitlement.EndDate;
                        }
                    }
                    ret += ", \"entitlementId\": " + "\"" + kalturaPpvEntitlement.EntitlementId + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"entitlement_id\": " + "\"" + kalturaPpvEntitlement.EntitlementId + "\"";
                    }
                    ret += ", \"householdId\": " + kalturaPpvEntitlement.HouseholdId;
                    if(kalturaPpvEntitlement.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaPpvEntitlement.Id;
                    }
                    if(kalturaPpvEntitlement.IsCancelationWindowEnabled.HasValue)
                    {
                        ret += ", \"isCancelationWindowEnabled\": " + kalturaPpvEntitlement.IsCancelationWindowEnabled;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_cancelation_window_enabled\": " + kalturaPpvEntitlement.IsCancelationWindowEnabled;
                        }
                    }
                    if(kalturaPpvEntitlement.IsInGracePeriod.HasValue)
                    {
                        ret += ", \"isInGracePeriod\": " + kalturaPpvEntitlement.IsInGracePeriod;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_in_grace_period\": " + kalturaPpvEntitlement.IsInGracePeriod;
                        }
                    }
                    if(kalturaPpvEntitlement.IsRenewable.HasValue)
                    {
                        ret += ", \"isRenewable\": " + kalturaPpvEntitlement.IsRenewable;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_renewable\": " + kalturaPpvEntitlement.IsRenewable;
                        }
                    }
                    if(kalturaPpvEntitlement.IsRenewableForPurchase.HasValue)
                    {
                        ret += ", \"isRenewableForPurchase\": " + kalturaPpvEntitlement.IsRenewableForPurchase;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_renewable_for_purchase\": " + kalturaPpvEntitlement.IsRenewableForPurchase;
                        }
                    }
                    if(kalturaPpvEntitlement.LastViewDate.HasValue)
                    {
                        ret += ", \"lastViewDate\": " + kalturaPpvEntitlement.LastViewDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"last_view_date\": " + kalturaPpvEntitlement.LastViewDate;
                        }
                    }
                    if(kalturaPpvEntitlement.MaxUses.HasValue)
                    {
                        ret += ", \"maxUses\": " + kalturaPpvEntitlement.MaxUses;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"max_uses\": " + kalturaPpvEntitlement.MaxUses;
                        }
                    }
                    if(kalturaPpvEntitlement.MediaFileId.HasValue)
                    {
                        ret += ", \"mediaFileId\": " + kalturaPpvEntitlement.MediaFileId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"media_file_id\": " + kalturaPpvEntitlement.MediaFileId;
                        }
                    }
                    if(kalturaPpvEntitlement.MediaId.HasValue)
                    {
                        ret += ", \"mediaId\": " + kalturaPpvEntitlement.MediaId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"media_id\": " + kalturaPpvEntitlement.MediaId;
                        }
                    }
                    if(kalturaPpvEntitlement.NextRenewalDate.HasValue)
                    {
                        ret += ", \"nextRenewalDate\": " + kalturaPpvEntitlement.NextRenewalDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"next_renewal_date\": " + kalturaPpvEntitlement.NextRenewalDate;
                        }
                    }
                    ret += ", \"paymentMethod\": " + kalturaPpvEntitlement.PaymentMethod;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"payment_method\": " + kalturaPpvEntitlement.PaymentMethod;
                    }
                    ret += ", \"productId\": " + "\"" + kalturaPpvEntitlement.ProductId + "\"";
                    if(kalturaPpvEntitlement.PurchaseDate.HasValue)
                    {
                        ret += ", \"purchaseDate\": " + kalturaPpvEntitlement.PurchaseDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"purchase_date\": " + kalturaPpvEntitlement.PurchaseDate;
                        }
                    }
                    if(kalturaPpvEntitlement.PurchaseId.HasValue)
                    {
                        ret += ", \"purchaseId\": " + kalturaPpvEntitlement.PurchaseId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"purchase_id\": " + kalturaPpvEntitlement.PurchaseId;
                        }
                    }
                    ret += ", \"type\": " + kalturaPpvEntitlement.Type;
                    ret += ", \"userId\": " + "\"" + kalturaPpvEntitlement.UserId + "\"";
                    if(kalturaPpvEntitlement.MediaFileId.HasValue)
                    {
                        ret += ", \"mediaFileId\": " + kalturaPpvEntitlement.MediaFileId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"media_file_id\": " + kalturaPpvEntitlement.MediaFileId;
                        }
                    }
                    if(kalturaPpvEntitlement.MediaId.HasValue)
                    {
                        ret += ", \"mediaId\": " + kalturaPpvEntitlement.MediaId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"media_id\": " + kalturaPpvEntitlement.MediaId;
                        }
                    }
                    break;
                    
                case "KalturaPPVItemPriceDetails":
                    KalturaPPVItemPriceDetails kalturaPPVItemPriceDetails = ottObject as KalturaPPVItemPriceDetails;
                    ret += "\"objectType\": " + "\"" + kalturaPPVItemPriceDetails.objectType + "\"";
                    if(kalturaPPVItemPriceDetails.relatedObjects != null && kalturaPPVItemPriceDetails.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPPVItemPriceDetails.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"collectionId\": " + "\"" + kalturaPPVItemPriceDetails.CollectionId + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"collection_id\": " + "\"" + kalturaPPVItemPriceDetails.CollectionId + "\"";
                    }
                    if(kalturaPPVItemPriceDetails.DiscountEndDate.HasValue)
                    {
                        ret += ", \"discountEndDate\": " + kalturaPPVItemPriceDetails.DiscountEndDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"discount_end_date\": " + kalturaPPVItemPriceDetails.DiscountEndDate;
                        }
                    }
                    if(kalturaPPVItemPriceDetails.EndDate.HasValue)
                    {
                        ret += ", \"endDate\": " + kalturaPPVItemPriceDetails.EndDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"end_date\": " + kalturaPPVItemPriceDetails.EndDate;
                        }
                    }
                    ret += ", \"firstDeviceName\": " + "\"" + kalturaPPVItemPriceDetails.FirstDeviceName + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"first_device_name\": " + "\"" + kalturaPPVItemPriceDetails.FirstDeviceName + "\"";
                    }
                    ret += ", \"fullPrice\": " + Serialize(kalturaPPVItemPriceDetails.FullPrice);
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"full_price\": " + Serialize(kalturaPPVItemPriceDetails.FullPrice);
                    }
                    if(kalturaPPVItemPriceDetails.IsInCancelationPeriod.HasValue)
                    {
                        ret += ", \"isInCancelationPeriod\": " + kalturaPPVItemPriceDetails.IsInCancelationPeriod;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_in_cancelation_period\": " + kalturaPPVItemPriceDetails.IsInCancelationPeriod;
                        }
                    }
                    if(kalturaPPVItemPriceDetails.IsSubscriptionOnly.HasValue)
                    {
                        ret += ", \"isSubscriptionOnly\": " + kalturaPPVItemPriceDetails.IsSubscriptionOnly;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_subscription_only\": " + kalturaPPVItemPriceDetails.IsSubscriptionOnly;
                        }
                    }
                    if(kalturaPPVItemPriceDetails.PPVDescriptions != null && kalturaPPVItemPriceDetails.PPVDescriptions.Count > 0)
                    {
                        ret += ", \"ppvDescriptions\": " + "[" + String.Join(", ", kalturaPPVItemPriceDetails.PPVDescriptions.Select(item => Serialize(item))) + "]";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"ppv_descriptions\": " + "[" + String.Join(", ", kalturaPPVItemPriceDetails.PPVDescriptions.Select(item => Serialize(item))) + "]";
                        }
                    }
                    ret += ", \"ppvModuleId\": " + "\"" + kalturaPPVItemPriceDetails.PPVModuleId + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"ppv_module_id\": " + "\"" + kalturaPPVItemPriceDetails.PPVModuleId + "\"";
                    }
                    ret += ", \"prePaidId\": " + "\"" + kalturaPPVItemPriceDetails.PrePaidId + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"pre_paid_id\": " + "\"" + kalturaPPVItemPriceDetails.PrePaidId + "\"";
                    }
                    ret += ", \"price\": " + Serialize(kalturaPPVItemPriceDetails.Price);
                    ret += ", \"ppvProductCode\": " + "\"" + kalturaPPVItemPriceDetails.ProductCode + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"ppv_product_code\": " + "\"" + kalturaPPVItemPriceDetails.ProductCode + "\"";
                    }
                    if(kalturaPPVItemPriceDetails.PurchasedMediaFileId.HasValue)
                    {
                        ret += ", \"purchasedMediaFileId\": " + kalturaPPVItemPriceDetails.PurchasedMediaFileId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"purchased_media_file_id\": " + kalturaPPVItemPriceDetails.PurchasedMediaFileId;
                        }
                    }
                    ret += ", \"purchaseStatus\": " + kalturaPPVItemPriceDetails.PurchaseStatus;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"purchase_status\": " + kalturaPPVItemPriceDetails.PurchaseStatus;
                    }
                    ret += ", \"purchaseUserId\": " + "\"" + kalturaPPVItemPriceDetails.PurchaseUserId + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"purchase_user_id\": " + "\"" + kalturaPPVItemPriceDetails.PurchaseUserId + "\"";
                    }
                    if(kalturaPPVItemPriceDetails.RelatedMediaFileIds != null && kalturaPPVItemPriceDetails.RelatedMediaFileIds.Count > 0)
                    {
                        ret += ", \"relatedMediaFileIds\": " + "[" + String.Join(", ", kalturaPPVItemPriceDetails.RelatedMediaFileIds.Select(item => Serialize(item))) + "]";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"related_media_file_ids\": " + "[" + String.Join(", ", kalturaPPVItemPriceDetails.RelatedMediaFileIds.Select(item => Serialize(item))) + "]";
                        }
                    }
                    if(kalturaPPVItemPriceDetails.StartDate.HasValue)
                    {
                        ret += ", \"startDate\": " + kalturaPPVItemPriceDetails.StartDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"start_date\": " + kalturaPPVItemPriceDetails.StartDate;
                        }
                    }
                    ret += ", \"subscriptionId\": " + "\"" + kalturaPPVItemPriceDetails.SubscriptionId + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"subscription_id\": " + "\"" + kalturaPPVItemPriceDetails.SubscriptionId + "\"";
                    }
                    break;
                    
                case "KalturaPpvPrice":
                    KalturaPpvPrice kalturaPpvPrice = ottObject as KalturaPpvPrice;
                    ret += "\"objectType\": " + "\"" + kalturaPpvPrice.objectType + "\"";
                    if(kalturaPpvPrice.relatedObjects != null && kalturaPpvPrice.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPpvPrice.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"price\": " + Serialize(kalturaPpvPrice.Price);
                    ret += ", \"productId\": " + "\"" + kalturaPpvPrice.ProductId + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"product_id\": " + "\"" + kalturaPpvPrice.ProductId + "\"";
                    }
                    ret += ", \"productType\": " + kalturaPpvPrice.ProductType;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"product_type\": " + kalturaPpvPrice.ProductType;
                    }
                    ret += ", \"purchaseStatus\": " + kalturaPpvPrice.PurchaseStatus;
                    ret += ", \"collectionId\": " + "\"" + kalturaPpvPrice.CollectionId + "\"";
                    if(kalturaPpvPrice.DiscountEndDate.HasValue)
                    {
                        ret += ", \"discountEndDate\": " + kalturaPpvPrice.DiscountEndDate;
                    }
                    if(kalturaPpvPrice.EndDate.HasValue)
                    {
                        ret += ", \"endDate\": " + kalturaPpvPrice.EndDate;
                    }
                    if(kalturaPpvPrice.FileId.HasValue)
                    {
                        ret += ", \"fileId\": " + kalturaPpvPrice.FileId;
                    }
                    ret += ", \"firstDeviceName\": " + "\"" + kalturaPpvPrice.FirstDeviceName + "\"";
                    ret += ", \"fullPrice\": " + Serialize(kalturaPpvPrice.FullPrice);
                    if(kalturaPpvPrice.IsInCancelationPeriod.HasValue)
                    {
                        ret += ", \"isInCancelationPeriod\": " + kalturaPpvPrice.IsInCancelationPeriod;
                    }
                    if(kalturaPpvPrice.IsSubscriptionOnly.HasValue)
                    {
                        ret += ", \"isSubscriptionOnly\": " + kalturaPpvPrice.IsSubscriptionOnly;
                    }
                    if(kalturaPpvPrice.PPVDescriptions != null && kalturaPpvPrice.PPVDescriptions.Count > 0)
                    {
                        ret += ", \"ppvDescriptions\": " + "[" + String.Join(", ", kalturaPpvPrice.PPVDescriptions.Select(item => Serialize(item))) + "]";
                    }
                    ret += ", \"ppvModuleId\": " + "\"" + kalturaPpvPrice.PPVModuleId + "\"";
                    ret += ", \"prePaidId\": " + "\"" + kalturaPpvPrice.PrePaidId + "\"";
                    ret += ", \"ppvProductCode\": " + "\"" + kalturaPpvPrice.ProductCode + "\"";
                    if(kalturaPpvPrice.PurchasedMediaFileId.HasValue)
                    {
                        ret += ", \"purchasedMediaFileId\": " + kalturaPpvPrice.PurchasedMediaFileId;
                    }
                    ret += ", \"purchaseUserId\": " + "\"" + kalturaPpvPrice.PurchaseUserId + "\"";
                    if(kalturaPpvPrice.RelatedMediaFileIds != null && kalturaPpvPrice.RelatedMediaFileIds.Count > 0)
                    {
                        ret += ", \"relatedMediaFileIds\": " + "[" + String.Join(", ", kalturaPpvPrice.RelatedMediaFileIds.Select(item => Serialize(item))) + "]";
                    }
                    if(kalturaPpvPrice.StartDate.HasValue)
                    {
                        ret += ", \"startDate\": " + kalturaPpvPrice.StartDate;
                    }
                    ret += ", \"subscriptionId\": " + "\"" + kalturaPpvPrice.SubscriptionId + "\"";
                    break;
                    
                case "KalturaPremiumService":
                    KalturaPremiumService kalturaPremiumService = ottObject as KalturaPremiumService;
                    ret += "\"objectType\": " + "\"" + kalturaPremiumService.objectType + "\"";
                    if(kalturaPremiumService.relatedObjects != null && kalturaPremiumService.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPremiumService.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaPremiumService.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaPremiumService.Id;
                    }
                    ret += ", \"name\": " + "\"" + kalturaPremiumService.Name + "\"";
                    break;
                    
                case "KalturaPreviewModule":
                    KalturaPreviewModule kalturaPreviewModule = ottObject as KalturaPreviewModule;
                    ret += "\"objectType\": " + "\"" + kalturaPreviewModule.objectType + "\"";
                    if(kalturaPreviewModule.relatedObjects != null && kalturaPreviewModule.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPreviewModule.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaPreviewModule.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaPreviewModule.Id;
                    }
                    if(kalturaPreviewModule.LifeCycle.HasValue)
                    {
                        ret += ", \"lifeCycle\": " + kalturaPreviewModule.LifeCycle;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"life_cycle\": " + kalturaPreviewModule.LifeCycle;
                        }
                    }
                    ret += ", \"name\": " + "\"" + kalturaPreviewModule.Name + "\"";
                    if(kalturaPreviewModule.NonRenewablePeriod.HasValue)
                    {
                        ret += ", \"nonRenewablePeriod\": " + kalturaPreviewModule.NonRenewablePeriod;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"non_renewable_period\": " + kalturaPreviewModule.NonRenewablePeriod;
                        }
                    }
                    break;
                    
                case "KalturaPrice":
                    KalturaPrice kalturaPrice = ottObject as KalturaPrice;
                    ret += "\"objectType\": " + "\"" + kalturaPrice.objectType + "\"";
                    if(kalturaPrice.relatedObjects != null && kalturaPrice.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPrice.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaPrice.Amount.HasValue)
                    {
                        ret += ", \"amount\": " + kalturaPrice.Amount;
                    }
                    if(kalturaPrice.CountryId.HasValue)
                    {
                        ret += ", \"countryId\": " + kalturaPrice.CountryId;
                    }
                    ret += ", \"currency\": " + "\"" + kalturaPrice.Currency + "\"";
                    ret += ", \"currencySign\": " + "\"" + kalturaPrice.CurrencySign + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"currency_sign\": " + "\"" + kalturaPrice.CurrencySign + "\"";
                    }
                    break;
                    
                case "KalturaPriceDetails":
                    KalturaPriceDetails kalturaPriceDetails = ottObject as KalturaPriceDetails;
                    ret += "\"objectType\": " + "\"" + kalturaPriceDetails.objectType + "\"";
                    if(kalturaPriceDetails.relatedObjects != null && kalturaPriceDetails.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPriceDetails.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaPriceDetails.Descriptions != null && kalturaPriceDetails.Descriptions.Count > 0)
                    {
                        ret += ", \"descriptions\": " + "[" + String.Join(", ", kalturaPriceDetails.Descriptions.Select(item => Serialize(item))) + "]";
                    }
                    if(kalturaPriceDetails.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaPriceDetails.Id;
                    }
                    if(kalturaPriceDetails.MultiCurrencyPrice != null && kalturaPriceDetails.MultiCurrencyPrice.Count > 0)
                    {
                        ret += ", \"multiCurrencyPrice\": " + "[" + String.Join(", ", kalturaPriceDetails.MultiCurrencyPrice.Select(item => Serialize(item))) + "]";
                    }
                    ret += ", \"name\": " + "\"" + kalturaPriceDetails.name + "\"";
                    ret += ", \"price\": " + Serialize(kalturaPriceDetails.Price);
                    break;
                    
                case "KalturaPriceDetailsFilter":
                    KalturaPriceDetailsFilter kalturaPriceDetailsFilter = ottObject as KalturaPriceDetailsFilter;
                    ret += "\"objectType\": " + "\"" + kalturaPriceDetailsFilter.objectType + "\"";
                    if(kalturaPriceDetailsFilter.relatedObjects != null && kalturaPriceDetailsFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPriceDetailsFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaPriceDetailsFilter.OrderBy;
                    ret += ", \"idIn\": " + "\"" + kalturaPriceDetailsFilter.IdIn + "\"";
                    break;
                    
                case "KalturaPriceDetailsListResponse":
                    KalturaPriceDetailsListResponse kalturaPriceDetailsListResponse = ottObject as KalturaPriceDetailsListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaPriceDetailsListResponse.objectType + "\"";
                    if(kalturaPriceDetailsListResponse.relatedObjects != null && kalturaPriceDetailsListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPriceDetailsListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaPriceDetailsListResponse.TotalCount;
                    if(kalturaPriceDetailsListResponse.Prices != null && kalturaPriceDetailsListResponse.Prices.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaPriceDetailsListResponse.Prices.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaPricePlan":
                    KalturaPricePlan kalturaPricePlan = ottObject as KalturaPricePlan;
                    ret += "\"objectType\": " + "\"" + kalturaPricePlan.objectType + "\"";
                    if(kalturaPricePlan.relatedObjects != null && kalturaPricePlan.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPricePlan.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaPricePlan.CouponId.HasValue)
                    {
                        ret += ", \"couponId\": " + kalturaPricePlan.CouponId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"coupon_id\": " + kalturaPricePlan.CouponId;
                        }
                    }
                    if(kalturaPricePlan.FullLifeCycle.HasValue)
                    {
                        ret += ", \"fullLifeCycle\": " + kalturaPricePlan.FullLifeCycle;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"full_life_cycle\": " + kalturaPricePlan.FullLifeCycle;
                        }
                    }
                    if(kalturaPricePlan.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaPricePlan.Id;
                    }
                    if(kalturaPricePlan.IsOfflinePlayback.HasValue)
                    {
                        ret += ", \"isOfflinePlayback\": " + kalturaPricePlan.IsOfflinePlayback;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_offline_playback\": " + kalturaPricePlan.IsOfflinePlayback;
                        }
                    }
                    if(kalturaPricePlan.IsWaiverEnabled.HasValue)
                    {
                        ret += ", \"isWaiverEnabled\": " + kalturaPricePlan.IsWaiverEnabled;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_waiver_enabled\": " + kalturaPricePlan.IsWaiverEnabled;
                        }
                    }
                    if(kalturaPricePlan.MaxViewsNumber.HasValue)
                    {
                        ret += ", \"maxViewsNumber\": " + kalturaPricePlan.MaxViewsNumber;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"max_views_number\": " + kalturaPricePlan.MaxViewsNumber;
                        }
                    }
                    ret += ", \"name\": " + "\"" + kalturaPricePlan.Name + "\"";
                    if(kalturaPricePlan.ViewLifeCycle.HasValue)
                    {
                        ret += ", \"viewLifeCycle\": " + kalturaPricePlan.ViewLifeCycle;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"view_life_cycle\": " + kalturaPricePlan.ViewLifeCycle;
                        }
                    }
                    if(kalturaPricePlan.WaiverPeriod.HasValue)
                    {
                        ret += ", \"waiverPeriod\": " + kalturaPricePlan.WaiverPeriod;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"waiver_period\": " + kalturaPricePlan.WaiverPeriod;
                        }
                    }
                    if(kalturaPricePlan.DiscountId.HasValue)
                    {
                        ret += ", \"discountId\": " + kalturaPricePlan.DiscountId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"discount_id\": " + kalturaPricePlan.DiscountId;
                        }
                    }
                    if(kalturaPricePlan.IsRenewable.HasValue)
                    {
                        ret += ", \"isRenewable\": " + kalturaPricePlan.IsRenewable;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_renewable\": " + kalturaPricePlan.IsRenewable;
                        }
                    }
                    if(kalturaPricePlan.PriceDetailsId.HasValue)
                    {
                        ret += ", \"priceDetailsId\": " + kalturaPricePlan.PriceDetailsId;
                    }
                    if(kalturaPricePlan.PriceId.HasValue)
                    {
                        ret += ", \"priceId\": " + kalturaPricePlan.PriceId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"price_id\": " + kalturaPricePlan.PriceId;
                        }
                    }
                    if(kalturaPricePlan.RenewalsNumber.HasValue)
                    {
                        ret += ", \"renewalsNumber\": " + kalturaPricePlan.RenewalsNumber;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"renewals_number\": " + kalturaPricePlan.RenewalsNumber;
                        }
                    }
                    break;
                    
                case "KalturaPricePlanFilter":
                    KalturaPricePlanFilter kalturaPricePlanFilter = ottObject as KalturaPricePlanFilter;
                    ret += "\"objectType\": " + "\"" + kalturaPricePlanFilter.objectType + "\"";
                    if(kalturaPricePlanFilter.relatedObjects != null && kalturaPricePlanFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPricePlanFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaPricePlanFilter.OrderBy;
                    ret += ", \"idIn\": " + "\"" + kalturaPricePlanFilter.IdIn + "\"";
                    break;
                    
                case "KalturaPricePlanListResponse":
                    KalturaPricePlanListResponse kalturaPricePlanListResponse = ottObject as KalturaPricePlanListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaPricePlanListResponse.objectType + "\"";
                    if(kalturaPricePlanListResponse.relatedObjects != null && kalturaPricePlanListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPricePlanListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaPricePlanListResponse.TotalCount;
                    if(kalturaPricePlanListResponse.PricePlans != null && kalturaPricePlanListResponse.PricePlans.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaPricePlanListResponse.PricePlans.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaPricesFilter":
                    KalturaPricesFilter kalturaPricesFilter = ottObject as KalturaPricesFilter;
                    ret += "\"objectType\": " + "\"" + kalturaPricesFilter.objectType + "\"";
                    if(kalturaPricesFilter.relatedObjects != null && kalturaPricesFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPricesFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaPricesFilter.FilesIds != null && kalturaPricesFilter.FilesIds.Count > 0)
                    {
                        ret += ", \"filesIds\": " + "[" + String.Join(", ", kalturaPricesFilter.FilesIds.Select(item => Serialize(item))) + "]";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"files_ids\": " + "[" + String.Join(", ", kalturaPricesFilter.FilesIds.Select(item => Serialize(item))) + "]";
                        }
                    }
                    if(kalturaPricesFilter.ShouldGetOnlyLowest.HasValue)
                    {
                        ret += ", \"shouldGetOnlyLowest\": " + kalturaPricesFilter.ShouldGetOnlyLowest;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"should_get_only_lowest\": " + kalturaPricesFilter.ShouldGetOnlyLowest;
                        }
                    }
                    if(kalturaPricesFilter.SubscriptionsIds != null && kalturaPricesFilter.SubscriptionsIds.Count > 0)
                    {
                        ret += ", \"subscriptionsIds\": " + "[" + String.Join(", ", kalturaPricesFilter.SubscriptionsIds.Select(item => Serialize(item))) + "]";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"subscriptions_ids\": " + "[" + String.Join(", ", kalturaPricesFilter.SubscriptionsIds.Select(item => Serialize(item))) + "]";
                        }
                    }
                    break;
                    
                case "KalturaProductCode":
                    KalturaProductCode kalturaProductCode = ottObject as KalturaProductCode;
                    ret += "\"objectType\": " + "\"" + kalturaProductCode.objectType + "\"";
                    if(kalturaProductCode.relatedObjects != null && kalturaProductCode.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaProductCode.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"code\": " + "\"" + kalturaProductCode.Code + "\"";
                    ret += ", \"inappProvider\": " + "\"" + kalturaProductCode.InappProvider + "\"";
                    break;
                    
                case "KalturaProductPrice":
                    KalturaProductPrice kalturaProductPrice = ottObject as KalturaProductPrice;
                    ret += "\"objectType\": " + "\"" + kalturaProductPrice.objectType + "\"";
                    if(kalturaProductPrice.relatedObjects != null && kalturaProductPrice.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaProductPrice.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"price\": " + Serialize(kalturaProductPrice.Price);
                    ret += ", \"productId\": " + "\"" + kalturaProductPrice.ProductId + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"product_id\": " + "\"" + kalturaProductPrice.ProductId + "\"";
                    }
                    ret += ", \"productType\": " + kalturaProductPrice.ProductType;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"product_type\": " + kalturaProductPrice.ProductType;
                    }
                    ret += ", \"purchaseStatus\": " + kalturaProductPrice.PurchaseStatus;
                    break;
                    
                case "KalturaProductPriceFilter":
                    KalturaProductPriceFilter kalturaProductPriceFilter = ottObject as KalturaProductPriceFilter;
                    ret += "\"objectType\": " + "\"" + kalturaProductPriceFilter.objectType + "\"";
                    if(kalturaProductPriceFilter.relatedObjects != null && kalturaProductPriceFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaProductPriceFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaProductPriceFilter.OrderBy;
                    ret += ", \"collectionIdIn\": " + "\"" + kalturaProductPriceFilter.CollectionIdIn + "\"";
                    ret += ", \"couponCodeEqual\": " + "\"" + kalturaProductPriceFilter.CouponCodeEqual + "\"";
                    ret += ", \"fileIdIn\": " + "\"" + kalturaProductPriceFilter.FileIdIn + "\"";
                    if(kalturaProductPriceFilter.isLowest.HasValue)
                    {
                        ret += ", \"isLowest\": " + kalturaProductPriceFilter.isLowest;
                    }
                    ret += ", \"subscriptionIdIn\": " + "\"" + kalturaProductPriceFilter.SubscriptionIdIn + "\"";
                    break;
                    
                case "KalturaProductPriceListResponse":
                    KalturaProductPriceListResponse kalturaProductPriceListResponse = ottObject as KalturaProductPriceListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaProductPriceListResponse.objectType + "\"";
                    if(kalturaProductPriceListResponse.relatedObjects != null && kalturaProductPriceListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaProductPriceListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaProductPriceListResponse.TotalCount;
                    if(kalturaProductPriceListResponse.ProductsPrices != null && kalturaProductPriceListResponse.ProductsPrices.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaProductPriceListResponse.ProductsPrices.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaProductsPriceListResponse":
                    KalturaProductsPriceListResponse kalturaProductsPriceListResponse = ottObject as KalturaProductsPriceListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaProductsPriceListResponse.objectType + "\"";
                    if(kalturaProductsPriceListResponse.relatedObjects != null && kalturaProductsPriceListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaProductsPriceListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaProductsPriceListResponse.TotalCount;
                    if(kalturaProductsPriceListResponse.ProductsPrices != null && kalturaProductsPriceListResponse.ProductsPrices.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaProductsPriceListResponse.ProductsPrices.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaProgramAsset":
                    KalturaProgramAsset kalturaProgramAsset = ottObject as KalturaProgramAsset;
                    ret += "\"objectType\": " + "\"" + kalturaProgramAsset.objectType + "\"";
                    if(kalturaProgramAsset.relatedObjects != null && kalturaProgramAsset.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaProgramAsset.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"description\": " + Serialize(kalturaProgramAsset.Description);
                    if(kalturaProgramAsset.EnableCatchUp.HasValue)
                    {
                        ret += ", \"enableCatchUp\": " + kalturaProgramAsset.EnableCatchUp;
                    }
                    if(kalturaProgramAsset.EnableCdvr.HasValue)
                    {
                        ret += ", \"enableCdvr\": " + kalturaProgramAsset.EnableCdvr;
                    }
                    if(kalturaProgramAsset.EnableStartOver.HasValue)
                    {
                        ret += ", \"enableStartOver\": " + kalturaProgramAsset.EnableStartOver;
                    }
                    if(kalturaProgramAsset.EnableTrickPlay.HasValue)
                    {
                        ret += ", \"enableTrickPlay\": " + kalturaProgramAsset.EnableTrickPlay;
                    }
                    if(kalturaProgramAsset.EndDate.HasValue)
                    {
                        ret += ", \"endDate\": " + kalturaProgramAsset.EndDate;
                    }
                    ret += ", \"externalId\": " + "\"" + kalturaProgramAsset.ExternalId + "\"";
                    if(kalturaProgramAsset.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaProgramAsset.Id;
                    }
                    if(kalturaProgramAsset.Images != null && kalturaProgramAsset.Images.Count > 0)
                    {
                        ret += ", \"images\": " + "[" + String.Join(", ", kalturaProgramAsset.Images.Select(item => Serialize(item))) + "]";
                    }
                    if(kalturaProgramAsset.MediaFiles != null && kalturaProgramAsset.MediaFiles.Count > 0)
                    {
                        ret += ", \"mediaFiles\": " + "[" + String.Join(", ", kalturaProgramAsset.MediaFiles.Select(item => Serialize(item))) + "]";
                    }
                    if(kalturaProgramAsset.Metas != null && kalturaProgramAsset.Metas.Count > 0)
                    {
                        ret += ", \"metas\": " + "{" + String.Join(", ", kalturaProgramAsset.Metas.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"name\": " + Serialize(kalturaProgramAsset.Name);
                    if(kalturaProgramAsset.StartDate.HasValue)
                    {
                        ret += ", \"startDate\": " + kalturaProgramAsset.StartDate;
                    }
                    ret += ", \"stats\": " + Serialize(kalturaProgramAsset.Statistics);
                    if(kalturaProgramAsset.Tags != null && kalturaProgramAsset.Tags.Count > 0)
                    {
                        ret += ", \"tags\": " + "{" + String.Join(", ", kalturaProgramAsset.Tags.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaProgramAsset.Type.HasValue)
                    {
                        ret += ", \"type\": " + kalturaProgramAsset.Type;
                    }
                    ret += ", \"crid\": " + "\"" + kalturaProgramAsset.Crid + "\"";
                    if(kalturaProgramAsset.EpgChannelId.HasValue)
                    {
                        ret += ", \"epgChannelId\": " + kalturaProgramAsset.EpgChannelId;
                    }
                    ret += ", \"epgId\": " + "\"" + kalturaProgramAsset.EpgId + "\"";
                    if(kalturaProgramAsset.LinearAssetId.HasValue)
                    {
                        ret += ", \"linearAssetId\": " + kalturaProgramAsset.LinearAssetId;
                    }
                    if(kalturaProgramAsset.RelatedMediaId.HasValue)
                    {
                        ret += ", \"relatedMediaId\": " + kalturaProgramAsset.RelatedMediaId;
                    }
                    break;
                    
                case "KalturaPurchase":
                    KalturaPurchase kalturaPurchase = ottObject as KalturaPurchase;
                    ret += "\"objectType\": " + "\"" + kalturaPurchase.objectType + "\"";
                    if(kalturaPurchase.relatedObjects != null && kalturaPurchase.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPurchase.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaPurchase.ContentId.HasValue)
                    {
                        ret += ", \"contentId\": " + kalturaPurchase.ContentId;
                    }
                    ret += ", \"productId\": " + kalturaPurchase.ProductId;
                    ret += ", \"productType\": " + kalturaPurchase.ProductType;
                    ret += ", \"adapterData\": " + "\"" + kalturaPurchase.AdapterData + "\"";
                    ret += ", \"coupon\": " + "\"" + kalturaPurchase.Coupon + "\"";
                    ret += ", \"currency\": " + "\"" + kalturaPurchase.Currency + "\"";
                    if(kalturaPurchase.PaymentGatewayId.HasValue)
                    {
                        ret += ", \"paymentGatewayId\": " + kalturaPurchase.PaymentGatewayId;
                    }
                    if(kalturaPurchase.PaymentMethodId.HasValue)
                    {
                        ret += ", \"paymentMethodId\": " + kalturaPurchase.PaymentMethodId;
                    }
                    ret += ", \"price\": " + kalturaPurchase.Price;
                    break;
                    
                case "KalturaPurchaseBase":
                    KalturaPurchaseBase kalturaPurchaseBase = ottObject as KalturaPurchaseBase;
                    ret += "\"objectType\": " + "\"" + kalturaPurchaseBase.objectType + "\"";
                    if(kalturaPurchaseBase.relatedObjects != null && kalturaPurchaseBase.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPurchaseBase.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaPurchaseBase.ContentId.HasValue)
                    {
                        ret += ", \"contentId\": " + kalturaPurchaseBase.ContentId;
                    }
                    ret += ", \"productId\": " + kalturaPurchaseBase.ProductId;
                    ret += ", \"productType\": " + kalturaPurchaseBase.ProductType;
                    break;
                    
                case "KalturaPurchaseSession":
                    KalturaPurchaseSession kalturaPurchaseSession = ottObject as KalturaPurchaseSession;
                    ret += "\"objectType\": " + "\"" + kalturaPurchaseSession.objectType + "\"";
                    if(kalturaPurchaseSession.relatedObjects != null && kalturaPurchaseSession.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPurchaseSession.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaPurchaseSession.ContentId.HasValue)
                    {
                        ret += ", \"contentId\": " + kalturaPurchaseSession.ContentId;
                    }
                    ret += ", \"productId\": " + kalturaPurchaseSession.ProductId;
                    ret += ", \"productType\": " + kalturaPurchaseSession.ProductType;
                    ret += ", \"adapterData\": " + "\"" + kalturaPurchaseSession.AdapterData + "\"";
                    ret += ", \"coupon\": " + "\"" + kalturaPurchaseSession.Coupon + "\"";
                    ret += ", \"currency\": " + "\"" + kalturaPurchaseSession.Currency + "\"";
                    if(kalturaPurchaseSession.PaymentGatewayId.HasValue)
                    {
                        ret += ", \"paymentGatewayId\": " + kalturaPurchaseSession.PaymentGatewayId;
                    }
                    if(kalturaPurchaseSession.PaymentMethodId.HasValue)
                    {
                        ret += ", \"paymentMethodId\": " + kalturaPurchaseSession.PaymentMethodId;
                    }
                    ret += ", \"price\": " + kalturaPurchaseSession.Price;
                    if(kalturaPurchaseSession.PreviewModuleId.HasValue)
                    {
                        ret += ", \"previewModuleId\": " + kalturaPurchaseSession.PreviewModuleId;
                    }
                    break;
                    
                case "KalturaPurchaseSettings":
                    KalturaPurchaseSettings kalturaPurchaseSettings = ottObject as KalturaPurchaseSettings;
                    ret += "\"objectType\": " + "\"" + kalturaPurchaseSettings.objectType + "\"";
                    if(kalturaPurchaseSettings.relatedObjects != null && kalturaPurchaseSettings.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPurchaseSettings.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"origin\": " + kalturaPurchaseSettings.Origin;
                    ret += ", \"pin\": " + "\"" + kalturaPurchaseSettings.PIN + "\"";
                    ret += ", \"type\": " + kalturaPurchaseSettings.Type;
                    if(kalturaPurchaseSettings.Permission.HasValue)
                    {
                        ret += ", \"permission\": " + kalturaPurchaseSettings.Permission;
                    }
                    break;
                    
                case "KalturaPurchaseSettingsResponse":
                    KalturaPurchaseSettingsResponse kalturaPurchaseSettingsResponse = ottObject as KalturaPurchaseSettingsResponse;
                    ret += "\"objectType\": " + "\"" + kalturaPurchaseSettingsResponse.objectType + "\"";
                    if(kalturaPurchaseSettingsResponse.relatedObjects != null && kalturaPurchaseSettingsResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPurchaseSettingsResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"origin\": " + kalturaPurchaseSettingsResponse.Origin;
                    ret += ", \"pin\": " + "\"" + kalturaPurchaseSettingsResponse.PIN + "\"";
                    ret += ", \"type\": " + kalturaPurchaseSettingsResponse.Type;
                    if(kalturaPurchaseSettingsResponse.PurchaseSettingsType.HasValue)
                    {
                        ret += ", \"purchaseSettingsType\": " + kalturaPurchaseSettingsResponse.PurchaseSettingsType;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"purchase_settings_type\": " + kalturaPurchaseSettingsResponse.PurchaseSettingsType;
                        }
                    }
                    break;
                    
                case "KalturaPushMessage":
                    KalturaPushMessage kalturaPushMessage = ottObject as KalturaPushMessage;
                    ret += "\"objectType\": " + "\"" + kalturaPushMessage.objectType + "\"";
                    if(kalturaPushMessage.relatedObjects != null && kalturaPushMessage.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPushMessage.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"action\": " + "\"" + kalturaPushMessage.Action + "\"";
                    ret += ", \"message\": " + "\"" + kalturaPushMessage.Message + "\"";
                    ret += ", \"sound\": " + "\"" + kalturaPushMessage.Sound + "\"";
                    ret += ", \"url\": " + "\"" + kalturaPushMessage.Url + "\"";
                    break;
                    
                case "KalturaPushParams":
                    KalturaPushParams kalturaPushParams = ottObject as KalturaPushParams;
                    ret += "\"objectType\": " + "\"" + kalturaPushParams.objectType + "\"";
                    if(kalturaPushParams.relatedObjects != null && kalturaPushParams.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaPushParams.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"externalToken\": " + "\"" + kalturaPushParams.ExternalToken + "\"";
                    ret += ", \"token\": " + "\"" + kalturaPushParams.Token + "\"";
                    break;
                    
                case "KalturaRecommendationProfile":
                    KalturaRecommendationProfile kalturaRecommendationProfile = ottObject as KalturaRecommendationProfile;
                    ret += "\"objectType\": " + "\"" + kalturaRecommendationProfile.objectType + "\"";
                    if(kalturaRecommendationProfile.relatedObjects != null && kalturaRecommendationProfile.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaRecommendationProfile.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"adapterUrl\": " + "\"" + kalturaRecommendationProfile.AdapterUrl + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"adapter_url\": " + "\"" + kalturaRecommendationProfile.AdapterUrl + "\"";
                    }
                    ret += ", \"externalIdentifier\": " + "\"" + kalturaRecommendationProfile.ExternalIdentifier + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"external_identifier\": " + "\"" + kalturaRecommendationProfile.ExternalIdentifier + "\"";
                    }
                    if(kalturaRecommendationProfile.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaRecommendationProfile.Id;
                    }
                    if(kalturaRecommendationProfile.IsActive.HasValue)
                    {
                        ret += ", \"isActive\": " + kalturaRecommendationProfile.IsActive;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_active\": " + kalturaRecommendationProfile.IsActive;
                        }
                    }
                    ret += ", \"name\": " + "\"" + kalturaRecommendationProfile.Name + "\"";
                    if(kalturaRecommendationProfile.Settings != null && kalturaRecommendationProfile.Settings.Count > 0)
                    {
                        ret += ", \"recommendationEngineSettings\": " + "{" + String.Join(", ", kalturaRecommendationProfile.Settings.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"recommendation_engine_settings\": " + "{" + String.Join(", ", kalturaRecommendationProfile.Settings.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                        }
                    }
                    ret += ", \"sharedSecret\": " + "\"" + kalturaRecommendationProfile.SharedSecret + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"shared_secret\": " + "\"" + kalturaRecommendationProfile.SharedSecret + "\"";
                    }
                    break;
                    
                case "KalturaRecommendationProfileListResponse":
                    KalturaRecommendationProfileListResponse kalturaRecommendationProfileListResponse = ottObject as KalturaRecommendationProfileListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaRecommendationProfileListResponse.objectType + "\"";
                    if(kalturaRecommendationProfileListResponse.relatedObjects != null && kalturaRecommendationProfileListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaRecommendationProfileListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaRecommendationProfileListResponse.TotalCount;
                    if(kalturaRecommendationProfileListResponse.RecommendationProfiles != null && kalturaRecommendationProfileListResponse.RecommendationProfiles.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaRecommendationProfileListResponse.RecommendationProfiles.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaRecording":
                    KalturaRecording kalturaRecording = ottObject as KalturaRecording;
                    ret += "\"objectType\": " + "\"" + kalturaRecording.objectType + "\"";
                    if(kalturaRecording.relatedObjects != null && kalturaRecording.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaRecording.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"assetId\": " + kalturaRecording.AssetId;
                    ret += ", \"createDate\": " + kalturaRecording.CreateDate;
                    if(kalturaRecording.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaRecording.Id;
                    }
                    ret += ", \"isProtected\": " + kalturaRecording.IsProtected;
                    ret += ", \"status\": " + kalturaRecording.Status;
                    ret += ", \"type\": " + kalturaRecording.Type;
                    ret += ", \"updateDate\": " + kalturaRecording.UpdateDate;
                    if(kalturaRecording.ViewableUntilDate.HasValue)
                    {
                        ret += ", \"viewableUntilDate\": " + kalturaRecording.ViewableUntilDate;
                    }
                    break;
                    
                case "KalturaRecordingAsset":
                    KalturaRecordingAsset kalturaRecordingAsset = ottObject as KalturaRecordingAsset;
                    ret += "\"objectType\": " + "\"" + kalturaRecordingAsset.objectType + "\"";
                    if(kalturaRecordingAsset.relatedObjects != null && kalturaRecordingAsset.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaRecordingAsset.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"description\": " + Serialize(kalturaRecordingAsset.Description);
                    if(kalturaRecordingAsset.EnableCatchUp.HasValue)
                    {
                        ret += ", \"enableCatchUp\": " + kalturaRecordingAsset.EnableCatchUp;
                    }
                    if(kalturaRecordingAsset.EnableCdvr.HasValue)
                    {
                        ret += ", \"enableCdvr\": " + kalturaRecordingAsset.EnableCdvr;
                    }
                    if(kalturaRecordingAsset.EnableStartOver.HasValue)
                    {
                        ret += ", \"enableStartOver\": " + kalturaRecordingAsset.EnableStartOver;
                    }
                    if(kalturaRecordingAsset.EnableTrickPlay.HasValue)
                    {
                        ret += ", \"enableTrickPlay\": " + kalturaRecordingAsset.EnableTrickPlay;
                    }
                    if(kalturaRecordingAsset.EndDate.HasValue)
                    {
                        ret += ", \"endDate\": " + kalturaRecordingAsset.EndDate;
                    }
                    ret += ", \"externalId\": " + "\"" + kalturaRecordingAsset.ExternalId + "\"";
                    if(kalturaRecordingAsset.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaRecordingAsset.Id;
                    }
                    if(kalturaRecordingAsset.Images != null && kalturaRecordingAsset.Images.Count > 0)
                    {
                        ret += ", \"images\": " + "[" + String.Join(", ", kalturaRecordingAsset.Images.Select(item => Serialize(item))) + "]";
                    }
                    if(kalturaRecordingAsset.MediaFiles != null && kalturaRecordingAsset.MediaFiles.Count > 0)
                    {
                        ret += ", \"mediaFiles\": " + "[" + String.Join(", ", kalturaRecordingAsset.MediaFiles.Select(item => Serialize(item))) + "]";
                    }
                    if(kalturaRecordingAsset.Metas != null && kalturaRecordingAsset.Metas.Count > 0)
                    {
                        ret += ", \"metas\": " + "{" + String.Join(", ", kalturaRecordingAsset.Metas.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"name\": " + Serialize(kalturaRecordingAsset.Name);
                    if(kalturaRecordingAsset.StartDate.HasValue)
                    {
                        ret += ", \"startDate\": " + kalturaRecordingAsset.StartDate;
                    }
                    ret += ", \"stats\": " + Serialize(kalturaRecordingAsset.Statistics);
                    if(kalturaRecordingAsset.Tags != null && kalturaRecordingAsset.Tags.Count > 0)
                    {
                        ret += ", \"tags\": " + "{" + String.Join(", ", kalturaRecordingAsset.Tags.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaRecordingAsset.Type.HasValue)
                    {
                        ret += ", \"type\": " + kalturaRecordingAsset.Type;
                    }
                    ret += ", \"crid\": " + "\"" + kalturaRecordingAsset.Crid + "\"";
                    if(kalturaRecordingAsset.EpgChannelId.HasValue)
                    {
                        ret += ", \"epgChannelId\": " + kalturaRecordingAsset.EpgChannelId;
                    }
                    ret += ", \"epgId\": " + "\"" + kalturaRecordingAsset.EpgId + "\"";
                    if(kalturaRecordingAsset.LinearAssetId.HasValue)
                    {
                        ret += ", \"linearAssetId\": " + kalturaRecordingAsset.LinearAssetId;
                    }
                    if(kalturaRecordingAsset.RelatedMediaId.HasValue)
                    {
                        ret += ", \"relatedMediaId\": " + kalturaRecordingAsset.RelatedMediaId;
                    }
                    ret += ", \"recordingId\": " + "\"" + kalturaRecordingAsset.RecordingId + "\"";
                    if(kalturaRecordingAsset.RecordingType.HasValue)
                    {
                        ret += ", \"recordingType\": " + kalturaRecordingAsset.RecordingType;
                    }
                    break;
                    
                case "KalturaRecordingContext":
                    KalturaRecordingContext kalturaRecordingContext = ottObject as KalturaRecordingContext;
                    ret += "\"objectType\": " + "\"" + kalturaRecordingContext.objectType + "\"";
                    if(kalturaRecordingContext.relatedObjects != null && kalturaRecordingContext.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaRecordingContext.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"assetId\": " + kalturaRecordingContext.AssetId;
                    ret += ", \"code\": " + kalturaRecordingContext.Code;
                    ret += ", \"message\": " + "\"" + kalturaRecordingContext.Message + "\"";
                    ret += ", \"recording\": " + Serialize(kalturaRecordingContext.Recording);
                    break;
                    
                case "KalturaRecordingContextFilter":
                    KalturaRecordingContextFilter kalturaRecordingContextFilter = ottObject as KalturaRecordingContextFilter;
                    ret += "\"objectType\": " + "\"" + kalturaRecordingContextFilter.objectType + "\"";
                    if(kalturaRecordingContextFilter.relatedObjects != null && kalturaRecordingContextFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaRecordingContextFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaRecordingContextFilter.OrderBy;
                    ret += ", \"assetIdIn\": " + "\"" + kalturaRecordingContextFilter.AssetIdIn + "\"";
                    break;
                    
                case "KalturaRecordingContextListResponse":
                    KalturaRecordingContextListResponse kalturaRecordingContextListResponse = ottObject as KalturaRecordingContextListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaRecordingContextListResponse.objectType + "\"";
                    if(kalturaRecordingContextListResponse.relatedObjects != null && kalturaRecordingContextListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaRecordingContextListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaRecordingContextListResponse.TotalCount;
                    if(kalturaRecordingContextListResponse.Objects != null && kalturaRecordingContextListResponse.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaRecordingContextListResponse.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaRecordingFilter":
                    KalturaRecordingFilter kalturaRecordingFilter = ottObject as KalturaRecordingFilter;
                    ret += "\"objectType\": " + "\"" + kalturaRecordingFilter.objectType + "\"";
                    if(kalturaRecordingFilter.relatedObjects != null && kalturaRecordingFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaRecordingFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaRecordingFilter.OrderBy;
                    ret += ", \"filterExpression\": " + "\"" + kalturaRecordingFilter.FilterExpression + "\"";
                    ret += ", \"statusIn\": " + "\"" + kalturaRecordingFilter.StatusIn + "\"";
                    break;
                    
                case "KalturaRecordingListResponse":
                    KalturaRecordingListResponse kalturaRecordingListResponse = ottObject as KalturaRecordingListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaRecordingListResponse.objectType + "\"";
                    if(kalturaRecordingListResponse.relatedObjects != null && kalturaRecordingListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaRecordingListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaRecordingListResponse.TotalCount;
                    if(kalturaRecordingListResponse.Objects != null && kalturaRecordingListResponse.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaRecordingListResponse.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaRegion":
                    KalturaRegion kalturaRegion = ottObject as KalturaRegion;
                    ret += "\"objectType\": " + "\"" + kalturaRegion.objectType + "\"";
                    if(kalturaRegion.relatedObjects != null && kalturaRegion.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaRegion.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"externalId\": " + "\"" + kalturaRegion.ExternalId + "\"";
                    ret += ", \"id\": " + kalturaRegion.Id;
                    ret += ", \"isDefault\": " + kalturaRegion.IsDefault;
                    ret += ", \"name\": " + "\"" + kalturaRegion.Name + "\"";
                    if(kalturaRegion.RegionalChannels != null && kalturaRegion.RegionalChannels.Count > 0)
                    {
                        ret += ", \"linearChannels\": " + "[" + String.Join(", ", kalturaRegion.RegionalChannels.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaRegionalChannel":
                    KalturaRegionalChannel kalturaRegionalChannel = ottObject as KalturaRegionalChannel;
                    ret += "\"objectType\": " + "\"" + kalturaRegionalChannel.objectType + "\"";
                    if(kalturaRegionalChannel.relatedObjects != null && kalturaRegionalChannel.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaRegionalChannel.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"channelNumber\": " + kalturaRegionalChannel.ChannelNumber;
                    ret += ", \"linearChannelId\": " + kalturaRegionalChannel.LinearChannelId;
                    break;
                    
                case "KalturaRegionFilter":
                    KalturaRegionFilter kalturaRegionFilter = ottObject as KalturaRegionFilter;
                    ret += "\"objectType\": " + "\"" + kalturaRegionFilter.objectType + "\"";
                    if(kalturaRegionFilter.relatedObjects != null && kalturaRegionFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaRegionFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaRegionFilter.OrderBy;
                    ret += ", \"externalIdIn\": " + "\"" + kalturaRegionFilter.ExternalIdIn + "\"";
                    break;
                    
                case "KalturaRegionListResponse":
                    KalturaRegionListResponse kalturaRegionListResponse = ottObject as KalturaRegionListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaRegionListResponse.objectType + "\"";
                    if(kalturaRegionListResponse.relatedObjects != null && kalturaRegionListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaRegionListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaRegionListResponse.TotalCount;
                    if(kalturaRegionListResponse.Regions != null && kalturaRegionListResponse.Regions.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaRegionListResponse.Regions.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaRegistryResponse":
                    KalturaRegistryResponse kalturaRegistryResponse = ottObject as KalturaRegistryResponse;
                    ret += "\"objectType\": " + "\"" + kalturaRegistryResponse.objectType + "\"";
                    if(kalturaRegistryResponse.relatedObjects != null && kalturaRegistryResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaRegistryResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"announcementId\": " + kalturaRegistryResponse.AnnouncementId;
                    ret += ", \"key\": " + "\"" + kalturaRegistryResponse.Key + "\"";
                    ret += ", \"url\": " + "\"" + kalturaRegistryResponse.Url + "\"";
                    break;
                    
                case "KalturaRegistrySettings":
                    KalturaRegistrySettings kalturaRegistrySettings = ottObject as KalturaRegistrySettings;
                    ret += "\"objectType\": " + "\"" + kalturaRegistrySettings.objectType + "\"";
                    if(kalturaRegistrySettings.relatedObjects != null && kalturaRegistrySettings.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaRegistrySettings.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"key\": " + "\"" + kalturaRegistrySettings.Key + "\"";
                    ret += ", \"value\": " + "\"" + kalturaRegistrySettings.Value + "\"";
                    break;
                    
                case "KalturaRegistrySettingsListResponse":
                    KalturaRegistrySettingsListResponse kalturaRegistrySettingsListResponse = ottObject as KalturaRegistrySettingsListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaRegistrySettingsListResponse.objectType + "\"";
                    if(kalturaRegistrySettingsListResponse.relatedObjects != null && kalturaRegistrySettingsListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaRegistrySettingsListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaRegistrySettingsListResponse.TotalCount;
                    if(kalturaRegistrySettingsListResponse.RegistrySettings != null && kalturaRegistrySettingsListResponse.RegistrySettings.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaRegistrySettingsListResponse.RegistrySettings.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaRelatedExternalFilter":
                    KalturaRelatedExternalFilter kalturaRelatedExternalFilter = ottObject as KalturaRelatedExternalFilter;
                    ret += "\"objectType\": " + "\"" + kalturaRelatedExternalFilter.objectType + "\"";
                    if(kalturaRelatedExternalFilter.relatedObjects != null && kalturaRelatedExternalFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaRelatedExternalFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaRelatedExternalFilter.OrderBy;
                    ret += ", \"name\": " + "\"" + kalturaRelatedExternalFilter.Name + "\"";
                    ret += ", \"dynamicOrderBy\": " + Serialize(kalturaRelatedExternalFilter.DynamicOrderBy);
                    ret += ", \"freeText\": " + "\"" + kalturaRelatedExternalFilter.FreeText + "\"";
                    ret += ", \"idEqual\": " + kalturaRelatedExternalFilter.IdEqual;
                    ret += ", \"typeIn\": " + "\"" + kalturaRelatedExternalFilter.TypeIn + "\"";
                    ret += ", \"utcOffsetEqual\": " + kalturaRelatedExternalFilter.UtcOffsetEqual;
                    break;
                    
                case "KalturaRelatedFilter":
                    KalturaRelatedFilter kalturaRelatedFilter = ottObject as KalturaRelatedFilter;
                    ret += "\"objectType\": " + "\"" + kalturaRelatedFilter.objectType + "\"";
                    if(kalturaRelatedFilter.relatedObjects != null && kalturaRelatedFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaRelatedFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaRelatedFilter.OrderBy;
                    ret += ", \"name\": " + "\"" + kalturaRelatedFilter.Name + "\"";
                    ret += ", \"dynamicOrderBy\": " + Serialize(kalturaRelatedFilter.DynamicOrderBy);
                    if(kalturaRelatedFilter.GroupBy != null && kalturaRelatedFilter.GroupBy.Count > 0)
                    {
                        ret += ", \"groupBy\": " + "[" + String.Join(", ", kalturaRelatedFilter.GroupBy.Select(item => Serialize(item))) + "]";
                    }
                    if(kalturaRelatedFilter.IdEqual.HasValue)
                    {
                        ret += ", \"idEqual\": " + kalturaRelatedFilter.IdEqual;
                    }
                    ret += ", \"kSql\": " + "\"" + kalturaRelatedFilter.KSql + "\"";
                    ret += ", \"typeIn\": " + "\"" + kalturaRelatedFilter.TypeIn + "\"";
                    break;
                    
                case "KalturaReminder":
                    KalturaReminder kalturaReminder = ottObject as KalturaReminder;
                    ret += "\"objectType\": " + "\"" + kalturaReminder.objectType + "\"";
                    if(kalturaReminder.relatedObjects != null && kalturaReminder.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaReminder.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaReminder.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaReminder.Id;
                    }
                    ret += ", \"name\": " + "\"" + kalturaReminder.Name + "\"";
                    ret += ", \"type\": " + kalturaReminder.Type;
                    break;
                    
                case "KalturaReminderFilter":
                    KalturaReminderFilter<int> kalturaReminderFilter = ottObject as KalturaReminderFilter<int>;
                    ret += "\"objectType\": " + "\"" + kalturaReminderFilter.objectType + "\"";
                    if(kalturaReminderFilter.relatedObjects != null && kalturaReminderFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaReminderFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaReminderFilter.OrderBy;
                    ret += ", \"kSql\": " + "\"" + kalturaReminderFilter.KSql + "\"";
                    break;
                    
                case "KalturaReminderListResponse":
                    KalturaReminderListResponse kalturaReminderListResponse = ottObject as KalturaReminderListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaReminderListResponse.objectType + "\"";
                    if(kalturaReminderListResponse.relatedObjects != null && kalturaReminderListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaReminderListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaReminderListResponse.TotalCount;
                    if(kalturaReminderListResponse.Reminders != null && kalturaReminderListResponse.Reminders.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaReminderListResponse.Reminders.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaReport":
                    KalturaReport kalturaReport = ottObject as KalturaReport;
                    ret += "\"objectType\": " + "\"" + kalturaReport.objectType + "\"";
                    if(kalturaReport.relatedObjects != null && kalturaReport.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaReport.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    break;
                    
                case "KalturaReportFilter":
                    KalturaReportFilter kalturaReportFilter = ottObject as KalturaReportFilter;
                    ret += "\"objectType\": " + "\"" + kalturaReportFilter.objectType + "\"";
                    if(kalturaReportFilter.relatedObjects != null && kalturaReportFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaReportFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaReportFilter.OrderBy;
                    break;
                    
                case "KalturaReportListResponse":
                    KalturaReportListResponse kalturaReportListResponse = ottObject as KalturaReportListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaReportListResponse.objectType + "\"";
                    if(kalturaReportListResponse.relatedObjects != null && kalturaReportListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaReportListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaReportListResponse.TotalCount;
                    if(kalturaReportListResponse.Objects != null && kalturaReportListResponse.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaReportListResponse.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaRequestConfiguration":
                    KalturaRequestConfiguration kalturaRequestConfiguration = ottObject as KalturaRequestConfiguration;
                    ret += "\"objectType\": " + "\"" + kalturaRequestConfiguration.objectType + "\"";
                    if(kalturaRequestConfiguration.relatedObjects != null && kalturaRequestConfiguration.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaRequestConfiguration.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"currency\": " + "\"" + kalturaRequestConfiguration.Currency + "\"";
                    ret += ", \"ks\": " + "\"" + kalturaRequestConfiguration.KS + "\"";
                    ret += ", \"language\": " + "\"" + kalturaRequestConfiguration.Language + "\"";
                    if(kalturaRequestConfiguration.PartnerID.HasValue)
                    {
                        ret += ", \"partnerId\": " + kalturaRequestConfiguration.PartnerID;
                    }
                    ret += ", \"responseProfile\": " + Serialize(kalturaRequestConfiguration.ResponseProfile);
                    if(kalturaRequestConfiguration.UserID.HasValue)
                    {
                        ret += ", \"userId\": " + kalturaRequestConfiguration.UserID;
                    }
                    break;
                    
                case "KalturaRuleAction":
                    KalturaRuleAction kalturaRuleAction = ottObject as KalturaRuleAction;
                    ret += "\"objectType\": " + "\"" + kalturaRuleAction.objectType + "\"";
                    if(kalturaRuleAction.relatedObjects != null && kalturaRuleAction.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaRuleAction.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"type\": " + kalturaRuleAction.Type;
                    break;
                    
                case "KalturaRuleFilter":
                    KalturaRuleFilter kalturaRuleFilter = ottObject as KalturaRuleFilter;
                    ret += "\"objectType\": " + "\"" + kalturaRuleFilter.objectType + "\"";
                    if(kalturaRuleFilter.relatedObjects != null && kalturaRuleFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaRuleFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"by\": " + kalturaRuleFilter.By;
                    break;
                    
                case "KalturaScheduledRecordingProgramFilter":
                    KalturaScheduledRecordingProgramFilter kalturaScheduledRecordingProgramFilter = ottObject as KalturaScheduledRecordingProgramFilter;
                    ret += "\"objectType\": " + "\"" + kalturaScheduledRecordingProgramFilter.objectType + "\"";
                    if(kalturaScheduledRecordingProgramFilter.relatedObjects != null && kalturaScheduledRecordingProgramFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaScheduledRecordingProgramFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaScheduledRecordingProgramFilter.OrderBy;
                    ret += ", \"name\": " + "\"" + kalturaScheduledRecordingProgramFilter.Name + "\"";
                    ret += ", \"dynamicOrderBy\": " + Serialize(kalturaScheduledRecordingProgramFilter.DynamicOrderBy);
                    ret += ", \"channelsIn\": " + "\"" + kalturaScheduledRecordingProgramFilter.ChannelsIn + "\"";
                    if(kalturaScheduledRecordingProgramFilter.EndDateLessThanOrNull.HasValue)
                    {
                        ret += ", \"endDateLessThanOrNull\": " + kalturaScheduledRecordingProgramFilter.EndDateLessThanOrNull;
                    }
                    ret += ", \"recordingTypeEqual\": " + kalturaScheduledRecordingProgramFilter.RecordingTypeEqual;
                    if(kalturaScheduledRecordingProgramFilter.StartDateGreaterThanOrNull.HasValue)
                    {
                        ret += ", \"startDateGreaterThanOrNull\": " + kalturaScheduledRecordingProgramFilter.StartDateGreaterThanOrNull;
                    }
                    break;
                    
                case "KalturaSearchAssetFilter":
                    KalturaSearchAssetFilter kalturaSearchAssetFilter = ottObject as KalturaSearchAssetFilter;
                    ret += "\"objectType\": " + "\"" + kalturaSearchAssetFilter.objectType + "\"";
                    if(kalturaSearchAssetFilter.relatedObjects != null && kalturaSearchAssetFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSearchAssetFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaSearchAssetFilter.OrderBy;
                    ret += ", \"name\": " + "\"" + kalturaSearchAssetFilter.Name + "\"";
                    ret += ", \"dynamicOrderBy\": " + Serialize(kalturaSearchAssetFilter.DynamicOrderBy);
                    if(kalturaSearchAssetFilter.GroupBy != null && kalturaSearchAssetFilter.GroupBy.Count > 0)
                    {
                        ret += ", \"groupBy\": " + "[" + String.Join(", ", kalturaSearchAssetFilter.GroupBy.Select(item => Serialize(item))) + "]";
                    }
                    ret += ", \"idIn\": " + "\"" + kalturaSearchAssetFilter.IdIn + "\"";
                    ret += ", \"kSql\": " + "\"" + kalturaSearchAssetFilter.KSql + "\"";
                    ret += ", \"typeIn\": " + "\"" + kalturaSearchAssetFilter.TypeIn + "\"";
                    break;
                    
                case "KalturaSearchExternalFilter":
                    KalturaSearchExternalFilter kalturaSearchExternalFilter = ottObject as KalturaSearchExternalFilter;
                    ret += "\"objectType\": " + "\"" + kalturaSearchExternalFilter.objectType + "\"";
                    if(kalturaSearchExternalFilter.relatedObjects != null && kalturaSearchExternalFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSearchExternalFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaSearchExternalFilter.OrderBy;
                    ret += ", \"name\": " + "\"" + kalturaSearchExternalFilter.Name + "\"";
                    ret += ", \"dynamicOrderBy\": " + Serialize(kalturaSearchExternalFilter.DynamicOrderBy);
                    ret += ", \"query\": " + "\"" + kalturaSearchExternalFilter.Query + "\"";
                    ret += ", \"typeIn\": " + "\"" + kalturaSearchExternalFilter.TypeIn + "\"";
                    ret += ", \"utcOffsetEqual\": " + kalturaSearchExternalFilter.UtcOffsetEqual;
                    break;
                    
                case "KalturaSearchHistory":
                    KalturaSearchHistory kalturaSearchHistory = ottObject as KalturaSearchHistory;
                    ret += "\"objectType\": " + "\"" + kalturaSearchHistory.objectType + "\"";
                    if(kalturaSearchHistory.relatedObjects != null && kalturaSearchHistory.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSearchHistory.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"action\": " + "\"" + kalturaSearchHistory.Action + "\"";
                    ret += ", \"createdAt\": " + kalturaSearchHistory.CreatedAt;
                    ret += ", \"deviceId\": " + "\"" + kalturaSearchHistory.DeviceId + "\"";
                    ret += ", \"filter\": " + "\"" + kalturaSearchHistory.Filter + "\"";
                    ret += ", \"id\": " + "\"" + kalturaSearchHistory.Id + "\"";
                    ret += ", \"language\": " + "\"" + kalturaSearchHistory.Language + "\"";
                    ret += ", \"name\": " + "\"" + kalturaSearchHistory.Name + "\"";
                    ret += ", \"service\": " + "\"" + kalturaSearchHistory.Service + "\"";
                    break;
                    
                case "KalturaSearchHistoryFilter":
                    KalturaSearchHistoryFilter kalturaSearchHistoryFilter = ottObject as KalturaSearchHistoryFilter;
                    ret += "\"objectType\": " + "\"" + kalturaSearchHistoryFilter.objectType + "\"";
                    if(kalturaSearchHistoryFilter.relatedObjects != null && kalturaSearchHistoryFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSearchHistoryFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaSearchHistoryFilter.OrderBy;
                    break;
                    
                case "KalturaSearchHistoryListResponse":
                    KalturaSearchHistoryListResponse kalturaSearchHistoryListResponse = ottObject as KalturaSearchHistoryListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaSearchHistoryListResponse.objectType + "\"";
                    if(kalturaSearchHistoryListResponse.relatedObjects != null && kalturaSearchHistoryListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSearchHistoryListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaSearchHistoryListResponse.TotalCount;
                    if(kalturaSearchHistoryListResponse.Objects != null && kalturaSearchHistoryListResponse.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaSearchHistoryListResponse.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaSeasonsReminderFilter":
                    KalturaSeasonsReminderFilter kalturaSeasonsReminderFilter = ottObject as KalturaSeasonsReminderFilter;
                    ret += "\"objectType\": " + "\"" + kalturaSeasonsReminderFilter.objectType + "\"";
                    if(kalturaSeasonsReminderFilter.relatedObjects != null && kalturaSeasonsReminderFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSeasonsReminderFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaSeasonsReminderFilter.OrderBy;
                    ret += ", \"kSql\": " + "\"" + kalturaSeasonsReminderFilter.KSql + "\"";
                    if(kalturaSeasonsReminderFilter.EpgChannelIdEqual.HasValue)
                    {
                        ret += ", \"epgChannelIdEqual\": " + kalturaSeasonsReminderFilter.EpgChannelIdEqual;
                    }
                    ret += ", \"seasonNumberIn\": " + "\"" + kalturaSeasonsReminderFilter.SeasonNumberIn + "\"";
                    ret += ", \"seriesIdEqual\": " + "\"" + kalturaSeasonsReminderFilter.SeriesIdEqual + "\"";
                    break;
                    
                case "KalturaSeriesRecording":
                    KalturaSeriesRecording kalturaSeriesRecording = ottObject as KalturaSeriesRecording;
                    ret += "\"objectType\": " + "\"" + kalturaSeriesRecording.objectType + "\"";
                    if(kalturaSeriesRecording.relatedObjects != null && kalturaSeriesRecording.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSeriesRecording.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"channelId\": " + kalturaSeriesRecording.ChannelId;
                    ret += ", \"createDate\": " + kalturaSeriesRecording.CreateDate;
                    ret += ", \"epgId\": " + kalturaSeriesRecording.EpgId;
                    if(kalturaSeriesRecording.ExcludedSeasons != null && kalturaSeriesRecording.ExcludedSeasons.Count > 0)
                    {
                        ret += ", \"excludedSeasons\": " + "[" + String.Join(", ", kalturaSeriesRecording.ExcludedSeasons.Select(item => Serialize(item))) + "]";
                    }
                    ret += ", \"id\": " + kalturaSeriesRecording.Id;
                    if(kalturaSeriesRecording.SeasonNumber.HasValue)
                    {
                        ret += ", \"seasonNumber\": " + kalturaSeriesRecording.SeasonNumber;
                    }
                    ret += ", \"seriesId\": " + "\"" + kalturaSeriesRecording.SeriesId + "\"";
                    ret += ", \"type\": " + kalturaSeriesRecording.Type;
                    ret += ", \"updateDate\": " + kalturaSeriesRecording.UpdateDate;
                    break;
                    
                case "KalturaSeriesRecordingFilter":
                    KalturaSeriesRecordingFilter kalturaSeriesRecordingFilter = ottObject as KalturaSeriesRecordingFilter;
                    ret += "\"objectType\": " + "\"" + kalturaSeriesRecordingFilter.objectType + "\"";
                    if(kalturaSeriesRecordingFilter.relatedObjects != null && kalturaSeriesRecordingFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSeriesRecordingFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaSeriesRecordingFilter.OrderBy;
                    break;
                    
                case "KalturaSeriesRecordingListResponse":
                    KalturaSeriesRecordingListResponse kalturaSeriesRecordingListResponse = ottObject as KalturaSeriesRecordingListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaSeriesRecordingListResponse.objectType + "\"";
                    if(kalturaSeriesRecordingListResponse.relatedObjects != null && kalturaSeriesRecordingListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSeriesRecordingListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaSeriesRecordingListResponse.TotalCount;
                    if(kalturaSeriesRecordingListResponse.Objects != null && kalturaSeriesRecordingListResponse.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaSeriesRecordingListResponse.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaSeriesReminder":
                    KalturaSeriesReminder kalturaSeriesReminder = ottObject as KalturaSeriesReminder;
                    ret += "\"objectType\": " + "\"" + kalturaSeriesReminder.objectType + "\"";
                    if(kalturaSeriesReminder.relatedObjects != null && kalturaSeriesReminder.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSeriesReminder.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaSeriesReminder.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaSeriesReminder.Id;
                    }
                    ret += ", \"name\": " + "\"" + kalturaSeriesReminder.Name + "\"";
                    ret += ", \"type\": " + kalturaSeriesReminder.Type;
                    ret += ", \"epgChannelId\": " + kalturaSeriesReminder.EpgChannelId;
                    if(kalturaSeriesReminder.SeasonNumber.HasValue)
                    {
                        ret += ", \"seasonNumber\": " + kalturaSeriesReminder.SeasonNumber;
                    }
                    ret += ", \"seriesId\": " + "\"" + kalturaSeriesReminder.SeriesId + "\"";
                    break;
                    
                case "KalturaSeriesReminderFilter":
                    KalturaSeriesReminderFilter kalturaSeriesReminderFilter = ottObject as KalturaSeriesReminderFilter;
                    ret += "\"objectType\": " + "\"" + kalturaSeriesReminderFilter.objectType + "\"";
                    if(kalturaSeriesReminderFilter.relatedObjects != null && kalturaSeriesReminderFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSeriesReminderFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaSeriesReminderFilter.OrderBy;
                    ret += ", \"kSql\": " + "\"" + kalturaSeriesReminderFilter.KSql + "\"";
                    if(kalturaSeriesReminderFilter.EpgChannelIdEqual.HasValue)
                    {
                        ret += ", \"epgChannelIdEqual\": " + kalturaSeriesReminderFilter.EpgChannelIdEqual;
                    }
                    ret += ", \"seriesIdIn\": " + "\"" + kalturaSeriesReminderFilter.SeriesIdIn + "\"";
                    break;
                    
                case "KalturaSession":
                    KalturaSession kalturaSession = ottObject as KalturaSession;
                    ret += "\"objectType\": " + "\"" + kalturaSession.objectType + "\"";
                    if(kalturaSession.relatedObjects != null && kalturaSession.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSession.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"createDate\": " + kalturaSession.createDate;
                    if(kalturaSession.expiry.HasValue)
                    {
                        ret += ", \"expiry\": " + kalturaSession.expiry;
                    }
                    ret += ", \"ks\": " + "\"" + kalturaSession.ks + "\"";
                    if(kalturaSession.partnerId.HasValue)
                    {
                        ret += ", \"partnerId\": " + kalturaSession.partnerId;
                    }
                    ret += ", \"privileges\": " + "\"" + kalturaSession.privileges + "\"";
                    ret += ", \"sessionType\": " + kalturaSession.sessionType;
                    ret += ", \"udid\": " + "\"" + kalturaSession.udid + "\"";
                    ret += ", \"userId\": " + "\"" + kalturaSession.userId + "\"";
                    break;
                    
                case "KalturaSessionInfo":
                    KalturaSessionInfo kalturaSessionInfo = ottObject as KalturaSessionInfo;
                    ret += "\"objectType\": " + "\"" + kalturaSessionInfo.objectType + "\"";
                    if(kalturaSessionInfo.relatedObjects != null && kalturaSessionInfo.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSessionInfo.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"createDate\": " + kalturaSessionInfo.createDate;
                    if(kalturaSessionInfo.expiry.HasValue)
                    {
                        ret += ", \"expiry\": " + kalturaSessionInfo.expiry;
                    }
                    ret += ", \"ks\": " + "\"" + kalturaSessionInfo.ks + "\"";
                    if(kalturaSessionInfo.partnerId.HasValue)
                    {
                        ret += ", \"partnerId\": " + kalturaSessionInfo.partnerId;
                    }
                    ret += ", \"privileges\": " + "\"" + kalturaSessionInfo.privileges + "\"";
                    ret += ", \"sessionType\": " + kalturaSessionInfo.sessionType;
                    ret += ", \"udid\": " + "\"" + kalturaSessionInfo.udid + "\"";
                    ret += ", \"userId\": " + "\"" + kalturaSessionInfo.userId + "\"";
                    break;
                    
                case "KalturaSlimAsset":
                    KalturaSlimAsset kalturaSlimAsset = ottObject as KalturaSlimAsset;
                    ret += "\"objectType\": " + "\"" + kalturaSlimAsset.objectType + "\"";
                    if(kalturaSlimAsset.relatedObjects != null && kalturaSlimAsset.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSlimAsset.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"id\": " + "\"" + kalturaSlimAsset.Id + "\"";
                    ret += ", \"type\": " + kalturaSlimAsset.Type;
                    break;
                    
                case "KalturaSlimAssetInfoWrapper":
                    KalturaSlimAssetInfoWrapper kalturaSlimAssetInfoWrapper = ottObject as KalturaSlimAssetInfoWrapper;
                    ret += "\"objectType\": " + "\"" + kalturaSlimAssetInfoWrapper.objectType + "\"";
                    if(kalturaSlimAssetInfoWrapper.relatedObjects != null && kalturaSlimAssetInfoWrapper.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSlimAssetInfoWrapper.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaSlimAssetInfoWrapper.TotalCount;
                    if(kalturaSlimAssetInfoWrapper.Objects != null && kalturaSlimAssetInfoWrapper.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaSlimAssetInfoWrapper.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaSocial":
                    KalturaSocial kalturaSocial = ottObject as KalturaSocial;
                    ret += "\"objectType\": " + "\"" + kalturaSocial.objectType + "\"";
                    if(kalturaSocial.relatedObjects != null && kalturaSocial.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSocial.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"birthday\": " + "\"" + kalturaSocial.Birthday + "\"";
                    ret += ", \"email\": " + "\"" + kalturaSocial.Email + "\"";
                    ret += ", \"firstName\": " + "\"" + kalturaSocial.FirstName + "\"";
                    ret += ", \"gender\": " + "\"" + kalturaSocial.Gender + "\"";
                    ret += ", \"id\": " + "\"" + kalturaSocial.ID + "\"";
                    ret += ", \"lastName\": " + "\"" + kalturaSocial.LastName + "\"";
                    ret += ", \"name\": " + "\"" + kalturaSocial.Name + "\"";
                    ret += ", \"pictureUrl\": " + "\"" + kalturaSocial.PictureUrl + "\"";
                    ret += ", \"status\": " + "\"" + kalturaSocial.Status + "\"";
                    ret += ", \"userId\": " + "\"" + kalturaSocial.UserId + "\"";
                    break;
                    
                case "KalturaSocialAction":
                    KalturaSocialAction kalturaSocialAction = ottObject as KalturaSocialAction;
                    ret += "\"objectType\": " + "\"" + kalturaSocialAction.objectType + "\"";
                    if(kalturaSocialAction.relatedObjects != null && kalturaSocialAction.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSocialAction.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"actionType\": " + kalturaSocialAction.ActionType;
                    if(kalturaSocialAction.AssetId.HasValue)
                    {
                        ret += ", \"assetId\": " + kalturaSocialAction.AssetId;
                    }
                    ret += ", \"assetType\": " + kalturaSocialAction.AssetType;
                    ret += ", \"id\": " + "\"" + kalturaSocialAction.Id + "\"";
                    if(kalturaSocialAction.Time.HasValue)
                    {
                        ret += ", \"time\": " + kalturaSocialAction.Time;
                    }
                    ret += ", \"url\": " + "\"" + kalturaSocialAction.Url + "\"";
                    break;
                    
                case "KalturaSocialActionFilter":
                    KalturaSocialActionFilter kalturaSocialActionFilter = ottObject as KalturaSocialActionFilter;
                    ret += "\"objectType\": " + "\"" + kalturaSocialActionFilter.objectType + "\"";
                    if(kalturaSocialActionFilter.relatedObjects != null && kalturaSocialActionFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSocialActionFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaSocialActionFilter.OrderBy;
                    ret += ", \"actionTypeIn\": " + "\"" + kalturaSocialActionFilter.ActionTypeIn + "\"";
                    ret += ", \"assetIdIn\": " + "\"" + kalturaSocialActionFilter.AssetIdIn + "\"";
                    ret += ", \"assetTypeEqual\": " + kalturaSocialActionFilter.AssetTypeEqual;
                    break;
                    
                case "KalturaSocialActionListResponse":
                    KalturaSocialActionListResponse kalturaSocialActionListResponse = ottObject as KalturaSocialActionListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaSocialActionListResponse.objectType + "\"";
                    if(kalturaSocialActionListResponse.relatedObjects != null && kalturaSocialActionListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSocialActionListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaSocialActionListResponse.TotalCount;
                    if(kalturaSocialActionListResponse.Objects != null && kalturaSocialActionListResponse.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaSocialActionListResponse.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaSocialActionRate":
                    KalturaSocialActionRate kalturaSocialActionRate = ottObject as KalturaSocialActionRate;
                    ret += "\"objectType\": " + "\"" + kalturaSocialActionRate.objectType + "\"";
                    if(kalturaSocialActionRate.relatedObjects != null && kalturaSocialActionRate.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSocialActionRate.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"actionType\": " + kalturaSocialActionRate.ActionType;
                    if(kalturaSocialActionRate.AssetId.HasValue)
                    {
                        ret += ", \"assetId\": " + kalturaSocialActionRate.AssetId;
                    }
                    ret += ", \"assetType\": " + kalturaSocialActionRate.AssetType;
                    ret += ", \"id\": " + "\"" + kalturaSocialActionRate.Id + "\"";
                    if(kalturaSocialActionRate.Time.HasValue)
                    {
                        ret += ", \"time\": " + kalturaSocialActionRate.Time;
                    }
                    ret += ", \"url\": " + "\"" + kalturaSocialActionRate.Url + "\"";
                    ret += ", \"rate\": " + kalturaSocialActionRate.Rate;
                    break;
                    
                case "KalturaSocialComment":
                    KalturaSocialComment kalturaSocialComment = ottObject as KalturaSocialComment;
                    ret += "\"objectType\": " + "\"" + kalturaSocialComment.objectType + "\"";
                    if(kalturaSocialComment.relatedObjects != null && kalturaSocialComment.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSocialComment.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"createDate\": " + kalturaSocialComment.CreateDate;
                    ret += ", \"header\": " + "\"" + kalturaSocialComment.Header + "\"";
                    ret += ", \"text\": " + "\"" + kalturaSocialComment.Text + "\"";
                    ret += ", \"writer\": " + "\"" + kalturaSocialComment.Writer + "\"";
                    break;
                    
                case "KalturaSocialCommentFilter":
                    KalturaSocialCommentFilter kalturaSocialCommentFilter = ottObject as KalturaSocialCommentFilter;
                    ret += "\"objectType\": " + "\"" + kalturaSocialCommentFilter.objectType + "\"";
                    if(kalturaSocialCommentFilter.relatedObjects != null && kalturaSocialCommentFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSocialCommentFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaSocialCommentFilter.OrderBy;
                    ret += ", \"assetIdEqual\": " + kalturaSocialCommentFilter.AssetIdEqual;
                    ret += ", \"assetTypeEqual\": " + kalturaSocialCommentFilter.AssetTypeEqual;
                    ret += ", \"createDateGreaterThan\": " + kalturaSocialCommentFilter.CreateDateGreaterThan;
                    ret += ", \"socialPlatformEqual\": " + kalturaSocialCommentFilter.SocialPlatformEqual;
                    break;
                    
                case "KalturaSocialCommentListResponse":
                    KalturaSocialCommentListResponse kalturaSocialCommentListResponse = ottObject as KalturaSocialCommentListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaSocialCommentListResponse.objectType + "\"";
                    if(kalturaSocialCommentListResponse.relatedObjects != null && kalturaSocialCommentListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSocialCommentListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaSocialCommentListResponse.TotalCount;
                    if(kalturaSocialCommentListResponse.Objects != null && kalturaSocialCommentListResponse.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaSocialCommentListResponse.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaSocialConfig":
                    KalturaSocialConfig kalturaSocialConfig = ottObject as KalturaSocialConfig;
                    ret += "\"objectType\": " + "\"" + kalturaSocialConfig.objectType + "\"";
                    if(kalturaSocialConfig.relatedObjects != null && kalturaSocialConfig.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSocialConfig.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    break;
                    
                case "KalturaSocialFacebookConfig":
                    KalturaSocialFacebookConfig kalturaSocialFacebookConfig = ottObject as KalturaSocialFacebookConfig;
                    ret += "\"objectType\": " + "\"" + kalturaSocialFacebookConfig.objectType + "\"";
                    if(kalturaSocialFacebookConfig.relatedObjects != null && kalturaSocialFacebookConfig.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSocialFacebookConfig.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"appId\": " + "\"" + kalturaSocialFacebookConfig.AppId + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"app_id\": " + "\"" + kalturaSocialFacebookConfig.AppId + "\"";
                    }
                    ret += ", \"permissions\": " + "\"" + kalturaSocialFacebookConfig.Permissions + "\"";
                    break;
                    
                case "KalturaSocialFriendActivity":
                    KalturaSocialFriendActivity kalturaSocialFriendActivity = ottObject as KalturaSocialFriendActivity;
                    ret += "\"objectType\": " + "\"" + kalturaSocialFriendActivity.objectType + "\"";
                    if(kalturaSocialFriendActivity.relatedObjects != null && kalturaSocialFriendActivity.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSocialFriendActivity.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"socialAction\": " + Serialize(kalturaSocialFriendActivity.SocialAction);
                    ret += ", \"userFullName\": " + "\"" + kalturaSocialFriendActivity.UserFullName + "\"";
                    ret += ", \"userPictureUrl\": " + "\"" + kalturaSocialFriendActivity.UserPictureUrl + "\"";
                    break;
                    
                case "KalturaSocialFriendActivityFilter":
                    KalturaSocialFriendActivityFilter kalturaSocialFriendActivityFilter = ottObject as KalturaSocialFriendActivityFilter;
                    ret += "\"objectType\": " + "\"" + kalturaSocialFriendActivityFilter.objectType + "\"";
                    if(kalturaSocialFriendActivityFilter.relatedObjects != null && kalturaSocialFriendActivityFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSocialFriendActivityFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaSocialFriendActivityFilter.OrderBy;
                    ret += ", \"actionTypeIn\": " + "\"" + kalturaSocialFriendActivityFilter.ActionTypeIn + "\"";
                    if(kalturaSocialFriendActivityFilter.AssetIdEqual.HasValue)
                    {
                        ret += ", \"assetIdEqual\": " + kalturaSocialFriendActivityFilter.AssetIdEqual;
                    }
                    if(kalturaSocialFriendActivityFilter.AssetTypeEqual.HasValue)
                    {
                        ret += ", \"assetTypeEqual\": " + kalturaSocialFriendActivityFilter.AssetTypeEqual;
                    }
                    break;
                    
                case "KalturaSocialFriendActivityListResponse":
                    KalturaSocialFriendActivityListResponse kalturaSocialFriendActivityListResponse = ottObject as KalturaSocialFriendActivityListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaSocialFriendActivityListResponse.objectType + "\"";
                    if(kalturaSocialFriendActivityListResponse.relatedObjects != null && kalturaSocialFriendActivityListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSocialFriendActivityListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaSocialFriendActivityListResponse.TotalCount;
                    if(kalturaSocialFriendActivityListResponse.Objects != null && kalturaSocialFriendActivityListResponse.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaSocialFriendActivityListResponse.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaSocialNetworkComment":
                    KalturaSocialNetworkComment kalturaSocialNetworkComment = ottObject as KalturaSocialNetworkComment;
                    ret += "\"objectType\": " + "\"" + kalturaSocialNetworkComment.objectType + "\"";
                    if(kalturaSocialNetworkComment.relatedObjects != null && kalturaSocialNetworkComment.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSocialNetworkComment.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"createDate\": " + kalturaSocialNetworkComment.CreateDate;
                    ret += ", \"header\": " + "\"" + kalturaSocialNetworkComment.Header + "\"";
                    ret += ", \"text\": " + "\"" + kalturaSocialNetworkComment.Text + "\"";
                    ret += ", \"writer\": " + "\"" + kalturaSocialNetworkComment.Writer + "\"";
                    ret += ", \"authorImageUrl\": " + "\"" + kalturaSocialNetworkComment.AuthorImageUrl + "\"";
                    ret += ", \"likeCounter\": " + "\"" + kalturaSocialNetworkComment.LikeCounter + "\"";
                    break;
                    
                case "KalturaSocialResponse":
                    KalturaSocialResponse kalturaSocialResponse = ottObject as KalturaSocialResponse;
                    ret += "\"objectType\": " + "\"" + kalturaSocialResponse.objectType + "\"";
                    if(kalturaSocialResponse.relatedObjects != null && kalturaSocialResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSocialResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"data\": " + "\"" + kalturaSocialResponse.Data + "\"";
                    ret += ", \"kalturaUsername\": " + "\"" + kalturaSocialResponse.KalturaName + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"kaltura_username\": " + "\"" + kalturaSocialResponse.KalturaName + "\"";
                    }
                    ret += ", \"minFriendsLimitation\": " + "\"" + kalturaSocialResponse.MinFriends + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"min_friends_limitation\": " + "\"" + kalturaSocialResponse.MinFriends + "\"";
                    }
                    ret += ", \"pic\": " + "\"" + kalturaSocialResponse.Pic + "\"";
                    ret += ", \"socialUsername\": " + "\"" + kalturaSocialResponse.SocialNetworkUsername + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"social_username\": " + "\"" + kalturaSocialResponse.SocialNetworkUsername + "\"";
                    }
                    ret += ", \"socialUser\": " + Serialize(kalturaSocialResponse.SocialUser);
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"social_user\": " + Serialize(kalturaSocialResponse.SocialUser);
                    }
                    ret += ", \"status\": " + "\"" + kalturaSocialResponse.Status + "\"";
                    ret += ", \"token\": " + "\"" + kalturaSocialResponse.Token + "\"";
                    ret += ", \"userId\": " + "\"" + kalturaSocialResponse.UserId + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"user_id\": " + "\"" + kalturaSocialResponse.UserId + "\"";
                    }
                    break;
                    
                case "KalturaSocialUser":
                    KalturaSocialUser kalturaSocialUser = ottObject as KalturaSocialUser;
                    ret += "\"objectType\": " + "\"" + kalturaSocialUser.objectType + "\"";
                    if(kalturaSocialUser.relatedObjects != null && kalturaSocialUser.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSocialUser.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"birthday\": " + "\"" + kalturaSocialUser.Birthday + "\"";
                    ret += ", \"email\": " + "\"" + kalturaSocialUser.Email + "\"";
                    ret += ", \"firstName\": " + "\"" + kalturaSocialUser.FirstName + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"first_name\": " + "\"" + kalturaSocialUser.FirstName + "\"";
                    }
                    ret += ", \"gender\": " + "\"" + kalturaSocialUser.Gender + "\"";
                    ret += ", \"id\": " + "\"" + kalturaSocialUser.ID + "\"";
                    ret += ", \"lastName\": " + "\"" + kalturaSocialUser.LastName + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"last_name\": " + "\"" + kalturaSocialUser.LastName + "\"";
                    }
                    ret += ", \"name\": " + "\"" + kalturaSocialUser.Name + "\"";
                    ret += ", \"userId\": " + "\"" + kalturaSocialUser.UserId + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"user_id\": " + "\"" + kalturaSocialUser.UserId + "\"";
                    }
                    break;
                    
                case "KalturaSocialUserConfig":
                    KalturaSocialUserConfig kalturaSocialUserConfig = ottObject as KalturaSocialUserConfig;
                    ret += "\"objectType\": " + "\"" + kalturaSocialUserConfig.objectType + "\"";
                    if(kalturaSocialUserConfig.relatedObjects != null && kalturaSocialUserConfig.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSocialUserConfig.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaSocialUserConfig.PermissionItems != null && kalturaSocialUserConfig.PermissionItems.Count > 0)
                    {
                        ret += ", \"actionPermissionItems\": " + "[" + String.Join(", ", kalturaSocialUserConfig.PermissionItems.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaStringValue":
                    KalturaStringValue kalturaStringValue = ottObject as KalturaStringValue;
                    ret += "\"objectType\": " + "\"" + kalturaStringValue.objectType + "\"";
                    if(kalturaStringValue.relatedObjects != null && kalturaStringValue.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaStringValue.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"description\": " + "\"" + kalturaStringValue.description + "\"";
                    ret += ", \"value\": " + "\"" + kalturaStringValue.value + "\"";
                    break;
                    
                case "KalturaStringValueArray":
                    KalturaStringValueArray kalturaStringValueArray = ottObject as KalturaStringValueArray;
                    ret += "\"objectType\": " + "\"" + kalturaStringValueArray.objectType + "\"";
                    if(kalturaStringValueArray.relatedObjects != null && kalturaStringValueArray.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaStringValueArray.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaStringValueArray.Objects != null && kalturaStringValueArray.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaStringValueArray.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaSubscription":
                    KalturaSubscription kalturaSubscription = ottObject as KalturaSubscription;
                    ret += "\"objectType\": " + "\"" + kalturaSubscription.objectType + "\"";
                    if(kalturaSubscription.relatedObjects != null && kalturaSubscription.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSubscription.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaSubscription.Channels != null && kalturaSubscription.Channels.Count > 0)
                    {
                        ret += ", \"channels\": " + "[" + String.Join(", ", kalturaSubscription.Channels.Select(item => Serialize(item))) + "]";
                    }
                    if(kalturaSubscription.CouponGroups != null && kalturaSubscription.CouponGroups.Count > 0)
                    {
                        ret += ", \"couponsGroups\": " + "[" + String.Join(", ", kalturaSubscription.CouponGroups.Select(item => Serialize(item))) + "]";
                    }
                    ret += ", \"couponsGroup\": " + Serialize(kalturaSubscription.CouponsGroup);
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"coupons_group\": " + Serialize(kalturaSubscription.CouponsGroup);
                    }
                    ret += ", \"dependencyType\": " + kalturaSubscription.DependencyType;
                    ret += ", \"description\": " + Serialize(kalturaSubscription.Description);
                    if(kalturaSubscription.Descriptions != null && kalturaSubscription.Descriptions.Count > 0)
                    {
                        ret += ", \"descriptions\": " + "[" + String.Join(", ", kalturaSubscription.Descriptions.Select(item => Serialize(item))) + "]";
                    }
                    ret += ", \"discountModule\": " + Serialize(kalturaSubscription.DiscountModule);
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"discount_module\": " + Serialize(kalturaSubscription.DiscountModule);
                    }
                    if(kalturaSubscription.EndDate.HasValue)
                    {
                        ret += ", \"endDate\": " + kalturaSubscription.EndDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"end_date\": " + kalturaSubscription.EndDate;
                        }
                    }
                    ret += ", \"externalId\": " + "\"" + kalturaSubscription.ExternalId + "\"";
                    if(kalturaSubscription.FileTypes != null && kalturaSubscription.FileTypes.Count > 0)
                    {
                        ret += ", \"fileTypes\": " + "[" + String.Join(", ", kalturaSubscription.FileTypes.Select(item => Serialize(item))) + "]";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"file_types\": " + "[" + String.Join(", ", kalturaSubscription.FileTypes.Select(item => Serialize(item))) + "]";
                        }
                    }
                    if(kalturaSubscription.GracePeriodMinutes.HasValue)
                    {
                        ret += ", \"gracePeriodMinutes\": " + kalturaSubscription.GracePeriodMinutes;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"grace_period_minutes\": " + kalturaSubscription.GracePeriodMinutes;
                        }
                    }
                    if(kalturaSubscription.HouseholdLimitationsId.HasValue)
                    {
                        ret += ", \"householdLimitationsId\": " + kalturaSubscription.HouseholdLimitationsId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"household_limitations_id\": " + kalturaSubscription.HouseholdLimitationsId;
                        }
                    }
                    ret += ", \"id\": " + "\"" + kalturaSubscription.Id + "\"";
                    ret += ", \"isCancellationBlocked\": " + kalturaSubscription.IsCancellationBlocked;
                    if(kalturaSubscription.IsInfiniteRenewal.HasValue)
                    {
                        ret += ", \"isInfiniteRenewal\": " + kalturaSubscription.IsInfiniteRenewal;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_infinite_renewal\": " + kalturaSubscription.IsInfiniteRenewal;
                        }
                    }
                    if(kalturaSubscription.IsRenewable.HasValue)
                    {
                        ret += ", \"isRenewable\": " + kalturaSubscription.IsRenewable;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_renewable\": " + kalturaSubscription.IsRenewable;
                        }
                    }
                    if(kalturaSubscription.IsWaiverEnabled.HasValue)
                    {
                        ret += ", \"isWaiverEnabled\": " + kalturaSubscription.IsWaiverEnabled;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_waiver_enabled\": " + kalturaSubscription.IsWaiverEnabled;
                        }
                    }
                    if(kalturaSubscription.MaxViewsNumber.HasValue)
                    {
                        ret += ", \"maxViewsNumber\": " + kalturaSubscription.MaxViewsNumber;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"max_views_number\": " + kalturaSubscription.MaxViewsNumber;
                        }
                    }
                    if(kalturaSubscription.MediaId.HasValue)
                    {
                        ret += ", \"mediaId\": " + kalturaSubscription.MediaId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"media_id\": " + kalturaSubscription.MediaId;
                        }
                    }
                    ret += ", \"name\": " + Serialize(kalturaSubscription.Name);
                    if(kalturaSubscription.Names != null && kalturaSubscription.Names.Count > 0)
                    {
                        ret += ", \"names\": " + "[" + String.Join(", ", kalturaSubscription.Names.Select(item => Serialize(item))) + "]";
                    }
                    if(kalturaSubscription.PremiumServices != null && kalturaSubscription.PremiumServices.Count > 0)
                    {
                        ret += ", \"premiumServices\": " + "[" + String.Join(", ", kalturaSubscription.PremiumServices.Select(item => Serialize(item))) + "]";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"premium_services\": " + "[" + String.Join(", ", kalturaSubscription.PremiumServices.Select(item => Serialize(item))) + "]";
                        }
                    }
                    ret += ", \"previewModule\": " + Serialize(kalturaSubscription.PreviewModule);
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"preview_module\": " + Serialize(kalturaSubscription.PreviewModule);
                    }
                    ret += ", \"price\": " + Serialize(kalturaSubscription.Price);
                    ret += ", \"pricePlanIds\": " + "\"" + kalturaSubscription.PricePlanIds + "\"";
                    if(kalturaSubscription.PricePlans != null && kalturaSubscription.PricePlans.Count > 0)
                    {
                        ret += ", \"pricePlans\": " + "[" + String.Join(", ", kalturaSubscription.PricePlans.Select(item => Serialize(item))) + "]";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"price_plans\": " + "[" + String.Join(", ", kalturaSubscription.PricePlans.Select(item => Serialize(item))) + "]";
                        }
                    }
                    ret += ", \"productCode\": " + "\"" + kalturaSubscription.ProductCode + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"product_code\": " + "\"" + kalturaSubscription.ProductCode + "\"";
                    }
                    if(kalturaSubscription.ProductCodes != null && kalturaSubscription.ProductCodes.Count > 0)
                    {
                        ret += ", \"productCodes\": " + "[" + String.Join(", ", kalturaSubscription.ProductCodes.Select(item => Serialize(item))) + "]";
                    }
                    if(kalturaSubscription.ProrityInOrder.HasValue)
                    {
                        ret += ", \"prorityInOrder\": " + kalturaSubscription.ProrityInOrder;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"prority_in_order\": " + kalturaSubscription.ProrityInOrder;
                        }
                    }
                    if(kalturaSubscription.RenewalsNumber.HasValue)
                    {
                        ret += ", \"renewalsNumber\": " + kalturaSubscription.RenewalsNumber;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"renewals_number\": " + kalturaSubscription.RenewalsNumber;
                        }
                    }
                    if(kalturaSubscription.StartDate.HasValue)
                    {
                        ret += ", \"startDate\": " + kalturaSubscription.StartDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"start_date\": " + kalturaSubscription.StartDate;
                        }
                    }
                    if(kalturaSubscription.UserTypes != null && kalturaSubscription.UserTypes.Count > 0)
                    {
                        ret += ", \"userTypes\": " + "[" + String.Join(", ", kalturaSubscription.UserTypes.Select(item => Serialize(item))) + "]";
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"user_types\": " + "[" + String.Join(", ", kalturaSubscription.UserTypes.Select(item => Serialize(item))) + "]";
                        }
                    }
                    if(kalturaSubscription.ViewLifeCycle.HasValue)
                    {
                        ret += ", \"viewLifeCycle\": " + kalturaSubscription.ViewLifeCycle;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"view_life_cycle\": " + kalturaSubscription.ViewLifeCycle;
                        }
                    }
                    if(kalturaSubscription.WaiverPeriod.HasValue)
                    {
                        ret += ", \"waiverPeriod\": " + kalturaSubscription.WaiverPeriod;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"waiver_period\": " + kalturaSubscription.WaiverPeriod;
                        }
                    }
                    break;
                    
                case "KalturaSubscriptionDependencySet":
                    KalturaSubscriptionDependencySet kalturaSubscriptionDependencySet = ottObject as KalturaSubscriptionDependencySet;
                    ret += "\"objectType\": " + "\"" + kalturaSubscriptionDependencySet.objectType + "\"";
                    if(kalturaSubscriptionDependencySet.relatedObjects != null && kalturaSubscriptionDependencySet.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSubscriptionDependencySet.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"id\": " + kalturaSubscriptionDependencySet.Id;
                    ret += ", \"name\": " + "\"" + kalturaSubscriptionDependencySet.Name + "\"";
                    ret += ", \"subscriptionIds\": " + "\"" + kalturaSubscriptionDependencySet.SubscriptionIds + "\"";
                    if(kalturaSubscriptionDependencySet.Type.HasValue)
                    {
                        ret += ", \"type\": " + kalturaSubscriptionDependencySet.Type;
                    }
                    if(kalturaSubscriptionDependencySet.BaseSubscriptionId.HasValue)
                    {
                        ret += ", \"baseSubscriptionId\": " + kalturaSubscriptionDependencySet.BaseSubscriptionId;
                    }
                    break;
                    
                case "KalturaSubscriptionDependencySetFilter":
                    KalturaSubscriptionDependencySetFilter kalturaSubscriptionDependencySetFilter = ottObject as KalturaSubscriptionDependencySetFilter;
                    ret += "\"objectType\": " + "\"" + kalturaSubscriptionDependencySetFilter.objectType + "\"";
                    if(kalturaSubscriptionDependencySetFilter.relatedObjects != null && kalturaSubscriptionDependencySetFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSubscriptionDependencySetFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaSubscriptionDependencySetFilter.OrderBy;
                    ret += ", \"idIn\": " + "\"" + kalturaSubscriptionDependencySetFilter.IdIn + "\"";
                    ret += ", \"subscriptionIdContains\": " + "\"" + kalturaSubscriptionDependencySetFilter.SubscriptionIdContains + "\"";
                    if(kalturaSubscriptionDependencySetFilter.TypeEqual.HasValue)
                    {
                        ret += ", \"typeEqual\": " + kalturaSubscriptionDependencySetFilter.TypeEqual;
                    }
                    ret += ", \"baseSubscriptionIdIn\": " + "\"" + kalturaSubscriptionDependencySetFilter.BaseSubscriptionIdIn + "\"";
                    break;
                    
                case "KalturaSubscriptionEntitlement":
                    KalturaSubscriptionEntitlement kalturaSubscriptionEntitlement = ottObject as KalturaSubscriptionEntitlement;
                    ret += "\"objectType\": " + "\"" + kalturaSubscriptionEntitlement.objectType + "\"";
                    if(kalturaSubscriptionEntitlement.relatedObjects != null && kalturaSubscriptionEntitlement.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSubscriptionEntitlement.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaSubscriptionEntitlement.CurrentDate.HasValue)
                    {
                        ret += ", \"currentDate\": " + kalturaSubscriptionEntitlement.CurrentDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"current_date\": " + kalturaSubscriptionEntitlement.CurrentDate;
                        }
                    }
                    if(kalturaSubscriptionEntitlement.CurrentUses.HasValue)
                    {
                        ret += ", \"currentUses\": " + kalturaSubscriptionEntitlement.CurrentUses;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"current_uses\": " + kalturaSubscriptionEntitlement.CurrentUses;
                        }
                    }
                    ret += ", \"deviceName\": " + "\"" + kalturaSubscriptionEntitlement.DeviceName + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"device_name\": " + "\"" + kalturaSubscriptionEntitlement.DeviceName + "\"";
                    }
                    ret += ", \"deviceUdid\": " + "\"" + kalturaSubscriptionEntitlement.DeviceUDID + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"device_udid\": " + "\"" + kalturaSubscriptionEntitlement.DeviceUDID + "\"";
                    }
                    if(kalturaSubscriptionEntitlement.EndDate.HasValue)
                    {
                        ret += ", \"endDate\": " + kalturaSubscriptionEntitlement.EndDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"end_date\": " + kalturaSubscriptionEntitlement.EndDate;
                        }
                    }
                    ret += ", \"entitlementId\": " + "\"" + kalturaSubscriptionEntitlement.EntitlementId + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"entitlement_id\": " + "\"" + kalturaSubscriptionEntitlement.EntitlementId + "\"";
                    }
                    ret += ", \"householdId\": " + kalturaSubscriptionEntitlement.HouseholdId;
                    if(kalturaSubscriptionEntitlement.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaSubscriptionEntitlement.Id;
                    }
                    if(kalturaSubscriptionEntitlement.IsCancelationWindowEnabled.HasValue)
                    {
                        ret += ", \"isCancelationWindowEnabled\": " + kalturaSubscriptionEntitlement.IsCancelationWindowEnabled;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_cancelation_window_enabled\": " + kalturaSubscriptionEntitlement.IsCancelationWindowEnabled;
                        }
                    }
                    if(kalturaSubscriptionEntitlement.IsInGracePeriod.HasValue)
                    {
                        ret += ", \"isInGracePeriod\": " + kalturaSubscriptionEntitlement.IsInGracePeriod;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_in_grace_period\": " + kalturaSubscriptionEntitlement.IsInGracePeriod;
                        }
                    }
                    if(kalturaSubscriptionEntitlement.IsRenewable.HasValue)
                    {
                        ret += ", \"isRenewable\": " + kalturaSubscriptionEntitlement.IsRenewable;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_renewable\": " + kalturaSubscriptionEntitlement.IsRenewable;
                        }
                    }
                    if(kalturaSubscriptionEntitlement.IsRenewableForPurchase.HasValue)
                    {
                        ret += ", \"isRenewableForPurchase\": " + kalturaSubscriptionEntitlement.IsRenewableForPurchase;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_renewable_for_purchase\": " + kalturaSubscriptionEntitlement.IsRenewableForPurchase;
                        }
                    }
                    if(kalturaSubscriptionEntitlement.LastViewDate.HasValue)
                    {
                        ret += ", \"lastViewDate\": " + kalturaSubscriptionEntitlement.LastViewDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"last_view_date\": " + kalturaSubscriptionEntitlement.LastViewDate;
                        }
                    }
                    if(kalturaSubscriptionEntitlement.MaxUses.HasValue)
                    {
                        ret += ", \"maxUses\": " + kalturaSubscriptionEntitlement.MaxUses;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"max_uses\": " + kalturaSubscriptionEntitlement.MaxUses;
                        }
                    }
                    if(kalturaSubscriptionEntitlement.MediaFileId.HasValue)
                    {
                        ret += ", \"mediaFileId\": " + kalturaSubscriptionEntitlement.MediaFileId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"media_file_id\": " + kalturaSubscriptionEntitlement.MediaFileId;
                        }
                    }
                    if(kalturaSubscriptionEntitlement.MediaId.HasValue)
                    {
                        ret += ", \"mediaId\": " + kalturaSubscriptionEntitlement.MediaId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"media_id\": " + kalturaSubscriptionEntitlement.MediaId;
                        }
                    }
                    if(kalturaSubscriptionEntitlement.NextRenewalDate.HasValue)
                    {
                        ret += ", \"nextRenewalDate\": " + kalturaSubscriptionEntitlement.NextRenewalDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"next_renewal_date\": " + kalturaSubscriptionEntitlement.NextRenewalDate;
                        }
                    }
                    ret += ", \"paymentMethod\": " + kalturaSubscriptionEntitlement.PaymentMethod;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"payment_method\": " + kalturaSubscriptionEntitlement.PaymentMethod;
                    }
                    ret += ", \"productId\": " + "\"" + kalturaSubscriptionEntitlement.ProductId + "\"";
                    if(kalturaSubscriptionEntitlement.PurchaseDate.HasValue)
                    {
                        ret += ", \"purchaseDate\": " + kalturaSubscriptionEntitlement.PurchaseDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"purchase_date\": " + kalturaSubscriptionEntitlement.PurchaseDate;
                        }
                    }
                    if(kalturaSubscriptionEntitlement.PurchaseId.HasValue)
                    {
                        ret += ", \"purchaseId\": " + kalturaSubscriptionEntitlement.PurchaseId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"purchase_id\": " + kalturaSubscriptionEntitlement.PurchaseId;
                        }
                    }
                    ret += ", \"type\": " + kalturaSubscriptionEntitlement.Type;
                    ret += ", \"userId\": " + "\"" + kalturaSubscriptionEntitlement.UserId + "\"";
                    if(kalturaSubscriptionEntitlement.IsInGracePeriod.HasValue)
                    {
                        ret += ", \"isInGracePeriod\": " + kalturaSubscriptionEntitlement.IsInGracePeriod;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_in_grace_period\": " + kalturaSubscriptionEntitlement.IsInGracePeriod;
                        }
                    }
                    if(kalturaSubscriptionEntitlement.IsRenewable.HasValue)
                    {
                        ret += ", \"isRenewable\": " + kalturaSubscriptionEntitlement.IsRenewable;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_renewable\": " + kalturaSubscriptionEntitlement.IsRenewable;
                        }
                    }
                    if(kalturaSubscriptionEntitlement.IsRenewableForPurchase.HasValue)
                    {
                        ret += ", \"isRenewableForPurchase\": " + kalturaSubscriptionEntitlement.IsRenewableForPurchase;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_renewable_for_purchase\": " + kalturaSubscriptionEntitlement.IsRenewableForPurchase;
                        }
                    }
                    ret += ", \"isSuspended\": " + kalturaSubscriptionEntitlement.IsSuspended;
                    if(kalturaSubscriptionEntitlement.NextRenewalDate.HasValue)
                    {
                        ret += ", \"nextRenewalDate\": " + kalturaSubscriptionEntitlement.NextRenewalDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"next_renewal_date\": " + kalturaSubscriptionEntitlement.NextRenewalDate;
                        }
                    }
                    if(kalturaSubscriptionEntitlement.PaymentGatewayId.HasValue)
                    {
                        ret += ", \"paymentGatewayId\": " + kalturaSubscriptionEntitlement.PaymentGatewayId;
                    }
                    if(kalturaSubscriptionEntitlement.PaymentMethodId.HasValue)
                    {
                        ret += ", \"paymentMethodId\": " + kalturaSubscriptionEntitlement.PaymentMethodId;
                    }
                    if(kalturaSubscriptionEntitlement.ScheduledSubscriptionId.HasValue)
                    {
                        ret += ", \"scheduledSubscriptionId\": " + kalturaSubscriptionEntitlement.ScheduledSubscriptionId;
                    }
                    if(kalturaSubscriptionEntitlement.UnifiedPaymentId.HasValue)
                    {
                        ret += ", \"unifiedPaymentId\": " + kalturaSubscriptionEntitlement.UnifiedPaymentId;
                    }
                    break;
                    
                case "KalturaSubscriptionFilter":
                    KalturaSubscriptionFilter kalturaSubscriptionFilter = ottObject as KalturaSubscriptionFilter;
                    ret += "\"objectType\": " + "\"" + kalturaSubscriptionFilter.objectType + "\"";
                    if(kalturaSubscriptionFilter.relatedObjects != null && kalturaSubscriptionFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSubscriptionFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaSubscriptionFilter.OrderBy;
                    ret += ", \"externalIdIn\": " + "\"" + kalturaSubscriptionFilter.ExternalIdIn + "\"";
                    if(kalturaSubscriptionFilter.MediaFileIdEqual.HasValue)
                    {
                        ret += ", \"mediaFileIdEqual\": " + kalturaSubscriptionFilter.MediaFileIdEqual;
                    }
                    ret += ", \"subscriptionIdIn\": " + "\"" + kalturaSubscriptionFilter.SubscriptionIdIn + "\"";
                    break;
                    
                case "KalturaSubscriptionListResponse":
                    KalturaSubscriptionListResponse kalturaSubscriptionListResponse = ottObject as KalturaSubscriptionListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaSubscriptionListResponse.objectType + "\"";
                    if(kalturaSubscriptionListResponse.relatedObjects != null && kalturaSubscriptionListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSubscriptionListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaSubscriptionListResponse.TotalCount;
                    if(kalturaSubscriptionListResponse.Subscriptions != null && kalturaSubscriptionListResponse.Subscriptions.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaSubscriptionListResponse.Subscriptions.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaSubscriptionPrice":
                    KalturaSubscriptionPrice kalturaSubscriptionPrice = ottObject as KalturaSubscriptionPrice;
                    ret += "\"objectType\": " + "\"" + kalturaSubscriptionPrice.objectType + "\"";
                    if(kalturaSubscriptionPrice.relatedObjects != null && kalturaSubscriptionPrice.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSubscriptionPrice.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"price\": " + Serialize(kalturaSubscriptionPrice.Price);
                    ret += ", \"productId\": " + "\"" + kalturaSubscriptionPrice.ProductId + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"product_id\": " + "\"" + kalturaSubscriptionPrice.ProductId + "\"";
                    }
                    ret += ", \"productType\": " + kalturaSubscriptionPrice.ProductType;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"product_type\": " + kalturaSubscriptionPrice.ProductType;
                    }
                    ret += ", \"purchaseStatus\": " + kalturaSubscriptionPrice.PurchaseStatus;
                    if(kalturaSubscriptionPrice.EndDate.HasValue)
                    {
                        ret += ", \"endDate\": " + kalturaSubscriptionPrice.EndDate;
                    }
                    ret += ", \"price\": " + Serialize(kalturaSubscriptionPrice.Price);
                    ret += ", \"purchaseStatus\": " + kalturaSubscriptionPrice.PurchaseStatus;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"purchase_status\": " + kalturaSubscriptionPrice.PurchaseStatus;
                    }
                    break;
                    
                case "KalturaSubscriptionSet":
                    KalturaSubscriptionSet kalturaSubscriptionSet = ottObject as KalturaSubscriptionSet;
                    ret += "\"objectType\": " + "\"" + kalturaSubscriptionSet.objectType + "\"";
                    if(kalturaSubscriptionSet.relatedObjects != null && kalturaSubscriptionSet.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSubscriptionSet.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"id\": " + kalturaSubscriptionSet.Id;
                    ret += ", \"name\": " + "\"" + kalturaSubscriptionSet.Name + "\"";
                    ret += ", \"subscriptionIds\": " + "\"" + kalturaSubscriptionSet.SubscriptionIds + "\"";
                    if(kalturaSubscriptionSet.Type.HasValue)
                    {
                        ret += ", \"type\": " + kalturaSubscriptionSet.Type;
                    }
                    break;
                    
                case "KalturaSubscriptionSetFilter":
                    KalturaSubscriptionSetFilter kalturaSubscriptionSetFilter = ottObject as KalturaSubscriptionSetFilter;
                    ret += "\"objectType\": " + "\"" + kalturaSubscriptionSetFilter.objectType + "\"";
                    if(kalturaSubscriptionSetFilter.relatedObjects != null && kalturaSubscriptionSetFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSubscriptionSetFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaSubscriptionSetFilter.OrderBy;
                    ret += ", \"idIn\": " + "\"" + kalturaSubscriptionSetFilter.IdIn + "\"";
                    ret += ", \"subscriptionIdContains\": " + "\"" + kalturaSubscriptionSetFilter.SubscriptionIdContains + "\"";
                    if(kalturaSubscriptionSetFilter.TypeEqual.HasValue)
                    {
                        ret += ", \"typeEqual\": " + kalturaSubscriptionSetFilter.TypeEqual;
                    }
                    break;
                    
                case "KalturaSubscriptionSetListResponse":
                    KalturaSubscriptionSetListResponse kalturaSubscriptionSetListResponse = ottObject as KalturaSubscriptionSetListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaSubscriptionSetListResponse.objectType + "\"";
                    if(kalturaSubscriptionSetListResponse.relatedObjects != null && kalturaSubscriptionSetListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSubscriptionSetListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaSubscriptionSetListResponse.TotalCount;
                    if(kalturaSubscriptionSetListResponse.SubscriptionSets != null && kalturaSubscriptionSetListResponse.SubscriptionSets.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaSubscriptionSetListResponse.SubscriptionSets.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaSubscriptionsFilter":
                    KalturaSubscriptionsFilter kalturaSubscriptionsFilter = ottObject as KalturaSubscriptionsFilter;
                    ret += "\"objectType\": " + "\"" + kalturaSubscriptionsFilter.objectType + "\"";
                    if(kalturaSubscriptionsFilter.relatedObjects != null && kalturaSubscriptionsFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSubscriptionsFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"by\": " + kalturaSubscriptionsFilter.By;
                    if(kalturaSubscriptionsFilter.Ids != null && kalturaSubscriptionsFilter.Ids.Count > 0)
                    {
                        ret += ", \"ids\": " + "[" + String.Join(", ", kalturaSubscriptionsFilter.Ids.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaSubscriptionSwitchSet":
                    KalturaSubscriptionSwitchSet kalturaSubscriptionSwitchSet = ottObject as KalturaSubscriptionSwitchSet;
                    ret += "\"objectType\": " + "\"" + kalturaSubscriptionSwitchSet.objectType + "\"";
                    if(kalturaSubscriptionSwitchSet.relatedObjects != null && kalturaSubscriptionSwitchSet.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaSubscriptionSwitchSet.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"id\": " + kalturaSubscriptionSwitchSet.Id;
                    ret += ", \"name\": " + "\"" + kalturaSubscriptionSwitchSet.Name + "\"";
                    ret += ", \"subscriptionIds\": " + "\"" + kalturaSubscriptionSwitchSet.SubscriptionIds + "\"";
                    if(kalturaSubscriptionSwitchSet.Type.HasValue)
                    {
                        ret += ", \"type\": " + kalturaSubscriptionSwitchSet.Type;
                    }
                    break;
                    
                case "KalturaTimeShiftedTvPartnerSettings":
                    KalturaTimeShiftedTvPartnerSettings kalturaTimeShiftedTvPartnerSettings = ottObject as KalturaTimeShiftedTvPartnerSettings;
                    ret += "\"objectType\": " + "\"" + kalturaTimeShiftedTvPartnerSettings.objectType + "\"";
                    if(kalturaTimeShiftedTvPartnerSettings.relatedObjects != null && kalturaTimeShiftedTvPartnerSettings.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaTimeShiftedTvPartnerSettings.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaTimeShiftedTvPartnerSettings.CatchUpBufferLength.HasValue)
                    {
                        ret += ", \"catchUpBufferLength\": " + kalturaTimeShiftedTvPartnerSettings.CatchUpBufferLength;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"catch_up_buffer_length\": " + kalturaTimeShiftedTvPartnerSettings.CatchUpBufferLength;
                        }
                    }
                    if(kalturaTimeShiftedTvPartnerSettings.CatchUpEnabled.HasValue)
                    {
                        ret += ", \"catchUpEnabled\": " + kalturaTimeShiftedTvPartnerSettings.CatchUpEnabled;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"catch_up_enabled\": " + kalturaTimeShiftedTvPartnerSettings.CatchUpEnabled;
                        }
                    }
                    if(kalturaTimeShiftedTvPartnerSettings.CdvrEnabled.HasValue)
                    {
                        ret += ", \"cdvrEnabled\": " + kalturaTimeShiftedTvPartnerSettings.CdvrEnabled;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"cdvr_enabled\": " + kalturaTimeShiftedTvPartnerSettings.CdvrEnabled;
                        }
                    }
                    if(kalturaTimeShiftedTvPartnerSettings.CleanupNoticePeriod.HasValue)
                    {
                        ret += ", \"cleanupNoticePeriod\": " + kalturaTimeShiftedTvPartnerSettings.CleanupNoticePeriod;
                    }
                    if(kalturaTimeShiftedTvPartnerSettings.NonEntitledChannelPlaybackEnabled.HasValue)
                    {
                        ret += ", \"nonEntitledChannelPlaybackEnabled\": " + kalturaTimeShiftedTvPartnerSettings.NonEntitledChannelPlaybackEnabled;
                    }
                    if(kalturaTimeShiftedTvPartnerSettings.NonExistingChannelPlaybackEnabled.HasValue)
                    {
                        ret += ", \"nonExistingChannelPlaybackEnabled\": " + kalturaTimeShiftedTvPartnerSettings.NonExistingChannelPlaybackEnabled;
                    }
                    if(kalturaTimeShiftedTvPartnerSettings.PaddingAfterProgramEnds.HasValue)
                    {
                        ret += ", \"paddingAfterProgramEnds\": " + kalturaTimeShiftedTvPartnerSettings.PaddingAfterProgramEnds;
                    }
                    if(kalturaTimeShiftedTvPartnerSettings.PaddingBeforeProgramStarts.HasValue)
                    {
                        ret += ", \"paddingBeforeProgramStarts\": " + kalturaTimeShiftedTvPartnerSettings.PaddingBeforeProgramStarts;
                    }
                    if(kalturaTimeShiftedTvPartnerSettings.ProtectionEnabled.HasValue)
                    {
                        ret += ", \"protectionEnabled\": " + kalturaTimeShiftedTvPartnerSettings.ProtectionEnabled;
                    }
                    if(kalturaTimeShiftedTvPartnerSettings.ProtectionPeriod.HasValue)
                    {
                        ret += ", \"protectionPeriod\": " + kalturaTimeShiftedTvPartnerSettings.ProtectionPeriod;
                    }
                    if(kalturaTimeShiftedTvPartnerSettings.ProtectionPolicy.HasValue)
                    {
                        ret += ", \"protectionPolicy\": " + kalturaTimeShiftedTvPartnerSettings.ProtectionPolicy;
                    }
                    if(kalturaTimeShiftedTvPartnerSettings.ProtectionQuotaPercentage.HasValue)
                    {
                        ret += ", \"protectionQuotaPercentage\": " + kalturaTimeShiftedTvPartnerSettings.ProtectionQuotaPercentage;
                    }
                    if(kalturaTimeShiftedTvPartnerSettings.QuotaOveragePolicy.HasValue)
                    {
                        ret += ", \"quotaOveragePolicy\": " + kalturaTimeShiftedTvPartnerSettings.QuotaOveragePolicy;
                    }
                    if(kalturaTimeShiftedTvPartnerSettings.RecordingLifetimePeriod.HasValue)
                    {
                        ret += ", \"recordingLifetimePeriod\": " + kalturaTimeShiftedTvPartnerSettings.RecordingLifetimePeriod;
                    }
                    if(kalturaTimeShiftedTvPartnerSettings.RecordingScheduleWindow.HasValue)
                    {
                        ret += ", \"recordingScheduleWindow\": " + kalturaTimeShiftedTvPartnerSettings.RecordingScheduleWindow;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"recording_schedule_window\": " + kalturaTimeShiftedTvPartnerSettings.RecordingScheduleWindow;
                        }
                    }
                    if(kalturaTimeShiftedTvPartnerSettings.RecordingScheduleWindowEnabled.HasValue)
                    {
                        ret += ", \"recordingScheduleWindowEnabled\": " + kalturaTimeShiftedTvPartnerSettings.RecordingScheduleWindowEnabled;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"recording_schedule_window_enabled\": " + kalturaTimeShiftedTvPartnerSettings.RecordingScheduleWindowEnabled;
                        }
                    }
                    if(kalturaTimeShiftedTvPartnerSettings.RecoveryGracePeriod.HasValue)
                    {
                        ret += ", \"recoveryGracePeriod\": " + kalturaTimeShiftedTvPartnerSettings.RecoveryGracePeriod;
                    }
                    if(kalturaTimeShiftedTvPartnerSettings.SeriesRecordingEnabled.HasValue)
                    {
                        ret += ", \"seriesRecordingEnabled\": " + kalturaTimeShiftedTvPartnerSettings.SeriesRecordingEnabled;
                    }
                    if(kalturaTimeShiftedTvPartnerSettings.StartOverEnabled.HasValue)
                    {
                        ret += ", \"startOverEnabled\": " + kalturaTimeShiftedTvPartnerSettings.StartOverEnabled;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"start_over_enabled\": " + kalturaTimeShiftedTvPartnerSettings.StartOverEnabled;
                        }
                    }
                    if(kalturaTimeShiftedTvPartnerSettings.TrickPlayBufferLength.HasValue)
                    {
                        ret += ", \"trickPlayBufferLength\": " + kalturaTimeShiftedTvPartnerSettings.TrickPlayBufferLength;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"trick_play_buffer_length\": " + kalturaTimeShiftedTvPartnerSettings.TrickPlayBufferLength;
                        }
                    }
                    if(kalturaTimeShiftedTvPartnerSettings.TrickPlayEnabled.HasValue)
                    {
                        ret += ", \"trickPlayEnabled\": " + kalturaTimeShiftedTvPartnerSettings.TrickPlayEnabled;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"trick_play_enabled\": " + kalturaTimeShiftedTvPartnerSettings.TrickPlayEnabled;
                        }
                    }
                    break;
                    
                case "KalturaTopic":
                    KalturaTopic kalturaTopic = ottObject as KalturaTopic;
                    ret += "\"objectType\": " + "\"" + kalturaTopic.objectType + "\"";
                    if(kalturaTopic.relatedObjects != null && kalturaTopic.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaTopic.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"automaticIssueNotification\": " + kalturaTopic.AutomaticIssueNotification;
                    ret += ", \"id\": " + "\"" + kalturaTopic.Id + "\"";
                    ret += ", \"lastMessageSentDateSec\": " + kalturaTopic.LastMessageSentDateSec;
                    ret += ", \"name\": " + "\"" + kalturaTopic.Name + "\"";
                    ret += ", \"subscribersAmount\": " + "\"" + kalturaTopic.SubscribersAmount + "\"";
                    break;
                    
                case "KalturaTopicFilter":
                    KalturaTopicFilter kalturaTopicFilter = ottObject as KalturaTopicFilter;
                    ret += "\"objectType\": " + "\"" + kalturaTopicFilter.objectType + "\"";
                    if(kalturaTopicFilter.relatedObjects != null && kalturaTopicFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaTopicFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaTopicFilter.OrderBy;
                    break;
                    
                case "KalturaTopicListResponse":
                    KalturaTopicListResponse kalturaTopicListResponse = ottObject as KalturaTopicListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaTopicListResponse.objectType + "\"";
                    if(kalturaTopicListResponse.relatedObjects != null && kalturaTopicListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaTopicListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaTopicListResponse.TotalCount;
                    if(kalturaTopicListResponse.Topics != null && kalturaTopicListResponse.Topics.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaTopicListResponse.Topics.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaTopicResponse":
                    KalturaTopicResponse kalturaTopicResponse = ottObject as KalturaTopicResponse;
                    ret += "\"objectType\": " + "\"" + kalturaTopicResponse.objectType + "\"";
                    if(kalturaTopicResponse.relatedObjects != null && kalturaTopicResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaTopicResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaTopicResponse.TotalCount;
                    if(kalturaTopicResponse.Topics != null && kalturaTopicResponse.Topics.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaTopicResponse.Topics.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaTransaction":
                    KalturaTransaction kalturaTransaction = ottObject as KalturaTransaction;
                    ret += "\"objectType\": " + "\"" + kalturaTransaction.objectType + "\"";
                    if(kalturaTransaction.relatedObjects != null && kalturaTransaction.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaTransaction.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaTransaction.CreatedAt.HasValue)
                    {
                        ret += ", \"createdAt\": " + kalturaTransaction.CreatedAt;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"created_at\": " + kalturaTransaction.CreatedAt;
                        }
                    }
                    if(kalturaTransaction.FailReasonCode.HasValue)
                    {
                        ret += ", \"failReasonCode\": " + kalturaTransaction.FailReasonCode;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"fail_reason_code\": " + kalturaTransaction.FailReasonCode;
                        }
                    }
                    ret += ", \"id\": " + "\"" + kalturaTransaction.Id + "\"";
                    ret += ", \"paymentGatewayReferenceId\": " + "\"" + kalturaTransaction.PGReferenceID + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"payment_gateway_reference_id\": " + "\"" + kalturaTransaction.PGReferenceID + "\"";
                    }
                    ret += ", \"paymentGatewayResponseId\": " + "\"" + kalturaTransaction.PGResponseID + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"payment_gateway_response_id\": " + "\"" + kalturaTransaction.PGResponseID + "\"";
                    }
                    ret += ", \"state\": " + "\"" + kalturaTransaction.State + "\"";
                    break;
                    
                case "KalturaTransactionHistoryFilter":
                    KalturaTransactionHistoryFilter kalturaTransactionHistoryFilter = ottObject as KalturaTransactionHistoryFilter;
                    ret += "\"objectType\": " + "\"" + kalturaTransactionHistoryFilter.objectType + "\"";
                    if(kalturaTransactionHistoryFilter.relatedObjects != null && kalturaTransactionHistoryFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaTransactionHistoryFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaTransactionHistoryFilter.OrderBy;
                    if(kalturaTransactionHistoryFilter.EndDateLessThanOrEqual.HasValue)
                    {
                        ret += ", \"endDateLessThanOrEqual\": " + kalturaTransactionHistoryFilter.EndDateLessThanOrEqual;
                    }
                    ret += ", \"entityReferenceEqual\": " + kalturaTransactionHistoryFilter.EntityReferenceEqual;
                    if(kalturaTransactionHistoryFilter.StartDateGreaterThanOrEqual.HasValue)
                    {
                        ret += ", \"startDateGreaterThanOrEqual\": " + kalturaTransactionHistoryFilter.StartDateGreaterThanOrEqual;
                    }
                    break;
                    
                case "KalturaTransactionsFilter":
                    KalturaTransactionsFilter kalturaTransactionsFilter = ottObject as KalturaTransactionsFilter;
                    ret += "\"objectType\": " + "\"" + kalturaTransactionsFilter.objectType + "\"";
                    if(kalturaTransactionsFilter.relatedObjects != null && kalturaTransactionsFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaTransactionsFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaTransactionsFilter.PageIndex.HasValue)
                    {
                        ret += ", \"pageIndex\": " + kalturaTransactionsFilter.PageIndex;
                    }
                    if(kalturaTransactionsFilter.PageSize.HasValue)
                    {
                        ret += ", \"pageSize\": " + kalturaTransactionsFilter.PageSize;
                    }
                    ret += ", \"by\": " + kalturaTransactionsFilter.By;
                    if(kalturaTransactionsFilter.EndDate.HasValue)
                    {
                        ret += ", \"endDate\": " + kalturaTransactionsFilter.EndDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"end_date\": " + kalturaTransactionsFilter.EndDate;
                        }
                    }
                    if(kalturaTransactionsFilter.StartDate.HasValue)
                    {
                        ret += ", \"startDate\": " + kalturaTransactionsFilter.StartDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"start_date\": " + kalturaTransactionsFilter.StartDate;
                        }
                    }
                    break;
                    
                case "KalturaTransactionStatus":
                    KalturaTransactionStatus kalturaTransactionStatus = ottObject as KalturaTransactionStatus;
                    ret += "\"objectType\": " + "\"" + kalturaTransactionStatus.objectType + "\"";
                    if(kalturaTransactionStatus.relatedObjects != null && kalturaTransactionStatus.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaTransactionStatus.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"adapterTransactionStatus\": " + kalturaTransactionStatus.AdapterStatus;
                    ret += ", \"externalId\": " + "\"" + kalturaTransactionStatus.ExternalId + "\"";
                    ret += ", \"externalMessage\": " + "\"" + kalturaTransactionStatus.ExternalMessage + "\"";
                    ret += ", \"externalStatus\": " + "\"" + kalturaTransactionStatus.ExternalStatus + "\"";
                    ret += ", \"failReason\": " + kalturaTransactionStatus.FailReason;
                    break;
                    
                case "KalturaTranslationToken":
                    KalturaTranslationToken kalturaTranslationToken = ottObject as KalturaTranslationToken;
                    ret += "\"objectType\": " + "\"" + kalturaTranslationToken.objectType + "\"";
                    if(kalturaTranslationToken.relatedObjects != null && kalturaTranslationToken.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaTranslationToken.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"language\": " + "\"" + kalturaTranslationToken.Language + "\"";
                    ret += ", \"value\": " + "\"" + kalturaTranslationToken.Value + "\"";
                    break;
                    
                case "KalturaTwitterTwit":
                    KalturaTwitterTwit kalturaTwitterTwit = ottObject as KalturaTwitterTwit;
                    ret += "\"objectType\": " + "\"" + kalturaTwitterTwit.objectType + "\"";
                    if(kalturaTwitterTwit.relatedObjects != null && kalturaTwitterTwit.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaTwitterTwit.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"createDate\": " + kalturaTwitterTwit.CreateDate;
                    ret += ", \"header\": " + "\"" + kalturaTwitterTwit.Header + "\"";
                    ret += ", \"text\": " + "\"" + kalturaTwitterTwit.Text + "\"";
                    ret += ", \"writer\": " + "\"" + kalturaTwitterTwit.Writer + "\"";
                    ret += ", \"authorImageUrl\": " + "\"" + kalturaTwitterTwit.AuthorImageUrl + "\"";
                    ret += ", \"likeCounter\": " + "\"" + kalturaTwitterTwit.LikeCounter + "\"";
                    break;
                    
                case "KalturaUnifiedPaymentRenewal":
                    KalturaUnifiedPaymentRenewal kalturaUnifiedPaymentRenewal = ottObject as KalturaUnifiedPaymentRenewal;
                    ret += "\"objectType\": " + "\"" + kalturaUnifiedPaymentRenewal.objectType + "\"";
                    if(kalturaUnifiedPaymentRenewal.relatedObjects != null && kalturaUnifiedPaymentRenewal.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaUnifiedPaymentRenewal.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"date\": " + kalturaUnifiedPaymentRenewal.Date;
                    if(kalturaUnifiedPaymentRenewal.Entitlements != null && kalturaUnifiedPaymentRenewal.Entitlements.Count > 0)
                    {
                        ret += ", \"entitlements\": " + "[" + String.Join(", ", kalturaUnifiedPaymentRenewal.Entitlements.Select(item => Serialize(item))) + "]";
                    }
                    ret += ", \"price\": " + Serialize(kalturaUnifiedPaymentRenewal.Price);
                    ret += ", \"unifiedPaymentId\": " + kalturaUnifiedPaymentRenewal.UnifiedPaymentId;
                    break;
                    
                case "KalturaUsageModule":
                    KalturaUsageModule kalturaUsageModule = ottObject as KalturaUsageModule;
                    ret += "\"objectType\": " + "\"" + kalturaUsageModule.objectType + "\"";
                    if(kalturaUsageModule.relatedObjects != null && kalturaUsageModule.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaUsageModule.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaUsageModule.CouponId.HasValue)
                    {
                        ret += ", \"couponId\": " + kalturaUsageModule.CouponId;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"coupon_id\": " + kalturaUsageModule.CouponId;
                        }
                    }
                    if(kalturaUsageModule.FullLifeCycle.HasValue)
                    {
                        ret += ", \"fullLifeCycle\": " + kalturaUsageModule.FullLifeCycle;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"full_life_cycle\": " + kalturaUsageModule.FullLifeCycle;
                        }
                    }
                    if(kalturaUsageModule.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaUsageModule.Id;
                    }
                    if(kalturaUsageModule.IsOfflinePlayback.HasValue)
                    {
                        ret += ", \"isOfflinePlayback\": " + kalturaUsageModule.IsOfflinePlayback;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_offline_playback\": " + kalturaUsageModule.IsOfflinePlayback;
                        }
                    }
                    if(kalturaUsageModule.IsWaiverEnabled.HasValue)
                    {
                        ret += ", \"isWaiverEnabled\": " + kalturaUsageModule.IsWaiverEnabled;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_waiver_enabled\": " + kalturaUsageModule.IsWaiverEnabled;
                        }
                    }
                    if(kalturaUsageModule.MaxViewsNumber.HasValue)
                    {
                        ret += ", \"maxViewsNumber\": " + kalturaUsageModule.MaxViewsNumber;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"max_views_number\": " + kalturaUsageModule.MaxViewsNumber;
                        }
                    }
                    ret += ", \"name\": " + "\"" + kalturaUsageModule.Name + "\"";
                    if(kalturaUsageModule.ViewLifeCycle.HasValue)
                    {
                        ret += ", \"viewLifeCycle\": " + kalturaUsageModule.ViewLifeCycle;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"view_life_cycle\": " + kalturaUsageModule.ViewLifeCycle;
                        }
                    }
                    if(kalturaUsageModule.WaiverPeriod.HasValue)
                    {
                        ret += ", \"waiverPeriod\": " + kalturaUsageModule.WaiverPeriod;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"waiver_period\": " + kalturaUsageModule.WaiverPeriod;
                        }
                    }
                    break;
                    
                case "KalturaUserAssetRule":
                    KalturaUserAssetRule kalturaUserAssetRule = ottObject as KalturaUserAssetRule;
                    ret += "\"objectType\": " + "\"" + kalturaUserAssetRule.objectType + "\"";
                    if(kalturaUserAssetRule.relatedObjects != null && kalturaUserAssetRule.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaUserAssetRule.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"description\": " + "\"" + kalturaUserAssetRule.Description + "\"";
                    if(kalturaUserAssetRule.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaUserAssetRule.Id;
                    }
                    ret += ", \"name\": " + "\"" + kalturaUserAssetRule.Name + "\"";
                    ret += ", \"ruleType\": " + kalturaUserAssetRule.RuleType;
                    break;
                    
                case "KalturaUserAssetRuleFilter":
                    KalturaUserAssetRuleFilter kalturaUserAssetRuleFilter = ottObject as KalturaUserAssetRuleFilter;
                    ret += "\"objectType\": " + "\"" + kalturaUserAssetRuleFilter.objectType + "\"";
                    if(kalturaUserAssetRuleFilter.relatedObjects != null && kalturaUserAssetRuleFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaUserAssetRuleFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaUserAssetRuleFilter.OrderBy;
                    if(kalturaUserAssetRuleFilter.AssetIdEqual.HasValue)
                    {
                        ret += ", \"assetIdEqual\": " + kalturaUserAssetRuleFilter.AssetIdEqual;
                    }
                    if(kalturaUserAssetRuleFilter.AssetTypeEqual.HasValue)
                    {
                        ret += ", \"assetTypeEqual\": " + kalturaUserAssetRuleFilter.AssetTypeEqual;
                    }
                    break;
                    
                case "KalturaUserAssetRuleListResponse":
                    KalturaUserAssetRuleListResponse kalturaUserAssetRuleListResponse = ottObject as KalturaUserAssetRuleListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaUserAssetRuleListResponse.objectType + "\"";
                    if(kalturaUserAssetRuleListResponse.relatedObjects != null && kalturaUserAssetRuleListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaUserAssetRuleListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaUserAssetRuleListResponse.TotalCount;
                    if(kalturaUserAssetRuleListResponse.Rules != null && kalturaUserAssetRuleListResponse.Rules.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaUserAssetRuleListResponse.Rules.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaUserAssetsList":
                    KalturaUserAssetsList kalturaUserAssetsList = ottObject as KalturaUserAssetsList;
                    ret += "\"objectType\": " + "\"" + kalturaUserAssetsList.objectType + "\"";
                    if(kalturaUserAssetsList.relatedObjects != null && kalturaUserAssetsList.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaUserAssetsList.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaUserAssetsList.List != null && kalturaUserAssetsList.List.Count > 0)
                    {
                        ret += ", \"list\": " + "[" + String.Join(", ", kalturaUserAssetsList.List.Select(item => Serialize(item))) + "]";
                    }
                    ret += ", \"listType\": " + kalturaUserAssetsList.ListType;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"list_type\": " + kalturaUserAssetsList.ListType;
                    }
                    break;
                    
                case "KalturaUserAssetsListFilter":
                    KalturaUserAssetsListFilter kalturaUserAssetsListFilter = ottObject as KalturaUserAssetsListFilter;
                    ret += "\"objectType\": " + "\"" + kalturaUserAssetsListFilter.objectType + "\"";
                    if(kalturaUserAssetsListFilter.relatedObjects != null && kalturaUserAssetsListFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaUserAssetsListFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"assetTypeEqual\": " + kalturaUserAssetsListFilter.AssetTypeEqual;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"asset_type\": " + kalturaUserAssetsListFilter.AssetTypeEqual;
                    }
                    ret += ", \"by\": " + kalturaUserAssetsListFilter.By;
                    ret += ", \"listTypeEqual\": " + kalturaUserAssetsListFilter.ListTypeEqual;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"list_type\": " + kalturaUserAssetsListFilter.ListTypeEqual;
                    }
                    break;
                    
                case "KalturaUserAssetsListItem":
                    KalturaUserAssetsListItem kalturaUserAssetsListItem = ottObject as KalturaUserAssetsListItem;
                    ret += "\"objectType\": " + "\"" + kalturaUserAssetsListItem.objectType + "\"";
                    if(kalturaUserAssetsListItem.relatedObjects != null && kalturaUserAssetsListItem.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaUserAssetsListItem.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"id\": " + "\"" + kalturaUserAssetsListItem.Id + "\"";
                    ret += ", \"listType\": " + kalturaUserAssetsListItem.ListType;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"list_type\": " + kalturaUserAssetsListItem.ListType;
                    }
                    if(kalturaUserAssetsListItem.OrderIndex.HasValue)
                    {
                        ret += ", \"orderIndex\": " + kalturaUserAssetsListItem.OrderIndex;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"order_index\": " + kalturaUserAssetsListItem.OrderIndex;
                        }
                    }
                    ret += ", \"type\": " + kalturaUserAssetsListItem.Type;
                    ret += ", \"userId\": " + "\"" + kalturaUserAssetsListItem.UserId + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"user_id\": " + "\"" + kalturaUserAssetsListItem.UserId + "\"";
                    }
                    break;
                    
                case "KalturaUserBillingTransaction":
                    KalturaUserBillingTransaction kalturaUserBillingTransaction = ottObject as KalturaUserBillingTransaction;
                    ret += "\"objectType\": " + "\"" + kalturaUserBillingTransaction.objectType + "\"";
                    if(kalturaUserBillingTransaction.relatedObjects != null && kalturaUserBillingTransaction.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaUserBillingTransaction.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaUserBillingTransaction.actionDate.HasValue)
                    {
                        ret += ", \"actionDate\": " + kalturaUserBillingTransaction.actionDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"action_date\": " + kalturaUserBillingTransaction.actionDate;
                        }
                    }
                    ret += ", \"billingAction\": " + kalturaUserBillingTransaction.billingAction;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"billing_action\": " + kalturaUserBillingTransaction.billingAction;
                    }
                    ret += ", \"billingPriceType\": " + kalturaUserBillingTransaction.billingPriceType;
                    if(kalturaUserBillingTransaction.billingProviderRef.HasValue)
                    {
                        ret += ", \"billingProviderRef\": " + kalturaUserBillingTransaction.billingProviderRef;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"billing_provider_ref\": " + kalturaUserBillingTransaction.billingProviderRef;
                        }
                    }
                    if(kalturaUserBillingTransaction.endDate.HasValue)
                    {
                        ret += ", \"endDate\": " + kalturaUserBillingTransaction.endDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"end_date\": " + kalturaUserBillingTransaction.endDate;
                        }
                    }
                    if(kalturaUserBillingTransaction.isRecurring.HasValue)
                    {
                        ret += ", \"isRecurring\": " + kalturaUserBillingTransaction.isRecurring;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"is_recurring\": " + kalturaUserBillingTransaction.isRecurring;
                        }
                    }
                    ret += ", \"itemType\": " + kalturaUserBillingTransaction.itemType;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"item_type\": " + kalturaUserBillingTransaction.itemType;
                    }
                    ret += ", \"paymentMethod\": " + kalturaUserBillingTransaction.paymentMethod;
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"payment_method\": " + kalturaUserBillingTransaction.paymentMethod;
                    }
                    ret += ", \"paymentMethodExtraDetails\": " + "\"" + kalturaUserBillingTransaction.paymentMethodExtraDetails + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"payment_method_extra_details\": " + "\"" + kalturaUserBillingTransaction.paymentMethodExtraDetails + "\"";
                    }
                    ret += ", \"price\": " + Serialize(kalturaUserBillingTransaction.price);
                    ret += ", \"purchasedItemCode\": " + "\"" + kalturaUserBillingTransaction.purchasedItemCode + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"purchased_item_code\": " + "\"" + kalturaUserBillingTransaction.purchasedItemCode + "\"";
                    }
                    ret += ", \"purchasedItemName\": " + "\"" + kalturaUserBillingTransaction.purchasedItemName + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"purchased_item_name\": " + "\"" + kalturaUserBillingTransaction.purchasedItemName + "\"";
                    }
                    if(kalturaUserBillingTransaction.purchaseID.HasValue)
                    {
                        ret += ", \"purchaseId\": " + kalturaUserBillingTransaction.purchaseID;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"purchase_id\": " + kalturaUserBillingTransaction.purchaseID;
                        }
                    }
                    ret += ", \"recieptCode\": " + "\"" + kalturaUserBillingTransaction.recieptCode + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"reciept_code\": " + "\"" + kalturaUserBillingTransaction.recieptCode + "\"";
                    }
                    ret += ", \"remarks\": " + "\"" + kalturaUserBillingTransaction.remarks + "\"";
                    if(kalturaUserBillingTransaction.startDate.HasValue)
                    {
                        ret += ", \"startDate\": " + kalturaUserBillingTransaction.startDate;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"start_date\": " + kalturaUserBillingTransaction.startDate;
                        }
                    }
                    ret += ", \"userFullName\": " + "\"" + kalturaUserBillingTransaction.UserFullName + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"user_full_name\": " + "\"" + kalturaUserBillingTransaction.UserFullName + "\"";
                    }
                    ret += ", \"userId\": " + "\"" + kalturaUserBillingTransaction.UserID + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"user_id\": " + "\"" + kalturaUserBillingTransaction.UserID + "\"";
                    }
                    break;
                    
                case "KalturaUserInterest":
                    KalturaUserInterest kalturaUserInterest = ottObject as KalturaUserInterest;
                    ret += "\"objectType\": " + "\"" + kalturaUserInterest.objectType + "\"";
                    if(kalturaUserInterest.relatedObjects != null && kalturaUserInterest.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaUserInterest.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"id\": " + "\"" + kalturaUserInterest.Id + "\"";
                    ret += ", \"topic\": " + Serialize(kalturaUserInterest.Topic);
                    break;
                    
                case "KalturaUserInterestListResponse":
                    KalturaUserInterestListResponse kalturaUserInterestListResponse = ottObject as KalturaUserInterestListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaUserInterestListResponse.objectType + "\"";
                    if(kalturaUserInterestListResponse.relatedObjects != null && kalturaUserInterestListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaUserInterestListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaUserInterestListResponse.TotalCount;
                    if(kalturaUserInterestListResponse.UserInterests != null && kalturaUserInterestListResponse.UserInterests.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaUserInterestListResponse.UserInterests.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaUserInterestTopic":
                    KalturaUserInterestTopic kalturaUserInterestTopic = ottObject as KalturaUserInterestTopic;
                    ret += "\"objectType\": " + "\"" + kalturaUserInterestTopic.objectType + "\"";
                    if(kalturaUserInterestTopic.relatedObjects != null && kalturaUserInterestTopic.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaUserInterestTopic.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"metaId\": " + "\"" + kalturaUserInterestTopic.MetaId + "\"";
                    ret += ", \"parentTopic\": " + Serialize(kalturaUserInterestTopic.ParentTopic);
                    ret += ", \"value\": " + "\"" + kalturaUserInterestTopic.Value + "\"";
                    break;
                    
                case "KalturaUserLoginPin":
                    KalturaUserLoginPin kalturaUserLoginPin = ottObject as KalturaUserLoginPin;
                    ret += "\"objectType\": " + "\"" + kalturaUserLoginPin.objectType + "\"";
                    if(kalturaUserLoginPin.relatedObjects != null && kalturaUserLoginPin.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaUserLoginPin.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaUserLoginPin.ExpirationTime.HasValue)
                    {
                        ret += ", \"expirationTime\": " + kalturaUserLoginPin.ExpirationTime;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"expiration_time\": " + kalturaUserLoginPin.ExpirationTime;
                        }
                    }
                    ret += ", \"pinCode\": " + "\"" + kalturaUserLoginPin.PinCode + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"pin_code\": " + "\"" + kalturaUserLoginPin.PinCode + "\"";
                    }
                    ret += ", \"userId\": " + "\"" + kalturaUserLoginPin.UserId + "\"";
                    if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                    {
                        ret += ", \"user_id\": " + "\"" + kalturaUserLoginPin.UserId + "\"";
                    }
                    break;
                    
                case "KalturaUserRole":
                    KalturaUserRole kalturaUserRole = ottObject as KalturaUserRole;
                    ret += "\"objectType\": " + "\"" + kalturaUserRole.objectType + "\"";
                    if(kalturaUserRole.relatedObjects != null && kalturaUserRole.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaUserRole.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"excludedPermissionNames\": " + "\"" + kalturaUserRole.ExcludedPermissionNames + "\"";
                    if(kalturaUserRole.Id.HasValue)
                    {
                        ret += ", \"id\": " + kalturaUserRole.Id;
                    }
                    ret += ", \"name\": " + "\"" + kalturaUserRole.Name + "\"";
                    ret += ", \"permissionNames\": " + "\"" + kalturaUserRole.PermissionNames + "\"";
                    if(kalturaUserRole.Permissions != null && kalturaUserRole.Permissions.Count > 0)
                    {
                        ret += ", \"permissions\": " + "[" + String.Join(", ", kalturaUserRole.Permissions.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaUserRoleFilter":
                    KalturaUserRoleFilter kalturaUserRoleFilter = ottObject as KalturaUserRoleFilter;
                    ret += "\"objectType\": " + "\"" + kalturaUserRoleFilter.objectType + "\"";
                    if(kalturaUserRoleFilter.relatedObjects != null && kalturaUserRoleFilter.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaUserRoleFilter.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"orderBy\": " + kalturaUserRoleFilter.OrderBy;
                    if(kalturaUserRoleFilter.CurrentUserRoleIdsContains.HasValue)
                    {
                        ret += ", \"currentUserRoleIdsContains\": " + kalturaUserRoleFilter.CurrentUserRoleIdsContains;
                    }
                    ret += ", \"idIn\": " + "\"" + kalturaUserRoleFilter.IdIn + "\"";
                    if(kalturaUserRoleFilter.Ids != null && kalturaUserRoleFilter.Ids.Count > 0)
                    {
                        ret += ", \"ids\": " + "[" + String.Join(", ", kalturaUserRoleFilter.Ids.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaUserRoleListResponse":
                    KalturaUserRoleListResponse kalturaUserRoleListResponse = ottObject as KalturaUserRoleListResponse;
                    ret += "\"objectType\": " + "\"" + kalturaUserRoleListResponse.objectType + "\"";
                    if(kalturaUserRoleListResponse.relatedObjects != null && kalturaUserRoleListResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaUserRoleListResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaUserRoleListResponse.TotalCount;
                    if(kalturaUserRoleListResponse.UserRoles != null && kalturaUserRoleListResponse.UserRoles.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaUserRoleListResponse.UserRoles.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "KalturaUserSocialActionResponse":
                    KalturaUserSocialActionResponse kalturaUserSocialActionResponse = ottObject as KalturaUserSocialActionResponse;
                    ret += "\"objectType\": " + "\"" + kalturaUserSocialActionResponse.objectType + "\"";
                    if(kalturaUserSocialActionResponse.relatedObjects != null && kalturaUserSocialActionResponse.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaUserSocialActionResponse.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    if(kalturaUserSocialActionResponse.NetworkStatus != null && kalturaUserSocialActionResponse.NetworkStatus.Count > 0)
                    {
                        ret += ", \"failStatus\": " + "[" + String.Join(", ", kalturaUserSocialActionResponse.NetworkStatus.Select(item => Serialize(item))) + "]";
                    }
                    ret += ", \"socialAction\": " + Serialize(kalturaUserSocialActionResponse.SocialAction);
                    break;
                    
                case "KalturaValue":
                    KalturaValue kalturaValue = ottObject as KalturaValue;
                    ret += "\"objectType\": " + "\"" + kalturaValue.objectType + "\"";
                    if(kalturaValue.relatedObjects != null && kalturaValue.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaValue.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"description\": " + "\"" + kalturaValue.description + "\"";
                    break;
                    
                case "KalturaWatchHistoryAsset":
                    KalturaWatchHistoryAsset kalturaWatchHistoryAsset = ottObject as KalturaWatchHistoryAsset;
                    ret += "\"objectType\": " + "\"" + kalturaWatchHistoryAsset.objectType + "\"";
                    if(kalturaWatchHistoryAsset.relatedObjects != null && kalturaWatchHistoryAsset.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaWatchHistoryAsset.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"asset\": " + Serialize(kalturaWatchHistoryAsset.Asset);
                    if(kalturaWatchHistoryAsset.Duration.HasValue)
                    {
                        ret += ", \"duration\": " + kalturaWatchHistoryAsset.Duration;
                    }
                    if(kalturaWatchHistoryAsset.IsFinishedWatching.HasValue)
                    {
                        ret += ", \"finishedWatching\": " + kalturaWatchHistoryAsset.IsFinishedWatching;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"finished_watching\": " + kalturaWatchHistoryAsset.IsFinishedWatching;
                        }
                    }
                    if(kalturaWatchHistoryAsset.LastWatched.HasValue)
                    {
                        ret += ", \"watchedDate\": " + kalturaWatchHistoryAsset.LastWatched;
                        if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                        {
                            ret += ", \"watched_date\": " + kalturaWatchHistoryAsset.LastWatched;
                        }
                    }
                    if(kalturaWatchHistoryAsset.Position.HasValue)
                    {
                        ret += ", \"position\": " + kalturaWatchHistoryAsset.Position;
                    }
                    break;
                    
                case "KalturaWatchHistoryAssetWrapper":
                    KalturaWatchHistoryAssetWrapper kalturaWatchHistoryAssetWrapper = ottObject as KalturaWatchHistoryAssetWrapper;
                    ret += "\"objectType\": " + "\"" + kalturaWatchHistoryAssetWrapper.objectType + "\"";
                    if(kalturaWatchHistoryAssetWrapper.relatedObjects != null && kalturaWatchHistoryAssetWrapper.relatedObjects.Count > 0)
                    {
                        ret += ", \"relatedObjects\": " + "{" + String.Join(", ", kalturaWatchHistoryAssetWrapper.relatedObjects.Select(pair => "\"" + pair.Key + "\": " + Serialize(pair.Value))) + "}";
                    }
                    ret += ", \"totalCount\": " + kalturaWatchHistoryAssetWrapper.TotalCount;
                    if(kalturaWatchHistoryAssetWrapper.Objects != null && kalturaWatchHistoryAssetWrapper.Objects.Count > 0)
                    {
                        ret += ", \"objects\": " + "[" + String.Join(", ", kalturaWatchHistoryAssetWrapper.Objects.Select(item => Serialize(item))) + "]";
                    }
                    break;
                    
                case "StatusWrapper":
                    StatusWrapper ktatusWrapper = ottObject as StatusWrapper;
                    ret += "\"executionTime\": " + ktatusWrapper.ExecutionTime;
                    ret += ", \"result\": " + (ktatusWrapper.Result is IKalturaJsonable ? Serialize(ktatusWrapper.Result as IKalturaJsonable) : JsonManager.GetInstance().Serialize(ktatusWrapper.Result));
                    break;
                    
            }
            ret += "}";
            return ret;
        }
        
    }
}
