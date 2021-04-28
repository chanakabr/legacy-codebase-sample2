using System.Collections.Generic;
using ApiObjects;
using ApiObjects.Response;

namespace ApiLogic.Api.Validators
{
    public interface IRegionValidator
    {
        Status IsValidToAdd(int groupId, Region regionToAdd);
        Status IsValidToUpdate(int groupId, Region regionToUpdate);
        Status IsValidToBulkUpdate(int groupId, long linearChannelId, IReadOnlyCollection<RegionChannelNumber> regionChannelNumbers);
    }
}
