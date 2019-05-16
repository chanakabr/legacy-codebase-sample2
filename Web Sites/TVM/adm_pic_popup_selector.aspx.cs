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

public partial class adm_pic_popup_selector : System.Web.UI.Page
{
    static protected string m_sIDs = "";
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_pics.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_pics.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.aspx");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString().Trim() == "1")
            {
                string sIDs = Request.Form["ids_place"].ToString();
                //Request.Form["id"] = sIDs;
                Int32 nID = DBManipulator.DoTheWork();
                Session["new_id"] = nID.ToString();
                return;
            }
            else if (Request.QueryString["pics_ids"] != null)
            {
                Session["new_id"] = null;
                m_sIDs = Request.QueryString["pics_ids"].ToString();
                string[] s = m_sIDs.Split(';');
                Int32 nC = s.Length;
                string sNewIDs = "";
                for (int i = 0; i < nC; i++)
                {
                    if (s[i].ToString() == "")
                        continue;
                    object o = PageUtils.GetTableSingleVal("pics", "id", int.Parse(s[i].ToString()));
                    if (o != null)
                    {
                        if (sNewIDs != "")
                            sNewIDs += ";";
                        sNewIDs += s[i].ToString();
                    }
                }
                m_sIDs = sNewIDs;
            }
            else if (Session["m_sIDs"] != null)
            {
                Session["new_id"] = null;
                m_sIDs = Session["m_sIDs"].ToString();
                Session["m_sIDs"] = null;
            }
            else
            {
                Session["new_id"] = null;
                m_sIDs = "";
            }

            if (Request.QueryString["theID"] != null && Request.QueryString["theID"] != "")
                Session["theID"] = Request.QueryString["theID"].ToString();

            if (Request.QueryString["maxPics"] != null && Request.QueryString["maxPics"] != "")
                Session["maxPics"] = Request.QueryString["maxPics"].ToString();

