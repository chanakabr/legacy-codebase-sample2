// NOTICE: This is a generated file, to modify it, edit Program.cs in Reflector project
using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using WebAPI.Controllers;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers;
using WebAPI.Managers.Scheme;
using WebAPI.Models.MultiRequest;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.Social;
using WebAPI.Models.General;
using WebAPI.Models.Notifications;
using WebAPI.Models.Notification;
using WebAPI.App_Start;
using WebAPI.Models.Catalog;
using WebAPI.Models.API;
using WebAPI.Models.Pricing;
using WebAPI.Models.Users;
using WebAPI.Models.Partner;
using WebAPI.Models.Upload;
using WebAPI.Models.DMS;
using WebAPI.Models.Domains;
using WebAPI.Models.Billing;
using WebAPI.EventNotifications;

namespace WebAPI.Reflection
{
    public class DataModel
    {
        public static string getApiName(PropertyInfo property)
        {
            switch (property.DeclaringType.Name)
            {
                case "KalturaAccessControlMessage":
                    switch(property.Name)
                    {
                        case "Code":
                            return "code";
                        case "Message":
                            return "message";
                    }
                    break;
                    
                case "KalturaActionPermissionItem":
                    switch(property.Name)
                    {
                        case "Action":
                            return "action";
                        case "ActionPrivacy":
                            return "actionPrivacy";
                        case "Network":
                            return "network";
                        case "Privacy":
                            return "privacy";
                    }
                    break;
                    
                case "KalturaAdsContext":
                    switch(property.Name)
                    {
                        case "Sources":
                            return "sources";
                    }
                    break;
                    
                case "KalturaAdsSource":
                    switch(property.Name)
                    {
                        case "AdsParams":
                            return "adsParam";
                        case "AdsPolicy":
                            return "adsPolicy";
                        case "Id":
                            return "id";
                        case "Type":
                            return "type";
                    }
                    break;
                    
                case "KalturaAnnouncement":
                    switch(property.Name)
                    {
                        case "Enabled":
                            return "enabled";
                        case "Id":
                            return "id";
                        case "ImageUrl":
                            return "imageUrl";
                        case "IncludeMail":
                            return "includeMail";
                        case "IncludeSms":
                            return "includeSms";
                        case "MailSubject":
                            return "mailSubject";
                        case "MailTemplate":
                            return "mailTemplate";
                        case "Message":
                            return "message";
                        case "Name":
                            return "name";
                        case "Recipients":
                            return "recipients";
                        case "StartTime":
                            return "startTime";
                        case "Status":
                            return "status";
                        case "Timezone":
                            return "timezone";
                    }
                    break;
                    
                case "KalturaAnnouncementListResponse":
                    switch(property.Name)
                    {
                        case "Announcements":
                            return "objects";
                    }
                    break;
                    
                case "KalturaApiActionPermissionItem":
                    switch(property.Name)
                    {
                        case "Action":
                            return "action";
                        case "Service":
                            return "service";
                    }
                    break;
                    
                case "KalturaApiArgumentPermissionItem":
                    switch(property.Name)
                    {
                        case "Action":
                            return "action";
                        case "Parameter":
                            return "parameter";
                        case "Service":
                            return "service";
                    }
                    break;
                    
                case "KalturaApiParameterPermissionItem":
                    switch(property.Name)
                    {
                        case "Action":
                            return "action";
                        case "Object":
                            return "object";
                        case "Parameter":
                            return "parameter";
                    }
                    break;
                    
                case "KalturaAppToken":
                    switch(property.Name)
                    {
                        case "CreateDate":
                            return "createDate";
                        case "Expiry":
                            return "expiry";
                        case "HashType":
                            return "hashType";
                        case "Id":
                            return "id";
                        case "PartnerId":
                            return "partnerId";
                        case "SessionDuration":
                            return "sessionDuration";
                        case "SessionPrivileges":
                            return "sessionPrivileges";
                        case "SessionType":
                            return "sessionType";
                        case "SessionUserId":
                            return "sessionUserId";
                        case "Status":
                            return "status";
                        case "Token":
                            return "token";
                        case "UpdateDate":
                            return "updateDate";
                    }
                    break;
                    
                case "KalturaAsset":
                    switch(property.Name)
                    {
                        case "CreateDate":
                            return "createDate";
                        case "Description":
                            return "description";
                        case "EnableCatchUp":
                            return "enableCatchUp";
                        case "EnableCdvr":
                            return "enableCdvr";
                        case "EnableStartOver":
                            return "enableStartOver";
                        case "EnableTrickPlay":
                            return "enableTrickPlay";
                        case "EndDate":
                            return "endDate";
                        case "ExternalId":
                            return "externalId";
                        case "Id":
                            return "id";
                        case "Images":
                            return "images";
                        case "MediaFiles":
                            return "mediaFiles";
                        case "Metas":
                            return "metas";
                        case "Name":
                            return "name";
                        case "StartDate":
                            return "startDate";
                        case "Statistics":
                            return "stats";
                        case "Tags":
                            return "tags";
                        case "Type":
                            return "type";
                        case "UpdateDate":
                            return "updateDate";
                    }
                    break;
                    
                case "KalturaAssetBookmark":
                    switch(property.Name)
                    {
                        case "IsFinishedWatching":
                            return "finishedWatching";
                        case "Position":
                            return "position";
                        case "PositionOwner":
                            return "positionOwner";
                        case "User":
                            return "user";
                    }
                    break;
                    
                case "KalturaAssetBookmarks":
                    switch(property.Name)
                    {
                        case "Bookmarks":
                            return "objects";
                    }
                    break;
                    
                case "KalturaAssetComment":
                    switch(property.Name)
                    {
                        case "AssetId":
                            return "assetId";
                        case "AssetType":
                            return "assetType";
                        case "Id":
                            return "id";
                        case "SubHeader":
                            return "subHeader";
                    }
                    break;
                    
                case "KalturaAssetCommentFilter":
                    switch(property.Name)
                    {
                        case "AssetIdEqual":
                            return "assetIdEqual";
                        case "AssetTypeEqual":
                            return "assetTypeEqual";
                    }
                    break;
                    
                case "KalturaAssetCommentListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaAssetCondition":
                    switch(property.Name)
                    {
                        case "Ksql":
                            return "ksql";
                    }
                    break;
                    
                case "KalturaAssetCount":
                    switch(property.Name)
                    {
                        case "Count":
                            return "count";
                        case "SubCounts":
                            return "subs";
                        case "Value":
                            return "value";
                    }
                    break;
                    
                case "KalturaAssetCountListResponse":
                    switch(property.Name)
                    {
                        case "AssetsCount":
                            return "assetsCount";
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaAssetFieldGroupBy":
                    switch(property.Name)
                    {
                        case "Value":
                            return "value";
                    }
                    break;
                    
                case "KalturaAssetFile":
                    switch(property.Name)
                    {
                        case "Url":
                            return "url";
                    }
                    break;
                    
                case "KalturaAssetFileContext":
                    switch(property.Name)
                    {
                        case "FullLifeCycle":
                            return "fullLifeCycle";
                        case "IsOfflinePlayBack":
                            return "isOfflinePlayBack";
                        case "ViewLifeCycle":
                            return "viewLifeCycle";
                    }
                    break;
                    
                case "KalturaAssetFilter":
                    switch(property.Name)
                    {
                        case "DynamicOrderBy":
                            return "dynamicOrderBy";
                    }
                    break;
                    
                case "KalturaAssetHistory":
                    switch(property.Name)
                    {
                        case "AssetId":
                            return "assetId";
                        case "AssetType":
                            return "assetType";
                        case "Duration":
                            return "duration";
                        case "IsFinishedWatching":
                            return "finishedWatching";
                        case "LastWatched":
                            return "watchedDate";
                        case "Position":
                            return "position";
                    }
                    break;
                    
                case "KalturaAssetHistoryFilter":
                    switch(property.Name)
                    {
                        case "AssetIdIn":
                            return "assetIdIn";
                        case "DaysLessThanOrEqual":
                            return "daysLessThanOrEqual";
                        case "StatusEqual":
                            return "statusEqual";
                        case "TypeIn":
                            return "typeIn";
                    }
                    break;
                    
                case "KalturaAssetHistoryListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaAssetInfo":
                    switch(property.Name)
                    {
                        case "EndDate":
                            return "endDate";
                        case "ExtraParams":
                            return "extraParams";
                        case "Metas":
                            return "metas";
                        case "StartDate":
                            return "startDate";
                        case "Tags":
                            return "tags";
                    }
                    break;
                    
                case "KalturaAssetInfoFilter":
                    switch(property.Name)
                    {
                        case "cutWith":
                            return "cut_with";
                        case "FilterTags":
                            return "filter_tags";
                        case "IDs":
                            return "ids";
                        case "ReferenceType":
                            return "referenceType";
                    }
                    break;
                    
                case "KalturaAssetInfoListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                        case "RequestId":
                            return "requestId";
                    }
                    break;
                    
                case "KalturaAssetListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaAssetMetaOrTagGroupBy":
                    switch(property.Name)
                    {
                        case "Value":
                            return "value";
                    }
                    break;
                    
                case "KalturaAssetPrice":
                    switch(property.Name)
                    {
                        case "AssetId":
                            return "asset_id";
                        case "AssetType":
                            return "asset_type";
                        case "FilePrices":
                            return "file_prices";
                    }
                    break;
                    
                case "KalturaAssetReminder":
                    switch(property.Name)
                    {
                        case "AssetId":
                            return "assetId";
                    }
                    break;
                    
                case "KalturaAssetRule":
                    switch(property.Name)
                    {
                        case "Actions":
                            return "actions";
                        case "Conditions":
                            return "conditions";
                    }
                    break;
                    
                case "KalturaAssetRuleBase":
                    switch(property.Name)
                    {
                        case "Description":
                            return "description";
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                    }
                    break;
                    
                case "KalturaAssetRuleFilter":
                    switch(property.Name)
                    {
                        case "AssetApplied":
                            return "assetApplied";
                        case "ConditionsContainType":
                            return "conditionsContainType";
                    }
                    break;
                    
                case "KalturaAssetRuleListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaAssetsBookmarksResponse":
                    switch(property.Name)
                    {
                        case "AssetsBookmarks":
                            return "objects";
                    }
                    break;
                    
                case "KalturaAssetsCount":
                    switch(property.Name)
                    {
                        case "Field":
                            return "field";
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaAssetsFilter":
                    switch(property.Name)
                    {
                        case "Assets":
                            return "assets";
                    }
                    break;
                    
                case "KalturaAssetStatistics":
                    switch(property.Name)
                    {
                        case "AssetId":
                            return "assetId";
                        case "BuzzAvgScore":
                            return "buzzScore";
                        case "Likes":
                            return "likes";
                        case "Rating":
                            return "rating";
                        case "RatingCount":
                            return "ratingCount";
                        case "Views":
                            return "views";
                    }
                    break;
                    
                case "KalturaAssetStatisticsListResponse":
                    switch(property.Name)
                    {
                        case "AssetsStatistics":
                            return "objects";
                    }
                    break;
                    
                case "KalturaAssetStatisticsQuery":
                    switch(property.Name)
                    {
                        case "AssetIdIn":
                            return "assetIdIn";
                        case "AssetTypeEqual":
                            return "assetTypeEqual";
                        case "EndDateGreaterThanOrEqual":
                            return "endDateGreaterThanOrEqual";
                        case "StartDateGreaterThanOrEqual":
                            return "startDateGreaterThanOrEqual";
                    }
                    break;
                    
                case "KalturaAssetStruct":
                    switch(property.Name)
                    {
                        case "ConnectedParentMetaId":
                            return "connectedParentMetaId";
                        case "ConnectingMetaId":
                            return "connectingMetaId";
                        case "CreateDate":
                            return "createDate";
                        case "Features":
                            return "features";
                        case "Id":
                            return "id";
                        case "IsProtected":
                            return "isProtected";
                        case "MetaIds":
                            return "metaIds";
                        case "Name":
                            return "name";
                        case "ParentId":
                            return "parentId";
                        case "PluralName":
                            return "pluralName";
                        case "SystemName":
                            return "systemName";
                        case "UpdateDate":
                            return "updateDate";
                    }
                    break;
                    
                case "KalturaAssetStructFilter":
                    switch(property.Name)
                    {
                        case "IdIn":
                            return "idIn";
                        case "IsProtectedEqual":
                            return "isProtectedEqual";
                        case "MetaIdEqual":
                            return "metaIdEqual";
                    }
                    break;
                    
                case "KalturaAssetStructListResponse":
                    switch(property.Name)
                    {
                        case "AssetStructs":
                            return "objects";
                    }
                    break;
                    
                case "KalturaAssetStructMeta":
                    switch(property.Name)
                    {
                        case "AssetStructId":
                            return "assetStructId";
                        case "CreateDate":
                            return "createDate";
                        case "DefaultIngestValue":
                            return "defaultIngestValue";
                        case "IngestInheritancePolicy":
                            return "ingestInheritancePolicy";
                        case "IngestReferencePath":
                            return "ingestReferencePath";
                        case "MetaId":
                            return "metaId";
                        case "ParentAssetStructId":
                            return "parentAssetStructId";
                        case "ParentInheritancePolicy":
                            return "parentInheritancePolicy";
                        case "ProtectFromIngest":
                            return "protectFromIngest";
                        case "UpdateDate":
                            return "updateDate";
                    }
                    break;
                    
                case "KalturaAssetStructMetaFilter":
                    switch(property.Name)
                    {
                        case "AssetStructIdEqual":
                            return "assetStructIdEqual";
                        case "MetaIdEqual":
                            return "metaIdEqual";
                    }
                    break;
                    
                case "KalturaAssetStructMetaListResponse":
                    switch(property.Name)
                    {
                        case "AssetStructMetas":
                            return "objects";
                    }
                    break;
                    
                case "KalturaAssetUserRule":
                    switch(property.Name)
                    {
                        case "Actions":
                            return "actions";
                        case "Conditions":
                            return "conditions";
                    }
                    break;
                    
                case "KalturaAssetUserRuleFilter":
                    switch(property.Name)
                    {
                        case "AttachedUserIdEqualCurrent":
                            return "attachedUserIdEqualCurrent";
                    }
                    break;
                    
                case "KalturaAssetUserRuleListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaBaseAssetInfo":
                    switch(property.Name)
                    {
                        case "Description":
                            return "description";
                        case "Id":
                            return "id";
                        case "Images":
                            return "images";
                        case "MediaFiles":
                            return "mediaFiles";
                        case "Name":
                            return "name";
                        case "Statistics":
                            return "stats";
                        case "Type":
                            return "type";
                    }
                    break;
                    
                case "KalturaBaseChannel":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                    }
                    break;
                    
                case "KalturaBaseOTTUser":
                    switch(property.Name)
                    {
                        case "FirstName":
                            return "firstName";
                        case "Id":
                            return "id";
                        case "LastName":
                            return "lastName";
                        case "Username":
                            return "username";
                    }
                    break;
                    
                case "KalturaBaseSearchAssetFilter":
                    switch(property.Name)
                    {
                        case "GroupBy":
                            return "groupBy";
                        case "Ksql":
                            return "kSql";
                    }
                    break;
                    
                case "KalturaBillingPartnerConfig":
                    switch(property.Name)
                    {
                        case "PartnerConfigurationType":
                            return "partnerConfigurationType";
                        case "Type":
                            return "type";
                        case "Value":
                            return "value";
                    }
                    break;
                    
                case "KalturaBillingResponse":
                    switch(property.Name)
                    {
                        case "ExternalReceiptCode":
                            return "externalReceiptCode";
                        case "ReceiptCode":
                            return "receiptCode";
                    }
                    break;
                    
                case "KalturaBillingTransaction":
                    switch(property.Name)
                    {
                        case "purchaseID":
                            return "purchaseId";
                    }
                    break;
                    
                case "KalturaBillingTransactionListResponse":
                    switch(property.Name)
                    {
                        case "transactions":
                            return "objects";
                    }
                    break;
                    
                case "KalturaBookmark":
                    switch(property.Name)
                    {
                        case "IsFinishedWatching":
                            return "finishedWatching";
                        case "IsReportingMode":
                            return "isReportingMode";
                        case "PlayerData":
                            return "playerData";
                        case "Position":
                            return "position";
                        case "PositionOwner":
                            return "positionOwner";
                        case "ProgramId":
                            return "programId";
                        case "User":
                            return "user";
                        case "UserId":
                            return "userId";
                    }
                    break;
                    
                case "KalturaBookmarkFilter":
                    switch(property.Name)
                    {
                        case "AssetIdIn":
                            return "assetIdIn";
                        case "AssetIn":
                            return "assetIn";
                        case "AssetTypeEqual":
                            return "assetTypeEqual";
                    }
                    break;
                    
                case "KalturaBookmarkListResponse":
                    switch(property.Name)
                    {
                        case "AssetsBookmarks":
                            return "objects";
                    }
                    break;
                    
                case "KalturaBookmarkPlayerData":
                    switch(property.Name)
                    {
                        case "averageBitRate":
                            return "averageBitrate";
                        case "currentBitRate":
                            return "currentBitrate";
                        case "FileId":
                            return "fileId";
                        case "totalBitRate":
                            return "totalBitrate";
                    }
                    break;
                    
                case "KalturaBulk":
                    switch(property.Name)
                    {
                        case "CreateDate":
                            return "createDate";
                        case "Id":
                            return "id";
                        case "Status":
                            return "status";
                        case "UpdateDate":
                            return "updateDate";
                    }
                    break;
                    
                case "KalturaBulkFilter":
                    switch(property.Name)
                    {
                        case "StatusEqual":
                            return "statusEqual";
                    }
                    break;
                    
                case "KalturaBulkListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaBundleFilter":
                    switch(property.Name)
                    {
                        case "BundleTypeEqual":
                            return "bundleTypeEqual";
                        case "IdEqual":
                            return "idEqual";
                        case "TypeIn":
                            return "typeIn";
                    }
                    break;
                    
                case "KalturaBuzzScore":
                    switch(property.Name)
                    {
                        case "AvgScore":
                            return "avgScore";
                        case "NormalizedAvgScore":
                            return "normalizedAvgScore";
                        case "UpdateDate":
                            return "updateDate";
                    }
                    break;
                    
                case "KalturaCDNAdapterProfile":
                    switch(property.Name)
                    {
                        case "AdapterUrl":
                            return "adapterUrl";
                        case "BaseUrl":
                            return "baseUrl";
                        case "Id":
                            return "id";
                        case "IsActive":
                            return "isActive";
                        case "Name":
                            return "name";
                        case "Settings":
                            return "settings";
                        case "SharedSecret":
                            return "sharedSecret";
                        case "SystemName":
                            return "systemName";
                    }
                    break;
                    
                case "KalturaCDNAdapterProfileListResponse":
                    switch(property.Name)
                    {
                        case "Adapters":
                            return "objects";
                    }
                    break;
                    
                case "KalturaCDNPartnerSettings":
                    switch(property.Name)
                    {
                        case "DefaultAdapterId":
                            return "defaultAdapterId";
                        case "DefaultRecordingAdapterId":
                            return "defaultRecordingAdapterId";
                    }
                    break;
                    
                case "KalturaCDVRAdapterProfile":
                    switch(property.Name)
                    {
                        case "AdapterUrl":
                            return "adapterUrl";
                        case "DynamicLinksSupport":
                            return "dynamicLinksSupport";
                        case "ExternalIdentifier":
                            return "externalIdentifier";
                        case "Id":
                            return "id";
                        case "IsActive":
                            return "isActive";
                        case "Name":
                            return "name";
                        case "Settings":
                            return "settings";
                        case "SharedSecret":
                            return "sharedSecret";
                    }
                    break;
                    
                case "KalturaCDVRAdapterProfileListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaChannel":
                    switch(property.Name)
                    {
                        case "AssetTypes":
                            return "assetTypes";
                        case "CreateDate":
                            return "createDate";
                        case "Description":
                            return "description";
                        case "FilterExpression":
                            return "filterExpression";
                        case "GroupBy":
                            return "groupBy";
                        case "Id":
                            return "id";
                        case "Images":
                            return "images";
                        case "IsActive":
                            return "isActive";
                        case "MediaTypes":
                            return "media_types";
                        case "Name":
                            return "name";
                        case "OldDescription":
                            return "oldDescription";
                        case "OldName":
                            return "oldName";
                        case "Order":
                            return "order";
                        case "OrderBy":
                            return "orderBy";
                        case "SystemName":
                            return "systemName";
                        case "UpdateDate":
                            return "updateDate";
                    }
                    break;
                    
                case "KalturaChannelExternalFilter":
                    switch(property.Name)
                    {
                        case "FreeText":
                            return "freeText";
                        case "IdEqual":
                            return "idEqual";
                        case "UtcOffsetEqual":
                            return "utcOffsetEqual";
                    }
                    break;
                    
                case "KalturaChannelFilter":
                    switch(property.Name)
                    {
                        case "ExcludeWatched":
                            return "excludeWatched";
                        case "IdEqual":
                            return "idEqual";
                        case "KSql":
                            return "kSql";
                        case "OrderBy":
                            return "orderBy";
                    }
                    break;
                    
                case "KalturaChannelListResponse":
                    switch(property.Name)
                    {
                        case "Channels":
                            return "objects";
                    }
                    break;
                    
                case "KalturaChannelOrder":
                    switch(property.Name)
                    {
                        case "DynamicOrderBy":
                            return "dynamicOrderBy";
                        case "SlidingWindowPeriod":
                            return "period";
                    }
                    break;
                    
                case "KalturaChannelProfile":
                    switch(property.Name)
                    {
                        case "AssetTypes":
                            return "assetTypes";
                        case "Description":
                            return "description";
                        case "FilterExpression":
                            return "filterExpression";
                        case "Id":
                            return "id";
                        case "IsActive":
                            return "isActive";
                        case "Name":
                            return "name";
                        case "Order":
                            return "order";
                    }
                    break;
                    
                case "KalturaChannelsFilter":
                    switch(property.Name)
                    {
                        case "IdEqual":
                            return "idEqual";
                        case "MediaIdEqual":
                            return "mediaIdEqual";
                        case "NameEqual":
                            return "nameEqual";
                        case "NameStartsWith":
                            return "nameStartsWith";
                    }
                    break;
                    
                case "KalturaClientConfiguration":
                    switch(property.Name)
                    {
                        case "ApiVersion":
                            return "apiVersion";
                        case "ClientTag":
                            return "clientTag";
                    }
                    break;
                    
                case "KalturaCollection":
                    switch(property.Name)
                    {
                        case "Channels":
                            return "channels";
                        case "CouponGroups":
                            return "couponsGroups";
                        case "Description":
                            return "description";
                        case "DiscountModule":
                            return "discountModule";
                        case "EndDate":
                            return "endDate";
                        case "ExternalId":
                            return "externalId";
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "PriceDetailsId":
                            return "priceDetailsId";
                        case "ProductCodes":
                            return "productCodes";
                        case "StartDate":
                            return "startDate";
                        case "UsageModule":
                            return "usageModule";
                    }
                    break;
                    
                case "KalturaCollectionFilter":
                    switch(property.Name)
                    {
                        case "CollectionIdIn":
                            return "collectionIdIn";
                        case "MediaFileIdEqual":
                            return "mediaFileIdEqual";
                    }
                    break;
                    
                case "KalturaCollectionListResponse":
                    switch(property.Name)
                    {
                        case "Collections":
                            return "objects";
                    }
                    break;
                    
                case "KalturaCompensation":
                    switch(property.Name)
                    {
                        case "Amount":
                            return "amount";
                        case "AppliedRenewalIterations":
                            return "appliedRenewalIterations";
                        case "CompensationType":
                            return "compensationType";
                        case "Id":
                            return "id";
                        case "PurchaseId":
                            return "purchaseId";
                        case "SubscriptionId":
                            return "subscriptionId";
                        case "TotalRenewalIterations":
                            return "totalRenewalIterations";
                    }
                    break;
                    
                case "KalturaConcurrencyCondition":
                    switch(property.Name)
                    {
                        case "ConcurrencyLimitationType":
                            return "concurrencyLimitationType";
                        case "Limit":
                            return "limit";
                    }
                    break;
                    
                case "KalturaConcurrencyPartnerConfig":
                    switch(property.Name)
                    {
                        case "DeviceFamilyIds":
                            return "deviceFamilyIds";
                        case "EvictionPolicy":
                            return "evictionPolicy";
                    }
                    break;
                    
                case "KalturaCondition":
                    switch(property.Name)
                    {
                        case "Description":
                            return "description";
                        case "Type":
                            return "type";
                    }
                    break;
                    
                case "KalturaConfigurationGroup":
                    switch(property.Name)
                    {
                        case "ConfigurationIdentifiers":
                            return "configurationIdentifiers";
                        case "Id":
                            return "id";
                        case "IsDefault":
                            return "isDefault";
                        case "Name":
                            return "name";
                        case "NumberOfDevices":
                            return "numberOfDevices";
                        case "PartnerId":
                            return "partnerId";
                        case "Tags":
                            return "tags";
                    }
                    break;
                    
                case "KalturaConfigurationGroupDevice":
                    switch(property.Name)
                    {
                        case "ConfigurationGroupId":
                            return "configurationGroupId";
                        case "PartnerId":
                            return "partnerId";
                        case "Udid":
                            return "udid";
                    }
                    break;
                    
                case "KalturaConfigurationGroupDeviceFilter":
                    switch(property.Name)
                    {
                        case "ConfigurationGroupIdEqual":
                            return "configurationGroupIdEqual";
                    }
                    break;
                    
                case "KalturaConfigurationGroupDeviceListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaConfigurationGroupListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaConfigurationGroupTag":
                    switch(property.Name)
                    {
                        case "ConfigurationGroupId":
                            return "configurationGroupId";
                        case "PartnerId":
                            return "partnerId";
                        case "Tag":
                            return "tag";
                    }
                    break;
                    
                case "KalturaConfigurationGroupTagFilter":
                    switch(property.Name)
                    {
                        case "ConfigurationGroupIdEqual":
                            return "configurationGroupIdEqual";
                    }
                    break;
                    
                case "KalturaConfigurationGroupTagListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaConfigurationIdentifier":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                    }
                    break;
                    
                case "KalturaConfigurations":
                    switch(property.Name)
                    {
                        case "AppName":
                            return "appName";
                        case "ClientVersion":
                            return "clientVersion";
                        case "ConfigurationGroupId":
                            return "configurationGroupId";
                        case "Content":
                            return "content";
                        case "ExternalPushId":
                            return "externalPushId";
                        case "Id":
                            return "id";
                        case "IsForceUpdate":
                            return "isForceUpdate";
                        case "PartnerId":
                            return "partnerId";
                        case "Platform":
                            return "platform";
                    }
                    break;
                    
                case "KalturaConfigurationsFilter":
                    switch(property.Name)
                    {
                        case "ConfigurationGroupIdEqual":
                            return "configurationGroupIdEqual";
                    }
                    break;
                    
                case "KalturaConfigurationsListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaCountry":
                    switch(property.Name)
                    {
                        case "Code":
                            return "code";
                        case "CurrencyCode":
                            return "currency";
                        case "CurrencySign":
                            return "currencySign";
                        case "Id":
                            return "id";
                        case "LanguagesCode":
                            return "languagesCode";
                        case "MainLanguageCode":
                            return "mainLanguageCode";
                        case "Name":
                            return "name";
                        case "TimeZoneId":
                            return "timeZoneId";
                        case "VatPercent":
                            return "vatPercent";
                    }
                    break;
                    
                case "KalturaCountryCondition":
                    switch(property.Name)
                    {
                        case "Countries":
                            return "countries";
                        case "Not":
                            return "not";
                    }
                    break;
                    
                case "KalturaCountryFilter":
                    switch(property.Name)
                    {
                        case "IdIn":
                            return "idIn";
                        case "IpEqual":
                            return "ipEqual";
                        case "IpEqualCurrent":
                            return "ipEqualCurrent";
                    }
                    break;
                    
                case "KalturaCountryListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaCoupon":
                    switch(property.Name)
                    {
                        case "CouponsGroup":
                            return "couponsGroup";
                        case "LeftUses":
                            return "leftUses";
                        case "Status":
                            return "status";
                        case "TotalUses":
                            return "totalUses";
                    }
                    break;
                    
                case "KalturaCouponsGroup":
                    switch(property.Name)
                    {
                        case "CouponGroupType":
                            return "couponGroupType";
                        case "Descriptions":
                            return "descriptions";
                        case "DiscountCode":
                            return "discountCode";
                        case "DiscountId":
                            return "discountId";
                        case "EndDate":
                            return "endDate";
                        case "Id":
                            return "id";
                        case "MaxHouseholdUses":
                            return "maxHouseholdUses";
                        case "MaxUsesNumber":
                            return "maxUsesNumber";
                        case "MaxUsesNumberOnRenewableSub":
                            return "maxUsesNumberOnRenewableSub";
                        case "Name":
                            return "name";
                        case "StartDate":
                            return "startDate";
                    }
                    break;
                    
                case "KalturaCouponsGroupListResponse":
                    switch(property.Name)
                    {
                        case "couponsGroups":
                            return "objects";
                    }
                    break;
                    
                case "KalturaCurrency":
                    switch(property.Name)
                    {
                        case "Code":
                            return "code";
                        case "IsDefault":
                            return "isDefault";
                        case "Name":
                            return "name";
                        case "Sign":
                            return "sign";
                    }
                    break;
                    
                case "KalturaCurrencyFilter":
                    switch(property.Name)
                    {
                        case "CodeIn":
                            return "codeIn";
                    }
                    break;
                    
                case "KalturaCurrencyListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaCustomDrmPlaybackPluginData":
                    switch(property.Name)
                    {
                        case "Data":
                            return "data";
                    }
                    break;
                    
                case "KalturaDetachedResponseProfile":
                    switch(property.Name)
                    {
                        case "Filter":
                            return "filter";
                        case "Name":
                            return "name";
                        case "RelatedProfiles":
                            return "relatedProfiles";
                    }
                    break;
                    
                case "KalturaDeviceBrand":
                    switch(property.Name)
                    {
                        case "DeviceFamilyId":
                            return "deviceFamilyid";
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                    }
                    break;
                    
                case "KalturaDeviceBrandListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaDeviceFamily":
                    switch(property.Name)
                    {
                        case "Devices":
                            return "devices";
                    }
                    break;
                    
                case "KalturaDeviceFamilyBase":
                    switch(property.Name)
                    {
                        case "ConcurrentLimit":
                            return "concurrentLimit";
                        case "DeviceLimit":
                            return "deviceLimit";
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                    }
                    break;
                    
                case "KalturaDeviceFamilyListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaDevicePin":
                    switch(property.Name)
                    {
                        case "Pin":
                            return "pin";
                    }
                    break;
                    
                case "KalturaDeviceRegistrationStatusHolder":
                    switch(property.Name)
                    {
                        case "Status":
                            return "status";
                    }
                    break;
                    
                case "KalturaDeviceReport":
                    switch(property.Name)
                    {
                        case "ConfigurationGroupId":
                            return "configurationGroupId";
                        case "LastAccessDate":
                            return "lastAccessDate";
                        case "LastAccessIP":
                            return "lastAccessIP";
                        case "OperationSystem":
                            return "operationSystem";
                        case "PartnerId":
                            return "partnerId";
                        case "PushParameters":
                            return "pushParameters";
                        case "Udid":
                            return "udid";
                        case "UserAgent":
                            return "userAgent";
                        case "VersionAppName":
                            return "versionAppName";
                        case "VersionNumber":
                            return "versionNumber";
                        case "VersionPlatform":
                            return "versionPlatform";
                    }
                    break;
                    
                case "KalturaDeviceReportFilter":
                    switch(property.Name)
                    {
                        case "LastAccessDateGreaterThanOrEqual":
                            return "lastAccessDateGreaterThanOrEqual";
                    }
                    break;
                    
                case "KalturaDiscount":
                    switch(property.Name)
                    {
                        case "Percentage":
                            return "percentage";
                    }
                    break;
                    
                case "KalturaDiscountDetails":
                    switch(property.Name)
                    {
                        case "EndtDate":
                            return "endDate";
                        case "Id":
                            return "id";
                        case "MultiCurrencyDiscount":
                            return "multiCurrencyDiscount";
                        case "StartDate":
                            return "startDate";
                    }
                    break;
                    
                case "KalturaDiscountDetailsFilter":
                    switch(property.Name)
                    {
                        case "IdIn":
                            return "idIn";
                    }
                    break;
                    
                case "KalturaDiscountDetailsListResponse":
                    switch(property.Name)
                    {
                        case "Discounts":
                            return "objects";
                    }
                    break;
                    
                case "KalturaDiscountModule":
                    switch(property.Name)
                    {
                        case "EndDate":
                            return "endDate";
                        case "Percent":
                            return "percent";
                        case "StartDate":
                            return "startDate";
                    }
                    break;
                    
                case "KalturaDrmPlaybackPluginData":
                    switch(property.Name)
                    {
                        case "LicenseURL":
                            return "licenseURL";
                        case "Scheme":
                            return "scheme";
                    }
                    break;
                    
                case "KalturaDrmProfile":
                    switch(property.Name)
                    {
                        case "AdapterUrl":
                            return "adapterUrl";
                        case "Id":
                            return "id";
                        case "IsActive":
                            return "isActive";
                        case "Name":
                            return "name";
                        case "Settings":
                            return "settings";
                        case "SharedSecret":
                            return "sharedSecret";
                        case "SystemName":
                            return "systemName";
                    }
                    break;
                    
                case "KalturaDrmProfileListResponse":
                    switch(property.Name)
                    {
                        case "Adapters":
                            return "objects";
                    }
                    break;
                    
                case "KalturaDynamicChannel":
                    switch(property.Name)
                    {
                        case "AssetTypes":
                            return "assetTypes";
                        case "GroupBy":
                            return "groupBy";
                        case "Ksql":
                            return "kSql";
                    }
                    break;
                    
                case "KalturaDynamicOrderBy":
                    switch(property.Name)
                    {
                        case "Name":
                            return "name";
                        case "OrderBy":
                            return "orderBy";
                    }
                    break;
                    
                case "KalturaEmailMessage":
                    switch(property.Name)
                    {
                        case "BccAddress":
                            return "bccAddress";
                        case "ExtraParameters":
                            return "extraParameters";
                        case "FirstName":
                            return "firstName";
                        case "LastName":
                            return "lastName";
                        case "SenderFrom":
                            return "senderFrom";
                        case "SenderName":
                            return "senderName";
                        case "SenderTo":
                            return "senderTo";
                        case "Subject":
                            return "subject";
                        case "TemplateName":
                            return "templateName";
                    }
                    break;
                    
                case "KalturaEngagement":
                    switch(property.Name)
                    {
                        case "AdapterDynamicData":
                            return "adapterDynamicData";
                        case "AdapterId":
                            return "adapterId";
                        case "CouponGroupId":
                            return "couponGroupId";
                        case "Id":
                            return "id";
                        case "IntervalSeconds":
                            return "intervalSeconds";
                        case "SendTimeInSeconds":
                            return "sendTimeInSeconds";
                        case "TotalNumberOfRecipients":
                            return "totalNumberOfRecipients";
                        case "Type":
                            return "type";
                        case "UserList":
                            return "userList";
                    }
                    break;
                    
                case "KalturaEngagementAdapter":
                    switch(property.Name)
                    {
                        case "AdapterUrl":
                            return "adapterUrl";
                        case "IsActive":
                            return "isActive";
                        case "ProviderUrl":
                            return "providerUrl";
                        case "Settings":
                            return "engagementAdapterSettings";
                        case "SharedSecret":
                            return "sharedSecret";
                    }
                    break;
                    
                case "KalturaEngagementAdapterBase":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                    }
                    break;
                    
                case "KalturaEngagementAdapterListResponse":
                    switch(property.Name)
                    {
                        case "EngagementAdapters":
                            return "objects";
                    }
                    break;
                    
                case "KalturaEngagementFilter":
                    switch(property.Name)
                    {
                        case "SendTimeGreaterThanOrEqual":
                            return "sendTimeGreaterThanOrEqual";
                        case "TypeIn":
                            return "typeIn";
                    }
                    break;
                    
                case "KalturaEngagementListResponse":
                    switch(property.Name)
                    {
                        case "Engagements":
                            return "objects";
                    }
                    break;
                    
                case "KalturaEntitlement":
                    switch(property.Name)
                    {
                        case "CurrentDate":
                            return "currentDate";
                        case "CurrentUses":
                            return "currentUses";
                        case "DeviceName":
                            return "deviceName";
                        case "DeviceUDID":
                            return "deviceUdid";
                        case "EndDate":
                            return "endDate";
                        case "EntitlementId":
                            return "entitlementId";
                        case "HouseholdId":
                            return "householdId";
                        case "Id":
                            return "id";
                        case "IsCancelationWindowEnabled":
                            return "isCancelationWindowEnabled";
                        case "IsInGracePeriod":
                            return "isInGracePeriod";
                        case "IsRenewable":
                            return "isRenewable";
                        case "IsRenewableForPurchase":
                            return "isRenewableForPurchase";
                        case "LastViewDate":
                            return "lastViewDate";
                        case "MaxUses":
                            return "maxUses";
                        case "MediaFileId":
                            return "mediaFileId";
                        case "MediaId":
                            return "mediaId";
                        case "NextRenewalDate":
                            return "nextRenewalDate";
                        case "PaymentMethod":
                            return "paymentMethod";
                        case "ProductId":
                            return "productId";
                        case "PurchaseDate":
                            return "purchaseDate";
                        case "PurchaseId":
                            return "purchaseId";
                        case "Type":
                            return "type";
                        case "UserId":
                            return "userId";
                    }
                    break;
                    
                case "KalturaEntitlementCancellation":
                    switch(property.Name)
                    {
                        case "HouseholdId":
                            return "householdId";
                        case "Id":
                            return "id";
                        case "ProductId":
                            return "productId";
                        case "Type":
                            return "type";
                        case "UserId":
                            return "userId";
                    }
                    break;
                    
