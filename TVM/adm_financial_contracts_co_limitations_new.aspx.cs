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

public partial class adm_financial_contracts_co_limitations_new : System.Web.UI.Page
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
                string sStart = Request.Form["2_val"].ToString();
                string sEnd = Request.Form["3_val"].ToString();
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
                    selectQuery += "select count(*) as co from fr_financial_entity_limitations where status=1 and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("FINANCIAL_ENTITY_ID", "=", nEntityID);
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
                Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("fr_financial_entity_limitations", "group_id", int.Parse(Session["fr_contract_id"].ToString())).ToString());
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
        string sRet = PageUtils.GetPreHeader() + ": Content owners monthly limitations";
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
        string sBack = "adm_financial_contracts_co_limitations.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("fr_financial_entity_limitations", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_name);

        DataRecordDropDownField dr_currency = new DataRecordDropDownField("lu_currency", "name", "id", "", "", 60, false);
        dr_currency.Initialize("Valid for currency", "adm_table_header_nbg", "FormInput", "CURRENCY_CD", false);
        dr_currency.SetDefault(2);
        theRecord.AddRecord(dr_currency);

        DataRecordDateTimeField dr_start_date = new DataRecordDateTimeField(true);
        dr_start_date.Initialize("Start Validity Date", "adm_table_header_nbg", "FormInput", "START_DATE", false);
        dr_start_date.SetDefault(DateTime.UtcNow);
        theRecord.AddRecord(dr_start_date);

        DataRecordDateTimeField dr_end_date = new DataRecordDateTimeField(true);
        dr_end_date.Initialize("End Validity Date", "adm_table_header_nbg", "FormInput", "END_DATE", false);
        theRecord.AddRecord(dr_end_date);

        DataRecordShortDoubleField dr_min_fix_price = new DataRecordShortDoubleField(true, 12, 12);
        dr_min_fix_price.Initialize("Minimum monthly amount", "adm_table_header_nbg", "FormInput", "MIN_FIX_PRICE", true);
        dr_min_fix_price.SetDefault(0);
        theRecord.AddRecord(dr_min_fix_price);

        DataRecordShortDoubleField dr_max_fix_price = new DataRecordShortDoubleField(true, 12, 12);
        dr_max_fix_price.Initialize("Maximum monthly amount", "adm_table_header_nbg", "FormInput", "MAX_FIX_PRICE", true);
        dr_max_fix_price.SetDefault(0);
        theRecord.AddRecord(dr_max_fix_price);

        DataRecordShortIntField dr_entity = new DataRecordShortIntField(false, 9, 9);
        dr_entity.Initialize("Group", "adm_table_header_nbg", "FormInput", "FINANCIAL_ENTITY_ID", false);
        dr_entity.SetValue(Session["fr_entity_id"].ToString());
        theRecord.AddRecord(dr_entity);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_financial_contracts_co_limitations_new.aspx?submited=1");

        return sTable;
    }
}
