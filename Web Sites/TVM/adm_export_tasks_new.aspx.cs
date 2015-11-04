using ApiObjects.QueueObjects;
using QueueWrapper.Queues.QueueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;
using log4net;
using System.Reflection;

public partial class adm_export_tasks_new : System.Web.UI.Page
{
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_export_tasks.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_export_tasks.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            bool flag = false;

            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
            {
                int taskid = 0;
                // Validate uniqe external id
                if (Session["export_task_id"] != null && Session["export_task_id"].ToString() != "" && int.Parse(Session["export_task_id"].ToString()) != 0)
                {
                    int.TryParse(Session["export_task_id"].ToString(), out taskid);
                }

                int frequencyMinVal = TVinciShared.WS_Utils.GetTcmIntValue("export_frequency_min_value");
                
                System.Collections.Specialized.NameValueCollection coll = HttpContext.Current.Request.Form;
                long frequency;
                if (coll != null && coll.Count > 2 && !string.IsNullOrEmpty(coll["1_val"]) && !string.IsNullOrEmpty(coll["5_val"]))
                {
                    if (IsExternalKeyExists(coll["1_val"], taskid))
                    {
                        Session["error_msg"] = "External Id must be unique";
                        flag = true;
                    }
                    else if (!long.TryParse(coll["5_val"], out frequency))
                    {
                        Session["error_msg"] = "Frequency must be a number";
                        flag = true;
                    }
                    else if (frequency < frequencyMinVal)
                    {
                        Session["error_msg"] = "Frequency must be at least " + frequencyMinVal;
                        flag = true;
                    }
                    else
                    {
                        Int32 nID = DBManipulator.DoTheWork();
                        if (nID != 0)
                        {
                            try
                            {
                                // insert new message to tasks queue (for celery)
                                apiWS.API m = new apiWS.API();
                                string sWSURL = TVinciShared.WS_Utils.GetTcmConfigValue("api_ws");

                                if (sWSURL != "")
                                    m.Url = sWSURL;
                                string sWSUserName = "";
                                string sWSPass = "";

                                TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "EnqueueExportTask", "api", "1.1.1.1", ref sWSUserName, ref sWSPass);

                                m.EnqueueExportTask(sWSUserName, sWSPass, nID);
                            }
                            catch (Exception ex)
                            {
                                log.Error("Error - " + string.Format("EnqueueExportTask in ws_api failed, taskId = {0}, ex = {1}", taskid, ex), ex);
                            }
                        }
                    }
                }
            }

            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);

            if (Request.QueryString["export_task_id"] != null && Request.QueryString["export_task_id"].ToString() != "")
            {
                Session["export_task_id"] = int.Parse(Request.QueryString["export_task_id"].ToString());
            }
            else if (!flag)
                Session["export_task_id"] = 0;
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": External Task");
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
        if (Session["export_task_id"] != null && Session["export_task_id"].ToString() != "" && int.Parse(Session["export_task_id"].ToString()) != 0)
            t = Session["export_task_id"];
        string sBack = "adm_export_tasks.aspx?search_save=1";

        object group_id = LoginManager.GetLoginGroupID();

        DBRecordWebEditor theRecord = new DBRecordWebEditor("bulk_export_tasks", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "name", true);
        theRecord.AddRecord(dr_name);

        DataRecordShortTextField dr_external_key = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_external_key.Initialize("External Key", "adm_table_header_nbg", "FormInput", "external_key", true);
        theRecord.AddRecord(dr_external_key);

        DataRecordDropDownField dr_data_type = new DataRecordDropDownField("lu_bulk_export_data_types", "NAME", "id", "", null, 60, false);
        var query = "select description as txt, id as id from lu_bulk_export_data_types";
        dr_data_type.SetSelectsQuery(query);
        dr_data_type.Initialize("Data Type", "adm_table_header_nbg", "FormInput", "data_type", false);
        dr_data_type.SetDefaultVal(query);
        theRecord.AddRecord(dr_data_type);

        DataRecordShortTextField dr_filter = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_filter.Initialize("Filter", "adm_table_header_nbg", "FormInput", "filter", false);
        theRecord.AddRecord(dr_filter);


        DataRecordDropDownField dr_export_type = new DataRecordDropDownField("lu_bulk_export_export_types", "NAME", "id", "", null, 60, false);
        query = "select description as txt, id as id from lu_bulk_export_export_types";
        dr_export_type.SetSelectsQuery(query);
        dr_export_type.Initialize("Export Type", "adm_table_header_nbg", "FormInput", "export_type", false);
        dr_export_type.SetDefaultVal(query);
        theRecord.AddRecord(dr_export_type);

        DataRecordShortTextField dr_frequency = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_frequency.Initialize("Frequency (minutes)", "adm_table_header_nbg", "FormInput", "frequency", true);
        theRecord.AddRecord(dr_frequency);

        DataRecordShortTextField dr_notificationUrl = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_notificationUrl.Initialize("Notification URL", "adm_table_header_nbg", "FormInput", "notification_url", true);
        theRecord.AddRecord(dr_notificationUrl);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "group_id", false);
        dr_groups.SetValue(group_id.ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_version = new DataRecordShortIntField(false, 9, 9);
        dr_version.Initialize("Version", "adm_table_header_nbg", "FormInput", "version", false);
        dr_version.SetValue(ODBCWrapper.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow).ToString());
        theRecord.AddRecord(dr_version);

        string sTable = theRecord.GetTableHTML("adm_export_tasks_new.aspx?submited=1");

        return sTable;
    }

    static public string GetWSURL(string sKey)
    {
        return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
    }

    static private bool IsExternalKeyExists(string externalKey, int taskId)
    {
        int groupID = LoginManager.GetLoginGroupID();
        bool res = false;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select ID from bulk_export_tasks where status=1 and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("external_key", "=", externalKey);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "<>", taskId);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                res = true;
                int taskid = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "ID", 0);
                log.Debug("IsExternalKeyExists - " + string.Format("id:{0}", taskid));
            }
        }
        selectQuery.Finish();
        selectQuery = null;

        return res;
    }
}