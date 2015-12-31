using System;
using System.Data;
using System.Web.UI;
using TVinciShared;

public partial class adm_pic_popup_uploader : System.Web.UI.Page
{
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

        Initialize();               
    }

    private void Initialize()
    {
        // Set ratios
        PopulateRatioList();
    }

    private void PopulateRatioList()
    {
        rdbRatio.DataValueField = "id";
        rdbRatio.DataTextField = "txt";
        rdbRatio.DataSource = GetRatioData();
        rdbRatio.DataBind();
        // This will select the radio with value 1
        rdbRatio.SelectedIndex = 0;
    }

    private DataView GetRatioData()
    {
        DataView data = new DataView();
        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select lur.ratio as 'txt', g.ratio_id as 'id' from groups g, lu_pics_ratios lur where g.ratio_id = lur.id and g.id = " + LoginManager.GetLoginGroupID().ToString() + " UNION " + "select lur.ratio as 'txt', gr.ratio_id as 'id' from group_ratios gr, lu_pics_ratios lur where gr.ratio_id = lur.id and gr.status = 1 and gr.group_id = " + LoginManager.GetLoginGroupID().ToString();

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

        //Get MediaId
        string media = string.Empty;
        if (Session["media_id"] != null && !string.IsNullOrEmpty(Session["media_id"].ToString()))
        {
            media = Session["media_id"].ToString();
            if (media == "0")
            {
                return;
            }
        }

        int mediaId = 0;
        int.TryParse(media, out mediaId);

        // setMediaThumb only if the ratio is the group default ratio
        bool setMediaThumb = ratioId == TvinciImporter.ImporterImpl.GetGroupDefaultRatio(groupID);

        int picId = TvinciImporter.ImporterImpl.DownloadPicToImageServer(picLink, name, groupID, mediaId, "eng", "THUMBNAIL", setMediaThumb, ratioId);

        if (setMediaThumb)
        {
            //update media with new Pic
            Session["Pic_Image_Url"] = PageUtils.GetPicImageUrlByRatio(picId, 90, 65);
            //Session is saved in order updating Media table at media_new.aspx
            Session[string.Format("Media_{0}_Pic_Id", mediaId)] = picId;
            ClientScript.RegisterStartupScript(typeof(Page), "close", "<script language=javascript>window.opener.ChangePic('9_val'," + picId + ");self.close();</script>");
        }
    }

    protected void btnCancel_Click(object sender, EventArgs e)
    {
        ClientScript.RegisterStartupScript(typeof(Page), "closePage", "window.close();", true);
    }
}
