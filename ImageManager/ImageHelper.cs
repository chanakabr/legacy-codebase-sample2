using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImageResizer;
using System.Net;
using System.IO;
using KLogMonitor;
using System.Reflection;

namespace ImageManager
{
    public class ImageHelper
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        static public bool DownloadAndCropImage(int nGroupID, string sURL, string sBasePath, List<ImageObj> images, string sPicBaseName, string sUploadedFileExt)
        {
            log.Debug("DownloadAndCropImage - " + string.Format("GroupID:{0}, images:{1}", nGroupID, images.Count));

            string name = string.Format("{0}_original.{1}", sPicBaseName, sUploadedFileExt.Replace(".", string.Empty));
            string originalPath = string.Format("{0}\\{1}", sBasePath, name);

            bool res = DownloadImage(sURL, originalPath);
            if (!res)
            {
                log.Debug("DownloadAndCropImage - " + string.Format("Error : Exception while download img"));
                return res;
            }

            foreach (ImageObj image in images)
            {
                string dest = string.Format("{0}\\{1}", sBasePath, image.ToString());
                try
                {
                    ImageResizer.ImageBuilder.Current.Build(originalPath, dest, image.oResizeSettings, false);
                    image.eResizeStatus = ResizeStatus.SUCCESS;
                }
                catch (Exception ex)
                {
                    image.eResizeStatus = ResizeStatus.FAILED;
                    log.Error("DownloadAndCropImage - " + string.Format("(Resize) Exception:{0}", ex.Message), ex);
                }
                log.Debug("DownloadAndCropImage - " + string.Format("GroupID:{0}, Image:{1}, {2}", nGroupID, image.ToString(), image.eResizeStatus));
            }

            RemoveImage(originalPath);
            return res;
        }

        static public bool DownloadImage(string sURL, string dest)
        {
            bool res = false;
            try
            {
                log.Debug("DownloadOriginlImage - " + string.Format("url:{0}, dest:{1}", sURL, dest));

                Uri uri = new Uri(sURL);
                HttpWebRequest httpRequest = (HttpWebRequest)HttpWebRequest.Create(uri);
                HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();

                using (Stream imageStream = httpResponse.GetResponseStream())
                {
                    try
                    {
                        ResizeSettings oResizeSettings = new ResizeSettings();
                        oResizeSettings.Quality = 100;
                        ImageResizer.ImageBuilder.Current.Build(imageStream, dest, oResizeSettings, false);
                        res = true;
                    }
                    catch (Exception ex)
                    {
                        log.Error("DownloadOriginlImage - " + string.Format("(imageStream) Exception:{0}", ex.Message), ex);
                        dest = string.Empty;
                    }
                    finally
                    {
                        httpResponse.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("DownloadOriginlImage - " + string.Format("Exception:{0}", ex.Message), ex);
                res = false;
            }

            return res;
        }

        static public void RemoveImage(string sImagePath)
        {
            try
            {
                FileInfo fileInf = new FileInfo(sImagePath);
                if (!fileInf.Exists)
                {
                    throw new Exception("File does not exist : " + sImagePath);
                }
                fileInf.Delete();
                log.Debug("RemoveOriginalImage - " + string.Format("Delete:{0}", sImagePath));
            }
            catch (Exception ex)
            {
                log.Error("RemoveOriginalImage - " + string.Format("(Delete) Exception:{0}", ex.Message), ex);
            }
        }
    }

}
