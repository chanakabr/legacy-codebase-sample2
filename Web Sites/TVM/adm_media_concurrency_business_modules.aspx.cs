using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TvinciImporter;
using TVinciShared;

public partial class adm_media_concurrency_business_modules : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;

    static protected string GetWSURL()
    {
        return TVinciShared.WS_Utils.GetTcmConfigValue("pricing_ws");
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_media_concurrency_rule.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_media_concurrency_rule.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID, "adm_media_concurrency_rule.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
            if (Request.QueryString["rule_id"] != null && Request.QueryString["rule_id"].ToString() != "")
            {
                Session["rule_id"] = int.Parse(Request.QueryString["rule_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("media_concurrency_rules", "group_id", int.Parse(Session["rule_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else if (Session["rule_id"] == null || Session["rule_id"].ToString() == "" || Session["rule_id"].ToString() == "0")
            {
                LoginManager.LogoutFromSite("index.html");
                return;
            }
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Device Managment:  Device Limitations Moudle: Media Concurrency Rules");
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

    protected void InsertBMID(Int32 nBMID, Int32 nRuleID, int type, Int32 nGroupID)
    {
        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("media_concurrency_bm");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("RULE_ID", "=", nRuleID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("type", "=", type);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BM_ID", "=", nBMID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", LoginManager.GetLoginID());
        Int32 nCommerceGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", nGroupID).ToString());
        if (nCommerceGroupID == 0)
            nCommerceGroupID = nGroupID;
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nCommerceGroupID);
        insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;
    }

    protected void UpdateBm(Int32 nID, Int32 nStatus)
    {
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media_concurrency_bm");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", nStatus);
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", LoginManager.GetLoginID());
        updateQuery += "where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nID);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;
    }

    protected Int32 GetRulesBMIds(Int32 bmID, Int32 nLogedInGroupID,int nType, ref Int32 nStatus)
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select id,status from media_concurrency_bm where is_active=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("RULE_ID", "=", int.Parse(Session["rule_id"].ToString()));
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("type", "=", nType);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("BM_ID", "=", bmID);
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

    //PPV
    public string changeItemStatus(string sID, string sAction)
    {
        if (Session["rule_id"] == null || Session["rule_id"].ToString() == "" || Session["rule_id"].ToString() == "0")
        {
            LoginManager.LogoutFromSite("index.html");
            return "";
        }

        Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("media_concurrency_rules", "group_id", int.Parse(Session["rule_id"].ToString())).ToString());
        Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
        if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
        {
            LoginManager.LogoutFromSite("login.html");
            return "";
        }
        Int32 nStatus = 0;
        Int32 nBMId = GetRulesBMIds(int.Parse(sID), nLogedInGroupID, 1, ref nStatus); 
        if (nBMId != 0)
        {
            if (nStatus == 0)
                UpdateBm(nBMId, 1);
            else
                UpdateBm(nBMId, 0);
        }
        else
        {
            InsertBMID(int.Parse(sID), int.Parse(Session["rule_id"].ToString()), 1, nLogedInGroupID);
        }

        return "";
    }
    
    //Subscription
    public string changeItemStatusSubscription(string sID, string sAction)
    {
        if (Session["rule_id"] == null || Session["rule_id"].ToString() == "" || Session["rule_id"].ToString() == "0")
        {
            LoginManager.LogoutFromSite("index.html");
            return "";
        }

        Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("media_concurrency_rules", "group_id", int.Parse(Session["rule_id"].ToString())).ToString());
        Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
        if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
        {
            LoginManager.LogoutFromSite("login.html");
            return "";
        }
        Int32 nStatus = 0;
        Int32 nBMId = GetRulesBMIds(int.Parse(sID), nLogedInGroupID, 2, ref nStatus); 
        if (nBMId != 0)
        {
            if (nStatus == 0)
                UpdateBm(nBMId, 1);
            else
                UpdateBm(nBMId, 0);
        }
        else
        {
            InsertBMID(int.Parse(sID), int.Parse(Session["rule_id"].ToString()), 2, nLogedInGroupID);
        }

        return "";
    }

    protected bool IsModuleBelongToRule(Int32 nRuleID, Int32 bmID, int nType)
    {
        bool bRet = false;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select RULE_ID from media_concurrency_bm where status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("BM_ID", "=", bmID);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("RULE_ID", "=", nRuleID);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("type", "=", nType);

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

    public void GetRuleID()
    {
        Response.Write(Session["rule_id"].ToString());
    }
    
    //PPV
    public string initDualObj()
    {
        if (Session["rule_id"] == null || Session["rule_id"].ToString() == "" || Session["rule_id"].ToString() == "0")
        {
            LoginManager.LogoutFromSite("index.html");
            return "";
        }

        Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("media_concurrency_rules", "group_id", int.Parse(Session["rule_id"].ToString())).ToString());
        Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
        if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
        {
            LoginManager.LogoutFromSite("login.html");
            return "";
        }

        string sRet = "";
        sRet += "Current PPV Module";
        sRet += "~~|~~";
        sRet += "Available PPV Module";
        sRet += "~~|~~";

        string sIP = "1.1.1.1";
        Int32 nPPVModuleGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", nLogedInGroupID).ToString());
        if (nPPVModuleGroupID == 0)
            nPPVModuleGroupID = nLogedInGroupID;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("pricing_connection");
        selectQuery += "select ID, NAME from ppv_modules where status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nPPVModuleGroupID);
        selectQuery += "order by name";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            sRet += "<root>";
            for (int i = 0; i < nCount; i++)
            {
                string sID = selectQuery.Table("query").DefaultView[i].Row["ID"].ToString();                               
                string sTitle = selectQuery.Table("query").DefaultView[i].Row["NAME"].ToString();
                if (IsModuleBelongToRule(int.Parse(Session["rule_id"].ToString()), int.Parse(sID), 1) == true)
                    sRet += "<item id=\"" + sID + "\"  title=\"" + sTitle +  "\" inList=\"true\" />";
                else
                    sRet += "<item id=\"" + sID + "\"  title=\"" + sTitle + "\" inList=\"false\" />";
            }
            sRet += "</root>";
        }
        selectQuery.Finish();
        selectQuery = null;

        return sRet;
    }

    //Subscription
    public string initDualObjSubscription()
    {
        if (Session["rule_id"] == null || Session["rule_id"].ToString() == "" || Session["rule_id"].ToString() == "0")
        {
            LoginManager.LogoutFromSite("index.html");
            return "";
        }

        Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("media_concurrency_rules", "group_id", int.Parse(Session["rule_id"].ToString())).ToString());
        Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
        if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
        {
            LoginManager.LogoutFromSite("login.html");
            return "";
        }

        string sRet = "";
        sRet += "Current Subscription Module";
        sRet += "~~|~~";
        sRet += "Available Subscription Module";
        sRet += "~~|~~";

        string sIP = "1.1.1.1";
        Int32 nSubscriptionGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", nLogedInGroupID).ToString());
        if (nSubscriptionGroupID == 0)
            nSubscriptionGroupID = nLogedInGroupID;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("pricing_connection");
        selectQuery += "select ID, NAME from subscriptions where status=1 and  is_active = 1 and START_DATE <= getdate()  and END_DATE >= GETDATE() and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nSubscriptionGroupID);
        selectQuery += "order by name";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            sRet += "<root>";
            for (int i = 0; i < nCount; i++)
            {
                string sID = selectQuery.Table("query").DefaultView[i].Row["ID"].ToString();
                string sTitle = selectQuery.Table("query").DefaultView[i].Row["NAME"].ToString();
                if (IsModuleBelongToRule(int.Parse(Session["rule_id"].ToString()), int.Parse(sID), 1) == true)
                    sRet += "<item id=\"" + sID + "\"  title=\"" + sTitle + "\" inList=\"true\" />";
                else
                    sRet += "<item id=\"" + sID + "\"  title=\"" + sTitle + "\" inList=\"false\" />";
            }
            sRet += "</root>";
        }
        selectQuery.Finish();
        selectQuery = null;

        return sRet;
    }

  


}