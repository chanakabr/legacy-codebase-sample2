using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ODBCWrapper;
using TVinciShared;

public partial class adm_operator_menus : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_operator.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_operator.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;

            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);

            if (Request.QueryString["operator_id"] != null && Request.QueryString["operator_id"].ToString() != "")
            {
                Session["operator_id"] = int.Parse(Request.QueryString["operator_id"].ToString());
            }

            Table table = new Table();
            foreach (DataRow platformRow in GetPlaformsFromDB().Rows)
            {
                List<KeyValuePair<int, string>> menuPairs = GetMenusFromDb(platformRow["NAME"].ToString());
                if (menuPairs.Count > 0)
                {
                    TableRow row = new TableRow();
                    TableCell cell = new TableCell();
                    //  cell.Attributes.Add("data-platformId", platform.Key.ToString());
                    Label label = new Label { Text = string.Format("{0} menu:", platformRow["NAME"].ToString()) };
                    cell.Controls.Add(label);
                    row.Cells.Add(cell);
                    DropDownList ddl = new DropDownList() { ID = "ddl_" + platformRow["NAME"].ToString() };
                    ddl.Items.Add(new ListItem("---", "0"));

                    TableCell ddlCell = new TableCell();
                    foreach (KeyValuePair<int, string> dbMenu in menuPairs)
                    {
                        ddl.Items.Add(new ListItem(dbMenu.Value, dbMenu.Key.ToString()));
                    }
                    if (platformRow["TVPMenuID"] != null)
                    {
                        ddl.SelectedValue = platformRow["TVPMenuID"].ToString();
                    }
                    ddlCell.Controls.Add(ddl);
                    row.Cells.Add(ddlCell);
                    //DataRecordDropDownField dr_menu_ID = new DataRecordDropDownField("groups_operators_menus", "txt", "MENU_ID", "", null, 60, true);
                    //dr_menu_ID.SetNoSelectStr("N\\A");

                    //dr_menu_ID.Initialize(string.Format("{0} Menu", platform.Value), "adm_table_header_nbg", "FormInput", "TVPMenuID", false);
                    //theRecord.AddRecord(dr_menu_ID);


                    table.Rows.Add(row);
                }


            }
            TableRow buttonsRow = new TableRow();

            TableCell confirmCell = new TableCell();
            confirmCell.Attributes.Add("id", "confirm_btn");
            confirmCell.Attributes.Add("href", "#confirm");
            confirmCell.Attributes.Add("onclick", "submitASPFormWithCheck(\"adm_operator_menus.aspx?submited=1\")");
            confirmCell.Controls.Add(new LinkButton() { ID = "confirm_btn", CssClass = "btn" });
            buttonsRow.Cells.Add(confirmCell);


            TableCell cancelCell = new TableCell();
            cancelCell.Attributes.Add("id", "cancel_btn");
            cancelCell.Attributes.Add("href", "#cancel_btn");
            cancelCell.Controls.Add(new LinkButton() { ID = "cancel_btn", CssClass = "btn", PostBackUrl = "adm_operator.aspx?search_save=1" });

            buttonsRow.Cells.Add(cancelCell);
            table.Rows.Add(buttonsRow);
            page_content.Controls.Add(table);
        }
        else
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
            {
                DataTable dbValsTable = GetPlaformsFromDB();
                foreach (DataRow row in dbValsTable.Rows)
                {
                    int menuId = int.Parse(Request.Form["ddl_" + row["NAME"].ToString()]);
                    if (row["TVPMenuID"] != null && !string.IsNullOrEmpty(row["TVPMenuID"].ToString()))
                    {
                        if (int.Parse(row["TVPMenuID"].ToString()) != menuId)
                        {
                            ODBCWrapper.UpdateQuery updateQuery = new UpdateQuery("groups_operators_menus");
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("TVPMenuID", "=", menuId);
                            updateQuery += " WHERE ";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("OperatorID", "=", (int)Session["operator_id"]);
                            updateQuery += " AND ";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PlatformID", "=", (int)row["ID"]);
                            updateQuery.Execute();
                            updateQuery.Finish();
                        }
                    }
                    else
                    {
                        ODBCWrapper.InsertQuery insertQuery = new InsertQuery("groups_operators_menus");
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("TVPMenuID", "=", menuId);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("OperatorID", "=", (int)Session["operator_id"]);
                        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PlatformID", "=", (int)row["ID"]);
                        insertQuery.Execute();
                        insertQuery.Finish();
                    }

                }
                Response.Redirect("adm_operator.aspx");
            }
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Operator management");
    }


    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }




    private DataTable GetPlaformsFromDB()
    {
        List<KeyValuePair<int, string>> retVal = new List<KeyValuePair<int, string>>();

        ODBCWrapper.DataSetSelectQuery query = new DataSetSelectQuery();
        query += "SELECT lup.ID, lup.NAME, gom.TVPMenuID FROM  groups_operators_menus gom RIGHT OUTER JOIN lu_platform lup ON gom.PlatformID = lup.ID AND ";
        query += ODBCWrapper.Parameter.NEW_PARAM("gom.OperatorID", "=", ((int)Session["operator_id"]));
        //query += " WHERE ";
        //query += ODBCWrapper.Parameter.NEW_PARAM("gom.OperatorID", "=", ((int)Session["operator_id"]));
        DataTable dt = query.Execute("query", true);
        query.Finish();
        return dt;
    }
    //[tvp_{0}_{1}_{2}]
    private List<KeyValuePair<int, string>> GetMenusFromDb(string platformName)
    {
        List<KeyValuePair<int, string>> retVal = new List<KeyValuePair<int, string>>();

        string groupName = GetName(LoginManager.GetLoginGroupID(), false);
        string operatorName = GetName((int)Session["operator_id"], true);
        ODBCWrapper.DataSetSelectQuery query = new DataSetSelectQuery();
        query += string.Format("select ID as MENU_ID, NAME as txt from [tvp_[0]_{1}_[2]].[dbo].tvp_menu where is_active=1 and status=1", groupName, platformName, operatorName);
        DataTable dt = query.Execute("query", true);
        if (dt != null)
        {
            foreach (DataRow dr in dt.Rows)
            {
                retVal.Add(new KeyValuePair<int, string>(int.Parse(dr["Menu_ID"].ToString()), dr["txt"].ToString()));
            }
        }
        return retVal;
    }
    //
    //dr_menu_ID.SetSelectsQuery(string.Format("select ID as MENU_ID, NAME as txt from [tvp_mediacorp].[dbo].tvp_menu where is_active=1 and status=1",groupName, operatorName, platform.Value));
    //protected void GetMainMenu()
    //{
    //    Response.Write(m_sMenu);
    //}

    //protected void GetSubMenu()
    //{
    //    Response.Write(m_sSubMenu);
    //}


    private string GetName(int id, bool isOperator)
    {
        ODBCWrapper.DataSetSelectQuery query = new DataSetSelectQuery();
        query += string.Format("SELECT {0} FROM {1} WHERE ", isOperator ? "NAME" : "GROUP_NAME", isOperator ? "GROUPS_OPERATORS" : "GROUPS");
        query += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", id);
        DataTable dt = query.Execute("query", true);
        if (dt != null)
        {
            return dt.DefaultView[0][0].ToString();
        }

        return string.Empty;

    }

    private void HandleSubmit()
    {

    }
}



