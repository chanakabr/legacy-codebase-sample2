using ApiObjects;
using ApiObjects.Response;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Users;

namespace WebAPI.Controllers
{
    [Service("deviceInformation")]
    [AddAction(ClientThrows = new eResponseStatus[] { eResponseStatus.Fail })]
    [UpdateAction(ClientThrows = new eResponseStatus[] { eResponseStatus.IdentifierRequired, eResponseStatus.Fail })]
    [DeleteAction(ClientThrows = new eResponseStatus[] { eResponseStatus.IdentifierRequired })]
    [ListAction(ClientThrows = new eResponseStatus[] { eResponseStatus.Fail }, IsFilterOptional = true)]
    public class DeviceInformationController : KalturaCrudController<KalturaDeviceInformation, KalturaDeviceInformationListResponse, DeviceInformation, long, KalturaDeviceInformationFilter>
    {
    }
}
