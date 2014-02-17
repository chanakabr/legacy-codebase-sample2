using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Logger;
using System.Reflection;
using System.Configuration;
using TVinciShared;
using ApiObjects;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using DAL;
using Tvinci.Core.DAL;
using ApiObjects.SearchObjects;
using ApiObjects.MediaIndexingObjects;
using QueueWrapper;
using EpgBL;

namespace Catalog
{
    public class Catalog
    {
        private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly string TAGS = "tags";
        private static readonly string METAS = "metas";

        private static readonly string LINEAR_MEDIA_TYPES_KEY = "LinearMediaTypes";
        private static readonly string PERMITTED_WATCH_RULES_KEY = "PermittedWatchRules";

        private const int DEFAULT_SEARCHER_MAX_RESULTS_SIZE = 100000;

        internal const int DEFAULT_PWWAWP_MAX_RESULTS_SIZE = 8;
        internal const int DEFAULT_PWLALP_MAX_RESULTS_SIZE = 8;
        /*Get All Relevant Details About Media (by id) , 
         Use Stored Procedure */
        public static bool CompleteDetailsForMediaResponse(MediasProtocolRequest mediaRequest, ref MediaResponse mediaResponse, int nStartIndex, int nEndIndex)
        {
            //Int32 nMedia;
            MediaObj oMediaObj = new MediaObj();
            List<BaseObject> lMediaObj = new List<BaseObject>();

            try
            {
                bool bIsMainLang = Utils.IsLangMain(mediaRequest.m_nGroupID, mediaRequest.m_oFilter.m_nLanguage);

                if (nStartIndex == 0 && nEndIndex == 0 && mediaRequest.m_lMediasIds != null && mediaRequest.m_lMediasIds.Count > 0)
                    nEndIndex = mediaRequest.m_lMediasIds.Count();

                //Start MultiThread Call
                Task[] tasks = new Task[nEndIndex - nStartIndex];
                ConcurrentDictionary<int, BaseObject> dMediaObj = new ConcurrentDictionary<int, BaseObject>();

                //Build the Dictionary to keep the specific order of the mediaIds
                for (int i = nStartIndex; i < nEndIndex; i++)
                {
                    int nMedia = mediaRequest.m_lMediasIds[i];
                    dMediaObj.TryAdd(nMedia, null);
                }


                //complete media id details 
                for (int i = nStartIndex; i < nEndIndex; i++)
                {
                    int nMedia = mediaRequest.m_lMediasIds[i];

                    tasks[i - nStartIndex] = new Task(
                         (obj) =>
                         {
                             try
                             {
                                 int taskMediaID = (int)obj;

                                 dMediaObj[taskMediaID] = GetMediaDetails(taskMediaID, mediaRequest, bIsMainLang);
                             }
                             catch (Exception ex)
                             {
                                 _logger.Error(ex.Message, ex);
                             }
                         }, nMedia);
                    tasks[i - nStartIndex].Start();
                }
                //Wait to all parallels tasks to finished:
                Task.WaitAll(tasks);

                if (mediaRequest.m_lMediasIds != null)
                {
                    mediaResponse.m_nTotalItems = mediaRequest.m_lMediasIds.Count;
                    foreach (int nMedia in mediaRequest.m_lMediasIds)
                    {
                        mediaResponse.m_lObj.Add(dMediaObj[nMedia]);
                    }
                }

                _logger.Info(string.Format("Finish Complete Details for {0} MediaIds", nEndIndex));
                return true;
            }

            catch (Exception ex)
            {
                _logger.Error("faild to complete details", ex);
                throw ex;
            }
        }

