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

public partial class adm_my_group : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        
       if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        else if (LoginManager.IsPagePermitted("adm_my_group.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(2, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 2, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork();
                Int32 nGroupID = LoginManager.GetLoginGroupID();
                GroupsCacheManager.GroupManager groupManager = new GroupsCacheManager.GroupManager();
                groupManager.UpdateGroup(nGroupID);
            }
        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected string GetWhereAmIStr()
    {
        string sGroupName = LoginManager.GetLoginGroupName();
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        if (Session["parent_group_id"] != null && Session["parent_group_id"].ToString() != "" && Session["parent_group_id"].ToString() != "0")
            nGroupID = int.Parse(Session["parent_group_id"].ToString());

        string sRet = "";
        bool bFirst = true;
        Int32 nLast = 0;
        nLast = int.Parse(PageUtils.GetTableSingleVal("groups", "parent_group_id", LoginManager.GetLoginGroupID()).ToString());
        while (nGroupID != nLast)
        {
            Int32 nParentID = int.Parse(PageUtils.GetTableSingleVal("groups", "PARENT_GROUP_ID", nGroupID).ToString());
            string sHeader = PageUtils.GetTableSingleVal("groups", "group_name", nGroupID).ToString();
            if (bFirst == false)
                sRet = "<span style=\"cursor:pointer;\" onclick=\"document.location.href='adm_groups.aspx?parent_group_id=" + nParentID.ToString() + "';\">" + sHeader + " </span><span class=\"arrow\">&raquo; </span>" + sRet;
            else
                sRet = sHeader;
            bFirst = false;
            nGroupID = nParentID;
        }
        sRet = "Groups: " + sRet;
        return sRet;
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": " + GetWhereAmIStr();
        sRet += " - Edit";
        
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
        object t = LoginManager.GetLoginGroupID();
        //if (Session["group_id"] != null && Session["group_id"].ToString() != "" && int.Parse(Session["group_id"].ToString()) != 0)
            //t = Session["group_id"];
        string sBack = "adm_my_group.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("groups", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_group_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_group_name.Initialize("Group name", "adm_table_header_nbg", "FormInput", "GROUP_NAME", true);
        theRecord.AddRecord(dr_group_name);

        DataRecordShortTextField dr_base_url = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_base_url.Initialize("Group Base URL", "adm_table_header_nbg", "FormInput", "GROUP_BASE_URL", false);
        theRecord.AddRecord(dr_base_url);

        DataRecordShortTextField dr_notify_code = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_notify_code.Initialize("Group Notify Code", "adm_table_header_nbg", "FormInput", "GROUP_NOTIFY_CODE", false);
        theRecord.AddRecord(dr_notify_code);

        DataRecordShortTextField dr_vip_url = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_vip_url.Initialize("Group Country Code", "adm_table_header_nbg", "FormInput", "GROUP_COUNTRY_CODE", false);
        theRecord.AddRecord(dr_vip_url);

        DataRecordShortTextField dr_secret_code = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_secret_code.Initialize("Group Secret Code", "adm_table_header_nbg", "FormInput", "GROUP_SECRET_CODE", false);
        theRecord.AddRecord(dr_secret_code);

        bool isDownloadPicWithImageServer = false;
        string imageUrl = string.Empty;
        int picId = 0;

        if (ImageUtils.IsDownloadPicWithImageServer())
        {
            isDownloadPicWithImageServer = true;
            int groupId = LoginManager.GetLoginGroupID();
            imageUrl = GetGroupLogoPicImageUrl(groupId, out picId);
        }
        DataRecordOnePicBrowserField dr_pic = new DataRecordOnePicBrowserField("myGroupLogo", isDownloadPicWithImageServer, imageUrl, picId);
        dr_pic.Initialize("Logo pic", "adm_table_header_nbg", "FormInput", "ADMIN_LOGO", false);
        theRecord.AddRecord(dr_pic);

        if (ImageUtils.IsDownloadPicWithImageServer())
        {
            isDownloadPicWithImageServer = true;
            int groupId = LoginManager.GetLoginGroupID();
            imageUrl = GetGroupDefaultPicImageUrl(groupId, out picId);
        }
        DataRecordOnePicBrowserField dr_default_pic = new DataRecordOnePicBrowserField("myGroup", isDownloadPicWithImageServer, imageUrl, picId);
        dr_default_pic.Initialize("Default pic", "adm_table_header_nbg", "FormInput", "DEFAULT_PIC_ID", false);
        theRecord.AddRecord(dr_default_pic);

        Int32 nParentID = LoginManager.GetLoginGroupID();
        nParentID = int.Parse(PageUtils.GetTableSingleVal("groups" , "parent_group_id" , nParentID).ToString());

        DataRecordShortIntField dr_parent_group_id = new DataRecordShortIntField(false, 9, 9);
        dr_parent_group_id.Initialize("group id", "adm_table_header_nbg", "FormInput", "parent_group_id", false);
        dr_parent_group_id.SetValue(nParentID.ToString());
        theRecord.AddRecord(dr_parent_group_id);

        DataRecordCheckBoxField dr_ads = new DataRecordCheckBoxField(true);
        dr_ads.Initialize("Adds group", "adm_table_header_nbg", "FormInput", "IS_ADS", true);
        theRecord.AddRecord(dr_ads);

        DataRecordCheckBoxField dr_block = new DataRecordCheckBoxField(true);
        dr_block.Initialize("Blocking enabled", "adm_table_header_nbg", "FormInput", "BLOCKS_ACTIVE", true);
        theRecord.AddRecord(dr_block);

        //DataRecordCheckBoxField dr_ip_block = new DataRecordCheckBoxField(true);
        //dr_ip_block.Initialize("Admin from all IP", "adm_table_header_nbg", "FormInput", "OPEN_ALL_IP", true);
        //theRecord.AddRecord(dr_ip_block);

        DataRecordShortIntField dr_fictivic_group_id = new DataRecordShortIntField(true, 9, 9);
        dr_fictivic_group_id.Initialize("Fictivic Group ID", "adm_table_header_nbg", "FormInput", "FICTIVIC_GROUP_ID", false);
        theRecord.AddRecord(dr_fictivic_group_id);

        DataRecordShortIntField dr_commerce_group_id = new DataRecordShortIntField(true, 9, 9);
        dr_commerce_group_id.Initialize("Commerce Group ID", "adm_table_header_nbg", "FormInput", "COMMERCE_GROUP_ID", false);
        theRecord.AddRecord(dr_commerce_group_id);

        DataRecordShortTextField dr_pics_ftp = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_pics_ftp.Initialize("Pics FTP address", "adm_table_header_nbg", "FormInput", "PICS_FTP", false);
        theRecord.AddRecord(dr_pics_ftp);

        DataRecordShortTextField dr_pics_ftp_un = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_pics_ftp_un.Initialize("Pics FTP Username", "adm_table_header_nbg", "FormInput", "PICS_FTP_USERNAME", false);
        theRecord.AddRecord(dr_pics_ftp_un);

        DataRecordShortTextField dr_pics_ftp_pass = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_pics_ftp_pass.Initialize("Pics FTP Password", "adm_table_header_nbg", "FormInput", "PICS_FTP_PASSWORD", false);
        dr_pics_ftp_pass.SetPassword();
        theRecord.AddRecord(dr_pics_ftp_pass);

        DataRecordShortTextField dr_pics_remote_base_url = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_pics_remote_base_url.Initialize("Pics Remote Base URL", "adm_table_header_nbg", "FormInput", "PICS_REMOTE_BASE_URL", false);
        theRecord.AddRecord(dr_pics_remote_base_url);

        DataRecordShortTextField dr_reports_ftp = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_reports_ftp.Initialize("Financial Reports FTP address", "adm_table_header_nbg", "FormInput", "Reports_FTP", false);
        theRecord.AddRecord(dr_reports_ftp);

        string sQuery = "";
        DataRecordDropDownField dr_watch_permissions = new DataRecordDropDownField("watch_permissions_types", "NAME", "id", "", null, 60, true);
        sQuery = "select name as txt,id as id from watch_permissions_types where status=1 and group_id= " + LoginManager.GetLoginGroupID().ToString();
        dr_watch_permissions.SetSelectsQuery(sQuery);
        dr_watch_permissions.Initialize("Default Watch Permission Rule", "adm_table_header_nbg", "FormInput", "DEFAULT_WATCH_PERMISSION_TYPE_ID", false);
        theRecord.AddRecord(dr_watch_permissions);

        DataRecordDropDownField dr_block_rules = new DataRecordDropDownField("geo_block_types", "NAME", "id", "", null, 60, true);
        sQuery = "select name as txt,id as id from geo_block_types where status=1 and group_id= " + LoginManager.GetLoginGroupID().ToString();
        dr_block_rules.SetSelectsQuery(sQuery);
        dr_block_rules.Initialize("Default Geo block Rule", "adm_table_header_nbg", "FormInput", "DEFAULT_BLOCK_TEMPLATE_ID", false);
        theRecord.AddRecord(dr_block_rules);

        DataRecordDropDownField dr_players_rules = new DataRecordDropDownField("geo_block_types", "NAME", "id", "", null, 60, true);
        sQuery = "select name as txt,id as id from players_groups_types where status=1 and group_id= " + LoginManager.GetLoginGroupID().ToString();
        dr_players_rules.SetSelectsQuery(sQuery);
        dr_players_rules.Initialize("Default Players Rule", "adm_table_header_nbg", "FormInput", "DEFAULT_PLAYERS_RULES", false);
        theRecord.AddRecord(dr_players_rules);

        DataRecordDropDownField dr_pli_template = new DataRecordDropDownField("media_types", "NAME", "id", "", null, 60, true);
        sQuery = "select name as txt,id as id from play_list_items_templates_types where status=1 and is_active=1 and group_id= " + LoginManager.GetLoginGroupID().ToString() + " order by ORDER_NUM";
        dr_pli_template.SetSelectsQuery(sQuery);
        dr_pli_template.Initialize("Default Playlist Structure", "adm_table_header_nbg", "FormInput", "DEFAULT_PLAYLIST_TEMPLATE_ID", false);
        theRecord.AddRecord(dr_pli_template);

        DataRecordShortTextField dr_cache_clean_url = new DataRecordShortTextField("ltr", true, 30, 512);
        dr_cache_clean_url.Initialize("Clear Cache URLs(comma seperated)", "adm_table_header_nbg", "FormInput", "CACHE_CLEAR_URLS", false);
        theRecord.AddRecord(dr_cache_clean_url);

        DataRecordShortTextField dr_media_publish_url = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_media_publish_url.Initialize("Media Publish Notify URL", "adm_table_header_nbg", "FormInput", "MEDIA_NOTIFY_URL", false);
        theRecord.AddRecord(dr_media_publish_url);

        DataRecordShortTextField dr_tags_notify_url = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_tags_notify_url.Initialize("Tags Notify URL", "adm_table_header_nbg", "FormInput", "TAGS_NOTIFY_URL", false);
        theRecord.AddRecord(dr_tags_notify_url);

        DataRecordShortTextField dr_caching_server_url = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_caching_server_url.Initialize("Caching server URL", "adm_table_header_nbg", "FormInput", "CACHING_SERVER_URL", false);
        theRecord.AddRecord(dr_caching_server_url);

        DataRecordShortTextField dr_mail_serv = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_mail_serv.Initialize("Mail server URL", "adm_table_header_nbg", "FormInput", "MAIL_SERVER", false);
        theRecord.AddRecord(dr_mail_serv);

        DataRecordShortTextField dr_mail_un = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_mail_un.Initialize("Mail server - username", "adm_table_header_nbg", "FormInput", "MAIL_USER_NAME", false);
        theRecord.AddRecord(dr_mail_un);

        DataRecordShortTextField dr_mail_p = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_mail_p.Initialize("Mail server - password", "adm_table_header_nbg", "FormInput", "MAIL_PASSWORD", false);
        theRecord.AddRecord(dr_mail_p);

        DataRecordShortTextField dr_mail_from = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_mail_from.Initialize("Mail server - from name", "adm_table_header_nbg", "FormInput", "MAIL_FROM_NAME", false);
        theRecord.AddRecord(dr_mail_from);

        DataRecordShortTextField dr_mail_ret_add = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_mail_ret_add.Initialize("Mail server - return address", "adm_table_header_nbg", "FormInput", "MAIL_RET_ADD", false);
        theRecord.AddRecord(dr_mail_ret_add);

        DataRecordShortTextField dr_mail_subject = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_mail_subject.Initialize("Send to friend subject", "adm_table_header_nbg", "FormInput", "MAIL_SUBJECT", false);
        theRecord.AddRecord(dr_mail_subject);

        DataRecordUploadField dr_mail_template = new DataRecordUploadField(60, "mailtemplates", false);
        dr_mail_template.Initialize("Mail template file", "adm_table_header_nbg", "FormInput", "MAIL_TEMPLATE", false);
        theRecord.AddRecord(dr_mail_template);

        DataRecordDropDownField dr_lang = new DataRecordDropDownField("lu_languages", "name", "id", "", "", 60, false);
        dr_lang.Initialize("Main Language", "adm_table_header_nbg", "FormInput", "LANGUAGE_ID", false);
        theRecord.AddRecord(dr_lang);

        DataRecordMultiField dr_more_languages = new DataRecordMultiField("lu_languages", "id", "id", "group_extra_languages", "GROUP_ID", "LANGUAGE_ID", false, "ltr", 60, "tags");
        dr_more_languages.Initialize("More Languages", "adm_table_header_nbg", "FormInput", "NAME", false);
        dr_more_languages.SetOrderCollectionBy("name");        
        theRecord.AddRecord(dr_more_languages);
              

        DataRecordDropDownField dr_device_limits = new DataRecordDropDownField("groups_device_limitation_modules", "Name", "id", "group_id", LoginManager.GetLoginGroupID(), 60, false);
        dr_device_limits.Initialize("Default Device Limit", "adm_table_header_nbg", "FormInput", "max_device_limit", false);
        theRecord.AddRecord(dr_device_limits);

        //MEDIA
        DataRecordDropDownField dr_ratios = new DataRecordDropDownField("lu_pics_ratios", "ratio", "id", "", "", 60, false);
        dr_ratios.Initialize("VOD Main Ratio", "adm_table_header_nbg", "FormInput", "RATIO_ID", false);        
        theRecord.AddRecord(dr_ratios);

        DataRecordMultiField dr_more_ratios = new DataRecordMultiField("lu_pics_ratios", "id", "id", "group_ratios", "GROUP_ID", "RATIO_ID", false, "ltr", 60, "tags");
        dr_more_ratios.Initialize("More Ratios", "adm_table_header_nbg", "FormInput", "RATIO", false);
        theRecord.AddRecord(dr_more_ratios);

        //EPG
        DataRecordDropDownField dr_epgRatios = new DataRecordDropDownField("lu_pics_epg_ratios", "ratio", "id", "", "", 60, true);
        dr_epgRatios.Initialize("EPG Main Ratio", "adm_table_header_nbg", "FormInput", "EPG_RATIO_ID", false);
        theRecord.AddRecord(dr_epgRatios); 
        
        DataRecordMultiField dr_more_ratios_epg = new DataRecordMultiField("lu_pics_epg_ratios", "id", "id", "group_epg_ratios", "GROUP_ID", "RATIO_ID", false, "ltr", 60, "tags");
        dr_more_ratios_epg.Initialize("More EPG Ratios", "adm_table_header_nbg", "FormInput", "RATIO", false);        
        theRecord.AddRecord(dr_more_ratios_epg);

        DataRecordCheckBoxField dr_use_default_info_struct = new DataRecordCheckBoxField(true);
        dr_use_default_info_struct.Initialize("Use default info struct", "adm_table_header_nbg", "FormInput", "USE_DEFAULT_INFO_STRUCT", true);
        theRecord.AddRecord(dr_use_default_info_struct);

        DataRecordShortTextField dr_default_info_struct = new DataRecordShortTextField("ltr", true, 60, 900);
        dr_default_info_struct.Initialize("Default info struct XML", "adm_table_header_nbg", "FormInput", "DEFAULT_INFO_STRUCT", false);
        theRecord.AddRecord(dr_default_info_struct);

        //cancellation regulation
        DataRecordDropDownField dr_view_lc = new DataRecordDropDownField("lu_min_periods", "DESCRIPTION", "id", "", null, 60, true);
        dr_view_lc.Initialize("Default Waiver Period", "adm_table_header_nbg", "FormInput", "WAIVER_PERIOD", false);        
        theRecord.AddRecord(dr_view_lc);

        DataRecordCheckBoxField dr_dtt_regionalization = new DataRecordCheckBoxField(true);
        dr_dtt_regionalization.Initialize("Enable region filtering", "adm_table_header_nbg", "FormInput", "IS_REGIONALIZATION_ENABLED", false);
        theRecord.AddRecord(dr_dtt_regionalization);

        DataRecordDropDownField dr_region = new DataRecordDropDownField("linear_channels_regions", "name", "id", "group_id", t, 60, true);
        dr_region.Initialize("Default Region", "adm_table_header_nbg", "FormInput", "DEFAULT_REGION", false);
        theRecord.AddRecord(dr_region);

        DataRecordShortTextField dr_dateFormat = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_dateFormat.Initialize("Default date Format for email notifications", "adm_table_header_nbg", "FormInput", "date_email_format", false);
        theRecord.AddRecord(dr_dateFormat);

        DataRecordDropDownField dr_recommendation_engine = new DataRecordDropDownField("recommendation_engines", "name", "id", "group_id", t, 60, true);
        dr_recommendation_engine.Initialize("Default Recommendation Engine", "adm_table_header_nbg", "FormInput", "SELECTED_RECOMMENDATION_ENGINE", false);
        string recommendationEnginesQuery = "select name as txt,id as id from recommendation_engines where status=1 and group_id= " + t.ToString();
        dr_recommendation_engine.SetSelectsQuery(recommendationEnginesQuery);
        theRecord.AddRecord(dr_recommendation_engine);

        DataRecordShortTextField dr_imageServerUrl = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_imageServerUrl.Initialize("Image server URL", "adm_table_header_nbg", "FormInput", "IMAGE_SERVER_URL", false);
        theRecord.AddRecord(dr_imageServerUrl);

        DataRecordShortTextField dr_InternalImageServerUrl = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_InternalImageServerUrl.Initialize("Internal image server URL", "adm_table_header_nbg", "FormInput", "INTERNAL_IMAGE_SERVER_URL", false);
        theRecord.AddRecord(dr_InternalImageServerUrl);

        string sTable = theRecord.GetTableHTML("adm_my_group.aspx?submited=1");

        return sTable;
    }

    private string GetGroupDefaultPicImageUrl(int groupId, out int picId)
    {
        string imageUrl = string.Empty;
        string baseUrl = string.Empty;
        int ratioId = 0;
        int version = 0;
        picId = 0;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select p.RATIO_ID, p.BASE_URL, p.ID, p.version from pics p left join groups g on g.Default_PIC_ID = p.ID where p.STATUS in (0, 1) and g.id = " + groupId.ToString();

        if (selectQuery.Execute("query", true) != null && selectQuery.Table("query").DefaultView != null && selectQuery.Table("query").DefaultView.Count > 0)
        {
            baseUrl = ODBCWrapper.Utils.GetSafeStr(selectQuery.Table("query").DefaultView[0].Row["BASE_URL"]);
            ratioId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row["RATIO_ID"]);
            picId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row["ID"]);
            version = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row["version"]);
            int parentGroupID = DAL.UtilsDal.GetParentGroupID(groupId);

            imageUrl = PageUtils.BuildVodUrl(parentGroupID, baseUrl, ratioId, version);
        }

        return imageUrl;
    }

    private string GetGroupLogoPicImageUrl(int groupId, out int picId)
    {
        string imageUrl = string.Empty;
        string baseUrl = string.Empty;
        int ratioId = 0;
        int version = 0;
        picId = 0;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select  p.RATIO_ID, p.BASE_URL, p.ID,  p.VERSION  from pics p left join groups g on g.ADMIN_LOGO = p.ID where p.STATUS in (0, 1) and g.id = " + groupId.ToString();

        if (selectQuery.Execute("query", true) != null && selectQuery.Table("query").DefaultView != null && selectQuery.Table("query").DefaultView.Count > 0)
        {
            baseUrl = ODBCWrapper.Utils.GetSafeStr(selectQuery.Table("query").DefaultView[0].Row["BASE_URL"]);
            ratioId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row["RATIO_ID"]);
            version = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row["VERSION"]);
            picId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row["ID"]);
            int parentGroupID = DAL.UtilsDal.GetParentGroupID(groupId);

            imageUrl = PageUtils.BuildVodUrl(parentGroupID, baseUrl, ratioId, version);
        }

        return imageUrl;
    }
}
