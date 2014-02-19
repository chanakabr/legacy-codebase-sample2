using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uploader
{

    public enum UploaderImpl
    {
        FTP = 1,
        AmazonS3 = 2
    }

    public class UploaderFactory
    {
        public static BaseUploader GetUploader(int groupID)
        {
            BaseUploader uploader = null;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

            selectQuery += "select uploaders.group_id, uploaders.address, uploaders.username, uploaders.password, uploaders.prefix, uploaders.uploader_type_id, uploaders.max_connections, regions.system_name as region from uploaders (nolock)";
            selectQuery += "inner join regions (nolock) on uploaders.region_id = regions.id where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", groupID);

            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;

                if (nCount > 0)
                {
                    string sAddress = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "address", 0);
                    string sUser = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "username", 0);
                    string sPass = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "password", 0);
                    string sPrefix = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "prefix", 0);
                    string sRegion = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "region", 0);

                    UploaderImpl uploaderImpl = (UploaderImpl)ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "uploader_type_id", 0);

                    switch (uploaderImpl)
                    {
                        case UploaderImpl.FTP:

                            uploader = new FTPUploader(groupID, sAddress, sUser, sPass, sPrefix);

                            break;

                        case UploaderImpl.AmazonS3:

                            uploader = new AmazonUploader(groupID, sAddress, sUser, sPass, sPrefix, sRegion);

                            break;
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            return uploader;
        }

        public static BaseUploader GetUploader(UploadSettings gus)
        {
            BaseUploader uploader = null;

            switch (gus.m_eImplementation)
            {
                case UploaderImpl.FTP:

                    uploader = new FTPUploader(gus.m_nGroupID, gus.m_sAddress, gus.m_sUser, gus.m_sPass, gus.m_sPrefix);

                    break;

                case UploaderImpl.AmazonS3:

                    uploader = new AmazonUploader(gus.m_nGroupID, gus.m_sAddress, gus.m_sUser, gus.m_sPass, gus.m_sPrefix, gus.m_sRegion);

                    break;
            }

            return uploader;
        }
    }
}