            if (Request.QueryString["lastPage"] != null && Request.QueryString["lastPage"] != "")
            {
                Session["lastPage"] = Request.QueryString["lastPage"].ToString();
            }
            else
            {
                Session["lastPage"] = null;
               // Session.Remove("lastPage");
            }
        }
    }

    public void GetNewIDFunc()
    {
        if (Session["new_id"] != null)
        {
            Session["second_new_id"] = Session["new_id"];
            Session["new_id"] = null;
        }
        else if (Session["second_new_id"] != null && Session["second_new_id"].ToString() != "0")
        {
            Int32 nGroupID = LoginManager.GetLoginGroupID();
            object oBasePicsURL = PageUtils.GetTableSingleVal("groups", "PICS_REMOTE_BASE_URL", nGroupID);
            string sBasePicsURL = "";
            if (oBasePicsURL != DBNull.Value && oBasePicsURL != null)
                sBasePicsURL = oBasePicsURL.ToString();
            if (sBasePicsURL == "")
                sBasePicsURL = "pics";
            else if (sBasePicsURL.ToLower().Trim().StartsWith("http://") == false &&
                sBasePicsURL.ToLower().Trim().StartsWith("https://") == false)
                sBasePicsURL = "http://" + sBasePicsURL;
            string sPic = PageUtils.GetTableSingleVal("pics", "base_url", int.Parse(Session["second_new_id"].ToString())).ToString();
            string sRef = sBasePicsURL;
            if (sBasePicsURL.EndsWith("=") == false)
                sRef += "/";
            sRef += ImageUtils.GetTNName(sPic, "tn");
            if (sBasePicsURL.EndsWith("=") == true)
            {
                string sTmp1 = "";
                string[] s = sRef.Split('.');
                for (int j = 0; j < s.Length - 1; j++)
                {
                    if (j > 0)
                        sTmp1 += ".";
                    sTmp1 += s[j];
                }
                sRef = sTmp1;
            }
            string sName = PageUtils.GetTableSingleVal("pics", "name", int.Parse(Session["second_new_id"].ToString())).ToString();
            Response.Write("addPic(" + Session["second_new_id"].ToString() + " , '" + sRef + "','" + sName.Replace("\"", "~~qoute~~").Replace("'", "~~apos~~") + "');");
            Session["second_new_id"] = null;
        }
    }

    public void GetSendID()
    {
        if (Session["theID"] != null)
            Response.Write(Session["theID"].ToString());
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

    private string getPicID()
    {
        string retVal = string.Empty;
        if (Session["media_id"] != null && !string.IsNullOrEmpty(Session["media_id"].ToString()) && Session["lastPage"] != null)
        {
            string mediaID = Session["media_id"].ToString();
            if (mediaID == "0")
            {
                retVal = null;
            }
            else
            {
                int mediaId = int.Parse(Session["media_id"].ToString());
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select media_pic_id from media where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", mediaId);
                if (selectQuery.Execute("query", true) != null)
                {
                    int count = selectQuery.Table("query").DefaultView.Count;
                    if (count > 0)
                    {
                        retVal = selectQuery.Table("query").DefaultView[0].Row["media_pic_id"].ToString();
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
                if (string.IsNullOrEmpty(retVal) || retVal == "0")
                {
                    retVal = null;
                }
            }
        }
        if ((Session["lastPage"] != null && Session["lastPage"].ToString() != "media") || Session["lastPage"] == null || string.IsNullOrEmpty(Session["lastPage"].ToString()))
        {
            retVal = null;
        }
        
        return retVal;
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object t = null; ;
        if (Session["media_id"] != null)
            t = getPicID();
        DBRecordWebEditor theRecord = new DBRecordWebEditor("pics", "adm_table_pager", "adm_pic_popup_selector.aspx", "", "ID", t, "javascript:window.close();", "pic_id");

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_name);

        DataRecordLongTextField dr_description = new DataRecordLongTextField("ltr", true, 60, 4);
        dr_description.Initialize("Description", "adm_table_header_nbg", "FormInput", "DESCRIPTION", false);
        theRecord.AddRecord(dr_description);

        DataRecordUploadField dr_upload = new DataRecordUploadField(60, "pics", true, "5");
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

        DataRecordRadioField dr_pic_ratio = new DataRecordRadioField("groups_ratios", "ratio", "id", "", null);
        string picQuery = "select lur.ratio as 'txt', g.ratio_id as 'id' from groups g, lu_pics_ratios lur where g.ratio_id = lur.id and g.id = " + LoginManager.GetLoginGroupID().ToString() + " UNION " + "select lur.ratio as 'txt', gr.ratio_id as 'id' from group_ratios gr, lu_pics_ratios lur where gr.ratio_id = lur.id and gr.group_id = " + LoginManager.GetLoginGroupID().ToString();
        dr_pic_ratio.SetSelectsQuery(picQuery);
        dr_pic_ratio.Initialize("Pic Ratio", "adm_table_header_nbg", "FormInput", "", false);
        dr_pic_ratio.SetDefault(0);
        theRecord.AddRecord(dr_pic_ratio);

        // checkBox for thumbnail pic size 
        DataRecordCheckBoxField dr_thumbnail = new DataRecordCheckBoxField(true);
        dr_thumbnail.Initialize("Thumbnail ","adm_table_header_nbg", "FormInput", "", false);
        theRecord.AddRecord(dr_thumbnail);


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
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        object oBasePicsURL = PageUtils.GetTableSingleVal("groups", "PICS_REMOTE_BASE_URL", nGroupID);
        string sBasePicsURL = "";
        if (oBasePicsURL != DBNull.Value && oBasePicsURL != null)
            sBasePicsURL = oBasePicsURL.ToString();
        if (sBasePicsURL == "")
            sBasePicsURL = "pics";
        else if (sBasePicsURL.ToLower().Trim().StartsWith("http://") == false &&
            sBasePicsURL.ToLower().Trim().StartsWith("https://") == false)
            sBasePicsURL = "http://" + sBasePicsURL;

        string sRet = "";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select distinct p.BASE_URL ,p.NAME, p.ID from pics p where status=1 ";
        //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("p.group_id", "=", LoginManager.GetLoginGroupID());
        //selectQuery += "and";
        string[] s = sIds.Split(';');
        if (s.Length > 0)
            selectQuery += " and p.id in (";
        for (int j = 0; j < s.Length; j++)
        {
            string sID = s[j].ToString();
            if (sID != "")
            {
                if (j > 0)
                    selectQuery += " , ";
                selectQuery += sID;
            }
        }
        if (s.Length > 0)
            selectQuery += " )";

        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount == 0)
                m_sIDs = "";
            for (int i = 0; i < nCount; i++)
            {
                string sRef = sBasePicsURL;
                if (sBasePicsURL.EndsWith("=") == false)
                    sRef += "/";
                sRef += ImageUtils.GetTNName(selectQuery.Table("query").DefaultView[i].Row["BASE_URL"].ToString(), "tn");
                if (sBasePicsURL.EndsWith("=") == true)
                {
                    string sTmp1 = "";
                    string[] s1 = sRef.Split('.');
                    for (int j = 0; j < s1.Length - 1; j++)
                    {
                        if (j > 0)
                            sTmp1 += ".";
                        sTmp1 += s1[j];
                    }
                    sRef = sTmp1;
                }
                string sName = selectQuery.Table("query").DefaultView[i].Row["NAME"].ToString();

                sRet += "<div id=\"out_img_" + selectQuery.Table("query").DefaultView[i].Row["ID"].ToString() + "\"><li id=\"img_" + selectQuery.Table("query").DefaultView[i].Row["ID"].ToString() + "\">";
                sRet += "<h5 title=\"" + sName + "\">" + sName + "</h5>";
                sRet += "<img src=\"" + sRef + "\" alt=\"" + sName + "\" title=\"" + sName + "\" /><a href=\"javascript:removePic(" + selectQuery.Table("query").DefaultView[i].Row["ID"].ToString() + ");\" title=\"Remove\">Remove</a></li></div>";
            }
        }

        selectQuery.Finish();
        selectQuery = null;
        return sRet;
    }

    public void GetGroups()
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        string sGroups = PageUtils.GetFullGroupsStr(nGroupID, "");
        Response.Write(sGroups);
    }

    public string SearchPics(string sTags)
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        object oBasePicsURL = PageUtils.GetTableSingleVal("groups", "PICS_REMOTE_BASE_URL", nGroupID);
        string sBasePicsURL = "";
        if (oBasePicsURL != DBNull.Value && oBasePicsURL != null)
            sBasePicsURL = oBasePicsURL.ToString();
        string sBaseForQuery = sBasePicsURL;
        if (sBasePicsURL == "")
            sBasePicsURL = "pics";
        else if (sBasePicsURL.ToLower().Trim().StartsWith("http://") == false &&
            sBasePicsURL.ToLower().Trim().StartsWith("https://") == false)
            sBasePicsURL = "http://" + sBasePicsURL;

        string sRet = "";
        string sGroups = PageUtils.GetFullGroupsStr(nGroupID, "");
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select distinct p.BASE_URL ,p.NAME, p.ID from pics p,pics_tags pt,tags t where ";
        //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.PICS_REMOTE_BASE_URL", "=", sBaseForQuery);
        selectQuery += " t.status=1 and p.id=pt.pic_id and p.status=1 and pt.status<>2 and t.id=pt.tag_id ";
        selectQuery += "and p.group_id " + sGroups;
        
        //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("p.group_id", "=", LoginManager.GetLoginGroupID());
        string[] sSplitArr = { ";" };
        string[] s = sTags.Split(sSplitArr , StringSplitOptions.RemoveEmptyEntries);
        if (s.Length > 0)
            selectQuery += " and (";
        for (int j = 0; j < s.Length; j++)
        {
            string sTagName = s[j].ToString();
            if (j > 0)
                selectQuery += " or ";
            string sLike = "like(N'%" + sTagName.Replace("'", "''") + "%')";
            selectQuery += "(p.name " + sLike;
            selectQuery += " or p.description " + sLike;
            selectQuery += " or ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("t.value", "=", sTagName);
            selectQuery += ")";
        }
        if (s.Length > 0)
            selectQuery += " )";

        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            
            for (int i = 0; i < nCount; i++)
            {
                string sRef = sBasePicsURL;
                if (sBasePicsURL.EndsWith("=") == false)
                    sRef += "/";
                sRef += ImageUtils.GetTNName(selectQuery.Table("query").DefaultView[i].Row["BASE_URL"].ToString(), "tn");
                if (sBasePicsURL.EndsWith("=") == true)
                {
                    string sTmp1 = "";
                    string[] s1 = sRef.Split('.');
                    for (int j = 0; j < s1.Length - 1; j++)
                    {
                        if (j > 0)
                            sTmp1 += ".";
                        sTmp1 += s1[j];
                    }
                    sRef = sTmp1;
                }
                string sName = selectQuery.Table("query").DefaultView[i].Row["NAME"].ToString();
                sRet += "<li>";
                sRet += "<h5 title=\"" + sName + "\">" + sName + "</h5>";
                sRet += "<img src=\"" + sRef + "\" alt=\"" + sName + "\" title=\"" + sName + "\" /><a href=\"javascript:addPic(" + selectQuery.Table("query").DefaultView[i].Row["ID"].ToString() + " , '" + sRef + "','" + sName.Replace("\"", "~~qoute~~").Replace("'", "~~apos~~") + "');\" title=\"Add\">Add</a></li>";
            }
        }

        selectQuery.Finish();
        selectQuery = null;
        return sRet;
    }
}
