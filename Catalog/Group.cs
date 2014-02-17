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
    public class Group
    {

        private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #region Members

        public int m_nParentGroupID { get; set; }
        public int m_nLangID { get; set; }
        public Dictionary<int, Dictionary<string, string>> m_oMetasValuesByGroupId { get; set; } // Holds mapped meta columns (<groupId, <meta , meta name>>)
        public Dictionary<int, string> m_oGroupTags { get; set; }
        public ConcurrentDictionary<int, Channel> m_oGroupChannels { get; set; }
        public EpgGroupSettings m_oEpgGroupSettings { get; set; }
        public List<string> m_sPermittedWatchRules { get; set; }
        private ConcurrentDictionary<int, List<long>> m_oOperatorChannelIDs { get; set; } // ipno

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
            this.m_oOperatorChannelIDs = new ConcurrentDictionary<int, List<long>>();
        }

        public List<long> GetOperatorChannelIDs(int nOperatorID)
        {
            if (!m_oOperatorChannelIDs.ContainsKey(nOperatorID))
            {
                bool createdNew = false;
                MutexSecurity mutexSecurity = Utils.CreateMutex();
                string sMutexName = GetMutexName(nOperatorID, OperatorChannelsAction.Build);
                using (Mutex mutex = new Mutex(false, sMutexName, out createdNew, mutexSecurity))
                {
                    try
                    {
                        _logger.Info(GetMutexLogMsg("Mutex about to get locked", nOperatorID, sMutexName));
                        mutex.WaitOne(-1);
                        if (!m_oOperatorChannelIDs.ContainsKey(nOperatorID))
                        {
                            _logger.Info(GetMutexLogMsg("Entered Critical Section", nOperatorID, sMutexName));
                            List<long> channelIDs = PricingDAL.Get_OperatorChannelIDs(m_nParentGroupID, nOperatorID, "pricing_connection");
                            _logger.Info(GetMutexLogMsg(String.Concat("Num of channels retrieved from DB: ", channelIDs.Count), nOperatorID, sMutexName));
                            m_oOperatorChannelIDs.TryAdd(nOperatorID, channelIDs);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("GetOperatorChannelIDs. Exception occurred.", ex);
                    }
                    finally
                    {
                        mutex.ReleaseMutex();
                        _logger.Info(GetMutexLogMsg("Mutex released", nOperatorID, sMutexName));
                    }
                }

            }
            List<long> res = null;
            if (m_oOperatorChannelIDs.TryGetValue(nOperatorID, out res))
                return res;
            return new List<long>(0);

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

        private string GetMutexName(int nOperatorID, OperatorChannelsAction action)
        {
            return String.Concat("CatalogGroupCacheAction_", action.ToString(), "_", "OperatorID_", nOperatorID);
        }

        private string GetMutexLogMsg(string sDesc, int nOperatorID, string sMutexName)
        {
            return string.Format("{0} . Operator ID: {1} , Parent Group ID: {2} , Mutex Name: {3}", sDesc, nOperatorID, m_nParentGroupID, sMutexName);
        }

        public bool AddChannelsToOperator(int nOperatorID, List<long> channelIDs)
        {
            bool res = false;
            if (m_oOperatorChannelIDs.ContainsKey(nOperatorID))
            {
                string sMutexName = GetMutexName(nOperatorID, OperatorChannelsAction.Add);
                bool createdNew = false;
                MutexSecurity mutexSecurity = Utils.CreateMutex();
                using (Mutex mutex = new Mutex(false, sMutexName, out createdNew, mutexSecurity))
                {
                    try
                    {
                        _logger.Info(GetMutexLogMsg("Mutex about to get locked", nOperatorID, sMutexName));
                        mutex.WaitOne(-1);
                        _logger.Info(GetMutexLogMsg("Entered Critical Section", nOperatorID, sMutexName));
                        List<long> listOfOperatorChannels = GetOperatorChannelIDs(nOperatorID);
                        _logger.Info(GetMutexLogMsg(string.Format("Length of listOfOperatorChannels: {0}", listOfOperatorChannels != null ? listOfOperatorChannels.Count.ToString() : "null"), nOperatorID, sMutexName));
                        if (listOfOperatorChannels != null && listOfOperatorChannels.Count > 0)
                        {
                            for (int i = 0; i < channelIDs.Count; i++)
                            {
                                if (!listOfOperatorChannels.Contains(channelIDs[i]))
                                {
                                    listOfOperatorChannels.Add(channelIDs[i]);
                                    res = true;
                                }
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("AddChannelToOperator. Exception occurred.", ex);
                    }
                    finally
                    {
                        mutex.ReleaseMutex();
                        _logger.Info(GetMutexLogMsg("Mutex released", nOperatorID, sMutexName));
                    }
                } // end using
            }

            return res;
        }

        public bool DeleteOperatorChannels(int nOperatorID)
        {
            bool res = false;

            if (m_oOperatorChannelIDs.ContainsKey(nOperatorID))
            {
                string sBuilderMutexName = GetMutexName(nOperatorID, OperatorChannelsAction.Build);
                bool createdNewBuilder = false;
                MutexSecurity mutexSecurity = Utils.CreateMutex();
                using (Mutex builderMutex = new Mutex(false, sBuilderMutexName, out createdNewBuilder, mutexSecurity))
                {
                    string sAdderMutexName = GetMutexName(nOperatorID, OperatorChannelsAction.Add);
                    bool createdNewAdder = false;
                    try
                    {
                        _logger.Info(GetMutexLogMsg("Builder mutex about to get locked", nOperatorID, sBuilderMutexName));
                        builderMutex.WaitOne(-1); // lock builder mutex
                        _logger.Info(GetMutexLogMsg("Builder mutex locked", nOperatorID, sBuilderMutexName));
                        using (Mutex adderMutex = new Mutex(false, sBuilderMutexName, out createdNewAdder, mutexSecurity))
                        {
                            try
                            {
                                _logger.Info(GetMutexLogMsg("Adder mutex about to get locked", nOperatorID, sAdderMutexName));
                                adderMutex.WaitOne(-1); // lock adder mutex
                                _logger.Info(GetMutexLogMsg("Adder mutex locked", nOperatorID, sAdderMutexName));
                                if (m_oOperatorChannelIDs.ContainsKey(nOperatorID))
                                {
                                    List<long> temp = null;
                                    res = m_oOperatorChannelIDs.TryRemove(nOperatorID, out temp);
                                    _logger.Info(string.Format("DeleteOperatorChannels result: {0}", res.ToString().ToLower()));
                                }

                            }
                            catch (Exception ex)
                            {
                                _logger.Error("Exception adder mutex", ex);
                            }
                            finally
                            {
                                adderMutex.ReleaseMutex();
                                _logger.Info(GetMutexLogMsg("Adder mutex released", nOperatorID, sAdderMutexName));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Exception builder mutex", ex);
                    }
                    finally
                    {
                        builderMutex.ReleaseMutex();
                        _logger.Info(GetMutexLogMsg("Builder mutex released", nOperatorID, sBuilderMutexName));
                    }
                }

            }

            return res;
        }

        private enum OperatorChannelsAction : byte
        {
            Build = 0,
            Add = 1
        }
        #endregion


    }
}
