using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Epg;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Phx.Lib.Log;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using ElasticSearch.Utilities;
using Tvinci.Core.DAL;
using ApiObjects.Base;

namespace Core.Catalog.CatalogManagement
{
    public interface IImageManager
    {
        GenericListResponse<Image> GetImagesByObject(int groupId, long imageObjectId, eAssetImageType imageObjectType, bool? isDefault = null);

        GenericResponse<Image> AddImage(int groupId, Image imageToAdd, long userId);

        Status SetContent(int groupId, long userId, long id, string url);

        Status DeleteImage(int groupId, long id, long userId);
    }

    public class ImageManager : IImageManager
    {
        private readonly ITtlService _ttlService;
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<ImageManager> lazy = new Lazy<ImageManager>(() => new ImageManager(TtlService.Instance), LazyThreadSafetyMode.PublicationOnly);

        public static ImageManager Instance { get { return lazy.Value; } }

        public ImageManager(ITtlService ttlService)
        {
            _ttlService = ttlService;
        }

        #region Private Methods

        private static GenericResponse<ApiObjects.Catalog.ImageType> CreateImageTypeResponseFromDataSet(DataSet ds)
        {
            GenericResponse<ImageType> response = new GenericResponse<ImageType>();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                long id = ODBCWrapper.Utils.GetLongSafeVal(ds.Tables[0].Rows[0], "ID");
                if (id > 0)
                {
                    response.Object = new ImageType()
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
                    response.SetStatus(CreateImageTypeResponseStatusFromResult(id));
                }

                if (response.Object != null)
                {
                    response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
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
            List<ImageType> tempResult = null;
            bool cacheResult = LayeredCache.Instance.Get<List<ImageType>>(
                key, ref tempResult, GetImageType, new Dictionary<string, object>() { { "groupId", groupId } },
                groupId, LayeredCacheConfigNames.GET_IMAGE_TYPE_CACHE_CONFIG_NAME, new List<string>() { LayeredCacheKeys.GetGroupImageTypesInvalidationKey(groupId) });

            if (!cacheResult)
            {
                log.Error(string.Format("GetImageTypes - Failed get data from cache groupId = {0}", groupId));
                result = null;
            }
            else
            {
                result = new List<ImageType>(tempResult);
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

        private static List<Ratio> GetGroupImageRatios(int groupId)
        {
            List<Ratio> result = null;

            // check if image types exists for group
            string key = LayeredCacheKeys.GetGroupRatiosKey(groupId);

            // try to get from cache  

            bool cacheResult = LayeredCache.Instance.Get<List<Ratio>>(key, ref result, GetRatios, new Dictionary<string, object>() { { "groupId", groupId } },
                groupId, LayeredCacheConfigNames.GET_RATIOS_CACHE_CONFIG_NAME, new List<string>() { LayeredCacheKeys.GetGroupRatiosInvalidationKey(groupId) });

            if (!cacheResult)
            {
                log.Error(string.Format("GetGroupRatios - Failed get data from cache groupId = {0}", groupId));
                result = null;
            }

            return result;
        }

        private static Tuple<List<Ratio>, bool> GetRatios(Dictionary<string, object> funcParams)
        {
            bool res = false;
            List<Ratio> ratios = null;
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

            return new Tuple<List<Ratio>, bool>(ratios, res);
        }

        private static List<Ratio> CreateRatios(DataTable dt)
        {
            List<Ratio> response = null;
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                response = new List<Ratio>();
                foreach (DataRow dr in dt.Rows)
                {
                    Ratio ratio = new Ratio()
                    {
                        Id = ODBCWrapper.Utils.GetLongSafeVal(dr, "id"),
                        Name = ODBCWrapper.Utils.GetSafeStr(dr, "name"),
                        Height = ODBCWrapper.Utils.GetIntSafeVal(dr, "height"),
                        Width = ODBCWrapper.Utils.GetIntSafeVal(dr, "width"),
                        PrecisionPrecentage = ODBCWrapper.Utils.GetIntSafeVal(dr, "precision_percentage")
                    };

                    response.Add(ratio);
                }
            }

            return response;
        }

        private static Image CreateImageFromDataRow(int groupId, DataRow row, Dictionary<long, string> imageTypeIdToRatioName, List<Ratio> ratios, long id = 0)
        {
            Image image = null;
            long imageId = id == 0 ? ODBCWrapper.Utils.GetLongSafeVal(row, "ID") : id;
            Dictionary<long, string> imageTypeIdToNameMap = Core.Catalog.CatalogManagement.ImageManager.GetImageTypeIdToNameMap(groupId);
            long ImageTypeId = ODBCWrapper.Utils.GetLongSafeVal(row, "IMAGE_TYPE_ID");
            string ImageTypeName = imageTypeIdToNameMap != null && imageTypeIdToNameMap.ContainsKey(ImageTypeId) ? 
                imageTypeIdToNameMap[ImageTypeId] : string.Empty;
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
                    ImageTypeId = ImageTypeId,
                    ImageTypeName = ImageTypeName,
                    IsDefault = ODBCWrapper.Utils.GetIntSafeVal(row, "IS_DEFAULT", 0) > 0 ? true : false,
                    ReferenceTable = ImageReferenceTable.Pics,
                    ReferenceId = imageId
                };

                if (imageTypeIdToRatioName.ContainsKey(image.ImageTypeId))
                {
                    image.RatioName = imageTypeIdToRatioName[image.ImageTypeId];
                }

                image.Url = TVinciShared.ImageUtils.BuildImageUrl(groupId, image.ContentId, image.Version, 0, 0, 0, true);
            }

            return image;
        }

        private static Ratio GetRatioById(int groupId, long ratioId)
        {
            Ratio ratio = null;
            List<Ratio> groupRatios = GetGroupImageRatios(groupId);
            if (groupRatios != null && groupRatios.Count > 0)
            {
                ratio = groupRatios.Count(x => x.Id == ratioId) == 1 ? groupRatios.FirstOrDefault(x => x.Id == ratioId) : null;
            }

            return ratio;
        }

