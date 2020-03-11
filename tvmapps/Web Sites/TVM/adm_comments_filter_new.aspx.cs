using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using TVinciShared;

public partial class adm_comments_filter_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsPagePermitted("adm_comments_filter.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsActionPermittedOnPage("adm_comments_filter.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(11, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 3, true);
        }

        if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
        {
            DBManipulator.DoTheWork();
            return;
        }

        if (Request.QueryString["comment_filter_id"] != null &&
            Request.QueryString["comment_filter_id"].ToString() != "")
        {
            Session["comment_filter_id"] = int.Parse(Request.QueryString["comment_filter_id"].ToString());
            Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("comment_filters", "group_id", int.Parse(Session["comment_filter_id"].ToString())).ToString());
            Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
            if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }
        }
        else
            Session["comment_filter_id"] = 0;

    }

    public void GetHeader()
    {
        if (Session["comment_filter_id"] != null && Session["comment_filter_id"].ToString() != "" && int.Parse(Session["comment_filter_id"].ToString()) != 0)
            Response.Write(PageUtils.GetPreHeader() + ":Comments filter - Edit");
        else
            Response.Write(PageUtils.GetPreHeader() + ":Comments filter - New");
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
        if (Session["comment_filter_id"] != null && Session["comment_filter_id"].ToString() != "" && int.Parse(Session["comment_filter_id"].ToString()) != 0)
            t = Session["comment_filter_id"];
        DBRecordWebEditor theRecord = new DBRecordWebEditor("comment_filters", "adm_table_pager", "adm_comments_filter.aspx?search_save=1", "", "ID", t, "adm_comments_filter.aspx", "comment_filter_id");

        DataRecordShortTextField dr_text = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_text.Initialize("Text", "adm_table_header_nbg", "FormInput", "Text", true);
        theRecord.AddRecord(dr_text);

        DataRecordCheckBoxField dr_wildcard = new DataRecordCheckBoxField(true);
        dr_wildcard.Initialize("Wild Card", "adm_table_header_nbg", "FormInput", "wildcard", false);
        theRecord.AddRecord(dr_wildcard);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_comments_filter_new.aspx?submited=1");
        return sTable;
    }
}
