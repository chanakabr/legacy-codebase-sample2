using ApiObjects;
using KLogMonitor;
using System;
using System.Data;
using System.Reflection;
using System.Web.UI;
using TVinciShared;

public partial class adm_pic_popup_uploader : System.Web.UI.Page
{
    private enum PopUpContext
    {
        None,
        Vod,
        EpgProgram,
        Category,
        Channel,
        DefaultPic,
        LogoPic
    }

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
            // in case session wasn't nullify
            ClearSession();

            if (!string.IsNullOrEmpty(Request.QueryString["epgIdentifier"]))
            {
                Session["epgIdentifier"] = Request.QueryString["epgIdentifier"].ToString();
            }

            if (!string.IsNullOrEmpty(Request.QueryString["channelID"]))
            {
                Session["channelID"] = int.Parse(Request.QueryString["channelID"].ToString());
            }

            if (!string.IsNullOrEmpty(Request.QueryString["theID"]))
            {
                Session["FieldId"] = Request.QueryString["theID"].ToString();
            }

            Initialize();
        }
    }

    protected void btnConfirm_Click(object sender, EventArgs e)
    {
        //lblStatus
        lblStatus.Text = "";
        lblStatus.Visible = false;

        int groupID = LoginManager.GetLoginGroupID();

        string name = txtName.Text.Trim();
        string picLink = txtPicLink.Text.Trim();
        string ratio = ddlRatio.SelectedValue;
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

        // check for valid Url
        if (!IsImageUrlVaild(picLink))
        {
            lblStatus.ForeColor = System.Drawing.Color.Red;
            lblStatus.Text = "Please fill valid pic link";
            lblStatus.Visible = true;
            return;
        }

        int id = 0;
        int picId = 0;
        PopUpContext picMediaType = GetPopUpContext();
        string mediaIdentifier = string.Format("MediaType_{0}_Id", picMediaType.ToString());

        if (Session[mediaIdentifier] == null || string.IsNullOrEmpty(Session[mediaIdentifier].ToString()) ||
          !int.TryParse(Session[mediaIdentifier].ToString(), out id))
        {
            log.Error("Pic_popup_uploader - Confirm failed.");
        }

        Session["MediaType"] = null;
        Session[mediaIdentifier] = null;

        string openerFieldToUpdate = string.Empty;
        if (Session["FieldId"] != null && !string.IsNullOrEmpty(Session["FieldId"].ToString()))
        {
            openerFieldToUpdate = Session["FieldId"].ToString();
            Session["FieldId"] = null;
        }

        bool setMediaThumb = ratioId == ImageUtils.GetGroupDefaultRatio(groupID);

        if (id > 0)
        {
            switch (picMediaType)
            {
                case PopUpContext.None:
                    log.ErrorFormat("Pic_popup_uploader - Confirm failed. Id {0} ", id);
                    break;
                case PopUpContext.Vod:
                    {
                        picId = TvinciImporter.ImporterImpl.DownloadPicToImageServer(picLink, name, groupID, id, "eng", setMediaThumb, ratioId, eAssetImageType.Media, false, LoginManager.GetLoginID());
                        // setMediaThumb only if the ratio is the group default ratio
                        ChangePicAtOpenerPage(picId, setMediaThumb, openerFieldToUpdate);
                    }
                    break;
                case PopUpContext.Category:
                    {
                        picId = TvinciImporter.ImporterImpl.DownloadPicToImageServer(picLink, name, groupID, id, "eng", false, ratioId, eAssetImageType.Category, false, LoginManager.GetLoginID());

                        ChangePicAtOpenerPage(picId, setMediaThumb, openerFieldToUpdate);

                    }
                    break;
                case PopUpContext.Channel:
                    {
                        picId = TvinciImporter.ImporterImpl.DownloadPicToImageServer(picLink, name, groupID, id, "eng", false, ratioId, eAssetImageType.Channel, false, LoginManager.GetLoginID());

                        ChangePicAtOpenerPage(picId, setMediaThumb, openerFieldToUpdate);

                    }
                    break;
                case PopUpContext.DefaultPic:
                    {
                        picId = TvinciImporter.ImporterImpl.DownloadPicToImageServer(picLink, name, groupID, id, "eng", false, ratioId, eAssetImageType.DefaultPic, false, LoginManager.GetLoginID());

                        ChangePicAtOpenerPage(picId, setMediaThumb, openerFieldToUpdate);

                    }
                    break;
                case PopUpContext.LogoPic:
                    {
                        picId = TvinciImporter.ImporterImpl.DownloadPicToImageServer(picLink, name, groupID, id, "eng", false, 0, eAssetImageType.LogoPic, false, LoginManager.GetLoginID());

                        ChangePicAtOpenerPage(picId, true, openerFieldToUpdate);

                    }
                    break;
                case PopUpContext.EpgProgram:
                    {

                        picId = TvinciImporter.ImporterImpl.DownloadEPGPicToImageServer(picLink, name, groupID, id, ratioId, false, LoginManager.GetLoginID(),Session["epgIdentifier"].ToString());

                        if (picId > 0)
                        {
                            //Session is saved in order updating Epg_Channel table at Epg_Channel_new.aspx
                            Session[string.Format("Epg_Channel_Schedule_{0}_Pic_Id", Session["epgIdentifier"].ToString())] = picId;
                            Session[string.Format("Epg_Pic_Id_{0}_Pic_Ratio", picId)] = ratioId;

                            // if the ratio is the group default ratio
                            if (ratioId == ImageUtils.GetGroupDefaultEpgRatio(groupID))
                            {
                                //update media with new Pic
                                Session["Pic_Image_Url"] = PageUtils.GetEpgPicImageUrl(picId, 90, 65);
                                ClientScript.RegisterStartupScript(typeof(Page), "close", "<script language=javascript>window.opener.document.getElementsByName('" + openerFieldToUpdate + "')[0].value = " + picId
                                    + ";window.opener.ChangePic('" + openerFieldToUpdate + "'," + picId + ");</script>");
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        Session["epgIdentifierForScheduleNew"] = Session["epgIdentifier"];
        ClearSession();
        ClientScript.RegisterStartupScript(typeof(Page), "closePage", "window.close();", true);
    }

    protected void ddlRatio_SelectedIndexChanged(object sender, EventArgs e)
    {
        int id = 0;
        PopUpContext type = GetPopUpContext();

        string mediaIdentifier = string.Format("MediaType_{0}_Id", type.ToString());

        if (Session[mediaIdentifier] == null || string.IsNullOrEmpty(Session[mediaIdentifier].ToString()) ||
          !int.TryParse(Session[mediaIdentifier].ToString(), out id))
        {
            log.Error("Pic_popup_uploader - Confirm failed.");
        }

        imgPicRatio.ImageUrl = string.Empty;

        //set imgPicRatio according to selected ratio
        switch (type)
        {
            case PopUpContext.Vod:
                imgPicRatio.ImageUrl = PageUtils.GetPicImageUrlByRatio(id, eAssetImageType.Media, int.Parse(ddlRatio.SelectedValue), 90, 65);
                break;
            case PopUpContext.EpgProgram:
                {
                    if (Session["epgIdentifier"] != null && !string.IsNullOrEmpty(Session["epgIdentifier"].ToString()))
                    {
                        imgPicRatio.ImageUrl = PageUtils.GetEpgPicImageUrl(Session["epgIdentifier"].ToString(), id, int.Parse(ddlRatio.SelectedValue), 90, 65);
                    }
                    break;
                }
            case PopUpContext.Category:
                imgPicRatio.ImageUrl = PageUtils.GetPicImageUrlByRatio(id, eAssetImageType.Category, int.Parse(ddlRatio.SelectedValue), 90, 65);
                break;
            case PopUpContext.Channel:
                imgPicRatio.ImageUrl = PageUtils.GetPicImageUrlByRatio(id, eAssetImageType.Channel, int.Parse(ddlRatio.SelectedValue), 90, 65);
                break;
            case PopUpContext.DefaultPic:
                imgPicRatio.ImageUrl = PageUtils.GetPicImageUrlByRatio(id, eAssetImageType.DefaultPic, int.Parse(ddlRatio.SelectedValue), 90, 65);
                break;
            case PopUpContext.LogoPic:
                imgPicRatio.ImageUrl = PageUtils.GetPicImageUrlByRatio(id, eAssetImageType.LogoPic, 0, 90, 65);
                break;
            default:
                break;
        }

        imgPicRatio.Visible = !string.IsNullOrEmpty(imgPicRatio.ImageUrl);
    }

    protected void btnCancel_Click(object sender, EventArgs e)
    {
        try
        {
            PopUpContext picMediaType = GetPopUpContext();
            string mediaIdentifier = string.Format("MediaType_{0}_Id", picMediaType.ToString());
            Session[mediaIdentifier] = null;
        }
        catch
        {
        }

        ClearSession();
        ClientScript.RegisterStartupScript(typeof(Page), "closePage", "window.close();", true);
    }

    private void Initialize()
    {
        // Get the pic popup Context ( media, epg, channel, category)
        PopUpContext popUpContext = GetPopUpContext();

        if (popUpContext == PopUpContext.LogoPic)
        {
            lblPicRatio.Visible = ddlRatio.Visible = false;
        }
        else
            PopulateRatioList(popUpContext);
    }

    private void PopulateRatioList(PopUpContext picMediaType)
    {
        int selectedIndex = 0;
        ddlRatio.DataValueField = "id";
        ddlRatio.DataTextField = "txt";
        ddlRatio.DataSource = GetRatioData(picMediaType, out selectedIndex);
        ddlRatio.DataBind();
        ddlRatio.SelectedValue = selectedIndex.ToString();
        ddlRatio_SelectedIndexChanged(null, null);
    }

    private object GetRatioData(PopUpContext picMediaType, out int selectedIndex)
    {
        selectedIndex = 0;
        int groupId = LoginManager.GetLoginGroupID();
        DataView data = new DataView();
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

        switch (picMediaType)
        {
            case PopUpContext.None:
                {
                    log.Error("Pic_popup_uploader - Confirm failed. Id");
                    return data;
                }
            case PopUpContext.Vod:
            case PopUpContext.Channel:
            case PopUpContext.Category:
            case PopUpContext.DefaultPic:
                {
                    selectQuery += "select lur.ratio as 'txt', g.ratio_id as 'id' from groups g, lu_pics_ratios lur where g.ratio_id = lur.id and g.id = " + groupId.ToString() + " UNION " +
                        "select lur.ratio as 'txt', gr.ratio_id as 'id' from group_ratios gr, lu_pics_ratios lur where gr.ratio_id = lur.id and gr.status = 1 and gr.group_id = " + groupId.ToString();

                    selectedIndex = ImageUtils.GetGroupDefaultRatio(groupId);
                }
                break;

            case PopUpContext.EpgProgram:
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

    private void ChangePicAtOpenerPage(int picId, bool iSChangePicNeeded, string openerFieldToUpdate)
    {
        if (picId > 0 && iSChangePicNeeded)
        {
            //update media with new Pic
            Session["Pic_Image_Url"] = PageUtils.GetPicImageUrlByRatio(picId, 90, 65);
            ClientScript.RegisterStartupScript(typeof(Page), "close", "<script language=javascript>window.opener.document.getElementsByName('" + openerFieldToUpdate + "')[0].value = " + picId
                + ";window.opener.ChangePic('" + openerFieldToUpdate + "'," + picId + ");</script>");
        }
    }

    private PopUpContext GetPopUpContext()
    {
        PopUpContext type = PopUpContext.None;

        if (Session["MediaType"] == null)
        {
            if (Request.QueryString["epgIdentifier"] != null && Request.QueryString["epgIdentifier"].ToString() != "")
            {
                type = PopUpContext.EpgProgram;
                SetMediaTypeSession("epg_channel_id", type);
            }
            else if (!string.IsNullOrEmpty(Request.QueryString["lastPage"]) && Request.QueryString["lastPage"].ToString() == "category")
            {
                type = PopUpContext.Category;
                SetMediaTypeSession("category_id", type);
            }
            else if (!string.IsNullOrEmpty(Request.QueryString["lastPage"]) && Request.QueryString["lastPage"].ToString() == "channel")
            {
                type = PopUpContext.Channel;
                SetMediaTypeSession("channel_id", type);
            }
            else if (!string.IsNullOrEmpty(Request.QueryString["lastPage"]) && Request.QueryString["lastPage"].ToString() == "myGroup")
            {
                type = PopUpContext.DefaultPic;
                SetMediaTypeSession("defaultPic", type);
            }
            else if (!string.IsNullOrEmpty(Request.QueryString["lastPage"]) && Request.QueryString["lastPage"].ToString() == "myGroupLogo")
            {
                type = PopUpContext.LogoPic;
                SetMediaTypeSession("logoPic", type);
            }
            else if (!string.IsNullOrEmpty(Request.QueryString["lastPage"]) && Request.QueryString["lastPage"].ToString() == "media")
            {
                type = PopUpContext.Vod;
                SetMediaTypeSession("media_id", type);
            }
        }
        else
        {
            var mediaType = Session["MediaType"];
            Enum.TryParse<PopUpContext>(Session["MediaType"].ToString(), out type);
        }

        Session["MediaType"] = type;
        return type;
    }

    private void SetMediaTypeSession(string contextIdName, PopUpContext type)
    {
        int sourceId = 0;

        switch (type)
        {
            case PopUpContext.Vod:
            case PopUpContext.EpgProgram:
            case PopUpContext.Category:
            case PopUpContext.Channel:
                {
                    string source = string.Empty;
                    source = Session[contextIdName].ToString();
                    int.TryParse(source, out sourceId);
                }
                break;
            case PopUpContext.DefaultPic:
            case PopUpContext.LogoPic:
                sourceId = LoginManager.GetLoginGroupID();
                break;
            default:
                break;
        }

        Session[string.Format("MediaType_{0}_Id", type)] = sourceId;
    }

    private void ClearSession()
    {
        // in case session wasn't nullify
        Session["MediaType"] = null;
        Session["epgIdentifier"] = null;
        Session["FieldId"] = null;
    }

    private bool IsImageUrlVaild(string picLink)
    {
        bool isImageUrlVaild = false;
        //check if thumb Url exist
        string checkImageUrl = WS_Utils.GetTcmConfigValue("CheckImageUrl");
        if (!string.IsNullOrEmpty(checkImageUrl) && checkImageUrl.ToLower().Equals("true"))
        {
            if (!ImageUtils.IsUrlExists(picLink))
            {
                log.ErrorFormat("DownloadPicToImageServer thumb Uri not valid: {0} ", picLink);
                isImageUrlVaild = false;
            }
            else
            {
                isImageUrlVaild = true;
            }
        }

        return isImageUrlVaild;
    }
}
