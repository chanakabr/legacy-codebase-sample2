using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using TVinciShared;
using ApiObjects;
using KLogMonitor;
using System.Reflection;

public partial class adm_subscription_channels : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
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
        if (LoginManager.IsPagePermitted("adm_media.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID, "adm_subscriptions.aspx");
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
        Response.Write(sRet + " Subscription (" + Session["subscription_id"].ToString() + ") Channels");
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

    protected void InsertSubscriptionChannelID(Int32 nChannelID, Int32 nSubscriptionID, Int32 nGroupID)
    {
        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("subscriptions_channels");
        insertQuery.SetConnectionKey("pricing_connection");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_id", "=", nSubscriptionID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CHANNEL_ID", "=", nChannelID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
        insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;

        TvinciImporter.ImporterImpl.UpdateOperator(nGroupID, 0, nSubscriptionID, nChannelID, eOperatorEvent.ChannelAddedToSubscription);
    }

    protected void UpdateSubscriptionChannelID(Int32 nID, Int32 nStatus, int nGroupID, int nChannelID, int nSubscriptionID)
    {
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_channels");
        updateQuery.SetConnectionKey("pricing_connection");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", nStatus);
        updateQuery += "where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nID);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;

        if (nStatus > 0)
        {
            TvinciImporter.ImporterImpl.UpdateOperator(nGroupID, 0, nSubscriptionID, nChannelID, eOperatorEvent.ChannelAddedToSubscription);
        }
        else
        {
            // channel removed from subscription
            TvinciImporter.ImporterImpl.UpdateOperator(nGroupID, 0, nSubscriptionID, nChannelID, eOperatorEvent.ChannelRemovedFromSubscription);
        }
    }

    protected Int32 GetSubscriptionChannelID(Int32 nChannelID, Int32 nLogedInGroupID, ref Int32 nStatus)
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("pricing_connection");
        selectQuery += "select id,status from subscriptions_channels where is_active=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_id", "=", int.Parse(Session["subscription_id"].ToString()));
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CHANNEL_ID", "=", nChannelID);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nLogedInGroupID);
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
        if (Session["subscription_id"] == null || Session["subscription_id"].ToString() == "" || Session["subscription_id"].ToString() == "0")
        {
            LoginManager.LogoutFromSite("index.html");
            return "";
        }

        Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("subscriptions", "group_id", int.Parse(Session["subscription_id"].ToString()), "pricing_connection").ToString());
        Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
        if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
        {
            LoginManager.LogoutFromSite("login.html");
            return "";
        }
        Int32 nStatus = 0;
        int nChannelID = Int32.Parse(sID);
        int nSubscriptionID = Int32.Parse(Session["subscription_id"].ToString());
        Int32 nSubscriptionChannelID = GetSubscriptionChannelID(int.Parse(sID), nLogedInGroupID, ref nStatus);
        if (nSubscriptionChannelID != 0)
        {
            if (nStatus == 0)
                UpdateSubscriptionChannelID(nSubscriptionChannelID, 1, nLogedInGroupID, nChannelID, nSubscriptionID);
            else
                UpdateSubscriptionChannelID(nSubscriptionChannelID, 0, nLogedInGroupID, nChannelID, nSubscriptionID);
        }
        else
        {
            InsertSubscriptionChannelID(int.Parse(sID), int.Parse(Session["subscription_id"].ToString()), nLogedInGroupID);
        }

        try
        {
            Notifiers.BaseSubscriptionNotifier t = null;
            Notifiers.Utils.GetBaseSubscriptionsNotifierImpl(ref t, LoginManager.GetLoginGroupID(), "pricing_connection");
            if (t != null)
                t.NotifyChange(Session["subscription_id"].ToString());
            return "";
        }
        catch (Exception ex)
        {
            log.Error("exception - " + Session["subscription_id"].ToString() + " : " + ex.Message, ex);
        }

        return "";
    }

    public string initDualObj()
    {
        if (Session["subscription_id"] == null || Session["subscription_id"].ToString() == "" || Session["subscription_id"].ToString() == "0")
        {
            LoginManager.LogoutFromSite("index.html");
            return "";
        }

        Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("subscriptions", "group_id", int.Parse(Session["subscription_id"].ToString()), "pricing_connection").ToString());
        Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
        if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
        {
            LoginManager.LogoutFromSite("login.html");
            return "";
        }

        Dictionary<string, object> dualList = new Dictionary<string, object>();
        dualList.Add("FirstListTitle", "Channels included in subscription");
        dualList.Add("SecondListTitle", "Available Channels");

        object[] resultData = null;
        List<object> subscriptionChannels = new List<object>();

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from channels where is_active=1 and status=1 and channel_type<>3 and watcher_id=0 and ";
        Int32 nCommerceGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", LoginManager.GetLoginGroupID()).ToString());
        if (nCommerceGroupID == 0)
            nCommerceGroupID = nLogedInGroupID;
        selectQuery += "group_id " + PageUtils.GetFullChildGroupsStr(nCommerceGroupID, "");
        if (selectQuery.Execute("query", true) != null)
        {
            var defaultView = selectQuery.Table("query").DefaultView;

            Int32 nCount = defaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                var currentRow = defaultView[i].Row;

                string sID = currentRow["ID"].ToString();
                string sGroupID = currentRow["group_ID"].ToString();
                string sTitle = "";

                object adminNameValue = currentRow["ADMIN_NAME"];
                if (adminNameValue != null &&
                    adminNameValue != DBNull.Value)
                {
                    sTitle = adminNameValue.ToString();
                }

                if (string.IsNullOrEmpty(sTitle))
                {
                    object nameValue = currentRow["NAME"];

                    if (nameValue != null && nameValue != DBNull.Value)
                    {
                        sTitle = string.Format("{0} - {1}", sID, nameValue);
                    }
                }

                string sGroupName = ODBCWrapper.Utils.GetTableSingleVal("groups", "GROUP_NAME", int.Parse(sGroupID)).ToString();
                sTitle += "(" + sGroupName.ToString() + ")";

                bool isInList = false;
                if (IsChannelBelong(int.Parse(sID)) == true)
                {
                    isInList = true;
                }

                var data = new
                {
                    ID = sID,
                    Title = sTitle,
                    Description = sTitle,
                    InList = isInList
                };
                subscriptionChannels.Add(data);
            }
        }
        selectQuery.Finish();
        selectQuery = null;

        resultData = new object[subscriptionChannels.Count];
        resultData = subscriptionChannels.ToArray();

        dualList.Add("Data", resultData);
        dualList.Add("pageName", "adm_subscription_channels.aspx");
        dualList.Add("withCalendar", false);

        return dualList.ToJSON();
    }

    protected bool IsChannelBelong(Int32 nChannelID)
    {
        try
        {
            bool bRet = false;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("pricing_connection");
            selectQuery += "select id from subscriptions_channels where is_active=1 and status=1 and ";
            Int32 nCommerceGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", LoginManager.GetLoginGroupID()).ToString());
            if (nCommerceGroupID == 0)
                nCommerceGroupID = LoginManager.GetLoginGroupID();
            selectQuery += " group_id " + PageUtils.GetFullChildGroupsStr(nCommerceGroupID, "");
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_ID", "=", int.Parse(Session["subscription_id"].ToString()));
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CHANNEL_ID", "=", nChannelID);
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
        catch
        {
            return false;
        }
    }
}
