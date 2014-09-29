using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects;
using System.Threading;
using System.Security.AccessControl;
using Logger;
using Tvinci.Core.DAL;
using DAL;
using Newtonsoft.Json;
using ApiObjects.Cache;
using Enyim.Caching.Memcached;
using System.Data;
using Catalog.Cache;

namespace Catalog
{
}
  /*  [Serializable]
    [JsonObject(Id = "group")]
    public class Group : IDisposable
    {

        private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const string GROUP_LOG_FILENAME = "Group";


        #region Members
        [JsonProperty("m_nParentGroupID")]
        public int m_nParentGroupID { get; set; }
        [JsonProperty("m_nLangID")]
        public int m_nLangID { get; set; }
        [JsonProperty("m_oMetasValuesByGroupId")]
        public Dictionary<int, Dictionary<string, string>> m_oMetasValuesByGroupId { get; set; } // Holds mapped meta columns (<groupId, <meta , meta name>>)
        [JsonProperty("m_oGroupTags")]
        public Dictionary<int, string> m_oGroupTags { get; set; }
        [JsonProperty("m_oGroupChannels")]
        public ConcurrentDictionary<int, Channel> m_oGroupChannels { get; set; }
        [JsonProperty("m_oEpgGroupSettings")]
        public EpgGroupSettings m_oEpgGroupSettings { get; set; }
        [JsonProperty("m_sPermittedWatchRules")]
        public List<string> m_sPermittedWatchRules { get; set; }
        [JsonProperty("m_nSubGroup")]
        public List<int> m_nSubGroup { get; set; }


        [JsonProperty("m_oOperatorChannelIDs")]
        private Dictionary<int, List<long>> m_oOperatorChannelIDs; // channel ids for each operator. used for ipno filtering.
        [JsonProperty("m_oLockers")]
        private ConcurrentDictionary<int, ReaderWriterLockSlim> m_oLockers; // readers-writers lockers for operator channel ids.
        [JsonProperty("m_dLanguages")]
        protected Dictionary<int, LanguageObj> m_dLangauges;
        [JsonProperty("m_oDefaultLanguage")]
        protected LanguageObj m_oDefaultLanguage;
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
            this.m_oGroupChannels = new ConcurrentDictionary<int, Channel>();
            this.m_oGroupTags = new Dictionary<int, string>();
            this.m_oEpgGroupSettings = new EpgGroupSettings();
            this.m_sPermittedWatchRules = new List<string>();
            this.m_oOperatorChannelIDs = new Dictionary<int, List<long>>();
            this.m_oLockers = new ConcurrentDictionary<int, ReaderWriterLockSlim>();
            this.m_dLangauges = new Dictionary<int, LanguageObj>();
            this.m_oDefaultLanguage = null;
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
                return new List<int>();
            }
        }

        public List<long> GetAllOperatorsChannelIDs()
        {
            SortedSet<long> allChannelIDs = new SortedSet<long>();
            List<int> operatorIDs = CatalogDAL.Get_GroupOperatorIDs(m_nParentGroupID);
            _logger.Info(string.Format("Group ID: {0} , Operators extracted from DB: {1}", m_nParentGroupID, operatorIDs.Aggregate<int, string>(string.Empty, (res, item) => String.Concat(res, ";", item))));
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
                    Logger.Logger.Log("Dispose", String.Concat("Dispose. Locked. Group ID: ", m_nParentGroupID), GROUP_LOG_FILENAME);
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
                Logger.Logger.Log("Dispose", String.Concat("Dispose. Unlocked. Group ID: ", m_nParentGroupID), GROUP_LOG_FILENAME);
            }
        }

        #endregion

        #region Internal
        internal bool AddChannels(int nGroupID, List<Channel> lNewCreatedChannels)
        {
            try
            {
                foreach (Channel channel in lNewCreatedChannels)
                {
                    if (!m_oGroupChannels.ContainsKey(channel.m_nChannelID))
                    {
                        m_oGroupChannels.TryAdd(channel.m_nChannelID, channel);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

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
                    Logger.Logger.Log("Build", string.Format("No operator channel ids were extracted from DB. Operator ID: {0} , Group ID: {1}", nOperatorID, m_nParentGroupID), GROUP_LOG_FILENAME);
                    bRes = false;
                }
                return bRes;
            }
            catch (Exception ex)
            {
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
                    Logger.Logger.Log("GetLocker", string.Format("Locked. Operator ID: {0} , Group ID: {1}", nOperatorID, m_nParentGroupID), GROUP_LOG_FILENAME);
                    if (!m_oLockers.ContainsKey(nOperatorID))
                    {
                        if (!m_oLockers.TryAdd(nOperatorID, new ReaderWriterLockSlim()))
                        {
                            Logger.Logger.Log("GetLocker", string.Format("Failed to create reader writer manager. operator id: {0} , group_id {1}", nOperatorID, m_nParentGroupID), GROUP_LOG_FILENAME);
                        }
                    }
                }
                Logger.Logger.Log("GetLocker", string.Format("Locker released. Operator ID: {0} , Group ID: {1}", nOperatorID, m_nParentGroupID), GROUP_LOG_FILENAME);
            }

            if (!m_oLockers.TryGetValue(nOperatorID, out locker))
            {
                Logger.Logger.Log("GetLocker", string.Format("Failed to read reader writer manager. operator id: {0} , group_id: {1}", nOperatorID, m_nParentGroupID), GROUP_LOG_FILENAME);
            }
        }

        private List<long> Read(int nOperatorID)
        {
            GroupManager groupManager = new GroupManager();
            Group group = groupManager.GetGroup(this.m_nParentGroupID);

            ReaderWriterLockSlim locker = null;
            GetLocker(nOperatorID, ref locker);
            if (locker == null)
            {
                Logger.Logger.Log("Read", string.Format("Read. Failed to obtain locker. Operator ID: {0}", nOperatorID), GROUP_LOG_FILENAME);
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
                        Logger.Logger.Log("Read", string.Format("Failed to build operator channel ids cache. Operator ID: {0} , Group ID: {1}", nOperatorID, m_nParentGroupID), GROUP_LOG_FILENAME);
                    }
                    res = Read(nOperatorID);
                }
            }
            else
            {
                locker.ExitReadLock();
                if (!Build(nOperatorID))
                {
                    Logger.Logger.Log("Read", string.Format("Failed to build operator channel ids cache. Operator ID: {0} , Group ID: {1}", nOperatorID, m_nParentGroupID), GROUP_LOG_FILENAME);
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
                    Logger.Logger.Log("Build", string.Format("Build. Failed to obtain locker. Operator ID: {0}", nOperatorID), GROUP_LOG_FILENAME);
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
                    Logger.Logger.Log("Add", string.Format("Add. Failed to obtain locker. Operator ID: {0} , Channel IDs: {1}", nOperatorID, channelIDs.Aggregate<long, string>(string.Empty, (res, item) => String.Concat(res, ";", item))), GROUP_LOG_FILENAME);
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
                    Logger.Logger.Log("Delete", string.Format("Delete. Failed to obtain locker. Operator ID: {0}", nOperatorID), GROUP_LOG_FILENAME);
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
                        Logger.Logger.Log("Delete", string.Format("Failed to remove channel ids from cache. Operator ID: {0} , Group ID: {1}", nOperatorID, m_nParentGroupID), GROUP_LOG_FILENAME);
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

        public List<Channel> GetChannelsFromCache(List<int> channelIds, int nOwnerGroup)
        {
            List<Channel> lRes = null;

            if (this != null && channelIds != null && channelIds.Count > 0)
            {
                lRes = new List<Channel>();
                Channel oChannel;
                foreach (int channelID in channelIds)
                {
                    if (this.m_oGroupChannels.TryGetValue(channelID, out oChannel))
                    {
                        lRes.Add(oChannel);
                    }
                }

                //get all channels from DB
                var channelsNotInCache = channelIds.Where(id => !lRes.Any(existId => existId.m_nChannelID == id));
                if (channelsNotInCache != null)
                {
                    List<int> lNotIncludedInCache = channelsNotInCache.ToList<int>();
                    if (lNotIncludedInCache.Count > 0)
                    {
                        List<Channel> lNewCreatedChannels = ChannelRepository.GetChannels(lNotIncludedInCache, this);
                        //add the channels from DB to cache 

                        GroupManager groupManager = new GroupManager();
                        bool bAdd = groupManager.InsertChannels(lNewCreatedChannels, this.m_nParentGroupID);
                        lRes.AddRange(lNewCreatedChannels);
                    }
                }
            }
            return lRes;
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
                    _logger.Error(string.Format("Language with same ID already exist in group. groupID={0};language id={1}", this.m_nParentGroupID, language.ID));
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

        public List<LanguageObj> GetLangauges()
        {
            return this.m_dLangauges.Select(kvp => kvp.Value).ToList();
        }

        public LanguageObj GetGroupDefaultLanguage()
        {
            return m_oDefaultLanguage;
        }
        #endregion

    }
}
   #region OLD CODE
        /*public List<long> GetDistinctAllOperatorsChannels()
        {
            if (Utils.IsGroupIDContainedInConfig(this.m_nParentGroupID, "GroupIDsWithIPNOFilteringSeperatedBySemiColon", ';'))
            {
                return GetAllOperatorsChannelIDs();
            }

            return new List<long>(0);
        }*/

       /* public bool AddChannelsToOperator(int nOperatorID, List<long> channelIDs)
        {
            return Add(nOperatorID, channelIDs);
        }*/

       /* public bool DeleteOperatorChannels(int nOperatorID)
        {
            return Delete(nOperatorID);
        }*/
        /*internal bool AddChannelsToOperatorDict(int nOperatorID, List<long> channelIDs)
        {
            return AddToDictionary(nOperatorID, channelIDs);
        }*/

        /*private bool AddToDictionary(int nOperatorID, List<long> channelIDs)
        {
            bool retVal = true;
            ReaderWriterLockSlim locker = null;
            GetLocker(nOperatorID, ref locker);
            if (m_oOperatorChannelIDs.ContainsKey(nOperatorID))
            {
                if (locker == null)
                {
                    Logger.Logger.Log("Add", string.Format("Add. Failed to obtain locker. Operator ID: {0} , Channel IDs: {1}", nOperatorID, channelIDs.Aggregate<long, string>(string.Empty, (res, item) => String.Concat(res, ";", item))), GROUP_LOG_FILENAME);
                    throw new Exception(string.Format("Add. Cannot retrieve reader writer manager for operator id: {0} , group id: {1}", nOperatorID, m_nParentGroupID));
                }
                try
                {
                    locker.EnterWriteLock();
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
                        }
                        else
                        {
                            // no channel ids in cache. we wait for the next read command that will lazy evaluate initialize the cache.
                            retVal = false;
                        }
                    }
                    else
                    {
                        // no channel ids in cache. we wait for the next read command that will lazy evaluate initialize the cache.
                        retVal = false;
                    }
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }

            return retVal;
        }*/

        


    