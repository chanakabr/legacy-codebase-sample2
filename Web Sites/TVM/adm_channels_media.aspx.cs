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
using System.Collections.Generic;
using KLogMonitor;
using System.Reflection;

public partial class adm_channels_media : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected const string orderByPlaceOrder = "{$@!#}";

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_channels.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            // if this is a KSQL channel, go back
            if (Request.QueryString["type_id"] != null)
            {
                string typeId = Request.QueryString["type_id"];

                // if this is a KSQL channel - return.
                if (typeId == 4.ToString())
                {
                    //Response.Redirect("adm_channels.aspx");
                }
            }

            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(6, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 2, false);
            if (Request.QueryString["search_save"] != null)
                Session["search_save"] = "1";
            else
                Session["search_save"] = null;

            if (Request.QueryString["channel_id"] != null &&
                Request.QueryString["channel_id"].ToString() != "")
            {
                Session["channel_id"] = int.Parse(Request.QueryString["channel_id"].ToString());
            }
            else if (Session["channel_id"] == null || Session["channel_id"].ToString() == "" || Session["channel_id"].ToString() == "0")
            {
                LoginManager.LogoutFromSite("index.html");
                return;
            }
        }
    }

    public void GetCahannelsIFrame()
    {
        string sRet = "<IFRAME SRC=\"admin_tree_player.aspx";
        sRet += "\" WIDTH=\"800px\" HEIGHT=\"300px\" FRAMEBORDER=\"0\"></IFRAME>";
        Response.Write(sRet);
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ":" + 
            PageUtils.GetTableSingleVal("channels", "NAME", int.Parse(Session["channel_id"].ToString())).ToString() + 
            ": Assets List");
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
        //TVinciShared.Channel channel = new TVinciShared.Channel(int.Parse(Session["channel_id"].ToString()), false , 0 , true , 0 , 0);


        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, false, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        int channelType = 0;
        int orderBy = 0;
        int orderDir = 0;
        int channelID = int.Parse(Session["channel_id"].ToString());
        GetChannelBasicData(ref channelType, ref orderBy, ref orderDir, channelID);
        FillTheTableEditor(ref theTable, sOldOrderBy, channelID, channelType, orderBy, orderDir);

        string sCSVFile = theTable.OpenCSV();
        theTable.Finish();
        theTable = null;
        return sCSVFile;
    }

    protected void FillTheTableEditor(ref DBTableWebEditor theTable, string sOrderBy, int channelID, int channelType, int orderBy, int orderDir)
    {
        // 1 = Auto channel 2 = manual channel
        GroupsCacheManager.ChannelType type = (GroupsCacheManager.ChannelType)channelType;
        
        string mediaIds = string.Empty;
        string epgIds = string.Empty;
        int countMedia = 0;
        int countEpg = 0;
        switch (type)
        {
            case GroupsCacheManager.ChannelType.Manual:
                GetAssetIdsFromDB(channelID, out mediaIds, out countMedia);
                break;
            default:
                GetAssetIdsFromCatalog(channelID, out mediaIds, out epgIds, out countMedia, out countEpg);
                break;
        }

        string mediaQuery = string.Empty;
        string epgQuery = string.Empty;

        // If there are no assets at all - simulate like we have 1 media with ID 0,
        // so that the table's query will work and not throw an exception
        if (string.IsNullOrEmpty(mediaIds) && string.IsNullOrEmpty(epgIds))
        {
            mediaIds = "0";
        }

        // Build media query - if we have any media IDs
        if (!string.IsNullOrEmpty(mediaIds))
        {
            #region Media Query

            mediaQuery = "select top " + countMedia + " m.id as id,m.name,m.description,CONVERT(VARCHAR(11),m.CREATE_DATE, 105) as 'Create Date', " +
                "CONVERT(VARCHAR(19),m.START_DATE, 120) as 'Start Date', " +
                "CONVERT(VARCHAR(19),m.End_DATE, 120) as 'End Date' ";

            if (type == GroupsCacheManager.ChannelType.Manual)
            {
                mediaQuery += ",cm.id as cm_id,cm.status,cm.order_num";
            }

            mediaQuery += " from ";
            if (type == GroupsCacheManager.ChannelType.Manual)
            {
                mediaQuery += "channels_media cm,";
            }

            mediaQuery += " media m where ";

            mediaQuery += " m.status=1 and m.id in (" + mediaIds + ")";

            if (type == GroupsCacheManager.ChannelType.Manual)
            {
                mediaQuery += " and m.id = cm.MEDIA_ID and cm.status <> 2";
                mediaQuery += " and cm.channel_id = " + channelID;
            }

            string mediaOrderBy = GetMediaOrderByStr(orderBy);

            if (!string.IsNullOrEmpty(mediaOrderBy))
            {
                if (GetChannelOrderDir(orderDir) == TVinciShared.OrderDir.DESC)
                {
                    mediaOrderBy = mediaOrderBy.Replace(orderByPlaceOrder, "desc");
                }
                else
                {
                    mediaOrderBy = mediaOrderBy.Replace(orderByPlaceOrder, "asc");
                }

                mediaQuery += " order by " + mediaOrderBy;
            }
            else if (type == GroupsCacheManager.ChannelType.Manual)
            {
                mediaQuery += " order by cm.order_num";
            }

            #endregion
        }

        // Build epg query - if we have any epg IDs
        if (!string.IsNullOrEmpty(epgIds))
        {
            #region EPG Query

            epgQuery = "select top " + countEpg + " e.id as id,e.name,e.description,CONVERT(VARCHAR(11),e.CREATE_DATE, 105) as 'Create Date', " +
                "CONVERT(VARCHAR(19),e.START_DATE, 120) as 'Start Date', " +
                "CONVERT(VARCHAR(19),e.End_DATE, 120) as 'End Date' ";

            epgQuery += " from ";

            epgQuery += " epg_channels_schedule e where ";

            epgQuery += " e.id in (" + epgIds + ")";

            string epgOrderBy = GeEpgOrderByStr(orderBy);

            if (!string.IsNullOrEmpty(epgOrderBy))
            {
                epgQuery += " order by " + epgOrderBy;

                if (GetChannelOrderDir(orderDir) == TVinciShared.OrderDir.DESC)
                {
                    epgQuery += " desc";
                }
            }

            #endregion
        }

        // Connect with UNION both queries of media and EPG - according to the need
        if (!string.IsNullOrEmpty(mediaQuery))
        {
            if (!string.IsNullOrEmpty(epgQuery))
            {
                theTable += "select * from (" + mediaQuery + ") a union select * from (" + epgQuery + ") b";
            }
            else
            {
                theTable += mediaQuery;
            }
        }
        else if (!string.IsNullOrEmpty(epgQuery))
        {
            theTable += epgQuery;
        }

        if (LoginManager.IsActionPermittedOnPage("adm_channels.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_media_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("media_id", "field=m_id");
            theTable.AddLinkColumn(linkColumn1);
        }

        if (type == GroupsCacheManager.ChannelType.Manual)
        {
            theTable.AddOrderNumField("channels_media", "cm_id", "order_num", "Order Number");
            theTable.AddHiddenField("order_num");
            theTable.AddHiddenField("cm_id");
            theTable.AddHiddenField("m_id");
            theTable.AddHiddenField("status");

            if (LoginManager.IsActionPermittedOnPage("adm_channels.aspx", LoginManager.PAGE_PERMISION_TYPE.REMOVE))
            {
                DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
                linkColumn.AddQueryStringValue("id", "field=cm_id");
                linkColumn.AddQueryStringValue("table", "channels_media");
                linkColumn.AddQueryStringValue("confirm", "true");
                linkColumn.AddQueryStringValue("main_menu", "6");
                linkColumn.AddQueryStringValue("sub_menu", "2");
                linkColumn.AddQueryStringValue("rep_field", "NAME");
                linkColumn.AddQueryStringValue("rep_name", "ων");
                theTable.AddLinkColumn(linkColumn);
            }

            if (LoginManager.IsActionPermittedOnPage("adm_channels.aspx", LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
            {
                DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
                linkColumn.AddQueryStringValue("id", "field=cm_id");
                linkColumn.AddQueryStringValue("table", "channels_media");
                linkColumn.AddQueryStringValue("confirm", "true");
                linkColumn.AddQueryStringValue("main_menu", "6");
                linkColumn.AddQueryStringValue("sub_menu", "2");
                linkColumn.AddQueryStringValue("rep_field", "NAME");
                linkColumn.AddQueryStringValue("rep_name", "ων");
                theTable.AddLinkColumn(linkColumn);
            }

            if (LoginManager.IsActionPermittedOnPage("adm_channels.aspx", LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
            {
                DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
                linkColumn.AddQueryStringValue("id", "field=cm_id");
                linkColumn.AddQueryStringValue("table", "channels_media");
                linkColumn.AddQueryStringValue("confirm", "false");
                linkColumn.AddQueryStringValue("main_menu", "6");
                linkColumn.AddQueryStringValue("sub_menu", "2");
                linkColumn.AddQueryStringValue("rep_field", "NAME");
                linkColumn.AddQueryStringValue("rep_name", "ων");
                theTable.AddLinkColumn(linkColumn);
            }
        }
    }

    private void GetAssetIdsFromDB(int channelID, out string mediaIds, out int countMedia)
    {
        mediaIds = string.Empty;
        countMedia = 0;
        try
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select MEDIA_ID from channels_media where status <> 2 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CHANNEL_ID", "=", channelID);
            selectQuery.SetCachedSec(0);
            if (selectQuery.Execute("query", true) != null)
            {
                countMedia = selectQuery.Table("query").DefaultView.Count;
                if (countMedia > 0)
                {
                    List<string> medias = new List<string>();
                    foreach (DataRow item in selectQuery.Table("query").Rows)
                    {
                        medias.Add(ODBCWrapper.Utils.GetSafeStr(item, "MEDIA_ID"));
                    }

                    mediaIds = string.Join(",", medias);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        catch (Exception ex)
        {         
   
        }
    }

    private string GetWSURL(string key)
    {
        return TVinciShared.WS_Utils.GetTcmConfigValue(key);
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();

        // get channel_type 
        int channelType = 0;
        int orderBy = 0;
        int orderDir = 0;
        int channelID = int.Parse(Session["channel_id"].ToString());
        GetChannelBasicData(ref channelType, ref orderBy, ref orderDir, channelID);
        bool bAdd = false;
        if (channelType == 2)
            bAdd = true;
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, bAdd, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOrderBy, channelID, channelType, orderBy, orderDir);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);
        Session["ContentPage"] = "adm_channels.aspx?search_save=1";
        theTable.Finish();
        theTable = null;
        return sTable;
    }

    private static void GetChannelBasicData(ref int channelType, ref int orderBy, ref int orderDir, int channelID)
    {
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
        selectQuery += "select channel_type, ORDER_BY_TYPE, ORDER_BY_DIR from channels WITH (nolock) where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", channelID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            try
            {
                channelType = int.Parse(selectQuery.Table("query").DefaultView[0].Row["channel_type"].ToString());
                orderBy = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ORDER_BY_TYPE"].ToString());
                orderDir = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ORDER_BY_DIR"].ToString());
            }
            catch
            {
            }
        }
    }

    #region Query Order

    private string GetMediaOrderByStr(int OrderBy)
    {
        string retVal = string.Empty;
        if (OrderBy == -10)
            retVal = " m.start_date ";
        else if (OrderBy == -11)
            retVal = " m.name ";
        else if (OrderBy == -12)
            retVal = " m.create_date ";
        else if (OrderBy == -9)
            retVal = " m.like_counter ";
        else if (OrderBy == -8)
            retVal = " ((m.VOTES_SUM/( case when m.VOTES_COUNT=0 then 1 else m.VOTES_COUNT end))) {$@!#}, m.VOTES_COUNT {$@!#}";
        else if (OrderBy == -7)
            retVal = " m.VIEWS ";
        //else if (m_nOrderBy == -6)
        //selectQuery += ",newid() ";
        else if (OrderBy == 1)
            retVal = " m.META1_STR ";
        else if (OrderBy == 2)
            retVal = " m.META2_STR ";
        else if (OrderBy == 3)
            retVal = " m.META3_STR ";
        else if (OrderBy == 4)
            retVal = " m.META4_STR ";
        else if (OrderBy == 5)
            retVal = " m.META5_STR ";
        else if (OrderBy == 6)
            retVal = " m.META6_STR ";
        else if (OrderBy == 7)
            retVal = " m.META7_STR ";
        else if (OrderBy == 8)
            retVal = " m.META8_STR ";
        else if (OrderBy == 9)
            retVal = " m.META9_STR ";
        else if (OrderBy == 10)
            retVal = " m.META10_STR ";
        else if (OrderBy == 11)
            retVal = " m.META11_STR ";
        else if (OrderBy == 12)
            retVal = " m.META12_STR ";
        else if (OrderBy == 13)
            retVal = " m.META13_STR ";
        else if (OrderBy == 14)
            retVal = " m.META14_STR ";
        else if (OrderBy == 15)
            retVal = " m.META15_STR ";
        else if (OrderBy == 16)
            retVal = " m.META16_STR ";
        else if (OrderBy == 17)
            retVal = " m.META17_STR ";
        else if (OrderBy == 18)
            retVal = " m.META18_STR ";
        else if (OrderBy == 19)
            retVal = " m.META19_STR ";
        else if (OrderBy == 20)
            retVal = " m.META20_STR ";

        else if (OrderBy == 21)
            retVal = " m.META1_DOUBLE ";
        else if (OrderBy == 22)
            retVal = " m.META2_DOUBLE ";
        else if (OrderBy == 23)
            retVal = " m.META3_DOUBLE ";
        else if (OrderBy == 24)
            retVal = " m.META4_DOUBLE ";
        else if (OrderBy == 25)
            retVal = " m.META5_DOUBLE ";
        else if (OrderBy == 26)
            retVal = " m.META6_DOUBLE ";
        else if (OrderBy == 27)
            retVal = " m.META7_DOUBLE ";
        else if (OrderBy == 28)
            retVal = " m.META8_DOUBLE ";
        else if (OrderBy == 29)
            retVal = " m.META9_DOUBLE ";
        else if (OrderBy == 30)
            retVal = " m.META10_DOUBLE ";

        return retVal;
    }

    private string GeEpgOrderByStr(int orderBy)
    {
        string orderString = string.Empty;

        switch (orderBy)
        {
            case -10:
            {
                orderString = " e.start_date";
                break;
            }
            case -11:
            {
                orderString = " e.name";
                break;
            }
            case -12:
            {
                orderString = " e.create_date";
                break;
            }
            default:
            {
                break;
            }
        }

        return orderString;
    }

    private TVinciShared.OrderDir GetChannelOrderDir(int orderDir)
    {
        TVinciShared.OrderDir retVal = TVinciShared.OrderDir.DESC;
        if (orderDir > 0)
        {
            retVal = (TVinciShared.OrderDir)orderDir;
        }
        return retVal;
    } 
    #endregion

    private string GetMediaIdsFromCatalog(int channelID)
    {
        string mediaIDs = "0";
        try
        {
            int[] assetIds;


            string sIP = "1.1.1.1";
            string sWSUserName = "";
            string sWSPass = "";

            int nParentGroupID = DAL.UtilsDal.GetParentGroupID(LoginManager.GetLoginGroupID());
            TVinciShared.WS_Utils.GetWSUNPass(nParentGroupID, "Channel", "api", sIP, ref sWSUserName, ref sWSPass);
            string sWSURL = GetWSURL("api_ws");
            if (string.IsNullOrEmpty(sWSURL) || string.IsNullOrEmpty(sWSUserName) || string.IsNullOrEmpty(sWSPass))
            {
                return mediaIDs;
            }

            apiWS.API client = new apiWS.API();
            client.Url = sWSURL;

            
            assetIds = client.GetChannelsAssetsIDs(sWSUserName, sWSPass, new int[] { channelID }, null, false, string.Empty, false, false);
            if (assetIds != null && assetIds.Length > 0)
            {
                mediaIDs = string.Join(",", assetIds);
            }
        }
        catch (Exception ex)
        {
            log.Error(string.Empty, ex);
            mediaIDs = "0";
        }

        return mediaIDs;
    }

    private void GetAssetIdsFromCatalog(int channelId, out string media, out string epgs, out int countMedia, out int countEpg)
    {
        media = string.Empty;
        epgs = string.Empty;
        countMedia = 0;
        countEpg = 0;

        try
        {
            int[] assetIds;


            string sIP = "1.1.1.1";
            string sWSUserName = "";
            string sWSPass = "";

            int nParentGroupID = DAL.UtilsDal.GetParentGroupID(LoginManager.GetLoginGroupID());
            TVinciShared.WS_Utils.GetWSUNPass(nParentGroupID, "Channel", "api", sIP, ref sWSUserName, ref sWSPass);
            string sWSURL = GetWSURL("api_ws");

            if (string.IsNullOrEmpty(sWSURL) || string.IsNullOrEmpty(sWSUserName) || string.IsNullOrEmpty(sWSPass))
            {
                return;
            }

            apiWS.API client = new apiWS.API();
            client.Url = sWSURL;

            var response = client.GetChannelAssets(sWSUserName, sWSPass, channelId);

            List<string> mediaIds = new List<string>();
            List<string> epgIds = new List<string>();

            foreach (var item in response)
            {
                switch (item.AssetType)
                {
                    case apiWS.eAssetTypes1.EPG:
                    {
                        epgIds.Add(item.AssetId);
                        break;
                    }
                    case apiWS.eAssetTypes1.MEDIA:
                    {
                        mediaIds.Add(item.AssetId);
                        break;
                    }
                    case apiWS.eAssetTypes1.NPVR:
                    break;
                    case apiWS.eAssetTypes1.UNKNOWN:
                    break;
                    default:
                    break;
                }
            }

            media = string.Join(",", mediaIds);
            epgs = string.Join(",", epgIds);

            countMedia = mediaIds.Count;
            countEpg = epgIds.Count;
        }
        catch (Exception ex)
        {
            log.Error(string.Empty, ex);
        }
    }
}
