using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTPUploadQueue
{
    public class FTPUploadHelper
    {
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
                Logger.Logger.Log("SetJobsForUpload", string.Format("GroupID={0}, Total={1}", nGroupID, nTotalJobs), "FTPUploaderManager");

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

        //FTPUploader Tasker

        static public List<UploadJob> GetGroupPendingJobs(int nGroupID, int nID)
        {
            List<UploadJob> lPendingJobs = new List<UploadJob>();

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select top 50 * from ftp_upload_queue (nolock) where";
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
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    UploadJob job = new UploadJob();

                    job.id = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "id", i);
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
                int nMaxID = lPendingJobs[lPendingJobs.Count-1].id;

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

        static public void UpdateJobs(List<UploadJob> jobs)
        {
            string sQuery = string.Empty;
            int nCounter = 0; 
            int nTotalJobs = jobs.Count;

            foreach (UploadJob job in jobs)
            {
                sQuery += string.Format("UPDATE ftp_upload_queue SET upload_runtime_status={0}, fail_count={1}, upload_status={2}, update_date={3} where id={4};",
                    0, job.fail_count, job.upload_status, DateTime.UtcNow, job.id);
                
                nCounter++;

                if ((nCounter % 300) == 0 || nCounter == nTotalJobs)
                {
                    ODBCWrapper.DirectQuery directQuery = new ODBCWrapper.DirectQuery();
                    directQuery += sQuery;
                    bool bRes = directQuery.Execute();
                    directQuery.Finish();
                    directQuery = null;

                    if (!bRes)
                    {
                        Logger.Logger.Log("Error UpdateJobs", string.Format("GroupID={0}, Total={1}, counter={2}, Querey : {3}", job.group_id, nTotalJobs, nCounter, sQuery), "FTPUploaderManager");
                    }

                    sQuery = string.Empty;
                }
            }
        }

        static public UploadJob GetJobByID(int nJobID)
        {
            UploadJob job = new UploadJob();

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from ftp_upload_queue (nolock) where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nJobID);
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    job.id = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "id", 0);
                    job.group_id = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "group_id", 0);
                    job.file_name = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "file_name", 0);
                    job.fail_count = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "fail_count", 0);
                    job.upload_status = (UploadJobStatus)ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "upload_status", 0);
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            return job;
        }

        //FTPRemove Tasker
        static public List<RemoveJob> GetGroupRemoveJobs(int nGroupID)
        {
            List<RemoveJob> jobs = new List<RemoveJob>();

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select m.ID, m.MEDIA_PIC_ID, m.GROUP_ID, p.BASE_URL, p.TECH_DATA, 0 as 'WIDTH', 0 as HEIGHT from media m";
            selectQuery += "left join pics p on p.id=m.media_pic_id";
            selectQuery += "where p.TECH_DATA is not null and p.TECH_DATA not like ''";
            selectQuery += "and m.id=224754 and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.MEDIA_PIC_ID", ">", 0);
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string file = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "BASE_URL", i);
                    string width = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "WIDTH", i);
                    string height = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "HEIGHT", i);
                    string tech_data = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "TECH_DATA", i);

                    string[] val = file.Split('.');
                    string fileName = string.Format("{0}_{1}X{2}.{3}", val[0], width, height, val[1]);

                    RemoveJob job = new RemoveJob();
                    job.file_name = fileName;
                    job.tech_data = tech_data;
                    jobs.Add(job);
                }
            }
            selectQuery.Finish();
            selectQuery = null;


            //ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            //selectQuery += "select m.ID, m.MEDIA_PIC_ID, m.GROUP_ID, p.BASE_URL, p.TECH_DATA, mps.WIDTH, mps.HEIGHT from media m";
            //selectQuery += "left join pics p on p.id=m.media_pic_id";
            //selectQuery += "left join media_pics_sizes mps on m.GROUP_ID=mps.GROUP_ID and isNu";
            //selectQuery += "where mps.STATUS=1";
            //selectQuery += "and (p.TECH_DATA is null or p.TECH_DATA like '')";
            //selectQuery += "and";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("p.group_id", "=", nGroupID);
            //selectQuery += "and";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.MEDIA_PIC_ID", ">", 0);
            //selectQuery += "and (";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.status", "=", 2);
            //selectQuery += "or m.FINAL_END_DATE+7<GETDATE())";
            //selectQuery += "UNION ALL";
            //selectQuery += "select m.ID, m.MEDIA_PIC_ID, m.GROUP_ID, p.BASE_URL, p.TECH_DATA, 0 as 'WIDTH', 0 as HEIGHT from media m";
            //selectQuery += "left join pics p on p.id=m.media_pic_id";
            //selectQuery += "where p.TECH_DATA is not null and p.TECH_DATA not like ''";
            //selectQuery += "and";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", nGroupID);
            //selectQuery += "and";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.MEDIA_PIC_ID", ">", 0);
            //selectQuery += "and (";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.status", "=", 2);
            //selectQuery += "or m.FINAL_END_DATE+7<GETDATE())";
            //if (selectQuery.Execute("query", true) != null)
            //{
            //    int nCount = selectQuery.Table("query").DefaultView.Count;
            //    for (int i = 0; i < nCount; i++)
            //    {
            //        string file = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "BASE_URL", i);
            //        string width = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "WIDTH", i);
            //        string height = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "HEIGHT", i);
            //        string tech_data = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "TECH_DATA", i);

            //        string[] val = file.Split('.');
            //        string fileName = string.Format("{0}_{1}X{2}.{3}", val[0], width, height, val[1]);

            //        RemoveJob job = new RemoveJob();
            //        job.file_name = fileName;
            //        job.tech_data = tech_data;
            //        jobs.Add(job);
            //    }
            //}
            //selectQuery.Finish();
            //selectQuery = null;

            return jobs;
        }
    }
}
