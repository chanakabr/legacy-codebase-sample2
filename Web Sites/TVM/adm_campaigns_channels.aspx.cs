using System;
using TVinciShared;

public partial class adm_campaigns_channels : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_media.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID, "adm_campaigns.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
            if (Request.QueryString["campaign_id"] != null &&
                Request.QueryString["campaign_id"].ToString() != "")
            {
                Session["campaign_id"] = int.Parse(Request.QueryString["campaign_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("campaigns", "group_id", int.Parse(Session["campaign_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else if (Session["campaign_id"] == null || Session["campaign_id"].ToString() == "" || Session["campaign_id"].ToString() == "0")
            {
                LoginManager.LogoutFromSite("index.html");
                return;
            }
        }
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ":";
        Response.Write(sRet + " Campaign (" + Session["campaign_id"].ToString() + ") Channels");
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
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, false, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOldOrderBy);

        string sCSVFile = theTable.OpenCSV();
        theTable.Finish();
        theTable = null;
        return sCSVFile;
    }

    protected void FillTheTableEditor(ref DBTableWebEditor theTable, string sOrderBy)
    {
    }

    protected void InsertCampaignChannelID(Int32 nChannelID, Int32 nCampaignID, Int32 nGroupID)
    {
        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("campaigns_channels");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("campaign_id", "=", nCampaignID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CHANNEL_ID", "=", nChannelID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
        insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;
    }

    protected void UpdateCampaignChannelID(Int32 nID, Int32 nStatus)
    {
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("campaigns_channels");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", nStatus);
        updateQuery += "where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nID);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;
    }

    protected Int32 GetCampaignChannelID(Int32 nChannelID, Int32 nLogedInGroupID, ref Int32 nStatus)
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select id,status from campaigns_channels where is_active=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("campaign_id", "=", int.Parse(Session["campaign_id"].ToString()));
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CHANNEL_ID", "=", nChannelID);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nLogedInGroupID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                nStatus = int.Parse(selectQuery.Table("query").DefaultView[0].Row["STATUS"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return nRet;
    }

    public string changeItemStatus(string sID, string sAction)
    {
        if (Session["campaign_id"] == null || Session["campaign_id"].ToString() == "" || Session["campaign_id"].ToString() == "0")
        {
            LoginManager.LogoutFromSite("index.html");
            return "";
        }

        Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("campaigns", "group_id", int.Parse(Session["campaign_id"].ToString())).ToString());
        Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
        if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
        {
            LoginManager.LogoutFromSite("login.html");
            return "";
        }
        Int32 nStatus = 0;
        Int32 nCampaignChannelID = GetCampaignChannelID(int.Parse(sID), nLogedInGroupID, ref nStatus);
        if (nCampaignChannelID != 0)
        {
            if (nStatus == 0)
                UpdateCampaignChannelID(nCampaignChannelID, 1);
            else
                UpdateCampaignChannelID(nCampaignChannelID, 0);
        }
        else
        {
            InsertCampaignChannelID(int.Parse(sID), int.Parse(Session["campaign_id"].ToString()), nLogedInGroupID);
        }

        

        return "";
    }

    public string initDualObj()
    {
        if (Session["campaign_id"] == null || Session["campaign_id"].ToString() == "" || Session["campaign_id"].ToString() == "0")
        {
            LoginManager.LogoutFromSite("index.html");
            return "";
        }

        Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("campaigns", "group_id", int.Parse(Session["campaign_id"].ToString())).ToString());
        Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
        if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
        {
            LoginManager.LogoutFromSite("login.html");
            return "";
        }

        string sRet = "";
        sRet += "Channels included in campaign";
        sRet += "~~|~~";
        sRet += "Available Channels";
        sRet += "~~|~~";
        sRet += "<root>";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from channels where is_active=1 and status=1 and channel_type<>3 and watcher_id=0 and ";
        Int32 nCommerceGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", LoginManager.GetLoginGroupID()).ToString());
        if (nCommerceGroupID == 0)
            nCommerceGroupID = nLogedInGroupID;
        selectQuery += "group_id " + PageUtils.GetFullChildGroupsStr(nCommerceGroupID, "");
        //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", LoginManager.GetLoginGroupID());
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string sID = selectQuery.Table("query").DefaultView[i].Row["ID"].ToString();
                string sGroupID = selectQuery.Table("query").DefaultView[i].Row["group_ID"].ToString();
                string sTitle = "";
                if (selectQuery.Table("query").DefaultView[i].Row["ADMIN_NAME"] != null &&
                    selectQuery.Table("query").DefaultView[i].Row["ADMIN_NAME"] != DBNull.Value)
                    sTitle = selectQuery.Table("query").DefaultView[i].Row["ADMIN_NAME"].ToString();

                string sGroupName = ODBCWrapper.Utils.GetTableSingleVal("groups", "GROUP_NAME", int.Parse(sGroupID)).ToString();
                sTitle += "(" + sGroupName.ToString() + ")";

                string sDescription = "";
                /*
                if (selectQuery.Table("query").DefaultView[i].Row["DESCRIPTION"] != null &&
                    selectQuery.Table("query").DefaultView[i].Row["DESCRIPTION"] != DBNull.Value)
                    sDescription = selectQuery.Table("query").DefaultView[i].Row["DESCRIPTION"].ToString();
                 */
                if (IsChannelBelong(int.Parse(sID)) == true)
                    sRet += "<item id=\"" + sID + "\"  title=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(sTitle, true) + "\" description=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(sDescription, true) + "\" inList=\"true\" />";
                else
                    sRet += "<item id=\"" + sID + "\"  title=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(sTitle, true) + "\" description=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(sDescription, true) + "\" inList=\"false\" />";
            }
        }
        selectQuery.Finish();
        selectQuery = null;


        sRet += "</root>";
        return sRet;
    }

    protected bool IsChannelBelong(Int32 nChannelID)
    {
        try
        {
            bool bRet = false;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from campaigns_channels where is_active=1 and status=1 and ";
            Int32 nCommerceGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", LoginManager.GetLoginGroupID()).ToString());
            if (nCommerceGroupID == 0)
                nCommerceGroupID = LoginManager.GetLoginGroupID();
            selectQuery += " group_id " + PageUtils.GetFullChildGroupsStr(nCommerceGroupID, "");
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("campaign_id", "=", int.Parse(Session["campaign_id"].ToString()));
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CHANNEL_ID", "=", nChannelID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    bRet = true;
            }
            selectQuery.Finish();
            selectQuery = null;
            return bRet;
        }
        catch
        {
            return false;
        }
    }
}