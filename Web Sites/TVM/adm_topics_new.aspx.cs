using ApiObjects;
using System;
using System.Collections.Specialized;
using System.Web;
using TvinciImporter;
using TVinciShared;

public partial class adm_topics_new : System.Web.UI.Page
{
    private const string NOTIFICATIONS_CONNECTION = "notifications_connection";
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_topics.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_topics.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;

            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true); 

            if (Request.QueryString["announcement_id"] != null && Request.QueryString["announcement_id"].ToString() != "")
            {
                Session["announcement_id"] = int.Parse(Request.QueryString["announcement_id"].ToString());
            }
            else if (Session["announcement_id"] == null || Session["announcement_id"].ToString() == "")
            {
                Session["announcement_id"] = 0;
            }

            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
            {

                ApiObjects.Response.Status result = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
                int id = 0;
                if (Session["announcement_id"] != null && Session["announcement_id"].ToString() != "")
                {
                    id = int.Parse(Session["announcement_id"].ToString());
                }
                int groupId = LoginManager.GetLoginGroupID();
                var automaticSending = GetPageData();

                result = ImporterImpl.SetTopicSettings(groupId, id, automaticSending);

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

                EndOfAction();

                return;
            }
           
           

            if (Session["error_msg_s"] != null && Session["error_msg_s"].ToString() != "")
            {
                lblError.Visible = true;
                lblError.Text = Session["error_msg_s"].ToString();
                Session["error_msg_s"] = null;
            }
            else
            {
                lblError.Visible = false;
                lblError.Text = "";
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

    protected string GetLangMenu(Int32 nGroupID)
    {
        try
        {
            string sTemp = "";
            Int32 nCount = 0;
            string sMainLang = "";
            Int32 nMainLangID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select l.name,l.id from groups g,lu_languages l where l.id=g.language_id and  ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sMainLang = selectQuery.Table("query").DefaultView[0].Row["name"].ToString();
                    nMainLangID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            sTemp += "<li><a class=\"on\" href=\"";
            sTemp += "adm_media_new.aspx?media_id=" + Session["media_id"].ToString();
            sTemp += "\"><span>";
            sTemp += sMainLang;
            sTemp += "</span></a></li>";

            Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("media", "group_id", int.Parse(Session["media_id"].ToString())).ToString());
            ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
            selectQuery1 += "select l.name,l.id from group_extra_languages gel,lu_languages l where gel.language_id=l.id and l.status=1 and gel.status=1 and  ";
            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("l.id", "<>", nMainLangID);
            selectQuery1 += "and";
            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("gel.group_id", "=", nOwnerGroupID);
            selectQuery1 += " order by l.name";
            if (selectQuery1.Execute("query", true) != null)
            {
                Int32 nCount1 = selectQuery1.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount1; i++)
                {
                    Int32 nLangID = int.Parse(selectQuery1.Table("query").DefaultView[i].Row["id"].ToString());
                    string nLangName = selectQuery1.Table("query").DefaultView[i].Row["name"].ToString();
                    sTemp += "<li><a href=\"";
                    sTemp += "adm_media_translate.aspx?media_id=" + Session["media_id"].ToString() + "&lang_id=" + nLangID.ToString();
                    sTemp += "\"><span>";
                    sTemp += nLangName;
                    sTemp += "</span></a></li>";
                }
                if (nCount1 == 0)
                    sTemp = "";
            }
            selectQuery1.Finish();
            selectQuery1 = null;

            return sTemp;
        }
        catch
        {
            HttpContext.Current.Response.Redirect("login.html");
            return "";
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Topic: " + PageUtils.GetTableSingleVal("announcements", "NAME", int.Parse(Session["announcement_id"].ToString()), NOTIFICATIONS_CONNECTION).ToString());
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    protected void GetLangMenu()
    {
        Response.Write(m_sLangMenu);
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {

        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object announcementId = null;

        if (Session["announcement_id"] != null && Session["announcement_id"].ToString() != "" && int.Parse(Session["announcement_id"].ToString()) != 0)
            announcementId = Session["announcement_id"];

        string sBack = "adm_topics.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("announcements", "adm_table_pager", sBack, "", "ID", announcementId, sBack, "");
        theRecord.SetConnectionKey(NOTIFICATIONS_CONNECTION);

        DataRecordDropDownField automaticSendingDropDown = new DataRecordDropDownField("", "automatic_sending", "id", "", null, 60, true);
        string automaticSendingQuery = "select 0 as id, 'No' as txt UNION ALL select 1 as id, 'Yes' as txt";
        automaticSendingDropDown.SetSelectsQuery(automaticSendingQuery);
        automaticSendingDropDown.SetNoSelectStr("Inherit");
        automaticSendingDropDown.Initialize("Automatic sending", "adm_table_header_nbg", "FormInput", "automatic_sending", true);
        theRecord.AddRecord(automaticSendingDropDown);

        string sTable = theRecord.GetTableHTML("adm_topics_new.aspx?submited=1");

        return sTable;
    }

    private eTopicAutomaticIssueNotification GetPageData()
    {
        eTopicAutomaticIssueNotification isAutomaticSending = eTopicAutomaticIssueNotification.Default;

        NameValueCollection nvc = Request.Form;

        if (!string.IsNullOrEmpty(nvc["0_val"]))
        {
            var automaticSendingStr = nvc["0_val"];
            switch (automaticSendingStr)
            {
                case "1":
                    isAutomaticSending = eTopicAutomaticIssueNotification.Yes;
                    break;
                case "0":
                    isAutomaticSending = eTopicAutomaticIssueNotification.No;
                    break;
                default:
                    break;
            }
        }

        return isAutomaticSending;
    }
}