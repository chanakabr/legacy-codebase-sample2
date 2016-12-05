using ApiObjects;
using EpgBL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TVinciShared;

public partial class adm_epg_channels_schedule : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_epg_channels.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(6, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 3, false);
            if (Request.QueryString["search_save"] != null)
                Session["search_save"] = "1";
            else
                Session["search_save"] = null;

            if (Request.QueryString["epg_channel_id"] != null &&
                Request.QueryString["epg_channel_id"].ToString() != "")
            {
                Session["epg_channel_id"] = int.Parse(Request.QueryString["epg_channel_id"].ToString());
            }
            else if (Session["epg_channel_id"] == null || Session["epg_channel_id"].ToString() == "" || Session["epg_channel_id"].ToString() == "0")
            {
                LoginManager.LogoutFromSite("index.html");
                return;
            }
        }
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ":" + PageUtils.GetTableSingleVal("epg_channels", "NAME", int.Parse(Session["epg_channel_id"].ToString())).ToString() + ": EPG Channel schedule");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    public string GetTableCSV(string startD, string startM, string startY, string endD, string endM, string endY)
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();
        CBTableWebEditor<EPGChannelProgrammeObject> theTable = new CBTableWebEditor<EPGChannelProgrammeObject>(true, true, false, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);
        FillTheTableEditor(ref theTable, sOldOrderBy, startD, startM, startY, endD, endM, endY);

        string sCSVFile = theTable.OpenCSV();
        theTable.Finish();
        theTable = null;
        return sCSVFile;
    }

    protected void FillTheTableEditor(ref CBTableWebEditor<EPGChannelProgrammeObject> theTable, string sOrderBy, string startD, string startM, string startY, string endD, string endM, string endY)
    {
        DateTime tStart = DateUtils.GetDateFromStr(startD + "/" + startM + "/" + startY);
        DateTime tEnd = DateUtils.GetDateFromStr(endD + "/" + endM + "/" + endY);

        //get epg programs from CB
        int groupId = LoginManager.GetLoginGroupID();
        int parentGroupId = DAL.UtilsDal.GetParentGroupID(groupId);
        int channelID = int.Parse(Session["epg_channel_id"].ToString());

        List<int> epgIds = new List<int>();
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
        selectQuery += "SELECT ID FROM epg_channels_schedule with (nolock) where status<>2 and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("epg_channel_id", "=", channelID);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", ">=", tStart.AddDays(-1));
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "<=", tEnd);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupId);
       
        if (selectQuery.Execute("query", true) != null)
        {
            int count = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < count; i++)
            {
                epgIds.Add(ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "ID", i));
            }
        }
        selectQuery.Finish();
        selectQuery = null;

        TvinciEpgBL oEpgBL = new TvinciEpgBL(parentGroupId);
        List<EPGChannelProgrammeObject> Epgs = oEpgBL.GetEpgs(epgIds);
        theTable.SetData(Epgs);

        theTable.AddField("Name");
        theTable.AddImageField("Pic");
        theTable.AddField("Description");
        theTable.AddField("Identifier");
        theTable.AddField("Start Date");
        theTable.AddField("End Date");
        theTable.AddField("State");

        theTable.AddHiddenField("pic_id");
        theTable.AddHiddenField("eci");     //EPG_CHANNEL ID
        theTable.AddHiddenField("m_id");    //Epg program ID
        theTable.AddHiddenField("id");      //Epg program ID
        theTable.AddHiddenField("status");
        theTable.AddHiddenField("is_active");

        theTable.AddOnOffField("On/Off", "Epg~~|~~On Off~~|~~m_id~~|~~On~~|~~Off");

        if (LoginManager.IsActionPermittedOnPage("adm_epg_channels.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT))
        {
            DataTableLinkColumn linkColumn1 = new DataTableLinkColumn("adm_epg_channels_schedule_new.aspx", "Edit", "");
            linkColumn1.AddQueryStringValue("epg_channels_schedule_id", "field=m_id");
            linkColumn1.AddQueryStringValue("epg_channels_id", "field=eci");
            theTable.AddLinkColumn(linkColumn1);
        }

        if (LoginManager.IsActionPermittedOnPage("adm_epg_channels.aspx", LoginManager.PAGE_PERMISION_TYPE.REMOVE))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_remove.aspx", "Delete", "STATUS=1;STATUS=3");
            linkColumn.AddQueryStringValue("id", "field=m_id");
            linkColumn.AddQueryStringValue("table", "Epg");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "6");
            linkColumn.AddQueryStringValue("sub_menu", "3");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "שם");
            linkColumn.AddQueryStringValue("db", "couchbase");
            linkColumn.AddQueryStringValue("m_sBasePageURL", "adm_epg_channels.aspx");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage("adm_epg_channels.aspx", LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Confirm", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=m_id");
            linkColumn.AddQueryStringValue("table", "Epg");
            linkColumn.AddQueryStringValue("confirm", "true");
            linkColumn.AddQueryStringValue("main_menu", "6");
            linkColumn.AddQueryStringValue("sub_menu", "3");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "שם");
            linkColumn.AddQueryStringValue("db", "couchbase");
            linkColumn.AddQueryStringValue("m_sBasePageURL", "adm_epg_channels.aspx");
            theTable.AddLinkColumn(linkColumn);
        }

        if (LoginManager.IsActionPermittedOnPage("adm_epg_channels.aspx", LoginManager.PAGE_PERMISION_TYPE.PUBLISH))
        {
            DataTableLinkColumn linkColumn = new DataTableLinkColumn("adm_generic_confirm.aspx", "Cancel", "STATUS=3;STATUS=4");
            linkColumn.AddQueryStringValue("id", "field=m_id");
            linkColumn.AddQueryStringValue("table", "Epg");
            linkColumn.AddQueryStringValue("confirm", "false");
            linkColumn.AddQueryStringValue("main_menu", "6");
            linkColumn.AddQueryStringValue("sub_menu", "3");
            linkColumn.AddQueryStringValue("rep_field", "NAME");
            linkColumn.AddQueryStringValue("rep_name", "שם");
            linkColumn.AddQueryStringValue("db", "couchbase");
            linkColumn.AddQueryStringValue("m_sBasePageURL", "adm_epg_channels.aspx");
            theTable.AddLinkColumn(linkColumn);
        }
    }

    public string GetPageContent(string sOrderBy, string sPageNum, string startD, string startM, string startY, string endD, string endM, string endY)
    {
        string sOldOrderBy = "";
        if (Session["order_by"] != null)
            sOldOrderBy = Session["order_by"].ToString();
        CBTableWebEditor<EPGChannelProgrammeObject> theTable = new CBTableWebEditor<EPGChannelProgrammeObject>(true, true, true, "", "adm_table_header", "adm_table_cell", "adm_table_alt_cell", "adm_table_link", "adm_table_pager", "adm_table", sOldOrderBy, 50);

        FillTheTableEditor(ref theTable, sOrderBy, startD, startM, startY, endD, endM, endY);

        FillDataTable(ref theTable);

        string sTable = theTable.GetPageHTML(int.Parse(sPageNum), sOrderBy);
        Session["ContentPage"] = "adm_epg_channels.aspx?search_save=1";

        theTable.Finish();
        theTable = null;
        return sTable;
    }

    public void GetSearchPannel()
    {
        DateTime dStart = DateTime.UtcNow;
        DateTime dEnd = DateTime.UtcNow.AddDays(1);
        string sRet = "<tr>\r\n";
        sRet += "<td colspan=\"2\">\r\n";
        sRet += "<table>\r\n";
        sRet += "<tr>\r\n";
        sRet += "<td>&nbsp;</td>\r\n";
        sRet += "</tr>\r\n";
        sRet += "<tr>\r\n";
        sRet += "<td class=\"FormError\"><div id=\"error_place\"></div></td>\r\n";
        sRet += "</tr>\r\n";
        sRet += "<tr>\r\n";
        sRet += "<td>&nbsp;</td>\r\n";
        sRet += "</tr>\r\n";
        sRet += "<tr>\r\n";
        sRet += "<td class=\"adm_table_header_nbg\" style=\"vertical-align: middle;\">From: </td>\r\n";
        sRet += "<td><input class=\"FormInput\" type=\"text\" size=\"2\" maxlength=\"2\" value=\"" + dStart.Day.ToString() + "\" id=\"s_day\" /> </td>\r\n";
        sRet += "<td>/</td>\r\n";
        sRet += "<td><input class=\"FormInput\" type=\"text\" size=\"2\" maxlength=\"2\" id=\"s_mounth\" value=\"" + dStart.Month.ToString() + "\" /> </td>\r\n";
        sRet += "<td>/</td>\r\n";
        sRet += "<td><input class=\"FormInput\" type=\"text\" size=\"4\" maxlength=\"4\" id=\"s_year\" value=\"" + dStart.Year.ToString() + "\" /> </td>\r\n";
        sRet += "<td>&nbsp;&nbsp;&nbsp;&nbsp;</td>\r\n";
        sRet += "<td class=\"adm_table_header_nbg\"  style=\"vertical-align: middle;\">To: </td>\r\n";
        sRet += "<td><input maxlength=\"2\" class=\"FormInput\" type=\"text\" size=\"2\" id=\"s_day_to\" value=\"" + dEnd.Day.ToString() + "\" /> </td>\r\n";
        sRet += "<td>/</td>\r\n";
        sRet += "<td><input maxlength=\"2\" class=\"FormInput\" type=\"text\" size=\"2\" id=\"s_mounth_to\" value=\"" + dEnd.Month.ToString() + "\" /> </td>\r\n";
        sRet += "<td>/</td>\r\n";
        sRet += "<td><input maxlength=\"4\" class=\"FormInput\" type=\"text\" size=\"4\" id=\"s_year_to\" value=\"" + dEnd.Year.ToString() + "\" /> </td>\r\n";
        sRet += "<td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</td>\r\n";
        sRet += "<td><a href=\"javascript:reloadPage();\" class=\"btn\">Go</a></td>\r\n";
        sRet += "</tr>\r\n";
        sRet += "</table>\r\n";
        sRet += "</td>\r\n";
        sRet += "</tr>\r\n";
        Response.Write(sRet);
    }

    private void FillDataTable(ref CBTableWebEditor<EPGChannelProgrammeObject> theTable)
    {
        DataTable dt = new DataTable();

        // Build DataTable
        dt.Columns.Add("Name");
        dt.Columns.Add("Pic");
        dt.Columns.Add("Description");
        dt.Columns.Add("DOW");
        dt.Columns.Add("Start Date");
        dt.Columns.Add("End Date");
        dt.Columns.Add("Identifier");
        dt.Columns.Add("State");

        foreach (DictionaryEntry item in theTable.GetHiddenFields())
        {
            string sKey = item.Key.ToString();
            dt.Columns.Add(sKey);
            dt.Columns[sKey].ColumnMapping = MappingType.Hidden;
        }
        foreach (DictionaryEntry item in theTable.GetOnOffFields())
        {
            dt.Columns.Add(item.Key.ToString());
        }

        List<EPGChannelProgrammeObject> lEpg = theTable.GetData();
        if (lEpg != null && lEpg.Count > 0)
        {
            //get all media description from DB 
            List<string> lEpgIdentifier = lEpg.Select(x => x.EPG_IDENTIFIER).ToList();
            List<KeyValuePair<string, string>> lMediaDescription = DAL.TvmDAL.GetMediaDescription(lEpgIdentifier);

            #region Fill DataTable Rows
            DataRow row;
            foreach (EPGChannelProgrammeObject epg in lEpg)
            {
                row = dt.NewRow();

                string sDate = string.Empty;
                DateTime DOW = DateTime.MinValue;
                if (!string.IsNullOrEmpty(epg.START_DATE))
                {
                    try
                    {
                        sDate = epg.START_DATE.Substring(0, epg.START_DATE.IndexOf(" "));
                        char[] delimiters = new char[] { '/' };
                        string[] sSplitDate = sDate.Split(delimiters);
                        DOW = new DateTime(int.Parse(sSplitDate[2]), int.Parse(sSplitDate[1]), int.Parse(sSplitDate[0]));
                    }
                    catch
                    {
                    }
                }

                DayOfWeek eDayOfWeek = DOW.DayOfWeek;
                switch (eDayOfWeek)
                {
                    case DayOfWeek.Friday:
                        row["DOW"] = "Fri";
                        break;
                    case DayOfWeek.Monday:
                        row["DOW"] = "Mon";
                        break;
                    case DayOfWeek.Saturday:
                        row["DOW"] = "Sat";
                        break;
                    case DayOfWeek.Sunday:
                        row["DOW"] = "Sun";
                        break;
                    case DayOfWeek.Thursday:
                        row["DOW"] = "Thu";
                        break;
                    case DayOfWeek.Tuesday:
                        row["DOW"] = "Tue";
                        break;
                    case DayOfWeek.Wednesday:
                        row["DOW"] = "Wed";
                        break;
                    default:
                        break;
                }
                row["Name"] = epg.NAME;
                if (epg.PIC_ID == 0)
                {
                    //Get pic data
                    int picId = 0;
                    string imgUrl = PageUtils.GetEpgChannelsSchedulePicImageUrlByEpgIdentifier(epg.EPG_IDENTIFIER, epg.EPG_CHANNEL_ID, out picId);
                    row["pic_id"] = picId;
                    row["Pic"] = imgUrl;  
                }
                else
                {
                    row["pic_id"] = epg.PIC_ID;
                    row["Pic"] = epg.PIC_URL;                
                }
                row["Description"] = epg.DESCRIPTION;
                row["Identifier"] = epg.EPG_IDENTIFIER;
                row["Start Date"] = epg.START_DATE;
                row["End Date"] = epg.END_DATE;

                switch (epg.STATUS) // TO DO complite from DB ???
                {
                    case "1":
                        row["State"] = "Active";
                        break;
                    case "2":
                        row["State"] = "Not active";
                        break;
                    case "3":
                        row["State"] = "Waiting for activation";
                        break;
                    case "4":
                        row["State"] = "Waiting for delete";
                        break;
                    default:
                        break;
                }

                row["eci"] = epg.EPG_CHANNEL_ID;
                row["m_id"] = epg.EPG_ID;
                row["id"] = epg.EPG_ID;
                row["status"] = epg.STATUS;
                row["is_active"] = epg.IS_ACTIVE;

                row["On/Off"] = (epg.IS_ACTIVE.ToLower() == "true") ? 1 : 0;

                dt.Rows.Add(row);
            }
            #endregion
        }

        theTable.FillDataTable(dt);
    }

    public static string GetDescription(string item, List<KeyValuePair<string, string>> list)
    {
        foreach (var x in list)
        {
            if (x.Key == item)
                return x.Value;
            return string.Empty;
        }
        return string.Empty;
    }

}
