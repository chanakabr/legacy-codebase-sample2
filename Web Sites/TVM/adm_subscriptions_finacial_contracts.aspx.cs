using ConfigurationManager;
using System;
using TVinciShared;

public partial class adm_subscriptions_finacial_contracts : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_finance_subscriptions_management.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID, "adm_finance_subscriptions_management.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
            if (Request.QueryString["subscription_id"] != null &&
                Request.QueryString["subscription_id"].ToString() != "")
            {
                Session["subscription_id"] = Request.QueryString["subscription_id"].ToString();
            }
            else if (Session["subscription_id"] == null || Session["subscription_id"].ToString() == "")
            {
                LoginManager.LogoutFromSite("index.html");
                return;
            }
        }
    }

    public void GetHeader()
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        //Response.Write(nGroupID.ToString());
        //return;
        string sWSUserName = "";
        string sWSPass = "";

        string sIP = "1.1.1.1";
        Int32 nCommerceGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", nGroupID).ToString());
        if (nCommerceGroupID == 0)
            nCommerceGroupID = nGroupID;
        TVinciShared.WS_Utils.GetWSUNPass(nCommerceGroupID, "GetSubscriptionsList", "pricing", sIP, ref sWSUserName, ref sWSPass);
        TvinciPricing.mdoule m = new TvinciPricing.mdoule();
        string sWSURL = ApplicationConfiguration.WebServicesConfiguration.Pricing.URL.Value;
        if (sWSURL != "")
            m.Url = sWSURL;
        TvinciPricing.Subscription oSubscription = m.GetSubscriptionData(sWSUserName, sWSPass , Session["subscription_id"].ToString(), string.Empty,string.Empty,string.Empty,false);

        string sRet = PageUtils.GetPreHeader() + ":";
        sRet += oSubscription.m_sObjectVirtualName + "(" + oSubscription.m_sObjectCode + ")";
        Response.Write(sRet + " : Financial contracts ");
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
    }

    public string GetIPAddress()
    {
        string strHostName = System.Net.Dns.GetHostName();
        System.Net.IPHostEntry ipHostInfo = System.Net.Dns.Resolve(System.Net.Dns.GetHostName());
        System.Net.IPAddress ipAddress = ipHostInfo.AddressList[0];

        return ipAddress.ToString();
    }

    protected void InsertSubscriptionsContractFamilyID(Int32 nContractFamilyID, string sSubscriptionID, Int32 nGroupID)
    {
        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("fr_subscriptions_contract_families");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_code", "=", sSubscriptionID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CONTRACT_FAMILY_ID", "=", nContractFamilyID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
        Int32 nCommerceGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", nGroupID).ToString());
        if (nCommerceGroupID == 0)
            nCommerceGroupID = nGroupID;
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nCommerceGroupID);
        insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;
    }

    protected void UpdateSubscriptionsContractFamilyID(Int32 nID, Int32 nStatus)
    {
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("fr_subscriptions_contract_families");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", nStatus);
        updateQuery += "where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nID);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;
    }

    protected Int32 GetSubscriptionsContractFamilyID(Int32 nContractFamilyID, Int32 nLogedInGroupID, ref Int32 nStatus)
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select id,status from fr_subscriptions_contract_families where is_active=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_code", "=", int.Parse(Session["subscription_id"].ToString()));
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CONTRACT_FAMILY_ID", "=", nContractFamilyID);
        selectQuery += "and";
        Int32 nCommerceGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", nLogedInGroupID).ToString());
        if (nCommerceGroupID == 0)
            nCommerceGroupID = nLogedInGroupID;
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nCommerceGroupID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                nStatus = int.Parse(selectQuery.Table("query").DefaultView[0].Row["STATUS"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return nRet;
    }

    public string changeItemStatus(string sID, string sAction)
    {
        Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
        if (Session["subscription_id"] == null || Session["subscription_id"].ToString() == "" || Session["subscription_id"].ToString() == "0")
        {
            LoginManager.LogoutFromSite("index.html");
            return "";
        }
        Int32 nStatus = 0;
        Int32 nSubscriptionsContractFamilyID = GetSubscriptionsContractFamilyID(int.Parse(sID), nLogedInGroupID, ref nStatus);
        if (nSubscriptionsContractFamilyID != 0)
        {
            if (nStatus == 0)
                UpdateSubscriptionsContractFamilyID(nSubscriptionsContractFamilyID, 1);
            else
                UpdateSubscriptionsContractFamilyID(nSubscriptionsContractFamilyID, 0);
        }
        else
        {
            InsertSubscriptionsContractFamilyID(int.Parse(sID), Session["subscription_id"].ToString(), nLogedInGroupID);
        }

        return "";
    }

    protected bool IsContractFamilyBelongToMediaFile(string sSubscriptioCode, Int32 nFinancContractEntityID)
    {
        bool bRet = false;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select id from fr_subscriptions_contract_families where status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CONTRACT_FAMILY_ID", "=", nFinancContractEntityID);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_code", "=", sSubscriptioCode);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
                bRet = true;
        }
        selectQuery.Finish();
        selectQuery = null;
        return bRet;
    }

    public void GetSubscriptionID()
    {
        Response.Write(Session["subscription_id"].ToString());
    }

    public string initDualObj()
    {
        Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
        if (Session["subscription_id"] == null || Session["subscription_id"].ToString() == "")
        {
            LoginManager.LogoutFromSite("index.html");
            return "";
        }

        string sRet = "";
        sRet += "Current Financial Contracts Families";
        sRet += "~~|~~";
        sRet += "Available Financial Contracts Families";
        sRet += "~~|~~";

        string sIP = "1.1.1.1";
        Int32 nCommerceGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", nLogedInGroupID).ToString());
        if (nCommerceGroupID == 0)
            nCommerceGroupID = nLogedInGroupID;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from fr_financial_entities where status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nCommerceGroupID);
        selectQuery += " and PARENT_ENTITY_ID<>0";
        selectQuery += "order by PARENT_ENTITY_ID";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            sRet += "<root>";
            for (int i = 0; i < nCount; i++)
            {
                string sID = selectQuery.Table("query").DefaultView[i].Row["ID"].ToString();
                Int32 nParentID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["PARENT_ENTITY_ID"].ToString());
                string sParentName = ODBCWrapper.Utils.GetTableSingleVal("fr_financial_entities", "NAME", nParentID).ToString();
                string sTitle = sParentName + " - " + selectQuery.Table("query").DefaultView[i].Row["NAME"].ToString();
                string sDescription = selectQuery.Table("query").DefaultView[i].Row["DESCRIPTION"].ToString();
                if (IsContractFamilyBelongToMediaFile(Session["subscription_id"].ToString(), int.Parse(sID)) == true)
                    sRet += "<item id=\"" + sID + "\"  title=\"" + sTitle + "\" description=\"" + sDescription + "\" inList=\"true\" />";
                else
                    sRet += "<item id=\"" + sID + "\"  title=\"" + sTitle + "\" description=\"" + sDescription + "\" inList=\"false\" />";
            }
            sRet += "</root>";
        }
        selectQuery.Finish();
        selectQuery = null;

        return sRet;
    }
}
