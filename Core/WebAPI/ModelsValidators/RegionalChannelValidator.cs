using System.Linq;
using WebAPI.Exceptions;
using WebAPI.Models.API;
using WebAPI.ObjectsConvertor.Extensions;

namespace WebAPI.ModelsValidators
{
    public static class RegionalChannelValidator
    {
        public static void Validate(this KalturaRegionalChannelMultiLcns model, string argumentName)
        {
            if (model.ParsedLcns().Any(x => x < 0))
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, $"{argumentName}.lcns");
            }

            if (model.ChannelNumber != default && model.ParsedLcns().First() != model.ChannelNumber)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, $"{argumentName}.channelNumber", $"{argumentName}.lcns");
            }
        }
    }
}