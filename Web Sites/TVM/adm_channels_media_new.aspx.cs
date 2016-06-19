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

public partial class adm_channels_media_new : System.Web.UI.Page
{
    protected string m_sMenu;
    protected string m_sSubMenu;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsPagePermitted("adm_channels.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        else if (LoginManager.IsActionPermittedOnPage("adm_channels.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.html");

        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        Int32 nMenuID = 0;
        if (!IsPostBack)
        {
            m_sMenu = TVinciShared.Menu.GetMainMenu(6, true, ref nMenuID);
            m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 2, true);
            if (Request.QueryString["submited"] != null && Request.QueryString["submited"].ToString() == "1")
            {   
                // check if media_i already exsits in channel 
                bool res = ValidateUnique();
                if (res)
                {
                    Int32 nId = DBManipulator.DoTheWork();
                    if (nId > 0)
                    {
                        object oChannelId = ODBCWrapper.Utils.GetTableSingleVal("channels_media", "CHANNEL_ID", nId);//get channel+media_id 
                        int nChannelId;
                        if (oChannelId != null && oChannelId != DBNull.Value)
                        {
                            bool result;
                            nChannelId = int.Parse(oChannelId.ToString());

                            nChannelId = int.Parse(oChannelId.ToString());
                            result = ImporterImpl.UpdateChannelIndex(LoginManager.GetLoginGroupID(), new List<int>() { nChannelId }, ApiObjects.eAction.Update);
                        }
                    }
                }
                else
                {
                    Session["error_msg"] = "media id is already related to channel";
                }
            }

            if (Session["channel_id"] == null || Session["channel_id"].ToString() == "" || Session["channel_id"].ToString() == "0")
            {
                LoginManager.LogoutFromSite("login.html");
                return;
            }

            if (Request.QueryString["channel_media_id"] != null && Request.QueryString["channel_media_id"].ToString() != "")
            {
                Session["channel_media_id"] = int.Parse(Request.QueryString["channel_media_id"].ToString());
                Int32 nOwnerGroupID = int.Parse(PageUtils.GetTableSingleVal("channels", "group_id", int.Parse(Session["channel_id"].ToString())).ToString());
                Int32 nLogedInGroupID = LoginManager.GetLoginGroupID();
                if (nLogedInGroupID != nOwnerGroupID && PageUtils.IsTvinciUser() == false)
                {
                    LoginManager.LogoutFromSite("login.html");
                    return;
                }
            }
            else
                Session["channel_media_id"] = "0";
        }
    }

    protected bool ValidateUnique()
    {
        bool retVal = true;
        int channelMediaId = 0;
        int media_id = 0;
        System.Collections.Specialized.NameValueCollection coll = HttpContext.Current.Request.Form;
        if (coll["0_val"] != null && !string.IsNullOrEmpty(coll["0_val"].ToString()))
        {
            media_id = int.Parse(coll["0_val"].ToString());
        }
        if (coll["3_val"] != null && !string.IsNullOrEmpty(coll["3_val"].ToString()))
        {
            channelMediaId = int.Parse(coll["3_val"].ToString());
        }
        if (media_id > 0 && channelMediaId > 0)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from channels_media where status = 1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CHANNEL_ID", "=", channelMediaId);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", media_id);

            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    retVal = false;
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        return retVal;
    }
    public void GetHeader()
    {
        if (Session["channel_media_id"] != null && Session["channel_media_id"].ToString() != "" && int.Parse(Session["channel_media_id"].ToString()) != 0)
        {
            Int32 nMediaID = int.Parse(PageUtils.GetTableSingleVal("channels_media", "media_id", int.Parse(Session["channel_media_id"].ToString())).ToString());
            Response.Write(PageUtils.GetPreHeader() + ":" + PageUtils.GetTableSingleVal("channels", "NAME", int.Parse(Session["channel_id"].ToString())).ToString() + ": " + PageUtils.GetTableSingleVal("media", "NAME", nMediaID).ToString() + " - Edit");
        }
        else
            Response.Write(PageUtils.GetPreHeader() + ":" + PageUtils.GetTableSingleVal("channels", "NAME", int.Parse(Session["channel_id"].ToString())).ToString() + ": New Media");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
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
        object channelMediaId = null;

        if (Session["channel_media_id"] != null && Session["channel_media_id"].ToString() != "" && int.Parse(Session["channel_media_id"].ToString()) != 0)
        {
            channelMediaId = Session["channel_media_id"];
        }

        string sBack = "adm_channels_media.aspx?channel_id=" + Session["channel_id"].ToString();
        DBRecordWebEditor theRecord = new DBRecordWebEditor("channels_media", "adm_table_pager", sBack, "", "ID", channelMediaId, sBack, "channel_id");

        DataRecordOneVideoBrowserField dr_media = new DataRecordOneVideoBrowserField("media", "media_tags", "media_id");
        dr_media.Initialize("The Media", "adm_table_header_nbg", "FormInput", "MEDIA_ID", true);
        theRecord.AddRecord(dr_media);

        DataRecordShortIntField dr_order_num = new DataRecordShortIntField(true, 3, 3);
        dr_order_num.Initialize("Order number", "adm_table_header_nbg", "FormInput", "ORDER_NUM", false);
        theRecord.AddRecord(dr_order_num);

        DataRecordShortIntField dr_groups = new DataRecordShortIntField(false, 9, 9);
        dr_groups.Initialize("Group", "adm_table_header_nbg", "FormInput", "GROUP_ID", false);
        dr_groups.SetValue(LoginManager.GetLoginGroupID().ToString());
        theRecord.AddRecord(dr_groups);

        DataRecordShortIntField dr_channel_id = new DataRecordShortIntField(false, 9, 9);
        dr_channel_id.Initialize("Media ID", "adm_table_header_nbg", "FormInput", "channel_id", false);
        dr_channel_id.SetValue(Session["channel_id"].ToString());
        theRecord.AddRecord(dr_channel_id);

        string sTable = theRecord.GetTableHTML("adm_channels_media_new.aspx?submited=1");
        return sTable;
    }
}
