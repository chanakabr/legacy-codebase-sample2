using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.DRM;
using ApiObjects.Notification;
using ApiObjects.Response;
using ConfigurationManager;
using DAL;
using KLogMonitor;
using KlogMonitorHelper;
using Newtonsoft.Json;
using QueueWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using Tvinci.Core.DAL;
using TVinciShared;

namespace IngestHandler
{
    public class EpgImageManager
    {
        private static readonly HttpClient _HttpClient = new HttpClient();
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        protected const string ROUTING_KEY_PROCESS_IMAGE_UPLOAD = "PROCESS_IMAGE_UPLOAD\\{0}";
        protected const string ROUTING_KEY_PROCESS_FREE_ITEM_UPDATE = "PROCESS_FREE_ITEM_UPDATE\\{0}";

        private const string MISSING_EXTERNAL_IDENTIFIER = "External identifier is missing ";
        private const string MISSING_ENTRY_ID = "entry_id is missing";
        private const string MISSING_ACTION = "action is missing";
        private const string ITEM_TYPE_NOT_RECOGNIZED = "Item type not recognized";
        private const string WATCH_PERMISSION_RULE_NOT_RECOGNIZED = "Watch permission rule not recognized";
        private const string GEO_BLOCK_RULE_NOT_RECOGNIZED = "Geo block rule not recognized";
        private const string DEVICE_RULE_NOT_RECOGNIZED = "Device rule not recognized";
        private const string PLAYERS_RULE_NOT_RECOGNIZED = "Players rule not recognized ";
        private const string FAILED_DOWNLOAD_PIC = "Failed download pic";
        private const string UPDATE_INDEX_FAILED = "Update index failed";
        private const string ERROR_EXPORT_CHANNEL = "ErrorExportChannel";
        private const string MEDIA_ID_NOT_EXIST = "Media Id not exist";
        private const string EPG_SCHED_ID_NOT_EXIST = "EPG schedule id not exist";

        static string m_sLocker = "";


        public static int DoesEPGPicExists(string sPicBaseName, int nGroupID)
        {
            var nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from EPG_pics (nolock) where STATUS=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("description", "=", sPicBaseName);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        // DO NOT DELETE
        public static int DownloadEPGPic(string sThumb, string sName, int nGroupID, int nEPGSchedID, int nChannelID, int ratioID = 0, long imageTypeId = 0)
        {
            int picId = 0;

            if (string.IsNullOrEmpty(sThumb))
            {
                log.Debug("File download - picture name is empty. nChannelID: " + nChannelID.ToString());
                return 0;
            }

            // use old/or image queue
            if (WS_Utils.IsGroupIDContainedInConfig(nGroupID, ApplicationConfiguration.UseOldImageServer.Value, ';'))
            {
                bool sUseQueue = ApplicationConfiguration.DownloadPicWithQueue.Value;
                //use the rabbit Queue
                if (sUseQueue)
                {
                    picId = DownloadEPGPicToQueue(sThumb, sName, nGroupID, nEPGSchedID, nChannelID, ratioID);
                }
                else
                {
                    picId = DownloadEPGPicToUploader(sThumb, sName, nGroupID, nEPGSchedID, nChannelID, ratioID);
                }
            }
            else
            {
                // use new image server
                picId = DownloadEPGPicToImageServer(sThumb, sName, nGroupID, nChannelID, ratioID, imageTypeId);
            }

            if (picId == 0)
                log.ErrorFormat("Failed download pic- channelID:{0}, ratioId{1}, url:{2}", nChannelID, ratioID, sThumb);
            else
            {
                if (WS_Utils.IsGroupIDContainedInConfig(nGroupID, ApplicationConfiguration.UseOldImageServer.Value, ';'))
                    log.DebugFormat("Successfully download pic- channelID:{0}, ratioId{1}, url:{2}", nChannelID, ratioID, sThumb);
                else
                    log.DebugFormat("Successfully processed image - channelID:{0}, ratioId{1}, url:{2}", nChannelID, ratioID, sThumb);
            }

            return picId;
        }

        public static int DownloadEPGPicToUploader(string sThumb, string sName, int nGroupID, int nEPGSchedID, int nChannelID, int ratioID)
        {
            if (sThumb.Trim() == "")
                return 0;

            string sBasePath = GetBasePath(nGroupID);
            string sPicBaseName1 = getPictureFileName(sThumb);

            int nPicID = 0;
            string picName = string.Format("{0}_{1}_{2}", nChannelID, ratioID, sPicBaseName1);
            nPicID = DoesEPGPicExists(picName, nGroupID);

            if (nPicID == 0)
            {
                string sUploadedFile = "";
                lock (m_sLocker)
                {
                    sUploadedFile = TVinciShared.ImageUtils.DownloadWebImage(sThumb, sBasePath);
                }
                if (sUploadedFile == "")
                    return 0;
                string sUploadedFileExt = "";
                int nExtractPos = sUploadedFile.LastIndexOf(".");
                if (nExtractPos > 0)
                    sUploadedFileExt = sUploadedFile.Substring(nExtractPos);

                string sPicBaseName = TVinciShared.ImageUtils.GetDateImageName();

                List<ImageManager.ImageObj> images = new List<ImageManager.ImageObj>();
                ImageManager.ImageObj tnImage = new ImageManager.ImageObj(sPicBaseName, ImageManager.ImageType.THUMB, 90, 65, sUploadedFileExt);
                ImageManager.ImageObj fullImage = new ImageManager.ImageObj(sPicBaseName, ImageManager.ImageType.FULL, 0, 0, sUploadedFileExt);
                images.Add(tnImage);
                images.Add(fullImage);

                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select * from epg_pics_sizes (nolock) where status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                if (ratioID > 0)
                {
                    selectQuery += " and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ratio_id", "=", ratioID);
                }

                if (selectQuery.Execute("query", true) != null)
                {
                    int nCount = selectQuery.Table("query").DefaultView.Count;

                    for (int nI = 0; nI < nCount; nI++)
                    {

                        int nWidth = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "WIDTH", nI);
                        int nHeight = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "HEIGHT", nI);
                        ImageManager.ImageObj image = new ImageManager.ImageObj(sPicBaseName, ImageManager.ImageType.SIZE, nWidth, nHeight, sUploadedFileExt);
                        images.Add(image);
                    }
                }
                selectQuery.Finish();
                selectQuery = null;

