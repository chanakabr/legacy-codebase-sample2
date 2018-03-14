using ConfigurationManager;
using System;
using TVinciShared;

public partial class adm_block_rules_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
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

                    if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                    {
                        return;
                    }
                    else
                    {
                        apiWS.API client = new apiWS.API();
                        client.Url = url;

                        client.UpdateGeoBlockRulesCache(parentGroupId);
                    }
                }

                return;
            }
            m_sMenu = TVinciShared.Menu.GetMainMenu(5, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["block_rule_id"] != null &&
                Request.QueryString["block_rule_id"].ToString() != "")
            {
                Session["block_rule_id"] = int.Parse(Request.QueryString["block_rule_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("geo_block_types", "group_id", int.Parse(Session["block_rule_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["block_rule_id"] = 0;
        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Geo block rules";
        if (Session["block_rule_id"] != null && Session["block_rule_id"].ToString() != "" && Session["block_rule_id"].ToString() != "0")
            sRet += " - Edit";
        else
            sRet += " - New";
        Response.Write(sRet);
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
        object t = null; ;
        if (Session["block_rule_id"] != null && Session["block_rule_id"].ToString() != "" && int.Parse(Session["block_rule_id"].ToString()) != 0)
        {
            t = Session["block_rule_id"];
        }


       

        string sBack = "adm_block_rules.aspx?search_save=1";

        if (Session["geo_rule_type"] != null)
        {
            sBack += "&geo_rule_type=" + Session["geo_rule_type"].ToString(); 
        }

        DBRecordWebEditor theRecord = new DBRecordWebEditor("geo_block_types", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_domain = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_domain.Initialize("Rule name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_domain);

        DataRecordRadioField dr_but_or_only = new DataRecordRadioField("lu_only_or_but", "description", "id", "", null);
        dr_but_or_only.Initialize("Rule type", "adm_table_header_nbg", "FormInput", "ONLY_OR_BUT", true);
        dr_but_or_only.SetDefault(0);
        theRecord.AddRecord(dr_but_or_only);

        DataRecordMultiField dr_countries = new DataRecordMultiField("countries", "id", "id", "geo_block_types_countries", "GEO_BLOCK_TYPE_ID", "COUNTRY_ID", false, "ltr", 60, "tags");
        dr_countries.Initialize("Rule type", "adm_table_header_nbg", "FormInput", "COUNTRY_NAME", false);
        dr_countries.SetExtraWhere("GROUP_ID is null");
        dr_countries.SetOrderCollectionBy("newid()");
        theRecord.AddRecord(dr_countries);

        bool bVisible = PageUtils.IsTvinciUser();
        if (bVisible == true)
        {
            DataRecordDropDownField dr_groups = new DataRecordDropDownField("groups", "GROUP_NAME", "id", "", null, 60, false);
            dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", true);
            dr_groups.SetWhereString("status<>2 and id " + PageUtils.GetAllChildGroupsStr());
            theRecord.AddRecord(dr_groups);
        }
        else
        {
            DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
            dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
            dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
            theRecord.AddRecord(dr_groups);
        }

        DataRecordShortIntField dr_groups1 = new DataRecordShortIntField(false, 9, 9);
        dr_groups1.Initialize("Group", "adm_table_header_nbg", "FormInput", "GEO_RULE_TYPE", false);

        string geoRuleType = "1";
        if (Session["geo_rule_type"] != null)
        {
            geoRuleType = Session["geo_rule_type"].ToString();
        }
        dr_groups1.SetValue(geoRuleType);

        //Fill PROXY Detection comboBox
        DataRecordDropDownField dr_proxy_rule = new DataRecordDropDownField("proxy_rule_values", "description", "id", "", null, 60, false);
        dr_proxy_rule.Initialize("Proxy Detection", "adm_table_header_nbg", "FormInput", "PROXY_RULE", true);
        theRecord.AddRecord(dr_proxy_rule);
        //Fill PROXY Detection minimum Level 
        DataRecordRadioField dr_proxy_level = new DataRecordRadioField("lu_proxy_level", "description", "id", "", null);
        dr_proxy_level.Initialize("Proxy Level", "adm_table_header_nbg", "FormInput", "PROXY_LEVEL", true);
        dr_proxy_level.SetDefault(1);
        theRecord.AddRecord(dr_proxy_level);

        theRecord.AddRecord(dr_groups1);

        string sTable = theRecord.GetTableHTML("adm_block_rules_new.aspx?submited=1");

        return sTable;
    }
}
