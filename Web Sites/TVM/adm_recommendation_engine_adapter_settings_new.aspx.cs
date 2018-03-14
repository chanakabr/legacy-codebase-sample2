using ConfigurationManager;
using System;
using System.Collections.Generic;
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
                int adapterId = 0;

                // After save is done:
                // Update cache (Recommendation Engines are saved in cache as well as in DB)
                // Update adapter itself that configuration has changed
                if (Session["adapter_id"] != null && !string.IsNullOrEmpty(Session["adapter_id"].ToString()) && int.TryParse(Session["adapter_id"].ToString(), out adapterId))
                {
                    string ip = "1.1.1.1";
                    string userName = "";
                    string password = "";

                    int parentGroupId = DAL.UtilsDal.GetParentGroupID(LoginManager.GetLoginGroupID());
                    TVinciShared.WS_Utils.GetWSUNPass(parentGroupId, "UpdateCache", "api", ip, ref userName, ref password);
                    string url = ApplicationConfiguration.WebServicesConfiguration.Api.URL.Value;
                    string version = TVinciShared.WS_Utils.GetTcmConfigValue("Version");

                    if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                    {
                        return;
                    }
                    else
                    {
                        List<string> keys = new List<string>();
                        keys.Add(string.Format("{0}_recommendation_engine_{1}", version, adapterId));

                        apiWS.API client = new apiWS.API();
                        client.Url = url;

                        var updateCacheResponse = client.UpdateCache(parentGroupId, "CACHE", keys.ToArray());
                        var updateConfigurationResponse = client.UpdateRecommendationEngineConfiguration(userName, password, adapterId);
                    }
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