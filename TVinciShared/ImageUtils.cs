using ApiObjects;
using DAL;
using KLogMonitor;
using QueueWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Reflection;
using System.Web;

namespace TVinciShared
{
    /// <summary>
    /// Summary description for ImageUtils
    /// </summary>
    public class ImageUtils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public ImageUtils()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        static protected void GetNewSize(ref System.Drawing.Image theImage,
        ref Int32 nWidth, ref Int32 nHeight)
        {
            Int32 nOrigWidth = theImage.Width;
            Int32 nOrigHeight = theImage.Height;
            Int32 nNewWidth = nWidth;
            Int32 nNewHeight = nHeight;
            if ((double)((double)nOrigWidth / (double)nNewWidth) > (double)((double)nOrigHeight / (double)nNewHeight))
            {
                nNewHeight = (Int32)(((double)nOrigHeight) * ((double)((double)nNewWidth / (double)nOrigWidth)));
                if (nNewHeight > nOrigHeight)
                {
                    nNewWidth = nOrigWidth;
                    nNewHeight = nOrigHeight;
                }
            }
            else
            {
                nNewWidth = (Int32)(((double)nOrigWidth) * ((double)((double)nNewHeight / (double)nOrigHeight)));
                if (nNewWidth > nOrigWidth)
                {
                    nNewWidth = nOrigWidth;
                    nNewHeight = nOrigHeight;
                }
            }
            nWidth = nNewWidth;
            nHeight = nNewHeight;
        }

        static protected void GetNewSizeForCrop(ref System.Drawing.Image theImage,
            ref Int32 nWidth, ref Int32 nHeight)
        {
            Int32 nOrigWidth = theImage.Width;
            Int32 nOrigHeight = theImage.Height;
            Int32 nNewWidth = nWidth;
            Int32 nNewHeight = nHeight;
            if ((double)((double)nOrigWidth / (double)nNewWidth) < (double)((double)nOrigHeight / (double)nNewHeight))
            {
                nNewHeight = (Int32)(((double)nOrigHeight) * ((double)((double)nNewWidth / (double)nOrigWidth)));
                if (nNewHeight > nOrigHeight)
                {
                    nNewWidth = nOrigWidth;
                    nNewHeight = nOrigHeight;
                }
            }
            else
            {
                nNewWidth = (Int32)(((double)nOrigWidth) * ((double)((double)nNewHeight / (double)nOrigHeight)));
                if (nNewWidth > nOrigWidth)
                {
                    nNewWidth = nOrigWidth;
                    nNewHeight = nOrigHeight;
                }
            }
            nWidth = nNewWidth;
            nHeight = nNewHeight;
        }

