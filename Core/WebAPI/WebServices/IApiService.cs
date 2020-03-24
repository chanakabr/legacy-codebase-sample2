using System;
using System.Collections.Generic;
using System.ServiceModel;
using ApiObjects;
using ApiObjects.AssetLifeCycleRules;
using ApiObjects.BulkExport;
using ApiObjects.CDNAdapter;
using ApiObjects.Response;
using ApiObjects.Roles;
using ApiObjects.Rules;
using ApiObjects.TimeShiftedTv;
using Core.Catalog.Response;
using ScheduledTasks;

namespace WebAPI.WebServices
{
    [ServiceContract(Namespace= "http://api.tvinci.com/")]
    public interface IApiService
    {
        [OperationContract]
        BulkExportTaskResponse AddBulkExportTask(string sWSUserName, string sWSPassword, string externalKey, string name, eBulkExportDataType dataType, string filter, eBulkExportExportType exportType, long frequency, string notificationUrl, List<int> vodTypes, bool isActive);
        [OperationContract]
        Status AddPermissionItemToPermission(string sWSUserName, string sWSPassword, long permissionId, long permissionItemId);
        [OperationContract]
        Status AddPermissionToRole(string sWSUserName, string sWSPassword, long roleId, long permissionId);
        [OperationContract]
        bool AddUserSocialAction(string sWSUserName, string sWSPassword, int nMediaID, string sSiteGuid, SocialAction socialAction, SocialPlatform socialPlatform);
        [OperationContract]
        AdminAccountUserResponse AdminSignIn(string sWSUserName, string sWSPassword, string username, string pass);
        [OperationContract]
        bool BuildIPToCountryIndex(string userName, string password);
        [OperationContract]
        List<int> ChannelsContainingMedia(string sWSUserName, string sWSPassword, List<int> lChannels, int nMediaID, int nMediaFileID);
        [OperationContract]
        bool CheckDomainParentalPIN(string sWSUserName, string sWSPassword, int nDomainID, int nRuleID, string sParentalPIN);
        [OperationContract]
        string CheckGeoBlockMedia(string sWSUserName, string sWSPassword, int nMediaID, string sIP);
        [OperationContract]
        bool CheckGeoCommerceBlock(string sWSUserName, string sWSPassword, int SubscriptionGeoCommerceID, string sIP);
        [OperationContract]
        bool CheckParentalPIN(string sWSUserName, string sWSPassword, string sSiteGUID, int nRuleID, string sParentalPIN);
        [OperationContract]
        Status CleanUserHistory(string sWSUserName, string sWSPassword, string siteGuid, List<int> lMediaIDs);
        [OperationContract]
        Status DeleteBulkExportTask(string sWSUserName, string sWSPassword, long id, string externalKey);
        [OperationContract]
        Status DeleteCDNAdapter(string sWSUserName, string sWSPassword, int adapterId);
        [OperationContract]
        Status DeleteExternalChannel(string sWSUserName, string sWSPassword, int externalChannelId);
        [OperationContract]
        Status DeleteKSQLChannel(string sWSUserName, string sWSPassword, int channelId);
        [OperationContract]
        Status DeleteOSSAdapter(string sWSUserName, string sWSPassword, int ossAdapterID);
        [OperationContract]
        Status DeleteOSSAdapterSettings(string sWSUserName, string sWSPassword, int ossAdapterId, List<OSSAdapterSettings> settings);
        [OperationContract]
        Status DeleteRecommendationEngine(string sWSUserName, string sWSPassword, int recommendationEngineId);
        [OperationContract]
        Status DeleteRecommendationEngineSettings(string sWSUserName, string sWSPassword, int recommendationEngineId, List<RecommendationEngineSettings> settings);
        [OperationContract]
        Status DisableDomainDefaultParentalRule(string userName, string webServicePassword, int domainId);
        [OperationContract]
        Status DisableUserDefaultParentalRule(string userName, string webServicePassword, string siteGuid, int domainId);
        [OperationContract]
        bool DoActionAssetRules(bool isSingleRun);
        [OperationContract]
        bool DoActionByRuleIds(string sWSUserName, string sWSPassword, List<long> ruleIds);
        [OperationContract]
        bool DoActionRules(bool isSingleRun);
        [OperationContract]
        bool DoesMediaBelongToChannels(string sWSUserName, string sWSPassword, int[] nChannels, int[] nFileTypeIDs, int nMediaID, bool bWithCache, string sDevice);
        [OperationContract]
        bool DoesMediaBelongToCollection(string sWSUserName, string sWSPassword, int nCollectionCode, int[] nFileTypeIDs, int nMediaID, string sDevice);
        [OperationContract]
        bool DoesMediaBelongToSubscription(string sWSUserName, string sWSPassword, int nSubscriptionCode, int[] nFileTypeIDs, int nMediaID, string sDevice);
        [OperationContract]
        Status EnqueueExportTask(string sWSUserName, string sWSPassword, long taskId);
        [OperationContract]
        bool Export(string sWSUserName, string sWSPassword, long taskId, string version);
        [OperationContract]
        CDNAdapterResponse GenerateCDNSharedSecret(string sWSUserName, string sWSPassword, int adapterId);
        [OperationContract]
        OSSAdapterResponse GenerateOSSSharedSecret(string sWSUserName, string sWSPassword, int ossAdapterId);
        [OperationContract]
        RecommendationEngineResponse GenerateRecommendationEngineSharedSecret(string sWSUserName, string sWSPassword, int recommendationEngineId);
        [OperationContract]
        bool GetAdminTokenValues(string sWSUserName, string sWSPassword, string sIP, string sToken, ref string sCountryCd2, ref string sLanguageFullName, ref string sDeviceName, ref UserStatus eUserStatus);
        [OperationContract]
        RegistryResponse GetAllRegistry(string sWSUserName, string sWSPassword);
        [OperationContract]
        List<string> GetAutoCompleteList(string sWSUserName, string sWSPassword, RequestObj request);
        [OperationContract]
        DeviceAvailabiltyRule GetAvailableDevices(string sWSUserName, string sWSPassword, int nMediaID);
        [OperationContract]
        FileTypeContainer[] GetAvailableFileTypes(string sWSUserName, string sWSPassword);
        [OperationContract]
        BulkExportTasksResponse GetBulkExportTasks(string sWSUserName, string sWSPassword, List<long> ids, List<string> externalKeys, BulkExportTaskOrderBy orderBy);
        [OperationContract]
        CDNAdapterResponse GetCDNAdapter(string sWSUserName, string sWSPassword, int adapterId);
        [OperationContract]
        CDNAdapterListResponse GetCDNAdapters(string sWSUserName, string sWSPassword);
        [OperationContract]
        CDNPartnerSettingsResponse GetCDNPartnerSettings(string sWSUserName, string sWSPassword);
        [OperationContract]
        UnifiedSearchResult[] GetChannelAssets(string username, string password, int channelId, int pageIndex, int pageSize);
        [OperationContract]
        int[] GetChannelMediaIDs(string sWSUserName, string sWSPassword, int nChannelID, int[] nFileTypeIDs, bool bWithCache, string sDevice);
        [OperationContract]
        List<int> GetChannelsAssetsIDs(string sWSUserName, string sWSPassword, int[] nChannels, int[] nFileTypeIDs, bool bWithCache, string sDevice, bool activeAssets, bool useStartDate);
        [OperationContract]
        int[] GetChannelsMediaIDs(string sWSUserName, string sWSPassword, int[] nChannels, int[] nFileTypeIDs, bool bWithCache, string sDevice);
        [OperationContract]
        string GetCoGuidByMediaFileId(string sWSUserName, string sWSPassword, int nMediaFileID);
        [OperationContract]
        List<int> GetCollectionMediaIds(string sWSUserName, string sWSPassword, int nCollectionCode, int[] nFileTypeIDs, string sDevice);
        [OperationContract]
        Country GetCountryByIp(string sWSUserName, string sWSPassword, string ip);
        [OperationContract]
        ApiObjects.CountryResponse GetCountryList(string sWSUserName, string sWSPassword, List<int> countryIds);
        [OperationContract]
        DeviceBrandResponse GetDeviceBrandList(string sWSUserName, string sWSPassword);
        [OperationContract]
        DeviceFamilyResponse GetDeviceFamilyList(string sWSUserName, string sWSPassword);
        [OperationContract]
        List<GroupRule> GetDomainGroupRules(string sWSUserName, string sWSPassword, int nDomainID);
        [OperationContract]
        ParentalRulesResponse GetDomainParentalRules(string userName, string password, int domainId);
        [OperationContract]
        List<EPGChannelObject> GetEPGChannel(string sWSUserName, string sWSPassword, string sPicSize);
        [OperationContract]
        List<GroupRule> GetEPGProgramRules(string sWSUserName, string sWSPassword, int nMediaId, int nProgramId, int siteGuid, string sIP, string deviceUdid);
        [OperationContract]
        GenericRuleResponse GetEpgRules(string userName, string webServicePassword, string siteGuid, long epgId, long channelMediaId, long domainId, string ip, GenericRuleOrderBy orderBy);
        [OperationContract]
        StatusErrorCodesResponse GetErrorCodesDictionary(string userName, string webServicePassword);
        [OperationContract]
        ExternalChannelResponseList GetExternalChannels(string sWSUserName, string sWSPassword);
        [OperationContract]
        OSSAdapterEntitlementsResponse GetExternalEntitlements(string sWSUserName, string sWSPassword, string userId);
        [OperationContract]
        FriendlyAssetLifeCycleRuleResponse GetFriendlyAssetLifeCycleRule(string sWSUserName, string sWSPassword, long id);
        [OperationContract]
        CDNAdapterResponse GetGroupDefaultCDNAdapter(string sWSUserName, string sWSPassword, eAssetTypes assetType);
        [OperationContract]
        int GetGroupIdByUsernamePassword(string username, string password);
        [OperationContract]
        List<LanguageObj> GetGroupLanguages(string sWSUserName, string sWSPassword);
        [OperationContract]
        string[] GetGroupMediaNames(string sWSUserName, string sWSPassword, string sGroupName);
        [OperationContract]
        List<GroupRule> GetGroupMediaRules(string sWSUserName, string sWSPassword, int nMediaID, int siteGuid, string sIP, string deviceUdid);
        [OperationContract]
        MetaResponse GetGroupMetaList(string sWSUserName, string sWSPassword, eAssetTypes assetType, MetaType metaType, MetaFieldName fieldNameEqual, MetaFieldName fieldNameNotEqual, List<MetaFeatureType> metaFeatureTypeList = null);
        [OperationContract]
        GroupOperator[] GetGroupOperators(string sWSUserName, string sWSPassword, string sScope = "");
        [OperationContract]
        string[] GetGroupPlayers(string sWSUserName, string sWSPassword, string sGroupName, bool sIncludeChildGroups);
        [OperationContract]
        List<GroupRule> GetGroupRules(string sWSUserName, string sWSPassword);
        [OperationContract]
        KSQLChannelResponse GetKSQLChannel(string sWSUserName, string sWSPassword, int channelId);
        [OperationContract]
        KSQLChannelResponseList GetKSQLChannels(string sWSUserName, string sWSPassword);
        [OperationContract]
        string GetLayeredCacheGroupConfig(string sWSUserName, string sWSPassword);
        [OperationContract]
        List<int> GetMediaChannels(string sWSUserName, string sWSPassword, int nMediaId);
        [OperationContract]
        List<MediaConcurrencyRule> GetMediaConcurrencyRules(string sWSUserName, string sWSPassword, int nMediaID, string sIP, int bmID, eBusinessModule eType);
        [OperationContract]
        List<int> GetMediaFilesByMediaId(string sWSUserName, string sWSPassword, int mediaId);
        [OperationContract]
        string GetMediaFileTypeDescription(string sWSUserName, string sWSPassword, int nMediaFileID);
        [OperationContract]
        int GetMediaFileTypeID(string sWSUserName, string sWSPassword, int nMediaFileID);
        [OperationContract]
        MediaMarkObject GetMediaMark(string sWSUserName, string sWSPassword, int nMediaID, string sSiteGuid);
        [OperationContract]
        GenericRuleResponse GetMediaRules(string userName, string webServicePassword, string siteGuid, long mediaId, long domainId, string ip, string udid, GenericRuleOrderBy orderBy);
        [OperationContract]
        List<GroupRule> GetNpvrRules(string sWSUserName, string sWSPassword, RecordedEPGChannelProgrammeObject recordedProgram, int siteGuid, string sIP, string deviceUdid);
        [OperationContract]
        List<GroupOperator> GetOperator(string sWSUserName, string sWSPassword, List<int> operatorIds);
        [OperationContract]
        OSSAdapterResponseList GetOSSAdapter(string sWSUserName, string sWSPassword);
        [OperationContract]
        OSSAdapterResponse GetOSSAdapterProfile(string sWSUserName, string sWSPassword, int ossAdapterId);
        [OperationContract]
        OSSAdapterSettingsResponse GetOSSAdapterSettings(string sWSUserName, string sWSPassword);
        [OperationContract]
        ParentalRulesResponse GetParentalEPGRules(string userName, string password, string siteGuid, long epgId, long domainId);
        [OperationContract]
        ParentalRulesResponse GetParentalMediaRules(string userName, string password, string siteGuid, long mediaId, long domainId);
        [OperationContract]
        PinResponse GetParentalPIN(string userName, string password, int domainId, string siteGuid, int? ruleId);
        [OperationContract]
        ParentalRulesResponse GetParentalRules(string userName, string password);
        [OperationContract]
        PermissionsResponse GetPermissions(string sWSUserName, string sWSPassword, List<long> permissionIds);
        [OperationContract]
        EPGChannelProgrammeObject GetProgramDetails(string sWSUserName, string sWSPass, int nProgramId);
        [OperationContract]
        Scheduling GetProgramSchedule(string sWSUserName, string sWSPass, int nProgramId);
        [OperationContract]
        PurchaseSettingsResponse GetPurchasePIN(string userName, string password, int domainId, string siteGuid);
        [OperationContract]
        PurchaseSettingsResponse GetPurchaseSettings(string userName, string password, int domainId, string siteGuid);
        [OperationContract]
        RecommendationEnginesResponseList GetRecommendationEngines(string sWSUserName, string sWSPassword);
        [OperationContract]
        RecommendationEngineSettinsResponse GetRecommendationEngineSettings(string sWSUserName, string sWSPassword);
        [OperationContract]
        RegionsResponse GetRegions(string sWSUserName, string sWSPassword, List<string> externalRegionList, RegionOrderBy orderBy);
        [OperationContract]
        RolesResponse GetRoles(string sWSUserName, string sWSPassword, List<long> roleIds);
        [OperationContract]
        ScheduledTaskLastRunDetails GetScheduledTaskLastRun(ScheduledTaskType scheduledTaskType);
        [OperationContract]
        GroupInfo[] GetSubGroupsTree(string sWSUserName, string sWSPassword, string sGroupName);
        [OperationContract]
        List<int> GetSubscriptionMediaIds(string sWSUserName, string sWSPassword, int nSubscriptionCode, int[] nFileTypeIDs, string sDevice);
        [OperationContract]
        TimeShiftedTvPartnerSettingsResponse GetTimeShiftedTvPartnerSettings(string sWSUserName, string sWSPassword);
        [OperationContract]
        OSSAdapterBillingDetailsResponse GetUserBillingDetails(string sWSUserName, string sWSPassword, long householdId, int ossAdapterId, string userIP);
        [OperationContract]
        List<GroupRule> GetUserGroupRules(string sWSUserName, string sWSPassword, string sSiteGuid);
        [OperationContract]
        ParentalRulesResponse GetUserParentalRules(string userName, string password, string siteGuid, int domainId);
        [OperationContract]
        ParentalRulesTagsResponse GetUserParentalRuleTags(string userName, string password, string siteGuid, long domainId);
        [OperationContract]
        string[] GetUserStartedWatchingMedias(string sWSUserName, string sWSPassword, string sSiteGuid, int nNumOfItems);
        [OperationContract]
        bool IncrementLayeredCacheGroupConfigVersion(string sWSUserName, string sWSPassword);
        [OperationContract]
        bool InitializeFreeItemsUpdate(string userName, string password);
        [OperationContract]
        CDNAdapterResponse InsertCDNAdapter(string sWSUserName, string sWSPassword, CDNAdapter adapter);
        [OperationContract]
        int InsertEPGSchedule(string sWSUserName, string sWSPassword, int channelID, string fileName, bool isDelete);
        [OperationContract]
        ExternalChannelResponse InsertExternalChannel(string sWSUserName, string sWSPassword, ExternalChannel externalChannel);
        [OperationContract]
        KSQLChannelResponse InsertKSQLChannel(string sWSUserName, string sWSPassword, KSQLChannel channel);
        [OperationContract]
        bool InsertOrUpdateAssetLifeCycleRulePpvsAndFileTypes(string sWSUserName, string sWSPassword, FriendlyAssetLifeCycleRule rule);
        [OperationContract]
        FriendlyAssetLifeCycleRuleResponse InsertOrUpdateFriendlyAssetLifeCycleRule(string sWSUserName, string sWSPassword, FriendlyAssetLifeCycleRule rule);
        [OperationContract]
        OSSAdapterResponse InsertOSSAdapter(string sWSUserName, string sWSPassword, OSSAdapter ossAdapter);
        [OperationContract]
        Status InsertOSSAdapterSettings(string sWSUserName, string sWSPassword, int ossAdapterId, List<OSSAdapterSettings> settings);
        [OperationContract]
        RecommendationEngineResponse InsertRecommendationEngine(string sWSUserName, string sWSPassword, RecommendationEngine recommendationEngine);
        [OperationContract]
        Status InsertRecommendationEngineSettings(string sWSUserName, string sWSPassword, int recommendationEngineId, List<RecommendationEngineSettings> settings);
        [OperationContract]
        ExternalChannelResponseList ListExternalChannels(string sWSUserName, string sWSPassword);
        [OperationContract]
        RecommendationEnginesResponseList ListRecommendationEngines(string sWSUserName, string sWSPassword);
        [OperationContract]
        MeidaMaper[] MapMediaFiles(string sWSUserName, string sWSPassword, int[] nMediaFileIDs);
        [OperationContract]
        MeidaMaper[] MapMediaFilesST(string sWSUserName, string sWSPassword, string sSeperatedMediaFileIDs);
        [OperationContract]
        Status MessageRecovery(string sWSUserName, string sWSPassword, long baseDateSec, List<string> messageDataTypes);
        [OperationContract]
        bool MigrateStatistics(string userName, string password, DateTime? startDate);
        [OperationContract]
        bool ModifyCB(string sWSUserName, string sWSPassword, string bucket, string key, eDbActionType action, string data, long ttlMinutes);
        [OperationContract]
        RateMediaObject RateMedia(string sWSUserName, string sWSPassword, int nMediaID, string sSiteGuid, int nRateVal);
        [OperationContract]
        bool RunImporter(string sWSUserName, string sWSPassword, string extraParams);
        [OperationContract]
        UnifiedSearchResult[] SearchAssets(string username, string password, string filter, int pageIndex, int pageSize, bool OnlyIsActive, int languageID, bool UseStartDate, string Udid, string UserIP, string SiteGuid, int DomainId, int ExectGroupId, bool IgnoreDeviceRule);
        [OperationContract]
        CDNAdapterResponse SendCDNAdapterConfiguration(string sWSUserName, string sWSPassword, int adapterID);
        [OperationContract]
        DrmAdapterResponse SendDrmAdapterConfiguration(string sWSUserName, string sWSPassword, int adapterID);
        [OperationContract]
        bool SendMailTemplate(string sWSUserName, string sWSPassword, MailRequestObj request);
        [OperationContract]
        bool SendToFriend(string sWSUserName, string sWSPassword, string sSenderName, string sSenderMail, string sMailTo, string sNameTo, int nMediaID);
        [OperationContract]
        CDNAdapterResponse SetCDNAdapter(string sWSUserName, string sWSPassword, CDNAdapter adapter, int adapterID);
        [OperationContract]
        bool SetDefaultRules(string sWSUserName, string sWSPassword, string sSiteGuid);
        [OperationContract]
        bool SetDomainGroupRule(string sWSUserName, string sWSPassword, int nDomainID, int nRuleID, string sPIN, int nIsActive);
        [OperationContract]
        Status SetDomainParentalRules(string userName, string webServicePassword, int domainId, long ruleId, int isActive);
        [OperationContract]
        ExternalChannelResponse SetExternalChannel(string sWSUserName, string sWSPassword, ExternalChannel externalChannel);
        [OperationContract]
        KSQLChannelResponse SetKSQLChannel(string sWSUserName, string sWSPassword, KSQLChannel channel);
        [OperationContract]
        bool SetLayeredCacheInvalidationKey(string sWSUserName, string sWSPassword, string key);
        [OperationContract]
        OSSAdapterResponse SetOSSAdapter(string sWSUserName, string sWSPassword, OSSAdapter ossAdapter);
        [OperationContract]
        Status SetOSSAdapterConfiguration(string sWSUserName, string sWSPassword, int ossAdapterId);
        [OperationContract]
        Status SetOSSAdapterSettings(string sWSUserName, string sWSPassword, int ossAdapterId, List<OSSAdapterSettings> settings);
        [OperationContract]
        Status SetParentalPIN(string userName, string password, int domainId, string siteGuid, string pin, int? ruleId);
        [OperationContract]
        Status SetPurchasePIN(string userName, string password, int domainId, string siteGuid, string pin);
        [OperationContract]
        Status SetPurchaseSettings(string userName, string password, int domainId, string siteGuid, int setting);
        [OperationContract]
        RecommendationEngineResponse SetRecommendationEngine(string sWSUserName, string sWSPassword, RecommendationEngine recommendationEngine);
        [OperationContract]
        Status SetRecommendationEngineSettings(string sWSUserName, string sWSPassword, int recommendationEngineId, List<RecommendationEngineSettings> settings);
        [OperationContract]
        bool SetRuleState(string sWSUserName, string sWSPassword, int nDomainID, string sSiteGUID, int nRuleID, int nStatus);
        [OperationContract]
        bool SetUserGroupRule(string sWSUserName, string sWSPassword, string sSiteGuid, int nRuleID, string sPIN, int nIsActive);
        [OperationContract]
        Status SetUserParentalRules(string userName, string webServicePassword, string siteGuid, long ruleId, int isActive, int domainId);
        [OperationContract]
        CategoryObject[] TVAPI_CategoriesTree(string sWSUserName, string sWSPassword, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, int nCategoryID, bool bWithChannels);
        [OperationContract]
        ChannelObject[] TVAPI_CategoryChannels(string sWSUserName, string sWSPassword, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, int nCategoryID);
        [OperationContract]
        ChannelObject[] TVAPI_ChannelsMedia(string sWSUserName, string sWSPassword, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, ChannelRequestObject[] theChannelsRequestObj);
        [OperationContract]
        ChannelObject TVAPI_GetMediaInfo(string sWSUserName, string sWSPassword, InitializationObject oInitObj, int[] nMediaIDs, MediaInfoStructObject theInfoStruct);
        [OperationContract]
        ChannelObject TVAPI_GetMedias(string sWSUserName, string sWSPassword, InitializationObject oInitObj, int[] nMediaIDs, MediaInfoStructObject theInfoStruct);
        [OperationContract]
        MediaInfoStructObject TVAPI_GetMediaStructure(string sWSUserName, string sWSPassword, InitializationObject oInitObj);
        [OperationContract]
        UserIMRequestObject TVAPI_GetTvinciGUID(string sWSUserName, string sWSPassword, InitializationObject oInitObj);
        [OperationContract]
        ChannelObject TVAPI_NowPlaying(string sWSUserName, string sWSPassword, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, PageDefinition thePageDef);
        [OperationContract]
        ChannelObject TVAPI_PeopleWhoWatchedAlsoWatched(string sWSUserName, string sWSPassword, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, int nMediaID, int nMediaFileID);
        [OperationContract]
        ChannelObject TVAPI_SearchMedia(string sWSUserName, string sWSPassword, InitializationObject oInitObj, SearchDefinitionObject oSearchDefinitionObj, MediaInfoStructObject theInfoStruct);
        [OperationContract]
        ChannelObject TVAPI_SearchRelated(string sWSUserName, string sWSPassword, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, PageDefinition thePageDef, int nMediaID);
        [OperationContract]
        GenericWriteResponse TVAPI_SendMediaByEmail(string sWSUserName, string sWSPassword, InitializationObject oInitObj, int nMediaID, string sFromEmail, string sToEmail, string sRecieverName, string sSenderName, string sContent);
        [OperationContract]
        TagResponseObject[] TVAPI_TagValues(string sWSUserName, string sWSPassword, InitializationObject oInitObj, TagRequestObject[] oTagsDefinition);
        [OperationContract]
        ChannelObject[] TVAPI_UserDeleteChannel(string sWSUserName, string sWSPassword, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, int nChannelID);
        [OperationContract]
        ChannelObject TVAPI_UserLastWatched(string sWSUserName, string sWSPassword, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct, PageDefinition thePageDef);
        [OperationContract]
        ChannelObject[] TVAPI_UserSavedChannels(string sWSUserName, string sWSPassword, InitializationObject oInitObj, MediaInfoStructObject theInfoStruct);
        [OperationContract]
        GenericWriteResponse TVAPI_UserSavePlaylist(string sWSUserName, string sWSPassword, InitializationObject oInitObj, int[] nMediaIDs, string sPlaylistTitle, bool bRewrite);
        [OperationContract]
        BulkExportTaskResponse UpdateBulkExportTask(string sWSUserName, string sWSPassword, long id, string externalKey, string name, eBulkExportDataType dataType, string filter, eBulkExportExportType exportType, long frequency, string notificationUrl, List<int> vodTypes, bool? isActive);
        [OperationContract]
        bool UpdateCache(int groupId, string bucket, string[] keys);
        [OperationContract]
        CDNPartnerSettingsResponse UpdateCDNPartnerSettings(string sWSUserName, string sWSPassword, CDNPartnerSettings settings);
        [OperationContract]
        bool UpdateFreeFileTypeOfModule(int groupID, int moduleID);
        [OperationContract]
        bool UpdateGeoBlockRulesCache(int groupId);
        [OperationContract]
        bool UpdateImageState(string sWSUserName, string sWSPassword, long rowId, int version, eMediaType mediaType, eTableStatus status);
        [OperationContract]
        bool UpdateLayeredCacheGroupConfig(string sWSUserName, string sWSPassword, int? version, bool? disableLayeredCache, List<string> layeredCacheSettingsToExclude, bool? shouldOverrideExistingExludeSettings, List<string> layeredCacheInvalidationKeySettingsToExclude, bool? shouldOverrideExistingInvalidationKeyExcludeSettings);
        [OperationContract]
        bool UpdateLayeredCacheGroupConfigST(string sWSUserName, string sWSPassword, int version, bool disableLayeredCache, string layeredCacheSettingsToExcludeCommaSeperated, bool shouldOverrideExistingExludeSettings, string layeredCacheInvalidationKeySettingsToExcludeCommaSeperated, bool shouldOverrideExistingInvalidationKeyExcludeSettings);
        [OperationContract]
        PinResponse UpdateParentalPIN(string userName, string password, int domainId, string siteGuid, string pin, int? ruleId);
        [OperationContract]
        PurchaseSettingsResponse UpdatePurchasePIN(string userName, string password, int domainId, string siteGuid, string pin);
        [OperationContract]
        PurchaseSettingsResponse UpdatePurchaseSettings(string userName, string password, int domainId, string siteGuid, int setting);
        [OperationContract]
        RecommendationEngineResponse UpdateRecommendationEngineConfiguration(string userName, string password, int recommendationEngineId);
        [OperationContract]
        bool UpdateScheduledTaskNextRunIntervalInSeconds(ScheduledTaskType scheduledTaskType, double nextRunIntervalInSeconds);
        [OperationContract]
        Status UpdateTimeShiftedTvEpgChannelsSettings(string sWSUserName, string sWSPassword, TimeShiftedTvPartnerSettings settings);
        [OperationContract]
        Status UpdateTimeShiftedTvPartnerSettings(string sWSUserName, string sWSPassword, TimeShiftedTvPartnerSettings settings);
        [OperationContract]
        bool ValidateBaseLink(string sWSUserName, string sWSPassword, int nMediaFileID, string sBaseLink);
        [OperationContract]
        Status ValidateParentalPIN(string userName, string password, string siteGuid, string pin, int domainId, int? ruleId);
        [OperationContract]
        Status ValidatePurchasePIN(string userName, string password, string siteGuid, string pin, int domainId);
    }
}