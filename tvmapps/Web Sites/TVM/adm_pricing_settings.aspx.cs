using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using KLogMonitor;
using TVinciShared;
using CachingProvider.LayeredCache;

public partial class adm_pricing_settings : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {

        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        else if (LoginManager.IsPagePermitted("adm_pricing_settings.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                // add invalidation key for user roles cache
                int groupId = ODBCWrapper.Utils.GetIntSafeVal(LoginManager.GetLoginGroupID());
                string invalidationKey = LayeredCacheKeys.GetPricingSettingsInvalidationKey(groupId);
                if (!CachingProvider.LayeredCache.LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to set invalidation key on User.Save key = {0}", invalidationKey);
                }

                DBManipulator.DoTheWork("pricing_connection");
            }
        }
    }


    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Billing Settings");
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        object t = null;

        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }

        int groupId = LoginManager.GetLoginGroupID();

        // check ig groupid is parent , if so show page with all filed else (not parent show a message)
        int tableID = GetPricingSettingsID(groupId);
        bool isParentGroup = IsParentGroup(groupId);

        string sTable = string.Empty;
        if (!isParentGroup)
        {
            sTable = (PageUtils.GetPreHeader() + ": Module is not implemented");
        }
        else
        {
            if (tableID > 0) // a new record
            {
                t = tableID;
            }

            string sBack = "adm_pricing_settings.aspx?search_save=1";
            DBRecordWebEditor theRecord = new DBRecordWebEditor("groups_parameters", "adm_table_pager", sBack, "", "ID", t, sBack, "");
            theRecord.SetConnectionKey("pricing_connection");

            DataRecordDropDownField dr_adsPolicy = new DataRecordDropDownField("", "NAME", "id", "", null, 60, true);
            dr_adsPolicy.SetSelectsDT(GetAdsPolicyDT());
            dr_adsPolicy.Initialize("Ads Policy", "adm_table_header_nbg", "FormInput", "ADS_POLICY", false);
            dr_adsPolicy.SetNoSelectStr("---");
            dr_adsPolicy.SetFieldType("string");
            theRecord.AddRecord(dr_adsPolicy);

            DataRecordShortTextField dr_adsParam = new DataRecordShortTextField("ltr", true, 60, 128);
            dr_adsParam.Initialize("Ads Param", "adm_table_header_nbg", "FormInput", "ADS_PARAM", false);
            theRecord.AddRecord(dr_adsParam);

            DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
            dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
            dr_groups.SetValue(groupId.ToString());
            theRecord.AddRecord(dr_groups);

            DataRecordShortIntField dr_fictivic_group = new DataRecordShortIntField(false, 9, 9);
            dr_fictivic_group.Initialize("Fictivic Group ID", "adm_table_header_nbg", "FormInput", "FICTIVIC_GROUP_ID", false);
            dr_fictivic_group.SetValue(GetFictivicGroupId(groupId));
            theRecord.AddRecord(dr_fictivic_group);

            sTable = theRecord.GetTableHTML("adm_pricing_settings.aspx?submited=1");
        }
        return sTable;
    }

    private string GetFictivicGroupId(int groupId)
    {
        object fictivicGroupId = ODBCWrapper.Utils.GetTableSingleVal("groups", "FICTIVIC_GROUP_ID", groupId, "MAIN_CONNECTION_STRING");
        if (fictivicGroupId == null)
            return "0";
        else return fictivicGroupId.ToString();
    }

    private System.Data.DataTable GetAdsPolicyDT()
    {
        System.Data.DataTable dt = new System.Data.DataTable();
        dt.Columns.Add("id", typeof(int));
        dt.Columns.Add("txt", typeof(string));
        foreach (ApiObjects.AdsPolicy r in Enum.GetValues(typeof(ApiObjects.AdsPolicy)))
        {
            dt.Rows.Add((int)r, r);
        }
        return dt;
    }

    private int GetPricingSettingsID(int groupID)
    {
        int pricingSettingId = 0;
        try
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("Pricing_connection");
            selectQuery += "select ID from groups_parameters where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    pricingSettingId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row, "ID");
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        catch (Exception ex)
        {
            log.Error("", ex);
            pricingSettingId = 0;
        }
        return pricingSettingId;
    }

    private bool IsParentGroup(int groupID)
    {
        bool res = false;
        try
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
            selectQuery += "select PARENT_GROUP_ID from groups where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", groupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    int parentGroupID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row, "PARENT_GROUP_ID");
                    if (parentGroupID == 1)
                    {
                        res = true;
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        catch (Exception ex)
        {
            log.Error("", ex);
            res = false;
        }
        return res;
    }
}