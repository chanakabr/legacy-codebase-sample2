using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_users_settings : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {

        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        else if (LoginManager.IsPagePermitted("adm_users_settings.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork("users_connection_string");              
            }
        }
    }


    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet =  "User Settings";
        sRet += " - Edit";
        Response.Write(sRet);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object t = LoginManager.GetLoginGroupID();
        //get the table id by group_id 
        int tableID = ODBCWrapper.Utils.GetIntSafeVal(
            ODBCWrapper.Utils.GetTableSingleVal("groups_parameters", "ID", "GROUP_ID", "=", t, 0, "users_connection_string"));       

        string sBack = "adm_users_settings.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("groups_parameters", "adm_table_pager", sBack, "", "ID", tableID, sBack, "");
        theRecord.SetConnectionKey("users_connection_string");
        
        DataRecordShortIntField dr_pin_must_hours = new DataRecordShortIntField(true, 9, 9);
        dr_pin_must_hours.Initialize("PIN Expirey Timeout (minitus)", "adm_table_header_nbg", "FormInput", "PIN_MUST_HOURS", false);
        theRecord.AddRecord(dr_pin_must_hours);

        string sTable = theRecord.GetTableHTML("adm_users_settings.aspx?submited=1");

        return sTable;
    }
}