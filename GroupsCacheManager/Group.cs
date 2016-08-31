using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using ApiObjects;
using DAL;
using KLogMonitor;
using Newtonsoft.Json;
using Tvinci.Core.DAL;

namespace GroupsCacheManager
{
    [Serializable]
    [JsonObject(Id = "group")]
    public class Group : IDisposable
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Consts

        private const string GROUP_LOG_FILENAME = "Group";

        #endregion

        #region Members
        [JsonProperty("m_nParentGroupID")]
        public int m_nParentGroupID { get; set; }
        [JsonProperty("m_nLangID")]
        public int m_nLangID { get; set; }
        [JsonProperty("m_oMetasValuesByGroupId")]
        public Dictionary<int, Dictionary<string, string>> m_oMetasValuesByGroupId { get; set; } // Holds mapped meta columns (<groupId, <meta , meta name>>)
        [JsonProperty("m_oGroupTags")]
        public Dictionary<int, string> m_oGroupTags { get; set; }
        [JsonProperty("m_oEpgGroupSettings")]
        public EpgGroupSettings m_oEpgGroupSettings { get; set; }
        [JsonProperty("m_sPermittedWatchRules")]
        public List<string> m_sPermittedWatchRules { get; set; }
        [JsonProperty("m_nSubGroup")]
        public List<int> m_nSubGroup { get; set; }
        [JsonProperty]
        public List<int> m_lServiceObject { get; set; }

        [JsonProperty("m_oOperatorChannelIDs")]
        private Dictionary<int, List<long>> m_oOperatorChannelIDs; // channel ids for each operator. used for ipno filtering.
        [JsonProperty("m_oLockers")]
        private ConcurrentDictionary<int, ReaderWriterLockSlim> m_oLockers; // readers-writers lockers for operator channel ids.
        [JsonProperty("m_dLanguages")]
        protected Dictionary<int, LanguageObj> m_dLangauges;
        [JsonProperty("m_oDefaultLanguage")]
        protected LanguageObj m_oDefaultLanguage;

        /// <summary>
        /// Dictionary that maps media type Id to its name
        /// </summary>
        protected Dictionary<int, string> mediaTypesIdToName;

        /// <summary>
        /// Dictionary that maps media type name to its id
        /// </summary>
        protected Dictionary<string, int> mediaTypesNameToId;

        /// Indicates if this group has DTT regionalization support or not
        /// </summary>
        [JsonProperty("m_bIsRegionalizationEnabled")]
        public bool isRegionalizationEnabled;

        /// <summary>
        /// The default region of this group (in case a domain isn't associated with any region)
        /// </summary>
        public int defaultRegion;
        /// List of channel Ids of this group
        /// </summary>
        [JsonProperty("m_nChannelIds")]
        public HashSet<int> channelIDs;

        /// <summary>
        /// The group's default recommendation engine
        /// </summary>
        [JsonProperty("default_recommendation_engine")]
        public int defaultRecommendationEngine;

        /// <summary>
        /// The group's default recommendation engine
        /// </summary>
        [JsonProperty("related_recommendation_engine")]
        public int RelatedRecommendationEngine;

        /// <summary>
        /// The group's default recommendation engine
        /// </summary>
        [JsonProperty("search_recommendation_engine")]
        public int SearchRecommendationEngine;

        /// <summary>
        /// The group's default recommendation engine
        /// </summary>
        [JsonProperty("related_recommendation_engine_enrichments")]
        public int RelatedRecommendationEngineEnrichments;

        /// <summary>
        /// The group's default recommendation engine
        /// </summary>
        [JsonProperty("search_recommendation_engine_enrichments")]
        public int SearchRecommendationEngineEnrichments;

        /// <summary>
        /// The media types of this group that represent linear channels
        /// </summary>
        [JsonProperty("linear_channel_media_types")]
        public List<int> linearChannelMediaTypes;

        /// <summary>
        /// Mapping based on the table groups_media_type
        /// </summary>
        [JsonProperty("group_media_file_type_to_file_type")]
        public Dictionary<int, int> groupMediaFileTypeToFileType;

        #endregion

        #region CTOR
        public Group()
        {

        }

        #endregion

