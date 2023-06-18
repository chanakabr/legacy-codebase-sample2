using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using WebAPI.Models.Domains;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class HouseholdDeviceMapper
    {

        public static int GetBrandId(this KalturaHouseholdDevice model)
        {
            return model.BrandId ?? 0;
        }
    }
}
