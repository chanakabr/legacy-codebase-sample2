using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiObjects;
using ApiObjects.Catalog;
using Core.Catalog;
using Phoenix.Generated.Api.Events.Crud.ProgramAsset;
using TVinciShared;
using Image = Phoenix.Generated.Api.Events.Crud.ProgramAsset.Image;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public class ProgramCrudEventMapper : IProgramCrudEventMapper
    {
        private static readonly Lazy<IProgramCrudEventMapper> _lazy = new Lazy<IProgramCrudEventMapper>(
            () => new ProgramCrudEventMapper(),
            LazyThreadSafetyMode.PublicationOnly);

        public static IProgramCrudEventMapper Instance => _lazy.Value;

        public ProgramAsset Map(EpgAsset epgAsset, LiveAsset liveAsset, long groupId, long updaterId, long operation)
        {
            var expirationDate = epgAsset.CatchUpEnabled == true
                ? epgAsset.EndDate?.AddMinutes(liveAsset.SummedCatchUpBuffer)
                : epgAsset.EndDate;
            
            return new ProgramAsset
            {
                Id = epgAsset.Id,
                Name = epgAsset.Name,
                MultilingualName = epgAsset.NamesWithLanguages?.Select(Map).ToArray()
                    ?? Array.Empty<TranslationValue>(),
                Description = epgAsset.Description,
                MultilingualDescription = epgAsset.DescriptionsWithLanguages?.Select(Map).ToArray()
                    ?? Array.Empty<TranslationValue>(),
                Metas = epgAsset.Metas?.Select(Map).ToArray() ?? Array.Empty<AssetMeta>(),
                Tags = epgAsset.Tags?.Select(Map).ToArray() ?? Array.Empty<AssetTag>(),
                Crid = epgAsset.Crid,
                LinearAssetId = epgAsset.LinearAssetId.Value,
                EpgChannelId = epgAsset.EpgChannelId.Value,
                EpgId = epgAsset.EpgIdentifier,
                Images = epgAsset.Images?.Select(Map).ToArray() ?? Array.Empty<Image>(),
                EndDate = epgAsset.EndDate.Value.ToUtcUnixTimestampSeconds(),
                ExpirationDate = expirationDate?.ToUtcUnixTimestampSeconds(),
                ExternalOfferIds = (epgAsset.ExternalOfferIds ?? new List<string>(0)).ToArray(),
                PartnerId = groupId,
                UpdaterId = updaterId,
                StartDate = epgAsset.StartDate.Value.ToUtcUnixTimestampSeconds(),
                Operation = operation
            };
        }
        
        private static Image Map(Core.Catalog.Image source)
            => new Image
            {
                ImageObjectId = source.ImageObjectId,
                Status = Map(source.Status),
                ContentId = source.ContentId,
                Version = source.Version,
                ImageTypeId = source.ImageTypeId
            };

        private static Status Map(eTableStatus source)
        {
            switch (source)
            {
                case eTableStatus.Pending:
                    return Status.Pending;
                case eTableStatus.OK:
                    return Status.Ready;
                case eTableStatus.Failed:
                    return Status.Failed;
                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, null);
            }
        }

        private static AssetTag Map(Tags source)
            => new AssetTag
            {
                Name = source.m_oTagMeta.m_sName,
                Values = source.Values.Select(Map).ToArray()
            };

        private static AssetTagValue Map(IEnumerable<LanguageContainer> translations)
        {
            var result = new AssetTagValue();
            var resultTranslations = new List<TranslationValue>();
            foreach (var translation in translations)
            {
                if (translation.IsDefault)
                {
                    result.Value = translation.m_sValue;
                }
                else
                {
                    resultTranslations.Add(Map(translation));
                }
            }
            
            result.Translations = resultTranslations.ToArray();
            
            return result;
        }

        private static AssetMeta Map(Metas source)
            => new AssetMeta
            {
                Type = source.m_oTagMeta.m_sType,
                Name = source.m_oTagMeta.m_sName,
                Value = source.m_sValue,
                Translations = source.Value?.Where(x => !x.IsDefault).Select(Map).ToArray()
            };

        private static TranslationValue Map(LanguageContainer source)
            => new TranslationValue
            {
                Language = source.m_sLanguageCode3,
                Value = source.m_sValue
            };
    }
}