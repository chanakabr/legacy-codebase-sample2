using ApiObjects.Notification;
using KLogMonitor;
using System;
using System.Reflection;
using System.Web;
using TvinciImporter;
using TVinciShared;

public partial class adm_topic_notification_new : System.Web.UI.Page
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
            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                TopicNotification topicNotification = null;
                if (!GetTopicNotification(ref topicNotification) || !InsertOrUpdateTopicNotification(topicNotification))
                {
                    log.ErrorFormat("Failed GetTopicNotification or InsertOrUpdateTopicNotification, topicNotification_id: {0}, name: {1}", topicNotification.Id, topicNotification.Name);
                    HttpContext.Current.Session["error_msg"] = "incorrect values while updating / failed inserting new topicNotification";
                }
                else
                {
                    Session["topic_notification_id"] = 0;
                    Session["TopicNotification"] = null;
                    EndOfAction();
                }
                return;
            }

            int topicNotificationId = 0;
            if (Request.QueryString["topic_notification_id"] != null 
                && !string.IsNullOrEmpty(Request.QueryString["topic_notification_id"].ToString()) 
                && int.TryParse(Request.QueryString["topic_notification_id"].ToString(), out topicNotificationId) && topicNotificationId > 0)
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
        Response.Write(PageUtils.GetPreHeader() + ": Topic Notification");
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
        long topicNotificationId = 0;
        TopicNotification topicNotification = null;

        if (Session["TopicNotification"] != null)
        {
            topicNotification = Session["TopicNotification"] as TopicNotification;
            Session["TopicNotification"] = null;
        }
        else if (Session["topic_notification_id"] != null && !string.IsNullOrEmpty(Session["topic_notification_id"].ToString()) && long.TryParse(Session["topic_notification_id"].ToString(), out topicNotificationId) && topicNotificationId > 0)
        {
            topicNotification = NotificationHelper.GetTopicNotification(LoginManager.GetLoginGroupID(), topicNotificationId);
        }

        string sBack = "adm_topic_notification.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("bla", "adm_table_pager", sBack, "", "ID", topicNotificationId, sBack, "");
        theRecord.SetConnectionKey("notifications_connection");       

        DataRecordShortTextField drShortTextField = new DataRecordShortTextField("ltr", true, 60, 256);
        drShortTextField.setFiledName("Name");
        drShortTextField.Initialize("Name", "adm_table_header_nbg", "FormInput", "name", true);
        if (topicNotification != null)
        {
            drShortTextField.SetValue(topicNotification.Name);
        }
        theRecord.AddRecord(drShortTextField);

        DataRecordLongTextField drLongTextField = new DataRecordLongTextField("ltr", true, 60, 4);
        drLongTextField.setFiledName("Description");
        drLongTextField.Initialize("Description", "adm_table_header_nbg", "FormInput", "Message", false);
        if (topicNotification != null)
        {
            drLongTextField.SetValue(topicNotification.Description);
        }
        theRecord.AddRecord(drLongTextField);

        //SubscriptionId
        DataRecordShortIntField drShortIntField = new DataRecordShortIntField(true, 9, 9);
        drShortIntField.setFiledName("SubscriptionId");
        drShortIntField.Initialize("Subscription Id", "adm_table_header_nbg", "FormInput", "Message", true);
        if (topicNotification != null && topicNotification.SubscribeReference != null && (topicNotification.SubscribeReference as SubscriptionSubscribeReference) != null)
        {
            drShortIntField.SetValue(((SubscriptionSubscribeReference)topicNotification.SubscribeReference).SubscriptionId.ToString());
        }
        theRecord.AddRecord(drShortIntField);

        string sTable = theRecord.GetTableHTML("adm_topic_notification_new.aspx?submited=1");

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

    private bool GetTopicNotification(ref TopicNotification topicNotification)
    {
        bool result = false;
        System.Collections.Specialized.NameValueCollection coll = HttpContext.Current.Request.Form;
        int nCount = coll.Count;
        int nCounter = 0;
        try
        {
            topicNotification = new TopicNotification();
            topicNotification.GroupId = LoginManager.GetLoginGroupID();
            long topicNotificationId = 0;
            long subId = 0;

            if (Session["topic_notification_id"] != null && long.TryParse(Session["topic_notification_id"].ToString(), out topicNotificationId))
            {
                topicNotification.Id = topicNotificationId;
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
                                case "Name":
                                    topicNotification.Name = sVal;
                                    break;
                                case "Description":
                                    topicNotification.Description = sVal;
                                    break;
                                case "SubscriptionId":
                                    subId = long.Parse(sVal);
                                    topicNotification.SubscribeReference = new SubscriptionSubscribeReference() { SubscriptionId = subId };
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

        }
        catch (Exception ex)
        {
            log.Error("Failed TopicNotification", ex);
        }

        if (result)
        {
            Session["TopicNotification"] = topicNotification;
        }

        return result;
    }

    private bool InsertOrUpdateTopicNotification(TopicNotification topicNotification)
    {
        bool result = false;
        if (topicNotification != null)
        {
            if (topicNotification.Id == 0)
            {
                topicNotification = NotificationHelper.AddTopicNotification(LoginManager.GetLoginGroupID(), topicNotification, LoginManager.GetLoginID());
            }
            else
            {
                topicNotification = NotificationHelper.UpdateTopicNotification(LoginManager.GetLoginGroupID(), topicNotification, LoginManager.GetLoginID());
            }

            if(topicNotification != null)
            {
                result = true;
            }
            else
            {
                Session["error_msg"] = "Failed to insert or update topic notification";
            }            
        }

        return result;
    }
}