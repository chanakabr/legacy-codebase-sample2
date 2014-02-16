using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTPUploadQueue
{
    public class FTPUploadQueueTasker : ScheduledTasks.BaseTask
    {
        private Int32 m_nGroupID;
        private string m_sPicBasePath;

        private FTPUploadQueueTasker(Int32 nTaskID, Int32 nIntervalInSec, string engrameters)
            : base(nTaskID, nIntervalInSec, engrameters)
        {
            string[] seperator = { "||" };
            string[] splited = engrameters.Split(seperator, StringSplitOptions.None);
            if (splited.Length == 2)
            {
                m_nGroupID = int.Parse(splited[0].ToString());
                m_sPicBasePath = splited[1];
                
            }
            else
            {
                m_nGroupID = int.Parse(engrameters);
                m_sPicBasePath = "D:/apps/services/scheduler/pics/";
            }
        }

        public static ScheduledTasks.BaseTask GetInstance(Int32 nTaskID, Int32 nIntervalInSec, string engrameters)
        {
            return new FTPUploadQueueTasker(nTaskID, nIntervalInSec, engrameters);
        }

        protected override bool DoTheTaskInner()
        {
            List<GroupFTPSettings> lGroups = new List<GroupFTPSettings>();

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id, pics_ftp, pics_ftp_username, pics_ftp_password from groups (nolock) where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("parent_group_id", "=", m_nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0 ; i < nCount ; i++)
                {
                    GroupFTPSettings gfs = new GroupFTPSettings();
                    gfs.m_nGroupID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "id", i);
                    gfs.m_sFTPIP = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "pics_ftp", i);
                    gfs.m_sFTPUser = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "pics_ftp_username", i);
                    gfs.m_sFTPPass = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "pics_ftp_password", i);

                    lGroups.Add(gfs);
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            foreach (GroupFTPSettings gfs in lGroups)
            {
                FTPUploader uploader = new FTPUploader(gfs.m_nGroupID, string.Empty, gfs.m_sFTPIP, gfs.m_sFTPUser, gfs.m_sFTPPass, m_sPicBasePath  + gfs.m_nGroupID + "/");
                uploader.UploadQueue_MT();
            }

            return true;
        }
    }
}