                case "KalturaEntitlementFilter":
                    switch(property.Name)
                    {
                        case "EntitlementTypeEqual":
                            return "entitlementTypeEqual";
                        case "EntityReferenceEqual":
                            return "entityReferenceEqual";
                        case "IsExpiredEqual":
                            return "isExpiredEqual";
                        case "ProductTypeEqual":
                            return "productTypeEqual";
                    }
                    break;
                    
                case "KalturaEntitlementListResponse":
                    switch(property.Name)
                    {
                        case "Entitlements":
                            return "objects";
                    }
                    break;
                    
                case "KalturaEntitlementRenewal":
                    switch(property.Name)
                    {
                        case "Date":
                            return "date";
                        case "Price":
                            return "price";
                        case "PurchaseId":
                            return "purchaseId";
                        case "SubscriptionId":
                            return "subscriptionId";
                    }
                    break;
                    
                case "KalturaEntitlementRenewalBase":
                    switch(property.Name)
                    {
                        case "Price":
                            return "price";
                        case "PurchaseId":
                            return "purchaseId";
                        case "SubscriptionId":
                            return "subscriptionId";
                    }
                    break;
                    
                case "KalturaEntitlementsFilter":
                    switch(property.Name)
                    {
                        case "By":
                            return "by";
                        case "EntitlementType":
                            return "entitlementType";
                    }
                    break;
                    
                case "KalturaEPGChannelAssets":
                    switch(property.Name)
                    {
                        case "Assets":
                            return "objects";
                        case "ChannelID":
                            return "channelId";
                    }
                    break;
                    
                case "KalturaEPGChannelAssetsListResponse":
                    switch(property.Name)
                    {
                        case "Channels":
                            return "objects";
                    }
                    break;
                    
                case "KalturaEpgChannelFilter":
                    switch(property.Name)
                    {
                        case "EndTime":
                            return "endTime";
                        case "IDs":
                            return "ids";
                        case "StartTime":
                            return "startTime";
                    }
                    break;
                    
                case "KalturaExportTask":
                    switch(property.Name)
                    {
                        case "Alias":
                            return "alias";
                        case "DataType":
                            return "dataType";
                        case "ExportType":
                            return "exportType";
                        case "Filter":
                            return "filter";
                        case "Frequency":
                            return "frequency";
                        case "Id":
                            return "id";
                        case "IsActive":
                            return "isActive";
                        case "Name":
                            return "name";
                        case "NotificationUrl":
                            return "notificationUrl";
                        case "VodTypes":
                            return "vodTypes";
                    }
                    break;
                    
                case "KalturaExportTaskFilter":
                    switch(property.Name)
                    {
                        case "IdIn":
                            return "idIn";
                    }
                    break;
                    
                case "KalturaExportTaskListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaExternalChannelProfile":
                    switch(property.Name)
                    {
                        case "Enrichments":
                            return "enrichments";
                        case "ExternalIdentifier":
                            return "externalIdentifier";
                        case "FilterExpression":
                            return "filterExpression";
                        case "Id":
                            return "id";
                        case "IsActive":
                            return "isActive";
                        case "Name":
                            return "name";
                        case "RecommendationEngineId":
                            return "recommendationEngineId";
                    }
                    break;
                    
                case "KalturaExternalChannelProfileListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaExternalReceipt":
                    switch(property.Name)
                    {
                        case "PaymentGatewayName":
                            return "paymentGatewayName";
                        case "ReceiptId":
                            return "receiptId";
                    }
                    break;
                    
                case "KalturaExternalRecording":
                    switch(property.Name)
                    {
                        case "ExternalId":
                            return "externalId";
                    }
                    break;
                    
                case "KalturaFacebookPost":
                    switch(property.Name)
                    {
                        case "Comments":
                            return "comments";
                        case "Link":
                            return "link";
                    }
                    break;
                    
                case "KalturaFairPlayPlaybackPluginData":
                    switch(property.Name)
                    {
                        case "Certificate":
                            return "certificate";
                    }
                    break;
                    
                case "KalturaFavorite":
                    switch(property.Name)
                    {
                        case "Asset":
                            return "asset";
                        case "AssetId":
                            return "assetId";
                        case "CreateDate":
                            return "createDate";
                        case "ExtraData":
                            return "extraData";
                    }
                    break;
                    
                case "KalturaFavoriteFilter":
                    switch(property.Name)
                    {
                        case "MediaIdIn":
                            return "mediaIdIn";
                        case "MediaIds":
                            return "media_ids";
                        case "MediaTypeEqual":
                            return "mediaTypeEqual";
                        case "MediaTypeIn":
                            return "mediaTypeIn";
                        case "UDID":
                            return "udid";
                        case "UdidEqualCurrent":
                            return "udidEqualCurrent";
                    }
                    break;
                    
                case "KalturaFavoriteListResponse":
                    switch(property.Name)
                    {
                        case "Favorites":
                            return "objects";
                    }
                    break;
                    
                case "KalturaFeed":
                    switch(property.Name)
                    {
                        case "AssetId":
                            return "assetId";
                    }
                    break;
                    
                case "KalturaFilter`1":
                    switch(property.Name)
                    {
                        case "OrderBy":
                            return "orderBy";
                    }
                    break;
                    
                case "KalturaFilterPager":
                    switch(property.Name)
                    {
                        case "PageIndex":
                            return "pageIndex";
                        case "PageSize":
                            return "pageSize";
                    }
                    break;
                    
                case "KalturaFollowDataBase":
                    switch(property.Name)
                    {
                        case "AnnouncementId":
                            return "announcementId";
                        case "FollowPhrase":
                            return "followPhrase";
                        case "Status":
                            return "status";
                        case "Timestamp":
                            return "timestamp";
                        case "Title":
                            return "title";
                    }
                    break;
                    
                case "KalturaFollowDataTvSeries":
                    switch(property.Name)
                    {
                        case "AssetId":
                            return "assetId";
                    }
                    break;
                    
                case "KalturaFollowTvSeries":
                    switch(property.Name)
                    {
                        case "AssetId":
                            return "assetId";
                    }
                    break;
                    
                case "KalturaFollowTvSeriesListResponse":
                    switch(property.Name)
                    {
                        case "FollowDataList":
                            return "objects";
                    }
                    break;
                    
                case "KalturaGenericListResponse`1":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaGenericRule":
                    switch(property.Name)
                    {
                        case "Description":
                            return "description";
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "RuleType":
                            return "ruleType";
                    }
                    break;
                    
                case "KalturaGenericRuleFilter":
                    switch(property.Name)
                    {
                        case "AssetId":
                            return "assetId";
                        case "AssetType":
                            return "assetType";
                    }
                    break;
                    
                case "KalturaGenericRuleListResponse":
                    switch(property.Name)
                    {
                        case "GenericRules":
                            return "objects";
                    }
                    break;
                    
                case "KalturaGroupPermission":
                    switch(property.Name)
                    {
                        case "Group":
                            return "group";
                    }
                    break;
                    
                case "KalturaHomeNetwork":
                    switch(property.Name)
                    {
                        case "Description":
                            return "description";
                        case "ExternalId":
                            return "externalId";
                        case "IsActive":
                            return "isActive";
                        case "Name":
                            return "name";
                    }
                    break;
                    
                case "KalturaHomeNetworkListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaHousehold":
                    switch(property.Name)
                    {
                        case "ConcurrentLimit":
                            return "concurrentLimit";
                        case "DefaultUsers":
                            return "defaultUsers";
                        case "Description":
                            return "description";
                        case "DeviceFamilies":
                            return "deviceFamilies";
                        case "DevicesLimit":
                            return "devicesLimit";
                        case "ExternalId":
                            return "externalId";
                        case "FrequencyNextDeviceAction":
                            return "frequencyNextDeviceAction";
                        case "FrequencyNextUserAction":
                            return "frequencyNextUserAction";
                        case "HouseholdLimitationsId":
                            return "householdLimitationsId";
                        case "Id":
                            return "id";
                        case "IsFrequencyEnabled":
                            return "isFrequencyEnabled";
                        case "MasterUsers":
                            return "masterUsers";
                        case "Name":
                            return "name";
                        case "PendingUsers":
                            return "pendingUsers";
                        case "RegionId":
                            return "regionId";
                        case "Restriction":
                            return "restriction";
                        case "RoleId":
                            return "roleId";
                        case "State":
                            return "state";
                        case "Users":
                            return "users";
                        case "UsersLimit":
                            return "usersLimit";
                    }
                    break;
                    
                case "KalturaHouseholdDevice":
                    switch(property.Name)
                    {
                        case "ActivatedOn":
                            return "activatedOn";
                        case "Brand":
                            return "brand";
                        case "BrandId":
                            return "brandId";
                        case "DeviceFamilyId":
                            return "deviceFamilyId";
                        case "Drm":
                            return "drm";
                        case "HouseholdId":
                            return "householdId";
                        case "Name":
                            return "name";
                        case "State":
                            return "state";
                        case "Status":
                            return "status";
                        case "Udid":
                            return "udid";
                    }
                    break;
                    
                case "KalturaHouseholdDeviceFamilyLimitations":
                    switch(property.Name)
                    {
                        case "ConcurrentLimit":
                            return "concurrentLimit";
                        case "DeviceLimit":
                            return "deviceLimit";
                        case "Frequency":
                            return "frequency";
                    }
                    break;
                    
                case "KalturaHouseholdDeviceFilter":
                    switch(property.Name)
                    {
                        case "DeviceFamilyIdIn":
                            return "deviceFamilyIdIn";
                        case "HouseholdIdEqual":
                            return "householdIdEqual";
                    }
                    break;
                    
                case "KalturaHouseholdDeviceListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaHouseholdLimitations":
                    switch(property.Name)
                    {
                        case "ConcurrentLimit":
                            return "concurrentLimit";
                        case "DeviceFamiliesLimitations":
                            return "deviceFamiliesLimitations";
                        case "DeviceFrequency":
                            return "deviceFrequency";
                        case "DeviceFrequencyDescription":
                            return "deviceFrequencyDescription";
                        case "DeviceLimit":
                            return "deviceLimit";
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "NpvrQuotaInSeconds":
                            return "npvrQuotaInSeconds";
                        case "UserFrequency":
                            return "userFrequency";
                        case "UserFrequencyDescription":
                            return "userFrequencyDescription";
                        case "UsersLimit":
                            return "usersLimit";
                    }
                    break;
                    
                case "KalturaHouseholdPaymentGateway":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "IsDefault":
                            return "isDefault";
                        case "Name":
                            return "name";
                    }
                    break;
                    
                case "KalturaHouseholdPaymentGatewayListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaHouseholdPaymentMethod":
                    switch(property.Name)
                    {
                        case "AllowMultiInstance":
                            return "allowMultiInstance";
                        case "Details":
                            return "details";
                        case "ExternalId":
                            return "externalId";
                        case "Id":
                            return "id";
                        case "IsDefault":
                            return "isDefault";
                        case "Name":
                            return "name";
                        case "PaymentGatewayId":
                            return "paymentGatewayId";
                        case "PaymentMethodProfileId":
                            return "paymentMethodProfileId";
                        case "Selected":
                            return "selected";
                    }
                    break;
                    
                case "KalturaHouseholdPaymentMethodListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaHouseholdPremiumServiceListResponse":
                    switch(property.Name)
                    {
                        case "PremiumServices":
                            return "objects";
                    }
                    break;
                    
                case "KalturaHouseholdQuota":
                    switch(property.Name)
                    {
                        case "AvailableQuota":
                            return "availableQuota";
                        case "HouseholdId":
                            return "householdId";
                        case "TotalQuota":
                            return "totalQuota";
                    }
                    break;
                    
                case "KalturaHouseholdUser":
                    switch(property.Name)
                    {
                        case "HouseholdId":
                            return "householdId";
                        case "HouseholdMasterUsername":
                            return "householdMasterUsername";
                        case "IsDefault":
                            return "isDefault";
                        case "IsMaster":
                            return "isMaster";
                        case "Status":
                            return "status";
                        case "UserId":
                            return "userId";
                    }
                    break;
                    
                case "KalturaHouseholdUserFilter":
                    switch(property.Name)
                    {
                        case "HouseholdIdEqual":
                            return "householdIdEqual";
                    }
                    break;
                    
                case "KalturaHouseholdUserListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaIdentifierTypeFilter":
                    switch(property.Name)
                    {
                        case "By":
                            return "by";
                        case "Identifier":
                            return "identifier";
                    }
                    break;
                    
                case "KalturaImage":
                    switch(property.Name)
                    {
                        case "ContentId":
                            return "contentId";
                        case "Id":
                            return "id";
                        case "ImageObjectId":
                            return "imageObjectId";
                        case "ImageObjectType":
                            return "imageObjectType";
                        case "ImageTypeId":
                            return "imageTypeId";
                        case "IsDefault":
                            return "isDefault";
                        case "Status":
                            return "status";
                        case "Url":
                            return "url";
                        case "Version":
                            return "version";
                    }
                    break;
                    
                case "KalturaImageFilter":
                    switch(property.Name)
                    {
                        case "IdIn":
                            return "idIn";
                        case "ImageObjectIdEqual":
                            return "imageObjectIdEqual";
                        case "ImageObjectTypeEqual":
                            return "imageObjectTypeEqual";
                        case "IsDefaultEqual":
                            return "isDefaultEqual";
                    }
                    break;
                    
                case "KalturaImageListResponse":
                    switch(property.Name)
                    {
                        case "Images":
                            return "objects";
                    }
                    break;
                    
                case "KalturaImageType":
                    switch(property.Name)
                    {
                        case "DefaultImageId":
                            return "defaultImageId";
                        case "HelpText":
                            return "helpText";
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "RatioId":
                            return "ratioId";
                        case "SystemName":
                            return "systemName";
                    }
                    break;
                    
                case "KalturaImageTypeFilter":
                    switch(property.Name)
                    {
                        case "IdIn":
                            return "idIn";
                        case "RatioIdIn":
                            return "ratioIdIn";
                    }
                    break;
                    
                case "KalturaImageTypeListResponse":
                    switch(property.Name)
                    {
                        case "ImageTypes":
                            return "objects";
                    }
                    break;
                    
                case "KalturaInboxMessage":
                    switch(property.Name)
                    {
                        case "CreatedAt":
                            return "createdAt";
                        case "Id":
                            return "id";
                        case "Message":
                            return "message";
                        case "Status":
                            return "status";
                        case "Type":
                            return "type";
                        case "Url":
                            return "url";
                    }
                    break;
                    
                case "KalturaInboxMessageFilter":
                    switch(property.Name)
                    {
                        case "CreatedAtGreaterThanOrEqual":
                            return "createdAtGreaterThanOrEqual";
                        case "CreatedAtLessThanOrEqual":
                            return "createdAtLessThanOrEqual";
                        case "TypeIn":
                            return "typeIn";
                    }
                    break;
                    
                case "KalturaInboxMessageListResponse":
                    switch(property.Name)
                    {
                        case "InboxMessages":
                            return "objects";
                    }
                    break;
                    
                case "KalturaInboxMessageResponse":
                    switch(property.Name)
                    {
                        case "InboxMessages":
                            return "objects";
                    }
                    break;
                    
                case "KalturaIntegerValueListResponse":
                    switch(property.Name)
                    {
                        case "Values":
                            return "objects";
                    }
                    break;
                    
                case "KalturaIpRangeCondition":
                    switch(property.Name)
                    {
                        case "FromIP":
                            return "fromIP";
                        case "ToIP":
                            return "toIP";
                    }
                    break;
                    
                case "KalturaItemPrice":
                    switch(property.Name)
                    {
                        case "FileId":
                            return "fileId";
                        case "PPVPriceDetails":
                            return "ppvPriceDetails";
                    }
                    break;
                    
                case "KalturaItemPriceListResponse":
                    switch(property.Name)
                    {
                        case "ItemPrice":
                            return "objects";
                    }
                    break;
                    
                case "KalturaLanguage":
                    switch(property.Name)
                    {
                        case "Code":
                            return "code";
                        case "Direction":
                            return "direction";
                        case "IsDefault":
                            return "isDefault";
                        case "Name":
                            return "name";
                        case "SystemName":
                            return "systemName";
                    }
                    break;
                    
                case "KalturaLanguageFilter":
                    switch(property.Name)
                    {
                        case "CodeIn":
                            return "codeIn";
                    }
                    break;
                    
                case "KalturaLanguageListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaLastPosition":
                    switch(property.Name)
                    {
                        case "Position":
                            return "position";
                        case "PositionOwner":
                            return "position_owner";
                        case "UserId":
                            return "user_id";
                    }
                    break;
                    
                case "KalturaLastPositionFilter":
                    switch(property.Name)
                    {
                        case "By":
                            return "by";
                        case "Ids":
                            return "ids";
                        case "Type":
                            return "type";
                    }
                    break;
                    
                case "KalturaLastPositionListResponse":
                    switch(property.Name)
                    {
                        case "LastPositions":
                            return "objects";
                    }
                    break;
                    
                case "KalturaLicensedUrl":
                    switch(property.Name)
                    {
                        case "AltUrl":
                            return "altUrl";
                        case "MainUrl":
                            return "mainUrl";
                    }
                    break;
                    
                case "KalturaLicensedUrlBaseRequest":
                    switch(property.Name)
                    {
                        case "AssetId":
                            return "assetId";
                    }
                    break;
                    
                case "KalturaLicensedUrlEpgRequest":
                    switch(property.Name)
                    {
                        case "StartDate":
                            return "startDate";
                        case "StreamType":
                            return "streamType";
                    }
                    break;
                    
                case "KalturaLicensedUrlMediaRequest":
                    switch(property.Name)
                    {
                        case "BaseUrl":
                            return "baseUrl";
                        case "ContentId":
                            return "contentId";
                    }
                    break;
                    
                case "KalturaLicensedUrlRecordingRequest":
                    switch(property.Name)
                    {
                        case "FileType":
                            return "fileType";
                    }
                    break;
                    
                case "KalturaListFollowDataTvSeriesResponse":
                    switch(property.Name)
                    {
                        case "FollowDataList":
                            return "objects";
                    }
                    break;
                    
                case "KalturaListResponse":
                    switch(property.Name)
                    {
                        case "TotalCount":
                            return "totalCount";
                    }
                    break;
                    
                case "KalturaLiveAsset":
                    switch(property.Name)
                    {
                        case "BufferCatchUp":
                            return "bufferCatchUpSetting";
                        case "BufferTrickPlay":
                            return "bufferTrickPlaySetting";
                        case "CatchUpEnabled":
                            return "enableCatchUp";
                        case "CdvrEnabled":
                            return "enableCdvr";
                        case "ChannelType":
                            return "channelType";
                        case "EnableCatchUpState":
                            return "enableCatchUpState";
                        case "EnableCdvrState":
                            return "enableCdvrState";
                        case "EnableRecordingPlaybackNonEntitledChannelState":
                            return "enableRecordingPlaybackNonEntitledChannelState";
                        case "EnableStartOverState":
                            return "enableStartOverState";
                        case "EnableTrickPlayState":
                            return "enableTrickPlayState";
                        case "ExternalCdvrId":
                            return "externalCdvrId";
                        case "ExternalEpgIngestId":
                            return "externalEpgIngestId";
                        case "RecordingPlaybackNonEntitledChannelEnabled":
                            return "enableRecordingPlaybackNonEntitledChannel";
                        case "StartOverEnabled":
                            return "enableStartOver";
                        case "SummedCatchUpBuffer":
                            return "catchUpBuffer";
                        case "SummedTrickPlayBuffer":
                            return "trickPlayBuffer";
                        case "TrickPlayEnabled":
                            return "enableTrickPlay";
                    }
                    break;
                    
                case "KalturaLoginResponse":
                    switch(property.Name)
                    {
                        case "LoginSession":
                            return "loginSession";
                        case "User":
                            return "user";
                    }
                    break;
                    
                case "KalturaLoginSession":
                    switch(property.Name)
                    {
                        case "KS":
                            return "ks";
                        case "RefreshToken":
                            return "refreshToken";
                    }
                    break;
                    
                case "KalturaManualChannel":
                    switch(property.Name)
                    {
                        case "MediaIds":
                            return "mediaIds";
                    }
                    break;
                    
                case "KalturaMediaAsset":
                    switch(property.Name)
                    {
                        case "CatchUpBuffer":
                            return "catchUpBuffer";
                        case "DeviceRule":
                            return "deviceRule";
                        case "DeviceRuleId":
                            return "deviceRuleId";
                        case "EnableRecordingPlaybackNonEntitledChannel":
                            return "enableRecordingPlaybackNonEntitledChannel";
                        case "EntryId":
                            return "entryId";
                        case "ExternalIds":
                            return "externalIds";
                        case "GeoBlockRule":
                            return "geoBlockRule";
                        case "GeoBlockRuleId":
                            return "geoBlockRuleId";
                        case "Status":
                            return "status";
                        case "TrickPlayBuffer":
                            return "trickPlayBuffer";
                        case "TypeDescription":
                            return "typeDescription";
                        case "WatchPermissionRule":
                            return "watchPermissionRule";
                    }
                    break;
                    
                case "KalturaMediaConcurrencyRule":
                    switch(property.Name)
                    {
                        case "ConcurrencyLimitationType":
                            return "concurrencyLimitationType";
                        case "Id":
                            return "id";
                        case "Limitation":
                            return "limitation";
                        case "Name":
                            return "name";
                    }
                    break;
                    
                case "KalturaMediaConcurrencyRuleListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaMediaFile":
                    switch(property.Name)
                    {
                        case "AdditionalData":
                            return "additionalData";
                        case "AltCdnCode":
                            return "altCdnCode";
                        case "AlternativeCdnAdapaterProfileId":
                            return "alternativeCdnAdapaterProfileId";
                        case "AltExternalId":
                            return "altExternalId";
                        case "AltStreamingCode":
                            return "altStreamingCode";
                        case "AssetId":
                            return "assetId";
                        case "BillingType":
                            return "billingType";
                        case "CdnAdapaterProfileId":
                            return "cdnAdapaterProfileId";
                        case "CdnCode":
                            return "cdnCode";
                        case "CdnName":
                            return "cdnName";
                        case "Duration":
                            return "duration";
                        case "EndDate":
                            return "endDate";
                        case "ExternalId":
                            return "externalId";
                        case "ExternalStoreId":
                            return "externalStoreId";
                        case "FileSize":
                            return "fileSize";
                        case "HandlingType":
                            return "handlingType";
                        case "Id":
                            return "id";
                        case "IsDefaultLanguage":
                            return "isDefaultLanguage";
                        case "Language":
                            return "language";
                        case "OrderNum":
                            return "orderNum";
                        case "OutputProtecationLevel":
                            return "outputProtecationLevel";
                        case "PPVModules":
                            return "ppvModules";
                        case "ProductCode":
                            return "productCode";
                        case "Quality":
                            return "quality";
                        case "StartDate":
                            return "startDate";
                        case "Status":
                            return "status";
                        case "Type":
                            return "type";
                        case "TypeId":
                            return "typeId";
                        case "Url":
                            return "url";
                    }
                    break;
                    
                case "KalturaMediaFileFilter":
                    switch(property.Name)
                    {
                        case "AssetIdEqual":
                            return "assetIdEqual";
                        case "IdEqual":
                            return "idEqual";
                    }
                    break;
                    
                case "KalturaMediaFileListResponse":
                    switch(property.Name)
                    {
                        case "Files":
                            return "objects";
                    }
                    break;
                    
                case "KalturaMediaFileType":
                    switch(property.Name)
                    {
                        case "AudioCodecs":
                            return "audioCodecs";
                        case "CreateDate":
                            return "createDate";
                        case "Description":
                            return "description";
                        case "DrmProfileId":
                            return "drmProfileId";
                        case "Id":
                            return "id";
                        case "IsTrailer":
                            return "isTrailer";
                        case "Name":
                            return "name";
                        case "Quality":
                            return "quality";
                        case "Status":
                            return "status";
                        case "StreamerType":
                            return "streamerType";
                        case "UpdateDate":
                            return "updateDate";
                        case "VideoCodecs":
                            return "videoCodecs";
                    }
                    break;
                    
                case "KalturaMediaFileTypeListResponse":
                    switch(property.Name)
                    {
                        case "Types":
                            return "objects";
                    }
                    break;
                    
                case "KalturaMediaImage":
                    switch(property.Name)
                    {
                        case "Height":
                            return "height";
                        case "Id":
                            return "id";
                        case "IsDefault":
                            return "isDefault";
                        case "Ratio":
                            return "ratio";
                        case "Url":
                            return "url";
                        case "Version":
                            return "version";
                        case "Width":
                            return "width";
                    }
                    break;
                    
                case "KalturaMessageAnnouncementListResponse":
                    switch(property.Name)
                    {
                        case "Announcements":
                            return "objects";
                    }
                    break;
                    
                case "KalturaMessageTemplate":
                    switch(property.Name)
                    {
                        case "Action":
                            return "action";
                        case "DateFormat":
                            return "dateFormat";
                        case "MailSubject":
                            return "mailSubject";
                        case "MailTemplate":
                            return "mailTemplate";
                        case "Message":
                            return "message";
                        case "MessageType":
                            return "messageType";
                        case "RatioId":
                            return "ratioId";
                        case "Sound":
                            return "sound";
                        case "URL":
                            return "url";
                    }
                    break;
                    
                case "KalturaMeta":
                    switch(property.Name)
                    {
                        case "AssetType":
                            return "assetType";
                        case "CreateDate":
                            return "createDate";
                        case "DataType":
                            return "dataType";
                        case "Features":
                            return "features";
                        case "FieldName":
                            return "fieldName";
                        case "HelpText":
                            return "helpText";
                        case "Id":
                            return "id";
                        case "IsProtected":
                            return "isProtected";
                        case "MultipleValue":
                            return "multipleValue";
                        case "Name":
                            return "name";
                        case "ParentId":
                            return "parentId";
                        case "PartnerId":
                            return "partnerId";
                        case "SystemName":
                            return "systemName";
                        case "Type":
                            return "type";
                        case "UpdateDate":
                            return "updateDate";
                    }
                    break;
                    
                case "KalturaMetaFilter":
                    switch(property.Name)
                    {
                        case "AssetStructIdEqual":
                            return "assetStructIdEqual";
                        case "AssetTypeEqual":
                            return "assetTypeEqual";
                        case "DataTypeEqual":
                            return "dataTypeEqual";
                        case "FeaturesIn":
                            return "featuresIn";
                        case "FieldNameEqual":
                            return "fieldNameEqual";
                        case "FieldNameNotEqual":
                            return "fieldNameNotEqual";
                        case "IdIn":
                            return "idIn";
                        case "MultipleValueEqual":
                            return "multipleValueEqual";
                        case "TypeEqual":
                            return "typeEqual";
                    }
                    break;
                    
                case "KalturaMetaListResponse":
                    switch(property.Name)
                    {
                        case "Metas":
                            return "objects";
                    }
                    break;
                    
                case "KalturaMultilingualString":
                    switch(property.Name)
                    {
                        case "Values":
                            return "values";
                    }
                    break;
                    
                case "KalturaMultilingualStringValueArray":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaNetworkActionStatus":
                    switch(property.Name)
                    {
                        case "Network":
                            return "network";
                        case "Status":
                            return "status";
                    }
                    break;
                    
                case "KalturaNotification":
                    switch(property.Name)
                    {
                        case "eventObject":
                            return "object";
                    }
                    break;
                    
                case "KalturaNotificationsPartnerSettings":
                    switch(property.Name)
                    {
                        case "AutomaticIssueFollowNotification":
                            return "automaticIssueFollowNotification";
                        case "ChurnMailSubject":
                            return "churnMailSubject";
                        case "ChurnMailTemplateName":
                            return "churnMailTemplateName";
                        case "InboxEnabled":
                            return "inboxEnabled";
                        case "MailNotificationAdapterId":
                            return "mailNotificationAdapterId";
                        case "MailSenderName":
                            return "mailSenderName";
                        case "MessageTTLDays":
                            return "messageTTLDays";
                        case "PushAdapterUrl":
                            return "pushAdapterUrl";
                        case "PushEndHour":
                            return "pushEndHour";
                        case "PushNotificationEnabled":
                            return "pushNotificationEnabled";
                        case "PushStartHour":
                            return "pushStartHour";
                        case "PushSystemAnnouncementsEnabled":
                            return "pushSystemAnnouncementsEnabled";
                        case "ReminderEnabled":
                            return "reminderEnabled";
                        case "ReminderOffset":
                            return "reminderOffsetSec";
                        case "SenderEmail":
                            return "senderEmail";
                        case "SmsEnabled":
                            return "smsEnabled";
                        case "TopicExpirationDurationDays":
                            return "topicExpirationDurationDays";
                    }
                    break;
                    
                case "KalturaNotificationsSettings":
                    switch(property.Name)
                    {
                        case "MailEnabled":
                            return "mailEnabled";
                        case "PushFollowEnabled":
                            return "pushFollowEnabled";
                        case "PushNotificationEnabled":
                            return "pushNotificationEnabled";
                        case "SmsEnabled":
                            return "smsEnabled";
                    }
                    break;
                    
                case "KalturaNpvrPremiumService":
                    switch(property.Name)
                    {
                        case "QuotaInMinutes":
                            return "quotaInMinutes";
                    }
                    break;
                    
