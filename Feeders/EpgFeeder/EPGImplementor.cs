using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using System.Data;
using ApiObjects;
using TVinciShared;
using Tvinci.Core.DAL;
using System.Text.RegularExpressions;
using System.Configuration;
using EpgBL;
using TvinciImporter;
using KLogMonitor;
using System.Reflection;


namespace EpgFeeder
{
    public abstract class EPGImplementor
    {

        #region Consts

        protected virtual string LogFileName
        {
            get
            {
                return "EPGImplementor";
            }
        }

        protected static readonly Regex HebrewRegex = new Regex(@"\p{IsHebrew}");
        protected static readonly Regex RussianRegex = new Regex(@"\p{IsCyrillic}");
        protected static readonly Regex ArabicRegex = new Regex(@"\p{IsArabic}");

        protected static readonly int MaxDescriptionSize = 1024;

        #endregion

        #region Public Members
        public string m_ParentGroupId { get; set; }
        public string s_GroupID;
        public string s_PathType;
        public string s_Path;
        public Dictionary<string, string> s_ExtraParamter;

        #endregion

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private NetworkCredential m_NetCredential;
        private string m_SuccessPath;
        private string m_FailedPath;
        private bool m_ProcessError = false;
        private string UpdaterID = "700";


        public EPGImplementor(string sGroupID)
        {
            s_GroupID = sGroupID;
            try
            {
                m_ParentGroupId = DAL.UtilsDal.GetParentGroupID(int.Parse(s_GroupID)).ToString();
            }
            catch { }
        }

        public EPGImplementor(string sGroupID, string sPathType, string sPath, Dictionary<string, string> sExtraParamter)
        {
            s_GroupID = sGroupID;
            s_PathType = sPathType;
            s_Path = sPath;
            s_ExtraParamter = sExtraParamter;
            sExtraParamter.TryGetValue("FTPSuccessFolder", out m_SuccessPath);
            sExtraParamter.TryGetValue("FTPFailedFolder", out m_FailedPath);
            try
            {
                m_ParentGroupId = DAL.UtilsDal.GetParentGroupID(int.Parse(s_GroupID)).ToString();
            }
            catch { }
        }

        #region Abstracts Methods

        public abstract void GetChannel();
        public abstract bool ResetChannelSchedule();

        public abstract Dictionary<DateTime, List<int>> ProcessConcreteXmlFile(XmlDocument xmlDoc);

        #endregion

        #region Public Methods

        public virtual Dictionary<DateTime, List<int>> SaveChannel() //TBD:handle EPGMediaCorp
        {
            //List<int> programIds = new List<int>();
            Dictionary<DateTime, List<int>> dateWithChannelIds = new Dictionary<DateTime, List<int>>(new DateComparer());
            switch (s_PathType)
            {
                case "WebURL":
                    //programIds = ProcessFileFromUrlOrPath();
                    dateWithChannelIds = ProcessFileFromUrlOrPath();
                    break;
                case "FTP":
                    //programIds = ProcessFilesFromFtpFolder();
                    dateWithChannelIds = ProcessFilesFromFtpFolder();
                    break;

                case "Local":
                    break;

                default:
                    break;
            }

            return dateWithChannelIds;
        }

