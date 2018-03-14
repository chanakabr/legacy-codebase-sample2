using ConfigurationManager;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using TVinciShared;

public partial class adm_recommendation_engine_adapter_new : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_recommendation_engine_adapter.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_recommendation_engine_adapter.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;

        if (!IsPostBack)
        {
            bool flag = false;

            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
            {
                int adapterId = 0;

                // Validate unique external id
                if (Session["adapter_id"] != null && Session["adapter_id"].ToString() != "" && int.Parse(Session["adapter_id"].ToString()) != 0)
                {
                    int.TryParse(Session["adapter_id"].ToString(), out adapterId);
                }

                System.Collections.Specialized.NameValueCollection coll = HttpContext.Current.Request.Form;

                if (coll != null && coll.Count > 2 && !string.IsNullOrEmpty(coll["1_val"]))
                {
                    if (IsExternalIDExists(coll["1_val"], adapterId))
                    {
                        Session["error_msg"] = "External Id must be unique";
                        flag = true;
                    }
                    else
                    {
                        Int32 nID = DBManipulator.DoTheWork();

                        // After save is done:
                        // Update cache (Recommendation Engines are saved in cache as well as in DB)
                        // Update adapter itself that configuration has changed

                        string ip = "1.1.1.1";
                        string userName = "";
                        string password = "";

                        int parentGroupId = DAL.UtilsDal.GetParentGroupID(LoginManager.GetLoginGroupID());
                        TVinciShared.WS_Utils.GetWSUNPass(parentGroupId, "UpdateCache", "api", ip, ref userName, ref password);
                        string url = ApplicationConfiguration.WebServicesConfiguration.Api.URL.Value;
                        string version = ApplicationConfiguration.Version.Value;

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

                        return;
                    }
                }
            }

            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);

            if (Request.QueryString["adapter_id"] != null && Request.QueryString["adapter_id"].ToString() != "")
            {
                Session["adapter_id"] = int.Parse(Request.QueryString["adapter_id"].ToString());
            }
            else if (!flag)
                Session["adapter_id"] = 0;
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Recommendation Engine");
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

        object adapterId = null;

        if (Session["adapter_id"] != null && Session["adapter_id"].ToString() != "" && int.Parse(Session["adapter_id"].ToString()) != 0)
        {
            adapterId = Session["adapter_id"];
        }

        string backUrl = "adm_recommendation_engine_adapter.aspx?search_save=1";

        object group_id = LoginManager.GetLoginGroupID();

        DBRecordWebEditor theRecord = new DBRecordWebEditor("recommendation_engines", "adm_table_pager", backUrl, "", "ID", adapterId, backUrl, "");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_name);

        DataRecordShortTextField dr_external_identifier = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_external_identifier.Initialize("External Identifier", "adm_table_header_nbg", "FormInput", "external_identifier", true);
        theRecord.AddRecord(dr_external_identifier);

        DataRecordShortTextField dr_adapter_url = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_adapter_url.Initialize("Adapter URL", "adm_table_header_nbg", "FormInput", "adapter_url", true);
        theRecord.AddRecord(dr_adapter_url);

        DataRecordShortTextField dr_shared_secret = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_shared_secret.Initialize("Shared Secret", "adm_table_header_nbg", "FormInput", "shared_secret", true);
        theRecord.AddRecord(dr_shared_secret);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "group_id", false);
        dr_groups.SetValue(group_id.ToString());
        theRecord.AddRecord(dr_groups);

        string table = theRecord.GetTableHTML("adm_recommendation_engine_adapter_new.aspx?submited=1");

        return table;
    }

    static private bool IsExternalIDExists(string extId, int adapterId)
    {
        int groupID = LoginManager.GetLoginGroupID();
        bool result = false;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("main_connection_string");
        selectQuery += "select ID from recommendation_engines where status=1 and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("external_identifier", "=", extId);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "<>", adapterId);

        if (selectQuery.Execute("query", true) != null)
        {
            int count = selectQuery.Table("query").DefaultView.Count;

            if (count > 0)
            {
                result = true;
                int newAdapterId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "ID", 0);
                string name = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "NAME", 0);
                log.Debug("IsExternalIDExists - " + string.Format("id:{0}, name:{1}", newAdapterId, name));
            }
        }

        selectQuery.Finish();
        selectQuery = null;

        return result;
    }
}