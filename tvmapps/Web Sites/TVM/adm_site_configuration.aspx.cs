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

public partial class adm_site_configuration : System.Web.UI.Page
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
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    protected void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Site configuration");
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
        DBRecordWebEditor theRecord = new DBRecordWebEditor("site_configuration", "adm_table_pager", "adm_site_configuration.aspx", "", "ID", 1, "adm_site_configuration.aspx", "");
        DataRecordShortTextField dr_title = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_title.Initialize("Pages header", "adm_table_header_nbg", "FormInput", "HEADER", false);
        theRecord.AddRecord(dr_title);

        DataRecordShortTextField dr_key_words = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_key_words.Initialize("Key words", "adm_table_header_nbg", "FormInput", "KEY_WORDS", false);
        theRecord.AddRecord(dr_key_words);

        DataRecordShortTextField dr_adm_logo = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_adm_logo.Initialize("Admin logo", "adm_table_header_nbg", "FormInput", "ADMIN_LOGO", false);
        theRecord.AddRecord(dr_adm_logo);

        string sTable = theRecord.GetTableHTML("");

        return sTable;
    }
}
