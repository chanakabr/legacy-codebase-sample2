using KLogMonitor;
using System;
using System.Data;
using System.Reflection;
using System.Web.UI;
using TVinciShared;

public partial class adm_pic_popup_uploader : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    static protected string m_sIDs = "";

    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted("adm_pics.aspx") == false)
            LoginManager.LogoutFromSite("login.html");
        if (LoginManager.IsActionPermittedOnPage("adm_pics.aspx", LoginManager.PAGE_PERMISION_TYPE.EDIT) == false)
            LoginManager.LogoutFromSite("login.aspx");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;

        if (!IsPostBack)
        {
            if (Request.QueryString["epgIdentifier"] != null && Request.QueryString["epgIdentifier"].ToString() != "")
            {
                Session["epgIdentifier"] = Request.QueryString["epgIdentifier"].ToString();
            }

            if (Request.QueryString["channelID"] != null && Request.QueryString["channelID"].ToString() != "")
            {
                Session["channelID"] = int.Parse(Request.QueryString["channelID"].ToString());
            }

            Initialize();

        }
    }

    private void Initialize()
    {
        // Set ratios
        PopulateRatioList();
    }

    private void PopulateRatioList()
    {
        int selectedIndex = 0;

        rdbRatio.DataValueField = "id";
        rdbRatio.DataTextField = "txt";
        rdbRatio.DataSource = GetRatioData(out selectedIndex);
        rdbRatio.DataBind();
        // This will select the radio with value 1
        rdbRatio.SelectedValue = selectedIndex.ToString();
    }

    private DataView GetRatioData(out int selectedIndex)
    {
        selectedIndex = 0;
        int groupId = LoginManager.GetLoginGroupID();
        DataView data = new DataView();
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

        MediaType picMediaType = GetPicMediaType();

        switch (picMediaType)
        {
            case MediaType.None:
                {
                    log.Error("Pic_popup_uploader - Confirm failed. Id");
                    return data;
                }
            case MediaType.Vod:
                {
                    selectQuery += "select lur.ratio as 'txt', g.ratio_id as 'id' from groups g, lu_pics_ratios lur where g.ratio_id = lur.id and g.id = " + groupId.ToString() + " UNION " +
                        "select lur.ratio as 'txt', gr.ratio_id as 'id' from group_ratios gr, lu_pics_ratios lur where gr.ratio_id = lur.id and gr.status = 1 and gr.group_id = " + groupId.ToString();

                    selectedIndex = ImageUtils.GetGroupDefaultRatio(groupId);
                }
                break;
            case MediaType.EpgProgram:
                {
                    selectQuery += "select lur.ratio as 'txt', g.ratio_id as 'id' from groups g, lu_pics_epg_ratios lur where g.ratio_id = lur.id and g.id = " + groupId.ToString() + " UNION " +
                        "select lur.ratio as 'txt', gr.ratio_id as 'id' from group_epg_ratios gr, lu_pics_epg_ratios lur where gr.ratio_id = lur.id and gr.group_id = " + groupId.ToString();
                    
                    selectedIndex = ImageUtils.GetGroupDefaultEpgRatio(groupId);
                }
                break;
            default:
                break;
        }

        if (selectQuery.Execute("query", true) != null && selectQuery.Table("query").DefaultView != null)
        {
            return selectQuery.Table("query").DefaultView;
        }

        return data;
    }

    protected void btnConfirm_Click(object sender, EventArgs e)
    {
        //lblStatus
        lblStatus.Text = "";
        lblStatus.Visible = false;

        int groupID = LoginManager.GetLoginGroupID();

        string name = txtName.Text.Trim();
        string picLink = txtPicLink.Text.Trim();
        string ratio = rdbRatio.SelectedValue;
        int ratioId = 0;

        int.TryParse(ratio, out ratioId);

        // name and picLink --> mandatory fields
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(picLink))
        {
            lblStatus.ForeColor = System.Drawing.Color.Red;
            lblStatus.Text = "Please fill required fields";
            lblStatus.Visible = true;
            return;
        }

        Uri uriResult;
        // check for valid Url
        bool result = Uri.TryCreate(picLink, UriKind.Absolute, out uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

        if (!result)
        {
            lblStatus.ForeColor = System.Drawing.Color.Red;
            lblStatus.Text = "Please fill valid pic link";
            lblStatus.Visible = true;
            return;
        }

        int id = 0;
        int picId = 0;
        MediaType picMediaType = GetPicMediaType();
        string mediaIdentifier = string.Format("MediaType_{0}_Id", picMediaType.ToString());

        if (Session[mediaIdentifier] == null || string.IsNullOrEmpty(Session[mediaIdentifier].ToString()) ||
          !int.TryParse(Session[mediaIdentifier].ToString(), out id))
        {
            log.Error("Pic_popup_uploader - Confirm failed.");
        }

        Session["MediaType"] = null;
        Session[mediaIdentifier] = null;


        if (id > 0)
        {
            // setMediaThumb only if the ratio is the group default ratio
            bool setMediaThumb = ratioId == ImageUtils.GetGroupDefaultRatio(groupID);

            switch (picMediaType)
            {
                case MediaType.None:
                    log.ErrorFormat("Pic_popup_uploader - Confirm failed. Id {0} ", id);
                    break;
                case MediaType.Vod:
                    {
                        // setMediaThumb only if the ratio is the group default ratio
                        setMediaThumb = ratioId == ImageUtils.GetGroupDefaultRatio(groupID);

                        picId = TvinciImporter.ImporterImpl.DownloadPicToImageServer(picLink, name, groupID, id, "eng", setMediaThumb, ratioId, false);

                        if (picId > 0)
                        {
                            if (setMediaThumb)
                            {
                                //update media with new Pic
                                Session["Pic_Image_Url"] = PageUtils.GetPicImageUrlByRatio(picId, 90, 65);
                                //Session is saved in order updating Media table at media_new.aspx
                                Session[string.Format("Media_{0}_Pic_Id", id)] = picId;
                                ClientScript.RegisterStartupScript(typeof(Page), "close", "<script language=javascript>window.opener.ChangePic('9_val'," + picId + ");self.close();</script>");
                            }
                        }
                    }
                    break;
                case MediaType.EpgProgram:
                    {
                        picId = TvinciImporter.ImporterImpl.DownloadEPGPicToImageServer(picLink, name, groupID, id, ratioId, false);

                        if (picId > 0)
                        {
                            //Session is saved in order updating Epg_Channel table at Epg_Channel_new.aspx
                            Session[string.Format("Epg_Channel_Schedule_{0}_Pic_Id", Session["epgIdentifier"].ToString())] = picId;
                            Session[string.Format("Epg_Pic_Id_{0}_Pic_Ratio", picId)] = ratioId;

                            // setMediaThumb only if the ratio is the group default ratio
                            setMediaThumb = ratioId == ImageUtils.GetGroupDefaultEpgRatio(groupID);

                            if (setMediaThumb)
                            {
                                //update media with new Pic
                                Session["Pic_Image_Url"] = PageUtils.GetEpgChannelsSchedulePicImageUrl(picId, 90, 65);
                                ClientScript.RegisterStartupScript(typeof(Page), "close", "<script language=javascript>window.opener.ChangePic('3_val'," + picId + ");self.close();</script>");
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        ClientScript.RegisterStartupScript(typeof(Page), "closePage", "window.close();", true);
    }

    private MediaType GetPicMediaType()
    {
        MediaType type = MediaType.None;

        string media = string.Empty;
        int mediaId = 0;
        string epgChannel = string.Empty;
        int epgChannelId = 0;

        if (Session["MediaType"] == null)
        {
            if (Request.QueryString["epgIdentifier"] != null && Request.QueryString["epgIdentifier"].ToString() != "")
            {
                epgChannel = Session["epg_channel_id"].ToString();
                int.TryParse(epgChannel, out epgChannelId);
                type = MediaType.EpgProgram;
                Session[string.Format("MediaType_{0}_Id", type)] = epgChannelId;
            }
            else
            {
                media = Session["media_id"].ToString();
                int.TryParse(media, out mediaId);
                type = MediaType.Vod;
                Session[string.Format("MediaType_{0}_Id", type)] = mediaId;
            }
        }
        else
        {
            var mediaType = Session["MediaType"];
            Enum.TryParse<MediaType>(Session["MediaType"].ToString(), out type);
        }

        Session["MediaType"] = type;
        return type;
    }

    protected void btnCancel_Click(object sender, EventArgs e)
    {
        MediaType picMediaType = GetPicMediaType();
        string mediaIdentifier = string.Format("MediaType_{0}_Id", picMediaType.ToString());

        Session["MediaType"] = null;
        Session[mediaIdentifier] = null;

        ClientScript.RegisterStartupScript(typeof(Page), "closePage", "window.close();", true);
    }

    private enum MediaType
    {
        None,
        Vod,
        EpgProgram
    }
}
