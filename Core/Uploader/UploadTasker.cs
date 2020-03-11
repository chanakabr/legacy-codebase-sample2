using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uploader
{
    class UploadTasker : ScheduledTasks.BaseTask
    {
        private Int32 m_nGroupID;

        private UploadTasker(Int32 nTaskID, Int32 nIntervalInSec, string engrameters)
            : base(nTaskID, nIntervalInSec, engrameters)
        {
            //string[] seperator = { "||" };
            //string[] splited = engrameters.Split(seperator, StringSplitOptions.None);

            //if (splited.Length == 1)
            //{
            //    m_nGroupID = int.Parse(splited[0].ToString());
            //}
            //else if (splited.Length == 2)
            //{
            //    m_nGroupID = int.Parse(splited[0].ToString());
            //    m_sPicBasePath = splited[1];
            //}
            //else
            //{
            //    m_nGroupID = int.Parse(engrameters);
            //}

            m_nGroupID = int.Parse(engrameters);
        }

        public static ScheduledTasks.BaseTask GetInstance(Int32 nTaskID, Int32 nIntervalInSec, string engrameters)
        {
            return new UploadTasker(nTaskID, nIntervalInSec, engrameters);
        }

        protected override bool DoTheTaskInner()
        {
            List<UploadSettings> _groupsUploadSettings = new List<UploadSettings>();

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

            selectQuery += "select groups.id, uploaders.address, uploaders.username, uploaders.password, uploaders.prefix, uploaders.uploader_type_id, uploaders.max_connections, uploaders.max_jobs, uploaders.base_path, regions.system_name as region from groups (nolock) ";
            selectQuery += "join uploaders (nolock) on groups.id = uploaders.group_id ";
            selectQuery += "join regions (nolock) on uploaders.region_id = regions.id where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("groups.parent_group_id", "=", m_nGroupID);
            selectQuery += " order by id";
            
            selectQuery.SetCachedSec(0);

            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;

                for (int i = 0 ; i < nCount ; i++)
                {
                    UploadSettings gus = new UploadSettings();

                    gus.m_nGroupID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "id", i);
                    gus.m_sAddress = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "address", i);
                    gus.m_sUser = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "username", i);
                    gus.m_sPass = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "password", i);
                    gus.m_sPrefix = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "prefix", i);
                    gus.m_sRegion = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "region", i);
                    gus.m_nMaxConnections = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "max_connections", i);
                    gus.m_nMaxJobs = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "max_jobs", i);
                    gus.m_sBasePath = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "base_path", i);
                    gus.m_eImplementation = (UploaderImpl)ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "uploader_type_id", i);

                    _groupsUploadSettings.Add(gus);
                }
            }

            selectQuery.Finish();
            selectQuery = null;

            foreach (UploadSettings groupUploadSettings in _groupsUploadSettings)
            {
                BaseUploader uploader = UploaderFactory.GetUploader(groupUploadSettings);

                uploader.UploadQueue(groupUploadSettings.m_sBasePath, groupUploadSettings.m_nMaxConnections, groupUploadSettings.m_nMaxJobs);
            }

            return true;
        }
    }
}
