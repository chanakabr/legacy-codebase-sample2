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

public partial class adm_commercials_config : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsPagePermitted("adm_commercials_config.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsActionPermittedOnPage("adm_commercials_config.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");
        //else if (PageUtils.IsTvinciUser() == false)
            //LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(11, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 5, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork();
                return;
            }
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ":Outer commercials systems configuration");
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
        DBRecordWebEditor theRecord = new DBRecordWebEditor("groups", "adm_table_pager", "adm_commercials_config.aspx", "", "ID", t, "adm_commercials_config.aspx", "media_tag_type_id");

        DataRecordCheckBoxField dr_active = new DataRecordCheckBoxField(true);
        dr_active.Initialize("Outer commercial enabled", "adm_table_header_nbg", "FormInput", "OUTER_COMMERCIAL_ACTIVE", false);
        theRecord.AddRecord(dr_active);

        DataRecordShortTextField dr_url = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_url.Initialize("Outer commercilal URL", "adm_table_header_nbg", "FormInput", "OUTER_COMMERCIAL_URL", false);
        theRecord.AddRecord(dr_url);

        DataRecordShortIntField dr_delta = new DataRecordShortIntField(true, 3, 3);
        dr_delta.Initialize("Commercial playlist delta", "adm_table_header_nbg", "FormInput", "OUTER_COMMERCILA_PLI_DELTA", true);
        theRecord.AddRecord(dr_delta);

        string sTable = theRecord.GetTableHTML("");
        return sTable;
    }
}
