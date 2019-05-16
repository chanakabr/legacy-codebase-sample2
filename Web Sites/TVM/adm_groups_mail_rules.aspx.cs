using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;
using System.Collections;

public partial class adm_groups_mail_rules : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;
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
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
            Int32 nGroupID = LoginManager.GetLoginGroupID();
            if (Request.QueryString["search_save"] != null)
                Session["search_save"] = "1";
            else
                Session["search_save"] = null;
        }
    }


    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Mail rules management");
        //Session["gallery_id"] = null;
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
        theTable += "SELECT gmr.ID, gmr.NAME as Name, mrt.Name as Type, gmr.Type as Type_ID, gmr.start_date as Start, gmr.end_date as 'End', gmr.STATUS, gmr.IS_ACTIVE FROM groups_mail_rules gmr, lu_mail_rule_types mrt where gmr.type = mrt.ID and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("gmr.group_id", "=", nGroupID);
        theTable += "and (";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("gmr.STATUS", "=", 1);
        theTable += "or ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("gmr.STATUS", "=", 4);
        theTable += ")";
        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("status");
        theTable.AddHiddenField("IS_ACTIVE");
        theTable.AddHiddenField("Type_ID");

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH) && LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            theTable.AddActivationField("groups_mail_rules");
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_groups_mail_rules_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("rule_id", "field=id");
            linkColumn1.AddQueryStringValue("ruleType", "field=Type_ID");
            theTable.AddLinkColumn(linkColumn1);
        }
        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "groups_mail_rules");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "9");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "groups_mail_rules");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "9");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "groups_mail_rules");
            linkColumn.AddQueryStringValue("confirm", "false");
            linkColumn.AddQueryStringValue("main_menu", "9");
            linkColumn.AddQueryStringValue("sub_menu", "1");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

    }

    public void GetAddList()
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        string sRet = "anylinkmenu1.items=[";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "SELECT * FROM lu_mail_rule_types";

        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                if (i != 0)
                    sRet += ",";
                string sButton = selectQuery.Table("query").DefaultView[i].Row["Name"].ToString();
                string sID = selectQuery.Table("query").DefaultView[i].Row["ID"].ToString();
                string sLink = "adm_groups_mail_rules_new.aspx?ruleType=" + sID;
                sRet += "[\"" + sButton + "\", \"" + sLink + "\"]";
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        sRet += "]";
        Response.Write(sRet);
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

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, true, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        theTable.SetNewList(" ");
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);
  
        theTable.Finish();
        theTable = null;
        return sTable;
    }
}
