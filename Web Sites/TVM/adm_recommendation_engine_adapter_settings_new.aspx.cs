using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_recommendation_engine_adapter_settings_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            LoginManager.LogoutFromSite("login.html");

        else if (LoginManager.IsPagePermitted("adm_recommendation_engine_adapter.aspx") == false)
            LoginManager.LogoutFromSite("login.html");

        else if (LoginManager.IsActionPermittedOnPage("adm_recommendation_engine_adapter.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID, "adm_recommendation_engine_adapter.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork();
                int paymentGatewayId = 0;

                if (Session["adapter_id"] != null && !string.IsNullOrEmpty(Session["adapter_id"].ToString()) && int.TryParse(Session["adapter_id"].ToString(), out paymentGatewayId))
                {
                    //// set adapter configuration
                    //Billing.module billing = new Billing.module();

                    //string sIP = "1.1.1.1";
                    //string sWSUserName = "";
                    //string sWSPass = "";
                    //TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "SetPaymentGatewayConfiguration", "billing", sIP, ref sWSUserName, ref sWSPass);
                    //string sWSURL = TVinciShared.WS_Utils.GetTcmConfigValue("billing_ws");
                    //if (!string.IsNullOrEmpty(sWSURL))
                    //    billing.Url = sWSURL;
                    //try
                    //{
                    //    Billing.Status status = billing.SetPaymentGatewayConfiguration(sWSUserName, sWSPass, paymentGatewayId);
                    //    Logger.Logger.Log("SetPaymentGatewayConfiguration", string.Format("payment gateway ID:{0}, status:{1}", paymentGatewayId, status.Code), "SetPaymentGatewayConfiguration");
                    //}
                    //catch (Exception ex)
                    //{
                    //    Logger.Logger.Log("Exception", string.Format("payment gateway ID:{0}, ex msg:{1}, ex st: {2} ", paymentGatewayId, ex.Message, ex.StackTrace), "SetPaymentGatewayConfiguration");
                    //}
                }

                return;
            }
            if (Request.QueryString["setting_id"] != null && Request.QueryString["setting_id"].ToString() != "")
            {
                Session["setting_id"] = int.Parse(Request.QueryString["setting_id"].ToString());
            }
            else
            {
                Session["setting_id"] = 0;
            }


            if (Session["adapter_id"] == null || Session["adapter_id"].ToString() == "" || Session["adapter_id"].ToString() == "0")
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }
        }
    }

    public void GetHeader()
    {
        if (Session["setting_id"] != null && Session["setting_id"].ToString() != "" && int.Parse(Session["setting_id"].ToString()) != 0)
            Response.Write(PageUtils.GetPreHeader() + ":Recommendation Engine Params - Edit");
        else
            Response.Write(PageUtils.GetPreHeader() + ":Recommendation Engine Params - New");
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
        Int32 groupID = LoginManager.GetLoginGroupID();

        object settingId = null;

        if (Session["setting_id"] != null && Session["setting_id"].ToString() != "" && int.Parse(Session["setting_id"].ToString()) != 0)
        {
            settingId = Session["setting_id"];
        }

        string backPage = "adm_recommendation_engine_adapter_settings.aspx?search_save=1&adapter_id=" + Session["adapter_id"].ToString();

        DBRecordWebEditor theRecord = 
            new DBRecordWebEditor("recommendation_engines_settings", "adm_table_pager", backPage, "", "ID", settingId, backPage, "recommendation_engine_id");
        theRecord.SetConnectionKey("main_connection_string");

        DataRecordShortTextField dr_key = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_key.Initialize("Key", "adm_table_header_nbg", "FormInput", "keyName", false);
        theRecord.AddRecord(dr_key);

        DataRecordLongTextField dr_value = new DataRecordLongTextField("ltr", true, 60, 4);
        dr_value.Initialize("Value", "adm_table_header_nbg", "FormInput", "value", false);
        theRecord.AddRecord(dr_value);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(groupID.ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_adapter_id = new DataRecordShortIntField(false, 9, 9);
        dr_adapter_id.Initialize("recommendation_engine_id", "adm_table_header_nbg", "FormInput", "recommendation_engine_id", false);
        dr_adapter_id.SetValue(Session["adapter_id"].ToString());
        theRecord.AddRecord(dr_adapter_id);

        string sTable = theRecord.GetTableHTML("adm_recommendation_engine_adapter_settings_new.aspx?submited=1");
        return sTable;
    }
}