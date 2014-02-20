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
using System.Net;

public partial class proxy : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            string sUserAgent = Request.ServerVariables["HTTP_USER_AGENT"];
            if (sUserAgent.ToLower().IndexOf("windows-media-player") == -1 && sUserAgent.ToLower().IndexOf("nsplayer") == -1)
                Response.Redirect("http://mz-web-lb-1.mediazone.co.il/TVinciDRM/getMMS.aspx/q=noentrance.wmv");
            string sURL = Request.QueryString["url"];
            Response.StatusCode = 302;
            sURL = sURL.Replace("mms://", "http://");
            Response.AddHeader("Location", sURL);
            Response.End();
        }
        catch (Exception ex)
        {
            Logger.Logger.Log("exception", ex.Message, "proxy");
        }
    }


}
