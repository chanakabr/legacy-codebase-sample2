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


    protected List<int> SearchMedias(string Query)
    {
      
        List<int> assetIds = new List<int>();
        UnifiedSearchResult[] assets;
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
                string sWSURL = GetWSURL("api_ws");
                sWSURL = "http://localhost/ws_api/api.asmx";
                if (string.IsNullOrEmpty(sWSURL) || string.IsNullOrEmpty(sWSUserName) || string.IsNullOrEmpty(sWSPass))
                {
                    log.DebugFormat("fail to get api WS sWSURL={0}, sWSUserName={1}, sWSPass={2}", sWSURL, sWSUserName, sWSPass);
                    return assetIds;
                }

                apiWS.API client = new apiWS.API();
                client.Url = sWSURL;

                assets = client.SearchAssets(sWSUserName, sWSPass, Query, 0, 50, false, 0, false, string.Empty, sIP, string.Empty, 0, LoginManager.GetLoginGroupID(), true);
                if (assets != null && assets.Length > 0)
                {
                    int asset = 0;
                    foreach (UnifiedSearchResult item in assets)
                    {
                        if (int.TryParse(item.AssetId, out asset))
                        {
                            assetIds.Add(asset);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            log.ErrorFormat("fail to get assets in SearchMedias Query = {0}", Query);
        }
        return assetIds;
    }
  private string GetWSURL(string key)
    {
        return TVinciShared.WS_Utils.GetTcmConfigValue(key);
    }

  public string SearchPics(string Query)
  {
      string sRet = "";
      List<int> assetIds = SearchMedias(Query);
      List<AssetDetails> assetDetailsIds = new List<AssetDetails>();
     // Dictionary<int, KeyValuePair<int, string>> defaulrPicIds = new Dictionary<int, KeyValuePair<int, string>>();// groupid <DEFAULT_PIC_ID, PICS_REMOTE_BASE_URL>
       ODBCWrapper.DataSetSelectQuery selectQuery;
      if (assetIds != null && assetIds.Count > 0)
      {
          selectQuery = new ODBCWrapper.DataSetSelectQuery();
          selectQuery += "select m.id, m.group_id, m.name, m.MEDIA_PIC_ID  from media m where m.status=1 and m.is_active=1 and ";
          selectQuery += " m.id in (" + string.Join(",", assetIds) + ") ";         
          if (selectQuery.Execute("query", true) != null)
          {
              DataTable dt = selectQuery.Table("query");
              foreach (DataRow dr in dt.Rows)
              {
                  assetDetailsIds.Add(new AssetDetails(
                      ODBCWrapper.Utils.GetIntSafeVal(dr, "id"), ODBCWrapper.Utils.GetIntSafeVal(dr, "id"),
                      ODBCWrapper.Utils.GetIntSafeVal(dr, "group_id"),
                      ODBCWrapper.Utils.GetSafeStr(dr, "name"),
                      ODBCWrapper.Utils.GetIntSafeVal(dr, "MEDIA_PIC_ID")
                  ));
              }
          }

          selectQuery.Finish();
          selectQuery = null;
      }

      //List<int> GroupIDs = DAL.UtilsDal.GetAllRelatedGroups(LoginManager.GetLoginGroupID());
      
      //selectQuery = new ODBCWrapper.DataSetSelectQuery();
      //selectQuery += "select DEFAULT_PIC_ID, PICS_REMOTE_BASE_URL,  id  from groups WITH(NOLOCK) where id in ( " + string.Join(",", GroupIDs) + " )";
      //if (selectQuery.Execute("query", true) != null)
      //{
      //    DataTable dt = selectQuery.Table("query");
      //    foreach (DataRow dr in dt.Rows)
      //    {
      //        int group = ODBCWrapper.Utils.GetIntSafeVal(dr, "id");
      //        string basePicsURL = ODBCWrapper.Utils.GetSafeStr(dr, "PICS_REMOTE_BASE_URL");
      //        if (string.IsNullOrEmpty(basePicsURL))
      //        {
      //            basePicsURL = "pics";
      //        }
      //        else if (basePicsURL.ToLower().Trim().StartsWith("http://") == false && basePicsURL.ToLower().Trim().StartsWith("https://") == false)
      //        {
      //            basePicsURL = "http://" + basePicsURL;
      //        }

      //        defaulrPicIds.Add( group,
      //            new KeyValuePair<int, string>(
      //            ODBCWrapper.Utils.GetIntSafeVal(dr, "DEFAULT_PIC_ID"),
      //            basePicsURL
      //            ));
      //    }
      //}
      //selectQuery.Finish();
      //selectQuery = null;
      
      foreach (AssetDetails asset in assetDetailsIds)
      {
          DataRecordMediaViewerField dr_player = new DataRecordMediaViewerField("", asset.assetId);
          dr_player.Initialize("", "adm_table_header_nbg", "FormInput", "", false);
          //int picId = asset.mediaPicId != 0 ? asset.mediaPicId : defaulrPicIds[asset.groupId].Key;
          string sRef = dr_player.GetImapgeSrc(asset.mediaPicId, asset.groupId);
                   
          sRet += "<li>";
          sRet += "<h5 title=\"" + asset.assetName + "\">" + asset.assetName + "</h5>";
          sRet += "<img src=\"" + sRef + "\" alt=\"" + asset.assetName + "\" /><a href=\"javascript:addPic(" + asset.assetId + " , '" + sRef + "','" + asset.assetName.Replace("\"", "~~qoute~~").Replace("'", "~~apos~~") + "');\" title=\"Add\">Add</a></li>";

      }

      return sRet;
  }

  public class AssetDetails
  {
      public int assetFileId = 0;
      public int assetId = 0;
      public int groupId = 0;
      public string assetName = string.Empty;
      public int mediaPicId = 0;

      public AssetDetails(int assetFileId, int assetId, int groupId, string assetName, int mediaPicId)
      {
          this.assetFileId = assetFileId;
          this.assetId = assetId;
          this.groupId = groupId;
          this.assetName = assetName;
          this.mediaPicId = mediaPicId;
      }

  }
}
