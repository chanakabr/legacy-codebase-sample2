using System;
using WebAPI.Exceptions;
using static WebAPI.Exceptions.BadRequestException;

namespace WebAPI.Models.Catalog
{
    public class KalturaLabelFilterValidator
    {
        public void Validate(KalturaLabelFilter filter, string argumentName)
        {
            if (filter == null)
            {
                throw new BadRequestException(INVALID_ARGUMENT, argumentName);
            }

            var hasIdIn = !string.IsNullOrEmpty(filter.IdIn);
            var hasLabelEqual = !string.IsNullOrEmpty(filter.LabelEqual);
            var hasLabelStartsWith = !string.IsNullOrEmpty(filter.LabelStartsWith);

            if (hasIdIn && hasLabelEqual)
            {
                throw new BadRequestException(ARGUMENTS_CONFLICTS_EACH_OTHER, $"{argumentName}.idIn", $"{argumentName}.labelEqual");
            }

            if (hasIdIn && hasLabelStartsWith)
            {
                throw new BadRequestException(ARGUMENTS_CONFLICTS_EACH_OTHER, $"{argumentName}.idIn", $"{argumentName}.labelStartsWith");
            }

            if (hasLabelEqual && hasLabelStartsWith)
            {
                throw new BadRequestException(ARGUMENTS_CONFLICTS_EACH_OTHER, $"{argumentName}.labelEqual", $"{argumentName}.labelStartsWith");
            }

            if (!Enum.IsDefined(typeof(KalturaEntityAttribute), filter.EntityAttributeEqual))
            {
                throw new BadRequestException(ARGUMENT_ENUM_VALUE_NOT_SUPPORTED, $"{argumentName}.entityAttributeEqual", filter.EntityAttributeEqual);
            }
        }
    }
}