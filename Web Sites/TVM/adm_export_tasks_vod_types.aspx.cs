using ConfigurationManager;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using TVinciShared;

public partial class adm_export_tasks_vod_types : System.Web.UI.Page
{
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected DataTable mediaTypes;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_export_tasks.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID, "adm_export_tasks.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
            if (Request.QueryString["task_id"] != null &&
                Request.QueryString["task_id"].ToString() != "")
            {
                Session["task_id"] = int.Parse(Request.QueryString["task_id"].ToString());
            }
            else if (Session["task_id"] == null || Session["task_id"].ToString() == "" || Session["task_id"].ToString() == "0")
            {
                LoginManager.LogoutFromSite("index.html");
                return;
            }
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ":" + " : VOD Types ");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    public string GetTableCSV()
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();
        DBTableWebEditor theTable =
            new DBTableWebEditor(true, true, false, "",
                "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOldOrderBy);

        string sCSVFile = theTable.OpenCSV();
        theTable.Finish();
        theTable = null;
        return sCSVFile;
    }

    protected void FillTheTableEditor(ref DBTableWebEditor theTable, string sOrderBy)
    {
    }

    public string changeItemStatus(string mediaTypeId, string action)
    {
        if (Session["task_id"] == null || Session["task_id"].ToString() == "" || Session["task_id"].ToString() == "0")
        {
            LoginManager.LogoutFromSite("index.html");
            return "";
        }

        long taskId = Convert.ToInt32(Session["task_id"]);
        bool isActive = false;

        var currentMediaTypes = GetCurrentMediaTypes(taskId);
        DataRow row = currentMediaTypes.Select(string.Format("MEDIA_TYPE_ID = {0}", mediaTypeId)).FirstOrDefault();
        if (row != null)
        {
            isActive = ODBCWrapper.Utils.GetIntSafeVal(row, "IS_ACTIVE") == 1 ? true : false;
            UpdateVodType(Convert.ToInt32(mediaTypeId), taskId, isActive);
        }
        else
        {
            InsertVodType(Convert.ToInt32(mediaTypeId), taskId);
        }

        UpdateTaskVersion(taskId);
        try
        {
            // insert new message to tasks queue (for celery)
            apiWS.API m = new apiWS.API();
            string sWSURL = ApplicationConfiguration.WebServicesConfiguration.Api.URL.Value;

            if (sWSURL != "")
                m.Url = sWSURL;
            string sWSUserName = "";
            string sWSPass = "";

            TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "EnqueueExportTask", "api", "1.1.1.1", ref sWSUserName, ref sWSPass);

            m.EnqueueExportTask(sWSUserName, sWSPass, taskId);
        }
        catch (Exception ex)
        {
            log.Error("Error - " + string.Format("EnqueueExportTask in ws_api failed, taskId = {0}, ex = {1}", taskId, ex), ex);
        }


        return "";
    }

    private void UpdateTaskVersion(long taskId)
    {
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("bulk_export_tasks");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("version", "=", ODBCWrapper.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow));
        updateQuery += " where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", taskId);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;

        return;
    }

    private void InsertVodType(int mediaTypeId, long taskID)
    {
        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("bulk_export_tasks_media_types");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("media_type_id", "=", mediaTypeId);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("task_id", "=", taskID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("updater_id", "=", LoginManager.GetLoginID());
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", LoginManager.GetLoginGroupID());
        insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;

        return;
    }

    private void UpdateVodType(int mediaTypeId, long taskID, bool isActive)
    {
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("bulk_export_tasks_media_types");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", isActive ? 0 : 1);
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", LoginManager.GetLoginGroupID());
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("updater_id", "=", LoginManager.GetLoginID());
        updateQuery += " where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("media_type_id", "=", mediaTypeId);
        updateQuery += " and ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("task_id", "=", taskID);
        updateQuery += " and ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;

        return;
    }

    public string initDualObj()
    {
        if (Session["task_id"] == null || Session["task_id"].ToString() == "" || Session["task_id"].ToString() == "0")
        {
            LoginManager.LogoutFromSite("index.html");
            return "";
        }

        Dictionary<string, object> dualList = new Dictionary<string, object>();
        dualList.Add("FirstListTitle", "Current VOD Types");
        dualList.Add("SecondListTitle", "Available VOD Types");


        long taskId = Convert.ToInt64(Session["task_id"]);

        mediaTypes = GetMediaTypes();
        var currentMediaTypes = GetCurrentMediaTypes(taskId);

        object[] resultData = null;

        if (mediaTypes != null)
        {
            int count = mediaTypes.Rows.Count;
            resultData = new object[count];

            for (int i = 0; i < count; i++)
            {
                DataRow mediaType = mediaTypes.Rows[i];

                string mediaTypeId = ODBCWrapper.Utils.ExtractString(mediaType, "ID");
                string title = ODBCWrapper.Utils.ExtractString(mediaType, "NAME");
                string description = ODBCWrapper.Utils.ExtractString(mediaType, "DESCRIPTION");

                bool inList = false;

                var row = currentMediaTypes.Select(string.Format("MEDIA_TYPE_ID = {0}", mediaTypeId)).FirstOrDefault();
                if (row != null)
                {
                    inList = ODBCWrapper.Utils.GetIntSafeVal(row, "IS_ACTIVE") == 1 ? true : false;
                }

                var data = new
                {
                    ID = mediaTypeId,
                    Title = title,
                    Description = description,
                    InList = inList,
                };

                resultData[i] = data;
            }
        }

        dualList.Add("Data", resultData);
        dualList.Add("pageName", "adm_export_tasks_vod_types.aspx");
        dualList.Add("withCalendar", false);

        return dualList.ToJSON();
    }

    private DataTable GetCurrentMediaTypes(long taskId)
    {
        DataTable table = null;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

        selectQuery += "select id,task_id,media_type_id,is_active from bulk_export_tasks_media_types where [status] = 1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("task_id", "=", taskId);
        selectQuery += " and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", LoginManager.GetLoginGroupID());

        if (selectQuery.Execute("query", true) != null)
        {
            table = selectQuery.Table("query");
        }

        selectQuery.Finish();
        selectQuery = null;

        return table;
    }

    private DataTable GetMediaTypes()
    {
        DataTable table = null;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

        selectQuery += "select id,name,[description],group_id from media_types where [status] = 1 and group_id " + TVinciShared.PageUtils.GetAllGroupTreeStr(LoginManager.GetLoginGroupID());
        if (selectQuery.Execute("query", true) != null)
        {
            table = selectQuery.Table("query");
        }

        selectQuery.Finish();
        selectQuery = null;

        return table;
    }

    public string GetTaskId()
    {
        return Convert.ToString(Session["task_id"]);
    }
}
