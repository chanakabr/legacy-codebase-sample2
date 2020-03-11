using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class staging_test : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string sIP = PageUtils.GetCallerIP();
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select id from groups_ips where ADMIN_OPEN=1 and is_active=1 and status=1 and (END_DATE is null OR END_DATE>getdate()) and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ip", "=", sIP);
        if (selectQuery.Execute("query", true) != null)
        {
            
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            Response.Write("Select Query Success " + " " + sIP);
            //if (nCount > 0)
            //    nID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
        }
        else
        {
            Response.Write("Select Query Connection Error");
        }
        selectQuery.Finish();
        selectQuery = null;
        
    }
}
