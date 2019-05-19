using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CachingProvider.LayeredCache;
using KLogMonitor;
using TVinciShared;

public partial class adm_coupons_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;

    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    static public bool IsTvinciImpl()
    {
        Int32 nImplID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("pricing_connection");
        selectQuery += "select * from groups_modules_implementations where is_active=1 and status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_ID", "=", 3);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                nImplID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["IMPLEMENTATION_ID"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        if (nImplID == 1)
            return true;
        return false;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            if (IsTvinciImpl() == false)
            {
                Server.Transfer("adm_module_not_implemented.aspx");
                return;
            }
            if (Session["coupon_group_id"] == null || Session["coupon_group_id"].ToString() == "")
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork("pricing_connection");
                int groupId = LoginManager.GetLoginGroupID();
                string invalidationKey = LayeredCacheKeys.GetPricingSettingsInvalidationKey(groupId);
                if (!CachingProvider.LayeredCache.LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to set pricing settings invalidation key after coupons lista add/update, key = {0}", invalidationKey);
                }
                return;
            }
            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["coupon_id"] != null &&
                Request.QueryString["coupon_id"].ToString() != "")
            {
                Session["coupon_id"] = int.Parse(Request.QueryString["coupon_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("coupons", "group_id", int.Parse(Session["coupon_id"].ToString()), "pricing_connection").ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["coupon_id"] = 0;
        }

    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Coupons";
        if (Session["coupon_id"] != null && Session["coupon_id"].ToString() != "" && Session["coupon_id"].ToString() != "0")
            sRet += " - Edit";
        else
            sRet += " - New";
        Response.Write(sRet);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object t = null; ;
        if (Session["coupon_id"] != null && Session["coupon_id"].ToString() != "" && int.Parse(Session["coupon_id"].ToString()) != 0)
            t = Session["coupon_id"];
        string sBack = "adm_coupons_list.aspx?coupon_group_id=" + Session["coupon_group_id"].ToString() + "&search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("coupons", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("pricing_connection");

        DataRecordShortTextField dr_domain = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_domain.Initialize("Code", "adm_table_header_nbg", "FormInput", "Code", true);
        theRecord.AddRecord(dr_domain);

        DataRecordShortIntField dr_coupon_group = new DataRecordShortIntField(false, 9, 9);
        dr_coupon_group.Initialize("Group", "adm_table_header_nbg", "FormInput", "COUPON_GROUP_ID", false);
        dr_coupon_group.SetValue(Session["coupon_group_id"].ToString());
        theRecord.AddRecord(dr_coupon_group);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_discounts_new.aspx?submited=1");

        return sTable;
    }
}
