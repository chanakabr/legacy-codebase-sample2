using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ApiObjects;
using ApiObjects.MediaMarks;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using ApiObjects.Statistics;
using Catalog.Cache;
using Catalog.Request;
using Catalog.Response;
using DAL;
using DalCB;
using ElasticSearch.Searcher;
using EpgBL;
using GroupsCacheManager;
using KLogMonitor;
using Newtonsoft.Json.Linq;
using NPVR;
using QueueWrapper;
using StatisticsBL;
using Tvinci.Core.DAL;
using TVinciShared;
using CachingHelpers;
using AdapterControllers;
using KlogMonitorHelper;
using System.IO;
using ApiObjects.PlayCycle;
using ApiObjects.Epg;
using System.Net;
using WS_API;
using Users;
using WS_Users;

namespace Catalog
{
    public class Catalog
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly KLogger statisticsLog = new KLogger("MediaEohLogger", true);
        private static readonly KLogger newWatcherMediaActionLog = new KLogger("NewWatcherMediaActionLogger", true);

        private static readonly string TAGS = "tags";
        private static readonly string METAS = "metas";

        private static readonly string LINEAR_MEDIA_TYPES_KEY = "LinearMediaTypes";
        private static readonly string PERMITTED_WATCH_RULES_KEY = "PermittedWatchRules";
        private static readonly int ASSET_STATS_VIEWS_INDEX = 0;
        private static readonly int ASSET_STATS_VOTES_INDEX = 1;
        private static readonly int ASSET_STATS_VOTES_SUM_INDEX = 2;
        private static readonly int ASSET_STATS_LIKES_INDEX = 3;

        private const int DEFAULT_SEARCHER_MAX_RESULTS_SIZE = 10000;

        internal const int DEFAULT_PWWAWP_MAX_RESULTS_SIZE = 8;
        internal const int DEFAULT_PWLALP_MAX_RESULTS_SIZE = 8;
        internal const int DEFAULT_PERSONAL_RECOMMENDED_MAX_RESULTS_SIZE = 20;
        internal const int FINISHED_PERCENT_THRESHOLD = 95;
        private static int DEFAULT_CURRENT_REQUEST_DAYS_OFFSET = 7;
        internal static readonly string STAT_ACTION_MEDIA_HIT = "mediahit";
        internal static readonly string STAT_ACTION_FIRST_PLAY = "firstplay";
        internal static readonly string STAT_ACTION_LIKE = "like";
        internal static readonly string STAT_ACTION_RATES = "rates";
        internal static readonly string STAT_ACTION_RATE_VALUE_FIELD = "rate_value";
        internal static readonly string STAT_SLIDING_WINDOW_AGGREGATION_NAME = "sliding_window";
        private const string USE_OLD_IMAGE_SERVER_KEY = "USE_OLD_IMAGE_SERVER";
        private static readonly long UNIX_TIME_1980 = DateUtils.DateTimeToUnixTimestamp(new DateTime(1980, 1, 1, 0, 0, 0));

        private static readonly string CB_MEDIA_MARK_DESGIN = ODBCWrapper.Utils.GetTcmConfigValue("cb_media_mark_design");

        private static readonly HashSet<string> reservedUnifiedSearchStringFields = new HashSet<string>()
		            {
			            "name",
			            "description",
			            "epg_channel_id",
                        "media_id",
                        "epg_id",
                        "crid",
                        "...."
		            };

        private static readonly HashSet<string> reservedUnifiedSearchNumericFields = new HashSet<string>()
		            {
			            "like_counter",
			            "views",
			            "rating",
			            "votes",
                        "epg_channel_id"
		            };

        private static int maxNGram = -1;

        internal static int GetCurrentRequestDaysOffset()
        {
            int res = DEFAULT_CURRENT_REQUEST_DAYS_OFFSET;
            string daysOffset = Utils.GetWSURL("CURRENT_REQUEST_DAYS_OFFSET");
            if (daysOffset.Length > 0 && Int32.TryParse(daysOffset, out res) && res > 0)
                return res;
            return DEFAULT_CURRENT_REQUEST_DAYS_OFFSET;
        }

