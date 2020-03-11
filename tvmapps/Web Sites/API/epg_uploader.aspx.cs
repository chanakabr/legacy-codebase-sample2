using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Data;

public partial class epg_uploader : System.Web.UI.Page
{
    private const string EPG_FEEDER_PARAMS = "{0}|{1}|WebURL|{2}";
    
    private const string EPG_XML_TV = "EPGxmlTv";
    private const string EPG_MEDIA_CORP_CHANNEL = "EPG_MediaCorp";
    private const string EPG_YES = "EPG_Yes";

    private const int MEDIA_CORP_GROUP_ID = 148;
    
    
    protected void Page_Load(object sender, EventArgs e)
    {    

    }
    protected void btnProceessXml_Click(object sender, EventArgs e)
    {
        if (fileUploader.HasFile == true)
        {
           string fileName = fileUploader.FileName;
            try
            {         
                int nGroupID;
                bool result = int.TryParse(txtGroupID.Text, out nGroupID);
                if (result == true)
                {
                    string folderPath = Server.MapPath(string.Empty);
                    folderPath = System.IO.Path.Combine(folderPath, "epg_upload");
                    folderPath = System.IO.Path.Combine(folderPath, nGroupID.ToString());
                    if (Directory.Exists(folderPath) == false)
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    string filePath = folderPath + @"/" + fileName;
                    fileUploader.SaveAs(filePath);

                    //ProcessXmlFile(txtGroupID.Text, txtChannel.Text, filePath);
                    //UploadXmlFileToFtp(nGroupID, filePath);

                    lblStatus.Text = "Upload status: File uploaded to Ftp!";                 
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Upload status: The file could not be uploaded. The following error occured: " + ex.Message;
            }
        
        }               
    }

    protected void ProcessXmlFile(string sGroupID, string sEPGChannel, string filePath)
    {
        int taskID = 1;
        int taskIntervalsInSeconds = 1;

        //int groupID = 0;
        //bool result = int.TryParse(sGroupID, out groupID);
        //if (result == true)       
        //{
        //    string epgFeederParams = string.Format(EPG_FEEDER_PARAMS, sGroupID, sEPGChannel, filePath);
        //    EpgFeeder.EpgFeederObj feeder = new EpgFeeder.EpgFeederObj(taskID, taskIntervalsInSeconds, epgFeederParams);
        //    feeder.DoTheTask();
        //}
        //else
        //{
        //    throw new Exception("GroupID must be number!!!"); 
        //}
    }
    
    protected void UploadXmlFileToFtp(int groupID, string filePath)
    {
        string ftpAddress;
        string ftpUser;
        string ftpPassword;
        GetFtpConnectionDetails(groupID, out ftpAddress, out ftpUser, out ftpPassword);
        //FTPUploadQueue.FTPUploader ftpUploader = new FTPUploadQueue.FTPUploader(filePath, ftpAddress, ftpUser, ftpPassword);
        //ftpUploader.Upload();
    }

    protected void GetFtpConnectionDetails(int groupID, out string ftpAddress, out string ftpUser, out string ftpPassword)
    {
        ftpAddress = "tvinci.cdnetworks.net/oded/";
        ftpUser = "tvinciibc";
        ftpPassword = "fNkG8372";
        return;

        ftpAddress = string.Empty;
        ftpUser = string.Empty;
        ftpPassword = string.Empty;

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += "select pics_ftp,pics_ftp_username,pics_ftp_password from groups";
        selectQuery += "where";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", groupID);
        if (selectQuery.Execute("query", true) != null)
        {
            int nCount = selectQuery.Table("query").DefaultView.Count;
            if (nCount > 0)
            {
                DataTable dtFtpData = selectQuery.Table("query");
                ftpAddress = dtFtpData.Rows[0]["pics_ftp"].ToString(); //TBD: change the column name to the new column(epg_ftp)
                ftpUser = dtFtpData.Rows[0]["pics_ftp_username"].ToString();
                ftpPassword = dtFtpData.Rows[0]["pics_ftp_password"].ToString();
            }
        }
    }



   
}