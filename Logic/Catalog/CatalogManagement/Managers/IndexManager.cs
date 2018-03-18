using ApiObjects;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using CachingProvider.LayeredCache;
using Core.Catalog.Cache;
using Core.Catalog.Response;
using ElasticSearch.Common;
using ElasticSearch.Common.DeleteResults;
using ElasticSearch.Searcher;
using GroupsCacheManager;
using KLogMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using Tvinci.Core.DAL;

namespace Core.Catalog.CatalogManagement
{
    public class IndexManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public static readonly string MEDIA = "media";
        public static readonly string EPG = "epg";
        public static readonly int DAYS = 30;

        #region Main Methods

        public static bool UpsertMedia(int groupId, int assetId)
        {
            bool result = false;
            ElasticSearch.Common.ESSerializerV2 esSerializer = new ElasticSearch.Common.ESSerializerV2();
            ElasticSearchApi esApi = new ElasticSearch.Common.ElasticSearchApi();

            if (assetId <= 0)
            {
                log.WarnFormat("Received media request of invalid media id {0} when calling UpsertMedia", assetId);
                return result;
            }

            CatalogGroupCache catalogGroupCache;
            if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
            {
                log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling UpsertMedia", groupId);
                return result;
            }

            try
            {
                //Create Media Object
                Dictionary<int, Dictionary<int, Media>> mediaDictionary = GetGroupMedias(groupId, assetId);
                if (mediaDictionary != null)
                {
                    // Just to be sure
                    if (mediaDictionary.ContainsKey(assetId))
                    {
                        foreach (int languageId in mediaDictionary[assetId].Keys)
                        {
                            LanguageObj language = catalogGroupCache.LanguageMapById.ContainsKey(languageId) ? catalogGroupCache.LanguageMapById[languageId] : null;
                            if (language != null)
                            {
                                string suffix = null;
                                if (!language.IsDefault)
                                {
                                    suffix = language.Code;
                                }

                                Media media = mediaDictionary[assetId][languageId];
                                if (media != null)
                                {
                                    string serializedMedia;
                                    serializedMedia = esSerializer.SerializeMediaObject(media, suffix);
                                    string type = GetTanslationType(MEDIA, language);
                                    if (!string.IsNullOrEmpty(serializedMedia))
                                    {
                                        result = esApi.InsertRecord(groupId.ToString(), type, media.m_nMediaID.ToString(), serializedMedia);
                                        if (!result)
                                        {
                                            log.Error("Error - " + string.Format("Could not update media in ES. GroupID={0};Type={1};MediaID={2};serializedObj={3};",
                                                                                    groupId, type, media.m_nMediaID, serializedMedia));
                                        }
                                        // support for old invalidation keys
                                        else
                                        {
                                            LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetMediaInvalidationKey(groupId, assetId));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Upsert Media threw an exception. Exception={0};Stack={1}", ex.Message, ex.StackTrace), ex);
            }

            return result;
        }

        public static bool DeleteMedia(int groupId, int assetId)
        {
            bool result = false;
            string index = groupId.ToString();
            ElasticSearchApi esApi = new ElasticSearch.Common.ElasticSearchApi();

            if (assetId <= 0)
            {
                log.WarnFormat("Received media request of invalid media id {0} when calling DeleteMedia", assetId);
                return result;
            }

            CatalogGroupCache catalogGroupCache;
            if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
            {
                log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling DeleteMedia", groupId);
                return result;
            }

            try
            {
                List<LanguageObj> languages = catalogGroupCache.LanguageMapById.Values.ToList();
                if (languages != null && languages.Count > 0)
                {
                    ESTerm term = new ESTerm(true)
                    {
                        Key = "media_id",
                        Value = assetId.ToString()
                    };

                    ESQuery query = new ESQuery(term);
                    string queryString = query.ToString();
                    result = true;
                    foreach (LanguageObj lang in languages)
                    {
                        string type = GetTanslationType(MEDIA, lang);
                        result = esApi.DeleteDocsByQuery(index, type, ref queryString) && result;                        
                    }
                }                       
            }
            catch (Exception ex)
            {            
                log.ErrorFormat("Could not delete media from ES. Media id={0}, ex={1}", assetId, ex);
            }

            if (!result)
            {
                log.ErrorFormat("Delete media with id {0} failed", assetId);
            }
            // support for old invalidation keys
            else
            {
                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetMediaInvalidationKey(groupId, assetId));
            }

            return result;
        }

        public static bool UpsertEpg(int groupId, int assetId)
        {
            throw new NotImplementedException("UpsertEpg should be implemented to new TVM logic");
            //bool result = false;

            //try
            //{
            //    ElasticSearch.Common.ESSerializerV2 esSerializer = new ElasticSearch.Common.ESSerializerV2();
            //    ElasticSearchApi esApi = new ElasticSearch.Common.ElasticSearchApi();

            //    // get all languages per group
            //    Group group = GroupsCache.Instance().GetGroup(groupId);

            //    if (group == null)
            //    {
            //        log.ErrorFormat("Couldn't get group {0}", groupId);
            //        return false;
            //    }

            //    // dictionary contains all language ids and its  code (string)
            //    List<LanguageObj> languages = group.GetLangauges();
            //    List<string> languageCodes = new List<string>();

            //    if (languages != null)
            //    {
            //        languageCodes = languages.Select(p => p.Code.ToLower()).ToList<string>();
            //    }
            //    else
            //    {
            //        // return false; // perhaps?
            //        log.Debug("Warning - " + string.Format("Group {0} has no languages defined.", groupId));
            //    }

            //    List<EpgCB> epgObjects = new List<EpgCB>();

            //    epgObjects = GetEpgPrograms(groupId, assetId, languageCodes);

            //    // GetLinear Channel Values 
            //    GetLinearChannelValues(epgObjects, groupId);

            //    if (epgObjects != null)
            //    {
            //        if (epgObjects.Count == 0)
            //        {
            //            log.WarnFormat("Attention - when updating EPG, epg list is empty for ID = {0}", assetId);
            //            result = true;
            //        }
            //        else
            //        {
            //            // Temporarily - assume success
            //            bool temporaryResult = true;

            //            // Create dictionary by languages
            //            foreach (LanguageObj language in languages)
            //            {
            //                // Filter programs to current language
            //                List<EpgCB> currentLanguageEpgs = epgObjects.Where(epg =>
            //                    epg.Language.ToLower() == language.Code.ToLower() || (language.IsDefault && string.IsNullOrEmpty(epg.Language))).ToList();

            //                if (currentLanguageEpgs != null && currentLanguageEpgs.Count > 0)
            //                {
            //                    List<ESBulkRequestObj<ulong>> bulkRequests = new List<ESBulkRequestObj<ulong>>();
            //                    string alias = GetEpgGroupAliasStr(groupId);

            //                    // Create bulk request object for each program
            //                    foreach (EpgCB epg in currentLanguageEpgs)
            //                    {
            //                        string suffix = null;

            //                        if (!language.IsDefault)
            //                        {
            //                            suffix = language.Code;
            //                        }

            //                        string serializedEpg = esSerializer.SerializeEpgObject(epg, suffix);

            //                        bulkRequests.Add(new ESBulkRequestObj<ulong>()
            //                        {
            //                            docID = GetEpgDocumentId(epg),
            //                            index = alias,
            //                            type = GetTanslationType(EPG, language),
            //                            Operation = eOperation.index,
            //                            document = serializedEpg,
            //                            routing = epg.StartDate.ToUniversalTime().ToString("yyyyMMdd"),
            //                        });
            //                    }

            //                    // send request to ES API
            //                    List<KeyValuePair<string, string>> invalidResults = esApi.CreateBulkRequest(bulkRequests);

            //                    if (invalidResults != null && invalidResults.Count > 0)
            //                    {
            //                        foreach (KeyValuePair<string, string> invalidResult in invalidResults)
            //                        {
            //                            log.Error("Error - " + string.Format(
            //                                "Could not update EPG in ES. GroupID={0};Type={1};EPG_ID={2};error={3};",
            //                                groupId, EPG, invalidResult.Key, invalidResult.Value));
            //                        }

            //                        result = false;
            //                        temporaryResult = false;
            //                    }
            //                    else
            //                    {
            //                        temporaryResult &= true;
            //                    }
            //                }
            //            }

            //            result = temporaryResult;
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    log.Error("Error - " + string.Format("Update EPGs threw an exception. Exception={0};Stack={1}", ex.Message, ex.StackTrace), ex);
            //    result = false;
            //}

            //return result;
        }

        public static bool DeleteEpg(int groupId, int assetId)
        {
            throw new NotImplementedException("DeleteEpg should be implemented to new TVM logic");
            //bool result = true;

            //try
            //{
            //    ElasticSearchApi esApi = new ElasticSearch.Common.ElasticSearchApi();

            //    // get all languages per group
            //    Group group = GroupsCache.Instance().GetGroup(groupId);

            //    if (group == null)
            //    {
            //        log.ErrorFormat("Couldn't get group {0}", groupId);
            //        return false;
            //    }

            //    // dictionary contains all language ids and its  code (string)
            //    List<LanguageObj> languages = group.GetLangauges();

            //    string alias = GetEpgGroupAliasStr(groupId);

            //    ESTerm term = new ESTerm(true)
            //    {
            //        Key = "epg_id",
            //        Value = assetId.ToString()
            //    };

            //    ESQuery query = new ESQuery(term);
            //    string queryString = query.ToString();

            //    foreach (LanguageObj lang in languages)
            //    {
            //        string type = GetTanslationType(EPG, lang);
            //        esApi.DeleteDocsByQuery(alias, type, ref queryString);
            //    }

            //    result = true;
            //}
            //catch (Exception ex)
            //{
            //    result = false;
            //    log.ErrorFormat("Failed deleting epg from index. id = {0} ex = {1}", assetId, ex);
            //}

            //return result;
        }

        public static bool UpsertChannel(int groupId, int channelId, Channel channel = null)
        {
            bool result = false;
            ElasticSearch.Common.ESSerializerV2 esSerializer = new ElasticSearch.Common.ESSerializerV2();
            ElasticSearchApi esApi = new ElasticSearch.Common.ElasticSearchApi();

            if (channelId <= 0)
            {
                log.WarnFormat("Received channel request of invalid channel id {0} when calling UpsertChannel", channelId);
                return result;
            }

            try
            {
                if (channel == null)
                {
                    channel = ChannelManager.GetChannelById(groupId, channelId);
                    if (channel == null)
                    {
                        log.ErrorFormat("failed to get channel object for groupId: {0}, channelId: {1} when calling UpsertChannel", groupId, channelId);
                        return result;
                    }
                }

                string index = ElasticSearch.Common.Utils.GetGroupChannelIndex(groupId);
                string type = "channel";
                string serializedChannel = esSerializer.SerializeChannelObject(channel);
                result = esApi.InsertRecord(index, type, channelId.ToString(), serializedChannel);

                // index percolator async
                if (result)
                {
                    if (!CatalogLogic.UpdateChannelIndex(new List<long>() { channelId }, groupId, eAction.Update))
                    {
                        log.ErrorFormat("Update channel percolator failed for Upsert Channel");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Upsert Channel threw an exception. Exception={0};Stack={1}", ex.Message, ex.StackTrace), ex);
            }

            if (!result)
            {
                log.ErrorFormat("Upsert channel with id {0} failed", channelId);
            }           

            return result;
        }

        public static bool DeleteChannel(int groupId, int channelId, Channel channel)
        {
            bool result = false;            
            ElasticSearchApi esApi = new ElasticSearch.Common.ElasticSearchApi();

            if (channelId <= 0)
            {
                log.WarnFormat("Received channel request of invalid channel id {0} when calling DeleteChannel", channelId);
                return result;
            }

            try
            {
                // update channel status to delete
                channel.m_nStatus = 2;
                if (UpsertChannel(groupId, channelId, channel))
                {
                    string index = ElasticSearch.Common.Utils.GetGroupChannelIndex(groupId);
                    ESTerm term = new ESTerm(true)
                    {
                        Key = "channel_id",
                        Value = channelId.ToString()
                    };

                    ESQuery query = new ESQuery(term);
                    string queryString = query.ToString();
                    string type = "channel";
                    result = esApi.DeleteDocsByQuery(index, type, ref queryString);

                    // index percolator async
                    if (result)
                    {
                        if (!CatalogLogic.UpdateChannelIndex(new List<long>() { channelId }, groupId, eAction.Delete))
                        {
                            log.ErrorFormat("Update channel percolator failed for Delete Channel");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Delete Channel threw an exception. Exception={0};Stack={1}", ex.Message, ex.StackTrace), ex);
            }

            if (!result)
            {
                log.ErrorFormat("Delete channel with id {0} failed", channelId);
            }

            return result;
        }

        #endregion

        #region Media

        public static Dictionary<int, Dictionary<int, Media>> GetGroupMedias(int groupId, int mediaId)
        {
            //dictionary contains medias such that first key is media_id, which returns a dictionary with a key language_id and value Media object.
            //E.g. mediaTranslations[123][2] --> will return media 123 of the hebrew language
            Dictionary<int, Dictionary<int, Media>> mediaTranslations = new Dictionary<int, Dictionary<int, Media>>();

            //temporary media dictionary
            Dictionary<int, Media> medias = new Dictionary<int, Media>();

            try
            {

                if (Core.Catalog.CatalogManagement.CatalogManager.DoesGroupUsesTemplates(groupId))
                {
                    return Core.Catalog.CatalogManagement.AssetManager.GetMediaForElasticSearchIndex(groupId, mediaId);                    
                }

                GroupManager groupManager = new GroupManager();
                Group group = groupManager.GetGroup(groupId);
                if (group == null)
                {
                    log.Error("Error - Could not load group from cache in GetGroupMedias");
                    return mediaTranslations;
                }

                ApiObjects.LanguageObj defaultLangauge = group.GetGroupDefaultLanguage();
                if (defaultLangauge == null)
                {
                    log.Error("Error - Could not get group default language from cache in GetGroupMedias");
                    return mediaTranslations;
                }

                ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Get_GroupMedias_ml");
                storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
                storedProcedure.AddParameter("@GroupID", groupId);
                storedProcedure.AddParameter("@MediaID", mediaId);

                DataSet dataSet = storedProcedure.ExecuteDataSet();
                //Task<DataSet> dataSetTask = Task<DataSet>.Factory.StartNew(() => storedProcedure.ExecuteDataSet());
                //dataSetTask.Wait();
                //DataSet dataSet = dataSetTask.Result;

                Core.Catalog.Utils.BuildMediaFromDataSet(ref mediaTranslations, ref medias, group, dataSet);

                // get media update dates
                DataTable updateDates = CatalogDAL.Get_MediaUpdateDate(new List<int>() { (int)mediaId });                
            }
            catch (Exception ex)
            {
                log.Error("Media Exception", ex);
            }

            return mediaTranslations;
        }

        #endregion

        #region EPG 

        public static List<EpgCB> GetEpgPrograms(int groupId, int epgId, List<string> languages, EpgBL.BaseEpgBL epgBL = null)
        {
            throw new NotImplementedException("GetEpgPrograms should be implemented to new TVM logic");
            //List<EpgCB> results = new List<EpgCB>();

            //// If no language was received - just get epg program by old method
            //if (languages == null || languages.Count == 0)
            //{
            //    EpgCB program = GetEpgProgram(groupId, epgId);

            //    results.Add(program);
            //}
            //else
            //{
            //    try
            //    {
            //        if (epgBL == null)
            //        {
            //            epgBL = EpgBL.Utils.GetInstance(groupId);
            //        }

            //        ulong uEpgID = (ulong)epgId;
            //        results = epgBL.GetEpgCB(uEpgID, languages);
            //    }
            //    catch (Exception ex)
            //    {
            //        log.Error("Error (GetEpgProgram) - " + string.Format("epg:{0}, msg:{1}, st:{2}", epgId, ex.Message, ex.StackTrace), ex);
            //    }
            //}

            //return results;
        }

        public static EpgCB GetEpgProgram(int nGroupID, int nEpgID)
        {
            throw new NotImplementedException("GetEpgProgram should be implemented to new TVM logic");
            //EpgCB res = null;

            //DataSet ds = Tvinci.Core.DAL.EpgDal.GetEpgProgramDetails(nGroupID, nEpgID);

            //if (ds != null && ds.Tables != null)
            //{
            //    if (ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
            //    {
            //        //Basic Details
            //        foreach (DataRow row in ds.Tables[0].Rows)
            //        {
            //            EpgCB epg = new EpgCB();
            //            epg.ChannelID = ODBCWrapper.Utils.GetIntSafeVal(row["EPG_CHANNEL_ID"]);
            //            epg.EpgID = ODBCWrapper.Utils.GetUnsignedLongSafeVal(row["ID"]);
            //            epg.GroupID = ODBCWrapper.Utils.GetIntSafeVal(row["GROUP_ID"]);
            //            epg.isActive = (ODBCWrapper.Utils.GetIntSafeVal(row["IS_ACTIVE"]) == 1) ? true : false;
            //            epg.Description = ODBCWrapper.Utils.GetSafeStr(row["DESCRIPTION"]);
            //            epg.Name = ODBCWrapper.Utils.GetSafeStr(row["NAME"]);
            //            if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row["START_DATE"])))
            //            {
            //                epg.StartDate = ODBCWrapper.Utils.GetDateSafeVal(row["START_DATE"]);
            //            }
            //            if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row["END_DATE"])))
            //            {
            //                epg.EndDate = ODBCWrapper.Utils.GetDateSafeVal(row["END_DATE"]);
            //            }
            //            epg.Crid = ODBCWrapper.Utils.GetSafeStr(row["crid"]);

            //            //Metas
            //            if (ds.Tables.Count >= 3 && ds.Tables[2] != null && ds.Tables[2].Rows != null && ds.Tables[2].Rows.Count > 0)
            //            {
            //                List<string> tempList;

            //                foreach (DataRow meta in ds.Tables[2].Rows)
            //                {
            //                    string metaName = ODBCWrapper.Utils.GetSafeStr(meta["name"]);
            //                    string metaValue = ODBCWrapper.Utils.GetSafeStr(meta["value"]);

            //                    if (epg.Metas.TryGetValue(metaName, out tempList))
            //                    {
            //                        tempList.Add(metaValue);
            //                    }
            //                    else
            //                    {
            //                        tempList = new List<string>() { metaValue };
            //                        epg.Metas.Add(metaName, tempList);
            //                    }
            //                }
            //            }

            //            //Tags
            //            if (ds.Tables.Count >= 4 && ds.Tables[3] != null && ds.Tables[3].Rows != null && ds.Tables[3].Rows.Count > 0)
            //            {
            //                List<string> tempList;
            //                foreach (DataRow tag in ds.Tables[3].Rows)
            //                {
            //                    string tagName = ODBCWrapper.Utils.GetSafeStr(tag["TagTypeName"]);
            //                    string tagValue = ODBCWrapper.Utils.GetSafeStr(tag["TagValueName"]);
            //                    if (epg.Tags.TryGetValue(tagName, out tempList))
            //                    {
            //                        tempList.Add(tagValue);
            //                    }
            //                    else
            //                    {
            //                        tempList = new List<string>() { tagValue };
            //                        epg.Tags.Add(tagName, tempList);
            //                    }
            //                }
            //            }

            //            res = epg;
            //        }
            //    }
            //}

            //return res;
        }
        
        // Get linear channel settings from catalog cache 
        public static void GetLinearChannelValues(List<EpgCB> lEpg, int groupID)
        {
            throw new NotImplementedException("GetLinearChannelValues should be implemented to new TVM logic");
            //try
            //{
            //    int days = TCMClient.Settings.Instance.GetValue<int>("CURRENT_REQUEST_DAYS_OFFSET");

            //    if (days == 0)
            //    {
            //        days = DAYS;
            //    }

            //    List<string> epgChannelIds = lEpg.Distinct().Select(item => item.ChannelID.ToString()).ToList<string>();
            //    Dictionary<string, LinearChannelSettings> linearChannelSettings = 
            //        CatalogCache.Instance().GetLinearChannelSettings(groupID, epgChannelIds);

            //    Parallel.ForEach(lEpg.Cast<EpgCB>(), currentElement =>
            //    {
            //        if (!linearChannelSettings.ContainsKey(currentElement.ChannelID.ToString()))
            //        {
            //            currentElement.SearchEndDate = currentElement.EndDate.AddDays(days);
            //        }
            //        else if (linearChannelSettings[currentElement.ChannelID.ToString()].EnableCatchUp)
            //        {
            //            currentElement.SearchEndDate =
            //                currentElement.EndDate.AddMinutes(linearChannelSettings[currentElement.ChannelID.ToString()].CatchUpBuffer);
            //        }
            //        else
            //        {
            //            currentElement.SearchEndDate = currentElement.EndDate;
            //        }
            //    });
            //}
            //catch (Exception ex)
            //{
            //    log.Error("Error - " + string.Format("Update EPGs threw an exception. (in GetLinearChannelValues). Exception={0};Stack={1}", ex.Message, ex.StackTrace), ex);
            //    throw ex;
            //}
        }

        #endregion

        #region Private Methods

        private static string GetTanslationType(string type, LanguageObj language)
        {
            if (language.IsDefault)
            {
                return type;
            }
            else
            {
                return string.Concat(type, "_", language.Code);
            }
        }

        protected static ulong GetEpgDocumentId(EpgCB epg)
        {
            return epg.EpgID;
        }
        
        protected static ulong GetEpgDocumentId(int epgId)
        {
            return (ulong)epgId;
        }

        private static string GetEpgGroupAliasStr(int nGroupID)
        {
            return string.Format("{0}_epg", nGroupID);
        }

        #endregion
    }
}
