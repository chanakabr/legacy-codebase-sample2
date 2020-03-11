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

public partial class adm_commercials_files_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsPagePermitted("adm_commercials.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsActionPermittedOnPage("adm_commercials.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(13, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 2, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork();
                return;
            }

            if (Session["commercial_id"] == null || Session["commercial_id"].ToString() == "" || Session["commercial_id"].ToString() == "0")
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }

            if (Request.QueryString["commercial_file_id"] != null && Request.QueryString["commercial_file_id"].ToString() != "")
            {
                Session["commercial_file_id"] = int.Parse(Request.QueryString["commercial_file_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("commercial", "group_id", int.Parse(Session["commercial_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["commercial_file_id"] = "0";
        }
    }

    public void GetHeader()
    {
        if (Session["commercial_file_id"] != null && Session["commercial_file_id"].ToString() != "" && int.Parse(Session["commercial_file_id"].ToString()) != 0)
            Response.Write(PageUtils.GetPreHeader() + ":Commercial File - Edit");
        else
            Response.Write(PageUtils.GetPreHeader() + ":Commercial File - New");
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
        if (Session["commercial_file_id"] != null && Session["commercial_file_id"].ToString() != "" && int.Parse(Session["commercial_file_id"].ToString()) != 0)
            t = Session["commercial_file_id"];
        string sBack = "adm_commercials_files.aspx?commercial_id=" + Session["commercial_id"].ToString();
        DBRecordWebEditor theRecord = new DBRecordWebEditor("commercial_files", "adm_table_pager", sBack, "", "ID", t, sBack, "commercial_id");

        DataRecordDropDownField dr_comm_type = new DataRecordDropDownField("lu_commercial_types", "description", "id", "", null, 60, false);
        dr_comm_type.Initialize("Commercial Type", "adm_table_header_nbg", "FormInput", "commercial_TYPE_ID", false);
        theRecord.AddRecord(dr_comm_type);

        DataRecordDropDownField dr_media_type = new DataRecordDropDownField("lu_media_types", "description", "id", "", null, 60, false);
        dr_media_type.Initialize("Media Type", "adm_table_header_nbg", "FormInput", "MEDIA_TYPE_ID", false);
        dr_media_type.SetOrderBy("id");
        theRecord.AddRecord(dr_media_type);

        DataRecordDropDownField dr_media_quality = new DataRecordDropDownField("lu_media_quality", "description", "id", "", null, 60, false);
        dr_media_quality.Initialize("Media Quality", "adm_table_header_nbg", "FormInput", "MEDIA_QUALITY_ID", false);
        dr_media_quality.SetOrderBy("id desc");
        theRecord.AddRecord(dr_media_quality);

        DataRecordRadioField dr_stram_suplier = new DataRecordRadioField("streaming_companies", "STREAMING_COMPANY_NAME", "id", "status", 1);
        string sQuery = "select STREAMING_COMPANY_NAME as txt,id as id from streaming_companies where status=1 and group_id= " + LoginManager.GetLoginGroupID().ToString();
        dr_stram_suplier.SetSelectsQuery(sQuery);
        dr_stram_suplier.Initialize("CDN", "adm_table_header_nbg", "FormInput", "STREAMING_SUPLIER_ID", true);
        dr_stram_suplier.SetDefault(0);
        theRecord.AddRecord(dr_stram_suplier);

        DataRecordShortTextField dr_streaming_code = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_streaming_code.Initialize("CDN Code", "adm_table_header_nbg", "FormInput", "STREAMING_CODE", true);
        theRecord.AddRecord(dr_streaming_code);

        DataRecordMediaViewerField dr_viewer = new DataRecordMediaViewerField("", int.Parse(Session["commercial_file_id"].ToString()));
        dr_viewer.VideoTable("commercial_files");
        dr_viewer.Initialize("Main Video", "adm_table_header_nbg", "FormInput", "STREAMING_CODE", false);
        theRecord.AddRecord(dr_viewer);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_commercial_id = new DataRecordShortIntField(false, 9, 9);
        dr_commercial_id.Initialize("Commercial ID", "adm_table_header_nbg", "FormInput", "commercial_id", false);
        dr_commercial_id.SetValue(Session["commercial_id"].ToString());
        theRecord.AddRecord(dr_commercial_id);

        string sTable = theRecord.GetTableHTML("adm_commercials_files_new.aspx?submited=1");
        return sTable;
    }
}
