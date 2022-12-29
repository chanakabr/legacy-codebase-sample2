using WebAPI.Models.Domains;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class HomeNetworkMapper
    {
        internal static bool getIsActive(this KalturaHomeNetwork model)
        {
            return model.IsActive.HasValue ? (bool)model.IsActive : true;
        }
    }
}
