using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using KLogMonitor;
using TVinciShared;

public partial class adm_users_settings : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
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
        Response.Write(PageUtils.GetPreHeader() + ": User Settings");
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        object t = null;

        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }

        object groupId = LoginManager.GetLoginGroupID();

        // check ig groupid is parent , if so show page with all filed else (not parent show a message)
        int tableID = GetUserSettingsID(ODBCWrapper.Utils.GetIntSafeVal(groupId));
        bool isParentGroup = IsParentGroup(ODBCWrapper.Utils.GetIntSafeVal(groupId));


        string sTable = string.Empty;
        if (!isParentGroup)
        {
            sTable = (PageUtils.GetPreHeader() + ": Module is not implemented");
        }
        else
        {
            if (tableID > 0) // a new record
            {
                t = tableID;
            }
            string sBack = "adm_users_settings.aspx?search_save=1";
            DBRecordWebEditor theRecord = new DBRecordWebEditor("groups_parameters", "adm_table_pager", sBack, "", "ID", t, sBack, "");
            theRecord.SetConnectionKey("users_connection_string");

            DataRecordCheckBoxField dr_LoginViaPin = new DataRecordCheckBoxField(true);
            dr_LoginViaPin.Initialize("Enable log-in via PIN", "adm_table_header_nbg", "FormInput", "login_via_pin", false);
            theRecord.AddRecord(dr_LoginViaPin);

            DataRecordCheckBoxField dr_SecurityQuestion = new DataRecordCheckBoxField(true);
            dr_SecurityQuestion.Initialize("Force security question", "adm_table_header_nbg", "FormInput", "security_question", false);
            theRecord.AddRecord(dr_SecurityQuestion);

            DataRecordShortIntField dr_pin_must_hours = new DataRecordShortIntField(true, 9, 9);
            dr_pin_must_hours.Initialize("PIN Expirey Timeout (minutes)", "adm_table_header_nbg", "FormInput", "PIN_MUST_HOURS", false);
            theRecord.AddRecord(dr_pin_must_hours);

            DataRecordCheckBoxField dr_allowUserDeletion = new DataRecordCheckBoxField(true);
            dr_allowUserDeletion.Initialize("Allow user deletion", "adm_table_header_nbg", "FormInput", "allow_delete_user", false);
            theRecord.AddRecord(dr_allowUserDeletion);

            DataRecordCheckBoxField dr_CloseAccountMail = new DataRecordCheckBoxField(true);
            dr_CloseAccountMail.Initialize("Send Close account Email", "adm_table_header_nbg", "FormInput", "SEND_CLOSE_ACCOUNT_MAIL", false);
            dr_CloseAccountMail.SetDefault(1);
            theRecord.AddRecord(dr_CloseAccountMail);


            sTable = theRecord.GetTableHTML("adm_users_settings.aspx?submited=1");
        }
        return sTable;
    }

    private bool IsParentGroup(int groupID)
    {
        bool res = false;
        try
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
            selectQuery += "select * from F_Get_GroupsParent(" + groupID.ToString() + ")";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    int parentGroupID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row, "PARENT_GROUP_ID");
                    if (parentGroupID == 1)
                    {
                        res = true;
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        catch (Exception ex)
        {
            log.Error("", ex);
            res = false;
        }
        return res;
    }

    private int GetUserSettingsID(int groupID)
    {
        int userSettingId = 0;
        try
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("users_connection_string");
            selectQuery += "select ID from groups_parameters where status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    userSettingId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row, "ID");
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        catch (Exception ex)
        {
            log.Error("", ex);
            userSettingId = 0;
        }
        return userSettingId;
    }
}