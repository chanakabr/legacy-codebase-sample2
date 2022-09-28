using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.DRM;
using ApiObjects.Epg;
using ApiObjects.Notification;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Phx.Lib.Appconfig;
using DAL;
using Phx.Lib.Log;

using Newtonsoft.Json;
using QueueWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using Tvinci.Core.DAL;
using TvinciImporter;
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

        public static async Task<IEnumerable<GenericResponse<EpgPicture>>> UploadEPGPictures(int groupID, IList<EpgPicture> pics)
        {
            var groupDefaultEpgRatio = ImageUtils.GetGroupDefaultEpgRatio(groupID);
            var results = pics.Select(p => new GenericResponse<EpgPicture>(Status.Ok, p)).ToList();
            ValidatePicturesHasUrl(results);
            var validPicturesToUpload = results.Where(p => p.IsOkStatusCode()).ToList();

            await CheckPictureUrlWithHeadRequest(validPicturesToUpload);
            validPicturesToUpload = results.Where(p => p.IsOkStatusCode()).ToList();

            foreach (var pic in validPicturesToUpload)
            {
                if (pic.Object.RatioId <= 0) { pic.Object.RatioId = groupDefaultEpgRatio; }
                var picName = getPictureFileName(pic.Object.Url);
                picName = string.Format("{0}_{1}_{2}", pic.Object.ChannelId, pic.Object.RatioId, picName);
                pic.Object.PicName = picName;
            }

            GetExistingPictures(groupID, validPicturesToUpload);
            var picsToInsert = validPicturesToUpload.Where(p => p.Object.PicID <= 0);

            InsertNewPicturesToDB(groupID, picsToInsert.ToList());

            return results;
        }

        private static void InsertNewPicturesToDB(int groupID, List<GenericResponse<EpgPicture>> picsToInsert)
        {
            var picIdPerEpgPictureCache = new Dictionary<string, int>();
            foreach (var picResult in picsToInsert)
            {
                var pic = picResult.Object;
                string epgPicKey = pic.Url+pic.PicName+ groupID+ 0+ pic.ChannelId+ pic.RatioId+ pic.ImageTypeId;
                if (picIdPerEpgPictureCache.ContainsKey(epgPicKey))
                {
                    pic.PicID = picIdPerEpgPictureCache[epgPicKey];
                }
                else
                {
                    pic.PicID = ImporterImpl.DownloadEPGPic(pic.Url, pic.PicName, groupID, 0, pic.ChannelId, pic.RatioId, pic.ImageTypeId);
                    picIdPerEpgPictureCache[epgPicKey] = pic.PicID;
                }
            }

            var EpgUrlPerId = EpgIngest.Utils.GetEpgPicsBaseUrls(groupID, picsToInsert.Where(x => x.Object != null).Select(x => x.Object.PicID).Distinct().ToList());

            foreach (var picData in EpgUrlPerId)
            {
                var allPicsById = picsToInsert.Where(x => x.Object != null && x.Object.PicID == picData.Key).Select(x=>x.Object).ToList();
                if (allPicsById.Any())
                {
                    foreach (var pic in allPicsById)
                    {
                        pic.BaseUrl = picData.Value;
                        pic.Url = picData.Value;
                    }
                }
            }

            
            
        }

        private static void GetExistingPictures(int groupID, IEnumerable<GenericResponse<EpgPicture>> validPicturesToUpload)
        {
            var existingPicsDict = GetExistingPicIds(groupID, validPicturesToUpload);
            foreach (var existingPic in existingPicsDict)
            {
                var existingPics = validPicturesToUpload.Where(p => p.Object.PicName == existingPic.Key);
                foreach (var pic in existingPics)
                {
                    pic.Object.PicID = existingPic.Value.Id;
                    pic.Object.Url = existingPic.Value.BaseUrl;
                }
            }
        }

        private static async Task CheckPictureUrlWithHeadRequest(IEnumerable<GenericResponse<EpgPicture>> validPicturesToUpload)
        {
            if (!ApplicationConfiguration.Current.CheckImageUrl.Value)
            {
                var originalMaxConcurrentConnections = ServicePointManager.DefaultConnectionLimit;
                ServicePointManager.DefaultConnectionLimit = 10; // TODO: Arthur - Take from TCM ? 
                var imageServerRequests = validPicturesToUpload.Select(pic =>
                {
                    var msg = new HttpRequestMessage(HttpMethod.Head, pic.Object.Url);
                    var request = _HttpClient.SendAsync(msg);
                    request.ContinueWith(async requestTask =>
                    {
                        var response = await requestTask;
                        log.Debug($"Img validation status:[{response.StatusCode}] url HEAD url:[{pic.Object.Url}]");
                        if (!response.IsSuccessStatusCode)
                        {
                            pic.SetStatus(eResponseStatus.InvalidUrlForImage);
                            log.Error($"UploadEPGPictures Uri HEAD check failed with code:[{response.StatusCode}], pic:[{pic}]");
                        }
                    });
                    return request;
                });

                await Task.WhenAll(imageServerRequests);
                ServicePointManager.DefaultConnectionLimit = originalMaxConcurrentConnections;
            }
        }

        private static void ValidatePicturesHasUrl(IList<GenericResponse<EpgPicture>> results)
        {
            if (results.Any(p => string.IsNullOrEmpty(p.Object.Url)))
            {
                var picsResultsWithoutUrl = results.Where(p => string.IsNullOrEmpty(p.Object.Url)).ToList();
                log.Error($"Some picture were sent without Url. pics:[{string.Join(",", picsResultsWithoutUrl.Select(r => r.Object))}], setting their Id to 0");
                foreach (var picResult in picsResultsWithoutUrl)
                {
                    picResult.SetStatus(eResponseStatus.ImageUrlRequired);
                }
            }
        }

        private static Dictionary<string, EPGPicDTO> GetExistingPicIds(int groupID, IEnumerable<GenericResponse<EpgPicture>> validPicturesToUpload)
        {

            var picNames = validPicturesToUpload.Select(p => p.Object.PicName).ToList();
            var existingPicsTbl = CatalogDAL.GetEpgPicturesByName(groupID, picNames);
            var existingPicsDict = new Dictionary<string, EPGPicDTO>();

            foreach (DataRow row in existingPicsTbl.Rows)
            {
                var pic = new EPGPicDTO();
                pic.Id = ODBCWrapper.Utils.GetIntSafeVal(row, "ID");
                pic.PicName   = ODBCWrapper.Utils.GetSafeStr(row, "DESCRIPTION");
                pic.BaseUrl = ODBCWrapper.Utils.GetSafeStr(row, "BASE_URL");
                existingPicsDict[pic.PicName] = pic;
            }

            return existingPicsDict;
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

    public class EPGPicDTO
    {
        public int Id { get; set; }
        public string PicName { get; set; }
        public string BaseUrl { get; set; }
    }
}

