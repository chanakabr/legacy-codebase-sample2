using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using ApiObjects;
using EpgBL;
using TvinciImporter;

namespace EpgFeeder
{
    public class EPGEutelsat : EPGImplementor
    {
        #region Member
        kabel.tv m_TvChannels;

        #endregion

        public EPGEutelsat(string sGroupID, string sPathType, string sPath, Dictionary<string, string> sExtraParamter)
            : base(sGroupID, sPathType, sPath, sExtraParamter)
        {
        }

        public override void GetChannel()
        {
        }

        public override Dictionary<DateTime, List<int>> ProcessConcreteXmlFile(XmlDocument xmlDoc)
        {
            ProcessOneXmlFile(xmlDoc);
            return SaveTvChannels();
        }

        private void ProcessOneXmlFile(XmlDocument xmlDoc)
        {
            XmlSerializer ser = new XmlSerializer(typeof(kabel.tv));
            StringReader xr = new StringReader(xmlDoc.InnerXml);
            XmlTextReader reader = new XmlTextReader(xr);
            m_TvChannels = (kabel.tv)ser.Deserialize(reader);
        }

        public override bool ResetChannelSchedule()
        {
            try
            {
                LoadXML();
                kabel.tvChannel item = m_TvChannels.channel;
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
            catch (Exception exp)
            { }
            return true;
        }

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
            { }
            return res;
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
                        m_TvChannels = (kabel.tv)ser.Deserialize(reader);
                        dateWithChannelIds = SaveTvChannels();
                        //programIds = SaveTvChannels();
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

            //return programIds;
            return dateWithChannelIds;
        }

        private Dictionary<DateTime, List<int>> SaveTvChannels()
        {
            Dictionary<DateTime, List<int>> epgDateWithChannelIds = new Dictionary<DateTime, List<int>>(new DateComparer());
            List<int> programIds = new List<int>();
            List<FieldTypeEntity> FieldEntityMapping = GetMappingFields();

            kabel.tvChannel item = m_TvChannels.channel;

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
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", item.displayname);
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
                //return programIds;
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

            EpgCB newEpgItem;
            int groupID = 0;
            if (!string.IsNullOrEmpty(m_ParentGroupId))
            {
                groupID = int.Parse(m_ParentGroupId);
            }
            else
            {
                groupID = DAL.UtilsDal.GetParentGroupID(int.Parse(s_GroupID));
            }

            BaseEpgBL oEpgBL = EpgBL.Utils.GetInstance(groupID);

            string update_epg_package = TVinciShared.WS_Utils.GetTcmConfigValue("update_epg_package");
            int nCountPackage = ODBCWrapper.Utils.GetIntSafeVal(update_epg_package);
            int nCount = 0;
            List<ulong> ulProgram = new List<ulong>();			
            foreach (var progItem in prog)
            {
                nCount++;
                DateTime dProgStartDate = DateTime.MinValue;
                DateTime dProgEndDate = DateTime.MinValue;
                if (!Utils.ParseEPGStrToDate(progItem.start, ref dProgStartDate) || !Utils.ParseEPGStrToDate(progItem.stop, ref dProgEndDate))
                {
                    Logger.Logger.Log("Program Dates Error", string.Format("start:{0}, end:{1}", progItem.start, progItem.stop), "EPG");
                    continue;
                }

                //need this for both DB and CB!!!!!!!!!
                Kabel_SetMappingValues(FieldEntityMapping, progItem);

                #region Insert EpgProgram To DB
                ODBCWrapper.InsertQuery insertProgQuery = new ODBCWrapper.InsertQuery("epg_channels_schedule");
                string progTitle = "";
                if (progItem.title != null && progItem.title.Length > 0)
                {
                    progTitle = progItem.title;
                    insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", progItem.title);
                }
                if (progItem.desc != null && progItem.desc.Length > 0)
                {
                    if (!string.IsNullOrEmpty(progItem.desc))
                    {
                        insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", progItem.desc);
                    }
                }
                else if (progItem.subtitle != null)
                {
                    if (!string.IsNullOrEmpty(progItem.subtitle))
                    {
                        insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", progItem.subtitle);
                    }
                }

                insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", s_GroupID);
                insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_CHANNEL_ID", "=", channelID);
                Guid EPGGuid = Guid.NewGuid();
                insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_IDENTIFIER", "=", EPGGuid.ToString());
                insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("PIC_ID", "=", 0);
                insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "=", dProgStartDate);
                insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", dProgEndDate);
                insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 400);

