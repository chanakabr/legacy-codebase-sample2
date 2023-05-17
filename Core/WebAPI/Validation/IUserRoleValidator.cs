using WebAPI.Models.API;

namespace WebAPI.Validation
{
    public interface IUserRoleValidator
    {
        void Validate(KalturaUserRole role);
    }
}