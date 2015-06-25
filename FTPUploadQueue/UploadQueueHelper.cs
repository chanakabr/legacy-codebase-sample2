using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;

namespace UploadQueue
{
    public class UploadQueueHelper
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        static public int AddJobToQueue(int nGroupID, string sFileName)
        {
            int nJobID = 0;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select top 1 id from ftp_upload_queue (nolock) where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("file_name", "=", sFileName);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("upload_runtime_status", "=", 2);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("fail_count", "<", 4);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("upload_status", "=", 0);
            selectQuery += "order by create_date desc";
            selectQuery.SetCachedSec(0);
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nJobID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "id", 0);
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            if (nJobID > 0)
                return nJobID;

            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("ftp_upload_queue");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("file_name", "=", sFileName);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("upload_runtime_status", "=", 2);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("fail_count", "=", 0);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("upload_status", "=", 0);
            insertQuery.Execute();
            insertQuery.Finish();

            return AddJobToQueue(nGroupID, sFileName);
        }

        static public void SetJobsForUpload(int nGroupID)
        {
            int nTotalJobs = 0;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select count(*) as 'counter' from ftp_upload_queue (nolock) where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("upload_runtime_status", "=", 2);
            selectQuery.SetCachedSec(0);
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nTotalJobs = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "counter", 0);
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            if (nTotalJobs > 0)
            {
                log.Debug("SetJobsForUpload - " + string.Format("GroupID={0}, Total={1}", nGroupID, nTotalJobs));

                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("ftp_upload_queue");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("upload_runtime_status", "=", 0);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("upload_runtime_status", "=", 2);
                updateQuery += "and";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }
        }
    }
}
