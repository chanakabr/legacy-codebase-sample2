using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WebAPI.Exceptions;
using WebAPI.Models.Catalog;

namespace WebAPI.Validation
{
    public class MediaFileTypeValidator : IMediaFileTypeValidator
    {
        private const int MAX_KEY_LENGTH = 50;
        private const int MAX_KEYS_COUNT = 15;

        private static readonly Lazy<MediaFileTypeValidator> Lazy = new Lazy<MediaFileTypeValidator>(() => new MediaFileTypeValidator(), LazyThreadSafetyMode.PublicationOnly);

        public static IMediaFileTypeValidator Instance => Lazy.Value;

        public void ValidateToAdd(KalturaMediaFileType mediaFileType, string argumentName)
        {
            if (mediaFileType.Name == null || mediaFileType.Name.Trim().Length == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, $"{argumentName}.name");
            }

            if (mediaFileType.Description == null || mediaFileType.Description.Trim().Length == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, $"{argumentName}.description");
            }

            if (mediaFileType.StreamerType == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, $"{argumentName}.streamerType");
            }

            if (!string.IsNullOrEmpty(mediaFileType.DynamicDataKeys))
            {
                ValidateDynamicDataKeys(mediaFileType.DynamicDataKeys.Split(','), argumentName);
            }
        }

        public void ValidateToUpdate(KalturaMediaFileType mediaFileType, string argumentName)
        {
            if (mediaFileType.DynamicDataKeys != null)
            {
                ValidateDynamicDataKeys(mediaFileType.DynamicDataKeys.Split(','), argumentName);
            }
        }

        private static void ValidateDynamicDataKeys(IReadOnlyCollection<string> dynamicDataKeys, string argumentName)
        {
            if (dynamicDataKeys.Count > MAX_KEYS_COUNT)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_ITEMS_CROSSED, $"{argumentName}.dynamicDataKeys[]", MAX_KEYS_COUNT);
            }

            if (dynamicDataKeys.Any(x => x.Length > MAX_KEY_LENGTH))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_LENGTH_CROSSED, $"{argumentName}.dynamicDataKeys[]", MAX_KEY_LENGTH);
            }

            if (dynamicDataKeys.Select(x => x).Distinct().Count() != dynamicDataKeys.Count)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, $"{argumentName}.dynamicDataKeys[]");
            }
        }
    }
}