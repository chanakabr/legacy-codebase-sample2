using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using KLogMonitor;
using TVinciShared;

public partial class adm_tvp_galleries_types_new : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
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
                Int32 nGroupID = LoginManager.GetLoginGroupID();
                DBManipulator.DoTheWork("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
                foreach (DictionaryEntry objItem in Cache)
                    Cache.Remove(objItem.Key.ToString());
                return;
            }
            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["tvp_channel_galleries_types_id"] != null &&
               Request.QueryString["tvp_channel_galleries_types_id"].ToString() != "")
            {
                Session["tvp_channel_galleries_types_id"] = int.Parse(Request.QueryString["tvp_channel_galleries_types_id"].ToString());
            }
            else
                Session["tvp_channel_galleries_types_id"] = 0;
        }

    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Channels Galleries Types ";
        if (Session["tvp_channel_galleries_types_id"] != null && Session["tvp_channel_galleries_types_id"].ToString() != "" && Session["tvp_channel_galleries_types_id"].ToString() != "0")
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
        Int32 nGroupID = LoginManager.GetLoginGroupID();

        object t = null; ;
        if (Session["tvp_channel_galleries_types_id"] != null && Session["tvp_channel_galleries_types_id"].ToString() != "" && int.Parse(Session["tvp_channel_galleries_types_id"].ToString()) != 0)
            t = Session["tvp_channel_galleries_types_id"];

        string sBack = "adm_tvp_galleries_types.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("tvp_template_channels_gallery_types", "adm_table_pager", sBack, "", "ID", t, sBack, "");
        theRecord.SetConnectionKey("tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
        log.Debug("Gallery Types - tvp_connection_" + nGroupID.ToString() + "_" + Session["platform"].ToString());
        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "VIRTUAL_NAME", true);
        theRecord.AddRecord(dr_name);

        DataRecordDropDownField dr_UI_COMPONENT_TYPE = new DataRecordDropDownField("lu_ui_components_types", "DESCRIPTION", "id", "", null, 60, false);
        dr_UI_COMPONENT_TYPE.Initialize("UI Component Type", "adm_table_header_nbg", "FormInput", "UI_COMPONENT_TYPE", true);
        theRecord.AddRecord(dr_UI_COMPONENT_TYPE);

        DataRecordBoolField dr_IN_SIDE = new DataRecordBoolField(true);
        dr_IN_SIDE.Initialize("In Side", "adm_table_header_nbg", "FormInput", "IN_SIDE", false);
        theRecord.AddRecord(dr_IN_SIDE);

        DataRecordBoolField dr_IN_TOP = new DataRecordBoolField(true);
        dr_IN_TOP.Initialize("In Top", "adm_table_header_nbg", "FormInput", "IN_TOP", false);
        theRecord.AddRecord(dr_IN_TOP);

        DataRecordBoolField dr_IN_BOTTOM = new DataRecordBoolField(true);
        dr_IN_BOTTOM.Initialize("In Bottom", "adm_table_header_nbg", "FormInput", "IN_BOTTOM", false);
        theRecord.AddRecord(dr_IN_BOTTOM);

        DataRecordBoolField dr_IN_MAIN = new DataRecordBoolField(true);
        dr_IN_MAIN.Initialize("In Main", "adm_table_header_nbg", "FormInput", "IN_MAIN", false);
        theRecord.AddRecord(dr_IN_MAIN);

        DataRecordDropDownField dr_NUM_OF_ITEMS = new DataRecordDropDownField("lu_num_of_items", "description", "id", "", null, 60, false);
        dr_NUM_OF_ITEMS.Initialize("Number of items", "adm_table_header_nbg", "FormInput", "NUM_OF_ITEMS", true);
        theRecord.AddRecord(dr_NUM_OF_ITEMS);

        DataRecordBoolField dr_GROUP_HAS_MAIN_DESCRIPTION = new DataRecordBoolField(true);
        dr_GROUP_HAS_MAIN_DESCRIPTION.Initialize("Group Has Main Description", "adm_table_header_nbg", "FormInput", "GROUP_HAS_MAIN_DESCRIPTION", false);
        theRecord.AddRecord(dr_GROUP_HAS_MAIN_DESCRIPTION);

        DataRecordBoolField dr_GROUP_HAS_SUB_DESCRIPTION = new DataRecordBoolField(true);
        dr_GROUP_HAS_SUB_DESCRIPTION.Initialize("Group Has Sub Description", "adm_table_header_nbg", "FormInput", "GROUP_HAS_SUB_DESCRIPTION", false);
        theRecord.AddRecord(dr_GROUP_HAS_SUB_DESCRIPTION);

        DataRecordBoolField dr_GROUP_HAS_TITLE = new DataRecordBoolField(true);
        dr_GROUP_HAS_TITLE.Initialize("Group Has Title", "adm_table_header_nbg", "FormInput", "GROUP_HAS_TITLE", false);
        theRecord.AddRecord(dr_GROUP_HAS_TITLE);

        DataRecordBoolField dr_HAS_TABS = new DataRecordBoolField(true);
        dr_HAS_TABS.Initialize("Group Has Buttons", "adm_table_header_nbg", "FormInput", "HAS_TABS", false);
        theRecord.AddRecord(dr_HAS_TABS);

        DataRecordBoolField dr_HAS_BUTTONS = new DataRecordBoolField(true);
        dr_HAS_BUTTONS.Initialize("Group Has Links", "adm_table_header_nbg", "FormInput", "HAS_BUTTONS", false);
        theRecord.AddRecord(dr_HAS_BUTTONS);

        DataRecordBoolField dr_HAS_TVM_ACCOUNT = new DataRecordBoolField(true);
        dr_HAS_TVM_ACCOUNT.Initialize("Group Has Tvm Account", "adm_table_header_nbg", "FormInput", "HAS_TVM_ACCOUNT", false);
        theRecord.AddRecord(dr_HAS_TVM_ACCOUNT);

        DataRecordDropDownField dr_DEFAULT_TVM_ACCOUNT = new DataRecordDropDownField("tvp_tvm_accounts", "NAME", "id", "", null, 60, false);
        dr_DEFAULT_TVM_ACCOUNT.Initialize("Group Default TVM Account For Main", "adm_table_header_nbg", "FormInput", "DEFAULT_TVM_ACCOUNT", true);
        dr_DEFAULT_TVM_ACCOUNT.SetWhereString("status=1 and is_active=1 and id<>0 and group_id= " + nGroupID.ToString());
        theRecord.AddRecord(dr_DEFAULT_TVM_ACCOUNT);

        DataRecordBoolField dr_HAS_CHANNEL_MAIN = new DataRecordBoolField(true);
        dr_HAS_CHANNEL_MAIN.Initialize("Item Has Channel Main", "adm_table_header_nbg", "FormInput", "HAS_CHANNEL_MAIN", false);
        theRecord.AddRecord(dr_HAS_CHANNEL_MAIN);

        DataRecordDropDownField dr_DEFAULT_CHANNEL_MAIN_TVM = new DataRecordDropDownField("tvp_tvm_accounts", "NAME", "id", "", null, 60, false);
        dr_DEFAULT_CHANNEL_MAIN_TVM.Initialize("Item Default TVM Account For Main", "adm_table_header_nbg", "FormInput", "DEFAULT_TVM_FOR_CHANNEL_MAIN", true);
        dr_DEFAULT_CHANNEL_MAIN_TVM.SetWhereString("status=1 and is_active=1 and group_id= " + nGroupID.ToString());
        theRecord.AddRecord(dr_DEFAULT_CHANNEL_MAIN_TVM);

        DataRecordBoolField dr_HAS_CHANNEL_SUB = new DataRecordBoolField(true);
        dr_HAS_CHANNEL_SUB.Initialize("Item Has Channel Sub", "adm_table_header_nbg", "FormInput", "HAS_CHANNEL_SUB", false);
        theRecord.AddRecord(dr_HAS_CHANNEL_SUB);

        DataRecordDropDownField dr_DEFAULT_CHANNEL_MAIN_SUB = new DataRecordDropDownField("tvp_tvm_accounts", "NAME", "id", "", null, 60, false);
        dr_DEFAULT_CHANNEL_MAIN_SUB.Initialize("Item Default TVM Account For SUB", "adm_table_header_nbg", "FormInput", "DEFAULT_TVM_FOR_CHANNEL_SUB", true);
        dr_DEFAULT_CHANNEL_MAIN_SUB.SetWhereString("status=1 and is_active=1 and group_id= " + nGroupID.ToString());
        theRecord.AddRecord(dr_DEFAULT_CHANNEL_MAIN_SUB);

        DataRecordBoolField dr_HAS_TITLE = new DataRecordBoolField(true);
        dr_HAS_TITLE.Initialize("Item Has Title", "adm_table_header_nbg", "FormInput", "HAS_TITLE", false);
        theRecord.AddRecord(dr_HAS_TITLE);

        DataRecordBoolField dr_HAS_LINK = new DataRecordBoolField(true);
        dr_HAS_LINK.Initialize("Item Has Link", "adm_table_header_nbg", "FormInput", "HAS_LINK", false);
        theRecord.AddRecord(dr_HAS_LINK);

        DataRecordBoolField dr_HAS_PIC_MAIN = new DataRecordBoolField(true);
        dr_HAS_PIC_MAIN.Initialize("Item Has Pic Main", "adm_table_header_nbg", "FormInput", "HAS_PIC_MAIN", false);
        theRecord.AddRecord(dr_HAS_PIC_MAIN);

        DataRecordBoolField dr_HAS_PIC_SUB = new DataRecordBoolField(true);
        dr_HAS_PIC_SUB.Initialize("Item Has Pic Sub", "adm_table_header_nbg", "FormInput", "HAS_PIC_SUB", false);
        theRecord.AddRecord(dr_HAS_PIC_SUB);

        DataRecordBoolField dr_HAS_SWF = new DataRecordBoolField(true);
        dr_HAS_SWF.Initialize("Item Has SWF", "adm_table_header_nbg", "FormInput", "HAS_SWF", false);
        theRecord.AddRecord(dr_HAS_SWF);

        DataRecordBoolField dr_HAS_MAIN_DESCRIPTION = new DataRecordBoolField(true);
        dr_HAS_MAIN_DESCRIPTION.Initialize("Item Has Main Description", "adm_table_header_nbg", "FormInput", "HAS_MAIN_DESCRIPTION", false);
        theRecord.AddRecord(dr_HAS_MAIN_DESCRIPTION);

        DataRecordBoolField dr_HAS_SUB_DESCRIPTION = new DataRecordBoolField(true);
        dr_HAS_SUB_DESCRIPTION.Initialize("Item Has Sub Description", "adm_table_header_nbg", "FormInput", "HAS_SUB_DESCRIPTION", false);
        theRecord.AddRecord(dr_HAS_SUB_DESCRIPTION);

        DataRecordBoolField dr_HAS_TOOLTIP_TEXT = new DataRecordBoolField(true);
        dr_HAS_TOOLTIP_TEXT.Initialize("Item Has Tooltip", "adm_table_header_nbg", "FormInput", "HAS_TOOLTIP_TEXT", false);
        theRecord.AddRecord(dr_HAS_TOOLTIP_TEXT);

        DataRecordShortIntField dr_default_page_size = new DataRecordShortIntField(true, 9, 9);
        dr_default_page_size.Initialize("Item Default Page Size", "adm_table_header_nbg", "FormInput", "DEFAULT_PAGE_SIZE", true);
        theRecord.AddRecord(dr_default_page_size);

        DataRecordBoolField dr_HAS_PAGE_SIZE = new DataRecordBoolField(true);
        dr_HAS_PAGE_SIZE.Initialize("Item Has Page Size", "adm_table_header_nbg", "FormInput", "HAS_PAGE_SIZE", false);
        theRecord.AddRecord(dr_HAS_PAGE_SIZE);

        DataRecordShortIntField dr_DEFAULT_MAX_RESULT_NUM = new DataRecordShortIntField(true, 9, 9);
        dr_DEFAULT_MAX_RESULT_NUM.Initialize("Item Default Maximum Results Number", "adm_table_header_nbg", "FormInput", "DEFAULT_MAX_RESULT_NUM", true);
        theRecord.AddRecord(dr_DEFAULT_MAX_RESULT_NUM);

        DataRecordBoolField dr_HAS_MAX_RESULT_NUM = new DataRecordBoolField(true);
        dr_HAS_MAX_RESULT_NUM.Initialize("Item Has Maximum Result Size", "adm_table_header_nbg", "FormInput", "HAS_MAX_RESULT_NUM", false);
        theRecord.AddRecord(dr_HAS_MAX_RESULT_NUM);

        DataRecordBoolField dr_HAS_DEFAULT_VIEW_TYPE = new DataRecordBoolField(true);
        dr_HAS_DEFAULT_VIEW_TYPE.Initialize("Item Has View Type", "adm_table_header_nbg", "FormInput", "HAS_DEFAULT_VIEW_TYPE", false);
        theRecord.AddRecord(dr_HAS_DEFAULT_VIEW_TYPE);

        DataRecordDropDownField dr_DEFAULT_VIEW_TYPE = new DataRecordDropDownField("lu_gallery_view_types", "Description", "id", "", null, 60, false);
        dr_DEFAULT_VIEW_TYPE.Initialize("Item Default View Type", "adm_table_header_nbg", "FormInput", "DEFAULT_VIEW_TYPE", true);
        theRecord.AddRecord(dr_DEFAULT_VIEW_TYPE);

        DataRecordBoolField dr_HAS_DEFAULT_PIC_SIZE = new DataRecordBoolField(true);
        dr_HAS_DEFAULT_PIC_SIZE.Initialize("Item Has Pic Size", "adm_table_header_nbg", "FormInput", "HAS_PIC_SIZE", false);
        theRecord.AddRecord(dr_HAS_DEFAULT_PIC_SIZE);

        DataRecordDropDownField dr_DEFAULT_PIC_SIZE = new DataRecordDropDownField("lu_gallery_pic_sizes", "Description", "id", "", null, 60, false);
        dr_DEFAULT_PIC_SIZE.Initialize("Item Default Pic Size", "adm_table_header_nbg", "FormInput", "DEFAULT_PIC_SIZE", true);
        theRecord.AddRecord(dr_DEFAULT_PIC_SIZE);

        DataRecordBoolField dr_HAS_DEFAULT_SUB_PIC_SIZE = new DataRecordBoolField(true);
        dr_HAS_DEFAULT_SUB_PIC_SIZE.Initialize("Item Has Sub Pic Size", "adm_table_header_nbg", "FormInput", "HAS_SUB_PIC_SIZE", false);
        theRecord.AddRecord(dr_HAS_DEFAULT_SUB_PIC_SIZE);

        DataRecordDropDownField dr_DEFAULT_SUB_PIC_SIZE = new DataRecordDropDownField("lu_gallery_pic_sizes", "Description", "id", "", null, 60, false);
        dr_DEFAULT_SUB_PIC_SIZE.Initialize("Item Default Sub Pic Size", "adm_table_header_nbg", "FormInput", "DEFAULT_SUB_PIC_SIZE", true);
        theRecord.AddRecord(dr_DEFAULT_SUB_PIC_SIZE);

        DataRecordMultiField dr_page_types = new DataRecordMultiField("lu_page_types", "id", "id", "dbo.tvp_galleries_templates_page_types", "tvp_gallery_template_id", "PAGE_TYPE", false, "ltr", 60, "tags");
        dr_page_types.Initialize("Available on page types", "adm_table_header_nbg", "FormInput", "DESCRIPTION", false);
        dr_page_types.SetOrderCollectionBy("newid()");
        dr_page_types.SetCollectionLength(50);
        theRecord.AddRecord(dr_page_types);

        DataRecordBoolField dr_HAS_TIME = new DataRecordBoolField(true);
        dr_HAS_TIME.Initialize("Item Has Time", "adm_table_header_nbg", "FormInput", "HAS_TIME", false);
        theRecord.AddRecord(dr_HAS_TIME);

        DataRecordBoolField dr_HAS_BOOLEAN = new DataRecordBoolField(true);
        dr_HAS_BOOLEAN.Initialize("Item Has Boolean", "adm_table_header_nbg", "FormInput", "HAS_BOOLEAN", false);
        theRecord.AddRecord(dr_HAS_BOOLEAN);

        DataRecordShortTextField dr_bool_text = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_bool_text.Initialize("Boolean title", "adm_table_header_nbg", "FormInput", "BOOLEAN_TITLE", false);
        theRecord.AddRecord(dr_bool_text);

        DataRecordBoolField dr_HAS_NUMERIC = new DataRecordBoolField(true);
        dr_HAS_NUMERIC.Initialize("Item Has Numeric", "adm_table_header_nbg", "FormInput", "HAS_NUMERIC", false);
        theRecord.AddRecord(dr_HAS_NUMERIC);

        DataRecordShortTextField dr_NUMERIC_TITLE = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_NUMERIC_TITLE.Initialize("Numeric title", "adm_table_header_nbg", "FormInput", "NUMERIC_TITLE", false);
        theRecord.AddRecord(dr_NUMERIC_TITLE);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_tvp_galleries_types_new.aspx?submited=1");

        return sTable;
    }
}
