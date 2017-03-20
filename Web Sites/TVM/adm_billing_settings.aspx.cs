using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using KLogMonitor;
using TVinciShared;

public partial class adm_billing_settings : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
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
            dr_paymentGW.SetWhereString("status=1 and is_active=1");
            theRecord.AddRecord(dr_paymentGW);

            DataRecordCheckBoxField dr_SecurityQuestion = new DataRecordCheckBoxField(true);
            dr_SecurityQuestion.Initialize("Enable Payment Gateway Selection", "adm_table_header_nbg", "FormInput", "ENABLE_PAYMENT_GATEWAY_SELECTION", false);
            theRecord.AddRecord(dr_SecurityQuestion);

            DataRecordDropDownField dr_ossAdapter = new DataRecordDropDownField("oss_adapter", "name", "id", "group_id", groupId, 60, true);
            dr_ossAdapter.SetFieldType("int");
            System.Data.DataTable ossAdapterDT = GetOSSAdapterDT();
            dr_ossAdapter.SetSelectsDT(ossAdapterDT);
            dr_ossAdapter.Initialize("OSS adapter", "adm_table_header_nbg", "FormInput", "OSS_ADAPTER", false);
            dr_ossAdapter.SetDefault(0);
            theRecord.AddRecord(dr_ossAdapter);


            DataRecordCheckBoxField dr_SendFirstPurchaseMail = new DataRecordCheckBoxField(true);
            dr_SendFirstPurchaseMail.Initialize("Send First Purchase Email", "adm_table_header_nbg", "FormInput", "SEND_FIRST_PURCHASE_MAIL", false);
            dr_SendFirstPurchaseMail.SetDefault(1);
            theRecord.AddRecord(dr_SendFirstPurchaseMail);

            DataRecordCheckBoxField dr_SendRenewMail = new DataRecordCheckBoxField(true);
            dr_SendRenewMail.Initialize("Send Renew Email", "adm_table_header_nbg", "FormInput", "SEND_RENEW_MAIL", false);
            dr_SendRenewMail.SetDefault(1);
            theRecord.AddRecord(dr_SendRenewMail);

            DataRecordCheckBoxField dr_SendFailPurchaseMail = new DataRecordCheckBoxField(true);
            dr_SendFailPurchaseMail.Initialize("Send Fail Purchase Email", "adm_table_header_nbg", "FormInput", "SEND_FAIL_PURCHASE_MAIL", false);
            dr_SendFailPurchaseMail.SetDefault(1);
            theRecord.AddRecord(dr_SendFailPurchaseMail);

            DataRecordCheckBoxField dr_SendFailRenewMail = new DataRecordCheckBoxField(true);
            dr_SendFailRenewMail.Initialize("Send Fail Renew Email", "adm_table_header_nbg", "FormInput", "SEND_FAIL_RENEW_MAIL", false);
            dr_SendFailRenewMail.SetDefault(1);
            theRecord.AddRecord(dr_SendFailRenewMail);

            DataRecordShortTextField shortTextField = null;

            shortTextField = new DataRecordShortTextField("ltr", true, 60, 128);
            shortTextField.Initialize("InApp URL", "adm_table_header_nbg", "FormInput", "InAppURL", false);
            theRecord.AddRecord(shortTextField);

            shortTextField = new DataRecordShortTextField("ltr", true, 60, 128);
            shortTextField.Initialize("InApp token", "adm_table_header_nbg", "FormInput", "InAppToken", false);
            theRecord.AddRecord(shortTextField);

            shortTextField = new DataRecordShortTextField("ltr", true, 60, 128);
            shortTextField.Initialize("InApp shared secret", "adm_table_header_nbg", "FormInput", "InAppSharedSecret", false);
            theRecord.AddRecord(shortTextField);

            //DataRecordShortIntField shortIntField = null;
            //shortIntField = new DataRecordShortIntField(true, 9, 9);
            //shortIntField.Initialize("Multi cc users", "adm_table_header_nbg", "FormInput", "MULTI_USER_CC", false);
            //theRecord.AddRecord(shortIntField);

            shortTextField = new DataRecordShortTextField("ltr", true, 60, 128);
            shortTextField.Initialize("Mail from - name", "adm_table_header_nbg", "FormInput", "MAIL_FROM_NAME", false);
            theRecord.AddRecord(shortTextField);

            shortTextField = new DataRecordShortTextField("ltr", true, 60, 128);
            shortTextField.Initialize("Mail from - address", "adm_table_header_nbg", "FormInput", "MAIL_FROM_ADD", false);
            theRecord.AddRecord(shortTextField);

            shortTextField = new DataRecordShortTextField("ltr", true, 60, 128);
            shortTextField.Initialize("Purchase mail template", "adm_table_header_nbg", "FormInput", "PURCHASE_MAIL", false);
            theRecord.AddRecord(shortTextField); 
            
            shortTextField = new DataRecordShortTextField("ltr", true, 60, 128);
            shortTextField.Initialize("Purchase mail subject", "adm_table_header_nbg", "FormInput", "PURCHASE_MAIL_SUBJECT", false);
            theRecord.AddRecord(shortTextField);

            shortTextField = new DataRecordShortTextField("ltr", true, 60, 128);
            shortTextField.Initialize("Purchase fail mail template", "adm_table_header_nbg", "FormInput", "PURCHASE_FAIL_MAIL", false);
            theRecord.AddRecord(shortTextField);

            shortTextField = new DataRecordShortTextField("ltr", true, 60, 128);
            shortTextField.Initialize("Purchase fail mail subject", "adm_table_header_nbg", "FormInput", "PURCHASE_FAIL_MAIL_SUBJECT", false);
            theRecord.AddRecord(shortTextField);

            shortTextField = new DataRecordShortTextField("ltr", true, 60, 128);
            shortTextField.Initialize("Preview module cor mail template", "adm_table_header_nbg", "FormInput", "PREVIEW_MODULE_COR_MAIL", false);
            theRecord.AddRecord(shortTextField);

            shortTextField = new DataRecordShortTextField("ltr", true, 60, 128);
            shortTextField.Initialize("Preview module cor mail subject", "adm_table_header_nbg", "FormInput", "PREVIEW_MODULE_COR_MAIL_SUBJECT", false);
            theRecord.AddRecord(shortTextField);

            shortTextField = new DataRecordShortTextField("ltr", true, 60, 128);
            shortTextField.Initialize("Preview module template", "adm_table_header_nbg", "FormInput", "preview_module_template", false);
            theRecord.AddRecord(shortTextField);

            shortTextField = new DataRecordShortTextField("ltr", true, 60, 128);
            shortTextField.Initialize("Purchase with preview module mail template", "adm_table_header_nbg", "FormInput", "PURCHASE_WITH_PREVIEW_MODULE_MAIL", false);
            theRecord.AddRecord(shortTextField);            

            shortTextField = new DataRecordShortTextField("ltr", true, 60, 128);
            shortTextField.Initialize("Purchase with preview module mail subject", "adm_table_header_nbg", "FormInput", "PURCHASE_WITH_PREVIEW_MODULE_MAIL_SUBJECT", false);
            theRecord.AddRecord(shortTextField);           

            shortTextField = new DataRecordShortTextField("ltr", true, 60, 128);
            shortTextField.Initialize("Purchase with gift card mail template", "adm_table_header_nbg", "FormInput", "PURCHASE_WITH_GIFT_CARD_MAIL", false);
            theRecord.AddRecord(shortTextField);

            shortTextField = new DataRecordShortTextField("ltr", true, 60, 128);
            shortTextField.Initialize("Purchase with gift card mail subject", "adm_table_header_nbg", "FormInput", "PURCHASE_WITH_GIFT_CARD_MAIL_SUBJECT", false);
            theRecord.AddRecord(shortTextField);

            shortTextField = new DataRecordShortTextField("ltr", true, 60, 128);
            shortTextField.Initialize("Gift card reminder mail template", "adm_table_header_nbg", "FormInput", "GIFT_CARD_REMINDER_MAIL_TEMPLATE_NAME", false);
            theRecord.AddRecord(shortTextField);

            shortTextField = new DataRecordShortTextField("ltr", true, 60, 128);
            shortTextField.Initialize("Gift card reminder mail subject", "adm_table_header_nbg", "FormInput", "GIFT_CARD_REMINDER_MAIL_SUBJECT", false);
            theRecord.AddRecord(shortTextField);



            sTable = theRecord.GetTableHTML("adm_billing_settings.aspx?submited=1");
        }
        return sTable;
    }

    private System.Data.DataTable GetOSSAdapterDT()
    {
        apiWS.API m = new apiWS.API();
        string sWSURL = TVinciShared.WS_Utils.GetTcmConfigValue("api_ws");

        if (sWSURL != "")
            m.Url = sWSURL;
        string sWSUserName = "";
        string sWSPass = "";

        string sIP = "1.1.1.1";
        TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "GetOSSAdapter", "api", "1.1.1.1", ref sWSUserName, ref sWSPass);


        System.Data.DataTable ossAdapterDT = GetBaseDT();

        apiWS.OSSAdapterResponseList ossAdapterResponseList = m.GetOSSAdapter(sWSUserName, sWSPass);
        if (ossAdapterResponseList != null && ossAdapterResponseList.Status.Code == 0 && ossAdapterResponseList.OSSAdapters != null)
        {
            System.Data.DataRow tmpRow = null;

            for (int i = 0; i < ossAdapterResponseList.OSSAdapters.Length; i++)
            {
                tmpRow = ossAdapterDT.NewRow();
                tmpRow["ID"] = ossAdapterResponseList.OSSAdapters[i].ID;
                tmpRow["txt"] = ossAdapterResponseList.OSSAdapters[i].Name;

                ossAdapterDT.Rows.InsertAt(tmpRow, 0);
                ossAdapterDT.AcceptChanges();
            }
        }
        return ossAdapterDT;
    }

    private System.Data.DataTable GetBaseDT()
    {
        System.Data.DataTable dT = new System.Data.DataTable();
        Int32 n = 0;
        string s = "";
        dT.Columns.Add(PageUtils.GetColumn("ID", n));
        dT.Columns.Add(PageUtils.GetColumn("txt", s));
        return dT.Copy();
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
            log.Error("", ex);
            billingSettingId = 0;
        }
        return billingSettingId;
    }
}