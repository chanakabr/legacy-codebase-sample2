using ApiObjects.Base;
using ApiObjects.Response;

namespace ApiObjects
{
    public class DeviceInformation : ICrudHandeledObject
    {
        public long Id { get; set; }
        public string Name { get; set; }

        //public virtual GenericResponse<DeviceInformation> Add(ContextData contextData) { return new GenericResponse<DeviceInformation> { }; }
    }

    public class DeviceModelInformation : DeviceInformation
    {
    }

    public class DeviceManufacturerInformation : DeviceInformation
    {
    }
}
