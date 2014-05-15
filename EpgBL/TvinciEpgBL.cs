using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiObjects;
using DalCB;
using Logger;

namespace EpgBL
{
    public class TvinciEpgBL : BaseEpgBL
    {  
        protected EpgDal_Couchbase m_oEpgCouchbase;
        private static readonly double EXPIRY_DATE = (Utils.GetDoubleValFromConfig("epg_doc_expiry") > 0) ? Utils.GetDoubleValFromConfig("epg_doc_expiry") : 7;
        private static readonly int DAYSBUFFER = 7;

        public TvinciEpgBL(int nGroupID)
        {
            this.m_nGroupID = nGroupID;
            m_oEpgCouchbase = new DalCB.EpgDal_Couchbase(m_nGroupID);
        }

        public override bool InsertEpg(EpgCB newEpgItem, out ulong epgID, ulong? cas = null)
        {
            epgID = 0;
            bool bRes = false;
            try
            {

                for (int i = 0; i < 3 && !bRes; i++)
                {
                    //ulong nNewID = m_oEpgCouchbase.IDGenerator("epgid");
                    //newEpgItem.EpgID = nNewID;
                    //epgID = nNewID;
                    //bRes = m_oEpgCouchbase.InsertProgram(nNewID.ToString(), newEpgItem, newEpgItem.EndDate.AddDays(EXPIRY_DATE));

                    ulong nNewID = newEpgItem.EpgID;

                    bRes = (cas.HasValue) ? m_oEpgCouchbase.InsertProgram(nNewID.ToString(), newEpgItem, newEpgItem.EndDate.AddDays(EXPIRY_DATE), cas.Value) :
                                            m_oEpgCouchbase.InsertProgram(nNewID.ToString(), newEpgItem, newEpgItem.EndDate.AddDays(EXPIRY_DATE));

                    if (bRes)
                    {
                        epgID = nNewID;
                    }

                    Logger.Logger.Log("InsertCBEpg", string.Format("insert result  CB id={0} result ={1}",nNewID, bRes), "InsertCBEpg");
                }

            }
            catch (Exception ex)
            {
                Logger.Logger.Log("InsertEpg", string.Format("Failed Insert Epg ex = {0} ", ex.Message), "BaseEpgBL");
            }
            return bRes;
        }

        public override bool UpdateEpg(EpgCB newEpgItem, ulong? cas = null)
        {
            bool bRes = false;
            for (int i = 0; i < 3 && !bRes; i++)
            {
                bRes = (cas.HasValue) ? m_oEpgCouchbase.UpdateProgram(newEpgItem.EpgID.ToString(), newEpgItem, newEpgItem.EndDate.AddDays(EXPIRY_DATE), cas.Value) : 
                                        m_oEpgCouchbase.UpdateProgram(newEpgItem.EpgID.ToString(), newEpgItem, newEpgItem.EndDate.AddDays(EXPIRY_DATE));
            }

            return bRes;
        }

        public bool RemoveEpg(ulong id)
        {
            bool bRes = true;

            EpgCB doc = this.GetEpgCB(id);

            if (doc != null)
            {
                bRes = m_oEpgCouchbase.DeleteProgram(id.ToString());                
                //doc.Status = 2;
                //bRes = this.UpdateEpg(doc);
            }

            return bRes;
        }

        public void RemoveEpg(List<ulong> lIDs)
        {
            foreach (ulong id in lIDs)
            {
                this.RemoveEpg(id);
            }
        }

        public override void RemoveGroupPrograms(DateTime? fromDate, DateTime? toDate)
        {
            List<EpgCB> lExisitingPrograms = this.GetGroupEpgs(0, 0, fromDate, toDate);

            if (lExisitingPrograms != null && lExisitingPrograms.Count > 0)
            {
                foreach (EpgCB epg in lExisitingPrograms)
                {
                    epg.Status = 2;
                    UpdateEpg(epg);
                }
            }

        }

