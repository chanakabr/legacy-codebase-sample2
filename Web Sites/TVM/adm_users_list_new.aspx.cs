using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using KLogMonitor;
using TVinciShared;

public partial class adm_users_list_new : System.Web.UI.Page
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
                Int32 nID = DBManipulator.DoTheWork("users_connection");
                try
                {
                    Notifiers.BaseUsersNotifier t = null;
                    Notifiers.Utils.GetBaseUsersNotifierImpl(ref t, LoginManager.GetLoginGroupID(), "users_connection");
                    if (t != null)
                        t.NotifyChange(nID.ToString());
                    return;
                }
                catch (Exception ex)
                {
                    log.Error("exception - " + nID.ToString() + " : " + ex.Message, ex);
                }
            }
            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["user_id"] != null &&
                Request.QueryString["user_id"].ToString() != "")
            {
                Session["user_id"] = int.Parse(Request.QueryString["user_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("users", "group_id", int.Parse(Session["user_id"].ToString()), "users_connection").ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["user_id"] = 0;
        }

    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Users";
        if (Session["user_id"] != null && Session["user_id"].ToString() != "" && Session["user_id"].ToString() != "0")
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
        if (Session["user_id"] != null && Session["user_id"].ToString() != "" && int.Parse(Session["user_id"].ToString()) != 0)
            t = Session["user_id"];
        string sBack = "adm_users_list.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("users", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("users_connection");

        DataRecordShortTextField dr_domain = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_domain.Initialize("Username", "adm_table_header_nbg", "FormInput", "USERNAME", true);
        theRecord.AddRecord(dr_domain);

        DataRecordShortTextField dr_firstname = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_firstname.Initialize("First Name", "adm_table_header_nbg", "FormInput", "FIRST_NAME", true);
        theRecord.AddRecord(dr_firstname);

        DataRecordShortTextField dr_lastname = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_lastname.Initialize("Last Name", "adm_table_header_nbg", "FormInput", "LAST_NAME", true);
        theRecord.AddRecord(dr_lastname);

        DataRecordShortTextField dr_email = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_email.Initialize("Email", "adm_table_header_nbg", "FormInput", "EMAIL_ADD", true);
        theRecord.AddRecord(dr_email);

        DataRecordShortTextField dr_affiliate = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_affiliate.Initialize("Affiliate", "adm_table_header_nbg", "FormInput", "REG_AFF", true);
        theRecord.AddRecord(dr_affiliate);

        DataRecordDropDownField dr_userType = new DataRecordDropDownField("users_types", "description", "id", "", null, 60, true);
        dr_userType.Initialize("UserType", "adm_table_header_nbg", "FormInput", "User_Type", false);
        string sQuery = "select id , [description] as txt from users_types(nolock) where is_active = 1 and [status] = 1 and group_id=" + LoginManager.GetLoginGroupID();
        dr_userType.SetNoSelectStr("---");
        string sUserTypeDefaultValue = GetUserTypeDefaultValue();
        dr_userType.SetDefaultVal(sUserTypeDefaultValue);
        dr_userType.SetSelectsQuery(sQuery);
        theRecord.AddRecord(dr_userType);



        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_users_list_new.aspx?submited=1");

        return sTable;
    }

    public string GetUserTypeDefaultValue()
    {
        string sRetValue = string.Empty;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("users_connection");
        selectQuery += "select top 1 description from users_types(nolock) where is_active=1 and status=1 and is_default=1  and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                sRetValue = selectQuery.Table("query").DefaultView[0].Row["Description"].ToString();
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return sRetValue;
    }
}
