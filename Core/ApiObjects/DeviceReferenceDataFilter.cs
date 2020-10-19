using ApiObjects.Base;
using System.Collections.Generic;

namespace ApiObjects
{
    public class DeviceReferenceDataFilter : ICrudFilter
    {
        public List<int> IdsIn { get; set; }
    }

    public class DeviceManufacturersReferenceDataFilter: DeviceReferenceDataFilter
    {
        public string NameEqual { get; set; }
    }
}
