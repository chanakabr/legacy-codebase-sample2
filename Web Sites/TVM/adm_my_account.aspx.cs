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

public partial class adm_my_account : System.Web.UI.Page
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
        m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                string sID = Request.Form["id"].ToString();
                Int32 nAccountID = LoginManager.GetLoginID();
                if (nAccountID != int.Parse(sID))
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
                string sEmail = Request.Form["1_val"].ToString();
                string sOldPass = Request.Form["2_val"].ToString();
                string sPass1 = Request.Form["3_val"].ToString();
                string sPass2 = Request.Form["4_val"].ToString();
                //if (sPass1 == "" || sPass2 == "")
                    //Session["error_msg"] = "Empty passwwords";
                if (sPass1 != sPass2)
                    Session["error_msg"] = "Password and confirmation passwords are not equal";
                else if (sOldPass == "")
                    Session["error_msg"] = "Old password incorrect";
                else
                {
                    bool bStrongPass = LoginManager.IsPasswordStrong(sPass1);
                    if (bStrongPass == true)
                    {
                        bool bSuccess = true; ;
                        if (sPass1 != "")
                            bSuccess = LoginManager.ChangeUserPassword(int.Parse(sID), sOldPass, sPass1);
                        if (bSuccess == true)
                        {
                            Session["ok_msg"] = "Password changed successfuly";
                            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("accounts");
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("email_add", "=", sEmail);
                            updateQuery += "where ";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", int.Parse(sID));
                            updateQuery.Execute();
                            updateQuery.Finish();
                            updateQuery = null;
                        }
                        else
                            Session["error_msg"] = "Wrong current password";
                    }
                    else
                        Session["error_msg"] = "Password not strong";
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
        Response.Write(PageUtils.GetPreHeader() + ": My account settings");
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
        object t = LoginManager.GetLoginID();
        DBRecordWebEditor theRecord = new DBRecordWebEditor("accounts", "adm_table_pager", "adm_my_account.aspx", "", "ID", t, "adm_my_account.aspx", "");

        DataRecordShortTextField dr_user_name = new DataRecordShortTextField("ltr", false, 60, 128);
        dr_user_name.Initialize("Username", "adm_table_header_nbg", "FormInput", "USERNAME", false);
        theRecord.AddRecord(dr_user_name);

        DataRecordShortTextField dr_email = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_email.Initialize("Email Address", "adm_table_header_nbg", "FormInput", "EMAIL_ADD", true);
        theRecord.AddRecord(dr_email);

        DataRecordShortTextField dr_old_pass = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_old_pass.SetPassword();
        dr_old_pass.Initialize("Current password", "adm_table_header_nbg", "FormInput", "PASSWORD", false);
        dr_old_pass.SetValue("");
        theRecord.AddRecord(dr_old_pass);

        DataRecordShortTextField dr_pass1_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_pass1_name.SetPassword();
        dr_pass1_name.Initialize("Password", "adm_table_header_nbg", "FormInput", "PASSWORD", false);
        dr_pass1_name.SetValue("");
        theRecord.AddRecord(dr_pass1_name);

        DataRecordShortTextField dr_pass2_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_pass2_name.SetPassword();
        dr_pass2_name.Initialize("Password confirmation", "adm_table_header_nbg", "FormInput", "PASSWORD", false);
        dr_pass2_name.SetValue("");
        theRecord.AddRecord(dr_pass2_name);

        string sTable = theRecord.GetTableHTML("adm_my_account.aspx?submited=1");

        return sTable;
    }
}
