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

public partial class adm_tvp_profiles : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        Session["tvp_page_id"] = null;
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted() == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
        {
            Response.Expires = -1;
            return;
        }
        if (!IsPostBack)
        {
            if (Request.QueryString["profile_loc"] != null)
                Session["profile_loc"] = int.Parse(Request.QueryString["profile_loc"].ToString());
            if (Session["profile_loc"] == null)
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID, "adm_tvp_profiles.aspx?profile_loc=" + Session["profile_loc"].ToString() + "&platform=" + Session["platform"].ToString());
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 3, false);
            if (Request.QueryString["search_save"] != null)
                Session["search_save"] = "1";
            else
                Session["search_save"] = null;
        }
        Response.Expires = -1;
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    public string GetTableCSV()
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, false, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);

        FillTheTableEditor(ref theTable, sOldOrderBy);

        string sCSVFile = theTable.OpenCSV();
        theTable.Finish();
        theTable = null;
        return sCSVFile;
    }

    protected void FillTheTableEditor(ref DBTableWebEditor theTable, string sOrderBy)
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        theTable.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
        theTable += "select tp.is_active,tp.id as id,tp.NAME as 'Name',tp.status,lcs.description as 'State' from tvp_profiles tp,lu_content_status lcs where lcs.id=tp.status and tp.status<>2 and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("tp.group_id", "=", nGroupID);
        theTable += "and";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("tp.PROFILE_TYPE", "=", int.Parse(Session["profile_loc"].ToString()));
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("status");
        theTable.AddActivationField("tvp_profiles");
        theTable.AddHiddenField("is_active");
        theTable.AddTechDetails("tvp_profiles");

        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_tvp_galleries.aspx", "Galleries", "");
            linkColumn1.AddQueryStringValue("tvp_profile_id", "field=id");
            linkColumn1.AddQueryCounterValue("select count(*) as val from tvp_galleries g where g.status=1 and g.is_active=1 and g.PROFILE_ID=", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_tvp_profiles_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("tvp_profile_id", "field=id");
            //linkColumn1.AddQueryStringValue("tvp_profile_loc", Session["profile_loc"].ToString());
            theTable.AddLinkColumn(linkColumn1);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("db", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
            linkColumn.AddQueryStringValue("table", "tvp_profiles");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "14");
            linkColumn.AddQueryStringValue("sub_menu", "2");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("db", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
            linkColumn.AddQueryStringValue("table", "tvp_profiles");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "14");
            linkColumn.AddQueryStringValue("sub_menu", "2");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "tvp_profiles");
            linkColumn.AddQueryStringValue("db", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
            linkColumn.AddQueryStringValue("confirm", "false");
            linkColumn.AddQueryStringValue("main_menu", "14");
            linkColumn.AddQueryStringValue("sub_menu", "2");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, true, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        theTable.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);

        theTable.Finish();
        theTable = null;
        return sTable;
    }

    public void GetHeader()
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        string sRet = PageUtils.GetPreHeader() + ": Profiles - " + ODBCWrapper.Utils.GetTableSingleVal("lu_profile_types", "DESCRIPTION", int.Parse(Session["profile_loc"].ToString()), "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString()).ToString();
        Response.Write(sRet);
    }
}
