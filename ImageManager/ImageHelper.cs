using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImageResizer;
using System.Net;
using System.IO;

namespace ImageManager
{
    public class ImageHelper
    {
        static public bool DownloadAndCropImage(int nGroupID, string sURL, string sBasePath, List<ImageObj> images, string sPicBaseName, string sUploadedFileExt)
        {
            Logger.Logger.Log("DownloadAndCropImage", string.Format("GroupID:{0}, images:{1}", nGroupID, images.Count), "ImageManager");
            
            string name = string.Format("{0}_original.{1}", sPicBaseName, sUploadedFileExt.Replace(".", string.Empty));
            string originalPath = string.Format("{0}\\{1}", sBasePath, name);

            bool res = DownloadImage(sURL, originalPath);
            if (!res)
            {
                Logger.Logger.Log("DownloadAndCropImage", string.Format("Error : Exception while download img"), "ImageManager");
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
                    Logger.Logger.Log("DownloadAndCropImage", string.Format("(Resize) Exception:{0}", ex.Message), "ImageManager");
                }
                Logger.Logger.Log("DownloadAndCropImage", string.Format("GroupID:{0}, Image:{1}, {2}", nGroupID, image.ToString(), image.eResizeStatus), "ImageManager");
            }

            RemoveImage(originalPath);
            return res;
        }

        static public bool DownloadImage(string sURL, string dest)
        {
            bool res = false;
            try
            {
                Logger.Logger.Log("DownloadOriginlImage", string.Format("url:{0}, dest:{1}", sURL, dest), "ImageManager");

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
                        Logger.Logger.Log("DownloadOriginlImage", string.Format("(imageStream) Exception:{0}", ex.Message), "ImageManager");
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
                Logger.Logger.Log("DownloadOriginlImage", string.Format("Exception:{0}", ex.Message), "ImageManager");
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
                Logger.Logger.Log("RemoveOriginalImage", string.Format("Delete:{0}", sImagePath), "ImageManager");
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("RemoveOriginalImage", string.Format("(Delete) Exception:{0}", ex.Message), "ImageManager");
            }
        }
    }

}
