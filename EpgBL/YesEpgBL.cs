using ApiObjects;
using KLogMonitor;
using KlogMonitorHelper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace EpgBL
{
    public class YesEpgBL : BaseEpgBL
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Consts
        protected static readonly Regex HebrewRegex = new Regex(@"\p{IsHebrew}");
        protected static readonly Regex RussianRegex = new Regex(@"\p{IsCyrillic}");
        protected static readonly Regex ArabicRegex = new Regex(@"\p{IsArabic}");
        #endregion

        public YesEpgBL(int nGroupID)
        {
            this.m_nGroupID = nGroupID;
        }

        #region Public

        public override List<EPGChannelProgrammeObject> GetEPGProgramsByProgramsIdentefier(int groupID, string[] pids, Language eLang, int duration)
        {
            List<EPGChannelProgrammeObject> EPG_CPO = new List<EPGChannelProgrammeObject>();
            foreach (string id in pids)
            {
                EPGChannelProgrammeObject res = GetProgramData("pid", id, eLang, duration);
                if (res != null && !string.IsNullOrEmpty(res.EPG_CHANNEL_ID))
                {
                    EPG_CPO.Add(res);
                }
            }
            return EPG_CPO;
        }

        public override List<EPGChannelProgrammeObject> SearchEPGContent(int groupID, string searchValue, int pageIndex, int pageSize)
        {
            List<EPGChannelProgrammeObject> res = GetProgramsList(searchValue, pageIndex, pageSize);

            return res;
        }

        public override List<EPGChannelProgrammeObject> GetEPGProgramsByScids(int groupID, string[] scids, Language eLang, int duration)
        {
            List<EPGChannelProgrammeObject> progs = new List<EPGChannelProgrammeObject>();

            foreach (string scid in scids)
            {
                EPGChannelProgrammeObject res = GetProgramData("scid", scid, eLang, duration);
                if (res != null && !string.IsNullOrEmpty(res.EPG_CHANNEL_ID))
                {
                    progs.Add(res);
                }
            }

            return progs;
        }

        public override EPGChannelProgrammeObject GetEpg(ulong nProgramID)
        {
            EPGChannelProgrammeObject res = GetProgramData(nProgramID.ToString());
            return res;
        }
        public override EpgCB GetEpgCB(string ProgramID, out ulong cas)
        {
            cas = 0;
            return null;
        }
        public override List<EPGChannelProgrammeObject> GetEpgs(List<int> lIds)
        {
            List<EPGChannelProgrammeObject> res = new List<EPGChannelProgrammeObject>();
            foreach (int epgID in lIds)
            {
                res.Add(GetProgramData(epgID.ToString()));
            }
            return res;
        }

        public override List<EpgCB> GetEpgs(List<string> lIds)
        {
            return null;
        }


        public override ConcurrentDictionary<int, List<EPGChannelProgrammeObject>> GetMultiChannelProgramsDicCurrent(int nNextTop, int nPrevTop, List<int> lChannelIDs)
        {
            ConcurrentDictionary<int, List<EPGChannelProgrammeObject>> dChannelEpgList = EpgBL.Utils.createDic(lChannelIDs);
            DateTime now = DateTime.UtcNow;
            int nTotalPrograms = Math.Abs(nPrevTop) + Math.Abs(nNextTop) + 1;
            int nTotalMinutes = 0;
            if (lChannelIDs != null && lChannelIDs.Count > 0)
            {
                int nChannelCount = lChannelIDs.Count;

                // save monitor and logs context data
                ContextData contextData = new ContextData();

                //Start MultiThread Call
                Task[] tasks = new Task[nChannelCount];
                for (int i = 0; i < nChannelCount; i++)
                {
                    int nChannel = lChannelIDs[i];

                    tasks[i] = Task.Factory.StartNew(
                         (obj) =>
                         {
                             // load monitor and logs context data
                             contextData.Load();

                             try
                             {
                                 int taskChannelID = (int)obj;
                                 if (dChannelEpgList.ContainsKey(taskChannelID))
                                 {
                                     List<EPGChannelProgrammeObject> lRes = GetEPGChannelPrograms(taskChannelID.ToString(), taskChannelID.ToString(), now, nTotalMinutes, nTotalPrograms);
                                     if (lRes != null && lRes.Count > 0)
                                         dChannelEpgList[taskChannelID].AddRange(lRes);
                                 }
                             }
                             catch (Exception ex)
                             {
                                 log.Error(string.Format("GetMultiChannelProgramsDic had an exception : ex={0} in {1}", ex.Message, ex.StackTrace), ex);
                             }
                         }, nChannel);
                }

                //Wait for all parallels tasks to finish:
                Task.WaitAll(tasks);
            }
            return dChannelEpgList;
        }
       

        public override bool InsertEpg(EpgCB newEpgItem, out ulong epgID, ulong? cas)
        {
            //TvinciEpgBL Bl = new TvinciEpgBL(this.m_nGroupID);
            //return Bl.InsertEpg(newEpgItem, out  epgID, cas);

            epgID = 0;
            return false;
        }
        public override bool InsertEpg(EpgCB newEpgItem, bool isMainLang, out string docID, ulong? cas = null)
        {
            docID = string.Empty;
            return false;
        }
        public override bool SetEpg(EpgCB newEpgItem, out ulong epgID, ulong? cas = null)
        {
            epgID = 0;
            return false;
        }

        public override bool UpdateEpg(EpgCB newEpgItem, ulong? cas)
        {
            return false;
        }
        public override bool UpdateEpg(EpgCB newEpgItem, bool isMainLang, out string docID, ulong? cas = null)
        {
            docID = string.Empty;
            return false;
        }
        public override void RemoveGroupPrograms(DateTime? fromDate, DateTime? toDate)
        {
        }

        public override void RemoveGroupPrograms(List<DateTime> lDates, int channelID)
        {
        }

        public override void RemoveGroupPrograms(List<int> lprogramIDs)
        {
        }

        public override void RemoveGroupPrograms(List<string> docIds)
        {
        }

        public override void RemoveGroupPrograms(DateTime? fromDate, DateTime? toDate, int channelID)
        {
        }
        public override EpgCB GetEpgCB(ulong nProgramID)
        {
            return null;
        }

        public override List<EpgCB> GetEpgCB(ulong nProgramID, List<string> languages)
        {
            return null;
        }
        public override EpgCB GetEpgCB(ulong nProgramID, out ulong cas)
        {
            cas = 0;
            return null;
        }

        //get all EPgs in the given range, including Epgs that are partially overlapping
        public override ConcurrentDictionary<int, List<EPGChannelProgrammeObject>> GetMultiChannelProgramsDic(int nPageSize, int nStartIndex, List<int> lChannelIDs, DateTime fromDate, DateTime toDate)
        {
            ConcurrentDictionary<int, List<EPGChannelProgrammeObject>> dChannelEpgList = EpgBL.Utils.createDic(lChannelIDs);

            if (lChannelIDs != null && lChannelIDs.Count > 0)
            {
                int nChannelCount = lChannelIDs.Count;

                // save monitor and logs context data
                ContextData contextData = new ContextData();

                //Start MultiThread Call
                Task[] tasks = new Task[nChannelCount];
                for (int i = 0; i < nChannelCount; i++)
                {
                    int nChannel = lChannelIDs[i];

                    tasks[i] = Task.Factory.StartNew(
                         (obj) =>
                         {
                             // load monitor and logs context data
                             contextData.Load();

                             try
                             {
                                 int taskChannelID = (int)obj;
                                 if (dChannelEpgList.ContainsKey(taskChannelID))
                                 {
                                     int nTotalMinutes = (int)(toDate - fromDate).TotalMinutes;
                                     if (nTotalMinutes > 0)
                                     {
                                         List<EPGChannelProgrammeObject> lRes = GetEPGChannelPrograms(taskChannelID.ToString(), taskChannelID.ToString(), fromDate, nTotalMinutes, 0);
                                         if (lRes != null && lRes.Count > 0)
                                             dChannelEpgList[taskChannelID].AddRange(lRes);
                                     }
                                 }
                             }
                             catch (Exception ex)
                             {
                                 log.Error(string.Format("GetMultiChannelProgramsDic had an exception : ex={0} in {1}", ex.Message, ex.StackTrace), ex);
                             }
                         }, nChannel);
                }

                //Wait for all parallels tasks to finish:
                Task.WaitAll(tasks);
            }
            return dChannelEpgList;
        }


        public override List<EPGChannelProgrammeObject> GetEPGPrograms(int groupID, string[] externalids, Language eLang, int duration)
        {

            return new List<EPGChannelProgrammeObject>();

        }
        #endregion

        #region Privat
        private List<EPGChannelProgrammeObject> GetEPGChannelPrograms(string sChannelID, string sEPGChannelID, DateTime dStartDay, int nTotalMinutes, int nTotalPrograms)
        {
            DateTime dNow = DateTime.UtcNow;

            List<EPGChannelProgrammeObject> res = new List<EPGChannelProgrammeObject>();

            string url = GetYesRestUrl(dStartDay, sEPGChannelID, nTotalMinutes, nTotalPrograms);

            DateTime dCisco = DateTime.UtcNow;
            string rest = TVinciShared.WS_Utils.SendXMLHttpReq(url, "", "");
            double dCiscoTime = DateTime.UtcNow.Subtract(dCisco).TotalMilliseconds;

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(rest);

            XmlNodeList events = doc.SelectNodes("/xml/schedules/evt");
            for (int i = 0; i < events.Count; i++)
            {
                XmlNode evt = events[i];
                string scid = TVinciShared.XmlUtils.GetNodeValue(ref evt, "scid");
                string seid = TVinciShared.XmlUtils.GetNodeValue(ref evt, "seid");
                string EPGIdentifier = TVinciShared.XmlUtils.GetNodeValue(ref evt, "cns/cn");
                if (!EPGIdentifier.Equals(sEPGChannelID))
                {
                    continue;
                }

                string name = TVinciShared.XmlUtils.GetNodeValue(ref evt, "ts");
                if (string.IsNullOrEmpty(name))
                {
                    name = TVinciShared.XmlUtils.GetNodeValue(ref evt, "ept");
                }

                string description = TVinciShared.XmlUtils.GetNodeValue(ref evt, "dss");
                string sdt = TVinciShared.XmlUtils.GetNodeValue(ref evt, "sdt");
                string edt = TVinciShared.XmlUtils.GetNodeValue(ref evt, "edt");

                string format = "yyyy-MM-ddTHH:mm:ss.fffZ";
                DateTime startDate = DateTime.ParseExact(sdt, format, null);
                DateTime endDate = DateTime.ParseExact(edt, format, null);

                string pic = TVinciShared.XmlUtils.GetNodeValue(ref evt, "is/i");
                string status = "1";
                string active = "1";
                string media = string.Empty;

                string createDate = string.Empty;
                string updateDate = string.Empty;
                string publishDate = string.Empty;

                List<EPGDictionary> metas = new List<EPGDictionary>();
                List<EPGDictionary> tags = new List<EPGDictionary>();

                XmlNodeList genres = evt.SelectNodes("gs/g");
                for (int j = 0; j < genres.Count; j++)
                {
                    XmlNode genre = genres[j];
                    string g = genre.InnerText;
                    if (!string.IsNullOrEmpty(g))
                    {
                        EPGDictionary ed = new EPGDictionary();
                        ed.Key = "Genre";
                        ed.Value = g;
                        tags.Add(ed);
                    }
                }

                string flag = TVinciShared.XmlUtils.GetNodeValue(ref evt, "flags");
                if (flag.Equals("fls"))
                {
                    EPGDictionary ed = new EPGDictionary();
                    ed.Key = "BlackOUT";
                    ed.Value = "True";
                    tags.Add(ed);
                }

                EPGChannelProgrammeObject prog = new EPGChannelProgrammeObject();
                long epgID = 0;
                bool tryParse = long.TryParse(scid, out epgID);
                prog.Initialize(epgID, sChannelID, EPGIdentifier, name, description, startDate.ToString(), endDate.ToString(), pic, status, active, m_nGroupID.ToString(),
                    string.Empty, createDate, publishDate, updateDate, tags, metas, media, 0);
                res.Add(prog);
            }

            double dTT = DateTime.UtcNow.Subtract(dNow).TotalMilliseconds;
            log.Debug("GetEPGChannelPrograms - " + string.Format("TT:{0}, ct:{1}, nt:{2}, URL:{3}", dTT, dCiscoTime, dTT - dCiscoTime, url));

            return res;
        }

        private string GetYesRestUrl(DateTime startDate, string sEPGChannelID, int nTotalMinutes, int nTotalPrograms)
        {
            string day = startDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string epgURL = TVinciShared.WS_Utils.GetTcmConfigValue("EPGUrl");
            StringBuilder url = new StringBuilder();
            url.AppendFormat(epgURL);
            url.AppendFormat("schedules?");
            url.AppendFormat("regionId=Israel");
            url.AppendFormat("&startTime={0}", day);
            url.AppendFormat("&filters={0}:equals:{1}", "cn", sEPGChannelID);
            url.Append("&locale=iw_IL");
            if (nTotalMinutes > 0)
            {
                url.AppendFormat("&duration={0}", nTotalMinutes.ToString());
            }
            if (nTotalPrograms > 0)
            {
                url.AppendFormat("&count={0}", nTotalPrograms.ToString());
            }

            return url.ToString();
        }

        private List<EPGChannelProgrammeObject> GetProgramsList(string searchValue, int pageIndex, int pageSize)
        {
            List<EPGChannelProgrammeObject> programs = new List<EPGChannelProgrammeObject>();

            string epgURL = TVinciShared.WS_Utils.GetTcmConfigValue("EPGUrl");
            //string epgURL = "http://lab-vms.tve.yeseng.co.il/opencase/sm/resource/rest/";
            StringBuilder url = new StringBuilder();
            url.AppendFormat(epgURL);
            url.AppendFormat("content?");
            url.AppendFormat("regionId=Israel");
            url.AppendFormat("&entityType={0}", "EPG");
            url.AppendFormat(GetLanguageParameter(searchValue));
            url.AppendFormat("&q={0}", searchValue);

            string rest = TVinciShared.WS_Utils.SendXMLHttpReq(url.ToString(), "", "");

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(rest);

            XmlNodeList events = doc.SelectNodes("/xml/contents/content");
            for (int i = 0; i < events.Count; i++)
            {
                XmlNode evt = events[i];
                string pid = TVinciShared.XmlUtils.GetNodeValue(ref evt, "pid");

                if (!string.IsNullOrEmpty(pid))
                {
                    EPGChannelProgrammeObject prog = GetProgramData(pid);
                    if (prog == null)
                    {
                        continue;
                    }

                    prog.NAME = TVinciShared.XmlUtils.GetNodeValue(ref evt, "ts");
                    prog.DESCRIPTION = TVinciShared.XmlUtils.GetNodeValue(ref evt, "dss");
                    prog.GROUP_ID = m_nGroupID.ToString();

                    List<EPGDictionary> tags = new List<EPGDictionary>();

                    XmlNodeList genres = evt.SelectNodes("gs/g");
                    for (int j = 0; j < genres.Count; j++)
                    {
                        XmlNode genre = genres[j];
                        string g = genre.InnerText;
                        if (!string.IsNullOrEmpty(g))
                        {
                            EPGDictionary ed = new EPGDictionary();
                            ed.Key = "Genre";
                            ed.Value = g;
                            tags.Add(ed);
                        }
                    }

                    prog.EPG_TAGS = tags;
                    programs.Add(prog);

                    if (programs.Count == pageSize)
                        break;
                }
            }

            return programs;
        }


        private string GetLanguageParameter(string sQuery)
        {
            string res = string.Empty;
            Language lang = GetLanguageOfString(sQuery);

            res = string.Format("&locale={0}", GetLanguageConvertor(lang));

            return res;
        }

        private string GetLanguageConvertor(Language eLang)
        {
            string sLang = "iw_il";
            switch (eLang)
            {
                case (Language.Hebrew):
                    {
                        sLang = "iw_il";
                        break;
                    }
                case (Language.Russian):
                    {
                        sLang = "ru_ru";
                        break;
                    }
                case (Language.Arabic):
                    {
                        sLang = "ar_sa";
                        break;
                    }
                case (Language.English):
                    {
                        sLang = "en_us";
                        break;
                    }
                default:
                    {
                        sLang = "iw_il";
                        break;
                    }
            }

            return sLang;

        }

        private Language GetLanguageOfString(string s)
        {
            if (HebrewRegex.IsMatch(s))
                return Language.Hebrew;
            if (RussianRegex.IsMatch(s))
                return Language.Russian;
            if (ArabicRegex.IsMatch(s))
                return Language.Arabic;
            return Language.English;
        }

        private EPGChannelProgrammeObject GetProgramData(string pid)
        {
            EPGChannelProgrammeObject program = new EPGChannelProgrammeObject();

            string date = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string epgURL = TVinciShared.WS_Utils.GetTcmConfigValue("EPGUrl");
            //string epgURL = "http://lab-vms.tve.yeseng.co.il/opencase/sm/resource/rest/";
            StringBuilder url = new StringBuilder();
            url.AppendFormat(epgURL);
            url.AppendFormat("programschedule?");
            url.AppendFormat("regionId=Israel");
            url.AppendFormat("&startTime={0}", date);
            url.AppendFormat("&programId={0}", pid);

            string rest = TVinciShared.WS_Utils.SendXMLHttpReq(url.ToString(), "", "");

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(rest);

            XmlNodeList prog = doc.SelectNodes("/xml/schedules/stn");
            if (prog.Count == 0)
                return program;

            for (int i = 0; i < prog.Count; i++)
            {
                XmlNode pr = prog[i];
                string cn = TVinciShared.XmlUtils.GetNodeValue(ref pr, "cns/cn");

                string sdt = TVinciShared.XmlUtils.GetNodeValue(ref pr, "evt/sdt");
                int duration = int.Parse(TVinciShared.XmlUtils.GetNodeValue(ref pr, "evt/d"));

                string format = "yyyy-MM-ddTHH:mm:ss.fffZ";
                DateTime startDate = DateTime.ParseExact(sdt, format, null);
                DateTime endDate = startDate.AddMinutes(duration);

                string epg_id = TVinciShared.XmlUtils.GetNodeValue(ref pr, "evt/scid");
                string epg_identifier = pid;
                long lEpg_id = 0;

                if (!string.IsNullOrEmpty(epg_id))
                {
                    lEpg_id = long.Parse(epg_id);
                }
                List<EPGDictionary> metas = new List<EPGDictionary>();
                string yesChannel = TVinciShared.XmlUtils.GetNodeValue(ref pr, "seid");

                EPGDictionary metaItem = new EPGDictionary();
                metaItem.Key = "YesChannel";
                metaItem.Value = yesChannel;
                metas.Add(metaItem);

                program.Initialize(lEpg_id, cn, epg_identifier, string.Empty, string.Empty, startDate.ToString(), endDate.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                    string.Empty, string.Empty, string.Empty, new List<EPGDictionary>(), metas, string.Empty, 0);
            }

            return program;
        }

        private EPGChannelProgrammeObject GetProgramData(string type, string val, Language eLang, int nDuration)
        {
            EPGChannelProgrammeObject program = new EPGChannelProgrammeObject();

            string date = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string epgURL = TVinciShared.WS_Utils.GetTcmConfigValue("EPGUrl");
            //string epgURL = "http://lab-vms.tve.yeseng.co.il/opencase/sm/resource/rest/";
            StringBuilder url = new StringBuilder();
            url.AppendFormat(epgURL);
            url.AppendFormat("schedules?");
            url.AppendFormat("regionId=Israel");
            url.AppendFormat("&startTime={0}", date);
            url.AppendFormat("&filters={0}:equals:{1}", type, val);
            url.AppendFormat("&locale={0}", GetLanguageConvertor(eLang));
            url.AppendFormat("&duration={0}", nDuration);

            string rest = TVinciShared.WS_Utils.SendXMLHttpReq(url.ToString(), "", "");

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(rest);

            XmlNode evt = doc.SelectSingleNode("/xml/schedules/evt");
            if (evt != null)
            {
                string cn = TVinciShared.XmlUtils.GetNodeValue(ref evt, "cns/cn");
                string seid = TVinciShared.XmlUtils.GetNodeValue(ref evt, "seid");

                string name = TVinciShared.XmlUtils.GetNodeValue(ref evt, "ts");
                if (string.IsNullOrEmpty(name))
                {
                    name = TVinciShared.XmlUtils.GetNodeValue(ref evt, "ept");
                }

                string description = TVinciShared.XmlUtils.GetNodeValue(ref evt, "dss");

                string sdt = TVinciShared.XmlUtils.GetNodeValue(ref evt, "sdt");
                string edt = TVinciShared.XmlUtils.GetNodeValue(ref evt, "edt");

                string format = "yyyy-MM-ddTHH:mm:ss.fffZ";
                DateTime startDate = DateTime.ParseExact(sdt, format, null);
                DateTime endDate = DateTime.ParseExact(edt, format, null);

                string epg_id = TVinciShared.XmlUtils.GetNodeValue(ref evt, "scid");
                string epg_identifier = TVinciShared.XmlUtils.GetNodeValue(ref evt, "pid");
                long lEpg_id = 0;

                if (!string.IsNullOrEmpty(epg_id))
                {
                    lEpg_id = long.Parse(epg_id);
                }

                string pic = TVinciShared.XmlUtils.GetNodeValue(ref evt, "is/i");
                string status = "1";
                string active = "1";
                string media = string.Empty;

                string createDate = string.Empty;
                string updateDate = string.Empty;
                string publishDate = string.Empty;

                List<EPGDictionary> metas = new List<EPGDictionary>();
                List<EPGDictionary> tags = new List<EPGDictionary>();

                string yesChannel = TVinciShared.XmlUtils.GetNodeValue(ref evt, "seid");

                EPGDictionary metaItem = new EPGDictionary();
                metaItem.Key = "YesChannel";
                metaItem.Value = TVinciShared.XmlUtils.GetNodeValue(ref evt, "seid");
                metas.Add(metaItem);

                XmlNodeList genres = evt.SelectNodes("gs/g");
                for (int j = 0; j < genres.Count; j++)
                {
                    XmlNode genre = genres[j];
                    string g = genre.InnerText;
                    if (!string.IsNullOrEmpty(g))
                    {
                        EPGDictionary ed = new EPGDictionary();
                        ed.Key = "Genre";
                        ed.Value = g;
                        tags.Add(ed);
                    }
                }

                string flag = TVinciShared.XmlUtils.GetNodeValue(ref evt, "flags");
                if (flag.Equals("fls"))
                {
                    EPGDictionary ed = new EPGDictionary();
                    ed.Key = "BlackOUT";
                    ed.Value = "True";
                    tags.Add(ed);
                }

                program.Initialize(lEpg_id, cn, epg_identifier, name, description, startDate.ToString(), endDate.ToString(), pic, status, active, m_nGroupID.ToString(),
                    string.Empty, createDate, publishDate, updateDate, tags, metas, media, 0);
            }

            return program;
        }

        /* private EPGChannelProgrammeObject GetProgramDataByScid(string scid, Language eLang, int nDuration)
         {
             EPGChannelProgrammeObject program = new EPGChannelProgrammeObject();

             string date = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
             string epgURL = ConfigurationManager.AppSettings["EPGUrl"];
             //string epgURL = "http://lab-vms.tve.yeseng.co.il/opencase/sm/resource/rest/";
             StringBuilder url = new StringBuilder();
             url.AppendFormat(epgURL);
             url.AppendFormat("schedules?");
             url.AppendFormat("regionId=Israel");
             url.AppendFormat("&startTime={0}", date);
             url.AppendFormat("&filters=scid:equals:{0}", scid);
             url.AppendFormat("&locale={0}", GetLanguageConvertor(eLang));
             url.AppendFormat("&duration={0}", nDuration);

             string rest = TVinciShared.WS_Utils.SendXMLHttpReq(url.ToString(), "", "");

             XmlDocument doc = new XmlDocument();
             doc.LoadXml(rest);

             XmlNode evt = doc.SelectSingleNode("/xml/schedules/evt");
             if (evt != null)
             {
                 string cn = TVinciShared.XmlUtils.GetNodeValue(ref evt, "cns/cn");
                 string seid = TVinciShared.XmlUtils.GetNodeValue(ref evt, "seid");

                 string name = TVinciShared.XmlUtils.GetNodeValue(ref evt, "ts");
                 if (string.IsNullOrEmpty(name))
                 {
                     name = TVinciShared.XmlUtils.GetNodeValue(ref evt, "ept");
                 }

                 string description = TVinciShared.XmlUtils.GetNodeValue(ref evt, "dss");

                 string sdt = TVinciShared.XmlUtils.GetNodeValue(ref evt, "sdt");
                 string edt = TVinciShared.XmlUtils.GetNodeValue(ref evt, "edt");

                 string format = "yyyy-MM-ddTHH:mm:ss.fffZ";
                 DateTime startDate = DateTime.ParseExact(sdt, format, null);
                 DateTime endDate = DateTime.ParseExact(edt, format, null);

                 string pic = TVinciShared.XmlUtils.GetNodeValue(ref evt, "is/i");
                 string status = "1";
                 string active = "1";
                 string media = string.Empty;

                 string createDate = string.Empty;
                 string updateDate = string.Empty;
                 string publishDate = string.Empty;

                 List<EPGDictionary> metas = new List<EPGDictionary>();
                 List<EPGDictionary> tags = new List<EPGDictionary>();

                 XmlNodeList genres = evt.SelectNodes("gs/g");
                 for (int j = 0; j < genres.Count; j++)
                 {
                     XmlNode genre = genres[j];
                     string g = genre.InnerText;
                     if (!string.IsNullOrEmpty(g))
                     {
                         EPGDictionary ed = new EPGDictionary();
                         ed.Key = "Genre";
                         ed.Value = g;
                         tags.Add(ed);
                     }
                 }

                 string flag = TVinciShared.XmlUtils.GetNodeValue(ref evt, "flags");
                 if (flag.Equals("fls"))
                 {
                     EPGDictionary ed = new EPGDictionary();
                     ed.Key = "BlackOUT";
                     ed.Value = "True";
                     tags.Add(ed);
                 }
                 long epgID = 0;
                 bool res =  long.TryParse(scid, out epgID);
                 program.Initialize(epgID, cn, cn, name, description, startDate.ToString(), endDate.ToString(), pic, status, active, m_nGroupID.ToString(),
                     string.Empty, createDate, publishDate, updateDate, tags, metas, media, 0);
             }

             return program;
         }*/
        #endregion

        public override List<EPGChannelProgrammeObject> GetEpgCBsWithLanguage(List<ulong> programIDs, string language)
        {
            return new List<EPGChannelProgrammeObject>();
        }

        public override List<EPGChannelProgrammeObject> GetEpgCBsWithLanguage(List<ulong> programIDs, List<LanguageObj> language)       
        {
            return new List<EPGChannelProgrammeObject>();
        }
    }
}