        static public string GetDateImageName()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }

        static public string GetDateImageName(int mediaID)
        {
            string retVal = string.Empty;
            if (mediaID > 0)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += " select p.base_url, m.id from pics p, media m where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("m.id", "=", mediaID);
                selectQuery += " and p.id = m.media_pic_id and p.status = 1";
                selectQuery.SetCachedSec(0);
                if (selectQuery.Execute("query", true) != null)
                {
                    int count = selectQuery.Table("query").DefaultView.Count;
                    if (count > 0)
                    {
                        retVal = selectQuery.Table("query").DefaultView[0].Row["base_url"].ToString();
                        if (retVal.IndexOf('.') > 0)
                        {
                            retVal = retVal.Substring(0, retVal.IndexOf('.'));
                            log.Debug("BaseURL - " + string.Format("media:{0}, base:{1}", mediaID, retVal));
                        }
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }

            if (string.IsNullOrEmpty(retVal))
            {
                retVal = GetDateImageName();
            }

            return retVal;
        }

        static public string GetDateImageNameEpg(int epgPicID, ref bool bIsNew)
        {
            string retVal = string.Empty;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += " select ep.base_url from EPG_pics ep where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", epgPicID);
            selectQuery += " and ep.status = 1";
            selectQuery.SetCachedSec(0);
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    retVal = selectQuery.Table("query").DefaultView[0].Row["base_url"].ToString();
                    if (retVal.IndexOf('.') > 0)
                    {
                        retVal = retVal.Substring(0, retVal.IndexOf('.'));
                        log.Debug("BaseURL - " + string.Format("epg Pic ID:{0}, base:{1}", epgPicID, retVal));
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            if (string.IsNullOrEmpty(retVal))
            {
                retVal = GetDateImageName();
                bIsNew = true;
            }

            return retVal;
        }

        static public ImageFormat GetFileFormat(string sPath)
        {
            string sFileExt = "";
            int ExtractPos = sPath.LastIndexOf(".");
            if (ExtractPos > 0)
                sFileExt = sPath.Substring(ExtractPos);
            if (sFileExt.StartsWith("."))
                sFileExt = sFileExt.Substring(1).ToUpper();
            if (sFileExt == "GIF")
                return ImageFormat.Gif;
            else if (sFileExt == "JPG")
                return ImageFormat.Jpeg;
            else if (sFileExt == "JPEG")
                return ImageFormat.Jpeg;
            else if (sFileExt == "JPEG")
                return ImageFormat.Jpeg;
            else if (sFileExt == "BMP")
                return ImageFormat.Bmp;
            else if (sFileExt == "PNG")
                return ImageFormat.Png;
            else
                return ImageFormat.Jpeg;


        }

        static System.Drawing.Image FixedSize(System.Drawing.Image imgPhoto, int Width, int Height)
        {
            Bitmap bmPhoto = new Bitmap(Width, Height,
                              PixelFormat.Format32bppArgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);
            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(Color.Transparent);
            setGraphicsQuality(ref grPhoto);

            grPhoto.DrawImage(imgPhoto, 0, 0, Width, Height);
            grPhoto.Dispose();
            return bmPhoto;
        }

        static System.Drawing.Image Crop(System.Drawing.Image imgPhoto, int nCorpWidth, int nCorpHeight, Int32 nOrigWidth, Int32 nOrigHeight)
        {
            Bitmap bmPhoto = new Bitmap(nCorpWidth, nCorpHeight, PixelFormat.Format32bppArgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(Color.Transparent);
            setGraphicsQuality(ref grPhoto);
            Int32 nX = (nOrigWidth - nCorpWidth) / 2;
            Int32 nY = (nOrigHeight - nCorpHeight) / 2; ;
            if (nX < 0)
                nX = 0;
            if (nY < 0)
                nY = 0;
            Rectangle rectDestination = new Rectangle(0, 0, bmPhoto.Width, bmPhoto.Height);
            Rectangle rectCropArea = new Rectangle(nX, nY, bmPhoto.Width, bmPhoto.Height);

            grPhoto.DrawImage(imgPhoto, rectDestination, rectCropArea, GraphicsUnit.Pixel);
            grPhoto.Dispose();
            return bmPhoto;
        }

        static public void RenameImage(string sOld, string sNew)
        {
            try
            {
                System.IO.File.Copy(sOld, sNew);

                /*
                long quality = 100L;
                if (sNew.EndsWith("jpg"))
                {
                    quality = 80L;
                }
                EncoderParameter qualityParam = new EncoderParameter(Encoder.Quality, quality);

                //Get the encoder type
                ImageCodecInfo oCodec = getEncoderInfo(GetEncoderType(sNew));

                EncoderParameters encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = qualityParam;

                System.Drawing.Image fullSizeImg = System.Drawing.Image.FromFile(sOld);
                if (File.Exists(sNew) == false)
                    fullSizeImg.Save(sNew, oCodec, encoderParams);
                fullSizeImg.Dispose();
                */
            }
            catch (Exception ex)
            {
                log.Error("RenameImage - old:" + sOld + ", new:" + sNew + ", ex:" + ex.Message);
            }
        }
        /*
        static public void ResizeImageAndSave(string sFullImagePath, string sResizablePath, Int32 nNewWidth, Int32 nNewHeight, bool bCrop)
        {
            System.Drawing.Image fullSizeImg = System.Drawing.Image.FromFile(sFullImagePath);
            if (bCrop == false)
            {
                GetNewSize(ref fullSizeImg, ref nNewWidth, ref nNewHeight);
                System.Drawing.Image bmp = FixedSize(fullSizeImg, nNewWidth, nNewHeight);
                bmp.Save(sResizablePath, ImageFormat.Png);
                bmp.Dispose();
            }
            else
            {
                Int32 nWidth = nNewWidth;
                Int32 nHeight = nNewHeight;
                GetNewSizeForCrop(ref fullSizeImg, ref nNewWidth, ref nNewHeight);
                System.Drawing.Image bmpTmp = FixedSize(fullSizeImg, nNewWidth, nNewHeight);
                System.Drawing.Image bmp = Crop(bmpTmp, nWidth, nHeight, nNewWidth, nNewHeight);
                bmp.Save(sResizablePath, ImageFormat.Png);
                bmp.Dispose();
            }
            fullSizeImg.Dispose();
        }
        */

        static public void DynamicResizeImage(string sFullImageURL, Int32 nNewWidth, Int32 nNewHeight, bool bCrop, ref System.Drawing.Image i)
        {
            try
            {
                //Bitmap theImage = new Bitmap(imageStream);
                if (bCrop == false)
                {
                    GetNewSize(ref i, ref nNewWidth, ref nNewHeight);
                    i = FixedSize(i, nNewWidth, nNewHeight);
                }
                else
                {
                    Int32 nWidth = nNewWidth;
                    Int32 nHeight = nNewHeight;
                    GetNewSizeForCrop(ref i, ref nNewWidth, ref nNewHeight);
                    i = FixedSize(i, nNewWidth, nNewHeight);
                    i = Crop(i, nWidth, nHeight, nNewWidth, nNewHeight);
                }

                // Encoder parameter for image quality
                //long quality = 100L;

                //EncoderParameter qualityParam = new EncoderParameter(Encoder.Quality, quality);

                ////Get the encoder type
                //ImageCodecInfo oCodec = getEncoderInfo(GetEncoderType(sFullImageURL));
                ////EncoderParameters encoderParams = new EncoderParameters(1);
                //EncoderParameters encoderParams = i.GetEncoderParameterList(oCodec.Clsid);
                ////encoderParams.Param[0] = qualityParam;
                ////i.Dispose();
            }
            catch (Exception ex)
            {
                log.Error("error resizing image " + sFullImageURL, ex);
            }
        }

        static public void ResizeImageAndSave(string sFullImagePath, string sResizablePath, Int32 nNewWidth, Int32 nNewHeight, bool bCrop, bool isOverride)
        {
            System.Drawing.Image bmp = null;
            System.Drawing.Image fullSizeImg = System.Drawing.Image.FromFile(sFullImagePath);
            try
            {
                if (bCrop == false)
                {
                    GetNewSize(ref fullSizeImg, ref nNewWidth, ref nNewHeight);
                    bmp = FixedSize(fullSizeImg, nNewWidth, nNewHeight);
                }
                else
                {
                    Int32 nWidth = nNewWidth;
                    Int32 nHeight = nNewHeight;
                    GetNewSizeForCrop(ref fullSizeImg, ref nNewWidth, ref nNewHeight);
                    System.Drawing.Image bmpTmp = FixedSize(fullSizeImg, nNewWidth, nNewHeight);
                    bmp = Crop(bmpTmp, nWidth, nHeight, nNewWidth, nNewHeight);
                }

                // Encoder parameter for image quality

                long quality = 100L;
                if (sFullImagePath.EndsWith("jpg"))
                {
                    quality = 80L;
                }

                EncoderParameter qualityParam = new EncoderParameter(Encoder.Quality, quality);

                //Get the encoder type
                ImageCodecInfo oCodec = getEncoderInfo(GetEncoderType(sResizablePath));

                EncoderParameters encoderParams = new EncoderParameters(1);
                //EncoderParameters encoderParams = fullSizeImg.GetEncoderParameterList(oCodec.Clsid);
                encoderParams.Param[0] = qualityParam;
                if (File.Exists(sResizablePath) == false || isOverride)
                {
                    bmp.Save(sResizablePath, oCodec, encoderParams);
                    log.Debug("Pic Resize - Pic " + sResizablePath + " saved ");
                }
                //bmp.Save(sResizablePath);
                //bmp.Save(sResizablePath, ImageFormat.Png);
                bmp.Dispose();

                fullSizeImg.Dispose();
            }
            catch (Exception ex)
            {
                if (bmp != null)
                {
                    bmp.Dispose();
                    log.Error("Pic Resize - Pic " + sResizablePath + " not saved " + ex.Message, ex);
                }
            }
        }

        static public void ResizeImageAndSave(string sFullImagePath, string sResizablePath, Int32 nNewWidth, Int32 nNewHeight, bool bCrop)
        {
            ResizeImageAndSave(sFullImagePath, sResizablePath, nNewWidth, nNewHeight, bCrop, false);
        }

        static public string GetEncoderType(string sFileName)
        {
            ImageFormat oImageFormat = GetFileFormat(sFileName);

            if (oImageFormat == ImageFormat.Png)
                return "image/png";
            else if (oImageFormat == ImageFormat.Jpeg)
                return "image/jpeg";
            else if (oImageFormat == ImageFormat.Bmp)
                return "image/bmp";
            else if (oImageFormat == ImageFormat.Gif)
                return "image/gif";
            else
                return "image/jpeg";
        }

        static private ImageCodecInfo getEncoderInfo(string mimeType)
        {
            // Get image codecs for all image formats
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            // Find the correct image codec
            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];

            return null;
        }

        static public string GetTNName(string sBaseName, string sEnding)
        {
            string sRet = "";
            string[] s = sBaseName.Split('.');
            if (s.Length == 2)
            {
                sRet += s[0] + "_";
                sRet += sEnding;
                sRet += ".";
                sRet += s[1];
                if (sEnding.Equals("tn"))
                {
                    Random random = new Random();
                    int randomInt = random.Next();
                    sRet += "?";
                    sRet += randomInt.ToString();
                }
            }
            return sRet;
        }

        static private void setGraphicsQuality(ref Graphics g)
        {
            //g.InterpolationMode =
            //System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
            g.PixelOffsetMode =
                System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            g.SmoothingMode =
                System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        }

        static public string DownloadWebImage(string sURL)
        {
            return DownloadWebImage(sURL, string.Empty);
        }

        static public string DownloadWebImage(string sURL, string sDirectory)
        {
            log.Debug("File downloaded - Start Download Url:" + " " + sURL);
            try
            {
                string sBasePath = "";
                if (string.IsNullOrEmpty(sDirectory))
                {
                    if (HttpContext.Current != null)
                        sBasePath = HttpContext.Current.Server.MapPath("");
                    else
                    {
                        if (!string.IsNullOrEmpty(HttpRuntime.AppDomainAppPath))
                        {
                            sBasePath = HttpRuntime.AppDomainAppPath;
                        }
                        else
                            sBasePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    }

                    log.Debug("Web path - " + sBasePath);
                }
                else
                {
                    sBasePath = sDirectory;
                }
                char[] delim = { '/' };
                Uri uri = new Uri(sURL);
                string[] splited = sURL.Split(delim);
                string sPicBaseName = splited[splited.Length - 1];
                if (sPicBaseName.IndexOf("?") != -1 && sPicBaseName.IndexOf("uuid") != -1)
                {
                    Int32 nStart = sPicBaseName.IndexOf("uuid=", 0) + 5;
                    Int32 nEnd = sPicBaseName.IndexOf("&", nStart);
                    if (nEnd != 4)
                        sPicBaseName = sPicBaseName.Substring(nStart, nEnd - nStart);
                    else
                        sPicBaseName = sPicBaseName.Substring(nStart);
                    sPicBaseName += ".jpg";
                }
                string sTmpImage = sBasePath + "/pics/" + sPicBaseName;

                HttpWebRequest httpRequest = (HttpWebRequest)HttpWebRequest.Create(uri);
                HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();

                using (Stream inputStream = httpResponse.GetResponseStream())
                using (Stream outputStream = File.OpenWrite(sTmpImage))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    do
                    {
                        bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                        outputStream.Write(buffer, 0, bytesRead);
                    } while (bytesRead != 0);
                }
                httpResponse.Close();

                log.Debug("File downloaded - Url:" + " " + sURL + " " + "File:" + " " + sPicBaseName);
                return sPicBaseName;
            }
            catch (Exception ex)
            {
                log.Error("Exception - " + sURL + " " + ex.Message + " " + ex.InnerException, ex);
                return "";
            }
        }


        public static string GetFileExt(string sFileName)
        {
            string sFileExt = string.Empty;
            int nExtractPos = sFileName.LastIndexOf(".");
            if (nExtractPos > 0)
                sFileExt = sFileName.Substring(nExtractPos);
            return sFileExt;
        }


        public static bool SendPictureDataToQueue(string sFullUrlDownload, string sNewName, string sBasePath, string[] sPicSizes, int nGroupID)
        {
            bool bIsUpdateSucceeded = false;
            List<object> args = new List<object>();

            int nParentGroupID = 0;

            try
            {
                nParentGroupID = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("groups", "COMMERCE_GROUP_ID", nGroupID, "MAIN_CONNECTION_STRING").ToString());
            }
            catch (Exception ex)
            {
                log.Error("Exception - " + string.Format("group:{0}, msg:{1}", nGroupID, ex.Message), ex);
                return false;
            }


            args.Add(nParentGroupID.ToString());

            //check for Http and if it is missing, insert the the remotePicsURL
            if (sFullUrlDownload.ToLower().Trim().StartsWith("http://") == false &&
                sFullUrlDownload.ToLower().Trim().StartsWith("https://") == false)
            {
                sFullUrlDownload = getRemotePicsURL(nGroupID) + sFullUrlDownload;
            }

            args.Add(sFullUrlDownload);//the full url from which the picture should be taken
            args.Add(sNewName);
            args.Add(sPicSizes);

            UploadConfig upConfig = new UploadConfig();
            upConfig.setUploadConfig(nGroupID);
            args.Add(upConfig);

            args.Add(sBasePath);

            string id = Guid.NewGuid().ToString();
            string task = TVinciShared.WS_Utils.GetTcmConfigValue("taskPicture");
            ApiObjects.PictureData data = new ApiObjects.PictureData(id, task, args);
            log.Debug("Queue - " + string.Format("{0}, {1}, {2}", nParentGroupID, id, task));

            //update the Queue with picture data
            if (data != null)
            {
                BaseQueue queue = new PictureQueue();
                string sRoutingKey = TVinciShared.WS_Utils.GetTcmConfigValue("routingKeyPicture");
                bIsUpdateSucceeded = queue.Enqueue(data, sRoutingKey);
            }
            log.Debug("Res - " + bIsUpdateSucceeded.ToString());
            return bIsUpdateSucceeded;
        }

        //get the url from which the pics are downloaded 
        public static string getRemotePicsURL(int nGroupID)
        {
            string sRemotePicsURL = string.Empty;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select PICS_REMOTE_BASE_URL from groups (nolock) where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sRemotePicsURL = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "PICS_REMOTE_BASE_URL", 0);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return sRemotePicsURL;
        }

        internal static void GetDateEpgImageDetails(string sPicDescription, int groupID, ref bool isNew, ref string picName, ref int picID, ref string baseURL)
        {
            isNew = true;
            baseURL = GetDateImageName(); // if nothing exsist generate name

            DataTable dt = Tvinci.Core.DAL.EpgDal.GetDateEpgImageDetails(sPicDescription, groupID);
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                isNew = false;
                picName = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "name");
                picID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "id");
                baseURL = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "base_url");
                baseURL = baseURL.Substring(0, baseURL.IndexOf('.'));

            }

            //string retVal = string.Empty;
            //ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            //selectQuery += " select id, name, base_url  from EPG_pics ep where ";
            //selectQuery += " ep.status = 1";
            //selectQuery += " and ";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("description", "=", sPicDescription);
            //selectQuery += " and ";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", groupID);

            //selectQuery.SetCachedSec(0);
            //if (selectQuery.Execute("query", true) != null)
            //{
            //    int count = selectQuery.Table("query").DefaultView.Count;
            //    if (count > 0)
            //    {

            //        retVal = selectQuery.Table("query").DefaultView[0].Row["base_url"].ToString();
            //        if (retVal.IndexOf('.') > 0)
            //        {
            //            retVal = retVal.Substring(0, retVal.IndexOf('.'));
            //        }
            //    }
            //}
            //selectQuery.Finish();
            //selectQuery = null;

            //if (string.IsNullOrEmpty(retVal))
            //{
            //    retVal = GetDateImageName();
            //    bIsNew = true;
            //}

            //return retVal;
        }

        public static string GetImageServerUrl(int groupId, eHttpRequestType httpRequestType)
        {
            string imageServerUrl = string.Empty;
            object imageServerUrlObj;
            {

                switch (httpRequestType)
                {
                    case eHttpRequestType.Post:

                        imageServerUrlObj = PageUtils.GetTableSingleVal("groups", "INTERNAL_IMAGE_SERVER_URL", groupId);
                        if (imageServerUrlObj == null || string.IsNullOrWhiteSpace(imageServerUrlObj.ToString()))
                            log.ErrorFormat(string.Format("INTERNAL_IMAGE_SERVER_URL wasn't found. GID: {0}", groupId));
                        else
                        {
                            imageServerUrl = imageServerUrlObj.ToString();
                            imageServerUrl = imageServerUrl.EndsWith("/") ? imageServerUrl + "InsertImage" : imageServerUrl + "/InsertImage";
                        }
                        break;

                    case eHttpRequestType.Get:

                        imageServerUrlObj = PageUtils.GetTableSingleVal("groups", "IMAGE_SERVER_URL", groupId);
                        if (imageServerUrlObj == null || string.IsNullOrWhiteSpace(imageServerUrlObj.ToString()))
                            log.ErrorFormat(string.Format("IMAGE_SERVER_URL wasn't found. GID: {0}", groupId));
                        else
                        {
                            imageServerUrl = imageServerUrlObj.ToString();
                            imageServerUrl = imageServerUrl.EndsWith("/") ? imageServerUrl + "GetImage/" : imageServerUrl + "/GetImage/";
                        }
                        break;

                    default:
                        break;
                }

            }
            return imageServerUrl;
        }

        public static string BuildImageUrl(int groupId, string imageId, int version = 0, int width = 0, int height = 0, int quality = 100, bool isDynamic = false)
        {
            string imageServerUrl = string.Empty;

            if (groupId == 0)
            {
                log.Error(string.Format("Illegal group ID. GID: {0}", groupId));
                return imageServerUrl;
            }

            if (string.IsNullOrEmpty(imageId))
            {
                log.Error(string.Format("Illegal imageId. GID: {0}", groupId));
                return imageServerUrl;
            }

            imageServerUrl = ImageUtils.GetImageServerUrl(groupId, eHttpRequestType.Get);
            if (string.IsNullOrEmpty(imageServerUrl))
            {
                log.Error(string.Format("IMAGE_SERVER_URL wasn't found. GID: {0}", groupId));
                return imageServerUrl;
            }

            if (isDynamic)
            {
                imageServerUrl = string.Format("{0}p/{1}/entry_id/{2}/version/{3}",
                                              imageServerUrl,        // 0 <image_server_url>
                                              groupId,               // 1 <partner_id>
                                              imageId,               // 2 <image_id>
                                              version);              // 3 <image_version>
            }
            else
            {
                imageServerUrl = string.Format("{0}p/{1}/entry_id/{2}/version/{3}/width/{4}/height/{5}/quality/{6}",
                                           imageServerUrl,       // 0 <image_server_url>
                                           groupId,              // 1 <partner_id>
                                           imageId,              // 2 <image_id>
                                           version,              // 3 <image_version>
                                           width,                // 4 <image_width>
                                           height,               // 5 <image_height>
                                           quality);             // 6 <quality>
            }

            return imageServerUrl;
        }

        public static bool IsDownloadPicWithImageServer(int groupId = 0)
        {
            // true in case image server is in use
            //------------------------------------
            if (groupId > 0)
                return !WS_Utils.IsGroupIDContainedInConfig(groupId, "USE_OLD_IMAGE_SERVER", ';');
            else
                return !WS_Utils.IsGroupIDContainedInConfig(LoginManager.GetLoginGroupID(), "USE_OLD_IMAGE_SERVER", ';');

        }

        public static string GetEpgPicImageUrl(string epgChannelId, out int picId)
        {
            string imageUrl = string.Empty;
            string baseUrl = string.Empty;
            int ratioId = 0;
            int version = 0;
            picId = 0;
            int groupId = LoginManager.GetLoginGroupID();

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select p.BASE_URL, p.ID, p.version from epg_pics p left join epg_channels ec on ec.PIC_ID = p.ID where p.STATUS in (0, 1) and ec.id = " + epgChannelId.ToString();

            if (selectQuery.Execute("query", true) != null && selectQuery.Table("query").DefaultView != null && selectQuery.Table("query").DefaultView.Count > 0)
            {

                baseUrl = ODBCWrapper.Utils.GetSafeStr(selectQuery.Table("query").DefaultView[0].Row["BASE_URL"]);
                picId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row["ID"]);
                version = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row["version"]);
                int parentGroupID = DAL.UtilsDal.GetParentGroupID(groupId);

                imageUrl = PageUtils.BuildEpgUrl(parentGroupID, baseUrl, ratioId, version);
            }

            return imageUrl;
        }

        public static bool IsUrlExists(string url)
        {
            HttpWebResponse response = null;

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "HEAD";
                response = (HttpWebResponse)request.GetResponse();
                //log.Debug("URL validated" + url);
                return true;
            }
            catch (WebException)
            {
                log.Error("URL wasn't found" + url);
                return false;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }
        }

        public static bool UpdateImageState(int groupId, long rowId, int version, eMediaType mediaType, eTableStatus status, int? updaterId = null)
        {
            bool res = false;
            int queryRes = 0;

            switch (status)
            {
                case eTableStatus.OK:

                    if (mediaType == eMediaType.VOD)
                        queryRes = ApiDAL.UpdateImageState(groupId, rowId, version, status, updaterId);

                    if (mediaType == eMediaType.EPG)
                        queryRes = ApiDAL.UpdateEpgImageState(groupId, rowId, version, status, updaterId);

                    if (queryRes > 0)
                    {
                        log.DebugFormat("{0} Successfully updated image state. groupId: {1}, imageId: {2}, version: {3}",
                            mediaType.ToString(),
                            groupId,
                            rowId,
                            version);
                        res = true;
                    }
                    else if (queryRes == 0)
                    {
                        log.WarnFormat("{0} Error while updating image state - 0 rows updated. groupId: {1}, imageId: {2}, version: {3}", mediaType.ToString(), groupId, rowId, version);
                    }
                    else
                    {
                        log.ErrorFormat("{0} Error while updating image state. groupId: {1}, imageId: {2}, version: {3}", mediaType.ToString(), groupId, rowId, version);
                    }
                    break;

                case eTableStatus.Failed:

                    log.ErrorFormat("{0} Failed to post new image to image server. groupId: {1}, imageId: {2}, version: {3}",
                              mediaType.ToString(),
                              groupId,
                              rowId,
                              version);
                    break;

                default:
                case eTableStatus.Pending:

                    log.ErrorFormat("{0} Illegal state received. groupId: {1}, imageId: {2}, version: {3}, state: {4}",
                        mediaType.ToString(),
                        groupId,
                        rowId,
                        version,
                        status.ToString());
                    break;
            }

            return res;
        }

        public static int GetGroupDefaultRatio(int groupId)
        {
            int rationId = 0;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
            selectQuery += "select RATIO_ID from groups (nolock) where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", groupId);
            selectQuery.SetCachedSec(120);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    rationId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "RATIO_ID", 0);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return rationId;
        }

        public static int GetGroupDefaultEpgRatio(int groupId)
        {
            int rationId = 0;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
            selectQuery += "select ISNULL( EPG_RATIO_ID, RATIO_ID) as 'RATIO_ID' from groups (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", groupId);
            selectQuery.SetCachedSec(120);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    rationId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "RATIO_ID", 0);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return rationId;
        }

        public static string GetImageUrl(int picId, int groupId)
        {
            string imageUrl = string.Empty;
            try
            {
                bool isImageServer = false;
                isImageServer = ImageUtils.IsDownloadPicWithImageServer(LoginManager.GetLoginGroupID());

                if (groupId == 0)
                {
                    groupId = LoginManager.GetLoginGroupID();
                }

                int parentGroupID = DAL.UtilsDal.GetParentGroupID(groupId);

                string basePicsURL = string.Empty;
                int defaultPicId = 0;
                int parentDefaultPicId = 0;

                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select DEFAULT_PIC_ID, PICS_REMOTE_BASE_URL, id from groups WITH(NOLOCK) where id in ( " + groupId + "," + parentGroupID + " )";
                if (selectQuery.Execute("query", true) != null)
                {
                    DataTable dt = selectQuery.Table("query");
                    foreach (DataRow dr in dt.Rows)
                    {

                        int group = ODBCWrapper.Utils.GetIntSafeVal(dr, "id");
                        if (group == groupId)
                        {
                            basePicsURL = ODBCWrapper.Utils.GetSafeStr(dr, "PICS_REMOTE_BASE_URL");
                            if (string.IsNullOrEmpty(basePicsURL))
                            {
                                basePicsURL = "pics";
                            }
                            else if (basePicsURL.ToLower().Trim().StartsWith("http://") == false && basePicsURL.ToLower().Trim().StartsWith("https://") == false)
                            {
                                basePicsURL = "http://" + basePicsURL;
                            }
                            defaultPicId = ODBCWrapper.Utils.GetIntSafeVal(dr, "DEFAULT_PIC_ID");
                        }
                        else if (group == parentGroupID)
                        {
                            parentDefaultPicId = ODBCWrapper.Utils.GetIntSafeVal(dr, "DEFAULT_PIC_ID");
                        }
                    }
                }
                selectQuery.Finish();
                selectQuery = null;

                if (picId == 0)
                {
                    picId = defaultPicId != 0 ? defaultPicId : parentDefaultPicId;
                }

                if (isImageServer)
                {
                    imageUrl = PageUtils.GetPicImageUrlByRatio(picId, 90, 65, groupId);
                }
                else
                {
                    imageUrl = basePicsURL;
                    string sPicURL = PageUtils.GetTableSingleVal("pics", "base_url", picId).ToString();
                    string sP = ImageUtils.GetTNName(sPicURL, "tn");

                    if (imageUrl.EndsWith("=") == false)
                    {
                        imageUrl = string.Format("{0}/", imageUrl);
                    }
                    if (imageUrl.EndsWith("=") == true)
                    {
                        string sTmp1 = "";
                        string[] s1 = sP.Split('.');
                        for (int j1 = 0; j1 < s1.Length - 1; j1++)
                        {
                            if (j1 > 0)
                                sTmp1 += ".";
                            sTmp1 += s1[j1];
                        }
                        sP = sTmp1;
                        imageUrl = string.Format("{0}{1}", imageUrl, sP);
                    }
                }
            }
            catch
            {
                imageUrl = string.Empty;
            }
            return imageUrl;
        }
    }
}