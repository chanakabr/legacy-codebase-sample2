using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_business_types : System.Web.UI.Page
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
            m_sMenu = TVinciShared.Menu.GetMainMenu(5, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 5, false);
            if (Request.QueryString["search_save"] != null)
                Session["search_save"] = "1";
            else
                Session["search_save"] = null;
        }
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

        theTable += "SELECT ID, m_id, media_desc as 'Media type',f_desc as 'Friendly description', is_trailer_value, is_trailer as 'Is Trailer' FROM (SELECT gmt.GROUP_ID, gmt.ID, lmt.ID AS m_id, gmt.DESCRIPTION AS f_desc, lmt.DESCRIPTION AS media_desc, is_trailer as is_trailer_value, case gmt.is_trailer when 1 then 'Yes' else 'No' end is_trailer FROM lu_media_types AS lmt LEFT OUTER JOIN groups_media_type AS gmt ON gmt.MEDIA_TYPE_ID = lmt.ID AND ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("gmt.GROUP_ID" , "=" , nGroupID);
        theTable += ") AS q WHERE (m_id > 0)";
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        else
            theTable += " order by f_desc desc";
        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("m_ID");
        theTable.AddHiddenField("is_trailer_value");

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_business_types_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("groups_media_type_id", "field=id");
            linkColumn1.AddQueryStringValue("media_type_id", "field=m_id");
            theTable.AddLinkColumn(linkColumn1);
        }
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();
        bool bAdd = false;
        if (PageUtils.IsTvinciUser() == true)
            bAdd = true;
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, bAdd, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);

        theTable.Finish();
        theTable = null;
        return sTable;
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Group Documented Actions");
    }
}
