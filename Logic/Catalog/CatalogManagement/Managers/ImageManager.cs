using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Tvinci.Core.DAL;
using CachingProvider.LayeredCache;
using ApiObjects;
using Newtonsoft.Json;
using System.Net;
using System.IO;

namespace Core.Catalog.CatalogManagement
{
    public class ImageManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Private Methods

        private static ImageTypeResponse CreateImageTypeResponseFromDataSet(DataSet ds)
        {
            ImageTypeResponse response = new ImageTypeResponse();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                long id = ODBCWrapper.Utils.GetLongSafeVal(ds.Tables[0].Rows[0], "ID");
                if (id > 0)
                {
                    response.ImageType = new ImageType()
                    {
                        Id = id,
                        Name = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "NAME"),
                        SystemName = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "SYSTEM_NAME"),
                        RatioId = ODBCWrapper.Utils.GetLongSafeVal(ds.Tables[0].Rows[0], "RATIO_ID"),
                        HelpText = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "HELP_TEXT"),
                        DefaultImageId = ODBCWrapper.Utils.GetLongSafeVal(ds.Tables[0].Rows[0], "DEFAULT_IMAGE_ID")
                    };
                }
                else
                {
                    response.Status = CreateImageTypeResponseStatusFromResult(id);
                }

                if (response.ImageType != null)
                {
                    response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }

            return response;
        }        

        private static Status CreateImageTypeResponseStatusFromResult(long result)
        {
            Status responseStatus = null;
            switch (result)
            {
                case -222:
                    responseStatus = new Status((int)eResponseStatus.ImageTypeAlreadyInUse, eResponseStatus.ImageTypeAlreadyInUse.ToString());
                    break;
                case -333:
                    responseStatus = new Status((int)eResponseStatus.ImageTypeDoesNotExist, eResponseStatus.ImageTypeDoesNotExist.ToString());
                    break;
                default:
                    responseStatus = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                    break;
            }

            return responseStatus;
        }

        private static List<ImageType> GetGroupImageTypes(int groupId)
        {
            List<ImageType> result = null;

            // check if image types exists for group
            string key = LayeredCacheKeys.GetGroupImageTypesKey(groupId);

            // try to get from cache  

            bool cacheResult = LayeredCache.Instance.Get<List<ImageType>>(
                key, ref result, GetImageType, new Dictionary<string, object>() { { "groupId", groupId } },
                groupId, LayeredCacheConfigNames.GET_IMAGE_TYPE_CACHE_CONFIG_NAME, new List<string>() { LayeredCacheKeys.GetGroupImageTypesInvalidationKey(groupId) });

            if (!cacheResult)
            {
                log.Error(string.Format("GetImageTypes - Failed get data from cache groupId = {0}", groupId));
                result = null;
            }

            return result;
        }

        private static Tuple<List<ImageType>, bool> GetImageType(Dictionary<string, object> funcParams)
        {
            bool res = false;
            List<ImageType> imageTypes = null;
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    if (groupId.HasValue && groupId.Value > 0)
                    {
                        DataSet ds = CatalogDAL.GetImageTypes(groupId.Value);
                        imageTypes = CreateImageTypes(ds);

                        res = imageTypes != null;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetImageType failed params : {0}", funcParams != null ? string.Join(";",
                         funcParams.Select(x => string.Format("key:{0}, value: {1}", x.Key, x.Value.ToString())).ToList()) : string.Empty), ex);
            }

            return new Tuple<List<ImageType>, bool>(imageTypes, res);
        }

        private static List<ImageType> CreateImageTypes(DataSet ds)
        {
            List<ImageType> response = null;
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable imageTypes = ds.Tables[0];
                if (imageTypes != null && imageTypes.Rows != null)
                {
                    response = new List<ImageType>();
                    foreach (DataRow dr in imageTypes.Rows)
                    {
                        ImageType imageType = new ImageType()
                        {
                            Id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID"),
                            Name = ODBCWrapper.Utils.GetSafeStr(dr, "NAME"),
                            SystemName = ODBCWrapper.Utils.GetSafeStr(dr, "SYSTEM_NAME"),
                            RatioId = ODBCWrapper.Utils.GetLongSafeVal(dr, "RATIO_ID"),
                            HelpText = ODBCWrapper.Utils.GetSafeStr(dr, "HELP_TEXT"),
                            DefaultImageId = ODBCWrapper.Utils.GetLongSafeVal(dr, "DEFAULT_IMAGE_ID")
                        };

                        response.Add(imageType);
                    }
                }
            }

            return response;
        }

        private static List<Core.Catalog.CatalogManagement.Ratio> GetImageRatios(int groupId)
        {
            List<Core.Catalog.CatalogManagement.Ratio> result = null;

            // check if image types exists for group
            string key = LayeredCacheKeys.GetGroupRatiosKey(groupId);

            // try to get from cache  

            bool cacheResult = LayeredCache.Instance.Get<List<Core.Catalog.CatalogManagement.Ratio>>(key, ref result, GetRatios, new Dictionary<string, object>() { { "groupId", groupId } },
                groupId, LayeredCacheConfigNames.GET_RATIOS_CACHE_CONFIG_NAME, new List<string>() { LayeredCacheKeys.GetGroupRatiosInvalidationKey(groupId) } );

            if (!cacheResult)
            {
                log.Error(string.Format("GetGroupRatios - Failed get data from cache groupId = {0}", groupId));
                result = null;
            }

            return result;
        }

        private static Tuple<List<Core.Catalog.CatalogManagement.Ratio>, bool> GetRatios(Dictionary<string, object> funcParams)
        {
            bool res = false;
            List<Core.Catalog.CatalogManagement.Ratio> ratios = null;
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    if (groupId.HasValue && groupId.Value > 0)
                    {
                        DataTable dt = CatalogDAL.GetGroupImageRatios(groupId.Value);
                        ratios = CreateRatios(dt);

                        res = ratios != null;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("GetRatios failed", ex);
            }

            return new Tuple<List<Core.Catalog.CatalogManagement.Ratio>, bool>(ratios, res);
        }

        private static List<Core.Catalog.CatalogManagement.Ratio> CreateRatios(DataTable dt)
        {
            List<Core.Catalog.CatalogManagement.Ratio> response = null;
            if (dt != null && dt.Rows != null)
            {
                response = new List<Core.Catalog.CatalogManagement.Ratio>();
                foreach (DataRow dr in dt.Rows)
                {
                    Core.Catalog.CatalogManagement.Ratio ratio = new Core.Catalog.CatalogManagement.Ratio()
                    {
                        Id = ODBCWrapper.Utils.GetLongSafeVal(dr, "id"),
                        Name = ODBCWrapper.Utils.GetSafeStr(dr, "name"),
                        Height = ODBCWrapper.Utils.GetIntSafeVal(dr, "height"),
                        Width = ODBCWrapper.Utils.GetIntSafeVal(dr, "width"),
                        AcceptedErrorMarginPrecentage = ODBCWrapper.Utils.GetIntSafeVal(dr, "accepted_error_margin_percentage")
                    };

                    response.Add(ratio);
                }
            }

            return response;
        }        

        private static Image CreateImageFromDataRow(int groupId, DataRow row, long id = 0)
        {
            Image image = null;
            long imageId = id == 0 ? ODBCWrapper.Utils.GetLongSafeVal(row, "ID") : id;
            if (imageId > 0)
            {
                image = new Image()
                {
                    Id = imageId,
                    ContentId = ODBCWrapper.Utils.GetSafeStr(row, "BASE_URL"),
                    ImageObjectId = ODBCWrapper.Utils.GetLongSafeVal(row, "ASSET_ID"),
                    ImageObjectType = (eAssetImageType)ODBCWrapper.Utils.GetIntSafeVal(row, "ASSET_IMAGE_TYPE"),
                    Status = (eTableStatus)ODBCWrapper.Utils.GetIntSafeVal(row, "STATUS"),
                    Version = ODBCWrapper.Utils.GetIntSafeVal(row, "VERSION"),
                    ImageTypeId = ODBCWrapper.Utils.GetLongSafeVal(row, "IMAGE_TYPE_ID"),
                    IsDefault = ODBCWrapper.Utils.GetIntSafeVal(row, "IS_DEFAULT", 0) > 0 ? true : false
                };

                image.Url = TVinciShared.ImageUtils.BuildImageUrl(groupId, image.ContentId, image.Version, 0, 0, 0, true);
            }
            

            return image;
        }

        private static Ratio GetRatioById(int groupId, long ratioId)
        {
            Ratio ratio = null;
            List<Ratio> groupRatios = GetImageRatios(groupId);
            if (groupRatios != null && groupRatios.Count > 0)
            {
                ratio = groupRatios.Where(x => x.Id == ratioId).Count() == 1 ? groupRatios.Where(x => x.Id == ratioId).FirstOrDefault() : null;
            }

            return ratio;
        }

        private static ImageType GetImageType(int groupId, long imageTypeId)
        {
            ImageType imageType = null;
            ImageTypeListResponse imageTypeResponse = GetImageTypes(groupId, true, new List<long>() { imageTypeId });
            if (imageTypeResponse != null && imageTypeResponse.Status != null && imageTypeResponse.Status.Code == (int)eResponseStatus.OK && imageTypeResponse.ImageTypes.Count == 1)
            {
                imageType = imageTypeResponse.ImageTypes[0];
            }

            return imageType;
        }

        // for backward compatibility
        private static string GetRatioName(int groupId, long ratioId)
        {
            string ratioName = string.Empty;
            if (ratioId > 0)
            {
                Ratio ratio = GetRatioById(groupId, ratioId);
                if (ratio != null)
                {
                    ratioName = ratio.Name;
                }
            }

            return ratioName;
        }        

        private static Tuple<List<Image>, bool> GetGroupDefaultImages(Dictionary<string, object> funcParams)
        {
            bool res = false;
            List<Image> groupDefaultImages = null;
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    if (groupId.HasValue && groupId.Value > 0)
                    {
                        List<ImageType> imageTypes = GetGroupImageTypes(groupId.Value);
                        if (imageTypes != null)
                        {
                            List<long> imageTypesWithDefaultPic = imageTypes.Where(x => x.DefaultImageId.HasValue && x.DefaultImageId.Value > 0).Select(x => x.DefaultImageId.Value).ToList();
                            if (imageTypesWithDefaultPic != null && imageTypesWithDefaultPic.Count > 0)
                            {
                                groupDefaultImages = new List<Image>();
                                ImageListResponse defaultImagesResponse = GetImagesByIds(groupId.Value, imageTypesWithDefaultPic, true);
                                if (defaultImagesResponse != null && defaultImagesResponse.Status != null && defaultImagesResponse.Status.Code == (int)eResponseStatus.OK)
                                {
                                    groupDefaultImages.AddRange(defaultImagesResponse.Images);
                                }
                            }
                        }                                                

                        res = groupDefaultImages != null;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetGroupDefaultImages failed params : {0}", funcParams != null ? string.Join(";",
                         funcParams.Select(x => string.Format("key:{0}, value: {1}", x.Key, x.Value.ToString())).ToList()) : string.Empty), ex);
            }

            return new Tuple<List<Image>, bool>(groupDefaultImages, res);
        }

        private static eAssetTypes ConverteAssetImageTypeToeAssetTypes(eAssetImageType assetImageType)
        {
            eAssetTypes result = eAssetTypes.UNKNOWN;
            switch (assetImageType)
            {
                case eAssetImageType.Media:
                    result = eAssetTypes.MEDIA;
                    break;
                case eAssetImageType.Channel:
                    break;
                case eAssetImageType.Category:
                    break;
                case eAssetImageType.DefaultPic:
                    break;
                case eAssetImageType.LogoPic:
                    break;
                case eAssetImageType.ImageType:
                    break;
                default:
                    break;
            }

            return result;
        }

        #endregion

        #region Internal

        internal static ImageListResponse CreateImageListResponseFromDataTable(int groupId, DataTable dt)
        {
            ImageListResponse response = new ImageListResponse();
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                response.Images = new List<Image>();

                foreach (DataRow row in dt.Rows)
                {
                    Image image = CreateImageFromDataRow(groupId, row);
                    if (image != null)
                    {
                        response.Images.Add(image);
                    }
                }
            }
            response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            return response;
        }

        internal static List<Image> GetGroupDefaultImages(int groupId)
        {
            List<Image> result = null;
            string key = LayeredCacheKeys.GetGroupDefaultImagesKey(groupId);

            // try to get from cache  
            bool cacheResult = LayeredCache.Instance.Get<List<Image>>(key, ref result, GetGroupDefaultImages, new Dictionary<string, object>() { { "groupId", groupId } },
                groupId, LayeredCacheConfigNames.GET_GROUP_DEFAULT_IMAGES_CACHE_CONFIG_NAME, new List<string>() { LayeredCacheKeys.GetGroupDefaultImagesInvalidationKey(groupId) });

            if (!cacheResult)
            {
                log.Error(string.Format("GetGroupDefaultImages - Failed get data from cache groupId = {0}", groupId));
                result = null;
            }

            return result;
        }

        #endregion

        #region Public Methods

        public static ImageTypeResponse AddImageType(int groupId, ImageType imageTypeToAdd, long userId)
        {
            ImageTypeResponse result = new ImageTypeResponse();
            try
            {
                DataSet ds = CatalogDAL.InsertImageType(groupId, imageTypeToAdd.Name, imageTypeToAdd.SystemName, imageTypeToAdd.RatioId.Value, imageTypeToAdd.HelpText,
                                                      userId, imageTypeToAdd.DefaultImageId);
                if (ds != null)
                {
                    result = CreateImageTypeResponseFromDataSet(ds);

                    string invalidationKey = LayeredCacheKeys.GetGroupImageTypesInvalidationKey(groupId);
                    if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    {
                        log.ErrorFormat("Failed to set invalidation key on AddImageType key = {0}", invalidationKey);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed AddImageType for groupId: {0} and imageType: {1}", groupId, JsonConvert.SerializeObject(imageTypeToAdd)), ex);
            }

            return result;
        }

        public static Status DeleteImageType(int groupId, long id, long userId)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                //check if exist before delete
                ImageTypeListResponse imageTypeListResponse = GetImageTypes(groupId, true, new List<long>(new long[] { id }));
                if (imageTypeListResponse != null && imageTypeListResponse.ImageTypes != null && imageTypeListResponse.ImageTypes.Count == 0)
                {
                    result = new Status() { Code = (int)eResponseStatus.ImageTypeDoesNotExist, Message = eResponseStatus.ImageTypeDoesNotExist.ToString() };
                    return result;
                }

                if (CatalogDAL.DeleteImageType(groupId, id, userId))
                {
                    string invalidationKey = LayeredCacheKeys.GetGroupImageTypesInvalidationKey(groupId);
                    if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    {
                        log.ErrorFormat("Failed to set invalidation key on DeleteImageType key = {0}", invalidationKey);
                    }

                    ImageType imageType = imageTypeListResponse.ImageTypes.First();
                    if (imageType != null && imageType.DefaultImageId.HasValue && imageType.DefaultImageId.Value > 0)
                    {
                        string defaultGroupImagesInvalidationKey = LayeredCacheKeys.GetGroupDefaultImagesInvalidationKey(groupId);
                        if (!LayeredCache.Instance.SetInvalidationKey(defaultGroupImagesInvalidationKey))
                        {
                            log.ErrorFormat("Failed to set invalidation key on DeleteImageType key = {0}", defaultGroupImagesInvalidationKey);
                        }
                    }

                    result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed DeleteImageType for groupId: {0} and ImageTypeId: {1}", groupId, id), ex);
            }

            return result;
        }

        public static ImageTypeResponse UpdateImageType(int groupId, long id, ImageType imageTypeToUpdate, long userId)
        {
            ImageTypeResponse result = new ImageTypeResponse();
            try
            {
                ImageTypeListResponse imageTypeListResponse = GetImageTypes(groupId, true, null);
                if (imageTypeListResponse == null || (imageTypeListResponse != null && imageTypeListResponse.ImageTypes == null) || (imageTypeListResponse.ImageTypes.Count == 0))
                {
                    result.Status = new Status() { Code = (int)eResponseStatus.ImageTypeDoesNotExist, Message = eResponseStatus.ImageTypeDoesNotExist.ToString() };
                    return result;
                }

                ImageType cachedImageType = imageTypeListResponse.ImageTypes.Where(x => x.Id == id).FirstOrDefault();
                if (cachedImageType == null)
                {
                    result.Status = new Status() { Code = (int)eResponseStatus.ImageTypeDoesNotExist, Message = eResponseStatus.ImageTypeDoesNotExist.ToString() };
                    return result;
                }

                cachedImageType = imageTypeListResponse.ImageTypes.Where(x => x.SystemName == imageTypeToUpdate.SystemName && x.Id != id).FirstOrDefault();
                if (cachedImageType != null)
                {
                    result.Status = new Status() { Code = (int)eResponseStatus.ImageTypeAlreadyInUse, Message = eResponseStatus.ImageTypeAlreadyInUse.ToString() };
                    return result;
                }

                DataSet ds = CatalogDAL.UpdateImageType(groupId, id, imageTypeToUpdate.Name, imageTypeToUpdate.SystemName, imageTypeToUpdate.RatioId,
                    imageTypeToUpdate.HelpText, userId, imageTypeToUpdate.DefaultImageId);
                if (ds != null)
                {
                    result = CreateImageTypeResponseFromDataSet(ds);
                    string invalidationKey = LayeredCacheKeys.GetGroupImageTypesInvalidationKey(groupId);
                    if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    {
                        log.ErrorFormat("Failed to set invalidation key on UpdateImageType key = {0}", invalidationKey);
                    }

                    if (result != null && result.ImageType != null && result.ImageType.DefaultImageId.HasValue && result.ImageType.DefaultImageId.Value > 0)
                    {
                        string defaultGroupImagesInvalidationKey = LayeredCacheKeys.GetGroupDefaultImagesInvalidationKey(groupId);
                        if (!LayeredCache.Instance.SetInvalidationKey(defaultGroupImagesInvalidationKey))
                        {
                            log.ErrorFormat("Failed to set invalidation key on UpdateImageType key = {0}", defaultGroupImagesInvalidationKey);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed UpdateImageType for groupId: {0}, id: {1} and ImageType: {2}", groupId, id, JsonConvert.SerializeObject(imageTypeToUpdate)), ex);
            }

            return result;
        }

        public static ImageTypeListResponse GetImageTypes(int groupId, bool isSearchByIds, List<long> ids)
        {
            ImageTypeListResponse response = new ImageTypeListResponse() { Status = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() } };

            List<ImageType> imageTypes = GetGroupImageTypes(groupId);

            if (imageTypes != null)
            {
                response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                response.TotalItems = imageTypes.Count;

                if (ids == null || ids.Count == 0)
                {
                    response.ImageTypes = imageTypes;
                    return response;
                }

                if (isSearchByIds)
                {
                    // return image Types according to Ids
                    response.ImageTypes = imageTypes.Where(x => ids.Contains(x.Id)).ToList();
                }
                else
                {
                    // return image Types according to ratio Ids
                    response.ImageTypes = imageTypes.Where(x => ids.Contains(x.RatioId.Value)).ToList();
                }

                response.TotalItems = response.ImageTypes.Count;
            }

            return response;
        }

        public static RatioListResponse GetRatios(int groupId)
        {
            RatioListResponse response = new RatioListResponse();

            response.Ratios = GetImageRatios(groupId);

            if (response.Ratios != null)
            {
                response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                response.TotalItems = response.Ratios.Count;
            }

            return response;
        }

        public static Status DeleteImage(int groupId, long id, long userId)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                //check if exist before delete
                ImageListResponse imagesResponse = GetImagesByIds(groupId, new List<long>(new long[] { id }));
                if (imagesResponse != null && imagesResponse.Images != null && imagesResponse.Images.Count == 0)
                {
                    result = new Status((int)eResponseStatus.ImageDoesNotExist, "Image does not exist");
                    return result;
                }

                if (CatalogDAL.DeletePic(groupId, id, userId))
                {
                    result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    Image image = imagesResponse.Images.First();
                    if (image != null && image.IsDefault.HasValue && image.IsDefault.Value)
                    {
                        string invalidationKey = LayeredCacheKeys.GetGroupDefaultImagesInvalidationKey(groupId);
                        if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                        {
                            log.ErrorFormat("Failed to set invalidation key on DeleteImage key = {0}", invalidationKey);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed DeleteImage for groupId: {0} and ImageId: {1}", groupId, id), ex);
            }

            return result;
        }

        public static ImageListResponse GetImagesByIds(int groupId, List<long> imageIds, bool? isDefault = null)
        {
            ImageListResponse response = new ImageListResponse();

            DataTable dt = CatalogDAL.GetImagesByIds(groupId, imageIds, isDefault);
            response = CreateImageListResponseFromDataTable(groupId, dt);
            return response;
        }

        public static ImageListResponse GetImagesByObject(int groupId, long imageObjectId, eAssetImageType imageObjectType, bool? isDefault = null)
        {
            ImageListResponse response = new ImageListResponse();

            DataTable dt = CatalogDAL.GetImagesByObject(groupId, imageObjectId, imageObjectType);
            response = CreateImageListResponseFromDataTable(groupId, dt);
            if (isDefault.HasValue && isDefault.Value)
            {
                List<Image> groupDefualtImages = GetGroupDefaultImages(groupId);
                response.Images.AddRange(groupDefualtImages);
            }

            return response;
        }

        public static ImageResponse AddImage(int groupId, Image imageToAdd, long userId)
        {
            ImageResponse result = new ImageResponse();
            try
            {
                if (imageToAdd.ImageObjectType == eAssetImageType.Media && imageToAdd.ImageObjectId > 0)
                {
                    AssetResponse asset = AssetManager.GetAsset(groupId, imageToAdd.ImageObjectId, eAssetTypes.MEDIA);
                    if (asset.Status.Code != (int)eResponseStatus.OK)
                    {
                        log.ErrorFormat("Asset not found. assetId = {0}, assetType = {2}", imageToAdd.ImageObjectId, imageToAdd.ImageObjectType);
                        result.Status = new Status(asset.Status.Code, "Asset not found");
                        return result;
                    }
                }

                ImageType imageType = GetImageType(groupId, imageToAdd.ImageTypeId);
                if (imageType == null)
                {
                    result.Status = new Status((int)eResponseStatus.ImageTypeDoesNotExist, eResponseStatus.ImageTypeDoesNotExist.ToString());
                    return result;
                }

                DataTable dt = CatalogDAL.InsertPic(groupId, userId, imageToAdd.ImageObjectId, imageToAdd.ImageObjectType, imageToAdd.ImageTypeId);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    long id = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "ID");
                    if (id > 0)
                    {
                        result.Image = CreateImageFromDataRow(groupId, dt.Rows[0], id);

                        if (result.Image != null)
                        {
                            if (imageToAdd.ImageObjectType == eAssetImageType.ImageType)
                            {
                                // update default image ID in image type
                                ImageTypeResponse imageTypeResult = UpdateImageType(groupId, result.Image.ImageTypeId,
                                    new ImageType() { DefaultImageId = result.Image.Id }, userId);

                                result.Status = imageTypeResult.Status;
                            }
                            else
                            {
                                result.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                            }
                        }
                    }
                    else
                    {
                        result.Status = new Status((int)eResponseStatus.ImageTypeAlreadyInUse, "Image type already in use for object");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed AddImage for groupId: {0} and imageType: {1}", groupId, JsonConvert.SerializeObject(imageToAdd)), ex);
            }

            return result;
        }

        public static Status SetContent(int groupId, long userId, long id, string url)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                //check if exist before setting content
                ImageListResponse imagesResponse = GetImagesByIds(groupId, new List<long>(new long[] { id }));
                if (imagesResponse != null && imagesResponse.Images != null && imagesResponse.Images.Count == 0)
                {
                    result = new Status((int)eResponseStatus.ImageDoesNotExist, "Image does not exist");
                    return result;
                }

                Image image = imagesResponse.Images[0];
                if (string.IsNullOrEmpty(image.ContentId))
                {
                    //first setContent
                    image.ContentId = TVinciShared.ImageUtils.GetDateImageName();
                }
                else
                {
                    image.Version++;
                }

                // validate image ratio
                ImageType imageType = GetImageType(groupId, image.ImageTypeId);                
                if (imageType == null)
                {
                    result = new Status((int)eResponseStatus.ImageTypeDoesNotExist, eResponseStatus.ImageTypeDoesNotExist.ToString());
                    return result;
                }                
                else if (imageType.RatioId.HasValue && imageType.RatioId.Value > 0)
                {
                    Ratio ratio = GetRatioById(groupId, imageType.RatioId.Value);
                    if (ratio != null && ratio.AcceptedErrorMarginPrecentage > 0)
                    {
                        try
                        {
                            using (WebClient webClient = new WebClient())
                            {
                                byte[] imageBytes = webClient.DownloadData(url);                                
                                MemoryStream imageStream = new MemoryStream(imageBytes);
                                System.Drawing.Image downloadedImage = System.Drawing.Image.FromStream(imageStream);
                                double downloadedImageRatio = (double)downloadedImage.Width / downloadedImage.Height;
                                double imageDefinedRatio = (double)ratio.Width / ratio.Height;
                                double imageRatioDif = Math.Round(Math.Abs(downloadedImageRatio - imageDefinedRatio) * 100);
                                if (ratio.AcceptedErrorMarginPrecentage < imageRatioDif)
                                {
                                    result = new Status((int)eResponseStatus.InvalidRatioForImage, eResponseStatus.InvalidRatioForImage.ToString());
                                    return result;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error(string.Format("Failed to validate image ratio, groupId: {0}, imageId: {1}, url: {2}", groupId, id, url), ex);
                        }
                    }
                }

                // post image
                ImageServerUploadRequest imageServerReq = new ImageServerUploadRequest() { GroupId = groupId, Id = image.ContentId, SourcePath = url, Version = image.Version };
                string res = TvinciImporter.Utils.HttpPost(TVinciShared.ImageUtils.GetImageServerUrl(groupId, eHttpRequestType.Post), JsonConvert.SerializeObject(imageServerReq), "application/json");

                // check result
                if (string.IsNullOrEmpty(res) || res.ToLower() != "true")
                {
                    log.ErrorFormat("POST to image server failed. imageId = {0}, contentId = {1}", image.Id, image.ContentId);
                    TVinciShared.ImageUtils.UpdateImageState(groupId, image.Id, image.Version, eMediaType.VOD, eTableStatus.Failed, (int)userId);
                    return result;
                }
                else if (res.ToLower() == "true")
                {
                    log.DebugFormat("POST to image server successfully sent. imageId = {0}, contentId = {1}", image.Id, image.ContentId);
                    TVinciShared.ImageUtils.UpdateImageState(groupId, image.Id, image.Version, eMediaType.VOD, eTableStatus.OK, (int)userId, image.ContentId);
                    result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

                    eAssetTypes assetType = ConverteAssetImageTypeToeAssetTypes(image.ImageObjectType);
                    // invalidate asset
                    string invalidationKey = LayeredCacheKeys.GetAssetInvalidationKey(assetType.ToString(), image.ImageObjectId);
                    if (assetType != eAssetTypes.UNKNOWN && !LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    {
                        log.ErrorFormat("Failed to invalidate asset with id: {0}, assetType: {1}, invalidationKey: {2} after SetContent",
                                            image.ImageObjectId, assetType.ToString(), invalidationKey);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed SetContent for groupId: {0} and ImageId: {1}", groupId, id), ex);
            }

            return result;
        }

        public static RatioResponse AddRatio(int groupId, long userId, Core.Catalog.CatalogManagement.Ratio ratio)
        {
            RatioResponse result = new RatioResponse();
            try
            {
                DataTable dt = CatalogDAL.InsertGroupImageRatios(groupId, userId, ratio.Name, ratio.Height, ratio.Width,
                                                                    ratio.AcceptedErrorMarginPrecentage);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    long id = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "ID");

                    if (id > 0)
                    {
                        result.Ratio = new Core.Catalog.CatalogManagement.Ratio()
                        {
                            Id = id,
                            Height = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "height"),
                            Width= ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "width"),
                            AcceptedErrorMarginPrecentage = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "accepted_error_margin_percentage"),
                            Name = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "NAME")
                        };

                        string invalidationKey = LayeredCacheKeys.GetGroupRatiosInvalidationKey(groupId);
                        if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                        {
                            log.ErrorFormat("Failed to set invalidation key on AddRatio key = {0}", invalidationKey);
                        }
                        result.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }
                    else
                    {
                        result.Status = new Status((int)eResponseStatus.RatioAlreadyExist, "Ratio Already Exist");
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed AddRatio for groupId: {0} and ratio: {1}", groupId, JsonConvert.SerializeObject(ratio)), ex);
            }

            return result;
        }

        public static List<Picture> ConvertImagesToPictures(List<Image> assetImages, int groupId)
        {
            List<Picture> pictures = new List<Picture>();
            if (assetImages != null && assetImages.Count > 0)
            {
                foreach (Image image in assetImages)
                {
                    ImageType imageType = ImageManager.GetImageType(groupId, image.ImageTypeId);
                    if (imageType != null && imageType.RatioId.HasValue && imageType.RatioId.Value > 0)
                    {
                        pictures.Add(new Picture(image, imageType.Name, ImageManager.GetRatioName(groupId, imageType.RatioId.Value)));
                    }
                }
            }

            return pictures;
        }

        // for backward compatibility
        public static string GetRatioNameByImageTypeId(int groupId, long imageTypeId)
        {
            string result = string.Empty;
            ImageType imageType = ImageManager.GetImageType(groupId, imageTypeId);
            if (imageType != null && imageType.RatioId.HasValue && imageType.RatioId.Value > 0)
            {
                result = ImageManager.GetRatioName(groupId, imageType.RatioId.Value);
            }

            return result;
        }

        #endregion
    }
}
