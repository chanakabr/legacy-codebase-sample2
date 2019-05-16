using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_ads_cdns_config_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsPagePermitted("adm_ads_cdns_config.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsActionPermittedOnPage("adm_ads_cdns_config.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");
        else if (PageUtils.IsTvinciUser() == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(4, true, ref nMenuID);
            if (Request.QueryString["ads_company_id"] != null &&
                Request.QueryString["ads_company_id"].ToString() != "")
            {
                Session["ads_company_id"] = int.Parse(Request.QueryString["ads_company_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("ads_companies", "group_id", int.Parse(Session["ads_company_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["ads_company_id"] = 0;

            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 2, false);
        }
    }

    public void GetHeader()
    {
        if (Session["ads_company_id"] != null && Session["ads_company_id"].ToString() != "" && int.Parse(Session["ads_company_id"].ToString()) != 0)
            Response.Write(PageUtils.GetPreHeader() + ":Ads companies - Edit");
        else
            Response.Write(PageUtils.GetPreHeader() + ":Ads companies - New");
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
        if (Session["ads_company_id"] != null && Session["ads_company_id"].ToString() != "" && int.Parse(Session["ads_company_id"].ToString()) != 0)
            t = Session["ads_company_id"];
        DBRecordWebEditor theRecord = new DBRecordWebEditor("ads_companies", "adm_table_pager", "adm_ads_cdns_config.aspx", "", "ID", t, "adm_ads_cdns_config.aspx", "ads_company_id");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "ADS_COMPANY_NAME", true);
        theRecord.AddRecord(dr_name);

        DataRecordShortTextField dr_base_video_url = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_base_video_url.Initialize("Base URL", "adm_table_header_nbg", "FormInput", "COMMERCIAL_URL", true);
        theRecord.AddRecord(dr_base_video_url);

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

        DataRecordDropDownField dr_action_code = new DataRecordDropDownField("lu_outer_comm_types", "DESCRIPTION", "id", "", null, 60, false);
        dr_action_code.Initialize("Action Type", "adm_table_header_nbg", "FormInput", "COMMERCIAL_TYPE_ID", true);
        theRecord.AddRecord(dr_action_code);

        string sTable = theRecord.GetTableHTML("");

        return sTable;
    }
}