        public override void RemoveGroupPrograms(List<DateTime> lDates, int channelID)
        {
            List<EpgCB> lExisitingPrograms = new List<EpgCB>();
            foreach (DateTime date in lDates)
            {
                List<EpgCB> lTempPrograms = new List<EpgCB>();
                lTempPrograms = this.GetChannelPrograms(0, 0, channelID, date, date.AddDays(1));

                if (lTempPrograms != null && lTempPrograms.Count > 0)
                {
                    foreach (EpgCB item in lTempPrograms)
                    {
                        if (!lExisitingPrograms.Contains(item))
                            lExisitingPrograms.Add(item);
                    }
                }
            }

            if (lExisitingPrograms != null && lExisitingPrograms.Count > 0)
            {
                List<ulong> lEpgIDs = new List<ulong>();
                foreach (EpgCB epg in lExisitingPrograms)
                {
                    if (epg != null)
                    {
                        lEpgIDs.Add(epg.EpgID);
                    }
                }
                this.RemoveEpg(lEpgIDs);
            }
        }

        public void RemoveGroupPrograms(DateTime? fromDate, DateTime? toDate, int channelID)
        {
            List<EpgCB> lExisitingPrograms = this.GetChannelPrograms(0, 0, channelID, fromDate, toDate);

            if (lExisitingPrograms != null && lExisitingPrograms.Count > 0)
            {
                foreach (EpgCB epg in lExisitingPrograms)
                {
                    epg.Status = 2;
                    UpdateEpg(epg);
                }
            }

        }

        public override EpgCB GetEpgCB(ulong nProgramID)
        {
            EpgCB oRes = m_oEpgCouchbase.GetProgram(nProgramID.ToString());
            oRes = (oRes != null && oRes.ParentGroupID == m_nGroupID) ? oRes : null;
            return oRes;
        }

        public override EpgCB GetEpgCB(ulong nProgramID, out ulong cas)
        {
            EpgCB oRes = m_oEpgCouchbase.GetProgram(nProgramID.ToString(), out cas);
            oRes = (oRes != null && oRes.ParentGroupID == m_nGroupID) ? oRes : null;
            return oRes;
        }

        public override EPGChannelProgrammeObject GetEpg(ulong nProgramID)
        {
            EPGChannelProgrammeObject oRes = new EPGChannelProgrammeObject();
            EpgCB oResCB = GetEpgCB(nProgramID);
            if (oResCB != null)
            {
                oRes = ConvertEpgCBtoEpgProgramm(oResCB);
            }
            else
                oRes = null;
            return oRes;
        }

        public List<EpgCB> GetGroupEpgs(int nPageSize, int nStartIndex, DateTime? dfromDate, DateTime? dToDate)
        {
            List<EpgCB> lRes = null;

            if (dfromDate.HasValue && dToDate.HasValue)
            {
                lRes = m_oEpgCouchbase.GetGroupProgramsByStartDate(nPageSize, nStartIndex, dfromDate.Value, dToDate.Value);
            }
            else if (dfromDate.HasValue)
            {
                lRes = m_oEpgCouchbase.GetGroupProgramsByStartDate(nPageSize, nStartIndex, dfromDate.Value);
            }
            else
            {
                lRes = m_oEpgCouchbase.GetGroupPrograms(nPageSize, nStartIndex);
            }

            return lRes;
        }

        public List<EpgCB> GetChannelPrograms(int nPageSize, int nStartIndex, int nChannelID, DateTime? fromUTCDay, DateTime? toUTCDay)
        {
            List<EpgCB> lRes = null;
            if (fromUTCDay.HasValue && toUTCDay.HasValue)
            {
                lRes = m_oEpgCouchbase.GetChannelProgramsByStartDate(nPageSize, nStartIndex, nChannelID, fromUTCDay.Value, toUTCDay.Value, false);
            }
            else if (fromUTCDay.HasValue) //fromUTCDay <= start_date < MaxValue
            {
                lRes = m_oEpgCouchbase.GetChannelProgramsByStartDate(nPageSize, nStartIndex, nChannelID, fromUTCDay.Value, DateTime.MaxValue, false);
            }
            else if (toUTCDay.HasValue) //MinValue <= start_date <= toUTCDay
            {
                lRes = m_oEpgCouchbase.GetChannelProgramsByStartDate(nPageSize, nStartIndex, nChannelID, DateTime.MinValue, toUTCDay.Value.AddSeconds(1), false);
            }
            else
            {
                lRes = m_oEpgCouchbase.GetChannelPrograms(nPageSize, nStartIndex, nChannelID);
            }

            return lRes;
        }

