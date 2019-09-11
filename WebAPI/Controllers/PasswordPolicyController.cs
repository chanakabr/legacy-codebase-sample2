using ApiObjects;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Users;

namespace WebAPI.Controllers
{
    [Service("passwordPolicy")]
    [AddAction]
    [UpdateAction]
    [DeleteAction]
    [ListAction]
    public class PasswordPolicyController : KalturaCrudController<KalturaPasswordPolicy, KalturaPasswordPolicyListResponse, PasswordPolicy, long, KalturaPasswordPolicyFilter, PasswordPolicyFilter>
    {
    }
}