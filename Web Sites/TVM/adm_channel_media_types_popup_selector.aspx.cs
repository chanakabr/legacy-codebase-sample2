using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DAL;
using TVinciShared;
using TvinciImporter;
using System.Data;

public partial class adm_channel_media_types_popup_selector : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;
        if (!IsPostBack)
        {   
            Session["channel_id"] = 0;
            if (Request.QueryString["channel_id"] != null && Request.QueryString["channel_id"].ToString() != "0" && Request.QueryString["channel_id"].ToString() != "")
            {
                Session["channel_id"] = int.Parse(Request.QueryString["channel_id"].ToString());
            }         

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
}