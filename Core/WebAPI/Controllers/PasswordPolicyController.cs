using ApiObjects;
using ApiObjects.Response;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Users;

namespace WebAPI.Controllers
{
    [Service("passwordPolicy")]
    [AddAction(ClientThrows = new [] { eResponseStatus.RoleDoesNotExists })]
    [UpdateAction(ClientThrows = new [] { eResponseStatus.PasswordPolicyDoesNotExist, eResponseStatus.RoleDoesNotExists })]
    [DeleteAction(ClientThrows = new [] { eResponseStatus.PasswordPolicyDoesNotExist })]
    [ListAction(ClientThrows = new eResponseStatus[] { }, IsFilterOptional = true)]
    public class PasswordPolicyController : KalturaCrudController<KalturaPasswordPolicy, KalturaPasswordPolicyListResponse, PasswordPolicy, long, KalturaPasswordPolicyFilter>
    {
    }
}