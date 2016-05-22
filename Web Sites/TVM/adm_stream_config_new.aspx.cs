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
using KLogMonitor;
using System.Reflection;
using apiWS;

public partial class adm_stream_config_new : System.Web.UI.Page
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
        else if (PageUtils.IsTvinciUser() == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
            {
                int adapterId = 0;
                // Validate unique alias
                if (Session["stream_company_id"] != null && Session["stream_company_id"].ToString() != "" && int.Parse(Session["stream_company_id"].ToString()) != 0)
                {
                    int.TryParse(Session["stream_company_id"].ToString(), out adapterId);
                }

                System.Collections.Specialized.NameValueCollection coll = HttpContext.Current.Request.Form;
                if (coll != null && coll.Count > 2 && !string.IsNullOrEmpty(coll["1_val"]))
                {
                    if (IsAliasExists(coll["1_val"], adapterId))
                    {
                        Session["error_msg"] = "System Name must be unique";
                    }
                    else
                    {
                        Int32 nID = DBManipulator.DoTheWork();
                        if (nID > 0)
                        {
                            // set adapter configuration 
                            string sIP = "1.1.1.1";
                            string sWSUserName = "";
                            string sWSPass = "";
                            TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "", "api", sIP, ref sWSUserName, ref sWSPass);
                            apiWS.API api = new apiWS.API();
                            string sWSURL = WS_Utils.GetTcmConfigValue("api_ws");
                            if (sWSURL != "")
                                api.Url = sWSURL;
                            try
                            {
                                CDNAdapterResponse configurationStatus = api.SendCDNAdapterConfiguration(sWSUserName, sWSPass, nID);
                                log.DebugFormat("SendCDNAdapterConfiguration, cdn adapter id:{0}, status:{1}", nID, configurationStatus.Status != null ? configurationStatus.Status.Code : 1);

                                // remove adapter from cache
                                string version = TVinciShared.WS_Utils.GetTcmConfigValue("Version");
                                string[] keys = new string[1] 
                                { 
                                    string.Format("{0}_cdn_adapter_{1}", version, nID)
                                };

                                QueueUtils.UpdateCache(LoginManager.GetLoginGroupID(), CouchbaseManager.eCouchbaseBucket.CACHE.ToString(), keys);
                            }
                            catch (Exception ex)
                            {
                                log.Error("Exception - " + string.Format("cdvr adapter id :{0}, ex msg:{1}, ex st: {2} ", nID, ex.Message, ex.StackTrace), ex);
                            }
                        }
                        return;
                    }
                }
            }

            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(4, true, ref nMenuID);
            if (Request.QueryString["stream_company_id"] != null &&
                Request.QueryString["stream_company_id"].ToString() != "")
            {
                Session["stream_company_id"] = int.Parse(Request.QueryString["stream_company_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("streaming_companies", "group_id", int.Parse(Session["stream_company_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["stream_company_id"] = 0;

            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
        }
    }

    public void GetHeader()
    {
        if (Session["stream_company_id"] != null && Session["stream_company_id"].ToString() != "" && int.Parse(Session["stream_company_id"].ToString()) != 0)
            Response.Write(PageUtils.GetPreHeader() + ":Streaming companies - Edit");
        else
            Response.Write(PageUtils.GetPreHeader() + ":Streaming companies - New");
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
        if (Session["stream_company_id"] != null && Session["stream_company_id"].ToString() != "" && int.Parse(Session["stream_company_id"].ToString()) != 0)
            t = Session["stream_company_id"];
        DBRecordWebEditor theRecord = new DBRecordWebEditor("streaming_companies", "adm_table_pager", "adm_stream_config.aspx", "", "ID", t, "adm_stream_config.aspx", "stream_company_id");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "STREAMING_COMPANY_NAME", true);
        theRecord.AddRecord(dr_name);

        DataRecordShortTextField dr_alias = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_alias.Initialize("SystemName", "adm_table_header_nbg", "FormInput", "ALIAS", true);
        theRecord.AddRecord(dr_alias);

        DataRecordShortTextField dr_adapter_url = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_adapter_url.Initialize("Adapter URL", "adm_table_header_nbg", "FormInput", "ADAPTER_URL", false);
        theRecord.AddRecord(dr_adapter_url);

        DataRecordShortTextField dr_shared_secret = new DataRecordShortTextField("ltr", false, 60, 128);
        dr_shared_secret.Initialize("Shared Secret", "adm_table_header_nbg", "FormInput", "shared_secret", false);
        theRecord.AddRecord(dr_shared_secret);

        if (t == null)
        {
            string sharedSecret = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);
            dr_shared_secret.SetValue(sharedSecret);
        }

        DataRecordShortTextField dr_base_video_url = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_base_video_url.Initialize("Base video URL", "adm_table_header_nbg", "FormInput", "VIDEO_BASE_URL", false);
        theRecord.AddRecord(dr_base_video_url);

        DataRecordDropDownField dr_action_code = new DataRecordDropDownField("lu_cdn_type", "DESCRIPTION_VAL", "DESCRIPTION", "", null, 60, true);
        dr_action_code.Initialize("Action Type", "adm_table_header_nbg", "FormInput", "CDN_STR_ID", false);
        dr_action_code.SetFieldType("string");        
        theRecord.AddRecord(dr_action_code);

        DataRecordShortIntField dr_cdn_ttl = new DataRecordShortIntField(true, 9, 9);
        dr_cdn_ttl.Initialize("Token TTL (in seconds)", "adm_table_header_nbg", "FormInput", "TTL", false);
        theRecord.AddRecord(dr_cdn_ttl);

        DataRecordShortTextField dr_cdn_salt = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_cdn_salt.Initialize("Salt", "adm_table_header_nbg", "FormInput", "SALT", false);
        theRecord.AddRecord(dr_cdn_salt);

        DataRecordDropDownField dr_tokenize = new DataRecordDropDownField("lu_token_impl", "FRIENDLY_NAME", "id", "", null, 60, true);
        dr_tokenize.Initialize("Token Provider", "adm_table_header_nbg", "FormInput", "TOKENIZE_IMPL_ID", false);
        theRecord.AddRecord(dr_tokenize);

        DataRecordCheckBoxField dr_url_type = new DataRecordCheckBoxField(true);
        dr_url_type.Initialize("Url Type Dynamic", "adm_table_header_nbg", "FormInput", "url_type", false);
        theRecord.AddRecord(dr_url_type);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_stream_config_new.aspx?submited=1");

        return sTable;
    }

    private bool IsAliasExists(string alias, int id)
    {
        int groupID = LoginManager.GetLoginGroupID();
        bool res = false;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select ID from streaming_companies where status=1 and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ALIAS", "=", alias);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "!=", id);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                res = true;
                int existingAdapterId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "ID", 0);
                string existingAdapterName = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "STREAMING_COMPANY_NAME", 0);
                log.DebugFormat("SystemName already exists for adaptr with id: {0}, name: {1}", existingAdapterId, existingAdapterName);
            }
        }
        selectQuery.Finish();
        selectQuery = null;

        return res;
    }
}
