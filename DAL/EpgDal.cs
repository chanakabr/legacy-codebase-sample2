using ApiObjects.Epg;
using KLogMonitor;
using ODBCWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Tvinci.Core.DAL
{
    public class EpgDal : BaseDal
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static DataTable GetEpgScheduleDataTable(int? topRowsNumber, int groupID, DateTime? fromUTCDay, DateTime? toUTCDay, int epgChannelID)
        {
            StoredProcedure spGetEpgSchedule = new StoredProcedure("Get_EpgChannelsSchedule");
            spGetEpgSchedule.SetConnectionKey("CONNECTION_STRING");

            spGetEpgSchedule.AddParameter("@groupID", groupID);

            if (topRowsNumber.HasValue == true)
            {
                spGetEpgSchedule.AddParameter("@topRowsNumber", topRowsNumber);

                if (fromUTCDay.HasValue == true)
                {
                    spGetEpgSchedule.AddParameter("@endDate", DBNull.Value);
                    spGetEpgSchedule.AddParameter("@startDate", fromUTCDay);
                }
                else if (toUTCDay.HasValue == true)
                {
                    spGetEpgSchedule.AddParameter("@startDate", DBNull.Value);
                    spGetEpgSchedule.AddParameter("@endDate", toUTCDay);
                }
            }
            else
            {
                spGetEpgSchedule.AddParameter("@startDate", fromUTCDay);
                spGetEpgSchedule.AddParameter("@endDate", toUTCDay);
            }

            spGetEpgSchedule.AddParameter("@epgChannelID", epgChannelID);

            DataSet ds = spGetEpgSchedule.ExecuteDataSet();

            if (ds != null)
            {
                return ds.Tables[0];
            }
            return null;
        }

        public static int DeleteProgramsOnDate(DateTime dStartDate, string sGroupID, int nEPGChannelID)
        {
            StoredProcedure spDeleteProgramsOnDate = new StoredProcedure("sp_DeleteEPGProgramsOnDate");
            spDeleteProgramsOnDate.SetConnectionKey("CONNECTION_STRING");
            spDeleteProgramsOnDate.AddParameter("@GroupID", sGroupID);
            spDeleteProgramsOnDate.AddParameter("@EpgChannelID", nEPGChannelID);
            spDeleteProgramsOnDate.AddParameter("@startDate", dStartDate);
            spDeleteProgramsOnDate.AddParameter("@endDate", dStartDate.AddDays(1));

            int retVal = spDeleteProgramsOnDate.ExecuteReturnValue<int>();

            return retVal;
        }

        public static List<int> GetEpgProgramsByChannelIds(int groupID, List<int> epgChannelIDs, DateTime fromUTCDay, DateTime toUTCDay)
        {
            List<int> epgIds = new List<int>();
            StoredProcedure spGetEpgSchedule = new StoredProcedure("Get_EpgProgramsByChannelIds");
            spGetEpgSchedule.SetConnectionKey("CONNECTION_STRING");

            spGetEpgSchedule.AddParameter("@groupID", groupID);
            spGetEpgSchedule.AddParameter("@startDate", fromUTCDay);
            spGetEpgSchedule.AddParameter("@endDate", toUTCDay);
            spGetEpgSchedule.AddIDListParameter("@epgChannelIDs", epgChannelIDs, "Id");

            DataSet ds = spGetEpgSchedule.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
            {

                foreach (DataRow dr in ds.Tables[0].Rows )
                {
                    int id = ODBCWrapper.Utils.GetIntSafeVal(dr, "id");
                    epgIds.Add(id);
                }
                return epgIds;
            }
            return null;
        }
        public static DataTable GetEpgMultiScheduleDataTable(int? topRowsNumber, int groupID, DateTime? fromUTCDay, DateTime? toUTCDay, string[] sEPGChannelIDs)
        {
            StoredProcedure spGetEpgSchedule = new StoredProcedure("Get_EpgMultiChannelsSchedule");
            spGetEpgSchedule.SetConnectionKey("CONNECTION_STRING");

            spGetEpgSchedule.AddParameter("@groupID", groupID);

            if (topRowsNumber.HasValue == true)
            {
                spGetEpgSchedule.AddParameter("@topRowsNumber", topRowsNumber);

                if (fromUTCDay.HasValue == true)
                {
                    spGetEpgSchedule.AddParameter("@endDate", DBNull.Value);
                    spGetEpgSchedule.AddParameter("@startDate", fromUTCDay);
                }
                else if (toUTCDay.HasValue == true)
                {
                    spGetEpgSchedule.AddParameter("@startDate", DBNull.Value);
                    spGetEpgSchedule.AddParameter("@endDate", toUTCDay);
                }
            }
            else
            {
                spGetEpgSchedule.AddParameter("@startDate", fromUTCDay);
                spGetEpgSchedule.AddParameter("@endDate", toUTCDay);
            }

            spGetEpgSchedule.AddIDListParameter("@epgChannelIDs", sEPGChannelIDs.Select(x => int.Parse(x)).ToList(), "Id");

            DataSet ds = spGetEpgSchedule.ExecuteDataSet();

            if (ds != null)
            {
                return ds.Tables[0];
            }
            return null;
        }

        public static DataTable GetEpgProgramInfo(int nGroupID, int nProgramID)
        {
            StoredProcedure spGetEpgProgramInfo = new StoredProcedure("Get_EpgProgramInfo");
            spGetEpgProgramInfo.SetConnectionKey("CONNECTION_STRING");
            spGetEpgProgramInfo.AddParameter("@groupID", nGroupID);
            spGetEpgProgramInfo.AddParameter("@programID", nProgramID);

            DataSet ds = spGetEpgProgramInfo.ExecuteDataSet();

            if (ds != null)
            {
                return ds.Tables[0];
            }

            return null;
        }

        public static DataTable GetEpgProgramTags(int nGroupID, int nProgID)
        {
            StoredProcedure spGetEpgProgramTags = new StoredProcedure("Get_EpgProgramTags");
            spGetEpgProgramTags.SetConnectionKey("CONNECTION_STRING");
            spGetEpgProgramTags.AddParameter("@groupID", nGroupID);
            spGetEpgProgramTags.AddParameter("@programID", nProgID);

            DataSet ds = spGetEpgProgramTags.ExecuteDataSet();

            if (ds != null)
            {
                return ds.Tables[0];
            }

            return null;
        }

        public static DataSet GetEpgProgramDetails(int nGroupID, int nProgramID)
        {
            StoredProcedure sp = new StoredProcedure("Get_EpgProgramDetails");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@groupID", nGroupID);
            sp.AddParameter("@programID", nProgramID);

            DataSet ds = sp.ExecuteDataSet();
            return ds;
        }


        public static DataSet Get_GroupsTagsAndMetas(int nGroupID, List<int> lSubGroupTree, int nIsSearchable = 1)
        {
            StoredProcedure GroupMedias = new StoredProcedure("Get_GroupsTagsAndMetas");
            GroupMedias.SetConnectionKey("MAIN_CONNECTION_STRING");
            GroupMedias.AddParameter("@GroupID", nGroupID);
            GroupMedias.AddIDListParameter<int>("@SubGroupTree", lSubGroupTree, "Id");
            GroupMedias.AddParameter("@IsSearchable", nIsSearchable);


            DataSet ds = GroupMedias.ExecuteDataSet();
            return ds;
        }

        public static DataSet Get_EpgPrograms(int nGroupID, DateTime? dDateTime, int nEpgID)
        {
            StoredProcedure GroupEpgs = new StoredProcedure("Get_GroupEpgs");
            GroupEpgs.SetConnectionKey("MAIN_CONNECTION_STRING");
            GroupEpgs.AddParameter("@GroupID", nGroupID);
            GroupEpgs.AddParameter("@EpgID", nEpgID);
            GroupEpgs.AddParameter("@StartDate", dDateTime);

            DataSet ds = GroupEpgs.ExecuteDataSet();

            return ds;
        }
        
        public static DataTable GetParentGroupIdByGroupId(int nGroupId)
        {
            DataTable data = null;
            StoredProcedure spGetParentGroupId = new StoredProcedure("Get_groupsParentID");

            spGetParentGroupId.SetConnectionKey("CONNECTION_STRING");
            spGetParentGroupId.AddParameter("@groupID", nGroupId);

            DataSet ds = spGetParentGroupId.ExecuteDataSet();

            if (ds != null)
            {
                return ds.Tables[0];
            }

            return data;
        }

        public static DataTable Get_EpgIDbyEPGIdentifier(List<string> EPGIdentifiers)
        {
            StoredProcedure spGetEpgIDbyEPGIdentifier = new StoredProcedure("Get_EpgIDbyEPGIdentifier");
            spGetEpgIDbyEPGIdentifier.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGetEpgIDbyEPGIdentifier.AddIDListParameter<string>("@EPGIdentifiers", EPGIdentifiers, "Id");

            DataSet ds = spGetEpgIDbyEPGIdentifier.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }
        public static DataTable Get_EpgIDbyEPGIdentifier(List<string> EPGIdentifiers, int channelID)
        {
            StoredProcedure spGetEpgIDbyEPGIdentifier = new StoredProcedure("Get_EpgIDbyEPGIdentifier");
            spGetEpgIDbyEPGIdentifier.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGetEpgIDbyEPGIdentifier.AddIDListParameter<string>("@EPGIdentifiers", EPGIdentifiers, "Id");
            spGetEpgIDbyEPGIdentifier.AddParameter("@channelID", channelID);

            DataSet ds = spGetEpgIDbyEPGIdentifier.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }


        public static DataTable Get_EPGTagValueIDs(int nGroupID, Dictionary<int, List<string>> lTagTypeAndValues)
        {
            StoredProcedure spGetEPGTagValueID = new StoredProcedure("Get_EPGTagValueIDs");
            spGetEPGTagValueID.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGetEPGTagValueID.AddKeyValueListParameter<int, string>("@TagTypeAndValue", lTagTypeAndValues, "key", "value");
            spGetEPGTagValueID.AddParameter("@GroupID", nGroupID);
            DataSet ds = spGetEPGTagValueID.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_EPGAllValuesPerTagType(int nGroupID, List<int> lTagTypes)
        {
            StoredProcedure spGetEPGAllValues = new StoredProcedure("Get_EPGAllValuesPerTagType");
            spGetEPGAllValues.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGetEPGAllValues.AddIDListParameter<int>("@TagTypes", lTagTypes, "Id");
            spGetEPGAllValues.AddParameter("@GroupID", nGroupID);
            DataSet ds = spGetEPGAllValues.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }


        /*Get all metas and tags for EPGs by groupID And it's mapping in the xml file*/
        public static DataSet GetEpgMappingFields(List<int> lSubTree, int groupID, int channelID = 0)
        {
            StoredProcedure sp = new StoredProcedure("GetEpgMappingFields");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddIDListParameter<int>("@SubGroupTree", lSubTree, "Id");
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@channelID", channelID);

            DataSet ds = sp.ExecuteDataSet();
            return ds;
        }

        /*return channel id by CHANNEL_ID (external gracenote channel id)*/
        public static int GetChannelByChannelID(int groupID, string sChannelID)
        {
            StoredProcedure sp = new StoredProcedure("Get_ChannelByChannelID");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@ChannelID", sChannelID);

            int retVal = sp.ExecuteReturnValue<int>();

            return retVal;
        }
        /*Return all channel list by group_id*/
        public static DataTable GetAllEpgChannelsList(int GroupID)
        {
            StoredProcedure sp = new StoredProcedure("Get_AllEpgChannelsList");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", GroupID);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        public static DataTable GetAllEpgChannelsList(int GroupID, List<string> channelExternalIds)
        {
            StoredProcedure sp = new StoredProcedure("Get_AllEpgChannelsList");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", GroupID);
            sp.AddIDListParameter<string>("@channelExternalIds", channelExternalIds, "STR");

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }


        public static DataTable Get_EPGTagValueTranslateIDs(int nGroupID, DataTable tagsAndValuesTranslate)
        {
            StoredProcedure spGetEPGTagValueID = new StoredProcedure("Get_EPGTagValueTranslteIDs");
            spGetEPGTagValueID.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGetEPGTagValueID.AddDataTableParameter("@Tags", tagsAndValuesTranslate);
            spGetEPGTagValueID.AddParameter("@GroupID", nGroupID);
            DataSet ds = spGetEPGTagValueID.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        // get all channels by group id from DB
        //the returned dictionary contains keys of the 'CHANNEL_ID' column, and per 'CHANNEL_ID' - the DB ID and the channel name.
        public static Dictionary<string, List<EpgChannelObj>> GetAllEpgChannelsDic(int nGroupID)
        {
            Dictionary<string, List<EpgChannelObj>> result = new Dictionary<string, List<EpgChannelObj>>();
            try
            {
                DataTable dt = GetAllEpgChannelsList(nGroupID);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string channelId = ODBCWrapper.Utils.GetSafeStr(row, "CHANNEL_ID").Replace("\r", "").Replace("\n", "");
                        string name = ODBCWrapper.Utils.GetSafeStr(row, "NAME");
                        int nID = ODBCWrapper.Utils.GetIntSafeVal(row, "ID");
                        int nChannelType = ODBCWrapper.Utils.GetIntSafeVal(row, "epg_channel_type");
                        EpgChannelObj oEpgChannelObj = new EpgChannelObj(nID, name, nChannelType);

                        if (!result.ContainsKey(channelId))
                        {
                            result.Add(channelId, new List<EpgChannelObj>() { oEpgChannelObj });
                        }
                        else
                        {
                            result[channelId].Add(oEpgChannelObj);
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return new Dictionary<string, List<EpgChannelObj>>();
            }
        }


        // get all channels by group id from DB
        //the returned dictionary contains keys of the 'CHANNEL_ID' column, and per 'CHANNEL_ID' - the DB ID and the channel name.
        public static Dictionary<string, List<EpgChannelObj>> GetAllEpgChannelsDic(int nGroupID, List<string> channelExternalIds)
        {
            Dictionary<string, List<EpgChannelObj>> result = new Dictionary<string, List<EpgChannelObj>>();
            try
            {
                DataTable dt = GetAllEpgChannelsList(nGroupID, channelExternalIds);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string channelId = ODBCWrapper.Utils.GetSafeStr(row, "CHANNEL_ID").Replace("\r", "").Replace("\n", "");
                        string name = ODBCWrapper.Utils.GetSafeStr(row, "NAME");
                        int nID = ODBCWrapper.Utils.GetIntSafeVal(row, "ID");
                        int nChannelType = ODBCWrapper.Utils.GetIntSafeVal(row, "epg_channel_type");
                        EpgChannelObj oEpgChannelObj = new EpgChannelObj(nID, name, nChannelType);

                        if (!result.ContainsKey(channelId))
                        {
                            result.Add(channelId, new List<EpgChannelObj>() { oEpgChannelObj });
                        }
                        else
                        {
                            result[channelId].Add(oEpgChannelObj);
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return new Dictionary<string, List<EpgChannelObj>>();
            }
        }

        public static int InsertNewChannel(int GroupID, string ChannelID, string Name)
        {
            StoredProcedure sp = new StoredProcedure("InsertNewChannel");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", GroupID);
            sp.AddParameter("@ChannelID", ChannelID);
            sp.AddParameter("@Name", Name);

            int retVal = sp.ExecuteReturnValue<int>();

            return retVal;
        }

        public static int UpdateEpgChannel(int GroupID, string channelID, int ID)
        {
            StoredProcedure sp = new StoredProcedure("UpdateEpgChannel");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", GroupID);
            sp.AddParameter("@ChannelID", channelID);
            sp.AddParameter("@ID", ID);


            int retVal = sp.ExecuteReturnValue<int>();
            return retVal;
        }

        public static int GetExistMedia(int GroupID, int EPG_IDENTIFIER)
        {
            StoredProcedure sp = new StoredProcedure("GetExistMedia");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", GroupID);
            sp.AddParameter("@EpgIdentifier", EPG_IDENTIFIER);

            int retVal = sp.ExecuteReturnValue<int>();
            return retVal;
        }

        public static DataTable Get_parental_rating()
        {
            StoredProcedure sp = new StoredProcedure("Get_parental_rating");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        public static DataTable EpgGuidExsits(List<string> keyCollection, int groupID, int channelID)
        {
            StoredProcedure sp = new StoredProcedure("EpgGuidExsits");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddIDListParameter<string>("@keyCollection", keyCollection, "Id");
            sp.AddParameter("@groupID", groupID);
            sp.AddParameter("@channelID", channelID);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        public static bool UpdateEpgChannelSchedulePublishDate(List<int> epgIDs, DateTime dPublishDate)
        {
            StoredProcedure sp = new StoredProcedure("UpdateEpgChannelSchedulePublishDate");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddIDListParameter<int>("@epgIDs", epgIDs, "Id");
            sp.AddParameter("@PublishDate", dPublishDate);

            bool retVal = sp.ExecuteReturnValue<bool>();
            return retVal;
        }

        public static bool UpdateEpgChannelSchedule(DataTable dtEPG)
        {
            StoredProcedure sp = new StoredProcedure("UpdateEpgChannelSchedule");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddDataTableParameter("@dt", dtEPG);

            bool retVal = sp.ExecuteReturnValue<bool>();
            return retVal;
        }

        public static bool DeleteEpgProgramDetails(List<int> epgIDs, int nGroupID)
        {
            StoredProcedure sp = new StoredProcedure("DeleteEpgProgramDetails");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddIDListParameter<int>("@epgIDs", epgIDs, "Id");
            sp.AddParameter("@groupID", nGroupID);

            bool retVal = sp.ExecuteReturnValue<bool>();
            return retVal;
        }

        public static bool DeleteEpgProgramDetails(List<int> epgIDs, List<int> protectedMetas)
        {
            StoredProcedure sp = new StoredProcedure("Delete_Epg_Program_Details");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddIDListParameter<int>("@epgIDs", epgIDs, "Id");            
            sp.AddIDListParameter<int>("@protectedMetas", protectedMetas, "Id");

            bool retVal = sp.ExecuteReturnValue<bool>();
            return retVal;
        }

        public static DataTable DeleteEpgs(int channelID, int groupID, DateTime dPublishDate, List<DateTime> deletedDays)
        {
            StoredProcedure sp = new StoredProcedure("DeleteEpgs");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");

            sp.AddParameter("@channelID", channelID);
            sp.AddParameter("@groupID", groupID);
            sp.AddParameter("@dPublishDate", dPublishDate);
            sp.AddIDListParameter<DateTime>("@deletedDays", deletedDays, "Id");

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        public static bool InsertNewEPGMultiPic(string epgIdentifier, int picID, int ratioID, int groupID, int channelID)
        {
            StoredProcedure sp = new StoredProcedure("InsertNewEPGMultiPic");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@epg_identifier", epgIdentifier);
            sp.AddParameter("@pic_id", picID);
            sp.AddParameter("@channel_id", channelID);
            sp.AddParameter("@ratio_id", ratioID);
            sp.AddParameter("@groupID", groupID);

            bool result = sp.ExecuteReturnValue<bool>();
            return result;
        }
        public static DataTable GetDateEpgImageDetails(string sPicDescription, int groupID)
        {
            StoredProcedure sp = new StoredProcedure("GetDateEpgImageDetails");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@description", sPicDescription);
            sp.AddParameter("@groupID", groupID);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        public static DataTable GetEpgMultiPictures(string epgIdentifier, int groupID, int channelID)
        {
            StoredProcedure sp = new StoredProcedure("GetEpgMultiPictures");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@epgIdentifier", epgIdentifier);
            sp.AddParameter("@channelID", channelID);
            sp.AddParameter("@groupID", groupID);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        public static Dictionary<string, string> Get_PicsEpgRatios()
        {
            Dictionary<string, string> ratios = new Dictionary<string, string>();
            StoredProcedure sp = new StoredProcedure("Get_PicsEpgRatios");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                string key = string.Empty;
                string value = string.Empty;
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    key = ODBCWrapper.Utils.GetSafeStr(dr, "id");
                    value = ODBCWrapper.Utils.GetSafeStr(dr, "ratio");

                    ratios.Add(key, value);
                }
            }

            return ratios;
        }

        public static bool DeleteEpgProgramPicturess(List<int> epgIDs, int groupID, int channelID)
        {
            StoredProcedure sp = new StoredProcedure("Delete_EpgProgramPictures");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddIDListParameter<int>("@epgIDs", epgIDs, "Id");
            sp.AddParameter("@groupID", groupID);
            sp.AddParameter("@channelID", channelID);

            bool retVal = sp.ExecuteReturnValue<bool>();
            return retVal;
        }

        public static DataSet Get_Group_EPGTagsAndMetas(int groupId, List<int> subGroupTree, int isSearchable = 1)
        {
            StoredProcedure storedProcedured = new StoredProcedure("Get_Group_EPGTagsAndMetas");
            storedProcedured.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedured.AddParameter("@GroupID", groupId);
            storedProcedured.AddIDListParameter<int>("@SubGroupTree", subGroupTree, "Id");
            storedProcedured.AddParameter("@IsSearchable", isSearchable);

            DataSet dataSet = storedProcedured.ExecuteDataSet();
            return dataSet;
        }

        public static bool UpdateEPGMultiPic(int groupId, string epgIdentifier, int channelID, int picID, int ratioID, int? updaterId)
        {
            bool res = false;
            try
            {
                StoredProcedure sp = new StoredProcedure("Set_EpgMultiPictures");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@groupID", groupId);
                sp.AddParameter("@epgIdentifier", epgIdentifier);
                sp.AddParameter("@channelId", channelID);
                sp.AddParameter("@picId", picID);
                sp.AddParameter("@ratioId", ratioID);
                if (updaterId.HasValue)
                    sp.AddParameter("@UpdaterID", updaterId.Value);

                res = sp.ExecuteReturnValue<bool>();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed UpdateEPGMultiPic, ex {0}", ex);
            }
            return res;
        }

        public static DataSet Get_EpgProgramsDetailsByChannelIds(int groupId, int epgChannelID, DateTime fromDate, DateTime toDate)
        {
            StoredProcedure storedProcedured = new StoredProcedure("Get_EpgProgramsDetailsByChannelIds");
            storedProcedured.SetConnectionKey("MAIN_CONNECTION_STRING");
            storedProcedured.AddParameter("@groupID", groupId);
            storedProcedured.AddIDListParameter<int>("@epgChannelIDs", new List<int>() { epgChannelID }, "Id");
            storedProcedured.AddParameter("@startDate", fromDate);
            storedProcedured.AddParameter("@endDate", toDate);

            DataSet dataSet = storedProcedured.ExecuteDataSet();
            return dataSet;
        }

        public static List<long> GetEpgIds(int epgChannelID, int groupId, DateTime fromDate, DateTime toDate, int? status = null)
        {
            List<long> list = null;
            try
            {
                
                StoredProcedure sp = new StoredProcedure("Get_EpgIds");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");

                sp.AddParameter("@epgChannelID", epgChannelID);
                sp.AddParameter("@groupID", groupId);
                sp.AddParameter("@fromDate", fromDate);
                sp.AddParameter("@toDate", toDate);
                if (status != null)
                {
                    sp.AddParameter("@status", status);
                }
                DataSet ds = sp.ExecuteDataSet();
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    list = ds.Tables[0].AsEnumerable().Select(x => (long)(x["id"])).ToList();
                }
            }
            catch 
            {
                return null;
            }
            return list;
        }

        public static DataTable GetProtectedEpgMetas(List<int> epgIds, List<int> protectedMetas)
        {
            StoredProcedure sp = new StoredProcedure("GetProtectedEpgMetas");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddIDListParameter<int>("@epgIDs", epgIds, "Id");            
            sp.AddIDListParameter<int>("@protectedMetas", protectedMetas, "Id");

            return sp.Execute();
        }
    }
}