        private static MediaObj GetMediaDetails(int nMedia, MediasProtocolRequest mediaRequest, bool bIsMainLang)
        {
           bool result = true;
            try
            {
                MediaObj oMediaObj = new MediaObj();
                
                // get mediaID and complete all details per media
                _logger.Info(string.Format("MediaId : {0}", nMedia));

                string sEndDate = string.Empty;
                bool bOnlyActiveMedia = true;
                bool bUseStartDate = true;
                int nLanguage = 0;

                if (mediaRequest.m_oFilter != null)
                {
                    sEndDate = ProtocolsFuncs.GetFinalEndDateField(mediaRequest.m_oFilter.m_bUseFinalDate);
                    bOnlyActiveMedia = mediaRequest.m_oFilter.m_bOnlyActiveMedia;
                    bUseStartDate = mediaRequest.m_oFilter.m_bUseStartDate;
                    nLanguage = mediaRequest.m_oFilter.m_nLanguage;
                }

                DataSet ds = CatalogDAL.Get_MediaDetails(mediaRequest.m_nGroupID, nMedia, mediaRequest.m_sSiteGuid, bOnlyActiveMedia, nLanguage, sEndDate, bUseStartDate);

                if (ds == null)
                    return null;
                if (ds.Tables.Count >= 6)
                {
                    bool isMedia = GetMediaBasicDetails(ref oMediaObj, ds.Tables[0], ds.Tables[5], bIsMainLang);
                    if (isMedia) //only if we found basic details for media - media in status = 1 , and active if necessary
                    {
                        oMediaObj.m_lPicture = GetAllPic(ds.Tables[1], ref result);
                        if (!result)
                        {
                            return null;
                        }
                        oMediaObj.m_lBranding = new List<Branding>();
                        oMediaObj.m_lFiles = FilesValues(ds.Tables[2], ref oMediaObj.m_lBranding, mediaRequest.m_oFilter.m_noFileUrl, nMedia, ref result);
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

                        /*last watched - By SiteGuid <> 0*/
                        if (ds.Tables.Count == 7)
                        {
                            if (ds.Tables[6].Rows != null && ds.Tables[6].Rows.Count > 0)
                            {
                                oMediaObj.m_sLastWatchedDevice = Utils.GetStrSafeVal(ds.Tables[6].Rows[0],"LastDeviceName");
                                string sLastWatchedDate = Utils.GetStrSafeVal(ds.Tables[6].Rows[0], "LastWatchedDate");
                                if (!string.IsNullOrEmpty(sLastWatchedDate))
                                {
                                    oMediaObj.m_dLastWatchedDate = System.Convert.ToDateTime(sLastWatchedDate);
                                }
                            }
                        }
                    }
                    else
                    {                       
                        return null;
                    }
                }
                return oMediaObj;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
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

                        oTags.m_oTagMeta = new TagMeta(Utils.GetStrSafeVal(dtTags.Rows[i],"tag_type_name"), typeof(string).ToString());
                        nTagId = Utils.GetIntSafeVal(dtTags.Rows[i],"tag_type_id");
                        oTags.m_lValues.Add(Utils.GetStrSafeVal(dtTags.Rows[i],"value"));
                        int j = i + 1;
                        for (; j < dtTags.Rows.Count; j++)
                        {
                            if (nTagId != Utils.GetIntSafeVal(dtTags.Rows[j],"tag_type_id"))
                                break;
                            oTags.m_lValues.Add(Utils.GetStrSafeVal(dtTags.Rows[j],"value"));
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
                _logger.Error(ex.Message, ex);
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
                Metas oMeta = new Metas();
                List<Metas> lMetas = new List<Metas>();
                string sFieldName;
                string sFieldVal;
                string sName;
                for (int i = 1; i < 20; i++)
                {
                    sFieldName = "META" + i.ToString() + "_STR_NAME";
                    sFieldVal = "META" + i.ToString() + "_STR";
                    if (dtMeta.Rows[0][sFieldName] != DBNull.Value)
                    {
                        sName = Utils.GetStrSafeVal(dtMeta.Rows[0],sFieldName);
                        if (!string.IsNullOrEmpty(sName))
                        {
                            if (dtMeta.Rows[0][sFieldVal] != DBNull.Value && !string.IsNullOrEmpty(dtMeta.Rows[0][sFieldVal].ToString()))
                            {
                                oMeta.m_oTagMeta = new TagMeta(sName, typeof(double).ToString());
                                oMeta.m_sValue = Utils.GetStrSafeVal(dtMeta.Rows[0],sFieldVal);
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
                        sName = Utils.GetStrSafeVal(dtMeta.Rows[0],sFieldName);
                        if (!string.IsNullOrEmpty(sName))
                        {
                            if (dtMeta.Rows[0][sFieldVal] != DBNull.Value && !string.IsNullOrEmpty(dtMeta.Rows[0][sFieldVal].ToString()))
                            {
                                oMeta.m_oTagMeta = new TagMeta(sName, typeof(double).ToString());
                                oMeta.m_sValue = Utils.GetStrSafeVal(dtMeta.Rows[0],sFieldVal);
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
                                oMeta.m_sValue = Utils.GetStrSafeVal(dtMeta.Rows[0],sFieldVal);
                                lMetas.Add(oMeta);
                            }
                        }
                    }
                    oMeta = new Metas();
                }
                return lMetas;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                result = false;
                return null;
            }
        }

        /*Insert all files that return from the "CompleteDetailsForMediaResponse" into List<FileMedia>*/
        private static List<FileMedia> FilesValues(DataTable dtFileMedia, ref List<Branding> lBranding, bool noFileUrl, int nMedia,ref bool result)
        {
            try
            {
                List<FileMedia> lFileMedia = new List<FileMedia>();
                FileMedia fileMedia = new FileMedia();
                Branding brand = new Branding();
                result = true;
                if (dtFileMedia != null)
                {
                    for (int i = 0; i < dtFileMedia.Rows.Count; i++)
                    {
                        if ((!string.IsNullOrEmpty(Utils.GetStrSafeVal(dtFileMedia.Rows[i], "BRAND_HEIGHT")) && dtFileMedia.Rows[i]["BRAND_HEIGHT"].ToString() != "0")||
                             !string.IsNullOrEmpty(Utils.GetStrSafeVal(dtFileMedia.Rows[i],"RECURRING_TYPE_ID")) && dtFileMedia.Rows[i]["RECURRING_TYPE_ID"].ToString() != "0")                          
                        {
                            brand.m_nFileId          = Utils.GetIntSafeVal(dtFileMedia.Rows[i],"id");
                            brand.m_nDuration        = Utils.GetIntSafeVal(dtFileMedia.Rows[i],"duration");
                            brand.m_sFileFormat      = Utils.GetStrSafeVal(dtFileMedia.Rows[i],"foramtDescription");
                            brand.m_sUrl             = Utils.GetStrSafeVal(dtFileMedia.Rows[i],"FileURL");
                            brand.m_nBrandHeight     = Utils.GetIntSafeVal(dtFileMedia.Rows[i],"BRAND_HEIGHT");
                            brand.m_nRecurringTypeId = Utils.GetIntSafeVal(dtFileMedia.Rows[i],"RECURRING_TYPE_ID");
                            brand.m_sBillingType     = Utils.GetStrSafeVal(dtFileMedia.Rows[i],"bill_type"); 
                            brand.m_nCdnID           = Utils.GetIntSafeVal(dtFileMedia.Rows[i],"CdnID");
                            lBranding.Add(brand);
                            brand = new Branding();
                        }
                        else
                        {
                            int tempAdProvID = 0;
                            fileMedia.m_nFileId            = Utils.GetIntSafeVal(dtFileMedia.Rows[i],"id");
                            fileMedia.m_nDuration          = Utils.GetIntSafeVal(dtFileMedia.Rows[i],"duration");
                            fileMedia.m_sFileFormat        = Utils.GetStrSafeVal(dtFileMedia.Rows[i],"foramtDescription");
                            fileMedia.m_sCoGUID            = Utils.GetStrSafeVal(dtFileMedia.Rows[i], "co_guid");
                            fileMedia.m_sLanguage          = Utils.GetStrSafeVal(dtFileMedia.Rows[i], "LANGUAGE");
                            fileMedia.m_nIsDefaultLanguage = Utils.GetIntSafeVal(dtFileMedia.Rows[i], "IS_DEFAULT_LANGUAGE");

                            if (noFileUrl)
                            {
                                fileMedia.m_sUrl = string.Format("{0}||{1}", nMedia, fileMedia.m_nFileId);
                            }
                            else
                            {
                                fileMedia.m_sUrl = Utils.GetStrSafeVal(dtFileMedia.Rows[i], "FileURL");
                            }

                            fileMedia.m_sBillingType = Utils.GetStrSafeVal(dtFileMedia.Rows[i],"bill_type");
                            fileMedia.m_nCdnID = Utils.GetIntSafeVal(dtFileMedia.Rows[i],"CdnID");
                            tempAdProvID = Utils.GetIntSafeVal(dtFileMedia.Rows[i], "COMMERCIAL_TYPE_PRE_ID");
                            if (tempAdProvID != 0)
                            {
                                fileMedia.m_oPreProvider = new AdProvider();
                                fileMedia.m_oPreProvider.ProviderID   = tempAdProvID;
                                fileMedia.m_oPreProvider.ProviderName = Utils.GetStrSafeVal(dtFileMedia.Rows[i], "COMMERCIAL_TYPE_PRE_NAME");
                                fileMedia.m_bIsPreSkipEnabled         = Utils.GetIntSafeVal(dtFileMedia.Rows[i], "OUTER_COMMERCIAL_SKIP_PRE") == 1;

                            }
                            tempAdProvID = Utils.GetIntSafeVal(dtFileMedia.Rows[i], "COMMERCIAL_TYPE_POST_ID");
                            if (tempAdProvID != 0)
                            {
                                fileMedia.m_oPostProvider = new AdProvider();
                                fileMedia.m_oPostProvider.ProviderID   = tempAdProvID;
                                fileMedia.m_oPostProvider.ProviderName = Utils.GetStrSafeVal(dtFileMedia.Rows[i], "COMMERCIAL_TYPE_POST_NAME");
                                fileMedia.m_bIsPostSkipEnabled         = Utils.GetIntSafeVal(dtFileMedia.Rows[i], "OUTER_COMMERCIAL_SKIP_POST") == 1;
                            }
                            tempAdProvID = Utils.GetIntSafeVal(dtFileMedia.Rows[i], "COMMERCIAL_TYPE_BREAK_ID");
                            if (tempAdProvID != 0)
                            {
                                fileMedia.m_oBreakProvider = new AdProvider();
                                fileMedia.m_oBreakProvider.ProviderID   = tempAdProvID;
                                fileMedia.m_oBreakProvider.ProviderName = Utils.GetStrSafeVal(dtFileMedia.Rows[i], "COMMERCIAL_TYPE_BREAK_NAME");
                                fileMedia.m_sBreakpoints                = Utils.GetStrSafeVal(dtFileMedia.Rows[i], "COMMERCIAL_BREAK_POINTS");

                            }
                            tempAdProvID = Utils.GetIntSafeVal(dtFileMedia.Rows[i], "COMMERCIAL_TYPE_OVERLAY_ID");
                            if (tempAdProvID != 0)
                            {
                                fileMedia.m_oOverlayProvider = new AdProvider();
                                fileMedia.m_oOverlayProvider.ProviderID   = tempAdProvID;
                                fileMedia.m_oOverlayProvider.ProviderName = Utils.GetStrSafeVal(dtFileMedia.Rows[i], "COMMERCIAL_TYPE_OVERLAY_NAME");
                                fileMedia.m_sOverlaypoints                = Utils.GetStrSafeVal(dtFileMedia.Rows[i], "COMMERCIAL_OVERLAY_POINTS");
                            }
                            lFileMedia.Add(fileMedia);
                            fileMedia = new FileMedia();
                        }
                    }
                }
                return lFileMedia;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                result = false;
                return null;
            }
        }

        /*Insert all Pictures that return from the "CompleteDetailsForMediaResponse" into List<Picture>*/
        private static List<Picture> GetAllPic(DataTable dtPic, ref bool result)
        {
            try
            {
                result = true;
                List<Picture> lPicObject = new List<Picture>();
                Picture picObj = new Picture();

                if (dtPic != null)
                {
                    for (int i = 0; i < dtPic.Rows.Count; i++)
                    {
                        picObj.m_sSize = Utils.GetStrSafeVal(dtPic.Rows[i],"PicSize");
                        picObj.m_sURL = Utils.GetStrSafeVal(dtPic.Rows[i],"m_sURL");
                        lPicObject.Add(picObj);
                        picObj = new Picture();
                    }
                }
                return lPicObject;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                result = false;
                return null;
            }
        }

        /*Insert all Basic Details about media  that return from the "CompleteDetailsForMediaResponse" into MediaObj*/
        private static bool GetMediaBasicDetails(ref MediaObj oMediaObj, DataTable dtMedia,DataTable dtUpdateDate, bool bIsMainLang)
        {
            bool result = false;
            try
            {
                if (dtMedia.Columns != null)
                {
                    if (dtMedia.Rows.Count != 0)
                    {
                        result = true;
                        oMediaObj.m_nID = Utils.GetIntSafeVal(dtMedia.Rows[0], "ID");
                        if (!bIsMainLang)
                        {
                            oMediaObj.m_sName = Utils.GetStrSafeVal(dtMedia.Rows[0],"TranslateName");
                            oMediaObj.m_sDescription = Utils.GetStrSafeVal(dtMedia.Rows[0],"TranslateDescription");
                        }
                        else
                        {
                            oMediaObj.m_sName = Utils.GetStrSafeVal(dtMedia.Rows[0],"NAME");
                            oMediaObj.m_sDescription = Utils.GetStrSafeVal(dtMedia.Rows[0],"DESCRIPTION");
                        }
                        oMediaObj.m_oMediaType = new MediaType();
                        oMediaObj.m_oMediaType.m_nTypeID = Utils.GetIntSafeVal(dtMedia.Rows[0],"MEDIA_TYPE_ID");
                        oMediaObj.m_oMediaType.m_sTypeName = Utils.GetStrSafeVal(dtMedia.Rows[0],"typeDescription");
                        oMediaObj.m_nLikeCounter = Utils.GetIntSafeVal(dtMedia.Rows[0],"like_counter");

                        string sEpgIdentifier = Utils.GetStrSafeVal(dtMedia.Rows[0], "EPG_IDENTIFIER");
                        if (!string.IsNullOrEmpty(sEpgIdentifier))
                        {
                            oMediaObj.m_ExternalIDs = sEpgIdentifier;
                        }
                        //Rating
                        oMediaObj.m_oRatingMedia = new RatingMedia();
                        oMediaObj.m_oRatingMedia.m_nViwes = Utils.GetIntSafeVal(dtMedia.Rows[0],"Viwes");
                        oMediaObj.m_oRatingMedia.m_nRatingSum = Utils.GetIntSafeVal(dtMedia.Rows[0],"RatingSum");
                        oMediaObj.m_oRatingMedia.m_nRatingCount = Utils.GetIntSafeVal(dtMedia.Rows[0],"RatingCount");

                        if (oMediaObj.m_oRatingMedia.m_nRatingCount > 0)
                        {
                            oMediaObj.m_oRatingMedia.m_nRatingAvg = (double)((double)oMediaObj.m_oRatingMedia.m_nRatingSum / (double)oMediaObj.m_oRatingMedia.m_nRatingCount);
                        }
                        oMediaObj.m_oRatingMedia.m_nVotesLoCnt = Utils.GetIntSafeVal(dtMedia.Rows[0],"VotesLoCnt");                        
                        oMediaObj.m_oRatingMedia.m_nVotesUpCnt = Utils.GetIntSafeVal(dtMedia.Rows[0],"VotesUpCnt");
                        oMediaObj.m_oRatingMedia.m_nVote1Count = Utils.GetIntSafeVal(dtMedia.Rows[0],"VOTES_1_COUNT");
                        oMediaObj.m_oRatingMedia.m_nVote2Count = Utils.GetIntSafeVal(dtMedia.Rows[0],"VOTES_2_COUNT");
                        oMediaObj.m_oRatingMedia.m_nVote3Count = Utils.GetIntSafeVal(dtMedia.Rows[0],"VOTES_3_COUNT");
                        oMediaObj.m_oRatingMedia.m_nVote4Count = Utils.GetIntSafeVal(dtMedia.Rows[0],"VOTES_4_COUNT");
                        oMediaObj.m_oRatingMedia.m_nVote5Count = Utils.GetIntSafeVal(dtMedia.Rows[0],"VOTES_5_COUNT");

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
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
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

                isLucene = searcher != null && searcher is LuceneWrapper;

                if (searcher != null)
                {
                    SearchResultsObj resultObj = searcher.SearchMedias(oMediaRequest.m_nGroupID, search, 0, oMediaRequest.m_oFilter.m_bUseStartDate);

                    if (resultObj != null)
                    {
                        lSearchResults = resultObj.m_resultIDs;
                        nTotalItems = resultObj.n_TotalItems;
                    }

                }
            }

            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
            #endregion

            return lSearchResults;          
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
                    FullSearchAddParams((MediaSearchFullRequest)request, ref m_dAnd, ref m_dOr);
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
                    foreach (int mediaType in request.m_nMediaTypes)
                    {
                        searchObj.m_sMediaTypes += mediaType.ToString() + ";";
                    }
                    searchObj.m_sMediaTypes = searchObj.m_sMediaTypes.Remove(searchObj.m_sMediaTypes.Length - 1, 1);
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
            }
            catch (Exception ex)
            {
                _logger.Error("Filed Build Search Object For Searcher Search", ex);
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

        /*Build Full search object*/
        static internal void FullSearchAddParams(MediaSearchFullRequest request, ref List<SearchValue> m_dAnd, ref List<SearchValue> m_dOr)
        {
            if (request.m_AndList != null)
            {
                foreach (KeyValue andKeyValue in request.m_AndList)
                {
                    SearchValue search = new SearchValue();
                    search.m_sKey   = andKeyValue.m_sKey;
                    search.m_lValue = new List<string> { andKeyValue.m_sValue };
                    search.m_sValue = andKeyValue.m_sValue;
                    m_dAnd.Add(search);
                }
            }

            if (request.m_OrList != null)
            {
                foreach (KeyValue orKeyValue in request.m_OrList)
                {
                    SearchValue search = new SearchValue();
                    search.m_sKey   = orKeyValue.m_sKey;
                    search.m_lValue = new List<string> { orKeyValue.m_sValue };
                    search.m_sValue = orKeyValue.m_sValue;
                    m_dOr.Add(search);
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
                            search          = new SearchValue();
                            search.m_sKey   = meta.m_sKey;
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
                    search          = new SearchValue();
                    search.m_sKey   = tags.m_sKey;
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
                _logger.Error("Catalog.GetOrderValues", ex);
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
                _logger.Error("SearchObjectString", ex);
            }
        }
        #endregion

        #region Build search Object for search Related
       
        /*Build the right MediaSearchRequest for a Search Related Media */
        public static MediaSearchRequest BuildMediasRequest(Int32 nMediaID, bool bIsMainLang, Filter filterRequest, ref Filter oFilter, Int32 nGroupID, List<Int32> nMediaTypes)
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

                DataSet ds = CatalogDAL.Build_MediaRelated(nGroupID, nMediaID, nLanguage);
                
                if (ds == null)
                    return null;
                oMediasRequest.m_nGroupID = nGroupID;
                if (ds.Tables.Count == 4)
                {
                    if (ds.Tables[1] != null) // basic details
                    {
                        oMediasRequest.m_sName = Utils.GetStrSafeVal(ds.Tables[1].Rows[0],"NAME");
                        oMediasRequest.m_nMediaTypes = new List<int>();
                        if (nMediaTypes == null || nMediaTypes.Count == 0)
                        {
                            if (ds.Tables[1].Rows[0]["MEDIA_TYPE_ID"] != DBNull.Value && !string.IsNullOrEmpty(ds.Tables[1].Rows[0]["MEDIA_TYPE_ID"].ToString()))
                                oMediasRequest.m_nMediaTypes.Add(Utils.GetIntSafeVal(ds.Tables[1].Rows[0],"MEDIA_TYPE_ID"));
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
                _logger.Error(ex.Message, ex);
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
                    sKey = Utils.GetStrSafeVal(dtTags.Rows[i],"NAME");
                    nTagID = Utils.GetIntSafeVal(dtTags.Rows[i],"tagID");
                    if (bIsMainLang)
                        sValue = Utils.GetStrSafeVal(dtTags.Rows[i],"tagValue");
                    else
                        sValue = Utils.GetStrSafeVal(dtTags.Rows[i],"tagTranslate");
                    lTags.Add(new KeyValue(sKey, sValue));
                }
                return lTags;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
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
                _logger.Error(ex.Message, ex);
                return null;
            }
        }
        #endregion

        #region Media play processing (media mrak, media hit)

        public static void GetMediaPlayData(int nMediaID, int nMediaFileID, ref int nOwnerGroupID, ref int nCDNID, ref int nQualityID, ref int nFormatID, ref int nBillingTypeID)
        {
            DataTable dtPlayData = CatalogDAL.Get_MediaPlayData(nMediaID, nMediaFileID);
            if (dtPlayData != null && dtPlayData.Rows.Count > 0)
            {
                nOwnerGroupID = Utils.GetIntSafeVal(dtPlayData.Rows[0], "group_id");
                nCDNID = Utils.GetIntSafeVal(dtPlayData.Rows[0], "streaming_suplier_id");
                nQualityID = Utils.GetIntSafeVal(dtPlayData.Rows[0], "media_quality_id");
                nFormatID = Utils.GetIntSafeVal(dtPlayData.Rows[0], "media_type_id");
                nBillingTypeID = Utils.GetIntSafeVal(dtPlayData.Rows[0], "billing_type_id");
            }
        }

        public static string GetLastPlayCycleKey(string sSiteGuid, int nMediaID, int nMediaFileID, string sUDID, int nGroupID, int nPlatform, int nCountryID)
        {
            string retVal = string.Empty;
            retVal = CatalogDAL.Get_LastPlayCycleKey(sSiteGuid, nMediaID, nMediaFileID, sUDID, nPlatform);

            if (string.IsNullOrEmpty(retVal))
            {
                retVal = Guid.NewGuid().ToString();
                CatalogDAL.Insert_NewPlayCycleKey(nGroupID, nMediaID, nMediaFileID, sSiteGuid, nPlatform, sUDID, nCountryID, retVal);
            }

            return retVal;
        }

        public static void UpdateFollowMe(int nGroupID, int nMediaID, string sSiteGUID, int nPlayTime, string sUDID)
        {
            if (string.IsNullOrEmpty(sSiteGUID) || nMediaID == 0)
            {
                return;
            }

            int nID = 0;
            DateTime dNow = DateTime.Now;

            bool isPC = sUDID.Contains("PC||") ? true : false;
            DataTable dt = CatalogDAL.Get_UserMediaMark(nGroupID, nMediaID, sSiteGUID, isPC, sUDID);

            if (dt != null)
            {
                Int32 nCount = dt.Rows.Count;
                if (nCount > 0)
                {
                    nID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "id");
                    dNow = ODBCWrapper.Utils.GetDateSafeVal(dt.Rows[0], "dNow");
                }
            }

            int nUpdateOrInsert = (nID == 0 ? 1 : 2);  // 1-insert , 2-update
            int nSiteGuid = 0;
            bool resultParse = int.TryParse(sSiteGUID, out nSiteGuid);
            CatalogDAL.UpdateOrInsert_UsersMediaMark(nID, nSiteGuid, sUDID, nMediaID, nGroupID, nPlayTime, nUpdateOrInsert);
        }

        public static int GetCountryIDByIP(string sIP)
        {
            int retCountryID = 0;
            long nIPVal = 0;
            string[] splited = sIP.Split('.');

            if (splited != null && splited.Length >= 3)
            {
                nIPVal = long.Parse(splited[3]) + Int64.Parse(splited[2]) * 256 + Int64.Parse(splited[1]) * 256 * 256 + Int64.Parse(splited[0]) * 256 * 256 * 256;
            }

            DataTable dtCountry = ApiDAL.Get_IPCountryCode(nIPVal);
            if (dtCountry != null && dtCountry.Rows.Count > 0)
            {
                retCountryID = Utils.GetIntSafeVal(dtCountry.Rows[0], "Country_ID");
            }
            return retCountryID;
        }

        public static int GetMediaActionID(string sAction)
        {
            int retActionID = 0;

            DataTable dtAction = CatalogDAL.Get_ActionValues(sAction);
            if (dtAction != null && dtAction.Rows.Count > 0)
            {
                retActionID = Utils.GetIntSafeVal(dtAction.Rows[0], "ID");
            }
            return retActionID;
        }

        public static string GetMediaPlayResponse(MediaPlayResponse response)
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

        internal static MediaSearchObj BuildBaseChannelSearchObject(Channel channel, BaseRequest request, OrderObj orderObj, int nParentGroupID, List<string> lPermittedWatchRules, int[] nDeviceRuleId)
        {
            MediaSearchObj searchObject = new MediaSearchObj();
            searchObject.m_nGroupId = channel.m_nGroupID;
            searchObject.m_nPageIndex = request.m_nPageIndex;
            searchObject.m_nPageSize = request.m_nPageSize;
            searchObject.m_bExact = true;
            searchObject.m_eCutWith = channel.m_eCutWith;
            searchObject.m_sMediaTypes = channel.m_nMediaType.ToString();
            if (!(lPermittedWatchRules == null) && lPermittedWatchRules.Count > 0)
                searchObject.m_sPermittedWatchRules = string.Join(" ", lPermittedWatchRules);
            searchObject.m_nDeviceRuleId = nDeviceRuleId;
            searchObject.m_nIndexGroupId = nParentGroupID;

            ApiObjects.SearchObjects.OrderObj oSearcherOrderObj = new ApiObjects.SearchObjects.OrderObj();
            if (orderObj != null && orderObj.m_eOrderBy != ApiObjects.SearchObjects.OrderBy.NONE)
                GetOrderValues(ref oSearcherOrderObj, orderObj);
	        else
                GetOrderValues(ref oSearcherOrderObj, channel.m_OrderObject);

            searchObject.m_oOrder = oSearcherOrderObj;

            if (request.m_oFilter != null)
            {
                searchObject.m_bUseStartDate = request.m_oFilter.m_bUseStartDate;
                searchObject.m_bUseFinalEndDate = request.m_oFilter.m_bUseFinalDate;
                searchObject.m_nUserTypeID = request.m_oFilter.m_nUserTypeID;

            }
            CopySearchValuesToSearchObjects(ref searchObject, channel.m_eCutWith, channel.m_lChannelTags);          
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

                searchObject.m_lFilterTagsAndMetas = ConvertKeyValuePairsToSearchValues(request.m_lFilterTags);
            }
        }

        private static List<SearchValue> ConvertKeyValuePairsToSearchValues(List<KeyValue> keyValues)
        {
            List<SearchValue> returnedSearchValues = null;

            if (keyValues != null && keyValues.Count > 0)
            {
                returnedSearchValues = new List<SearchValue>();
                foreach (KeyValue keyValue in keyValues)
                {
                    if (!string.IsNullOrEmpty(keyValue.m_sKey) && !string.IsNullOrEmpty(keyValue.m_sValue))
                    {
                        SearchValue searchVal = new SearchValue();
                        searchVal.m_sKey = keyValue.m_sKey;
                        searchVal.m_lValue = new List<string> { keyValue.m_sValue };
                        returnedSearchValues.Add(searchVal);
                    }
                }
            }

            return returnedSearchValues;        
        }

        private static string GetPermittedWatchRules(int nGroupId, DataTable extractedPermittedWatchRulesDT)
        {
            DataTable permittedWathRulesDt = null;
            if (extractedPermittedWatchRulesDT == null)
                permittedWathRulesDt = Tvinci.Core.DAL.CatalogDAL.GetPermittedWatchRulesByGroupId(nGroupId);
            else
                permittedWathRulesDt = extractedPermittedWatchRulesDT;
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

        private static string GetPermittedWatchRules(int nGroupId)
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
                foreach (SearchValue searchValue in channelSearchValues)
                {
                    if (!string.IsNullOrEmpty(searchValue.m_sKey))
                    {
                        search = new SearchValue();
                        search.m_sKey = searchValue.m_sKey;
                        search.m_lValue = searchValue.m_lValue;
                        search.m_sKeyPrefix = searchValue.m_sKeyPrefix;
                        search.m_eInnerCutWith = searchValue.m_eInnerCutWith;

                        switch (cutWith)
                        {
                            case CutWith.OR:
                                {
                                    m_dOr.Add(search);
                                    break;
                                }
                            case CutWith.AND:
                                {
                                    m_dAnd.Add(search);
                                    break;
                                }
                            default:
                                break;
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

        internal static List<int> GetSubscriptionChannelIds(int nGroupId, int nSubscriptionId)
        {
            DataTable channelIdsDt = Tvinci.Core.DAL.CatalogDAL.Get_ChannelsBySubscription(nGroupId, nSubscriptionId);
            List<int> lChannelIds = null;
            if (channelIdsDt != null && channelIdsDt.Rows.Count > 0)
            {
                lChannelIds = new List<int>();
                foreach (DataRow permittedWatchRuleRow in channelIdsDt.Rows)
                {
                    lChannelIds.Add(Utils.GetIntSafeVal(permittedWatchRuleRow, "ID"));
                }
            }

            return lChannelIds;
        }

        #region UPDATE
        public static bool UpdateIndex(List<int> lMediaIds, int nGroupId, eAction eAction)
        {
            return Update(lMediaIds, nGroupId, eObjectType.Media, eAction, eObjectType.Media);
        }

        public static bool UpdateChannelIndex(List<int> lChannelIds, int nGroupId, eAction eAction)
        {
            return Update(lChannelIds, nGroupId, eObjectType.Channel, eAction, eObjectType.Channel);
        }        
        
        private static bool Update(List<int> lIds, int nGroupId, eObjectType eUpdatedObjectType, eAction eAction, eObjectType eObjectType)
        {
            bool bIsUpdateIndexSucceeded = false;

            if (lIds != null && lIds.Count > 0)
            {
                Group group = GroupsCache.Instance.GetGroup(nGroupId);

                if (group != null)
                {
                    ApiObjects.MediaIndexingObjects.IndexingData data = new ApiObjects.MediaIndexingObjects.IndexingData(lIds, group.m_nParentGroupID, eUpdatedObjectType, eAction);

                    if (data != null)
                    {
                        BaseQueue queue = new CatalogQueue();
                        bIsUpdateIndexSucceeded = queue.Enqueue(data, string.Format(@"{0}\{1}", group.m_nParentGroupID, eObjectType.ToString()));
                    }
                }
            }

            return bIsUpdateIndexSucceeded;
        }
        #endregion 

        internal static SearchResultsObj GetProgramIdsFromSearcher(EpgSearchRequest request, ref bool isLucene)
        {
            try
            {
                SearchResultsObj epgReponse = null;
                if (request == null || string.IsNullOrEmpty(request.m_sSearch) || request.m_nGroupID == 0)
                    throw new Exception("request object null or miss 'must' parameters ");

                ISearcher searcher = Bootstrapper.GetInstance<ISearcher>();

                if (searcher == null)
                {
                    _logger.ErrorFormat("Unable to get searcher (ISearcher) instance");
                    return epgReponse;
                }

                #region SearchPrograms
                _logger.InfoFormat("Build Epg Search Object And Call Search Epgs at searching service. groupID={0}", request.m_nGroupID);
                try
                {
                    isLucene = searcher is LuceneWrapper;
                    EpgSearchObj epgSearch = null;
                    List<List<string>> jsonizedChannelsDefinitions = null;
                    if (IsUseIPNOFiltering(request, ref searcher, ref jsonizedChannelsDefinitions))
                    {
                        Dictionary<string, string> dict = GetLinearMediaTypeIDsAndWatchRuleIDs(request.m_nGroupID);
                        MediaSearchObj linearChannelMediaIDsRequest = BuildLinearChannelsMediaIDsRequest(request.m_nGroupID,
                            dict, jsonizedChannelsDefinitions);
                        SearchResultsObj searcherAnswer = searcher.SearchMedias(request.m_nGroupID, linearChannelMediaIDsRequest, 0, true);
                        List<long> ipnoEPGChannelsMediaIDs = ExtractMediaIDs(searcherAnswer);
                        List<long> epgChannelsIDs = GetEPGChannelsIDs(ipnoEPGChannelsMediaIDs);
                        request.m_oEPGChannelIDs = epgChannelsIDs;
                        epgSearch = BuildEpgSearchObject(request, isLucene);
                    }
                    else
                    {
                        epgSearch = BuildEpgSearchObject(request, isLucene);
                    }


                    if (epgSearch != null)
                        epgReponse = searcher.SearchEpgs(epgSearch);


                    return epgReponse;
                }

                catch (Exception ex)
                {
                    _logger.ErrorFormat("GetProgramIdsFromSearcher ex={0}", ex.Message);
                    epgReponse = null;
                }
                #endregion

                return epgReponse;

            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("GetProgramIdsFromSearcher ex={0}", ex.Message);
                return null;
            }
        }

        private static EpgSearchObj BuildEpgSearchObject(EpgSearchRequest request, bool bWhiteSpace)
        {
            try
            {
                List<string> lSearchList = new List<string>();
                EpgSearchObj searcherEpgSearch = new EpgSearchObj();
                _logger.InfoFormat("BuildEpgSearchObject groupID = {0},search = {1}, dates {2}-{3} ", request.m_nGroupID, request.m_sSearch, request.m_dStartDate, request.m_dEndDate);

                searcherEpgSearch.m_bSearchAnd = false; //Search by OR 
                searcherEpgSearch.m_bDesc = true;
                searcherEpgSearch.m_sOrderBy = "start_date";
                searcherEpgSearch.m_dEndDate = request.m_dEndDate;
                searcherEpgSearch.m_dStartDate = request.m_dStartDate;
                List<EpgSearchValue> esvList = new List<EpgSearchValue>();
                string sVal = string.Empty;

                //Get all tags and meta for group
                GetGroupsTagsAndMetas(request.m_nGroupID, ref lSearchList);
                if (lSearchList == null)
                    return null;
                foreach (string item in lSearchList)
                {
                    if (bWhiteSpace)
                        sVal = request.m_sSearch.Replace(' ', '_');
                    else
                        sVal = request.m_sSearch;
                    esvList.Add(new EpgSearchValue(item, sVal));
                }

                searcherEpgSearch.m_lSearch = esvList;
                // get parent group by request.m_nGroupID
                Group group = GroupsCache.Instance.GetGroup(request.m_nGroupID);
                if (group != null)
                    searcherEpgSearch.m_nGroupID = group.m_nParentGroupID;

                searcherEpgSearch.m_nPageIndex = request.m_nPageIndex;
                searcherEpgSearch.m_nPageSize = request.m_nPageSize;

                searcherEpgSearch.m_oEpgChannelIDs = request.m_oEPGChannelIDs;

                return searcherEpgSearch;
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("BuildEpgSearchObject failed ex = {0}", ex.Message); //TO DO what to print from request ???
                return null;
            }
        }

        private static void GetGroupsTagsAndMetas(int nGroupID,ref  List<string> lSearchList)
        {
            try
            {          
                DataSet ds = EpgDal.Get_GroupsTagsAndMetas(nGroupID);

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
            catch (Exception ex)
            {
                lSearchList = null;
            }
        }


        internal static bool CompleteDetailsForProgramResponse(EpgProgramDetailsRequest pRequest, ref EpgProgramResponse pResponse)
        {            
            ProgramObj oProgramObj = null;
            List<BaseObject> lProgramObj = new List<BaseObject>();

            try
            {
                int nStartIndex = pRequest.m_nPageIndex * pRequest.m_nPageSize;
                int nEndIndex = pRequest.m_nPageIndex * pRequest.m_nPageSize + pRequest.m_nPageSize;

                if (nStartIndex == 0 && nEndIndex == 0 && pRequest.m_lProgramsIds != null && pRequest.m_lProgramsIds.Count > 0)
                    nEndIndex = pRequest.m_lProgramsIds.Count();

                //generate a list with the relevant EPG IDs (according to page size and page index)
                List<int> lEpgIDs = new List<int>();
                for (int i = nStartIndex; i < nEndIndex; i++)
                {
                   lEpgIDs.Add(pRequest.m_lProgramsIds[i]);                    
                }

                BaseEpgBL epgBL = EpgBL.Utils.GetInstance(pRequest.m_nGroupID);
                List<EPGChannelProgrammeObject> lEpgProg = epgBL.GetEpgs(lEpgIDs);               
                EPGChannelProgrammeObject epgProg = null;                              

                //keeping the original order and amount of items (some of the items might return as null)
                if (pRequest.m_lProgramsIds != null && lEpgProg != null)
                {
                    pResponse.m_nTotalItems = lEpgProg.Count;                    
                    foreach (int nProgram in pRequest.m_lProgramsIds)
                    {
                        if (lEpgProg.Exists(x => x.EPG_ID == nProgram)) 
                        {
                            epgProg = lEpgProg.Find(x => x.EPG_ID == nProgram);
                            oProgramObj = new ProgramObj();
                            oProgramObj.m_oProgram = epgProg;
                            oProgramObj.m_nID = (int)epgProg.EPG_ID;                            
                            bool succeedParse = DateTime.TryParse(epgProg.UPDATE_DATE, out oProgramObj.m_dUpdateDate);
                        }
                        else
                        {
                            oProgramObj = null;
                        }

                        lProgramObj.Add(oProgramObj);
                    }
                    pResponse.m_lObj = lProgramObj;
                }

                _logger.Info(string.Format("Finish Complete Details for {0} ProgramIds", nEndIndex));
                return true;
            }

            catch (Exception ex)
            {
                _logger.Error("failed to complete details", ex);
                throw ex;
            }
        }

        private static ProgramObj GetProgramDetails(int nProgramID, EpgProgramDetailsRequest pRequest)
        {
            bool result = true;
            try
            {
                ProgramObj oProgramObj = new ProgramObj();

                // get mediaID and complete all details per media
                _logger.Info(string.Format("ProgramID : {0}", nProgramID));

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
                _logger.Error(ex.Message, ex);
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
                _logger.Error(ex.Message, ex);
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
                _logger.Error(ex.Message, ex);
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
                    oProgramObj.m_oProgram.LIKE_COUNTER = Utils.GetIntSafeVal(dt.Rows[0] ,"like_counter");
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
                _logger.Error(ex.Message, ex);
                return false;
            }
        }

        internal static List<string>  EpgAutoComplete(EpgAutoCompleteRequest request)
        {
            try
            {
                List<string> result = null;
                ISearcher searcher = Bootstrapper.GetInstance<ISearcher>();

                if (searcher == null)
                {
                    _logger.ErrorFormat("Unable to get searcher (ISearcher) instance");
                    return null;
                }

                #region AutoComplete
                _logger.InfoFormat("Build Epg Search Object And Call Search Epgs at serch service, groupID={0}", request.m_nGroupID);
                try
                {
                    bool bWhiteSpace = false;
                    if (searcher is LuceneWrapper)
                    {
                        bWhiteSpace = true;
                    }

                    EpgSearchObj epgSearch = BuildEpgSearchObject(request, bWhiteSpace);

                    if (epgSearch != null)
                        result = searcher.GetEpgAutoCompleteList(epgSearch);
                    if (result != null)
                    {                        
                        return result;
                    }
                    else
                        return null;
                }

                catch (Exception ex)
                {
                    _logger.ErrorFormat("GetProgramIdsFromSearcher ex={0}", ex.Message);
                    result = null;
                }
                #endregion
                return result;
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("GetProgramIdsFromSearcher ex={0}", ex.Message);
                return null;
            }
        }

        private static EpgSearchObj BuildEpgSearchObject(EpgAutoCompleteRequest request, bool bWhiteSpace)
        {
            try
            {
                List<string> lSearchList = new List<string>();
                EpgSearchObj oEpgSearch = new EpgSearchObj();
                _logger.InfoFormat("BuildEpgSearchObject groupID = {0},search = {1}, dates {2}-{3} ", request.m_nGroupID, request.m_sSearch, request.m_dStartDate, request.m_dEndDate);

                oEpgSearch.m_bSearchAnd = false; //search with or
                oEpgSearch.m_bDesc = false;
                oEpgSearch.m_sOrderBy = "name";

                oEpgSearch.m_dEndDate = request.m_dEndDate;
                oEpgSearch.m_dStartDate = request.m_dStartDate;
                List<EpgSearchValue> dEsv = new List<EpgSearchValue>();
                string sVal = string.Empty;
                //Get all tags and meta for group
                GetGroupsTagsAndMetas(request.m_nGroupID, ref lSearchList);
                
                if (lSearchList == null)
                    return null;
                foreach (string item in lSearchList)
                {
                    if (bWhiteSpace)
                        sVal = request.m_sSearch.Replace(' ', '_');
                    else
                        sVal = request.m_sSearch;
                    dEsv.Add(new EpgSearchValue(item, sVal));
                }

                oEpgSearch.m_lSearch = dEsv;
                
                // get parent group by request.m_nGroupID
                int nParentGroup = UtilsDal.GetParentGroupID(request.m_nGroupID);
                oEpgSearch.m_nGroupID = nParentGroup;

                oEpgSearch.m_nPageIndex = request.m_nPageIndex;
                oEpgSearch.m_nPageSize = request.m_nPageSize;

                return oEpgSearch;
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("BuildEpgSearchObject failed ex = {0}", ex.Message); //TO DO what to print from request ???
                return null;
            }
        }

        //returns a list of EpgIDsResultsObj, per channel, each one has a list of epgID and its update date
        //the amount of epgs that are returned, in case of 'current' is limited only by the 'NextTop' and 'PrevTop' paramters inside the request (not by page size and page index), per channel
        public static List<EpgResultsObj> GetEPGPrograms(EpgRequest request)
        {
            using (Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true))
            {
                log.Method = "Catalog.GetEPGProgramIds";
                try
                {
                    if (request == null || request.m_nGroupID == 0)
                    {
                        log.Error("SearchEpgsByParams return null due to epgSearch == null || epgSearch.m_nGroupID==0", false);
                        return null;
                    }

                    List<EpgResultsObj> epgResponse = new List<EpgResultsObj>(); 
                    EpgResultsObj resultPerChannel;
                    BaseEpgBL epgBL = EpgBL.Utils.GetInstance(request.m_nGroupID);
                    Group group = GroupsCache.Instance.GetGroup(request.m_nGroupID);
                    if (group != null)
                    {
                        ConcurrentDictionary<int, List<EPGChannelProgrammeObject>> dicDocs = null;
                        if (request.m_eSearchType == EpgSearchType.ByDate)
                        {
                            //page size and page index of the request effect only per channel (and not the total amount of results) 
                            dicDocs = epgBL.GetMultiChannelProgramsDic(request.m_nPageSize, request.m_nPageIndex, request.m_nChannelIDs, request.m_dStartDate, request.m_dEndDate);
                            log.Message = string.Format("SearchEpgs by params of GroupID={0}, between Dates {1} - {2} in channels {3}",
                                request.m_nGroupID, request.m_dStartDate.ToShortDateString(), request.m_dEndDate.ToShortDateString(), request.m_nChannelIDs.ToString());
                        }
                        else if (request.m_eSearchType == ApiObjects.EpgSearchType.Current)
                        {
                            int nNextTop = 0;
                            int nPrevTop = 0;
                            getTopValues(request, ref nNextTop, ref nPrevTop); //insert default values, if needed                            

                            //all results return, according to m_nPrevTop and m_nNextTop (and not page size and page index)
                            dicDocs = epgBL.GetMultiChannelProgramsDicCurrent(nNextTop, nPrevTop, request.m_nChannelIDs);
                            log.Message = string.Format("SearchEpgs for current epgs by params of GroupID={0}, last {1} and next {2} epgs of channels {3}",
                                request.m_nGroupID, nPrevTop.ToString(), nNextTop.ToString(), request.m_nChannelIDs.ToString());
                        }
                        log.Info(log.Message, false);
                        if (dicDocs != null)
                        {
                            foreach (int channelID in dicDocs.Keys)
                            {
                                List<EPGChannelProgrammeObject> epgList;
                                if (dicDocs.TryGetValue(channelID, out epgList) && epgList != null && epgList.Count > 0)
                                {
                                    //build the response
                                    resultPerChannel = createEpgResults(dicDocs[channelID], channelID);
                                    epgResponse.Add(resultPerChannel);
                                }
                            }
                        }
                        else
                            log.Error("Dictionary of results per channel returned empty, response will by empty.", false);
                    }
                    else
                    {
                        log.Error("returning null because group was not retrieved from the Group Cache ", false);
                        return null;
                    }
                    return epgResponse;
                }
                catch (Exception ex)
                {
                    log.Message = string.Format("SearchEpgsByParams had an exception: ex={0} in {1}", ex.Message, ex.StackTrace);
                    log.Error(log.Message, false);
                    return null;
                }
            }             
        }        

        private static EpgResultsObj createEpgResults(List<EPGChannelProgrammeObject> epgList, int nChannelID)
        {
            EpgResultsObj resultPerChannel = new EpgResultsObj();
            resultPerChannel.m_nChannelID = nChannelID;
            resultPerChannel.m_nTotalItems = epgList.Count();
            resultPerChannel.m_lEpgProgram = epgList;            
            return resultPerChannel;
        }       

        //insert default values to Top Next and Top prev, if they are 0
        private static void getTopValues (EpgRequest request, ref int nNextTop, ref int nPrevTop)
        {
            if (request.m_eSearchType == EpgSearchType.Current)
            {
                int itemAmount;
                bool succeedParse;
                if (request.m_nNextTop == 0) //insert default value, to prevent querying everything
                {
                    succeedParse = int.TryParse(Utils.GetWSURL("EPG_NEXT_TOP_ITEMS"), out itemAmount);
                    if (succeedParse)
                        nNextTop = itemAmount;
                    else
                        nNextTop = 10;
                }
                else
                {
                    nNextTop = request.m_nNextTop;
                }
                if (request.m_nPrevTop == 0)//insert default value, to prevent querying everything
                {
                    succeedParse = int.TryParse(Utils.GetWSURL("EPG_PREV_TOP_ITEMS"), out itemAmount);
                    if (succeedParse)
                        nPrevTop = itemAmount;
                    else
                        nPrevTop = 10;
                }
                else
                {
                    nPrevTop = request.m_nPrevTop;
                }
            }            
        }
        
        public static List<AssetStatsResult> GetAssetStatsResults(int nGroupID, List<int> lAssetIDs, DateTime dStartDate, DateTime dEndDate, StatsType eType)
        {
            using (Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true))
            {
                log.Method = "Catalog.GetMediaStatsResults";
                List<AssetStatsResult> resList = null;
                DataSet ds;
                bool sendLog = false;

                try
                {
                    if (eType == StatsType.MEDIA)
                    {
                        if (dStartDate == DateTime.MinValue && dEndDate == DateTime.MaxValue)
                            ds = CatalogDAL.GetMediasStats(nGroupID, lAssetIDs, null, null);
                        else
                            ds = CatalogDAL.GetMediasStats(nGroupID, lAssetIDs, dStartDate, dEndDate);
                        if (ds != null)
                            resList = getMediaStatFromDataSet(ds, lAssetIDs);
                        else
                            sendLog = true;
                    }
                    else if (eType == StatsType.EPG)
                    {
                        if (dStartDate == DateTime.MinValue && dEndDate == DateTime.MaxValue)
                        {
                            resList = getEpgStatFromBL(nGroupID, lAssetIDs);
                        }
                        else
                        {
                            ds = CatalogDAL.GetEpgStats(nGroupID, lAssetIDs, dStartDate, dEndDate);
                            if (ds != null)
                                resList = getEpgStatFromDataSet(ds, lAssetIDs);
                            else
                                sendLog = true;
                        }
                    }
                    if (sendLog)
                    {
                        log.Message = string.Format("Could not retrieve the media Statistics from the DB - DataSet is empty. the response will be null." +
                            "group ID: {0}, mediaIDs{1}, startTime: {2}, endTime: {3}", nGroupID.ToString(), lAssetIDs.ToString(), dStartDate.ToString(), dEndDate.ToString());
                        log.Error(log.Message, false);
                        return null;
                    }
                }

                catch (Exception ex)
                {
                    log.Message = string.Format("Could not retrieve the media Statistics in Catalog.GetMediaStatsResults from the DB."
                                                + "exception message: {0}, stack: {1}", ex.Message, ex.StackTrace, "Catalog");
                    log.Error(log.Message, false);
                    return null;
                }
                return resList;
            }
        }

        private static List<AssetStatsResult> getEpgStatFromDataSet(DataSet ds, List<int> lAssetIDs)
        {
            List<AssetStatsResult> resList = new List<AssetStatsResult>();
            AssetStatsResult epgStat;
            try
            {                
                if (ds.Tables != null && ds.Tables.Count == 1)
                {
                    //getting only medias that were in the DB
                    if (ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            if (row != null)
                            {
                                epgStat = new AssetStatsResult();
                                epgStat.m_nAssetID = Utils.GetIntSafeVal(row, "ID");
                                epgStat.m_nLikes = Utils.GetIntSafeVal(row, "like_counter");
                                resList.Add(epgStat);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                BaseLog log = new BaseLog(eLogType.WcfRequest, DateTime.UtcNow, true);
                log.Method = "Catalog.getMediaStatFromDataSet";
                log.Message = string.Format("Could not retrieve the media Statistics in Catalog.getMediaStatFromDataSet . exception message: {0}, stack: {1}", ex.Message, ex.StackTrace, "Catalog");
                log.Error(log.Message, false);
                return null;
            }
            return resList;
        }

        private static List<AssetStatsResult> getEpgStatFromBL(int nGroupID, List<int> lAssetIDs)
        {
            List<AssetStatsResult> resList = new List<AssetStatsResult>();
            AssetStatsResult epgStat;
            try
            {               
                BaseEpgBL epgBL = EpgBL.Utils.GetInstance(nGroupID);
                List<EPGChannelProgrammeObject> lEpg = epgBL.GetEpgs(lAssetIDs);
                foreach (EPGChannelProgrammeObject epg in lEpg)
                {
                    if (epg != null)
                    {
                        epgStat = new AssetStatsResult();
                        epgStat.m_nAssetID = (int)epg.EPG_ID;
                        epgStat.m_nLikes = epg.LIKE_COUNTER;
                        resList.Add(epgStat);
                    }
                }
            }
            catch (Exception ex)
            {
                BaseLog log = new BaseLog(eLogType.WcfRequest, DateTime.UtcNow, true);
                log.Method = "Catalog.getMediaStatFromDataSet";
                log.Message = string.Format("Could not retrieve the media Statistics in Catalog.getMediaStatFromDataSet . exception message: {0}, stack: {1}", ex.Message, ex.StackTrace, "Catalog");
                log.Error(log.Message, false);
                return null;
            }
            return resList;
        }

        private static List<AssetStatsResult> getMediaStatFromDataSet(DataSet ds, List<int> mediaIDs)
        {
            using (Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true))
            {
                log.Method = "Catalog.getMediaStatFromDataSet";
                List<AssetStatsResult> resList = new List<AssetStatsResult>();
                AssetStatsResult mediaStat;
                try
                {
                    //if the request was sent without dates, the select is only on 1 table
                    if (ds.Tables != null && ds.Tables.Count == 1)
                    {
                        //getting only medias that were in the DB
                        if (ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                if (row != null)
                                {
                                    mediaStat = new AssetStatsResult();
                                    mediaStat.m_nAssetID = Utils.GetIntSafeVal(row, "ID");
                                    mediaStat.m_nViews = Utils.GetIntSafeVal(row, "VIEWS");
                                    mediaStat.m_nVotes = Utils.GetIntSafeVal(row, "VOTES_COUNT");
                                    int sumVotes = Utils.GetIntSafeVal(row, "VOTES_SUM");
                                    if (mediaStat.m_nVotes != 0)
                                        mediaStat.m_dRate = (double)sumVotes / mediaStat.m_nVotes;
                                    mediaStat.m_nLikes = Utils.GetIntSafeVal(row, "like_counter");
                                    resList.Add(mediaStat);
                                }
                            }
                        }
                    }
                    //if the request was sent with dates, 4 tables will return from the DB
                    else if (ds.Tables != null && ds.Tables.Count == 4)
                    {
                        Dictionary<int, AssetStatsResult> resultDic = new Dictionary<int, AssetStatsResult>();
                        foreach (int id in mediaIDs)
                            resultDic.Add(id, new AssetStatsResult());
                        //retrieving only medias that were in the DB
                        if (ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                if (row != null)
                                {
                                    int id = Utils.GetIntSafeVal(row, "ID");
                                    resultDic[id].m_nAssetID = id;
                                }
                            }
                        }
                        //retrieving the relevant views
                        if (ds.Tables[1] != null && ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count > 0)
                        {
                            foreach (DataRow row in ds.Tables[1].Rows)
                            {
                                if (row != null)
                                {
                                    int id = Utils.GetIntSafeVal(row, "MEDIA_ID");
                                    int views = Utils.GetIntSafeVal(row, "VIEWS");
                                    resultDic[id].m_nViews = views;
                                }
                            }
                        }
                        //retrieving the relevant Rate and Vote count
                        if (ds.Tables[2] != null && ds.Tables[2].Rows != null && ds.Tables[2].Rows.Count > 0)
                        {
                            foreach (DataRow row in ds.Tables[2].Rows)
                            {
                                if (row != null)
                                {
                                    int id = Utils.GetIntSafeVal(row, "MEDIA_ID");
                                    int votesCount = Utils.GetIntSafeVal(row, "VOTES_COUNT");
                                    resultDic[id].m_nVotes = votesCount;
                                    int votesSum = Utils.GetIntSafeVal(row, "VOTES_SUM");
                                    if (resultDic[id].m_nVotes != 0)
                                        resultDic[id].m_dRate = (double)votesSum / resultDic[id].m_nVotes;
                                }
                            }
                        }
                        //retrieving the relevant likes
                        if (ds.Tables[3] != null && ds.Tables[3].Rows != null && ds.Tables[3].Rows.Count > 0)
                        {
                            foreach (DataRow row in ds.Tables[3].Rows)
                            {
                                if (row != null)
                                {
                                    int id = Utils.GetIntSafeVal(row, "media_id");
                                    int likes = Utils.GetIntSafeVal(row, "like_counter");
                                    resultDic[id].m_nLikes = likes;
                                }
                            }
                        }
                        resList = resultDic.Values.ToList();
                    }
                    else
                    {
                        log.Message = string.Format("Could not retrieve the media Statistics in Catalog.getMediaStatFromDataSet from the dataSet,"
                                                    + "dataSet is empty or number of retrieved tables is unexpected");
                        log.Error(log.Message, false);
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    log.Message = string.Format("Could not retrieve the media Statistics in Catalog.getMediaStatFromDataSet . exception message: {0}, stack: {1}", ex.Message, ex.StackTrace, "Catalog");
                    log.Error(log.Message, false);
                    return null;
                }

                return resList;
            }
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
                    int nOperatorID = Utils.GetOperatorIDBySiteGuid(oMediaRequest.m_nGroupID, lSiteGuid);
                    if (nOperatorID == 0)
                    {
                        throw new Exception("IPNO Filtering. No operator ID extracted from DB");
                    }
                    else
                    {
                        // we have operator id
                        res = true;
                        List<long> channelsOfIPNO = GroupsCache.Instance.GetOperatorChannelIDs(oMediaRequest.m_nGroupID, nOperatorID);
                        List<long> allChannelsOfAllIPNOs = GroupsCache.Instance.GetDistinctAllOperatorsChannels(oMediaRequest.m_nGroupID);
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

        private static List<long> GetEPGChannelsIDs(List<long> ipnoEPGChannelsMediaIDs)
        {
            List<long> res;
            DataTable dt = CatalogDAL.Get_EPGChannelsIDsByMediaIDs(ipnoEPGChannelsMediaIDs);
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                int length = dt.Rows.Count;
                res = new List<long>(length);
                for (int i = 0; i < length; i++)
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

        private static List<long> ExtractMediaIDs(SearchResultsObj sro)
        {
            int length = sro.n_TotalItems;
            List<long> res = new List<long>(length);
            for (int i = 0; i < length; i++)
            {
                res.Add(sro.m_resultIDs[i].assetID);
            }

            return res;
        }


        private static MediaSearchObj BuildLinearChannelsMediaIDsRequest(int nGroupID, Dictionary<string, string> dict, List<List<string>> jsonizedChannelsDefinitions)
        {
            MediaSearchObj res = new MediaSearchObj();
            res.m_nGroupId = nGroupID;
            res.m_sMediaTypes = dict[LINEAR_MEDIA_TYPES_KEY];
            res.m_sPermittedWatchRules = dict[PERMITTED_WATCH_RULES_KEY];
            res.m_lChannelsDefinitionsMediaNeedsToBeInAtLeastOneOfIt = jsonizedChannelsDefinitions[0];
            res.m_lOrMediaNotInAnyOfTheseChannelsDefinitions = jsonizedChannelsDefinitions[1];
            res.m_nPageIndex = 0;
            res.m_nPageSize = GetSearcherMaxResultsSize();
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

        private static Dictionary<string, string> GetLinearMediaTypeIDsAndWatchRuleIDs(int nGroupID)
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

    }
       
}