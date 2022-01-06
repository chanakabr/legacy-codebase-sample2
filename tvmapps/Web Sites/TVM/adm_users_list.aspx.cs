using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Phx.Lib.Log;
using TVinciShared;
using System.Data;

public partial class adm_users_list : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

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



    protected void FillTheTableEditor(ref DBTableWebEditor theTable, string sOrderBy)
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        theTable.SetConnectionKey("users_connection");

        ODBCWrapper.DataSetSelectQuery countQuery = new ODBCWrapper.DataSetSelectQuery();
        countQuery.SetConnectionKey("users_connection");

        theTable += "select ";
        countQuery += "select count(*) as count";

        theTable += "u.is_active,u.id as id,u.id as 'User ID',u.status,u.fail_count,u.Password,u.USERNAME as 'Username',u.FIRST_NAME as 'First Name',u.LAST_NAME as 'Last Name'," +
        "u.EMAIL_ADD as 'Email Address',u.REG_AFF as 'Affiliate',u.HANDLING_STATUS as 'Open Ticket', 'Active' as 'State',ut.ID as 'User Type ID',ut.description as 'User Type'";
        string queryPart = 
        "from " +
        "users u WITH (nolock) " +
        "inner join lu_content_status lcs (nolock) " +
        "on u.status = lcs.id and u.status<>2 " +
        "left join users_types ut WITH (nolock) on u.User_Type = ut.ID and u.group_id = ut.group_id and ut.is_active = 1 and ut.status = 1 " +
        "where ";
        theTable += queryPart;
        countQuery += queryPart;

        theTable += ODBCWrapper.Parameter.NEW_PARAM("u.group_id", "=", nGroupID);
        countQuery += ODBCWrapper.Parameter.NEW_PARAM("u.group_id", "=", nGroupID);

        queryPart = " and u.USERNAME NOT LIKE '%{Household}%' ";
        theTable += queryPart;
        countQuery += queryPart;

        if (Session["search_free_ul"] != null && Session["search_free_ul"].ToString() != "")
        {
            string sLike = "like(N'%" + Session["search_free_ul"].ToString() + "%')";
            queryPart = " and (u.USERNAME " + sLike + " or u.FIRST_NAME " + sLike + " or u.LAST_NAME " + sLike + " or u.EMAIL_ADD " + sLike + ")";
            theTable += queryPart;
            countQuery += queryPart;
        }
        else if(Session["search_exact_ul"] != null && Session["search_exact_ul"].ToString() != "")
        {
            string exactSearch = $"= '{Session["search_exact_ul"].ToString()}'";
            queryPart = " and (u.USERNAME " + exactSearch + " or u.FIRST_NAME " + exactSearch + " or u.LAST_NAME " + exactSearch + " or u.EMAIL_ADD " + exactSearch + ")";
            theTable += queryPart;
            countQuery += queryPart;
        }

        if (Session["search_only_open_tickets"] != null && Session["search_only_open_tickets"].ToString() != "" && Session["search_only_open_tickets"].ToString() != "-1")
        {
            theTable += " and ";
            countQuery += " and ";
            theTable += ODBCWrapper.Parameter.NEW_PARAM("u.HANDLING_STATUS", "=", int.Parse(Session["search_only_open_tickets"].ToString()));
            countQuery += ODBCWrapper.Parameter.NEW_PARAM("u.HANDLING_STATUS", "=", int.Parse(Session["search_only_open_tickets"].ToString()));
        }

        if (Session["domain_id"] != null && Session["domain_id"].ToString() != "")
        {
            Int32 nDomainId = int.Parse(Session["domain_id"].ToString());
            queryPart = " and u.id in (select ud.user_id from users_domains as ud where ";
            theTable += queryPart;
            countQuery += queryPart;

            theTable += ODBCWrapper.Parameter.NEW_PARAM("ud.domain_id", "=", nDomainId);
            countQuery += ODBCWrapper.Parameter.NEW_PARAM("ud.domain_id", "=", nDomainId);

            queryPart = "and ud.status=1 and ud.is_active=1)";
            theTable += queryPart;
            countQuery += queryPart;
        }

        if (sOrderBy != "")
        {
            queryPart = $" order by {sOrderBy}";
            theTable += queryPart;
        }
        else
        {
            queryPart = " order by u.id desc";
            theTable += queryPart;
        }

        theTable.SetShouldForceReadOnly(true);

        try
        {
            var dt = countQuery.Execute("query", true, true);

            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                theTable.SetQueryCount(Convert.ToInt32(dt.Rows[0]["count"]));
            }
        }
        catch (Exception ex)
        {
            log.Error("Users count query failed, setting default query count to 10000 instead", ex);
            // set query count to 10000 as default if count query was not successful since the query used to be top(10000)
            theTable.SetQueryCount(10000);
        }

        //
        //
        //
        //
        //

        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("status");
        theTable.AddHiddenField("fail_count");
        theTable.AddOnOffField("Open Ticket", "users~~|~~HANDLING_STATUS~~|~~id~~|~~Open~~|~~Close");
        theTable.AddOrderByColumn("Last Name", "u.LAST_NAME");
        theTable.AddOrderByColumn("First Name", "u.FIRST_NAME");
        // remove On/Off
        //theTable.AddActivationField("users");
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

        List<KeyValuePair<int, string>> couponGroups = GetAllGroupCouponByGroup();
        List<KeyValuePair<int, string>> engagementType = new List<KeyValuePair<int, string>>();
        foreach (ApiObjects.eEngagementType r in Enum.GetValues(typeof(ApiObjects.eEngagementType)))
        {
            engagementType.Add(new KeyValuePair<int, string>((int)r, r.ToString()));
        }


        DataTableLinkColumn linkColumnEngagements = new DataTableLinkColumn("javascript:Engagements", "Engagements", "");
        linkColumnEngagements.AddQueryStringValue("user_id", "field=id");
        linkColumnEngagements.AddQueryStringValue("coupon_groups", couponGroups.ToJSON());
        linkColumnEngagements.AddQueryStringValue("engagement_type", engagementType.ToJSON());
        theTable.AddLinkColumn(linkColumnEngagements);

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

        // Delete User should not allowed here
        //----------------------------------------------
        //if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        //{
        //    DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
        //    linkColumn.AddQueryStringValue("id", "field=id");
        //    linkColumn.AddQueryStringValue("db", "users_connection");
        //    linkColumn.AddQueryStringValue("table", "users");
        //    linkColumn.AddQueryStringValue("confirm", "true");
        //    linkColumn.AddQueryStringValue("main_menu", "14");
        //    linkColumn.AddQueryStringValue("sub_menu", "2");
        //    linkColumn.AddQueryStringValue("rep_field", "username");
        //    linkColumn.AddQueryStringValue("rep_name", "Username");
        //    theTable.AddLinkColumn(linkColumn);
        //}

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

        // Delete User should not allowed here
        //----------------------------------------------
        //if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        //{
        //    DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
        //    linkColumn.AddQueryStringValue("id", "field=id");
        //    linkColumn.AddQueryStringValue("table", "users");
        //    linkColumn.AddQueryStringValue("db", "users_connection");
        //    linkColumn.AddQueryStringValue("confirm", "false");
        //    linkColumn.AddQueryStringValue("main_menu", "14");
        //    linkColumn.AddQueryStringValue("sub_menu", "2");
        //    linkColumn.AddQueryStringValue("rep_field", "username");
        //    linkColumn.AddQueryStringValue("rep_name", "Username");
        //    theTable.AddLinkColumn(linkColumn);
        //}

    }

    private List<KeyValuePair<int, string>> GetAllGroupCouponByGroup()
    {
        List<KeyValuePair<int, string>> couponGroups = new List<KeyValuePair<int, string>>();
        try
        {

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select code, id  from pricing.dbo.coupons_groups cg where is_active = 1 and status <> 2 and ISNULL(COUPON_GROUP_TYPE, 0) <> 1";
            selectQuery += "and group_id=" + LoginManager.GetLoginGroupID();
            selectQuery.SetCachedSec(0);
            if (selectQuery.Execute("query", true) != null)
            {
                DataTable dt = selectQuery.Table("query");
                foreach (DataRow dr in dt.Rows)
                {
                    string couponName = ODBCWrapper.Utils.GetSafeStr(dr, "CODE");
                    int couponId = ODBCWrapper.Utils.GetIntSafeVal(dr, "id");

                    couponGroups.Add(new KeyValuePair<int, string>(couponId, couponName));
                }
            }
            selectQuery.Finish();
            selectQuery = null;

        }
        catch (Exception ex)
        {

        }
        return couponGroups;
    }

    public string GetPageContent(string sOrderBy, string sPageNum, string search_free_ul, string search_exact_ul ,string search_only_paid_users, string search_only_open_tickets)
    {
        Session["search_free_ul"] = search_free_ul.Replace("'", "''");
        Session["search_exact_ul"] = search_exact_ul.Replace("'", "''");

        if (search_only_paid_users != "")
            Session["search_only_paid_users"] = search_only_paid_users.Replace("'", "''");

        if (search_only_open_tickets != "")
            Session["search_only_open_tickets"] = search_only_open_tickets.Replace("'", "''");
        else if (Session["search_save"] == null)
        {
            Session["search_free_ul"] = "";
            Session["search_exact_ul"] = "";
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
