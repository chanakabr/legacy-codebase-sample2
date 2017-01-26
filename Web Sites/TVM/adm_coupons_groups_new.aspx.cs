using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;
using System.Configuration;

public partial class adm_coupons_groups_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;

    static public bool IsTvinciImpl()
    {
        Int32 nImplID = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("pricing_connection");
        selectQuery += "select * from groups_modules_implementations(nolock) where is_active=1 and status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", LoginManager.GetLoginGroupID());
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_ID", "=", 3);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                nImplID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["IMPLEMENTATION_ID"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        if (nImplID == 1)
            return true;
        return false;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            if (IsTvinciImpl() == false)
            {
                Server.Transfer("adm_module_not_implemented.aspx");
                return;
            }

            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                DBManipulator.DoTheWork("pricing_connection");
                return;
            }
            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["coupon_group_id"] != null &&
                Request.QueryString["coupon_group_id"].ToString() != "")
            {
                Session["coupon_group_id"] = int.Parse(Request.QueryString["coupon_group_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("coupons_groups", "group_id", int.Parse(Session["coupon_group_id"].ToString()), "pricing_connection").ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["coupon_group_id"] = 0;
        }

    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Coupon Groups";
        if (Session["coupon_group_id"] != null && Session["coupon_group_id"].ToString() != "" && Session["coupon_group_id"].ToString() != "0")
            sRet += " - Edit";
        else
            sRet += " - New";
        Response.Write(sRet);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    static protected string GetWSURL()
    {
        return TVinciShared.WS_Utils.GetTcmConfigValue("pricing_ws");
    }

    static protected string GetMainLang()
    {
        string sMainLang = "";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select l.CODE3,l.id from groups g,lu_languages l where l.id=g.language_id and  ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.id", "=", LoginManager.GetLoginGroupID());
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                sMainLang = selectQuery.Table("query").DefaultView[0].Row["CODE3"].ToString();
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return sMainLang;
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object t = null; ;
        if (Session["coupon_group_id"] != null && Session["coupon_group_id"].ToString() != "" && int.Parse(Session["coupon_group_id"].ToString()) != 0)
            t = Session["coupon_group_id"];
        string sBack = "adm_coupons_groups.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("coupons_groups", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("pricing_connection");

        DataRecordShortTextField dr_domain = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_domain.Initialize("Code", "adm_table_header_nbg", "FormInput", "Code", true);
        theRecord.AddRecord(dr_domain);

        DataRecordDateTimeField dr_start_date = new DataRecordDateTimeField(true);
        dr_start_date.Initialize("Start Date", "adm_table_header_nbg", "FormInput", "START_DATE", false);
        dr_start_date.SetDefault(DateTime.Now);
        theRecord.AddRecord(dr_start_date);

        DataRecordDateTimeField dr_end_date = new DataRecordDateTimeField(true);
        dr_end_date.Initialize("End Date", "adm_table_header_nbg", "FormInput", "END_DATE", false);
        dr_end_date.SetDefault(new DateTime(2099, 1, 1));
        theRecord.AddRecord(dr_end_date);

        DataRecordRadioField dr_disc = new DataRecordRadioField("discount_codes", "code", "id", "", null);
        dr_disc.SetFieldType("string");
        System.Data.DataTable discCodesDT = new System.Data.DataTable();
        string sWSUserName = "";
        string sWSPass = "";

        string sIP = "1.1.1.1";
        TVinciShared.WS_Utils.GetWSUNPass(LoginManager.GetLoginGroupID(), "GetDiscountsModuleListForAdmin", "pricing", sIP, ref sWSUserName, ref sWSPass);
        TvinciPricing.mdoule m = new TvinciPricing.mdoule();
        string sWSURL = GetWSURL();
        if (sWSURL != "")
            m.Url = sWSURL;

        TvinciPricing.DiscountModule[] oModules = m.GetDiscountsModuleListForAdmin(sWSUserName, sWSPass);
        if (oModules != null)
        {
            Int32 n = 0;
            string s = "";
            discCodesDT.Columns.Add(PageUtils.GetColumn("ID", n));
            discCodesDT.Columns.Add(PageUtils.GetColumn("txt", s));
            for (int i = 0; i < oModules.Length; i++)
            {
                System.Data.DataRow tmpRow = null;

                tmpRow = discCodesDT.NewRow();
                tmpRow["ID"] = oModules[i].m_nObjectID;
                tmpRow["txt"] = oModules[i].m_sCode;
                TvinciPricing.LanguageContainer[] lang = oModules[i].m_sDescription;
                if (lang != null)
                {
                    string sMainLang = GetMainLang();
                    for (int j = 0; j < lang.Length; j++)
                    {
                        if (lang[j].m_sLanguageCode3 == sMainLang)
                            tmpRow["txt"] += "(" + lang[j].m_sValue + ")";
                    }
                }
                discCodesDT.Rows.InsertAt(tmpRow, 0);
                discCodesDT.AcceptChanges();
            }
        }

        dr_disc.SetSelectsDT(discCodesDT);
        dr_disc.Initialize("Discount Code", "adm_table_header_nbg", "FormInput", "DISCOUNT_CODE", true);
        dr_disc.SetDefault(0);
        theRecord.AddRecord(dr_disc);

        DataRecordShortIntField dr_maxusetime = new DataRecordShortIntField(true, 9, 9);
        dr_maxusetime.Initialize("Maximum number of initiated activations", "adm_table_header_nbg", "FormInput", "MAX_USE_TIME", true);
        dr_maxusetime.SetDefault(1);
        theRecord.AddRecord(dr_maxusetime);

        DataRecordShortIntField dr_max_recurring_number = new DataRecordShortIntField(true, 9, 9);
        dr_max_recurring_number.Initialize("Maximum recurring billing cycles", "adm_table_header_nbg", "FormInput", "MAX_RECURRING_USES", true);
        dr_max_recurring_number.SetDefault(0);
        theRecord.AddRecord(dr_max_recurring_number);

        DataRecordRadioField dr_financial = new DataRecordRadioField("lu_financial_entities", "text", "id", "", null);
        dr_financial.SetFieldType("string");
        dr_financial.Initialize("Financial Entity", "adm_table_header_nbg", "FormInput", "financial_entity_id", true);
        dr_financial.SetDefault(0);
        theRecord.AddRecord(dr_financial);

        DataRecordRadioField dr_coupon_type = new DataRecordRadioField("lu_coupon_group_type", "text", "id", "", null);
        dr_coupon_type.SetFieldType("string");
        dr_coupon_type.Initialize("Coupon Type", "adm_table_header_nbg", "FormInput", "COUPON_GROUP_TYPE", true);
        dr_coupon_type.SetDefault(0);
        theRecord.AddRecord(dr_coupon_type);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_coupons_groups_new.aspx?submited=1");

        return sTable;
    }
}