        /*Get All Relevant Details About Media (by id) , 
         Use Stored Procedure */
        internal static bool CompleteDetailsForMediaResponse(MediasProtocolRequest mediaRequest, ref MediaResponse mediaResponse, int nStartIndex, int nEndIndex)
        {
            List<MediaObj> lMediaObj = new List<MediaObj>();
            int totalItems = 0;
            int groupId = mediaRequest.m_nGroupID;
            Filter filter = mediaRequest.m_oFilter;
            string siteGuid = mediaRequest.m_sSiteGuid;
            List<int> mediaIds = mediaRequest.m_lMediasIds;

            try
            {
                lMediaObj = CompleteMediaDetails(mediaIds, nStartIndex, ref nEndIndex, ref totalItems, groupId, filter, siteGuid);

                mediaResponse.m_nTotalItems = totalItems;
                mediaResponse.m_lObj = lMediaObj.Select(media => (BaseObject)media).ToList();

                return true;
            }

            catch (Exception ex)
            {
                log.Error("faild to complete details", ex);
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
        internal static List<MediaObj> CompleteMediaDetails(List<int> mediaIds, int groupId, Filter filter, string siteGuid)
        {
            int startIndex = 0;
            int endIndex = 0;
            int totalItems = 0;

            return Catalog.CompleteMediaDetails(mediaIds, startIndex, ref endIndex, ref totalItems, groupId, filter, siteGuid);
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
             ref int totalItems, int groupId, Filter filter, string siteGuid)
        {
            List<MediaObj> mediaObjects = new List<MediaObj>();

            bool bIsMainLang = Utils.IsLangMain(groupId, filter.m_nLanguage);

            if (nStartIndex == 0 && nEndIndex == 0 && mediaIds != null && mediaIds.Count() > 0)
            {
                nEndIndex = mediaIds.Count();
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
            ContextData contextData = new ContextData();

            List<int> nonExistingMediaIDs = new List<int>();

            //complete media id details 
            for (int i = nStartIndex; i < nEndIndex; i++)
            {
                int nMedia = mediaIds[i];

                tasks[i - nStartIndex] = Task.Factory.StartNew((obj) =>
                {
                    // load monitor and logs context data
                    contextData.Load();

                    try
                    {
                        int taskMediaID = (int)obj;

                        var currentMedia = GetMediaDetails(taskMediaID, groupId, filter, siteGuid, bIsMainLang, lSubGroup);
                        dMediaObj[taskMediaID] = currentMedia;

                        // If couldn't get media details for this media - probably it doesn't exist, and it shouldn't appear in ES index
                        if (currentMedia == null)
                        {
                            nonExistingMediaIDs.Add(taskMediaID);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Failed in GetMediaDetails. Group ID = {0}, media id = {1}.", groupId, obj, ex);
                    }
                }, nMedia);
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
                    var currentMedia = dMediaObj[nMedia];

                    mediaObjects.Add(currentMedia);
                }
            }

            // If there are any media that we couldn't find via stored procedure - delete them off from ES index
            if (nonExistingMediaIDs != null && nonExistingMediaIDs.Count > 0)
                // If there are any media that we couldn't find via stored procedure - delete them off from ES index
                if (nonExistingMediaIDs != null && nonExistingMediaIDs.Count > 0)
                {
                    List<int> idsToUpdate = new List<int>();
                    List<int> idsToDelete = new List<int>();
                    List<int> idsToTurnOff = new List<int>();

                    foreach (var id in nonExistingMediaIDs)
                    {
                        // Look for the origin row of the media in the database
                        DataRow currentMediaRow = ODBCWrapper.Utils.GetTableSingleRow("media", id);

                        // If no row returned - delete the record in index
                        if (currentMediaRow == null)
                        {
                            idsToDelete.Add(id);
                        }
                        else
                        {
                            int status = ODBCWrapper.Utils.ExtractInteger(currentMediaRow, "status");

                            // if the status is invalid - delete the record in index
                            if (status != 1)
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
                    }

                    // Add messages to queue for each type of action
                    Catalog.Update(idsToDelete, groupId, eObjectType.Media, eAction.Delete);
                    Catalog.Update(idsToUpdate, groupId, eObjectType.Media, eAction.Update);
                    Catalog.Update(idsToTurnOff, groupId, eObjectType.Media, eAction.Off);
                }

            return mediaObjects;
        }

        private static MediaObj GetMediaDetails(int nMedia, MediasProtocolRequest mediaRequest, bool bIsMainLang, List<int> lSubGroup)
        {
            return GetMediaDetails(nMedia, mediaRequest.m_nGroupID, mediaRequest.m_oFilter, mediaRequest.m_sSiteGuid, bIsMainLang, lSubGroup);
        }

        private static MediaObj GetMediaDetails(int nMedia, int groupId, Filter filter, string siteGuid, bool bIsMainLang, List<int> lSubGroup)
        {
            bool result = true;

            try
            {
                MediaObj oMediaObj = new MediaObj();

                string sEndDate = string.Empty;
                bool bOnlyActiveMedia = true;
                bool bUseStartDate = true;
                int nLanguage = 0;

                sEndDate = ProtocolsFuncs.GetFinalEndDateField(true);

                if (filter != null)
                {
                    bOnlyActiveMedia = filter.m_bOnlyActiveMedia;
                    bUseStartDate = filter.m_bUseStartDate;
                    nLanguage = filter.m_nLanguage;
                }

                DataSet ds = CatalogDAL.Get_MediaDetails(groupId, nMedia, siteGuid, bOnlyActiveMedia, nLanguage, sEndDate, bUseStartDate, lSubGroup);

                if (ds == null)
                    return null;
                if (ds.Tables.Count >= 6)
                {
                    int assetGroupId = 0;
                    int isLinear = 0;
                    bool isMedia = GetMediaBasicDetails(ref oMediaObj, ds.Tables[0], ds.Tables[5], bIsMainLang, ref assetGroupId, ref isLinear);

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
                        oMediaObj.m_lFiles = FilesValues(ds.Tables[2], ref oMediaObj.m_lBranding, filter.m_noFileUrl, ref result);
                        if (!result)
                        {
                            return null;
                        }
                        oMediaObj.m_lMetas = GetMetaDetails(ds.Tables[3], ref result);
                        if (!result)
                        {
                            return null;
                        }
                        oMediaObj.m_lTags = GetTagsDetails(ds.Tables[4], bIsMainLang, ref result);
                        if (!result)
                        {
                            return null;
                        }

                        if (isLinear != 0 && !string.IsNullOrEmpty(oMediaObj.m_ExternalIDs))
                        {
                            // get linear Channel settings 
                            Dictionary<string, LinearChannelSettings> linearChannelSettings = CatalogCache.Instance().GetLinearChannelSettings(groupId, new List<string>(){oMediaObj.m_ExternalIDs});
                            if (linearChannelSettings != null && linearChannelSettings.ContainsKey(oMediaObj.m_ExternalIDs) && linearChannelSettings.Values != null)
                            {
                                oMediaObj.EnableCatchUp = linearChannelSettings[oMediaObj.m_ExternalIDs].EnableCatchUp;
                                oMediaObj.EnableCDVR = linearChannelSettings[oMediaObj.m_ExternalIDs].EnableCDVR;
                                oMediaObj.EnableStartOver = linearChannelSettings[oMediaObj.m_ExternalIDs].EnableStartOver;
                                oMediaObj.EnableTrickPlay = linearChannelSettings[oMediaObj.m_ExternalIDs].EnableTrickPlay;
                                oMediaObj.CatchUpBuffer = linearChannelSettings[oMediaObj.m_ExternalIDs].CatchUpBuffer;
                                oMediaObj.TrickPlayBuffer = linearChannelSettings[oMediaObj.m_ExternalIDs].TrickPlayBuffer;
                                oMediaObj.EnableRecordingPlaybackNonEntitledChannel = linearChannelSettings[oMediaObj.m_ExternalIDs].EnableRecordingPlaybackNonEntitledChannel;
                                oMediaObj.EnableRecordingPlaybackNonExistingChannel = linearChannelSettings[oMediaObj.m_ExternalIDs].EnableRecordingPlaybackNonExistingChannel;
                            }
                        }

                        /*last watched - By SiteGuid <> 0*/

                        if (!string.IsNullOrEmpty(siteGuid) && siteGuid != "0")
                        {
                            DateTime? dtLastWatch = null;

                            // ask CB for it
                            try
                            {
                                dtLastWatch = CatalogDAL.Get_MediaUserLastWatch(nMedia, siteGuid);
                            }
                            catch (Exception ex)
                            {
                                log.Error("Error - " +
                                    string.Format("Failed getting last watched date of SiteGuid = {0}, Media = {1}, error of type: {2}", siteGuid, nMedia, ex.Message),
                                    ex);
                            }

                            oMediaObj.m_dLastWatchedDate = dtLastWatch;
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

      

        /*Insert all tags that return from the "CompleteDetailsForMediaResponse" into List<Tags>*/
        private static List<Tags> GetTagsDetails(DataTable dtTags, bool bIsMainLang, ref bool result)
        {
            try
            {
                result = true;
                List<Tags> lTags = new List<Tags>();
                Tags oTags = new Tags();
                int nTagId;
                int i = 0;
                if (dtTags != null)
                {
                    while (i < dtTags.Rows.Count)
                    {
                        oTags = new Tags();
                        oTags.m_lValues = new List<string>();

                        oTags.m_oTagMeta = new TagMeta(Utils.GetStrSafeVal(dtTags.Rows[i], "tag_type_name"), typeof(string).ToString());
                        nTagId = Utils.GetIntSafeVal(dtTags.Rows[i], "tag_type_id");
                        oTags.m_lValues.Add(Utils.GetStrSafeVal(dtTags.Rows[i], "value"));
                        int j = i + 1;
                        for (; j < dtTags.Rows.Count; j++)
                        {
                            if (nTagId != Utils.GetIntSafeVal(dtTags.Rows[j], "tag_type_id"))
                                break;
                            oTags.m_lValues.Add(Utils.GetStrSafeVal(dtTags.Rows[j], "value"));
                        }

                        if (oTags.m_lValues != null && oTags.m_lValues.Count > 0)
                        {
                            lTags.Add(oTags);
                        }
                        i = j;
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

        /*Insert all metas that return from the "CompleteDetailsForMediaResponse" into List<Metas>*/
        private static List<Metas> GetMetaDetails(DataTable dtMeta, ref bool result)
        {
            try
            {
                result = true;
                List<Metas> lMetas = new List<Metas>();

                if (dtMeta != null && dtMeta.Rows != null && dtMeta.Rows.Count > 0)
                {
                    Metas oMeta = new Metas();
                    string sFieldName;
                    string sFieldVal;
                    string sName;
                    for (int i = 1; i < 20; i++)
                    {
                        sFieldName = "META" + i.ToString() + "_STR_NAME";
                        sFieldVal = "META" + i.ToString() + "_STR";
                        if (dtMeta.Rows[0][sFieldName] != DBNull.Value)
                        {
                            sName = Utils.GetStrSafeVal(dtMeta.Rows[0], sFieldName);
                            if (!string.IsNullOrEmpty(sName))
                            {
                                if (dtMeta.Rows[0][sFieldVal] != DBNull.Value && !string.IsNullOrEmpty(dtMeta.Rows[0][sFieldVal].ToString()))
                                {
                                    oMeta.m_oTagMeta = new TagMeta(sName, typeof(string).ToString());
                                    oMeta.m_sValue = Utils.GetStrSafeVal(dtMeta.Rows[0], sFieldVal);
                                    lMetas.Add(oMeta);
                                }
                            }
                        }
                        oMeta = new Metas();
                    }

                    for (int i = 1; i < 11; i++)
                    {
                        sFieldName = "META" + i.ToString() + "_DOUBLE_NAME";
                        sFieldVal = "META" + i.ToString() + "_DOUBLE";
                        if (dtMeta.Rows[0][sFieldName] != DBNull.Value)
                        {
                            sName = Utils.GetStrSafeVal(dtMeta.Rows[0], sFieldName);
                            if (!string.IsNullOrEmpty(sName))
                            {
                                if (dtMeta.Rows[0][sFieldVal] != DBNull.Value && !string.IsNullOrEmpty(dtMeta.Rows[0][sFieldVal].ToString()))
                                {
                                    oMeta.m_oTagMeta = new TagMeta(sName, typeof(double).ToString());
                                    oMeta.m_sValue = Utils.GetStrSafeVal(dtMeta.Rows[0], sFieldVal);
                                    lMetas.Add(oMeta);
                                }
                            }
                        }
                        oMeta = new Metas();
                    }

                    for (int i = 1; i < 11; i++)
                    {
                        sFieldName = "META" + i.ToString() + "_BOOL_NAME";
                        sFieldVal = "META" + i.ToString() + "_BOOL";
                        if (dtMeta.Rows[0][sFieldName] != DBNull.Value)
                        {
                            sName = Utils.GetStrSafeVal(dtMeta.Rows[0], sFieldName);
                            if (!string.IsNullOrEmpty(sName))
                            {
                                if (dtMeta.Rows[0][sFieldVal] != DBNull.Value && !string.IsNullOrEmpty(dtMeta.Rows[0][sFieldVal].ToString()))
                                {
                                    oMeta.m_oTagMeta = new TagMeta(sName, typeof(bool).ToString());
                                    oMeta.m_sValue = Utils.GetStrSafeVal(dtMeta.Rows[0], sFieldVal);
                                    lMetas.Add(oMeta);
                                }
                            }
                        }
                        oMeta = new Metas();
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
        private static List<Picture> GetAllPic(int groupId, int assetId, DataTable dtPic, ref bool result, int assetGroupId)
        {
            result = true;
            List<Picture> lPicObject = new List<Picture>();
            Picture picObj;
            try
            {
                // use old/new image server
                if (WS_Utils.IsGroupIDContainedInConfig(groupId, USE_OLD_IMAGE_SERVER_KEY, ';'))
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
                    List<Ratio> groupRatios = CatalogCache.Instance().GetGroupRatios(assetGroupId);
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
                                    log.DebugFormat("picture size doesn't have a corresponding group ratio configuration. GID: {0}, RatioId: {1}", assetGroupId, pictureSize.RatioId);
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
                                picObj.id = string.Format("{0}_{1}", Path.GetFileNameWithoutExtension(pic.BaseUrl), ratio.Id);

                                // get version: if ratio_id exists in pictures table => get its version
                                picObj.version = pic.Version;

                                // build image URL. 
                                // template: <image_server_url>/p/<partner_id>/entry_id/<image_id>/version/<image_version>/width/<image_width>/height/<image_height>/quality/<image_quality>
                                // Example:  http://localhost/ImageServer/Service.svc/GetImage/p/215/entry_id/123/version/10/width/432/height/230/quality/100
                                picObj.m_sURL = ImageUtils.BuildImageUrl(groupId, picObj.id, picObj.version, pictureSize.Width, pictureSize.Height, 100);

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
                                picObj.id = string.Format("{0}_{1}", Path.GetFileNameWithoutExtension(pic.BaseUrl), pic.RatioId);

                                picObj.version = pic.Version;


                                // build image URL. 
                                // template: <image_server_url>/p/<partner_id>/entry_id/<image_id>/version/<image_version>
                                // Example:  http://localhost/ImageServer/Service.svc/GetImage/p/215/entry_id/123/version/10
                                picObj.m_sURL = ImageUtils.BuildImageUrl(groupId, picObj.id, picObj.version, 0, 0, 100, true);

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
        private static bool GetMediaBasicDetails(ref MediaObj oMediaObj, DataTable dtMedia, DataTable dtUpdateDate, bool bIsMainLang, ref int assetGroupId, ref int isLinear)
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
                        if (!bIsMainLang)
                        {
                            oMediaObj.m_sName = Utils.GetStrSafeVal(dtMedia.Rows[0], "TranslateName");
                            oMediaObj.m_sDescription = Utils.GetStrSafeVal(dtMedia.Rows[0], "TranslateDescription");
                        }
                        else
                        {
                            oMediaObj.m_sName = Utils.GetStrSafeVal(dtMedia.Rows[0], "NAME");
                            oMediaObj.m_sDescription = Utils.GetStrSafeVal(dtMedia.Rows[0], "DESCRIPTION");
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
                            oMediaObj.m_oRatingMedia.m_nRatingAvg = (double)((double)oMediaObj.m_oRatingMedia.m_nRatingSum / (double)oMediaObj.m_oRatingMedia.m_nRatingCount);
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

        /*Call To Searcher to get mediaIds by search Object*/
        public static List<SearchResult> GetMediaIdsFromSearcher(BaseMediaSearchRequest oMediaRequest, ref int nTotalItems, ref bool isLucene)
        {
            List<SearchResult> lSearchResults = null;

            #region SearchMedias

            try
            {
                List<List<string>> jsonizedChannelsDefinitions = null;
                ISearcher searcher = Bootstrapper.GetInstance<ISearcher>();
                ApiObjects.SearchObjects.MediaSearchObj search = null;

                // Group have user types per media  +  siteGuid != empty
                if (!string.IsNullOrEmpty(oMediaRequest.m_sSiteGuid) && IsGroupHaveUserType(oMediaRequest))
                {
                    if (oMediaRequest.m_oFilter == null)
                    {
                        oMediaRequest.m_oFilter = new Filter();
                    }
                    //call ws_users to get userType                  
                    oMediaRequest.m_oFilter.m_nUserTypeID = Utils.GetUserType(oMediaRequest.m_sSiteGuid, oMediaRequest.m_nGroupID);
                }

                if (IsUseIPNOFiltering(oMediaRequest, ref searcher, ref jsonizedChannelsDefinitions))
                {
                    search = BuildSearchObject(oMediaRequest, jsonizedChannelsDefinitions[0], jsonizedChannelsDefinitions[1]);
                }
                else
                {
                    search = BuildSearchObject(oMediaRequest);
                }


                search.m_nPageIndex = oMediaRequest.m_nPageIndex;
                search.m_nPageSize = oMediaRequest.m_nPageSize;


                if (searcher != null)
                {
                    isLucene = searcher is LuceneWrapper;
                    GroupManager groupManager = new GroupManager();
                    CatalogCache catalogCache = CatalogCache.Instance();
                    int nParentGroupID = catalogCache.GetParentGroup(oMediaRequest.m_nGroupID);
                    Group groupInCache = groupManager.GetGroup(nParentGroupID);

                    if (groupInCache != null)
                    {
                        LanguageObj objLang = groupInCache.GetLanguage(oMediaRequest.m_oFilter.m_nLanguage);
                        search.m_oLangauge = objLang;
                    }

                    SearchResultsObj resultObj = searcher.SearchMedias(oMediaRequest.m_nGroupID, search, 0, oMediaRequest.m_oFilter.m_bUseStartDate, oMediaRequest.m_nGroupID);


                    if (resultObj != null)
                    {
                        lSearchResults = resultObj.m_resultIDs;
                        nTotalItems = resultObj.n_TotalItems;
                    }

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
        public static List<UnifiedSearchResult> GetAssetIdFromSearcher(UnifiedSearchRequest request, ref int totalItems, ref int to)
        {
            List<UnifiedSearchResult> searchResultsList = new List<UnifiedSearchResult>();
            totalItems = 0;
            
            // Group have user types per media  +  siteGuid != empty
            if (!string.IsNullOrEmpty(request.m_sSiteGuid) && Utils.IsGroupIDContainedInConfig(request.m_nGroupID, "GroupIDsWithIUserTypeSeperatedBySemiColon", ';'))
            {
                if (request.m_oFilter == null)
                {
                    request.m_oFilter = new Filter();
                }

                //call ws_users to get userType                  
                request.m_oFilter.m_nUserTypeID = Utils.GetUserType(request.m_sSiteGuid, request.m_nGroupID);
            }

            UnifiedSearchDefinitions searchDefinitions = BuildUnifiedSearchObject(request);

            ISearcher searcher = Bootstrapper.GetInstance<ISearcher>();

            if (searcher != null)
            {
                List<UnifiedSearchResult> searchResults = searcher.UnifiedSearch(searchDefinitions, ref totalItems, ref to);

                if (searchResults != null)
                {
                    searchResultsList = searchResults;
                }
            }

            return searchResultsList;
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
            UnifiedSearchDefinitionsBuilder definitionsCache = new UnifiedSearchDefinitionsBuilder();

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

            string userName = string.Empty;
            string password = string.Empty;

            //get username + password from wsCache
            Credentials credentials =
                TvinciCache.WSCredentials.GetWSCredentials(ApiObjects.eWSModules.CATALOG, groupId, ApiObjects.eWSModules.API);

            if (credentials != null)
            {
                userName = credentials.m_sUsername;
                password = credentials.m_sPassword;
            }

            // validate user name and password length
            if (userName.Length == 0 || password.Length == 0)
            {
                throw new Exception(string.Format(
                    "No WS_API login parameters were extracted from DB. userId={0}, groupid={1}",
                    siteGuid, groupId));
            }

            // Initialize web service
            using (API apiWebService = new API())
            {
                // Call webservice method
                var serviceResponse = apiWebService.GetUserParentalRuleTags(userName, password, siteGuid, 0);

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
        }

        /// <summary>
        /// For a given IP, gets all the rules that don't block this specific IP
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        private static List<int> GetGeoBlockRules(int groupId, string ip)
        {
            int countryId = ElasticSearch.Utilities.IpToCountry.GetCountryByIp(ip);

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
        public static HashSet<string> GetUnifiedSearchKey(string originalKey, Group group, out bool isTagOrMeta)
        {
            isTagOrMeta = false;          

            HashSet<string> searchKeys = new HashSet<string>();

            // get alias + regex expression 
            List<FieldTypeEntity> FieldEpgAliasMapping = CatalogDAL.GetAliasMappingFields(group.m_nParentGroupID);

            if (originalKey.StartsWith("tags."))
            {
                foreach (string tag in group.m_oGroupTags.Values)
                {
                    if (tag.Equals(originalKey.Substring(5), StringComparison.OrdinalIgnoreCase))
                    {
                        isTagOrMeta = true;

                        searchKeys.Add(originalKey.ToLower());
                        break;
                    }
                }

                foreach (FieldTypeEntity FieldEpgAlias in FieldEpgAliasMapping.Where(x => x.FieldType == FieldTypes.Tag))
                {
                    if (FieldEpgAlias.Alias.Equals(originalKey.Substring(5), StringComparison.OrdinalIgnoreCase))
                    {
                        isTagOrMeta = true;
                        searchKeys.Add(string.Format("tags.{0}", FieldEpgAlias.Name.ToLower()));
                        break;
                    }
                }

                foreach (var tag in group.m_oEpgGroupSettings.m_lTagsName)
                {
                    if (tag.Equals(originalKey.Substring(5), StringComparison.OrdinalIgnoreCase))
                    {
                        isTagOrMeta = true;
                        searchKeys.Add(originalKey.ToLower());
                        break;
                    }
                }
            }

            else if (originalKey.StartsWith("metas."))
            {
                var metas = group.m_oMetasValuesByGroupId.Select(i => i.Value).Cast<Dictionary<string, string>>().SelectMany(d => d.Values).ToList();

                foreach (var meta in metas)
                {
                    if (meta.Equals(originalKey.Substring(6), StringComparison.OrdinalIgnoreCase))
                    {
                        isTagOrMeta = true;
                        searchKeys.Add(originalKey.ToLower());
                        break;
                    }
                }

                foreach (FieldTypeEntity FieldEpgAlias in FieldEpgAliasMapping.Where(x => x.FieldType == FieldTypes.Meta))
                {
                    if (FieldEpgAlias.Alias.Equals(originalKey.Substring(6), StringComparison.OrdinalIgnoreCase))
                    {
                        isTagOrMeta = true;
                        searchKeys.Add(string.Format("metas.{0}", FieldEpgAlias.Name.ToLower()));
                        break;
                    }
                }

                foreach (var meta in group.m_oEpgGroupSettings.m_lMetasName)
                {
                    if (meta.Equals(originalKey.Substring(6), StringComparison.OrdinalIgnoreCase))
                    {
                        isTagOrMeta = true;
                        searchKeys.Add(originalKey.ToLower());
                        break;
                    }
                }
            }
            else
            {
                foreach (string tag in group.m_oGroupTags.Values)
                {
                    if (tag.Equals(originalKey, StringComparison.OrdinalIgnoreCase))
                    {
                        isTagOrMeta = true;

                        searchKeys.Add(string.Format("tags.{0}", tag.ToLower()));
                        break;
                    }
                }

                var metas = group.m_oMetasValuesByGroupId.Select(i => i.Value).Cast<Dictionary<string, string>>().SelectMany(d => d.Values).ToList();

                foreach (var meta in metas)
                {
                    if (meta.Equals(originalKey, StringComparison.OrdinalIgnoreCase))
                    {
                        isTagOrMeta = true;
                        searchKeys.Add(string.Format("metas.{0}", meta.ToLower()));
                        break;
                    }
                }

                foreach (FieldTypeEntity FieldEpgAlias in FieldEpgAliasMapping.Where(x => x.FieldType == FieldTypes.Tag))
                {
                    if (FieldEpgAlias.Alias.Equals(originalKey, StringComparison.OrdinalIgnoreCase))
                    {
                        isTagOrMeta = true;
                        searchKeys.Add(string.Format("tags.{0}", FieldEpgAlias.Name.ToLower()));
                        break;
                    }
                }

                foreach (FieldTypeEntity FieldEpgAlias in FieldEpgAliasMapping.Where(x => x.FieldType == FieldTypes.Meta))
                {
                    if (FieldEpgAlias.Alias.Equals(originalKey, StringComparison.OrdinalIgnoreCase))
                    {
                        isTagOrMeta = true;
                        searchKeys.Add(string.Format("metas.{0}", FieldEpgAlias.Name.ToLower()));
                        break;
                    }
                }

                foreach (var tag in group.m_oEpgGroupSettings.m_lTagsName)
                {
                    if (tag.Equals(originalKey, StringComparison.OrdinalIgnoreCase))
                    {
                        isTagOrMeta = true;
                        searchKeys.Add(string.Format("tags.{0}", tag.ToLower()));
                        break;
                    }
                }

                foreach (var meta in group.m_oEpgGroupSettings.m_lMetasName)
                {
                    if (meta.Equals(originalKey, StringComparison.OrdinalIgnoreCase))
                    {
                        isTagOrMeta = true;
                        searchKeys.Add(string.Format("metas.{0}", meta.ToLower()));
                        break;
                    }
                }


            }
            if (!isTagOrMeta)
            {
                searchKeys.Add(originalKey.ToLower());
            }

            return searchKeys;
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
            int groupId, out Dictionary<int, int> parentMediaTypes, out Dictionary<int, string> associationTags, List<int> relevantMediaTypes = null,
            bool shouldGetAllMediaTypes = false, GroupManager groupManager = null)
        {
            parentMediaTypes = new Dictionary<int, int>();
            associationTags = new Dictionary<int, string>();

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

        private static bool IsGroupHaveUserType(BaseMediaSearchRequest oMediaRequest)
        {
            try
            {
                return Utils.IsGroupIDContainedInConfig(oMediaRequest.m_nGroupID, "GroupIDsWithIUserTypeSeperatedBySemiColon", ';');
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return false;
            }
        }

        public static bool IsGroupUseFPNPC(int nGroupID) //FPNPC - First Play New Play Cycle
        {
            try
            {
                return Utils.IsGroupIDContainedInConfig(nGroupID, "GroupIDsWithIFPNPC", ';');
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return false;
            }
        }

        #region Build Search object for Searcher project.

        static internal MediaSearchObj BuildSearchObject(BaseMediaSearchRequest request, List<string> jsonizedChannelsDefinitionsToSearchIn,
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
                    FullSearchAddParams(request.m_nGroupID, ((MediaSearchFullRequest)request).m_AndList, ((MediaSearchFullRequest)request).m_OrList, ref m_dAnd, ref m_dOr);
                    searchObj.m_sName = string.Empty;
                    searchObj.m_sDescription = string.Empty;
                }
                else if (request is MediaSearchRequest)
                {
                    // is it normal search
                    NormalSearchAddParams((MediaSearchRequest)request, ref m_dAnd, ref m_dOr);

                    searchObj.m_sName = ((MediaSearchRequest)request).m_sName;
                    searchObj.m_sDescription = ((MediaSearchRequest)request).m_sDescription;

                    if (((MediaSearchRequest)request).m_bAnd)
                        searchObj.m_eCutWith = CutWith.AND;
                    else
                        searchObj.m_eCutWith = CutWith.OR;

                    if (!string.IsNullOrEmpty(((MediaSearchRequest)request).m_sName) || !string.IsNullOrEmpty(((MediaSearchRequest)request).m_sDescription))
                    {
                        SearchObjectString(m_dAnd, m_dOr, ((MediaSearchRequest)request).m_sName, ((MediaSearchRequest)request).m_sDescription, ((MediaSearchRequest)request).m_bAnd);
                    }
                }

                GetOrderValues(ref oSearcherOrderObj, request.m_oOrderObj);
                if (oSearcherOrderObj.m_eOrderBy == ApiObjects.SearchObjects.OrderBy.META && string.IsNullOrEmpty(oSearcherOrderObj.m_sOrderValue))
                {
                    oSearcherOrderObj.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.CREATE_DATE;
                    oSearcherOrderObj.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                }

                #region Build Searcher Search Object
                searchObj.m_bUseStartDate = request.m_oFilter.m_bUseStartDate;
                searchObj.m_bUseFinalEndDate = request.m_oFilter.m_bUseFinalDate;
                searchObj.m_nUserTypeID = request.m_oFilter.m_nUserTypeID;

                searchObj.m_nMediaID = request.m_nMediaID;
                if (request.m_nMediaTypes != null && request.m_nMediaTypes.Count > 0)
                {
                    searchObj.m_sMediaTypes = string.Join(";", request.m_nMediaTypes.Select((i) => i.ToString()).ToArray());
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

                if (request.m_oFilter != null)
                    searchObj.m_nDeviceRuleId = ProtocolsFuncs.GetDeviceAllowedRuleIDs(request.m_oFilter.m_sDeviceId, request.m_nGroupID).ToArray();

                if (m_dOr.Count > 0)
                {
                    searchObj.m_dOr = m_dOr;
                }

                if (m_dAnd.Count > 0)
                {
                    searchObj.m_dAnd = m_dAnd;
                }

                searchObj.m_bExact = request.m_bExact;

                searchObj.m_sPermittedWatchRules = GetPermittedWatchRules(request.m_nGroupID);

                searchObj.m_lChannelsDefinitionsMediaNeedsToBeInAtLeastOneOfIt = jsonizedChannelsDefinitionsToSearchIn;
                searchObj.m_lOrMediaNotInAnyOfTheseChannelsDefinitions = jsonizedChannelsDefinitionsTheMediaShouldNotAppearIn;

                #endregion

                List<int> regionIds;
                List<string> linearMediaTypes;

                Catalog.SetSearchRegions(request.m_nGroupID, request.domainId, request.m_sSiteGuid, out regionIds, out linearMediaTypes);

                searchObj.regionIds = regionIds;
                searchObj.linearChannelMediaTypes = linearMediaTypes;

                Catalog.GetParentMediaTypesAssociations(request.m_nGroupID, out searchObj.parentMediaTypes, out searchObj.associationTags);
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

        internal static MediaSearchObj BuildSearchObject(BaseMediaSearchRequest request, List<string> jsonizedChannelsDefinitionsToSearchIn)
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
        internal static void SetSearchRegions(int groupId, int domainId, string siteGuid, out List<int> regionIds, out List<string> linearMediaTypes)
        {
            regionIds = null;
            linearMediaTypes = null;

            GroupManager groupManager = new GroupManager();
            Group group = groupManager.GetGroup(groupId);

            // If this group has regionalization enabled at all
            if (group.isRegionalizationEnabled)
            {
                regionIds = new List<int>();
                linearMediaTypes = new List<string>();

                // If this is a guest user or something like this - get default region
                if (domainId == 0)
                {
                    int defaultRegion = group.defaultRegion;

                    if (defaultRegion != 0)
                    {
                        regionIds.Add(defaultRegion);
                    }
                }
                // Otherwise get the region of the requesting domain
                else
                {
                    int regionId = GetRegionIdOfDomain(groupId, domainId, siteGuid, group);

                    if (regionId > -1)
                    {
                        regionIds.Add(regionId);
                    }
                }

                // Now we need linear media types - so we filter them and not other media types
                Dictionary<string, string> dictionary = Catalog.GetLinearMediaTypeIDsAndWatchRuleIDs(groupId);

                if (dictionary.ContainsKey(Catalog.LINEAR_MEDIA_TYPES_KEY))
                {
                    // Split by semicolon
                    var mediaTypesArray = dictionary[Catalog.LINEAR_MEDIA_TYPES_KEY].Split(';');

                    // Convert to list
                    linearMediaTypes.AddRange(mediaTypesArray);
                }
            }
        }

        private static int GetRegionIdOfDomain(int groupId, int domainId, string siteGuid, Group group)
        {
            int regionId = -1;

            string userName = string.Empty;
            string password = string.Empty;

            //get username + password from wsCache
            Credentials credentials =
                TvinciCache.WSCredentials.GetWSCredentials(ApiObjects.eWSModules.CATALOG, groupId, ApiObjects.eWSModules.DOMAINS);

            if (credentials != null)
            {
                userName = credentials.m_sUsername;
                password = credentials.m_sPassword;
            }

            if (userName.Length == 0 || password.Length == 0)
            {
                throw new Exception(string.Format(
                    "No WS_Domains login parameters were extracted from DB. userId={0}, groupid={1}",
                    siteGuid, groupId));
            }

            using (WS_Domains.module domainsWebService = new WS_Domains.module())
            {
                Domain domain = null;
                var domainRes = domainsWebService.GetDomainInfo(userName, password, domainId);
                if (domainRes != null)
                {
                    domain = domainRes.Domain;
                }

                // If the domain is not associated to a domain - get default region
                if (domain.m_nRegion == 0)
                {
                    int defaultRegion = group.defaultRegion;

                    if (defaultRegion != 0)
                    {
                        regionId = defaultRegion;
                    }
                }
                else
                {
                    regionId = domain.m_nRegion;
                }
            }

            return regionId;
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
                        searchKey = GetFullSearchKey(andKeyValue.m_sKey, ref group); // returns search key with prefix e.g. metas.{key}

                        SearchValue search = new SearchValue();
                        search.m_sKey = searchKey;
                        search.m_lValue = new List<string> { andKeyValue.m_sValue };
                        search.m_sValue = andKeyValue.m_sValue;
                        resultAnds.Add(search);
                    }
                }

                if (originalOrs != null)
                {
                    foreach (KeyValue orKeyValue in originalOrs)
                    {
                        SearchValue search = new SearchValue();

                        searchKey = GetFullSearchKey(orKeyValue.m_sKey, ref group);// returns search key with prefix e.g. metas.{key}
                        search.m_sKey = searchKey;
                        search.m_lValue = new List<string> { orKeyValue.m_sValue };
                        search.m_sValue = orKeyValue.m_sValue;
                        resultOrs.Add(search);
                    }
                }
            }
        }

        /*Build Normal search object*/
        static internal void NormalSearchAddParams(MediaSearchRequest request, ref List<SearchValue> m_dAnd, ref List<SearchValue> m_dOr)
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
                            search.m_lValue = new List<string>() { metaVar };

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
                    search.m_lValue = new List<string>() { tags.m_sValue };

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
        public static void GetOrderValues(ref ApiObjects.SearchObjects.OrderObj oSearchOrderObj, ApiObjects.SearchObjects.OrderObj oOrderObj)
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
        private static void SearchObjectString(List<SearchValue> m_dAnd, List<SearchValue> m_dOr, string sName, string sDescription, bool bAnd)
        {
            SearchValue searchValue;
            try
            {
                if (!String.IsNullOrEmpty(sName))
                {
                    searchValue = new SearchValue();
                    searchValue.m_sKey = "name";
                    searchValue.m_sValue = sName;
                    searchValue.m_lValue = new List<string>() { sName };

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
                    searchValue.m_lValue = new List<string>() { sDescription };
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
        public static MediaSearchRequest BuildMediasRequest(Int32 nMediaID, bool bIsMainLang, Filter filterRequest, 
            ref Filter oFilter, Int32 nGroupID, List<Int32> nMediaTypes, string sSiteGuid, OrderObj orderObj)
        {
            try
            {
                oFilter = filterRequest;
                MediaSearchRequest oMediasRequest = new MediaSearchRequest();

                int nLanguage = 0;
                if (filterRequest != null)
                {
                    nLanguage = filterRequest.m_nLanguage;
                }

                GroupManager groupManager = new GroupManager();

                CatalogCache catalogCache = CatalogCache.Instance();
                int nParentGroupID = catalogCache.GetParentGroup(nGroupID);

                List<int> lSubGroupTree = groupManager.GetSubGroup(nParentGroupID);
                DataSet ds = CatalogDAL.Build_MediaRelated(nGroupID, nMediaID, nLanguage, lSubGroupTree);

                if (ds == null)
                    return null;
                
                oMediasRequest.m_nGroupID = nGroupID;
                oMediasRequest.m_sSiteGuid = sSiteGuid;
                oMediasRequest.m_oOrderObj = orderObj;

                if (ds.Tables.Count == 4)
                {
                    if (ds.Tables[1] != null) // basic details
                    {
                        oMediasRequest.m_sName = Utils.GetStrSafeVal(ds.Tables[1].Rows[0], "NAME");
                        oMediasRequest.m_nMediaTypes = new List<int>();
                        if (nMediaTypes == null || nMediaTypes.Count == 0)
                        {
                            if (ds.Tables[1].Rows[0]["MEDIA_TYPE_ID"] != DBNull.Value && !string.IsNullOrEmpty(ds.Tables[1].Rows[0]["MEDIA_TYPE_ID"].ToString()))
                                oMediasRequest.m_nMediaTypes.Add(Utils.GetIntSafeVal(ds.Tables[1].Rows[0], "MEDIA_TYPE_ID"));
                        }
                        else
                        {
                            oMediasRequest.m_nMediaTypes = nMediaTypes;
                        }
                        oFilter.m_sDeviceId = filterRequest.m_sDeviceId;
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
                return oMediasRequest;
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
                            if (dtMeta.Rows[0][sFieldVal] != DBNull.Value && !string.IsNullOrEmpty(dtMeta.Rows[0][sFieldVal].ToString()))
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
                            if (dtMeta.Rows[0][sFieldVal] != DBNull.Value && !string.IsNullOrEmpty(dtMeta.Rows[0][sFieldVal].ToString()))
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
                    sFieldVal = "META" + i.ToString() + "BOOL";
                    sFieldRelated = "IS_META" + i.ToString() + "_BOOL_RELATED";
                    if (dtMeta.Rows[0][sFieldName] != DBNull.Value)
                    {
                        sName = dtMeta.Rows[0][sFieldName].ToString();
                        nIsRelated = Utils.GetIntSafeVal(dtMeta.Rows[0], sFieldRelated);
                        if (!string.IsNullOrEmpty(sName) && nIsRelated != 0)
                        {
                            if (dtMeta.Rows[0][sFieldVal] != DBNull.Value && !string.IsNullOrEmpty(dtMeta.Rows[0][sFieldVal].ToString()))
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

        public static void GetMediaPlayData(int nMediaID, int nMediaFileID, ref int nOwnerGroupID, ref int nCDNID, ref int nQualityID, ref int nFormatID, ref int nBillingTypeID, ref int nMediaTypeID)
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

        public static void UpdateFollowMe(int nGroupID, string sAssetID, string sSiteGUID, int nPlayTime, string sUDID, int duration,
            string assetAction, int mediaTypeId,
            int nDomainID = 0, ePlayType ePlayType = ePlayType.MEDIA, bool isFirstPlay = false, bool isLinearChannel = false, long recordingId = 0)
        {
            if (Catalog.IsAnonymousUser(sSiteGUID))
            {
                return;
            }

            if (nDomainID < 1)
            {
                DomainSuspentionStatus eSuspendStat = DomainSuspentionStatus.OK;
                int opID = 0;
                bool isMaster = false;
                nDomainID = DomainDal.GetDomainIDBySiteGuid(nGroupID, int.Parse(sSiteGUID), ref opID, ref isMaster, ref eSuspendStat);
            }

             // take finished percent threshold
	        int finishedPercentThreshold = 0;
	        object dbThresholdVal = ODBCWrapper.Utils.GetTableSingleVal("groups", "FINISHED_PERCENT_THRESHOLD", nGroupID, 86400);
	        if (dbThresholdVal == null ||
	            dbThresholdVal == DBNull.Value ||
	            !int.TryParse(dbThresholdVal.ToString(), out finishedPercentThreshold))
	        {
	            finishedPercentThreshold = Catalog.FINISHED_PERCENT_THRESHOLD;
	        }

            if (nDomainID > 0)
            {
                switch (ePlayType)
                {
                    case ePlayType.MEDIA:
                    CatalogDAL.UpdateOrInsert_UsersMediaMark(nDomainID, int.Parse(sSiteGUID), sUDID, int.Parse(sAssetID), nGroupID, 
                        nPlayTime, duration, assetAction, mediaTypeId, isFirstPlay, isLinearChannel, finishedPercentThreshold);
                        break;
                    case ePlayType.NPVR:
                        CatalogDAL.UpdateOrInsert_UsersNpvrMark(nDomainID, int.Parse(sSiteGUID), sUDID, sAssetID, nGroupID, nPlayTime, duration, assetAction, recordingId, isFirstPlay);
                        break;
                    case ePlayType.EPG:
                        CatalogDAL.UpdateOrInsert_UsersEpgMark(nDomainID, int.Parse(sSiteGUID), sUDID, int.Parse(sAssetID), nGroupID, nPlayTime, duration, assetAction, isFirstPlay);
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
                retActionID = (int)action;

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

        internal static MediaSearchObj BuildBaseChannelSearchObject(GroupsCacheManager.Channel channel, BaseRequest request, OrderObj orderObj, int nParentGroupID, List<string> lPermittedWatchRules, int[] nDeviceRuleId, LanguageObj oLanguage)
        {
            MediaSearchObj searchObject = new MediaSearchObj();
            searchObject.m_nGroupId = channel.m_nGroupID;
            searchObject.m_nPageIndex = request.m_nPageIndex;
            searchObject.m_nPageSize = request.m_nPageSize;
            searchObject.m_bExact = true;
            searchObject.m_eCutWith = channel.m_eCutWith;
            searchObject.m_sMediaTypes = string.Join(";", channel.m_nMediaType);

            if ((lPermittedWatchRules != null) && lPermittedWatchRules.Count > 0)
            {
                searchObject.m_sPermittedWatchRules = string.Join(" ", lPermittedWatchRules);
            }

            searchObject.m_nDeviceRuleId = nDeviceRuleId;
            searchObject.m_nIndexGroupId = nParentGroupID;
            searchObject.m_oLangauge = oLanguage;

            ApiObjects.SearchObjects.OrderObj oSearcherOrderObj = new ApiObjects.SearchObjects.OrderObj();

            if (orderObj != null && orderObj.m_eOrderBy != ApiObjects.SearchObjects.OrderBy.NONE)
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

            Catalog.SetSearchRegions(request.m_nGroupID, request.domainId, request.m_sSiteGuid, out regionIds, out linearMediaTypes);

            searchObject.regionIds = regionIds;
            searchObject.linearChannelMediaTypes = linearMediaTypes;

            Catalog.GetParentMediaTypesAssociations(request.m_nGroupID,
                out searchObject.parentMediaTypes, out searchObject.associationTags);

            return searchObject;
        }

        internal static void AddChannelMultiFiltersToSearchObject(ref MediaSearchObj searchObject, ChannelRequestMultiFiltering request)
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
                    ConvertKeyValuePairsToSearchValues(request.m_lFilterTags, request.m_nGroupID, request.m_eFilterCutWith);
            }
        }

        private static List<SearchValue> ConvertKeyValuePairsToSearchValues(List<KeyValue> keyValues, int groupId, CutWith cutWith)
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
                        bool isTagOrMeta;
                        var searchKeys = GetUnifiedSearchKey(keyValue.m_sKey, group, out isTagOrMeta);

                        switch (cutWith)
                        {
                            case CutWith.OR:
                            {
                                foreach (var currentKey in searchKeys)
                                {
                                    SearchValue search = new SearchValue();
                                    search.m_sKey = currentKey;
                                    search.m_lValue = new List<string> { keyValue.m_sValue };
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
                                    searchKey = searchKeys.First();
                                }

                                search.m_sKey = searchKey;
                                search.m_lValue = new List<string> { keyValue.m_sValue };

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

        private static string GetPermittedWatchRules(int nGroupId, DataTable extractedPermittedWatchRulesDT)
        {
            DataTable permittedWathRulesDt = null;

            if (extractedPermittedWatchRulesDT == null)
            {
                GroupsCacheManager.GroupManager groupManager = new GroupsCacheManager.GroupManager();
                List<int> lSubGroup = groupManager.GetSubGroup(nGroupId);
                permittedWathRulesDt = Tvinci.Core.DAL.CatalogDAL.GetPermittedWatchRulesByGroupId(nGroupId, lSubGroup);
            }
            else
            {
                permittedWathRulesDt = extractedPermittedWatchRulesDT;
            }

            List<string> lWatchRulesIds = null;
            if (permittedWathRulesDt != null && permittedWathRulesDt.Rows.Count > 0)
            {
                lWatchRulesIds = new List<string>();
                foreach (DataRow permittedWatchRuleRow in permittedWathRulesDt.Rows)
                {
                    lWatchRulesIds.Add(Utils.GetStrSafeVal(permittedWatchRuleRow, "RuleID"));
                }
            }

            string sRules = string.Empty;

            if (lWatchRulesIds != null && lWatchRulesIds.Count > 0)
            {
                sRules = string.Join(" ", lWatchRulesIds);
            }

            return sRules;
        }

        internal static string GetPermittedWatchRules(int nGroupId)
        {
            return GetPermittedWatchRules(nGroupId, null);
        }

        private static void CopySearchValuesToSearchObjects(ref MediaSearchObj searchObject, CutWith cutWith, List<SearchValue> channelSearchValues)
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
                            bool isTagOrMeta;
                            var searchKeys = GetUnifiedSearchKey(searchValue.m_sKey, group, out isTagOrMeta);

                            switch (cutWith)
                            {
                                case CutWith.OR:
                                {
                                    foreach (var currentKey in searchKeys)
                                    {
                                        search = new SearchValue();
                                        search.m_sKey = currentKey;
                                        search.m_lValue = searchValue.m_lValue;
                                        search.m_sKeyPrefix = searchValue.m_sKeyPrefix;
                                        search.m_eInnerCutWith = searchValue.m_eInnerCutWith;
                                        m_dOr.Add(search);
                                    }

                                    break;
                                }
                                case CutWith.AND:
                                {
                                    search = new SearchValue();

                                    string searchKey = searchValue.m_sKey;

                                    if (isTagOrMeta)
                                    {
                                        searchKey = searchKeys.First();
                                    }

                                    search.m_sKey = searchKey;
                                    search.m_lValue = searchValue.m_lValue;
                                    search.m_sKeyPrefix = searchValue.m_sKeyPrefix;
                                    search.m_eInnerCutWith = searchValue.m_eInnerCutWith;

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

        public static bool UpdateEpgIndex(List<long> lEpgIds, int nGroupId, eAction eAction)
        {
            return UpdateEpg(lEpgIds, nGroupId, eObjectType.EPG, eAction);
        }

        public static bool UpdateEpgChannelIndex(List<long> ids, int groupId, eAction action)
        {
            return UpdateEpg(ids, groupId, eObjectType.EpgChannel, action);
        }

        public static bool UpdateChannelIndex(List<long> lChannelIds, int nGroupId, eAction eAction)
        {
            return Update(lChannelIds, nGroupId, eObjectType.Channel, eAction);
        }

        private static bool Update(List<int> ids, int groupId, eObjectType updatedObjectType, eAction action)
        {
            var longIds = ids.Select(i => (long)i).ToList();

            return Update(longIds, groupId, updatedObjectType, action);
        }

        private static bool Update(List<long> ids, int groupId, eObjectType updatedObjectType, eAction action)
        {
            bool isUpdateIndexSucceeded = false;

            if (ids != null && ids.Count > 0)
            {
                GroupManager groupManager = new GroupManager();

                CatalogCache catalogCache = CatalogCache.Instance();
                int parentGroupId = catalogCache.GetParentGroup(groupId);

                Group group = groupManager.GetGroup(parentGroupId);

                if (group != null)
                {
                    ApiObjects.CeleryIndexingData data = new CeleryIndexingData(group.m_nParentGroupID,
                        ids, updatedObjectType, action, DateTime.UtcNow);

                    var queue = new CatalogQueue();

                    isUpdateIndexSucceeded = queue.Enqueue(data, string.Format(@"Tasks\{0}\{1}", group.m_nParentGroupID, updatedObjectType.ToString()));

                    // backward compatibility
                    var legacyQueue = new CatalogQueue(true);
                    ApiObjects.MediaIndexingObjects.IndexingData oldData = new ApiObjects.MediaIndexingObjects.IndexingData(ids, group.m_nParentGroupID, updatedObjectType, action);

                    legacyQueue.Enqueue(oldData, string.Format(@"{0}\{1}", group.m_nParentGroupID, updatedObjectType.ToString()));
                }
            }

            return isUpdateIndexSucceeded;
        }

        private static bool UpdateEpg(List<long> ids, int groupId, eObjectType objectType, eAction action)
        {
            bool isUpdateIndexSucceeded = false;

            if (ids != null && ids.Count > 0)
            {
                GroupManager groupManager = new GroupManager();

                CatalogCache catalogCache = CatalogCache.Instance();
                int parentGroupID = catalogCache.GetParentGroup(groupId);

                Group group = groupManager.GetGroup(parentGroupID);

                if (group != null)
                {
                    ApiObjects.CeleryIndexingData data = new CeleryIndexingData(group.m_nParentGroupID,
                        ids, objectType, action, DateTime.UtcNow);

                    var queue = new CatalogQueue();

                    isUpdateIndexSucceeded = queue.Enqueue(data, string.Format(@"Tasks\{0}\{1}", group.m_nParentGroupID, objectType.ToString()));
                    if (isUpdateIndexSucceeded)
                        log.DebugFormat("successfully enqueue epg upload. data: {0}", data);
                    else
                        log.ErrorFormat("Failed enqueue of epg upload. data: {0}", data);

                    // Backward compatibility
                    if (objectType == eObjectType.EPG)
                    {
                        var legacyQueue = new CatalogQueue(true);
                        ApiObjects.MediaIndexingObjects.IndexingData oldData = new ApiObjects.MediaIndexingObjects.IndexingData(ids, group.m_nParentGroupID, objectType, action);

                        legacyQueue.Enqueue(oldData, string.Format(@"{0}\{1}", group.m_nParentGroupID, objectType.ToString()));
                    }
                }
            }

            return isUpdateIndexSucceeded;
        }

        #endregion

        internal static SearchResultsObj GetProgramIdsFromSearcher(EpgSearchObj epgSearchReq)
        {

            ISearcher searcher = Bootstrapper.GetInstance<ISearcher>();

            if (searcher == null)
            {
                throw new Exception(String.Concat("Failed to create Searcher instance. Request is: ", epgSearchReq != null ? epgSearchReq.ToString() : "null"));
            }

            return searcher.SearchEpgs(epgSearchReq);
        }

        internal static void GetGroupsTagsAndMetas(int nGroupID, ref  List<string> lSearchList)
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


        internal static bool CompleteDetailsForProgramResponse(EpgProgramDetailsRequest pRequest, ref EpgProgramResponse pResponse)
        {
            ProgramObj oProgramObj = null;
            List<BaseObject> lProgramObj = new List<BaseObject>();
            int nStartIndex = pRequest.m_nPageIndex * pRequest.m_nPageSize;
            int nEndIndex = pRequest.m_nPageIndex * pRequest.m_nPageSize + pRequest.m_nPageSize;

            if ((nStartIndex == 0 && nEndIndex == 0 && pRequest.m_lProgramsIds != null && pRequest.m_lProgramsIds.Count > 0) ||
                nEndIndex > pRequest.m_lProgramsIds.Count)
            {
                nEndIndex = pRequest.m_lProgramsIds.Count();
            }


            //generate a list with the relevant EPG IDs (according to page size and page index)
            List<int> lEpgIDs = new List<int>();
            for (int i = nStartIndex; i < nEndIndex; i++)
            {
                lEpgIDs.Add(pRequest.m_lProgramsIds[i]);
            }

            LanguageObj lang = null;
            string langCode = null;
            if (pRequest.m_oFilter != null)
            {
                lang = GetLanguage(pRequest.m_nGroupID, pRequest.m_oFilter.m_nLanguage);
            }

            if (lang != null && !lang.IsDefault)
            {
                langCode = lang.Code;
            }

            BaseEpgBL epgBL = EpgBL.Utils.GetInstance(pRequest.m_nGroupID);
            List<EPGChannelProgrammeObject> programs = epgBL.GetEpgCBsWithLanguage(lEpgIDs.Select(e => (ulong)e).ToList(), langCode);
            
            // get all linear settings about channel + group
            GetLinearChannelSettings(pRequest.m_nGroupID, programs);


            EPGChannelProgrammeObject epgProg = null;

            //keeping the original order and amount of items (some of the items might return as null)
            if (pRequest.m_lProgramsIds != null && programs != null)
            {
                pResponse.m_nTotalItems = programs.Count;               
            }

            foreach (int nProgram in pRequest.m_lProgramsIds)
            {
                if (programs.Exists(x => x.EPG_ID == nProgram))
                {
                    epgProg = programs.Find(x => x.EPG_ID == nProgram);
                    oProgramObj = new ProgramObj();
                    oProgramObj.m_oProgram = epgProg;
                    oProgramObj.AssetId = epgProg.EPG_ID.ToString();

                    bool succeedParse = DateTime.TryParse(epgProg.UPDATE_DATE, out oProgramObj.m_dUpdateDate);
                    lProgramObj.Add(oProgramObj);
                }

            }
            pResponse.m_lObj = lProgramObj;

            return true;
        }

        public static void GetLinearChannelSettings(int groupID, List<EPGChannelProgrammeObject> lEpgProg)
        {
            try
            {
                List<string> epgChannelIds = lEpgProg.Where(item => item != null && !string.IsNullOrEmpty(item.EPG_CHANNEL_ID)).Select(item => item.EPG_CHANNEL_ID).ToList<string>();// get all epg channel ids

                Dictionary<string, LinearChannelSettings> linearChannelSettings = CatalogCache.Instance().GetLinearChannelSettings(groupID, epgChannelIds);
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
                log.Error(string.Format("failed Catalog.GetLinearChannelSettings for groupId: {0}, lEpgProg: {1}", groupID, lEpgProg != null ? string.Join(", ", lEpgProg.Select(x => x.EPG_ID).ToList()) : string.Empty), ex);
            }
        }

        private static void SetLinearEpgProgramSettings(EPGChannelProgrammeObject epg, LinearChannelSettings linearSettings)
        {
            // 0 ==> need to get value from LinearChannelSettings
            // 1 ==> check the value at LinearChannelSettings : if true - OK else false
            // 2 ==> false - do nothing
            if (epg.ENABLE_CDVR != 2)// get value from epg channel 
            {
                epg.ENABLE_CDVR = linearSettings.EnableCDVR == true ? 1 : 0;
            }

            if (epg.ENABLE_START_OVER != 2)// get value from epg channel
            {
                epg.ENABLE_START_OVER = linearSettings.EnableStartOver == true ? 1 : 0;
            }
            if (epg.ENABLE_CATCH_UP != 2)// get value from epg channel
            {
                epg.ENABLE_CATCH_UP = linearSettings.EnableCatchUp == true ? 1 : 0;
            }

            epg.CHANNEL_CATCH_UP_BUFFER = linearSettings.CatchUpBuffer;

            if (epg.ENABLE_TRICK_PLAY != 2)// get value from epg channel
            {
                epg.ENABLE_TRICK_PLAY = linearSettings.EnableTrickPlay == true ? 1 : 0;
            }
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
                    if (isProgram) //only if we found basic details for media - media in status = 1 , and active if necessary
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

        internal static List<long> GetEpgChannelIDsForIPNOFiltering(int groupID, ref ISearcher initializedSearcher,
            int domainId, string siteGuid,
            ref List<List<string>> jsonizedChannelsDefinitions)
        {
            List<long> res = new List<long>();
            Dictionary<string, string> dict = GetLinearMediaTypeIDsAndWatchRuleIDs(groupID);
            MediaSearchObj linearChannelMediaIDsRequest = BuildLinearChannelsMediaIDsRequest(groupID,
                domainId, siteGuid,
                dict, jsonizedChannelsDefinitions);
            SearchResultsObj searcherAnswer = initializedSearcher.SearchMedias(groupID, linearChannelMediaIDsRequest, 0, true, groupID);

            if (searcherAnswer.n_TotalItems > 0)
            {
                List<long> ipnoEPGChannelsMediaIDs = ExtractMediaIDs(searcherAnswer);
                res.AddRange(GetEPGChannelsIDs(ipnoEPGChannelsMediaIDs));
            }

            return res;
        }

        internal static List<string> EpgAutoComplete(EpgSearchObj request)
        {

            ISearcher searcher = Bootstrapper.GetInstance<ISearcher>();

            if (searcher == null || request == null)
            {
                throw new Exception("EpgAutoComplete. Either EpgSearchObj or Searcher instance is null.");
            }

            return searcher.GetEpgAutoCompleteList(request);

        }

        /*
         * We set isSortResults to true when we are unable to bring the results sorted from CB. In this case the code will verify that the
         * results are sorted.
         * When we are able to bring the results from CV sorted, we set this flag to false.
         * 
         */
        private static ICollection<EPGChannelProgrammeObject> GetEPGProgramsCollectionFactory(bool isSortResults)
        {
            if (isSortResults)
                return new SortedSet<EPGChannelProgrammeObject>(new EPGChannelProgrammeObject.EPGChannelProgrammeObjectStartDateComparer());
            return new List<EPGChannelProgrammeObject>();
        }

        internal static EpgResponse GetEPGProgramsFromCB(List<int> epgIDs, int parentGroupID, bool isSortResults, List<int> epgChannelIDs, int languageId)
        {
            EpgResponse res = new EpgResponse();
            List<EPGChannelProgrammeObject> epgs = GetEpgsByGroupIdLanguageIdAndEpgIds(parentGroupID, epgIDs, languageId);
            if (epgs != null && epgs.Count > 0)
            {
                int totalItems = 0;
                // get all linear settings about channel + group
                GetLinearChannelSettings(parentGroupID, epgs);

                Dictionary<int, ICollection<EPGChannelProgrammeObject>> channelIdsToProgrammesMapping = new Dictionary<int, ICollection<EPGChannelProgrammeObject>>(epgChannelIDs.Count);

                for (int i = 0; i < epgs.Count; i++)
                {
                    int tempEpgChannelID = 0;
                    if (epgs[i] == null || !Int32.TryParse(epgs[i].EPG_CHANNEL_ID, out tempEpgChannelID) || tempEpgChannelID < 1)
                    {
                        continue;
                    }
                   
                    if (channelIdsToProgrammesMapping.ContainsKey(tempEpgChannelID))
                    {
                        channelIdsToProgrammesMapping[tempEpgChannelID].Add(epgs[i]);
                    }
                    else
                    {
                        channelIdsToProgrammesMapping.Add(tempEpgChannelID, GetEPGProgramsCollectionFactory(isSortResults));
                        channelIdsToProgrammesMapping[tempEpgChannelID].Add(epgs[i]);
                    }

                } // for

                // build response in the same order we rcvd the epg channel ids.
                for (int i = 0; i < epgChannelIDs.Count; i++)
                {
                    if (channelIdsToProgrammesMapping.ContainsKey(epgChannelIDs[i]))
                    {
                        res.programsPerChannel.Add(BuildResObjForChannel(channelIdsToProgrammesMapping[epgChannelIDs[i]], epgChannelIDs[i], ref totalItems));
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
            List<ProgramObj> epgsInformation = new List<ProgramObj>();

            // Don't do anything if no valid input
            if (epgIds == null || epgIds.Count == 0)
            {
                return epgsInformation;
            }

            List<EPGChannelProgrammeObject> basicEpgObjects = null;

            bool shouldGetDefault = true;

            LanguageObj language = null;
            if (filter != null)
            {
                Group group = GroupsCache.Instance().GetGroup(groupId);
                language = group.GetLanguage(filter.m_nLanguage);

                if (language != null && !language.IsDefault)
                {
                    shouldGetDefault = false;
                }
            }

            if (shouldGetDefault)
            {
                basicEpgObjects = GetEpgsByGroupAndIDs(groupId, epgIds.Select(id => (int)id).ToList());
            }
            else
            {
                BaseEpgBL epgBL = EpgBL.Utils.GetInstance(groupId);
                basicEpgObjects = epgBL.GetEpgCBsWithLanguage(epgIds.Select(id => (ulong)id).ToList(), language.Code);
            }

            if (basicEpgObjects != null && basicEpgObjects.Count > 0)
            {
                // get all linear settings about channel + group
                GetLinearChannelSettings(groupId, basicEpgObjects);

                for (int i = 0; i < basicEpgObjects.Count; i++)
                {
                    var currentEpg = basicEpgObjects[i];

                    int tempEpgChannelID = 0;

                    if (currentEpg == null || !Int32.TryParse(currentEpg.EPG_CHANNEL_ID, out tempEpgChannelID) || tempEpgChannelID < 1)
                    {
                        continue;
                    }
                    DateTime updateDate = DateTime.MinValue;
                    DateTime.TryParse(currentEpg.UPDATE_DATE, out updateDate);

                    epgsInformation.Add(new ProgramObj()
                    {
                        m_oProgram = currentEpg,
                        AssetId = currentEpg.EPG_ID.ToString(),
                        m_dUpdateDate = updateDate
                    }
                    );
                }
            }

            return epgsInformation;
        }

        private static EpgResultsObj BuildResObjForChannel(ICollection<EPGChannelProgrammeObject> programmes, int epgChannelID, ref int totalItems)
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
                        oProgram.PIC_URL = oProgram.PIC_URL.Replace(".", string.Format("_{0}X{1}.", epgPicWidth, epgPicHeight));
                    }
                    oProgram.PIC_URL = string.Format("{0}{1}", baseEpgPicUrl, oProgram.PIC_URL);
                }
            }
        }

        private static void GetEpgPicUrlData(List<EPGChannelProgrammeObject> epgList, Dictionary<int, List<string>> groupTreeEpgPicUrl,
            ref string epgPicBaseUrl, ref string epgPicWidth, ref string epgPicHeight)
        {
            int groupID = 0;
            if (epgList != null && epgList.Count > 0 && epgList[0] != null && Int32.TryParse(epgList[0].GROUP_ID, out groupID)
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

        private static string GetAssetStatsResultsLogMsg(string sMsg, int nGroupID, List<int> lAssetIDs, DateTime dStartDate, DateTime dEndDate, StatsType eType)
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

        private static void GetDataForGetAssetStatsFromES(int parentGroupID, List<int> assetIDs, DateTime startDate,
            DateTime endDate, StatsType type, Dictionary<int, AssetStatsResult> assetIDsToStatsMapping)
        {
            string index = ElasticSearch.Common.Utils.GetGroupStatisticsIndex(parentGroupID);
            ElasticSearch.Common.ElasticSearchApi esApi = new ElasticSearch.Common.ElasticSearchApi();


            switch (type)
            {
                case StatsType.MEDIA:
                    {
                        List<string> aggregations = new List<string>(3);
                        aggregations.Add(BuildSlidingWindowCountAggregationRequest(parentGroupID, assetIDs, startDate, endDate, STAT_ACTION_FIRST_PLAY)); // views count
                        aggregations.Add(BuildSlidingWindowCountAggregationRequest(parentGroupID, assetIDs, startDate, endDate, STAT_ACTION_LIKE));
                        aggregations.Add(BuildSlidingWindowStatisticsAggregationRequest(parentGroupID, assetIDs, startDate, endDate, STAT_ACTION_RATES, STAT_ACTION_RATE_VALUE_FIELD));

                        string esResp = esApi.MultiSearch(index, ElasticSearch.Common.Utils.ES_STATS_TYPE, aggregations, null);

                        List<string> responses = ParseResponsesFromMultiAggregations(esResp);
                        string currResp = responses[0];
                        Dictionary<string, Dictionary<int, int>> viewsRaw = ESAggregationsResult.DeserializeAggrgations<int>(currResp);
                        currResp = responses[1];
                        Dictionary<string, Dictionary<int, int>> likesRaw = ESAggregationsResult.DeserializeAggrgations<int>(currResp);
                        currResp = responses[2];
                        Dictionary<string, List<StatisticsAggregationResult>> ratesRaw = ESAggregationsResult.DeserializeStatisticsAggregations(currResp, "sub_stats");

                        Dictionary<int, int> views = null, likes = null;
                        List<StatisticsAggregationResult> rates = null;
                        viewsRaw.TryGetValue(STAT_SLIDING_WINDOW_AGGREGATION_NAME, out views);
                        likesRaw.TryGetValue(STAT_SLIDING_WINDOW_AGGREGATION_NAME, out likes);
                        ratesRaw.TryGetValue(STAT_SLIDING_WINDOW_AGGREGATION_NAME, out rates);

                        InjectResultsIntoAssetStatsResponse(assetIDsToStatsMapping, views != null ? views : new Dictionary<int, int>(0),
                            likes != null ? likes : new Dictionary<int, int>(0),
                            rates != null ? rates : new List<StatisticsAggregationResult>(0));
                        break;
                    }
                case StatsType.EPG:
                    {
                        // in epg we bring just likes
                        string likesAggregations = BuildSlidingWindowCountAggregationRequest(parentGroupID, assetIDs, startDate, endDate, STAT_ACTION_LIKE);
                        string searchResponse = esApi.Search(index, ElasticSearch.Common.Utils.ES_STATS_TYPE, ref likesAggregations);

                        if (!string.IsNullOrEmpty(searchResponse))
                        {
                            Dictionary<string, Dictionary<int, int>> likesRaw = ESAggregationsResult.DeserializeAggrgations<int>(searchResponse);
                            Dictionary<int, int> likes = null;
                            likesRaw.TryGetValue(STAT_SLIDING_WINDOW_AGGREGATION_NAME, out likes);

                            if (likes != null && likes.Count > 0)
                            {
                                InjectResultsIntoAssetStatsResponse(assetIDsToStatsMapping, new Dictionary<int, int>(0), likes,
                                    new List<StatisticsAggregationResult>(0));
                            }
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

        }

        private static void InjectResultsIntoAssetStatsResponse(Dictionary<int, AssetStatsResult> assetIDsToStatsMapping,
            Dictionary<int, int> views, Dictionary<int, int> likes,
            List<StatisticsAggregationResult> rates)
        {

            // views and likes

            foreach (KeyValuePair<int, AssetStatsResult> kvp in assetIDsToStatsMapping)
            {
                if (views.ContainsKey(kvp.Key))
                {
                    kvp.Value.m_nViews = views[kvp.Key];
                }
                if (likes.ContainsKey(kvp.Key))
                {
                    kvp.Value.m_nLikes = likes[kvp.Key];
                }
            }

            // rates
            for (int i = 0; i < rates.Count; i++)
            {
                int assetId = 0;

                if (Int32.TryParse(rates[i].key, out assetId) && assetId > 0 && assetIDsToStatsMapping.ContainsKey(assetId))
                {
                    assetIDsToStatsMapping[assetId].m_nVotes = rates[i].count;
                    assetIDsToStatsMapping[assetId].m_dRate = rates[i].avg;
                }

            }
        }

        private static List<string> ParseResponsesFromMultiAggregations(string esResp)
        {
            List<string> res = new List<string>();
            if (!string.IsNullOrEmpty(esResp))
            {
                JObject jObj = JObject.Parse(esResp);
                JToken responses = jObj["responses"];
                if (responses != null && responses.Count() > 0)
                {
                    foreach (var response in responses)
                    {
                        res.Add(response.ToString());
                    }
                }


            }

            return res;
        }

        private static List<EPGChannelProgrammeObject> GetEpgsByGroupAndIDs(int groupID, List<int> epgIDs)
        {
            BaseEpgBL epgBL = EpgBL.Utils.GetInstance(groupID);
            return epgBL.GetEpgs(epgIDs);
        }

        private static List<EPGChannelProgrammeObject> GetEpgsByGroupIdLanguageIdAndEpgIds(int groupID, List<int> epgIDs, int languageId)
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
                return epgBL.GetEpgCBsWithLanguage(epgIDs.Select(x => (ulong)x).ToList(), langCode);
            }
        }

        internal static List<AssetStatsResult> GetAssetStatsResults(int nGroupID, List<int> lAssetIDs, DateTime dStartDate, DateTime dEndDate, StatsType eType)
        {
            // Data structures here are used for returning List<AssetStatsResult> in the same order asset ids are given in lAssetIDs
            SortedSet<AssetStatsResult.IndexedAssetStatsResult> set = null;
            Dictionary<int, AssetStatsResult> assetIdToAssetStatsMapping = null;
            InitializeAssetStatsResultsDataStructs(lAssetIDs, ref set, ref assetIdToAssetStatsMapping);

            switch (eType)
            {
                case StatsType.MEDIA:
                    {
                        BaseStaticticsBL staticticsBL = StatisticsBL.Utils.GetInstance(nGroupID);
                        Dictionary<string, BuzzWeightedAverScore> buzzDict = null;
                        if (TvinciCache.GroupsFeatures.GetGroupFeatureStatus(nGroupID, GroupFeature.BUZZFEED))
                        {
                            buzzDict = staticticsBL.GetBuzzAverScore(lAssetIDs);
                        }

                        bool isBuzzNotEmpty = buzzDict != null && buzzDict.Count > 0;

                        if (IsBringAllStatsRegardlessDates(dStartDate, dEndDate))
                        {
                            /*
                             * When dates are fictive, we get all data (Views, VotesCount, VotesSum, Likes) from media table in SQL DB.
                             */
                            Dictionary<int, int[]> dict = CatalogDAL.Get_MediaStatistics(null, null, nGroupID, lAssetIDs);

                            if (dict.Count > 0)
                            {
                                foreach (KeyValuePair<int, int[]> kvp in dict)
                                {
                                    if (assetIdToAssetStatsMapping.ContainsKey(kvp.Key))
                                    {
                                        int votesCount = kvp.Value[ASSET_STATS_VOTES_INDEX];
                                        assetIdToAssetStatsMapping[kvp.Key].m_nViews = kvp.Value[ASSET_STATS_VIEWS_INDEX];
                                        assetIdToAssetStatsMapping[kvp.Key].m_nVotes = votesCount;
                                        assetIdToAssetStatsMapping[kvp.Key].m_nLikes = kvp.Value[ASSET_STATS_LIKES_INDEX];
                                        if (votesCount > 0)
                                        {
                                            assetIdToAssetStatsMapping[kvp.Key].m_dRate = ((double)kvp.Value[ASSET_STATS_VOTES_SUM_INDEX]) / votesCount;
                                        }
                                        if (isBuzzNotEmpty)
                                        {
                                            string strAssetID = kvp.Key.ToString();
                                            if (buzzDict.ContainsKey(strAssetID) && buzzDict[strAssetID] != null)
                                            {
                                                assetIdToAssetStatsMapping[kvp.Key].m_buzzAverScore = buzzDict[strAssetID];
                                            }
                                            else
                                            {
                                                log.Error("Error - " + GetAssetStatsResultsLogMsg(String.Concat("Buzz Meter for media id: ", strAssetID, " does not exist. "), nGroupID, lAssetIDs, dStartDate, dEndDate, eType));
                                            }
                                        }
                                    }
                                } // foreach
                            } // end if dict is not empty
                            else
                            {
                                // log here no data retrieved from media table.
                                log.Error("Error - " + GetAssetStatsResultsLogMsg("No data retrieved from media table.", nGroupID, lAssetIDs, dStartDate, dEndDate, eType));
                            }
                        }
                        else
                        {
                            /*
                             * When we have valid dates in Media Asset Stats request we fetch the data as follows:
                             * 1. Views Rating and Likes from ES statistics index.
                             * 
                             *  Only for groups that are not contained in GROUPS_USING_DB_FOR_ASSETS_STATS
                             */

                            if (Utils.IsGroupIDContainedInConfig(nGroupID, "GROUPS_USING_DB_FOR_ASSETS_STATS", ';'))
                            {
                                #region Old Get MediaStatistics code - goes to DB for views and to CB for likes\rate\votes

                                Dictionary<int, int[]> dict = CatalogDAL.Get_MediaStatistics(dStartDate, dEndDate, nGroupID, lAssetIDs);
                                if (dict.Count > 0)
                                {
                                    foreach (KeyValuePair<int, int[]> kvp in dict)
                                    {
                                        if (assetIdToAssetStatsMapping.ContainsKey(kvp.Key))
                                        {
                                            assetIdToAssetStatsMapping[kvp.Key].m_nViews = kvp.Value[ASSET_STATS_VIEWS_INDEX];
                                            if (isBuzzNotEmpty)
                                            {
                                                string strAssetID = kvp.Key.ToString();
                                                if (buzzDict.ContainsKey(strAssetID) && buzzDict[strAssetID] != null)
                                                {
                                                    assetIdToAssetStatsMapping[kvp.Key].m_buzzAverScore = buzzDict[strAssetID];
                                                }
                                                else
                                                {
                                                    log.Error("Error - " + GetAssetStatsResultsLogMsg(String.Concat("No buzz meter found for media id: ", kvp.Key), nGroupID, lAssetIDs, dStartDate, dEndDate, eType));
                                                }
                                            }
                                        }
                                    } // foreach
                                }
                                else
                                {
                                    log.Error("Error - " + GetAssetStatsResultsLogMsg("No media views retrieved from DB. ", nGroupID, lAssetIDs, dStartDate, dEndDate, eType));
                                }

                                // save monitor and logs context data
                                ContextData contextData = new ContextData();

                                // bring social actions from CB social bucket
                                Task<AssetStatsResult.SocialPartialAssetStatsResult>[] tasks = new Task<AssetStatsResult.SocialPartialAssetStatsResult>[lAssetIDs.Count];
                                for (int i = 0; i < lAssetIDs.Count; i++)
                                {
                                    tasks[i] = Task.Factory.StartNew<AssetStatsResult.SocialPartialAssetStatsResult>((item) =>
                                    {
                                        // load monitor and logs context data
                                        contextData.Load();

                                        return GetSocialAssetStats(nGroupID, (int)item, eType, dStartDate, dEndDate);
                                    }
                                        , lAssetIDs[i]);
                                }
                                Task.WaitAll(tasks);
                                for (int i = 0; i < tasks.Length; i++)
                                {
                                    if (tasks[i] != null)
                                    {
                                        AssetStatsResult.SocialPartialAssetStatsResult socialData = tasks[i].Result;
                                        if (socialData != null && assetIdToAssetStatsMapping.ContainsKey(socialData.assetId))
                                        {
                                            assetIdToAssetStatsMapping[socialData.assetId].m_nLikes = socialData.likesCounter;
                                            assetIdToAssetStatsMapping[socialData.assetId].m_dRate = socialData.rate;
                                            assetIdToAssetStatsMapping[socialData.assetId].m_nVotes = socialData.votes;
                                        }
                                    }
                                    tasks[i].Dispose();
                                }
                                #endregion
                            }
                            else
                            {
                                /************* For versions after Joker that don't want to use DB for getting stats, we fetch the data from ES statistics index **********/
                                GetDataForGetAssetStatsFromES(nGroupID, lAssetIDs, dStartDate, dEndDate, StatsType.MEDIA, assetIdToAssetStatsMapping);
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
                            List<EPGChannelProgrammeObject> lEpg = GetEpgsByGroupAndIDs(nGroupID, lAssetIDs);
                            if (lEpg != null && lEpg.Count > 0)
                            {
                                for (int i = 0; i < lEpg.Count; i++)
                                {
                                    if (lEpg[i] != null)
                                    {
                                        int currEpgId = (int)lEpg[i].EPG_ID;
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
                                log.Error("Error - " + GetAssetStatsResultsLogMsg("No EPGs retrieved from epg_channels_schedule CB bucket", nGroupID, lAssetIDs, dStartDate, dEndDate, eType));
                            }

                        }
                        else
                        {
                            // we bring data from ES statistics index only for groups that are not contained in GROUPS_USING_DB_FOR_ASSETS_STATS
                            if (Utils.IsGroupIDContainedInConfig(nGroupID, "GROUPS_USING_DB_FOR_ASSETS_STATS", ';'))
                            {
                                #region Old Get MediaStatistics code - goes to DB for views and to CB for likes\rate\votes

                                // save monitor and logs context data
                                ContextData contextData = new ContextData();

                                // we bring data from social bucket in CB.
                                Task<AssetStatsResult.SocialPartialAssetStatsResult>[] tasks = new Task<AssetStatsResult.SocialPartialAssetStatsResult>[lAssetIDs.Count];
                                for (int i = 0; i < lAssetIDs.Count; i++)
                                {
                                    tasks[i] = Task.Factory.StartNew<AssetStatsResult.SocialPartialAssetStatsResult>((item) =>
                                    {
                                        // load monitor and logs context data
                                        contextData.Load();

                                        return GetSocialAssetStats(nGroupID, (int)item, eType, dStartDate, dEndDate);
                                    }
                                        , lAssetIDs[i]);
                                }
                                Task.WaitAll(tasks);
                                for (int i = 0; i < tasks.Length; i++)
                                {
                                    if (tasks[i] != null)
                                    {
                                        AssetStatsResult.SocialPartialAssetStatsResult socialData = tasks[i].Result;
                                        if (socialData != null && assetIdToAssetStatsMapping.ContainsKey(socialData.assetId))
                                        {
                                            assetIdToAssetStatsMapping[socialData.assetId].m_nLikes = socialData.likesCounter;
                                            assetIdToAssetStatsMapping[socialData.assetId].m_dRate = socialData.rate;
                                            assetIdToAssetStatsMapping[socialData.assetId].m_nVotes = socialData.votes;
                                        }
                                    }
                                    tasks[i].Dispose();
                                }
                                #endregion
                            }
                            else
                            {
                                /************* For versions after Joker that don't want to use DB for getting stats, we fetch the data from ES statistics index **********/
                                GetDataForGetAssetStatsFromES(nGroupID, lAssetIDs, dStartDate, dEndDate, StatsType.EPG, assetIdToAssetStatsMapping);
                            }
                        }

                        break;
                    }
                default:
                    {
                        throw new NotImplementedException(String.Concat("Unsupported stats type: ", eType.ToString()));
                    }

            } // switch

            return set.Select((item) => (item.Result)).ToList<AssetStatsResult>();
        }

        private static AssetStatsResult.SocialPartialAssetStatsResult GetSocialAssetStats(int groupId, int assetId, StatsType statsTypes,
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
                        res.likesCounter = socialDal.GetAssetSocialActionCount(assetId, eAssetType.MEDIA, eUserAction.LIKE, startDate, endDate);
                        res.votes = socialDal.GetAssetSocialActionCount(assetId, eAssetType.MEDIA, eUserAction.RATES, startDate, endDate);
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
                        res.likesCounter = socialDal.GetAssetSocialActionCount(assetId, eAssetType.PROGRAM, eUserAction.LIKE, startDate, endDate);
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

        internal static bool IsUseIPNOFiltering(BaseRequest oMediaRequest,
            ref ISearcher initializedSearcher, ref List<List<string>> outJsonizedChannelsDefinitions)
        {
            int operatorID = 0;
            return IsUseIPNOFiltering(oMediaRequest, ref initializedSearcher, ref outJsonizedChannelsDefinitions, ref operatorID);
        }


        internal static bool IsUseIPNOFiltering(BaseRequest oMediaRequest,
            ref ISearcher initializedSearcher, ref List<List<string>> outJsonizedChannelsDefinitions, ref int operatorID)
        {

            bool res = false;
            long lSiteGuid = 0;
            if (Utils.IsGroupIDContainedInConfig(oMediaRequest.m_nGroupID, "GroupIDsWithIPNOFilteringSeperatedBySemiColon", ';'))
            {
                /*
                 * 1. We need to filter results by IPNO.
                 * 2. IPNO is a container of subscriptions.
                 * 3. Subscription is a container of (Tvinci) channels
                 * 4. Hence, we query the ES's percolator for (Tvinci) channels definitions
                 * 5. We query the ES using IDs query. The (Tvinci) channels ids we store in cache.
                 * 6. Then we concat the channels definitions with an AND to the search query.
                 * 7. The IPNO ID we extract from database using the user's site guid.
                 */
                if (!Int64.TryParse(oMediaRequest.m_sSiteGuid, out lSiteGuid) || lSiteGuid == 0)
                {
                    // anonymous user
                    if (Utils.IsGroupIDContainedInConfig(oMediaRequest.m_nGroupID, "GroupIDsWithIPNOFilteringShowAllCatalogAnonymousUser", ';'))
                    {
                        //user is able to watch the entire catalog
                        res = false;
                    }
                    else
                    {
                        throw new Exception("IPNO Filtering. No site guid at Catalog Request");
                    }
                }
                else
                {
                    // site guid exists. let's fetch operator id from DB.
                    operatorID = Utils.GetOperatorIDBySiteGuid(oMediaRequest.m_nGroupID, lSiteGuid);
                    if (operatorID == 0)
                    {
                        throw new Exception("IPNO Filtering. No operator ID extracted from DB");
                    }
                    else
                    {
                        // we have operator id
                        res = true;
                        GroupManager groupManager = new GroupManager();

                        CatalogCache catalogCache = CatalogCache.Instance();
                        int nParentGroupID = catalogCache.GetParentGroup(oMediaRequest.m_nGroupID);



                        List<long> channelsOfIPNO = groupManager.GetOperatorChannelIDs(nParentGroupID, operatorID);
                        List<long> allChannelsOfAllIPNOs = groupManager.GetDistinctAllOperatorsChannels(nParentGroupID);

                        if (channelsOfIPNO != null && channelsOfIPNO.Count > 0 && allChannelsOfAllIPNOs != null && allChannelsOfAllIPNOs.Count > 0)
                        {
                            // get channels definitions from ES Percolator

                            outJsonizedChannelsDefinitions = initializedSearcher.GetChannelsDefinitions(new List<List<long>>(2) { channelsOfIPNO, allChannelsOfAllIPNOs }, oMediaRequest.m_nGroupID);
                        }
                        else
                        {
                            throw new Exception("IPNO Filtering. No cached channels");
                        }
                    }
                }

            } // end big if

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

            Catalog.SetSearchRegions(nGroupID, domainId, siteGuid, out regionIds, out linearMediaTypes);

            res.regionIds = regionIds;
            res.linearChannelMediaTypes = linearMediaTypes;

            Catalog.GetParentMediaTypesAssociations(nGroupID, out res.parentMediaTypes, out res.associationTags);

            return res;
        }

        private static int GetSearcherMaxResultsSize()
        {
            int res = 0;
            string maxResultsStr = Utils.GetWSURL("MAX_RESULTS");
            if (!string.IsNullOrEmpty(maxResultsStr) && Int32.TryParse(maxResultsStr, out res) && res > 0)
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
                    sb.Append(String.Concat(i == 0 ? string.Empty : ";", ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["ID"])));
                }
                res = sb.ToString();
            }

            return res;
        }

        private static string GetFullSearchKey(string originalKey, ref Group group)
        {
            bool isTagOrMeta;
            return Catalog.GetFullSearchKey(originalKey, ref group, out isTagOrMeta);
        }


        private static string GetFullSearchKey(string originalKey, ref Group group, out bool isTagOrMeta)
        {
            isTagOrMeta = false;

            string searchKey = originalKey;

            foreach (string tag in group.m_oGroupTags.Values)
            {
                if (tag.Equals(originalKey, StringComparison.OrdinalIgnoreCase))
                {
                    searchKey = string.Concat(TAGS, ".", tag.ToLower());
                    isTagOrMeta = true;
                    break;
                }
            }

            if (!isTagOrMeta)
            {
                var metas = group.m_oMetasValuesByGroupId.Select(i => i.Value).Cast<Dictionary<string, string>>().SelectMany(d => d.Values).ToList();

                foreach (var meta in metas)
                {
                    if (meta.Equals(originalKey, StringComparison.OrdinalIgnoreCase))
                    {
                        searchKey = string.Concat(METAS, ".", meta.ToLower());
                        isTagOrMeta = true;
                        break;
                    }
                }

            }

            return searchKey;
        }

        public static int GetLastPosition(int mediaID, int userID)
        {

            if (mediaID == 0 || userID == 0)
                return 0;

            return CatalogDAL.GetLastPosition(mediaID, userID);
        }

        internal static bool IsConcurrent(string siteGuid, string udid, int groupID, ref int domainID, 
            int mediaID, int mediaFileID, int platform, int countryID, PlayCycleSession playCycleSession, ePlayType playType = ePlayType.MEDIA)
        {
            bool result = true;
            long siteGuidLong = 0;

            if (!Int64.TryParse(siteGuid, out siteGuidLong))
            {
                throw new Exception(GetIsConcurrentLogMsg("SiteGuid is in incorrect format.", siteGuid, udid, groupID));
            }

            if (siteGuidLong == 0)
            {
                // concurrency limitation does not apply for anonymous users.
                // anonymous user is identified by receiving SiteGuid=0 from the clients.
                return false;
            }

            // Get MCRuleID from PlayCycleSession on CB
            int mediaConcurrencyRuleID = 0;

            if (playType == ePlayType.MEDIA)
            {
                if (playCycleSession != null)
                {
                    mediaConcurrencyRuleID = playCycleSession.MediaConcurrencyRuleID;
                }
                else // get from DB incase getting from CB failed
                {
                    mediaConcurrencyRuleID = CatalogDAL.GetRuleIDPlayCycleKey(siteGuid, mediaID, mediaFileID, udid, platform);
                }
            }

            string domainsUsername = string.Empty;
            string domainsPassword = string.Empty;
            string domainsUrl = string.Empty;

            //get username + password from wsCache
            Credentials credentials = TvinciCache.WSCredentials.GetWSCredentials(ApiObjects.eWSModules.CATALOG, groupID, ApiObjects.eWSModules.DOMAINS);
            if (credentials != null)
            {
                domainsUsername = credentials.m_sUsername;
                domainsPassword = credentials.m_sPassword;
            }

            if (domainsUsername.Length == 0 || domainsPassword.Length == 0)
            {
                throw new Exception(GetIsConcurrentLogMsg("No WS_Domains login parameters were extracted from DB.", siteGuid, udid, groupID));
            }

            using (WS_Domains.module domains = new WS_Domains.module())
            {

                ValidationResponseObject domainsResp = domains.ValidateLimitationModule(
                    domainsUsername, domainsPassword, udid, 0, siteGuidLong, 0, ValidationType.Concurrency, mediaConcurrencyRuleID, 0, mediaID);

                if (domainsResp != null)
                {
                    domainID = (int)domainsResp.m_lDomainID;
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
                                throw new Exception(GetIsConcurrentLogMsg(String.Concat("WS_Domains returned status: ", domainsResp.m_eStatus.ToString()), siteGuid, udid, groupID));
                            }
                    }
                }
                else
                {
                    throw new Exception(GetIsConcurrentLogMsg("WS_Domains response is null.", siteGuid, udid, groupID));
                }
            }

            return result;
        }

        private static string GetIsConcurrentLogMsg(string sMessage, string sSiteGuid, string sUDID, int nGroupID)
        {
            StringBuilder sb = new StringBuilder("IsConcurrent Err. ");
            sb.Append(sMessage);
            sb.Append(String.Concat(" Site Guid: ", sSiteGuid));
            sb.Append(String.Concat(" UDID: ", sUDID));
            sb.Append(String.Concat(" Group ID: ", nGroupID));

            return sb.ToString();
        }

        internal static List<FileMedia> GetMediaFilesDetails(int groupID, List<int> mediaFileIDs, string mediaFileCoGuid)
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
            return (!string.IsNullOrEmpty(Utils.GetStrSafeVal(dr, "BRAND_HEIGHT")) && !dr["BRAND_HEIGHT"].ToString().Equals("0"))
                || (!string.IsNullOrEmpty(Utils.GetStrSafeVal(dr, "RECURRING_TYPE_ID")) && !dr["RECURRING_TYPE_ID"].ToString().Equals("0"));
        }

        /*Insert all files that return from the "CompleteDetailsForMediaResponse" into List<FileMedia>*/
        private static List<FileMedia> FilesValues(DataTable dtFileMedia, ref List<Branding> lBranding, bool noFileUrl, ref bool result)
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
                            fileMedia.m_nIsDefaultLanguage = Utils.GetIntSafeVal(dtFileMedia.Rows[i], "IS_DEFAULT_LANGUAGE");
                            fileMedia.m_sAltCoGUID = Utils.GetStrSafeVal(dtFileMedia.Rows[i], "ALT_CO_GUID");

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
                                fileMedia.m_oPreProvider.ProviderName = Utils.GetStrSafeVal(dtFileMedia.Rows[i], "COMMERCIAL_TYPE_PRE_NAME");
                                fileMedia.m_bIsPreSkipEnabled = Utils.GetIntSafeVal(dtFileMedia.Rows[i], "OUTER_COMMERCIAL_SKIP_PRE") == 1;

                            }
                            tempAdProvID = Utils.GetIntSafeVal(dtFileMedia.Rows[i], "COMMERCIAL_TYPE_POST_ID");
                            if (tempAdProvID != 0)
                            {
                                fileMedia.m_oPostProvider = new AdProvider();
                                fileMedia.m_oPostProvider.ProviderID = tempAdProvID;
                                fileMedia.m_oPostProvider.ProviderName = Utils.GetStrSafeVal(dtFileMedia.Rows[i], "COMMERCIAL_TYPE_POST_NAME");
                                fileMedia.m_bIsPostSkipEnabled = Utils.GetIntSafeVal(dtFileMedia.Rows[i], "OUTER_COMMERCIAL_SKIP_POST") == 1;
                            }
                            tempAdProvID = Utils.GetIntSafeVal(dtFileMedia.Rows[i], "COMMERCIAL_TYPE_BREAK_ID");
                            if (tempAdProvID != 0)
                            {
                                fileMedia.m_oBreakProvider = new AdProvider();
                                fileMedia.m_oBreakProvider.ProviderID = tempAdProvID;
                                fileMedia.m_oBreakProvider.ProviderName = Utils.GetStrSafeVal(dtFileMedia.Rows[i], "COMMERCIAL_TYPE_BREAK_NAME");
                                fileMedia.m_sBreakpoints = Utils.GetStrSafeVal(dtFileMedia.Rows[i], "COMMERCIAL_BREAK_POINTS");

                            }
                            tempAdProvID = Utils.GetIntSafeVal(dtFileMedia.Rows[i], "COMMERCIAL_TYPE_OVERLAY_ID");
                            if (tempAdProvID != 0)
                            {
                                fileMedia.m_oOverlayProvider = new AdProvider();
                                fileMedia.m_oOverlayProvider.ProviderID = tempAdProvID;
                                fileMedia.m_oOverlayProvider.ProviderName = Utils.GetStrSafeVal(dtFileMedia.Rows[i], "COMMERCIAL_TYPE_OVERLAY_NAME");
                                fileMedia.m_sOverlaypoints = Utils.GetStrSafeVal(dtFileMedia.Rows[i], "COMMERCIAL_OVERLAY_POINTS");
                            }
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

        internal static List<ChannelViewsResult> GetChannelViewsResult(int nGroupID)
        {
            List<ChannelViewsResult> channelViews = new List<ChannelViewsResult>();

            #region Define Aggregations Query
            ElasticSearch.Searcher.FilteredQuery filteredQuery = new ElasticSearch.Searcher.FilteredQuery()
            {
                PageIndex = 0,
                PageSize = 1
            };
            filteredQuery.Filter = new ElasticSearch.Searcher.QueryFilter();

            BaseFilterCompositeType filter = new FilterCompositeType(CutWith.AND);
            filter.AddChild(new ESTerm(true)
            {
                Key = "group_id",
                Value = nGroupID.ToString()
            });

            #region define date filter
            ESRange dateRange = new ESRange(false)
            {
                Key = "action_date"
            };
            string sMax = DateTime.UtcNow.ToString(ElasticSearch.Common.Utils.ES_DATE_FORMAT);
            string sMin = DateTime.UtcNow.AddSeconds(-30.0).ToString(ElasticSearch.Common.Utils.ES_DATE_FORMAT);
            dateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, sMin));
            dateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, sMax));
            filter.AddChild(dateRange);
            #endregion

            #region define action filter
            ESTerms esActionTerm = new ESTerms(false)
            {
                Key = "action"
            };
            esActionTerm.Value.Add(STAT_ACTION_MEDIA_HIT);
            filter.AddChild(esActionTerm);
            #endregion

            filteredQuery.Filter.FilterSettings = filter;

            var aggregations = new ESBaseAggsItem()
            {
                Name = "stats",
                Field = "media_id",
                Type = eElasticAggregationType.terms,
            };

            filteredQuery.Aggregations.Add(aggregations);

            #endregion

            string aggregationsQuery = filteredQuery.ToString();

            //Search
            string index = ElasticSearch.Common.Utils.GetGroupStatisticsIndex(nGroupID);
            ElasticSearch.Common.ElasticSearchApi esApi = new ElasticSearch.Common.ElasticSearchApi();
            string retval = esApi.Search(index, ElasticSearch.Common.Utils.ES_STATS_TYPE, ref aggregationsQuery);

            if (!string.IsNullOrEmpty(retval))
            {
                //Get aggregations results
                Dictionary<string, Dictionary<string, int>> aggregationResults = ESAggregationsResult.DeserializeAggrgations<string>(retval);

                if (aggregationResults != null && aggregationResults.Count > 0)
                {
                    Dictionary<string, int> aggregationResult;
                    //retrieve channel_views aggregations results
                    aggregationResults.TryGetValue("channel_views", out aggregationResult);

                    if (aggregationResult != null && aggregationResult.Count > 0)
                    {
                        foreach (string key in aggregationResult.Keys)
                        {
                            int count = aggregationResult[key];

                            int nChannelID;
                            if (int.TryParse(key, out nChannelID))
                            {
                                channelViews.Add(new ChannelViewsResult(nChannelID, count));
                            }

                        }
                    }
                }
            }

            return channelViews;
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

        internal static bool GetMediaMarkHitInitialData(string sSiteGuid, string userIP, int mediaID, int mediaFileID, ref int countryID,
            ref int ownerGroupID, ref int cdnID, ref int qualityID, ref int formatID, ref int mediaTypeID, ref int billingTypeID, ref int fileDuration)
        {
            if (!TVinciShared.WS_Utils.GetTcmBoolValue("CATALOG_HIT_CACHE"))
            {
                countryID = ElasticSearch.Utilities.IpToCountry.GetCountryByIp(userIP);
                return CatalogDAL.GetMediaPlayData(mediaID, mediaFileID, ref ownerGroupID, ref cdnID, ref qualityID, ref formatID, ref mediaTypeID, ref billingTypeID, ref fileDuration);
            }

            #region  try get values from catalog cache

            double cacheTime = TVinciShared.WS_Utils.GetTcmDoubleValue("CATALOG_HIT_CACHE_TIME_IN_MINUTES");

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
                countryID = (int)oCountryID;
                bIP = true;
            }

            string m_mf_Key = string.Format("{0}_media_{1}_mediaFile_{2}", eWSModules.CATALOG, mediaID, mediaFileID);
            List<KeyValuePair<string, int>> lMedia = catalogCache.Get<List<KeyValuePair<string, int>>>(m_mf_Key);

            if (lMedia != null && lMedia.Count > 0)
            {
                InitMediaMarkHitDataFromCache(ref ownerGroupID, ref cdnID, ref qualityID, ref formatID, ref mediaTypeID, ref billingTypeID, ref fileDuration, lMedia);

                log.DebugFormat("GetMediaMarkHitInitialData, get media mark hit datafrom cache: " +
                    "cdnID {0}, qualityID {1}, formatId {2}, mediaTypeId {3}, billingTypeId {4}, fileDuration {5}",
                    cdnID, qualityID, formatID, mediaTypeID, billingTypeID, fileDuration);

                if (cdnID == -1 && qualityID == -1 && formatID == -1 && mediaTypeID == -1 && billingTypeID == -1 && fileDuration == -1)
                {
                    return false;
                }

                bMedia = true;
            }

            #endregion

            if (!bIP) // try getting countryID from ES, if it fails get countryID from DB
            {
                log.DebugFormat("GetMediaMarkHitInitialData, try getting countryID from ES, if it fails get countryID from DB");

                countryID = ElasticSearch.Utilities.IpToCountry.GetCountryByIp(userIP);
                //getting from ES failed
                if (countryID == 0)
                {
                    log.DebugFormat("GetMediaMarkHitInitialData, getting country from ES failed, getting from DB");

                    long ipVal = 0;
                    ipVal = ParseIPOutOfString(userIP);
                    CatalogDAL.Get_IPCountryCode(ipVal, ref countryID);
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
                if (CatalogDAL.GetMediaPlayData(mediaID, mediaFileID, ref ownerGroupID, ref cdnID, ref qualityID, ref formatID, ref mediaTypeID, ref billingTypeID, ref fileDuration))
                {
                    InitMediaMarkHitDataToCache(ownerGroupID, cdnID, qualityID, formatID, mediaTypeID, billingTypeID, fileDuration, ref lMedia);

                    catalogCache.Set(m_mf_Key, lMedia, cacheTime);
                    bMedia = true;
                }
                else
                {
                    log.DebugFormat("GetMediaMarkHitInitialData, GetMediaPlayData failed, setting in cache invalid media");

                    // If couldn't get media - create an "invalid" record and save it in cache
                    InitMediaMarkHitDataToCache(ownerGroupID, -1, -1, -1, -1, -1, -1, ref lMedia);
                    catalogCache.Set(m_mf_Key, lMedia, cacheTime);
                    bMedia = false;
                }
            }
            else if (cdnID == -1 && qualityID == -1 && formatID == -1 && mediaTypeID == -1 && billingTypeID == -1 && fileDuration == -1)
            {
                bMedia = false;
            }
            
            return bIP && bMedia;
        }

        private static void InitMediaMarkHitDataFromCache(ref int ownerGroupID, ref int cdnID, ref int qualityID, ref int formatID, ref int mediaTypeID, ref int billingTypeID, ref int fileDuration, List<KeyValuePair<string, int>> lMedia)
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

        private static void InitMediaMarkHitDataToCache(int ownerGroupID, int cdnID, int qualityID, int formatID, int mediaTypeID, int billingTypeID, int fileDuration, ref List<KeyValuePair<string, int>> lMedia)
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

        internal static bool GetNPVRMarkHitInitialData(long domainRecordingId, ref int fileDuration, ref long recordingId, int groupId, int domainId)
        {
            bool result = false;
            bool shouldGoToCas = false;
            bool shouldCache = false;
            recordingId = 0;

            CatalogCache catalogCache = CatalogCache.Instance();
            string key = string.Format("Recording_{0}", domainRecordingId);

            if (!TVinciShared.WS_Utils.GetTcmBoolValue("CATALOG_HIT_CACHE"))
            {
                shouldGoToCas = true;
            }
            else
            {
                shouldCache = true;

                object cacheDuration = catalogCache.Get(key);

                if (cacheDuration != null)
                {
                    fileDuration = Convert.ToInt32(cacheDuration);
                    result = true;
                }
                else
                {
                    shouldGoToCas = true;
                }
            }

            if (shouldGoToCas)
            {
                string userName = string.Empty;
                string password = string.Empty;

                //get username + password from wsCache
                Credentials credentials =
                    TvinciCache.WSCredentials.GetWSCredentials(ApiObjects.eWSModules.CATALOG, groupId, ApiObjects.eWSModules.CONDITIONALACCESS);

                if (credentials != null)
                {
                    userName = credentials.m_sUsername;
                    password = credentials.m_sPassword;
                }

                // validate user name and password length
                if (userName.Length == 0 || password.Length == 0)
                {
                    throw new Exception(string.Format(
                        "No WS_CAS login parameters were extracted from DB. domainId = {0}, groupid={1}",
                        domainId, groupId));
                }

                using (WS_ConditionalAccess.module cas = new WS_ConditionalAccess.module())
                {
                    var recording = cas.GetRecordingByID(userName, password, domainId, domainRecordingId);
                    
                    // Validate recording
                    if (recording != null && recording.Status != null && recording.Status.Code == 0)
                    {
                        fileDuration = (int)((recording.EpgEndDate - recording.EpgStartDate).TotalSeconds);
                        recordingId = recording.Id;

                        if (shouldCache)
                        {
                            double timeInCache = (double)(fileDuration / 60);

                            bool setResult = catalogCache.Set(key, fileDuration, timeInCache);

                            if (!setResult)
                            {
                                log.ErrorFormat("Failed setting file duration of recording {0} in cache", domainRecordingId);
                            }
                        }

                        result = true;
                    }
                    else
                    {
                        // if recording is invalid, still cache that this recording is invalid

                        result = false;
                        catalogCache.Set(key, 0, 10);
                    }
                }
            }

            return result;
        }


        private static long ParseIPOutOfString(string userIP)
        {
            long nIPVal = 0;

            if (!string.IsNullOrEmpty(userIP))
            {
                string[] splited = userIP.Split('.');

                if (splited != null && splited.Length >= 3)
                {
                    nIPVal = long.Parse(splited[3]) + Int64.Parse(splited[2]) * 256 + Int64.Parse(splited[1]) * 256 * 256 + Int64.Parse(splited[0]) * 256 * 256 * 256;
                }
            }

            return nIPVal;

        }

        private static string BuildSlidingWindowCountAggregationRequest(int groupID, List<int> mediaIDs, DateTime startDate, DateTime endDate,
            string action)
        {
            #region Define Aggregation Query 
            ElasticSearch.Searcher.FilteredQuery filteredQuery = new ElasticSearch.Searcher.FilteredQuery()
            {
                PageIndex = 0,
                PageSize = 1
            };
            filteredQuery.Filter = new ElasticSearch.Searcher.QueryFilter();

            BaseFilterCompositeType filter = new FilterCompositeType(CutWith.AND);
            filter.AddChild(new ESTerm(true)
            {
                Key = "group_id",
                Value = groupID.ToString()
            });

            #region define date filter
            ESRange dateRange = new ESRange(false)
            {
                Key = "action_date"
            };
            string sMax = endDate.ToString(ElasticSearch.Common.Utils.ES_DATE_FORMAT);
            string sMin = startDate.ToString(ElasticSearch.Common.Utils.ES_DATE_FORMAT);
            dateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, sMin));
            dateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, sMax));
            filter.AddChild(dateRange);
            #endregion

            #region define action filter
            ESTerm esActionTerm = new ESTerm(false)
            {
                Key = "action",
                Value = action
            };
            filter.AddChild(esActionTerm);
            #endregion

            #region define media id filter
            ESTerms esMediaIdTerms = new ESTerms(true)
            {
                Key = "media_id"
            };
            esMediaIdTerms.Value.AddRange(mediaIDs.Select(item => item.ToString()));
            filter.AddChild(esMediaIdTerms);
            #endregion

            #region define order filter
            // if no ordering is specified the default is order by count descending
            #endregion

            filteredQuery.Filter.FilterSettings = filter;

            ESBaseAggsItem aggregation = new ESBaseAggsItem()
            {
                Field = "media_id",
                Name = STAT_SLIDING_WINDOW_AGGREGATION_NAME,
                Type = eElasticAggregationType.terms
            };

            filteredQuery.Aggregations.Add(aggregation);
            #endregion

            return filteredQuery.ToString();
        }

        internal static List<int> SlidingWindowCountAggregations(int nGroupId, List<int> lMediaIds, DateTime dtStartDate,
            DateTime dtEndDate, string action)
        {
            List<int> result = new List<int>();

            // if no ordering is specified to BuildSlidingWindowCountAggregationsRequest function then default is order by count descending
            string aggregationsQuery = BuildSlidingWindowCountAggregationRequest(nGroupId, lMediaIds, dtStartDate, dtEndDate, action);

            //Search
            string index = ElasticSearch.Common.Utils.GetGroupStatisticsIndex(nGroupId);
            ElasticSearch.Common.ElasticSearchApi esApi = new ElasticSearch.Common.ElasticSearchApi();
            string retval = esApi.Search(index, ElasticSearch.Common.Utils.ES_STATS_TYPE, ref aggregationsQuery);

            if (!string.IsNullOrEmpty(retval))
            {
                //Get aggregations results
                Dictionary<string, Dictionary<string, int>> aggregationResults = ESAggregationsResult.DeserializeAggrgations<string>(retval);

                if (aggregationResults != null && aggregationResults.Count > 0)
                {
                    Dictionary<string, int> aggregationResult;
                    //retrieve channel_views aggregations results
                    aggregationResults.TryGetValue(STAT_SLIDING_WINDOW_AGGREGATION_NAME, out aggregationResult);

                    if (aggregationResult != null && aggregationResult.Count > 0)
                    {
                        foreach (string key in aggregationResult.Keys)
                        {
                            int count = aggregationResult[key];

                            int nMediaId;
                            if (int.TryParse(key, out nMediaId))
                            {
                                result.Add(nMediaId);
                            }
                        }
                    }
                }
            }

            return result;
        }

        internal static Dictionary<int, int> SlidingWindowCountAggregationsMappings(int nGroupId, List<int> lMediaIds, DateTime dtStartDate,
            DateTime dtEndDate, string action)
        {
            Dictionary<int, int> result = new Dictionary<int, int>();

            string aggregationsQuery = BuildSlidingWindowCountAggregationRequest(nGroupId, lMediaIds, dtStartDate, dtEndDate, action);

            //Search
            string index = ElasticSearch.Common.Utils.GetGroupStatisticsIndex(nGroupId);
            ElasticSearch.Common.ElasticSearchApi esApi = new ElasticSearch.Common.ElasticSearchApi();
            string retval = esApi.Search(index, ElasticSearch.Common.Utils.ES_STATS_TYPE, ref aggregationsQuery);

            if (!string.IsNullOrEmpty(retval))
            {
                //Get aggregations results
                Dictionary<string, Dictionary<string, int>> aggregationResults = ESAggregationsResult.DeserializeAggrgations<string>(retval);

                if (aggregationResults != null && aggregationResults.Count > 0)
                {
                    Dictionary<string, int> aggregationResult;
                    //retrieve channel_views aggregations results
                    aggregationResults.TryGetValue(STAT_SLIDING_WINDOW_AGGREGATION_NAME, out aggregationResult);

                    if (aggregationResult != null && aggregationResult.Count > 0)
                    {
                        foreach (string key in aggregationResult.Keys)
                        {
                            int count = aggregationResult[key];

                            int nMediaId;
                            if (int.TryParse(key, out nMediaId) && !result.ContainsKey(nMediaId))
                            {
                                result.Add(nMediaId, count);
                            }
                        }
                    }
                }
            }

            return result;
        }

        private static string BuildSlidingWindowStatisticsAggregationRequest(int groupID, List<int> mediaIDs, DateTime startDate,
            DateTime endDate, string action, string valueField)
        {
            #region Define Aggregation Query
            ElasticSearch.Searcher.FilteredQuery filteredQuery = new ElasticSearch.Searcher.FilteredQuery()
            {
                PageIndex = 0,
                PageSize = 1
            };
            filteredQuery.Filter = new ElasticSearch.Searcher.QueryFilter();

            BaseFilterCompositeType filter = new FilterCompositeType(CutWith.AND);
            filter.AddChild(new ESTerm(true)
            {
                Key = "group_id",
                Value = groupID.ToString()
            });

            #region define date filter
            ESRange dateRange = new ESRange(false)
            {
                Key = "action_date"
            };
            string sMax = endDate.ToString(ElasticSearch.Common.Utils.ES_DATE_FORMAT);
            string sMin = startDate.ToString(ElasticSearch.Common.Utils.ES_DATE_FORMAT);
            dateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, sMin));
            dateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, sMax));
            filter.AddChild(dateRange);
            #endregion

            #region define action filter
            ESTerm esActionTerm = new ESTerm(false)
            {
                Key = "action",
                Value = action
            };
            filter.AddChild(esActionTerm);
            #endregion

            #region define media id filter
            ESTerms esMediaIdTerms = new ESTerms(true)
            {
                Key = "media_id"
            };
            esMediaIdTerms.Value.AddRange(mediaIDs.Select(item => item.ToString()));
            filter.AddChild(esMediaIdTerms);
            #endregion

            filteredQuery.Filter.FilterSettings = filter;

            #endregion

            var aggregation = new ESBaseAggsItem()
            {
                Name = STAT_SLIDING_WINDOW_AGGREGATION_NAME,
                Field = "media_id",
                Type = eElasticAggregationType.terms,
            };

            aggregation.SubAggrgations.Add(new ESBaseAggsItem()
            {
                Name = "sub_stats",
                Type = eElasticAggregationType.stats,
                Field = Catalog.STAT_ACTION_RATE_VALUE_FIELD
            });

            filteredQuery.Aggregations.Add(aggregation);

            return filteredQuery.ToString();
        }

        internal static List<int> SlidingWindowStatisticsAggregations(int nGroupId, List<int> lMediaIds, DateTime dtStartDate,
            DateTime dtEndDate, string action, string valueField, AggregationsComparer.eCompareType compareType)
        {
            List<int> result = new List<int>();

            string aggregationsQuery = BuildSlidingWindowStatisticsAggregationRequest(nGroupId, lMediaIds, dtStartDate, dtEndDate,
                action, valueField);

            //Search
            string index = ElasticSearch.Common.Utils.GetGroupStatisticsIndex(nGroupId);
            ElasticSearch.Common.ElasticSearchApi esApi = new ElasticSearch.Common.ElasticSearchApi();
            string retval = esApi.Search(index, ElasticSearch.Common.Utils.ES_STATS_TYPE, ref aggregationsQuery);

            if (!string.IsNullOrEmpty(retval))
            {
                //Get aggregation results
                Dictionary<string, List<StatisticsAggregationResult>> aggregationResults = 
                    ESAggregationsResult.DeserializeStatisticsAggregations(retval, "sub_stats");

                if (aggregationResults != null && aggregationResults.Count > 0)
                {
                    List<StatisticsAggregationResult> aggregationResult;
                    //retrieve channel_views aggregation results
                    aggregationResults.TryGetValue(STAT_SLIDING_WINDOW_AGGREGATION_NAME, out aggregationResult);

                    if (aggregationResult != null && aggregationResult.Count > 0)
                    {
                        int mediaId;

                        // sorts order by descending
                        aggregationResult.Sort(new AggregationsComparer(compareType));

                        foreach (var stats in aggregationResult)
                        {
                            if (int.TryParse(stats.key, out mediaId))
                            {
                                result.Add(mediaId);
                            }
                        }
                    }
                }
            }

            return result;
        }

        internal static int GetLastPosition(string NpvrID, int userID)
        {
            if (string.IsNullOrEmpty(NpvrID) || userID == 0)
                return 0;

            return CatalogDAL.GetLastPosition(NpvrID, userID);
        }

        /*This method return all last position (desc order by create date) by domain and \ or user_id 
         * if userType is household and user is default - return all last positions of all users in domain by assetID (BY MEDIA ID)         
         else return last position of user_id (incase userType is not household or last position of user_id and default_user (incase userType is household) */
        internal static AssetBookmarks GetAssetLastPosition(string assetID, eAssetTypes assetType, int userID, bool isDefaultUser, List<int> users, List<int> defaultUsers, Dictionary<string, User> usersDictionary)
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
            DomainMediaMark domainMediaMark = CatalogDAL.GetAssetLastPosition(assetID, assetType, usersToGetLastPosition);
            if (domainMediaMark == null || domainMediaMark.devices == null)
            {
                return response;
            }

            List<Bookmark> bookmarks = new List<Bookmark>();

            if (domainMediaMark.devices != null)
            {
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

                    if (!bookmarks.Where(x => x.User.m_sSiteGUID == userMediaMark.UserID.ToString()).Any())
                    {
                        bool isFinished = userMediaMark.AssetAction.ToUpper() == "FINISH" ? true :
                                          (((float)userMediaMark.Location / (float)userMediaMark.FileDuration) * 100 > FINISHED_PERCENT_THRESHOLD) ? true : false;
                        bookmarks.Add(new Bookmark(usersDictionary[userMediaMark.UserID.ToString()], userType, userMediaMark.Location, isFinished));
                    }
                }
            }

            if (bookmarks.Count > 0)
                response = new AssetBookmarks(assetType, assetID, bookmarks.OrderBy(x => x.User.m_sSiteGUID).ToList());

            return response;
        }

        internal static NPVRSeriesResponse GetSeriesRecordings(int groupID, NPVRSeriesRequest request)
        {
            NPVRSeriesResponse nPVRSeriesResponse = new NPVRSeriesResponse();

            if (NPVRProviderFactory.Instance().IsGroupHaveNPVRImpl(groupID))
            {
                INPVRProvider npvr = NPVRProviderFactory.Instance().GetProvider(groupID);
                if (npvr != null)
                {
                    int domainID = 0;
                    if (IsUserValid(request.m_sSiteGuid, groupID, ref domainID) && domainID > 0)
                    {
                        NPVRRetrieveSeriesResponse response = npvr.RetrieveSeries(new NPVRRetrieveParamsObj()
                        {
                            EntityID = domainID.ToString(),
                            PageIndex = request.m_nPageIndex,
                            PageSize = request.m_nPageSize
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
                                log.Error("Error - " + string.Format("GetSeriesRecordings. NPVR layer returned errorneus response. Req: {0} , Resp Err Msg: {1}", request.ToString(), response.msg));
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
                }
                else
                {
                    throw new Exception("INPVRProvider instance is null.");
                }
            }
            else
            {
                throw new ArgumentException(String.Concat("Group does not have NPVR implementation. G ID: ", groupID));
            }

            return nPVRSeriesResponse;
        }

        internal static List<RecordedEPGChannelProgrammeObject> GetRecordings(int groupID, NPVRRetrieveRequest request)
        {
            List<RecordedEPGChannelProgrammeObject> res = null;
            if (NPVRProviderFactory.Instance().IsGroupHaveNPVRImpl(groupID))
            {
                INPVRProvider npvr = NPVRProviderFactory.Instance().GetProvider(groupID);
                if (npvr != null)
                {
                    int domainID = 0;
                    if (IsUserValid(request.m_sSiteGuid, groupID, ref domainID) && domainID > 0)
                    {
                        NPVRRetrieveParamsObj args = new NPVRRetrieveParamsObj();
                        args.PageIndex = request.m_nPageIndex;
                        args.PageSize = request.m_nPageSize;
                        args.EntityID = domainID.ToString();
                        args.OrderBy = (NPVROrderBy)((int)request.m_oOrderObj.m_eOrderBy);
                        args.Direction = (NPVROrderDir)((int)request.m_oOrderObj.m_eOrderDir);
                        switch (request.m_eNPVRSearchBy)
                        {
                            case NPVRSearchBy.ByStartDate:
                                args.StartDate = request.m_dtStartDate;
                                args.SearchBy.Add(SearchByField.byStartTime);
                                break;
                            case NPVRSearchBy.ByRecordingStatus:
                                args.RecordingStatus.AddRange(request.m_lRecordingStatuses.Distinct().Select((item) => (NPVRRecordingStatus)((int)item)));
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
                                log.Error("Error - " + string.Format("GetRecordings. No epgs returned from CB for the request: {0}", request.ToString()));
                            }
                        }
                        if (request.m_lSeriesIDs != null && request.m_lSeriesIDs.Count > 0)
                        {
                            args.SeriesIDs.AddRange(request.m_lSeriesIDs.Distinct());
                            args.SearchBy.Add(SearchByField.bySeasonId);
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
                }
                else
                {
                    throw new Exception("INPVRProvider instance is null.");
                }
            }
            else
            {
                throw new ArgumentException(String.Concat("Group does not have NPVR implementation. G ID: ", groupID));
            }

            return res;
        }

        internal static bool IsUserValid(string siteGuid, int groupID, ref int domainID)
        {
            long temp = 0;
            if (!Int64.TryParse(siteGuid, out temp) || temp < 1)
                return false;
            Credentials oCredentials = TvinciCache.WSCredentials.GetWSCredentials(ApiObjects.eWSModules.CATALOG, groupID, ApiObjects.eWSModules.USERS);
            bool res = false;
            using (UsersService u = new UsersService())
            {
                UserResponseObject resp = u.GetUserData(oCredentials.m_sUsername, oCredentials.m_sPassword, siteGuid, string.Empty);
                if (resp != null && resp.m_RespStatus == ResponseStatus.OK && resp.m_user != null && resp.m_user.m_domianID > 0)
                {
                    domainID = resp.m_user.m_domianID;
                    res = true;
                }
                else
                {
                    res = false;
                }

            }

            return res;
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
                string sWSUsername = string.Empty;
                string sWSPassword = string.Empty;
                string sWSUrl = string.Empty;

                //get username + password from wsCache
                Credentials oCredentials = TvinciCache.WSCredentials.GetWSCredentials(ApiObjects.eWSModules.CATALOG, groupID, ApiObjects.eWSModules.DOMAINS);
                if (oCredentials != null)
                {
                    sWSUsername = oCredentials.m_sUsername;
                    sWSPassword = oCredentials.m_sPassword;
                }
                if (string.IsNullOrEmpty(sWSUsername) || string.IsNullOrEmpty(sWSPassword))
                {
                    throw new Exception(string.Format("No WS_Domains login parameters were extracted from DB. domainID={0}, groupID={1}", domainID, groupID));
                }

                // get domain info - to have the users list in domain + default users in domain
                using (WS_Domains.module domains = new WS_Domains.module())
                {
                    var domainRes = domains.GetDomainInfo(sWSUsername, sWSPassword, domainID);
                    if (domainRes != null)
                    {
                        domainResponse = domainRes;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("GetDomain - " + string.Format("Failed ex={0}, domainID={1}, groupdID={2}", ex.Message, domainID, groupID), ex);
            }

            return domainResponse;
        }

        internal static Dictionary<string, User> GetUsers(int groupID, List<int> users)
        {
            Dictionary<string, User> usersDictionary = new Dictionary<string, User>();
            UsersResponse usersResponse = null;
            Credentials oCredentials = TvinciCache.WSCredentials.GetWSCredentials(ApiObjects.eWSModules.CATALOG, groupID, ApiObjects.eWSModules.USERS);

            if (string.IsNullOrEmpty(oCredentials.m_sUsername) || string.IsNullOrEmpty(oCredentials.m_sPassword))
            {
                return usersDictionary;
            }

            using (UsersService u = new UsersService())
            {
                usersResponse = u.GetUsers(oCredentials.m_sUsername, oCredentials.m_sPassword, users.Select(i => i.ToString()).ToArray(), string.Empty);
            }
            if (usersResponse != null && usersResponse.resp != null && usersResponse.resp.Code == (int)ResponseStatus.OK && usersResponse.users != null)
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
        internal static void SetEpgSearchChannelsByRegions(ref EpgSearchObj searcherEpgSearch, EpgSearchRequest epgSearchRequest)
        {
            List<long> channelIds = null;
            List<int> regionIds;
            List<string> linearMediaTypes;

            // Get region/regions for search
            Catalog.SetSearchRegions(epgSearchRequest.m_nGroupID, epgSearchRequest.domainId,
                epgSearchRequest.m_sSiteGuid, out regionIds, out linearMediaTypes);

            // Ask Stored procedure for EPG Identifier of linear channel in current region(s), by joining media and media_regions
            channelIds = CatalogDAL.Get_EpgIdentifier_ByRegion(epgSearchRequest.m_nGroupID, regionIds);

            searcherEpgSearch.m_oEpgChannelIDs = new List<long>(channelIds);
        }

        public static bool SendRebuildIndexMessage(int groupId, eObjectType type, bool switchIndexAlias, bool deleteOldIndices,
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

                    result = queue.Enqueue(data, string.Format(@"Tasks\{0}\{1}", group.m_nParentGroupID, type.ToString()));
                }
            }
            catch (Exception ex)
            {
                result = false;
                log.ErrorFormat("Failed sending message to queue on rebuilding index: group id = {0}, ex = {1}", groupId, ex);
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
                    ApiObjects.CeleryIndexingData data = new CeleryIndexingData(group.m_nParentGroupID, new List<long>(), type,
                        eAction.Rebase, date);

                    var queue = new CatalogQueue();

                    result = queue.Enqueue(data, string.Format(@"Tasks\{0}\{1}", group.m_nParentGroupID, type.ToString()));
                }
            }
            catch (Exception ex)
            {
                result = false;
                log.ErrorFormat("Failed sending message to queue on rebasing index: group id = {0}, ex = {1}", groupId, ex);
            }

            return result;
        }

        #region External Channel Request

        internal static ApiObjects.Response.Status GetExternalChannelAssets(ExternalChannelRequest request, out int totalItems, out List<UnifiedSearchResult> searchResultsList, out string requestId)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status();

            searchResultsList = new List<UnifiedSearchResult>();
            totalItems = 0;
            requestId = "";

            var externalChannelsCache = ExternalChannelCache.Instance();

            // If no internal ID provided - get the internal ID by the external identifier
            if (string.IsNullOrEmpty(request.internalChannelID))
            {
                int internalId = CatalogDAL.GetExternalChannelIdByExternalIdentifier(request.m_nGroupID, request.externalChannelID);

                if (internalId > 0)
                {
                    request.internalChannelID = internalId.ToString();
                }
                else
                {
                    status.Code = (int)eResponseStatus.ExternalChannelNotExist;
                    status.Message = string.Format("External Channel with the external identifier {0} was not found.", request.externalChannelID);
                    return status;
                }
            }

            ExternalChannel externalChannel = externalChannelsCache.GetChannel(request.m_nGroupID, request.internalChannelID);

            if (externalChannel == null || externalChannel.ID <= 0)
            {
                status.Code = (int)eResponseStatus.ExternalChannelNotExist;
                status.Message = string.Format("External Channel with the ID {0} was not found.", request.internalChannelID);
                return status;
            }

            log.DebugFormat("GetExternalChannelAssets - external channel is: {0}", Newtonsoft.Json.JsonConvert.SerializeObject(externalChannel));

            // Build dictionary of enrichments for recommendation engine adapter
            Dictionary<string, string> enrichments = Catalog.GetEnrichments(request, externalChannel.Enrichments);

            // If no recommendation engine defined - use group's default
            if (externalChannel.RecommendationEngineId <= 0)
            {
                GroupManager groupManager = new GroupManager();
                externalChannel.RecommendationEngineId = groupManager.GetGroup(request.m_nGroupID).defaultRecommendationEngine;
            }

            // If there is still no recommendation engine
            if (externalChannel.RecommendationEngineId <= 0)
            {
                status.Code = (int)eResponseStatus.ExternalChannelHasNoRecommendationEngine;
                status.Message = "External Channel has no recommendation engine selected.";
                return status;
            }

            // Adapter will respond with a collection of media assets ID with Kaltura terminology
            List<RecommendationResult> recommendations =
                RecommendationAdapterController.GetInstance().GetChannelRecommendations(externalChannel, enrichments, request.free, out requestId,
                request.m_nPageIndex, request.m_nPageSize);

            if (recommendations == null)
            {
                status.Code = (int)(eResponseStatus.AdapterAppFailure);
                status.Message = "No recommendations received";
                return status;
            }

            if (recommendations.Count == 0)
            {
                return status;
            }

            ISearcher searcher = Bootstrapper.GetInstance<ISearcher>();

            // If there is no filter - no need to go to Searcher, just page the results list, fill update date and return it to client
            if (string.IsNullOrEmpty(externalChannel.FilterExpression) && string.IsNullOrEmpty(request.filterQuery))
            {
                var allRecommendations = recommendations.Select(result =>
                    new UnifiedSearchResult()
                    {
                        AssetId = result.id,
                        AssetType = (eAssetTypes)result.type,
                        m_dUpdateDate = DateTime.MinValue
                    }
                    ).ToList();

                searchResultsList =
                    searcher.FillUpdateDates(request.m_nGroupID, allRecommendations, ref totalItems, request.m_nPageSize, request.m_nPageIndex);
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

                    status = BooleanPhraseNode.ParseSearchExpression(externalChannel.FilterExpression, ref channelFilterTree);

                    if (status.Code != (int)eResponseStatus.OK)
                    {
                        return status;
                    }
                }

                // Parse filter expression of the client request
                if (!string.IsNullOrEmpty(request.filterQuery))
                {
                    request.filterQuery = HttpUtility.HtmlDecode(request.filterQuery);

                    status = BooleanPhraseNode.ParseSearchExpression(request.filterQuery, ref requestFilterTree);

                    if (status.Code != (int)eResponseStatus.OK)
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
                    Utils.IsGroupIDContainedInConfig(request.m_nGroupID, "GroupIDsWithIUserTypeSeperatedBySemiColon", ';'))
                {
                    if (request.m_oFilter == null)
                    {
                        request.m_oFilter = new Filter();
                    }

                    //call ws_users to get userType                  
                    request.m_oFilter.m_nUserTypeID = Utils.GetUserType(request.m_sSiteGuid, request.m_nGroupID);
                }

                UnifiedSearchDefinitions searchDefinitions = BuildUnifiedSearchObject(request, externalChannel, filterTree);

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

                if (searcher != null)
                {

                    int to = 0;
                    // The provided response should be filtered according to the Filter defined in the applicable 3rd-party channel settings
                    List<UnifiedSearchResult> searchResults = searcher.UnifiedSearch(searchDefinitions, ref totalItems, ref to);

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
                        RecommendationAdapterController.GetInstance().ShareFilteredResponse(externalChannel, recommendationResults);
                    }
                }
            }

            return status;
        }

        internal static ApiObjects.Response.Status GetExternalRelatedAssets(MediaRelatedExternalRequest request, out int totalItems, out List<RecommendationResult> resultsList, out string requestId)
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
                    status.Code = (int)eResponseStatus.RecommendationEngineNotExist;
                    return status;
                }

                int mediaTypeID = Catalog.GetMediaTypeID(request.m_nMediaID);
                if (mediaTypeID == 0)
                {
                    status.Message = "Asset doesn't exist";
                    status.Code = (int)eResponseStatus.BadSearchRequest;
                    return status;
                }

                List<ExternalRecommendationEngineEnrichment> enrichmentsToSend = new List<ExternalRecommendationEngineEnrichment>();

                foreach (int currentValue in Enum.GetValues(typeof(ExternalRecommendationEngineEnrichment)))
                {
                    if ((group.RelatedRecommendationEngineEnrichments & currentValue) > 0)
                    {
                        enrichmentsToSend.Add((ExternalRecommendationEngineEnrichment)currentValue);
                    }
                }

                Dictionary<string, string> enrichments = Catalog.GetEnrichments(request, enrichmentsToSend);

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
                                                                                                out requestId);
                }
                catch (KalturaException ex)
                {
                    if ((int)ex.Data["StatusCode"] == (int)eResponseStatus.RecommendationEngineNotExist)
                    {
                        status.Message = "Recommendation Engine Not Exist";
                        status.Code = (int)eResponseStatus.RecommendationEngineNotExist;
                    }
                    if ((int)ex.Data["StatusCode"] == (int)eResponseStatus.AdapterUrlRequired)
                    {
                        status.Message = "Recommendation engine adapter has no URL";
                        status.Code = (int)eResponseStatus.AdapterUrlRequired;
                    }
                    else
                    {
                        status.Message = "Adapter failed completing request";
                        status.Code = (int)eResponseStatus.AdapterAppFailure;
                    }
                    return status;
                }

                resultsList = recommendations;
                totalItems = recommendations.Count;

                if (recommendations == null)
                {
                    status.Code = (int)(eResponseStatus.AdapterAppFailure);
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

        internal static ApiObjects.Response.Status GetExternalSearchAssets(MediaSearchExternalRequest request, out int totalItems, out List<RecommendationResult> resultsList, out string requestId)
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
                int recommendationEngineId = group.SearchRecommendationEngine;

                if (recommendationEngineId == 0)
                {
                    status.Message = "Recommendation Engine Not Exist";
                    status.Code = (int)eResponseStatus.RecommendationEngineNotExist;
                    return status;
                }

                List<ExternalRecommendationEngineEnrichment> enrichmentsToSend = new List<ExternalRecommendationEngineEnrichment>();

                foreach (int currentValue in Enum.GetValues(typeof(ExternalRecommendationEngineEnrichment)))
                {
                    if ((group.SearchRecommendationEngineEnrichments & currentValue) > 0)
                    {
                        enrichmentsToSend.Add((ExternalRecommendationEngineEnrichment)currentValue);
                    }
                }

                Dictionary<string, string> enrichments = Catalog.GetEnrichments(request, enrichmentsToSend);

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
                                                                                                out requestId);
                }
                catch (KalturaException ex)
                {
                    if ((int)ex.Data["StatusCode"] == (int)eResponseStatus.RecommendationEngineNotExist)
                    {
                        status.Message = "Recommendation Engine Not Exist";
                        status.Code = (int)eResponseStatus.RecommendationEngineNotExist;
                    }
                    if ((int)ex.Data["StatusCode"] == (int)eResponseStatus.AdapterUrlRequired)
                    {
                        status.Message = "Recommendation engine adapter has no URL";
                        status.Code = (int)eResponseStatus.AdapterUrlRequired;
                    }
                    else
                    {
                        status.Message = "Adapter failed completing request";
                        status.Code = (int)eResponseStatus.AdapterAppFailure;
                    }
                    return status;
                }

                resultsList = recommendations;
                totalItems = recommendations.Count;

                if (recommendations == null)
                {
                    status.Code = (int)(eResponseStatus.AdapterAppFailure);
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

        /// <summary>
        /// Builds a dictionary of enrichments for recommendation engine adapter
        /// </summary>
        /// <param name="request"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetEnrichments(BaseRequest request, List<ExternalRecommendationEngineEnrichment> list)
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
                                string coutnryCode = ElasticSearch.Utilities.IpToCountry.GetCountryCodeByIp(request.m_sUserIP);

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
                    case ExternalRecommendationEngineEnrichment.NPVRSupport:
                        {
                            if (request is ExternalChannelRequest)
                            {
                                log.ErrorFormat("GetEnrichments - channel {0} has unsupported enirchment {1} / {2} defined",
                                    (request as ExternalChannelRequest).internalChannelID, (int)enrichment, enrichment.ToString());
                            }
                            break;
                        }
                    case ExternalRecommendationEngineEnrichment.Catchup:
                        {
                            if (request is ExternalChannelRequest)
                            {
                                log.ErrorFormat("GetEnrichments - channel {0} has unsupported enirchment {1} / {2} defined",
                                    (request as ExternalChannelRequest).internalChannelID, (int)enrichment, enrichment.ToString());
                            }

                            break;
                        }
                    case ExternalRecommendationEngineEnrichment.Parental:
                        {
                            if (request is ExternalChannelRequest)
                            {
                                log.ErrorFormat("GetEnrichments - channel {0} has unsupported enirchment {1} / {2} defined",
                                    (request as ExternalChannelRequest).internalChannelID, (int)enrichment, enrichment.ToString());
                            }
                            break;
                        }
                    case ExternalRecommendationEngineEnrichment.DTTRegion:
                        {
                            // External ID of region of current domain

                            GroupManager manager = new GroupManager();
                            Group group = manager.GetGroup(request.m_nGroupID);

                            int regionId = GetRegionIdOfDomain(request.m_nGroupID, request.domainId, request.m_sSiteGuid, group);

                            DataRow regionRow = ODBCWrapper.Utils.GetTableSingleRow("linear_channels_regions", regionId);

                            if (regionRow != null)
                            {
                                dictionary["region"] = ODBCWrapper.Utils.ExtractString(regionRow, "EXTERNAL_ID");
                            }

                            break;
                        }
                    case ExternalRecommendationEngineEnrichment.AtHome:
                        {
                            if (request is ExternalChannelRequest)
                            {
                                log.ErrorFormat("GetEnrichments - channel {0} has unsupported enirchment {1} / {2} defined",
                                    (request as ExternalChannelRequest).internalChannelID, (int)enrichment, enrichment.ToString());
                            }

                            break;
                        }
                    default:
                        {
                            if (request is ExternalChannelRequest)
                            {
                                log.ErrorFormat("GetEnrichments - channel {0} has unsupported enirchment {1} / {2} defined",
                                    (request as ExternalChannelRequest).internalChannelID, (int)enrichment, enrichment.ToString());
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
                request.m_nGroupID, string.Empty, string.Empty, null, null, externalChannel.FilterExpression, string.Empty,
                filterTree);

            UnifiedSearchDefinitions definitions = BuildUnifiedSearchObject(alternateRequest);

            // Order is a new kind - "recommendation". Which means the order is predefined
            definitions.order = new OrderObj()
            {
                m_eOrderBy = ApiObjects.SearchObjects.OrderBy.RECOMMENDATION,
                m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC
            };

            return definitions;
        }

        #endregion

        #region Internal Channel Request

        internal static ApiObjects.Response.Status GetInternalChannelAssets(InternalChannelRequest request, out int totalItems, out List<UnifiedSearchResult> searchResults)
        {
            // Set default values for out parameters
            totalItems = 0;
            searchResults = new List<UnifiedSearchResult>();

            ApiObjects.Response.Status status = null;

            Group group = null;
            GroupsCacheManager.Channel channel = null;

            // Get group and channel objects from cache/DB
            GroupManager groupManager = new GroupsCacheManager.GroupManager();
            CatalogCache catalogCache = CatalogCache.Instance();

            int parentGroupID = catalogCache.GetParentGroup(request.m_nGroupID);

            if (string.IsNullOrEmpty(request.internalChannelID))
            {
                return new ApiObjects.Response.Status((int)eResponseStatus.Error, "Internal Channel ID was not provided");
            }

            int channelId = int.Parse(request.internalChannelID);

            groupManager.GetGroupAndChannel(channelId, parentGroupID, ref group, ref channel);

            if (channel == null)
            {
                return new ApiObjects.Response.Status((int)eResponseStatus.ObjectNotExist, string.Format("Channel with identifier {1} does not exist for group {0}", parentGroupID, channelId));
            }

            // Build search object
            UnifiedSearchDefinitions unifiedSearchDefinitions = BuildInternalChannelSearchObject(channel, request, group);

            int pageIndex = 0;
            int pageSize = 0;

            // If this is a manual channel, a sliding window or we have an additional filter - 
            // the initial search will not be paged. Paging will be done later on
            if (channel.m_nChannelTypeID == (int)ChannelType.Manual || ChannelRequest.IsSlidingWindow(channel))
            {
                pageIndex = unifiedSearchDefinitions.pageIndex;
                pageSize = unifiedSearchDefinitions.pageSize;
                unifiedSearchDefinitions.pageSize = 0;
                unifiedSearchDefinitions.pageIndex = 0;
            }

            ISearcher searcher = Bootstrapper.GetInstance<ISearcher>();

            if (searcher == null)
            {
                return new ApiObjects.Response.Status((int)eResponseStatus.Error, "Failed getting instance of searcher");
            }

            int to = 0;

            // Perform initial search of channel
            searchResults = searcher.UnifiedSearch(unifiedSearchDefinitions, ref totalItems, ref to);

            if (searchResults == null)
            {
                return new ApiObjects.Response.Status((int)eResponseStatus.Error, "Failed performing channel search");
            }

            List<int> assetIDs = searchResults.Select(item => int.Parse(item.AssetId)).ToList();

            #region Sliding Window

            if (ChannelRequest.IsSlidingWindow(channel))
            {
                assetIDs = ChannelRequest.OrderMediaBySlidingWindow(group.m_nParentGroupID, channel.m_OrderObject.m_eOrderBy,
                    channel.m_OrderObject.m_eOrderDir == ApiObjects.SearchObjects.OrderDir.DESC, pageSize,
                    pageIndex, assetIDs, channel.m_OrderObject.m_dSlidingWindowStartTimeField);

                totalItems = 0;

                if (assetIDs != null && assetIDs.Count > 0)
                {
                    totalItems = assetIDs.Count;
                    Dictionary<string, UnifiedSearchResult> assetDictionary = searchResults.ToDictionary(item => item.AssetId);

                    searchResults.Clear();

                    foreach (int item in assetIDs)
                    {
                        if (assetDictionary.ContainsKey(item.ToString()))
                        {
                            searchResults.Add(assetDictionary[item.ToString()]);
                        }
                    }
                }
                else
                {
                    searchResults.Clear();
                }
            }

            #endregion

            #region Channel Type - Manual

            if (channel.m_nChannelTypeID == (int)ChannelType.Manual)
            {
                ChannelRequest.OrderMediasByOrderNum(ref assetIDs, channel, unifiedSearchDefinitions.order);

                int validNumberOfMediasRange = pageSize;

                if (Utils.ValidatePageSizeAndPageIndexAgainstNumberOfMedias(assetIDs.Count, pageIndex, ref validNumberOfMediasRange))
                {
                    if (validNumberOfMediasRange > 0)
                    {
                        assetIDs = assetIDs.GetRange(pageSize * pageIndex, validNumberOfMediasRange);
                    }
                }
                else
                {
                    assetIDs.Clear();
                }

                if (searcher.GetType().Equals(typeof(ElasticsearchWrapper)) && assetIDs != null && assetIDs.Count > 0)
                {
                    Dictionary<string, UnifiedSearchResult> assetDictionary = searchResults.ToDictionary(item => item.AssetId);

                    searchResults = new List<UnifiedSearchResult>();

                    foreach (int item in assetIDs)
                    {
                        if (assetDictionary.ContainsKey(item.ToString()))
                        {
                            searchResults.Add(assetDictionary[item.ToString()]);
                        }
                    }
                }
            }

            #endregion

            if (assetIDs == null)
            {
                searchResults = null;
                totalItems = 0;
                return new ApiObjects.Response.Status((int)eResponseStatus.Error, "Failed performing channel search");
            }

            status = new ApiObjects.Response.Status((int)eResponseStatus.OK);

            return status;
        }

        internal static ApiObjects.Response.Status GetRelatedAssets(MediaRelatedRequest request, out int totalItems, out List<UnifiedSearchResult> searchResults)
        {
            // Set default values for out parameters
            totalItems = 0;
            searchResults = new List<UnifiedSearchResult>();

            ApiObjects.Response.Status status = null;

            Group group = null;

            // Get group and channel objects from cache/DB
            GroupManager groupManager = new GroupsCacheManager.GroupManager();
            CatalogCache catalogCache = CatalogCache.Instance();

            int parentGroupID = catalogCache.GetParentGroup(request.m_nGroupID);

            group = groupManager.GetGroup(parentGroupID);

            // Build search object
            UnifiedSearchDefinitions unifiedSearchDefinitions = BuildRelatedObject(request, group);

            int pageIndex = request.m_nPageIndex;
            int pageSize = request.m_nPageSize;

            ISearcher searcher = Bootstrapper.GetInstance<ISearcher>();

            if (searcher == null)
            {
                return new ApiObjects.Response.Status((int)eResponseStatus.Error, "Failed getting instance of searcher");
            }

            int to = 0;

            // Perform initial search of channel
            searchResults = searcher.UnifiedSearch(unifiedSearchDefinitions, ref totalItems, ref to);

            if (searchResults == null)
            {
                return new ApiObjects.Response.Status((int)eResponseStatus.Error, "Failed performing related assets search");
            }

            List<int> assetIDs = searchResults.Select(item => int.Parse(item.AssetId)).ToList();

            if (assetIDs == null)
            {
                searchResults = null;
                totalItems = 0;
                return new ApiObjects.Response.Status((int)eResponseStatus.Error, "Failed performing related assets search");
            }

            status = new ApiObjects.Response.Status((int)eResponseStatus.OK);

            return status;
        }

        private static UnifiedSearchDefinitions BuildRelatedObject(MediaRelatedRequest request, Group group)
        {
            UnifiedSearchDefinitions definitions = new UnifiedSearchDefinitions();
            definitions.shouldSearchEpg = true;
            definitions.shouldSearchMedia = true;

            Filter filter = new Filter();

            bool bIsMainLang = Utils.IsLangMain(request.m_nGroupID, request.m_oFilter.m_nLanguage);

            MediaSearchRequest mediaSearchRequest =
                BuildMediasRequest(request.m_nMediaID, bIsMainLang, request.m_oFilter, ref filter, request.m_nGroupID, request.m_nMediaTypes, request.m_sSiteGuid, request.OrderObj);

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
            definitions.indexGroupId = group.m_nParentGroupID;

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
                deviceRules = ProtocolsFuncs.GetDeviceAllowedRuleIDs(request.m_oFilter.m_sDeviceId, request.m_nGroupID).ToArray();
            }

            definitions.deviceRuleId = deviceRules;

            #endregion

            #region Media Types, Permitted Watch Rules, Language

            // BEO-1338: Related media types is from the Media Search Request object - it knows the best!
            definitions.mediaTypes = mediaSearchRequest.m_nMediaTypes;

            if (group.m_sPermittedWatchRules != null && group.m_sPermittedWatchRules.Count > 0)
            {
                definitions.permittedWatchRules = string.Join(" ", group.m_sPermittedWatchRules);
            }

            #endregion

            #region Request Filter Object

            if (request.m_oFilter != null)
            {
                definitions.shouldUseStartDate = request.m_oFilter.m_bUseStartDate;
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

                        BooleanLeaf leaf = new BooleanLeaf(
                            string.Format("tags.{0}{1}", key.ToLower(), suffix), value.ToLower(), typeof(string), ComparisonOperator.Equals);
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

                        BooleanLeaf leaf = new BooleanLeaf(
                            string.Format("metas.{0}{1}", key.ToLower(), suffix), value.ToLower(), typeof(string), ComparisonOperator.Equals);
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
                var status = BooleanPhraseNode.ParseSearchExpression(filterExpression, ref filterTree);

                if (status.Code != (int)eResponseStatus.OK)
                {
                    throw new KalturaException(status.Message, status.Code);
                }

                // Add prefixes, check if non start/end date exist
                UpdateNodeTreeFields(request, ref filterTree, definitions, group);

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
            }

            definitions.filterPhrase = root;

            #endregion

            return definitions;
        }

        /// <summary>
        /// Update filter tree fields for specific fields/values.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filterTree"></param>
        /// <param name="definitions"></param>
        /// <param name="group"></param>
        public static void UpdateNodeTreeFields(BaseRequest request, ref BooleanPhraseNode filterTree, UnifiedSearchDefinitions definitions, Group group)
        {
            if (group != null)
            {
                Dictionary<BooleanPhraseNode, BooleanPhrase> parentMapping = new Dictionary<BooleanPhraseNode, BooleanPhrase>();

                Queue<BooleanPhraseNode> nodes = new Queue<BooleanPhraseNode>();
                nodes.Enqueue(filterTree);

                // BFS
                while (nodes.Count > 0)
                {
                    BooleanPhraseNode node = nodes.Dequeue();

                    // If it is a leaf, just replace the field name
                    if (node.type == BooleanNodeType.Leaf)
                    {
                        TreatLeaf(request, ref filterTree, definitions, group, node, parentMapping);
                    }
                    else if (node.type == BooleanNodeType.Parent)
                    {
                        BooleanPhrase phrase = node as BooleanPhrase;

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

        /// <summary>
        /// Update filter tree node fields for specific fields/values.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filterTree"></param>
        /// <param name="definitions"></param>
        /// <param name="group"></param>
        /// <param name="node"></param>
        public static void TreatLeaf(BaseRequest request, ref BooleanPhraseNode filterTree, UnifiedSearchDefinitions definitions,
            Group group, BooleanPhraseNode node, Dictionary<BooleanPhraseNode, BooleanPhrase> parentMapping)
        {
            bool shouldUseCache = WS_Utils.GetTcmBoolValue("Use_Search_Cache");

            // initialize maximum nGram member only once - when this is negative it is still not set
            if (maxNGram < 0)
            {
                maxNGram = TVinciShared.WS_Utils.GetTcmIntValue("max_ngram");
            }

            List<int> geoBlockRules = null;
            Dictionary<string, List<string>> mediaParentalRulesTags = null;
            Dictionary<string, List<string>> epgParentalRulesTags = null;

            BooleanLeaf leaf = node as BooleanLeaf;
            bool isTagOrMeta;

            // Add prefix (meta/tag) e.g. metas.{key}

            HashSet<string> searchKeys = GetUnifiedSearchKey(leaf.field, group, out isTagOrMeta);

            string suffix = string.Empty;

            if (definitions.langauge != null && !definitions.langauge.IsDefault)
            {
                suffix = string.Format("_{0}", definitions.langauge.Code);
            }

            if (searchKeys.Count > 1)
            {
                if (isTagOrMeta)
                {
                    List<BooleanPhraseNode> newList = new List<BooleanPhraseNode>();

                    // Split the single leaf into several brothers connected with:
                    // "or" operand (if it positive)
                    // "and" operand (if it negative)
                    foreach (var searchKey in searchKeys)
                    {
                        // add language suffix (if the language is not the default)
                        string languageSpecificSearchKey = string.Format("{0}{1}", searchKey, suffix);

                        newList.Add(new BooleanLeaf(languageSpecificSearchKey, leaf.value, leaf.valueType, leaf.operand));
                    }

                    eCutType cutType = eCutType.Or;

                    if (leaf.operand == ComparisonOperator.NotContains || leaf.operand == ComparisonOperator.NotEquals)
                    {
                        cutType = eCutType.And;
                    }

                    BooleanPhrase newPhrase = new BooleanPhrase(newList, cutType);

                    BooleanPhraseNode.ReplaceLeafWithPhrase(ref filterTree, parentMapping, leaf, newPhrase);
                }
            }
            else if (searchKeys.Count == 1)
            {
                string searchKeyLowered = searchKeys.FirstOrDefault().ToLower();
                string originalKey = leaf.field;

                // Default - string, until proved otherwise
                leaf.valueType = typeof(string);

                // If this is a tag or a meta, we need to add the language suffix
                // If not, we check if it is one of the "core" fields.
                // If it is not one of them, an exception will be thrown
                if (isTagOrMeta)
                {
                    // add language suffix (if the language is not the default)
                    string languageSpecificSearchKey = string.Format("{0}{1}", searchKeyLowered, suffix);

                    searchKeys.Clear();
                    searchKeys.Add(languageSpecificSearchKey);

                    leaf.field = languageSpecificSearchKey;
                }
                else
                {
                    // If the filter uses non-default start/end dates, we tell the definitions no to use default start/end date
                    if (searchKeyLowered == "start_date")
                    {
                        definitions.defaultStartDate = false;
                        GetLeafDate(ref leaf, request.m_dServerTime);

                        if (!definitions.shouldDateSearchesApplyToAllTypes)
                        {
                            leaf.assetTypes = new List<eObjectType>()
                            {
                                eObjectType.EPG,
                                eObjectType.Recording
                            };
                        }
                    }
                    else if (searchKeyLowered == "end_date")
                    {
                        definitions.defaultEndDate = false;
                        GetLeafDate(ref leaf, request.m_dServerTime);

                        if (!definitions.shouldDateSearchesApplyToAllTypes)
                        {
                            leaf.assetTypes = new List<eObjectType>()
                            {
                                eObjectType.EPG,
                                eObjectType.Recording
                            };
                        }
                    }
                    else if (searchKeyLowered == "update_date")
                    {
                        GetLeafDate(ref leaf, request.m_dServerTime);
                    }
                    else if (searchKeyLowered == "geo_block")
                    {
                        // geo_block is a personal filter that currently will work only with "true".
                        if (leaf.operand == ComparisonOperator.Equals && leaf.value.ToString().ToLower() == "true")
                        {
                            if (geoBlockRules == null)
                            {
                                geoBlockRules = GetGeoBlockRules(request.m_nGroupID, request.m_sUserIP);
                            }

                            BooleanLeaf mediaTypeCondition = new BooleanLeaf("_type", "media", typeof(string), ComparisonOperator.Prefix);
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
                            throw new KalturaException("Invalid search value or operator was sent for geo_block", (int)eResponseStatus.BadSearchRequest);
                        }
                    }
                    else if (searchKeyLowered == "parental_rules")
                    {
                        // Same as geo_block: it is a personal filter that currently will work only with "true".
                        if (leaf.operand == ComparisonOperator.Equals && leaf.value.ToString().ToLower() == "true")
                        {
                            if (mediaParentalRulesTags == null || epgParentalRulesTags == null)
                            {
                                if (shouldUseCache)
                                {
                                    var parentalRulesTags = ParentalRulesTagsCache.Instance().GetParentalRulesTags(request.m_nGroupID, request.m_sSiteGuid);

                                    if (parentalRulesTags != null)
                                    {
                                        mediaParentalRulesTags = parentalRulesTags.mediaTags;
                                        epgParentalRulesTags = parentalRulesTags.epgTags;
                                    }
                                }
                                else
                                {
                                    Catalog.GetParentalRulesTags(request.m_nGroupID, request.m_sSiteGuid, out mediaParentalRulesTags, out epgParentalRulesTags);
                                }
                            }

                            List<BooleanPhraseNode> newMediaNodes = new List<BooleanPhraseNode>();
                            List<BooleanPhraseNode> newEpgNodes = new List<BooleanPhraseNode>();

                            newMediaNodes.Add(new BooleanLeaf("_type", "media", typeof(string), ComparisonOperator.Prefix));

                            // Run on all tags and their values
                            foreach (KeyValuePair<string, List<string>> tagValues in mediaParentalRulesTags)
                            {
                                // Create a Not-in leaf for each of the tags
                                BooleanLeaf newLeaf = new BooleanLeaf(
                                    string.Concat("tags.", tagValues.Key.ToLower()),
                                    tagValues.Value,
                                    typeof(List<string>),
                                    ComparisonOperator.NotIn);

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
                                    string.Concat("tags.", tagValues.Key.ToLower()),
                                    tagValues.Value,
                                    typeof(List<string>),
                                    ComparisonOperator.NotIn);

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
                            throw new KalturaException("Invalid search value or operator was sent for parental_rules", (int)eResponseStatus.BadSearchRequest);
                        }
                    }
                    else if (searchKeyLowered == ESUnifiedQueryBuilder.ENTITLED_ASSETS_FIELD)
                    {
                        // Same as geo_block: it is a personal filter that currently will work only with "true".
                        if (leaf.operand != ComparisonOperator.Equals)
                        {
                            throw new KalturaException("Invalid search value or operator was sent for entitled_assets", (int)eResponseStatus.BadSearchRequest);
                        }

                        string loweredValue = leaf.value.ToString().ToLower();

                        definitions.entitlementSearchDefinitions = new EntitlementSearchDefinitions();

                        switch (loweredValue)
                        {
                            case ("free"):
                            {
                                definitions.entitlementSearchDefinitions.shouldGetFreeAssets = true;
                                break;
                            }
                            case ("entitled"):
                            {
                                definitions.entitlementSearchDefinitions.shouldGetPurchasedAssets = true;
                                break;
                            }
                            case ("both"):
                            {
                                definitions.entitlementSearchDefinitions.shouldGetFreeAssets = true;
                                definitions.entitlementSearchDefinitions.shouldGetPurchasedAssets = true;
                                break;
                            }
                            default:
                            {
                                definitions.entitlementSearchDefinitions = null;
                                throw new KalturaException("Invalid search value or operator was sent for entitled_assets", (int)eResponseStatus.BadSearchRequest);
                            }
                        }

                        // I mock a "contains" operator so that the query builder will know it is a not-exact search
                        leaf.operand = ComparisonOperator.Contains;
                    }
                    else if (reservedUnifiedSearchNumericFields.Contains(searchKeyLowered))
                    {
                        leaf.valueType = typeof(long);
                    }
                    else if (reservedUnifiedSearchStringFields.Contains(searchKeyLowered))
                    {
                        if (searchKeyLowered == "name" || searchKeyLowered == "description")
                        {
                            // add language suffix (if the language is not the default)
                            searchKeys.Clear();
                            searchKeys.Add(string.Format("{0}{1}", searchKeyLowered, suffix));
                        }
                    }
                    else
                    {
                        throw new KalturaException(string.Format("Invalid search key was sent: {0}", originalKey), (int)eResponseStatus.InvalidSearchField);
                    }
                }

                leaf.field = searchKeys.FirstOrDefault();

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
                        throw new KalturaException(string.Format("Invalid IN clause of: {0}", originalKey), (int)eResponseStatus.SyntaxError);
                    }

                    foreach (var single in values)
                    {
                        int temporaryInteger;

                        if (!int.TryParse(single, out temporaryInteger))
                        {
                            throw new KalturaException(string.Format("Invalid IN clause of: {0}", originalKey),
                                (int)eResponseStatus.SyntaxError);
                        }
                    }

                    // Put new list of strings in boolean leaf
                    leaf.value = values.ToList();
                }

                #endregion

            }

            #region Trim search value

            // If the search is contains or not contains, trim the search value to the size of the maximum NGram.
            // Otherwise the search will not work completely 
            if (maxNGram > 0 &&
                (leaf.operand == ComparisonOperator.Contains || leaf.operand == ComparisonOperator.NotContains
                || leaf.operand == ComparisonOperator.WordStartsWith))
            {
                leaf.value = leaf.value.ToString().Truncate(maxNGram);
            }

            #endregion
        }

        private static void GetLeafDate(ref BooleanLeaf leaf, DateTime serverTime)
        {
            leaf.valueType = typeof(DateTime);
            long epoch = Convert.ToInt64(leaf.value);

            // if the epoch time is greater then 1980 - it's a date, otherwise it's relative (to now) time in seconds
            if (epoch > UNIX_TIME_1980)
                leaf.value = DateUtils.UnixTimeStampToDateTime(epoch);
            else
                leaf.value = serverTime.AddSeconds(epoch);
        }

        public static UnifiedSearchDefinitions BuildInternalChannelSearchObject(GroupsCacheManager.Channel channel, InternalChannelRequest request, Group group)
        {
            UnifiedSearchDefinitions definitions = new UnifiedSearchDefinitions();

            #region Basic

            definitions.groupId = channel.m_nGroupID;
            definitions.indexGroupId = group.m_nParentGroupID;

            definitions.pageIndex = request.m_nPageIndex;
            definitions.pageSize = request.m_nPageSize;

            definitions.shouldAddActive = request.m_oFilter != null ? request.m_oFilter.m_bOnlyActiveMedia : true;

            #endregion

            #region Device Rules

            int[] deviceRules = null;

            if (request.m_oFilter != null)
            {
                deviceRules = ProtocolsFuncs.GetDeviceAllowedRuleIDs(request.m_oFilter.m_sDeviceId, request.m_nGroupID).ToArray();
            }

            definitions.deviceRuleId = deviceRules;

            definitions.shouldIgnoreDeviceRuleID = request.m_bIgnoreDeviceRuleID;
            #endregion

            #region Media Types, Permitted Watch Rules, Language

            definitions.mediaTypes = channel.m_nMediaType.ToList();

            if (group.m_sPermittedWatchRules != null && group.m_sPermittedWatchRules.Count > 0)
            {
                definitions.permittedWatchRules = string.Join(" ", group.m_sPermittedWatchRules);
            }

            definitions.langauge = group.GetGroupDefaultLanguage();

            #endregion

            #region Order

            var orderObj = request.order;

            ApiObjects.SearchObjects.OrderObj searcherOrderObj = new ApiObjects.SearchObjects.OrderObj();

            if (orderObj != null && orderObj.m_eOrderBy != ApiObjects.SearchObjects.OrderBy.NONE)
            {
                GetOrderValues(ref searcherOrderObj, orderObj);
            }
            else
            {
                GetOrderValues(ref searcherOrderObj, channel.m_OrderObject);
            }

            definitions.order = searcherOrderObj;

            #endregion

            #region Request Filter Object

            if (request.m_oFilter != null)
            {
                definitions.shouldUseStartDate = request.m_oFilter.m_bUseStartDate;
                definitions.shouldUseFinalEndDate = request.m_oFilter.m_bUseFinalDate;
                definitions.userTypeID = request.m_oFilter.m_nUserTypeID;
            }

            #endregion

            BooleanPhraseNode initialTree = null;
            bool emptyRequest = false;

            // If this is a KSQL channel
            if (channel.m_nChannelTypeID == (int)ChannelType.KSQL)
            {
                BooleanPhraseNode filterTree = null;
                var parseStatus = BooleanPhraseNode.ParseSearchExpression(channel.filterQuery, ref filterTree);

                if (parseStatus.Code != (int)eResponseStatus.OK)
                {
                    throw new KalturaException(parseStatus.Message, parseStatus.Code);
                }
                else
                {
                    initialTree = filterTree;
                    Catalog.UpdateNodeTreeFields(request, ref initialTree, definitions, group);
                }

                #region Asset Types

                definitions.shouldSearchEpg = false;
                definitions.shouldSearchMedia = false;

                // Special case - if no type was specified or "All" is contained, search all types
                if ((definitions.mediaTypes == null || definitions.mediaTypes.Count == 0) ||
                    (definitions.mediaTypes.Count == 1 && definitions.mediaTypes.Remove(0)))
                {
                    definitions.shouldSearchEpg = true;
                    definitions.shouldSearchMedia = true;
                }

                if (definitions.mediaTypes.Remove(GroupsCacheManager.Channel.EPG_ASSET_TYPE))
                {
                    definitions.shouldSearchEpg = true;
                }

                // If there are items left in media types after removing 0, we are searching for media
                if (definitions.mediaTypes.Count > 0)
                {
                    definitions.shouldSearchMedia = true;
                }

                HashSet<int> mediaTypes = new HashSet<int>(group.GetMediaTypes());

                // Validate that the media types in the "assetTypes" list exist in the group's list of media types
                foreach (var mediaType in definitions.mediaTypes)
                {
                    // If one of them doesn't exist, throw an exception that says the request is bad
                    if (!mediaTypes.Contains(mediaType))
                    {
                        throw new KalturaException(string.Format("Invalid media type was sent: {0}", mediaType), (int)eResponseStatus.BadSearchRequest);
                    }
                }

                #endregion
            }
            else
            {
                definitions.shouldSearchMedia = true;

                #region Channel Tags

                eCutType cutType = eCutType.And;
                switch (channel.m_eCutWith)
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

                // If there is at least one tag
                if (channel.m_lChannelTags != null && channel.m_lChannelTags.Count > 0)
                {
                    List<BooleanPhraseNode> channelTagsNodes = new List<BooleanPhraseNode>();

                    foreach (SearchValue searchValue in channel.m_lChannelTags)
                    {
                        if (!string.IsNullOrEmpty(searchValue.m_sKey))
                        {
                            eCutType innerCutType = eCutType.And;
                            switch (channel.m_eCutWith)
                            {
                                case CutWith.WCF_ONLY_DEFAULT_VALUE:
                                break;
                                case CutWith.OR:
                                {
                                    innerCutType = eCutType.Or;
                                    break;
                                }
                                case CutWith.AND:
                                {
                                    innerCutType = eCutType.And;
                                    break;
                                }
                                default:
                                break;
                            }

                            string key = searchValue.m_sKey.ToLower();

                            BooleanPhraseNode newNode = null;
                            if (!string.IsNullOrEmpty(searchValue.m_sKeyPrefix))
                            {
                                key = string.Format("{0}.{1}", searchValue.m_sKeyPrefix, key);
                            }

                            // If only 1 value, use it as a single node (there's no need to create a phrase with 1 node...)
                            if (searchValue.m_lValue.Count == 1)
                            {
                                newNode = new BooleanLeaf(key, searchValue.m_lValue[0], typeof(string), ComparisonOperator.Equals);
                            }
                            else
                            {
                                // If there are several values, connect all the values with the inner cut type
                                List<BooleanPhraseNode> innerNodes = new List<BooleanPhraseNode>();

                                foreach (var item in searchValue.m_lValue)
                                {
                                    BooleanLeaf leaf = new BooleanLeaf(key, item, typeof(string), ComparisonOperator.Equals);
                                    innerNodes.Add(leaf);
                                }

                                newNode = new BooleanPhrase(innerNodes, innerCutType);
                            }

                            channelTagsNodes.Add(newNode);
                        }
                    }

                    initialTree = new BooleanPhrase(channelTagsNodes, cutType);
                }
                else
                {
                    // if there are no tags:
                    // filter everything out
                    emptyRequest = true;
                }

                if (channel.m_nChannelTypeID == (int)ChannelType.Automatic && 
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
                initialTree = new BooleanLeaf("media_Id", 0);
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

                if (status.Code != (int)eResponseStatus.OK)
                {
                    throw new KalturaException(status.Message, status.Code);
                }

                Catalog.UpdateNodeTreeFields(request, ref requestFilterTree, definitions, group);

                if (definitions.entitlementSearchDefinitions != null)
                {
                    UnifiedSearchDefinitionsBuilder.BuildEntitlementSearchDefinitions(definitions, request, request.order, group.m_nParentGroupID, group);
                }

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

            #endregion

            // Get days offset for EPG search from TCM
            definitions.epgDaysOffest = Catalog.GetCurrentRequestDaysOffset();

            #region Regions and associations

            List<int> regionIds;
            List<string> linearMediaTypes;

            Catalog.SetSearchRegions(request.m_nGroupID, request.domainId, request.m_sSiteGuid, out regionIds, out linearMediaTypes);

            definitions.regionIds = regionIds;
            definitions.linearChannelMediaTypes = linearMediaTypes;

            Catalog.GetParentMediaTypesAssociations(request.m_nGroupID,
                out definitions.parentMediaTypes, out definitions.associationTags);

            #endregion

            return definitions;
        }

        public static UnifiedSearchDefinitions BuildInternalChannelSearchObjectWithBaseRequest(GroupsCacheManager.Channel channel, BaseRequest request, Group group)
        {
            InternalChannelRequest channelRequest = new InternalChannelRequest(
                channel.m_nChannelID.ToString(),
                string.Empty,
                group.m_nParentGroupID,
                request.m_nPageSize,
                request.m_nPageIndex,
                request.m_sUserIP,
                request.m_sSignature,
                request.m_sSignString,
                request.m_oFilter,
                string.Empty,
                new OrderObj()
                {

                }
                );

            return BuildInternalChannelSearchObject(channel, channelRequest, group);
        }

        private static MediaSearchObj BuildInternalChannelSearchObject(GroupsCacheManager.Channel channel, InternalChannelRequest request, int groupId, LanguageObj languageObj, List<string> lPermittedWatchRules)
        {
            int[] nDeviceRuleId = null;
            if (request.m_oFilter != null)
                nDeviceRuleId = ProtocolsFuncs.GetDeviceAllowedRuleIDs(request.m_oFilter.m_sDeviceId, request.m_nGroupID).ToArray();

            return Catalog.BuildBaseChannelSearchObject(channel, request,
                request.order, groupId, channel.m_nGroupID == channel.m_nParentGroupID ? lPermittedWatchRules : null, nDeviceRuleId, languageObj);
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

        public static void WriteMediaEohStatistics(int nWatcherID, string sSessionID, int m_nGroupID, int nOwnerGroupID, int mediaId, int nMediaFileID, int nBillingTypeID, int nCDNID, int nMediaDuration,
                                                   int nCountryID, int nPlayerID, int nFirstPlayCounter, int nPlayCounter, int nLoadCounter, int nPauseCounter, int nStopCounter, int nFinishCounter, int nFullScreenCounter,
                                                   int nExitFullScreenCounterint, int nSendToFriendCounter, int nPlayTimeCounter, int nFileQualityID, int nFileFormatID, DateTime dStartHourDate, int nUpdaterID,
                                                   int nBrowser, int nPlatform, string sSiteGuid, string sDeviceUdID, string sPlayCycleID, int nSwooshCounter, ContextData context)
        {
            try
            {
                context.Load();
                // We write an empty string as the first parameter to split the start of the log from the mediaEoh row data
                string infoToLog = string.Join(",", new object[] { " ", nWatcherID, sSessionID, m_nGroupID, nOwnerGroupID, mediaId, nMediaFileID, nBillingTypeID, nCDNID, nMediaDuration, nCountryID, nPlayerID,
                                                               nFirstPlayCounter, nPlayCounter, nLoadCounter, nPauseCounter, nStopCounter, nFinishCounter, nFullScreenCounter, nExitFullScreenCounterint,
                                                               nSendToFriendCounter, nPlayTimeCounter, nFileQualityID, nFileFormatID, dStartHourDate, nUpdaterID, nBrowser, nPlatform, sSiteGuid,
                                                               sDeviceUdID, sPlayCycleID, nSwooshCounter });
                statisticsLog.Info(infoToLog);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in WriteMediaEohStatistics, mediaID: {0}, mediaFileID: {1}, groupID: {2}, siteGuid: {3}, udid: {4}, Exception: {5}", mediaId,
                                    nMediaFileID, m_nGroupID, sSiteGuid, sDeviceUdID, ex);
            }
        }

        public static bool UpdateRecordingsIndex(List<long> recordingsIds, int groupId, eAction action)
        {
            return Catalog.Update(recordingsIds, groupId, eObjectType.Recording, action);
        }

        public static bool RebuildEpgChannel(int groupId, int epgChannelID, DateTime fromDate, DateTime toDate, bool duplicates)
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

                log.DebugFormat("RebuildEpgChannel:{0}, epgChannelID:{1}, fromDate:{2}, toDate:{3}", groupId, epgChannelID, fromDate, toDate);
                DataSet ds = EpgDal.Get_EpgProgramsDetailsByChannelIds(groupId, epgChannelID, fromDate, toDate);
                List<EpgCB> epgs = ConvertToEpgCB(ds, mainLang);

                // get all epg program ids from DB in channel ID between date (all statuses)
                List<long> lProgramsID = GetEpgIds(epgChannelID, groupId, fromDate, toDate);
                log.DebugFormat("RebuildEpgProgramsChannel : lProgramsID:{0}, epgChannelID:{1}", string.Join(",", lProgramsID), epgChannelID);

                #region Delete
                // delete all current Epgs in CB related to channel between dates 
                BaseEpgBL epgBL = EpgBL.Utils.GetInstance(groupId);
                epgBL.RemoveGroupPrograms(lProgramsID.ConvertAll<int>(x => (int)x));
                // Delete from ES
                bool resultEpgIndex;
                if (lProgramsID != null && lProgramsID.Count > 0)
                {
                    resultEpgIndex = UpdateEpgIndex(lProgramsID, groupId, ApiObjects.eAction.Delete);
                }
                #endregion

                #region insert Bulks

                // insert all above  
                int nCount = 0;
                int nCountPackage = TVinciShared.WS_Utils.GetTcmIntValue("update_epg_package");
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
                    epgIds.Add((long)epg.EpgID);
                    if (nCount >= nCountPackage)
                    {
                        epgIds.Add((long)epg.EpgID);
                        resultEpgIndex = UpdateEpgIndex(epgIds, groupId, ApiObjects.eAction.Update);
                        epgIds = new List<long>();
                        nCount = 0;
                    }
                   
                }

                if (nCount > 0 && epgIds != null && epgIds.Count > 0)
                {
                    resultEpgIndex = UpdateEpgIndex(epgIds, groupId, ApiObjects.eAction.Update);
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

        private static bool RemoveDuplicatesEpgPrograms(int groupId, int epgChannelID, DateTime fromDate, DateTime toDate)
        {           
            try
            {
                List<long> epgIds = EpgDal.GetEpgIds(epgChannelID, groupId, fromDate, toDate, 2);
                if (epgIds != null && epgIds.Count > 0)
                {
                    BaseEpgBL epgBL = EpgBL.Utils.GetInstance(groupId);
                    epgBL.RemoveGroupPrograms(epgIds.ConvertAll<int>(x => (int)x));
                    bool resultEpgIndex = UpdateEpgIndex(epgIds, groupId, ApiObjects.eAction.Delete);                    
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
                            epgPicture.Url = ODBCWrapper.Utils.GetSafeStr(dr, "m_sURL"); ;

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
                                item.Metas.Add(key, new List<string>() { value });
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
                                item.Tags.Add(key, new List<string>() { value });
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

        private static void BasicEpgProgramDetails(DataTable dt, DataTable dtUpdateDate ,List<EpgCB> epgs, string mainLang)
        {           
            EpgCB epg;
            try
            {
                foreach (DataRow dr in dt.Rows)
                {
                    epg = new EpgCB();
                    string pic_url = string.Empty;
                    epg.EpgID = (ulong)Utils.GetLongSafeVal(dr, "ID");
                    epg.EpgIdentifier = Utils.GetStrSafeVal(dr, "EPG_IDENTIFIER");
                    epg.Name = Utils.GetStrSafeVal(dr, "NAME");
                    epg.Description = Utils.GetStrSafeVal(dr, "DESCRIPTION");
                    epg.ChannelID = Utils.GetIntSafeVal(dr, "EPG_CHANNEL_ID");
                    epg.PicUrl = Utils.GetStrSafeVal(dr, "PIC_URL");
                    epg.Status = Utils.GetIntSafeVal(dr, "STATUS");
                    epg.isActive = Utils.GetIntSafeVal(dr, "IS_ACTIVE") == 1 ? true : false;
                    epg.GroupID = Utils.GetIntSafeVal(dr, "GROUP_ID");
                    epg.PicID = Utils.GetIntSafeVal(dr, "pic_id");
                    epg.ParentGroupID = Utils.GetIntSafeVal(dr, "PARENT_GROUP_ID");
                    epg.GroupID = Utils.GetIntSafeVal(dr, "GROUP_ID");
                    //Dates
                    epg.StartDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "START_DATE");
                    epg.EndDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "END_DATE");
                    epg.CreateDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "CREATE_DATE");
                    epg.EnableCatchUp =  Utils.GetIntSafeVal(dr, "ENABLE_CATCH_UP");
                    epg.EnableCDVR =  Utils.GetIntSafeVal(dr, "ENABLE_CDVR");
                    epg.EnableStartOver =  Utils.GetIntSafeVal(dr, "ENABLE_START_OVER");
                    epg.EnableTrickPlay=  Utils.GetIntSafeVal(dr, "ENABLE_TRICK_PLAY");
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

        public static ApiObjects.Response.Status ClearStatistics(int groupId, DateTime until)
        {
            ApiObjects.Response.Status status = null;

            var wrapper = new ElasticsearchWrapper();
            status = wrapper.DeleteStatistics(groupId, until);

            return status;
        }

        public static List<WatchHistory> GetUserWatchHistory(int groupId, string siteGuid, List<int> assetTypes,
            List<string> assetIds, List<int> excludedAssetTypes, eWatchStatus filterStatus, int numOfDays,
            ApiObjects.SearchObjects.OrderDir orderDir, int pageIndex, int pageSize, int finishedPercent, out int totalItems)
        {
            List<WatchHistory> usersWatchHistory = new List<WatchHistory>();
            var mediaMarksManager = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.MEDIAMARK);
            var mediaHitsManager = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.MEDIA_HITS);

            totalItems = 0;

            // build date filter
            long minFilterdate = ODBCWrapper.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow.AddDays(-numOfDays));
            long maxFilterDate = ODBCWrapper.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow);

            try
            {
                CouchbaseManager.ViewStaleState staleState = CouchbaseManager.ViewStaleState.Ok;

                string staleStateConfiguration = ODBCWrapper.Utils.GetTcmConfigValue("WatchHistory_StaleMode");

                if (!string.IsNullOrEmpty(staleStateConfiguration))
                {
                    // try to parse the TCM value - if successful, use it, if not, make sure we are with default value of OK
                    if (!Enum.TryParse<CouchbaseManager.ViewStaleState>(staleStateConfiguration, out staleState))
                    {
                        staleState = CouchbaseManager.ViewStaleState.Ok;
                    }
                }

                // get views
                CouchbaseManager.ViewManager viewManager = new CouchbaseManager.ViewManager(CB_MEDIA_MARK_DESGIN, "users_watch_history")
                {
                    startKey = new object[] { long.Parse(siteGuid), minFilterdate },
                    endKey = new object[] { long.Parse(siteGuid), maxFilterDate },
                    staleState = staleState,
                    asJson = true
                };

                List<WatchHistory> unFilteredresult = mediaMarksManager.View<WatchHistory>(viewManager);

                if (unFilteredresult != null && unFilteredresult.Count > 0)
                {
                    GroupsCacheManager.GroupManager groupManager = new GroupsCacheManager.GroupManager();
                    Group group = groupManager.GetGroup(groupId);
                    if (group.m_sPermittedWatchRules != null && group.m_sPermittedWatchRules.Count > 0)
                    {
                        string watchRules = string.Join(" ", group.m_sPermittedWatchRules);

                        // validate media on ES
                        UnifiedSearchDefinitions searchDefinitions = new UnifiedSearchDefinitions()
                        {
                            groupId = groupId,
                            permittedWatchRules = watchRules,
                            specificAssets = new Dictionary<eAssetTypes, List<string>>(),
                            defaultEndDate = true,
                            defaultStartDate = true,
                            shouldUseFinalEndDate = true,
                            shouldUseStartDate = true,
                            shouldAddActive = true,
                            shouldSearchMedia = true,
                        };

                        searchDefinitions.specificAssets.Add(eAssetTypes.MEDIA, unFilteredresult.Where(x => x.AssetTypeId != (int)eAssetTypes.EPG &&
                                                                                             x.AssetTypeId != (int)eAssetTypes.NPVR)
                                                                                             .Select(x => x.AssetId).ToList());

                        searchDefinitions.specificAssets.Add(eAssetTypes.NPVR, unFilteredresult.Where(x => x.AssetTypeId == (int)eAssetTypes.NPVR)
                                                                                             .Select(x => x.RecordingId.ToString()).ToList());

                        ElasticsearchWrapper esWrapper = new ElasticsearchWrapper();
                        int esTotalItems = 0, to = 0;
                        var searchResults = esWrapper.UnifiedSearch(searchDefinitions, ref esTotalItems, ref to);

                        List<int> activeMediaIds = new List<int>();
                        List<string> activeRecordingIds = new List<string>();

                        if (searchResults != null && searchResults.Count > 0)
                        {
                            foreach (var searchResult in searchResults)
                            {
                                int assetId = int.Parse(searchResult.AssetId);

                                if (searchResult.AssetType == eAssetTypes.MEDIA)
                                {
                                    activeMediaIds.Add(assetId);
                                    unFilteredresult.First(x => int.Parse(x.AssetId) == assetId &&
                                                                x.AssetTypeId != (int)eAssetTypes.EPG &&
                                                                x.AssetTypeId != (int)eAssetTypes.NPVR)
                                                                .UpdateDate = searchResult.m_dUpdateDate;
                                }
                                else if (searchResult.AssetType == eAssetTypes.NPVR)
                                {
                                    activeRecordingIds.Add(searchResult.AssetId);
                                    unFilteredresult.First(x => x.AssetId == searchResult.AssetId &&
                                                                x.AssetTypeId == (int)eAssetTypes.NPVR)
                                                                .UpdateDate = searchResult.m_dUpdateDate;
                                }
                            }
                        }

                        //remove medias that are not active
                        unFilteredresult.RemoveAll(x => x.AssetTypeId != (int)eAssetTypes.EPG &&
                            x.AssetTypeId != (int)eAssetTypes.NPVR &&
                            !activeMediaIds.Contains(int.Parse(x.AssetId)));

                        //remove recordings that are not active
                        unFilteredresult.RemoveAll(x =>
                            x.AssetTypeId == (int)eAssetTypes.NPVR &&
                            !activeRecordingIds.Contains(x.AssetId));
                    }

                    // filter status 
                    switch (filterStatus)
                    {
                        case eWatchStatus.Progress:
                            // remove all finished
                            unFilteredresult.RemoveAll(x => (x.Duration != 0) && (((float)x.Location / (float)x.Duration * 100) >= finishedPercent));
                            unFilteredresult.ForEach(x => x.IsFinishedWatching = false);
                            break;

                        case eWatchStatus.Done:

                            // remove all in progress
                            unFilteredresult.RemoveAll(x => (x.Duration != 0) && (((float)x.Location / (float)x.Duration * 100) < finishedPercent));
                            unFilteredresult.ForEach(x => x.IsFinishedWatching = true);
                            break;

                        case eWatchStatus.All:

                            foreach (var item in unFilteredresult)
                            {
                                if ((item.Duration != 0) && (((float)item.Location / (float)item.Duration * 100) >= finishedPercent))
                                    item.IsFinishedWatching = true;
                                else
                                    item.IsFinishedWatching = false;
                            }
                            break;

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

                    // Location is not saved on Media Marks bucket. It is saved in media hits bucket.
                    // We will get the location of the relevant media marks from this bucket
                    // But we will go only if necessary and only for the unfinished assets
                    if (filterStatus != eWatchStatus.Done && unFilteredresult.Count > 0)
                    {
                        List<string> keysToGetLocation = new List<string>();

                        foreach (var currentResult in unFilteredresult)
                        {
                            if (!currentResult.IsFinishedWatching)
                            {
                                string key = GetWatchHistoryCouchbaseKey(currentResult);

                                keysToGetLocation.Add(key);
                            }
                        }

                        if (keysToGetLocation.Count > 0)
                        {                            
                            var mediaHitsDictionary = mediaHitsManager.GetValues<MediaMarkLog>(keysToGetLocation, true, true);                            

                            if (mediaHitsDictionary != null && mediaHitsDictionary.Keys.Count() > 0)
                            {
                                foreach (var currentResult in unFilteredresult)
                                {
                                    string key = GetWatchHistoryCouchbaseKey(currentResult);

                                    if (mediaHitsDictionary.ContainsKey(key))
                                    {                                        
                                        currentResult.Location = mediaHitsDictionary[key].LastMark.Location;
                                    }
                                }
                            }
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

                    // page index 
                    usersWatchHistory = unFilteredresult.Skip(pageSize * pageIndex).Take(pageSize).ToList();
                }
            }
            catch (Exception ex)
            {
                // ASK IRA. NO LOG IN THIS DAMN CLASS !!!
                throw ex;
            }

            return usersWatchHistory;
        }

        private static string GetWatchHistoryCouchbaseKey(WatchHistory currentResult)
        {
            string assetType = string.Empty;

            switch (currentResult.AssetTypeId)
            {
                case (int)eAssetTypes.EPG:                    {
                        assetType = "epg";
                        break;
                    }

                case (int)eAssetTypes.NPVR:
                    {
                        assetType = "npvr";
                        break;
                    }
                case (int)eAssetTypes.MEDIA:
                default:
                    {
                        assetType = "m";
                        break;
                    }

            }

            string key = string.Format("u{0}_{1}{2}", currentResult.UserID, assetType, currentResult.AssetId);
            return key;
        }

        public static void WriteNewWatcherMediaActionLog(int nWatcherID, string sSessionID, int nBillingTypeID, int nOwnerGroupID, int nQualityID, int nFormatID, int nMediaID, int nMediaFileID, int nGroupID,
                                                        int nCDNID, int nActionID, int nCountryID, int nPlayerID, int nLoc, int nBrowser, int nPlatform, string sSiteGUID, string sUDID, ContextData context)
        {
            try
            {
                context.Load();
                // We write an empty string as the first parameter to split the start of the log from the mediaEoh row data
                string infoToLog = string.Join(",", new object[] { " ", nWatcherID, sSessionID, nBillingTypeID, nOwnerGroupID, nQualityID, nFormatID, nMediaID, nMediaFileID, nGroupID, nCDNID,
                                                                        nActionID, nCountryID, nPlayerID, nLoc, nBrowser, nPlatform, sSiteGUID, sUDID });
                newWatcherMediaActionLog.Info(infoToLog);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error in WriteNewWatcherMediaActionLog, nWatcherID: {0}, mediaID: {1}, mediaFileID: {2}, groupID: {3}, actionID: {4}, userId: {5}",
                                         nMediaID, nMediaFileID, nGroupID, nActionID, sSiteGUID), ex);
            }
        }
    }
}



