using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using CachingProvider.LayeredCache;
using ConfigurationManager;
using Core.Api.Managers;
using Core.Catalog.Cache;
using Core.Catalog.Request;
using ElasticSearch.Common;
using ElasticSearch.Common.DeleteResults;
using ElasticSearch.Searcher;
using GroupsCacheManager;
using KLogMonitor;
using KlogMonitorHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tvinci.Core.DAL;

namespace Core.Catalog.CatalogManagement
{
    public class IndexManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const string MEDIA = "media";
        private const string CHANNEL = "channel";
        private const string EPG = "epg";
        public static readonly int DAYS = 7;
        private const string PERCOLATOR = ".percolator";

        private static readonly double EXPIRY_DATE = (ApplicationConfiguration.EPGDocumentExpiry.IntValue > 0) ? ApplicationConfiguration.EPGDocumentExpiry.IntValue : 7;

        #region Public Methods

        public static bool GetMetasAndTagsForMapping(int groupId, bool? doesGroupUsesTemplates, ref Dictionary<string, KeyValuePair<eESFieldType, string>> metas, ref List<string> tags,
            ref HashSet<string> metasToPad,
            BaseESSeralizer serializer, Group group = null, CatalogGroupCache catalogGroupCache = null, bool isEpg = false)
        {
            bool result = true;
            tags = new List<string>();
            metas = new Dictionary<string, KeyValuePair<eESFieldType, string>>();
            metasToPad = new HashSet<string>();

            if (!doesGroupUsesTemplates.HasValue)
            {
                doesGroupUsesTemplates = CatalogManager.DoesGroupUsesTemplates(groupId);
            }

            if (doesGroupUsesTemplates.Value && catalogGroupCache != null)
            {
                try
                {
                    HashSet<string> topicsToIgnore = Core.Catalog.CatalogLogic.GetTopicsToIgnoreOnBuildIndex();
                    tags = catalogGroupCache.TopicsMapBySystemNameAndByType.Where(x => x.Value.ContainsKey(ApiObjects.MetaType.Tag.ToString()) && !topicsToIgnore.Contains(x.Key)).Select(x => x.Key.ToLower()).ToList();

                    foreach (KeyValuePair<string, Dictionary<string, Topic>> topics in catalogGroupCache.TopicsMapBySystemNameAndByType)
                    {
                        //TODO anat ask Ira
                        if (topics.Value.Keys.Any(x => x != ApiObjects.MetaType.Tag.ToString() && x != ApiObjects.MetaType.ReleatedEntity.ToString()))
                        {
                            string nullValue = string.Empty;
                            eESFieldType metaType;

                            if (isEpg)
                            {
                                metaType = eESFieldType.STRING;
                            }
                            else
                            {
                                ApiObjects.MetaType topicMetaType = CatalogManager.GetTopicMetaType(topics.Value);
                                serializer.GetMetaType(topicMetaType, out metaType, out nullValue);
                            }

                            if (!metas.ContainsKey(topics.Key.ToLower()))
                            {
                                metas.Add(topics.Key.ToLower(), new KeyValuePair<eESFieldType, string>(metaType, nullValue));
                            }
                            else
                            {
                                log.ErrorFormat("Duplicate topic found for group {0} name {1}", groupId, topics.Key.ToLower());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Failed BuildIndex for groupId: {0} because CatalogGroupCache", groupId), ex);
                    return false;
                }
            }
            else if (group != null)
            {
                try
                {
                    if (group.m_oEpgGroupSettings != null && group.m_oEpgGroupSettings.m_lTagsName != null)
                    {
                        foreach (var item in group.m_oEpgGroupSettings.m_lTagsName)
                        {
                            if (!tags.Contains(item.ToLower()))
                            {
                                tags.Add(item.ToLower());
                            }
                        }
                    }

                    if (group.m_oGroupTags != null)
                    {
                        foreach (var item in group.m_oGroupTags.Values)
                        {
                            if (!tags.Contains(item.ToLower()))
                            {
                                tags.Add(item.ToLower());
                            }
                        }
                    }

                    if (group.m_oMetasValuesByGroupId != null)
                    {
                        foreach (Dictionary<string, string> metaMap in group.m_oMetasValuesByGroupId.Values)
                        {
                            foreach (KeyValuePair<string, string> meta in metaMap)
                            {
                                string nullValue = string.Empty;
                                eESFieldType metaType;

                                if (isEpg)
                                {
                                    metaType = eESFieldType.STRING;
                                }
                                else
                                {
                                    serializer.GetMetaType(meta.Key, out metaType, out nullValue);
                                }

                                if (!metas.ContainsKey(meta.Value.ToLower()))
                                {
                                    metas.Add(meta.Value.ToLower(), new KeyValuePair<eESFieldType, string>(metaType, nullValue));
                                }
                                else
                                {
                                    log.WarnFormat("Duplicate media meta found for group {0} name {1}", groupId, meta.Value);
                                }
                            }
                        }
                    }

                    if (group.m_oEpgGroupSettings != null && group.m_oEpgGroupSettings.m_lMetasName != null)
                    {
                        foreach (string epgMeta in group.m_oEpgGroupSettings.m_lMetasName)
                        {
                            string nullValue = string.Empty;
                            eESFieldType metaType;

                            if (isEpg)
                            {
                                metaType = eESFieldType.STRING;
                            }
                            else
                            {
                                serializer.GetMetaType(epgMeta, out metaType, out nullValue);
                            }

                            if (!metas.ContainsKey(epgMeta.ToLower()))
                            {
                                metas.Add(epgMeta.ToLower(), new KeyValuePair<eESFieldType, string>(metaType, nullValue));
                            }
                            else
                            {
                                var mediaMetaType = metas[epgMeta].Key;

                                // If the metas is numeric for media and it exists also for epg, we will have problems with sorting 
                                // (since epg metas are string and there will be a type mismatch)
                                // the solution is to add another field of a padded string to the indices and sort by it
                                if (mediaMetaType == eESFieldType.INTEGER ||
                                    mediaMetaType == eESFieldType.DOUBLE ||
                                    mediaMetaType == eESFieldType.LONG)
                                {
                                    metasToPad.Add(epgMeta.ToLower());
                                }
                                else
                                {
                                    log.WarnFormat("Duplicate epg meta found for group {0} name {1}", groupId, epgMeta);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Failed get metas and tags for mapping for group {0} ex = {1}", groupId, ex);
                }
            }

            return result;
        }

        public static Dictionary<int, Dictionary<int, Media>> GetGroupMedias(int groupId, long mediaId)
        {
            //dictionary contains medias such that first key is media_id, which returns a dictionary with a key language_id and value Media object.
            //E.g. mediaTranslations[123][2] --> will return media 123 of the hebrew language
            Dictionary<int, Dictionary<int, Media>> mediaTranslations = new Dictionary<int, Dictionary<int, Media>>();

            //temporary media dictionary
            Dictionary<int, Media> medias = new Dictionary<int, Media>();

            try
            {

                if (CatalogManager.DoesGroupUsesTemplates(groupId))
                {
                    return AssetManager.GetMediaForElasticSearchIndex(groupId, mediaId);
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
                Utils.BuildMediaFromDataSet(ref mediaTranslations, ref medias, group, dataSet);

                // get media update dates
                DataTable updateDates = CatalogDAL.Get_MediaUpdateDate(new List<int>() { (int)mediaId });
            }
            catch (Exception ex)
            {
                log.Error("Media Exception", ex);
            }

            return mediaTranslations;
        }

        public static bool UpsertMedia(int groupId, long assetId)
        {
            bool result = false;

            if (assetId <= 0)
            {
                log.WarnFormat("Received media request of invalid media id {0} when calling UpsertMedia", assetId);
                return result;
            }
            
            Dictionary<int, LanguageObj> languagesMap = null;
            if (CatalogManager.DoesGroupUsesTemplates(groupId))
            { 
                CatalogGroupCache catalogGroupCache;
                if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling UpsertMedia", groupId);
                    return false;
                }

                languagesMap = new Dictionary<int, LanguageObj>(catalogGroupCache.LanguageMapById);
            }
            else
            {
                GroupManager groupManager = new GroupManager();                
                Group group = groupManager.GetGroup(groupId);
                if (group == null)
                {
                    log.ErrorFormat("Could not load group {0} in upsertMedia", groupId);
                    return false;
                }

                List<LanguageObj> languages = group.GetLangauges();
                languagesMap = languages.ToDictionary(x => x.ID, x => x);
            }

            try
            {
                ESSerializerV2 esSerializer = new ESSerializerV2();
                ElasticSearchApi esApi = new ElasticSearchApi();

                //Create Media Object
                Dictionary<int, Dictionary<int, Media>> mediaDictionary = GetGroupMedias(groupId, assetId);
                if (mediaDictionary != null && mediaDictionary.Count > 0 && mediaDictionary.ContainsKey((int)assetId))
                {
                    foreach (int languageId in mediaDictionary[(int)assetId].Keys)
                    {
                        LanguageObj language = languagesMap.ContainsKey(languageId) ? languagesMap[languageId] : null;
                        if (language != null)
                        {
                            string suffix = null;
                            if (!language.IsDefault)
                            {
                                suffix = language.Code;
                            }

                            Media media = mediaDictionary[(int)assetId][languageId];
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
                else
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Upsert Media threw an exception. Exception={0};Stack={1}", ex.Message, ex.StackTrace), ex);
            }

            return result;
        }
        
        public static void PadMediaMetas(HashSet<string> metasToPad, Media media)
        {
            if (metasToPad != null && metasToPad.Count > 0 && media.m_dMeatsValues != null)
            {
                foreach (var meta in media.m_dMeatsValues.ToList())
                {
                    if (metasToPad.Contains(meta.Key.ToLower()))
                    {
                        string metaValue = meta.Value;

                        metaValue = PadValue(metaValue);

                        media.m_dMeatsValues[string.Format("padded_{0}", meta.Key.ToLower())] = metaValue;
                    }
                }
            }
        }

        public static void PadEPGMetas(HashSet<string> metasToPad, EpgCB epg)
        {
            if (metasToPad != null && metasToPad.Count > 0 && epg.Metas != null)
            {
                foreach (var meta in epg.Metas.ToList())
                {
                    if (meta.Value != null && meta.Value.Count > 0 &&
                        metasToPad.Contains(meta.Key.ToLower()))
                    {
                        string metaValue = meta.Value.First();

                        metaValue = PadValue(metaValue);

                        epg.Metas[string.Format("padded_{0}", meta.Key.ToLower())] = new List<string>() { metaValue };
                    }
                }
            }
        }

        public static string PadValue(string metaValue)
        {
            if (string.IsNullOrEmpty(metaValue))
            {
                return metaValue;
            }

            double parsedDouble;
            int parsedInt;

            // only for doubles and not for integers - get only the first two decimal digits
            if (double.TryParse(metaValue, out parsedDouble) && !int.TryParse(metaValue, out parsedInt))
            {
                metaValue = string.Format("{0:N2}", parsedDouble);
            }

            metaValue = metaValue.PadLeft(7, '0');

            return metaValue;
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

            List<LanguageObj> languages = null;
            bool doesGroupUsesTemplates = CatalogManager.DoesGroupUsesTemplates(groupId);
            if (doesGroupUsesTemplates)
            {
                CatalogGroupCache catalogGroupCache;
                if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling DeleteMedia", groupId);
                    return false;
                }

                languages = catalogGroupCache.LanguageMapById.Values.ToList();
            }
            else
            {
                GroupManager groupManager = new GroupManager();
                Group group = groupManager.GetGroup(groupId);
                if (group == null)
                {
                    log.ErrorFormat("Could not load group {0} in upsertMedia", groupId);
                    return false;
                }

                languages = group.GetLangauges();
            }

            try
            {
                if (languages != null && languages.Count > 0)
                {
                    result = true;

                    foreach (LanguageObj lang in languages)
                    {
                        string type = GetTanslationType(MEDIA, lang);
                        ESDeleteResult deleteResult = esApi.DeleteDoc(index, type, assetId.ToString());

                        if (!deleteResult.Found)
                        {
                            log.WarnFormat("IndexManager - DeleteMedia Delete request: delete media with ID {0} and language {1} not found", assetId, lang.Code);
                        }
                        else
                        {
                            if (!deleteResult.Ok)
                            {
                                log.ErrorFormat("IndexManager - DeleteMedia error: Could not delete media from ES. Media id={0} language={1}", assetId, lang.Code);
                            }

                            result = deleteResult.Ok && result;
                        }
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

        public static bool UpsertChannel(int groupId, int channelId, Channel channel = null, long userId = 0)
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
                    // isAllowedToViewInactiveAssets = true because only operator can cause upsert of channel
                    GenericResponse<Channel> response = ChannelManager.GetChannelById(groupId, channelId, true, userId);
                    if (response != null && response.Status != null && response.Status.Code != (int)eResponseStatus.OK)
                    {
                        return result;
                    }

                    channel = response.Object;
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
                    result = true;                    
                    if ((channel.m_nChannelTypeID != (int)ChannelType.Manual || (channel.m_lChannelTags != null && channel.m_lChannelTags.Count > 0))
                            && !UpdateChannelPercolator(groupId, new List<int>() { channelId }, channel))
                    {
                        log.ErrorFormat("Update channel percolator failed for Upsert Channel with channelId: {0}", channelId);                        
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Upsert Channel threw an exception. channelId: {0}, Exception={1};Stack={2}", channelId, ex.Message, ex.StackTrace), ex);
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
                        result = UpdateChannelPercolator(esApi, channel, groupId, mediaAliases, epgAliases);
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

                            if (channelToUpdate != null)
                            {
                                result = result && UpdateChannelPercolator(esApi, channelToUpdate, groupId, mediaAliases, epgAliases);
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

        public static bool UpsertProgram(int groupId, List<int> epgIds)
        {
            bool result = true;

            try
            {
                ESSerializerV2 esSerializer = new ESSerializerV2();
                ElasticSearchApi esApi = new ElasticSearchApi();

                bool doesGroupUsesTemplates = CatalogManager.DoesGroupUsesTemplates(groupId);
                CatalogGroupCache catalogGroupCache = null;
                Group group = null;
                if (doesGroupUsesTemplates)
                {
                    if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                    {
                        log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling UpsertProgram", groupId);
                        return false;
                    }
                }
                else
                {
                    group = GroupsCache.Instance().GetGroup(groupId);
                    if (group == null)
                    {
                        log.ErrorFormat("Couldn't get group {0} when calling UpsertProgram", groupId);
                        return false;
                    }
                }

                // dictionary contains all language ids and its  code (string)
                List<LanguageObj> languages = doesGroupUsesTemplates ? catalogGroupCache.LanguageMapById.Values.ToList() : group.GetLangauges();
                List<string> languageCodes = new List<string>();

                if (languages != null)
                {
                    languageCodes = languages.Select(p => p.Code.ToLower()).ToList<string>();
                }
                else
                {
                    // return false; // perhaps?
                    log.Debug("Warning - " + string.Format("Group {0} has no languages defined.", groupId));
                }

                List<EpgCB> epgObjects = new List<EpgCB>();

                if (epgIds.Count == 1)
                {
                    epgObjects = GetEpgPrograms(groupId, epgIds[0], languageCodes);
                }
                else
                {
                    Task<List<EpgCB>>[] programsTasks = new Task<List<EpgCB>>[epgIds.Count];
                    ContextData cd = new ContextData();

                    //open task factory and run GetEpgProgram on different threads
                    //wait to finish
                    //bulk insert
                    for (int i = 0; i < epgIds.Count; i++)
                    {
                        programsTasks[i] = Task.Run<List<EpgCB>>(() =>
                        {
                            cd.Load();
                            return GetEpgPrograms(groupId, epgIds[i], languageCodes);
                        });
                    }

                    Task.WaitAll(programsTasks);

                    epgObjects = programsTasks.SelectMany(t => t.Result).Where(t => t != null).ToList();
                }

                // GetLinear Channel Values 
                GetLinearChannelValues(epgObjects, groupId);

                // TODO - Lior, remove these 5 lines below - used only to currently support linear media id search on elastic search
                List<string> epgChannelIds = epgObjects.Select(item => item.ChannelID.ToString()).ToList<string>();
                Dictionary<string, LinearChannelSettings> linearChannelSettings = CatalogCache.Instance().GetLinearChannelSettings(groupId, epgChannelIds);
                if (linearChannelSettings == null)
                {
                    linearChannelSettings = new Dictionary<string, LinearChannelSettings>();
                }

                if (epgObjects != null)
                {
                    if (epgObjects.Count == 0)
                    {
                        log.WarnFormat("Attention - when updating EPG, epg list is empty for IDs = {0}",
                            string.Join(",", epgIds));
                        result = true;
                    }
                    else
                    {
                        List<ESBulkRequestObj<ulong>> bulkRequests = new List<ESBulkRequestObj<ulong>>();
                        List<KeyValuePair<string, string>> invalidResults = null;

                        #region Get Linear Channels Regions

                        Dictionary<long, List<int>> linearChannelsRegionsMapping = null;
                        if (group.isRegionalizationEnabled)
                        {
                            linearChannelsRegionsMapping = CatalogManager.GetLinearMediaRegions(groupId);
                        }

                        #endregion

                        // Temporarily - assume success
                        bool temporaryResult = true;

                        // Create dictionary by languages
                        foreach (LanguageObj language in languages)
                        {
                            // Filter programs to current language
                            List<EpgCB> currentLanguageEpgs = epgObjects.Where(epg =>
                                epg.Language.ToLower() == language.Code.ToLower() || (language.IsDefault && string.IsNullOrEmpty(epg.Language))).ToList();

                            if (currentLanguageEpgs != null && currentLanguageEpgs.Count > 0)
                            {
                                string alias = string.Format("{0}_epg", groupId);

                                // Create bulk request object for each program
                                foreach (EpgCB epg in currentLanguageEpgs)
                                {
                                    string suffix = null;

                                    if (!language.IsDefault)
                                    {
                                        suffix = language.Code;
                                    }

                                    // TODO - Lior, remove all this if - used only to currently support linear media id search on elastic search
                                    if (linearChannelSettings.ContainsKey(epg.ChannelID.ToString()))
                                    {
                                        epg.LinearMediaId = linearChannelSettings[epg.ChannelID.ToString()].LinearMediaId;
                                    }

                                    if (epg.LinearMediaId > 0 && linearChannelsRegionsMapping != null && linearChannelsRegionsMapping.ContainsKey(epg.LinearMediaId))
                                    {
                                        epg.regions = linearChannelsRegionsMapping[epg.LinearMediaId];
                                    }

                                    string serializedEpg = esSerializer.SerializeEpgObject(epg, suffix);
                                    string ttl = string.Format("{0}m", Math.Ceiling((epg.EndDate.AddDays(EXPIRY_DATE) - DateTime.UtcNow).TotalMinutes));

                                    bulkRequests.Add(new ESBulkRequestObj<ulong>()
                                    {
                                        docID = epg.EpgID,
                                        index = alias,
                                        type = GetTanslationType(EPG, language),
                                        Operation = eOperation.index,
                                        document = serializedEpg,
                                        routing = epg.StartDate.ToUniversalTime().ToString("yyyyMMdd"),
                                        ttl = ttl
                                    });

                                    int sizeOfBulk = ApplicationConfiguration.ElasticSearchHandlerConfiguration.BulkSize.IntValue;
                                    if (bulkRequests.Count > sizeOfBulk)
                                    {
                                        // send request to ES API
                                        invalidResults = esApi.CreateBulkRequest(bulkRequests);

                                        if (invalidResults != null && invalidResults.Count > 0)
                                        {
                                            foreach (var invalidResult in invalidResults)
                                            {
                                                log.Error("Error - " + string.Format("Could not update EPG in ES. GroupID={0};Type={1};EPG_ID={2};error={3};",
                                                                                     groupId, EPG, invalidResult.Key, invalidResult.Value));
                                            }

                                            result = false;
                                            temporaryResult = false;
                                        }
                                        else
                                        {
                                            temporaryResult &= true;
                                            EpgAssetManager.InvalidateEpgs(groupId, bulkRequests.Select(x => (long)x.docID), doesGroupUsesTemplates);
                                            
                                        }

                                        bulkRequests.Clear();
                                    }
                                }
                            }
                        }

                        if (bulkRequests.Count > 0)
                        {
                            // send request to ES API
                            invalidResults = esApi.CreateBulkRequest(bulkRequests);

                            if (invalidResults != null && invalidResults.Count > 0)
                            {
                                foreach (var invalidResult in invalidResults)
                                {
                                    log.Error("Error - " + string.Format("Could not update EPG in ES. GroupID={0};Type={1};EPG_ID={2};error={3};",
                                                                         groupId, EPG, invalidResult.Key, invalidResult.Value));
                                }

                                result = false;
                                temporaryResult = false;
                            }
                            else
                            {
                                temporaryResult &= true;
                                EpgAssetManager.InvalidateEpgs(groupId, bulkRequests.Select(x => (long)x.docID), doesGroupUsesTemplates);
                            }

                            result = temporaryResult;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error: Update EPGs threw an exception. Exception={0}", ex);
                throw ex;
            }

            return result;
        }

        public static bool DeleteProgram(int groupId, List<long> epgIds)
        {
            bool result = false;
            //result &= Core.Catalog.CatalogManagement.IndexManager.DeleteEpg(groupId, id);
            bool doesGroupUsesTemplates = CatalogManager.DoesGroupUsesTemplates(groupId);

            if (epgIds != null & epgIds.Count > 0)
            {
                CatalogGroupCache catalogGroupCache = null;
                Group group = null;
                if (doesGroupUsesTemplates)
                {
                    if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                    {
                        log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling UpdateEpg", groupId);
                        return false;
                    }
                }
                else
                {
                    group = GroupsCache.Instance().GetGroup(groupId);
                    if (group == null)
                    {
                        log.ErrorFormat("Couldn't get group {0}", groupId);
                        return false;
                    }
                }

                // dictionary contains all language ids and its  code (string)
                List<LanguageObj> languages = doesGroupUsesTemplates ? catalogGroupCache.LanguageMapById.Values.ToList() : group.GetLangauges();

                string alias = string.Format("{0}_epg", groupId);

                ESTerms terms = new ESTerms(true)
                {
                    Key = "epg_id"
                };

                terms.Value.AddRange(epgIds.Select(id => id.ToString()));

                ESQuery query = new ESQuery(terms);
                string queryString = query.ToString();

                ElasticSearchApi esApi = new ElasticSearch.Common.ElasticSearchApi();
                foreach (var lang in languages)
                {
                    string type = GetTanslationType(EPG, lang);
                    esApi.DeleteDocsByQuery(alias, type, ref queryString);
                }

                result = true;
            }
            
            // support for old invalidation keys
            if (result)
            {
                // invalidate epg's for OPC and NON-OPC accounts
                EpgAssetManager.InvalidateEpgs(groupId, epgIds, doesGroupUsesTemplates);
            }

            return result;
        }

        public static MediaSearchObj BuildBaseChannelSearchObject(Channel channel)
        {
            ApiObjects.SearchObjects.MediaSearchObj searchObject = new ApiObjects.SearchObjects.MediaSearchObj();
            searchObject.m_nGroupId = channel.m_nGroupID;
            searchObject.m_bExact = true;
            searchObject.m_eCutWith = channel.m_eCutWith;

            if (channel.m_nMediaType != null)
            {
                searchObject.m_sMediaTypes = string.Join(";", channel.m_nMediaType.Select(type => type.ToString()));
            }

            searchObject.m_sPermittedWatchRules = GetPermittedWatchRules(channel.m_nGroupID);
            searchObject.m_oOrder = new ApiObjects.SearchObjects.OrderObj();

            searchObject.m_bUseStartDate = false;
            searchObject.m_bUseFinalEndDate = false;

            CopySearchValuesToSearchObjects(ref searchObject, channel.m_eCutWith, channel.m_lChannelTags);

            // If it is a manual channel without media, make it an empty request
            if (channel.m_nChannelTypeID == (int)ChannelType.Manual &&
                (channel.m_lChannelTags == null || channel.m_lChannelTags.Count == 0))
            {
                searchObject.m_eCutWith = CutWith.AND;
                searchObject.m_eFilterTagsAndMetasCutWith = CutWith.AND;
                searchObject.m_lFilterTagsAndMetas = new List<SearchValue>()
                {
                    new SearchValue("media_id", "0")
                    {
                        m_eInnerCutWith = CutWith.AND,
                        m_lValue = new List<string>()
                        {
                            "0"
                        }
                    }
                };
            }

            return searchObject;
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

        private static bool UpdateChannelPercolator(ElasticSearchApi esApi, Channel channel, int groupId, List<string> mediaAliases, List<string> epgAliases)
        {
            bool result = false;
            if (channel != null)
            {
                bool isMedia = false;
                bool isEpg = false;

                string channelQuery = string.Empty;

                bool doesGroupUsesTemplates = CatalogManager.DoesGroupUsesTemplates(groupId);
                
                if ((channel.m_nChannelTypeID == (int)ChannelType.KSQL) ||
                    (channel.m_nChannelTypeID == (int)ChannelType.Manual && doesGroupUsesTemplates && channel.AssetUserRuleId > 0))
                {
                    if (channel.m_nChannelTypeID == (int)ChannelType.Manual && channel.AssetUserRuleId > 0)
                    {
                        StringBuilder builder = new StringBuilder();
                        builder.Append("(or ");

                        foreach (var item in channel.m_lChannelTags)
                        {
                            builder.AppendFormat("media_id='{0}' ", item.m_lValue);
                        }

                        builder.Append(")");

                        channel.filterQuery = builder.ToString();
                    }

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
                    MediaSearchObj mediaSearchObject = BuildBaseChannelSearchObject(channel);

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

        public static UnifiedSearchDefinitions BuildSearchDefinitions(Channel channel, bool useMediaTypes)
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

            if (channel.AssetUserRuleId.HasValue && channel.AssetUserRuleId.Value > 0)
            {
                var assetUserRule = AssetUserRuleManager.GetAssetUserRuleByRuleId(channel.m_nGroupID, channel.AssetUserRuleId.Value);

                if (assetUserRule != null && assetUserRule.Status != null && assetUserRule.Status.Code == (int)eResponseStatus.OK && assetUserRule.Object != null)
                {
                    BooleanPhraseNode phrase = null;

                    var rulesIds = new List<long>();
                    string queryString = string.Empty;

                    UnifiedSearchDefinitionsBuilder.GetQueryStringFromAssetUserRules(new List<ApiObjects.Rules.AssetUserRule>()
                    {
                        assetUserRule.Object
                    },
                    out rulesIds,
                    out queryString);

                    BooleanPhrase.ParseSearchExpression(queryString, ref phrase);

                    CatalogLogic.UpdateNodeTreeFields(dummyRequest, ref phrase, definitions, group, group.m_nParentGroupID);

                    definitions.assetUserRuleFilterPhrase = phrase;
                }
            }

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

                CatalogLogic.UpdateNodeTreeFields(dummyRequest,
                    ref definitions.filterPhrase, definitions, group, channel.m_nParentGroupID);
            }

            return definitions;
        }

        private static string GetPermittedWatchRules(int nGroupId)
        {
            List<string> groupPermittedWatchRules = CatalogLogic.GetGroupPermittedWatchRules(nGroupId);
            string sRules = string.Empty;

            if (groupPermittedWatchRules != null && groupPermittedWatchRules.Count > 0)
            {
                sRules = string.Join(" ", groupPermittedWatchRules);
            }

            return sRules;
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

        private static List<EpgCB> GetEpgPrograms(int groupId, int epgId, List<string> languages, EpgBL.BaseEpgBL epgBL = null)
        {
            List<EpgCB> results = new List<EpgCB>();

            // If no language was received - just get epg program by old method
            if (languages == null || languages.Count == 0)
            {
                EpgCB program = GetEpgProgram(groupId, epgId);

                results.Add(program);
            }
            else
            {
                try
                {
                    if (epgBL == null)
                    {
                        epgBL = EpgBL.Utils.GetInstance(groupId);
                    }

                    ulong uEpgID = (ulong)epgId;
                    results = epgBL.GetEpgCB(uEpgID, languages);
                }
                catch (Exception ex)
                {
                    log.Error("Error (GetEpgProgram) - " + string.Format("epg:{0}, msg:{1}, st:{2}", epgId, ex.Message, ex.StackTrace), ex);
                }
            }

            return results;
        }

        private static EpgCB GetEpgProgram(int nGroupID, int nEpgID)
        {
            EpgCB res = null;

            DataSet ds = Tvinci.Core.DAL.EpgDal.GetEpgProgramDetails(nGroupID, nEpgID);

            if (ds != null && ds.Tables != null)
            {
                if (ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                {
                    //Basic Details
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        EpgCB epg = new EpgCB();
                        epg.ChannelID = ODBCWrapper.Utils.GetIntSafeVal(row["EPG_CHANNEL_ID"]);
                        epg.EpgID = ODBCWrapper.Utils.GetUnsignedLongSafeVal(row["ID"]);
                        epg.GroupID = ODBCWrapper.Utils.GetIntSafeVal(row["GROUP_ID"]);
                        epg.IsActive = (ODBCWrapper.Utils.GetIntSafeVal(row["IS_ACTIVE"]) == 1) ? true : false;
                        epg.Description = ODBCWrapper.Utils.GetSafeStr(row["DESCRIPTION"]);
                        epg.Name = ODBCWrapper.Utils.GetSafeStr(row["NAME"]);
                        if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row["START_DATE"])))
                        {
                            epg.StartDate = ODBCWrapper.Utils.GetDateSafeVal(row["START_DATE"]);
                        }
                        if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row["END_DATE"])))
                        {
                            epg.EndDate = ODBCWrapper.Utils.GetDateSafeVal(row["END_DATE"]);
                        }
                        epg.Crid = ODBCWrapper.Utils.GetSafeStr(row["crid"]);

                        //Metas
                        if (ds.Tables.Count >= 3 && ds.Tables[2] != null && ds.Tables[2].Rows != null && ds.Tables[2].Rows.Count > 0)
                        {
                            List<string> tempList;

                            foreach (DataRow meta in ds.Tables[2].Rows)
                            {
                                string metaName = ODBCWrapper.Utils.GetSafeStr(meta["name"]);
                                string metaValue = ODBCWrapper.Utils.GetSafeStr(meta["value"]);

                                if (epg.Metas.TryGetValue(metaName, out tempList))
                                {
                                    tempList.Add(metaValue);
                                }
                                else
                                {
                                    tempList = new List<string>() { metaValue };
                                    epg.Metas.Add(metaName, tempList);
                                }
                            }
                        }

                        //Tags
                        if (ds.Tables.Count >= 4 && ds.Tables[3] != null && ds.Tables[3].Rows != null && ds.Tables[3].Rows.Count > 0)
                        {
                            List<string> tempList;
                            foreach (DataRow tag in ds.Tables[3].Rows)
                            {
                                string tagName = ODBCWrapper.Utils.GetSafeStr(tag["TagTypeName"]);
                                string tagValue = ODBCWrapper.Utils.GetSafeStr(tag["TagValueName"]);
                                if (epg.Tags.TryGetValue(tagName, out tempList))
                                {
                                    tempList.Add(tagValue);
                                }
                                else
                                {
                                    tempList = new List<string>() { tagValue };
                                    epg.Tags.Add(tagName, tempList);
                                }
                            }
                        }

                        res = epg;
                    }
                }
            }

            return res;
        }

        private static void GetLinearChannelValues(List<EpgCB> lEpg, int groupID)
        {
            try
            {
                int days = ApplicationConfiguration.CatalogLogicConfiguration.CurrentRequestDaysOffset.IntValue;

                if (days == 0)
                {
                    days = DAYS;
                }

                List<string> epgChannelIds = lEpg.Distinct().Select(item => item.ChannelID.ToString()).ToList<string>();
                Dictionary<string, LinearChannelSettings> linearChannelSettings = CatalogCache.Instance().GetLinearChannelSettings(groupID, epgChannelIds);

                Parallel.ForEach(lEpg.Cast<EpgCB>(), currentElement =>
                {
                    if (!linearChannelSettings.ContainsKey(currentElement.ChannelID.ToString()))
                    {
                        currentElement.SearchEndDate = currentElement.EndDate.AddDays(days);
                    }
                    else if (linearChannelSettings[currentElement.ChannelID.ToString()].EnableCatchUp)
                    {
                        currentElement.SearchEndDate =
                            currentElement.EndDate.AddMinutes(linearChannelSettings[currentElement.ChannelID.ToString()].CatchUpBuffer);
                    }
                    else
                    {
                        currentElement.SearchEndDate = currentElement.EndDate;
                    }
                });
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Update EPGs threw an exception. (in GetLinearChannelValues). Exception={0};Stack={1}", ex.Message, ex.StackTrace), ex);
                throw ex;
            }
        }
        #endregion
    }
}
