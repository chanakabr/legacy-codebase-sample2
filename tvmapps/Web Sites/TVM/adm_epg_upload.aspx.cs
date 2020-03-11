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
using System.IO;
using System.Threading;
using System.Collections.Generic;
using TVinciShared;

public partial class adm_epg_upload : System.Web.UI.Page
{


   
    
    protected string m_sMenu;
    protected string m_sSubMenu;

    private const string GROUP_CONFIG_KEY_PREFIX = "epg_ingest_groupID_";


    protected void Page_Load(object sender, EventArgs e)
    {
        if (LoginManager.CheckLogin() == false)
            Response.Redirect("login.html");
        if (LoginManager.IsPagePermitted() == false)
            LoginManager.LogoutFromSite("login.html");
        if (AMS.Web.RemoteScripting.InvokeMethod(this))
            return;

        Int32 nMenuID = 0;
        m_sMenu = TVinciShared.Menu.GetMainMenu(7, true, ref nMenuID);
        m_sSubMenu = TVinciShared.Menu.GetSubMenu(nMenuID, 1, false);
    }

    public void GetHeader()
    {
        Response.Write(PageUtils.GetPreHeader() + ": Epg Ingest - Upload Xml File");
    }

    protected void GetMainMenu()
    {
        Response.Write(m_sMenu);
    }

    protected void GetSubMenu()
    {
        Response.Write(m_sSubMenu);
    }


    protected void UploadEpgFile(object sender, EventArgs e)
    {
        if (FileUpload1.HasFile == true)
        {
            string fileName = FileUpload1.FileName;
            try
            {
                if (Path.HasExtension(fileName) && (Path.GetExtension(fileName) == ".xml"))
                {
                    int nGroupID = LoginManager.GetLoginGroupID();

                    string folderPath = Server.MapPath(string.Empty);
                    folderPath = System.IO.Path.Combine(folderPath, "epg_upload");
                    folderPath = System.IO.Path.Combine(folderPath, nGroupID.ToString());
                    if (Directory.Exists(folderPath) == false)
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    string filePath = folderPath + @"/" + fileName;
                    FileUpload1.SaveAs(filePath);

                    UploadXmlFileToFtp(nGroupID, filePath);


                    LblUploadStatus.ForeColor = System.Drawing.Color.Blue;
                    LblUploadStatus.Text = "Upload status: File uploaded to Ftp!";
                    LblUploadStatus.Visible = true;                 
                }
                else
                {
                    LblUploadStatus.ForeColor = System.Drawing.Color.Red;
                    LblUploadStatus.Text = "File must be Xml type !";
                    LblUploadStatus.Visible = true;
                }
            }
            catch (Exception ex)
            {
                LblUploadStatus.ForeColor = System.Drawing.Color.Red;
                LblUploadStatus.Text = "Upload status: The file could not be uploaded. The following error occured: " + ex.Message;
                LblUploadStatus.Visible = true;

            }
        }
        else
        {
            LblUploadStatus.ForeColor = System.Drawing.Color.Red;
            LblUploadStatus.Text = "No File To Upload";
            LblUploadStatus.Visible = true;
        }
    }


    protected void UploadXmlFileToFtp(int groupID, string filePath)
    {
        string ftpAddress;
        string ftpUser;
        string ftpPassword;
        GetFtpConnectionDetails(groupID, out ftpAddress, out ftpUser, out ftpPassword);
        if (ftpAddress.Length > 0)
        {
            //FTPUploadQueue.FTPUploader ftpUploader = new FTPUploadQueue.FTPUploader(filePath, ftpAddress, ftpUser, ftpPassword);
            //ftpUploader.Upload();
        }
        else
        {
            throw new Exception("No ftp Address declared for this group!");
        }
    }


    protected void GetFtpConnectionDetails(int groupID, out string ftpAddress, out string ftpUser, out string ftpPassword)
    {
        //ftpAddress = "tvinci.cdnetworks.net/oded/";
        //ftpUser = "tvinciibc";
        //ftpPassword = "fNkG8372";
        //return;

        ftpAddress = string.Empty;
        ftpUser = string.Empty;
        ftpPassword = string.Empty;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select pics_ftp_username,pics_ftp_password from groups";
        selectQuery += "where";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", groupID);
        if (selectQuery.Execute("query", true) != null)
        {
            int nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                DataTable dtFtpData = selectQuery.Table("query");
                ftpAddress = GetFtpPathFromConfiguration(groupID);
                ftpUser = dtFtpData.Rows[0]["pics_ftp_username"].ToString();
                ftpPassword = dtFtpData.Rows[0]["pics_ftp_password"].ToString();
            }
        }
        selectQuery.Finish();
        selectQuery = null;
    }

    protected string GetFtpPathFromConfiguration(int groupID)
    {  
        string groupFtpKey = GROUP_CONFIG_KEY_PREFIX + groupID.ToString();
        string retFtp  = TVinciShared.WS_Utils.GetTcmConfigValue(groupFtpKey);

        return retFtp;
    }



}