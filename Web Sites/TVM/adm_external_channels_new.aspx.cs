using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_external_channels_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_external_channels.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_external_channels.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;

        if (!IsPostBack)
        {
            bool flag = false;

            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
            {
                int channelId = 0;

                // Validate uniqe external id
                if (Session["channel_id"] != null && Session["channel_id"].ToString() != "" && int.Parse(Session["channel_id"].ToString()) != 0)
                {
                    int.TryParse(Session["channel_id"].ToString(), out channelId);
                }

                System.Collections.Specialized.NameValueCollection coll = HttpContext.Current.Request.Form;

                if (coll != null && coll.Count > 2 && !string.IsNullOrEmpty(coll["1_val"]))
                {
                    if (IsExternalIDExists(coll["1_val"], channelId))
                    {
                        Session["error_msg"] = "External Id must be unique";
                        flag = true;
                    }
                    else
                    {
                        Int32 nID = DBManipulator.DoTheWork();

                        string ip = "1.1.1.1";
                        string userName = "";
                        string password = "";

                        int parentGroupId = DAL.UtilsDal.GetParentGroupID(LoginManager.GetLoginGroupID());
                        TVinciShared.WS_Utils.GetWSUNPass(parentGroupId, "UpdateCache", "api", ip, ref userName, ref password);
                        string url = TVinciShared.WS_Utils.GetTcmConfigValue("api_ws");
                        string version = TVinciShared.WS_Utils.GetTcmConfigValue("Version");

                        if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                        {
                            return;
                        }
                        else
                        {
                            List<string> keys = new List<string>();
                            keys.Add(string.Format("{0}_external_channel_{1}_{2}", version, parentGroupId, channelId));

                            apiWS.API client = new apiWS.API();
                            client.Url = url;

                            client.UpdateCache(parentGroupId, "CACHE", keys.ToArray());
                        }

                        return;
                    }
                }
            }

            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);

            if (Request.QueryString["channel_id"] != null && Request.QueryString["channel_id"].ToString() != "")
            {
                Session["channel_id"] = int.Parse(Request.QueryString["channel_id"].ToString());
            }
            else if (!flag)
                Session["channel_id"] = 0;
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": External Channel");
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

        object channelId = null;

        if (Session["channel_id"] != null && Session["channel_id"].ToString() != "" && int.Parse(Session["channel_id"].ToString()) != 0)
        {
            channelId = Session["channel_id"];
        }

        string backUrl = "adm_external_channels.aspx?search_save=1";

        object group_id = LoginManager.GetLoginGroupID();

        DBRecordWebEditor theRecord = new DBRecordWebEditor("external_channels", "adm_table_pager", backUrl, "", "ID", channelId, backUrl, "");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_name);

        DataRecordShortTextField dr_external_identifier = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_external_identifier.Initialize("External Identifier", "adm_table_header_nbg", "FormInput", "external_identifier", true);
        theRecord.AddRecord(dr_external_identifier);

        DataRecordDropDownField dr_recommendation_engine = new DataRecordDropDownField("recommendation_engines", "name", "id", "group_id", group_id, 60, true);
        dr_recommendation_engine.Initialize("Recommendation Engine Provider", "adm_table_header_nbg", "FormInput", "RECOMMENDATION_ENGINE_ID", false);
        theRecord.AddRecord(dr_recommendation_engine);

        DataRecordLongTextField dr_filter_expression = new DataRecordLongTextField("ltr", true, 60, 3);
        dr_filter_expression.Initialize("Filter to apply for the response:", "adm_table_header_nbg", "FormInput", "FILTER_EXPRESSION", false);
        theRecord.AddRecord(dr_filter_expression);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "group_id", false);
        dr_groups.SetValue(group_id.ToString());
        theRecord.AddRecord(dr_groups);

        string table = theRecord.GetTableHTML("adm_external_channels_new.aspx?submited=1");

        return table;
    }

    static public string GetWSURL(string sKey)
    {
        return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
    }

    static private bool IsExternalIDExists(string externalId, int channelId)
    {
        int groupID = LoginManager.GetLoginGroupID();
        bool result = false;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("main_connection_string");
        selectQuery += "select ID from external_channels where status=1 and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("external_identifier", "=", externalId);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "<>", channelId);

        if (selectQuery.Execute("query", true) != null)
        {
            int count = selectQuery.Table("query").DefaultView.Count;

            if (count > 0)
            {
                result = true;
                int newChannelId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "ID", 0);
                string name = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "NAME", 0);
                Logger.Logger.Log("IsExternalIDExists", string.Format("id:{0}, name:{1}", newChannelId, name), "recommendation_engines");
            }
        }

        selectQuery.Finish();
        selectQuery = null;

        return result;
    }
}