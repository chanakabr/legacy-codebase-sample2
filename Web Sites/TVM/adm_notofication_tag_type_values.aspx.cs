using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;
using System.Data;
using ODBCWrapper;


public partial class adm_notofication_tag_type_values : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;

    protected void Page_Load(object sender, EventArgs e)
    {

        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        //if (LoginManager.IsPagePermitted("adm_tvm_notifications.aspx") == false)
        //    LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;


        string tagTypeID = string.Empty;
        string nID = string.Empty;

        #region  if (!IsPostBack)
        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
            {
                //Save the tags in notification_parameters table at MessageBox DB
                nID = Session["nID"].ToString();
                
                //get all tagsids from Kaltura DB
                System.Collections.Specialized.NameValueCollection coll = HttpContext.Current.Request.Form;

                List<string> tagIds = null;
                string nGroupIds = GetRegularChildGroupsStr(LoginManager.GetLoginGroupID(), "MAIN_CONNECTION_STRING");
                string sQuery = " select id  from	Tvinci.dbo.tags  ";
                sQuery += " where	status in (1,3,4) and	group_id  in (" + nGroupIds + ")";
                sQuery += " and	TAG_TYPE_ID = " + Session["TagTypeID"];

                string sTagValues = coll["0_val"];

                if (sTagValues.LastIndexOf(';') == (sTagValues.Length - 1))
                    sTagValues = sTagValues.Remove(sTagValues.Length - 1, 1);

                string[] valuesArray = sTagValues.Split(';');
                string values = string.Empty;
                for (int i = 0; i < valuesArray.Count(); i++)
                {
                    values += "'" + valuesArray[i] + "'" + " , ";
                }
                values = values.Remove(values.LastIndexOf(','), 1);

                sQuery += " and VALUE in (" + values + ")";

                ODBCWrapper.DataSetSelectQuery selectQueryTags = new ODBCWrapper.DataSetSelectQuery();
                selectQueryTags.SetConnectionKey("MAIN_CONNECTION_STRING");
                selectQueryTags += sQuery;
                if (selectQueryTags.Execute("query", true) != null)
                {
                    Int32 nCount = selectQueryTags.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        tagIds = new List<string>();
                        for (int i = 0; i < nCount; i++)
                        {
                            tagIds.Add(ODBCWrapper.Utils.GetSafeStr(selectQueryTags.Table("query").DefaultView[i].Row["id"]));
                        }
                    }
                }
                selectQueryTags.Finish();
                selectQueryTags = null;
                //Insert/Delete tags to notification_parameters
                if (tagIds != null)
                {
                    InsertNotificationsParameters(tagIds, nID, nGroupIds, LoginManager.GetLoginGroupID());
                }
            }


            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(14, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);

            if (Request.QueryString["TagTypeID"] != null && Request.QueryString["TagTypeID"].ToString() != "")
            {
                Session["new_id"] = null;
                tagTypeID = Request.QueryString["TagTypeID"].ToString();
                Session["TagTypeID"] = tagTypeID;
            }          

            if (Request.QueryString["nID"] != null && Request.QueryString["nID"].ToString() != "")
            {
                nID = Request.QueryString["nID"].ToString();
                Session["nID"] = nID;
            }
          
        }
        #endregion
    }




    private void InsertNotificationsParameters(List<string> tagsIDs, string nID, string nGroupIds, int nGroupID)
    {   
        Int32 index = 0;
        string tagVal = string.Empty;       
        List<int> idsUpdateList = null;


        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery.SetConnectionKey("notifications_connection");
        selectQuery += "select id, value  from notifications_parameters where status = 1 and is_active = 1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("notification_id", "=", nID);
        selectQuery += "order by id desc";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                for (int i = 0; i < nCount; i++)
                {
                    index = int.Parse(selectQuery.Table("query").DefaultView[i].Row["id"].ToString());
                    tagVal = selectQuery.Table("query").DefaultView[i].Row["value"].ToString();

                    //If the tagValue already in the table - do nothing - no need to save it again
                    if (tagsIDs.Contains(tagVal))
                        tagsIDs.Remove(tagVal);
                    
                    //if tagvalue in DB table but not in the list - then need to update it status .
                    else
                    {
                        if (idsUpdateList == null)
                            idsUpdateList = new List<int>();
                        idsUpdateList.Add(index);
                    }
                }
            }
        }
        selectQuery.Finish();
        selectQuery = null;

        if (idsUpdateList != null)
        {
            for (int i = 0; i < idsUpdateList.Count(); i++)
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("notifications_parameters");
                updateQuery.SetConnectionKey("notifications_connection");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 2);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("Update_date", "=", DateTime.Now);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("Updater_ID", "=", LoginManager.GetLoginID());
                updateQuery += "where";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", idsUpdateList[i]);
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }
        }

        if (tagsIDs != null)
        {
            DataTable nDataTable = new DataTable();
            FillParametersDataTable(tagsIDs, nID, nGroupID, ref nDataTable);

            if (nDataTable != null)
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("notifications_parameters");
                insertQuery.SetConnectionKey("notifications_connection");
                try
                {
                    insertQuery.InsertBulk("notifications_parameters", nDataTable);
                }
                catch
                {
                    //TBD: Write to log
                }
                finally
                {
                    if (insertQuery != null)
                    {
                        insertQuery.Finish();
                    }
                    insertQuery = null;
                }
                nDataTable.Clear();
            }
        }
    }

    private void FillParametersDataTable(List<string> tagList, string nID, int nGroupID, ref DataTable nDataTable)
    {

        nDataTable = new DataTable();
        nDataTable.Columns.Add("notification_id", typeof(long));
        nDataTable.Columns.Add("value", typeof(string));
        nDataTable.Columns.Add("group_id", typeof(long));
        nDataTable.Columns.Add("updater_id", typeof(int));
        
        if (tagList != null && tagList.Count > 0)
        {
            foreach (string val in tagList)
            {
                DataRow row = nDataTable.NewRow();
                row["notification_id"] = nID;
                row["value"] = val;
                row["group_id"] = nGroupID;
                row["updater_id"] = LoginManager.GetLoginID();
                nDataTable.Rows.Add(row);
            }
        }
        else
        {
            nDataTable = null;
        }

    }
    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Notification Tag Values");
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
            return Session["LastContentPage"].ToString();
        }
        int tagTypeID = 0;
        int notificationID = 0;
        object t = null;
        object oNotificationID = null;

        try
        {
            if (Session["TagTypeID"] != null && Session["TagTypeID"].ToString() != "" && int.Parse(Session["TagTypeID"].ToString()) != 0)
            {
                tagTypeID = int.Parse(Session["TagTypeID"].ToString());
                t = Session["TagTypeID"];
            }
            if (Session["nId"] != null && Session["nId"].ToString() != "" && int.Parse(Session["nId"].ToString()) != 0)
            {
                notificationID = int.Parse(Session["nId"].ToString());
                oNotificationID = Session["nId"];
            }
        }
        catch
        {
        }
        string sBack = "adm_tvm_notifications.aspx";        
        DBRecordWebEditor theRecord = new DBRecordWebEditor("notifications", "adm_table_pager", sBack, "", "id", oNotificationID, sBack, "");
        
        AddTagsFields(ref theRecord, tagTypeID);
        theRecord.SetConnectionKey("notifications_connection");

        string sTable = theRecord.GetTableHTML("adm_notofication_tag_type_values.aspx?submited=1");
       
        return sTable;
    }

    /// <summary>
    /// Build the tags values by tag type id
    /// </summary>
    /// <param name="theRecord"></param>
    /// <param name="tagTypeID"></param>
    protected void AddTagsFields(ref DBRecordWebEditor theRecord, int tagTypeID)
    {
        string tagName = ODBCWrapper.Utils.GetTableSingleVal("media_tags_types", "NAME", tagTypeID).ToString();

        DataRecordMultiField dr_tags = new DataRecordMultiField("Tvinci.dbo.tags", "id", "id", "notifications_parameters", "notification_id", "value", false, "ltr", 60, "tags");
        dr_tags.Initialize(tagName, "adm_table_header_nbg", "FormInput", "VALUE", false);  
        string sQuery = " select top 25 VALUE as txt,id as val from Tvinci.dbo.tags where  status in (1,3,4) and group_id " + PageUtils.GetFullChildGroupsStr(LoginManager.GetLoginGroupID(), "MAIN_CONNECTION_STRING");
        sQuery += " and TAG_TYPE_ID = " + tagTypeID;
        sQuery += "  order by id ";
        dr_tags.SetConnectionKey("notifications_connection");
        dr_tags.SetCollectionQuery(sQuery);
        theRecord.AddRecord(dr_tags);
    }

    private string GetRegularChildGroupsStr(int nGroupID, string sConnKey)
    {
        string groups = string.Empty;
        List<string> lGroups = new List<string>();
        DataTable dt = DAL.NotificationDal.GetRegularChildGroupsStr(nGroupID);
        if (dt != null && dt.DefaultView.Count > 0)
        {
            foreach (DataRow dr in dt.Rows)
            {
                lGroups.Add(ODBCWrapper.Utils.GetSafeStr(dr["group_id"]));
            }
            groups = string.Join(",", lGroups.ToArray());
        }
        return groups;
    }  
}