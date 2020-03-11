using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class ODBC : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

        GetGroupID.Click += new EventHandler(GetGroupID_Click);
        GetFileID.Click += new EventHandler(GetFileID_Click);
    }

    void GetGroupID_Click(object sender, EventArgs e)
    {
        ConditionalAccess.BaseConditionalAccess t = null;
        Int32 myInt = ConditionalAccess.Utils.GetGroupID("conditionalaccess_134", "11111", "00000", ref t);
        //"conditionalaccess_109", "11111", "GetUserPermittedSubscriptions", ref t);
        GroupID.Text = myInt.ToString();
    }




    void GetFileID_Click(object sender, EventArgs e)
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetCachedSec(0);
        selectQuery += "select id from media_files where group_id=104 and MEDIA_TYPE_ID=202 and MEDIA_QUALITY_ID=3 and STREAMING_SUPLIER_ID=202 and ";

        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", "163444");
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
                nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
        }
        selectQuery.Finish();
        selectQuery = null;
        FileID.Text =  nRet.ToString();
    }
}