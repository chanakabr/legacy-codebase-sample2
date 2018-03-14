using ConfigurationManager;
using ODBCWrapper;
using System;
using System.Collections.Generic;
using TVinciShared;

public partial class adm_parental_rules_values : System.Web.UI.Page
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
                    UpdateQuery updateQuery = new UpdateQuery("parental_rule_tag_values");

                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ASSET_TYPE", "=", (int)Session["asset_type"]);
                    updateQuery += " WHERE ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("RULE_ID", "=", (int)Session["rule_id"]);
                    updateQuery += " AND ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
                    updateQuery += " AND ASSET_TYPE IS NULL";
                    updateQuery.Execute();
                    updateQuery.Finish();

                    string ip = "1.1.1.1";
                    string userName = "";
                    string password = "";

                    int parentGroupId = DAL.UtilsDal.GetParentGroupID(LoginManager.GetLoginGroupID());
                    TVinciShared.WS_Utils.GetWSUNPass(parentGroupId, "UpdateCache", "api", ip, ref userName, ref password);
                    string url = ApplicationConfiguration.WebServicesConfiguration.Api.URL.Value;
                    string version = ApplicationConfiguration.Version.Value;

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

            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["rule_id"] != null &&
                Request.QueryString["rule_id"].ToString() != "")
            {
                Session["rule_id"] = int.Parse(Request.QueryString["rule_id"].ToString());
            }
            else
                Session["rule_id"] = 0;

            if (Request.QueryString["asset_type"] != null &&
                Request.QueryString["asset_type"].ToString() != "")
            {
                Session["asset_type"] = int.Parse(Request.QueryString["asset_type"].ToString());
            }
            else
                Session["asset_type"] = 0;
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Parental rule");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    public string GetPageContent(string orderBy, string pageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object ruleId = null;
        
        if (Session["rule_id"] != null && Session["rule_id"].ToString() != "" && int.Parse(Session["rule_id"].ToString()) != 0)
            ruleId = Session["rule_id"];

        int assetTypeId = -1;

        if (Session["asset_type"] != null && Session["asset_type"] != DBNull.Value)
        {
            assetTypeId = Convert.ToInt32(Session["asset_type"]);
        }

        string backPage = "adm_parental_rules.aspx?search_save=1";

        DBRecordWebEditor theRecord = new DBRecordWebEditor("parental_rules", "adm_table_pager", backPage, "", "ID", ruleId, backPage, "");

        string tagTypeName = string.Empty;
        Int32 tagTypeID = 0;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

        string tagsTypesTable = string.Empty;
        string tagsTable = string.Empty;
        string tagTypeIdColumn = string.Empty;
        string assetName = string.Empty;

        // This is defined in core enum: MEDIA = 2, EPG = 0
        if (assetTypeId == 2)
        {
            tagsTypesTable = "media_tags_types";
            tagsTable = "tags";
            tagTypeIdColumn = "tag_type_id";
            assetName = "media";
        }
        else if (assetTypeId == 0)
        {
            tagsTypesTable = "epg_tags_types";
            tagsTable = "epg_tags";
            tagTypeIdColumn = "epg_tag_type_id";
            assetName = "epg";
        }

        selectQuery += string.Format("select tt.id, tt.name from parental_rules pr, {0} tt where", tagsTypesTable);
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("pr.id", "=", ruleId.ToString());
        selectQuery += string.Format("and pr.{0}_tag_type_id=tt.id", assetName);
        
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;

            for (int i = 0; i < nCount; i++)
            {
                tagTypeName = selectQuery.Table("query").DefaultView[i].Row["NAME"].ToString();
                tagTypeID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());

                DataRecordMultiField dr_tags = new DataRecordMultiField(tagsTable, "id", "id", "parental_rule_tag_values", "rule_id", "TAG_ID", false, "ltr", 60, "tags");
                dr_tags.SetCollectionLength(20);
                dr_tags.SetExtraWhere(tagTypeIdColumn + "=" + tagTypeID);
                dr_tags.SetCollectionQuery(
                    string.Format("SELECT TOP 20 value as txt, ID as val from {0} where {1} = {2} AND STATUS = 1", 
                    tagsTable, tagTypeIdColumn, tagTypeID));
                dr_tags.SetMiddleTableType(tagsTypesTable);
                dr_tags.Initialize(tagTypeName, "adm_table_header_nbg", "FormInput", "VALUE", false);

                theRecord.AddRecord(dr_tags);
            }
        }

        selectQuery.Finish();
        selectQuery = null;

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_parental_rules_values.aspx?submited=1");

        return sTable;
    }
}