using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_payment_gateway_new : System.Web.UI.Page
{
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
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
            {
                Int32 nID = DBManipulator.DoTheWork("billing_connection");                
                return;
            }
            
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);

            if (Request.QueryString["paymentGW_id"] != null && Request.QueryString["paymentGW_id"].ToString() != "")
            {
                Session["paymentGW_id"] = int.Parse(Request.QueryString["paymentGW_id"].ToString());
            }
            else
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

        DBRecordWebEditor theRecord = new DBRecordWebEditor("payment_gateway", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("billing_connection");
        
        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_name);

        DataRecordLongTextField dr_url = new DataRecordLongTextField("ltr", true, 60, 4);
        dr_url.Initialize("URL", "adm_table_header_nbg", "FormInput", "url", false);
        theRecord.AddRecord(dr_url);
        
        string sTable = theRecord.GetTableHTML("adm_payment_gateway_new.aspx?submited=1");

        return sTable;
    }
}