        #region Public
        public void Init(int groupID)
        {
            this.m_nParentGroupID = groupID;
            this.m_oMetasValuesByGroupId = new Dictionary<int, Dictionary<string, string>>();
            this.m_oGroupTags = new Dictionary<int, string>();
            this.m_oEpgGroupSettings = new EpgGroupSettings();
            this.m_sPermittedWatchRules = new List<string>();
            this.m_oOperatorChannelIDs = new Dictionary<int, List<long>>();
            this.m_oLockers = new ConcurrentDictionary<int, ReaderWriterLockSlim>();
            this.m_dLangauges = new Dictionary<int, LanguageObj>();
            this.m_lServiceObject = new List<int>();
            this.m_oDefaultLanguage = null;
            this.mediaTypesIdToName = new Dictionary<int, string>();
            this.mediaTypesNameToId = new Dictionary<string, int>();
            this.channelIDs = new HashSet<int>();
            this.linearChannelMediaTypes = new List<int>();
            this.groupMediaFileTypeToFileType = new Dictionary<int, int>();
        }

        public List<long> GetOperatorChannelIDs(int nOperatorID)
        {
            return Read(nOperatorID);
        }


        public List<int> GetAllOperators()
        {
            try
            {
                if (m_oOperatorChannelIDs != null)
                {
                    return m_oOperatorChannelIDs.Keys.ToList<int>();
                }
                return new List<int>();
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return new List<int>();
            }
        }

        public List<long> GetAllOperatorsChannelIDs()
        {
            SortedSet<long> allChannelIDs = new SortedSet<long>();
            List<int> operatorIDs = CatalogDAL.Get_GroupOperatorIDs(m_nParentGroupID);
            log.Info(string.Format("Group ID: {0} , Operators extracted from DB: {1}", m_nParentGroupID, operatorIDs.Aggregate<int, string>(string.Empty, (res, item) => String.Concat(res, ";", item))));
            if (operatorIDs.Count > 0)
            {
                for (int i = 0; i < operatorIDs.Count; i++)
                {
                    allChannelIDs.UnionWith(GetOperatorChannelIDs(operatorIDs[i]));
                }
            }

            return allChannelIDs.ToList();
        }

        public void Dispose()
        {
            if (m_oLockers != null && m_oLockers.Count > 0)
            {
                lock (m_oLockers)
                {
                    log.Debug("Dispose - " + String.Concat("Dispose. Locked. Group ID: ", m_nParentGroupID));
                    if (m_oLockers != null && m_oLockers.Count > 0)
                    {
                        foreach (KeyValuePair<int, ReaderWriterLockSlim> kvp in m_oLockers)
                        {
                            if (kvp.Value != null)
                            {
                                kvp.Value.Dispose();
                            }
                        } // end foreach
                    }
                    m_oLockers.Clear();
                } // end lock
                log.Debug("Dispose - " + String.Concat("Dispose. Unlocked. Group ID: ", m_nParentGroupID));
            }
        }

        #endregion

        #region Internal

        internal string GetSubTreeGroupIds()
        {
            if (m_nSubGroup != null && m_nSubGroup.Count > 0)
            {
                return string.Join(",", m_nSubGroup);
            }
            return string.Empty;
        }

        internal bool RemoveOperator(int nOperatorID)
        {
            bool bRes = false;
            try
            {
                bRes = m_oOperatorChannelIDs.Remove(nOperatorID);
                return bRes;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return bRes;
            }
        }

        internal bool UpdateChannelsToOperator(int nOperatorID, List<long> channelIDs)
        {
            bool bRes = false;
            try
            {
                if (channelIDs != null && channelIDs.Count > 0)
                {
                    m_oOperatorChannelIDs.Add(nOperatorID, channelIDs);
                    bRes = true;
                }
                else
                {
                    m_oOperatorChannelIDs.Add(nOperatorID, new List<long>(1) { 0 });
                    log.Debug("Build  - " + string.Format("No operator channel ids were extracted from DB. Operator ID: {0} , Group ID: {1}", nOperatorID, m_nParentGroupID));
                    bRes = false;
                }
                return bRes;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return false;
            }
        }

        #endregion

        #region Private

