using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTPUploadQueue
{

    public enum UploadJobStatus
    {
        PENDING  = 0,
        FINISHED = 1,
        FAILED = 2
    }

    public class GroupFTPSettings
    {
        public int m_nGroupID { get; set; }
        public string m_sFTPIP { get; set; }
        public string m_sFTPUser { get; set; }
        public string m_sFTPPass { get; set; }

        public GroupFTPSettings()
        {
            m_nGroupID = 0;
            m_sFTPIP = string.Empty;
            m_sFTPUser = string.Empty;
            m_sFTPPass = string.Empty;
        }
    }

    public class UploadJob
    {
        public int id { get; set; }
        public string file_name { get; set; }
        public int group_id { get; set; }
        public int fail_count { get; set; }
        public UploadJobStatus upload_status { get; set; }

        public UploadJob()
        {
            id = 0;
            file_name = string.Empty;
            group_id = 0;
            fail_count = 0;
            upload_status = UploadJobStatus.PENDING;
        }

        public override string ToString()
        {
            return string.Format("ID={0}, group_id={1}, file_name={2}, fail_count={3}, upload_status={4}",
                    id.ToString(), group_id.ToString(), file_name, fail_count.ToString(), upload_status.ToString());
        }
    }

    public class RemoveJob
    {
        public string file_name { get; set; }
        public string tech_data { get; set; }

        public RemoveJob()
        {
            file_name = string.Empty;
            tech_data = string.Empty;
        }
    }
}
