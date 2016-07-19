using DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using TvinciImporter;
using TVinciShared;

public partial class adm_channels_new : System.Web.UI.Page
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
                bool result;
                int nId = DBManipulator.DoTheWork();

                if (nId != 0)
                {
                    int loginGroupID = LoginManager.GetLoginGroupID();
                    //Update MediaType if its new channel
                    if (Session["media_type_ids"] != null && Session["media_type_ids"] is List<int>)
                    {
                        List<int> updatedMediaType = Session["media_type_ids"] as List<int>;
                        InsertChannelMediaType(updatedMediaType, nId, loginGroupID);
                        Session["media_type_ids"] = null;
                    }

                    //Update channel at Lucene/ ES

                    result = ImporterImpl.UpdateChannelIndex(loginGroupID, new List<int>() { nId }, ApiObjects.eAction.Update);
                }
                return;
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
                Session["channel_id"] = int.Parse(Request.QueryString["channel_id"].ToString());
            else
                Session["channel_id"] = 0;

            if (Request.QueryString["channel_type"] != null &&
                Request.QueryString["channel_type"].ToString() != "")
                Session["channel_type"] = int.Parse(Request.QueryString["channel_type"].ToString());
            else
            {
                if (Session["channel_id"] != null &&
                    Session["channel_id"].ToString() != "" &&
                    Session["channel_id"].ToString() != "0")
                    StartChannelType();
                else
                    LoginManager.LogoutFromSite("login.aspx");
            }
            Int32 nOwnerGroupID = LoginManager.GetLoginGroupID();
            m_sLangMenu = GetLangMenu(nOwnerGroupID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 2, false);
        }
    }

    protected void GetLangMenu()
    {
        Response.Write(m_sLangMenu);
    }

    protected void StartChannelType()
    {
        Session["channel_type"] = int.Parse(PageUtils.GetTableSingleVal("channels", "CHANNEL_TYPE", int.Parse(Session["channel_id"].ToString())).ToString());
    }

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

    protected void AddCutBy(ref DBRecordWebEditor theRecord)
    {
        List<string> tags = new List<string>();
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += " select * from media_tags_types where status=1 and group_id " + PageUtils.GetGroupsStrByParent(LoginManager.GetLoginGroupID());
        selectQuery += " order by order_num";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string sTagTypeName = selectQuery.Table("query").DefaultView[i].Row["NAME"].ToString();
                if (!tags.Contains(sTagTypeName.ToLower()))
                {
                    tags.Add(sTagTypeName.ToLower());
                    Int32 nTagTypeID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                    DataRecordMultiField dr_tags = new DataRecordMultiField("tags", "id", "id", "channel_tags", "channel_id", "TAG_ID", true, "ltr", 60, "tags");
                    dr_tags.Initialize(sTagTypeName, "adm_table_header_nbg", "FormInput", "VALUE", false);
                    dr_tags.SetCollectionLength(8);
                    dr_tags.SetExtraWhere("TAG_TYPE_ID=" + nTagTypeID.ToString());
                    theRecord.AddRecord(dr_tags);
                }
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        {
            DataRecordMultiField dr_tags = new DataRecordMultiField("tags", "id", "id", "channel_tags", "channel_id", "TAG_ID", true, "ltr", 60, "tags");
            dr_tags.Initialize("Free", "adm_table_header_nbg", "FormInput", "VALUE", false);
            dr_tags.SetCollectionLength(8);
            dr_tags.SetExtraWhere("TAG_TYPE_ID=0");
            theRecord.AddRecord(dr_tags);
        }
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

        if (bIsAuto)
        {
            DataRecordRadioField dr_IsSlidingWindow = new DataRecordRadioField("lu_on_off", "description", "id", "", null);
            dr_IsSlidingWindow.Initialize("Sliding Window enabled", "adm_table_header_nbg", "FormInput", "IsSlidingWindow", true);
            dr_IsSlidingWindow.SetDefault(0);
            theRecord.AddRecord(dr_IsSlidingWindow);

            DataRecordDropDownField dr_SlidingWindowPeriod = new DataRecordDropDownField("lu_min_periods", "description", "id", "", null, 90, false);
            dr_SlidingWindowPeriod.Initialize("Window period", "adm_table_header_nbg", "FormInput", "SlidingWindowPeriod", true);
            theRecord.AddRecord(dr_SlidingWindowPeriod);
        }

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

    protected void AddStrFields(ref DBRecordWebEditor theRecord)
    {
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from groups where status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", LoginManager.GetLoginGroupID());
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                for (int i = 1; i < 21; i++)
                {
                    string sFieldName = "META" + i.ToString() + "_STR_NAME";
                    object oName = selectQuery.Table("query").DefaultView[0].Row[sFieldName];
                    if (oName != DBNull.Value && oName != null && oName.ToString() != "")
                    {
                        string sName = oName.ToString();
                        string sField = "META" + i.ToString() + "_STR";
                        DataRecordLongTextField dr_name = new DataRecordLongTextField("ltr", true, 60, 3);
                        dr_name.Initialize(sName, "adm_table_header_nbg", "FormInput", sField, false);
                        theRecord.AddRecord(dr_name);
                    }
                }
            }
        }
        selectQuery.Finish();
        selectQuery = null;
    }

    protected void AddIntFields(ref DBRecordWebEditor theRecord)
    {
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from groups where status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", LoginManager.GetLoginGroupID());
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                for (int i = 1; i < 11; i++)
                {
                    string sFieldName = "META" + i.ToString() + "_DOUBLE_NAME";
                    object oName = selectQuery.Table("query").DefaultView[0].Row[sFieldName];
                    if (oName != DBNull.Value && oName != null && oName.ToString() != "")
                    {
                        string sName = oName.ToString();
                        string sField = "META" + i.ToString() + "_DOUBLE";
                        string sFieldCB = "USE_META" + i.ToString() + "_DOUBLE";



                        DataRecordCheckBoxField dr_use = new DataRecordCheckBoxField(true);
                        dr_use.Initialize("Cut Using " + sName, "adm_table_header_nbg", "FormInput", sFieldCB, false);
                        //theRecord.AddRecord(dr_use);
                                                
                        DataRecordShortDoubleField dr_name = new DataRecordShortDoubleField(true, 12, 12);
                        dr_name.Initialize("", "adm_table_header_nbg", "FormInput", sField, false);
                        //theRecord.AddRecord(dr_name);

                        DataRecordCutWithDoubleField dr_cut = new DataRecordCutWithDoubleField(ref dr_use, ref dr_name, "Check to cut using this field");
                        dr_cut.Initialize(sName, "adm_table_header_nbg", "FormInput", sField, false);
                        theRecord.AddRecord(dr_cut);
                    }
                }
            }
        }
        selectQuery.Finish();
        selectQuery = null;
    }

    protected void AddBoolFields(ref DBRecordWebEditor theRecord)
    {
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select * from groups where status=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", LoginManager.GetLoginGroupID());
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                for (int i = 1; i < 11; i++)
                {
                    string sFieldName = "META" + i.ToString() + "_BOOL_NAME";
                    object oName = selectQuery.Table("query").DefaultView[0].Row[sFieldName];
                    if (oName != DBNull.Value && oName != null && oName.ToString() != "")
                    {
                        string sName = oName.ToString();
                        string sField = "META" + i.ToString() + "_BOOL";
                        string sFieldCB = "USE_META" + i.ToString() + "_BOOL";

                        DataRecordCheckBoxField dr_use = new DataRecordCheckBoxField(true);
                        dr_use.Initialize(sName, "adm_table_header_nbg", "FormInput", sFieldCB, false);
                        //theRecord.AddRecord(dr_use);

                        DataRecordBoolField dr_name = new DataRecordBoolField(true);
                        dr_name.Initialize("", "adm_table_header_nbg", "FormInput", sField, false);
                        //theRecord.AddRecord(dr_name);

                        DataRecordCutWithBoolField dr_cut = new DataRecordCutWithBoolField(ref dr_use, ref dr_name, "Check to cut using this field");
                        dr_cut.Initialize(sName, "adm_table_header_nbg", "FormInput", sField, false);
                        theRecord.AddRecord(dr_cut);


                    }
                }
            }
        }
        selectQuery.Finish();
        selectQuery = null;
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object channelId = null; ;
        if (Session["channel_id"] != null && Session["channel_id"].ToString() != "" && int.Parse(Session["channel_id"].ToString()) != 0)
            channelId = Session["channel_id"];
        DBRecordWebEditor theRecord = new DBRecordWebEditor("channels", "adm_table_pager", "adm_channels.aspx?search_save=1", "", "ID", channelId, "adm_channels.aspx?search_save=1", "channel_id");

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

        //DataRecordCheckBoxField dr_rss = new DataRecordCheckBoxField(true);
        //dr_rss.Initialize("Enable feed ", "adm_table_header_nbg", "FormInput", "IS_RSS", false);
        //theRecord.AddRecord(dr_rss);

        int channelID = 0;
        string categories = "No Categories";
        if (channelId != null && int.TryParse(channelId.ToString(), out channelID))
        {
            categories = GetChannelCategories(channelID);
        }

        DataRecordShortTextField dr_categories = new DataRecordShortTextField("ltr", false, 60, 20);
        dr_categories.Initialize("Categories", "adm_table_header_nbg", "FormInput", "", false);
        dr_categories.SetValue(categories);
        theRecord.AddRecord(dr_categories);

        //DataRecordTimeField dr_relevant_time = new DataRecordTimeField();
        //dr_relevant_time.Initialize("Linear Start Time", "adm_table_header_nbg", "FormInput", "LINEAR_START_TIME", false);
        //theRecord.AddRecord(dr_relevant_time);

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

        //string sDefPT = "";
        //object oDefPT = ODBCWrapper.Utils.GetTableSingleVal("groups", "DEFAULT_PLAYLIST_TEMPLATE_ID", LoginManager.GetLoginGroupID());
        //if (oDefPT != DBNull.Value && oDefPT != null)
        //    sDefPT = oDefPT.ToString();

        //DataRecordDropDownField dr_pli_template = new DataRecordDropDownField("play_list_items_templates_types", "NAME", "id", "", null, 60, true);
        //string sQuery = "select name as txt,id as id from play_list_items_templates_types where status=1 and is_active=1 and group_id= " + LoginManager.GetLoginGroupID().ToString();
        //dr_pli_template.SetSelectsQuery(sQuery);
        //dr_pli_template.Initialize("Playlist schema", "adm_table_header_nbg", "FormInput", "PLAYLIST_TEMPLATE_ID", false);
        //dr_pli_template.SetNoSelectStr("---");
        //dr_pli_template.SetDefaultVal(sDefPT);
        //theRecord.AddRecord(dr_pli_template);

        if (int.Parse(Session["channel_type"].ToString()) == 1)
        {
            DataRecordRadioField dr_cut_type = new DataRecordRadioField("lu_cut_type", "description", "id", "", null);
            dr_cut_type.Initialize("Cut Tags Type", "adm_table_header_nbg", "FormInput", "IS_AND", false);
            theRecord.AddRecord(dr_cut_type);

            DataRecordBrowserField dr_media_types = new DataRecordBrowserField("OpenMediaTypeBrowser", "adm_channels_new.aspx");
            dr_media_types.Initialize("Media Type", "adm_table_header_nbg", "FormInput", "ID", false);
            theRecord.AddRecord(dr_media_types);

            AddStrFields(ref theRecord);
            AddIntFields(ref theRecord);
            AddBoolFields(ref theRecord);
            AddCutBy(ref theRecord);
            AddOrderBy(ref theRecord, true);
        }
        else
        {
            AddOrderBy(ref theRecord, false);
        }

        DataRecordRadioField dr_channels_order_by_dir = new DataRecordRadioField("lu_channels_order_by_DIR", "description", "id", "", null);
        dr_channels_order_by_dir.Initialize("Order Direction", "adm_table_header_nbg", "FormInput", "ORDER_BY_DIR", true);
        dr_channels_order_by_dir.SetDefault(0);
        theRecord.AddRecord(dr_channels_order_by_dir);

        DataRecordLongTextField dr_edit_data = new DataRecordLongTextField("rtl", true, 60, 10);
        dr_edit_data.Initialize("Editor remarks", "adm_table_header_nbg", "FormInput", "EDITOR_REMARKS", false);
        theRecord.AddRecord(dr_edit_data);

        DataRecordShortIntField dr_channel_type = new DataRecordShortIntField(false, 3, 3);
        dr_channel_type.Initialize("Channel type", "adm_table_header_nbg", "FormInput", "CHANNEL_TYPE", false);
        if (int.Parse(Session["channel_type"].ToString()) == 1)
            dr_channel_type.SetValue("1");
        else
            dr_channel_type.SetValue("2");
        theRecord.AddRecord(dr_channel_type);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        string sTable = theRecord.GetTableHTML("adm_channels_new.aspx?submited=1");

        return sTable;
    }

    public string changeItemStatus(string sID, string sAction)
    {
        Int32 groupID = LoginManager.GetLoginGroupID();
        Int32 nStatus = 0;
        int mediaTypeID = int.Parse(sID);
        int channelID = 0;

        if (Session["channel_id"] != null && Session["channel_id"].ToString() != "" && int.Parse(Session["channel_id"].ToString()) != 0)
            channelID = int.Parse(Session["channel_id"].ToString());
        if (channelID != 0)
        {
            Int32 channelMediaTypeID = GetChannelMediaType(mediaTypeID, channelID, groupID, ref nStatus);

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
            List<int> mediaTypeList = new List<int>();
            if (Session["media_type_ids"] != null && Session["media_type_ids"] is List<int>)
            {
                mediaTypeList = Session["media_type_ids"] as List<int>;
            }
            mediaTypeList.Add(mediaTypeID);
            Session["media_type_ids"] = mediaTypeList;

        }
        return "";
    }

    private void InsertChannelMediaType(int mediaTypeID, int channelID, int groupID)
    {
        bool inserted = TvmDAL.InsertChannelMediaType(groupID, channelID, new List<int>() { mediaTypeID });
    }

    private void InsertChannelMediaType(List<int> mediaTypeIDs, int channelID, int groupID)
    {
        bool inserted = TvmDAL.InsertChannelMediaType(groupID, channelID, mediaTypeIDs);
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
        sRet += "Media Types included in Channels";
        sRet += "~~|~~";
        sRet += "Available Media Types";
        sRet += "~~|~~";
        sRet += "<root>";
        int channelID = 0;
        if (Session["channel_id"] != null && !string.IsNullOrEmpty(Session["channel_id"].ToString()))
        {
            channelID = Int32.Parse(Session["channel_id"].ToString());
        }


        List<KeyValuePair<string, string>> mediaTypeByGroup = null;
        List<KeyValuePair<string, string>> mediaTypeByChannel = null;

        BuildMediaType(channelID, LoginManager.GetLoginGroupID(), ref mediaTypeByGroup, ref mediaTypeByChannel);

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

    private void BuildMediaType(int channelID, int groupID, ref List<KeyValuePair<string, string>> mediaTypeByGroup, ref List<KeyValuePair<string, string>> mediaTypeByChannel)
    {
        DataSet ds = TvmDAL.Get_ChannelMediaTypes(groupID, channelID);
        if (ds != null && ds.Tables != null && ds.Tables.Count == 2)
        {
            DataTable mediaTypeByGroupDT = ds.Tables[0];
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

    private Int32 GetChannelMediaType(Int32 mediaTypeID, Int32 channelID, Int32 groupID, ref int nStatus)
    {
        Int32 nRet = 0;
        DataTable dt = TvmDAL.GetChannelMediaType(groupID, channelID, mediaTypeID);
        if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
        {
            DataRow dr = dt.Rows[0];
            nRet = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
            nStatus = ODBCWrapper.Utils.GetIntSafeVal(dr, "STATUS");
        }
        return nRet;
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

    private string GetChannelCategories(int channelID)
    {
        List<string> categories = new List<string>();
        
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select c.admin_name from categories c left join categories_channels cc on c.ID=cc.CATEGORY_ID";
        selectQuery += "where c.IS_ACTIVE = 1 and c.STATUS = 1 and cc.STATUS = 1 and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("cc.CHANNEL_ID", "=", channelID);
        if (selectQuery.Execute("query", true) != null)
        {
            int nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                string category = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "admin_name", i);
                if (!string.IsNullOrEmpty(category))
                    categories.Add(category);
            }
        }
        selectQuery.Finish();
        selectQuery = null;

        return string.Join(";",categories.ToArray());
    }
}
