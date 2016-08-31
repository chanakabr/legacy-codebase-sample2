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
using apiWS;
using System.Collections.Generic;
using KLogMonitor;
using System.Reflection;
using System.Text;

public partial class adm_video_popup_selector : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

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
        StringBuilder sRet = new StringBuilder();

        if (string.IsNullOrEmpty(sIds))
            return string.Empty;

        sIds = sIds.Trim().Trim(';').Replace(";", ",");

        DataTable dt = null;
        ODBCWrapper.DataSetSelectQuery selectQuery;
        selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select m.id, m.group_id, m.name, m.MEDIA_PIC_ID  from media m where m.status=1 and ";
        selectQuery += " m.id in (" + sIds + ") ";
        if (selectQuery.Execute("query", true) != null)
        {
            dt = selectQuery.Table("query");
        }
        selectQuery.Finish();
        selectQuery = null;

        if (dt != null && dt.Rows != null)
        {
            foreach (DataRow dr in dt.Rows)
            {
                int id = ODBCWrapper.Utils.GetIntSafeVal(dr, "id");
                string name = ODBCWrapper.Utils.GetSafeStr(dr, "name");
                string picUrl = ImageUtils.GetImageUrl(ODBCWrapper.Utils.GetIntSafeVal(dr, "MEDIA_PIC_ID"), ODBCWrapper.Utils.GetIntSafeVal(dr, "group_id"));

                sRet.AppendFormat("<li id=\"vid_{0}\">", +id);
                sRet.AppendFormat("<h5 title=\"{0)\">{0}</h5>", name);
                sRet.AppendFormat("<img src=\"{0}\" alt=\"{1}\" title=\"{1}\" height=\"65\" width=\"90\"><a href=\"javascript:removePic({2});\" title=\"Remove\">Remove</a></li>",
                    picUrl, name, id);
            }
        }
        
        return sRet.ToString();
    }

    protected List<string> SearchMedias(string Query)
    {
        List<string> assetIds = new List<string>();
        try
        {
            if (!string.IsNullOrEmpty(Query))
            {
                //call api to get assets 

                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";

                int nParentGroupID = DAL.UtilsDal.GetParentGroupID(LoginManager.GetLoginGroupID());
                TVinciShared.WS_Utils.GetWSUNPass(nParentGroupID, "SearchAssets", "api", sIP, ref sWSUserName, ref sWSPass);
                string sWSURL = TVinciShared.WS_Utils.GetTcmConfigValue("api_ws");
                if (string.IsNullOrEmpty(sWSURL) || string.IsNullOrEmpty(sWSUserName) || string.IsNullOrEmpty(sWSPass))
                {
                    log.ErrorFormat("fail to get api WS Url={0}, UserName={1}, Password={2}", sWSURL, sWSUserName, sWSPass);
                    return assetIds;
                }

                apiWS.API client = new apiWS.API();
                client.Url = sWSURL;

                UnifiedSearchResult[] assets = client.SearchAssets(sWSUserName, sWSPass, Query, 0, 50, false, 0, false, string.Empty, sIP, string.Empty, 0, LoginManager.GetLoginGroupID(), true);
                if (assets != null && assets.Length > 0)
                {
                    foreach (UnifiedSearchResult item in assets)
                    {
                        assetIds.Add(item.AssetId);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            log.Error(string.Format("fail to get assets in SearchMedias Query = {0}", Query), ex);
        }
        return assetIds;
    }

    public string SearchPics(string Query)
    {
        StringBuilder sRet = new StringBuilder();
        List<string> assetIds = SearchMedias(Query);
        if (assetIds != null && assetIds.Count > 0)
        {
            DataTable dt = null;
            ODBCWrapper.DataSetSelectQuery selectQuery;
            selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select m.id, m.group_id, m.name, m.MEDIA_PIC_ID  from media m where m.status=1 and ";
            selectQuery += " m.id in (" + string.Join(",", assetIds) + ") ";
            if (selectQuery.Execute("query", true) != null)
            {
                dt = selectQuery.Table("query");
            }
            selectQuery.Finish();
            selectQuery = null;

            if (dt != null && dt.Rows != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string name = ODBCWrapper.Utils.GetSafeStr(dr, "name");
                    string picUrl = ImageUtils.GetImageUrl(ODBCWrapper.Utils.GetIntSafeVal(dr, "MEDIA_PIC_ID"), ODBCWrapper.Utils.GetIntSafeVal(dr, "group_id"));
                    sRet.AppendFormat("<li><h5 title=\"{0}\">{1}</h5>", name, name);
                    sRet.AppendFormat("<img src=\"{0}\" alt=\"{1}\" height=\"65\" width=\"90\"><a href=\"javascript:addPic({2},'{3}','{4}');\" title=\"Add\">Add</a></li>",
                        picUrl, name, ODBCWrapper.Utils.GetIntSafeVal(dr, "id"), picUrl, name.Replace("\"", "~~qoute~~").Replace("'", "~~apos~~"));
                }
            }
        }        
        return sRet.ToString();
    }
}
