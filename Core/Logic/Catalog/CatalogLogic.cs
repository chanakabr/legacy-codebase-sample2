using AdapterControllers;
using ApiLogic.Api.Managers;
using ApiLogic.Catalog;
using ApiLogic.Catalog.CatalogManagement.Models;
using ApiLogic.Catalog.CatalogManagement.Services;
using ApiLogic.Catalog.NextEpisode;
using ApiLogic.Catalog.Services;
using ApiLogic.Catalog.Tree;
using ApiLogic.IndexManager.Helpers;
using ApiLogic.IndexManager.QueryBuilders.SearchPriority;
using ApiLogic.IndexManager.Sorting;
using ApiLogic.Segmentation;
using APILogic.Api.Managers;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Catalog;
using ApiObjects.Epg;
using ApiObjects.MediaMarks;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using ApiObjects.Segmentation;
using ApiObjects.Statistics;
using ApiObjects.TimeShiftedTv;
using CachingHelpers;
using CachingProvider.LayeredCache;
using Catalog.Response;
using Core.Catalog.Cache;
using Core.Catalog.CatalogManagement;
using Core.Catalog.Request;
using Core.Catalog.Response;
using Core.GroupManagers;
using Core.Notification;
using Core.Users;
using CouchbaseManager;
using DAL;
using DalCB;
using ElasticSearch.Searcher;
using EpgBL;
using GroupsCacheManager;
using KalturaRequestContext;
using Newtonsoft.Json;
using NPVR;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;
using QueueWrapper;
using StatisticsBL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Tvinci.Core.DAL;
using TVinciShared;
using OrderDir = ApiObjects.SearchObjects.OrderDir;
using Status = ApiObjects.Response.Status;

namespace Core.Catalog
{
    public class CatalogLogic : ICatalogLogic
    {
        private static readonly KLogger log = new KLogger(nameof(CatalogLogic));

        private static readonly KLogger statisticsLog =
            new KLogger(nameof(CatalogLogic), "MediaEohLogger");

        private static readonly KLogger newWatcherMediaActionLog =
            new KLogger(nameof(CatalogLogic), "NewWatcherMediaActionLogger");

        public static readonly string TAGS = "tags";
        public static readonly string METAS = "metas";
        public static readonly string NAME = "name";
        public static readonly string DESCRIPION = "description";
        public static readonly string EXTERNAL_ID = "external_id";
        public static readonly string ENTRY_ID = "entry_id";
        public static readonly string STATUS = "status";
        public static readonly string IS_ACTIVE = "is_active";
        public static readonly string EXTERNALID = "externalid";
        public static readonly string ENTRYID = "entryid";
        public static readonly string CREATIONDATE = "createdate";
        public static readonly string CREATE_DATE = "create_date";
        public static readonly string PLAYBACKSTARTDATETIME = "playbackstartdatetime";
        public static readonly string START_DATE = "start_date";
        public static readonly string PLAYBACKENDDATETIME = "playbackenddatetime";
        public static readonly string FINAL_DATE = "final_date";
        public static readonly string CATALOGSTARTDATETIME = "catalogstartdatetime";
        public static readonly string CATALOG_START_DATE = "catalog_start_date";
        public static readonly string CATALOGENDDATETIME = "catalogenddatetime";
        public static readonly string END_DATE = "end_date";
        public static readonly string LASTMODIFIED = "lastmodified";
        public static readonly string UPDATE_DATE = "update_date";
        public static readonly string INHERITANCE_POLICY = "inheritance_policy";
        public static readonly string LINEAR_MEDIA_ID = "linear_media_id";
        public static readonly string MEDIA_ID = "media_id";
        public static readonly string EPG_ID = "epg_id";
        public static readonly string RECORDING_ID = "recording_id";
        public static readonly string EPG_CHANNEL_ID = "epg_channel_id";
        public static readonly string ASSET_TYPE = "asset_type";
        public static readonly string MEDIA_ASSET_TYPE = "media";
        public static readonly string EPG_ASSET_TYPE = "epg";
        public static readonly string EXTERNAL_OFFER_ID = "external_offer_id";
        public static readonly string EXTERNAL_OFFER_IDS = "external_offer_ids";

        public static readonly string L2V_LINEAR_ASSET_ID =
            $"{NamingHelper.LIVE_TO_VOD_PREFIX}.{NamingHelper.LINEAR_ASSET_ID}";
        public static readonly string L2V_EPG_CHANNEL_ID =
            $"{NamingHelper.LIVE_TO_VOD_PREFIX}.{NamingHelper.EPG_CHANNEL_ID}";
        public static readonly string L2V_CRID = $"{NamingHelper.LIVE_TO_VOD_PREFIX}.{NamingHelper.CRID}";
        public static readonly string L2V_EPG_ID = $"{NamingHelper.LIVE_TO_VOD_PREFIX}.{NamingHelper.EPG_ID}";
        public static readonly string L2V_ORIGINAL_START_DATE =
            $"{NamingHelper.LIVE_TO_VOD_PREFIX}.{NamingHelper.ORIGINAL_START_DATE}";
        public static readonly string L2V_ORIGINAL_END_DATE =
            $"{NamingHelper.LIVE_TO_VOD_PREFIX}.{NamingHelper.ORIGINAL_END_DATE}";

        private static readonly string LINEAR_MEDIA_TYPES_KEY = "LinearMediaTypes";
        private static readonly string PERMITTED_WATCH_RULES_KEY = "PermittedWatchRules";

        private const int DEFAULT_SEARCHER_MAX_RESULTS_SIZE = 10000;
        private const int MD5_HASH_SIZE_BYTES = 32;

        internal const int DEFAULT_PWWAWP_MAX_RESULTS_SIZE = 8;
        internal const int DEFAULT_PWLALP_MAX_RESULTS_SIZE = 8;
        internal const int DEFAULT_PERSONAL_RECOMMENDED_MAX_RESULTS_SIZE = 20;
        internal const int FINISHED_PERCENT_THRESHOLD = 95;
        private static int DEFAULT_CURRENT_REQUEST_DAYS_OFFSET = 7;

        private static readonly long UNIX_TIME_1980 =
            DateUtils.DateTimeToUtcUnixTimestampSeconds(new DateTime(1980, 1, 1, 0, 0, 0));

        private static readonly string META_DOUBLE_SUFFIX = "_DOUBLE";
        private static readonly string META_BOOL_SUFFIX = "_BOOL";
        private static readonly string META_DATE_PREFIX = "date";

        private static readonly string CB_MEDIA_MARK_DESGIN =
            ApplicationConfiguration.Current.CouchBaseDesigns.MediaMarkDesign.Value;

        private const string NO_META_TO_UPDATE = "No meta update";
        private const string NAME_REQUIRED = "Name must have a value";
        private const string META_NOT_EXIST = "Meta not exist";
        private static string PARENT_ID_SHOULD_NOT_POINT_TO_ITSELF = "Parent id should not point to itself";
        private static string META_DOES_NOT_A_USER_INTEREST = "Meta not a user interest";
        private static string PARENT_ID_NOT_A_USER_INTERSET = "Parent meta id should be recognized as user interest";

        private static string PARENT_ASSET_TYPE_DIFFRENT_FROM_META =
            "Parent meta asset type should be as meta asset type";

        private static string PARENT_DUPLICATE_ASSOCIATION = "Parent should be associated to only 1 meta";
        private static string WRONG_META_NAME = "Wrong meta name";
        private static string META_NOT_BELONG_TO_PARTNER = "Meta not belong to partner";

        private static string PARENT_PARNER_DIFFRENT_FROM_META_PARTNER =
            "Partner parent should be the some as meta partner";

        private static readonly HashSet<string> internalReservedUnifiedSearchNumericFields = new HashSet<string>()
        {
            "allowed_countries",
            "blocked_countries"
        };

        private static readonly HashSet<string> predefinedAssetTypes = new HashSet<string>()
        {
            "media",
            "epg",
            "recording"
        };

        private static readonly Lazy<ICatalogLogic> LazyInstance = new Lazy<ICatalogLogic>(
                () => new CatalogLogic(SortingAdapter.Instance),
                LazyThreadSafetyMode.PublicationOnly);

        private static int maxNGram = -1;

        private readonly ISortingAdapter _sortingAdapter;

        private CatalogLogic(ISortingAdapter sortingAdapter)
        {
            _sortingAdapter = sortingAdapter ?? throw new ArgumentNullException(nameof(sortingAdapter));
        }

        public static ICatalogLogic Instance => LazyInstance.Value;

        /* Get All Relevant Details About Media (by id), use LayeredCache */
        internal static bool CompleteDetailsForMediaResponse(MediasProtocolRequest mediaRequest,
            ref MediaResponse mediaResponse, int nStartIndex, int nEndIndex, bool managementData = false)
        {
            List<MediaObj> lMediaObj = new List<MediaObj>();
            mediaResponse.m_nTotalItems = 0;
            mediaResponse.m_lObj = new List<BaseObject>();
            int groupId = mediaRequest.m_nGroupID;
            Filter filter = mediaRequest.m_oFilter;
            string siteGuid = mediaRequest.m_sSiteGuid;
            List<int> mediaIds = mediaRequest.m_lMediasIds;

            try
            {
                if (mediaIds != null && mediaIds.Count > 0)
                {
                    int endIndex = 0;
                    if (nStartIndex == 0 && nEndIndex == 0 && mediaIds != null && mediaIds.Count > 0)
                    {
                        endIndex = mediaIds.Count;
                    }

                    List<BaseObject> assetsToRetrieve = new List<BaseObject>();
                    // get only assets in requested page
                    for (int i = nStartIndex; i < endIndex; i++)
                    {
                        assetsToRetrieve.Add(new BaseObject()
                            {AssetId = mediaIds[i].ToString(), AssetType = eAssetTypes.MEDIA});
                    }

                    mediaResponse.m_nTotalItems = mediaIds.Count;
                    mediaResponse.m_lObj =
                        Core.Catalog.Utils.GetOrderedAssets(groupId, assetsToRetrieve, filter, managementData);
                }

                return true;
            }

            catch (Exception ex)
            {
                log.Error("failed to complete details", ex);
                throw ex;
            }
        }

        /// <summary>
        /// Creates list of media objects to the sent media ids.
        /// </summary>
        /// <param name="mediaIds"></param>
        /// <param name="groupId"></param>
        /// <param name="filter"></param>
        /// <param name="siteGuid"></param>
        /// <returns></returns>
        internal static List<MediaObj> CompleteMediaDetails(List<int> mediaIds, int groupId, Filter filter,
            bool managementData = false)
        {
            int startIndex = 0;
            int endIndex = 0;
            int totalItems = 0;

            return CatalogLogic.CompleteMediaDetails(mediaIds, startIndex, ref endIndex, ref totalItems, groupId,
                filter, managementData);
        }

        /// <summary>
        /// Creates list of media objects to the sent media ids. Allows use of start and end index, counts total items
        /// </summary>
        /// <param name="mediaIds"></param>
        /// <param name="nStartIndex"></param>
        /// <param name="nEndIndex"></param>
        /// <param name="totalItems"></param>
        /// <param name="groupId"></param>
        /// <param name="filter"></param>
        /// <param name="siteGuid"></param>
        /// <returns></returns>
        internal static List<MediaObj> CompleteMediaDetails(List<int> mediaIds, int nStartIndex, ref int nEndIndex,
            ref int totalItems, int groupId, Filter filter, bool managementData = false)
        {
            List<MediaObj> mediaObjects = new List<MediaObj>();

            bool bIsMainLang = Utils.IsLangMain(groupId, filter.m_nLanguage);

            if (nStartIndex == 0 && nEndIndex == 0 && mediaIds != null && mediaIds.Count > 0)
            {
                nEndIndex = mediaIds.Count;
            }

            //Start MultiThread Call
            Task[] tasks = new Task[nEndIndex - nStartIndex];
            ConcurrentDictionary<int, MediaObj> dMediaObj = new ConcurrentDictionary<int, MediaObj>();

            //Build the Dictionary to keep the specific order of the mediaIds
            for (int i = nStartIndex; i < nEndIndex; i++)
            {
                int nMedia = mediaIds[i];
                dMediaObj.TryAdd(nMedia, null);
            }

            GroupManager groupManager = new GroupManager();
            CatalogCache catalogCache = CatalogCache.Instance();
            int nParentGroupID = catalogCache.GetParentGroup(groupId);
            List<int> lSubGroup = groupManager.GetSubGroup(nParentGroupID);

            // save monitor and logs context data
            LogContextData contextData = new LogContextData();

            List<int> nonExistingMediaIDs = new List<int>();

            //complete media id details
            for (int i = nStartIndex; i < nEndIndex; i++)
            {
                int nMedia = mediaIds[i];

                tasks[i - nStartIndex] = Task.Run(() =>
                {
                    // load monitor and logs context data
                    contextData.Load();

                    try
                    {
                        var currentMedia = GetMediaDetails(nMedia, groupId, filter, bIsMainLang, lSubGroup,
                            managementData);
                        dMediaObj[nMedia] = currentMedia;

                        // If couldn't get media details for this media - probably it doesn't exist, and it shouldn't appear in ES index
                        if (currentMedia == null)
                        {
                            nonExistingMediaIDs.Add(nMedia);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Failed in GetMediaDetails. Group ID = {0}, media id = {1}.", groupId, nMedia,
                            ex);
                    }
                });
            }

            Task.WaitAll(tasks);

            if (tasks != null && tasks.Length > 0)
            {
                for (int i = 0; i < tasks.Length; i++)
                {
                    if (tasks[i] != null)
                        tasks[i].Dispose();
                }
            }

            if (mediaIds != null)
            {
                totalItems = mediaIds.Count;
                foreach (int nMedia in mediaIds)
                {
                    if (dMediaObj[nMedia] != null)
                    {
                        mediaObjects.Add(dMediaObj[nMedia]);
                    }
                }
            }

            // If there are any media that we couldn't find via stored procedure - delete them off from ES index
            if (nonExistingMediaIDs != null && nonExistingMediaIDs.Count > 0)
            {
                // filter - only Ids larger than 0
                nonExistingMediaIDs = nonExistingMediaIDs.Where(id => id > 0).ToList();

                List<int> idsToUpdate = new List<int>();
                List<int> idsToDelete = new List<int>();
                List<int> idsToTurnOff = new List<int>();

                bool doesGroupUsesTemplates = CatalogManager.Instance.DoesGroupUsesTemplates(groupId);

                foreach (var id in nonExistingMediaIDs)
                {
                    // Look for the origin row of the media in the database
                    DataRow currentMediaRow =
                        ODBCWrapper.Utils.GetTableSingleRow("media", id, "MAIN_CONNECTION_STRING");

                    // If no row returned - delete the record in index
                    if (currentMediaRow == null || ODBCWrapper.Utils.ExtractInteger(currentMediaRow, "status") != 1 ||
                        (!doesGroupUsesTemplates &&
                         ODBCWrapper.Utils.ExtractDateTime(currentMediaRow, "FINAL_END_DATE") <
                         DateTime.UtcNow)) //BEO-10220
                    {
                        idsToDelete.Add(id);
                    }
                    else
                    {
                        int isActive = ODBCWrapper.Utils.ExtractInteger(currentMediaRow, "is_active");

                        // if media is not active, turn it off
                        if (isActive != 1)
                        {
                            idsToTurnOff.Add(id);
                        }
                        // if media is active and has valid status, update it in index
                        else
                        {
                            idsToUpdate.Add(id);
                        }
                    }
                }

                // Add messages to queue for each type of action
                CatalogLogic.Update(idsToDelete, groupId, eObjectType.Media, eAction.Delete);
                CatalogLogic.Update(idsToUpdate, groupId, eObjectType.Media, eAction.Update);
                CatalogLogic.Update(idsToTurnOff, groupId, eObjectType.Media, eAction.Off);
            }

            return mediaObjects;
        }

        private static MediaObj GetMediaDetails(int nMedia, MediasProtocolRequest mediaRequest, bool bIsMainLang,
            List<int> lSubGroup)
        {
            return GetMediaDetails(nMedia, mediaRequest.m_nGroupID, mediaRequest.m_oFilter, bIsMainLang, lSubGroup);
        }

        private static MediaObj GetMediaDetails(int nMedia, int groupId, Filter filter, bool bIsMainLang,
            List<int> lSubGroup, bool managementData = false)
        {
            bool result = true;

            try
            {
                if (CatalogManagement.CatalogManager.Instance.DoesGroupUsesTemplates(groupId))
                {
                    return CatalogManagement.AssetManager.GetMediaObj(groupId, nMedia);
                }

                GroupManager groupManager = new GroupManager();
                Group group = groupManager.GetGroup(groupId);
                MediaObj oMediaObj = new MediaObj();

                string sEndDate = string.Empty;
                bool bOnlyActiveMedia = true;
                bool bUseStartDate = true;
                List<int> languagesIds = new List<int>();
                List<LanguageObj> languages = new List<LanguageObj>();
                LanguageObj language = null;

                if (group.isGeoAvailabilityWindowingEnabled || managementData)
                    sEndDate = "GEO";
                else
                    sEndDate = ProtocolsFuncs.GetFinalEndDateField(true, managementData);

                if (filter != null)
                {
                    bOnlyActiveMedia = filter.m_bOnlyActiveMedia;
                    bUseStartDate = filter.m_bUseStartDate;

                    if (filter.m_nLanguage == 0)
                    {
                        languages = group.GetLangauges();
                    }
                    else
                    {
                        // Getting the language  from the filter request
                        language = group.GetLanguage(filter.m_nLanguage);

                        // in case no language was found - throw Exception
                        if (language == null)
                            throw new Exception(string.Format("Error while getting language: {0} for group: {1}",
                                filter.m_nLanguage, groupId));
                        else
                            languages.Add(language);
                    }
                }
                else
                {
                    // in case no language was found - return default language
                    languages.Add(group.GetGroupDefaultLanguage());
                }

                foreach (var languageId in languages.Select(x => x.ID))
                {
                    if (!languagesIds.Contains(languageId))
                        languagesIds.Add(languageId);
                }


                DataSet ds = CatalogDAL.Get_MediaDetailsWithLanguages(groupId, nMedia, bOnlyActiveMedia, languagesIds,
                    sEndDate, bUseStartDate);

                if (ds == null)
                    return null;
                if (ds.Tables.Count >= 8)
                {
                    int assetGroupId = 0;
                    int isLinear = 0;

                    bool isMedia = GetMediaBasicDetails(ref oMediaObj, ds.Tables[0], ds.Tables[3], group.GetLangauges(),
                        ds.Tables[5], bIsMainLang, ref assetGroupId, ref isLinear, managementData);

                    // only if we found basic details for media - media in status = 1 , and active if necessary. If not - return null.
                    if (!isMedia)
                    {
                        return null;
                    }
                    else
                    {
                        oMediaObj.m_lPicture = GetAllPic(groupId, nMedia, ds.Tables[1], ref result, assetGroupId);
                        if (!result)
                        {
                            return null;
                        }

                        oMediaObj.m_lBranding = new List<Branding>();


                        Dictionary<int, List<string>> mediaFilePPVModules = null;
                        if (managementData)
                        {
                            mediaFilePPVModules = GetMediaFilePPVModules(ds.Tables[2]);
                        }

                        oMediaObj.m_lFiles = FilesValues(ds.Tables[2], ref oMediaObj.m_lBranding, filter.m_noFileUrl,
                            ref result, managementData, mediaFilePPVModules);
                        if (!result)
                        {
                            return null;
                        }

                        oMediaObj.m_lMetas = GetMetaDetails(ds.Tables[0], ds.Tables[3], group, ref result);
                        if (!result)
                        {
                            return null;
                        }

                        oMediaObj.m_lTags = GetTagsDetails(ds.Tables[6], ds.Tables[4], bIsMainLang, group, ref result,
                            nMedia);
                        if (!result)
                        {
                            return null;
                        }

                        oMediaObj.m_lMetas.AddRange(GetDateMetaDetails(ds.Tables[7], ref result));
                        if (!result)
                        {
                            return null;
                        }

                        if (isLinear != 0 && !string.IsNullOrEmpty(oMediaObj.m_ExternalIDs))
                        {
                            // get linear Channel settings
                            Dictionary<string, LinearChannelSettings> linearChannelSettings = CatalogCache.Instance()
                                .GetLinearChannelSettings(groupId, new List<string>() {oMediaObj.m_ExternalIDs});
                            if (linearChannelSettings != null &&
                                linearChannelSettings.ContainsKey(oMediaObj.m_ExternalIDs) &&
                                linearChannelSettings.Values != null)
                            {
                                oMediaObj.EnableCatchUp = linearChannelSettings[oMediaObj.m_ExternalIDs].EnableCatchUp;
                                oMediaObj.EnableCDVR = linearChannelSettings[oMediaObj.m_ExternalIDs].EnableCDVR;
                                oMediaObj.EnableStartOver =
                                    linearChannelSettings[oMediaObj.m_ExternalIDs].EnableStartOver;
                                oMediaObj.EnableTrickPlay =
                                    linearChannelSettings[oMediaObj.m_ExternalIDs].EnableTrickPlay;
                                oMediaObj.CatchUpBuffer = linearChannelSettings[oMediaObj.m_ExternalIDs].CatchUpBuffer;
                                oMediaObj.PaddingBeforeProgramStarts = linearChannelSettings[oMediaObj.m_ExternalIDs]
                                    .PaddingBeforeProgramStarts;
                                oMediaObj.PaddingAfterProgramEnds = linearChannelSettings[oMediaObj.m_ExternalIDs]
                                    .PaddingAfterProgramEnds;
                                oMediaObj.TrickPlayBuffer =
                                    linearChannelSettings[oMediaObj.m_ExternalIDs].TrickPlayBuffer;
                                oMediaObj.EnableRecordingPlaybackNonEntitledChannel =
                                    linearChannelSettings[oMediaObj.m_ExternalIDs]
                                        .EnableRecordingPlaybackNonEntitledChannel;
                                oMediaObj.ExternalCdvrId =
                                    CatalogDAL.GetEPGChannelCDVRId(0, long.Parse(oMediaObj.m_ExternalIDs));
                            }
                        }
                    }
                }

                return oMediaObj;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                result = false;
                return null;
            }
        }

        internal static Dictionary<int, List<string>> GetMediaFilePPVModules(DataTable dtFileMedia)
        {
            List<int> mediaFileIds = null;
            int mediaFileId = 0;
            Dictionary<int, List<string>> dicMediaFilePPVModules = null;

            if (dtFileMedia != null && dtFileMedia.Rows.Count > 0)
            {
                dicMediaFilePPVModules = new Dictionary<int, List<string>>();
                mediaFileIds = new List<int>();
                for (int index = 0; index < dtFileMedia.Rows.Count; index++)
                {
                    mediaFileId = Utils.GetIntSafeVal(dtFileMedia.Rows[index], "id");
                    if (mediaFileId > 0)
                        mediaFileIds.Add(mediaFileId);
                }

                if (mediaFileIds.Count > 0)
                {
                    var mediaFilePPVModulesTable = CatalogDAL.GetMediaFilePPVModules(mediaFileIds);
                    var name = string.Empty;
                    DateTime? startDate = null;
                    DateTime? endDate = null;
                    string ppbMoudleName = string.Empty;

                    if (mediaFilePPVModulesTable != null && mediaFilePPVModulesTable.Rows != null &&
                        mediaFilePPVModulesTable.Rows.Count > 0)
                    {
                        for (int index = 0; index < mediaFilePPVModulesTable.Rows.Count; index++)
                        {
                            mediaFileId = Utils.GetIntSafeVal(mediaFilePPVModulesTable.Rows[index], "MEDIA_FILE_ID");
                            name = Utils.GetStrSafeVal(mediaFilePPVModulesTable.Rows[index], "NAME");
                            startDate = ODBCWrapper.Utils.GetNullableDateSafeVal(mediaFilePPVModulesTable.Rows[index],
                                "START_DATE");
                            endDate = ODBCWrapper.Utils.GetNullableDateSafeVal(mediaFilePPVModulesTable.Rows[index],
                                "END_DATE");

                            ppbMoudleName = BuildPPVMoudleName(name, startDate, endDate);

                            if (!dicMediaFilePPVModules.ContainsKey(mediaFileId))
                            {
                                dicMediaFilePPVModules.Add(mediaFileId, new List<string>());
                            }

                            dicMediaFilePPVModules[mediaFileId].Add(ppbMoudleName);
                        }
                    }
                }
            }

            return dicMediaFilePPVModules;
        }

        private static string BuildPPVMoudleName(string name, DateTime? startDate, DateTime? endDate)
        {
            StringBuilder ppvMoudleName = new StringBuilder();
            ppvMoudleName.Append(name);
            if (startDate.HasValue)
            {
                ppvMoudleName.AppendFormat(";{0}", startDate.Value.ToString(DateUtils.MAIN_FORMAT));
            }

            if (endDate.HasValue)
            {
                ppvMoudleName.AppendFormat(";{0}", endDate.Value.ToString(DateUtils.MAIN_FORMAT));
            }

            return ppvMoudleName.Length > 0 ? ppvMoudleName.ToString() : string.Empty;
        }

        /*Insert all tags that return from the "CompleteDetailsForMediaResponse" into List<Tags>*/
        private static List<Tags> GetTagsDetails(DataTable tagLangs, DataTable dtTags, bool bIsMainLang, Group group,
            ref bool result, int mediaId)
        {
            try
            {
                result = true;
                List<Tags> tagList = new List<Tags>();
                Tags tags = new Tags();
                string tagTypeName = string.Empty;
                string tagValue = string.Empty;
                int tagId = 0;

                Dictionary<string, List<KeyValuePair<int, string>>>
                    tagTypeNameAndValues = null; // hold foreach tagType: list of values
                Dictionary<string, List<KeyValuePair<int, List<LanguageContainer>>>>
                    dicTagIdLanguageContainers = null; // hold foreach tagType/tagId: list of LanguageContainer
                Dictionary<int, List<LanguageContainer>> tagIdLanguageContainers = null;
                List<KeyValuePair<int, string>> tagValues = null;
                List<LanguageContainer> tagLangContainerList = null;

                if (dtTags != null)
                {
                    tagTypeNameAndValues = new Dictionary<string, List<KeyValuePair<int, string>>>();
                    dicTagIdLanguageContainers =
                        new Dictionary<string, List<KeyValuePair<int, List<LanguageContainer>>>>();
                    MediaTagsTranslations mediaTagsTranslations = null;

                    if (group.isTagsSingleTranslation)
                    {
                        //GET asset tags translations
                        mediaTagsTranslations = CatalogDAL.GetMediaTagsTranslations(mediaId);
                    }

                    for (int rowIndex = 0; rowIndex < dtTags.Rows.Count; rowIndex++)
                    {
                        tagIdLanguageContainers = new Dictionary<int, List<LanguageContainer>>();
                        tagLangContainerList = new List<LanguageContainer>();

                        tagTypeName = Utils.GetStrSafeVal(dtTags.Rows[rowIndex], "tag_type_name");
                        tagValue = Utils.GetStrSafeVal(dtTags.Rows[rowIndex], "value");
                        tagId = Utils.GetIntSafeVal(dtTags.Rows[rowIndex], "tag_id");

                        if (!string.IsNullOrEmpty(tagTypeName))
                        {
                            // bundle tags by tagTypeName
                            if (!tagTypeNameAndValues.ContainsKey(tagTypeName))
                            {
                                tagTypeNameAndValues.Add(tagTypeName, new List<KeyValuePair<int, string>>());
                            }

                            tagValues = tagTypeNameAndValues[tagTypeName];
                            tagValues.Add(new KeyValuePair<int, string>(tagId, tagValue));

                            //add is default lang values
                            LanguageObj language = group.GetLangauges().Where(x => x.IsDefault).FirstOrDefault();
                            if (language != null)
                                tagLangContainerList.Add(new LanguageContainer()
                                    {m_sLanguageCode3 = language.Code, m_sValue = tagValue});

                            if (group.isTagsSingleTranslation && mediaTagsTranslations != null &&
                                mediaTagsTranslations.Translations?.Count > 0)
                            {
                                var tagsTranslations = mediaTagsTranslations.Translations.Where(x => x.TagId == tagId)
                                    .ToList();
                                if (tagsTranslations?.Count > 0)
                                {
                                    //get translate values for tag_id + add to tagLangContainerList
                                    tagLangContainerList.AddRange(GetTagsLanguageContainer(tagsTranslations,
                                        group.GetLangauges()));
                                }
                            }
                            else if (tagLangs != null && tagLangs.Rows.Count > 0)
                            {
                                // check for translated tags according to tag id
                                DataRow[] translationRows = tagLangs.Select(string.Format("tag_id = {0}", tagId));
                                tagLangContainerList.AddRange(GetTagsLanguageContainer(translationRows,
                                    group.GetLangauges()));
                            }

                            // bundle LanguageContainers by tag id
                            if (!tagIdLanguageContainers.ContainsKey(tagId))
                            {
                                tagIdLanguageContainers.Add(tagId, tagLangContainerList);
                            }

                            if (!dicTagIdLanguageContainers.ContainsKey(tagTypeName))
                            {
                                dicTagIdLanguageContainers.Add(tagTypeName,
                                    new List<KeyValuePair<int, List<LanguageContainer>>>());
                            }

                            var k = new KeyValuePair<int, List<LanguageContainer>>(tagId,
                                tagIdLanguageContainers[tagId]);
                            dicTagIdLanguageContainers[tagTypeName].Add(k);
                        }
                    }
                }

                foreach (var tagType in tagTypeNameAndValues)
                {
                    tags = new Tags()
                    {
                        m_oTagMeta = new TagMeta(tagType.Key, typeof(string).ToString()),
                        m_lValues = tagType.Value.Select(x => x.Value).ToList()
                    };
                    tags.Values = new List<LanguageContainer[]>();
                    foreach (var n in dicTagIdLanguageContainers[tagType.Key])
                    {
                        tags.Values.Add(n.Value.ToArray());
                    }

                    tagList.Add(tags);
                }

                return tagList;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                result = false;
                return null;
            }
        }

        private static List<Metas> GetDateMetaDetails(DataTable dtDatesMeta, ref bool result)
        {
            try
            {
                result = true;
                List<Metas> lMetas = new List<Metas>();

                if (dtDatesMeta.Rows != null && dtDatesMeta.Rows.Count > 0)
                {
                    Metas oMeta = new Metas();
                    foreach (DataRow metaRow in dtDatesMeta.Rows)
                    {
                        oMeta = new Metas();
                        oMeta.m_oTagMeta = new TagMeta(ODBCWrapper.Utils.GetSafeStr(metaRow, "name"),
                            typeof(DateTime).ToString());
                        DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(metaRow, "value");
                        oMeta.m_sValue = dt.ToString(DateUtils.MAIN_FORMAT);
                        lMetas.Add(oMeta);
                    }
                }

                return lMetas;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                result = false;
                return null;
            }
        }

        /*Insert all metas that return from the "CompleteDetailsForMediaResponse" into List<Metas>*/
        private static List<Metas> GetMetaDetails(DataTable dtMedia, DataTable dtMeta, Group group, ref bool result)
        {
            try
            {
                result = true;
                Dictionary<string, string> metasValue = null;
                List<Metas> lMetas = new List<Metas>();
                int mediaGroupId = Utils.GetIntSafeVal(dtMedia.Rows[0], "GROUP_ID");
                if (group.m_oMetasValuesByGroupId.ContainsKey(mediaGroupId))
                {
                    metasValue = group.m_oMetasValuesByGroupId[mediaGroupId];
                }

                Metas oMeta = new Metas();
                string sFieldVal;
                for (int i = 1; i <= 20; i++)
                {
                    sFieldVal = string.Format("META{0}_STR", i);
                    if (metasValue != null && metasValue.ContainsKey(sFieldVal) &&
                        !string.IsNullOrEmpty(metasValue[sFieldVal]) &&
                        dtMedia.Rows[0][sFieldVal] != DBNull.Value)
                    {
                        oMeta.m_oTagMeta = new TagMeta(metasValue[sFieldVal], typeof(string).ToString());
                        oMeta.m_sValue = Utils.GetStrSafeVal(dtMedia.Rows[0], sFieldVal);
                        oMeta.Value =
                            GetMediaLanguageContainer(dtMedia.Rows[0], dtMeta, group.GetLangauges(), sFieldVal);
                        lMetas.Add(oMeta);
                    }

                    oMeta = new Metas();
                }

                for (int i = 1; i < 11; i++)
                {
                    sFieldVal = string.Format("META{0}_DOUBLE", i);
                    if (metasValue != null && metasValue.ContainsKey(sFieldVal) &&
                        !string.IsNullOrEmpty(metasValue[sFieldVal]) &&
                        dtMedia.Rows[0][sFieldVal] != DBNull.Value)
                    {
                        oMeta.m_oTagMeta = new TagMeta(metasValue[sFieldVal], typeof(double).ToString());
                        oMeta.m_sValue = Utils.GetStrSafeVal(dtMedia.Rows[0], sFieldVal);
                        lMetas.Add(oMeta);
                    }

                    oMeta = new Metas();
                }

                for (int i = 1; i < 11; i++)
                {
                    sFieldVal = string.Format("META{0}_BOOL", i);

                    if (metasValue != null && metasValue.ContainsKey(sFieldVal) &&
                        !string.IsNullOrEmpty(metasValue[sFieldVal]) &&
                        dtMedia.Rows[0][sFieldVal] != DBNull.Value)
                    {
                        oMeta.m_oTagMeta = new TagMeta(metasValue[sFieldVal], typeof(bool).ToString());
                        oMeta.m_sValue = Utils.GetStrSafeVal(dtMedia.Rows[0], sFieldVal);
                        lMetas.Add(oMeta);
                    }

                    oMeta = new Metas();
                }

                return lMetas;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                result = false;
                return null;
            }
        }

        private static string GetFictiveFileMediaUrl(int nMediaID, int nMediaFileID)
        {
            return string.Format("{0}||{1}", nMediaID, nMediaFileID);
        }

        /// <summary>
        /// in case of old server image: Insert all Pictures that return from the "CompleteDetailsForMediaResponse" into Pictures list
        /// in case of new server version: rebuild image URL so it will lead to new image server
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="assetId"></param>
        /// <param name="dtPic"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        internal static List<Picture> GetAllPic(int groupId, int assetId, DataTable dtPic, ref bool result,
            int assetGroupId)
        {
            result = true;
            List<Picture> lPicObject = new List<Picture>();
            Picture picObj;
            try
            {
                // use old/new image server

                if (WS_Utils.IsGroupIDContainedInConfig(groupId,
                    ApplicationConfiguration.Current.UseOldImageServer.Value, ';'))
                {
                    if (dtPic != null && dtPic.Rows != null)
                    {
                        // old image server
                        for (int i = 0; i < dtPic.Rows.Count; i++)
                        {
                            picObj = new Picture();
                            picObj.m_sSize = Utils.GetStrSafeVal(dtPic.Rows[i], "PicSize");
                            picObj.m_sURL = Utils.GetStrSafeVal(dtPic.Rows[i], "m_sURL");
                            picObj.ratio = Utils.GetStrSafeVal(dtPic.Rows[i], "ratio");
                            lPicObject.Add(picObj);
                        }
                    }
                }
                else
                {
                    // new image server - get group ratios
                    var groupRatios = CatalogCache.Instance().GetGroupRatios(assetGroupId);
                    if (groupRatios == null || groupRatios.Count == 0)
                    {
                        log.ErrorFormat("group ratios were not found. GID {0}", assetGroupId);
                        return null;
                    }

                    // get default images
                    List<PicData> defaultGroupPics = CatalogCache.Instance().GetDefaultImages(assetGroupId);

                    // get picture sizes
                    List<PicSize> pictureSizes = CatalogCache.Instance().GetGroupPicSizes(assetGroupId);

                    // get pictures data
                    DataRowCollection rows = CatalogDAL.GetPicsTableData(assetId, eAssetImageType.Media);

                    if ((rows != null && rows.Count > 0) ||
                        (defaultGroupPics != null && defaultGroupPics.Count > 0))
                    {
                        List<PicData> picsTableData = new List<PicData>();
                        if (rows != null)
                        {
                            foreach (DataRow row in rows)
                            {
                                picsTableData.Add(new PicData()
                                {
                                    RatioId = Utils.GetIntSafeVal(row, "RATIO_ID"),
                                    Version = Utils.GetIntSafeVal(row, "VERSION"),
                                    BaseUrl = Utils.GetStrSafeVal(row, "BASE_URL"),
                                    PicId = Utils.GetLongSafeVal(row, "ID"),
                                    Ratio = Utils.GetStrSafeVal(row, "RATIO"),
                                    GroupId = Utils.GetIntSafeVal(row, "GROUP_ID")
                                });
                            }
                        }

                        // check if migrated image or new one
                        bool isMigratedImage = false;
                        PicData picDataZeroRatio = picsTableData.FirstOrDefault(x => x.RatioId == 0);
                        if (picDataZeroRatio != null)
                            isMigratedImage = true;

                        if (pictureSizes != null && pictureSizes.Count > 0)
                        {
                            // new images server with picture sizes
                            foreach (var pictureSize in pictureSizes)
                            {
                                var ratio = groupRatios.FirstOrDefault(x => x.Id == pictureSize.RatioId);
                                if (ratio == null)
                                {
                                    log.DebugFormat(
                                        "picture size doesn't have a corresponding group ratio configuration. GID: {0}, RatioId: {1}",
                                        assetGroupId, pictureSize.RatioId);
                                    continue;
                                }

                                picObj = new Picture();

                                // ratio ID
                                var pic = picsTableData.FirstOrDefault(x => x.RatioId == pictureSize.RatioId);

                                if (pic == null && isMigratedImage)
                                    pic = picDataZeroRatio;

                                if (pic == null && defaultGroupPics != null)
                                {
                                    pic = defaultGroupPics.FirstOrDefault(x => x.RatioId == pictureSize.RatioId);
                                    picObj.isDefault = true;
                                }

                                if (pic == null)
                                    continue;

                                // get size string: <width>X<height>
                                picObj.m_sSize = string.Format("{0}X{1}", pictureSize.Width, pictureSize.Height);

                                // get ratio string
                                picObj.ratio = ratio.Name;

                                // get picture id: <pic_base_url>_<ratio_id>
                                picObj.id = string.Format("{0}_{1}", Path.GetFileNameWithoutExtension(pic.BaseUrl),
                                    ratio.Id);

                                // get version: if ratio_id exists in pictures table => get its version
                                picObj.version = pic.Version;

                                // build image URL.
                                // template: <image_server_url>/p/<partner_id>/entry_id/<image_id>/version/<image_version>/width/<image_width>/height/<image_height>/quality/<image_quality>
                                // Example:  http://localhost/ImageServer/Service.svc/GetImage/p/215/entry_id/123/version/10/width/432/height/230/quality/100
                                picObj.m_sURL = ImageUtils.BuildImageUrl(groupId, picObj.id, picObj.version,
                                    pictureSize.Width, pictureSize.Height, 100);

                                lPicObject.Add(picObj);
                            }
                        }
                        else
                        {
                            // build picture object for each ratio
                            foreach (var ratio in groupRatios)
                            {
                                picObj = new Picture();

                                // get ratio string
                                picObj.ratio = ratio.Name;

                                // get version
                                PicData pic = picsTableData.FirstOrDefault(x => x.RatioId == ratio.Id);

                                if (pic == null && isMigratedImage)
                                    pic = picDataZeroRatio;

                                if (pic == null && defaultGroupPics != null)
                                {
                                    pic = defaultGroupPics.FirstOrDefault(x => x.RatioId == ratio.Id);
                                    picObj.isDefault = true;
                                }

                                if (pic == null)
                                    continue;

                                // get picture id: <pic_base_url>_<ratio_id>
                                picObj.id = string.Format("{0}_{1}", Path.GetFileNameWithoutExtension(pic.BaseUrl),
                                    pic.RatioId);

                                picObj.version = pic.Version;


                                // build image URL.
                                // template: <image_server_url>/p/<partner_id>/entry_id/<image_id>/version/<image_version>
                                // Example:  http://localhost/ImageServer/Service.svc/GetImage/p/215/entry_id/123/version/10
                                picObj.m_sURL = ImageUtils.BuildImageUrl(groupId, picObj.id, picObj.version, 0, 0, 100,
                                    true);

                                lPicObject.Add(picObj);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                result = false;
                return null;
            }

            return lPicObject;
        }

        /*Insert all Basic Details about media  that return from the "CompleteDetailsForMediaResponse" into MediaObj*/
        private static bool GetMediaBasicDetails(ref MediaObj oMediaObj, DataTable dtMedia, DataTable mediaMetas,
            List<LanguageObj> groupLanguages, DataTable dtUpdateDate
            , bool bIsMainLang, ref int assetGroupId, ref int isLinear, bool managementData = false)
        {
            bool result = false;
            try
            {
                if (dtMedia.Columns != null)
                {
                    if (dtMedia.Rows.Count != 0)
                    {
                        assetGroupId = Utils.GetIntSafeVal(dtMedia.Rows[0], "GROUP_ID");
                        result = true;
                        oMediaObj.AssetId = Utils.GetStrSafeVal(dtMedia.Rows[0], "ID");
                        oMediaObj.EntryId = Utils.GetStrSafeVal(dtMedia.Rows[0], "ENTRY_ID");
                        oMediaObj.CoGuid = Utils.GetStrSafeVal(dtMedia.Rows[0], "CO_GUID");

                        oMediaObj.Name = GetMediaLanguageContainer(dtMedia.Rows[0], mediaMetas, groupLanguages, "NAME");
                        if (oMediaObj.Name != null && oMediaObj.Name.Length > 0)
                        {
                            oMediaObj.m_sName = oMediaObj.Name[0].m_sValue;
                        }

                        oMediaObj.Description =
                            GetMediaLanguageContainer(dtMedia.Rows[0], mediaMetas, groupLanguages, "DESCRIPTION");
                        if (oMediaObj.Description != null && oMediaObj.Description.Length > 0)
                        {
                            oMediaObj.m_sDescription = oMediaObj.Description[0].m_sValue;
                        }

                        oMediaObj.m_oMediaType = new MediaType();
                        oMediaObj.m_oMediaType.m_nTypeID = Utils.GetIntSafeVal(dtMedia.Rows[0], "MEDIA_TYPE_ID");
                        oMediaObj.m_oMediaType.m_sTypeName = Utils.GetStrSafeVal(dtMedia.Rows[0], "typeDescription");
                        oMediaObj.m_nLikeCounter = Utils.GetIntSafeVal(dtMedia.Rows[0], "like_counter");

                        string sEpgIdentifier = Utils.GetStrSafeVal(dtMedia.Rows[0], "EPG_IDENTIFIER");
                        if (!string.IsNullOrEmpty(sEpgIdentifier))
                        {
                            oMediaObj.m_ExternalIDs = sEpgIdentifier;
                        }

                        isLinear = Utils.GetIntSafeVal(dtMedia.Rows[0], "IS_LINEAR");
                        //Rating
                        oMediaObj.m_oRatingMedia = new RatingMedia();
                        oMediaObj.m_oRatingMedia.m_nViwes = Utils.GetIntSafeVal(dtMedia.Rows[0], "Viwes");
                        oMediaObj.m_oRatingMedia.m_nRatingSum = Utils.GetIntSafeVal(dtMedia.Rows[0], "RatingSum");
                        oMediaObj.m_oRatingMedia.m_nRatingCount = Utils.GetIntSafeVal(dtMedia.Rows[0], "RatingCount");

                        if (oMediaObj.m_oRatingMedia.m_nRatingCount > 0)
                        {
                            oMediaObj.m_oRatingMedia.m_nRatingAvg =
                                (double) ((double) oMediaObj.m_oRatingMedia.m_nRatingSum /
                                          (double) oMediaObj.m_oRatingMedia.m_nRatingCount);
                        }

                        oMediaObj.m_oRatingMedia.m_nVotesLoCnt = Utils.GetIntSafeVal(dtMedia.Rows[0], "VotesLoCnt");
                        oMediaObj.m_oRatingMedia.m_nVotesUpCnt = Utils.GetIntSafeVal(dtMedia.Rows[0], "VotesUpCnt");
                        oMediaObj.m_oRatingMedia.m_nVote1Count = Utils.GetIntSafeVal(dtMedia.Rows[0], "VOTES_1_COUNT");
                        oMediaObj.m_oRatingMedia.m_nVote2Count = Utils.GetIntSafeVal(dtMedia.Rows[0], "VOTES_2_COUNT");
                        oMediaObj.m_oRatingMedia.m_nVote3Count = Utils.GetIntSafeVal(dtMedia.Rows[0], "VOTES_3_COUNT");
                        oMediaObj.m_oRatingMedia.m_nVote4Count = Utils.GetIntSafeVal(dtMedia.Rows[0], "VOTES_4_COUNT");
                        oMediaObj.m_oRatingMedia.m_nVote5Count = Utils.GetIntSafeVal(dtMedia.Rows[0], "VOTES_5_COUNT");

                        //Dates
                        string sDate = string.Empty;
                        sDate = Utils.GetStrSafeVal(dtMedia.Rows[0], "CATALOG_START_DATE");
                        if (!string.IsNullOrEmpty(sDate))
                        {
                            oMediaObj.m_dCatalogStartDate = System.Convert.ToDateTime(sDate);
                            sDate = string.Empty;
                        }

                        sDate = Utils.GetStrSafeVal(dtMedia.Rows[0], "CREATE_DATE");
                        if (!string.IsNullOrEmpty(dtMedia.Rows[0]["CREATE_DATE"].ToString()))
                        {
                            oMediaObj.m_dCreationDate = System.Convert.ToDateTime(sDate);
                            sDate = string.Empty;
                        }

                        sDate = Utils.GetStrSafeVal(dtMedia.Rows[0], "FINAL_END_DATE");
                        if (!string.IsNullOrEmpty(sDate))
                        {
                            oMediaObj.m_dFinalDate = System.Convert.ToDateTime(sDate);
                            sDate = string.Empty;
                        }
                        else
                        {
                            oMediaObj.m_dFinalDate = DateTime.MaxValue;
                            sDate = string.Empty;
                        }

                        sDate = Utils.GetStrSafeVal(dtMedia.Rows[0], "PUBLISH_DATE");
                        if (!string.IsNullOrEmpty(sDate))
                        {
                            oMediaObj.m_dPublishDate = System.Convert.ToDateTime(sDate);
                            sDate = string.Empty;
                        }

                        sDate = Utils.GetStrSafeVal(dtMedia.Rows[0], "START_DATE");
                        if (!string.IsNullOrEmpty(sDate))
                        {
                            oMediaObj.m_dStartDate = System.Convert.ToDateTime(sDate);
                            sDate = string.Empty;
                        }

                        sDate = Utils.GetStrSafeVal(dtMedia.Rows[0], "END_DATE");
                        if (!string.IsNullOrEmpty(sDate))
                        {
                            oMediaObj.m_dEndDate = System.Convert.ToDateTime(sDate);
                            sDate = string.Empty;
                        }
                        else
                        {
                            oMediaObj.m_dEndDate = DateTime.MaxValue;
                            sDate = string.Empty;
                        }

                        //UpdateDate
                        if (dtUpdateDate != null)
                        {
                            sDate = Utils.GetStrSafeVal(dtUpdateDate.Rows[0], "UPDATE_DATE");
                            if (!string.IsNullOrEmpty(sDate))
                            {
                                oMediaObj.m_dUpdateDate = System.Convert.ToDateTime(sDate);
                                sDate = string.Empty;
                            }
                        }

                        // is active
                        oMediaObj.IsActive = Utils.GetIntSafeVal(dtMedia.Rows[0], "IS_ACTIVE") == 1 ? true : false;

                        if (managementData)
                        {
                            if (Utils.GetIntSafeVal(dtMedia.Rows[0], "device_rule_id") > 0)
                            {
                                oMediaObj.DeviceRule = TvmRuleManager.GetDeviceRuleName(assetGroupId,
                                    Utils.GetIntSafeVal(dtMedia.Rows[0], "device_rule_id"));
                            }

                            if (Utils.GetIntSafeVal(dtMedia.Rows[0], "WATCH_PERMISSION_TYPE_ID") > 0)
                            {
                                oMediaObj.WatchPermissionRule = GetWatchPermissionTypeName(assetGroupId,
                                    Utils.GetIntSafeVal(dtMedia.Rows[0], "WATCH_PERMISSION_TYPE_ID"));
                            }

                            if (Utils.GetIntSafeVal(dtMedia.Rows[0], "BLOCK_TEMPLATE_ID") > 0)
                            {
                                oMediaObj.GeoblockRule = TvmRuleManager.GetGeoBlockRuleName(assetGroupId,
                                    Utils.GetIntSafeVal(dtMedia.Rows[0], "BLOCK_TEMPLATE_ID"));
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return false;
            }
        }

        private static LanguageContainer[] GetMediaLanguageContainer(DataRow mediaRow, DataTable metasTranslations,
            List<LanguageObj> groupLanguages, string columnName)
        {
            List<LanguageContainer> langContainers = new List<LanguageContainer>();
            LanguageObj language = null;
            string value = string.Empty;
            int langId = 0;

            if (metasTranslations != null && metasTranslations.Rows.Count > 0)
            {
                langContainers = new List<LanguageContainer>();
                value = string.Empty;
                langId = 0;

                foreach (DataRow row in metasTranslations.Rows)
                {
                    langId = Utils.GetIntSafeVal(row, "LANGUAGE_ID");
                    value = Utils.GetStrSafeVal(row, columnName);
                    if (!string.IsNullOrEmpty(value) && langId > 0)
                    {
                        language = groupLanguages.Where(x => x.ID == langId).FirstOrDefault();
                        if (language != null)
                            langContainers.Add(new LanguageContainer()
                                {m_sLanguageCode3 = language.Code, m_sValue = value});
                    }
                }
            }

            //add is default lang values
            language = groupLanguages.Where(x => x.IsDefault).FirstOrDefault();
            value = Utils.GetStrSafeVal(mediaRow, columnName);
            if (language != null)
                langContainers.Add(new LanguageContainer() {m_sLanguageCode3 = language.Code, m_sValue = value});

            return langContainers.ToArray();
        }

        private static List<LanguageContainer> GetTagsLanguageContainer(DataRow[] translatedTags,
            List<LanguageObj> groupLanguages)
        {
            List<LanguageContainer> langContainers = new List<LanguageContainer>();
            LanguageObj language = null;

            if (translatedTags != null)
            {
                string value = string.Empty;
                int langId = 0;

                foreach (DataRow row in translatedTags)
                {
                    langId = Utils.GetIntSafeVal(row, "LANGUAGE_ID");
                    value = Utils.GetStrSafeVal(row, "value");
                    if (!string.IsNullOrEmpty(value) && langId > 0)
                    {
                        language = groupLanguages.FirstOrDefault(x => x.ID == langId);
                        if (language != null)
                            langContainers.Add(new LanguageContainer()
                                {m_sLanguageCode3 = language.Code, m_sValue = value});
                    }
                }
            }

            return langContainers;
        }

        private static string GetWatchPermissionTypeName(int assetGroupId, int watchPermissionRuleId)
        {
            Dictionary<int, string> watchPermissionsTypes =
                CatalogCache.Instance().GetGroupWatchPermissionsTypes(assetGroupId);
            if (watchPermissionsTypes == null || watchPermissionsTypes.Count == 0)
            {
                log.ErrorFormat("group watchPermissionsTypes were not found. GID {0}", assetGroupId);
                return string.Empty;
            }

            if (watchPermissionsTypes.ContainsKey(watchPermissionRuleId))
                return watchPermissionsTypes[watchPermissionRuleId];
            else
            {
                log.ErrorFormat("group watchPermissionsType {0} were not found. GID {1}", watchPermissionRuleId,
                    assetGroupId);
                return string.Empty;
            }
        }

        private static string GetMediaQuality(int mediaQualityId)
        {
            Dictionary<int, string> mediaQualities = CatalogCache.Instance().GetMediaQualities();
            if (mediaQualities == null || mediaQualities.Count == 0)
            {
                log.ErrorFormat("mediaQualities were not found. mediaQuality {0}", mediaQualityId);
                return string.Empty;
            }

            if (mediaQualities.ContainsKey(mediaQualityId))
                return mediaQualities[mediaQualityId];
            else
            {
                log.ErrorFormat("MediaQualityId {0} were not found.", mediaQualityId);
                return string.Empty;
            }
        }

        /*Call To Searcher to get mediaIds by search Object*/
        public static List<SearchResult> GetMediaIdsFromSearcher(BaseMediaSearchRequest oMediaRequest,
            ref int nTotalItems)
        {
            List<SearchResult> lSearchResults = null;

            #region SearchMedias

            try
            {
                IIndexManager indexManager = IndexManagerFactory.Instance.GetIndexManager(oMediaRequest.m_nGroupID);
                ApiObjects.SearchObjects.MediaSearchObj search = null;

                // Group have user types per media  +  siteGuid != empty
                if (!string.IsNullOrEmpty(oMediaRequest.m_sSiteGuid) && IsGroupHaveUserType(oMediaRequest))
                {
                    if (oMediaRequest.m_oFilter == null)
                    {
                        oMediaRequest.m_oFilter = new Filter();
                    }

                    //call ws_users to get userType
                    oMediaRequest.m_oFilter.m_nUserTypeID =
                        Utils.GetUserType(oMediaRequest.m_sSiteGuid, oMediaRequest.m_nGroupID);
                }

                search = BuildSearchObject(oMediaRequest);

                search.m_nPageIndex = oMediaRequest.m_nPageIndex;
                search.m_nPageSize = oMediaRequest.m_nPageSize;

                GroupManager groupManager = new GroupManager();
                CatalogCache catalogCache = CatalogCache.Instance();
                int nParentGroupID = catalogCache.GetParentGroup(oMediaRequest.m_nGroupID);
                Group groupInCache = groupManager.GetGroup(nParentGroupID);

                if (groupInCache != null)
                {
                    LanguageObj objLang = groupInCache.GetLanguage(oMediaRequest.m_oFilter.m_nLanguage);
                    search.m_oLangauge = objLang;
                }

                SearchResultsObj resultObj =
                    indexManager.SearchMedias(search, 0, oMediaRequest.m_oFilter.m_bUseStartDate);

                if (resultObj != null)
                {
                    lSearchResults = resultObj.m_resultIDs;
                    nTotalItems = resultObj.n_TotalItems;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }

            #endregion

            return lSearchResults;
        }

        /// <summary>
        /// Builds search object and performs query to get asset Ids that match the request requirements
        /// </summary>
        /// <param name="request"></param>
        /// <param name="totalItems"></param>
        /// <returns></returns>
        public List<UnifiedSearchResult> GetAssetIdFromSearcher(UnifiedSearchRequest request, ref int totalItems,
            ref int to,
            out List<AggregationsResult> aggregationsResults)
        {
            List<UnifiedSearchResult> searchResultsList = new List<UnifiedSearchResult>();
            aggregationsResults = null;
            totalItems = 0;

            // Group have user types per media  +  siteGuid != empty
            if (!string.IsNullOrEmpty(request.m_sSiteGuid) && Utils.IsGroupIDContainedInConfig(request.m_nGroupID,
                ApplicationConfiguration.Current.CatalogLogicConfiguration.GroupsWithIUserTypeSeperatedBySemiColon
                    .Value, ';'))
            {
                if (request.m_oFilter == null)
                {
                    request.m_oFilter = new Filter();
                }

                //call ws_users to get userType
                request.m_oFilter.m_nUserTypeID = Utils.GetUserType(request.m_sSiteGuid, request.m_nGroupID);
            }

            UnifiedSearchDefinitions searchDefinitions = BuildUnifiedSearchObject(request);

            UnifiedSearchResponse searchResult;

            string deviceId = string.Empty;
            int langId = 0;

            if (request.m_oFilter != null)
            {
                deviceId = request.m_oFilter.m_sDeviceId;
                langId = request.m_oFilter.m_nLanguage;
            }

            string cacheKey = GetSearchCacheKey(request.m_nGroupID, request.m_sSiteGuid, request.domainId, deviceId,
                request.m_sUserIP, request.assetTypes, request.filterQuery,
                searchDefinitions, searchDefinitions.PersonalData, langId);

            if (!string.IsNullOrEmpty(cacheKey))
            {
                log.DebugFormat("Going to get search assets from cache with key: {0}", cacheKey);
                searchResult = GetUnifiedSearchResultsFromCache(request.m_nGroupID, searchDefinitions, cacheKey);
            }
            else
            {
                log.DebugFormat("Going to get search assets from ES");
                searchResult = UnifiedSearch(request.m_nGroupID, searchDefinitions);
            }

            searchResultsList = searchResult.searchResults;
            totalItems = searchResult.m_nTotalItems;
            aggregationsResults = searchResult.aggregationResults;

            return searchResultsList;
        }

        private static UnifiedSearchResponse GetUnifiedSearchResultsFromCache(int groupId,
            UnifiedSearchDefinitions unifiedSearchDefinitions, string cacheKey)
        {
            UnifiedSearchResponse cachedResult = new UnifiedSearchResponse()
            {
                status = new ApiObjects.Response.Status((int) eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            if (!LayeredCache.Instance.Get<UnifiedSearchResponse>(cacheKey, ref cachedResult, GetUnifiedSearchResults,
                new Dictionary<string, object>()
                    {{"groupId", groupId}, {"unifiedSearchDefinitions", unifiedSearchDefinitions}},
                groupId, LayeredCacheConfigNames.UNIFIED_SEARCH_WITH_PERSONAL_DATA, null, true))
            {
                log.ErrorFormat("Failed getting unified search results from LayeredCache, key: {0}", cacheKey);
            }

            return cachedResult;
        }

        private static Tuple<UnifiedSearchResponse, bool> GetUnifiedSearchResults(Dictionary<string, object> funcParams)
        {
            bool result = false;
            UnifiedSearchResponse cachedResult = new UnifiedSearchResponse()
            {
                status = new ApiObjects.Response.Status((int) eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            try
            {
                if (funcParams != null && funcParams.Count == 2)
                {
                    if (funcParams.ContainsKey("groupId") && funcParams.ContainsKey("unifiedSearchDefinitions"))
                    {
                        int? groupId = funcParams["groupId"] as int?;
                        UnifiedSearchDefinitions unifiedSearchDefinitions =
                            funcParams["unifiedSearchDefinitions"] as UnifiedSearchDefinitions;

                        if (groupId.HasValue && unifiedSearchDefinitions != null)
                        {
                            cachedResult = UnifiedSearch(groupId.Value, unifiedSearchDefinitions);

                            if (cachedResult.status.Code == (int) eResponseStatus.OK)
                            {
                                result = true;
                            }
                            else
                            {
                                log.ErrorFormat("Failed to get search results from ES, code = {0}, message = {1}",
                                    cachedResult.status.Code, cachedResult.status.Message);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(
                    string.Format("GetUnifiedSearchResults failed params : {0}", string.Join(";", funcParams.Keys)),
                    ex);
            }

            return new Tuple<UnifiedSearchResponse, bool>(cachedResult, result);
        }

        public static UnifiedSearchResponse UnifiedSearch(int groupId,
            UnifiedSearchDefinitions unifiedSearchDefinitions)
        {
            UnifiedSearchResponse response = new UnifiedSearchResponse();
            int parentGroupId = CatalogCache.Instance().GetParentGroup(groupId);
            IIndexManager indexManager = IndexManagerFactory.Instance.GetIndexManager(parentGroupId);

            int totalItems = 0;
            List<UnifiedSearchResult> searchResults = indexManager.UnifiedSearch(unifiedSearchDefinitions,
                ref totalItems, out var aggregationsResults);

            if (searchResults != null)
            {
                response.searchResults = searchResults;
            }

            response.m_nTotalItems = totalItems;
            response.aggregationResults = aggregationsResults;
            response.status = new ApiObjects.Response.Status((int) eResponseStatus.OK);

            return response;
        }

        /// <summary>
        /// Creates a language object for a given group
        /// </summary>
        /// <param name="request"></param>
        /// <param name="searchDefinitions"></param>
        private static LanguageObj GetLanguage(int groupId, int languageId)
        {
            LanguageObj language = null;

            GroupManager groupManager = new GroupManager();
            CatalogCache catalogCache = CatalogCache.Instance();
            int parentGroupId = catalogCache.GetParentGroup(groupId);
            Group groupInCache = groupManager.GetGroup(parentGroupId);

            if (groupInCache != null)
            {
                if (languageId <= 0)
                {
                    language = groupInCache.GetGroupDefaultLanguage();
                }
                else
                {
                    language = groupInCache.GetLanguage(languageId);
                }
            }

            return language;
        }

        /// <summary>
        /// For a given request, creates the proper definitions that the wrapper will use
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        internal static UnifiedSearchDefinitions BuildUnifiedSearchObject(UnifiedSearchRequest request)
        {
            CatalogGroupCache catalogGroupCache = null;
            var _ = CatalogManager.Instance.DoesGroupUsesTemplates(request.m_nGroupID) &&
                CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(request.m_nGroupID, out catalogGroupCache);

            var resultProcessor = new FilterTreeResultProcessor();
            var filterTreeValidator = new FilterTreeValidator(resultProcessor, catalogGroupCache?.GetProgramAssetStructId());
            UnifiedSearchDefinitionsBuilder definitionsCache = new UnifiedSearchDefinitionsBuilder(
                filterTreeValidator,
                AssetOrderingService.Instance);

            UnifiedSearchDefinitions definitions = definitionsCache.GetDefinitions(request);

            definitions.pageIndex = request.m_nPageIndex;
            definitions.pageSize = request.m_nPageSize;
            definitions.from = request.from;

            return definitions;
        }

        internal static void GetParentalRulesTags(int groupId, string siteGuid,
            out Dictionary<string, List<string>> mediaTags, out Dictionary<string, List<string>> epgTags)
        {
            mediaTags = new Dictionary<string, List<string>>();
            epgTags = new Dictionary<string, List<string>>();

            // Call webservice method
            var serviceResponse = Api.Module.GetUserParentalRuleTags(groupId, siteGuid, 0);

            // Validate webservice response
            if (serviceResponse == null || serviceResponse.status == null)
            {
                throw new Exception(string.Format(
                    "Error when getting user parental rule tags from WS_API - response from WS is null! ......... user_id = {0}, group_id = {1}",
                    siteGuid, groupId));
            }

            if (serviceResponse.status.Code != 0)
            {
                throw new Exception(string.Format(
                    "Error when getting user parental rule tags from  code ={0}, message = {1}, user_id = {2}, group_id = {3}",
                    serviceResponse.status.Code, serviceResponse.status.Message,
                    siteGuid, groupId));
            }

            Group group = new GroupManager().GetGroup(groupId);

            // Media: Convert TagPair array to our dictionary
            if (serviceResponse.mediaTags != null)
            {
                foreach (var tag in serviceResponse.mediaTags)
                {
                    // If the tag of the rule exists on this group (might happen if tag is deleted after rule is created)
                    if (group.m_oGroupTags.ContainsKey(tag.id))
                    {
                        string tagName = group.m_oGroupTags[tag.id];

                        if (!mediaTags.ContainsKey(tagName))
                        {
                            mediaTags[tagName] = new List<string>();
                        }

                        if (tag.value != null)
                        {
                            mediaTags[tagName].Add(tag.value);
                        }
                    }
                }
            }

            // EPG: Convert TagPair array to our dictionary
            if (serviceResponse.epgTags != null)
            {
                foreach (var tag in serviceResponse.epgTags)
                {
                    // If the tag of the rule exists on this group (might happen if tag is deleted after rule is created)
                    if (group.m_oGroupTags.ContainsKey(tag.id))
                    {
                        string tagName = group.m_oEpgGroupSettings.tags[tag.id].ToString();

                        if (!epgTags.ContainsKey(tagName))
                        {
                            epgTags[tagName] = new List<string>();
                        }

                        if (tag.value != null)
                        {
                            epgTags[tagName].Add(tag.value);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// For a given IP, gets all the rules that don't block this specific IP
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        private static List<int> GetGeoBlockRules(int groupId, string ip)
        {
            int countryId = Utils.GetIP2CountryId(groupId, ip);

            //GroupsCache.Instance().
            List<int> result = GeoBlockRulesCache.Instance().GetGeoBlockRulesByCountry(groupId, countryId);

            // Make sure DAL didn't return empty result
            if (result == null)
            {
                result = new List<int>();
            }

            // Always add 0, for media without rules at all
            result.Add(0);

            return result;
        }

        /// <summary>
        /// Verifies that the search key is a tag or a meta of either EPG or media
        /// </summary>
        /// <param name="originalKey"></param>
        /// <param name="group"></param>
        /// <param name="isTagOrMeta"></param>
        /// <returns></returns>
        public static HashSet<BooleanLeafFieldDefinitions> GetUnifiedSearchKey(string originalKey, Group group,
            int groupId)
        {
            Type type;
            if (CatalogManagement.CatalogManager.Instance.DoesGroupUsesTemplates(groupId))
            {
                return CatalogManagement.CatalogManager.Instance.GetUnifiedSearchKey(groupId, originalKey);
            }
            else
            {
                return GetUnifiedSearchKey(originalKey, group);
            }
        }

        /// <summary>
        /// Verifies that the search key is a tag or a meta of either EPG or media
        /// </summary>
        /// <param name="originalKey"></param>
        /// <param name="group"></param>
        /// <param name="isTagOrMeta"></param>
        /// <returns></returns>
        public static HashSet<BooleanLeafFieldDefinitions> GetUnifiedSearchKey(string originalKey, Group group)
        {
            Type valueType = typeof(string);

            HashSet<BooleanLeafFieldDefinitions> searchKeys = new HashSet<BooleanLeafFieldDefinitions>();
            HashSet<string> alreadyContained = new HashSet<string>();
            // get alias + regex expression
            List<FieldTypeEntity> FieldEpgAliasMapping =
                ConditionalAccess.Utils.Instance.GetAliasMappingFields(group.m_nParentGroupID);
            Dictionary<string, string> reverseDictionary = new Dictionary<string, string>();
            var dictionaries = group.m_oMetasValuesByGroupId.Select(i => i.Value).Cast<Dictionary<string, string>>();

            foreach (var dictionary in dictionaries)
            {
                foreach (var pair in dictionary)
                {
                    reverseDictionary[pair.Value] = pair.Key;
                }
            }

            if (originalKey.StartsWith("tags."))
            {
                foreach (string tag in group.m_oGroupTags.Values)
                {
                    string tagToLower = tag.ToLower();
                    if (!alreadyContained.Contains(tagToLower) &&
                        tag.Equals(originalKey.Substring(5), StringComparison.OrdinalIgnoreCase))
                    {
                        alreadyContained.Add(tagToLower);
                        searchKeys.Add(new BooleanLeafFieldDefinitions()
                        {
                            Field = tagToLower,
                            ValueType = valueType,
                            FieldType = eFieldType.Tag
                        });
                        break;
                    }
                }

                foreach (FieldTypeEntity FieldEpgAlias in
                    FieldEpgAliasMapping.Where(x => x.FieldType == FieldTypes.Tag))
                {
                    string tagToLower = FieldEpgAlias.Name.ToLower();
                    if (!alreadyContained.Contains(tagToLower) &&
                        FieldEpgAlias.Alias.Equals(originalKey.Substring(5), StringComparison.OrdinalIgnoreCase))
                    {
                        alreadyContained.Add(tagToLower);
                        searchKeys.Add(new BooleanLeafFieldDefinitions()
                        {
                            Field = tagToLower,
                            ValueType = valueType,
                            FieldType = eFieldType.Tag
                        });
                        break;
                    }
                }

                foreach (var tag in group.m_oEpgGroupSettings.m_lTagsName)
                {
                    string tagToLower = tag.ToLower();
                    if (!alreadyContained.Contains(tag) &&
                        tag.Equals(originalKey.Substring(5), StringComparison.OrdinalIgnoreCase))
                    {
                        alreadyContained.Add(tagToLower);
                        searchKeys.Add(new BooleanLeafFieldDefinitions()
                        {
                            Field = tagToLower,
                            ValueType = valueType,
                            FieldType = eFieldType.Tag
                        });
                        break;
                    }
                }
            }
            else if (originalKey.StartsWith("metas."))
            {
                var metas = group.m_oMetasValuesByGroupId.Select(i => i.Value).Cast<Dictionary<string, string>>()
                    .SelectMany(d => d.Values).ToList();

                foreach (var meta in metas)
                {
                    string metaToLower = meta.ToLower();
                    if (!alreadyContained.Contains(meta) &&
                        meta.Equals(originalKey.Substring(6), StringComparison.OrdinalIgnoreCase))
                    {
                        GetMetaType(reverseDictionary[meta], out valueType);
                        alreadyContained.Add(metaToLower);
                        searchKeys.Add(new BooleanLeafFieldDefinitions()
                        {
                            Field = metaToLower,
                            ValueType = valueType,
                            FieldType = valueType == typeof(string) ? eFieldType.StringMeta : eFieldType.NonStringMeta
                        });
                        break;
                    }
                }

                foreach (FieldTypeEntity FieldEpgAlias in FieldEpgAliasMapping.Where(
                    x => x.FieldType == FieldTypes.Meta))
                {
                    string metaToLower = FieldEpgAlias.Name.ToLower();
                    if (!alreadyContained.Contains(FieldEpgAlias.Name) &&
                        FieldEpgAlias.Alias.Equals(originalKey.Substring(6), StringComparison.OrdinalIgnoreCase))
                    {
                        alreadyContained.Add(metaToLower);
                        searchKeys.Add(new BooleanLeafFieldDefinitions()
                        {
                            Field = metaToLower,
                            ValueType = valueType,
                            FieldType = eFieldType.StringMeta
                        });
                        break;
                    }
                }

                foreach (var meta in group.m_oEpgGroupSettings.m_lMetasName)
                {
                    string metaToLower = meta.ToLower();
                    if (!alreadyContained.Contains(meta) &&
                        meta.Equals(originalKey.Substring(6), StringComparison.OrdinalIgnoreCase))
                    {
                        alreadyContained.Add(metaToLower);
                        searchKeys.Add(new BooleanLeafFieldDefinitions()
                        {
                            Field = meta.ToLower(),
                            ValueType = valueType,
                            FieldType = eFieldType.StringMeta
                        });
                        break;
                    }
                }
            }
            else
            {
                #region Tags

                foreach (string tag in group.m_oGroupTags.Values)
                {
                    string tagToLower = tag.ToLower();
                    if (!alreadyContained.Contains(tagToLower) &&
                        tag.Equals(originalKey, StringComparison.OrdinalIgnoreCase))
                    {
                        alreadyContained.Add(tagToLower);
                        searchKeys.Add(new BooleanLeafFieldDefinitions()
                        {
                            Field = tagToLower,
                            ValueType = valueType,
                            FieldType = eFieldType.Tag
                        });
                        break;
                    }
                }

                foreach (var tag in group.m_oEpgGroupSettings.m_lTagsName)
                {
                    string tagToLower = tag.ToLower();
                    if (!alreadyContained.Contains(tagToLower) &&
                        tag.Equals(originalKey, StringComparison.OrdinalIgnoreCase))
                    {
                        alreadyContained.Add(tagToLower);
                        searchKeys.Add(new BooleanLeafFieldDefinitions()
                        {
                            Field = tagToLower,
                            ValueType = valueType,
                            FieldType = eFieldType.Tag
                        });
                        break;
                    }
                }

                foreach (FieldTypeEntity FieldEpgAlias in
                    FieldEpgAliasMapping.Where(x => x.FieldType == FieldTypes.Tag))
                {
                    string tagToLower = FieldEpgAlias.Name.ToLower();
                    if (!alreadyContained.Contains(tagToLower) &&
                        FieldEpgAlias.Alias.Equals(originalKey, StringComparison.OrdinalIgnoreCase))
                    {
                        alreadyContained.Add(tagToLower);
                        searchKeys.Add(new BooleanLeafFieldDefinitions()
                        {
                            Field = tagToLower,
                            ValueType = valueType,
                            FieldType = eFieldType.Tag
                        });
                        break;
                    }
                }

                #endregion

                // check for unique tags/metas separately, but too lazy to have two different hashsets :-)
                alreadyContained.Clear();

                #region Metas

                var metas = group.m_oMetasValuesByGroupId.Select(i => i.Value).Cast<Dictionary<string, string>>()
                    .SelectMany(d => d.Values).ToList();

                foreach (var meta in metas)
                {
                    string metaToLower = meta.ToLower();
                    if (!alreadyContained.Contains(metaToLower) &&
                        meta.Equals(originalKey, StringComparison.OrdinalIgnoreCase))
                    {
                        GetMetaType(reverseDictionary[meta], out valueType);
                        alreadyContained.Add(metaToLower);
                        searchKeys.Add(new BooleanLeafFieldDefinitions()
                        {
                            Field = metaToLower,
                            ValueType = valueType,
                            FieldType = valueType == typeof(string) ? eFieldType.StringMeta : eFieldType.NonStringMeta
                        });
                        break;
                    }
                }

                foreach (FieldTypeEntity FieldEpgAlias in FieldEpgAliasMapping.Where(
                    x => x.FieldType == FieldTypes.Meta))
                {
                    string metaToLower = FieldEpgAlias.Name.ToLower();
                    if (!alreadyContained.Contains(metaToLower) &&
                        FieldEpgAlias.Alias.Equals(originalKey, StringComparison.OrdinalIgnoreCase))
                    {
                        alreadyContained.Add(metaToLower);
                        searchKeys.Add(new BooleanLeafFieldDefinitions()
                        {
                            Field = metaToLower,
                            ValueType = valueType,
                            FieldType = eFieldType.Tag
                        });
                        break;
                    }
                }

                foreach (var meta in group.m_oEpgGroupSettings.m_lMetasName)
                {
                    string metaToLower = meta.ToLower();
                    if (!alreadyContained.Contains(metaToLower) &&
                        meta.Equals(originalKey, StringComparison.OrdinalIgnoreCase))
                    {
                        alreadyContained.Add(metaToLower);
                        searchKeys.Add(new BooleanLeafFieldDefinitions()
                        {
                            Field = metaToLower,
                            ValueType = valueType,
                            FieldType = eFieldType.StringMeta
                        });
                        break;
                    }
                }
            }

            #endregion

            if (searchKeys.Count == 0)
            {
                searchKeys.Add(new BooleanLeafFieldDefinitions()
                {
                    Field = originalKey.ToLower(),
                    ValueType = valueType,
                    FieldType = eFieldType.Default
                });
            }

            return searchKeys;
        }

        private static void GetMetaType(string meta, out Type type)
        {
            type = typeof(string);

            if (meta.Contains(META_BOOL_SUFFIX))
            {
                type = typeof(int);
            }
            else if (meta.Contains(META_DOUBLE_SUFFIX))
            {
                type = typeof(double);
            }
            else if (meta.StartsWith(META_DATE_PREFIX))
            {
                type = typeof(DateTime);
            }
        }

        /// <summary>
        /// For a given group, gets the media types definitions of parent relations and associated tags
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="parentMediaTypes"></param>
        /// <param name="associationTags"></param>
        /// <param name="relevantMediaTypes"></param>
        /// <param name="shouldGetAllMediaTypes"></param>
        /// <param name="groupManager"></param>
        internal static void GetParentMediaTypesAssociations(
            int groupId, out Dictionary<int, int> parentMediaTypes, out Dictionary<int, string> associationTags,
            List<int> relevantMediaTypes = null,
            bool shouldGetAllMediaTypes = false, GroupManager groupManager = null)
        {
            parentMediaTypes = new Dictionary<int, int>();
            associationTags = new Dictionary<int, string>();

            if (CatalogManager.Instance.DoesGroupUsesTemplates(groupId))
            {
                CatalogGroupCache catalogGroupCache;
                if (!CatalogManagement.CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId,
                    out catalogGroupCache))
                {
                    log.ErrorFormat(
                        "failed to get catalogGroupCache for groupId: {0} when calling GetParentMediaTypesAssociations",
                        groupId);
                    return;
                }

                parentMediaTypes = catalogGroupCache.AssetStructsMapById.Where(x => x.Value.ParentId.HasValue)
                    .ToDictionary(x => (int) x.Key, x => (int) x.Value.ParentId);
            }
            else
            {
                if (groupManager == null)
                {
                    groupManager = new GroupManager();
                }

                if (relevantMediaTypes == null)
                {
                    relevantMediaTypes = new List<int>();
                    shouldGetAllMediaTypes = true;
                }

                // Get media types of the group
                List<GroupsCacheManager.MediaType> groupMediaTypes = groupManager.GetMediaTypesOfGroup(groupId);

                foreach (var mediaType in groupMediaTypes)
                {
                    // Validate that this media type is defined for parent/association tag
                    if (mediaType.parentId > 0 && !string.IsNullOrEmpty(mediaType.associationTag))
                    {
                        // If this is relevant for the search at all
                        if (relevantMediaTypes.Contains(mediaType.parentId) ||
                            (relevantMediaTypes.Count == 0 && shouldGetAllMediaTypes))
                        {
                            parentMediaTypes.Add(mediaType.id, mediaType.parentId);
                            associationTags.Add(mediaType.id, mediaType.associationTag);
                        }
                    }
                }
            }
        }

        private static bool IsGroupHaveUserType(BaseMediaSearchRequest oMediaRequest)
        {
            try
            {
                return Utils.IsGroupIDContainedInConfig(oMediaRequest.m_nGroupID,
                    ApplicationConfiguration.Current.CatalogLogicConfiguration.GroupsWithIUserTypeSeperatedBySemiColon
                        .Value, ';');
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return false;
            }
        }


        #region Build Search object for Searcher project.

        static internal MediaSearchObj BuildSearchObject(BaseMediaSearchRequest request,
            List<string> jsonizedChannelsDefinitionsToSearchIn,
            List<string> jsonizedChannelsDefinitionsTheMediaShouldNotAppearIn)
        {
            // Build search object
            MediaSearchObj searchObj = new MediaSearchObj();
            try
            {
                //Build 2 CondList for search tags / metaStr / metaDobule .
                List<SearchValue> m_dAnd = new List<SearchValue>();
                List<SearchValue> m_dOr = new List<SearchValue>();

                OrderObj oSearcherOrderObj = new OrderObj();
                oSearcherOrderObj.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.CREATE_DATE;
                oSearcherOrderObj.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;

                SearchValue search = new SearchValue();
                if (request is MediaSearchFullRequest)
                {
                    // is it full search
                    FullSearchAddParams(request.m_nGroupID, ((MediaSearchFullRequest) request).m_AndList,
                        ((MediaSearchFullRequest) request).m_OrList, ref m_dAnd, ref m_dOr);
                    searchObj.m_sName = string.Empty;
                    searchObj.m_sDescription = string.Empty;
                }
                else if (request is MediaSearchRequest)
                {
                    // is it normal search
                    NormalSearchAddParams((MediaSearchRequest) request, ref m_dAnd, ref m_dOr);

                    searchObj.m_sName = ((MediaSearchRequest) request).m_sName;
                    searchObj.m_sDescription = ((MediaSearchRequest) request).m_sDescription;

                    if (((MediaSearchRequest) request).m_bAnd)
                        searchObj.m_eCutWith = CutWith.AND;
                    else
                        searchObj.m_eCutWith = CutWith.OR;

                    if (!string.IsNullOrEmpty(((MediaSearchRequest) request).m_sName) ||
                        !string.IsNullOrEmpty(((MediaSearchRequest) request).m_sDescription))
                    {
                        SearchObjectString(m_dAnd, m_dOr, ((MediaSearchRequest) request).m_sName,
                            ((MediaSearchRequest) request).m_sDescription, ((MediaSearchRequest) request).m_bAnd);
                    }
                }

                GetOrderValues(ref oSearcherOrderObj, request.m_oOrderObj);
                if (oSearcherOrderObj.m_eOrderBy == ApiObjects.SearchObjects.OrderBy.META &&
                    string.IsNullOrEmpty(oSearcherOrderObj.m_sOrderValue))
                {
                    oSearcherOrderObj.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.CREATE_DATE;
                    oSearcherOrderObj.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                }

                #region Build Searcher Search Object

                searchObj.m_bUseStartDate = request.m_oFilter.m_bUseStartDate;
                searchObj.m_bUseFinalEndDate = request.m_oFilter.m_bUseFinalDate;
                searchObj.m_nUserTypeID = request.m_oFilter.m_nUserTypeID;
                searchObj.m_bUseActive = request.m_oFilter.m_bOnlyActiveMedia;

                searchObj.m_nMediaID = request.m_nMediaID;
                if (request.m_nMediaTypes != null && request.m_nMediaTypes.Count > 0)
                {
                    searchObj.m_sMediaTypes =
                        string.Join(";", request.m_nMediaTypes.Select((i) => i.ToString()).ToArray());
                }
                else
                {
                    searchObj.m_sMediaTypes = "0";
                }

                searchObj.m_oOrder = new OrderObj();
                searchObj.m_oOrder.m_eOrderDir = oSearcherOrderObj.m_eOrderDir;
                searchObj.m_oOrder.m_eOrderBy = oSearcherOrderObj.m_eOrderBy;
                searchObj.m_oOrder.m_sOrderValue = oSearcherOrderObj.m_sOrderValue;
                if (request.m_nMediaID > 0)
                {
                    searchObj.m_oOrder.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.RELATED;
                }

                if (searchObj.m_oOrder.m_eOrderBy == ApiObjects.SearchObjects.OrderBy.META)
                {
                    if (CatalogManagement.CatalogManager.Instance.DoesGroupUsesTemplates(request.m_nGroupID))
                    {
                        if (!CatalogManagement.CatalogManager.Instance.CheckMetaExists(request.m_nGroupID,
                            searchObj.m_oOrder.m_sOrderValue.ToLower()))
                        {
                            //return error - meta not erxsits
                            log.ErrorFormat(
                                "meta not exsits for group -  unified search definitions. groupId = {0}, meta name = {1}",
                                request.m_nGroupID, searchObj.m_oOrder.m_sOrderValue);
                            throw new Exception(string.Format(
                                "meta not exsits for group -  unified search definitions. groupId = {0}, meta name = {1}",
                                request.m_nGroupID, searchObj.m_oOrder.m_sOrderValue));
                        }
                    }
                    else
                    {
                        GroupManager groupManager = new GroupManager();
                        Group group = groupManager.GetGroup(request.m_nGroupID);
                        if (!Utils.CheckMetaExsits(false, true, false, group,
                            searchObj.m_oOrder.m_sOrderValue.ToLower()))
                        {
                            //return error - meta not erxsits
                            log.ErrorFormat(
                                "meta not exsits for group -  unified search definitions. groupId = {0}, meta name = {1}",
                                request.m_nGroupID, searchObj.m_oOrder.m_sOrderValue);
                            throw new Exception(string.Format(
                                "meta not exsits for group -  unified search definitions. groupId = {0}, meta name = {1}",
                                request.m_nGroupID, searchObj.m_oOrder.m_sOrderValue));
                        }
                    }
                }

                if (request.m_oFilter != null)
                    searchObj.m_nDeviceRuleId = Api.api
                        .GetDeviceAllowedRuleIDs(request.m_nGroupID, request.m_oFilter.m_sDeviceId, request.domainId)
                        .ToArray();

                if (m_dOr.Count > 0)
                {
                    if (CatalogManagement.CatalogManager.Instance.DoesGroupUsesTemplates(request.m_nGroupID))
                    {
                        searchObj.m_nGroupId = request.m_nGroupID;
                        CopySearchValuesToSearchObjects(ref searchObj, CutWith.OR, m_dOr);
                    }
                    else
                    {
                        searchObj.m_dOr = m_dOr;
                    }
                }

                if (m_dAnd.Count > 0)
                {
                    if (CatalogManagement.CatalogManager.Instance.DoesGroupUsesTemplates(request.m_nGroupID))
                    {
                        searchObj.m_nGroupId = request.m_nGroupID;
                        CopySearchValuesToSearchObjects(ref searchObj, CutWith.AND, m_dAnd);
                    }
                    else
                    {
                        searchObj.m_dAnd = m_dAnd;
                    }
                }

                searchObj.m_bExact = request.m_bExact;

                searchObj.m_sPermittedWatchRules = GetPermittedWatchRules(request.m_nGroupID);

                searchObj.m_lChannelsDefinitionsMediaNeedsToBeInAtLeastOneOfIt = jsonizedChannelsDefinitionsToSearchIn;
                searchObj.m_lOrMediaNotInAnyOfTheseChannelsDefinitions =
                    jsonizedChannelsDefinitionsTheMediaShouldNotAppearIn;

                #endregion

                List<int> regionIds;
                List<string> linearMediaTypes;
                bool doesGroupUsesTemplates =
                    CatalogManagement.CatalogManager.Instance.DoesGroupUsesTemplates(request.m_nGroupID);

                CatalogLogic.SetSearchRegions(request.m_nGroupID, doesGroupUsesTemplates, request.domainId,
                    request.m_sSiteGuid, out regionIds, out linearMediaTypes);

                searchObj.regionIds = regionIds;
                searchObj.linearChannelMediaTypes = linearMediaTypes;

                CatalogLogic.GetParentMediaTypesAssociations(request.m_nGroupID, out searchObj.parentMediaTypes,
                    out searchObj.associationTags);
            }
            catch (Exception ex)
            {
                log.Error("Filed Build Search Object For Searcher Search", ex);
            }

            return searchObj;
        }

        internal static MediaSearchObj BuildSearchObject(BaseMediaSearchRequest request)
        {
            return BuildSearchObject(request, null, null);
        }

        internal static MediaSearchObj BuildSearchObject(BaseMediaSearchRequest request,
            List<string> jsonizedChannelsDefinitionsToSearchIn)
        {
            return BuildSearchObject(request, jsonizedChannelsDefinitionsToSearchIn, null);
        }

        /// <summary>
        /// Returns list of regions to perform search by them.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="domainId"></param>
        /// <param name="siteGuid"></param>
        /// <returns></returns>
        internal static void SetSearchRegions(int groupId, bool isOPC, int domainId, string siteGuid,
            out List<int> regionIds, out List<string> linearMediaTypes)
        {
            regionIds = null;
            linearMediaTypes = new List<string>();
            bool isRegionalizationEnabled = false;
            int defaultRegion = 0;

            if (isOPC)
            {
                CatalogGroupCache catalogGroupCache;
                if (CatalogManagement.CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId,
                    out catalogGroupCache) && catalogGroupCache.IsRegionalizationEnabled)
                {
                    isRegionalizationEnabled = catalogGroupCache.IsRegionalizationEnabled;
                    defaultRegion = catalogGroupCache.DefaultRegion;

                    linearMediaTypes.AddRange(catalogGroupCache.AssetStructsMapById.Values
                        .Where(v => v.IsLinearAssetStruct).Select(a => a.Id.ToString()));
                }
            }
            else
            {
                GroupManager groupManager = new GroupManager();
                Group group = groupManager.GetGroup(groupId);

                if (group != null && group.isRegionalizationEnabled)
                {
                    isRegionalizationEnabled = true;
                    defaultRegion = group.defaultRegion;
                    Dictionary<string, string> dictionary = CatalogLogic.GetLinearMediaTypeIDsAndWatchRuleIDs(groupId);

                    if (dictionary.ContainsKey(CatalogLogic.LINEAR_MEDIA_TYPES_KEY))
                    {
                        // Split by semicolon
                        var mediaTypesArray = dictionary[CatalogLogic.LINEAR_MEDIA_TYPES_KEY].Split(';');

                        // Convert to list
                        linearMediaTypes.AddRange(mediaTypesArray);
                    }
                }
            }

            // If this group has regionalization enabled at all
            if (isRegionalizationEnabled)
            {
                var regionId = GetRegionIdOfUser(groupId, domainId, siteGuid, defaultRegion);
                // Get the region of the requesting domain
                if (regionId == -1 && defaultRegion != 0)
                {
                    regionId = defaultRegion;
                }

                if (regionId > -1)
                {
                    Region region = ApiLogic.Api.Managers.RegionManager.GetRegion(groupId, regionId);
                    if (region != null)
                    {
                        regionIds = new List<int> { regionId };
                        if (region.parentId > 0)
                        {
                            regionIds.Add(region.parentId);
                        }
                    }
                }
            }
        }

        public static int GetRegionIdOfUser(int groupId, int domainId, string siteGuid, int defaultRegion = -1)
        {
            int regionId = -1;
            bool isRegionalizationEnabled = false;

            if (defaultRegion > -1)
            {
                isRegionalizationEnabled = true;
            }
            else
            {
                if (CatalogManager.Instance.DoesGroupUsesTemplates(groupId))
                {
                    if (CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId,
                        out CatalogGroupCache catalogGroupCache) && catalogGroupCache.IsRegionalizationEnabled)
                    {
                        isRegionalizationEnabled = true;
                        defaultRegion = catalogGroupCache.DefaultRegion;
                    }
                }
                else
                {
                    var group = GroupsCache.Instance().GetGroup(groupId);
                    if (group != null && group.isRegionalizationEnabled)
                    {
                        isRegionalizationEnabled = true;
                        defaultRegion = group.defaultRegion;
                    }
                }
            }

            if (isRegionalizationEnabled)
            {
                regionId = RequestContextUtilsInstance.Get().GetRegionId() ?? GetRegionByDomain(groupId, domainId, defaultRegion);
            }

            return regionId;
        }

        // Old standard - only if regionId from KS not working
        private static int GetRegionByDomain(int groupId, int domainId, int defaultRegion)
        {
            var domainRes = Domains.Module.GetDomainInfo(groupId, domainId);
            if (domainRes != null && domainRes.Status.IsOkStatusCode())
            {
                // If the domain is not associated to a region - get default region
                if (domainRes.Domain.m_nRegion > 0)
                {
                    return domainRes.Domain.m_nRegion;
                }
                else
                {
                    return defaultRegion;
                }
            }

            return -1;
        }

        /*Build Full search object*/
        static internal void FullSearchAddParams(int groupId, List<KeyValue> originalAnds, List<KeyValue> originalOrs,
            ref List<SearchValue> resultAnds, ref List<SearchValue> resultOrs)
        {
            CatalogCache catalogCache = CatalogCache.Instance();
            int nParentGroupID = catalogCache.GetParentGroup(groupId);

            GroupManager groupManager = new GroupManager();
            Group group = groupManager.GetGroup(nParentGroupID);

            if (group != null)
            {
                string searchKey;
                if (originalAnds != null)
                {
                    foreach (KeyValue andKeyValue in originalAnds)
                    {
                        searchKey = GetFullSearchKey(andKeyValue.m_sKey, ref group, out _,
                            out var type); // returns search key with prefix e.g. metas.{key}

                        SearchValue search = new SearchValue();
                        search.m_sKey = searchKey;
                        search.m_lValue = new List<string> {andKeyValue.m_sValue};
                        search.m_sValue = andKeyValue.m_sValue;
                        search.fieldType = type;
                        resultAnds.Add(search);
                    }
                }

                if (originalOrs != null)
                {
                    foreach (KeyValue orKeyValue in originalOrs)
                    {
                        SearchValue search = new SearchValue();

                        searchKey = GetFullSearchKey(orKeyValue.m_sKey, ref group, out _,
                            out var type); // returns search key with prefix e.g. metas.{key}
                        search.m_sKey = searchKey;
                        search.m_lValue = new List<string> {orKeyValue.m_sValue};
                        search.m_sValue = orKeyValue.m_sValue;
                        search.fieldType = type;
                        resultOrs.Add(search);
                    }
                }
            }
        }

        /*Build Normal search object*/
        static internal void NormalSearchAddParams(MediaSearchRequest request, ref List<SearchValue> m_dAnd,
            ref List<SearchValue> m_dOr)
        {
            SearchValue search = new SearchValue();
            if (request.m_lMetas != null && request.m_lMetas.Count > 0)
            {
                foreach (KeyValue meta in request.m_lMetas)
                {
                    if (meta.m_sValue != null)
                    {
                        string[] valueArr = meta.m_sValue.Split(';');
                        foreach (string metaVar in valueArr)
                        {
                            search = new SearchValue();
                            search.m_sKey = meta.m_sKey;
                            if (!string.IsNullOrEmpty(metaVar))
                            {
                                search.m_sValue = metaVar.ToLower();
                            }

                            search.m_sKeyPrefix = METAS;
                            //// ADDED
                            search.m_lValue = new List<string>() {metaVar};

                            if (!string.IsNullOrEmpty(search.m_sKey) && !string.IsNullOrEmpty(metaVar))
                            {
                                if (request.m_bAnd)
                                {
                                    m_dAnd.Add(search);
                                }
                                else
                                {
                                    m_dOr.Add(search);
                                }
                            }
                        }
                    }
                }
            }

            if (request.m_lTags != null && request.m_lTags.Count > 0)
            {
                foreach (KeyValue tags in request.m_lTags)
                {
                    search = new SearchValue();
                    search.m_sKey = tags.m_sKey;
                    if (!string.IsNullOrEmpty(tags.m_sValue))
                    {
                        search.m_sValue = tags.m_sValue.ToLower();
                    }

                    search.m_sKeyPrefix = TAGS;

                    //// ADDED
                    search.m_lValue = new List<string>() {tags.m_sValue};

                    if (!string.IsNullOrEmpty(search.m_sKey) && !string.IsNullOrEmpty(search.m_sValue))
                    {
                        if (request.m_bAnd)
                        {
                            m_dAnd.Add(search);
                        }
                        else
                        {
                            m_dOr.Add(search);
                        }
                    }
                }
            }
        }

        /*Build Order Object for Searcher*/
        /// <summary>
        /// This function builds Searcher Order Object
        /// </summary>
        /// <param name="OrderObj">The new Searcher Order Object</param>
        /// <param name="oOrderObj">Catalog Order Object</param>
        public static void GetOrderValues(ref ApiObjects.SearchObjects.OrderObj oSearchOrderObj,
            ApiObjects.SearchObjects.OrderObj oOrderObj)
        {
            try
            {
                if (oOrderObj != null)
                {
                    oSearchOrderObj.m_bIsSlidingWindowField = oOrderObj.m_bIsSlidingWindowField;
                    oSearchOrderObj.lu_min_period_id = oOrderObj.lu_min_period_id;

                    switch (oOrderObj.m_eOrderDir)
                    {
                        case ApiObjects.SearchObjects.OrderDir.ASC:
                            oSearchOrderObj.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC;
                            break;
                        default:
                            oSearchOrderObj.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                            break;
                    }

                    switch (oOrderObj.m_eOrderBy)
                    {
                        case ApiObjects.SearchObjects.OrderBy.ID:
                            oSearchOrderObj.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.ID;
                            break;
                        case ApiObjects.SearchObjects.OrderBy.VIEWS:
                            oSearchOrderObj.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.VIEWS;
                            break;
                        case ApiObjects.SearchObjects.OrderBy.RATING:
                            oSearchOrderObj.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.RATING;
                            break;
                        case ApiObjects.SearchObjects.OrderBy.VOTES_COUNT:
                            oSearchOrderObj.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.VOTES_COUNT;
                            break;
                        case ApiObjects.SearchObjects.OrderBy.LIKE_COUNTER:
                            oSearchOrderObj.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.LIKE_COUNTER;
                            break;
                        case ApiObjects.SearchObjects.OrderBy.START_DATE:
                            oSearchOrderObj.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.START_DATE;
                            break;
                        case ApiObjects.SearchObjects.OrderBy.NAME:
                            oSearchOrderObj.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.NAME;
                            break;
                        case ApiObjects.SearchObjects.OrderBy.CREATE_DATE:
                            oSearchOrderObj.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.CREATE_DATE;
                            break;
                        case ApiObjects.SearchObjects.OrderBy.META:
                            oSearchOrderObj.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.META;
                            oSearchOrderObj.m_sOrderValue = oOrderObj.m_sOrderValue;
                            break;
                        case ApiObjects.SearchObjects.OrderBy.RELATED:
                            oSearchOrderObj.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.RELATED;
                            break;
                        case ApiObjects.SearchObjects.OrderBy.UPDATE_DATE:
                            oSearchOrderObj.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.UPDATE_DATE;
                            oSearchOrderObj.m_sOrderValue = oOrderObj.m_sOrderValue;
                            break;
                        case ApiObjects.SearchObjects.OrderBy.MEDIA_ID:
                            oSearchOrderObj.m_eOrderBy = OrderBy.MEDIA_ID;
                            break;
                        case ApiObjects.SearchObjects.OrderBy.EPG_ID:
                            oSearchOrderObj.m_eOrderBy = OrderBy.EPG_ID;
                            break;
                        default:
                            oSearchOrderObj.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.START_DATE;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Catalog.GetOrderValues", ex);
            }
        }

        /*Reference method to build search object for Searcher*/
        private static void SearchObjectString(List<SearchValue> m_dAnd, List<SearchValue> m_dOr, string sName,
            string sDescription, bool bAnd)
        {
            SearchValue searchValue;
            try
            {
                if (!String.IsNullOrEmpty(sName))
                {
                    searchValue = new SearchValue();
                    searchValue.m_sKey = "name";
                    searchValue.m_sValue = sName;
                    searchValue.m_lValue = new List<string>() {sName};

                    if (bAnd)
                    {
                        m_dAnd.Add(searchValue);
                    }
                    else
                    {
                        m_dOr.Add(searchValue);
                    }
                }

                if (!String.IsNullOrEmpty(sDescription))
                {
                    searchValue = new SearchValue();
                    searchValue.m_sKey = "description";
                    searchValue.m_sValue = sDescription;
                    searchValue.m_lValue = new List<string>() {sDescription};
                    if (bAnd)
                    {
                        m_dAnd.Add(searchValue);
                    }
                    else
                    {
                        m_dOr.Add(searchValue);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("SearchObjectString", ex);
            }
        }

        #endregion

        #region Build search Object for search Related

        /*Build the right MediaSearchRequest for a Search Related Media */
        public static MediaSearchRequest BuildMediasRequest(
            Int32 nMediaID,
            bool bIsMainLang,
            Filter filterRequest,
            ref Filter oFilter,
            Int32 nGroupID,
            List<Int32> nMediaTypes,
            string sSiteGuid,
            bool doesGroupUsesTemplates,
            CatalogGroupCache catalogGroupCache)
        {
            try
            {
                oFilter = filterRequest;
                MediaSearchRequest oMediasRequest = new MediaSearchRequest();
                oMediasRequest.m_nMediaTypes = new List<int>();
                if (nMediaTypes?.Count > 0)
                {
                    oMediasRequest.m_nMediaTypes.AddRange(nMediaTypes);
                }

                int languageId = 0;
                if (filterRequest != null)
                {
                    languageId = filterRequest.m_nLanguage;
                    oFilter.m_sDeviceId = filterRequest.m_sDeviceId;
                }

                oMediasRequest.m_nGroupID = nGroupID;
                oMediasRequest.m_sSiteGuid = sSiteGuid;

                if (doesGroupUsesTemplates)
                {
                    return BuildSearchRelatedRequestForOpcAccount(ref oMediasRequest, nGroupID, nMediaID, bIsMainLang,
                        languageId, catalogGroupCache);
                }
                else
                {
                    GroupManager groupManager = new GroupManager();

                    CatalogCache catalogCache = CatalogCache.Instance();
                    int nParentGroupID = catalogCache.GetParentGroup(nGroupID);

                    List<int> lSubGroupTree = groupManager.GetSubGroup(nParentGroupID);
                    DataSet ds = CatalogDAL.Build_MediaRelated(nGroupID, nMediaID, languageId, lSubGroupTree);

                    if (ds == null)
                        return null;

                    if (ds.Tables.Count == 4)
                    {
                        if (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0) // basic details
                        {
                            oMediasRequest.m_sName = Utils.GetStrSafeVal(ds.Tables[1].Rows[0], "NAME");
                            if (oMediasRequest.m_nMediaTypes.Count == 0)
                            {
                                if (ds.Tables[1].Rows[0]["MEDIA_TYPE_ID"] != DBNull.Value &&
                                    !string.IsNullOrEmpty(ds.Tables[1].Rows[0]["MEDIA_TYPE_ID"].ToString()))
                                    oMediasRequest.m_nMediaTypes.Add(Utils.GetIntSafeVal(ds.Tables[1].Rows[0],
                                        "MEDIA_TYPE_ID"));
                            }
                        }
                        else
                        {
                            return null;
                        }

                        if (ds.Tables[2] != null) //TAGS
                        {
                            oMediasRequest.m_lTags = BuildTagsForSearch(bIsMainLang, ds.Tables[2]);
                        }

                        if (ds.Tables[3] != null) //META
                        {
                            oMediasRequest.m_lMetas = BuildMetasForSearch(ds.Tables[3]);
                        }
                    }
                }

                return oMediasRequest;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return null;
            }
        }

        public static MediaSearchRequest BuildSearchRelatedRequestForOpcAccount(
            ref MediaSearchRequest mediaSearchRequest, int groupId, long mediaId, bool isMainLanguage, int languageId,
            CatalogGroupCache catalogGroupCache)
        {
            try
            {
                HashSet<string> metasToIgnore = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
                metasToIgnore.Add(CatalogManagement.AssetManager.CATALOG_START_DATE_TIME_META_SYSTEM_NAME);
                metasToIgnore.Add(CatalogManagement.AssetManager.PLAYBACK_END_DATE_TIME_META_SYSTEM_NAME);
                GenericResponse<Asset> assetResponse =
                    AssetManager.Instance.GetAsset(groupId, mediaId, eAssetTypes.MEDIA, false);
                if (assetResponse == null || assetResponse.Status == null ||
                    assetResponse.Status.Code != (int) eResponseStatus.OK || assetResponse.Object == null)
                {
                    return null;
                }

                var mediaAsset = assetResponse.Object as MediaAsset;
                if (mediaAsset == null)
                {
                    return null;
                }

                mediaSearchRequest.m_sName = mediaAsset.Name;
                mediaSearchRequest.m_lMetas = new List<KeyValue>();
                // metas that are related and shouldn't be ignored
                if (mediaAsset.Metas != null && mediaAsset.Metas.Count > 0 && mediaAsset.Metas.Any(x =>
                    !metasToIgnore.Contains(x.m_oTagMeta.m_sName)
                    && catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(x.m_oTagMeta.m_sName)
                    && catalogGroupCache.TopicsMapBySystemNameAndByType[x.m_oTagMeta.m_sName]
                        .ContainsKey(x.m_oTagMeta.m_sType)
                    && catalogGroupCache.TopicsMapBySystemNameAndByType[x.m_oTagMeta.m_sName][x.m_oTagMeta.m_sType]
                        .SearchRelated))
                {
                    mediaSearchRequest.m_lMetas.AddRange(mediaAsset.Metas.Where(x =>
                            !metasToIgnore.Contains(x.m_oTagMeta.m_sName)
                            && catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(x.m_oTagMeta.m_sName)
                            && catalogGroupCache.TopicsMapBySystemNameAndByType[x.m_oTagMeta.m_sName]
                                .ContainsKey(x.m_oTagMeta.m_sType)
                            && catalogGroupCache.TopicsMapBySystemNameAndByType[x.m_oTagMeta.m_sName][
                                x.m_oTagMeta.m_sType].SearchRelated)
                        .Select(x => new KeyValue(x.m_oTagMeta.m_sName, x.m_sValue)).ToList());
                }

                mediaSearchRequest.m_lTags = new List<KeyValue>();
                // tags that are related
                if (mediaAsset.Tags != null && mediaAsset.Tags.Count > 0 && mediaAsset.Tags.Any(x =>
                    catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(x.m_oTagMeta.m_sName)
                    && catalogGroupCache.TopicsMapBySystemNameAndByType[x.m_oTagMeta.m_sName]
                        .ContainsKey(x.m_oTagMeta.m_sType)
                    && catalogGroupCache.TopicsMapBySystemNameAndByType[x.m_oTagMeta.m_sName][x.m_oTagMeta.m_sType]
                        .SearchRelated))
                {
                    List<Tags> mediaRelatedAssetTags = mediaAsset.Tags.Where(x =>
                        !metasToIgnore.Contains(x.m_oTagMeta.m_sName)
                        && catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(x.m_oTagMeta.m_sName)
                        && catalogGroupCache.TopicsMapBySystemNameAndByType[x.m_oTagMeta.m_sName]
                            .ContainsKey(x.m_oTagMeta.m_sType)
                        && catalogGroupCache.TopicsMapBySystemNameAndByType[x.m_oTagMeta.m_sName][x.m_oTagMeta.m_sType]
                            .SearchRelated).ToList();
                    if (mediaRelatedAssetTags != null && mediaRelatedAssetTags.Count > 0)
                    {
                        foreach (Tags relatedTag in mediaRelatedAssetTags)
                        {
                            string key = relatedTag.m_oTagMeta.m_sName;
                            foreach (string tagValue in relatedTag.m_lValues)
                            {
                                mediaSearchRequest.m_lTags.Add(new KeyValue(relatedTag.m_oTagMeta.m_sName, tagValue));
                            }
                        }
                    }
                }

                if (!isMainLanguage && languageId > 0)
                {
                    string languageCode = catalogGroupCache.LanguageMapById[languageId].Code;
                    if (mediaAsset.NamesWithLanguages.Any(x => x.m_sLanguageCode3 == languageCode))
                    {
                        mediaSearchRequest.m_sName = mediaAsset.NamesWithLanguages
                            .Where(x => x.m_sLanguageCode3 == languageCode).First().m_sValue;
                    }

                    foreach (KeyValue metas in mediaSearchRequest.m_lMetas)
                    {
                        if (mediaAsset.Metas.Any(x =>
                            x.m_oTagMeta.m_sName == metas.m_sKey && x.Value != null &&
                            x.Value.Any(y => y.m_sLanguageCode3 == languageCode)))
                        {
                            metas.m_sValue = mediaAsset.Metas.Where(x => x.m_oTagMeta.m_sName == metas.m_sKey).First()
                                .Value.Where(y => y.m_sLanguageCode3 == languageCode).First().m_sValue;
                        }
                    }

                    List<KeyValue> translatedTags = new List<KeyValue>();
                    foreach (string tagName in mediaSearchRequest.m_lTags.Select(x => x.m_sKey).Distinct())
                    {
                        if (mediaAsset.Tags.Any(x =>
                            x.m_oTagMeta.m_sName == tagName && x.Values != null &&
                            x.Values.Any(y => y.Any(z => z.m_sLanguageCode3 == languageCode))))
                        {
                            List<Tags> mediaAssetTranslatedTags = mediaAsset.Tags.Where(x =>
                                x.m_oTagMeta.m_sName == tagName && x.Values != null &&
                                x.Values.Any(y => y.Any(z => z.m_sLanguageCode3 == languageCode))).ToList();
                            foreach (Tags tagsTranslation in mediaAssetTranslatedTags)
                            {
                                if (tagsTranslation.Values != null && tagsTranslation.Values.Count > 0)
                                {
                                    foreach (LanguageContainer[] langContainerArray in tagsTranslation.Values)
                                    {
                                        if (langContainerArray != null &&
                                            langContainerArray.Any(x => x.m_sLanguageCode3 == languageCode))
                                        {
                                            translatedTags.AddRange(langContainerArray
                                                .Where(x => x.m_sLanguageCode3 == languageCode)
                                                .Select(y => new KeyValue(tagName, y.m_sValue)).ToList());
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (translatedTags.Count > 0)
                    {
                        mediaSearchRequest.m_lTags.Clear();
                        mediaSearchRequest.m_lTags.AddRange(translatedTags);
                    }
                }

                if (mediaSearchRequest.m_nMediaTypes.Count == 0)
                {
                    mediaSearchRequest.m_nMediaTypes.Add(mediaAsset.MediaType.m_nTypeID);
                }

                return mediaSearchRequest;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return null;
            }
        }

        /*Build tags for search related protocol*/
        private static List<KeyValue> BuildTagsForSearch(bool bIsMainLang, DataTable dtTags)
        {
            try
            {
                if (dtTags.Rows == null)
                    return null;

                string sKey;
                string sValue;
                int nTagID;
                List<KeyValue> lTags = new List<KeyValue>();

                for (int i = 0; i < dtTags.Rows.Count; i++)
                {
                    sKey = Utils.GetStrSafeVal(dtTags.Rows[i], "NAME");
                    nTagID = Utils.GetIntSafeVal(dtTags.Rows[i], "tagID");
                    if (bIsMainLang)
                        sValue = Utils.GetStrSafeVal(dtTags.Rows[i], "tagValue");
                    else
                        sValue = Utils.GetStrSafeVal(dtTags.Rows[i], "tagTranslate");
                    lTags.Add(new KeyValue(sKey, sValue));
                }

                return lTags;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return null;
            }
        }

        /*Build metas for search related protocol*/
        private static List<KeyValue> BuildMetasForSearch(DataTable dtMeta)
        {
            try
            {
                string sFieldName;
                string sFieldVal;
                string sFieldRelated;
                string sName;
                string sValue;
                int nIsRelated;

                List<KeyValue> lMetas = new List<KeyValue>();

                #region meta str

                for (int i = 1; i < 21; i++)
                {
                    sFieldName = "META" + i.ToString() + "_STR_NAME";
                    sFieldVal = "META" + i.ToString() + "_STR";
                    sFieldRelated = "IS_META" + i.ToString() + "_STR_RELATED";

                    if (dtMeta.Rows[0][sFieldName] != DBNull.Value)
                    {
                        sName = Utils.GetStrSafeVal(dtMeta.Rows[0], sFieldName);
                        nIsRelated = Utils.GetIntSafeVal(dtMeta.Rows[0], sFieldRelated);
                        if (!string.IsNullOrEmpty(sName) && nIsRelated != 0)
                        {
                            if (dtMeta.Rows[0][sFieldVal] != DBNull.Value &&
                                !string.IsNullOrEmpty(dtMeta.Rows[0][sFieldVal].ToString()))
                            {
                                sValue = dtMeta.Rows[0][sFieldVal].ToString();
                                lMetas.Add(new KeyValue(sName, sValue));
                            }
                        }
                    }
                }

                #endregion

                #region meta double

                for (int i = 1; i < 11; i++)
                {
                    sFieldName = "META" + i.ToString() + "_DOUBLE_NAME";
                    sFieldVal = "META" + i.ToString() + "_DOUBLE";
                    sFieldRelated = "IS_META" + i.ToString() + "_DOUBLE_RELATED";
                    if (dtMeta.Rows[0][sFieldName] != DBNull.Value)
                    {
                        sName = dtMeta.Rows[0][sFieldName].ToString();
                        nIsRelated = Utils.GetIntSafeVal(dtMeta.Rows[0], sFieldRelated);
                        if (!string.IsNullOrEmpty(sName) && nIsRelated != 0)
                        {
                            if (dtMeta.Rows[0][sFieldVal] != DBNull.Value &&
                                !string.IsNullOrEmpty(dtMeta.Rows[0][sFieldVal].ToString()))
                            {
                                sValue = dtMeta.Rows[0][sFieldVal].ToString();
                                lMetas.Add(new KeyValue(sName, sValue));
                            }
                        }
                    }
                }

                #endregion

                #region meta bool

                for (int i = 1; i < 11; i++)
                {
                    sFieldName = "META" + i.ToString() + "_BOOL_NAME";
                    sFieldVal = "META" + i.ToString() + "_BOOL";
                    sFieldRelated = "IS_META" + i.ToString() + "_BOOL_RELATED";
                    if (dtMeta.Rows[0][sFieldName] != DBNull.Value)
                    {
                        sName = dtMeta.Rows[0][sFieldName].ToString();
                        nIsRelated = Utils.GetIntSafeVal(dtMeta.Rows[0], sFieldRelated);
                        if (!string.IsNullOrEmpty(sName) && nIsRelated != 0)
                        {
                            if (dtMeta.Rows[0][sFieldVal] != DBNull.Value &&
                                !string.IsNullOrEmpty(dtMeta.Rows[0][sFieldVal].ToString()))
                            {
                                sValue = dtMeta.Rows[0][sFieldVal].ToString();
                                lMetas.Add(new KeyValue(sName, sValue));
                            }
                        }
                    }
                }

                #endregion

                return lMetas;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return null;
            }
        }

        #endregion

        #region Media play processing (media mrak, media hit)

        public static void GetMediaPlayData(int nMediaID, int nMediaFileID, ref int nOwnerGroupID, ref int nCDNID,
            ref int nQualityID, ref int nFormatID, ref int nBillingTypeID, ref int nMediaTypeID)
        {
            DataTable dtPlayData = CatalogDAL.Get_MediaPlayData(nMediaID, nMediaFileID);
            if (dtPlayData != null && dtPlayData.Rows != null && dtPlayData.Rows.Count > 0)
            {
                nOwnerGroupID = Utils.GetIntSafeVal(dtPlayData.Rows[0], "group_id");
                nCDNID = Utils.GetIntSafeVal(dtPlayData.Rows[0], "streaming_suplier_id");
                nQualityID = Utils.GetIntSafeVal(dtPlayData.Rows[0], "media_quality_id");
                nFormatID = Utils.GetIntSafeVal(dtPlayData.Rows[0], "media_file_type_id");
                nMediaTypeID = Utils.GetIntSafeVal(dtPlayData.Rows[0], "media_type_id");
                nBillingTypeID = Utils.GetIntSafeVal(dtPlayData.Rows[0], "billing_type_id");
            }
        }

        public static void UpdateFollowMe(DevicePlayData devicePlayData, int groupId, int locationSec, int duration,
            MediaPlayActions action, eExpirationTTL ttl,
            bool isReportingMode, int mediaTypeId, bool isFirstPlay = false, bool isLinearChannel = false)
        {
            if (devicePlayData.UserId == 0)
            {
                return;
            }

            if (devicePlayData.DomainId < 1)
            {
                DomainSuspentionStatus eSuspendStat = DomainSuspentionStatus.OK;
                int opID = 0;
                bool isMaster = false;
                devicePlayData.DomainId = DomainDal.GetDomainIDBySiteGuid(groupId, devicePlayData.UserId, ref opID,
                    ref isMaster, ref eSuspendStat);
            }

            if (devicePlayData.DomainId > 0)
            {
                if (!isReportingMode && devicePlayData.DeviceFamilyId == 0)
                {
                    devicePlayData.DeviceFamilyId =
                        Api.api.Instance.GetDeviceFamilyIdByUdid(devicePlayData.DomainId, groupId, devicePlayData.UDID);
                }

                ePlayType playType = devicePlayData.GetPlayType();

                // UpdateOrInsertOrDeleteDevicePlayData
                if (!isReportingMode && (playType == ePlayType.MEDIA || playType == ePlayType.NPVR ||
                                         playType == ePlayType.EPG))
                {
                    devicePlayData.AssetAction = action.ToString();
                    devicePlayData.TimeStamp = DateTime.UtcNow.ToUtcUnixTimestampSeconds();
                    devicePlayData.CreatedAt = isFirstPlay ? devicePlayData.TimeStamp : devicePlayData.CreatedAt;

                    if (action == MediaPlayActions.STOP || action == MediaPlayActions.FINISH)
                    {
                        CatalogDAL.DeleteDevicePlayData(devicePlayData.UDID);
                    }
                    else
                    {
                        // get partner configuration for ttl.
                        uint expirationTTL = ConcurrencyManager.GetDevicePlayDataExpirationTTL(groupId, ttl);

                        CatalogDAL.UpdateOrInsertDevicePlayData(devicePlayData, isReportingMode, expirationTTL);
                    }
                }

                var assetType = eAssetTypes.MEDIA;
                if (playType == ePlayType.EPG)
                {
                    assetType = eAssetTypes.EPG;
                    mediaTypeId = (int) eAssetTypes.EPG;
                }
                else if (playType == ePlayType.NPVR)
                {
                    assetType = eAssetTypes.NPVR;
                    mediaTypeId = (int) eAssetTypes.NPVR;
                }

                var userMediaMark =
                    devicePlayData.ConvertToUserMediaMark(locationSec, duration, mediaTypeId, assetType);
                if (isFirstPlay)
                {
                    CatalogManager.Instance.SetHistoryValues(groupId, userMediaMark);
                }
                else
                {   // TODO could be removed (after we delete feature-toggle MediaMarkNewModel)
                    // because fixed in CatalogDal.InsertMediaMarkToUserMediaMarks, see `existingAssetLocation`
                    userMediaMark = CatalogDAL.GetUserMediaMark(userMediaMark);
                    userMediaMark.Location = locationSec;
                    userMediaMark.AssetAction = action.ToString();
                }


                switch (playType)
                {
                    case ePlayType.MEDIA:
                        CatalogDAL.UpdateOrInsertUsersMediaMark(userMediaMark, isFirstPlay, groupId);
                        break;
                    case ePlayType.NPVR:
                        CatalogDAL.UpdateOrInsertUsersNpvrMark(userMediaMark, isFirstPlay, groupId);
                        break;
                    case ePlayType.EPG:
                        CatalogDAL.UpdateOrInsertUsersEpgMark(userMediaMark, isFirstPlay, groupId);
                        break;
                    default:
                        break;
                }
            }
        }

        internal static int GetMediaActionID(string sAction)
        {
            MediaPlayActions action;

            int retActionID = -1;

            if (Enum.TryParse<MediaPlayActions>(sAction, true, out action))
                retActionID = (int) action;

            return retActionID;
        }

        internal static int GetMediaTypeID(int nMediaId)
        {
            int retTypeID = 0;

            retTypeID = CatalogDAL.Get_MediaTypeIdByMediaId(nMediaId);

            return retTypeID;
        }

        internal static string GetMediaPlayResponse(MediaPlayResponse response)
        {
            string retXml = string.Empty;

            switch (response)
            {
                case MediaPlayResponse.MEDIA_MARK:
                {
                    retXml = "media_mark";
                    break;
                }
                case MediaPlayResponse.CONCURRENT:
                {
                    retXml = "Concurrent";
                    break;
                }
                case MediaPlayResponse.HIT:
                {
                    retXml = "hit";
                    break;
                }
                case MediaPlayResponse.ACTION_NOT_RECOGNIZED:
                {
                    retXml = "Action not recognized";
                    break;
                }
                case MediaPlayResponse.OK:
                {
                    retXml = "OK";
                    break;
                }

                case MediaPlayResponse.ERROR:
                {
                    retXml = "Error";
                    break;
                }
            }

            return retXml;
        }

        #endregion

        internal static MediaSearchObj BuildBaseChannelSearchObject(GroupsCacheManager.Channel channel,
            BaseRequest request,
            OrderObj orderObj, int nParentGroupID, List<string> lPermittedWatchRules, int[] nDeviceRuleId,
            LanguageObj oLanguage)
        {
            MediaSearchObj searchObject = new MediaSearchObj
            {
                m_nGroupId = channel.m_nGroupID,
                m_nPageIndex = request.m_nPageIndex,
                m_nPageSize = request.m_nPageSize,
                m_bExact = true,
                m_eCutWith = channel.m_eCutWith,
                m_sMediaTypes = string.Join(";", channel.m_nMediaType)
            };

            if ((lPermittedWatchRules != null) && lPermittedWatchRules.Count > 0)
            {
                searchObject.m_sPermittedWatchRules = string.Join(" ", lPermittedWatchRules);
            }

            searchObject.m_nDeviceRuleId = nDeviceRuleId;
            searchObject.m_nIndexGroupId = nParentGroupID;
            searchObject.m_oLangauge = oLanguage;

            var oSearcherOrderObj = new OrderObj();

            if (orderObj != null && orderObj.m_eOrderBy != OrderBy.NONE)
            {
                GetOrderValues(ref oSearcherOrderObj, orderObj);
            }
            else
            {
                GetOrderValues(ref oSearcherOrderObj, channel.m_OrderObject);
            }

            searchObject.m_oOrder = oSearcherOrderObj;

            if (request.m_oFilter != null)
            {
                searchObject.m_bUseStartDate = request.m_oFilter.m_bUseStartDate;
                searchObject.m_bUseFinalEndDate = request.m_oFilter.m_bUseFinalDate;
                searchObject.m_nUserTypeID = request.m_oFilter.m_nUserTypeID;
                searchObject.m_bUseActive = request.m_oFilter.m_bOnlyActiveMedia;
            }

            CopySearchValuesToSearchObjects(ref searchObject, channel.m_eCutWith, channel.m_lChannelTags);

            List<int> regionIds;
            List<string> linearMediaTypes;
            bool doesGroupUsesTemplates =
                CatalogManagement.CatalogManager.Instance.DoesGroupUsesTemplates(request.m_nGroupID);

            CatalogLogic.SetSearchRegions(request.m_nGroupID, doesGroupUsesTemplates, request.domainId,
                request.m_sSiteGuid, out regionIds, out linearMediaTypes);

            searchObject.regionIds = regionIds;
            searchObject.linearChannelMediaTypes = linearMediaTypes;

            CatalogLogic.GetParentMediaTypesAssociations(request.m_nGroupID,
                out searchObject.parentMediaTypes, out searchObject.associationTags);

            return searchObject;
        }

        internal static void AddChannelMultiFiltersToSearchObject(ref MediaSearchObj searchObject,
            ChannelRequestMultiFiltering request)
        {
            // Taking care of additional filtering tags/metas given by the user
            if (request.m_lFilterTags != null && request.m_lFilterTags.Count > 0)
            {
                switch (request.m_eFilterCutWith)
                {
                    case CutWith.OR:
                        searchObject.m_eFilterTagsAndMetasCutWith = CutWith.OR;
                        break;
                    case CutWith.AND:
                        searchObject.m_eFilterTagsAndMetasCutWith = CutWith.AND;
                        break;
                }

                searchObject.m_lFilterTagsAndMetas =
                    ConvertKeyValuePairsToSearchValues(request.m_lFilterTags, request.m_nGroupID,
                        request.m_eFilterCutWith);
            }
        }

        private static List<SearchValue> ConvertKeyValuePairsToSearchValues(List<KeyValue> keyValues, int groupId,
            CutWith cutWith)
        {
            List<SearchValue> returnedSearchValues = null;

            if (keyValues != null && keyValues.Count > 0)
            {
                Group group = GroupsCache.Instance().GetGroup(groupId);

                returnedSearchValues = new List<SearchValue>();
                foreach (KeyValue keyValue in keyValues)
                {
                    if (!string.IsNullOrEmpty(keyValue.m_sKey) && !string.IsNullOrEmpty(keyValue.m_sValue))
                    {
                        bool isTagOrMeta = false;
                        var searchKeys = GetUnifiedSearchKey(keyValue.m_sKey, group, groupId);

                        isTagOrMeta = searchKeys.Count > 1 ||
                                      searchKeys.FirstOrDefault().FieldType != eFieldType.Default;
                        switch (cutWith)
                        {
                            case CutWith.OR:
                            {
                                foreach (var currentKey in searchKeys)
                                {
                                    SearchValue search = new SearchValue();
                                    search.m_sKey = currentKey.Field;
                                    search.m_lValue = new List<string> {keyValue.m_sValue};
                                    search.fieldType = currentKey.FieldType;
                                    returnedSearchValues.Add(search);
                                }

                                break;
                            }
                            case CutWith.AND:
                            {
                                SearchValue search = new SearchValue();

                                string searchKey = keyValue.m_sKey;

                                if (isTagOrMeta)
                                {
                                    searchKey = searchKeys.First().Field;
                                }

                                search.m_sKey = searchKey;
                                search.m_lValue = new List<string> {keyValue.m_sValue};

                                returnedSearchValues.Add(search);
                                break;
                            }
                            default:
                                break;
                        }
                    }
                }
            }

            return returnedSearchValues;
        }

        private static string GetPermittedWatchRules(int groupId, DataTable extractedPermittedWatchRulesDT)
        {
            List<string> watchPermissionRules = null;
            if (extractedPermittedWatchRulesDT == null)
            {
                watchPermissionRules = WatchRuleManager.Instance.GetGroupPermittedWatchRules(groupId);
            }
            else
            {
                if (extractedPermittedWatchRulesDT != null && extractedPermittedWatchRulesDT.Rows.Count > 0)
                {
                    watchPermissionRules = new List<string>();
                    foreach (DataRow permittedWatchRuleRow in extractedPermittedWatchRulesDT.Rows)
                    {
                        watchPermissionRules.Add(Utils.GetStrSafeVal(permittedWatchRuleRow, "RuleID"));
                    }
                }
            }

            string sRules = string.Empty;
            if (watchPermissionRules != null && watchPermissionRules.Count > 0)
            {
                sRules = string.Join(" ", watchPermissionRules);
            }

            return sRules;
        }

        internal static string GetPermittedWatchRules(int nGroupId)
        {
            return GetPermittedWatchRules(nGroupId, null);
        }

        private static void CopySearchValuesToSearchObjects(ref MediaSearchObj searchObject, CutWith cutWith,
            List<SearchValue> channelSearchValues)
        {
            List<SearchValue> m_dAnd = new List<SearchValue>();
            List<SearchValue> m_dOr = new List<SearchValue>();

            SearchValue search = new SearchValue();
            if (channelSearchValues != null && channelSearchValues.Count > 0)
            {
                Group group = GroupsCache.Instance().GetGroup(searchObject.m_nGroupId);

                foreach (SearchValue searchValue in channelSearchValues)
                {
                    if (!string.IsNullOrEmpty(searchValue.m_sKey))
                    {
                        if (!string.IsNullOrEmpty(searchValue.m_sKeyPrefix))
                        {
                            search = new SearchValue();
                            search.m_sKey = searchValue.m_sKey;
                            search.m_lValue = searchValue.m_lValue;
                            search.m_sKeyPrefix = searchValue.m_sKeyPrefix;
                            search.m_eInnerCutWith = searchValue.m_eInnerCutWith;

                            switch (cutWith)
                            {
                                case CutWith.WCF_ONLY_DEFAULT_VALUE:
                                    break;
                                case CutWith.OR:
                                    m_dOr.Add(search);
                                    break;
                                case CutWith.AND:
                                    m_dAnd.Add(search);
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            var searchKeys = GetUnifiedSearchKey(searchValue.m_sKey, group, searchObject.m_nGroupId);

                            switch (cutWith)
                            {
                                case CutWith.OR:
                                {
                                    foreach (var currentKey in searchKeys)
                                    {
                                        search = new SearchValue();
                                        search.m_sKey = currentKey.Field;
                                        search.m_lValue = searchValue.m_lValue;
                                        search.m_sKeyPrefix = searchValue.m_sKeyPrefix;
                                        search.m_eInnerCutWith = searchValue.m_eInnerCutWith;
                                        search.fieldType = currentKey.FieldType;
                                        m_dOr.Add(search);
                                    }

                                    break;
                                }
                                case CutWith.AND:
                                {
                                    search = new SearchValue();

                                    string searchKey = searchValue.m_sKey;

                                    //if (searchKeys.Count > 1 || searchKeys.FirstOrDefault().FieldType != eFieldType.Default)
                                    //{
                                    //    searchKey = searchKeys.First().Field;
                                    //}

                                    search.m_sKey = searchKey;
                                    search.m_lValue = searchValue.m_lValue;
                                    search.m_sKeyPrefix = searchValue.m_sKeyPrefix;
                                    search.m_eInnerCutWith = searchValue.m_eInnerCutWith;
                                    search.fieldType = searchKeys.First().FieldType;

                                    m_dAnd.Add(search);
                                    break;
                                }
                                default:
                                    break;
                            }
                        }
                    }
                }
            }

            if (m_dOr.Count > 0)
            {
                searchObject.m_dOr = m_dOr;
            }

            if (m_dAnd.Count > 0)
            {
                searchObject.m_dAnd = m_dAnd;
            }
        }

        internal static List<int> GetBundleChannelIds(int nGroupId, int nBundleId, eBundleType bundleType)
        {
            DataTable channelIdsDt;
            switch (bundleType)
            {
                case eBundleType.SUBSCRIPTION:
                {
                    channelIdsDt = CatalogDAL.Get_ChannelsBySubscription(nGroupId, nBundleId);
                    break;
                }
                case eBundleType.COLLECTION:
                {
                    channelIdsDt = CatalogDAL.Get_ChannelsByCollection(nGroupId, nBundleId);
                    break;
                }
                default:
                {
                    channelIdsDt = null;
                    break;
                }
            }

            List<int> lChannelIds = null;
            if (channelIdsDt != null && channelIdsDt.Rows.Count > 0)
            {
                lChannelIds = new List<int>(channelIdsDt.Rows.Count);
                foreach (DataRow permittedWatchRuleRow in channelIdsDt.Rows)
                {
                    lChannelIds.Add(Utils.GetIntSafeVal(permittedWatchRuleRow, "ID"));
                }
            }

            return lChannelIds;
        }

        #region UPDATE

        public static bool UpdateIndex(List<long> lMediaIds, int nGroupId, eAction eAction)
        {
            return Update(lMediaIds, nGroupId, eObjectType.Media, eAction);
        }

        public static bool UpdateEpgIndex(List<long> lEpgIds, int nGroupId, eAction eAction,
            IEnumerable<string> epgChannelIds, bool shouldGetChannelIds)
        {
            return UpdateEpg(lEpgIds, nGroupId, eObjectType.EPG, eAction, epgChannelIds, shouldGetChannelIds);
        }

        public static bool UpdateEpgRegionsIndex(List<long> epgIds, IEnumerable<long> linearChannelIds, int groupId,
            eAction action, IEnumerable<string> epgChannelIds, bool shouldGetChannelIds)
        {
            return UpdateEpgCustom(epgIds, groupId,
                (group, doesGroupUsesTemplates) =>
                {
                    var result = UpdateEpgRegionsUsingChannels(linearChannelIds, group);
                    UpdateEpgLegacy(epgIds, group, eObjectType.EPG, action, epgChannelIds, shouldGetChannelIds,
                        doesGroupUsesTemplates);
                    return result;
                });
        }

        public static bool UpdateEpgChannelIndex(List<long> ids, int groupId, eAction action)
        {
            return UpdateEpg(ids, groupId, eObjectType.EpgChannel, action, null, false);
        }

        public static bool UpdateChannelIndex(List<long> lChannelIds, int nGroupId, eAction eAction)
        {
            return Update(lChannelIds, nGroupId, eObjectType.Channel, eAction);
        }

        private static bool Update(List<int> ids, int groupId, eObjectType updatedObjectType, eAction action)
        {
            var longIds = ids.Select(i => (long) i).ToList();

            return Update(longIds, groupId, updatedObjectType, action);
        }

        private static bool Update(List<long> ids, int groupId, eObjectType updatedObjectType, eAction action, bool isAsync = false)
        {
            bool isUpdateIndexSucceeded = false;

            if (ids != null && ids.Count > 0)
            {
                int groupIdForCelery = groupId;
                Group group = null;
                bool doesGroupUsesTemplates = CatalogManagement.CatalogManager.Instance.DoesGroupUsesTemplates(groupId);
                if (!doesGroupUsesTemplates)
                {
                    GroupManager groupManager = new GroupManager();
                    CatalogCache catalogCache = CatalogCache.Instance();
                    int parentGroupId = catalogCache.GetParentGroup(groupId);
                    group = groupManager.GetGroup(parentGroupId);
                    groupIdForCelery = group.m_nParentGroupID;
                }

                if (doesGroupUsesTemplates || group != null)
                {
                    if (isAsync)
                    {
                        IndexRecordingMessageService.PublishIndexRecordingKafkaEvent(groupIdForCelery, ids, action);
                    }
                    else
                    {
                        ApiObjects.CeleryIndexingData data = new CeleryIndexingData(groupIdForCelery, ids,
                            updatedObjectType, action, DateTime.UtcNow);
                        var queue = new CatalogQueue();
                        isUpdateIndexSucceeded = queue.Enqueue(data,
                            string.Format(@"Tasks\{0}\{1}", groupIdForCelery, updatedObjectType.ToString()));

                        // backward compatibility
                        var legacyQueue = new CatalogQueue(true);
                        ApiObjects.MediaIndexingObjects.IndexingData oldData =
                            new ApiObjects.MediaIndexingObjects.IndexingData(ids, groupIdForCelery, updatedObjectType,
                                action);
                        legacyQueue.Enqueue(oldData,
                            string.Format(@"{0}\{1}", groupIdForCelery, updatedObjectType.ToString()));
                    }
                }

                switch (updatedObjectType)
                {
                    case eObjectType.Unknown:
                        break;
                    case eObjectType.Media:
                        if (action != eAction.GeoUpdate)
                        {
                            foreach (long id in ids)
                            {
                                LayeredCache.Instance.SetInvalidationKey(
                                    LayeredCacheKeys.GetMediaInvalidationKey(groupId, id));
                                if (doesGroupUsesTemplates)
                                {
                                    LayeredCache.Instance.SetInvalidationKey(
                                        LayeredCacheKeys.GetAssetInvalidationKey(groupId, eAssetType.MEDIA.ToString(),
                                            id));
                                }
                            }
                        }

                        break;
                    case eObjectType.Channel:
                        // Set invalidation for the entire group, used for GetChannelsContainingMedia
                        LayeredCache.Instance.SetInvalidationKey(
                            LayeredCacheKeys.GetGroupChannelsInvalidationKey(groupId));

                        // only needed for none OPC accounts since OPC accounts already implement immediate index update + invalidation and not via update index
                        if (!doesGroupUsesTemplates)
                        {
                            foreach (var id in ids)
                            {
                                LayeredCache.Instance.SetInvalidationKey(
                                    LayeredCacheKeys.GetChannelInvalidationKey(groupId, (int) id));
                            }
                        }

                        break;
                    case eObjectType.EPG:
                        // invalidate epg's for OPC and NON-OPC accounts
                        EpgAssetManager.InvalidateEpgs(groupId, ids, doesGroupUsesTemplates, null, true);
                        break;
                    case eObjectType.EpgChannel:
                        break;
                    case eObjectType.Recording:
                        break;
                    default:
                        break;
                }
            }

            return isUpdateIndexSucceeded;
        }

        private static bool UpdateEpg(List<long> ids, int groupId, eObjectType objectType, eAction action,
            IEnumerable<string> epgChannelIds, bool shouldGetChannelIds)
        {
            return UpdateEpgCustom(ids, groupId,
                (group, doesGroupUsesTemplates) =>
                {
                    var result = UpdateEpg(ids, group, objectType, action);
                    UpdateEpgLegacy(ids, group, eObjectType.EPG, action, epgChannelIds, shouldGetChannelIds,
                        doesGroupUsesTemplates);
                    return result;
                });
        }

        private static bool UpdateEpgCustom(List<long> ids, int groupId, Func<int, bool, bool> updateEpgFunc)
        {
            bool isUpdateIndexSucceeded = false;

            if (ids != null && ids.Count > 0)
            {
                bool doesGroupUsesTemplates = CatalogManager.Instance.DoesGroupUsesTemplates(groupId);

                if (!doesGroupUsesTemplates)
                {
                    GroupManager groupManager = new GroupManager();

                    CatalogCache catalogCache = CatalogCache.Instance();
                    groupId = catalogCache.GetParentGroup(groupId);
                }

                if (groupId > 0)
                {
                    isUpdateIndexSucceeded = updateEpgFunc(groupId, doesGroupUsesTemplates);
                }
            }

            return isUpdateIndexSucceeded;
        }

        private static bool UpdateEpg(List<long> ids, int groupId, eObjectType objectType, eAction action)
        {
            bool isUpdateIndexSucceeded;
            var data = new CeleryIndexingData(groupId, ids, objectType, action, DateTime.UtcNow);
            var queue = new CatalogQueue();

            isUpdateIndexSucceeded =
                queue.Enqueue(data, string.Format(@"Tasks\{0}\{1}", groupId, objectType.ToString()));
            if (isUpdateIndexSucceeded)
                log.DebugFormat("successfully enqueue epg upload. data: {0}", data);
            else
                log.ErrorFormat("Failed enqueue of epg upload. data: {0}", data);
            return isUpdateIndexSucceeded;
        }

        private static bool UpdateEpgRegionsUsingChannels(IEnumerable<long> linearChannelsId, int groupId)
        {
            return UpdateEpg(linearChannelsId.ToList(), groupId, eObjectType.Channel, eAction.EpgRegionUpdate);
        }

        private static void UpdateEpgLegacy(
            List<long> ids,
            int groupId,
            eObjectType objectType,
            eAction action,
            IEnumerable<string> epgChannelIds,
            bool shouldGetChannelIds,
            bool doesGroupUsesTemplates)
        {
            // Backward compatibility
            if (objectType == eObjectType.EPG)
            {
                var legacyQueue = new CatalogQueue(true);
                ApiObjects.MediaIndexingObjects.IndexingData oldData =
                    new ApiObjects.MediaIndexingObjects.IndexingData(ids, groupId, objectType, action);
                legacyQueue.Enqueue(oldData, string.Format(@"{0}\{1}", groupId, objectType.ToString()));

                // invalidate epg's for OPC and NON-OPC accounts
                EpgAssetManager.InvalidateEpgs(groupId, ids, doesGroupUsesTemplates, epgChannelIds,
                    shouldGetChannelIds);
            }
        }

        #endregion

        internal static SearchResultsObj GetProgramIdsFromSearcher(EpgSearchObj epgSearchReq)
        {
            CatalogCache catalogCache = CatalogCache.Instance();
            int nParentGroupID = catalogCache.GetParentGroup(epgSearchReq.m_nGroupID);
            IIndexManager indexManager = IndexManagerFactory.Instance.GetIndexManager(nParentGroupID);

            if (indexManager == null)
            {
                throw new Exception(String.Concat("Failed to create Searcher instance. Request is: ",
                    epgSearchReq != null ? epgSearchReq.ToString() : "null"));
            }

            return indexManager.SearchEpgs(epgSearchReq);
        }

        internal static void GetGroupsTagsAndMetas(int nGroupID, ref List<string> lSearchList)
        {
            GroupManager groupManager = new GroupManager();

            CatalogCache catalogCache = CatalogCache.Instance();
            int nParentGroupID = catalogCache.GetParentGroup(nGroupID);

            List<int> lSubGroup = groupManager.GetSubGroup(nParentGroupID);

            DataSet ds = EpgDal.Get_GroupsTagsAndMetas(nGroupID, lSubGroup);

            lSearchList.Add("name");
            lSearchList.Add("description");

            if (ds != null && ds.Tables != null && ds.Tables.Count >= 2)
            {
                //Metas
                if (ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        string filed = ODBCWrapper.Utils.GetSafeStr(row["name"]);
                        if (!string.IsNullOrEmpty(filed))
                        {
                            lSearchList.Add(filed);
                        }
                    }
                }

                //Tags
                if (ds.Tables[1] != null && ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[1].Rows)
                    {
                        string filed = ODBCWrapper.Utils.GetSafeStr(row["name"]);
                        if (!string.IsNullOrEmpty(filed))
                        {
                            lSearchList.Add(filed);
                        }
                    }
                }
            }
        }

        internal static EpgProgramResponse CompleteDetailsForProgramResponse(EpgProgramDetailsRequest pRequest)
        {
            int nStartIndex = pRequest.m_nPageIndex * pRequest.m_nPageSize;
            int nEndIndex = pRequest.m_nPageIndex * pRequest.m_nPageSize + pRequest.m_nPageSize;

            if ((nStartIndex == 0 && nEndIndex == 0 && pRequest.m_lProgramsIds != null &&
                 pRequest.m_lProgramsIds.Count > 0) ||
                nEndIndex > pRequest.m_lProgramsIds.Count)
            {
                nEndIndex = pRequest.m_lProgramsIds.Count();
            }

            if (pRequest.m_lProgramsIds == null || pRequest.m_lProgramsIds.Count == 0)
            {
                return new EpgProgramResponse {m_lObj = new List<BaseObject>()};
            }

            var languages = GetLanguages(pRequest.m_oFilter, pRequest.m_nGroupID);
            var epgIds = pRequest.m_lProgramsIds?.Select(id => (long) id).ToList();
            var programs = GetProgramObjects(pRequest.m_nGroupID, epgIds, languages);

            return new EpgProgramResponse
            {
                m_nTotalItems = programs.Count,
                m_lObj = programs.Cast<BaseObject>().ToList()
            };
        }

        public static void GetLinearChannelSettings(int groupID, List<EPGChannelProgrammeObject> lEpgProg)
        {
            try
            {
                List<string> epgChannelIds = lEpgProg
                    .Where(item => item != null && !string.IsNullOrEmpty(item.EPG_CHANNEL_ID))
                    .Select(item => item.EPG_CHANNEL_ID).ToList<string>(); // get all epg channel ids

                Dictionary<string, LinearChannelSettings> linearChannelSettings =
                    CatalogCache.Instance().GetLinearChannelSettings(groupID, epgChannelIds);
                foreach (EPGChannelProgrammeObject epg in lEpgProg)
                {
                    if (linearChannelSettings.ContainsKey(epg.EPG_CHANNEL_ID))
                    {
                        LinearChannelSettings linearSettings = linearChannelSettings[epg.EPG_CHANNEL_ID];
                        if (linearSettings != null)
                        {
                            SetLinearEpgProgramSettings(epg, linearSettings);
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                log.Error(
                    string.Format("failed Catalog.GetLinearChannelSettings for groupId: {0}, lEpgProg: {1}", groupID,
                        lEpgProg != null ? string.Join(", ", lEpgProg.Select(x => x.EPG_ID).ToList()) : string.Empty),
                    ex);
            }
        }

        private static void SetLinearEpgProgramSettings(EPGChannelProgrammeObject epg,
            LinearChannelSettings linearSettings)
        {
            // 0 ==> need to get value from LinearChannelSettings
            // 1 ==> check the value at LinearChannelSettings : if true - OK else false
            // 2 ==> false - do nothing
            if (epg.ENABLE_CDVR != 2) // get value from epg channel
            {
                epg.ENABLE_CDVR = linearSettings.EnableCDVR == true ? 1 : 0;
            }

            if (epg.ENABLE_START_OVER != 2) // get value from epg channel
            {
                epg.ENABLE_START_OVER = linearSettings.EnableStartOver == true ? 1 : 0;
            }

            if (epg.ENABLE_CATCH_UP != 2) // get value from epg channel
            {
                epg.ENABLE_CATCH_UP = linearSettings.EnableCatchUp == true ? 1 : 0;
            }

            epg.CHANNEL_CATCH_UP_BUFFER = linearSettings.CatchUpBuffer;

            if (epg.ENABLE_TRICK_PLAY != 2) // get value from epg channel
            {
                epg.ENABLE_TRICK_PLAY = linearSettings.EnableTrickPlay == true ? 1 : 0;
            }

            epg.LINEAR_MEDIA_ID = linearSettings.LinearMediaId;
        }

        private static EPGChannelProgrammeObject GetLinearEpgProgramSettings(DataRow[] dr, DataRow drAccount)
        {
            EPGChannelProgrammeObject epg = new EPGChannelProgrammeObject();
            int enable = 0;
            int enableChannel = 0;

            enable = ODBCWrapper.Utils.GetIntSafeVal(dr[0], "ENABLE_CDVR"); // account
            if (enable == 1)
            {
                enableChannel = ODBCWrapper.Utils.GetIntSafeVal(drAccount, "ENABLE_CDVR"); // channel settings
                if (enableChannel != 0) //None/NULL
                {
                    enable = enableChannel;
                }
            }

            epg.ENABLE_CDVR = enable;

            enable = ODBCWrapper.Utils.GetIntSafeVal(dr[0], "ENABLE_CATCH_UP"); // account
            if (enable == 1)
            {
                enableChannel = ODBCWrapper.Utils.GetIntSafeVal(drAccount, "ENABLE_CATCH_UP"); // channel settings
                if (enableChannel != 0) //None/NULL
                {
                    enable = enableChannel;
                }
            }

            epg.ENABLE_CATCH_UP = enable;

            enable = ODBCWrapper.Utils.GetIntSafeVal(dr[0], "ENABLE_START_OVER"); // account
            if (enable == 1)
            {
                enableChannel = ODBCWrapper.Utils.GetIntSafeVal(drAccount, "ENABLE_START_OVER"); // channel settings
                if (enableChannel != 0) //None/NULL
                {
                    enable = enableChannel;
                }
            }

            epg.ENABLE_START_OVER = enable;

            enable = ODBCWrapper.Utils.GetIntSafeVal(dr[0], "ENABLE_TRICK_PLAY"); // account
            if (enable == 0)
            {
                enableChannel = ODBCWrapper.Utils.GetIntSafeVal(drAccount, "ENABLE_TRICK_PLAY"); //channel settings
                if (enableChannel != 0) //None/NULL
                {
                    enable = enableChannel;
                }
            }

            epg.ENABLE_TRICK_PLAY = enable;

            return epg;
        }

        private static ProgramObj GetProgramDetails(int nProgramID, EpgProgramDetailsRequest pRequest)
        {
            bool result = true;
            try
            {
                ProgramObj oProgramObj = new ProgramObj();

                DataSet ds = EpgDal.GetEpgProgramDetails(pRequest.m_nGroupID, nProgramID);

                if (ds == null)
                    return null;
                if (ds.Tables.Count >= 5)
                {
                    bool isProgram = GetProgramBasicDetails(ref oProgramObj, ds.Tables[0], ds.Tables[1]);
                    if (
                        isProgram) //only if we found basic details for media - media in status = 1 , and active if necessary
                    {
                        oProgramObj.m_oProgram.EPG_Meta = GetEpgMetaDetails(ds.Tables[2], ref result);
                        oProgramObj.m_oProgram.EPG_TAGS = GetEpgTagsDetails(ds.Tables[3], ref result);
                    }
                    else
                    {
                        return null;
                    }
                }

                return oProgramObj;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                result = false;
                return null;
            }
        }

        private static List<EPGDictionary> GetEpgTagsDetails(DataTable dtTags, ref bool result)
        {
            try
            {
                result = true;
                EPGDictionary oTag;
                List<EPGDictionary> lTags = new List<EPGDictionary>();

                if (dtTags != null && dtTags.DefaultView != null)
                {
                    foreach (DataRow row in dtTags.Rows)
                    {
                        oTag = new EPGDictionary();
                        oTag.Key = Utils.GetStrSafeVal(row, "TagTypeName");
                        oTag.Value = Utils.GetStrSafeVal(row, "TagValueName");
                        lTags.Add(oTag);
                    }
                }

                return lTags;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                result = false;
                return null;
            }
        }

        private static List<EPGDictionary> GetEpgMetaDetails(DataTable dtMeta, ref bool result)
        {
            try
            {
                result = true;
                EPGDictionary oMeta;
                List<EPGDictionary> lMetas = new List<EPGDictionary>();

                if (dtMeta != null && dtMeta.DefaultView != null)
                {
                    foreach (DataRow row in dtMeta.Rows)
                    {
                        oMeta = new EPGDictionary();
                        oMeta.Key = Utils.GetStrSafeVal(row, "name");
                        oMeta.Value = Utils.GetStrSafeVal(row, "value");
                        lMetas.Add(oMeta);
                    }
                }

                return lMetas;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                result = false;
                return null;
            }
        }

        private static bool GetProgramBasicDetails(ref ProgramObj oProgramObj, DataTable dt, DataTable dtUpdateDate)
        {
            bool result = false;
            try
            {
                if (dt.Columns != null && dt.Rows.Count != 0)
                {
                    result = true;

                    string pic_url = string.Empty;
                    oProgramObj.m_oProgram.EPG_ID = Utils.GetIntSafeVal(dt.Rows[0], "ID");
                    oProgramObj.m_oProgram.EPG_IDENTIFIER = Utils.GetStrSafeVal(dt.Rows[0], "EPG_IDENTIFIER");
                    oProgramObj.m_oProgram.NAME = Utils.GetStrSafeVal(dt.Rows[0], "NAME");
                    oProgramObj.m_oProgram.DESCRIPTION = Utils.GetStrSafeVal(dt.Rows[0], "DESCRIPTION");
                    oProgramObj.m_oProgram.EPG_CHANNEL_ID = Utils.GetStrSafeVal(dt.Rows[0], "EPG_CHANNEL_ID");
                    oProgramObj.m_oProgram.LIKE_COUNTER = Utils.GetIntSafeVal(dt.Rows[0], "like_counter");
                    oProgramObj.m_oProgram.PIC_URL = Utils.GetStrSafeVal(dt.Rows[0], "PIC_URL");
                    oProgramObj.m_oProgram.STATUS = Utils.GetStrSafeVal(dt.Rows[0], "STATUS");
                    oProgramObj.m_oProgram.IS_ACTIVE = Utils.GetStrSafeVal(dt.Rows[0], "IS_ACTIVE");
                    oProgramObj.m_oProgram.GROUP_ID = Utils.GetStrSafeVal(dt.Rows[0], "GROUP_ID");
                    oProgramObj.m_oProgram.UPDATER_ID = Utils.GetStrSafeVal(dt.Rows[0], "UPDATER_ID");
                    oProgramObj.m_oProgram.media_id = Utils.GetStrSafeVal(dt.Rows[0], "MEDIA_ID");

                    //Dates
                    oProgramObj.m_oProgram.START_DATE = Utils.GetStrSafeVal(dt.Rows[0], "START_DATE");
                    oProgramObj.m_oProgram.END_DATE = Utils.GetStrSafeVal(dt.Rows[0], "END_DATE");
                    oProgramObj.m_oProgram.CREATE_DATE = Utils.GetStrSafeVal(dt.Rows[0], "CREATE_DATE");
                    oProgramObj.m_oProgram.PUBLISH_DATE = Utils.GetStrSafeVal(dt.Rows[0], "PUBLISH_DATE");
                    oProgramObj.m_oProgram.UPDATE_DATE = Utils.GetStrSafeVal(dtUpdateDate.Rows[0], "UPDATE_DATE");
                }

                return result;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return false;
            }
        }

        internal static List<long> GetEpgChannelIDsForIPNOFiltering(int groupID,
            int domainId, string siteGuid,
            ref List<List<string>> jsonizedChannelsDefinitions)
        {
            List<long> res = new List<long>();
            Dictionary<string, string> dict = GetLinearMediaTypeIDsAndWatchRuleIDs(groupID);
            MediaSearchObj linearChannelMediaIDsRequest = BuildLinearChannelsMediaIDsRequest(groupID,
                domainId, siteGuid,
                dict, jsonizedChannelsDefinitions);
            var indexManager = IndexManagerFactory.Instance.GetIndexManager(groupID);
            SearchResultsObj searcherAnswer = indexManager.SearchMedias(linearChannelMediaIDsRequest, 0, true);

            if (searcherAnswer.n_TotalItems > 0)
            {
                List<long> ipnoEPGChannelsMediaIDs = ExtractMediaIDs(searcherAnswer);
                res.AddRange(GetEPGChannelsIDs(ipnoEPGChannelsMediaIDs));
            }

            return res;
        }

        internal static List<string> EpgAutoComplete(EpgSearchObj request)
        {
            IIndexManager indexManager = IndexManagerFactory.Instance.GetIndexManager(request.m_nGroupID);

            if (indexManager == null || request == null)
            {
                throw new Exception("EpgAutoComplete. Either EpgSearchObj or Searcher instance is null.");
            }

            return indexManager.GetEpgAutoCompleteList(request);
        }

        private static List<LanguageObj> GetLanguages(Filter filter, int groupId)
        {
            var group = GroupsCache.Instance().GetGroup(groupId);
            if (filter == null)
            {
                return new List<LanguageObj> {group.GetGroupDefaultLanguage()};
            }

            if (filter.m_nLanguage == 0)
            {
                return group.GetLangauges();
            }

            var language = group.GetLanguage(filter.m_nLanguage);
            // in case no language was found - throw Exception
            if (language == null)
            {
                throw new Exception(
                    $"Error while getting language: {filter.m_nLanguage} for group: {group.m_nParentGroupID}");
            }

            return new List<LanguageObj> {language};
        }

        private static List<ProgramObj> GetProgramObjects(int groupId, List<long> epgIds, List<LanguageObj> languages)
        {
            var epgBl = EpgBL.Utils.GetInstance(groupId);
            bool isOpcAccount = CatalogManagement.CatalogManager.Instance.DoesGroupUsesTemplates(groupId);
            var basicEpgObjects =
                epgBl.GetEpgCBsWithLanguage(epgIds.Select(id => (ulong) id).ToList(), languages, isOpcAccount);
            if (basicEpgObjects == null || basicEpgObjects.Count == 0)
            {
                return new List<ProgramObj>();
            }

            // get all linear settings about channel + group
            GetLinearChannelSettings(groupId, basicEpgObjects);

            return basicEpgObjects
                .Where(x => epgIds.Contains(x.EPG_ID))
                .Select(MapToProgramObj)
                .ToList();
        }

        private static ProgramObj MapToProgramObj(EPGChannelProgrammeObject currentEpg)
            => new ProgramObj
            {
                m_oProgram = currentEpg,
                AssetId = currentEpg.EPG_ID.ToString(),
                m_dUpdateDate = Utils.ConvertStringToDateTimeByFormat(
                    currentEpg.UPDATE_DATE,
                    EPGChannelProgrammeObject.DATE_FORMAT,
                    out var convertedDate)
                    ? convertedDate
                    : DateTime.MinValue
            };

        /*
         * We set isSortResults to true when we are unable to bring the results sorted from CB. In this case the code will verify that the
         * results are sorted.
         * When we are able to bring the results from CV sorted, we set this flag to false.
         *
         */
        private static ICollection<EPGChannelProgrammeObject> GetEPGProgramsCollectionFactory(bool isSortResults)
        {
            if (isSortResults)
                return new SortedSet<EPGChannelProgrammeObject>(
                    new EPGChannelProgrammeObject.EPGChannelProgrammeObjectStartDateComparer());
            return new List<EPGChannelProgrammeObject>();
        }

        internal static EpgResponse GetEPGProgramsFromCB(List<int> epgIDs, int parentGroupID, bool isSortResults,
            List<int> epgChannelIDs, int languageId)
        {
            EpgResponse res = new EpgResponse();
            List<EPGChannelProgrammeObject> epgs =
                GetEpgsByGroupIdLanguageIdAndEpgIds(parentGroupID, epgIDs, languageId);
            if (epgs != null && epgs.Count > 0)
            {
                int totalItems = 0;
                // get all linear settings about channel + group
                GetLinearChannelSettings(parentGroupID, epgs);

                Dictionary<int, ICollection<EPGChannelProgrammeObject>> channelIdsToProgrammesMapping =
                    new Dictionary<int, ICollection<EPGChannelProgrammeObject>>(epgChannelIDs.Count);

                for (int i = 0; i < epgs.Count; i++)
                {
                    int tempEpgChannelID = 0;
                    if (epgs[i] == null || !Int32.TryParse(epgs[i].EPG_CHANNEL_ID, out tempEpgChannelID) ||
                        tempEpgChannelID < 1)
                    {
                        continue;
                    }

                    if (channelIdsToProgrammesMapping.ContainsKey(tempEpgChannelID))
                    {
                        channelIdsToProgrammesMapping[tempEpgChannelID].Add(epgs[i]);
                    }
                    else
                    {
                        channelIdsToProgrammesMapping.Add(tempEpgChannelID,
                            GetEPGProgramsCollectionFactory(isSortResults));
                        channelIdsToProgrammesMapping[tempEpgChannelID].Add(epgs[i]);
                    }
                } // for

                // build response in the same order we rcvd the epg channel ids.
                for (int i = 0; i < epgChannelIDs.Count; i++)
                {
                    if (channelIdsToProgrammesMapping.ContainsKey(epgChannelIDs[i]))
                    {
                        res.programsPerChannel.Add(BuildResObjForChannel(
                            channelIdsToProgrammesMapping[epgChannelIDs[i]], epgChannelIDs[i], ref totalItems));
                    }
                }

                res.m_nTotalItems = totalItems;
            }
            else
            {
                #region Logging

                StringBuilder sb = new StringBuilder("Failed to retrieve epg programmes from cb. ");
                sb.Append(String.Concat("G ID: ", parentGroupID));
                sb.Append(String.Concat("sizeof(epgIDs) : ", epgIDs != null ? epgIDs.Count : 0));
                log.Error("Error - " + sb.ToString());

                #endregion
            }

            return res;
        }

        /// <summary>
        /// For a list of Epg Ids, creates relevant program objects, filled with data
        /// </summary>
        /// <param name="epgIds"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        internal static List<ProgramObj> GetEPGProgramInformation(List<long> epgIds, int groupId, Filter filter = null)
        {
            // Don't do anything if no valid input
            if (epgIds == null || epgIds.Count == 0)
            {
                return new List<ProgramObj>();
            }

            var languages = GetLanguages(filter, groupId);

            return GetProgramObjects(groupId, epgIds, languages);
        }

        private static EpgResultsObj BuildResObjForChannel(ICollection<EPGChannelProgrammeObject> programmes,
            int epgChannelID, ref int totalItems)
        {
            EpgResultsObj res = new EpgResultsObj();
            res.m_lEpgProgram.AddRange(programmes);
            res.m_nChannelID = epgChannelID;
            res.m_nTotalItems = programmes.Count;
            totalItems += programmes.Count;

            return res;
        }

        private static void MutateFullEpgPicURL(List<EPGChannelProgrammeObject> epgList,
            string baseEpgPicUrl, string epgPicWidth, string epgPicHeight)
        {
            foreach (ApiObjects.EPGChannelProgrammeObject oProgram in epgList)
            {
                if (oProgram != null && !string.IsNullOrEmpty(baseEpgPicUrl) && !string.IsNullOrEmpty(oProgram.PIC_URL))
                {
                    if (!string.IsNullOrEmpty(epgPicWidth) && !string.IsNullOrEmpty(epgPicHeight))
                    {
                        oProgram.PIC_URL =
                            oProgram.PIC_URL.Replace(".", string.Format("_{0}X{1}.", epgPicWidth, epgPicHeight));
                    }

                    oProgram.PIC_URL = string.Format("{0}{1}", baseEpgPicUrl, oProgram.PIC_URL);
                }
            }
        }

        private static void GetEpgPicUrlData(List<EPGChannelProgrammeObject> epgList,
            Dictionary<int, List<string>> groupTreeEpgPicUrl,
            ref string epgPicBaseUrl, ref string epgPicWidth, ref string epgPicHeight)
        {
            int groupID = 0;
            if (epgList != null && epgList.Count > 0 && epgList[0] != null &&
                Int32.TryParse(epgList[0].GROUP_ID, out groupID)
                && groupID > 0 && groupTreeEpgPicUrl.ContainsKey(groupID))
            {
                epgPicBaseUrl = groupTreeEpgPicUrl[groupID][0];
                epgPicWidth = groupTreeEpgPicUrl[groupID][1];
                epgPicHeight = groupTreeEpgPicUrl[groupID][2];
            }
        }

        private static bool IsBringAllStatsRegardlessDates(DateTime startDate, DateTime endDate)
        {
            return startDate.Equals(DateTime.MinValue) && endDate.Equals(DateTime.MaxValue);
        }

        private static string GetAssetStatsResultsLogMsg(string sMsg, int nGroupID, List<int> lAssetIDs,
            DateTime dStartDate, DateTime dEndDate, StatsType eType)
        {
            StringBuilder sb = new StringBuilder(sMsg);
            sb.Append(String.Concat(" G ID: ", nGroupID));
            sb.Append(String.Concat(" SD: ", dStartDate.ToString()));
            sb.Append(String.Concat(" ED: ", dEndDate.ToString()));
            sb.Append(String.Concat(" Type: ", eType.ToString()));
            if (lAssetIDs != null && lAssetIDs.Count > 0)
            {
                sb.Append(" Asset IDs: ");
                for (int i = 0; i < lAssetIDs.Count; i++)
                {
                    sb.Append(String.Concat(lAssetIDs[i], ";"));
                }
            }
            else
            {
                sb.Append(String.Concat(" Asset IDs list is null or empty. "));
            }

            return sb.ToString();
        }

        private static void InitializeAssetStatsResultsDataStructs(List<int> assetIds,
            ref SortedSet<AssetStatsResult.IndexedAssetStatsResult> set,
            ref Dictionary<int, AssetStatsResult> assetIdToAssetStatsResultMapping)
        {
            set = new SortedSet<AssetStatsResult.IndexedAssetStatsResult>();
            assetIdToAssetStatsResultMapping = new Dictionary<int, AssetStatsResult>(assetIds.Count);
            for (int i = 0; i < assetIds.Count; i++)
            {
                if (assetIds[i] > 0)
                {
                    AssetStatsResult result = new AssetStatsResult();
                    result.m_nAssetID = assetIds[i];
                    if (!assetIdToAssetStatsResultMapping.ContainsKey(assetIds[i]))
                    {
                        set.Add(new AssetStatsResult.IndexedAssetStatsResult(i, result));
                        assetIdToAssetStatsResultMapping.Add(assetIds[i], result);
                    }
                }
            }
        }

        private static List<EPGChannelProgrammeObject> GetEpgsByGroupAndIDs(int groupID, List<int> epgIDs)
        {
            BaseEpgBL epgBL = EpgBL.Utils.GetInstance(groupID);
            bool isOpcAccount = CatalogManagement.CatalogManager.Instance.DoesGroupUsesTemplates(groupID);
            var result = epgBL.GetEpgs(epgIDs, isOpcAccount);

            //var language = new GroupManager().GetGroup(groupID).GetGroupDefaultLanguage();

            //BaseEpgBL.UpdateProgrammeWithMultilingual(ref result, language);

            return result;
        }

        private static List<EPGChannelProgrammeObject> GetEpgsByGroupIdLanguageIdAndEpgIds(int groupID,
            List<int> epgIDs, int languageId)
        {
            LanguageObj lang = null;
            string langCode = string.Empty;
            if (languageId > 0)
            {
                lang = GetLanguage(groupID, languageId);
            }

            if (lang != null && !lang.IsDefault)
            {
                langCode = lang.Code;
            }

            if (string.IsNullOrEmpty(langCode))
            {
                return GetEpgsByGroupAndIDs(groupID, epgIDs);
            }
            else
            {
                BaseEpgBL epgBL = EpgBL.Utils.GetInstance(groupID);
                bool isOpcAccount = CatalogManagement.CatalogManager.Instance.DoesGroupUsesTemplates(groupID);
                return epgBL.GetEpgCBsWithLanguage(epgIDs.Select(x => (ulong) x).ToList(), langCode, isOpcAccount);
            }
        }

        internal static List<AssetStatsResult> GetAssetStatsResults(int groupId, List<int> assetIDs,
            DateTime dStartDate, DateTime dEndDate, StatsType statsType)
        {
            // Data structures here are used for returning List<AssetStatsResult> in the same order asset ids are given in lAssetIDs
            SortedSet<AssetStatsResult.IndexedAssetStatsResult> set = null;
            Dictionary<int, AssetStatsResult> assetIdToAssetStatsMapping = null;
            InitializeAssetStatsResultsDataStructs(assetIDs, ref set, ref assetIdToAssetStatsMapping);

            var indexManager = IndexManagerFactory.Instance.GetIndexManager(groupId);
            switch (statsType)
            {
                case StatsType.MEDIA:
                {
                    BaseStaticticsBL staticticsBL = StatisticsBL.Utils.GetInstance(groupId);
                    Dictionary<string, BuzzWeightedAverScore> buzzDict = null;
                    if (TvinciCache.GroupsFeatures.GetGroupFeatureStatus(groupId, GroupFeature.BUZZFEED))
                    {
                        buzzDict = staticticsBL.GetBuzzAverScore(assetIDs);
                    }

                    bool isBuzzNotEmpty = buzzDict != null && buzzDict.Count > 0;

                    /************* For versions after Joker that don't want to use DB for getting stats, we fetch the data from ES statistics index **********/

                    if (dStartDate != DateTime.MinValue || dEndDate != DateTime.MaxValue)
                    {
                        indexManager.GetAssetStats(assetIDs, dStartDate, dEndDate, StatsType.MEDIA,
                            ref assetIdToAssetStatsMapping);
                    }
                    else
                    {
                        Dictionary<string, string> keysToOriginalValueMap =
                            assetIDs.ToDictionary(x => LayeredCacheKeys.GetMediaStatsKey(x), x => x.ToString());
                        Dictionary<string, AssetStatsResult> results = new Dictionary<string, AssetStatsResult>();
                        Dictionary<string, object> funcParameters = new Dictionary<string, object>()
                        {
                            {"assetsIds", assetIDs},
                            {"statsType", StatsType.MEDIA},
                            {"groupId", groupId},
                            {"mapping", assetIdToAssetStatsMapping},
                        };

                        bool success = LayeredCache.Instance.GetValues<AssetStatsResult>(keysToOriginalValueMap,
                            ref results, GetAssetsStats, funcParameters, groupId,
                            LayeredCacheConfigNames.ASSET_STATS_CONFIG_NAME);

                        if (!success)
                        {
                            log.ErrorFormat("Failed getting stats for medias {0} group {1}", string.Join(",", assetIDs),
                                groupId);
                        }
                        else
                        {
                            foreach (var assetStats in results.Values)
                            {
                                assetIdToAssetStatsMapping[assetStats.m_nAssetID].CopyFrom(assetStats);
                            }
                        }
                    }

                    break;
                }
                case StatsType.EPG:
                {
                    /*
                     * Notice: In EPG we bring only likes!!
                     *
                     */
                    if (IsBringAllStatsRegardlessDates(dStartDate, dEndDate))
                    {
                        /*
                         * When we don't have dates we bring the likes count from epg_channels_schedule bucket in CB
                         *
                         */
                        List<EPGChannelProgrammeObject> lEpg = GetEpgsByGroupAndIDs(groupId, assetIDs);
                        if (lEpg != null && lEpg.Count > 0)
                        {
                            for (int i = 0; i < lEpg.Count; i++)
                            {
                                if (lEpg[i] != null)
                                {
                                    int currEpgId = (int) lEpg[i].EPG_ID;
                                    if (assetIdToAssetStatsMapping.ContainsKey(currEpgId))
                                    {
                                        assetIdToAssetStatsMapping[currEpgId].m_nLikes = lEpg[i].LIKE_COUNTER;
                                    }
                                }
                            } // for
                        }
                        else
                        {
                            // log here no epgs retrieved from epg_channels_schedule CB bucket.
                            log.Error("Error - " + GetAssetStatsResultsLogMsg(
                                "No EPGs retrieved from epg_channels_schedule CB bucket", groupId, assetIDs, dStartDate,
                                dEndDate, statsType));
                        }
                    }
                    else
                    {
                        /************* For versions after Joker that don't want to use DB for getting stats, we fetch the data from ES statistics index **********/

                        if (dStartDate != DateTime.MinValue || dEndDate != DateTime.MaxValue)
                        {
                            indexManager.GetAssetStats(assetIDs, dStartDate, dEndDate, StatsType.EPG,
                                ref assetIdToAssetStatsMapping);
                        }
                        else
                        {
                            Dictionary<string, string> keysToOriginalValueMap =
                                assetIDs.ToDictionary(x => LayeredCacheKeys.GetEPGStatsKey(x), x => x.ToString());
                            Dictionary<string, AssetStatsResult> results = new Dictionary<string, AssetStatsResult>();
                            Dictionary<string, object> funcParameters = new Dictionary<string, object>()
                            {
                                {"assetsIds", assetIDs},
                                {"statsType", StatsType.EPG},
                                {"groupId", groupId},
                                {"mapping", assetIdToAssetStatsMapping},
                            };
                            Dictionary<string, List<string>> invalidationKeys =
                                keysToOriginalValueMap.Keys.ToDictionary(x => x,
                                    x => new List<string>() {LayeredCacheKeys.GetAssetStatsInvalidationKey(groupId)});

                            bool success = LayeredCache.Instance.GetValues<AssetStatsResult>(keysToOriginalValueMap,
                                ref results, GetAssetsStats, funcParameters, groupId,
                                LayeredCacheConfigNames.ASSET_STATS_CONFIG_NAME, invalidationKeys);

                            if (!success)
                            {
                                log.ErrorFormat("Failed getting stats for epgs {0} group {1}",
                                    string.Join(",", assetIDs), groupId);
                            }
                            else
                            {
                                foreach (var assetStats in results.Values)
                                {
                                    assetIdToAssetStatsMapping[assetStats.m_nAssetID].CopyFrom(assetStats);
                                }
                            }
                            //foreach (var assetId in lAssetIDs)
                            //{
                            //    Dictionary<string, object> funcParameters = new Dictionary<string, object>()
                            //{
                            //    { "assetId", assetId },
                            //    { "statsType", StatsType.EPG },
                            //    { "groupId", nGroupID },
                            //    { "mapping", assetIdToAssetStatsMapping },
                            //};

                            //    AssetStatsResult assetStatsResult = null;
                            //    bool success = LayeredCache.Instance.Get<AssetStatsResult>(LayeredCacheKeys.GetEPGStatsKey(assetId), ref assetStatsResult, GetAssetStats, funcParameters,
                            //        nGroupID, LayeredCacheConfigNames.ASSET_STATS_CONFIG_NAME);

                            //    if (!success)
                            //    {
                            //        log.ErrorFormat("Failed getting stats for EPG {0} group {1}", assetId, nGroupID);
                            //    }
                            //}
                        }
                    }

                    break;
                }
                default:
                {
                    throw new NotImplementedException(String.Concat("Unsupported stats type: ", statsType.ToString()));
                }
            } // switch

            return set.Select((item) => (item.Result)).ToList<AssetStatsResult>();
        }

        private static Tuple<Dictionary<string, AssetStatsResult>, bool> GetAssetsStats(
            Dictionary<string, object> funcParams)
        {
            Dictionary<string, AssetStatsResult> result = null;
            bool success = false;

            try
            {
                if (funcParams != null &&
                    funcParams.ContainsKey("assetsIds") && funcParams.ContainsKey("statsType") &&
                    funcParams.ContainsKey("groupId") && funcParams.ContainsKey("mapping"))
                {
                    List<int> assetIds = null;

                    if (funcParams.ContainsKey(LayeredCache.MISSING_KEYS) &&
                        funcParams[LayeredCache.MISSING_KEYS] != null)
                    {
                        assetIds = ((List<string>) funcParams[LayeredCache.MISSING_KEYS]).Select(x => int.Parse(x))
                            .ToList();
                    }
                    else
                    {
                        assetIds = funcParams["assetsIds"] as List<int>;
                    }

                    int groupId = Convert.ToInt32(funcParams["groupId"]);
                    var indexManager = IndexManagerFactory.Instance.GetIndexManager(groupId);
                    StatsType statsType = (StatsType) funcParams["statsType"];
                    Dictionary<int, AssetStatsResult> mapping =
                        (Dictionary<int, AssetStatsResult>) funcParams["mapping"];

                    indexManager.GetAssetStats(assetIds, DateTime.MinValue, DateTime.MaxValue, statsType, ref mapping);

                    result = new Dictionary<string, AssetStatsResult>();

                    foreach (var assetId in assetIds)
                    {
                        if (mapping.ContainsKey(assetId))
                        {
                            string key = string.Empty;

                            switch (statsType)
                            {
                                case StatsType.MEDIA:
                                    key = LayeredCacheKeys.GetMediaStatsKey(assetId);
                                    break;
                                case StatsType.EPG:
                                    key = LayeredCacheKeys.GetEPGStatsKey(assetId);
                                    break;
                                default:
                                    break;
                            }

                            result[key] = mapping[assetId];
                        }
                    }

                    success = true;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error when getting assets stats result. ex = {0}", ex);
            }

            return new Tuple<Dictionary<string, AssetStatsResult>, bool>(result, success);
        }

        private static AssetStatsResult.SocialPartialAssetStatsResult GetSocialAssetStats(int groupId, int assetId,
            StatsType statsTypes,
            DateTime startDate, DateTime endDate)
        {
            SocialDAL_Couchbase socialDal = new SocialDAL_Couchbase(groupId);
            AssetStatsResult.SocialPartialAssetStatsResult res = new AssetStatsResult.SocialPartialAssetStatsResult()
            {
                assetId = assetId
            };
            switch (statsTypes)
            {
                case StatsType.MEDIA:
                {
                    // in media we bring likes and rates where rates := if ratesCount != 0 then ratesSum/ratesCount otherwise 0d
                    res.likesCounter = socialDal.GetAssetSocialActionCount(assetId, eAssetType.MEDIA, eUserAction.LIKE,
                        startDate, endDate);
                    res.votes = socialDal.GetAssetSocialActionCount(assetId, eAssetType.MEDIA, eUserAction.RATES,
                        startDate, endDate);
                    double votesSum = socialDal.GetRatesSum(assetId, eAssetType.MEDIA, startDate, endDate);
                    if (res.votes > 0)
                    {
                        res.rate = votesSum / res.votes;
                    }
                    else
                    {
                        res.rate = 0d;
                    }

                    break;
                }
                case StatsType.EPG:
                {
                    // in epg we bring just likes.
                    res.likesCounter = socialDal.GetAssetSocialActionCount(assetId, eAssetType.PROGRAM,
                        eUserAction.LIKE, startDate, endDate);
                    res.rate = 0d;
                    break;
                }
                default:
                {
                    break;
                }
            } // switch

            return res;
        }

        internal static List<long> GetEPGChannelsIDs(List<long> ipnoEPGChannelsMediaIDs)
        {
            List<long> res;
            DataTable dt = CatalogDAL.Get_EPGChannelsIDsByMediaIDs(ipnoEPGChannelsMediaIDs);
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                res = new List<long>(dt.Rows.Count);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    res.Add(ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[i]["ID"]));
                }
            }
            else
            {
                throw new Exception("No EPG Channel IDs extracted from DB");
            }

            return res;
        }

        internal static List<long> ExtractMediaIDs(SearchResultsObj sro)
        {
            List<long> res = new List<long>(sro.n_TotalItems);
            for (int i = 0; i < sro.n_TotalItems; i++)
            {
                res.Add(sro.m_resultIDs[i].assetID);
            }

            return res;
        }

        internal static MediaSearchObj BuildLinearChannelsMediaIDsRequest(int nGroupID,
            int domainId, string siteGuid,
            Dictionary<string, string> dict, List<List<string>> jsonizedChannelsDefinitions)
        {
            MediaSearchObj res = new MediaSearchObj();
            res.m_nGroupId = nGroupID;
            res.m_sMediaTypes = dict[LINEAR_MEDIA_TYPES_KEY];
            res.m_sPermittedWatchRules = dict[PERMITTED_WATCH_RULES_KEY];

            if (jsonizedChannelsDefinitions != null)
            {
                res.m_lChannelsDefinitionsMediaNeedsToBeInAtLeastOneOfIt = jsonizedChannelsDefinitions[0];
                res.m_lOrMediaNotInAnyOfTheseChannelsDefinitions = jsonizedChannelsDefinitions[1];
            }

            res.m_nPageIndex = 0;
            res.m_nPageSize = GetSearcherMaxResultsSize();

            List<int> regionIds;
            List<string> linearMediaTypes;
            bool doesGroupUsesTemplates = CatalogManagement.CatalogManager.Instance.DoesGroupUsesTemplates(nGroupID);

            CatalogLogic.SetSearchRegions(nGroupID, doesGroupUsesTemplates, domainId, siteGuid, out regionIds,
                out linearMediaTypes);

            res.regionIds = regionIds;
            res.linearChannelMediaTypes = linearMediaTypes;

            CatalogLogic.GetParentMediaTypesAssociations(nGroupID, out res.parentMediaTypes, out res.associationTags);

            return res;
        }

        private static int GetSearcherMaxResultsSize()
        {
            int res = ApplicationConfiguration.Current.ElasticSearchConfiguration.MaxResults.Value;

            if (res > 0)
                return res;
            return DEFAULT_SEARCHER_MAX_RESULTS_SIZE;
        }

        internal static Dictionary<string, string> GetLinearMediaTypeIDsAndWatchRuleIDs(int nGroupID)
        {
            Dictionary<string, string> res = new Dictionary<string, string>(2);

            DataSet ds = CatalogDAL.Get_DataForEPGIPNOSearcherRequest(nGroupID);
            if (ds != null && ds.Tables != null && ds.Tables.Count == 2)
            {
                res.Add(PERMITTED_WATCH_RULES_KEY, GetPermittedWatchRules(nGroupID, ds.Tables[0]));
                res.Add(LINEAR_MEDIA_TYPES_KEY, GetLinearMediaTypesSeperatedBySemiColon(ds.Tables[1]));
            }
            else
            {
                res.Add(LINEAR_MEDIA_TYPES_KEY, string.Empty);
                res.Add(PERMITTED_WATCH_RULES_KEY, string.Empty);
            }

            return res;
        }

        private static string GetLinearMediaTypesSeperatedBySemiColon(DataTable dt)
        {
            string res = string.Empty;
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                int length = dt.Rows.Count;
                for (int i = 0; i < length; i++)
                {
                    sb.Append(
                        String.Concat(i == 0 ? string.Empty : ";", ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["ID"])));
                }

                res = sb.ToString();
            }

            return res;
        }

        private static string GetFullSearchKey(string originalKey, ref Group group)
        {
            return CatalogLogic.GetFullSearchKey(originalKey, ref group, out _, out _);
        }

        private static string GetFullSearchKey(string originalKey, ref Group group, out bool isTagOrMeta,
            out eFieldType type)
        {
            type = eFieldType.Default;
            isTagOrMeta = false;

            string searchKey = originalKey;

            foreach (string tag in group.m_oGroupTags.Values)
            {
                if (tag.Equals(originalKey, StringComparison.OrdinalIgnoreCase))
                {
                    //searchKey = string.Concat(TAGS, ".", tag.ToLower());
                    type = eFieldType.Tag;
                    isTagOrMeta = true;
                    break;
                }
            }

            if (!isTagOrMeta)
            {
                var metas = group.m_oMetasValuesByGroupId.Select(i => i.Value).Cast<Dictionary<string, string>>()
                    .SelectMany(d => d.Values).ToList();

                foreach (var meta in metas)
                {
                    if (meta.Equals(originalKey, StringComparison.OrdinalIgnoreCase))
                    {
                        //searchKey = string.Concat(METAS, ".", meta.ToLower());
                        type = eFieldType.StringMeta;
                        isTagOrMeta = true;
                        break;
                    }
                }
            }

            string loweredKey = searchKey.ToLower();
            if (loweredKey == "name" || loweredKey == "description")
            {
                type = eFieldType.LanguageSpecificField;
            }

            return searchKey;
        }

        public static int GetLastMediaPosition(int mediaID, int userID)
        {
            if (mediaID == 0 || userID == 0)
                return 0;

            return CatalogDAL.GetLastMediaPosition(mediaID, userID);
        }

        internal static bool IsConcurrent(int groupId, ref DevicePlayData devicePlayData)
        {
            bool result = true;

            if (devicePlayData.UserId == 0)
            {
                // concurrency limitation does not apply for anonymous users.
                // anonymous user is identified by receiving UserId=0 from the clients.
                return false;
            }

            ValidationResponseObject domainsResp =
                Domains.Module.ValidateLimitationModule(groupId, 0, ValidationType.Concurrency, devicePlayData);

            if (domainsResp == null)
            {
                throw new Exception(GetIsConcurrentLogMsg("WS_Domains response is null.", devicePlayData, groupId));
            }

            devicePlayData.DomainId = (int) domainsResp.m_lDomainID;
            switch (domainsResp.m_eStatus)
            {
                case DomainResponseStatus.ConcurrencyLimitation:
                case DomainResponseStatus.MediaConcurrencyLimitation:
                {
                    result = true;
                    break;
                }
                case DomainResponseStatus.OK:
                {
                    result = false;
                    break;
                }
                default:
                {
                    throw new Exception(GetIsConcurrentLogMsg
                    (String.Concat("WS_Domains returned status: ", domainsResp.m_eStatus.ToString()),
                        devicePlayData, groupId));
                }
            }

            return result;
        }

        private static string GetIsConcurrentLogMsg(string message, DevicePlayData devicePlayData, int groupId)
        {
            StringBuilder sb = new StringBuilder("IsConcurrent Err. ");
            sb.Append(message);
            sb.Append(String.Concat(" User Id: ", devicePlayData.UserId));
            sb.Append(String.Concat(" UDID: ", devicePlayData.UDID));
            sb.Append(String.Concat(" Group ID: ", groupId));

            return sb.ToString();
        }

        internal static List<FileMedia> GetMediaFilesDetails(int groupID, List<int> mediaFileIDs,
            string mediaFileCoGuid)
        {
            List<FileMedia> res = null;
            GroupManager groupManager = new GroupManager();

            CatalogCache catalogCache = CatalogCache.Instance();
            int nParentGroupID = catalogCache.GetParentGroup(groupID);


            List<int> groupTreeVals = groupManager.GetSubGroup(nParentGroupID);

            DataTable dt = CatalogDAL.Get_MediaFilesDetails(groupTreeVals, mediaFileIDs, mediaFileCoGuid);
            List<Branding> brands = new List<Branding>();
            bool isSuccess = true;
            res = FilesValues(dt, ref brands, false, ref isSuccess);
            if (isSuccess && res != null)
                return res;
            return new List<FileMedia>(0);
        }

        private static bool IsBrand(DataRow dr)
        {
            return (!string.IsNullOrEmpty(Utils.GetStrSafeVal(dr, "BRAND_HEIGHT")) &&
                    !dr["BRAND_HEIGHT"].ToString().Equals("0"))
                   || (!string.IsNullOrEmpty(Utils.GetStrSafeVal(dr, "RECURRING_TYPE_ID")) &&
                       !dr["RECURRING_TYPE_ID"].ToString().Equals("0"));
        }

        /*Insert all files that return from the "CompleteDetailsForMediaResponse" into List<FileMedia>*/
        internal static List<FileMedia> FilesValues(DataTable dtFileMedia, ref List<Branding> lBranding, bool noFileUrl,
            ref bool result, bool managementData = false, Dictionary<int, List<string>> dicMediaFilePPVModules = null)
        {
            try
            {
                List<FileMedia> lFileMedia = null;
                result = true;
                if (dtFileMedia != null && dtFileMedia.Rows != null && dtFileMedia.Rows.Count > 0)
                {
                    lFileMedia = new List<FileMedia>(dtFileMedia.Rows.Count);
                    for (int i = 0; i < dtFileMedia.Rows.Count; i++)
                    {
                        if (IsBrand(dtFileMedia.Rows[i]))
                        {
                            Branding brand = new Branding();
                            brand.m_nFileId = Utils.GetIntSafeVal(dtFileMedia.Rows[i], "id");
                            brand.m_nDuration = Utils.GetIntSafeVal(dtFileMedia.Rows[i], "duration");
                            brand.m_sFileFormat = Utils.GetStrSafeVal(dtFileMedia.Rows[i], "foramtDescription");
                            brand.m_sUrl = Utils.GetStrSafeVal(dtFileMedia.Rows[i], "FileURL");
                            brand.m_nBrandHeight = Utils.GetIntSafeVal(dtFileMedia.Rows[i], "BRAND_HEIGHT");
                            brand.m_nRecurringTypeId = Utils.GetIntSafeVal(dtFileMedia.Rows[i], "RECURRING_TYPE_ID");
                            brand.m_sBillingType = Utils.GetStrSafeVal(dtFileMedia.Rows[i], "bill_type");
                            brand.m_nCdnID = Utils.GetIntSafeVal(dtFileMedia.Rows[i], "CdnID");
                            lBranding.Add(brand);
                        }
                        else
                        {
                            FileMedia fileMedia = new FileMedia();
                            int tempAdProvID = 0;
                            fileMedia.m_nFileId = Utils.GetIntSafeVal(dtFileMedia.Rows[i], "id");
                            fileMedia.m_nMediaID = ODBCWrapper.Utils.GetIntSafeVal(dtFileMedia.Rows[i]["MEDIA_ID"]);
                            fileMedia.m_nDuration = Utils.GetIntSafeVal(dtFileMedia.Rows[i], "duration");
                            fileMedia.m_sFileFormat = Utils.GetStrSafeVal(dtFileMedia.Rows[i], "foramtDescription");
                            fileMedia.m_sCoGUID = Utils.GetStrSafeVal(dtFileMedia.Rows[i], "co_guid");
                            fileMedia.m_sLanguage = Utils.GetStrSafeVal(dtFileMedia.Rows[i], "LANGUAGE");
                            fileMedia.m_nIsDefaultLanguage =
                                Utils.GetIntSafeVal(dtFileMedia.Rows[i], "IS_DEFAULT_LANGUAGE");
                            fileMedia.m_sAltCoGUID = Utils.GetStrSafeVal(dtFileMedia.Rows[i], "ALT_CO_GUID");
                            var catalogEndDate =
                                ODBCWrapper.Utils.GetNullableDateSafeVal(dtFileMedia.Rows[i], "CATALOG_END_DATE");
                            fileMedia.CatalogEndDate =
                                catalogEndDate.HasValue ? catalogEndDate.Value : new DateTime(2099, 1, 1);
                            if (noFileUrl)
                            {
                                fileMedia.m_sUrl = GetFictiveFileMediaUrl(fileMedia.m_nMediaID, fileMedia.m_nFileId);
                                fileMedia.m_sAltUrl = GetFictiveFileMediaUrl(fileMedia.m_nMediaID, fileMedia.m_nFileId);
                            }
                            else
                            {
                                fileMedia.m_sUrl = Utils.GetStrSafeVal(dtFileMedia.Rows[i], "FileURL");
                                fileMedia.m_sAltUrl = Utils.GetStrSafeVal(dtFileMedia.Rows[i], "ALT_FILE_URL");
                            }

                            fileMedia.m_sBillingType = Utils.GetStrSafeVal(dtFileMedia.Rows[i], "bill_type");
                            fileMedia.m_nCdnID = Utils.GetIntSafeVal(dtFileMedia.Rows[i], "CdnID");
                            fileMedia.m_nAltCdnID = Utils.GetIntSafeVal(dtFileMedia.Rows[i], "ALT_CDN_ID");
                            tempAdProvID = Utils.GetIntSafeVal(dtFileMedia.Rows[i], "COMMERCIAL_TYPE_PRE_ID");
                            if (tempAdProvID != 0)
                            {
                                fileMedia.m_oPreProvider = new AdProvider();
                                fileMedia.m_oPreProvider.ProviderID = tempAdProvID;
                                fileMedia.m_oPreProvider.ProviderName =
                                    Utils.GetStrSafeVal(dtFileMedia.Rows[i], "COMMERCIAL_TYPE_PRE_NAME");
                                fileMedia.m_bIsPreSkipEnabled =
                                    Utils.GetIntSafeVal(dtFileMedia.Rows[i], "OUTER_COMMERCIAL_SKIP_PRE") == 1;
                            }

                            tempAdProvID = Utils.GetIntSafeVal(dtFileMedia.Rows[i], "COMMERCIAL_TYPE_POST_ID");
                            if (tempAdProvID != 0)
                            {
                                fileMedia.m_oPostProvider = new AdProvider();
                                fileMedia.m_oPostProvider.ProviderID = tempAdProvID;
                                fileMedia.m_oPostProvider.ProviderName =
                                    Utils.GetStrSafeVal(dtFileMedia.Rows[i], "COMMERCIAL_TYPE_POST_NAME");
                                fileMedia.m_bIsPostSkipEnabled =
                                    Utils.GetIntSafeVal(dtFileMedia.Rows[i], "OUTER_COMMERCIAL_SKIP_POST") == 1;
                            }

                            tempAdProvID = Utils.GetIntSafeVal(dtFileMedia.Rows[i], "COMMERCIAL_TYPE_BREAK_ID");
                            if (tempAdProvID != 0)
                            {
                                fileMedia.m_oBreakProvider = new AdProvider();
                                fileMedia.m_oBreakProvider.ProviderID = tempAdProvID;
                                fileMedia.m_oBreakProvider.ProviderName =
                                    Utils.GetStrSafeVal(dtFileMedia.Rows[i], "COMMERCIAL_TYPE_BREAK_NAME");
                                fileMedia.m_sBreakpoints =
                                    Utils.GetStrSafeVal(dtFileMedia.Rows[i], "COMMERCIAL_BREAK_POINTS");
                            }

                            tempAdProvID = Utils.GetIntSafeVal(dtFileMedia.Rows[i], "COMMERCIAL_TYPE_OVERLAY_ID");
                            if (tempAdProvID != 0)
                            {
                                fileMedia.m_oOverlayProvider = new AdProvider();
                                fileMedia.m_oOverlayProvider.ProviderID = tempAdProvID;
                                fileMedia.m_oOverlayProvider.ProviderName =
                                    Utils.GetStrSafeVal(dtFileMedia.Rows[i], "COMMERCIAL_TYPE_OVERLAY_NAME");
                                fileMedia.m_sOverlaypoints =
                                    Utils.GetStrSafeVal(dtFileMedia.Rows[i], "COMMERCIAL_OVERLAY_POINTS");
                            }

                            if (managementData)
                            {
                                if (Utils.GetIntSafeVal(dtFileMedia.Rows[i], "MEDIA_QUALITY_ID") > 0)
                                {
                                    fileMedia.Quality =
                                        GetMediaQuality(Utils.GetIntSafeVal(dtFileMedia.Rows[i], "MEDIA_QUALITY_ID"));
                                }

                                fileMedia.HandlingType = "CLIP";
                                fileMedia.ProductCode = Utils.GetStrSafeVal(dtFileMedia.Rows[i], "Product_Code");
                                fileMedia.CdnCode = Utils.GetStrSafeVal(dtFileMedia.Rows[i], "STREAMING_CODE");
                                fileMedia.StreamingCompanyName =
                                    Utils.GetStrSafeVal(dtFileMedia.Rows[i], "STREAMING_COMPANY_NAME");
                                // call SP
                                if (dicMediaFilePPVModules.ContainsKey(fileMedia.m_nFileId))
                                    fileMedia.PPVModules = dicMediaFilePPVModules[fileMedia.m_nFileId];
                            }

                            fileMedia.FileSize = Utils.GetLongSafeVal(dtFileMedia.Rows[i], "FILE_SIZE");


                            lFileMedia.Add(fileMedia);
                        }
                    }
                }
                else
                {
                    lFileMedia = new List<FileMedia>(0);
                }

                return lFileMedia;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                result = false;
                return null;
            }
        }

        internal static bool IsAnonymousUser(string siteGuid)
        {
            int nSiteGuid = 0;
            return string.IsNullOrEmpty(siteGuid) || !Int32.TryParse(siteGuid, out nSiteGuid) || nSiteGuid == 0;
        }

        internal static bool IsAnonymousUser(string siteGuid, out int nSiteGuid)
        {
            nSiteGuid = 0;
            return string.IsNullOrEmpty(siteGuid) || !Int32.TryParse(siteGuid, out nSiteGuid) || nSiteGuid == 0;
        }

        internal static bool GetMediaMarkHitInitialData(string sSiteGuid, string userIP, int mediaID, int mediaFileID,
            ref int countryID,
            ref int ownerGroupID, ref int cdnID, ref int qualityID, ref int formatID, ref int mediaTypeID,
            ref int billingTypeID, ref int fileDuration, int groupId)
        {
            if (!ApplicationConfiguration.Current.CatalogLogicConfiguration.ShouldUseHitCache.Value)
            {
                countryID = Utils.GetIP2CountryId(groupId, userIP);
                return CatalogDAL.GetMediaPlayData(mediaID, mediaFileID, ref ownerGroupID, ref cdnID, ref qualityID,
                    ref formatID, ref mediaTypeID, ref billingTypeID, ref fileDuration);
            }

            #region try get values from catalog cache

            double cacheTime = ApplicationConfiguration.Current.CatalogLogicConfiguration.HitCacheTimeInMinutes.Value;

            if (cacheTime == 0)
            {
                cacheTime = 120d;
            }

            CatalogCache catalogCache = CatalogCache.Instance();
            string ipKey = string.Format("{0}_userIP_{1}", eWSModules.CATALOG, userIP);

            object oCountryID = catalogCache.Get(ipKey);

            bool bIP = false;
            bool bMedia = false;

            if (oCountryID != null)
            {
                countryID = (int) oCountryID;
                bIP = true;
            }

            string m_mf_Key = string.Format("{0}_media_{1}_mediaFile_{2}", eWSModules.CATALOG, mediaID, mediaFileID);
            List<KeyValuePair<string, int>> lMedia = catalogCache.Get<List<KeyValuePair<string, int>>>(m_mf_Key);

            if (lMedia != null && lMedia.Count > 0)
            {
                InitMediaMarkHitDataFromCache(ref ownerGroupID, ref cdnID, ref qualityID, ref formatID, ref mediaTypeID,
                    ref billingTypeID, ref fileDuration, lMedia);

                log.DebugFormat("GetMediaMarkHitInitialData, get media mark hit datafrom cache: " +
                                "cdnID {0}, qualityID {1}, formatId {2}, mediaTypeId {3}, billingTypeId {4}, fileDuration {5}",
                    cdnID, qualityID, formatID, mediaTypeID, billingTypeID, fileDuration);

                if (cdnID == -1 && qualityID == -1 && formatID == -1 && mediaTypeID == -1 && billingTypeID == -1 &&
                    fileDuration == -1)
                {
                    return false;
                }

                bMedia = true;
            }

            #endregion

            if (!bIP) // try getting countryID from ES, if it fails get countryID from DB
            {
                log.DebugFormat(
                    "GetMediaMarkHitInitialData, try getting countryID from ES, if it fails get countryID from DB");

                countryID = Utils.GetIP2CountryId(groupId, userIP);

                //getting from ES failed
                if (countryID == 0)
                {
                    log.DebugFormat("GetMediaMarkHitInitialData, getting country from ES failed, getting from DB");

                    long ipVal = 0;
                    ipVal = ParseIPOutOfString(userIP);

                    if (ipVal > 0)
                    {
                        CatalogDAL.Get_IPCountryCode(ipVal, ref countryID);
                    }
                }

                if (countryID > 0)
                {
                    log.DebugFormat("GetMediaMarkHitInitialData, setting countryId in cache");

                    catalogCache.Set(ipKey, countryID, cacheTime);
                    bIP = true;
                }
                else
                {
                    log.DebugFormat("GetMediaMarkHitInitialData, setting countryId in cache");
                }
            }

            if (!bMedia)
            {
                if (CatalogDAL.GetMediaPlayData(mediaID, mediaFileID, ref ownerGroupID, ref cdnID, ref qualityID,
                    ref formatID, ref mediaTypeID, ref billingTypeID, ref fileDuration))
                {
                    InitMediaMarkHitDataToCache(ownerGroupID, cdnID, qualityID, formatID, mediaTypeID, billingTypeID,
                        fileDuration, ref lMedia);

                    catalogCache.Set(m_mf_Key, lMedia, cacheTime);
                    bMedia = true;
                }
                else
                {
                    log.DebugFormat(
                        "GetMediaMarkHitInitialData, GetMediaPlayData failed, setting in cache invalid media");

                    // If couldn't get media - create an "invalid" record and save it in cache
                    InitMediaMarkHitDataToCache(ownerGroupID, -1, -1, -1, -1, -1, -1, ref lMedia);
                    catalogCache.Set(m_mf_Key, lMedia, cacheTime);
                    bMedia = false;
                }
            }
            else if (cdnID == -1 && qualityID == -1 && formatID == -1 && mediaTypeID == -1 && billingTypeID == -1 &&
                     fileDuration == -1)
            {
                bMedia = false;
            }

            return bIP && bMedia;
        }

        private static void InitMediaMarkHitDataFromCache(ref int ownerGroupID, ref int cdnID, ref int qualityID,
            ref int formatID, ref int mediaTypeID, ref int billingTypeID, ref int fileDuration,
            List<KeyValuePair<string, int>> lMedia)
        {
            foreach (KeyValuePair<string, int> item in lMedia)
            {
                switch (item.Key)
                {
                    case "ownerGroupID":
                        ownerGroupID = item.Value;
                        break;
                    case "cdnID":
                        cdnID = item.Value;
                        break;
                    case "qualityID":
                        qualityID = item.Value;
                        break;
                    case "formatID":
                        formatID = item.Value;
                        break;
                    case "mediaTypeID":
                        mediaTypeID = item.Value;
                        break;
                    case "billingTypeID":
                        billingTypeID = item.Value;
                        break;
                    case "fileDuration":
                        fileDuration = item.Value;
                        break;

                    default:
                        break;
                }
            }
        }

        private static void InitMediaMarkHitDataToCache(int ownerGroupID, int cdnID, int qualityID, int formatID,
            int mediaTypeID, int billingTypeID, int fileDuration, ref List<KeyValuePair<string, int>> lMedia)
        {
            lMedia = new List<KeyValuePair<string, int>>();
            lMedia.Add(new KeyValuePair<string, int>("ownerGroupID", ownerGroupID));
            lMedia.Add(new KeyValuePair<string, int>("cdnID", cdnID));
            lMedia.Add(new KeyValuePair<string, int>("qualityID", qualityID));
            lMedia.Add(new KeyValuePair<string, int>("formatID", formatID));
            lMedia.Add(new KeyValuePair<string, int>("mediaTypeID", mediaTypeID));
            lMedia.Add(new KeyValuePair<string, int>("billingTypeID", billingTypeID));
            lMedia.Add(new KeyValuePair<string, int>("fileDuration", fileDuration));
        }

        internal static bool GetNPVRMarkHitInitialData(long domainRecordingId, ref int fileDuration, int groupId,
            int domainId)
        {
            bool result = false;

            var recording =
                ConditionalAccess.Module.GetRecordingByDomainRecordingId(groupId, domainId, domainRecordingId);

            // Validate recording
            if (recording != null && recording.Status != null && recording.Status.Code == 0)
            {
                fileDuration = (int) ((recording.EpgEndDate - recording.EpgStartDate).TotalSeconds);

                result = true;
            }
            else
            {
                // if recording is invalid, still cache that this recording is invalid
                result = false;
            }

            return result;
        }

        private static long ParseIPOutOfString(string userIP)
        {
            long nIPVal = 0;

            if (!string.IsNullOrEmpty(userIP))
            {
                string[] splited = userIP.Split('.');

                if (splited != null && splited.Length >= 4)
                {
                    nIPVal = long.Parse(splited[3]) + Int64.Parse(splited[2]) * 256 +
                             Int64.Parse(splited[1]) * 256 * 256 + Int64.Parse(splited[0]) * 256 * 256 * 256;
                }
            }

            return nIPVal;
        }

        internal static int GetLastNpvrPosition(string NpvrID, int userID)
        {
            if (string.IsNullOrEmpty(NpvrID) || userID == 0)
                return 0;

            return CatalogDAL.GetLastNpvrPosition(NpvrID, userID);
        }

        /// <summary>
        /// This method return all last position (desc order by create date) by domain and \ or user_id
        /// if userType is household and user is default - return all last positions of all users in domain by assetID (BY MEDIA ID)
        /// else return last position of user_id (incase userType is not household or last position of user_id and default_user (incase userType is household)
        /// </summary>
        /// <param name="assetID"></param>
        /// <param name="assetType"></param>
        /// <param name="userID"></param>
        /// <param name="isDefaultUser"></param>
        /// <param name="users"></param>
        /// <param name="defaultUsers"></param>
        /// <param name="usersDictionary"></param>
        /// <returns></returns>
        internal static AssetBookmarks GetAssetLastPosition(int groupID, string assetID, eAssetTypes assetType,
            int userID, bool isDefaultUser, List<int> users,
            List<int> defaultUsers, Dictionary<string, User> usersDictionary)
        {
            AssetBookmarks response = null;

            // Build list of users that we want to get their last position
            List<int> usersToGetLastPosition = new List<int>();
            usersToGetLastPosition.AddRange(defaultUsers);
            if (isDefaultUser)
            {
                usersToGetLastPosition.AddRange(users);
            }
            else
            {
                usersToGetLastPosition.Add(userID);
            }

            // get last positions from catalog DAL
            DomainMediaMark domainMediaMark =
                CatalogDAL.GetAssetLastPosition(assetID, assetType, usersToGetLastPosition);
            if (domainMediaMark == null || domainMediaMark.devices == null)
            {
                return response;
            }

            List<Bookmark> bookmarks = new List<Bookmark>();

            if (domainMediaMark.devices != null)
            {
                int finishedPercentThreshold = CatalogLogic.FINISHED_PERCENT_THRESHOLD;
                var generalPartnerConfig = GeneralPartnerConfigManager.Instance.GetGeneralPartnerConfig(groupID);
                if (generalPartnerConfig != null && generalPartnerConfig.FinishedPercentThreshold.HasValue)
                {
                    finishedPercentThreshold = generalPartnerConfig.FinishedPercentThreshold.Value;
                }

                foreach (UserMediaMark userMediaMark in domainMediaMark.devices)
                {
                    eUserType userType;
                    if (defaultUsers.Contains(userMediaMark.UserID))
                    {
                        userType = eUserType.HOUSEHOLD;
                    }
                    else
                    {
                        userType = eUserType.PERSONAL;
                    }

                    if (!bookmarks.Any(x => x.User.m_sSiteGUID == userMediaMark.UserID.ToString()))
                    {
                        bool isFinished = userMediaMark.IsFinished(finishedPercentThreshold);
                        bookmarks.Add(new Bookmark(usersDictionary[userMediaMark.UserID.ToString()], userType,
                            userMediaMark.Location, isFinished));
                    }
                }
            }

            if (bookmarks.Count > 0)
                response = new AssetBookmarks(assetType, assetID, bookmarks.OrderBy(x => x.User.m_sSiteGUID));

            return response;
        }

        // copy-paste of GetAssetLastPosition, but receives a list of assets
        private static AssetBookmarkRequestEqualityComparer assetBookmarkRequestEqualityComparer = new AssetBookmarkRequestEqualityComparer();
        internal static IEnumerable<AssetBookmarks> GetAssetsLastPosition(
            int groupId,
            IReadOnlyCollection<AssetBookmarkRequest> assets,
            int userID,
            bool isDefaultUser,
            List<int> users,
            List<int> defaultUsers,
            Dictionary<string, User> usersDictionary)
        {
            // Build list of users that we want to get their last position
            List<int> usersToGetLastPosition = new List<int>(defaultUsers);
            if (isDefaultUser)
            {
                usersToGetLastPosition.AddRange(users);
            }
            else
            {
                usersToGetLastPosition.Add(userID);
            }

            int finishedPercentThreshold =
                GeneralPartnerConfigManager.Instance.GetGeneralPartnerConfig(groupId)?.FinishedPercentThreshold ??
                CatalogLogic.FINISHED_PERCENT_THRESHOLD;

            var assetToBookmarks = new Dictionary<AssetBookmarkRequest, List<Bookmark>>(assetBookmarkRequestEqualityComparer);
            foreach (var userId in usersToGetLastPosition.Distinct())
            {
                var userMediaMarks = GetUserMediaMarks(groupId, userId, numOfDays: 0);
                var user = usersDictionary[userId.ToString()];
                var userType = defaultUsers.Contains(userId)
                    ? eUserType.HOUSEHOLD
                    : eUserType.PERSONAL;
                foreach (var mediaMark in userMediaMarks)
                {
                    var asset = new AssetBookmarkRequest{ AssetType = mediaMark.AssetType, AssetID = mediaMark.AssetID.ToString() };
                    if (!assets.Contains(asset, assetBookmarkRequestEqualityComparer)) continue;

                    List<Bookmark> bookmarks;
                    if (!assetToBookmarks.TryGetValue(asset, out bookmarks))
                    {
                        bookmarks = new List<Bookmark>();
                        assetToBookmarks[asset] = bookmarks;
                    }
                    bookmarks.Add(new Bookmark(user, userType, mediaMark.Location, mediaMark.IsFinished(finishedPercentThreshold)));
                }
            }

            foreach (var kv in assetToBookmarks)
            {
                yield return new AssetBookmarks(kv.Key.AssetType, kv.Key.AssetID, kv.Value.OrderBy(x => x.User.m_sSiteGUID));
            }
        }

	internal static NPVRSeriesResponse GetSeriesRecordings(int groupID, NPVRSeriesRequest request,
            INPVRProvider npvr)
        {
            NPVRSeriesResponse nPVRSeriesResponse = new NPVRSeriesResponse();

            int domainID = 0;
            if (IsUserValid(request.m_sSiteGuid, groupID, ref domainID) && domainID > 0)
            {
                NPVRRetrieveSeriesResponse response = npvr.RetrieveSeries(new NPVRRetrieveParamsObj()
                {
                    EntityID = domainID.ToString(),
                    PageIndex = request.m_nPageIndex,
                    PageSize = request.m_nPageSize,
                    SeriesIDs = string.IsNullOrEmpty(request.seriesID)
                        ? new List<string>()
                        : new List<string>() {request.seriesID},
                    SeasonNumber = request.seasonNumber,
                    OrderBy = (NPVROrderBy) ((int) request.m_oOrderObj.m_eOrderBy),
                    Direction = (NPVROrderDir) ((int) request.m_oOrderObj.m_eOrderDir)
                });
                if (response != null)
                {
                    if (response.isOK)
                    {
                        nPVRSeriesResponse.totalItems = response.totalItems;
                        nPVRSeriesResponse.recordedSeries = response.results;
                    }
                    else
                    {
                        log.Error("Error - " +
                                  string.Format(
                                      "GetSeriesRecordings. NPVR layer returned errorneus response. Req: {0} , Resp Err Msg: {1}",
                                      request.ToString(), response.msg));
                        nPVRSeriesResponse.recordedSeries = new List<RecordedSeriesObject>(0);
                    }
                }
                else
                {
                    throw new Exception("NPVR layer returned response null.");
                }
            }
            else
            {
                throw new Exception("Either user is not valid or user has no domain.");
            }

            return nPVRSeriesResponse;
        }

        internal static List<RecordedEPGChannelProgrammeObject> GetRecordings(int groupID, NPVRRetrieveRequest request,
            INPVRProvider npvr)
        {
            List<RecordedEPGChannelProgrammeObject> res = null;

            int domainID = 0;
            if (IsUserValid(request.m_sSiteGuid, groupID, ref domainID) && domainID > 0)
            {
                NPVRRetrieveParamsObj args = new NPVRRetrieveParamsObj();
                args.PageIndex = request.m_nPageIndex;
                args.PageSize = request.m_nPageSize;
                args.EntityID = domainID.ToString();
                args.OrderBy = (NPVROrderBy) ((int) request.m_oOrderObj.m_eOrderBy);
                args.Direction = (NPVROrderDir) ((int) request.m_oOrderObj.m_eOrderDir);
                switch (request.m_eNPVRSearchBy)
                {
                    case NPVRSearchBy.ByStartDate:
                        args.StartDate = request.m_dtStartDate;
                        args.SearchBy.Add(SearchByField.byStartTime);
                        break;
                    case NPVRSearchBy.ByRecordingStatus:
                        args.RecordingStatus.AddRange(request.m_lRecordingStatuses.Distinct()
                            .Select((item) => (NPVRRecordingStatus) ((int) item)));
                        args.SearchBy.Add(SearchByField.byStatus);
                        break;
                    case NPVRSearchBy.ByRecordingID:
                        args.AssetIDs.AddRange(request.m_lRecordingIDs.Distinct());
                        args.SearchBy.Add(SearchByField.byAssetId);
                        break;
                    default:
                        break;
                }

                if (request.m_nEPGChannelID > 0)
                {
                    args.EpgChannelID = request.m_nEPGChannelID.ToString();
                    args.SearchBy.Add(SearchByField.byChannelId);
                }

                if (request.m_lProgramIDs != null && request.m_lProgramIDs.Count > 0)
                {
                    List<EPGChannelProgrammeObject> epgs = GetEpgsByGroupAndIDs(groupID, request.m_lProgramIDs);
                    if (epgs != null && epgs.Count > 0)
                    {
                        // get all linear settings about channel + group
                        GetLinearChannelSettings(groupID, epgs);

                        args.EpgProgramIDs.AddRange(epgs.Select((item) => item.EPG_IDENTIFIER));
                        args.SearchBy.Add(SearchByField.byProgramId);
                    }
                    else
                    {
                        log.Error("Error - " +
                                  string.Format("GetRecordings. No epgs returned from CB for the request: {0}",
                                      request.ToString()));
                    }
                }

                if (request.m_lSeriesIDs != null && request.m_lSeriesIDs.Count > 0)
                {
                    args.SeriesIDs.AddRange(request.m_lSeriesIDs.Distinct());
                    args.SearchBy.Add(SearchByField.bySeriesId);
                }

                if (!string.IsNullOrEmpty(request.timeFormat))
                {
                    args.TimeFormat = request.timeFormat;
                }

                NPVRRetrieveAssetsResponse npvrResp = npvr.RetrieveAssets(args);
                if (npvrResp != null)
                {
                    res = npvrResp.results;
                }
                else
                {
                    throw new Exception("NPVR layer returned response null.");
                }
            }
            else
            {
                throw new Exception("Either user is not valid or user has no domain.");
            }

            return res;
        }

        internal static bool IsUserValid(string siteGuid, int groupID, ref int domainID)
        {
            long temp = 0;
            if (!Int64.TryParse(siteGuid, out temp) || temp < 1)
            {
                return false;
            }

            UserResponseObject resp = Core.Users.Module.GetUserData(groupID, siteGuid, string.Empty);
            if (resp != null && resp.m_RespStatus == ResponseStatus.OK && resp.m_user != null &&
                resp.m_user.m_domianID > 0)
            {
                domainID = resp.m_user.m_domianID;
                return true;
            }
            else
            {
                return false;
            }
        }

        internal static DomainResponse GetDomain(int domainID, int groupID)
        {
            DomainResponse domainResponse = null;
            if (domainID <= 0 || groupID <= 0)
            {
                return domainResponse;
            }

            try
            {
                var domainRes = Core.Domains.Module.GetDomainInfo(groupID, domainID);
                if (domainRes != null)
                {
                    domainResponse = domainRes;
                }
            }
            catch (Exception ex)
            {
                log.Error(
                    "GetDomain - " + string.Format("Failed ex={0}, domainID={1}, groupdID={2}", ex.Message, domainID,
                        groupID), ex);
            }

            return domainResponse;
        }

        internal static Dictionary<string, User> GetUsers(int groupID, List<int> users)
        {
            Dictionary<string, User> usersDictionary = new Dictionary<string, User>();
            UsersResponse usersResponse = null;
            Credentials oCredentials = TvinciCache.WSCredentials.GetWSCredentials(ApiObjects.eWSModules.CATALOG,
                groupID, ApiObjects.eWSModules.USERS);

            if (string.IsNullOrEmpty(oCredentials.m_sUsername) || string.IsNullOrEmpty(oCredentials.m_sPassword))
            {
                return usersDictionary;
            }

            usersResponse =
                Core.Users.Module.GetUsers(groupID, users.Select(i => i.ToString()).ToArray(), string.Empty);
            if (usersResponse != null && usersResponse.resp != null &&
                usersResponse.resp.Code == (int) ResponseStatus.OK && usersResponse.users != null)
            {
                foreach (UserResponseObject user in usersResponse.users)
                {
                    if (user != null && user.m_RespStatus == ResponseStatus.OK)
                    {
                        if (!usersDictionary.ContainsKey(user.m_user.m_sSiteGUID))
                        {
                            usersDictionary.Add(user.m_user.m_sSiteGUID, user.m_user);
                        }
                    }
                }
            }

            return usersDictionary;
        }

        internal static void BuildEpgUrlPicture(ref List<EPGChannelProgrammeObject> retList, int groupID)
        {
            string epgPicBaseUrl = string.Empty;
            string epgPicWidth = string.Empty;
            string epgPicHeight = string.Empty;
            Dictionary<int, List<string>> groupTreeEpgPicUrl = CatalogDAL.Get_GroupTreePicEpgUrl(groupID);
            GetEpgPicUrlData(retList, groupTreeEpgPicUrl, ref epgPicBaseUrl, ref epgPicWidth, ref epgPicHeight);
            MutateFullEpgPicURL(retList, epgPicBaseUrl, epgPicWidth, epgPicHeight);
        }

        /// <summary>
        /// Finds out the region for search, according to the domain and/or group, and gets the linear channels of those regions.
        /// </summary>
        /// <param name="searcherEpgSearch"></param>
        /// <param name="epgSearchRequest"></param>
        internal static void SetEpgSearchChannelsByRegions(ref EpgSearchObj searcherEpgSearch,
            EpgSearchRequest epgSearchRequest)
        {
            List<long> channelIds = null;
            List<int> regionIds;
            List<string> linearMediaTypes;
            bool doesGroupUsesTemplates =
                CatalogManagement.CatalogManager.Instance.DoesGroupUsesTemplates(epgSearchRequest.m_nGroupID);

            // Get region/regions for search
            CatalogLogic.SetSearchRegions(epgSearchRequest.m_nGroupID, doesGroupUsesTemplates,
                epgSearchRequest.domainId,
                epgSearchRequest.m_sSiteGuid, out regionIds, out linearMediaTypes);

            // Ask Stored procedure for EPG Identifier of linear channel in current region(s), by joining media and media_regions
            channelIds = CatalogDAL.Get_EpgIdentifier_ByRegion(epgSearchRequest.m_nGroupID, regionIds);

            searcherEpgSearch.m_oEpgChannelIDs = new List<long>(channelIds);
        }

        public static bool SendRebuildIndexMessage(int groupId, eObjectType type, bool switchIndexAlias,
            bool deleteOldIndices,
            DateTime? startDate = null, DateTime? endDate = null)
        {
            bool result = false;

            try
            {
                GroupManager groupManager = new GroupManager();

                CatalogCache catalogCache = CatalogCache.Instance();
                int parentGroupId = catalogCache.GetParentGroup(groupId);

                Group group = groupManager.GetGroup(parentGroupId);

                if (group != null)
                {
                    ApiObjects.CeleryIndexBuildingData data = new CeleryIndexBuildingData(group.m_nParentGroupID,
                        type, switchIndexAlias, deleteOldIndices, startDate, endDate);

                    var queue = new CatalogQueue();

                    result = queue.Enqueue(data,
                        string.Format(@"Tasks\{0}\{1}", group.m_nParentGroupID, type.ToString()));
                }
            }
            catch (Exception ex)
            {
                result = false;
                log.ErrorFormat("Failed sending message to queue on rebuilding index: group id = {0}, ex = {1}",
                    groupId, ex);
            }

            return result;
        }

        public static bool SendRebaseIndexMessage(int groupId, eObjectType type, DateTime date)
        {
            bool result = false;

            try
            {
                GroupManager groupManager = new GroupManager();

                CatalogCache catalogCache = CatalogCache.Instance();
                int parentGroupId = catalogCache.GetParentGroup(groupId);

                Group group = groupManager.GetGroup(parentGroupId);

                if (group != null)
                {
                    ApiObjects.CeleryIndexingData data = new CeleryIndexingData(group.m_nParentGroupID,
                        new List<long>(), type,
                        eAction.Rebase, date);

                    var queue = new CatalogQueue();

                    result = queue.Enqueue(data,
                        string.Format(@"Tasks\{0}\{1}", group.m_nParentGroupID, type.ToString()));
                }
            }
            catch (Exception ex)
            {
                result = false;
                log.ErrorFormat("Failed sending message to queue on rebasing index: group id = {0}, ex = {1}", groupId,
                    ex);
            }

            return result;
        }

        #region External Channel Request

        internal static ApiObjects.Response.Status GetExternalChannelAssets(ExternalChannelRequest request,
            out int totalItems, out List<UnifiedSearchResult> searchResultsList, out string requestId)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status();

            searchResultsList = new List<UnifiedSearchResult>();
            totalItems = 0;
            requestId = "";

            var externalChannelsCache = ExternalChannelCache.Instance();

            // If no internal ID provided - get the internal ID by the external identifier
            if (string.IsNullOrEmpty(request.internalChannelID))
            {
                int internalId =
                    CatalogDAL.GetExternalChannelIdByExternalIdentifier(request.m_nGroupID, request.externalChannelID);

                if (internalId > 0)
                {
                    request.internalChannelID = internalId.ToString();
                }
                else
                {
                    status.Code = (int) eResponseStatus.ExternalChannelNotExist;
                    status.Message = string.Format("External Channel with the external identifier {0} was not found.",
                        request.externalChannelID);
                    return status;
                }
            }

            ExternalChannel externalChannel =
                externalChannelsCache.GetChannel(request.m_nGroupID, request.internalChannelID);

            if (externalChannel == null || externalChannel.ID <= 0)
            {
                status.Code = (int) eResponseStatus.ExternalChannelNotExist;
                status.Message = string.Format("External Channel with the ID {0} was not found.",
                    request.internalChannelID);
                return status;
            }

            log.DebugFormat("GetExternalChannelAssets - external channel is: {0}",
                Newtonsoft.Json.JsonConvert.SerializeObject(externalChannel));

            // Build dictionary of enrichments for recommendation engine adapter
            Dictionary<string, string> enrichments = CatalogLogic.GetEnrichments(request, externalChannel.Enrichments);

            // If no recommendation engine defined - use group's default
            var group = new GroupManager().GetGroup(request.m_nGroupID);
            if (externalChannel.RecommendationEngineId <= 0)
            {
                externalChannel.RecommendationEngineId = group.defaultRecommendationEngine;
            }

            // If there is still no recommendation engine
            if (externalChannel.RecommendationEngineId <= 0)
            {
                status.Code = (int) eResponseStatus.ExternalChannelHasNoRecommendationEngine;
                status.Message = "External Channel has no recommendation engine selected.";
                return status;
            }

            // Adapter will respond with a collection of media assets ID with Kaltura terminology
            List<RecommendationResult> recommendations =
                RecommendationAdapterController.GetInstance().GetChannelRecommendations(externalChannel, enrichments,
                    request.free, out requestId,
                    request.m_nPageIndex, request.m_nPageSize, out totalItems);

            if (recommendations == null)
            {
                status.Code = (int) (eResponseStatus.AdapterAppFailure);
                status.Message = "No recommendations received";
                return status;
            }

            if (recommendations.Count == 0)
            {
                return status;
            }

            // If there is no filter - no need to go to Searcher, just page the results list, fill update date and return it to client
            if (string.IsNullOrEmpty(externalChannel.FilterExpression) && string.IsNullOrEmpty(request.filterQuery))
            {
                searchResultsList = GetValidateRecommendationsAssets(recommendations, request, group);

                if (totalItems == 0 ||
                    (recommendations.Count != searchResultsList.Count &&
                     totalItems <= recommendations.Count)) //BEO-10244
                {
                    totalItems = searchResultsList.Count;
                }
            }
            // If there is, go to ES and perform further filter
            else
            {
                // Build boolean phrase tree based on filter expression
                BooleanPhraseNode channelFilterTree = null;
                BooleanPhraseNode requestFilterTree = null;
                BooleanPhraseNode filterTree = null;

                // Parse filter expression of the external channel
                if (!string.IsNullOrEmpty(externalChannel.FilterExpression))
                {
                    externalChannel.FilterExpression = HttpUtility.HtmlDecode(externalChannel.FilterExpression);

                    status = BooleanPhraseNode.ParseSearchExpression(externalChannel.FilterExpression,
                        ref channelFilterTree);

                    if (status.Code != (int) eResponseStatus.OK)
                    {
                        return status;
                    }
                }

                // Parse filter expression of the client request
                if (!string.IsNullOrEmpty(request.filterQuery))
                {
                    request.filterQuery = HttpUtility.HtmlDecode(request.filterQuery);

                    status = BooleanPhraseNode.ParseSearchExpression(request.filterQuery, ref requestFilterTree);

                    if (status.Code != (int) eResponseStatus.OK)
                    {
                        return status;
                    }
                }

                // Connect the two filter's with an AND, if they both exist.
                // If only one exists, use only it.
                if (channelFilterTree != null)
                {
                    if (requestFilterTree != null)
                    {
                        List<BooleanPhraseNode> nodes = new List<BooleanPhraseNode>()
                        {
                            channelFilterTree,
                            requestFilterTree
                        };

                        filterTree = new BooleanPhrase(nodes, eCutType.And);
                    }
                    else
                    {
                        filterTree = channelFilterTree;
                    }
                }
                else if (requestFilterTree != null)
                {
                    filterTree = requestFilterTree;
                }

                // Group have user types per media  +  siteGuid != empty
                if (!string.IsNullOrEmpty(request.m_sSiteGuid) &&
                    Utils.IsGroupIDContainedInConfig(request.m_nGroupID,
                        ApplicationConfiguration.Current.CatalogLogicConfiguration
                            .GroupsWithIUserTypeSeperatedBySemiColon.Value, ';'))
                {
                    if (request.m_oFilter == null)
                    {
                        request.m_oFilter = new Filter();
                    }

                    //call ws_users to get userType
                    request.m_oFilter.m_nUserTypeID = Utils.GetUserType(request.m_sSiteGuid, request.m_nGroupID);
                }

                UnifiedSearchDefinitions searchDefinitions =
                    BuildUnifiedSearchObject(request, externalChannel, filterTree);

                searchDefinitions.specificAssets = new Dictionary<eAssetTypes, List<string>>();

                // Map recommendations to dictionary of search definitions
                foreach (var recommendation in recommendations)
                {
                    if (!searchDefinitions.specificAssets.ContainsKey(recommendation.type))
                    {
                        searchDefinitions.specificAssets[recommendation.type] = new List<string>();
                    }

                    searchDefinitions.specificAssets[recommendation.type].Add(recommendation.id);
                }

                // Map order of IDs
                searchDefinitions.specificOrder = recommendations.Select(item => long.Parse(item.id)).ToList();

                int parentGroupId = CatalogCache.Instance().GetParentGroup(request.m_nGroupID);
                var indexManager = IndexManagerFactory.Instance.GetIndexManager(parentGroupId);

                // The provided response should be filtered according to the Filter defined in the applicable 3rd-party channel settings
                List<UnifiedSearchResult> searchResults = indexManager.UnifiedSearch(searchDefinitions, ref totalItems);

                if (searchResults != null)
                {
                    searchResultsList = searchResults;

                    List<RecommendationResult> recommendationResults = searchResultsList.Select(result =>
                        new RecommendationResult()
                        {
                            id = result.AssetId,
                            type = result.AssetType
                        }).ToList();

                    // After applying the filter - the recommendation engine should be reported back with the remaining result set
                    // async query - no response is expected from the recommendation engine
                    RecommendationAdapterController.GetInstance()
                        .ShareFilteredResponse(externalChannel, recommendationResults);
                }
            }

            return status;
        }

        private static List<UnifiedSearchResult> GetValidateRecommendationsAssets(List<RecommendationResult> recommendations, BaseRequest request, Group group)
        {
            var searchResultsList = new List<UnifiedSearchResult>();
            var groupPermittedWatchRules = WatchRuleManager.Instance.GetGroupPermittedWatchRules(request.m_nGroupID);

            bool isOPC = CatalogManager.Instance.DoesGroupUsesTemplates(request.m_nGroupID);
            if (isOPC || (groupPermittedWatchRules != null && groupPermittedWatchRules.Count > 0))
            {
                string watchRules = string.Join(" ", WatchRuleManager.Instance.GetGroupPermittedWatchRules(request.m_nGroupID));

                // validate media on ES
                UnifiedSearchDefinitions searchDefinitions = new UnifiedSearchDefinitions()
                {
                    groupId = request.m_nGroupID,
                    permittedWatchRules = watchRules,
                    specificAssets = new Dictionary<eAssetTypes, List<string>>(),
                    shouldUseFinalEndDate = true,
                    shouldUseStartDateForMedia = true,
                    shouldAddIsActiveTerm = true,
                    shouldIgnoreDeviceRuleID = true,
                    shouldUseSearchEndDate = true,
                    shouldUseEndDateForEpg = false,
                    shouldUseStartDateForEpg = false
                };

                var shopUserId = request.GetCallerUserId();
                if (shopUserId > 0)
                {
                    UnifiedSearchDefinitionsBuilder.GetUserAssetRulesPhrase(request, group, ref searchDefinitions, request.m_nGroupID, shopUserId);
                }

                int elasticSearchPageSize = 0;
                var recommendationsMapping = recommendations.GroupBy(x => x.type)
                    .ToDictionary(x => x.Key, x => x.ToDictionary(y => y.id, y => y.TagsExtarData));

                if (recommendationsMapping.ContainsKey(eAssetTypes.MEDIA) &&
                    recommendationsMapping[eAssetTypes.MEDIA].Count > 0)
                {
                    searchDefinitions.specificAssets.Add(eAssetTypes.MEDIA,
                        recommendationsMapping[eAssetTypes.MEDIA].Keys.ToList());
                    searchDefinitions.shouldSearchMedia = true;
                    elasticSearchPageSize += recommendationsMapping[eAssetTypes.MEDIA].Count;
                }

                if (recommendationsMapping.ContainsKey(eAssetTypes.EPG) &&
                    recommendationsMapping[eAssetTypes.EPG].Count > 0)
                {
                    searchDefinitions.specificAssets.Add(eAssetTypes.EPG,
                        recommendationsMapping[eAssetTypes.EPG].Keys.ToList());
                    searchDefinitions.shouldSearchEpg = true;
                    elasticSearchPageSize += recommendationsMapping[eAssetTypes.EPG].Count;
                }

                searchDefinitions.pageSize = elasticSearchPageSize;

                var searchResultsMapping = new Dictionary<eAssetTypes, Dictionary<string, DateTime>>();

                if (elasticSearchPageSize > 0)
                {
                    int parentGroupId = CatalogCache.Instance().GetParentGroup(request.m_nGroupID);
                    var indexManager = IndexManagerFactory.Instance.GetIndexManager(parentGroupId);
                    int esTotalItems = 0;
                    var searchResults = indexManager.UnifiedSearch(searchDefinitions, ref esTotalItems);

                    if (searchResults != null && searchResults.Count > 0)
                    {
                        searchResultsMapping = searchResults.GroupBy(x => x.AssetType).ToDictionary(x => x.Key,
                            x => x.ToDictionary(y => y.AssetId, y => y.m_dUpdateDate));
                    }
                }

                foreach (var recommendation in recommendations)
                {
                    if (recommendation.type == eAssetTypes.NPVR)
                    {
                        searchResultsList.Add(new RecommendationSearchResult
                        {
                            AssetId = recommendation.id,
                            AssetType = recommendation.type,
                            TagsExtraData = recommendation.TagsExtarData
                        });
                    }
                    else if (searchResultsMapping.ContainsKey(recommendation.type) &&
                             searchResultsMapping[recommendation.type].ContainsKey(recommendation.id))
                    {
                        searchResultsList.Add(new RecommendationSearchResult
                        {
                            AssetId = recommendation.id,
                            AssetType = recommendation.type,
                            m_dUpdateDate = searchResultsMapping[recommendation.type][recommendation.id],
                            TagsExtraData = recommendation.TagsExtarData
                        });
                    }
                }
            }

            return searchResultsList;
        }

        internal static ApiObjects.Response.Status GetExternalRelatedAssets(MediaRelatedExternalRequest request,
            out int totalItems, out List<RecommendationResult> resultsList, out string requestId)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status();
            totalItems = 0;
            requestId = "";
            resultsList = new List<RecommendationResult>();

            BaseResponse respone = new BaseResponse();

            GroupManager groupManager = new GroupManager();

            Group group = groupManager.GetGroup(request.m_nGroupID);
            if (group != null)
            {
                int recommendationEngineId = group.RelatedRecommendationEngine;

                if (recommendationEngineId == 0)
                {
                    status.Message = "Recommendation Engine Not Exist";
                    status.Code = (int) eResponseStatus.RecommendationEngineNotExist;
                    return status;
                }

                int mediaTypeID = CatalogLogic.GetMediaTypeID(request.m_nMediaID);
                if (mediaTypeID == 0)
                {
                    status.Message = "Asset doesn't exist";
                    status.Code = (int) eResponseStatus.BadSearchRequest;
                    return status;
                }

                List<ExternalRecommendationEngineEnrichment> enrichmentsToSend =
                    new List<ExternalRecommendationEngineEnrichment>();

                foreach (int currentValue in Enum.GetValues(typeof(ExternalRecommendationEngineEnrichment)))
                {
                    if ((group.RelatedRecommendationEngineEnrichments & currentValue) > 0)
                    {
                        enrichmentsToSend.Add((ExternalRecommendationEngineEnrichment) currentValue);
                    }
                }

                Dictionary<string, string> enrichments = CatalogLogic.GetEnrichments(request, enrichmentsToSend);

                List<RecommendationResult> recommendations = null;

                try
                {
                    recommendations =
                        RecommendationAdapterController.GetInstance().GetRelatedRecommendations(recommendationEngineId,
                            request.m_nMediaID,
                            mediaTypeID,
                            request.m_nGroupID,
                            request.m_sSiteGuid,
                            request.m_oFilter.m_sDeviceId,
                            request.m_sLanguage,
                            request.m_nUtcOffset,
                            request.m_sUserIP,
                            request.m_sSignature,
                            request.m_sSignString,
                            request.m_nMediaTypes,
                            request.m_nPageSize,
                            request.m_nPageIndex,
                            enrichments,
                            request.m_sFreeParam,
                            out requestId,
                            out totalItems);
                }
                catch (KalturaException ex)
                {
                    if ((int) ex.Data["StatusCode"] == (int) eResponseStatus.RecommendationEngineNotExist)
                    {
                        status.Message = "Recommendation Engine Not Exist";
                        status.Code = (int) eResponseStatus.RecommendationEngineNotExist;
                    }

                    if ((int) ex.Data["StatusCode"] == (int) eResponseStatus.AdapterUrlRequired)
                    {
                        status.Message = "Recommendation engine adapter has no URL";
                        status.Code = (int) eResponseStatus.AdapterUrlRequired;
                    }
                    else
                    {
                        status.Message = "Adapter failed completing request";
                        status.Code = (int) eResponseStatus.AdapterAppFailure;
                    }

                    return status;
                }

                resultsList = recommendations;
                //totalItems = recommendations.Count;

                if (recommendations == null)
                {
                    status.Code = (int) (eResponseStatus.AdapterAppFailure);
                    status.Message = "No recommendations received";
                    return status;
                }

                if (recommendations.Count == 0)
                {
                    return status;
                }
            }

            return status;
        }

        internal static ApiObjects.Response.Status GetExternalSearchAssets(MediaSearchExternalRequest request,
            out int totalItems, out List<UnifiedSearchResult> searchResultsList, out string requestId)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status();
            totalItems = 0;
            requestId = "";
            searchResultsList = new List<UnifiedSearchResult>();

            BaseResponse respone = new BaseResponse();

            GroupManager groupManager = new GroupManager();

            Group group = groupManager.GetGroup(request.m_nGroupID);
            if (group != null)
            {
                int recommendationEngineId = group.SearchRecommendationEngine;

                if (recommendationEngineId == 0)
                {
                    status.Message = "Recommendation Engine Not Exist";
                    status.Code = (int) eResponseStatus.RecommendationEngineNotExist;
                    return status;
                }

                List<ExternalRecommendationEngineEnrichment> enrichmentsToSend =
                    new List<ExternalRecommendationEngineEnrichment>();

                foreach (int currentValue in Enum.GetValues(typeof(ExternalRecommendationEngineEnrichment)))
                {
                    if ((group.SearchRecommendationEngineEnrichments & currentValue) > 0)
                    {
                        enrichmentsToSend.Add((ExternalRecommendationEngineEnrichment) currentValue);
                    }
                }

                Dictionary<string, string> enrichments = CatalogLogic.GetEnrichments(request, enrichmentsToSend);
                List<RecommendationResult> recommendations = null;
                try
                {
                    recommendations =
                        RecommendationAdapterController.GetInstance().GetSearchRecommendations(recommendationEngineId,
                            request.m_sQuery,
                            request.m_nGroupID,
                            request.m_sSiteGuid,
                            request.m_oFilter.m_sDeviceId,
                            request.m_sLanguage,
                            request.m_nUtcOffset,
                            request.m_sUserIP,
                            request.m_sSignature,
                            request.m_sSignString,
                            request.m_nMediaTypes,
                            request.m_nPageSize,
                            request.m_nPageIndex,
                            enrichments,
                            out requestId,
                            out totalItems);
                }
                catch (KalturaException ex)
                {
                    if ((int) ex.Data["StatusCode"] == (int) eResponseStatus.RecommendationEngineNotExist)
                    {
                        status.Message = "Recommendation Engine Not Exist";
                        status.Code = (int) eResponseStatus.RecommendationEngineNotExist;
                    }

                    if ((int) ex.Data["StatusCode"] == (int) eResponseStatus.AdapterUrlRequired)
                    {
                        status.Message = "Recommendation engine adapter has no URL";
                        status.Code = (int) eResponseStatus.AdapterUrlRequired;
                    }
                    else
                    {
                        status.Message = "Adapter failed completing request";
                        status.Code = (int) eResponseStatus.AdapterAppFailure;
                    }

                    return status;
                }

                if (recommendations == null)
                {
                    status.Code = (int) (eResponseStatus.AdapterAppFailure);
                    status.Message = "No recommendations received";
                    return status;
                }

                if (recommendations.Count == 0)
                {
                    return status;
                }

                searchResultsList = GetValidateRecommendationsAssets(recommendations, request, group);

                if (totalItems == 0 ||
                    (recommendations.Count != searchResultsList.Count &&
                     totalItems <= recommendations.Count)) //BEO-10244
                {
                    totalItems = searchResultsList.Count;
                }
            }

            return status;
        }

        /// <summary>
        /// Builds a dictionary of enrichments for recommendation engine adapter
        /// </summary>
        /// <param name="request"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetEnrichments(BaseRequest request,
            List<ExternalRecommendationEngineEnrichment> list)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            foreach (ExternalRecommendationEngineEnrichment enrichment in list)
            {
                switch (enrichment)
                {
                    case ExternalRecommendationEngineEnrichment.ClientLocation:
                    {
                        try
                        {
                            string coutnryCode = APILogic.Utils.GetIP2CountryCode(request.m_nGroupID, request.m_sUserIP);

                            dictionary["client_location"] = coutnryCode;
                        }
                        catch (Exception ex)
                        {
                            log.ErrorFormat("Failed getting country by IP. IP = {0}", request.m_sUserIP, ex);
                            dictionary["client_location"] = "0";
                        }

                        break;
                    }
                    case ExternalRecommendationEngineEnrichment.UserId:
                    {
                        dictionary["user_id"] = request.m_sSiteGuid;
                        break;
                    }
                    case ExternalRecommendationEngineEnrichment.HouseholdId:
                    {
                        dictionary["household_id"] = request.domainId.ToString();
                        break;
                    }
                    case ExternalRecommendationEngineEnrichment.DeviceId:
                    {
                        if (request is ExternalChannelRequest)
                        {
                            dictionary["device_id"] = (request as ExternalChannelRequest).deviceId;
                        }

                        if (request is MediaRelatedExternalRequest)
                        {
                            dictionary["device_id"] = (request as MediaRelatedExternalRequest).m_sDeviceID;
                        }

                        if (request is MediaSearchExternalRequest)
                        {
                            dictionary["device_id"] = (request as MediaSearchExternalRequest).m_sDeviceID;
                        }

                        break;
                    }
                    case ExternalRecommendationEngineEnrichment.DeviceType:
                    {
                        if (request is ExternalChannelRequest)
                        {
                            dictionary["device_type"] = (request as ExternalChannelRequest).deviceType;
                        }

                        break;
                    }
                    case ExternalRecommendationEngineEnrichment.UTCOffset:
                    {
                        if (request is ExternalChannelRequest)
                        {
                            dictionary["utc_offset"] = (request as ExternalChannelRequest).utcOffset;
                        }

                        if (request is MediaRelatedExternalRequest)
                        {
                            dictionary["utc_offset"] = (request as MediaRelatedExternalRequest).m_nUtcOffset.ToString();
                        }

                        if (request is MediaSearchExternalRequest)
                        {
                            dictionary["utc_offset"] = (request as MediaSearchExternalRequest).m_nUtcOffset.ToString();
                        }

                        break;
                    }
                    case ExternalRecommendationEngineEnrichment.Language:
                    {
                        // If requested specific language - use it. Otherwise, group's default
                        LanguageObj objLang = null;

                        if (request.m_oFilter == null)
                        {
                            objLang = GetLanguage(request.m_nGroupID, -1);
                        }
                        else
                        {
                            objLang = GetLanguage(request.m_nGroupID, request.m_oFilter.m_nLanguage);
                        }

                        if (objLang != null)
                        {
                            dictionary["language"] = objLang.Code;
                        }

                        break;
                    }
                    //case ExternalRecommendationEngineEnrichment.NPVRSupport:
                    //    {
                    //        if (request is ExternalChannelRequest)
                    //        {
                    //            log.ErrorFormat("GetEnrichments - channel {0} has unsupported enirchment {1} / {2} defined",
                    //                (request as ExternalChannelRequest).internalChannelID, (int)enrichment, enrichment.ToString());
                    //        }
                    //        break;
                    //    }
                    //case ExternalRecommendationEngineEnrichment.Catchup:
                    //    {
                    //        if (request is ExternalChannelRequest)
                    //        {
                    //            log.ErrorFormat("GetEnrichments - channel {0} has unsupported enirchment {1} / {2} defined",
                    //                (request as ExternalChannelRequest).internalChannelID, (int)enrichment, enrichment.ToString());
                    //        }

                    //        break;
                    //    }
                    //case ExternalRecommendationEngineEnrichment.Parental:
                    //    {
                    //        if (request is ExternalChannelRequest)
                    //        {
                    //            log.ErrorFormat("GetEnrichments - channel {0} has unsupported enirchment {1} / {2} defined",
                    //                (request as ExternalChannelRequest).internalChannelID, (int)enrichment, enrichment.ToString());
                    //        }
                    //        break;
                    //    }
                    case ExternalRecommendationEngineEnrichment.DTTRegion:
                        {
                            // External ID of region of current domain

                            GroupManager manager = new GroupManager();
                            Group group = manager.GetGroup(request.m_nGroupID);

                            if (group != null && group.isRegionalizationEnabled)
                            {
                                int regionId = GetRegionIdOfUser(request.m_nGroupID, request.domainId, request.m_sSiteGuid, group.defaultRegion);

                                DataRow regionRow =
                                ODBCWrapper.Utils.GetTableSingleRow("linear_channels_regions", regionId);

                                if (regionRow != null)
                                {
                                    dictionary["region"] = ODBCWrapper.Utils.ExtractString(regionRow, "EXTERNAL_ID");
                                }
                            }

                            break;
                        }
                    //case ExternalRecommendationEngineEnrichment.AtHome:
                    //    {
                    //        if (request is ExternalChannelRequest)
                    //        {
                    //            log.ErrorFormat("GetEnrichments - channel {0} has unsupported enirchment {1} / {2} defined",
                    //                (request as ExternalChannelRequest).internalChannelID, (int)enrichment, enrichment.ToString());
                    //        }

                    //        break;
                    //    }
                    default:
                    {
                        if (request is ExternalChannelRequest)
                        {
                            log.ErrorFormat("GetEnrichments - channel {0} has unsupported enirchment {1} / {2} defined",
                                (request as ExternalChannelRequest).internalChannelID, (int) enrichment,
                                enrichment.ToString());
                        }

                        break;
                    }
                }
            }

            return dictionary;
        }

        /// <summary>
        /// Because unified search works only with Unified Search Request, we convert the External Channel Request to make it work
        /// </summary>
        /// <param name="request"></param>
        /// <param name="externalChannel"></param>
        /// <param name="filterTree"></param>
        /// <returns></returns>
        private static UnifiedSearchDefinitions BuildUnifiedSearchObject(ExternalChannelRequest request,
            ExternalChannel externalChannel, BooleanPhraseNode filterTree)
        {
            UnifiedSearchRequest alternateRequest = new UnifiedSearchRequest(request.m_nPageSize, request.m_nPageIndex,
                request.m_nGroupID, string.Empty, string.Empty, null, null, externalChannel.FilterExpression,
                string.Empty,
                filterTree);

            UnifiedSearchDefinitions definitions = BuildUnifiedSearchObject(alternateRequest);

            // Order is a new kind - "recommendation". Which means the order is predefined
            definitions.order = new OrderObj
            {
                m_eOrderBy = OrderBy.RECOMMENDATION,
                m_eOrderDir = OrderDir.ASC
            };

            definitions.orderByFields = new List<IEsOrderByField>
            {
                new EsOrderByField(OrderBy.RECOMMENDATION, OrderDir.ASC)
            };

            return definitions;
        }

        #endregion

        #region Internal Channel Request

        internal static ApiObjects.Response.Status GetInternalChannelAssets(InternalChannelRequest request,
            out int totalItems, out List<UnifiedSearchResult> searchResults,
            out List<AggregationsResult> aggregationsResult)
        {
            // Set default values for out parameters
            totalItems = 0;
            aggregationsResult = null;
            searchResults = new List<UnifiedSearchResult>();
            aggregationsResult = null;

            ApiObjects.Response.Status status = null;
            GroupsCacheManager.Channel channel = null;
            int channelId = int.Parse(request.internalChannelID);
            int parentGroupID = request.m_nGroupID;
            Group group = null;
            CatalogGroupCache catalogGroupCache = null;
            if (CatalogManager.Instance.DoesGroupUsesTemplates(request.m_nGroupID))
            {
                var contextData = new ContextData(request.m_nGroupID) { UserId = request.GetCallerUserId() };
                GenericResponse<GroupsCacheManager.Channel> response = ChannelManager.Instance.GetChannelById(contextData, channelId, request.isAllowedToViewInactiveAssets);
                if (response != null && response.Status != null && response.Status.Code != (int) eResponseStatus.OK)
                {
                    return response.Status;
                }

                channel = response.Object;
                if (!CatalogManagement.CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(request.m_nGroupID,
                    out catalogGroupCache))
                {
                    log.ErrorFormat(
                        "failed to get catalogGroupCache for groupId: {0} when calling GetInternalChannelAssets",
                        request.m_nGroupID);
                    return new ApiObjects.Response.Status((int) eResponseStatus.Error,
                        "failed to get catalogGroupCache");
                }
            }
            else
            {
                // Get group and channel objects from cache/DB
                GroupManager groupManager = new GroupsCacheManager.GroupManager();
                CatalogCache catalogCache = CatalogCache.Instance();
                parentGroupID = catalogCache.GetParentGroup(request.m_nGroupID);
                if (string.IsNullOrEmpty(request.internalChannelID))
                {
                    return new ApiObjects.Response.Status((int) eResponseStatus.Error,
                        "Internal Channel ID was not provided");
                }

                groupManager.GetGroupAndChannel(channelId, parentGroupID, ref group, ref channel);
            }

            // continue pnly for existing active channel or with inactive channels only for operators
            if (channel == null || (!request.isAllowedToViewInactiveAssets && channel.m_nIsActive != 1))
            {
                return new ApiObjects.Response.Status((int) eResponseStatus.ObjectNotExist,
                    string.Format("Channel with identifier {1} does not exist for group {0}", parentGroupID,
                        channelId));
            }

            // Build search object
            UnifiedSearchDefinitions unifiedSearchDefinitions =
                BuildInternalChannelSearchObject(channel, request, group, parentGroupID, catalogGroupCache, false);

            UnifiedSearchResponse searchResult;

            var channelUpdateDate = channel.UpdateDate.HasValue ? DateUtils.DateTimeToUtcUnixTimestampSeconds(channel.UpdateDate.Value) : 0;

            string cacheKey = GetChannelSearchCacheKey(parentGroupID, request.internalChannelID, request.m_sSiteGuid,
                request.domainId, request.m_oFilter.m_sDeviceId, request.m_sUserIP, request.filterQuery,
                unifiedSearchDefinitions, unifiedSearchDefinitions.PersonalData, request.m_oFilter.m_nLanguage, channelUpdateDate);
            if (!string.IsNullOrEmpty(cacheKey))
            {
                log.DebugFormat("Going to get channel assets from cache with key: {0}", cacheKey);
                searchResult =
                    GetUnifiedSearchChannelResultsFromCache(parentGroupID, unifiedSearchDefinitions, channel, cacheKey);
            }
            else
            {
                log.DebugFormat("Going to get channel assets from ES");
                searchResult = ChannelUnifiedSearch(parentGroupID, unifiedSearchDefinitions, channel);
            }

            status = searchResult.status;
            searchResults = searchResult.searchResults;
            totalItems = searchResult.m_nTotalItems;
            aggregationsResult = searchResult.aggregationResults;

            return status;
        }

        private static UnifiedSearchResponse ChannelUnifiedSearch(int groupId,
            UnifiedSearchDefinitions unifiedSearchDefinitions, GroupsCacheManager.Channel channel)
        {
            var parentGroupId = CatalogCache.Instance().GetParentGroup(groupId);
            IIndexManager indexManager = IndexManagerFactory.Instance.GetIndexManager(parentGroupId);

            if (indexManager == null)
                return new UnifiedSearchResponse {status = Status.ErrorMessage("Failed getting instance of searcher")};

            if (unifiedSearchDefinitions.groupBy?.Count == 1 // only one group by
                && unifiedSearchDefinitions.groupBy.Single().Key ==
                unifiedSearchDefinitions.distinctGroup.Key) // distinct is on
            {
                parentGroupId = parentGroupId == 0 ? groupId : parentGroupId;
                var aggregation = indexManager.UnifiedSearchForGroupBy(unifiedSearchDefinitions);

                return new UnifiedSearchResponse
                {
                    aggregationResults = new List<AggregationsResult> {aggregation},
                    m_nTotalItems = aggregation.totalItems,
                    status = Status.Ok
                };
            }

            // Perform initial search of channel
            int totalItems = 0;
            var searchResults =
                indexManager.UnifiedSearch(unifiedSearchDefinitions, ref totalItems, out var aggregationsResult);

            if (searchResults == null)
                return new UnifiedSearchResponse {status = Status.ErrorMessage("Failed performing channel search")};
            if (totalItems == 0)
            {
                return new UnifiedSearchResponse
                {
                    searchResults = searchResults,
                    m_nTotalItems = totalItems,
                    aggregationResults = aggregationsResult,
                    status = Status.Ok
                };
            }

            return new UnifiedSearchResponse
            {
                searchResults = searchResults,
                m_nTotalItems = totalItems,
                aggregationResults = aggregationsResult,
                status = Status.Ok
            };
        }

        private static UnifiedSearchResponse GetUnifiedSearchChannelResultsFromCache(int groupId,
            UnifiedSearchDefinitions unifiedSearchDefinitions, GroupsCacheManager.Channel channel, string cacheKey)
        {
            UnifiedSearchResponse cachedResult = new UnifiedSearchResponse()
            {
                status = new ApiObjects.Response.Status((int) eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            if (!LayeredCache.Instance.Get<UnifiedSearchResponse>(cacheKey, ref cachedResult,
                GetChannelUnifiedSearchResults,
                new Dictionary<string, object>()
                {
                    {"groupId", groupId}, {"unifiedSearchDefinitions", unifiedSearchDefinitions}, {"channel", channel}
                },
                groupId, LayeredCacheConfigNames.UNIFIED_SEARCH_WITH_PERSONAL_DATA))
            {
                log.ErrorFormat("Failed getting unified search results from LayeredCache, key: {0}", cacheKey);
            }

            return cachedResult;
        }

        private static Tuple<UnifiedSearchResponse, bool> GetChannelUnifiedSearchResults(
            Dictionary<string, object> funcParams)
        {
            bool result = false;
            UnifiedSearchResponse cachedResult = new UnifiedSearchResponse()
            {
                status = new ApiObjects.Response.Status((int) eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            try
            {
                if (funcParams != null && funcParams.Count == 3)
                {
                    if (funcParams.ContainsKey("groupId") && funcParams.ContainsKey("unifiedSearchDefinitions") &&
                        funcParams.ContainsKey("channel"))
                    {
                        int? groupId = funcParams["groupId"] as int?;
                        UnifiedSearchDefinitions unifiedSearchDefinitions =
                            funcParams["unifiedSearchDefinitions"] as UnifiedSearchDefinitions;
                        GroupsCacheManager.Channel channel = funcParams["channel"] as GroupsCacheManager.Channel;

                        if (groupId.HasValue && unifiedSearchDefinitions != null && channel != null)
                        {
                            cachedResult = ChannelUnifiedSearch(groupId.Value, unifiedSearchDefinitions, channel);

                            if (cachedResult.status.Code == (int) eResponseStatus.OK)
                            {
                                result = true;
                            }
                            else
                            {
                                log.ErrorFormat("Failed to get search results from ES, code = {0}, message = {1}",
                                    cachedResult.status.Code, cachedResult.status.Message);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(
                    string.Format("GetChannelUnifiedSearchResults failed params : {0}",
                        string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<UnifiedSearchResponse, bool>(cachedResult, result);
        }

        private static string GetDeviceRulesCacheKey(int groupId, int domainId, string udid)
        {
            if (string.IsNullOrEmpty(udid))
            {
                return null;
            }

            List<int> deviceRules = Api.api.GetDeviceAllowedRuleIDs(groupId, udid, domainId);

            if (deviceRules == null || deviceRules.Count == 0)
            {
                return null;
            }

            var deviceRulesCacheVal = string.Join("|", deviceRules.OrderBy(r => r));
            if (deviceRulesCacheVal.Length > MD5_HASH_SIZE_BYTES)
            {
                try
                {
                    deviceRulesCacheVal = EncryptUtils.HashMD5(deviceRulesCacheVal);
                }
                catch (Exception ex)
                {
                    log.Error("Failed to MD5 Device Rules for personal ES cache", ex);
                    throw;
                }
            }
            return deviceRulesCacheVal;
        }

        private static string GetChannelSearchCacheKey(int groupId, string channelId, string userId, int domainId,
            string udid, string ip, string ksql, UnifiedSearchDefinitions unifiedSearchDefinitions,
            List<string> personalData, int langId, long lastUpdateDate)
        {
            string key = null;
            if (LayeredCache.Instance.ShouldGoToCache(LayeredCacheConfigNames.UNIFIED_SEARCH_WITH_PERSONAL_DATA,
                groupId))
            {
                if ((personalData != null && personalData.Count > 0) || !string.IsNullOrEmpty(ksql) ||
                    (unifiedSearchDefinitions.assetUserRuleIds != null &&
                     unifiedSearchDefinitions.assetUserRuleIds.Count > 0))
                {
                    StringBuilder cacheKey = new StringBuilder();
                    cacheKey.AppendFormat("ch={0}", channelId);
                    cacheKey.AppendFormat("_gId={0}", groupId);
                    cacheKey.Append($"_l={langId}");  //BEO-11556

                    if (unifiedSearchDefinitions.groupBy != null && unifiedSearchDefinitions.groupBy.Any())
                    {
                        var groupByKey = string.Join(",", unifiedSearchDefinitions.groupBy.Select(_ => $"{_.Key}|{_.Type}|{_.Value}"));
                        cacheKey
                            .AppendFormat("_gb={0}", groupByKey)
                            .AppendFormat("_gbo={0}", unifiedSearchDefinitions.GroupByOption);
                    }

                    if (lastUpdateDate > 0)  //BEO-11618
                    {
                        cacheKey.Append($"_ud={lastUpdateDate}");
                    }

                    cacheKey.AppendFormat("_p={0}|{1}", unifiedSearchDefinitions.pageIndex,
                            unifiedSearchDefinitions.pageSize);
                    cacheKey.AppendFormat("_or={0}|{1}", unifiedSearchDefinitions.order.m_eOrderBy,
                        unifiedSearchDefinitions.order.m_eOrderDir);
                    if (unifiedSearchDefinitions.order.m_eOrderBy == OrderBy.META)
                    {
                        cacheKey.AppendFormat("|{0}", unifiedSearchDefinitions.order.m_sOrderValue);
                        // IRA: what else with ordering
                    }

                    try
                    {
                        var deviceRulesCacheVal = GetDeviceRulesCacheKey(groupId, domainId, udid);
                        if (deviceRulesCacheVal != null)
                        {
                            cacheKey.AppendFormat("_dr={0}", deviceRulesCacheVal);
                        }
                    }
                    catch (Exception e)
                    {
                        return key;
                    }

                    if (personalData.Contains(NamingHelper.ENTITLED_ASSETS_FIELD))
                    {
                        UserPurhcasedAssetsResponse purchasedAssets =
                            ConditionalAccess.Module.GetUserPurchasedAssets(groupId, domainId, null);
                        if (purchasedAssets.status.Code == (int) eResponseStatus.OK)
                        {
                            if (purchasedAssets.assets != null && purchasedAssets.assets.Count > 0)
                            {
                                return key;
                            }
                            else
                            {
                                UserBundlesResponse bundelsResponse =
                                    ConditionalAccess.Module.GetUserBundles(groupId, domainId, null);
                                if (bundelsResponse.status.Code == (int) eResponseStatus.OK)
                                {
                                    cacheKey.AppendFormat("_e={0}",
                                        bundelsResponse.channels != null && bundelsResponse.channels.Count > 0
                                            ? string.Join("|", bundelsResponse.channels.OrderBy(c => c))
                                            : "0");
                                }
                            }
                        }
                    }

                    if (personalData.Contains(NamingHelper.GEO_BLOCK_FIELD))
                    {
                        int countryId = Utils.GetIP2CountryId(groupId, ip);
                        cacheKey.AppendFormat("_cId={0}", countryId);
                    }

                    if (personalData.Contains(NamingHelper.PARENTAL_RULES_FIELD))
                    {
                        Dictionary<long, eRuleLevel> ruleIds = null;

                        string parentalRulesKey = LayeredCacheKeys.GetUserParentalRulesKey(groupId, userId);
                        bool parentalRulesCacheResult = LayeredCache.Instance.Get<Dictionary<long, eRuleLevel>>(
                            parentalRulesKey, ref ruleIds,
                            APILogic.Utils.GetUserParentalRules,
                            new Dictionary<string, object>() {{"groupId", groupId}, {"userId", userId}},
                            groupId, LayeredCacheConfigNames.USER_PARENTAL_RULES_LAYERED_CACHE_CONFIG_NAME,
                            new List<string>() {LayeredCacheKeys.GetUserParentalRuleInvalidationKey(groupId, userId)});

                        if (parentalRulesCacheResult && ruleIds != null && ruleIds.Count > 0)
                        {
                            cacheKey.AppendFormat("_pr={0}", string.Join("|", ruleIds.Keys.OrderBy(r => r)));
                        }
                    }

                    if (!string.IsNullOrEmpty(ksql))
                    {
                        string ksqlMd5 = null;
                        try
                        {
                            ksqlMd5 = EncryptUtils.HashMD5(ksql);
                        }
                        catch (Exception ex)
                        {
                            log.Error("Failed to MD5 KSQL for personal ES cache", ex);
                            return key;
                        }

                        cacheKey.AppendFormat("_ksql={0}", ksqlMd5);
                    }

                    if (unifiedSearchDefinitions.assetUserRuleIds != null &&
                        unifiedSearchDefinitions.assetUserRuleIds.Count > 0)
                    {
                        cacheKey.AppendFormat("_aur={0}",
                            string.Join("|", unifiedSearchDefinitions.assetUserRuleIds.OrderBy(r => r)));
                    }

                    if ((unifiedSearchDefinitions.regionIds?.Any()).GetValueOrDefault())
                    {
                        cacheKey.Append($"_rgs={string.Join("|", unifiedSearchDefinitions.regionIds.OrderBy(r => r))}");
                    }

                    key = cacheKey.ToString();
                }
            }

            return key;
        }

        private string GetSearchCacheKey(int groupId, string userId, int domainId, string udid, string ip,
            List<int> assetTypes, string ksql, UnifiedSearchDefinitions unifiedSearchDefinitions,
            List<string> personalData, int langId)
        {
            string key = null;
            if (LayeredCache.Instance.ShouldGoToCache(LayeredCacheConfigNames.UNIFIED_SEARCH_WITH_PERSONAL_DATA,
                groupId))
            {
                if (personalData != null && personalData.Count > 0)
                {
                    StringBuilder cacheKey = new StringBuilder("search");
                    cacheKey.AppendFormat("_gId={0}", groupId);
                    cacheKey.AppendFormat($"_l={langId}"); //BEO-11556
                    cacheKey.AppendFormat("_p={0}|{1}", unifiedSearchDefinitions.pageIndex, unifiedSearchDefinitions.pageSize);
                    cacheKey.Append(BuildOrderCacheKeyPart(unifiedSearchDefinitions));

                    if (unifiedSearchDefinitions.groupBy != null && unifiedSearchDefinitions.groupBy.Any())
                    {
                        var groupByKey = string.Join(",", unifiedSearchDefinitions.groupBy.Select(_ => $"{_.Key}|{_.Type}|{_.Value}"));
                        cacheKey.AppendFormat("_gb={0}", groupByKey);
                    }

                    if (assetTypes != null && assetTypes.Count > 0)
                    {
                        cacheKey.AppendFormat("_t={0}", string.Join("|", assetTypes.OrderBy(at => at)));
                    }

                    try
                    {
                        var deviceRulesCacheVal = GetDeviceRulesCacheKey(groupId, domainId, udid);
                        if (deviceRulesCacheVal != null)
                        {
                            cacheKey.AppendFormat("_dr={0}", deviceRulesCacheVal);
                        }
                    }
                    catch (Exception e)
                    {
                        return key;
                    }

                    if (personalData.Contains(NamingHelper.ENTITLED_ASSETS_FIELD))
                    {
                        UserPurhcasedAssetsResponse purchasedAssets =
                            ConditionalAccess.Module.GetUserPurchasedAssets(groupId, domainId, null);
                        if (purchasedAssets.status.Code == (int) eResponseStatus.OK)
                        {
                            if (purchasedAssets.assets != null && purchasedAssets.assets.Count > 0)
                            {
                                return key;
                            }
                            else
                            {
                                UserBundlesResponse bundelsResponse =
                                    ConditionalAccess.Module.GetUserBundles(groupId, domainId, null);
                                if (bundelsResponse.status.Code == (int) eResponseStatus.OK)
                                {
                                    string entitlementsMd5 = null;
                                    try
                                    {
                                        entitlementsMd5 = EncryptUtils.HashMD5(
                                            bundelsResponse.channels != null && bundelsResponse.channels.Count > 0
                                                ? string.Join("|", bundelsResponse.channels.OrderBy(c => c))
                                                : "0");
                                    }
                                    catch (Exception ex)
                                    {
                                        log.Error("Failed to entitlements KSQL for personal ES cache", ex);
                                        return key;
                                    }

                                    cacheKey.Append($"_e={entitlementsMd5}");
                                }
                            }
                        }
                    }

                    if (personalData.Contains(NamingHelper.GEO_BLOCK_FIELD))
                    {
                        int countryId = Utils.GetIP2CountryId(groupId, ip);
                        cacheKey.AppendFormat("_cId={0}", countryId);
                    }

                    if (personalData.Contains(NamingHelper.PARENTAL_RULES_FIELD))
                    {
                        Dictionary<long, eRuleLevel> ruleIds = null;

                        string parentalRulesKey = LayeredCacheKeys.GetUserParentalRulesKey(groupId, userId);
                        bool parentalRulesCacheResult = LayeredCache.Instance.Get<Dictionary<long, eRuleLevel>>(
                            parentalRulesKey, ref ruleIds,
                            APILogic.Utils.GetUserParentalRules,
                            new Dictionary<string, object>() {{"groupId", groupId}, {"userId", userId}},
                            groupId, LayeredCacheConfigNames.USER_PARENTAL_RULES_LAYERED_CACHE_CONFIG_NAME,
                            new List<string>() {LayeredCacheKeys.GetUserParentalRuleInvalidationKey(groupId, userId)});

                        if (parentalRulesCacheResult && ruleIds != null && ruleIds.Count > 0)
                        {
                            cacheKey.AppendFormat("_pr={0}", string.Join("|", ruleIds.Keys.OrderBy(r => r)));
                        }
                    }

                    if (!string.IsNullOrEmpty(ksql))
                    {
                        string ksqlMd5 = null;
                        try
                        {
                            ksqlMd5 = EncryptUtils.HashMD5(ksql);
                        }
                        catch (Exception ex)
                        {
                            log.Error("Failed to MD5 KSQL for personal ES cache", ex);
                            return key;
                        }

                        cacheKey.AppendFormat("_ksql={0}", ksqlMd5);
                    }

                    if (unifiedSearchDefinitions.assetUserRuleIds != null &&
                        unifiedSearchDefinitions.assetUserRuleIds.Count > 0)
                    {
                        cacheKey.AppendFormat("_aur={0}",
                            string.Join("|", unifiedSearchDefinitions.assetUserRuleIds.OrderBy(r => r)));
                    }

                    key = cacheKey.ToString();
                }
            }

            return key;
        }

        private string BuildOrderCacheKeyPart(UnifiedSearchDefinitions definitions)
        {
            var esOrderByFields = _sortingAdapter.ResolveOrdering(definitions);
            var pagingCacheKeyParts = new List<string>();
            foreach (var orderByField in esOrderByFields)
            {
                var pagingCacheKeyPart = new StringBuilder(BuildOrderByFieldPart(orderByField));
                if (orderByField is EsOrderByMetaField orderByMetaField)
                {
                    pagingCacheKeyPart.Append($"|{orderByMetaField.MetaName}");
                }

                pagingCacheKeyParts.Add(pagingCacheKeyPart.ToString());
                // IRA: what else with ordering
            }

            return $"_order={string.Join("|", pagingCacheKeyParts)}";
        }

        private static string BuildOrderByFieldPart(IEsOrderByField esOrderByField)
        {
            switch (esOrderByField)
            {
                case EsOrderByField orderByField:
                    return $"{orderByField.OrderByField}|{orderByField.OrderByDirection}";
                case EsOrderBySlidingWindow orderBySlidingWindowField:
                    return $"{orderBySlidingWindowField.OrderByField}" +
                        $"|{orderBySlidingWindowField.OrderByDirection}" +
                        $"|{orderBySlidingWindowField.SlidingWindowPeriod}";
                case EsOrderByStatisticsField orderByStatisticsField:
                    return $"{orderByStatisticsField.OrderByField}" +
                        $"|{orderByStatisticsField.OrderByDirection}" +
                        $"|{orderByStatisticsField.TrendingAssetWindow}";
                case EsOrderByMetaField orderByMetaField:
                    return $"{OrderBy.META}" +
                        $"|{orderByMetaField.OrderByDirection}" +
                        $"|{orderByMetaField.MetaName}";
                case EsOrderByStartDateAndAssociationTags _:
                    return $"{OrderBy.START_DATE}|{esOrderByField.OrderByDirection}";
                default:
                    return string.Empty;
            }
        }

        internal static Status GetRelatedAssets(MediaRelatedRequest request, out int totalItems,
            out List<UnifiedSearchResult> searchResults, out List<AggregationsResult> aggregationsResults)
        {
            // Set default values for out parameters
            aggregationsResults = null;
            totalItems = 0;
            searchResults = new List<UnifiedSearchResult>();

            ApiObjects.Response.Status status = null;
            bool doesGroupUsesTemplates =
                CatalogManagement.CatalogManager.Instance.DoesGroupUsesTemplates(request.m_nGroupID);
            Group group = null;
            int parentGroupID = request.m_nGroupID;

            if (!doesGroupUsesTemplates)
            {
                // Get group and channel objects from cache/DB

                GroupManager groupManager = new GroupsCacheManager.GroupManager();
                CatalogCache catalogCache = CatalogCache.Instance();
                parentGroupID = catalogCache.GetParentGroup(request.m_nGroupID);
                group = groupManager.GetGroup(parentGroupID);
            }

            // Build search object
            UnifiedSearchDefinitions unifiedSearchDefinitions = null;
            status = BuildRelatedObject(request, group, out unifiedSearchDefinitions, parentGroupID,
                doesGroupUsesTemplates);

            if (status.Code != (int) eResponseStatus.OK)
            {
                return status;
            }

            int pageIndex = request.m_nPageIndex;
            int pageSize = request.m_nPageSize;

            IIndexManager indexManager = IndexManagerFactory.Instance.GetIndexManager(parentGroupID);

            if (indexManager == null)
            {
                return new ApiObjects.Response.Status((int) eResponseStatus.Error,
                    "Failed getting instance of searcher");
            }

            // Perform initial search of channel
            searchResults =
                indexManager.UnifiedSearch(unifiedSearchDefinitions, ref totalItems, out aggregationsResults);

            if (searchResults == null)
            {
                return new ApiObjects.Response.Status((int) eResponseStatus.Error,
                    "Failed performing related assets search");
            }

            List<int> assetIDs = searchResults.Select(item => int.Parse(item.AssetId)).ToList();

            if (assetIDs == null)
            {
                searchResults = null;
                totalItems = 0;
                return new ApiObjects.Response.Status((int) eResponseStatus.Error,
                    "Failed performing related assets search");
            }

            status = new ApiObjects.Response.Status((int) eResponseStatus.OK);

            return status;
        }

        private static ApiObjects.Response.Status BuildRelatedObject(MediaRelatedRequest request, Group group,
            out UnifiedSearchDefinitions definitions, int groupId, bool doesGroupUsesTemplates)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status()
                {Code = (int) eResponseStatus.Error, Message = eResponseStatus.Error.ToString()};
            definitions = new UnifiedSearchDefinitions();
            definitions.shouldSearchEpg = false;
            definitions.shouldSearchMedia = true; // related media search MEDIA ONLY
            definitions.GroupByOption = request.isGroupingOptionInclude ? GroupingOption.Include : GroupingOption.Omit;

            Filter filter = new Filter();
            CatalogGroupCache catalogGroupCache = null;
            if (doesGroupUsesTemplates &&
                !CatalogManagement.CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId,
                    out catalogGroupCache))
            {
                log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling BuildRelatedObject",
                    groupId);
                return status;
            }

            bool bIsMainLang = doesGroupUsesTemplates
                ? catalogGroupCache.GetDefaultLanguage().ID == request.m_oFilter.m_nLanguage
                : Utils.IsLangMain(request.m_nGroupID, request.m_oFilter.m_nLanguage);

            MediaSearchRequest mediaSearchRequest = BuildMediasRequest(request.m_nMediaID, bIsMainLang,
                request.m_oFilter, ref filter, request.m_nGroupID, request.m_nMediaTypes, request.m_sSiteGuid,
                doesGroupUsesTemplates, catalogGroupCache);

            if (mediaSearchRequest == null)
            {
                return new ApiObjects.Response.Status((int) ApiObjects.Response.eResponseStatus.AssetDoseNotExists,
                    ApiObjects.Response.eResponseStatus.AssetDoseNotExists.ToString());
            }

            LanguageObj language = null;

            if (filter == null)
            {
                language = GetLanguage(request.m_nGroupID, -1);
            }
            else
            {
                language = GetLanguage(request.m_nGroupID, filter.m_nLanguage);
            }

            definitions.langauge = language;

            #region Basic

            definitions.groupId = request.m_nGroupID;
            definitions.indexGroupId = doesGroupUsesTemplates ? groupId : group.m_nParentGroupID;

            definitions.pageIndex = request.m_nPageIndex;
            definitions.pageSize = request.m_nPageSize;

            #endregion

            #region Excluded Media

            // Exclude the original media from the search
            if (request.m_nMediaID > 0)
            {
                definitions.excludedAssets = new Dictionary<eAssetTypes, List<string>>();
                definitions.excludedAssets[eAssetTypes.MEDIA] = new List<string>();
                definitions.excludedAssets[eAssetTypes.MEDIA].Add(request.m_nMediaID.ToString());
            }

            #endregion

            #region Device Rules

            int[] deviceRules = null;

            if (request.m_oFilter != null)
            {
                deviceRules = Api.api
                    .GetDeviceAllowedRuleIDs(request.m_nGroupID, request.m_oFilter.m_sDeviceId, request.domainId)
                    .ToArray();
            }

            definitions.deviceRuleId = deviceRules;

            #endregion

            #region Media Types, Permitted Watch Rules, Language

            // BEO-1338: Related media types is from the Media Search Request object - it knows the best!
            definitions.mediaTypes = mediaSearchRequest.m_nMediaTypes;


            if (!doesGroupUsesTemplates && group.m_sPermittedWatchRules != null &&
                group.m_sPermittedWatchRules.Count > 0)
            {
                definitions.permittedWatchRules = string.Join(" ", group.m_sPermittedWatchRules);
            }

            #endregion

            #region Request Filter Object

            if (request.m_oFilter != null)
            {
                definitions.shouldUseStartDateForMedia = request.m_oFilter.m_bUseStartDate;
                definitions.shouldUseCatalogStartDateForMedia = doesGroupUsesTemplates;
                definitions.shouldUseFinalEndDate = request.m_oFilter.m_bUseFinalDate;
                definitions.userTypeID = request.m_oFilter.m_nUserTypeID;
            }

            #endregion

            #region Tags & Metas

            eCutType cutType = eCutType.Or;

            BooleanPhrase phrase = null;

            List<BooleanPhraseNode> nodes = new List<BooleanPhraseNode>();

            string suffix = string.Empty;

            if (definitions.langauge != null && !definitions.langauge.IsDefault)
            {
                suffix = string.Format("_{0}", definitions.langauge.Code);
            }

            if (mediaSearchRequest.m_lTags != null && mediaSearchRequest.m_lTags.Count > 0)
            {
                foreach (KeyValue keyValue in mediaSearchRequest.m_lTags)
                {
                    if (!string.IsNullOrEmpty(keyValue.m_sKey))
                    {
                        string key = keyValue.m_sKey;
                        string value = keyValue.m_sValue;

                        BooleanLeaf leaf = new BooleanLeaf(key.ToLower(), value.ToLower(), typeof(string), ComparisonOperator.Equals, true)
                        {
                            fieldType = eFieldType.Tag
                        };
                        nodes.Add(leaf);
                    }
                }
            }

            if (mediaSearchRequest.m_lMetas != null && mediaSearchRequest.m_lMetas.Count > 0)
            {
                foreach (KeyValue keyValue in mediaSearchRequest.m_lMetas)
                {
                    if (!string.IsNullOrEmpty(keyValue.m_sKey))
                    {
                        string key = keyValue.m_sKey;
                        string value = keyValue.m_sValue;

                        HashSet<BooleanLeafFieldDefinitions> searchKeys = null;
                        if (doesGroupUsesTemplates)
                        {
                            searchKeys = CatalogManagement.CatalogManager.Instance.GetUnifiedSearchKey(groupId, key);
                        }
                        else
                        {
                            searchKeys = GetUnifiedSearchKey(key, group);
                        }

                        Type type = searchKeys.FirstOrDefault().ValueType;
                        bool shouldLowercase = false;

                        eFieldType fieldType = eFieldType.StringMeta;
                        if (type == typeof(int) || type == typeof(long) || type == typeof(double))
                        {
                            shouldLowercase = true;
                            fieldType = eFieldType.NonStringMeta;
                        }

                        BooleanLeaf leaf = new BooleanLeaf(key.ToLower(), value.ToLower(), type, ComparisonOperator.Equals, shouldLowercase)
                        {
                            fieldType = fieldType
                        };
                        nodes.Add(leaf);
                    }
                }
            }

            phrase = new BooleanPhrase(nodes, cutType);

            // Connect the request's filter query with the channel's tags/metas definitions

            BooleanPhraseNode root = null;

            if (!string.IsNullOrEmpty(request.m_sFilter))
            {
                string filterExpression = HttpUtility.HtmlDecode(request.m_sFilter).ToLower();

                // Build boolean phrase tree based on filter expression
                BooleanPhraseNode filterTree = null;
                status = BooleanPhraseNode.ParseSearchExpression(filterExpression, ref filterTree);

                if (status.Code != (int) eResponseStatus.OK)
                {
                    throw new KalturaException(status.Message, status.Code);
                }

                // Add prefixes, check if non start/end date exist
                UpdateNodeTreeFields(request, ref filterTree, definitions, group, groupId);

                if (phrase != null)
                {
                    List<BooleanPhraseNode> rootNodes = new List<BooleanPhraseNode>();

                    rootNodes.Add(phrase);
                    rootNodes.Add(filterTree);

                    root = new BooleanPhrase(rootNodes, eCutType.And);
                }
                else
                {
                    root = filterTree;
                }
            }
            else
            {
                root = phrase;

                status.Code = (int) eResponseStatus.OK;
                status.Message = eResponseStatus.OK.ToString();
            }

            definitions.filterPhrase = root;

            #endregion

            #region Entitlements, user preferences

            if (definitions.entitlementSearchDefinitions != null)
            {
                UnifiedSearchDefinitionsBuilder.BuildEntitlementSearchDefinitions(definitions, request, groupId, group);
            }

            if (definitions.shouldGetUserPreferences)
            {
                definitions.userPreferences =
                    CatalogLogic.GetUserPreferences(request.m_nGroupID, Convert.ToInt32(request.m_sSiteGuid));
            }

            #endregion

            #region Group By

            Utils.BuildSearchGroupBy(request.searchGroupBy, group, definitions, request.m_nGroupID);

            #endregion

            #region Geo Availability

            if (doesGroupUsesTemplates
                ? catalogGroupCache.IsGeoAvailabilityWindowingEnabled
                : group.isGeoAvailabilityWindowingEnabled)
            {
                definitions.countryId = Utils.GetIP2CountryId(request.m_nGroupID, request.m_sUserIP);
            }

            #endregion

            #region Asset User Rule

            var shopUserId = request.GetCallerUserId();
            if (shopUserId > 0)
            {
                UnifiedSearchDefinitionsBuilder.GetUserAssetRulesPhrase(request, group, ref definitions, groupId, shopUserId);
            }

            #endregion

            #region Preference

            if (!string.IsNullOrEmpty(request.m_sSiteGuid))
            {
                definitions.preference = request.m_sSiteGuid;
            }
            else if (!string.IsNullOrEmpty(request.m_sUserIP))
            {
                definitions.preference = request.m_sUserIP.Replace(".", string.Empty);
            }
            else
            {
                definitions.preference = "BeInternal";
            }

            #endregion

            #region Search Results Priority

            definitions.PriorityGroupsMappings = PriorityGroupsPreprocessor.Instance.Preprocess(request.PriorityGroupsMappings, request, definitions, group, groupId);

            #endregion

            #region Order

            // Ordering should go at the end as it depends on other definitions properties.
            var model = new AssetListEsOrderingCommonInput
            {
                GroupId = definitions.groupId,
                ShouldSearchEpg = definitions.shouldSearchEpg,
                ShouldSearchMedia = definitions.shouldSearchRecordings,
                ShouldSearchRecordings = definitions.shouldSearchRecordings,
                AssociationTags = definitions.associationTags,
                ParentMediaTypes = definitions.parentMediaTypes,
                Language = language
            };

            var orderingResult = AssetOrderingService.Instance.MapToEsOrderByFields(request, model);
            definitions.orderByFields = orderingResult.EsOrderByFields;
            definitions.order = orderingResult.Order;

            #endregion

            return status;
        }

        public static void UpdateNodeTreeFields(BaseRequest request, ref BooleanPhraseNode filterTree,
            UnifiedSearchDefinitions definitions, Group group, int groupId)
        {
            UpdateNodeTreeFields(request, ref filterTree, definitions, group, groupId, CatalogManager.Instance);
        }

        /// <summary>
        /// Update filter tree fields for specific fields/values.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filterTree"></param>
        /// <param name="definitions"></param>
        /// <param name="group"></param>
        public static void UpdateNodeTreeFields(BaseRequest request, ref BooleanPhraseNode filterTree,
            UnifiedSearchDefinitions definitions, Group group, int groupId, ICatalogManager catalogManager)
        {
            if (filterTree != null)
            {
                Dictionary<BooleanPhraseNode, BooleanPhrase> parentMapping =
                    new Dictionary<BooleanPhraseNode, BooleanPhrase>();

                Queue<BooleanPhraseNode> nodes = new Queue<BooleanPhraseNode>();
                nodes.Enqueue(filterTree);

                // BFS
                while (nodes.Count > 0)
                {
                    BooleanPhraseNode node = nodes.Dequeue();

                    // If it is a leaf, just replace the field name
                    if (node.type == BooleanNodeType.Leaf)
                    {
                        TreatLeaf(request, ref filterTree, definitions, group, node, parentMapping, groupId,
                            catalogManager);
                    }
                    else if (node.type == BooleanNodeType.Parent)
                    {
                        BooleanPhrase phrase = node as BooleanPhrase;

                        if (phrase.operand == eCutType.Or)
                        {
                            definitions.hasOrNode = true;
                        }

                        // Run on tree - enqueue all child nodes to continue going deeper
                        foreach (var childNode in phrase.nodes)
                        {
                            nodes.Enqueue(childNode);
                            parentMapping.Add(childNode, phrase);
                        }
                    }
                }
            }
        }

        public static void TreatLeaf(BaseRequest request, ref BooleanPhraseNode filterTree,
            UnifiedSearchDefinitions definitions,
            Group group, BooleanPhraseNode node, Dictionary<BooleanPhraseNode, BooleanPhrase> parentMapping,
            int groupId)
        {
            TreatLeaf(request, ref filterTree, definitions, group, node, parentMapping, groupId,
                CatalogManager.Instance);
        }

        /// <summary>
        /// Update filter tree node fields for specific fields/values.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filterTree"></param>
        /// <param name="definitions"></param>
        /// <param name="group"></param>
        /// <param name="node"></param>
        public static void TreatLeaf(BaseRequest request, ref BooleanPhraseNode filterTree,
            UnifiedSearchDefinitions definitions,
            Group group, BooleanPhraseNode node, Dictionary<BooleanPhraseNode, BooleanPhrase> parentMapping,
            int groupId, ICatalogManager catalogManager)
        {
            bool shouldUseCache = ApplicationConfiguration.Current.CatalogLogicConfiguration.ShouldUseSearchCache.Value;

            // initialize maximum nGram member only once - when this is negative it is still not set
            if (maxNGram < 0)
            {
                maxNGram = ApplicationConfiguration.Current.ElasticSearchConfiguration.MaxNGram.Value;
            }

            List<int> geoBlockRules = null;
            Dictionary<string, List<string>> mediaParentalRulesTags = null;
            Dictionary<string, List<string>> epgParentalRulesTags = null;

            BooleanLeaf leaf = node as BooleanLeaf;

            // Add prefix (meta/tag) e.g. metas.{key}
            Type metaType;
            var searchKeys = new HashSet<BooleanLeafFieldDefinitions>();
            bool doesGroupUseTemplates = catalogManager.DoesGroupUsesTemplates(groupId);
            if (doesGroupUseTemplates)
            {
                searchKeys = catalogManager.GetUnifiedSearchKey(groupId, leaf.field);
            }
            else
            {
                searchKeys = GetUnifiedSearchKey(leaf.field, group);
            }

            if (searchKeys.Count > 1 && searchKeys.First().FieldType != eFieldType.Default)
            {
                List<BooleanPhraseNode> newList = new List<BooleanPhraseNode>();

                // Split the single leaf into several brothers connected with:
                // "or" operand (if it positive)
                // "and" operand (if it negative)
                foreach (var searchKey in searchKeys)
                {
                    object value = leaf.value;

                    bool shouldLowercase = true;
                    bool isNumericValueType = false;

                    if (searchKey.ValueType == typeof(double))
                    {
                        value = Convert.ToDouble(value);
                        shouldLowercase = false;
                        isNumericValueType = true;
                    }
                    else if (searchKey.ValueType == typeof(int) || searchKey.ValueType == typeof(long))
                    {
                        value = Convert.ToInt64(value);
                        shouldLowercase = false;
                        isNumericValueType = true;
                    }

                    var newLeaf = new BooleanLeaf(searchKey.Field, value, value.GetType(), leaf.operand,
                        shouldLowercase, true)
                    {
                        fieldType = searchKey.FieldType
                    };

                    if (isNumericValueType == true)
                    {
                        HandleNumericLeaf(newLeaf);
                    }

                    newList.Add(newLeaf);
                }

                eCutType cutType = eCutType.Or;

                if (leaf.operand == ComparisonOperator.NotContains || leaf.operand == ComparisonOperator.NotEquals ||
                    leaf.operand == ComparisonOperator.NotExists)
                {
                    cutType = eCutType.And;
                }

                BooleanPhrase newPhrase = new BooleanPhrase(newList, cutType);

                BooleanPhraseNode.ReplaceLeafWithPhrase(ref filterTree, parentMapping, leaf, newPhrase);
            }
            else if (searchKeys.Count == 1)
            {
                var searchKey = searchKeys.FirstOrDefault();
                string searchKeyLowered = searchKey.Field.ToLower();
                string originalKey = leaf.field;

                // Default - string, until proved otherwise
                leaf.valueType = typeof(string);

                // If this is a tag or a meta, we need to add the language suffix
                // If not, we check if it is one of the "core" fields.
                // If it is not one of them, an exception will be thrown
                if (searchKey.FieldType != eFieldType.Default)
                {
                    searchKey.Field = searchKeyLowered;

                    leaf.field = searchKeyLowered;
                    leaf.isLanguageSpecific = true;

                    if (searchKey.ValueType == typeof(DateTime))
                    {
                        leaf.valueType = typeof(long);

                        if (leaf.value != DBNull.Value && leaf.value != null &&
                            Convert.ToString(leaf.value) != string.Empty)
                        {
                            GetLeafDate(ref leaf, request.m_dServerTime);
                        }
                        else
                        {
                            leaf.value = default(DateTime);
                        }

                        leaf.shouldLowercase = false;
                    }
                    else if (searchKey.ValueType == typeof(double))
                    {
                        HandleNumericLeaf(leaf);

                        if (doesGroupUseTemplates)
                        {
                            definitions.numericEpgMetas.Add(searchKey.Field);
                        }
                        leaf.valueType = typeof(double);
                        leaf.shouldLowercase = false;
                    }
                    else if (searchKey.ValueType == typeof(int) || searchKey.ValueType == typeof(long))
                    {
                        if (leaf.value != DBNull.Value && leaf.value != null &&
                            Convert.ToString(leaf.value) != string.Empty)
                        {
                            leaf.value = Convert.ToInt64(leaf.value);
                        }
                        else
                        {
                            leaf.value = default(long);
                        }

                        if (doesGroupUseTemplates)
                        {
                            definitions.numericEpgMetas.Add(searchKey.Field);
                        }
                        leaf.valueType = typeof(long);
                        leaf.shouldLowercase = false;
                    }
                    else
                    {
                        leaf.shouldLowercase = true;
                    }
                }
                else
                {
                    // If the filter uses non-default start/end dates, we tell the definitions no to use default start/end date
                    // TODO - Lior , ask Ira if to allow this for all types or only EPG\Recording
                    if (CatalogReservedFields.ReservedUnifiedDateFields.Contains(searchKeyLowered))
                    {
                        leaf.isReservedUnifiedSearchDate = true;
                        definitions.shouldUseStartDateForEpg = false;
                        GetLeafDate(ref leaf, request.m_dServerTime);

                        if (!definitions.shouldDateSearchesApplyToAllTypes)
                        {
                            leaf.assetTypes = new List<eObjectType>()
                            {
                                eObjectType.EPG,
                                eObjectType.Recording
                            };
                        }

                        leaf.shouldLowercase = false;

                        bool mustBeAllowedToViewInactiveAssets = false;
                        if (searchKeyLowered == CREATIONDATE)
                        {
                            searchKey.Field = CREATE_DATE;
                            mustBeAllowedToViewInactiveAssets = true;
                        }
                        else if (searchKeyLowered == PLAYBACKSTARTDATETIME)
                        {
                            searchKey.Field = START_DATE;
                        }
                        else if (searchKeyLowered == PLAYBACKENDDATETIME)
                        {
                            searchKey.Field = FINAL_DATE;
                            mustBeAllowedToViewInactiveAssets = true;
                        }
                        else if (searchKeyLowered == CATALOGSTARTDATETIME)
                        {
                            searchKey.Field = CATALOG_START_DATE;
                            mustBeAllowedToViewInactiveAssets = true;
                        }
                        else if (searchKeyLowered == CATALOGENDDATETIME)
                        {
                            searchKey.Field = END_DATE;
                        }
                        else if (searchKeyLowered == LASTMODIFIED)
                        {
                            searchKey.Field = UPDATE_DATE;
                        }
                        else if (searchKeyLowered == L2V_ORIGINAL_START_DATE || searchKeyLowered == L2V_ORIGINAL_END_DATE)
                        {
                            searchKey.Field = searchKeyLowered;
                        }

                        if (mustBeAllowedToViewInactiveAssets && !definitions.isAllowedToViewInactiveAssets)
                        {
                            throw new KalturaException(string.Format("Unauthorized use of field {0}", searchKeyLowered),
                                (int) eResponseStatus.BadSearchRequest);
                        }
                    }
                    else if (searchKeyLowered == UPDATE_DATE)
                    {
                        leaf.isReservedUnifiedSearchDate = true;
                        GetLeafDate(ref leaf, request.m_dServerTime);

                        leaf.shouldLowercase = false;
                    }
                    else if (searchKeyLowered == NamingHelper.GEO_BLOCK_FIELD)
                    {
                        // geo_block is a personal filter that currently will work only with "true".
                        if (leaf.operand == ComparisonOperator.Equals && leaf.value.ToString().ToLower() == "true")
                        {
                            definitions.PersonalData.Add(NamingHelper.GEO_BLOCK_FIELD);

                            if (geoBlockRules == null)
                            {
                                geoBlockRules = GetGeoBlockRules(request.m_nGroupID, request.m_sUserIP);
                            }

                            BooleanLeaf mediaTypeCondition = new BooleanLeaf(NamingHelper.ASSET_TYPE, "media",
                                typeof(string), ComparisonOperator.Prefix);
                            BooleanLeaf newLeaf =
                                new BooleanLeaf("geo_block_rule_id",
                                    geoBlockRules.Select(id => id.ToString()).ToList(),
                                    typeof(List<string>), ComparisonOperator.In);

                            BooleanPhrase newPhrase = new BooleanPhrase(
                                new List<BooleanPhraseNode>()
                                {
                                    mediaTypeCondition,
                                    newLeaf
                                },
                                eCutType.And);

                            BooleanPhraseNode.ReplaceLeafWithPhrase(ref filterTree, parentMapping, leaf, newPhrase);
                        }
                        else
                        {
                            throw new KalturaException("Invalid search value or operator was sent for geo_block",
                                (int) eResponseStatus.BadSearchRequest);
                        }
                    }
                    else if (searchKeyLowered == NamingHelper.PARENTAL_RULES_FIELD)
                    {
                        // Same as geo_block: it is a personal filter that currently will work only with "true".
                        if (leaf.operand == ComparisonOperator.Equals && leaf.value.ToString().ToLower() == "true")
                        {
                            if (mediaParentalRulesTags == null || epgParentalRulesTags == null)
                            {
                                definitions.PersonalData.Add(NamingHelper.PARENTAL_RULES_FIELD);

                                if (shouldUseCache)
                                {
                                    var parentalRulesTags = ParentalRulesTagsCache.Instance()
                                        .GetParentalRulesTags(request.m_nGroupID, request.m_sSiteGuid);

                                    if (parentalRulesTags != null)
                                    {
                                        mediaParentalRulesTags = parentalRulesTags.mediaTags;
                                        epgParentalRulesTags = parentalRulesTags.epgTags;
                                    }
                                }
                                else
                                {
                                    CatalogLogic.GetParentalRulesTags(request.m_nGroupID, request.m_sSiteGuid,
                                        out mediaParentalRulesTags, out epgParentalRulesTags);
                                }
                            }

                            List<BooleanPhraseNode> newMediaNodes = new List<BooleanPhraseNode>();
                            List<BooleanPhraseNode> newEpgNodes = new List<BooleanPhraseNode>();

                            newMediaNodes.Add(new BooleanLeaf("_type", "media", typeof(string),
                                ComparisonOperator.Prefix));

                            // Run on all tags and their values
                            foreach (KeyValuePair<string, List<string>> tagValues in mediaParentalRulesTags)
                            {
                                // Create a Not-in leaf for each of the tags
                                BooleanLeaf newLeaf = new BooleanLeaf(
                                    tagValues.Key.ToLower(),
                                    tagValues.Value.Select(value => value.ToLower()).ToList(),
                                    typeof(List<string>),
                                    ComparisonOperator.NotIn,
                                    true)
                                {
                                    fieldType = eFieldType.Tag
                                };

                                newMediaNodes.Add(newLeaf);
                            }

                            List<BooleanPhraseNode> typeNodesList = new List<BooleanPhraseNode>()
                            {
                                new BooleanLeaf("_type", "epg", typeof(string), ComparisonOperator.Prefix),
                                new BooleanLeaf("_type", "recording", typeof(string), ComparisonOperator.Prefix)
                            };

                            var newTypesNode = new BooleanPhrase(typeNodesList, eCutType.Or);

                            newEpgNodes.Add(newTypesNode);

                            // Run on all tags and their values
                            foreach (KeyValuePair<string, List<string>> tagValues in mediaParentalRulesTags)
                            {
                                // Create a Not-in leaf for each of the tags
                                BooleanLeaf newLeaf = new BooleanLeaf(
                                    tagValues.Key.ToLower(),
                                    tagValues.Value.Select(value => value.ToLower()).ToList(),
                                    typeof(List<string>),
                                    ComparisonOperator.NotIn,
                                    true)
                                {
                                    fieldType = eFieldType.Tag
                                };

                                newEpgNodes.Add(newLeaf);
                            }

                            // connect all tags with AND
                            BooleanPhrase newMediaPhrase = new BooleanPhrase(newMediaNodes, eCutType.And);
                            BooleanPhrase newEpgPhrase = new BooleanPhrase(newEpgNodes, eCutType.And);

                            // connect media and epg with OR
                            List<BooleanPhraseNode> newOrNodes = new List<BooleanPhraseNode>();
                            newOrNodes.Add(newMediaPhrase);
                            newOrNodes.Add(newEpgPhrase);

                            BooleanPhrase orPhrase = new BooleanPhrase(newOrNodes, eCutType.Or);

                            // Replace the original leaf (parental_rules='true') with the new phrase
                            BooleanPhraseNode.ReplaceLeafWithPhrase(ref filterTree, parentMapping, leaf, orPhrase);
                        }
                        else
                        {
                            throw new KalturaException("Invalid search value or operator was sent for parental_rules",
                                (int) eResponseStatus.BadSearchRequest);
                        }
                    }
                    else if (searchKeyLowered == NamingHelper.ENTITLED_ASSETS_FIELD)
                    {
                        // Same as geo_block: it is a personal filter that currently will work only with "true".
                        if (leaf.operand != ComparisonOperator.Equals)
                        {
                            throw new KalturaException("Invalid search value or operator was sent for entitled_assets",
                                (int) eResponseStatus.BadSearchRequest);
                        }

                        string loweredValue = leaf.value.ToString().ToLower();

                        if (definitions.entitlementSearchDefinitions == null)
                        {
                            definitions.entitlementSearchDefinitions = new EntitlementSearchDefinitions();
                        }

                        switch (loweredValue)
                        {
                            case ("not_entitled"):
                            {
                                definitions.entitlementSearchDefinitions.shouldGetFreeAssets = true;
                                definitions.entitlementSearchDefinitions.shouldGetPurchasedAssets = true;
                                definitions.entitlementSearchDefinitions.shouldSearchNotEntitled = true;
                                definitions.PersonalData.Add(NamingHelper.ENTITLED_ASSETS_FIELD);
                                break;
                            }
                            case ("free"):
                            {
                                definitions.entitlementSearchDefinitions.shouldGetFreeAssets = true;
                                break;
                            }
                            case ("entitled"):
                            {
                                definitions.entitlementSearchDefinitions.shouldGetPurchasedAssets = true;
                                definitions.PersonalData.Add(NamingHelper.ENTITLED_ASSETS_FIELD);
                                break;
                            }
                            case ("entitledsubscriptions"):
                            {
                                definitions.entitlementSearchDefinitions.shouldGetPurchasedAssets = true;
                                definitions.entitlementSearchDefinitions.shouldGetOnlySubscriptionAssets = true;
                                definitions.PersonalData.Add(NamingHelper.ENTITLED_ASSETS_FIELD);
                                break;
                            }
                            case ("both"):
                            {
                                definitions.entitlementSearchDefinitions.shouldGetFreeAssets = true;
                                definitions.entitlementSearchDefinitions.shouldGetPurchasedAssets = true;
                                definitions.PersonalData.Add(NamingHelper.ENTITLED_ASSETS_FIELD);
                                break;
                            }
                            default:
                            {
                                definitions.entitlementSearchDefinitions = null;
                                throw new KalturaException(
                                    "Invalid search value or operator was sent for entitled_assets",
                                    (int) eResponseStatus.BadSearchRequest);
                            }
                        }

                        // I mock a "contains" operator so that the query builder will know it is a not-exact search
                        leaf.operand = ComparisonOperator.Contains;
                    }
                    else if (searchKeyLowered == NamingHelper.USER_INTERESTS_FIELD)
                    {
                        // Same as geo_block: it is a personal filter that currently will work only with "true".
                        if (leaf.operand != ComparisonOperator.Equals && leaf.value.ToString().ToLower() != "true")
                        {
                            throw new KalturaException("Invalid search value or operator was sent for user_interests",
                                (int) eResponseStatus.BadSearchRequest);
                        }
                        else
                        {
                            // I mock a "contains" operator so that the query builder will know it is a not-exact search
                            leaf.operand = ComparisonOperator.Contains;
                            definitions.shouldGetUserPreferences = true;
                        }
                    }
                    else if (searchKeyLowered == NamingHelper.ASSET_TYPE)
                    {
                        string loweredValue = leaf.value.ToString().ToLower();
                        int assetType;

                        // asset type - accepts only "equals", only predefined types (epg, media, recording) and numbers
                        if (leaf.operand != ComparisonOperator.Equals ||
                            (!predefinedAssetTypes.Contains(loweredValue) &&
                             !int.TryParse(loweredValue, out assetType)))
                        {
                            throw new KalturaException("Invalid search value or operator was sent for asset_type",
                                (int) eResponseStatus.BadSearchRequest);
                        }
                        else
                        {
                            definitions.ksqlAssetTypes.Add(loweredValue);

                            // I mock a "contains" operator so that the query builder will know it is a not-exact search
                            leaf.operand = ComparisonOperator.Contains;
                        }
                    }
                    else if (CatalogReservedFields.ReservedUnifiedSearchNumericFields.Contains(searchKeyLowered))
                    {
                        leaf.shouldLowercase = false;

                        if (searchKeyLowered == STATUS)
                        {
                            // We will allow KSQL to contain "status" field only for operators.
                            if (definitions.isAllowedToViewInactiveAssets)
                            {
                                searchKey.Field = IS_ACTIVE;
                            }
                            else
                            {
                                throw new KalturaException("Unauthorized use of field status",
                                    (int) eResponseStatus.BadSearchRequest);
                            }
                        }

                        if (searchKeyLowered == NamingHelper.RECORDING_ID)
                        {
                            definitions.shouldSearchRecordings = true;
                            // I mock a "in" operator so that the query builder will know it is a not-exact search
                            leaf.operand = ComparisonOperator.In;
                        }

                        if (leaf.operand != ComparisonOperator.In)
                        {
                            leaf.valueType = typeof(long);

                            try
                            {
                                leaf.value = Convert.ToInt64(leaf.value);
                            }
                            catch (Exception ex)
                            {
                                throw new KalturaException(
                                    string.Format("Invalid search value was sent for numeric field: {0}", originalKey),
                                    (int) eResponseStatus.BadSearchRequest);
                            }
                        }

                        if (leaf.field == MEDIA_ID)
                        {
                            definitions.hasMediaIdTerm = true;
                        }
                    }
                    else if (internalReservedUnifiedSearchNumericFields.Contains(searchKeyLowered))
                    {
                        leaf.shouldLowercase = false;

                        if (leaf.operand != ComparisonOperator.In)
                        {
                            leaf.valueType = typeof(long);

                            try
                            {
                                leaf.value = Convert.ToInt64(leaf.value);
                            }
                            catch (Exception ex)
                            {
                                throw new KalturaException(
                                    string.Format("Invalid search value was sent for numeric field: {0}", originalKey),
                                    (int) eResponseStatus.BadSearchRequest);
                            }
                        }
                    }
                    else if (CatalogReservedFields.ReservedUnifiedSearchStringFields.Contains(searchKeyLowered))
                    {
                        leaf.shouldLowercase = true;

                        if (searchKeyLowered == NAME || searchKeyLowered == DESCRIPION)
                        {
                            leaf.isLanguageSpecific = true;
                            searchKey.Field = searchKeyLowered;
                            searchKey.FieldType = eFieldType.LanguageSpecificField;
                        }
                        else if (searchKeyLowered == EXTERNALID)
                        {
                            searchKey.Field = EXTERNAL_ID;
                        }
                        else if (searchKeyLowered == ENTRYID)
                        {
                            searchKey.Field = ENTRY_ID;
                        }
                        else if (searchKeyLowered == EXTERNAL_OFFER_ID)
                        {
                            leaf.shouldLowercase = false;
                            searchKey.Field = EXTERNAL_OFFER_IDS;
                        }
                    }
                    else if (searchKeyLowered == INHERITANCE_POLICY)
                    {
                        if (!definitions.isAllowedToViewInactiveAssets)
                        {
                            throw new KalturaException("Unauthorized use of field inheritance_policy",
                                (int) eResponseStatus.BadSearchRequest);
                        }

                        if (leaf.operand != ComparisonOperator.Equals)
                        {
                            throw new KalturaException(
                                "Invalid search value or operator was sent for inheritance_policy",
                                (int) eResponseStatus.BadSearchRequest);
                        }

                        string loweredValue = leaf.value.ToString().ToLower();
                        AssetInheritancePolicy inheritancePolicy = AssetInheritancePolicy.Enable;

                        if (!Enum.TryParse(loweredValue, true, out inheritancePolicy))
                        {
                            throw new KalturaException(
                                "Invalid search value or operator was sent for inheritance_policy",
                                (int) eResponseStatus.BadSearchRequest);
                        }

                        leaf.valueType = typeof(int);
                        leaf.value = (int) inheritancePolicy;
                    }
                    else if (searchKeyLowered == NamingHelper.AUTO_FILL_FIELD)
                    {
                        // Same as geo_block: it is a personal filter that currently will work only with "true".
                        if (leaf.operand == ComparisonOperator.Equals && leaf.value.ToString().ToLower() == "true")
                        {
                            definitions.ShouldSearchAutoFill = true;
                        }
                        else
                        {
                            throw new KalturaException("Invalid search value or operator was sent for auto_fill",
                                (int) eResponseStatus.BadSearchRequest);
                        }
                    }
                    else
                    {
                        throw new KalturaException(string.Format("Invalid search key was sent: {0}", originalKey),
                            (int) eResponseStatus.InvalidSearchField);
                    }
                }

                //
                // DONT MISS THE LINE
                //
                leaf.field = searchKey.Field;

                #region IN operator

                // Handle IN operator - validate the value, convert it into a proper list that the ES-QueryBuilder can use
                if (leaf.operand == ComparisonOperator.In || leaf.operand == ComparisonOperator.NotIn &&
                    leaf.valueType != typeof(List<string>))
                {
                    leaf.valueType = typeof(List<string>);
                    string value = leaf.value.ToString().ToLower();

                    string[] values = value.Split(',');

                    // If there are
                    if (values.Length == 0)
                    {
                        throw new KalturaException(string.Format("Invalid IN clause of: {0}", originalKey),
                            (int) eResponseStatus.SyntaxError);
                    }

                    foreach (var single in values)
                    {
                        int temporaryInteger;

                        if (!int.TryParse(single, out temporaryInteger))
                        {
                            throw new KalturaException(string.Format("Invalid IN clause of: {0}", originalKey),
                                (int) eResponseStatus.SyntaxError);
                        }
                    }

                    // Put new list of strings in boolean leaf
                    leaf.value = values.ToList();
                }

                #endregion

                leaf.fieldType = searchKey.FieldType;
            }

            #region Trim search value

            // If the search is contains or not contains, trim the search value to the size of the maximum NGram.
            // Otherwise the search will not work completely
            if (maxNGram > 0 &&
                (leaf.operand == ComparisonOperator.Contains || leaf.operand == ComparisonOperator.NotContains ||
                 leaf.operand == ComparisonOperator.WordStartsWith ||
                 leaf.operand == ComparisonOperator.PhraseStartsWith))
            {
                leaf.value = leaf.value.ToString().Truncate(maxNGram);
            }

            #endregion
        }

        // well, in ESv7, NEST serializes doubles with .0 even if the number is an int.
        // if the data is indedxed as a string (and on epg metas it is)
        // 1.0 does not equal 1
        // changing the way we index EPG is too risky, as far as i know, a lot depends on it being string (if not, i'd gladly change it...)
        // so the solution is to see if the value is an int or not by... flooring the double and see if its value remains the same or not.
        private static void HandleNumericLeaf(BooleanLeaf leaf)
        {
            if (leaf.operand == ComparisonOperator.In)
            {
                return;
            }

            if (leaf.value != DBNull.Value && leaf.value != null)
            {
                string leafValue = Convert.ToString(leaf.value);
                double.TryParse(leafValue, out var doubleValue);

                var flooredValue = Math.Floor(doubleValue);
                if (doubleValue == flooredValue)
                {
                    leaf.value = Convert.ToInt32(flooredValue);
                }
                else
                {
                    leaf.value = doubleValue;
                }
            }
            else
            {
                leaf.value = default(double);
            }
        }

        private static void GetLeafDate(ref BooleanLeaf leaf, DateTime serverTime)
        {
            leaf.valueType = typeof(DateTime);
            long epoch = Convert.ToInt64(leaf.value);

            // if the epoch time is greater then 1980 - it's a date, otherwise it's relative (to now) time in seconds
            if (epoch > UNIX_TIME_1980)
            {
                leaf.value = DateUtils.UtcUnixTimestampSecondsToDateTime(epoch);
            }
            else
            {
                if (serverTime == default(DateTime) || serverTime == DateTime.MinValue)
                {
                    serverTime = DateTime.UtcNow;
                }

                leaf.value = serverTime.AddSeconds(epoch);
            }
        }

        public static UnifiedSearchDefinitions BuildInternalChannelSearchObject(GroupsCacheManager.Channel channel,
            InternalChannelRequest request, Group group, int groupId,
            CatalogGroupCache catalogGroupCache = null, bool isSearchEntitlementInternal = true)
        {
            var resultProcessor = new FilterTreeResultProcessor();
            var filterTreeValidator =
                new FilterTreeValidator(resultProcessor, catalogGroupCache?.GetProgramAssetStructId());
            UnifiedSearchDefinitions definitions = new UnifiedSearchDefinitions();
            bool doesGroupUsesTemplates = CatalogManagement.CatalogManager.Instance.DoesGroupUsesTemplates(groupId);

            if (doesGroupUsesTemplates && catalogGroupCache == null)
            {
                if (!CatalogManagement.CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId,
                    out catalogGroupCache))
                {
                    log.ErrorFormat(
                        "failed to get catalogGroupCache for groupId: {0} when calling BuildInternalChannelSearchObject",
                        groupId);
                    return definitions;
                }
            }

            #region Basic

            definitions.groupId = channel.m_nGroupID;
            definitions.indexGroupId = doesGroupUsesTemplates ? groupId : group.m_nParentGroupID;

            definitions.pageIndex = request.m_nPageIndex;
            definitions.pageSize = request.m_nPageSize;

            if (!doesGroupUsesTemplates && request.isAllowedToViewInactiveAssets)
            {
                definitions.shouldIgnoreDeviceRuleID = true;
                request.isAllowedToViewInactiveAssets = false;
            }

            definitions.shouldDateSearchesApplyToAllTypes = request.isAllowedToViewInactiveAssets;
            definitions.shouldAddIsActiveTerm = request.m_oFilter != null ? request.m_oFilter.m_bOnlyActiveMedia : true;

            definitions.isAllowedToViewInactiveAssets = request.isAllowedToViewInactiveAssets;
            if (definitions.isAllowedToViewInactiveAssets)
            {
                definitions.shouldAddIsActiveTerm = false;
                definitions.shouldIgnoreDeviceRuleID = true;
            }

            if (request.m_oFilter != null)
            {
                definitions.shouldUseFinalEndDate = request.m_oFilter.m_bUseFinalDate;
                definitions.userTypeID = request.m_oFilter.m_nUserTypeID;
            }

            // in case operator is searching we override the existing value
            definitions.shouldUseStartDateForMedia = !request.isAllowedToViewInactiveAssets;
            definitions.shouldUseCatalogStartDateForMedia = doesGroupUsesTemplates;
            definitions.shouldIgnoreEndDate = request.isAllowedToViewInactiveAssets;

            #endregion

            #region Device Rules

            if (request.m_bIgnoreDeviceRuleID)
            {
                definitions.shouldIgnoreDeviceRuleID = true;
            }

            if (!definitions.shouldIgnoreDeviceRuleID && request.m_oFilter != null)
            {
                definitions.deviceRuleId = Api.api
                    .GetDeviceAllowedRuleIDs(request.m_nGroupID, request.m_oFilter.m_sDeviceId, request.domainId)
                    .ToArray();
            }

            #endregion

            #region Media Types, Permitted Watch Rules, Language

            definitions.mediaTypes = channel.m_nMediaType.ToList();

            if (!doesGroupUsesTemplates && group != null && group.m_sPermittedWatchRules != null &&
                group.m_sPermittedWatchRules.Count > 0)
            {
                definitions.permittedWatchRules = string.Join(" ", group.m_sPermittedWatchRules);
            }

            var languageId = request.m_oFilter?.m_nLanguage ?? -1;
            var language = GetLanguage(request.m_nGroupID, languageId);
            definitions.langauge = language;

            #endregion

            BooleanPhraseNode initialTree = null;
            bool emptyRequest = false;

            // If this is a KSQL channel
            if (channel.m_nChannelTypeID == (int) ChannelType.KSQL)
            {
                BooleanPhraseNode filterTree = null;
                var parseStatus = BooleanPhraseNode.ParseSearchExpression(channel.filterQuery, ref filterTree);

                if (parseStatus.Code != (int) eResponseStatus.OK)
                {
                    throw new KalturaException(parseStatus.Message, parseStatus.Code);
                }
                else
                {
                    initialTree = filterTree;
                    CatalogLogic.UpdateNodeTreeFields(request, ref initialTree, definitions, group, groupId);
                }

                #region Asset Types

                definitions.shouldSearchEpg = false;
                definitions.shouldSearchMedia = false;
                var shouldUseSearchEndDate =
                    request.GetShouldUseSearchEndDate() && !request.isAllowedToViewInactiveAssets;

                // if for some reason we are left with "0" in the list of media types (for example: "0, 424, 425"), let's ignore this 0.
                // In non-opc accounts,
                // this 0 probably came when TVM/CRUD decided this channel is for all types, but forgot to delete it when the others join in (424, 425 etc.).
                // in opc accounts it just means EPG
                // IN ANY CASE
                // we don't want to search for asset_type = 0, because the assets are not indexed with it! EPGs are just indexed in their index
                // and media will never have 0 media type
                bool hasZeroMediaType = definitions.mediaTypes != null ? definitions.mediaTypes.Remove(0) : false;
                bool hasMinusTwentySixMediaType =
                    definitions.mediaTypes.Remove(GroupsCacheManager.Channel.EPG_ASSET_TYPE);

                var indexesModel = filterTreeValidator.ValidateTree(initialTree, definitions.mediaTypes);
                // Special case - if no type was specified or "All" is contained, search all types
                if ((!doesGroupUsesTemplates && hasZeroMediaType && definitions.mediaTypes.Count == 0) ||
                    (!hasZeroMediaType && (definitions.mediaTypes == null || definitions.mediaTypes.Count == 0)))
                {
                    definitions.shouldSearchEpg = indexesModel?.ShouldSearchEpg ?? true;
                    definitions.shouldSearchMedia = indexesModel?.ShouldSearchMedia ?? true;
                    definitions.shouldUseSearchEndDate = shouldUseSearchEndDate;
                }
                else
                {
                    if (doesGroupUsesTemplates)
                    {
                        var programAssetStructId =
                            catalogGroupCache.GetRealAssetStructId(0, out bool hasProgramStructMediaType);
                        if (hasProgramStructMediaType)
                        {
                            hasProgramStructMediaType = definitions.mediaTypes.Remove((int) programAssetStructId);
                        }

                        // in OPC accounts, 0 media type means EPG
                        if (hasZeroMediaType || hasProgramStructMediaType)
                        {
                            definitions.shouldSearchEpg = true;
                            definitions.shouldUseSearchEndDate = shouldUseSearchEndDate;
                        }

                        definitions.shouldSearchEpg = indexesModel?.ShouldSearchEpg ?? definitions.shouldSearchEpg;
                        definitions.shouldSearchMedia =
                            indexesModel?.ShouldSearchMedia ?? definitions.shouldSearchMedia;
                    }
                    else
                    {
                        // in non-OPC accounts, -26 media type means EPG
                        if (hasMinusTwentySixMediaType)
                        {
                            definitions.shouldSearchEpg = true;
                            definitions.shouldUseSearchEndDate = shouldUseSearchEndDate;
                        }
                    }
                }

                // If there are items left in media types after removing 0, we are searching for media
                if (definitions.mediaTypes.Count > 0)
                {
                    definitions.shouldSearchMedia = true;
                }

                HashSet<int> mediaTypes = null;
                if (doesGroupUsesTemplates)
                {
                    mediaTypes = new HashSet<int>(catalogGroupCache.AssetStructsMapById.Keys.Select(x => (int) x));
                }
                else
                {
                    mediaTypes = new HashSet<int>(group.GetMediaTypes());
                }

                // Validate that the media types in the "assetTypes" list exist in the group's list of media types
                foreach (var mediaType in definitions.mediaTypes)
                {
                    // If one of them doesn't exist, throw an exception that says the request is bad
                    if (!mediaTypes.Contains(mediaType))
                    {
                        throw new KalturaException(string.Format("Invalid media type was sent: {0}", mediaType),
                            (int) eResponseStatus.BadSearchRequest);
                    }
                }

                #endregion

                #region Group By

                if (channel.searchGroupBy != null && channel.searchGroupBy.groupBy != null &&
                    channel.searchGroupBy.groupBy.Count > 0)
                {
                    Utils.BuildSearchGroupBy(channel.searchGroupBy, group, definitions, request.m_nGroupID);

                    // channel not contain definition for distinct ==> as default insert first groupBy in the list
                    if (string.IsNullOrEmpty(definitions.distinctGroup.Key) &&
                        string.IsNullOrEmpty(definitions.distinctGroup.Value) &&
                        string.IsNullOrEmpty(channel.searchGroupBy.distinctGroup))
                    {
                        definitions.distinctGroup = definitions.groupBy[0];
                        definitions.extraReturnFields.Add(NamingHelper.GetExtraFieldName(definitions.distinctGroup.Key,
                            definitions.distinctGroup.Type));
                    }
                }

                #endregion
            }
            else
            {
                definitions.shouldSearchMedia = true;

                #region Channel Tags

                // If there is at least one tag
                if (channel.m_lChannelTags != null && channel.m_lChannelTags.Count > 0)
                {
                    if (channel.m_nChannelTypeID == (int) ChannelType.Manual &&
                        channel.m_lChannelTags.Any(x => x.m_sKey.Equals("epg_id")))
                    {
                        definitions.shouldSearchEpg = true;
                    }

                    initialTree = GetChannelTagsBooleanPhrase(channel.m_lChannelTags, channel.m_eCutWith, group, groupId);
                }
                else
                {
                    // if there are no tags: filter everything out
                    emptyRequest = true;
                }

                if (channel.m_nChannelTypeID == (int) ChannelType.Automatic &&
                    definitions.mediaTypes != null && definitions.mediaTypes.Count > 0)
                {
                    // If there is at least one media type - it is not an empty request
                    emptyRequest = false;
                }

                // if it contains ONLY 0 - it means search all
                if (definitions.mediaTypes.Count == 1 && definitions.mediaTypes.Contains(0))
                {
                    definitions.mediaTypes.Remove(0);
                }

                #endregion
            }

            if (initialTree == null && emptyRequest)
            {
                // if there are no tags:
                // filter everything out
                initialTree = new BooleanLeaf(MEDIA_ID, 0);
            }

            #region Final Filter Tree

            // Connect the request's filter query with the channel's tags/metas definitions

            BooleanPhraseNode root = null;

            if (!string.IsNullOrEmpty(request.filterQuery))
            {
                string filterExpression = HttpUtility.HtmlDecode(request.filterQuery);

                // Build boolean phrase tree based on filter expression
                BooleanPhraseNode requestFilterTree = null;
                var status = BooleanPhraseNode.ParseSearchExpression(filterExpression, ref requestFilterTree);

                if (status.Code != (int) eResponseStatus.OK)
                {
                    throw new KalturaException(status.Message, status.Code);
                }

                CatalogLogic.UpdateNodeTreeFields(request, ref requestFilterTree, definitions, group, groupId);

                if (initialTree != null)
                {
                    List<BooleanPhraseNode> rootNodes = new List<BooleanPhraseNode>();

                    rootNodes.Add(initialTree);
                    rootNodes.Add(requestFilterTree);

                    root = new BooleanPhrase(rootNodes, eCutType.And);
                }
                else
                {
                    root = requestFilterTree;
                }
            }
            else
            {
                root = initialTree;
            }

            definitions.filterPhrase = root;

            #region Search Results Priority

            definitions.PriorityGroupsMappings = PriorityGroupsPreprocessor.Instance.Preprocess(request.PriorityGroupsMappings, request, definitions, group, groupId);

            #endregion

            if (!isSearchEntitlementInternal && definitions.entitlementSearchDefinitions != null)
            {
                UnifiedSearchDefinitionsBuilder.BuildEntitlementSearchDefinitions(definitions, request, doesGroupUsesTemplates ? groupId : group.m_nParentGroupID, group);
            }

            if (definitions.shouldGetUserPreferences)
            {
                definitions.userPreferences =
                    CatalogLogic.GetUserPreferences(request.m_nGroupID, Convert.ToInt32(request.m_sSiteGuid));
            }

            #endregion

            // Get days offset for EPG search from TCM
            definitions.epgDaysOffest = ApplicationConfiguration.Current.CatalogLogicConfiguration
                .CurrentRequestDaysOffset.Value;

            #region Regions and associations

            if (!definitions.isAllowedToViewInactiveAssets && !definitions.IgnoreSearchRegions)
            {
                CatalogLogic.SetSearchRegions(request.m_nGroupID, doesGroupUsesTemplates, request.domainId, request.m_sSiteGuid, out var regionIds, out var linearMediaTypes);

                definitions.regionIds = regionIds;
                definitions.linearChannelMediaTypes = linearMediaTypes;
            }

            if (doesGroupUsesTemplates)
            {
                definitions.ObjectVirtualAssetIds = catalogGroupCache.GetObjectVirtualAssetIds();
            }

            CatalogLogic.GetParentMediaTypesAssociations(request.m_nGroupID,
                out definitions.parentMediaTypes, out definitions.associationTags);

            #endregion

            #region Geo Availability

            var isGeoAvailabilityWindowingEnabled = doesGroupUsesTemplates
                ? catalogGroupCache.IsGeoAvailabilityWindowingEnabled
                : group.isGeoAvailabilityWindowingEnabled;
            if (!definitions.isAllowedToViewInactiveAssets && isGeoAvailabilityWindowingEnabled)
            {
                definitions.countryId = Utils.GetIP2CountryId(request.m_nGroupID, request.m_sUserIP);
            }

            #endregion

            #region Asset User Rule

            if (channel.AssetUserRuleId.HasValue && channel.AssetUserRuleId.Value > 0)
            {
                UnifiedSearchDefinitionsBuilder.GetChannelUserAssetRulesPhrase(request, group, ref definitions, groupId, channel.AssetUserRuleId.Value);
            }

            var shopUserId = request.GetCallerUserId();
            if (shopUserId > 0)
            {
                UnifiedSearchDefinitionsBuilder.GetUserAssetRulesPhrase(request, group, ref definitions, groupId, shopUserId);
            }

            #endregion

            #region Segmentation

            if (channel.SupportSegmentBasedOrdering && !string.IsNullOrEmpty(request.m_sSiteGuid) &&
                request.m_sSiteGuid != "0")
            {
                var userSegmentIds = UserSegmentLogic.ListAll(groupId, request.m_sSiteGuid);
                if (userSegmentIds?.Count > 0)
                {
                    List<SegmentationType> segmentationTypes = SegmentationTypeLogic.GetSegmentationTypesBySegmentIds(groupId, userSegmentIds);

                    definitions.boostScoreValues = new List<BoostScoreValueDefinition>();

                    foreach (var segmentationType in segmentationTypes)
                    {
                        if (segmentationType.Conditions != null)
                        {
                            foreach (var condition in segmentationType.Conditions)
                            {
                                if (condition is ContentScoreCondition castedCondition)
                                {
                                    if (!string.IsNullOrEmpty(castedCondition.Field) && castedCondition.Values != null &&
                                        castedCondition.Values.Count > 0)
                                    {
                                        var fields = GetUnifiedSearchKey(castedCondition.Field.ToLower(), group, groupId);

                                        if (fields.Any() && fields.FirstOrDefault().FieldType != eFieldType.Default)
                                        {
                                            foreach (var field in fields)
                                            {
                                                foreach (var value in castedCondition.Values)
                                                {
                                                    definitions.boostScoreValues.Add(new BoostScoreValueDefinition()
                                                    {
                                                        Key = field.Field,
                                                        Value = value,
                                                        Type = field.FieldType
                                                    });
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (segmentationType.Actions != null)
                        {
                            foreach (var action in segmentationType.Actions)
                            {
                                if (action is SegmentAssetOrderAction castedAction)
                                {
                                    if (!string.IsNullOrEmpty(castedAction.Name) && castedAction.Values != null &&
                                        castedAction.Values.Count > 0)
                                    {
                                        var fields = GetUnifiedSearchKey(castedAction.Name.ToLower(), group, groupId);

                                        if (fields.Any())
                                        {
                                            foreach (var field in fields)
                                            {
                                                foreach (var value in castedAction.Values)
                                                {
                                                    definitions.boostScoreValues.Add(
                                                        new BoostScoreValueDefinition()
                                                        {
                                                            Key = field.Field,
                                                            Value = value,
                                                            Type = field.FieldType
                                                        });
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if ((segmentationType.Value != null) && (segmentationType.Value is SegmentValues) &&
                            (segmentationType.Value.Source != null) &&
                            (segmentationType.Value.Source is ContentSource))
                        {
                            var castedValue = segmentationType.Value as SegmentValues;
                            var castedSource = segmentationType.Value.Source as ContentSource;

                            if (!string.IsNullOrEmpty(castedSource.Field))
                            {
                                foreach (var value in castedValue.Values)
                                {
                                    if (userSegmentIds.Contains(value.Id))
                                    {
                                        var fields = GetUnifiedSearchKey(castedSource.Field.ToLower(), group, groupId);

                                        if (fields.Any() && fields.FirstOrDefault().FieldType != eFieldType.Default)
                                        {
                                            foreach (var field in fields)
                                            {
                                                definitions.boostScoreValues.Add(new BoostScoreValueDefinition()
                                                {
                                                    Key = field.Field,
                                                    Value = value.Value,
                                                    Type = field.FieldType
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region Preference

            if (!string.IsNullOrEmpty(request.m_sSiteGuid))
            {
                definitions.preference = request.m_sSiteGuid;
            }
            else if (!string.IsNullOrEmpty(request.m_sUserIP))
            {
                definitions.preference = request.m_sUserIP.Replace(".", string.Empty);
            }
            else
            {
                definitions.preference = "BeInternal";
            }

            #endregion

            #region Group By

            Utils.BuildSearchGroupBy(request.searchGroupBy, group, definitions, request.m_nGroupID);

            definitions.GroupByOption = request.searchGroupBy?.isGroupingOptionInclude == true
                ? GroupingOption.Include
                : GroupingOption.Omit;

            #endregion

            #region Order

            // Ordering should go at the end as it depends on other definitions properties.
            var model = new AssetListEsOrderingCommonInput
            {
                GroupId = definitions.groupId,
                ShouldSearchEpg = definitions.shouldSearchEpg,
                ShouldSearchMedia = definitions.shouldSearchRecordings,
                ShouldSearchRecordings = definitions.shouldSearchRecordings,
                AssociationTags = definitions.associationTags,
                ParentMediaTypes = definitions.parentMediaTypes,
                Language = language
            };

            var orderingResult = AssetOrderingService.Instance.MapToChannelEsOrderByFields(request, channel, model);
            definitions.orderByFields = orderingResult.EsOrderByFields;
            if (orderingResult.SpecificOrder?.Count > 0)
            {
                definitions.specificOrder = orderingResult.SpecificOrder.ToList();
            }

            // Still need these assignments as IndexManagerV7 doesn't support secondary sorting.
            definitions.order = orderingResult.Order;

            #endregion

            return definitions;
        }

        private static BooleanPhrase GetChannelTagsBooleanPhrase(List<SearchValue> channelTags, CutWith channelCutWith, Group group, int groupId)
        {
            List<BooleanPhraseNode> channelTagsNodes = new List<BooleanPhraseNode>();

            foreach (SearchValue searchValue in channelTags)
            {
                if (!string.IsNullOrEmpty(searchValue.m_sKey))
                {
                    eCutType innerCutType = GetCutTypeByCutWith(searchValue.m_eInnerCutWith);
                    string key = searchValue.m_sKey.ToLower();

                    BooleanPhraseNode newNode = null;
                    if (!string.IsNullOrEmpty(searchValue.m_sKeyPrefix))
                    {
                        key = string.Format("{0}.{1}", searchValue.m_sKeyPrefix, key);
                    }

                    var keyLeafFieldDefinition = CatalogLogic.GetUnifiedSearchKey(key, group, groupId).FirstOrDefault();

                    bool shouldLowercase = true;
                    if (CatalogReservedFields.ReservedUnifiedSearchNumericFields.Contains(searchValue.m_sKey))
                    {
                        shouldLowercase = false;
                    }

                    // If only 1 value, use it as a single node (there's no need to create a phrase with 1 node...)
                    if (searchValue.m_lValue.Count == 1)
                    {
                        newNode = new BooleanLeaf(keyLeafFieldDefinition?.Field ?? key,
                            searchValue.m_lValue[0],
                            keyLeafFieldDefinition?.ValueType ?? typeof(string),
                            ComparisonOperator.Equals,
                            shouldLowercase)
                        {
                            fieldType = keyLeafFieldDefinition?.FieldType ?? eFieldType.Default
                        };
                    }
                    else
                    {
                        // If there are several values, connect all the values with the inner cut type
                        List<BooleanPhraseNode> innerNodes = new List<BooleanPhraseNode>();

                        foreach (var item in searchValue.m_lValue)
                        {
                            BooleanLeaf leaf = new BooleanLeaf(keyLeafFieldDefinition?.Field ?? key,
                                item,
                                keyLeafFieldDefinition?.ValueType ?? typeof(string),
                                ComparisonOperator.Equals,
                                shouldLowercase)
                            {
                                fieldType = keyLeafFieldDefinition?.FieldType ?? eFieldType.Default
                            };
                            innerNodes.Add(leaf);
                        }

                        newNode = new BooleanPhrase(innerNodes, innerCutType);
                    }

                    channelTagsNodes.Add(newNode);
                }
            }

            eCutType cutType = GetCutTypeByCutWith(channelCutWith);
            var booleanPhrase = new BooleanPhrase(channelTagsNodes, cutType);
            return booleanPhrase;
        }

        private static eCutType GetCutTypeByCutWith(CutWith cutWith)
        {
            eCutType cutType = eCutType.And;
            switch (cutWith)
            {
                case CutWith.WCF_ONLY_DEFAULT_VALUE:
                    break;
                case CutWith.OR:
                {
                    cutType = eCutType.Or;
                    break;
                }
                case CutWith.AND:
                {
                    cutType = eCutType.And;
                    break;
                }
                default:
                    break;
            }

            return cutType;
        }

        public static UnifiedSearchDefinitions BuildInternalChannelSearchObjectWithBaseRequest(
            GroupsCacheManager.Channel channel,
            BaseRequest request,
            Group group,
            int groupId,
            bool doesGroupUsesTemplates,
            bool isAllowedToViewInactiveAssets,
            string assetFilterKsql)
        {
            InternalChannelRequest channelRequest = new InternalChannelRequest(channel.m_nChannelID.ToString(),
                string.Empty, doesGroupUsesTemplates ? groupId : group.m_nParentGroupID, request.m_nPageSize,
                request.m_nPageIndex, request.m_sUserIP, request.m_sSignature, request.m_sSignString, request.m_oFilter,
                assetFilterKsql,
                new OrderObj() { }, request.OriginalUserId);

            channelRequest.isAllowedToViewInactiveAssets = isAllowedToViewInactiveAssets;

            return BuildInternalChannelSearchObject(channel, channelRequest, group, groupId);
        }

        private static MediaSearchObj BuildInternalChannelSearchObject(GroupsCacheManager.Channel channel,
            InternalChannelRequest request, int groupId, LanguageObj languageObj, List<string> lPermittedWatchRules)
        {
            int[] nDeviceRuleId = null;
            if (request.m_oFilter != null)
                nDeviceRuleId = Api.api
                    .GetDeviceAllowedRuleIDs(request.m_nGroupID, request.m_oFilter.m_sDeviceId, request.domainId)
                    .ToArray();

            return CatalogLogic.BuildBaseChannelSearchObject(channel, request,
                request.order, groupId, channel.m_nGroupID == channel.m_nParentGroupID ? lPermittedWatchRules : null,
                nDeviceRuleId, languageObj);
        }

        #endregion

        public static bool RebuildGroup(int nGroupId, bool rebuild)
        {
            bool res = false;
            try
            {
                log.DebugFormat("RebuildGroup:{0}, rebuild:{1}", nGroupId, rebuild);
                GroupsCacheManager.GroupManager groupManager = new GroupsCacheManager.GroupManager();
                res = groupManager.RemoveGroup(nGroupId);
                log.DebugFormat("RemoveGroup:{0}, res:{1}", nGroupId, res);
                if (rebuild)
                {
                    Group group = groupManager.GetGroup(nGroupId);
                    res = group != null;
                    log.DebugFormat("GetGroup:{0}, res:{1}", nGroupId, res);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("RebuildGroup:{0}, ex:{1}", nGroupId, ex.Message), ex);
            }

            return res;
        }

        public static string GetGroup(int nGroupId)
        {
            string sGroup = string.Empty;
            log.DebugFormat("GetGroup:{0}", nGroupId);
            GroupsCacheManager.GroupManager groupManager = new GroupsCacheManager.GroupManager();
            Group group = groupManager.GetGroup(nGroupId);
            if (group != null)
            {
                sGroup = Newtonsoft.Json.JsonConvert.SerializeObject(group);
            }

            return sGroup;
        }

        public static void WriteMediaEohStatistics(int nWatcherID, string sSessionID, int m_nGroupID, int nOwnerGroupID,
            int mediaId, int nMediaFileID, int nBillingTypeID, int nCDNID, int nMediaDuration,
            int nCountryID, int nPlayerID, int nFirstPlayCounter, int nPlayCounter, int nLoadCounter, int nPauseCounter,
            int nStopCounter, int nFinishCounter, int nFullScreenCounter,
            int nExitFullScreenCounterint, int nSendToFriendCounter, int nPlayTimeCounter, int nFileQualityID,
            int nFileFormatID, DateTime dStartHourDate, int nUpdaterID,
            int nBrowser, int nPlatform, string sSiteGuid, string sDeviceUdID, string sPlayCycleID, int nSwooshCounter)
        {
            try
            {
                // We write an empty string as the first parameter to split the start of the log from the mediaEoh row data
                string infoToLog = string.Join(",", new object[]
                {
                    " ", nWatcherID, sSessionID, m_nGroupID, nOwnerGroupID, mediaId, nMediaFileID, nBillingTypeID,
                    nCDNID, nMediaDuration, nCountryID, nPlayerID,
                    nFirstPlayCounter, nPlayCounter, nLoadCounter, nPauseCounter, nStopCounter, nFinishCounter,
                    nFullScreenCounter, nExitFullScreenCounterint,
                    nSendToFriendCounter, nPlayTimeCounter, nFileQualityID, nFileFormatID, dStartHourDate, nUpdaterID,
                    nBrowser, nPlatform, sSiteGuid,
                    sDeviceUdID, sPlayCycleID, nSwooshCounter
                });
                statisticsLog.Info(infoToLog);
            }
            catch (Exception ex)
            {
                log.ErrorFormat(
                    "Error in WriteMediaEohStatistics, mediaID: {0}, mediaFileID: {1}, groupID: {2}, siteGuid: {3}, udid: {4}, Exception: {5}",
                    mediaId,
                    nMediaFileID, m_nGroupID, sSiteGuid, sDeviceUdID, ex);
            }
        }

        public static bool UpdateRecordingsIndex(List<long> recordingsIds, int groupId, eAction action)
        {
            TimeShiftedTvPartnerSettings accountSettings = ConditionalAccess.Utils.GetTimeShiftedTvPartnerSettings(groupId);
            return CatalogLogic.Update(recordingsIds, groupId, eObjectType.Recording, action, accountSettings.PersonalizedRecordingEnable == true);
        }

        public static bool RebuildEpgChannel(int groupId, int epgChannelID, DateTime fromDate, DateTime toDate,
            bool duplicates)
        {
            try
            {
                if (duplicates)
                {
                    return RemoveDuplicatesEpgPrograms(groupId, epgChannelID, fromDate, toDate);
                }
                else
                {
                    return RebuildEpgProgramsChannel(groupId, epgChannelID, fromDate, toDate);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("RebuildGroup:{0}, ex:{1}", groupId, ex.Message), ex);
            }

            return false;
        }

        private static bool RebuildEpgProgramsChannel(int groupId, int epgChannelID, DateTime fromDate, DateTime toDate)
        {
            bool res = false;
            try
            {
                List<LanguageObj> groupLang = CatalogDAL.GetGroupLanguages(groupId);
                string mainLang = groupLang.Where(x => x.IsDefault).Select(x => x.Code).FirstOrDefault();

                log.DebugFormat("RebuildEpgChannel:{0}, epgChannelID:{1}, fromDate:{2}, toDate:{3}", groupId,
                    epgChannelID, fromDate, toDate);
                DataSet ds = EpgDal.Get_EpgProgramsDetailsByChannelIds(groupId, epgChannelID, fromDate, toDate);
                List<EpgCB> epgs = ConvertToEpgCB(ds, mainLang);

                // get all epg program ids from DB in channel ID between date (all statuses)
                List<long> lProgramsID = GetEpgIds(epgChannelID, groupId, fromDate, toDate);
                log.DebugFormat("RebuildEpgProgramsChannel : lProgramsID:{0}, epgChannelID:{1}",
                    string.Join(",", lProgramsID), epgChannelID);

                #region Delete

                // delete all current Epgs in CB related to channel between dates
                BaseEpgBL epgBL = EpgBL.Utils.GetInstance(groupId);
                epgBL.RemoveGroupPrograms(lProgramsID.ConvertAll<int>(x => (int) x));
                // Delete from ES
                bool resultEpgIndex;
                var epgChannelIds = new List<string>() {epgChannelID.ToString()};
                if (lProgramsID != null && lProgramsID.Count > 0)
                {
                    resultEpgIndex = UpdateEpgIndex(lProgramsID, groupId, eAction.Delete, epgChannelIds, false);
                }

                #endregion

                #region insert Bulks

                // insert all above
                int nCount = 0;
                int nCountPackage = ApplicationConfiguration.Current.CatalogLogicConfiguration.UpdateEPGPackage.Value;
                if (nCountPackage == 0)
                    nCountPackage = 200;
                List<long> epgIds = new List<long>();
                foreach (EpgCB epg in epgs)
                {
                    nCount++;
                    // insert to CB
                    ulong epgID = 0;
                    epgBL.InsertEpg(epg, out epgID);
                    // insert to ES
                    epgIds.Add((long) epg.EpgID);
                    if (nCount >= nCountPackage)
                    {
                        epgIds.Add((long) epg.EpgID);
                        resultEpgIndex = UpdateEpgIndex(epgIds, groupId, eAction.Update, epgChannelIds, false);
                        epgIds = new List<long>();
                        nCount = 0;
                    }
                }

                if (nCount > 0 && epgIds != null && epgIds.Count > 0)
                {
                    resultEpgIndex = UpdateEpgIndex(epgIds, groupId, eAction.Update, epgChannelIds, false);
                }

                #endregion

                res = true;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("RebuildEpgProgramsChannel:{0}, epgChannelID:{1}, fromDate:{2}, toDate:{3}, ex:{4}",
                    groupId, epgChannelID, fromDate, toDate, ex.InnerException);
                return false;
            }

            return res;
        }

        private static bool RemoveDuplicatesEpgPrograms(int groupId, int epgChannelID, DateTime fromDate,
            DateTime toDate)
        {
            try
            {
                List<long> epgIds = EpgDal.GetEpgIds(epgChannelID, groupId, fromDate, toDate, 2);
                if (epgIds != null && epgIds.Count > 0)
                {
                    BaseEpgBL epgBL = EpgBL.Utils.GetInstance(groupId);
                    epgBL.RemoveGroupPrograms(epgIds.ConvertAll<int>(x => (int) x));
                    bool resultEpgIndex = UpdateEpgIndex(epgIds, groupId, eAction.Delete,
                        new List<string>() {epgChannelID.ToString()}, false);
                }

                return true;
            }
            catch
            {
            }

            return false;
        }

        private static List<long> GetEpgIds(int epgChannelID, int groupId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                List<long> epgIds = EpgDal.GetEpgIds(epgChannelID, groupId, fromDate, toDate);
                return epgIds;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("fail to DeleteEpgs from DB ex = {0}", ex.Message), ex);
                return null;
            }
        }

        private static List<EpgCB> ConvertToEpgCB(DataSet ds, string mainLang)
        {
            List<EpgCB> epgs = new List<EpgCB>();
            try
            {
                if (ds != null && ds.Tables != null && ds.Tables.Count == 6)
                {
                    if (ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0
                        && ds.Tables[1] != null && ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count > 0)
                    {
                        BasicEpgProgramDetails(ds.Tables[0], ds.Tables[1], epgs, mainLang);
                    }

                    if (epgs != null && epgs.Count > 0)
                    {
                        GetMetaDetails(epgs, ds.Tables[2]);
                        GetTagDetails(epgs, ds.Tables[3]);

                        GetPicDetails(epgs, ds.Tables[4], ds.Tables[5]);
                    }
                }
            }
            catch (Exception)
            {
                epgs = null;
            }

            return epgs;
        }

        private static void GetPicDetails(List<EpgCB> epgs, DataTable dtEpgIDPic, DataTable dtPic)
        {
            try
            {
                if (dtEpgIDPic == null || dtPic == null)
                    return;

                foreach (EpgCB item in epgs)
                {
                    DataRow[] picID = dtEpgIDPic.Select("ID=" + item.EpgID);
                    DataRow[] pics = dtPic.Select("id=" + picID[0]["picID"]);
                    EpgPicture epgPicture;
                    if (pics != null && pics.Count() > 0)
                    {
                        item.pictures = new List<ApiObjects.Epg.EpgPicture>();
                        foreach (DataRow dr in pics)
                        {
                            epgPicture = new EpgPicture();
                            epgPicture.PicHeight = ODBCWrapper.Utils.GetIntSafeVal(dr, "HEIGHT");
                            epgPicture.PicID = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
                            epgPicture.PicWidth = ODBCWrapper.Utils.GetIntSafeVal(dr, "WIDTH");
                            epgPicture.Ratio = ODBCWrapper.Utils.GetSafeStr(dr, "ratio");
                            epgPicture.RatioId = ODBCWrapper.Utils.GetIntSafeVal(dr, "ratio_id");
                            epgPicture.Url = ODBCWrapper.Utils.GetSafeStr(dr, "m_sURL");
                            ;

                            item.pictures.Add(epgPicture);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }
        }

        private static void GetMetaDetails(List<EpgCB> epgs, DataTable dt)
        {
            try
            {
                if (dt == null)
                    return;
                foreach (EpgCB item in epgs)
                {
                    DataRow[] metas = dt.Select("program_id=" + item.EpgID);
                    if (metas != null && metas.Count() > 0)
                    {
                        item.Metas = new Dictionary<string, List<string>>();
                        foreach (DataRow dr in metas)
                        {
                            string key = Utils.GetStrSafeVal(dr, "name");
                            string value = Utils.GetStrSafeVal(dr, "value");

                            if (item.Metas.ContainsKey(key))
                            {
                                item.Metas[key].Add(value);
                            }
                            else
                            {
                                item.Metas.Add(key, new List<string>() {value});
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }
        }

        private static void GetTagDetails(List<EpgCB> epgs, DataTable dt)
        {
            try
            {
                if (dt == null)
                    return;
                foreach (EpgCB item in epgs)
                {
                    DataRow[] tags = dt.Select("program_id=" + item.EpgID);
                    if (tags != null && tags.Count() > 0)
                    {
                        item.Tags = new Dictionary<string, List<string>>();
                        foreach (DataRow dr in tags)
                        {
                            string key = Utils.GetStrSafeVal(dr, "TagTypeName");
                            string value = Utils.GetStrSafeVal(dr, "TagValueName");

                            if (item.Tags.ContainsKey(key))
                            {
                                item.Tags[key].Add(value);
                            }
                            else
                            {
                                item.Tags.Add(key, new List<string>() {value});
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }
        }

        private static void BasicEpgProgramDetails(DataTable dt, DataTable dtUpdateDate, List<EpgCB> epgs,
            string mainLang)
        {
            EpgCB epg;
            try
            {
                foreach (DataRow dr in dt.Rows)
                {
                    epg = new EpgCB();
                    string pic_url = string.Empty;
                    epg.EpgID = (ulong) Utils.GetLongSafeVal(dr, "ID");
                    epg.EpgIdentifier = Utils.GetStrSafeVal(dr, "EPG_IDENTIFIER");
                    epg.Name = Utils.GetStrSafeVal(dr, "NAME");
                    epg.Description = Utils.GetStrSafeVal(dr, "DESCRIPTION");
                    epg.ChannelID = Utils.GetIntSafeVal(dr, "EPG_CHANNEL_ID");
                    epg.PicUrl = Utils.GetStrSafeVal(dr, "PIC_URL");
                    epg.Status = Utils.GetIntSafeVal(dr, "STATUS");
                    epg.IsActive = Utils.GetIntSafeVal(dr, "IS_ACTIVE") == 1 ? true : false;
                    epg.GroupID = Utils.GetIntSafeVal(dr, "GROUP_ID");
                    epg.PicID = Utils.GetIntSafeVal(dr, "pic_id");
                    epg.ParentGroupID = Utils.GetIntSafeVal(dr, "PARENT_GROUP_ID");
                    epg.GroupID = Utils.GetIntSafeVal(dr, "GROUP_ID");
                    //Dates
                    epg.StartDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "START_DATE");
                    epg.EndDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "END_DATE");
                    epg.CreateDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "CREATE_DATE");
                    epg.EnableCatchUp = Utils.GetIntSafeVal(dr, "ENABLE_CATCH_UP");
                    epg.EnableCDVR = Utils.GetIntSafeVal(dr, "ENABLE_CDVR");
                    epg.EnableStartOver = Utils.GetIntSafeVal(dr, "ENABLE_START_OVER");
                    epg.EnableTrickPlay = Utils.GetIntSafeVal(dr, "ENABLE_TRICK_PLAY");
                    epg.Language = mainLang;
                    DataRow updateDr = dtUpdateDate.Select("ID=" + epg.EpgID).FirstOrDefault();
                    epg.UpdateDate = ODBCWrapper.Utils.GetDateSafeVal(updateDr, "UPDATE_DATE");
                    epgs.Add(epg);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }
        }

        public static List<WatchHistory> GetUserWatchHistory(int groupId, string siteGuid, int domainId,
            List<int> assetTypes,
            List<string> assetIds, List<int> excludedAssetTypes, eWatchStatus filterStatus, int numOfDays,
            ApiObjects.SearchObjects.OrderDir orderDir, int pageIndex, int pageSize, bool suppress, string filterQuery,
            out int totalItems)
        {
            log.DebugFormat("Start GetUserWatchHistory for user {0} in groupId {1}", siteGuid, groupId);

            List<WatchHistory> usersWatchHistory = new List<WatchHistory>();

            totalItems = 0;

            try
            {
                if (!string.IsNullOrEmpty(filterQuery)) //BEO - 9621.use filterQuery
                {
                    assetTypes = assetTypes ?? new List<int>();
                    assetIds = assetIds ?? new List<string>();
                }

                var unFilteredresult = GetRawUserWatchHistory(groupId, siteGuid, assetTypes, assetIds,
                    excludedAssetTypes, filterStatus, numOfDays);
                Dictionary<string, WatchHistory> seriesMap = new Dictionary<string, WatchHistory>();

                if (unFilteredresult != null && unFilteredresult.Count > 0)
                {
                    unFilteredresult = unFilteredresult.Where(x => x.AssetTypeId != (int) eAssetTypes.UNKNOWN).ToList();

                    GroupsCacheManager.GroupManager groupManager = new GroupsCacheManager.GroupManager();
                    Group group = groupManager.GetGroup(groupId);

                    string watchRules = string.Empty;
                    bool isOPC = CatalogManager.Instance.DoesGroupUsesTemplates(groupId);

                    if ((group.m_sPermittedWatchRules != null && group.m_sPermittedWatchRules.Count > 0) || isOPC)
                    {
                        if (!isOPC)
                        {
                            watchRules = string.Join(" ", group.m_sPermittedWatchRules);
                        }

                        // validate media on ES
                        UnifiedSearchDefinitions searchDefinitions = new UnifiedSearchDefinitions()
                        {
                            groupId = groupId,
                            permittedWatchRules = watchRules,
                            specificAssets = new Dictionary<eAssetTypes, List<string>>(),
                            shouldAddIsActiveTerm = true,
                            shouldIgnoreDeviceRuleID = true,
                            extraReturnFields = new HashSet<string>()
                        };

                        if (!string.IsNullOrEmpty(filterQuery)) //BEO - 9621.use filterQuery
                        {
                            string filter = filterQuery;
                            BooleanPhraseNode filterTree = null;
                            var status = BooleanPhraseNode.ParseSearchExpression(filter, ref filterTree);

                            searchDefinitions.filterPhrase = filterTree;

                            BaseRequest request = new BaseRequest()
                            {
                                m_sSiteGuid = siteGuid,
                                m_nGroupID = groupId,
                                m_dServerTime = DateTime.UtcNow
                                //,m_sUserIP
                            };

                            UpdateNodeTreeFields(request, ref filterTree, searchDefinitions, group,
                                groupId); //BEO-10234
                        }

                        int elasticSearchPageSize = 0;
                        string seriesIdExtraReturnField = "metas.seriesid";

                        List<string> listOfMedia = unFilteredresult.Where(x => x.AssetTypeId > 1)
                            .Select(x => x.AssetId.ToString()).ToList();
                        if (listOfMedia.Count > 0)
                        {
                            searchDefinitions.specificAssets.Add(eAssetTypes.MEDIA, listOfMedia);
                            searchDefinitions.shouldSearchMedia = true;
                            searchDefinitions.shouldUseFinalEndDate = true;
                            searchDefinitions.shouldUseStartDateForMedia = true;
                            elasticSearchPageSize += listOfMedia.Count;
                            if (suppress)
                            {
                                if (!isOPC)
                                {
                                    string episodeAssociationTagName = NotificationCache.Instance().GetEpisodeAssociationTagName(groupId);

                                    if (!string.IsNullOrEmpty(episodeAssociationTagName))
                                    {
                                        var prefix = GetElasticPrefixByFieldType(episodeAssociationTagName, group);
                                        seriesIdExtraReturnField = $"{prefix}.{episodeAssociationTagName.ToLower()}";
                                    }
                                    else
                                    {
                                        suppress = false;
                                    }
                                }

                                if (suppress)
                                {
                                    searchDefinitions.extraReturnFields.Add(seriesIdExtraReturnField);
                                    searchDefinitions.extraReturnFields.Add("media_type_id");
                                }
                            }
                        }

                        List<string> listOfEpg = unFilteredresult.Where(x => x.AssetTypeId == (int) eAssetTypes.EPG)
                            .Select(x => x.AssetId.ToString()).ToList();
                        if (listOfEpg.Count > 0)
                        {
                            searchDefinitions.specificAssets.Add(eAssetTypes.EPG, listOfEpg);
                            searchDefinitions.shouldSearchEpg = true;
                            searchDefinitions.shouldUseSearchEndDate = true;
                            searchDefinitions.shouldUseStartDateForEpg = false;
                            searchDefinitions.shouldUseEndDateForEpg = false;
                            elasticSearchPageSize += listOfEpg.Count;
                        }

                        searchDefinitions.pageSize = elasticSearchPageSize;
                        searchDefinitions.shouldReturnExtendedSearchResult =
                            searchDefinitions.extraReturnFields?.Count > 0;

                        searchDefinitions.EpgFeatureVersion = GroupSettingsManager.Instance.GetEpgFeatureVersion(groupId);

                        if (elasticSearchPageSize > 0)
                        {
                            List<int> activeMediaIds = new List<int>();
                            List<int> activeEpg = new List<int>();

                            int parentGroupId = CatalogCache.Instance().GetParentGroup(groupId);
                            var indexManager = IndexManagerFactory.Instance.GetIndexManager(parentGroupId);
                            int esTotalItems = 0;
                            var searchResults = indexManager.UnifiedSearch(searchDefinitions, ref esTotalItems);

                            long? episodeStructId = 0;
                            bool isEpisode = false;

                            if (searchResults != null && searchResults.Count > 0)
                            {
                                foreach (var searchResult in searchResults)
                                {
                                    int assetId = int.Parse(searchResult.AssetId);

                                    if (searchResult.AssetType == eAssetTypes.MEDIA)
                                    {
                                        bool addToList = true;
                                        var watched = unFilteredresult.First(x =>
                                            int.Parse(x.AssetId) == assetId && x.AssetTypeId > 1);
                                        watched.UpdateDate = searchResult.m_dUpdateDate;

                                        if (suppress)
                                        {
                                            ExtendedSearchResult ecr = (ExtendedSearchResult) searchResult;
                                            string seriesId =
                                                Api.api.GetStringParamFromExtendedSearchResult(ecr,
                                                    seriesIdExtraReturnField);

                                            if (!string.IsNullOrEmpty(seriesId))
                                            {
                                                if (episodeStructId == 0)
                                                {
                                                    if (isOPC)
                                                    {
                                                        CatalogGroupCache catalogGroupCache;
                                                        if (CatalogManagement.CatalogManager.Instance
                                                            .TryGetCatalogGroupCacheFromCache(groupId,
                                                                out catalogGroupCache))
                                                        {
                                                            var seriesStructId = catalogGroupCache.AssetStructsMapById
                                                                .Values.FirstOrDefault(x => x.IsSeriesAssetStruct)?.Id;

                                                            episodeStructId = seriesStructId == null
                                                                ? null
                                                                : catalogGroupCache.AssetStructsMapById
                                                                .Values.FirstOrDefault(x =>
                                                                        x.ParentId > 0 && x.ParentId == seriesStructId)?.Id;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        episodeStructId = NotificationCache.Instance()
                                                            .GetEpisodeMediaTypeId(groupId);
                                                    }
                                                }

                                                string mediaTypeIdValue =
                                                    Api.api.GetStringParamFromExtendedSearchResult(ecr,
                                                        "media_type_id");
                                                long mediaTypeId;

                                                if (long.TryParse(mediaTypeIdValue, out mediaTypeId) &&
                                                    (mediaTypeId == episodeStructId ||
                                                     (episodeStructId == null && IsMediaTypeEpisodeLike(isOPC, groupId, mediaTypeId))))
                                                {
                                                    if (!seriesMap.ContainsKey(seriesId))
                                                    {
                                                        seriesMap.Add(seriesId, watched);
                                                    }
                                                    else
                                                    {
                                                        if (watched.LastWatch > seriesMap[seriesId].LastWatch)
                                                        {
                                                            activeMediaIds.Remove(
                                                                int.Parse(seriesMap[seriesId].AssetId));
                                                            seriesMap[seriesId] = watched;
                                                        }
                                                        else
                                                        {
                                                            addToList = false;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        if (addToList)
                                        {
                                            activeMediaIds.Add(assetId);
                                        }
                                    }

                                    if (searchResult.AssetType == eAssetTypes.EPG)
                                    {
                                        activeEpg.Add(assetId);
                                        unFilteredresult.First(x =>
                                                int.Parse(x.AssetId) == assetId &&
                                                x.AssetTypeId == (int) eAssetTypes.EPG)
                                            .UpdateDate = searchResult.m_dUpdateDate;
                                    }
                                }
                            }

                            //remove medias that are not active
                            unFilteredresult.RemoveAll(x =>
                                x.AssetTypeId > 1 && !activeMediaIds.Contains(int.Parse(x.AssetId)));

                            //remove programs that are not active
                            unFilteredresult.RemoveAll(x =>
                                x.AssetTypeId == (int) eAssetTypes.EPG && !activeEpg.Contains(int.Parse(x.AssetId)));
                        }

                        var unFilteredRecordings = unFilteredresult.Where(x => x.AssetTypeId == (int) eAssetTypes.NPVR);

                        if (unFilteredRecordings != null)
                        {
                            var recordings = Core.ConditionalAccess.Module.SearchDomainRecordings(groupId, siteGuid,
                                domainId, new TstvRecordingStatus[] {TstvRecordingStatus.Recorded}, string.Empty,
                                0, 0,
                                new OrderObj()
                                    {m_eOrderBy = OrderBy.ID, m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC},
                                true, null);

                            if (recordings != null && recordings.Recordings?.Count > 0)
                            {
                                foreach (var item in unFilteredRecordings)
                                {
                                    var recording =
                                        recordings.Recordings.FirstOrDefault(x => x.Id.ToString().Equals(item.AssetId));
                                    if (recording != null)
                                    {
                                        item.EpgId = recording.EpgId;
                                    }
                                }
                            }

                            //remove recordings that are not active
                            unFilteredresult.RemoveAll(x => x.AssetTypeId == (int) eAssetTypes.NPVR && x.EpgId == 0);
                        }
                    }

                    // order list
                    switch (orderDir)
                    {
                        case ApiObjects.SearchObjects.OrderDir.ASC:

                            unFilteredresult = unFilteredresult.OrderBy(x => x.LastWatch).ToList();
                            break;
                        case ApiObjects.SearchObjects.OrderDir.DESC:
                        case ApiObjects.SearchObjects.OrderDir.NONE:
                        default:

                            unFilteredresult = unFilteredresult.OrderByDescending(x => x.LastWatch).ToList();
                            break;
                    }

                    // update total items
                    totalItems = unFilteredresult.Count;

                    // page index /size. if size and index are 0 return all
                    if (pageSize == 0 && pageIndex == 0)
                    {
                        usersWatchHistory = unFilteredresult;
                    }
                    else
                    {
                        usersWatchHistory = unFilteredresult.Skip(pageSize * pageIndex).Take(pageSize).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Debug(
                    $"An Exception was occurred in GetUserWatchHistory. groupId:{groupId}, siteGuid:{siteGuid}, filterStatus:{filterStatus}, numOfDays:{numOfDays}.",
                    ex);
                throw ex;
            }

            return usersWatchHistory;
        }

        private static bool IsMediaTypeEpisodeLike(bool isOpc, int groupId, long mediaTypeId)
        {
            if (isOpc && CatalogManagement.CatalogManager.Instance
                .TryGetCatalogGroupCacheFromCache(groupId,
                    out var catalogGroupCache))
            {
                if (!catalogGroupCache.AssetStructsMapById.ContainsKey(mediaTypeId))
                {
                    return false;
                }

                return catalogGroupCache.AssetStructsMapById[mediaTypeId].ParentId != null;
            }

            return false;
        }

        public static List<WatchHistory> GetRawUserWatchHistory(int groupId, string siteGuid, List<int> assetTypes,
            List<string> assetIds, List<int> excludedAssetTypes, eWatchStatus filterStatus, int numOfDays)
        {
            List<WatchHistory> unFilteredresult = new List<WatchHistory>();

            try
            {
                int userId = 0;
                int.TryParse(siteGuid, out userId);

                var mediaMarkLogs = GetUserMediaMarks(groupId, userId, numOfDays).ToList();
                if (mediaMarkLogs.Count == 0) return unFilteredresult;

                int finishedPercent = CatalogLogic.FINISHED_PERCENT_THRESHOLD;
                var generalPartnerConfig = GeneralPartnerConfigManager.Instance.GetGeneralPartnerConfig(groupId);
                if (generalPartnerConfig != null && generalPartnerConfig.FinishedPercentThreshold.HasValue)
                {
                    finishedPercent = generalPartnerConfig.FinishedPercentThreshold.Value;
                }

                unFilteredresult = mediaMarkLogs.Select(mediaMarkLog =>
                {
                    int recordingId = 0;
                    int.TryParse(mediaMarkLog.NpvrID, out recordingId);

                    return new WatchHistory()
                    {
                        AssetId = mediaMarkLog.AssetID.ToString(),
                        Duration = mediaMarkLog.FileDuration,
                        AssetTypeId = mediaMarkLog.AssetTypeId,
                        LastWatch = mediaMarkLog.CreatedAtEpoch,
                        Location = mediaMarkLog.AssetAction.ToLower().Equals("finish") ? mediaMarkLog.FileDuration : mediaMarkLog.Location,
                        RecordingId = recordingId,
                        UserID = userId,
                        IsFinishedWatching = mediaMarkLog.IsFinished(finishedPercent)
                    };
                }).ToList();

                // filter status
                switch (filterStatus)
                {
                    case eWatchStatus.Progress:
                        // remove all finished
                        unFilteredresult.RemoveAll(x => x.IsFinishedWatching);
                        break;
                    case eWatchStatus.Done:
                        // remove all in progress
                        unFilteredresult.RemoveAll(x => !x.IsFinishedWatching);
                        break;
                    case eWatchStatus.All:
                    default:
                        break;
                }

                // filter asset types
                if (assetTypes != null && assetTypes.Count > 0)
                    unFilteredresult = unFilteredresult.Where(x => assetTypes.Contains(x.AssetTypeId)).ToList();

                // filter asset ids
                if (assetIds != null && assetIds.Count > 0)
                    unFilteredresult = unFilteredresult.Where(x => assetIds.Contains(x.AssetId)).ToList();

                // filter excluded asset types
                if (excludedAssetTypes != null && excludedAssetTypes.Count > 0)
                    unFilteredresult.RemoveAll(x => excludedAssetTypes.Contains(x.AssetTypeId));
            }
            catch (Exception ex)
            {
            }

            return unFilteredresult;
        }

        private static IEnumerable<UserMediaMark> GetUserMediaMarks(int groupId, int userId, int numOfDays)
        {
            var mediaMarksManager = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.MEDIAMARK);
            var userAssetMarks = mediaMarksManager.Get<UserMediaMarks>(UtilsDal.GetUserAllAssetMarksDocKey(userId.ToString()));
            if (userAssetMarks?.mediaMarks == null || userAssetMarks.mediaMarks.Count == 0)
            {
                return Enumerable.Empty<UserMediaMark>();
            }

            // build date filter
            long minFilterdate = numOfDays > 0 ? DateTime.UtcNow.AddDays(-numOfDays).ToUtcUnixTimestampSeconds() : 0;
            var utcNow = DateUtils.GetUtcUnixTimestampNow();

            var dateFilteredResult = userAssetMarks.mediaMarks.Where(mark => mark.CreatedAt > minFilterdate && (mark.ExpiredAt == 0 || mark.ExpiredAt > utcNow));

            return GetAssetMarks(dateFilteredResult, mediaMarksManager, userId, groupId);
        }

        // TODO agressive migration
        private static IEnumerable<UserMediaMark> GetAssetMarks(IEnumerable<AssetAndLocation> mediaMarks, ICouchbaseManager mediaMarksManager, int userId, int groupId)
        {
            var oldModelMediaMarks = new List<AssetAndLocation>();
            var mediaMarksNewModelEnabled = MediaMarksNewModel.Enabled(groupId);
            foreach (var mediaMark in mediaMarks)
            {
                var e = mediaMark.Extra;
                if (!mediaMarksNewModelEnabled || e == null)
                {
                    oldModelMediaMarks.Add(mediaMark);
                    continue;
                }

                yield return new UserMediaMark
                {
                    UDID = e.UDID,
                    AssetID = mediaMark.AssetId,
                    UserID = userId,
                    Location = e.Location,
                    CreatedAt = DateTimeOffset.FromUnixTimeSeconds(mediaMark.CreatedAt).UtcDateTime,
                    NpvrID = mediaMark.NpvrId,
                    playType = e.PlayType.ToString(),
                    FileDuration = e.FileDuration,
                    AssetAction = e.AssetAction.ToString(),
                    AssetTypeId = e.AssetTypeId,
                    CreatedAtEpoch = mediaMark.CreatedAt,
                    MediaConcurrencyRuleIds = null, // never read
                    AssetType = mediaMark.AssetType,
                    ExpiredAt = mediaMark.ExpiredAt,
                    LocationTagValue = e.LocationTagValue
                };
            }

            var mediaMarkKeys = CatalogDAL.ConvertUserMediaMarksToKeys(userId.ToString(), oldModelMediaMarks);
            var mediaMarkLogsDictionary = mediaMarksManager.GetValues<MediaMarkLog>(mediaMarkKeys, true, true);
            foreach (var kv in mediaMarkLogsDictionary)
            {
                yield return kv.Value.LastMark;
            }
        }

        public static void WriteNewWatcherMediaActionLog(int nWatcherID, string sSessionID, int nBillingTypeID,
            int nOwnerGroupID, int nQualityID, int nFormatID, int nMediaID, int nMediaFileID, int nGroupID,
            int nCDNID, int nActionID, int nCountryID, int nPlayerID, int nLoc, int nBrowser, int nPlatform,
            string sSiteGUID, string sUDID, string userIP)
        {
            try
            {
                object[] obj = null;

                // We write an empty string as the first parameter to split the start of the log from the mediaEoh row data
                if (ApplicationConfiguration.Current.CatalogLogicConfiguration.ShouldAddUserIPToStats.Value)
                {
                    obj = new object[]
                    {
                        " ", nWatcherID, sSessionID, nBillingTypeID, nOwnerGroupID, nQualityID, nFormatID, nMediaID,
                        nMediaFileID, nGroupID, nCDNID,
                        nActionID, nCountryID, nPlayerID, nLoc, nBrowser, nPlatform, sSiteGUID, sUDID, userIP
                    };
                }
                else
                {
                    obj = new object[]
                    {
                        " ", nWatcherID, sSessionID, nBillingTypeID, nOwnerGroupID, nQualityID, nFormatID, nMediaID,
                        nMediaFileID, nGroupID, nCDNID,
                        nActionID, nCountryID, nPlayerID, nLoc, nBrowser, nPlatform, sSiteGUID, sUDID
                    };
                }

                newWatcherMediaActionLog.Info(string.Join(",", obj));
            }
            catch (Exception ex)
            {
                log.Error(string.Format(
                    "Error in WriteNewWatcherMediaActionLog, mediaID: {0}, mediaFileID: {1}, groupID: {2}, actionID: {3}, userId: {4}",
                    nMediaID, nMediaFileID, nGroupID, nActionID, sSiteGUID), ex);
            }
        }

        public static UserInterestsMetasAndTags GetUserPreferences(int partnerId, int userId)
        {
            // get user interests
            UserInterests userInterests = InterestDal.GetUserInterest(partnerId, userId);
            if (userInterests == null || userInterests.UserInterestList == null ||
                userInterests.UserInterestList.Count == 0)
            {
                log.DebugFormat("User interests were not found. Partner ID: {0}, User ID: {1}", partnerId, userId);
                return null;
            }

            if (GroupSettingsManager.Instance.IsOpc(partnerId))
            {
                return GetUserPreferencesForOpcAccount(partnerId, userId, userInterests);
            }

            UserInterestsMetasAndTags result = new UserInterestsMetasAndTags();

            // get partner interests configuration
            List<ApiObjects.Meta> availableTopics = NotificationCache.Instance().GetPartnerTopicInterests(partnerId);
            if (availableTopics == null || availableTopics.Count == 0)
            {
                log.DebugFormat("Partner interest configuration was not found. Partner ID: {0}, User ID: {1}",
                    partnerId, userId);
                return null;
            }

            // iterate through all tree
            foreach (var interestLeaf in userInterests.UserInterestList)
            {
                // iterate through branch
                UserInterestTopic node = interestLeaf.Topic;
                while (node != null)
                {
                    // process node
                    var topic = availableTopics.FirstOrDefault(x => x.Id == node.MetaId);
                    if (topic != null)
                    {
                        List<string> valueList = new List<string>();
                        if (topic.MultipleValue)
                        {
                            if (result.Tags.TryGetValue(topic.Name, out valueList))
                                valueList.Add(node.Value);
                            else
                                result.Tags.Add(topic.Name, new List<string> {node.Value});
                        }
                        else
                        {
                            if (result.Metas.TryGetValue(topic.Name, out valueList))
                                valueList.Add(node.Value);
                            else
                                result.Metas.Add(topic.Name, new List<string> {node.Value});
                        }
                    }

                    // go to parent node
                    node = node.ParentTopic;
                }
            }

            return result;
        }

        private static UserInterestsMetasAndTags GetUserPreferencesForOpcAccount(int partnerId, int userId,
            UserInterests userInterests)
        {
            UserInterestsMetasAndTags result = new UserInterestsMetasAndTags();

            if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(partnerId,
                out CatalogGroupCache catalogGroupCache))
            {
                log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling AddUserInterest",
                    partnerId);
                return null;
            }

            // get partner interests configuration
            var availableTopics = catalogGroupCache.TopicsMapById.Values.Where(x => x.IsInterest).ToList();
            if (availableTopics == null || availableTopics.Count == 0)
            {
                log.DebugFormat("Partner interest configuration was not found. Partner ID: {0}, User ID: {1}",
                    partnerId, userId);
                return null;
            }

            // iterate through all tree
            foreach (var interestLeaf in userInterests.UserInterestList)
            {
                // iterate through branch
                UserInterestTopic node = interestLeaf.Topic;
                while (node != null)
                {
                    // process node
                    var topic = availableTopics.FirstOrDefault(x => x.Id.ToString() == node.MetaId);
                    if (topic != null)
                    {
                        List<string> valueList = new List<string>();

                        if (topic.Type == ApiObjects.MetaType.Tag)
                        {
                            if (result.Tags.TryGetValue(topic.SystemName, out valueList))
                                valueList.Add(node.Value);
                            else
                                result.Tags.Add(topic.SystemName, new List<string> {node.Value});
                        }
                        else
                        {
                            if (result.Metas.TryGetValue(topic.SystemName, out valueList))
                                valueList.Add(node.Value);
                            else
                                result.Metas.Add(topic.SystemName, new List<string> {node.Value});
                        }
                    }

                    // go to parent node
                    node = node.ParentTopic;
                }
            }

            return result;
        }

        internal static MetaResponse UpdateGroupMeta(int groupId, ApiObjects.Meta meta)
        {
            MetaResponse response = new MetaResponse();
            string logData = string.Format("groupId: {0}, meta: {1}", groupId, JsonConvert.SerializeObject(meta));

            try
            {
                ApiObjects.Meta dbMeta;
                response = ValidateMetaToUpdate(groupId, meta, out dbMeta);
                if (response.Status != null && response.Status.Code != (int) eResponseStatus.OK)
                    return response;

                // update meta with partnerId
                meta.PartnerId = dbMeta.PartnerId;

                // get partner topics
                var partnerTopicInterests = NotificationCache.Instance().GetPartnerTopicInterests(groupId);

                if (meta.SkipFeatures)
                {
                    // get meta from DB - check if exist
                    if (partnerTopicInterests == null || partnerTopicInterests.Count == 0)
                    {
                        log.ErrorFormat("Error getting partner topic interests. {0}", logData);
                        response.Status = new ApiObjects.Response.Status()
                            {Code = (int) eResponseStatus.Error, Message = eResponseStatus.Error.ToString()};
                        return response;
                    }

                    ApiObjects.Meta apiMeta = partnerTopicInterests.Where(x =>
                            x.PartnerId == meta.PartnerId && x.Name == meta.Name && x.AssetType == meta.AssetType)
                        .FirstOrDefault();
                    if (apiMeta == null)
                    {
                        log.ErrorFormat("Error while UpdateGroupMeta. currentGroupId: {0}, MetaName:{1}",
                            meta.PartnerId, meta.Name);
                        response.Status = new ApiObjects.Response.Status((int) eResponseStatus.NotaTopicInterestMeta,
                            "Not a topic interest meta");
                        return response;
                    }

                    meta.Features = apiMeta.Features;

                    // update
                    response = SetTopicInterest(meta.PartnerId, meta, partnerTopicInterests);
                }
                else if (meta.Features != null && !meta.Features.Contains(MetaFeatureType.USER_INTEREST))
                {
                    // delete meta from TopicInterest
                    if (partnerTopicInterests == null || partnerTopicInterests.Count == 0)
                    {
                        log.ErrorFormat("Error getting partner topic interests. {0}", logData);
                        response.Status = new ApiObjects.Response.Status()
                            {Code = (int) eResponseStatus.Error, Message = eResponseStatus.Error.ToString()};
                        return response;
                    }

                    response = DeleteTopicInterest(meta.PartnerId, meta, partnerTopicInterests);
                }
                else if (meta.Features != null && meta.Features.Contains(MetaFeatureType.USER_INTEREST))
                {
                    // update
                    response = SetTopicInterest(meta.PartnerId, meta, partnerTopicInterests);
                }

                // clear cache after update
                NotificationCache.Instance().RemoveTopicInterestsFromCache(groupId);
            }
            catch (Exception ex)
            {
                response.Status =
                    new ApiObjects.Response.Status((int) eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.ErrorFormat("Failed groupID={0}, meta={1}, error: {2}", groupId, JsonConvert.SerializeObject(meta),
                    ex);
            }

            return response;
        }

        private static MetaResponse ValidateMetaToUpdate(int groupId, ApiObjects.Meta meta, out ApiObjects.Meta dbMeta)
        {
            dbMeta = null;
            MetaResponse response = new MetaResponse();
            string logData = string.Format("groupId: {0}, meta: {1}", groupId, JsonConvert.SerializeObject(meta));

            if (meta == null)
            {
                log.ErrorFormat("No meta to update");
                response.Status =
                    new ApiObjects.Response.Status((int) eResponseStatus.NoMetaToUpdate, NO_META_TO_UPDATE);
                return response;
            }

            if (string.IsNullOrEmpty(meta.Name))
            {
                log.ErrorFormat("Meta Name missing. {0}", logData);
                response.Status = new ApiObjects.Response.Status((int) eResponseStatus.NameRequired, NAME_REQUIRED);
                return response;
            }

            // make sure that meta/tag are related to partner
            var metaResponse = TopicManager.Instance.GetGroupMetaList(groupId, meta.AssetType, meta.Type,
                MetaFieldName.None, MetaFieldName.None, null);
            if (metaResponse == null || metaResponse.Status == null ||
                metaResponse.Status.Code != (int) eResponseStatus.OK || metaResponse.MetaList == null ||
                metaResponse.MetaList.Count == 0)
            {
                log.ErrorFormat("Error while getting group meta list. {0}", logData);
                response.Status =
                    new ApiObjects.Response.Status((int) eResponseStatus.Error, eResponseStatus.Error.ToString());
                return response;
            }

            dbMeta = metaResponse.MetaList.Where(x => x.Id == meta.Id).FirstOrDefault();
            // meta not exist
            if (dbMeta == null)
            {
                log.ErrorFormat("Error meta not found. {0}", logData);
                response.Status = new ApiObjects.Response.Status((int) eResponseStatus.MetaNotFound, META_NOT_EXIST);
                return response;
            }

            if (dbMeta.Name != meta.Name)
            {
                log.ErrorFormat("Wrong meta name. {0}", logData);
                response.Status = new ApiObjects.Response.Status((int) eResponseStatus.WrongMetaName, WRONG_META_NAME);
                return response;
            }

            if (meta.PartnerId != 0 && dbMeta.PartnerId != meta.PartnerId)
            {
                log.ErrorFormat("Meta not belong to partner. {0}", logData);
                response.Status = new ApiObjects.Response.Status((int) eResponseStatus.MetaNotBelongtoPartner,
                    META_NOT_BELONG_TO_PARTNER);
                return response;
            }

            response.Status = new ApiObjects.Response.Status((int) eResponseStatus.OK, eResponseStatus.OK.ToString());
            return response;
        }

        private static ApiObjects.Response.Status ValidateTopic(ApiObjects.Meta meta,
            List<ApiObjects.Meta> partnerTopicInterests)
        {
            if (string.IsNullOrEmpty(meta.ParentId) || meta.ParentId == "0")
            {
                return new ApiObjects.Response.Status((int) eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            // parent meta id should not point to the meta itself.
            if (meta.ParentId == meta.Id)
            {
                log.ErrorFormat("Error. Parent meta id should not point to the meta itself. {0}",
                    JsonConvert.SerializeObject(meta));
                return new ApiObjects.Response.Status((int) eResponseStatus.ParentIdShouldNotPointToItself,
                    PARENT_ID_SHOULD_NOT_POINT_TO_ITSELF);
            }

            // parent meta id should be recognized as user_interst
            if ((partnerTopicInterests == null || partnerTopicInterests.Count == 0) &&
                (!string.IsNullOrEmpty(meta.ParentId) || meta.ParentId == "0"))
            {
                log.ErrorFormat("Error. Parent meta id should be recognized as user_interst. {0}",
                    JsonConvert.SerializeObject(meta));
                return new ApiObjects.Response.Status((int) eResponseStatus.ParentIdNotAUserInterest,
                    PARENT_ID_NOT_A_USER_INTERSET);
            }

            var parentMetaId = partnerTopicInterests.Where(x => x.Id == meta.ParentId).FirstOrDefault();
            if (parentMetaId == null)
            {
                log.ErrorFormat("Error. Parent meta id should be recognized as user_interst. {0}",
                    JsonConvert.SerializeObject(meta));
                return new ApiObjects.Response.Status((int) eResponseStatus.ParentIdNotAUserInterest,
                    PARENT_ID_NOT_A_USER_INTERSET);
            }

            // asset type of parent meta should be as meta asset type
            if (parentMetaId.AssetType != meta.AssetType)
            {
                log.ErrorFormat("Error. asset type of parent meta should be as meta asset type. {0}",
                    JsonConvert.SerializeObject(meta));
                return new ApiObjects.Response.Status((int) eResponseStatus.ParentAssetTypeDiffrentFromMeta,
                    PARENT_ASSET_TYPE_DIFFRENT_FROM_META);
            }

            // partner parent should be the some as meta partner
            if (parentMetaId.PartnerId != meta.PartnerId)
            {
                log.ErrorFormat("Error. partner parent should be the some as meta partner. {0}",
                    JsonConvert.SerializeObject(meta));
                return new ApiObjects.Response.Status((int) eResponseStatus.ParentParnerDiffrentFromMetaPartner,
                    PARENT_PARNER_DIFFRENT_FROM_META_PARTNER);
            }

            // parentMetaId should be associated to only 1 meta
            var someMeta = partnerTopicInterests.Where(x => x.ParentId == meta.ParentId).FirstOrDefault();
            if (someMeta != null && someMeta.Id != meta.Id)
            {
                log.ErrorFormat("Error. parentMetaId should be associated to only 1 meta. {0}",
                    JsonConvert.SerializeObject(meta));
                return new ApiObjects.Response.Status((int) eResponseStatus.ParentDuplicateAssociation,
                    PARENT_DUPLICATE_ASSOCIATION);
            }

            return new ApiObjects.Response.Status((int) eResponseStatus.OK, eResponseStatus.OK.ToString());
        }

        private static MetaResponse SetTopicInterest(int groupId, ApiObjects.Meta meta,
            List<ApiObjects.Meta> partnerTopicInterests)
        {
            MetaResponse response = new MetaResponse();

            response.Status = ValidateTopic(meta, partnerTopicInterests);
            if (response.Status.Code != (int) eResponseStatus.OK)
            {
                return response;
            }

            ApiObjects.Meta updatedMeta = CatalogDAL.SetTopicInterest(groupId, meta);

            if (updatedMeta != null)
            {
                response.Status = new ApiObjects.Response.Status((int) eResponseStatus.OK, "Meta set changes");
                response.MetaList = new List<ApiObjects.Meta>();
                response.MetaList.Add(updatedMeta);
            }
            else
            {
                response.Status =
                    new ApiObjects.Response.Status((int) eResponseStatus.Error, "Meta failed set changes");
            }

            return response;
        }

        private static MetaResponse DeleteTopicInterest(int groupId, ApiObjects.Meta meta,
            List<ApiObjects.Meta> partnerTopicInterests)
        {
            MetaResponse response = new MetaResponse();

            // check if meta is user_interset before remove
            ApiObjects.Meta dbMeta = partnerTopicInterests.Where(x => x.Id == meta.Id).FirstOrDefault();
            if (dbMeta == null)
            {
                log.ErrorFormat("Error. Meta is not recognized as user interest. groupId: {0}, Meta:{1}", groupId,
                    JsonConvert.SerializeObject(meta));
                response.Status = new ApiObjects.Response.Status((int) eResponseStatus.MetaNotAUserinterest,
                    META_DOES_NOT_A_USER_INTEREST);
                return response;
            }

            if (!CatalogDAL.DeleteTopicInterest(groupId, meta.Name))
            {
                log.ErrorFormat("Error while DeleteTopicInterest. groupId: {0}, MetaName:{1}", groupId, meta.Name);
                response.Status =
                    new ApiObjects.Response.Status((int) eResponseStatus.Error, eResponseStatus.Error.ToString());
                return response;
            }

            response.Status = new ApiObjects.Response.Status((int) eResponseStatus.OK, "Meta set changes");

            //clear userInterset values
            meta.Features = null;
            meta.ParentId = string.Empty;

            response.MetaList = new List<ApiObjects.Meta>();
            response.MetaList.Add(meta);
            return response;
        }

        public static HashSet<string> GetTopicsToIgnoreOnBuildIndex()
        {
            HashSet<string> topicsToIgnore = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            topicsToIgnore.UnionWith(CatalogReservedFields.ReservedUnifiedSearchStringFields);
            topicsToIgnore.UnionWith(CatalogReservedFields.ReservedUnifiedSearchNumericFields);
            topicsToIgnore.UnionWith(CatalogReservedFields.ReservedUnifiedDateFields);

            return topicsToIgnore;
        }

        private static List<LanguageContainer> GetTagsLanguageContainer(List<TagTranslations> translatedTags,
            List<LanguageObj> groupLanguages)
        {
            List<LanguageContainer> langContainers = new List<LanguageContainer>();
            LanguageObj language = null;

            if (translatedTags?.Count > 0)
            {
                string value = string.Empty;
                int langId = 0;

                foreach (var item in translatedTags)
                {
                    langId = item.LanguageId;
                    value = item.Value;
                    if (!string.IsNullOrEmpty(value) && langId > 0)
                    {
                        language = groupLanguages.FirstOrDefault(x => x.ID == langId);
                        if (language != null)
                            langContainers.Add(new LanguageContainer()
                                {m_sLanguageCode3 = language.Code, m_sValue = value});
                    }
                }
            }

            return langContainers;
        }

        private static string GetElasticPrefixByFieldType(string fieldName, Group group)
        {
            var eFieldType = NextEpisodeService.GetFieldType(fieldName, group);
            switch (eFieldType)
            {
                case eFieldType.Tag:
                    return "tags";
                case eFieldType.StringMeta:
                    return "metas";
                default:
                    throw new Exception($"Unknown field type for fieldName={fieldName}");
            }
        }
    }
}
