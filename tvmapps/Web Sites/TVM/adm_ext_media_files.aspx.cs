using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using TVinciShared;

public partial class adm_ext_media_files : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_media.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 2, false);
            if (Request.QueryString["search_save"] != null)
                Session["search_save"] = "1";
            else
                Session["search_save"] = null;

            if (Request.QueryString["ext_media_id"] != null &&
                Request.QueryString["ext_media_id"].ToString() != "")
                Session["ext_media_id"] = int.Parse(Request.QueryString["ext_media_id"].ToString());
            else
            {
                Session["ext_media_id"] = null;
                LoginManager.LogoutFromSite("index.html");
                return;
            }
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ":" + PageUtils.GetTableSingleVal("media", "NAME", int.Parse(Session["ext_media_id"].ToString())).ToString() + " Content Files ");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    public string GetTableCSV()
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, false, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOldOrderBy);

        string sCSVFile = theTable.OpenCSV();
        theTable.Finish();
        theTable = null;
        return sCSVFile;
    }

    protected void FillTheTableEditor(ref DBTableWebEditor theTable, string sOrderBy)
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        theTable += "select mf.editor_remarks,mf.is_active,mf.id,mf.status,lmt.description as 'Media Type',lmq.description as 'Media Quality',lcs.description as 'State' from lu_content_status lcs,lu_media_quality lmq,lu_media_types lmt,media_files mf where lcs.id=mf.status and mf.MEDIA_TYPE_ID=lmt.id and mf.MEDIA_QUALITY_ID=lmq.id and " + PageUtils.GetStatusQueryPart("mf") + "and";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("mf.media_id", "=", int.Parse(Session["ext_media_id"].ToString()));

        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("status");
        theTable.AddVideoField("media_files");
        //theTable.AddOrderNumField("media_files", "ID", "order_num", "Order number");
        theTable.AddActivationField("media_files");
        theTable.AddHiddenField("is_active");
        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        else
            theTable += " order by mf.order_num,mf.id desc";
        theTable.AddTechDetails("media_files");
        theTable.AddEditorRemarks("media_files");
        theTable.AddHiddenField("EDITOR_REMARKS");
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();

        DBTableWebEditor theTable = new DBTableWebEditor(true, true, true, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);
        Session["ContentPage"] = "adm_media.aspx";
        theTable.Finish();
        theTable = null;
        return sTable;
    }
}
