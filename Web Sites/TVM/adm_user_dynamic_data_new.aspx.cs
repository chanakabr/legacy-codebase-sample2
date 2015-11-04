using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using KLogMonitor;
using TVinciShared;

public partial class adm_user_dynamic_data_new : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
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
                DBManipulator.DoTheWork("users_connection");

                try
                {
                    Notifiers.BaseUsersNotifier t = null;
                    Notifiers.Utils.GetBaseUsersNotifierImpl(ref t, LoginManager.GetLoginGroupID(), "users_connection");
                    if (t != null)
                        t.NotifyChange(Session["user_id"].ToString());
                    return;
                }
                catch (Exception ex)
                {
                    log.Error("exception - " + Session["user_id"].ToString() + " : " + ex.Message, ex);
                }

                return;
            }
            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["user_dynamic_data_id"] != null &&
                Request.QueryString["user_dynamic_data_id"].ToString() != "")
            {
                Session["user_dynamic_data_id"] = int.Parse(Request.QueryString["user_dynamic_data_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("users_dynamic_data", "group_id", int.Parse(Session["user_dynamic_data_id"].ToString()), "users_connection").ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["user_dynamic_data_id"] = 0;
        }

    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sUserName = ODBCWrapper.Utils.GetTableSingleVal("users", "username", int.Parse(Session["user_id"].ToString()), "users_connection").ToString();
        string sRet = PageUtils.GetPreHeader() + ": User Dynamic Data (" + sUserName + ")";
        if (Session["user_dynamic_data_id"] != null && Session["user_dynamic_data_id"].ToString() != "" && Session["user_dynamic_data_id"].ToString() != "0")
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
        if (Session["user_dynamic_data_id"] != null && Session["user_dynamic_data_id"].ToString() != "" && int.Parse(Session["user_dynamic_data_id"].ToString()) != 0)
            t = Session["user_dynamic_data_id"];
        string sBack = "adm_user_dynamic_data.aspx?search_save=1&user_id=" + Session["user_id"].ToString();
        DBRecordWebEditor theRecord = new DBRecordWebEditor("users_dynamic_data", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("users_connection");

        DataRecordShortTextField dr_domain = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_domain.Initialize("Type", "adm_table_header_nbg", "FormInput", "DATA_TYPE", true);
        theRecord.AddRecord(dr_domain);

        DataRecordShortTextField dr_firstname = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_firstname.Initialize("Value", "adm_table_header_nbg", "FormInput", "DATA_VALUE", true);
        theRecord.AddRecord(dr_firstname);

        DataRecordShortIntField dr_user_id = new DataRecordShortIntField(false, 9, 9);
        dr_user_id.Initialize("User ID", "adm_table_header_nbg", "FormInput", "USER_ID", false);
        dr_user_id.SetValue(Session["user_id"].ToString());
        theRecord.AddRecord(dr_user_id);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_user_dynamic_data_new.aspx?submited=1");

        return sTable;
    }
}
