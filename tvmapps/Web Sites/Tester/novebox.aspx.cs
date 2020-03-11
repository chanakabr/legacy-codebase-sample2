using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;



public partial class novebox : System.Web.UI.Page
{
    protected Int32 InsertNewFileID(string sGuid, Int32 nMediaID)
    {
        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("media_files");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_TYPE_ID", "=", 202);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_QUALITY_ID", "=", 3);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STREAMING_SUPLIER_ID", "=", 202);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STREAMING_CODE", "=", sGuid);
        
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", 104);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
        insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;
        return GetFileID(sGuid , nMediaID);
    }

    protected Int32 GetFileID(string sGuid, Int32 nMediaID)
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetCachedSec(0);
        selectQuery += "select id from media_files where group_id=104 and MEDIA_TYPE_ID=202 and MEDIA_QUALITY_ID=3 and STREAMING_SUPLIER_ID=202 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("STREAMING_CODE", "=", sGuid);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMediaID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
                nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
        }
        selectQuery.Finish();
        selectQuery = null;
        return nRet;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        //selectQuery.SetConnectionKey("");
        selectQuery.SetCachedSec(0);
        selectQuery += "select id,meta1_str from media where group_id=104";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                Int32 nMediaID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                string sGUID = selectQuery.Table("query").DefaultView[i].Row["meta1_str"].ToString();
                if (sGUID != "")
                {
                    Int32 nFileID = GetFileID(sGUID, nMediaID);
                    if (nFileID == 0)
                    {
                        nFileID = InsertNewFileID(sGUID, nMediaID);
                    }
                }
            }
        }
        selectQuery.Finish();
        selectQuery = null;
    }
}
