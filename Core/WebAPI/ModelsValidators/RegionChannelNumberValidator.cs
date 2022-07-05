using System.Collections.Generic;
using System.Linq;
using WebAPI.Exceptions;
using WebAPI.Models.API;

namespace WebAPI.ModelsValidators
{
    public static class RegionChannelNumberValidator
    {
        public static void Validate(bool enableMultiLcns, IReadOnlyCollection<KalturaRegionChannelNumber> regionChannelNumbers)
        {
            if (regionChannelNumbers == null || regionChannelNumbers.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, nameof(regionChannelNumbers));
            }
            
            if (regionChannelNumbers.Select(x => x.RegionId).Distinct().Count() != regionChannelNumbers.Count)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, "regionId");
            }

            foreach (var regionChannelNumber in regionChannelNumbers.OfType<KalturaRegionChannelNumberMultiLcns>().Where(x => enableMultiLcns))
            {
                regionChannelNumber.Validate();
            }
        }

        public static void Validate(this KalturaRegionChannelNumberMultiLcns model)
        {
            if (string.IsNullOrEmpty(model.LCNs))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "lcns");
            }

            if (model.ChannelNumber != default && model.LCNs.First() != model.ChannelNumber)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "channelNumber", "lcns");
            }
        }
    }
}