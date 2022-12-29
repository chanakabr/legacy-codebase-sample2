using WebAPI.Models.Domains;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class HouseholdMapper
    {
        internal static long getId(this KalturaHousehold model)
        {
            return model.Id.HasValue ? (long)model.Id : 0;
        }
    }
}
