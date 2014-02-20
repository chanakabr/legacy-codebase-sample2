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

public partial class adm_video_popup_selector : System.Web.UI.Page
{
    static protected string m_sIDs;
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.aspx");
        if (LoginManager.IsPagePermitted("adm_media.aspx") == false)
            LoginManager.LogoutFromSite("login.aspx");
        if (LoginManager.IsActionPermittedOnPage("adm_media.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.aspx");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
            {
                string sIDs = Request.Form["ids_place"].ToString();
                Int32 nID = DBManipulator.DoTheWork();
                sIDs += nID.ToString() + ";";
                m_sIDs = sIDs;
                Session["m_sIDs"] = m_sIDs;
                return;
            }
            else if (Request.QueryString["pics_ids"] != null)
            {
                m_sIDs = Request.QueryString["pics_ids"].ToString();
            }
            else if (Session["m_sIDs"] != null)
            {
                m_sIDs = Session["m_sIDs"].ToString();
                Session["m_sIDs"] = null;
            }
            else
                m_sIDs = "";

            if (Request.QueryString["theID"] != null && Request.QueryString["theID"] != "")
                Session["theID"] = Request.QueryString["theID"].ToString();

            if (Request.QueryString["maxPics"] != null && Request.QueryString["maxPics"] != "")
                Session["maxPics"] = Request.QueryString["maxPics"].ToString();

            if (Request.QueryString["vidTable"] != null && Request.QueryString["vidTable"] != "")
                Session["vidTable"] = Request.QueryString["vidTable"].ToString();

            if (Request.QueryString["vidTableTags"] != null && Request.QueryString["vidTableTags"] != "")
                Session["vidTableTags"] = Request.QueryString["vidTableTags"].ToString();

            if (Request.QueryString["vidTableTagsRef"] != null && Request.QueryString["vidTableTagsRef"] != "")
                Session["vidTableTagsRef"] = Request.QueryString["vidTableTagsRef"].ToString();
        }
    }

    public void GetSendID()
    {
        if (Session["theID"] != null)
            Response.Write(Session["theID"].ToString());
    }

    public void GetVidTable()
    {
        if (Session["vidTable"] != null)
            Response.Write(Session["vidTable"].ToString());
    }

    public void GetMaxPics()
    {
        if (Session["maxPics"] != null)
            Response.Write(Session["maxPics"].ToString());
    }

    public void GetIDs()
    {
        if (Session["m_sIDs"] != null)
        {
            m_sIDs = Session["m_sIDs"].ToString();
        }

        Response.Write(m_sIDs);
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object t = null; ;
        DBRecordWebEditor theRecord = new DBRecordWebEditor("pics", "adm_table_pager", "adm_pic_popup_selector.aspx", "", "ID", t, "javascript:window.close();", "pic_id");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_name);

        DataRecordLongTextField dr_description = new DataRecordLongTextField("ltr", true, 60, 4);
        dr_description.Initialize("Description", "adm_table_header_nbg", "FormInput", "DESCRIPTION", false);
        theRecord.AddRecord(dr_description);

        DataRecordUploadField dr_upload = new DataRecordUploadField(60, "pics", true);
        if (Session["pic_id"] != null && Session["pic_id"].ToString() != "" && int.Parse(Session["pic_id"].ToString()) != 0)
            dr_upload.Initialize("The pic", "adm_table_header_nbg", "FormInput", "BASE_URL", false);
        else
            dr_upload.Initialize("The pic", "adm_table_header_nbg", "FormInput", "BASE_URL", true);
        PageUtils.AddCutCroptDimentions(ref dr_upload);
        theRecord.AddRecord(dr_upload);

        DataRecordShortTextField dr_pic_link = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_pic_link.Initialize("Pic link", "adm_table_header_nbg", "FormInput", "PIC_LINK", false);
        theRecord.AddRecord(dr_pic_link);

        DataRecordRadioField dr_link_target = new DataRecordRadioField("lu_link_target", "description", "id", "", null);
        dr_link_target.Initialize("Pic link target", "adm_table_header_nbg", "FormInput", "PIC_LINK_TARGET", false);
        dr_link_target.SetDefault(1);
        theRecord.AddRecord(dr_link_target);


        DataRecordShortTextField dr_credit = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_credit.Initialize("Credit", "adm_table_header_nbg", "FormInput", "CREDIT", false);
        theRecord.AddRecord(dr_credit);

        DataRecordShortTextField dr_link = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_link.Initialize("Credit link", "adm_table_header_nbg", "FormInput", "CREDIT_LINK", false);
        theRecord.AddRecord(dr_link);

        DataRecordMultiField dr_tags = new DataRecordMultiField("tags", "id", "id", "pics_tags", "PIC_ID", "TAG_ID", true, "ltr", 60, "tags");
        dr_tags.Initialize("Tags", "adm_table_header_nbg", "FormInput", "VALUE", true);
        dr_tags.SetCollectionLength(8);
        dr_tags.SetExtraWhere("TAG_TYPE_ID=0");
        //dr_tags.SetOrderCollectionBy("CLICK_CNT desc");
        theRecord.AddRecord(dr_tags);

        DataRecordLongTextField dr_remarks = new DataRecordLongTextField("ltr", true, 60, 4);
        dr_remarks.Initialize("Remarks", "adm_table_header_nbg", "FormInput", "REMARKS", false);
        theRecord.AddRecord(dr_remarks);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_pic_popup_selector.aspx?submited=1");

        return sTable;
    }

    public string GetPics(string sIds)
    {
        string sRet = "";
        string[] s = sIds.Split(';');
        for (int j = 0; j < s.Length; j++)
        {
            string sID = s[j].ToString();
            if (sID != "")
            {
                Int32 nMediaVidID = 0;
                ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
                selectQuery1 += "select mf.id from media_files mf where mf.status=1  and mf.MEDIA_TYPE_ID=1 and ";
                selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("mf.media_id", "=", int.Parse(sID));
                if (selectQuery1.Execute("query", true) != null)
                {
                    Int32 nCount1 = selectQuery1.Table("query").DefaultView.Count;
                    if (nCount1 > 0)
                    {
                        nMediaVidID = int.Parse(selectQuery1.Table("query").DefaultView[0].Row["id"].ToString());
                    }
                }
                selectQuery1.Finish();
                selectQuery1 = null;

                DataRecordMediaViewerField dr_player = new DataRecordMediaViewerField("", nMediaVidID);
                dr_player.Initialize("Video", "adm_table_header_nbg", "FormInput", "STREAMING_CODE", false);
                string sRef = dr_player.GetTNImage();
                string sName = PageUtils.GetTableSingleVal(Session["vidTable"].ToString(), "name", int.Parse(sID)).ToString();

                sRet += "<li id=\"vid_" + sID + "\">";
                sRet += "<h5 title=\"" + sName + "\">" + sName + "</h5>";
                sRet += "<img src=\"" + sRef + "\" alt=\"" + sName + "\" title=\"" + sName + "\" /><a href=\"javascript:removePic(" + sID + ");\" title=\"Remove\">Remove</a></li>";
            }
        }
        return sRet;
    }

    public string SearchPics(string sTags)
    {
        string sRet = "";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        string sTagsTable = Session["vidTable"].ToString() + "_tags";
        selectQuery += "select distinct s.ID from " + Session["vidTable"].ToString() + " s," + Session["vidTableTags"].ToString() + " st,tags t where s.id=st." + Session["vidTableTagsRef"].ToString() + " and s.status<>2 and s.is_active=1 and st.status<>2 and t.id=st.tag_id ";
        selectQuery += "and (";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("s.group_id", "=", LoginManager.GetLoginGroupID());
        string sWPGID = PageUtils.GetPermittedWatchRulesID(LoginManager.GetLoginGroupID());
        if (sWPGID != "")
        {
            selectQuery += " or s.WATCH_PERMISSION_TYPE_ID in (";
            selectQuery += sWPGID;
            selectQuery += ")";
        }
        selectQuery += ")";
        string[] sSplitArr = { ";" };
        string[] s = sTags.Split(sSplitArr, StringSplitOptions.RemoveEmptyEntries);
        if (s.Length > 0)
            selectQuery += " and (( ";
        for (int j = 0; j < s.Length; j++)
        {
            string sTagName = s[j].ToString();
            if (sTagName.Trim() != "")
            {
                if (j > 0)
                    selectQuery += " or ";
                selectQuery += "LTRIM(RTRIM(LOWER(s.name))) like (N'%" + sTagName.Trim().ToLower().Replace("'" , "''") + "%') ";
            }
        }
        
        if (s.Length > 0)
            selectQuery += ") or (";
        for (int j = 0; j < s.Length; j++)
        {
            string sTagName = s[j].ToString();
            if (sTagName.Trim() != "")
            {
                if (j > 0)
                    selectQuery += " or ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("t.value", "=", sTagName);
            }
        }
        if (s.Length > 0)
            selectQuery += " ))";

        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;

            for (int i = 0; i < nCount; i++)
            {
                Int32 nMediaVidID = 0;
                ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
                selectQuery1 += "select mf.id from media_files mf where mf.status=1 and mf.is_active=1 and ";
                selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("mf.media_id", "=", int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString()));
                selectQuery1 += " order by mf.MEDIA_TYPE_ID";
                if (selectQuery1.Execute("query", true) != null)
                {
                    Int32 nCount1 = selectQuery1.Table("query").DefaultView.Count;
                    if (nCount1 > 0)
                    {
                        nMediaVidID = int.Parse(selectQuery1.Table("query").DefaultView[0].Row["id"].ToString());
                    }
                }
                selectQuery1.Finish();
                selectQuery1 = null;

                DataRecordMediaViewerField dr_player = new DataRecordMediaViewerField("", nMediaVidID);
                dr_player.Initialize("הוידאו", "adm_table_header_nbg", "FormInput", "STREAMING_CODE", false);
                string sRef = dr_player.GetTNImage();
                string sName = PageUtils.GetTableSingleVal(Session["vidTable"].ToString(), "name", int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString())).ToString();
                sRet += "<li>";
                sRet += "<h5 title=\"" + sName + "\">" + sName + "</h5>";
                sRet += "<img src=\"" + sRef + "\" alt=\"" + sName + "\" /><a href=\"javascript:addPic(" + selectQuery.Table("query").DefaultView[i].Row["ID"].ToString() + " , '" + sRef + "','" + sName.Replace("\"", "~~qoute~~").Replace("'", "~~apos~~") + "');\" title=\"Add\">Add</a></li>";
            }
        }

        selectQuery.Finish();
        selectQuery = null;
        return sRet;
    }
}
