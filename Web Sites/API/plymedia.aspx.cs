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

public partial class plymedia : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string sRet = "";
        if (!IsPostBack)
        {
            if (Request.QueryString["media_file_id"] == null)
                sRet = "<error description=\"no valid media\"></error>";
            else
            {
                try
                {
                    Int32 nMediaFileID = int.Parse(Request.QueryString["media_file_id"].ToString());
                    sRet = TVinciShared.ProtocolsFuncs.GetMiniMediaTagInner(nMediaFileID);
                }
                catch (Exception ex)
                {
                    sRet = "<error description=\"no valid media\"></error>";
                }
            }
            Response.ClearHeaders();
            Response.Clear();
            Response.ContentType = "text/xml";
            Response.Expires = -1;
            Response.Write(sRet);
        }
    }
}
