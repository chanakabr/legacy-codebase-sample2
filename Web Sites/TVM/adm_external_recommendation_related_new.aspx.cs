using ConfigurationManager;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Web;
using TVinciShared;

public partial class adm_external_recommendation_related_new : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected DataTable allEnrichments;
    
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_external_recommendation_related_new.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_external_recommendation_related_new.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;

        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
            {
                System.Collections.Specialized.NameValueCollection coll = HttpContext.Current.Request.Form;

                if (coll != null && coll.Count > 2 && !string.IsNullOrEmpty(coll["1_val"]))
                {
                    Int32 nID = DBManipulator.DoTheWork();                    
                    Int32 nGroupID = LoginManager.GetLoginGroupID();
                    GroupsCacheManager.GroupManager groupManager = new GroupsCacheManager.GroupManager();
                    groupManager.UpdateGroup(nGroupID);

                    return;
                }
            }

            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": External Recommendaion Engine Related");
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

        string backUrl = "adm_external_recommendation_related_new.aspx?search_save=1";

        object group_id = LoginManager.GetLoginGroupID();

        DBRecordWebEditor theRecord = new DBRecordWebEditor("groups", "adm_table_pager", backUrl, "", "ID", group_id, backUrl, "");

        DataRecordDropDownField dr_recommendation_engine = new DataRecordDropDownField("recommendation_engines", "name", "id", "group_id", group_id, 60, true);
        dr_recommendation_engine.Initialize("Recommendation Engine Provider", "adm_table_header_nbg", "FormInput", "RELATED_RECOMMENDATION_ENGINE", false);
        string recommendationEnginesQuery = "select name as txt,id as id from recommendation_engines where status=1 and group_id= " + group_id.ToString();
        dr_recommendation_engine.SetSelectsQuery(recommendationEnginesQuery);
        theRecord.AddRecord(dr_recommendation_engine);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "group_id", false);
        dr_groups.SetValue(group_id.ToString());
        theRecord.AddRecord(dr_groups);

        string table = theRecord.GetTableHTML("adm_external_recommendation_related_new.aspx?submited=1");

        return table;
    }

    public string GetIPAddress()
    {
        string strHostName = System.Net.Dns.GetHostName();
        System.Net.IPHostEntry ipHostInfo = System.Net.Dns.Resolve(System.Net.Dns.GetHostName());
        System.Net.IPAddress ipAddress = ipHostInfo.AddressList[0];

        return ipAddress.ToString();
    }

    public string changeItemStatus(string id, string action)
    {
        allEnrichments = GetEnrichments();

        int groupId = LoginManager.GetLoginGroupID();

        object channelEnrichmentsObject = PageUtils.GetTableSingleVal("groups", "RELATED_RECOMMENDATION_ENGINE_ENRICHMENTS", groupId);
        long enrichments = 0;

        if (channelEnrichmentsObject != DBNull.Value)
        {
            enrichments = Convert.ToInt64(channelEnrichmentsObject);
        }

        int rowIndex = allEnrichments.DefaultView.Find(id);

        if (rowIndex > -1)
        {
            DataRow row = allEnrichments.DefaultView[rowIndex].Row;

            long value = ODBCWrapper.Utils.ExtractValue<long>(row, "VALUE");

            long alreadyContained = enrichments & value;

            // Always perform XOR, to flip the bit
            enrichments = enrichments ^ value;
        }

        UpdateChannel(groupId, enrichments);

        return "";
    }

    private void UpdateChannel(int groupId, long channelEnrichments)
    {
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("groups");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("RELATED_RECOMMENDATION_ENGINE_ENRICHMENTS", "=", channelEnrichments);
        updateQuery += "where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", groupId);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;

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
            keys.Add(string.Format("{0}_{1}", version, parentGroupId));

            apiWS.API client = new apiWS.API();
            client.Url = url;

            client.UpdateCache(parentGroupId, "CACHE", keys.ToArray());
        }

        return;
    }

    public string changeItemDates(string sID, string sStartDate, string sEndDate)
    {
        return "";
    }

    public string initDualObj()
    {
        Dictionary<string, object> dualList = new Dictionary<string, object>();
        dualList.Add("FirstListTitle", "Current Enrichments");
        dualList.Add("SecondListTitle", "Available Enrichments");


        //int channelId = Convert.ToInt32(Session["channel_id"]);
        long enrichments = GetGroupEnrichments(LoginManager.GetLoginGroupID());

        allEnrichments = GetEnrichments();
        object[] resultData = null;

        if (allEnrichments != null)
        {
            int count = allEnrichments.Rows.Count;
            resultData = new object[count];

            for (int i = 0; i < count; i++)
            {
                DataRow currentEnrichment = allEnrichments.Rows[i];

                string enrichmentId = ODBCWrapper.Utils.ExtractString(currentEnrichment, "ID");
                string title = ODBCWrapper.Utils.ExtractString(currentEnrichment, "NAME");

                bool inList = false;

                long value = ODBCWrapper.Utils.ExtractValue<long>(currentEnrichment, "VALUE");

                // Channel enrichments is a binary digit map - each digit represents another enrichemnt
                // ENRICHMENTS & VALUE:
                // If the result of the mask is 1, the channel has this enrichment. Otherwise it doesn't

                var mask = enrichments & value;

                if (mask > 0)
                {
                    inList = true;
                }

                string description = "";

                var data = new
                {
                    ID = enrichmentId,
                    Title = title,
                    Description = description,
                    InList = inList,
                    //StartDate = startDate,
                    //EndDate = endDate
                };

                resultData[i] = data;
            }
        }

        dualList.Add("Data", resultData);
        dualList.Add("pageName", "adm_external_recommendation_related_new.aspx");
        dualList.Add("withCalendar", false);

        log.Debug("External recommendation engine related - " + resultData.ToJSON());

        return dualList.ToJSON();
    }

    private long GetGroupEnrichments(int groupsId)
    {
        long value = 0;
        DataTable table = null;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

        selectQuery += "select RELATED_RECOMMENDATION_ENGINE_ENRICHMENTS from groups where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", groupsId);

        if (selectQuery.Execute("query", true) != null)
        {
            table = selectQuery.Table("query");

            int count = table.DefaultView.Count;

            if (count > 0)
            {
                value = ODBCWrapper.Utils.ExtractValue<long>(table.Rows[0], "RELATED_RECOMMENDATION_ENGINE_ENRICHMENTS");
            }
        }

        selectQuery.Finish();
        selectQuery = null;

        return value;

    }

    private DataTable GetEnrichments()
    {
        DataTable table = null;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

        selectQuery += "select id, name, value from external_channels_enrichments where is_active = 1";

        if (selectQuery.Execute("query", true) != null)
        {
            table = selectQuery.Table("query");
            table.DefaultView.Sort = "id";
        }

        selectQuery.Finish();
        selectQuery = null;

        return table;
    }

    public void GetGroupId()
    {
        Response.Write(LoginManager.GetLoginGroupID());
    }
}