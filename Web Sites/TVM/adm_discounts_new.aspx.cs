using CachingProvider.LayeredCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_discounts_new : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    protected string m_sMenu;
    protected string m_sSubMenu;

    static public bool IsTvinciImpl()
    {
        Int32 nImplID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("pricing_connection");
        selectQuery += "select * from groups_modules_implementations where is_active=1 and status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_ID", "=", 2);
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

    protected void Page_Load(object sender, EventArgs e)
    {
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            if (IsTvinciImpl() == false)
            {
                Server.Transfer("adm_module_not_implemented.aspx");
                return;
            }

            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork("pricing_connection");

                string invalidationKey = LayeredCacheKeys.GetGroupDiscountsInvalidationKey(LoginManager.GetLoginGroupID());
                if (!CachingProvider.LayeredCache.LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to set invalidation key for CouponsGroupsInvalidationKey. key = {0}", invalidationKey);
                }

                return;
            }
            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["discount_code_id"] != null &&
                Request.QueryString["discount_code_id"].ToString() != "")
            {
                Session["discount_code_id"] = int.Parse(Request.QueryString["discount_code_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("discount_codes", "group_id", int.Parse(Session["discount_code_id"].ToString()), "pricing_connection").ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["discount_code_id"] = 0;
        }

    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Discount Codes";
        if (Session["discount_code_id"] != null && Session["discount_code_id"].ToString() != "" && Session["discount_code_id"].ToString() != "0")
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
        if (Session["discount_code_id"] != null && Session["discount_code_id"].ToString() != "" && int.Parse(Session["discount_code_id"].ToString()) != 0)
            t = Session["discount_code_id"];
        string sBack = "adm_discounts.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("discount_codes", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("pricing_connection");

        DataRecordShortTextField dr_domain = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_domain.Initialize("Code", "adm_table_header_nbg", "FormInput", "Code", true);
        theRecord.AddRecord(dr_domain);

        DataRecordShortDoubleField dr_price = new DataRecordShortDoubleField(true, 12, 12);
        dr_price.Initialize("Fix amount of: ", "adm_table_header_nbg", "FormInput", "PRICE", true);
        dr_price.SetDefault(0);
        theRecord.AddRecord(dr_price);

        DataRecordDropDownField dr_currency = new DataRecordDropDownField("lu_currency", "name", "id", "", "", 60, false);
        dr_currency.Initialize("Currency", "adm_table_header_nbg", "FormInput", "CURRENCY_CD", false);
        dr_currency.SetDefault(2);
        theRecord.AddRecord(dr_currency);

        DataRecordShortDoubleField dr_per = new DataRecordShortDoubleField(true, 12, 12);
        dr_per.Initialize("Percentage of: ", "adm_table_header_nbg", "FormInput", "DISCOUNT_PERCENT", true);
        dr_per.SetDefault(0);
        theRecord.AddRecord(dr_per);

        DataRecordDateTimeField dr_start_date = new DataRecordDateTimeField(true);
        dr_start_date.Initialize("Start Date", "adm_table_header_nbg", "FormInput", "START_DATE", false);
        dr_start_date.SetDefault(DateTime.UtcNow);
        theRecord.AddRecord(dr_start_date);

        DataRecordDateTimeField dr_end_date = new DataRecordDateTimeField(true);
        dr_end_date.Initialize("End Date", "adm_table_header_nbg", "FormInput", "END_DATE", false);
        dr_end_date.SetDefault(new DateTime(2099, 1, 1));
        theRecord.AddRecord(dr_end_date);

        DataRecordRadioField dr_whenalgo = new DataRecordRadioField("lu_WHENALGO_TYPE", "description", "id", "", null);
        dr_whenalgo.Initialize("Validity of discount", "adm_table_header_nbg", "FormInput", "WHENALGO_TYPE", true);
        theRecord.AddRecord(dr_whenalgo);

        DataRecordShortIntField dr_whendelta = new DataRecordShortIntField(true, 9, 9);
        dr_whendelta.Initialize("N (Validity of discount) equals", "adm_table_header_nbg", "FormInput", "WHENALGO_TIMES", true);
        dr_whendelta.SetDefault(1);
        theRecord.AddRecord(dr_whendelta);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_discounts_new.aspx?submited=1");

        return sTable;
    }
}
