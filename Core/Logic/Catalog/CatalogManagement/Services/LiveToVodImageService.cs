using System.Collections.Generic;
using System.Linq;
using ApiObjects;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Microsoft.Extensions.Logging;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public class LiveToVodImageService : ILiveToVodImageService
    {
        private readonly IImageManager _imageManager;

        public LiveToVodImageService(IImageManager imageManager, ILogger<LiveToVodImageService> logger)
        {
            _imageManager = imageManager;
        }

        public void AddImages(long partnerId, IEnumerable<Image> imagesToAdd, long assetId, long updaterId)
        {
            if (imagesToAdd?.Any() != true)
            {
                return;
            }

            var imageTypesToImages = imagesToAdd.ToLookup(x => x.ImageTypeId);
            foreach (var imageTypeToImage in imageTypesToImages)
            {
                InsertImage(partnerId, assetId, imageTypeToImage.First(), updaterId);
            }
        }

        public void UpdateImages(long partnerId, IEnumerable<Image> imagesToUpsert, long assetId, long updaterId)
        {
            if (imagesToUpsert?.Any() != true)
            {
                return;
            }

            var imageTypesToImage = imagesToUpsert.ToLookup(x => x.ImageTypeId);
            var imagesResponse = _imageManager.GetImagesByObject((int)partnerId, assetId, eAssetImageType.Media);
            if (!imagesResponse.IsOkStatusCode())
            {
                return;
            }

            var existingImages = imagesResponse.Objects?.ToDictionary(x => x.ImageTypeId, x => x.Id)
                ?? new Dictionary<long, long>();
            foreach (var imageTypeToImage in imageTypesToImage)
            {
                var image = imageTypeToImage.First();
                if (existingImages.TryGetValue(image.ImageTypeId, out var imageId))
                {
                    // TODO: consider to make this operation asynchronous
                    _imageManager.SetContent((int)partnerId, updaterId, imageId, image.SourceUrl);
                }
                else
                {
                    InsertImage(partnerId, assetId, image, updaterId);
                }
            }
        }

        private void InsertImage(long partnerId, long assetId, Image image, long updaterId)
        {
            var imageToInsert = new Image
            {
                ImageObjectType = eAssetImageType.Media,
                ImageObjectId = assetId,
                ImageTypeId = image.ImageTypeId
            };

            var imageResponse = _imageManager.AddImage((int)partnerId, imageToInsert, updaterId);
            if (!imageResponse.IsOkStatusCode())
            {
                return;
            }

            // TODO: consider to make this operation asynchronous
            _imageManager.SetContent((int)partnerId, updaterId, imageResponse.Object.Id, image.SourceUrl);
        }
    }
}