using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using TvinciImporter;
using System.Web;

using DAL;
using EpgBL;
using ApiObjects;
using KLogMonitor;
using System.Reflection;

namespace EpgFeeder
{
    public class EPGGeneral : EPGImplementor
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        tv m_TvChannels;

        protected override string LogFileName
        {
            get
            {
                return "EPGGeneral";
            }
        }

        public EPGGeneral(string sGroupID)
            : base(sGroupID)
        {
        }

        public EPGGeneral(string sGroupID, string sPathType, string sPath, Dictionary<string, string> sExtraParamter)
            : base(sGroupID, sPathType, sPath, sExtraParamter)
        {
        }

        protected int GetProgramIDByEPGIdentifier(Guid EPGIdentfier)
        {
            int res = 0;
            ODBCWrapper.DataSetSelectQuery selectProgramQuery = new ODBCWrapper.DataSetSelectQuery();
            selectProgramQuery += "select ID from epg_channels_schedule where ";
            selectProgramQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_IDENTIFIER", "=", EPGIdentfier.ToString());
            if (selectProgramQuery.Execute("query", true) != null)
            {
                int count = selectProgramQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    res = ODBCWrapper.Utils.GetIntSafeVal(selectProgramQuery, "ID", 0);
                }
            }
            return res;
        }

        private string GetMetaDataStructure(tvProgramme item)
        {
            StringBuilder strMeta = new StringBuilder();
            try
            {
                if (item.audio != null)
                {
                    if (!string.IsNullOrEmpty(item.audio.present))
                    {
                        strMeta.Append(string.Format("{0};#{1},", "Audio", HttpUtility.UrlEncode(item.audio.present)));
                    }
                    if (!string.IsNullOrEmpty(item.audio.stereo))
                    {
                        strMeta.Append(string.Format("{0};#{1},", "Audio", HttpUtility.UrlEncode(item.audio.stereo)));
                    }
                }
                if (item.category != null)
                {
                    var category = from t in item.category
                                   select t.Value.ToString();
                    strMeta.Append(string.Format("{0};#{1},", "Category", HttpUtility.UrlEncode(string.Join("|", category.ToArray()))));
                }
                if (!string.IsNullOrEmpty(item.clumpidx))
                {
                    strMeta.Append(string.Format("{0};#{1},", "clumpidx", HttpUtility.UrlEncode(item.clumpidx)));
                }

                if (item.country != null)
                {
                    var country = from t in item.country
                                  select t.Value.ToString();
                    strMeta.Append(string.Format("{0};#{1},", "Credits_Country", HttpUtility.UrlEncode(string.Join("|", country.ToArray()))));
                }
                if (item.credits != null)
                {
                    if (item.credits.actor != null)
                    {
                        var actor = from t in item.credits.actor
                                    select t.Value.ToString();
                        strMeta.Append(string.Format("{0};#{1},", "Credits_Actor", HttpUtility.UrlEncode(string.Join("|", actor.ToArray()))));
                    }

                    if (item.credits.adapter != null)
                    {
                        var adapter = from t in item.credits.actor
                                      select t.Value.ToString();
                        strMeta.Append(string.Format("{0};#{1},", "Credits_Adapter", HttpUtility.UrlEncode(string.Join("|", adapter.ToArray()))));
                    }

                    if (item.credits.commentator != null)
                    {
                        var commentator = from t in item.credits.commentator
                                          select t.ToString();
                        strMeta.Append(string.Format("{0};#{1},", "Credits_Commentator", HttpUtility.UrlEncode(string.Join("|", commentator.ToArray()))));
                    }

                    if (item.credits.composer != null)
                    {
                        var composer = from t in item.credits.composer
                                       select t.ToString();
                        strMeta.Append(string.Format("{0};#{1},", "Credits_Composer", HttpUtility.UrlEncode(string.Join("|", composer.ToArray()))));
                    }
                    if (item.credits.date != null)
                    {
                        var date = from t in item.credits.date
                                   select t.ToString();
                        strMeta.Append(string.Format("{0};#{1},", "Credits_Date", HttpUtility.UrlEncode(string.Join("|", date.ToArray()))));
                    }
                    if (item.credits.director != null)
                    {
                        var director = from t in item.credits.director
                                       select t.ToString();
                        strMeta.Append(string.Format("{0};#{1},", "Credits_Director", HttpUtility.UrlEncode(string.Join("|", director.ToArray()))));
                    }
                    if (item.credits.editor != null)
                    {
                        var editor = from t in item.credits.director
                                     select t.ToString();
                        strMeta.Append(string.Format("{0};#{1},", "Credits_Editor", HttpUtility.UrlEncode(string.Join("|", editor.ToArray()))));
                    }
                    if (item.credits.guest != null)
                    {
                        var guest = from t in item.credits.director
                                    select t.ToString();
                        strMeta.Append(string.Format("{0};#{1},", "Credits_Guest", HttpUtility.UrlEncode(string.Join("|", guest.ToArray()))));
                    }
                    if (item.credits.presenter != null)
                    {
                        var presenter = from t in item.credits.director
                                        select t.ToString();
                        strMeta.Append(string.Format("{0};#{1},", "Credits_Presenter", HttpUtility.UrlEncode(string.Join("|", presenter.ToArray()))));
                    }
                    if (item.credits.producer != null)
                    {
                        var producer = from t in item.credits.producer
                                       select t.ToString();
                        strMeta.Append(string.Format("{0};#{1},", "Credits_Producer", HttpUtility.UrlEncode(string.Join("|", producer.ToArray()))));
                    }
                    if (item.credits.writer != null)
                    {
                        var writer = from t in item.credits.writer
                                     select t.ToString();
                        strMeta.Append(string.Format("{0};#{1},", "Credits_Writer", HttpUtility.UrlEncode(string.Join("|", writer.ToArray()))));
                    }
                }
                if (!string.IsNullOrEmpty(item.date))
                {
                    strMeta.Append(string.Format("{0};#{1},", "Date", HttpUtility.UrlEncode(item.date)));
                }
                if (item.episodenum != null)
                {
                    var episodenum = from t in item.episodenum
                                     select t.Value.ToString();
                    strMeta.Append(string.Format("{0};#{1},", "EpisodeNum", HttpUtility.UrlEncode(string.Join("|", episodenum.ToArray()))));
                }
                if (item.language != null)
                {
                    strMeta.Append(string.Format("{0};#{1},", "Language", HttpUtility.UrlEncode(item.language.Value)));
                }
                if (item.lastchance != null)
                {
                    strMeta.Append(string.Format("{0};#{1},", "LastChance", HttpUtility.UrlEncode(item.lastchance.Value)));
                }
                if (item.length != null)
                {
                    strMeta.Append(string.Format("{0};#{1},", "Length", HttpUtility.UrlEncode(item.length.Value)));
                }
                if (item.origlanguage != null)
                {
                    strMeta.Append(string.Format("{0};#{1},", "OrigLanguage", HttpUtility.UrlEncode(item.origlanguage.Value)));
                }
                if (!string.IsNullOrEmpty(item.pdcstart))
                {
                    strMeta.Append(string.Format("{0};#{1},", "pdcStart", HttpUtility.UrlEncode(item.pdcstart)));
                }
                if (item.premiere != null)
                {
                    strMeta.Append(string.Format("{0};#{1},", "Premiere", HttpUtility.UrlEncode(item.premiere.Value)));
                }
                if (item.rating != null)
                {
                    var rating = from t in item.rating
                                 select t.value.ToString();
                    strMeta.Append(string.Format("{0};#{1},", "Rating", HttpUtility.UrlEncode(string.Join("|", rating.ToArray()))));
                }
                if (item.review != null)
                {
                    var review = from t in item.review
                                 select t.Value.ToString();
                    strMeta.Append(string.Format("{0};#{1},", "Review", HttpUtility.UrlEncode(string.Join("|", review.ToArray()))));
                }
                if (!string.IsNullOrEmpty(item.showview))
                {
                    strMeta.Append(string.Format("{0};#{1},", "ShowView", HttpUtility.UrlEncode(item.showview)));
                }
                if (item.starrating != null)
                {
                    var starrating = from t in item.starrating
                                     select t.value.ToString();
                    strMeta.Append(string.Format("{0};#{1},", "StarRating", HttpUtility.UrlEncode(string.Join("|", starrating.ToArray()))));
                }
                if (item.subtitle != null)
                {
                    var subtitle = from t in item.subtitle
                                   select t.Value.ToString();
                    strMeta.Append(string.Format("{0};#{1},", "Subtitle", HttpUtility.UrlEncode(string.Join("|", subtitle.ToArray()))));
                }
                if (!string.IsNullOrEmpty(item.videoplus))
                {
                    strMeta.Append(string.Format("{0};#{1},", "videoplus", HttpUtility.UrlEncode(item.videoplus)));
                }
                if (!string.IsNullOrEmpty(item.vpsstart))
                {
                    strMeta.Append(string.Format("{0};#{1},", "vpsstart", HttpUtility.UrlEncode(item.vpsstart)));
                }
            }
            catch (Exception exp)
            { }
            return strMeta.ToString();
        }

        private bool ParseEPGStrToDate(string dateStr, ref DateTime theDate)
        {
            if (string.IsNullOrEmpty(dateStr) || dateStr.Length < 14)
                return false;

            string format = "yyyyMMddHHmmss";
            bool res = DateTime.TryParseExact(dateStr.Substring(0, 14), format, null, System.Globalization.DateTimeStyles.None, out theDate);
            return res;
        }

        private Int32 GetMediaIDByChannelID(Int32 EPG_IDENTIFIER)
        {
            Int32 res = 0;
            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select * from media";
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
            { }
            return res;
        }

        private Int32 GetExistChannel(string sChannelID)
        {
            Int32 res = 0;
            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select * from epg_channels";
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
            { }
            return res;
        }

        private Int32 GetExistMedia(Int32 EPG_IDENTIFIER)
        {
            Int32 res = 0;
            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select * from media";
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
            { }
            return res;
        }

        public override bool ResetChannelSchedule()
        {
            try
            {
                LoadXML();
                foreach (tvChannel item in m_TvChannels.channel)
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
            { }
            return true;
        }

        public override void GetChannel()
        {

        }

        public Dictionary<DateTime, List<int>> LoadXML()
        {
            //List<int> programIds = new List<int>();
            Dictionary<DateTime, List<int>> dateWithChannelIds = new Dictionary<DateTime, List<int>>(new DateComparer());
            try
            {
                switch (s_PathType)
                {
                    case "WebURL":

                        XmlSerializer ser = new XmlSerializer(typeof(tv));

                        XmlDocument xml = new XmlDocument();
                        xml.Load(s_Path);
                        StringReader xr = new StringReader(xml.InnerXml);
                        XmlTextReader reader = new XmlTextReader(xr);
                        m_TvChannels = (tv)ser.Deserialize(reader);
                        dateWithChannelIds = SaveTvChannels();
                        break;

                    case "FTP":
                        break;

                    case "Local":
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            { }

            return dateWithChannelIds;
        }


        private Dictionary<DateTime, List<int>> SaveTvChannels()
        {
            Dictionary<DateTime, List<int>> epgDateWithChannelIds = new Dictionary<DateTime, List<int>>(new DateComparer());
            List<FieldTypeEntity> FieldEntityMapping = GetMappingFields();
            EpgCB newEpgItem;


            string update_epg_package = TVinciShared.WS_Utils.GetTcmConfigValue("update_epg_package");
            int nCountPackage = ODBCWrapper.Utils.GetIntSafeVal(update_epg_package);
            int nCount = 0;
            List<ulong> ulProgram = new List<ulong>();

            #region get Group ID (parent Group if possible)
            int groupID = 0;
            if (!string.IsNullOrEmpty(m_ParentGroupId))
            {
                groupID = int.Parse(m_ParentGroupId);
            }
            else
            {
                groupID = DAL.UtilsDal.GetParentGroupID(int.Parse(s_GroupID));
            }
            #endregion

            BaseEpgBL oEpgBL = EpgBL.Utils.GetInstance(groupID);

            foreach (tvChannel item in m_TvChannels.channel)
            {
                Int32 channelID = GetExistChannel(item.id);

                #region Add or Update EPG Channel
                if (channelID == 0)
                {
                    //Insert New Channel
                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("epg_channels");

                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", item.displayname[0].Value);
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
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", item.displayname[0].Value);
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
                    //TBD: When media not exist insert it to db and handle updating of MEDIA_TYPE_ID,WATCH_PERMISSION_TYPE_ID columns to be not hardcoded.
                    //return;
                    return epgDateWithChannelIds;
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

                DeleteAllPrograms(channelID, prog);

                //List<FieldTypeEntity> lFieldTypeEntity = GetMappingFields();
                Dictionary<string, EpgCB> epgDic = new Dictionary<string, EpgCB>();

                foreach (var progItem in prog)
                {
                    //nCount++;
                    DateTime dProgStartDate = DateTime.MinValue;
                    DateTime dProgEndDate = DateTime.MinValue;
                    if (!ParseEPGStrToDate(progItem.start, ref dProgStartDate) || !ParseEPGStrToDate(progItem.stop, ref dProgEndDate))
                    {
                        log.Error("Program Dates Error - " + string.Format("start:{0}, end:{1}", progItem.start, progItem.stop));
                        continue;
                    }

                    SetMappingValues(FieldEntityMapping, progItem);

                    #region Generate EpgCB
                    newEpgItem = new EpgCB();
                    newEpgItem.ChannelID = ODBCWrapper.Utils.GetIntSafeVal(channelID);

                    if (progItem.title != null && progItem.title.Length > 0)
                    {
                        newEpgItem.Name = progItem.title[0].Value;
                    }

                    if (progItem.desc != null && progItem.desc.Length > 0)
                    {
                        var desc = from t in progItem.desc
                                   where !string.IsNullOrEmpty(t.Value)
                                   select t.Value.ToString();
                        newEpgItem.Description = string.Join(",", desc.ToArray());
                    }
                    else if (progItem.subtitle != null)
                    {
                        var subtitle = from t in progItem.subtitle
                                       select t.Value.ToString();
                        newEpgItem.Description = string.Join(",", subtitle.ToArray());
                    }

                    newEpgItem.GroupID = ODBCWrapper.Utils.GetIntSafeVal(s_GroupID);
                    newEpgItem.ParentGroupID = groupID;
                    Guid EPGGuid = Guid.NewGuid();
                    newEpgItem.EpgIdentifier = EPGGuid.ToString();
                    newEpgItem.StartDate = dProgStartDate;
                    newEpgItem.EndDate = dProgEndDate;
                    newEpgItem.UpdateDate = DateTime.UtcNow;
                    newEpgItem.CreateDate = DateTime.UtcNow;
                    newEpgItem.isActive = true;
                    newEpgItem.Status = 1;

                    newEpgItem.Metas = Utils.GetEpgProgramMetas(FieldEntityMapping);
                    // When We stop insert to DB , we still need to insert new tags to DB !!!!!!!
                    newEpgItem.Tags = Utils.GetEpgProgramTags(FieldEntityMapping);

                    #region Upload Picture

                    if (progItem.icon != null && progItem.icon.Length > 0)
                    {
                        string imgurl = progItem.icon[0].src;
                        if (!string.IsNullOrEmpty(imgurl))
                        {
                            int nPicID = ImporterImpl.DownloadEPGPic(imgurl, newEpgItem.Name, int.Parse(s_GroupID), 0, channelID);//the EPGs ID is not used in eh download function
                            #region Update EpgProgram with the PicID
                            if (nPicID != 0)
                            {
                                //Update CB
                                newEpgItem.PicID = nPicID;
                                newEpgItem.PicUrl = TVinciShared.CouchBaseManipulator.getEpgPicUrl(nPicID);
                            }
                            #endregion
                        }
                    }

                    #endregion

                    #endregion

                    epgDic.Add(newEpgItem.EpgIdentifier, newEpgItem);
                }

                //insert EPGs to DB in batches
                InsertEpgsDBBatches(ref epgDic, groupID, nCountPackage, FieldEntityMapping);

                foreach (EpgCB epg in epgDic.Values)
                {
                    nCount++;

                    #region Insert EpgProgram to CB
                    ulong epgID = 0;
                    bool bInsert = oEpgBL.InsertEpg(epg, out epgID);
                    #endregion

                    #region Insert EpgProgram ES

                    if (nCount >= nCountPackage)
                    {
                        ulProgram.Add(epg.EpgID);
                        int nGroupID = ODBCWrapper.Utils.GetIntSafeVal(s_GroupID);
                        bool resultEpgIndex = UpdateEpgIndex(ulProgram, nGroupID, ApiObjects.eAction.Update);

                        ulProgram = new List<ulong>();
                        nCount = 0;
                    }
                    else
                    {
                        ulProgram.Add(epg.EpgID);
                    }

                    #endregion

                    DateTime progDate = new DateTime(epg.StartDate.Year, epg.StartDate.Month, epg.StartDate.Day);

                    if (!epgDateWithChannelIds.ContainsKey(progDate))
                    {
                        List<int> channelIds = new List<int>();
                        epgDateWithChannelIds.Add(progDate, channelIds);
                    }

                    if (epgDateWithChannelIds[progDate].FindIndex(i => i == channelID) == -1)
                    {
                        epgDateWithChannelIds[progDate].Add(channelID);
                    }
                }

                if (nCount > 0 && ulProgram != null && ulProgram.Count > 0)
                {
                    int nGroupID = ODBCWrapper.Utils.GetIntSafeVal(s_GroupID);
                    bool resultEpgIndex = UpdateEpgIndex(ulProgram, nGroupID, ApiObjects.eAction.Update);
                }

                //start Upload proccess Queue
                UploadQueue.UploadQueueHelper.SetJobsForUpload(int.Parse(s_GroupID));
            }

            //return programIds;
            return epgDateWithChannelIds;
        }

        private void DeleteAllPrograms(Int32 channelID, IEnumerable<tvProgramme> prog)
        {
            Dictionary<DateTime, bool> deletedChannelDates = new Dictionary<DateTime, bool>();
            DateTime dProgStartDate = DateTime.MinValue;
            DateTime dProgEndDate = DateTime.MinValue;

            #region Delete all existing programs in DB that have start/end dates within the new schedule
            foreach (var progItem in prog)
            {
                dProgStartDate = DateTime.MinValue;
                dProgEndDate = DateTime.MinValue;

                if (!ParseEPGStrToDate(progItem.start, ref dProgStartDate) || !ParseEPGStrToDate(progItem.stop, ref dProgEndDate))
                {
                    continue;
                }

                if (dProgStartDate.Date.Equals(dProgEndDate.Date) && !deletedChannelDates.ContainsKey(dProgStartDate.Date))
                {
                    deletedChannelDates.Add(dProgStartDate.Date, true);
                }
            }

            foreach (DateTime progStartDate in deletedChannelDates.Keys)
            {
                log.Debug("Delete Program on Date - " + string.Format("Group ID = {0}; Deleting Programs on Date {1} that belong to channel {2}", s_GroupID, progStartDate, channelID));
                Tvinci.Core.DAL.EpgDal.DeleteProgramsOnDate(progStartDate, s_GroupID, channelID);
            }
            #endregion

            #region Delete all existing programs in CB that have start/end dates within the new schedule
            int nParentGroupID = int.Parse(m_ParentGroupId);
            BaseEpgBL oEpgBL = EpgBL.Utils.GetInstance(nParentGroupID);
            List<DateTime> lDates = new List<DateTime>();
            dProgStartDate = DateTime.MinValue;
            foreach (var progItem in prog)
            {
                Utils.ParseEPGStrToDate(progItem.start, ref dProgStartDate);
                if (!lDates.Contains(dProgStartDate.Date))
                {
                    lDates.Add(dProgStartDate.Date);
                }
            }

            log.Debug("Delete Program on Date - " + string.Format("Group ID = {0}; Deleting Programs  that belong to channel {1}", s_GroupID, channelID));

            oEpgBL.RemoveGroupPrograms(lDates, channelID);
            #endregion

            #region Delete all existing programs in ES that have start/end dates within the new schedule
            bool resDelete = Utils.DeleteEPGDocFromES(m_ParentGroupId, channelID, lDates);
            #endregion

        }


        public override Dictionary<DateTime, List<int>> ProcessConcreteXmlFile(XmlDocument xmlDoc)
        {
            ProcessOneXmlFile(xmlDoc);
            return SaveTvChannels();
        }

        private void ProcessOneXmlFile(XmlDocument xmlDoc)
        {
            XmlSerializer ser = new XmlSerializer(typeof(tv));
            StringReader xr = new StringReader(xmlDoc.InnerXml);
            XmlTextReader reader = new XmlTextReader(xr);
            m_TvChannels = (tv)ser.Deserialize(reader);
        }
    }

}


