using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_groups_language_display_new : System.Web.UI.Page
{

    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork("MAIN_CONNECTION_STRING");
                return;
            }

            int id = 0;
            if (Request.QueryString["id"] != null && !string.IsNullOrEmpty(Request.QueryString["id"].ToString())
                && int.TryParse(Request.QueryString["id"].ToString(), out id) && id > 0)
            {
                Session["id"] = id;
            }
            else
            {
                Session["id"] = 0;                
            }
        }
    }    

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Group Language Display";
        if (Session["id"] != null && Session["id"].ToString() != "" && Session["id"].ToString() != "0")
            sRet += " - Edit";
        else
            sRet += " - New";
        Response.Write(sRet);
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
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        object t = null; ;
        if (Session["id"] != null && Session["id"].ToString() != "" && int.Parse(Session["id"].ToString()) != 0)
            t = Session["id"];
        string sBack = "adm_groups_language_display.aspx";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("groups_language_display_name", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("MAIN_CONNECTION_STRING");

        DataRecordDropDownField dr_language = new DataRecordDropDownField("", "NAME", "ID", "", null, 60, false);
        string sQuery = @"select distinct l.id, l.name as txt from lu_languages l
                            where id in (select language_id
				                         from groups g
				                         where g.id=" + LoginManager.GetLoginGroupID() +
                                         @"union all
				                         select language_id
				                         from group_extra_languages gel
				                         where gel.group_id=" + LoginManager.GetLoginGroupID() +
                                         @"and gel.[status]=1)";
        dr_language.SetConnectionKey("MAIN_CONNECTION_STRING");
        dr_language.SetSelectsQuery(sQuery);
        dr_language.Initialize("Language", "adm_table_header_nbg", "FormInput", "LANGUAGE_ID", true);
        dr_language.SetOrderBy("ID");        
        theRecord.AddRecord(dr_language);

        DataRecordShortTextField dr_displayName = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_displayName.Initialize("Display Name", "adm_table_header_nbg", "FormInput", "display_name", true);
        dr_displayName.SetDefault(0);
        theRecord.AddRecord(dr_displayName);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_groups_language_display_new.aspx?submited=1");
        return sTable;
    }

}