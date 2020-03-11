using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_profiles_tags : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted() == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(9, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 4, false);
            Int32 nGroupID = LoginManager.GetLoginGroupID();
            if (Request.QueryString["search_save"] != null)
                Session["search_save"] = "1";
            else
                Session["search_save"] = null;
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ":Profiles Tags management");
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
        theTable += "select t.id,t.status,mtt.description as 'Profile Tag type',t.Value as 'Profile Tag Code',t.description as 'Profile Tag Description' from profile_tags t";
        theTable += ",profile_tags_types mtt ";
        theTable += "where t.status<>2 and mtt.id=t.PROFILE_TAG_TYPE_ID and mtt.status=1 ";
        if (Session["search_tag_type"] != null && Session["search_tag_type"].ToString() != "" && Session["search_tag_type"].ToString() != "-1")
        {
            theTable += " and ";
            theTable += ODBCWrapper.Parameter.NEW_PARAM("mtt.id", "=", int.Parse(Session["search_tag_type"].ToString()));

        }
        if (Session["search_free"] != null && Session["search_free"].ToString() != "")
        {
            string sLike = "like(N'%" + Session["search_free"].ToString().Replace("'", "''") + "%')";
            theTable += " and (t.value " + sLike + ")";
        }
        theTable += "and";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("t.group_id", "=", nGroupID);
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        else
            theTable += " order by mtt.name,t.description desc";
        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("status");
        theTable.AddTechDetails("profile_tags");
        theTable.AddOrderByColumn("Profile Tag type", "mtt.description");
        theTable.AddOrderByColumn("Profile Tag description", "t.description");
        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_profiles_tags_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("profile_tag_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }
    }

    public string GetPageContent(string sOrderBy, string sPageNum, string search_tag_type, string search_free)
    {
        if (search_tag_type != "-1")
            Session["search_tag_type"] = search_tag_type;
        else if (Session["search_save"] == null)
            Session["search_tag_type"] = "-1";

        if (search_free != "")
            Session["search_free"] = search_free;
        else if (Session["search_save"] == null)
            Session["search_free"] = "";

        if (sOrderBy != "")
            Session["order_by"] = sOrderBy;
        else if (Session["search_save"] == null)
            Session["order_by"] = "";
        else
            sOrderBy = Session["order_by"].ToString();

        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();

        DBTableWebEditor theTable = new DBTableWebEditor(true, true, true, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);
        theTable.Finish();
        theTable = null;
        return sTable;
    }
}