                //chnage the second parameter to BaseURL 
                string sFullImagePath = sBasePath + "/pics/" + sUploadedFile;
                if (sThumb.Contains("http://") || sThumb.Contains("https://"))
                {
                    sFullImagePath = sThumb;
                }
                string sDestImagePath = sBasePath + "/pics/" + nGroupID.ToString();

                bool downloadRes = ImageManager.ImageHelper.DownloadAndCropImage(nGroupID, sFullImagePath, sDestImagePath, images, sPicBaseName, sUploadedFileExt);

                if (downloadRes)
                {
                    foreach (ImageManager.ImageObj image in images)
                    {
                        if (image.eResizeStatus == ImageManager.ResizeStatus.SUCCESS)
                        {
                            UploadQueue.UploadQueueHelper.AddJobToQueue(nGroupID, image.ToString());
                        }
                    }

                    nPicID = InsertNewEPGPic(sName, picName, sPicBaseName + sUploadedFileExt, nGroupID);
                }
            }
            return nPicID;
        }

        public static int DownloadEPGPicToQueue(string sThumb, string sName, int nGroupID, int nEPGSchedID, int nChannelID, int ratioID)
        {
            //string sBasePath = GetBasePath(nGroupID);
            string sBasePath = ImageUtils.getRemotePicsURL(nGroupID);
            string picName = string.Empty;
            int picId = 0;

            GetEpgPicNameAndId(sThumb, nGroupID, nChannelID, ratioID, out picName, out picId);

            if (picId == 0)
            {
                string sUploadedFileExt = ImageUtils.GetFileExt(sThumb);
                string sPicNewName = TVinciShared.ImageUtils.GetDateImageName();
                string[] sPicSizes = getEPGPicSizes(nGroupID, ratioID);

                bool bIsUpdateSucceeded = ImageUtils.SendPictureDataToQueue(sThumb, sPicNewName, sBasePath, sPicSizes, nGroupID);

                picId = InsertNewEPGPic(sName, picName, sPicNewName + sUploadedFileExt, nGroupID);  //insert with sPicName instead of full path
            }
            return picId;
        }

        public static int DownloadEPGPicToImageServer(string thumb, string name, int groupID, int channelID, int ratioID, long imageTypeId, bool isAsync = true,
            int? updaterId = null, string epgIdentifier = null)
        {
            int version = 0;
            string picName = string.Empty;
            int picId = 0;

            //check if thumb Url exist            
            if (!ApplicationConfiguration.CheckImageUrl.Value)
            {
                if (!ImageUtils.IsUrlExists(thumb))
                {
                    log.ErrorFormat("DownloadPicToImageServer thumb Uri not valid: {0} ", thumb);
                    return picId;
                }
            }

            // in case ratio Id = 0 get default group's ratio
            if (ratioID <= 0)
            {
                ratioID = ImageUtils.GetGroupDefaultEpgRatio(groupID);
            }

            //check for epg_image default threshold value
            int pendingThresholdInMinutes = ApplicationConfiguration.EpgImagePendingThresholdInMinutes.IntValue;
            int activeThresholdInMinutes = ApplicationConfiguration.EpgImageActiveThresholdInMinutes.IntValue;

            GetEpgPicNameAndId(thumb, groupID, channelID, ratioID, out picName, out picId);

            if (picId == 0)
            {
                string sPicNewName = TVinciShared.ImageUtils.GetDateImageName();

                picId = CatalogDAL.InsertEPGPic(groupID, name, picName, sPicNewName, imageTypeId);

                if (picId > 0)
                {

                    if (isAsync)
                    {
                        SendImageDataToImageUploadQueue(thumb, groupID, version, picId, sPicNewName, eMediaType.EPG);
                    }
                    else
                    {
                        int parentGroupId = DAL.UtilsDal.GetParentGroupID(groupID);
                        ImageServerUploadRequest imageServerReq = new ImageServerUploadRequest() { GroupId = parentGroupId, Id = sPicNewName, SourcePath = thumb, Version = version };

                        // post image
                        var imageServerUrl = ImageUtils.GetImageServerUrl(groupID, eHttpRequestType.Post);
                        var content = new StringContent(JsonConvert.SerializeObject(imageServerReq), Encoding.UTF8, "application/json");
                        var httpResult = _HttpClient.PostAsync(imageServerUrl, content).GetAwaiter().GetResult();
                        var result = httpResult.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

                        // check result
                        if (string.IsNullOrEmpty(result) || result.ToLower() != "true")
                        {
                            ImageUtils.UpdateImageState(groupID, picId, version, eMediaType.EPG, eTableStatus.Failed, updaterId);
                            picId = 0;
                        }
                        else if (result.ToLower() == "true")
                        {
                            ImageUtils.UpdateImageState(groupID, picId, version, eMediaType.EPG, eTableStatus.OK, updaterId);

                            // Update EpgMultiPictures
                            EpgDal.UpdateEPGMultiPic(groupID, epgIdentifier, channelID, picId, ratioID, updaterId);

                            log.DebugFormat("post image success. picId {0} ", picId);
                        }
                    }
                }
                else
                {
                    log.ErrorFormat("Error while creating new EpgPic, thumb {0}", thumb);
                }
            }
            else
            {
                log.DebugFormat("EpgPic exists, thumb {0}, picId {1}", thumb, picId);
            }

            return picId;
        }

        private static void SendImageDataToImageUploadQueue(string sourcePath, int groupId, int version, int picId, string picNewName, eMediaType mediaType)
        {
            try
            {
                // generate ImageUploadData and send to Queue 
                int parentGroupId = DAL.UtilsDal.GetParentGroupID(groupId);

                // get image server URL
                string imageServerUrl = ImageUtils.GetImageServerUrl(groupId, eHttpRequestType.Post);
                if (string.IsNullOrEmpty(imageServerUrl))
                    throw new Exception(string.Format("IMAGE_SERVER_URL wasn't found. GID: {0}", groupId));

                if (sourcePath.ToLower().Trim().StartsWith("http://") == false &&
                sourcePath.ToLower().Trim().StartsWith("https://") == false)
                {
                    sourcePath = ImageUtils.getRemotePicsURL(groupId) + sourcePath;
                }

                ImageUploadData data = new ImageUploadData(parentGroupId, picNewName, version, sourcePath, picId, imageServerUrl, mediaType);

                var queue = new ImageUploadQueue();

                bool enqueueSuccessful = queue.Enqueue(data, string.Format(ROUTING_KEY_PROCESS_IMAGE_UPLOAD, parentGroupId));

                if (!enqueueSuccessful)
                {
                    log.ErrorFormat("Failed enqueue of image upload. data: {0}", data);
                }
                else
                {
                    log.DebugFormat("successfully enqueue image upload. data: {0}", data);
                }
            }
            catch (Exception exc)
            {
                log.ErrorFormat("Failed image upload: Exception:{0} ", exc);
            }
        }

        private static void GetEpgPicNameAndId(string thumb, int groupID, int channelID, int ratioID, out string picName, out int picId, int pendingThresholdInMinutes = 0, int activeThresholdInMinutes = 0)
        {
            picName = getPictureFileName(thumb);
            picId = 0;
            picName = string.Format("{0}_{1}_{2}", channelID, ratioID, picName);
            picId = CatalogDAL.GetEpgPicsData(groupID, picName);
        }


        private static string getPictureFileName(string sThumb)
        {
            string sPicName = sThumb;

            if (sPicName.IndexOf("?") != -1 && sPicName.IndexOf("uuid") != -1)
            {
                int nStart = sPicName.IndexOf("uuid=", 0) + 5;
                int nEnd = sPicName.IndexOf("&", nStart);
                if (nEnd != 4)
                    sPicName = sPicName.Substring(nStart, nEnd - nStart);
                else
                    sPicName = sPicName.Substring(nStart);
                sPicName += ".jpg";
            }

            if (sPicName.Length >= 200) // the column in DB limit with 255 char
            {
                sPicName = sThumb.Substring(sThumb.Length - 200); // get all 200 chars from the end !!
            }

            return sPicName;
        }

        //Epg Pics will alsays have "full" and "tn". also, all sizes of the group in 'epg_pics_sizes' will be added  
        private static string[] getEPGPicSizes(int nGroupID, int ratioID)
        {
            string[] str;
            List<string> lString = new List<string>();

            lString.Add("full");
            lString.Add("tn");

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from epg_pics_sizes (nolock) where status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            if (ratioID > 0)
            {
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ratio_id", "=", ratioID);
            }
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    int nWidth = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "WIDTH", i);
                    int nHeight = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "HEIGHT", i);
                    string sSize = nWidth + "X" + nHeight;
                    lString.Add(sSize);
                }
            }

            str = lString.ToArray();
            return str;
        }


        static private string GetBasePath(int nGroupID)
        {
            string key = string.Format("pics_base_path_{0}", nGroupID);

            if (!string.IsNullOrEmpty(TVinciShared.WS_Utils.GetTcmConfigValue(key)))
            {
                return TVinciShared.WS_Utils.GetTcmConfigValue(key);
            }
            if (!string.IsNullOrEmpty(ApplicationConfiguration.PicsBasePath.Value))
            {
                return ApplicationConfiguration.PicsBasePath.Value;
            }

            string sBasePath = string.Empty;
            sBasePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            return sBasePath;
        }


        public static int InsertNewEPGPic(string sName, string sRemarks, string sBaseURL, int nGroupID)
        {
            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("EPG_pics");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sName);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sRemarks);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BASE_URL", "=", sBaseURL);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
            insertQuery.Execute();
            insertQuery.Finish();
            insertQuery = null;

            int nRet = 0;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from EPG_pics (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("NAME", "=", sName);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DESCRIPTION", "=", sRemarks);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("BASE_URL", "=", sBaseURL);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }


        //get tags by group and by non group 
        public static int GetTagTypeID(int nGroupID, string sTagName)
        {
            if (sTagName.ToLower().Trim() == "free")
                return 0;

            int nRet = 0;
            string sGroups = TVinciShared.PageUtils.GetParentsGroupsStr(nGroupID);
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select mtt.id from media_tags_types mtt (nolock) where status=1 and ( (TagFamilyID IS NULL and group_id " + sGroups + " ) ";
            selectQuery += " or ( group_id = 0 and TagFamilyID = 1 ) )";
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(NAME)))", "=", sTagName.Trim().ToLower());
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            if (nRet == 0)
            {
                bool bIs_Parent = false;
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select PARENT_GROUP_ID from groups where id = " + nGroupID;
                if (selectQuery.Execute("query", true) != null)
                {
                    bIs_Parent = selectQuery.Table("query").DefaultView[0].Row["PARENT_GROUP_ID"].ToString() == "1" ? true : false;
                }
                selectQuery.Finish();
                selectQuery = null;

                if (bIs_Parent == true)
                {
                    nRet = CheckOnChildNodes(nGroupID, sTagName);
                }
            }
            return nRet;
        }

        public static int CheckOnChildNodes(int nGroupID, string sTagName)
        {
            int nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from groups where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PARENT_GROUP_ID", "=", nGroupID.ToString());
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    for (int i = 0; i < nCount; ++i)
                    {
                        int ChildID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                        nRet = GetTagTypeID(ChildID, sTagName);

                        if (nRet != 0)
                        {
                            break;
                        }
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            return nRet;
        }

    }
}

