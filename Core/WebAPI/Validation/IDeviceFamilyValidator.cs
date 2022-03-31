using WebAPI.Models.Domains;

namespace WebAPI.Validation
{
    public interface IDeviceFamilyValidator
    {
        void ValidateToAdd(long groupId, KalturaDeviceFamily deviceFamily);
        void ValidateToUpdate(long groupId, KalturaDeviceFamily deviceFamily);
    }
}