using WebAPI.Models.Catalog;

namespace WebAPI.ModelsFactory
{
    public static class AssetFactory
    {
        public static KalturaRecordingAsset Create(KalturaProgramAsset asset)
        {
            var instance = new KalturaRecordingAsset()
            {
                CatchUpEnabled = asset.EnableCatchUp,
                CdvrEnabled = asset.EnableCdvr,
                CreateDate = asset.CreateDate,
                Crid = asset.Crid,
                Description = asset.Description,
                EnableCatchUp = asset.EnableCatchUp,
                EnableCdvr = asset.EnableCdvr,
                EnableStartOver = asset.EnableStartOver,
                EnableTrickPlay = asset.EnableTrickPlay,
                EndDate = asset.EndDate,
                EpgChannelId = asset.EpgChannelId,
                EpgId = asset.EpgId,
                TrickPlayEnabled = asset.EnableTrickPlay,
                Images = asset.Images,
                IndexStatus = asset.IndexStatus,
                MediaFiles = asset.MediaFiles,
                Metas = asset.Metas,
                Name = asset.Name,
                RelatedEntities = asset.RelatedEntities,
                relatedObjects = asset.relatedObjects,
                StartDate = asset.StartDate,
                StartOverEnabled = asset.EnableStartOver,
                Statistics = asset.Statistics,
                Tags = asset.Tags,
                Type = asset.Type,
                UpdateDate = asset.UpdateDate,
                RelatedMediaId = asset.RelatedMediaId,
                LinearAssetId = asset.LinearAssetId,
                Id = asset.Id,
                ExternalId = asset.ExternalId,
                ExternalOfferIds = asset.ExternalOfferIds,
            };

            return instance;
        }
    }
}
