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

public partial class adm_financial_contracts_co_list_new : System.Web.UI.Page
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
                string sLicOrSub = Request.Form["1_val"].ToString();
                string sCurrency = Request.Form["2_val"].ToString();
                string sRule = Request.Form["3_val"].ToString();
                string sStart = Request.Form["4_val"].ToString();
                string sEnd = Request.Form["5_val"].ToString();
                if (sEnd == "")
                    sEnd = "1/1/2500";
                DateTime dStart = DateUtils.GetDateFromStr(sStart);
                DateTime dEnd = DateUtils.GetDateFromStr(sEnd);
                if (dStart > dEnd)
                {
                    Session["error_msg"] = "Please enter valid dates";
                }
                try
                {
                    Int32 nInside = 0;
                    Int32 nEntityID = int.Parse(Session["fr_entity_id"].ToString());
                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery += "select count(*) as co from fr_financial_entity_contracts where status=1 and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("FINANCIAL_ENTITY_ID", "=", nEntityID);
                    if (sRule == "")
                        sRule = "0";
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRIES_RULE_ID", "=", int.Parse(sRule));
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LICENSE_OR_SUB", "=", int.Parse(sLicOrSub));
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
                    {
                        DBManipulator.DoTheWork();
                        return;
                    }
                    else
                        Session["error_msg"] = "There is a parallel contract - please check and enter the contract again";
                }
                catch
                {
                    Session["error_msg"] = "Please enter a valid numbers";
                }
                
            }
            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID, "adm_financial_contracts_co.aspx");
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
        string sRet = PageUtils.GetPreHeader() + ": Content owners contracts";
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
        string sBack = "adm_financial_contracts_co_list.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("fr_financial_entity_contracts", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_name);
        
        DataRecordRadioField dr_l_or_s = new DataRecordRadioField("lu_license_or_sub", "description", "id", "", null);
        dr_l_or_s.Initialize("Related to", "adm_table_header_nbg", "FormInput", "LICENSE_OR_SUB", true);
        dr_l_or_s.SetDefault(0);
        theRecord.AddRecord(dr_l_or_s);
        
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
        dr_end_date.SetDefault(new DateTime(2099, 1, 1));
        theRecord.AddRecord(dr_end_date);

        DataRecordShortDoubleField dr_fix_price = new DataRecordShortDoubleField(true, 12, 12);
        dr_fix_price.Initialize("Fix amount of: ", "adm_table_header_nbg", "FormInput", "FIX_PRICE", true);
        dr_fix_price.SetDefault(0);
        theRecord.AddRecord(dr_fix_price);

        DataRecordShortDoubleField dr_per = new DataRecordShortDoubleField(true, 12, 12);
        dr_per.Initialize("Percentage of: ", "adm_table_header_nbg", "FormInput", "PER", true);
        dr_per.SetDefault(0);
        theRecord.AddRecord(dr_per);

        DataRecordShortDoubleField dr_min_price = new DataRecordShortDoubleField(true, 12, 12);
        dr_min_price.Initialize("Min Amount (0 for none): ", "adm_table_header_nbg", "FormInput", "MIN_AMOUNT", true);
        dr_min_price.SetDefault(0);
        theRecord.AddRecord(dr_min_price);

        DataRecordShortDoubleField dr_max_price = new DataRecordShortDoubleField(true, 12, 12);
        dr_max_price.Initialize("Max Amount (0 for none): ", "adm_table_header_nbg", "FormInput", "MAX_AMOUNT", true);
        dr_max_price.SetDefault(0);
        theRecord.AddRecord(dr_max_price);

        //DataRecordBoolField dr_is_minmax_relevant = new DataRecordBoolField(true);
        //dr_is_minmax_relevant.Initialize("Is Min/Max amount relevant", "adm_table_header_nbg", "FormInput", "IS_OR_PRICE_RELEVANT", false);
        //theRecord.AddRecord(dr_is_minmax_relevant);

        //DataRecordShortDoubleField dr_min_max_price = new DataRecordShortDoubleField(true, 12, 12);
        //dr_min_max_price.Initialize("Min/Max amount: ", "adm_table_header_nbg", "FormInput", "OR_PRICE", true);
        //dr_min_max_price.SetDefault(0);
        //theRecord.AddRecord(dr_min_max_price);

        //DataRecordRadioField dr_minmax = new DataRecordRadioField("lu_lower_or_higher", "description", "id", "", null);
        //dr_minmax.Initialize("Min/Max Amount", "adm_table_header_nbg", "FormInput", "LOWER_OR_HIGHER", true);
        //dr_minmax.SetDefault(0);
        //theRecord.AddRecord(dr_minmax);

        DataRecordRadioField dr_out_of = new DataRecordRadioField("lu_out_of_type", "description", "id", "", null);
        dr_out_of.Initialize("Calculated on:", "adm_table_header_nbg", "FormInput", "OUT_OF_TYPE", true);
        dr_out_of.SetDefault(0);
        theRecord.AddRecord(dr_out_of);

        DataRecordShortIntField dr_level_n = new DataRecordShortIntField(true, 9, 9);
        dr_level_n.Initialize("Calculated On Level(when On Level is marked)", "adm_table_header_nbg", "FormInput", "CALC_ON_LEVEL", false);
        dr_level_n.SetDefault(0);
        theRecord.AddRecord(dr_level_n);

        DataRecordShortIntField dr_level_n1 = new DataRecordShortIntField(true, 9, 9);
        dr_level_n1.Initialize("Belong to Level N (0 and Up)", "adm_table_header_nbg", "FormInput", "LEVEL_NUM", false);
        dr_level_n1.SetDefault(0);
        theRecord.AddRecord(dr_level_n1);

        DataRecordShortIntField dr_entity = new DataRecordShortIntField(false, 9, 9);
        dr_entity.Initialize("Group", "adm_table_header_nbg", "FormInput", "FINANCIAL_ENTITY_ID", false);
        dr_entity.SetValue(Session["fr_entity_id"].ToString());
        theRecord.AddRecord(dr_entity);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_financial_contracts_co_list_new.aspx?submited=1");

        return sTable;
    }
}
