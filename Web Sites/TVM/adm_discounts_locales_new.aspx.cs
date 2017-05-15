using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_discounts_locales_new : System.Web.UI.Page
{

    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsPagePermitted("adm_discounts.aspx") == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            if (IsTvinciImpl() == false)
            {
                Server.Transfer("adm_module_not_implemented.aspx");
                return;
            }
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID, "adm_discounts.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork("pricing_connection");
                return;
            }

            if (Session["discount_code_id"] == null || Session["discount_code_id"].ToString() == "" || Session["discount_code_id"].ToString() == "0")
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }

            if (Request.QueryString["discount_code_locale_id"] != null && Request.QueryString["discount_code_locale_id"].ToString() != "")
            {
                Session["discount_code_locale_id"] = int.Parse(Request.QueryString["discount_code_locale_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("discount_codes_locales", "group_id", int.Parse(Session["discount_code_locale_id"].ToString()), "pricing_connection").ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["discount_code_locale_id"] = "0";
        }
    }

    static public bool IsTvinciImpl()
    {
        Int32 nImplID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("pricing_connection");
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
        if (nImplID == 1)
            return true;
        return false;
    }

    public void GetHeader()
    {
        if (Session["discount_code_locale_id"] != null && Session["discount_code_locale_id"].ToString() != "" && int.Parse(Session["discount_code_locale_id"].ToString()) != 0)
            Response.Write(PageUtils.GetPreHeader() + ":Discount code Locale - Edit");
        else
            Response.Write(PageUtils.GetPreHeader() + ":Discount code Locale - New");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
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
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        object t = null; ;
        if (Session["discount_code_locale_id"] != null && Session["discount_code_locale_id"].ToString() != "" && int.Parse(Session["discount_code_locale_id"].ToString()) != 0)
            t = Session["discount_code_locale_id"];
        string sBack = "adm_discounts_locales.aspx?search_save=1&discount_code_id=" + Session["discount_code_id"].ToString();
        DBRecordWebEditor theRecord = new DBRecordWebEditor("discount_codes_locales", "adm_table_pager", sBack, "", "ID", t, sBack, "discount_code_locale_id");
        theRecord.SetConnectionKey("pricing_connection");

        DataRecordDropDownField dr_country = new DataRecordDropDownField("countries", "COUNTRY_NAME", "COUNTRY_CD2", "", null, 60, false);
        dr_country.SetConnectionKey("main_connection");
        dr_country.SetFieldType("string");
        dr_country.Initialize("Country", "adm_table_header_nbg", "FormInput", "COUNTRY_CODE", false);
        dr_country.SetOrderBy("ID");
        theRecord.AddRecord(dr_country);

        DataRecordShortDoubleField dr_price = new DataRecordShortDoubleField(true, 12, 12);
        dr_price.Initialize("Price", "adm_table_header_nbg", "FormInput", "PRICE", true);
        dr_price.SetDefault(0);
        theRecord.AddRecord(dr_price);

        DataRecordShortDoubleField dr_per = new DataRecordShortDoubleField(true, 12, 12);
        dr_per.Initialize("Percentage of: ", "adm_table_header_nbg", "FormInput", "DISCOUNT_PERCENT", true);
        dr_per.SetDefault(0);
        theRecord.AddRecord(dr_per);

        DataRecordDropDownField dr_currency = new DataRecordDropDownField("lu_currency", "name", "id", "", "", 60, false);
        dr_currency.SetConnectionKey("pricing_connection");
        dr_currency.Initialize("Currency", "adm_table_header_nbg", "FormInput", "CURRENCY_CD", false);
        dr_currency.SetDefault(2);
        theRecord.AddRecord(dr_currency);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_discount_code_locale_id = new DataRecordShortIntField(false, 9, 9);
        dr_discount_code_locale_id.Initialize("Discount Code ID", "adm_table_header_nbg", "FormInput", "discount_code_id", false);
        dr_discount_code_locale_id.SetValue(Session["discount_code_id"].ToString());
        theRecord.AddRecord(dr_discount_code_locale_id);

        string sTable = theRecord.GetTableHTML("adm_discounts_locales_new.aspx?submited=1");
        return sTable;
    }

}