        private void GetLocker(int nOperatorID, ref ReaderWriterLockSlim locker)
        {
            if (!m_oLockers.ContainsKey(nOperatorID))
            {
                lock (m_oLockers)
                {
                    log.Debug("GetLocker - " + string.Format("Locked. Operator ID: {0} , Group ID: {1}", nOperatorID, m_nParentGroupID));
                    if (!m_oLockers.ContainsKey(nOperatorID))
                    {
                        if (!m_oLockers.TryAdd(nOperatorID, new ReaderWriterLockSlim()))
                        {
                            log.Debug("GetLocker - " + string.Format("Failed to create reader writer manager. operator id: {0} , group_id {1}", nOperatorID, m_nParentGroupID));
                        }
                    }
                }
                log.Debug("GetLocker - " + string.Format("Locker released. Operator ID: {0} , Group ID: {1}", nOperatorID, m_nParentGroupID));
            }

            if (!m_oLockers.TryGetValue(nOperatorID, out locker))
            {
                log.Debug("GetLocker - " + string.Format("Failed to read reader writer manager. operator id: {0} , group_id: {1}", nOperatorID, m_nParentGroupID));
            }
        }

        private List<long> Read(int nOperatorID)
        {
            // Get Group from cache 
            GroupManager groupManager = new GroupManager();
            Group group = groupManager.GetGroup(this.m_nParentGroupID);

            ReaderWriterLockSlim locker = null;
            GetLocker(nOperatorID, ref locker);
            if (locker == null)
            {
                log.Debug("Read - " + string.Format("Read. Failed to obtain locker. Operator ID: {0}", nOperatorID));
                throw new Exception(string.Format("Read. Cannot retrieve reader writer manager for operator id: {0} , group id: {1}", nOperatorID, m_nParentGroupID));
            }
            locker.EnterReadLock();
            List<long> res = null;
            if (group.m_oOperatorChannelIDs.ContainsKey(nOperatorID))
            {
                res = group.m_oOperatorChannelIDs[nOperatorID];
                if (res != null && res.Count > 0)
                {
                    locker.ExitReadLock();
                }
                else
                {
                    locker.ExitReadLock();
                    if (!Build(nOperatorID))
                    {
                        log.Debug("Read - " + string.Format("Failed to build operator channel ids cache. Operator ID: {0} , Group ID: {1}", nOperatorID, m_nParentGroupID));
                    }
                    res = Read(nOperatorID);
                }
            }
            else
            {
                locker.ExitReadLock();
                if (!Build(nOperatorID))
                {
                    log.Debug("Read - " + string.Format("Failed to build operator channel ids cache. Operator ID: {0} , Group ID: {1}", nOperatorID, m_nParentGroupID));
                }
                res = Read(nOperatorID);
            }

            return res;
        }

