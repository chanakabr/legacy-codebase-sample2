using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Reflection;
using System.Web;
using TvinciImporter;
using TVinciShared;

public partial class adm_mail_notifications_adapter_new : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_mail_notifications_adapter.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_mail_notifications_adapter.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
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
                if (Session["mail_notifications_adapter_id"] != null && Session["mail_notifications_adapter_id"].ToString() != "" && int.Parse(Session["mail_notifications_adapter_id"].ToString()) != 0)
                {
                    int.TryParse(Session["mail_notifications_adapter_id"].ToString(), out adapterId);
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
                        adapterId = DBManipulator.DoTheWork("notifications_connection");
                        if (adapterId > 0)
                        {
                            int groupId = LoginManager.GetLoginGroupID();
                            ApiObjects.Response.Status result = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                            result = ImporterImpl.SetMailNotificationsAdapterConfiguration(groupId, adapterId);

                            if (result == null || result.Code != (int)ApiObjects.Response.eResponseStatus.OK)
                            {
                                log.ErrorFormat("Error while SetMailNotificationsAdapterConfiguration. Message: {0}", result.Message);
                            }

                            return;
                        }
                    }
                }
            }

            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);

            if (Request.QueryString["mail_notifications_adapter_id"] != null && Request.QueryString["mail_notifications_adapter_id"].ToString() != "")
            {
                Session["mail_notifications_adapter_id"] = int.Parse(Request.QueryString["mail_notifications_adapter_id"].ToString());
            }
            else if (!flag)
                Session["mail_notifications_adapter_id"] = 0;
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Mail Notifications Adapter");
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
        if (Session["mail_notifications_adapter_id"] != null && Session["mail_notifications_adapter_id"].ToString() != "" && int.Parse(Session["mail_notifications_adapter_id"].ToString()) != 0)
            t = Session["mail_notifications_adapter_id"];
        string sBack = "adm_mail_notifications_adapter.aspx?search_save=1";

        object group_id = LoginManager.GetLoginGroupID();

        DBRecordWebEditor theRecord = new DBRecordWebEditor("mail_notifications_adapters", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("notifications_connection");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_name);

        DataRecordShortTextField dr_external_identifier = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_external_identifier.Initialize("External Identifier", "adm_table_header_nbg", "FormInput", "external_identifier", true);
        theRecord.AddRecord(dr_external_identifier);

        DataRecordShortTextField dr_adapter_url = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_adapter_url.Initialize("Adapter URL", "adm_table_header_nbg", "FormInput", "adapter_url", true);
        theRecord.AddRecord(dr_adapter_url);


        DataRecordLongTextField dr_settings = new DataRecordLongTextField("ltr", true, 60, 30, true);
        dr_settings.Initialize("Settings", "adm_table_header_nbg", "FormInput", "settings", false);
        theRecord.AddRecord(dr_settings);

        DataRecordShortTextField dr_shared_secret = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_shared_secret.Initialize("Shared Secret", "adm_table_header_nbg", "FormInput", "shared_secret", false);
        theRecord.AddRecord(dr_shared_secret);

        //if (t == null)
        //{
        //    string sharedSecret = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);
        //    dr_shared_secret.SetValue(sharedSecret);
        //}

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "group_id", false);
        dr_groups.SetValue(group_id.ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_mail_notifications_adapter_new.aspx?submited=1");

        return sTable;
    }

    static private bool IsExternalIDExists(string extId, int adapterId)
    {
        int groupID = LoginManager.GetLoginGroupID();
        bool res = false;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("notifications_connection");
        selectQuery += "select ID from mail_notifications_adapters where status=1 and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("external_identifier", "=", extId);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "<>", adapterId);
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