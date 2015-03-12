using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_group_regions_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;

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
                Int32 regionId = DBManipulator.DoTheWork();
                Session["region_id"] = regionId.ToString();
                Int32 regionIdDesc = 0;
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select id from linear_channels_regions where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", regionId);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        regionIdDesc = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                }
                selectQuery.Finish();
                selectQuery = null;
                int groupID = LoginManager.GetLoginGroupID();
                if (regionIdDesc != 0)
                {
                    ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("linear_channels_regions");
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("Name", "=", Request.Form["0_val"].ToString());
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("EXTERNAL_ID", "=", Request.Form["1_val"].ToString());
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", LoginManager.GetLoginID());
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                    updateQuery += " where ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", regionIdDesc);
                    updateQuery.Execute();
                    updateQuery.Finish();
                    updateQuery = null;
                }
                else
                {
                    string name = Request.Form["0_val"].ToString();
                    string extrernalId = Request.Form["1_val"].ToString();
                    ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("linear_channels_regions");
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Name", "=", name);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("EXTERNAL_ID", "=", extrernalId);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", LoginManager.GetLoginID());
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CREATE_DATE", "=", DateTime.UtcNow);
                    if (insertQuery.Execute())
                    {
                        if (Session["media_ids"] != null && Session["media_ids"] is List<int>)
                        {
                            
                            regionId = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("linear_channels_regions", "id", "external_id", "=", extrernalId).ToString());
                            if (regionId != null)
                            {
                                List<int> mediaIds = Session["media_ids"] as List<int>;
                                InsertRegionMedias(mediaIds, regionId);
                            }
                        }
                    }
                    insertQuery.Finish();
                    insertQuery = null;
                }
                return;
            }
            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            
            if (Request.QueryString["region_id"] != null &&
                Request.QueryString["region_id"].ToString() != "")
            {
                Session["region_id"] = int.Parse(Request.QueryString["region_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("linear_channels_regions", "group_id", int.Parse(Request.QueryString["region_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["region_id"] = 0;
        }
        Int32 nGroupID = LoginManager.GetLoginGroupID();
    }

    private void InsertRegionMedias(List<int> mediaIds, int regionId)
    {
        if (mediaIds != null)
        {
            foreach (var id in mediaIds)
            {
                InserteMediaRegion(id, regionId);
            }
        }
    }


    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Regions: ";
        if (Session["region_id"] != null && Session["region_id"].ToString() != "" && Session["region_id"].ToString() != "0")
        {
            object sSubName = ODBCWrapper.Utils.GetTableSingleVal("linear_channels_regions", "name", "id", "=", int.Parse(Session["region_id"].ToString()));
            if (sSubName != null && sSubName != DBNull.Value)
                sRet += sSubName.ToString();
            sRet += " - Edit";
        }
        else
            sRet += " - New";
        Response.Write(sRet);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }


    protected string GetCurrentValue(string sField, string sTable, Int32 regionId)
    {
        string sRet = "";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select " + sField + " from " + sTable + " where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", regionId);
        selectQuery += " and is_active=1 and status=1"; 
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                object oRet = selectQuery.Table("query").DefaultView[0].Row[sField];
                if (oRet != null && oRet != DBNull.Value)
                    sRet = oRet.ToString();
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return sRet;
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object t = null; ;
        if (Session["region_id"] != null && Session["region_id"].ToString() != "" && int.Parse(Session["region_id"].ToString()) != 0)
            t = Session["region_id"];
        string sBack = "adm_group_regions.aspx?search_save=1";
        DBRecordWebEditor theRecord = new DBRecordWebEditor("linear_channels_regions", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordLongTextField dr_Name = new DataRecordLongTextField("ltr", true, 60, 4);
        dr_Name.Initialize("Name", "adm_table_header_nbg", "FormInput", "", false);
        
        DataRecordLongTextField dr_ExternalID = new DataRecordLongTextField("ltr", true, 60, 4);
        dr_ExternalID.Initialize("External ID", "adm_table_header_nbg", "FormInput", "", false);


        if (Session["region_id"] != null && Session["region_id"].ToString() != "0")
        {
            dr_Name.SetValue(GetCurrentValue("name", "linear_channels_regions", int.Parse(Session["region_id"].ToString())));
            dr_ExternalID.SetValue(GetCurrentValue("external_id", "linear_channels_regions", int.Parse(Session["region_id"].ToString())));
        }
        else
        {
            dr_Name.SetValue("");
            dr_ExternalID.SetValue("");
        }

        theRecord.AddRecord(dr_Name);
        theRecord.AddRecord(dr_ExternalID);

        

        string sTable = theRecord.GetTableHTML("adm_group_regions_new.aspx?submited=1");

        return sTable;
    }

    public string initDualObj()
    {
        string sRet = "";
        sRet += "Linear Channels in this Region";
        sRet += "~~|~~";
        sRet += "All Linear Channels";
        sRet += "~~|~~";
        sRet += "<root>";

        int regionId = 0;
        if (Session["region_id"] != null && Session["region_id"].ToString() != "0")
        {
            regionId = int.Parse(Session["region_id"].ToString());
        }
        ODBCWrapper.DataSetSelectQuery channelsSelectQuery = new ODBCWrapper.DataSetSelectQuery();
        channelsSelectQuery += "select m.id, m.name from media m inner join media_types mt on m.MEDIA_TYPE_ID = mt.id and m.STATUS = 1 and mt.IS_LINEAR = 1 and ";
        channelsSelectQuery += " m.group_id " + PageUtils.GetFullChildGroupsStr(LoginManager.GetLoginGroupID(), "");
        if (channelsSelectQuery.Execute("query", true) != null)
        {
            DataTable regionChannels = null;
            if (regionId != 0)
            {
                ODBCWrapper.DataSetSelectQuery regionChannelsSelectQuery = new ODBCWrapper.DataSetSelectQuery();
                regionChannelsSelectQuery += "select id, media_id from media_regions where status = 1 and ";
                regionChannelsSelectQuery += ODBCWrapper.Parameter.NEW_PARAM("region_id", "=", regionId);
                if (regionChannelsSelectQuery.Execute("query", true) != null)
                {
                    regionChannels = regionChannelsSelectQuery.Table("query");
                }
                regionChannelsSelectQuery.Finish();
                regionChannelsSelectQuery = null;
            }
            Int32 nCount = channelsSelectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string sID = ODBCWrapper.Utils.GetStrSafeVal(channelsSelectQuery, "id", i);
                string sTitle = ODBCWrapper.Utils.GetStrSafeVal(channelsSelectQuery, "name", i);
                DataRow drChannel = null;
                if (regionChannels != null)
                {
                    drChannel = regionChannels.Select(string.Format("media_id = {0}", sID)).FirstOrDefault();
                }
                if (drChannel != null)
                    sRet += "<item id=\"" + sID + "\"  title=\"" + sTitle + "\" description=\"" + sTitle + "\" inList=\"true\" />";
                else
                    sRet += "<item id=\"" + sID + "\"  title=\"" + sTitle + "\" description=\"" + sTitle + "\" inList=\"false\" />";
            }
        }
            
        channelsSelectQuery.Finish();
        channelsSelectQuery = null;


        sRet += "</root>";
        
        return sRet;

    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected Int32 GetRegionMediaID(Int32 mediaId, Int32 regionId, ref Int32 status)
    {
        Int32 result = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("CONNECTION_STRING");
        selectQuery += "select id from media_regions where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("media_id", "=", mediaId);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("region_id", "=", regionId);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                result = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                status = int.Parse(selectQuery.Table("query").DefaultView[0].Row["STATUS"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return result;
    }

    public string changeItemStatus(string id, string action)
    {
        if (Session["region_id"] != null && Session["region_id"].ToString() != "0")
        {
            int regionId = int.Parse(Session["region_id"].ToString());
            Int32 status = 0;
            int mediaID = int.Parse(id);
            Int32 mediaRegionId = GetRegionMediaID(mediaID, regionId, ref status);

            if (mediaRegionId != 0)
            {
                if (status == 0)
                    UpdateMediaRegions(mediaRegionId, 1, mediaID, regionId);
                else
                    UpdateMediaRegions(mediaRegionId, 0, mediaID, regionId);
            }
            else
            {
                InserteMediaRegion(mediaID, regionId);
            }
        }
        else
        {
            List<int> mediaList = new List<int>();
            if (Session["media_ids"] != null && Session["media_ids"] is List<int>)
            {
                mediaList = Session["media_ids"] as List<int>;
            }
            int mediaID = int.Parse(id);
            mediaList.Add(mediaID);
            Session["media_ids"] = mediaList;
        }
        return "";
    }

    protected void InserteMediaRegion(Int32 mediaID, Int32 regionId)
    {
        bool bInsert = false;
        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("media_regions");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", mediaID);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("REGION_ID", "=", regionId);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", LoginManager.GetLoginID());
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CREATE_DATE", "=", DateTime.UtcNow);

        bInsert = insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;
    }

    protected void UpdateMediaRegions(Int32 id, Int32 status, int mediaId, int regionId)
    {
        bool bUpdate = false;
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("media_regions");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", status);
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", LoginManager.GetLoginID());
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
        updateQuery += "where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", id);
        bUpdate = updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;
    }
}