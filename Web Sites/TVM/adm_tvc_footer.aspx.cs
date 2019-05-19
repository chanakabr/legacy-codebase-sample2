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
using System.Threading;
using TVinciShared;

public partial class adm_tvc_footer : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsPagePermitted() == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(15, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 3, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork();
                Int32 nGroupID = LoginManager.GetLoginGroupID();
                object oBaseSiteAdd = ODBCWrapper.Utils.GetTableSingleVal("tvc", "SITE_BASE_ADD", "group_id", "=", nGroupID);
                string sBaseSiteAdd = "";
                if (oBaseSiteAdd != DBNull.Value && oBaseSiteAdd != null)
                {
                    sBaseSiteAdd = oBaseSiteAdd.ToString();
                    if (sBaseSiteAdd.EndsWith("/") == false)
                        sBaseSiteAdd += "/";
                }
                Notifier tt = new Notifier(sBaseSiteAdd + "technical.aspx?Action=RefreshConfiguration", "");
                ThreadStart job = new ThreadStart(tt.NotifyGet);
                Thread thread = new Thread(job);
                System.Threading.Thread.Sleep(250);
                thread.Start();
                return;
            }
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ":TVC Players Initialization");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    protected void GetPlayerDetails(Int32 nGroupID, ref string sPlayerUN, ref string sPlayerPass)
    {
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select gp.USERNAME,gp.PASSWORD from groups_passwords gp where gp.status=1 and gp.is_active=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("gp.group_id", "=", nGroupID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                sPlayerUN = selectQuery.Table("query").DefaultView[0].Row["USERNAME"].ToString();
                sPlayerPass = selectQuery.Table("query").DefaultView[0].Row["PASSWORD"].ToString();
            }
        }
        selectQuery.Finish();
        selectQuery = null;
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        object t = null;
        object tvc_id = ODBCWrapper.Utils.GetTableSingleVal("tvc", "id", "group_id", "=", LoginManager.GetLoginGroupID());
        if (tvc_id != DBNull.Value && tvc_id != null)
            t = tvc_id;
        string sBack = "adm_tvc_footer.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("tvc", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        string sPUN = "";
        string sPPass = "";
        GetPlayerDetails(nGroupID, ref sPUN, ref sPPass);

        DataRecordTVMChannelCategoryField footer_category_id = new DataRecordTVMChannelCategoryField(true, sPUN, sPPass);
        footer_category_id.Initialize("Footer Category ID", "adm_table_header_nbg", "FormInput", "FOOTER_CATEGORY_ID", false);
        footer_category_id.SetDefault(0);
        theRecord.AddRecord(footer_category_id);

        DataRecordShortIntField dr_line_items = new DataRecordShortIntField(true, 3, 3);
        dr_line_items.Initialize("Shown Line Items", "adm_table_header_nbg", "FormInput", "FOOTER_LINE_ITEMS_CNT", false);
        theRecord.AddRecord(dr_line_items);

        DataRecordShortIntField dr_max_line_items = new DataRecordShortIntField(true, 3, 3);
        dr_max_line_items.Initialize("Max Line Items", "adm_table_header_nbg", "FormInput", "FOOTER_CHANNEL_MAX_ITEMS", false);
        theRecord.AddRecord(dr_max_line_items);

        DataRecordShortTextField dr_copywrite_text = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_copywrite_text.Initialize("Copywrite Line", "adm_table_header_nbg", "FormInput", "COPYWRITE_LINE", false);
        theRecord.AddRecord(dr_copywrite_text);

        DataRecordShortTextField dr_seo_line = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_seo_line.Initialize("SEO Line", "adm_table_header_nbg", "FormInput", "FOOTER_SEO_LINE", false);
        theRecord.AddRecord(dr_seo_line);

        DataRecordShortTextField dr_copywrite_link = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_copywrite_link.Initialize("Copywrite Link", "adm_table_header_nbg", "FormInput", "COPYWRITE_LINK", false);
        theRecord.AddRecord(dr_copywrite_link);

        DataRecordShortTextField dr_footer_link1_text = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_footer_link1_text.Initialize("Link 1 Text", "adm_table_header_nbg", "FormInput", "FOOTER_TEXT1", false);
        theRecord.AddRecord(dr_footer_link1_text);

        DataRecordShortTextField dr_footer_link1_link = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_footer_link1_link.Initialize("Link 1 URL", "adm_table_header_nbg", "FormInput", "FOOTER_LINK1", false);
        theRecord.AddRecord(dr_footer_link1_link);

        DataRecordShortTextField dr_footer_link2_text = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_footer_link2_text.Initialize("Link 2 Text", "adm_table_header_nbg", "FormInput", "FOOTER_TEXT2", false);
        theRecord.AddRecord(dr_footer_link2_text);

        DataRecordShortTextField dr_footer_link2_link = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_footer_link2_link.Initialize("Link 2 URL", "adm_table_header_nbg", "FormInput", "FOOTER_LINK2", false);
        theRecord.AddRecord(dr_footer_link2_link);

        DataRecordShortTextField dr_footer_link3_text = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_footer_link3_text.Initialize("Link 3 Text", "adm_table_header_nbg", "FormInput", "FOOTER_TEXT3", false);
        theRecord.AddRecord(dr_footer_link3_text);

        DataRecordShortTextField dr_footer_link3_link = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_footer_link3_link.Initialize("Link 3 URL", "adm_table_header_nbg", "FormInput", "FOOTER_LINK3", false);
        theRecord.AddRecord(dr_footer_link3_link);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_tvc_player.aspx?submited=1");
        return sTable;
    }
}
