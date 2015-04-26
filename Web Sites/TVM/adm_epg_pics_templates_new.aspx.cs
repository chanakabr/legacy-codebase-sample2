using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_epg_pics_templates_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsPagePermitted("adm_epg_pics_templates.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsActionPermittedOnPage("adm_epg_pics_templates.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");      

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(11, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 4, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork();
                return;
            }

            if (Request.QueryString["epg_pic_size_id"] != null &&
                Request.QueryString["epg_pic_size_id"].ToString() != "")
            {
                Session["epg_pic_size_id"] = int.Parse(Request.QueryString["epg_pic_size_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("EPG_pics_sizes", "group_id", int.Parse(Session["epg_pic_size_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["epg_pic_size_id"] = 0;
        }
    }

    public void GetHeader()
    {
        if (Session["epg_pic_size_id"] != null && Session["epg_pic_size_id"].ToString() != "" && int.Parse(Session["epg_pic_size_id"].ToString()) != 0)
            Response.Write(PageUtils.GetPreHeader() + ": EPG Pics sizes - Edit");
        else
            Response.Write(PageUtils.GetPreHeader() + ": EPG Pics sizes - New");
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
        if (Session["epg_pic_size_id"] != null && Session["epg_pic_size_id"].ToString() != "" && int.Parse(Session["epg_pic_size_id"].ToString()) != 0)
            t = Session["epg_pic_size_id"];
        DBRecordWebEditor theRecord = new DBRecordWebEditor("epg_pics_sizes", "adm_table_pager", "adm_epg_pics_templates.aspx", "", "ID", t, "adm_epg_pics_templates.aspx", "epg_pic_size_id");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "Name", true);
        theRecord.AddRecord(dr_name);

        DataRecordShortTextField dr_description = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_description.Initialize("Description", "adm_table_header_nbg", "FormInput", "DESCRIPTION", true);
        theRecord.AddRecord(dr_description);

        DataRecordShortIntField dr_width = new DataRecordShortIntField(true, 4, 4);
        dr_width.Initialize("Width", "adm_table_header_nbg", "FormInput", "Width", true);
        theRecord.AddRecord(dr_width);

        DataRecordShortIntField dr_height = new DataRecordShortIntField(true, 4, 4);
        dr_height.Initialize("Height", "adm_table_header_nbg", "FormInput", "Height", true);
        theRecord.AddRecord(dr_height);

        DataRecordDropDownField dr_ratios = new DataRecordDropDownField("lu_groups_ratios", "ratio", "id", "", null, 60, false);
        dr_ratios.Initialize("Ratio", "adm_table_header_nbg", "FormInput", "RATIO_ID", true);
        dr_ratios.SetSelectsQuery("select lur.ratio as 'txt', lur.id from  lu_pics_epg_ratios lur, groups g where g.id = " + LoginManager.GetLoginGroupID() + " and lur.id = g.ratio_id" + " UNION select lur.ratio as 'txt', lur.id from lu_pics_epg_ratios lur, group_epg_ratios gr where gr.group_id = " + LoginManager.GetLoginGroupID() + " and gr.ratio_id = lur.id and gr.status = 1");
        theRecord.AddRecord(dr_ratios);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("");
        return sTable;
    }
}