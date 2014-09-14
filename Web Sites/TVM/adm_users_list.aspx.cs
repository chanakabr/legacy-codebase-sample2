using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;
using System.Data;

public partial class adm_users_list : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;

    static public bool IsTvinciImpl()
    {
        Int32 nImplID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("users_connection");
        selectQuery += "select * from groups_modules_implementations where is_active=1 and status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_ID", "=", 1);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                nImplID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["IMPLEMENTATION_ID"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        // Logger.Logger.Log("Module Impl", "Implementation :" + nImplID + " Group ID :" + LoginManager.GetLoginGroupID().ToString(), "Users Impl");
        if (nImplID > 0)
            return true;
        return false;
    }

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
            if (IsTvinciImpl() == false)
            {
                Server.Transfer("adm_module_not_implemented.aspx");
                return;
            }
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 2, false);
            if (Request.QueryString["search_save"] != null)
                Session["search_save"] = "1";
            else
                Session["search_save"] = null;

            if (Request.QueryString["user_id"] != null && Request.QueryString["unlock"] == "1" && LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("users");
                updateQuery.SetConnectionKey("users_connection");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("fail_count", "=", 0);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("LAST_FAIL_DATE", "=", DBNull.Value);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", int.Parse(Request.QueryString["user_id"].ToString()));
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }

            if (Request.QueryString["domain_id"] != null)
            {
                Session["domain_id"] = Request.QueryString["domain_id"];
            }
            else
            {
                Session["domain_id"] = null;
            }
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

    public string GetTableCSV1()
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

    public string GetTableCSV()
    {

        Int32 nGroupID = LoginManager.GetLoginGroupID();

        DataTable dtUsers = new DataTable();
        Dictionary<int, string> dUserDD = new Dictionary<int, string>();

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

        selectQuery += "select ";
        selectQuery += "u.is_active,u.id as id,u.id as 'User ID',u.status,u.fail_count,u.Password,u.USERNAME as 'Username',u.FIRST_NAME as 'First Name',u.LAST_NAME as 'Last Name',";
        selectQuery += "u.EMAIL_ADD as 'Email Address',u.REG_AFF as 'Affiliate',u.HANDLING_STATUS as 'Open Ticket',lcs.description as 'State',ut.ID as 'User Type ID',ut.description as 'User Type'";
        selectQuery += "from ";
        selectQuery += "users u (nolock) ";
        selectQuery += "inner join lu_content_status lcs (nolock)";
        selectQuery += "on u.status = lcs.id and u.status<>2";
        selectQuery += "left join users_types ut(nolock) on u.User_Type = ut.ID and u.group_id = ut.group_id and ut.is_active = 1 and ut.status = 1 ";
        selectQuery += "where";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("u.group_id", "=", nGroupID);
        selectQuery += "  and  u.USERNAME NOT LIKE '%{Household}%'  ";               
 
        if (Session["search_free_ul"] != null && Session["search_free_ul"].ToString() != "")
        {
            string sLike = "like(N'%" + Session["search_free_ul"].ToString() + "%')";
            selectQuery += " and (u.USERNAME " + sLike + " or u.FIRST_NAME " + sLike + " or u.LAST_NAME " + sLike + " or u.EMAIL_ADD " + sLike + ")";
        }

        if (Session["start_user_id"] != null && Session["start_user_id"].ToString() != "")
        {
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("u.id", ">=", int.Parse(Session["start_user_id"].ToString()));
        }
        if (Session["end_user_id"] != null && Session["end_user_id"].ToString() != "")
        {
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("u.id", "<=", int.Parse(Session["end_user_id"].ToString()));
        }

        selectQuery.SetConnectionKey("users_connection");
        if (selectQuery.Execute("query", true) != null)
        {
            int nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                int nUserID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "id", i);
                dUserDD.Add(nUserID, string.Empty);
            }

            dtUsers = selectQuery.Table("query");

        }
        selectQuery.Finish();
        selectQuery = null;

        if (dUserDD.Keys.Count == 0)
        {
            return "";
        }

        DataColumn colDD = new DataColumn();
        colDD.DataType = System.Type.GetType("System.String");
        colDD.ColumnName = "Dynamic_Data";
        dtUsers.Columns.Add(colDD);

        selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select user_id, DATA_TYPE, DATA_VALUE from users_dynamic_data (nolock) where is_active=1 and status=1 and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
        selectQuery += "and user_id in (" + string.Join(",", dUserDD.Keys.Select(x => x.ToString()).ToArray()) + ")";
        selectQuery.SetConnectionKey("users_connection");
        if (selectQuery.Execute("query", true) != null)
        {
            int nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                int nUserID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "user_id", i);
                string sKey = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "DATA_TYPE", i);
                string sVal = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "DATA_VALUE", i);
                dUserDD[nUserID] += string.Format("<{0}:{1}> ", sKey, sVal);
            }
        }
        selectQuery.Finish();
        selectQuery = null;

        for (int i = 0; i < dtUsers.Rows.Count; i++)
        {
            int nUserID = int.Parse(dtUsers.Rows[i]["id"].ToString());

            dtUsers.Rows[i]["Dynamic_Data"] = dUserDD[nUserID];

        }

        GridView gv = new GridView();

        gv.DataSource = dtUsers;
        gv.DataBind();
        HttpContext.Current.Response.Clear();
        HttpContext.Current.Response.AddHeader("content-disposition", "attachment;filename=myFileName.xls");
        HttpContext.Current.Response.Charset = "UTF-8";
        HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
        System.IO.StringWriter stringWrite = new System.IO.StringWriter();
        HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

        gv.RenderControl(htmlWrite);
        HttpContext.Current.Response.Write(stringWrite.ToString());
        HttpContext.Current.Response.End();

        return "";
    }

    protected void FillTheTableEditor(ref DBTableWebEditor theTable, string sOrderBy)
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        theTable.SetConnectionKey("users_connection");

        theTable += "select ";
        theTable += "u.is_active,u.id as id,u.id as 'User ID',u.status,u.fail_count,u.Password,u.USERNAME as 'Username',u.FIRST_NAME as 'First Name',u.LAST_NAME as 'Last Name',";
        theTable += "u.EMAIL_ADD as 'Email Address',u.REG_AFF as 'Affiliate',u.HANDLING_STATUS as 'Open Ticket',lcs.description as 'State',ut.ID as 'User Type ID',ut.description as 'User Type'";
        theTable += "from ";
        theTable += "users u (nolock) ";
        theTable += "inner join lu_content_status lcs (nolock)";
        theTable += "on u.status = lcs.id and u.status<>2";
        theTable += "left join users_types ut(nolock) on u.User_Type = ut.ID and u.group_id = ut.group_id and ut.is_active = 1 and ut.status = 1 ";
        theTable += "where  ";
        
        theTable += ODBCWrapper.Parameter.NEW_PARAM("u.group_id", "=", nGroupID);

        theTable += " and		u.USERNAME NOT LIKE '%{Household}%' ";

        if (Session["search_free_ul"] != null && Session["search_free_ul"].ToString() != "")
        {
            string sLike = "like(N'%" + Session["search_free_ul"].ToString() + "%')";
            theTable += " and (u.USERNAME " + sLike + " or u.FIRST_NAME " + sLike + " or u.LAST_NAME " + sLike + " or u.EMAIL_ADD " + sLike + ")";
        }

        //if ( Session["start_user_id"] != null && Session["start_user_id"].ToString() != "" )
        //{
        //    theTable += " and ";
        //    theTable += ODBCWrapper.Parameter.NEW_PARAM("u.id", ">=", int.Parse(Session["start_user_id"].ToString()));
        //}
        //if (Session["end_user_id"] != null && Session["end_user_id"].ToString() != "")
        //{
        //    theTable += " and ";
        //    theTable += ODBCWrapper.Parameter.NEW_PARAM("u.id", "<=", int.Parse(Session["end_user_id"].ToString()));
        //}

        if (Session["search_only_open_tickets"] != null && Session["search_only_open_tickets"].ToString() != "" && Session["search_only_open_tickets"].ToString() != "-1")
        {
            theTable += " and ";
            theTable += ODBCWrapper.Parameter.NEW_PARAM("u.HANDLING_STATUS", "=", int.Parse(Session["search_only_open_tickets"].ToString()));
        }
        if (Session["domain_id"] != null && Session["domain_id"].ToString() != "")
        {
            Int32 nDomainId = int.Parse(Session["domain_id"].ToString());
            theTable += " and ";
            theTable += "u.id in (select ud.user_id from users_domains as ud where ";
            theTable += ODBCWrapper.Parameter.NEW_PARAM("ud.domain_id", "=", nDomainId);
            theTable += "and ud.status=1 and ud.is_active=1)";
        }

        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        else
            theTable += " order by id desc";
        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("status");
        theTable.AddHiddenField("fail_count");
        theTable.AddOnOffField("Open Ticket", "users~~|~~HANDLING_STATUS~~|~~id~~|~~Open~~|~~Close");
        theTable.AddOrderByColumn("Last Name", "u.LAST_NAME");
        theTable.AddOrderByColumn("First Name", "u.FIRST_NAME");
        theTable.AddActivationField("users");
        theTable.AddHiddenField("is_active");
        theTable.AddHiddenField("password");

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_user_dynamic_data.aspx", "Dynamic Data", "");
            linkColumn1.AddQueryStringValue("user_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }
        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            //DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_user_change_pass.aspx", "Change Password", "");
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("javascript:Password", "Password", "");
            linkColumn1.AddQueryStringValue("user_id", "field=id");
            //linkColumn1.AddQueryStringValue("password", "field=password");
            theTable.AddLinkColumn(linkColumn1);
        }
        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_user_purchases_report.aspx", "Purchase Report", "");
            //DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("javascript:alert(\"not implemented yet\");", "Purchase Report", "");
            linkColumn1.AddQueryStringValue("user_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("javascript:Gift", "Grant Gift", "");
            linkColumn1.AddQueryStringValue("user_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_users_log.aspx", "Log", "");
            linkColumn1.AddQueryStringValue("user_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_users_list.aspx", "Un Lock", "fail_count=3");
            linkColumn1.AddQueryStringValue("user_id", "field=id");
            linkColumn1.AddQueryStringValue("unlock", "1");
            theTable.AddLinkColumn(linkColumn1);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_users_list_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("user_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("db", "users_connection");
            linkColumn.AddQueryStringValue("table", "users");
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
            linkColumn.AddQueryStringValue("db", "users_connection");
            linkColumn.AddQueryStringValue("table", "users");
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
            linkColumn.AddQueryStringValue("table", "users");
            linkColumn.AddQueryStringValue("db", "users_connection");
            linkColumn.AddQueryStringValue("confirm", "false");
            linkColumn.AddQueryStringValue("main_menu", "14");
            linkColumn.AddQueryStringValue("sub_menu", "2");
            linkColumn.AddQueryStringValue("rep_field", "username");
            linkColumn.AddQueryStringValue("rep_name", "Username");
            theTable.AddLinkColumn(linkColumn);
        }

    }

    public string GetPageContent(string sOrderBy, string sPageNum, string search_free_ul, string search_only_paid_users, string search_only_open_tickets)
    {
        Session["search_free_ul"] = search_free_ul.Replace("'", "''");

        //if (string.IsNullOrEmpty(start_user_id))
        //{
        //    // get deafult value  =  0
        //    start_user_id = "0";
        //}
        //Session["start_user_id"] = start_user_id;

        //string config_end_user_id = WS_Utils.GetTcmConfigValue("end_user_id_list");
        
        //if (string.IsNullOrEmpty(end_user_id))
        //{
        //    // get deafult value  from confuguration          
        //    end_user_id = (int.Parse(start_user_id) + int.Parse(config_end_user_id)).ToString();
        //}
        //else  
        //{  
        //    end_user_id = Math.Min( int.Parse(start_user_id) + int.Parse(config_end_user_id) , int.Parse(end_user_id)).ToString();
        //}
        //Session["end_user_id"] = end_user_id;

        if (search_only_paid_users != "")
            Session["search_only_paid_users"] = search_only_paid_users.Replace("'", "''");

        if (search_only_open_tickets != "")
            Session["search_only_open_tickets"] = search_only_open_tickets.Replace("'", "''");
        else if (Session["search_save"] == null)
        {
            Session["search_free_ul"] = "";
            Session["search_only_paid_users"] = "";
            Session["search_only_open_tickets"] = "";
        }

        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();

        bool bNewButton = true;
        if (Session["domain_id"] != null && Session["domain_id"].ToString() != "")
        {
            bNewButton = false;
        }

        DBTableWebEditor theTable = new DBTableWebEditor(true, true, bNewButton, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        theTable.SetConnectionKey("users_connection");
        FillTheTableEditor(ref theTable, sOrderBy);


        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);

        theTable.Finish();
        theTable = null;
        return sTable;
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Users");
    }
}
