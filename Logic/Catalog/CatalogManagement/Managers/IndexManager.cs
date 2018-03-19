using ApiObjects;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using CachingProvider.LayeredCache;
using Core.Catalog.Cache;
using Core.Catalog.Request;
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
        private const string MEDIA = "media";
        private const string CHANNEL = "channel";
        private const string EPG = "epg";
        private const string PERCOLATOR = ".percolator";

        #region Public Methods

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
                                    string serializedMedia = esSerializer.SerializeMediaObject(media, suffix);
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
                    result = true;
                    
                    foreach (LanguageObj lang in languages)
                    {
                        string type = GetTanslationType(MEDIA, lang);
                        ESDeleteResult deleteResult = esApi.DeleteDoc(index, type, assetId.ToString());
                        result = deleteResult.Ok && result;                        
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
                if (esApi.InsertRecord(index, type, channelId.ToString(), serializedChannel))
                {
                    if (UpdateChannelPercolator(groupId, new List<int>() { channelId }, channel))
                    {
                        result = true;
                    }
                    else
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

        public static bool DeleteChannel(int groupId, int channelId)
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
                string index = ElasticSearch.Common.Utils.GetGroupChannelIndex(groupId);
                ESDeleteResult deleteResult = esApi.DeleteDoc(index, CHANNEL, channelId.ToString());
                if (deleteResult.Ok)
                {
                    result = true;
                    if (DeleteChannelPercolator(groupId, new List<int>() { channelId }))
                    {
                        log.ErrorFormat("Delete channel percolator failed for Delete Channel");
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

        public static bool UpdateChannelPercolator(int groupId, List<int> channelIds, Channel channel = null)
        {
            bool result = false;
            ElasticSearchApi esApi = new ElasticSearch.Common.ElasticSearchApi();
            List<string> mediaAliases = esApi.GetAliases(groupId.ToString());
            List<string> epgAliases = esApi.GetAliases(string.Format("{0}_epg", groupId));
            try
            {
                if (mediaAliases != null && mediaAliases.Count > 0)
                {
                    if (channel != null)
                    {
                        result = UpdateChannelPercolator(esApi, channel, new List<int>() { groupId }, mediaAliases, epgAliases);
                    }
                    else
                    {                        
                        GroupManager groupManager = new GroupManager();
                        Group group = groupManager.GetGroup(groupId);
                        if (group == null || group.channelIDs == null || group.channelIDs.Count == 0)
                        {
                            return result;
                        }

                        result = true;
                        foreach (int channelId in channelIds)
                        {
                            Channel channelToUpdate = ChannelRepository.GetChannel(channelId, group);
                            if (channel != null)
                            {
                                result = result && UpdateChannelPercolator(esApi, channelToUpdate, group.m_nSubGroup, mediaAliases, epgAliases);
                            }
                        }
                    }
                }

                // Set invalidation for the entire group
                string invalidationKey = LayeredCacheKeys.GetGroupChannelsInvalidationKey(groupId);
                if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to invalidate key: {0} after UpdateChannelPercolator", invalidationKey);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Update Channel Percolator threw an exception. Exception={0};Stack={1}", ex.Message, ex.StackTrace), ex);
            }

            if (!result)
            {
                log.ErrorFormat("Update Channel Percolator with ids {0} failed", channelIds != null && channelIds.Count > 0 ? string.Join(",", channelIds) : string.Empty);
            }

            return result;
        }

        public static bool DeleteChannelPercolator(int groupId, List<int> channelIds)
        {
            bool result = false;
            ElasticSearch.Common.ElasticSearchApi esApi = new ElasticSearchApi();
            string mediaIndex = groupId.ToString();
            string epgIndex = string.Format("{0}_epg", groupId);
            ESDeleteResult deleteResult;

            try
            {
                bool epgExists = esApi.IndexExists(epgIndex);
                List<string> mediaAliases = esApi.GetAliases(mediaIndex);
                List<string> epgAliases = null;

                if (epgExists)
                {
                    epgAliases = esApi.GetAliases(epgIndex);
                }

                // If we found aliases to both, or if we don't have EPG at all
                if (mediaAliases != null && epgAliases != null &&
                    (!epgExists || (mediaAliases.Count > 0 && epgAliases.Count > 0)))
                {
                    result = true;
                }

                if (mediaAliases != null && mediaAliases.Count > 0)
                {
                    foreach (int channelID in channelIds)
                    {
                        foreach (string index in mediaAliases)
                        {
                            deleteResult = esApi.DeleteDoc(index, PERCOLATOR, channelID.ToString());
                            result &= deleteResult.Ok;

                            if (!deleteResult.Ok)
                            {
                                log.Error("Error - " + string.Concat("Could not delete channel from elasticsearch. ID=", channelID));
                            }
                        }
                    }
                }
                else
                {
                    log.Error("Error - " + string.Concat("Could not find indices for alias ", mediaIndex));
                }

                if (epgAliases != null && epgAliases.Count > 0)
                {
                    foreach (int channelId in channelIds)
                    {
                        foreach (string index in epgAliases)
                        {
                            deleteResult = esApi.DeleteDoc(index, PERCOLATOR, channelId.ToString());
                            result &= deleteResult.Ok;

                            if (!deleteResult.Ok)
                            {
                                log.Error("Error - " + string.Concat("Could not delete channel from elasticsearch. ID=", channelId));
                            }
                        }
                    }
                }
                else
                {
                    log.Error("Error - " + string.Concat("Could not find indices for alias ", epgIndex));
                }

                // Set invalidation for the entire group
                string invalidationKey = LayeredCacheKeys.GetGroupChannelsInvalidationKey(groupId);
                if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to invalidate key: {0} after UpdateChannelPercolator", invalidationKey);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Delete Channel Percolator threw an exception. Exception={0};Stack={1}", ex.Message, ex.StackTrace), ex);
            }

            if (!result)
            {
                log.ErrorFormat("Delete Channel Percolator with ids {0} failed", channelIds != null && channelIds.Count > 0 ? string.Join(",", channelIds) : string.Empty);
            }

            return result;
        }

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

        private static ulong GetEpgDocumentId(EpgCB epg)
        {
            return epg.EpgID;
        }

        private static ulong GetEpgDocumentId(int epgId)
        {
            return (ulong)epgId;
        }

        private static string GetEpgGroupAliasStr(int nGroupID)
        {
            return string.Format("{0}_epg", nGroupID);
        }

        private static bool UpdateChannelPercolator(ElasticSearchApi esApi, Channel channel, List<int> subGroupIds, List<string> mediaAliases, List<string> epgAliases)
        {
            bool result = false;
            if (channel != null && channel.m_nIsActive == 1)
            {
                bool isMedia = false;
                bool isEpg = false;

                string channelQuery = string.Empty;

                if (channel.m_nChannelTypeID == (int)ChannelType.KSQL)
                {
                    UnifiedSearchDefinitions definitions = BuildSearchDefinitions(channel, true);

                    isMedia = definitions.shouldSearchMedia;
                    isEpg = definitions.shouldSearchEpg;

                    ESUnifiedQueryBuilder unifiedQueryBuilder = new ESUnifiedQueryBuilder(definitions);
                    channelQuery = unifiedQueryBuilder.BuildSearchQueryString(true);
                }
                else
                {
                    isMedia = true;
                    ESMediaQueryBuilder mediaQueryParser = new ESMediaQueryBuilder()
                    {
                        QueryType = eQueryType.EXACT
                    };

                    mediaQueryParser.m_nGroupID = channel.m_nGroupID;
                    MediaSearchObj mediaSearchObject = BuildBaseChannelSearchObject(channel, subGroupIds);

                    mediaQueryParser.oSearchObject = mediaSearchObject;
                    channelQuery = mediaQueryParser.BuildSearchQueryString(true);
                }

                log.DebugFormat("Update channel with query: {0}", channelQuery);

                if (isMedia)
                {
                    foreach (string alias in mediaAliases)
                    {
                        result = esApi.AddQueryToPercolatorV2(alias, channel.m_nChannelID.ToString(), ref channelQuery);
                    }
                }

                if (isEpg)
                {
                    foreach (string alias in epgAliases)
                    {
                        result = esApi.AddQueryToPercolatorV2(alias, channel.m_nChannelID.ToString(), ref channelQuery);
                    }
                }
            }

            return result;
        }

        private static UnifiedSearchDefinitions BuildSearchDefinitions(Channel channel, bool useMediaTypes)
        {
            UnifiedSearchDefinitions definitions = new UnifiedSearchDefinitions();

            definitions.groupId = channel.m_nGroupID;

            if (useMediaTypes)
            {
                definitions.mediaTypes = new List<int>(channel.m_nMediaType);
            }

            if (channel.m_nMediaType != null)
            {
                // Nothing = all
                if (channel.m_nMediaType.Count == 0)
                {
                    definitions.shouldSearchEpg = true;
                    definitions.shouldSearchMedia = true;
                }
                else
                {
                    if (channel.m_nMediaType.Contains(Channel.EPG_ASSET_TYPE))
                    {
                        definitions.shouldSearchEpg = true;
                    }

                    // If there's anything besides EPG
                    if (channel.m_nMediaType.Count(type => type != Channel.EPG_ASSET_TYPE) > 0)
                    {
                        definitions.shouldSearchMedia = true;
                    }
                }
            }

            definitions.permittedWatchRules = GetPermittedWatchRules(channel.m_nGroupID);
            definitions.order = new OrderObj();

            definitions.shouldUseStartDateForMedia = false;
            definitions.shouldUseFinalEndDate = false;

            if (!string.IsNullOrEmpty(channel.filterQuery))
            {
                BooleanPhraseNode filterTree = null;
                Status parseStatus = BooleanPhraseNode.ParseSearchExpression(channel.filterQuery, ref filterTree);

                if (parseStatus.Code != (int)eResponseStatus.OK)
                {
                    throw new KalturaException(parseStatus.Message, parseStatus.Code);
                }
                else
                {
                    definitions.filterPhrase = filterTree;
                }

                GroupManager groupManager = new GroupManager();

                Group group = groupManager.GetGroup(channel.m_nParentGroupID);

                if (group == null)
                {
                    log.Error("Error - Could not load group from cache in GetGroupMedias");
                }

                BaseRequest dummyRequest = new BaseRequest()
                {
                    domainId = 0,
                    m_nGroupID = channel.m_nParentGroupID,
                    m_nPageIndex = 0,
                    m_nPageSize = 0,
                    m_oFilter = new Filter(),
                    m_sSiteGuid = string.Empty,
                    m_sUserIP = string.Empty
                };

                CatalogLogic.UpdateNodeTreeFields(dummyRequest,
                    ref definitions.filterPhrase, definitions, group, channel.m_nParentGroupID);
            }

            return definitions;
        }

        private static string GetPermittedWatchRules(int nGroupId, List<int> lSubGroup = null)
        {
            DataTable permittedWathRulesDt = CatalogDAL.GetPermittedWatchRulesByGroupId(nGroupId, lSubGroup);
            List<string> lWatchRulesIds = null;
            if (permittedWathRulesDt != null && permittedWathRulesDt.Rows.Count > 0)
            {
                lWatchRulesIds = new List<string>();
                foreach (DataRow permittedWatchRuleRow in permittedWathRulesDt.Rows)
                {
                    lWatchRulesIds.Add(ODBCWrapper.Utils.GetSafeStr(permittedWatchRuleRow["RuleID"]));
                }
            }

            string sRules = string.Empty;

            if (lWatchRulesIds != null && lWatchRulesIds.Count > 0)
            {
                sRules = string.Join(" ", lWatchRulesIds);
            }

            return sRules;
        }

        private static MediaSearchObj BuildBaseChannelSearchObject(Channel channel, List<int> lSubGroups)
        {
            ApiObjects.SearchObjects.MediaSearchObj searchObject = new ApiObjects.SearchObjects.MediaSearchObj();
            searchObject.m_nGroupId = channel.m_nGroupID;
            searchObject.m_bExact = true;
            searchObject.m_eCutWith = channel.m_eCutWith;

            if (channel.m_nMediaType != null)
            {
                searchObject.m_sMediaTypes = string.Join(";", channel.m_nMediaType.Select(type => type.ToString()));
            }

            searchObject.m_sPermittedWatchRules = GetPermittedWatchRules(channel.m_nGroupID, lSubGroups);
            searchObject.m_oOrder = new ApiObjects.SearchObjects.OrderObj();

            searchObject.m_bUseStartDate = false;
            searchObject.m_bUseFinalEndDate = false;

            CopySearchValuesToSearchObjects(ref searchObject, channel.m_eCutWith, channel.m_lChannelTags);
            return searchObject;
        }

        public static void CopySearchValuesToSearchObjects(ref MediaSearchObj searchObject, CutWith cutWith, List<SearchValue> channelSearchValues)
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
                            case ApiObjects.SearchObjects.CutWith.OR:
                                {
                                    m_dOr.Add(search);
                                    break;
                                }
                            case ApiObjects.SearchObjects.CutWith.AND:
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

        #endregion
    }
}
