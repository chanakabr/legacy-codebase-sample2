using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using TVinciShared;
using KLogMonitor;
using System.Reflection;
using System.Data;
using DAL;

public partial class adm_subscription_file_types : System.Web.UI.Page
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
        Response.Write(sRet + " Subscription (" + Session["subscription_id"].ToString() + ") File Types");
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

    protected void InsertSubscriptionFileTypeID(Int32 nFileTypeID, Int32 nSubscriptionID, Int32 nGroupID)
    {
        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("subscriptions_file_types");
        insertQuery.SetConnectionKey("pricing_connection");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_id", "=", nSubscriptionID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FILE_TYPE_ID", "=", nFileTypeID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
        insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;
    }

    protected void UpdateSubscriptionFileTypeID(Int32 nID, Int32 nStatus)
    {
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_file_types");
        updateQuery.SetConnectionKey("pricing_connection");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", nStatus);
        updateQuery += "where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nID);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;
    }

    protected Int32 GetSubscriptionFileTypeID(Int32 nFileTypeID, Int32 nLogedInGroupID, ref Int32 nStatus)
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("pricing_connection");
        selectQuery += "select id,status from subscriptions_file_types where is_active=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_id", "=", int.Parse(Session["subscription_id"].ToString()));
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("FILE_TYPE_ID", "=", nFileTypeID);
        selectQuery += "and";
        Int32 nCommerceGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", nLogedInGroupID).ToString());
        if (nCommerceGroupID == 0)
            nCommerceGroupID = LoginManager.GetLoginGroupID();
        selectQuery += " group_id " + PageUtils.GetFullChildGroupsStr(nCommerceGroupID, "");
        //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nLogedInGroupID);
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
        Int32 nSubscriptionFileTypeID = GetSubscriptionFileTypeID(int.Parse(sID), nLogedInGroupID, ref nStatus);
        if (nSubscriptionFileTypeID != 0)
        {
            if (nStatus == 0)
                UpdateSubscriptionFileTypeID(nSubscriptionFileTypeID, 1);
            else
                UpdateSubscriptionFileTypeID(nSubscriptionFileTypeID, 0);
        }
        else
        {
            InsertSubscriptionFileTypeID(int.Parse(sID), int.Parse(Session["subscription_id"].ToString()), nLogedInGroupID);
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
            log.Error("exception - " + Session["subscription_id"].ToString() + " : " + ex.Message);
        }

        return "";
    }

    public string initDualObj()
    {
        long subscriptionId = 0;
        if (Session["subscription_id"] == null || Session["subscription_id"].ToString() == "" || !long.TryParse(Session["subscription_id"].ToString(), out subscriptionId) || subscriptionId <= 0)
        {
            LoginManager.LogoutFromSite("index.html");
            return "";
        }

        Dictionary<string, object> dualList = new Dictionary<string, object>();
        dualList.Add("FirstListTitle", "Current File Types");
        dualList.Add("SecondListTitle", "Available File Types");

        object[] resultData = null;
        List<object> fileTypes = new List<object>();
        Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
        DataSet ds = TvmDAL.GetSubscriptionPossibleChannels(nLogedInGroupID, subscriptionId);
        if (ds != null && ds.Tables != null && ds.Tables.Count == 2)
        {
            DataTable availableFileTypes = ds.Tables[0];
            DataTable fileTypesIncludedInSubscription = ds.Tables[1];
            HashSet<long> subscriptionFileTypesMap = new HashSet<long>();
            if (fileTypesIncludedInSubscription != null && fileTypesIncludedInSubscription.Rows != null)
            {
                foreach (DataRow dr in fileTypesIncludedInSubscription.Rows)
                {
                    long fileTypeId = ODBCWrapper.Utils.GetLongSafeVal(dr, "FILE_TYPE_ID", 0);
                    if (fileTypeId > 0 && !subscriptionFileTypesMap.Contains(fileTypeId))
                    {
                        subscriptionFileTypesMap.Add(fileTypeId);
                    }
                }
            }
            if (availableFileTypes != null && availableFileTypes.Rows != null)
            {
                foreach (DataRow dr in availableFileTypes.Rows)
                {
                    long fileTypeId = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID", 0);
                    string title = ODBCWrapper.Utils.GetSafeStr(dr, "DESCRIPTION");
                    string groupName = ODBCWrapper.Utils.GetSafeStr(dr, "GROUP_NAME");

                    title += "(" + groupName + ")";
                    var data = new
                    {
                        ID = fileTypeId.ToString(),
                        Title = title,
                        Description = title,
                        InList = subscriptionFileTypesMap.Contains(fileTypeId)
                    };
                    fileTypes.Add(data);
                }
            }
        }

        resultData = new object[fileTypes.Count];
        resultData = fileTypes.ToArray();

        dualList.Add("Data", resultData);
        dualList.Add("pageName", "adm_subscription_file_types.aspx");
        dualList.Add("withCalendar", false);

        return dualList.ToJSON();
    }

    protected bool IsChannelBelong(Int32 nFileTypeID)
    {
        try
        {
            bool bRet = false;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("pricing_connection");
            selectQuery += "select id from subscriptions_file_types where is_active=1 and status=1 and ";
            Int32 nCommerceGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", LoginManager.GetLoginGroupID()).ToString());
            if (nCommerceGroupID == 0)
                nCommerceGroupID = LoginManager.GetLoginGroupID();
            selectQuery += " group_id " + PageUtils.GetFullChildGroupsStr(nCommerceGroupID, "");
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", LoginManager.GetLoginGroupID());
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_ID", "=", int.Parse(Session["subscription_id"].ToString()));
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("FILE_TYPE_ID", "=", nFileTypeID);
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
