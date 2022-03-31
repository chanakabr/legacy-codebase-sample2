using WebAPI.Models.API;

namespace WebAPI.Validation
{
    public interface IDeviceFamilyFilterValidator
    {
        void Validate(KalturaDeviceFamilyFilter filter, string argumentName);
    }
}