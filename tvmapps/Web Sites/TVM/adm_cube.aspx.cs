using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;
using System.Text;
using System.Security.Cryptography;

public partial class adm_cube : System.Web.UI.Page
{
    private const string MD5_SECRET_CODE = "STATISTICSSERVICE1@3$";

    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
        {
            Response.Expires = -1;
            return;
        }
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(10, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
        }
        Response.Expires = -1;
    }

    public string GetInitParameters()
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        if (nGroupID == 1 && Session["parent_group_id"] != null && !string.IsNullOrEmpty(Session["parent_group_id"].ToString()))
        {
            nGroupID = int.Parse(Session["parent_group_id"].ToString());
        }
        string groupName = ODBCWrapper.Utils.GetTableSingleVal("groups", "GROUP_NAME", nGroupID).ToString();
       
        int userID = LoginManager.GetLoginID();

        // Create md5 code
        byte[] originalBytes = ASCIIEncoding.Default.GetBytes(groupName + userID + MD5_SECRET_CODE);
        byte[] encodedBytes = new MD5CryptoServiceProvider().ComputeHash(originalBytes);
        string s = string.Concat("USERID=", userID, ",GROUPID=", nGroupID.ToString(), ",MD5CHECKCODE=", BitConverter.ToString(encodedBytes));
        if (Request.QueryString["media_id"] != null)
        {
            string sMediaName = ODBCWrapper.Utils.GetTableSingleVal("media", "NAME", int.Parse(Request.QueryString["media_id"].ToString())).ToString();
            //s += ",ParameterType=media,ParameterValue=" + Request.QueryString["media_id"].ToString();
            s += ",ParameterType=media,ParameterValue=" + Server.UrlEncode(sMediaName);
        }
        return s;

        //Response.Write(ODBCWrapper.Utils.GetTableSingleVal("groups", "GROUP_NAME", nGroupID).ToString());
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Basic statistics";
        Response.Write(sRet);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }
}
