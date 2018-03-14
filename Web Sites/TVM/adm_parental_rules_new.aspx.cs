using ConfigurationManager;
using GroupsCacheManager;
using System;
using System.Collections.Generic;
using TVinciShared;

public partial class adm_parental_rules_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_parental_rules.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_parental_rules.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;

        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
            {
                int id = DBManipulator.DoTheWork();

                if (id > 0)
                {
                    string ip = "1.1.1.1";
                    string userName = "";
                    string password = "";

                    int parentGroupId = DAL.UtilsDal.GetParentGroupID(LoginManager.GetLoginGroupID());
                    TVinciShared.WS_Utils.GetWSUNPass(parentGroupId, "Channel", "api", ip, ref userName, ref password);
                    string url = ApplicationConfiguration.WebServicesConfiguration.Api.URL.Value;
                    string version = TVinciShared.WS_Utils.GetTcmConfigValue("Version");

                    if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                    {
                        return;
                    }
                    else
                    {
                        object ruleId = null;

                        if (Session["rule_id"] != null && Session["rule_id"].ToString() != "" && int.Parse(Session["rule_id"].ToString()) != 0)
                        {
                            ruleId = Session["rule_id"];
                        }

                        List<string> keys = new List<string>();
                        keys.Add(string.Format("{0}_parental_rule_{1}", version, ruleId));

                        apiWS.API client = new apiWS.API();
                        client.Url = url;

                        client.UpdateCache(parentGroupId, "CACHE", keys.ToArray());
                    }
                }

                return;
            }

            Int32 nMenuID = 0;

            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);

            if (Request.QueryString["rule_id"] != null &&
                Request.QueryString["rule_id"].ToString() != "")
            {
                Session["rule_id"] = int.Parse(Request.QueryString["rule_id"].ToString());
            }
            else
                Session["rule_id"] = 0;
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Parental Rule");
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
        int currentGroup = LoginManager.GetLoginGroupID();

        GroupManager groupManager = new GroupManager();
        List<int> subGroups = groupManager.GetSubGroup(currentGroup);
        
        string groups  = currentGroup.ToString();

        if (subGroups != null && subGroups.Count > 0)
        {
            groups = string.Join(",", subGroups);
        }

        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object ruleId = null;

        if (Session["rule_id"] != null && Session["rule_id"].ToString() != "" && int.Parse(Session["rule_id"].ToString()) != 0)
        {
            ruleId = Session["rule_id"];
        }

        string backPage = "adm_parental_rules.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("parental_rules", "adm_table_pager", backPage, "", "ID", ruleId, backPage, "");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_name);

        DataRecordShortIntField dr_order = new DataRecordShortIntField(true, 9, 9);
        dr_order.Initialize("Order", "adm_table_header_nbg", "FormInput", "ORDER_NUM", true);
        theRecord.AddRecord(dr_order);

        // <summary>
        ///// Rule type – Movies, TV series or both
        ///// </summary>
        //public enum eParentalRuleType
        //{
        //    All = 0,
        //    Movies = 1,
        //    TVSeries = 2
        //}
        DataRecordDropDownField dr_rule_type = new DataRecordDropDownField("", "NAME", "id", "", null, 60, true);
        string ruleTypeQuery = "select 0 as id, 'Both' as txt UNION ALL select 1 as id, 'Movies' as txt UNION ALL select 2 as id, 'TV Series' as txt";
        dr_rule_type.SetSelectsQuery(ruleTypeQuery);
        dr_rule_type.Initialize("Type", "adm_table_header_nbg", "FormInput", "RULE_TYPE", true);
        theRecord.AddRecord(dr_rule_type);

        DataRecordLongTextField dr_description = new DataRecordLongTextField("ltr", true, 60, 3);
        dr_description.Initialize("Description", "adm_table_header_nbg", "FormInput", "DESCRIPTION", false);
        theRecord.AddRecord(dr_description);

        DataRecordDropDownField dr_media_tag_type = new DataRecordDropDownField("media_tags_types", "NAME", "id", "", null, 60, true);
        string mediaTagTypeQuery = string.Format(
            @"SELECT    t.NAME AS txt, 
                        t.id   AS id 
            FROM   groups g 
                   LEFT JOIN media_tags_types t 
                          ON ( t.group_id = g.id ) 
            WHERE  t.status = 1 
                   AND g.id IN ({0}) 
                   AND g.id != g.commerce_group_id 
                   AND g.id != g.fictivic_group_id", 
             groups);
        dr_media_tag_type.SetSelectsQuery(mediaTagTypeQuery);
        dr_media_tag_type.Initialize("Media Tag Type", "adm_table_header_nbg", "FormInput", "media_tag_type_id", false);
        theRecord.AddRecord(dr_media_tag_type);

        DataRecordDropDownField dr_epg_tag_type = new DataRecordDropDownField("epg_tags_types", "NAME", "id", "", null, 60, true);
        string epgTagTypeQuery = "select name as txt,id as id from epg_tags_types where status=1 and group_id IN (" + groups + ")";
        dr_epg_tag_type.SetSelectsQuery(epgTagTypeQuery);
        dr_epg_tag_type.Initialize("EPG Tag Type", "adm_table_header_nbg", "FormInput", "epg_tag_type_id", false);
        theRecord.AddRecord(dr_epg_tag_type);


        DataRecordBoolField dr_block_anonymous = new DataRecordBoolField(true);
        dr_block_anonymous.Initialize("Block Anonymous Access", "adm_table_header_nbg", "FormInput", "block_anonymous_access", false);
        theRecord.AddRecord(dr_block_anonymous);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_parental_rules_new.aspx?submited=1");

        return sTable;
    }

}