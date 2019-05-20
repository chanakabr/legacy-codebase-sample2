using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using KLogMonitor;
using System.Reflection;
using SkiaSharp;

namespace ImageManager
{
    public class ImageHelper
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        static public bool DownloadAndCropImage(int nGroupID, string sURL, string sBasePath, List<ImageObj> images, string sPicBaseName, string sUploadedFileExt)
        {
            _Logger.Debug("DownloadAndCropImage - " + string.Format("GroupID:{0}, images:{1}", nGroupID, images.Count));

            var name = string.Format("{0}_original.{1}", sPicBaseName, sUploadedFileExt.Replace(".", string.Empty));
            var originalPath = string.Format("{0}\\{1}", sBasePath, name);

            bool res = DownloadImage(sURL, originalPath);
            if (!res)
            {
                _Logger.Debug("DownloadAndCropImage - " + string.Format("Error : Exception while download img"));
                return res;
            }

            foreach (var image in images)
            {
                string dest = string.Format("{0}\\{1}", sBasePath, image.ToString());
                try
                {
                    ResizeOrCropAndSaveImage(originalPath, dest, image.oResizeSettings);
                    image.eResizeStatus = ResizeStatus.SUCCESS;
                }
                catch (Exception ex)
                {
                    image.eResizeStatus = ResizeStatus.FAILED;
                    _Logger.Error("DownloadAndCropImage - " + string.Format("(Resize) Exception:{0}", ex.Message), ex);
                }
                _Logger.Debug("DownloadAndCropImage - " + string.Format("GroupID:{0}, Image:{1}, {2}", nGroupID, image.ToString(), image.eResizeStatus));
            }

            RemoveImage(originalPath);
            return res;
        }



        static public bool DownloadImage(string sURL, string dest)
        {
            bool res = false;
            try
            {
                _Logger.Debug("DownloadOriginlImage - " + string.Format("url:{0}, dest:{1}", sURL, dest));

                var uri = new Uri(sURL);
                var httpRequest = (HttpWebRequest)HttpWebRequest.Create(uri);
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();

                using (var imageStream = httpResponse.GetResponseStream())
                {
                    try
                    {
                        var oResizeSettings = new ResizeSettings { Quality = 100 };
                        ResizeOrCropAndSaveImage(imageStream, dest, oResizeSettings);
                        res = true;
                    }
                    catch (Exception ex)
                    {
                        _Logger.Error("DownloadOriginlImage - " + string.Format("(imageStream) Exception:{0}", ex.Message), ex);
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
                _Logger.Error("DownloadOriginlImage - " + string.Format("Exception:{0}", ex.Message), ex);
                res = false;
            }

            return res;
        }

        static public void RemoveImage(string sImagePath)
        {
            try
            {
                var fileInf = new FileInfo(sImagePath);
                if (!fileInf.Exists)
                {
                    throw new Exception("File does not exist : " + sImagePath);
                }
                fileInf.Delete();
                _Logger.Debug("RemoveOriginalImage - " + string.Format("Delete:{0}", sImagePath));
            }
            catch (Exception ex)
            {
                _Logger.Error("RemoveOriginalImage - " + string.Format("(Delete) Exception:{0}", ex.Message), ex);
            }
        }


        private static void ResizeOrCropAndSaveImage(Stream inputImageStream, string destinationPath, ResizeSettings resizeSettings)
        {
            var bitmap = SKBitmap.Decode(inputImageStream);
            ResizeOrCropSKBitmap(destinationPath, resizeSettings, bitmap);
        }

        private static void ResizeOrCropAndSaveImage(string imagePath, string destinationPath, ResizeSettings resizeSettings)
        {
            var bitmap = SKBitmap.Decode(imagePath);
            ResizeOrCropSKBitmap(destinationPath, resizeSettings, bitmap);
        }

        private static void ResizeOrCropSKBitmap(string destinationPath, ResizeSettings resizeSettings, SKBitmap bitmap)
        {
            var image = SKImage.FromBitmap(bitmap);

            if (resizeSettings.FitMode == FitMode.Crop)
            {
                var subset = image.Subset(SKRectI.Create(20, 20, 90, 90));
                using (var imageData = subset.Encode(SKEncodedImageFormat.Png, resizeSettings.Quality))
                {
                    SaveImageDataToFile(imageData, destinationPath);
                }
            }
            else if (resizeSettings.FitMode == FitMode.Resize)
            {
                var resizeInfo = new SKImageInfo(resizeSettings.Width, resizeSettings.Height);
                using (var resizedBitmap = bitmap.Resize(resizeInfo, SKFilterQuality.High))
                using (var newImg = SKImage.FromPixels(resizedBitmap.PeekPixels()))
                using (var imageData = newImg.Encode(SKEncodedImageFormat.Png, resizeSettings.Quality))
                {
                    SaveImageDataToFile(imageData, destinationPath);

                }
            }
        }

        private static void SaveImageDataToFile(SKData data, string destPath)
        {
            using (var fs = new FileStream(destPath, FileMode.Create))
            {
                data.SaveTo(fs);
            }
        }
    }
}
