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

public partial class adm_external_channels_enrichments : System.Web.UI.Page
{
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
        if (LoginManager.IsPagePermitted("adm_external_channels.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID, "adm_external_channels.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
            if (Request.QueryString["channel_id"] != null &&
                Request.QueryString["channel_id"].ToString() != "")
            {
                Session["channel_id"] = int.Parse(Request.QueryString["channel_id"].ToString());
                Int32 ownerGroupId = int.Parse(PageUtils.GetTableSingleVal("external_channels", "group_id", int.Parse(Session["channel_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();

                if (nLogedInGroupID != ownerGroupId && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else if (Session["channel_id"] == null || Session["channel_id"].ToString() == "" || Session["channel_id"].ToString() == "0")
            {
                LoginManager.LogoutFromSite("index.html");
                return;
            }
        }
    }

    public void GetHeader()
    {
        string response = PageUtils.GetPreHeader() + ":";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("CONNECTION_STRING");
        selectQuery += "select name from external_channels where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", int.Parse(Session["channel_id"].ToString()));

        if (selectQuery.Execute("query", true) != null)
        {
            Int32 count = selectQuery.Table("query").DefaultView.Count;

            if (count > 0)
            {
                response += selectQuery.Table("query").DefaultView[0].Row["name"].ToString();
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
        if (Session["channel_id"] == null || Session["channel_id"].ToString() == "" || Session["channel_id"].ToString() == "0")
        {
            LoginManager.LogoutFromSite("index.html");
            return "";
        }

        allEnrichments = GetEnrichments();

        int channelId = Convert.ToInt32(Session["channel_id"]);

        object channelEnrichmentsObject = PageUtils.GetTableSingleVal("external_channels", "enrichments", channelId);
        long channelEnrichments = 0;
        
        if (channelEnrichmentsObject != DBNull.Value)
        {
            channelEnrichments = Convert.ToInt64(channelEnrichmentsObject);
        }

        int rowIndex = allEnrichments.DefaultView.Find(id);

        if (rowIndex > -1)
        {
            DataRow row = allEnrichments.DefaultView[rowIndex].Row;

            long value = ODBCWrapper.Utils.ExtractValue<long>(row, "VALUE");

            long alreadyContained = channelEnrichments & value;

            // Always perform XOR, to flip the bit
            channelEnrichments = channelEnrichments ^ value;
        }

        UpdateChannel(channelId, channelEnrichments);

        return "";
    }

    private void UpdateChannel(int channelId, long channelEnrichments)
    {
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("external_channels");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("enrichments", "=", channelEnrichments);
        updateQuery += "where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", channelId);
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
            keys.Add(string.Format("{0}_external_channel_{1}_{2}", version, parentGroupId, channelId));

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
        if (Session["channel_id"] == null || Session["channel_id"].ToString() == "" || Session["channel_id"].ToString() == "0")
        {
            LoginManager.LogoutFromSite("index.html");
            return "";
        }


        Dictionary<string, object> dualList = new Dictionary<string, object>();
        dualList.Add("FirstListTitle", "Current Enrichments");
        dualList.Add("SecondListTitle", "Available Enrichments");


        int channelId = Convert.ToInt32(Session["channel_id"]);
        long channelEnrichments = GetChannelEnrichments(channelId);

        allEnrichments = GetEnrichments();
        object[] resultData = null;

        if (allEnrichments != null)
        {
            int count = allEnrichments.Rows.Count;
            //Logger.Logger.Log("Pricing WS", "Count is " + count.ToString(), "PricingWS");
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

                var mask = channelEnrichments & value;

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
        dualList.Add("pageName", "adm_external_channels_enrichments.aspx");
        dualList.Add("withCalendar", false);

        Logger.Logger.Log("External Channels", resultData.ToJSON(), "External Channels");

        return dualList.ToJSON();
    }

    private long GetChannelEnrichments(int channelId)
    {
        long value = 0;
        DataTable table = null;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

        selectQuery += "select enrichments from external_channels where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", channelId);

        if (selectQuery.Execute("query", true) != null)
        {
            table = selectQuery.Table("query");

            int count = table.DefaultView.Count;

            if (count > 0)
            {
                value = ODBCWrapper.Utils.ExtractValue<long>(table.Rows[0], "enrichments");
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

    public void GetExternalChannelId()
    {
        Response.Write(Session["channel_id"].ToString());
    }
}
