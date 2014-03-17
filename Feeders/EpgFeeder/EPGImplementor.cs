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
        #endregion

        #region Public Members
        public string m_ParentGroupId { get; set; }
        public string s_GroupID;
        public string s_PathType;
        public string s_Path;
        public Dictionary<string, string> s_ExtraParamter;

        #endregion

        #region Private Members

        private NetworkCredential m_NetCredential;
        private string m_SuccessPath;
        private string m_FailedPath;
        private bool m_ProcessError = false;
        private string UpdaterID = "700";
        #endregion


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
                        item.ID = ODBCWrapper.Utils.GetIntSafeVal(dr,"ID");
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
                        item.ID = ODBCWrapper.Utils.GetIntSafeVal(dr , "ID");
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
                Logger.Logger.Log("InsertEPGProgramMetaValue", string.Format("could not Insert EPG Program Meta Value '{0}' epg_meta_id '{1}' , error message: {2}", value, EPGMetaID, exp.Message), LogFileName);
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
                Logger.Logger.Log("EPGImplementor.ProcessFilesFromFtpFolder(), Before get response from ftp", string.Format("userName:{0},password:{1},successPath:{2},failedPath:{3}", userName, password, successPath, failedPath), LogFileName);

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



                    if (!string.IsNullOrEmpty(filename))
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
                            Logger.Logger.Log("EPGImplementor.ProcessFilesFromFtpFolder() Exception:", string.Format("there an error occurring during the processing  EPG File '{0}', Error : {1}", filename, ex.Message), LogFileName);
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

                Logger.Logger.Log("EPGImplementor.ProcessFilesFromFtpFolder() ", "After Processing files form ftp", LogFileName);
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

                Logger.Logger.Log("EPG_LoadFiles_From_FTP", string.Format("there an error occurred during the Load Files process,  Error: {0}", exp.Message), LogFileName);

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
                Logger.Logger.Log("EpgFeeder", string.Format("failed update EpgIndex ex={0}", ex.Message), "EpgFeeder");
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
                Logger.Logger.Log("InsertEPGTagValue", string.Format("could not Insert EPG Tag Value Tag id {0} value '{1}' , error message: {2}", EPGTagTypeID, value, exp.Message), LogFileName);
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
                Logger.Logger.Log("InsertEPGProgramTag", string.Format("could not Insert EPG Program Tag id {0} to program id {1} , error message: {2}", tagid, ProgramID, exp.Message), LogFileName);
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
            Logger.Logger.Log("Start EPGImplementor.GetXmlDocFromFTPFile()", string.Format("File Name:{0}", sFileName), LogFileName);


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
                Logger.Logger.Log("EPGImplementor.GetXmlDocFromFTPFile() Exception:", string.Format("there an error occurred during the Get FTP Stream file process, Stream file '{0}' , Error: {1}", sFileName, exp.Message), LogFileName);
            }
            finally
            {
                if (responseDownload != null)
                {
                    responseDownload.Close();
                }
            }

            Logger.Logger.Log("End EPGImplementor.GetXmlDocFromFTPFile()", string.Format("File Name:{0}", sFileName), LogFileName);
            return xmlDoc;
        }
        /// <summary>
        /// Move file to spasfice destination
        /// </summary>
        /// <param name="sFileName">set file name</param>
        /// <param name="sTargetDirectoryPath">Target Directory Path</param>
        /// <returns>retur true if success else return false</returns>
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
                request.Credentials = m_NetCredential;
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

                Logger.Logger.Log("EPG Move file", string.Format("Move source file '{0}' to directory path '{1}' success .! , response status: {2}", sFileName, sTargetDirectoryPath, Uploadresponse.StatusDescription), LogFileName);
            }
            catch (Exception exp)
            {
                if (response != null)
                    response.Close();

                if (stream != null)
                    stream.Close();

                if (Uploadresponse != null)
                    Uploadresponse.Close();

                Logger.Logger.Log("EPG Move file", string.Format("there an error occurring during move file process, Move file '{0}' , Error: {1}", sFileName, exp.Message), LogFileName, "EPG Moves source file faild.");
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
                Logger.Logger.Log("EPG Delete file", string.Format("delete source file '{0}' success .! , response status: {1}", sFileName, response.StatusDescription), LogFileName);
            }
            catch (Exception exp)
            {
                if (response != null)
                {
                    response.Close();
                }
                res = false;
                Logger.Logger.Log("EPG_Delete", string.Format("there an error occurred during the delete process, delete file '{0}'  , Error: {1}", sFileName, exp.Message), LogFileName, "EPG  - delete source file faild.");
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
                Logger.Logger.Log("EPG FTP Stream ", string.Format("there an error occurred during the Get FTP Stream file process, Stream file '{0}' , Error: {1}", sFileName, exp.Message), LogFileName, "EPG - Get stream file failed.");
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
            //try
            //{
            //    Lucene.LuceneServiceClient service = new Lucene.LuceneServiceClient();
            //    string sWSURL = GetWSURL("LuceneWCF");
            //    if (sWSURL != "")
            //        service.Endpoint.Address = new System.ServiceModel.EndpointAddress(sWSURL);
            //    service.BuildEpgByChannel(ODBCWrapper.Utils.GetIntSafeVal(s_GroupID), channelID, minStartDate, maxStartDate);
            //}
            //catch (Exception ex)
            //{
            //    string msg = string.Format("faild to update lucene ex={0} , channelID={1}, Between dates {2}-{3}", ex.Message, channelID, minStartDate, maxStartDate);
            //    Logger.Logger.Log("BuildEpgByChannel", msg, "EpgFeeder");
            //}
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

            #region Delete all existing programs in ES that have start/end dates within the new schedule
            bool resDelete = Utils.DeleteEPGDocFromES(m_ParentGroupId, channelID, lDates);
            #endregion
        }      
    }
}
