using System;
using System.Linq;
using System.Threading;
using WebAPI.Exceptions;
using WebAPI.Models.Catalog;
using WebAPI.ModelsValidators;
using ApiLogicMediaFileValidator = ApiLogic.Catalog.CatalogManagement.Validators.MediaFileValidator;

namespace WebAPI.Validation
{
    public class MediaFileValidator : IMediaFileValidator
    {
        private static readonly Lazy<MediaFileValidator> Lazy = new Lazy<MediaFileValidator>(() => new MediaFileValidator(), LazyThreadSafetyMode.PublicationOnly);

        public static IMediaFileValidator Instance => Lazy.Value;

        public void ValidateToAdd(KalturaMediaFile mediaFile, string argumentName)
        {
            if (mediaFile.AssetId <= 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "assetId");
            }

            if (!mediaFile.TypeId.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "type");
            }

            if (string.IsNullOrEmpty(mediaFile.ExternalId))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "externalId");
            }

            if (mediaFile.DynamicData != null)
            {
                ValidateDynamicData(mediaFile, argumentName);
            }

            MediaFileLabelValidator.Instance.ValidateToAdd(mediaFile.Labels, KalturaEntityAttribute.MEDIA_FILE_LABELS, $"{nameof(mediaFile)}.labels");
        }

        public void ValidateToUpdate(KalturaMediaFile mediaFile, string argumentName)
        {
            if (mediaFile.DynamicData != null)
            {
                ValidateDynamicData(mediaFile, argumentName);
            }

            MediaFileLabelValidator.Instance.ValidateToAdd(mediaFile.Labels, KalturaEntityAttribute.MEDIA_FILE_LABELS, $"{nameof(mediaFile)}.labels");
        }

        private void ValidateDynamicData(KalturaMediaFile model, string argumentName)
        {
            if (model.DynamicData.Count > ApiLogicMediaFileValidator.MAX_DYNAMIC_DATA_ITEMS_COUNT)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_ITEMS_CROSSED, $"{argumentName}.dynamicData", ApiLogicMediaFileValidator.MAX_DYNAMIC_DATA_ITEMS_COUNT);
            }

            if (model.DynamicData.Keys.Any(string.IsNullOrEmpty))
            {
                throw new BadRequestException(BadRequestException.KEY_CANNOT_BE_EMPTY_OR_NULL, $"{argumentName}.dynamicData");
            }

            if (model.DynamicData.Keys.Any(x => x.Length > ApiLogicMediaFileValidator.MAX_DYNAMIC_DATA_KEY_LENGTH))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_LENGTH_CROSSED, $"{argumentName}.dynamicData", ApiLogicMediaFileValidator.MAX_DYNAMIC_DATA_KEY_LENGTH);
            }

            if (model.DynamicData.Values.Any(x => x.Objects.Count > ApiLogicMediaFileValidator.MAX_DYNAMIC_DATA_VALUES_COUNT))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_ITEMS_CROSSED, $"{argumentName}.dynamicData[].objects[]", ApiLogicMediaFileValidator.MAX_DYNAMIC_DATA_VALUES_COUNT);
            }

            if (model.DynamicData.Values.Any(x => x.Objects.Any(_ => string.IsNullOrEmpty(_.value))))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, $"{argumentName}.dynamicData[].objects[].value");
            }

            if (model.DynamicData.Values.Any(x => x.Objects.Any(_ => _.value.Length > ApiLogicMediaFileValidator.MAX_DYNAMIC_DATA_VALUE_LENGTH)))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_LENGTH_CROSSED, $"{argumentName}.dynamicData[].objects[].value", ApiLogicMediaFileValidator.MAX_DYNAMIC_DATA_VALUE_LENGTH);
            }

            if (model.DynamicData.Values.Any(x => x.Objects.Any(_ => _.value.Contains(','))))
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, $"{argumentName}.dynamicData[].objects[].value");
            }

            if (model.DynamicData.Any(x => x.Value.Objects.Select(_ => _.value).Distinct().Count() != x.Value.Objects.Count))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, $"{argumentName}.dynamicData[].objects[]");
            }
        }
    }
}