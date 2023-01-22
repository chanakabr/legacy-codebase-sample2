using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using CachingProvider.LayeredCache;
using Core.Catalog.CatalogManagement;
using Core.Catalog.Request;
using Core.Catalog.Response;
using DAL;
using GroupsCacheManager;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Tvinci.Core.DAL;
using ApiLogic.IndexManager.Helpers;

namespace Core.Catalog
{
    public class Utils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly HashSet<string> LocalizedReservedGroupByFields = new HashSet<string>
        {
            "name",
            "description"
        };

        private static readonly HashSet<string> ReservedGroupByFields = new HashSet<string>(LocalizedReservedGroupByFields)
        {
            "media_type_id",
            "crid",
            "suppressed",
            "linear_media_id"
        };

        public const int DEFAULT_CATALOG_LOG_THRESHOLD_MILLISEC = 500; // half a second
        public const string EMPTY_ASSET_ID = "0";

        public static string GetSignature(string sSigningString, Int32 nGroupID)
        {
            string retVal;

            string hmacSecret = ApplicationConfiguration.Current.CatalogSignatureKey.Value;

            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

            using (HMACSHA1 myhmacsha1 = new HMACSHA1(encoding.GetBytes(hmacSecret)))
            {
                retVal = System.Convert.ToBase64String(myhmacsha1.ComputeHash(encoding.GetBytes(sSigningString)));
            }
            return retVal;
        }


        public static List<SearchResult> GetMediaForPaging(List<SearchResult> medias, BaseRequest request)
        {
            List<SearchResult> mediaList = new List<SearchResult>();
            try
            {
                if (medias.Count() == 0)
                    return mediaList;

                int startIndex = 0;
                int countItems = medias.Count();

                if (request.m_nPageIndex > 0)
                    startIndex = request.m_nPageIndex * request.m_nPageSize;//first page index = 0

                if (request.m_nPageSize > 0)
                    countItems = request.m_nPageSize;
                else
                    countItems = medias.Count() - startIndex;

                if (medias.Count() < startIndex)
                    return mediaList;

                if ((startIndex + countItems) > medias.Count())
                    countItems = medias.Count() - startIndex;

                mediaList = medias.ToList<SearchResult>().GetRange(startIndex, countItems);

                return mediaList;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return null;
            }
        }

        public static bool GetPagingValues(int nRowCount, int nPageIndex, int nPageSize, ref int startIndex, ref int count)
        {
            try
            {
                if (nRowCount == 0)
                {
                    return false;
                }

                startIndex = 0;
                count = nRowCount;

                if (nPageIndex > 0)
                    startIndex = nPageIndex * nPageSize;//first page index = 0

                if (nPageSize > 0)
                    count = nPageSize;
                else
                    count = nRowCount - startIndex;

                if (nRowCount < startIndex)
                {
                    startIndex = 0;
                    count = nRowCount;
                }


                if ((startIndex + count) > nRowCount)
                    count = nRowCount - startIndex;

                return true;

            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return false;
            }
        }

