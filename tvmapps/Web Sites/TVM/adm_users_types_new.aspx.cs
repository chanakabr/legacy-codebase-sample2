using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_users_types_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_users_types.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_users_types.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;

        if (!IsPostBack)
        {
            Int32 nMenuID = 0;

            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true); 
        }

        if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
        {        
            //Need to update all users types for this group id to is_default = 0 beofre updatng user selection. 

            UpdateIsDefaultValue(0);

            DBManipulator.DoTheWork("users_connection");
            return;
        }

        if (Request.QueryString["user_type_id"] != null &&
            Request.QueryString["user_type_id"].ToString() != "")
        {
            Session["user_type_id"] = int.Parse(Request.QueryString["user_type_id"].ToString());
        }
        else
            Session["user_type_id"] = 0;
  
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Users types values");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
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
        object t = null; ;
        if (Session["user_type_id"] != null && Session["user_type_id"].ToString() != "" && int.Parse(Session["user_type_id"].ToString()) != 0)
            t = Session["user_type_id"];
        string sBack = "adm_users_types.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("users_types", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("users_connection");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "description", true);
        theRecord.AddRecord(dr_name);

        DataRecordCheckBoxField dr_IsDefault = new DataRecordCheckBoxField(true);
        dr_IsDefault.Initialize("Is default", "adm_table_header_nbg", "FormInput", "is_default", false);
        theRecord.AddRecord(dr_IsDefault);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);
     
        string sTable = theRecord.GetTableHTML("adm_users_types_new.aspx?submited=1");

        return sTable;
    }

    private void UpdateIsDefaultValue(int isDefaultValue)
    {
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("users_types");
        updateQuery.SetConnectionKey("users_connection");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_default", "=", isDefaultValue);
        updateQuery += "where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", LoginManager.GetLoginGroupID());
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;
    }
}