using WebAPI.Models.Domains;

namespace WebAPI.Validation
{
    public interface IDeviceBrandValidator
    {
        void ValidateToAdd(long groupId, KalturaDeviceBrand deviceBrand);
        void ValidateToUpdate(long groupId, KalturaDeviceBrand deviceBrand);
    }
}