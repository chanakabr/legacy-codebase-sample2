using ApiObjects;
using ApiObjects.Epg;
using ApiObjects.SearchObjects;
using Phx.Lib.Appconfig;
using DalCB;
using Phx.Lib.Log;

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Core.Api;
using Core.GroupManagers;
using TVinciShared;

namespace EpgBL
{
    public class TvinciEpgBL : BaseEpgBL
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        protected EpgDal_Couchbase m_oEpgCouchbase;
        private static readonly double EXPIRY_DATE = (ApplicationConfiguration.Current.EPGDocumentExpiry.Value > 0) ? ApplicationConfiguration.Current.EPGDocumentExpiry.Value : 7;
        private static readonly int DAYSBUFFER = 7;
        private const string USE_OLD_IMAGE_SERVER_KEY = "USE_OLD_IMAGE_SERVER";

        private const string EPG_DATETIME_FORMAT = "dd/MM/yyyy HH:mm:ss";

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
                if (newEpgItem == null)
                    return false;

                for (int i = 0; i < 3 && !bRes; i++)
                {
                    ulong nNewID = newEpgItem.EpgID;


                    bRes = (cas.HasValue) ? m_oEpgCouchbase.InsertProgram(nNewID.ToString(), newEpgItem, newEpgItem.EndDate.AddDays(EXPIRY_DATE), cas.Value) :
                                            m_oEpgCouchbase.InsertProgram(nNewID.ToString(), newEpgItem, newEpgItem.EndDate.AddDays(EXPIRY_DATE));

                    if (bRes)
                    {
                        epgID = nNewID;
                    }
                    else
                    {
                        log.Error("InsertEpg - " + string.Format("Failed insert to CB id={0}", nNewID));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("InsertEpg = " + string.Format("Exception, EpgID={0}, EpgIdentifier={1}, ChannelID={2}, ex={3} , ST: {4}",
                   newEpgItem.EpgID, newEpgItem.EpgIdentifier, newEpgItem.ChannelID, ex.Message, ex.StackTrace), ex);
            }
            return bRes;
        }

        public override bool InsertEpg(EpgCB newEpgItem, bool isMainLang, out string docID, ulong? cas = null)
        {
            //epgID = 0;
            bool bRes = false;
            docID = string.Empty;
            try
            {
                if (newEpgItem == null)
                    return false;

                for (int i = 0; i < 3 && !bRes; i++)
                {
                    if (isMainLang)
                    {
                        docID = newEpgItem.EpgID.ToString();
                    }
                    else
                    {
                        docID = string.Format("epg_{0}_lang_{1}", newEpgItem.EpgID, newEpgItem.Language.ToLower());
                    }

                    bRes = (cas.HasValue) ? m_oEpgCouchbase.InsertProgram(docID, newEpgItem, newEpgItem.EndDate.AddDays(EXPIRY_DATE), cas.Value) :
                                            m_oEpgCouchbase.InsertProgram(docID, newEpgItem, newEpgItem.EndDate.AddDays(EXPIRY_DATE));
                }

                if (!bRes)
                {
                    log.Error("InsertEpg - " + string.Format("Failed insert to CB id={0}", docID));
                    docID = string.Empty;
                }
            }
            catch (Exception ex)
            {
                log.Error("InsertEpg - " + string.Format("Exception, EpgID={0}, EpgIdentifier={1}, ChannelID={2}, ex={3} , ST: {4}",
                   newEpgItem.EpgID, newEpgItem.EpgIdentifier, newEpgItem.ChannelID, ex.Message, ex.StackTrace), ex);
            }
            return bRes;
        }

