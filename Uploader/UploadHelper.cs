using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uploader
{
    class UploadHelper
    {
        static public List<UploadJob> GetGroupPendingJobs(int nGroupID, int nID)
        {
            List<UploadJob> lPendingJobs = new List<UploadJob>();

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from ftp_upload_queue (nolock) where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("fail_count", "<", 4);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("upload_status", "=", 0);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("upload_runtime_status", "=", 0);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", ">", nID);
            selectQuery += "order by id";
            selectQuery.SetCachedSec(0);
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    UploadJob job = new UploadJob();

                    job.id = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "id", i);
                    job.media_id = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "media_id", i);
                    job.group_id = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "group_id", i);
                    job.file_name = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "file_name", i);
                    job.fail_count = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "fail_count", i);
                    job.upload_status = (UploadJobStatus)ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "upload_status", i);

                    lPendingJobs.Add(job);
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            if (lPendingJobs.Count > 0)
            {
                int nMinID = lPendingJobs[0].id;
                int nMaxID = lPendingJobs[lPendingJobs.Count - 1].id;

                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("ftp_upload_queue");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("upload_runtime_status", "=", 1);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                updateQuery += " and ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("upload_runtime_status", "=", 0);
                updateQuery += " and ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("upload_status", "=", 0);
                updateQuery += "and";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("fail_count", "<", 4);
                updateQuery += "and id >= " + nMinID;
                updateQuery += "and id <= " + nMaxID;
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }

            return lPendingJobs;
        }

        static public List<UploadJob> GetGroupPendingJobs(int nGroupID, int nID, int nNumOfRows)
        {
            List<UploadJob> lPendingJobs = new List<UploadJob>();

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select top " + nNumOfRows + " * from ftp_upload_queue (nolock) where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("fail_count", "<", 4);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("upload_status", "=", 0);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("upload_runtime_status", "=", 0);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", ">", nID);
            selectQuery += "order by id";
            selectQuery.SetCachedSec(0);
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    UploadJob job = new UploadJob();

                    job.id = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "id", i);
                    job.media_id = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "media_id", i);
                    job.group_id = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "group_id", i);
                    job.file_name = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "file_name", i);
                    job.fail_count = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "fail_count", i);
                    job.upload_status = (UploadJobStatus)ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "upload_status", i);

                    lPendingJobs.Add(job);
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            if (lPendingJobs.Count > 0)
            {
                int nMinID = lPendingJobs[0].id;
                int nMaxID = lPendingJobs[lPendingJobs.Count - 1].id;

                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("ftp_upload_queue");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("upload_runtime_status", "=", 1);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                updateQuery += " and ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("upload_runtime_status", "=", 0);
                updateQuery += " and ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("upload_status", "=", 0);
                updateQuery += "and";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("fail_count", "<", 4);
                updateQuery += "and id >= " + nMinID;
                updateQuery += "and id <= " + nMaxID;
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }

            return lPendingJobs;
        }

        static public void UpdateJob(UploadJob job)
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("ftp_upload_queue");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("fail_count", "=", job.fail_count);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("upload_status", "=", (int)job.upload_status);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.UtcNow);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("upload_runtime_status", "=", 0);
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", job.id);
            updateQuery += "and";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", job.group_id);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
        }
    }
}
