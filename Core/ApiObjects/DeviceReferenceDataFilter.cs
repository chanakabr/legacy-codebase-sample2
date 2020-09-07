using ApiObjects.Base;
using System.Collections.Generic;

namespace ApiObjects
{
    public class DeviceReferenceDataFilter : ICrudFilter
    {
        public List<int> DeviceReferenceDataIdsIn { get; set; }
    }

    public class DeviceManufacturersReferenceDataFilter: DeviceReferenceDataFilter
    {
    }
}
