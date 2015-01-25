using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using ODBCWrapper;
using ApiObjects.Epg;

namespace Tvinci.Core.DAL
{
    public class EpgDal : BaseDal
    {
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
        public static DataSet GetEpgMappingFields(List<int> lSubTree, int groupID, int channelID= 0)
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

        // get all channels by group id from DB
        //the returned dictionary contains keys of the 'CHANNEL_ID' column, and per 'CHANNEL_ID' - the DB ID and the channel name.
        public static Dictionary<string, EpgChannelObj> GetAllEpgChannelsDic(int nGroupID)
        {
            Dictionary<string, EpgChannelObj> result = new Dictionary<string, EpgChannelObj>();
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

                       // KeyValuePair<int, string> kvp = new KeyValuePair<int, string>(nID, name);
                        if (!result.ContainsKey(channelId))
                        {
                            result.Add(channelId, oEpgChannelObj);
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                return new Dictionary<string, EpgChannelObj>();
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

       
    }
}
