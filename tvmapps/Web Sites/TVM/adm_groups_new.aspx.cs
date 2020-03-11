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

public partial class adm_groups_new : System.Web.UI.Page
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
            m_sMenu = TVinciShared.Menu.GetMainMenu(2, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["group_id"] != null &&
                Request.QueryString["group_id"].ToString() != "")
                Session["group_id"] = int.Parse(Request.QueryString["group_id"].ToString());
            else
            {
                Session["group_id"] = 0;
                if (PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("index.html");
                    return;
                }
            }

            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                Int32 nGroupID = DBManipulator.DoTheWork();
                if (Session["group_id"].ToString() == "0")
                {
                    CreateDefaultValuesForGroup(nGroupID);
                }
            }
        }
    }

    protected void InsertPlayer(Int32 nGroupID, string sName)
    {
        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("groups_passwords");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("USERNAME", "=", sName);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PASSWORD", "=", sName);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DOMAIN", "=", sName);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
        insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;
    }

    protected void InsertCDN(Int32 nGroupID)
    {
        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("streaming_companies");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STREAMING_COMPANY_NAME", "=", "Default CDN");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("VIDEO_BASE_URL", "=", "http://switch3.castup.net/cunet/gm.asp?ai=569&ak=null&ar=");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("THUMB_NAILS_BASE_URL", "=", "");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CDN_STR_ID", "=", "normal");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
        //insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
        insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;
    }

    protected void InsertPicSize(Int32 nGroupID, string sName, string sDescription , 
        Int32 nWidth , Int32 nHeight , Int32 nToCrop)
    {
        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("media_pics_sizes");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sName);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sDescription);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("WIDTH", "=", nWidth);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("HEIGHT", "=", nHeight);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("TO_CROP", "=", nToCrop);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
        //insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
        insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;
    }
    
    protected void CreateDefaultValuesForGroup(Int32 nGroupID)
    {
        string sName = ODBCWrapper.Utils.GetTableSingleVal("groups" , "group_name" , nGroupID).ToString();
        InsertPlayer(nGroupID, sName + "-main");
        InsertCDN(nGroupID);
        InsertPicSize(nGroupID, "catalog tn 4:3", "catalog tn 4:3", 105, 80, 1);
        InsertPicSize(nGroupID, "Carusel", "Carusel", 195, 150, 1);
        InsertPicSize(nGroupID, "catalog tn 16:9", "catalog tn 16:9", 105, 55, 1);
        InsertPicSize(nGroupID, "video tn", "video tn", 70, 52, 1);
        InsertPicSize(nGroupID, "video big", "video big", 590, 445, 1);
        InsertPicSize(nGroupID, "commercial", "commercial", 300, 250, 1);
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
        if (Session["group_id"] != null && Session["group_id"].ToString() != "" && Session["group_id"].ToString() != "0")
            sRet += " - Edit";
        else
            sRet += " - New";

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
        object t = null; ;
        if (Session["group_id"] != null && Session["group_id"].ToString() != "" && int.Parse(Session["group_id"].ToString()) != 0)
            t = Session["group_id"];
        string sBack = "adm_groups.aspx?search_save=1";
        if (Session["parent_group_id"] != null)
            sBack += "&parent_group_id=" + Session["parent_group_id"].ToString();
        DBRecordWebEditor theRecord = new DBRecordWebEditor("groups", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_group_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_group_name.Initialize("Group name", "adm_table_header_nbg", "FormInput", "GROUP_NAME", true);
        theRecord.AddRecord(dr_group_name);

        DataRecordOnePicBrowserField dr_pic = new DataRecordOnePicBrowserField();
        dr_pic.Initialize("Logo pic", "adm_table_header_nbg", "FormInput", "ADMIN_LOGO", true);
        theRecord.AddRecord(dr_pic);

        Int32 nParentID = LoginManager.GetLoginGroupID();
        if (Session["parent_group_id"] != null && Session["parent_group_id"].ToString() != "" && Session["parent_group_id"].ToString() != "0")
            nParentID = int.Parse(Session["parent_group_id"].ToString());

        DataRecordShortIntField dr_parent_group_id = new DataRecordShortIntField(false, 9, 9);
        dr_parent_group_id.Initialize("group id", "adm_table_header_nbg", "FormInput", "parent_group_id", false);
        dr_parent_group_id.SetValue(nParentID.ToString());
        theRecord.AddRecord(dr_parent_group_id);

        DataRecordCheckBoxField dr_block = new DataRecordCheckBoxField(true);
        dr_block.Initialize("Blocking enabled", "adm_table_header_nbg", "FormInput", "BLOCKS_ACTIVE", true);
        theRecord.AddRecord(dr_block);

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
        if (Session["group_id"] == null || Session["group_id"].ToString() == "" || int.Parse(Session["group_id"].ToString()) == 0)
            dr_pics_remote_base_url.SetValue("http://tvp.tvinci.com/pics/");
        theRecord.AddRecord(dr_pics_remote_base_url);

        DataRecordShortTextField dr_mail_serv = new DataRecordShortTextField("ltr", true, 30, 128);
        dr_mail_serv.Initialize("Mail server URL", "adm_table_header_nbg", "FormInput", "MAIL_SERVER", false);
        if (Session["group_id"] == null || Session["group_id"].ToString() == "" || int.Parse(Session["group_id"].ToString()) == 0)
            dr_mail_serv.SetValue("localhost");
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

        DataRecordUploadField dr_mail_template = new DataRecordUploadField(60, "mailtemplates", false);
        dr_mail_template.Initialize("Mail template file", "adm_table_header_nbg", "FormInput", "MAIL_TEMPLATE", false);
        theRecord.AddRecord(dr_mail_template);



        //Npvr filed - provider + quota
        DataRecordDropDownField dr_npvr_provider = new DataRecordDropDownField("lu_npvr_providers", "NAME", "id", "", null, 60, true);
        string sQuery = "select name as txt,id as id from lu_npvr_providers where status=1 and is_active = 1";
        dr_npvr_provider.SetSelectsQuery(sQuery);
        dr_npvr_provider.Initialize("NPVR Provider", "adm_table_header_nbg", "FormInput", "npvr_provider_id", false);
        string deafultNpvr = "NO NPVR";
        dr_npvr_provider.SetDefaultVal(deafultNpvr);
        theRecord.AddRecord(dr_npvr_provider);

        DataRecordShortIntField dr_npvr_quota = new DataRecordShortIntField(true, 30, 128);
        dr_npvr_quota.Initialize("NPVR Quota", "adm_table_header_nbg", "FormInput", "npvr_quota_in_seconds", false);
        theRecord.AddRecord(dr_npvr_quota);

        string sTable = theRecord.GetTableHTML("adm_groups_new.aspx?submited=1");

        return sTable;
    }
}
