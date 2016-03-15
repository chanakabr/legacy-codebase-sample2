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
using Notifiers;
using System.Collections.Generic;
using TvinciImporter;
using KLogMonitor;
using System.Reflection;
public partial class adm_media_files_new : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsPagePermitted("adm_media.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsActionPermittedOnPage("adm_media.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID, "adm_media.aspx");
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DateTime? prevStartDate = null;
                DateTime? prevEndDate = null;                

                // get current media file end date
                if (Session["media_file_id"] != null)
                {
                    DataRow mediaFileDetails = ODBCWrapper.Utils.GetTableSingleRowColumnsByParamValue("media_files", "id", Session["media_file_id"].ToString(), new List<string>() { "start_date", "end_date" });
                    prevStartDate = ODBCWrapper.Utils.GetNullableDateSafeVal(mediaFileDetails, "start_date");
                    prevEndDate = ODBCWrapper.Utils.GetNullableDateSafeVal(mediaFileDetails, "end_date");
                }
                
                Int32 nMediaFileID = DBManipulator.DoTheWork();
                if (nMediaFileID > 0)
                {
                    // get mediaID and updated end date (if changed)
                    DataRow updatedMediaFileDetails = ODBCWrapper.Utils.GetTableSingleRowColumnsByParamValue("media_files", "id", nMediaFileID.ToString(), new List<string>() { "media_id", "start_date", "end_date" });
                    int nMediaID = ODBCWrapper.Utils.GetIntSafeVal(updatedMediaFileDetails, "media_id");
                    DateTime? updatedStartDate = ODBCWrapper.Utils.GetNullableDateSafeVal(updatedMediaFileDetails, "start_date");
                    DateTime? updatedEndDate = ODBCWrapper.Utils.GetNullableDateSafeVal(updatedMediaFileDetails, "end_date");                    
                    int nLoginGroupId = LoginManager.GetLoginGroupID();

                    if (nMediaID > 0)
                    {
                        if (!ImporterImpl.UpdateIndex(new List<int>() { nMediaID }, nLoginGroupId, ApiObjects.eAction.Update))
                        {
                            log.Error(string.Format("Failed updating index for mediaID: {0}, groupID: {1}", nMediaID, nLoginGroupId));
                        }

                        // check if changes in the start date require future index update call, incase updatedStartDate is in more than 2 years we don't update the index (per Ira's request)
                        if (RabbitHelper.IsFutureIndexUpdate(prevStartDate, updatedStartDate))
                        {
                            if (!RabbitHelper.InsertFreeItemsIndexUpdate(nLoginGroupId, ApiObjects.eObjectType.Media, new List<int>() { nMediaID }, updatedStartDate.Value))
                            {
                                log.Error(string.Format("Failed inserting free items index update for startDate: {0}, mediaID: {1}, groupID: {2}", updatedStartDate.Value, nMediaID, nLoginGroupId));
                            }
                        }

                        // check if changes in the end date require future index update call, incase updatedEndDate is in more than 2 years we don't update the index (per Ira's request)
                        if (RabbitHelper.IsFutureIndexUpdate(prevEndDate, updatedEndDate))
                        {
                            if (!RabbitHelper.InsertFreeItemsIndexUpdate(nLoginGroupId, ApiObjects.eObjectType.Media, new List<int>() { nMediaID }, updatedEndDate.Value))
                            {
                                log.Error(string.Format("Failed inserting free items index update for endDate: {0}, mediaID: {1}, groupID: {2}", updatedEndDate.Value, nMediaID, nLoginGroupId));
                            }
                        }
                    }

                    try
                    {
                        Notifiers.BaseMediaNotifier t = null;
                        Notifiers.Utils.GetBaseMediaNotifierImpl(ref t, LoginManager.GetLoginGroupID());

                        string errorMessage = "";

                        if (t != null)
                        {
                            t.NotifyChange(nMediaID.ToString(), ref errorMessage);
                        }

                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            HttpContext.Current.Session["error_msg_sub"] = "Error in Package ID " + nMediaID + ":\r\n" + errorMessage;
                        }

                        return;
                    }
                    catch (Exception ex)
                    {
                        log.Error("exception - " + nMediaID.ToString() + " : " + ex.Message, ex);
                    }
                }

                return;
            }

            if (Session["media_id"] == null || Session["media_id"].ToString() == "" || Session["media_id"].ToString() == "0")
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }

            if (Request.QueryString["media_file_id"] != null && Request.QueryString["media_file_id"].ToString() != "")
            {
                Session["media_file_id"] = int.Parse(Request.QueryString["media_file_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("media", "group_id", int.Parse(Session["media_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["media_file_id"] = "0";
        }
    }

    public void GetHeader()
    {
        if (Session["media_file_id"] != null && Session["media_file_id"].ToString() != "" && int.Parse(Session["media_file_id"].ToString()) != 0)
            Response.Write(PageUtils.GetPreHeader() + ":Media File - Edit");
        else
            Response.Write(PageUtils.GetPreHeader() + ":Media File - New");
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
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        Int32 nBackUPEnabled = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "CDN_BACKUP_ACTIVE", nGroupID).ToString());
        bool bBackUP = false;
        if (nBackUPEnabled == 1)
            bBackUP = true;
        Int32 nAds = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "IS_ADS", nGroupID).ToString());
        bool bAds = false;
        if (nAds == 1)
            bAds = true;
        object t = null; ;
        if (Session["media_file_id"] != null && Session["media_file_id"].ToString() != "" && int.Parse(Session["media_file_id"].ToString()) != 0)
            t = Session["media_file_id"];
        string sBack = "adm_media_files.aspx?search_save=1&media_id=" + Session["media_id"].ToString();
        DBRecordWebEditor theRecord = new DBRecordWebEditor("media_files", "adm_table_pager", sBack, "", "ID", t, sBack, "media_id");

        DataRecordDropDownField dr_media_type = new DataRecordDropDownField("groups_media_type", "description", "media_type_id", "group_id", nGroupID, 60, false);
        dr_media_type.Initialize("Media Type", "adm_table_header_nbg", "FormInput", "MEDIA_TYPE_ID", false);
        theRecord.AddRecord(dr_media_type);

        //add start/end date per file 
        DataRecordDateTimeField dr_start_date = new DataRecordDateTimeField(true);
        dr_start_date.Initialize("Start Date", "adm_table_header_nbg", "FormInput", "START_DATE", false);
        dr_start_date.SetDefault(DateTime.Now);
        theRecord.AddRecord(dr_start_date);

        DataRecordDateTimeField dr_end_date = new DataRecordDateTimeField(true);
        dr_end_date.Initialize("End Date", "adm_table_header_nbg", "FormInput", "END_DATE", false);
        theRecord.AddRecord(dr_end_date);


        DataRecordDropDownField dr_media_quality = new DataRecordDropDownField("lu_media_quality", "description", "id", "", null, 60, false);
        dr_media_quality.Initialize("Media Quality", "adm_table_header_nbg", "FormInput", "MEDIA_QUALITY_ID", false);
        dr_media_quality.SetOrderBy("id desc");
        theRecord.AddRecord(dr_media_quality);

        DataRecordDropDownField dr_billing_type = new DataRecordDropDownField("lu_billing_type", "description", "id", "", null, 60, false);
        dr_billing_type.Initialize("Billing Type", "adm_table_header_nbg", "FormInput", "BILLING_TYPE_ID", false);
        dr_billing_type.SetOrderBy("id");
        theRecord.AddRecord(dr_billing_type);

        Int32 nCommerceGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", nGroupID).ToString());
        if (nCommerceGroupID == 0)
            nCommerceGroupID = nGroupID;

        DataRecordDropDownField dr_finance = new DataRecordDropDownField("fr_financial_weights", "NAME", "ID", "group_id", nCommerceGroupID, 60, false);
        dr_finance.Initialize("Financial Weight", "adm_table_header_nbg", "FormInput", "FINANCIAL_WEIGHT_ID", false);
        theRecord.AddRecord(dr_finance);

        DataRecordDropDownField dr_override_player_type = new DataRecordDropDownField("lu_player_descriptions", "description", "id", "", null, 60, true);
        dr_override_player_type.SetNoSelectStr("---Default Media Type Player---");
        dr_override_player_type.Initialize("Streaming Type", "adm_table_header_nbg", "FormInput", "OVERRIDE_PLAYER_TYPE_ID", false);
        theRecord.AddRecord(dr_override_player_type);

        DataRecordLongTextField dr_conf = new DataRecordLongTextField("ltr", true, 60, 4);
        dr_conf.Initialize("Configuration data", "adm_table_header_nbg", "FormInput", "ADDITIONAL_DATA", false);
        theRecord.AddRecord(dr_conf);

        DataRecordRadioField dr_stram_suplier = new DataRecordRadioField("streaming_companies", "STREAMING_COMPANY_NAME", "id", "status", 1);
        string sQuery = "select STREAMING_COMPANY_NAME as txt,id as id from streaming_companies where status=1 and group_id= " + LoginManager.GetLoginGroupID().ToString();
        dr_stram_suplier.SetSelectsQuery(sQuery);
        dr_stram_suplier.Initialize("CDN", "adm_table_header_nbg", "FormInput", "STREAMING_SUPLIER_ID", false);
        dr_stram_suplier.SetDefault(0);
        theRecord.AddRecord(dr_stram_suplier);

        DataRecordOnePicBrowserField dr_pic = new DataRecordOnePicBrowserField();
        dr_pic.Initialize("File pic(for file medias only)", "adm_table_header_nbg", "FormInput", "REF_ID", false);
        theRecord.AddRecord(dr_pic);

        DataRecordShortIntField dr_branding_hight = new DataRecordShortIntField(true, 3, 3);
        dr_branding_hight.Initialize("Branding height(pixels)", "adm_table_header_nbg", "FormInput", "BRAND_HEIGHT", false);
        theRecord.AddRecord(dr_branding_hight);

        DataRecordRadioField dr_recurring = new DataRecordRadioField("lu_recurring_type", "description", "id", "", null);
        dr_recurring.Initialize("Branding Recurring type", "adm_table_header_nbg", "FormInput", "RECURRING_TYPE_ID", true);
        dr_recurring.SetDefault(0);
        theRecord.AddRecord(dr_recurring);

        DataRecordLongTextField dr_streaming_code = new DataRecordLongTextField("ltr", true, 60, 3);
        dr_streaming_code.Initialize("CDN Code", "adm_table_header_nbg", "FormInput", "STREAMING_CODE", false);
        theRecord.AddRecord(dr_streaming_code);
        if (bAds == true)
        {
            DataRecordShortIntField dr_max_session_views = new DataRecordShortIntField(true, 3, 3);
            dr_max_session_views.Initialize("Max session views(0 for Unlimited)", "adm_table_header_nbg", "FormInput", "MAX_SESSION_VIEWS", false);
            theRecord.AddRecord(dr_max_session_views);

            DataRecordShortIntField dr_max_views = new DataRecordShortIntField(true, 3, 3);
            dr_max_views.Initialize("Max total views(0 for Unlimited)", "adm_table_header_nbg", "FormInput", "MAX_VIEWS", false);
            theRecord.AddRecord(dr_max_views);
        }

        DataRecordMediaViewerField dr_viewer = new DataRecordMediaViewerField("", int.Parse(Session["media_file_id"].ToString()));
        dr_viewer.Initialize("Main Video", "adm_table_header_nbg", "FormInput", "STREAMING_CODE", false);
        theRecord.AddRecord(dr_viewer);


        if (bBackUP == true)
        {
            DataRecordRadioField dr_alt_stram_suplier = new DataRecordRadioField("streaming_companies", "STREAMING_COMPANY_NAME", "id", "status", 1);
            sQuery = "select STREAMING_COMPANY_NAME as txt,id as id from streaming_companies where status=1 and group_id= " + LoginManager.GetLoginGroupID().ToString();
            dr_alt_stram_suplier.SetSelectsQuery(sQuery);
            dr_alt_stram_suplier.Initialize("BackUp CDN", "adm_table_header_nbg", "FormInput", "ALT_STREAMING_SUPLIER_ID", true);
            dr_alt_stram_suplier.SetDefault(0);
            theRecord.AddRecord(dr_alt_stram_suplier);

            DataRecordLongTextField dr_alt_streaming_code = new DataRecordLongTextField("ltr", true, 60, 3);
            dr_alt_streaming_code.Initialize("BackUp CDN Code", "adm_table_header_nbg", "FormInput", "alt_STREAMING_CODE", true);
            theRecord.AddRecord(dr_alt_streaming_code);

            DataRecordMediaViewerField dr_alt_viewer = new DataRecordMediaViewerField("alt_", int.Parse(Session["media_file_id"].ToString()));
            dr_alt_viewer.Initialize("BackUp Video", "adm_table_header_nbg", "FormInput", "alt_STREAMING_CODE", false);
            theRecord.AddRecord(dr_alt_viewer);
        }

        DataRecordCheckBoxField dr_pre_skip = new DataRecordCheckBoxField(true);
        dr_pre_skip.Initialize("Pre skip enabled", "adm_table_header_nbg", "FormInput", "OUTER_COMMERCIAL_SKIP_PRE", false);
        theRecord.AddRecord(dr_pre_skip);

        DataRecordDropDownField dr_type_pre = new DataRecordDropDownField("ads_companies", "ADS_COMPANY_NAME", "id", "group_id", nGroupID, 60, true);
        dr_type_pre.SetNoSelectStr("---");
        dr_type_pre.Initialize("Pre Provider", "adm_table_header_nbg", "FormInput", "COMMERCIAL_TYPE_PRE_ID", false);
        theRecord.AddRecord(dr_type_pre);

        DataRecordDropDownField dr_type_poverlay = new DataRecordDropDownField("ads_companies", "ADS_COMPANY_NAME", "id", "group_id", nGroupID, 60, true);
        dr_type_poverlay.Initialize("Overlay Provider", "adm_table_header_nbg", "FormInput", "COMMERCIAL_TYPE_OVERLAY_ID", false);
        dr_type_poverlay.SetNoSelectStr("---");
        theRecord.AddRecord(dr_type_poverlay);

        DataRecordShortTextField dr_overlay_points = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_overlay_points.Initialize("Overlay points (secs) ; seperated", "adm_table_header_nbg", "FormInput", "COMMERCIAL_OVERLAY_POINTS", false);
        theRecord.AddRecord(dr_overlay_points);

        DataRecordDropDownField dr_type_break = new DataRecordDropDownField("ads_companies", "ADS_COMPANY_NAME", "id", "group_id", nGroupID, 60, true);
        dr_type_break.Initialize("Break Provider", "adm_table_header_nbg", "FormInput", "COMMERCIAL_TYPE_BREAK_ID", false);
        dr_type_break.SetNoSelectStr("---");
        theRecord.AddRecord(dr_type_break);

        DataRecordShortTextField dr_break_points = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_break_points.Initialize("Break points (secs) ; seperated", "adm_table_header_nbg", "FormInput", "COMMERCIAL_BREAK_POINTS", false);
        theRecord.AddRecord(dr_break_points);

        DataRecordCheckBoxField dr_post_skip = new DataRecordCheckBoxField(true);
        dr_post_skip.Initialize("Post skip enabled", "adm_table_header_nbg", "FormInput", "OUTER_COMMERCIAL_SKIP_POST", false);
        theRecord.AddRecord(dr_post_skip);

        DataRecordDropDownField dr_type_post = new DataRecordDropDownField("ads_companies", "ADS_COMPANY_NAME", "id", "group_id", nGroupID, 60, true);
        dr_type_post.Initialize("Post Provider", "adm_table_header_nbg", "FormInput", "COMMERCIAL_TYPE_POST_ID", false);
        dr_type_post.SetNoSelectStr("---");
        theRecord.AddRecord(dr_type_post);

        DataRecordShortIntField dr_order_num = new DataRecordShortIntField(true, 3, 3);
        dr_order_num.Initialize("Order number", "adm_table_header_nbg", "FormInput", "ORDER_NUM", false);
        theRecord.AddRecord(dr_order_num);

        DataRecordLongTextField dr_remarks = new DataRecordLongTextField("ltr", true, 60, 4);
        dr_remarks.Initialize("Remarks", "adm_table_header_nbg", "FormInput", "EDITOR_REMARKS", false);
        theRecord.AddRecord(dr_remarks);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_media_id = new DataRecordShortIntField(false, 9, 9);
        dr_media_id.Initialize("Media ID", "adm_table_header_nbg", "FormInput", "MEDIA_ID", false);
        dr_media_id.SetValue(Session["media_id"].ToString());
        theRecord.AddRecord(dr_media_id);

        DataRecordShortTextField dr_Product_Code = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_Product_Code.Initialize("Product Code", "adm_table_header_nbg", "FormInput", "Product_Code", false);
        theRecord.AddRecord(dr_Product_Code);

        DataRecordShortTextField dr_CO_GUID = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_CO_GUID.Initialize("CO_GUID", "adm_table_header_nbg", "FormInput", "co_guid", false);
        theRecord.AddRecord(dr_CO_GUID);

        DataRecordShortTextField dr_Language = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_Language.Initialize("Language", "adm_table_header_nbg", "FormInput", "LANGUAGE", false);
        theRecord.AddRecord(dr_Language);

        DataRecordCheckBoxField dr_Is_default_language = new DataRecordCheckBoxField(true);
        dr_Is_default_language.Initialize("Is default language", "adm_table_header_nbg", "FormInput", "IS_DEFAULT_LANGUAGE", false);
        theRecord.AddRecord(dr_Is_default_language);

        string sTable = theRecord.GetTableHTML("adm_media_files_new.aspx?submited=1");
        return sTable;
    }
}
