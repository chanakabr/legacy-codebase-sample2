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

public partial class adm_mail_configuration : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.aspx");
        if (LoginManager.IsPagePermitted() == false)
            LoginManager.LogoutFromSite("login.aspx");
        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.aspx");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(5, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 2, true);
        }
    }

    protected void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Mailer configuration");
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
        t = 1;
        DBRecordWebEditor theRecord = new DBRecordWebEditor("site_configuration", "adm_table_pager", "adm_mail_configuration.aspx", "", "ID", 1, "adm_mail_configuration.aspx", "");
        DataRecordShortTextField dr_mail_serv = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_mail_serv.Initialize("Mail server URL", "adm_table_header_nbg", "FormInput", "MAIL_SERVER", true);
        theRecord.AddRecord(dr_mail_serv);

        DataRecordShortTextField dr_mail_un = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_mail_un.Initialize("Mail server - username", "adm_table_header_nbg", "FormInput", "MAIL_USER_NAME", true);
        theRecord.AddRecord(dr_mail_un);

        DataRecordShortTextField dr_mail_p = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_mail_p.Initialize("Mail server - password", "adm_table_header_nbg", "FormInput", "MAIL_PASSWORD", true);
        theRecord.AddRecord(dr_mail_p);

        string sTable = theRecord.GetTableHTML("");

        return sTable;
    }
}
