using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Net;
using ApiObjects;
using System.Configuration;
using System.Web;
using KLogMonitor;
using System.Reflection;
using ConfigurationManager;

namespace EpgFeeder
{
    public class EPGYes : EPGImplementor
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected override string LogFileName
        {
            get
            {
                return "EPGYes";
            }
        }


        #region Private Members
        private Tvinci.EPG.Yes.tv m_TvChannels;

        #endregion

        public EPGYes(string sGroupID)
            : base(sGroupID)
        {

        }

        public EPGYes(string sGroupID, string sPathType, string sPath, Dictionary<string, string> sExtraParamter)
            : base(sGroupID, sPathType, sPath, sExtraParamter)
        {


        }
        #region Public Methods

        public override Dictionary<DateTime, List<int>> ProcessConcreteXmlFile(XmlDocument xmlDoc)
        {
            ProcessOneXmlFile(xmlDoc);
            return SaveTvChannels();
        }

        public override void GetChannel()
        {

        }
        public override bool ResetChannelSchedule()
        {
            try
            {
                LoadXML();
                foreach (Tvinci.EPG.Yes.tvChannel item in m_TvChannels.channel)
                {
                    Int32 channelID = GetExistChannel(item.id);
                    if (channelID != 0)
                    {
                        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("epg_channels");
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 0);
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 0);
                        updateQuery += " where ";
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", s_GroupID);
                        updateQuery += " and ";
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", channelID);
                        updateQuery += " and ";
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("CHANNEL_ID", "=", item.id);

                        updateQuery.Execute();
                        updateQuery.Finish();
                        updateQuery = null;
                    }
                }
            }
            catch (Exception exp)
            {
                log.Error("", exp);
                return false;
            }
            return true;
        }
        public void LoadXML()
        {
            try
            {
                switch (s_PathType)
                {
                    case "WebURL":

                        XmlSerializer ser = new XmlSerializer(typeof(Tvinci.EPG.Yes.tv));

                        XmlDocument xml = new XmlDocument();
                        xml.Load(s_Path);
                        StringReader xr = new StringReader(xml.InnerXml);
                        XmlTextReader reader = new XmlTextReader(xr);
                        m_TvChannels = (Tvinci.EPG.Yes.tv)ser.Deserialize(reader);
                        break;

                    case "FTP":
                        ProcessFilesFromFtpFolder();
                        break;

                    case "Local":
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                log.Error("", ex);
            }
        }

        public override List<EPGChannelProgrammeObject> GetEPGChannelProgramsByDates(Int32 groupID, string sChannelID, string sPicSize, DateTime fromDay, DateTime toDay, double nUTCOffset)
        {
            List<EPGChannelProgrammeObject> events = new List<EPGChannelProgrammeObject>();

            //string sChannelEPGIdentifier = GetChannelEPGIdentifier(sChannelID);

            DateTime fromUTCDay = fromDay.AddHours(nUTCOffset);
            DateTime toUTCDay = toDay.AddHours(nUTCOffset);
            int nTotalMinutes = (int)(toUTCDay - fromUTCDay).TotalMinutes;

            if (nTotalMinutes > 0)
            {
                events = GetEPGChannelPrograms(sChannelID, sChannelID, fromUTCDay, nTotalMinutes, 0);
            }

            return events;
        }

        public override List<EPGChannelProgrammeObject> GetEPGMultiChannelPrograms(Int32 groupID, string[] sEPGChannelIDs, string sPicSize, EPGUnit unit, int nFromOffsetUnit, int nToOffsetUnit, int nUTCOffset)
        {
            List<EPGChannelProgrammeObject> res = new List<EPGChannelProgrammeObject>();

            foreach (string channel in sEPGChannelIDs)
            {
                res.AddRange(GetEPGChannelPrograms(groupID, channel, sPicSize, unit, nFromOffsetUnit, nToOffsetUnit, nUTCOffset));
            }

            return res;
        }

        public override List<EPGChannelProgrammeObject> GetEPGChannelPrograms(Int32 groupID, string sEPGChannelID, string sPicSize, EPGUnit unit, int nFromOffsetUnit, int nToOffsetUnit, int nUTCOffset)
        {
            List<EPGChannelProgrammeObject> res = new List<EPGChannelProgrammeObject>();

            DateTime startDate = DateTime.UtcNow;  //.AddHours(nUTCOffset);
            int nTotalPrograms = 0;
            int nTotalMinutes = 0;

            //Get data include last offset unit.
            nToOffsetUnit++;

            switch (unit)
            {
                case EPGUnit.Days:

                    startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, 00, 00, 00);
                    startDate = startDate.AddDays(nFromOffsetUnit);

                    nTotalMinutes = (nToOffsetUnit - nFromOffsetUnit) * 60 * 24;
                    res = GetEPGChannelPrograms(sEPGChannelID, sEPGChannelID, startDate, nTotalMinutes, nTotalPrograms);

                    break;
                case EPGUnit.Hours:

                    startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, startDate.Hour, 00, 00);
                    startDate = startDate.AddHours(nFromOffsetUnit);

                    nTotalMinutes = (nToOffsetUnit - nFromOffsetUnit) * 60;
                    res = GetEPGChannelPrograms(sEPGChannelID, sEPGChannelID, startDate, nTotalMinutes, nTotalPrograms);

                    break;

                case EPGUnit.Current:

                    nTotalPrograms = nToOffsetUnit - nFromOffsetUnit;
                    res = GetEPGChannelPrograms(sEPGChannelID, sEPGChannelID, startDate, nTotalMinutes, nTotalPrograms);

                    break;
            }

            return res;
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
                EPGChannelProgrammeObject res = GetProgramDataByScid(scid, eLang, duration);
                if (res != null && !string.IsNullOrEmpty(res.EPG_CHANNEL_ID))
                {
                    progs.Add(res);
                }
            }

            return progs;
        }

        #endregion

        #region Private Methods
        private Int32 GetExistChannel(string sChannelID)
        {
            Int32 res = 0;
            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select id from epg_channels";
                selectQuery += "Where";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("Group_ID", "=", s_GroupID);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CHANNEL_ID", "=", sChannelID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        res = Int32.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception exp)
            {
                log.Error("", exp);
            }
            return res;
        }
        private DateTime ParseEPGStrToDate(string dateStr)
        {
            DateTime dt = new DateTime();
            int year = int.Parse(dateStr.Substring(0, 4));
            int month = int.Parse(dateStr.Substring(4, 2));
            int day = int.Parse(dateStr.Substring(6, 2));
            int hour = int.Parse(dateStr.Substring(8, 2));
            int min = int.Parse(dateStr.Substring(10, 2));
            int sec = int.Parse(dateStr.Substring(12, 2));
            dt = new DateTime(year, month, day, hour, min, sec);
            return dt;
        }
        private Int32 GetExistMedia(Int32 EPG_IDENTIFIER)
        {
            Int32 res = 0;
            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select id from media";
                selectQuery += "Where";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("Group_ID", "=", s_GroupID);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_IDENTIFIER", "=", EPG_IDENTIFIER);

                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        res = Int32.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception exp)
            {
                log.Error("", exp);
            }
            return res;
        }
        private Int32 GetMediaIDByChannelID(Int32 EPG_IDENTIFIER)
        {
            Int32 res = 0;
            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select id from media";
                selectQuery += "Where";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("Group_ID", "=", s_GroupID);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_IDENTIFIER", "=", EPG_IDENTIFIER);

                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        res = Int32.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception exp)
            {
                log.Error("", exp);
            }
            return res;
        }
        private void ProcessOneXmlFile(XmlDocument xmlDoc)
        {
            log.Debug("Start EPGYes.ProcessOneXmlFile() Deserialize xmlDoc");
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(Tvinci.EPG.Yes.tv));
                StringReader xr = new StringReader(xmlDoc.InnerXml);
                XmlTextReader reader = new XmlTextReader(xr);
                m_TvChannels = (Tvinci.EPG.Yes.tv)ser.Deserialize(reader);
            }
            catch (Exception ex)
            {
                log.Error("EPGYes.ProcessOneXmlFile() Exception on Deserialize xml:" + ex.Message, ex);
            }
            log.Debug("End EPGYes.ProcessOneXmlFile() Deserialize xmlDoc");
        }
        private Dictionary<DateTime, List<int>> SaveTvChannels()
        {
            Dictionary<DateTime, List<int>> dateWithChannelIds = new Dictionary<DateTime, List<int>>(new DateComparer());
            log.Debug("Start EPGYes.SaveTvChannels() Save tv channels to db");

            try
            {
                foreach (Tvinci.EPG.Yes.tvChannel item in m_TvChannels.channel)
                {
                    Int32 channelID = GetExistChannel(item.id);

                    #region Add or Update EPG Channel
                    if (channelID == 0)
                    {
                        //Insert New Channel
                        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("epg_channels");

                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", item.displayname);
                        //insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", item.id);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", s_GroupID);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CHANNEL_ID", "=", item.id);
                        insertQuery.Execute();
                        insertQuery.Finish();
                        insertQuery = null;
                        channelID = GetExistChannel(item.id);
                    }
                    else
                    {
                        //Update  Exist Channel
                        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("epg_channels");
                        //updateQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", item.displayname);
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", s_GroupID);
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("CHANNEL_ID", "=", item.id);
                        updateQuery += " where ";
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", channelID);
                        updateQuery.Execute();
                        updateQuery.Finish();
                        updateQuery = null;


                    }
                    #endregion

                    #region Add or Update Channel Media ID
                    DateTime dStartDate = DateTime.Now;
                    DateTime dCatalogEndDate = new DateTime(2099, 1, 1);

                    Int32 nMediaID = GetExistMedia(channelID);

                    if (nMediaID == 0)
                    {
                        ODBCWrapper.InsertQuery insertMediaQuery = new ODBCWrapper.InsertQuery("media");
                        insertMediaQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", item.displayname);
                        insertMediaQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", s_GroupID);
                        insertMediaQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_IDENTIFIER", "=", channelID);
                        insertMediaQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TYPE_ID", "=", "357"); // Media Type Live
                        insertMediaQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "=", dStartDate);
                        insertMediaQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", dCatalogEndDate);
                        insertMediaQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                        insertMediaQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                        insertMediaQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCH_PERMISSION_TYPE_ID", "=", 143);
                        insertMediaQuery.Execute();
                        insertMediaQuery.Finish();
                        insertMediaQuery = null;
                        nMediaID = GetMediaIDByChannelID(channelID);
                    }
                    else
                    {
                        ODBCWrapper.UpdateQuery updateMediaQuery = new ODBCWrapper.UpdateQuery("media");
                        //updateMediaQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", item.displayname);
                        updateMediaQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", s_GroupID);
                        updateMediaQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_IDENTIFIER", "=", channelID);
                        updateMediaQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TYPE_ID", "=", "357");
                        //updateMediaQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "=", dStartDate);
                        //updateMediaQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", dCatalogEndDate);
                        updateMediaQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                        updateMediaQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                        updateMediaQuery += ODBCWrapper.Parameter.NEW_PARAM("WATCH_PERMISSION_TYPE_ID", "=", 143);
                        updateMediaQuery += " where ";
                        updateMediaQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nMediaID);
                        updateMediaQuery.Execute();
                        updateMediaQuery.Finish();
                        updateMediaQuery = null;
                    }


                    #endregion

                    #region Update EPG Channel with Media ID
                    if (nMediaID != 0)
                    {
                        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("epg_channels");
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("Media_ID", "=", nMediaID);
                        updateQuery += " where ";
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", channelID);
                        updateQuery.Execute();
                        updateQuery.Finish();
                        updateQuery = null;
                    }

                    #endregion

                    var prog = from p in m_TvChannels.programme
                               where p.channel == item.id
                               select p;


                    for (int i = 0; i < prog.Count(); i++)
                    {
                        var progItem = prog.ElementAt(i);

                        DateTime dProgStartDate = ParseEPGStrToDate(progItem.start);
                        DateTime dProgEndDate = ParseEPGStrToDate(progItem.start);
                        dProgEndDate = dProgEndDate.AddHours(1);
                        if (i < prog.Count() - 1)
                        {
                            var temp = prog.ElementAt(i + 1);
                            dProgEndDate = ParseEPGStrToDate(temp.start);//?????
                        }
                        ODBCWrapper.InsertQuery insertProgQuery = new ODBCWrapper.InsertQuery("epg_channels_schedule");

                        insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", progItem.title.Value.ToString());
                        insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", s_GroupID);
                        insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_CHANNEL_ID", "=", channelID);
                        Guid EPGGuid = Guid.NewGuid();
                        insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_IDENTIFIER", "=", EPGGuid.ToString());
                        insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("PIC_ID", "=", 0);
                        insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "=", dProgStartDate);
                        insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", dProgEndDate);
                        insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                        insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                        insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 500);

                        insertProgQuery.Execute();
                        insertProgQuery.Finish();
                        insertProgQuery = null;
                    }

                }
            }
            catch (Exception ex)
            {
                log.Error("EPGYes.SaveTvChannels() Exception on saving channels to db:" + ex.Message, ex);
            }
            log.Debug("End EPGYes.SaveTvChannels() Save tv channels to db");

            return dateWithChannelIds;
        }

        private string GetChannelEPGIdentifier(string sChannelID)
        {
            string res = string.Empty;
            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select EPG_IDENTIFIER from media where status<>2 and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", sChannelID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        res = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "EPG_IDENTIFIER", 0);
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception exp)
            {
                log.Error("", exp);
            }
            return res;
        }

        private string GetYesRestUrl(DateTime startDate, string sEPGChannelID, int nTotalMinutes, int nTotalPrograms)
        {
            string day = startDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string epgURL = ApplicationConfiguration.EPGUrl.Value;
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
                prog.Initialize(0, sChannelID, EPGIdentifier, name, description, startDate.ToString(), endDate.ToString(), pic, status, active, s_GroupID,
                    string.Empty, createDate, publishDate, updateDate, tags, metas, media, 0);
                res.Add(prog);
            }

            double dTT = DateTime.UtcNow.Subtract(dNow).TotalMilliseconds;
            log.Debug("GetEPGChannelPrograms - " + string.Format("TT:{0}, ct:{1}, nt:{2}, URL:{3}", dTT, dCiscoTime, dTT - dCiscoTime, url));

            return res;
        }

        private List<EPGChannelProgrammeObject> GetProgramsList(string searchValue, int pageIndex, int pageSize)
        {
            List<EPGChannelProgrammeObject> programs = new List<EPGChannelProgrammeObject>();

            string epgURL = ApplicationConfiguration.EPGUrl.Value; //ConfigurationManager.AppSettings["EPGUrl"];
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
                    prog.GROUP_ID = s_GroupID;

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

        private EPGChannelProgrammeObject GetProgramData(string pid)
        {
            EPGChannelProgrammeObject program = new EPGChannelProgrammeObject();

            string date = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string epgURL = ApplicationConfiguration.EPGUrl.Value;
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
                return null;

            for (int i = 0; i < prog.Count; i++)
            {
                XmlNode pr = prog[i];
                string cn = TVinciShared.XmlUtils.GetNodeValue(ref pr, "cns/cn");

                string sdt = TVinciShared.XmlUtils.GetNodeValue(ref pr, "evt/sdt");
                int duration = int.Parse(TVinciShared.XmlUtils.GetNodeValue(ref pr, "evt/d"));

                string format = "yyyy-MM-ddTHH:mm:ss.fffZ";
                DateTime startDate = DateTime.ParseExact(sdt, format, null);
                DateTime endDate = startDate.AddMinutes(duration);

                program.Initialize(0, cn, cn, string.Empty, string.Empty, startDate.ToString(), endDate.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                    string.Empty, string.Empty, string.Empty, new List<EPGDictionary>(), new List<EPGDictionary>(), string.Empty, 0);
            }

            return program;
        }

        private EPGChannelProgrammeObject GetProgramDataByScid(string scid, Language eLang, int nDuration)
        {
            EPGChannelProgrammeObject program = new EPGChannelProgrammeObject();

            string date = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string epgURL = ApplicationConfiguration.EPGUrl.Value;
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

                program.Initialize(0, cn, cn, name, description, startDate.ToString(), endDate.ToString(), pic, status, active, s_GroupID,
                    string.Empty, createDate, publishDate, updateDate, tags, metas, media, 0);
            }

            return program;
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
        #endregion
    }
}
