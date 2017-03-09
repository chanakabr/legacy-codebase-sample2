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
using TvinciImporter;
using ApiObjects;

public partial class adm_time_shifted_tv_settings : System.Web.UI.Page
{

    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        else if (LoginManager.IsPagePermitted("adm_time_shifted_tv_settings.aspx") == false)
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
                // get catchup valuefrom db to this group_id 
                int groupId = LoginManager.GetLoginGroupID();
                apiWS.TimeShiftedTvPartnerSettings tstvOld = GetTimeShiftedTVSettings(groupId);

                int id = DBManipulator.DoTheWork("MAIN_CONNECTION_STRING");
                if (id > 0)
                {
                    apiWS.TimeShiftedTvPartnerSettings tstvNew = GetTimeShiftedTVSettings(groupId);
                    if ((tstvOld == null && tstvNew != null) || tstvOld.IsCatchUpEnabled != tstvNew.IsCatchUpEnabled || tstvOld.CatchUpBufferLength != tstvNew.CatchUpBufferLength)
                    {
                        // call api service
                        apiWS.API api = new apiWS.API();
                        string sWSURL = TVinciShared.WS_Utils.GetTcmConfigValue("api_ws");

                        if (sWSURL != "")
                            api.Url = sWSURL;
                        string sWSUserName = "";
                        string sWSPass = "";
                        TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "UpdateTimeShiftedTvEpgChannelsSettings", "api", "1.1.1.1", ref sWSUserName, ref sWSPass);

                        apiWS.Status status = api.UpdateTimeShiftedTvEpgChannelsSettings(sWSUserName, sWSPass, tstvNew);
                    }
                }
            }
        }
    }

    private apiWS.TimeShiftedTvPartnerSettings GetTimeShiftedTVSettings(int groupId)
    {
        apiWS.TimeShiftedTvPartnerSettings tstv = null;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select top (1) enable_catch_up, catch_up_buffer from dbo.time_shifted_tv_settings WITH(NOLOCK)  where is_active = 1 and status = 1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", groupId);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                tstv = new apiWS.TimeShiftedTvPartnerSettings()
                {
                    IsCatchUpEnabled = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").Rows[0], "enable_catch_up") == 1 ? true : false,
                    CatchUpBufferLength = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").Rows[0], "catch_up_buffer")
                };
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return tstv;
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Time Shifted TV Settings");
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

        // check if to insert a new record to the table or update an existing one
        int idFromTable = DAL.TvmDAL.GetTimeShiftedTVSettingsID(groupID);
 
        if (idFromTable > 0)
        {
            fieldIndexValue = idFromTable;
        }

        string sBack = "adm_time_shifted_tv_settings.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("time_shifted_tv_settings", "adm_table_pager", sBack, "", "ID", fieldIndexValue, sBack, "");
        theRecord.SetConnectionKey("MAIN_CONNECTION_STRING");

        DataRecordCheckBoxField dr_catchUp = new DataRecordCheckBoxField(true);
        dr_catchUp.Initialize("Enable Catch-Up", "adm_table_header_nbg", "FormInput", "enable_catch_up", false);
        dr_catchUp.SetDefault(0);
        theRecord.AddRecord(dr_catchUp);

        DataRecordShortIntField dr_catchUpBuffer = new DataRecordShortIntField(true, 9, 9, 0);
        dr_catchUpBuffer.Initialize("Catch-Up Buffer Length", "adm_table_header_nbg", "FormInput", "catch_up_buffer", false);
        dr_catchUpBuffer.SetDefault(7);
        theRecord.AddRecord(dr_catchUpBuffer);

        DataRecordCheckBoxField dr_cdvr = new DataRecordCheckBoxField(true);
        dr_cdvr.Initialize("Enable C-DVR", "adm_table_header_nbg", "FormInput", "enable_cdvr", false);
        dr_cdvr.SetDefault(0);
        theRecord.AddRecord(dr_cdvr);

        DataRecordCheckBoxField dr_startOver = new DataRecordCheckBoxField(true);
        dr_startOver.Initialize("Enable Start-Over", "adm_table_header_nbg", "FormInput", "enable_start_over", false);
        dr_startOver.SetDefault(0);
        theRecord.AddRecord(dr_startOver);

        DataRecordCheckBoxField dr_trickPlay = new DataRecordCheckBoxField(true);
        dr_trickPlay.Initialize("Enable Live Trick-Play ", "adm_table_header_nbg", "FormInput", "enable_trick_play", false);
        dr_trickPlay.SetDefault(0);
        theRecord.AddRecord(dr_trickPlay);

        DataRecordShortIntField dr_trickPlayBuffer = new DataRecordShortIntField(true, 9, 9, 0);
        dr_trickPlayBuffer.Initialize("Live Trick-Play Buffer Length", "adm_table_header_nbg", "FormInput", "trick_play_buffer", false);
        dr_trickPlay.SetDefault(1);
        theRecord.AddRecord(dr_trickPlayBuffer);

        DataRecordCheckBoxField dr_scheduleWindow = new DataRecordCheckBoxField(true);
        dr_scheduleWindow.Initialize("Enable Recording Schedule Window ", "adm_table_header_nbg", "FormInput", "enable_recording_schedule_window", false);
        theRecord.AddRecord(dr_scheduleWindow);

        DataRecordShortIntField dr_scheduleWindowBuffer = new DataRecordShortIntField(true, 9, 9);
        dr_scheduleWindowBuffer.Initialize("Recording Schedule Window Length", "adm_table_header_nbg", "FormInput", "recording_schedule_window_buffer", false);
        dr_scheduleWindowBuffer.SetDefault(0);
        theRecord.AddRecord(dr_scheduleWindowBuffer);

        DataRecordShortIntField dr_paddingBeforeProgramStarts = new DataRecordShortIntField(true, 9, 9, 0);
        dr_paddingBeforeProgramStarts.Initialize("Padding Before Program Stars", "adm_table_header_nbg", "FormInput", "padding_before_program_starts", false);
        dr_paddingBeforeProgramStarts.SetDefault(0);
        theRecord.AddRecord(dr_paddingBeforeProgramStarts);

        DataRecordShortIntField dr_paddingAfterProgramEnds = new DataRecordShortIntField(true, 9, 9, 0);
        dr_paddingAfterProgramEnds.Initialize("Padding After Program Ends", "adm_table_header_nbg", "FormInput", "padding_after_program_ends", false);
        dr_paddingBeforeProgramStarts.SetDefault(0);
        theRecord.AddRecord(dr_paddingAfterProgramEnds);

        DataRecordCheckBoxField dr_protection = new DataRecordCheckBoxField(true);
        dr_protection.Initialize("Enable Protection", "adm_table_header_nbg", "FormInput", "enable_protection", false);
        theRecord.AddRecord(dr_protection);

        DataRecordShortIntField dr_protectionPeriod = new DataRecordShortIntField(true, 9, 9, 1);
        dr_protectionPeriod.Initialize("Record Protection Period", "adm_table_header_nbg", "FormInput", "protection_period", false);
        dr_protectionPeriod.SetDefault(90);
        theRecord.AddRecord(dr_protectionPeriod);

        DataRecordShortIntField dr_protectionQuotaPercentage = new DataRecordShortIntField(true, 9, 9, 10, 100);
        dr_protectionQuotaPercentage.Initialize("Record Protection Quota Percentage", "adm_table_header_nbg", "FormInput", "protection_quota_percentage", false);
        dr_protectionQuotaPercentage.SetDefault(25);
        theRecord.AddRecord(dr_protectionQuotaPercentage);

        DataRecordShortIntField dr_recordingLifetimePeriod = new DataRecordShortIntField(true, 9, 9, 1);
        dr_recordingLifetimePeriod.Initialize("Recording Lifetime Period", "adm_table_header_nbg", "FormInput", "recording_lifetime_period", false);
        dr_recordingLifetimePeriod.SetDefault(182);
        theRecord.AddRecord(dr_recordingLifetimePeriod);

        DataRecordShortIntField dr_cleanupNoticePeriod = new DataRecordShortIntField(true, 9, 9, 1);
        dr_cleanupNoticePeriod.Initialize("Cleanup Notice Period", "adm_table_header_nbg", "FormInput", "cleanup_notice_period", false);
        dr_cleanupNoticePeriod.SetDefault(7);
        theRecord.AddRecord(dr_cleanupNoticePeriod);

        DataRecordDropDownField dr_adapters = new DataRecordDropDownField("time_shifted_tv_settings", "adapter_id", "id", "", null, 60, true);
        string sQuery = "select name as txt,id as id from conditionalAccess..cdvr_adapters where status=1 and is_active=1 and group_id=" + groupID;
        dr_adapters.SetSelectsQuery(sQuery);
        dr_adapters.Initialize("C-DVR Adapter", "adm_table_header_nbg", "FormInput", "adapter_id", false);
        dr_adapters.SetDefaultVal("---");
        theRecord.AddRecord(dr_adapters);

        DataRecordDropDownField dr_quota = new DataRecordDropDownField("time_shifted_tv_settings", "quota_module_id", "id", "", null, 60, true);
        sQuery = "select name as txt,id as id from quota_modules where status=1 and is_active=1 and group_id=" + groupID;
        dr_quota.SetSelectsQuery(sQuery);
        dr_quota.Initialize("Quota Management", "adm_table_header_nbg", "FormInput", "quota_module_id", false);
        dr_quota.SetDefaultVal("---");
        theRecord.AddRecord(dr_quota);

        DataRecordCheckBoxField dr_seriesRecording = new DataRecordCheckBoxField(true);
        dr_seriesRecording.Initialize("Enable Series Recording ", "adm_table_header_nbg", "FormInput", "enable_series_recording", false);
        dr_seriesRecording.SetDefault(1);
        theRecord.AddRecord(dr_seriesRecording);

        DataRecordCheckBoxField dr_enableRecordingPlaybackNonEntitled  = new DataRecordCheckBoxField(true);
        dr_enableRecordingPlaybackNonEntitled.Initialize("Enable Recording Playback (for non-entitled channel) ", "adm_table_header_nbg", "FormInput", "enable_recording_playback_non_entitled", false);
        dr_enableRecordingPlaybackNonEntitled.SetDefault(0);
        theRecord.AddRecord(dr_enableRecordingPlaybackNonEntitled);

        DataRecordCheckBoxField dr_enableRecordingPlaybackNonExisting = new DataRecordCheckBoxField(true);
        dr_enableRecordingPlaybackNonExisting.Initialize("Enable Recording Playback (for non-existing  channel) ", "adm_table_header_nbg", "FormInput", "enable_recording_playback_non_existing", false);
        dr_enableRecordingPlaybackNonExisting.SetDefault(0);
        theRecord.AddRecord(dr_enableRecordingPlaybackNonExisting);

        //lu_quota_overage_policy
        DataRecordDropDownField dr_quota_overage_policy = new DataRecordDropDownField("time_shifted_tv_settings", "quota_overage_policy", "id", "", null, 60, true);
        sQuery = "select description as txt, id as id from lu_quota_overage_policy";
        dr_quota_overage_policy.SetSelectsQuery(sQuery);
        dr_quota_overage_policy.Initialize("Quota Overage Policy", "adm_table_header_nbg", "FormInput", "quota_overage_policy", false);
       // dr_quota_overage_policy.SetDefaultVal("---");
        theRecord.AddRecord(dr_quota_overage_policy);


        //lu_protection_policy
        DataRecordDropDownField dr_protection_policy = new DataRecordDropDownField("time_shifted_tv_settings", "protection_policy", "id", "", null, 60, true);
        sQuery = "select description as txt, id as id from lu_protection_policy";
        dr_protection_policy.SetSelectsQuery(sQuery);
        dr_protection_policy.Initialize("Protection Policy", "adm_table_header_nbg", "FormInput", "protection_policy", false);
        // dr_quota_overage_policy.SetDefaultVal("---");
        theRecord.AddRecord(dr_protection_policy);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_time_shifted_tv_settings.aspx?submited=1");

        return sTable;
    }
    
}