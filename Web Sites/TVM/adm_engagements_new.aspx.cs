using ApiObjects;
using ApiObjects.Notification;
using ApiObjects.Response;
using System;
using System.Collections.Specialized;
using System.Web;
using TvinciImporter;
using TVinciShared;
using KLogMonitor;
using System.Reflection;

public partial class adm_engagements_new : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
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

                int type = 0;
                if (Session["type"] != null && int.TryParse(Session["type"].ToString(), out type))
                {
                    Engagement engagement = GetPageData(type);

                    if (engagement != null)
                    {
                        result = ImporterImpl.AddEngagement(groupId, ref engagement);
                    }
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

            if (Request.QueryString["type"] != null &&
               Request.QueryString["type"].ToString() != "")
                Session["type"] = int.Parse(Request.QueryString["type"].ToString());


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

    private Engagement GetPageData(int type)
    {
        NameValueCollection nvc = Request.Form;

        Engagement engagement = new Engagement();

        engagement.IsActive = true;

        if (!string.IsNullOrEmpty(nvc["0_val"]))
        {
            int engagementType = int.Parse(nvc["0_val"]);
            engagement.EngagementType = Enum.IsDefined(typeof(eEngagementType), engagementType) ? (eEngagementType)engagementType : eEngagementType.Churn;
        }

        if (!string.IsNullOrEmpty(nvc["1_val"]))
        {
            string date = nvc["1_val"].ToString();
            string minutes = nvc["1_valMin"].ToString();
            string hour = nvc["1_valHour"].ToString();
            bool isValid = DBManipulator.validateParam("int", hour, 0, 23);
            if (isValid == true)
                isValid = DBManipulator.validateParam("int", minutes, 0, 59);
            if (isValid == true)
                isValid = DBManipulator.validateParam("date", date, 0, 59);
            DateTime sendTime = DateUtils.GetDateFromStr(date);

            if (!string.IsNullOrEmpty(hour))
                sendTime = sendTime.AddHours(int.Parse(hour));

            if (!string.IsNullOrEmpty(minutes))
                sendTime = sendTime.AddMinutes(int.Parse(minutes));

            engagement.SendTime = sendTime;

        }

        int tempValue = 0;

        if (!string.IsNullOrEmpty(nvc["2_val"]) && int.TryParse(nvc["2_val"], out tempValue))
        {
            engagement.CouponGroupId = tempValue;
        }

        // this fields relevant only for adapter user list
        if (type == 1)
        {
            if (!string.IsNullOrEmpty(nvc["3_val"]) && int.TryParse(nvc["3_val"], out tempValue))
            {
                engagement.AdapterId = tempValue;
            }

            if (!string.IsNullOrEmpty(nvc["4_val"]))
            {
                engagement.AdapterDynamicData = nvc["4_val"];
            }

            if (!string.IsNullOrEmpty(nvc["5_val"]) && int.TryParse(nvc["5_val"], out tempValue))
            {
                engagement.IntervalSeconds = tempValue * 3600;
            }
        }
        
        if (type == 2)
        {
            if (!string.IsNullOrEmpty(nvc["3_val"]))
            {
                engagement.UserList = nvc["3_val"];
            }
        }

        return engagement;
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
        int engagementId = 0;

        bool isViewMode = IsViewMode();
        
        if (isViewMode)
        {
            int.TryParse(Session["engagement_id"].ToString(), out engagementId);    
        }

        string sBack = "adm_engagements.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("engagements", "adm_table_pager", sBack, "", "ID", isViewMode ? engagementId.ToString() : null, sBack, "");
        theRecord.SetConnectionKey("notifications_connection");

        DataRecordLongTextField longTextField;
        DataRecordShortIntField shortIntField;

        DataRecordDropDownField dropDownField = new DataRecordDropDownField("", "name", "id", "", null, 60, false);
        dropDownField.SetSelectsDT(GetEngagementType());
        dropDownField.Initialize("Engagement type", "adm_table_header_nbg", "FormInput", "engagement_type", true);
        dropDownField.SetEnable(!isViewMode);
        theRecord.AddRecord(dropDownField);

        if (isViewMode)
        {
            shortIntField = new DataRecordShortIntField(!isViewMode, 9, 9);
            shortIntField.Initialize("Total recipients number", "adm_table_header_nbg", "FormInput", "TOTAL_NUMBER_OF_RECIPIENTS", false);
            theRecord.AddRecord(shortIntField);
        }

        DataRecordDateTimeField dateTimeField = new DataRecordDateTimeField(!isViewMode);
        dateTimeField.Initialize("Begin send date & time", "adm_table_header_nbg", "FormInput", "SEND_TIME", true);
        if (!isViewMode)
        {
            dateTimeField.SetDefault(DateTime.UtcNow);
        }
        //dateTimeField.setTimeZone("UTC");
        theRecord.AddRecord(dateTimeField);

        if (!isViewMode)
        {
            dropDownField = new DataRecordDropDownField("coupons_groups", "CODE", "id", "group_id", groupId, 60, false);
            dropDownField.SetSelectsQuery("select id, CODE as 'txt' from pricing.dbo.coupons_groups where status<>2 and is_active=1 and ISNULL(COUPON_GROUP_TYPE, 0) <> 1 and group_id=" + groupId);
            dropDownField.Initialize("Coupon group", "adm_table_header_nbg", "FormInput", "COUPON_GROUP_ID", true);
            theRecord.AddRecord(dropDownField);
        }
        else
        {
            DataRecordShortTextField couponsGroup = new DataRecordShortTextField("ltr", false, 60, 60);
            couponsGroup.Initialize("Coupon group", "adm_table_header_nbg", "FormInput", "ADAPTER_DYNAMIC_DATA", false);
            couponsGroup.SetValue(GetCouponGroupName(engagementId));
            theRecord.AddRecord(couponsGroup);
        }

        // this fields relevant only for adapter user list
        int type = 0;
        if (Session["type"] != null)
        {
            int.TryParse(Session["type"].ToString(), out type);
        }

        if (type == 1)
        {
            dropDownField = new DataRecordDropDownField("engagement_adapter", "name", "id", "group_id", groupId, 60, false);
            dropDownField.Initialize("Source user's list", "adm_table_header_nbg", "FormInput", "ADAPTER_ID", true);
            dropDownField.SetWhereString("status=1 and is_active=1");
            dropDownField.SetEnable(!isViewMode);
            theRecord.AddRecord(dropDownField);

            longTextField = new DataRecordLongTextField("ltr", !isViewMode, 60, 4);
            longTextField.Initialize("Adapter dynamic data", "adm_table_header_nbg", "FormInput", "ADAPTER_DYNAMIC_DATA", false);
            theRecord.AddRecord(longTextField);

            shortIntField = new DataRecordShortIntField(!isViewMode, 9, 9);
            shortIntField.Initialize("Recurring interval (hours)", "adm_table_header_nbg", "FormInput", "INTERVAL_SECONDS", false);
            theRecord.AddRecord(shortIntField);
        }

        // this fields relevant only for manual user list
        if (type == 2)
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

    private string GetCouponGroupName(int engagementId)
    {
        string couponName = "";

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select cg.CODE from pricing.dbo.coupons_groups cg join MessageBox.dbo.engagements e on e.COUPON_GROUP_ID = cg.id";
        selectQuery += "and e.id=" + engagementId;
        selectQuery.SetCachedSec(0);
        if (selectQuery.Execute("query", true) != null)
        {
            int count = selectQuery.Table("query").DefaultView.Count;
            if (count > 0)
            {
                couponName = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "CODE", 0);
            }
        }
        selectQuery.Finish();
        selectQuery = null;

        return couponName;
    }
}