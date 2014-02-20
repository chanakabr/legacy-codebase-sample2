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

public partial class adm_channels_media : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_channels.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(6, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 2, false);
            if (Request.QueryString["search_save"] != null)
                Session["search_save"] = "1";
            else
                Session["search_save"] = null;

            if (Request.QueryString["channel_id"] != null &&
                Request.QueryString["channel_id"].ToString() != "")
            {
                Session["channel_id"] = int.Parse(Request.QueryString["channel_id"].ToString());
            }
            else if (Session["channel_id"] == null || Session["channel_id"].ToString() == "" || Session["channel_id"].ToString() == "0")
            {
                LoginManager.LogoutFromSite("index.html");
                return;
            }
        }
    }

    public void GetCahannelsIFrame()
    {
        string sRet = "<IFRAME SRC=\"admin_tree_player.aspx";
        sRet += "\" WIDTH=\"800px\" HEIGHT=\"300px\" FRAMEBORDER=\"0\"></IFRAME>";
        Response.Write(sRet);
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ":" + PageUtils.GetTableSingleVal("channels", "NAME", int.Parse(Session["channel_id"].ToString())).ToString() + ": Media List");
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
        TVinciShared.Channel channel = new TVinciShared.Channel(int.Parse(Session["channel_id"].ToString()), false , 0 , true , 0 , 0);
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, false, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOldOrderBy, ref channel);

        string sCSVFile = theTable.OpenCSV();
        theTable.Finish();
        theTable = null;
        return sCSVFile;
    }

    protected void FillTheTableEditor(ref DBTableWebEditor theTable, string sOrderBy , ref TVinciShared.Channel channel)
    {
        
        theTable += "select m.id as m_id,m.name,m.description,CONVERT(VARCHAR(11),m.CREATE_DATE, 105) as 'Create Date',CONVERT(VARCHAR(19),m.START_DATE, 120) as 'Start Date',CONVERT(VARCHAR(19),m.End_DATE, 120) as 'End Date' ";
        if (channel.GetType() == 2)
        {
            theTable += ",cm.id as cm_id,cm.status,cm.order_num";
        }
        theTable += " from ";
        if (channel.GetType() == 2)
        {
            theTable += "channels_media cm,";
        }

        theTable += " media m where ";
        if (channel.GetType() == 1)
        {
            string sMediaIDs = channel.GetChannelMediaIDs_OLD(0, null, false, false);
            
            if (string.IsNullOrEmpty(sMediaIDs))
                sMediaIDs = "0";

            theTable += " m.id in (" + sMediaIDs + ")";
        }
        if (channel.GetType() == 2)
        {
            theTable += " cm.media_id=m.id and cm.status=1 and m.status=1 and ";
            theTable += ODBCWrapper.Parameter.NEW_PARAM("cm.channel_id", "=", int.Parse(Session["channel_id"].ToString()));
        }
         

        if (!string.IsNullOrEmpty(channel.GetOrderByStr()))
        {
            theTable += " order by " + channel.GetOrderByStr();
            if (channel.GetChannelOrderDir() == OrderDir.DESC)
            {
                theTable += " desc";
            }
        }
        else if (channel.GetType() == 2)
        {
            theTable += " order by cm.order_num";
        }

        

        if (LoginManager.IsActionPermittedOnPage("adm_channels.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_media_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("media_id", "field=m_id");
            theTable.AddLinkColumn(linkColumn1);
        }
        if (channel.GetType() == 2)
        {
            theTable.AddOrderNumField("channels_media", "cm_id", "order_num", "Order Number");
            theTable.AddHiddenField("order_num");
            theTable.AddHiddenField("cm_id");
            theTable.AddHiddenField("m_id");
            theTable.AddHiddenField("status");
            /*
            if (LoginManager.IsActionPermittedOnPage("adm_channels.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT))
            {
                DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_channels_media_new.aspx", "Edit Inner", "");
                linkColumn1.AddQueryStringValue("channel_media_id", "field=cm_id");
                theTable.AddLinkColumn(linkColumn1);
            }
             */
            if (LoginManager.IsActionPermittedOnPage("adm_channels.aspx", LoginManager.PAGE_PERMISION_TYPE.REMOVE))
            {
                DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
                linkColumn.AddQueryStringValue("id", "field=cm_id");
                linkColumn.AddQueryStringValue("table", "channels_media");
                linkColumn.AddQueryStringValue("confirm", "true");
                linkColumn.AddQueryStringValue("main_menu", "6");
                linkColumn.AddQueryStringValue("sub_menu", "2");
                linkColumn.AddQueryStringValue("rep_field", "NAME");
                linkColumn.AddQueryStringValue("rep_name", "ων");
                theTable.AddLinkColumn(linkColumn);
            }

            if (LoginManager.IsActionPermittedOnPage("adm_channels.aspx", LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
            {
                DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
                linkColumn.AddQueryStringValue("id", "field=cm_id");
                linkColumn.AddQueryStringValue("table", "channels_media");
                linkColumn.AddQueryStringValue("confirm", "true");
                linkColumn.AddQueryStringValue("main_menu", "6");
                linkColumn.AddQueryStringValue("sub_menu", "2");
                linkColumn.AddQueryStringValue("rep_field", "NAME");
                linkColumn.AddQueryStringValue("rep_name", "ων");
                theTable.AddLinkColumn(linkColumn);
            }

            if (LoginManager.IsActionPermittedOnPage("adm_channels.aspx", LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
            {
                DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
                linkColumn.AddQueryStringValue("id", "field=cm_id");
                linkColumn.AddQueryStringValue("table", "channels_media");
                linkColumn.AddQueryStringValue("confirm", "false");
                linkColumn.AddQueryStringValue("main_menu", "6");
                linkColumn.AddQueryStringValue("sub_menu", "2");
                linkColumn.AddQueryStringValue("rep_field", "NAME");
                linkColumn.AddQueryStringValue("rep_name", "ων");
                theTable.AddLinkColumn(linkColumn);
            }
        }
        //if (channel.GetType() == 1 )
        //{
        //    string orderByStr = channel.GetOrderByStr();
        //    if (!string.IsNullOrEmpty(orderByStr))
        //    {
        //        theTable += " order by ";
        //        theTable += orderByStr;
        //        switch (channel.GetChannelOrderDir())
        //        {
        //            case OrderDir.ASC:
        //                theTable += " asc ";
        //                break;
        //            case OrderDir.DESC:
        //                theTable += " desc ";
        //                break;
        //            default:
        //                break;
        //        }
                
        //    }
            
        //}
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();

        TVinciShared.Channel channel = new TVinciShared.Channel(int.Parse(Session["channel_id"].ToString()), false, 0, true, 0, 0);
        bool bAdd = false;
        if (channel.GetType() == 2)
            bAdd = true;
        DBTableWebEditor theTable = new DBTableWebEditor(true, true, bAdd, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOrderBy, ref channel);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);
        Session["ContentPage"] = "adm_channels.aspx?search_save=1";
        theTable.Finish();
        theTable = null;
        return sTable;
    }
}
