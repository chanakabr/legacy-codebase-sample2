using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_device_brands : System.Web.UI.Page
{
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
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID, "adm_media.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
            if (Request.QueryString["family_id"] != null &&
                Request.QueryString["family_id"].ToString() != "")
            {
                Session["family_id"] = int.Parse(Request.QueryString["family_id"].ToString());
                //Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("media_files", "group_id", int.Parse(Session["media_file_id"].ToString())).ToString());
                //Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                //if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                //{
                //    LoginManager.LogoutFromSite("login.html");
                //    return;
                //}
            }
            else if (Session["family_id"] == null || Session["family_id"].ToString() == "" || Session["family_id"].ToString() == "0")
            {
                LoginManager.LogoutFromSite("index.html");
                return;
            }
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + " : Device Brands ");
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

    protected void InsertDeviceBrandID(Int32 deviceBrandID, Int32 deviceFamilyID, Int32 nGroupID)
    {
        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("groups_device_brands");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("device_brand_id", "=", deviceBrandID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("device_family_id", "=", deviceFamilyID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
        insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;
    }

    protected void UpdateDeviceBrandID(Int32 nID, Int32 nStatus)
    {
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("groups_device_brands");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", nStatus);
        updateQuery += "where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nID);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;
    }

    protected Int32 GetDeviceBrandID(Int32 nDeviceBrand, Int32 nLogedInGroupID, ref Int32 nStatus)
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();;
        selectQuery += "select id,status from groups_device_brands where is_active=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("device_family_id", "=", int.Parse(Session["family_id"].ToString()));
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("device_brand_id", "=", nDeviceBrand);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", LoginManager.GetLoginGroupID());
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
        if (Session["family_id"] == null || Session["family_id"].ToString() == "" || Session["family_id"].ToString() == "0")
        {
            LoginManager.LogoutFromSite("index.html");
            return "";
        }

        //Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("media_files", "group_id", int.Parse(Session["media_file_id"].ToString())).ToString());
        Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
      
        Int32 nStatus = 0;
        Int32 nPPVModuleMediaFilesID = GetDeviceBrandID(int.Parse(sID), nLogedInGroupID, ref nStatus);
        if (nPPVModuleMediaFilesID != 0)
        {
            if (nStatus == 0)
                UpdateDeviceBrandID(nPPVModuleMediaFilesID, 1);
            else
                UpdateDeviceBrandID(nPPVModuleMediaFilesID, 0);
        }
        else
        {
            InsertDeviceBrandID(int.Parse(sID), int.Parse(Session["family_id"].ToString()), nLogedInGroupID);
        }

        return "";
    }

    protected bool IsReachDeviceLimit()
    {
        bool retVal = true;
        int groupID = LoginManager.GetLoginGroupID();
        int familyID = 0;
        if (Session["family_id"] == null || Session["family_id"].ToString() == "" || Session["family_id"].ToString() == "0")
        {
            familyID = int.Parse(Session["family_id"].ToString());
        }

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select max_limit from groups_device_families with (nolock) where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", groupID);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("device_family_id", "=", familyID);
        selectQuery += " and is_active = 1 and status = 1";
        if (selectQuery.Execute("query", true) != null)
        {
            int count = selectQuery.Table("query").DefaultView.Count;
            if (count > 0)
            {
                int maxLimit = int.Parse(selectQuery.Table("query").DefaultView[0].Row["max_limit"].ToString());
                List<string> addedBrands = GetFamilyGroupBrands(groupID, familyID);
                
                if (addedBrands != null)
                {
                    if (addedBrands.Count < maxLimit)
                    {
                        retVal = false;
                    }
                }
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return retVal;
    }

    protected List<string> GetFamilyGroupBrands(int groupID, int familyID)
    {
        List<string> retVal = null;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select device_brand_id from groups_device_brands with (nolock) where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", groupID);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("device_family_id", "=", familyID);
        selectQuery += "and ";
        selectQuery += "is_active = 1 and status = 1";
        if (selectQuery.Execute("query", true) != null)
        {
            int count = selectQuery.Table("query").DefaultView.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    if (retVal == null)
                    {
                        retVal = new List<string>();
                    }
                    string sID = selectQuery.Table("query").DefaultView[i].Row["device_brand_id"].ToString();
                    if (!retVal.Contains(sID))
                    {
                        retVal.Add(sID);
                    }
                }
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return retVal;
    }

    public string initDualObj()
    {
        if (Session["family_id"] == null || Session["family_id"].ToString() == "" || Session["family_id"].ToString() == "0")
        {
            LoginManager.LogoutFromSite("index.html");
            return "";
        }

        int familyID = int.Parse(Session["family_id"].ToString());
        //Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("media_files", "group_id", int.Parse(Session["media_file_id"].ToString())).ToString());
        Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
        //if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
        //{
        //    LoginManager.LogoutFromSite("login.html");
        //    return "";
        //}

        List<string> currentBrandsList = GetFamilyGroupBrands(nLogedInGroupID, familyID);
        System.Text.StringBuilder sRet = new System.Text.StringBuilder();
        sRet.Append("Current Device Brands");
        sRet.Append("~~|~~");
        sRet.Append("Available Device Brands");
        sRet.Append("~~|~~");

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from lu_DeviceBrands with (nolock) where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("Device_Family_ID", "=", familyID);
        Int32 nCount = 0;
        if (selectQuery.Execute("query", true) != null)
        {
            nCount = selectQuery.Table("query").DefaultView.Count;
        }
        
        sRet.Append("<root>");
        for (int i = 0; i < nCount; i++)
        {
            string sID = selectQuery.Table("query").DefaultView[i].Row["ID"].ToString();
            string sTitle = selectQuery.Table("query").DefaultView[i].Row["Name"].ToString();
            //Need to add brand description
            string sDescription = "";
            string sInList = "false";
            if (currentBrandsList != null && currentBrandsList.Contains(sID))
            {
                sInList = "true";
                
            }
            
            sRet.Append("<item id=\"" + sID + "\"  title=\"" + sTitle + "\" description=\"" + sDescription + "\" inList=\"" + sInList + "\" />");
        }
        sRet.Append("</root>");
        selectQuery.Finish();
        selectQuery = null;
        return sRet.ToString();
    }

}
