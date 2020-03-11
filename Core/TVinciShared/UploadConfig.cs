using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVinciShared
{
    public class UploadConfig
    {

        #region Members

        public string type;
        public string address;
        public string user_name;
        public string password;
        public string prefix;
        public string region;       

        #endregion

        public UploadConfig()
        {
            type = string.Empty;
            address = string.Empty;
            user_name = string.Empty;
            password = string.Empty;
            prefix = string.Empty;
            region = string.Empty;
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
                    this.address = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "address", 0);
                    this.user_name = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "username", 0);
                    this.password = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "password", 0);
                    this.prefix = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "prefix", 0);
                    this.region = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "region", 0);
                    Uploader.UploaderImpl uploaderImpl = (Uploader.UploaderImpl)ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "uploader_type_id", 0);
                    this.type = Enum.GetName(typeof(Uploader.UploaderImpl), uploaderImpl).ToLower();                   
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return ;
        }
    }
}
