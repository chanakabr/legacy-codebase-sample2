using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using TVinciShared;

public partial class adm_all_accounts_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;

    protected bool DoesUserNameExdists(string sUserName)
    {
        bool bRet = false;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select id from accounts where status<>2 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("username", "=", sUserName);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
                bRet = true;
        }
        selectQuery.Finish();
        selectQuery = null;
        return bRet;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");

        Int32 nMenuID = 0;
        m_sMenu = TVinciShared.Menu.GetMainMenu(1, true, ref nMenuID);
        m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 2, true);
        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                string sName = Request.Form["0_val"].ToString();
                string sPass1 = Request.Form["1_val"].ToString();
                string sPass2 = Request.Form["2_val"].ToString();
                string sGroupID = Request.Form["3_val"].ToString();
                if (sPass1 == "" || sPass2 == "")
                    Session["error_msg"] = "Empty passwwords";
                else if (sPass1 != sPass2)
                    Session["error_msg"] = "Password and confirmation passwords are not equal";
                else
                {
                    bool bDoesUserNameExists = DoesUserNameExdists(sName);
                    if (bDoesUserNameExists == false)
                    {
                        bool bSuccess = LoginManager.CreateNewAccount(sName, sPass1, int.Parse(sGroupID));
                        if (bSuccess == true)
                        {
                            Response.Redirect("adm_all_accounts.aspx");
                        }
                        else
                            Session["error_msg"] = "Account with the same username allready exists - please choose another username.";
                    }
                    else
                        Session["error_msg"] = "User name exists";
                }
            }
        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": new account");
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
        object t = null;
        DBRecordWebEditor theRecord = new DBRecordWebEditor("accounts", "adm_table_pager", "adm_all_accounts.aspx", "", "ID", t, "adm_all_accounts.aspx", "");

        DataRecordShortTextField dr_user_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_user_name.Initialize("Username", "adm_table_header_nbg", "FormInput", "USERNAME", false);
        theRecord.AddRecord(dr_user_name);

        DataRecordShortTextField dr_pass1_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_pass1_name.SetPassword();
        dr_pass1_name.Initialize("Password", "adm_table_header_nbg", "FormInput", "PASSWORD", true);
        dr_pass1_name.SetValue("");
        theRecord.AddRecord(dr_pass1_name);

        DataRecordShortTextField dr_pass2_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_pass2_name.SetPassword();
        dr_pass2_name.Initialize("Password confirmation", "adm_table_header_nbg", "FormInput", "PASSWORD", true);
        dr_pass2_name.SetValue("");
        theRecord.AddRecord(dr_pass2_name);

        DataRecordDropDownField dr_group_id = new DataRecordDropDownField("groups", "group_name", "id", "", null, 60, false);
        string sInStr = PageUtils.GetAllChildGroupsStr();
        string sQuery = "select group_name as txt,id as id from groups where id " + sInStr;
        dr_group_id.SetSelectsQuery(sQuery);
        dr_group_id.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        theRecord.AddRecord(dr_group_id);

        string sTable = theRecord.GetTableHTML("adm_all_accounts_new.aspx?submited=1");

        return sTable;
    }
}
