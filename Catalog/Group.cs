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

namespace Catalog
{
    public class Group : IDisposable
    {

        private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const string GROUP_LOG_FILENAME = "Group";
        #region Members

        public int m_nParentGroupID { get; set; }
        public int m_nLangID { get; set; }
        public Dictionary<int, Dictionary<string, string>> m_oMetasValuesByGroupId { get; set; } // Holds mapped meta columns (<groupId, <meta , meta name>>)
        public Dictionary<int, string> m_oGroupTags { get; set; }
        public ConcurrentDictionary<int, Channel> m_oGroupChannels { get; set; }
        public EpgGroupSettings m_oEpgGroupSettings { get; set; }
        public List<string> m_sPermittedWatchRules { get; set; }
        private Dictionary<int, List<long>> m_oOperatorChannelIDs; // channel ids for each operator. used for ipno filtering.
        private ConcurrentDictionary<int, ReaderWriterLockSlim> m_oLockers; // readers-writers lockers for operator channel ids.
        protected Dictionary<int, LanguageObj> m_dLangauges;
        protected LanguageObj m_oDefaultLanguage;
        #endregion

        #region CTOR

        public Group(int groupID)
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

        public List<long> GetOperatorChannelIDs(int nOperatorID)
        {
            return Read(nOperatorID);
        }

        private List<long> Read(int nOperatorID)
        {
            ReaderWriterLockSlim locker = null;
            GetLocker(nOperatorID, ref locker);
            if (locker == null)
            {
                Logger.Logger.Log("Read", string.Format("Read. Failed to obtain locker. Operator ID: {0}", nOperatorID), GROUP_LOG_FILENAME);
                throw new Exception(string.Format("Read. Cannot retrieve reader writer manager for operator id: {0} , group id: {1}", nOperatorID, m_nParentGroupID));
            }
            locker.EnterReadLock();
            List<long> res = null;
            if (m_oOperatorChannelIDs.ContainsKey(nOperatorID))
            {
                res = m_oOperatorChannelIDs[nOperatorID];
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
                locker.EnterWriteLock();
                if (!m_oOperatorChannelIDs.ContainsKey(nOperatorID))
                {
                    List<long> channelIDs = PricingDAL.Get_OperatorChannelIDs(m_nParentGroupID, nOperatorID, "pricing_connection");
                    if (channelIDs != null && channelIDs.Count > 0)
                    {
                        m_oOperatorChannelIDs.Add(nOperatorID, channelIDs);
                    }
                    else
                    {
                        m_oOperatorChannelIDs.Add(nOperatorID, new List<long>(1) { 0 });
                        Logger.Logger.Log("Build", string.Format("No operator channel ids were extracted from DB. Operator ID: {0} , Group ID: {1}", nOperatorID, m_nParentGroupID), GROUP_LOG_FILENAME);
                        res = false;
                    }

                }
                else
                {
                    // no need to build. already built by another thread.
                }
                locker.ExitWriteLock();
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

                locker.ExitWriteLock();
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
                    res = m_oOperatorChannelIDs.Remove(nOperatorID);
                    if (!res)
                    {
                        // failed to remove from dictionary
                        Logger.Logger.Log("Delete", string.Format("Failed to remove channel ids from dictionary. Operator ID: {0} , Group ID: {1}", nOperatorID, m_nParentGroupID), GROUP_LOG_FILENAME);
                    }
                }
                locker.ExitWriteLock();
            }

            return res;
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

        public bool AddChannelsToOperator(int nOperatorID, List<long> channelIDs)
        {
            return Add(nOperatorID, channelIDs);
        }

        public bool DeleteOperatorChannels(int nOperatorID)
        {
            return Delete(nOperatorID);
        }

        #endregion



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
    }
}
