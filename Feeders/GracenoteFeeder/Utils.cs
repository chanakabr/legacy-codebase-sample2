using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using ApiObjects;
using ElasticSearch.Common;
using ElasticSearch.Searcher;
using EnumProject;
using EpgBL;
using GroupsCacheManager;
using Tvinci.Core.DAL;
using TvinciImporter;
using System.IO.Compression;
using KLogMonitor;
using System.Reflection;

namespace GracenoteFeeder
{
    public static class Utils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public static readonly int MaxDescriptionSize = 1024;
        public static readonly int MaxNameSize = 255;


        /*Build the FieldTypeEntity Mapping for each Tag / Meta with it's xml mapping */
        public static List<FieldTypeEntity> GetMappingFields(int nGroupID, int channelID, EpgChannelType eEpgChannelType)
        {
            try
            {
                List<FieldTypeEntity> AllFieldTypeMapping = new List<FieldTypeEntity>();
                List<FieldTypeEntity> AllFieldType = new List<FieldTypeEntity>();
                GroupManager groupManager = new GroupManager();
                List<int> lSubTree = new List<int>();
                lSubTree = groupManager.GetSubGroup(nGroupID);

                DataSet ds = EpgDal.GetEpgMappingFields(lSubTree, nGroupID, channelID);

                if (ds != null && ds.Tables != null && ds.Tables.Count >= 4)
                {
                    if (ds.Tables.Count == 5 && ds.Tables[4] != null && ds.Tables[4].Rows != null && ds.Tables[4].Rows.Count > 0)// the channel type 
                    {
                        int nEpgChannelType = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[4].Rows[0], "epg_channel_type");
                        eEpgChannelType = (EpgChannelType)nEpgChannelType;
                    }
                    if (ds.Tables[0] != null) // basic
                    {
                        InitializeMappingFields(ds.Tables[0], ds.Tables[3], enums.FieldTypes.Basic, ref AllFieldTypeMapping, eEpgChannelType);
                    }
                    if (ds.Tables[1] != null) // metas
                    {
                        InitializeMappingFields(ds.Tables[1], ds.Tables[3], enums.FieldTypes.Meta, ref AllFieldTypeMapping, eEpgChannelType);
                    }
                    if (ds.Tables[2] != null) // Tags
                    {
                        InitializeMappingFields(ds.Tables[2], ds.Tables[3], enums.FieldTypes.Tag, ref AllFieldTypeMapping, eEpgChannelType);
                    }

                }

                return AllFieldTypeMapping;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return new List<FieldTypeEntity>();
            }
        }

        //delete all programs by dateTime        
        public static void DeleteProgramsByChannelAndDate(Int32 channelID, DateTime dProgStartDate, int nParentGroupID)
        {
            DateTime dProgEndDate = dProgStartDate.AddDays(1).AddMilliseconds(-1);

            #region Delete all existing programs in CB that have start/end dates within the new schedule
            BaseEpgBL oEpgBL = EpgBL.Utils.GetInstance(nParentGroupID);
            List<DateTime> lDates = new List<DateTime>() { dProgStartDate };

            log.Debug("Delete Program on Date - " + string.Format("ParentGroup ID = {0}; Deleting Programs  that belong to channel {1}", nParentGroupID, channelID));

            oEpgBL.RemoveGroupPrograms(lDates, channelID);
            #endregion

            #region Delete all existing programs in DB that have start/end dates within the new schedule

            DeleteScheduleProgramByDate(channelID, dProgStartDate);

            #endregion

            #region Delete all existing programs in ES that have start/end dates within the new schedule
            bool resDelete = DeleteEPGDocFromES(nParentGroupID, channelID, lDates);
            #endregion

        }

        internal static bool DeleteEPGDocFromES(int parentGroupID, int channelID, List<DateTime> lDates)
        {
            bool resDelete = false;
            try
            {
                ElasticSearchApi oESApi = new ElasticSearchApi();

                string sQuery = BuildDeleteQuery(channelID, lDates);
                string sIndex = string.Format("{0}_{1}", parentGroupID.ToString(), "epg");
                resDelete = oESApi.DeleteDocsByQuery(sIndex, "epg", ref sQuery);
                return resDelete;
            }
            catch (Exception ex)
            {
                log.Error("DeleteDocFromES - " + string.Format("channelID = {0},ex = {1}", channelID, ex.Message), ex);
                return false;
            }
        }

        /*Build query by channelId and spesipic dates*/
        private static string BuildDeleteQuery(int channelID, List<DateTime> lDates)
        {
            string sQuery = string.Empty;

            ESTerm epgChannelTerm = new ESTerm(true) { Key = "epg_channel_id", Value = channelID.ToString() };

            BoolQuery oBoolQuery = new BoolQuery();


            BoolQuery oBoolQueryDates = new BoolQuery();
            foreach (DateTime date in lDates)
            {
                string sMaxtDate = date.AddDays(1).AddMilliseconds(-1).ToString("yyyyMMddHHmmss");

                ESRange startDateRange = new ESRange(false);

                startDateRange.Key = "start_date";
                string sMin = date.ToString("yyyyMMddHHmmss");
                startDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, sMin));
                startDateRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, sMaxtDate));

                oBoolQueryDates.AddChild(startDateRange, ApiObjects.SearchObjects.CutWith.OR);
            }

            oBoolQuery.AddChild(epgChannelTerm, ApiObjects.SearchObjects.CutWith.AND); // channel must be equel to channelID
            oBoolQuery.AddChild(oBoolQueryDates, ApiObjects.SearchObjects.CutWith.AND);// and start date must be in lDates list (with or between dates)

            sQuery = oBoolQuery.ToString();


            return sQuery;

        }


        private static void DeleteScheduleProgramByDate(int channelID, DateTime date)
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

        // initialize each item with all external_ref  
        private static void InitializeMappingFields(DataTable dataTable, DataTable dataTableRef, enums.FieldTypes fieldTypes, ref List<FieldTypeEntity> AllFieldTypeMapping, EpgChannelType eEpgChannelType)
        {
            FieldTypeEntity item;

            foreach (DataRow dr in dataTable.Rows)
            {
                switch (fieldTypes)
                {
                    case enums.FieldTypes.Basic:
                        item = new FieldTypeEntity();
                        item.FieldType = fieldTypes;
                        item.ID = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
                        item.Name = ODBCWrapper.Utils.GetSafeStr(dr, "Name");
                        AllFieldTypeMapping.Add(item);
                        break;
                    case enums.FieldTypes.Meta:
                        item = InitializationMeta(dr, eEpgChannelType, dataTableRef);
                        AllFieldTypeMapping.Add(item);
                        break;
                    case enums.FieldTypes.Tag:
                        InitializationTags(dr, eEpgChannelType, dataTableRef, ref AllFieldTypeMapping);
                        break;
                    default:
                        break;
                }
            }
        }

        private static void InitializationTags(DataRow dr, EpgChannelType eEpgChannelType, DataTable dataTableRef, ref List<FieldTypeEntity> AllFieldTypeMapping)
        {
            bool bNewTag = false;
            int ID = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
            string Name = ODBCWrapper.Utils.GetSafeStr(dr, "Name");
            FieldTypeEntity item = AllFieldTypeMapping.FirstOrDefault(i => i.ID == ID && i.Name == Name);

            if (item == null) // new tag 
            {
                item = new FieldTypeEntity();
                item.ID = ID;
                item.Name = Name;
                item.FieldType = enums.FieldTypes.Tag;
                bNewTag = true;
            }

            string sDefaultVal = ODBCWrapper.Utils.GetSafeStr(dr, "DefaultValue"); // for tag or metas 
            // if this is a DTT Tag + DTT epg channel 
            if (eEpgChannelType == EpgChannelType.DTT)
            {
                int nTagTypeFlag = ODBCWrapper.Utils.GetIntSafeVal(dr, "tag_type_flag");
                TagTypeFlag eTagTypeFlag = (TagTypeFlag)nTagTypeFlag;
                if (eTagTypeFlag == TagTypeFlag.TimeShifted)
                {
                    sDefaultVal = "NO";
                }
            }
            item.Value.Add(sDefaultVal);

            foreach (var x in dataTableRef.Select("type = " + (int)enums.FieldTypes.Tag + " and field_id = " + item.ID))
            {

                if (item.XmlReffName == null)
                {
                    item.XmlReffName = new List<string>();
                    item.XmlReffName.Add(ODBCWrapper.Utils.GetSafeStr(x, "external_ref"));
                }
                else
                {
                    item.XmlReffName.Add(ODBCWrapper.Utils.GetSafeStr(x, "external_ref"));
                }
            }
            if (bNewTag)
            {
                AllFieldTypeMapping.Add(item);
            }
        }

        private static FieldTypeEntity InitializationMeta(DataRow dr, EpgChannelType eEpgChannelType, DataTable dataTableRef)
        {
            FieldTypeEntity item = new FieldTypeEntity();
            item.ID = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
            item.Name = ODBCWrapper.Utils.GetSafeStr(dr, "Name");
            item.FieldType = enums.FieldTypes.Meta;

            string sDefaultVal = ODBCWrapper.Utils.GetSafeStr(dr, "DefaultValue"); // for tag or metas 
            // if this is a DTT Tag + DTT epg channel 
            if (eEpgChannelType == EpgChannelType.DTT)
            {
                int nMetaTypeFlag = ODBCWrapper.Utils.GetIntSafeVal(dr, "meta_type_flag");
                TagTypeFlag eTagTypeFlag = (TagTypeFlag)nMetaTypeFlag;
                if (eTagTypeFlag == TagTypeFlag.TimeShifted)
                {
                    sDefaultVal = "NO";
                }
            }
            item.Value.Add(sDefaultVal);

            foreach (var x in dataTableRef.Select("type = " + (int)enums.FieldTypes.Meta + " and field_id = " + item.ID))
            {

                if (item.XmlReffName == null)
                {
                    item.XmlReffName = new List<string>();
                    item.XmlReffName.Add(ODBCWrapper.Utils.GetSafeStr(x, "external_ref"));
                }
                else
                {
                    item.XmlReffName.Add(ODBCWrapper.Utils.GetSafeStr(x, "external_ref"));
                }
            }
            return item;
        }

        public static bool ParseEPGStrToDate(string dateStr, ref DateTime theDate)
        {
            if (string.IsNullOrEmpty(dateStr) || dateStr.Length < 14)
                return false;

            string format = "yyyy-MM-ddTHH:mm";
            bool res = DateTime.TryParseExact(dateStr, format, null, System.Globalization.DateTimeStyles.None, out theDate);
            return res;
        }

        public static DateTime ParseEPGStrToDate(string date, string programtime)
        {
            DateTime dt = new DateTime();
            try
            {
                int year = int.Parse(date.Substring(0, 4));
                int month = int.Parse(date.Substring(5, 2));
                int day = int.Parse(date.Substring(8, 2));
                int hour = int.Parse(programtime.Substring(0, 2));
                int min = int.Parse(programtime.Substring(2, 2));
                int sec = int.Parse(programtime.Substring(4, 2));
                dt = new DateTime(year, month, day, hour, min, sec);
            }
            catch (Exception exp)
            {
                log.Error("", exp);
            }
            return dt;
        }

        /*create EpgCB object by all the values from XML*/
        public static EpgCB generateEPGCB(string epg_url, string description, string name, int channelID, string EPGGuid, DateTime dProgStartDate,
            DateTime dProgEndDate, XmlNode progItem, int groupID, int parentGroupID, List<FieldTypeEntity> lFieldTypeEntity)
        {
            EpgCB newEpgItem = new EpgCB();
            try
            {
                log.Debug("generateEPGCB - " + string.Format("EpgIdentifier '{0}' ", EPGGuid));

                newEpgItem.ChannelID = channelID;
                newEpgItem.Name = string.Format("{0}", name);
                newEpgItem.Description = string.Format("{0} ", description);
                newEpgItem.GroupID = groupID;
                newEpgItem.ParentGroupID = parentGroupID;
                newEpgItem.EpgIdentifier = EPGGuid;
                newEpgItem.StartDate = dProgStartDate;
                newEpgItem.EndDate = dProgEndDate;
                newEpgItem.UpdateDate = DateTime.UtcNow;
                newEpgItem.CreateDate = DateTime.UtcNow;
                newEpgItem.isActive = true;
                newEpgItem.Status = 1;

                newEpgItem.Metas = Utils.GetEpgProgramMetas(lFieldTypeEntity);
                // When We stop insert to DB , we still need to insert new tags to DB !!!!!!!
                newEpgItem.Tags = Utils.GetEpgProgramTags(lFieldTypeEntity);

                #region Update Image ID
                if (!string.IsNullOrEmpty(epg_url))
                {
                    string strName = name.Replace("\r", "").Replace("\n", "");
                    if (strName.Length > MaxNameSize)
                    {
                        strName = strName.Substring(0, MaxNameSize);
                    }
                    int nPicID = ImporterImpl.DownloadEPGPic(epg_url, strName, groupID, 0, channelID);
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


        private static Dictionary<string, List<string>> GetEpgProgramMetas(List<FieldTypeEntity> FieldEntityMapping)
        {
            Dictionary<string, List<string>> dMetas = new Dictionary<string, List<string>>();

            var MetaFieldEntity = from item in FieldEntityMapping
                                  where item.FieldType == enums.FieldTypes.Meta && item.XmlReffName.Capacity > 0 && item.Value != null && item.Value.Count > 0
                                  select item;

            foreach (var item in MetaFieldEntity)
            {
                string itemNamel = item.Name.ToLower();
                foreach (var value in item.Value)
                {
                    if (dMetas.ContainsKey(itemNamel))
                    {
                        dMetas[itemNamel].AddRange(item.Value);
                    }
                    else
                    {
                        dMetas.Add(itemNamel, item.Value);
                    }
                }
            }
            return dMetas;
        }

        private static Dictionary<string, List<string>> GetEpgProgramTags(List<FieldTypeEntity> FieldEntityMapping)
        {
            Dictionary<string, List<string>> dTags = new Dictionary<string, List<string>>();
            var TagFieldEntity = from item in FieldEntityMapping
                                 where item.FieldType == enums.FieldTypes.Tag && item.XmlReffName.Capacity > 0 && item.Value != null && item.Value.Count > 0
                                 select item;


            foreach (var item in TagFieldEntity)
            {
                string itemNamel = item.Name.ToLower();
                if (dTags.ContainsKey(itemNamel))
                {
                    dTags[itemNamel].AddRange(item.Value);
                }
                else
                {
                    dTags.Add(itemNamel, item.Value);
                }

            }
            return dTags;
        }

        public static void UpdateExistingTagValuesPerEPG(EpgCB epg, List<FieldTypeEntity> FieldEntityMappingTags, ref DataTable dtEpgTags,
       ref DataTable dtEpgTagsValues, Dictionary<int, List<KeyValuePair<string, int>>> TagTypeIdWithValue, ref Dictionary<KeyValuePair<string, int>, List<string>> newTagValueEpgs, int nUpdaterID)
        {
            KeyValuePair<string, int> kvp = new KeyValuePair<string, int>();

            foreach (string sTagName in epg.Tags.Keys)
            {
                List<FieldTypeEntity> tagField = FieldEntityMappingTags.Where(x => x.Name.ToLower() == sTagName).ToList();//get the tag_type_ID
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

        private static void FillEpgExtraDataTable(ref DataTable dtEPGExtra, bool bIsMeta, string sValue, ulong nProgID, int nID, int nGroupID, int nStatus,
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


        private static void FillEpgTagValueTable(ref DataTable dtEPGTagValue, string sValue, ulong nProgID, int nTagTypeID, int nGroupID, int nStatus,
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

        public static void UpdateMetasPerEPG(ref DataTable dtEpgMetas, EpgCB epg, List<FieldTypeEntity> FieldEntityMappingMetas, int nUpdaterID)
        {
            List<FieldTypeEntity> metaField = new List<FieldTypeEntity>();
            foreach (string sMetaName in epg.Metas.Keys)
            {
                metaField = FieldEntityMappingMetas.Where(x => x.Name.ToLower() == sMetaName).ToList();
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


        //insert new tag values and update the tag value ID in tagValueWithID
        public static void InsertNewTagValues(Dictionary<string, EpgCB> epgDic, DataTable dtEpgTagsValues, ref DataTable dtEpgTags,
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

        //Insert rows of table to the db at once using bulk operation.      
        public static void InsertBulk(DataTable dt, string sTableName, string sConnName)
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

        public static void FillEPGDataTable(Dictionary<string, EpgCB> epgDic, ref DataTable dtEPG)
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

                        epg.Name = epg.Name.Replace("\r", "").Replace("\n", "");
                        if (epg.Name.Length >= MaxNameSize)
                            row["NAME"] = epg.Name.Substring(0, MaxNameSize); //insert only 255 chars (limitation of the column in the DB)
                        else
                            row["NAME"] = epg.Name;
                        epg.Description = epg.Description.Replace("\r", "").Replace("\n", "");
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


        public static DataTable InitEPGDataTable()
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

        public static Dictionary<int, List<KeyValuePair<string, int>>> getTagTypeWithRelevantValues(int nGroupID, List<FieldTypeEntity> FieldEntityMappingTags, Dictionary<int, List<string>> tagsAndValues)
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


        public static DataTable InitEPGProgramMetaDataTable()
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

        public static DataTable InitEPGProgramTagsDataTable()
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

        public static DataTable InitEPG_Tags_Values()
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

        //generate a Dictionary of all tag and values in the epg
        public static void GenerateTagsAndValues(EpgCB epg, List<FieldTypeEntity> FieldEntityMapping, ref  Dictionary<int, List<string>> tagsAndValues)
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

        public static bool UpdateEpgIndex(List<ulong> epgIDs, int nGroupID, ApiObjects.eAction action)
        {
            bool result = false;
            try
            {
                result = ImporterImpl.UpdateEpg(epgIDs, nGroupID, action);
                return result;
            }
            catch (Exception ex)
            {
                log.Error("EpgFeeder - " + string.Format("failed update EpgIndex ex={0}", ex.Message), ex);
                return false;
            }
        }


        // request gracenote api and get xml as a response
        public static string getXmlFromGracenote(string postData, string method, string uri)// 251545958-935BCC7F5502DCD265D67EBF29B2EB2B
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(uri);//"https://c11031808.ipg.web.cddbp.net/webapi/xml/1.0/tvgridbatch_update");
                GetProxyConfig(request);

                if (method == "POST")
                {
                    var data = Encoding.ASCII.GetBytes(postData);
                    request.Method = "POST";

                    request.ContentLength = data.Length;
                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }
                else
                {
                    request.Method = "GET";
                }
                request.ContentType = "application/x-www-form-urlencoded";


                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();


                return responseString.ToString();
            }
            catch (Exception ex)
            {
                log.Error("getXmlFromGracenote - " + string.Format("ex={0}, uri={1}", ex.Message, uri), ex);
                return string.Empty;
            }
        }

        private static void GetProxyConfig(HttpWebRequest request)
        {
            try
            {
                // add proxy configuration - for KDG internat connection
                WebProxy myProxy = new WebProxy();
                string proxyAddress = TVinciShared.WS_Utils.GetTcmConfigValue("proxyAddressKDG");
                string username = TVinciShared.WS_Utils.GetTcmConfigValue("proxyUsernameKDG");
                string password = TVinciShared.WS_Utils.GetTcmConfigValue("proxyPasswordKDG");
                if (!string.IsNullOrEmpty(proxyAddress))
                {
                    myProxy.Address = new Uri(proxyAddress);
                    myProxy.Credentials = new NetworkCredential(username, password);
                    request.Proxy = myProxy;
                }
            }
            catch (Exception ex)
            {
                log.Error("GetProxyConfig - " + string.Format("fail to initialize proxy details ex={0}", ex.Message), ex);
            }
        }

        public static string Compress(string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs);
                }
                return Convert.ToBase64String(mso.ToArray());
            }
        }


        public static DataTable InitEPGDataTableWithID()
        {
            DataTable dt = new DataTable();
            // Add three column objects to the table. 
            DataColumn ID = new DataColumn();
            ID.DataType = typeof(long);
            ID.ColumnName = "ID";
            ID.Unique = true;
            dt.Columns.Add(ID);
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

        public static void FillEPGDataTable(Dictionary<string, EpgCB> epgDic, ref DataTable dtEPG, DateTime dPublishDate)
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

                        epg.Name = epg.Name.Replace("\r", "").Replace("\n", "");
                        if (epg.Name.Length >= MaxNameSize)
                            row["NAME"] = epg.Name.Substring(0, MaxNameSize); //insert only 255 chars (limitation of the column in the DB)
                        else
                            row["NAME"] = epg.Name;
                        epg.Description = epg.Description.Replace("\r", "").Replace("\n", "");
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
                        row["PUBLISH_DATE"] = dPublishDate;
                        row["CREATE_DATE"] = epg.CreateDate;
                        row["EPG_TAG"] = null;
                        row["media_id"] = epg.ExtraData.MediaID;
                        row["FB_OBJECT_ID"] = epg.ExtraData.FBObjectID;
                        row["like_counter"] = epg.Statistics.Likes;

                        if (row.Table.Columns.Contains("ID") && epg.EpgID > 0)
                        {
                            row["ID"] = epg.EpgID;
                        }
                        dtEPG.Rows.Add(row);
                    }
                }
            }

        }
    }
}
