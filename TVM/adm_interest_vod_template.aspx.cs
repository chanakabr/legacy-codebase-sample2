using ApiObjects;
using ApiObjects.Notification;
using ApiObjects.Response;
using System;
using System.Collections.Specialized;
using TvinciImporter;
using TvinciImporter.WSCatalog;
using TVinciShared;

public partial class adm_interest_vod_template : System.Web.UI.Page
{
    protected MessageTemplate messageTemplate;
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        else if (LoginManager.IsPagePermitted("adm_interest_vod_template.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
        m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 3, true);

        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
            {
                int groupId = LoginManager.GetLoginGroupID();

                ApiObjects.Response.Status result = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

                messageTemplate = GetPageData();
                if (messageTemplate != null)
                {
                    result = ImporterImpl.SetMessageTemplate(groupId, ref messageTemplate);
                }

                if (result == null)
                {
                    Session["error_msg_s"] = "Error";
                    Session["error_msg"] = "Error";
                }
                else if (result.Code != (int)ApiObjects.Response.eResponseStatus.OK)
                {
                    Session["error_msg"] = result.Message;
                    Session["error_msg_s"] = result.Message;
                }
            }
        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Interest VOD Template");
    }

    protected void GetLangMenu()
    {
        Response.Write(m_sLangMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        object t = null;
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }

        object groupId = LoginManager.GetLoginGroupID();

        int templateId = GetTemplateId(ODBCWrapper.Utils.GetIntSafeVal(groupId));
        bool isParentGroup = IsParentGroup(ODBCWrapper.Utils.GetIntSafeVal(groupId));

        string sTable = string.Empty;
        if (!isParentGroup)
        {
            sTable = (PageUtils.GetPreHeader() + ": Module is not implemented");
        }
        else
        {
            if (templateId > 0) // a new record
            {
                t = templateId;
            }

            string sBack = "adm_interest_vod_template.aspx?search_save=1";
            DBRecordWebEditor theRecord = new DBRecordWebEditor("message_templates", "adm_table_pager", sBack, "", "ID", t, sBack, "");
            theRecord.SetConnectionKey("notifications_connection");

            DataRecordLongTextField dr_message = new DataRecordLongTextField("ltr", true, 60, 4);
            dr_message.Initialize("Message", "adm_table_header_nbg", "FormInput", "message", true);
            theRecord.AddRecord(dr_message);

            DataRecordShortTextField dr_dateFormat = new DataRecordShortTextField("ltr", true, 60, 256);
            dr_dateFormat.Initialize("Date format", "adm_table_header_nbg", "FormInput", "date_format", true);
            theRecord.AddRecord(dr_dateFormat);

            DataRecordShortTextField dr_sound = new DataRecordShortTextField("ltr", true, 60, 256);
            dr_sound.Initialize("Sound", "adm_table_header_nbg", "FormInput", "sound", false);
            theRecord.AddRecord(dr_sound);

            DataRecordShortTextField dr_action = new DataRecordShortTextField("ltr", true, 60, 256);
            dr_action.Initialize("Action", "adm_table_header_nbg", "FormInput", "action", false);
            theRecord.AddRecord(dr_action);

            DataRecordShortTextField dr_url = new DataRecordShortTextField("ltr", true, 60, 256);
            dr_url.Initialize("URL", "adm_table_header_nbg", "FormInput", "url", false);
            theRecord.AddRecord(dr_url);

            DataRecordShortTextField drShortTextField = new DataRecordShortTextField("ltr", true, 60, 256);
            drShortTextField.Initialize("Mail template", "adm_table_header_nbg", "FormInput", "MAIL_TEMPLATE", false);
            theRecord.AddRecord(drShortTextField);

            drShortTextField = new DataRecordShortTextField("ltr", true, 60, 256);
            drShortTextField.Initialize("Mail subject", "adm_table_header_nbg", "FormInput", "MAIL_SUBJECT", false);
            theRecord.AddRecord(drShortTextField);

            //ratio
            DataRecordDropDownField drDropDownField = new DataRecordDropDownField("lu_pics_ratios", "ratio", "id", "", "", 60, false);            
            drDropDownField.Initialize("Image ratio", "adm_table_header_nbg", "FormInput", "RATIO_ID", false);
            drDropDownField.SetConnectionKey("CONNECTION_STRING");
            drDropDownField.SetValue(GetCurrentValue("RATIO_ID", "message_templates", templateId, "notifications_connection"));
            theRecord.AddRecord(drDropDownField);

            sTable = theRecord.GetTableHTML("adm_interest_vod_template.aspx?submited=1");
        }
        return sTable;
    }

    private string GetCurrentValue(string sField, string sTable, int templateId, string connectionString)
    {
        string sRet = "";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select " + sField + " from " + sTable + " where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", templateId);
        selectQuery += " and status=1";
        selectQuery.SetConnectionKey(connectionString);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                object oRet = selectQuery.Table("query").DefaultView[0].Row[sField];
                if (oRet != null && oRet != DBNull.Value)
                    sRet = oRet.ToString();
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return sRet;
    }

    private int GetTemplateId(int groupID)
    {

        int seriesTemplateId = 0;
        try
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("notifications_connection");
            selectQuery += string.Format("select ID, Message, date_format from message_templates where status=1 and asset_type={0} and", (int)MessageTemplateType.InterestVod);
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    seriesTemplateId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row, "ID");
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        catch
        {
            seriesTemplateId = 0;
        }
        return seriesTemplateId;

    }

    private bool IsParentGroup(int groupID)
    {
        bool res = false;
        try
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
            selectQuery += "select PARENT_GROUP_ID from groups where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", groupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    int parentGroupID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row, "PARENT_GROUP_ID");
                    if (parentGroupID == 1)
                    {
                        res = true;
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        catch (Exception)
        {
            res = false;
        }
        return res;
    }

    private MessageTemplate GetPageData()
    {
        NameValueCollection nvc = Request.Form;

        MessageTemplate followTemplate = new MessageTemplate() { TemplateType = MessageTemplateType.InterestVod };

        if (!string.IsNullOrEmpty(nvc["0_val"]))
        {
            followTemplate.Message = nvc["0_val"];
        }

        if (!string.IsNullOrEmpty(nvc["1_val"]))
        {
            followTemplate.DateFormat = nvc["1_val"];
        }

        if (!string.IsNullOrEmpty(nvc["2_val"]))
        {
            followTemplate.Sound = nvc["2_val"];
        }

        if (!string.IsNullOrEmpty(nvc["3_val"]))
        {
            followTemplate.Action = nvc["3_val"];
        }

        if (!string.IsNullOrEmpty(nvc["4_val"]))
        {
            followTemplate.URL = nvc["4_val"];
        }

        if (!string.IsNullOrEmpty(nvc["5_val"]))
        {
            followTemplate.MailTemplate = nvc["5_val"];
        }

        if (!string.IsNullOrEmpty(nvc["6_val"]))
        {
            followTemplate.MailSubject = nvc["6_val"];
        }

        if (!string.IsNullOrEmpty(nvc["7_val"]))
        {
            int ratioId = 0;
            int.TryParse(nvc["7_val"], out ratioId);
            followTemplate.RatioId = ratioId;
        }

        return followTemplate;
    }
}