        private static ImageType GetImageType(int groupId, long imageTypeId)
        {
            ImageType imageType = null;
            GenericListResponse<ImageType> imageTypeResponse = GetImageTypes(groupId, true, new List<long>() { imageTypeId });
            if (imageTypeResponse != null && imageTypeResponse.Status != null && imageTypeResponse.Status.Code == (int)eResponseStatus.OK && imageTypeResponse.Objects.Count == 1)
            {
                imageType = imageTypeResponse.Objects[0];
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

        private static Dictionary<long, string> GetGroupRatioIdToNameMap(int groupId)
        {
            Dictionary<long, string> result = null;
            List<Ratio> groupRatios = GetGroupImageRatios(groupId);
            if (groupRatios != null && groupRatios.Count > 0)
            {
                result = new Dictionary<long, string>();
                foreach (Ratio ratio in groupRatios)
                {
                    if (!result.ContainsKey(ratio.Id))
                    {
                        result.Add(ratio.Id, ratio.Name);
                    }
                }
            }

            return result;
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
                            groupDefaultImages = new List<Image>();
                            List<long> imageTypesWithDefaultPic = imageTypes.Where(x => x.DefaultImageId.HasValue && x.DefaultImageId.Value > 0).Select(x => x.DefaultImageId.Value).ToList();
                            if (imageTypesWithDefaultPic != null && imageTypesWithDefaultPic.Count > 0)
                            {
                                GenericListResponse<Image> defaultImagesResponse = GetImagesByIds(groupId.Value, imageTypesWithDefaultPic, true);
                                if (defaultImagesResponse != null && defaultImagesResponse.Status != null && defaultImagesResponse.Status.Code == (int)eResponseStatus.OK)
                                {
                                    groupDefaultImages.AddRange(defaultImagesResponse.Objects);
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

        private static void InvalidateAsset(int groupId, long id, eAssetImageType assetImageType, [System.Runtime.CompilerServices.CallerMemberName] string callingMethod = "")
        {
            // invalidate media
            if (assetImageType == eAssetImageType.Media)
            {
                AssetManager.Instance.InvalidateAsset(eAssetTypes.MEDIA, groupId, id, callingMethod);
            }
            // invalidate program
            if (assetImageType == eAssetImageType.Program)
            {
                AssetManager.Instance.InvalidateAsset(eAssetTypes.EPG, groupId, id, callingMethod);
            }
            // invalidate channel
            else if (assetImageType == eAssetImageType.Channel)
            {
                string invalidationKey = LayeredCacheKeys.GetChannelInvalidationKey(groupId, (int)id);
                if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to invalidate channel with id: {0}, invalidationKey: {1} after {2}", id, invalidationKey, callingMethod);
                }
            }
        }

        private static void UpdateImagesForGroupWithPicSizes(int groupId, ref GenericListResponse<Image> imagesResponse, bool updateUrl = true)
        {
            try
            {
                // get picture sizes
                var ratioIdToPicSize = Cache.CatalogCache.Instance().GetGroupRatioIdToPicSizeMapping(groupId);

                if (ratioIdToPicSize != null && ratioIdToPicSize.Count > 0)
                {
                    foreach (Image image in imagesResponse.Objects)
                    {
                        ImageType imageType = ImageManager.GetImageType(groupId, image.ImageTypeId);
                        if (imageType != null && imageType.RatioId.HasValue && imageType.RatioId.Value > 0 && ratioIdToPicSize.ContainsKey(imageType.RatioId.Value))
                        {
                            var picSize = ratioIdToPicSize[imageType.RatioId.Value].First();
                            image.Height = picSize.Height;
                            image.Width = picSize.Width;

                            if (updateUrl)
                            {
                                image.Url = TVinciShared.ImageUtils.BuildImageUrl(groupId, image.ContentId, image.Version, image.Width, image.Height, 100);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed UpdateImagesForGroupWithPicSizes for groupId: {0}", groupId), ex);
            }
        }

        private static Image CreateEpgPicImageFromDataRow(int groupId, DataRow row, long id = 0)
        {
            Image image = null;
            long imageId = id == 0 ? ODBCWrapper.Utils.GetLongSafeVal(row, "ID") : id;
            if (imageId > 0)
            {
                image = new Image()
                {
                    Id = imageId,
                    ContentId = ODBCWrapper.Utils.GetSafeStr(row, "BASE_URL"),
                    ImageObjectType = eAssetImageType.ProgramGroup,
                    Status = (eTableStatus)ODBCWrapper.Utils.GetIntSafeVal(row, "STATUS"),
                    Version = ODBCWrapper.Utils.GetIntSafeVal(row, "VERSION"),
                    ImageTypeId = ODBCWrapper.Utils.GetLongSafeVal(row, "IMAGE_TYPE_ID"),
                };

                image.Url = TVinciShared.ImageUtils.BuildImageUrl(groupId, image.ContentId, image.Version, 0, 0, 0, true);
            }

            return image;
        }

        private static ImageReferenceTable GetImageReferenceTable(eAssetImageType imageObjectType)
        {
            switch (imageObjectType)
            {
                case eAssetImageType.ProgramGroup:
                    return ImageReferenceTable.EpgPics;
                case eAssetImageType.Media:
                case eAssetImageType.Channel:
                case eAssetImageType.Category:
                case eAssetImageType.DefaultPic:
                case eAssetImageType.LogoPic:
                case eAssetImageType.ImageType:
                case eAssetImageType.Program:
                default:
                    return ImageReferenceTable.Pics;
            }
        }

        #endregion

        #region Internal

        internal static GenericListResponse<Image> CreateImageListResponseFromDataTable(int groupId, DataTable dt, bool isWithPicSizes = false)
        {
            GenericListResponse<Image> response = new GenericListResponse<Image>();
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                response.Objects = new List<Image>();
                Dictionary<long, string> imageTypeIdToRatioName = ImageManager.GetImageTypeIdToRatioNameMap(groupId);
                GenericListResponse<Ratio> ratios = ImageManager.GetRatios(groupId);
                Dictionary<long, List<PicSize>> ratioIdToPicSize = null;
                List<ApiObjects.Ratio> oldGroupRatios = null;

                if (isWithPicSizes)
                {
                    ratioIdToPicSize = Cache.CatalogCache.Instance().GetGroupRatioIdToPicSizeMapping(groupId);
                    oldGroupRatios = Cache.CatalogCache.Instance().GetGroupRatios(groupId);
                }

                foreach (DataRow row in dt.Rows)
                {
                    Image image = CreateImageFromDataRow(groupId, row, imageTypeIdToRatioName, ratios.Objects);
                    if (image != null)
                    {
                        // backward compatibility for pic sizes
                        if (isWithPicSizes && !string.IsNullOrEmpty(image.RatioName) && oldGroupRatios != null && oldGroupRatios.Count > 0
                            && oldGroupRatios.Any(x => x.Name == image.RatioName) && ratioIdToPicSize != null && ratioIdToPicSize.Count > 0)
                        {
                            ApiObjects.Ratio oldRatio = oldGroupRatios.First(x => x.Name == image.RatioName);
                            if (ratioIdToPicSize.ContainsKey(oldRatio.Id))
                            {
                                foreach (PicSize picSize in ratioIdToPicSize[oldRatio.Id])
                                {
                                    Image imageToAdd = new Image(image)
                                    {
                                        Height = picSize.Height,
                                        Width = picSize.Width
                                    };
                                    imageToAdd.Url = TVinciShared.ImageUtils.BuildImageUrl(groupId, imageToAdd.ContentId, imageToAdd.Version, imageToAdd.Width, imageToAdd.Height, 100);
                                    response.Objects.Add(imageToAdd);
                                }
                            }
                        }
                        else
                        {
                            response.Objects.Add(image);
                        }
                    }
                }
            }
            response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());

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

        internal static Dictionary<ImageReferenceTable, Dictionary<long, long>> CreateImageReferncesIdsFromDataTable(DataTable dt)
        {
            Dictionary<ImageReferenceTable, Dictionary<long, long>> response = new Dictionary<ImageReferenceTable, Dictionary<long, long>>();
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    ImageReferenceTable table = (ImageReferenceTable)ODBCWrapper.Utils.GetIntSafeVal(row, "TABLE_REFERENCE");
                    if (!response.ContainsKey(table))
                    {
                        response.Add(table, new Dictionary<long, long>());
                    }

                    var tableReferenceId = ODBCWrapper.Utils.GetLongSafeVal(row, "TABLE_REFERENCE_ID");
                    if (!response[table].ContainsKey(tableReferenceId))
                    {
                        response[table].Add(tableReferenceId, ODBCWrapper.Utils.GetLongSafeVal(row, "ID"));
                    }
                }
            }

            return response;
        }

        internal static GenericListResponse<Image> CreateEpgPicsListResponseFromDataTable(int groupId, DataTable dt)
        {
            GenericListResponse<Image> response = new GenericListResponse<Image>();
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                response.Objects = new List<Image>();

                foreach (DataRow row in dt.Rows)
                {
                    Image image = CreateEpgPicImageFromDataRow(groupId, row);
                    if (image != null)
                    {
                        response.Objects.Add(image);
                    }
                }

                UpdateImagesForGroupWithPicSizes(groupId, ref response, false);
            }
            response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());

            return response;
        }

        #endregion

        #region Public Methods

        public static GenericResponse<ImageType> AddImageType(int groupId, ImageType imageTypeToAdd, long userId)
        {
            GenericResponse<ImageType> result = new GenericResponse<ImageType>();
            try
            {
                if (imageTypeToAdd.DefaultImageId.HasValue)
                {
                    GenericListResponse<Image> imageList = GetImagesByIds(groupId, new List<long>() { imageTypeToAdd.DefaultImageId.Value });
                    if (imageList == null || imageList.Status == null || imageList.Status.Code != (int)eResponseStatus.OK || imageList.Objects == null || imageList.Objects.Count != 1)
                    {
                        result.SetStatus(eResponseStatus.ImageDoesNotExist, eResponseStatus.ImageDoesNotExist.ToString());
                        return result;
                    }
                }

                if (imageTypeToAdd.RatioId.HasValue)
                {
                    List<Ratio> ratioList = GetGroupImageRatios(groupId);
                    // check for RatioId valid
                    if (ratioList != null && ratioList.Count(x => x.Id == imageTypeToAdd.RatioId.Value) == 0)
                    {
                        result.SetStatus((int)eResponseStatus.RatioDoesNotExist, eResponseStatus.RatioDoesNotExist.ToString());
                        return result;
                    }
                }

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
                log.Error(string.Format("Failed AddImageType for groupId: {0}", groupId), ex);
            }

            return result;
        }

        public static Status DeleteImageType(int groupId, long id, long userId)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                //check if exist before delete
                GenericListResponse<ImageType> imageTypeListResponse = GetImageTypes(groupId, true, new List<long>(new long[] { id }));
                if (imageTypeListResponse != null && imageTypeListResponse.Objects != null && imageTypeListResponse.Objects.Count == 0)
                {
                    result = new Status() { Code = (int)eResponseStatus.ImageTypeDoesNotExist, Message = eResponseStatus.ImageTypeDoesNotExist.ToString() };
                    return result;
                }

                DataSet ds = CatalogDAL.DeleteImageType(groupId, id, userId);
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    /* We don't care about the first table, we just checked to see count > 0 to know if the image type was deleted successfully
                       The second table is needed to invalidate the assets that had images of this image type  */
                    DataTable imagesDt = ds.Tables.Count > 1 && ds.Tables[1] != null ? ds.Tables[1] : null;
                    if (imagesDt != null && imagesDt.Rows != null && imagesDt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in imagesDt.Rows)
                        {
                            long imageObjectId = ODBCWrapper.Utils.GetLongSafeVal(dr, "ASSET_ID");
                            eAssetImageType imageObjectType = (eAssetImageType)ODBCWrapper.Utils.GetIntSafeVal(dr, "ASSET_IMAGE_TYPE");
                            if (imageObjectId > 0)
                            {
                                // invalidate asset with this image
                                InvalidateAsset(groupId, imageObjectId, imageObjectType);
                            }
                        }
                    }

                    result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

                    string invalidationKey = LayeredCacheKeys.GetGroupImageTypesInvalidationKey(groupId);
                    if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    {
                        log.ErrorFormat("Failed to set invalidation key on DeleteImageType key = {0}", invalidationKey);
                    }

                    ImageType imageType = imageTypeListResponse.Objects.FirstOrDefault();
                    if (imageType != null && imageType.DefaultImageId.HasValue && imageType.DefaultImageId.Value > 0)
                    {
                        string defaultGroupImagesInvalidationKey = LayeredCacheKeys.GetGroupDefaultImagesInvalidationKey(groupId);
                        if (!LayeredCache.Instance.SetInvalidationKey(defaultGroupImagesInvalidationKey))
                        {
                            log.ErrorFormat("Failed to set invalidation key on DeleteImageType key = {0}", defaultGroupImagesInvalidationKey);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed DeleteImageType for groupId: {0} and ImageTypeId: {1}", groupId, id), ex);
            }

            return result;
        }

        public static GenericResponse<ImageType> UpdateImageType(int groupId, long id, ImageType imageTypeToUpdate, long userId)
        {
            GenericResponse<ImageType> result = new GenericResponse<ImageType>();
            try
            {
                if (imageTypeToUpdate.DefaultImageId.HasValue)
                {
                    GenericListResponse<Image> imageList = GetImagesByIds(groupId, new List<long>() { imageTypeToUpdate.DefaultImageId.Value });
                    if (imageList == null || imageList.Status == null || imageList.Status.Code != (int)eResponseStatus.OK || imageList.Objects == null || imageList.Objects.Count != 1)
                    {
                        result.SetStatus(eResponseStatus.ImageDoesNotExist, eResponseStatus.ImageDoesNotExist.ToString());
                        return result;
                    }

                    if (imageList.Objects[0].ImageTypeId != id)
                    {
                        result.SetStatus(eResponseStatus.DefaultImageInvalidImageType, eResponseStatus.DefaultImageInvalidImageType.ToString());
                        return result;
                    }
                }

                GenericListResponse<ImageType> imageTypeListResponse = GetImageTypes(groupId, true, null);
                if (imageTypeListResponse == null || (imageTypeListResponse != null && imageTypeListResponse.Objects == null) || (imageTypeListResponse.Objects.Count == 0))
                {
                    result.SetStatus(eResponseStatus.ImageTypeDoesNotExist, eResponseStatus.ImageTypeDoesNotExist.ToString());
                    return result;
                }

                ImageType cachedImageType = imageTypeListResponse.Objects.FirstOrDefault(x => x.Id == id);
                if (cachedImageType == null)
                {
                    result.SetStatus(eResponseStatus.ImageTypeDoesNotExist, eResponseStatus.ImageTypeDoesNotExist.ToString());
                    return result;
                }

                cachedImageType = imageTypeListResponse.Objects.FirstOrDefault(x => x.SystemName == imageTypeToUpdate.SystemName && x.Id != id);
                if (cachedImageType != null)
                {
                    result.SetStatus(eResponseStatus.ImageTypeAlreadyInUse, eResponseStatus.ImageTypeAlreadyInUse.ToString());
                    return result;
                }

                if (imageTypeToUpdate.RatioId.HasValue)
                {
                    List<Ratio> ratioList = GetGroupImageRatios(groupId);
                    // check for RatioId valid
                    if (ratioList != null && ratioList.Count(x => x.Id == imageTypeToUpdate.RatioId.Value) == 0)
                    {
                        result.SetStatus((int)eResponseStatus.RatioDoesNotExist, eResponseStatus.RatioDoesNotExist.ToString());
                        return result;
                    }
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

                    if (result != null && result.Object != null && result.Object.DefaultImageId.HasValue && result.Object.DefaultImageId.Value > 0)
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
                log.Error(string.Format("Failed UpdateImageType for groupId: {0}, id: {1}", groupId, id), ex);
            }

            return result;
        }

        public static GenericListResponse<ImageType> GetImageTypes(int groupId, bool isSearchByIds, List<long> ids)
        {
            GenericListResponse<ImageType> response = new GenericListResponse<ImageType>();

            List<ImageType> imageTypes = GetGroupImageTypes(groupId);

            if (imageTypes != null)
            {
                response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                response.TotalItems = imageTypes.Count;

                if (ids == null || ids.Count == 0)
                {
                    response.Objects = imageTypes;
                    return response;
                }

                if (isSearchByIds)
                {
                    // return image Types according to Ids
                    response.Objects = imageTypes.Where(x => ids.Contains(x.Id)).ToList();
                }
                else
                {
                    // return image Types according to ratio Ids
                    response.Objects = imageTypes.Where(x => ids.Contains(x.RatioId.Value)).ToList();
                }

                response.TotalItems = response.Objects.Count;
            }

            return response;
        }

        public static GenericListResponse<Ratio> GetRatios(int groupId)
        {
            GenericListResponse<Ratio> response = new GenericListResponse<Ratio>();

            response.Objects = GetGroupImageRatios(groupId);

            if (response.Objects != null)
            {
                response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                response.TotalItems = response.Objects.Count;
            }

            return response;
        }

        public Status DeleteImage(int groupId, long id, long userId)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                //check if exist before delete
                GenericListResponse<Image> imagesResponse = GetImagesByIds(groupId, new List<long>(new long[] { id }));
                if (imagesResponse != null && imagesResponse.Objects != null && imagesResponse.Objects.Count == 0)
                {
                    result = new Status((int)eResponseStatus.ImageDoesNotExist, "Image does not exist");
                    return result;
                }

                if (imagesResponse.Objects[0].ImageObjectType == eAssetImageType.ProgramGroup)
                {
                    log.ErrorFormat("Image cannot be deleted due to imageType. assetId = {0}, assetType = {1}", imagesResponse.Objects[0].ImageObjectId, imagesResponse.Objects[0].ImageObjectType);
                    result.Set((int)eResponseStatus.Error, "Image cannot be deleted due to imageType");
                    return result;
                }

                if (CatalogDAL.DeleteImage(groupId, id))
                {
                    Image image = imagesResponse.Objects.First();
                    if (!CatalogDAL.DeletePic(groupId, image.ReferenceId, userId))
                    {
                        log.ErrorFormat("Failed to delete image from {0} table. Id = {1}", image.ReferenceTable, image.ReferenceId);
                    }

                    result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

                    if (image != null)
                    {
                        if (image.IsDefault.HasValue && image.IsDefault.Value)
                        {
                            string invalidationKey = LayeredCacheKeys.GetGroupDefaultImagesInvalidationKey(groupId);
                            if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                            {
                                log.ErrorFormat("Failed to set invalidation key on DeleteImage key = {0}", invalidationKey);
                            }
                        }

                        // in case of program, need to remove pic and update CB
                        if (image.ImageObjectType == eAssetImageType.Program)
                        {
                            var docId = EpgAssetManager.GetEpgCBKey(groupId, image.ImageObjectId);
                            EpgCB program = EpgDal.GetEpgCB(docId);
                            if (program != null && program.pictures != null)
                            {
                                /// remove picture
                                var pic = program.pictures.Where(x => x.IsProgramImage && x.PicID == image.ReferenceId).FirstOrDefault();
                                if (pic != null && program.pictures.Remove(pic))
                                {
                                    if (!EpgDal.SaveEpgCB(docId, program, cb => _ttlService.GetEpgCouchbaseTtlSeconds(cb)))
                                    {
                                        log.ErrorFormat("Error while update epgCB at DeleteImage. groupId: {0}, imageId:{1}, user: {2}", groupId, id, userId);
                                    }
                                }
                            }
                        }

                        // invalidate asset with this image
                        InvalidateAsset(groupId, image.ImageObjectId, image.ImageObjectType);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed DeleteImage for groupId: {0} and ImageId: {1}", groupId, id), ex);
            }

            return result;
        }

        public static GenericListResponse<Image> GetImagesByIds(int groupId, List<long> imageIds, bool? isDefault = null)
        {
            // TODO: IRA - Do we care about the order? 
            GenericListResponse<Image> response = new GenericListResponse<Image>();

            DataTable dt = CatalogDAL.GetImages(groupId, imageIds);
            Dictionary<ImageReferenceTable, Dictionary<long, long>> referncesIds = CreateImageReferncesIdsFromDataTable(dt);

            GenericListResponse<Image> tempResponse = new GenericListResponse<Image>();

            if (referncesIds.ContainsKey(ImageReferenceTable.Pics))
            {
                dt = CatalogDAL.GetImagesByIds(groupId, referncesIds[ImageReferenceTable.Pics].Keys.ToList(), isDefault);
                tempResponse = CreateImageListResponseFromDataTable(groupId, dt);

                if (tempResponse.HasObjects())
                {
                    // update IDs
                    tempResponse.Objects.ForEach(i => i.Id = referncesIds[ImageReferenceTable.Pics][i.Id]);
                    // check if group uses pic sizes
                    UpdateImagesForGroupWithPicSizes(groupId, ref tempResponse);

                    response = tempResponse;
                }
            }

            if (referncesIds.ContainsKey(ImageReferenceTable.EpgPics))
            {
                dt = CatalogDAL.GetEpgPics(groupId, referncesIds[ImageReferenceTable.EpgPics].Keys.ToList());
                tempResponse = CreateEpgPicsListResponseFromDataTable(groupId, dt);

                if (tempResponse.HasObjects())
                {
                    // update IDs
                    tempResponse.Objects.ForEach(i => i.Id = referncesIds[ImageReferenceTable.EpgPics][i.Id]);
                    response.Objects.AddRange(tempResponse.Objects);
                }
            }

            response.TotalItems = response.HasObjects() ? response.Objects.Count : 0;
            return response;
        }

        public GenericListResponse<Image> GetImagesByObject(int groupId, long imageObjectId, eAssetImageType imageObjectType, bool? isDefault = null)
        {
            if (imageObjectType == eAssetImageType.Program)
            {
                return GetProgramImages(groupId, imageObjectId);
            }
            else
            {
                return GetAssetImages(groupId, imageObjectId, imageObjectType, isDefault);
            }
        }

        public GenericResponse<Image> AddImage(int groupId, Image imageToAdd, long userId)
        {
            GenericResponse<Image> result = new GenericResponse<Image>();
            try
            {
                if (imageToAdd.ImageObjectType == eAssetImageType.ProgramGroup)
                {
                    log.ErrorFormat("ImageType is not addable. assetId = {0}, assetType = {1}", imageToAdd.ImageObjectId, imageToAdd.ImageObjectType);
                    result.SetStatus(eResponseStatus.Error, "ImageType is not addable");
                    return result;
                }

                if (imageToAdd.ImageObjectType == eAssetImageType.Media && imageToAdd.ImageObjectId > 0)
                {
                    // isAllowedToViewInactiveAssets = true because only operator can add image
                    GenericResponse<Asset> asset = AssetManager.Instance.GetAsset(groupId, imageToAdd.ImageObjectId, eAssetTypes.MEDIA, true);
                    if (asset.Status.Code != (int)eResponseStatus.OK)
                    {
                        log.ErrorFormat("Asset not found. assetId = {0}, assetType = {1}", imageToAdd.ImageObjectId, imageToAdd.ImageObjectType);
                        result.SetStatus(asset.Status.Code, "Asset not found");
                        return result;
                    }
                }

                if (imageToAdd.ImageObjectType == eAssetImageType.Channel && imageToAdd.ImageObjectId > 0)
                {
                    //isAllowedToViewInactiveAssets = true because only operator can add image
                    var contextData = new ContextData(groupId) { UserId = userId };
                    GenericResponse<GroupsCacheManager.Channel> channel = ChannelManager.Instance.GetChannel(contextData, (int)imageToAdd.ImageObjectId, true);
                    if (channel.Status.Code != (int)eResponseStatus.OK)
                    {
                        log.ErrorFormat("Channel not found. channelId = {0}", imageToAdd.ImageObjectId);
                        result.SetStatus(channel.Status.Code, "Channel not found");
                        return result;
                    }
                }

                if (imageToAdd.ImageObjectType == eAssetImageType.Program && imageToAdd.ImageObjectId > 0)
                {
                    GenericResponse<Asset> asset = AssetManager.Instance.GetAsset(groupId, imageToAdd.ImageObjectId, eAssetTypes.EPG, true);
                    if (asset.Status.Code != (int)eResponseStatus.OK)
                    {
                        log.ErrorFormat("Asset not found. assetId = {0}, assetType = {1}", imageToAdd.ImageObjectId, imageToAdd.ImageObjectType);
                        result.SetStatus(asset.Status.Code, "Asset not found");
                        return result;
                    }
                }

                ImageType imageType = GetImageType(groupId, imageToAdd.ImageTypeId);
                if (imageType == null)
                {
                    result.SetStatus(eResponseStatus.ImageTypeDoesNotExist, eResponseStatus.ImageTypeDoesNotExist.ToString());
                    return result;
                }

                DataTable dt = CatalogDAL.InsertPic(groupId, userId, imageToAdd.ImageObjectId, imageToAdd.ImageObjectType, imageToAdd.ImageTypeId);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    long id = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "ID");
                    if (id > 0)
                    {
                        var imageTypeIdToRatioName = ImageManager.GetImageTypeIdToRatioNameMap(groupId);
                        var ratios = ImageManager.GetRatios(groupId);
                        result.Object = CreateImageFromDataRow(groupId, dt.Rows[0], imageTypeIdToRatioName, ratios.Objects, id);

                        if (result.Object != null)
                        {
                            result.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                        }

                        ImageReferenceTable table = GetImageReferenceTable(imageToAdd.ImageObjectType);
                        id = CatalogDAL.InsertImage(groupId, (int)table, id);
                        if (id > 0)
                        {
                            result.Object.Id = id;

                            if (imageToAdd.ImageObjectType == eAssetImageType.ImageType)
                            {
                                // update default image ID in image type
                                GenericResponse<ImageType> imageTypeResult = UpdateImageType(groupId, result.Object.ImageTypeId,
                                    new ImageType() { DefaultImageId = result.Object.Id }, userId);

                                result.SetStatus(imageTypeResult.Status);
                            }
                        }
                        else
                        {
                            result.SetStatus(eResponseStatus.Error, eResponseStatus.Error.ToString());
                            log.ErrorFormat("Failed AddImage to Reference table. Id = {0} groupId: {1}", result.Object.Id);
                        }
                    }
                    else
                    {
                        result.SetStatus(eResponseStatus.ImageTypeAlreadyInUse, "Image type already in use for object");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed AddImage for groupId: {0}", groupId), ex);
            }

            return result;
        }

        public Status SetContent(int groupId, long userId, long id, string url)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                //check if exist before setting content
                GenericListResponse<Image> imagesResponse = GetImagesByIds(groupId, new List<long>(new long[] { id }));
                if (imagesResponse != null && imagesResponse.Objects != null && imagesResponse.Objects.Count == 0)
                {
                    result = new Status((int)eResponseStatus.ImageDoesNotExist, "Image does not exist");
                    return result;
                }

                if (imagesResponse.Objects[0].ImageObjectType == eAssetImageType.ProgramGroup)
                {
                    log.ErrorFormat("Image cannot be updated due to imageType. assetId = {0}, assetType = {1}", imagesResponse.Objects[0].ImageObjectId, imagesResponse.Objects[0].ImageObjectType);
                    result.Set((int)eResponseStatus.Error, "Image cannot be updated due to imageType");
                    return result;
                }

                Image image = imagesResponse.Objects[0];
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
                Ratio ratio = null;
                ImageType imageType = GetImageType(groupId, image.ImageTypeId);
                if (imageType == null)
                {
                    result = new Status((int)eResponseStatus.ImageTypeDoesNotExist, eResponseStatus.ImageTypeDoesNotExist.ToString());
                    return result;
                }

                if (imageType.RatioId.HasValue && imageType.RatioId.Value > 0)
                {
                    ratio = GetRatioById(groupId, imageType.RatioId.Value);
                    if (ratio != null && ratio.PrecisionPrecentage > 0)
                    {
                        image.RatioName = ratio.Name;

                        try
                        {
                            var imageBytes = ApiLogic.FileManager.Instance.DownloadImage(groupId, url, image.ContentId, image);

                            if (!imageBytes.IsOkStatusCode())
                            {
                                log.Error($"Failed to set content of: id: {id}, url: {url}");
                                result = imageBytes.Status;
                                return result;
                            }

                            var imageStream = new MemoryStream(imageBytes.Object);
                            double downloadedImageRatio;

                            using (System.Drawing.Image downloadedImage = System.Drawing.Image.FromStream(imageStream)) 
                            {
                                downloadedImageRatio = (double)downloadedImage.Width / downloadedImage.Height;
                            }
                            
                            double imageDefinedRatio = (double)ratio.Width / ratio.Height;
                            double imageRatioPrecisionPrecentage = Math.Round((1 - Math.Abs((downloadedImageRatio - imageDefinedRatio) / imageDefinedRatio)) * 100);
                            if (ratio.PrecisionPrecentage > imageRatioPrecisionPrecentage)
                            {
                                result = new Status((int)eResponseStatus.InvalidRatioForImage, eResponseStatus.InvalidRatioForImage.ToString());
                                return result;
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error(string.Format("Failed to validate image ratio, groupId: {0}, imageId: {1}, url: {2}, ex: {3}", groupId, id, url, ex), ex);
                            result = new Status((int)eResponseStatus.InvalidUrlForImage, eResponseStatus.InvalidUrlForImage.ToString());
                            return result;
                        }
                    }
                }

                // post image
                ImageServerUploadRequest imageServerReq = new ImageServerUploadRequest() { GroupId = groupId, Id = image.ContentId, SourcePath = url, Version = image.Version };
                string res = TvinciImporter.Utils.HttpPost(TVinciShared.ImageUtils.GetImageServerUrl(groupId, eHttpRequestType.Post), JsonConvert.SerializeObject(imageServerReq), "application/json");

                // check result
                if (string.IsNullOrEmpty(res) || res.ToLower() != "true")
                {
                    log.ErrorFormat("POST to image server failed. imageId = {0}, contentId = {1}", image.ReferenceId, image.ContentId);
                    TVinciShared.ImageUtils.UpdateImageState(groupId, image.ReferenceId, image.Version, eMediaType.VOD, eTableStatus.Failed, (int)userId);
                    result = new Status((int)eResponseStatus.InvalidUrlForImage, eResponseStatus.InvalidUrlForImage.ToString());
                    return result;
                }
                else if (res.ToLower() == "true")
                {
                    log.DebugFormat("POST to image server successfully sent. imageId = {0}, contentId = {1}", image.ReferenceId, image.ContentId);
                    TVinciShared.ImageUtils.UpdateImageState(groupId, image.ReferenceId, image.Version, eMediaType.VOD, eTableStatus.OK, (int)userId, image.ContentId);
                    result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

                    if (image.ImageObjectType == eAssetImageType.Program)
                    {
                        EpgAssetManager.UpdateProgramAssetPictures(groupId, userId, image, url);
                    }

                    // invalidate asset with this image
                    InvalidateAsset(groupId, image.ImageObjectId, image.ImageObjectType);

                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed SetContent for groupId: {0} and ImageId: {1}", groupId, id), ex);
            }

            return result;
        }

        public static RatioResponse AddRatio(int groupId, long userId, Ratio ratio)
        {
            RatioResponse result = new RatioResponse();
            try
            {
                DataTable dt = CatalogDAL.InsertGroupImageRatios(groupId, userId, ratio.Name, ratio.Height, ratio.Width,
                                                                    ratio.PrecisionPrecentage);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    long id = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "ID");

                    if (id > 0)
                    {
                        result.Ratio = new Ratio()
                        {
                            Id = id,
                            Height = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "height"),
                            Width = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "width"),
                            PrecisionPrecentage = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "precision_percentage"),
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
                log.Error(string.Format("Failed AddRatio for groupId: {0}", groupId), ex);
            }

            return result;
        }

        public static RatioResponse UpdateRatio(int groupId, long userId, Ratio ratio, long ratioId)
        {
            RatioResponse result = new RatioResponse();
            try
            {
                DataTable dt = CatalogDAL.UpdateGroupImageRatios(ratioId, groupId, userId, ratio.PrecisionPrecentage);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    result.Ratio = new Ratio()
                    {
                        Id = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "ID"),
                        Height = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "height"),
                        Width = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "width"),
                        PrecisionPrecentage = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "precision_percentage"),
                        Name = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "NAME")
                    };

                    string invalidationKey = LayeredCacheKeys.GetGroupRatiosInvalidationKey(groupId);
                    if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    {
                        log.ErrorFormat("Failed to set invalidation key on UpdateRatio key = {0}", invalidationKey);
                    }

                    result.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
                else
                {
                    result.Status = new Status((int)eResponseStatus.RatioDoesNotExist, "Ratio Does Not Exist");
                    return result;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed UpdateRatio for ratioId: {0} and groupId: {1} ", ratioId, groupId), ex);
            }

            return result;
        }

        public static List<Picture> ConvertImagesToPictures(List<Image> assetImages, int groupId)
        {
            List<Picture> pictures = new List<Picture>();
            if (assetImages != null && assetImages.Count > 0)
            {
                // get picture sizes
                Dictionary<long, List<PicSize>> ratioIdToPicSizes = Cache.CatalogCache.Instance().GetGroupRatioIdToPicSizeMapping(groupId);
                List<ApiObjects.Ratio> oldGroupRatios = Cache.CatalogCache.Instance().GetGroupRatios(groupId);

                foreach (Image image in assetImages)
                {
                    ImageType imageType = ImageManager.GetImageType(groupId, image.ImageTypeId);
                    if (imageType != null && imageType.RatioId.HasValue && imageType.RatioId.Value > 0)
                    {
                        if (image.Width > 0 && image.Height > 0)
                        {
                            PicSize picSize = new PicSize() { Height = image.Height, Width = image.Width };
                            pictures.Add(new Picture(groupId, image, imageType.Name, ImageManager.GetRatioName(groupId, imageType.RatioId.Value), picSize));
                        }
                        else if (!string.IsNullOrEmpty(image.RatioName) && oldGroupRatios != null && oldGroupRatios.Count > 0
                                 && oldGroupRatios.Any(x => x.Name == image.RatioName) && ratioIdToPicSizes != null && ratioIdToPicSizes.Count > 0)
                        {
                            ApiObjects.Ratio oldRatio = oldGroupRatios.First(x => x.Name == image.RatioName);
                            if (ratioIdToPicSizes.ContainsKey(oldRatio.Id))
                            {
                                foreach (PicSize picSize in ratioIdToPicSizes[oldRatio.Id])
                                {
                                    pictures.Add(new Picture(groupId, image, imageType.Name, ImageManager.GetRatioName(groupId, imageType.RatioId.Value), picSize));
                                }
                            }
                        }
                        else
                        {
                            pictures.Add(new Picture(groupId, image, imageType.Name, ImageManager.GetRatioName(groupId, imageType.RatioId.Value), null));
                        }
                    }
                }
            }

            return pictures;
        }

        // for backward compatibility
        public static Dictionary<long, string> GetImageTypeIdToRatioNameMap(int groupId)
        {
            Dictionary<long, string> result = null;
            List<ImageType> groupImageTypes = GetGroupImageTypes(groupId);
            if (groupImageTypes != null && groupImageTypes.Count > 0)
            {
                Dictionary<long, string> ratioIdToNameMap = GetGroupRatioIdToNameMap(groupId);
                if (ratioIdToNameMap != null && ratioIdToNameMap.Count > 0)
                {
                    result = new Dictionary<long, string>();
                    foreach (ImageType imageType in groupImageTypes)
                    {
                        if (!result.ContainsKey(imageType.Id))
                        {
                            if (ratioIdToNameMap.ContainsKey(imageType.RatioId.Value))
                            {
                                result.Add(imageType.Id, ratioIdToNameMap[imageType.RatioId.Value]);
                            }
                            else
                            {
                                result.Add(imageType.Id, string.Empty);
                            }
                        }
                    }
                }
            }

            return result;
        }
        
        public static Dictionary<long, string> GetImageTypeIdToNameMap(int groupId)
        {
            Dictionary<long, string> result = null;
            List<ImageType> groupImageTypes = GetGroupImageTypes(groupId);
            if (groupImageTypes != null && groupImageTypes.Count > 0)
            {
                    result = groupImageTypes.Aggregate(new Dictionary<long, string>(),
                        (x, y) =>
                        {
                            x.Add(y.Id, y.Name);
                            return x;
                        });
            }

            return result;
        }
        
        public static Dictionary<string, ApiObjects.Catalog.ImageType> GetImageTypesMapBySystemName(int groupId)
        {
            Dictionary<string, ImageType> groupRatioNamesToImageTypes = null;

            GenericListResponse<ImageType> imageTypes = GetImageTypes(groupId, false, null);
            if (imageTypes != null && imageTypes.HasObjects())
            {
                groupRatioNamesToImageTypes = new Dictionary<string, ImageType>();

                foreach (var imageType in imageTypes.Objects)
                {
                    if (!groupRatioNamesToImageTypes.ContainsKey(imageType.SystemName))
                    {
                        groupRatioNamesToImageTypes.Add(imageType.SystemName, imageType);
                    }
                }
            }

            return groupRatioNamesToImageTypes;
        }

        public static Dictionary<string, List<EpgPicture>> GetGroupEpgPicturesSizes(int groupId)
        {
            string allEpgPicturesKey = LayeredCacheKeys.GetAllEpgPicturesKey(groupId);
            // map ratio name to ratios
            Dictionary<string, List<EpgPicture>> epgPictures = null;

            if (!LayeredCache.Instance.Get<Dictionary<string, List<EpgPicture>>>(allEpgPicturesKey,
                                                                                ref epgPictures,
                                                                                GetAllEpgPictures,
                                                                                new Dictionary<string, object>() { { "groupId", groupId } },
                                                                                groupId,
                                                                                LayeredCacheConfigNames.GET_ALL_EPG_PICTURES))
            {
                log.ErrorFormat("GetGroupEpgPicturesSizes - GetAllEpgPictures - Failed get data from cache. groupId: {0}", groupId);
            }

            return epgPictures;
        }

        private static Tuple<Dictionary<string, List<EpgPicture>>, bool> GetAllEpgPictures(Dictionary<string, object> funcParams)
        {
            Dictionary<string, List<EpgPicture>> epgPictures = null;
            bool res = false;
            try
            {
                if (funcParams != null && funcParams.Count == 1)
                {
                    if (funcParams.ContainsKey("groupId"))
                    {
                        int? groupId = funcParams["groupId"] as int?;

                        if (groupId.HasValue)
                        {
                            List<ApiObjects.Ratio> epgRatios = new List<ApiObjects.Ratio>();
                            Dictionary<int, List<EpgPicture>> groupsToPictures = CatalogDAL.GetGroupTreeMultiPicEpgUrl(groupId.Value, ref epgRatios);

                            if (groupsToPictures != null && groupsToPictures.ContainsKey(groupId.Value))
                            {
                                epgPictures = new Dictionary<string, List<EpgPicture>>();
                                res = true;
                                var pictures = groupsToPictures[groupId.Value];

                                foreach (EpgPicture epgPicture in pictures)
                                {
                                    if (epgPictures.ContainsKey(epgPicture.Ratio))
                                    {
                                        epgPictures[epgPicture.Ratio].Add(epgPicture);
                                    }
                                    else
                                    {
                                        epgPictures.Add(epgPicture.Ratio, new List<EpgPicture>() { epgPicture });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res = false;
                log.Warn(string.Format("GetAllEpgPictures faild params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<string, List<EpgPicture>>, bool>(epgPictures, res);
        }

        private static GenericListResponse<Image> GetProgramImages(int groupId, long imageObjectId)
        {
            GenericListResponse<Image> response = new GenericListResponse<Image>();

            var docId = EpgAssetManager.GetEpgCBKey(groupId, imageObjectId);
            EpgCB epgCB = EpgDal.GetEpgCB(docId);

            if (epgCB != null && epgCB.pictures != null)
            {
                Dictionary<long, Image> pics = new Dictionary<long, Image>();
                Dictionary<long, Image> epgPics = new Dictionary<long, Image>();
                var ratioNamesToImageTypes = GetImageTypesMapBySystemName(groupId);
                Dictionary<long, string> imageTypeIdToNameMap = Core.Catalog.CatalogManagement.ImageManager.GetImageTypeIdToNameMap(groupId);
                
                foreach (var pic in epgCB.pictures)
                {
                    long imageTypeId = pic.ImageTypeId > 0 ? pic.ImageTypeId : ratioNamesToImageTypes.ContainsKey(pic.Ratio) ? ratioNamesToImageTypes[pic.Ratio].Id : 0;
                    string imageTypeName = imageTypeIdToNameMap != null && imageTypeIdToNameMap.ContainsKey(imageTypeId) ? 
                        imageTypeIdToNameMap[imageTypeId] : string.Empty;
                    
                    Image image = new Image()
                    {
                        Id = pic.PicID,
                        ContentId = pic.Url,
                        Version = pic.Version,
                        IsDefault = false,
                        ImageObjectId = imageObjectId,
                        Status = eTableStatus.OK,
                        ImageTypeId = imageTypeId,
                        ImageTypeName = imageTypeName
                    };
                    if (pic.IsProgramImage)
                    {
                        image.ImageObjectType = eAssetImageType.Program;
                        pics.Add(image.Id, image);
                    }
                    else
                    {
                        image.ImageObjectType = eAssetImageType.ProgramGroup;
                        epgPics.Add(image.Id, image);
                    }

                    image.Url = TVinciShared.ImageUtils.BuildImageUrl(groupId, image.ContentId, image.Version, 0, 0, 0, true);

                }

                Dictionary<ImageReferenceTable, Dictionary<long, long>> referncesIds;
                DataTable imagesDT;
                // Get images for updating image.Id 
                // if not exist, create new row  and update id.
                if (pics != null && pics.Keys.Count > 0)
                {
                    imagesDT = CatalogDAL.GetImagesByTableReferenceIds(groupId, ImageReferenceTable.Pics, pics.Keys.ToList());
                    referncesIds = CreateImageReferncesIdsFromDataTable(imagesDT);

                    foreach (Image item in pics.Values)
                    {
                        if (referncesIds.Keys.Count > 0 && referncesIds[ImageReferenceTable.Pics].ContainsKey(item.Id))
                        {
                            item.Id = referncesIds[ImageReferenceTable.Pics][item.Id];
                        }
                        else
                        {
                            item.Id = CatalogDAL.InsertImage(groupId, (int)ImageReferenceTable.Pics, item.Id);
                        }
                    }

                    response.Objects = pics.Values.ToList();
                }

                if (epgPics != null && epgPics.Keys.Count > 0)
                {
                    imagesDT = CatalogDAL.GetImagesByTableReferenceIds(groupId, ImageReferenceTable.EpgPics, epgPics.Keys.ToList());
                    referncesIds = CreateImageReferncesIdsFromDataTable(imagesDT);

                    foreach (Image item in epgPics.Values)
                    {
                        if (referncesIds.Keys.Count > 0 && referncesIds[ImageReferenceTable.EpgPics].ContainsKey(item.Id))
                        {
                            item.Id = referncesIds[ImageReferenceTable.EpgPics][item.Id];
                        }
                        else
                        {
                            item.Id = CatalogDAL.InsertImage(groupId, (int)ImageReferenceTable.EpgPics, item.Id);
                        }
                    }

                    response.Objects.AddRange(epgPics.Values.ToList());

                }
            }

            response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            return response;
        }

        private static GenericListResponse<Image> GetProgramImages(int groupId, List<long> imageObjectIds)
        {
            GenericListResponse<Image> response = new GenericListResponse<Image>();
            GenericListResponse<Image> result;
            foreach (var imageObjectId in imageObjectIds)
            {
                result = GetProgramImages(groupId, imageObjectId);
                if (result.Objects?.Count > 0)
                {
                    response.Objects.AddRange(result.Objects);
                }
            }

            response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            return response;
        }


        private static GenericListResponse<Image> GetAssetImages(int groupId, long imageObjectId, eAssetImageType imageObjectType, bool? isDefault = null)
        {
            GenericListResponse<Image> response = new GenericListResponse<Image>();

            DataTable dt = CatalogDAL.GetImagesByObject(groupId, imageObjectId, imageObjectType);
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                return GetAssetImage(groupId, dt, isDefault);
            }

            response.SetStatus(eResponseStatus.OK);
            return response;
        }

        private static GenericListResponse<Image> GetAssetImage(int groupId, DataTable picsData, bool? isDefault = null)
        {
            GenericListResponse<Image> response = new GenericListResponse<Image>();
            DataTable imagesDT = null;
            Dictionary<long, Image> pics = null;
            Dictionary<ImageReferenceTable, Dictionary<long, long>> referncesIds = null;

            if (picsData != null && picsData.Rows.Count > 0)
            {
                response = CreateImageListResponseFromDataTable(groupId, picsData);
                if (response.HasObjects())
                {
                    pics = new Dictionary<long, Image>();

                    foreach (var item in response.Objects)
                    {
                        pics.Add(item.Id, item);
                    }

                    // update IDs
                    imagesDT = CatalogDAL.GetImagesByTableReferenceIds(groupId, ImageReferenceTable.Pics, pics.Keys.ToList());
                    referncesIds = CreateImageReferncesIdsFromDataTable(imagesDT);

                    response.Objects = pics.Values.ToList();
                    foreach (Image item in response.Objects)
                    {
                        if (referncesIds.Keys.Count > 0 && referncesIds[ImageReferenceTable.Pics].ContainsKey(item.Id))
                        {
                            item.Id = referncesIds[ImageReferenceTable.Pics][item.Id];
                        }
                        else
                        {
                            item.Id = CatalogDAL.InsertImage(groupId, (int)ImageReferenceTable.Pics, item.Id);
                        }
                    }

                    //    // check if group uses pic sizes
                    //    UpdateImagesForGroupWithPicSizes(groupId, ref response);
                }

                if (isDefault.HasValue && isDefault.Value)
                {
                    List<Image> groupDefualtImages = GetGroupDefaultImages(groupId);
                    HashSet<long> assetImageTypes = new HashSet<long>(response.Objects.Select(x => x.ImageTypeId).ToList());
                    response.Objects.AddRange(groupDefualtImages.Where(x => !assetImageTypes.Contains(x.ImageTypeId)));
                }

                if (response.Objects?.Count > 0)
                {
                    response.SetStatus(eResponseStatus.OK);
                }
            }
            return response;
        }

        private static GenericListResponse<Image> GetAssetImages(int groupId, List<long> imageObjectIds, eAssetImageType imageObjectType, bool? isDefault = null)
        {
            GenericListResponse<Image> response = new GenericListResponse<Image>();

            DataTable dt = CatalogDAL.GetImagesByObject(groupId, imageObjectIds, imageObjectType);
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                return GetAssetImage(groupId, dt, isDefault);
            }

            response.SetStatus(eResponseStatus.OK);
            return response;
        }

        public static GenericListResponse<Image> GetImagesByObjects(int groupId, List<long> imageObjectIds, eAssetImageType assetImageType, bool? isDefault)
        {
            if (assetImageType == eAssetImageType.Program)
            {
                return GetProgramImages(groupId, imageObjectIds);
            }
            else
            {
                return GetAssetImages(groupId, imageObjectIds, assetImageType, isDefault);
            }
        }

        #endregion
    }
}
