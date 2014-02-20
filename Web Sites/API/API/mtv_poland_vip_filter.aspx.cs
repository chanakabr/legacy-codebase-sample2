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

public partial class mtv_poland_vip_filter : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            string sURL = Request.QueryString["cd"];
            bool bIsVIP = true;


            
            Response.StatusCode = 302;
            Response.AddHeader("Location", sURL);
            Response.End();
        }
        catch (Exception ex)
        {
            Logger.Logger.Log("exception", ex.Message, "proxy");
        }
    }
}
