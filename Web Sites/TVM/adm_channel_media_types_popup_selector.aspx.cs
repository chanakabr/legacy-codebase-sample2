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

            Session["include_epg"] = false;

            if (Request.QueryString["include_epg"] != null && Request.QueryString["include_epg"].ToString().ToLower() == "true")
            {
                Session["include_epg"] = true;
            }
        }
    }

    public string changeItemStatus(string sID, string sAction)
    {
        Int32 groupID = LoginManager.GetLoginGroupID();
        Int32 status = 0;
        int assetTypeID = int.Parse(sID);
        int channelID = 0;
        bool includeEpg = false;

        if (Session["channel_id"] != null && Session["channel_id"].ToString() != "" && int.Parse(Session["channel_id"].ToString()) != 0)
            channelID = int.Parse(Session["channel_id"].ToString());

        if (channelID != 0)
        {
            Int32 channelAssetTypeID = GetChannelAssetType(assetTypeID, channelID, groupID, ref status);

            if (channelAssetTypeID != 0)
            {
                if (status == 0)
                    UpdateChannelAssetType(channelAssetTypeID, 1, groupID, channelID);
                else
                    UpdateChannelAssetType(channelAssetTypeID, 0, groupID, channelID);
            }
            else
            {
                InsertChannelAssetType(assetTypeID, channelID, groupID);
            }
        }
        else
        {
            // save asset type id values to associate with channel (after get channelId)
            List<int> assetTypeList = new List<int>();
            if (Session["asset_type_ids"] != null && Session["asset_type_ids"] is List<int>)
            {
                assetTypeList = Session["asset_type_ids"] as List<int>;
            }
            assetTypeList.Add(assetTypeID);
            Session["asset_type_ids"] = assetTypeList;

        }

        return "";
    }

    private void InsertChannelAssetType(int assetTypeID, int channelID, int groupID)
    {
        bool inserted = TvmDAL.Insert_ChannelAssetType(groupID, channelID, new List<int>() { assetTypeID });
        if (inserted)
        {
            bool result = ImporterImpl.UpdateChannelIndex(groupID, new List<int>() { channelID }, ApiObjects.eAction.Update);
        }
    }

    private void UpdateChannelAssetType(int channelAssetTypeID, int status, int groupID, int channelID)
    {
        bool updated = TvmDAL.UpdateChannelAssetType(channelAssetTypeID, status, groupID, channelID);
        if (updated)
        {
            bool result = ImporterImpl.UpdateChannelIndex(groupID, new List<int>() { channelID }, ApiObjects.eAction.Update);
        }
    }

    public string initDualObj()
    {
        if (string.IsNullOrEmpty(Session["channel_id"].ToString()) || Session["channel_id"] == "0")
        {
            LoginManager.LogoutFromSite("index.html");
            return "";
        }

        Dictionary<string, object> dualList = new Dictionary<string, object>();
        dualList.Add("FirstListTitle", "Asset Types included in Channels");
        dualList.Add("SecondListTitle", "Available Asset Types");

        int channelID = Convert.ToInt32(Session["channel_id"]);

        bool includeEpg = false;

        if (Session["include_epg"] != null && Convert.ToBoolean(Session["include_epg"]))
        {
            includeEpg = true;
        }

        object[] resultData = null;

        BuildAssetTypeObjectArray(channelID, LoginManager.GetLoginGroupID(), ref resultData, includeEpg);

        dualList.Add("Data", resultData);
        dualList.Add("pageName", "adm_channel_media_types_popup_selector.aspx");
        dualList.Add("withCalendar", false);

        return dualList.ToJSON();
    }

    private void BuildAssetTypeObjectArray(int channelID, int groupID, ref object[] resultData, bool includeEpg)
    {
        DataSet ds = null;

        if (includeEpg)
        {
            ds = TvmDAL.Get_ChannelAssetTypes(groupID, channelID);
        }
        else
        {
            ds = TvmDAL.Get_ChannelMediaTypes(groupID, channelID);
        }

        if (ds != null && ds.Tables != null && ds.Tables.Count == 2)
        {
            DataTable availableAssetTypes = ds.Tables[0];
            DataTable channelAssetTypes = ds.Tables[1];
            List<object> assetTypesForDuallist = new List<object>();

            // save asset type id values to associate with channel (after get channelId)
            List<int> sessionAssetTypeList = new List<int>();
            if (Session["asset_type_ids"] != null && Session["asset_type_ids"] is List<int>)
            {
                sessionAssetTypeList = Session["asset_type_ids"] as List<int>;
            }

            if (availableAssetTypes != null && availableAssetTypes.Rows != null && availableAssetTypes.Rows.Count > 0)
            {
                foreach (DataRow dr in availableAssetTypes.Rows)
                {
                    int id = ODBCWrapper.Utils.ExtractInteger(dr, "ID");

                    // asset typ will be in AVAILABLE list if it doesn't appear in session's saved asset types
                    // and vice versa: will be in CURRENT list if it does appear in it
                    bool inList = sessionAssetTypeList.Contains(id);

                    var data = new
                    {
                        ID = id.ToString(),
                        Title = ODBCWrapper.Utils.ExtractString(dr, "NAME"),
                        Description = ODBCWrapper.Utils.ExtractString(dr, "NAME"),
                        InList = inList
                    };
                    assetTypesForDuallist.Add(data);
                }
            }

            if (channelAssetTypes != null && channelAssetTypes.Rows != null && channelAssetTypes.Rows.Count > 0)
            {
                foreach (DataRow dr in channelAssetTypes.Rows)
                {
                    var data = new
                    {
                        ID = ODBCWrapper.Utils.ExtractString(dr, "ID"),
                        Title = ODBCWrapper.Utils.ExtractString(dr, "NAME"),
                        Description = ODBCWrapper.Utils.ExtractString(dr, "NAME"),
                        InList = true
                    };
                    assetTypesForDuallist.Add(data);
                }
            }
            resultData = new object[assetTypesForDuallist.Count];
            resultData = assetTypesForDuallist.ToArray();

        }
    }

    private Int32 GetChannelAssetType(Int32 assetTypeID, Int32 channelID, Int32 groupID, ref int status)
    {
        Int32 result = 0;
        DataTable dt = TvmDAL.GetChannelMediaType(groupID, channelID, assetTypeID);
        if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
        {
            DataRow dr = dt.Rows[0];
            result = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
            status = ODBCWrapper.Utils.GetIntSafeVal(dr, "STATUS");
        }
        return result;
    }
}