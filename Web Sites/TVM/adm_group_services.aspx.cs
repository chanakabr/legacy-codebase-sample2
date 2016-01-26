using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Tvinci.Core.DAL;
using TVinciShared;

public partial class adm_group_services : System.Web.UI.Page
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
        if (LoginManager.IsPagePermitted() == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(2, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 11, false);
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

    protected void InsertGroupService(Int32 nServiceID, Int32 nGroupID)
    {
        bool bInsert = false;
        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("groups_services");
        insertQuery.SetConnectionKey("CONNECTION_STRING");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SERVICE_ID", "=", nServiceID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", LoginManager.GetLoginID());
        bInsert = insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;

        if (bInsert)
        {         
            GroupsCacheManager.GroupManager groupManager = new GroupsCacheManager.GroupManager();
            groupManager.AddServices(nGroupID, new List<int>() { nServiceID });
        }

    }

    private List<int> GetGroupServiceByID(int nServiceID, int nGroupID)
    {
        return CatalogDAL.GetGroupServices(nGroupID, nServiceID);
    }

    protected void UpdateGroupsServices(Int32 nID, Int32 nStatus, int nServiceID, int nGroupID)
    {
        bool bUpdate = false;
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("groups_services");        
        updateQuery.SetConnectionKey("CONNECTION_STRING");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", nStatus);
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", LoginManager.GetLoginID());
        updateQuery += "where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nID);
        bUpdate = updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;

        if (bUpdate)
        {
            GroupsCacheManager.GroupManager groupManager = new GroupsCacheManager.GroupManager();            
            if (nStatus == 0) // unactive 
            {
                groupManager.DeleteServices(nGroupID, new List<int>() { nServiceID });
            }
            else
            {                
                groupManager.AddServices(nGroupID, new List<int>() { nServiceID });
            }
        }
    }

    protected Int32 GetGroupServiceID(Int32 nServiceID, Int32 nLogedInGroupID, ref Int32 nStatus)
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("CONNECTION_STRING");
        selectQuery += "select id, status, is_active from groups_services where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("service_id", "=", nServiceID);
        selectQuery += "and";
        Int32 nCommerceGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", nLogedInGroupID).ToString());
        if (nCommerceGroupID == 0)
            nCommerceGroupID = LoginManager.GetLoginGroupID();
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
        Int32 nStatus = 0;
        int nServiceID = int.Parse(sID);
        Int32 nGroupServiceID = GetGroupServiceID(nServiceID, nLogedInGroupID, ref nStatus);
        
        if (nGroupServiceID != 0)
        {
            if (nStatus == 0)
                UpdateGroupsServices(nGroupServiceID, 1, nServiceID, nLogedInGroupID);
            else
                UpdateGroupsServices(nGroupServiceID, 0, nServiceID, nLogedInGroupID);
        }
        else
        {
            InsertGroupService(int.Parse(sID), nLogedInGroupID);
        }

        return "";
    }

    public string initDualObj()
    {
        Dictionary<string, object> dualList = new Dictionary<string, object>();
        dualList.Add("FirstListTitle", "Current Services");
        dualList.Add("SecondListTitle", "Available Services");

        object[] resultData = null;
        List<object> premiumServices = new List<object>();

        ODBCWrapper.DataSetSelectQuery servicesSelectQuery = new ODBCWrapper.DataSetSelectQuery();
        servicesSelectQuery.SetConnectionKey("CONNECTION_STRING");
        servicesSelectQuery += "select ID, DESCRIPTION from lu_services where status=1 ";
        
        if (servicesSelectQuery.Execute("query", true) != null)
        {
            ODBCWrapper.DataSetSelectQuery groupServicesSelectQuery = new ODBCWrapper.DataSetSelectQuery();
            groupServicesSelectQuery.SetConnectionKey("CONNECTION_STRING");
            groupServicesSelectQuery += "select ID, SERVICE_ID from groups_services where status = 1 and is_active = 1 and ";
            groupServicesSelectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", LoginManager.GetLoginGroupID());

            if (groupServicesSelectQuery.Execute("query", true) != null)
            {

                Int32 nCount = servicesSelectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sID = ODBCWrapper.Utils.GetStrSafeVal(servicesSelectQuery, "ID", i);
                    string sTitle = ODBCWrapper.Utils.GetStrSafeVal(servicesSelectQuery, "DESCRIPTION", i);
                    DataRow drService = groupServicesSelectQuery.Table("query").Select(string.Format("SERVICE_ID = {0}", sID)).FirstOrDefault();
                    bool isInList = false;
                    if (drService != null)
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
                    premiumServices.Add(data);
                }
            }
            groupServicesSelectQuery.Finish();
            groupServicesSelectQuery = null;
        }
        servicesSelectQuery.Finish();
        servicesSelectQuery = null;

        resultData = new object[premiumServices.Count];
        resultData = premiumServices.ToArray();

        dualList.Add("Data", resultData);
        dualList.Add("pageName", "adm_group_services.aspx");
        dualList.Add("withCalendar", false);

        return dualList.ToJSON();
    }
}



