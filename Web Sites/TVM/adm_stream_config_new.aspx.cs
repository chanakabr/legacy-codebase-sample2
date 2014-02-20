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

public partial class adm_stream_config_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsPagePermitted("adm_stream_config.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsActionPermittedOnPage("adm_stream_config.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");
        else if (PageUtils.IsTvinciUser() == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(4, true, ref nMenuID);
            if (Request.QueryString["stream_company_id"] != null &&
                Request.QueryString["stream_company_id"].ToString() != "")
            {
                Session["stream_company_id"] = int.Parse(Request.QueryString["stream_company_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("streaming_companies", "group_id", int.Parse(Session["stream_company_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["stream_company_id"] = 0;

            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
        }
    }

    public void GetHeader()
    {
        if (Session["stream_company_id"] != null && Session["stream_company_id"].ToString() != "" && int.Parse(Session["stream_company_id"].ToString()) != 0)
            Response.Write(PageUtils.GetPreHeader() + ":Streaming companies - Edit");
        else
            Response.Write(PageUtils.GetPreHeader() + ":Streaming companies - New");
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
        if (Session["stream_company_id"] != null && Session["stream_company_id"].ToString() != "" && int.Parse(Session["stream_company_id"].ToString()) != 0)
            t = Session["stream_company_id"];
        DBRecordWebEditor theRecord = new DBRecordWebEditor("streaming_companies", "adm_table_pager", "adm_stream_config.aspx", "", "ID", t, "adm_stream_config.aspx", "stream_company_id");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "STREAMING_COMPANY_NAME", true);
        theRecord.AddRecord(dr_name);

        DataRecordShortTextField dr_base_video_url = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_base_video_url.Initialize("Base video URL", "adm_table_header_nbg", "FormInput", "VIDEO_BASE_URL", true);
        theRecord.AddRecord(dr_base_video_url);

        DataRecordShortTextField dr_base_notify_url = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_base_notify_url.Initialize("Base Notify URL", "adm_table_header_nbg", "FormInput", "CDN_BASE_NOTIFY", false);
        theRecord.AddRecord(dr_base_notify_url);

        bool bVisible = PageUtils.IsTvinciUser();
        if (bVisible == true)
        {
            DataRecordDropDownField dr_groups = new DataRecordDropDownField("groups", "GROUP_NAME", "id", "", null, 60, false);
            dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", true);
            dr_groups.SetWhereString("status<>2 and id " + PageUtils.GetAllChildGroupsStr());
            theRecord.AddRecord(dr_groups);
        }
        else
        {
            DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
            dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", true);
            dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
            theRecord.AddRecord(dr_groups);
        }

        //DataRecordShortTextField dr_base_adm_video_url = new DataRecordShortTextField("ltr", true, 60, 128);
        //dr_base_adm_video_url.Initialize("Base video", "adm_table_header_nbg", "FormInput", "ADM_VIDEO_BASE_URL", true);
        //theRecord.AddRecord(dr_base_adm_video_url);

        //DataRecordShortTextField dr_base_tn_url = new DataRecordShortTextField("ltr", true, 60, 128);
        //dr_base_tn_url.Initialize("כתובת בסיס - טאמבנייל", "adm_table_header_nbg", "FormInput", "THUMB_NAILS_BASE_URL", false);
        //theRecord.AddRecord(dr_base_tn_url);

        DataRecordDropDownField dr_action_code = new DataRecordDropDownField("lu_cdn_type", "DESCRIPTION_VAL", "DESCRIPTION", "", null, 60, false);
        dr_action_code.Initialize("Action Type", "adm_table_header_nbg", "FormInput", "CDN_STR_ID", true);
        dr_action_code.SetFieldType("string");
        theRecord.AddRecord(dr_action_code);

        //DataRecordShortTextField dr_cdn_str = new DataRecordShortTextField("ltr", true, 60, 128);
        //dr_cdn_str.Initialize("Action Code", "adm_table_header_nbg", "FormInput", "CDN_STR_ID", false);
        //theRecord.AddRecord(dr_cdn_str);

        string sTable = theRecord.GetTableHTML("");

        return sTable;
    }
}
