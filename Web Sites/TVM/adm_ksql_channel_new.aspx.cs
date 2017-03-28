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
using TvinciImporter;
using System.Collections.Generic;
using DAL;

public partial class adm_ksql_channel_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_channels.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_channels.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
        {
            Response.Expires = -1;
            return;
        }
        if (!IsPostBack)
        {
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                var form = HttpContext.Current.Request.Form;

                // the filter expression is the 9th input field
                string filterExpression = form["8_val"];
                string fieldName = "KSQL_FILTER";

                bool finish = false;
                int counter = 0;

                // Find the field that is KSQL filter. This is because sometimes the number changes and I don't know which field it is exactly
                while (!finish)
                {
                    string currentField = form[counter + "_field"];
                    string currentFieldType = form[counter + "_type"];

                    if (string.IsNullOrEmpty(currentFieldType))
                    {
                        finish = true;
                    }
                    else if (currentField == fieldName)
                    {
                        finish = true;
                        filterExpression = form[counter + "_val"];
                    }
                    else
                    {
                        counter++;
                    }
                }

                bool validExpression = true;

                if (!string.IsNullOrEmpty(filterExpression))
                {
                    ApiObjects.SearchObjects.BooleanPhraseNode filterTree = null;
                    var status = ApiObjects.SearchObjects.BooleanPhraseNode.ParseSearchExpression(filterExpression, ref filterTree);

                    if (status == null || status.Code != 0)
                    {
                        Session["error_msg"] = string.Format("Invalid KSQL filter: {0}", status.Message);
                        validExpression = false;
                    }
                }
                
                // Save only if expression is valid (or empty)
                if (validExpression)
                {
                    bool result;
                    int channelId = DBManipulator.DoTheWork();

                    if (channelId != 0)
                    {
                        int loginGroupID = LoginManager.GetLoginGroupID();

                        // Update asset types if it is a new channel
                        if (Session["asset_type_ids"] != null && Session["asset_type_ids"] is List<int>)
                        {
                            List<int> updatedAssetTypes = Session["asset_type_ids"] as List<int>;
                            InsertChannelAssetType(updatedAssetTypes, channelId, loginGroupID);
                            Session["asset_type_ids"] = null;
                        }

                        //Update channel at Lucene/ ES

                        result = ImporterImpl.UpdateChannelIndex(loginGroupID, new List<int>() { channelId }, ApiObjects.eAction.Update);
                    }
                    return;
                }
            }

            // if this is a KSQL channel
            if (Request.QueryString["type_id"] != null)
            {
                string typeId = Request.QueryString["type_id"];

                // if this is a KSQL channel
                if (typeId == 4.ToString())
                {
                    Response.Redirect(string.Format("adm_ksql_channel_new.aspx?channel_id={0}", Request.QueryString["channel_id"]));
                }
            }

            Int32 nMenuID = 0;
            m_sMenu = TVinciShared.Menu.GetMainMenu(6, true, ref nMenuID);

            if (Request.QueryString["channel_id"] != null &&
                Request.QueryString["channel_id"].ToString() != "")
            {
                Session["channel_id"] = int.Parse(Request.QueryString["channel_id"].ToString());
            }
            else
            {
                Session["channel_id"] = 0;
            }

            Session["channel_type"] = (int)GroupsCacheManager.ChannelType.KSQL;

            Int32 nOwnerGroupID = LoginManager.GetLoginGroupID();
            m_sLangMenu = GetLangMenu(nOwnerGroupID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 2, false);
        }
    }

    protected void GetLangMenu()
    {
        Response.Write(m_sLangMenu);
    }

    //protected void StartChannelType()
    //{
    //    Session["channel_type"] = int.Parse(PageUtils.GetTableSingleVal("channels", "CHANNEL_TYPE", int.Parse(Session["channel_id"].ToString())).ToString());
    //}

    protected string GetLangMenu(Int32 nGroupID)
    {
        if (Session["channel_id"] != null &&
            Session["channel_id"].ToString() == "0")
            return "";
        try
        {
            string sTemp = "";
            Int32 nCount = 0;
            string sMainLang = "";
            Int32 nMainLangID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select l.name,l.id from groups g,lu_languages l where l.id=g.language_id and  ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sMainLang = selectQuery.Table("query").DefaultView[0].Row["name"].ToString();
                    nMainLangID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            sTemp += "<li><a class=\"on\" href=\"";
            sTemp += "adm_channels_new.aspx?channel_id=" + Session["channel_id"].ToString();
            sTemp += "\"><span>";
            sTemp += sMainLang;
            sTemp += "</span></a></li>";

            //Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("media", "group_id", int.Parse(Session["media_id"].ToString())).ToString());
            ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
            selectQuery1 += "select l.name,l.id from group_extra_languages gel,lu_languages l where gel.language_id=l.id and l.status=1 and gel.status=1 and  ";
            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("l.id", "<>", nMainLangID);
            selectQuery1 += "and";
            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("gel.group_id", "=", nGroupID);
            selectQuery1 += " order by l.name";
            if (selectQuery1.Execute("query", true) != null)
            {
                Int32 nCount1 = selectQuery1.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount1; i++)
                {
                    Int32 nLangID = int.Parse(selectQuery1.Table("query").DefaultView[i].Row["id"].ToString());
                    string nLangName = selectQuery1.Table("query").DefaultView[i].Row["name"].ToString();
                    sTemp += "<li><a href=\"";
                    sTemp += "adm_channel_translate.aspx?channel_id=" + Session["channel_id"].ToString() + "&lang_id=" + nLangID.ToString();
                    sTemp += "\"><span>";
                    sTemp += nLangName;
                    sTemp += "</span></a></li>";
                }
                if (nCount1 == 0)
                    sTemp = "";
            }
            selectQuery1.Finish();
            selectQuery1 = null;

            return sTemp;
        }
        catch
        {
            HttpContext.Current.Response.Redirect("login.html");
            return "";
        }
    }

    public void GetHeader()
    {
        string sRet = PageUtils.GetPreHeader() + ": Channels";
        if (Session["channel_id"] != null && Session["channel_id"].ToString() != "" && Session["channel_id"].ToString() != "0")
            sRet += " - Edit";
        else
            sRet += " - New";
        Response.Write(sRet);
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

    protected string GetSafeStrVal(ref ODBCWrapper.DataSetSelectQuery selectQuery, string sField)
    {
        string sRet = "";
        object oVal = selectQuery.Table("query").DefaultView[0].Row[sField];
        if (oVal != DBNull.Value && oVal != null)
            sRet = oVal.ToString();
        return sRet;
    }

    protected void AddOrderBy(ref DBRecordWebEditor theRecord, bool bIsAuto)
    {
        string META1_STR_NAME = "";
        string META2_STR_NAME = "";
        string META3_STR_NAME = "";
        string META4_STR_NAME = "";
        string META5_STR_NAME = "";
        string META6_STR_NAME = "";
        string META7_STR_NAME = "";
        string META8_STR_NAME = "";
        string META9_STR_NAME = "";
        string META10_STR_NAME = "";
        string META11_STR_NAME = "";
        string META12_STR_NAME = "";
        string META13_STR_NAME = "";
        string META14_STR_NAME = "";
        string META15_STR_NAME = "";
        string META16_STR_NAME = "";
        string META17_STR_NAME = "";
        string META18_STR_NAME = "";
        string META19_STR_NAME = "";
        string META20_STR_NAME = "";

        string META1_DOUBLE_NAME = "";
        string META2_DOUBLE_NAME = "";
        string META3_DOUBLE_NAME = "";
        string META4_DOUBLE_NAME = "";
        string META5_DOUBLE_NAME = "";
        string META6_DOUBLE_NAME = "";
        string META7_DOUBLE_NAME = "";
        string META8_DOUBLE_NAME = "";
        string META9_DOUBLE_NAME = "";
        string META10_DOUBLE_NAME = "";
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from groups where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", LoginManager.GetLoginGroupID());
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                META1_STR_NAME = GetSafeStrVal(ref selectQuery, "META1_STR_NAME");
                META2_STR_NAME = GetSafeStrVal(ref selectQuery, "META2_STR_NAME");
                META3_STR_NAME = GetSafeStrVal(ref selectQuery, "META3_STR_NAME");
                META4_STR_NAME = GetSafeStrVal(ref selectQuery, "META4_STR_NAME");
                META5_STR_NAME = GetSafeStrVal(ref selectQuery, "META5_STR_NAME");
                META6_STR_NAME = GetSafeStrVal(ref selectQuery, "META6_STR_NAME");
                META7_STR_NAME = GetSafeStrVal(ref selectQuery, "META7_STR_NAME");
                META8_STR_NAME = GetSafeStrVal(ref selectQuery, "META8_STR_NAME");
                META9_STR_NAME = GetSafeStrVal(ref selectQuery, "META9_STR_NAME");
                META10_STR_NAME = GetSafeStrVal(ref selectQuery, "META10_STR_NAME");

                META11_STR_NAME = GetSafeStrVal(ref selectQuery, "META11_STR_NAME");
                META12_STR_NAME = GetSafeStrVal(ref selectQuery, "META12_STR_NAME");
                META13_STR_NAME = GetSafeStrVal(ref selectQuery, "META13_STR_NAME");
                META14_STR_NAME = GetSafeStrVal(ref selectQuery, "META14_STR_NAME");
                META15_STR_NAME = GetSafeStrVal(ref selectQuery, "META15_STR_NAME");
                META16_STR_NAME = GetSafeStrVal(ref selectQuery, "META16_STR_NAME");
                META17_STR_NAME = GetSafeStrVal(ref selectQuery, "META17_STR_NAME");
                META18_STR_NAME = GetSafeStrVal(ref selectQuery, "META18_STR_NAME");
                META19_STR_NAME = GetSafeStrVal(ref selectQuery, "META19_STR_NAME");
                META20_STR_NAME = GetSafeStrVal(ref selectQuery, "META20_STR_NAME");

                META1_DOUBLE_NAME = GetSafeStrVal(ref selectQuery, "META1_DOUBLE_NAME");
                META2_DOUBLE_NAME = GetSafeStrVal(ref selectQuery, "META2_DOUBLE_NAME");
                META3_DOUBLE_NAME = GetSafeStrVal(ref selectQuery, "META3_DOUBLE_NAME");
                META4_DOUBLE_NAME = GetSafeStrVal(ref selectQuery, "META4_DOUBLE_NAME");
                META5_DOUBLE_NAME = GetSafeStrVal(ref selectQuery, "META5_DOUBLE_NAME");

                META6_DOUBLE_NAME = GetSafeStrVal(ref selectQuery, "META6_DOUBLE_NAME");
                META7_DOUBLE_NAME = GetSafeStrVal(ref selectQuery, "META7_DOUBLE_NAME");
                META8_DOUBLE_NAME = GetSafeStrVal(ref selectQuery, "META8_DOUBLE_NAME");
                META9_DOUBLE_NAME = GetSafeStrVal(ref selectQuery, "META9_DOUBLE_NAME");
                META10_DOUBLE_NAME = GetSafeStrVal(ref selectQuery, "META10_DOUBLE_NAME");
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        DataTable d = new DataTable();
        Int32 n = 0;
        string s = "";
        d.Columns.Add(PageUtils.GetColumn("ID", n));
        d.Columns.Add(PageUtils.GetColumn("txt", s));

        System.Data.DataRow tmpRow = null;

        tmpRow = d.NewRow();
        tmpRow["ID"] = -6;
        tmpRow["txt"] = "Random";
        d.Rows.InsertAt(tmpRow, 0);
        d.AcceptChanges();

        tmpRow = d.NewRow();
        tmpRow["ID"] = -11;
        tmpRow["txt"] = "A.B.C";
        d.Rows.InsertAt(tmpRow, 0);
        d.AcceptChanges();

        tmpRow = d.NewRow();
        tmpRow["ID"] = -8;
        tmpRow["txt"] = "Rating";
        d.Rows.InsertAt(tmpRow, 0);
        d.AcceptChanges();

        tmpRow = d.NewRow();
        tmpRow["ID"] = -80;
        tmpRow["txt"] = "Votes";
        d.Rows.InsertAt(tmpRow, 0);
        d.AcceptChanges();

        tmpRow = d.NewRow();
        tmpRow["ID"] = -7;
        tmpRow["txt"] = "Views";
        d.Rows.InsertAt(tmpRow, 0);
        d.AcceptChanges();

        tmpRow = d.NewRow();
        tmpRow["ID"] = -10;
        tmpRow["txt"] = "Start Date";
        d.Rows.InsertAt(tmpRow, 0);
        d.AcceptChanges();

        tmpRow = d.NewRow();
        tmpRow["ID"] = -9;
        tmpRow["txt"] = "Likes";
        d.Rows.InsertAt(tmpRow, 0);
        d.AcceptChanges();

        tmpRow = d.NewRow();
        tmpRow["ID"] = -12;
        tmpRow["txt"] = "Create Date";
        d.Rows.InsertAt(tmpRow, 0);
        d.AcceptChanges();

        if (bIsAuto == false)
        {
            tmpRow = d.NewRow();
            tmpRow["ID"] = 0;
            tmpRow["txt"] = "Order Num";
            d.Rows.InsertAt(tmpRow, 0);
            d.AcceptChanges();
        }

        SafeMerge(META1_STR_NAME, "META1_STR_NAME", 1, ref d);
        SafeMerge(META2_STR_NAME, "META2_STR_NAME", 2, ref d);
        SafeMerge(META3_STR_NAME, "META3_STR_NAME", 3, ref d);
        SafeMerge(META4_STR_NAME, "META4_STR_NAME", 4, ref d);
        SafeMerge(META5_STR_NAME, "META5_STR_NAME", 5, ref d);
        SafeMerge(META6_STR_NAME, "META6_STR_NAME", 6, ref d);
        SafeMerge(META7_STR_NAME, "META7_STR_NAME", 7, ref d);
        SafeMerge(META8_STR_NAME, "META8_STR_NAME", 8, ref d);
        SafeMerge(META9_STR_NAME, "META9_STR_NAME", 9, ref d);
        SafeMerge(META10_STR_NAME, "META10_STR_NAME", 10, ref d);

        SafeMerge(META11_STR_NAME, "META11_STR_NAME", 11, ref d);
        SafeMerge(META12_STR_NAME, "META12_STR_NAME", 12, ref d);
        SafeMerge(META13_STR_NAME, "META13_STR_NAME", 13, ref d);
        SafeMerge(META14_STR_NAME, "META14_STR_NAME", 14, ref d);
        SafeMerge(META15_STR_NAME, "META15_STR_NAME", 15, ref d);
        SafeMerge(META16_STR_NAME, "META16_STR_NAME", 16, ref d);
        SafeMerge(META17_STR_NAME, "META17_STR_NAME", 17, ref d);
        SafeMerge(META18_STR_NAME, "META18_STR_NAME", 18, ref d);
        SafeMerge(META19_STR_NAME, "META19_STR_NAME", 19, ref d);
        SafeMerge(META20_STR_NAME, "META20_STR_NAME", 20, ref d);

        SafeMerge(META1_DOUBLE_NAME, "META1_DOUBLE_NAME", 21, ref d);
        SafeMerge(META2_DOUBLE_NAME, "META2_DOUBLE_NAME", 22, ref d);
        SafeMerge(META3_DOUBLE_NAME, "META3_DOUBLE_NAME", 23, ref d);
        SafeMerge(META4_DOUBLE_NAME, "META4_DOUBLE_NAME", 24, ref d);
        SafeMerge(META5_DOUBLE_NAME, "META5_DOUBLE_NAME", 25, ref d);

        SafeMerge(META6_DOUBLE_NAME, "META6_DOUBLE_NAME", 26, ref d);
        SafeMerge(META7_DOUBLE_NAME, "META7_DOUBLE_NAME", 27, ref d);
        SafeMerge(META8_DOUBLE_NAME, "META8_DOUBLE_NAME", 28, ref d);
        SafeMerge(META9_DOUBLE_NAME, "META9_DOUBLE_NAME", 29, ref d);
        SafeMerge(META10_DOUBLE_NAME, "META10_DOUBLE_NAME", 30, ref d);

        DataRecordRadioField dr_channels_order_by_types = new DataRecordRadioField("lu_channels_order_by_types", "description", "id", "", null);
        dr_channels_order_by_types.Initialize("Order By", "adm_table_header_nbg", "FormInput", "ORDER_BY_TYPE", true);
        dr_channels_order_by_types.SetSelectsDT(d);
        dr_channels_order_by_types.SetDefault(0);
        theRecord.AddRecord(dr_channels_order_by_types);

    }

    static protected void SafeMerge(string sMetaVal, string sMetaName, Int32 nID, ref DataTable d)
    {
        DataTable dToMerge = GetOrderByPart(sMetaVal, sMetaName, nID);
        if (dToMerge.DefaultView.Count > 0 && dToMerge.DefaultView[0].Row["txt"].ToString() != "")
            d.Merge(dToMerge);
    }

    static protected DataTable GetOrderByPart(string sMetaVal, string sMetaName, Int32 nID)
    {
        DataTable d = null;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select " + sMetaName + " as txt," + nID.ToString() + " as ID from groups where ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", LoginManager.GetLoginGroupID());
        if (selectQuery.Execute("query", true) != null)
        {
            d = selectQuery.Table("query").Copy();
        }
        selectQuery.Finish();
        selectQuery = null;
        return d;
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }

        object channelId = null;

        if (Session["channel_id"] != null && Session["channel_id"].ToString() != "" && int.Parse(Session["channel_id"].ToString()) != 0)
        {
            channelId = Session["channel_id"];
        }

        DBRecordWebEditor theRecord = new DBRecordWebEditor("channels", "adm_table_pager", 
            "adm_channels.aspx?search_save=1", "", "ID", channelId, "adm_channels.aspx?search_save=1", "channel_id");

        DataRecordShortIntField dr_order_num = new DataRecordShortIntField(true, 3, 3);
        dr_order_num.Initialize("Order number", "adm_table_header_nbg", "FormInput", "ORDER_NUM", true);
        dr_order_num.SetDefault(1);
        theRecord.AddRecord(dr_order_num);

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "NAME", true);
        theRecord.AddRecord(dr_name);

        DataRecordShortTextField dr_admin_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_admin_name.Initialize("Unique Name", "adm_table_header_nbg", "FormInput", "ADMIN_NAME", true);
        theRecord.AddRecord(dr_admin_name);

        DataRecordLongTextField dr_bio = new DataRecordLongTextField("ltr", true, 60, 20);
        dr_bio.Initialize("Description", "adm_table_header_nbg", "FormInput", "DESCRIPTION", false);
        theRecord.AddRecord(dr_bio);

         if (channelId != null && !string.IsNullOrEmpty(channelId.ToString()))
        {
            bool isDownloadPicWithImageServer = false;
            string imageUrl = string.Empty;
            int picId = 0;

            if (ImageUtils.IsDownloadPicWithImageServer())
            {
                isDownloadPicWithImageServer = true;
                int groupId = LoginManager.GetLoginGroupID();
                imageUrl = GetPicImageUrlByRatio(channelId, groupId, out picId);
            }
            
            DataRecordOnePicBrowserField dr_logo_Pic = new DataRecordOnePicBrowserField("channel", isDownloadPicWithImageServer, imageUrl, picId);
            dr_logo_Pic.Initialize("Pic", "adm_table_header_nbg", "FormInput", "PIC_ID", false);
            dr_logo_Pic.SetDefault(0);
            theRecord.AddRecord(dr_logo_Pic);
        }

        DataRecordBrowserField dr_asset_types = new DataRecordBrowserField("OpenAssetTypeBrowser", "adm_ksql_channel_new.aspx");
        dr_asset_types.Initialize("Asset Type", "adm_table_header_nbg", "FormInput", "ID", false);
        dr_asset_types.SetClassName("btn_assetTypes");
        theRecord.AddRecord(dr_asset_types);

        AddOrderBy(ref theRecord, true);

        DataRecordRadioField dr_channels_order_by_dir = new DataRecordRadioField("lu_channels_order_by_DIR", "description", "id", "", null);
        dr_channels_order_by_dir.Initialize("Order Direction", "adm_table_header_nbg", "FormInput", "ORDER_BY_DIR", true);
        dr_channels_order_by_dir.SetDefault(0);
        theRecord.AddRecord(dr_channels_order_by_dir);

        DataRecordLongTextField dr_edit_data = new DataRecordLongTextField("rtl", true, 60, 10);
        dr_edit_data.Initialize("Editor remarks", "adm_table_header_nbg", "FormInput", "EDITOR_REMARKS", false);
        theRecord.AddRecord(dr_edit_data);


        DataRecordLongTextField dr_filter = new DataRecordLongTextField("ltr", true, 60, 20, true);
        dr_filter.Initialize("KSQL Expression", "adm_table_header_nbg", "FormInput", "KSQL_FILTER", false);
        theRecord.AddRecord(dr_filter);

        DataRecordShortIntField dr_channel_type = new DataRecordShortIntField(false, 3, 3);
        dr_channel_type.Initialize("Channel type", "adm_table_header_nbg", "FormInput", "CHANNEL_TYPE", false);
        dr_channel_type.SetValue("4");
        
        theRecord.AddRecord(dr_channel_type);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string tableHTML = theRecord.GetTableHTML("adm_ksql_channel_new.aspx?submited=1");

        return tableHTML;
    }

    public string changeItemStatus(string sID, string sAction)
    {
        Int32 groupID = LoginManager.GetLoginGroupID();
        Int32 nStatus = 0;
        int mediaTypeID = int.Parse(sID);
        int channelID = 0;

        if (Session["channel_id"] != null && Session["channel_id"] != DBNull.Value && Convert.ToInt32(Session["channel_id"]) != 0)
        {
            channelID = Convert.ToInt32(Session["channel_id"]);
        }

        if (channelID != 0)
        {
            Int32 channelMediaTypeID = GetChannelAssetType(mediaTypeID, channelID, groupID, ref nStatus);

            if (channelMediaTypeID != 0)
            {
                if (nStatus == 0)
                    UpdateChannelMediaType(channelMediaTypeID, 1, groupID, channelID);
                else
                    UpdateChannelMediaType(channelMediaTypeID, 0, groupID, channelID);
            }
            else
            {
                InsertChannelMediaType(mediaTypeID, channelID, groupID);
            }
        }
        else
        {
            // save media type id values to associate with cjannel (after get channelId)
            List<int> assetTypeList = new List<int>();
            if (Session["asset_type_ids"] != null && Session["asset_type_ids"] is List<int>)
            {
                assetTypeList = Session["asset_type_ids"] as List<int>;
            }
            assetTypeList.Add(mediaTypeID);
            Session["asset_type_ids"] = assetTypeList;

        }
        return "";
    }

    private void InsertChannelMediaType(int mediaTypeID, int channelID, int groupID)
    {
        bool inserted = TvmDAL.InsertChannelMediaType(groupID, channelID, new List<int>() { mediaTypeID });
    }

    private void InsertChannelAssetType(List<int> assetTypeIds, int channelID, int groupID)
    {
        bool inserted = TvmDAL.Insert_ChannelAssetType(groupID, channelID, assetTypeIds);

        if (inserted)
        {
            bool result = ImporterImpl.UpdateChannelIndex(groupID, new List<int>() { channelID }, ApiObjects.eAction.Update);
        }
    }

    private void UpdateChannelMediaType(int channelMediaTypeID, int status, int groupID, int channelID)
    {
        bool updated = TvmDAL.UpdateChannelMediaType(channelMediaTypeID, status, groupID, channelID);
        if (updated)
        {
            bool result = ImporterImpl.UpdateChannelIndex(groupID, new List<int>() { channelID }, ApiObjects.eAction.Update);
        }
    }

    public string initDualObj()
    {
        string sRet = "";
        sRet += "Asset Types included in Channels";
        sRet += "~~|~~";
        sRet += "Available Asset Types";
        sRet += "~~|~~";
        sRet += "<root>";
        int channelID = 0;

        if (Session["channel_id"] != null && !string.IsNullOrEmpty(Session["channel_id"].ToString()))
        {
            channelID = Int32.Parse(Session["channel_id"].ToString());
        }

        List<KeyValuePair<string, string>> mediaTypeByGroup = null;
        List<KeyValuePair<string, string>> mediaTypeByChannel = null;

        BuildAssetTypes(channelID, LoginManager.GetLoginGroupID(), ref mediaTypeByGroup, ref mediaTypeByChannel);

        if (mediaTypeByGroup != null && mediaTypeByGroup.Count > 0)
        {
            foreach (KeyValuePair<string, string> kvp in mediaTypeByGroup)
            {
                sRet += "<item id=\"" + kvp.Key + "\"  title=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(kvp.Value, true) + "\" description=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(string.Empty, true) + "\" inList=\"false\" />";
            }
        }

        if (mediaTypeByChannel != null && mediaTypeByChannel.Count > 0)
        {
            foreach (KeyValuePair<string, string> kvp in mediaTypeByChannel)
            {
                sRet += "<item id=\"" + kvp.Key + "\"  title=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(kvp.Value, true) + "\" description=\"" + TVinciShared.ProtocolsFuncs.XMLEncode(string.Empty, true) + "\" inList=\"true\" />";
            }
        }

        sRet += "</root>";
        return sRet;
    }

    private void BuildAssetTypes(int channelID, int groupID, ref List<KeyValuePair<string, string>> mediaTypeByGroup, ref List<KeyValuePair<string, string>> mediaTypeByChannel)
    {
        DataSet ds = TvmDAL.Get_ChannelMediaTypes(groupID, channelID);
        if (ds != null && ds.Tables != null && ds.Tables.Count == 2)
        {
            DataTable mediaTypeByGroupDT = ds.Tables[0];

            // Manually add EPG type
            mediaTypeByGroupDT.Rows.Add(GroupsCacheManager.Channel.EPG_ASSET_TYPE, "EPG");

            DataTable mediaTypeByChannelDT = ds.Tables[1];
            mediaTypeByGroup = new List<KeyValuePair<string, string>>();
            mediaTypeByChannel = new List<KeyValuePair<string, string>>();

            if (mediaTypeByGroupDT != null && mediaTypeByGroupDT.Rows != null && mediaTypeByGroupDT.Rows.Count > 0)
            {
                mediaTypeByGroup = new List<KeyValuePair<string, string>>();
                foreach (DataRow dr in mediaTypeByGroupDT.Rows)
                {
                    string sID = ODBCWrapper.Utils.GetSafeStr(dr, "ID");
                    string sName = ODBCWrapper.Utils.GetSafeStr(dr, "NAME");
                    mediaTypeByGroup.Add(new KeyValuePair<string, string>(sID, sName));
                }
            }

            if (mediaTypeByChannelDT != null && mediaTypeByChannelDT.Rows != null && mediaTypeByChannelDT.Rows.Count > 0)
            {
                foreach (DataRow dr in mediaTypeByChannelDT.Rows)
                {
                    string sID = ODBCWrapper.Utils.GetSafeStr(dr, "ID");
                    string sName = ODBCWrapper.Utils.GetSafeStr(dr, "NAME");
                    mediaTypeByChannel.Add(new KeyValuePair<string, string>(sID, sName));
                }
            }
        }
    }

    private Int32 GetChannelAssetType(Int32 mediaTypeID, Int32 channelID, Int32 groupID, ref int status)
    {
        Int32 result = 0;
        DataTable dt = TvmDAL.GetChannelMediaType(groupID, channelID, mediaTypeID);
        
        if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
        {
            DataRow dr = dt.Rows[0];
            result = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
            status = ODBCWrapper.Utils.GetIntSafeVal(dr, "STATUS");
        }

        return result;
    }

    private string GetPicImageUrlByRatio(object channelId, int groupId, out int picId)
    {
        string imageUrl = string.Empty;
        string baseUrl = string.Empty;
        int ratioId = 0;
        int version = 0;
        picId = 0;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select p.RATIO_ID, p.BASE_URL, p.ID, p.version from pics p left join channels c on c.PIC_ID = p.ID where p.STATUS in (0, 1) and c.id = " + channelId.ToString();

        if (selectQuery.Execute("query", true) != null && selectQuery.Table("query").DefaultView != null && selectQuery.Table("query").DefaultView.Count > 0)
        {

            baseUrl = ODBCWrapper.Utils.GetSafeStr(selectQuery.Table("query").DefaultView[0].Row["BASE_URL"]);
            ratioId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row["RATIO_ID"]);
            picId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row["ID"]);
            version = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row["version"]);
            int parentGroupID = DAL.UtilsDal.GetParentGroupID(groupId);

            imageUrl = PageUtils.BuildVodUrl(parentGroupID, baseUrl, ratioId, version);
        }

        return imageUrl;
    }
}