        private bool Build(int nOperatorID)
        {
            bool res = true;
            ReaderWriterLockSlim locker = null;
            if (!m_oOperatorChannelIDs.ContainsKey(nOperatorID))
            {
                GetLocker(nOperatorID, ref locker);
                if (locker == null)
                {
                    log.Debug("Build - " + string.Format("Build. Failed to obtain locker. Operator ID: {0}", nOperatorID));
                    throw new Exception(string.Format("Build. Cannot retrieve reader writer manager for operator id: {0} , group id: {1}", nOperatorID, m_nParentGroupID));
                }
                try
                {
                    locker.EnterWriteLock();
                    if (!m_oOperatorChannelIDs.ContainsKey(nOperatorID))
                    {
                        List<long> channelIDs = PricingDAL.Get_OperatorChannelIDs(m_nParentGroupID, nOperatorID, "pricing_connection");

                        // update group with operator anyway (with or without channels)
                        GroupManager groupManager = new GroupManager();
                        res = groupManager.UpdateoOperatorChannels(m_nParentGroupID, nOperatorID, channelIDs, true);
                    }
                    else
                    {
                        // no need to build. already built by another thread.
                    }
                    //locker.ExitWriteLock();
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
            else
            {
                // no need to build
            }
            return res;
        }

        private bool Add(int nOperatorID, List<long> channelIDs)
        {
            bool retVal = true;
            ReaderWriterLockSlim locker = null;
            GetLocker(nOperatorID, ref locker);
            if (m_oOperatorChannelIDs.ContainsKey(nOperatorID))
            {
                if (locker == null)
                {
                    log.Debug("Add - " + string.Format("Add. Failed to obtain locker. Operator ID: {0} , Channel IDs: {1}", nOperatorID, channelIDs.Aggregate<long, string>(string.Empty, (res, item) => String.Concat(res, ";", item))));
                    throw new Exception(string.Format("Add. Cannot retrieve reader writer manager for operator id: {0} , group id: {1}", nOperatorID, m_nParentGroupID));
                }
                try
                {
                    locker.EnterWriteLock();
                    if (m_oOperatorChannelIDs.ContainsKey(nOperatorID))
                    {
                        GroupManager groupManager = new GroupManager();
                        retVal = groupManager.AddChannelsToOperator(m_nParentGroupID, nOperatorID, channelIDs);
                    }
                    else
                    {
                        // no channel ids in cache. we wait for the next read command that will lazy evaluate initialize the cache.
                        retVal = false;
                    }

                    //locker.ExitWriteLock();
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }

            return retVal;
        }

        private bool Delete(int nOperatorID)
        {
            bool res = true;
            ReaderWriterLockSlim locker = null;
            GetLocker(nOperatorID, ref locker);
            if (m_oOperatorChannelIDs.ContainsKey(nOperatorID))
            {
                if (locker == null)
                {
                    log.Debug("Delete - " + string.Format("Delete. Failed to obtain locker. Operator ID: {0}", nOperatorID));
                    throw new Exception(string.Format("Delete. Cannot retrieve reader writer manager for operator id: {0} , group id: {1}", nOperatorID, m_nParentGroupID));
                }
                locker.EnterWriteLock();
                if (m_oOperatorChannelIDs.ContainsKey(nOperatorID))
                {
                    GroupManager groupManager = new GroupManager();
                    res = groupManager.DeleteOperator(m_nParentGroupID, nOperatorID);
                    if (!res)
                    {
                        // failed to remove from dictionary
                        log.Debug("Delete - " + string.Format("Failed to remove channel ids from cache. Operator ID: {0} , Group ID: {1}", nOperatorID, m_nParentGroupID));
                    }
                }
                locker.ExitWriteLock();
            }

            return res;
        }

        #endregion

        #region Cache
        internal bool AddChannelsToOperatorCache(int nOperatorID, List<long> channelIDs, bool bAddNewOperator)
        {
            bool retVal = false;
            try
            {
                if (m_oOperatorChannelIDs.ContainsKey(nOperatorID))
                {
                    List<long> cachedChannels = m_oOperatorChannelIDs[nOperatorID];
                    if (cachedChannels != null && cachedChannels.Count > 0)
                    {
                        int length = channelIDs.Count;
                        for (int i = 0; i < length; i++)
                        {
                            if (!cachedChannels.Contains(channelIDs[i]))
                                cachedChannels.Add(channelIDs[i]);
                        }
                        m_oOperatorChannelIDs[nOperatorID] = cachedChannels;
                        retVal = true;

                    }
                }
                else
                {
                    if (bAddNewOperator)
                    {
                        if (channelIDs == null || channelIDs.Count == 0)
                        {
                            channelIDs = new List<long>(1) { 0 };
                        }

                        m_oOperatorChannelIDs.Add(nOperatorID, channelIDs);

                        retVal = true;
                    }
                    else
                    {
                        retVal = false;  // if operator dosn't exsits - don't add it now (it will build next Read call)
                    }
                }
                return retVal;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return false;
            }
        }

        public bool DeleteOperatorCache(int nOperatorID)
        {
            bool res = false;

            if (m_oOperatorChannelIDs.ContainsKey(nOperatorID))
            {
                res = m_oOperatorChannelIDs.Remove(nOperatorID);
            }
            return res;
        }

        #endregion

        #region Channels


        public bool HasChannel(int id)
        {
            return this.channelIDs.Contains(id);
        }

        #endregion

        #region Language
        public bool AddLanguage(LanguageObj language)
        {
            bool bRes = false;
            if (language != null)
            {
                try
                {
                    m_dLangauges.Add(language.ID, language);

                    if (language.IsDefault)
                        m_oDefaultLanguage = language;

                    bRes = true;
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Language with same ID already exist in group. groupID={0};language id={1} - " + this.m_nParentGroupID, language.ID), ex);
                }
            }

            return bRes;
        }

        public void AddLanguage(List<LanguageObj> languages)
        {
            foreach (LanguageObj langauge in languages)
            {
                AddLanguage(langauge);
            }
        }

        public LanguageObj GetLanguage(int nLangID)
        {
            LanguageObj res;

            m_dLangauges.TryGetValue(nLangID, out res);

            return res;
        }

        public LanguageObj GetLanguage(string languageCode)
        {
            LanguageObj res;

            res = m_dLangauges.Values.Where(l => l.Code == languageCode).FirstOrDefault();

            return res;
        }

        public List<LanguageObj> GetLangauges()
        {
            return this.m_dLangauges.Select(kvp => kvp.Value).ToList();
        }

        public LanguageObj GetGroupDefaultLanguage()
        {
            return m_oDefaultLanguage;
        }
        #endregion

        #region Services

        public bool AddServices(List<int> services)
        {
            bool bAdd = false;
            if (services != null && services.Count > 0)
            {
                try
                {
                    if (m_lServiceObject == null)
                    {
                        m_lServiceObject = new List<int>();
                    }

                    foreach (int newItem in services)
                    {
                        if (!m_lServiceObject.Contains(newItem))
                        {
                            m_lServiceObject.Add(newItem);
                            bAdd = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("failed to add Service . groupID={0}; - " + this.m_nParentGroupID), ex);
                }
            }

            return bAdd;
        }

        public int GetServices(int nServiceID)
        {
            int res = 0;

            if (m_lServiceObject != null && m_lServiceObject.Contains(nServiceID))
            {
                res = nServiceID;
            }

            return res;
        }

        public List<int> GetServices()
        {
            return this.m_lServiceObject;
        }

        public bool RemoveServices(List<int> servicesID)
        {
            bool bRemove = false;
            try
            {
                if (m_lServiceObject != null)
                {
                    foreach (int removeItem in servicesID)
                    {
                        if (m_lServiceObject.Contains(removeItem))
                        {
                            m_lServiceObject.Remove(removeItem);
                            bRemove = true;
                        }
                    }
                }
                return bRemove;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return false;
            }
        }

        public bool UpdateServices(List<int> services)
        {
            try
            {
                //foreach (int item in m_lServiceObject)
                //{
                //    foreach (int updateItem in services)
                //    {
                //        if (item.ID == updateItem.ID)
                //        {
                //            item.Name = updateItem.Name;
                //        }
                //    }
                //}
                return false;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return false;
            }
        }




        #endregion

        #region Media Types

        /// <summary>
        /// Initialize dictionaries of media types if not initialized yet
        /// </summary>
        private void InitializeMediaTypes()
        {
            Dictionary<int, int> mediaTypeParents;

            if (this.mediaTypesNameToId == null ||
                this.mediaTypesIdToName == null ||
                this.mediaTypesNameToId.Count == 0 ||
                this.mediaTypesIdToName.Count == 0)
            {
                CatalogDAL.GetMediaTypes(this.m_nParentGroupID,
                    out this.mediaTypesIdToName,
                    out this.mediaTypesNameToId,
                    out mediaTypeParents, out linearChannelMediaTypes);
            }
        }

        /// <summary>
        /// Gets list of all media types Ids in this group
        /// </summary>
        /// <returns></returns>
        public List<int> GetMediaTypes()
        {
            InitializeMediaTypes();

            // Convert dictionary to list of ints
            return (this.mediaTypesIdToName.Keys.ToList());
        }


        /// <summary>
        /// Reverse lookup of Id by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetMediaTypeIdByName(string name)
        {
            int id = 0;

            InitializeMediaTypes();

            // Simple lookup in dictionary
            this.mediaTypesNameToId.TryGetValue(name, out id);

            return (id);
        }

        /// <summary>
        /// Reverse lookup of several Ids by names
        /// </summary>
        /// <param name="names"></param>
        /// <returns></returns>
        public List<int> GetMediaTypeIdsByNames(List<string> names)
        {
            List<int> ids = new List<int>();

            InitializeMediaTypes();

            // Lookup each name in dictionary and add to list
            foreach (var name in names)
            {
                int id;

                this.mediaTypesNameToId.TryGetValue(name, out id);

                if (id != 0)
                {
                    ids.Add(id);
                }
            }

            return (ids);
        }

        #endregion

    }
}
