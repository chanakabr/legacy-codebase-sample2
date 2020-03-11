using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using TVinciShared;

public partial class admin_tree_player : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
    }

    protected Int32 GetMediaFileID(Int32 nGroupID)
    {
        Int32 nRet = 0;
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select top 1 mf.id from media_files mf,media m where mf.is_active=1 and mf.status=1 and m.is_active=1 and m.status=1 and mf.media_id=m.id and mf.MEDIA_TYPE_ID<>0 and mf.MEDIA_QUALITY_ID<>0 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", nGroupID);
        selectQuery += " order by mf.media_id desc";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        return nRet;
    }

    protected void GetFileDetails(Int32 nGroupID, ref string sFileFormat, ref string sFileQuality)
    {
        Int32 nMediaFileID = GetMediaFileID(nGroupID);
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select lmq.description as lmq_desc,lmt.description as lmt_desc from lu_media_quality lmq,lu_media_types lmt,media_files mf where lmt.id=mf.MEDIA_TYPE_ID and lmq.id=mf.MEDIA_QUALITY_ID  and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.id", "=", nMediaFileID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                sFileFormat = selectQuery.Table("query").DefaultView[0].Row["lmt_desc"].ToString();
                sFileQuality = selectQuery.Table("query").DefaultView[0].Row["lmq_desc"].ToString();
            }
        }
        selectQuery.Finish();
        selectQuery = null;
    }

    protected void GetPlayerDetails(Int32 nGroupID , ref string sUN, ref string sPass)
    {
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select gp.USERNAME,gp.PASSWORD from groups_passwords gp where gp.status=1 and gp.is_active=1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("gp.group_id", "=", nGroupID);
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                sUN = selectQuery.Table("query").DefaultView[0].Row["USERNAME"].ToString();
                sPass = selectQuery.Table("query").DefaultView[0].Row["PASSWORD"].ToString();
            }
        }
        selectQuery.Finish();
        selectQuery = null;
    }

    public void GetFlashVars()
    {
        Int32 nGroupID = LoginManager.GetLoginGroupID();
        string sFileFormat = "";
        string sFileQuality = "HIGH";
        string sPlayerUN = "";
        string sPlayerPass = "";

        GetPlayerDetails(nGroupID, ref sPlayerUN, ref sPlayerPass);
        GetFileDetails(nGroupID, ref sFileFormat, ref sFileQuality);
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select top 1 gp.USERNAME,gp.PASSWORD,lmq.description as lmq_desc,lmt.description as lmt_desc from groups_passwords gp,lu_media_quality lmq,media m,lu_media_types lmt,media_files mf where m.id=mf.media_id and m.status=1 and m.is_active=1 and mf.is_active=1 and lmt.id=mf.MEDIA_TYPE_ID and lmq.id=mf.MEDIA_QUALITY_ID and gp.group_id=mf.group_id and gp.status=1 and gp.is_active=1 and lmt.id<>0 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.group_id", "=", nGroupID);
        selectQuery += " order by lmt.id,mf.id desc";
        if (selectQuery.Execute("query", true) != null)
        {
            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                sPlayerUN = selectQuery.Table("query").DefaultView[0].Row["USERNAME"].ToString();
                sPlayerPass = selectQuery.Table("query").DefaultView[0].Row["PASSWORD"].ToString();
                sFileFormat = selectQuery.Table("query").DefaultView[0].Row["lmt_desc"].ToString();
                sFileQuality = selectQuery.Table("query").DefaultView[0].Row["lmq_desc"].ToString();
            }
        }
        selectQuery.Finish();
        selectQuery = null;
        string sStartingCategoryID = "";
        string sStartingChannelID = "";
        if (Session["parent_category_id"] != null)
            sStartingCategoryID = Session["parent_category_id"].ToString();
        if (Session["channel_id"] != null)
            sStartingChannelID = Session["channel_id"].ToString();

        //string sRet = "auto_play=true&file_format=" + sFileFormat + "&category_id=" + sCategoryID + "&pic_size1=full&";
        string sRet = "auto_play=true&file_format=" + sFileFormat + "&pic_size1=full&";
        sRet += "file_quality=" + sFileQuality;
        sRet += "&with_channels=true&no_cache=1";
        if (sStartingChannelID != "")
            sRet += "&starting_channel_id=" + sStartingChannelID;
        if (sStartingCategoryID != "")
            sRet += "&starting_category_id=" + sStartingCategoryID;
        sRet += "&player_un=" + sPlayerUN + "&player_pass=" + sPlayerPass;
        Response.Write(sRet);
    }
}
