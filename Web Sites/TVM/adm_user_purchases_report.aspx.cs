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

public partial class adm_user_purchases_report : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;

    static public bool IsTvinciImpl()
    {
        Int32 nImplID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("users_connection");
        selectQuery += "select IMPLEMENTATION_ID from groups_modules_implementations where is_active=1 and status=1 and ";
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
        if (LoginManager.IsPagePermitted("adm_media.aspx") == false)
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
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID, "adm_users_list.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);

            if (Request.QueryString["user_id"] != null &&
                Request.QueryString["user_id"].ToString() != "")
            {
                Session["user_id"] = int.Parse(Request.QueryString["user_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("users", "group_id", int.Parse(Session["user_id"].ToString()), "users_connection").ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else if (Session["user_id"] == null || Session["user_id"].ToString() == "" || Session["user_id"].ToString() == "0")
            {
                LoginManager.LogoutFromSite("index.html");
                return;
            }
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ":" + ODBCWrapper.Utils.GetTableSingleVal("users", "userNAME", int.Parse(Session["user_id"].ToString()), "users_connection").ToString() + " User purchase report ");
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

    static protected DataTable GetDataTableForIDs(string sMediaIDs)
    {
        string[] sep = { "," };
        string[] sMediaIDsA = sMediaIDs.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        DataTable d = new DataTable();
        Int32 n = 0;
        d.Columns.Add(PageUtils.GetColumn("ID", n));
        System.Data.DataRow tmpRow = null;
        Int32 nCount = sMediaIDsA.Length;
        for (int i = 0; i < nCount; i++)
        {
            tmpRow = d.NewRow();
            tmpRow["ID"] = int.Parse(sMediaIDsA[i]);
            d.Rows.InsertAt(tmpRow, 0);
            d.AcceptChanges();
        }
        return d.Copy();
    }

    static public string GetWSURL(string sKey)
    {
        return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
    }

    protected void FillTheTableEditor(ref DBTableWebEditor theTable, string sOrderBy)
    {

        Int32 nGroupID = LoginManager.GetLoginGroupID();
        ca_ws.module p = new ca_ws.module();
        string sIP = "1.1.1.1";
        string sWSUserName = "";
        string sWSPass = "";
        TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "GetUserBillingHistory", "conditionalaccess", sIP, ref sWSUserName, ref sWSPass);
        string sWSURL = GetWSURL("conditionalaccess_ws");
        if (sWSURL != "")
            p.Url = sWSURL;

        string user = Session["user_id"].ToString();

        ca_ws.BillingTransactions casResponse = p.GetUserBillingHistory(sWSUserName, sWSPass, user, 0, 1000, ca_ws.TransactionHistoryOrderBy.CreateDateDesc);

        if (casResponse == null || casResponse.resp == null || casResponse.resp.Code != 0 || casResponse.transactions == null)
        {
            return;
        }

        var transactionsResponse = casResponse.transactions;

        if (transactionsResponse.m_nTransactionsCount > 1000)
        {
            casResponse = p.GetUserBillingHistory(sWSUserName, sWSPass, user, 0, transactionsResponse.m_nTransactionsCount, ca_ws.TransactionHistoryOrderBy.CreateDateDesc);

            if (casResponse == null || casResponse.resp == null || casResponse.resp.Code != 0 || casResponse.transactions == null)
            {
                return;
            }

            transactionsResponse = casResponse.transactions;
        }

        ca_ws.PermittedSubscriptionContainer[] permittedSubscriptions = p.GetUserPermittedSubscriptions(sWSUserName, sWSPass, user);

        DataTable d = new DataTable();
        Int32 n = 0;
        string s = "";
        DateTime dtt = DateTime.UtcNow;
        d.Columns.Add(PageUtils.GetColumn("ID", n));
        d.Columns.Add(PageUtils.GetColumn("Type", s));
        d.Columns.Add(PageUtils.GetColumn("Item Name", s));
        d.Columns.Add(PageUtils.GetColumn("Paid", s));
        d.Columns.Add(PageUtils.GetColumn("Action", s));
        d.Columns.Add(PageUtils.GetColumn("Action Date", s));
        d.Columns.Add(PageUtils.GetColumn("Validity", s));
        d.Columns.Add(PageUtils.GetColumn("Remarks", s));
        d.Columns.Add(PageUtils.GetColumn("PurchasedItemCode", s));
        d.Columns.Add(PageUtils.GetColumn("Cancancel", n));
        d.Columns.Add(PageUtils.GetColumn("Canstrech", n));
        d.Columns.Add(PageUtils.GetColumn("BasePurchasedID", n));
        d.Columns.Add(PageUtils.GetColumn("Canrenew", n));

        Int32 countTransactions = transactionsResponse.m_nTransactionsCount;
        
        var transactions = transactionsResponse.m_Transactions;

        for (int i = 0; i < countTransactions; i++)
        {
            var currentTransaction = transactions[i];

            System.Data.DataRow tmpRow = null;
            tmpRow = d.NewRow();
            tmpRow["ID"] = int.Parse(currentTransaction.m_sRecieptCode);
            tmpRow["Type"] = transactions[i].m_eItemType.ToString();
            tmpRow["PurchasedItemCode"] = currentTransaction.m_sPurchasedItemCode.ToString();
            tmpRow["BasePurchasedID"] = currentTransaction.m_nPurchaseID;

            tmpRow["Item Name"] = currentTransaction.m_sPurchasedItemName + " (" + currentTransaction.m_sPurchasedItemCode + ")";
            try
            {
                tmpRow["Paid"] = String.Format("{0:0.##}", currentTransaction.m_Price.m_dPrice) + transactions[i].m_Price.m_oCurrency.m_sCurrencySign + " (" + currentTransaction.m_ePaymentMethod.ToString();
            }
            catch
            {
                tmpRow["Paid"] = "";
            }
            if (currentTransaction.m_sPaymentMethodExtraDetails != "")
            {
                tmpRow["Paid"] += " - ";
                if (currentTransaction.m_ePaymentMethod == ca_ws.PaymentMethod.CreditCard ||
                    currentTransaction.m_ePaymentMethod == ca_ws.PaymentMethod.DebitCard)
                    tmpRow["Paid"] += "****";
                tmpRow["Paid"] += currentTransaction.m_sPaymentMethodExtraDetails;
            }
            if (tmpRow["Paid"].ToString() != "")
                tmpRow["Paid"] += ")";
            tmpRow["Remarks"] = currentTransaction.m_sRemarks;
            tmpRow["Action"] = currentTransaction.m_eBillingAction.ToString();
            tmpRow["Action Date"] = currentTransaction.m_dtActionDate.ToString("MM/dd/yyyy HH:mm");
            if (currentTransaction.m_eItemType == ca_ws.BillingItemsType.Subscription &&
                (currentTransaction.m_eBillingAction == ca_ws.BillingAction.Purchase ||
                currentTransaction.m_eBillingAction == ca_ws.BillingAction.RenewPayment))
            {
                tmpRow["Canstrech"] = "1";
                tmpRow["Validity"] = currentTransaction.m_dtStartDate.ToString("MM/dd/yyyy HH:mm") + "-" + currentTransaction.m_dtEndDate.ToString("MM/dd/yyyy HH:mm");
            }
            else
            {
                tmpRow["Canstrech"] = "0";
                tmpRow["Validity"] = "";
            }
            if (currentTransaction.m_eItemType == ca_ws.BillingItemsType.Subscription)
            {
                bool bSubExist = false;
                bool bSubRenewable = false;
                bool bIsRecurring = false;
                if (permittedSubscriptions != null)
                {
                    for (int j = 0; j < permittedSubscriptions.Length; j++)
                    {
                        if (permittedSubscriptions[j].m_sSubscriptionCode == currentTransaction.m_sPurchasedItemCode)
                        {
                            bSubExist = true;
                            bSubRenewable = permittedSubscriptions[j].m_bRecurringStatus;
                            bIsRecurring = permittedSubscriptions[j].m_bIsSubRenewable;
                        }
                    }
                }
                if (bIsRecurring == true && currentTransaction.m_dtEndDate > DateTime.UtcNow && bSubRenewable == true && bSubExist == true)
                    tmpRow["Cancancel"] = "1";
                else
                    tmpRow["Cancancel"] = "0";

                if (bIsRecurring == true && currentTransaction.m_dtEndDate > DateTime.UtcNow && bSubRenewable == false && bSubExist == true)
                    tmpRow["Canrenew"] = "1";
                else
                    tmpRow["Canrenew"] = "0";
            }

            d.Rows.InsertAt(tmpRow, d.Rows.Count);
            d.AcceptChanges();
        }
        theTable.FillDataTable(d.Copy());
        theTable.AddHiddenField("Cancancel");
        theTable.AddHiddenField("PurchasedItemCode");
        theTable.AddHiddenField("Canstrech");
        theTable.AddHiddenField("BasePurchasedID");
        theTable.AddHiddenField("Canrenew");
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("javascript:StopSubRenewals", "Stop renewals", "Cancancel=1");
            linkColumn1.AddQueryStringValue("user_id", user);
            linkColumn1.AddQueryStringValue("sub_code", "field=PurchasedItemCode");
            linkColumn1.AddQueryStringValue("purchase_id", "field=BasePurchasedID");
            theTable.AddLinkColumn(linkColumn1);
        }
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("javascript:RenewSubRenewals", "Activate renewals", "Canrenew=1");
            linkColumn1.AddQueryStringValue("user_id", user);
            linkColumn1.AddQueryStringValue("sub_code", "field=PurchasedItemCode");
            linkColumn1.AddQueryStringValue("purchase_id", "field=BasePurchasedID");
            theTable.AddLinkColumn(linkColumn1);
        }
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("javascript:StrechSub", "Stretch", "Canstrech=1");
            linkColumn1.AddQueryStringValue("user_id", user);
            linkColumn1.AddQueryStringValue("sub_code", "field=PurchasedItemCode");
            linkColumn1.AddQueryStringValue("purchase_id", "field=BasePurchasedID");
            linkColumn1.AddQueryStringValue("sub_renewable", "field=Cancancel");

            theTable.AddLinkColumn(linkColumn1);
        }
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();

        DBTableWebEditor theTable = new DBTableWebEditor(true, true, false, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        theTable.SetConnectionKey("users_connection");
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy, false);
        Session["ContentPage"] = "adm_users_list.aspx";
        Session["LastContentPage"] = "adm_user_purchases_report.aspx?search_save=1";
        Session["order_by"] = sOldOrderBy;
        theTable.Finish();
        theTable = null;
        return sTable;
    }
}
