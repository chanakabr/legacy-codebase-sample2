using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_limitation_media_concurrency_rules : System.Web.UI.Page
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
        if (LoginManager.IsPagePermitted("adm_domain_limitation_modules.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_domain_limitation_modules.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID, "adm_domain_limitation_modules.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
            if (Request.QueryString["limit_module_id"] != null && Request.QueryString["limit_module_id"].ToString() != "")
            {
                Session["limit_module_id"] = int.Parse(Request.QueryString["limit_module_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("groups_device_limitation_modules", "group_id", int.Parse(Session["limit_module_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else if (Session["limit_module_id"] == null || Session["limit_module_id"].ToString() == "" || Session["limit_module_id"].ToString() == "0")
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

    protected void InsertDeviceMediaConcurrencyRules(Int32 nMCRuleID, Int32 ndeviceLimitIDID, Int32 nGroupID)
    {
        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("groups_device_media_concurrency_rules");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_CONCURRENCY_RULE_ID", "=", nMCRuleID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_LIMITATION_ID", "=", ndeviceLimitIDID);
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

    protected void UpdateDeviceMediaConcurrencyRules(Int32 nID, Int32 nStatus)
    {
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("groups_device_media_concurrency_rules");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", nStatus);
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", LoginManager.GetLoginID());
        updateQuery += "where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nID);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;
    }

    protected Int32 GetId(Int32 nMCRuleID, Int32 nLogedInGroupID, ref Int32 nStatus)
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select id,status from groups_device_media_concurrency_rules where is_active=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_LIMITATION_ID", "=", int.Parse(Session["limit_module_id"].ToString()));        
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_CONCURRENCY_RULE_ID", "=", nMCRuleID);
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
        if (Session["limit_module_id"] == null || Session["limit_module_id"].ToString() == "" || Session["limit_module_id"].ToString() == "0")
        {
            LoginManager.LogoutFromSite("index.html");
            return "";
        }

        Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("groups_device_limitation_modules", "group_id", int.Parse(Session["limit_module_id"].ToString())).ToString());
        Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
        if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
        {
            LoginManager.LogoutFromSite("login.html");
            return "";
        }
        Int32 nStatus = 0;
        Int32 nId = GetId(int.Parse(sID), nLogedInGroupID,  ref nStatus);
        
        if (nId != 0)
        {
            if (nStatus == 0)
                UpdateDeviceMediaConcurrencyRules(nId, 1);
            else
                UpdateDeviceMediaConcurrencyRules(nId, 0);
        }
        else
        {
            InsertDeviceMediaConcurrencyRules(int.Parse(sID), int.Parse(Session["limit_module_id"].ToString()), nLogedInGroupID);
                                                    
        }

        return "";
    }

    protected bool IsBelongToRule(Int32 dlID, Int32 mcID, int nType)
    {
        bool bRet = false;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select MEDIA_CONCURRENCY_RULE_ID from groups_device_media_concurrency_rules where status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_CONCURRENCY_RULE_ID", "=", mcID);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_LIMITATION_ID", "=", dlID);
       
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

    public void GetGDID()
    {
        Response.Write(Session["limit_module_id"].ToString());
    }

    public string initDualObj()
    {
        if (Session["limit_module_id"] == null || Session["limit_module_id"].ToString() == "" || Session["limit_module_id"].ToString() == "0")
        {
            LoginManager.LogoutFromSite("index.html");
            return "";
        }

        Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("groups_device_limitation_modules", "group_id", int.Parse(Session["limit_module_id"].ToString())).ToString());
        Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
        if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
        {
            LoginManager.LogoutFromSite("login.html");
            return "";
        }

        Int32 nMCGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", nLogedInGroupID).ToString());
        if (nMCGroupID == 0)
            nMCGroupID = nLogedInGroupID;

        Dictionary<string, object> dualList = new Dictionary<string, object>();
        dualList.Add("FirstListTitle", "Device Families");
        dualList.Add("SecondListTitle", "Available Device Families");

        object[] resultData = null;
        List<object> concurrencyRules = new List<object>();

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select ID, NAME from media_concurrency_rules where status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nMCGroupID);
        selectQuery += "order by name";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;            
            for (int i = 0; i < nCount; i++)
            {
                string sID = selectQuery.Table("query").DefaultView[i].Row["ID"].ToString();
                string sTitle = selectQuery.Table("query").DefaultView[i].Row["NAME"].ToString();
                if (IsBelongToRule(int.Parse(Session["limit_module_id"].ToString()), int.Parse(sID), 1) == true)
                {
                    var data = new
                    {
                        ID = sID,
                        Title = sTitle,
                        Description = sTitle,
                        InList = true
                    };
                    concurrencyRules.Add(data);
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
                    concurrencyRules.Add(data);
                }

            }
        }
        selectQuery.Finish();
        selectQuery = null;

        resultData = new object[concurrencyRules.Count];
        resultData = concurrencyRules.ToArray();

        dualList.Add("Data", resultData);
        dualList.Add("pageName", "adm_limitation_media_concurrency_rules.aspx");
        dualList.Add("withCalendar", false);

        return dualList.ToJSON();
    }

}