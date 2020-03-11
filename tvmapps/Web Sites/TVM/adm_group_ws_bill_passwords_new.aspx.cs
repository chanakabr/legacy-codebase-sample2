using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using TVinciShared;

public partial class adm_group_ws_bill_passwords_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(2, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 3, true);
            if (Request.QueryString["group_ws_bill_pass_id"] != null &&
                Request.QueryString["group_ws_bill_pass_id"].ToString() != "")
            {
                Session["group_ws_bill_pass_id"] = int.Parse(Request.QueryString["group_ws_bill_pass_id"].ToString());

                Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups_modules_ips", "group_id", int.Parse(Session["group_ws_bill_pass_id"].ToString()), "billing_connection").ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["group_ws_bill_pass_id"] = 0;

            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
                DBManipulator.DoTheWork("billing_connection");
        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Group API Passwords";
        if (Session["group_ws_bill_pass_id"] != null && Session["group_ws_bill_pass_id"].ToString() != "" && Session["group_ws_bill_pass_id"].ToString() != "0")
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
        if (Session["group_ws_bill_pass_id"] != null && Session["group_ws_bill_pass_id"].ToString() != "" && int.Parse(Session["group_ws_bill_pass_id"].ToString()) != 0)
            t = Session["group_ws_bill_pass_id"];
        string sBack = "adm_group_ws_bill_passwords.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("groups_modules_ips", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("billing_connection");
        DataRecordShortTextField dr_d = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_d.Initialize("IP", "adm_table_header_nbg", "FormInput", "IP", true);
        if (Session["group_ws_bill_pass_id"] == null || Session["group_ws_bill_pass_id"].ToString() == "" || Session["group_ws_bill_pass_id"].ToString() == "0")
            dr_d.SetValue("1.1.1.1");
        theRecord.AddRecord(dr_d);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);
        /*
        DataRecordShortTextField dr_sc = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_sc.Initialize("Secret Code", "adm_table_header_nbg", "FormInput", "SECRET_CODE", true);
        theRecord.AddRecord(dr_sc);
        */
        DataRecordShortTextField dr_un = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_un.Initialize("User Name", "adm_table_header_nbg", "FormInput", "USERNAME", true);
        theRecord.AddRecord(dr_un);

        DataRecordShortTextField dr_pass = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_pass.Initialize("Password", "adm_table_header_nbg", "FormInput", "PASSWORD", true);
        theRecord.AddRecord(dr_pass);

        DataRecordShortTextField dr_module_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_module_name.Initialize("Function Name", "adm_table_header_nbg", "FormInput", "MODULE_NAME", true);
        theRecord.AddRecord(dr_module_name);

        DataRecordShortTextField dr_ws_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_ws_name.Initialize("WS name(billing - local)", "adm_table_header_nbg", "FormInput", "WS_NAME", true);
        if (Session["group_ws_bill_pass_id"] == null || Session["group_ws_bill_pass_id"].ToString() == "" || Session["group_ws_bill_pass_id"].ToString() == "0")
            dr_ws_name.SetValue("billing");
        theRecord.AddRecord(dr_ws_name);
        /*
        DataRecordCheckBoxField dr_admin = new DataRecordCheckBoxField(true);
        dr_admin.Initialize("Allow client side", "adm_table_header_nbg", "FormInput", "ALLOW_CLIENT_SIDE", true);
        theRecord.AddRecord(dr_admin);
        */
        string sTable = theRecord.GetTableHTML("adm_group_ws_bill_passwords_new.aspx?submited=1");

        return sTable;
    }
}
