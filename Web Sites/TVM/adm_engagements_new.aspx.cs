using ApiObjects.Notification;
using ApiObjects.Response;
using System;
using System.Collections.Specialized;
using System.Web;
using TvinciImporter;
using TVinciShared;

public partial class adm_engagements_new : System.Web.UI.Page
{
    protected Engagement engagement;
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        Int32 nMenuID = 0;

        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_engagements.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_engagements.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
            {
                int groupId = LoginManager.GetLoginGroupID();
                ApiObjects.Response.Status result = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

                engagement = GetPageData();

                if (engagement != null)
                {
                    result = ImporterImpl.AddEngagement(groupId, ref engagement);
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
                return;
            }

            m_sMenu = TVinciShared.Menu.GetMainMenu(23, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 7, true);

            if (Request.QueryString["engagement_id"] != null && Request.QueryString["engagement_id"].ToString() != "")
            {
                Session["engagement_id"] = int.Parse(Request.QueryString["engagement_id"].ToString());
            }
            else
            {
                Session["engagement_id"] = 0;
            }

            if (Request.QueryString["user_list"] != null &&
               Request.QueryString["user_list"].ToString() != "")
                Session["user_list"] = int.Parse(Request.QueryString["user_list"].ToString());


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

    private Engagement GetPageData()
    {
        NameValueCollection nvc = Request.Form;

        Engagement engagement = new Engagement();

        if (!string.IsNullOrEmpty(nvc["0_val"]))
        {
            engagement.EngagementType = int.Parse(nvc["0_val"]);
        }

        if (!string.IsNullOrEmpty(nvc["1_val"]))
        {
            //engagement.SendTime = nvc["1_val"]; + nvc["1_valHour"] + nvc["1_valMin"]
        }
        // this fields relevant only for adapter user list
        if (int.Parse(Session["user_list"].ToString()) == 1)
        {
            if (!string.IsNullOrEmpty(nvc["2_val"]))
            {
                engagement.AdapterId = int.Parse(nvc["2_val"]);
            }

            if (!string.IsNullOrEmpty(nvc["3_val"]))
            {
                engagement.AdapterDynamicData = nvc["3_val"];
            }

            if (!string.IsNullOrEmpty(nvc["4_val"]))
            {
                engagement.Interval = int.Parse(nvc["4_val"]);
            }
        }

        // this fields relevant only for manual user list
        if (int.Parse(Session["user_list"].ToString()) == 2)
        {
            if (!string.IsNullOrEmpty(nvc["2_val"]))
            {
                engagement.UserList = nvc["2_val"];
            }
        }

        return engagement;
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
        string sRet = PageUtils.GetPreHeader() + ": Engagement";
        if (IsViewMode())
            sRet += " - View";
        else
            sRet += " - New";
        Response.Write(sRet);
    }

    private bool IsViewMode()
    {
        return (Session["engagement_id"] != null && Session["engagement_id"].ToString() != "" && Session["engagement_id"].ToString() != "0");
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

        object groupId = LoginManager.GetLoginGroupID();
        object engagementId = null;


        bool isViewMode = IsViewMode();
        if (isViewMode)
        {
            engagementId = Session["engagement_id"];
        }

        string sBack = "adm_engagements.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("engagements", "adm_table_pager", sBack, "", "ID", engagementId, sBack, "");
        theRecord.SetConnectionKey("notifications_connection");

        DataRecordLongTextField longTextField;
        DataRecordShortIntField shortIntField;

        DataRecordDropDownField dropDownField = new DataRecordDropDownField("", "name", "id", "", null, 60, false);
        dropDownField.SetSelectsDT(GetEngagementType());
        dropDownField.Initialize("Engagement type", "adm_table_header_nbg", "FormInput", "TEMPLATE_TYPE", true);
        dropDownField.SetEnable(!isViewMode);
        theRecord.AddRecord(dropDownField);

        if (isViewMode)
        {
            shortIntField = new DataRecordShortIntField(!isViewMode, 9, 9);
            shortIntField.Initialize("Total recipients number", "adm_table_header_nbg", "FormInput", "TOTAL_NUMBER_OF_RECIPIENTS", false);
            theRecord.AddRecord(shortIntField);
        }

        DataRecordDateTimeField dateTimeField = new DataRecordDateTimeField(!isViewMode);
        dateTimeField.Initialize("Begin send date & time", "adm_table_header_nbg", "FormInput", "SEND_TIME", false);
        dateTimeField.SetDefault(DateTime.Now);
        theRecord.AddRecord(dateTimeField);

        // this fields relevant only for adapter user list
        if (int.Parse(Session["user_list"].ToString()) == 1)
        {
            dropDownField = new DataRecordDropDownField("engagement_adapter", "name", "id", "group_id", groupId, 60, true);
            dropDownField.Initialize("Source user's list", "adm_table_header_nbg", "FormInput", "ADAPTER_ID", true);
            dropDownField.SetWhereString("status=1 and is_active=1");
            dropDownField.SetEnable(!isViewMode);
            theRecord.AddRecord(dropDownField);

            longTextField = new DataRecordLongTextField("ltr", !isViewMode, 60, 4);
            longTextField.Initialize("Adapter dynamic data", "adm_table_header_nbg", "FormInput", "ADAPTER_DYNAMIC_DATA", true);
            theRecord.AddRecord(longTextField);

            shortIntField = new DataRecordShortIntField(!isViewMode, 9, 9);
            shortIntField.Initialize("Recurring interval (hours)", "adm_table_header_nbg", "FormInput", "INTERVAL", false);
            shortIntField.setMulFactor(60 * 60);
            theRecord.AddRecord(shortIntField);
        }

        // this fields relevant only for manual user list
        if (int.Parse(Session["user_list"].ToString()) == 2)
        {
            longTextField = new DataRecordLongTextField("ltr", !isViewMode, 60, 4);
            longTextField.Initialize("User list", "adm_table_header_nbg", "FormInput", "USER_LIST", true);
            theRecord.AddRecord(longTextField);
        }

        string sTable = theRecord.GetTableHTML("adm_engagements_new.aspx?submited=1");

        return sTable;
    }

    private System.Data.DataTable GetEngagementType()
    {
        System.Data.DataTable dt = new System.Data.DataTable();
        dt.Columns.Add("id", typeof(int));
        dt.Columns.Add("txt", typeof(string));
        int i = 0;
        foreach (ApiObjects.eEngagementType r in Enum.GetValues(typeof(ApiObjects.eEngagementType)))
        {

            dt.Rows.Add((int)r, r);
        }
        return dt;
    }
}