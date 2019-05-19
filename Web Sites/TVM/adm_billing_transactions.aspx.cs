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

public partial class adm_billing_transactions : System.Web.UI.Page
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
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(2, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
            if (Request.QueryString["search_save"] != null)
                Session["search_save"] = "1";
            else
                Session["search_save"] = null;

            if (Request.QueryString["parent_group_id"] != null)
                Session["parent_group_id"] = Request.QueryString["parent_group_id"].ToString();
            else
                Session["parent_group_id"] = null;
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
        //b.BILLING_REASON as 'Reason',
        theTable += "select b.id as id,b.SITE_GUID as 'User',b.LAST_FOUR_DIGITS as 'CC',b.PRICE as 'Amount',b.PAYMENT_METHOD_ADDITION as 'Addition',b.TOTAL_PRICE as 'Total',b.CURRENCY_CODE as 'Currency',CASE b.BILLING_STATUS when 0 then 'OK' ELSE 'Fail: '+CAST(b.BILLING_STATUS as nvarchar(5)) END as 'Status',CASE b.IS_RECURRING WHEN 1 THEN 'True' ELSE 'False' END as 'Recurring',b.MEDIA_FILE_ID as 'File',b.SUBSCRIPTION_CODE as 'Subscription',b.CELL_PHONE as 'MSISDN',lit.NAME as 'Provider',b.PURCHASE_ID as 'Purchase Ref',b.PAYMENT_NUMBER as 'Payment',CASE b.NUMBER_OF_PAYMENTS WHEN 0 Then 'Infinate' WHEN 1 Then 'One' ELSE CAST(b.PAYMENT_NUMBER as nvarchar(5)) END as '# of Payments',CONVERT(VARCHAR(12),b.CREATE_DATE, 103) as 'Date' from lu_implementation_type lit,billing_transactions b where lit.id=b.BILLING_PROVIDER and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("b.group_id", "=", nGroupID);
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        else
            theTable += " order by id desc";
        //theTable.AddHiddenField("ID");
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();
        bool bAdd = false;
        if (LoginManager.GetLoginGroupID() == 1)
            bAdd = true;
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, bAdd, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy, "adm_billing_transactions.aspx");
        theTable.Finish();
        theTable = null;
        return sTable;
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader());
    }
}
