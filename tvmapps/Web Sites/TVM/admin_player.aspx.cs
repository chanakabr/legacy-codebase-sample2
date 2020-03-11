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

public partial class admin_player : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Request.QueryString["media_file_id"] != null)
            Session["media_file_id"] = Request.QueryString["media_file_id"];
        else
            Session["media_file_id"] = null;

        if (Request.QueryString["cdn_type"] != null)
            Session["cdn_type"] = Request.QueryString["cdn_type"];
        else
            Session["cdn_type"] = null;

        if (Request.QueryString["player_type"] != null)
            Session["player_type"] = Request.QueryString["player_type"];
        else
            Session["player_type"] = null;

        if (Request.QueryString["audio_url"] != null)
            Session["audio_url"] = Request.QueryString["audio_url"];
        else
            Session["audio_url"] = null;

        if (Request.QueryString["size"] != null)
            Session["size"] = Request.QueryString["size"];
        else
            Session["size"] = null;

        if (Request.QueryString["flv"] != null)
            Session["flv"] = Request.QueryString["flv"];
        else
            Session["flv"] = null;

        if (Request.QueryString["autoplay"] != null)
            Session["autoplay"] = Request.QueryString["autoplay"];
        else
            Session["autoplay"] = null;
    }

    public void GetPlayerType()
    {
        if (Session["player_type"] != null)
        {
            Response.Write(Session["player_type"].ToString().Trim().ToLower());
        }
    }
    public void GetPlayerWidth()
    {
        string sSize = "big";
        if (Session["size"] != null)
            sSize = Session["size"].ToString().Trim().ToLower();
        if (sSize == "big")
            Response.Write("100%");
        else
            Response.Write("100%");
    }

    public void GetPlayerHeight()
    {
        string sSize = "big";
        if (Session["size"] != null)
            sSize = Session["size"].ToString().Trim().ToLower();
        if (sSize == "big")
            Response.Write("100%");
        else
            Response.Write("100%");
    }

    public void GetNDSIframe()
    {
        string sRet = "";
        if (Session["file_format"] != null && Session["file_format"].ToString() == "gib")
            sRet = "<iframe id=\"NDSFrame\" width=\"0px;\" height=\"0px;\" src=\"NDSPlayer.htm\"></iframe>";
        Response.Write(sRet);
    }

    public void GetFlashVars()
    {
        string sRet = "";
        if (Session["player_type"] != null && Session["player_type"].ToString().Trim().ToLower() == "video")
        {
            if (Session["media_file_id"] != null)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select gp.USERNAME,gp.PASSWORD,mf.media_id,lmt.player_description,lmt.DESCRIPTION as t_d,lmq.DESCRIPTION as q_d from groups_passwords gp,lu_media_types lmt,lu_media_quality lmq,media_files mf where mf.MEDIA_TYPE_ID=lmt.id and mf.MEDIA_QUALITY_ID=lmq.id and gp.group_id=mf.group_id and gp.status=1 and gp.is_active=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("mf.id", "=", int.Parse(Session["media_file_id"].ToString()));
                selectQuery += " order by gp.id desc";
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        string sPlayerUN = selectQuery.Table("query").DefaultView[0].Row["USERNAME"].ToString();
                        string sPlayerPass = selectQuery.Table("query").DefaultView[0].Row["PASSWORD"].ToString();
                        string sFileFormat = selectQuery.Table("query").DefaultView[0].Row["t_d"].ToString();
                        string sFileFormatPD = selectQuery.Table("query").DefaultView[0].Row["player_description"].ToString();
                        string sFileQuality = selectQuery.Table("query").DefaultView[0].Row["q_d"].ToString();
                        string sMediaID = selectQuery.Table("query").DefaultView[0].Row["media_id"].ToString();
                        if (sFileFormat == "GIB")
                            sFileFormat = "gib";
                        Session["file_format"] = sFileFormat;
                        if (sFileFormatPD.Trim().ToUpper() != "PNG")
                            sRet += "pic_size1=full&pic_size2=full&";
                        sRet += "debug_protocols=true&server_base_url=http://vod.orange.co.il/&lang=&auto_play=false&Prod=1&auto_init=1&config_file=http://tvm.tvinci.com/flash/config_admin.xml&language_file=http://tvm.tvinci.com/flash/lucy_language.xml&skin_file=http://admin.tvinci.com/flash/gui_admin.swf&layout_file=http://tvm.tvinci.com/flash/lucy_layout7.xml";
                        //sRet += "debug_protocols=true&server_base_url=http://vod.orange.co.il/&lang=Hebrew&auto_play=false&Prod=1&auto_init=1&config_file=http://localhost:1890/TVM/flash/config_admin.xml&language_file=http://localhost:1890/TVM/flash/lucy_language.xml&skin_file=http://localhost:1890/TVM/flash/gui_admin.swf&layout_file=http://localhost:1890/TVM/flash/lucy_layout7.xml";
                        sRet += "&media_id=" + sMediaID + "&auto_play=false&object_id=" + sPlayerUN + "&object_key=" + sPlayerPass + "&file_format=" + sFileFormat + "&file_quality=" + sFileQuality + "&";
                        //sRet += "media_id=" + sMediaID + "&auto_play=false&player_un=" + sPlayerUN + "&player_pass=" + sPlayerPass + "&file_format=" + sFileFormat + "&file_quality=" + sFileQuality + "&"; ;
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            else
            {
                if (Session["flv"] != null)
                    sRet += "vid_url=" + Server.UrlEncode((Session["flv"].ToString())) + "&";
                if (Session["cdn_type"] != null)
                    sRet += "vid_cdn=" + Session["cdn_type"].ToString() + "&";
                if (Session["autoplay"] != null)
                    sRet += "vid_autoplay=" + Session["autoplay"].ToString() + "&";
            }
        }
        if (Session["player_type"] != null && Session["player_type"].ToString().Trim().ToLower() == "audio")
        {
            if (Session["audio_url"] != null)
            {
                string sURL = Server.UrlEncode(Session["audio_url"].ToString());
                sRet += "audio_url=" + sURL + "&";
            }
            if (Session["autoplay"] != null)
                sRet += "auto_play=" + Session["autoplay"].ToString();
        }
        sRet += "&no_cache=1&relative_server_path=false&server_environment_url=https://vod.orange.co.il/papi.aspx";
        Response.Write(sRet);
    }
}
