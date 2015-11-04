using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using KLogMonitor;
using TVinciShared;

public partial class adm_media_locales_new : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsPagePermitted("adm_media.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsActionPermittedOnPage("adm_media.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
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
                DBManipulator.DoTheWork();

                try
                {
                    Notifiers.BaseMediaNotifier t = null;
                    Notifiers.Utils.GetBaseMediaNotifierImpl(ref t, LoginManager.GetLoginGroupID());
                    if (t != null)
                        t.NotifyChange(Session["media_id"].ToString());
                    return;
                }
                catch (Exception ex)
                {
                    log.Error("exception - " + Session["media_id"].ToString() + " : " + ex.Message, ex);
                }

                return;
            }

            if (Session["media_id"] == null || Session["media_id"].ToString() == "" || Session["media_id"].ToString() == "0")
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }

            if (Request.QueryString["media_locale_id"] != null && Request.QueryString["media_locale_id"].ToString() != "")
            {
                Session["media_locale_id"] = int.Parse(Request.QueryString["media_locale_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("media", "group_id", int.Parse(Session["media_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["media_locale_id"] = "0";
        }
    }

    public void GetHeader()
    {
        if (Session["media_locale_id"] != null && Session["media_locale_id"].ToString() != "" && int.Parse(Session["media_locale_id"].ToString()) != 0)
            Response.Write(PageUtils.GetPreHeader() + ":Media Locale - Edit");
        else
            Response.Write(PageUtils.GetPreHeader() + ":Media Locale - New");
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
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        object t = null; ;
        if (Session["media_locale_id"] != null && Session["media_locale_id"].ToString() != "" && int.Parse(Session["media_locale_id"].ToString()) != 0)
            t = Session["media_locale_id"];
        string sBack = "adm_media_locales.aspx?search_save=1&media_id=" + Session["media_id"].ToString();
        DBRecordWebEditor theRecord = new DBRecordWebEditor("media_locale_values", "adm_table_pager", sBack, "", "ID", t, sBack, "media_id");

        DataRecordDropDownField dr_device = new DataRecordDropDownField("groups_devices", "DEVICE_NAME", "ID", "group_id", nGroupID, 60, true);
        dr_device.Initialize("Device", "adm_table_header_nbg", "FormInput", "device_id", false);
        dr_device.SetNoSelectStr("---");
        dr_device.SetOrderBy("DEVICE_NAME");
        theRecord.AddRecord(dr_device);

        DataRecordDropDownField dr_country = new DataRecordDropDownField("countries", "COUNTRY_NAME", "ID", "", null, 60, false);
        dr_country.Initialize("Country", "adm_table_header_nbg", "FormInput", "country_id", false);
        dr_country.SetOrderBy("ID");
        theRecord.AddRecord(dr_country);

        DataRecordDropDownField dr_language = new DataRecordDropDownField("lu_languages", "NAME", "ID", "", null, 60, false);
        dr_language.Initialize("Language", "adm_table_header_nbg", "FormInput", "language_id", false);
        dr_language.SetOrderBy("name");
        theRecord.AddRecord(dr_language);

        DataRecordDateTimeField dr_start_date = new DataRecordDateTimeField(true);
        dr_start_date.Initialize("Start Date", "adm_table_header_nbg", "FormInput", "START_DATE", false);
        dr_start_date.SetDefault(DateTime.Now);
        theRecord.AddRecord(dr_start_date);

        DataRecordDateTimeField dr_end_date = new DataRecordDateTimeField(true);
        dr_end_date.Initialize("Catalog End Date", "adm_table_header_nbg", "FormInput", "END_DATE", false);
        theRecord.AddRecord(dr_end_date);

        DataRecordDateTimeField dr_final_end_date = new DataRecordDateTimeField(true);
        dr_final_end_date.Initialize("Final End Date", "adm_table_header_nbg", "FormInput", "FINAL_END_DATE", false);
        theRecord.AddRecord(dr_final_end_date);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_media_id = new DataRecordShortIntField(false, 9, 9);
        dr_media_id.Initialize("Media ID", "adm_table_header_nbg", "FormInput", "MEDIA_ID", false);
        dr_media_id.SetValue(Session["media_id"].ToString());
        theRecord.AddRecord(dr_media_id);

        string sTable = theRecord.GetTableHTML("adm_media_locales_new.aspx?submited=1");
        return sTable;
    }
}