        protected virtual void SetMappingValues(List<FieldTypeEntity> FieldEntityMapping, tvProgramme node)
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
                        case "rating":
                            {
                                if (node.rating != null && node.rating.Length > 0)
                                {
                                    foreach (tvProgrammeRating tpr in node.rating)
                                    {
                                        FieldEntityMapping[i].Value.Add(tpr.value);
                                    }
                                }
                                break;
                            }
                        case "sub-title":
                            {
                                if (node.subtitle != null && node.subtitle.Length > 0)
                                {
                                    foreach (tvProgrammeSubtitle tps in node.subtitle)
                                    {
                                        FieldEntityMapping[i].Value.Add(tps.Value);
                                    }
                                }
                                break;
                            }
                        default:
                            break;
                    }
                }
            }
        }

        protected virtual void SetMappingValues(List<FieldTypeEntity> FieldEntityMapping, XmlNode node)
        {
            for (int i = 0; i < FieldEntityMapping.Count; i++)
            {
                FieldEntityMapping[i].Value = new List<string>();
                foreach (string XmlRefName in FieldEntityMapping[i].XmlReffName)
                {
                    foreach (XmlNode multinode in node.SelectNodes(XmlRefName))
                    {
                        if (!string.IsNullOrEmpty(multinode.InnerText))
                        {
                            FieldEntityMapping[i].Value.Add(multinode.InnerXml);
                        }
                    }
                }
            }
        }

        protected virtual void InsertEPGTags(List<FieldTypeEntity> FieldEntityMapping, int ProgramID)
        {
            var TagFieldEntity = from item in FieldEntityMapping
                                 where item.FieldType == enums.FieldTypes.Tag && item.XmlReffName.Capacity > 0 && item.Value.Count > 0
                                 select item;

            foreach (var item in TagFieldEntity)
            {
                foreach (string tagvalue in item.Value)
                {
                    int tagid = GetExistEPGTagID(tagvalue, item.ID);
                    if (tagid > 0)
                    {
                        //Inset New EPG Tag Value
                        if (!isExistProgramTag(ProgramID, item.ID))
                        {
                            InsertEPGProgramTag(ProgramID, tagid);
                        }
                    }
                    else
                    {
                        //Insert new EPG Tags
                        InsertEPGTagValue(tagvalue, item.ID);

                        tagid = GetExistEPGTagID(tagvalue, item.ID);

                        //Inset New EPG Tag Value
                        InsertEPGProgramTag(ProgramID, tagid);
                    }
                }
            }
        }

        protected virtual void InserEPGMetas(List<FieldTypeEntity> FieldEntityMapping, int ProgramID)
        {
            var MetaFieldEntity = from item in FieldEntityMapping
                                  where item.FieldType == enums.FieldTypes.Meta && item.XmlReffName.Capacity > 0 && item.Value.Count > 0
                                  select item;

            foreach (var item in MetaFieldEntity)
            {
                string value = string.Join(" ", item.Value.ToArray());
                //Insert EPG program Meta value
                InsertEPGProgramMetaValue(value, item.ID, ProgramID);
            }
        }

        protected virtual List<FieldTypeEntity> GetMappingFields()
        {
            List<FieldTypeEntity> AllFieldTypeMapping = new List<FieldTypeEntity>();
            List<FieldTypeEntity> AllFieldType = new List<FieldTypeEntity>();

            #region Basic fields definition
            ODBCWrapper.DataSetSelectQuery selectQueryBasic = new ODBCWrapper.DataSetSelectQuery();
            selectQueryBasic += " select * from lu_EPG_basics_types where status = 1 ";
            if (selectQueryBasic.Execute("query", true) != null)
            {
                int count = selectQueryBasic.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    int i = 0;
                    foreach (DataRowView dr in selectQueryBasic.Table("query").DefaultView)
                    {

                        FieldTypeEntity item = new FieldTypeEntity();
                        item.ID = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
                        item.Name = ODBCWrapper.Utils.GetSafeStr(dr, "Name");
                        item.FieldType = enums.FieldTypes.Basic;
                        AllFieldType.Add(item);
                        i++;
                    }
                }
            }
            selectQueryBasic.Finish();
            selectQueryBasic = null;
            #endregion

            string sGroupTreeStr = PageUtils.GetAllGroupTreeStr(int.Parse(s_GroupID));

            #region Meta fields definition
            ODBCWrapper.DataSetSelectQuery selectQueryMetas = new ODBCWrapper.DataSetSelectQuery();
            selectQueryMetas += " select * from EPG_metas_types where group_id ";
            selectQueryMetas += sGroupTreeStr;
            selectQueryMetas += " and status = 1 ";
            if (selectQueryMetas.Execute("query", true) != null)
            {
                int count = selectQueryMetas.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    int i = 0;
                    foreach (DataRowView dr in selectQueryMetas.Table("query").DefaultView)
                    {

                        FieldTypeEntity item = new FieldTypeEntity();
                        item.ID = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
                        item.Name = ODBCWrapper.Utils.GetSafeStr(dr, "Name");
                        item.FieldType = enums.FieldTypes.Meta;
                        AllFieldType.Add(item);
                        i++;
                    }
                }
            }
            selectQueryMetas.Finish();
            selectQueryMetas = null;
            #endregion

            #region Tag fields defintion
            ODBCWrapper.DataSetSelectQuery selectQueryTags = new ODBCWrapper.DataSetSelectQuery();
            selectQueryTags += " select * from EPG_tags_types where group_id ";
            selectQueryTags += sGroupTreeStr;
            selectQueryTags += " and status = 1 ";
            if (selectQueryTags.Execute("query", true) != null)
            {
                int count = selectQueryTags.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    int i = 0;
                    foreach (DataRowView dr in selectQueryTags.Table("query").DefaultView)
                    {

                        FieldTypeEntity item = new FieldTypeEntity();
                        item.ID = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
                        item.Name = ODBCWrapper.Utils.GetSafeStr(dr, "Name");
                        item.FieldType = enums.FieldTypes.Tag;
                        AllFieldType.Add(item);
                        i++;
                    }
                }
            }
            selectQueryTags.Finish();
            selectQueryTags = null;
            #endregion

            #region Set xml refereance field name
            ODBCWrapper.DataSetSelectQuery selectQueryFieldMapping = new ODBCWrapper.DataSetSelectQuery();
            selectQueryFieldMapping += " select * from EPG_fields_mapping where  group_id ";
            selectQueryFieldMapping += sGroupTreeStr;
            selectQueryFieldMapping += "and status = 1 and is_active = 1";
            if (selectQueryFieldMapping.Execute("query", true) != null)
            {
                int count = selectQueryFieldMapping.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    int i = 0;
                    foreach (DataRowView dr in selectQueryFieldMapping.Table("query").DefaultView)
                    {
                        enums.FieldTypes type = (enums.FieldTypes)Enum.Parse(typeof(enums.FieldTypes), ODBCWrapper.Utils.GetSafeStr(dr["type"]));

                        foreach (var x in AllFieldType.FindAll(c => c.FieldType == type && c.ID == ODBCWrapper.Utils.GetIntSafeVal(selectQueryFieldMapping, "field_id", i)))
                        {
                            if (x.XmlReffName == null)
                            {
                                x.XmlReffName = new List<string>();
                                x.XmlReffName.Add(ODBCWrapper.Utils.GetSafeStr(dr, "external_ref"));
                            }
                            else
                            {
                                x.XmlReffName.Add(ODBCWrapper.Utils.GetSafeStr(dr, "external_ref"));
                            }

                        }
                        i++;
                    }
                }
            }
            selectQueryFieldMapping.Finish();
            selectQueryFieldMapping = null;
            #endregion
            AllFieldTypeMapping = AllFieldType.FindAll(x => x.XmlReffName.Count > 0);
            return AllFieldTypeMapping;
        }

        public virtual List<EPGChannelProgrammeObject> GetEPGChannelPrograms(Int32 groupID, string sEPGChannelID, string sPicSize, EPGUnit unit, int nFromOffsetUnit, int nToOffsetUnit, int nUTCOffset)
        {
            List<EPGChannelProgrammeObject> res = new List<EPGChannelProgrammeObject>();
            DataTable dtChannelsSchedule = null;

            DateTime fromOffsetDay;
            DateTime toOffsetDay;

            DateTime UTCOffsetFromTimeZone = DateTimeOffset.UtcNow.AddHours(nUTCOffset).DateTime;
            DateTime UTCOffsetToTimeZone = DateTimeOffset.UtcNow.AddHours(nUTCOffset).DateTime;

            switch (unit)
            {
                case EPGUnit.Days:
                    DateTime fromDay = UTCOffsetFromTimeZone.AddDays(nFromOffsetUnit);
                    DateTime toDay = UTCOffsetToTimeZone.AddDays(nToOffsetUnit);
                    fromOffsetDay = new DateTime(fromDay.Year, fromDay.Month, fromDay.Day, 00, 00, 00);
                    toOffsetDay = new DateTime(toDay.Year, toDay.Month, toDay.Day, 23, 59, 59);

                    #region Get EPG Program Schedule
                    dtChannelsSchedule = EpgDal.GetEpgScheduleDataTable(null, groupID, fromOffsetDay, toOffsetDay, int.Parse(sEPGChannelID));
                    res = GetEpgScheduleList(groupID, dtChannelsSchedule, sPicSize);
                    #endregion
                    break;
                case EPGUnit.Hours:

                    fromOffsetDay = UTCOffsetFromTimeZone.AddHours(nFromOffsetUnit);
                    toOffsetDay = UTCOffsetToTimeZone.AddHours(nToOffsetUnit);

                    #region Get EPG Program Schedule
                    dtChannelsSchedule = EpgDal.GetEpgScheduleDataTable(null, groupID, fromOffsetDay, toOffsetDay, int.Parse(sEPGChannelID));
                    res = GetEpgScheduleList(groupID, dtChannelsSchedule, sPicSize);
                    #endregion
                    break;

                case EPGUnit.Current:
                    fromOffsetDay = UTCOffsetFromTimeZone;
                    toOffsetDay = UTCOffsetToTimeZone;

                    #region Get EPG Program Schedule
                    if (nFromOffsetUnit > 0)
                    {
                        #region Get Before Programmes
                        dtChannelsSchedule = EpgDal.GetEpgScheduleDataTable(nFromOffsetUnit, groupID, fromOffsetDay, null, int.Parse(sEPGChannelID));
                        res = GetEpgScheduleList(groupID, dtChannelsSchedule, sPicSize);
                        #endregion
                    }
                    #region Get Current Program
                    dtChannelsSchedule = EpgDal.GetEpgScheduleDataTable(null, groupID, fromOffsetDay, toOffsetDay, int.Parse(sEPGChannelID));
                    List<EPGChannelProgrammeObject> currentList = GetEpgScheduleList(groupID, dtChannelsSchedule, sPicSize);
                    if (currentList != null && currentList.Count > 0)
                    {
                        res.AddRange(currentList);
                    }
                    #endregion

                    if (nToOffsetUnit > 0)
                    {
                        #region Get after Programmes
                        dtChannelsSchedule = EpgDal.GetEpgScheduleDataTable(nToOffsetUnit, groupID, null, toOffsetDay, int.Parse(sEPGChannelID));
                        List<EPGChannelProgrammeObject> afterList = GetEpgScheduleList(groupID, dtChannelsSchedule, sPicSize);
                        if (afterList != null && afterList.Count > 0)
                        {
                            res.AddRange(afterList);
                        }
                        #endregion
                    }
                    #endregion
                    break;
            }
            return res;
        }

        public virtual List<EPGChannelProgrammeObject> GetEPGChannelProgramsByDates(Int32 groupID, string sEPGChannelID, string sPicSize, DateTime fromDay, DateTime toDay, double nUTCOffset)
        {
            List<EPGChannelProgrammeObject> res = new List<EPGChannelProgrammeObject>();
            DateTime fromUTCDay = fromDay.AddHours(nUTCOffset);
            DateTime toUTCDay = toDay.AddHours(nUTCOffset);

            DataTable dtChannelsSchedule = EpgDal.GetEpgScheduleDataTable(null, groupID, fromUTCDay, toUTCDay, int.Parse(sEPGChannelID));

            res = GetEpgScheduleList(groupID, dtChannelsSchedule, sPicSize);

            return res;
        }

        public virtual List<EPGChannelProgrammeObject> GetEPGMultiChannelPrograms(Int32 groupID, string[] sEPGChannelIDs, string sPicSize, EPGUnit unit, int nFromOffsetUnit, int nToOffsetUnit, int nUTCOffset)
        {
            List<EPGChannelProgrammeObject> res = new List<EPGChannelProgrammeObject>();
            DataTable dtMultiChannelsSchedule = null;


            DateTime fromOffsetDay;
            DateTime toOffsetDay;

            DateTime UTCOffsetFromTimeZone = DateTimeOffset.UtcNow.AddHours(nUTCOffset).DateTime;
            DateTime UTCOffsetToTimeZone = DateTimeOffset.UtcNow.AddHours(nUTCOffset).DateTime;


            switch (unit)
            {
                case EPGUnit.Days:
                    DateTime fromDay = UTCOffsetFromTimeZone.AddDays(nFromOffsetUnit);
                    DateTime toDay = UTCOffsetToTimeZone.AddDays(nToOffsetUnit);
                    fromOffsetDay = new DateTime(fromDay.Year, fromDay.Month, fromDay.Day, 00, 00, 00);
                    toOffsetDay = new DateTime(toDay.Year, toDay.Month, toDay.Day, 23, 59, 59);

                    #region Get EPG Program Schedule
                    dtMultiChannelsSchedule = EpgDal.GetEpgMultiScheduleDataTable(null, groupID, fromOffsetDay, toOffsetDay, sEPGChannelIDs);
                    res = GetEpgScheduleList(groupID, dtMultiChannelsSchedule, sPicSize);
                    #endregion
                    break;
                case EPGUnit.Hours:

                    fromOffsetDay = UTCOffsetFromTimeZone.AddHours(nFromOffsetUnit);
                    toOffsetDay = UTCOffsetToTimeZone.AddHours(nToOffsetUnit);

                    #region Get EPG Program Schedule
                    dtMultiChannelsSchedule = EpgDal.GetEpgMultiScheduleDataTable(null, groupID, fromOffsetDay, toOffsetDay, sEPGChannelIDs);
                    res = GetEpgScheduleList(groupID, dtMultiChannelsSchedule, sPicSize);
                    #endregion
                    break;

                case EPGUnit.Current:
                    fromOffsetDay = UTCOffsetFromTimeZone;
                    toOffsetDay = UTCOffsetToTimeZone;

                    #region Get EPG Program Schedule
                    if (nFromOffsetUnit > 0)
                    {
                        #region Get Before Programmes
                        dtMultiChannelsSchedule = EpgDal.GetEpgMultiScheduleDataTable(nFromOffsetUnit, groupID, fromOffsetDay, null, sEPGChannelIDs);
                        res = GetEpgScheduleList(groupID, dtMultiChannelsSchedule, sPicSize);
                        #endregion
                    }
                    #region Get Current Program
                    dtMultiChannelsSchedule = EpgDal.GetEpgMultiScheduleDataTable(null, groupID, fromOffsetDay, toOffsetDay, sEPGChannelIDs);
                    List<EPGChannelProgrammeObject> currentList = GetEpgScheduleList(groupID, dtMultiChannelsSchedule, sPicSize);
                    if (currentList != null && currentList.Count > 0)
                    {
                        res.AddRange(currentList);
                    }
                    #endregion

                    if (nToOffsetUnit > 0)
                    {
                        #region Get after Programmes
                        dtMultiChannelsSchedule = EpgDal.GetEpgMultiScheduleDataTable(nToOffsetUnit, groupID, null, toOffsetDay, sEPGChannelIDs);
                        List<EPGChannelProgrammeObject> afterList = GetEpgScheduleList(groupID, dtMultiChannelsSchedule, sPicSize);
                        if (afterList != null && afterList.Count > 0)
                        {
                            res.AddRange(afterList);
                        }
                        #endregion
                    }
                    #endregion
                    break;
            }
            return res;
        }

        public virtual List<EPGChannelProgrammeObject> GetEPGProgramsByScids(int groupID, string[] scids, Language eLang, int duration)
        {
            return new List<EPGChannelProgrammeObject>();
        }

        protected virtual void InsertEPGProgramMetaValue(string value, int EPGMetaID, int ProgramID)
        {
            try
            {
                if (!string.IsNullOrEmpty(value))
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
            }
            catch (Exception exp)
            {
                log.Error("InsertEPGProgramMetaValue - " + string.Format("could not Insert EPG Program Meta Value '{0}' epg_meta_id '{1}' , error message: {2}", value, EPGMetaID, exp.Message), exp);
            }
        }

        public virtual List<EPGChannelProgrammeObject> SearchEPGContent(int groupID, string searchValue, int pageIndex, int pageSize)
        {
            List<EPGChannelProgrammeObject> res = new List<EPGChannelProgrammeObject>();
            return res;
        }

        public virtual Dictionary<DateTime, List<int>> ProcessFileFromUrlOrPath()
        {
            //List<int> ids = new List<int>();
            Dictionary<DateTime, List<int>> dateWithChannelIds = new Dictionary<DateTime, List<int>>(new DateComparer());
            XmlDocument xmlDoc = new XmlDocument();
            using (StreamReader reader = new StreamReader(s_Path, Encoding.UTF8))
            {
                xmlDoc.Load(reader);
                dateWithChannelIds = ProcessConcreteXmlFile(xmlDoc);
            }

            return dateWithChannelIds;
        }

        public virtual Dictionary<DateTime, List<int>> ProcessFilesFromFtpFolder()
        {
            Dictionary<DateTime, List<int>> datesVsChannelIds = new Dictionary<DateTime, List<int>>(new DateComparer());
            List<int> programIds = new List<int>();
            bool enabledelete = false;
            m_ProcessError = false;
            string userName = string.Empty;
            string password = string.Empty;
            string successPath = string.Empty;
            string failedPath = string.Empty;
            FtpWebResponse response = null;
            Stream stream = null;
            StreamReader reader = null;
            string filename = string.Empty;


            s_ExtraParamter.TryGetValue("FTPUserName", out userName);
            s_ExtraParamter.TryGetValue("FTPPassword", out password);
            s_ExtraParamter.TryGetValue("FTPSuccessFolder", out successPath);
            s_ExtraParamter.TryGetValue("FTPFailedFolder", out failedPath);


            try
            {
                log.Debug("EPGImplementor.ProcessFilesFromFtpFolder(), Before get response from ftp - " + string.Format("userName:{0},password:{1},successPath:{2},failedPath:{3}", userName, password, successPath, failedPath));

                m_NetCredential = new NetworkCredential(userName, password);
                FtpWebRequest reqFTP;
                reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(string.Format("{0}", s_Path)));
                reqFTP.Credentials = m_NetCredential;
                reqFTP.Method = WebRequestMethods.Ftp.ListDirectory;
                response = (FtpWebResponse)reqFTP.GetResponse();
                stream = response.GetResponseStream();
                reader = new StreamReader(stream);
                string[] spleter = { " " };

                while (reader.Peek() > 0)
                {

                    filename = reader.ReadLine();

                    if (!string.IsNullOrEmpty(filename) && (filename.EndsWith("xml") || filename.EndsWith("XML")))
                    {
                        Stream fileStream = null;
                        try
                        {
                            XmlDocument xmlDoc = GetXmlDocFromFTPFile(filename, m_NetCredential);
                            Dictionary<DateTime, List<int>> dateWithChannelIds = ProcessConcreteXmlFile(xmlDoc);
                            datesVsChannelIds.AddRange<DateTime, List<int>>(dateWithChannelIds);
                            //programIds.AddRange(ProcessConcreteXmlFile(xmlDoc));
                            //ProcessConcreteXmlFile(xmlDoc);
                            enabledelete = MoveFile(filename);
                        }
                        catch (Exception ex)
                        {
                            m_ProcessError = true;
                            enabledelete = MoveFile(filename);
                            log.Error("EPGImplementor.ProcessFilesFromFtpFolder() Exception: " + string.Format("there an error occurring during the processing  EPG File '{0}', Error : {1}", filename, ex.Message));
                        }
                        finally
                        {
                            if (fileStream != null)
                            {
                                fileStream.Close();
                            }
                            if (enabledelete == true)
                            {
                                DeleteFileFromFtp(filename);
                            }
                        }

                    }
                }

                log.Debug("EPGImplementor.ProcessFilesFromFtpFolder() After Processing files form ftp");
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

                log.Debug("EPG_LoadFiles_From_FTP - " + string.Format("there an error occurred during the Load Files process,  Error: {0}", exp.Message));
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

            //return programIds;
            return datesVsChannelIds;
        }

        protected virtual Language GetLanguageOfString(string s)
        {
            if (HebrewRegex.IsMatch(s))
                return Language.Hebrew;
            if (RussianRegex.IsMatch(s))
                return Language.Russian;
            if (ArabicRegex.IsMatch(s))
                return Language.Arabic;
            return Language.English;
        }


        protected virtual bool UpdateEpgIndex(List<ulong> epgIDs, int nGroupID, ApiObjects.eAction action)
        {
            bool result = false;
            try
            {
                result = ImporterImpl.UpdateEpgIndex(epgIDs, nGroupID, action);
                return result;
            }
            catch (Exception ex)
            {
                log.Error("EpgFeeder - " + string.Format("failed update EpgIndex ex={0}", ex.Message), ex);
                return false;
            }
        }
        #endregion

        #region Private Methods

        private int GetExistEPGTagID(string value, int EPGTagTypeId)
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
                if (count > 0)
                    res = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "ID", 0);
            }
            selectQuery.Finish();
            selectQuery = null;

            return res;
        }

        private List<EPGChannelProgrammeObject> GetEpgScheduleList(int groupID, DataTable dtChannelsSchedule, string sPicSize)
        {
            List<EPGChannelProgrammeObject> retList = new List<EPGChannelProgrammeObject>();

            if (dtChannelsSchedule != null && dtChannelsSchedule.Rows.Count > 0)
            {
                Dictionary<string, List<EPGDictionary>> AllEPG_ResponseTag = GetAllEPGTagProgram(groupID, dtChannelsSchedule);
                Dictionary<string, List<EPGDictionary>> AllEPG_ResponseMeta = GetAllEPGMetaProgram(groupID, dtChannelsSchedule);
                foreach (DataRowView dr in dtChannelsSchedule.DefaultView)
                {
                    EPGChannelProgrammeObject item = CreateEpgScheduleObject(dr, AllEPG_ResponseTag, AllEPG_ResponseMeta, sPicSize);
                    retList.Add(item);
                }
            }
            return retList;
        }

        private EPGChannelProgrammeObject CreateEpgScheduleObject(DataRowView datRowView, Dictionary<string, List<EPGDictionary>> AllEPG_ResponseTag,
                                                                  Dictionary<string, List<EPGDictionary>> AllEPG_ResponseMeta, string sPicSize)
        {
            long program_id = ODBCWrapper.Utils.GetLongSafeVal(datRowView, "id");
            string EPG_CHANNEL_ID = ODBCWrapper.Utils.GetSafeStr(datRowView, "EPG_CHANNEL_ID");
            string EPG_IDENTIFIER = ODBCWrapper.Utils.GetSafeStr(datRowView, "EPG_IDENTIFIER");
            string NAME = ODBCWrapper.Utils.GetSafeStr(datRowView, "NAME");
            string DESCRIPTION = ODBCWrapper.Utils.GetSafeStr(datRowView, "DESCRIPTION");
            DateTime oStartDate = ODBCWrapper.Utils.GetDateSafeVal(datRowView, "START_DATE");
            string START_DATE = oStartDate.ToString("dd/MM/yyyy HH:mm:ss");
            DateTime oEndtDate = ODBCWrapper.Utils.GetDateSafeVal(datRowView, "END_DATE");
            string END_DATE = oEndtDate.ToString("dd/MM/yyyy HH:mm:ss");
            long pic_id = ODBCWrapper.Utils.GetLongSafeVal(datRowView, "PIC_ID");
            string sPic_base_url = ODBCWrapper.Utils.GetSafeStr(datRowView, "pic_base_url");
            string sPic_remote_base_url = ODBCWrapper.Utils.GetSafeStr(datRowView, "pics_remote_base_url");
            string PIC_URL = PageUtils.GetPicURL(pic_id, sPic_base_url, sPic_remote_base_url, sPicSize);
            string STATUS = ODBCWrapper.Utils.GetSafeStr(datRowView, "STATUS");
            string IS_ACTIVE = ODBCWrapper.Utils.GetSafeStr(datRowView, "IS_ACTIVE");
            string GROUP_ID = ODBCWrapper.Utils.GetSafeStr(datRowView, "GROUP_ID");
            string UPDATER_ID = ODBCWrapper.Utils.GetSafeStr(datRowView, "UPDATER_ID");
            string UPDATE_DATE = ODBCWrapper.Utils.GetSafeStr(datRowView, "UPDATE_DATE");
            string PUBLISH_DATE = ODBCWrapper.Utils.GetSafeStr(datRowView, "PUBLISH_DATE");
            string CREATE_DATE = ODBCWrapper.Utils.GetSafeStr(datRowView, "CREATE_DATE");
            string media_id = ODBCWrapper.Utils.GetSafeStr(datRowView, "MEDIA_ID");
            int nLikeCounter = ODBCWrapper.Utils.GetIntSafeVal(datRowView, "like_counter");

            List<EPGDictionary> EPG_ResponseTag = (from t in AllEPG_ResponseTag
                                                   where t.Key == program_id.ToString()
                                                   select t.Value).FirstOrDefault<List<EPGDictionary>>();

            List<EPGDictionary> EPG_ResponseMeta = (from t in AllEPG_ResponseMeta
                                                    where t.Key == program_id.ToString()
                                                    select t.Value).FirstOrDefault<List<EPGDictionary>>();


            EPGChannelProgrammeObject item = new EPGChannelProgrammeObject();
            item.Initialize(program_id, EPG_CHANNEL_ID, EPG_IDENTIFIER, NAME, DESCRIPTION, START_DATE, END_DATE, PIC_URL, STATUS, IS_ACTIVE, GROUP_ID, UPDATER_ID, UPDATE_DATE, PUBLISH_DATE, CREATE_DATE, EPG_ResponseTag, EPG_ResponseMeta, media_id, nLikeCounter);
            return item;
        }

        private List<EPGChannelProgrammeObject> GetEpgMultiScheduleList(int groupID, DataTable dtMultiChannelsSchedule, string sPicSize)
        {
            List<EPGChannelProgrammeObject> retList = new List<EPGChannelProgrammeObject>();

            if (dtMultiChannelsSchedule != null && dtMultiChannelsSchedule.Rows.Count > 0)
            {
                Dictionary<string, List<EPGDictionary>> AllEPG_ResponseTag = GetAllEPGTagProgram(groupID, dtMultiChannelsSchedule);
                Dictionary<string, List<EPGDictionary>> AllEPG_ResponseMeta = GetAllEPGMetaProgram(groupID, dtMultiChannelsSchedule);
                foreach (DataRowView dr in dtMultiChannelsSchedule.DefaultView)
                {
                    EPGChannelProgrammeObject item = CreateEpgScheduleObject(dr, AllEPG_ResponseTag, AllEPG_ResponseMeta, sPicSize);
                    retList.Add(item);
                }
            }
            return retList;
        }

        private void InsertEPGTagValue(string value, int EPGTagTypeID)
        { //Insert new EPG Tags
            try
            {
                ODBCWrapper.InsertQuery insertTagProgQuery = new ODBCWrapper.InsertQuery("EPG_tags");
                insertTagProgQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", s_GroupID);
                insertTagProgQuery += ODBCWrapper.Parameter.NEW_PARAM("value", "=", value);
                insertTagProgQuery += ODBCWrapper.Parameter.NEW_PARAM("epg_tag_type_id", "=", EPGTagTypeID);
                insertTagProgQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                insertTagProgQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", UpdaterID);
                insertTagProgQuery.Execute();
                insertTagProgQuery.Finish();
                insertTagProgQuery = null;
            }
            catch (Exception exp)
            {
                log.Error("InsertEPGTagValue - " + string.Format("could not Insert EPG Tag Value Tag id {0} value '{1}' , error message: {2}", EPGTagTypeID, value, exp.Message), exp);
            }
        }

        private void InsertEPGProgramTag(int ProgramID, int tagid)
        {
            try
            {
                //Inset New EPG Tag Value               
                ODBCWrapper.InsertQuery insertProgTagProgQuery = new ODBCWrapper.InsertQuery("EPG_program_tags");
                insertProgTagProgQuery += ODBCWrapper.Parameter.NEW_PARAM("program_id", "=", ProgramID);
                insertProgTagProgQuery += ODBCWrapper.Parameter.NEW_PARAM("epg_tag_id", "=", tagid);
                insertProgTagProgQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", s_GroupID);
                insertProgTagProgQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                insertProgTagProgQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", UpdaterID);
                insertProgTagProgQuery.Execute();
                insertProgTagProgQuery.Finish();
                insertProgTagProgQuery = null;
            }
            catch (Exception exp)
            {
                log.Error("InsertEPGProgramTag - " + string.Format("could not Insert EPG Program Tag id {0} to program id {1} , error message: {2}", tagid, ProgramID, exp.Message), exp);
            }
        }

        private bool isExistProgramTag(int ProgramID, int tagid)
        {
            bool res = false;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += " select id from EPG_program_tags";
            selectQuery += "Where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("Group_ID", "=", s_GroupID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("program_id", "=", ProgramID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("epg_tag_id", "=", tagid);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);

            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    res = true;
                }
            }

            return res;
        }

        private Dictionary<string, List<EPGDictionary>> GetAllEPGTagProgram(int nGroupID, DataTable ProgramID)
        {
            Dictionary<string, List<EPGDictionary>> EPG_ResponseTag = new Dictionary<string, List<EPGDictionary>>();

            if (ProgramID != null && ProgramID.Rows.Count > 0)
            {
                string programIDSQL = ODBCWrapper.Utils.GetDelimitedStringFromDataTable(ProgramID, ",", "id", "in (", ")");  //convert program id to SQL statment               

                ODBCWrapper.DataSetSelectQuery selectTagsQuery = new ODBCWrapper.DataSetSelectQuery();
                selectTagsQuery += " select tt.name as Name, tv.value as Value, tp.program_id as program_id from epg_program_tags as tp inner join epg_tags as tv on tp.epg_tag_id = tv.id";
                selectTagsQuery += "inner join epg_tags_types as tt on tv.epg_tag_type_id = tt.id ";
                selectTagsQuery += "where tp.group_id ";
                selectTagsQuery += PageUtils.GetAllGroupTreeStr(nGroupID);
                selectTagsQuery += " and ";
                selectTagsQuery += string.Format("tp.program_id {0}", programIDSQL);
                selectTagsQuery += " and ";
                selectTagsQuery += "tp.status=1 and tv.status=1 and tt.status=1 and tt.is_active=1";

                if (selectTagsQuery.Execute("query", true) != null)
                {
                    int countMeta = selectTagsQuery.Table("query").DefaultView.Count;
                    if (countMeta > 0)
                    {
                        foreach (DataRowView drTag in selectTagsQuery.Table("query").DefaultView)
                        {
                            EPGDictionary tagItem = new EPGDictionary();
                            tagItem.Key = ODBCWrapper.Utils.GetSafeStr(drTag, "Name");
                            tagItem.Value = ODBCWrapper.Utils.GetSafeStr(drTag, "Value");
                            if (EPG_ResponseTag.ContainsKey(drTag["program_id"].ToString()))
                            {
                                List<EPGDictionary> temp = EPG_ResponseTag[drTag["program_id"].ToString()];
                                temp.Add(tagItem);
                                EPG_ResponseTag[drTag["program_id"].ToString()] = temp;
                            }
                            else
                            {
                                List<EPGDictionary> temp = new List<EPGDictionary>();
                                temp.Add(tagItem);
                                EPG_ResponseTag.Add(drTag["program_id"].ToString(), temp);
                            }

                        }
                    }
                }
                selectTagsQuery.Finish();
                selectTagsQuery = null;
            }
            return EPG_ResponseTag;
        }

        private Dictionary<string, List<EPGDictionary>> GetAllEPGMetaProgram(int nGroupID, DataTable ProgramID)
        {
            Dictionary<string, List<EPGDictionary>> EPG_ResponseMeta = new Dictionary<string, List<EPGDictionary>>();

            if (ProgramID != null && ProgramID.Rows.Count > 0)
            {
                string programIDSQL = ODBCWrapper.Utils.GetDelimitedStringFromDataTable(ProgramID, ",", "id", "in (", ")");  //convert program id to SQL statment

                ODBCWrapper.DataSetSelectQuery selectMetaQuery = new ODBCWrapper.DataSetSelectQuery();
                selectMetaQuery += " select mt.name as Name, mp.value as Value, mp.program_id as program_id  from epg_program_metas as mp inner join epg_metas_types as mt";
                selectMetaQuery += "on mt.id = mp.epg_meta_id ";
                selectMetaQuery += "where mp.group_id ";
                selectMetaQuery += PageUtils.GetAllGroupTreeStr(nGroupID);
                selectMetaQuery += " and ";
                selectMetaQuery += string.Format("mp.program_id {0}", programIDSQL);
                selectMetaQuery += " and ";
                selectMetaQuery += "mp.status=1 and mt.status=1 and mt.is_active=1";

                if (selectMetaQuery.Execute("query", true) != null)
                {
                    int countMeta = selectMetaQuery.Table("query").DefaultView.Count;
                    if (countMeta > 0)
                    {
                        foreach (DataRowView drMeta in selectMetaQuery.Table("query").DefaultView)
                        {
                            EPGDictionary metaItem = new EPGDictionary();
                            metaItem.Key = ODBCWrapper.Utils.GetSafeStr(drMeta, "Name");
                            metaItem.Value = ODBCWrapper.Utils.GetSafeStr(drMeta, "Value");

                            if (EPG_ResponseMeta.ContainsKey(drMeta["program_id"].ToString()))
                            {
                                List<EPGDictionary> temp = EPG_ResponseMeta[drMeta["program_id"].ToString()];
                                temp.Add(metaItem);
                                EPG_ResponseMeta[drMeta["program_id"].ToString()] = temp;
                            }
                            else
                            {
                                List<EPGDictionary> temp = new List<EPGDictionary>();
                                temp.Add(metaItem);
                                EPG_ResponseMeta.Add(drMeta["program_id"].ToString(), temp);
                            }
                        }
                    }
                }
                selectMetaQuery.Finish();
                selectMetaQuery = null;
            }
            return EPG_ResponseMeta;
        }


        private XmlDocument GetXmlDocFromFTPFile(string sFileName, NetworkCredential networkCredentials)
        {
            log.Debug("Start EPGImplementor.GetXmlDocFromFTPFile() - " + string.Format("File Name:{0}", sFileName));


            XmlDocument xmlDoc = new XmlDocument();
            FtpWebResponse responseDownload = null;

            try
            {
                //create instance FTPWebRequest to the specific path. 
                FtpWebRequest reqdownloadFTP = (FtpWebRequest)WebRequest.Create(new Uri(string.Format("{0}/{1}", s_Path, sFileName)));
                //Add Network Credential
                reqdownloadFTP.Credentials = networkCredentials;
                //specific the action method
                reqdownloadFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                reqdownloadFTP.UseBinary = true;
                //get response 
                responseDownload = (FtpWebResponse)reqdownloadFTP.GetResponse();
                //stream response                

                using (StreamReader sr = new StreamReader(responseDownload.GetResponseStream(), true))
                {
                    xmlDoc.Load(sr);
                }
            }
            catch (Exception exp)
            {
                log.Error("EPGImplementor.GetXmlDocFromFTPFile() Exception: - " + string.Format("there an error occurred during the Get FTP Stream file process, Stream file '{0}' , Error: {1}", sFileName, exp.Message), exp);
            }
            finally
            {
                if (responseDownload != null)
                {
                    responseDownload.Close();
                }
            }

            log.Debug("End EPGImplementor.GetXmlDocFromFTPFile() - " + string.Format("File Name:{0}", sFileName));
            return xmlDoc;
        }
        /// <summary>
        /// Move file to spasfice destination
        /// </summary>
        /// <param name="sFileName">set file name</param>
        /// <param name="sTargetDirectoryPath">Target Directory Path</param>
        /// <returns>return true if success else return false</returns>
        private bool MoveFile(string sFileName)
        {
            bool moveSucceeded = false;
            string sTargetDirectoryPath = GetMoveFilePath();
            FtpWebResponse response = null;
            Stream stream = null;
            FtpWebResponse Uploadresponse = null;
            int retryCount = 3;
            for (int i = 0; i < retryCount && !moveSucceeded; i++)
            {
                try
                {
                    // Get the object used to communicate with the server.
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(string.Format("{0}/{1}", sTargetDirectoryPath, sFileName)));
                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    request.Credentials = m_NetCredential;
                    stream = GetFTPStreamFile(sFileName, out response);
                    // Copy the contents of the file to the request stream.                
                    StreamReader sourceStream = new StreamReader(stream);
                    byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
                    sourceStream.Close();

                    request.ContentLength = fileContents.Length;

                    Stream requestStream = request.GetRequestStream();
                    requestStream.Write(fileContents, 0, fileContents.Length);
                    requestStream.Close();


                    Uploadresponse = (FtpWebResponse)request.GetResponse();

                    moveSucceeded = true;

                    log.Debug("EPG Move file - " + string.Format("Move source file '{0}' to directory path '{1}' success .! , response status: {2}", sFileName, sTargetDirectoryPath, Uploadresponse.StatusDescription));
                }
                catch (Exception exp)
                {
                    log.Error("EPG Move file - " + string.Format("there an error occurring during move file process, Move file '{0}' , Error: {1}", sFileName, exp.Message), exp);
                }
                finally
                {
                    if (response != null)
                    {
                        response.Close();
                        response = null;
                    }

                    if (stream != null)
                    {
                        stream.Close();
                        stream = null;
                    }

                    if (Uploadresponse != null)
                    {
                        Uploadresponse.Close();
                        Uploadresponse = null;
                    }

                    i++;
                }
            }
            return moveSucceeded;
        }
        /// <summary>
        /// Delete file from source 
        /// </summary>
        /// <param name="sFileName">set file name</param>
        /// <returns>retur true if success else return false</returns>
        private bool DeleteFileFromFtp(string sFileName)
        {
            bool res = false;
            FtpWebResponse response = null;
            try
            {
                FtpWebRequest reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(string.Format("{0}/{1}", s_Path, sFileName)));
                reqFTP.Credentials = m_NetCredential;
                reqFTP.Method = WebRequestMethods.Ftp.DeleteFile;
                response = (FtpWebResponse)reqFTP.GetResponse();
                res = true;
                log.Debug("EPG Delete file - " + string.Format("delete source file '{0}' success .! , response status: {1}", sFileName, response.StatusDescription));
            }
            catch (Exception exp)
            {
                if (response != null)
                {
                    response.Close();
                }
                res = false;
                log.Error("EPG_Delete - " + string.Format("there an error occurred during the delete process, delete file '{0}'  , Error: {1}", sFileName, exp.Message) + " EPG  - delete source file failed.", exp);
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
                reqdownloadFTP.Credentials = m_NetCredential;
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
                log.Error("EPG FTP Stream " + string.Format("there an error occurred during the Get FTP Stream file process, Stream file '{0}' , Error: {1}", sFileName, exp.Message), exp);
                response = null;
            }

            return stream;
        }
        private string GetMoveFilePath()
        {
            if (m_ProcessError)
            {
                return m_FailedPath;
            }
            else
            {
                return m_SuccessPath;
            }
        }

        #endregion

        #region Protected Methods
        //call Lucene and update the channel 
        protected void BuildEpgByChannel(Int32 channelID, DateTime minStartDate, DateTime maxStartDate)
        {
        }

        static protected string GetWSURL(string sUrl)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(sUrl);
        }
        #endregion


        protected void DeleteAllPrograms(Int32 channelID, IEnumerable<tvProgramme> prog)
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
                log.Debug("Delete Program on Date - " + string.Format("Group ID = {0}; Deleting Programs on Date {1} that belong to channel {2}", s_GroupID, progStartDate, channelID));
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


        protected void InsertEpgsDBBatches(ref Dictionary<string, EpgCB> epgDic, int groupID, int nCountPackage, List<FieldTypeEntity> FieldEntityMapping)
        {

            Dictionary<string, EpgCB> epgBatch = new Dictionary<string, EpgCB>();
            Dictionary<int, List<string>> tagsAndValues = new Dictionary<int, List<string>>();
            int nEpgCount = 0;
            try
            {
                foreach (string sGuid in epgDic.Keys)
                {
                    epgBatch.Add(sGuid, epgDic[sGuid]);
                    nEpgCount++;

                    //generate a Dictionary of all tag and values in the epg
                    GenerateTagsAndValues(epgDic[sGuid], FieldEntityMapping, ref tagsAndValues);

                    if (nEpgCount >= nCountPackage)
                    {
                        InsertEpgs(groupID, ref epgBatch, FieldEntityMapping, tagsAndValues);
                        nEpgCount = 0;
                        foreach (string guid in epgBatch.Keys)
                        {
                            if (epgBatch[guid].EpgID > 0)
                            {
                                epgDic[guid].EpgID = epgBatch[guid].EpgID;
                            }
                        }
                        epgBatch.Clear();
                        tagsAndValues.Clear();
                    }
                }

                if (nEpgCount > 0 && epgBatch.Keys.Count() > 0)
                {
                    InsertEpgs(groupID, ref epgBatch, FieldEntityMapping, tagsAndValues);
                    foreach (string guid in epgBatch.Keys)
                    {
                        if (epgBatch[guid].EpgID > 0)
                        {
                            epgDic[guid].EpgID = epgBatch[guid].EpgID;
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                log.Error("InsertEpgsDBBatches - " + string.Format("Exception in inserting EPGs in group: {0}. exception: {1} ", groupID, exc.Message), exc);
                return;
            }
        }

        //generate a Dictionary of all tag and values in the epg
        protected void GenerateTagsAndValues(EpgCB epg, List<FieldTypeEntity> FieldEntityMapping, ref  Dictionary<int, List<string>> tagsAndValues)
        {
            foreach (string tagType in epg.Tags.Keys)
            {
                string tagTypel = tagType.ToLower();
                int tagTypeID = 0;
                List<FieldTypeEntity> tagField = FieldEntityMapping.Where(x => x.FieldType == enums.FieldTypes.Tag && x.Name.ToLower() == tagTypel).ToList();
                if (tagField != null && tagField.Count > 0)
                {
                    tagTypeID = tagField[0].ID;
                }
                else
                {
                    log.Debug("UpdateExistingTagValuesPerEPG - " + string.Format("Missing tag Definition in FieldEntityMapping of tag:{0} in EPG:{1}", tagType, epg.EpgID));
                    continue;//missing tag definition in DB (in FieldEntityMapping)                        
                }

                if (!tagsAndValues.ContainsKey(tagTypeID))
                {
                    tagsAndValues.Add(tagTypeID, new List<string>());
                }
                foreach (string tagValue in epg.Tags[tagType])
                {
                    if (!tagsAndValues[tagTypeID].Contains(tagValue.ToLower()))
                        tagsAndValues[tagTypeID].Add(tagValue.ToLower());
                }
            }
        }

        //this FUnction inserts Epgs, thier Metas and tags to DB, and updates the EPGID in the EpgCB object according to the ID of the epg_channels_schedule in the DB
        protected void InsertEpgs(int nGroupID, ref Dictionary<string, EpgCB> epgDic, List<FieldTypeEntity> FieldEntityMapping, Dictionary<int, List<string>> tagsAndValues)
        {
            try
            {
                DataTable dtEpgMetas = InitEPGProgramMetaDataTable();
                DataTable dtEpgTags = InitEPGProgramTagsDataTable();
                DataTable dtEpgTagsValues = InitEPG_Tags_Values();

                int nUpdaterID = 0;
                if (!int.TryParse(UpdaterID, out nUpdaterID))
                    nUpdaterID = 700;
                string sConn = "MAIN_CONNECTION_STRING";

                List<FieldTypeEntity> FieldEntityMappingMetas = FieldEntityMapping.Where(x => x.FieldType == enums.FieldTypes.Meta).ToList();
                List<FieldTypeEntity> FieldEntityMappingTags = FieldEntityMapping.Where(x => x.FieldType == enums.FieldTypes.Tag).ToList();

                Dictionary<KeyValuePair<string, int>, List<string>> newTagValueEpgs = new Dictionary<KeyValuePair<string, int>, List<string>>();// new tag values and the EPGs that have them
                //    Dictionary<int, List<KeyValuePair<string, int>>> TagTypeIdWithValue = getTagTypeWithAllValues(nGroupID, FieldEntityMappingTags);  //all the tag types IDs and thier values that are in the DB (can be more than one) 

                Dictionary<int, List<KeyValuePair<string, int>>> TagTypeIdWithValue = getTagTypeWithRelevantValues(nGroupID, FieldEntityMappingTags, tagsAndValues);//return relevant tag value ID, if they exist in the DB

                InsertEPG_Channels_sched(ref epgDic);

                foreach (EpgCB epg in epgDic.Values)
                {
                    if (epg != null)
                    {
                        //update Metas
                        UpdateMetasPerEPG(ref dtEpgMetas, epg, FieldEntityMappingMetas, nUpdaterID);
                        //update Tags                    
                        UpdateExistingTagValuesPerEPG(epg, FieldEntityMappingTags, ref dtEpgTags, ref dtEpgTagsValues, TagTypeIdWithValue, ref newTagValueEpgs, nUpdaterID);
                    }
                }

                InsertNewTagValues(epgDic, dtEpgTagsValues, ref dtEpgTags, newTagValueEpgs, nGroupID, nUpdaterID);

                InsertBulk(dtEpgMetas, "EPG_program_metas", sConn); //insert EPG Metas to DB
                InsertBulk(dtEpgTags, "EPG_program_tags", sConn); //insert EPG Tags to DB
            }
            catch (Exception exc)
            {
                log.Error("InsertEpgs - " + string.Format("Exception in inserting EPGs in group: {0}. exception: {1} ", nGroupID, exc.Message), exc);
                return;
            }
        }

        //insert new tag values and update the tag value ID in tagValueWithID
        protected void InsertNewTagValues(Dictionary<string, EpgCB> epgDic, DataTable dtEpgTagsValues, ref DataTable dtEpgTags,
            Dictionary<KeyValuePair<string, int>, List<string>> newTagValueEpgs, int nGroupID, int nUpdaterID)
        {
            Dictionary<KeyValuePair<string, int>, int> tagValueWithID = new Dictionary<KeyValuePair<string, int>, int>();
            Dictionary<int, List<string>> dicTagTypeIDAndValues = new Dictionary<int, List<string>>();
            string sConn = "MAIN_CONNECTION_STRING";

            if (dtEpgTagsValues != null && dtEpgTagsValues.Rows != null && dtEpgTagsValues.Rows.Count > 0)
            {
                //insert all New tag values from dtEpgTagsValues to DB
                InsertBulk(dtEpgTagsValues, "EPG_tags", sConn);

                //retrun back all the IDs of the new Tags_Values
                for (int k = 0; k < dtEpgTagsValues.Rows.Count; k++)
                {
                    DataRow row = dtEpgTagsValues.Rows[k];
                    string sTagValue = ODBCWrapper.Utils.GetSafeStr(row, "value");
                    int nTagTypeID = ODBCWrapper.Utils.GetIntSafeVal(row, "epg_tag_type_id");
                    if (!dicTagTypeIDAndValues.Keys.Contains(nTagTypeID))
                    {
                        dicTagTypeIDAndValues.Add(nTagTypeID, new List<string>() { sTagValue });
                    }
                    else
                    {
                        dicTagTypeIDAndValues[nTagTypeID].Add(sTagValue);
                    }
                }

                DataTable dtTagValueID = EpgDal.Get_EPGTagValueIDs(nGroupID, dicTagTypeIDAndValues);

                //update the IDs in tagValueWithID
                if (dtTagValueID != null && dtTagValueID.Rows != null)
                {
                    for (int i = 0; i < dtTagValueID.Rows.Count; i++)
                    {
                        DataRow row = dtTagValueID.Rows[i];
                        if (row != null)
                        {
                            int nTagValueID = ODBCWrapper.Utils.GetIntSafeVal(row, "ID");
                            string sTagValue = ODBCWrapper.Utils.GetSafeStr(row, "VALUE");
                            int nTagType = ODBCWrapper.Utils.GetIntSafeVal(row, "epg_tag_type_id");

                            KeyValuePair<string, int> tagValueAndType = new KeyValuePair<string, int>(sTagValue, nTagType);
                            if (!tagValueWithID.Keys.Contains(tagValueAndType))
                            {
                                tagValueWithID.Add(tagValueAndType, nTagValueID);
                            }
                        }
                    }
                }
            }

            //go over all newTagValueEpgs and update the EPG_Program_Tags
            foreach (KeyValuePair<string, int> kvpUpdated in newTagValueEpgs.Keys)
            {
                int TagValueID = 0;
                List<KeyValuePair<string, int>> tempTagValue = tagValueWithID.Keys.Where(x => x.Key == kvpUpdated.Key && x.Value == kvpUpdated.Value).ToList();
                if (tempTagValue != null && tempTagValue.Count > 0)
                {
                    TagValueID = tagValueWithID[tempTagValue[0]];
                    if (TagValueID > 0)
                    {
                        foreach (string epgGUID in newTagValueEpgs[kvpUpdated])
                        {
                            EpgCB epgToUpdate = epgDic[epgGUID];
                            FillEpgExtraDataTable(ref dtEpgTags, false, "", epgToUpdate.EpgID, TagValueID, epgToUpdate.GroupID, epgToUpdate.Status, nUpdaterID, DateTime.UtcNow, DateTime.UtcNow);
                        }
                    }
                }
            }
        }




        protected Dictionary<int, List<KeyValuePair<string, int>>> getTagTypeWithRelevantValues(int nGroupID, List<FieldTypeEntity> FieldEntityMappingTags, Dictionary<int, List<string>> tagsAndValues)
        {
            Dictionary<int, List<KeyValuePair<string, int>>> dicTagTypeWithValues = new Dictionary<int, List<KeyValuePair<string, int>>>();//per tag type, thier values and IDs

            DataTable dtTagValueID = EpgDal.Get_EPGTagValueIDs(nGroupID, tagsAndValues);

            if (dtTagValueID != null && dtTagValueID.Rows != null)
            {
                for (int i = 0; i < dtTagValueID.Rows.Count; i++)
                {
                    DataRow row = dtTagValueID.Rows[i];
                    if (row != null)
                    {
                        int nTagTypeID = ODBCWrapper.Utils.GetIntSafeVal(row, "epg_tag_type_id");
                        string sValue = ODBCWrapper.Utils.GetSafeStr(row, "VALUE");
                        int nID = ODBCWrapper.Utils.GetIntSafeVal(row, "ID");
                        KeyValuePair<string, int> kvp = new KeyValuePair<string, int>(sValue, nID);
                        if (dicTagTypeWithValues.ContainsKey(nTagTypeID))
                        {
                            //check if the value exists already in the dictionary (maybe in UpperCase\LowerCase)
                            List<KeyValuePair<string, int>> resultList = new List<KeyValuePair<string, int>>();
                            resultList = dicTagTypeWithValues[nTagTypeID].Where(x => x.Key.ToLower() == sValue.ToLower() && x.Value == nID).ToList();
                            if (resultList.Count == 0)
                            {
                                dicTagTypeWithValues[nTagTypeID].Add(kvp);
                            }
                        }
                        else
                        {
                            List<KeyValuePair<string, int>> lValues = new List<KeyValuePair<string, int>>() { kvp };
                            dicTagTypeWithValues.Add(nTagTypeID, lValues);
                        }
                    }
                }
            }
            return dicTagTypeWithValues;
        }



        //protected Dictionary<int, List<KeyValuePair<string, int>>> getTagTypeWithAllValues(int nGroupID, List<FieldTypeEntity> FieldEntityMappingTags)
        //{
        //    List<int> lTagTypeIDs = new List<int>();           
        //    foreach (FieldTypeEntity field in FieldEntityMappingTags)
        //        lTagTypeIDs.Add(field.ID);
        //    Dictionary<int, List<KeyValuePair<string, int>>> dicTagTypeWithValues = new Dictionary<int, List<KeyValuePair<string, int>>>();//per tag type, thier values and IDs
        //    DataTable dtTagValues = EpgDal.Get_EPGAllValuesPerTagType(nGroupID, lTagTypeIDs);
        //    if (dtTagValues != null && dtTagValues.Rows != null)
        //    {
        //        for(int i=0; i < dtTagValues.Rows.Count; i++)
        //        {
        //            DataRow row = dtTagValues.Rows[i];
        //            if (row != null)
        //            {
        //                int nTagTypeID = ODBCWrapper.Utils.GetIntSafeVal(row, "epg_tag_type_id");
        //                string sValue = ODBCWrapper.Utils.GetSafeStr(row, "VALUE");
        //                int nID = ODBCWrapper.Utils.GetIntSafeVal(row, "ID");
        //                KeyValuePair<string, int> kvp = new KeyValuePair<string, int>(sValue, nID);
        //                if (dicTagTypeWithValues.ContainsKey(nTagTypeID))
        //                {
        //                    //check if the value exists already in the dictionary (maybe in UpperCase\LowerCase)
        //                    List <KeyValuePair <string, int>> resultList = new List<KeyValuePair<string,int>>();
        //                    resultList = dicTagTypeWithValues[nTagTypeID].Where(x => x.Key.ToLower() == sValue.ToLower() && x.Value == nID).ToList();
        //                    if (resultList.Count == 0)
        //                    {
        //                        dicTagTypeWithValues[nTagTypeID].Add(kvp);
        //                    }
        //                }
        //                else
        //                {                             
        //                    List <KeyValuePair<string,int>> lValues = new List<KeyValuePair<string,int>>() {kvp};                          
        //                    dicTagTypeWithValues.Add(nTagTypeID, lValues);
        //                }
        //            }
        //        }
        //    }
        //    return dicTagTypeWithValues;
        //}


        protected void UpdateExistingTagValuesPerEPG(EpgCB epg, List<FieldTypeEntity> FieldEntityMappingTags, ref DataTable dtEpgTags,
           ref DataTable dtEpgTagsValues, Dictionary<int, List<KeyValuePair<string, int>>> TagTypeIdWithValue, ref Dictionary<KeyValuePair<string, int>, List<string>> newTagValueEpgs, int nUpdaterID)
        {
            KeyValuePair<string, int> kvp = new KeyValuePair<string, int>();

            foreach (string sTagName in epg.Tags.Keys)
            {
                List<FieldTypeEntity> tagField = FieldEntityMappingTags.Where(x => x.Name == sTagName).ToList();//get the tag_type_ID
                int nTagTypeID = 0;

                if (tagField != null && tagField.Count > 0)
                {
                    nTagTypeID = tagField[0].ID;
                }
                else
                {
                    log.Debug("UpdateExistingTagValuesPerEPG - " + string.Format("Missing tag Definition in FieldEntityMapping of tag:{0} in EPG:{1}", sTagName, epg.EpgID));
                    continue;//missing tag definition in DB (in FieldEntityMapping)                        
                }

                foreach (string sTagValue in epg.Tags[sTagName])
                {
                    if (sTagValue != "")
                    {
                        kvp = new KeyValuePair<string, int>(sTagValue, nTagTypeID);

                        if (TagTypeIdWithValue.ContainsKey(nTagTypeID))
                        {
                            List<KeyValuePair<string, int>> list = TagTypeIdWithValue[nTagTypeID].Where(x => x.Key.ToLower() == sTagValue.ToLower()).ToList();
                            if (list != null && list.Count > 0)
                            {
                                //Insert New EPG Tag Value in EPG_Program_Tags, we are assuming this tag value was not assigned to the program because the program is new                                                    
                                FillEpgExtraDataTable(ref dtEpgTags, false, "", epg.EpgID, list[0].Value, epg.GroupID, epg.Status, nUpdaterID, DateTime.UtcNow, DateTime.UtcNow);
                            }
                            else//tha tag value does not exist in the DB
                            {
                                //the newTagValueEpgs has this tag + value: only need to update that this specific EPG is using it
                                if (newTagValueEpgs.Where(x => x.Key.Key == kvp.Key && x.Key.Value == kvp.Value).ToList().Count > 0)
                                {
                                    newTagValueEpgs[kvp].Add(epg.EpgIdentifier);
                                }
                                else //need to insert a new tag +value to the newTagValueEpgs and update the relevant table 
                                {
                                    FillEpgTagValueTable(ref dtEpgTagsValues, sTagValue, epg.EpgID, nTagTypeID, epg.GroupID, epg.Status, nUpdaterID, DateTime.UtcNow, DateTime.UtcNow);
                                    List<string> lEpgGUID = new List<string>() { epg.EpgIdentifier };
                                    newTagValueEpgs.Add(kvp, lEpgGUID);
                                }
                            }
                        }
                        else //this tag type does not have the relevant values in the DB, need to insert a new tag +value to the newTagValueEpgs and update the relevant table 
                        {
                            //check if it was not already added to the newTagValueEpgs
                            if (newTagValueEpgs.Where(x => x.Key.Key == kvp.Key && x.Key.Value == kvp.Value).ToList().Count == 0)
                            {
                                FillEpgTagValueTable(ref dtEpgTagsValues, sTagValue, epg.EpgID, nTagTypeID, epg.GroupID, epg.Status, nUpdaterID, DateTime.UtcNow, DateTime.UtcNow);
                                List<string> lEpgGUID = new List<string>() { epg.EpgIdentifier };
                                newTagValueEpgs.Add(kvp, lEpgGUID);
                            }
                            else ////the newTagValueEpgs has this tag + value: only need to update that this  specific EPG is using it
                            {
                                newTagValueEpgs[kvp].Add(epg.EpgIdentifier);
                            }
                        }
                    }
                    tagField = null;
                }
            }
        }

        protected void UpdateMetasPerEPG(ref DataTable dtEpgMetas, EpgCB epg, List<FieldTypeEntity> FieldEntityMappingMetas, int nUpdaterID)
        {
            List<FieldTypeEntity> metaField = new List<FieldTypeEntity>();
            foreach (string sMetaName in epg.Metas.Keys)
            {
                metaField = FieldEntityMappingMetas.Where(x => x.Name == sMetaName).ToList();
                int nID = 0;
                if (metaField != null && metaField.Count > 0)
                {
                    nID = metaField[0].ID;
                    if (epg.Metas[sMetaName].Count > 0)
                    {
                        string sValue = epg.Metas[sMetaName][0];
                        FillEpgExtraDataTable(ref dtEpgMetas, true, sValue, epg.EpgID, nID, epg.GroupID, epg.Status, nUpdaterID, DateTime.UtcNow, DateTime.UtcNow);
                    }
                }
                else
                {   //missing meta definition in DB (in FieldEntityMapping)
                    log.Debug("UpdateMetasPerEPG - " + string.Format("Missing Meta Definition in FieldEntityMapping of Meta:{0} in EPG:{1}", sMetaName, epg.EpgID));
                }
                metaField = null;
            }
        }

        protected void InsertEPG_Channels_sched(ref Dictionary<string, EpgCB> epgDic)
        {
            EpgCB epg;
            Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();

            DataTable dtEPG = InitEPGDataTable();
            FillEPGDataTable(epgDic, ref dtEPG);
            string sConn = "MAIN_CONNECTION_STRING";
            InsertBulk(dtEPG, "epg_channels_schedule", sConn); //insert EPGs to DB

            //get back the IDs list of the EPGs          
            DataTable dtEpgIDGUID = EpgDal.Get_EpgIDbyEPGIdentifier(epgDic.Keys.ToList());
            if (dtEpgIDGUID != null && dtEpgIDGUID.Rows != null)
            {
                for (int i = 0; i < dtEpgIDGUID.Rows.Count; i++)
                {
                    DataRow row = dtEpgIDGUID.Rows[i];
                    if (row != null)
                    {
                        string sGuid = ODBCWrapper.Utils.GetSafeStr(row, "EPG_IDENTIFIER");
                        ulong nEPG_ID = (ulong)ODBCWrapper.Utils.GetIntSafeVal(row, "ID");
                        if (epgDic.TryGetValue(sGuid, out epg) && epg != null)
                            epgDic[sGuid].EpgID = nEPG_ID;  //update the EPGCB with the ID
                    }
                }
            }
        }

        //Insert rows of table to the db at once using bulk operation.      
        protected void InsertBulk(DataTable dt, string sTableName, string sConnName)
        {
            if (dt != null)
            {
                ODBCWrapper.InsertQuery insertMessagesBulk = new ODBCWrapper.InsertQuery();
                insertMessagesBulk.SetConnectionKey(sConnName);
                try
                {
                    insertMessagesBulk.InsertBulk(sTableName, dt);
                }
                catch (Exception ex)
                {
                    log.Error("", ex);
                }
                finally
                {
                    if (insertMessagesBulk != null)
                    {
                        insertMessagesBulk.Finish();
                    }
                    insertMessagesBulk = null;
                }
            }
        }

        private DataTable InitEPGDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("EPG_CHANNEL_ID", typeof(long));
            dt.Columns.Add("EPG_IDENTIFIER", typeof(string));
            dt.Columns.Add("NAME", typeof(string));
            dt.Columns.Add("DESCRIPTION", typeof(string));
            dt.Columns.Add("START_DATE", typeof(DateTime));
            dt.Columns.Add("END_DATE", typeof(DateTime));
            dt.Columns.Add("PIC_ID", typeof(long));
            dt.Columns.Add("STATUS", typeof(int));
            dt.Columns.Add("IS_ACTIVE", typeof(int));
            dt.Columns.Add("GROUP_ID", typeof(long));
            dt.Columns.Add("UPDATER_ID", typeof(long));
            dt.Columns.Add("UPDATE_DATE", typeof(DateTime));
            dt.Columns.Add("PUBLISH_DATE", typeof(DateTime));
            dt.Columns.Add("CREATE_DATE", typeof(DateTime));
            dt.Columns.Add("EPG_TAG", typeof(string));
            dt.Columns.Add("media_id", typeof(long));
            dt.Columns.Add("FB_OBJECT_ID", typeof(string));
            dt.Columns.Add("like_counter", typeof(long));
            return dt;
        }

        private DataTable InitEPGProgramMetaDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("value", typeof(string));
            dt.Columns.Add("epg_meta_id", typeof(int));
            dt.Columns.Add("program_id", typeof(int));
            dt.Columns.Add("group_id", typeof(int));
            dt.Columns.Add("status", typeof(int));
            dt.Columns.Add("updater_id", typeof(int));
            dt.Columns.Add("create_date", typeof(DateTime));
            dt.Columns.Add("update_date", typeof(DateTime));
            return dt;
        }

        private DataTable InitEPGProgramTagsDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("program_id", typeof(int));
            dt.Columns.Add("epg_tag_id", typeof(int));
            dt.Columns.Add("group_id", typeof(int));
            dt.Columns.Add("status", typeof(int));
            dt.Columns.Add("updater_id", typeof(int));
            dt.Columns.Add("create_date", typeof(DateTime));
            dt.Columns.Add("update_date", typeof(DateTime));
            return dt;
        }

        private DataTable InitEPG_Tags_Values()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("value", typeof(string));
            dt.Columns.Add("epg_tag_type_id", typeof(string));
            dt.Columns.Add("group_id", typeof(int));
            dt.Columns.Add("status", typeof(int));
            dt.Columns.Add("updater_id", typeof(int));
            dt.Columns.Add("create_date", typeof(DateTime));
            dt.Columns.Add("update_date", typeof(DateTime));
            return dt;
        }

        protected void FillEPGDataTable(Dictionary<string, EpgCB> epgDic, ref DataTable dtEPG)
        {
            if (epgDic != null && epgDic.Count > 0)
            {
                foreach (EpgCB epg in epgDic.Values)
                {
                    if (epg != null)
                    {
                        DataRow row = dtEPG.NewRow();
                        row["EPG_CHANNEL_ID"] = epg.ChannelID;
                        row["EPG_IDENTIFIER"] = epg.EpgIdentifier;
                        row["NAME"] = epg.Name;
                        if (epg.Description.Length >= MaxDescriptionSize)
                            row["DESCRIPTION"] = epg.Description.Substring(0, MaxDescriptionSize); //insert only 1024 chars (limitation of the column in the DB)
                        else
                            row["DESCRIPTION"] = epg.Description;
                        row["START_DATE"] = epg.StartDate;
                        row["END_DATE"] = epg.EndDate;
                        row["PIC_ID"] = epg.PicID;
                        row["STATUS"] = epg.Status;
                        row["IS_ACTIVE"] = epg.isActive;
                        row["GROUP_ID"] = epg.GroupID;
                        row["UPDATER_ID"] = 400;
                        row["UPDATE_DATE"] = epg.UpdateDate;
                        row["PUBLISH_DATE"] = DateTime.UtcNow;
                        row["CREATE_DATE"] = epg.CreateDate;
                        row["EPG_TAG"] = null;
                        row["media_id"] = epg.ExtraData.MediaID;
                        row["FB_OBJECT_ID"] = epg.ExtraData.FBObjectID;
                        row["like_counter"] = epg.Statistics.Likes;
                        dtEPG.Rows.Add(row);
                    }
                }
            }

        }

        protected void FillEpgExtraDataTable(ref DataTable dtEPGExtra, bool bIsMeta, string sValue, ulong nProgID, int nID, int nGroupID, int nStatus,
            int nUpdaterID, DateTime dCreateTime, DateTime dUpdateTime)
        {
            DataRow row = dtEPGExtra.NewRow();
            if (bIsMeta)
            {
                row["value"] = sValue;
                row["epg_meta_id"] = nID;
            }
            else
            {
                row["epg_tag_id"] = nID;
            }

            row["program_id"] = nProgID;
            row["group_id"] = nGroupID;
            row["status"] = nStatus;
            row["updater_id"] = nUpdaterID;
            row["create_date"] = dCreateTime;
            row["update_date"] = dUpdateTime;
            dtEPGExtra.Rows.Add(row);
        }

        protected void FillEpgTagValueTable(ref DataTable dtEPGTagValue, string sValue, ulong nProgID, int nTagTypeID, int nGroupID, int nStatus,
           int nUpdaterID, DateTime dCreateTime, DateTime dUpdateTime)
        {
            DataRow row = dtEPGTagValue.NewRow();
            row["value"] = sValue;
            row["epg_tag_type_id"] = nTagTypeID;
            row["group_id"] = nGroupID;
            row["status"] = nStatus;
            row["updater_id"] = nUpdaterID;
            row["create_date"] = dCreateTime;
            row["update_date"] = dUpdateTime;
            dtEPGTagValue.Rows.Add(row);
        }

    }
}
