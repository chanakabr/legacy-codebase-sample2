using System;
using System.Collections.Generic;
using System.Net.Http;
using System.IO;
using KLogMonitor;
using System.Reflection;
using SkiaSharp;
using TVinciShared;

namespace ImageManager
{
    public class ImageHelper
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly HttpClient httpClient = HttpClientUtil.GetHttpClient();

        public static bool DownloadAndCropImage(int nGroupID, string sURL, string sBasePath, List<ImageObj> images, string sPicBaseName, string sUploadedFileExt)
        {
            _Logger.Debug($"DownloadAndCropImage - GroupID:{nGroupID}, images:{images.Count}");

            var name = $"{sPicBaseName}_original.{sUploadedFileExt.Replace(".", string.Empty)}";
            var originalPath = $"{sBasePath}\\{name}";

            bool res = DownloadImage(sURL, originalPath);
            if (!res)
            {
                _Logger.Debug($"DownloadAndCropImage - Error : Exception while download img");
                return res;
            }

            foreach (var image in images)
            {
                string dest = $"{sBasePath}\\{image.ToString()}";
                try
                {
                    ResizeOrCropAndSaveImage(originalPath, dest, image.oResizeSettings);
                    image.eResizeStatus = ResizeStatus.SUCCESS;
                }
                catch (Exception ex)
                {
                    image.eResizeStatus = ResizeStatus.FAILED;
                    _Logger.Error($"DownloadAndCropImage - (Resize) Exception:{ex.Message}", ex);
                }
                _Logger.Debug($"DownloadAndCropImage - GroupID:{nGroupID}, Image:{image.ToString()}, {image.eResizeStatus}");
            }

            RemoveImage(originalPath);
            return res;
        }



        public static bool DownloadImage(string sURL, string dest)
        {
            bool res = false;
            try
            {
                _Logger.Debug("DownloadOriginlImage - " + string.Format("url:{0}, dest:{1}", sURL, dest));
                using (var httpResponse = httpClient.GetAsync(sURL).ExecuteAndWait().EnsureSuccessStatusCode())
                using (var imageStream = httpResponse.Content.ReadAsStreamAsync().ExecuteAndWait())
                {
                    try
                    {
                        var oResizeSettings = new ResizeSettings { Quality = 100 };
                        ResizeOrCropAndSaveImage(imageStream, dest, oResizeSettings);
                        res = true;
                    }
                    catch (Exception ex)
                    {
                        _Logger.Error($"DownloadOriginlImage - (imageStream) Exception:{ex.Message}", ex);
                        dest = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                _Logger.Error($"DownloadOriginlImage - Exception:{ex.Message}", ex);
                res = false;
            }

            return res;
        }

        public static void RemoveImage(string sImagePath)
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


        public static void ResizeOrCropAndSaveImage(byte[] inputByteArr, Stream destStream, ResizeSettings resizeSettings)
        {
            var skData = SKData.CreateCopy(inputByteArr);
            var bitmap = SKBitmap.Decode(skData);
            ResizeOrCropSKBitmap(destStream, resizeSettings, bitmap);
        }


        public static void ResizeOrCropAndSaveImage(Stream inputImageStream, string destinationPath, ResizeSettings resizeSettings)
        {
            var bitmap = SKBitmap.Decode(inputImageStream);
            using (var fs = new FileStream(destinationPath, FileMode.Create))
            {
                ResizeOrCropSKBitmap(fs, resizeSettings, bitmap);
            }
        }

        public static void ResizeOrCropAndSaveImage(string imagePath, string destinationPath, ResizeSettings resizeSettings)
        {
            using (var fs = new FileStream(destinationPath, FileMode.Create))
            {
                ResizeOrCropAndSaveImage(imagePath, fs, resizeSettings);
            }
        }

        public static void ResizeOrCropAndSaveImage(string imagePath, Stream destStream, ResizeSettings resizeSettings)
        {
            var bitmap = SKBitmap.Decode(imagePath);
            ResizeOrCropSKBitmap(destStream, resizeSettings, bitmap);
        }

        private static void ResizeOrCropSKBitmap(Stream destStream, ResizeSettings resizeSettings, SKBitmap bitmap)
        {
            var image = SKImage.FromBitmap(bitmap);

            if (resizeSettings.FitMode == FitMode.Crop)
            {
                var subset = image.Subset(SKRectI.Create(20, 20, 90, 90));
                using (var imageData = subset.Encode(SKEncodedImageFormat.Png, resizeSettings.Quality))
                {
                    imageData.SaveTo(destStream);
                }
            }
            else if (resizeSettings.FitMode == FitMode.Resize)
            {
                var resizeInfo = new SKImageInfo(resizeSettings.Width, resizeSettings.Height);
                using (var resizedBitmap = bitmap.Resize(resizeInfo, SKFilterQuality.High))
                using (var newImg = SKImage.FromPixels(resizedBitmap.PeekPixels()))
                using (var imageData = newImg.Encode(SKEncodedImageFormat.Png, resizeSettings.Quality))
                {
                    imageData.SaveTo(destStream);

                }
            }
        }
    }
}
