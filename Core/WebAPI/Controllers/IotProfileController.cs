using ApiObjects;
using ApiObjects.Response;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;

namespace WebAPI.Controllers
{
    [AddAction(ClientThrows = new eResponseStatus[] { eResponseStatus.Error, eResponseStatus.Fail, eResponseStatus.AlreadyExist })]
    [UpdateAction(ClientThrows = new eResponseStatus[] { eResponseStatus.Error, eResponseStatus.NoConfigurationFound })]
    [GetAction(ClientThrows = new eResponseStatus[] { eResponseStatus.Error, eResponseStatus.Fail })]
    [Service("iotProfile")]
    public class IotProfileController : KalturaCrudController<KalturaIotProfile, KalturaIotProfileListResponse, IotProfile, long, KalturaIotProfileFilter>
    {
    }
}
