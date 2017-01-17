using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_subscription_services : System.Web.UI.Page
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
        if (LoginManager.IsPagePermitted("adm_multi_pricing_plans.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID, "adm_multi_pricing_plans.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
            if (Request.QueryString["subscription_id"] != null &&
                Request.QueryString["subscription_id"].ToString() != "")
            {
                Session["subscription_id"] = int.Parse(Request.QueryString["subscription_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("subscriptions", "group_id", int.Parse(Session["subscription_id"].ToString()), "pricing_connection").ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else if (Session["subscription_id"] == null || Session["subscription_id"].ToString() == "" || Session["subscription_id"].ToString() == "0")
            {
                LoginManager.LogoutFromSite("index.html");
                return;
            }
        }
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ":";
        Response.Write(sRet + " Premium Services");
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

    protected void InsertSubscriptionsServices(Int32 nServiceID, Int32 nGroupID, Int32 nSubscriptionID, long? quota = null)
    {
        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("subscriptions_services");
        insertQuery.SetConnectionKey("pricing_connection");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_ID", "=", nSubscriptionID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SERVICE_ID", "=", nServiceID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
        if (quota.HasValue)
        {
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("QUOTA_IN_MINUTES", "=", quota.Value);
        }
        insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;
    }

    protected void UpdateSubscriptionsServices(Int32 nID, Int32 nStatus, bool isNPVR)
    {
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_services");
        updateQuery.SetConnectionKey("pricing_connection");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", nStatus);
        if (isNPVR)
        {
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("QUOTA_IN_MINUTES", "=", 0);
        }

        updateQuery += "where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nID);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;
    }

   

    protected void UpdateSubscriptionsServicesQuota(Int32 nID, long quota)
    {
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_services");
        updateQuery.SetConnectionKey("pricing_connection");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("QUOTA_IN_MINUTES", "=", quota);
        updateQuery += "where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nID);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;
    }

    protected Int32 GetSubscriptionServiceID(Int32 nServiceID, Int32 nLogedInGroupID, int nSubscriptionID, ref Int32 nStatus)
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("pricing_connection");
        selectQuery += "select id, status from subscriptions_services where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("service_id", "=", nServiceID);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_id", "=", nSubscriptionID);
        selectQuery += " and ";
        Int32 nCommerceGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", nLogedInGroupID).ToString());
        if (nCommerceGroupID == 0)
            nCommerceGroupID = LoginManager.GetLoginGroupID();
        selectQuery += " group_id " + PageUtils.GetFullChildGroupsStr(nCommerceGroupID, "");
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
        Int32 nStatus = 0;
        Int32 nSubscriptionID = int.Parse(Session["subscription_id"].ToString());
        Int32 nGroupServiceID = GetSubscriptionServiceID(int.Parse(sID), nLogedInGroupID, nSubscriptionID, ref nStatus);
        
        if (nGroupServiceID != 0)
        {
            if (nStatus == 0)
                UpdateSubscriptionsServices(nGroupServiceID, 1, sID == "3");
            else
                UpdateSubscriptionsServices(nGroupServiceID, 0, sID == "3");
        }
        else
        {
            InsertSubscriptionsServices(int.Parse(sID), nLogedInGroupID, nSubscriptionID);
        }

        return "";
    }


    public string changeNumberField(string sID, string val)
    {
        Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
        Int32 nStatus = 0;
        Int32 nSubscriptionID = int.Parse(Session["subscription_id"].ToString());
        Int32 nGroupServiceID = GetSubscriptionServiceID(int.Parse(sID), nLogedInGroupID, nSubscriptionID, ref nStatus);
        long quota = 0 ;
        if (long.TryParse(val, out quota))
        {
            if (nGroupServiceID != 0)
            {
                UpdateSubscriptionsServicesQuota(nGroupServiceID, quota);
            }
            else
            {
                InsertSubscriptionsServices(int.Parse(sID), nLogedInGroupID, nSubscriptionID, quota);
            }
        }
        return "";
    }

    public string initDualObj()
    {
        Dictionary<string, object> dualList = new Dictionary<string, object>();
        dualList.Add("FirstListTitle", "Device Families");
        dualList.Add("SecondListTitle", "Available Device Families");

        object[] resultData = null;
        List<object> premiumServices = new List<object>();

        ODBCWrapper.DataSetSelectQuery groupServicesSelectQuery = new ODBCWrapper.DataSetSelectQuery();
        groupServicesSelectQuery.SetConnectionKey("CONNECTION_STRING");
        groupServicesSelectQuery += "select gs.SERVICE_ID as SERVICE_ID, s.DESCRIPTION as DESCRIPTION from groups_services as gs join lu_services as s on s.id = gs.service_id where s.status=1 and gs.status = 1 and gs.is_active = 1 and ";
        Int32 nCommerceGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", LoginManager.GetLoginGroupID()).ToString());
        if (nCommerceGroupID == 0)
            nCommerceGroupID = LoginManager.GetLoginGroupID();
        groupServicesSelectQuery += " gs.group_id " + PageUtils.GetFullChildGroupsStr(nCommerceGroupID, "");
        if (groupServicesSelectQuery.Execute("query", true) != null)
        {
            ODBCWrapper.DataSetSelectQuery subscriptionServicesSelectQuery = new ODBCWrapper.DataSetSelectQuery();
            subscriptionServicesSelectQuery.SetConnectionKey("pricing_connection");
            subscriptionServicesSelectQuery += "select ID, SERVICE_ID, QUOTA_IN_MINUTES from subscriptions_services where status = 1 and is_active = 1 and ";
            subscriptionServicesSelectQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_id", "=", int.Parse(Session["subscription_id"].ToString()));
            subscriptionServicesSelectQuery += " and ";
            if (nCommerceGroupID == 0)
                nCommerceGroupID = LoginManager.GetLoginGroupID();
            subscriptionServicesSelectQuery += " group_id " + PageUtils.GetFullChildGroupsStr(nCommerceGroupID, "");
            if (subscriptionServicesSelectQuery.Execute("query", true) != null)
            {

                Int32 nCount = groupServicesSelectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sID = ODBCWrapper.Utils.GetStrSafeVal(groupServicesSelectQuery, "SERVICE_ID", i);
                    string sTitle = ODBCWrapper.Utils.GetStrSafeVal(groupServicesSelectQuery, "DESCRIPTION", i);

                   // string Quota = ODBCWrapper.Utils.GetStrSafeVal(subscriptionServicesSelectQuery, "QUOTA_IN_MINUTES", i);

                    DataRow drService = subscriptionServicesSelectQuery.Table("query").Select(string.Format("SERVICE_ID = {0}", sID)).FirstOrDefault();
                    if (drService != null)
                    {
                        var data = new
                        {
                            ID = sID,
                            Title = sTitle,
                            Description = sTitle,
                            InList = true,
                            NumberField = sID == "3" ? ODBCWrapper.Utils.GetLongSafeVal(drService, "QUOTA_IN_MINUTES", 0) : -1
                        };
                        premiumServices.Add(data);
                    }
                    else
                    {
                        var data = new
                        {
                            ID = sID,
                            Title = sTitle,
                            Description = sTitle,
                            InList = false,
                            NumberField = sID == "3" ? 0 : -1
                        };
                        premiumServices.Add(data);
                    }
                }
            }
            subscriptionServicesSelectQuery.Finish();
            subscriptionServicesSelectQuery = null;
        }
        groupServicesSelectQuery.Finish();
        groupServicesSelectQuery = null;

        resultData = new object[premiumServices.Count];
        resultData = premiumServices.ToArray();

        dualList.Add("Data", resultData);
        dualList.Add("pageName", "adm_subscription_services.aspx");
        dualList.Add("withQuota", true);

        return dualList.ToJSON();
    }

}