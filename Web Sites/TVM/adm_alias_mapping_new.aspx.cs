using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;

public partial class adm_alias_mapping_new : System.Web.UI.Page
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
                int id = 0;
                if (Session["alias_mapping_id"] != null && Session["alias_mapping_id"].ToString() != "")
                {
                    id = int.Parse(Session["alias_mapping_id"].ToString());
                }
                int aliasId = 0;
                    int filedId = 0;
                        int type = 0;
                        int groupId = LoginManager.GetLoginGroupID();

                        PageFiled(ref aliasId, ref type, ref filedId);

                        if (type == 0 || filedId == 0 || aliasId == 0)
                        {
                            Session["error_msg_s"] = "Error-empty fields";
                            Session["error_msg"] = "Error-empty fields";
                        }
                        else
                        {
                            if (id == 0)
                            {
                                //insert new
                                InsertAlias(aliasId, type, filedId, groupId, ref id);
                                Session["alias_mapping_id"] = id;
                            }
                            else
                            {
                                //update
                                UpdateAlias(aliasId, type, filedId, id);
                            }
                        }
                        EndOfAction();

                        return;
             
            }
            m_sMenu = TVinciShared.Menu.GetMainMenu(5, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, true);
            if (Request.QueryString["alias_mapping_id"] != null &&
                Request.QueryString["alias_mapping_id"].ToString() != "")
            {
                Session["alias_mapping_id"] = int.Parse(Request.QueryString["alias_mapping_id"].ToString());

            }
            else
                Session["alias_mapping_id"] = 0;

            if (Session["error_msg_s"] != null && Session["error_msg_s"].ToString() != "")
            {
                lblError.Visible = true;
                lblError.Text = Session["error_msg_s"].ToString();
                Session["error_msg_s"] = null;
            }
            else
            {
                lblError.Visible = false;
                lblError.Text = "";
            }

        }
    }
    protected void InsertAlias( int aliasId, int type, int filedId,int groupId, ref int  id)
    {
        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("alias_mapping");
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 1);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("alias_id", "=", aliasId);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("field_id", "=", filedId);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("type", "=", type);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", groupId);
        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("updater_id", "=", LoginManager.GetLoginID());
        
        insertQuery.Execute();
        insertQuery.Finish();
        insertQuery = null;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select id,status from alias_mapping where is_active=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("alias_id", "=", aliasId);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("field_id", "=", filedId);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("type", "=", type);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", groupId);
        selectQuery += "and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);

        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                id = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;
    }

    protected void UpdateAlias(int aliasId, int type, int filedId, int id)
    {
        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("alias_mapping");
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("alias_id", "=", aliasId);
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("field_id", "=", filedId);
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("updater_id", "=", LoginManager.GetLoginID()); ;
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("type", "=", type);
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.UtcNow);
        updateQuery += "where ";
        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", id);
        updateQuery.Execute();
        updateQuery.Finish();
        updateQuery = null;
    }

    private void PageFiled(ref int aliasId, ref int type, ref int filedId)
    {
        System.Collections.Specialized.NameValueCollection coll = HttpContext.Current.Request.Form;
        if (coll["table_name"] == null)
        {
            HttpContext.Current.Session["error_msg"] = "missing table name - cannot update";
        }
        else
        {
            int nCount = coll.Count;
            int nCounter = 0;         

            try
            {
                while (nCounter < nCount)
                {
                    try
                    {
                        if (coll[nCounter.ToString() + "_fieldName"] != null)
                        {
                            string sFieldName = coll[nCounter.ToString() + "_fieldName"].ToString();
                            string sVal = "";
                            if (coll[nCounter.ToString() + "_val"] != null)
                            {
                                sVal = coll[nCounter.ToString() + "_val"].ToString();
                            }
                            #region case
                            switch (sFieldName)
                            {
                                case "AliasName":
                                    if (!string.IsNullOrEmpty(sVal))
                                    {
                                        aliasId = int.Parse(sVal);
                                    }
                                    break;
                                case "Type":
                                    if (!string.IsNullOrEmpty(sVal))
                                    {
                                        type = int.Parse(sVal);
                                    }
                                    break;
                                case "relatedToTag":
                                    if (!string.IsNullOrEmpty(sVal) && type == 3)
                                    {
                                        filedId = int.Parse(sVal);
                                    }
                                    break;
                                case "relatedToMeta":
                                    if (!string.IsNullOrEmpty(sVal) && type == 2)
                                    {
                                        filedId = int.Parse(sVal);
                                    }
                                    break;                               
                                default:
                                    break;

                            }
                            #endregion
                        }

                    }
                    catch (Exception ex)
                    {
                        break;
                    }

                    nCounter++;
                }
            }
            catch (Exception)
            {
            }
        }
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Alias Mapping";
        if (Session["alias_mapping_id"] != null && Session["alias_mapping_id"].ToString() != "" && Session["alias_mapping_id"].ToString() != "0")
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
        if (Session["alias_mapping_id"] != null && Session["alias_mapping_id"].ToString() != "" && int.Parse(Session["alias_mapping_id"].ToString()) != 0)
            t = Session["alias_mapping_id"];
        string sBack = "adm_alias_mapping.aspx?search_save=1";

        DBRecordWebEditor theRecord = new DBRecordWebEditor("alias_mapping", "adm_table_pager", sBack, "", "ID", t, sBack, "");

        DataRecordDropDownField dr_alias_name = new DataRecordDropDownField("lu_alias", "name", "id", "", null, 60, true);
        dr_alias_name.SetNoSelectStr("---");
        dr_alias_name.setFiledName("AliasName");
        dr_alias_name.Initialize("Alias Name", "adm_table_header_nbg", "FormInput", "alias_id", false);
        theRecord.AddRecord(dr_alias_name);

        DataRecordRadioField dr_epg_field_types = new DataRecordRadioField("lu_epg_field_types", "Type", "id", "", null);
        dr_epg_field_types.SetSelectsQuery("SELECT  ID , Type as 'txt'  FROM TVinci.dbo.lu_epg_field_types where id > 1");
        dr_epg_field_types.setFiledName("Type");
        dr_epg_field_types.Initialize("Type", "adm_table_header_nbg", "FormInput", "type",true);
        dr_epg_field_types.SetDefault(2);
        theRecord.AddRecord(dr_epg_field_types);

        DataRecordDropDownField dr_meta_type_mapping = new DataRecordDropDownField("EPG_metas_types", "name", "id", "", null, 60, true);
        string sQueryMate = " SELECT  ID , name as 'txt'  FROM TVinci.dbo.EPG_metas_types where status = 1 and is_active = 1 and group_id = " + LoginManager.GetLoginGroupID();       
        dr_meta_type_mapping.SetSelectsQuery(sQueryMate);
        dr_meta_type_mapping.SetNoSelectStr("---");
        dr_meta_type_mapping.setFiledName("relatedToMeta");
        dr_meta_type_mapping.Initialize("related to meta", "adm_table_header_nbg", "FormInput", "field_id", false);
        theRecord.AddRecord(dr_meta_type_mapping);

        DataRecordDropDownField dr_tag_type_mapping = new DataRecordDropDownField("EPG_tags_types", "name", "id", "", null, 60, true);
        string sTagQuery = " SELECT  ID ,name as 'txt'  FROM TVinci.dbo.EPG_tags_types where status = 1 and is_active = 1 and group_id = " + LoginManager.GetLoginGroupID();
        dr_tag_type_mapping.SetSelectsQuery(sTagQuery);
        dr_tag_type_mapping.SetNoSelectStr("---");
        dr_tag_type_mapping.setFiledName("relatedToTag");
        dr_tag_type_mapping.Initialize("related to tag", "adm_table_header_nbg", "FormInput", "field_id", false);
        theRecord.AddRecord(dr_tag_type_mapping);


       string sTable = theRecord.GetTableHTML("adm_alias_mapping_new.aspx?submited=1");

        return sTable;
    }

    private void EndOfAction()
    {

        System.Collections.Specialized.NameValueCollection coll = HttpContext.Current.Request.Form;
        if (HttpContext.Current.Session["error_msg"] != null && HttpContext.Current.Session["error_msg"].ToString() != "")
        {
            if (coll["failure_back_page"] != null)
            {
                HttpContext.Current.Response.Write("<script>window.document.location.href='" + coll["failure_back_page"].ToString() + "';</script>");
            }
            else
            {
                HttpContext.Current.Response.Write("<script>window.document.location.href='login.aspx';</script>");
            }
        }
        else
        {
            if (HttpContext.Current.Request.QueryString["back_n_next"] != null)
            {
                HttpContext.Current.Session["last_page_html"] = null;
                string s = HttpContext.Current.Session["back_n_next"].ToString();
                HttpContext.Current.Response.Write("<script>window.document.location.href='" + s.ToString() + "';</script>");
                HttpContext.Current.Session["back_n_next"] = null;
            }
            else
            {
                if (coll["success_back_page"] != null)
                    HttpContext.Current.Response.Write("<script>window.document.location.href='" + coll["success_back_page"].ToString() + "';</script>");
                else
                    HttpContext.Current.Response.Write("<script>window.document.location.href='login.aspx';</script>");
            }
            CachingManager.CachingManager.RemoveFromCache("SetValue_" + coll["table_name"].ToString() + "_");
        }
    }
}