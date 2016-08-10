using ApiObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using TvinciImporter;
using TVinciShared;

public partial class adm_channels : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sSubSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted() == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
        {
            Response.Expires = -1;
            return;
        }
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(6, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 2, false);
            if (Request.QueryString["search_save"] != null)
                Session["search_save"] = "1";
            else
                Session["search_save"] = null;

            System.Collections.SortedList sortedMenu = GetSubMenuList();
            m_sSubSubMenu = TVinciShared.Menu.GetSubMenu(sortedMenu, -1, false);

            Session["asset_type_ids"] = null;
        }
    }

    protected System.Collections.SortedList GetSubMenuList()
    {
        System.Collections.SortedList sortedMenu = new SortedList();
        string sButton = "Add A.Channel";
        sButton += "|";
        sButton += "adm_channels_new.aspx?channel_type=1";
        sortedMenu[0] = sButton;

        sButton = "Add M.Channel";
        sButton += "|";
        sButton += "adm_channels_new.aspx?channel_type=2";
        sortedMenu[1] = sButton;

        int groupId = LoginManager.GetLoginGroupID();
        int parentGroupId = DAL.UtilsDal.GetParentGroupID(groupId);

        // add KSQL button only if we are in parent group
        if (groupId == parentGroupId)
        {
            sButton = "Add KSQL.Channel|adm_ksql_channel_new.aspx";

            sortedMenu[2] = sButton;
        }

        return sortedMenu;
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Channels");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    protected void GetSubSubMenu()
    {
        Response.Write(m_sSubSubMenu);
    }

    public string GetTableCSV()
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();
        DBTableWebEditor theTable =
            new DBTableWebEditor(true, true, false, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 20);
        FillTheTableEditor(ref theTable, sOldOrderBy);

        string sCSVFile = theTable.OpenCSV();
        theTable.Finish();
        theTable = null;
        return sCSVFile;
    }

    protected void FillTheTableEditor(ref DBTableWebEditor theTable, string sOrderBy)
    {
        // example
        /*	 
            SELECT    q.editor_remarks, 
                      q.order_num, 
                      q.is_active, 
                      q.status, 
                      q.s_id         AS 'ID', 
                      q.s_id         AS 'CID', 
                      p.base_url     AS 'Pic', 
                      q.NAME         AS 'Name', 
                      q.admin_name   AS 'Uniqe Name', 
                      q.description  AS 'Description', 
                      q.is_rss       AS 'Enables feed', 
                      q.channel_type AS 'Channel Type', 
                      s.NAME         AS 'Subscription', 
                      q.s_desc       AS 'State',
                      q.channel_type_id as channel_type
            FROM      ( 
                             SELECT lct.description AS 'channel_type', 
                                    c.subscription_id, 
                                    c.order_num AS 'order_num', 
                                    c.is_active AS 'q_ia', 
                                    c.pic_id    AS 'pic_id', 
                                    CASE c.is_rss 
                                           WHEN 0 THEN 'False' 
                                           ELSE 'True' 
                                    END AS is_rss, 
                                    c.status, 
                                    c.NAME          AS 'NAME', 
                                    c.admin_name    AS 'ADMIN_NAME', 
                                    c.description   AS 'Description', 
                                    c.id            AS 's_id', 
                                    lcs.description AS 's_desc', 
                                    c.is_active, 
                                    c.editor_remarks,
                                    c.CHANNEL_TYPE as channel_type_id
                             FROM   lu_channel_type lct, 
                                    channels c, 
                                    lu_content_status lcs 
                             WHERE  ( 
                                           lct.id=c.channel_type 
                                    OR     c.channel_type = 4) 
                             AND    c.status<>2 
                             AND    lcs.id=c.status 
                             AND    c.watcher_id=@0 
                             AND    c.group_id=@1 )q 
            LEFT JOIN pics p 
            ON        p.id=q.pic_id 
            AND       p.status IN (1,3,4) 
            LEFT JOIN subscriptions s 
            ON        s.id=q.subscription_id 
            ORDER BY  q.order_num, 
                      q.s_id DESC(0,216)
         */

        Int32 nGroupID = LoginManager.GetLoginGroupID();


        // ?
        // lu_channel_types_NEW

        theTable += @"SELECT q.editor_remarks, 
       q.order_num, 
       q.is_active, 
       q.status, 
       q.s_id            AS 'ID', 
       q.s_id            AS 'CID', 
       p.base_url        AS 'Pic', 
       q.NAME            AS 'Name', 
       q.admin_name      AS 'Uniqe Name', 
       q.description     AS 'Description', 
       q.is_rss          AS 'Enables feed', 
       q.channel_type    AS 'Channel Type', 
       s.NAME            AS 'Subscription', 
       q.s_desc          AS 'State', 
       q.channel_type_id AS channel_type ,
       q.pic_Id
FROM   (SELECT lct.description AS 'channel_type', 
               c.subscription_id, 
               c.order_num     AS 'order_num', 
               c.is_active     AS 'q_ia', 
               c.pic_id        AS 'pic_id', 
               CASE c.is_rss 
                 WHEN 0 THEN 'False' 
                 ELSE 'True' 
               END             AS is_rss, 
               c.status, 
               c.NAME          AS 'NAME', 
               c.admin_name    AS 'ADMIN_NAME', 
               c.description   AS 'Description', 
               c.id            AS 's_id', 
               lcs.description AS 's_desc', 
               c.is_active, 
               c.editor_remarks, 
               c.channel_type  AS channel_type_id 
        FROM   lu_channels_types_pyro lct, channels c
               LEFT JOIN lu_content_status lcs 
                      ON lcs.id = c.status 
        WHERE  c.status <> 2 AND c.channel_type = lct.ID";

        if (Session["search_free"] != null && Session["search_free"].ToString() != "")
        {
            string sLike = "like('%" + Session["search_free"].ToString().Replace("'", "''") + "%')";
            theTable += " and (c.ADMIN_NAME " + sLike + " OR c.NAME " + sLike + " OR c.DESCRIPTION " + sLike + ")";
        }
        if (Session["search_channel_type"] != null && Session["search_channel_type"].ToString() != "" && Session["search_channel_type"].ToString() != "-1")
        {
            theTable += " and ";
            if (Session["search_channel_type"].ToString().Trim() != "0")
                theTable += ODBCWrapper.Parameter.NEW_PARAM("c.WATCHER_ID", "=", 0);
            else
                theTable += ODBCWrapper.Parameter.NEW_PARAM("c.WATCHER_ID", "<>", 0);
        }
        theTable += "and ";
        theTable += ODBCWrapper.Parameter.NEW_PARAM("c.group_id", "=", nGroupID);

        theTable += @") q LEFT JOIN pics p 
              ON p.id = q.pic_id and ";

        theTable += PageUtils.GetStatusQueryPart("p");
        theTable += @" LEFT JOIN subscriptions s 
              ON s.id = q.subscription_id ";

        if (sOrderBy != "")
        {
            theTable += " order by ";
            theTable += sOrderBy;
        }
        else
        {
            theTable += " order by q.order_num,q.s_id desc";
        }

        theTable.AddHiddenField("ID");
        theTable.AddHiddenField("status");
        theTable.AddOrderByColumn("Name", "q.NAME");
        theTable.AddOrderByColumn("ID", "q.s_id");
        theTable.AddOrderByColumn("State", "q.s_desc");
        theTable.AddOrderByColumn("Channel Type", "q.channel_type");
        theTable.AddOrderByColumn("Channel ID", "q.s_id");
        theTable.AddImageField("Pic");
        theTable.AddTechDetails("channels");
        theTable.AddOrderNumField("channels", "id", "order_num", "Order Number");
        theTable.AddHiddenField("order_num");
        theTable.AddEditorRemarks("channels");
        theTable.AddHiddenField("EDITOR_REMARKS");
        theTable.AddHiddenField("channel_type");
        theTable.AddActivationField("channels", "adm_channels.aspx");
        theTable.AddHiddenField("pic_Id");
        theTable.AddHiddenField("is_active");

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_channels_media.aspx", "Asset", "");
            linkColumn1.AddQueryStringValue("channel_id", "field=id");
            linkColumn1.AddQueryStringValue("type_id", "field=channel_type");
            theTable.AddLinkColumn(linkColumn1);
        }
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_statistics_views.aspx", "Statistics", "");
            linkColumn1.AddQueryStringValue("channel_id", "field=id");
            theTable.AddLinkColumn(linkColumn1);
        }
        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_channels_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("channel_id", "field=id");
            linkColumn1.AddQueryStringValue("type_id", "field=channel_type");
            theTable.AddLinkColumn(linkColumn1);
        }
        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "channels");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "6");
            linkColumn.AddQueryStringValue("sub_menu", "2");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "ων");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "channels");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "6");
            linkColumn.AddQueryStringValue("sub_menu", "2");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "ων");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage(LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=id");
            linkColumn.AddQueryStringValue("table", "channels");
            linkColumn.AddQueryStringValue("confirm", "false");
            linkColumn.AddQueryStringValue("main_menu", "6");
            linkColumn.AddQueryStringValue("sub_menu", "2");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "ων");
            theTable.AddLinkColumn(linkColumn);
        }
    }

    public string GetPageContent(string sOrderBy, string sPageNum, string search_free, string search_channel_type)
    {
        if (search_free != "")
            Session["search_free"] = search_free.Replace("'", "''");
        else if (Session["search_save"] == null)
            Session["search_free"] = "";

        if (search_channel_type != "-1")
            Session["search_channel_type"] = search_channel_type;
        else if (Session["search_save"] == null)
            Session["search_channel_type"] = "";

        if (sOrderBy != "")
            Session["order_by"] = sOrderBy;
        else if (Session["search_save"] == null)
            Session["order_by"] = "";
        else
            sOrderBy = Session["order_by"].ToString();

        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();

        DBTableWebEditor theTable = new DBTableWebEditor(true, true, false, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 20);
        FillTheTableEditor(ref theTable, sOrderBy);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);
        theTable.Finish();
        theTable = null;
        return sTable;
    }

    public void GetCahannelsIFrame()
    {
        string sRet = "<IFRAME SRC=\"admin_tree_player.aspx";
        sRet += "\" WIDTH=\"800px\" HEIGHT=\"300px\" FRAMEBORDER=\"0\"></IFRAME>";
        Response.Write(sRet);
    }

    public void UpdateOnOffStatus(string theTableName, string sID, string sStatus)
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        List<int> idsToUpdate = new List<int>() { int.Parse(sID) };

        eAction eAction;
        int nAction = int.Parse(sStatus);
        eAction = (nAction == 0) ? eAction = eAction.Delete : eAction = eAction.Update;

        bool result = ImporterImpl.UpdateChannel(nGroupID, idsToUpdate, eAction);
    }
}