        public static bool IsLangMain(int nGroupID, int nLanguage)
        {
            bool bIsMain = true;

            if (nLanguage == 0)
                return bIsMain;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select g.LANGUAGE_ID from groups g (nolock) where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.id", "=", nGroupID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        Int32 nMainLangID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["LANGUAGE_ID"].ToString());
                        if (nLanguage == nMainLangID)
                            bIsMain = true;
                        else
                            bIsMain = false;
                    }
                }
            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
            }
            return bIsMain;
        }

        public static string GetStrSafeVal(DataRow dr, string sField)
        {
            try
            {
                if (dr != null && dr[sField] != DBNull.Value)
                    return dr[sField].ToString();
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static Int64 GetLongSafeVal(DataRow dr, string sField)
        {
            try
            {
                if (dr != null && dr[sField] != DBNull.Value)
                {
                    return Convert.ToInt64(dr[sField]);
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public static int GetIntSafeVal(DataRow dr, string sField)
        {
            try
            {
                if (dr != null && dr[sField] != DBNull.Value)
                    return int.Parse(dr[sField].ToString());
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// This function gets the number of items in a collection, a page index and a range. It checks whether the range of items is valid among the
        /// the nNumOfMedias. 
        /// </summary>
        /// <param name="nNumOfMedias">Number of items in a collection</param>
        /// <param name="nPageIndex">The requested page index</param>
        /// <param name="nValidRange">The range of items request within a page index (updated if required)</param>
        /// <returns>True if valid, false if not valid</returns>
        internal static bool ValidatePageSizeAndPageIndexAgainstNumberOfMedias(int nNumOfMedias, int nPageIndex, ref int nValidRange)
        {
            bool bIsValidRange = false;
            if (nValidRange > 0 || nPageIndex > 0)
            {
                int nSizePageIndexMultiplicity = nPageIndex * nValidRange;
                if (nSizePageIndexMultiplicity < nNumOfMedias)
                {
                    if (nNumOfMedias - nSizePageIndexMultiplicity < nValidRange)
                    {
                        nValidRange = nNumOfMedias - nSizePageIndexMultiplicity;
                    }

                    bIsValidRange = true;
                }
            }
            else if (nValidRange == 0 && nPageIndex == 0)   // Returning all items in collection
            {
                bIsValidRange = true;
            }

            return bIsValidRange;
        }

        public static void OrderMediasByStats(List<int> medias, int nOrderType, int nOrderDirection)
        {
            if (!medias.Any())
                return;

            var dt = CatalogDAL.Get_OrderedMediaIdList(medias, nOrderType, nOrderDirection);

            var anyMedias = dt != null && dt.Rows.Count > 0;
            if (!anyMedias)
            {
                return;
            }

            medias.Clear();
            medias.AddRange(dt.AsEnumerable().Select(dr => ODBCWrapper.Utils.GetIntSafeVal(dr["ID"])));

        }

        public static List<SearchResult> GetMediaUpdateDate(int nParentGroupID, List<int> lMediaIDs)
        {
            List<SearchResult> lMediaRes = new List<SearchResult>();

            if (lMediaIDs == null || lMediaIDs.Count == 0)
                return lMediaRes;

            var indexManager = IndexManagerFactory.Instance.GetIndexManager(nParentGroupID);

            var assetsUpdateDates = indexManager.GetAssetsUpdateDate(eObjectType.Media, lMediaIDs);

            Dictionary<int, SearchResult> idToSearchResult = assetsUpdateDates.ToDictionary<SearchResult, int>(item => item.assetID);

            foreach (var mediaId in lMediaIDs)
            {
                lMediaRes.Add(idToSearchResult[mediaId]);
            }

            return lMediaRes;
        }

        public static bool IsGroupIDContainedInConfig(long lGroupID, string rawStrFromConfig, char cSeperator)
        {
            bool res = false;
            if (rawStrFromConfig.Length > 0)
            {
                string[] strArrOfIDs = rawStrFromConfig.Split(cSeperator);
                if (strArrOfIDs != null && strArrOfIDs.Length > 0)
                {
                    List<long> listOfIDs = strArrOfIDs.Select(s =>
                    {
                        long l = 0;
                        if (Int64.TryParse(s, out l))
                            return l;
                        return 0;
                    }).ToList();

                    res = listOfIDs.Contains(lGroupID);
                }
            }

            return res;
        }

        public static int GetOperatorIDBySiteGuid(int nGroupID, long lSiteGuid)
        {
            int res = 0;
            bool bIsDomainMaster = false;
            DomainSuspentionStatus eSuspendStat = DomainSuspentionStatus.OK;
            DomainDal.GetDomainIDBySiteGuid(nGroupID, (int)lSiteGuid, ref res, ref bIsDomainMaster, ref eSuspendStat);

            return res;

        }

        public static string GetESTypeByLanguage(string sType, ApiObjects.LanguageObj oLanguage)
        {
            string sResult;

            if (oLanguage != null && !oLanguage.IsDefault)
            {
                sResult = string.Concat(sType, "_", oLanguage.Code);
            }
            else
            {
                sResult = sType;
            }

            return sResult;
        }

        public static bool KeyInGroupTags(int nGroupID, string sTagType)
        {
            bool bRes = false;

            GroupManager groupManager = new GroupManager();
            Group group = groupManager.GetGroup(nGroupID);

            if (group != null)
            {
                if (group.m_oGroupTags.ContainsValue(sTagType))
                {
                    bRes = true;
                }
            }

            return bRes;
        }

        internal static List<ApiObjects.EPGChannelProgrammeObject> CompleteFullEpgPicURL(List<ApiObjects.EPGChannelProgrammeObject> epgList)
        {
            try
            {
                string sBaseURL = string.Empty;
                string sWidth = string.Empty;
                string sHeight = string.Empty;
                if (epgList != null && epgList.Count > 0 && epgList[0] != null)
                {
                    int groupID = int.Parse(epgList[0].GROUP_ID);
                    DataTable dtPic = Tvinci.Core.DAL.CatalogDAL.GetPicEpgURL(groupID);
                    if (dtPic != null && dtPic.Rows != null && dtPic.Rows.Count > 0)
                    {
                        sBaseURL = ODBCWrapper.Utils.GetSafeStr(dtPic.Rows[0], "baseURL");
                        sWidth = ODBCWrapper.Utils.GetSafeStr(dtPic.Rows[0], "WIDTH");
                        sHeight = ODBCWrapper.Utils.GetSafeStr(dtPic.Rows[0], "HEIGHT");
                        if (sBaseURL.Substring(sBaseURL.Length - 1, 1) != "/")
                        {
                            sBaseURL = string.Format("{0}/", sBaseURL);
                        }
                    }

                    foreach (ApiObjects.EPGChannelProgrammeObject oProgram in epgList)
                    {
                        if (oProgram != null && !string.IsNullOrEmpty(sBaseURL) && !string.IsNullOrEmpty(oProgram.PIC_URL))
                        {
                            if (!string.IsNullOrEmpty(sWidth) && !string.IsNullOrEmpty(sHeight))
                            {
                                oProgram.PIC_URL = oProgram.PIC_URL.Replace(".", string.Format("_{0}X{1}.", sWidth, sHeight));
                            }
                            oProgram.PIC_URL = string.Format("{0}{1}", sBaseURL, oProgram.PIC_URL);
                        }
                    }
                }
                return epgList;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return null;
            }
        }

        public static List<T> ListPaging<T>(List<T> list, int nPageSize, int nPageIndex)
        {
            List<T> result = new List<T>();

            if (list != null && list.Count > 0)
            {
                int skip = nPageIndex * nPageSize;

                if (list.Count > skip)
                {
                    result = (list.Count) > (skip + nPageSize) ? list.Skip(skip).Take(nPageSize).ToList() : list.Skip(skip).ToList();
                }
            }

            return result;
        }

        internal static int GetUserType(string sSiteGuid, int nGroupID)
        {
            int nUserTypeID = 0;
            try
            {
                //get username + password from wsCache
                Credentials oCredentials = TvinciCache.WSCredentials.GetWSCredentials(ApiObjects.eWSModules.CATALOG, nGroupID, ApiObjects.eWSModules.USERS);
                if (oCredentials != null)
                {
                    nUserTypeID = Core.Users.Module.GetUserType(nGroupID, sSiteGuid);
                }

                return nUserTypeID;
            }
            catch (Exception ex)
            {
                log.Error("Exception - " + string.Format("Failed to obtain user type. Site Guid: {0} , Ex Msg: {1} , Stack Trace: {2}", sSiteGuid, ex.Message, ex.StackTrace), ex);
                return 0;
            }
        }

        public static void BuildMediaFromDataSet(ref Dictionary<int, Dictionary<int, Media>> mediaTranslations,
            ref Dictionary<int, Media> medias, Group group, DataSet dataSet, int mediaId)
        {
            if (dataSet != null && dataSet.Tables.Count > 0)
            {
                var suppressesIndexes = Api.api.GetMediaSuppressedIndexes(group.m_nParentGroupID)?.Object;

                if (dataSet.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in dataSet.Tables[0].Rows)
                    {
                        Media media = new Media();
                        if (dataSet.Tables[0].Columns != null && dataSet.Tables[0].Rows != null)
                        {
                            #region media info
                            media.m_nMediaID = ODBCWrapper.Utils.GetIntSafeVal(row, "ID");
                            media.m_nWPTypeID = ODBCWrapper.Utils.GetIntSafeVal(row, "watch_permission_type_id");
                            media.m_nMediaTypeID = ODBCWrapper.Utils.GetIntSafeVal(row, "media_type_id");
                            media.m_nGroupID = ODBCWrapper.Utils.GetIntSafeVal(row, "group_id");
                            media.m_nIsActive = ODBCWrapper.Utils.GetIntSafeVal(row, "is_active");
                            media.m_nDeviceRuleId = ODBCWrapper.Utils.GetIntSafeVal(row, "device_rule_id");
                            media.m_nLikeCounter = ODBCWrapper.Utils.GetIntSafeVal(row, "like_counter");
                            media.m_nViews = ODBCWrapper.Utils.GetIntSafeVal(row, "views");
                            media.m_sUserTypes = ODBCWrapper.Utils.GetSafeStr(row["user_types"]);

                            double dSum = ODBCWrapper.Utils.GetDoubleSafeVal(row, "votes_sum");
                            double dCount = ODBCWrapper.Utils.GetDoubleSafeVal(row, "votes_count");

                            if (dCount > 0)
                            {
                                media.m_nVotes = (int)dCount;
                                media.m_dRating = dSum / dCount;
                            }

                            media.m_sName = ODBCWrapper.Utils.GetSafeStr(row, "name");
                            media.m_sDescription = ODBCWrapper.Utils.GetSafeStr(row, "description");

                            if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row, "create_date")))
                            {
                                DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(row, "create_date");
                                media.m_sCreateDate = dt.ToString("yyyyMMddHHmmss");
                            }
                            if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row, "update_date")))
                            {
                                DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(row, "update_date");
                                media.m_sUpdateDate = dt.ToString("yyyyMMddHHmmss");
                            }
                            if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row, "start_date")))
                            {
                                DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(row, "start_date");
                                media.m_sStartDate = dt.ToString("yyyyMMddHHmmss");
                            }

                            if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row, "end_date")))
                            {
                                DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(row, "end_date");
                                media.m_sEndDate = dt.ToString("yyyyMMddHHmmss");
                            }

                            if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row, "final_end_date")))
                            {
                                DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(row, "final_end_date");
                                media.m_sFinalEndDate = dt.ToString("yyyyMMddHHmmss");
                            }

                            media.geoBlockRule = ODBCWrapper.Utils.ExtractInteger(row, "geo_block_rule_id");

                            string epgIdentifier = ODBCWrapper.Utils.ExtractString(row, "epg_identifier");

                            if (!string.IsNullOrEmpty(epgIdentifier))
                            {
                                media.epgIdentifier = epgIdentifier;
                            }

                            media.inheritancePolicy = ODBCWrapper.Utils.GetNullableInt(row, "INHERITANCE_POLICY");

                            #endregion

                            #region - get all metas by groupId
                            Dictionary<string, string> dMetas;
                            //Get Meta - MetaNames (e.g. will contain key/value <META1_STR, show>)
                            if (group.m_oMetasValuesByGroupId.TryGetValue(media.m_nGroupID, out dMetas))
                            {
                                foreach (string sMeta in dMetas.Keys)
                                {
                                    //Retreive meta name and check that it is not null or empty so that it will not form an invalid field later on
                                    string sMetaName;
                                    dMetas.TryGetValue(sMeta, out sMetaName);

                                    if (!string.IsNullOrEmpty(sMetaName) && sMeta.StartsWith("META"))
                                    {
                                        string sMetaValue = ODBCWrapper.Utils.GetSafeStr(row[sMeta]);

                                        if (!media.m_dMeatsValues.ContainsKey(sMetaName))
                                        {
                                            media.m_dMeatsValues.Add(sMetaName, sMetaValue);
                                        }
                                        else
                                        {
                                            log.WarnFormat("Duplicate meta found. group Id = {0}, name = {1}, media_id = {2}", media.m_nGroupID, sMetaName, media.m_nMediaID);
                                        }
                                    }
                                }
                            }
                            #endregion
                        }

                        medias.Add(media.m_nMediaID, media);

                        #region - get all date meta
                        if (dataSet.Tables.Count > 6 && dataSet.Tables[6].Columns != null && dataSet.Tables[6].Rows != null && dataSet.Tables[6].Rows.Count > 0)
                        {
                            foreach (DataRow dateMetaRow in dataSet.Tables[6].Rows)
                            {
                                mediaId = ODBCWrapper.Utils.GetIntSafeVal(dateMetaRow, "media_id");
                                string metaName = ODBCWrapper.Utils.GetSafeStr(dateMetaRow, "name");
                                DateTime val = ODBCWrapper.Utils.GetDateSafeVal(dateMetaRow, "value");
                                try
                                {
                                    if (!medias[mediaId].m_dMeatsValues.ContainsKey(metaName))
                                    {
                                        medias[mediaId].m_dMeatsValues.Add(metaName, val.ToString("yyyyMMddHHmmss"));
                                    }
                                    else
                                    {
                                        log.WarnFormat("Duplicate meta found. group Id = {0}, name = {1}, media_id = {2}", media.m_nGroupID, metaName, media.m_nMediaID);
                                    }
                                }
                                catch
                                {
                                    log.Error(string.Format("Caught exception when trying to add media to group date metas. mediaId = {0}, metaName = {1}, val = {2}",
                                        mediaId, metaName, val));
                                }
                            }
                        }
                        #endregion
                    }

                    #region - get all the media files types for each mediaId that have been selected.
                    if (dataSet.Tables[1].Columns != null && dataSet.Tables[1].Rows != null && dataSet.Tables[1].Rows.Count > 0)
                    {
                        foreach (DataRow row in dataSet.Tables[1].Rows)
                        {
                            int mediaID = ODBCWrapper.Utils.GetIntSafeVal(row, "media_id");
                            string sMFT = ODBCWrapper.Utils.GetSafeStr(row, "media_type_id");
                            bool isFree = ODBCWrapper.Utils.ExtractBoolean(row, "is_free");

                            Media theMedia = medias[mediaID];

                            theMedia.m_sMFTypes += string.Format("{0};", sMFT);

                            int mediaTypeId;

                            if (isFree)
                            {
                                // if at least one of the media types is free - this media is free
                                theMedia.isFree = true;

                                if (int.TryParse(sMFT, out mediaTypeId))
                                {
                                    theMedia.freeFileTypes.Add(mediaTypeId);
                                }
                            }
                        }
                    }
                    #endregion

                    #region - get regions of media

                    // Regions table should be 6h on stored procedure
                    if (dataSet.Tables.Count > 5 && dataSet.Tables[5].Columns != null && dataSet.Tables[5].Rows != null)
                    {
                        foreach (DataRow mediaRegionRow in dataSet.Tables[5].Rows)
                        {
                            mediaId = ODBCWrapper.Utils.ExtractInteger(mediaRegionRow, "MEDIA_ID");
                            int regionId = ODBCWrapper.Utils.ExtractInteger(mediaRegionRow, "REGION_ID");

                            // Accumulate region ids in list
                            medias[mediaId].regions.Add(regionId);
                        }
                    }

                    // If no regions were found for this media - use 0, that indicates that the media is region-less
                    foreach (Media media in medias.Values)
                    {
                        if (media.regions.Count == 0)
                        {
                            media.regions.Add(0);
                        }
                    }


                    #endregion

                    MediaTagsTranslations tagsTranslations = null;

                    #region - get all media tags
                    if (dataSet.Tables[2].Columns != null && dataSet.Tables[2].Rows != null && dataSet.Tables[2].Rows.Count > 0)
                    {
                        if (group.isTagsSingleTranslation)
                        {
                            tagsTranslations = CatalogDAL.GetMediaTagsTranslations(mediaId);
                        }

                        foreach (DataRow row in dataSet.Tables[2].Rows)
                        {
                            int nTagMediaID = ODBCWrapper.Utils.GetIntSafeVal(row, "media_id");
                            int mttn = ODBCWrapper.Utils.GetIntSafeVal(row, "tag_type_id");
                            string val = ODBCWrapper.Utils.GetSafeStr(row, "value");
                            long tagID = ODBCWrapper.Utils.GetLongSafeVal(row, "tag_id");

                            try
                            {
                                if (group.m_oGroupTags.ContainsKey(mttn))
                                {
                                    string sTagName = group.m_oGroupTags[mttn];

                                    if (!string.IsNullOrEmpty(sTagName))
                                    {
                                        if (!medias[nTagMediaID].m_dTagValues.ContainsKey(sTagName))
                                        {
                                            medias[nTagMediaID].m_dTagValues.Add(sTagName, new HashSet<string>());
                                        }

                                        if (!medias[nTagMediaID].m_dTagValues[sTagName].Contains(val))
                                        {
                                            medias[nTagMediaID].m_dTagValues[sTagName].Add(val);
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                log.ErrorFormat("Error - Caught exception when trying to add media to group tags. TagMediaId={0}; TagTypeID={1}; TagID={2}; TagValue={3}", nTagMediaID, mttn, tagID, val);
                            }
                        }
                    }
                    #endregion

                    #region Clone medias to all translated languages
                    foreach (int mediaID in medias.Keys)
                    {
                        Media media = medias[mediaID];

                        Dictionary<int, Media> tempMediaTrans = new Dictionary<int, Media>();
                        foreach (ApiObjects.LanguageObj oLanguage in group.GetLangauges())
                        {
                            tempMediaTrans.Add(oLanguage.ID, media.Clone());
                        }

                        mediaTranslations.Add(mediaID, tempMediaTrans);

                    }
                    #endregion

                    #region get all translated metas and media info

                    if (dataSet.Tables[3].Columns != null && dataSet.Tables[3].Rows != null && dataSet.Tables[3].Rows.Count > 0)
                    {
                        Dictionary<string, string> dMetas;

                        foreach (DataRow row in dataSet.Tables[3].Rows)
                        {
                            int mediaID = ODBCWrapper.Utils.GetIntSafeVal(row, "MEDIA_ID");
                            int nLanguageID = ODBCWrapper.Utils.GetIntSafeVal(row, "LANGUAGE_ID");

                            if (mediaTranslations.ContainsKey(mediaID) && mediaTranslations[mediaID].ContainsKey(nLanguageID))
                            {
                                Media oMedia = mediaTranslations[mediaID][nLanguageID];

                                if (group.m_oMetasValuesByGroupId.TryGetValue(oMedia.m_nGroupID, out dMetas))
                                {
                                    #region get media translated name
                                    string sTransName = ODBCWrapper.Utils.GetSafeStr(row, "NAME");

                                    if (!string.IsNullOrEmpty(sTransName))
                                        oMedia.m_sName = sTransName;
                                    #endregion

                                    #region get media translated description
                                    string sTransDesc = ODBCWrapper.Utils.GetSafeStr(row, "DESCRIPTION");

                                    if (!string.IsNullOrEmpty(sTransDesc))
                                        oMedia.m_sDescription = sTransDesc;
                                    #endregion

                                    #region get media translated metas
                                    foreach (string sMeta in dMetas.Keys)
                                    {
                                        //if meta is a string, then get translated value from DB, for all other metas, we keep the same values as there's no translation
                                        if (sMeta.EndsWith("_STR"))
                                        {
                                            string sMetaName;
                                            dMetas.TryGetValue(sMeta, out sMetaName);

                                            if (!string.IsNullOrEmpty(sMetaName))
                                            {
                                                string sMetaValue = ODBCWrapper.Utils.GetSafeStr(row, sMeta);

                                                if (!string.IsNullOrEmpty(sMetaValue))
                                                {
                                                    oMedia.m_dMeatsValues[sMetaName] = sMetaValue;
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                }
                            }
                        }
                    }
                    #endregion

                    #region - get all translated media tags

                    if (tagsTranslations != null)
                    {

                        if (tagsTranslations.Translations?.Count > 0)
                        {
                            foreach (var tagTranslation in tagsTranslations.Translations)
                            {
                                GetTranslatedMediaTags(group, tagTranslation.TagTypeId, tagTranslation.Value, mediaId, tagTranslation.LanguageId, ref mediaTranslations);
                            }
                        }
                    }
                    else if (dataSet.Tables[4].Columns != null && dataSet.Tables[4].Rows != null && dataSet.Tables[4].Rows.Count > 0)
                    {
                        foreach (DataRow row in dataSet.Tables[4].Rows)
                        {
                            int nTagMediaID = ODBCWrapper.Utils.GetIntSafeVal(row, "media_id");
                            int mttn = ODBCWrapper.Utils.GetIntSafeVal(row, "tag_type_id");
                            string val = ODBCWrapper.Utils.GetSafeStr(row, "translated_value");
                            int nLangID = ODBCWrapper.Utils.GetIntSafeVal(row, "language_id");
                            long tagID = ODBCWrapper.Utils.GetLongSafeVal(row, "tag_id");

                            GetTranslatedMediaTags(group, mttn, val, nTagMediaID, nLangID, ref mediaTranslations);
                        }
                    }

                    #endregion

                    #region - get countries of media

                    // Regions table should be 6h on stored procedure
                    var hasCountries = dataSet.Tables.Count > 7 && dataSet.Tables[7].Columns != null && dataSet.Tables[7].Rows != null;
                    if (hasCountries)
                    {
                        foreach (DataRow mediaCountryRow in dataSet.Tables[7].Rows)
                        {
                            mediaId = ODBCWrapper.Utils.GetIntSafeVal(mediaCountryRow, "MEDIA_ID");
                            int countryId = ODBCWrapper.Utils.GetIntSafeVal(mediaCountryRow, "COUNTRY_ID");
                            bool isAllowed = ODBCWrapper.Utils.GetIntSafeVal(mediaCountryRow, "IS_ALLOWED") == 1;

                            if (isAllowed)
                            {
                                medias[mediaId].allowedCountries.Add(countryId);
                            }
                            else
                            {
                                medias[mediaId].blockedCountries.Add(countryId);
                            }
                        }
                    }

                    #endregion

                    #region get media suppressed index value and media allowed country

                    // If no allowed countries were found for this media - use 0, that indicates that the media is allowed everywhere
                    foreach (Media media in medias.Values)
                    {
                        if (hasCountries && media.allowedCountries.Count == 0)
                        {
                            media.allowedCountries.Add(0);
                        }

                        //BEO-12023 - Support TVM suppressed
                        if (suppressesIndexes != null && suppressesIndexes.Any())
                        {
                            media.suppressed = IndexManagerCommonHelpers
                                .GetSuppressedIndex(media, suppressesIndexes);

                            if (!string.IsNullOrEmpty(media.suppressed) && medias.ContainsKey(media.m_nMediaID))
                                medias[media.m_nMediaID].suppressed = media.suppressed;
                        }
                    }
                    #endregion
                }
            }
        }

        /// <summary>
        /// Extract suppressed value from media
        /// </summary>
        public static void ExtractSuppressedValue(CatalogGroupCache catalogGroupCache, Media media)
        {
            try
            {
                if (catalogGroupCache != null)
                {
                    var assetStruct = catalogGroupCache.AssetStructsMapById.ContainsKey(media.m_nMediaTypeID) ?
                        catalogGroupCache.AssetStructsMapById[media.m_nMediaTypeID] : null;
                    if (assetStruct != null)
                    {
                        var suppressedOrderMetaIds = assetStruct.AssetStructMetas.Where(m => m.Value.SuppressedOrder.HasValue)?
                            .OrderBy(m => m.Value.SuppressedOrder).Select(m => m.Key).ToList();
                        if (suppressedOrderMetaIds != null && suppressedOrderMetaIds.Count > 0)
                        {
                            //find default meta to suppress by
                            foreach (var metaId in suppressedOrderMetaIds)
                            {
                                var topic = catalogGroupCache.TopicsMapById[metaId];
                                if (AssetManager.BasicMediaAssetMetasSystemNameToName.ContainsKey(topic.SystemName))
                                {
                                    switch (topic.SystemName)
                                    {
                                        case AssetManager.NAME_META_SYSTEM_NAME:
                                            media.suppressed = media.m_sName;
                                            break;
                                        case AssetManager.DESCRIPTION_META_SYSTEM_NAME:
                                            media.suppressed = media.m_sDescription;
                                            break;
                                        case AssetManager.EXTERNAL_ID_META_SYSTEM_NAME:
                                            media.suppressed = media.epgIdentifier;
                                            break;
                                        case AssetManager.ENTRY_ID_META_SYSTEM_NAME:
                                            media.suppressed = media.EntryId;
                                            break;
                                        case AssetManager.PLAYBACK_START_DATE_TIME_META_SYSTEM_NAME:
                                            media.suppressed = media.m_sStartDate;
                                            break;
                                        case AssetManager.PLAYBACK_END_DATE_TIME_META_SYSTEM_NAME:
                                            media.suppressed = media.m_sEndDate;
                                            break;
                                        case AssetManager.CATALOG_START_DATE_TIME_META_SYSTEM_NAME:
                                            media.suppressed = media.CatalogStartDate;
                                            break;
                                        case AssetManager.CATALOG_END_DATE_TIME_META_SYSTEM_NAME:
                                            media.suppressed = media.m_sFinalEndDate;
                                            break;
                                        case AssetManager.CREATE_DATE_TIME_META_SYSTEM_NAME:
                                            media.suppressed = media.m_sCreateDate;
                                            break;
                                        //not supported
                                        case AssetManager.STATUS_META_SYSTEM_NAME:
                                            break;
                                        default:
                                            log.Warn($"Attempt passing unmapped topic: {topic.SystemName} as a suppressed value");
                                            break;
                                    }
                                }
                                
                                if (string.IsNullOrEmpty(media.suppressed) && media.m_dMeatsValues != null && media.m_dMeatsValues.ContainsKey(topic.SystemName))
                                {
                                    //calculated suppressed value
                                    media.suppressed = media.m_dMeatsValues[topic.SystemName];
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Debug($"Error handling media: {media.EntryId} suppressed value, error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Extract suppressed value from epg
        /// </summary>
        public static void ExtractSuppressedValue(CatalogGroupCache catalogGroupCache, EpgCB epg)
        {
            try
            {
                if (catalogGroupCache != null)
                {
                    var m_nMediaTypeID = catalogGroupCache.GetRealAssetStructId(0, out bool isProgramStruct);
                    var assetStruct = catalogGroupCache.AssetStructsMapById.ContainsKey(m_nMediaTypeID) ?
                        catalogGroupCache.AssetStructsMapById[m_nMediaTypeID] : null;
                    if (assetStruct != null)
                    {
                        List<long> suppressedOrderMetaIds = assetStruct.AssetStructMetas.Where(m => m.Value.SuppressedOrder.HasValue)?
                            .OrderBy(m => m.Value.SuppressedOrder).Select(m => m.Key).ToList();
                        if (suppressedOrderMetaIds != null && suppressedOrderMetaIds.Count > 0)
                        {
                            //find default meta to suppress by
                            foreach (var metaId in suppressedOrderMetaIds)
                            {
                                var topic = catalogGroupCache.TopicsMapById[metaId];
                                if (EpgAssetManager.BasicProgramMetasSystemNameToName.ContainsKey(topic.SystemName))
                                {
                                    switch (topic.SystemName)
                                    {
                                        case EpgAssetManager.NAME_META_SYSTEM_NAME:
                                            epg.Suppressed = epg.Name;
                                            break;
                                        case EpgAssetManager.DESCRIPTION_META_SYSTEM_NAME:
                                            epg.Suppressed = epg.Description;
                                            break;
                                        case EpgAssetManager.START_DATE_META_SYSTEM_NAME:
                                            epg.Suppressed = epg.StartDate.ToString("yyyyMMddHHmmss");
                                            break;
                                        case EpgAssetManager.END_DATE_META_SYSTEM_NAME:
                                            epg.Suppressed = epg.EndDate.ToString("yyyyMMddHHmmss");
                                            break;
                                        case EpgAssetManager.CRID_META_SYSTEM_NAME:
                                            epg.Suppressed = epg.Crid;
                                            break;
                                        case EpgAssetManager.EXTERNAL_ID_META_SYSTEM_NAME:
                                            epg.Suppressed = epg.EpgIdentifier;
                                            break;
                                        //not supported
                                        case EpgAssetManager.SERIES_NAME_META_SYSTEM_NAME:
                                        case EpgAssetManager.SERIES_ID_META_SYSTEM_NAME:
                                        case EpgAssetManager.EPISODE_NUMBER_META_SYSTEM_NAME:
                                        case EpgAssetManager.SEASON_NUMBER_META_SYSTEM_NAME:
                                        case EpgAssetManager.PARENTAL_RATING_META_SYSTEM_NAME:
                                        case EpgAssetManager.GENRE_META_SYSTEM_NAME:
                                            break;
                                        default:
                                            log.Warn($"Attempt passing unmapped topic: {topic.SystemName} as a suppressed value");
                                            break;
                                    }
                                }
                                
                                if (string.IsNullOrEmpty(epg.Suppressed) && epg.Metas != null && epg.Metas.ContainsKey(topic.SystemName))
                                {
                                    epg.Suppressed = epg.Metas[topic.SystemName].First();
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Debug($"Error handling epg: {epg.EpgIdentifier} suppressed value, error: {ex.Message}", ex);
            }
        }

        public static void GetTranslatedMediaTags(Group group, int tagType, string val, int mediaId, int langId,
            ref Dictionary<int, Dictionary<int, Media>> mediaTranslations)
        {
            if (group.m_oGroupTags.ContainsKey(tagType) && !string.IsNullOrEmpty(val))
            {
                Media media;

                if (mediaTranslations.ContainsKey(mediaId) && mediaTranslations[mediaId].ContainsKey(langId))
                {
                    media = mediaTranslations[mediaId][langId];
                    string tagTypeName = group.m_oGroupTags[tagType];

                    if (!media.m_dTagValues.ContainsKey(tagTypeName))
                    {
                        media.m_dTagValues.Add(tagTypeName, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
                    }
                    if (!media.m_dTagValues[tagTypeName].Contains(val))
                    {
                        media.m_dTagValues[tagTypeName].Add(val);
                    }
                }
            }
        }

        internal static Dictionary<string, long> GetEpgChannelIdToLinearMediaIdMap(int groupId, List<string> epgChannelIds)
        {
            Dictionary<string, long> epgChannelIdToLinearMediaIdMap = new Dictionary<string, long>();
            DataTable dt = CatalogDAL.GetEpgChannelIdToLinearMediaIdMap(groupId, epgChannelIds);
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    long channelId = ODBCWrapper.Utils.GetLongSafeVal(dr, "id", 0);
                    long linearMediaId = ODBCWrapper.Utils.GetLongSafeVal(dr, "media_id", 0);
                    if (channelId > 0 && linearMediaId > 0 && !epgChannelIdToLinearMediaIdMap.ContainsKey(channelId.ToString()))
                    {
                        epgChannelIdToLinearMediaIdMap.Add(channelId.ToString(), linearMediaId);
                    }
                }
            }

            return epgChannelIdToLinearMediaIdMap;
        }

        internal static int GetIP2CountryId(int groupId, string ip)
        {
            int res = 0;
            try
            {
                ApiObjects.Country country = Core.Api.api.GetCountryByIp(groupId, ip);
                res = country != null ? country.Id : res;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed Utils.GetIP2CountryId with groupId: {0}, ip: {1}", groupId, ip), ex);
            }

            return res;
        }

        internal static string GetIP2CountryName(int groupId, string ip)
        {
            string res = string.Empty;
            try
            {
                ApiObjects.Country country = Core.Api.api.GetCountryByIp(groupId, ip);
                res = country != null ? country.Name : res;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed Utils.GetIP2CountryName with groupId: {0}, ip: {1}", groupId, ip), ex);
            }

            return res;
        }

        public static bool CheckMetaExsits(bool shouldSearchEpg, bool shouldSearchMedia, bool shouldSearchRecordings, Group group, string metaName)
        {
            if (string.IsNullOrEmpty(metaName))
            {
                // no need to check we'll use default order 
                return true;
            }

            if (shouldSearchMedia)
            {
                // check meta in Media
                if (group.m_oMetasValuesByGroupId == null)
                {
                    return false;
                }

                var metas = group.m_oMetasValuesByGroupId.Select(i => i.Value).Cast<Dictionary<string, string>>().SelectMany(d => d.Values).ToList().ConvertAll(x => x.ToLower());
                if (!metas.Contains(metaName))
                {
                    return false;
                }
            }

            if (shouldSearchEpg || shouldSearchRecordings)
            {
                if (group.m_oEpgGroupSettings == null)
                {
                    return false;
                }

                if (group.m_oEpgGroupSettings.metas == null)
                {
                    return false;
                }

                var EpgMetas = group.m_oEpgGroupSettings.metas.Select(i => i.Value.ToLower()).ToList();
                if (!EpgMetas.Contains(metaName))
                {
                    return false;
                }
            }
            return true;
        }

        public static void BuildSearchGroupBy(SearchAggregationGroupBy searchGroupBy, Group @group, UnifiedSearchDefinitions definitions, int groupId)
        {
            if (searchGroupBy != null && searchGroupBy.groupBy != null && searchGroupBy.groupBy.Count > 0)
            {
                var allTags = new HashSet<string>();
                var allMetas = new Dictionary<string, bool>();

                #region Preparations

                bool doesGroupUsesTemplates = CatalogManagement.CatalogManager.Instance.DoesGroupUsesTemplates(groupId);
                CatalogGroupCache catalogGroupCache = null;

                if (!doesGroupUsesTemplates)
                {
                    if (definitions.shouldSearchMedia)
                    {
                        allMetas = group.MetaValueToTypesMapping;
                        foreach (var tagInGroup in group.m_oGroupTags.Values)
                        {
                            allTags.Add(tagInGroup.ToLower());
                        }
                    }

                    if (definitions.shouldSearchEpg || definitions.shouldSearchRecordings)
                    {
                        if (group.m_oEpgGroupSettings != null && group.m_oEpgGroupSettings.m_lMetasName != null)
                        {
                            foreach (var epgMetaName in group.m_oEpgGroupSettings.m_lMetasName)
                            {
                                if (!allMetas.ContainsKey(epgMetaName))
                                {
                                    allMetas.Add(epgMetaName, true);
                                }
                                else
                                {
                                    allMetas[epgMetaName] = true;
                                }
                            }
                        }

                        foreach (var tag in group.m_oEpgGroupSettings.m_lTagsName)
                        {
                            allTags.Add(tag);
                        }
                    }
                }
                else
                {
                    try
                    {
                        if (!CatalogManagement.CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                        {
                            log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling BuildSearchGroupBy", groupId);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(string.Format("Failed TryGetCatalogGroupCacheFromCache for groupId: {0} on BuildSearchGroupBy", groupId), ex);
                    }
                }

                #endregion

                definitions.groupBy = new List<GroupByDefinition>();
                GroupByDefinition distinctGroupBy = null;

                foreach (var groupBy in searchGroupBy.groupBy)
                {
                    var groupByDefinition = new GroupByDefinition()
                    {
                        Key = groupBy
                    };

                    string lowered = groupBy.ToLower();
                    bool isValid = false;

                    if (ReservedGroupByFields.Contains(lowered))
                    {
                        groupByDefinition.Type = eFieldType.Default;

                        if (LocalizedReservedGroupByFields.Contains(lowered))
                        {
                            groupByDefinition.Type = eFieldType.LanguageSpecificField;
                        }

                        isValid = true;
                    }
                    else if (doesGroupUsesTemplates)
                    {
                        if (catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(lowered))
                        {
                            if (catalogGroupCache.TopicsMapBySystemNameAndByType[lowered].ContainsKey(ApiObjects.MetaType.Tag.ToString()))
                            {
                                isValid = true;
                                groupByDefinition.Type = eFieldType.Tag;
                            }
                            else if (definitions.shouldSearchEpg ||
                                     definitions.shouldSearchRecordings ||
                                     catalogGroupCache.TopicsMapBySystemNameAndByType[lowered].ContainsKey(ApiObjects.MetaType.String.ToString()) ||
                                     catalogGroupCache.TopicsMapBySystemNameAndByType[lowered].ContainsKey(ApiObjects.MetaType.MultilingualString.ToString()) ||
                                     catalogGroupCache.TopicsMapBySystemNameAndByType[lowered].ContainsKey(ApiObjects.MetaType.ReleatedEntity.ToString()))
                            {
                                isValid = true;
                                groupByDefinition.Type = eFieldType.StringMeta;
                            }
                            else
                            {
                                isValid = true;
                                groupByDefinition.Type = eFieldType.NonStringMeta;
                            }
                        }
                    }
                    else
                    {
                        if (allMetas.ContainsKey(lowered))
                        {
                            if (allMetas[lowered])
                            {
                                isValid = true;
                                groupByDefinition.Type = eFieldType.StringMeta;
                            }
                            else
                            {
                                isValid = true;
                                groupByDefinition.Type = eFieldType.NonStringMeta;
                            }
                        }
                        else if (allTags.Contains(lowered))
                        {
                            isValid = true;
                            groupByDefinition.Type = eFieldType.Tag;
                        }
                    }

                    if (!isValid)
                    {
                        throw new KalturaException($"Invalid group by field was sent: {groupBy}", (int)eResponseStatus.BadSearchRequest);
                    }

                    // Transform the list of group bys to metas/tags list
                    definitions.groupBy.Add(groupByDefinition);
                    if (groupBy == searchGroupBy.distinctGroup)
                    {
                        distinctGroupBy = groupByDefinition;
                    }
                }

                definitions.groupByOrder = searchGroupBy.groupByOrder;
                definitions.topHitsCount = searchGroupBy.topHitsCount;

                // Validate that we have a distinct group and that it is one of the fields listed in "group by"
                if (!string.IsNullOrEmpty(searchGroupBy.distinctGroup) && searchGroupBy.groupBy.Contains(searchGroupBy.distinctGroup))
                {
                    definitions.distinctGroup = distinctGroupBy;
                    // TODO: this in Index manager
                    //definitions.extraReturnFields.Add(distinctGroupBy.Key);
                }
            }
        }

        private static Tuple<List<int>, bool> GetMediaChannels(Dictionary<string, object> funcParams)
        {
            bool res = false;
            List<int> result = new List<int>();
            try
            {
                if (funcParams != null && funcParams.Count == 2)
                {
                    if (funcParams.ContainsKey("groupId") && funcParams.ContainsKey("mediaId"))
                    {
                        int? groupId, mediaId;
                        groupId = funcParams["groupId"] as int?;
                        mediaId = funcParams["mediaId"] as int?;

                        if (groupId.HasValue && mediaId.HasValue)
                        {
                            IIndexManager indexManager = IndexManagerFactory.Instance.GetIndexManager(groupId.Value);
                            result = indexManager.GetMediaChannels(mediaId.Value);
                            res = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetMediaChannels faild params : {0}", string.Join(";", funcParams.Keys)), ex);
            }
            return new Tuple<List<int>, bool>(result, res);
        }

        internal static List<int> GetChannelsContainingMedia(int groupId, int mediaId)
        {
            List<int> result = null;
            try
            {
                List<string> invalidationKeys = new List<string>() { LayeredCacheKeys.GetGroupChannelsInvalidationKey(groupId) };
                invalidationKeys.AddRange(LayeredCacheKeys.GetAssetMultipleInvalidationKeys(groupId, ApiObjects.eAssetTypes.MEDIA.ToString(), mediaId));
                string key = LayeredCacheKeys.GetChannelsContainingMediaKey(mediaId);
                if (!LayeredCache.Instance.Get<List<int>>(key, ref result, GetMediaChannels, new Dictionary<string, object>() { { "groupId", groupId }, { "mediaId", mediaId } },
                                                            groupId, LayeredCacheConfigNames.CHANNELS_CONTAINING_MEDIA_LAYERED_CACHE_CONFIG_NAME, invalidationKeys))
                {
                    log.ErrorFormat("Failed getting channels containing media from LayeredCache, groupId: {0}, mediaId: {1}, key: {2}", groupId, mediaId, key);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetChannelsContainingMedia for groupId: {0}, mediaId: {1}", groupId, mediaId), ex);
            }

            return result;
        }

        internal static UnifiedSearchResult[] SearchAssets(int groupId, string filter, int pageIndex, int pageSize, bool OnlyIsActive, bool UseStartDate)
        {
            UnifiedSearchResult[] assets = null;

            try
            {
                string catalogSignString = Guid.NewGuid().ToString();
                string catalogSignatureString = ApplicationConfiguration.Current.CatalogSignatureKey.Value;

                string catalogSignature = TVinciShared.WS_Utils.GetCatalogSignature(catalogSignString, catalogSignatureString);

                try
                {
                    UnifiedSearchRequest assetRequest = new UnifiedSearchRequest()
                    {
                        m_nGroupID = groupId,
                        m_oFilter = new Filter()
                        {
                            m_bOnlyActiveMedia = OnlyIsActive,
                            m_bUseStartDate = UseStartDate,
                        },
                        m_sSignature = catalogSignature,
                        m_sSignString = catalogSignString,
                        m_nPageIndex = pageIndex,
                        m_nPageSize = pageSize,
                        filterQuery = filter,
                        isAllowedToViewInactiveAssets = true,
                        shouldIgnoreDeviceRuleID = true,
                        order = new OrderObj()
                        {
                            m_eOrderBy = ApiObjects.SearchObjects.OrderBy.ID,
                            m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC
                        }
                    };

                    BaseResponse response = assetRequest.GetResponse(assetRequest);
                    assets = ((UnifiedSearchResponse)response).searchResults.ToArray();
                }
                catch (Exception ex)
                {

                }
            }
            catch (Exception ex)
            {
                log.Error("Configuration Reading - Couldn't read values from configuration ", ex);
            }

            return assets;
        }

        public static List<BaseObject> GetOrderedAssets(int groupId, List<BaseObject> assets, Filter filter, bool managementData = false)
        {
            List<BaseObject> result = new List<BaseObject>();
            int languageId = filter != null ? filter.m_nLanguage : 0;
            try
            {
                if (assets != null && assets.Count > 0)
                {
                    List<KeyValuePair<eAssetTypes, long>> assetsToRetrieve = assets.Where(x => x.AssetType != eAssetTypes.NPVR).Select(x =>
                                                                           new KeyValuePair<eAssetTypes, long>(x.AssetType, long.Parse(x.AssetId))).ToList();
                    List<BaseObject> npvrs = assets.Where(x => x.AssetType == eAssetTypes.NPVR).ToList();
                    List<long> epgIdsFromRecording = GetEpgIdsFromNpvrObject(npvrs);
                    if (epgIdsFromRecording != null && epgIdsFromRecording.Count > 0)
                    {
                        assetsToRetrieve.AddRange(epgIdsFromRecording.Select(x => new KeyValuePair<eAssetTypes, long>(eAssetTypes.EPG, x)));
                    }

                    List<BaseObject> unOrderedAssets = GetAssets(groupId, assetsToRetrieve, filter, managementData);

                    if (unOrderedAssets == null || unOrderedAssets.Count == 0)
                    {
                        return result;
                    }

                    string keyFormat = "{0}_{1}"; // mapped asset key format = assetType_assetId
                    Dictionary<string, BaseObject> mappedAssets = unOrderedAssets.Where(x => x.AssetId != EMPTY_ASSET_ID).ToDictionary(x => string.Format(keyFormat, x.AssetType.ToString(), x.AssetId), x => x);
                    foreach (BaseObject baseAsset in assets)
                    {
                        string key = string.Empty;
                        bool isNpvr = baseAsset.AssetType == eAssetTypes.NPVR;
                        RecordingType? scheduledRecordingType = null;
                        bool isMulti = false;

                        if (isNpvr)
                        {
                            string epgId = GetEpgIdFromNpvrObject(baseAsset, ref scheduledRecordingType, ref isMulti);
                            if (!string.IsNullOrEmpty(epgId))
                            {
                                key = string.Format(keyFormat, eAssetTypes.EPG.ToString(), epgId);
                            }
                        }
                        else
                        {
                            key = string.Format(keyFormat, baseAsset.AssetType.ToString(), baseAsset.AssetId);
                        }

                        if (mappedAssets.TryGetValue(key, out var mappedAsset))
                        {
                            if (isNpvr && long.TryParse(baseAsset.AssetId, out var recordingId) && recordingId > 0)
                            {
                                var recordingObject = new RecordingObj
                                {
                                    RecordingId = recordingId,
                                    RecordingType = scheduledRecordingType,
                                    Program = mappedAsset as ProgramObj,
                                    m_dUpdateDate = baseAsset.m_dUpdateDate,
                                    IsMulti = isMulti
                                };

                                result.Add(recordingObject);
                            }
                            else
                            {
                                result.Add(mappedAsset);
                            }
                        }
                        // support for TVPAPI (returns empty object for assets that don't exist)
                        else
                        {
                            result.Add(null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetOrderedAssets for groupId: {0}, assets: {1}", groupId,
                                            assets != null ? string.Join(",", assets.Select(x => string.Format("{0}_{1}", x.AssetType.ToString(), x.AssetId)).ToList()) : string.Empty), ex);
            }

            return result;
        }

        public static bool ConvertStringToDateTimeByFormat(string dateInString, string convertToFormat, out DateTime dateTime)
            => DateTime.TryParseExact(
                dateInString,
                convertToFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out dateTime);

        private static string GetEpgIdFromNpvrObject(BaseObject baseObject, ref RecordingType? scheduledRecordingType, ref bool isMulti)
        {
            string epgId = null;
            try
            {
                RecordingSearchResult searchResult = baseObject as RecordingSearchResult;
                if (searchResult != null)
                {
                    epgId = searchResult.EpgId;
                    scheduledRecordingType = searchResult.RecordingType;
                    isMulti = searchResult.IsMulti;
                }
                else
                {
                    UserWatchHistory watchHistory = baseObject as UserWatchHistory;
                    if (watchHistory != null)
                    {
                        epgId = watchHistory.EpgId.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetRecordingObjFromBaseObject, baseObject: {0}", baseObject != null ? string.Format("{0}_{1}", baseObject.AssetType, baseObject.AssetId) : "null"), ex);
            }

            return epgId;
        }

        private static List<long> GetEpgIdsFromNpvrObject(List<BaseObject> npvrs)
        {
            List<long> result = new List<long>();
            try
            {
                if (npvrs != null && npvrs.Count > 0)
                {
                    foreach (BaseObject npvr in npvrs)
                    {
                        RecordingSearchResult searchResult = npvr as RecordingSearchResult;
                        if (searchResult != null)
                        {
                            result.Add(long.Parse(searchResult.EpgId));
                        }
                        else
                        {
                            UserWatchHistory watchHistory = npvr as UserWatchHistory;
                            if (watchHistory != null)
                            {
                                result.Add(watchHistory.EpgId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetEpgIdsFromNpvrObject, npvrs: {0}",
                            npvrs != null ? string.Join(",", npvrs.Select(x => string.Format("{0}_{1}", x.AssetType, x.AssetId)).ToList()) : string.Empty), ex);
            }

            return result;
        }

        private static List<BaseObject> GetAssets(int groupId, List<KeyValuePair<eAssetTypes, long>> assets, Filter filter, bool managementData = false)
        {
            List<BaseObject> result = null;
            int languageId = filter != null ? filter.m_nLanguage : 0;
            try
            {
                if (assets != null && assets.Count > 0)
                {
                    result = new List<BaseObject>();
                    List<int> mediaIds = assets.Where(x => x.Key == eAssetTypes.MEDIA).Select(x => (int)x.Value).Distinct().ToList();
                    List<long> epgIds = assets.Where(x => x.Key == eAssetTypes.EPG).Select(x => x.Value).Distinct().ToList();
                    if (mediaIds != null && mediaIds.Count > 0)
                    {
                        List<MediaObj> mediaAssets = null;
                        if (managementData)
                        {
                            mediaAssets = CatalogLogic.CompleteMediaDetails(mediaIds, groupId, filter, true);
                        }
                        else
                        {
                            mediaAssets = GetMediaObjectsFromCache(groupId, mediaIds, filter);
                        }

                        if (mediaAssets != null)
                        {
                            result.AddRange(mediaAssets);
                            if (mediaAssets.Count != mediaIds.Count)
                            {
                                List<int> missingMediaIds = mediaIds.Except(mediaAssets.Select(x => int.Parse(x.AssetId))).ToList();
                                log.WarnFormat("GetMediaObjectsFromCache didn't find the following mediaIds: {0}", string.Join(",", missingMediaIds));
                            }
                        }
                        else
                        {
                            log.WarnFormat("GetMediaObjectsFromCache didn't find the following mediaIds: {0}", string.Join(",", mediaIds));
                        }

                    }

                    if (epgIds != null && epgIds.Count > 0)
                    {
                        List<ProgramObj> epgs = GetProgramFromCache(groupId, epgIds, filter);

                        if (epgs != null)
                        {
                            result.AddRange(epgs);
                            if (epgs.Count != epgIds.Count)
                            {
                                List<long> missingEpgIds = epgIds.Except(epgs.Select(x => long.Parse(x.AssetId))).ToList();
                                log.WarnFormat("GetProgramFromCache didn't find the following epgIds: {0}", string.Join(",", missingEpgIds));
                            }
                        }
                        else
                        {
                            log.WarnFormat("GetProgramFromCache didn't find the following epgIds: {0}", string.Join(",", epgIds));
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetAssets with groupId: {0}, assets: {1}", groupId,
                                        assets != null ? string.Join(",", assets.Select(x => string.Format("{0}_{1}_{2}", x.Key, x.Value, languageId)).ToList()) : string.Empty), ex);
            }

            return result;
        }

        private static List<MediaObj> GetMediaObjectsFromCache(int groupId, List<int> ids, Filter filter)
        {
            List<MediaObj> result = null;
            try
            {
                if (ids == null || ids.Count == 0)
                {
                    return result;
                }

                eAssetTypes assetType = eAssetTypes.MEDIA;
                Dictionary<string, MediaObj> mediaObjMap = null;
                int languageId = filter != null ? filter.m_nLanguage : 0;
                Dictionary<string, string> keyToOriginalValueMap = LayeredCacheKeys.GetAssetsWithLanguageKeyMap(assetType.ToString(), ids.Select(x => x.ToString()).ToList(), languageId);
                Dictionary<string, List<string>> invalidationKeysMap = LayeredCacheKeys.GetMediaInvalidationKeysMap(groupId, assetType.ToString(), ids.Select(x => (long)x).ToList(), languageId);

                if (!LayeredCache.Instance.GetValues<MediaObj>(keyToOriginalValueMap, ref mediaObjMap, GetMediaObjects, new Dictionary<string, object>() { { "groupId", groupId }, { "ids", ids },
                                                                { "filter", filter } }, groupId, LayeredCacheConfigNames.GET_ASSETS_WITH_LANGUAGE_LIST_CACHE_CONFIG_NAME, invalidationKeysMap))
                {
                    log.ErrorFormat("Failed getting GetMediaObjectsFromCache from LayeredCache, groupId: {0}, ids: {1}, filter language: {2}",
                                    groupId, ids != null ? string.Join(",", ids) : string.Empty, filter != null ? filter.m_nLanguage.ToString() : "null filter");
                }
                else if (mediaObjMap != null)
                {
                    result = mediaObjMap.Values.ToList();
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetMediaAssetsFromCache with groupId: {0}, ids: {1}", groupId, ids != null ? string.Join(",", ids) : string.Empty), ex);
            }

            return result;
        }

        private static Tuple<Dictionary<string, MediaObj>, bool> GetMediaObjects(Dictionary<string, object> funcParams)
        {
            bool res = false;
            Dictionary<string, MediaObj> result = new Dictionary<string, MediaObj>();
            try
            {
                if (funcParams != null && funcParams.ContainsKey("ids") && funcParams.ContainsKey("filter") && funcParams.ContainsKey("groupId"))
                {
                    string key = string.Empty;
                    List<int> ids;
                    int? groupId = funcParams["groupId"] as int?;
                    Filter filter = funcParams["filter"] as Filter;
                    if (funcParams.ContainsKey(LayeredCache.MISSING_KEYS) && funcParams[LayeredCache.MISSING_KEYS] != null)
                    {
                        ids = ((List<string>)funcParams[LayeredCache.MISSING_KEYS]).Select(x => int.Parse(x)).ToList();
                    }
                    else
                    {
                        ids = funcParams["ids"] != null ? funcParams["ids"] as List<int> : null;
                    }

                    List<MediaObj> assets = new List<MediaObj>();

                    // filter can be null? according to CatalogLogic.CompleteMediaDetails code, yes it can...
                    if (ids != null && groupId.HasValue)
                    {
                        assets = CatalogLogic.CompleteMediaDetails(ids, groupId.Value, filter, false);
                        res = assets.Count == ids.Count || (assets.Count > 0 && filter.AllowPartialResponse);
                    }

                    if (res)
                    {
                        result = assets.Where(x => x != null).ToDictionary(x => LayeredCacheKeys.GetAssetWithLanguageKey(eAssetTypes.MEDIA.ToString(), x.AssetId, filter.m_nLanguage), x => x);
                        List<string> unvalidMediaIdsToAdd = ids.Select(x => x.ToString()).Except(assets.Where(x => x != null).Select(x => x.AssetId)).ToList();
                        if (unvalidMediaIdsToAdd != null && unvalidMediaIdsToAdd.Count > 0)
                        {
                            foreach (string mediaId in unvalidMediaIdsToAdd)
                            {
                                result.Add(LayeredCacheKeys.GetAssetWithLanguageKey(eAssetTypes.MEDIA.ToString(), mediaId, filter.m_nLanguage), new MediaObj() { AssetId = "0" });
                            }
                        }
                    }
                    else
                    {
                        List<int> missingIds = assets.Select(x => int.Parse(x.AssetId)).Except(ids).ToList();
                        log.DebugFormat("Missing media ids: {0}", string.Join(",", missingIds));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetMediaObjects failed params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<string, MediaObj>, bool>(result, res);
        }

        internal static List<ProgramObj> GetProgramFromCache(int groupId, List<long> ids, Filter filter)
        {
            List<ProgramObj> result = null;
            try
            {
                if (ids == null || ids.Count == 0)
                {
                    return result;
                }

                eAssetTypes assetType = eAssetTypes.EPG;
                Dictionary<string, ProgramObj> programsMap = null;
                int languageId = filter != null ? filter.m_nLanguage : 0;
                Dictionary<string, string> keyToOriginalValueMap = LayeredCacheKeys.GetAssetsWithLanguageKeyMap(assetType.ToString(), ids.Select(x => x.ToString()).ToList(), languageId);
                Dictionary<string, List<string>> invalidationKeysMap = LayeredCacheKeys.GetEpgInvalidationKeysMap(groupId, assetType.ToString(), ids, languageId);

                if (!LayeredCache.Instance.GetValues<ProgramObj>(keyToOriginalValueMap, ref programsMap, GetPrograms, new Dictionary<string, object>() { { "groupId", groupId }, { "ids", ids },
                                                                { "filter", filter } }, groupId, LayeredCacheConfigNames.GET_ASSETS_WITH_LANGUAGE_LIST_CACHE_CONFIG_NAME, invalidationKeysMap))
                {
                    log.ErrorFormat("Failed getting GetProgramFromCache from LayeredCache, groupId: {0}, ids: {1}, filter language: {2}",
                                    groupId, ids != null ? string.Join(",", ids) : string.Empty, filter != null ? filter.m_nLanguage.ToString() : "null filter");
                }
                else if (programsMap != null)
                {
                    result = programsMap.Values.ToList();
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetProgramFromCache with groupId: {0}, ids: {1}", groupId, ids != null ? string.Join(",", ids) : string.Empty), ex);
            }

            return result;
        }

        private static Tuple<Dictionary<string, ProgramObj>, bool> GetPrograms(Dictionary<string, object> funcParams)
        {
            bool res = false;
            Dictionary<string, ProgramObj> result = new Dictionary<string, ProgramObj>();
            try
            {
                if (funcParams != null && funcParams.ContainsKey("ids") && funcParams.ContainsKey("filter") && funcParams.ContainsKey("groupId"))
                {
                    string key = string.Empty;
                    List<long> ids;
                    int? groupId = funcParams["groupId"] as int?;
                    Filter filter = funcParams["filter"] as Filter;
                    if (funcParams.ContainsKey(LayeredCache.MISSING_KEYS) && funcParams[LayeredCache.MISSING_KEYS] != null)
                    {
                        ids = ((List<string>)funcParams[LayeredCache.MISSING_KEYS]).Select(x => long.Parse(x)).ToList();
                    }
                    else
                    {
                        ids = funcParams["ids"] != null ? funcParams["ids"] as List<long> : null;
                    }

                    List<ProgramObj> programs = new List<ProgramObj>();

                    // filter can be null? according to CatalogLogic.GetEPGProgramInformation code, yes it can...
                    if (ids != null && groupId.HasValue)
                    {
                        programs = CatalogLogic.GetEPGProgramInformation(ids, groupId.Value, filter);
                        res = programs.Count == ids.Count || (programs.Count > 0 && filter.AllowPartialResponse);
                    }

                    if (res)
                    {
                        result = programs.ToDictionary(x => LayeredCacheKeys.GetAssetWithLanguageKey(eAssetTypes.EPG.ToString(), x.AssetId, filter.m_nLanguage), x => x);
                    }
                    else
                    {
                        List<long> missingIds = programs.Select(x => long.Parse(x.AssetId)).Except(ids).ToList();
                        log.DebugFormat("Missing program ids: {0}", string.Join(",", missingIds));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetPrograms failed params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<string, ProgramObj>, bool>(result, res);
        }

    }
}

