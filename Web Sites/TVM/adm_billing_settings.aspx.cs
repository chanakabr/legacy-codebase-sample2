using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_billing_settings : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {

        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        else if (LoginManager.IsPagePermitted("adm_billing_settings.aspx") == false)
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
                DBManipulator.DoTheWork("billing_connection");
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

        object groupId = LoginManager.GetLoginGroupID();

        // check ig groupid is parent , if so show page with all filed else (not parent show a message)
        int tableID = GetBillingSettingsID(ODBCWrapper.Utils.GetIntSafeVal(groupId));
        bool isParentGroup = IsParentGroup(ODBCWrapper.Utils.GetIntSafeVal(groupId));

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
            string sBack = "adm_billing_settings.aspx?search_save=1";
            DBRecordWebEditor theRecord = new DBRecordWebEditor("groups_parameters", "adm_table_pager", sBack, "", "ID", t, sBack, "");
            theRecord.SetConnectionKey("billing_connection");

            DataRecordDropDownField dr_paymentGW = new DataRecordDropDownField("payment_gateway", "name", "id", "group_id", groupId, 60, true);
            dr_paymentGW.Initialize("Active Payment Gateway", "adm_table_header_nbg", "FormInput", "DEFAULT_PAYMENT_GATEWAY", false);
            theRecord.AddRecord(dr_paymentGW);

            DataRecordCheckBoxField dr_SecurityQuestion = new DataRecordCheckBoxField(true);
            dr_SecurityQuestion.Initialize("Enable Payment Gateway Selection", "adm_table_header_nbg", "FormInput", "ENABLE_PAYMENT_GATEWAY_SELECTION", false);
            theRecord.AddRecord(dr_SecurityQuestion);

            sTable = theRecord.GetTableHTML("adm_billing_settings.aspx?submited=1");
        }
        return sTable;
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
            res = false;
        }
        return res;
    }

    private int GetBillingSettingsID(int groupID)
    {
        int billingSettingId = 0;
        try
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("billing_connection");
            selectQuery += "select ID from groups_parameters where status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    billingSettingId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row, "ID");
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        catch (Exception ex)
        {
            billingSettingId = 0;
        }
        return billingSettingId;
    }
}