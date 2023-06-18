using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.ObjectsConvertor.Extensions;

namespace WebAPI.ModelsValidators
{
    public static class AssetValidator
    {
        public static void ValidateForInsert(this KalturaAsset model)
        {
            if (!(model is KalturaLiveAsset) && !(model is KalturaMediaAsset) && !(model is KalturaProgramAsset))
            {
                throw new ClientException((int)StatusCode.Error, "Invalid assetType");
            }

            if ((model is KalturaProgramAsset) && model.Type.HasValue && model.Type.Value != 0)
            {
                throw new ClientException((int)StatusCode.Error, "Invalid type value");
            }

            if (model.Name == null || model.Name.Values == null || model.Name.Values.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
            }
            model.Name.Validate("multilingualName");

            if (model.Description != null && model.Description.Values != null && model.Description.Values.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "description");
            }

            if (model.Description != null)
            {
                model.Description.Validate("multilingualDescription");
            }

            if (!model.Type.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "type");
            }

            if (string.IsNullOrEmpty(model.ExternalId))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "externalId");
            }

            model.ValidateMetas();
            model.ValidateTags();
            model.ValidateRelatedEntities();

            switch (model)
            {
                case KalturaLiveAsset live: ValidateLiveAssetForInsert(live); break;
                case KalturaRecordingAsset recording : ValidateProgramAssetForInsert(recording); break;
                case KalturaProgramAsset program: ValidateProgramAssetForInsert(program); break;
                default:
                    break;
            }
        }

        private static void ValidateLiveAssetForInsert(KalturaLiveAsset model)
        {
            if (model.EnableCatchUpState == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "enableCatchUpState");
            }

            if (model.EnableCdvrState == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "enableCdvrState");
            }

            if (model.EnableRecordingPlaybackNonEntitledChannelState == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "enableRecordingPlaybackNonEntitledChannelState");
            }

            if (model.EnableStartOverState == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "enableStartOverState");
            }

            if (model.EnableTrickPlayState == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "enableTrickPlayState");
            }

            if (model.BufferCatchUp == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "bufferCatchUpSetting");
            }

            if (model.BufferTrickPlay == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "bufferTrickPlaySetting");
            }

            if (string.IsNullOrEmpty(model.ExternalEpgIngestId))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "externalEpgIngestId");
            }
        }

        private static void ValidateProgramAssetForInsert(KalturaProgramAsset model)
        {
            if (!model.LinearAssetId.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "linearAssetId");
            }

            if (string.IsNullOrEmpty(model.Crid))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "crid");
            }
        }

        public static void ValidateForUpdate(this KalturaAsset model)
        {
            if (!(model is KalturaLiveAsset) && !(model is KalturaMediaAsset) && !(model is KalturaProgramAsset))
            {
                throw new ClientException((int)StatusCode.Error, "Invalid assetType");
            }

            if (model.Name != null)
            {
                if ((model.Name.Values == null || model.Name.Values.Count == 0))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
                }
                else
                {
                    model.Name.Validate("multilingualName");
                }
            }

            if (model.Description != null)
            {
                if ((model.Description.Values == null || model.Description.Values.Count == 0))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "description");
                }
                else
                {
                    model.Description.Validate("multilingualDescription", true, false);
                }
            }

            if (model.ExternalId != null && model.ExternalId == string.Empty)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "externalId");
            }

            model.ValidateMetas();
            model.ValidateTags();
            model.ValidateRelatedEntities();

            switch (model)
            {
                case KalturaLiveAsset live: ValidateLiveAssetForUpdate(live); break;
                default:
                    break;
            }
        }

        private static void ValidateLiveAssetForUpdate(KalturaLiveAsset model)
        {
            if (model.ExternalEpgIngestId != null && model.ExternalEpgIngestId == string.Empty)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "externalEpgIngestId");
            }
        }

        private static void ValidateTags(this KalturaAsset model)
        {
            if (model.Tags != null && model.Tags.Count > 0)
            {
                foreach (KeyValuePair<string, KalturaMultilingualStringValueArray> tagValues in model.Tags)
                {
                    if (tagValues.Value.Objects != null && tagValues.Value.Objects.Count > 0)
                    {
                        foreach (KalturaMultilingualStringValue item in tagValues.Value.Objects)
                        {
                            if (item.value == null)
                            {
                                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, $"KalturaMultilingualStringValue.value {tagValues.Key}");
                            }

                            List<ApiObjects.LanguageContainer> noneDefaultLanugageContainer = item.value.GetNoneDefaultLanugageContainer();
                            if (noneDefaultLanugageContainer != null && noneDefaultLanugageContainer.Count > 0)
                            {
                                throw new BadRequestException(BadRequestException.TAG_TRANSLATION_NOT_ALLOWED);
                            }
                        }
                    }

                }
            }
        }

        private static void ValidateMetas(this KalturaAsset model)
        {
            if (model.Metas != null && model.Metas.Count > 0)
            {
                foreach (KeyValuePair<string, KalturaValue> metaValues in model.Metas)
                {
                    if (metaValues.Value is KalturaMultilingualStringValue)
                    {
                        KalturaMultilingualStringValue multilingualStringValue = metaValues.Value as KalturaMultilingualStringValue;
                        if (multilingualStringValue.value == null)
                        {
                            throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaMultilingualStringValue.value");
                        }

                        multilingualStringValue.value.Validate(metaValues.Key);
                    }
                }
            }
        }

        private static void ValidateRelatedEntities(this KalturaAsset model)
        {
            if (model.RelatedEntities?.Count > 5)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_LENGTH_CROSSED, "asset.relatedEntities", 5);
            }

            if (model.RelatedEntities?.Count > 0)
            {
                foreach (KeyValuePair<string, KalturaRelatedEntityArray> relatedEntityArray in model.RelatedEntities)
                {
                    if (relatedEntityArray.Value?.Objects?.Count > 0)
                    {
                        if (relatedEntityArray.Value?.Objects?.Count > 20)
                        {
                            throw new BadRequestException(BadRequestException.ARGUMENT_MAX_LENGTH_CROSSED, "asset.relatedEntities.objects", 20);

                        }

                        foreach (KalturaRelatedEntity item in relatedEntityArray.Value.Objects)
                        {
                            if (item == null)
                            {
                                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "item");
                            }
                        }
                    }
                }
            }
        }
    }
}
