using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using TVinciShared;
using TvinciImporter;

public partial class adm_operator_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_operator.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_operator.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
            {
                DBManipulator.DoTheWork();
                return;
            }
            Int32 nMenuID = 0;

            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);

            if (Request.QueryString["operator_id"] != null &&
                Request.QueryString["operator_id"].ToString() != "")
            {
                Session["operator_id"] = int.Parse(Request.QueryString["operator_id"].ToString());
            }
            else
                Session["operator_id"] = 0;
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Operator management");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    protected void GetLangMenu()
    {
        Response.Write(m_sLangMenu);
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object t = null; ;
        if (Session["operator_id"] != null && Session["operator_id"].ToString() != "" && int.Parse(Session["operator_id"].ToString()) != 0)
            t = Session["operator_id"];
        string sBack = "adm_operator.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("groups_operators", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Operator name", "adm_table_header_nbg", "FormInput", "Name", true);
        theRecord.AddRecord(dr_name);

        DataRecordShortTextField dr_client_ID = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_client_ID.Initialize("Client ID", "adm_table_header_nbg", "FormInput", "client_id", true);
        theRecord.AddRecord(dr_client_ID);

        DataRecordShortTextField dr_client_secret = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_client_secret.Initialize("Client secret", "adm_table_header_nbg", "FormInput", "client_secret", false);
        theRecord.AddRecord(dr_client_secret);

        object parent_group_id = LoginManager.GetLoginGroupID();
        if (parent_group_id == null)
        {
            parent_group_id = 156;
        }

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "group_id", false);
        dr_groups.SetValue(parent_group_id.ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortTextField dr_extra_params = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_extra_params.Initialize("Extra parameters", "adm_table_header_nbg", "FormInput", "extra_params", false);
        theRecord.AddRecord(dr_extra_params);

        DataRecordShortTextField dr_url_code = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_url_code.Initialize("URL Code", "adm_table_header_nbg", "FormInput", "URL_Code", false);
        theRecord.AddRecord(dr_url_code);

        DataRecordShortTextField dr_url_creds = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_url_creds.Initialize("URL creds", "adm_table_header_nbg", "FormInput", "URL_Creds", false);
        theRecord.AddRecord(dr_url_creds);

        DataRecordShortTextField dr_url_refresh = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_url_refresh.Initialize("URL Refresh", "adm_table_header_nbg", "FormInput", "URL_Refresh", false);
        theRecord.AddRecord(dr_url_refresh);

        DataRecordShortTextField dr_url_login = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_url_login.Initialize("URL Login", "adm_table_header_nbg", "FormInput", "URL_Login", true);
        theRecord.AddRecord(dr_url_login);

        DataRecordOnePicBrowserField dr_pic = new DataRecordOnePicBrowserField("operator");
        dr_pic.Initialize("Thumb", "adm_table_header_nbg", "FormInput", "Pic_ID", false);
        theRecord.AddRecord(dr_pic);

        DataRecordShortTextField dr_color_code = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_color_code.Initialize("Color code", "adm_table_header_nbg", "FormInput", "Color_Code", true);
        theRecord.AddRecord(dr_color_code);

        DataRecordShortTextField dr_about_us = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_about_us.Initialize("About Us", "adm_table_header_nbg", "FormInput", "About_Us", false);
        theRecord.AddRecord(dr_about_us);

        DataRecordShortTextField dr_contact_us = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_contact_us.Initialize("Contact Us", "adm_table_header_nbg", "FormInput", "Contact_Us", false);
        theRecord.AddRecord(dr_contact_us);


        DataRecordDropDownField dr_sub_group_ID = new DataRecordDropDownField("groups", "GROUP_NAME", "ID", "", null, 60, true);
        dr_sub_group_ID.Initialize("Sub group name", "adm_table_header_nbg", "FormInput", "Sub_Group_ID", false);
        string sQuery = "select ID, GROUP_NAME as txt from groups where is_active=1 and status=1 and PARENT_GROUP_ID = " + parent_group_id.ToString() + " and ID not in (select Sub_Group_ID from groups_operators where is_active=1 and status=1 and Sub_Group_ID is not null)" +
                        " or ID = (select Sub_Group_ID from groups_operators where ID = " + Session["operator_id"].ToString() + ")";
        dr_sub_group_ID.SetSelectsQuery(sQuery);
        theRecord.AddRecord(dr_sub_group_ID);

        string sTable = theRecord.GetTableHTML("adm_operator_new.aspx?submited=1");

        return sTable;
    }
}
