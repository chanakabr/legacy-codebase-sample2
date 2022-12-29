using System.Collections.Generic;
using WebAPI.Models.Partner;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class RollingDeviceRemovalDataMapper
    {
        internal static List<int> GetRollingDeviceRemovalFamilyIds(this KalturaRollingDeviceRemovalData model)
        {
            return Utils.Utils.ParseCommaSeparatedValues<List<int>, int>
                (model.RollingDeviceRemovalFamilyIds, "KalturaRollingDeviceRemovalData.RollingDeviceRemovalFamilyIds", false, false);
        }
    }
}