                insertProgQuery.Execute();
                insertProgQuery.Finish();
                insertProgQuery = null;

                int ProgramID = 0;
                ProgramID = GetProgramIDByEPGIdentifier(EPGGuid);

                if (ProgramID > 0)
                {
                    DateTime progDate = new DateTime(dProgStartDate.Year, dProgStartDate.Month, dProgStartDate.Day);

                    if (!epgDateWithChannelIds.ContainsKey(progDate))
                    {
                        List<int> channelIds = new List<int>();
                        epgDateWithChannelIds.Add(progDate, channelIds);
                    }

                    if (epgDateWithChannelIds[progDate].FindIndex(i => i == channelID) == -1)
                    {
                        epgDateWithChannelIds[progDate].Add(channelID);
                    }

                    programIds.Add(ProgramID);

                    InserEPGMetas(FieldEntityMapping, ProgramID);
                    InsertEPGTags(FieldEntityMapping, ProgramID);                  
                }
                #endregion

                #region Insert EpgProgram Doc To CB
                newEpgItem = new EpgCB();
                ulong uProgramID = (ulong)ProgramID;
                newEpgItem.EpgID = uProgramID;
                newEpgItem.ChannelID = ODBCWrapper.Utils.GetIntSafeVal(channelID);
                newEpgItem.Name = progItem.title;
                if (!string.IsNullOrEmpty(progItem.desc))
                {
                    newEpgItem.Description = progItem.desc;
                }
                else if (!string.IsNullOrEmpty(progItem.subtitle))
                {
                    newEpgItem.Description = progItem.subtitle;
                }
                newEpgItem.GroupID = ODBCWrapper.Utils.GetIntSafeVal(s_GroupID);               
                newEpgItem.ParentGroupID = groupID;                
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

                newEpgItem.ExtraData = new EpgExtraData();
                // not include in the DATA we get 
                //newEpgItem.ExtraData.MediaID = 0;
                //newEpgItem.ExtraData.FBObjectID = ""; 
                ulong epgID = 0;                              
                bool bInsert = oEpgBL.InsertEpg(newEpgItem, out epgID);
                #endregion 
                
                #region Insert EpgProgram ES

                if (nCount >= nCountPackage)
                {
                    int nGroupID = ODBCWrapper.Utils.GetIntSafeVal(s_GroupID);
                    bool resultEpgIndex = UpdateEpgIndex(ulProgram, nGroupID, ApiObjects.eAction.Update);

                    ulProgram = new List<ulong>();
                    nCount = 0;
                }
                else
                {
                    ulProgram.Add(uProgramID);
                }

                #endregion

                #region Upload Picture

                if (progItem.icon != null)
                {
                    string imgurl = progItem.icon.src;

                    if (!string.IsNullOrEmpty(imgurl))
                    {
                        int nPicID = ImporterImpl.DownloadEPGPic(imgurl, progTitle, int.Parse(s_GroupID), ProgramID, channelID);
                        #region Update EpgProgram with the PicID
                        if (nPicID != 0)
                        {
                            //Update DB
                            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("epg_channels_schedule");
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PIC_ID", "=", nPicID);
                            updateQuery += " where ";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", ProgramID);
                            updateQuery.Execute();
                            updateQuery.Finish();
                            updateQuery = null;

                            //Update CB
                            newEpgItem.PicUrl = nPicID.ToString();
                            newEpgItem.EpgID = epgID;                           
                            oEpgBL.UpdateEpg(newEpgItem);
                        }
                        #endregion
                    }
                }

