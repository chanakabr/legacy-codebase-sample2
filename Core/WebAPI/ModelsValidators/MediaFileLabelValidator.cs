using System;
using System.Linq;
using System.Threading;
using WebAPI.Exceptions;
using WebAPI.Models.Catalog;
using static WebAPI.Exceptions.BadRequestException;

namespace WebAPI.ModelsValidators
{
    public class MediaFileLabelValidator : IMediaFileLabelValidator
    {
        private const int LABEL_MAX_LENGTH = 128;
        private const int MAX_MEDIA_FILE_LABELS_COUNT = 25;

        private static readonly Lazy<IMediaFileLabelValidator> Lazy = new Lazy<IMediaFileLabelValidator>(() => new MediaFileLabelValidator(), LazyThreadSafetyMode.PublicationOnly);

        public static IMediaFileLabelValidator Instance => Lazy.Value;

        public void ValidateToAdd(string commaSeparatedLabelValues, KalturaEntityAttribute entityAttribute, string argumentName)
        {
            if (string.IsNullOrEmpty(commaSeparatedLabelValues))
            {
                return;
            }

            var labelValues = commaSeparatedLabelValues.Split(',');
            foreach (var labelValue in labelValues)
            {
                var label = new KalturaLabel
                {
                    EntityAttribute = entityAttribute,
                    Value = labelValue
                };
                ValidateToAdd(label, argumentName);
            }

            if (labelValues.Length > MAX_MEDIA_FILE_LABELS_COUNT)
            {
                throw new BadRequestException(ARGUMENT_MAX_ITEMS_CROSSED, argumentName, MAX_MEDIA_FILE_LABELS_COUNT);
            }

            if (labelValues.Length != labelValues.Distinct().Count())
            {
                throw new BadRequestException(ARGUMENTS_VALUES_DUPLICATED, argumentName);
            }
        }

        public void ValidateToAdd(KalturaLabel label, string argumentName)
        {
            ValidateBase(label, argumentName);

            if (!Enum.IsDefined(typeof(KalturaEntityAttribute), label.EntityAttribute))
            {
                throw new BadRequestException(ARGUMENT_ENUM_VALUE_NOT_SUPPORTED, $"{argumentName}.entityAttribute", label.EntityAttribute);
            }
        }

        public void ValidateToUpdate(KalturaLabel label, string argumentName)
        {
            ValidateBase(label, argumentName);
        }

        private void ValidateBase(KalturaLabel label, string argumentName)
        {
            if (label == null)
            {
                throw new BadRequestException(INVALID_ARGUMENT, argumentName);
            }

            var labelValue = label.Value?.Trim();
            if (string.IsNullOrEmpty(labelValue))
            {
                throw new BadRequestException(ARGUMENT_CANNOT_BE_EMPTY, $"{argumentName}.value");
            }

            if (labelValue.Length > LABEL_MAX_LENGTH)
            {
                throw new BadRequestException(ARGUMENT_MAX_LENGTH_CROSSED, $"{argumentName}.value", LABEL_MAX_LENGTH);
            }

            if (label.Value.Contains(','))
            {
                throw new BadRequestException(INVALID_ARGUMENT, $"{argumentName}.value");
            }
        }
    }
}
