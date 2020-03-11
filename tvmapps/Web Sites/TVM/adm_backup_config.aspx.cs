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

public partial class adm_backup_config : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsPagePermitted("adm_backup_config.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsActionPermittedOnPage("adm_backup_config.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(4, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 2, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork();
                return;
            }
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ":CDN BackUp mechanism");
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
        object t = LoginManager.GetLoginGroupID();
        DBRecordWebEditor theRecord = new DBRecordWebEditor("groups", "adm_table_pager", "adm_backup_config.aspx", "", "ID", t, "adm_backup_config.aspx", "media_tag_type_id");

        DataRecordCheckBoxField dr_active = new DataRecordCheckBoxField(true);
        dr_active.Initialize("CDN BackUp Active", "adm_table_header_nbg", "FormInput", "CDN_BACKUP_ACTIVE", false);
        theRecord.AddRecord(dr_active);

        DataRecordShortIntField dr_max = new DataRecordShortIntField(true, 6, 6);
        dr_max.Initialize("Max Watch Per Min", "adm_table_header_nbg", "FormInput", "CDN_BACKUP_VAL", true);
        theRecord.AddRecord(dr_max);

        string sTable = theRecord.GetTableHTML("");
        return sTable;
    }
}
