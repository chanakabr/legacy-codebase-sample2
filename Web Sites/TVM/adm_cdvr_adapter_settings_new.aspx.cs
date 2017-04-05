using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using KLogMonitor;
using TVinciShared;

public partial class adm_cdvr_adapter_settings_new : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            LoginManager.LogoutFromSite("login.html");

        else if (LoginManager.IsPagePermitted("adm_cdvr_adapter.aspx") == false)
            LoginManager.LogoutFromSite("login.html");

        else if (LoginManager.IsActionPermittedOnPage("adm_cdvr_adapter.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID, "adm_cdvr_adapter.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork("CA_CONNECTION_STRING");
                int cdvrAdapterId = 0;

                if (Session["cdvr_adapter_id"] != null && !string.IsNullOrEmpty(Session["cdvr_adapter_id"].ToString()) && int.TryParse(Session["cdvr_adapter_id"].ToString(), out cdvrAdapterId))
                {
                    // set adapter configuration
                    ca_ws.module cas = new ca_ws.module();

                    string sIP = "1.1.1.1";
                    string sWSUserName = "";
                    string sWSPass = "";
                    TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "SendCDVRAdapterConfiguration", "conditionalaccess", sIP, ref sWSUserName, ref sWSPass);
                    string sWSURL = TVinciShared.WS_Utils.GetTcmConfigValue("conditionalaccess_ws");
                    if (!string.IsNullOrEmpty(sWSURL))
                        cas.Url = sWSURL;
                    try
                    {
                        ca_ws.CDVRAdapterResponse response = cas.SendCDVRAdapterConfiguration(sWSUserName, sWSPass, cdvrAdapterId);
                        log.Debug("SetCDVRAdapterConfiguration - " + string.Format("cdvr adapter id:{0}, status:{1}", 
                            cdvrAdapterId, response != null && response.Status != null ? response.Status.Code : -1));

                        // remove adapter from cache
                        string version = TVinciShared.WS_Utils.GetTcmConfigValue("Version");
                        string[] keys = new string[1] 
                                { 
                                    string.Format("{0}_cdvr_adapter_{1}", version, cdvrAdapterId)
                                };

                        QueueUtils.UpdateCache(LoginManager.GetLoginGroupID(), CouchbaseManager.eCouchbaseBucket.CACHE.ToString(), keys);
                    }
                    catch (Exception ex)
                    {
                        log.Debug("Exception - " + string.Format("oass adapter id:{0}, ex msg:{1}, ex st: {2} ", cdvrAdapterId, ex.Message, ex.StackTrace), ex);
                    }
                }

                return;
            }
            if (Request.QueryString["cdvr_adapter_setting_id"] != null && Request.QueryString["cdvr_adapter_setting_id"].ToString() != "")
            {
                Session["cdvr_adapter_setting_id"] = int.Parse(Request.QueryString["cdvr_adapter_setting_id"].ToString());
            }
            else
            {
                Session["cdvr_adapter_setting_id"] = 0;
            }


            if (Session["cdvr_adapter_id"] == null || Session["cdvr_adapter_id"].ToString() == "" || Session["cdvr_adapter_id"].ToString() == "0")
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }
        }
    }

    public void GetHeader()
    {
        if (Session["cdvr_adapter_setting_id"] != null && Session["cdvr_adapter_setting_id"].ToString() != "" && int.Parse(Session["cdvr_adapter_setting_id"].ToString()) != 0)
            Response.Write(PageUtils.GetPreHeader() + ":CDVR Adapter Settings - Edit");
        else
            Response.Write(PageUtils.GetPreHeader() + ":CDVR Adapter Settings - New");
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
        if (Session["cdvr_adapter_setting_id"] != null && Session["cdvr_adapter_setting_id"].ToString() != "" && int.Parse(Session["cdvr_adapter_setting_id"].ToString()) != 0)
            t = Session["cdvr_adapter_setting_id"];
        string sBack = "adm_cdvr_adapter_settings.aspx?search_save=1&cdvr_adapter_id=" + Session["cdvr_adapter_id"].ToString();

        DBRecordWebEditor theRecord = new DBRecordWebEditor("cdvr_adapters_settings", "adm_table_pager", sBack, "", "ID", t, sBack, "cdvr_adapter_id");

        theRecord.SetConnectionKey("CA_CONNECTION_STRING");

        DataRecordShortTextField dr_key = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_key.Initialize("Key", "adm_table_header_nbg", "FormInput", "key_name", false);
        theRecord.AddRecord(dr_key);

        DataRecordLongTextField dr_value = new DataRecordLongTextField("ltr", true, 60, 4);
        dr_value.Initialize("Value", "adm_table_header_nbg", "FormInput", "value", false);
        theRecord.AddRecord(dr_value);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(groupID.ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_cdvr_adapter_id = new DataRecordShortIntField(false, 9, 9);
        dr_cdvr_adapter_id.Initialize("adapter_id", "adm_table_header_nbg", "FormInput", "adapter_id", false);
        dr_cdvr_adapter_id.SetValue(Session["cdvr_adapter_id"].ToString());
        theRecord.AddRecord(dr_cdvr_adapter_id);



        string sTable = theRecord.GetTableHTML("adm_cdvr_adapter_settings_new.aspx?submited=1");
        return sTable;
    }
}