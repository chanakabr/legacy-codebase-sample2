using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Uploader
{
    public enum UploadJobStatus
    {
        PENDING = 0,
        FINISHED = 1,
        FAILED = 2
    }

    public class UploadSettings
    {
        public int m_nGroupID { get; set; }
        public string m_sAddress { get; set; }
        public string m_sUser { get; set; }
        public string m_sPass { get; set; }
        public string m_sPrefix { get; set; }
        public string m_sRegion { get; set; }
        public int m_nMaxConnections { get; set; }
        public int m_nMaxJobs { get; set; }
        public string m_sBasePath { get; set; }
        public UploaderImpl m_eImplementation { get; set; }

        public UploadSettings()
        {
            m_nGroupID = 0;
            m_sAddress = string.Empty;
            m_sUser = string.Empty;
            m_sPass = string.Empty;
            m_sPrefix = string.Empty;
            m_sRegion = string.Empty;
            m_nMaxConnections = 10;
            m_nMaxJobs = 50;
            m_sBasePath = string.Empty;
            m_eImplementation = UploaderImpl.FTP;
        }
    }

    public class UploadJob
    {
        public int id { get; set; }
        public int media_id { get; set; }
        public string file_name { get; set; }
        public int group_id { get; set; }
        public int fail_count { get; set; }
        public UploadJobStatus upload_status { get; set; }

        public UploadJob()
        {
            id = 0;
            media_id = 0;
            file_name = string.Empty;
            group_id = 0;
            fail_count = 0;
            upload_status = UploadJobStatus.PENDING;
        }

        public override string ToString()
        {
            return string.Format("ID={0}, group_id={1}, file_name={2}, fail_count={3}, upload_status={4}, media_id={5}",
                    id, group_id, file_name, fail_count, upload_status, media_id);
        }
    }
}
