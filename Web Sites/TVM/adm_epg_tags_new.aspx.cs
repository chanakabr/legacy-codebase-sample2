using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_epg_tags_new : System.Web.UI.Page
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
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork();
                return;
            }
            m_sMenu = TVinciShared.Menu.GetMainMenu(5, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["epg_tag_id"] != null &&
                Request.QueryString["epg_tag_id"].ToString() != "")
            {
                Session["epg_tag_id"] = int.Parse(Request.QueryString["epg_tag_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("EPG_Tags_Types", "group_id", int.Parse(Session["epg_tag_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["epg_tag_id"] = 0;
        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": EPG tags";
        if (Session["epg_tag_id"] != null && Session["epg_tag_id"].ToString() != "" && Session["epg_tag_id"].ToString() != "0")
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
        if (Session["epg_tag_id"] != null && Session["epg_tag_id"].ToString() != "" && int.Parse(Session["epg_tag_id"].ToString()) != 0)
            t = Session["epg_tag_id"];
        string sBack = "adm_epg_tags.aspx?search_save=1";

        DBRecordWebEditor theRecord = new DBRecordWebEditor("epg_tags_types", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_Name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_Name.Initialize("tag name", "adm_table_header_nbg", "FormInput", "Name", true);
        theRecord.AddRecord(dr_Name);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_IsActive = new DataRecordShortIntField(false, 9, 9);
        dr_IsActive.Initialize("IsActive", "adm_table_header_nbg", "FormInput", "is_active", false);
        dr_IsActive.SetValue("1");
        theRecord.AddRecord(dr_IsActive);

        DataRecordShortIntField dr_order_num = new DataRecordShortIntField(true, 3, 3);
        dr_order_num.Initialize("Order number", "adm_table_header_nbg", "FormInput", "ORDER_NUM", false);
        theRecord.AddRecord(dr_order_num);

        string sTable = theRecord.GetTableHTML("adm_epg_tags_new.aspx?submited=1");

        return sTable;
    }
}