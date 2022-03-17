using WebAPI.Models.Domains;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class HouseholdUserMapper
    {
        public static bool getIsMaster(this KalturaHouseholdUser model)
        {
            return model.IsMaster.HasValue ? (bool)model.IsMaster : false;
        }
    }
}