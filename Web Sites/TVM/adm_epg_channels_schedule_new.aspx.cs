using EpgBL;
using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;
using TvinciImporter;

public partial class adm_epg_channels_schedule_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected string m_sLangMenu;

    

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsPagePermitted("adm_epg_channels.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsActionPermittedOnPage("adm_epg_channels.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            Int32 nOwnerGroupID = LoginManager.GetLoginGroupID();
            m_sMenu = TVinciShared.Menu.GetMainMenu(6, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 3, true);
            
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {
                int epgID = 0;
                //if (Session["epg_channels_schedule_id"] != null && Session["epg_channels_schedule_id"].ToString() != "")
                //    epgID = int.Parse(Session["epg_channels_schedule_id"].ToString());

                int progID = DBManipulator.DoTheWork();//insert the EPG to DB first
                epgID = progID;

                //retreive all tags and Metas IDs from DB
                Dictionary<int, string> tagsDic = getMetaTag(false);
                Dictionary<int, string> metasDic = getMetaTag(true);
                int nGroupID = LoginManager.GetLoginGroupID();
                int nParentGroupID = DAL.UtilsDal.GetParentGroupID(nGroupID);
                TvinciEpgBL epgBLTvinci = new TvinciEpgBL(nParentGroupID);  //assuming this is a Kaltura user - the TVM does not support editing of yes Epg
                              
                EpgCB epg = epgBLTvinci.GetEpgCB((ulong)epgID);
                CouchBaseManipulator.DoTheWork(ref epg, metasDic, tagsDic); //update the data of the Epg from the page

                ulong nID = 0;
                if (epg.EpgID == 0)
                {
                    epg.EpgID = (ulong)epgID;
                    epgBLTvinci.InsertEpg(epg, out nID);
                }
                else
                {
                    epg.EpgID = (ulong)epgID;
                    epgBLTvinci.UpdateEpg(epg);
                }

                bool result = false;
               
                result = ImporterImpl.UpdateEpgIndex(new List<ulong>() { epg.EpgID }, nGroupID, eAction.Update);

                return;
            }

            if (Session["epg_channel_id"] == null || Session["epg_channel_id"].ToString() == "" || Session["epg_channel_id"].ToString() == "0")
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }

            if (Request.QueryString["epg_channels_schedule_id"] != null && Request.QueryString["epg_channels_schedule_id"].ToString() != "")
            {
                Session["epg_channels_schedule_id"] = int.Parse(Request.QueryString["epg_channels_schedule_id"].ToString());
                Int32 nOwnerGroupIDChannel = int.Parse(PageUtils.GetTableSingleVal("epg_channels", "group_id", int.Parse(Session["epg_channel_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupIDChannel && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["epg_channels_schedule_id"] = "0";

            m_sLangMenu = GetLangMenu(nOwnerGroupID);
        }
    }

    protected void GetLangMenu()
    {
        Response.Write(m_sLangMenu);
    }

    protected string GetLangMenu(Int32 nGroupID)
    {
        if (Session["epg_channels_schedule_id"] != null &&
            Session["epg_channels_schedule_id"].ToString() == "0")
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
            sTemp += "adm_epg_channels_schedule_new.aspx?epg_channels_schedule_id=" + Session["epg_channels_schedule_id"].ToString();
            sTemp += "\"><span>";
            sTemp += sMainLang;
            sTemp += "</span></a></li>";
            
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
                    sTemp += "adm_epg_channels_schedule_translate.aspx?epg_channels_schedule_id=" + Session["epg_channels_schedule_id"].ToString() + "&lang_id=" + nLangID.ToString();
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
            //HttpContext.Current.Response.Redirect("login.html");
            return "";
        }
    }

    public void GetHeader()
    {
        if (Session["epg_channels_schedule_id"] != null && Session["epg_channels_schedule_id"].ToString() != "" && int.Parse(Session["epg_channels_schedule_id"].ToString()) != 0)
        {
            //Int32 nEPGChannelID = int.Parse(PageUtils.GetTableSingleVal("epg_channels_schedule", "EPG_CHANNEL_ID", int.Parse(Session["epg_channels_schedule_id"].ToString())).ToString());
            Response.Write(PageUtils.GetPreHeader() + "EPG Channels schedule : " + PageUtils.GetTableSingleVal("epg_channels_schedule", "NAME", int.Parse(Session["epg_channels_schedule_id"].ToString())).ToString() + " - Edit");
        }
        else
            Response.Write(PageUtils.GetPreHeader() + "EPG Channels schedule - New");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }

  
    //get a Dictionary of the meta\tag ID and its type 
    protected Dictionary<int, string> getMetaTag(bool isMeta)
    {
        Dictionary<int, string> result = new Dictionary<int, string>();        
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += " select ID, name from";
        if (isMeta)
        {
            selectQuery += "epg_metas_types";
        }
        else
        {
            selectQuery += "EPG_tags_types";
        }
        selectQuery += "where status=1 and";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", LoginManager.GetLoginGroupID());

        if (!isMeta)
            selectQuery += "order by order_num";

        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            for (int i = 0; i < nCount; i++)
            {
                object oName = selectQuery.Table("query").DefaultView[i].Row["name"].ToString();
                if (oName != DBNull.Value && oName != null && oName.ToString() != "")
                    result.Add(int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString()), oName.ToString());
            }               
        }
        
        selectQuery.Finish();
        selectQuery = null;
        return result;
    }

    //add the display of all the metas
    protected void AddMetasFields(ref DBRecordWebEditor theRecord, EpgCB epg)
    {
        Dictionary<int,string> lMetas = getMetaTag(true);    
        // change the key in the dictionary from CB to lower letters!!!
        Dictionary<string, List<string>> tempMetas = new Dictionary<string, List<string>>();
        foreach (KeyValuePair<string, List<string>> kv in epg.Metas)
	  {
            tempMetas.Add(kv.Key.ToLower(), kv.Value);
        }

        if (tempMetas != null && tempMetas.Count > 0)
        {
            epg.Metas = tempMetas;
        }
	
	   foreach(int id in lMetas.Keys)
        {
            string sName = lMetas[id];
            DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128, id);
            dr_name.Initialize(sName, "adm_table_header_nbg", "FormInput", string.Empty, "", false);
            string sNameLower = sName.ToLower();                
            if (epg.Metas.Keys.Contains(sNameLower))
            {
                string val = "";
                val += epg.Metas[sNameLower][0]; //asumming each meta has only one value
                dr_name.SetValue(val);
            }
            theRecord.AddRecord(dr_name);  
        }  
    }
   
    //add the display of all the tags
    protected void AddTagsFields(ref DBRecordWebEditor theRecord, EpgCB epg)
    {      
        Dictionary<int, string> lTags = getMetaTag(false);
        foreach (int tagID in lTags.Keys)
        {
            string sName = lTags[tagID];
            DataRecordMultiField dr_tags = new DataRecordMultiField("epg_tags", "id", "id", "EPG_program_tags", "program_id", "epg_tag_id", true, "ltr", 60, "tags");///
            dr_tags.Initialize(sName, "adm_table_header_nbg", "FormInput", "VALUE", "", false);            
            dr_tags.SetCollectionLength(8);
            dr_tags.SetExtraWhere("epg_tag_type_id=" + tagID.ToString());
            string sNameLower = sName.ToLower();
            if (epg.Tags.Keys.Contains(sNameLower))
            {
                string val = "";
                foreach (string tagVal in epg.Tags[sNameLower])
                    val += tagVal + ";";
                dr_tags.SetValue(val);
            }
            theRecord.AddRecord(dr_tags);
        }
    }

    public string GetPageContent(string sOrderBy, string sPageNum)
    {
        if (Session["error_msg"] != null && Session["error_msg"].ToString() != "")
        {
            Session["error_msg"] = "";
            return Session["last_page_html"].ToString();
        }
        object t = null; ;
        if (Session["epg_channels_schedule_id"] != null && Session["epg_channels_schedule_id"].ToString() != "" && int.Parse(Session["epg_channels_schedule_id"].ToString()) != 0)
            t = Session["epg_channels_schedule_id"];
        string sBack = "adm_epg_channels_schedule.aspx?epg_channel_id=" + Session["epg_channel_id"].ToString();
        DBRecordWebEditor theRecord = new DBRecordWebEditor("epg_channels_schedule", "adm_table_pager", sBack, "", "ID", t, sBack, "epg_channel_id");

        if (t == null)//this is the epg ID
            t = 0;

        //Retrieving the EpgCB or generating one if needed        
        int nParentGroupID = DAL.UtilsDal.GetParentGroupID(LoginManager.GetLoginGroupID());
        TvinciEpgBL epgBL = new TvinciEpgBL(nParentGroupID);  //assuming this is a Kaltura user - the TVM does not support editing of yes Epg      
        EpgCB epg = epgBL.GetEpgCB(ulong.Parse(t.ToString()));       
        if (epg == null)
            epg = new EpgCB();      

        DataRecordShortTextField dr_name = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_name.Initialize("Name", "adm_table_header_nbg", "FormInput", "NAME", epg.Name, true);          
        theRecord.AddRecord(dr_name);

        DataRecordDateTimeField dr_start_date = new DataRecordDateTimeField(true);
        dr_start_date.Initialize("Start Date/Time", "adm_table_header_nbg", "FormInput", "START_DATE", epg.StartDate.ToString("dd/MM/yyyy HH:mm:ss"), true);        
        theRecord.AddRecord(dr_start_date);

        DataRecordDateTimeField dr_end_date = new DataRecordDateTimeField(true);
        dr_end_date.Initialize("End Date/Time", "adm_table_header_nbg", "FormInput", "END_DATE", epg.EndDate.ToString("dd/MM/yyyy HH:mm:ss"), true);        
        theRecord.AddRecord(dr_end_date);

        if (!string.IsNullOrEmpty(epg.EpgIdentifier))
        {            
            DataRecordOnePicBrowserField dr_pic = new DataRecordOnePicBrowserField(string.Empty, epg.EpgIdentifier, epg.ChannelID);
            dr_pic.Initialize("Thumb", "adm_table_header_nbg", "FormInput", "PIC_ID", false);
            dr_pic.SetValue(epg.PicID.ToString());
            theRecord.AddRecord(dr_pic);
        }

        DataRecordLongTextField dr_bio = new DataRecordLongTextField("ltr", true, 60, 5);
        dr_bio.Initialize("Description", "adm_table_header_nbg", "FormInput", "DESCRIPTION", epg.Description, false);           
        theRecord.AddRecord(dr_bio);

        DataRecordShortTextField dr_d = new DataRecordShortTextField("ltr", true, 60, 128);
        dr_d.Initialize("EPG Identifier", "adm_table_header_nbg", "FormInput", "EPG_IDENTIFIER", epg.EpgIdentifier, true);           
        theRecord.AddRecord(dr_d);                      

        DataRecordOneVideoBrowserField dr_media = new DataRecordOneVideoBrowserField("media", "media_tags", "media_id");     
        dr_media.Initialize("Related Media", "adm_table_header_nbg", "FormInput", "MEDIA_ID", false);
        dr_media.SetValue(epg.ExtraData.MediaID.ToString());       
        theRecord.AddRecord(dr_media);

        AddMetasFields(ref theRecord, epg);
        AddTagsFields(ref theRecord, epg);        

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_epg_channel_id = new DataRecordShortIntField(false, 9, 9);
        dr_epg_channel_id.Initialize("Epg Channel ID", "adm_table_header_nbg", "FormInput", "epg_channel_id", false);
        dr_epg_channel_id.SetValue(Session["epg_channel_id"].ToString());
        theRecord.AddRecord(dr_epg_channel_id);
       
        string sTableNew = theRecord.GetTableHTMLCB("adm_epg_channels_schedule_new.aspx?submited=1", false, epg);           
       
        return sTableNew;
    }
}
