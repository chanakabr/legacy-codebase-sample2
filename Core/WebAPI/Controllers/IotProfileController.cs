using ApiObjects;
using ApiObjects.Response;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;

namespace WebAPI.Controllers
{
    [AddAction(ClientThrows = new eResponseStatus[] { eResponseStatus.AlreadyExist })]
    [UpdateAction(ClientThrows = new eResponseStatus[] { eResponseStatus.NoConfigurationFound })]
    [GetAction(ClientThrows = new eResponseStatus[] { eResponseStatus.NoConfigurationFound })]
    [Service("iotProfile")]
    public class IotProfileController : KalturaCrudController<KalturaIotProfile, KalturaIotProfileListResponse, IotProfile, long, KalturaIotProfileFilter>
    {
    }
}
