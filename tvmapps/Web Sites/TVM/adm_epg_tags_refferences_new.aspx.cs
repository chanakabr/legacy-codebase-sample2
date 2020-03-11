using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_epg_tags_refferences_new : System.Web.UI.Page
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
            if (Request.QueryString["epg_ref_id"] != null &&
                Request.QueryString["epg_ref_id"].ToString() != "")
            {
                Session["epg_ref_id"] = int.Parse(Request.QueryString["epg_ref_id"].ToString());
                
            }
            else
                Session["epg_ref_id"] = 0;
        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": EPG tags Refferences";
        if (Session["epg_ref_id"] != null && Session["epg_ref_id"].ToString() != "" && Session["epg_ref_id"].ToString() != "0")
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
        if (Session["epg_ref_id"] != null && Session["epg_ref_id"].ToString() != "" && int.Parse(Session["epg_ref_id"].ToString()) != 0)
            t = Session["epg_ref_id"];
        string sBack = "adm_epg_tags_refferences.aspx?search_save=1" + string.Format("&epg_tag_id={0}", Session["epg_tag_id"]);

        DBRecordWebEditor theRecord = new DBRecordWebEditor("EPG_fields_mapping", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_Name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_Name.Initialize("Refference", "adm_table_header_nbg", "FormInput", "external_ref", true);
        theRecord.AddRecord(dr_Name);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_type = new DataRecordShortIntField(false, 9, 9);
        dr_type.Initialize("Type", "adm_table_header_nbg", "FormInput", "type", false);
        dr_type.SetValue("3");
        theRecord.AddRecord(dr_type);

        DataRecordShortIntField dr_tag = new DataRecordShortIntField(false, 9, 9);
        dr_tag.Initialize("Type", "adm_table_header_nbg", "FormInput", "field_id", false);
        dr_tag.SetValue(Session["epg_tag_id"].ToString());
        theRecord.AddRecord(dr_tag);

        string sTable = theRecord.GetTableHTML("adm_epg_tags_refferences_new.aspx?submited=1");

        Session["ContentPage"] = "adm_epg_tags.aspx";
        

        return sTable;
    }
}