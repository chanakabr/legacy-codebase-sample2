using WebAPI.Models.LiveToVod;

namespace WebAPI.Validation
{
    public interface ILiveToVodPartnerConfigurationValidator
    {
        void Validate(KalturaLiveToVodPartnerConfiguration configuration, string argumentName);
    }
}