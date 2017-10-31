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

public partial class adm_collection_channels : System.Web.UI.Page
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
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID, "adm_collections.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
            if (Request.QueryString["collection_id"] != null &&
                Request.QueryString["collection_id"].ToString() != "")
            {
                Session["collection_id"] = int.Parse(Request.QueryString["collection_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("collections", "group_id", int.Parse(Session["collection_id"].ToString()), "pricing_connection").ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else if (Session["collection_id"] == null || Session["collection_id"].ToString() == "" || Session["collection_id"].ToString() == "0")
            {
                LoginManager.LogoutFromSite("index.html");
                return;
            }
        }
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ":";
        Response.Write(sRet + " Collections (" + Session["collection_id"].ToString() + ") Channels");
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

    protected void InsertCollectionChannelID(Int32 nChannelID, Int32 nCollectionID, Int32 nGroupID)
    {
        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("collections_channels");
        insertQuery.SetConnectionKey("pricing_connection");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COLLECTION_ID", "=", nCollectionID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CHANNEL_ID", "=", nChannelID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 777);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
        insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;
    }

    protected void UpdateCollectionChannelID(Int32 nID, Int32 nStatus, int nGroupID, int nChannelID, int nCollectionID)
    {
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("collections_channels");
        updateQuery.SetConnectionKey("pricing_connection");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", nStatus);
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 777);
        updateQuery += "where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nID);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;
    }

    protected Int32 GetCollectionChannelID(Int32 nChannelID, Int32 nLogedInGroupID, ref Int32 nStatus)
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("pricing_connection");
        selectQuery += "select id,status from collections_channels where is_active=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COLLECTION_ID", "=", int.Parse(Session["collection_id"].ToString()));
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
        if (Session["collection_id"] == null || Session["collection_id"].ToString() == "" || Session["collection_id"].ToString() == "0")
        {
            LoginManager.LogoutFromSite("index.html");
            return "";
        }

        Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("collections", "group_id", int.Parse(Session["collection_id"].ToString()), "pricing_connection").ToString());
        Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
        if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
        {
            LoginManager.LogoutFromSite("login.html");
            return "";
        }
        Int32 nStatus = 0;
        int nChannelID = Int32.Parse(sID);
        int nCollectionID = Int32.Parse(Session["collection_id"].ToString());
        Int32 nCollectionChannelID = GetCollectionChannelID(int.Parse(sID), nLogedInGroupID, ref nStatus);
        if (nCollectionChannelID != 0)
        {
            if (nStatus == 0)
                UpdateCollectionChannelID(nCollectionChannelID, 1, nLogedInGroupID, nChannelID, nCollectionID);
            else
                UpdateCollectionChannelID(nCollectionChannelID, 0, nLogedInGroupID, nChannelID, nCollectionID);
        }
        else
        {
            InsertCollectionChannelID(int.Parse(sID), int.Parse(Session["collection_id"].ToString()), nLogedInGroupID);
        }

        return "";
    }

    //public string initDualObj()
    //{
    //    if (Session["collection_id"] == null || Session["collection_id"].ToString() == "" || Session["collection_id"].ToString() == "0")
    //    {
    //        LoginManager.LogoutFromSite("index.html");
    //        return "";
    //    }

    //    Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("collections", "group_id", int.Parse(Session["collection_id"].ToString()), "pricing_connection").ToString());
    //    Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
    //    if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
    //    {
    //        LoginManager.LogoutFromSite("login.html");
    //        return "";
    //    }

    //    string sRet = "";
    //    sRet += "Channels included in collection";
    //    sRet += "~~|~~";
    //    sRet += "Available Channels";
    //    sRet += "~~|~~";
    //    sRet += "<root>";
    //    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
    //    selectQuery += "select * from channels where is_active=1 and status=1 and channel_type<>3 and watcher_id=0 and ";
    //    Int32 nCommerceGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", LoginManager.GetLoginGroupID()).ToString());
    //    if (nCommerceGroupID == 0)
    //        nCommerceGroupID = nLogedInGroupID;
    //    selectQuery += "group_id " + PageUtils.GetFullChildGroupsStr(nCommerceGroupID, "");
    //    log.Debug("Collections group ids - " + PageUtils.GetFullChildGroupsStr(nCommerceGroupID, ""));
    //    if (selectQuery.Execute("query", true) != null)
    //    {
    //        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
    //        for (int i = 0; i < nCount; i++)
    //        {
    //            string sID = selectQuery.Table("query").DefaultView[i].Row["ID"].ToString();
    //            string sGroupID = selectQuery.Table("query").DefaultView[i].Row["group_ID"].ToString();
    //            string sTitle = "";

    //            if (selectQuery.Table("query").DefaultView[i].Row["ADMIN_NAME"] != null &&
    //                selectQuery.Table("query").DefaultView[i].Row["ADMIN_NAME"] != DBNull.Value)
    //                sTitle = selectQuery.Table("query").DefaultView[i].Row["ADMIN_NAME"].ToString();

    //            string sGroupName = ODBCWrapper.Utils.GetTableSingleVal("groups", "GROUP_NAME", int.Parse(sGroupID)).ToString();
    //            sTitle += "(" + sGroupName.ToString() + ")";

    //            string sDescription = "";

    //            if (IsChannelBelong(int.Parse(sID)) == true)
    //                sRet += "<item id=\"" + sID + "\"  title=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(sTitle, true) + "\" description=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(sDescription, true) + "\" inList=\"true\" />";
    //            else
    //                sRet += "<item id=\"" + sID + "\"  title=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(sTitle, true) + "\" description=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(sDescription, true) + "\" inList=\"false\" />";
    //        }
    //    }
    //    selectQuery.Finish();
    //    selectQuery = null;


    //    sRet += "</root>";
    //    return sRet;
    //}

    protected bool IsChannelBelong(Int32 nChannelID)
    {
        try
        {
            bool bRet = false;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("pricing_connection");
            selectQuery += "select id from collections_channels where is_active=1 and status=1 and ";
            Int32 nCommerceGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", LoginManager.GetLoginGroupID()).ToString());
            if (nCommerceGroupID == 0)
                nCommerceGroupID = LoginManager.GetLoginGroupID();
            selectQuery += " group_id " + PageUtils.GetFullChildGroupsStr(nCommerceGroupID, "");
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COLLECTION_ID", "=", int.Parse(Session["collection_id"].ToString()));
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

    public string initDualObj()
    {
        Int32 logedInGroupID = LoginManager.GetLoginGroupID();

        Dictionary<string, object> dualList = new Dictionary<string, object>();
        dualList.Add("FirstListTitle", "Current Channels");
        dualList.Add("SecondListTitle", "Available Channels");

        object[] resultData = null;
        List<object> channels = new List<object>();

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select ID, NAME, DESCRIPTION, GROUP_ID from channels where is_active=1 and status=1 and channel_type<>3 and watcher_id=0 and ";
        Int32 nCommerceGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", LoginManager.GetLoginGroupID()).ToString());
        if (nCommerceGroupID == 0)
            nCommerceGroupID = logedInGroupID;
        selectQuery += "group_id " + PageUtils.GetFullChildGroupsStr(nCommerceGroupID, "");
        log.Debug("Collections group ids - " + PageUtils.GetFullChildGroupsStr(nCommerceGroupID, ""));
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string sID = selectQuery.Table("query").DefaultView[i].Row["ID"].ToString();
                string sGroupID = selectQuery.Table("query").DefaultView[i].Row["group_ID"].ToString();
                string sTitle = "";
                string sDescription = "";

                if (selectQuery.Table("query").DefaultView[i].Row["NAME"] != null &&
                    selectQuery.Table("query").DefaultView[i].Row["NAME"] != DBNull.Value)
                    sTitle = selectQuery.Table("query").DefaultView[i].Row["NAME"].ToString();

                if (selectQuery.Table("query").DefaultView[i].Row["DESCRIPTION"] != null &&
                    selectQuery.Table("query").DefaultView[i].Row["DESCRIPTION"] != DBNull.Value)
                    sDescription = selectQuery.Table("query").DefaultView[i].Row["DESCRIPTION"].ToString();

                string sGroupName = ODBCWrapper.Utils.GetTableSingleVal("groups", "GROUP_NAME", int.Parse(sGroupID)).ToString();
                sTitle += "(" + sGroupName.ToString() + ")";

                var data = new
                    {
                        ID = sID,
                        Title = sTitle,
                        Description = sDescription,
                        InList = IsChannelBelong(int.Parse(sID))
                    };
                    channels.Add(data);
            }
        }
        selectQuery.Finish();
        selectQuery = null;
                
        resultData = new object[channels.Count];
        resultData = channels.ToArray();

        dualList.Add("Data", resultData);
        dualList.Add("pageName", "adm_group_services.aspx");
        dualList.Add("withCalendar", false);

        return dualList.ToJSON();
    }
}