        //this call will return a list of channels (a new object) which will contain a list of EpgNew objects
        public List<EpgCB> GetMultiChannelPrograms(int nPageSize, int nStartIndex, List<int> lChannelIDs, DateTime? fromUTCDay, DateTime? toUTCDay)
        {
            List<EpgCB> lRes = new List<EpgCB>();

            if (lChannelIDs != null && lChannelIDs.Count > 0)
            {
                Task<List<EpgCB>>[] tChannelTasks = new Task<List<EpgCB>>[lChannelIDs.Count];
                for (int i = 0; i < lChannelIDs.Count; i++)
                {
                    tChannelTasks[i] = Task.Factory.StartNew<List<EpgCB>>(
                        (index) =>
                        {
                            return GetChannelPrograms(nPageSize, nStartIndex, lChannelIDs[(int)index], fromUTCDay, toUTCDay);
                        }, i);
                }

                Task.WaitAll(tChannelTasks);

                foreach (Task<List<EpgCB>> task in tChannelTasks)
                {
                    if (task.Result != null && task.Result.Count > 0)
                    {
                        lRes.AddRange(task.Result);
                    }
                }
            }
            return lRes;
        }

        //will need to return an object containing two dictionaries one for metas and the other for tags maybe we'll want to store it in cache and update when needed
        public EpgGroupSettings GetGroupEpgTagsAndMetas(bool bIsSearchable)
        {
            DataSet ds = Tvinci.Core.DAL.EpgDal.Get_GroupsTagsAndMetas(m_nGroupID);
            EpgGroupSettings egs = new EpgGroupSettings();
            try
            {
                if (ds != null && ds.Tables != null && ds.Tables.Count >= 2)
                {
                    #region metas
                    if (ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            string filed = ODBCWrapper.Utils.GetSafeStr(row["name"]);
                            if (!string.IsNullOrEmpty(filed))
                            {
                                egs.m_lMetasName.Add(filed);
                            }
                        }
                    }
                    #endregion
                    #region tags
                    if (ds.Tables[1] != null && ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count > 0)
                    {
                        foreach (DataRow row in ds.Tables[1].Rows)
                        {
                            string filed = ODBCWrapper.Utils.GetSafeStr(row["name"]);
                            if (!string.IsNullOrEmpty(filed))
                            {
                                egs.m_lTagsName.Add(filed);
                            }
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Error", string.Format("Caugh exception when fetching EPG group tags and metas. Ex={0}; stack={1}", ex.Message, ex.StackTrace), "BaseEpgBL");
            }

            return egs;
        }

        //get all EPgs in the given range, including Epgs that are partially overlapping
        public override ConcurrentDictionary<int, List<EPGChannelProgrammeObject>> GetMultiChannelProgramsDic(int nPageSize, int nStartIndex, List<int> lChannelIDs, DateTime fromDate, DateTime toDate)
        {
            using (Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true))
            {
                log.Method = "BaseEpgBL.GetMultiChannelProgramsDic";
                ConcurrentDictionary<int, List<EPGChannelProgrammeObject>> dChannelEpgList = EpgBL.Utils.createDic(lChannelIDs);
           
                if (lChannelIDs != null && lChannelIDs.Count > 0)
                {
                    int nChannelCount = lChannelIDs.Count;

                    //Start MultiThread Call
                    Task[] tasks = new Task[nChannelCount];

                    for (int i = 0; i < nChannelCount; i++)
                    {
                        int nChannel = lChannelIDs[i];

                        tasks[i] = Task.Factory.StartNew(
                             (obj) =>
                             {
                                 try
                                 {
                                     int taskChannelID = (int)obj;
                                     if (dChannelEpgList.ContainsKey(taskChannelID))
                                     {
                                         List<EpgCB>  lRes = new List<EpgCB>();
                                         //((fromDate - 1 Day) <= start_date <toDate)
                                         lRes = m_oEpgCouchbase.GetChannelProgramsByStartDate(nPageSize, nStartIndex, taskChannelID, fromDate.AddDays(-1), toDate, false);

                                         if (lRes != null && lRes.Count > 0)
                                         {
                                             lRes.RemoveAll(x => x.EndDate < fromDate); //remove Epgs that ended before fromUTCDay
                                             List<EPGChannelProgrammeObject> lProg = ConvertEpgCBtoEpgProgramm(lRes);
                                             dChannelEpgList[taskChannelID].AddRange(lProg);                                                                                    
                                         }
                                     }
                                 }
                                 catch (Exception ex)
                                 {
                                     log.Message = string.Format("GetMultiChannelProgramsDic had an exception : ex={0} in {1}", ex.Message, ex.StackTrace);
                                     log.Error(log.Message, false);
                                 }
                             }, nChannel);
                    }

                    //Wait for all parallels tasks to finish:
                    Task.WaitAll(tasks);
                }
                return dChannelEpgList;
            }
        }
        
        //get 'current' Epgs - next, previous and current Epgs, per channel
        public override ConcurrentDictionary<int, List<EPGChannelProgrammeObject>> GetMultiChannelProgramsDicCurrent(int nNextTop, int nPrevTop, List<int> lChannelIDs)
        {
            using (Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true))
            {
                log.Method = "BaseEpgBL.GetMultiChannelProgramsDic";
                ConcurrentDictionary<int, List<EPGChannelProgrammeObject>> dChannelEpgList = EpgBL.Utils.createDic(lChannelIDs);
                DateTime now = DateTime.UtcNow;
                int nGoBack = -1;
                if (lChannelIDs != null && lChannelIDs.Count > 0)
                {
                    int nChannelCount = lChannelIDs.Count;
                    //Start MultiThread Call
                    Task[] tasks = new Task[nChannelCount];
                    for (int i = 0; i < nChannelCount; i++)
                    {
                        int nChannel = lChannelIDs[i];

                        tasks[i] = Task.Factory.StartNew(
                             (obj) =>
                             {
                                 try
                                 {
                                     int taskChannelID = (int)obj;
                                     if (dChannelEpgList.ContainsKey(taskChannelID))
                                     {
                                         List<EpgCB> lTotal = new List<EpgCB>();

                                         //Next includes: (now <= start date < (now + 7 Days))
                                         List<EpgCB> lRes = m_oEpgCouchbase.GetChannelProgramsByStartDate(nNextTop, 0, taskChannelID, now, now.AddDays(DAYSBUFFER), false);
                                         if (lRes != null && lRes.Count > 0)
                                         {
                                             lTotal.AddRange(lRes);                                            
                                         }

                                         //Current: ((now-1 Day) <= start_date < now)
                                         //assuming that the current programs are not more than 24h long
                                         lRes = m_oEpgCouchbase.GetChannelProgramsByStartDate(0, 0, taskChannelID, now.AddDays(nGoBack), now, false);
                                         if (lRes != null && lRes.Count > 0)
                                         {
                                             lRes.RemoveAll(x => x.EndDate < now); //remove Epgs that ended before now
                                             lTotal.AddRange(lRes);                                            
                                         }

                                         //Prev includes: (now-7 Days) <= start_date < now
                                         //the results might include one extra EPG that has not ended yet ==> we take the nPrevTop + 1 results after sorting them in Descending order
                                         lRes = m_oEpgCouchbase.GetChannelProgramsByStartDate(nPrevTop + 1, 0, taskChannelID, now.AddDays(-DAYSBUFFER), now, true);
                                         if (lRes != null && lRes.Count > 0)
                                         {
                                             lRes.RemoveAll(x => x.EndDate > now); //remove Epgs that ended before now
                                             if (lRes.Count > nPrevTop) //remove the extra EPG, if needed
                                             {
                                                 lRes.RemoveAt(nPrevTop);
                                             }
                                             lTotal.AddRange(lRes);                                            
                                         }

                                         //return only distinct epgs
                                         lTotal.Select(x => x.EpgID).Distinct().ToList();                                      

                                         //order the results by start date  
                                         var query = lTotal.OrderBy(s => s.StartDate).Select(s => s);
                                         lTotal = query.ToList();

                                         dChannelEpgList[taskChannelID] = ConvertEpgCBtoEpgProgramm(lTotal);                                       
                                     }
                                 }
                                 catch (Exception ex)
                                 {
                                     log.Message = string.Format("GetMultiChannelProgramsDic had an exception : ex={0} in {1}", ex.Message, ex.StackTrace);
                                     log.Error(log.Message, false);
                                 }
                             }, nChannel);
                    }

                    //Wait for all parallels tasks to finish:
                    Task.WaitAll(tasks);
                }
                return dChannelEpgList;
            }
        }

