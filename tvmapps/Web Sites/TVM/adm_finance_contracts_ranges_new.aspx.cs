using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;
using System.Data;

public partial class adm_finance_contracts_ranges_new : System.Web.UI.Page
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
                string sName = Request.Form["0_val"].ToString();

                string sMin = Request.Form["1_val"].ToString();
                Int32 nMin = int.Parse(sMin);

                string sMax = Request.Form["2_val"].ToString();
                Int32 nMax = int.Parse(sMax);

                string sRangeType = Request.Form["3_val"].ToString();
                Int32 nRangeType = int.Parse(sRangeType);

                string sStartFrom = Request.Form["4_val"].ToString();
                Int32 nStartFrom = int.Parse(sStartFrom);

                try
                {
                    Int32 nInside = 0;
                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery += "select count(*) as co from fr_financial_contracts_ranges where status=1 and is_active=1 ";
                    selectQuery += " and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", LoginManager.GetLoginGroupID().ToString());
                    if (Session["fr_contract_range_id"] != null && Session["fr_contract_range_id"].ToString() != "0")
                    {
                        selectQuery += " and ";
                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "<>", int.Parse(Session["fr_contract_range_id"].ToString()));
                    }
                    selectQuery += " and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("min", "=", nMin);
                    selectQuery += " and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("max", "=", nMax);
                    selectQuery += " and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("range_type", "=", nRangeType);
                    selectQuery += " and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("start_from", "=", nStartFrom);                    

                    if (selectQuery.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                        if (nCount > 0)
                        {
                            nInside = int.Parse(selectQuery.Table("query").DefaultView[0].Row["co"].ToString());
                        }
                    }
                    selectQuery.Finish();
                    selectQuery = null;

                    if (nInside == 0)
                        DBManipulator.DoTheWork();
                    else
                        Session["error_msg"] = "There is a parallel contract range - please check or use it";
                }
                catch
                {
                    Session["error_msg"] = "Please enter a valid numbers";
                }
                return;
            }

            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID, "adm_finance_contracts_ranges_new.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["fr_contract_range_id"] != null && Request.QueryString["fr_contract_range_id"].ToString() != "")
            {
                Session["fr_contract_range_id"] = int.Parse(Request.QueryString["fr_contract_range_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("fr_financial_contracts_ranges", "group_id", int.Parse(Session["fr_contract_range_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["fr_contract_range_id"] = 0;
        }
    }


    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object t = null; ;
        if (Session["fr_contract_range_id"] != null && Session["fr_contract_range_id"].ToString() != "" && int.Parse(Session["fr_contract_range_id"].ToString()) != 0)
            t = Session["fr_contract_range_id"];

        string sBack = "adm_finance_contracts_ranges.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("fr_financial_contracts_ranges", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        
        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr",true,60,128);
        dr_name.Initialize("Contract Range Name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_name);

        DataRecordShortDoubleField dr_min_range = new DataRecordShortDoubleField(true, 12, 12);
        dr_min_range.Initialize("Min range: ", "adm_table_header_nbg", "FormInput", "MIN", true);
        dr_min_range.SetDefault(0);
        theRecord.AddRecord(dr_min_range);

        DataRecordShortDoubleField dr_max_range = new DataRecordShortDoubleField(true, 12, 12);
        dr_max_range.Initialize("Max range: ", "adm_table_header_nbg", "FormInput", "MAX", true);
        dr_max_range.SetDefault(0);
        theRecord.AddRecord(dr_max_range);


        int rangTypeVal =  0;
        int startFromVal = 0;

        //Get current record RANGE_TYPE ,  START_FROM values 

        if (t != null)
        {

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += " select RANGE_TYPE as RANGE_TYPE_CODE, START_FROM as START_FROM_CODE ";
            selectQuery += " from fr_financial_contracts_ranges where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", t);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    rangTypeVal = int.Parse(selectQuery.Table("query").DefaultView[0].Row["RANGE_TYPE_CODE"].ToString());
                    startFromVal = int.Parse(selectQuery.Table("query").DefaultView[0].Row["START_FROM_CODE"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;

        }
        DataRecordDropDownField dr_range_type = new DataRecordDropDownField("lu_financial_range_type", "NAME", "ID", "", null, 60, true);
        dr_range_type.Initialize("Range Type", "adm_table_header_nbg", "FormInput", "RANGE_TYPE", false);
        dr_range_type.SetValue(rangTypeVal.ToString());
        theRecord.AddRecord(dr_range_type);

        DataRecordDropDownField dr_start_from = new DataRecordDropDownField("lu_financial_ranges_start_from", "NAME", "ID", "", null, 60, true);
        dr_start_from.Initialize("Start From", "adm_table_header_nbg", "FormInput", "START_FROM", false);
        dr_start_from.SetValue(startFromVal.ToString());
        theRecord.AddRecord(dr_start_from);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
                            
        theRecord.AddRecord(dr_groups);


        string sTable = theRecord.GetTableHTML("adm_finance_contracts_ranges_new.aspx?submited=1");

        return sTable;
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Contracts Ranges";
        if (Session["fr_contract_range_id"] != null && Session["fr_contract_range_id"].ToString() != "" && Session["fr_contract_range_id"].ToString() != "0")
            sRet += " - Edit";
        else
            sRet += " - New";
        Response.Write(sRet);
    }
}