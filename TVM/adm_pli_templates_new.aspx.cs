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

public partial class adm_pli_templates_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsPagePermitted("adm_pli_templates.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsActionPermittedOnPage("adm_pli_templates.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(11, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork();
                return;
            }

            if (Request.QueryString["pli_template_id"] != null &&
                Request.QueryString["pli_template_id"].ToString() != "")
            {
                Session["pli_template_id"] = int.Parse(Request.QueryString["pli_template_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("play_list_items_templates_types", "group_id", int.Parse(Session["pli_template_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["pli_template_id"] = 0;
        }
    }

    public void GetHeader()
    {
        if (Session["pli_template_id"] != null && Session["pli_template_id"].ToString() != "" && int.Parse(Session["pli_template_id"].ToString()) != 0)
            Response.Write(PageUtils.GetPreHeader() + ":Playlist Template - Edit");
        else
            Response.Write(PageUtils.GetPreHeader() + ":Playlist Template - New");
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
        if (Session["pli_template_id"] != null && Session["pli_template_id"].ToString() != "" && int.Parse(Session["pli_template_id"].ToString()) != 0)
            t = Session["pli_template_id"];
        DBRecordWebEditor theRecord = new DBRecordWebEditor("play_list_items_templates_types", "adm_table_pager", "adm_pli_templates.aspx", "", "ID", t, "adm_pli_templates.aspx", "pli_template_id");

        Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Type Name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_name);
        
        DataRecordDropDownField dr_type_pre = new DataRecordDropDownField("ads_companies", "ADS_COMPANY_NAME", "id", "group_id", nLogedInGroupID, 60, true);
        dr_type_pre.SetNoSelectStr("---");
        dr_type_pre.Initialize("Pre Provider", "adm_table_header_nbg", "FormInput", "COMMERCIAL_TYPE_PRE_ID", false);
        theRecord.AddRecord(dr_type_pre);

        DataRecordShortIntField dr_delta_pre = new DataRecordShortIntField(true, 3, 3);
        dr_delta_pre.Initialize("Pre playlist delta", "adm_table_header_nbg", "FormInput", "OUTER_COMMERCILA_PLI_DELTA_PRE", false);
        theRecord.AddRecord(dr_delta_pre);

        DataRecordCheckBoxField dr_pre_skip = new DataRecordCheckBoxField(true);
        dr_pre_skip.Initialize("Pre skip enabled", "adm_table_header_nbg", "FormInput", "OUTER_COMMERCIAL_SKIP_PRE", false);
        theRecord.AddRecord(dr_pre_skip);
        
        DataRecordDropDownField dr_type_poverlay = new DataRecordDropDownField("ads_companies", "ADS_COMPANY_NAME", "id", "group_id", nLogedInGroupID, 60, true);
        dr_type_poverlay.Initialize("Overlay Provider", "adm_table_header_nbg", "FormInput", "COMMERCIAL_TYPE_OVERLAY_ID", false);
        dr_type_poverlay.SetNoSelectStr("---");
        theRecord.AddRecord(dr_type_poverlay);

        DataRecordShortIntField dr_delta_overlay = new DataRecordShortIntField(true, 3, 3);
        dr_delta_overlay.Initialize("Overlay delta (sec)", "adm_table_header_nbg", "FormInput", "OUTER_COMMERCILA_PLI_DELTA_OVERLAY", false);
        theRecord.AddRecord(dr_delta_overlay);

        DataRecordShortIntField dr_overlay_start = new DataRecordShortIntField(true, 3, 3);
        dr_overlay_start.Initialize("Overlay start point(sec)", "adm_table_header_nbg", "FormInput", "OUTER_COMMERCILA_PLI_OVERLAY_START", false);
        theRecord.AddRecord(dr_overlay_start);
        
        DataRecordDropDownField dr_type_break = new DataRecordDropDownField("ads_companies", "ADS_COMPANY_NAME", "id" , "group_id", nLogedInGroupID, 60, true);
        dr_type_break.Initialize("Break Provider", "adm_table_header_nbg", "FormInput", "COMMERCIAL_TYPE_BREAK_ID", false);
        dr_type_break.SetNoSelectStr("---");
        theRecord.AddRecord(dr_type_break);

        DataRecordShortIntField dr_delta_break = new DataRecordShortIntField(true, 3, 3);
        dr_delta_break.Initialize("Break delta (sec)", "adm_table_header_nbg", "FormInput", "OUTER_COMMERCILA_PLI_DELTA_BREAK", false);
        theRecord.AddRecord(dr_delta_break);

        DataRecordShortIntField dr_break_start = new DataRecordShortIntField(true, 3, 3);
        dr_break_start.Initialize("Break start point(sec)", "adm_table_header_nbg", "FormInput", "OUTER_COMMERCILA_PLI_BREAK_START", false);
        theRecord.AddRecord(dr_break_start);
        
        DataRecordDropDownField dr_type_post = new DataRecordDropDownField("ads_companies", "ADS_COMPANY_NAME", "id", "group_id", nLogedInGroupID , 60, true);
        dr_type_post.Initialize("Post Provider", "adm_table_header_nbg", "FormInput", "COMMERCIAL_TYPE_POST_ID", false);
        dr_type_post.SetNoSelectStr("---");
        theRecord.AddRecord(dr_type_post);

        DataRecordShortIntField dr_delta_post = new DataRecordShortIntField(true, 3, 3);
        dr_delta_post.Initialize("Post playlist delta", "adm_table_header_nbg", "FormInput", "OUTER_COMMERCILA_PLI_DELTA_POST", false);
        theRecord.AddRecord(dr_delta_post);

        DataRecordCheckBoxField dr_post_skip = new DataRecordCheckBoxField(true);
        dr_post_skip.Initialize("Post skip enabled", "adm_table_header_nbg", "FormInput", "OUTER_COMMERCIAL_SKIP_POST", false);
        theRecord.AddRecord(dr_post_skip);

        DataRecordShortIntField dr_order_num = new DataRecordShortIntField(true, 3, 3);
        dr_order_num.Initialize("Order Number", "adm_table_header_nbg", "FormInput", "ORDER_NUM", true);
        dr_order_num.SetDefault(1);
        theRecord.AddRecord(dr_order_num);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("");
        return sTable;
    }
}
