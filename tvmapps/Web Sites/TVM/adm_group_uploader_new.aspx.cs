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

public partial class adm_group_uploader_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_group_uploader.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_group_uploader.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
            {

                DBManipulator.DoTheWork();
                return;
            }
            Int32 nMenuID = 0;

            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);

            if (Request.QueryString["uploader_id"] != null &&
                Request.QueryString["uploader_id"].ToString() != "")
            {
                Session["uploader_id"] = int.Parse(Request.QueryString["uploader_id"].ToString());
            }
            else
                Session["uploader_id"] = 0;
        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Pic Uplaoder";
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

        object t = null; ;
        if (Session["uploader_id"] != null && Session["uploader_id"].ToString() != "" && int.Parse(Session["uploader_id"].ToString()) != 0)
            t = Session["uploader_id"];

        string sBack = "adm_group_uploader.aspx?search_save=1";

        DBRecordWebEditor theRecord = new DBRecordWebEditor("uploaders", "adm_table_pager", sBack, "", "id", t, sBack, "");

        string sQuery = "";

        DataRecordDropDownField dr_uploader_type = new DataRecordDropDownField("lu_uploader_types", "NAME", "id", "", null, 60, true);
        sQuery = "select DESCRIPTION as txt,id as id from lu_uploader_types";
        dr_uploader_type.SetSelectsQuery(sQuery);
        dr_uploader_type.Initialize("Uploader Type:", "adm_table_header_nbg", "FormInput", "UPLOADER_TYPE_ID", false);
        theRecord.AddRecord(dr_uploader_type);

        DataRecordShortTextField dr_address_ = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_address_.Initialize("FTP Address / Bucket Name", "adm_table_header_nbg", "FormInput", "ADDRESS", true);
        theRecord.AddRecord(dr_address_);

        DataRecordShortTextField dr_user = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_user.Initialize("User Name / Access Key ", "adm_table_header_nbg", "FormInput", "USERNAME", true);
        theRecord.AddRecord(dr_user);

        DataRecordShortTextField dr_pass = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_pass.Initialize("Password / Secret Key", "adm_table_header_nbg", "FormInput", "PASSWORD", true);
        dr_pass.SetPassword();
        theRecord.AddRecord(dr_pass);

        DataRecordShortTextField dr_prefix = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_prefix.Initialize("Prefix", "adm_table_header_nbg", "FormInput", "PREFIX", false);
        theRecord.AddRecord(dr_prefix);

        DataRecordDropDownField dr_region = new DataRecordDropDownField("regions", "NAME", "id", "", null, 60, true);
        sQuery = "select name as txt,id as id from regions where id <> 0";
        dr_region.SetSelectsQuery(sQuery);
        dr_region.Initialize("Endpoint Region", "adm_table_header_nbg", "FormInput", "REGION_ID", false);
        theRecord.AddRecord(dr_region);

        //DataRecordShortTextField dr_conn = new DataRecordShortTextField("ltr", true, 60, 128);
        //dr_conn.Initialize("Max Connections", "adm_table_header_nbg", "FormInput", "MAX_CONNECTIONS", false);
        //theRecord.AddRecord(dr_conn);

        //DataRecordShortTextField dr_jobs = new DataRecordShortTextField("ltr", true, 60, 128);
        //dr_jobs.Initialize("Max Jobs", "adm_table_header_nbg", "FormInput", "MAX_JOBS", false);
        //theRecord.AddRecord(dr_jobs);

        //DataRecordShortTextField dr_path = new DataRecordShortTextField("ltr", true, 60, 128);
        //dr_path.Initialize("Base Path", "adm_table_header_nbg", "FormInput", "BASE_PATH", false);
        //theRecord.AddRecord(dr_path);

        string sTable = theRecord.GetTableHTML("adm_group_uploader_new.aspx?submited=1");

        return sTable;
    }
}
