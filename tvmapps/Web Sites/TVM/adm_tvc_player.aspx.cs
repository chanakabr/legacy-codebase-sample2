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

public partial class adm_tvc_player : System.Web.UI.Page
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
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 2, true);
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

    protected void GetPlayerDetails(Int32 nGroupID, ref string sPlayerUN , ref string sPlayerPass)
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
        string sBack = "adm_tvc_player.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("tvc", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        string sPUN = "";
        string sPPass = "";
        GetPlayerDetails(nGroupID, ref sPUN, ref sPPass);

        DataRecordUploadField dr_skin = new DataRecordUploadField(36, "skins", false);
        dr_skin.Initialize("Big UMA Skin", "adm_table_header_nbg", "FormInput", "PLAYER_SKIN_FILE", false);
        theRecord.AddRecord(dr_skin);

        DataRecordUploadField dr_skin_mini = new DataRecordUploadField(36, "skins", false);
        dr_skin_mini.Initialize("Mini UMA Skin", "adm_table_header_nbg", "FormInput", "MINI_PLAYER_SKIN_FILE", false);
        theRecord.AddRecord(dr_skin_mini);

        DataRecordUploadField dr_xml_config = new DataRecordUploadField(36, "skins", false);
        dr_xml_config.Initialize("UMA XML Config File", "adm_table_header_nbg", "FormInput", "PLAYER_CONFIG_FILE", false);
        theRecord.AddRecord(dr_xml_config);

        DataRecordUploadField dr_lang_xml_config = new DataRecordUploadField(36, "skins", false);
        dr_lang_xml_config.Initialize("UMA Language XML Config File", "adm_table_header_nbg", "FormInput", "PLAYER_LANG_CONFIG_FILE", false);
        theRecord.AddRecord(dr_lang_xml_config);

        DataRecordUploadField dr_trns_config = new DataRecordUploadField(36, "skins", false);
        dr_trns_config.Initialize("Site Texts XML Config File", "adm_table_header_nbg", "FormInput", "TRANSLATION_CONFIG_FILE", false);
        theRecord.AddRecord(dr_trns_config);

        DataRecordShortTextField dr_file_format = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_file_format.Initialize("File Format (FLV/WMV)", "adm_table_header_nbg", "FormInput", "FILE_FORMAT", true);
        theRecord.AddRecord(dr_file_format);

        DataRecordCheckBoxField dr_auto_play = new DataRecordCheckBoxField(true);
        dr_auto_play.Initialize("Auto Play", "adm_table_header_nbg", "FormInput", "AUTO_PLAY", false);
        theRecord.AddRecord(dr_auto_play);



        DataRecordTVMChannelCategoryField tree_category_id = new DataRecordTVMChannelCategoryField(true, sPUN, sPPass);
        tree_category_id.Initialize("Tree Category ID", "adm_table_header_nbg", "FormInput", "PLAYER_TREE_CATEGORY_ID", false);
        tree_category_id.SetDefault(0);
        theRecord.AddRecord(tree_category_id);

        DataRecordTVMChannelCategoryField menu_category_id = new DataRecordTVMChannelCategoryField(true, sPUN, sPPass);
        menu_category_id.Initialize("Menu Category ID", "adm_table_header_nbg", "FormInput", "PLAYER_MENU_CATEGORY_ID", false);
        menu_category_id.SetDefault(0);
        theRecord.AddRecord(menu_category_id);

        DataRecordTVMChannelCategoryField start_channel_id = new DataRecordTVMChannelCategoryField(false, sPUN, sPPass);
        start_channel_id.Initialize("Start Channel ID", "adm_table_header_nbg", "FormInput", "PLAYER_START_CHANNEL_ID", false);
        start_channel_id.SetDefault(0);
        theRecord.AddRecord(start_channel_id);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_tvc_player.aspx?submited=1");
        return sTable;
    }
}
