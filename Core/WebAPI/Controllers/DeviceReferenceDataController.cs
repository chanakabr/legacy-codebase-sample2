using ApiObjects;
using ApiObjects.Response;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Users;

namespace WebAPI.Controllers
{
    [Service("deviceReferenceData")]
    [AddAction(ClientThrows = new eResponseStatus[] { eResponseStatus.Fail, eResponseStatus.AlreadyExist })]
    [UpdateAction(ClientThrows = new eResponseStatus[] { eResponseStatus.IdentifierRequired, eResponseStatus.Fail })]
    [DeleteAction(ClientThrows = new eResponseStatus[] { eResponseStatus.IdentifierRequired })]
    [ListAction(ClientThrows = new eResponseStatus[] { eResponseStatus.Fail }, IsFilterOptional = false, IsPagerOptional = true)]
    public class DeviceReferenceDataController : KalturaCrudController<KalturaDeviceReferenceData, KalturaDeviceReferenceDataListResponse, DeviceReferenceData, long, KalturaDeviceReferenceDataFilter>
    {
    }
}
