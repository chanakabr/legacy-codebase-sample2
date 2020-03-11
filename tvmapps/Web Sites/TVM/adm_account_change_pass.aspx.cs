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

public partial class adm_account_change_pass : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
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
            if (Request.QueryString["account_id"] != null)
                Session["account_id"] = Request.QueryString["account_id"].ToString();
            if ((Request.QueryString["account_id"] == null || Request.QueryString["account_id"].ToString() == "") && Session["account_id"] == null)
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }
            Int32 nAccountID = int.Parse(Session["account_id"].ToString());
            Int32 nGroupID = LoginManager.GetLoginGroupID();
            bool bBelongs = PageUtils.DoesAccountBelongToGroup(nAccountID, nGroupID);
            if (bBelongs == false)
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                string sPass1 = Request.Form["1_val"].ToString();
                string sPass2 = Request.Form["2_val"].ToString();
                if (sPass1 == "" || sPass2 == "")
                    Session["error_msg"] = "Empty passwwords";
                else if (sPass1 != sPass2)
                    Session["error_msg"] = "Password and confirmation passwords are not equal";
                else
                {
                    bool bStrongPass = LoginManager.IsPasswordStrong(sPass1);
                    if (bStrongPass == true)
                    {
                        bool bSuccess = LoginManager.ChangeUserPassword(int.Parse(Session["account_id"].ToString()), "", sPass1);
                        if (bSuccess == true)
                        {
                            Session["account_id"] = null;
                            Response.Redirect("adm_accounts.aspx");
                        }
                        else
                        {
                            Session["error_msg"] = "Password change failed";
                        }
                    }
                    else
                    {
                        Session["error_msg"] = "Password not strong";
                    }
                }
                //DBManipulator.DoTheWork();
            }
        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + " : My account settings");
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
        object t = int.Parse(Session["account_id"].ToString());
        DBRecordWebEditor theRecord = new DBRecordWebEditor("accounts", "adm_table_pager", "adm_accounts.aspx", "", "ID", t, "adm_accounts.aspx", "");

        DataRecordShortTextField dr_user_name = new DataRecordShortTextField("ltr", false, 60, 128);
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

        string sTable = theRecord.GetTableHTML("adm_account_change_pass.aspx?submited=1");

        return sTable;
    }
}