                #endregion
            }

            //start Upload proccess Queue
            UploadQueue.UploadQueueHelper.SetJobsForUpload(int.Parse(s_GroupID));
            //return programIds;
            return epgDateWithChannelIds;
        }


        
        private void DeleteAllPrograms(Int32 channelID, IEnumerable<kabel.tvProgramme> prog)
        {
            Dictionary<DateTime, bool> deletedChannelDates = new Dictionary<DateTime, bool>();
            DateTime dProgStartDate = DateTime.MinValue;
            DateTime dProgEndDate = DateTime.MinValue;

            #region Delete all existing programs in DB that have start/end dates within the new schedule
            foreach (var progItem in prog)
            {
                dProgStartDate = DateTime.MinValue;
                dProgEndDate = DateTime.MinValue;

                if (!Utils.ParseEPGStrToDate(progItem.start, ref dProgStartDate) || !Utils.ParseEPGStrToDate(progItem.stop, ref dProgEndDate))
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
                Logger.Logger.Log("Delete Program on Date", string.Format("Group ID = {0}; Deleting Programs on Date {1} that belong to channel {2}", s_GroupID, progStartDate, channelID), "EpgFeeder");
                Tvinci.Core.DAL.EpgDal.DeleteProgramsOnDate(progStartDate, s_GroupID, channelID);
            }
            #endregion

            #region Delete all existing programs in CB that have start/end dates within the new schedule
            int nParentGroupID = 0;
            if (!string.IsNullOrEmpty(m_ParentGroupId))
            {
                nParentGroupID = int.Parse(m_ParentGroupId);
            }
            else
            {
                nParentGroupID = DAL.UtilsDal.GetParentGroupID(int.Parse(s_GroupID));
            }
            BaseEpgBL oEpgBL = EpgBL.Utils.GetInstance(nParentGroupID);      

            List<DateTime> lDates = new List<DateTime>();
            dProgStartDate = DateTime.MinValue;
            foreach (var progItem in prog)
            {
                Utils.ParseEPGStrToDate(progItem.start, ref dProgStartDate);
                if (!lDates.Contains(dProgStartDate))
                {
                    lDates.Add(dProgStartDate);
                }
            }

            Logger.Logger.Log("Delete Program on Date", string.Format("Group ID = {0}; Deleting Programs  that belong to channel {1}", s_GroupID, channelID), "EpgFeeder");

            oEpgBL.RemoveGroupPrograms(lDates, channelID);
            #endregion
        }
                  
        protected void Kabel_SetMappingValues(List<FieldTypeEntity> FieldEntityMapping, kabel.tvProgramme node)
        {
            if (node == null)
                return;

            for (int i = 0; i < FieldEntityMapping.Count; i++)
            {
                FieldEntityMapping[i].Value = new List<string>();

                foreach (string fieldName in FieldEntityMapping[i].XmlReffName)
                {
                    switch (fieldName.ToLower())
                    {
                        case "episode name":
                            {
                                if (node.subtitle != null && node.subtitle.Length > 0)
                                {
                                    FieldEntityMapping[i].Value.Add(node.subtitle);
                                }                              
                                break;
                            }
                        case "episode num":
                            {
                                if (node.episodenum != null && node.episodenum.Value != null)
                                {
                                    string[] wordsSeEp = node.episodenum.Value.Split('.');
                                    string[] numOfNum = wordsSeEp[1].Split('/');
                                    string finelValue = numOfNum[0] == "" ? "" : (int.Parse(numOfNum[0]) + 1).ToString();
                                    FieldEntityMapping[i].Value.Add(finelValue);
                                }
                                break;
                            }
                        case "season num":
                            {
                                if (node.episodenum != null && node.episodenum.Value != null)
                                {
                                    string[] wordsSeEp = node.episodenum.Value.Split('.');
                                    string[] numOfNum = wordsSeEp[0].Split('/');
                                    string finelValue = numOfNum[0] == "" ? "" : (int.Parse(numOfNum[0]) + 1).ToString();
                                    FieldEntityMapping[i].Value.Add(finelValue);
                                }
                                break;
                            }
                        case "description":
                            {
                                if (node.desc != null)
                                {
                                    FieldEntityMapping[i].Value.Add(node.desc);
                                }
                                break;
                            }
                        case "country":
                            {
                                if (node.country != null)
                                {
                                    foreach (string tpc in node.country)
                                    {
                                        FieldEntityMapping[i].Value.Add(tpc);
                                    }
                                }
                                break;
                            }
                        case "language":
                            {
                                if (node.language != null)
                                {
                                    FieldEntityMapping[i].Value.Add(node.language);
                                }
                                break;
                            }
                        case "rating":
                            {
                                if (node.rating != null && node.rating.Length > 0)
                                {
                                    foreach (kabel.tvProgrammeRating tpr in node.rating)
                                    {
                                        if (tpr.system != null && tpr.value != null)
                                        {
                                            FieldEntityMapping[i].Value.Add(tpr.system + tpr.value);
                                        }
                                    }
                                }
                                break;
                            }
                        case "aspect":
                            {
                                if (node.video != null && node.video.aspect != null)
                                {
                                    FieldEntityMapping[i].Value.Add(node.video.aspect);
                                }
                                break;
                            }
                        case "quality":
                            {
                                if (node.video != null && node.video.quality != null)
                                {
                                    FieldEntityMapping[i].Value.Add(node.video.quality);
                                }
                                break;
                            }
                        case "director":
                            {
                                if (node.credits != null && node.credits.director != null)
                                {
                                    foreach (string str in node.credits.director)
                                    {
                                        FieldEntityMapping[i].Value.Add(str);
                                    }
                                }
                                break;
                            }
                        case "actor":
                            {
                                if (node.credits != null && node.credits.actor != null)
                                {
                                    foreach (string str in node.credits.actor)
                                    {
                                        FieldEntityMapping[i].Value.Add(str);
                                    }
                                }
                                break;
                            }
                        case "presenter":
                            {
                                if (node.credits != null && node.credits.presenter != null)
                                {
                                    foreach (string str in node.credits.presenter)
                                    {
                                        FieldEntityMapping[i].Value.Add(str);
                                    }
                                }
                                break;
                            }
                        case "producer":
                            {
                                if (node.credits != null && node.credits.producer != null)
                                {
                                    foreach (string str in node.credits.producer)
                                    {
                                        FieldEntityMapping[i].Value.Add(str);
                                    }
                                }
                                break;
                            }
                        case "writer":
                            {
                                if (node.credits != null && node.credits.writer != null)
                                {
                                    foreach (string str in node.credits.writer)
                                    {
                                        FieldEntityMapping[i].Value.Add(str);
                                    }
                                }
                                break;
                            }
                        case "year":
                            {
                                if (node.date != null)
                                {
                                    FieldEntityMapping[i].Value.Add(node.date);
                                }
                                break;
                            }
                        case "duration":
                            {
                                if (node.length != null && node.length.Value != null)
                                {
                                    FieldEntityMapping[i].Value.Add(node.length.Value);
                                }
                                break;
                            }
                        case "catchup":
                            {
                                if (node.CatchUp != null)
                                {
                                    FieldEntityMapping[i].Value.Add(node.CatchUp);
                                }
                                break;
                            }
                        case "startover":
                            {
                                if (node.StartOver != null)
                                {
                                    FieldEntityMapping[i].Value.Add(node.StartOver);
                                }
                                break;
                            }
                        case "allowrecording":
                            {
                                if (node.AllowRecording != null)
                                {
                                    FieldEntityMapping[i].Value.Add(node.AllowRecording);
                                }
                                break;
                            }
                        case "downloadable":
                            {
                                if (node.Downloadable != null)
                                {
                                    FieldEntityMapping[i].Value.Add(node.Downloadable);
                                }
                                break;
                            }
                        default:
                            break;
                    }
                }
            }
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
    }
}
