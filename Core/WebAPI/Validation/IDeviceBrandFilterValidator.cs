using WebAPI.Models.API;

namespace WebAPI.Validation
{
    public interface IDeviceBrandFilterValidator
    {
        void Validate(KalturaDeviceBrandFilter filter, string argumentName);
    }
}