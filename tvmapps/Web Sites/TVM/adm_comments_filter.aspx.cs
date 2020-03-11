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
using System.Text;

public partial class adm_comments_filter : System.Web.UI.Page
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
        {
            Response.Expires = -1;
            return;
        }
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 3, false);
            if (Request.QueryString["search_save"] != null)
            {
                Session["search_save"] = "1";
                UpdateRegx();
            }
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
        //theTable.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
        theTable += "select ID, Text, is_active, status, 'Wild Card' = CASE WHEN wildcard=0 THEN 'False' ELSE 'True' END from comment_filters where status=1 and group_id=" + nGroupID;
        theTable += " order by Text";
        /*
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        */ 
        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("status");
        theTable.AddActivationField("comment_filters");
        theTable.AddHiddenField("is_active");

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_comments_filter_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("comment_filter_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            //linkColumn.AddQueryStringValue("db", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
            linkColumn.AddQueryStringValue("table", "comment_filters");
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
            //linkColumn.AddQueryStringValue("db", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
            linkColumn.AddQueryStringValue("table", "comment_filters");
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
            linkColumn.AddQueryStringValue("table", "comment_filters");
            //linkColumn.AddQueryStringValue("db", "tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
            linkColumn.AddQueryStringValue("confirm", "true");
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
        DBTableWebEditor theTable = null;
        //if (PageUtils.IsTvinciUser() == true)
            theTable = new DBTableWebEditor(true, true, true, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        //else
        //    theTable = new DBTableWebEditor(true, true, false, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        //theTable.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);
        Session["ContentPage"] = "adm_comments_filter.aspx";
        theTable.Finish();
        theTable = null;
        Response.Expires = -1;
        return sTable;
    }


    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Comments Filter");
    }

    public string ChangeActiveStateRow(string sTable, string sID, string sStatus, string sConnectionKey)
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        Int32 nRowGroupID = 0;
        try
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            if (sConnectionKey != "")
                selectQuery.SetConnectionKey(sConnectionKey);
            selectQuery += "select group_id from " + sTable + " where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", int.Parse(sID));
            if (selectQuery.Execute("query", true) != null)
            {
                nRowGroupID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["group_id"].ToString());
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        catch { }
        bool bBelongs = false;
        if (nGroupID == 0)
            bBelongs = false;
        if (nRowGroupID != 0 && nRowGroupID != nGroupID)
        {
            PageUtils.DoesGroupIsParentOfGroup(nGroupID, nRowGroupID, ref bBelongs);
        }
        else
            bBelongs = true;
        if (bBelongs == false)
        {
            LoginManager.LogoutFromSite("login.html");
            return "";
        }
        else
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery(sTable);
            if (sConnectionKey != "")
                updateQuery.SetConnectionKey(sConnectionKey);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", int.Parse(sStatus));
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("updater_id", "=", LoginManager.GetLoginID());
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.Now);
            updateQuery += "where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", int.Parse(sID));
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;

            UpdateRegx();

            string sRet = "activation_" + sID.ToString() + "~~|~~";
            if (int.Parse(sStatus) == 1)
            {
                sRet += "<b>On</b> / <a href=\"javascript: ChangeActiveStateRow('" + sTable + "'," + sID.ToString() + ",0,'" + sConnectionKey + "');\" ";
                sRet += " class='adm_table_link_div' >";
                sRet += "Off";
                sRet += "</a>";
            }
            else
            {
                sRet += "<b>Off</b> / <a href=\"javascript: ChangeActiveStateRow('" + sTable + "'," + sID.ToString() + ",1,'" + sConnectionKey + "');\" ";
                sRet += " class='adm_table_link_div' >";
                sRet += "On";
                sRet += "</a>";
            }
            return sRet;
        }
    }


    private void UpdateRegx()
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        
        Int32 nGLFID = GetGLFID(nGroupID);

        string sExpression = GetRegExpression(nGroupID);

        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("group_language_filters");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("Expression", "=", sExpression);
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.Now);
        updateQuery += "where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
        updateQuery += "and";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nGLFID);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;
    }



    private string GetRegExpression(Int32 nGroupID)
    {
        string pattern = string.Empty;

        string sWords = GetWords(0, nGroupID);
        string sWildWords = GetWords(1, nGroupID);

        if (!string.IsNullOrEmpty(sWords))
        {
            pattern += @"(\b(" + sWords + @")\b)";
        }

        if (!string.IsNullOrEmpty(sWildWords))
        {
            if (!string.IsNullOrEmpty(pattern))
            {
                pattern += "|";
            }

            pattern += @"(\w*(" + sWildWords + @")\w*)";
        }

        return pattern;
    }

    private string GetWords(Int32 nWildcard, int nGroupID)
    {
        StringBuilder sb = new StringBuilder();

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select Text from comment_filters where is_active=1 and status=1 and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("wildcard", "=", nWildcard);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                if (i > 0)
                {
                    sb.Append("|");
                }

                sb.Append(selectQuery.Table("query").DefaultView[i].Row["Text"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;

        return sb.ToString();
    }

    private Int32 GetGLFID(Int32 nGroupID)
    {
        Int32 nGLFID = 0;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select id from group_language_filters where is_active=1 and status=1 and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                nGLFID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;

        if (nGLFID == 0)
        {
            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("group_language_filters");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 1);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
            insertQuery.Execute();
            insertQuery.Finish();
            insertQuery = null;

            selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from group_language_filters where is_active=1 and status=1 and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nGLFID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        return nGLFID;

    }


}