                case "KalturaOSSAdapterBaseProfile":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                    }
                    break;
                    
                case "KalturaOSSAdapterProfile":
                    switch(property.Name)
                    {
                        case "AdapterUrl":
                            return "adapterUrl";
                        case "ExternalIdentifier":
                            return "externalIdentifier";
                        case "IsActive":
                            return "isActive";
                        case "Settings":
                            return "ossAdapterSettings";
                        case "SharedSecret":
                            return "sharedSecret";
                    }
                    break;
                    
                case "KalturaOSSAdapterProfileListResponse":
                    switch(property.Name)
                    {
                        case "OSSAdapterProfiles":
                            return "objects";
                    }
                    break;
                    
                case "KalturaOTTCategory":
                    switch(property.Name)
                    {
                        case "Channels":
                            return "channels";
                        case "ChildCategories":
                            return "childCategories";
                        case "Id":
                            return "id";
                        case "Images":
                            return "images";
                        case "Name":
                            return "name";
                        case "ParentCategoryId":
                            return "parentCategoryId";
                    }
                    break;
                    
                case "KalturaOTTUser":
                    switch(property.Name)
                    {
                        case "Address":
                            return "address";
                        case "AffiliateCode":
                            return "affiliateCode";
                        case "City":
                            return "city";
                        case "Country":
                            return "country";
                        case "CountryId":
                            return "countryId";
                        case "CreateDate":
                            return "createDate";
                        case "DynamicData":
                            return "dynamicData";
                        case "Email":
                            return "email";
                        case "ExternalId":
                            return "externalId";
                        case "FacebookId":
                            return "facebookId";
                        case "FacebookImage":
                            return "facebookImage";
                        case "FacebookToken":
                            return "facebookToken";
                        case "HouseholdID":
                            return "householdId";
                        case "IsHouseholdMaster":
                            return "isHouseholdMaster";
                        case "Phone":
                            return "phone";
                        case "RoleIds":
                            return "roleIds";
                        case "SuspensionState":
                            return "suspensionState";
                        case "SuspentionState":
                            return "suspentionState";
                        case "UpdateDate":
                            return "updateDate";
                        case "UserState":
                            return "userState";
                        case "UserType":
                            return "userType";
                        case "Zip":
                            return "zip";
                    }
                    break;
                    
                case "KalturaOTTUserDynamicData":
                    switch(property.Name)
                    {
                        case "Key":
                            return "key";
                        case "UserId":
                            return "userId";
                        case "Value":
                            return "value";
                    }
                    break;
                    
                case "KalturaOTTUserDynamicDataList":
                    switch(property.Name)
                    {
                        case "DynamicData":
                            return "dynamicData";
                        case "UserId":
                            return "userId";
                    }
                    break;
                    
                case "KalturaOTTUserFilter":
                    switch(property.Name)
                    {
                        case "ExternalIdEqual":
                            return "externalIdEqual";
                        case "IdIn":
                            return "idIn";
                        case "RoleIdsIn":
                            return "roleIdsIn";
                        case "UsernameEqual":
                            return "usernameEqual";
                    }
                    break;
                    
                case "KalturaOTTUserListResponse":
                    switch(property.Name)
                    {
                        case "Users":
                            return "objects";
                    }
                    break;
                    
                case "KalturaOTTUserType":
                    switch(property.Name)
                    {
                        case "Description":
                            return "description";
                        case "Id":
                            return "id";
                    }
                    break;
                    
                case "KalturaParentalRule":
                    switch(property.Name)
                    {
                        case "CreateDate":
                            return "createDate";
                        case "epgTagTypeId":
                            return "epgTag";
                        case "IsActive":
                            return "isActive";
                        case "mediaTagTypeId":
                            return "mediaTag";
                        case "Origin":
                            return "origin";
                        case "UpdateDate":
                            return "updateDate";
                    }
                    break;
                    
                case "KalturaParentalRuleFilter":
                    switch(property.Name)
                    {
                        case "EntityReferenceEqual":
                            return "entityReferenceEqual";
                    }
                    break;
                    
                case "KalturaParentalRuleListResponse":
                    switch(property.Name)
                    {
                        case "ParentalRule":
                            return "objects";
                    }
                    break;
                    
                case "KalturaPartnerConfigurationFilter":
                    switch(property.Name)
                    {
                        case "PartnerConfigurationTypeEqual":
                            return "partnerConfigurationTypeEqual";
                    }
                    break;
                    
                case "KalturaPartnerConfigurationListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaPaymentGateway":
                    switch(property.Name)
                    {
                        case "paymentGateway":
                            return "payment_gateway";
                        case "selectedBy":
                            return "selected_by";
                    }
                    break;
                    
                case "KalturaPaymentGatewayBaseProfile":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "IsDefault":
                            return "isDefault";
                        case "Name":
                            return "name";
                        case "PaymentMethods":
                            return "paymentMethods";
                    }
                    break;
                    
                case "KalturaPaymentGatewayConfiguration":
                    switch(property.Name)
                    {
                        case "Configuration":
                            return "paymentGatewayConfiguration";
                    }
                    break;
                    
                case "KalturaPaymentGatewayProfile":
                    switch(property.Name)
                    {
                        case "AdapterUrl":
                            return "adapterUrl";
                        case "ExternalIdentifier":
                            return "externalIdentifier";
                        case "ExternalVerification":
                            return "externalVerification";
                        case "IsActive":
                            return "isActive";
                        case "PendingInterval":
                            return "pendingInterval";
                        case "PendingRetries":
                            return "pendingRetries";
                        case "RenewIntervalMinutes":
                            return "renewIntervalMinutes";
                        case "RenewStartMinutes":
                            return "renewStartMinutes";
                        case "RenewUrl":
                            return "renewUrl";
                        case "Settings":
                            return "paymentGatewaySettings";
                        case "SharedSecret":
                            return "sharedSecret";
                        case "StatusUrl":
                            return "statusUrl";
                        case "TransactUrl":
                            return "transactUrl";
                    }
                    break;
                    
                case "KalturaPaymentGatewayProfileListResponse":
                    switch(property.Name)
                    {
                        case "PaymentGatewayProfiles":
                            return "objects";
                    }
                    break;
                    
                case "KalturaPaymentMethod":
                    switch(property.Name)
                    {
                        case "AllowMultiInstance":
                            return "allowMultiInstance";
                        case "HouseholdPaymentMethods":
                            return "householdPaymentMethods";
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                    }
                    break;
                    
                case "KalturaPaymentMethodProfile":
                    switch(property.Name)
                    {
                        case "AllowMultiInstance":
                            return "allowMultiInstance";
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "PaymentGatewayId":
                            return "paymentGatewayId";
                    }
                    break;
                    
                case "KalturaPaymentMethodProfileFilter":
                    switch(property.Name)
                    {
                        case "PaymentGatewayIdEqual":
                            return "paymentGatewayIdEqual";
                    }
                    break;
                    
                case "KalturaPaymentMethodProfileListResponse":
                    switch(property.Name)
                    {
                        case "PaymentMethodProfiles":
                            return "objects";
                    }
                    break;
                    
                case "KalturaPermission":
                    switch(property.Name)
                    {
                        case "FriendlyName":
                            return "friendlyName";
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                    }
                    break;
                    
                case "KalturaPermissionFilter":
                    switch(property.Name)
                    {
                        case "CurrentUserPermissionsContains":
                            return "currentUserPermissionsContains";
                    }
                    break;
                    
                case "KalturaPermissionItem":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "IsExcluded":
                            return "isExcluded";
                        case "Name":
                            return "name";
                    }
                    break;
                    
                case "KalturaPermissionListResponse":
                    switch(property.Name)
                    {
                        case "Permissions":
                            return "objects";
                    }
                    break;
                    
                case "KalturaPersistedFilter`1":
                    switch(property.Name)
                    {
                        case "Name":
                            return "name";
                    }
                    break;
                    
                case "KalturaPersonalAsset":
                    switch(property.Name)
                    {
                        case "Bookmarks":
                            return "bookmarks";
                        case "Files":
                            return "files";
                        case "Following":
                            return "following";
                        case "Id":
                            return "id";
                        case "Type":
                            return "type";
                    }
                    break;
                    
                case "KalturaPersonalAssetListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaPersonalAssetRequest":
                    switch(property.Name)
                    {
                        case "FileIds":
                            return "fileIds";
                        case "Id":
                            return "id";
                        case "Type":
                            return "type";
                    }
                    break;
                    
                case "KalturaPersonalFeedListResponse":
                    switch(property.Name)
                    {
                        case "PersonalFollowFeed":
                            return "objects";
                    }
                    break;
                    
                case "KalturaPersonalFile":
                    switch(property.Name)
                    {
                        case "Discounted":
                            return "discounted";
                        case "Entitled":
                            return "entitled";
                        case "Id":
                            return "id";
                        case "Offer":
                            return "offer";
                    }
                    break;
                    
                case "KalturaPersonalFollowFeedResponse":
                    switch(property.Name)
                    {
                        case "PersonalFollowFeed":
                            return "objects";
                    }
                    break;
                    
                case "KalturaPersonalList":
                    switch(property.Name)
                    {
                        case "CreateDate":
                            return "createDate";
                        case "Id":
                            return "id";
                        case "Ksql":
                            return "ksql";
                        case "Name":
                            return "name";
                        case "PartnerListType":
                            return "partnerListType";
                    }
                    break;
                    
                case "KalturaPersonalListFilter":
                    switch(property.Name)
                    {
                        case "PartnerListTypeIn":
                            return "partnerListTypeIn";
                    }
                    break;
                    
                case "KalturaPersonalListListResponse":
                    switch(property.Name)
                    {
                        case "PersonalListList":
                            return "objects";
                    }
                    break;
                    
                case "KalturaPersonalListSearchFilter":
                    switch(property.Name)
                    {
                        case "PartnerListTypeIn":
                            return "partnerListTypeIn";
                    }
                    break;
                    
                case "KalturaPin":
                    switch(property.Name)
                    {
                        case "Origin":
                            return "origin";
                        case "PIN":
                            return "pin";
                        case "Type":
                            return "type";
                    }
                    break;
                    
                case "KalturaPinResponse":
                    switch(property.Name)
                    {
                        case "Origin":
                            return "origin";
                        case "PIN":
                            return "pin";
                        case "Type":
                            return "type";
                    }
                    break;
                    
                case "KalturaPlaybackContext":
                    switch(property.Name)
                    {
                        case "Actions":
                            return "actions";
                        case "Messages":
                            return "messages";
                        case "Sources":
                            return "sources";
                    }
                    break;
                    
                case "KalturaPlaybackContextOptions":
                    switch(property.Name)
                    {
                        case "AssetFileIds":
                            return "assetFileIds";
                        case "Context":
                            return "context";
                        case "MediaProtocol":
                            return "mediaProtocol";
                        case "StreamerType":
                            return "streamerType";
                        case "UrlType":
                            return "urlType";
                    }
                    break;
                    
                case "KalturaPlaybackSource":
                    switch(property.Name)
                    {
                        case "AdsParams":
                            return "adsParam";
                        case "AdsPolicy":
                            return "adsPolicy";
                        case "Drm":
                            return "drm";
                        case "Format":
                            return "format";
                        case "Protocols":
                            return "protocols";
                    }
                    break;
                    
                case "KalturaPlayerAssetData":
                    switch(property.Name)
                    {
                        case "averageBitRate":
                            return "averageBitrate";
                        case "currentBitRate":
                            return "currentBitrate";
                        case "totalBitRate":
                            return "totalBitrate";
                    }
                    break;
                    
                case "KalturaPpv":
                    switch(property.Name)
                    {
                        case "CouponsGroup":
                            return "couponsGroup";
                        case "Descriptions":
                            return "descriptions";
                        case "DiscountModule":
                            return "discountModule";
                        case "FileTypes":
                            return "fileTypes";
                        case "FirstDeviceLimitation":
                            return "firstDeviceLimitation";
                        case "Id":
                            return "id";
                        case "IsSubscriptionOnly":
                            return "isSubscriptionOnly";
                        case "Name":
                            return "name";
                        case "Price":
                            return "price";
                        case "ProductCode":
                            return "productCode";
                        case "UsageModule":
                            return "usageModule";
                    }
                    break;
                    
                case "KalturaPpvEntitlement":
                    switch(property.Name)
                    {
                        case "MediaFileId":
                            return "mediaFileId";
                        case "MediaId":
                            return "mediaId";
                    }
                    break;
                    
                case "KalturaPPVItemPriceDetails":
                    switch(property.Name)
                    {
                        case "CollectionId":
                            return "collectionId";
                        case "DiscountEndDate":
                            return "discountEndDate";
                        case "EndDate":
                            return "endDate";
                        case "FirstDeviceName":
                            return "firstDeviceName";
                        case "FullPrice":
                            return "fullPrice";
                        case "IsInCancelationPeriod":
                            return "isInCancelationPeriod";
                        case "IsSubscriptionOnly":
                            return "isSubscriptionOnly";
                        case "PPVDescriptions":
                            return "ppvDescriptions";
                        case "PPVModuleId":
                            return "ppvModuleId";
                        case "PrePaidId":
                            return "prePaidId";
                        case "Price":
                            return "price";
                        case "ProductCode":
                            return "ppvProductCode";
                        case "PurchasedMediaFileId":
                            return "purchasedMediaFileId";
                        case "PurchaseStatus":
                            return "purchaseStatus";
                        case "PurchaseUserId":
                            return "purchaseUserId";
                        case "RelatedMediaFileIds":
                            return "relatedMediaFileIds";
                        case "StartDate":
                            return "startDate";
                        case "SubscriptionId":
                            return "subscriptionId";
                    }
                    break;
                    
                case "KalturaPpvPrice":
                    switch(property.Name)
                    {
                        case "CollectionId":
                            return "collectionId";
                        case "DiscountEndDate":
                            return "discountEndDate";
                        case "EndDate":
                            return "endDate";
                        case "FileId":
                            return "fileId";
                        case "FirstDeviceName":
                            return "firstDeviceName";
                        case "FullPrice":
                            return "fullPrice";
                        case "IsInCancelationPeriod":
                            return "isInCancelationPeriod";
                        case "IsSubscriptionOnly":
                            return "isSubscriptionOnly";
                        case "PPVDescriptions":
                            return "ppvDescriptions";
                        case "PPVModuleId":
                            return "ppvModuleId";
                        case "PrePaidId":
                            return "prePaidId";
                        case "ProductCode":
                            return "ppvProductCode";
                        case "PurchasedMediaFileId":
                            return "purchasedMediaFileId";
                        case "PurchaseUserId":
                            return "purchaseUserId";
                        case "RelatedMediaFileIds":
                            return "relatedMediaFileIds";
                        case "StartDate":
                            return "startDate";
                        case "SubscriptionId":
                            return "subscriptionId";
                    }
                    break;
                    
                case "KalturaPremiumService":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                    }
                    break;
                    
                case "KalturaPreviewModule":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "LifeCycle":
                            return "lifeCycle";
                        case "Name":
                            return "name";
                        case "NonRenewablePeriod":
                            return "nonRenewablePeriod";
                    }
                    break;
                    
                case "KalturaPrice":
                    switch(property.Name)
                    {
                        case "Amount":
                            return "amount";
                        case "CountryId":
                            return "countryId";
                        case "Currency":
                            return "currency";
                        case "CurrencySign":
                            return "currencySign";
                    }
                    break;
                    
                case "KalturaPriceDetails":
                    switch(property.Name)
                    {
                        case "Descriptions":
                            return "descriptions";
                        case "Id":
                            return "id";
                        case "MultiCurrencyPrice":
                            return "multiCurrencyPrice";
                        case "Price":
                            return "price";
                    }
                    break;
                    
                case "KalturaPriceDetailsFilter":
                    switch(property.Name)
                    {
                        case "IdIn":
                            return "idIn";
                    }
                    break;
                    
                case "KalturaPriceDetailsListResponse":
                    switch(property.Name)
                    {
                        case "Prices":
                            return "objects";
                    }
                    break;
                    
                case "KalturaPricePlan":
                    switch(property.Name)
                    {
                        case "DiscountId":
                            return "discountId";
                        case "IsRenewable":
                            return "isRenewable";
                        case "PriceDetailsId":
                            return "priceDetailsId";
                        case "PriceId":
                            return "priceId";
                        case "RenewalsNumber":
                            return "renewalsNumber";
                    }
                    break;
                    
                case "KalturaPricePlanFilter":
                    switch(property.Name)
                    {
                        case "IdIn":
                            return "idIn";
                    }
                    break;
                    
                case "KalturaPricePlanListResponse":
                    switch(property.Name)
                    {
                        case "PricePlans":
                            return "objects";
                    }
                    break;
                    
                case "KalturaPricesFilter":
                    switch(property.Name)
                    {
                        case "FilesIds":
                            return "filesIds";
                        case "ShouldGetOnlyLowest":
                            return "shouldGetOnlyLowest";
                        case "SubscriptionsIds":
                            return "subscriptionsIds";
                    }
                    break;
                    
                case "KalturaProductCode":
                    switch(property.Name)
                    {
                        case "Code":
                            return "code";
                        case "InappProvider":
                            return "inappProvider";
                    }
                    break;
                    
                case "KalturaProductPrice":
                    switch(property.Name)
                    {
                        case "Price":
                            return "price";
                        case "ProductId":
                            return "productId";
                        case "ProductType":
                            return "productType";
                        case "PurchaseStatus":
                            return "purchaseStatus";
                    }
                    break;
                    
                case "KalturaProductPriceFilter":
                    switch(property.Name)
                    {
                        case "CollectionIdIn":
                            return "collectionIdIn";
                        case "CouponCodeEqual":
                            return "couponCodeEqual";
                        case "FileIdIn":
                            return "fileIdIn";
                        case "SubscriptionIdIn":
                            return "subscriptionIdIn";
                    }
                    break;
                    
                case "KalturaProductPriceListResponse":
                    switch(property.Name)
                    {
                        case "ProductsPrices":
                            return "objects";
                    }
                    break;
                    
                case "KalturaProductsPriceListResponse":
                    switch(property.Name)
                    {
                        case "ProductsPrices":
                            return "objects";
                    }
                    break;
                    
                case "KalturaProgramAsset":
                    switch(property.Name)
                    {
                        case "Crid":
                            return "crid";
                        case "EpgChannelId":
                            return "epgChannelId";
                        case "EpgId":
                            return "epgId";
                        case "LinearAssetId":
                            return "linearAssetId";
                        case "RelatedMediaId":
                            return "relatedMediaId";
                    }
                    break;
                    
                case "KalturaPublicCouponGenerationOptions":
                    switch(property.Name)
                    {
                        case "Code":
                            return "code";
                    }
                    break;
                    
                case "KalturaPurchase":
                    switch(property.Name)
                    {
                        case "AdapterData":
                            return "adapterData";
                        case "Coupon":
                            return "coupon";
                        case "Currency":
                            return "currency";
                        case "PaymentGatewayId":
                            return "paymentGatewayId";
                        case "PaymentMethodId":
                            return "paymentMethodId";
                        case "Price":
                            return "price";
                    }
                    break;
                    
                case "KalturaPurchaseBase":
                    switch(property.Name)
                    {
                        case "ContentId":
                            return "contentId";
                        case "ProductId":
                            return "productId";
                        case "ProductType":
                            return "productType";
                    }
                    break;
                    
                case "KalturaPurchaseSession":
                    switch(property.Name)
                    {
                        case "PreviewModuleId":
                            return "previewModuleId";
                    }
                    break;
                    
                case "KalturaPurchaseSettings":
                    switch(property.Name)
                    {
                        case "Permission":
                            return "permission";
                    }
                    break;
                    
                case "KalturaPurchaseSettingsResponse":
                    switch(property.Name)
                    {
                        case "PurchaseSettingsType":
                            return "purchaseSettingsType";
                    }
                    break;
                    
                case "KalturaPushMessage":
                    switch(property.Name)
                    {
                        case "Action":
                            return "action";
                        case "Message":
                            return "message";
                        case "Sound":
                            return "sound";
                        case "Url":
                            return "url";
                    }
                    break;
                    
                case "KalturaPushParams":
                    switch(property.Name)
                    {
                        case "ExternalToken":
                            return "externalToken";
                        case "Token":
                            return "token";
                    }
                    break;
                    
                case "KalturaRandomCouponGenerationOptions":
                    switch(property.Name)
                    {
                        case "NumberOfCoupons":
                            return "numberOfCoupons";
                        case "UseLetters":
                            return "useLetters";
                        case "UseNumbers":
                            return "useNumbers";
                        case "UseSpecialCharacters":
                            return "useSpecialCharacters";
                    }
                    break;
                    
                case "KalturaRatio":
                    switch(property.Name)
                    {
                        case "Height":
                            return "height";
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "PrecisionPrecentage":
                            return "precisionPrecentage";
                        case "Width":
                            return "width";
                    }
                    break;
                    
                case "KalturaRatioListResponse":
                    switch(property.Name)
                    {
                        case "Ratios":
                            return "objects";
                    }
                    break;
                    
                case "KalturaRecommendationProfile":
                    switch(property.Name)
                    {
                        case "AdapterUrl":
                            return "adapterUrl";
                        case "ExternalIdentifier":
                            return "externalIdentifier";
                        case "Id":
                            return "id";
                        case "IsActive":
                            return "isActive";
                        case "Name":
                            return "name";
                        case "Settings":
                            return "recommendationEngineSettings";
                        case "SharedSecret":
                            return "sharedSecret";
                    }
                    break;
                    
                case "KalturaRecommendationProfileListResponse":
                    switch(property.Name)
                    {
                        case "RecommendationProfiles":
                            return "objects";
                    }
                    break;
                    
                case "KalturaRecording":
                    switch(property.Name)
                    {
                        case "AssetId":
                            return "assetId";
                        case "CreateDate":
                            return "createDate";
                        case "Id":
                            return "id";
                        case "IsProtected":
                            return "isProtected";
                        case "Status":
                            return "status";
                        case "Type":
                            return "type";
                        case "UpdateDate":
                            return "updateDate";
                        case "ViewableUntilDate":
                            return "viewableUntilDate";
                    }
                    break;
                    
                case "KalturaRecordingAsset":
                    switch(property.Name)
                    {
                        case "RecordingId":
                            return "recordingId";
                        case "RecordingType":
                            return "recordingType";
                    }
                    break;
                    
                case "KalturaRecordingContext":
                    switch(property.Name)
                    {
                        case "AssetId":
                            return "assetId";
                        case "Code":
                            return "code";
                        case "Message":
                            return "message";
                        case "Recording":
                            return "recording";
                    }
                    break;
                    
                case "KalturaRecordingContextFilter":
                    switch(property.Name)
                    {
                        case "AssetIdIn":
                            return "assetIdIn";
                    }
                    break;
                    
                case "KalturaRecordingContextListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaRecordingFilter":
                    switch(property.Name)
                    {
                        case "ExternalRecordingIdIn":
                            return "externalRecordingIdIn";
                        case "FilterExpression":
                            return "filterExpression";
                        case "Ksql":
                            return "kSql";
                        case "StatusIn":
                            return "statusIn";
                    }
                    break;
                    
                case "KalturaRecordingListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaRegion":
                    switch(property.Name)
                    {
                        case "ExternalId":
                            return "externalId";
                        case "Id":
                            return "id";
                        case "IsDefault":
                            return "isDefault";
                        case "Name":
                            return "name";
                        case "RegionalChannels":
                            return "linearChannels";
                    }
                    break;
                    
                case "KalturaRegionalChannel":
                    switch(property.Name)
                    {
                        case "ChannelNumber":
                            return "channelNumber";
                        case "LinearChannelId":
                            return "linearChannelId";
                    }
                    break;
                    
                case "KalturaRegionFilter":
                    switch(property.Name)
                    {
                        case "ExternalIdIn":
                            return "externalIdIn";
                    }
                    break;
                    
                case "KalturaRegionListResponse":
                    switch(property.Name)
                    {
                        case "Regions":
                            return "objects";
                    }
                    break;
                    
                case "KalturaRegistryResponse":
                    switch(property.Name)
                    {
                        case "AnnouncementId":
                            return "announcementId";
                        case "Key":
                            return "key";
                        case "Url":
                            return "url";
                    }
                    break;
                    
                case "KalturaRegistrySettings":
                    switch(property.Name)
                    {
                        case "Key":
                            return "key";
                        case "Value":
                            return "value";
                    }
                    break;
                    
                case "KalturaRegistrySettingsListResponse":
                    switch(property.Name)
                    {
                        case "RegistrySettings":
                            return "objects";
                    }
                    break;
                    
                case "KalturaRelatedExternalFilter":
                    switch(property.Name)
                    {
                        case "FreeText":
                            return "freeText";
                        case "IdEqual":
                            return "idEqual";
                        case "TypeIn":
                            return "typeIn";
                        case "UtcOffsetEqual":
                            return "utcOffsetEqual";
                    }
                    break;
                    
                case "KalturaRelatedFilter":
                    switch(property.Name)
                    {
                        case "ExcludeWatched":
                            return "excludeWatched";
                        case "IdEqual":
                            return "idEqual";
                        case "KSql":
                            return "kSql";
                        case "TypeIn":
                            return "typeIn";
                    }
                    break;
                    
                case "KalturaReminder":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "Type":
                            return "type";
                    }
                    break;
                    
                case "KalturaReminderFilter`1":
                    switch(property.Name)
                    {
                        case "KSql":
                            return "kSql";
                    }
                    break;
                    
                case "KalturaReminderListResponse":
                    switch(property.Name)
                    {
                        case "Reminders":
                            return "objects";
                    }
                    break;
                    
                case "KalturaReportListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaRequestConfiguration":
                    switch(property.Name)
                    {
                        case "Currency":
                            return "currency";
                        case "KS":
                            return "ks";
                        case "Language":
                            return "language";
                        case "PartnerID":
                            return "partnerId";
                        case "ResponseProfile":
                            return "responseProfile";
                        case "UserID":
                            return "userId";
                    }
                    break;
                    
                case "KalturaRuleAction":
                    switch(property.Name)
                    {
                        case "Description":
                            return "description";
                        case "Type":
                            return "type";
                    }
                    break;
                    
                case "KalturaRuleFilter":
                    switch(property.Name)
                    {
                        case "By":
                            return "by";
                    }
                    break;
                    
                case "KalturaScheduledRecordingProgramFilter":
                    switch(property.Name)
                    {
                        case "ChannelsIn":
                            return "channelsIn";
                        case "EndDateLessThanOrNull":
                            return "endDateLessThanOrNull";
                        case "RecordingTypeEqual":
                            return "recordingTypeEqual";
                        case "StartDateGreaterThanOrNull":
                            return "startDateGreaterThanOrNull";
                    }
                    break;
                    
                case "KalturaSearchAssetFilter":
                    switch(property.Name)
                    {
                        case "IdIn":
                            return "idIn";
                        case "KSql":
                            return "kSql";
                        case "TypeIn":
                            return "typeIn";
                    }
                    break;
                    
                case "KalturaSearchAssetListFilter":
                    switch(property.Name)
                    {
                        case "ExcludeWatched":
                            return "excludeWatched";
                    }
                    break;
                    
                case "KalturaSearchExternalFilter":
                    switch(property.Name)
                    {
                        case "Query":
                            return "query";
                        case "TypeIn":
                            return "typeIn";
                        case "UtcOffsetEqual":
                            return "utcOffsetEqual";
                    }
                    break;
                    
                case "KalturaSearchHistory":
                    switch(property.Name)
                    {
                        case "Action":
                            return "action";
                        case "CreatedAt":
                            return "createdAt";
                        case "DeviceId":
                            return "deviceId";
                        case "Filter":
                            return "filter";
                        case "Id":
                            return "id";
                        case "Language":
                            return "language";
                        case "Name":
                            return "name";
                        case "Service":
                            return "service";
                    }
                    break;
                    
                case "KalturaSearchHistoryListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaSeasonsReminderFilter":
                    switch(property.Name)
                    {
                        case "EpgChannelIdEqual":
                            return "epgChannelIdEqual";
                        case "SeasonNumberIn":
                            return "seasonNumberIn";
                        case "SeriesIdEqual":
                            return "seriesIdEqual";
                    }
                    break;
                    
                case "KalturaSeriesRecording":
                    switch(property.Name)
                    {
                        case "ChannelId":
                            return "channelId";
                        case "CreateDate":
                            return "createDate";
                        case "EpgId":
                            return "epgId";
                        case "ExcludedSeasons":
                            return "excludedSeasons";
                        case "Id":
                            return "id";
                        case "SeasonNumber":
                            return "seasonNumber";
                        case "SeriesId":
                            return "seriesId";
                        case "Type":
                            return "type";
                        case "UpdateDate":
                            return "updateDate";
                    }
                    break;
                    
                case "KalturaSeriesRecordingListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaSeriesReminder":
                    switch(property.Name)
                    {
                        case "EpgChannelId":
                            return "epgChannelId";
                        case "SeasonNumber":
                            return "seasonNumber";
                        case "SeriesId":
                            return "seriesId";
                    }
                    break;
                    
                case "KalturaSeriesReminderFilter":
                    switch(property.Name)
                    {
                        case "EpgChannelIdEqual":
                            return "epgChannelIdEqual";
                        case "SeriesIdIn":
                            return "seriesIdIn";
                    }
                    break;
                    
                case "KalturaSlimAsset":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Type":
                            return "type";
                    }
                    break;
                    
                case "KalturaSlimAssetInfoWrapper":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaSocial":
                    switch(property.Name)
                    {
                        case "Birthday":
                            return "birthday";
                        case "Email":
                            return "email";
                        case "FirstName":
                            return "firstName";
                        case "Gender":
                            return "gender";
                        case "ID":
                            return "id";
                        case "LastName":
                            return "lastName";
                        case "Name":
                            return "name";
                        case "PictureUrl":
                            return "pictureUrl";
                        case "Status":
                            return "status";
                        case "UserId":
                            return "userId";
                    }
                    break;
                    
                case "KalturaSocialAction":
                    switch(property.Name)
                    {
                        case "ActionType":
                            return "actionType";
                        case "AssetId":
                            return "assetId";
                        case "AssetType":
                            return "assetType";
                        case "Id":
                            return "id";
                        case "Time":
                            return "time";
                        case "Url":
                            return "url";
                    }
                    break;
                    
                case "KalturaSocialActionFilter":
                    switch(property.Name)
                    {
                        case "ActionTypeIn":
                            return "actionTypeIn";
                        case "AssetIdIn":
                            return "assetIdIn";
                        case "AssetTypeEqual":
                            return "assetTypeEqual";
                    }
                    break;
                    
                case "KalturaSocialActionListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaSocialActionRate":
                    switch(property.Name)
                    {
                        case "Rate":
                            return "rate";
                    }
                    break;
                    
                case "KalturaSocialComment":
                    switch(property.Name)
                    {
                        case "CreateDate":
                            return "createDate";
                        case "Header":
                            return "header";
                        case "Text":
                            return "text";
                        case "Writer":
                            return "writer";
                    }
                    break;
                    
                case "KalturaSocialCommentFilter":
                    switch(property.Name)
                    {
                        case "AssetIdEqual":
                            return "assetIdEqual";
                        case "AssetTypeEqual":
                            return "assetTypeEqual";
                        case "CreateDateGreaterThan":
                            return "createDateGreaterThan";
                        case "SocialPlatformEqual":
                            return "socialPlatformEqual";
                    }
                    break;
                    
                case "KalturaSocialCommentListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaSocialFacebookConfig":
                    switch(property.Name)
                    {
                        case "AppId":
                            return "appId";
                        case "Permissions":
                            return "permissions";
                    }
                    break;
                    
                case "KalturaSocialFriendActivity":
                    switch(property.Name)
                    {
                        case "SocialAction":
                            return "socialAction";
                        case "UserFullName":
                            return "userFullName";
                        case "UserPictureUrl":
                            return "userPictureUrl";
                    }
                    break;
                    
                case "KalturaSocialFriendActivityFilter":
                    switch(property.Name)
                    {
                        case "ActionTypeIn":
                            return "actionTypeIn";
                        case "AssetIdEqual":
                            return "assetIdEqual";
                        case "AssetTypeEqual":
                            return "assetTypeEqual";
                    }
                    break;
                    
                case "KalturaSocialFriendActivityListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaSocialNetworkComment":
                    switch(property.Name)
                    {
                        case "AuthorImageUrl":
                            return "authorImageUrl";
                        case "LikeCounter":
                            return "likeCounter";
                    }
                    break;
                    
                case "KalturaSocialResponse":
                    switch(property.Name)
                    {
                        case "Data":
                            return "data";
                        case "KalturaName":
                            return "kalturaUsername";
                        case "MinFriends":
                            return "minFriendsLimitation";
                        case "Pic":
                            return "pic";
                        case "SocialNetworkUsername":
                            return "socialUsername";
                        case "SocialUser":
                            return "socialUser";
                        case "Status":
                            return "status";
                        case "Token":
                            return "token";
                        case "UserId":
                            return "userId";
                    }
                    break;
                    
                case "KalturaSocialUser":
                    switch(property.Name)
                    {
                        case "Birthday":
                            return "birthday";
                        case "Email":
                            return "email";
                        case "FirstName":
                            return "firstName";
                        case "Gender":
                            return "gender";
                        case "ID":
                            return "id";
                        case "LastName":
                            return "lastName";
                        case "Name":
                            return "name";
                        case "UserId":
                            return "userId";
                    }
                    break;
                    
                case "KalturaSocialUserConfig":
                    switch(property.Name)
                    {
                        case "PermissionItems":
                            return "actionPermissionItems";
                    }
                    break;
                    
                case "KalturaSSOAdapterProfile":
                    switch(property.Name)
                    {
                        case "AdapterUrl":
                            return "adapterUrl";
                        case "ExternalIdentifier":
                            return "externalIdentifier";
                        case "Id":
                            return "id";
                        case "IsActive":
                            return "isActive";
                        case "Name":
                            return "name";
                        case "Settings":
                            return "settings";
                        case "SharedSecret":
                            return "sharedSecret";
                    }
                    break;
                    
                case "KalturaSSOAdapterProfileListResponse":
                    switch(property.Name)
                    {
                        case "SSOAdapters":
                            return "objects";
                    }
                    break;
                    
                case "KalturaStringValueArray":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaSubscription":
                    switch(property.Name)
                    {
                        case "Channels":
                            return "channels";
                        case "CouponGroups":
                            return "couponsGroups";
                        case "CouponsGroup":
                            return "couponsGroup";
                        case "DependencyType":
                            return "dependencyType";
                        case "Description":
                            return "description";
                        case "Descriptions":
                            return "descriptions";
                        case "DiscountModule":
                            return "discountModule";
                        case "EndDate":
                            return "endDate";
                        case "ExternalId":
                            return "externalId";
                        case "FileTypes":
                            return "fileTypes";
                        case "GracePeriodMinutes":
                            return "gracePeriodMinutes";
                        case "HouseholdLimitationsId":
                            return "householdLimitationsId";
                        case "Id":
                            return "id";
                        case "IsCancellationBlocked":
                            return "isCancellationBlocked";
                        case "IsInfiniteRenewal":
                            return "isInfiniteRenewal";
                        case "IsRenewable":
                            return "isRenewable";
                        case "IsWaiverEnabled":
                            return "isWaiverEnabled";
                        case "MaxViewsNumber":
                            return "maxViewsNumber";
                        case "MediaId":
                            return "mediaId";
                        case "Name":
                            return "name";
                        case "Names":
                            return "names";
                        case "PremiumServices":
                            return "premiumServices";
                        case "PreviewModule":
                            return "previewModule";
                        case "Price":
                            return "price";
                        case "PricePlanIds":
                            return "pricePlanIds";
                        case "PricePlans":
                            return "pricePlans";
                        case "ProductCode":
                            return "productCode";
                        case "ProductCodes":
                            return "productCodes";
                        case "ProrityInOrder":
                            return "prorityInOrder";
                        case "RenewalsNumber":
                            return "renewalsNumber";
                        case "StartDate":
                            return "startDate";
                        case "UserTypes":
                            return "userTypes";
                        case "ViewLifeCycle":
                            return "viewLifeCycle";
                        case "WaiverPeriod":
                            return "waiverPeriod";
                    }
                    break;
                    
                case "KalturaSubscriptionDependencySet":
                    switch(property.Name)
                    {
                        case "BaseSubscriptionId":
                            return "baseSubscriptionId";
                    }
                    break;
                    
                case "KalturaSubscriptionDependencySetFilter":
                    switch(property.Name)
                    {
                        case "BaseSubscriptionIdIn":
                            return "baseSubscriptionIdIn";
                    }
                    break;
                    
                case "KalturaSubscriptionEntitlement":
                    switch(property.Name)
                    {
                        case "IsInGracePeriod":
                            return "isInGracePeriod";
                        case "IsRenewable":
                            return "isRenewable";
                        case "IsRenewableForPurchase":
                            return "isRenewableForPurchase";
                        case "IsSuspended":
                            return "isSuspended";
                        case "NextRenewalDate":
                            return "nextRenewalDate";
                        case "PaymentGatewayId":
                            return "paymentGatewayId";
                        case "PaymentMethodId":
                            return "paymentMethodId";
                        case "ScheduledSubscriptionId":
                            return "scheduledSubscriptionId";
                        case "UnifiedPaymentId":
                            return "unifiedPaymentId";
                    }
                    break;
                    
                case "KalturaSubscriptionFilter":
                    switch(property.Name)
                    {
                        case "ExternalIdIn":
                            return "externalIdIn";
                        case "MediaFileIdEqual":
                            return "mediaFileIdEqual";
                        case "SubscriptionIdIn":
                            return "subscriptionIdIn";
                    }
                    break;
                    
                case "KalturaSubscriptionListResponse":
                    switch(property.Name)
                    {
                        case "Subscriptions":
                            return "objects";
                    }
                    break;
                    
                case "KalturaSubscriptionPrice":
                    switch(property.Name)
                    {
                        case "EndDate":
                            return "endDate";
                        case "Price":
                            return "price";
                        case "PurchaseStatus":
                            return "purchaseStatus";
                    }
                    break;
                    
                case "KalturaSubscriptionSet":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "SubscriptionIds":
                            return "subscriptionIds";
                        case "Type":
                            return "type";
                    }
                    break;
                    
                case "KalturaSubscriptionSetFilter":
                    switch(property.Name)
                    {
                        case "IdIn":
                            return "idIn";
                        case "SubscriptionIdContains":
                            return "subscriptionIdContains";
                        case "TypeEqual":
                            return "typeEqual";
                    }
                    break;
                    
                case "KalturaSubscriptionSetListResponse":
                    switch(property.Name)
                    {
                        case "SubscriptionSets":
                            return "objects";
                    }
                    break;
                    
                case "KalturaSubscriptionsFilter":
                    switch(property.Name)
                    {
                        case "By":
                            return "by";
                        case "Ids":
                            return "ids";
                    }
                    break;
                    
                case "KalturaTag":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Tag":
                            return "tag";
                        case "TagTypeId":
                            return "type";
                    }
                    break;
                    
                case "KalturaTagFilter":
                    switch(property.Name)
                    {
                        case "LanguageEqual":
                            return "languageEqual";
                        case "TagEqual":
                            return "tagEqual";
                        case "TagStartsWith":
                            return "tagStartsWith";
                        case "TypeEqual":
                            return "typeEqual";
                    }
                    break;
                    
                case "KalturaTagListResponse":
                    switch(property.Name)
                    {
                        case "Tags":
                            return "objects";
                    }
                    break;
                    
                case "KalturaTimeOffsetRuleAction":
                    switch(property.Name)
                    {
                        case "Offset":
                            return "offset";
                        case "TimeZone":
                            return "timeZone";
                    }
                    break;
                    
                case "KalturaTimeShiftedTvPartnerSettings":
                    switch(property.Name)
                    {
                        case "CatchUpBufferLength":
                            return "catchUpBufferLength";
                        case "CatchUpEnabled":
                            return "catchUpEnabled";
                        case "CdvrEnabled":
                            return "cdvrEnabled";
                        case "CleanupNoticePeriod":
                            return "cleanupNoticePeriod";
                        case "NonEntitledChannelPlaybackEnabled":
                            return "nonEntitledChannelPlaybackEnabled";
                        case "NonExistingChannelPlaybackEnabled":
                            return "nonExistingChannelPlaybackEnabled";
                        case "PaddingAfterProgramEnds":
                            return "paddingAfterProgramEnds";
                        case "PaddingBeforeProgramStarts":
                            return "paddingBeforeProgramStarts";
                        case "ProtectionEnabled":
                            return "protectionEnabled";
                        case "ProtectionPeriod":
                            return "protectionPeriod";
                        case "ProtectionPolicy":
                            return "protectionPolicy";
                        case "ProtectionQuotaPercentage":
                            return "protectionQuotaPercentage";
                        case "QuotaOveragePolicy":
                            return "quotaOveragePolicy";
                        case "RecordingLifetimePeriod":
                            return "recordingLifetimePeriod";
                        case "RecordingScheduleWindow":
                            return "recordingScheduleWindow";
                        case "RecordingScheduleWindowEnabled":
                            return "recordingScheduleWindowEnabled";
                        case "RecoveryGracePeriod":
                            return "recoveryGracePeriod";
                        case "SeriesRecordingEnabled":
                            return "seriesRecordingEnabled";
                        case "StartOverEnabled":
                            return "startOverEnabled";
                        case "TrickPlayBufferLength":
                            return "trickPlayBufferLength";
                        case "TrickPlayEnabled":
                            return "trickPlayEnabled";
                    }
                    break;
                    
                case "KalturaTopic":
                    switch(property.Name)
                    {
                        case "AutomaticIssueNotification":
                            return "automaticIssueNotification";
                        case "Id":
                            return "id";
                        case "LastMessageSentDateSec":
                            return "lastMessageSentDateSec";
                        case "Name":
                            return "name";
                        case "SubscribersAmount":
                            return "subscribersAmount";
                    }
                    break;
                    
                case "KalturaTopicListResponse":
                    switch(property.Name)
                    {
                        case "Topics":
                            return "objects";
                    }
                    break;
                    
                case "KalturaTopicResponse":
                    switch(property.Name)
                    {
                        case "Topics":
                            return "objects";
                    }
                    break;
                    
                case "KalturaTransaction":
                    switch(property.Name)
                    {
                        case "CreatedAt":
                            return "createdAt";
                        case "FailReasonCode":
                            return "failReasonCode";
                        case "Id":
                            return "id";
                        case "PGReferenceID":
                            return "paymentGatewayReferenceId";
                        case "PGResponseID":
                            return "paymentGatewayResponseId";
                        case "State":
                            return "state";
                    }
                    break;
                    
                case "KalturaTransactionHistoryFilter":
                    switch(property.Name)
                    {
                        case "EndDateLessThanOrEqual":
                            return "endDateLessThanOrEqual";
                        case "EntityReferenceEqual":
                            return "entityReferenceEqual";
                        case "StartDateGreaterThanOrEqual":
                            return "startDateGreaterThanOrEqual";
                    }
                    break;
                    
                case "KalturaTransactionsFilter":
                    switch(property.Name)
                    {
                        case "By":
                            return "by";
                        case "EndDate":
                            return "endDate";
                        case "StartDate":
                            return "startDate";
                    }
                    break;
                    
                case "KalturaTransactionStatus":
                    switch(property.Name)
                    {
                        case "AdapterStatus":
                            return "adapterTransactionStatus";
                        case "ExternalId":
                            return "externalId";
                        case "ExternalMessage":
                            return "externalMessage";
                        case "ExternalStatus":
                            return "externalStatus";
                        case "FailReason":
                            return "failReason";
                    }
                    break;
                    
                case "KalturaTranslationToken":
                    switch(property.Name)
                    {
                        case "Language":
                            return "language";
                        case "Value":
                            return "value";
                    }
                    break;
                    
                case "KalturaUnifiedPaymentRenewal":
                    switch(property.Name)
                    {
                        case "Date":
                            return "date";
                        case "Entitlements":
                            return "entitlements";
                        case "Price":
                            return "price";
                        case "UnifiedPaymentId":
                            return "unifiedPaymentId";
                    }
                    break;
                    
                case "KalturaUploadedFileTokenResource":
                    switch(property.Name)
                    {
                        case "Token":
                            return "token";
                    }
                    break;
                    
                case "KalturaUploadToken":
                    switch(property.Name)
                    {
                        case "CreateDate":
                            return "createDate";
                        case "FileSize":
                            return "fileSize";
                        case "Id":
                            return "id";
                        case "Status":
                            return "status";
                        case "UpdateDate":
                            return "updateDate";
                    }
                    break;
                    
                case "KalturaUrlResource":
                    switch(property.Name)
                    {
                        case "Url":
                            return "url";
                    }
                    break;
                    
                case "KalturaUsageModule":
                    switch(property.Name)
                    {
                        case "CouponId":
                            return "couponId";
                        case "FullLifeCycle":
                            return "fullLifeCycle";
                        case "Id":
                            return "id";
                        case "IsOfflinePlayback":
                            return "isOfflinePlayback";
                        case "IsWaiverEnabled":
                            return "isWaiverEnabled";
                        case "MaxViewsNumber":
                            return "maxViewsNumber";
                        case "Name":
                            return "name";
                        case "ViewLifeCycle":
                            return "viewLifeCycle";
                        case "WaiverPeriod":
                            return "waiverPeriod";
                    }
                    break;
                    
                case "KalturaUserAssetRule":
                    switch(property.Name)
                    {
                        case "Description":
                            return "description";
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "RuleType":
                            return "ruleType";
                    }
                    break;
                    
                case "KalturaUserAssetRuleFilter":
                    switch(property.Name)
                    {
                        case "AssetIdEqual":
                            return "assetIdEqual";
                        case "AssetTypeEqual":
                            return "assetTypeEqual";
                    }
                    break;
                    
                case "KalturaUserAssetRuleListResponse":
                    switch(property.Name)
                    {
                        case "Rules":
                            return "objects";
                    }
                    break;
                    
                case "KalturaUserAssetsList":
                    switch(property.Name)
                    {
                        case "List":
                            return "list";
                        case "ListType":
                            return "listType";
                    }
                    break;
                    
                case "KalturaUserAssetsListFilter":
                    switch(property.Name)
                    {
                        case "AssetTypeEqual":
                            return "assetTypeEqual";
                        case "By":
                            return "by";
                        case "ListTypeEqual":
                            return "listTypeEqual";
                    }
                    break;
                    
                case "KalturaUserAssetsListItem":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "ListType":
                            return "listType";
                        case "OrderIndex":
                            return "orderIndex";
                        case "Type":
                            return "type";
                        case "UserId":
                            return "userId";
                    }
                    break;
                    
                case "KalturaUserBillingTransaction":
                    switch(property.Name)
                    {
                        case "UserFullName":
                            return "userFullName";
                        case "UserID":
                            return "userId";
                    }
                    break;
                    
                case "KalturaUserInterest":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Topic":
                            return "topic";
                    }
                    break;
                    
                case "KalturaUserInterestListResponse":
                    switch(property.Name)
                    {
                        case "UserInterests":
                            return "objects";
                    }
                    break;
                    
                case "KalturaUserInterestTopic":
                    switch(property.Name)
                    {
                        case "MetaId":
                            return "metaId";
                        case "ParentTopic":
                            return "parentTopic";
                        case "Value":
                            return "value";
                    }
                    break;
                    
                case "KalturaUserLoginPin":
                    switch(property.Name)
                    {
                        case "ExpirationTime":
                            return "expirationTime";
                        case "PinCode":
                            return "pinCode";
                        case "UserId":
                            return "userId";
                    }
                    break;
                    
                case "KalturaUserRole":
                    switch(property.Name)
                    {
                        case "ExcludedPermissionNames":
                            return "excludedPermissionNames";
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "PermissionNames":
                            return "permissionNames";
                        case "Permissions":
                            return "permissions";
                    }
                    break;
                    
                case "KalturaUserRoleFilter":
                    switch(property.Name)
                    {
                        case "CurrentUserRoleIdsContains":
                            return "currentUserRoleIdsContains";
                        case "IdIn":
                            return "idIn";
                        case "Ids":
                            return "ids";
                    }
                    break;
                    
                case "KalturaUserRoleListResponse":
                    switch(property.Name)
                    {
                        case "UserRoles":
                            return "objects";
                    }
                    break;
                    
                case "KalturaUserSocialActionResponse":
                    switch(property.Name)
                    {
                        case "NetworkStatus":
                            return "failStatus";
                        case "SocialAction":
                            return "socialAction";
                    }
                    break;
                    
                case "KalturaWatchHistoryAsset":
                    switch(property.Name)
                    {
                        case "Asset":
                            return "asset";
                        case "Duration":
                            return "duration";
                        case "IsFinishedWatching":
                            return "finishedWatching";
                        case "LastWatched":
                            return "watchedDate";
                        case "Position":
                            return "position";
                    }
                    break;
                    
                case "KalturaWatchHistoryAssetWrapper":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
            }
            
            return property.Name;
        }
        
        public static object execAction(string service, string action, List<object> methodParams)
        {
            service = service.ToLower();
            action = action.ToLower();
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion();
            switch (service)
            {
                case "announcement":
                    switch(action)
                    {
                        case "add":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("announcement", "add", false);
                                return AnnouncementController.AddOldStandard((KalturaAnnouncement) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("announcement", "add", false);
                            return AnnouncementController.Add((KalturaAnnouncement) methodParams[0]);
                            
                        case "addoldstandard":
                            RolesManager.ValidateActionPermitted("announcement", "addOldStandard", false);
                            return AnnouncementController.AddOldStandard((KalturaAnnouncement) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("announcement", "delete", false);
                            return AnnouncementController.Delete((long) methodParams[0]);
                            
                        case "enablesystemannouncements":
                            RolesManager.ValidateActionPermitted("announcement", "enableSystemAnnouncements", false);
                            return AnnouncementController.EnableSystemAnnouncements();
                            
                        case "list":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("announcement", "list", false);
                                return AnnouncementController.ListOldStandard((KalturaFilterPager) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("announcement", "list", false);
                            return AnnouncementController.List((KalturaAnnouncementFilter) methodParams[0], (KalturaFilterPager) methodParams[1]);
                            
                        case "listoldstandard":
                            RolesManager.ValidateActionPermitted("announcement", "listOldStandard", false);
                            return AnnouncementController.ListOldStandard((KalturaFilterPager) methodParams[0]);
                            
                        case "update":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("announcement", "update", false);
                                return AnnouncementController.UpdateOldStandard((KalturaAnnouncement) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("announcement", "update", false);
                            return AnnouncementController.Update((int) methodParams[0], (KalturaAnnouncement) methodParams[1]);
                            
                        case "updateoldstandard":
                            RolesManager.ValidateActionPermitted("announcement", "updateOldStandard", false);
                            return AnnouncementController.UpdateOldStandard((KalturaAnnouncement) methodParams[0]);
                            
                        case "updatestatus":
                            RolesManager.ValidateActionPermitted("announcement", "updateStatus", false);
                            return AnnouncementController.UpdateStatus((long) methodParams[0], (bool) methodParams[1]);
                            
                        case "createAnnouncement":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("announcement", "createAnnouncement", false);
                                return AnnouncementController.EnableSystemAnnouncements();
                            }
                            break;
                            
                    }
                    break;
                    
                case "apptoken":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("appToken", "add", false);
                            return AppTokenController.Add((KalturaAppToken) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("appToken", "delete", false);
                            return AppTokenController.Delete((string) methodParams[0]);
                            
                        case "get":
                            RolesManager.ValidateActionPermitted("appToken", "get", false);
                            return AppTokenController.Get((string) methodParams[0]);
                            
                        case "startsession":
                            RolesManager.ValidateActionPermitted("appToken", "startSession", false);
                            return AppTokenController.StartSession((string) methodParams[0], (string) methodParams[1], (string) methodParams[2], (Nullable<int>) methodParams[3], (string) methodParams[4]);
                            
                    }
                    break;
                    
                case "assetcomment":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("assetComment", "add", false);
                            return AssetCommentController.Add((KalturaAssetComment) methodParams[0]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("assetComment", "list", false);
                            return AssetCommentController.List((KalturaAssetCommentFilter) methodParams[0], (KalturaFilterPager) methodParams[1]);
                            
                    }
                    break;
                    
                case "asset":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("asset", "add", false);
                            return AssetController.Add((KalturaAsset) methodParams[0]);
                            
                        case "autocomplete":
                            RolesManager.ValidateActionPermitted("asset", "autocomplete", false);
                            return AssetController.Autocomplete((string) methodParams[0], (List<KalturaCatalogWithHolder>) methodParams[1], (List<KalturaIntegerValue>) methodParams[2], (Nullable<KalturaOrder>) methodParams[3], (Nullable<int>) methodParams[4]);
                            
                        case "channel":
                            RolesManager.ValidateActionPermitted("asset", "channel", false);
                            return AssetController.Channel((int) methodParams[0], (List<KalturaCatalogWithHolder>) methodParams[1], (Nullable<KalturaOrder>) methodParams[2], (KalturaFilterPager) methodParams[3], (string) methodParams[4]);
                            
                        case "count":
                            RolesManager.ValidateActionPermitted("asset", "count", false);
                            return AssetController.Count((KalturaSearchAssetFilter) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("asset", "delete", false);
                            return AssetController.Delete((long) methodParams[0], (KalturaAssetReferenceType) methodParams[1]);
                            
                        case "externalchannel":
                            RolesManager.ValidateActionPermitted("asset", "externalChannel", false);
                            return AssetController.ExternalChannel((int) methodParams[0], (List<KalturaCatalogWithHolder>) methodParams[1], (Nullable<KalturaOrder>) methodParams[2], (KalturaFilterPager) methodParams[3], (Nullable<float>) methodParams[4], (string) methodParams[5]);
                            
                        case "get":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("asset", "get", false);
                                return AssetController.GetOldStandard((string) methodParams[0], (KalturaAssetReferenceType) methodParams[1], (List<KalturaCatalogWithHolder>) methodParams[2]);
                            }
                            RolesManager.ValidateActionPermitted("asset", "get", false);
                            return AssetController.Get((string) methodParams[0], (KalturaAssetReferenceType) methodParams[1]);
                            
                        case "getadscontext":
                            RolesManager.ValidateActionPermitted("asset", "getAdsContext", false);
                            return AssetController.GetAdsContext((string) methodParams[0], (KalturaAssetType) methodParams[1], (KalturaPlaybackContextOptions) methodParams[2]);
                            
                        case "getoldstandard":
                            RolesManager.ValidateActionPermitted("asset", "getOldStandard", false);
                            return AssetController.GetOldStandard((string) methodParams[0], (KalturaAssetReferenceType) methodParams[1], (List<KalturaCatalogWithHolder>) methodParams[2]);
                            
                        case "getplaybackcontext":
                            RolesManager.ValidateActionPermitted("asset", "getPlaybackContext", false);
                            return AssetController.GetPlaybackContext((string) methodParams[0], (KalturaAssetType) methodParams[1], (KalturaPlaybackContextOptions) methodParams[2]);
                            
                        case "list":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("asset", "list", false);
                                return AssetController.ListOldStandard((KalturaAssetInfoFilter) methodParams[0], (List<KalturaCatalogWithHolder>) methodParams[1], (Nullable<KalturaOrder>) methodParams[2], (KalturaFilterPager) methodParams[3]);
                            }
                            RolesManager.ValidateActionPermitted("asset", "list", false);
                            return AssetController.List((KalturaAssetFilter) methodParams[0], (KalturaFilterPager) methodParams[1]);
                            
                        case "listoldstandard":
                            RolesManager.ValidateActionPermitted("asset", "listOldStandard", false);
                            return AssetController.ListOldStandard((KalturaAssetInfoFilter) methodParams[0], (List<KalturaCatalogWithHolder>) methodParams[1], (Nullable<KalturaOrder>) methodParams[2], (KalturaFilterPager) methodParams[3]);
                            
                        case "related":
                            RolesManager.ValidateActionPermitted("asset", "related", false);
                            return AssetController.Related((int) methodParams[0], (string) methodParams[1], (KalturaFilterPager) methodParams[2], (List<KalturaIntegerValue>) methodParams[3], (List<KalturaCatalogWithHolder>) methodParams[4]);
                            
                        case "relatedexternal":
                            RolesManager.ValidateActionPermitted("asset", "relatedExternal", false);
                            return AssetController.RelatedExternal((int) methodParams[0], (KalturaFilterPager) methodParams[1], (List<KalturaIntegerValue>) methodParams[2], (int) methodParams[3], (List<KalturaCatalogWithHolder>) methodParams[4], (string) methodParams[5]);
                            
                        case "removemetasandtags":
                            RolesManager.ValidateActionPermitted("asset", "removeMetasAndTags", false);
                            return AssetController.RemoveMetasAndTags((long) methodParams[0], (KalturaAssetReferenceType) methodParams[1], (string) methodParams[2]);
                            
                        case "search":
                            RolesManager.ValidateActionPermitted("asset", "search", false);
                            return AssetController.Search((Nullable<KalturaOrder>) methodParams[0], (List<KalturaIntegerValue>) methodParams[1], (string) methodParams[2], (List<KalturaCatalogWithHolder>) methodParams[3], (KalturaFilterPager) methodParams[4], (string) methodParams[5]);
                            
                        case "searchexternal":
                            RolesManager.ValidateActionPermitted("asset", "searchExternal", false);
                            return AssetController.searchExternal((string) methodParams[0], (KalturaFilterPager) methodParams[1], (List<KalturaIntegerValue>) methodParams[2], (int) methodParams[3], (List<KalturaCatalogWithHolder>) methodParams[4]);
                            
                        case "update":
                            RolesManager.ValidateActionPermitted("asset", "update", false);
                            return AssetController.Update((long) methodParams[0], (KalturaAsset) methodParams[1]);
                            
                    }
                    break;
                    
                case "assetfile":
                    switch(action)
                    {
                        case "getcontext":
                            RolesManager.ValidateActionPermitted("assetFile", "getContext", false);
                            return AssetFileController.GetContext((string) methodParams[0], (KalturaContextType) methodParams[1]);
                            
                        case "playmanifest":
                            return AssetFileController.PlayManifest((int) methodParams[0], (string) methodParams[1], (KalturaAssetType) methodParams[2], (long) methodParams[3], (KalturaPlaybackContextType) methodParams[4], (string) methodParams[5]);
                            
                    }
                    break;
                    
                case "assethistory":
                    switch(action)
                    {
                        case "clean":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("assetHistory", "clean", false);
                                return AssetHistoryController.CleanOldStandard((KalturaAssetsFilter) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("assetHistory", "clean", false);
                            AssetHistoryController.Clean((KalturaAssetHistoryFilter) methodParams[0]);
                            return null;
                            
                        case "cleanoldstandard":
                            RolesManager.ValidateActionPermitted("assetHistory", "cleanOldStandard", false);
                            return AssetHistoryController.CleanOldStandard((KalturaAssetsFilter) methodParams[0]);
                            
                        case "list":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("assetHistory", "list", false);
                                return AssetHistoryController.ListOldStandard((KalturaAssetHistoryFilter) methodParams[0], (KalturaFilterPager) methodParams[1]);
                            }
                            RolesManager.ValidateActionPermitted("assetHistory", "list", false);
                            return AssetHistoryController.List((KalturaAssetHistoryFilter) methodParams[0], (KalturaFilterPager) methodParams[1]);
                            
                        case "listoldstandard":
                            RolesManager.ValidateActionPermitted("assetHistory", "listOldStandard", false);
                            return AssetHistoryController.ListOldStandard((KalturaAssetHistoryFilter) methodParams[0], (KalturaFilterPager) methodParams[1]);
                            
                    }
                    break;
                    
                case "assetrule":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("assetRule", "add", false);
                            return AssetRuleController.Add((KalturaAssetRule) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("assetRule", "delete", false);
                            return AssetRuleController.Delete((long) methodParams[0]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("assetRule", "list", false);
                            return AssetRuleController.List((KalturaAssetRuleFilter) methodParams[0]);
                            
                        case "update":
                            RolesManager.ValidateActionPermitted("assetRule", "update", false);
                            return AssetRuleController.Update((long) methodParams[0], (KalturaAssetRule) methodParams[1]);
                            
                    }
                    break;
                    
                case "assetstatistics":
                    switch(action)
                    {
                        case "query":
                            RolesManager.ValidateActionPermitted("assetStatistics", "query", false);
                            return AssetStatisticsController.Query((KalturaAssetStatisticsQuery) methodParams[0]);
                            
                    }
                    break;
                    
                case "assetstruct":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("assetStruct", "add", false);
                            return AssetStructController.Add((KalturaAssetStruct) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("assetStruct", "delete", false);
                            return AssetStructController.Delete((long) methodParams[0]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("assetStruct", "list", false);
                            return AssetStructController.List((KalturaAssetStructFilter) methodParams[0]);
                            
                        case "update":
                            RolesManager.ValidateActionPermitted("assetStruct", "update", false);
                            return AssetStructController.Update((long) methodParams[0], (KalturaAssetStruct) methodParams[1]);
                            
                    }
                    break;
                    
                case "assetstructmeta":
                    switch(action)
                    {
                        case "list":
                            RolesManager.ValidateActionPermitted("assetStructMeta", "list", false);
                            return AssetStructMetaController.List((KalturaAssetStructMetaFilter) methodParams[0]);
                            
                        case "update":
                            RolesManager.ValidateActionPermitted("assetStructMeta", "update", false);
                            return AssetStructMetaController.Update((long) methodParams[0], (long) methodParams[1], (KalturaAssetStructMeta) methodParams[2]);
                            
                    }
                    break;
                    
                case "assetuserrule":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("assetUserRule", "add", false);
                            return AssetUserRuleController.Add((KalturaAssetUserRule) methodParams[0]);
                            
                        case "attachuser":
                            RolesManager.ValidateActionPermitted("assetUserRule", "attachUser", false);
                            AssetUserRuleController.AttachUser((long) methodParams[0]);
                            return null;
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("assetUserRule", "delete", false);
                            AssetUserRuleController.Delete((long) methodParams[0]);
                            return null;
                            
                        case "detachuser":
                            RolesManager.ValidateActionPermitted("assetUserRule", "detachUser", false);
                            AssetUserRuleController.DetachUser((long) methodParams[0]);
                            return null;
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("assetUserRule", "list", false);
                            return AssetUserRuleController.List((KalturaAssetUserRuleFilter) methodParams[0]);
                            
                        case "update":
                            RolesManager.ValidateActionPermitted("assetUserRule", "update", false);
                            return AssetUserRuleController.Update((long) methodParams[0], (KalturaAssetUserRule) methodParams[1]);
                            
                    }
                    break;
                    
                case "bookmark":
                    switch(action)
                    {
                        case "add":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("bookmark", "add", true);
                                return BookmarkController.AddOldStandard((string) methodParams[0], (KalturaAssetType) methodParams[1], (long) methodParams[2], (KalturaPlayerAssetData) methodParams[3]);
                            }
                            RolesManager.ValidateActionPermitted("bookmark", "add", true);
                            return BookmarkController.Add((KalturaBookmark) methodParams[0]);
                            
                        case "addoldstandard":
                            RolesManager.ValidateActionPermitted("bookmark", "addOldStandard", true);
                            return BookmarkController.AddOldStandard((string) methodParams[0], (KalturaAssetType) methodParams[1], (long) methodParams[2], (KalturaPlayerAssetData) methodParams[3]);
                            
                        case "list":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("bookmark", "list", false);
                                return BookmarkController.ListOldStandard((KalturaAssetsFilter) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("bookmark", "list", false);
                            return BookmarkController.List((KalturaBookmarkFilter) methodParams[0]);
                            
                        case "listoldstandard":
                            RolesManager.ValidateActionPermitted("bookmark", "listOldStandard", false);
                            return BookmarkController.ListOldStandard((KalturaAssetsFilter) methodParams[0]);
                            
                    }
                    break;
                    
                case "bulk":
                    switch(action)
                    {
                        case "list":
                            RolesManager.ValidateActionPermitted("bulk", "list", false);
                            return BulkController.List((KalturaBulkFilter) methodParams[0], (KalturaFilterPager) methodParams[1]);
                            
                        case "servelog":
                            RolesManager.ValidateActionPermitted("bulk", "serveLog", false);
                            return BulkController.ServeLog((long) methodParams[0]);
                            
                    }
                    break;
                    
                case "category":
                    switch(action)
                    {
                        case "get":
                            RolesManager.ValidateActionPermitted("category", "get", false);
                            return CategoryController.Get((int) methodParams[0]);
                            
                    }
                    break;
                    
                case "cdnadapterprofile":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("cdnAdapterProfile", "add", false);
                            return CdnAdapterProfileController.Add((KalturaCDNAdapterProfile) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("cdnAdapterProfile", "delete", false);
                            return CdnAdapterProfileController.Delete((int) methodParams[0]);
                            
                        case "generatesharedsecret":
                            RolesManager.ValidateActionPermitted("cdnAdapterProfile", "generateSharedSecret", false);
                            return CdnAdapterProfileController.GenerateSharedSecret((int) methodParams[0]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("cdnAdapterProfile", "list", false);
                            return CdnAdapterProfileController.List();
                            
                        case "update":
                            RolesManager.ValidateActionPermitted("cdnAdapterProfile", "update", false);
                            return CdnAdapterProfileController.Update((int) methodParams[0], (KalturaCDNAdapterProfile) methodParams[1]);
                            
                    }
                    break;
                    
                case "cdnpartnersettings":
                    switch(action)
                    {
                        case "get":
                            RolesManager.ValidateActionPermitted("cdnPartnerSettings", "get", false);
                            return CdnPartnerSettingsController.Get();
                            
                        case "update":
                            RolesManager.ValidateActionPermitted("cdnPartnerSettings", "update", false);
                            return CdnPartnerSettingsController.Update((KalturaCDNPartnerSettings) methodParams[0]);
                            
                    }
                    break;
                    
                case "cdvradapterprofile":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("cDVRAdapterProfile", "add", false);
                            return CDVRAdapterProfileController.Add((KalturaCDVRAdapterProfile) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("cDVRAdapterProfile", "delete", false);
                            return CDVRAdapterProfileController.Delete((int) methodParams[0]);
                            
                        case "generatesharedsecret":
                            RolesManager.ValidateActionPermitted("cDVRAdapterProfile", "generateSharedSecret", false);
                            return CDVRAdapterProfileController.GenerateSharedSecret((int) methodParams[0]);
                            
                        case "list":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("cDVRAdapterProfile", "list", false);
                                return CDVRAdapterProfileController.ListOldStandard();
                            }
                            RolesManager.ValidateActionPermitted("cDVRAdapterProfile", "list", false);
                            return CDVRAdapterProfileController.List();
                            
                        case "listoldstandard":
                            RolesManager.ValidateActionPermitted("cDVRAdapterProfile", "listOldStandard", false);
                            return CDVRAdapterProfileController.ListOldStandard();
                            
                        case "update":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("cDVRAdapterProfile", "update", false);
                                return CDVRAdapterProfileController.UpdateOldStandard((KalturaCDVRAdapterProfile) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("cDVRAdapterProfile", "update", false);
                            return CDVRAdapterProfileController.Update((int) methodParams[0], (KalturaCDVRAdapterProfile) methodParams[1]);
                            
                        case "updateoldstandard":
                            RolesManager.ValidateActionPermitted("cDVRAdapterProfile", "updateOldStandard", false);
                            return CDVRAdapterProfileController.UpdateOldStandard((KalturaCDVRAdapterProfile) methodParams[0]);
                            
                    }
                    break;
                    
                case "channel":
                    switch(action)
                    {
                        case "add":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("channel", "add", false);
                                return ChannelController.AddOldStandard((KalturaChannelProfile) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("channel", "add", false);
                            return ChannelController.Add((KalturaChannel) methodParams[0]);
                            
                        case "addoldstandard":
                            RolesManager.ValidateActionPermitted("channel", "addOldStandard", false);
                            return ChannelController.AddOldStandard((KalturaChannelProfile) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("channel", "delete", false);
                            return ChannelController.Delete((int) methodParams[0]);
                            
                        case "get":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("channel", "get", false);
                                return ChannelController.GetOldStandard((int) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("channel", "get", false);
                            return ChannelController.Get((int) methodParams[0]);
                            
                        case "getoldstandard":
                            RolesManager.ValidateActionPermitted("channel", "getOldStandard", false);
                            return ChannelController.GetOldStandard((int) methodParams[0]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("channel", "list", false);
                            return ChannelController.List((KalturaChannelsFilter) methodParams[0], (KalturaFilterPager) methodParams[1]);
                            
                        case "update":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("channel", "update", false);
                                return ChannelController.UpdateOldStandard((KalturaChannelProfile) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("channel", "update", false);
                            return ChannelController.Update((int) methodParams[0], (KalturaChannel) methodParams[1]);
                            
                        case "updateoldstandard":
                            RolesManager.ValidateActionPermitted("channel", "updateOldStandard", false);
                            return ChannelController.UpdateOldStandard((KalturaChannelProfile) methodParams[0]);
                            
                    }
                    break;
                    
                case "collection":
                    switch(action)
                    {
                        case "list":
                            RolesManager.ValidateActionPermitted("collection", "list", false);
                            return CollectionController.List((KalturaCollectionFilter) methodParams[0]);
                            
                    }
                    break;
                    
                case "compensation":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("compensation", "add", false);
                            return CompensationController.Add((KalturaCompensation) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("compensation", "delete", false);
                            CompensationController.Delete((long) methodParams[0]);
                            return null;
                            
                        case "get":
                            RolesManager.ValidateActionPermitted("compensation", "get", false);
                            return CompensationController.Get((long) methodParams[0]);
                            
                    }
                    break;
                    
                case "configurationgroup":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("configurationGroup", "add", false);
                            return ConfigurationGroupController.Add((KalturaConfigurationGroup) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("configurationGroup", "delete", false);
                            return ConfigurationGroupController.Delete((string) methodParams[0]);
                            
                        case "get":
                            RolesManager.ValidateActionPermitted("configurationGroup", "get", false);
                            return ConfigurationGroupController.Get((string) methodParams[0]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("configurationGroup", "list", false);
                            return ConfigurationGroupController.List();
                            
                        case "update":
                            RolesManager.ValidateActionPermitted("configurationGroup", "update", false);
                            return ConfigurationGroupController.Update((string) methodParams[0], (KalturaConfigurationGroup) methodParams[1]);
                            
                    }
                    break;
                    
                case "configurationgroupdevice":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("configurationGroupDevice", "add", false);
                            return ConfigurationGroupDeviceController.Add((KalturaConfigurationGroupDevice) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("configurationGroupDevice", "delete", false);
                            return ConfigurationGroupDeviceController.Delete((string) methodParams[0]);
                            
                        case "get":
                            RolesManager.ValidateActionPermitted("configurationGroupDevice", "get", false);
                            return ConfigurationGroupDeviceController.Get((string) methodParams[0]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("configurationGroupDevice", "list", false);
                            return ConfigurationGroupDeviceController.List((KalturaConfigurationGroupDeviceFilter) methodParams[0], (KalturaFilterPager) methodParams[1]);
                            
                    }
                    break;
                    
                case "configurationgrouptag":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("configurationGroupTag", "add", false);
                            return ConfigurationGroupTagController.Add((KalturaConfigurationGroupTag) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("configurationGroupTag", "delete", false);
                            return ConfigurationGroupTagController.Delete((string) methodParams[0]);
                            
                        case "get":
                            RolesManager.ValidateActionPermitted("configurationGroupTag", "get", false);
                            return ConfigurationGroupTagController.Get((string) methodParams[0]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("configurationGroupTag", "list", false);
                            return ConfigurationGroupTagController.List((KalturaConfigurationGroupTagFilter) methodParams[0]);
                            
                    }
                    break;
                    
                case "configurations":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("configurations", "add", false);
                            return ConfigurationsController.Add((KalturaConfigurations) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("configurations", "delete", false);
                            return ConfigurationsController.Delete((string) methodParams[0]);
                            
                        case "get":
                            RolesManager.ValidateActionPermitted("configurations", "get", false);
                            return ConfigurationsController.Get((string) methodParams[0]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("configurations", "list", false);
                            return ConfigurationsController.List((KalturaConfigurationsFilter) methodParams[0]);
                            
                        case "servebydevice":
                            HttpContext.Current.Items[RequestParser.REQUEST_SERVE_CONTENT_TYPE] = "application/json";
                            return ConfigurationsController.ServeByDevice((string) methodParams[0], (string) methodParams[1], (string) methodParams[2], (string) methodParams[3], (string) methodParams[4], (int) methodParams[5]);
                            
                        case "update":
                            RolesManager.ValidateActionPermitted("configurations", "update", false);
                            return ConfigurationsController.Update((string) methodParams[0], (KalturaConfigurations) methodParams[1]);
                            
                    }
                    break;
                    
                case "country":
                    switch(action)
                    {
                        case "list":
                            RolesManager.ValidateActionPermitted("country", "list", false);
                            return CountryController.List((KalturaCountryFilter) methodParams[0]);
                            
                    }
                    break;
                    
                case "coupon":
                    switch(action)
                    {
                        case "get":
                            RolesManager.ValidateActionPermitted("coupon", "get", false);
                            return CouponController.Get((string) methodParams[0]);
                            
                    }
                    break;
                    
                case "couponsgroup":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("couponsGroup", "add", false);
                            return CouponsGroupController.Add((KalturaCouponsGroup) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("couponsGroup", "delete", false);
                            return CouponsGroupController.Delete((long) methodParams[0]);
                            
                        case "generate":
                            RolesManager.ValidateActionPermitted("couponsGroup", "generate", false);
                            return CouponsGroupController.Generate((long) methodParams[0], (KalturaCouponGenerationOptions) methodParams[1]);
                            
                        case "get":
                            RolesManager.ValidateActionPermitted("couponsGroup", "get", false);
                            return CouponsGroupController.Get((long) methodParams[0]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("couponsGroup", "list", false);
                            return CouponsGroupController.List();
                            
                        case "update":
                            RolesManager.ValidateActionPermitted("couponsGroup", "update", false);
                            return CouponsGroupController.Update((long) methodParams[0], (KalturaCouponsGroup) methodParams[1]);
                            
                    }
                    break;
                    
                case "currency":
                    switch(action)
                    {
                        case "list":
                            RolesManager.ValidateActionPermitted("currency", "list", false);
                            return CurrencyController.List((KalturaCurrencyFilter) methodParams[0]);
                            
                    }
                    break;
                    
                case "devicebrand":
                    switch(action)
                    {
                        case "list":
                            RolesManager.ValidateActionPermitted("deviceBrand", "list", false);
                            return DeviceBrandController.List();
                            
                    }
                    break;
                    
                case "devicefamily":
                    switch(action)
                    {
                        case "list":
                            RolesManager.ValidateActionPermitted("deviceFamily", "list", false);
                            return DeviceFamilyController.List();
                            
                    }
                    break;
                    
                case "discountdetails":
                    switch(action)
                    {
                        case "list":
                            RolesManager.ValidateActionPermitted("discountDetails", "list", false);
                            return DiscountDetailsController.List((KalturaDiscountDetailsFilter) methodParams[0]);
                            
                    }
                    break;
                    
                case "drmprofile":
                    switch(action)
                    {
                        case "list":
                            RolesManager.ValidateActionPermitted("drmProfile", "list", false);
                            return DrmProfileController.List();
                            
                    }
                    break;
                    
                case "email":
                    switch(action)
                    {
                        case "send":
                            RolesManager.ValidateActionPermitted("email", "send", false);
                            return EmailController.Send((KalturaEmailMessage) methodParams[0]);
                            
                    }
                    break;
                    
                case "engagementadapter":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("engagementAdapter", "add", false);
                            return EngagementAdapterController.Add((KalturaEngagementAdapter) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("engagementAdapter", "delete", false);
                            return EngagementAdapterController.Delete((int) methodParams[0]);
                            
                        case "generatesharedsecret":
                            RolesManager.ValidateActionPermitted("engagementAdapter", "generateSharedSecret", false);
                            return EngagementAdapterController.GenerateSharedSecret((int) methodParams[0]);
                            
                        case "get":
                            RolesManager.ValidateActionPermitted("engagementAdapter", "get", false);
                            return EngagementAdapterController.Get((int) methodParams[0]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("engagementAdapter", "list", false);
                            return EngagementAdapterController.List();
                            
                        case "update":
                            RolesManager.ValidateActionPermitted("engagementAdapter", "update", false);
                            return EngagementAdapterController.Update((int) methodParams[0], (KalturaEngagementAdapter) methodParams[1]);
                            
                    }
                    break;
                    
                case "engagement":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("engagement", "add", false);
                            return EngagementController.Add((KalturaEngagement) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("engagement", "delete", false);
                            return EngagementController.Delete((int) methodParams[0]);
                            
                        case "get":
                            RolesManager.ValidateActionPermitted("engagement", "get", false);
                            return EngagementController.Get((int) methodParams[0]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("engagement", "list", false);
                            return EngagementController.List((KalturaEngagementFilter) methodParams[0]);
                            
                    }
                    break;
                    
                case "entitlement":
                    switch(action)
                    {
                        case "buy":
                            RolesManager.ValidateActionPermitted("entitlement", "buy", false);
                            return EntitlementController.Buy((string) methodParams[0], (bool) methodParams[1], (float) methodParams[2], (string) methodParams[3], (string) methodParams[4], (string) methodParams[5], (string) methodParams[6], (int) methodParams[7], (string) methodParams[8]);
                            
                        case "cancel":
                            RolesManager.ValidateActionPermitted("entitlement", "cancel", false);
                            return EntitlementController.Cancel((int) methodParams[0], (KalturaTransactionType) methodParams[1]);
                            
                        case "cancelrenewal":
                            RolesManager.ValidateActionPermitted("entitlement", "cancelRenewal", false);
                            EntitlementController.CancelRenewal((string) methodParams[0]);
                            return null;
                            
                        case "cancelscheduledsubscription":
                            RolesManager.ValidateActionPermitted("entitlement", "cancelScheduledSubscription", false);
                            return EntitlementController.CancelScheduledSubscription((long) methodParams[0]);
                            
                        case "externalreconcile":
                            RolesManager.ValidateActionPermitted("entitlement", "externalReconcile", false);
                            return EntitlementController.ExternalReconcile();
                            
                        case "forcecancel":
                            RolesManager.ValidateActionPermitted("entitlement", "forceCancel", false);
                            return EntitlementController.ForceCancel((int) methodParams[0], (KalturaTransactionType) methodParams[1]);
                            
                        case "getnextrenewal":
                            RolesManager.ValidateActionPermitted("entitlement", "getNextRenewal", false);
                            return EntitlementController.GetNextRenewal((int) methodParams[0]);
                            
                        case "grant":
                            RolesManager.ValidateActionPermitted("entitlement", "grant", false);
                            return EntitlementController.Grant((int) methodParams[0], (KalturaTransactionType) methodParams[1], (bool) methodParams[2], (int) methodParams[3]);
                            
                        case "list":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("entitlement", "list", false);
                                return EntitlementController.ListOldStandard((KalturaEntitlementsFilter) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("entitlement", "list", false);
                            return EntitlementController.List((KalturaEntitlementFilter) methodParams[0], (KalturaFilterPager) methodParams[1]);
                            
                        case "listexpired":
                            RolesManager.ValidateActionPermitted("entitlement", "listExpired", false);
                            return EntitlementController.ListExpired((KalturaEntitlementsFilter) methodParams[0], (KalturaFilterPager) methodParams[1]);
                            
                        case "listoldstandard":
                            RolesManager.ValidateActionPermitted("entitlement", "listOldStandard", false);
                            return EntitlementController.ListOldStandard((KalturaEntitlementsFilter) methodParams[0]);
                            
                        case "swap":
                            RolesManager.ValidateActionPermitted("entitlement", "swap", false);
                            return EntitlementController.Swap((int) methodParams[0], (int) methodParams[1], (bool) methodParams[2]);
                            
                        case "update":
                            RolesManager.ValidateActionPermitted("entitlement", "update", false);
                            return EntitlementController.Update((int) methodParams[0], (KalturaEntitlement) methodParams[1]);
                            
                    }
                    break;
                    
                case "epgchannel":
                    switch(action)
                    {
                        case "list":
                            RolesManager.ValidateActionPermitted("epgChannel", "list", false);
                            return EpgChannelController.List((KalturaEpgChannelFilter) methodParams[0], (List<KalturaCatalogWithHolder>) methodParams[1]);
                            
                    }
                    break;
                    
                case "exporttask":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("exportTask", "add", false);
                            return ExportTaskController.Add((KalturaExportTask) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("exportTask", "delete", false);
                            return ExportTaskController.Delete((long) methodParams[0]);
                            
                        case "get":
                            RolesManager.ValidateActionPermitted("exportTask", "get", false);
                            return ExportTaskController.Get((long) methodParams[0]);
                            
                        case "list":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("exportTask", "list", false);
                                return ExportTaskController.ListOldStandard((KalturaExportFilter) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("exportTask", "list", false);
                            return ExportTaskController.List((KalturaExportTaskFilter) methodParams[0]);
                            
                        case "listoldstandard":
                            RolesManager.ValidateActionPermitted("exportTask", "listOldStandard", false);
                            return ExportTaskController.ListOldStandard((KalturaExportFilter) methodParams[0]);
                            
                        case "update":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("exportTask", "update", false);
                                return ExportTaskController.UpdateOldStandard((KalturaExportTask) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("exportTask", "update", false);
                            return ExportTaskController.Update((long) methodParams[0], (KalturaExportTask) methodParams[1]);
                            
                        case "updateoldstandard":
                            RolesManager.ValidateActionPermitted("exportTask", "updateOldStandard", false);
                            return ExportTaskController.UpdateOldStandard((KalturaExportTask) methodParams[0]);
                            
                    }
                    break;
                    
                case "externalchannelprofile":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("externalChannelProfile", "add", false);
                            return ExternalChannelProfileController.Add((KalturaExternalChannelProfile) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("externalChannelProfile", "delete", false);
                            return ExternalChannelProfileController.Delete((int) methodParams[0]);
                            
                        case "list":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("externalChannelProfile", "list", false);
                                return ExternalChannelProfileController.ListOldStandard();
                            }
                            RolesManager.ValidateActionPermitted("externalChannelProfile", "list", false);
                            return ExternalChannelProfileController.List();
                            
                        case "listoldstandard":
                            RolesManager.ValidateActionPermitted("externalChannelProfile", "listOldStandard", false);
                            return ExternalChannelProfileController.ListOldStandard();
                            
                        case "update":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("externalChannelProfile", "update", false);
                                return ExternalChannelProfileController.UpdateOldStandard((KalturaExternalChannelProfile) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("externalChannelProfile", "update", false);
                            return ExternalChannelProfileController.Update((int) methodParams[0], (KalturaExternalChannelProfile) methodParams[1]);
                            
                        case "updateoldstandard":
                            RolesManager.ValidateActionPermitted("externalChannelProfile", "updateOldStandard", false);
                            return ExternalChannelProfileController.UpdateOldStandard((KalturaExternalChannelProfile) methodParams[0]);
                            
                    }
                    break;
                    
                case "favorite":
                    switch(action)
                    {
                        case "add":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("favorite", "add", false);
                                return FavoriteController.AddOldStandard((string) methodParams[0], (string) methodParams[1], (string) methodParams[2]);
                            }
                            RolesManager.ValidateActionPermitted("favorite", "add", false);
                            return FavoriteController.Add((KalturaFavorite) methodParams[0]);
                            
                        case "addoldstandard":
                            RolesManager.ValidateActionPermitted("favorite", "addOldStandard", false);
                            return FavoriteController.AddOldStandard((string) methodParams[0], (string) methodParams[1], (string) methodParams[2]);
                            
                        case "delete":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("favorite", "delete", false);
                                return FavoriteController.DeleteOldStandard((List<KalturaIntegerValue>) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("favorite", "delete", false);
                            return FavoriteController.Delete((int) methodParams[0]);
                            
                        case "deleteoldstandard":
                            RolesManager.ValidateActionPermitted("favorite", "deleteOldStandard", false);
                            return FavoriteController.DeleteOldStandard((List<KalturaIntegerValue>) methodParams[0]);
                            
                        case "list":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("favorite", "list", false);
                                return FavoriteController.ListOldStandard((KalturaFavoriteFilter) methodParams[0], (List<KalturaCatalogWithHolder>) methodParams[1], (string) methodParams[2]);
                            }
                            RolesManager.ValidateActionPermitted("favorite", "list", false);
                            return FavoriteController.List((KalturaFavoriteFilter) methodParams[0]);
                            
                        case "listoldstandard":
                            RolesManager.ValidateActionPermitted("favorite", "listOldStandard", false);
                            return FavoriteController.ListOldStandard((KalturaFavoriteFilter) methodParams[0], (List<KalturaCatalogWithHolder>) methodParams[1], (string) methodParams[2]);
                            
                    }
                    break;
                    
                case "followtvseries":
                    switch(action)
                    {
                        case "add":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("followTvSeries", "add", false);
                                return FollowTvSeriesController.AddOldStandard((int) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("followTvSeries", "add", false);
                            return FollowTvSeriesController.Add((KalturaFollowTvSeries) methodParams[0]);
                            
                        case "addoldstandard":
                            RolesManager.ValidateActionPermitted("followTvSeries", "addOldStandard", false);
                            return FollowTvSeriesController.AddOldStandard((int) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("followTvSeries", "delete", false);
                            return FollowTvSeriesController.Delete((int) methodParams[0]);
                            
                        case "deletewithtoken":
                            FollowTvSeriesController.DeleteWithToken((int) methodParams[0], (string) methodParams[1], (int) methodParams[2]);
                            return null;
                            
                        case "list":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("followTvSeries", "list", false);
                                return FollowTvSeriesController.ListOldStandard((Nullable<KalturaOrder>) methodParams[0], (KalturaFilterPager) methodParams[1]);
                            }
                            RolesManager.ValidateActionPermitted("followTvSeries", "list", false);
                            return FollowTvSeriesController.List((KalturaFollowTvSeriesFilter) methodParams[0], (KalturaFilterPager) methodParams[1]);
                            
                        case "listoldstandard":
                            RolesManager.ValidateActionPermitted("followTvSeries", "listOldStandard", false);
                            return FollowTvSeriesController.ListOldStandard((Nullable<KalturaOrder>) methodParams[0], (KalturaFilterPager) methodParams[1]);
                            
                    }
                    break;
                    
                case "homenetwork":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("homeNetwork", "add", false);
                            return HomeNetworkController.Add((KalturaHomeNetwork) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("homeNetwork", "delete", false);
                            return HomeNetworkController.Delete((string) methodParams[0]);
                            
                        case "list":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("homeNetwork", "list", false);
                                return HomeNetworkController.ListOldStandard();
                            }
                            RolesManager.ValidateActionPermitted("homeNetwork", "list", false);
                            return HomeNetworkController.List();
                            
                        case "listoldstandard":
                            RolesManager.ValidateActionPermitted("homeNetwork", "listOldStandard", false);
                            return HomeNetworkController.ListOldStandard();
                            
                        case "update":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("homeNetwork", "update", false);
                                return HomeNetworkController.UpdateOldStandard((KalturaHomeNetwork) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("homeNetwork", "update", false);
                            return HomeNetworkController.Update((string) methodParams[0], (KalturaHomeNetwork) methodParams[1]);
                            
                        case "updateoldstandard":
                            RolesManager.ValidateActionPermitted("homeNetwork", "updateOldStandard", false);
                            return HomeNetworkController.UpdateOldStandard((KalturaHomeNetwork) methodParams[0]);
                            
                    }
                    break;
                    
                case "household":
                    switch(action)
                    {
                        case "add":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("household", "add", false);
                                return HouseholdController.AddOldStandard((string) methodParams[0], (string) methodParams[1], (string) methodParams[2]);
                            }
                            RolesManager.ValidateActionPermitted("household", "add", false);
                            return HouseholdController.Add((KalturaHousehold) methodParams[0]);
                            
                        case "addoldstandard":
                            RolesManager.ValidateActionPermitted("household", "addOldStandard", false);
                            return HouseholdController.AddOldStandard((string) methodParams[0], (string) methodParams[1], (string) methodParams[2]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("household", "delete", false);
                            return HouseholdController.Delete((Nullable<int>) methodParams[0]);
                            
                        case "deletebyoperator":
                            RolesManager.ValidateActionPermitted("household", "deleteByOperator", false);
                            return HouseholdController.DeleteByOperator((KalturaIdentifierTypeFilter) methodParams[0]);
                            
                        case "get":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("household", "get", false);
                                return HouseholdController.GetOldStandard((List<KalturaHouseholdWithHolder>) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("household", "get", false);
                            return HouseholdController.Get((Nullable<int>) methodParams[0]);
                            
                        case "getbyoperator":
                            RolesManager.ValidateActionPermitted("household", "getByOperator", false);
                            return HouseholdController.GetByOperator((KalturaIdentifierTypeFilter) methodParams[0], (List<KalturaHouseholdWithHolder>) methodParams[1]);
                            
                        case "getchargeid":
                            RolesManager.ValidateActionPermitted("household", "getChargeID", false);
                            return HouseholdController.GetChargeID((string) methodParams[0]);
                            
                        case "getoldstandard":
                            RolesManager.ValidateActionPermitted("household", "getOldStandard", false);
                            return HouseholdController.GetOldStandard((List<KalturaHouseholdWithHolder>) methodParams[0]);
                            
                        case "purge":
                            RolesManager.ValidateActionPermitted("household", "purge", false);
                            return HouseholdController.Purge((int) methodParams[0]);
                            
                        case "resetfrequency":
                            RolesManager.ValidateActionPermitted("household", "resetFrequency", false);
                            return HouseholdController.ResetFrequency((KalturaHouseholdFrequencyType) methodParams[0]);
                            
                        case "resume":
                            RolesManager.ValidateActionPermitted("household", "resume", false);
                            return HouseholdController.Resume();
                            
                        case "setchargeid":
                            RolesManager.ValidateActionPermitted("household", "setChargeID", false);
                            return HouseholdController.SetChargeID((string) methodParams[0], (string) methodParams[1]);
                            
                        case "setpaymentmethodexternalid":
                            RolesManager.ValidateActionPermitted("household", "setPaymentMethodExternalId", false);
                            return HouseholdController.SetPaymentMethodExternalId((string) methodParams[0], (string) methodParams[1], (string) methodParams[2], (string) methodParams[3]);
                            
                        case "suspend":
                            RolesManager.ValidateActionPermitted("household", "suspend", false);
                            return HouseholdController.Suspend((Nullable<int>) methodParams[0]);
                            
                        case "update":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("household", "update", false);
                                return HouseholdController.UpdateOldStandard((string) methodParams[0], (string) methodParams[1]);
                            }
                            RolesManager.ValidateActionPermitted("household", "update", false);
                            return HouseholdController.Update((KalturaHousehold) methodParams[0]);
                            
                        case "updateoldstandard":
                            RolesManager.ValidateActionPermitted("household", "updateOldStandard", false);
                            return HouseholdController.UpdateOldStandard((string) methodParams[0], (string) methodParams[1]);
                            
                    }
                    break;
                    
                case "householddevice":
                    switch(action)
                    {
                        case "add":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("householdDevice", "add", false);
                                return HouseholdDeviceController.AddOldStandard((string) methodParams[0], (int) methodParams[1], (string) methodParams[2]);
                            }
                            RolesManager.ValidateActionPermitted("householdDevice", "add", false);
                            return HouseholdDeviceController.Add((KalturaHouseholdDevice) methodParams[0]);
                            
                        case "addbypin":
                            RolesManager.ValidateActionPermitted("householdDevice", "addByPin", false);
                            return HouseholdDeviceController.AddByPin((string) methodParams[0], (string) methodParams[1]);
                            
                        case "addoldstandard":
                            RolesManager.ValidateActionPermitted("householdDevice", "addOldStandard", false);
                            return HouseholdDeviceController.AddOldStandard((string) methodParams[0], (int) methodParams[1], (string) methodParams[2]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("householdDevice", "delete", false);
                            return HouseholdDeviceController.Delete((string) methodParams[0]);
                            
                        case "generatepin":
                            RolesManager.ValidateActionPermitted("householdDevice", "generatePin", false);
                            return HouseholdDeviceController.GeneratePin((string) methodParams[0], (int) methodParams[1]);
                            
                        case "get":
                            RolesManager.ValidateActionPermitted("householdDevice", "get", false);
                            return HouseholdDeviceController.Get();
                            
                        case "getstatus":
                            RolesManager.ValidateActionPermitted("householdDevice", "getStatus", false);
                            return HouseholdDeviceController.GetStatus((string) methodParams[0]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("householdDevice", "list", false);
                            return HouseholdDeviceController.List((KalturaHouseholdDeviceFilter) methodParams[0]);
                            
                        case "loginwithpin":
                            return HouseholdDeviceController.LoginWithPin((int) methodParams[0], (string) methodParams[1], (string) methodParams[2]);
                            
                        case "update":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("householdDevice", "update", false);
                                return HouseholdDeviceController.UpdateOldStandard((string) methodParams[0], (string) methodParams[1]);
                            }
                            RolesManager.ValidateActionPermitted("householdDevice", "update", false);
                            return HouseholdDeviceController.Update((string) methodParams[0], (KalturaHouseholdDevice) methodParams[1]);
                            
                        case "updateoldstandard":
                            RolesManager.ValidateActionPermitted("householdDevice", "updateOldStandard", false);
                            return HouseholdDeviceController.UpdateOldStandard((string) methodParams[0], (string) methodParams[1]);
                            
                        case "updatestatus":
                            RolesManager.ValidateActionPermitted("householdDevice", "updateStatus", false);
                            return HouseholdDeviceController.UpdateStatus((string) methodParams[0], (KalturaDeviceStatus) methodParams[1]);
                            
                    }
                    break;
                    
                case "householdlimitations":
                    switch(action)
                    {
                        case "get":
                            RolesManager.ValidateActionPermitted("householdLimitations", "get", false);
                            return HouseholdLimitationsController.Get((int) methodParams[0]);
                            
                    }
                    break;
                    
                case "householdpaymentgateway":
                    switch(action)
                    {
                        case "disable":
                            RolesManager.ValidateActionPermitted("householdPaymentGateway", "disable", false);
                            return HouseholdPaymentGatewayController.Disable((int) methodParams[0]);
                            
                        case "enable":
                            RolesManager.ValidateActionPermitted("householdPaymentGateway", "enable", false);
                            return HouseholdPaymentGatewayController.Enable((int) methodParams[0]);
                            
                        case "getchargeid":
                            RolesManager.ValidateActionPermitted("householdPaymentGateway", "getChargeID", false);
                            return HouseholdPaymentGatewayController.GetChargeID((string) methodParams[0]);
                            
                        case "invoke":
                            RolesManager.ValidateActionPermitted("householdPaymentGateway", "invoke", false);
                            return HouseholdPaymentGatewayController.Invoke((int) methodParams[0], (string) methodParams[1], (List<KalturaKeyValue>) methodParams[2]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("householdPaymentGateway", "list", false);
                            return HouseholdPaymentGatewayController.List();
                            
                        case "resume":
                            RolesManager.ValidateActionPermitted("householdPaymentGateway", "resume", false);
                            HouseholdPaymentGatewayController.Resume((int) methodParams[0]);
                            return null;
                            
                        case "setchargeid":
                            RolesManager.ValidateActionPermitted("householdPaymentGateway", "setChargeID", false);
                            return HouseholdPaymentGatewayController.SetChargeID((string) methodParams[0], (string) methodParams[1]);
                            
                        case "suspend":
                            RolesManager.ValidateActionPermitted("householdPaymentGateway", "suspend", false);
                            HouseholdPaymentGatewayController.Suspend((int) methodParams[0]);
                            return null;
                            
                        case "delete":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("householdPaymentGateway", "delete", false);
                                return HouseholdPaymentGatewayController.Disable((int) methodParams[0]);
                            }
                            break;
                            
                        case "set":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("householdPaymentGateway", "set", false);
                                return HouseholdPaymentGatewayController.Enable((int) methodParams[0]);
                            }
                            break;
                            
                    }
                    break;
                    
                case "householdpaymentmethod":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("householdPaymentMethod", "add", false);
                            return HouseholdPaymentMethodController.Add((KalturaHouseholdPaymentMethod) methodParams[0]);
                            
                        case "forceremove":
                            RolesManager.ValidateActionPermitted("householdPaymentMethod", "forceRemove", false);
                            return HouseholdPaymentMethodController.ForceRemove((int) methodParams[0], (int) methodParams[1]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("householdPaymentMethod", "list", false);
                            return HouseholdPaymentMethodController.List();
                            
                        case "remove":
                            RolesManager.ValidateActionPermitted("householdPaymentMethod", "remove", false);
                            return HouseholdPaymentMethodController.Remove((int) methodParams[0], (int) methodParams[1]);
                            
                        case "setasdefault":
                            RolesManager.ValidateActionPermitted("householdPaymentMethod", "setAsDefault", false);
                            return HouseholdPaymentMethodController.SetAsDefault((int) methodParams[0], (int) methodParams[1]);
                            
                        case "setexternalid":
                            RolesManager.ValidateActionPermitted("householdPaymentMethod", "setExternalId", false);
                            return HouseholdPaymentMethodController.SetExternalId((string) methodParams[0], (string) methodParams[1], (string) methodParams[2], (string) methodParams[3]);
                            
                    }
                    break;
                    
                case "householdpremiumservice":
                    switch(action)
                    {
                        case "list":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("householdPremiumService", "list", false);
                                return HouseholdPremiumServiceController.ListOldStandard();
                            }
                            RolesManager.ValidateActionPermitted("householdPremiumService", "list", false);
                            return HouseholdPremiumServiceController.List();
                            
                        case "listoldstandard":
                            RolesManager.ValidateActionPermitted("householdPremiumService", "listOldStandard", false);
                            return HouseholdPremiumServiceController.ListOldStandard();
                            
                    }
                    break;
                    
                case "householdquota":
                    switch(action)
                    {
                        case "get":
                            RolesManager.ValidateActionPermitted("householdQuota", "get", false);
                            return HouseholdQuotaController.Get();
                            
                    }
                    break;
                    
                case "householduser":
                    switch(action)
                    {
                        case "add":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("householdUser", "add", false);
                                return HouseholdUserController.AddOldStandard((string) methodParams[0], (bool) methodParams[1]);
                            }
                            RolesManager.ValidateActionPermitted("householdUser", "add", false);
                            return HouseholdUserController.Add((KalturaHouseholdUser) methodParams[0]);
                            
                        case "addbyoperator":
                            RolesManager.ValidateActionPermitted("householdUser", "addByOperator", false);
                            return HouseholdUserController.AddByOperator((string) methodParams[0], (int) methodParams[1], (bool) methodParams[2]);
                            
                        case "addoldstandard":
                            RolesManager.ValidateActionPermitted("householdUser", "addOldStandard", false);
                            return HouseholdUserController.AddOldStandard((string) methodParams[0], (bool) methodParams[1]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("householdUser", "delete", false);
                            return HouseholdUserController.Delete((string) methodParams[0]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("householdUser", "list", false);
                            return HouseholdUserController.List((KalturaHouseholdUserFilter) methodParams[0]);
                            
                    }
                    break;
                    
                case "image":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("image", "add", false);
                            return ImageController.Add((KalturaImage) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("image", "delete", false);
                            return ImageController.Delete((long) methodParams[0]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("image", "list", false);
                            return ImageController.List((KalturaImageFilter) methodParams[0]);
                            
                        case "setcontent":
                            RolesManager.ValidateActionPermitted("image", "setContent", false);
                            ImageController.SetContent((long) methodParams[0], (KalturaContentResource) methodParams[1]);
                            return null;
                            
                    }
                    break;
                    
                case "imagetype":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("imageType", "add", false);
                            return ImageTypeController.Add((KalturaImageType) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("imageType", "delete", false);
                            return ImageTypeController.Delete((long) methodParams[0]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("imageType", "list", false);
                            return ImageTypeController.List((KalturaImageTypeFilter) methodParams[0]);
                            
                        case "update":
                            RolesManager.ValidateActionPermitted("imageType", "update", false);
                            return ImageTypeController.Update((long) methodParams[0], (KalturaImageType) methodParams[1]);
                            
                    }
                    break;
                    
                case "inboxmessage":
                    switch(action)
                    {
                        case "get":
                            RolesManager.ValidateActionPermitted("inboxMessage", "get", false);
                            return InboxMessageController.Get((string) methodParams[0]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("inboxMessage", "list", false);
                            return InboxMessageController.List((KalturaInboxMessageFilter) methodParams[0], (KalturaFilterPager) methodParams[1]);
                            
                        case "updatestatus":
                            RolesManager.ValidateActionPermitted("inboxMessage", "updateStatus", false);
                            return InboxMessageController.UpdateStatus((string) methodParams[0], (KalturaInboxMessageStatus) methodParams[1]);
                            
                    }
                    break;
                    
                case "language":
                    switch(action)
                    {
                        case "list":
                            RolesManager.ValidateActionPermitted("language", "list", false);
                            return LanguageController.List((KalturaLanguageFilter) methodParams[0]);
                            
                    }
                    break;
                    
                case "lastposition":
                    switch(action)
                    {
                        case "list":
                            RolesManager.ValidateActionPermitted("lastPosition", "list", false);
                            return LastPositionController.List((KalturaLastPositionFilter) methodParams[0]);
                            
                    }
                    break;
                    
                case "licensedurl":
                    switch(action)
                    {
                        case "get":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("licensedUrl", "get", false);
                                return LicensedUrlController.GetOldStandard((KalturaAssetType) methodParams[0], (int) methodParams[1], (string) methodParams[2], (string) methodParams[3], (Nullable<long>) methodParams[4], (Nullable<KalturaStreamType>) methodParams[5]);
                            }
                            RolesManager.ValidateActionPermitted("licensedUrl", "get", false);
                            return LicensedUrlController.Get((KalturaLicensedUrlBaseRequest) methodParams[0]);
                            
                        case "getoldstandard":
                            RolesManager.ValidateActionPermitted("licensedUrl", "getOldStandard", false);
                            return LicensedUrlController.GetOldStandard((KalturaAssetType) methodParams[0], (int) methodParams[1], (string) methodParams[2], (string) methodParams[3], (Nullable<long>) methodParams[4], (Nullable<KalturaStreamType>) methodParams[5]);
                            
                    }
                    break;
                    
                case "mediaconcurrencyrule":
                    switch(action)
                    {
                        case "list":
                            RolesManager.ValidateActionPermitted("mediaConcurrencyRule", "list", false);
                            return MediaConcurrencyRuleController.List();
                            
                    }
                    break;
                    
                case "mediafile":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("mediaFile", "add", false);
                            return MediaFileController.Add((KalturaMediaFile) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("mediaFile", "delete", false);
                            return MediaFileController.Delete((long) methodParams[0]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("mediaFile", "list", false);
                            return MediaFileController.List((KalturaMediaFileFilter) methodParams[0]);
                            
                        case "update":
                            RolesManager.ValidateActionPermitted("mediaFile", "update", false);
                            return MediaFileController.Update((long) methodParams[0], (KalturaMediaFile) methodParams[1]);
                            
                    }
                    break;
                    
                case "mediafiletype":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("mediaFileType", "add", false);
                            return MediaFileTypeController.Add((KalturaMediaFileType) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("mediaFileType", "delete", false);
                            return MediaFileTypeController.Delete((int) methodParams[0]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("mediaFileType", "list", false);
                            return MediaFileTypeController.List();
                            
                        case "update":
                            RolesManager.ValidateActionPermitted("mediaFileType", "update", false);
                            return MediaFileTypeController.Update((int) methodParams[0], (KalturaMediaFileType) methodParams[1]);
                            
                    }
                    break;
                    
                case "messagetemplate":
                    switch(action)
                    {
                        case "get":
                            RolesManager.ValidateActionPermitted("messageTemplate", "get", false);
                            return MessageTemplateController.Get((KalturaMessageTemplateType) methodParams[0]);
                            
                        case "set":
                            RolesManager.ValidateActionPermitted("messageTemplate", "set", false);
                            return MessageTemplateController.Set((KalturaMessageTemplate) methodParams[0]);
                            
                        case "update":
                            RolesManager.ValidateActionPermitted("messageTemplate", "update", false);
                            return MessageTemplateController.Update((KalturaMessageTemplateType) methodParams[0], (KalturaMessageTemplate) methodParams[1]);
                            
                    }
                    break;
                    
                case "meta":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("meta", "add", false);
                            return MetaController.Add((KalturaMeta) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("meta", "delete", false);
                            return MetaController.Delete((long) methodParams[0]);
                            
                        case "list":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("meta", "list", false);
                                return MetaController.ListOldStandard((KalturaMetaFilter) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("meta", "list", false);
                            return MetaController.List((KalturaMetaFilter) methodParams[0]);
                            
                        case "listoldstandard":
                            RolesManager.ValidateActionPermitted("meta", "listOldStandard", false);
                            return MetaController.ListOldStandard((KalturaMetaFilter) methodParams[0]);
                            
                        case "update":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("meta", "update", false);
                                return MetaController.UpdateOldStandard((string) methodParams[0], (KalturaMeta) methodParams[1]);
                            }
                            RolesManager.ValidateActionPermitted("meta", "update", false);
                            return MetaController.Update((long) methodParams[0], (KalturaMeta) methodParams[1]);
                            
                        case "updateoldstandard":
                            RolesManager.ValidateActionPermitted("meta", "updateOldStandard", false);
                            return MetaController.UpdateOldStandard((string) methodParams[0], (KalturaMeta) methodParams[1]);
                            
                    }
                    break;
                    
                case "multirequest":
                    switch(action)
                    {
                        case "do":
                            return MultiRequestController.Do((KalturaMultiRequestAction[]) methodParams[0]);
                            
                    }
                    break;
                    
                case "notification":
                    switch(action)
                    {
                        case "register":
                            RolesManager.ValidateActionPermitted("notification", "register", false);
                            return NotificationController.Register((string) methodParams[0], (KalturaNotificationType) methodParams[1]);
                            
                        case "sendpush":
                            RolesManager.ValidateActionPermitted("notification", "sendPush", false);
                            return NotificationController.SendPush((int) methodParams[0], (KalturaPushMessage) methodParams[1]);
                            
                        case "sendsms":
                            RolesManager.ValidateActionPermitted("notification", "sendSms", false);
                            return NotificationController.SendSms((string) methodParams[0]);
                            
                        case "setdevicepushtoken":
                            RolesManager.ValidateActionPermitted("notification", "setDevicePushToken", false);
                            return NotificationController.SetDevicePushToken((string) methodParams[0]);
                            
                    }
                    break;
                    
                case "notificationspartnersettings":
                    switch(action)
                    {
                        case "get":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("notificationsPartnerSettings", "get", false);
                                return NotificationsPartnerSettingsController.GetOldStandard();
                            }
                            RolesManager.ValidateActionPermitted("notificationsPartnerSettings", "get", false);
                            return NotificationsPartnerSettingsController.Get();
                            
                        case "getoldstandard":
                            RolesManager.ValidateActionPermitted("notificationsPartnerSettings", "getOldStandard", false);
                            return NotificationsPartnerSettingsController.GetOldStandard();
                            
                        case "update":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("notificationsPartnerSettings", "update", false);
                                return NotificationsPartnerSettingsController.UpdateOldStandard((KalturaPartnerNotificationSettings) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("notificationsPartnerSettings", "update", false);
                            return NotificationsPartnerSettingsController.Update((KalturaNotificationsPartnerSettings) methodParams[0]);
                            
                        case "updateoldstandard":
                            RolesManager.ValidateActionPermitted("notificationsPartnerSettings", "updateOldStandard", false);
                            return NotificationsPartnerSettingsController.UpdateOldStandard((KalturaPartnerNotificationSettings) methodParams[0]);
                            
                    }
                    break;
                    
                case "notificationssettings":
                    switch(action)
                    {
                        case "get":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("notificationsSettings", "get", false);
                                return NotificationsSettingsController.GetOldStandard();
                            }
                            RolesManager.ValidateActionPermitted("notificationsSettings", "get", false);
                            return NotificationsSettingsController.Get();
                            
                        case "getoldstandard":
                            RolesManager.ValidateActionPermitted("notificationsSettings", "getOldStandard", false);
                            return NotificationsSettingsController.GetOldStandard();
                            
                        case "update":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("notificationsSettings", "update", false);
                                return NotificationsSettingsController.UpdateOldStandard((KalturaNotificationSettings) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("notificationsSettings", "update", false);
                            return NotificationsSettingsController.Update((KalturaNotificationsSettings) methodParams[0]);
                            
                        case "updateoldstandard":
                            RolesManager.ValidateActionPermitted("notificationsSettings", "updateOldStandard", false);
                            return NotificationsSettingsController.UpdateOldStandard((KalturaNotificationSettings) methodParams[0]);
                            
                        case "updatewithtoken":
                            return NotificationsSettingsController.UpdateWithToken((KalturaNotificationsSettings) methodParams[0], (string) methodParams[1], (int) methodParams[2]);
                            
                    }
                    break;
                    
                case "ossadapterprofile":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("ossAdapterProfile", "add", false);
                            return OssAdapterProfileController.Add((KalturaOSSAdapterProfile) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("ossAdapterProfile", "delete", false);
                            return OssAdapterProfileController.Delete((int) methodParams[0]);
                            
                        case "generatesharedsecret":
                            RolesManager.ValidateActionPermitted("ossAdapterProfile", "generateSharedSecret", false);
                            return OssAdapterProfileController.GenerateSharedSecret((int) methodParams[0]);
                            
                        case "get":
                            RolesManager.ValidateActionPermitted("ossAdapterProfile", "get", false);
                            return OssAdapterProfileController.Get((int) methodParams[0]);
                            
                        case "list":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("ossAdapterProfile", "list", false);
                                return OssAdapterProfileController.ListOldStandard();
                            }
                            RolesManager.ValidateActionPermitted("ossAdapterProfile", "list", false);
                            return OssAdapterProfileController.List();
                            
                        case "listoldstandard":
                            RolesManager.ValidateActionPermitted("ossAdapterProfile", "listOldStandard", false);
                            return OssAdapterProfileController.ListOldStandard();
                            
                        case "update":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("ossAdapterProfile", "update", false);
                                return OssAdapterProfileController.UpdateOldStandard((KalturaOSSAdapterProfile) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("ossAdapterProfile", "update", false);
                            return OssAdapterProfileController.Update((int) methodParams[0], (KalturaOSSAdapterProfile) methodParams[1]);
                            
                        case "updateoldstandard":
                            RolesManager.ValidateActionPermitted("ossAdapterProfile", "updateOldStandard", false);
                            return OssAdapterProfileController.UpdateOldStandard((KalturaOSSAdapterProfile) methodParams[0]);
                            
                    }
                    break;
                    
                case "ossadapterprofilesettings":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("ossAdapterProfileSettings", "add", false);
                            return OssAdapterProfileSettingsController.Add((int) methodParams[0], (SerializableDictionary<string, KalturaStringValue>) methodParams[1]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("ossAdapterProfileSettings", "delete", false);
                            return OssAdapterProfileSettingsController.Delete((int) methodParams[0], (SerializableDictionary<string, KalturaStringValue>) methodParams[1]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("ossAdapterProfileSettings", "list", false);
                            return OssAdapterProfileSettingsController.List();
                            
                        case "update":
                            RolesManager.ValidateActionPermitted("ossAdapterProfileSettings", "update", false);
                            return OssAdapterProfileSettingsController.Update((int) methodParams[0], (SerializableDictionary<string, KalturaStringValue>) methodParams[1]);
                            
                    }
                    break;
                    
                case "ottcategory":
                    switch(action)
                    {
                        case "get":
                            RolesManager.ValidateActionPermitted("ottCategory", "get", false);
                            return OttCategoryController.Get((int) methodParams[0]);
                            
                    }
                    break;
                    
                case "ottuser":
                    switch(action)
                    {
                        case "activate":
                            return OttUserController.Activate((int) methodParams[0], (string) methodParams[1], (string) methodParams[2]);
                            
                        case "addrole":
                            RolesManager.ValidateActionPermitted("ottUser", "addRole", false);
                            return OttUserController.AddRole((long) methodParams[0]);
                            
                        case "anonymouslogin":
                            return OttUserController.AnonymousLogin((int) methodParams[0], (string) methodParams[1]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("ottUser", "delete", false);
                            return OttUserController.Delete();
                            
                        case "facebooklogin":
                            return OttUserController.FacebookLogin((int) methodParams[0], (string) methodParams[1], (string) methodParams[2]);
                            
                        case "get":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("ottUser", "get", false);
                                return OttUserController.GetOldStandard();
                            }
                            RolesManager.ValidateActionPermitted("ottUser", "get", false);
                            return OttUserController.Get();
                            
                        case "getencrypteduserid":
                            RolesManager.ValidateActionPermitted("ottUser", "getEncryptedUserId", false);
                            return OttUserController.GetEncryptedUserId();
                            
                        case "getoldstandard":
                            RolesManager.ValidateActionPermitted("ottUser", "getOldStandard", false);
                            return OttUserController.GetOldStandard();
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("ottUser", "list", false);
                            return OttUserController.List((KalturaOTTUserFilter) methodParams[0]);
                            
                        case "login":
                            if(HttpContext.Current.Request.HttpMethod.ToLower() == "get")
                            {
                                throw new BadRequestException(BadRequestException.HTTP_METHOD_NOT_SUPPORTED, HttpContext.Current.Request.HttpMethod.ToUpper());
                            }
                            return OttUserController.Login((int) methodParams[0], (string) methodParams[1], (string) methodParams[2], (SerializableDictionary<string, KalturaStringValue>) methodParams[3], (string) methodParams[4]);
                            
                        case "loginwithpin":
                            if(HttpContext.Current.Request.HttpMethod.ToLower() == "get")
                            {
                                throw new BadRequestException(BadRequestException.HTTP_METHOD_NOT_SUPPORTED, HttpContext.Current.Request.HttpMethod.ToUpper());
                            }
                            return OttUserController.LoginWithPin((int) methodParams[0], (string) methodParams[1], (string) methodParams[2], (string) methodParams[3]);
                            
                        case "logout":
                            RolesManager.ValidateActionPermitted("ottUser", "logout", false);
                            return OttUserController.Logout();
                            
                        case "refreshsession":
                            RolesManager.ValidateActionPermitted("ottUser", "refreshSession", true);
                            return OttUserController.RefreshSession((string) methodParams[0], (string) methodParams[1]);
                            
                        case "register":
                            return OttUserController.Register((int) methodParams[0], (KalturaOTTUser) methodParams[1], (string) methodParams[2]);
                            
                        case "resendactivationtoken":
                            return OttUserController.ResendActivationToken((int) methodParams[0], (string) methodParams[1]);
                            
                        case "resetpassword":
                            if(isOldVersion)
                            {
                                return OttUserController.setPassword((int) methodParams[0], (string) methodParams[1], (string) methodParams[2]);
                            }
                            return OttUserController.resetPassword((int) methodParams[0], (string) methodParams[1], (string) methodParams[2]);
                            
                        case "setinitialpassword":
                            if(HttpContext.Current.Request.HttpMethod.ToLower() == "get")
                            {
                                throw new BadRequestException(BadRequestException.HTTP_METHOD_NOT_SUPPORTED, HttpContext.Current.Request.HttpMethod.ToUpper());
                            }
                            return OttUserController.setInitialPassword((int) methodParams[0], (string) methodParams[1], (string) methodParams[2]);
                            
                        case "setpassword":
                            return OttUserController.setPassword((int) methodParams[0], (string) methodParams[1], (string) methodParams[2]);
                            
                        case "update":
                            RolesManager.ValidateActionPermitted("ottUser", "update", false);
                            return OttUserController.Update((KalturaOTTUser) methodParams[0], (string) methodParams[1]);
                            
                        case "updatedynamicdata":
                            RolesManager.ValidateActionPermitted("ottUser", "updateDynamicData", false);
                            return OttUserController.UpdateDynamicData((string) methodParams[0], (KalturaStringValue) methodParams[1]);
                            
                        case "updatelogindata":
                            if(HttpContext.Current.Request.HttpMethod.ToLower() == "get")
                            {
                                throw new BadRequestException(BadRequestException.HTTP_METHOD_NOT_SUPPORTED, HttpContext.Current.Request.HttpMethod.ToUpper());
                            }
                            RolesManager.ValidateActionPermitted("ottUser", "updateLoginData", false);
                            return OttUserController.UpdateLoginData((string) methodParams[0], (string) methodParams[1], (string) methodParams[2]);
                            
                        case "updatepassword":
                            if(HttpContext.Current.Request.HttpMethod.ToLower() == "get")
                            {
                                throw new BadRequestException(BadRequestException.HTTP_METHOD_NOT_SUPPORTED, HttpContext.Current.Request.HttpMethod.ToUpper());
                            }
                            RolesManager.ValidateActionPermitted("ottUser", "updatePassword", true);
                            OttUserController.updatePassword((int) methodParams[0], (string) methodParams[1]);
                            return null;
                            
                        case "validatetoken":
                            return OttUserController.validateToken((int) methodParams[0], (string) methodParams[1]);
                            
                        case "add":
                            if(isOldVersion)
                            {
                                return OttUserController.Register((int) methodParams[0], (KalturaOTTUser) methodParams[1], (string) methodParams[2]);
                            }
                            break;
                            
                        case "sendPassword":
                            if(isOldVersion)
                            {
                                return OttUserController.resetPassword((int) methodParams[0], (string) methodParams[1], (string) methodParams[2]);
                            }
                            break;
                            
                        case "changePassword":
                            if(isOldVersion)
                            {
                                if(HttpContext.Current.Request.HttpMethod.ToLower() == "get")
                                {
                                    throw new BadRequestException(BadRequestException.HTTP_METHOD_NOT_SUPPORTED, HttpContext.Current.Request.HttpMethod.ToUpper());
                                }
                                RolesManager.ValidateActionPermitted("ottUser", "changePassword", false);
                                return OttUserController.UpdateLoginData((string) methodParams[0], (string) methodParams[1], (string) methodParams[2]);
                            }
                            break;
                            
                    }
                    break;
                    
                case "parentalrule":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("parentalRule", "add", false);
                            return ParentalRuleController.Add((KalturaParentalRule) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("parentalRule", "delete", false);
                            return ParentalRuleController.Delete((long) methodParams[0]);
                            
                        case "disable":
                            RolesManager.ValidateActionPermitted("parentalRule", "disable", false);
                            return ParentalRuleController.Disable((long) methodParams[0], (KalturaEntityReferenceBy) methodParams[1]);
                            
                        case "disabledefault":
                            RolesManager.ValidateActionPermitted("parentalRule", "disableDefault", false);
                            return ParentalRuleController.DisableDefault((KalturaEntityReferenceBy) methodParams[0]);
                            
                        case "enable":
                            RolesManager.ValidateActionPermitted("parentalRule", "enable", false);
                            return ParentalRuleController.Enable((long) methodParams[0], (KalturaEntityReferenceBy) methodParams[1]);
                            
                        case "get":
                            RolesManager.ValidateActionPermitted("parentalRule", "get", false);
                            return ParentalRuleController.Get((long) methodParams[0]);
                            
                        case "list":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("parentalRule", "list", false);
                                return ParentalRuleController.ListOldStandard((KalturaRuleFilter) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("parentalRule", "list", false);
                            return ParentalRuleController.List((KalturaParentalRuleFilter) methodParams[0]);
                            
                        case "listoldstandard":
                            RolesManager.ValidateActionPermitted("parentalRule", "listOldStandard", false);
                            return ParentalRuleController.ListOldStandard((KalturaRuleFilter) methodParams[0]);
                            
                        case "update":
                            RolesManager.ValidateActionPermitted("parentalRule", "update", false);
                            return ParentalRuleController.Update((long) methodParams[0], (KalturaParentalRule) methodParams[1]);
                            
                    }
                    break;
                    
                case "parentalruleprofile":
                    switch(action)
                    {
                        case "list":
                            RolesManager.ValidateActionPermitted("parentalRuleProfile", "list", false);
                            return ParentalRuleProfileController.List();
                            
                    }
                    break;
                    
                case "partnerconfiguration":
                    switch(action)
                    {
                        case "list":
                            RolesManager.ValidateActionPermitted("partnerConfiguration", "list", false);
                            return PartnerConfigurationController.List((KalturaPartnerConfigurationFilter) methodParams[0]);
                            
                        case "update":
                            RolesManager.ValidateActionPermitted("partnerConfiguration", "update", false);
                            return PartnerConfigurationController.Update((KalturaPartnerConfiguration) methodParams[0]);
                            
                    }
                    break;
                    
                case "paymentgateway":
                    switch(action)
                    {
                        case "delete":
                            RolesManager.ValidateActionPermitted("paymentGateway", "delete", false);
                            return PaymentGatewayController.Delete((int) methodParams[0]);
                            
                        case "forceremovepaymentmethod":
                            RolesManager.ValidateActionPermitted("paymentGateway", "forceRemovePaymentMethod", false);
                            return PaymentGatewayController.ForceRemovePaymentMethod((int) methodParams[0], (int) methodParams[1]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("paymentGateway", "list", false);
                            return PaymentGatewayController.List();
                            
                        case "removepaymentmethod":
                            RolesManager.ValidateActionPermitted("paymentGateway", "removePaymentMethod", false);
                            return PaymentGatewayController.RemovePaymentMethod((int) methodParams[0], (int) methodParams[1]);
                            
                        case "set":
                            RolesManager.ValidateActionPermitted("paymentGateway", "set", false);
                            return PaymentGatewayController.Set((int) methodParams[0]);
                            
                        case "setpaymentmethod":
                            RolesManager.ValidateActionPermitted("paymentGateway", "setPaymentMethod", false);
                            return PaymentGatewayController.SetPaymentMethod((int) methodParams[0], (int) methodParams[1]);
                            
                    }
                    break;
                    
                case "paymentgatewayprofile":
                    switch(action)
                    {
                        case "add":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("paymentGatewayProfile", "add", false);
                                return PaymentGatewayProfileController.AddOldStandard((KalturaPaymentGatewayProfile) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("paymentGatewayProfile", "add", false);
                            return PaymentGatewayProfileController.Add((KalturaPaymentGatewayProfile) methodParams[0]);
                            
                        case "addoldstandard":
                            RolesManager.ValidateActionPermitted("paymentGatewayProfile", "addOldStandard", false);
                            return PaymentGatewayProfileController.AddOldStandard((KalturaPaymentGatewayProfile) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("paymentGatewayProfile", "delete", false);
                            return PaymentGatewayProfileController.Delete((int) methodParams[0]);
                            
                        case "generatesharedsecret":
                            RolesManager.ValidateActionPermitted("paymentGatewayProfile", "generateSharedSecret", false);
                            return PaymentGatewayProfileController.GenerateSharedSecret((int) methodParams[0]);
                            
                        case "getconfiguration":
                            RolesManager.ValidateActionPermitted("paymentGatewayProfile", "getConfiguration", false);
                            return PaymentGatewayProfileController.GetConfiguration((string) methodParams[0], (string) methodParams[1], (List<KalturaKeyValue>) methodParams[2]);
                            
                        case "list":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("paymentGatewayProfile", "list", false);
                                return PaymentGatewayProfileController.ListOldStandard();
                            }
                            RolesManager.ValidateActionPermitted("paymentGatewayProfile", "list", false);
                            return PaymentGatewayProfileController.List();
                            
                        case "listoldstandard":
                            RolesManager.ValidateActionPermitted("paymentGatewayProfile", "listOldStandard", false);
                            return PaymentGatewayProfileController.ListOldStandard();
                            
                        case "update":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("paymentGatewayProfile", "update", false);
                                return PaymentGatewayProfileController.UpdateOldStandard((int) methodParams[0], (KalturaPaymentGatewayProfile) methodParams[1]);
                            }
                            RolesManager.ValidateActionPermitted("paymentGatewayProfile", "update", false);
                            return PaymentGatewayProfileController.Update((int) methodParams[0], (KalturaPaymentGatewayProfile) methodParams[1]);
                            
                        case "updateoldstandard":
                            RolesManager.ValidateActionPermitted("paymentGatewayProfile", "updateOldStandard", false);
                            return PaymentGatewayProfileController.UpdateOldStandard((int) methodParams[0], (KalturaPaymentGatewayProfile) methodParams[1]);
                            
                    }
                    break;
                    
                case "paymentgatewayprofilesettings":
                    switch(action)
                    {
                        case "add":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("paymentGatewayProfileSettings", "add", false);
                                return PaymentGatewayProfileSettingsController.AddOldStandard((int) methodParams[0], (SerializableDictionary<string, KalturaStringValue>) methodParams[1]);
                            }
                            RolesManager.ValidateActionPermitted("paymentGatewayProfileSettings", "add", false);
                            return PaymentGatewayProfileSettingsController.Add((KalturaPaymentGatewayProfile) methodParams[0]);
                            
                        case "addoldstandard":
                            RolesManager.ValidateActionPermitted("paymentGatewayProfileSettings", "addOldStandard", false);
                            return PaymentGatewayProfileSettingsController.AddOldStandard((int) methodParams[0], (SerializableDictionary<string, KalturaStringValue>) methodParams[1]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("paymentGatewayProfileSettings", "delete", false);
                            return PaymentGatewayProfileSettingsController.Delete((int) methodParams[0], (SerializableDictionary<string, KalturaStringValue>) methodParams[1]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("paymentGatewayProfileSettings", "list", false);
                            return PaymentGatewayProfileSettingsController.List();
                            
                        case "update":
                            RolesManager.ValidateActionPermitted("paymentGatewayProfileSettings", "update", false);
                            return PaymentGatewayProfileSettingsController.Update((int) methodParams[0], (SerializableDictionary<string, KalturaStringValue>) methodParams[1]);
                            
                    }
                    break;
                    
                case "paymentmethodprofile":
                    switch(action)
                    {
                        case "add":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("paymentMethodProfile", "add", false);
                                return PaymentMethodProfileController.AddOldStandard((int) methodParams[0], (KalturaPaymentMethodProfile) methodParams[1]);
                            }
                            RolesManager.ValidateActionPermitted("paymentMethodProfile", "add", false);
                            return PaymentMethodProfileController.Add((KalturaPaymentMethodProfile) methodParams[0]);
                            
                        case "addoldstandard":
                            RolesManager.ValidateActionPermitted("paymentMethodProfile", "addOldStandard", false);
                            return PaymentMethodProfileController.AddOldStandard((int) methodParams[0], (KalturaPaymentMethodProfile) methodParams[1]);
                            
                        case "delete":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("paymentMethodProfile", "delete", false);
                                return PaymentMethodProfileController.DeleteOldStandard((int) methodParams[0], (int) methodParams[1]);
                            }
                            RolesManager.ValidateActionPermitted("paymentMethodProfile", "delete", false);
                            return PaymentMethodProfileController.Delete((int) methodParams[0]);
                            
                        case "deleteoldstandard":
                            RolesManager.ValidateActionPermitted("paymentMethodProfile", "deleteOldStandard", false);
                            return PaymentMethodProfileController.DeleteOldStandard((int) methodParams[0], (int) methodParams[1]);
                            
                        case "list":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("paymentMethodProfile", "list", false);
                                return PaymentMethodProfileController.ListOldStandard((int) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("paymentMethodProfile", "list", false);
                            return PaymentMethodProfileController.List((KalturaPaymentMethodProfileFilter) methodParams[0]);
                            
                        case "listoldstandard":
                            RolesManager.ValidateActionPermitted("paymentMethodProfile", "listOldStandard", false);
                            return PaymentMethodProfileController.ListOldStandard((int) methodParams[0]);
                            
                        case "update":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("paymentMethodProfile", "update", false);
                                return PaymentMethodProfileController.UpdateOldStandard((int) methodParams[0], (KalturaPaymentMethodProfile) methodParams[1]);
                            }
                            RolesManager.ValidateActionPermitted("paymentMethodProfile", "update", false);
                            return PaymentMethodProfileController.Update((int) methodParams[0], (KalturaPaymentMethodProfile) methodParams[1]);
                            
                        case "updateoldstandard":
                            RolesManager.ValidateActionPermitted("paymentMethodProfile", "updateOldStandard", false);
                            return PaymentMethodProfileController.UpdateOldStandard((int) methodParams[0], (KalturaPaymentMethodProfile) methodParams[1]);
                            
                    }
                    break;
                    
                case "permission":
                    switch(action)
                    {
                        case "getcurrentpermissions":
                            RolesManager.ValidateActionPermitted("permission", "getCurrentPermissions", false);
                            return PermissionController.GetCurrentPermissions();
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("permission", "list", false);
                            return PermissionController.List((KalturaPermissionFilter) methodParams[0]);
                            
                    }
                    break;
                    
                case "personalasset":
                    switch(action)
                    {
                        case "list":
                            RolesManager.ValidateActionPermitted("personalAsset", "list", false);
                            return PersonalAssetController.List((List<KalturaPersonalAssetRequest>) methodParams[0], (List<KalturaPersonalAssetWithHolder>) methodParams[1]);
                            
                    }
                    break;
                    
                case "personalfeed":
                    switch(action)
                    {
                        case "list":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("personalFeed", "list", false);
                                return PersonalFeedController.ListOldStandard((Nullable<KalturaOrder>) methodParams[0], (KalturaFilterPager) methodParams[1]);
                            }
                            RolesManager.ValidateActionPermitted("personalFeed", "list", false);
                            return PersonalFeedController.List((KalturaPersonalFeedFilter) methodParams[0], (KalturaFilterPager) methodParams[1]);
                            
                        case "listoldstandard":
                            RolesManager.ValidateActionPermitted("personalFeed", "listOldStandard", false);
                            return PersonalFeedController.ListOldStandard((Nullable<KalturaOrder>) methodParams[0], (KalturaFilterPager) methodParams[1]);
                            
                    }
                    break;
                    
                case "personallist":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("personalList", "add", false);
                            return PersonalListController.Add((KalturaPersonalList) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("personalList", "delete", false);
                            PersonalListController.Delete((long) methodParams[0]);
                            return null;
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("personalList", "list", false);
                            return PersonalListController.List((KalturaPersonalListFilter) methodParams[0], (KalturaFilterPager) methodParams[1]);
                            
                    }
                    break;
                    
                case "pin":
                    switch(action)
                    {
                        case "get":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("pin", "get", false);
                                return PinController.GetOldStandard((KalturaEntityReferenceBy) methodParams[0], (KalturaPinType) methodParams[1], (Nullable<int>) methodParams[2]);
                            }
                            RolesManager.ValidateActionPermitted("pin", "get", false);
                            return PinController.Get((KalturaEntityReferenceBy) methodParams[0], (KalturaPinType) methodParams[1], (Nullable<int>) methodParams[2]);
                            
                        case "getoldstandard":
                            RolesManager.ValidateActionPermitted("pin", "getOldStandard", false);
                            return PinController.GetOldStandard((KalturaEntityReferenceBy) methodParams[0], (KalturaPinType) methodParams[1], (Nullable<int>) methodParams[2]);
                            
                        case "update":
                            if(isOldVersion)
                            {
                                if(HttpContext.Current.Request.HttpMethod.ToLower() == "get")
                                {
                                    throw new BadRequestException(BadRequestException.HTTP_METHOD_NOT_SUPPORTED, HttpContext.Current.Request.HttpMethod.ToUpper());
                                }
                                RolesManager.ValidateActionPermitted("pin", "update", false);
                                return PinController.UpdateOldStandard((string) methodParams[0], (KalturaEntityReferenceBy) methodParams[1], (KalturaPinType) methodParams[2], (Nullable<int>) methodParams[3]);
                            }
                            if(HttpContext.Current.Request.HttpMethod.ToLower() == "get")
                            {
                                throw new BadRequestException(BadRequestException.HTTP_METHOD_NOT_SUPPORTED, HttpContext.Current.Request.HttpMethod.ToUpper());
                            }
                            RolesManager.ValidateActionPermitted("pin", "update", false);
                            return PinController.Update((KalturaEntityReferenceBy) methodParams[0], (KalturaPinType) methodParams[1], (KalturaPin) methodParams[2], (Nullable<int>) methodParams[3]);
                            
                        case "updateoldstandard":
                            if(HttpContext.Current.Request.HttpMethod.ToLower() == "get")
                            {
                                throw new BadRequestException(BadRequestException.HTTP_METHOD_NOT_SUPPORTED, HttpContext.Current.Request.HttpMethod.ToUpper());
                            }
                            RolesManager.ValidateActionPermitted("pin", "updateOldStandard", false);
                            return PinController.UpdateOldStandard((string) methodParams[0], (KalturaEntityReferenceBy) methodParams[1], (KalturaPinType) methodParams[2], (Nullable<int>) methodParams[3]);
                            
                        case "validate":
                            if(HttpContext.Current.Request.HttpMethod.ToLower() == "get")
                            {
                                throw new BadRequestException(BadRequestException.HTTP_METHOD_NOT_SUPPORTED, HttpContext.Current.Request.HttpMethod.ToUpper());
                            }
                            RolesManager.ValidateActionPermitted("pin", "validate", false);
                            return PinController.Validate((string) methodParams[0], (KalturaPinType) methodParams[1], (Nullable<int>) methodParams[2]);
                            
                    }
                    break;
                    
                case "ppv":
                    switch(action)
                    {
                        case "get":
                            RolesManager.ValidateActionPermitted("ppv", "get", false);
                            return PpvController.Get((long) methodParams[0]);
                            
                    }
                    break;
                    
                case "price":
                    switch(action)
                    {
                        case "list":
                            RolesManager.ValidateActionPermitted("price", "list", false);
                            return PriceController.List((KalturaPricesFilter) methodParams[0], (string) methodParams[1]);
                            
                    }
                    break;
                    
                case "pricedetails":
                    switch(action)
                    {
                        case "list":
                            RolesManager.ValidateActionPermitted("priceDetails", "list", false);
                            return PriceDetailsController.List((KalturaPriceDetailsFilter) methodParams[0]);
                            
                    }
                    break;
                    
                case "priceplan":
                    switch(action)
                    {
                        case "list":
                            RolesManager.ValidateActionPermitted("pricePlan", "list", false);
                            return PricePlanController.List((KalturaPricePlanFilter) methodParams[0]);
                            
                        case "update":
                            RolesManager.ValidateActionPermitted("pricePlan", "update", false);
                            return PricePlanController.Update((long) methodParams[0], (KalturaPricePlan) methodParams[1]);
                            
                    }
                    break;
                    
                case "productprice":
                    switch(action)
                    {
                        case "list":
                            RolesManager.ValidateActionPermitted("productPrice", "list", false);
                            return ProductPriceController.List((KalturaProductPriceFilter) methodParams[0]);
                            
                    }
                    break;
                    
                case "purchasesettings":
                    switch(action)
                    {
                        case "get":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("purchaseSettings", "get", false);
                                return PurchaseSettingsController.GetOldStandard((KalturaEntityReferenceBy) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("purchaseSettings", "get", false);
                            return PurchaseSettingsController.Get((KalturaEntityReferenceBy) methodParams[0]);
                            
                        case "getoldstandard":
                            RolesManager.ValidateActionPermitted("purchaseSettings", "getOldStandard", false);
                            return PurchaseSettingsController.GetOldStandard((KalturaEntityReferenceBy) methodParams[0]);
                            
                        case "update":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("purchaseSettings", "update", false);
                                return PurchaseSettingsController.UpdateOldStandard((int) methodParams[0], (KalturaEntityReferenceBy) methodParams[1]);
                            }
                            RolesManager.ValidateActionPermitted("purchaseSettings", "update", false);
                            return PurchaseSettingsController.Update((KalturaEntityReferenceBy) methodParams[0], (KalturaPurchaseSettings) methodParams[1]);
                            
                        case "updateoldstandard":
                            RolesManager.ValidateActionPermitted("purchaseSettings", "updateOldStandard", false);
                            return PurchaseSettingsController.UpdateOldStandard((int) methodParams[0], (KalturaEntityReferenceBy) methodParams[1]);
                            
                    }
                    break;
                    
                case "ratio":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("ratio", "add", false);
                            return RatioController.Add((KalturaRatio) methodParams[0]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("ratio", "list", false);
                            return RatioController.List();
                            
                        case "update":
                            RolesManager.ValidateActionPermitted("ratio", "update", false);
                            return RatioController.Update((long) methodParams[0], (KalturaRatio) methodParams[1]);
                            
                    }
                    break;
                    
                case "recommendationprofile":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("recommendationProfile", "add", false);
                            return RecommendationProfileController.Add((KalturaRecommendationProfile) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("recommendationProfile", "delete", false);
                            return RecommendationProfileController.Delete((int) methodParams[0]);
                            
                        case "generatesharedsecret":
                            RolesManager.ValidateActionPermitted("recommendationProfile", "generateSharedSecret", false);
                            return RecommendationProfileController.GenerateSharedSecret((int) methodParams[0]);
                            
                        case "list":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("recommendationProfile", "list", false);
                                return RecommendationProfileController.ListOldStandard();
                            }
                            RolesManager.ValidateActionPermitted("recommendationProfile", "list", false);
                            return RecommendationProfileController.List();
                            
                        case "listoldstandard":
                            RolesManager.ValidateActionPermitted("recommendationProfile", "listOldStandard", false);
                            return RecommendationProfileController.ListOldStandard();
                            
                        case "update":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("recommendationProfile", "update", false);
                                return RecommendationProfileController.UpdateOldStandard((KalturaRecommendationProfile) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("recommendationProfile", "update", false);
                            return RecommendationProfileController.Update((int) methodParams[0], (KalturaRecommendationProfile) methodParams[1]);
                            
                        case "updateoldstandard":
                            RolesManager.ValidateActionPermitted("recommendationProfile", "updateOldStandard", false);
                            return RecommendationProfileController.UpdateOldStandard((KalturaRecommendationProfile) methodParams[0]);
                            
                    }
                    break;
                    
                case "recommendationprofilesettings":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("recommendationProfileSettings", "add", false);
                            return RecommendationProfileSettingsController.Add((int) methodParams[0], (SerializableDictionary<string, KalturaStringValue>) methodParams[1]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("recommendationProfileSettings", "delete", false);
                            return RecommendationProfileSettingsController.Delete((int) methodParams[0], (SerializableDictionary<string, KalturaStringValue>) methodParams[1]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("recommendationProfileSettings", "list", false);
                            return RecommendationProfileSettingsController.List();
                            
                        case "update":
                            RolesManager.ValidateActionPermitted("recommendationProfileSettings", "update", false);
                            return RecommendationProfileSettingsController.Update((int) methodParams[0], (SerializableDictionary<string, KalturaStringValue>) methodParams[1]);
                            
                    }
                    break;
                    
                case "recording":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("recording", "add", false);
                            return RecordingController.Add((KalturaRecording) methodParams[0]);
                            
                        case "cancel":
                            RolesManager.ValidateActionPermitted("recording", "cancel", false);
                            return RecordingController.Cancel((long) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("recording", "delete", false);
                            return RecordingController.Delete((long) methodParams[0]);
                            
                        case "get":
                            RolesManager.ValidateActionPermitted("recording", "get", false);
                            return RecordingController.Get((long) methodParams[0]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("recording", "list", false);
                            return RecordingController.List((KalturaRecordingFilter) methodParams[0], (KalturaFilterPager) methodParams[1]);
                            
                        case "protect":
                            RolesManager.ValidateActionPermitted("recording", "protect", false);
                            return RecordingController.Protect((long) methodParams[0]);
                            
                    }
                    break;
                    
                case "region":
                    switch(action)
                    {
                        case "list":
                            RolesManager.ValidateActionPermitted("region", "list", false);
                            return RegionController.List((KalturaRegionFilter) methodParams[0]);
                            
                    }
                    break;
                    
                case "registrysettings":
                    switch(action)
                    {
                        case "list":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("registrySettings", "list", false);
                                return RegistrySettingsController.ListOldStandard();
                            }
                            RolesManager.ValidateActionPermitted("registrySettings", "list", false);
                            return RegistrySettingsController.List();
                            
                        case "listoldstandard":
                            RolesManager.ValidateActionPermitted("registrySettings", "listOldStandard", false);
                            return RegistrySettingsController.ListOldStandard();
                            
                    }
                    break;
                    
                case "reminder":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("reminder", "add", false);
                            return ReminderController.Add((KalturaReminder) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("reminder", "delete", false);
                            return ReminderController.Delete((long) methodParams[0], (KalturaReminderType) methodParams[1]);
                            
                        case "deletewithtoken":
                            ReminderController.DeleteWithToken((long) methodParams[0], (KalturaReminderType) methodParams[1], (string) methodParams[2], (int) methodParams[3]);
                            return null;
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("reminder", "list", false);
                            return ServiceController.ExecGeneric(typeof(ReminderController).GetMethod("List"), methodParams);
                            
                    }
                    break;
                    
                case "report":
                    switch(action)
                    {
                        case "get":
                            RolesManager.ValidateActionPermitted("report", "get", false);
                            return ReportController.Get((string) methodParams[0]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("report", "list", false);
                            return ReportController.List((KalturaReportFilter) methodParams[0], (KalturaFilterPager) methodParams[1]);
                            
                    }
                    break;
                    
                case "searchhistory":
                    switch(action)
                    {
                        case "clean":
                            RolesManager.ValidateActionPermitted("searchHistory", "clean", false);
                            return SearchHistoryController.Clean((KalturaSearchHistoryFilter) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("searchHistory", "delete", false);
                            return SearchHistoryController.Delete((string) methodParams[0]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("searchHistory", "list", false);
                            return SearchHistoryController.List((KalturaSearchHistoryFilter) methodParams[0], (KalturaFilterPager) methodParams[1]);
                            
                    }
                    break;
                    
                case "seriesrecording":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("seriesRecording", "add", false);
                            return SeriesRecordingController.Add((KalturaSeriesRecording) methodParams[0]);
                            
                        case "cancel":
                            RolesManager.ValidateActionPermitted("seriesRecording", "cancel", false);
                            return SeriesRecordingController.Cancel((long) methodParams[0]);
                            
                        case "cancelbyepgid":
                            RolesManager.ValidateActionPermitted("seriesRecording", "cancelByEpgId", false);
                            return SeriesRecordingController.CancelByEpgId((long) methodParams[0], (long) methodParams[1]);
                            
                        case "cancelbyseasonnumber":
                            RolesManager.ValidateActionPermitted("seriesRecording", "cancelBySeasonNumber", false);
                            return SeriesRecordingController.CancelBySeasonNumber((long) methodParams[0], (long) methodParams[1]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("seriesRecording", "delete", false);
                            return SeriesRecordingController.Delete((long) methodParams[0]);
                            
                        case "deletebyseasonnumber":
                            RolesManager.ValidateActionPermitted("seriesRecording", "deleteBySeasonNumber", false);
                            return SeriesRecordingController.DeleteBySeasonNumber((long) methodParams[0], (int) methodParams[1]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("seriesRecording", "list", false);
                            return SeriesRecordingController.List((KalturaSeriesRecordingFilter) methodParams[0]);
                            
                    }
                    break;
                    
                case "session":
                    switch(action)
                    {
                        case "get":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("session", "get", false);
                                return SessionController.GetOldStandard((string) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("session", "get", false);
                            return SessionController.Get((string) methodParams[0]);
                            
                        case "getoldstandard":
                            RolesManager.ValidateActionPermitted("session", "getOldStandard", false);
                            return SessionController.GetOldStandard((string) methodParams[0]);
                            
                        case "revoke":
                            RolesManager.ValidateActionPermitted("session", "revoke", false);
                            return SessionController.Revoke();
                            
                        case "switchuser":
                            RolesManager.ValidateActionPermitted("session", "switchUser", false);
                            return SessionController.SwitchUser((string) methodParams[0]);
                            
                    }
                    break;
                    
                case "socialaction":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("socialAction", "add", false);
                            return SocialActionController.Add((KalturaSocialAction) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("socialAction", "delete", false);
                            return SocialActionController.Delete((string) methodParams[0]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("socialAction", "list", false);
                            return SocialActionController.List((KalturaSocialActionFilter) methodParams[0], (KalturaFilterPager) methodParams[1]);
                            
                    }
                    break;
                    
                case "socialcomment":
                    switch(action)
                    {
                        case "list":
                            RolesManager.ValidateActionPermitted("socialComment", "list", false);
                            return SocialCommentController.List((KalturaSocialCommentFilter) methodParams[0], (KalturaFilterPager) methodParams[1]);
                            
                    }
                    break;
                    
                case "social":
                    switch(action)
                    {
                        case "get":
                            RolesManager.ValidateActionPermitted("social", "get", false);
                            return SocialController.Get((KalturaSocialNetwork) methodParams[0]);
                            
                        case "getbytoken":
                            if(isOldVersion)
                            {
                                if(HttpContext.Current.Request.HttpMethod.ToLower() == "get")
                                {
                                    throw new BadRequestException(BadRequestException.HTTP_METHOD_NOT_SUPPORTED, HttpContext.Current.Request.HttpMethod.ToUpper());
                                }
                                RolesManager.ValidateActionPermitted("social", "getByToken", false);
                                return SocialController.GetByTokenOldStandard((int) methodParams[0], (string) methodParams[1], (KalturaSocialNetwork) methodParams[2]);
                            }
                            if(HttpContext.Current.Request.HttpMethod.ToLower() == "get")
                            {
                                throw new BadRequestException(BadRequestException.HTTP_METHOD_NOT_SUPPORTED, HttpContext.Current.Request.HttpMethod.ToUpper());
                            }
                            RolesManager.ValidateActionPermitted("social", "getByToken", false);
                            return SocialController.GetByToken((int) methodParams[0], (string) methodParams[1], (KalturaSocialNetwork) methodParams[2]);
                            
                        case "getbytokenoldstandard":
                            if(HttpContext.Current.Request.HttpMethod.ToLower() == "get")
                            {
                                throw new BadRequestException(BadRequestException.HTTP_METHOD_NOT_SUPPORTED, HttpContext.Current.Request.HttpMethod.ToUpper());
                            }
                            RolesManager.ValidateActionPermitted("social", "getByTokenOldStandard", false);
                            return SocialController.GetByTokenOldStandard((int) methodParams[0], (string) methodParams[1], (KalturaSocialNetwork) methodParams[2]);
                            
                        case "getconfiguration":
                            RolesManager.ValidateActionPermitted("social", "getConfiguration", false);
                            return SocialController.GetConfiguration((Nullable<KalturaSocialNetwork>) methodParams[0], (Nullable<int>) methodParams[1]);
                            
                        case "login":
                            if(HttpContext.Current.Request.HttpMethod.ToLower() == "get")
                            {
                                throw new BadRequestException(BadRequestException.HTTP_METHOD_NOT_SUPPORTED, HttpContext.Current.Request.HttpMethod.ToUpper());
                            }
                            return SocialController.Login((int) methodParams[0], (string) methodParams[1], (KalturaSocialNetwork) methodParams[2], (string) methodParams[3]);
                            
                        case "merge":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("social", "merge", false);
                                return SocialController.MergeOldStandard((int) methodParams[0], (string) methodParams[1], (string) methodParams[2], (string) methodParams[3], (string) methodParams[4], (KalturaSocialNetwork) methodParams[5]);
                            }
                            RolesManager.ValidateActionPermitted("social", "merge", false);
                            return SocialController.Merge((string) methodParams[0], (KalturaSocialNetwork) methodParams[1]);
                            
                        case "mergeoldstandard":
                            RolesManager.ValidateActionPermitted("social", "mergeOldStandard", false);
                            return SocialController.MergeOldStandard((int) methodParams[0], (string) methodParams[1], (string) methodParams[2], (string) methodParams[3], (string) methodParams[4], (KalturaSocialNetwork) methodParams[5]);
                            
                        case "register":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("social", "register", false);
                                return SocialController.RegisterOldStandard((int) methodParams[0], (string) methodParams[1], (bool) methodParams[2], (bool) methodParams[3], (KalturaSocialNetwork) methodParams[4], (string) methodParams[5]);
                            }
                            RolesManager.ValidateActionPermitted("social", "register", false);
                            return SocialController.Register((int) methodParams[0], (string) methodParams[1], (KalturaSocialNetwork) methodParams[2], (string) methodParams[3]);
                            
                        case "registeroldstandard":
                            RolesManager.ValidateActionPermitted("social", "registerOldStandard", false);
                            return SocialController.RegisterOldStandard((int) methodParams[0], (string) methodParams[1], (bool) methodParams[2], (bool) methodParams[3], (KalturaSocialNetwork) methodParams[4], (string) methodParams[5]);
                            
                        case "unmerge":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("social", "unmerge", false);
                                return SocialController.UnmergeOldStandard((string) methodParams[0], (string) methodParams[1], (string) methodParams[2], (KalturaSocialNetwork) methodParams[3]);
                            }
                            RolesManager.ValidateActionPermitted("social", "unmerge", false);
                            return SocialController.Unmerge((KalturaSocialNetwork) methodParams[0]);
                            
                        case "unmergeoldstandard":
                            RolesManager.ValidateActionPermitted("social", "unmergeOldStandard", false);
                            return SocialController.UnmergeOldStandard((string) methodParams[0], (string) methodParams[1], (string) methodParams[2], (KalturaSocialNetwork) methodParams[3]);
                            
                        case "updateconfiguration":
                            RolesManager.ValidateActionPermitted("social", "UpdateConfiguration", false);
                            return SocialController.UpdateConfiguration((KalturaSocialConfig) methodParams[0]);
                            
                        case "config":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("social", "config", false);
                                return SocialController.GetConfiguration((Nullable<KalturaSocialNetwork>) methodParams[0], (Nullable<int>) methodParams[1]);
                            }
                            break;
                            
                    }
                    break;
                    
                case "socialfriendactivity":
                    switch(action)
                    {
                        case "list":
                            RolesManager.ValidateActionPermitted("socialFriendActivity", "list", false);
                            return SocialFriendActivityController.List((KalturaSocialFriendActivityFilter) methodParams[0], (KalturaFilterPager) methodParams[1]);
                            
                    }
                    break;
                    
                case "ssoadapterprofile":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("ssoAdapterProfile", "add", false);
                            return SsoAdapterProfileController.Add((KalturaSSOAdapterProfile) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("ssoAdapterProfile", "delete", false);
                            return SsoAdapterProfileController.Delete((int) methodParams[0]);
                            
                        case "generatesharedsecret":
                            RolesManager.ValidateActionPermitted("ssoAdapterProfile", "generateSharedSecret", false);
                            return SsoAdapterProfileController.GenerateSharedSecret((int) methodParams[0]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("ssoAdapterProfile", "list", false);
                            return SsoAdapterProfileController.List();
                            
                        case "update":
                            RolesManager.ValidateActionPermitted("ssoAdapterProfile", "update", false);
                            return SsoAdapterProfileController.Update((int) methodParams[0], (KalturaSSOAdapterProfile) methodParams[1]);
                            
                    }
                    break;
                    
                case "subscription":
                    switch(action)
                    {
                        case "list":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("subscription", "list", false);
                                return SubscriptionController.ListOldStandard((KalturaSubscriptionsFilter) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("subscription", "list", false);
                            return SubscriptionController.List((KalturaSubscriptionFilter) methodParams[0]);
                            
                        case "listoldstandard":
                            RolesManager.ValidateActionPermitted("subscription", "listOldStandard", false);
                            return SubscriptionController.ListOldStandard((KalturaSubscriptionsFilter) methodParams[0]);
                            
                        case "validatecoupon":
                            RolesManager.ValidateActionPermitted("subscription", "validateCoupon", false);
                            return SubscriptionController.ValidateCoupon((int) methodParams[0], (string) methodParams[1]);
                            
                    }
                    break;
                    
                case "subscriptionset":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("subscriptionSet", "add", false);
                            return SubscriptionSetController.Add((KalturaSubscriptionSet) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("subscriptionSet", "delete", false);
                            return SubscriptionSetController.Delete((long) methodParams[0]);
                            
                        case "get":
                            RolesManager.ValidateActionPermitted("subscriptionSet", "get", false);
                            return SubscriptionSetController.Get((long) methodParams[0]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("subscriptionSet", "list", false);
                            return SubscriptionSetController.List((KalturaSubscriptionSetFilter) methodParams[0]);
                            
                        case "update":
                            RolesManager.ValidateActionPermitted("subscriptionSet", "update", false);
                            return SubscriptionSetController.Update((long) methodParams[0], (KalturaSubscriptionSet) methodParams[1]);
                            
                    }
                    break;
                    
                case "system":
                    switch(action)
                    {
                        case "getcountry":
                            RolesManager.ValidateActionPermitted("system", "getCountry", false);
                            return SystemController.GetCountry((string) methodParams[0]);
                            
                        case "gettime":
                            return SystemController.GetTime();
                            
                        case "getversion":
                            return SystemController.GetVersion();
                            
                        case "ping":
                            return SystemController.Ping();
                            
                    }
                    break;
                    
                case "tag":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("tag", "add", false);
                            return TagController.Add((KalturaTag) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("tag", "delete", false);
                            return TagController.Delete((long) methodParams[0]);
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("tag", "list", false);
                            return TagController.List((KalturaTagFilter) methodParams[0], (KalturaFilterPager) methodParams[1]);
                            
                        case "update":
                            RolesManager.ValidateActionPermitted("tag", "update", false);
                            return TagController.Update((long) methodParams[0], (KalturaTag) methodParams[1]);
                            
                    }
                    break;
                    
                case "timeshiftedtvpartnersettings":
                    switch(action)
                    {
                        case "get":
                            RolesManager.ValidateActionPermitted("timeShiftedTvPartnerSettings", "get", false);
                            return TimeShiftedTvPartnerSettingsController.Get();
                            
                        case "update":
                            RolesManager.ValidateActionPermitted("timeShiftedTvPartnerSettings", "update", false);
                            return TimeShiftedTvPartnerSettingsController.Update((KalturaTimeShiftedTvPartnerSettings) methodParams[0]);
                            
                    }
                    break;
                    
                case "topic":
                    switch(action)
                    {
                        case "delete":
                            RolesManager.ValidateActionPermitted("topic", "delete", false);
                            return TopicController.Delete((int) methodParams[0]);
                            
                        case "get":
                            RolesManager.ValidateActionPermitted("topic", "get", false);
                            return TopicController.Get((int) methodParams[0]);
                            
                        case "list":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("topic", "list", false);
                                return TopicController.ListOldStandard((KalturaFilterPager) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("topic", "list", false);
                            return TopicController.List((KalturaTopicFilter) methodParams[0], (KalturaFilterPager) methodParams[1]);
                            
                        case "listoldstandard":
                            RolesManager.ValidateActionPermitted("topic", "listOldStandard", false);
                            return TopicController.ListOldStandard((KalturaFilterPager) methodParams[0]);
                            
                        case "updatestatus":
                            RolesManager.ValidateActionPermitted("topic", "updateStatus", false);
                            return TopicController.UpdateStatus((int) methodParams[0], (KalturaTopicAutomaticIssueNotification) methodParams[1]);
                            
                    }
                    break;
                    
                case "transaction":
                    switch(action)
                    {
                        case "downgrade":
                            RolesManager.ValidateActionPermitted("transaction", "downgrade", false);
                            TransactionController.Downgrade((KalturaPurchase) methodParams[0]);
                            return null;
                            
                        case "getpurchasesessionid":
                            RolesManager.ValidateActionPermitted("transaction", "getPurchaseSessionId", false);
                            return TransactionController.getPurchaseSessionId((KalturaPurchaseSession) methodParams[0]);
                            
                        case "processreceipt":
                            RolesManager.ValidateActionPermitted("transaction", "processReceipt", false);
                            return TransactionController.ProcessReceipt((int) methodParams[0], (KalturaTransactionType) methodParams[1], (string) methodParams[2], (string) methodParams[3], (int) methodParams[4]);
                            
                        case "purchase":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("transaction", "purchase", false);
                                return TransactionController.PurchaseOldStandard((float) methodParams[0], (string) methodParams[1], (int) methodParams[2], (KalturaTransactionType) methodParams[3], (int) methodParams[4], (string) methodParams[5], (int) methodParams[6], (Nullable<int>) methodParams[7], (string) methodParams[8]);
                            }
                            RolesManager.ValidateActionPermitted("transaction", "purchase", false);
                            return TransactionController.Purchase((KalturaPurchase) methodParams[0]);
                            
                        case "purchaseoldstandard":
                            RolesManager.ValidateActionPermitted("transaction", "purchaseOldStandard", false);
                            return TransactionController.PurchaseOldStandard((float) methodParams[0], (string) methodParams[1], (int) methodParams[2], (KalturaTransactionType) methodParams[3], (int) methodParams[4], (string) methodParams[5], (int) methodParams[6], (Nullable<int>) methodParams[7], (string) methodParams[8]);
                            
                        case "purchasesessionidoldstandard":
                            RolesManager.ValidateActionPermitted("transaction", "purchaseSessionIdOldStandard", false);
                            return TransactionController.PurchaseSessionIdOldStandard((float) methodParams[0], (string) methodParams[1], (int) methodParams[2], (KalturaTransactionType) methodParams[3], (int) methodParams[4], (string) methodParams[5], (int) methodParams[6]);
                            
                        case "setwaiver":
                            RolesManager.ValidateActionPermitted("transaction", "setWaiver", false);
                            return TransactionController.SetWaiver((int) methodParams[0], (KalturaTransactionType) methodParams[1]);
                            
                        case "updatestate":
                            RolesManager.ValidateActionPermitted("transaction", "updateState", false);
                            TransactionController.UpdateState((string) methodParams[0], (int) methodParams[1], (string) methodParams[2], (string) methodParams[3], (string) methodParams[4], (int) methodParams[5], (string) methodParams[6]);
                            return null;
                            
                        case "updatestatus":
                            RolesManager.ValidateActionPermitted("transaction", "updateStatus", false);
                            TransactionController.UpdateStatus((string) methodParams[0], (string) methodParams[1], (string) methodParams[2], (KalturaTransactionStatus) methodParams[3]);
                            return null;
                            
                        case "upgrade":
                            RolesManager.ValidateActionPermitted("transaction", "upgrade", false);
                            return TransactionController.Upgrade((KalturaPurchase) methodParams[0]);
                            
                        case "validatereceipt":
                            RolesManager.ValidateActionPermitted("transaction", "validateReceipt", false);
                            return TransactionController.ValidateReceipt((KalturaExternalReceipt) methodParams[0]);
                            
                        case "purchaseSessionId":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("transaction", "purchaseSessionId", false);
                                return TransactionController.PurchaseSessionIdOldStandard((float) methodParams[0], (string) methodParams[1], (int) methodParams[2], (KalturaTransactionType) methodParams[3], (int) methodParams[4], (string) methodParams[5], (int) methodParams[6]);
                            }
                            break;
                            
                        case "waiver":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("transaction", "waiver", false);
                                return TransactionController.SetWaiver((int) methodParams[0], (KalturaTransactionType) methodParams[1]);
                            }
                            break;
                            
                    }
                    break;
                    
                case "transactionhistory":
                    switch(action)
                    {
                        case "list":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("transactionHistory", "list", false);
                                return TransactionHistoryController.ListOldStandard((KalturaTransactionsFilter) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("transactionHistory", "list", false);
                            return TransactionHistoryController.List((KalturaTransactionHistoryFilter) methodParams[0], (KalturaFilterPager) methodParams[1]);
                            
                        case "listoldstandard":
                            RolesManager.ValidateActionPermitted("transactionHistory", "listOldStandard", false);
                            return TransactionHistoryController.ListOldStandard((KalturaTransactionsFilter) methodParams[0]);
                            
                    }
                    break;
                    
                case "unifiedpayment":
                    switch(action)
                    {
                        case "getnextrenewal":
                            RolesManager.ValidateActionPermitted("unifiedPayment", "getNextRenewal", false);
                            return UnifiedPaymentController.GetNextRenewal((int) methodParams[0]);
                            
                    }
                    break;
                    
                case "uploadtoken":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("uploadToken", "add", false);
                            return UploadTokenController.Add((KalturaUploadToken) methodParams[0]);
                            
                        case "upload":
                            RolesManager.ValidateActionPermitted("uploadToken", "upload", false);
                            return UploadTokenController.Upload((string) methodParams[0], (KalturaOTTFile) methodParams[1]);
                            
                    }
                    break;
                    
                case "userassetrule":
                    switch(action)
                    {
                        case "list":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("userAssetRule", "list", false);
                                return UserAssetRuleController.ListOldStandard((KalturaGenericRuleFilter) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("userAssetRule", "list", false);
                            return UserAssetRuleController.List((KalturaUserAssetRuleFilter) methodParams[0]);
                            
                        case "listoldstandard":
                            RolesManager.ValidateActionPermitted("userAssetRule", "listOldStandard", false);
                            return UserAssetRuleController.ListOldStandard((KalturaGenericRuleFilter) methodParams[0]);
                            
                    }
                    break;
                    
                case "userassetslist":
                    switch(action)
                    {
                        case "list":
                            RolesManager.ValidateActionPermitted("userAssetsList", "list", false);
                            return UserAssetsListController.List((KalturaUserAssetsListFilter) methodParams[0]);
                            
                    }
                    break;
                    
                case "userassetslistitem":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("userAssetsListItem", "add", false);
                            return UserAssetsListItemController.Add((KalturaUserAssetsListItem) methodParams[0]);
                            
                        case "delete":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("userAssetsListItem", "delete", false);
                                return UserAssetsListItemController.DeleteOldStandard((KalturaUserAssetsListItem) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("userAssetsListItem", "delete", false);
                            return UserAssetsListItemController.Delete((string) methodParams[0], (KalturaUserAssetsListType) methodParams[1]);
                            
                        case "deleteoldstandard":
                            RolesManager.ValidateActionPermitted("userAssetsListItem", "deleteOldStandard", false);
                            return UserAssetsListItemController.DeleteOldStandard((KalturaUserAssetsListItem) methodParams[0]);
                            
                        case "get":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("userAssetsListItem", "get", false);
                                return UserAssetsListItemController.GetOldStandard((KalturaUserAssetsListItem) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("userAssetsListItem", "get", false);
                            return UserAssetsListItemController.Get((string) methodParams[0], (KalturaUserAssetsListType) methodParams[1], (KalturaUserAssetsListItemType) methodParams[2]);
                            
                        case "getoldstandard":
                            RolesManager.ValidateActionPermitted("userAssetsListItem", "getOldStandard", false);
                            return UserAssetsListItemController.GetOldStandard((KalturaUserAssetsListItem) methodParams[0]);
                            
                    }
                    break;
                    
                case "userinterest":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("userInterest", "add", false);
                            return UserInterestController.Add((KalturaUserInterest) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("userInterest", "delete", false);
                            return UserInterestController.Delete((string) methodParams[0]);
                            
                        case "deletewithtoken":
                            UserInterestController.DeleteWithToken((string) methodParams[0], (string) methodParams[1], (int) methodParams[2]);
                            return null;
                            
                        case "list":
                            RolesManager.ValidateActionPermitted("userInterest", "list", false);
                            return UserInterestController.List();
                            
                    }
                    break;
                    
                case "userloginpin":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("userLoginPin", "add", false);
                            return UserLoginPinController.Add((string) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("userLoginPin", "delete", false);
                            return UserLoginPinController.Delete((string) methodParams[0]);
                            
                        case "deleteall":
                            RolesManager.ValidateActionPermitted("userLoginPin", "deleteAll", false);
                            return UserLoginPinController.DeleteAll();
                            
                        case "update":
                            RolesManager.ValidateActionPermitted("userLoginPin", "update", false);
                            return UserLoginPinController.Update((string) methodParams[0], (string) methodParams[1]);
                            
                    }
                    break;
                    
                case "userrole":
                    switch(action)
                    {
                        case "add":
                            RolesManager.ValidateActionPermitted("userRole", "add", false);
                            return UserRoleController.Add((KalturaUserRole) methodParams[0]);
                            
                        case "delete":
                            RolesManager.ValidateActionPermitted("userRole", "delete", false);
                            return UserRoleController.Delete((long) methodParams[0]);
                            
                        case "list":
                            if(isOldVersion)
                            {
                                RolesManager.ValidateActionPermitted("userRole", "list", false);
                                return UserRoleController.ListOldStandard((KalturaUserRoleFilter) methodParams[0]);
                            }
                            RolesManager.ValidateActionPermitted("userRole", "list", false);
                            return UserRoleController.List((KalturaUserRoleFilter) methodParams[0]);
                            
                        case "listoldstandard":
                            RolesManager.ValidateActionPermitted("userRole", "listOldStandard", false);
                            return UserRoleController.ListOldStandard((KalturaUserRoleFilter) methodParams[0]);
                            
                        case "update":
                            RolesManager.ValidateActionPermitted("userRole", "update", false);
                            return UserRoleController.Update((long) methodParams[0], (KalturaUserRole) methodParams[1]);
                            
                    }
                    break;
                    
                case "version":
                    switch(action)
                    {
                        case "":
                            return VersionController.Get();
                            
                    }
                    break;
                    
            }
            
            throw new RequestParserException(RequestParserException.INVALID_ACTION, service, action);
        }
        
        public static Dictionary<string, MethodParam> getMethodParams(string service, string action)
        {
            service = service.ToLower();
            action = action.ToLower();
            Dictionary<string, MethodParam> ret = new Dictionary<string, MethodParam>();
            Version currentVersion = (Version)HttpContext.Current.Items[RequestParser.REQUEST_VERSION];
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string paramName;
            string newParamName = null;
            if(isOldVersion)
            {
                switch (service)
                {
                    case "announcement":
                        switch(action)
                        {
                            case "add":
                                action = "addoldstandard";
                                break;
                                
                            case "createAnnouncement":
                                action = "enablesystemannouncements";
                                break;
                                
                            case "list":
                                action = "listoldstandard";
                                break;
                                
                            case "update":
                                action = "updateoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "asset":
                        switch(action)
                        {
                            case "get":
                                action = "getoldstandard";
                                break;
                                
                            case "list":
                                action = "listoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "assethistory":
                        switch(action)
                        {
                            case "clean":
                                action = "cleanoldstandard";
                                break;
                                
                            case "list":
                                action = "listoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "bookmark":
                        switch(action)
                        {
                            case "add":
                                action = "addoldstandard";
                                break;
                                
                            case "list":
                                action = "listoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "cdvradapterprofile":
                        switch(action)
                        {
                            case "list":
                                action = "listoldstandard";
                                break;
                                
                            case "update":
                                action = "updateoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "channel":
                        switch(action)
                        {
                            case "add":
                                action = "addoldstandard";
                                break;
                                
                            case "get":
                                action = "getoldstandard";
                                break;
                                
                            case "update":
                                action = "updateoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "entitlement":
                        switch(action)
                        {
                            case "list":
                                action = "listoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "exporttask":
                        switch(action)
                        {
                            case "list":
                                action = "listoldstandard";
                                break;
                                
                            case "update":
                                action = "updateoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "externalchannelprofile":
                        switch(action)
                        {
                            case "list":
                                action = "listoldstandard";
                                break;
                                
                            case "update":
                                action = "updateoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "favorite":
                        switch(action)
                        {
                            case "add":
                                action = "addoldstandard";
                                break;
                                
                            case "delete":
                                action = "deleteoldstandard";
                                break;
                                
                            case "list":
                                action = "listoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "followtvseries":
                        switch(action)
                        {
                            case "add":
                                action = "addoldstandard";
                                break;
                                
                            case "list":
                                action = "listoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "homenetwork":
                        switch(action)
                        {
                            case "list":
                                action = "listoldstandard";
                                break;
                                
                            case "update":
                                action = "updateoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "household":
                        switch(action)
                        {
                            case "add":
                                action = "addoldstandard";
                                break;
                                
                            case "get":
                                action = "getoldstandard";
                                break;
                                
                            case "update":
                                action = "updateoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "householddevice":
                        switch(action)
                        {
                            case "add":
                                action = "addoldstandard";
                                break;
                                
                            case "update":
                                action = "updateoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "householdpaymentgateway":
                        switch(action)
                        {
                            case "delete":
                                action = "disable";
                                break;
                                
                            case "set":
                                action = "enable";
                                break;
                                
                        }
                        break;
                        
                    case "householdpremiumservice":
                        switch(action)
                        {
                            case "list":
                                action = "listoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "householduser":
                        switch(action)
                        {
                            case "add":
                                action = "addoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "licensedurl":
                        switch(action)
                        {
                            case "get":
                                action = "getoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "meta":
                        switch(action)
                        {
                            case "list":
                                action = "listoldstandard";
                                break;
                                
                            case "update":
                                action = "updateoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "notificationspartnersettings":
                        switch(action)
                        {
                            case "get":
                                action = "getoldstandard";
                                break;
                                
                            case "update":
                                action = "updateoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "notificationssettings":
                        switch(action)
                        {
                            case "get":
                                action = "getoldstandard";
                                break;
                                
                            case "update":
                                action = "updateoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "ossadapterprofile":
                        switch(action)
                        {
                            case "list":
                                action = "listoldstandard";
                                break;
                                
                            case "update":
                                action = "updateoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "ottuser":
                        switch(action)
                        {
                            case "get":
                                action = "getoldstandard";
                                break;
                                
                            case "add":
                                action = "register";
                                break;
                                
                            case "sendPassword":
                                action = "resetpassword";
                                break;
                                
                            case "resetPassword":
                                action = "setpassword";
                                break;
                                
                            case "changePassword":
                                action = "updatelogindata";
                                break;
                                
                        }
                        break;
                        
                    case "parentalrule":
                        switch(action)
                        {
                            case "list":
                                action = "listoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "paymentgatewayprofile":
                        switch(action)
                        {
                            case "add":
                                action = "addoldstandard";
                                break;
                                
                            case "list":
                                action = "listoldstandard";
                                break;
                                
                            case "update":
                                action = "updateoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "paymentgatewayprofilesettings":
                        switch(action)
                        {
                            case "add":
                                action = "addoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "paymentmethodprofile":
                        switch(action)
                        {
                            case "add":
                                action = "addoldstandard";
                                break;
                                
                            case "delete":
                                action = "deleteoldstandard";
                                break;
                                
                            case "list":
                                action = "listoldstandard";
                                break;
                                
                            case "update":
                                action = "updateoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "personalfeed":
                        switch(action)
                        {
                            case "list":
                                action = "listoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "pin":
                        switch(action)
                        {
                            case "get":
                                action = "getoldstandard";
                                break;
                                
                            case "update":
                                action = "updateoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "purchasesettings":
                        switch(action)
                        {
                            case "get":
                                action = "getoldstandard";
                                break;
                                
                            case "update":
                                action = "updateoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "recommendationprofile":
                        switch(action)
                        {
                            case "list":
                                action = "listoldstandard";
                                break;
                                
                            case "update":
                                action = "updateoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "registrysettings":
                        switch(action)
                        {
                            case "list":
                                action = "listoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "session":
                        switch(action)
                        {
                            case "get":
                                action = "getoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "social":
                        switch(action)
                        {
                            case "getByToken":
                                action = "getbytokenoldstandard";
                                break;
                                
                            case "config":
                                action = "getconfiguration";
                                break;
                                
                            case "merge":
                                action = "mergeoldstandard";
                                break;
                                
                            case "register":
                                action = "registeroldstandard";
                                break;
                                
                            case "unmerge":
                                action = "unmergeoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "subscription":
                        switch(action)
                        {
                            case "list":
                                action = "listoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "topic":
                        switch(action)
                        {
                            case "list":
                                action = "listoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "transaction":
                        switch(action)
                        {
                            case "purchase":
                                action = "purchaseoldstandard";
                                break;
                                
                            case "purchaseSessionId":
                                action = "purchasesessionidoldstandard";
                                break;
                                
                            case "waiver":
                                action = "setwaiver";
                                break;
                                
                        }
                        break;
                        
                    case "transactionhistory":
                        switch(action)
                        {
                            case "list":
                                action = "listoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "userassetrule":
                        switch(action)
                        {
                            case "list":
                                action = "listoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "userassetslistitem":
                        switch(action)
                        {
                            case "delete":
                                action = "deleteoldstandard";
                                break;
                                
                            case "get":
                                action = "getoldstandard";
                                break;
                                
                        }
                        break;
                        
                    case "userrole":
                        switch(action)
                        {
                            case "list":
                                action = "listoldstandard";
                                break;
                                
                        }
                        break;
                        
                }
            }
            switch (service)
            {
                case "announcement":
                    switch(action)
                    {
                        case "add":
                            ret.Add("announcement", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaAnnouncement),
                            });
                            return ret;
                            
                        case "addoldstandard":
                            ret.Add("announcement", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaAnnouncement),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                            });
                            return ret;
                            
                        case "enablesystemannouncements":
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaAnnouncementFilter),
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            return ret;
                            
                        case "listoldstandard":
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            return ret;
                            
                        case "update":
                            ret.Add("announcementId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("announcement", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaAnnouncement),
                            });
                            return ret;
                            
                        case "updateoldstandard":
                            ret.Add("announcement", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaAnnouncement),
                            });
                            return ret;
                            
                        case "updatestatus":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                            });
                            ret.Add("status", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(bool),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "apptoken":
                    switch(action)
                    {
                        case "add":
                            ret.Add("appToken", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaAppToken),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "get":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "startsession":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("tokenHash", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("userId", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            ret.Add("expiry", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsNullable = true,
                                Type = typeof(Int32),
                            });
                            ret.Add("udid", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "assetcomment":
                    switch(action)
                    {
                        case "add":
                            ret.Add("comment", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaAssetComment),
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaAssetCommentFilter),
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "asset":
                    switch(action)
                    {
                        case "add":
                            ret.Add("asset", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaAsset),
                            });
                            return ret;
                            
                        case "autocomplete":
                            ret.Add("query", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("with", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsList = true,
                                GenericType = typeof(KalturaCatalogWithHolder),
                                Type = typeof(List<KalturaCatalogWithHolder>),
                            });
                            ret.Add("filter_types", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsList = true,
                                GenericType = typeof(KalturaIntegerValue),
                                Type = typeof(List<KalturaIntegerValue>),
                            });
                            ret.Add("order_by", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsNullable = true,
                                Type = typeof(KalturaOrder),
                                IsEnum = true,
                            });
                            ret.Add("size", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsNullable = true,
                                Type = typeof(Int32),
                            });
                            return ret;
                            
                        case "channel":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "asset", "channel") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinInteger = 1,
                                },
                            });
                            ret.Add("with", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsList = true,
                                GenericType = typeof(KalturaCatalogWithHolder),
                                Type = typeof(List<KalturaCatalogWithHolder>),
                            });
                            ret.Add("order_by", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsNullable = true,
                                Type = typeof(KalturaOrder),
                                IsEnum = true,
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            ret.Add("filter_query", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "count":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaSearchAssetFilter),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "asset", "delete") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            ret.Add("assetReferenceType", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaAssetReferenceType),
                            });
                            return ret;
                            
                        case "externalchannel":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "asset", "externalChannel") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinInteger = 1,
                                },
                            });
                            ret.Add("with", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsList = true,
                                GenericType = typeof(KalturaCatalogWithHolder),
                                Type = typeof(List<KalturaCatalogWithHolder>),
                            });
                            ret.Add("order_by", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsNullable = true,
                                Type = typeof(KalturaOrder),
                                IsEnum = true,
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            ret.Add("utc_offset", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsNullable = true,
                                Type = typeof(Single),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("utc_offset", "asset", "externalChannel") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MaxFloat = 12,
                                    MinFloat = -12,
                                },
                            });
                            ret.Add("free_param", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "get":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("assetReferenceType", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaAssetReferenceType),
                            });
                            return ret;
                            
                        case "getadscontext":
                            ret.Add("assetId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("assetType", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaAssetType),
                            });
                            ret.Add("contextDataParams", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaPlaybackContextOptions),
                            });
                            return ret;
                            
                        case "getoldstandard":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("type", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaAssetReferenceType),
                            });
                            ret.Add("with", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsList = true,
                                GenericType = typeof(KalturaCatalogWithHolder),
                                Type = typeof(List<KalturaCatalogWithHolder>),
                            });
                            return ret;
                            
                        case "getplaybackcontext":
                            ret.Add("assetId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("assetType", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaAssetType),
                            });
                            ret.Add("contextDataParams", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaPlaybackContextOptions),
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaAssetFilter),
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            return ret;
                            
                        case "listoldstandard":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaAssetInfoFilter),
                            });
                            ret.Add("with", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsList = true,
                                GenericType = typeof(KalturaCatalogWithHolder),
                                Type = typeof(List<KalturaCatalogWithHolder>),
                            });
                            ret.Add("order_by", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsNullable = true,
                                Type = typeof(KalturaOrder),
                                IsEnum = true,
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            return ret;
                            
                        case "related":
                            ret.Add("media_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            ret.Add("filter_types", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsList = true,
                                GenericType = typeof(KalturaIntegerValue),
                                Type = typeof(List<KalturaIntegerValue>),
                            });
                            ret.Add("with", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsList = true,
                                GenericType = typeof(KalturaCatalogWithHolder),
                                Type = typeof(List<KalturaCatalogWithHolder>),
                            });
                            return ret;
                            
                        case "relatedexternal":
                            ret.Add("asset_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("asset_id", "asset", "relatedExternal") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinInteger = 1,
                                },
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            ret.Add("filter_type_ids", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsList = true,
                                GenericType = typeof(KalturaIntegerValue),
                                Type = typeof(List<KalturaIntegerValue>),
                            });
                            ret.Add("utc_offset", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = 0,
                                Type = typeof(int),
                            });
                            ret.Add("with", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsList = true,
                                GenericType = typeof(KalturaCatalogWithHolder),
                                Type = typeof(List<KalturaCatalogWithHolder>),
                            });
                            ret.Add("free_param", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "removemetasandtags":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "asset", "removeMetasAndTags") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            ret.Add("assetReferenceType", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaAssetReferenceType),
                            });
                            ret.Add("idIn", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("idIn", "asset", "removeMetasAndTags") {
                                    RequiresPermission = false,
                                    DynamicMinInt = 1,
                                    MaxLength = -1,
                                    MinLength = -1,
                                },
                            });
                            return ret;
                            
                        case "search":
                            ret.Add("order_by", new MethodParam(){
                                NewName = newParamName,
                                IsNullable = true,
                                Type = typeof(KalturaOrder),
                                IsEnum = true,
                            });
                            ret.Add("filter_types", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsList = true,
                                GenericType = typeof(KalturaIntegerValue),
                                Type = typeof(List<KalturaIntegerValue>),
                            });
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("filter", "asset", "search") {
                                    RequiresPermission = false,
                                    MaxLength = 2048,
                                    MinLength = -1,
                                },
                            });
                            ret.Add("with", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsList = true,
                                GenericType = typeof(KalturaCatalogWithHolder),
                                Type = typeof(List<KalturaCatalogWithHolder>),
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            ret.Add("request_id", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "searchexternal":
                            ret.Add("query", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            ret.Add("filter_type_ids", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsList = true,
                                GenericType = typeof(KalturaIntegerValue),
                                Type = typeof(List<KalturaIntegerValue>),
                            });
                            ret.Add("utc_offset", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = 0,
                                Type = typeof(int),
                            });
                            ret.Add("with", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsList = true,
                                GenericType = typeof(KalturaCatalogWithHolder),
                                Type = typeof(List<KalturaCatalogWithHolder>),
                            });
                            return ret;
                            
                        case "update":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "asset", "update") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            ret.Add("asset", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaAsset),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "assetfile":
                    switch(action)
                    {
                        case "getcontext":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("contextType", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaContextType),
                            });
                            return ret;
                            
                        case "playmanifest":
                            ret.Add("partnerId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("partnerId", "assetFile", "playManifest") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinInteger = 1,
                                },
                            });
                            ret.Add("assetId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("assetType", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaAssetType),
                            });
                            ret.Add("assetFileId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("assetFileId", "assetFile", "playManifest") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinInteger = 1,
                                },
                            });
                            ret.Add("contextType", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaPlaybackContextType),
                            });
                            ret.Add("ks", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "assethistory":
                    switch(action)
                    {
                        case "clean":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaAssetHistoryFilter),
                            });
                            return ret;
                            
                        case "cleanoldstandard":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaAssetsFilter),
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaAssetHistoryFilter),
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            return ret;
                            
                        case "listoldstandard":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaAssetHistoryFilter),
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "assetrule":
                    switch(action)
                    {
                        case "add":
                            ret.Add("assetRule", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaAssetRule),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaAssetRuleFilter),
                            });
                            return ret;
                            
                        case "update":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                            });
                            ret.Add("assetRule", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaAssetRule),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "assetstatistics":
                    switch(action)
                    {
                        case "query":
                            ret.Add("query", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaAssetStatisticsQuery),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "assetstruct":
                    switch(action)
                    {
                        case "add":
                            ret.Add("assetStruct", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaAssetStruct),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "assetStruct", "delete") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaAssetStructFilter),
                            });
                            return ret;
                            
                        case "update":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "assetStruct", "update") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            ret.Add("assetStruct", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaAssetStruct),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "assetstructmeta":
                    switch(action)
                    {
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaAssetStructMetaFilter),
                            });
                            return ret;
                            
                        case "update":
                            ret.Add("assetStructId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("assetStructId", "assetStructMeta", "update") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            ret.Add("metaId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("metaId", "assetStructMeta", "update") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            ret.Add("assetStructMeta", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaAssetStructMeta),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "assetuserrule":
                    switch(action)
                    {
                        case "add":
                            ret.Add("assetUserRule", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaAssetUserRule),
                            });
                            return ret;
                            
                        case "attachuser":
                            ret.Add("ruleId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("ruleId", "assetUserRule", "attachUser") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "assetUserRule", "delete") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            return ret;
                            
                        case "detachuser":
                            ret.Add("ruleId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("ruleId", "assetUserRule", "detachUser") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaAssetUserRuleFilter),
                            });
                            return ret;
                            
                        case "update":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "assetUserRule", "update") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            ret.Add("assetUserRule", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaAssetUserRule),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "bookmark":
                    switch(action)
                    {
                        case "add":
                            ret.Add("bookmark", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaBookmark),
                            });
                            return ret;
                            
                        case "addoldstandard":
                            ret.Add("asset_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("asset_type", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaAssetType),
                            });
                            ret.Add("file_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                            });
                            ret.Add("player_asset_data", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaPlayerAssetData),
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaBookmarkFilter),
                            });
                            return ret;
                            
                        case "listoldstandard":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaAssetsFilter),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "bulk":
                    switch(action)
                    {
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaBulkFilter),
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            return ret;
                            
                        case "servelog":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "category":
                    switch(action)
                    {
                        case "get":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "cdnadapterprofile":
                    switch(action)
                    {
                        case "add":
                            ret.Add("adapter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaCDNAdapterProfile),
                            });
                            return ret;
                            
                        case "delete":
                            paramName = "adapterId";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "adapter_id";
                                newParamName = "adapterId";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "generatesharedsecret":
                            ret.Add("adapterId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "list":
                            return ret;
                            
                        case "update":
                            ret.Add("adapterId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("adapter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaCDNAdapterProfile),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "cdnpartnersettings":
                    switch(action)
                    {
                        case "get":
                            return ret;
                            
                        case "update":
                            ret.Add("settings", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaCDNPartnerSettings),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "cdvradapterprofile":
                    switch(action)
                    {
                        case "add":
                            ret.Add("adapter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaCDVRAdapterProfile),
                            });
                            return ret;
                            
                        case "delete":
                            paramName = "adapterId";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "adapter_id";
                                newParamName = "adapterId";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "generatesharedsecret":
                            paramName = "adapterId";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "adapter_id";
                                newParamName = "adapterId";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "list":
                            return ret;
                            
                        case "listoldstandard":
                            return ret;
                            
                        case "update":
                            ret.Add("adapterId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("adapter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaCDVRAdapterProfile),
                            });
                            return ret;
                            
                        case "updateoldstandard":
                            ret.Add("adapter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaCDVRAdapterProfile),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "channel":
                    switch(action)
                    {
                        case "add":
                            ret.Add("channel", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaChannel),
                            });
                            return ret;
                            
                        case "addoldstandard":
                            ret.Add("channel", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaChannelProfile),
                            });
                            return ret;
                            
                        case "delete":
                            paramName = "channelId";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "channel_id";
                                newParamName = "channelId";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "get":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "channel", "get") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinInteger = 1,
                                },
                            });
                            return ret;
                            
                        case "getoldstandard":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "channel", "getOldStandard") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinInteger = 1,
                                },
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaChannelsFilter),
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            return ret;
                            
                        case "update":
                            paramName = "id";
                            newParamName = null;
                            if(isOldVersion || currentVersion.CompareTo(new Version("5.0.0.0")) < 0)
                            {
                                paramName = "channelId";
                                newParamName = "id";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("channel", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaChannel),
                            });
                            return ret;
                            
                        case "updateoldstandard":
                            ret.Add("channel", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaChannelProfile),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "collection":
                    switch(action)
                    {
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaCollectionFilter),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "compensation":
                    switch(action)
                    {
                        case "add":
                            ret.Add("compensation", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaCompensation),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                            });
                            return ret;
                            
                        case "get":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "configurationgroup":
                    switch(action)
                    {
                        case "add":
                            ret.Add("configurationGroup", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaConfigurationGroup),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "get":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "list":
                            return ret;
                            
                        case "update":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("configurationGroup", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaConfigurationGroup),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "configurationgroupdevice":
                    switch(action)
                    {
                        case "add":
                            ret.Add("configurationGroupDevice", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaConfigurationGroupDevice),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("udid", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "get":
                            ret.Add("udid", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaConfigurationGroupDeviceFilter),
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "configurationgrouptag":
                    switch(action)
                    {
                        case "add":
                            ret.Add("configurationGroupTag", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaConfigurationGroupTag),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("tag", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "get":
                            ret.Add("tag", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaConfigurationGroupTagFilter),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "configurations":
                    switch(action)
                    {
                        case "add":
                            ret.Add("configurations", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaConfigurations),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "get":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaConfigurationsFilter),
                            });
                            return ret;
                            
                        case "servebydevice":
                            ret.Add("applicationName", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("clientVersion", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("platform", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("udid", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("tag", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("partnerId", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = 0,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "update":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("configurations", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaConfigurations),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "country":
                    switch(action)
                    {
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaCountryFilter),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "coupon":
                    switch(action)
                    {
                        case "get":
                            ret.Add("code", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "couponsgroup":
                    switch(action)
                    {
                        case "add":
                            ret.Add("couponsGroup", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaCouponsGroup),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                            });
                            return ret;
                            
                        case "generate":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                            });
                            ret.Add("couponGenerationOptions", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaCouponGenerationOptions),
                            });
                            return ret;
                            
                        case "get":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "couponsGroup", "get") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            return ret;
                            
                        case "list":
                            return ret;
                            
                        case "update":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                            });
                            ret.Add("couponsGroup", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaCouponsGroup),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "currency":
                    switch(action)
                    {
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaCurrencyFilter),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "devicebrand":
                    switch(action)
                    {
                        case "list":
                            return ret;
                            
                    }
                    break;
                    
                case "devicefamily":
                    switch(action)
                    {
                        case "list":
                            return ret;
                            
                    }
                    break;
                    
                case "discountdetails":
                    switch(action)
                    {
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaDiscountDetailsFilter),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "drmprofile":
                    switch(action)
                    {
                        case "list":
                            return ret;
                            
                    }
                    break;
                    
                case "email":
                    switch(action)
                    {
                        case "send":
                            ret.Add("emailMessage", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaEmailMessage),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "engagementadapter":
                    switch(action)
                    {
                        case "add":
                            ret.Add("engagementAdapter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaEngagementAdapter),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "generatesharedsecret":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "get":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "list":
                            return ret;
                            
                        case "update":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("engagementAdapter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaEngagementAdapter),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "engagement":
                    switch(action)
                    {
                        case "add":
                            ret.Add("engagement", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaEngagement),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "get":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaEngagementFilter),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "entitlement":
                    switch(action)
                    {
                        case "buy":
                            paramName = "itemId";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "item_id";
                                newParamName = "itemId";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            paramName = "isSubscription";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "is_subscription";
                                newParamName = "isSubscription";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(bool),
                            });
                            ret.Add("price", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(float),
                            });
                            ret.Add("currency", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            paramName = "couponCode";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "coupon_code";
                                newParamName = "couponCode";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            paramName = "extraParams";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "extra_params";
                                newParamName = "extraParams";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            paramName = "encryptedCvv";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "encrypted_cvv";
                                newParamName = "encryptedCvv";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            paramName = "fileId";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "file_id";
                                newParamName = "fileId";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = 0,
                                Type = typeof(int),
                            });
                            ret.Add("udid", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "cancel":
                            paramName = "assetId";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "asset_id";
                                newParamName = "assetId";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("assetId", "entitlement", "cancel") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinInteger = 1,
                                },
                            });
                            paramName = "productType";
                            newParamName = null;
                            if(isOldVersion || currentVersion.CompareTo(new Version("4.7.0.0")) < 0)
                            {
                                paramName = "transactionType";
                                newParamName = "productType";
                            }
                            if(isOldVersion)
                            {
                                paramName = "transaction_type";
                                newParamName = "productType";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaTransactionType),
                            });
                            return ret;
                            
                        case "cancelrenewal":
                            paramName = "subscriptionId";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "subscription_id";
                                newParamName = "subscriptionId";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "cancelscheduledsubscription":
                            ret.Add("scheduledSubscriptionId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("scheduledSubscriptionId", "entitlement", "cancelScheduledSubscription") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            return ret;
                            
                        case "externalreconcile":
                            return ret;
                            
                        case "forcecancel":
                            paramName = "assetId";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "asset_id";
                                newParamName = "assetId";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("assetId", "entitlement", "forceCancel") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinInteger = 1,
                                },
                            });
                            paramName = "productType";
                            newParamName = null;
                            if(isOldVersion || currentVersion.CompareTo(new Version("4.7.0.0")) < 0)
                            {
                                paramName = "transactionType";
                                newParamName = "productType";
                            }
                            if(isOldVersion)
                            {
                                paramName = "transaction_type";
                                newParamName = "productType";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaTransactionType),
                            });
                            return ret;
                            
                        case "getnextrenewal":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "grant":
                            paramName = "productId";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "product_id";
                                newParamName = "productId";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("productId", "entitlement", "grant") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinInteger = 1,
                                },
                            });
                            paramName = "productType";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "product_type";
                                newParamName = "productType";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaTransactionType),
                            });
                            ret.Add("history", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(bool),
                            });
                            paramName = "contentId";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "content_id";
                                newParamName = "contentId";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = 0,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaEntitlementFilter),
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            return ret;
                            
                        case "listexpired":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaEntitlementsFilter),
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            return ret;
                            
                        case "listoldstandard":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaEntitlementsFilter),
                            });
                            return ret;
                            
                        case "swap":
                            ret.Add("currentProductId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("newProductId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("history", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(bool),
                            });
                            return ret;
                            
                        case "update":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("entitlement", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaEntitlement),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "epgchannel":
                    switch(action)
                    {
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaEpgChannelFilter),
                            });
                            ret.Add("with", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsList = true,
                                GenericType = typeof(KalturaCatalogWithHolder),
                                Type = typeof(List<KalturaCatalogWithHolder>),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "exporttask":
                    switch(action)
                    {
                        case "add":
                            ret.Add("task", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaExportTask),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                            });
                            return ret;
                            
                        case "get":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaExportTaskFilter),
                            });
                            return ret;
                            
                        case "listoldstandard":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaExportFilter),
                            });
                            return ret;
                            
                        case "update":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "exportTask", "update") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            ret.Add("task", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaExportTask),
                            });
                            return ret;
                            
                        case "updateoldstandard":
                            ret.Add("task", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaExportTask),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "externalchannelprofile":
                    switch(action)
                    {
                        case "add":
                            paramName = "externalChannel";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "external_channel";
                                newParamName = "externalChannel";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaExternalChannelProfile),
                            });
                            return ret;
                            
                        case "delete":
                            paramName = "externalChannelId";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "external_channel_id";
                                newParamName = "externalChannelId";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "list":
                            return ret;
                            
                        case "listoldstandard":
                            return ret;
                            
                        case "update":
                            ret.Add("externalChannelId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("externalChannel", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaExternalChannelProfile),
                            });
                            return ret;
                            
                        case "updateoldstandard":
                            ret.Add("external_channel", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaExternalChannelProfile),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "favorite":
                    switch(action)
                    {
                        case "add":
                            ret.Add("favorite", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFavorite),
                            });
                            return ret;
                            
                        case "addoldstandard":
                            ret.Add("media_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("media_type", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            ret.Add("extra_data", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "deleteoldstandard":
                            ret.Add("media_ids", new MethodParam(){
                                NewName = newParamName,
                                IsList = true,
                                GenericType = typeof(KalturaIntegerValue),
                                Type = typeof(List<KalturaIntegerValue>),
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFavoriteFilter),
                            });
                            return ret;
                            
                        case "listoldstandard":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFavoriteFilter),
                            });
                            ret.Add("with", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsList = true,
                                GenericType = typeof(KalturaCatalogWithHolder),
                                Type = typeof(List<KalturaCatalogWithHolder>),
                            });
                            ret.Add("udid", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "followtvseries":
                    switch(action)
                    {
                        case "add":
                            ret.Add("followTvSeries", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFollowTvSeries),
                            });
                            return ret;
                            
                        case "addoldstandard":
                            ret.Add("asset_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "delete":
                            paramName = "assetId";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "asset_id";
                                newParamName = "assetId";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("assetId", "followTvSeries", "delete") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinInteger = 1,
                                },
                            });
                            return ret;
                            
                        case "deletewithtoken":
                            ret.Add("assetId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("token", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("partnerId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFollowTvSeriesFilter),
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            return ret;
                            
                        case "listoldstandard":
                            ret.Add("order_by", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsNullable = true,
                                Type = typeof(KalturaOrder),
                                IsEnum = true,
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "homenetwork":
                    switch(action)
                    {
                        case "add":
                            paramName = "homeNetwork";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "home_network";
                                newParamName = "homeNetwork";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaHomeNetwork),
                            });
                            return ret;
                            
                        case "delete":
                            paramName = "externalId";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "external_id";
                                newParamName = "externalId";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "list":
                            return ret;
                            
                        case "listoldstandard":
                            return ret;
                            
                        case "update":
                            ret.Add("externalId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("homeNetwork", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaHomeNetwork),
                            });
                            return ret;
                            
                        case "updateoldstandard":
                            ret.Add("home_network", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaHomeNetwork),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "household":
                    switch(action)
                    {
                        case "add":
                            ret.Add("household", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaHousehold),
                            });
                            return ret;
                            
                        case "addoldstandard":
                            ret.Add("name", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("description", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("external_id", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsNullable = true,
                                Type = typeof(Int32),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "household", "delete") {
                                    RequiresPermission = true,
                                    MaxLength = -1,
                                    MinLength = -1,
                                },
                            });
                            return ret;
                            
                        case "deletebyoperator":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaIdentifierTypeFilter),
                            });
                            return ret;
                            
                        case "get":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsNullable = true,
                                Type = typeof(Int32),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "household", "get") {
                                    RequiresPermission = true,
                                    MaxLength = -1,
                                    MinLength = -1,
                                },
                            });
                            return ret;
                            
                        case "getbyoperator":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaIdentifierTypeFilter),
                            });
                            ret.Add("with", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsList = true,
                                GenericType = typeof(KalturaHouseholdWithHolder),
                                Type = typeof(List<KalturaHouseholdWithHolder>),
                            });
                            return ret;
                            
                        case "getchargeid":
                            ret.Add("pg_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "getoldstandard":
                            ret.Add("with", new MethodParam(){
                                NewName = newParamName,
                                IsList = true,
                                GenericType = typeof(KalturaHouseholdWithHolder),
                                Type = typeof(List<KalturaHouseholdWithHolder>),
                            });
                            return ret;
                            
                        case "purge":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "resetfrequency":
                            paramName = "frequencyType";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "household_frequency_type";
                                newParamName = "frequencyType";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaHouseholdFrequencyType),
                            });
                            return ret;
                            
                        case "resume":
                            return ret;
                            
                        case "setchargeid":
                            ret.Add("pg_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("charge_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "setpaymentmethodexternalid":
                            ret.Add("payment_gateway_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("payment_method_name", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("payment_details", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("payment_method_external_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "suspend":
                            ret.Add("roleId", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsNullable = true,
                                Type = typeof(Int32),
                            });
                            return ret;
                            
                        case "update":
                            ret.Add("household", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaHousehold),
                            });
                            return ret;
                            
                        case "updateoldstandard":
                            ret.Add("name", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("description", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "householddevice":
                    switch(action)
                    {
                        case "add":
                            ret.Add("device", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaHouseholdDevice),
                            });
                            return ret;
                            
                        case "addbypin":
                            paramName = "deviceName";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "device_name";
                                newParamName = "deviceName";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("pin", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "addoldstandard":
                            ret.Add("device_name", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("device_brand_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("udid", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("udid", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "generatepin":
                            ret.Add("udid", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            paramName = "brandId";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "brand_id";
                                newParamName = "brandId";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "get":
                            return ret;
                            
                        case "getstatus":
                            ret.Add("udid", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaHouseholdDeviceFilter),
                            });
                            return ret;
                            
                        case "loginwithpin":
                            ret.Add("partnerId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("pin", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("udid", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "update":
                            ret.Add("udid", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("device", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaHouseholdDevice),
                            });
                            return ret;
                            
                        case "updateoldstandard":
                            ret.Add("device_name", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("udid", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "updatestatus":
                            ret.Add("udid", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("status", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaDeviceStatus),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "householdlimitations":
                    switch(action)
                    {
                        case "get":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "householdpaymentgateway":
                    switch(action)
                    {
                        case "disable":
                            ret.Add("paymentGatewayId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "enable":
                            ret.Add("paymentGatewayId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "getchargeid":
                            ret.Add("paymentGatewayExternalId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "invoke":
                            ret.Add("paymentGatewayId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("intent", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            paramName = "extraParameters";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "extra_parameters";
                                newParamName = "extraParameters";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                IsList = true,
                                GenericType = typeof(KalturaKeyValue),
                                Type = typeof(List<KalturaKeyValue>),
                            });
                            return ret;
                            
                        case "list":
                            return ret;
                            
                        case "resume":
                            ret.Add("paymentGatewayId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "setchargeid":
                            ret.Add("paymentGatewayExternalId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("chargeId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "suspend":
                            ret.Add("paymentGatewayId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "householdpaymentmethod":
                    switch(action)
                    {
                        case "add":
                            ret.Add("householdPaymentMethod", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaHouseholdPaymentMethod),
                            });
                            return ret;
                            
                        case "forceremove":
                            ret.Add("paymentGatewayId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("paymentMethodId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("paymentMethodId", "householdPaymentMethod", "forceRemove") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinInteger = 1,
                                },
                            });
                            return ret;
                            
                        case "list":
                            return ret;
                            
                        case "remove":
                            ret.Add("paymentGatewayId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("paymentMethodId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("paymentMethodId", "householdPaymentMethod", "remove") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinInteger = 1,
                                },
                            });
                            return ret;
                            
                        case "setasdefault":
                            ret.Add("paymentGatewayId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("paymentMethodId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("paymentMethodId", "householdPaymentMethod", "setAsDefault") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinInteger = 1,
                                },
                            });
                            return ret;
                            
                        case "setexternalid":
                            ret.Add("paymentGatewayId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("paymentMethodName", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("paymentDetails", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("paymentMethodExternalId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "householdpremiumservice":
                    switch(action)
                    {
                        case "list":
                            return ret;
                            
                        case "listoldstandard":
                            return ret;
                            
                    }
                    break;
                    
                case "householdquota":
                    switch(action)
                    {
                        case "get":
                            return ret;
                            
                    }
                    break;
                    
                case "householduser":
                    switch(action)
                    {
                        case "add":
                            ret.Add("householdUser", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaHouseholdUser),
                            });
                            return ret;
                            
                        case "addbyoperator":
                            ret.Add("user_id_to_add", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("household_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("is_master", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = false,
                                Type = typeof(bool),
                            });
                            return ret;
                            
                        case "addoldstandard":
                            ret.Add("user_id_to_add", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("is_master", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = false,
                                Type = typeof(bool),
                            });
                            return ret;
                            
                        case "delete":
                            paramName = "id";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "user_id_to_delete";
                                newParamName = "id";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaHouseholdUserFilter),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "image":
                    switch(action)
                    {
                        case "add":
                            ret.Add("image", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaImage),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "image", "delete") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaImageFilter),
                            });
                            return ret;
                            
                        case "setcontent":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "image", "setContent") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            ret.Add("content", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaContentResource),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "imagetype":
                    switch(action)
                    {
                        case "add":
                            ret.Add("imageType", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaImageType),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "imageType", "delete") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaImageTypeFilter),
                            });
                            return ret;
                            
                        case "update":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "imageType", "update") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            ret.Add("imageType", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaImageType),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "inboxmessage":
                    switch(action)
                    {
                        case "get":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaInboxMessageFilter),
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            return ret;
                            
                        case "updatestatus":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("status", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaInboxMessageStatus),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "language":
                    switch(action)
                    {
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaLanguageFilter),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "lastposition":
                    switch(action)
                    {
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaLastPositionFilter),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "licensedurl":
                    switch(action)
                    {
                        case "get":
                            ret.Add("request", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaLicensedUrlBaseRequest),
                            });
                            return ret;
                            
                        case "getoldstandard":
                            paramName = "assetType";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "asset_type";
                                newParamName = "assetType";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaAssetType),
                            });
                            paramName = "contentId";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "content_id";
                                newParamName = "contentId";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            paramName = "baseUrl";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "base_url";
                                newParamName = "baseUrl";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            paramName = "assetId";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "asset_id";
                                newParamName = "assetId";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            paramName = "startDate";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "start_date";
                                newParamName = "startDate";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsNullable = true,
                                Type = typeof(Int64),
                            });
                            paramName = "streamType";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "stream_type";
                                newParamName = "streamType";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsNullable = true,
                                Type = typeof(KalturaStreamType),
                                IsEnum = true,
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "mediaconcurrencyrule":
                    switch(action)
                    {
                        case "list":
                            return ret;
                            
                    }
                    break;
                    
                case "mediafile":
                    switch(action)
                    {
                        case "add":
                            ret.Add("mediaFile", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaMediaFile),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "mediaFile", "delete") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaMediaFileFilter),
                            });
                            return ret;
                            
                        case "update":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "mediaFile", "update") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            ret.Add("mediaFile", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaMediaFile),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "mediafiletype":
                    switch(action)
                    {
                        case "add":
                            ret.Add("mediaFileType", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaMediaFileType),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "list":
                            return ret;
                            
                        case "update":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("mediaFileType", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaMediaFileType),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "messagetemplate":
                    switch(action)
                    {
                        case "get":
                            paramName = "messageType";
                            newParamName = null;
                            if(isOldVersion || currentVersion.CompareTo(new Version("3.6.2094.15157")) < 0)
                            {
                                paramName = "assetType";
                                newParamName = "messageType";
                            }
                            if(isOldVersion)
                            {
                                paramName = "asset_Type";
                                newParamName = "messageType";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaMessageTemplateType),
                            });
                            return ret;
                            
                        case "set":
                            ret.Add("message_template", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaMessageTemplate),
                            });
                            return ret;
                            
                        case "update":
                            paramName = "messageType";
                            newParamName = null;
                            if(isOldVersion || currentVersion.CompareTo(new Version("3.6.2094.15157")) < 0)
                            {
                                paramName = "assetType";
                                newParamName = "messageType";
                            }
                            if(isOldVersion)
                            {
                                paramName = "asset_Type";
                                newParamName = "messageType";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaMessageTemplateType),
                            });
                            ret.Add("template", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaMessageTemplate),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "meta":
                    switch(action)
                    {
                        case "add":
                            ret.Add("meta", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaMeta),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "meta", "delete") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaMetaFilter),
                            });
                            return ret;
                            
                        case "listoldstandard":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaMetaFilter),
                            });
                            return ret;
                            
                        case "update":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "meta", "update") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            ret.Add("meta", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaMeta),
                            });
                            return ret;
                            
                        case "updateoldstandard":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("meta", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaMeta),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "multirequest":
                    switch(action)
                    {
                        case "do":
                            ret.Add("request", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(KalturaMultiRequestAction[]),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "notification":
                    switch(action)
                    {
                        case "register":
                            ret.Add("identifier", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("type", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaNotificationType),
                            });
                            return ret;
                            
                        case "sendpush":
                            ret.Add("userId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("pushMessage", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaPushMessage),
                            });
                            return ret;
                            
                        case "sendsms":
                            ret.Add("message", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "setdevicepushtoken":
                            ret.Add("pushToken", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "notificationspartnersettings":
                    switch(action)
                    {
                        case "get":
                            return ret;
                            
                        case "getoldstandard":
                            return ret;
                            
                        case "update":
                            ret.Add("settings", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaNotificationsPartnerSettings),
                            });
                            return ret;
                            
                        case "updateoldstandard":
                            ret.Add("settings", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaPartnerNotificationSettings),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "notificationssettings":
                    switch(action)
                    {
                        case "get":
                            return ret;
                            
                        case "getoldstandard":
                            return ret;
                            
                        case "update":
                            ret.Add("settings", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaNotificationsSettings),
                            });
                            return ret;
                            
                        case "updateoldstandard":
                            ret.Add("settings", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaNotificationSettings),
                            });
                            return ret;
                            
                        case "updatewithtoken":
                            ret.Add("settings", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaNotificationsSettings),
                            });
                            ret.Add("token", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("partnerId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "ossadapterprofile":
                    switch(action)
                    {
                        case "add":
                            paramName = "ossAdapter";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "oss_adapter";
                                newParamName = "ossAdapter";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaOSSAdapterProfile),
                            });
                            return ret;
                            
                        case "delete":
                            paramName = "ossAdapterId";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "oss_adapter_id";
                                newParamName = "ossAdapterId";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "generatesharedsecret":
                            paramName = "ossAdapterId";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "oss_adapter_id";
                                newParamName = "ossAdapterId";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "get":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "list":
                            return ret;
                            
                        case "listoldstandard":
                            return ret;
                            
                        case "update":
                            ret.Add("ossAdapterId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("ossAdapter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaOSSAdapterProfile),
                            });
                            return ret;
                            
                        case "updateoldstandard":
                            ret.Add("oss_adapter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaOSSAdapterProfile),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "ossadapterprofilesettings":
                    switch(action)
                    {
                        case "add":
                            ret.Add("oss_adapter_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("settings", new MethodParam(){
                                NewName = newParamName,
                                IsMap = true,
                                GenericType = typeof(KalturaStringValue),
                                Type = typeof(SerializableDictionary<string, KalturaStringValue>),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("oss_adapter_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("settings", new MethodParam(){
                                NewName = newParamName,
                                IsMap = true,
                                GenericType = typeof(KalturaStringValue),
                                Type = typeof(SerializableDictionary<string, KalturaStringValue>),
                            });
                            return ret;
                            
                        case "list":
                            return ret;
                            
                        case "update":
                            ret.Add("oss_adapter_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("settings", new MethodParam(){
                                NewName = newParamName,
                                IsMap = true,
                                GenericType = typeof(KalturaStringValue),
                                Type = typeof(SerializableDictionary<string, KalturaStringValue>),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "ottcategory":
                    switch(action)
                    {
                        case "get":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "ottuser":
                    switch(action)
                    {
                        case "activate":
                            ret.Add("partnerId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("username", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            paramName = "activationToken";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "activation_token";
                                newParamName = "activationToken";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "addrole":
                            paramName = "roleId";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "role_id";
                                newParamName = "roleId";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                            });
                            return ret;
                            
                        case "anonymouslogin":
                            ret.Add("partnerId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("udid", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "delete":
                            return ret;
                            
                        case "facebooklogin":
                            ret.Add("partnerId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("token", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("udid", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "get":
                            return ret;
                            
                        case "getencrypteduserid":
                            return ret;
                            
                        case "getoldstandard":
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaOTTUserFilter),
                            });
                            return ret;
                            
                        case "login":
                            ret.Add("partnerId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("username", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            ret.Add("password", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            paramName = "extraParams";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "extra_params";
                                newParamName = "extraParams";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsMap = true,
                                GenericType = typeof(KalturaStringValue),
                                Type = typeof(SerializableDictionary<string, KalturaStringValue>),
                            });
                            ret.Add("udid", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "loginwithpin":
                            ret.Add("partnerId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("pin", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("udid", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            ret.Add("secret", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "logout":
                            return ret;
                            
                        case "refreshsession":
                            paramName = "refreshToken";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "refresh_token";
                                newParamName = "refreshToken";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("udid", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "register":
                            ret.Add("partnerId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("user", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaOTTUser),
                            });
                            ret.Add("password", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("password", "ottUser", "register") {
                                    RequiresPermission = false,
                                    MaxLength = 128,
                                    MinLength = -1,
                                },
                            });
                            return ret;
                            
                        case "resendactivationtoken":
                            ret.Add("partnerId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("username", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "resetpassword":
                            ret.Add("partnerId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("username", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("templateName", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("templateName", "ottUser", "resetPassword") {
                                    RequiresPermission = true,
                                    MaxLength = -1,
                                    MinLength = -1,
                                },
                            });
                            return ret;
                            
                        case "setinitialpassword":
                            ret.Add("partnerId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("token", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("password", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "setpassword":
                            ret.Add("partnerId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("username", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("password", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "update":
                            ret.Add("user", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaOTTUser),
                            });
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "ottUser", "update") {
                                    RequiresPermission = true,
                                    MaxLength = -1,
                                    MinLength = -1,
                                },
                            });
                            return ret;
                            
                        case "updatedynamicdata":
                            ret.Add("key", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("value", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaStringValue),
                            });
                            return ret;
                            
                        case "updatelogindata":
                            ret.Add("username", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            paramName = "oldPassword";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "old_password";
                                newParamName = "oldPassword";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            paramName = "newPassword";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "new_password";
                                newParamName = "newPassword";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "updatepassword":
                            ret.Add("userId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("password", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "validatetoken":
                            ret.Add("partnerId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("token", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "parentalrule":
                    switch(action)
                    {
                        case "add":
                            ret.Add("parentalRule", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaParentalRule),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "parentalRule", "delete") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            return ret;
                            
                        case "disable":
                            paramName = "ruleId";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "rule_id";
                                newParamName = "ruleId";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                            });
                            paramName = "entityReference";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "by";
                                newParamName = "entityReference";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaEntityReferenceBy),
                            });
                            return ret;
                            
                        case "disabledefault":
                            paramName = "entityReference";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "by";
                                newParamName = "entityReference";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaEntityReferenceBy),
                            });
                            return ret;
                            
                        case "enable":
                            paramName = "ruleId";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "rule_id";
                                newParamName = "ruleId";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                            });
                            paramName = "entityReference";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "by";
                                newParamName = "entityReference";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaEntityReferenceBy),
                            });
                            return ret;
                            
                        case "get":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "parentalRule", "get") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaParentalRuleFilter),
                            });
                            return ret;
                            
                        case "listoldstandard":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaRuleFilter),
                            });
                            return ret;
                            
                        case "update":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "parentalRule", "update") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            ret.Add("parentalRule", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaParentalRule),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "parentalruleprofile":
                    switch(action)
                    {
                        case "list":
                            return ret;
                            
                    }
                    break;
                    
                case "partnerconfiguration":
                    switch(action)
                    {
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaPartnerConfigurationFilter),
                            });
                            return ret;
                            
                        case "update":
                            ret.Add("configuration", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaPartnerConfiguration),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "paymentgateway":
                    switch(action)
                    {
                        case "delete":
                            paramName = "paymentGatewayId";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "payment_gateway_id";
                                newParamName = "paymentGatewayId";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "forceremovepaymentmethod":
                            ret.Add("payment_gateway_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("payment_gateway_id", "paymentGateway", "forceRemovePaymentMethod") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinInteger = 1,
                                },
                            });
                            ret.Add("payment_method_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("payment_method_id", "paymentGateway", "forceRemovePaymentMethod") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinInteger = 1,
                                },
                            });
                            return ret;
                            
                        case "list":
                            return ret;
                            
                        case "removepaymentmethod":
                            ret.Add("payment_gateway_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("payment_gateway_id", "paymentGateway", "removePaymentMethod") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinInteger = 1,
                                },
                            });
                            ret.Add("payment_method_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("payment_method_id", "paymentGateway", "removePaymentMethod") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinInteger = 1,
                                },
                            });
                            return ret;
                            
                        case "set":
                            ret.Add("payment_gateway_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "setpaymentmethod":
                            ret.Add("payment_gateway_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("payment_gateway_id", "paymentGateway", "setPaymentMethod") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinInteger = 1,
                                },
                            });
                            ret.Add("payment_method_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("payment_method_id", "paymentGateway", "setPaymentMethod") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinInteger = 1,
                                },
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "paymentgatewayprofile":
                    switch(action)
                    {
                        case "add":
                            ret.Add("paymentGateway", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaPaymentGatewayProfile),
                            });
                            return ret;
                            
                        case "addoldstandard":
                            paramName = "paymentGateway";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "payment_gateway";
                                newParamName = "paymentGateway";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaPaymentGatewayProfile),
                            });
                            return ret;
                            
                        case "delete":
                            paramName = "paymentGatewayId";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "payment_gateway_id";
                                newParamName = "paymentGatewayId";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "generatesharedsecret":
                            paramName = "paymentGatewayId";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "payment_gateway_id";
                                newParamName = "paymentGatewayId";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "getconfiguration":
                            ret.Add("alias", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("intent", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            paramName = "extraParameters";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "extra_parameters";
                                newParamName = "extraParameters";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                IsList = true,
                                GenericType = typeof(KalturaKeyValue),
                                Type = typeof(List<KalturaKeyValue>),
                            });
                            return ret;
                            
                        case "list":
                            return ret;
                            
                        case "listoldstandard":
                            return ret;
                            
                        case "update":
                            ret.Add("paymentGatewayId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("paymentGateway", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaPaymentGatewayProfile),
                            });
                            return ret;
                            
                        case "updateoldstandard":
                            paramName = "paymentGatewayId";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "payment_gateway_id";
                                newParamName = "paymentGatewayId";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            paramName = "paymentGateway";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "payment_gateway";
                                newParamName = "paymentGateway";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaPaymentGatewayProfile),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "paymentgatewayprofilesettings":
                    switch(action)
                    {
                        case "add":
                            ret.Add("profile", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaPaymentGatewayProfile),
                            });
                            return ret;
                            
                        case "addoldstandard":
                            ret.Add("payment_gateway_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("settings", new MethodParam(){
                                NewName = newParamName,
                                IsMap = true,
                                GenericType = typeof(KalturaStringValue),
                                Type = typeof(SerializableDictionary<string, KalturaStringValue>),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("payment_gateway_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("settings", new MethodParam(){
                                NewName = newParamName,
                                IsMap = true,
                                GenericType = typeof(KalturaStringValue),
                                Type = typeof(SerializableDictionary<string, KalturaStringValue>),
                            });
                            return ret;
                            
                        case "list":
                            return ret;
                            
                        case "update":
                            ret.Add("payment_gateway_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("settings", new MethodParam(){
                                NewName = newParamName,
                                IsMap = true,
                                GenericType = typeof(KalturaStringValue),
                                Type = typeof(SerializableDictionary<string, KalturaStringValue>),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "paymentmethodprofile":
                    switch(action)
                    {
                        case "add":
                            ret.Add("paymentMethod", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaPaymentMethodProfile),
                            });
                            return ret;
                            
                        case "addoldstandard":
                            ret.Add("payment_gateway_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("payment_method", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaPaymentMethodProfile),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("paymentMethodId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "deleteoldstandard":
                            ret.Add("payment_gateway_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("payment_method_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaPaymentMethodProfileFilter),
                            });
                            return ret;
                            
                        case "listoldstandard":
                            ret.Add("payment_gateway_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "update":
                            ret.Add("paymentMethodId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("paymentMethod", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaPaymentMethodProfile),
                            });
                            return ret;
                            
                        case "updateoldstandard":
                            paramName = "paymentGatewayId";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "payment_gateway_id";
                                newParamName = "paymentGatewayId";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            paramName = "paymentMethod";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "payment_method";
                                newParamName = "paymentMethod";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaPaymentMethodProfile),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "permission":
                    switch(action)
                    {
                        case "getcurrentpermissions":
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaPermissionFilter),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "personalasset":
                    switch(action)
                    {
                        case "list":
                            ret.Add("assets", new MethodParam(){
                                NewName = newParamName,
                                IsList = true,
                                GenericType = typeof(KalturaPersonalAssetRequest),
                                Type = typeof(List<KalturaPersonalAssetRequest>),
                            });
                            ret.Add("with", new MethodParam(){
                                NewName = newParamName,
                                IsList = true,
                                GenericType = typeof(KalturaPersonalAssetWithHolder),
                                Type = typeof(List<KalturaPersonalAssetWithHolder>),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "personalfeed":
                    switch(action)
                    {
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaPersonalFeedFilter),
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            return ret;
                            
                        case "listoldstandard":
                            ret.Add("order_by", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsNullable = true,
                                Type = typeof(KalturaOrder),
                                IsEnum = true,
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "personallist":
                    switch(action)
                    {
                        case "add":
                            ret.Add("personalList", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaPersonalList),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("personalListId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("personalListId", "personalList", "delete") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaPersonalListFilter),
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "pin":
                    switch(action)
                    {
                        case "get":
                            ret.Add("by", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaEntityReferenceBy),
                            });
                            ret.Add("type", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaPinType),
                            });
                            ret.Add("ruleId", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsNullable = true,
                                Type = typeof(Int32),
                            });
                            return ret;
                            
                        case "getoldstandard":
                            ret.Add("by", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaEntityReferenceBy),
                            });
                            ret.Add("type", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaPinType),
                            });
                            ret.Add("ruleId", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsNullable = true,
                                Type = typeof(Int32),
                            });
                            return ret;
                            
                        case "update":
                            ret.Add("by", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaEntityReferenceBy),
                            });
                            ret.Add("type", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaPinType),
                            });
                            ret.Add("pin", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaPin),
                            });
                            ret.Add("ruleId", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsNullable = true,
                                Type = typeof(Int32),
                            });
                            return ret;
                            
                        case "updateoldstandard":
                            ret.Add("pin", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("by", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaEntityReferenceBy),
                            });
                            ret.Add("type", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaPinType),
                            });
                            ret.Add("ruleId", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsNullable = true,
                                Type = typeof(Int32),
                            });
                            return ret;
                            
                        case "validate":
                            ret.Add("pin", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("type", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaPinType),
                            });
                            ret.Add("ruleId", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsNullable = true,
                                Type = typeof(Int32),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "ppv":
                    switch(action)
                    {
                        case "get":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "price":
                    switch(action)
                    {
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaPricesFilter),
                            });
                            ret.Add("coupon_code", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "pricedetails":
                    switch(action)
                    {
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaPriceDetailsFilter),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "priceplan":
                    switch(action)
                    {
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaPricePlanFilter),
                            });
                            return ret;
                            
                        case "update":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                            });
                            ret.Add("pricePlan", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaPricePlan),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "productprice":
                    switch(action)
                    {
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaProductPriceFilter),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "purchasesettings":
                    switch(action)
                    {
                        case "get":
                            ret.Add("by", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaEntityReferenceBy),
                            });
                            return ret;
                            
                        case "getoldstandard":
                            ret.Add("by", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaEntityReferenceBy),
                            });
                            return ret;
                            
                        case "update":
                            ret.Add("entityReference", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaEntityReferenceBy),
                            });
                            ret.Add("settings", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaPurchaseSettings),
                            });
                            return ret;
                            
                        case "updateoldstandard":
                            ret.Add("setting", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("by", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaEntityReferenceBy),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "ratio":
                    switch(action)
                    {
                        case "add":
                            ret.Add("ratio", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaRatio),
                            });
                            return ret;
                            
                        case "list":
                            return ret;
                            
                        case "update":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                            });
                            ret.Add("ratio", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaRatio),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "recommendationprofile":
                    switch(action)
                    {
                        case "add":
                            paramName = "recommendationEngine";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "recommendation_engine";
                                newParamName = "recommendationEngine";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaRecommendationProfile),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "generatesharedsecret":
                            paramName = "recommendationEngineId";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "recommendation_engine_id";
                                newParamName = "recommendationEngineId";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "list":
                            return ret;
                            
                        case "listoldstandard":
                            return ret;
                            
                        case "update":
                            ret.Add("recommendationEngineId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("recommendationEngine", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaRecommendationProfile),
                            });
                            return ret;
                            
                        case "updateoldstandard":
                            ret.Add("recommendation_engine", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaRecommendationProfile),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "recommendationprofilesettings":
                    switch(action)
                    {
                        case "add":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("settings", new MethodParam(){
                                NewName = newParamName,
                                IsMap = true,
                                GenericType = typeof(KalturaStringValue),
                                Type = typeof(SerializableDictionary<string, KalturaStringValue>),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("settings", new MethodParam(){
                                NewName = newParamName,
                                IsMap = true,
                                GenericType = typeof(KalturaStringValue),
                                Type = typeof(SerializableDictionary<string, KalturaStringValue>),
                            });
                            return ret;
                            
                        case "list":
                            return ret;
                            
                        case "update":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("settings", new MethodParam(){
                                NewName = newParamName,
                                IsMap = true,
                                GenericType = typeof(KalturaStringValue),
                                Type = typeof(SerializableDictionary<string, KalturaStringValue>),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "recording":
                    switch(action)
                    {
                        case "add":
                            ret.Add("recording", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaRecording),
                            });
                            return ret;
                            
                        case "cancel":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                            });
                            return ret;
                            
                        case "get":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaRecordingFilter),
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            return ret;
                            
                        case "protect":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "region":
                    switch(action)
                    {
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaRegionFilter),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "registrysettings":
                    switch(action)
                    {
                        case "list":
                            return ret;
                            
                        case "listoldstandard":
                            return ret;
                            
                    }
                    break;
                    
                case "reminder":
                    switch(action)
                    {
                        case "add":
                            ret.Add("reminder", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaReminder),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                            });
                            ret.Add("type", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaReminderType),
                            });
                            return ret;
                            
                        case "deletewithtoken":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                            });
                            ret.Add("type", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaReminderType),
                            });
                            ret.Add("token", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("partnerId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaReminderFilter<>),
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "report":
                    switch(action)
                    {
                        case "get":
                            ret.Add("udid", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaReportFilter),
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "searchhistory":
                    switch(action)
                    {
                        case "clean":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaSearchHistoryFilter),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaSearchHistoryFilter),
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "seriesrecording":
                    switch(action)
                    {
                        case "add":
                            ret.Add("recording", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaSeriesRecording),
                            });
                            return ret;
                            
                        case "cancel":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                            });
                            return ret;
                            
                        case "cancelbyepgid":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                            });
                            ret.Add("epgId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("epgId", "seriesRecording", "cancelByEpgId") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            return ret;
                            
                        case "cancelbyseasonnumber":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                            });
                            ret.Add("seasonNumber", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("seasonNumber", "seriesRecording", "cancelBySeasonNumber") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                            });
                            return ret;
                            
                        case "deletebyseasonnumber":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                            });
                            ret.Add("seasonNumber", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("seasonNumber", "seriesRecording", "deleteBySeasonNumber") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinInteger = 1,
                                },
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaSeriesRecordingFilter),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "session":
                    switch(action)
                    {
                        case "get":
                            ret.Add("session", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("session", "session", "get") {
                                    RequiresPermission = true,
                                    MaxLength = -1,
                                    MinLength = -1,
                                },
                            });
                            return ret;
                            
                        case "getoldstandard":
                            paramName = "session";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "ks_to_parse";
                                newParamName = "session";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "revoke":
                            return ret;
                            
                        case "switchuser":
                            ret.Add("userIdToSwitch", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "socialaction":
                    switch(action)
                    {
                        case "add":
                            ret.Add("socialAction", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaSocialAction),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaSocialActionFilter),
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "socialcomment":
                    switch(action)
                    {
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaSocialCommentFilter),
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "social":
                    switch(action)
                    {
                        case "get":
                            ret.Add("type", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaSocialNetwork),
                            });
                            return ret;
                            
                        case "getbytoken":
                            ret.Add("partnerId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("token", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("type", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaSocialNetwork),
                            });
                            return ret;
                            
                        case "getbytokenoldstandard":
                            ret.Add("partnerId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("token", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("type", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaSocialNetwork),
                            });
                            return ret;
                            
                        case "getconfiguration":
                            ret.Add("type", new MethodParam(){
                                NewName = newParamName,
                                IsNullable = true,
                                Type = typeof(KalturaSocialNetwork),
                                IsEnum = true,
                            });
                            ret.Add("partnerId", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsNullable = true,
                                Type = typeof(Int32),
                            });
                            return ret;
                            
                        case "login":
                            ret.Add("partnerId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("token", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("type", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaSocialNetwork),
                            });
                            ret.Add("udid", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "merge":
                            ret.Add("token", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("type", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaSocialNetwork),
                            });
                            return ret;
                            
                        case "mergeoldstandard":
                            ret.Add("partnerId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("token", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("username", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("password", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("social_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("type", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaSocialNetwork),
                            });
                            return ret;
                            
                        case "register":
                            ret.Add("partnerId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("token", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("type", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaSocialNetwork),
                            });
                            ret.Add("email", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "registeroldstandard":
                            ret.Add("partnerId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("token", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("should_create_domain", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(bool),
                            });
                            ret.Add("subscribe_newsletter", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(bool),
                            });
                            ret.Add("type", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaSocialNetwork),
                            });
                            ret.Add("email", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "unmerge":
                            ret.Add("type", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaSocialNetwork),
                            });
                            return ret;
                            
                        case "unmergeoldstandard":
                            ret.Add("token", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("username", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("password", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("type", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaSocialNetwork),
                            });
                            return ret;
                            
                        case "updateconfiguration":
                            ret.Add("configuration", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaSocialConfig),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "socialfriendactivity":
                    switch(action)
                    {
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaSocialFriendActivityFilter),
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "ssoadapterprofile":
                    switch(action)
                    {
                        case "add":
                            ret.Add("ssoAdapter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaSSOAdapterProfile),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("ssoAdapterId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "generatesharedsecret":
                            ret.Add("ssoAdapterId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "list":
                            return ret;
                            
                        case "update":
                            ret.Add("ssoAdapterId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("ssoAdapter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaSSOAdapterProfile),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "subscription":
                    switch(action)
                    {
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaSubscriptionFilter),
                            });
                            return ret;
                            
                        case "listoldstandard":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaSubscriptionsFilter),
                            });
                            return ret;
                            
                        case "validatecoupon":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("code", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "subscriptionset":
                    switch(action)
                    {
                        case "add":
                            ret.Add("subscriptionSet", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaSubscriptionSet),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "subscriptionSet", "delete") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            return ret;
                            
                        case "get":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "subscriptionSet", "get") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaSubscriptionSetFilter),
                            });
                            return ret;
                            
                        case "update":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "subscriptionSet", "update") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            ret.Add("subscriptionSet", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaSubscriptionSet),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "system":
                    switch(action)
                    {
                        case "getcountry":
                            ret.Add("ip", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "gettime":
                            return ret;
                            
                        case "getversion":
                            return ret;
                            
                        case "ping":
                            return ret;
                            
                    }
                    break;
                    
                case "tag":
                    switch(action)
                    {
                        case "add":
                            ret.Add("tag", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaTag),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "tag", "delete") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaTagFilter),
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            return ret;
                            
                        case "update":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                            });
                            ret.Add("tag", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaTag),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "timeshiftedtvpartnersettings":
                    switch(action)
                    {
                        case "get":
                            return ret;
                            
                        case "update":
                            ret.Add("settings", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaTimeShiftedTvPartnerSettings),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "topic":
                    switch(action)
                    {
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "get":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "topic", "get") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinInteger = 1,
                                },
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaTopicFilter),
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            return ret;
                            
                        case "listoldstandard":
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            return ret;
                            
                        case "updatestatus":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("automaticIssueNotification", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaTopicAutomaticIssueNotification),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "transaction":
                    switch(action)
                    {
                        case "downgrade":
                            ret.Add("purchase", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaPurchase),
                            });
                            return ret;
                            
                        case "getpurchasesessionid":
                            ret.Add("purchaseSession", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaPurchaseSession),
                            });
                            return ret;
                            
                        case "processreceipt":
                            ret.Add("product_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("product_type", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaTransactionType),
                            });
                            ret.Add("purchase_receipt", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("payment_gateway_name", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("content_id", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = 0,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "purchase":
                            ret.Add("purchase", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaPurchase),
                            });
                            return ret;
                            
                        case "purchaseoldstandard":
                            ret.Add("price", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(float),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("price", "transaction", "purchaseOldStandard") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinFloat = 0,
                                },
                            });
                            ret.Add("currency", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("product_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("product_id", "transaction", "purchaseOldStandard") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinInteger = 1,
                                },
                            });
                            ret.Add("product_type", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaTransactionType),
                            });
                            ret.Add("content_id", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = 0,
                                Type = typeof(int),
                            });
                            ret.Add("coupon", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            ret.Add("payment_gateway_id", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = 0,
                                Type = typeof(int),
                            });
                            ret.Add("payment_method_id", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsNullable = true,
                                Type = typeof(Int32),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("payment_method_id", "transaction", "purchaseOldStandard") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinInteger = 1,
                                },
                            });
                            ret.Add("adapterData", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "purchasesessionidoldstandard":
                            ret.Add("price", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(float),
                            });
                            ret.Add("currency", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("product_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("product_type", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaTransactionType),
                            });
                            ret.Add("content_id", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = 0,
                                Type = typeof(int),
                            });
                            ret.Add("coupon", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            ret.Add("preview_module_id", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = 0,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "setwaiver":
                            paramName = "assetId";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "asset_id";
                                newParamName = "assetId";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("assetId", "transaction", "setWaiver") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinInteger = 1,
                                },
                            });
                            paramName = "transactionType";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "transaction_type";
                                newParamName = "transactionType";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaTransactionType),
                            });
                            return ret;
                            
                        case "updatestate":
                            ret.Add("payment_gateway_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("adapter_transaction_state", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("external_transaction_id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("external_status", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("external_message", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("fail_reason", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            ret.Add("signature", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "updatestatus":
                            ret.Add("paymentGatewayId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("externalTransactionId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("signature", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("status", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaTransactionStatus),
                            });
                            return ret;
                            
                        case "upgrade":
                            ret.Add("purchase", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaPurchase),
                            });
                            return ret;
                            
                        case "validatereceipt":
                            ret.Add("externalReceipt", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaExternalReceipt),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "transactionhistory":
                    switch(action)
                    {
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaTransactionHistoryFilter),
                            });
                            ret.Add("pager", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaFilterPager),
                            });
                            return ret;
                            
                        case "listoldstandard":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaTransactionsFilter),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "unifiedpayment":
                    switch(action)
                    {
                        case "getnextrenewal":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "uploadtoken":
                    switch(action)
                    {
                        case "add":
                            ret.Add("uploadToken", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaUploadToken),
                            });
                            return ret;
                            
                        case "upload":
                            ret.Add("uploadTokenId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("fileData", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(KalturaOTTFile),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "userassetrule":
                    switch(action)
                    {
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaUserAssetRuleFilter),
                            });
                            return ret;
                            
                        case "listoldstandard":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaGenericRuleFilter),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "userassetslist":
                    switch(action)
                    {
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaUserAssetsListFilter),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "userassetslistitem":
                    switch(action)
                    {
                        case "add":
                            ret.Add("userAssetsListItem", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaUserAssetsListItem),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("assetId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("listType", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaUserAssetsListType),
                            });
                            return ret;
                            
                        case "deleteoldstandard":
                            ret.Add("userAssetsListItem", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaUserAssetsListItem),
                            });
                            return ret;
                            
                        case "get":
                            ret.Add("assetId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("listType", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaUserAssetsListType),
                            });
                            ret.Add("itemType", new MethodParam(){
                                NewName = newParamName,
                                IsEnum = true,
                                Type = typeof(KalturaUserAssetsListItemType),
                            });
                            return ret;
                            
                        case "getoldstandard":
                            ret.Add("userAssetsListItem", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaUserAssetsListItem),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "userinterest":
                    switch(action)
                    {
                        case "add":
                            ret.Add("userInterest", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaUserInterest),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "deletewithtoken":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("token", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("partnerId", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(int),
                            });
                            return ret;
                            
                        case "list":
                            return ret;
                            
                    }
                    break;
                    
                case "userloginpin":
                    switch(action)
                    {
                        case "add":
                            ret.Add("secret", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "delete":
                            paramName = "pinCode";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "pin_code";
                                newParamName = "pinCode";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            return ret;
                            
                        case "deleteall":
                            return ret;
                            
                        case "update":
                            paramName = "pinCode";
                            newParamName = null;
                            if(isOldVersion)
                            {
                                paramName = "pin_code";
                                newParamName = "pinCode";
                            }
                            ret.Add(paramName, new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(string),
                            });
                            ret.Add("secret", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                Type = typeof(string),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "userrole":
                    switch(action)
                    {
                        case "add":
                            ret.Add("role", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaUserRole),
                            });
                            return ret;
                            
                        case "delete":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                            });
                            return ret;
                            
                        case "list":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaUserRoleFilter),
                            });
                            return ret;
                            
                        case "listoldstandard":
                            ret.Add("filter", new MethodParam(){
                                NewName = newParamName,
                                IsOptional = true,
                                DefaultValue = null,
                                IsKalturaObject = true,
                                Type = typeof(KalturaUserRoleFilter),
                            });
                            return ret;
                            
                        case "update":
                            ret.Add("id", new MethodParam(){
                                NewName = newParamName,
                                Type = typeof(long),
                                SchemeArgument = new RuntimeSchemeArgumentAttribute("id", "userRole", "update") {
                                    RequiresPermission = false,
                                    MaxLength = -1,
                                    MinLength = -1,
                                    MinLong = 1,
                                },
                            });
                            ret.Add("role", new MethodParam(){
                                NewName = newParamName,
                                IsKalturaObject = true,
                                Type = typeof(KalturaUserRole),
                            });
                            return ret;
                            
                    }
                    break;
                    
                case "version":
                    switch(action)
                    {
                        case "":
                            return ret;
                            
                    }
                    break;
                    
            }
            
            throw new RequestParserException(RequestParserException.INVALID_ACTION, service, action);
        }
        
        public static HttpStatusCode? getFailureHttpCode(string service, string action)
        {
            service = service.ToLower();
            action = action.ToLower();
            switch (service)
            {
                case "assetfile":
                    switch(action)
                    {
                        case "playmanifest":
                            return HttpStatusCode.NotFound;
                            
                    }
                    break;
                    
            }
            
            return null;
        }
        
    }
}
