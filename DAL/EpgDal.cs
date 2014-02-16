using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Tvinci.Core.DAL
{
    public class EpgDal : BaseDal
    {   
        public static DataTable GetEpgScheduleDataTable(int? topRowsNumber, int groupID, DateTime? fromUTCDay, DateTime? toUTCDay, int epgChannelID)
        {
            ODBCWrapper.StoredProcedure spGetEpgSchedule = new ODBCWrapper.StoredProcedure("Get_EpgChannelsSchedule");
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
            ODBCWrapper.StoredProcedure spDeleteProgramsOnDate = new ODBCWrapper.StoredProcedure("sp_DeleteEPGProgramsOnDate");
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
            ODBCWrapper.StoredProcedure spGetEpgSchedule = new ODBCWrapper.StoredProcedure("Get_EpgMultiChannelsSchedule");
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
            ODBCWrapper.StoredProcedure spGetEpgProgramInfo = new ODBCWrapper.StoredProcedure("Get_EpgProgramInfo");
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
            ODBCWrapper.StoredProcedure spGetEpgProgramTags = new ODBCWrapper.StoredProcedure("Get_EpgProgramTags");
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
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_EpgProgramDetails");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@groupID", nGroupID);
            sp.AddParameter("@programID", nProgramID);

            DataSet ds = sp.ExecuteDataSet();
            return ds;
        }

        public static DataSet Get_GroupsTagsAndMetas(int nGroupID)
        {
            ODBCWrapper.StoredProcedure GroupMedias = new ODBCWrapper.StoredProcedure("Get_GroupsTagsAndMetas");
            GroupMedias.SetConnectionKey("MAIN_CONNECTION_STRING");
            GroupMedias.AddParameter("@GroupID", nGroupID);

            DataSet ds = GroupMedias.ExecuteDataSet();
            return ds;
        }

        public static DataSet Get_EpgPrograms(int nGroupID, DateTime? dDateTime, int nEpgID)
        {
            ODBCWrapper.StoredProcedure GroupEpgs = new ODBCWrapper.StoredProcedure("Get_GroupEpgs");
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
            ODBCWrapper.StoredProcedure spGetParentGroupId = new ODBCWrapper.StoredProcedure("Get_groupsParentID");

            spGetParentGroupId.SetConnectionKey("CONNECTION_STRING");
            spGetParentGroupId.AddParameter("@groupID", nGroupId);

            DataSet ds = spGetParentGroupId.ExecuteDataSet();

            if (ds != null)
            {
                return ds.Tables[0];
            }

            return data;
        }
    }
}
