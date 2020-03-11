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

public partial class adm_tvc_init : System.Web.UI.Page
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
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(15, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
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
                Notifier tt = new Notifier(sBaseSiteAdd + "technical.aspx?Action=RefreshConfiguration","");
                ThreadStart job = new ThreadStart(tt.NotifyGet);
                Thread thread = new Thread(job);
                System.Threading.Thread.Sleep(250);
                thread.Start();
                //tt.NotifyGet();
            }
        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Basic Initialization";
        Response.Write(sRet);
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
        object tvc_id = ODBCWrapper.Utils.GetTableSingleVal("tvc", "id", "group_id", "=", LoginManager.GetLoginGroupID());
        if (tvc_id != DBNull.Value && tvc_id != null)
            t = tvc_id;
        //object t = LoginManager.GetLoginGroupID();
        string sBack = "adm_tvc_init.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("tvc", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_base_add = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_base_add.Initialize("Site base address", "adm_table_header_nbg", "FormInput", "SITE_BASE_ADD", true);
        theRecord.AddRecord(dr_base_add);

        DataRecordShortTextField dr_video_page_header = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_video_page_header.Initialize("Video Page Header", "adm_table_header_nbg", "FormInput", "VIDEO_PAGE_HEADER", false);
        theRecord.AddRecord(dr_video_page_header);

        DataRecordShortTextField dr_catalog_page_header = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_catalog_page_header.Initialize("Catalog Page Header", "adm_table_header_nbg", "FormInput", "CATALOG_PAGE_HEADER", false);
        theRecord.AddRecord(dr_catalog_page_header);

        DataRecordShortTextField dr_all_header = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_all_header.Initialize("All header", "adm_table_header_nbg", "FormInput", "ALL_HEADER", false);
        theRecord.AddRecord(dr_all_header);

        DataRecordShortTextField dr_link_header = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_link_header.Initialize("Link header", "adm_table_header_nbg", "FormInput", "LINK_HEADER", false);
        theRecord.AddRecord(dr_link_header);

        DataRecordShortTextField dr_type_header = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_type_header.Initialize("Type header", "adm_table_header_nbg", "FormInput", "TYPE_HEADER", false);
        theRecord.AddRecord(dr_type_header);

        DataRecordShortIntField dr_page_size = new DataRecordShortIntField(true, 3, 3);
        dr_page_size.Initialize("Catalog Page Size", "adm_table_header_nbg", "FormInput", "CATALOG_PAGE_SIZE", false);
        theRecord.AddRecord(dr_page_size);

        DataRecordShortTextField dr_page_title = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_page_title.Initialize("Page title", "adm_table_header_nbg", "FormInput", "PAGE_TITLE", false);
        theRecord.AddRecord(dr_page_title);

        DataRecordShortTextField dr_page_desc = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_page_desc.Initialize("Page description", "adm_table_header_nbg", "FormInput", "PAGE_description", false);
        theRecord.AddRecord(dr_page_desc);

        DataRecordShortTextField dr_page_keywords = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_page_keywords.Initialize("Key Words(Seperate with comma)", "adm_table_header_nbg", "FormInput", "PAGE_KEY_WORDS", false);
        theRecord.AddRecord(dr_page_keywords);

        DataRecordShortTextField dr_player_bg = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_player_bg.Initialize("Player Background Color", "adm_table_header_nbg", "FormInput", "PLAYER_BG_COLOR", false);
        theRecord.AddRecord(dr_player_bg);

        DataRecordCheckBoxField dr_comments_enabled = new DataRecordCheckBoxField(true);
        dr_comments_enabled.Initialize("Comments enabled", "adm_table_header_nbg", "FormInput", "COMMENTS_ENABLED", false);
        theRecord.AddRecord(dr_comments_enabled);

        DataRecordCheckBoxField dr_comments_auto_approve = new DataRecordCheckBoxField(true);
        dr_comments_auto_approve.Initialize("Comments auto approve", "adm_table_header_nbg", "FormInput", "COMMENTS_AUTO_APPROVE", false);
        theRecord.AddRecord(dr_comments_auto_approve);

        DataRecordCheckBoxField dr_with_wmp = new DataRecordCheckBoxField(true);
        dr_with_wmp.Initialize("Does player support WMP", "adm_table_header_nbg", "FormInput", "WITH_WMP", false);
        theRecord.AddRecord(dr_with_wmp);

        DataRecordCheckBoxField dr_with_type = new DataRecordCheckBoxField(true);
        dr_with_type.Initialize("Does catalog page with Type box", "adm_table_header_nbg", "FormInput", "WITH_TYPE", false);
        theRecord.AddRecord(dr_with_type);

        DataRecordShortTextField dr_extra_info_code = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_extra_info_code.Initialize("Extran Info Code(FIELD;NAME | FIELD;NAME)", "adm_table_header_nbg", "FormInput", "EXTRAN_INFO_CODE", false);
        theRecord.AddRecord(dr_extra_info_code);

        DataRecordLongTextField dr_stat_script = new DataRecordLongTextField("ltr", true, 90, 8);
        dr_stat_script.Initialize("Statistics Script", "adm_table_header_nbg", "FormInput", "STAT_SCRIPT", false);
        theRecord.AddRecord(dr_stat_script);

        DataRecordLongTextField dr_meta_script = new DataRecordLongTextField("ltr", true, 90, 8);
        dr_meta_script.Initialize("Additional page meta", "adm_table_header_nbg", "FormInput", "META_SCRIPT", false);
        theRecord.AddRecord(dr_meta_script);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_tvc_init.aspx?submited=1");

        return sTable;
    }
}
