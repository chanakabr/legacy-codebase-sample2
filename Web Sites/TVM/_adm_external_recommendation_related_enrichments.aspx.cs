using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;
using System.Configuration;
using System.Globalization;
using System.Data;
using KLogMonitor;
using System.Reflection;

public partial class adm_external_recommendation_related_enrichments : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected DataTable allEnrichments;

    static protected string GetWSURL()
    {
        return TVinciShared.WS_Utils.GetTcmConfigValue("pricing_ws");
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_external_recommendation_related_new.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID, "adm_external_recommendation_related_new.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);

            //Int32 ownerGroupId = int.Parse(PageUtils.GetTableSingleVal("external_channels", "group_id", int.Parse(Session["channel_id"].ToString())).ToString());
            Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();

            if (PageUtils.IsTvinciUser() == false)
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }            
        }
    }

    public void GetHeader()
    {
        string response = PageUtils.GetPreHeader() + ":";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("CONNECTION_STRING");
        selectQuery += "select group_name from groups where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", LoginManager.GetLoginGroupID());

        if (selectQuery.Execute("query", true) != null)
        {
            Int32 count = selectQuery.Table("query").DefaultView.Count;

            if (count > 0)
            {
                response += selectQuery.Table("query").DefaultView[0].Row["group_name"].ToString();
            }
        }

        selectQuery.Finish();
        selectQuery = null;
        Response.Write(response + " : Enrichments ");
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
        string url = TVinciShared.WS_Utils.GetTcmConfigValue("api_ws");
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
        dualList.Add("pageName", "adm_external_recommendation_related_enrichments.aspx");
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