        public override List<EPGChannelProgrammeObject> GetEpgs(List<int> lIds)
        {
            List<string> lIdsStrings = new List<string>();
            lIdsStrings = lIds.ConvertAll<string>(x => x.ToString());
            List<EpgCB> lResCB = m_oEpgCouchbase.GetProgram(lIdsStrings);
            
            //remove items that do not match the group ID
            int count = lResCB.Count;
            for (int i = 0; i < count; i++)
            {
                if (lResCB[i] != null && lResCB[i].ParentGroupID != m_nGroupID)
                    lResCB.RemoveAt(i);
            }

            List<EPGChannelProgrammeObject> lRes = ConvertEpgCBtoEpgProgramm(lResCB);
            return lRes;
        }

        #region Private
        private static EPGChannelProgrammeObject ConvertEpgCBtoEpgProgramm(EpgCB epg)
        {
            EPGChannelProgrammeObject oProg = new EPGChannelProgrammeObject();
            oProg = new EPGChannelProgrammeObject();
            EPGDictionary dicEpgl;

            List<EPGDictionary> lMetas = new List<EPGDictionary>();
            foreach (string key in epg.Metas.Keys)
            {
                foreach (string val in epg.Metas[key])
                {
                    dicEpgl = new EPGDictionary();
                    dicEpgl.Key = key;
                    dicEpgl.Value = val;
                    lMetas.Add(dicEpgl);
                }
            }

            List<EPGDictionary> lTags = new List<EPGDictionary>();
            foreach (string key in epg.Tags.Keys)
            {
                foreach (string val in epg.Tags[key])
                {
                    dicEpgl = new EPGDictionary();
                    dicEpgl.Key = key;
                    dicEpgl.Value = val;
                    lTags.Add(dicEpgl);
                }
            }

            int nUPDATER_ID = 0;                      //not in use
            DateTime nPUBLISH_DATE = DateTime.UtcNow; //not in use  
            oProg.Initialize((long)epg.EpgID, epg.ChannelID.ToString(), epg.EpgIdentifier, epg.Name, epg.Description, epg.StartDate.ToString("dd/MM/yyyy HH:mm:ss"), epg.EndDate.ToString("dd/MM/yyyy HH:mm:ss"), epg.PicUrl, epg.Status.ToString(),
                epg.isActive.ToString(), epg.GroupID.ToString(), nUPDATER_ID.ToString(), epg.UpdateDate.ToString(), nPUBLISH_DATE.ToString(), epg.CreateDate.ToString(), lTags, lMetas, epg.ExtraData.MediaID.ToString(), (int)epg.Statistics.Likes);
            return oProg;
        }

        private static List<EPGChannelProgrammeObject> ConvertEpgCBtoEpgProgramm(List<EpgCB> epgList)
        {
            List<EPGChannelProgrammeObject> lProg = new List<EPGChannelProgrammeObject>();
            foreach (EpgCB epg in epgList)
            {
                if (epg != null)
                    lProg.Add(ConvertEpgCBtoEpgProgramm(epg));
            }
            return lProg;
        }
        #endregion

        #region Not Implement

        public override List<EPGChannelProgrammeObject> SearchEPGContent(int groupID, string searchValue, int pageIndex, int pageSize)
        {
            return new List<EPGChannelProgrammeObject>();
        }
        public override List<EPGChannelProgrammeObject> GetEPGProgramsByScids(int groupID, string[] scids, Language eLang, int duration)
        {
            return new List<EPGChannelProgrammeObject>();
        }
        public override List<EPGChannelProgrammeObject> GetEPGProgramsByProgramsIdentefier(int groupID, string[] pids, Language eLang, int duration)
        {
            return new List<EPGChannelProgrammeObject>();
        }
        #endregion
    }
}
