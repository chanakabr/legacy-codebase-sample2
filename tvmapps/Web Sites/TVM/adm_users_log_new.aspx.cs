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
using TVinciShared;

public partial class adm_users_log_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;

    static public bool IsTvinciImpl()
    {
        Int32 nImplID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("users_connection");
        selectQuery += "select IMPLEMENTATION_ID from groups_modules_implementations where is_active=1 and status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_ID", "=", 1);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                nImplID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["IMPLEMENTATION_ID"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        if (nImplID > 0)
            return true;
        return false;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            if (IsTvinciImpl() == false)
            {
                Server.Transfer("adm_module_not_implemented.aspx");
                return;
            }

            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                Int32 nID = DBManipulator.DoTheWork("users_connection");
                Int32 nUpdaterID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("users_log", "updater_id", nID , "users_connection").ToString());
                string sUpdater = ODBCWrapper.Utils.GetTableSingleVal("accounts", "USERNAME", nUpdaterID).ToString();
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("users_log");
                updateQuery.SetConnectionKey("users_connection");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("writer", "=", sUpdater);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nID);
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }
            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["user_log_id"] != null &&
                Request.QueryString["user_log_id"].ToString() != "")
            {
                Session["user_log_id"] = int.Parse(Request.QueryString["user_log_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("users", "group_id", int.Parse(Session["user_log_id"].ToString()), "users_connection").ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["user_log_id"] = 0;
        }

    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": " + ODBCWrapper.Utils.GetTableSingleVal("users", "userNAME", int.Parse(Session["user_id"].ToString()), "users_connection").ToString() + " Log";
        if (Session["user_log_id"] != null && Session["user_log_id"].ToString() != "" && Session["user_log_id"].ToString() != "0")
            sRet += " - Edit";
        else
            sRet += " - New";
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
        object t = null; ;
        if (Session["user_log_id"] != null && Session["user_log_id"].ToString() != "" && int.Parse(Session["user_log_id"].ToString()) != 0)
            t = Session["user_log_id"];
        string sBack = "adm_users_log.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("users_log", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("users_connection");

        DataRecordLongTextField dr_remark = new DataRecordLongTextField("ltr", true, 60, 3);
        dr_remark.Initialize("Entry", "adm_table_header_nbg", "FormInput", "MESSAGE", true);
        theRecord.AddRecord(dr_remark);

        DataRecordShortIntField dr_user_id = new DataRecordShortIntField(false, 9, 9);
        dr_user_id.Initialize("user_id", "adm_table_header_nbg", "FormInput", "user_id", false);
        dr_user_id.SetValue(Session["user_id"].ToString());
        theRecord.AddRecord(dr_user_id);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_users_log_new.aspx?submited=1");

        return sTable;
    }
}
