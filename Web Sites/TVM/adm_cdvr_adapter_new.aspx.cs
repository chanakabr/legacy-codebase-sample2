using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using KLogMonitor;
using TVinciShared;
using ca_ws;

public partial class adm_cdvr_adapter_new : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_cdvr_adapter.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_cdvr_adapter.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            bool flag = false;

            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
            {
                int adapterId = 0;
                // Validate uniqe external id
                if (Session["cdvr_adapter_id"] != null && Session["cdvr_adapter_id"].ToString() != "" && int.Parse(Session["cdvr_adapter_id"].ToString()) != 0)
                {
                    int.TryParse(Session["cdvr_adapter_id"].ToString(), out adapterId);
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
                        Int32 nID = DBManipulator.DoTheWork("CA_CONNECTION_STRING");
                        if (nID > 0)
                        {
                            // set adapter configuration 
                            string sIP = "1.1.1.1";
                            string sWSUserName = "";
                            string sWSPass = "";
                            TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "", "conditionalaccess", sIP, ref sWSUserName, ref sWSPass);
                            ca_ws.module cas = new ca_ws.module();
                            string sWSURL = GetWSURL("conditionalaccess_ws");
                            if (sWSURL != "")
                                cas.Url = sWSURL;
                            try
                            {
                                CDVRAdapterResponse status = cas.SendCDVRAdapterConfiguration(sWSUserName, sWSPass, nID);
                                log.Debug("SetOSSAdapterConfiguration - " + string.Format("oss adapter id:{0}, status:{1}", nID, status.Status != null ? status.Status.Code: 1));

                                // remove adapter from cache
                                string version = TVinciShared.WS_Utils.GetTcmConfigValue("Version");
                                string[] keys = new string[1] 
                                { 
                                    string.Format("{0}_cdvr_adapter_{1}", version, nID)
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
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);

            if (Request.QueryString["cdvr_adapter_id"] != null && Request.QueryString["cdvr_adapter_id"].ToString() != "")
            {
                Session["cdvr_adapter_id"] = int.Parse(Request.QueryString["cdvr_adapter_id"].ToString());
            }
            else if (!flag)
                Session["cdvr_adapter_id"] = 0;
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": CDVR Adapter");
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
        if (Session["cdvr_adapter_id"] != null && Session["cdvr_adapter_id"].ToString() != "" && int.Parse(Session["cdvr_adapter_id"].ToString()) != 0)
            t = Session["cdvr_adapter_id"];
        string sBack = "adm_cdvr_adapter.aspx?search_save=1";

        object group_id = LoginManager.GetLoginGroupID();

        DBRecordWebEditor theRecord = new DBRecordWebEditor("cdvr_adapters", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("CA_CONNECTION_STRING");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_name);

        DataRecordShortTextField dr_external_identifier = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_external_identifier.Initialize("External Identifier", "adm_table_header_nbg", "FormInput", "external_identifier", true);
        theRecord.AddRecord(dr_external_identifier);

        DataRecordShortTextField dr_adapter_url = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_adapter_url.Initialize("Adapter URL", "adm_table_header_nbg", "FormInput", "adapter_url", true);
        theRecord.AddRecord(dr_adapter_url);

        DataRecordShortTextField dr_shared_secret = new DataRecordShortTextField("ltr", false, 60, 128);
        dr_shared_secret.Initialize("Shared Secret", "adm_table_header_nbg", "FormInput", "shared_secret", false);
        theRecord.AddRecord(dr_shared_secret);

        DataRecordCheckBoxField dr_dynamicLinksSupport = new DataRecordCheckBoxField(true);
        dr_dynamicLinksSupport.Initialize("Dynamic Links Support", "adm_table_header_nbg", "FormInput", "dynamic_links_support", true);
        theRecord.AddRecord(dr_dynamicLinksSupport);

        if (t == null)
        {
            string sharedSecret = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);
            dr_shared_secret.SetValue(sharedSecret);
        }

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "group_id", false);
        dr_groups.SetValue(group_id.ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_cdvr_adapter_new.aspx?submited=1");

        return sTable;
    }

    static public string GetWSURL(string sKey)
    {
        return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
    }

    static private bool IsExternalIDExists(string extId, int pgid)
    {
        int groupID = LoginManager.GetLoginGroupID();
        bool res = false;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("CA_CONNECTION_STRING");
        selectQuery += "select ID from cdvr_adapters where status=1 and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("external_identifier", "=", extId);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "<>", pgid);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                res = true;
                int pgeid = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "ID", 0);
                string pgname = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "NAME", 0);
                log.Debug("IsExternalIDExists - " + string.Format("id:{0}, name:{1}", pgeid, pgname));
            }
        }
        selectQuery.Finish();
        selectQuery = null;

        return res;
    }
}