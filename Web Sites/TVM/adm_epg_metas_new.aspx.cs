using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_epg_metas_new : System.Web.UI.Page
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
            if (Request.QueryString["epg_meta_id"] != null &&
                Request.QueryString["epg_meta_id"].ToString() != "")
            {
                Session["epg_meta_id"] = int.Parse(Request.QueryString["epg_meta_id"].ToString());
               
            }
            else
                Session["epg_meta_id"] = 0;
        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": EPG Metas";
        if (Session["epg_meta_id"] != null && Session["epg_meta_id"].ToString() != "" && Session["epg_meta_id"].ToString() != "0")
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
        if (Session["epg_meta_id"] != null && Session["epg_meta_id"].ToString() != "" && int.Parse(Session["epg_meta_id"].ToString()) != 0)
            t = Session["epg_meta_id"];
        string sBack = "adm_epg_metas.aspx?search_save=1";

        DBRecordWebEditor theRecord = new DBRecordWebEditor("epg_metas_types", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_Name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_Name.Initialize("Meta name", "adm_table_header_nbg", "FormInput", "Name", true);
        theRecord.AddRecord(dr_Name);

        DataRecordShortTextField dr_Default_Value = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_Default_Value.Initialize("Default Value", "adm_table_header_nbg", "FormInput", "default_value", false);
        theRecord.AddRecord(dr_Default_Value);
        


        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_type = new DataRecordShortIntField(false, 9, 9);
        dr_type.Initialize("Type", "adm_table_header_nbg", "FormInput", "type", false);
        dr_type.SetValue("2");
        theRecord.AddRecord(dr_type);

        DataRecordShortIntField dr_IsActive = new DataRecordShortIntField(false, 9, 9);
        dr_IsActive.Initialize("IsActive", "adm_table_header_nbg", "FormInput", "is_active", false);
        dr_IsActive.SetValue("1");
        theRecord.AddRecord(dr_IsActive);

        DataRecordShortIntField dr_order_num = new DataRecordShortIntField(false, 3, 3);
        dr_order_num.Initialize("Order number", "adm_table_header_nbg", "FormInput", "ORDER_NUM", false);
        dr_order_num.SetValue("1");
        theRecord.AddRecord(dr_order_num);

        //is_searchable true/ false
        DataRecordCheckBoxField dr_Searchable = new DataRecordCheckBoxField(true);
        dr_Searchable.Initialize("is searchable", "adm_table_header_nbg", "FormInput", "is_searchable", false);
        theRecord.AddRecord(dr_Searchable);

        DataRecordDropDownField dr_meta_flag = new DataRecordDropDownField("lu_tag_type_flag", "DESCRIPTION", "id", "", null, 60, true);
        dr_meta_flag.SetNoSelectStr("---");
        dr_meta_flag.Initialize("Tag Type Flag", "adm_table_header_nbg", "FormInput", "meta_type_flag", false);        
        theRecord.AddRecord(dr_meta_flag);


        DataRecordDropDownField dr_meta_type_mapping = new DataRecordDropDownField("lu_tag_type_flag", "DESCRIPTION", "id", "", null, 60, true);
        dr_meta_type_mapping.SetNoSelectStr("---");
        dr_meta_type_mapping.Initialize("related to meta", "adm_table_header_nbg", "FormInput", "", false);
        theRecord.AddRecord(dr_meta_type_mapping);


        //protection for specific epg meta so manual changes of meta values in the tvm will not be overridden by epg updates (in INGEST)
        //is_searchable true/ false
        DataRecordCheckBoxField dr_protection_meta_update = new DataRecordCheckBoxField(true);
        dr_protection_meta_update.Initialize("Protection meta from ingest updates", "adm_table_header_nbg", "FormInput", "is_protect_from_updates", false);
        theRecord.AddRecord(dr_protection_meta_update);

        string sTable = theRecord.GetTableHTML("adm_epg_metas_new.aspx?submited=1");

        return sTable;
    }
}