//public void GetPageContent(string sOrderBy, string sPageNum)
//{
//    //if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
//    //{
//    //    Session["error_msg"] = "";
//    //    return Session["last_page_html"].ToString();
//    //}
//    //object t = null; ;
//    //if (Session["operator_id"] != null)
//    //    t = Session["operator_id"];
//    string sBack = "adm_operator.aspx?search_save=1";

//    //DBRecordWebEditor theRecord = new DBRecordWebEditor("groups_operators_menus", "adm_table_pager", sBack, "ID", "ID", 0, sBack, "");
//    //string groupName = GetName(LoginManager.GetLoginGroupID(), false);
//    //string operatorName = GetName((int) t, true);

//    //DataRecordShortIntField drOperatorField = new DataRecordShortIntField(false, 0, 0);
//    //drOperatorField.Initialize("", "", "", "OperatorID", true);
//    //theRecord.AddRecord(drOperatorField);
//    Table table = new Table();
//    foreach (KeyValuePair<int, string> platform in GetPlaformsFromDB().Rows)
//    {

//        TableRow row = new TableRow();
//        TableCell cell = new TableCell();
//        cell.Attributes.Add("data-platformId", platform.Key.ToString());
//        Label label = new Label();
//        label.Text = string.Format("{0} menu:", platform.Value);
//        cell.Controls.Add(label);
//        row.Cells.Add(cell);
//        DropDownList ddl = new DropDownList();
//        ddl.Items.Add(new ListItem("", "0"));
//        TableCell ddlCell = new TableCell();
//        foreach (KeyValuePair<int, string> dbMenu in GetMenusFromDb(platform.Value))
//        {
//            ddl.Items.Add(new ListItem(dbMenu.Value, dbMenu.Key.ToString()));
//        }
//        ddlCell.Controls.Add(ddl);

//        //DataRecordDropDownField dr_menu_ID = new DataRecordDropDownField("groups_operators_menus", "txt", "MENU_ID", "", null, 60, true);
//        //dr_menu_ID.SetNoSelectStr("N\\A");

//        //dr_menu_ID.Initialize(string.Format("{0} Menu", platform.Value), "adm_table_header_nbg", "FormInput", "TVPMenuID", false);
//        //theRecord.AddRecord(dr_menu_ID);


//    }


//    page_content.Controls.Add(table);
//}

//   DataRecordDropDownField dr_platform = new DataRecordDropDownField("lu_platform", "NAME", "ID", "", null, 60, true);
//   dr_platform.Initialize("Platform", "adm_table_header_nbg", "FormInput", "Sub_Group_ID", false);
//   dr_platform.SetAddtionalHtml("onchange=javascript:refreshMenuIdList(this)");

//   //DataRecordDropDownField dr_menu_ID = new DataRecordDropDownField("groups_operators", "txt", "MENU_ID", "", null, 60, true);
//   //dr_menu_ID.Initialize("Menu", "adm_table_header_nbg", "FormInput", "Menu_ID", false);

//   //to be replaced with "tvp_" + %GroupName% + "_" + Session["platform"].ToString()) + "_" + Session["operator_id"].ToString() ;
//   //dr_menu_ID.SetConnectionKey("tvp_connection_" + parent_group_id + "_" + Session["platform"].ToString());


////   dr_menu_ID.SetSelectsQuery(sQuery2);


//string sTable = theRecord.GetTableHTML("adm_operator_menus.aspx?submited=1");

//return sTable;
//  return table;
