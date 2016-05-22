using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Reflection;
using KLogMonitor;
using TVinciShared;
using System.Data;

public partial class adm_cdn_settings : System.Web.UI.Page
{

    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        else if (LoginManager.IsPagePermitted("adm_cdn_settings.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork("MAIN_CONNECTION_STRING");
            }
        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": CDN Settings");
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        object fieldIndexValue = null;

        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }

        int groupID = LoginManager.GetLoginGroupID();

        int cdnSettingsId = 0;
        try
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select ID from cdn_settings where status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    cdnSettingsId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row, "ID");
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        catch (Exception)
        {
            cdnSettingsId = 0;
        }

        if (cdnSettingsId > 0)
        {
            fieldIndexValue = cdnSettingsId;
        }

        string sBack = "adm_cdn_settings.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("cdn_settings", "adm_table_pager", sBack, "", "ID", fieldIndexValue, sBack, "");
        theRecord.SetConnectionKey("MAIN_CONNECTION_STRING");

        string groups = TVinciShared.PageUtils.GetAllGroupTreeStr(groupID);

        DataRecordDropDownField dr_vodAdapters = new DataRecordDropDownField("cdn_settings", "vod_adapter_id", "id", "", null, 60, true);
        string sQuery = "select STREAMING_COMPANY_NAME as txt,id as id from streaming_companies where status=1 and ADAPTER_URL is not null and adapter_url <> '' and is_active=1 and group_id " + groups;
        dr_vodAdapters.SetSelectsQuery(sQuery);
        dr_vodAdapters.Initialize("VOD CDN default adapter", "adm_table_header_nbg", "FormInput", "vod_adapter_id", false);
        dr_vodAdapters.SetDefaultVal("---");
        theRecord.AddRecord(dr_vodAdapters);

        DataRecordDropDownField dr_epgAdapters = new DataRecordDropDownField("cdn_settings", "epg_adapter_id", "id", "", null, 60, true);
        dr_epgAdapters.SetSelectsQuery(sQuery);
        dr_epgAdapters.Initialize("Live & catch-up CDN default adapter", "adm_table_header_nbg", "FormInput", "epg_adapter_id", false);
        dr_epgAdapters.SetDefaultVal("---");
        theRecord.AddRecord(dr_epgAdapters);

        DataRecordDropDownField dr_recordingAdapters = new DataRecordDropDownField("cdn_settings", "recording_adapter_id", "id", "", null, 60, true);
        dr_recordingAdapters.SetSelectsQuery(sQuery);
        dr_recordingAdapters.Initialize("Recordings CDN default adapter", "adm_table_header_nbg", "FormInput", "recording_adapter_id", false);
        dr_recordingAdapters.SetDefaultVal("---");
        theRecord.AddRecord(dr_recordingAdapters);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_cdn_settings.aspx?submited=1");

        return sTable;
    }
    
}