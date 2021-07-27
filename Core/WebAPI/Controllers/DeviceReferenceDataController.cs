using ApiObjects;
using ApiObjects.Response;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Users;

namespace WebAPI.Controllers
{
    [Service("deviceReferenceData")]
    [AddAction(ClientThrows = new [] { eResponseStatus.AlreadyExist })]
    [UpdateAction(ClientThrows = new eResponseStatus[] { })]
    [DeleteAction(ClientThrows = new eResponseStatus[] { })]
    [ListAction(ClientThrows = new eResponseStatus[] { }, IsFilterOptional = false, IsPagerOptional = true)]
    public class DeviceReferenceDataController : KalturaCrudController<KalturaDeviceReferenceData, KalturaDeviceReferenceDataListResponse, DeviceReferenceData, long, KalturaDeviceReferenceDataFilter>
    {
    }
}
