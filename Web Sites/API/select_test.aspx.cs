using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class select_test : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += " select * from media where id = 197040";
        if (selectQuery.Execute("query", true) != null)
        {
            int count = selectQuery.Table("query").DefaultView.Count;
            if (count > 0)
            {

                Response.ClearHeaders();
                Response.Clear();
                Response.Write("Query exectued item found");
            }
            else
            {

                Response.ClearHeaders();
                Response.Clear();
                Response.Write("Query exectued item not found");
            }
        }
        else
        {
            Response.ClearHeaders();
            Response.Clear();
            Response.Write("Query failed");
        }
        selectQuery.Finish();
        selectQuery = null;
    }
}