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

public partial class adm_pic_popup_uploader : System.Web.UI.Page
{
    static protected string m_sIDs = "";

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

        Initialize();               
    }

    private void Initialize()
    {
        // Set ratios
        PopulateRatioList();
    }

    private void PopulateRatioList()
    {
        rdbRatio.DataValueField = "id";
        rdbRatio.DataTextField = "txt";
        rdbRatio.DataSource = GetRatioData();
        rdbRatio.DataBind();
        // This will select the radio with value 1
        rdbRatio.SelectedIndex = 0;
    }

    private DataView GetRatioData()
    {
        DataView data = new DataView();
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select lur.ratio as 'txt', g.ratio_id as 'id' from groups g, lu_pics_ratios lur where g.ratio_id = lur.id and g.id = " + LoginManager.GetLoginGroupID().ToString() + " UNION " + "select lur.ratio as 'txt', gr.ratio_id as 'id' from group_ratios gr, lu_pics_ratios lur where gr.ratio_id = lur.id and gr.status = 1 and gr.group_id = " + LoginManager.GetLoginGroupID().ToString();

        if (selectQuery.Execute("query", true) != null && selectQuery.Table("query").DefaultView != null)
        {
            return selectQuery.Table("query").DefaultView;
        }

        return data;
    }


    //public void GetNewIDFunc()
    //{
    //    if (Session["new_id"] != null)
    //    {
    //        Session["second_new_id"] = Session["new_id"];
    //        Session["new_id"] = null;
    //    }
    //    else if (Session["second_new_id"] != null && Session["second_new_id"].ToString() != "0")
    //    {
    //        Int32 nGroupID = LoginManager.GetLoginGroupID();
    //        object oBasePicsURL = PageUtils.GetTableSingleVal("groups", "PICS_REMOTE_BASE_URL", nGroupID);
    //        string sBasePicsURL = "";
    //        if (oBasePicsURL != DBNull.Value && oBasePicsURL != null)
    //            sBasePicsURL = oBasePicsURL.ToString();
    //        if (sBasePicsURL == "")
    //            sBasePicsURL = "pics";
    //        else if (sBasePicsURL.ToLower().Trim().StartsWith("http://") == false &&
    //            sBasePicsURL.ToLower().Trim().StartsWith("https://") == false)
    //            sBasePicsURL = "http://" + sBasePicsURL;
    //        string sPic = PageUtils.GetTableSingleVal("pics", "base_url", int.Parse(Session["second_new_id"].ToString())).ToString();
    //        string sRef = sBasePicsURL;
    //        if (sBasePicsURL.EndsWith("=") == false)
    //            sRef += "/";
    //        sRef += ImageUtils.GetTNName(sPic, "tn");
    //        if (sBasePicsURL.EndsWith("=") == true)
    //        {
    //            string sTmp1 = "";
    //            string[] s = sRef.Split('.');
    //            for (int j = 0; j < s.Length - 1; j++)
    //            {
    //                if (j > 0)
    //                    sTmp1 += ".";
    //                sTmp1 += s[j];
    //            }
    //            sRef = sTmp1;
    //        }
    //        string sName = PageUtils.GetTableSingleVal("pics", "name", int.Parse(Session["second_new_id"].ToString())).ToString();
    //        Response.Write("addPic(" + Session["second_new_id"].ToString() + " , '" + sRef + "','" + sName.Replace("\"", "~~qoute~~").Replace("'", "~~apos~~") + "');");
    //        Session["second_new_id"] = null;
    //    }
    //}

    //public void GetSendID()
    //{
    //    if (Session["theID"] != null)
    //        Response.Write(Session["theID"].ToString());
    //}

    //public void GetMaxPics()
    //{
    //    if (Session["maxPics"] != null)
    //        Response.Write(Session["maxPics"].ToString());
    //}

    //public void GetIDs()
    //{
    //    if (Session["m_sIDs"] != null)
    //    {
    //        m_sIDs = Session["m_sIDs"].ToString();
    //    }

    //    Response.Write(m_sIDs);
    //}

    //private string getPicID()
    //{
    //    string retVal = string.Empty;
    //    if (Session["media_id"] != null && !string.IsNullOrEmpty(Session["media_id"].ToString()) && Session["lastPage"] != null)
    //    {
    //        string mediaID = Session["media_id"].ToString();
    //        if (mediaID == "0")
    //        {
    //            retVal = null;
    //        }
    //        else
    //        {
    //            int mediaId = int.Parse(Session["media_id"].ToString());
    //            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
    //            selectQuery += "select media_pic_id from media where ";
    //            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", mediaId);
    //            if (selectQuery.Execute("query", true) != null)
    //            {
    //                int count = selectQuery.Table("query").DefaultView.Count;
    //                if (count > 0)
    //                {
    //                    retVal = selectQuery.Table("query").DefaultView[0].Row["media_pic_id"].ToString();
    //                }
    //            }
    //            selectQuery.Finish();
    //            selectQuery = null;
    //            if (string.IsNullOrEmpty(retVal) || retVal == "0")
    //            {
    //                retVal = null;
    //            }
    //        }
    //    }
    //    if ((Session["lastPage"] != null && Session["lastPage"].ToString() != "media") || Session["lastPage"] == null || string.IsNullOrEmpty(Session["lastPage"].ToString()))
    //    {
    //        retVal = null;
    //    }

    //    return retVal;
    //}

    //public string GetPageContent(string sOrderBy, string sPageNum)
    //{
    //    if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
    //    {
    //        Session["error_msg"] = "";
    //        return Session["last_page_html"].ToString();
    //    }
    //    object t = null; ;
    //    if (Session["media_id"] != null)
    //        t = getPicID();
    //    DBRecordWebEditor theRecord = new DBRecordWebEditor("pics", "adm_table_pager", "adm_pic_popup_uploader.aspx", "", "ID", t, "javascript:window.close();", "pic_id");

    //    DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
    //    dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "NAME", true);
    //    theRecord.AddRecord(dr_name);

    //    DataRecordShortTextField dr_pic_link = new DataRecordShortTextField("ltr", true, 60, 128);
    //    dr_pic_link.Initialize("Pic link", "adm_table_header_nbg", "FormInput", "PIC_LINK", true);
    //    theRecord.AddRecord(dr_pic_link);

    //    //DataRecordRadioField dr_pic_ratio = new DataRecordRadioField("groups_ratios", "ratio", "id", "", null);
    //    //string picQuery = "select lur.ratio as 'txt', g.ratio_id as 'id' from groups g, lu_pics_ratios lur where g.ratio_id = lur.id and g.id = " + LoginManager.GetLoginGroupID().ToString() + " UNION " + "select lur.ratio as 'txt', gr.ratio_id as 'id' from group_ratios gr, lu_pics_ratios lur where gr.ratio_id = lur.id and gr.status = 1 and gr.group_id = " + LoginManager.GetLoginGroupID().ToString();
    //    //dr_pic_ratio.SetSelectsQuery(picQuery);
    //    //dr_pic_ratio.Initialize("Pic Ratio", "adm_table_header_nbg", "FormInput", "", false);
    //    //dr_pic_ratio.SetDefault(0);
    //    //theRecord.AddRecord(dr_pic_ratio);

    //    DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
    //    dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
    //    dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
    //    theRecord.AddRecord(dr_groups);

    //    string sTable = theRecord.GetTableHTML("adm_pic_popup_uploader.aspx?submited=1");

    //    return sTable;
    //}

    //public string GetPics(string sIds)
    //{
    //    Int32 nGroupID = LoginManager.GetLoginGroupID();
    //    object oBasePicsURL = PageUtils.GetTableSingleVal("groups", "PICS_REMOTE_BASE_URL", nGroupID);
    //    string sBasePicsURL = "";
    //    if (oBasePicsURL != DBNull.Value && oBasePicsURL != null)
    //        sBasePicsURL = oBasePicsURL.ToString();
    //    if (sBasePicsURL == "")
    //        sBasePicsURL = "pics";
    //    else if (sBasePicsURL.ToLower().Trim().StartsWith("http://") == false &&
    //        sBasePicsURL.ToLower().Trim().StartsWith("https://") == false)
    //        sBasePicsURL = "http://" + sBasePicsURL;

    //    string sRet = "";
    //    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
    //    selectQuery += "select distinct p.BASE_URL ,p.NAME, p.ID from pics p where status=1 ";
    //    //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("p.group_id", "=", LoginManager.GetLoginGroupID());
    //    //selectQuery += "and";
    //    string[] s = sIds.Split(';');
    //    if (s.Length > 0)
    //        selectQuery += " and p.id in (";
    //    for (int j = 0; j < s.Length; j++)
    //    {
    //        string sID = s[j].ToString();
    //        if (sID != "")
    //        {
    //            if (j > 0)
    //                selectQuery += " , ";
    //            selectQuery += sID;
    //        }
    //    }
    //    if (s.Length > 0)
    //        selectQuery += " )";

    //    if (selectQuery.Execute("query", true) != null)
    //    {
    //        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
    //        if (nCount == 0)
    //            m_sIDs = "";
    //        for (int i = 0; i < nCount; i++)
    //        {
    //            string sRef = sBasePicsURL;
    //            if (sBasePicsURL.EndsWith("=") == false)
    //                sRef += "/";
    //            sRef += ImageUtils.GetTNName(selectQuery.Table("query").DefaultView[i].Row["BASE_URL"].ToString(), "tn");
    //            if (sBasePicsURL.EndsWith("=") == true)
    //            {
    //                string sTmp1 = "";
    //                string[] s1 = sRef.Split('.');
    //                for (int j = 0; j < s1.Length - 1; j++)
    //                {
    //                    if (j > 0)
    //                        sTmp1 += ".";
    //                    sTmp1 += s1[j];
    //                }
    //                sRef = sTmp1;
    //            }
    //            string sName = selectQuery.Table("query").DefaultView[i].Row["NAME"].ToString();

    //            sRet += "<div id=\"out_img_" + selectQuery.Table("query").DefaultView[i].Row["ID"].ToString() + "\"><li id=\"img_" + selectQuery.Table("query").DefaultView[i].Row["ID"].ToString() + "\">";
    //            sRet += "<h5 title=\"" + sName + "\">" + sName + "</h5>";
    //            sRet += "<img src=\"" + sRef + "\" alt=\"" + sName + "\" title=\"" + sName + "\" /><a href=\"javascript:removePic(" + selectQuery.Table("query").DefaultView[i].Row["ID"].ToString() + ");\" title=\"Remove\">Remove</a></li></div>";
    //        }
    //    }

    //    selectQuery.Finish();
    //    selectQuery = null;
    //    return sRet;
    //}

    protected void btnConfirm_Click(object sender, EventArgs e)
    {
        //lblStatus
        lblStatus.Text = "";
        lblStatus.Visible = false;

        int groupID = LoginManager.GetLoginGroupID();

        string name = txtName.Text.Trim();
        string picLink = txtPicLink.Text.Trim();
        string ratio = rdbRatio.SelectedValue;
        int ratioId = 0;

        int.TryParse(ratio, out ratioId);

        // name and picLink --> mandatory fields
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(picLink))
        {
            lblStatus.ForeColor = System.Drawing.Color.Red;
            lblStatus.Text = "Please fill required fields";
            lblStatus.Visible = true;
            return;
        }

        Uri uriResult;
        // check for valid Url
        bool result = Uri.TryCreate(picLink, UriKind.Absolute, out uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

        if (!result)
        {
            lblStatus.ForeColor = System.Drawing.Color.Red;
            lblStatus.Text = "Please fill valid pic link";
            lblStatus.Visible = true;
            return;
        }

        //Get MediaId
        string media = string.Empty;
        if (Session["media_id"] != null && !string.IsNullOrEmpty(Session["media_id"].ToString()))
        {
            media = Session["media_id"].ToString();
            if (media == "0")
            {
                return;
            }
        }

        int mediaId = 0;
        int.TryParse(media, out mediaId);

        // setMediaThumb only if the ratio is the group default ratio
        bool setMediaThumb = ratioId == TvinciImporter.ImporterImpl.GetGroupDefaultRatio(groupID);

        int picId = TvinciImporter.ImporterImpl.DownloadPicToImageServer(picLink, name, groupID, mediaId, "eng", "THUMBNAIL", setMediaThumb, ratioId);

        if (setMediaThumb)
        {
            //update media with new Pic
            Session["Pic_Image_Url"] = PageUtils.GetPicImageUrlByRatio(picId, 90, 65);

            Page.ClientScript.RegisterStartupScript(this.GetType(), "close", "<script language=javascript>window.opener.ChangePic('9_val'," + picId + ");self.close();</script>");

        }

    }

    protected void btnCancel_Click(object sender, EventArgs e)
    {
        ClientScript.RegisterStartupScript(typeof(Page), "closePage", "window.close();", true);
    }



}
