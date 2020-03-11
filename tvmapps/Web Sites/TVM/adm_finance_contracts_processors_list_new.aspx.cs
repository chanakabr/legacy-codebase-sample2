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

public partial class adm_finance_contracts_processors_list_new : System.Web.UI.Page
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
                string sCurrency = Request.Form["1_val"].ToString();
                string sRule = Request.Form["2_val"].ToString();
                string sStart = Request.Form["3_val"].ToString();
                string sEnd = Request.Form["4_val"].ToString();
                string sMinAmount = Request.Form["5_val"].ToString();
                double dMinAmount = double.Parse(sMinAmount);
                string sMaxAmount = Request.Form["6_val"].ToString();
                double dMaxAmount = double.Parse(sMaxAmount);
                if (dMaxAmount == 0.0)
                    dMaxAmount = 9999999;
               
                if (sEnd == "")
                    sEnd = "1/1/2500";
                DateTime dStart = DateUtils.GetDateFromStr(sStart);
                DateTime dEnd = DateUtils.GetDateFromStr(sEnd);
                if (dStart > dEnd)
                {
                    Session["error_msg"] = "Please enter valid dates";
                }
                string sContractRange = Request.Form["14_val"].ToString();
                Int32 nContractRang = int.Parse(sContractRange);

                try
                {
                    bool bContractOk = false;
                    bool bContractRangeIsNull = false;
                    int ncontractRangeId = 0;
                    Int32 nCount = 0;
                    Int32 nEntityID = int.Parse(Session["fr_entity_id"].ToString());
                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery += "select contract_range_id from fr_financial_entity_contracts where status=1 and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("FINANCIAL_ENTITY_ID", "=", nEntityID);
                    if (sRule == "")
                        sRule = "0";
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRIES_RULE_ID", "=", int.Parse(sRule));
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CD", "=", int.Parse(sCurrency));
                    if (Session["fr_contract_id"] != null && Session["fr_contract_id"].ToString() != "0")
                    {
                        selectQuery += "and";
                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "<>", int.Parse(Session["fr_contract_id"].ToString()));
                    }
                    selectQuery += " and (";
                    selectQuery += "(";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("start_date", ">=", dStart);
                    selectQuery += " and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("start_date", "<=", dEnd);
                    selectQuery += ")";
                    selectQuery += "or";
                    selectQuery += "(";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("end_date", ">=", dStart);
                    selectQuery += " and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("end_date", "<=", dEnd);
                    selectQuery += ")";
                    selectQuery += "or";
                    selectQuery += "( end_date is null and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("start_date", "<=", dEnd);
                    selectQuery += ")";
                    selectQuery += ")";

                    selectQuery += " and (";
                    selectQuery += "(";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MIN_AMOUNT", ">", dMinAmount);
                    selectQuery += " and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MIN_AMOUNT", "<", dMaxAmount);
                    selectQuery += ")";

                    selectQuery += "or";
                    selectQuery += "(";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_AMOUNT", ">", dMinAmount);
                    selectQuery += " and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_AMOUNT", "<", dMaxAmount);
                    selectQuery += ")";

                    selectQuery += "or";
                    selectQuery += "( MAX_AMOUNT=0 and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MIN_AMOUNT", "<", dMaxAmount);
                    selectQuery += ")";
                    selectQuery += ")";
                  
                    if (selectQuery.Execute("query", true) != null)
                    {
                        nCount = selectQuery.Table("query").DefaultView.Count;
                       


                        if (nCount == 0)
                            bContractOk = true;
                        else
                        {
                            if (nContractRang > 0)
                            {

                                string ContractRangIds = "(";
                                for (int i = 0; i < nCount; i++)
                                {
                                    if (i > 0)
                                    {
                                        ContractRangIds += ",";
                                    }

                                    ncontractRangeId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "contract_range_id", i);
                                    if (ncontractRangeId == 0)//there is a contract with no contract range - so can't save the new one 
                                    {
                                        bContractRangeIsNull = true; 
                                        bContractOk = false;
                                    }

                                    ContractRangIds += ncontractRangeId.ToString();
                                }
                                ContractRangIds += ")";

                                double dMin = 0;
                                double dMax = 0;
                                int nRangeType = 0;
                                int nStartFrom = 0;

                                if (!bContractRangeIsNull) //if there were one contract that isn't relates to any contract range - new contract can't be saved
                                {
                                    ODBCWrapper.DataSetSelectQuery selectContractRangeQuery = new ODBCWrapper.DataSetSelectQuery();
                                    selectContractRangeQuery += "select * from fr_financial_contracts_ranges where status=1 and is_active = 1 and ";
                                    selectContractRangeQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nContractRang);
                                    if (selectContractRangeQuery.Execute("query", true) != null)
                                    {
                                        nCount = selectContractRangeQuery.Table("query").DefaultView.Count;
                                        if (nCount > 0)
                                        {
                                            dMin = ODBCWrapper.Utils.GetDoubleSafeVal(selectContractRangeQuery, "Min", 0);
                                            dMax = ODBCWrapper.Utils.GetDoubleSafeVal(selectContractRangeQuery, "Max", 0);
                                            nRangeType = ODBCWrapper.Utils.GetIntSafeVal(selectContractRangeQuery, "RANGE_TYPE", 0);
                                            nStartFrom = ODBCWrapper.Utils.GetIntSafeVal(selectContractRangeQuery, "START_FROM", 0);
                                        }
                                    }
                                    selectContractRangeQuery.Finish();
                                    selectContractRangeQuery = null;


                                    selectContractRangeQuery = new ODBCWrapper.DataSetSelectQuery();
                                    selectContractRangeQuery += "select id from fr_financial_contracts_ranges where status=1 and is_active = 1";
                                    selectContractRangeQuery += "and ID in " + ContractRangIds;
                                    selectContractRangeQuery += "and";
                                    selectContractRangeQuery += ODBCWrapper.Parameter.NEW_PARAM("RANGE_TYPE", "=", nRangeType);
                                    selectContractRangeQuery += "and";
                                    selectContractRangeQuery += ODBCWrapper.Parameter.NEW_PARAM("START_FROM", "=", nStartFrom);
                                    selectContractRangeQuery += " and (";
                                    selectContractRangeQuery += "(";
                                    selectContractRangeQuery += ODBCWrapper.Parameter.NEW_PARAM("MIN", ">=", dMin);
                                    selectContractRangeQuery += " and ";
                                    selectContractRangeQuery += ODBCWrapper.Parameter.NEW_PARAM("MIN", "<", dMax);
                                    selectContractRangeQuery += ")";
                                    selectContractRangeQuery += "or";
                                    selectContractRangeQuery += "(";
                                    selectContractRangeQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX", ">=", dMin);
                                    selectContractRangeQuery += " and ";
                                    selectContractRangeQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX", "<", dMax);
                                    selectContractRangeQuery += ")";
                                    selectContractRangeQuery += "or";
                                    selectContractRangeQuery += "(";
                                    selectContractRangeQuery += ODBCWrapper.Parameter.NEW_PARAM("MIN", ">=", dMin);
                                    selectContractRangeQuery += " and ";
                                    selectContractRangeQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX", "<", dMax);
                                    selectContractRangeQuery += ")";

                                    selectContractRangeQuery += "or";
                                    selectContractRangeQuery += "( MAX=0 and ";
                                    selectContractRangeQuery += ODBCWrapper.Parameter.NEW_PARAM("MIN", "<", dMax);
                                    selectContractRangeQuery += ")";
                                    selectContractRangeQuery += ")";

                                    if (selectContractRangeQuery.Execute("query", true) != null)
                                    {
                                        nCount = selectContractRangeQuery.Table("query").DefaultView.Count;
                                        if (nCount == 0)
                                            bContractOk = true;
                                    }
                                    selectContractRangeQuery.Finish();
                                    selectContractRangeQuery = null;
                                }
                            }
                        }
                    }
                    selectQuery.Finish();
                    selectQuery = null;

                    if (bContractOk)
                        DBManipulator.DoTheWork();
                    else
                        Session["error_msg"] = "There is a parallel contract - please check and enter the contract again";
                }

                catch
                {
                    Session["error_msg"] = "Please enter a valid numbers";
                }
                return;
            }
            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID, "adm_finance_contracts_processors.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["fr_contract_id"] != null &&
                Request.QueryString["fr_contract_id"].ToString() != "")
            {
                Session["fr_contract_id"] = int.Parse(Request.QueryString["fr_contract_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("fr_financial_entity_contracts", "group_id", int.Parse(Session["fr_contract_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["fr_contract_id"] = 0;
        }

    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Processors contracts";
        if (Session["fr_contract_id"] != null && Session["fr_contract_id"].ToString() != "" && Session["fr_contract_id"].ToString() != "0")
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
        object t = null; ;
        if (Session["fr_contract_id"] != null && Session["fr_contract_id"].ToString() != "" && int.Parse(Session["fr_contract_id"].ToString()) != 0)
            t = Session["fr_contract_id"];
        string sBack = "adm_finance_contracts_processors_list.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("fr_financial_entity_contracts", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_name);

        DataRecordDropDownField dr_currency = new DataRecordDropDownField("lu_currency", "name", "id", "", "", 60, false);
        dr_currency.Initialize("Valid for currency", "adm_table_header_nbg", "FormInput", "CURRENCY_CD", false);
        dr_currency.SetDefault(2);
        theRecord.AddRecord(dr_currency);

        DataRecordDropDownField dr_block_rules = new DataRecordDropDownField("geo_block_types", "NAME", "id", "", null, 60, true);
        string sQuery = "select name as txt,id as id from geo_block_types where GEO_RULE_TYPE=2 and status=1 and group_id= " + LoginManager.GetLoginGroupID().ToString();
        dr_block_rules.SetSelectsQuery(sQuery);
        dr_block_rules.Initialize("Countries rule", "adm_table_header_nbg", "FormInput", "COUNTRIES_RULE_ID", false);
        //dr_block_rules.SetDefaultVal(sDefBR);
        theRecord.AddRecord(dr_block_rules);

        DataRecordDateTimeField dr_start_date = new DataRecordDateTimeField(true);
        dr_start_date.Initialize("Start Validity Date", "adm_table_header_nbg", "FormInput", "START_DATE", false);
        dr_start_date.SetDefault(DateTime.UtcNow);
        theRecord.AddRecord(dr_start_date);

        DataRecordDateTimeField dr_end_date = new DataRecordDateTimeField(true);
        dr_end_date.Initialize("End Validity Date", "adm_table_header_nbg", "FormInput", "END_DATE", false);
        //dr_end_date.SetDefault(new DateTime(2099, 1, 1));
        theRecord.AddRecord(dr_end_date);

        DataRecordShortDoubleField dr_min_amount = new DataRecordShortDoubleField(true, 12, 12);
        dr_min_amount.Initialize("Valid for amounts higher or equal to: ", "adm_table_header_nbg", "FormInput", "MIN_AMOUNT", true);
        dr_min_amount.SetDefault(0);
        theRecord.AddRecord(dr_min_amount);

        DataRecordShortDoubleField dr_max_amount = new DataRecordShortDoubleField(true, 12, 12);
        dr_max_amount.Initialize("Valid for amounts lower than (0 for no limit): ", "adm_table_header_nbg", "FormInput", "MAX_AMOUNT", true);
        dr_max_amount.SetDefault(0);
        theRecord.AddRecord(dr_max_amount);

        DataRecordShortDoubleField dr_fix_price = new DataRecordShortDoubleField(true, 12, 12);
        dr_fix_price.Initialize("Fix amount of: ", "adm_table_header_nbg", "FormInput", "FIX_PRICE", true);
        dr_fix_price.SetDefault(0);
        theRecord.AddRecord(dr_fix_price);

        DataRecordShortDoubleField dr_per = new DataRecordShortDoubleField(true, 12, 12);
        dr_per.Initialize("Percentage of: ", "adm_table_header_nbg", "FormInput", "PER", true);
        dr_per.SetDefault(0);
        theRecord.AddRecord(dr_per);

        DataRecordBoolField dr_is_minmax_relevant = new DataRecordBoolField(true);
        dr_is_minmax_relevant.Initialize("Is Min/Max amount relevant", "adm_table_header_nbg", "FormInput", "IS_OR_PRICE_RELEVANT", false);
        theRecord.AddRecord(dr_is_minmax_relevant);

        DataRecordShortDoubleField dr_min_max_price = new DataRecordShortDoubleField(true, 12, 12);
        dr_min_max_price.Initialize("Min/Max amount: ", "adm_table_header_nbg", "FormInput", "OR_PRICE", true);
        dr_min_max_price.SetDefault(0);
        theRecord.AddRecord(dr_min_max_price);

        DataRecordRadioField dr_minmax = new DataRecordRadioField("lu_lower_or_higher", "description", "id", "", null);
        dr_minmax.Initialize("Min/Max Amount", "adm_table_header_nbg", "FormInput", "LOWER_OR_HIGHER", true);
        dr_minmax.SetDefault(0);
        theRecord.AddRecord(dr_minmax);

        DataRecordShortIntField dr_entity = new DataRecordShortIntField(false, 9, 9);
        dr_entity.Initialize("Group", "adm_table_header_nbg", "FormInput", "FINANCIAL_ENTITY_ID", false);
        dr_entity.SetValue(Session["fr_entity_id"].ToString());
        theRecord.AddRecord(dr_entity);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        //Show all relavent Range Contract
        DataRecordDropDownField dr_contract_range = new DataRecordDropDownField("fr_financial_contracts_ranges", "NAME", "id", "group_id",LoginManager.GetLoginGroupID() , 60, true);
        //dr_start_from.SetValue(startFromVal.ToString());
        dr_contract_range.Initialize("Contract range", "adm_table_header_nbg", "FormInput", "CONTRACT_RANGE_ID", false);
        theRecord.AddRecord(dr_contract_range);








        string sTable = theRecord.GetTableHTML("adm_finance_contracts_processors_list_new.aspx?submited=1");

        return sTable;
    }
}
