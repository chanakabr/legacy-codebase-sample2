using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects.SearchObjects;
using GroupsCacheManager;
using System.Data;
using System.Threading.Tasks;
using KLogMonitor;
using System.Reflection;
using ApiObjects.Response;
using KlogMonitorHelper;
using Core.Catalog.CatalogManagement;
using ConfigurationManager;
using Core.Catalog;

namespace ElasticSearchHandler.IndexBuilders
{
    public class MediaIndexBuilderV2 : AbstractIndexBuilder
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Consts

        // Basic TCM configurations for indexing - number of shards/replicas, size of bulks 
        long mediaPageSize = ApplicationConfiguration.Current.ElasticSearchHandlerConfiguration.MediaPageSize.Value;

        protected const string VERSION = "2";
        #endregion

        #region Ctor

        public MediaIndexBuilderV2(int groupID)
            : base(groupID)
        {
        }

        #endregion

        #region Interface Methods

        public override bool BuildIndex()
        {
            #region insert medias

            bool doesGroupUsesTemplates = CatalogManager.Instance.DoesGroupUsesTemplates(groupId);
            CatalogGroupCache catalogGroupCache;
            Group group;
            List<ApiObjects.LanguageObj> languages;
            ApiObjects.LanguageObj defaultLanguage;
            GetGroupData(out catalogGroupCache, out group, out languages, out defaultLanguage);

            string newIndexName = _IndexManager.SetupMediaIndex();
            log.DebugFormat("Start GetGroupMediasTotal for group {0}", groupId);

            if (doesGroupUsesTemplates)
            {
                Dictionary<int, Dictionary<int, Media>> groupMedias;
                long nextId = 0;
                mediaPageSize = mediaPageSize == 0 ? 1000 : mediaPageSize;

                while (true)
                {
                    System.Collections.Concurrent.ConcurrentDictionary<int, Dictionary<int, Media>> opcGroupMedias = 
                        ElasticsearchTasksCommon.Utils.GetGroupMediasTotalForOPCAccount(groupId, 0, nextId, mediaPageSize);
                    if (opcGroupMedias == null || opcGroupMedias.Count == 0)
                        break;

                    groupMedias = opcGroupMedias.ToDictionary(x => x.Key, x => x.Value);
                    _IndexManager.InsertMedias(groupMedias, newIndexName);
                    var nextNextId = groupMedias.Max(x => x.Key);
                    if (nextId == nextNextId)
                        break;

                    nextId = nextNextId;
                }
            }
            else
            {
                // Get ALL media in group
                Dictionary<int, Dictionary<int, Media>> groupMedias = ElasticsearchTasksCommon.Utils.GetGroupMediasTotal(groupId, 0);
                _IndexManager.InsertMedias(groupMedias, newIndexName);
            }

            #endregion

            #region insert channel queries

            HashSet<int> channelIds = new HashSet<int>();
            if (!doesGroupUsesTemplates)
            {
                channelIds = group.channelIDs;
            }

            _IndexManager.AddChannelsPercolatorsToIndex(channelIds, newIndexName);

            #endregion

            // Switch index alias + Delete old indices handling
            _IndexManager.PublishMediaIndex(newIndexName, this.SwitchIndexAlias, this.DeleteOldIndices);

            return true;
        }

        private void GetGroupData(out CatalogGroupCache catalogGroupCache, out Group group, 
            out List<ApiObjects.LanguageObj> languages, out ApiObjects.LanguageObj defaultLanguage)
        {
            catalogGroupCache = null;
            group = null;
            GroupManager groupManager = new GroupManager();

            bool doesGroupUsesTemplates = CatalogManager.Instance.DoesGroupUsesTemplates(groupId);
            if (doesGroupUsesTemplates)
            {
                if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling BuildIndex", groupId);
                }

                languages = catalogGroupCache.LanguageMapById.Values.ToList();
                defaultLanguage = catalogGroupCache.GetDefaultLanguage();
            }
            else
            {
                groupManager.RemoveGroup(groupId);
                group = groupManager.GetGroup(groupId);

                if (group == null)
                {
                    log.ErrorFormat("Could not load group {0} in media index builder", groupId);
                }

                languages = group.GetLangauges();
                defaultLanguage = group.GetGroupDefaultLanguage();
            }
        }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        private static ApiObjects.SearchObjects.MediaSearchObj BuildBaseChannelSearchObject(Channel channel)
        {
            ApiObjects.SearchObjects.MediaSearchObj searchObject = new ApiObjects.SearchObjects.MediaSearchObj();
            searchObject.m_nGroupId = channel.m_nGroupID;
            searchObject.m_bExact = true;
            searchObject.m_eCutWith = channel.m_eCutWith;

            if (channel.m_nMediaType != null)
            {
                searchObject.m_sMediaTypes = string.Join(";", channel.m_nMediaType.Select(type => type.ToString()));
            }

            searchObject.m_sPermittedWatchRules = ElasticsearchTasksCommon.Utils.GetPermittedWatchRules(channel.m_nGroupID);
            searchObject.m_oOrder = new ApiObjects.SearchObjects.OrderObj();

            searchObject.m_bUseStartDate = false;
            searchObject.m_bUseFinalEndDate = false;

            CopySearchValuesToSearchObjects(ref searchObject, channel.m_eCutWith, channel.m_lChannelTags);
            return searchObject;
        }

        private static void CopySearchValuesToSearchObjects(ref ApiObjects.SearchObjects.MediaSearchObj searchObject,
            ApiObjects.SearchObjects.CutWith cutWith, List<ApiObjects.SearchObjects.SearchValue> channelSearchValues)
        {
            List<ApiObjects.SearchObjects.SearchValue> m_dAnd = new List<ApiObjects.SearchObjects.SearchValue>();
            List<ApiObjects.SearchObjects.SearchValue> m_dOr = new List<ApiObjects.SearchObjects.SearchValue>();

            ApiObjects.SearchObjects.SearchValue search = new ApiObjects.SearchObjects.SearchValue();
            if (channelSearchValues != null && channelSearchValues.Count > 0)
            {
                foreach (ApiObjects.SearchObjects.SearchValue searchValue in channelSearchValues)
                {
                    if (!string.IsNullOrEmpty(searchValue.m_sKey))
                    {
                        search = new ApiObjects.SearchObjects.SearchValue();
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
