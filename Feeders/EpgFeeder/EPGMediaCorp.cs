using ApiObjects;
using ConfigurationManager;
using EpgBL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml;
using TvinciImporter;

namespace EpgFeeder
{
    public class EPGMediaCorp : EPGImplementor
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        List<string> FilePathList = new List<string>();
        string sPath_successPath;
        string sPath_FailedPath;
        string userName;
        string password;
        NetworkCredential NetCredential;
        string UpdaterID = "700";
        bool ProcessError = false;

        List<DateTime> EPGDateRang;//not used

        protected override string LogFileName
        {
            get
            {
                return "EPGMediaCorp";
            }
        }

        public EPGMediaCorp(string sGroupID)
            : base(sGroupID)
        {

        }

        public EPGMediaCorp(string sGroupID, string sPathType, string sPath, Dictionary<string, string> sExtraParamter)
            : base(sGroupID, sPathType, sPath, sExtraParamter)
        {
            // Init NetworkCredential          
            sExtraParamter.TryGetValue("FTPUserName", out userName);
            sExtraParamter.TryGetValue("FTPPassword", out password);
            sExtraParamter.TryGetValue("FTPSuccessFolder", out sPath_successPath);
            sExtraParamter.TryGetValue("FTPFailedFolder", out sPath_FailedPath);

            NetCredential = new NetworkCredential(userName, password);
            EPGDateRang = new List<DateTime>();
        }
        /// <summary>
        /// Save Chaneel
        /// </summary>
        public override Dictionary<DateTime, List<int>> SaveChannel()
        {
            //Get list file name from the spasfic Path)
            Dictionary<DateTime, List<int>> dateWithChannelIds = new Dictionary<DateTime, List<int>>(new DateComparer());

            LoadFile();

            foreach (string fname in FilePathList)
            {
                Dictionary<DateTime, List<int>> epgDateVsChannelIds = SaveMediaCorpEPGData(fname);
                dateWithChannelIds.AddRange<DateTime, List<int>>(epgDateVsChannelIds);
            }

            return dateWithChannelIds;
        }
        /// <summary>
        /// Get Channel
        /// </summary>
        public override void GetChannel()
        {
            //throw new NotImplementedException();
        }
        protected int GetExistEPGTagID(string value, int EPGTagTypeId)
        {
            int res = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += " select id from EPG_tags";
            selectQuery += "Where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("Group_ID", "=", s_GroupID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(value)))", "=", value.Trim().ToLower());
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("epg_tag_type_id", "=", EPGTagTypeId);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);

            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                res = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "ID", 0);
            }
            selectQuery.Finish();
            selectQuery = null;
            return res;
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
            selectProgramQuery.Finish();
            selectProgramQuery = null;
            return res;
        }

        private string GetSingleNodeValue(XmlNode node, string xpath)
        {
            string res = "";
            try
            {
                res = node.SelectSingleNode(xpath).InnerText;
            }
            catch (Exception exp)
            {
                string errorMessage = string.Format("could not get the node '{0}' innerText value, error:{1}", xpath, exp.Message);
                log.Error("Media Corp: Upload EPG File - " + errorMessage, exp);
            }
            return res;
        }

        private void DeleteProgramsByChannelAndDate(Int32 channelID, DateTime dProgStartDate)
        {
            DateTime dProgEndDate = dProgStartDate.AddDays(1).AddMilliseconds(-1);

            #region Delete all existing programs in CB that have start/end dates within the new schedule
            int nParentGroupID = int.Parse(m_ParentGroupId);
            BaseEpgBL oEpgBL = EpgBL.Utils.GetInstance(nParentGroupID);
            List<DateTime> lDates = new List<DateTime>() { dProgStartDate };

            log.Debug("Delete Program on Date - " + string.Format("Group ID = {0}; Deleting Programs  that belong to channel {1}", s_GroupID, channelID));

            oEpgBL.RemoveGroupPrograms(lDates, channelID);
            #endregion

            #region Delete all existing programs in DB that have start/end dates within the new schedule

            DeleteScheduleProgramByDate(channelID, dProgStartDate);

            #endregion

            #region Delete all existing programs in ES that have start/end dates within the new schedule
            bool resDelete = Utils.DeleteEPGDocFromES(m_ParentGroupId, channelID, lDates);
            #endregion

        }




        /// <summary>
        /// Save EPG schedule program from file name
        /// </summary>
        /// <param name="sFileName">set file name</param>
        private Dictionary<DateTime, List<int>> SaveMediaCorpEPGData(string sFileName)
        {
            Dictionary<DateTime, List<int>> epgDateWithChannelIds = new Dictionary<DateTime, List<int>>(new DateComparer());
            //List<int> lProgramIds = new List<int>();
            FtpWebResponse response = null;
            Stream stream = null;
            string channel_id = "";
            bool enabledelete = false;
            ProcessError = false;


            //-------------------- Start Add Schedule EPG to Data Base ------------------------------//
            //
            //
            //create xml document to load FTP file
            Int32 channelID = 0;

            stream = GetFTPStreamFile(sFileName, out response);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(stream);

            XmlNodeList xmlnodelist = xmlDoc.GetElementsByTagName("program");
            if (xmlnodelist.Count > 0)
            {
                channel_id = GetSingleNodeValue(xmlnodelist[0], "channel_id");
                channelID = GetExistChannel(channel_id);
            }

            try
            {
                if (channelID > 0)
                {
                    log.Debug("Media Corp: EPG - " + string.Format("\r\n###################################### START EPG Channel {0} ######################################\r\n", channelID));

                    List<FieldTypeEntity> FieldEntityMapping = GetMappingFields();

                    //EPGDateRang = new List<DateTime>(); //not used
                    #region get Group ID (parent Group if possible)
                    int groupID = 0;
                    int nGroupID = ODBCWrapper.Utils.GetIntSafeVal(s_GroupID);
                    if (!string.IsNullOrEmpty(m_ParentGroupId))
                    {
                        groupID = int.Parse(m_ParentGroupId);
                    }
                    else
                    {
                        groupID = DAL.UtilsDal.GetParentGroupID(nGroupID);
                    }
                    #endregion

                    BaseEpgBL oEpgBL = EpgBL.Utils.GetInstance(groupID);                    
                    int nCountPackage = ApplicationConfiguration.CatalogLogicConfiguration.UpdateEPGPackage.IntValue;
                    int nCount = 0;
                    List<ulong> ulProgram = new List<ulong>();
                    List<DateTime> deletedDays = new List<DateTime>();
                    Dictionary<string, EpgCB> epgDic = new Dictionary<string, EpgCB>();

                    foreach (XmlNode node in xmlnodelist)
                    {
                        Guid EPGGuid = Guid.NewGuid();

                        #region Basic xml Data
                        string schedule_date = GetSingleNodeValue(node, "schedule_date");
                        string start_time = GetSingleNodeValue(node, "start_time");
                        string duration = GetSingleNodeValue(node, "duration");
                        string program_desc_english = GetSingleNodeValue(node, "program_desc_english");
                        string program_desc_chinese = GetSingleNodeValue(node, "program_desc_chinese");
                        string episode_no = GetSingleNodeValue(node, "episode_no");
                        string syp = GetSingleNodeValue(node, "syp");
                        string syp_chi = GetSingleNodeValue(node, "syp_chi");
                        string epg_poster = GetSingleNodeValue(node, "epg_poster");
                        #endregion

                        #region Set field mapping valus
                        SetMappingValues(FieldEntityMapping, node);
                        #endregion

                        #region Delete Programs by channel + date

                        DateTime dDate = ParseEPGStrToDate(schedule_date, "000000");// get all day start from 00:00:00
                        if (!deletedDays.Contains(dDate))
                        {
                            deletedDays.Add(dDate);
                            DeleteProgramsByChannelAndDate(channelID, dDate);
                        }

                        #endregion

                        #region Generate EPG CB

                        DateTime dProgStartDate = ParseEPGStrToDate(schedule_date, start_time);
                        DateTime dProgEndDate = dProgStartDate.Add(GetProgramDuration(duration));
                        // AddDateRange(dProgStartDate); //not used

                        EpgCB newEpgItem = generateEPGCB(epg_poster, program_desc_english, program_desc_chinese, episode_no, syp, syp_chi, channelID, EPGGuid.ToString(), dProgStartDate, dProgEndDate, node);

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

                        //  lProgramIds.Add(ProgramID);                            
                    }

                    if (nCount > 0 && ulProgram != null && ulProgram.Count > 0)
                    {
                        bool resultEpgIndex = UpdateEpgIndex(ulProgram, nGroupID, ApiObjects.eAction.Update);
                    }

                    //start Upload proccess Queue
                    UploadQueue.UploadQueueHelper.SetJobsForUpload(nGroupID);

                    //foreach (DateTime date in EPGDateRang)
                    //{
                    //    DeleteScheduleProgramByDate(channelID, date);
                    //    ApproverdScheduleProgramByDate(channelID, date);
                    //}

                    if (response != null)
                        response.Close();

                    if (stream != null)
                        stream.Close();

                    enabledelete = MoveFile(sFileName);
                    log.Debug("Media Corp: EPG - " + string.Format("\r\n###################################### END EPG Channel {0} ######################################\r\n", channelID));

                    //
                    //
                    //-------------------- End Add Schedule EPG to Data Base --------------------------------//
                }
                else
                {
                    ProcessError = true;
                    log.Debug("Media Corp: Channel Id doesn’t exist - " + string.Format("could not add programs schedule for EPG channel id '{0}' in file name: {1}", channel_id, sFileName));

                    if (response != null)
                        response.Close();

                    if (stream != null)
                        stream.Close();
                    enabledelete = MoveFile(sFileName);
                }
            }
            catch (Exception exp)
            {
                ProcessError = true;
                if (response != null)
                    response.Close();

                if (stream != null)
                    stream.Close();
                log.Error("Media Corp: Upload EPG File - " + string.Format("there an error occurring during the process Upload EPG File '{0}', Error : {1}", sFileName, exp.Message));
                enabledelete = MoveFile(sFileName);
            }
            finally
            {
                if (response != null)
                    response.Close();

                if (stream != null)
                    stream.Close();

                //delete the source file that proccess even if the proc
                if (enabledelete)
                {
                    DeleteFile(sFileName);
                }
            }

            return epgDateWithChannelIds;
        }

        private string GetMoveFilePath()
        {
            if (ProcessError)
            {
                return sPath_FailedPath;
            }
            else
            {
                return sPath_successPath;
            }
        }

        private void InsertProgramSchedule(string program_desc_english, string program_desc_chinese, string episode_no, string syp, string syp_chi, int channelID, string EPGGuid, DateTime dProgStartDate, DateTime dProgEndDate)
        {
            try
            {
                ODBCWrapper.InsertQuery insertProgQuery = new ODBCWrapper.InsertQuery("epg_channels_schedule");
                insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", string.Format("{0} {1} {2}", program_desc_english, program_desc_chinese, episode_no));
                insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", string.Format("{0} {1}", syp, syp_chi));
                insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", s_GroupID);
                insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_CHANNEL_ID", "=", channelID);
                insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_IDENTIFIER", "=", EPGGuid);
                insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("PIC_ID", "=", 0);
                insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "=", dProgStartDate);
                insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", dProgEndDate);
                insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                insertProgQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", UpdaterID);

                bool res = insertProgQuery.Execute();

                insertProgQuery.Finish();
                insertProgQuery = null;
                if (!res)
                {
                    log.Debug("InsertProgramSchedule - " + string.Format("could not Insert Program Schedule in channelID '{0}' ,start date {1} end date {2}  , error message: SQL execute query error.", channelID, dProgStartDate, dProgEndDate));
                }
            }
            catch (Exception exp)
            {
                log.Error("InsertProgramSchedule - " + string.Format("could not Insert Program Schedule in channelID '{0}' ,start date {1} end date {2}  , error message: {2}", channelID, dProgStartDate, dProgEndDate, exp.Message), exp);
            }
        }
        private EpgCB generateEPGCB(string epg_poster, string program_desc_english, string program_desc_chinese, string episode_no, string syp, string syp_chi,
            int channelID, string EPGGuid, DateTime dProgStartDate, DateTime dProgEndDate, XmlNode progItem)
        {
            EpgCB newEpgItem = new EpgCB();
            try
            {
                //BaseEpgBL oEpgBL = EpgBL.Utils.GetInstance(int.Parse(m_ParentGroupId));

                log.Debug("generateEPGCB - " + string.Format("EpgIdentifier '{0}' ", EPGGuid));

                newEpgItem.ChannelID = channelID;
                newEpgItem.Name = string.Format("{0} {1} {2}", program_desc_english, program_desc_chinese, episode_no);
                newEpgItem.Description = string.Format("{0} {1}", syp, syp_chi);
                newEpgItem.GroupID = ODBCWrapper.Utils.GetIntSafeVal(s_GroupID);
                newEpgItem.ParentGroupID = int.Parse(m_ParentGroupId);
                newEpgItem.EpgIdentifier = EPGGuid;
                newEpgItem.StartDate = dProgStartDate;
                newEpgItem.EndDate = dProgEndDate;
                newEpgItem.UpdateDate = DateTime.UtcNow;
                newEpgItem.CreateDate = DateTime.UtcNow;
                newEpgItem.isActive = true;
                newEpgItem.Status = 1;

                List<FieldTypeEntity> lFieldTypeEntity = GetMappingFields();
                SetMappingValues(lFieldTypeEntity, progItem);

                newEpgItem.Metas = Utils.GetEpgProgramMetas(lFieldTypeEntity);
                // When We stop insert to DB , we still need to insert new tags to DB !!!!!!!
                newEpgItem.Tags = Utils.GetEpgProgramTags(lFieldTypeEntity);

                #region Update Image ID
                if (!string.IsNullOrEmpty(epg_poster))
                {
                    int nPicID = ImporterImpl.DownloadEPGPic(epg_poster, program_desc_english, int.Parse(s_GroupID), 0, channelID);
                    if (nPicID != 0)
                    {
                        newEpgItem.PicID = nPicID;
                        newEpgItem.PicUrl = TVinciShared.CouchBaseManipulator.getEpgPicUrl(nPicID);
                    }
                }
                #endregion
            }
            catch (Exception exp)
            {
                log.Error("generateEPGCB - " + string.Format("could not generate Program Schedule in channelID '{0}' ,start date {1} end date {2}  , error message: {2}", channelID, dProgStartDate, dProgEndDate, exp.Message), exp);
            }
            return newEpgItem;
        }

        private void InsertEPGProgramMetaValue(string value, int EPGMetaID, int ProgramID)
        {
            try
            {
                ODBCWrapper.InsertQuery insertMetaProgQuery = new ODBCWrapper.InsertQuery("EPG_program_metas");
                insertMetaProgQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", s_GroupID);
                insertMetaProgQuery += ODBCWrapper.Parameter.NEW_PARAM("value", "=", value);
                insertMetaProgQuery += ODBCWrapper.Parameter.NEW_PARAM("epg_meta_id", "=", EPGMetaID);
                insertMetaProgQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                insertMetaProgQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", UpdaterID);
                insertMetaProgQuery += ODBCWrapper.Parameter.NEW_PARAM("program_id", "=", ProgramID);
                insertMetaProgQuery.Execute();
                insertMetaProgQuery.Finish();
                insertMetaProgQuery = null;
            }
            catch (Exception exp)
            {
                log.Error("InsertEPGProgramMetaValue - " + string.Format("could not Insert EPG Program Meta Value '{0}' epg_meta_id '{1}' , error message: {2}", value, EPGMetaID, exp.Message), exp);
            }
        }

        /// <summary>
        /// remove channel schedule from DB
        /// </summary>
        /// <returns></returns>
        public override bool ResetChannelSchedule()
        {
            try
            {
            }
            catch (Exception exp)
            {
                log.Error("", exp);
            }
            return true;
        }
        private DateTime ParseEPGStrToDate(string date, string programtime)
        {
            DateTime dt = new DateTime();
            try
            {
                int year = int.Parse(date.Substring(0, 4));
                int month = int.Parse(date.Substring(4, 2));
                int day = int.Parse(date.Substring(6, 2));
                int hour = int.Parse(programtime.Substring(0, 2));
                int min = int.Parse(programtime.Substring(2, 2));
                int sec = int.Parse(programtime.Substring(4, 2));
                dt = new DateTime(year, month, day, hour, min, sec);
            }
            catch (Exception exp)
            {
                string errormessage = string.Format("Media Corp: Upload EPG File", "could not parse EPG date field value '{0} - {1}', error message: {2} \r\n", date, programtime, exp.Message);
                log.Error("Media Corp: Upload EPG File - " + errormessage, exp);
            }
            return dt;
        }
        private TimeSpan GetProgramDuration(string duration)
        {
            TimeSpan tspan;
            int hour = int.Parse(duration.Substring(0, 2));
            int min = int.Parse(duration.Substring(2, 2));
            int sec = int.Parse(duration.Substring(4, 2));

            tspan = new TimeSpan(hour, min, sec);
            return tspan;
        }
        /// <summary>
        /// read files from source directory
        /// </summary>
        /// <param name="filename">set File name</param>
        private void ReadFile(string filename)
        {


        }
        /// <summary>
        /// download FTP stream file
        /// </summary>
        /// <param name="sFileName">set file name</param>
        /// <param name="response">out FtpWebResponse response</param>
        /// <returns>Download Stream object </returns>
        private Stream GetFTPStreamFile(string sFileName, out FtpWebResponse response)
        {
            Stream stream = null;
            try
            {
                //create instance FTPWebRequest to the specific path. 
                FtpWebRequest reqdownloadFTP = (FtpWebRequest)WebRequest.Create(new Uri(string.Format("{0}/{1}", s_Path, sFileName)));
                //Add Network Credential
                reqdownloadFTP.Credentials = NetCredential;
                //specific the action method
                reqdownloadFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                reqdownloadFTP.UseBinary = true;


                //get response 
                FtpWebResponse responseDownload = (FtpWebResponse)reqdownloadFTP.GetResponse();
                //stream response
                stream = responseDownload.GetResponseStream();
                response = responseDownload;

            }
            catch (Exception exp)
            {
                log.Error("Media Corp: EPG FTP Stream " + string.Format("there an error occurred during the Get FTP Stream file process, Stream file '{0}' , Error: {1}", sFileName, exp.Message), exp);
                response = null;
            }

            return stream;
        }
        /// <summary>
        /// Move file to specified destination
        /// </summary>
        /// <param name="sFileName">set file name</param>
        /// <param name="sTargetDirectoryPath">Target Directory Path</param>
        /// <returns>return true if success else return false</returns>
        private bool MoveFile(string sFileName)
        {
            bool res = false;
            FtpWebResponse response = null;
            Stream stream = null;
            FtpWebResponse Uploadresponse = null;
            string sTargetDirectoryPath = GetMoveFilePath();
            try
            {
                // Get the object used to communicate with the server.
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(string.Format("{0}/{1}", sTargetDirectoryPath, sFileName)));
                request.Method = WebRequestMethods.Ftp.UploadFile;

                // This example assumes the FTP site uses anonymous logon.
                request.Credentials = NetCredential;
                stream = GetFTPStreamFile(sFileName, out response);
                // Copy the contents of the file to the request stream.
                StreamReader sourceStream = new StreamReader(stream);


                byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
                sourceStream.Close();

                if (response != null)
                    response.Close();

                if (stream != null)
                    stream.Close();
                request.ContentLength = fileContents.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(fileContents, 0, fileContents.Length);
                requestStream.Close();

                Uploadresponse = (FtpWebResponse)request.GetResponse();

                res = true;

                log.Debug("Media Corp: EPG Move file. " + string.Format("Move source file '{0}' to directory path '{1}' success .! , response status: {2}", sFileName, sTargetDirectoryPath, Uploadresponse.StatusDescription));
            }
            catch (Exception exp)
            {
                if (response != null)
                    response.Close();

                if (stream != null)
                    stream.Close();

                if (Uploadresponse != null)
                    Uploadresponse.Close();

                log.Error("Media Corp: EPG Move file: " + string.Format("there an error occurring during move file process, Move file '{0}' , Error: {1}", sFileName, exp.Message), exp);
            }
            finally
            {
                if (response != null)
                    response.Close();

                if (stream != null)
                    stream.Close();

                if (Uploadresponse != null)
                    Uploadresponse.Close();
            }
            return res;
        }
        /// <summary>
        /// Delete file from source 
        /// </summary>
        /// <param name="sFileName">set file name</param>
        /// <returns>return true if success else return false</returns>
        private bool DeleteFile(string sFileName)
        {
            bool res = false;
            FtpWebResponse response = null;
            try
            {

                FtpWebRequest reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(string.Format("{0}/{1}", s_Path, sFileName)));
                reqFTP.Credentials = NetCredential;
                reqFTP.Method = WebRequestMethods.Ftp.DeleteFile;
                response = (FtpWebResponse)reqFTP.GetResponse();
                res = true;
                log.Debug("Media Corp: EPG Delete file - " + string.Format("delete source file '{0}' success .! , response status: {1}", sFileName, response.StatusDescription));
            }
            catch (Exception exp)
            {
                if (response != null)
                {
                    response.Close();
                }
                res = false;
                log.Error("MediaCorp_EPG_Delete - " + string.Format("there an error occurred during the delete process, delete file '{0}' EPGMediaCorp , Error: {1}", sFileName, exp.Message), exp);
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }

            return res;
        }
        /// <summary>
        /// Load files from source path
        /// </summary>
        private void LoadFile()
        {
            FtpWebResponse response = null;
            Stream stream = null;
            StreamReader reader = null;
            try
            {
                FtpWebRequest reqFTP;
                reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(string.Format("{0}", s_Path)));
                reqFTP.Credentials = NetCredential;
                reqFTP.Method = WebRequestMethods.Ftp.ListDirectory;
                response = (FtpWebResponse)reqFTP.GetResponse();
                stream = response.GetResponseStream();
                reader = new StreamReader(stream);
                string[] spleter = { " " };

                while (reader.Peek() > 0)
                {
                    string filename = "";
                    filename = reader.ReadLine();

                    if (!string.IsNullOrEmpty(filename))
                    {
                        string[] arrfilename = filename.Split(spleter, StringSplitOptions.RemoveEmptyEntries);
                        FilePathList.Add(arrfilename[arrfilename.Length - 1]);
                    }
                }
            }

            catch (Exception exp)
            {
                if (response != null)
                {
                    response.Close();
                }
                if (stream != null)
                {
                    stream.Close();
                }
                if (reader != null)
                {
                    reader.Close();
                }
                log.Error("MediaCorp_EPG_LoadFile - " + string.Format("there an error occurred during the Load Files process,  Error: {0}", exp.Message), exp);

            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
                if (stream != null)
                {
                    stream.Close();
                }
                if (reader != null)
                {
                    reader.Close();
                }
            }
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
            {
                log.Error("GetMediaIDByChannelID - " + string.Format("could not get Get Media ID By ChannelID by EPG_IDENTIFIER  {0}, error message: {1}", EPG_IDENTIFIER.ToString(), exp.Message), exp);
            }
            return res;
        }
        private Int32 GetExistChannel(string sChannelID)
        {
            Int32 res = 0;
            if (!string.IsNullOrEmpty(sChannelID))
            {
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
                {
                    log.Error("GetExistChannel - " + string.Format("could not get Get Exist Channel  by ID {0}, error message: {1}", sChannelID, exp.Message), exp);
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
            {
                log.Error("GetExistMedia - " + string.Format("could not get Exist Media by EPG Identifier {0}, error message: {1}", EPG_IDENTIFIER.ToString(), exp.Message), exp);
            }
            return res;
        }

        private bool AddDateRange(DateTime date)
        {
            bool res = false;
            try
            {
                if (!EPGDateRang.Exists(c => c.Day == date.Day && c.Month == date.Month && c.Year == date.Year))
                {
                    log.Debug("AddDateRange - " + string.Format("add date '{0}' to EPG date range success.", date.ToString()));
                    EPGDateRang.Add(date);
                    res = false;
                }
            }
            catch (Exception ex)
            {
                log.Error("AddDateRange - " + string.Format("error add date '{0}' to EPG date range , error message: {1}", date.ToString(), ex.Message), ex);
            }
            return res;
        }

        private void DeleteScheduleProgramByDate(int channelID, DateTime date)
        {
            DateTime fromDate = new DateTime(date.Year, date.Month, date.Day, 00, 00, 00);
            DateTime toDate = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);

            try
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("epg_channels_schedule");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 2);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", ">=", fromDate.ToString("yyyy-MM-dd HH:mm:ss"));
                updateQuery += " and ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "<=", toDate.ToString("yyyy-MM-dd HH:mm:ss"));
                updateQuery += " and ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                updateQuery += " and ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_CHANNEL_ID", "=", channelID);

                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;

                log.Debug("DeleteScheduleProgramByDate - " + string.Format("success delete schedule program EPG_CHANNEL_ID '{0}' between date {1} and {2}.", channelID, fromDate.ToString("yyyy-MM-dd HH:mm:ss"), toDate.ToString("yyyy-MM-dd HH:mm:ss")));
            }
            catch (Exception ex)
            {
                //ProcessError = true;
                log.Error("DeleteScheduleProgramByDate - " + string.Format("error delete schedule program EPG_CHANNEL_ID '{0}' between date {1} , error message: {2}", channelID, date.ToString(), ex.Message), ex);
            }
        }

        private void ApproverdScheduleProgramByDate(int channelID, DateTime date)
        {
            DateTime fromDate = new DateTime(date.Year, date.Month, date.Day, 00, 00, 00);
            DateTime toDate = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);

            try
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("epg_channels_schedule");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", ">=", fromDate.ToString("yyyy-MM-dd HH:mm:ss"));
                updateQuery += " and ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "<=", toDate.ToString("yyyy-MM-dd HH:mm:ss"));
                updateQuery += " and ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 5);
                updateQuery += " and ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_CHANNEL_ID", "=", channelID);

                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;

                log.Error("ApproverdScheduleProgramByDate " + string.Format("success approved schedule program EPG_CHANNEL_ID '{0}' between date {1} and {2}.\r\n", channelID, fromDate.ToString("yyyy-MM-dd HH:mm:ss"), toDate.ToString("yyyy-MM-dd HH:mm:ss")));
            }
            catch (Exception ex)
            {
                //ProcessError = true;
                log.Error("ApproverdScheduleProgramByDate " + string.Format("success approved schedule program EPG_CHANNEL_ID '{0}' between date {1} , error message: {2}", channelID, date.ToString(), ex.Message), ex);
            }
        }

        public override Dictionary<DateTime, List<int>> ProcessConcreteXmlFile(XmlDocument xmlDoc)
        {
            throw new NotImplementedException();
        }

    }
}
