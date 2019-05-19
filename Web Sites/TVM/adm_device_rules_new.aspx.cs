using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;
using System.Data;


public partial class adm_device_rules_new : System.Web.UI.Page
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
                DBManipulator.DoTheWork();
                return;
            }
            m_sMenu = TVinciShared.Menu.GetMainMenu(5, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["device_rule_id"] != null &&
                Request.QueryString["device_rule_id"].ToString() != "")
            {
                Session["device_rule_id"] = int.Parse(Request.QueryString["device_rule_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("device_rules", "group_id", int.Parse(Session["device_rule_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["device_rule_id"] = 0;
        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Block media by device rules";
        if (Session["device_rule_id"] != null && Session["device_rule_id"].ToString() != "" && Session["device_rule_id"].ToString() != "0")
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
        if (Session["device_rule_id"] != null && Session["device_rule_id"].ToString() != "" && int.Parse(Session["device_rule_id"].ToString()) != 0)
            t = Session["device_rule_id"];
        string sBack = "adm_device_rules.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("device_rules", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordShortTextField dr_domain = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_domain.Initialize("Rule name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_domain);

        DataRecordRadioField dr_but_or_only = new DataRecordRadioField("lu_only_or_but", "description", "id", "", null);
        dr_but_or_only.Initialize("Rule type", "adm_table_header_nbg", "FormInput", "ONLY_OR_BUT", true);
        dr_but_or_only.SetDefault(0);
        theRecord.AddRecord(dr_but_or_only);

        //DataRecordMultiField dr_countries = new DataRecordMultiField("countries", "id", "id", "geo_block_types_countries", "GEO_BLOCK_TYPE_ID", "COUNTRY_ID", false, "ltr", 60, "tags");
        //dr_countries.Initialize("Rule type", "adm_table_header_nbg", "FormInput", "COUNTRY_NAME", false);
        //dr_countries.SetExtraWhere("GROUP_ID is null");
        //dr_countries.SetOrderCollectionBy("newid()");
        //theRecord.AddRecord(dr_countries);

        //DataRecordMultiField dr_devices = new DataRecordMultiField("groups_device_brands , lu_devicebrands", "device_brand_id", "id", "Device_Rules_Brands", "Rule_ID", "Brand_ID", false, "ltr", 60, "tags");
        
        //????? Look up!!!
        DataRecordMultiField dr_devices = new DataRecordMultiField("lu_devicebrands", "id", "id", "Device_Rules_Brands", "Rule_ID", "Brand_ID", false, "ltr", 60, "tags");
        dr_devices.SetJoinCondition("device_brand_id=lu_devicebrands.ID"); 
        dr_devices.SetCollectionQuery(string.Format("select top 8 ldb.NAME as txt, ldb.id as val from  groups_device_brands gdb, lu_devicebrands ldb where group_id {0} and gdb.device_brand_id=ldb.ID",PageUtils.GetParentsGroupsStr(LoginManager.GetLoginGroupID())));
        dr_devices.Initialize("Device types", "adm_table_header_nbg", "FormInput", "NAME", false);
        theRecord.AddRecord(dr_devices);

        //ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        //selectQuery += "SELECT lu.NAME, lu.ID FROM group_device_brands gdb,lu_DeviceBrands lu WHERE ";
        //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("gdb.GROUP_ID", "=", LoginManager.GetLoginGroupID());
        //if (selectQuery.Execute("query", true) != null)
        //{
        //    foreach (DataRow dr in selectQuery.Table("query").Rows)
        //    {
        //        DataRecordBoolField dr_name = new DataRecordBoolField(true);
        //        dr_name.Initialize(sName, "adm_table_header_nbg", "FormInput", sField, false);
        //        theRecord.AddRecord(dr_name);
        //    }
        //}

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

        //DataRecordShortIntField dr_groups1 = new DataRecordShortIntField(false, 9, 9);
        //dr_groups1.Initialize("Group", "adm_table_header_nbg", "FormInput", "GEO_RULE_TYPE", false);
        //dr_groups1.SetValue("1");
        //theRecord.AddRecord(dr_groups1);

        string sTable = theRecord.GetTableHTML("adm_device_rules_new.aspx?submited=1");

        return sTable;
    }
}