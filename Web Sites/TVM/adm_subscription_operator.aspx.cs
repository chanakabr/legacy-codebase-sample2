using ApiObjects;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;
using TVinciShared;

public partial class adm_subscription_operators : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;

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
        Response.Write(sRet + " Subscription (" + Session["subscription_id"].ToString() + ") Operators");
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

    protected void InsertSubscriptionOperatorID(Int32 nOperatorID, Int32 nSubscriptionID, Int32 nGroupID)
    {
        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("subscription_operators");
        insertQuery.SetConnectionKey("pricing_connection");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_ID", "=", nSubscriptionID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("OPERATOR_ID", "=", nOperatorID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 577);
        insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;

        TvinciImporter.ImporterImpl.UpdateOperator(nGroupID, nOperatorID, nSubscriptionID, 0, eOperatorEvent.SubscriptionAddedToOperator);
    }

    protected void UpdateSubscriptionOperatorID(Int32 nID, Int32 nStatus, int nGroupID, int nOperatorID, int nSubscriptionID)
    {
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscription_operators");
        updateQuery.SetConnectionKey("pricing_connection");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", nStatus);
        updateQuery += "where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nID);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;

        if (nStatus > 0)
        {
            // sub added to operator
            TvinciImporter.ImporterImpl.UpdateOperator(nGroupID, nOperatorID, nSubscriptionID, 0, eOperatorEvent.SubscriptionAddedToOperator);
        }
        else
        {
            // sub removed from operator
            TvinciImporter.ImporterImpl.UpdateOperator(nGroupID, nOperatorID, nSubscriptionID, 0, eOperatorEvent.SubscriptionRemovedFromOperator);
        }
    }

    protected Int32 GetSubscriptionOperatorID(Int32 nOperatorID, Int32 nLogedInGroupID, ref Int32 nStatus)
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("pricing_connection");
        selectQuery += "select id,status from subscription_operators with (nolock) where is_active=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_id", "=", int.Parse(Session["subscription_id"].ToString()));
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("OPERATOR_ID", "=", nOperatorID);
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
        int nOperatorID = Int32.Parse(sID);
        int nSubscriptionID = Int32.Parse(Session["subscription_id"].ToString());
        Int32 nSubscriptionOperatorID = GetSubscriptionOperatorID(int.Parse(sID), nLogedInGroupID, ref nStatus);
        if (nSubscriptionOperatorID != 0)
        {
            if (nStatus == 0)
            {
                int subID = int.Parse(Session["subscription_id"].ToString());
                string sSubGroupID = ODBCWrapper.Utils.GetTableSingleVal("groups_operators", "Sub_Group_ID", "id", "=", sID).ToString();
                string sSubName = ODBCWrapper.Utils.GetTableSingleVal("subscriptions", "NAME", "id", "=", subID, "pricing_connection").ToString();
                int fictivicID = DBManipulator.BuildFictivicMedia("Package", sSubName, subID, int.Parse(sSubGroupID));

                UpdateSubscriptionOperatorID(nSubscriptionOperatorID, 1, nLogedInGroupID, nOperatorID, nSubscriptionID);
            }
            else
                UpdateSubscriptionOperatorID(nSubscriptionOperatorID, 0, nLogedInGroupID, nOperatorID, nSubscriptionID);
        }
        else
        {
            int subID = int.Parse(Session["subscription_id"].ToString());
            string sSubGroupID = ODBCWrapper.Utils.GetTableSingleVal("groups_operators", "Sub_Group_ID", "id", "=", sID).ToString();
            string sSubName = ODBCWrapper.Utils.GetTableSingleVal("subscriptions", "NAME", "id", "=", subID, "pricing_connection").ToString();
            int fictivicID = DBManipulator.BuildFictivicMedia("Package", sSubName, subID, int.Parse(sSubGroupID));

            InsertSubscriptionOperatorID(int.Parse(sID), int.Parse(Session["subscription_id"].ToString()), nLogedInGroupID);
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
        dualList.Add("FirstListTitle", "Operators included in subscription");
        dualList.Add("SecondListTitle", "Available operators");

        object[] resultData = null;
        List<object> subscriptionOperators = new List<object>();

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from groups_operators where is_active=1 and status=1 and type=1 and ";
        Int32 nCommerceGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", LoginManager.GetLoginGroupID()).ToString());
        if (nCommerceGroupID == 0)
            nCommerceGroupID = nLogedInGroupID;
        selectQuery += "group_id " + PageUtils.GetFullChildGroupsStr(nCommerceGroupID, "");
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string sID = selectQuery.Table("query").DefaultView[i].Row["ID"].ToString();
                string sGroupID = selectQuery.Table("query").DefaultView[i].Row["group_ID"].ToString();
                string sTitle = "";
                if (selectQuery.Table("query").DefaultView[i].Row["Name"] != null &&
                    selectQuery.Table("query").DefaultView[i].Row["Name"] != DBNull.Value)
                    sTitle = selectQuery.Table("query").DefaultView[i].Row["Name"].ToString();

                string sGroupName = ODBCWrapper.Utils.GetTableSingleVal("groups", "GROUP_NAME", int.Parse(sGroupID)).ToString();
                sTitle += "(" + sGroupName.ToString() + ")";

                string sDescription = "";
                
                if (IsChannelBelong(int.Parse(sID)) == true)
                {
                    var data = new
                    {
                        ID = sID,
                        Title = sTitle,
                        Description = sDescription,
                        InList = true
                    };
                    subscriptionOperators.Add(data);
                }                    
                else
                {
                    var data = new
                    {
                        ID = sID,
                        Title = sTitle,
                        Description = sTitle,
                        InList = false
                    };
                    subscriptionOperators.Add(data);
                }
            }
        }
        selectQuery.Finish();
        selectQuery = null;

        resultData = new object[subscriptionOperators.Count];
        resultData = subscriptionOperators.ToArray();

        dualList.Add("Data", resultData);
        dualList.Add("pageName", "adm_subscription_operator.aspx");
        dualList.Add("withCalendar", false);

        return dualList.ToJSON();
    }

    protected bool IsChannelBelong(Int32 nOperatorID)
    {
        try
        {
            bool bRet = false;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("pricing_connection");
            selectQuery += "select id from subscription_operators where is_active=1 and status=1 and ";
            Int32 nCommerceGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", LoginManager.GetLoginGroupID()).ToString());
            if (nCommerceGroupID == 0)
                nCommerceGroupID = LoginManager.GetLoginGroupID();
            selectQuery += " group_id " + PageUtils.GetFullChildGroupsStr(nCommerceGroupID, "");
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_ID", "=", int.Parse(Session["subscription_id"].ToString()));
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("OPERATOR_ID", "=", nOperatorID);
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
