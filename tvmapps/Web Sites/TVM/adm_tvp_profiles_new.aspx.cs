using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using TVinciShared;

public partial class adm_tvp_profiles_new : System.Web.UI.Page
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
                Int32 nGroupID = LoginManager.GetLoginGroupID();
                DBManipulator.DoTheWork("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
                return;
            }
            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["tvp_profile_id"] != null &&
               Request.QueryString["tvp_profile_id"].ToString() != "")
            {
                Session["tvp_profile_id"] = int.Parse(Request.QueryString["tvp_profile_id"].ToString());
            }
            else
                Session["tvp_profile_id"] = 0;
        }

    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        string sRet = PageUtils.GetPreHeader() + ": Profiles - " + ODBCWrapper.Utils.GetTableSingleVal("lu_profile_types", "DESCRIPTION", int.Parse(Session["profile_loc"].ToString()), "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()).ToString();
        if (Session["tvp_profile_id"] != null && Session["tvp_profile_id"].ToString() != "" && Session["tvp_profile_id"].ToString() != "0")
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
        Int32 nGroupID = LoginManager.GetLoginGroupID();

        object t = null; ;
        if (Session["tvp_profile_id"] != null && Session["tvp_profile_id"].ToString() != "" && int.Parse(Session["tvp_profile_id"].ToString()) != 0)
            t = Session["tvp_profile_id"];

        string sBack = "adm_tvp_profiles.aspx?search_save=1&profile_loc=" + Session["profile_loc"].ToString() + "&platform=" + Session["platform"].ToString();
        DBRecordWebEditor theRecord = new DBRecordWebEditor("tvp_profiles", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());

        DataRecordShortTextField dr_domain = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_domain.Initialize("NAME", "adm_table_header_nbg", "FormInput", "Name", true);
        theRecord.AddRecord(dr_domain);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_profile_type = new DataRecordShortIntField(false, 9, 9);
        dr_profile_type.Initialize("Profile type", "adm_table_header_nbg", "FormInput", "PROFILE_TYPE", false);
        dr_profile_type.SetValue(Session["profile_loc"].ToString());
        theRecord.AddRecord(dr_profile_type);

        string sTable = theRecord.GetTableHTML("adm_tvp_profiles_new.aspx?submited=1");

        return sTable;
    }
}
