using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;
using System.IO;
using Uploader;
using KLogMonitor;
using System.Reflection;

public partial class AjaxPicResize : System.Web.UI.Page
{
    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    protected void Page_Load(object sender, EventArgs e)
    {
        log.Debug("AjaxPicResize - Begin Ajax Pic Resize");

        string sRet = "Fail";
        string picWidth = Request.QueryString["w"];
        string picHeight = Request.QueryString["h"];
        string group_id = Request.QueryString["gid"];
        bool bCrop = false;
        bool.TryParse(Request.QueryString["c"], out bCrop);

        int nGroupID = int.Parse(group_id);

        object oPicsBasePath = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_REMOTE_BASE_URL", nGroupID);
        string sPicsBasePath = "";

        if (oPicsBasePath != DBNull.Value && oPicsBasePath != null)
            sPicsBasePath = oPicsBasePath.ToString();

        if (!string.IsNullOrEmpty(sPicsBasePath) && sPicsBasePath.IndexOf("http:") == -1)
        {
            sPicsBasePath = string.Format("http://{0}", sPicsBasePath);
        }

        sPicsBasePath = sPicsBasePath.EndsWith("/") ? sPicsBasePath : sPicsBasePath + "/";

        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        selectQuery += " select p.base_url from pics p, media m where m.status = 1 and ";
        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.group_id", "=", int.Parse(group_id));
        selectQuery += " and p.id = m.media_pic_id and p.status = 1 and m.media_pic_id <> 0";
        if (selectQuery.Execute("query", true) != null)
        {
            int count = selectQuery.Table("query").DefaultView.Count;
            log.Debug("AjaxPicResize - Found " + count + " Pics");
            if (count > 0)
            {
                string sDirectory = string.Format("{0}/pics/{1}/{2}X{3}/", HttpContext.Current.Server.MapPath(""), group_id, picWidth, picHeight);
                if (!Directory.Exists(sDirectory))
                {
                    Directory.CreateDirectory(sDirectory);
                }
                for (int i = 0; i < count; i++)
                {
                    log.Debug("AjaxPicResize - Progress " + i.ToString() + " : " + count);
                    string baseURL = selectQuery.Table("query").DefaultView[i].Row["base_url"].ToString();
                    string sUploadedFileExt = "";
                    int nExtractPos = baseURL.LastIndexOf(".");
                    string sPicBaseName = "";
                    if (nExtractPos > 0)
                    {
                        sUploadedFileExt = baseURL.Substring(nExtractPos);
                        sPicBaseName = baseURL.Substring(0, nExtractPos);
                    }
                    if (!string.IsNullOrEmpty(sPicBaseName))
                    {
                        string sFullPicPath = string.Format("{0}{1}_full{2}", sPicsBasePath, sPicBaseName, sUploadedFileExt);
                        log.Debug("AjaxPicResize - Download Web Image " + sFullPicPath);
                        string sDownloadImage = ImageUtils.DownloadWebImage(sFullPicPath);
                        log.Debug("AjaxPicResize - Download Web Image " + sFullPicPath + " Returned " + sDownloadImage);

                        if (!string.IsNullOrEmpty(sDownloadImage))
                        {
                            string sOrigDirectory = string.Format("{0}/pics/", HttpContext.Current.Server.MapPath(""));
                            string sResizePath = string.Format("{0}{1}_{2}X{3}{4}", sDirectory, sPicBaseName, picWidth, picHeight, sUploadedFileExt);
                            string sOrigPath = string.Format("{0}{1}_full{2}", sOrigDirectory, sPicBaseName, sUploadedFileExt);
                            ImageUtils.ResizeImageAndSave(sOrigPath, sResizePath, int.Parse(picWidth), int.Parse(picHeight), bCrop);

                        }
                    }
                }

                log.Debug("AjaxPicResize - Start FTP Upload from " + sDirectory);

                DBManipulator.UploadDirectoryToGroup(nGroupID, sDirectory);
            }
        }
        selectQuery.Finish();
        selectQuery = null;
    }
}