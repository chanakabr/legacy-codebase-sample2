using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using KLogMonitor;
using TVinciShared;

public partial class adm_payment_gateway_payment_method_new : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            LoginManager.LogoutFromSite("login.html");

        else if (LoginManager.IsPagePermitted("adm_payment_gateway.aspx") == false)
            LoginManager.LogoutFromSite("login.html");

        else if (LoginManager.IsActionPermittedOnPage("adm_payment_gateway.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID, "adm_payment_gateway.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork("billing_connection");
                return;
            }
            if (Request.QueryString["payment_gateway_payment_method_id"] != null && Request.QueryString["payment_gateway_payment_method_id"].ToString() != "")
            {
                Session["payment_gateway_payment_method_id"] = int.Parse(Request.QueryString["payment_gateway_payment_method_id"].ToString());
            }
            else
            {
                Session["payment_gateway_payment_method_id"] = 0;
            }


            if (Session["paymentGW_id"] == null || Session["paymentGW_id"].ToString() == "" || Session["paymentGW_id"].ToString() == "0")
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }
        }
    }

    public void GetHeader()
    {
        if (Session["payment_gateway_payment_method_id"] != null && Session["payment_gateway_payment_method_id"].ToString() != "" && int.Parse(Session["payment_gateway_payment_method_id"].ToString()) != 0)
            Response.Write(PageUtils.GetPreHeader() + ":Payment Gateway Payment Method - Edit");
        else
            Response.Write(PageUtils.GetPreHeader() + ":PaymentGateWay Payment Method- New");
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

        object t = null; ;
        if (Session["payment_gateway_payment_method_id"] != null && Session["payment_gateway_payment_method_id"].ToString() != "" && int.Parse(Session["payment_gateway_payment_method_id"].ToString()) != 0)
            t = Session["payment_gateway_payment_method_id"];
        string sBack = "adm_payment_gateway_payment_method.aspx?search_save=1&paymentGW_id=" + Session["paymentGW_id"].ToString();

        DBRecordWebEditor theRecord = new DBRecordWebEditor("payment_gateway_payment_method", "adm_table_pager", sBack, "", "ID", t, sBack, "payment_gateway_id");
        theRecord.SetConnectionKey("billing_connection");
        
        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "NAME", false);
        theRecord.AddRecord(dr_name);
              
        DataRecordCheckBoxField dr_allow_multi_instance = new DataRecordCheckBoxField(true);
        dr_allow_multi_instance.Initialize("Allow Multi Instance", "adm_table_header_nbg", "FormInput", "ALLOW_MULTI_INSTANCE", false);
        theRecord.AddRecord(dr_allow_multi_instance);


        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(groupID.ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_payment_gateway_id = new DataRecordShortIntField(false, 9, 9);
        dr_payment_gateway_id.Initialize("payment_gateway_id", "adm_table_header_nbg", "FormInput", "payment_gateway_id", false);
        dr_payment_gateway_id.SetValue(Session["paymentGW_id"].ToString());
        theRecord.AddRecord(dr_payment_gateway_id);



        string sTable = theRecord.GetTableHTML("adm_payment_gateway_payment_method_new.aspx?submited=1");
        return sTable;
    }
}