        public override bool SetEpg(EpgCB newEpgItem, out ulong epgID, ulong? cas = null)
        {
            epgID = 0;

            bool bRes = false;
            try
            {
                if (newEpgItem == null)
                    return false;

                for (int i = 0; i < 3 && !bRes; i++)
                {
                    ulong nNewID = newEpgItem.EpgID;

                    bRes = (cas.HasValue) ? m_oEpgCouchbase.SetProgram(nNewID.ToString(), newEpgItem, newEpgItem.EndDate.AddDays(EXPIRY_DATE), cas.Value) :
                                            m_oEpgCouchbase.SetProgram(nNewID.ToString(), newEpgItem, newEpgItem.EndDate.AddDays(EXPIRY_DATE));

                    if (bRes)
                    {
                        epgID = nNewID;
                    }
                    else
                    {
                        log.Error("InsertEpg - " + string.Format("Failed insert to CB id={0}", nNewID));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("InsertEpg - " + string.Format("Exception, EpgID={0}, EpgIdentifier={1}, ChannelID={2}, ex={3} , ST: {4}",
                   newEpgItem.EpgID, newEpgItem.EpgIdentifier, newEpgItem.ChannelID, ex.Message, ex.StackTrace), ex);
            }
            return bRes;
        }

        public override bool UpdateEpg(EpgCB newEpgItem, ulong? cas = null)
        {
            bool result = false;

            for (int i = 0; i < 3 && !result; i++)
            {
                string documentId = newEpgItem.DocumentId;

                if (string.IsNullOrEmpty(documentId))
                {
                    documentId = newEpgItem.EpgID.ToString();
                }

                var expiresAt = newEpgItem.EndDate.AddDays(EXPIRY_DATE);

                result = (cas.HasValue) ? m_oEpgCouchbase.UpdateProgram(documentId, newEpgItem, expiresAt, cas.Value) :
                                        m_oEpgCouchbase.UpdateProgram(documentId, newEpgItem, expiresAt);
            }

            return result;
        }

        public override bool UpdateEpg(EpgCB newEpgItem, bool isMainLang, out string docID, ulong? cas = null)
        {
            bool bRes = false;
            docID = string.Empty;
            try
            {
                if (newEpgItem == null)
                    return false;

                for (int i = 0; i < 3 && !bRes; i++)
                {
                    if (isMainLang)
                    {
                        docID = newEpgItem.EpgID.ToString();
                    }
                    else
                    {
                        docID = string.Format("epg_{0}_lang_{1}", newEpgItem.EpgID, newEpgItem.Language.ToLower());
                    }

                    bRes = (cas.HasValue) ? m_oEpgCouchbase.UpdateProgram(docID, newEpgItem, newEpgItem.EndDate.AddDays(EXPIRY_DATE), cas.Value) :
                                            m_oEpgCouchbase.UpdateProgram(docID, newEpgItem, newEpgItem.EndDate.AddDays(EXPIRY_DATE));
                }

                if (!bRes)
                {
                    log.Error("InsertEpg - " + string.Format("Failed insert to CB id={0}", docID));
                    docID = string.Empty;
                }
            }
            catch (Exception ex)
            {
                log.Error("InsertEpg - " + string.Format("Exception, EpgID={0}, EpgIdentifier={1}, ChannelID={2}, ex={3} , ST: {4}",
                   newEpgItem.EpgID, newEpgItem.EpgIdentifier, newEpgItem.ChannelID, ex.Message, ex.StackTrace), ex);
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
            }

            return bRes;
        }

        public void RemoveEpg(List<ulong> lIDs)
        {
            bool bRemove = false;
            foreach (ulong id in lIDs)
            {
                bRemove = this.RemoveEpg(id);
                log.Debug("delete - " + string.Format("remove id = {0}, success = {1}", id, bRemove));
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

        public override void RemoveGroupPrograms(List<int> lprogramIDs)
        {
            List<string> lIdsStrings = lprogramIDs.ConvertAll<string>(x => x.ToString());
            List<EpgCB> lResCB = m_oEpgCouchbase.GetProgram(lIdsStrings);

            foreach (EpgCB epg in lResCB)
            {
                epg.Status = 2;
                UpdateEpg(epg);
            }
        }
        public override void RemoveGroupPrograms(List<string> docIds)
        {
            List<EpgCB> lResCB = m_oEpgCouchbase.GetProgram(docIds);

            foreach (EpgCB epg in lResCB)
            {
                epg.Status = 2;
                UpdateEpg(epg);
            }
        }

        public override void RemoveGroupPrograms(List<DateTime> lDates, int channelID)
        {
            List<EpgCB> lExisitingPrograms = new List<EpgCB>();
            Dictionary<ulong, EpgCB> dExisitingPrograms = new Dictionary<ulong, EpgCB>();
            foreach (DateTime date in lDates)
            {
                List<EpgCB> lTempPrograms = new List<EpgCB>();
                lTempPrograms = this.GetChannelPrograms(0, 0, channelID, date, date.AddDays(1));
                log.Debug("RemoveGroupPrograms - " + string.Format("Date = {0}, channelID ={1}", date, channelID));
                log.Debug("RemoveGroupPrograms - " + string.Format("lTempPrograms count ={0}", lTempPrograms != null ? lTempPrograms.Count : 0));


                if (lTempPrograms != null && lTempPrograms.Count > 0)
                {
                    foreach (EpgCB item in lTempPrograms)
                    {
                        log.Debug("RemoveGroupPrograms - " + string.Format("item = {0}", item.EpgID));

                        if (!lExisitingPrograms.Exists(x => x.EpgID == item.EpgID))
                        {
                            lExisitingPrograms.Add(item);
                        }
                        //if (!dExisitingPrograms.ContainsKey(item.EpgID))
                        //{
                        //    dExisitingPrograms.Add(item.EpgID, item);
                        //    lExisitingPrograms.Add(item);
                        //}
                    }
                }
            }

            log.Debug("RemoveGroupPrograms - " + string.Format("lExisitingPrograms count ={0}", lExisitingPrograms != null ? lExisitingPrograms.Count : 0));

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

        public override void RemoveGroupPrograms(DateTime? fromDate, DateTime? toDate, int channelID)
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

        public override EpgCB GetEpgCB(ulong nProgramID, bool includeRecordingFallback = false)
        {
            EpgCB oRes;

            var docKey = GetEpgCBKey(m_nGroupID, (long)nProgramID);

            if (includeRecordingFallback)
            {
                var list = m_oEpgCouchbase.GetProgram(new List<string>() { docKey });
                oRes = list.FirstOrDefault();
            }
            else
            {
                if (string.IsNullOrEmpty(docKey))
                {
                    log.Debug($"GetEpgCB docKey is empty for epgId:{nProgramID}");
                    docKey = nProgramID.ToString();
                }

                oRes = m_oEpgCouchbase.GetProgram(docKey);
            }

            return (oRes != null && oRes.ParentGroupID == m_nGroupID) ? oRes : null; ;
        }

        public override List<EpgCB> GetEpgCB(ulong nProgramID, List<string> languages, bool isAddAction = false)
        {
            try
            {
                var partialLangObjects = languages.Select(landCode => new LanguageObj { Code = landCode });
                var docIDs = GetEpgsCBKeys(m_nGroupID,new[] { (long)nProgramID }, partialLangObjects, isAddAction);
                var defaultLangDocId = GetEpgCBKey(m_nGroupID, (long)nProgramID, null, isAddAction);
                docIDs.Add(defaultLangDocId);
                var lResCB = m_oEpgCouchbase.GetProgram(docIDs);

                return lResCB;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed in GetEpgCB. Program Id = {0}. error = {1}", nProgramID, ex);
                return new List<EpgCB>();
            }
        }

        public override List<EPGChannelProgrammeObject> GetEpgCBsWithLanguage(List<ulong> programIDs, List<LanguageObj> languages, bool isOpcAccount)
        {
            var result = new List<EPGChannelProgrammeObject>();
            var resultForMultilingual = new List<EPGChannelProgrammeObject>();

            try
            {
                var docID = string.Empty;
                var docIDs = new List<string>();
                LanguageObj defaultLanguage = null;
                List<EpgCB> cbEpgs = null;

                if (languages != null && languages.Count > 0)
                {
                    // in case default Language is needed calling the default CB document with program Id
                    defaultLanguage = languages.FirstOrDefault(x => x.IsDefault);
                    if (defaultLanguage != null)
                    {
                        foreach (var programId in programIDs)
                        {
                            docID = GetEpgCBKey(m_nGroupID, (long)programId);
                            docIDs.Add(docID);
                        }

                        cbEpgs = m_oEpgCouchbase.GetProgram(docIDs);
                        result = ConvertEpgCBtoEpgProgramm(cbEpgs);

                        // update all EPGChannelProgrammeObjects (name, description, tags, meta) with language details. 
                        UpdateProgrammeWithMultilingual(ref result, defaultLanguage);
                    }

                    var noneDefaultLanguages = languages.Where(x => !x.IsDefault).ToList();
                    if (noneDefaultLanguages != null)
                    {
                        foreach (var languageObj in noneDefaultLanguages)
                        {
                            docIDs = new List<string>();
                            foreach (ulong programId in programIDs)
                            {
                                docID = GetEpgCBKey(m_nGroupID, (long)programId, languageObj.Code.ToLower());
                                docIDs.Add(docID);
                            }
                            cbEpgs = m_oEpgCouchbase.GetProgram(docIDs);
                            resultForMultilingual = ConvertEpgCBtoEpgProgramm(cbEpgs);

                            // update all EPGChannelProgrammeObjects (name, description, tags, meta) with language details. 
                            if (result == null || result.Count == 0)
                            {
                                UpdateProgrammeWithMultilingual(ref resultForMultilingual, languageObj);
                                result = resultForMultilingual;
                            }
                            else
                            {
                                UpdateProgrammeWithMultilingual(ref result, languageObj, resultForMultilingual);
                            }
                        }
                    }
                }

                // get picture sizes from DB
                List<Ratio> epgRatios = new List<Ratio>();
                Dictionary<int, List<EpgPicture>> pictures = Tvinci.Core.DAL.CatalogDAL.GetGroupTreeMultiPicEpgUrl(m_nGroupID, ref epgRatios);

                MutateFullEpgPicURL(result, pictures, m_nGroupID, isOpcAccount);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed to get EPGs from CB with lang = {0}, Exception: {1}", languages != null && languages.Count > 0 ? string.Join(",", languages) : string.Empty, ex);
            }

            return result;
        }

        public override List<EPGChannelProgrammeObject> GetEpgCBsWithLanguage(List<ulong> programIDs, string language, bool isOpcAccount)
        {
            List<EPGChannelProgrammeObject> result = new List<EPGChannelProgrammeObject>();

            try
            {
                string docID = string.Empty;
                List<string> docIDs = new List<string>();
                //Build list of keys with language
                foreach (ulong programId in programIDs)
                {
                    docID = GetEpgCBKey(m_nGroupID, (long)programId, language);
                    docIDs.Add(docID);

                }
                List<EpgCB> cbEpgs = m_oEpgCouchbase.GetProgram(docIDs);
                result = ConvertEpgCBtoEpgProgramm(cbEpgs);

                // get picture sizes from DB
                List<Ratio> epgRatios = new List<Ratio>();
                Dictionary<int, List<EpgPicture>> pictures = Tvinci.Core.DAL.CatalogDAL.GetGroupTreeMultiPicEpgUrl(m_nGroupID, ref epgRatios);

                MutateFullEpgPicURL(result, pictures, m_nGroupID, isOpcAccount);
            }
            catch (Exception)
            {
                log.ErrorFormat("Failed to get EPGs from CB with lang = {0}", language);
            }

            return result;
        }

        public override EpgCB GetEpgCB(ulong nProgramID, out ulong cas)
        {
            var docId = GetEpgCBKey(m_nGroupID, (long)nProgramID);
            EpgCB oRes = m_oEpgCouchbase.GetProgram(docId, out cas);
            oRes = (oRes != null && oRes.ParentGroupID == m_nGroupID) ? oRes : null;
            return oRes;
        }

        public override EpgCB GetEpgCB(string ProgramID, out ulong cas)
        {
            // TODO: Arthur: I think this is not in use, see if we can remove this
            EpgCB oRes = m_oEpgCouchbase.GetProgram(ProgramID, out cas);
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

        public List<EpgCB> GetGroupEpgs(int nPageSize, int nStartIndex, DateTime? dfromDate, DateTime? dToDate, bool falseStaleState = false, long bulkSize = 0)
        {
            List<EpgCB> lRes = null;

            if (dfromDate.HasValue && dToDate.HasValue)
            {
                lRes = m_oEpgCouchbase.GetGroupProgramsByStartDate(nPageSize, nStartIndex, dfromDate.Value, dToDate.Value, falseStaleState, bulkSize);
            }
            else if (dfromDate.HasValue)
            {
                lRes = m_oEpgCouchbase.GetGroupProgramsByStartDate(nPageSize, nStartIndex, dfromDate.Value, falseStaleState, bulkSize);
            }
            else
            {
                lRes = m_oEpgCouchbase.GetGroupPrograms(nPageSize, nStartIndex, falseStaleState, bulkSize);
            }


            return lRes;
        }

        public List<EpgCB> GetGroupEpgsWithBulkSize(DateTime? dfromDate, DateTime? dToDate, bool falseStaleState, int bulkSize)
        {
            List<EpgCB> epgs = new List<EpgCB>();
            int pageSize = 0, startIndex = 0;
            if (bulkSize > 0)
            {
                pageSize = bulkSize;
            }

            bool shouldGetNextPage = true;
            while (shouldGetNextPage)
            {
                List<EpgCB> page = GetGroupEpgs(pageSize, startIndex, dfromDate, dToDate, falseStaleState, bulkSize);
                if (page?.Count > 0)
                {
                    epgs.AddRange(page);
                    page = null;
                }
                else
                {
                    shouldGetNextPage = false;
                }

                startIndex += pageSize;
            }


            return epgs;
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
                // save monitor and logs context data
                LogContextData contextData = new LogContextData();

                Task<List<EpgCB>>[] tChannelTasks = new Task<List<EpgCB>>[lChannelIDs.Count];
                for (int i = 0; i < lChannelIDs.Count; i++)
                {
                    tChannelTasks[i] = Task.Run<List<EpgCB>>(() =>
                        {
                            // load monitor and logs context data
                            contextData.Load();

                            return GetChannelPrograms(nPageSize, nStartIndex, lChannelIDs[i], fromUTCDay, toUTCDay);
                        });
                }

                Task.WaitAll(tChannelTasks);

                foreach (Task<List<EpgCB>> task in tChannelTasks)
                {
                    if (task != null)
                    {
                        if (task.Result != null && task.Result.Count > 0)
                        {
                            lRes.AddRange(task.Result);
                        }
                        task.Dispose();
                    }
                }
            }
            return lRes;
        }

        //will need to return an object containing two dictionaries one for metas and the other for tags maybe we'll want to store it in cache and update when needed
        public EpgGroupSettings GetGroupEpgTagsAndMetas(bool bIsSearchable)
        {
            DataSet ds = Tvinci.Core.DAL.EpgDal.Get_GroupsTagsAndMetas(m_nGroupID, new List<int>());
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
                log.Error("Error - " + string.Format("Caugh exception when fetching EPG group tags and metas. Ex={0}; stack={1}", ex.Message, ex.StackTrace), ex);
            }

            return egs;
        }

        //get all EPgs in the given range, including Epgs that are partially overlapping
        public override ConcurrentDictionary<int, List<EPGChannelProgrammeObject>> GetMultiChannelProgramsDic(int nPageSize, int nStartIndex, List<int> lChannelIDs, DateTime fromDate, DateTime toDate)
        {
            ConcurrentDictionary<int, List<EPGChannelProgrammeObject>> dChannelEpgList = EpgBL.Utils.createDic(lChannelIDs);

            if (lChannelIDs != null && lChannelIDs.Count > 0)
            {
                // save monitor and logs context data
                LogContextData contextData = new LogContextData();

                Task[] tasks = new Task[lChannelIDs.Count];

                for (int i = 0; i < lChannelIDs.Count; i++)
                {
                    int nChannel = lChannelIDs[i];

                    tasks[i] = Task.Run(() =>
                         {
                             // load monitor and logs context data
                             contextData.Load();
                             try
                             {
                                 if (dChannelEpgList.ContainsKey(nChannel))
                                 {
                                     List<EpgCB> lRes = new List<EpgCB>();
                                     //((fromDate - 1 Day) <= start_date <toDate)
                                     lRes = m_oEpgCouchbase.GetChannelProgramsByStartDate(nPageSize, nStartIndex, nChannel, fromDate.AddDays(-1), toDate, false);

                                     if (lRes != null && lRes.Count > 0)
                                     {
                                         lRes.RemoveAll(x => x.EndDate < fromDate); //remove Epgs that ended before fromUTCDay
                                         List<EPGChannelProgrammeObject> lProg = ConvertEpgCBtoEpgProgramm(lRes);
                                         dChannelEpgList[nChannel].AddRange(lProg);
                                     }
                                 }
                             }
                             catch (Exception ex)
                             {
                                 log.Error("Exception - " + string.Format("Exception at GetMultiChannelProgramsDic task. C ID: {0} , Msg: {1} , ST: {2}", nChannel, ex.Message, ex.StackTrace), ex);
                             }
                         });
                }

                //Wait for all parallels tasks to finish:
                Task.WaitAll(tasks);
                if (tasks != null && tasks.Length > 0)
                {
                    for (int i = 0; i < tasks.Length; i++)
                    {
                        if (tasks[i] != null)
                        {
                            tasks[i].Dispose();
                        }
                    }
                }
            }
            return dChannelEpgList;
        }

        //get 'current' Epgs - next, previous and current Epgs, per channel
        public override ConcurrentDictionary<int, List<EPGChannelProgrammeObject>> GetMultiChannelProgramsDicCurrent(int nNextTop, int nPrevTop, List<int> lChannelIDs)
        {
            ConcurrentDictionary<int, List<EPGChannelProgrammeObject>> dChannelEpgList = EpgBL.Utils.createDic(lChannelIDs);
            DateTime now = DateTime.UtcNow;
            int nGoBack = -1;
            if (lChannelIDs != null && lChannelIDs.Count > 0)
            {
                // save monitor and logs context data
                LogContextData contextData = new LogContextData();

                //Start MultiThread Call
                Task[] tasks = new Task[lChannelIDs.Count];
                for (int i = 0; i < lChannelIDs.Count; i++)
                {
                    int nChannel = lChannelIDs[i];

                    tasks[i] = Task.Run(() =>
                         {
                             // load monitor and logs context data
                             contextData.Load();
                             try
                             {
                                 if (dChannelEpgList.ContainsKey(nChannel))
                                 {
                                     List<EpgCB> lTotal = new List<EpgCB>();

                                     //Next includes: (now <= start date < (now + 7 Days))
                                     List<EpgCB> lRes = m_oEpgCouchbase.GetChannelProgramsByStartDate(nNextTop, 0, nChannel, now, now.AddDays(DAYSBUFFER), false);
                                     if (lRes != null && lRes.Count > 0)
                                     {
                                         lTotal.AddRange(lRes);
                                     }

                                     //Current: ((now-1 Day) <= start_date < now)
                                     //assuming that the current programs are not more than 24h long
                                     lRes = m_oEpgCouchbase.GetChannelProgramsByStartDate(0, 0, nChannel, now.AddDays(nGoBack), now, false);
                                     if (lRes != null && lRes.Count > 0)
                                     {
                                         lRes.RemoveAll(x => x.EndDate < now); //remove Epgs that ended before now
                                         lTotal.AddRange(lRes);
                                     }

                                     //Prev includes: (now-7 Days) <= start_date < now
                                     //the results might include one extra EPG that has not ended yet ==> we take the nPrevTop + 1 results after sorting them in Descending order
                                     lRes = m_oEpgCouchbase.GetChannelProgramsByStartDate(nPrevTop + 1, 0, nChannel, now.AddDays(-DAYSBUFFER), now, true);
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

                                     dChannelEpgList[nChannel] = ConvertEpgCBtoEpgProgramm(lTotal);
                                 }
                             }
                             catch (Exception ex)
                             {
                                 log.Error("Exception - " + string.Format("Exception at GetMultiChannelProgramsDicCurrent. C ID: {0} , Msg: {1} ST: {2}", nChannel, ex.Message, ex.StackTrace), ex);
                             }
                         });
                }

                //Wait for all parallels tasks to finish:
                Task.WaitAll(tasks);
                if (tasks != null && tasks.Length > 0)
                {
                    for (int i = 0; i < tasks.Length; i++)
                    {
                        if (tasks[i] != null)
                        {
                            tasks[i].Dispose();
                        }
                    }
                }
            }
            return dChannelEpgList;
        }


        public override List<EPGChannelProgrammeObject> GetEpgs(List<int> lIds, bool isOpcAccount)
        {
            var lIdsStrings = lIds.ConvertAll(x => GetEpgCBKey(m_nGroupID, x));
            return GetEpgChannelProgrammeObjects(lIdsStrings, isOpcAccount);
        }

        public override List<EPGChannelProgrammeObject> GetEpgChannelProgrammeObjects(List<string> lIdsStrings, bool isOpcAccount)
        {
            // get EPG programs
            List<EpgCB> result = m_oEpgCouchbase.GetProgram(lIdsStrings);

            List<EPGChannelProgrammeObject> epgChannelProgram = null;
            if (result != null)
            {
                // convert objects
                epgChannelProgram = ConvertEpgCBtoEpgProgramm(result.Where(item => item != null && item.ParentGroupID == m_nGroupID));

                // get picture sizes from DB
                List<Ratio> epgRatios = new List<Ratio>();
                Dictionary<int, List<EpgPicture>> pictures = Tvinci.Core.DAL.CatalogDAL.GetGroupTreeMultiPicEpgUrl(m_nGroupID, ref epgRatios);

                MutateFullEpgPicURL(epgChannelProgram, pictures, m_nGroupID, isOpcAccount);
            }

            return epgChannelProgram;
        }

        private static void MutateFullEpgPicURL(List<EPGChannelProgrammeObject> epgList, Dictionary<int, List<EpgPicture>> pictures, int groupId, bool isOpcAccount)
        {
            try
            {
                if (WS_Utils.IsGroupIDContainedInConfig(groupId, ApplicationConfiguration.Current.UseOldImageServer.Value, ';'))
                {
                    // use old image server flow
                    MutateFullEpgPicURLOldImageServerFlow(epgList, pictures);
                }
                else
                {
                    EpgPicture pictureItem;
                    List<EpgPicture> finalEpgPicture = null;
                    foreach (ApiObjects.EPGChannelProgrammeObject oProgram in epgList)
                    {
                        int progGroup = int.Parse(oProgram.GROUP_ID);
                        // fix for GEN-1580 - EPG images are not returned with pic sizes on TVPAPI after OPC migration
                        // if it's an OPC account and the epg groupId (one that was migrated but ingested with "regular" groupId prior to migration) is not the parent
                        // groupId (passed in the method) then it means we need to use the parent groupId value instead
                        if (isOpcAccount && progGroup != groupId)
                        {
                            progGroup = groupId;
                        }
                        
                        finalEpgPicture = new List<EpgPicture>();
                        if (oProgram.EPG_PICTURES != null && oProgram.EPG_PICTURES.Count > 0) // work with list of pictures --LUNA version 
                        {
                            foreach (EpgPicture pict in oProgram.EPG_PICTURES)
                            {
                                // get picture base URL
                                string picBaseName = Path.GetFileNameWithoutExtension(pict.Url);

                                if (pictures == null || !pictures.ContainsKey(progGroup))
                                {
                                    pictureItem = new EpgPicture();
                                    pictureItem.Ratio = pict.Ratio;

                                    //BEO-11508
                                    pictureItem.ImageTypeId = pict.ImageTypeId;

                                    // build image URL. 
                                    // template: <image_server_url>/p/<partner_id>/entry_id/<image_id>/version/<image_version>
                                    // Example:  http://localhost/ImageServer/Service.svc/GetImage/p/215/entry_id/123/version/10
                                    pictureItem.Url = ImageUtils.BuildImageUrl(groupId, picBaseName, 0, 0, 0, 100, true);

                                    finalEpgPicture.Add(pictureItem);
                                }
                                else
                                {
                                    if (!pictures.ContainsKey(progGroup))
                                        continue;

                                    List<EpgPicture> ratios = pictures[progGroup].Where(x => x.Ratio == pict.Ratio).ToList();

                                    foreach (EpgPicture ratioItem in ratios)
                                    {
                                        pictureItem = new EpgPicture();
                                        pictureItem.Ratio = pict.Ratio;
                                        pictureItem.PicHeight = ratioItem.PicHeight;
                                        pictureItem.PicWidth = ratioItem.PicWidth;
                                        pictureItem.Version = 0;
                                        pictureItem.Id = picBaseName;

                                        // build image URL. 
                                        // template: <image_server_url>/p/<partner_id>/entry_id/<image_id>/version/<image_version>/width/<image_width>/height/<image_height>/quality/<image_quality>
                                        // Example:  http://localhost/ImageServer/Service.svc/GetImage/p/215/entry_id/123/version/10/width/432/height/230/quality/100
                                        pictureItem.Url = ImageUtils.BuildImageUrl(groupId, picBaseName, 0, ratioItem.PicWidth, ratioItem.PicHeight, 100);

                                        finalEpgPicture.Add(pictureItem);
                                    }
                                }
                            }
                        }

                        oProgram.EPG_PICTURES = finalEpgPicture; // Reassignment epg pictures
                        if (!string.IsNullOrEmpty(oProgram.PIC_URL) && pictures != null && pictures.Count > 0 && pictures.ContainsKey(progGroup))
                        {
                            oProgram.PIC_URL = CompletePicURLForBackSupport(oProgram.PIC_URL, pictures[progGroup]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("MutateFullEpgPicURL - " + string.Format("Failed ex={0}", ex.Message), ex);
            }
        }

        private static void MutateFullEpgPicURLOldImageServerFlow(List<EPGChannelProgrammeObject> epgList, Dictionary<int, List<EpgPicture>> pictures)
        {
            try
            {
                EpgPicture pictureItem;

                List<EpgPicture> finalEpgPicture = null;
                foreach (ApiObjects.EPGChannelProgrammeObject oProgram in epgList)
                {
                    int group = int.Parse(oProgram.GROUP_ID);
                    if (!pictures.ContainsKey(group))
                    {
                        continue;
                    }

                    finalEpgPicture = new List<EpgPicture>();
                    if (oProgram.EPG_PICTURES != null && oProgram.EPG_PICTURES.Count > 0) // work with list of pictures --LUNA version 
                    {
                        foreach (EpgPicture pict in oProgram.EPG_PICTURES)
                        {
                            List<EpgPicture> ratios = pictures[group].Where(x => x.Ratio == pict.Ratio).ToList();

                            foreach (EpgPicture ratioItem in ratios)
                            {
                                pictureItem = new EpgPicture();
                                pictureItem.Ratio = pict.Ratio;
                                pictureItem.PicHeight = ratioItem.PicHeight;
                                pictureItem.PicWidth = ratioItem.PicWidth;

                                pictureItem.Url = pict.Url;
                                if (ratioItem.PicHeight != 0 && ratioItem.PicWidth != 0)
                                {
                                    pictureItem.Url = pictureItem.Url.Replace(".", string.Format("_{0}X{1}.", ratioItem.PicWidth, ratioItem.PicHeight));
                                }
                                pictureItem.Url = string.Format("{0}{1}", ratioItem.Url, pictureItem.Url);

                                finalEpgPicture.Add(pictureItem);
                            }
                        }
                    }

                    oProgram.EPG_PICTURES = finalEpgPicture; // Reassignment epg pictures
                    if (!string.IsNullOrEmpty(oProgram.PIC_URL) && pictures != null && pictures.Count > 0 && pictures.ContainsKey(group))
                    {
                        oProgram.PIC_URL = CompletePicURLForBackSupport(oProgram.PIC_URL, pictures[group]);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("MutateFullEpgPicURL - " + string.Format("Failed ex={0}", ex.Message), ex);
            }
        }

        private static string CompletePicURLForBackSupport(string oldPicURL, List<EpgPicture> pictures)
        {
            string newPicURL = oldPicURL;
            if (!string.IsNullOrEmpty(newPicURL) && pictures != null && pictures.Count > 0)
            {
                EpgPicture pict = pictures.FirstOrDefault();
                if (pict != null && !string.IsNullOrEmpty(pict.Url))
                {
                    string baseEpgPicUrl = pict.Url;
                    if (pict.PicHeight != 0 && pict.PicWidth != 0)
                    {
                        newPicURL = newPicURL.Replace(".", string.Format("_{0}X{1}.", pict.PicWidth, pict.PicHeight));
                    }
                    newPicURL = string.Format("{0}{1}", baseEpgPicUrl, newPicURL);
                }
            }

            return newPicURL;
        }

        public override List<EpgCB> GetEpgs(List<string> lIds, bool isRecordings = false)
        {
            try
            {
                if (isRecordings)
                {
                    return m_oEpgCouchbase.GetProgramFromRecordings(lIds);
                }

                lIds = lIds.Select(x => GetEpgCBKey(m_nGroupID, long.Parse(x))).ToList();

                List<EpgCB> lResCB = m_oEpgCouchbase.GetProgram(lIds);
                return lResCB;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public override List<EPGChannelProgrammeObject> GetEPGPrograms(int groupID, string[] externalids, Language eLang, int duration, bool isOpcAccount)
        {
            List<EPGChannelProgrammeObject> lRes = null;
            try
            {
                if (externalids != null && externalids.Count() > 0)
                {
                    List<EpgCB> lResCB = m_oEpgCouchbase.GetGroupPrograms(0, 0, groupID, externalids.ToList());
                    if (lResCB != null)
                    {
                        lRes = ConvertEpgCBtoEpgProgramm(lResCB.Where(item => item != null && item.ParentGroupID == m_nGroupID));

                        List<Ratio> epgRatios = new List<Ratio>();
                        Dictionary<int, List<EpgPicture>> pictures = Tvinci.Core.DAL.CatalogDAL.GetGroupTreeMultiPicEpgUrl(m_nGroupID, ref epgRatios);
                        if (pictures != null)
                        {
                            MutateFullEpgPicURL(lRes, pictures, groupID, isOpcAccount);
                        }
                    }
                }

                if (lRes == null)
                {
                    lRes = new List<EPGChannelProgrammeObject>();
                }
            }
            catch (Exception ex)
            {
                log.Error("GetEPGPrograms - " + string.Format("Failed ex={0}", ex.Message), ex);
            }
            return lRes;
        }

        public override List<EPGChannelProgrammeObject> GetChannelPrograms(int channelId, DateTime startDate, DateTime endDate, bool isOpcAccount, List<ESOrderObj> esOrderObjs = null)
        {
            List<EPGChannelProgrammeObject> result = new List<EPGChannelProgrammeObject>();

            try
            {
                var indexManager = Core.Catalog.IndexManagerFactory.Instance.GetIndexManager(m_nGroupID);
                var documentIds = indexManager.GetChannelPrograms(channelId, startDate, endDate, esOrderObjs);
                if (GroupSettingsManager.Instance.GetEpgFeatureVersion(m_nGroupID) == EpgFeatureVersion.V1)
                {
                    documentIds = GetEpgsCBKeysV1(documentIds.Select(long.Parse), null);
                }

                result = GetEpgChannelProgrammeObjects(documentIds, isOpcAccount);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error when getting channel programs for channel {0}. ex = {1}", channelId, ex);
            }

            return result;
        }

        
     
        
        
        #region Private

        public static EPGChannelProgrammeObject ConvertEpgCBtoEpgProgramm(EpgCB epg)
        {
            EPGChannelProgrammeObject oProg = new EPGChannelProgrammeObject();
            EPGDictionary dicEpgl;

            List<EPGDictionary> lMetas = new List<EPGDictionary>();
            if (epg.Metas != null && epg.Metas.Count > 0)
            {
                foreach (var meta in epg.Metas)
                {
                    // BEO-14168 meta has one single value per language by nature. Two ar more values look suspicious.
                    if (meta.Value.Count() > 1)
                    {
                        var values = string.Join(",", meta.Value);
                        
                        log.ErrorFormat("Partner: '{0}'. Program '{1}' with epg id '{2}' and identifier '{3}' has more than one values for meta '{4}'. Values: {5}",
                            epg.GroupID, epg.Name, epg.EpgID, epg.EpgIdentifier, meta.Key, values);
                    }
                    
                    foreach (var value in meta.Value)
                    {
                        dicEpgl = new EPGDictionary()
                        {
                            Key = meta.Key,
                            Value = value
                        };
                        lMetas.Add(dicEpgl);
                    }
                }
            }

            List<EPGDictionary> lTags = new List<EPGDictionary>();
            if (epg.Tags != null && epg.Tags.Count > 0)
            {
                foreach (var tag in epg.Tags)
                {
                    foreach (string val in tag.Value)
                    {
                        dicEpgl = new EPGDictionary()
                        {
                            Key = tag.Key,
                            Value = val
                        };
                        lTags.Add(dicEpgl);
                    }
                }
            }

            int nUPDATER_ID = 0;                      //not in use
            DateTime nPUBLISH_DATE = DateTime.UtcNow; //not in use  
            oProg.Initialize((long)epg.EpgID, epg.ChannelID.ToString(), epg.EpgIdentifier, epg.Name, epg.Description, epg.StartDate.ToString(EPG_DATETIME_FORMAT),
                             epg.EndDate.ToString(EPG_DATETIME_FORMAT), epg.PicUrl, epg.Status.ToString(), epg.IsActive.ToString(), epg.GroupID.ToString(), nUPDATER_ID.ToString(),
                             epg.UpdateDate.ToString(EPG_DATETIME_FORMAT), nPUBLISH_DATE.ToString(EPG_DATETIME_FORMAT), epg.CreateDate.ToString(EPG_DATETIME_FORMAT), lTags, lMetas,
                             epg.ExtraData.MediaID.ToString(), (int)epg.Statistics.Likes, epg.pictures, epg.EnableCDVR, epg.EnableCatchUp, epg.EnableStartOver, epg.EnableTrickPlay, epg.Crid);
            oProg.PIC_ID = epg.PicID;
            return oProg;
        }

        public static List<EPGChannelProgrammeObject> ConvertEpgCBtoEpgProgramm(IEnumerable<EpgCB> epgList)
        {
            List<EPGChannelProgrammeObject> lProg = new List<EPGChannelProgrammeObject>();
            foreach (EpgCB epg in epgList)
            {
                if (epg != null)
                    lProg.Add(ConvertEpgCBtoEpgProgramm(epg));
            }
            return lProg;
        }

        /// <summary>
        /// This is the main alias of all programs
        /// </summary>
        public string GetProgramIndexAlias()
        {
            return $"{m_nGroupID}_epg";
        }


        public List<string> GetEpgsCBKeys(int groupId, IEnumerable<long> epgIds, IEnumerable<LanguageObj> langCodes, bool isAddAction)
        {
            var result = new List<string>();
            var epgFeatureVersion = GroupSettingsManager.Instance.GetEpgFeatureVersion(groupId);

            if (epgFeatureVersion != EpgFeatureVersion.V1 && !isAddAction)
            {
                var indexManager = Core.Catalog.IndexManagerFactory.Instance.GetIndexManager(groupId);
                 //using the new EPG ingest the document id has a suffix cintaining the bulk upload that inserted it
                 //so there is no way for us to now what is the document id.
                 //ES holds the current document in CB so we go there to take it
                result = indexManager.GetEpgCBDocumentIdsByEpgId(epgIds, langCodes);
                //all vlues that are long get epgcbkeyssv1
                result = result.SelectMany(x =>
                {
                    return long.TryParse(x, out var parsedEpgId)
                        ? GetEpgsCBKeysV1(new[] {parsedEpgId}, langCodes)
                        : new List<string>
                        {
                            x.ToString()
                        };
                }).ToList();
            }
            else
            {
                result.AddRange(GetEpgsCBKeysV1(epgIds, langCodes));
            }

            return result.Distinct().ToList();
        }

        private List<string> GetEpgsCBKeysV1(IEnumerable<long> epgIds, IEnumerable<LanguageObj> langCodes)
        {
            var result = new List<string>();
            if (langCodes == null)
            {
                result = epgIds.Select(x => x.ToString()).ToList();
            }
            else
            {
                foreach (var epgId in epgIds)
                {
                    var keys = langCodes.Select(langCode => langCode.IsDefault ? epgId.ToString() : $"epg_{epgId}_lang_{langCode.Code.ToLower()}");

                    result.AddRange(keys.ToList());
                }
            }

            return result;
        }

        public string GetEpgCBKey(int groupId, long epgId, string langCode = null, bool isAddAction = false)
        {

            var langs = string.IsNullOrEmpty(langCode) ? null : new[] {new LanguageObj { Code = langCode } };
            var keys = GetEpgsCBKeys(groupId, new[] { epgId }, langs, isAddAction);
            return keys.FirstOrDefault();
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
