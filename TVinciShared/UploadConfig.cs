using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVinciShared
{
    public class UploadConfig
    {

        #region Members

        public string m_sType;
        public string m_sAddress;
        public string m_sUserName;
        public string m_sPass;
        public string m_sPrefix;       
        public string m_sRegion;

        #endregion

        public UploadConfig()
        {
            m_sType = string.Empty;
            m_sAddress = string.Empty;
            m_sUserName = string.Empty;
            m_sPass = string.Empty;
            m_sPrefix = string.Empty;            
            m_sRegion = string.Empty;
        }

        public void setUploadConfig(int nGroupID)
        {            
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select uploaders.group_id, uploaders.address, uploaders.username, uploaders.password, uploaders.prefix, uploaders.uploader_type_id, regions.system_name as region from uploaders (nolock)";
            selectQuery += "inner join regions (nolock) on uploaders.region_id = regions.id where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);

            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    this.m_sAddress = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "address", 0);
                    this.m_sUserName = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "username", 0);
                    this.m_sPass = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "password", 0);
                    this.m_sPrefix = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "prefix", 0);
                    this.m_sRegion = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "region", 0);
                    Uploader.UploaderImpl uploaderImpl = (Uploader.UploaderImpl)ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "uploader_type_id", 0); 
                    this.m_sType = Enum.GetName(typeof(Uploader.UploaderImpl), uploaderImpl).ToLower();                   
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return ;
        }
    }
}
