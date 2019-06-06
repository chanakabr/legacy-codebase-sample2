using ApiObjects.Notification;
using KLogMonitor;
using System;
using System.Reflection;
using System.Web;
using TvinciImporter;
using TVinciShared;
using System.Linq;

public partial class adm_topic_notification_message_new : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_topic_notification.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_topic_notification.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;

        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            int topicNotificationId = 0;

            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                TopicNotificationMessage topicNotificationMessage = null;
                if (!GetTopicNotificationMessage(ref topicNotificationMessage) || !InsertOrUpdateTopicNotificationMessage(topicNotificationMessage))
                {
                    log.ErrorFormat("Failed GetTopicNotificationMessage or InsertOrUpdateTopicNotificationMessage, topicNotificationMessage_id: {0}", topicNotificationMessage.Id);
                    HttpContext.Current.Session["error_msg"] = "incorrect values while updating / failed inserting new topicNotificationMessage";
                }
                else
                {
                    Session["topic_notification_message_id"] = 0;
                    Session["TopicNotificationMessage"] = null;
                    EndOfAction();
                }

                if ((Request.QueryString["topic_notification_id"] != null && !string.IsNullOrEmpty(Request.QueryString["topic_notification_id"].ToString())
                    && int.TryParse(Request.QueryString["topic_notification_id"].ToString(), out topicNotificationId) && topicNotificationId > 0)
                    || (Session["topic_notification_id"] != null && !string.IsNullOrEmpty(Session["topic_notification_id"].ToString())
                    && int.TryParse(Session["topic_notification_id"].ToString(), out topicNotificationId) && topicNotificationId > 0))
                {
                    Session["topic_notification_id"] = topicNotificationId;
                }
                else
                {
                    Session["topic_notification_id"] = 0;
                }
                return;
            }

            int topicNotificationMessageId = 0;
            if (Request.QueryString["topic_notification_message_id"] != null
                && !string.IsNullOrEmpty(Request.QueryString["topic_notification_message_id"].ToString())
                && int.TryParse(Request.QueryString["topic_notification_message_id"].ToString(), out topicNotificationMessageId) && topicNotificationMessageId > 0)
            {
                Session["topic_notification_message_id"] = topicNotificationMessageId;
            }
            else
            {
                Session["topic_notification_message_id"] = 0;
            }

            if ((Request.QueryString["topic_notification_id"] != null && !string.IsNullOrEmpty(Request.QueryString["topic_notification_id"].ToString())
                && int.TryParse(Request.QueryString["topic_notification_id"].ToString(), out topicNotificationId) && topicNotificationId > 0)
                || (Session["topic_notification_id"] != null && !string.IsNullOrEmpty(Session["topic_notification_id"].ToString())
                && int.TryParse(Session["topic_notification_id"].ToString(), out topicNotificationId) && topicNotificationId > 0))
            {
                Session["topic_notification_id"] = topicNotificationId;
            }
            else
            {
                Session["topic_notification_id"] = 0;
            }
        }
    }

    private void EndOfAction()
    {

        System.Collections.Specialized.NameValueCollection coll = HttpContext.Current.Request.Form;
        if (HttpContext.Current.Session["error_msg"] != null && HttpContext.Current.Session["error_msg"].ToString() != "")
        {
            if (coll["failure_back_page"] != null)
            {
                HttpContext.Current.Response.Write("<script>window.document.location.href='" + coll["failure_back_page"].ToString() + "';</script>");
            }
            else
            {
                HttpContext.Current.Response.Write("<script>window.document.location.href='login.aspx';</script>");
            }
        }
        else
        {
            if (HttpContext.Current.Request.QueryString["back_n_next"] != null)
            {
                HttpContext.Current.Session["last_page_html"] = null;
                string s = HttpContext.Current.Session["back_n_next"].ToString();
                HttpContext.Current.Response.Write("<script>window.document.location.href='" + s.ToString() + "';</script>");
                HttpContext.Current.Session["back_n_next"] = null;
            }
            else
            {
                if (coll["success_back_page"] != null)
                    HttpContext.Current.Response.Write("<script>window.document.location.href='" + coll["success_back_page"].ToString() + "';</script>");
                else
                    HttpContext.Current.Response.Write("<script>window.document.location.href='login.aspx';</script>");
            }
            CachingManager.CachingManager.RemoveFromCache("SetValue_" + coll["table_name"].ToString() + "_");
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Topic Notification Message");
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
        long topicNotificationMessageId = 0;
        TopicNotificationMessage topicNotificationMessage = null;

        if (Session["TopicNotificationMessage"] != null)
        {
            topicNotificationMessage = Session["TopicNotificationMessage"] as TopicNotificationMessage;
            Session["TopicNotificationMessage"] = null;
        }
        else if (Session["topic_notification_message_id"] != null && !string.IsNullOrEmpty(Session["topic_notification_message_id"].ToString()) 
            && long.TryParse(Session["topic_notification_message_id"].ToString(), out topicNotificationMessageId) && topicNotificationMessageId > 0)
        {
            topicNotificationMessage = NotificationHelper.GetTopicNotificationMessage(LoginManager.GetLoginGroupID(), topicNotificationMessageId);
        }

        string sBack = "adm_topic_notification_message.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("bla", "adm_table_pager", sBack, "", "ID", topicNotificationMessageId, sBack, "");
        theRecord.SetConnectionKey("notifications_connection");

        DataRecordLongTextField drLongTextField = new DataRecordLongTextField("ltr", true, 60, 4);
        drLongTextField.setFiledName("message");
        drLongTextField.Initialize("Message", "adm_table_header_nbg", "FormInput", "name", true);
        if (topicNotificationMessage != null)
        {
            drLongTextField.SetValue(topicNotificationMessage.Message);
        }
        theRecord.AddRecord(drLongTextField);

        drLongTextField = new DataRecordLongTextField("ltr", true, 60, 4);
        drLongTextField.setFiledName("image_url");
        drLongTextField.Initialize("Image URL", "adm_table_header_nbg", "FormInput", "image_url", false);
        if (topicNotificationMessage != null)
        {
            drLongTextField.SetValue(topicNotificationMessage.ImageUrl);
        }
        theRecord.AddRecord(drLongTextField);

        DataRecordShortIntField d3 = new DataRecordShortIntField(true, 9, 9);
        d3.setFiledName("offset");
        d3.Initialize("Offset (seconds)", "adm_table_header_nbg", "FormInput", "Offset (seconds)", true);
        if (topicNotificationMessage != null)
        {
            d3.SetValue(((TopicNotificationSubscriptionTrigger)topicNotificationMessage.Trigger).Offset.ToString());
        }
        theRecord.AddRecord(d3);

        if (IsMailNotificationEnabled())
        {
            TopicNotificationMailDispatcher tnmd = null;
            if (topicNotificationMessage != null && topicNotificationMessage.Dispatchers != null)
            {
                var tnmdObject = topicNotificationMessage.Dispatchers.First(x => x.Type == TopicNotificationDispatcherType.Mail);
                tnmd = tnmdObject != null ? (TopicNotificationMailDispatcher)tnmdObject : null;
            }

            DataRecordShortTextField drShortTextField = new DataRecordShortTextField("ltr", true, 60, 256);
            drShortTextField.setFiledName("mail_template");
            drShortTextField.Initialize("Mail template", "adm_table_header_nbg", "FormInput", "MAIL_TEMPLATE", false);
            if (tnmd != null)
            {
                drShortTextField.SetValue(tnmd.BodyTemplate);
            }
            theRecord.AddRecord(drShortTextField);

            drShortTextField = new DataRecordShortTextField("ltr", true, 60, 256);
            drShortTextField.setFiledName("mail_subject");
            drShortTextField.Initialize("Mail subject", "adm_table_header_nbg", "FormInput", "MAIL_SUBJECT", false);
            if (tnmd != null)
            {
                drShortTextField.SetValue(tnmd.SubjectTemplate);
            }
            theRecord.AddRecord(drShortTextField);
        }


        if (IsSMSNotificationEnabled())
        {
            TopicNotificationSmsDispatcher tnsd = null;
            if (topicNotificationMessage != null && topicNotificationMessage.Dispatchers != null)
            {
                var tsndObject = topicNotificationMessage.Dispatchers.First(x => x.Type == TopicNotificationDispatcherType.Sms);
                tnsd = tsndObject != null ? (TopicNotificationSmsDispatcher)tsndObject : null;
            }

            DataRecordCheckBoxField drCheckBoxField = new DataRecordCheckBoxField(true);
            drCheckBoxField.setFiledName("include_sms");
            drCheckBoxField.Initialize("SMS", "adm_table_header_nbg", "FormInput", "INCLUDE_SMS", false);
            drCheckBoxField.SetDefault(tnsd != null ? 1 : 0);
            theRecord.AddRecord(drCheckBoxField);
        }

        string sTable = theRecord.GetTableHTML("adm_topic_notification_message_new.aspx?submited=1");

        return sTable;
    }
   
    private void PageFiled(ref string name, ref string description, ref int subscriptionId)
    {
        System.Collections.Specialized.NameValueCollection coll = HttpContext.Current.Request.Form;
        if (coll["table_name"] == null)
        {
            HttpContext.Current.Session["error_msg"] = "missing table name - cannot update";
        }
        else
        {
            int nCount = coll.Count;
            int nCounter = 0;

            try
            {
                while (nCounter < nCount)
                {
                    try
                    {
                        if (coll[nCounter.ToString() + "_fieldName"] != null)
                        {
                            string sFieldName = coll[nCounter.ToString() + "_fieldName"].ToString();
                            string sVal = "";
                            if (coll[nCounter.ToString() + "_val"] != null)
                            {
                                sVal = coll[nCounter.ToString() + "_val"].ToString();
                            }
                            #region case
                            switch (sFieldName)
                            {
                                case "Name":
                                    name = sVal.Replace("\r\n", "<br\\>");
                                    break;
                                case "Description":
                                    description = sVal.Replace("\r\n", "<br\\>");
                                    break;
                                case "SubscriptionId":
                                    if (!string.IsNullOrEmpty(sVal))
                                    {
                                        subscriptionId = int.Parse(sVal);
                                    }
                                    break;
                                default:
                                    break;

                            }
                            #endregion
                        }

                    }
                    catch (Exception)
                    {
                        break;
                    }

                    nCounter++;
                }
                //convert datetime to UTC
                ////date = ODBCWrapper.Utils.ConvertToUtc(date, timezone);

            }
            catch (Exception)
            {
            }
        }
    }

    private bool GetTopicNotificationMessage(ref TopicNotificationMessage topicNotificationMessage)
    {
        bool result = false;
        System.Collections.Specialized.NameValueCollection coll = HttpContext.Current.Request.Form;
        int nCount = coll.Count;
        int nCounter = 0;
        try
        {
            topicNotificationMessage = new TopicNotificationMessage()
            {
                Dispatchers = new System.Collections.Generic.List<TopicNotificationDispatcher>()
            };

            topicNotificationMessage.GroupId = LoginManager.GetLoginGroupID();

            long topicNotificationMessageId = 0;

            TopicNotificationMailDispatcher mail = null;
            TopicNotificationSmsDispatcher sms = null;

            if (Session["topic_notification_message_id"] != null && long.TryParse(Session["topic_notification_message_id"].ToString(), out topicNotificationMessageId))
            {
                topicNotificationMessage.Id = topicNotificationMessageId;
            }

            long topicNotificationId = 0;
            if (Session["topic_notification_id"] != null && long.TryParse(Session["topic_notification_id"].ToString(), out topicNotificationId))
            {
                topicNotificationMessage.TopicNotificationId = topicNotificationId;
            }

            result = true;
            while (nCounter < nCount && result)
            {
                try
                {
                    if (coll[nCounter.ToString() + "_fieldName"] != null)
                    {
                        string sFieldName = coll[nCounter.ToString() + "_fieldName"].ToString();
                        string sVal = "";
                        if (coll[nCounter.ToString() + "_val"] != null)
                        {
                            sVal = coll[nCounter.ToString() + "_val"].ToString();
                        }

                        if (!string.IsNullOrEmpty(sVal))
                        {
                            #region case
                            switch (sFieldName)
                            {
                                case "message":
                                    topicNotificationMessage.Message = sVal;
                                    break;
                                case "image_url":
                                    topicNotificationMessage.ImageUrl = sVal;
                                    break;
                                case "offset":
                                    topicNotificationMessage.Trigger = new TopicNotificationSubscriptionTrigger()
                                    {
                                        Offset = long.Parse(sVal),
                                        TriggerType = ApiObjects.TopicNotificationSubscriptionTriggerType.StartDate
                                    };
                                    break;
                                case "mail_template":
                                    if (mail == null)
                                    {
                                        mail = new TopicNotificationMailDispatcher();
                                    }
                                    mail.BodyTemplate = sVal;
                                    break;
                                case "mail_subject":
                                    if (mail == null)
                                    {
                                        mail = new TopicNotificationMailDispatcher();
                                    }
                                    mail.SubjectTemplate = sVal;
                                    break;
                                case "include_sms":
                                    if (sVal == "1")
                                        sms = new TopicNotificationSmsDispatcher();
                                    break;
                                default:
                                    break;

                            }
                            #endregion
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Failed in switch GetTopicNotification", ex);
                    result = false;
                    break;
                }

                nCounter++;
            }

            if (mail != null)
                topicNotificationMessage.Dispatchers.Add(mail);

            if (sms != null)
                topicNotificationMessage.Dispatchers.Add(sms);

        }
        catch (Exception ex)
        {
            log.Error("Failed GetFriendlyAssetLifeCycleRule", ex);
        }

        if (result)
        {
            Session["TopicNotificationMessage"] = topicNotificationMessage;
        }

        return result;
    }

    private bool InsertOrUpdateTopicNotificationMessage(TopicNotificationMessage topicNotificationMessage)
    {
        bool result = false;
        if (topicNotificationMessage != null)
        {
            if (topicNotificationMessage.Id == 0)
            {
                topicNotificationMessage = NotificationHelper.AddTopicNotificationMessage(LoginManager.GetLoginGroupID(), topicNotificationMessage, LoginManager.GetLoginID());
            }
            else
            {
                topicNotificationMessage = NotificationHelper.UpdateTopicNotificationMessage(LoginManager.GetLoginGroupID(), topicNotificationMessage, LoginManager.GetLoginID());
            }

            if(topicNotificationMessage != null)
            {
                result = true;
            }
            else
            {
                Session["error_msg"] = "Failed to insert or update topic notification message";
            }            
        }

        return result;
    }

    private bool IsMailNotificationEnabled()
    {
        bool mailNotificationEnabled = false;
        try
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("notifications_connection");
            selectQuery += "select MAIL_NOTIFICATION_ADAPTER_ID from notification_settings where status=1 and is_active =1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    int mailNotificationAdapterId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row, "MAIL_NOTIFICATION_ADAPTER_ID");
                    mailNotificationEnabled = mailNotificationAdapterId > 0;
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        catch (Exception ex)
        {
            log.Error("", ex);
        }
        return mailNotificationEnabled;
    }

    private bool IsSMSNotificationEnabled()
    {
        bool enabled = false;
        try
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("notifications_connection");
            selectQuery += "select IS_SMS_ENABLE from notification_settings where status=1 and is_active =1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    enabled = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row, "IS_SMS_ENABLE") == 1 ? true : false;
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }
        catch (Exception ex)
        {
            log.Error("", ex);
        }
        return enabled;
    }
}