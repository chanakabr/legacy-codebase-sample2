using ConfigurationManager;
using KLogMonitor;
using System;
using System.Reflection;
using System.Web;
using TVinciShared;

public partial class adm_payment_gateway_new : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_payment_gateway.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_payment_gateway.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            bool flag = false;

            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
            {
                int pgid = 0;
                // Validate uniqe external id
                if (Session["paymentGW_id"] != null && Session["paymentGW_id"].ToString() != "" && int.Parse(Session["paymentGW_id"].ToString()) != 0)
                {
                    int.TryParse(Session["paymentGW_id"].ToString(), out pgid);
                }

                System.Collections.Specialized.NameValueCollection coll = HttpContext.Current.Request.Form;
                if (coll != null && coll.Count > 2 && !string.IsNullOrEmpty(coll["1_val"]))
                {
                    if (IsExternalIDExists(coll["1_val"], pgid))
                    {
                        Session["error_msg"] = "External Id must be unique";
                        flag = true;
                    }
                    else
                    {
                        Int32 nID = DBManipulator.DoTheWork("billing_connection");

                        // set adapter configuration
                        Billing.module billing = new Billing.module();

                        string sIP = "1.1.1.1";
                        string sWSUserName = "";
                        string sWSPass = "";
                        TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "SetPaymentGatewayConfiguration", "billing", sIP, ref sWSUserName, ref sWSPass);
                        string sWSURL = ApplicationConfiguration.WebServicesConfiguration.Billing.URL.Value;
                        if (sWSURL != "")
                            billing.Url = sWSURL;
                        try
                        {
                            Billing.Status status = billing.SetPaymentGatewayConfiguration(sWSUserName, sWSPass, nID);
                            log.Debug("SetPaymentGatewayConfiguration - " + string.Format("payment gateway ID:{0}, status:{1}", nID, status.Code));
                        }
                        catch (Exception ex)
                        {
                            log.Error("Exception - " + string.Format("payment gateway ID:{0}, ex msg:{1}, ex st: {2} ", nID, ex.Message, ex.StackTrace), ex);
                        }
                        return;
                    }
                }
            }

            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);

            if (Request.QueryString["paymentGW_id"] != null && Request.QueryString["paymentGW_id"].ToString() != "")
            {
                Session["paymentGW_id"] = int.Parse(Request.QueryString["paymentGW_id"].ToString());
            }
            else if (!flag)
                Session["paymentGW_id"] = 0;
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Payment Gateway");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
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
        if (Session["paymentGW_id"] != null && Session["paymentGW_id"].ToString() != "" && int.Parse(Session["paymentGW_id"].ToString()) != 0)
            t = Session["paymentGW_id"];
        string sBack = "adm_payment_gateway.aspx?search_save=1";

        object group_id = LoginManager.GetLoginGroupID();

        DBRecordWebEditor theRecord = new DBRecordWebEditor("payment_gateway", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("billing_connection");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_name);

        DataRecordShortTextField dr_external_identifier = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_external_identifier.Initialize("External Identifier", "adm_table_header_nbg", "FormInput", "external_identifier", true);
        theRecord.AddRecord(dr_external_identifier);

        DataRecordShortTextField dr_adapter_url = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_adapter_url.Initialize("Adapter URL", "adm_table_header_nbg", "FormInput", "adapter_url", false);
        theRecord.AddRecord(dr_adapter_url);

        DataRecordShortTextField dr_transact_url = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_transact_url.Initialize("Payment Gateway Transact URL", "adm_table_header_nbg", "FormInput", "transact_url", false);
        theRecord.AddRecord(dr_transact_url);

        DataRecordShortTextField dr_status_url = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_status_url.Initialize("Payment Gateway Status URL", "adm_table_header_nbg", "FormInput", "status_url", false);
        theRecord.AddRecord(dr_status_url);

        DataRecordShortTextField dr_renew_url = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_renew_url.Initialize("Payment Gateway Renew URL", "adm_table_header_nbg", "FormInput", "renew_url", false);
        theRecord.AddRecord(dr_renew_url);

        DataRecordShortIntField dr_pending_interval = new DataRecordShortIntField(true, 9, 9, 5);
        dr_pending_interval.Initialize("Pending Interval (Minutes)", "adm_table_header_nbg", "FormInput", "pending_interval", false);
        theRecord.AddRecord(dr_pending_interval);

        DataRecordShortIntField dr_pending_retries = new DataRecordShortIntField(true, 9, 9, null, 100);
        dr_pending_retries.Initialize("Pending Retries", "adm_table_header_nbg", "FormInput", "pending_retries", false);
        theRecord.AddRecord(dr_pending_retries);

        DataRecordShortIntField dr_renewal_interval = new DataRecordShortIntField(true, 9, 9, 15);
        dr_renewal_interval.Initialize("Renewal Interval (Minutes)", "adm_table_header_nbg", "FormInput", "renewal_interval_minutes", false);
        theRecord.AddRecord(dr_renewal_interval);

        DataRecordShortIntField dr_renewal_start = new DataRecordShortIntField(true, 9, 9);
        dr_renewal_start.Initialize("Renewal Start Offset (Minutes)", "adm_table_header_nbg", "FormInput", "renewal_start_minutes", false);
        theRecord.AddRecord(dr_renewal_start);

        DataRecordShortTextField dr_shared_secret = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_shared_secret.Initialize("Shared Secret", "adm_table_header_nbg", "FormInput", "shared_secret", false);
        theRecord.AddRecord(dr_shared_secret);

        DataRecordCheckBoxField dr_external_verification = new DataRecordCheckBoxField(true);
        dr_external_verification.Initialize("External Verification", "adm_table_header_nbg", "FormInput", "external_verification", false);
        theRecord.AddRecord(dr_external_verification);
        
        //DataRecordCheckBoxField dr_supportPaymentMethod = new DataRecordCheckBoxField(true);
        //dr_supportPaymentMethod.Initialize("Support Payment Method", "adm_table_header_nbg", "FormInput", "is_payment_method_support", false);
        //theRecord.AddRecord(dr_supportPaymentMethod);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "group_id", false);
        dr_groups.SetValue(group_id.ToString());
        theRecord.AddRecord(dr_groups);
        
        string sTable = theRecord.GetTableHTML("adm_payment_gateway_new.aspx?submited=1");

        return sTable;
    }

    static private bool IsExternalIDExists(string extId, int pgid)
    {
        int groupID = LoginManager.GetLoginGroupID();
        bool res = false;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("billing_connection");
        selectQuery += "select ID from payment_gateway where status=1 and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("external_identifier", "=", extId);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "<>", pgid);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                res = true;
                int pgeid = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "ID", 0);
                string pgname = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "NAME", 0);
                log.Debug("IsExternalIDExists - " + string.Format("id:{0}, name:{1}", pgeid, pgname));
            }
        }
        selectQuery.Finish();
        selectQuery = null;

        return res;
    }
}