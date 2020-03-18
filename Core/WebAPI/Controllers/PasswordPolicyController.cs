using ApiObjects;
using ApiObjects.Response;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Users;

namespace WebAPI.Controllers
{
    [Service("passwordPolicy")]
    [AddAction(ClientThrows = new eResponseStatus[] { eResponseStatus.RoleDoesNotExists })]
    [UpdateAction(ClientThrows = new eResponseStatus[] { eResponseStatus.PasswordPolicyDoesNotExist, eResponseStatus.RoleDoesNotExists })]
    [DeleteAction(ClientThrows = new eResponseStatus[] { eResponseStatus.PasswordPolicyDoesNotExist })]
    [ListAction(ClientThrows = new eResponseStatus[] { eResponseStatus.PasswordPolicyDoesNotExist }, IsFilterOptional = true)]
    public class PasswordPolicyController : KalturaCrudController<KalturaPasswordPolicy, KalturaPasswordPolicyListResponse, PasswordPolicy, long, KalturaPasswordPolicyFilter>
    {
    }
}