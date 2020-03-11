using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using KLogMonitor;
using System.Reflection;

public partial class monitoring : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            Int32 nCO = 0;
            bool bOK = true;
            Response.Clear();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select count(*) as co from media";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nCO = int.Parse(selectQuery.Table("query").DefaultView[0].Row["co"].ToString());
                }
            }
            else
                bOK = false;
            selectQuery.Finish();
            selectQuery = null;
            if (bOK == true)
                Response.Write("OK");
            else
                Response.Write("FAIL");
        }
        catch (Exception ex)
        {
            log.Error("exceptions - " + ex.Message + " " + ex.StackTrace, ex);
            throw (ex);
        }
    }
}
