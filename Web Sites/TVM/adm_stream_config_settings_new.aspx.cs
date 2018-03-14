using apiWS;
using ConfigurationManager;
using KLogMonitor;
using System;
using System.Reflection;
using TVinciShared;

public partial class adm_stream_config_settings_new : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            LoginManager.LogoutFromSite("login.html");

        else if (LoginManager.IsPagePermitted("adm_stream_config.aspx") == false)
            LoginManager.LogoutFromSite("login.html");

        else if (LoginManager.IsActionPermittedOnPage("adm_stream_config.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID, "adm_stream_config.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork();
                int cdnAdapterId = 0;

                if (Session["cdn_adapter_id"] != null && !string.IsNullOrEmpty(Session["cdn_adapter_id"].ToString()) && int.TryParse(Session["cdn_adapter_id"].ToString(), out cdnAdapterId))
                {
                    int parentGroupId = DAL.UtilsDal.GetParentGroupID(LoginManager.GetLoginGroupID());
                    // set adapter configuration
                    apiWS.API api = new apiWS.API();

                    string sIP = "1.1.1.1";
                    string sWSUserName = "";
                    string sWSPass = "";
                    TVinciShared.WS_Utils.GetWSUNPass(parentGroupId, "SendCDNAdapterConfiguration", "api", sIP, ref sWSUserName, ref sWSPass);
                    string sWSURL = ApplicationConfiguration.WebServicesConfiguration.Api.URL.Value;
                    if (!string.IsNullOrEmpty(sWSURL))
                        api.Url = sWSURL;
                    try
                    {
                        CDNAdapterResponse response = api.SendCDNAdapterConfiguration(sWSUserName, sWSPass, cdnAdapterId);                        
                        log.DebugFormat("SetCDNAdapterConfiguration - cdn adapter id:{0}, status:{1}",
                            cdnAdapterId, response != null && response.Status != null ? response.Status.Code : -1);

                        // remove adapter from cache
                        string version = ApplicationConfiguration.Version.Value;
                        string[] keys = new string[1] 
                                { 
                                    string.Format("{0}_cdn_adapter_{1}", version, cdnAdapterId)
                                };

                        QueueUtils.UpdateCache(parentGroupId, CouchbaseManager.eCouchbaseBucket.CACHE.ToString(), keys);
                    }
                    catch (Exception ex)
                    {
                        log.Debug("Exception - " + string.Format("CDN adapter id:{0}, ex msg:{1}, ex st: {2} ", cdnAdapterId, ex.Message, ex.StackTrace), ex);
                    }
                }

                return;
            }
            if (Request.QueryString["cdn_adapter_setting_id"] != null && Request.QueryString["cdn_adapter_setting_id"].ToString() != "")
            {
                Session["cdn_adapter_setting_id"] = int.Parse(Request.QueryString["cdn_adapter_setting_id"].ToString());
            }
            else
            {
                Session["cdn_adapter_setting_id"] = 0;
            }


            if (Session["cdn_adapter_id"] == null || Session["cdn_adapter_id"].ToString() == "" || Session["cdn_adapter_id"].ToString() == "0")
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }
        }
    }

    public void GetHeader()
    {
        if (Session["cdn_adapter_setting_id"] != null && Session["cdn_adapter_setting_id"].ToString() != "" && int.Parse(Session["cdn_adapter_setting_id"].ToString()) != 0)
            Response.Write(PageUtils.GetPreHeader() + ":CDN Adapter Settings - Edit");
        else
            Response.Write(PageUtils.GetPreHeader() + ":CDN Adapter Settings - New");
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
        if (Session["cdn_adapter_setting_id"] != null && Session["cdn_adapter_setting_id"].ToString() != "" && int.Parse(Session["cdn_adapter_setting_id"].ToString()) != 0)
            t = Session["cdn_adapter_setting_id"];
        string sBack = "adm_stream_config_settings.aspx?search_save=1&cdn_adapter_id=" + Session["cdn_adapter_id"].ToString();

        DBRecordWebEditor theRecord = new DBRecordWebEditor("streaming_companies_settings", "adm_table_pager", sBack, "", "ID", t, sBack, "cdn_adapter_id");

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

        DataRecordShortIntField dr_cdn_adapter_id = new DataRecordShortIntField(false, 9, 9);
        dr_cdn_adapter_id.Initialize("adapter_id", "adm_table_header_nbg", "FormInput", "adapter_id", false);
        dr_cdn_adapter_id.SetValue(Session["cdn_adapter_id"].ToString());
        theRecord.AddRecord(dr_cdn_adapter_id);

        string sTable = theRecord.GetTableHTML("adm_stream_config_settings_new.aspx?submited=1");
        return sTable;
    }
}